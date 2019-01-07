using System;
using System.Collections.Generic;

namespace Xrm.Crm.WebApi.Authorization
{
    public class ServerToServerAuthentication : BaseAuthorization
    {
        private readonly Guid _clientId;
        private readonly string _crmBaseUrl;
        private readonly string _clientSecret;
        private readonly Guid _tenantId;
        private const string Authority = "https://login.microsoftonline.com/";
        private AuthenticationResult AuthenticationResult;


        public ServerToServerAuthentication(string crmConnectionString)
        {
            var keys = GetConectionStringValues(crmConnectionString);
            _clientId = Guid.Parse(keys["clientId"]);
            _crmBaseUrl = keys["crmBaseUrl"];
            _clientSecret = keys["clientSecret"];
            _tenantId = Guid.Parse(keys["tenantId"]);
            AuthenticationResult = null;
        }

        public ServerToServerAuthentication(Guid clientId, string clientSecret, string crmBaseUrl, Guid tenantId)
        {
            _clientId = clientId;
            _crmBaseUrl = crmBaseUrl;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            AuthenticationResult = null;
        }

        public override void RefreshCredentials()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
         }

        public string GetAccessToken()
        {
            if (AuthenticationResult != null && AuthenticationResult.IsValid())
                return AuthenticationResult.AccessToken;

            RefreshAccessToken();
            return AuthenticationResult.AccessToken;
        }

        private void RefreshAccessToken()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _clientId.ToString()),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("resource", _crmBaseUrl)
            });
            
            var result = httpClient.PostAsync(GetOauth2Url(), content).GetAwaiter().GetResult();
            result.EnsureSuccessStatusCode();
            
            string resultContent = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            AuthenticationResult = JsonConvert.DeserializeObject<AuthenticationResult>(resultContent);
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

        private string GetOauth2Url(){
            return $"{Authority}/{_tenantId.ToString("D")}/oauth2/token";
        }
    }
}