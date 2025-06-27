using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
  public class BombermanController : NetworkBehaviour
  {
    public List<Bomb>           SpawnedBombs = new(4);
    [HideInInspector]
    public Vector3              SpawnPos;
    [SerializeField]
    private float               _speed = 6.0f;
    [SerializeField]
    private float               _speedBoostMultiplayer = 2f;

    private GameObject          _bombPrefab;
    private CharacterController _CC;
    private BombermanInput      _lastInput;

    // Networked Properties
    [Networked]
    public int                  Score             { get; set; } = 0;
    [Networked]
    public NetworkBool          Alive             { get; set; } = true;

    [Networked(relevancy: Relevancy.InputSource)]
    public int                  MaxBombs          { get; set; } = 1;
    [Networked(relevancy: Relevancy.InputSource)]
    public float                SpeedPowerUpTimer { get; set; } = 0;
    [Networked(relevancy: Relevancy.InputSource)]
    public float                BombPowerUpTimer  { get; set; } = 0;

    public override void NetworkStart()
    {
      _bombPrefab = Sandbox.GetPrefab("Bomb");
      // we store the spawn pos so that we use it later during respawn.
      SpawnPos = transform.position;
      _CC = GetComponent<CharacterController>();
    }

    public override void OnInputSourceLeft()
    {
      Sandbox.GetComponent<BombermanEventsHandler>().KillPlayer(this);
      // destroy the player object when its input source (controller player) leaves the game.
      Sandbox.Destroy(Object);
    }

    public override void NetworkFixedUpdate()
    {
      if (!Alive)
        return;

      FetchInput(out _lastInput);

      if (IsInputSource || IsServer)
      {
        // clamp movement inputs.
        _lastInput.Movement = new Vector3(Mathf.Clamp(_lastInput.Movement.x, -1f, 1f), Mathf.Clamp(_lastInput.Movement.y, -1f, 1f));

        if (BombPowerUpTimer > 0)
          BombPowerUpTimer -= Sandbox.FixedDeltaTime;
        else
          MaxBombs = 1;

        if (SpeedPowerUpTimer > 0)
          SpeedPowerUpTimer -= Sandbox.FixedDeltaTime;

        var hasSpeedBoost = SpeedPowerUpTimer > 0;
        var speed = hasSpeedBoost ? _speed * _speedBoostMultiplayer : _speed;

        _CC.Move(_lastInput.Movement * speed * Sandbox.FixedDeltaTime);

        // we make sure the z coord of the pos of the player is always zero.
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if (IsServer && _lastInput.PlantBomb && SpawnedBombs.Count < MaxBombs)
        {
          // round the bomb pos so that it snaps to the nearest square.
          var bomb = Sandbox.NetworkInstantiate(_bombPrefab, Round(transform.position), Quaternion.identity).GetComponent<Bomb>();
          bomb.Bomber = this;
        }
      }
    }

    public void ReceivePowerUp(PowerUpType type, float boostTime)
    {
      if (type == PowerUpType.IncreaseBombs)
      {
        SpeedPowerUpTimer += boostTime;
      }
      else if (type == PowerUpType.Speed)
      {
        BombPowerUpTimer += boostTime;
        MaxBombs += 1;
      }
    }

    public void Die()
    {
      Alive = false;
      Sandbox.GetComponent<BombermanEventsHandler>().KillPlayer(this);
    }

    public void Respawn()
    {
      Sandbox.GetComponent<BombermanEventsHandler>().RespawnPlayer(this);

      Alive = true;
      SpeedPowerUpTimer = 0;
      BombPowerUpTimer = 0;
      MaxBombs = 1;

      transform.position = SpawnPos;
    }

    [OnChanged(nameof(Alive))]
    private void OnAliveChanged(OnChangedData onChangedData)
    {
      // based on state of Alive:

      // * hide/show player object.
      GetComponentInChildren<Renderer>().SetEnabled(Sandbox, Alive);

      // * enable/disable the CharacterController.
      _CC.enabled = Alive;
    }

    public Vector3 Round(Vector3 vec)
    {
      return new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), Mathf.Round(vec.z));
    }

  }
}