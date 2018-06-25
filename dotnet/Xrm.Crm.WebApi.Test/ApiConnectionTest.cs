using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.BatchOperations;
using Xrm.Crm.WebApi.Enums;
using Xunit;

namespace Xrm.Crm.WebApi.Test
{
    public class ApiConnectionTest
    {
        private static string crmConnectionString = Environment.GetEnvironmentVariable("CRM_CONNECTION",EnvironmentVariableTarget.Machine);
        private static WebApi WebApi;
        private static Guid LeadId;

        public ApiConnectionTest(){
            if(!crmConnectionString.ToUpper().Contains("HML"))
                throw new System.Exception("Must be HML Environment");

            var serverToServerAuthentication = new ServerToServerAuthentication(crmConnectionString);
            WebApi = new WebApi(serverToServerAuthentication);
        }

        [Fact]
        public void ConnectToCrmTest(){
            var apiMetadata = WebApi.WebApiMetadata.EntitiesDefinitions.ToList();
            Assert.True(apiMetadata.Count > 0);
        }   

        [Fact]
        public void CreateTest()
        {   
            var lead = new Entity("lead");
            lead["firstname"] = "Test";
            lead["lastname"] = "Test";
            LeadId = WebApi.Create(lead);

            Assert.NotEqual(LeadId, Guid.Empty);
        }

        [Fact]
        public void RetrieveSingle()
        {   
            var lead = WebApi.Retrieve("lead",LeadId);
            Assert.True(lead.GetAttributeValue<string>("firstname") == "Test");
            Assert.True(lead.GetAttributeValue<string>("lastname") == "Test");
        }
    }
}
