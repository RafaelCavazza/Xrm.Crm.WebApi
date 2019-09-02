namespace Xrm.Crm.WebApi.Exceptions
{
    public class WebApiException : System.Exception
    {
        public string JSON {get;set;}
        
        public WebApiException(string message) : base(message)
        {

        }
    }
}