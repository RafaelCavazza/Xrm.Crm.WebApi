using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Response;
using Xrm.Crm.WebApi.Request;
using Xrm.Crm.WebApi.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using Xrm.Crm.WebApi.Exceptions;
using Xrm.Crm.WebApi.Messages.Actions;
using Xrm.Crm.WebApi.Models;
using Xrm.Crm.WebApi.Models.Enums;
using Xrm.Crm.WebApi.Models.Requests;
using Xrm.Crm.WebApi.Serialization;
using Xrm.Crm.WebApi.Metadata;

namespace Xrm.Crm.WebApi
{
    public partial class WebApi : IWebApi
    {
        public Uri ApiUrl { get; }
        public WebApiMetadata WebApiMetadata { get; internal set; }
        
        public BaseAuthorization Authorization { get; }


        public WebApi(BaseAuthorization authorization) 
            : this(authorization, authorization.GetCrmBaseUrl().TrimEnd('/') + "/api/data/v8.2/") 
        { }

        public WebApi(BaseAuthorization authorization, string apiUrl)
        {
            Authorization = authorization;
            Authorization.ConfigureHttpClient();

            ApiUrl = new Uri(apiUrl);
            WebApiMetadata = new WebApiMetadata(authorization, apiUrl);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new WebApiContractResolver(),
                Converters = new List<JsonConverter> { new EntityReferenceJsonConverter(WebApiMetadata) }
            };
        }

        public Guid Create(Entity entity)
        {
            return CreateAsync(entity).GetAwaiter().GetResult();
        }

        public async Task<Guid> CreateAsync(Entity entity)
        {
            string fullUrl = ApiUrl + WebApiMetadata.GetEntitySetName(entity.LogicalName);
            JObject jObject = RequestEntityParser.EntityToJObject(entity, WebApiMetadata);
            var request = new HttpRequestMessage(new HttpMethod("Post"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);

            return GetEntityIdFromResponse(fullUrl, response);
        }

        private Guid GetEntityIdFromResponse(string fullUrl, HttpResponseMessage response)
        {
            HttpResponseHeaders headers = response.Headers;
            string headerValue = headers.First(h => h.Key.Contains("OData-EntityId")).Value.First();
            return new Guid(headerValue.Split('(').Last().Split(')')[0]);
        }

        public Entity Retrieve(string entityName, Guid entityId, params string[] properties)
        {
            return RetrieveAsync(entityName, entityId, properties).GetAwaiter().GetResult();
        }

        public async Task<Entity> RetrieveAsync(string entityName, Guid entityId, params string[] properties)
        {
            string entityCollection = WebApiMetadata.GetEntitySetName(entityName);
            string fullUrl = ApiUrl + entityCollection + entityId.ToString("P");

            if (properties?.Any() ?? false)
            {
                fullUrl += "?$select=" + string.Join(",", properties);
            }

            var request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);
            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);

