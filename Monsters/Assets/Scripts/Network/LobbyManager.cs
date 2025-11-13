using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public DiscoveryListener discoveryListener;
    public DiscoveryBroadcaster broadcaster; // добавь/включай на хосте
    public RectTransform serverListContent;
    public GameObject serverListItemPrefab; // prefab: UI entry with hostNameText + joinButton
    public InputField hostNameInput;
    public Button createButton;
    public Button refreshButton;
    public Button startGameButton; // только хост видит и нажимает

    private Dictionary<string, GameObject> spawnedItems = new Dictionary<string, GameObject>();

    void Start()
    {
        createButton.onClick.AddListener(OnCreateClick);
        refreshButton.onClick.AddListener(RefreshList);
        startGameButton.onClick.AddListener(OnStartGame);
        startGameButton.gameObject.SetActive(false);
        InvokeRepeating(nameof(RefreshList), 0.5f, 1f);
    }

    void OnCreateClick()
    {
        string hostName = string.IsNullOrEmpty(hostNameInput.text) ? "Host" : hostNameInput.text;
        // Start host
        NetworkManager.Singleton.StartHost();
        // start broadcaster with hostName
        if (broadcaster != null)
        {
            broadcaster.hostName = hostName;
            broadcaster.maxPlayers = 8;
            broadcaster.enabled = true;
        }
        createButton.interactable = false;
        startGameButton.gameObject.SetActive(true);
    }

    void RefreshList()
    {
        // Clear existing list and populate from discoveryListener.lobbies
        var lobbies = discoveryListener.lobbies;
        lock (lobbies)
        {
            // remove stale items
            foreach (var k in new List<string>(spawnedItems.Keys))
            {
                if (!lobbies.ContainsKey(k))
                {
                    Destroy(spawnedItems[k]);
                    spawnedItems.Remove(k);
                }
            }

            // add/update
            foreach (var kv in lobbies)
            {
                string key = kv.Key;
                var info = kv.Value;
                if (!spawnedItems.ContainsKey(key))
                {
                    var go = Instantiate(serverListItemPrefab, serverListContent);
                    go.transform.Find("HostName").GetComponent<UnityEngine.UI.Text>().text = info.host;
                    go.transform.Find("Players").GetComponent<UnityEngine.UI.Text>().text = $"{info.players}/{info.maxPlayers}";
                    var btn = go.transform.Find("JoinButton").GetComponent<Button>();
                    string ip = info.ip;
                    int port = info.port;
                    btn.onClick.AddListener(() => Join(ip, (ushort)port));
                    spawnedItems[key] = go;
                }
                else
                {
                    // update players text
                    spawnedItems[key].transform.Find("Players").GetComponent<UnityEngine.UI.Text>().text = $"{info.players}/{info.maxPlayers}";
                }
            }
        }
    }

    public void Join(string ip, ushort port)
    {
        // set connection data and start client
        NetworkManagerWrapper.SetClientConnection(ip, port);
        NetworkManager.Singleton.StartClient();

        // optionally disable UI to show connecting...
    }

    public void OnStartGame()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only host can start game.");
            return;
        }

        // отключаем рассылку (подать started=true возможно)
        // Здесь используем Netcode SceneManager для загрузки сцены всем клиентам:
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
