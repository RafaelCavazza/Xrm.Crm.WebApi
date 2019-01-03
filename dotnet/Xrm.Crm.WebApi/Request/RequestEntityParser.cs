using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;

namespace Xrm.Crm.WebApi.Request
{
    public static class RequestEntityParser
    {
        public static JObject EntityToJObject(Entity entity, WebApiMetadata webApiMetadata)
        {
            var jObject = new JObject();

            foreach(var attibute in entity.Attributes)
            {
                var jToken = GetJTokenFromAttribute(attibute.Key, attibute.Value, webApiMetadata);
                jObject.Add(jToken);
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
            
            if(activityParty.TargetEntity != null)
                jObject[$"partyid_{activityParty.TargetEntity.LogicalName}@odata.bind"] = EntityReferenceTostring(activityParty.TargetEntity, webApiMetadata); 

            foreach(var attribute in activityParty.Attributes){
                jObject.Add(attribute.Key, (JToken) attribute.Value);
            }
            
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

        public static JProperty GetJTokenFromAttribute(string key, object value, WebApiMetadata webApiMetadata){

            if(value == null)
            {
                return new JProperty(key, null);
            }
            else if(value is EntityReference) 
            {
                return new JProperty(key + "@odata.bind", EntityReferenceTostring((EntityReference)value, webApiMetadata));
            }
            else if (IsActivityPartyEnumerable(value))
            {
                var activityParties = GetActivityPartyCollections(value);
                var jArray = new JArray();
                foreach(var activityParty in activityParties)
                {
                    jArray.Add(ActivityPartyToJObject(activityParty, webApiMetadata));
                }
                return new JProperty(key, jArray);
            }
            else if(value is List<Entity>)
            {
                var objects = new JArray();
                foreach(var nestedEntity in (List<Entity>) value)
                {
                    objects.Add(EntityToJObject(nestedEntity, webApiMetadata));
                }
                return new JProperty(key, objects);
            }
            else
            {
                return new JProperty(key, JValue.FromObject(value));
            }
        }
    }
}