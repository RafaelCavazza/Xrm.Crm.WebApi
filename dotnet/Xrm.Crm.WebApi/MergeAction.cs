using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Exception;

namespace Xrm.Crm.WebApi.Request
{
    public class MergeAction
    {
        public EntityReference Target {get;set;}
        public EntityReference Subordinate {get;set;}
        public Dictionary<string, object> UpdateContent {get;set;}
        public bool ParentingChecks {get;set;}
        private EntityDefinitions EntityDefinitions;

        private static string[] MergeEntities = new string[] {"account","contact","incident","lead"};


        public JObject GetRequestObject(WebApiMetadata webApiMetadata){
            SetEntityDefinitions(webApiMetadata);
            var requestObject = new JObject();
            requestObject.Add(GetTargetProperty());
            requestObject.Add(GetSubordinateProperty());
            requestObject.Add("PerformParentingChecks", ParentingChecks);
            requestObject.Add(GetUpdateContent(webApiMetadata));
            return requestObject;
        }

        public JProperty GetTargetProperty(){
            var jObject = new JObject();
            jObject[EntityDefinitions.PrimaryIdAttribute] = Target.Id.ToString("D");
            jObject["@odata.type"] = $"Microsoft.Dynamics.CRM.{EntityDefinitions.LogicalName}";
            return new JProperty("Target",jObject);
        }

        public JProperty GetSubordinateProperty(){
            var jObject = new JObject();
            jObject[EntityDefinitions.PrimaryIdAttribute] = Subordinate.Id.ToString("D");
            jObject["@odata.type"] = $"Microsoft.Dynamics.CRM.{EntityDefinitions.LogicalName}";
            return new JProperty("Subordinate",jObject);
        }

        public JProperty GetUpdateContent(WebApiMetadata webApiMetadata){

            var jObject = new JObject();
            jObject["@odata.type"] = $"Microsoft.Dynamics.CRM.{EntityDefinitions.LogicalName}";
            jObject[EntityDefinitions.PrimaryIdAttribute] = Target.Id.ToString("D");

            if(UpdateContent == null)
                return new JProperty("UpdateContent", jObject);

            foreach(var attribute in UpdateContent){
                if(jObject.ContainsKey(attribute.Key))
                    continue;
                jObject.Add(RequestEntityParser.GetJTokenFromAttribute(attribute.Key, attribute.Value, webApiMetadata));
            }
            return new JProperty("UpdateContent", jObject);
        }

        private void SetEntityDefinitions(WebApiMetadata webApiMetadata){

            if(Target == null)
                throw new WebApiException("Missing 'Target' value.");

            if(Subordinate == null)
                throw new WebApiException("Missing 'Subordinate' value.");

            var targetEntityDefinitions = webApiMetadata.GetEntityDefinitions(Target.LogicalName);
            var subordinateEntityDefinitions = webApiMetadata.GetEntityDefinitions(Target.LogicalName);

            if(targetEntityDefinitions.LogicalName != subordinateEntityDefinitions.LogicalName)
                throw new WebApiException("Target and Subordinate must be the same entity type.");

            if(MergeEntities.Contains(targetEntityDefinitions.LogicalName) == false )
                throw new WebApiException($"Entity with name '{targetEntityDefinitions.LogicalName}' is not supported for merge.");

            EntityDefinitions = targetEntityDefinitions;
        }
    }
}