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
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.SourceTreeQuery;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using AvePoint.RA.Service.Services.AzureFileShare.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SourceTreeQuery
{
    public class AzureFileShareTreeQuerier : SourceTreeQuerier<AzureFileShareTreeNode>, IAzureFileShareQuery
    {
        public override SourceFlag Flag => SourceFlag.AzureFileShare;

        private static readonly Guid AzureFileShareRootId = new Guid("904A825C-CF8E-4310-97E1-C9E011A6B5A0");

        private static IRMAzureFileShareConnectionGroupService AzureFileShareConnectionGroupService =>
            PlatformWindsorManager.GetService<IRMAzureFileShareConnectionGroupService>();

        private static IRMAzureFileShareConnectionService AzureFileShareConnectionService =>
            PlatformWindsorManager.GetService<IRMAzureFileShareConnectionService>();

        private static IAzureFileShareSettingDao AzureFileShareSettingDao => new AzureFileShareSettingDao();

        protected override Dictionary<RMNodeLevel, Func<AzureFileShareTreeNode, Task<IEnumerable<AzureFileShareTreeNode>>>> LevelContainerQueries =>
            new Dictionary<RMNodeLevel, Func<AzureFileShareTreeNode, Task<IEnumerable<AzureFileShareTreeNode>>>>
            {
                { RMNodeLevel.Root, GetChildrenContainersUnderRootAsync },
                { RMNodeLevel.AzureFileShareGroup, GetChildrenConnectionsUnderGroupAsync},
                { RMNodeLevel.AzureFileShareConnection,  GetChildrenFoldersUnderConnectionAsync },
                { RMNodeLevel.AzureFileShareDirectory, GetChildrenFoldersUnderFolderAsync },
            };

        protected override Dictionary<RMNodeLevel, Func<PagingSourceTreeNode<AzureFileShareTreeNode>, PagingSourceTreeNode<AzureFileShareTreeNode>>> LevelContainerPagingQueries =>
            new Dictionary<RMNodeLevel, Func<PagingSourceTreeNode<AzureFileShareTreeNode>, PagingSourceTreeNode<AzureFileShareTreeNode>>>();

        protected override Dictionary<RMNodeLevel, Func<PagingSourceTreeNode<AzureFileShareTreeNode>, PagingSourceTreeNode<AzureFileShareTreeNode>>> LevelItemPagingQueriers =>
            new Dictionary<RMNodeLevel, Func<PagingSourceTreeNode<AzureFileShareTreeNode>, PagingSourceTreeNode<AzureFileShareTreeNode>>>();

        public override AzureFileShareTreeNode GetRootNode()
        {
            return new AzureFileShareTreeNode 
            {
                Id = AzureFileShareRootId.ToString(),
                RealId = AzureFileShareRootId.ToString(),
                Level = RMNodeLevel.Root,
                LeafName = I18NEntity.GetString("RM_JS_SPS_AZ_RootNode"),
                DisplayName = I18NEntity.GetString("RM_JS_SPS_AZ_RootNode"),
                FullPath = I18NEntity.GetString("RM_JS_SPS_AZ_RootNode"),
            };

        }

        public async Task<IEnumerable<AzureFileShareTreeNode>> GetChildrenContainersUnderRootAsync(AzureFileShareTreeNode node)
        {
            var groups = await AzureFileShareConnectionGroupService.GetAllAsync();
            return groups.ConvertAll(item => new AzureFileShareTreeNode 
            { 
                Id = item.Id.ToString(),
                RealId = item.Id.ToString(),
                LeafName = item.Name,
                DisplayName = item.Name,
                FullPath = item.Name,
                Level = RMNodeLevel.AzureFileShareGroup,
                ContainerId = item.Id.ToString(),
            });

        }

        public async Task<IEnumerable<AzureFileShareTreeNode>> GetChildrenConnectionsUnderGroupAsync(AzureFileShareTreeNode node)
        {
            var connections = await AzureFileShareConnectionService.GetAllByConnectionGroupAsync(new Guid(node.Id));
            return connections.ConvertAll(item => new AzureFileShareTreeNode
            {
                Id = item.Id.ToString(),
                RealId = item.Id.ToString(),
                LeafName = item.Name,
                DisplayName = item.Name,
                RelativePath = "",
                FullPath = item.Name,
                Level = RMNodeLevel.AzureFileShareConnection,
                ConnectionId = item.Id,
                ContainerId = node.Id,
            });
        }

        public async Task<IEnumerable<AzureFileShareTreeNode>> GetChildrenFoldersUnderConnectionAsync(AzureFileShareTreeNode node)
        {
            var connection = await AzureFileShareConnectionService.GetAsync(new Guid(node.Id));
            var connectionInfo = AzureFileShareConnectionConverter.ConvertToConnectionInfo(connection);
            var apiContext = new AzureFileShareApiContext(connectionInfo);
            var rootDirectory = new AzureFileShareApiDirectoryClient(apiContext, new Uri(new Uri(connectionInfo.AccessEndPoint), connectionInfo.FileShareName).ToString());
            var subDirectories = rootDirectory.SubDirectories;
            return subDirectories.ConvertAll(item => new AzureFileShareTreeNode
            {
                Id = item.Id.ToString(),
                RealId = item.RealId,
                LeafName = item.Name,
                DisplayName = item.Name,
                FullPath = item.FullPath,
                Level = RMNodeLevel.AzureFileShareDirectory,
                RelativePath = item.RelativePath,
                ConnectionId = connection.Id,
                ContainerId = node.ContainerId,
            });
        }

        public async Task<IEnumerable<AzureFileShareTreeNode>> GetChildrenFoldersUnderFolderAsync(AzureFileShareTreeNode node)
        {
            var connection = await AzureFileShareConnectionService.GetAsync(node.ConnectionId);
            var connectionInfo = AzureFileShareConnectionConverter.ConvertToConnectionInfo(connection);
            var apiContext = new AzureFileShareApiContext(connectionInfo);
            var directory = new AzureFileShareApiDirectoryClient(apiContext, node.FullPath);
            var subDirectories = directory.SubDirectories;
            return subDirectories.ConvertAll(item => new AzureFileShareTreeNode
            {
                Id = item.Id.ToString(),
                RealId = item.RealId,
                LeafName = item.Name,
                DisplayName = item.Name,
                FullPath = item.FullPath,
                Level = RMNodeLevel.AzureFileShareDirectory,
                RelativePath = item.RelativePath,
                ConnectionId = connection.Id,
                ContainerId = node.ContainerId,
            });
        }
        protected override bool HasSetting(AzureFileShareTreeNode node)

        {
            return AzureFileShareSettingDao.Exist(item => item.ScopeId == new Guid(node.Id));
        }

    }
}
