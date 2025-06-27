using UnityEngine;
using Netick;
using Netick.Unity;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
  /// <summary>
  /// This is a helper script for quick prototyping, used to spawn/despawn a player prefab when a player (client or host) has connected/disconnected.
  /// </summary>
  [AddComponentMenu("Netick/Player Spawner")]
  public class PlayerSpawner : NetworkEventsListener
  {
    public GameObject PlayerPrefab;
    public Transform  SpawnPosition;
    public float      HorizontalOffset                = 5f;
    public bool       StaggerSpawns                  = true;
    public bool       DestroyPlayerObjectWhenLeaving = true;

    // This is called when a player has joined the game.
    public override void OnPlayerJoined(NetworkSandbox sandbox, NetworkPlayerId player)
    {
      if (sandbox.IsClient)
        return;
      var spawnPos = SpawnPosition.position;
      if (StaggerSpawns)
        spawnPos += (HorizontalOffset * Vector3.left) * (sandbox.Players.Count - 1);
      var playerObj = sandbox.NetworkInstantiate(PlayerPrefab, spawnPos, SpawnPosition.rotation, player);
      sandbox.SetPlayerObject(player, playerObj);
    }

    // This is called when a player has left the game.
    public override void OnPlayerLeft(NetworkSandbox sandbox, NetworkPlayerId player)
    {
      if (sandbox.IsClient)
        return;
      if (!DestroyPlayerObjectWhenLeaving)
        return;
      if (sandbox.TryGetPlayerObject(player, out var playerObj))
        Sandbox.Destroy(playerObj);
    }
  }
}
