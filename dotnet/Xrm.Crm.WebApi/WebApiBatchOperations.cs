using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xrm.Crm.WebApi.BatchOperations;
using Xrm.Crm.WebApi.Core;
using Xrm.Crm.WebApi.Core.Enums;
using Xrm.Crm.WebApi.Reponse;
using Xrm.Crm.WebApi.Request;
using System.Linq;

namespace Xrm.Crm.WebApi
{
    public partial class WebApi
    {
        public BatchRequestResponse BulkCreate(List<Entity> entities)
        {
            return BulkCreateAsync(entities).GetAwaiter().GetResult();
        }

        public async Task<BatchRequestResponse> BulkCreateAsync(List<Entity> entities)
        {
            var batchRequest = new BatchRequest();
            foreach(var entity in entities)
                batchRequest.AddRequest(new CreateRequest(entity));

            return await ExecuteBatchRequestAsync(batchRequest);
        }

        public BatchRequestResponse BulkUpdate(List<Entity> entities)
        {
            
            return BulkUpdateAsync(entities).GetAwaiter().GetResult();
        }

        public async Task<BatchRequestResponse> BulkUpdateAsync(List<Entity> entities)
        {
            var batchRequest = new BatchRequest();
            foreach(var entity in entities)
                batchRequest.AddRequest(new UpdateRequest(entity));
                
            return await ExecuteBatchRequestAsync(batchRequest);
        }

        public BatchRequestResponse BulkUpsert(List<Entity> entities)
        {
            return BulkUpsertAsync(entities).GetAwaiter().GetResult();
        }

        public async Task<BatchRequestResponse> BulkUpsertAsync(List<Entity> entities)
        {
            var batchRequest = new BatchRequest();
            foreach(var entity in entities)
                batchRequest.AddRequest(new UpsertRequest(entity));
                
            return await ExecuteBatchRequestAsync(batchRequest);
        }

        public BatchRequestResponse BulkDelete(List<Entity> entities)
        {
            return BulkDeleteAsync(entities).GetAwaiter().GetResult();
        }

        public async Task<BatchRequestResponse> BulkDeleteAsync(List<Entity> entities)
        {
            var batchRequest = new BatchRequest();
            foreach(var entity in entities)
                batchRequest.AddRequest(new DeleteRequest(entity));
                
            return await ExecuteBatchRequestAsync(batchRequest);
        }

        private void SetRequestContent(HttpRequestMessage request, string requestBody, Guid batchId )
        {
            request.Content = new StringContent(requestBody, Encoding.UTF8);
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.TryAddWithoutValidation("Content-Type",$"multipart/mixed;boundary=batch_{batchId.ToString("N")}");    
        }

        public BatchRequestResponse ExecuteBatchRequest(BatchRequest batchRequest) 
        {
            return ExecuteBatchRequestAsync(batchRequest).GetAwaiter().GetResult();
        }

        public async Task<BatchRequestResponse> ExecuteBatchRequestAsync(BatchRequest batchRequest) 
        {
            var requestBody = BatchRequestParser.GetRequestString(batchRequest, this);
            
            var request = new HttpRequestMessage(new HttpMethod("POST"),  ApiUrl + "$batch");
            SetRequestContent(request, requestBody, batchRequest.BatchId);

            var response = await _baseAuthorization.GetHttpCliente().SendAsync(request);
            ResponseValidator.EnsureSuccessStatusCode(response);
            var data = await response.Content.ReadAsStringAsync(); 

            var batchRequestResponse = new BatchRequestResponse(data);
            foreach(var entity in batchRequestResponse.Entities)
                entity.LogicalName = WebApiMetadata.GetLogicalName(entity.LogicalName);
              
            return batchRequestResponse;
        }
    }
}