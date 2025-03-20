using UnityEngine;
using Netick;
using Netick.Unity;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
  /// <summary>
  /// This is a helper script for quick prototyping, used to start Netick.
  /// </summary>
  [AddComponentMenu("Netick/Game Starter")]
  public class GameStarter : NetworkEventsListener
  {
    public GameObject               SandboxPrefab;
    public NetworkTransportProvider Transport;
    public StartMode                Mode                           = StartMode.MultiplePeers;
    [Range(1, 5)]
    public int                      Clients                        = 1;
    public bool                     StartServerInMultiplePeersMode = true;

    public bool                     AutoStart;
    public bool                     AutoConnect;

    [Header("Network")]
    [Range(0, 65535)]
    public int                      Port;
    public string                   ServerIPAddress                = "127.0.0.1";

    [Header("Headless Server FPS")]
    public bool                     Cap                            = true;
    public int                      FPS                            = 450;

    [Header("UI")]
    public bool                     ShowDisconnectButton           = true;
    public bool                     ShowConnectButton              = true;
    public Vector2                  Offset                         = new Vector2(36, 0);

    private void Reset()
    {
      if (Port == 0)
        Port = Random.Range(4000, 65535);
    }

    private void Awake()
    {
      if (Application.isBatchMode)
      {
        Application.targetFrameRate = FPS;
        Network.StartAsServer(Transport, Port, SandboxPrefab);
      }

      else if (AutoStart)
      {
        if (Network.Instance == null)
        {
          switch (Mode)
          {
            case StartMode.Server:
              Network.StartAsServer(Transport, Port, SandboxPrefab);
              break;
            case StartMode.Client:
              Network.StartAsClient(Transport, Port, SandboxPrefab).Connect(Port, ServerIPAddress);
              break;
            case StartMode.MultiplePeers:
              var sandboxes = Network.StartAsMultiplePeers(Transport, Port, SandboxPrefab, StartServerInMultiplePeersMode, true, Clients);

              if (AutoConnect)
              {
                for (int i = 0; i < sandboxes.Clients.Length; i++)
                  sandboxes.Clients[i].Connect(Port, ServerIPAddress);
              }


              break;
          }
        }
      }
    }

    private void OnGUI()
    {
      if (Network.IsRunning)
      {
        if (Sandbox != null && Sandbox.IsClient)
        {
          if (!Sandbox.IsVisible)
            return;

          if (Sandbox.IsConnected)
          {
            if (ShowDisconnectButton)
            {
              GUI.Label(new Rect(Offset.x, Offset.y + 170, 200, 50), $"Connected to {Sandbox.ServerEndPoint}");

              if (GUI.Button(new Rect(Offset.x, Offset.y + 220, 200, 50), "Disconnect"))
                Sandbox.DisconnectFromServer();
            }

          }
          else if (ShowConnectButton)
          {
            if (GUI.Button(new Rect(Offset.x, Offset.y + 40, 200, 50), "Connect"))
              Sandbox.Connect(Port, ServerIPAddress);

            ServerIPAddress = GUI.TextField(new Rect(Offset.x, Offset.y + 100, 200, 50), ServerIPAddress);
            Port = int.Parse(GUI.TextField(new Rect(Offset.x, Offset.y + 160, 200, 50), Port.ToString()));
          }
        }


        return;
      }

      if (GUI.Button(new Rect(Offset.x, Offset.y + 40, 200, 50), "Run Host"))
      {
        Network.StartAsHost(Transport, Port, SandboxPrefab);
      }

      if (GUI.Button(new Rect(Offset.x, Offset.y + 100, 200, 50), "Run Client"))
      {
        var sandbox = Network.StartAsClient(Transport, Port, SandboxPrefab);
        sandbox.Connect(Port, ServerIPAddress);
      }

      if (GUI.Button(new Rect(Offset.x, Offset.y + 160, 200, 50), "Run Server"))
      {
        Network.StartAsServer(Transport, Port, SandboxPrefab);
      }

      if (GUI.Button(new Rect(Offset.x, Offset.y + 220, 200, 50), "Run Host + Client"))
      {
        var sandboxes = Network.StartAsMultiplePeers(Transport, Port, SandboxPrefab, StartServerInMultiplePeersMode, true, Clients);

        if (AutoConnect)
        {
          for (int i = 0; i < Clients; i++)
            sandboxes.Clients[i].Connect(Port, ServerIPAddress);
        }
      }

      ServerIPAddress = GUI.TextField(new Rect(Offset.x, Offset.y + 280, 200, 50), ServerIPAddress);

    }
  }
}
