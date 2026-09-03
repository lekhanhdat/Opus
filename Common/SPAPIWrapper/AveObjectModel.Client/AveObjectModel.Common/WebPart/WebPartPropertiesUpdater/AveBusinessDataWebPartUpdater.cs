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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AvePoint.ObjectModel.Common.WebPart
{
    public class AveBusinessDataWebPartUpdater : AveWebPartPropertyUpdater
    {
        protected AveBusinessDataWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        {
        }

        protected bool ReplaceListIdProperties(XmlDocument definationXmlDoc)
        {
            string[] nodeNames = new string[] { "ListName", "ListId" };
            bool needPost = false;
            foreach (string nodeName in nodeNames)
            {
                XmlNode listNode = definationXmlDoc.SelectSingleNode(".//*[@name = '" + nodeName + "']");
                if (listNode != null && IsGuid(listNode.InnerText))
                {
                    Guid listId = new Guid(listNode.InnerText);
                    if (listId.Equals(Guid.Empty))
                    {
                        continue;
                    }
                    else if (this.Cache.ListIdMapping.ContainsKey(listId))
                    {
                        listNode.InnerText = "{" + this.Cache.ListIdMapping[listId].ToString() + "}";
                    }
                    else
                    {
                        needPost = true;
                    }
                }
            }
            return needPost;
        }

        protected void UpdateStringNode(XmlDocument definationXmlDoc, List<string> needUpdateProperties = null)
        {
            if (needUpdateProperties == null || needUpdateProperties.Count <= 0)
            {
                XmlNodeList nodes = definationXmlDoc.DocumentElement.SelectNodes(".//*/properties/property[@type='string']");
                if (nodes != null && nodes.Count > 0)
                {
                    foreach (XmlNode tempNode in nodes)
                    {
                        UpdateCertainStringNode(tempNode);
                    }
                }
                return;
            }
            foreach (string tempProperty in needUpdateProperties)
            {
                XmlNode tempNode = definationXmlDoc.DocumentElement.SelectSingleNode(".//*/properties/property[@type='string' and @name='" + tempProperty + "']");
                UpdateCertainStringNode(tempNode);
            }
        }

        private void UpdateCertainStringNode(XmlNode tempNode)
        {
            if (tempNode == null || !tempNode.HasChildNodes)
            {
                return;
            }
            tempNode.InnerText = "<![CDATA[" + tempNode.InnerXml + "]]>";
        }

        /// <summary>
        /// Update BusinessData WebPart properties, can be overwritten by drived classes to update accurate properties.
        /// </summary>
        /// <param name="webpartInfo"></param>
        /// <param name="definationXmlDoc"></param>
        /// <returns></returns>
        public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        {
            UpdateLink();
            UpdateStringNode(definationXmlDoc);
            return ReplaceListIdProperties(definationXmlDoc);
        }
    }

    public class BusinessDataDetailsWebPartUpdater : AveBusinessDataWebPartUpdater
    {
        public BusinessDataDetailsWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        { }

        //public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        //{
        //    UpdateLink();
        //    UpdateStringNode(definationXmlDoc);
        //    return ReplaceListIdProperties(definationXmlDoc);
        //}
    }

    public class BusinessDataListWebPartUpdater : AveBusinessDataWebPartUpdater
    {
        public BusinessDataListWebPartUpdater(AveWebPartCache webPartCache, AveWebPartLinkUpdater webPartLinkUpdater, IAveWeb web)
            : base(webPartCache, webPartLinkUpdater, web)
        { }

        //public override bool UpdateWebPartProperty(AveWebPartBaseInfo webpartInfo, XmlDocument definationXmlDoc)
        //{
        //    UpdateLink();
        //    UpdateStringNode(definationXmlDoc);
        //    return ReplaceListIdProperties(definationXmlDoc);
        //}
    }

}
