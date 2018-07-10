using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Exception;
using Xrm.Crm.WebApi.Response;
namespace Xrm.Crm.WebApi.Metadata
{
    public class WebApiMetadata
    {
        private readonly BaseAuthorization _baseAuthorization;
        private readonly Uri _apiUrl;
        private readonly string _entityDefinitionsUrl = "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute,CollectionSchemaName";
        private readonly string _relationshipDefinitions = "RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata?$select=SchemaName,ReferencedAttribute,ReferencedEntity,ReferencingAttribute,ReferencingEntity,ReferencedEntityNavigationPropertyName,ReferencingEntityNavigationPropertyName";
        private readonly string _attributeMetadata = " &$expand=Attributes($select=SchemaName)";
                
        private List<EntityDefinitions> entitiesDefinitions {get; set;}
        public List<EntityDefinitions> EntitiesDefinitions 
        {
            get
            {
                if(entitiesDefinitions == null)
                    SetEntityDefinitions().GetAwaiter().GetResult();
                
                return entitiesDefinitions;
            }
        }

        private List<RelationshipDefinitions> relationshipDefinitions {get; set;}
        public List<RelationshipDefinitions> RelationshipDefinitions 
        {
            get
            {
                if(relationshipDefinitions == null)
                    SetRelationshipDefinitions().GetAwaiter().GetResult();
                
                return relationshipDefinitions;
            }
        }

        public bool LoadAttributes {get; set;}
        public bool LoadRelationshipDefinitions {get; set;}

        public EntityDefinitions this[string name]
        {
            get
            {
                var entityDefinitons = GetEntityDefinitions(name);

                if(entityDefinitons!=null)
                    return entityDefinitons;
                    
                SetEntityDefinitions().GetAwaiter().GetResult();
                return GetEntityDefinitions(name);
            }
        }

        public Uri ApiUrl => _apiUrl;

        public WebApiMetadata(BaseAuthorization baseAuthorization, string apiUrl)
        {
            _baseAuthorization = baseAuthorization;
            _baseAuthorization.ConfigHttpClient();
            _apiUrl = new Uri(apiUrl);
        }

        public async Task SetEntityDefinitions()
        {
            var url = _apiUrl + _entityDefinitionsUrl;

            if(LoadAttributes)
                url += _attributeMetadata;

            var request = new HttpRequestMessage(new HttpMethod("GET"), url);
            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
            var data = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(data);
            entitiesDefinitions = result["value"].ToObject<List<EntityDefinitions>>();
        }

        public string GetEntitySetName(string name)
        {
            return this[name]?.EntitySetName;
        }

        public string GetLogicalName(string name)
        {
            return this[name]?.LogicalName;
        }

        public EntityDefinitions GetEntityDefinitions(string anyName)
        {
            return EntitiesDefinitions.FirstOrDefault(e => 
                    (e.LogicalName?.ToLower()??"").Equals(anyName.ToLower()) ||
                    (e.CollectionSchemaName?.ToLower()??"").Equals(anyName.ToLower()) ||
                    (e.EntitySetName?.ToLower()??"").Equals(anyName.ToLower())
                );
        }

        public RelationshipDefinitions GetRelationshipDefinitions(string referencingEntity , string referencingAttribute, string referencedEntity){              
            var relationship = RelationshipDefinitions.FirstOrDefault(r => 
                    r.ReferencingEntity.ToLower()  == referencingEntity.ToLower() && 
                    r.ReferencingAttribute.ToLower() == referencingAttribute &&
                    r.ReferencedEntity.ToLower() == referencedEntity.ToLower()
                );

            return relationship;
        }

        private async Task SetRelationshipDefinitions(){
            if(!LoadRelationshipDefinitions)
                throw new WebApiException("Can't load RelationshipDefinitions the 'LoadRelationshipDefinitions' property is set to false.");

            var url = _apiUrl + _relationshipDefinitions;
            using(var request = new HttpRequestMessage(new HttpMethod("GET"), url)){
                var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
                ResponseValidator.EnsureSuccessStatusCode(response);
                var data = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(data);
                relationshipDefinitions = result["value"].ToObject<List<RelationshipDefinitions>>();
            }  
        }
    }
}