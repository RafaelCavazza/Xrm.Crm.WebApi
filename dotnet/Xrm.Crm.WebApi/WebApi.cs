using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Reponse;
using Xrm.Crm.WebApi;
using System.Collections.Generic;
using Xrm.Crm.WebApi.Request;
using System.Net.Http.Headers;
using Xrm.Crm.WebApi.Enums;

namespace Xrm.Crm.WebApi
{
    public partial class WebApi
    {
        private readonly BaseAuthorization _baseAuthorization;
        public readonly Uri ApiUrl;
        public WebApiMetadata WebApiMetadata {get; internal set;}

        public WebApi(BaseAuthorization baseAuthorization) : this(baseAuthorization, baseAuthorization.GetCrmBaseUrl() + "/api/data/v8.2/")
        {
        }

        public WebApi(BaseAuthorization baseAuthorization, string apiUrl)
        {
            _baseAuthorization = baseAuthorization;
            _baseAuthorization.ConfigHttpClient();
            ApiUrl = new Uri(apiUrl);
            WebApiMetadata = new WebApiMetadata(baseAuthorization, apiUrl);
        }

        public Guid Create(Entity entity)
        {
            return CreateAsync(entity).GetAwaiter().GetResult();
        }

        public async Task<Guid> CreateAsync(Entity entity)
        {   
            var fullUrl = ApiUrl + WebApiMetadata.GetEntitySetName(entity.LogicalName);
            var jObject = RequestEntityParser.EntityToJObject(entity, WebApiMetadata);
            var request = new HttpRequestMessage(new HttpMethod("Post"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };

            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);

            return GetEntityIdFromResponse(fullUrl, response);
        }

        private Guid GetEntityIdFromResponse(string fullUrl, HttpResponseMessage response)
        {
            var headers = response.Headers;
            var headerValue = headers.First( h => h.Key.Contains("OData-EntityId")).Value.First();
            return new Guid(headerValue.Replace(fullUrl,"").Replace("(",String.Empty).Replace(")", String.Empty));
        }

        public Entity Retrive(string entityName, Guid entityId)
        {
            return RetriveAsync(entityName, entityId).GetAwaiter().GetResult();
        }

        public async Task<Entity> RetriveAsync(string entityName, Guid entityId)
        {
            var entityCollection = WebApiMetadata.GetEntitySetName(entityName);
            var fullUrl = ApiUrl + entityCollection + entityId.ToString("P");
            var request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);
            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);

            var data = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(data);
            var entity = ResponseAttributeFormatter.FormatEntityResponde(result);
            entity.LogicalName = entityName;
            entity.Id = entityId;
            return entity;
        }
     
        public RetriveMultipleResponse RetriveMultiple(string entityCollection, RetriveOptions options)
        {
            return RetriveMultipleAsync(entityCollection, options).GetAwaiter().GetResult();
        }

        public RetriveMultipleResponse RetriveMultiple(FetchXmlExpression fetchXml)
        {
            return RetriveMultipleAsync(fetchXml).GetAwaiter().GetResult();
        }
        
        public async Task<RetriveMultipleResponse> RetriveMultipleAsync(FetchXmlExpression fetchXml)
        {            
            var entityCollection = WebApiMetadata.GetEntitySetName(fetchXml.LogicalName);
            var retriveOptions =  new RetriveOptions { FetchXml = fetchXml };
            return await RetriveMultipleAsync(entityCollection, retriveOptions);
        }  

        public async Task<RetriveMultipleResponse> RetriveMultipleAsync(string entityCollection, RetriveOptions options)
        {
            var fullUrl = ApiUrl + entityCollection;
            fullUrl = options.GetRetriveUrl(new Uri(fullUrl));
            var request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);

            foreach(var header in options.GetPreferList())
                request.Headers.Add("Prefer", header);
                
            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);

            var data = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(data);
            var retriveMultipleResponse = new RetriveMultipleResponse(result);

            while (!string.IsNullOrWhiteSpace(retriveMultipleResponse.NextLink))
            {
                var nextResults = await _baseAuthorization.GetHttpCliente().GetAsync(retriveMultipleResponse.NextLink);
                ResponseValidator.EnsureSuccessStatusCode(nextResults);
                var nextData = await nextResults.Content.ReadAsStringAsync();
                var nextValues = JObject.Parse(nextData);
                retriveMultipleResponse.AddResult(nextValues);
            }

            var logicalName = WebApiMetadata.GetLogicalName(entityCollection);
            var entityDefinition = WebApiMetadata.EntitiesDefinitions.FirstOrDefault(e => e.LogicalName == logicalName);
            var primaryKey = entityDefinition?.PrimaryIdAttribute;

            foreach(var entity in retriveMultipleResponse.Entities)
            { 
                if(entity.Contais(primaryKey))
                    entity.Id= Guid.Parse(entity.GetAttributeValue<string>(primaryKey));

                entity.LogicalName = logicalName;
            }

            return retriveMultipleResponse;
        }

        public void  Update(Entity entity)
        {
            UpdateAsync(entity).GetAwaiter().GetResult();
        }

        public async Task  UpdateAsync(Entity entity)
        {
            await UpsertAsync(entity, UpsertOptions.OnlyUpdate);
        }

        public void  Upsert(Entity entity, UpsertOptions upsertOptions = UpsertOptions.None)
        {
            UpsertAsync(entity).GetAwaiter().GetResult();
        }

        public async Task  UpsertAsync(Entity entity, UpsertOptions upsertOptions = UpsertOptions.None)
        {
            var fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl(entity, WebApiMetadata);
            var jObject = RequestEntityParser.EntityToJObject(entity, WebApiMetadata);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };

            if(upsertOptions == UpsertOptions.OnlyUpdate)
                request.Headers.Add("If-Match","*");

            if(upsertOptions == UpsertOptions.OnlyCreate)
                request.Headers.Add("If-None-Match","*");
            
            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public async Task DeleteAsync(Entity entity)
        {
            var fullUrl = ApiUrl + WebApiMetadata.GetEntitySetName(entity.LogicalName);
            var request = new HttpRequestMessage(new HttpMethod("Delete"), fullUrl)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public void Delete(Entity entity)
        {
            DeleteAsync(entity).GetAwaiter().GetResult();
        }

        public void CloseIncident(IncidentResolution incidentResolution, int status)
        {
            CloseIncidentAsync(incidentResolution, status).GetAwaiter().GetResult();
        }

        public async Task CloseIncidentAsync(IncidentResolution incidentResolution, int status)
        {
            var fullUrl = ApiUrl + "CloseIncident";
            var jObject = new JObject();
            var jIncidentResolution = new JObject();
            jObject["Status"] = status;
            jObject["IncidentResolution"] = jIncidentResolution;
            jIncidentResolution["subject"] = incidentResolution.Subject;
            jIncidentResolution["incidentid@odata.bind"] = $"/incidents{incidentResolution.IncidentId.ToString("P")}";
            jIncidentResolution["timespent"] = incidentResolution.Timespent;
            jIncidentResolution["description"] = incidentResolution.Description;
                        
            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl);
            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public void QualifyLead(QualifyLeadAction action)
        {
            var fullUrl = $"{ApiUrl}/leads({action.LeadId.ToString("P")})/Microsoft.Dynamics.CRM.QualifyLead";
            var jObject = new JObject();
            jObject["CreateAccount"] = action.CreateAccount;
            jObject["CreateContact"] = action.CreateContact;
            jObject["CreateOpportunity"] = action.CreateOpportunity;
            jObject["Status"] = action.Status;
            //Bind Other values
            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl);
            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }
    }
}
