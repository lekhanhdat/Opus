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

namespace Microsoft365Backup.DataBuilder.TeamHtml.Formatter.Instance
{
    using HtmlAgilityPack;
    using System.Linq;

    internal class EmojiFormatter : HtmlFormatter
    {
        public EmojiFormatter(HtmlDocument doc, ConversationItem item) : base(doc, item)
        {
        }

        public override bool Process()
        {
            return ChangeEmojiTagToValue();
        }

        private bool ChangeEmojiTagToValue()
        {
            bool change = false;

            var docs = doc.DocumentNode.SelectNodes("//emoji");

            if (docs != null)
            {
                foreach (var i in docs)
                {
                    change = true;
                    var emoji = i.Attributes["alt"].Value;
                    var emojiId = i.Attributes["id"].Value;
                    i.Attributes.RemoveAll();
                    i.InnerHtml = string.IsNullOrEmpty(emojiId) ? emoji : TeamHtmlResources.GetEmojiSource(emojiId, emoji);
                }
            }

            return change;
        }
    }
}