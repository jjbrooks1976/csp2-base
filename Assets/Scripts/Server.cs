using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class Server : MonoBehaviour
{
    public const string ADDRESS = "127.0.0.1";
    public const ushort PORT = 9000;

    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    void Start()
    {
        InitializeNetwork();
    }

    void Update()
    {
    }

    private void InitializeNetwork()
    {
        networkDriver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = PORT;
        if (networkDriver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind to port " + PORT);
        }
        else
        {
            Debug.Log("Listing on port " + PORT);
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    void OnDestroy()
    {
        if (networkDriver.IsCreated)
        {
            networkDriver.Dispose();
            connections.Dispose();
        }
    }
}
