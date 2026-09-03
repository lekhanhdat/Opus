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

namespace ExchangeUtility.Graph.Teams
{
    using System.Linq;
    using Microsoft365Backup.DataBuilder.TeamHtml;
    using AvePoint.Wrapper.Common;

    public class ReplaceStickerImageMiddleware : TeamsMessageMiddleware
    {
        public override void Invoke(TeamsMessageContext context)
        {
            if (context.Message.MessageContent == null || context.Message.MessageContent.HostedContents == null || context.Message.MessageContent.HostedContents.Count == 0)
            {
                Next?.Invoke(context);
                return;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(context.Message.Body.Content);
            htmlDocument.DocumentNode.SelectNodes("//img")?.ForEach(i =>
            {
                if (TeamsMessageUtility.HasHostedContentNode(i))
                {
                    var src = i.Attributes["src"].Value;
                    var temporaryId = TeamUtil.GetHostedContentId(src);
                    if (context.Message.MessageContent.HostedContents.Exists(h => h.TemporaryId == temporaryId))
                    {
                        i.SetAttributeValue("src", string.Format(TeamUtil.HostedContentSource, temporaryId));
                        if (i.Attributes.Contains("width") && i.Attributes.Contains("height"))
                        {
                            i.SetAttributeValue("style", $"vertical-align: bottom; width: {i.Attributes["width"].Value}px; height: {i.Attributes["height"].Value}px");
                        }
                    }
                    else
                    {
                        var properties = TeamUtil.DecodeHostedContentUrl(src);
                        if (properties.TryGetValue("url", out string asmUrl))
                        {
                            i.SetAttributeValue("src", asmUrl);
                        }
                    }
                }
            });
            context.Message.Body.Content = htmlDocument.DocumentNode.OuterHtml;

            Next?.Invoke(context);
        }
    }
}