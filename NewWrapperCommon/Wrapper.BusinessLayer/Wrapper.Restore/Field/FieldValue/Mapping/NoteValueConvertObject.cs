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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.Wrapper.Restore
{
    class NoteValueConvertObject : BaseValueConvertObject
    {
        private object sourceValue;
        private string sourceFieldName;
        private IAveFieldMultiLineText destField;

        public NoteValueConvertObject(IAveField destField, AveSPItem mItem, int originalRowId, object sourceValue, string sourceFieldName)
            : base(destField, mItem, originalRowId)
        {
            this.sourceValue = sourceValue;
            this.sourceFieldName = sourceFieldName;
        }

        public override object ConvertMultiValue(List<string> values)
        {
            destField = base.destField as IAveFieldMultiLineText;
            AveXmlField xmlField = mItem.ParentList.AveFields.XmlFields[sourceFieldName];
            if (destField.RichText && ((xmlField.Type == AveFieldType.Note && xmlField.RichText) || xmlField.TypeAsString.Equals("HTML", StringComparison.Ordinal)))
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                var tempValue = sourceValue == null ? string.Empty : sourceValue.ToString();
                string htmlString = "<HtmlRoot>" + tempValue + "</HtmlRoot>";
                htmlDoc.LoadHtml(htmlString);
                HtmlNode rootNode = htmlDoc.DocumentNode.FirstChild;
                int count = 0;
                SetHtmlInnerText(rootNode, values, ref count);
                return ReplaceLinks(rootNode.InnerHtml, destField);
            }
            else
            {
                //todo:qlluo: 测试一下多行text column
                string splitChar = this.mItem.ParentSite.SPContextKind.IsServerMode13Upper() ? "\n" : "\r\n";
                return SerializeMultiValue(values, splitChar);
            }
        }

        private void SetHtmlInnerText(HtmlNode node, List<string> innerTexts, ref int i)
        {
            foreach (var n in node.ChildNodes)
            {
                if (n.ChildNodes.Count == 0 && n.InnerText != "\r\n" && n.InnerText != "\n")
                {
                    n.InnerHtml = innerTexts[i];
                    i++;
                }
                else
                {
                    SetHtmlInnerText(n, innerTexts, ref i);
                }
            }
        }
    }
}
