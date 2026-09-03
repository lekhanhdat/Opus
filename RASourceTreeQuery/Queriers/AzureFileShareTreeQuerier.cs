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
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using AvePoint.RA.Service.Services.AzureFileShare.Converters;
using RASourceTreeQuery.Model;
using RASourceTreeQuery.Queriers.IQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RASourceTreeQuery.Queriers
{
    public class AzureFileShareTreeQuerier : SourceTreeQuerier<AzureFileShareTreeNode>, IAzureFileShareQuery
    {
        public override SourceFlag Flag => SourceFlag.AzureFileShare;

        public static readonly Guid AzureFileShareRootId = new Guid("904A825C-CF8E-4310-97E1-C9E011A6B5A0");

        public static readonly IRMAzureFileShareConnectionGroupService AzureFileShareConnectionGroupService =
            PlatformWindsorManager.GetService<IRMAzureFileShareConnectionGroupService>();

        public static readonly IRMAzureFileShareConnectionService AzureFileShareConnectionService =
            PlatformWindsorManager.GetService<IRMAzureFileShareConnectionService>();

        protected override Dictionary<RMNodeLevel, Func<AzureFileShareTreeNode, IEnumerable<AzureFileShareTreeNode>>> LevelContainerQueries =>
            new Dictionary<RMNodeLevel, Func<AzureFileShareTreeNode, IEnumerable<AzureFileShareTreeNode>>>
            {
                { RMNodeLevel.Root, GetChildrenContainersUnderRoot },
                { RMNodeLevel.AzureFileShareGroup, GetChildrenConnectionsUnderGroup},
                { RMNodeLevel.AzureFileShareConnection, GetChildrenFoldersUnderConnection },
                { RMNodeLevel.AzureFileShareDirectory, GetChildrenFoldersUnderFolder },
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
                LeafName = "^Azure File Share Connection Groups",
                DisplayName = "^Azure File Share Connection Groups",
                FullPath = "^Azure File Share Connection Groups",
            };

        }

        public IEnumerable<AzureFileShareTreeNode> GetChildrenContainersUnderRoot(AzureFileShareTreeNode node)
        {
            var groups = AzureFileShareConnectionGroupService.GetAll();
            return groups.ConvertAll(item => new AzureFileShareTreeNode 
            { 
                Id = item.Id.ToString(),
                RealId = item.Id.ToString(),
                LeafName = item.Name,
                DisplayName = item.Name,
                FullPath = item.Name,
                Level = RMNodeLevel.AzureFileShareGroup
            });

        }

        public IEnumerable<AzureFileShareTreeNode> GetChildrenConnectionsUnderGroup(AzureFileShareTreeNode node)
        {
            var connections = AzureFileShareConnectionService.GetAllByConnectionGroup(new Guid(node.Id));
            return connections.ConvertAll(item => new AzureFileShareTreeNode
            {
                Id = item.Id.ToString(),
                RealId = item.Id.ToString(),
                LeafName = item.Name,
                DisplayName = item.Name,
                RelativePath = "",
                FullPath = item.Name,
                Level = RMNodeLevel.AzureFileShareConnection
            });
        }

        public IEnumerable<AzureFileShareTreeNode> GetChildrenFoldersUnderConnection(AzureFileShareTreeNode node)
        {
            var connection = AzureFileShareConnectionService.Get(new Guid(node.Id));
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
                RelativePath = "",
                ConnectionId = connection.Id
            });
        }

        public IEnumerable<AzureFileShareTreeNode> GetChildrenFoldersUnderFolder(AzureFileShareTreeNode node)
        {
            var connection = AzureFileShareConnectionService.Get(node.ConnectionId);
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
                ConnectionId = connection.Id
            });
        }

        protected override bool HasSetting(AzureFileShareTreeNode node)
        {
            return false;
        }

    }
}
