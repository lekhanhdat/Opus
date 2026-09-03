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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public static class TreeNodeUtil
    {

        private readonly static List<string> specialChart = new List<string>()
        {
            "../", ":", "..%u2216", "..%c0%af", @"..\"
        };
        public static RMSPTreeNode GetGroupNode(this RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.WebApplication)
            {
                // Hydrate missing SiteCollection/TeamsEntire -> WebApplication link from DB.
                if ((node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Office365GroupEntire)
                    && (node.Parent == null || node.Parent.Level == -2))
                {
                    var remoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
                    string parentId = null;
                    if (node.Level == (int)NodeLevel.SiteCollection)
                    {
                        var siteCollection = remoteNodeService?.GetRemoteSiteCollectionById(node.Id);
                        parentId = siteCollection?.parentId;
                    }
                    else
                    {
                        var teamsNode = remoteNodeService?.GetTeamsNodeByTeamsId(node.Id);
                        parentId = teamsNode?.ParentId;
                    }

                    if (!string.IsNullOrEmpty(parentId))
                    {
                        var parentWebApp = remoteNodeService.GetWebApplicationById(parentId);
                        node.ParentId = parentId;
                        node.Parent = new RMSPTreeNode
                        {
                            Id = parentId,
                            SPObjectId = parentId,
                            Level = (int)NodeLevel.WebApplication,
                            Name = parentWebApp?.url,
                            DisplayName = parentWebApp?.url,
                            FullPath = parentWebApp?.url
                        };
                    }
                }

                node = node.Parent;
            }
            return node;
        }
        public static ExchangeOnlineTreeNodeDto GetGroupNode(this ExchangeOnlineTreeNodeDto node)
        {
            while (node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }

        public static RMGoogleTreeNode GetGroupNode(this RMGoogleTreeNode node)
        {
            while (node.Level != (int)NodeLevel.GoogleMyDriveContainer && node.Level != (int)NodeLevel.GoogleSharedDriveContainer)
            {
                node = node.Parent;
            }
            return node;
        }

        public static RMSPTreeNode GetSiteCollectionNode(this RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        public static RMSPTreeNode GetTeamsNode(this RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.Office365GroupEntire)
            {
                node = node.Parent;
            }
            return node;
        }
        public static ExchangeOnlineTreeNodeDto GetTeamsNode(this ExchangeOnlineTreeNodeDto node)
        {
            while (node != null && node.Level != NodeLevel.Office365GroupEntire)
            {
                node = node.Parent;
            }
            return node;
        }
        public static ExchangeOnlineTreeNodeDto GetSiteCollectionNode(this ExchangeOnlineTreeNodeDto node)
        {
            while (node != null && node.Level != NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        public static string GetParentProfileId(this RMSPTreeNode tree)
        {
            string profileId = string.Empty;
            var groupNode = GetGroupNode(tree);
            var siteNode = GetSiteCollectionNode(tree);

            var groupId = "00000000-0000-0000-0000-000000000000";
            var siteId = "00000000-0000-0000-0000-000000000000";
            if (groupNode != null)
            {
                groupId = groupNode.SPObjectId;
            }
            if (siteNode != null)
            {
                siteId = siteNode.SPObjectId;
            }
            if (tree.Level == (int)NodeLevel.SiteCollection)
            {
                return groupId + "|" + siteId + "|";
            }
            if (tree.Level == (int)NodeLevel.WebApplication)
            {
                return groupId + "|";
            }
            var parentWebId = GetParentWebIds(tree);
            if (!string.IsNullOrEmpty(parentWebId))
            {
                parentWebId += "|";
            }
            profileId = groupId + "|" + siteId + "|" + parentWebId + tree.SPObjectId + "|";
            return profileId;
        }

        private static string GetParentWebIds(RMSPTreeNode node)
        {
            var result = "";
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
                if (node != null && node.Level == (int)NodeLevel.Site)
                {
                    result = result == "" ? node.SPObjectId : node.SPObjectId + "|" + result;
                }
            }
            return result;
        }
        public static bool CheckPathTraversal(string path)
        {
            if (CheckPath(path))
            {
                return !specialChart.Any(s => path.IndexOf(s) > 0);
            }
            return false;
        }

        private static bool CheckPath(string path)
        {
            var fileName = Path.GetFileName(path);
            return !Path.GetInvalidFileNameChars().Any(s => fileName.IndexOf(s) > 0);
        }

        public static string GetSPContainderId(RMSPTreeNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            // For a SiteCollection/TeamsEntire node loaded without parent information,
            // hydrate its parent container from DB before traversing upward.
            if ((node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Office365GroupEntire)
                && (node.Parent == null || node.Parent.Level == -2))
            {
                var remoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
                string parentId = null;
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    var siteCollection = remoteNodeService?.GetRemoteSiteCollectionById(node.Id);
                    parentId = siteCollection?.parentId;
                }
                else
                {
                    var teamsNode = remoteNodeService?.GetTeamsNodeByTeamsId(node.Id);
                    parentId = teamsNode?.ParentId;
                }

                if (!string.IsNullOrEmpty(parentId))
                {
                    var parentWebApp = remoteNodeService.GetWebApplicationById(parentId);
                    node.ParentId = parentId;
                    node.Parent = new RMSPTreeNode
                    {
                        Id = parentId,
                        SPObjectId = parentId,
                        Level = (int)NodeLevel.WebApplication,
                        Name = parentWebApp?.url,
                        DisplayName = parentWebApp?.url,
                        FullPath = parentWebApp?.url
                    };
                }
            }

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return node.Id;
            }

            if (node.Parent == null || node.Parent.Level == -2)
            {
                return string.Empty;
            }

            return GetSPContainderId(node.Parent);
        }

        #region EXO Nodes

        public static RMEXOTreeNode GetMailboxGroupNode(this RMEXOTreeNode node)
        {
            while (node.Level != (int)NodeLevel.ExchangeOnlineMailboxGroup && node.Level != (int)NodeLevel.ExchangeOnlineO365GroupGroup)
            {
                node = node.Parent;
            }
            return node;
        }

        public static RMEXOTreeNode GetMailboxNode(this RMEXOTreeNode node)
        {
            while (node.Level != (int)NodeLevel.ExchangeOnlineMailbox && node.Level != (int)NodeLevel.ExchangeOnlineO365Group)
            {
                node = node.Parent;
            }
            return node;
        }

        public static string GetEXOContainderId(RMEXOTreeNode node)
        {
            if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return node.Id;
            }
            else
            {
                return GetEXOContainderId(node.Parent);
            }
        }
        #endregion

    }
}
