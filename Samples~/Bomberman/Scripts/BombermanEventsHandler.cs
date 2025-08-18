using System.Collections.Generic;
using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
  public class BombermanEventsHandler : NetworkBehaviour
  {
    public List<BombermanController> AlivePlayers    = new(4);
    private GameObject               _playerPrefab;
    private Vector3[]                _spawnPositions = new Vector3[4] { new Vector3(11, 9, 0), new Vector3(11, 1, 0), new Vector3(1, 9, 0), new Vector3(1, 1, 0) };
    private Queue<Vector3>           _freePositions  = new(4);

    public override void NetworkAwake()
    {
      Sandbox.Events.OnInput          += OnInput;
      Sandbox.Events.OnConnectRequest += OnConnectRequest;
      Sandbox.Events.OnPlayerJoined   += OnPlayerJoined;
      Sandbox.Events.OnPlayerLeft     += OnPlayerLeft;
      _playerPrefab                    = Sandbox.GetPrefab("Bomberman Player");
      Sandbox.InitializePool(Sandbox.GetPrefab("Bomb"), 5);
      Sandbox.InitializePool(_playerPrefab, 4);

      for (int i = 0; i < 4; i++)
        _freePositions.Enqueue(_spawnPositions[i]);
    }

    public override void NetworkStart()
    {
      if (IsServer)
        RestartGame();
    }

    public void OnConnectRequest(NetworkSandbox sandbox, NetworkConnectionRequest request)
    {
      if (Sandbox.Players.Count >= 4)
        request.Refuse();
    }

    // This is called when a player has has joined the game.
    public void OnPlayerJoined(NetworkSandbox sandbox, NetworkPlayerId player)
    {
      if (IsClient)
        return;
      var playerObj = sandbox.NetworkInstantiate(_playerPrefab, _spawnPositions[Sandbox.Players.Count], Quaternion.identity, player).GetComponent<BombermanController>();
      sandbox.SetPlayerObject(player, playerObj.Object);
      AlivePlayers.Add(playerObj);
    }

    // This is called when a player has has left the game.
    public void OnPlayerLeft(NetworkSandbox sandbox, NetworkPlayerId player)
    {
      if (IsClient)
        return;
      var playerObj = sandbox.GetPlayerObject<BombermanController>(player);
      _freePositions.Enqueue(playerObj.SpawnPos);
    }

    // This is called to read inputs.
    public void OnInput(NetworkSandbox sandbox)
    {
      var input = sandbox.GetInput<BombermanInput>();
      input.Movement = GetMovementDir();
      input.PlantBomb |= Input.GetKeyDown(KeyCode.Space);
      sandbox.SetInput(input);
    }

    public void RestartGame()
    {
      // destroy level.
      foreach (var block in Sandbox.FindObjectsOfType<Block>())
        Sandbox.Destroy(block.Object);
      foreach (var bomb in Sandbox.FindObjectsOfType<Bomb>())
        Sandbox.Destroy(bomb.Object);

      // create new level.
      var blockPrefab = Sandbox.GetPrefab("DestroyableBlock");
      var powerUpPrefab = Sandbox.GetPrefab("Power Up");
      var numberOfBoosters = Random.Range(2, 4 + 1);
      var takenPositions = new List<Vector3>();
      var maxX = 11;
      var maxY = 9;

      for (int x = 1; x <= maxX; x++)
      {
        for (int y = 1; y <= maxY; y++)
        {
          var spawn = Random.value > 0.5f;
          var pos = new Vector3(x, y);

          if (spawn && IsValidPos(pos))
          {
            Sandbox.NetworkInstantiate(blockPrefab, pos, Quaternion.identity);
            takenPositions.Add(pos);
          }
        }
      }

      while (numberOfBoosters > 0)
      {
        var randomPos = new Vector3(Random.Range(1, 11 + 1), Random.Range(1, 9 + 1), 0);
        var type = (Random.value > 0.5f) ? PowerUpType.Speed : PowerUpType.IncreaseBombs;

        if (!takenPositions.Contains(randomPos) && IsValidPos(randomPos))
        {
          var booster = Sandbox.NetworkInstantiate(powerUpPrefab, randomPos, Quaternion.identity).GetComponent<PowerUp>();
          booster.Type = type;
          numberOfBoosters--;
        }
      }

      // reset players.
      foreach (var player in Sandbox.Players)
        Sandbox.GetPlayerObject<BombermanController>(player).Respawn();
    }

    private bool IsValidPos(Vector3 pos)
    {
      // if the pos is the position of a static block, we ignore it.
      if ((pos.x >= 2 && pos.x <= 10) && (pos.y >= 2 && pos.y <= 8))
        if (pos.x % 2 == 0 && pos.y % 2 == 0)
          return false;

      // if the pos is near the position of the spawn locations of the players, we ignore it.
      foreach (var loc in _spawnPositions)
      {
        if (pos == loc) return false;
        if (pos == loc + Vector3.up || pos == loc + Vector3.down) return false;
        if (pos == loc + Vector3.left || pos == loc + Vector3.right) return false;
      }

      return true;
    }

    public void KillPlayer(BombermanController bomber)
    {
      AlivePlayers.Remove(bomber);

      if (AlivePlayers.Count == 1)
      {
        AlivePlayers[0].Score++;
        RestartGame();
      }

      else if (AlivePlayers.Count < 1)
        RestartGame();
    }

    public void RespawnPlayer(BombermanController bomber)
    {
      if (!AlivePlayers.Contains(bomber))
        AlivePlayers.Add(bomber);
    }
    private Vector2 GetMovementDir()
    {
      if (Input.GetKey(KeyCode.W)) return Vector2.up;
      else if (Input.GetKey(KeyCode.D)) return Vector2.right;
      else if (Input.GetKey(KeyCode.S)) return Vector2.down;
      else if (Input.GetKey(KeyCode.A)) return Vector2.left;
      else return Vector2.zero;
    }
  }
}