using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using Netick.Unity;

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
}