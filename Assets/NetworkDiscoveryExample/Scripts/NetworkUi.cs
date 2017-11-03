using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AXNGames.Networking
{
    public class NetworkUi : MonoBehaviour
    {
        public Button joinButton;
        public Button hostButton;
        public GameObject hostPanel;
        public Text ipAddressText;

        public void HostGame()
        {
            CustomNetworkDiscovery.Instance.StartBroadcasting();
            NetworkManager.singleton.StartHost();
        }

        public void ReceiveGameBroadcast()
        {
            CustomNetworkDiscovery.Instance.ReceiveBroadcast();
        }

        public void JoinGame()
        {
            NetworkManager.singleton.networkAddress = ipAddressText.text;
            NetworkManager.singleton.StartClient();
            CustomNetworkDiscovery.Instance.StopBroadcasting();
        }

        public void OnReceiveBraodcast(string fromIp, string data)
        {
            hostButton.gameObject.SetActive(false);
            joinButton.gameObject.SetActive(false);
            ipAddressText.text = fromIp;
            hostPanel.SetActive(true);
        }

        void Start()
        {
            CustomNetworkDiscovery.Instance.onServerDetected += OnReceiveBraodcast;
        }

        void OnDestroy()
        {
            CustomNetworkDiscovery.Instance.onServerDetected -= OnReceiveBraodcast;
        }
    }

}
