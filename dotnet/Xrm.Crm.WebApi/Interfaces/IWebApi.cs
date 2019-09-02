using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xrm.Crm.WebApi.Authorization;
using Xrm.Crm.WebApi.Messages.Actions;
using Xrm.Crm.WebApi.Models;
using Xrm.Crm.WebApi.Models.Enums;
using Xrm.Crm.WebApi.Request;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi.Interfaces
{
    public interface IWebApi : IWebApiBatchOperations
    {
        BaseAuthorization Authorization {get;}
        Guid Create (Entity entity);
        Task<Guid> CreateAsync (Entity entity);

        Entity Retrieve (string entityName, Guid entityId, params string[] properties);
        Task<Entity> RetrieveAsync (string entityName, Guid entityId, params string[] properties);
        RetrieveMultipleResponse RetrieveMultiple (FetchXmlExpression fetchXml);
        Task<RetrieveMultipleResponse> RetrieveMultipleAsync (FetchXmlExpression fetchXml);
        RetrieveMultipleResponse RetrieveMultiple (string entityCollection, RetrieveOptions options);
        Task<RetrieveMultipleResponse> RetrieveMultipleAsync (string entityCollection, RetrieveOptions options);
       
        void Update (Entity entity);
        Task UpdateAsync (Entity entity);
        void Upsert (Entity entity, UpsertOptions upsertOptions = UpsertOptions.None);
        Task UpsertAsync (Entity entity, UpsertOptions upsertOptions = UpsertOptions.None);
        
        void Delete (Entity entity);
        Task DeleteAsync (Entity entity);

        void Execute(IWebApiAction action);
        Task ExecuteAsync(IWebApiAction action);
        
        void CloseIncident (IncidentResolution incidentResolution, int status);
        Task CloseIncidentAsync (IncidentResolution incidentResolution, int status);
        void QualifyLead (QualifyLeadRequest action);
        Task<List<Entity>> QualifyLeadAsync (QualifyLeadRequest action);
        string SendEmail (Guid activityId, bool issueSend, string trackingToken);
        Task<string> SendEmailAsync (Guid activityId, bool issueSend, string trackingToken);
        void Merge(MergeRequest mergeRequest);
        Task MergeAsync(MergeRequest mergeRequest);

        Guid AddToQueue(Guid queueId, EntityReference entity);
        Task<Guid> AddToQueueAsync(Guid queueId, EntityReference entity);
        void Disassociate (Entity entity, string navigationProperty);
        Task DisassociateAsync(Entity entity, string navigationProperty);
        void AddListMembersList (Guid listId, List<Entity> members);
        Task AddListMembersListAsync(Guid listId, List<Entity> members);
    }
}