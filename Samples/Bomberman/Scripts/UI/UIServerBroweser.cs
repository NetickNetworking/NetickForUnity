using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netick;
using Netick.Unity;
public class UIServerBroweser : NetworkEventsListener
{
    [SerializeField]
    private GameObject       _UIServerElementPrefab; 
    [SerializeField]
    private Vector3          _startPosition;
    [SerializeField]
    private float            _stepSize = 50;

    [SerializeField]
    private List<GameObject> _servers = new List<GameObject>();

    public void Refresh()
    {
        Sandbox?.RefreshMatchList();
    }

    public override void OnStartup(NetworkSandbox sandbox)
    {
        if (sandbox.IsServer)
            Destroy(gameObject);
    }

    public override void OnConnectedToServer(NetworkSandbox sandbox, NetworkConnection server)
    {
        Destroy(gameObject);
    }

    public override void OnMatchListUpdate(NetworkSandbox sandbox, List<Session> sessions)
    {
        foreach (var server in _servers)
            Destroy(server);

        _servers.Clear();

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var newElement = Instantiate(_UIServerElementPrefab, gameObject.transform).GetComponent<UIServerElement>();
            newElement.Init(Sandbox,session.Name, session.IP, session.Port, _startPosition - (Vector3.up * i * _stepSize));
            _servers.Add(newElement.gameObject);
        }
    }
}
