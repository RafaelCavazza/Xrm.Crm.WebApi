namespace Xrm.Crm.WebApi.Exception
{
    public class WebApiException : System.Exception
    {
        public string JSON {get;set;}
        
        public WebApiException(string message) : base(message)
        {

        }
    }
}