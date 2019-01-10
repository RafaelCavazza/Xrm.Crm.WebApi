using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Xrm.Crm.WebApi
{
    public class Entity
    {
        public Dictionary<string, object> Attributes { get; set; }            
        public Dictionary<string, string> FormattedValues { get; set; }
        public Dictionary<string, object> KeyAttributes { get; set; }
        public string Etag { get; internal set; }
        public string LogicalName { get; set; }
        public Guid Id { get; set; }

        public Entity()
        {
            if(Attributes == null)
                Attributes = new Dictionary<string, object>();

            if(FormattedValues == null)
                FormattedValues = new Dictionary<string, string>();

            if(KeyAttributes == null)
                KeyAttributes = new Dictionary<string, object>();
        }

        public Entity(string logicalName) : this()
        {
            LogicalName = logicalName;
        }

        public Entity(string logicalName, Guid id): this()
        {
            LogicalName = logicalName;
            Id = id;
        }

        public Entity(string logicalName, string keyName, object keyValue): this()
        {
            LogicalName = logicalName;
            KeyAttributes = new Dictionary<string, object>();
            KeyAttributes.Add(keyName, keyValue);
        }

        public Entity(string logicalName, Dictionary<string, object> keyAttributes): this()
        {
            LogicalName = logicalName;
            KeyAttributes = keyAttributes;
        }

        public object this[string index]
        {
            get
            {
                if (Contains(index))
                    return Attributes[index];
                return null;
            }
            set
            {
                Attributes[index] = value;
            }
        }

        public T GetAttributeValue<T>(string atributeName)
        {
            if (!Contains(atributeName))
                return default(T);

            if(typeof(T) == typeof(int))
                return (T) (object)Convert.ToInt32(Attributes[atributeName]);

            if(( typeof(DateTime) == typeof(T) ||  typeof(DateTime?) == typeof(T) ) && Attributes[atributeName] is string)
                return (T) (object) Convert.ToDateTime(Attributes[atributeName]);

            if(typeof(Guid) == typeof(T) && Attributes[atributeName] is string)                
                return (T) (object) new Guid(Attributes[atributeName].ToString());

            return (T)Attributes[atributeName];
        }

        public bool Contains(string atributeName)
        {        
            return !string.IsNullOrWhiteSpace(atributeName) && Attributes.ContainsKey(atributeName);
        }

        public bool ContainsValue(string atributeName)
        {        
            if(!Contains(atributeName))
                return false;
            
            return Attributes[atributeName] != null;
        }
    }
}
