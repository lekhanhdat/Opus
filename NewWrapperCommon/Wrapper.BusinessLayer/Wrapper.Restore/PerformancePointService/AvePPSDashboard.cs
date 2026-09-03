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
    public class AveSPPPSDashboard : AvePPSBase
    {
        public AveSPPPSDashboard(AvePerformancePointServiceControl avePerformancePointService) : base(avePerformancePointService)
        {
        }

        public override string Replace(XmlDocument document)
        {
            XmlElement root = document.DocumentElement;
            ReplaceDashBoardRoot(root);
            foreach (XmlElement element in document.DocumentElement.ChildElements())
            {
                if(string.Equals(element.Name,"Location",StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceLocation(element);
                }
                if(string.Equals(element.Name,"Pages",StringComparison.OrdinalIgnoreCase))
                {
                    ReplacePages(element);
                }
            }
            return document.OuterXml;
        }

        /// <summary>
        /// DashBoard 根节点中有一些属性需要替换
        /// </summary>
        /// <param name="root"></param>
        private void ReplaceDashBoardRoot(XmlElement root)
        {
            if(root.HasAttribute("DeploymentPath"))
            {
                string oldDeploymentUrl = root.GetAttribute("DeploymentPath");
                root.SetAttribute("DeploymentPath", ReplaceDefault(oldDeploymentUrl));
            }
            if (root.HasAttribute("SitePath"))
            {
                string oldSitePath = root.GetAttribute("SitePath");
                root.SetAttribute("SitePath", ReplaceDefault(oldSitePath));
            }
            if (root.HasAttribute("MasterPagePath"))
            {
                string oldMasterPagePath = root.GetAttribute("MasterPagePath");
                root.SetAttribute("MasterPagePath", ReplaceDefault(oldMasterPagePath));
            }
        }

        public override void SetInfoMapping(string url, XmlElement location)
        {
            //
        }

        /// <summary>
        /// 替换Pages结点中的属性
        /// </summary>
        /// <param name="pagesElement"></param>
        private void ReplacePages(XmlElement pagesElement)
        {
            foreach (XmlElement dashboardElement in pagesElement.ChildElements())
            {
                ReplaceDashboardElement(dashboardElement);
            }
        }

        /// <summary>
        /// 替换DashBoardElement结点中的属性
        /// </summary>
        /// <param name="dashboardElement"></param>
        private void ReplaceDashboardElement(XmlElement dashboardElement)
        {
            foreach (XmlElement dashboardElementChild in dashboardElement.ChildElements())
            {
                if(string.Equals(dashboardElementChild.Name,"DashboardElements",StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlElement innerDashboardElement in dashboardElementChild.ChildElements())
                    {
                        ReplaceDashboardElement(innerDashboardElement);                        
                    }
                }
                else if (string.Equals(dashboardElementChild.Name, "UnderlyingElementLocation", StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceUnderlyingElementLocation(dashboardElementChild);
                }
            }
        }

        /// <summary>
        /// 替换UnderlyingElementLocation结点中的属性
        /// </summary>
        /// <param name="dashboardElementChild"></param>
        private void ReplaceUnderlyingElementLocation(XmlElement dashboardElementChild)
        {
            if(string.Equals(dashboardElementChild.GetAttribute("ItemType"),"Scorecard",StringComparison.OrdinalIgnoreCase))
            {
                ReplaceScoreCardInfo(dashboardElementChild);
            }
            else if (string.Equals(dashboardElementChild.GetAttribute("ItemType"), "Filter", StringComparison.OrdinalIgnoreCase))
            {
                ReplaceFilterInfo(dashboardElementChild);
            }
            else if (string.Equals(dashboardElementChild.GetAttribute("ItemType"), "ReportView", StringComparison.OrdinalIgnoreCase))
            {
                ReplaceReportInfo(dashboardElementChild);
            }
        }

        private void ReplaceScoreCardInfo(XmlElement element)
        {
            ReplaceWithCachedItemInfo(element, PerformancePointService.ScoreCardUrlInfoMapping);
        }

        private void ReplaceFilterInfo(XmlElement element)
        {
            ReplaceWithCachedItemInfo(element, PerformancePointService.FilterUrlInfoMapping);
        }

        private void ReplaceReportInfo(XmlElement element)
        {
            ReplaceWithCachedItemInfo(element, PerformancePointService.ReportUrlInfoMapping);
        }
    }
}