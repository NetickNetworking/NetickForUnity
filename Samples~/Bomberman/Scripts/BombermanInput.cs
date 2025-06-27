using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
  [Networked]
  public struct BombermanInput : INetworkInput
  {
    [Networked]
    public Vector2     Movement { get; set; }
    public NetworkBool PlantBomb;
  }
}