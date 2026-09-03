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
    using System.Text;
    using Microsoft365Backup.DataBuilder.TeamHtml;

    public class AttachmentUrlMiddleware : TeamsMessageMiddleware
    {
        public override void Invoke(TeamsMessageContext context)
        {
            if (context.Message.Attachments?.Any(a => a.ContentType == TeamUtil.AttachmentReferenceType) ?? false)
            {
                var content = new StringBuilder();
                context.Message.Attachments
                    .Where(a => a.ContentType == TeamUtil.AttachmentReferenceType)
                    .ForEach(a => content.Append($"<div><a href=\"{a.ContentUrl}\">{a.Name}</a></div>"));

                context.Message.Body.Content += string.Format(TeamHtmlResources.AttachmentUrlsTemplate_html, content.ToString());
            }
            
            Next?.Invoke(context);
        }
    }
}