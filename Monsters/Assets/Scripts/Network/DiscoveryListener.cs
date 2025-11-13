using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class DiscoveryListener : MonoBehaviour
{
    public int listenPort = 47777;
    public float refreshTimeout = 5f; // удалять старые записи

    private UdpClient udp;
    private Thread listenThread;
    private bool running;

    // struct to hold incoming lobbies
    public class LobbyInfo
    {
        public string game;
        public string host;
        public int players;
        public int maxPlayers;
        public string ip;
        public int port;
        public bool started;
        public float lastSeen;
    }

    public Dictionary<string, LobbyInfo> lobbies = new Dictionary<string, LobbyInfo>();

    void Start()
    {
        udp = new UdpClient(listenPort);
        udp.EnableBroadcast = true;
        running = true;
        listenThread = new Thread(ListenLoop) { IsBackground = true };
        listenThread.Start();
        InvokeRepeating(nameof(CleanupOld), 1f, 1f);
    }

    void ListenLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
        while (running)
        {
            try
            {
                var data = udp.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(data);
                var payload = JsonUtility.FromJson<BroadcastPayload>(json);
                if (payload == null) continue;
                // identify lobby by ip:port
                string key = payload.ip + ":" + payload.port;
                lock (lobbies)
                {
                    LobbyInfo info;
                    if (!lobbies.ContainsKey(key))
                    {
                        info = new LobbyInfo();
                        lobbies[key] = info;
                    }
                    else info = lobbies[key];

                    info.game = payload.game;
                    info.host = payload.host;
                    info.players = payload.players;
                    info.maxPlayers = payload.maxPlayers;
                    info.ip = payload.ip;
                    info.port = payload.port;
                    info.started = payload.started;
                    info.lastSeen = Time.time;
                }
            }
            catch (SocketException) { }
            catch (System.Exception ex)
            {
                Debug.LogWarning("DiscoveryListener exception: " + ex.Message);
            }
        }
    }

    void CleanupOld()
    {
        float now = Time.time;
        List<string> toRemove = new List<string>();
        lock (lobbies)
        {
            foreach (var kv in lobbies)
            {
                if (now - kv.Value.lastSeen > refreshTimeout) toRemove.Add(kv.Key);
            }
            foreach (var k in toRemove) lobbies.Remove(k);
        }
    }

    void OnDestroy()
    {
        running = false;
        udp?.Close();
        if (listenThread != null && listenThread.IsAlive) listenThread.Abort();
    }
}
