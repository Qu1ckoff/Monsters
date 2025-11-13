using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public static class NetworkManagerWrapper
{
    public static int CurrentPlayerCount()
    {
        if (NetworkManager.Singleton == null) return 1;
        return NetworkManager.Singleton.ConnectedClientsList.Count + (NetworkManager.Singleton.IsHost ? 0 : 1);
    }

    public static void SetClientConnection(string ip, ushort port)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(ip, port);
    }
}
