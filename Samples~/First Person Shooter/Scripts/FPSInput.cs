using UnityEngine;
using Netick;

namespace Netick.Samples.FPS
{
  [Networked]
  public struct FPSInput : INetworkInput
  {
    [Networked]
    public Vector2     YawPitch { get; set; }
    [Networked]
    public Vector2     Movement { get; set; }
    public NetworkBool ShootInput;
  }
}