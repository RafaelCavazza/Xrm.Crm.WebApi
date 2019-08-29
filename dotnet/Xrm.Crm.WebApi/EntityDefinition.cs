namespace Xrm.Crm.WebApi
{
    public class EntityDefinition
    {
        public string LogicalName { get; set; }
        public string EntitySetName { get; set; }
        public string PrimaryIdAttribute { get; set; }
        public string CollectionSchemaName { get; set; }    
    }
}