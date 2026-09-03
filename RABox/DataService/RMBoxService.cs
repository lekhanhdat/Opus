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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Box.V2.Models;
using RABox.Util;

namespace RABox
{
    public class RMBoxService
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMBoxService));

        private BoxClientContext boxClient;
        private BoxConnectionItem _connectionInfo;
        public readonly string EnterpriseId;
        private string currentUserId = string.Empty;

        public BoxClientContext GetUserContext(string userId)
        {
            if (currentUserId != userId)
            {
                currentUserId = userId;
                boxClient.AsUser(currentUserId);
            }

            return boxClient;
        }

        public RMBoxService(BoxConnectionItem connectionInfo)
        {
            _connectionInfo = connectionInfo;
            boxClient = new BoxClientContext(_connectionInfo);
            EnterpriseId = boxClient.ConnectionInfo.EnterpriseId;
        }

        public List<BoxUserProxy> GetAllUsers()
        {
            var allEnterpriseUsers = new List<BoxUserProxy>();
            var users = boxClient.GetAllUsers();
            var boxUserProxies = users.ConvertAll(u => new BoxUserProxy(boxClient, u));
            foreach (var user in boxUserProxies)
            {
                if (user.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info($"User {user.LoginName} is inactive.");
                    continue;
                }
                else
                {
                    allEnterpriseUsers.Add(user);
                }
            }

            return allEnterpriseUsers.OrderBy(u => u.Name).ToList();
        }

        public BoxFolderProxy GetContainer(Dictionary<string, object> queryParams)
        {
            return null;
        }

        public IAsyncEnumerable<BoxFolderProxy> GetContainers(BoxFolderProxy parent, Dictionary<string, object> queryParams)
        {
            return null;
        }

        public IAsyncEnumerable<BoxFileProxy> GetItems(BoxFolderProxy parent)
        {
            return null;
        }

        public List<BoxItemProxy> GetTrashedItems()
        {
            List<BoxItem> items = boxClient.GetTrashedItems();

            return items.ConvertAll(item => new BoxItemProxy(boxClient, item));
        }

        public BoxFileProxy GetTrashedFile(string id)
        {
            var item = boxClient.GetTrashedFile(id);

            return new BoxFileProxy(boxClient, item);
        }

        public Tuple<Dictionary<Guid, BoxItemProxy>, Dictionary<Guid, BoxItemProxy>> GetModifiedSubItems(BoxFolderProxy scanFolder, ref string lastStreamPosition)
        {
            List<BoxEnterpriseEvent> items = boxClient.GetModifiedSubItems(scanFolder.Id, ref lastStreamPosition);
            var trashedItems = new Dictionary<Guid, BoxItemProxy>();
            var modifiedItems = new Dictionary<Guid, BoxItemProxy>();

            foreach (var item in items)
            {
                if (item.Source is BoxItem boxItem)
                {
                    BoxItemProxy boxItemProxy;

                    if (item.EventType == BoxUtility.TrashedEventType)
                    {
                        boxItemProxy = new BoxItemProxy(boxClient, boxItem);

                        trashedItems[boxItemProxy.UniqueId] = boxItemProxy;
                    }
                    else
                    {
                        BoxFolderProxy parentFolder = scanFolder;

                        if (boxItem.Type == BoxType.file.ToString() && boxItem.CreatedBy.Id == BoxUtility.BoxAnonymousUserId)
                        {
                            boxItem = boxClient.GetFile(boxItem.Id);
                        }
                        else if (boxItem.PathCollection == null || boxItem.PathCollection.TotalCount == 0)
                        {
                            boxItem = boxItem.Type == BoxType.file.ToString() ? boxClient.GetFile(boxItem.Id) : boxClient.GetFolder(boxItem.Id);
                        }

                        var ancestor = boxItem.PathCollection.Entries.SkipWhile(folder => folder.Id != scanFolder.Id).ToList();

                        foreach (var parent in ancestor)
                        {
                            if (parent.Id != scanFolder.Id)
                            {
                                parentFolder = new BoxFolderProxy(boxClient, parent, parentFolder);
                            }
                        }

                        if (boxItem is BoxFile boxFile)
                        {
                            boxItemProxy = new BoxFileProxy(boxClient, boxFile, parentFolder);
                        }
                        else if (boxItem is BoxFolder boxFolder)
                        {
                            if (boxFolder.Id == scanFolder.Id)
                            {
                                boxItemProxy = new BoxFolderProxy(boxClient, boxFolder);
                            }
                            else
                            {
                                boxItemProxy = new BoxFolderProxy(boxClient, boxFolder, parentFolder);
                            }
                        }
                        else
                        {
                            continue;
                        }
                        modifiedItems[boxItemProxy.UniqueId] = boxItemProxy;
                    }
                }
            }

            return new Tuple<Dictionary<Guid, BoxItemProxy>, Dictionary<Guid, BoxItemProxy>>(trashedItems, modifiedItems);
        }

        public string InitStreamPosition()
        {
            return boxClient.InitStreamPosition();
        }

        public JMJobDetailsCommon GenJobDetail(JobType jobType, BoxTreeNode treeNode, RMNodeLevel nodeLevel, string comment = "")
        {
            var detail = new JMJobDetailsCommon();
            var itemType = "";
            switch (nodeLevel)
            {
                case RMNodeLevel.BoxFile:
                    itemType = I18NResource.ObjectLevelDocument; break;
                case RMNodeLevel.BoxFolder:
                    itemType = I18NResource.DataTypeBoxFolder; break;
                case RMNodeLevel.BoxUser:
                    itemType = I18NResource.DataTypeBoxUser; break;
                default:
                    break;
            }

            if (jobType == JobType.BoxDataSynchronisation || jobType == JobType.BoxDataSynchronisationSchedule)
            {
                detail = GenerateSyncActionDetail(treeNode, itemType, comment);
            }

            if (jobType == JobType.BoxRecordsDisposal)
            {
                detail = GenerateDisposalActionJobDetail(treeNode, itemType, comment);
            }

            return detail;
        }

        public JMBoxDataSyncDetail GenerateSyncActionDetail(BoxTreeNode treeNode, string itemType, string comment = "")
        {
            var detail = new JMBoxDataSyncDetail();
            var boxUserEmail = treeNode.FullPath.Split('\\')[0];
            detail.ObjectName = boxUserEmail;
            detail.FullPath = boxUserEmail;
            detail.Comment = comment;
            detail.ItemType = itemType;
            return detail;
        }

        public JMArchiverActionJobDetails GenerateDisposalActionJobDetail(BoxTreeNode treeNode, string itemType, string comment = "")
        {
            var detail = new JMArchiverActionJobDetails();
            var boxUserEmail = treeNode.FullPath.Split('\\')[0];
            detail.SourceLocation = boxUserEmail;
            detail.DestinationLocation = boxUserEmail;
            detail.Size = "0";
            detail.FileSize = 0;
            detail.RuleName = string.Empty;
            detail.Level = itemType;
            detail.ActionTab = (int)ActionTab.Action;
            detail.Action = string.Empty;
            detail.FinishTime = DateTime.UtcNow.Ticks;
            detail.Comment = comment;

            return detail;
        }
    }
}
