using System;

namespace Xrm.Crm.WebApi.Actions
{
    public class IncidentResolution
    {
        public string Subject {get;set;}
        public Guid IncidentId {get;set;}
        public int? Timespent {get;set;}
        public string Description {get;set;}

        public IncidentResolution(Guid incidentId)
        {
            IncidentId = incidentId;
        }
    }
}