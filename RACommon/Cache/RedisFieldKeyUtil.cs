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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.SyncNode.Compatible;
using AvePoint.RA.CommonUtil;
using System;
using System.Text;
using AOS = Cloud.Sdk.Data.Aos.Tenant;
using O365 = AvePoint.GCommon.Contract.Server.ControlPanel.Office365;

namespace AvePoint.RA.Common.Cache
{
    public static class RedisFieldKeyUtil
    {
        private const string OneDriveGroup_FieldKey_Prefix = "ODG";
        private const string GroupSitesGroup_FieldKey_Prefix = "GSG";
        private const string SiteCollectionGroup_FieldKey_Prefix = "SCG";
        private const string MailboxGroup_FieldKey_Prefix = "MG";
        private const string O365GroupGroup_FieldKey_Prefix = "GMG";

        private static RALogger logger = RALogger.GetInstance(typeof(RedisFieldKeyUtil));

        public static string GenerateContainerFieldKey(NodeLevel nodeLevel, string name)
        {
            var fieldKey_Prefix = string.Empty;
            switch (nodeLevel)
            {
                case NodeLevel.WebApplication:
                    fieldKey_Prefix = SiteCollectionGroup_FieldKey_Prefix;
                    break;
                case NodeLevel.SkyDriveProGroup:
                    fieldKey_Prefix = OneDriveGroup_FieldKey_Prefix;
                    break;
                case NodeLevel.O365GroupSitesGroup:
                    fieldKey_Prefix = GroupSitesGroup_FieldKey_Prefix;
                    break;
                case NodeLevel.ExchangeOnlineMailboxGroup:
                    fieldKey_Prefix = MailboxGroup_FieldKey_Prefix;
                    break;
                case NodeLevel.ExchangeOnlineO365GroupGroup:
                    fieldKey_Prefix = O365GroupGroup_FieldKey_Prefix;
                    break;
                default:
                    break;
            }
            return GenerateFieldKey(fieldKey_Prefix, name);
        }

        #region RemoteNode
        public static string GenerateRemoteNodeGroupFieldKey(Cloud.Sdk.Data.AosModern.RemoteNodeType aosRemoteNodeType, string groupName)
        {
            return GenerateRemoteNodeGroupFieldKey(ConvertToRemoteNodeType(aosRemoteNodeType), groupName);
        }

        public static string GenerateRemoteNodeGroupFieldKeyByAosId(Cloud.Sdk.Data.AosModern.RemoteNodeType aosRemoteNodeType, string aosId)
        {
            return GenerateRemoteNodeGroupFieldKey(ConvertToRemoteNodeType(aosRemoteNodeType), aosId);
        }

        public static string GenerateRemoteNodeGroupFieldKey(RMCompatibleRemoteNode aosSyncNode)
        {
            return GenerateRemoteNodeGroupFieldKey(ConvertToRemoteNodeType(aosSyncNode.NodeType), aosSyncNode.ParentId);
        }

        public static string GenerateRemoteNodeGroupFieldKey(RemoteNodePara daoGroup)
        {
            return GenerateRemoteNodeGroupFieldKey(daoGroup.NodeType, daoGroup.NodeName);
        }

        public static string GenerateRemoteNodeGroupFieldKeyByAosId(RemoteNodePara daoGroup)
        {
            return GenerateRemoteNodeGroupFieldKey(daoGroup.NodeType, daoGroup.AosId);
        }

        public static string GenerateRemoteNodeGroupFieldKey(O365.RemoveNodeType remoteNodeType, string groupName)
        {
            var fieldKey = string.Empty;
            switch (remoteNodeType)
            {
                case O365.RemoveNodeType.SkyDrivePro:
                    fieldKey = OneDriveGroup_FieldKey_Prefix;
                    break;
                case O365.RemoveNodeType.O365GroupSites:
                    fieldKey = GroupSitesGroup_FieldKey_Prefix;
                    break;
                case O365.RemoveNodeType.SiteCollection:
                    fieldKey = SiteCollectionGroup_FieldKey_Prefix;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("{0} remote node type is out of range.", remoteNodeType.ToString());
            }
            return GenerateFieldKey(fieldKey, groupName);
        }

        public static RemoteNodeCachePair GenerateRemoteNodeCachePair(string fieldKeyStr)
        {
            string[] parts = SplitFieldKey(fieldKeyStr);
            return new RemoteNodeCachePair()
            {
                NodeLevel = ConvertStrToNodeLevel(parts[0]),
                GroupName = parts[1],
            };
        }

