using System;
using UnityEngine;

namespace Netick.Unity
{
  /// <summary>
  /// A helper script that provides a simple built-in replay timeline UI.
  /// </summary>
  [AddComponentMenu("Netick/Replay Timeline")]
  public class ReplayTimeline : NetworkEventsListener
  {
    [Header("Layout")]
    public float      ReplayBarHeight       = 60f;
    public float      ButtonWidth           = 50f;
    public float      ButtonHeight          = 30f;
    public float      Spacing               = 0f;

    [Header("Colors")]
    public Color      ButtonForegroundColor = Color.white;
    public Color      ButtonBackgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
    public Color      ButtonHoverColor      = new(0.1f, 0.1f, 0.1f, 1f);
    public Color      BarBackgroundColor    = new(0.1f, 0.1f, 0.1f, 0.8f);

    private GUIStyle  _centeredLabelStyle;
    private GUIStyle  _barBackgroundStyle;
    private GUIStyle  _buttonStyle;
    private GUIStyle  _boldButtonStyle;

    private Texture2D _barTexture;
    private Texture2D _buttonNormalTexture;
    private Texture2D _buttonHoverTexture;

    private float     _currentPointerPos;
    private int       _targetFrame          = -1;
    private float     _timeScale            = 1f;

    private bool      IsInReplay            => Sandbox != null && Sandbox.IsRunning && Sandbox.IsReplay && Sandbox.Replay.Playback.FrameCount > 0;
    private bool      IsPlaying             => Time.timeScale != 0f;

    private void Awake()
    {
      _barBackgroundStyle = null;
    }

    private void OnDestroy()
    {
      if (_barTexture != null) 
        Destroy(_barTexture);
      if (_buttonNormalTexture != null)
        Destroy(_buttonNormalTexture);
      if (_buttonHoverTexture != null) 
        Destroy(_buttonHoverTexture);
    }

    private void Update()
    {
      if (!IsInReplay)
        return;

      if (Input.GetKeyDown(KeyCode.Space))
        TogglePaused();
    }

    private void TogglePaused()
    {
      Time.timeScale = IsPlaying ? 0 : Mathf.Max(0f, _timeScale);
    }

    private void EnsureStylesInitialized()
    {
      if (_barBackgroundStyle != null)
        return;

      _barTexture = new Texture2D(1, 1);
      _barTexture.SetPixel(0, 0, BarBackgroundColor);
      _barTexture.Apply();

      _barBackgroundStyle = new GUIStyle(GUI.skin.box)
      {
        normal  = { background = _barTexture },
        border  = new RectOffset(0, 0, 0, 0),
        padding = new RectOffset(4, 4, 0, 0)
      };

      _buttonNormalTexture = new Texture2D(1, 1);
      _buttonNormalTexture.SetPixel(0, 0, ButtonBackgroundColor);
      _buttonNormalTexture.Apply();

      _buttonHoverTexture = new Texture2D(1, 1);
      _buttonHoverTexture.SetPixel(0, 0, ButtonHoverColor);
      _buttonHoverTexture.Apply();

      _buttonStyle = new GUIStyle(GUI.skin.button)
      {
        normal = { background = _buttonNormalTexture, textColor = ButtonForegroundColor },
        hover  = { background = _buttonHoverTexture, textColor = ButtonForegroundColor },
        active = { background = _buttonHoverTexture, textColor = Color.gray },
        border = new RectOffset(0, 0, 0, 0)
      };

      _boldButtonStyle    = new GUIStyle(_buttonStyle) { fontStyle = FontStyle.Bold };
      _centeredLabelStyle = new GUIStyle(GUI.skin.label)
      {
        alignment = TextAnchor.MiddleCenter,
        normal    = { textColor = ButtonForegroundColor }
      };
    }

    private void OnGUI()
    {
      if (!IsInReplay)
        return;

      EnsureStylesInitialized();

      GUILayout.BeginArea(new Rect(0, Screen.height - ReplayBarHeight, Screen.width, ReplayBarHeight), _barBackgroundStyle);
      GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

      DrawTimelineBar();
      GUILayout.Space(Spacing);
      DrawControls();

      GUILayout.EndVertical();
      GUILayout.EndArea();
    }

    private void DrawTimelineBar()
    {
      var   playback     = Sandbox.Replay.Playback;
      float smoothPos    = Mathf.Min(playback.Position + Sandbox.LocalAlpha * Sandbox.FixedDeltaTime, playback.Duration - Sandbox.FixedDeltaTime);
      float previousPos  = _currentPointerPos;
      _currentPointerPos = GUILayout.HorizontalSlider(_currentPointerPos, 0f, playback.Duration, GUILayout.ExpandWidth(true));

      if (Mathf.Abs(_currentPointerPos - previousPos) > Mathf.Epsilon)
      {
        playback.SeekToTime(_currentPointerPos);
        _targetFrame = playback.TimeToFrameIndex(_currentPointerPos);
      }

      if (_targetFrame == -1)
        _currentPointerPos = smoothPos;
      else if (_targetFrame != playback.FrameIndex)
        _targetFrame       = -1;
    }

    private void DrawControls()
    {
      var playback   = Sandbox.Replay.Playback;
      float jumpTime = Mathf.Min(playback.Duration * 0.2f, 5f);

      GUILayout.BeginHorizontal();

      if (GUILayout.Button(IsPlaying ? "||" : ">", _buttonStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
        TogglePaused();

      GUILayout.Label($"{playback.Position:F1}s / {playback.Duration:F1}s", _centeredLabelStyle, GUILayout.Width(100f), GUILayout.Height(ButtonHeight));

      if (GUILayout.Button("<<", _buttonStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)) && IsPlaying)
        playback.SeekToTimeRelative(-jumpTime);

      if (GUILayout.Button(">>", _buttonStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)) && IsPlaying)
        playback.SeekToTimeRelative(jumpTime);

      GUILayout.Space(20f);
      GUILayout.Label("Speed:", _centeredLabelStyle, GUILayout.Width(60f), GUILayout.Height(ButtonHeight));

      DrawSpeedButton("¼×", 0.25f);
      DrawSpeedButton("½×", 0.5f);
      DrawSpeedButton("1×", 1f);
      DrawSpeedButton("2×", 2f);
      DrawSpeedButton("4×", 4f);
      DrawSpeedButton("8×", 8f);

      GUILayout.EndHorizontal();
    }

    private void DrawSpeedButton(string label, float scale)
    {
      var style = Mathf.Approximately(_timeScale, scale) ? _boldButtonStyle : _buttonStyle;

      if (GUILayout.Button(label, style, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
      {
        _timeScale = scale;
        if (IsPlaying)
          Time.timeScale = Mathf.Max(0f, _timeScale);
      }
    }
  }
}
