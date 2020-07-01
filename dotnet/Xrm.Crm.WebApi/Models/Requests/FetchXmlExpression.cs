using System.Linq;
using System.Xml.Linq;

namespace Xrm.Crm.WebApi.Models.Requests
{
    public class FetchXmlExpression
    {
        private XDocument document;
        public string LogicalName {get; internal set;}

        public FetchXmlExpression(string fetchXml)
        {
            document = XDocument.Parse(fetchXml);
            LogicalName = document
                .Descendants("entity")
                .First()
                .Attribute("name")?.Value;
        }

        public override string ToString()
        {
            return document.ToString(SaveOptions.DisableFormatting);
        }

        public static implicit operator FetchXmlExpression(string fetchXml)
        {
            return new FetchXmlExpression(fetchXml);
        }

        public static implicit operator string(FetchXmlExpression fetchXml)
        {
            return fetchXml.ToString();
        }
    }
}