        private static NodeLevel ConvertStrToNodeLevel(string nodeTypeStr)
        {
            var nodeLevel = NodeLevel.WebApplication;
            switch (nodeTypeStr)
            {
                case OneDriveGroup_FieldKey_Prefix:
                    nodeLevel = NodeLevel.SkyDriveProGroup;
                    break;
                case GroupSitesGroup_FieldKey_Prefix:
                    nodeLevel = NodeLevel.O365GroupSitesGroup;
                    break;
                case SiteCollectionGroup_FieldKey_Prefix:
                    nodeLevel = NodeLevel.WebApplication;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Node Type {0} is our of range.", nodeTypeStr);
            }
            return nodeLevel;
        }

        private static RemoveNodeType ConvertToRemoteNodeType(Cloud.Sdk.Data.AosModern.RemoteNodeType syncNodeType)
        {
            if (syncNodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive)
            {
                return RemoveNodeType.SkyDrivePro;
            }
            else if (syncNodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
            {
                return RemoveNodeType.O365GroupSites;
            }
            else
            {
                return RemoveNodeType.SiteCollection;
            }
        }
        #endregion

        #region Mailbox
        public static string GenerateMailboxGroupFieldKey(RMCompatibleRemoteNode aosSyncNode)
        {
            return GenerateMailboxGroupFieldKey(GetNodeLevel(aosSyncNode.NodeType), aosSyncNode.ParentName);
        }

        public static string GenerateMailboxGroupFieldKeyByAosId(RMCompatibleRemoteNode aosSyncNode)
        {
            return GenerateMailboxGroupFieldKey(GetNodeLevel(aosSyncNode.NodeType), aosSyncNode.ParentId);
        }

        public static string GenerateMailboxGroupFieldKey(RemoteNodePara daoGroup)
        {
            return GenerateMailboxGroupFieldKey(daoGroup.NodeLevel, daoGroup.NodeName);
        }

        public static string GenerateMailboxGroupFieldKeyByAosId(RemoteNodePara daoGroup)
        {
            return GenerateMailboxGroupFieldKey(daoGroup.NodeLevel, daoGroup.AosId);
        }

        public static string GenerateMailboxGroupFieldKey(NodeLevel nodeLevel, string groupName)
        {
            var fieldKey_Prefix = string.Empty;
            switch (nodeLevel)
            {
                case NodeLevel.ExchangeOnlineMailboxGroup:
                    fieldKey_Prefix = MailboxGroup_FieldKey_Prefix;
                    break;
                case NodeLevel.ExchangeOnlineO365GroupGroup:
                    fieldKey_Prefix = O365GroupGroup_FieldKey_Prefix;
                    break;
            }
            return GenerateFieldKey(fieldKey_Prefix, groupName);
        }

        public static MailboxGroupCachePair GenerateMailboxGroupCachePair(string fieldKey)
        {
            string[] parts = SplitFieldKey(fieldKey);
            return new MailboxGroupCachePair()
            {
                NodeLevel = ConvertStrToMailboxLevel(parts[0]),
                GroupName = parts[1],
            };
        }

        private static NodeLevel ConvertStrToMailboxLevel(string levelStr)
        {
            NodeLevel nodeLevel = NodeLevel.ExchangeOnlineMailboxGroup;
            switch (levelStr)
            {
                case MailboxGroup_FieldKey_Prefix:
                    nodeLevel = NodeLevel.ExchangeOnlineMailboxGroup;
                    break;
                case O365GroupGroup_FieldKey_Prefix:
                    nodeLevel = NodeLevel.ExchangeOnlineO365GroupGroup;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("{0} is out of range.", levelStr);
            }
            return nodeLevel;
        }

        private static NodeLevel GetNodeLevel(Cloud.Sdk.Data.AosModern.RemoteNodeType nodeType)
        {
            if (nodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
            {
                return NodeLevel.ExchangeOnlineO365GroupGroup;
            }
            else
            {
                return NodeLevel.ExchangeOnlineMailboxGroup;
            }
        }
        #endregion

        private static string GenerateFieldKey(string fieldKeyPrefix, string groupName)
        {
            var sBuilder = new StringBuilder();
            sBuilder.Append(fieldKeyPrefix).Append("_").Append(groupName);
            return sBuilder.ToString();
        }

        private static string[] SplitFieldKey(string fieldKeyStr)
        {
            int index = fieldKeyStr.IndexOf("_");
            if (index == -1)
            {
                logger.Error("Failed to find _ in the fieldKey.");
                return new string[] { };
            }
            string nodeLevelPrefix = fieldKeyStr.Substring(0, index);
            string groupName = fieldKeyStr.Substring(index + 1);
            return new string[] { nodeLevelPrefix, groupName };
        }
    }

    public class MailboxGroupCachePair
    {
        public NodeLevel NodeLevel { get; set; }
        public string GroupName { get; set; }
    }

    public class RemoteNodeCachePair
    {
        public NodeLevel NodeLevel { get; set; }
        public string GroupName { get; set; }
    }
}
