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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AvePPSScoreCard : AvePPSBase
    {
        public AvePPSScoreCard(AvePerformancePointServiceControl avePerformancePointService) : base(avePerformancePointService)
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
                    case "ConfiguredViews":
                        ReplaceConfiguredViews(element);
                        break;
                    case "ReplacePoints":
                        ReplacePoints(element);
                        break;
                    default:
                        continue;
                }
            }
            return document.OuterXml;
        }

        public override void SetInfoMapping(string url, XmlElement location)
        {
            PerformancePointService.ScoreCardUrlInfoMapping.Add(url,location);
        }

        private void ReplacePoints(XmlElement element)
        {
            //处理 EndPoints 和 BeginPoints 结点
            //暂时没有发现需要处理的URL
        }   

        private void ReplaceConfiguredViews(XmlElement element)
        {
            foreach (XmlElement configuredView in element.ChildElements())
            {
                ReplaceConfiguredView(configuredView);
            }
        }

        private void ReplaceConfiguredView(XmlElement configuredView)
        {
            foreach (XmlElement configuredViewChild in configuredView.ChildElements())
            {
                if(string.Equals(configuredViewChild.Name,"DataSourceLocation",StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceDataSourceLocation(configuredViewChild);
                }
                else if(string.Equals(configuredViewChild.Name,"GridViewDefinition",StringComparison.Ordinal))
                {
                    ReplaceGridViewDefinition(configuredViewChild);
                }
            }
        }

        private void ReplaceGridViewDefinition(XmlElement gridViewDefinition)
        {
            foreach (XmlElement gridViewDefinitionChild in gridViewDefinition.ChildElements())
            {
                if (gridViewDefinitionChild.HasAttribute("ItemUrl"))
                {
                    string oldUrl=gridViewDefinitionChild.GetAttribute("ItemUrl");
                    if(!string.IsNullOrEmpty(oldUrl))
                    {
                        gridViewDefinitionChild.SetAttribute("ItemUrl", ReplaceDefault(oldUrl));
                    }
                }else if (string.Equals(gridViewDefinitionChild.Name, "RootRowHeader", StringComparison.Ordinal) || string.Equals(gridViewDefinitionChild.Name, "RootColumnHeader", StringComparison.Ordinal))
                {
                    ReplaceHead(gridViewDefinitionChild);
                }
            }
        }

        private void ReplaceHead(XmlElement header)//RootColumnHeader  RootRowHeader 结点
        {
            foreach (XmlElement headerChild in header.ChildElements())
            {
                if(string.Equals(headerChild.Name,"LinkedKpiLocation",StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceKpiInfo(headerChild);
                }
                if(string.Equals(headerChild.Name,"Children",StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceRowAndColumnHeaderChildren(headerChild);
                }
            }
        }

        private void ReplaceRowAndColumnHeaderChildren(XmlElement headerChild)
        {
            foreach (XmlElement childNode in headerChild.ChildElements())
            {
                if(string.Equals(childNode.Name,"GridHeaderItem",StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceGridHeaderItem(childNode);
                }
            }
        }

        private void ReplaceGridHeaderItem(XmlElement gridHeaderItem)
        {
            if(gridHeaderItem.HasAttribute("DimensionValue"))
            {
                string oldUrl = gridHeaderItem.GetAttribute("DimensionValue");
                gridHeaderItem.SetAttribute("DimensionValue",ReplaceDefault(oldUrl));
            }
            foreach (XmlElement gridHeaderItemChild in gridHeaderItem.ChildElements())
            {
                if (string.Equals(gridHeaderItemChild.Name, "Children", StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceRowAndColumnHeaderChildren(gridHeaderItemChild);
                }
                else if (string.Equals(gridHeaderItemChild.Name, "LinkedKpiLocation", StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceKpiInfo(gridHeaderItemChild);
                }
                else if (string.Equals(gridHeaderItemChild.Name, "TrendIndicatorLocation", StringComparison.OrdinalIgnoreCase))
                {
                    //暂时没有建出来需要处理的情况
                    //ReplaceIndicatorInfo(gridHeaderItemChild);
                }
                else if (string.Equals(gridHeaderItemChild.Name, "OverrideIndicatorLocation", StringComparison.OrdinalIgnoreCase))
                {
                    //暂时没有建出来需要处理的情况
                    //ReplaceIndicatorInfo(gridHeaderItemChild);
                }
            }
        }

        protected void ReplaceKpiInfo(XmlElement element)
        {
            ReplaceWithCachedItemInfo(element, PerformancePointService.KpiUrlInfoMapping);
        }
    }
}