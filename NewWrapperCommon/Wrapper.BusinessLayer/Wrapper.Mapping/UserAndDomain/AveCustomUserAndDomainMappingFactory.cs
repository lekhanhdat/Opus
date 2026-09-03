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




namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.IO;
    using System.Xml;
    using AvePoint.Wrapper.Common;

    public class AveCustomUserAndDomainMappingFactory : ISingleton, IAveCustomUserAndDomainMappingFactory, IDisposable
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveCustomUserAndDomainMapping customUserAndDomainMapping;
        public AveCustomUserAndDomainMappingFactory()
        {
            InitConfiguration(string.Empty);
        }
        public void InitConfiguration(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    var userMappingsXml = doc.SelectSingleNode("config.example/UserMappings") as XmlElement;
                    var domainMappingsXml = doc.SelectSingleNode("config.example/DomainMappings") as XmlElement;
                    if (userMappingsXml != null || domainMappingsXml != null)
                    {
                        InitConfiguration(userMappingsXml, domainMappingsXml);
                    }
                }
                else
                {
                    customUserAndDomainMapping = new AveCustomUserAndDomainMapping();
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.ERROR, "Error occurred when init configuration, Path:{0}, Reason:{1}.", path, ex);
            }
        }
        public void InitConfiguration(XmlElement userMapping, XmlElement domainMapping)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.AveCustomUserAndDomainMappingFactory.InitConfiguration"))
            {

            Dictionary<string, string> userMappings = new Dictionary<string, string>();
            Dictionary<string, string> domainMappings = new Dictionary<string, string>();
            if (userMapping != null)
            {
                userMappings = userMapping.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "UserMapping", StringComparison.OrdinalIgnoreCase))
                    .Cast<XmlElement>().ToDictionary(child => child.GetAttribute("SourceUser"), child => child.GetAttribute("DestinationUser"), StringComparer.OrdinalIgnoreCase);
            }
            if (domainMapping != null)
            {
                domainMappings = domainMapping.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "DomainMapping", StringComparison.OrdinalIgnoreCase))
                    .Cast<XmlElement>().ToDictionary(child => child.GetAttribute("SourceDomain"), child => child.GetAttribute("DestinationDomain"), StringComparer.OrdinalIgnoreCase);
            }
            customUserAndDomainMapping = new AveCustomUserAndDomainMapping(userMappings, domainMappings);

            }

        }
        [Obsolete]
        public AveCustomUserAndDomainMapping GetCustomUserAndDomainMapping()
        {
            return customUserAndDomainMapping;
        }
        IAveCustomUserAndDomainMapping IAveCustomUserAndDomainMappingFactory.GetCustomUserAndDomainMapping()
        {
            return GetCustomUserAndDomainMapping();
        }
        public void Dispose()
        {
            customUserAndDomainMapping.Dispose();
        }
    }
}
