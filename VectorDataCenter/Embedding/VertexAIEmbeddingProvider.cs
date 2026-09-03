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
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AvePoint.RA.VectorDataCenter.Embedding
{

    public class VertexAIEmbeddingProvider : IEmbeddingProvider
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(VertexAIEmbeddingProvider));
        public string Name => "VertexAI";
        private readonly string _projectId;
        private readonly string _serviceAccount;
        private readonly string _modelName;
        private readonly string _endpoint;
        private readonly ServiceAccountCredential? _serviceCredential;

        public VertexAIEmbeddingProvider()
        {
            _projectId = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_PROJECT_ID];
            _modelName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_TEXT_MODEL_NAME];  //"textembedding-gecko";
            _serviceAccount = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VERTEX_AI_SERVICE_ACCOUNT];
            _endpoint = $"https://us-central1-aiplatform.googleapis.com/v1/projects/{_projectId}/locations/us-central1/publishers/google/models/{_modelName}:predict";
            var tokenServerUrl = "https://oauth2.googleapis.com/token";
            string privateKey = LoadPrivateKey();
            var zer = new ServiceAccountCredential.Initializer(_serviceAccount, tokenServerUrl);
            zer = zer.FromPrivateKey(privateKey);
            zer.Scopes = ["https://www.googleapis.com/auth/cloud-platform"];
#if DEBUG
            zer.HttpClientFactory = new CustomHttpClientFactory();
#endif
            _serviceCredential = new ServiceAccountCredential(zer);
        }

        private string LoadPrivateKey()
        {
            return RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.VERTEX_AI_PRIVATE_KEY];
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            string token;
            if (_serviceCredential != null)
            {
                token = await _serviceCredential.GetAccessTokenForRequestAsync();
            }
            else
            {
                throw new InvalidOperationException("ServiceCredential is not initialized.");
            }

            var requestJson = $@"{{
                    'instances': [
                        {{ 'content': {JsonSerializer.Serialize(text)} }}
                    ]
                    }}".Replace('"', '\'');

            string json = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = client.PostAsync(_endpoint, content).GetAwaiter().GetResult();

                json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            // Parse the response to extract the embedding vector
            var vertexResponse = JsonConvert.DeserializeObject<VertexAIResponse>(json);

            if (vertexResponse?.Error != null)
            {
                throw new Exception($"Error from Vertex AI: {vertexResponse.Error.Message} (Code: {vertexResponse.Error.Code}, Status: {vertexResponse.Error.Status})");
            }
            var embedding = vertexResponse?.Predictions?.FirstOrDefault()?.Embeddings?.Values;
            return embedding ?? [];
        }

        #region VertexAIResponse entity classes

        private class VertexAIResponse
        {
            [JsonProperty("predictions")]
            public List<Prediction>? Predictions { get; set; }
            // Add support for error type
            [JsonProperty("error")]
            public PredictionError? Error { get; set; }
        }
        private class Prediction
        {
            [JsonProperty("embeddings")]
            public Embeddings? Embeddings { get; set; }
        }
        private class Embeddings
        {
            [JsonProperty("statistics")]
            public Statistics? Statistics { get; set; }
            [JsonProperty("values")]
            public float[]? Values { get; set; }
        }
        private class Statistics
        {
            [JsonProperty("truncated")]
            public bool Truncated { get; set; }
            [JsonProperty("token_count")]
            public int Token_count { get; set; }
        }
        private class PredictionError
        {
            [JsonProperty("code")]
            public int Code { get; set; }
            [JsonProperty("message")]
            public string? Message { get; set; }
            [JsonProperty("status")]
            public string? Status { get; set; }
        }
    }
    #endregion

    #region Custom proxy handler

    public class CustomHttpClientFactory : Google.Apis.Http.HttpClientFactory
    {
        // You need proxy file to create handler. You can comment it if you don't need config proxy to access google
        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
        {
            string developmentJson = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, "config", "Proxy.json");
            if (!File.Exists(developmentJson))
            {
                throw new ArgumentNullException("Proxy not found.");
            }
            LocalProxy? proxyConfig = null;
            using (StreamReader stream = new(developmentJson))
            {
                string proxyJson = stream.ReadToEnd();
                if (proxyJson.IsNotNullOrEmpty())
                {
                    proxyConfig = JsonConvert.DeserializeObject<LocalProxy>(proxyJson)!;
                }
                if (proxyJson.IsNullOrEmpty() || proxyConfig is null)
                {
                    throw new ArgumentNullException("Proxy not found.");
                }
            }
            WebProxy proxy = new(proxyConfig.Host, true)
            {
                Credentials = new NetworkCredential(proxyConfig.Account, proxyConfig.Password)
            };
            HttpClient.DefaultProxy = proxy;
            return base.CreateHandler(args);
        }

        public class LocalProxy
        {
            public string Host
            {
                get; set;
            }
            public string Account
            {
                get; set;
            }
            public string Password
            {
                get; set;
            }
        }
    }

    #endregion
}
