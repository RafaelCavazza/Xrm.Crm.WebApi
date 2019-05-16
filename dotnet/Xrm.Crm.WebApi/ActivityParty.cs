using System.Collections.Generic;
using Xrm.Crm.WebApi.Enums;

namespace Xrm.Crm.WebApi
{
    public class ActivityParty
    {
        public EntityReference TargetEntity {get; set;}
        public ParticipationTypeMask ParticipationTypeMask {get; set;}
        public Dictionary<string, object> Attributes { get; set; }

        public ActivityParty()
        {
            Attributes = new Dictionary<string, object>();
        }
    }
}