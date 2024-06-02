using System.Collections.Generic;
using TMPro;
using Mirror;
using UnityEngine;

using NeuroRehab.Core;
using NeuroRehab.ScriptObjects;
using UnityEngine.UI;
using System.Net;

namespace NeuroRehab.UI.Managers.Menu {

    /// <summary>
	/// Displays a list of servers and buttons to add/delete server from the server list
    /// </summary>
    public class ServerMenuManager : MonoBehaviour
    {
        [SerializeField] private NetworkSettings networkSettings;
        [SerializeField] private TMP_Dropdown serversDropdown;
        [SerializeField] private TMP_InputField inputServerField;
        [SerializeField] private Button addServerButton;
        [SerializeField] private Button deleteServerButton;
        private TouchScreenKeyboard keyboard;

        void Start()
        {
            UpdateServersDropdown();
            serversDropdown.onValueChanged.AddListener(ServerDropdownHandler);

            inputServerField.onSelect.AddListener(ShowKeyboard);
            inputServerField.onDeselect.AddListener(HideKeyboard);
            addServerButton.onClick.AddListener(AddServerButtonClick);
            deleteServerButton.onClick.AddListener(RemoveSelectedServer);
        }

        private void UpdateServersDropdown()
        {
            serversDropdown.ClearOptions();

            List<string> ipList = new();
            foreach (var server in networkSettings.ServersList)
            {
                ipList.Add($"{server.serverIp}:{server.serverPort}");
            }

            serversDropdown.AddOptions(ipList);
            int selectedIndex = serversDropdown.options.FindIndex(option => option.text.Split(':')[0] == networkSettings.IpAddress);
            if (selectedIndex != -1)
            {
                serversDropdown.value = selectedIndex;
            }
            else
            {
                serversDropdown.value = 0;
            }

            serversDropdown.RefreshShownValue();
        }

        private void ShowKeyboard(string arg0)
        {
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        }

        private void HideKeyboard(string arg0)
        {
            if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                keyboard = null;
            }
        }
        private void RemoveSelectedServer()
        {
            int selectedIndex = serversDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < networkSettings.ServersList.Count)
            {
                networkSettings.ServersList.RemoveAt(selectedIndex);
                UpdateServersDropdown();
            }
        }

        private void ServerDropdownHandler(int value)
        {
            string selectedIPPort = serversDropdown.options[serversDropdown.value].text;
            string[] parts = selectedIPPort.Split(':');
            string ip = parts[0];
            int port = int.Parse(parts[1]);
            NetworkManager.singleton.networkAddress = ip;
            networkSettings.IpAddress = ip;
            networkSettings.WebsocketPort = port;
        }

        public void AddServerButtonClick()
        {
            AddServerToList(inputServerField.text);
            inputServerField.text = "";
            UpdateServersDropdown();
        }

        private void AddServerToList(string inputText)
        {
            string[] parts = inputText.Split(':');
            if (parts.Length == 2 && IPAddress.TryParse(parts[0], out _) && int.TryParse(parts[1], out int port))
            {
                networkSettings.ServersList.Add(new ServersData
                {
                    serverIp = parts[0],
                    serverPort = port
                });
            }
        }
    }
}
