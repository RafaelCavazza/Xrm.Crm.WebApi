using System;
using Xunit;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Request;

namespace Xrm.Crm.WebApi.Test
{
    public class FetchXmlExpressionTest
    {        
        private string Xml = $@"
        <fetch>
            <entity name='contact'>
                <attribute name='fullname' />
                <attribute name='telephone1' />
                <order attribute='fullname' descending='false'/>
            </entity>
        </fetch>";

        [Fact]
        public  void ParseFetchXmlTest(){
            var fetchXml = new FetchXmlExpression(Xml);
            Assert.True(fetchXml.LogicalName == "contact");
        }   

        [Fact]
        public void ImplicitConversionTest()
        {   
            var fetchXml = new FetchXmlExpression(Xml);
            string fetchXmlString = fetchXml;
            FetchXmlExpression fetchXmlExpression = fetchXmlString;

            Assert.True(fetchXmlExpression.LogicalName == "contact");
            Assert.True(fetchXmlString.Contains("name=\"contact\""));
        }

        [Fact]
        public void ToStringTest()
        {   
            var fetchXml = new FetchXmlExpression(Xml);
            string fetchXmlString = fetchXml.ToString();
            Assert.True(!fetchXmlString.Contains(Environment.NewLine));
        }
    }
}
