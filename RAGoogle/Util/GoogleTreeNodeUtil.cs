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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;

namespace RAGoogle.Util
{
    public class GoogleTreeNodeUtil
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleTreeNodeUtil));
        public static GoogleDriveTreeNodeDto GetContainerNode(GoogleDriveTreeNodeDto node)
        {
            if (IsContainer(node))
            {
                return node;
            }
            if (IsDrive(node))
            {
                return node.Parent;
            }
            return null;
        }

        public static string GetContainerId(GoogleDriveTreeNodeDto node)
        {
            return GetContainerNode(node)?.ContainerId ?? string.Empty;
        }

        public static string GetObjectId(GoogleDriveTreeNodeDto node)
        {
            if (IsContainer(node))
            {
                return node.ContainerId;
            }
            if (IsDrive(node))
            {
                return node.NodeId;
            }
            return string.Empty;
        }

        public static string GetDriveId(GoogleDriveTreeNodeDto node)
        {
            if (IsDrive(node))
            {
                return node.NodeId;
            }
            if (IsContainer(node))
            {
                return string.Empty;
            }
            return string.Empty;
        }

        public static bool IsContainer(GoogleDriveTreeNodeDto node)
        {
            var level = node.Level;
            return (level is NodeLevel.GoogleMyDriveContainer
                 || level is NodeLevel.GoogleSharedDriveContainer);
        }

        public static bool IsDrive(GoogleDriveTreeNodeDto node)
        {
            var level = node.Level;
            return (level is NodeLevel.GoogleMyDrive
                 || level is NodeLevel.GoogleSharedDrive);
        }

        public static bool IsSharedDrive(GoogleDriveTreeNodeDto node)
        {
            var level = node.Level;
            return level is NodeLevel.GoogleSharedDrive;
        }

        public static bool IsMyDrive(GoogleDriveTreeNodeDto node)
        {
            var level = node.Level;
            return level is NodeLevel.GoogleMyDrive;
        }
        
        public static string GenerateArchiveJobMonitorExtension(RMGoogleTreeNode selectNode, TreeMode treeMode, List<string> driveIds = null,bool useImportSite = false)
        {
            ArchiveGoogleJobMonitorExtension extension = new();
            extension.TreeMode = treeMode;
            if (selectNode.Level is (int)NodeLevel.GoogleMyDriveContainer  or (int)NodeLevel.GoogleSharedDriveContainer)
            {
                extension.IsDriveContainer = true;
                extension.ContainerNode = selectNode;
            }
            else
            {
                extension.IsDriveContainer = false;
                if (driveIds != null)
                {
                    extension.DriveIds = driveIds;
                }
                else
                {
                    extension.DriveIds = [selectNode.ObjectId];
                }
            }
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }
    }
}
