using Xrm.Crm.WebApi.Interfaces;
using Xrm.Crm.WebApi.Models;

namespace Xrm.Crm.WebApi.Messages.Actions
{
	public class CloseIncidentRequest : IWebApiAction
	{
		public string RelativeUrl { get; } = "CloseIncident";

		public int Status { get; set; }
		public IncidentResolution IncidentResolution { get; set; }
	}
}
