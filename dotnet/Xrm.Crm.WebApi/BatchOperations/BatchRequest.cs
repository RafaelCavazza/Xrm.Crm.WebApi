using System;
using System.Collections.Generic;

namespace Xrm.Crm.WebApi.BatchOperations
{
    public class BatchRequest
    {
        public Guid BatchId {get; internal set;}
        public Guid ChangeSetId {get; internal set;}
        public int ContentId { get; set;}

        public List<BaseRequest> Requests {get; internal set;}

        public BatchRequest()
        {
            BatchId = Guid.NewGuid();
            ChangeSetId = Guid.NewGuid();
            Requests = new List<BaseRequest>(); 
            ContentId = 1;
        }

        public void AddRequest(BaseRequest baseBatchRequest)
        {
            Requests.Add(baseBatchRequest);
        }

        internal string GetBatchBodyHeader()
        {
            var requestBody = $"--batch_{BatchId.ToString("N")}" + Environment.NewLine;
            requestBody += $"Content-Type: multipart/mixed;boundary=changeset_{ChangeSetId.ToString("N")}" + Environment.NewLine + Environment.NewLine;
            return requestBody;
        }

        internal string GetBatchBodyFooter()
        {
            var requestBody =$"--changeset_{ChangeSetId.ToString("N")}--" + Environment.NewLine;
            requestBody +=$"--batch_{BatchId.ToString("N")}--" + Environment.NewLine;
            return requestBody;
        }
    }
}