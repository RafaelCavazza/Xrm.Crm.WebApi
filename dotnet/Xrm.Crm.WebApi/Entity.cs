﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Xrm.Crm.WebApi
{
    public class Entity
    {
        public Dictionary<string, object> Attributes { get; set; }            
        public Dictionary<string, string> FormatedValues { get; set; }
        public Dictionary<string, object> KeyAttributes { get; set; }
        public string Etag { get; internal set; }
        public string LogicalName { get; set; }
        public Guid Id { get; set; }

        public Entity()
        {
            if(Attributes == null)
                Attributes = new Dictionary<string, object>();

            if(FormatedValues == null)
                FormatedValues = new Dictionary<string, string>();

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
                if (Contais(index))
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
            if (!Contais(atributeName))
                return default(T);

            if(typeof(T) == typeof(int))
                return (T) (object)Convert.ToInt32(Attributes[atributeName]);

            return (T)Attributes[atributeName];
        }

        public bool Contais(string atributeName)
        {        
            return !string.IsNullOrWhiteSpace(atributeName) && Attributes.ContainsKey(atributeName);
        }
    }
}