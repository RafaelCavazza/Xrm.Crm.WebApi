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

        public ApiConnectionTest(){
            if(!crmConnectionString.ToUpper().Contains("HML"))
                throw new System.Exception("Must be HML Environment");

            var serverToServerAuthentication = new ServerToServerAuthentication(crmConnectionString);
            WebApi = new WebApi(serverToServerAuthentication);
        }

        [Fact]
        public void ConnectToCrmTest(){
            var apiMetadata = WebApi.WebApiMetadata.EntityDefinitions.ToList();
            Assert.True(apiMetadata.Count > 0);
        }   

               [Fact]
        public void CreateTest()
        {   
            var lead = new Entity("lead");
            lead["firstname"] = "Test";
            lead["lastname"] = "Test";
            var LeadId = WebApi.Create(lead);
            Assert.NotEqual(LeadId, Guid.Empty);
        }


        [Fact]
        public void FetchXmlTest()
        {   
            var lead = new Entity("lead");
            lead["firstname"] = "Test";
            lead["lastname"] = "Test";
            var LeadId = WebApi.RetrieveMultiple(@"<fetch>
                        <entity name='new_chat'>
                            <attribute name='activityid' />
                                <filter>
                                    <condition attribute='activityid' operator='eq' value='{477B2FBB-E218-E811-8114-E0071B6FC061}' />
                                </filter>
                                <link-entity name='activityparty' from='activityid' to='activityid' alias='activityparty' link-type='inner'>
                                    <all-attributes/>
                                    <filter type='or'>
                                        <condition attribute='participationtypemask' operator='eq' value='2' />                                        
                                        <condition attribute='participationtypemask' operator='eq' value='1' />
                                    </filter>
                                </link-entity>
                        </entity>
                    </fetch>");
        }

        [Fact]
        public void FetchXmlNestedEntitiesTest()
        {   
            var lead = new Entity("lead");
            lead["firstname"] = "Test";
            lead["lastname"] = "Test";
            var LeadId = WebApi.RetrieveMultiple(@"<fetch>
                        <entity name='new_chat'>
                            <all-attributes/>
                                <filter>
                                    <condition attribute='activityid' operator='eq' value='{477B2FBB-E218-E811-8114-E0071B6FC061}' />
                                </filter>
                                <link-entity name='activityparty' from='activityid' to='activityid' alias='activityparty' link-type='inner'>
                                    <all-attributes/>
                                    <filter type='or'>
                                        <condition attribute='participationtypemask' operator='eq' value='2' />                                        
                                        <condition attribute='participationtypemask' operator='eq' value='1' />
                                    </filter>
                                </link-entity>
                        </entity>
                    </fetch>");
        }
    }
}
