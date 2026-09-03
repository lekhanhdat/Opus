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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Box.Converters;
using RABox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box
{
    public class RMBoxBrowser : IRMBoxBrowser
    {
        private static RALogger logger = new RALogger(typeof(RMBoxBrowser));

        private IRMBoxConnectionGroupDao BoxConnectionGroupDao => PlatformWindsorManager.GetService<IRMBoxConnectionGroupDao>();

        private IRMBoxConnectionDao BoxConnectionDao => PlatformWindsorManager.GetService<IRMBoxConnectionDao>();

        public async Task<IEnumerable<BoxTreeNode>> BrowseAsync(BoxTreeNode node)
        {
            switch (node.Level)
            {
                case RMNodeLevel.Root:
                    return GetConnectionGroupNode(node);
                case RMNodeLevel.BoxConnectionGroup:
                    return GetConnectionNode(node);
                case RMNodeLevel.BoxConnection:
                    return GetBoxUserNode(node);
                case RMNodeLevel.BoxUser:
                    return await GetRootFolderNodeAsync(node);
                case RMNodeLevel.BoxFolder:
                    return await GetFolderNodeAsync(node);
                default:
                    return null;
            }
        }

        public IEnumerable<BoxTreeNode> GetBoxUserNode(BoxTreeNode node)
        {
            try
            {
                var connectionId = new Guid(node.Id);
                var connection = BoxConnectionDao.GetById(connectionId);
                var groupId = connection.ConnectionGroupId;
                var connectionItem = BoxConnectionConverter.ConvertToItem(connection);
                var boxService = new RMBoxService(connectionItem);
                var users = boxService.GetAllUsers();
                if (users == null || !users.Any())
                {
                    logger.Error($"No user nodes retrieved for the selected node. Node ID: [{node.Id}].");
                    return new List<BoxTreeNode>();
                }

                return users.ConvertAll(u => new BoxTreeNode
                {
                    Id = u.UniqueId.ToString(),
                    RealId = u.Id,
                    ContainerId = groupId.ToString(),
                    OwnerId = u.Id,
                    ConnectionId = connectionId.ToString(),
                    Parent = node,
                    LeafName = u.LoginName,
                    DisplayName = u.Name,
                    FullPath = u.LoginName,
                    Level = RMNodeLevel.BoxUser,
                });
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when retrieved user nodes by selected, node ID: [{node.Id}], Error: {ex}", ex);
                return new List<BoxTreeNode>();
            }
        }

        public IEnumerable<BoxTreeNode> GetConnectionGroupNode(BoxTreeNode node)
        {
            var connectionGroups = BoxConnectionGroupDao.GetAll();
            if (!connectionGroups.Any())
            {
                logger.Error($"No connection group nodes retrievedby selected node, node ID: [{node.Id}].");
                return new List<BoxTreeNode>();
            }
            return connectionGroups.OrderBy(cg => cg.Name).ConvertAll(item => new BoxTreeNode
            {
                Id = item.Id.ToString(),
                RealId = Guid.Empty.ToString(),
                ConnectionId = Guid.Empty.ToString(),
                ContainerId = Guid.Empty.ToString(),
                OwnerId = Guid.Empty.ToString(),
                Parent = node,
                LeafName = item.Name,
                DisplayName = item.Name,
                FullPath = item.Name,
                Level = RMNodeLevel.BoxConnectionGroup,
            });
        }

        public IEnumerable<BoxTreeNode> GetConnectionNode(BoxTreeNode node)
        {
            var groupId = new Guid(node.Id);
            var connections = BoxConnectionDao.GetAllByConnectionGroup(groupId);
            if (!connections.Any())
            {
                logger.Error($"No connection nodes retrieved by selected node, node ID: [{node.Id}].");
                return new List<BoxTreeNode>();
            }

            return connections.OrderBy(c => c.Name).ConvertAll(item => new BoxTreeNode
            {
                Id = item.Id.ToString(),
                RealId = Guid.Empty.ToString(),
                ContainerId = groupId.ToString(),
                OwnerId = Guid.Empty.ToString(),
                ConnectionId = item.Id.ToString(),
                Parent = node,
                LeafName = item.Name,
                DisplayName = item.Name,
                FullPath = $@"{node.LeafName}\{item.Name}",
                Level = RMNodeLevel.BoxConnection,
            });
        }

        public async Task<IEnumerable<BoxTreeNode>> GetFolderNodeAsync(BoxTreeNode node)
        {
            try
            {
                var connectionId = new Guid(node.ConnectionId);
                var connection = BoxConnectionDao.GetById(connectionId);
                var connectionItem = BoxConnectionConverter.ConvertToItem(connection);
                if (connection == null)
                {
                    logger.Error($"No folder nodes retrieved by selected node, node ID: [{node.Id}].");
                    return new List<BoxTreeNode>();
                }
                var boxClientContext = new BoxClientContext(connectionItem);
                var result = new List<BoxTreeNode>();
                boxClientContext.AsUser(node.OwnerId);
                var boxFolderProxy = new BoxFolderProxy(boxClientContext, node.RealId);
                var subFolders = boxFolderProxy.GetSubFolders();
                if (subFolders != null && subFolders.Any())
                {
                    result.AddRange(subFolders.ConvertAll(subFolder => new BoxTreeNode
                    {
                        Id = subFolder.UniqueId.ToString(),
                        ContainerId = node.ContainerId,
                        RealId = subFolder.Id,
                        DisplayName = subFolder.Name,
                        OwnerId = node.OwnerId,
                        ConnectionId = connectionId.ToString(),
                        Parent = node,
                        LeafName = subFolder.Name,
                        Level = RMNodeLevel.BoxFolder,
                        FullPath = BuildFolderNodeFullPath(node, subFolder.Name),
                    }));
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when retrieved folder nodes by selected node, node ID: [{node.Id}], Error: {ex}", ex);
                return new List<BoxTreeNode>();
            }

        }

        public async Task<IEnumerable<BoxTreeNode>> GetRootFolderNodeAsync(BoxTreeNode node)
        {
            try
            {
                var connectionId = new Guid(node.ConnectionId);
                var connection = BoxConnectionDao.GetById(connectionId);
                var connectionItem = BoxConnectionConverter.ConvertToItem(connection);
                if (connection == null)
                {
                    logger.Error($"No root folder nodes retrieved by selected node, node ID: [{node.Id}].");
                    return new List<BoxTreeNode>();
                }
                var boxClientContext = new BoxClientContext(connectionItem);
                var result = new List<BoxTreeNode>();
                boxClientContext.AsUser(node.OwnerId);
                var boxFolderProxy = new BoxFolderProxy(boxClientContext, BoxUtility.BoxRootFolderId);
                var folders = boxFolderProxy.GetSubFolders();

                if (folders != null && folders.Any())
                {
                    result.AddRange(folders.ConvertAll(f => new BoxTreeNode
                    {
                        Id = f.UniqueId.ToString(),
                        ContainerId = node.ContainerId,
                        RealId = f.Id,
                        OwnerId = node.OwnerId,
                        ConnectionId = connectionId.ToString(),
                        DisplayName = f.Name,
                        Parent = node,
                        LeafName = f.Name,
                        Level = RMNodeLevel.BoxFolder,
                        FullPath = BuildFolderNodeFullPath(node, f.Name),
                    }));
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when retrieved rootfolder nodes by selected node, node ID: [{node.Id}], Error: {ex}", ex);
                return new List<BoxTreeNode>();
            }
        }

        public BoxTreeNode GetRootNode()
        {
            return new BoxTreeNode()
            {
                Level = RMNodeLevel.Root,
                DisplayName = "Connection groups",
                LeafName = "Connection groups",
                FullPath = "Connection groups",
                Name = "Connection groups",
                Id = RecordsConstants.BOX_ROOT_GUID.ToString()
            };
        }

        private string BuildFolderNodeFullPath(BoxTreeNode parentNode, string folderName)
        {
            StringBuilder fullPath = new StringBuilder();

            fullPath.Append(folderName);

            var tempParentNode = parentNode;

            while (tempParentNode.Level != RMNodeLevel.BoxConnection)
            {
                fullPath.Insert(0, $@"{tempParentNode.LeafName}\");
                tempParentNode = tempParentNode.Parent;
            }

            return fullPath.ToString();
        }
    }
}
