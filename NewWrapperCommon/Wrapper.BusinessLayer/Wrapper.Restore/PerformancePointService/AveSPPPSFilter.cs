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
using System.Xml;
using System.Linq;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPPPSFilter : AvePPSBase
    {
        public AveSPPPSFilter(AvePerformancePointServiceControl avePerformancePointService) : base(avePerformancePointService)
        {
        }

        public override string Replace(XmlDocument document)
        {
            foreach (XmlElement element in document.DocumentElement.ChildElements())
            {
                switch (element.Name)
                {
                    case "Location":
                        ReplaceLocation(element);
                        break;
                    case "DataSourceLocation":
                        ReplaceDataSourceLocation(element);
                        break;
                    case "BeginPoints":
                    case "EndPoints":
                        ReplacePoint(element);
                        break;
                    default:
                        continue;
                }
            }
            return document.OuterXml;
        }

        public override void SetInfoMapping(string url, XmlElement location)
        {
            PerformancePointService.FilterUrlInfoMapping.Add(url,location);
        }

        /// <summary>
        /// 替换Begin Point和End Point结点中的属性
        /// </summary>
        /// <param name="element"></param>
        private void ReplacePoint(XmlElement element)
        {
            var needReplaceProperties = new string[] { "CustomDefinition" };
            foreach (var needReplaceProperty in needReplaceProperties)
            {
                XmlNodeList nodeList=element.GetElementsByTagName(needReplaceProperty);
                foreach (XmlElement node in nodeList.OfType<XmlElement>())
                {
                    ReplaceCustomDefinition(node);
                }
            }           
        }

        private void ReplaceCustomDefinition(XmlElement node)
        {
            XmlDocument customDefinitionDoc = new XmlDocument();
            try
            {
                customDefinitionDoc.LoadXml(node.InnerText.Trim());

                foreach (XmlElement dataSource in customDefinitionDoc.DocumentElement.GetElementsByTagName("DataSourceLocation").OfType<XmlElement>())
                {
                    ReplaceDataSourceLocation(dataSource);
                }
            }
            catch (XmlException)
            {
                return;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceCustomValueError, e);
            }
            node.InnerText = customDefinitionDoc.OuterXml;
        }
    }
}