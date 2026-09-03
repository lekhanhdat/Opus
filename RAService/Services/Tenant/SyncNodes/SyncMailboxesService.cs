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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.SyncNode.Compatible;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Tenant.Notification;
using AvePoint.RA.Service.Services.Tenant.Notification.Excutor;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using Cloud.Sdk.Data.Aos.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes
{
    public class SyncMailboxesService : AbstractSyncService<EmailAccountDto>, ISyncService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(SyncMailboxesService));
        private Dictionary<string, EmailAccountGroupDto> newMailboxGroupDict = new Dictionary<string, EmailAccountGroupDto>();
        private Dictionary<string, EmailAccountGroupDto> updateMailboxGroupDitc = new Dictionary<string, EmailAccountGroupDto>();
        private Dictionary<string, EmailAccountDto> newMailboxNameToDtoDict = new Dictionary<string, EmailAccountDto>();

        public SyncMailboxesService(SyncDataJobContext context) : base(context)
        {
        }

        private IRMMailboxService MailboxService
        {
            get
            {
                return PlatformWindsorManager.GetService<IRMMailboxService>();
            }
        }

        private ISyncMailboxRedisService MailboxRedisService
        {
            get
            {
                return PlatformWindsorManager.GetService<ISyncMailboxRedisService>();
            }
        }

        #region Cache
        #region GetGroupsCache
        protected override Dictionary<string, RemoteNodePara> GetGroupsCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosNodes)
        {
            return MailboxRedisService.GetGroupsCache(tenantGroupId, aosNodes);
        }

        protected override string GetGroupFieldKey(RMCompatibleRemoteNode aosNode)
        {
            NodeLevel groupNodeLevel = ConvertGroupNodeLevel(aosNode.NodeType);
            //return RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(groupNodeLevel, aosNode.ParentName);
            return RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(groupNodeLevel, aosNode.ParentId);
        }

        protected override RemoteNodePara GetGroupCacheByNameAndNodeLevel(string parentName, RMCompatibleRemoteNode aosNode)
        {
            NodeLevel groupNodeLevel = ConvertGroupNodeLevel(aosNode.NodeType);
            //return MailboxService.GetMailGroupByNameAndNodeLevel(parentName, (int)groupNodeLevel);
            return MailboxService.GetMailGroupByAosIdAndNodeLevel(aosNode.ParentId, (int)groupNodeLevel);
        }

        protected override void AddGroupsToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> newGroupsCache)
        {
            MailboxRedisService.AddGroupsToCache(tenantGroupId, newGroupsCache);
        }

        protected override RemoteNodePara GetGroupFromDB(string groupFieldKey)
        {
            MailboxGroupCachePair cachePair = RedisFieldKeyUtil.GenerateMailboxGroupCachePair(groupFieldKey);
            //return MailboxService.GetMailGroupByNameAndNodeLevel(cachePair.GroupName, (int)cachePair.NodeLevel);
            return MailboxService.GetMailGroupByAosIdAndNodeLevel(cachePair.GroupName, (int)cachePair.NodeLevel);
        }
        #endregion

        #region GetNodesCache
        protected override Dictionary<string, SyncRemoteNodePara> GetNodesCache(string tenantGroupId, List<string> urls)
        {
            return MailboxRedisService.GetNodesCache(tenantGroupId, urls);
        }

        protected override List<EmailAccountDto> GetNodesFromDBByUrls(List<string> urls)
        {
            return MailboxService.GetMailboxesByEmailAddressNameWithoutEncryption(urls);
        }

        protected override SyncRemoteNodePara ConvertDaoNodeModelToCacheModel(EmailAccountDto daoModel)
        {
            return new SyncRemoteNodePara()
            {
                NodeName = daoModel.Email,
                ParentId = daoModel.ParentId,
                AppType = daoModel.AppType,
                AuthType = daoModel.ConnectionType,
                ServiceAccountId = daoModel.ServiceAccountId,
                TenantId = daoModel.TenantId,
                UserName = daoModel.Username,
                NodeLevel = daoModel.NodeLevel,
                ScanSource = (RemoteNodeScanSource)(int)daoModel.ScanSource,
            };
        }

        protected override string FieldKeySelector(EmailAccountDto node)
        {
            return node.Email.ToLower();
        }

        protected override void AddNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> newNodesDict)
        {
            MailboxRedisService.AddNodesToCache(tenantGroupId, newNodesDict);
        }
        #endregion

        protected override void InitCache(string tenantGroupId)
        {
            MailboxRedisService.InitCache(tenantGroupId);
        }
        #endregion

        protected override void AddToListOfNewGroup(RMCompatibleRemoteNode node)
        {
            NodeLevel nodeLevel = GetNodeLevel(node.NodeType);
            //string groupKey = RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(nodeLevel, node.ParentName);
            string groupKey = RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(nodeLevel, node.ParentId);
            if (!newMailboxGroupDict.Keys.Contains(groupKey))
            {
                newMailboxGroupDict.Add(groupKey, new EmailAccountGroupDto()
                {
                    id = node.ParentId,
                    Name = node.ParentName,
                    NodeLevel = nodeLevel,
                    AosId = node.ParentId
                });
            }
        }

        protected override void AddToListOfUpdateGroup(RMCompatibleRemoteNode node, RemoteNodePara existGroup)
        {
            if(existGroup.NodeName == node.ParentName)
            {
                return;
            }

            NodeLevel nodeLevel = GetNodeLevel(node.NodeType);
            //string groupKey = RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(nodeLevel, node.ParentName);
            string groupKey = RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(nodeLevel, node.ParentId);
            if (!updateMailboxGroupDitc.Keys.Contains(groupKey))
            {
                updateMailboxGroupDitc.Add(groupKey, new EmailAccountGroupDto()
                {
                    id = node.ParentId,
                    Name = node.ParentName,
                    NodeLevel = nodeLevel,
                    AosId = node.ParentId
                });
            }
        }

        protected override async System.Threading.Tasks.Task AddToListsForNodesAsync(RMCompatibleRemoteNode node)
        {
            if (!newMailboxNameToDtoDict.Keys.Contains(node.Url.ToLower()))
            {
                newMailboxNameToDtoDict.Add(node.Url.ToLower(), new EmailAccountDto()
                {
                    Email = node.Url,
                    ParentId = node.ParentId,
                    ParentName = node.ParentName,
                    Username = node.UserName,
                    Password = string.Empty,
                    SPVersion = node.SPVersion,
                    State = EmailAccountState.AccessAll,
                    Id = node.Id,
                    TenantId = !string.IsNullOrEmpty(node.TenantId) ? node.TenantId : string.Empty,
                    ConnectionType = (BposConnectionType)node.ConnectionType,
                    NodeLevel = (node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group) ? NodeLevel.ExchangeOnlineO365Group : NodeLevel.ExchangeOnlineMailbox,
                    AppType = ConvertIdentityTypeToAppType(node.AppProfileType),
                    MailboxType = GetMailboxType(node),
                    ScanSource = MailboxScanSource.AOS,
                    ServiceAccountId = GetServiceAccountId(node),
                    ObjectId = node.ObjectId
                });
            }
        }

        protected override string GetUserName(RMCompatibleRemoteNode node)
        {
            var authType = (BposConnectionType)node.ConnectionType;
            return (authType == BposConnectionType.ServiceAccount) ? string.Empty : node.ProfileUserName;
        }

        protected override string GetServiceAccountId(RMCompatibleRemoteNode node)
        {
            var authType = node.ConnectionType;
            return (authType == Cloud.Sdk.Data.AosModern.ConnectionType.ServiceAccount) ? HashCodeHelper.ToMD5HashCode(node.UserName.ToLowerInvariant()) : string.Empty;
        }

        private MailboxType GetMailboxType(RMCompatibleRemoteNode node)
        {
            if (node == null)
            {
                return MailboxType.None;
            }
            if (node.ObjectType == Cloud.Sdk.Data.AosModern.RemoteObjectType.PublicFolderMailbox)
            {
                return MailboxType.PublicFolder;
            }
            if (node.GroupType == Cloud.Sdk.Data.AosModern.O365GroupType.TeamsGroup)
            {
                return MailboxType.Teams;
            }
            return MailboxType.None;
        }

        protected override bool CheckExitedGroup(RMCompatibleRemoteNode node, RemoteNodePara group)
        {
            return group.NodeLevel == GetNodeLevel(node.NodeType);
        }

        private NodeLevel GetNodeLevel(Cloud.Sdk.Data.AosModern.RemoteNodeType nodeType)
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

        private RemoteNodePara ConvertDaoGroupModelToCacheModel(EmailAccountGroupDto daoGroupModel)
        {
            if (daoGroupModel == null)
            {
                throw new ArgumentNullException("Email group model is null.");
            }
            return new RemoteNodePara()
            {
                NodeId = daoGroupModel.id,
                NodeName = daoGroupModel.Name,
                NodeLevel = daoGroupModel.NodeLevel,
                AosId = daoGroupModel.AosId
            };
        }


        protected override void SyncNodesAndGroups(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> updateObjectsDict, List<string> deleteOjects, List<string> deleteOneDriveObjects, Dictionary<string, SyncRemoteNodePara> updateSecondParentIdDict)
        {
            logger.Info("Begin to sync mailbox containers and nodes.");
            if (newMailboxGroupDict.Count > 0)
            {
                List<EmailAccountGroupDto> newMailboxGroups = newMailboxGroupDict.Values.ToList();
                Dictionary<string, RemoteNodePara> newMailboxGroupCacheDict = ConvertMailboxGroupDictToCacheDict(newMailboxGroupDict);
                MailboxRedisService.AddGroupsToCache(tenantGroupId, newMailboxGroupCacheDict, () =>
                {
                    MailboxService.CreateMailboxGroups(newMailboxGroups);
                });
                logger.Info("Add {0} mailbox containers.", newMailboxGroupDict.Count);
                LogSyncDataInfo(newMailboxGroupDict.Keys.ToList());
                SyncDataJobProcessor.AddJobDetails4ContainerAdded(
                    RMRemoteNodeSourceType.ExchangeOnline, 
                    newMailboxGroupDict.Values.Where(n => n.NodeLevel != NodeLevel.ExchangeOnlineO365GroupGroup).Select(g => g.Name));
            }
            if (updateMailboxGroupDitc.Count > 0)
            {
                List<EmailAccountGroupDto> updateMailboxGroups = updateMailboxGroupDitc.Values.ToList();
                Dictionary<string, RemoteNodePara> updateMailboxGroupCacheDict = ConvertMailboxGroupDictToCacheDict(updateMailboxGroupDitc);
                MailboxRedisService.UpdateGroupToCache(tenantGroupId, updateMailboxGroupCacheDict, () =>
                {
                    MailboxService.UpdateEmailGroups(updateMailboxGroups);
                });
                logger.Info("AdUpdated {0} mailbox containers.", updateMailboxGroupDitc.Count);
                LogSyncDataInfo(updateMailboxGroupDitc.Keys.ToList());
                SyncDataJobProcessor.AddJobDetails4ContainerUpdate(
                    RMRemoteNodeSourceType.ExchangeOnline,
                    updateMailboxGroupDitc.Values.Where(n => n.NodeLevel != NodeLevel.ExchangeOnlineO365GroupGroup).Select(g => g.Name));
            }
            if (newMailboxNameToDtoDict.Count > 0)
            {
                IEnumerable<string> distinctUserNames = newMailboxNameToDtoDict.Values.Where(a => !string.IsNullOrEmpty(a.Username)).Select(a => a.Username).Distinct();
                Dictionary<string, string> userNameToEncryptedStrDict = distinctUserNames.ToDictionary(name => name, name => EncryUserName(name));
                List<EmailAccountDto> newMailboxes = newMailboxNameToDtoDict.Values.ToList();
                Dictionary<string, SyncRemoteNodePara> newMailboxCacheDict = ConvertMailboxToMailboxCacheDict(newMailboxNameToDtoDict, userNameToEncryptedStrDict);
                MailboxRedisService.AddNodesToCache(tenantGroupId, newMailboxCacheDict, () =>
                {
                    newMailboxes.ForEach(m => m.FromDAO = executorContext.InitializedFromDAO);
                    MailboxService.SyncMailboxs(newMailboxes);
                });
                logger.Info("Add {0} mailboxes.", newMailboxNameToDtoDict.Count);
                LogSyncDataInfo(newMailboxNameToDtoDict.Keys.ToList());
                foreach (var item in newMailboxNameToDtoDict)
                {
                    if(item.Value.NodeLevel != NodeLevel.ExchangeOnlineO365Group)
                    {
                        SyncDataJobProcessor.AddJobDetails4Added(RMRemoteNodeSourceType.ExchangeOnline, item.Value.ParentName, item.Key);
                    }
                }
            }
            if (updateObjectsDict.Count > 0)
            {
                List<SyncRemoteNodePara> updateObjects = updateObjectsDict.Values.ToList();
                List<string> distinctUserNames = updateObjects.Where(a => !string.IsNullOrEmpty(a.UserName)).Select(a => a.UserName).Distinct().ToList();
                var userNameToEncrptedUserNameDict = new Dictionary<string, string>();
                distinctUserNames.ForEach(name =>
                {
                    userNameToEncrptedUserNameDict.Add(name, EncryUserName(name));
                });
                if (userNameToEncrptedUserNameDict.Count > 0)
                {
                    foreach (SyncRemoteNodePara cachedNode in updateObjects)
                    {
                        if (!string.IsNullOrEmpty(cachedNode.UserName) && userNameToEncrptedUserNameDict.ContainsKey(cachedNode.UserName))
                        {
                            cachedNode.UserName = userNameToEncrptedUserNameDict[cachedNode.UserName];
                        }
                    }
                }
                MailboxRedisService.UpdateNodesToCache(tenantGroupId, updateObjectsDict, () =>
                {
                    MailboxService.UpdateSyncMails(updateObjects);
                });
                logger.Info("Update {0} mailboxes.", updateObjectsDict.Count);
                LogSyncDataInfo(updateObjectsDict.Keys.ToList());
                foreach (var item in updateObjectsDict)
                {
                    if (item.Value.NodeLevel != NodeLevel.ExchangeOnlineO365Group)
                    {
                        SyncDataJobProcessor.AddJobDetails4Updated(RMRemoteNodeSourceType.ExchangeOnline, item.Value.ParentName, item.Key);
                    }
                }
            }
            if (deleteOjects.Count > 0)
            {
                MailboxRedisService.DeleteNodesFromCache(tenantGroupId, deleteOjects, () =>
                {
                    var parentNames = MailboxService.GetParentNamesByMailboxes(deleteOjects, false);
                    MailboxService.DeleteMailboxByNames(deleteOjects);
                    foreach (var item in parentNames)
                    {
                        SyncDataJobProcessor.AddJobDetails4Removed(RMRemoteNodeSourceType.ExchangeOnline, item.Value, item.Key);
                    }
                });
                logger.Info("Delete {0} mailboxes.", deleteOjects.Count);
                LogSyncDataInfo(deleteOjects);
            }
            logger.Info("Sync mailbox containers and nodes successfully.");
        }

        private Dictionary<string, RemoteNodePara> ConvertMailboxGroupDictToCacheDict(Dictionary<string, EmailAccountGroupDto> dbGroupDict)
        {
            if (dbGroupDict == null || dbGroupDict.Count == 0)
            {
                return new Dictionary<string, RemoteNodePara>();
            }
            var result = new Dictionary<string, RemoteNodePara>();
            foreach (var pair in dbGroupDict)
            {
                result.Add(pair.Key, ConvertDaoGroupModelToCacheModel(pair.Value));
            }
            return result;
        }

        private Dictionary<string, SyncRemoteNodePara> ConvertMailboxToMailboxCacheDict(Dictionary<string, EmailAccountDto> dbMailboxDict, Dictionary<string, string> encryptedUserNameDict)
        {
            if (dbMailboxDict == null || dbMailboxDict.Count == 0)
            {
                return new Dictionary<string, SyncRemoteNodePara>();
            }
            var result = new Dictionary<string, SyncRemoteNodePara>();
            foreach (var pair in dbMailboxDict)
            {
                if (string.IsNullOrEmpty(pair.Value.Username))
                {
                    pair.Value.Username = string.Empty;
                }
                else
                {
                    pair.Value.Username = encryptedUserNameDict.ContainsKey(pair.Value.Username) ? encryptedUserNameDict[pair.Value.Username] : string.Empty;
                }
                var cacheNodeModel = ConvertDaoNodeModelToCacheModel(pair.Value);
                result.Add(pair.Key, cacheNodeModel);
            }
            return result;
        }

        protected override NodeLevel ConvertNodeLevel(RMCompatibleRemoteNode node)
        {
            switch (node.NodeType)
            {
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox:
                    return NodeLevel.ExchangeOnlineMailbox;
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group:
                    return NodeLevel.ExchangeOnlineO365Group;
                default:
                    throw new Exception("Current node type {0} not supported sync.");
            }
        }

        private NodeLevel ConvertGroupNodeLevel(Cloud.Sdk.Data.AosModern.RemoteNodeType nodeType)
        {
            switch (nodeType)
            {
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox:
                    return NodeLevel.ExchangeOnlineMailboxGroup;
                case Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group:
                    return NodeLevel.ExchangeOnlineO365GroupGroup;
                default:
                    throw new ArgumentOutOfRangeException("Current node type {0} not supported mailbox sync.");
            }
        }
    }
}
