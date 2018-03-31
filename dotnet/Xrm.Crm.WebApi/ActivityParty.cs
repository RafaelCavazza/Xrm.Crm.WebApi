using Xrm.Crm.WebApi.Enums;

namespace Xrm.Crm.WebApi.Core
{
    public class ActivityParty
    {
        public EntityReference TargetEntity {get; set;}
        public ParticipationTypeMask ParticipationTypeMask {get; set;}
    }
}