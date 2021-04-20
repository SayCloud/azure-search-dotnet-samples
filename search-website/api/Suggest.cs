using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System;
using Azure;
using System.Collections.Generic;
using System.Linq;

namespace FunctionApp1_Dina_03262021_search_test
{
    public static class Suggest
    {

        private static string searchApiKey = Environment.GetEnvironmentVariable("SearchApiKey", EnvironmentVariableTarget.Process);
        private static string searchServiceName = Environment.GetEnvironmentVariable("SearchServiceName", EnvironmentVariableTarget.Process);
        private static string searchIndexName = Environment.GetEnvironmentVariable("SearchIndexName", EnvironmentVariableTarget.Process) ?? "good-books";


        [FunctionName("suggest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)] HttpRequest req,
            ILogger log)
        {

            // Get Document Id
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<RequestBodySuggest>(requestBody);

            // Cognitive Search 
            Uri serviceEndpoint = new Uri($"https://{searchServiceName}.search.windows.net/");

            SearchClient searchClient = new SearchClient(
                serviceEndpoint,
                searchIndexName,
                new AzureKeyCredential(searchApiKey)
            );

            AutocompleteOptions options = new AutocompleteOptions()
            {
                Size = data.Size
            };

            var autoCompleteResponse = await searchClient.AutocompleteAsync(data.SearchText, data.SuggesterName, options);
            var response = new Dictionary<string, List<AutocompleteItem>>();
            response["suggestions"] = autoCompleteResponse.Value.Results.ToList();

            return new OkObjectResult(response);
        }
    }
}

