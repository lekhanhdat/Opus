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
using AveClientRequest.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        [NoAPI]
        public override void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            base.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> needUpdateProperties)
        {
            return base.UpdateNavigationNode(webServerRelativeUrl, navigationNodeProperties, needUpdateProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties)
        {
            return base.GetNavigationNodes(webServerRelativeUrl, navigationNodeId, navigationNodeSource, navProperties);
        }

        [NoAPI("CSOM 获取不到Available Hierarchy Fields和Available Key Filter Fields")]
        public override Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            return base.GetMetadataNavigationSettings(webServerRelativeUrl, listId, listTitle);
        }

        [KeepOriginalWithAPIAttribute]
        public override Dictionary<string, object> AddNavigationNode(string webRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> newNodeProperties, string navigationSource)
        {
            return base.AddNavigationNode(webRelativeUrl, parentNodeProperties, newNodeProperties, navigationSource);
        }

        [NoAPI]
        public override Dictionary<string, object> GetNavigation(string webServerRelativeUrl)
        {
            Dictionary<string, object> nodesProp = new Dictionary<string, object>();
            var token = tokenProviders.GetProviderByType(Office365.Api.TokenType.IDCLR);
            if (token != null)
            {
                try
                {
                    string getUrl;
                    if (this.mCompatibilityLevel == 15)
                    {
                        getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/15/AreaNavigationSettings.aspx";
                    }
                    else
                    {
                        getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/AreaNavigationSettings.aspx";
                    }
                    string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, token);
                    string searchContent = "newNode = new NavigationNode(";
                    AveHttpWebRequestUtility.GetNodesProperties(html, searchContent, nodesProp);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get Web:{0} Navigation failed.Error Message:{1}", webServerRelativeUrl, ex.ToString());
                }
            }
            return this.GetNavigation(webServerRelativeUrl, nodesProp);
        }

        [KeepOriginalWithAPI]
        public override void DeleteNavigationNode(string webServerRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> deleteNodeProperties)
        {
            base.DeleteNavigationNode(webServerRelativeUrl, parentNodeProperties, deleteNodeProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetQuickLaunchFromInheritWeb(string webServerRelativeUrl)
        {
            return base.GetQuickLaunchFromInheritWeb(webServerRelativeUrl);
        }

        [NoAPI]
        public override void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            base.SetMetadataNavigationSettings(webServerRelativeUrl, listTitle, listId, updateProperties);
        }
        [NoAPI]
        public override bool RestoreNavigation(string webServerRelativeUrl, string nodes, Hashtable webAllProperties)
        {
            return base.RestoreNavigation(webServerRelativeUrl, nodes, webAllProperties);
        }
        [KeepOriginalWithAPI]
        public override void UpdateNavigationUseShared(string webServerRelativeUrl, bool useShared)
        {
            base.UpdateNavigationUseShared(webServerRelativeUrl, useShared);
        }
    }
}
