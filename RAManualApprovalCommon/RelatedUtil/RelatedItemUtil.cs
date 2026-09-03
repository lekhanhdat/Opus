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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMRelatedRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace RAManualApprovalCommon.RelatedUtil
{
    public class RelatedItemUtil
    {
        public static List<RMRelatedItemInfo> GetRelatedProperties(string recordsRelatedValue)
        {
            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
            if (!string.IsNullOrEmpty(recordsRelatedValue))
            {
                var sourceUrlValue = recordsRelatedValue;
                XmlDocument xmlDoc = new XmlDocument();
                sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                xmlDoc.LoadXml(sourceUrlValue);
                if (xmlDoc.GetElementsByTagName("a").Count > 0)
                {
                    foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                    {
                        XmlElement element = ele as XmlElement;
                        var relatedObjString = element.GetAttribute("rel");
                        relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                        //JavaScriptSerializer jss = new JavaScriptSerializer();
                        //RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
                        RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                        var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
                        string url = relatedItemUrl;
                        relatedObj.url = relatedItemUrl;
                        relatedObj.url = url;
                        infos.Add(relatedObj);
                    }
                }
                else if (xmlDoc.GetElementsByTagName("RMRelatedItemInfo").Count > 0)
                {
                    infos = AvePoint.GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(sourceUrlValue);
                }
            }
            return infos;
        }

        public static string SerializeRelatedProperties(List<RMRelatedItemInfo> relatedItemInfos)
        {
            return AvePoint.GCommon.Utility.SerializerHelper.SerializeToXmlString<List<RMRelatedItemInfo>>(relatedItemInfos);
        }
    }
}
