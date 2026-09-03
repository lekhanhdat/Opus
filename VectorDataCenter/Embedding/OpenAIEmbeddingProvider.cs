/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using System.Net.Http.Headers;
using System.Text.Json;

namespace AvePoint.RA.VectorDataCenter.Embedding
{
    public class OpenAIEmbeddingProvider : IEmbeddingProvider
    {
        public string Name => "OpenAI";
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;

        public OpenAIEmbeddingProvider(string apiKey, string endpoint)
        {
            _apiKey = apiKey;
            _endpoint = endpoint;
            _httpClient = new HttpClient();
        }

        public OpenAIEmbeddingProvider()
        {
            // TODO: Replace with your actual OpenAI API key and endpoint
            _apiKey = "sk-...";
            _endpoint = "https://api.openai.com/v1/embeddings";
            _httpClient = new HttpClient();
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            // Replace with actual OpenAI embedding API call
            var requestBody = new { input = text, model = "text-embedding-ada-002" };
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            // Parse the response to extract the embedding vector
            // This is a placeholder; update with actual response parsing
            var embedding = JsonSerializer.Deserialize<OpenAIResponse>(json)?.data?.FirstOrDefault()?.embedding;
            return embedding ?? Array.Empty<float>();
        }

        private class OpenAIResponse
        {
            public List<DataItem>? data { get; set; }
        }
        private class DataItem
        {
            public float[]? embedding { get; set; }
        }
    }
}
