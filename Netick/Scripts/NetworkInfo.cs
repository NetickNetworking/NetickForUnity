using UnityEngine;
using Netick;
using Netick.Unity;
using Network = Netick.Unity.Network;

namespace Netick.Samples
{
    [AddComponentMenu("Netick/Network Info")]
    public class NetworkInfo : NetworkEventsListener
    {
        private void OnGUI()
        {
            if (Network.Instance != null)
            {
                if (Sandbox != null && Sandbox.IsConnected)
                {
                    Draw(0, "In",            Sandbox.InKBps.ToString(), "KB/s");
                    Draw(1, "Out",           Sandbox.OutKBps.ToString(), "KB/s");
                    Draw(2, "RTT",           (Sandbox.RTT * 1000f).ToString(), "ms");
                    Draw(3, "Interp Delay",  (Sandbox.InterpolationDelay * 1000f).ToString(), "ms");
                    Draw(4, "Resims",        Sandbox.Monitor.Resimulations.Average.ToString(), "Ticks");
                    Draw(5, "Delta time",    (Time.deltaTime * 1000f).ToString(), "ms");
                }
            }
        }

        private void Draw(int offset, string title, string content, string unit)
        {
            GUI.Label(new Rect(10,  10 + (15 * offset), 200, 50), $"{title}: ");
            GUI.Label(new Rect(130, 10 + (15 * offset), 200, 50), $"{content} {unit}");
        }
    }
}
