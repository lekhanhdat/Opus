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
    using System.Linq;
    using HtmlAgilityPack;

    class StickerImageFormatter : HtmlFormatter
    {
        public StickerImageFormatter(HtmlDocument doc, ConversationItem item) : base(doc, item) { }

        public override bool Process()
        {
            var change = RemoveSuffixFromSrc();
            change |= ChangeHostedContent2ImageContent();
            change |= ChangeHostedContentUrl2SkypeAsmUrl();
            return change;
        }

        private bool RemoveSuffixFromSrc() => doc.DocumentNode.SelectNodes("//img[@itemtype='http://schema.skype.com/AMSImage']")?.Count(RemoveCIDSuffixFromSrc) > 0;

        private bool RemoveCIDSuffixFromSrc(HtmlNode node) => RemoveSubStringInAttribute(node, ATTRIBUTE_SRC, "cid:");

        private bool ChangeHostedContent2ImageContent() => doc.DocumentNode.SelectNodes($"//img[starts-with(@src,'{ENDPOINT_GRAPH}')]")?.Count(ChangeHostedContent2ImageContent) > 0;

        private bool ChangeHostedContent2ImageContent(HtmlNode node)
        {
            var src = node.GetAttributeValue(ATTRIBUTE_SRC, string.Empty);
            var hostedContentsId = TeamUtil.GetHostedContentId(src);
            if (Item.HostedContents != null && Item.HostedContents.TryGetValue(hostedContentsId, out var imageContent))
            {
                node.SetAttributeValue(ATTRIBUTE_SRC, $"data:{TeamUtil.HostedContentImageContentType};base64,{imageContent}");
                return true;
            }
            return false;
        }

        private bool ChangeHostedContentUrl2SkypeAsmUrl() => doc.DocumentNode.SelectNodes($"//img[starts-with(@src,'{ENDPOINT_GRAPH}')]")?.Count(ChangeHostedContentUrl2SkypeAsmUrl) > 0;

        private bool ChangeHostedContentUrl2SkypeAsmUrl(HtmlNode node)
        {
            var src = node.GetAttributeValue(ATTRIBUTE_SRC, string.Empty);
            var properties = TeamUtil.DecodeHostedContentUrl(src);
            if (properties.TryGetValue("url", out string asmUrl))
            {
                node.SetAttributeValue(ATTRIBUTE_SRC, asmUrl);
                return true;
            }
            return false;
        }
    }
}