using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Exceptions;

namespace Xrm.Crm.WebApi.Response
{
    internal class ResponseValidator
    {
        internal static void EnsureSuccessStatusCode(HttpResponseMessage response,string jsonData = null)
        {
            if (response.IsSuccessStatusCode)
                return;

            var message = String.Empty;            
            var errorData = response.Content.ReadAsStringAsync().Result;

            if (response.Content.Headers.ContentType.MediaType.Equals("text/plain"))
                message = errorData;
            else if (response.Content.Headers.ContentType.MediaType.Equals("application/json"))
                message = GetErroMesssage(errorData);
            else if (response.Content.Headers.ContentType.MediaType.Equals("text/html"))
                message = $"HTML Error Content: {Environment.NewLine}{Environment.NewLine} {errorData}";
            else
                message = $"Error occurred and no handler is available for content in the {response.Content.Headers.ContentType.MediaType} format.";

            throw new WebApiException(message) { JSON = jsonData };
        }

        private static string GetErroMesssage(string errorData){
            var jcontent = (JObject)JsonConvert.DeserializeObject(errorData);
            IDictionary<string, JToken> d = jcontent;
                
            if (d.ContainsKey("error"))
            {
                JObject error = (JObject)jcontent.Property("error").Value;
                return (String)error.Property("message").Value;
            }
            else if (d.ContainsKey("Message"))
                return (String)jcontent.Property("Message").Value;

            return errorData;
        }
    }
}