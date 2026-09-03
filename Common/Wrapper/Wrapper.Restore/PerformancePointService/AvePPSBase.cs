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
using System.Reflection;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Restore
{
    public abstract class AvePPSBase
    {
        public static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected AvePerformancePointServiceControl PerformancePointService;

        public static AvePPSBase CreateInstance(string itemType, AvePerformancePointServiceControl control)
        {
            switch (itemType)
            {
                case "Kpi":
                    return new AveSPPPSKpi(control);                    
                case "ReportView":
                    return  new AveSPPPSReportViewer(control);
                case "Scorecard":
                    return  new AvePPSScoreCard(control);
                case "Indicator":
                    return  new AvePPSIndicator(control);
                case "Dashboard":
                    return  new AveSPPPSDashboard(control);
                case "Filter":
                    return  new AveSPPPSFilter(control);
                default:
                    throw new ArgumentException("Invalid item type in create performance point service instance");
            }
        }

        protected AvePPSBase(AvePerformancePointServiceControl avePerformancePointService)
        {           
            PerformancePointService = avePerformancePointService;
        }

        public abstract string Replace(XmlDocument document);

        protected void ReplaceLocation(XmlElement location)
        {
            try
            {
                string oldUrl = location.GetAttribute("ItemUrl");
                if(string.IsNullOrEmpty(oldUrl))
                {
                    return;
                }
                if (location.HasAttribute("ItemUrl"))
                {
                    string newItemUrl = PerformancePointService.Web.ServerRelativeUrl + "/" + PerformancePointService.ListItem.Url;
                    location.SetAttribute("ItemUrl", newItemUrl);
                }        
                if (location.HasAttribute("ItemGuid"))
                {
                    location.SetAttribute("ItemGuid", PerformancePointService.ListItem.UniqueId.ToString());
                }
                if (location.HasAttribute("SpSiteCollectionGuid"))
                {
                    location.SetAttribute("SpSiteCollectionGuid", PerformancePointService.Site.SPSite.ID.ToString());
                }
                if (location.HasAttribute("SpSiteGuid"))
                {
                    location.SetAttribute("SpSiteGuid", PerformancePointService.Web.ID.ToString());
                }
                if (location.HasAttribute("SpListGuid"))
                {
                    location.SetAttribute("SpListGuid", PerformancePointService.ListItem.ParentList.ID.ToString());
                } 
                if(!oldUrl.EndsWith("_.000", StringComparison.Ordinal))
                {//如果客户没有保存最后一次修改，那么当前version的url会像version版本一样，url没有_.000
                    oldUrl = oldUrl + "/" + PerformancePointService.ListItem.ID + "_.000";
                }
                SetInfoMapping(oldUrl,location);
            }
            catch (Exception e)
            {
                log.Warn("Error while replace Location Url:"+ e);
            }
        }

        protected void ReplaceDataSourceLocation(XmlElement dataSourceLocation)
        {
            try
            {
                string oldUrl = dataSourceLocation.GetAttribute("ItemUrl");
                if (WrapperRuntime.WrapperCache.PerformancePointCache.DataSourceInfoMapping.ContainsKey(oldUrl))
                {
                    XmlElement datasourceElement = WrapperRuntime.WrapperCache.PerformancePointCache.DataSourceInfoMapping[oldUrl];

                    dataSourceLocation.SetAttribute("ItemUrl", datasourceElement.GetAttribute("ItemUrl"));

                    if (dataSourceLocation.HasAttribute("ItemGuid"))
                    {
                        dataSourceLocation.SetAttribute("ItemGuid", datasourceElement.GetAttribute("ItemGuid"));
                    }
                    if (dataSourceLocation.HasAttribute("SpSiteCollectionGuid"))
                    {
                        dataSourceLocation.SetAttribute("SpSiteCollectionGuid", datasourceElement.GetAttribute("SpSiteCollectionGuid"));
                    }
                    if (dataSourceLocation.HasAttribute("SpSiteGuid"))
                    {
                        dataSourceLocation.SetAttribute("SpSiteGuid", datasourceElement.GetAttribute("SpSiteGuid"));
                    }
                    if (dataSourceLocation.HasAttribute("SpListGuid"))
                    {
                        dataSourceLocation.SetAttribute("SpListGuid", datasourceElement.GetAttribute("SpListGuid"));
                    }                 
                }
                else
                {
                    log.Log(AveLogLevel.WARN,"No data source mapping was found. Please make sure the data source file is checked");
                }
            }catch(Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceDataSourceLocationError, e);
            }
        }

        protected string ReplaceDefault(string oldUrl)
        {
            return AveReplaceProcessor.UrlReplace(oldUrl, PerformancePointService.Site.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), PerformancePointService.Site.SourceSiteInfo, PerformancePointService.Site.ServerRelativeUrl);
        }                                      

        protected static void ReplaceWithCachedItemInfo(XmlElement element, Dictionary<string, XmlElement> mapping)
        {
            string oldUrl = element.GetAttribute("ItemUrl");
            if (string.IsNullOrEmpty(oldUrl))
            {
                return;
            }
            if (mapping.ContainsKey(oldUrl))
            {
                XmlElement maapingItem = mapping[oldUrl];
                if (maapingItem.HasAttribute("ItemUrl"))
                {
                    element.SetAttribute("ItemUrl", maapingItem.GetAttribute("ItemUrl"));
                }
                if (maapingItem.HasAttribute("ItemGuid"))
                {
                    element.SetAttribute("ItemGuid", maapingItem.GetAttribute("ItemGuid"));
                }
                if (maapingItem.HasAttribute("SpSiteCollectionGuid"))
                {
                    element.SetAttribute("SpSiteCollectionGuid", maapingItem.GetAttribute("SpSiteCollectionGuid"));
                }
                if (maapingItem.HasAttribute("SpSiteGuid"))
                {
                    element.SetAttribute("SpSiteGuid", maapingItem.GetAttribute("SpSiteGuid"));
                }
                if (maapingItem.HasAttribute("SpListGuid"))
                {
                    element.SetAttribute("SpListGuid", maapingItem.GetAttribute("SpListGuid"));
                }
            }
            else
            {
                log.Warn("Can not get mapping while restore element: "+element.OuterXml);
            }
        }

        public abstract void SetInfoMapping(string url, XmlElement location);
    }
}