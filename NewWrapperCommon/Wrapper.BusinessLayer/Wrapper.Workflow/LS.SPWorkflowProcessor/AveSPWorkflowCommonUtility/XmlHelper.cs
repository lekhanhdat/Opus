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
namespace LS.SPWorkflowProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;
    using AvePoint.GCommon;

    internal static class XmlHelper
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// load xml content, if failed, return null
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static XmlDocument LoadXmlDocument(string content)
        {
            XmlDocument xmlConfig = null;
            try
            {
                xmlConfig = new XmlDocument();
                xmlConfig.LoadXml(content);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while process LoadXmlDocument.Error:{0}", ex);
                xmlConfig = null;
            }
            return xmlConfig;
        }

        public static Dictionary<string, string> GetElementAttributes(XmlDocument xmlConfig, string xpath)
        {
            var properties = new Dictionary<string, string>();
            try
            {
                XmlNode selectedNode = xmlConfig.SelectSingleNode(xpath);
                if (selectedNode != null && selectedNode.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlAttribute attribute in selectedNode.Attributes)
                    {
                        properties.Add(attribute.Name, attribute.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while process GetElementAttributes.Error:{0}", ex);
            }
            finally
            {
                if (xmlConfig != null)
                {
                    xmlConfig.RemoveAll();
                }
            }

            return properties;
        }
    }
}