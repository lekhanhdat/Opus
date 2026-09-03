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

using Microsoft365Backup.DataBuilder.TeamHtml;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.GraphAPI;
using AvePoint.Wrapper.Common;
using ExchangeCommonWrapper;
using Util.MSAzure;

public class StickerImageMiddleware : TeamsMessageMiddleware
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(StickerImageMiddleware));

    public override void Invoke(TeamsMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(context.TeamService);

        context.Message.MessageContent.HostedContents = context.Message.MessageContent.HostedContents ?? new List<HostedContent>();

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(context.Message.Body.Content);
        htmlDocument.DocumentNode.SelectNodes("//img")?.ForEach(i =>
        {
            if (TeamsMessageUtility.HasHostedContentNode(i))
            {
                if (context.Environment is AzureEnvironment.China)
                {
                    throw new NotSupportedException(ExchangeConstants.ChinaNotSupportedHostedContentKey);
                }

                var url = i.Attributes["src"].Value;
                byte[] imageBytes = null;
                if (!IsMatchRegion(url, context))
                    throw new NotSupportedException("The Region does not match");
                try
                {
                    imageBytes = context.TeamService.GetHostedContentAsByte(url);
                }
                catch (GraphAPIException ex) when ((ex.HttpStatusCode == HttpStatusCode.Unauthorized || ex.HttpStatusCode == HttpStatusCode.Forbidden) && context.TeamService4ServiceAccount != null)
                {
                    logger.Warn("Get image unauthorized, we are going to try to get by service account.");
                    imageBytes = context.TeamService4ServiceAccount.GetHostedContentAsByte(url);
                }
                catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound || ex.HttpStatusCode == HttpStatusCode.Gone)
                {
                    logger.Warn("The image is empty.");
                }
                if (imageBytes != null)
                {
                    var temporaryId = TeamUtil.GetHostedContentId(url);
                    var imageContent = Convert.ToBase64String(imageBytes);
                    context.Message.MessageContent.HostedContents.Add(new HostedContent
                    {
                        TemporaryId = temporaryId,
                        ContentBytes = imageContent,
                        ContentType = TeamUtil.HostedContentImageContentType
                    });
                }
            }
        });

        Next?.Invoke(context);
    }
}