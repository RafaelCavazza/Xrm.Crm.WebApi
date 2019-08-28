using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xrm.Crm.WebApi;
using Xrm.Crm.WebApi.BatchOperations;
using Xrm.Crm.WebApi.Models;
using Xunit;
using Xrm.Crm.WebApi.Request;

namespace Xrm.Crm.WebApi.Test
{
    public class RequestFormatterTest
    {           
        [Fact]
        public void ConnectToCrm(){
            var entity = new Entity("teste","numero",105);
        }   
    }
}
