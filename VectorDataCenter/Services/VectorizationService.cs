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
using AvePoint.RA.VectorDataCenter.Models;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.AI;
using AvePoint.RAI.Core.Services;
using AvePoint.RAI.Core.Utils;
using System.Linq;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Common;

namespace AvePoint.RA.VectorDataCenter.Services
{
    public class VectorizationService
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(VectorizationService));
        private readonly ITextEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;

        public VectorizationService(ITextEmbeddingService embeddingService, IVectorStore vectorStore)
        {
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));

            _logger.Info("VectorizationService initialized with embedding service provider: {0}", _embeddingService.GetProvider().Name);
        }

        /// <summary>
        /// Create VectorizationService with RAI.Core embedding provider
        /// </summary>
        /// <param name="vectorStore">Vector storage implementation</param>
        /// <returns>VectorizationService instance with RAI.Core embedding provider</returns>
        public static async Task<VectorizationService> CreateWithRAIProvider(IVectorStore vectorStore)
        {
            _logger.Info("Creating VectorizationService with RAI.Core VertexAI provider");

            try
            {
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                _logger.Info($"Start to get embedding services (GCP Environment: {isGCP})");

                ITextEmbeddingService embeddingService = isGCP
                   ? await AiClientManager.GetVertexAIEmbeddingServiceAsync()
                   : await AiClientManager.GetAzureOpenAIEmbeddingServiceAsync();

                _logger.Info("VectorizationService created successfully with RAI.Core provider");
                return new VectorizationService(embeddingService, vectorStore);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create VectorizationService with RAI.Core provider: {0}", ex, ex.Message);
                throw;
            }
        }

        public async Task StoreTermAsync(TermDescription term)
        {
            if (term == null)
            {
                _logger.Error("Term parameter is null");
                throw new ArgumentNullException(nameof(term));
            }

            if (string.IsNullOrEmpty(term.Name))
            {
                _logger.Error("Term name is null or empty");
                throw new ArgumentException("Term name cannot be null or empty", nameof(term));
            }

            // Return early if description is null
            if (term.Description == null)
            {
                _logger.Info("Term description is null for term '{0}', skipping vectorization", term.Name);
                return;
            }

            string description = term.Description;

            // Only limit text length if it exceeds 2000 characters
            string limitedDescription;
            if (description.Length > 2000)
            {
                limitedDescription = TextLimitingUtils.LimitTextLength(description);
                if (limitedDescription.Length != description.Length)
                {
                    _logger.Info("Description truncated from {0} to {1} characters for term '{2}'",
                        description.Length, limitedDescription.Length, term.Name);
                }
            }
            else
            {
                _logger.Info("Description length is within limit ({0} characters) for term '{1}'", description.Length, term.Name);
                limitedDescription = description;
            }

            var response = await _embeddingService.GetEmbeddingsAsync(new[] { limitedDescription });
            
            if (response == null)
            {
                _logger.Error("Embedding service returned null response for term '{0}'", term.Name);
                throw new InvalidOperationException("Embedding service returned null response");
            }

            if (response.Embeddings == null || !response.Embeddings.Any())
            {
                _logger.Error("Embedding service returned null or empty embeddings for term '{0}'", term.Name);
                throw new InvalidOperationException("Embedding service returned null or empty embeddings");
            }

            var vector = response.Embeddings.First();
            await _vectorStore.StoreVectorAsync(term.Id, term.Name, vector, limitedDescription);
        }

        public async Task DeleteTermAsync(Guid id)
        {
            await _vectorStore.DeleteVectorAsync(id);
        }

    }
}
