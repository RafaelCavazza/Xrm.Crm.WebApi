using System;
using System.Collections.Generic;

namespace Xrm.Crm.WebApi
{
    public class EntityReference
    {
        public string LogicalName {get; internal set;}        
        public Guid Id {get; internal set;}
        public string Name {get; set;}
        public string LookupLogicalName {get; set;}        
        public Dictionary<string, object> KeyAttributes { get; set; }
        
        public EntityReference(string logicalName, string id) : this (logicalName, new Guid(id))
        {
        }

        public EntityReference(string logicalName, Guid id)
        {
            LogicalName = logicalName.ToLower();
            Id = id;
        }

        public EntityReference(string logicalName, string keyName, object keyValue)
        {
            LogicalName = logicalName;
            KeyAttributes = new Dictionary<string, object>();
            KeyAttributes.Add(keyName, keyValue);
        }

        public EntityReference(string logicalName, Dictionary<string, object> keyAttributes)
        {
            LogicalName = logicalName;
            KeyAttributes = keyAttributes;
        }
    }
}