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
using AvePoint.RA.Common.AI;
using RAChatCenter.ChatCompletion;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Common;
using AvePoint.RAI.Core.Services;

namespace RAChatCenter.Services
{
    public class ChatCompletionServices
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ChatCompletionServices));

        public ChatCompletionServices()
        {
            _logger.Info("ChatServices initialized with message: {0}");
        }

        public static async Task<IChatCompletionProvider> CreateWithRAIProvider()
        {
            _logger.Info("Creating ChatCompletionServices with RAI.Core VertexAI provider");

            try
            {
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                _logger.Info($"Start to get chat services (GCP Environment: {isGCP})");

                IChatCompletionService chatService = isGCP
                   ? await AiClientManager.GetVertexAIChatServiceAsync()
                   : await AiClientManager.GetAzureOpenAIChatServiceAsync();

                var adapter = new RAIChatCompletionProviderAdapter(chatService);

                _logger.Info("ChatCompletionServices created successfully with RAI.Core provider");

                return adapter;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create ChatCompletionServices: {0}", ex, ex.Message);
                throw;
            }
        }

    }
}
