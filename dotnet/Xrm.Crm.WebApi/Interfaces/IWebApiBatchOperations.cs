using System.Collections.Generic;
using System.Threading.Tasks;
using Xrm.Crm.WebApi.BatchOperations;
using Xrm.Crm.WebApi.Response;

namespace Xrm.Crm.WebApi.Interfaces
{
    public interface IWebApiBatchOperations
    {
        BatchRequestResponse BulkCreate(List<Entity> entities);
        Task<BatchRequestResponse> BulkCreateAsync(List<Entity> entities);
        BatchRequestResponse BulkUpdate(List<Entity> entities);
        Task<BatchRequestResponse> BulkUpdateAsync(List<Entity> entities);
        BatchRequestResponse BulkUpsert(List<Entity> entities);
        Task<BatchRequestResponse> BulkUpsertAsync(List<Entity> entities);
        BatchRequestResponse BulkDelete(List<Entity> entities);
        Task<BatchRequestResponse> BulkDeleteAsync(List<Entity> entities);
        BatchRequestResponse ExecuteBatchRequest(BatchRequest batchRequest);
        Task<BatchRequestResponse> ExecuteBatchRequestAsync(BatchRequest batchRequest);
    }
}