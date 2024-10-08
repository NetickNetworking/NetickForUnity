using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using Netick.Unity;
using UnityEngine.UIElements;

namespace Netick.Transport
{
  [CreateAssetMenu(fileName = "LiteNetLibTransportProvider", menuName = "Netick/Transport/LiteNetLibTransportProvider", order = 1)]
  public class LiteNetLibTransportProvider : NetworkTransportProvider
  {
    [Tooltip("Time duration (in seconds) until a connection is dropped when no packets were received.")]
    public float DisconnectTimeout      = 5;
    [Tooltip("Time interval (in seconds) between connection attempts.")]
    public float ReconnectInterval      = 0.5f;
    [Tooltip("Max number of connect attempts.")]
    public int   MaxConnectAttempts     = 10;
    [Tooltip("LiteNetLib internal logic update interval (in seconds).")]
    public float UpdateInterval         = 0.015f;

    public override NetworkTransport    MakeTransportInstance() => new LiteNetLibTransport(this);
  }

  public class LiteNetLibTransport : NetworkTransport, INetEventListener
  {
    public class LNLConnection : TransportConnection
    {
      public LiteNetLibTransport        Transport;
      public NetPeer                    LNLPeer;
      public override IEndPoint         EndPoint => LNLPeer.EndPoint.ToNetickEndPoint();

      public override int               Mtu =>      LNLPeer.Mtu;

      public LNLConnection(LiteNetLibTransport transport)
      {
        Transport = transport;
      }

      public unsafe override void Send(IntPtr ptr, int length)
      {
        SendLNL((byte*)ptr.ToPointer(), length, DeliveryMethod.Unreliable);
      }

      public unsafe override void SendUserData(IntPtr ptr, int length, TransportDeliveryMethod transportDeliveryMethod)
      {
        SendLNL((byte*)ptr.ToPointer(), length, transportDeliveryMethod == TransportDeliveryMethod.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
      }

      private unsafe void SendLNL(byte* ptr, int length, DeliveryMethod deliveryMethod)
      {
        if (Transport._bytes.Length < length)
          Transport._bytes    = new byte[length];

        for (int i = 0; i < length; i++)
          Transport._bytes[i] = ptr[i];
        LNLPeer.Send(Transport._bytes, 0, length, deliveryMethod);
      }
    }

    private LiteNetLibTransportProvider        _provider;
    private NetManager                         _netManager;
    private BitBuffer                          _buffer;

    private byte[]                             _bytes           = new byte[2048];
    private readonly byte[]                    _connectionBytes = new byte[200];

    private int                                _port;
    private Dictionary<NetPeer, LNLConnection> _clients         = new();
    private Queue<LNLConnection>               _freeClients     = new();

    // LAN Discovery
    private List<Session>                      _sessions        = new();
    private NetDataWriter                      _writer          = new NetDataWriter();
    private string                             _machineName;

    public LiteNetLibTransport(LiteNetLibTransportProvider provider)
    {
      this._provider                 = provider;
    }

    public override void Init()
    {
      _buffer                        = new BitBuffer(createChunks: false);
      _netManager                    = new NetManager((INetEventListener)this) { AutoRecycle = true };
      _machineName                   = Environment.MachineName;
      _netManager.DisconnectTimeout  = (int)(_provider.DisconnectTimeout * 1000);
      _netManager.ReconnectDelay     = (int)(_provider.ReconnectInterval * 1000);
      _netManager.MaxConnectAttempts = _provider.MaxConnectAttempts;
      _netManager.UpdateTime         = (int)(_provider.UpdateInterval * 1000);
     
      for (int i = 0; i < Engine.Config.MaxPlayers; i++)
        _freeClients.Enqueue(new LNLConnection(this));
    }

    public override void PollEvents()
    {
      _netManager.PollEvents();
    }

    public override void ForceUpdate()
    {
      _netManager.TriggerUpdate();
    }

    public override void Run(RunMode mode, int port)
    {
      if (mode == RunMode.Client)
      {
        _netManager.UnconnectedMessagesEnabled = true;
        _netManager.Start();
      }

      else
      {
        _netManager.BroadcastReceiveEnabled = true;
        _netManager.Start(port);
      }

      _port = port;
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
      {
        _netManager.Connect(address, port, "");
      }
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

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
    {
      if (_clients.Count >= Engine.Config.MaxPlayers)
      {
        request.Reject();
        return;
      }

      int len       = request.Data.AvailableBytes;
      request.Data.GetBytes(_connectionBytes, 0, len);
      bool accepted = NetworkPeer.OnConnectRequest(_connectionBytes, len, request.RemoteEndPoint.ToNetickEndPoint());

      if (accepted)
        request.Accept();
      else
        request.Reject();
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
      var connection     = _freeClients.Dequeue();
      connection.LNLPeer = peer;

      _clients.   Add(peer, connection);
      NetworkPeer.OnConnected(connection);
    }

    void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
      if (!Engine.IsServer)
      {
        if (disconnectInfo.Reason == DisconnectReason.ConnectionRejected)
        {
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused);
          return;
        }

        if (disconnectInfo.Reason == DisconnectReason.ConnectionFailed || disconnectInfo.Reason == DisconnectReason.Timeout)
        {
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Timeout);
          return;
        }

        if (peer == null)
        {
          Debug.Log($"LiteNetLib Network Error: {disconnectInfo.Reason}");
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused);
          return;
        }

      }

      if (peer == null)
      {
        return;
      }

      if (_clients.ContainsKey(peer))
      {
        TransportDisconnectReason reason = disconnectInfo.Reason == DisconnectReason.Timeout ? TransportDisconnectReason.Timeout : TransportDisconnectReason.Shutdown;

        NetworkPeer. OnDisconnected(_clients[peer], reason);
        _freeClients.Enqueue(_clients[peer]);
        _clients.    Remove(peer);
      }
    }

     unsafe void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
      if (_clients.TryGetValue(peer, out var c))
      {
        var len = reader.AvailableBytes;

        if (_bytes.Length < reader.AvailableBytes)
          _bytes      = new byte[reader.AvailableBytes];

        reader.       GetBytes(_bytes, 0, reader.AvailableBytes);
     
        fixed(byte* ptr = _bytes)
        {
          _buffer.    SetFrom(ptr, len, _bytes.Length);
          NetworkPeer.Receive(c, _buffer);
        }
      }
    }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
      Debug.Log("LiteNetLib Network Error: " + socketError);
      NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused);
    }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
  }


}