            string data = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(data);
            Entity entity = ResponseAttributeFormatter.FormatEntityResponse(result);
            entity.LogicalName = entityName;
            entity.Id = entityId;
            return entity;
        }

        public RetrieveMultipleResponse RetrieveMultiple(FetchXmlExpression fetchXml)
        {
            return RetrieveMultipleAsync(fetchXml).GetAwaiter().GetResult();
        }

        public async Task<RetrieveMultipleResponse> RetrieveMultipleAsync(FetchXmlExpression fetchXml)
        {
            string entityCollection = WebApiMetadata.GetEntitySetName(fetchXml.LogicalName);
            var retrieveOptions = new RetrieveOptions { FetchXml = fetchXml };
            return await RetrieveMultipleAsync(entityCollection, retrieveOptions);
        }

        public RetrieveMultipleResponse RetrieveMultiple(string entityCollection, RetrieveOptions options)
        {
            return RetrieveMultipleAsync(entityCollection, options).GetAwaiter().GetResult();
        }

        public async Task<RetrieveMultipleResponse> RetrieveMultipleAsync(string entityCollection, RetrieveOptions options)
        {
            string fullUrl = ApiUrl + entityCollection;
            fullUrl = options.GetRetrieveUrl(new Uri(fullUrl));
            var request = new HttpRequestMessage(new HttpMethod("GET"), fullUrl);

            foreach (string header in options.GetPreferList())
            {
                request.Headers.Add("Prefer", header);
            }

            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);

            string data = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(data);
            var retrieveMultipleResponse = new RetrieveMultipleResponse(result);

            while (!string.IsNullOrWhiteSpace(retrieveMultipleResponse.NextLink))
            {
                HttpResponseMessage nextResults = await Authorization.GetHttpClient().GetAsync(retrieveMultipleResponse.NextLink);
                ResponseValidator.EnsureSuccessStatusCode(nextResults);
                string nextData = await nextResults.Content.ReadAsStringAsync();
                JObject nextValues = JObject.Parse(nextData);
                retrieveMultipleResponse.AddResult(nextValues);
            }

            string logicalName = WebApiMetadata.GetLogicalName(entityCollection);
            EntityDefinition entityDefinition = WebApiMetadata.EntityDefinitions.FirstOrDefault(e => e.LogicalName == logicalName);
            string primaryKey = entityDefinition?.PrimaryIdAttribute;

            foreach (Entity entity in retrieveMultipleResponse.Entities)
            {
                if (entity.Contains(primaryKey))
                {
                    entity.Id = Guid.Parse(entity.GetAttributeValue<string>(primaryKey));
                }

                entity.LogicalName = logicalName;
            }

            return retrieveMultipleResponse;
        }

        public void Update(Entity entity)
        {
            UpdateAsync(entity).GetAwaiter().GetResult();
        }

        public async Task UpdateAsync(Entity entity)
        {
            await UpsertAsync(entity, UpsertOptions.OnlyUpdate);
        }

        public void Upsert(Entity entity, UpsertOptions upsertOptions = UpsertOptions.None)
        {
            UpsertAsync(entity, upsertOptions).GetAwaiter().GetResult();
        }

        public async Task UpsertAsync(Entity entity, UpsertOptions upsertOptions = UpsertOptions.None)
        {
            string fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl(entity, WebApiMetadata);
            JObject jObject = RequestEntityParser.EntityToJObject(entity, WebApiMetadata);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };

            if (upsertOptions == UpsertOptions.OnlyUpdate)
            {
                request.Headers.Add("If-Match", "*");
            }

            if (upsertOptions == UpsertOptions.OnlyCreate)
            {
                request.Headers.Add("If-None-Match", "*");
            }

            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public void Delete(Entity entity)
        {
            DeleteAsync(entity).GetAwaiter().GetResult();
        }

        public async Task DeleteAsync(Entity entity)
        {
            string fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl(entity, WebApiMetadata);
            var request = new HttpRequestMessage(new HttpMethod("Delete"), fullUrl)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public void Execute(IWebApiAction action)
        {
            ExecuteAsync(action).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(IWebApiAction action)
        {
            string json = JsonConvert.SerializeObject(action);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string fullUrl = ApiUrl + action.RelativeUrl;

            HttpResponseMessage response = await Authorization.GetHttpClient().PostAsync(fullUrl, content);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public void QualifyLead(QualifyLeadRequest action)
        {
            QualifyLeadAsync(action).GetAwaiter().GetResult();
        }

        public async Task<List<Entity>> QualifyLeadAsync(QualifyLeadRequest action)
        {
            string fullUrl = $"{ApiUrl}/leads({action.LeadId:P})/Microsoft.Dynamics.CRM.QualifyLead";
            JObject jObject = action.GetRequestObject();

            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<JObject>(responseContent);
            List<Entity> entities = QualifyLeadResponseFormatter.GetCreatedEntities(data);

            foreach (Entity entity in entities)
            {
                EntityDefinition entityDefinition = WebApiMetadata.GetEntityDefinition(entity.LogicalName);
                string primaryKey = entityDefinition?.PrimaryIdAttribute;
                if (entity.Contains(primaryKey))
                {
                    entity.Id = Guid.Parse(entity.GetAttributeValue<string>(primaryKey));
                }
            }

            return entities;
        }

        public string SendEmail(Guid activityId, bool issueSend, string trackingToken)
        {
            return SendEmailAsync(activityId, issueSend, trackingToken).GetAwaiter().GetResult();
        }

        public async Task<string> SendEmailAsync(Guid activityId, bool issueSend, string trackingToken)
        {
            var jObject = new JObject();
            jObject["IssueSend"] = issueSend;
            if (!string.IsNullOrWhiteSpace(trackingToken))
            {
                jObject["TrackingToken"] = trackingToken;
            }

            string fullUrl = $"{ApiUrl}/emails({activityId:P})/Microsoft.Dynamics.CRM.SendEmail";

            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(responseContent);
            return data["Subject"].ToString();
        }

        public void Merge(MergeRequest mergeRequest)
        {
            MergeAsync(mergeRequest).GetAwaiter().GetResult();
        }

        public async Task MergeAsync(MergeRequest mergeRequest)
        {

            string fullUrl = $"{ApiUrl}/Merge";
            JObject requestObject = mergeRequest.GetRequestObject(WebApiMetadata);
            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public Guid AddToQueue(Guid queueId, EntityReference entity)
        {
            return AddToQueueAsync(queueId, entity).GetAwaiter().GetResult();
        }

        public async Task<Guid> AddToQueueAsync(Guid queueId, EntityReference entity)
        {

            string fullUrl = $"{ApiUrl}/queues{queueId:P}/Microsoft.Dynamics.CRM.AddToQueue";
            EntityDefinition entityDefinitions = WebApiMetadata.GetEntityDefinition(entity.LogicalName);
            var target = new JObject();
            target[entityDefinitions.PrimaryIdAttribute] = entity.Id.ToString("D");
            target["@odata.type"] = $"Microsoft.Dynamics.CRM.{entityDefinitions.LogicalName}";
            var requestObject = new JObject();
            requestObject.Add("Target", target);

            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
            var data = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            return data["QueueItemId"].ToObject<Guid>();
        }

        public void Disassociate(Entity entity, string navigationProperty)
        {
            DisassociateAsync(entity, navigationProperty).GetAwaiter().GetResult();
        }

        public async Task DisassociateAsync(Entity entity, string navigationProperty)
        {
            string fullUrl = ApiUrl + RequestEntityParser.GetEntityApiUrl(entity, WebApiMetadata) + "/" + navigationProperty + "/$ref";
            var request = new HttpRequestMessage(new HttpMethod("DELETE"), fullUrl);
            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }

        public void AddListMembersList(Guid listId, List<Entity> members)
        {
            AddListMembersListAsync(listId, members).GetAwaiter().GetResult();
        }

        public async Task AddListMembersListAsync(Guid listId, List<Entity> members)
        {
            var jObject = new JObject();
            var list = new JObject();
            var litsMembers = new JArray();
            list["listid"] = listId.ToString("D");
            list["@odata.type"] = "Microsoft.Dynamics.CRM.list";

            foreach (Entity member in members)
            {
                var jMember = new JObject();

                if (member.LogicalName.ToLower() == "contact")
                {
                    jMember["@odata.type"] = "Microsoft.Dynamics.CRM.contact";
                    jMember["contactid"] = member.Id.ToString("D");
                }
                else if (member.LogicalName.ToLower() == "account")
                {
                    jMember["@odata.type"] = "Microsoft.Dynamics.CRM.account";
                    jMember["accountid"] = member.Id.ToString("D");
                }
                else
                {
                    throw new WebApiException($"Logical name {member.LogicalName} cannot be mapped to List Members");
                }

                litsMembers.Add(jMember);
            }

            jObject["List"] = list;
            jObject["Members"] = litsMembers;

            string fullUrl = $"{ApiUrl}/AddListMembersList";
            var request = new HttpRequestMessage(new HttpMethod("POST"), fullUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(jObject), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await Authorization.GetHttpClient().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
        }
    }
}