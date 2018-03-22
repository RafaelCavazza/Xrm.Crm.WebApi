using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.ADAL;
using Xrm.Crm.WebApi.Core;
using Xrm.Crm.WebApi.BatchOperations;
using Xrm.Crm.WebApi.Core.Enums;
using Xunit;

namespace Xrm.Crm.WebApi.Test
{
    public class UnitTest1
    {
        private static string crmConnectionString = Environment.GetEnvironmentVariable("CRM_CONNECTION",EnvironmentVariableTarget.Machine);
        private readonly WebApi _webApi;

        public UnitTest1(){
            var serverToServerAuthentication = new ServerToServerAuthentication(crmConnectionString);
            _webApi = new WebApi(serverToServerAuthentication);
        }   

        [Fact]
        public void RetriveSingle()
        {
        }
    }
}
