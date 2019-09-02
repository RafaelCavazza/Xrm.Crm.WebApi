using Xrm.Crm.WebApi.Interfaces;
using Xrm.Crm.WebApi.Models;
using Xrm.Crm.WebApi.Models.Requests;

namespace Xrm.Crm.WebApi.Messages.Actions
{
	public class CloseIncidentRequest : IWebApiAction
	{
		public string RelativeUrl { get; } = "CloseIncident";

		public int Status { get; set; }
		public IncidentResolution IncidentResolution { get; set; }
	}
}
