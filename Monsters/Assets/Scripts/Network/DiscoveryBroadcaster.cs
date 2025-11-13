using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class DiscoveryBroadcaster : MonoBehaviour
{
    public int broadcastPort = 47777;
    public float broadcastInterval = 1f;
    public string gameId = "MonsterArena";
    public string hostName = "HostPlayer";
    public int maxPlayers = 8;

    private UdpClient udp;
    private bool running;

    void Start()
    {
        udp = new UdpClient();
        udp.EnableBroadcast = true;
        running = true;
        InvokeRepeating(nameof(SendBroadcast), 0.2f, broadcastInterval);
    }

    void SendBroadcast()
    {
        if (!running) return;

        var payload = new BroadcastPayload()
        {
            game = gameId,
            host = hostName,
            players = NetworkManagerWrapper.CurrentPlayerCount(),
            maxPlayers = maxPlayers,
            ip = GetLocalIPAddress(),
            port = 7777,
            started = false
        };

        string json = JsonUtility.ToJson(payload);
        byte[] data = Encoding.UTF8.GetBytes(json);

        try
        {
            udp.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, broadcastPort));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Broadcast send failed: " + ex.Message);
        }
    }

    void OnDestroy()
    {
        running = false;
        CancelInvoke();
        udp?.Close();
    }

    string GetLocalIPAddress()
    {
        string localIP = "127.0.0.1";
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch { }
        return localIP;
    }
}
