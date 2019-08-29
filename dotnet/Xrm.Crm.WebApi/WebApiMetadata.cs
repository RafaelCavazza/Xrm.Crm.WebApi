using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi
{
    public class WebApiMetadata
    {
        private readonly BaseAuthorization _baseAuthorization;
        private readonly Uri _apiUrl;
        private readonly string _entityDefinitionsUrl = "EntityDefinitions?$select=LogicalName,EntitySetName,PrimaryIdAttribute,CollectionSchemaName";
        private List<EntityDefinition> entitiesDefinitions;
        
        public List<EntityDefinition> EntityDefinitions
        {
            get
            {
                if (entitiesDefinitions == null)
                {
                    LoadEntityDefinitions().GetAwaiter().GetResult();
                }

                return entitiesDefinitions;
            }
        }

        public EntityDefinition this[string name]
        {
            get
            {
                var entityDefinitons = GetEntityDefinition(name);

                if (entityDefinitons != null)
                {
                    return entityDefinitons;
                }

                LoadEntityDefinitions().GetAwaiter().GetResult();
                return GetEntityDefinition(name);
            }
        }

        public Uri ApiUrl => _apiUrl;

        public WebApiMetadata(BaseAuthorization baseAuthorization, string apiUrl)
        {
            _baseAuthorization = baseAuthorization;
            _baseAuthorization.ConfigureHttpClient();
            _apiUrl = new Uri(apiUrl);
        }

        public string GetEntitySetName(string name)
        {
            return this[name]?.EntitySetName;
        }

        public string GetLogicalName(string name)
        {
            return this[name]?.LogicalName;
        }

        public EntityDefinition GetEntityDefinition(string anyName)
        {
            return EntityDefinitions.FirstOrDefault(e =>
                    (e.LogicalName ?? "").Equals(anyName, StringComparison.OrdinalIgnoreCase) ||
                    (e.CollectionSchemaName ?? "").Equals(anyName, StringComparison.OrdinalIgnoreCase) ||
                    (e.EntitySetName ?? "").Equals(anyName, StringComparison.OrdinalIgnoreCase)
                );
        }
        public async Task LoadEntityDefinitions()
        {
            var url = _apiUrl + _entityDefinitionsUrl;
            var request = new HttpRequestMessage(new HttpMethod("GET"), url);
            var response = await _baseAuthorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
            var data = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(data);
            entitiesDefinitions = result["value"].ToObject<List<EntityDefinition>>();
        }
    }
}