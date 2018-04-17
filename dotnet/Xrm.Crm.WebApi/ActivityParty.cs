using Xrm.Crm.WebApi.Enums;

namespace Xrm.Crm.WebApi
{
    public class ActivityParty
    {
        public EntityReference TargetEntity {get; set;}
        public ParticipationTypeMask ParticipationTypeMask {get; set;}
    }
}