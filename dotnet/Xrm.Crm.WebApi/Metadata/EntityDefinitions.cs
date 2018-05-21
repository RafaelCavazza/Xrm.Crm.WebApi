using System;
using System.Linq;
using System.Collections.Generic;

namespace Xrm.Crm.WebApi.Metadata
{
    public class EntityDefinitions
    {
        public Guid MetadataId { get; set;}
        public string LogicalName { get; set;}
        public string EntitySetName { get; set;}
        public string PrimaryIdAttribute { get; set;}
        public string CollectionSchemaName { get; set;}
        public List<Attribute> Attributes { get; set;}
    }
}