using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.CognitiveSearch.Search;
using Microsoft.CognitiveSearch.Skills.Cryptonyms;
using Microsoft.CognitiveSearch.Skills.Hocr;
using Microsoft.CognitiveSearch.Skills.Image;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.CognitiveSearch.WebApiSkills
{
    public static class JfkWebApiSkills
    {
        [FunctionName("facet-graph-nodes")]
        public static IActionResult GetFacetGraphNodes([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            if (!req.QueryString.HasValue)
            {
                return new BadRequestObjectResult($"{skillName} - Requires a query string in the following format: q=oswald&f=entities");
            }

            string searchServiceName = GetAppSetting("SearchServiceName");
            string searchServiceApiKey = GetAppSetting("SearchServiceApiKey");
            string indexName = String.IsNullOrEmpty(req.Headers["IndexName"]) ? Config.AZURE_SEARCH_INDEX_NAME : (string)req.Headers["IndexName"];
            if (String.IsNullOrEmpty(searchServiceName) || String.IsNullOrEmpty(searchServiceApiKey) || String.IsNullOrEmpty(indexName))
            {
                return new BadRequestObjectResult($"{skillName} - Information for the search service is missing");
            }
            SearchClientHelper searchClient = new SearchClientHelper(searchServiceName, searchServiceApiKey, indexName);

            FacetGraphGenerator facetGraphGenerator = new FacetGraphGenerator(searchClient);
            string query = string.IsNullOrEmpty(req.Query["q"].FirstOrDefault()) ? "*" : req.Query["q"].First();
            string facet = string.IsNullOrEmpty(req.Query["f"].FirstOrDefault()) ? "entities" : req.Query["f"].First();
            JObject facetGraph = facetGraphGenerator.GetFacetGraphNodes(query, facet);

            return (ActionResult)new OkObjectResult(facetGraph);
        }

        [FunctionName("link-cryptonyms")]
        public static IActionResult RunCryptonymLinker([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            CryptonymLinker cryptonymLinker = new CryptonymLinker(executionContext.FunctionAppDirectory);
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    string word = inRecord.Data["word"] as string;
                    if (word.All(Char.IsUpper) && cryptonymLinker.Cryptonyms.TryGetValue(word, out string description))
                    {
                        outRecord.Data["cryptonym"] = new { value = word, description };
                    }
                    return outRecord;
                });

            return (ActionResult)new OkObjectResult(response);
        }

        [FunctionName("link-cryptonyms-list")]
        public static IActionResult RunCryptonymLinkerForLists([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            CryptonymLinker cryptonymLinker = new CryptonymLinker(executionContext.FunctionAppDirectory);
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    var words = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(inRecord.Data["words"]));
                    var cryptos = words.Select(jword =>
                    {
                        var word = jword.Value<string>();
                        if (word.All(Char.IsUpper) && cryptonymLinker.Cryptonyms.TryGetValue(word, out string description))
                        {
                            return new { value = word, description };
                        }
                        return null;
                    });

                    outRecord.Data["cryptonyms"] = cryptos.ToArray();
                    return outRecord;
                });

            return (ActionResult)new OkObjectResult(response);
        }

        [FunctionName("image-store")]
        public static async Task<IActionResult> RunImageStore([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null || requestRecords.Count() != 1)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array: Skill requires exactly 1 image per request.");
            }

            string blobStorageConnectionString = GetAppSetting("BlobStorageAccountConnectionString");
            string blobContainerName = String.IsNullOrEmpty(req.Headers["BlobContainerName"]) ? Config.AZURE_STORAGE_CONTAINER_NAME : (string)req.Headers["BlobContainerName"];
            if (String.IsNullOrEmpty(blobStorageConnectionString) || String.IsNullOrEmpty(blobContainerName))
            {
                return new BadRequestObjectResult($"{skillName} - Information for the blob storage account is missing");
            }
            ImageStore imageStore = new ImageStore(blobStorageConnectionString, blobContainerName);

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    string imageData = inRecord.Data["imageData"] as string;
                    string imageUri = await imageStore.UploadToBlob(imageData, Guid.NewGuid().ToString());
                    outRecord.Data["imageStoreUri"] = imageUri;
                    return outRecord;
                });

            return (ActionResult)new OkObjectResult(response);
        }

        [FunctionName("hocr-generator")]
        public static IActionResult RunHocrGenerator([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null || requestRecords.Count() != 1)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array: Skill requires exactly 1 image per request.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    List<OcrImageMetadata> imageMetadataList = JsonConvert.DeserializeObject<List<OcrImageMetadata>>(JsonConvert.SerializeObject(inRecord.Data["ocrImageMetadataList"]));
                    Dictionary<string, string> annotations = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(inRecord.Data["wordAnnotations"]))
                                                    .Where(o => o.Type != JTokenType.Null)
                                                    .GroupBy(o => o["value"].Value<string>())
                                                    .Select(g => g.First())
                                                    .ToDictionary(o => o["value"].Value<string>(), o => o["description"].Value<string>());

                    List<HocrPage> pages = new List<HocrPage>();
                    for(int i = 0; i < imageMetadataList.Count; i++)
                    {
                        pages.Add(new HocrPage(imageMetadataList[i], i, annotations));
                    }
                    HocrDocument hocrDocument = new HocrDocument(pages);
                    outRecord.Data["hocrDocument"] = hocrDocument;
                    return outRecord;
                });

            return (ActionResult)new OkObjectResult(response);
        }

        private static string GetAppSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}