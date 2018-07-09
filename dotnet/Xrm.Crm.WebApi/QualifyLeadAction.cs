using System;

namespace Xrm.Crm.WebApi
{
    public class QualifyLeadAction
    {
        public bool CreateAccount {get;set;}
        public bool CreateContact {get;set;}
        public bool CreateOpportunity {get;set;}
        public Guid? OpportunityCurrencyId {get;set;}
        public Guid? OpportunityCustomerId {get;set;}
        public Guid? SourceCampaignId {get;set;}
        public Guid? ProcessInstanceId {get;set;}
        public int Status {get;set;}

        public Guid LeadId {get;set;}

        public QualifyLeadAction(Guid leadId, int status){
            LeadId = leadId;
            Status = status;
        }
    }
}