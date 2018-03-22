using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi.Exception;

namespace Xrm.Crm.WebApi.Reponse
{
    internal class ResponseValidator
    {
        
        internal static void EnsureSuccessStatusCode(HttpResponseMessage response,string jsonData = null)
        {
            if (response.IsSuccessStatusCode)
                return;

            var message = String.Empty;            

            string errorData = response.Content.ReadAsStringAsync().Result;

            if (response.Content.Headers.ContentType.MediaType.Equals("text/plain"))
            {
                message = errorData;
            }
            else if (response.Content.Headers.ContentType.MediaType.Equals("application/json"))
            {
                var jcontent = (JObject)JsonConvert.DeserializeObject(errorData);
                IDictionary<string, JToken> d = jcontent;
                
                if (d.ContainsKey("error"))
                {
                    JObject error = (JObject)jcontent.Property("error").Value;
                    message = (String)error.Property("message").Value;
                }
                else if (d.ContainsKey("Message"))
                    message = (String)jcontent.Property("Message").Value;


            }
            else if (response.Content.Headers.ContentType.MediaType.Equals("text/html"))
            {
                message = "HTML Error Content:";
                message += "\n\n" + errorData;
            }
            else
            {
                message = String.Format("Error occurred and no handler is available for content in the {0} format.",
                    response.Content.Headers.ContentType.MediaType.ToString());
            }

            var exception = new WebApiException(message);

            if (jsonData != null)
                exception.JSON = jsonData;
            
            throw exception;
        }
    }
}