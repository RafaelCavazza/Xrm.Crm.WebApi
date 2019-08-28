using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Exception;
using Xrm.Crm.WebApi.Models;
using Xrm.Crm.WebApi.Request;

namespace Xrm.Crm.WebApi.Actions
{
    public class Merge
    {
        public EntityReference Target {get;set;}
        public EntityReference Subordinate {get;set;}
        public Dictionary<string, object> UpdateContent {get;set;}
        public bool ParentingChecks {get;set;}
        private EntityDefinition entityDefinition;

        private static readonly string[] mergeEntities = { "account", "contact", "incident", "lead" };


        public JObject GetRequestObject(WebApiMetadata webApiMetadata)
        {
            SetEntityDefinitions(webApiMetadata);
            var requestObject = new JObject();
            requestObject.Add(GetTargetProperty());
            requestObject.Add(GetSubordinateProperty());
            requestObject.Add("PerformParentingChecks", ParentingChecks);
            requestObject.Add(GetUpdateContent(webApiMetadata));
            return requestObject;
        }

        public JProperty GetTargetProperty()
        {
            var jObject = new JObject();
            jObject[entityDefinition.PrimaryIdAttribute] = Target.Id.ToString("D");
            jObject["@odata.type"] = $"Microsoft.Dynamics.CRM.{entityDefinition.LogicalName}";
            return new JProperty("Target",jObject);
        }

        public JProperty GetSubordinateProperty()
        {
            var jObject = new JObject();
            jObject[entityDefinition.PrimaryIdAttribute] = Subordinate.Id.ToString("D");
            jObject["@odata.type"] = $"Microsoft.Dynamics.CRM.{entityDefinition.LogicalName}";
            return new JProperty("Subordinate",jObject);
        }

        public JProperty GetUpdateContent(WebApiMetadata webApiMetadata)
        {

            var jObject = new JObject();
            jObject["@odata.type"] = $"Microsoft.Dynamics.CRM.{entityDefinition.LogicalName}";
            jObject[entityDefinition.PrimaryIdAttribute] = Target.Id.ToString("D");

            if(UpdateContent == null)
            {
                return new JProperty("UpdateContent", jObject);
            }

            foreach(KeyValuePair<string, object> attribute in UpdateContent)
            {
                if(jObject.ContainsKey(attribute.Key))
                {
                    continue;
                }

                jObject.Add(RequestEntityParser.GetJTokenFromAttribute(attribute.Key, attribute.Value, webApiMetadata));
            }
            return new JProperty("UpdateContent", jObject);
        }

        private void SetEntityDefinitions(WebApiMetadata webApiMetadata)
        {

            if(Target == null)
            {
                throw new WebApiException("Missing 'Target' value.");
            }

            if(Subordinate == null)
            {
                throw new WebApiException("Missing 'Subordinate' value.");
            }

            EntityDefinition targetEntityDefinitions = webApiMetadata.GetEntityDefinition(Target.LogicalName);
            EntityDefinition subordinateEntityDefinitions = webApiMetadata.GetEntityDefinition(Target.LogicalName);

            if(targetEntityDefinitions.LogicalName != subordinateEntityDefinitions.LogicalName)
            {
                throw new WebApiException("Target and Subordinate must be the same entity type.");
            }

            if(mergeEntities.Contains(targetEntityDefinitions.LogicalName) == false )
            {
                throw new WebApiException($"Entity with name '{targetEntityDefinitions.LogicalName}' is not supported for merge.");
            }

            entityDefinition = targetEntityDefinitions;
        }
    }
}