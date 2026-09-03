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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RAI.Core.Models;
using AvePoint.RAI.Core.Services;
using RAChatCenter.ChatCompletion;

namespace RAChatCenter.Services
{
    public class RAIChatCompletionProviderAdapter : IChatCompletionProvider
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RAIChatCompletionProviderAdapter));
        private readonly IChatCompletionService _chatCompletionService;

        public string Message => _chatCompletionService.GetProvider().Name;

        public RAIChatCompletionProviderAdapter(IChatCompletionService chatCompletionService)
        {
            _chatCompletionService = chatCompletionService ?? throw new ArgumentNullException(nameof(chatCompletionService));
            _logger.Info("RAI Chat Provider Adapter initialized with provider: {0}", Message);
        }

        public async Task<ChatCompletionResponse> GetChatCompletionResponseAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.Warn("Attempting to get chat completetion response for empty or null text");
                return new ChatCompletionResponse(string.Empty, string.Empty);
            }

            try
            {
                _logger.Debug("Getting completetion response for message with length: {0}", message.Length);

                var messages = new List<ChatMessage>
                {
                    new ChatMessage("user", message)
                };

                var response = await _chatCompletionService.GetChatCompletionAsync(messages);

                if (response.Content != null)
                {
                    _logger.Debug("Successfully Getting completetion response with dimension: {0}", response.Content);
                    return response;
                }
                else
                {
                    _logger.Warn("No Content returned from service");
                    return new ChatCompletionResponse(string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get completetion response for text: {0}", ex, ex.Message);
                throw;
            }
        }
    }
}
