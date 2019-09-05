using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Models;

namespace Xrm.Crm.WebApi.Response
{
    internal static class ResponseAttributeFormatter
    {
        public static Entity FormatEntityResponse(JObject jObject)
        {
            var etag = jObject["@odata.etag"]?.ToString();
            var formatedValues = new Dictionary<string, string>();
            var attributes = jObject.ToObject<Dictionary<string, object>>();
            var newAttributes = new Dictionary<string, object>();
            var toRemove = new List<string>();

            var enttyReferenceAttributes = attributes
                .Where(a => a.Key.EndsWith("@Microsoft.Dynamics.CRM.lookuplogicalname"))
                .ToList();

            foreach (var entyReferenceAttribute in enttyReferenceAttributes)
            {
                var key = entyReferenceAttribute.Key.Split('@')[0];
                var complements = attributes.Where(a => a.Key.Contains(key)).ToList();
                var logicalname = entyReferenceAttribute.Value.ToString();
                var id = complements.First(a => a.Key == key).Value;

                var entityReference = new EntityReference(logicalname, id.ToString());
                entityReference.Name = complements.FirstOrDefault(c => c.Key.Contains("FormattedValue")).Value?.ToString();
                entityReference.LookupLogicalName = complements.FirstOrDefault(c => c.Key.Contains("lookuplogicalname")).Value?.ToString();

                var attributeName = FormatAttributeName(key);
                newAttributes.Add(attributeName, entityReference);

                foreach (var attribute in complements)
                    if (!attribute.Key.Contains("FormattedValue") && !toRemove.Contains(attribute.Key))
                        toRemove.Add(attribute.Key);
            }

            foreach (var att in toRemove)
                attributes.Remove(att);

            var nestedEntities = new Dictionary<string, List<Entity>>();
            foreach (var attribute in attributes)
            {
                if (attribute.Value is JArray)
                {
                    var entities = new List<Entity>();
                    foreach (var nestedEntity in (JArray)attribute.Value)
                    {
                        try
                        {
                            entities.Add(FormatEntityResponse((JObject)nestedEntity));
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    nestedEntities.Add(attribute.Key, entities);
                    continue;
                }

                if (IsFormatedValue(attribute.Key))
                {
                    AddFormatedValue(attribute.Key, attribute.Value?.ToString(), formatedValues);
                    continue;
                }

                if (!attribute.Key.Contains("_value") && !attribute.Key.Contains("_x002e_") && !attribute.Key.Contains("_x0040_"))
                    continue;

                var newName = FormatAttributeName(attribute.Key);

                if (!attributes.ContainsKey(newName) && !string.IsNullOrWhiteSpace(newName))
                    newAttributes.Add(newName, attribute.Value);
            }

            foreach (var nestedEntity in nestedEntities)
                attributes[nestedEntity.Key] = nestedEntity.Value;

            foreach (var attribute in newAttributes)
                attributes.Add(attribute.Key, attribute.Value);

            var entity = new Entity();
            entity.Attributes = attributes;
            entity.FormattedValues = formatedValues;
            return entity;
        }

        private static string FormatAttributeName(string name)
        {
            var matchValue = new Regex(@"^(_)(.+?)(_value)$").Match(name);
            var newName = name;

            if (matchValue.Success && matchValue.Groups.Count > 2)
                newName = matchValue.Groups[2].Value;

            newName = newName.Replace("_x002e_", ".").Replace("_x0040_", "@");

            return newName;
        }

        private static void AddFormatedValue(string name, string value, Dictionary<string, string> formatedValues)
        {
            var newName = name.Replace("@OData.Community.Display.V1.FormattedValue", "");
            newName = FormatAttributeName(newName);
            if (!formatedValues.ContainsKey(newName) && !string.IsNullOrWhiteSpace(newName))
                formatedValues.Add(newName, value);
        }

        private static bool IsFormatedValue(string name)
        {
            return (name ?? "").Contains("@OData.Community.Display.V1.FormattedValue");
        }
    }
}