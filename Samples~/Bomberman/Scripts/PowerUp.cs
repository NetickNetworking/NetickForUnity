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
    public float      PowerUpTime = 35;
    private Material  _mat;

    // Networked Properties
    [Networked]
    public PowerUpType Type { get; set; }

    private void Awake()
    {
      _mat = GetComponentInChildren<Renderer>().material;
    }

    public override void NetworkRender()
    {
      var color = Type == PowerUpType.IncreaseBombs ? Color.green : Color.blue;
      _mat.color = Color.Lerp(color, color * 0.5f, Mathf.InverseLerp(-1f, 1f, Mathf.Sin(15f * Time.time)));
    }

    public void OnTriggerEnter(Collider other)
    {
      if (Sandbox == null)
        return;

      var player = other.gameObject.GetComponent<BombermanController>();

      if (Sandbox.IsServer && player != null)
      {
        player.ReceivePowerUp(Type, PowerUpTime);
        Sandbox.Destroy(Object);
      }
    }
  }
}
