using UnityEngine;
using Netick;
using Netick.Unity;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
  [AddComponentMenu("Netick/Network Info")]
  public class NetworkInfo : NetworkEventsListener
  {
    public enum AnchorPoint     
    { 
      TopLeft, 
      TopRight, 
      BottomLeft,
      BottomRight 
    }

    public enum LayoutDirection
    { 
      Vertical,
      Horizontal 
    }

    [Header("Text Layout Settings")]
    public AnchorPoint          TextAnchor                = AnchorPoint.TopLeft;
    public LayoutDirection      TextLayout                = LayoutDirection.Vertical;
    public Vector2              TextStartOffset           = new Vector2(27, 20);
    public float                HorizontalPadding         = 20f;
    public float                LineHeight                = 15f;

    [Header("Icon Layout Settings")]
    public AnchorPoint          IconAnchor                = AnchorPoint.TopRight;
    public LayoutDirection      IconLayout                = LayoutDirection.Vertical;
    public Vector2              IconStartOffset           = new Vector2(-40, 40);
    public Vector2              IconSpacing               = new Vector2(10, 10);
    public float                IconSize                  = 30;

    [Header("Sizing")]
    public float                ContentFixedWidth         = 120f;
    public float                VerticalTitleOffset       = 150f;

    [Header("Network Stats Settings")]
    public bool                 ShowProfilerStats         = false;
    public float                MediumLatencyThreshold    = 150;
    public float                HighLatencyThreshold      = 250;
    public float                MediumPacketLossThreshold = 1;
    public float                HighPacketLossThreshold   = 10;

    private Texture             _packetLossIcon;
    private Texture             _latencyIcon;
    private Texture             _serverLagIcon;
    private Vector2             _curOffset;
    private GUIStyle            _labelStyle;
    private const string        FloatFormat               = "F3";

    private void Awake()
    {
      _packetLossIcon = Resources.Load<Texture>("Network Icons/PacketLoss");
      _latencyIcon    = Resources.Load<Texture>("Network Icons/Latency");
      _serverLagIcon  = Resources.Load<Texture>("Network Icons/ServerLag");
      _labelStyle     = null;
    }

    private void OnGUI()
    {
      if (Sandbox == null || !Sandbox.IsRunning || (Sandbox.IsClient && !Sandbox.IsConnected)) 
        return;

      if (_labelStyle == null)
      {
        _labelStyle          = new GUIStyle(GUI.skin.label);
        _labelStyle.wordWrap = false;
        _labelStyle.padding  = new RectOffset(0, 0, 0, 0); 
        _labelStyle.margin   = new RectOffset(0, 0, 0, 0); 
      }

      _curOffset             = GetAnchorPosition(TextAnchor) + TextStartOffset;

      if (TextLayout == LayoutDirection.Vertical && (TextAnchor == AnchorPoint.BottomLeft || TextAnchor == AnchorPoint.BottomRight))
      {
        float totalHeight    = GetTotalLineCount() * LineHeight;
        _curOffset.y        -= totalHeight;
      }

      if (Sandbox != null && Sandbox.IsVisible)
      {
        DrawAllMetrics();
        DrawIcons();
      }
    }

    private void DrawAllMetrics()
    {
      if (Sandbox.IsClient)
      {
        DrawText("RTT", (Sandbox.RTT * 1000f).ToString(FloatFormat), "ms");
      }

      DrawText("In", Sandbox.InKBps.ToString(FloatFormat), "KB/s");
      DrawText("Out", Sandbox.OutKBps.ToString(FloatFormat), "KB/s");

      if (Sandbox.IsClient)
      {
        DrawText("In Loss", (Sandbox.InPacketLoss * 100f).ToString(FloatFormat), "%");
        DrawText("Out Loss", (Sandbox.OutPacketLoss * 100f).ToString(FloatFormat), "%");
        DrawText("Interp Delay", (Sandbox.InterpolationDelay * 1000f).ToString(FloatFormat), "ms");
        DrawText("Resims", Sandbox.Monitor.Resimulations.Average.ToString(FloatFormat), "ticks");
        DrawText("Srv Tick Time", (Sandbox.Monitor.ServerTickTime.Max * 1000f).ToString(FloatFormat), "ms");
      }

      DrawText("Delta Time", (Time.deltaTime * 1000f).ToString(FloatFormat), "ms");

      if (ShowProfilerStats && Sandbox.Config.EnableProfiling)
      {
        DrawText("Tick Time", (Sandbox.Monitor.TickProfiler.Time).ToString(FloatFormat), "ms");
        DrawText("NetworkFixedUpdate Time", (Sandbox.Monitor.FixedUpdateProfiler.Time).ToString(FloatFormat), "ms");
        
        if (Sandbox.IsClient) 
          DrawText("Resimulate Time", (Sandbox.Monitor.ResimulateProfiler.Time).ToString(FloatFormat), "ms");

        DrawText("NetworkRender Time", (Sandbox.Monitor.RenderProfiler.Time).ToString(FloatFormat), "ms");
        
        if (Sandbox.IsServer && Sandbox.IsRecording) 
          DrawText("Replay Write Time", (Sandbox.Monitor.WriteReplayProfiler.Time).ToString(FloatFormat), "ms");
        
        if (Sandbox.IsServer) 
          DrawText("Process Changes Time", (Sandbox.Monitor.ProcessStateChangesProfiler.Time).ToString(FloatFormat), "ms");

        DrawText("Send Time", (Sandbox.Monitor.SendProfiler.Time).ToString(FloatFormat), "ms");
        DrawText("Receive Time", (Sandbox.Monitor.ReceiveProfiler.Time).ToString(FloatFormat), "ms");
        DrawText("NetcodeIntoGameEngine Time", (Sandbox.Monitor.NetcodeIntoGameEngineProfiler.Time).ToString(FloatFormat), "ms");
        DrawText("GameEngineIntoNetcode Time", (Sandbox.Monitor.GameEngineIntoNetcodeProfiler.Time).ToString(FloatFormat), "ms");
      }
    }

    private void DrawText(string title, string content, string unit)
    {
      string titleStr   = $"{title}: ";
      string valueStr   = $"{content} {unit}";

      float titleWidth  = _labelStyle.CalcSize(new GUIContent(titleStr)).x;
      float valueOffset = (TextLayout == LayoutDirection.Vertical) ? VerticalTitleOffset : titleWidth;
      float totalWidth  = valueOffset + ContentFixedWidth;

      float x           = _curOffset.x;
      float y           = _curOffset.y;

      if (TextAnchor == AnchorPoint.TopRight || TextAnchor == AnchorPoint.BottomRight)
        x -= totalWidth;

      GUI.Label(new Rect(x, y, titleWidth, LineHeight), titleStr, _labelStyle);
      GUI.Label(new Rect(x + valueOffset, y, ContentFixedWidth, LineHeight), valueStr, _labelStyle);

      if (TextLayout == LayoutDirection.Vertical)
        _curOffset.y += LineHeight;
      else
      {
        bool isRight  = (TextAnchor == AnchorPoint.TopRight || TextAnchor == AnchorPoint.BottomRight);
        _curOffset.x += isRight ? -(totalWidth + HorizontalPadding) : (totalWidth + HorizontalPadding);
      }
    }

    public void DrawIcons()
    {
      Vector2 iconPos = GetAnchorPosition(IconAnchor) + IconStartOffset;
      bool isRight    = (IconAnchor == AnchorPoint.TopRight || IconAnchor == AnchorPoint.BottomRight);
      bool isBottom   = (IconAnchor == AnchorPoint.BottomLeft || IconAnchor == AnchorPoint.BottomRight);

      if (isRight) 
        iconPos.x    -= IconSize;
      if (isBottom)
        iconPos.y    -= IconSize;

      Vector2 step    = Vector2.zero;
      if (IconLayout == LayoutDirection.Horizontal)
        step.x        = isRight ? -(IconSize + IconSpacing.x) : (IconSize + IconSpacing.x);
      else
        step.y        = isBottom ? -(IconSize + IconSpacing.y) : (IconSize + IconSpacing.y);

      var pktLoss     = Mathf.Max(Sandbox.InPacketLoss, Sandbox.OutPacketLoss) * 100;
      var rtt         = Sandbox.RTT * 1000f;

      if (pktLoss >= MediumPacketLossThreshold)
      {
        Color color   = pktLoss < HighPacketLossThreshold ? Color.yellow : Color.red;
        DrawIcon(ref iconPos, _packetLossIcon, color, step);
      }

      if (rtt >= MediumLatencyThreshold)
      {
        Color color   = rtt >= HighLatencyThreshold ? Color.red : Color.yellow;
        DrawIcon(ref iconPos, _latencyIcon, color, step);
      }

      if (Sandbox.Monitor.ServerTickTime.Max >= Sandbox.FixedDeltaTime)
      {
        Color color   = Sandbox.Monitor.ServerTickTime.Average >= Sandbox.FixedDeltaTime ? Color.red : Color.yellow;
        DrawIcon(ref iconPos, _serverLagIcon, color, step);
      }
    }

    private void DrawIcon(ref Vector2 pos, Texture tex, Color col, Vector2 step)
    {
      GUI.DrawTexture(new Rect(pos, Vector2.one * IconSize), tex, ScaleMode.ScaleToFit, true, 1f, col, 0f, 0f);
      pos += step;
    }

    private Vector2 GetAnchorPosition(AnchorPoint anchor)
    {
      switch (anchor)
      {
        case AnchorPoint.TopRight: return new Vector2(Screen.width, 0);
        case AnchorPoint.BottomLeft: return new Vector2(0, Screen.height);
        case AnchorPoint.BottomRight: return new Vector2(Screen.width, Screen.height);
        default: return Vector2.zero;
      }
    }

    private int GetTotalLineCount()
    {
      int lines = 0;

      if (Sandbox.IsClient)
        lines++; // RTT

      lines++; // In
      lines++; // Out

      if (Sandbox.IsClient)
      {
        lines++; // In Loss
        lines++; // Out Loss
        lines++; // Interp Delay
        lines++; // Resims
        lines++; // Srv Tick Time
      }

      lines++; // Delta Time

      // Profiler Lines 
      if (ShowProfilerStats && Sandbox.Config.EnableProfiling)
      {
        lines += 8; // The 8 static profiler lines
        if (Sandbox.IsClient) lines++; // Resimulate Time
        if (Sandbox.IsServer && Sandbox.IsRecording) lines++; // Replay Write Time
        if (Sandbox.IsServer) lines++; // Process Changes Time
      }

      return lines;
    }
  }
}