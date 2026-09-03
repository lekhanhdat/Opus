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
    using AvePoint.GCommon;
    using System.Reflection;
    using System.IO;
    using System.Xml;
    using AvePoint.Common;
    using AvePoint.Wrapper.Common;

    public class AveCustomTemplateMappingFactory : ISingleton, IAveCustomTemplateMappingFactory, IDisposable
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveCustomTemplateMapping customTemplateMapping;
        private AveCustomTemplateMappingFactory()
        {
            InitConfiguaration(string.Empty);
        }
        public void InitConfiguaration(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    var templateMappingsXml = doc.SelectSingleNode("config.example/CustomTemplateMapping") as XmlElement;
                    InitConfiguaration(templateMappingsXml);
                }
                else
                {
                    //customUserAndDomainMapping = new AveCustomTemplateMapping();
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.ERROR, "Error occurred when init configuration, Path:{0}, Reason:{1}.", path, ex);
            }
        }
        public void InitConfiguaration(XmlElement config)
        {
            customTemplateMapping = new AveCustomTemplateMapping(config);
        }
        [Obsolete]
        public AveCustomTemplateMapping GetCustomTemplateMapping()
        {
            return customTemplateMapping;
        }
        IAveCustomTemplateMapping IAveCustomTemplateMappingFactory.GetCustomTemplateMapping()
        {
            return GetCustomTemplateMapping();
        }
        public void Dispose()
        {
            customTemplateMapping.Dispose();
        }
    }
}
