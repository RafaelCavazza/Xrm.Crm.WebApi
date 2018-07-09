using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using System;

namespace Xrm.Crm.WebApi.Reponse
{
    internal static class ResponseAttributeFormatter
    {
        public static Entity FormatEntityResponse(JObject jObject)
        {
            var etag = jObject["@odata.etag"]?.ToString();
            var formatedValues = new Dictionary<string, string>();
            var attributes = jObject.ToObject<Dictionary<string, object>>();
            var newAttributes = new Dictionary<string, object>(); 


            var enttyReferenceAttributes = attributes
                .Where(a => a.Key.StartsWith("_") && a.Key.EndsWith("_value"))
                .ToList();

            foreach(var enttyReferenceAttribute in enttyReferenceAttributes)
            {
                var complements = attributes.Where(a => a.Key.Contains(enttyReferenceAttribute.Key)).ToList();
                if(!complements.Any(k => k.Key.Contains("lookuplogicalname")))
                    continue;
                
                var logicalname = complements.First(a => a.Key.Contains("lookuplogicalname")).Value.ToString();
                var entityReference = new EntityReference(logicalname,enttyReferenceAttribute.Value.ToString());
                entityReference.Name = complements.FirstOrDefault(c=> c.Key.Contains("FormattedValue")).Value?.ToString();

                var attributeName = FormatAttributeName(enttyReferenceAttribute.Key);
                newAttributes.Add(attributeName, entityReference);

                foreach (var attribute in complements)
                    if(!attribute.Key.Contains("FormattedValue"))
                        attributes.Remove(attribute.Key);
            }

            foreach (var atribute in attributes)
            {
                if (IsFormatedValue(atribute.Key))
                {
                    AddFormatedValue(atribute.Key, atribute.Value?.ToString(), formatedValues);
                    continue;
                }

                if (!atribute.Key.Contains("_value") && !atribute.Key.Contains("_x002e_") && !atribute.Key.Contains("_x0040_"))
                    continue;
    
                var newName = FormatAttributeName(atribute.Key);

                if (!attributes.ContainsKey(newName) && !string.IsNullOrWhiteSpace(newName))
                    newAttributes.Add(newName, atribute.Value);
            }

            foreach (var attribute in newAttributes)
                attributes.Add(attribute.Key,attribute.Value);
            
            var entity = new Entity();
            entity.Attributes = attributes; 
            entity.FormatedValues = formatedValues;
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
            if(string.IsNullOrWhiteSpace(name))
                return false;

            return name.Contains("@OData.Community.Display.V1.FormattedValue");
        }
    }
}