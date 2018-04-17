using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Xrm.Crm.WebApi;

namespace Xrm.Crm.WebApi.Reponse
{
    public class RetriveMultipleResponse
    {
        public List<Entity> Entities { get; set; }
        public string NextLink { get; set; }
        public string RecordCount { get; set; }
        public string FetchxmlPagingCookie { get; set; }
        public string OdataContext { get; set; }

        public RetriveMultipleResponse(JObject result)
        {
            Entities = new List<Entity>();
            AddResult(result);
        }

        public void AddResult(JObject result)
        {
            foreach (JObject value in result["value"]?.ToList())
            {
                var entity = ResponseAttributeFormatter.FormatEntityResponde(value);
                Entities.Add(entity);
            }

            NextLink = result["@odata.nextLink"]?.ToString();
            RecordCount = result["@odata.count"]?.ToString();
            OdataContext = result["@odata.context"]?.ToString();
        }
    }
}
