using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.ADAL;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.BatchOperations;
using Xrm.Crm.WebApi.Enums;
using Xunit;

namespace Xrm.Crm.WebApi.Test
{
    public class UnitTest1
    {
        private static string crmConnectionString = Environment.GetEnvironmentVariable("CRM_CONNECTION",EnvironmentVariableTarget.Machine);

        [Fact]
        public void ConnectToCrm(){
            var serverToServerAuthentication = new ServerToServerAuthentication(crmConnectionString);
            var webApi = new WebApi(serverToServerAuthentication);
            var apiMetadata = webApi.WebApiMetadata.EntitiesDefinitions.ToList();
            Assert.True(apiMetadata.Count > 0);
        }   

        [Fact]
        public void RetrieveSingle()
        {
        }
    }
}
