using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xrm.Crm.WebApi.Authorization;

namespace Xrm.Crm.WebApi.ADAL
{
    public class ServerToServerAuthentication : BaseAuthorization
    {
        private readonly Guid _clientId;
        private readonly string _crmBaseUrl;
        private readonly string _clientSecret;
        private readonly Guid _tenantId;
        private const string Authority = "https://login.microsoftonline.com/";
        private AuthenticationResult _authenticationResult;


        public ServerToServerAuthentication(string crmConnectionString)
        {
            var keys = GetConectionStringValues(crmConnectionString);
            _clientId = Guid.Parse(keys["clientId"]);
            _crmBaseUrl = keys["crmBaseUrl"];
            _clientSecret = keys["clientSecret"];
            _tenantId = Guid.Parse(keys["tenantId"]);
            _authenticationResult = null;
        }

        public ServerToServerAuthentication(Guid clientId, string clientSecret, string crmBaseUrl, Guid tenantId)
        {
            _clientId = clientId;
            _crmBaseUrl = crmBaseUrl;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            _authenticationResult = null;
        }

        public override void RefreshCredentials()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
         }

        public string GetAccessToken()
        {
            if (_authenticationResult != null && _authenticationResult.ExpiresOn > DateTimeOffset.UtcNow)
                return _authenticationResult.AccessToken;

            RefreshAccessToken();
            return _authenticationResult.AccessToken;
        }

        private void RefreshAccessToken()
        {
            var clientcred = new ClientCredential(_clientId.ToString(), _clientSecret);
            var authContext = new AuthenticationContext(Authority + _tenantId);
            var authenticationResult = authContext.AcquireTokenAsync(_crmBaseUrl, clientcred);
            authenticationResult.Wait();
            _authenticationResult =  authenticationResult.Result;
        }

        public override string GetCrmBaseUrl()
        {
            return _crmBaseUrl.ToLower();
        }

        private Dictionary<string, string> GetConectionStringValues(string crmConnectionString)
        {
            var keysValues = crmConnectionString.Split(';');
            var keysDictionary = new Dictionary<string,string>();

            foreach(var keyValue in keysValues)
            {
                var key = keyValue.Substring(0,keyValue.IndexOf('='));
                var value = keyValue.Substring(keyValue.IndexOf('=')+1);
                keysDictionary.Add(key,value);
            }

            return keysDictionary;
        }
    }
}
