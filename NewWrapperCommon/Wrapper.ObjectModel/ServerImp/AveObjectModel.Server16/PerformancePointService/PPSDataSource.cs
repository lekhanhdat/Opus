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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    internal class PPSDataSource
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string PPSDataSourceContentTypeName = "PerformancePoint Data Source";

        public static void UpdateDataSourceContent(AveItem item)
        {
            if (item.File.Item.ContentType != null && string.Compare(item.File.Item.ContentType.Name, PPSDataSourceContentTypeName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return;
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(item.File.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions));

            foreach (XmlElement tmpElement in xmlDocument.DocumentElement.ChildNodes.Cast<XmlElement>().Where(tmpElement => tmpElement.Name == "Location"))
            {
                //ReplaceLocationNode(item,tmpElement);
            }
            //item.mSite.QueryService.ChangeContentByNative(item.info, Encoding.Default.GetBytes(xmlDocument.OuterXml));
        }

        public static Stream ReplaceStreamBeforeAddFile(Stream stream, SPFolder folder, SPFile file, AveBaseItemInfo info, bool isCurrentVersion)
        {
            #region ADO-12509开启performance point service之后，创建的item的xml中需要替换ServerName，该属性值为指向的list 绝对url需要进行替换
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(stream);

            foreach (XmlElement tmpElement in xmlDocument.GetElementsByTagName("DataSource"))
            {
                if (tmpElement.HasAttribute("ServerName"))
                {
                    string value;
                    if (info.MappingManager.SiteMappingManager.GetValueFromAbsoluteUrlMapping(tmpElement.GetAttribute("ServerName"), out value))
                    {
                        tmpElement.SetAttribute("ServerName", value);
                    }
                }
            }
            #endregion
            foreach (XmlElement tmpElement in xmlDocument.DocumentElement.ChildNodes.Cast<XmlElement>().Where(tmpElement => tmpElement.Name == "Location"))
            {
                ReplaceLocationNode(folder, file, tmpElement, isCurrentVersion);
            }
            MemoryStream newStream = new MemoryStream();
            xmlDocument.Save(newStream);
            newStream.TryToResetStreamPosition();
            return newStream;
        }

        public static void ReplaceLocationNode(SPFolder folder, SPFile file, XmlElement tmpElement, bool isCurrentVersion)
        {
            string oldUrl = string.Empty;
            try
            {
                string newValue;
                if (file == null)
                {
                    newValue = folder.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + folder.Url;
                }
                else
                {
                    newValue = folder.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + folder.Url + "/" + file.Item.ID + "_.000";
                }
                oldUrl = tmpElement.GetAttribute("ItemUrl");
                tmpElement.SetAttribute("ItemUrl", newValue);
                if (!isCurrentVersion)
                {
                    return;
                }
                tmpElement.SetAttribute("SpSiteCollectionGuid", folder.ParentWeb.Site.ID.ToString());
                tmpElement.SetAttribute("SpSiteGuid", folder.ParentWeb.ID.ToString());
                tmpElement.SetAttribute("SpListGuid", folder.ParentListId.ToString());
                tmpElement.SetAttribute("ItemGuid", file.UniqueId.ToString());
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.ReplaceLocNodeError, e.ToString());
            }
            if (!WrapperRuntime.WrapperCache.PerformancePointCache.DataSourceInfoMapping.ContainsKey(oldUrl))
            {
                WrapperRuntime.WrapperCache.PerformancePointCache.DataSourceInfoMapping.Add(oldUrl, tmpElement);
            }
        }

        private static string ReplaceItemUrl(AveItem item)
        {
            if (!item.info.IsCurrentVersion)
            {
                return item.mParentFolder.Url;
            }
            return item.mWeb.ServerRelativeUrl.TrimEnd('/') + "/" + item.mParentFolder.Url + "/" + item.info.RowId + "_.000";
        }
    }
}
