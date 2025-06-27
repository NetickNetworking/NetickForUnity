using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.Bomberman
{
  public class Block : NetworkBehaviour
  {
    // Networked Properties
    [Networked]
    public NetworkBool Visible { get; set; } = true;

    [OnChanged(nameof(Visible))]
    private void OnVisibleChanged(OnChangedData onChangedData)
    {
      // for visual components, don't use "enabled" property when you want to disable/enable it, instead use SetEnabled().
      // -- GetComponent<Renderer>().enabled = Visible; #### Not like this.

      GetComponent<Renderer>().SetEnabled(Sandbox, Visible); // #### Like this.

      GetComponent<BoxCollider>().enabled = Visible;
    }
  }
}

