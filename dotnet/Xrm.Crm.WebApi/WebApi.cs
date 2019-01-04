using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Enums;
using Xrm.Crm.WebApi.Response;
using Xrm.Crm.WebApi.Request;
using Xrm.Crm.WebApi.Interfaces;

namespace Xrm.Crm.WebApi {
    public partial class WebApi : IWebApi
    {
        private readonly BaseAuthorization _baseAuthorization;
        public readonly Uri ApiUrl;
        public WebApiMetadata WebApiMetadata { get; internal set; }

        public WebApi (BaseAuthorization baseAuthorization) : this (baseAuthorization, baseAuthorization.GetCrmBaseUrl () + "/api/data/v8.2/") { }

        public WebApi (BaseAuthorization baseAuthorization, string apiUrl) {
            _baseAuthorization = baseAuthorization;
            _baseAuthorization.ConfigHttpClient ();
            ApiUrl = new Uri (apiUrl);
            WebApiMetadata = new WebApiMetadata (baseAuthorization, apiUrl);
        }

        public Guid Create (Entity entity) {
            return CreateAsync (entity).GetAwaiter ().GetResult ();
        }

        public async Task<Guid> CreateAsync (Entity entity) {
            var fullUrl = ApiUrl + WebApiMetadata.GetEntitySetName (entity.LogicalName);
            var jObject = RequestEntityParser.EntityToJObject (entity, WebApiMetadata);
            var request = new HttpRequestMessage (new HttpMethod ("Post"), fullUrl) {
                Content = new StringContent (JsonConvert.SerializeObject (jObject), Encoding.UTF8, "application/json")
            };

            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);

            return GetEntityIdFromResponse (fullUrl, response);
        }

        private Guid GetEntityIdFromResponse (string fullUrl, HttpResponseMessage response) {
            var headers = response.Headers;
            var headerValue = headers.First (h => h.Key.Contains ("OData-EntityId")).Value.First ();
            return new Guid( headerValue.Split('(').Last().Split(')')[0] );
        }

        public Entity Retrieve (string entityName, Guid entityId, params string[] properties) {
            return RetrieveAsync (entityName, entityId, properties).GetAwaiter ().GetResult ();
        }

        public async Task<Entity> RetrieveAsync (string entityName, Guid entityId, params string[] properties) {
            var entityCollection = WebApiMetadata.GetEntitySetName (entityName);
            var fullUrl = ApiUrl + entityCollection + entityId.ToString ("P");

            if(properties?.Any() ?? false)
                fullUrl += "?$select=" + string.Join(",", properties);

            var request = new HttpRequestMessage (new HttpMethod ("GET"), fullUrl);
            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);

