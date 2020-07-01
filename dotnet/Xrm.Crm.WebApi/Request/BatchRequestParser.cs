using Xrm.Crm.WebApi.Messages.BatchOperations;

namespace Xrm.Crm.WebApi.Request
{
    internal class BatchRequestParser
    {
        internal static string GetRequestString(BatchRequest batchRequest, WebApi webApi)
        {
            var requestString = batchRequest.GetBatchBodyHeader();

            foreach(var request in batchRequest.Requests)
            {
                requestString += request.GetBatchString(batchRequest, webApi, null);
                var nextRequest = request.NextRequest;   
                var ContentId = batchRequest.ContentId;

                while(nextRequest!=null)                
                {   
                    batchRequest.ContentId++;
                    requestString += nextRequest.GetBatchString(batchRequest, webApi, ContentId);
                    nextRequest = nextRequest.NextRequest;
                }
                batchRequest.ContentId++;
            }

            requestString+= System.Environment.NewLine + batchRequest.GetBatchBodyFooter();
            return requestString;
        }
    }
}