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
using System.IO;
using System.Linq;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Restore;


namespace AvePoint.Item.Restore
{
    public class AppendUtility
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AppendUtility));

        private static List<string> systemList = InitSystemList();

        private static List<string> InitSystemList()
        {
            try
            {
                string configLocation = Path.Combine(AveEnv.AgentDataPath, "SP2010/Item/SP2010ItemSystemLists.cfg");
                XmlDocument xmlDoc = new XmlDocument();
                if (!File.Exists(configLocation))
                {
                    throw new FileNotFoundException("Can not find config file", configLocation);
                }
                xmlDoc.Load(configLocation);

                return xmlDoc.DocumentElement.Cast<XmlNode>().Where(node => node is XmlElement)
                    .Cast<XmlElement>().Where(node => node.HasAttribute("serverTemplate") && node.HasAttribute("title"))
                    .Select(node => node.GetAttribute("serverTemplate") + "," + node.GetAttribute("title"))
                    .ToList();
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                logger.Log(AveLogLevel.WARN, "Config file can not be found. Error Message: {0}.", fileNotFoundException);
            }
            catch (XmlException xmlException)
            {
                logger.Log(AveLogLevel.WARN, "Config file can not be loaded. Error Message: {0}.", xmlException);
            }
            catch (Exception exception)
            {
                logger.Log(AveLogLevel.WARN, "An error occurred while Init System List. Error Message: {0}.", exception);
            }
            return new List<string>();
        }

        public static bool CheckIsSystemList(AveSPList list)
        {
            if (list.SPList == null)
            {
                return true;
            }
            return systemList.Contains((int)list.SPList.BaseTemplate + "," + list.SPList.Title, StringComparer.OrdinalIgnoreCase);            
        }

        public static bool CheckIsSystemFile(AveSPDoc itemObject)
        {
            string fileServerRaletiveUrl = itemObject.ParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + itemObject.AveSPItem.Name;
            var file = itemObject.Web.GetFile(fileServerRaletiveUrl);

            return file.Exists && file.Item == null;//if file.Item is null,it must be a system file
        }
    }
}
