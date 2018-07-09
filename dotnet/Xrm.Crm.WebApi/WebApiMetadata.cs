using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi
{
    public class WebApiMetadata
    {
        private readonly BaseAuthorization _baseAuthorization;
        private readonly Uri _apiUrl;
        private readonly string _entityDefinitionsUrl = "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute,CollectionSchemaName";
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
    }
}