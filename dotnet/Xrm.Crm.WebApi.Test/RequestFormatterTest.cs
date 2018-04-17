using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.ADAL;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.BatchOperations;
using Xunit;
using Xrm.Crm.WebApi.Request;

namespace Xrm.Crm.WebApi.Test
{
    public class RequestFormatterTest
    {   
        //TODO Implemente MOCK
        private static string crmConnectionString = Environment.GetEnvironmentVariable("CRM_CONNECTION",EnvironmentVariableTarget.Machine);

        [Fact]
        public void ConnectToCrm(){
            var entity = new Entity("teste","numero",105);
        }   
    }
}
