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
using AvePoint.RA.VectorDataCenter.Storage;
using AvePoint.RA.VectorDataCenter.Similarity;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.AI;
using AvePoint.RAI.Core.Services;
using AvePoint.RAI.Core.Utils;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Common;

namespace AvePoint.RA.VectorDataCenter.Services
{
    public class QueryService
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(QueryService));
        private readonly ITextEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly ISimilarityCalculator _similarityCalculator;

        public QueryService(ITextEmbeddingService embeddingService, IVectorStore vectorStore, ISimilarityCalculator similarityCalculator)
        {
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _similarityCalculator = similarityCalculator ?? throw new ArgumentNullException(nameof(similarityCalculator));

            _logger.Info("QueryService initialized with embedding service provider: {0}", _embeddingService.GetProvider().Name);
        }

        /// <summary>
        /// Create QueryService with RAI.Core embedding provider
        /// </summary>
        /// <param name="vectorStore">Vector storage implementation</param>
        /// <param name="similarityCalculator">Similarity calculation implementation</param>
        /// <returns>QueryService instance with RAI.Core embedding provider</returns>
        public static async Task<QueryService> CreateWithRAIProvider(IVectorStore vectorStore, ISimilarityCalculator similarityCalculator)
        {
            _logger.Info("Creating QueryService with RAI.Core provider");

            try
            {
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                _logger.Info($"Start to get embedding services (GCP Environment: {isGCP})");

                ITextEmbeddingService embeddingService = isGCP
                   ? await AiClientManager.GetVertexAIEmbeddingServiceAsync()
                   : await AiClientManager.GetAzureOpenAIEmbeddingServiceAsync();

                _logger.Info("QueryService created successfully with RAI.Core provider");
                return new QueryService(embeddingService, vectorStore, similarityCalculator);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create QueryService with RAI.Core provider: {0}", ex, ex.Message);
                throw;
            }
        }

        public async Task<List<(string id, float score)>?> QueryAsync(string text, int topK = 5)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Only limit text length if it exceeds 2000 characters
            string limitedText;
            if (text.Length > 2000)
            {
                limitedText = TextLimitingUtils.LimitTextLength(text);
                if (limitedText.Length != text.Length)
                {
                    _logger.Info("Text truncated from {0} to {1} characters for embedding query", text.Length, limitedText.Length);
                }
            }
            else
            {
                _logger.Info("Text length is within limit: {0} characters", text.Length);
                limitedText = text;
            }

            var response = await _embeddingService.GetEmbeddingsAsync(new[] { limitedText });
            var tokenUsage = response.TokensUsed;
            TokenUsageCache.Add(tokenUsage);
            var queryVector = response.Embeddings.First();

            // Updated: Expecting candidates to include score if available
            var candidates = await _vectorStore.QuerySimilarAsync(queryVector, topK);
            var results = new List<(string, float)>();
            foreach (var (id, scoreFromStore) in candidates)
            {
                float score = scoreFromStore ?? 0;
                results.Add((id, score));
            }
            // If using distance (lower is better), order ascending. If similarity (higher is better), order descending.
            // Adjust as needed for your metric.
            return results.OrderByDescending(r => r.Item2).ToList();
        }

        public async Task<string> QueryMetaDataByTermId(Guid termId)
        {
            return await _vectorStore.QueryMetaDataByTermId(termId);
        }
    }
}
