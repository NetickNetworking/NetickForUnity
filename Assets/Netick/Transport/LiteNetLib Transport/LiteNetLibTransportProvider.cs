using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System;
using Netick.Unity;

namespace Netick.Transport
{
  [CreateAssetMenu(fileName = "LiteNetLibTransportProvider", menuName = "Netick/Transport/LiteNetLibTransportProvider", order = 1)]
  public class LiteNetLibTransportProvider : NetworkTransportProvider
  {
    public override NetworkTransport MakeTransportInstance() => new LiteNetLibTransport();
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
        byte* p = (byte*)ptr.ToPointer();

        for (int i = 0; i < length; i++)
          Transport._bytes[i] = p[i];

        LNLPeer.Send(Transport._bytes, 0, length, DeliveryMethod.Unreliable);
      }
    }

    private NetManager                         _netManager;


    private BitBuffer                          _buffer;
 //   private int                                _bufferSize;

    private readonly byte[]                    _bytes = new byte[2048];
    private readonly byte[]                    _connectionBytes = new byte[200];

    private int                                _port;
    private bool                               _isServer = false;
    private Dictionary<NetPeer, LNLConnection> _clients = new Dictionary<NetPeer, LNLConnection>();
    private Queue<LNLConnection>               _freeClients = new Queue<LNLConnection>();

    // LAN Matchmaking
    private List<Session>                      _sessions = new List<Session>();
    private NetDataWriter                      _writer = new NetDataWriter();
    private string                             _machineName;

    public override void Init()
    {
      _buffer = new BitBuffer(createChunks: false);
     // _bufferSize = 875 * 4;

      _netManager = new NetManager((INetEventListener)this) { AutoRecycle = true };
      _machineName = Environment.MachineName;
      //_netManager.DisconnectTimeout = 1000;

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
        _isServer = false;
      }

      else
      {
        _netManager.BroadcastReceiveEnabled = true;
        _netManager.Start(port);
        _isServer = true;
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
        _writer.Reset();
        _writer.Put(connectionData, 0, connectionDataLen);
        _netManager.Connect(address, port, _writer);
      }


    }

    public override void Disconnect(TransportConnection connection)
    {
      _netManager.DisconnectPeer(((LNLConnection)connection).LNLPeer);
    }

    //public override void HostMatch(string name)
    //{

    //}

    //public override void UpdateMatchList()
    //{
    //  if (!_netManager.IsRunning)
    //    _netManager.Start();

    //  _sessions.Clear();
    //  _writer.Reset();
    //  _writer.Put(NetickConfig.LAN_DISCOVERY);
    //  _netManager.SendBroadcast(_writer, _port);
    //}

    /// ////////////////////////////////////////////

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
    {
      if (_clients.Count >= Engine.Config.MaxPlayers)
      {
        request.Reject();
        return;
      }

      int len = request.Data.AvailableBytes;
      request.Data.GetBytes(_connectionBytes, 0, len);
      bool accepted = NetworkPeer.OnConnectRequest(_connectionBytes, len, request.RemoteEndPoint.ToNetickEndPoint());

      if (accepted)
        request.Accept();
      else
        request.Reject();
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
      var connection = _freeClients.Dequeue();
      connection.LNLPeer = peer;

      _clients.Add(peer, connection);
      NetworkPeer.OnConnected(connection);
    }

    void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
      if (!_isServer)
      {
        if (disconnectInfo.Reason == DisconnectReason.ConnectionRejected || disconnectInfo.Reason == DisconnectReason.ConnectionFailed)
        {
          NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused);
          return;
        }

        if (peer == null)
        {
          Debug.Log($"ERROR: {disconnectInfo.Reason}");
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

        NetworkPeer.OnDisconnected(_clients[peer], reason);
        _freeClients.Enqueue(_clients[peer]);
        _clients.Remove(peer);
      }
    }

     unsafe void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
      if (_clients.TryGetValue(peer, out var c))
      {
        var len = reader.AvailableBytes;
        reader.GetBytes(_bytes, 0, reader.AvailableBytes);
     
        fixed(byte* ptr = _bytes)
        {
          _buffer.SetFrom(ptr, len, _bytes.Length);
          NetworkPeer.Receive(c, _buffer);
        }
      }
    }


    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
      //ulong msgType = reader.GetULong();

      //if (msgType == NetickConfig.LAN_DISCOVERY_RESPONSE)
      //{
      //  string name = reader.GetString();
      //  int port = reader.GetInt();

      //  var newSession = new Session()
      //  {
      //    Name = name,
      //    IP = remoteEndPoint.Address.ToString(),
      //    Port = port
      //  };

      //  if (!_sessions.Contains(newSession))
      //    _sessions.Add(newSession);

      //  OnMatchListUpdate(_sessions);
      //}

      //else if (_isServer && msgType == NetickConfig.LAN_DISCOVERY)
      //{
      //  _writer.Reset();
      //  _writer.Put(NetickConfig.LAN_DISCOVERY_RESPONSE);
      //  _writer.Put(_machineName);
      //  _writer.Put(_port);

      //  _netManager.SendUnconnectedMessage(_writer, remoteEndPoint);
      //}
    }


    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
      Debug.Log("[S] NetworkError: " + socketError);
      NetworkPeer.OnConnectFailed(ConnectionFailedReason.Refused);
    }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }



  }


}

