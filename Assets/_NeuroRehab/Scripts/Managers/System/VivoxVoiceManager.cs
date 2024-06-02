using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Linq;

namespace NeuroRehab.Managers{

    /// <summary>
	/// Used to register a user in the Vivox service
    /// </summary>
    public class VivoxVoiceManager : MonoBehaviour {
        [SerializeField] private string _key;
        [SerializeField] private string _issuer;
        [SerializeField] private string _domain;
        [SerializeField] private string _server;

        private static bool isAppInitialized = false;

        private void Awake() {
            if (isAppInitialized)
            {
                Destroy(gameObject);
                return;
            }

            isAppInitialized = true;
            DontDestroyOnLoad(gameObject);
            InitializeVivox();
            
        }

        private async void InitializeVivox() {
            var options = new InitializationOptions();
            if (CheckManualCredentials())
            {
                options.SetVivoxCredentials(_server, _domain, _issuer, _key);
            }

            await UnityServices.InitializeAsync(options);

            if (!CheckManualCredentials())
            {
                AuthenticationService.Instance.ClearSessionToken();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            await VivoxService.Instance.InitializeAsync();

            if (!VivoxService.Instance.IsLoggedIn)
        	{
                LoginOptions loginOptions = new LoginOptions
                {
                    DisplayName = GenerateRandomUsername()
                };
                await VivoxService.Instance.LoginAsync(loginOptions);
        	}
        }

        private bool CheckManualCredentials() {
            return !(string.IsNullOrEmpty(_issuer) && string.IsNullOrEmpty(_domain) && string.IsNullOrEmpty(_server));
        }

        public string GenerateRandomUsername(int length = 8) {
     		System.Random random = new System.Random();
        	const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        	return new string(Enumerable.Repeat(chars, length)
            	.Select(s => s[random.Next(s.Length)]).ToArray());
    	}
    }
}