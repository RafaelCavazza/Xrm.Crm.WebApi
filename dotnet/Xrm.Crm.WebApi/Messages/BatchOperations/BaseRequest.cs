using System;

namespace Xrm.Crm.WebApi.Messages.BatchOperations
{
    public abstract class BaseRequest
    {
        protected string NewLine = Environment.NewLine;
        public BaseRequest NextRequest {get; internal set;}
        internal abstract string GetBatchString(BatchRequest batchRequest, WebApi webApi, int? lastContentId);
    }
}