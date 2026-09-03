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

namespace Microsoft365Backup.DataBuilder.TeamHtml
{
    using AvePoint.RA.CommonUtil;
    using HtmlAgilityPack;
    using Microsoft365Backup.DataBuilder.TeamHtml.Formatter.Instance;
    using System;
    using System.Linq;

    /// <summary>
    /// Used to preprocess html fragments before producing conversation html
    /// Preprocessing includes but is not limited to:
    /// 1. URL\ID replacement and correction
    /// 2. Add\modify\delete styles (styles can also be processed through the onload event when html is loaded)
    ///
    /// How to extend TeamHtmlFormatter and quickly test
    /// 1. Write a subclass of HtmlFormatter and override the Process() method to implement specific preprocessing functions.
    /// 2. Add the newly written subclass to TeamHtmlFormatter.InitFormatters()
    /// 3. Run UT, TeamsConversationBuilderTest.BuildHtmlTest(), and check the generated html.
    /// If the html fragment is more complex, please save the html as a file in c:\data\conversation,
    /// and UT will automatically load the html in the changed path.
    /// </summary>
    public class TeamHtmlFormatter
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(TeamHtmlFormatter));
        private readonly HtmlDocument doc = new HtmlDocument();
        private readonly string html;
        private readonly ConversationItem item;
        private readonly HtmlFormatter[] formatters;

        public TeamHtmlFormatter(ConversationItem item)
        {
            this.item = item;
            html = item.Body;
            doc.LoadHtml(html);
            formatters = InitFormatters(doc, item);
        }

        private static HtmlFormatter[] InitFormatters(HtmlDocument doc, ConversationItem item)
        {
            return
            [
                new OwaReferenceAttachmentsFormatter(doc, item),
                new StickerImageFormatter(doc, item),
                new EmojiFormatter(doc, item)
            ];
        }

        public string Process()
        {
            try
            {
                bool change = this.formatters.Count(f => f.Process()) > 0;
                return change ? this.doc.DocumentNode.OuterHtml : this.html;
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to format conversation html body, error: {ex}");
                return this.html;
            }
        }
    }
}