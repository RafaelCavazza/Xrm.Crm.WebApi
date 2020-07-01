using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xrm.Crm.WebApi.Models;

namespace Xrm.Crm.WebApi.Serialization
{
	public class WebApiContractResolver : DefaultContractResolver
	{
		private const string ODataBind = "@odata.bind";

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);

			var property = member as PropertyInfo;

			if (property == null)
			{
				return jsonProperty;
			}

			if (property.PropertyType == typeof(EntityReference))
			{
				jsonProperty.PropertyName = property.DeclaringType == typeof(ActivityParty)
					? $"partyid_{jsonProperty.PropertyName}{ODataBind}"
					: jsonProperty.PropertyName + ODataBind;
			}

			return jsonProperty;
		}
	}
}
