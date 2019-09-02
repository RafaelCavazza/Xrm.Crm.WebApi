namespace Xrm.Crm.WebApi.Models.Requests
{
	public class IncidentResolution
	{
		public EntityReference IncidentId { get; set; }

		public string Subject { get; set; }
		public string Description { get; set; }

		public int? Timespent { get; set; }

		public IncidentResolution(EntityReference incidentId)
		{
			IncidentId = incidentId;
		}
	}
}