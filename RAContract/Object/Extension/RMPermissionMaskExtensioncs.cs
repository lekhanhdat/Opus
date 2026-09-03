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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Object.Extension
{
    public static class RMPermissionMaskExtensioncs
    {
        /// <summary>
        /// Remove the sourceFlag from sourceFlages list if has no related permission
        /// </summary>
        /// <param name="userPermission"></param>
        /// <param name="sourceFlags"></param>
        /// <returns></returns>
        public static List<SourceFlag> RemoveNoPermissionFourceFlags(this RMPermissionMasks userPermission, List<SourceFlag> sourceFlags)
        {
            if (sourceFlags == null) return null;

            //if (!RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled()) return sourceFlags;

            //var userPermission = SecurityTrimmingHelper.GetCurrentUserPermission();
            var managerHoldPermission = userPermission.HasPermission(RMPermissionMasks.ManageHold);
           
            if (!userPermission.HasPermission(RMPermissionMasks.SPOEnduser) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => s== SourceFlag.SharePoint);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.SPOnPremEnduser) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => s == SourceFlag.SharePointOnPrem);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.OneDriveEnduser) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => s == SourceFlag.OneDrive);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.EXOEnduser) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => s == SourceFlag.Exchange);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.FSEnduser) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => s == SourceFlag.FileSystem);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.PhysicalEndUser) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => s == SourceFlag.Physical);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.ControlPanelAdmin) && !managerHoldPermission)
            {
                sourceFlags.RemoveAll(s => (int)s >= 1000);
            }

            return sourceFlags;
        }

        public static List<int> RemoveNoPermissionNodeTypes(this RMPermissionMasks userPermission, List<int> nodeTypes)
        {
            if (nodeTypes == null) return null;

            //if (!userPermission.HasPermission(RMPermissionMasks.SPOEnduser) && nodeTypes.Contains((int)NodeLevel.Item))
            //{
            //    nodeTypes.Remove((int)NodeLevel.Item);
            //}
            if (!userPermission.HasPermission(RMPermissionMasks.SPOEnduser) 
                && !userPermission.HasPermission(RMPermissionMasks.OneDriveEnduser))
            {
                nodeTypes.RemoveAll(n => n == (int)RMNodeLevel.Item);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.PhysicalEndUser))
            {
                nodeTypes.RemoveAll(n => n == (int)RMNodeLevel.PhysicalBox);
                nodeTypes.RemoveAll(n => n == (int)RMNodeLevel.PhysicalFile);
                nodeTypes.RemoveAll(n => n == (int)RMNodeLevel.PhysicalRecord);
            }

            return nodeTypes;
        }

        public static List<RMNodeLevel> RemoveNoPermissionNodeTypes(this RMPermissionMasks userPermission, List<RMNodeLevel> nodeTypes)
        {
            if (nodeTypes == null) return null;

            if (!userPermission.HasPermission(RMPermissionMasks.SPOEnduser)
                && !userPermission.HasPermission(RMPermissionMasks.OneDriveEnduser)
                && !userPermission.HasPermission(RMPermissionMasks.SPOnPremEnduser))
            {
                nodeTypes.RemoveAll(n => n == RMNodeLevel.Item);
            }

            if (!userPermission.HasPermission(RMPermissionMasks.PhysicalEndUser))
            {
                nodeTypes.RemoveAll(n => n == RMNodeLevel.PhysicalBox);
                nodeTypes.RemoveAll(n => n == RMNodeLevel.PhysicalFile);
                nodeTypes.RemoveAll(n => n == RMNodeLevel.PhysicalRecord);
            }

            return nodeTypes;
        }
    }
}
