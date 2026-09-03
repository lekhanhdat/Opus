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
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPPPSKpi : AvePPSBase
    {
        public AveSPPPSKpi(AvePerformancePointServiceControl avePerformancePointService) : base(avePerformancePointService)
        {
        }

        /// <summary>
        /// 替换Actuals，Targets结点中的属性
        /// </summary>
        /// <param name="node"></param>
        private void ReplaceActualsAndTargets(XmlElement node)
        {
            foreach (XmlElement childNode in node)
            {
                foreach (XmlElement secondChild in childNode)
                {
                    if(string.Equals(secondChild.Name,"DataSourceLocation",StringComparison.OrdinalIgnoreCase))
                    {
                        ReplaceDataSourceLocation(secondChild);
                        continue;
                    }
                    if(string.Equals(secondChild.Name,"IndicatorLocation",StringComparison.OrdinalIgnoreCase))
                    {
                        ReplaceIndicatorInfo(secondChild);
                    }
                }
            }        
        }

       [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public override string Replace(XmlDocument document)
        {
            foreach (XmlElement element in document.DocumentElement)
            {                        
                switch (element.Name)
                {
                    case "Location":
                        ReplaceLocation(element);                        
                        break;
                    case "Actuals":
                    case "Targets":
                        ReplaceActualsAndTargets(element);
                        break;
                    default:
                        continue;
                }
            }
            return document.OuterXml;
        }

        public override void SetInfoMapping(string url, XmlElement location)
        {
            this.PerformancePointService.KpiUrlInfoMapping.Add(url, location);
        }

        private void ReplaceIndicatorInfo(XmlElement element)
        {
            ReplaceWithCachedItemInfo(element, PerformancePointService.IndicatorUrlInfoMapping);
        }
    }
}