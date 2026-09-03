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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings
{
    public static class RMFileSystemTreeNodeFactory
    {
        public static RMFSTreeNode Create<T>(T item, RMFSTreeNode parent, NodeLevel level)
        {
            var child = new RMFSTreeNode
            {
                Parent = parent,
                ParentId = parent.Id.ToString(),
                Level = (int)level
            };

            switch (level)
            {
                case NodeLevel.WebApplication:
                    MapGroupNode(child, item as FSConnectionGroup);
                    break;
                case NodeLevel.SiteCollection:
                    MapCollectionNode(child, parent, item as FSConnection);
                    break;
                case NodeLevel.FSFolder:
                    MapFolderNode(child, parent, item as HBTreeNode);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported NodeLevel: {level}");
            }

            return child;
        }

        private static void MapGroupNode(RMFSTreeNode child, FSConnectionGroup item)
        {
            child.Id = item.Id;
            child.Name = item.Name;
            child.ConnGroupId = item.Id;
            child.FullPath = item.Name;
        }

        private static void MapCollectionNode(RMFSTreeNode child, RMFSTreeNode parent, FSConnection item)
        {
            child.Id = item.Id;
            child.Name = item.Name;
            child.AgentId = item.AgentId;
            child.FullPath = item.UNCPath;
            child.ConnGroupId = parent.ConnGroupId;
            child.PathType = item.PathType;
            child.IsPause = item.IsPause;
        }

        private static void MapFolderNode(RMFSTreeNode child, RMFSTreeNode parent, HBTreeNode item)
        {
            child.Id = item.Url.ToLowerInvariant().ToMd5();
            child.Name = item.Name;
            child.FullPath = item.Url;
            child.ConnGroupId = parent.ConnGroupId;
            child.AgentId = parent.AgentId;
        }
    }
}
