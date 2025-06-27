using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{

  public class Bomb : NetworkBehaviour
  {
    public GameObject           ExplosionPrefab;

    public BombermanController  Bomber;
    public float                ExplosionDelay = 3.0f;

    private readonly Vector3[]  _directionsAroundBomb = new Vector3[4] { Vector3.right, Vector3.left, Vector3.up, Vector3.down };
    private static RaycastHit[] _hits = new RaycastHit[20];

    public override void NetworkStart()
    {
      Bomber?.SpawnedBombs.Add(this);
      GetComponent<Renderer>().enabled = true;
    }

    public override void NetworkDestroy()
    {
      Bomber?.SpawnedBombs.Remove(this);

      // spawn explosion.
      if (ExplosionPrefab != null)
        Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
    }

    public override void NetworkFixedUpdate()
    {
      if (Sandbox.TickToTime(Sandbox.Tick - Object.SpawnTick) >= ExplosionDelay)
        Explode();
    }

    private void Explode()
    {
      // hide bomb after delay.
      GetComponent<Renderer>().enabled = false;

      // dealing damage is done on the server only.
      if (IsServer)
      {
        DamageTargetsAroundBomb(transform.position);
        Sandbox.Destroy(Object);
      }
    }

    private void DamageTargetsAroundBomb(Vector3 pos)
    {
      // find all objects around the bomb position.
      foreach (var dir in _directionsAroundBomb)
      {
        var hitsCount = Sandbox.Physics.Raycast(pos, dir, _hits, 1f);

        for (int i = 0; i < hitsCount; i++)
        {
          var target = _hits[i].collider.gameObject;
          var block = target.GetComponent<Block>();
          var bomber = target.GetComponent<BombermanController>();

          if (block != null)
            block.Visible = false;
          bomber?.Die();
        }
      }
    }
  }
}