using Xrm.Crm.WebApi.Interfaces;
using Xrm.Crm.WebApi.Models;

namespace Xrm.Crm.WebApi.Messages.Actions
{
	public class CloseQuoteRequest : IWebApiAction
	{
		public string RelativeUrl { get; } = "CloseQuote";

		public QuoteClose QuoteClose { get; set; }
		public int Status { get; set; }
	}
}
