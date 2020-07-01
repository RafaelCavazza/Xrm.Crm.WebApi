using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;
using System.IO;
using Newtonsoft.Json;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi.Test
{
    public class QualifyLeadResponseFormatterTest
    {

        private string ResponseJSON = File.ReadAllText("TestFiles/QualifyResponse.json");

        [Fact]
        public void FormatResponseTest()
        {
            var data = JsonConvert.DeserializeObject<JObject>(ResponseJSON);
            var entities = QualifyLeadResponseFormatter.GetCreatedEntities(data);

            Assert.Equal(2, entities.Count);
            Assert.True(entities.Any(e => e.LogicalName == "contact"));
            Assert.True(entities.Any(e => e.LogicalName == "opportunity"));
        }
    }
}