            var data = await response.Content.ReadAsStringAsync ();
            var result = JObject.Parse (data);
            var entity = ResponseAttributeFormatter.FormatEntityResponse (result);
            entity.LogicalName = entityName;
            entity.Id = entityId;
            return entity;
        }

        public RetrieveMultipleResponse RetrieveMultiple (string entityCollection, RetrieveOptions options) {
            return RetrieveMultipleAsync (entityCollection, options).GetAwaiter ().GetResult ();
        }

        public RetrieveMultipleResponse RetrieveMultiple (FetchXmlExpression fetchXml) {
            return RetrieveMultipleAsync (fetchXml).GetAwaiter ().GetResult ();
        }

        public async Task<RetrieveMultipleResponse> RetrieveMultipleAsync (FetchXmlExpression fetchXml) {
            var entityCollection = WebApiMetadata.GetEntitySetName (fetchXml.LogicalName);
            var retrieveOptions = new RetrieveOptions { FetchXml = fetchXml };
            return await RetrieveMultipleAsync (entityCollection, retrieveOptions);
        }

        public async Task<RetrieveMultipleResponse> RetrieveMultipleAsync (string entityCollection, RetrieveOptions options) {
            var fullUrl = ApiUrl + entityCollection;
            fullUrl = options.GetRetrieveUrl (new Uri (fullUrl));
            var request = new HttpRequestMessage (new HttpMethod ("GET"), fullUrl);

            foreach (var header in options.GetPreferList ())
                request.Headers.Add ("Prefer", header);

            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);

            var data = await response.Content.ReadAsStringAsync ();
            var result = JObject.Parse (data);
            var retrieveMultipleResponse = new RetrieveMultipleResponse (result);

            while (!string.IsNullOrWhiteSpace (retrieveMultipleResponse.NextLink)) {
                var nextResults = await _baseAuthorization.GetHttpCliente ().GetAsync (retrieveMultipleResponse.NextLink);
                ResponseValidator.EnsureSuccessStatusCode (nextResults);
                var nextData = await nextResults.Content.ReadAsStringAsync ();
                var nextValues = JObject.Parse (nextData);
                retrieveMultipleResponse.AddResult (nextValues);
            }

            var logicalName = WebApiMetadata.GetLogicalName (entityCollection);
            var entityDefinition = WebApiMetadata.EntitiesDefinitions.FirstOrDefault (e => e.LogicalName == logicalName);
            var primaryKey = entityDefinition?.PrimaryIdAttribute;

            foreach (var entity in retrieveMultipleResponse.Entities) {
                if (entity.Contains (primaryKey))
                    entity.Id = Guid.Parse (entity.GetAttributeValue<string> (primaryKey));

                entity.LogicalName = logicalName;
            }

            return retrieveMultipleResponse;
        }

        public void Update (Entity entity) {
            UpdateAsync (entity).GetAwaiter ().GetResult ();
        }

        public async Task UpdateAsync (Entity entity) {
            await UpsertAsync (entity, UpsertOptions.OnlyUpdate);
        }

        public void Upsert (Entity entity, UpsertOptions upsertOptions = UpsertOptions.None) {
            UpsertAsync (entity, upsertOptions).GetAwaiter ().GetResult ();
        }

        public async Task UpsertAsync (Entity entity, UpsertOptions upsertOptions = UpsertOptions.None) {
            var fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl (entity, WebApiMetadata);
            var jObject = RequestEntityParser.EntityToJObject (entity, WebApiMetadata);
            var request = new HttpRequestMessage (new HttpMethod ("PATCH"), fullUrl) {
                Content = new StringContent (JsonConvert.SerializeObject (jObject), Encoding.UTF8, "application/json")
            };

            if (upsertOptions == UpsertOptions.OnlyUpdate)
                request.Headers.Add ("If-Match", "*");

            if (upsertOptions == UpsertOptions.OnlyCreate)
                request.Headers.Add ("If-None-Match", "*");

            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
        }

        public async Task DeleteAsync (Entity entity) {
            var fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl (entity, WebApiMetadata);
            var request = new HttpRequestMessage (new HttpMethod ("Delete"), fullUrl) {
                Content = new StringContent ("{}", Encoding.UTF8, "application/json")
            };

            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
        }

        public void Delete (Entity entity) {
            DeleteAsync (entity).GetAwaiter ().GetResult ();
        }

        public void CloseIncident (IncidentResolution incidentResolution, int status) {
            CloseIncidentAsync (incidentResolution, status).GetAwaiter ().GetResult ();
        }

        public async Task CloseIncidentAsync (IncidentResolution incidentResolution, int status) {
            var fullUrl = ApiUrl + "CloseIncident";
            var jObject = new JObject ();
            var jIncidentResolution = new JObject ();
            jObject["Status"] = status;
            jObject["IncidentResolution"] = jIncidentResolution;
            jIncidentResolution["subject"] = incidentResolution.Subject;
            jIncidentResolution["incidentid@odata.bind"] = $"/incidents{incidentResolution.IncidentId.ToString("P")}";
            if(incidentResolution.Timespent != null)
                jIncidentResolution["timespent"] = incidentResolution.Timespent;
            jIncidentResolution["description"] = incidentResolution.Description;

            var request = new HttpRequestMessage (new HttpMethod ("POST"), fullUrl){
                Content = new StringContent (JsonConvert.SerializeObject (jObject), Encoding.UTF8, "application/json")
            };
        
            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
        }

        public void QualifyLead (QualifyLeadAction action) { 
            QualifyLeadAsync(action).GetAwaiter().GetResult();
        }

        public async Task<List<Entity>> QualifyLeadAsync (QualifyLeadAction action) {

            var fullUrl = $"{ApiUrl}/leads({action.LeadId.ToString("P")})/Microsoft.Dynamics.CRM.QualifyLead";
            var jObject = action.GetRequestObject(); 

            var request = new HttpRequestMessage (new HttpMethod ("POST"), fullUrl){
                Content = new StringContent (JsonConvert.SerializeObject (jObject), Encoding.UTF8, "application/json")
            };

            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
            var responseContent = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<JObject>(responseContent);
            var entities = QualifyLeadResponseFormatter.GetCreatedEntities(data);

            foreach(var entity in entities){
                var entityDefinition = WebApiMetadata.GetEntityDefinitions(entity.LogicalName);
                var primaryKey = entityDefinition?.PrimaryIdAttribute;
                if (entity.Contains (primaryKey))
                    entity.Id = Guid.Parse (entity.GetAttributeValue<string> (primaryKey));
            }

            return entities;
        }

        public string SendEmail (Guid activityId, bool issueSend, string trackingToken) { 
            return SendEmailAsync(activityId, issueSend, trackingToken).GetAwaiter().GetResult();
        }   

        public async Task<string> SendEmailAsync (Guid activityId, bool issueSend, string trackingToken) { 
            var jObject = new JObject();
            jObject["IssueSend"] = issueSend;
            if(!string.IsNullOrWhiteSpace(trackingToken) )
                jObject["TrackingToken"] = trackingToken;

            var fullUrl = $"{ApiUrl}/emails({activityId.ToString("P")})/Microsoft.Dynamics.CRM.SendEmail";
            
            var request = new HttpRequestMessage (new HttpMethod ("POST"), fullUrl){
                Content = new StringContent (JsonConvert.SerializeObject (jObject), Encoding.UTF8, "application/json")
            };

            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
            var responseContent = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(responseContent);
            return data["Subject"].ToString();
        }

        public void Merge(MergeAction mergeAction){
            MergeAsync(mergeAction).GetAwaiter().GetResult();
        }
            
        public async Task MergeAsync(MergeAction mergeAction){

            var fullUrl = $"{ApiUrl}/Merge";
            var requestObject = mergeAction.GetRequestObject(WebApiMetadata);
            var request = new HttpRequestMessage (new HttpMethod ("POST"), fullUrl){
                Content = new StringContent (JsonConvert.SerializeObject (requestObject), Encoding.UTF8, "application/json")
            };
            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
        }

        public Guid AddToQueue(Guid queueId, EntityReference entity){
            return AddToQueueAsync(queueId, entity).GetAwaiter().GetResult();
        }
        
        public async Task<Guid> AddToQueueAsync(Guid queueId, EntityReference entity){

            var fullUrl = $"{ApiUrl}/queues{queueId.ToString("P")}/Microsoft.Dynamics.CRM.AddToQueue";
            var entityDefinitions = WebApiMetadata.GetEntityDefinitions(entity.LogicalName);
            var target = new JObject();
            target[entityDefinitions.PrimaryIdAttribute] = entity.Id.ToString("D");
            target["@odata.type"] = $"Microsoft.Dynamics.CRM.{entityDefinitions.LogicalName}";
            var requestObject = new JObject();
            requestObject.Add("Target",target);

            var request = new HttpRequestMessage (new HttpMethod ("POST"), fullUrl){
                Content = new StringContent (JsonConvert.SerializeObject (requestObject), Encoding.UTF8, "application/json")
            };
            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
            var data = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            return data["QueueItemId"].ToObject<Guid>();
        }

        public void Disassociate (Entity entity, string navigationProperty){
            DisassociateAsync(entity, navigationProperty).GetAwaiter().GetResult();
        }
        
        public async Task DisassociateAsync(Entity entity, string navigationProperty){
            var fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl (entity, WebApiMetadata) + "/" + navigationProperty + "/$ref";
            var request = new HttpRequestMessage (new HttpMethod ("DELETE"), fullUrl);
            var response = await _baseAuthorization.GetHttpCliente ().SendAsync (request);
            ResponseValidator.EnsureSuccessStatusCode (response);
        }
    }
}