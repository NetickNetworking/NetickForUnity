using LiteNetLib;
using LiteNetLib.Utils;
using Netick.Unity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netick.Transport
{
  public class LiteNetLibTransport : NetworkTransport, INetEventListener
  {
    private class LNLRequest : IConnectionRequest
    {
      public LiteNetLibTransport         Transport;
      public ConnectionRequest           Request;
      public IEndPoint                   Source             => Request.RemoteEndPoint.ToNetickEndPoint();
    
      public LNLRequest(LiteNetLibTransport transport)
      {
        Transport = transport;
      }

      public void Accept()
      {
        if (Request == null)
          return;

        Request?.Accept();
        Transport._freeRequests.Enqueue(this);
        Request = null;
      }

      public void Refuse(ReadOnlySpan<byte> refusalData)
      {
        if (Request == null)
          return;

        if (Transport._bytesBuffer.Length < refusalData.Length)
          Transport._bytesBuffer = new byte[refusalData.Length];
        if (refusalData.Length > 0)
          refusalData.CopyTo(Transport._bytesBuffer);

        Request?.Reject(Transport._bytesBuffer, 0, refusalData.Length);
        Transport._freeRequests.Enqueue(this);
        Request = null;
      }
    }

    public class LNLConnection : TransportConnection
    {
      public LiteNetLibTransport               Transport;
      public NetPeer                           LNLPeer;
      public override IEndPoint                EndPoint => LNLPeer.EndPoint.ToNetickEndPoint();
      public override int                      Mtu      => LNLPeer.Mtu;

      public LNLConnection(LiteNetLibTransport transport)
      {
        Transport = transport;
      }

      public unsafe override void Send(IntPtr ptr, int length)                                         => LNLPeer.Send(new ReadOnlySpan<byte>(ptr.ToPointer(), length), DeliveryMethod.Unreliable);
      public unsafe override void SendUserData(IntPtr ptr, int length, TransportDeliveryMethod method) => LNLPeer.Send(new ReadOnlySpan<byte>(ptr.ToPointer(), length), method == TransportDeliveryMethod.Unreliable ? DeliveryMethod.Unreliable : DeliveryMethod.ReliableOrdered);
    }

    private LiteNetLibTransportProvider        _provider;
    private NetManager                         _netManager;
    private NetDataWriter                      _writer      = new NetDataWriter();
    private BitBuffer                          _bitBuffer;
    private byte[]                             _bytesBuffer = new byte[1024];
    private int                                _port;
    private Dictionary<NetPeer, LNLConnection> _connections;
    private Queue<LNLConnection>               _freeConnections;
    private Queue<LNLRequest>                  _freeRequests;
    private byte[]                             _serverIsFullMsg;
    private NetworkSandbox                     _sandbox;

    public LiteNetLibTransport(LiteNetLibTransportProvider provider)
    {
      this._provider                 = provider;
    }

    public override void Init()
    {
      _sandbox                       = (NetworkSandbox)Engine.UserObject;
      _connections                   = new(Engine.Config.MaxPlayers);
      _freeConnections               = new(Engine.Config.MaxPlayers);
      _freeRequests                  = new(Engine.Config.MaxPlayers);
      _bitBuffer                     = new BitBuffer(createChunks: false);
      _netManager                    = new NetManager(this) { AutoRecycle = true };
      _netManager.DisconnectTimeout  = (int)(_provider.DisconnectTimeout * 1000);
      _netManager.ReconnectDelay     = (int)(_provider.ReconnectInterval * 1000);
      _netManager.MaxConnectAttempts = _provider.MaxConnectAttempts;
      _netManager.UpdateTime         = (int)(_provider.UpdateInterval * 1000);
      _serverIsFullMsg               = Encoding.UTF8.GetBytes("ServerFull");
      int connCount                  = Engine.IsClient ? 1 : Engine.MaxClients;

      for (int i = 0; i < connCount; i++)
        _freeConnections.Enqueue(new LNLConnection(this));
      for (int i = 0; i < connCount; i++)
        _freeRequests.Enqueue(new LNLRequest(this));
    }

    public override void PollEvents()  => _netManager.PollEvents();
    public override void ForceUpdate() => _netManager.TriggerUpdate();

    public override void Run(RunMode mode, int port)
    {
      _port = port;

      if (mode == RunMode.Client)
      {
        _netManager.UnconnectedMessagesEnabled = true;
        _netManager.Start();
      }
      else
      {
        _netManager.BroadcastReceiveEnabled    = true;
        _netManager.Start(port);
      }
    }

    public override void Shutdown()
    {
      _netManager.Stop();
    }

    public override void Connect(string address, int port, byte[] connectionData, int connectionDataLen)
    {
      if (!_netManager.IsRunning)
        _netManager.Start();

      if (connectionData == null)
        _netManager.Connect(address, port, "");
      else
      {
        _writer.    Reset();
        _writer.    Put(connectionData, 0, connectionDataLen);
        _netManager.Connect(address, port, _writer);
      }
    }

    public override void Disconnect(TransportConnection connection)
    {
      _netManager.DisconnectPeer(((LNLConnection)connection).LNLPeer);
    }

    public override void DisconnectWithData(TransportConnection connection, ReadOnlySpan<byte> data)
    {
      if (_bytesBuffer.Length < data.Length)
        _bytesBuffer = new byte[data.Length];

      data.CopyTo(_bytesBuffer);
      _netManager.DisconnectPeer(((LNLConnection)connection).LNLPeer, _bytesBuffer, 0, data.Length);
    }

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
    {
      if (_freeConnections.Count == 0)
      {
        request.Reject(_serverIsFullMsg);
        return;
      }

      var r       = _freeRequests.Count > 0 ? _freeRequests.Dequeue() : new LNLRequest(this); 
      r.Request   = request;

      if (request.Data.AvailableBytes > 0)
      {
        Span<byte> kickData = stackalloc byte[request.Data.AvailableBytes];
        request.Data.GetRemainingBytesSegment().AsSpan().CopyTo(kickData);
        NetworkPeer.OnConnectRequest(r, kickData);
      }
      else
      {
        NetworkPeer.OnConnectRequest(r);
      }
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
      if (_freeConnections.Count == 0)
      {
        peer.Disconnect(_serverIsFullMsg);
        return;
      }
      var connection     = _freeConnections.Dequeue();
      connection.LNLPeer = peer;
      _connections.      Add(peer, connection);
      NetworkPeer.       OnConnected(connection);
    }

    unsafe void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
      var dataLength      = disconnectInfo.AdditionalData != null ? disconnectInfo.AdditionalData.AvailableBytes : 0;
      Span<byte> kickData = stackalloc byte[dataLength];
      if (dataLength > 0)
        disconnectInfo.AdditionalData.GetRemainingBytesSegment().AsSpan().CopyTo(kickData);

      if (peer != null && _connections.ContainsKey(peer))
      {
        TransportDisconnectReason reason = disconnectInfo.Reason == DisconnectReason.Timeout ? TransportDisconnectReason.Timeout : TransportDisconnectReason.Shutdown;
        NetworkPeer.OnDisconnected(_connections[peer], reason, kickData);
        _freeConnections.Enqueue(_connections[peer]);
        _connections.Remove(peer);
        return;
      }

      if (Engine.IsClient)
      {
        var serverFull   = dataLength > 0 && Encoding.UTF8.GetString(kickData) == "ServerFull";

        if (serverFull)
        {
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.ServerFull);
          return;
        }

        if (disconnectInfo.Reason == DisconnectReason.ConnectionRejected)
        {
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused, kickData);
          return;
        }

        if (disconnectInfo.Reason == DisconnectReason.ConnectionFailed || disconnectInfo.Reason == DisconnectReason.Timeout)
        {
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Timeout);
          return;
        }

        if (peer == null)
        {
          UnityEngine.Debug.Log($"LiteNetLib Network Error: {disconnectInfo.Reason}");
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused, kickData);
          return;
        }
      }
    }

    unsafe void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
      if (!_connections.TryGetValue(peer, out var c))
        return;

      fixed (byte* ptr = reader.RawData)
      {
        _bitBuffer. SetFrom(ptr + reader.Position, reader.AvailableBytes, reader.RawData.Length);
        NetworkPeer.Receive(c, _bitBuffer);
      }
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
      UnityEngine.Debug.Log("LiteNetLib Network Error: " + socketError);
      NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused);
    }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
  }
}