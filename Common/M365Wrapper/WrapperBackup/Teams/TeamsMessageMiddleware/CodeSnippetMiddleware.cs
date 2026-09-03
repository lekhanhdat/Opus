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

namespace ExchangeUtility.Graph.Teams;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Newtonsoft.Json;

using Microsoft365Backup.DataBuilder.TeamHtml;
using AvePoint.RA.CommonUtil;
using ExchangeCommonWrapper;
using AvePoint.GCommon.GraphAPI;
using Util.MSAzure;

public class CodeSnippetMiddleware : TeamsMessageMiddleware
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(CodeSnippetMiddleware));

    public override void Invoke(TeamsMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(context.TeamService);

        context.Message.MessageContent.CodeSnippets = new Dictionary<string, CodeSnippetContent>();

        context.Message.Attachments?.Where(attachment => attachment.ContentType == TeamUtil.AttachmentCardCodeSnippetType && !string.IsNullOrEmpty(attachment.Content) && !context.Message.MessageContent.CodeSnippets.ContainsKey(attachment.Id)).ForEach(attachment =>
        {
            var codeSnippetInfo = JsonConvert.DeserializeObject<CodeSnippetContent>(attachment.Content);
            if (TeamsMessageUtility.HasHostedContentText(codeSnippetInfo.CodeSnippetUrl))
            {
                if (context.Environment is AzureEnvironment.China)
                {
                    throw new NotSupportedException(ExchangeConstants.ChinaNotSupportedHostedContentKey);
                }

                string codeSnippet = null;
                if (!IsMatchRegion(codeSnippetInfo.CodeSnippetUrl, context))
                    throw new NotSupportedException("The Region does not match");
                try
                {
                    codeSnippet = context.TeamService.GetHostedContentAsString(codeSnippetInfo.CodeSnippetUrl);
                }
                catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    codeSnippet = string.Empty;
                    codeSnippetInfo.Name = null;
                    codeSnippetInfo.Language = null;
                    logger.Warn("The code snippet is empty.");
                }
                if (codeSnippet != null)
                {
                    codeSnippetInfo.Content = codeSnippet;
                    context.Message.MessageContent.CodeSnippets.Add(attachment.Id, codeSnippetInfo);
                }
            }
            else
            {
                logger.Warn("The code snippet url is invalid: {0}.", codeSnippetInfo.CodeSnippetUrl);
            }
        });

        Next?.Invoke(context);
    }
}