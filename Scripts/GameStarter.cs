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
  
    public Vector2                  ButtonSize                     = new Vector2(200, 20);
    public Vector2                  LabelSize                      = new Vector2(200, 200);
    public Vector2                  StartOffset                    = new Vector2(27, 20);
    public Vector2                  Spacing                        = new Vector2(0, 25);
    private Vector2                 _curOffset                     = default;
    private Color                   _buttonForegroundColor         = Color.white;
    private Color                   _buttonBackgroundColor         = new(0.2f, 0.2f, 0.2f, 1f);
    private Color                   _buttonHoverColor              = new(0.1f, 0.1f, 0.1f, 1f);
    private Texture2D               _buttonNormalTexture;
    private Texture2D               _buttonHoverTexture;
    private GUIStyle                _buttonStyle;

    private void Reset()
    {
      if (Port == 0)
        Port = Random.Range(4000, 65535);
    }

    private void Awake()
    {
      _buttonStyle = null;

      if (Application.isBatchMode)
      {
        if (Cap)
          Application.targetFrameRate = FPS;
        if (!Network.IsRunning)
          Network.StartAsServer(Transport, Port, SandboxPrefab);
        return;
      }

      if (AutoStart)
      {
        if (Network.Instance == null)
        {
          switch (Mode)
          {
            case StartMode.Server:
              Network.StartAsServer(Transport, Port, SandboxPrefab);
              break;
            case StartMode.Client:
              Network.StartAsClient(Transport, SandboxPrefab).Connect(Port, ServerIPAddress);
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
      if (_buttonStyle == null)
        InitButtonStyle();

      _curOffset = StartOffset;
      if (Network.IsRunning)
      {
        if (Sandbox != null && Sandbox.IsClient)
        {
          if (!Sandbox.IsVisible)
            return;

          _curOffset.x = (Screen.width - ButtonSize.x) - StartOffset.x;

          if (!Sandbox.IsReplay)
          {
            if (Sandbox.IsConnected)
            {
              if (ShowDisconnectButton)
              {
                Label($"Connected to {Sandbox.ServerEndPoint}");

                if (Button("Disconnect"))
                  Sandbox.DisconnectFromServer();
              }

            }
            else if (ShowConnectButton)
            {
              if (Button("Connect"))
                Sandbox.Connect(Port, ServerIPAddress);

              ServerIPAddress = TextField(ServerIPAddress);
              Port = int.Parse(TextField(Port.ToString()));
            }
          }
          else
          {
            if (Sandbox.Replay.Playback.IsInitialized)
              Label($"Connected using replay: {Sandbox.Replay.Playback.ReplayAddress}");
          }


        }

        return;
      }

      if (Button("Run Host"))
      {
        Network.StartAsHost(Transport, Port, SandboxPrefab);
      }

      if (Button("Run Client"))
      {
        var sandbox = Network.StartAsClient(Transport, SandboxPrefab);
        sandbox.Connect(Port, ServerIPAddress);
      }

      if (Button("Run Server"))
      {
        Network.StartAsServer(Transport, Port, SandboxPrefab);
      }

      if (Button("Run Replay Client"))
      {
        var sandbox = Network.StartAsReplayClient(SandboxPrefab);
        sandbox.StartReplayPlayback();
      }

      if (Button("Run Single Player"))
      {
        Network.StartAsSinglePlayer(SandboxPrefab);
      }

      if (Button("Run Host + Client"))
      {
        var sandboxes = Network.StartAsMultiplePeers(Transport, Port, SandboxPrefab, StartServerInMultiplePeersMode, true, Clients);

        if (AutoConnect)
        {
          for (int i = 0; i < Clients; i++)
            sandboxes.Clients[i].Connect(Port, ServerIPAddress);
        }
      }

      ServerIPAddress = TextField(ServerIPAddress);
    }

    private bool Button(string title)
    {
      bool result = GUI.Button(new Rect(_curOffset.x, _curOffset.y, ButtonSize.x, ButtonSize.y), title, _buttonStyle);
      _curOffset += Spacing;
      return result;
    }

    private string TextField(string title)
    {
      var result = GUI.TextField(new Rect(_curOffset.x,  _curOffset.y, ButtonSize.x, ButtonSize.y), title);
      _curOffset += Spacing;
      return result;
    }

    private void Label(string title)
    {
       GUI.Label(new Rect(_curOffset.x, _curOffset.y, LabelSize.x, LabelSize.y), title);
      _curOffset += Spacing;
    }

    void InitButtonStyle()
    {
      _buttonNormalTexture = new Texture2D(1, 1);
      _buttonNormalTexture.SetPixel(0, 0, _buttonBackgroundColor);
      _buttonNormalTexture.Apply();

      _buttonHoverTexture = new Texture2D(1, 1);
      _buttonHoverTexture.SetPixel(0, 0, _buttonHoverColor);
      _buttonHoverTexture.Apply();

      _buttonStyle = new GUIStyle(GUI.skin.button)
      {
        normal = { background = _buttonNormalTexture, textColor = _buttonForegroundColor },
        hover  = { background = _buttonHoverTexture, textColor = _buttonForegroundColor },
        active = { background = _buttonHoverTexture, textColor = Color.gray },
        border = new RectOffset(0, 0, 0, 0)
      };
    }

    private void OnDestroy()
    {
      if (_buttonNormalTexture != null)
        Destroy(_buttonNormalTexture);
      if (_buttonHoverTexture != null)
        Destroy(_buttonHoverTexture);
    }
  }
}
