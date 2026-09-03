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
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class BaseControl : FormControlBase
    {
        AveLogger logger = AveLogger.GetInstance(typeof(BaseControl));
        public BaseControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
            mWeb = web;
            mList = list;
            mControlNode = controlNode;
            this.nsManager = nsManager;
            this.contentTypeId = contentTypeId;
            AddControlNameSpace();
        }

        public override void AddControlNameSpace()
        { }

        public override void ProcessControl(bool isPost)
        {
            base.ProcessControl(isPost);
            ProcessResponsiveData();


        }

        /// <summary>
        /// 处理on-premise to online时对Responsive的特殊逻辑
        /// </summary>
        protected void ProcessResponsiveData()
        {
            TryAddResponsiveCssElement();
            TryAddMediaSizeElement();
        }

        private void TryAddResponsiveCssElement()
        {

            XmlNode responsiveCssNode = GetPropertyNode(GetXPath("ResponsiveCss"));
            var cssValue = GetCssValue();
            if (responsiveCssNode == null && !string.IsNullOrEmpty(cssValue))
            {
                responsiveCssNode = mControlNode.OwnerDocument.CreateElement(Prefix, "ResponsiveCss", mControlNode.GetNamespaceOfPrefix(Prefix));
                responsiveCssNode.InnerText = cssValue;
                var paddingWidthNode = GetPropertyNode(GetXPath("PaddingWidth"));
                mControlNode.InsertAfter(responsiveCssNode, paddingWidthNode);
            }
        }

        private string GetCssValue()
        {
            XmlNode attributesNode = GetPropertyNode(GetXPath("Attributes"));
            if (attributesNode != null && attributesNode.HasChildNodes)
            {
                return attributesNode.FirstChild.ChildNodes[1].InnerText;
            }
            return string.Empty;
        }

        private void TryAddMediaSizeElement()
        {
            XmlNode mediaSizesNode = GetPropertyNode(GetXPath("MediaSizes"));
            if (mediaSizesNode != null)
            {
                return;
            }
            var mediaSize = GetMediaSizesValue();
            if (mediaSize == null)
            {
                return;
            }
            var avaiblePrefix = GetAvaiblePrefix(this.Prefix);
            mediaSizesNode = this.mControlNode.OwnerDocument.CreateElement(Prefix, "MediaSizes", mControlNode.GetNamespaceOfPrefix(Prefix));
            (mediaSizesNode as XmlElement).SetAttribute(string.Format("xmlns:{0}", avaiblePrefix), "http://schemas.microsoft.com/2003/10/Serialization/Arrays");
            mediaSizesNode.InnerXml = string.Format("<{0}:KeyValueOfScreenSizeintdYhT3k5k><{0}:Key>Xs</{0}:Key><{0}:Value>{1}</{0}:Value></{0}:KeyValueOfScreenSizeintdYhT3k5k><{0}:KeyValueOfScreenSizeintdYhT3k5k><{0}:Key>Sm</{0}:Key><{0}:Value>{2}</{0}:Value></{0}:KeyValueOfScreenSizeintdYhT3k5k><{0}:KeyValueOfScreenSizeintdYhT3k5k><{0}:Key>Md</{0}:Key><{0}:Value>{3}</{0}:Value></{0}:KeyValueOfScreenSizeintdYhT3k5k>", avaiblePrefix, mediaSize.Xs, mediaSize.Sm / 2, mediaSize.Md);
            var isVisibleNode = GetPropertyNode(GetXPath("IsVisible"));
            if (isVisibleNode != null)
            {
                mControlNode.InsertAfter(mediaSizesNode, isVisibleNode);
            }
            else
            {
                logger.Warn("Can not find isVisible node, can not insert MediaSizes node after it.");
            }
        }

        private string GetAvaiblePrefix(string prefix)
        {
            int prefixNumber;
            if (string.IsNullOrEmpty(prefix) || prefix.Length != 4 || !int.TryParse(prefix[1].ToString(), out prefixNumber))
            {
                return "d2p1";
            }
            return string.Format("d{0}p1", prefixNumber + 2);
        }

        private MediaSize GetMediaSizesValue()
        {
            try
            {
                var mediaSizes = GetCssValue();
                if (string.IsNullOrEmpty(mediaSizes))
                {
                    return null;
                }
                var mediaSize = new MediaSize();
                var sizes = mediaSizes.Split(' ');
                foreach (var size in sizes)
                {
                    if (string.IsNullOrEmpty(size))
                    {
                        continue;
                    }

                    if (size.IndexOf("col-xs-") == 0)
                    {
                        mediaSize.Xs = int.Parse(size.Substring("col-xs-".Length));
                        continue;
                    }
                    if (size.IndexOf("col-sm-") == 0)
                    {
                        mediaSize.Sm = int.Parse(size.Substring("col-sm-".Length));
                        continue;
                    }
                    if (size.IndexOf("col-md-") == 0)
                    {
                        mediaSize.Md = int.Parse(size.Substring("col-md-".Length));
                        continue;
                    }
                }
                return mediaSize;
            }
            catch (Exception)
            {
                //need logs
            }
            return null;
        }

    }
}
