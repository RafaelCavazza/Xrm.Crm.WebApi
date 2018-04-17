using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Xrm.Crm.WebApi
{
    public class FetchXmlExpression
    {
        private XDocument document;
        public string LogicalName {get; internal set;}

        public FetchXmlExpression(string fetchXml)
        {
            document = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(fetchXml.ToLower())));
            LogicalName = (string) document
                            .Descendants("entity")
                            .First()
                            .Attribute("name");
        }

        public override string ToString()
        {
            return document.ToString();
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