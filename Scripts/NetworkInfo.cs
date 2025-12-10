using UnityEngine;
using Netick;
using Netick.Unity;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
  /// <summary>
  /// This is a helper script for quick prototyping, used to show useful network information of Netick.
  /// </summary>
  [AddComponentMenu("Netick/Network Info")]
  public class NetworkInfo : NetworkEventsListener
  {
    [Header("Network Stats")]
    public bool     ShowProfilerStats         = false;
    public Vector2  StartOffset               = new Vector2(27, 20);
    public Vector2  Spacing                   = new Vector2(0, 15);

    [Header("Network Conditions Icons")]
    public float    MediumLatencyThreshold    = 150;
    public float    HighLatencyThreshold      = 250;
    public float    MediumPacketLossThreshold = 1;
    public float    HighPacketLossThreshold   = 10;

    public Vector2  PacketLossIconOffset      = new Vector2(-80, 30);
    public Vector2  LatencyIconOffset         = new Vector2(-80, 70);
    public Vector2  ServerLagIconOffset       = new Vector2(-80, 110);
    public float    IconSize                  = 30;

    private Texture _packetLossIcon;
    private Texture _latencyIcon;
    private Texture _serverLagIcon;
    private Vector2 _curOffset;

    private void Awake()
    {
      _packetLossIcon = Resources.Load<Texture>("Network Icons/PacketLoss");
      _latencyIcon    = Resources.Load<Texture>("Network Icons/Latency");
      _serverLagIcon  = Resources.Load<Texture>("Network Icons/ServerLag");
    }

    private void OnGUI()
    {
      if (Network.IsRunning)
      {
        _curOffset = StartOffset;

        if (Sandbox != null && Sandbox.IsConnected && Sandbox.IsVisible)
        {
          DrawText("RTT",          (Sandbox.RTT * 1000f).ToString(),                         "ms");
          DrawText("In",            Sandbox.InKBps.ToString(),                               "KB/s");
          DrawText("Out",           Sandbox.OutKBps.ToString(),                              "KB/s");
          DrawText("In Loss",      (Sandbox.InPacketLoss * 100f).ToString(),                 "%");
          DrawText("Out Loss",     (Sandbox.OutPacketLoss * 100f).ToString(),                "%");
          DrawText("Interp Delay", (Sandbox.InterpolationDelay * 1000f).ToString(),          "ms");
          DrawText("Resims",        Sandbox.Monitor.Resimulations.Average.ToString(),        "ticks");
          DrawText("Srv Tick Time", (Sandbox.Monitor.ServerTickTime.Max * 1000f).ToString(), "ms");
          DrawText("Delta Time",    (Time.deltaTime * 1000f).ToString(),                     "ms");

          DrawIcons();
        }

        if (Sandbox != null && Sandbox.IsVisible && ShowProfilerStats && Sandbox.Config.EnableProfiling)
        {
          DrawText("Tick Time",                  (Sandbox.Monitor.TickProfiler.Time).ToString(),                  "ms");
          DrawText("NetworkFixedUpdate Time",    (Sandbox.Monitor.FixedUpdateProfiler.Time).ToString(),           "ms");

          if (Sandbox.IsClient)
            DrawText("Resimulate Time",          (Sandbox.Monitor.ResimulateProfiler.Time).ToString(),            "ms");

          DrawText("NetworkRender Time",         (Sandbox.Monitor.RenderProfiler.Time).ToString(),                "ms");

          if (Sandbox.IsServer && Sandbox.IsRecording)
            DrawText("Replay Write Time",        (Sandbox.Monitor.WriteReplayProfiler.Time).ToString(),           "ms");

          if (Sandbox.IsServer)
            DrawText("Process Changes Time",     (Sandbox.Monitor.ProcessStateChangesProfiler.Time).ToString(),   "ms");

          DrawText("Send Time",                  (Sandbox.Monitor.SendProfiler.Time).ToString(),                  "ms");
          DrawText("Receive Time",               (Sandbox.Monitor.ReceiveProfiler.Time).ToString(),               "ms");

          DrawText("NetcodeIntoGameEngine Time", (Sandbox.Monitor.NetcodeIntoGameEngineProfiler.Time).ToString(), "ms");
          DrawText("GameEngineIntoNetcode Time", (Sandbox.Monitor.GameEngineIntoNetcodeProfiler.Time).ToString(), "ms");
        }
      }
    }

    private void DrawText(string title, string content, string unit)
    {
      GUI.Label(new Rect(_curOffset.x,       _curOffset.y, 200, 50), $"{title}: ");
      GUI.Label(new Rect(_curOffset.x + 200, _curOffset.y, 200, 50), $"{content} {unit}");
      _curOffset += Spacing;
    }

    public void DrawIcons()
    {
      var pktLossIconPos   = PacketLossIconOffset + (Screen.width * Vector2.right);
      var latencyIconPos   = LatencyIconOffset    + (Screen.width * Vector2.right);
      var serverLagIconPos = ServerLagIconOffset  + (Screen.width * Vector2.right);

      var pktLoss          = Mathf.Max(Sandbox.InPacketLoss, Sandbox.OutPacketLoss) * 100; // multiplying by 100 to convert from a decimal to a percentage.
      var rtt              = Sandbox.RTT * 1000f; // multiplying by 1000 to convert from seconds to milliseconds.

      if (pktLoss >= MediumPacketLossThreshold)
      {
        Color color        = pktLoss < HighPacketLossThreshold ? Color.yellow : Color.red;
        GUI.DrawTexture(new Rect(pktLossIconPos, Vector2.one * IconSize), _packetLossIcon, ScaleMode.ScaleToFit, true, 1f, color, 0f, 0f);
      }

      if (rtt >= MediumLatencyThreshold)
      {
        Color color        = rtt >= HighLatencyThreshold ? Color.red : Color.yellow;
        GUI.DrawTexture(new Rect(latencyIconPos, Vector2.one * IconSize), _latencyIcon, ScaleMode.ScaleToFit, true, 1f, color, 0f, 0f);
      }

      if (Sandbox.Monitor.ServerTickTime.Max >= Sandbox.FixedDeltaTime)
      {
        Color color        = Sandbox.Monitor.ServerTickTime.Average >= Sandbox.FixedDeltaTime ? Color.red : Color.yellow;
        GUI.DrawTexture(new Rect(serverLagIconPos, Vector2.one * IconSize), _serverLagIcon, ScaleMode.ScaleToFit, true, 1f, color, 0f, 0f);
      }

    }
  }
}
