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
namespace AvePoint.GCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon.Contract.AveLicense;
    using AvePoint.GCommon.Contract.Server.Common;

    public class DeployIDUtility
    {
        private static AveLogger logger = new AveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static DeployIDDto GetFromFile()
        {
            try
            {
                var path = "";
                var doc = GetDocument(out path);
                if (doc == null)
                {
                    return null;
                }
                var deployId = doc.SelectSingleNode("/configuration/properties/DeployId");
                if (deployId != null && !string.IsNullOrEmpty(deployId.InnerText))
                {
                    return new DeployIDDto() { Id = deployId.InnerText, CreateTime = DateTime.UtcNow.Ticks };
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return null;
        }


        public static void CreateToFile(DeployIDDto dto)
        {
            try
            {
                var path = "";
                var doc = GetDocument(out path);
                if (doc == null)
                {
                    return;
                }
                var properties = doc.SelectSingleNode("/configuration/properties");
                if (properties != null)
                {
                    var deployId = doc.CreateElement("DeployId");
                    deployId.InnerText = dto.Id;
                    var old = doc.SelectSingleNode("/configuration/properties/DeployId");
                    if (old != null)
                    {
                        properties.RemoveChild(old);
                    }
                    properties.AppendChild(deployId);
                }
                doc.Save(path);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("insert deploy id to file error:{0}", e.Message));
                logger.Error(e.ToString());
            }
        }

        private static XmlDocument GetDocument(out string path)
        {
            try
            {
                var insatllPath = InstallationUtility.GetControlInstallPath();
                path = Path.Combine(insatllPath, @"Control\bin\ServiceVersion.config");
                if (!File.Exists(path))
                {
                    return null;
                }

                var doc = new XmlDocument();
                doc.Load(path);
                return doc;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("get ServiceVersion.config file error:{0}", e.Message));
                logger.Error(e.ToString());
            }
            path = "";
            return null;
        }

        public  static bool IsSMSP(bool logerror = true)
        {
            try
            {
                var product = LicenseWrapper.ProductType;
                logger.Debug(string.Format("product type is {0}", product.ToString()));
                return product == ProductType.NetApp || product == ProductType.NetApp_IBM;
            }
            catch (Exception e)
            {
                if (logerror)
                {
                    logger.Error(string.Format("get product type error:{0}", e.Message));
                    logger.Error(e.ToString());
                }
            }
            return true;
        }
    }
}
