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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Navigation;
using AvePoint.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveNavigationSerializer : IAveNavigationSerializer
    {
        AveWeb mAveWeb;
        AveNavigationImport m_importManager = null;

        private SPWeb Web
        {
            get { return mAveWeb.Web; }
        }

        public AveNavigationSerializer(AveWeb web)
        {
            mAveWeb = web;
            m_importManager = new AveNavigationImport(web);
        }

        public AveNavigationSerializer(AveWeb web, object importSettings)
        {
            mAveWeb = web;
            m_importManager = new AveNavigationImport(web);
        }

        public AveNavigationInfoList GetObjectData()
        {
            AveNavigationInfoList nodeList = new AveNavigationInfoList();
            int rankChild = 0;
            if (Web.Navigation.TopNavigationBar != null)
            {
                foreach (SPNavigationNode node in Web.Navigation.TopNavigationBar)
                {
                    AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.TopNavigationBar);
                    navigationInfo.RankChild = rankChild++;
                    nodeList.NavNodes.Add(navigationInfo);
                    BuildNavNodesTree(navigationInfo, node.Children, AveNavigationScope.TopNavigationBar);
                }
            }
            if (Web.Navigation.QuickLaunch != null)
            {
                rankChild = 0;
                foreach (SPNavigationNode node in Web.Navigation.QuickLaunch)
                {
                    AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch);
                    navigationInfo.RankChild = rankChild++;
                    nodeList.NavNodes.Add(navigationInfo);
                    BuildNavNodesTree(navigationInfo, node.Children, AveNavigationScope.QuickLaunch);
                }
            }
            return nodeList;
        }

        private void BuildNavNodesTree(AveNavigationInfo parentNode, SPNavigationNodeCollection NodeCollection, AveNavigationScope scope)
        {
            int rank = 0;
            foreach (SPNavigationNode node in NodeCollection)
            {
                try
                {
                    AveNavigationInfo nodeInfo = ConvertNavNodetoNodeInfo(node, scope);
                    nodeInfo.RankChild = rank++;
                    parentNode.Children.Add(nodeInfo);
                    BuildNavNodesTree(nodeInfo, node.Children, scope);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPNa293", mAveSPWeb.SPWeb.Id, mAveSPWeb.SPWeb.Url, node.Title, node.Url, e);
                }
            }
        }

        private AveNavigationInfo ConvertNavNodetoNodeInfo(SPNavigationNode node, AveNavigationScope scope)
        {
            var navNodeInfo = new AveNavigationInfo { Scope = scope, Title = node.Title, ParentTitle = node.Parent.Title, IsExternal = node.IsExternal, Eid = node.Id };
            if (node.IsExternal && node.Url.Equals(node.Navigation.Web.ServerRelativeUrl))
            {
                navNodeInfo.Url = string.Empty;
            }
            else
            {
                navNodeInfo.Url = node.Url;
            }
            navNodeInfo.MetaInfo = GetNavNodeMetainfo(navNodeInfo.Eid);
            if (navNodeInfo.MetaInfo != null)
            {
                navNodeInfo.HasMetaInfo = true;
            }
            if ((AveEnv.IsMoss) && node.Properties.Contains("NodeType"))
            {
                navNodeInfo.NodeType = (int)(Enum.Parse(typeof(AveNodeTypes), node.Properties["NodeType"].ToString()));
            }
            else
            {
                navNodeInfo.NodeType = -1;
            }
            if (node.Properties.Contains("Target"))
            {
                navNodeInfo.Target = node.Properties["Target"].ToString();
            }
            if (node.Properties.Contains("Description"))
            {
                navNodeInfo.Description = node.Properties["Description"].ToString();
            }
            if (node.Properties.Contains("Audience"))
            {
                navNodeInfo.Audience = node.Properties["Audience"].ToString();
            }
            return navNodeInfo;
        }

        [WrapperOptimization(true)]
        private string GetNavNodeMetainfo(int Eid)
        {
            APIProxy.Current = delegate() { return GetNavNodeMetainfoProxy(Eid); };
            return AveProxyProvider.GetProxy().GetNavigationNodeMetainfo(mAveWeb, Eid);
        }
        private string GetNavNodeMetainfoProxy(int Eid)
        {
            //need to get metainfo by API later on.
            return string.Empty;
        }

        public object SetObjectData(KeyValuePair<Guid, AveNavigationInfoList> navigationInfoList)
        {
            //if (obj == null || !(obj is KeyValuePair<Guid, AveNavigationInfoList>))
            //{
            //    return null;
            //}

            //KeyValuePair<Guid, AveNavigationInfoList> data = (KeyValuePair<Guid, AveNavigationInfoList>)obj;
            m_importManager.Run(navigationInfoList);

            return null;
        }
    }
}
