using Newtonsoft.Json;
using Xrm.Crm.WebApi.Serialization;

namespace Xrm.Crm.WebApi.Models.Requests
{
	public class IncidentResolution
	{
		[JsonProperty(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
		public EntityReference IncidentId { get; set; }
		
		[JsonProperty(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
		public string Subject { get; set; }
		[JsonProperty(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
		public string Description { get; set; }
		
		[JsonProperty(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
		public int? Timespent { get; set; }

		public IncidentResolution(EntityReference incidentId)
		{
			IncidentId = incidentId;
		}
	}
}