using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{

	public class Bomb : NetworkBehaviour
	{
		public GameObject		   ExplosionPrefab;

		public BombermanController Bomber;
		public float			   ExplosionDelay = 3.0f;

		private readonly Vector3[] _directionsAroundBomb = new Vector3[4] { Vector3.right, Vector3.left, Vector3.up, Vector3.down };

		public override void NetworkStart()
		{
			Bomber?.SpawnedBombs.Add(this);
		}

		public override void NetworkDestroy()
		{
			Bomber?.SpawnedBombs.Remove(this);

			// spawn explosion
			if (ExplosionPrefab != null)
				Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
		}

		public override void NetworkReset()
		{
			GetComponent<Renderer>().enabled = true;
		}

		public override void NetworkFixedUpdate()
		{
			if (Sandbox.TickToTime(Sandbox.Tick - Object.SpawnTick) >= ExplosionDelay)
				Explode();
		}

		private void Explode()
		{
			// hide bomb after delay
			GetComponent<Renderer>().enabled = false;

			// dealing damage is done on the server only
			if (IsServer)
				DamageTargetsAroundBomb(transform.position);

			// only the server can destroy the bomb or the client but only when the Id of the bomb is -1, meaning it was a spawn-predicted but never was confirmed by the server
			if (IsServer || Id == -1)
				Sandbox.Destroy(Object);
		}

		
		private void DamageTargetsAroundBomb(Vector3 pos)
		{
			// Find all objects around the bomb position
			// Note: Causes GC
            foreach (var dir in _directionsAroundBomb)
            {
				var hits = Physics.RaycastAll(pos, dir, 1f);

				foreach (var hit in hits)
					Damage(hit.collider.gameObject);
			}
		}

		private void Damage(GameObject target)
		{
			var obj    = target.GetComponent<NetworkObject>();
			var block  = target.GetComponent<Block>();
			var bomber = target.GetComponent<BombermanController>();

			// make sure the object is not null and in the same sandbox as the bomb
			if (obj == null || obj.Sandbox != Sandbox)
				return;

			if (block != null)
				block.Visible = false;

			if (bomber != null)
				bomber.Die();
		}
	}
}