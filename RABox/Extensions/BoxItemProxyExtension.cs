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
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using RABox.Util;

namespace RABox.Extensions
{
    public static class BoxItemProxyExtension
    {
        public static JMBoxDataSyncDetail GenerateSyncActionDetail(this BoxItemProxy item, BoxTreeNode scanNode, string comment = "")
        {
            var detail = new JMBoxDataSyncDetail();
            detail.ObjectName = item.Name;
            detail.FullPath = item.Id == scanNode.RealId || item.Id == BoxUtility.BoxRootFolderId ? scanNode.FullPath : item.CombinePath(scanNode.FullPath, item.FullPath);
            detail.Comment = comment;
            detail.ItemType = item.Type == "folder" ? I18NResource.DataTypeBoxFolder : I18NResource.ObjectLevelDocument;
            return detail;
        }

        public static SyncFailureItemEntity GenerateFailureItemEntity(this BoxItemProxy item, BoxTreeNode scanNode, string jobId)
        {
            var entity = new SyncFailureItemEntity(scanNode.Id, item.UniqueId.ToString())
            {
                DataSource = (int)SourceFlag.Box,
                FullPath = item.Id == scanNode.RealId || item.Id == BoxUtility.BoxRootFolderId ? scanNode.FullPath : item.CombinePath(scanNode.FullPath, item.FullPath),
                ParentId = item.Parent.UniqueId.ToString(),
                NodeId = item.Id,
                ContainerId = scanNode.ConnectionId.ToString(),
                OwnerId = scanNode.OwnerId.ToString(),
                IsDirectory = item.Type == "folder",
                JobId = jobId,
            };

            return entity;
        }
    }
}
