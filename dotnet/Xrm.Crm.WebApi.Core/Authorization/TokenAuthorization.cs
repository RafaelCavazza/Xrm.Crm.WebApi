using System;
using System.Net.Http.Headers;

namespace Xrm.Crm.WebApi.Core.Authorization
{
    public class TokenAuthorization : BaseAuthorization
    {
        private readonly string _accessToken;
        private readonly string _crmBaseUrl;
        private readonly Func<string> _accessTokenRefresh;

        public TokenAuthorization(string accessToken, string crmBaseUrl, Func<string> accessTokenRefresh = null) : base()
        {
            _accessToken = accessToken;
            _accessTokenRefresh = accessTokenRefresh;
            _crmBaseUrl = crmBaseUrl;
        }

        protected virtual string GetAccessToken()
        {
            if (_accessTokenRefresh != null)
                return _accessTokenRefresh();
            return _accessToken;
        }

        public override void RefreshCredentials()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
        }

        public override string GetCrmBaseUrl()
        {
            return _crmBaseUrl;
        }
    }
}
