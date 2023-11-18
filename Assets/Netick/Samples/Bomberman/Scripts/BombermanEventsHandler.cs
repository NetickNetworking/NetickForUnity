using System.Collections.Generic;
using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
    public class BombermanEventsHandler : NetworkEventsListener
    {
       
        public List<BombermanController>  AlivePlayers   = new List<BombermanController>();
        public Vector3[]                  SpawnPositions = new Vector3[4]  
        { 
            new Vector3(11, 9, 0), 
            new Vector3(11, 1, 0), 
            new Vector3(1,  9, 0), 
            new Vector3(1,  1, 0)
        };

        private GameObject               _playerPrefab;
        private Queue<Vector3>           _freePositions = new Queue<Vector3>(4);

        // ******************** Netick Callbacks ********************

        // This is called on the server and the clients when Netick has started.
        public override void OnStartup(NetworkSandbox sandbox)
        {
            _playerPrefab = sandbox.GetPrefab("Bomberman Player");
            sandbox.InitializePool(sandbox.GetPrefab("Bomb"), 5);
        }

        // This is called to read inputs.
        public override void OnInput(NetworkSandbox sandbox)
        {
            var input = sandbox.GetInput<BombermanInput>();

            input.Movement   = GetMovementDir();
            input.PlantBomb |= Input.GetKeyDown(KeyCode.Space);
            sandbox.SetInput(input);
        }

        // This is called on the server when a client has connected.
        public override void OnClientConnected(NetworkSandbox sandbox, NetworkConnection client)
        {
            var player = sandbox.NetworkInstantiate(_playerPrefab, SpawnPositions[Sandbox.ConnectedClients.Count], Quaternion.identity, client).GetComponent<BombermanController>();

            client.PlayerObject = player.gameObject;
            player.PlayerNumber = Sandbox.ConnectedClients.Count;

            AlivePlayers.Add(player);
        }

        // This is called on the server when a client has disconnected.
        public override void OnClientDisconnected(NetworkSandbox sandbox, NetworkConnection client, TransportDisconnectReason reason)
        {
            _freePositions.Enqueue(((GameObject)client.PlayerObject).GetComponent<BombermanController>().SpawnPos);
        }

        public override void OnConnectRequest(NetworkSandbox sandbox, NetworkConnectionRequest request)
        {
            if (_freePositions.Count < 1)
                request.Refuse();
        }

        // This is called on the server and the clients when the scene has been loaded.
        public override void OnSceneLoaded(NetworkSandbox sandbox)
        {
            if (sandbox.IsClient)
                return;

            _freePositions.Clear();

            for (int i = 0; i < 4; i++)
                _freePositions.Enqueue(SpawnPositions[i]);

            for (int i = 0; i < sandbox.ConnectedPlayers.Count; i++)
            {
                var player = sandbox.NetworkInstantiate(_playerPrefab, SpawnPositions[i], Quaternion.identity, sandbox.ConnectedPlayers[i]);
                sandbox.ConnectedPlayers[i].PlayerObject = player.gameObject;
            }

            RestartGame();
        }

        // *******************  ********************

        public void RestartGame()
        {
            DestroyLevel();
            CreateNewLevel();

            foreach (var player in Sandbox.ConnectedPlayers)
                ((GameObject)player.PlayerObject).GetComponent<BombermanController>().Respawn();
        }

        private void DestroyLevel()
        {
            var blocks = Sandbox.FindObjectsOfType<Block>();
            var bombs  = Sandbox.FindObjectsOfType<Bomb>();

            foreach (var block in blocks)
                Sandbox.Destroy(block.Object);
            foreach (var bomb in bombs)
                Sandbox.Destroy(bomb.Object);
        }


        private void CreateNewLevel()
        {
            var blockPrefab      = Sandbox.GetPrefab("DestroyableBlock");
            var powerUpPrefab    = Sandbox.GetPrefab("Power Up");
            var numberOfBoosters = Random.Range(2, 4+1);
            var takenPositions   = new List<Vector3>();
            var maxX             = 11;
            var maxY             = 9;

            for (int x = 1; x <= maxX; x++)
            {
                for (int y = 1; y <= maxY; y++)
                {
                    var spawn = Random.value > 0.5f;
                    var pos   = new Vector3(x, y);
        
                    if (spawn && IsValidPos(pos))
                    {
                        Sandbox.NetworkInstantiate(blockPrefab, pos, Quaternion.identity);
                        takenPositions.Add(pos);
                    }
                }
            }

            while (numberOfBoosters > 0)
            {
                var randomPos = new Vector3(Random.Range(1, 11+1), Random.Range(1, 9+1), 0);
                var type      = (Random.value > 0.5f) ? PowerUpType.Speed : PowerUpType.IncreaseBombs;

                if (!takenPositions.Contains(randomPos) && IsValidPos(randomPos))
                {
                    var booster  = Sandbox.NetworkInstantiate(powerUpPrefab, randomPos, Quaternion.identity).GetComponent<PowerUp>();
                    booster.Type = type;
                    numberOfBoosters--;
                }
            }
        }

        private bool IsValidPos(Vector3 pos)
        {
            // if the pos is the position of a static block, we ignore it
            if ((pos.x >= 2 && pos.x <= 10) && (pos.y >= 2 && pos.y <= 8))
                if (pos.x % 2 == 0 && pos.y % 2 == 0)
                    return false;

            // if the pos is near the position of the spawn locations of the players, we ignore it
            foreach (var loc in SpawnPositions)
            {
                if (pos == loc)
                    return false;
                if (pos == loc + Vector3.up   || pos == loc + Vector3.down)
                    return false;
                if (pos == loc + Vector3.left || pos == loc + Vector3.right)
                    return false;
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
            if (Input.GetKey(KeyCode.W))
                return Vector2.up;
            else if (Input.GetKey(KeyCode.D))
                return Vector2.right;
            else if (Input.GetKey(KeyCode.S))
                return Vector2.down;
            else if (Input.GetKey(KeyCode.A))
                return Vector2.left;
            else
                return Vector2.zero;
        }
    }
}