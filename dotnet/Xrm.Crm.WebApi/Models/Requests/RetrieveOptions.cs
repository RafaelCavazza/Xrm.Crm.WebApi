using System;
using System.Collections.Generic;
using System.Web;
using Xrm.Crm.WebApi.Exceptions;

namespace Xrm.Crm.WebApi.Models.Requests
{
    public class RetrieveOptions
    {
        public string[] Select { get; set; }
        public string[] OrderBy { get; set; }
        public string Filter { get; set; }
        public string FetchXml { get; set; }
        public int Top { get; set; }
        public int Skip { get; set; }
        public bool IncludeCount { get; set; }
        public bool TrackChanges { get; set; }
        public string TrackChangesLink { get; set; }

        public Guid SavedQuery { get; set; }
        public Guid UserQuery { get; set; }

        public string GetRetrieveUrl(Uri apiUri)
        {
            if (apiUri == null)
                throw new WebApiException("ApiUri can't be null");

            var uriBuilder = new UriBuilder(apiUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (!string.IsNullOrWhiteSpace(Filter))
                query["$filter"] = Filter;

            if (Select?.Length > 0)
                query["$select"] = String.Join(",", Select);

            if (OrderBy?.Length > 0)
                query["$orderby"] = String.Join(",", OrderBy);

            if (Top > 0)
                query["$top"] = Top.ToString();

            if (Skip > 0)
                query["$skip"] = Skip.ToString();

            if (IncludeCount)
                query["$count"] = "true";


            if (!string.IsNullOrEmpty(TrackChangesLink))
            {

            }

            //TODO: Advanced options?
            if (SavedQuery != Guid.Empty)
            {
                query["savedQuery"] = SavedQuery.ToString();
            }
            else if (UserQuery != Guid.Empty)
            {
                query["userQuery"] = UserQuery.ToString();
            }
            else if (!string.IsNullOrWhiteSpace(FetchXml))
            {
                query["fetchXml"] = FetchXml;
            }

            //END: Advanced options?

            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        public List<string> GetPreferList()
        {
            var preferList = new List<string>();

            if (TrackChanges)
                preferList.Add("odata.track-changes");

            return preferList;
        }
    }
}
