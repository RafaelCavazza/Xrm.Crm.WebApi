using Newtonsoft.Json;
using Xrm.Crm.WebApi.Models;
using Xrm.Crm.WebApi.Request;

namespace Xrm.Crm.WebApi.Messages.BatchOperations
{
    public class CreateRequest : BaseRequest
    {
        public Entity EntityToCreate {get; internal set;}

        public CreateRequest(Entity entityToCreate)
        {
            EntityToCreate = entityToCreate;
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
            var entityUrl = lastContentId != null ? "$"+lastContentId.ToString() : webApi.ApiUrl + RequestEntityParser.GetEntityApiUrl(EntityToCreate, webApi.WebApiMetadata);
            var jObject = RequestEntityParser.EntityToJObject(EntityToCreate, webApi.WebApiMetadata);
            var entityString = JsonConvert.SerializeObject(jObject);

            var batchString = $"--changeset_{batchRequest.ChangeSetId.ToString("N")}" + NewLine;
            batchString += $"Content-Type: application/http" + NewLine;
            batchString += $"Content-Transfer-Encoding:binary" + NewLine;
            batchString += $"Content-ID: {batchRequest.ContentId}" + NewLine + NewLine;
            batchString += $"POST {entityUrl} HTTP/1.1" + NewLine ;
            batchString += $"Content-Type: application/json;type=entry" + NewLine + NewLine;
            batchString += entityString + NewLine + NewLine;
            return batchString;
        }

        public static implicit operator CreateRequest(Entity toCreate)
        {
                return new CreateRequest(toCreate);
        }
    }
}