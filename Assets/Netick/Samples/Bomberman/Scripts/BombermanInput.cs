using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
    public struct BombermanInput : INetworkInput
    {
        public Vector2 Movement;
        public bool    PlantBomb;
    }
}