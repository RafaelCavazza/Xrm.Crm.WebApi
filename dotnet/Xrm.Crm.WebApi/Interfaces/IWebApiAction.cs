using Newtonsoft.Json;

namespace Xrm.Crm.WebApi.Interfaces
{
	/// <summary>
	/// Marker Interface
	/// </summary>
	public interface IWebApiAction
	{
		[JsonIgnore]
		string RelativeUrl { get; }
	}
}
