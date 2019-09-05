using Newtonsoft.Json.Serialization;

namespace Xrm.Crm.WebApi.Serialization
{
	public class LowerCaseNamingStrategy : NamingStrategy
	{
		protected override string ResolvePropertyName(string name)
		{
			return name.ToLowerInvariant();
		}
	}
}
