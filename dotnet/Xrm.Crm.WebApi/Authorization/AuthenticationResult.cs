using System;
using Newtonsoft.Json;

namespace Xrm.Crm.WebApi.Authorization
{
    public class AuthenticationResult
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType {get;set;}

        [JsonProperty(PropertyName = "expires_in")]
        public int? ExpiresIn {get;set;}

        [JsonProperty(PropertyName = "ext_expires_in")]
        public int? ExtExpiresIn {get;set;}

        [JsonProperty(PropertyName = "expires_on")]
        public int? ExpiresOn {get;set;}

        [JsonProperty(PropertyName = "not_before")]
        public int? NotBefore {get;set;}

        [JsonProperty(PropertyName = "resource")]
        public string Resource {get;set;}

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken {get;set;}

        public bool IsValid(){
            var nowUnixTimeStamp = ConvertToUnixTimestamp(DateTime.Now);
            return nowUnixTimeStamp > NotBefore && nowUnixTimeStamp < ExpiresOn;
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}