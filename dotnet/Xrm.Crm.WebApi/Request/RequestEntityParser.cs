using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Metadata;

namespace Xrm.Crm.WebApi.Request
{
    public static class RequestEntityParser
    {
        public static JObject EntityToJObject(Entity entity, WebApiMetadata webApiMetadata)
        {
            var jObject = new JObject();           
            var entityDefinitions = webApiMetadata.GetEntityDefinitions(entity.LogicalName);

            foreach(var attibute in entity.Attributes)
            {
                var key = attibute.Key;

                if(webApiMetadata.LoadAttributes){
                    var atributo = entityDefinitions.Attributes.FirstOrDefault(a => a.SchemaName.ToLower().Equals(attibute.Key.ToLower()));
                    key = atributo.SchemaName ?? key;
                }

                if(attibute.Value is EntityReference) 
                {            
                    jObject[key + "@odata.bind"] = EntityReferenceTostring((EntityReference)attibute.Value, webApiMetadata);
                }
                else if (IsActivityPartyEnumerable(attibute.Value))
                {
                    var activityParties = GetActivityPartyCollections(attibute.Value);
                    var jArray = new JArray();
                    foreach(var activityParty in activityParties)
                    {
                        jArray.Add(ActivityPartyToJObject(activityParty, webApiMetadata));
                    }

                    jObject[key] = jArray;
                }
                else if(attibute.Value is List<Entity>)
                {
                    var objects = new JArray();
                    foreach(var nestedEntity in (List<Entity>)attibute.Value)
                    {
                        objects.Add(EntityToJObject(nestedEntity, webApiMetadata));
                    }
                    jObject[key] = objects;
                }
                else
                {
                    jObject[key] = JValue.FromObject(attibute.Value);
                }
            }

            return jObject;
        }
        
        public static string EntityReferenceTostring(EntityReference entityReference, WebApiMetadata webApiMetadata)
        {
            var logicalName = entityReference.LogicalName.ToLower();
            var entitySetName = webApiMetadata.GetEntitySetName(logicalName);
            if(entityReference.KeyAttributes?.Any() == true)
            {
                var keys = entityReference.KeyAttributes.Select( s => $"{s.Key}='{s.Value.ToString().Replace("'", "''")}'");
                return $"{entitySetName}({string.Join("&",keys)})";
            }

            return $"/{entitySetName}{entityReference.Id.ToString("P")}";            
        }

        public static JObject ActivityPartyToJObject(ActivityParty activityParty, WebApiMetadata webApiMetadata)
        {
            var jObject = new JObject();
            jObject["participationtypemask"] = (int) activityParty.ParticipationTypeMask;
            jObject[$"partyid_{activityParty.TargetEntity.LogicalName}@odata.bind"] = EntityReferenceTostring(activityParty.TargetEntity, webApiMetadata); 
            return jObject;
        }

        public static bool IsActivityPartyEnumerable(object value)
        {
            return  value is IEnumerable<ActivityParty> ||
                    value is List<ActivityParty> ||
                    value is ActivityParty[];
        }

        public static IEnumerable<ActivityParty> GetActivityPartyCollections(object value)
        {
            if(value is IEnumerable<ActivityParty>)
                return (IEnumerable<ActivityParty>) value;
            else if(value is List<ActivityParty>)
                return (IEnumerable<ActivityParty>) value;
            else if(value is ActivityParty[])
                return (ActivityParty[]) value;
            else 
                return null;
        }

        public static string GetEntityApiUrl(Entity entity, WebApiMetadata webApiMetadata)
        {
            var entitySetName = webApiMetadata.GetEntitySetName(entity.LogicalName);
            if(entity.KeyAttributes?.Any() == true)
            {
                var keys = entity.KeyAttributes.Select( s => $"{s.Key}='{s.Value.ToString().Replace("'", "''")}'");
                return $"{entitySetName}({string.Join("&",keys)})";
            }

            if(entity.Id != Guid.Empty)
                return entitySetName + entity.Id.ToString("P");
            
            return entitySetName;
        }
    }
}