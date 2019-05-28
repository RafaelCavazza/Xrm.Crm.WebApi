using Xrm.Crm.WebApi.Request;
using Newtonsoft.Json;

namespace Xrm.Crm.WebApi.BatchOperations
{
    public class UpsertRequest : BaseRequest
    {
        public Entity EntityToUpdate {get; internal set;}

        public UpsertRequest(Entity entityToUpdate)
        {
            EntityToUpdate = entityToUpdate;
        }

        public void ThenUpdate(UpdateRequest nextRequest)
        {
            NextRequest = nextRequest;
        }

        public void ThenUpsert(BaseRequest nextRequest)
        {
            NextRequest = nextRequest;
        }

        public void ThenDelete(BaseRequest nextRequest)
        {
            NextRequest = nextRequest;
        }

        internal override string GetBatchString(BatchRequest batchRequest, WebApi webApi, int? lastContentId)
        {        
            var entityUrl = lastContentId != null ? "$" +lastContentId.ToString() : webApi.ApiUrl + RequestEntityParser.GetEntityApiUrl(EntityToUpdate, webApi.WebApiMetadata);
            var jObject = RequestEntityParser.EntityToJObject(EntityToUpdate, webApi.WebApiMetadata);
            var entityString = JsonConvert.SerializeObject(jObject);

            var batchString = $"--changeset_{batchRequest.ChangeSetId.ToString("N")}" + NewLine;
            batchString += $"Content-Type: application/http" + NewLine;
            batchString += $"Content-Transfer-Encoding:binary" + NewLine;
            batchString += $"Content-ID: {batchRequest.ContentId}" + NewLine + NewLine;
            batchString += $"PATCH {entityUrl} HTTP/1.1" + NewLine;            
            batchString += $"Content-Type: application/json;type=entry" + NewLine;
            batchString += entityString + NewLine + NewLine;
            return batchString;
        }

        public static implicit operator UpsertRequest(Entity toUpdate)
        {
                return new UpsertRequest(toUpdate);
        }
    }
}