using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.CognitiveSearch.Search
{
    public class SearchClientHelper
    {
        private static SearchServiceClient _searchClient;

        private string IndexName;
        public static string errorMessage;

        public SearchClientHelper(string serviceName, string apiKey, string indexName)
        {
            try
            {
                IndexName = indexName;
                _searchClient = new SearchServiceClient(serviceName, new SearchCredentials(apiKey));
            }
            catch (Exception e)
            {
                errorMessage = e.Message.ToString();
            }
        }
        
        public DocumentSearchResult GetFacets(string searchText, string facetName, int maxCount = 30)
        {
            // Execute search based on query string
            try
            {
                SearchParameters sp = new SearchParameters()
                {
                    SearchMode = SearchMode.Any,
                    Top = 0,
                    Select = new List<String>() { "id" },
                    Facets = new List<String>() { $"{facetName}, count:{maxCount}" },
                    QueryType = QueryType.Full
                };

                return _searchClient.Indexes.GetClient(IndexName).Documents.Search(searchText, sp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }
    }
}