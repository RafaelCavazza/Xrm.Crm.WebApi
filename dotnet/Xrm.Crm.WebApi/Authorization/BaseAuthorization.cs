﻿using System;
using System.Net.Http;

namespace Xrm.Crm.WebApi.Authorization
{
    public abstract class BaseAuthorization
    {
        public Guid CallerId { get; set; }
        public TimeSpan Timeout { get; set; }
        
        protected HttpClient httpClient;
        protected HttpClientHandler handler;

        protected BaseAuthorization()
        {
            handler = new HttpClientHandler();
            handler.UseCookies = false;
            httpClient = new HttpClient(handler);
            Timeout = new TimeSpan(0,2,0);
            CallerId = Guid.Empty;
        }
        
        public HttpClient GetHttpClient()
        {
            RefreshCredentials();
            RefreshCallerId();

            return httpClient;
        }

        public abstract void RefreshCredentials();

        private void RefreshCallerId()
        {
            if(httpClient?.DefaultRequestHeaders?.Contains("MSCRMCallerID") ?? false)
            {
                httpClient.DefaultRequestHeaders.Remove("MSCRMCallerID");
            }

            if(CallerId != Guid.Empty)
            {
                httpClient?.DefaultRequestHeaders?.Add("MSCRMCallerID", CallerId.ToString());
            }
        }

        public void ConfigureHttpClient()
        {   
            if(!httpClient.DefaultRequestHeaders.Contains("Accept"))
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            if(!httpClient.DefaultRequestHeaders.Contains("OData-MaxVersion"))
            {
                httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            }

            if(!httpClient.DefaultRequestHeaders.Contains("OData-Version"))
            {
                httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            }

            if (CallerId != Guid.Empty && !httpClient.DefaultRequestHeaders.Contains("MSCRMCallerID"))
            {
                httpClient.DefaultRequestHeaders.Add("MSCRMCallerID", CallerId.ToString());
            }

            if(!httpClient.DefaultRequestHeaders.Contains("Prefer"))
            {
                httpClient.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=\"*\"");
            }

            httpClient.Timeout = Timeout;
        }

        public abstract string GetCrmBaseUrl();
    }
}
