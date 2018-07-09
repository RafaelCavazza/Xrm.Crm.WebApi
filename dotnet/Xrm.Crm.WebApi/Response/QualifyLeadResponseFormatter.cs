using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi.Response
{
    public static class QualifyLeadResponseFormatter
    {
        public static List<Entity> GetCreatedEntities(JObject data){
            var values = data["value"].ToObject<JArray>();
            var entities = new List<Entity>();

            foreach(var value in values){
                var entity = ResponseAttributeFormatter.FormatEntityResponse( (JObject) value);
                var type = value["@odata.type"].ToString();
                var logicalName = type.Split('.').Last();
                entity.LogicalName = logicalName;
                entities.Add(entity);
            }
            return entities;
        }
    }
}