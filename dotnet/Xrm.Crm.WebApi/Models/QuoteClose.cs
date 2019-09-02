using Newtonsoft.Json;
using Xrm.Crm.WebApi.Serialization;

namespace Xrm.Crm.WebApi.Models
{
	public class QuoteClose
	{
		[JsonProperty(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
		public EntityReference QuoteId { get; set; }
		[JsonProperty(NamingStrategyType = typeof(LowerCaseNamingStrategy))]
		public string Subject { get; set; }
	}
}
