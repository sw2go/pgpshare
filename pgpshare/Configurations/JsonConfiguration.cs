using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

namespace SE.Minimel.Web.Configurations
{
	public static class JsonConfiguration
	{
		public static Action<JsonOptions> CreateJsonOptions()
		{
			return new Action<JsonOptions>(options =>
			{
				options.JsonSerializerOptions.IgnoreNullValues = true;
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.WriteIndented = true;

				// options.JsonSerializerOptions.Converters.Add(new SystemObjectNewtonsoftCompatibleConverter()); // Workaround until json value gets serialized to object automatically

				// TODO Implement AuthorizationContractResolver
				// With Newtonsoft (old): o.SerializerSettings.ContractResolver = new AuthorizationContractResolver(serviceProvider);
			});
		}
	}
}