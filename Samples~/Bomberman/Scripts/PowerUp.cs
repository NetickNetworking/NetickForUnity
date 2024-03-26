using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
    public enum PowerUpType
    {
        Speed,
        IncreaseBombs
    }

    public class PowerUp : NetworkBehaviour
    {
        public float       PowerUpTime = 35;
        private Material   _mat;

        // Networked properties
        [Networked]
        public PowerUpType Type { get; set; }

        private void Awake()
        {
            _mat = GetComponentInChildren<Renderer>().material;
        }

        private void Update()
        {
            if (Type == PowerUpType.IncreaseBombs)
                _mat.color = GetColor(Color.green);
            else
                _mat.color = GetColor(Color.blue);
        }

        private Color GetColor(Color color)
        {
            return Color.Lerp(color, color * 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(15f * Time.time)));
        }

        public void OnTriggerEnter(Collider other)
        {
            if (Sandbox == null)
                return;

            var player = other.gameObject.GetComponent<BombermanController>();

            if (Sandbox.IsServer)
            {
                player.ReceivePowerUp(Type, PowerUpTime);
                Sandbox.Destroy(Object);
            }
        }
    }
}
