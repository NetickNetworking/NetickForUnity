using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netick;
using Netick.Unity;

public class UIServerElement : MonoBehaviour
{
    [SerializeField]
    private Text           _nameText;

    [SerializeField]
    private Text           _ipText;

    [SerializeField]
    private string         _ip;
    [SerializeField]
    private int            _Port;

    private NetworkSandbox _sandbox;

    public void Init(NetworkSandbox sandbox, string name, string ip, int port, Vector3 position)
    {
        _sandbox       = sandbox;

        _ip            = ip;
        _Port          = port;
        _nameText.text = name;
        _ipText.text   = ip;
        GetComponent<RectTransform>().anchoredPosition3D = position;

    }

    public void Connect()
    {
        _sandbox.Connect(_Port, _ip);
    }
}
