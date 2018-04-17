using System;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.Request;
using Newtonsoft.Json;

namespace Xrm.Crm.WebApi.BatchOperations
{
    public class DeleteRequest : BaseRequest
    {
        public Entity EntityToDelete {get; internal set;}

        public DeleteRequest(Entity entityToDelete)
        {
            EntityToDelete = entityToDelete;
        }

        internal override string GetBatchString(BatchRequest batchRequest, WebApi webApi, int? lastContentId)
        {        
            var entityUrl = lastContentId != null ? "$"+lastContentId.ToString() : webApi.ApiUrl + RequestEntityParser.GetEntityApiUrl(EntityToDelete, webApi.WebApiMetadata);
            var jObject = "{}";
            var entityString = JsonConvert.SerializeObject(jObject);
            var batchString = $"--changeset_{batchRequest.ChangeSetId.ToString("N")}" + NewLine;
            batchString += $"Content-Type: application/http" + NewLine;
            batchString += $"Content-Transfer-Encoding:binary" + NewLine;
            batchString += $"Content-ID: {batchRequest.ContentId}" + NewLine + NewLine;
            batchString += $"DELETE {entityUrl} HTTP/1.1" + NewLine ;
            batchString += $"Content-Type: application/json;type=entry" + NewLine + NewLine;
            batchString += entityString + NewLine + NewLine;
            return batchString;
        }

        public static implicit operator DeleteRequest(Entity toDelete)
        {
                return new DeleteRequest(toDelete);
        }
    }
}