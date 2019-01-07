using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.BatchOperations;
using Xunit;
using Xrm.Crm.WebApi.Request;
using System.IO;
using Newtonsoft.Json;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi.Test
{
    public class QualifyLeadResponseFormatterTest
    {           

        private string ResponseJSON = File.ReadAllText("TestFiles/QualifyResponse.json");
         
        [Fact]
        public void FormatResponseTest(){
            var data = JsonConvert.DeserializeObject<JObject>(ResponseJSON);
            var entities = QualifyLeadResponseFormatter.GetCreatedEntities(data);

            Assert.Equal(entities.Count, 2);
            Assert.True(entities.Any(e => e.LogicalName == "contact"));
            Assert.True(entities.Any(e => e.LogicalName == "opportunity"));
        }   
    }
}
