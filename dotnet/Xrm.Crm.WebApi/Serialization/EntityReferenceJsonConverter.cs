using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xrm.Crm.WebApi.Metadata;
using Xrm.Crm.WebApi.Models;

namespace Xrm.Crm.WebApi.Serialization
{
	public class EntityReferenceJsonConverter : JsonConverter<EntityReference>
	{
		private readonly WebApiMetadata webApiMetadata;

		public EntityReferenceJsonConverter(WebApiMetadata webApiMetadata)
		{
			this.webApiMetadata = webApiMetadata;
		}

		public override void WriteJson(JsonWriter writer, EntityReference value, JsonSerializer serializer)
		{
			string logicalName = value.LogicalName.ToLower();
			string entitySetName = webApiMetadata.GetEntitySetName(logicalName);

			string reference;
			if (value.KeyAttributes?.Any() == true)
			{
				IEnumerable<string> keys = value.KeyAttributes
					.Select(s => $"{s.Key}='{s.Value.ToString().Replace("'", "''")}'");
				reference = $"{entitySetName}({string.Join("&", keys)})";
			}
			else
			{
				reference = $"/{entitySetName}{value.Id:P}";
			}

			writer.WriteValue(reference);
		}

		public override EntityReference ReadJson(JsonReader reader, Type objectType, EntityReference existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override bool CanRead => false;
	}
}