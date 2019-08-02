using System.Net;
using UnityEngine;
using Unity.Ucg.Usqp;

public class SampleSqpServerGameObjectScript : MonoBehaviour
{
    UsqpServer m_Server;

    [Tooltip("IP address to bind to the SQP Server")]
    public string IpToBindToServer = "127.0.0.1";

    [Tooltip("Port to bind to the SQP Server")]
    public int PortToBindToServer = 9999;

    // Create a new Sqp Data object and instantiate it with some sample values
    // These values can be overriden on the GameObject
    [Tooltip("The data to be sent in response to SQP queries")]
    public ServerInfo.Data SqpServerData = new ServerInfo.Data()
    {
        BuildId = "Test Build 1234",
        CurrentPlayers = 6,
        GameType = "Test Game Type",
        Map = "Test Map",
        MaxPlayers = 16,
        Port = 1234,
        ServerName = "Test Server"
    };

    void Start()
    {
        // Spin up a new SQP server
        var address = IPAddress.Parse(IpToBindToServer);
        var endpoint = new IPEndPoint(address, PortToBindToServer);
        m_Server = new UsqpServer(endpoint)
        {
            // Use our GameObject's SQP data as the server's data
            ServerInfoData = SqpServerData
        };
    }

    void FixedUpdate()
    {
        // Update server
        m_Server?.Update();
    }
}
