using System;
using System.Net;
using System.Security;

namespace Xrm.Crm.WebApi.Authorization
{
    public class OnPremisseAuthorization : BaseAuthorization
    {
        private readonly string _user;
        private readonly string _crmBaseUrl;
        private readonly SecureString _password;
        private readonly NetworkCredential _networkCredential;

        public OnPremisseAuthorization(NetworkCredential networkCredential, string crmBaseUrl) : base()
        {
            _networkCredential = networkCredential;
        }

        public override void RefreshCredentials()
        {
            handler.Credentials = _networkCredential;
        }

        public override string GetCrmBaseUrl()
        {
            return _crmBaseUrl;
        }
    }
}
