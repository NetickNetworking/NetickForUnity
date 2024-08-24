using UnityEngine;
using Netick;
using Netick.Unity;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
    [AddComponentMenu("Netick/Network Info")]
    public class NetworkInfo : NetworkEventsListener
    { 
         [Header("Network Stats")]
         public Vector2  Offset                    = new Vector2(27, 20);

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
                if (Sandbox != null && Sandbox.IsConnected)
                {
                    DrawText(0, "RTT",           (Sandbox.RTT * 1000f).ToString(), "ms");
                    DrawText(1, "In",            Sandbox.InKBps.ToString(), "KB/s");
                    DrawText(2, "Out",           Sandbox.OutKBps.ToString(), "KB/s");
                    DrawText(3, "In Loss",       (Sandbox.InPacketLoss  * 100f).ToString(), "%");
                    DrawText(4, "Out Loss",      (Sandbox.OutPacketLoss * 100f).ToString(), "%");
                    DrawText(5, "Interp Delay",  (Sandbox.InterpolationDelay * 1000f).ToString(), "ms");
                    DrawText(6, "Resims",        Sandbox.Monitor.Resimulations.Average.ToString(), "Ticks");
                    DrawText(7, "Srv Tick Time", (Sandbox.Monitor.ServerTickTime.Max * 1000f).ToString(), "ms");
                    DrawText(8, "Delta time",    (Time.deltaTime * 1000f).ToString(), "ms");

                    DrawIcons();
                }
            }
        }

        private void DrawText(int offset, string title, string content, string unit)
        {
            GUI.Label(new Rect(Offset.x + 10,  Offset.y + 10 + (15 * offset), 200, 50), $"{title}: ");
            GUI.Label(new Rect(Offset.x + 130, Offset.y + 10 + (15 * offset), 200, 50), $"{content} {unit}");
        }

        public void DrawIcons()
        {
            var pktLossIconPos   = PacketLossIconOffset + (Screen.width * Vector2.right);
            var latencyIconPos   = LatencyIconOffset + (Screen.width * Vector2.right);
            var serverLagIconPos = ServerLagIconOffset + (Screen.width * Vector2.right);

            var pktLoss          = Mathf.Max(Sandbox.InPacketLoss, Sandbox.OutPacketLoss) * 100; // multiplying by 100 to convert from a decimal to a percentage.
            var rtt              = Sandbox.RTT * 1000f; // multiplying by 1000 to convert from seconds to milliseconds.

            if (pktLoss >= MediumPacketLossThreshold)
            {
                Color color      = pktLoss < HighPacketLossThreshold ? Color.yellow : Color.red;
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
