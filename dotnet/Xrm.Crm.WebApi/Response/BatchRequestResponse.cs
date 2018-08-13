using System.Collections.Generic;
using Xrm.Crm.WebApi;
using System;
using System.Linq;
using Xrm.Crm.WebApi.Exception;

namespace Xrm.Crm.WebApi.Response
{
    public class BatchRequestResponse
    {
        public List<Entity> Entities {get;set;}
        public string BatchResponse {get; internal set;}

        public BatchRequestResponse(string response)
        {
            Entities = new List<Entity>();
            var clearResponse = RemoveHeaders(response);
            var changesetresponses = SplitString(clearResponse,"--changesetresponse_");
            changesetresponses = changesetresponses.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            if(!changesetresponses.Any())
                throw new WebApiException($"Empty Response: {Environment.NewLine}{response}");

            foreach(var changesetresponse in changesetresponses.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                var entity = ChangesetResponseParser(changesetresponse);
                if(entity != null)
                    Entities.Add(ChangesetResponseParser(changesetresponse));
            }
        }

        private Entity ChangesetResponseParser(string changesetresponse)
        {
            var lines = SplitString(changesetresponse, Environment.NewLine);

            var httpLine = lines.First(l => l.Contains("HTTP"));
            var httpStatus = int.Parse(httpLine.Split(' ')[1]);

            if(httpStatus > 299 || httpStatus < 200)
                throw new WebApiException(changesetresponse);

            if(!lines.Any(l => l.Contains("Location:")))
                return null;

            var location = httpLine = lines.First(l => l.Contains("Location:"));
            var entity = location.Split('/').Last();
            var entityName = entity.Split('(')[0];

            var entityKey = entity.Split('(').Last().Split(')').First();

            if(Guid.TryParse(entityKey, out Guid entityId))
                return new Entity(entityName, entityId);
            
            var responseEntity = new Entity(entityName);
            var keys = entityKey.Split(',');
            foreach(var key in keys){
                var values = key.Split('=');
                if(values.Length != 2)
                    continue;

                var name = values[0];
                var value = values[1].Substring(1, values[1].Length -2).Replace("''", "'");
                responseEntity.KeyAttributes.Add(values[0], value);
            }
            return responseEntity;
        }

        private string RemoveHeaders(string response)
        {
            var lines = SplitString(response, Environment.NewLine);
            lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            BatchResponse = lines[0].Replace("--batchresponse_","");  
            lines[0] = String.Empty;
            lines[1] = String.Empty;
            lines[lines.Count - 2] = String.Empty;
            lines[lines.Count - 1] = String.Empty;
            lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            return string.Join(Environment.NewLine, lines);
        }

        private List<string> SplitString(string value, string splitter)
        {
            return value.Split(new [] {splitter}, StringSplitOptions.None).ToList();
        }
    }
}