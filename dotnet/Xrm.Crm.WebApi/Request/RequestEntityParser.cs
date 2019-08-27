using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Xrm.Crm.WebApi.Request
{
    public static class RequestEntityParser
    {
        public static JObject EntityToJObject(Entity entity, WebApiMetadata webApiMetadata)
        {
            var jObject = new JObject();

            foreach(KeyValuePair<string, object> attibute in entity.Attributes)
            {
                JProperty jToken = GetJTokenFromAttribute(attibute.Key, attibute.Value, webApiMetadata);
                jObject.Add(jToken);
            }

            return jObject;
        }
        
        public static string EntityReferenceTostring(EntityReference entityReference, WebApiMetadata webApiMetadata)
        {
            string logicalName = entityReference.LogicalName.ToLower();
            string entitySetName = webApiMetadata.GetEntitySetName(logicalName);
            
			if(entityReference.KeyAttributes?.Any() == true)
            {
                IEnumerable<string> keys = entityReference.KeyAttributes.Select( s => $"{s.Key}='{s.Value.ToString().Replace("'", "''")}'");
                return $"{entitySetName}({string.Join("&",keys)})";
            }

            return $"/{entitySetName}{entityReference.Id:P}";            
        }

        public static JObject ActivityPartyToJObject(ActivityParty activityParty, WebApiMetadata webApiMetadata)
        {
            var jObject = new JObject();
            jObject["participationtypemask"] = (int) activityParty.ParticipationTypeMask;
            
            if(activityParty.TargetEntity != null)
			{
				jObject[$"partyid_{activityParty.TargetEntity.LogicalName}@odata.bind"] = EntityReferenceTostring(activityParty.TargetEntity, webApiMetadata);
			}

			foreach(KeyValuePair<string, object> attribute in activityParty.Attributes){
                jObject.Add(attribute.Key, JValue.FromObject(attribute.Value));
            }

            return jObject;
        }

        public static string GetEntityApiUrl(Entity entity, WebApiMetadata webApiMetadata)
        {
            string entitySetName = webApiMetadata.GetEntitySetName(entity.LogicalName);
            if(entity.KeyAttributes?.Any() == true)
            {
                IEnumerable<string> keys = entity.KeyAttributes.Select( s => $"{s.Key}='{s.Value.ToString().Replace("'", "''")}'");
                return $"{entitySetName}({string.Join("&",keys)})";
            }

            if(entity.Id != Guid.Empty)
			{
				return entitySetName + entity.Id.ToString("P");
			}

			return entitySetName;
        }

        public static JProperty GetJTokenFromAttribute(string key, object value, WebApiMetadata webApiMetadata){

            switch (value)
			{
				case null:
					return new JProperty(key, null);

				case EntityReference reference:
					return new JProperty(key + "@odata.bind", EntityReferenceTostring(reference, webApiMetadata));

				case IEnumerable<ActivityParty> activityParties:
				{
					var jArray = new JArray();
               
					foreach(ActivityParty activityParty in activityParties)
					{
						jArray.Add(ActivityPartyToJObject(activityParty, webApiMetadata));
					}

					return new JProperty(key, jArray);
				}

				case List<Entity> list:
				{
					var objects = new JArray();
					foreach(Entity nestedEntity in list)
					{
						objects.Add(EntityToJObject(nestedEntity, webApiMetadata));
					}
					return new JProperty(key, objects);
				}

				default:
					return new JProperty(key, JValue.FromObject(value));
			}
		}
    }
}