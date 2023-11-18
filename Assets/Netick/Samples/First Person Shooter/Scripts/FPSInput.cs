using UnityEngine;
using Netick;

namespace Netick.Samples.FPS
{
    public struct FPSInput : INetworkInput
    {
        public Vector2 YawPitch;
        public Vector2 Movement;
        public bool    ShootInput;
    }
}