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

using System;
namespace AvePoint.Wrapper.Restore
{
    public interface IAveObjectSecurity
    {
        AvePoint.Wrapper.Common.IReport GetReport();
        AvePoint.Wrapper.Common.IAveRoleDefinition GetRoleWithCache(int oldId, IAveSPWeb aveWeb);
        AvePoint.Wrapper.Common.IAveUser GetSPUser(int principalId, IAveSPWeb aveSPWeb);
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<AvePoint.Wrapper.Common.AveRoleAssignmentInfo>> GroupRoleAssignmentInfos(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveRoleAssignmentInfo> roleAssignmentInfos);
        IAveSPSite ParentSite { get; }
        void Restore(AvePoint.Wrapper.Common.AveMemberInfoCollection memeberInfoCol, SecurityRestoreOption restoreOption);
        void Restore(AvePoint.Wrapper.Common.AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption);
        int RestoreRole(AvePoint.Wrapper.Common.AveRoleInfo roleInfo, IAveSPWeb aveWeb);
        void RestoreRoleAssignments(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption);
        void RestoreRoles(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveRoleInfo> roleInfos);
        void RestoreRoles(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveRoleInfo> roleInfos, SecurityRestoreOption restoreOption);
        bool SourceHasUniqueRoleAssignment { get; set; }
    }

    public class SecurityRestoreOption
    {
        public bool MergePermissionFromInheritanceWeb = false;
        public bool NeedRestore = true;
        [Obsolete("use ConflictResolutionForPincipal instead ")]
        public bool OverWritePermission //对某个user的permission的控制
        {
            set
            {
                ConflictResolutionForPincipal = value ? ConflictResolutionForPincipal.OverWrite : ConflictResolutionForPincipal.Merge;
            }
            get
            {
                return ConflictResolutionForPincipal == ConflictResolutionForPincipal.OverWrite;
            }
        }
        [Obsolete("use ConflictResolutionForSecurityObject instead")]
        public bool OverWriteItemPermission //对web、list、item级别的所有permission的控制
        {
            set
            {
                ConflictResolutionForSecurityObject = value ? ConflictResolutionForSecurityObject.OverWrite : ConflictResolutionForSecurityObject.Merge;
            }
            get
            {
                return ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite;
            }
        }

        public ConflictResolutionForSecurityObject ConflictResolutionForSecurityObject { set; get; }
        public ConflictResolutionForPincipal ConflictResolutionForPincipal { set; get; }
        public bool PromotePermissionToRootWeb { set; get; }
    }

    public enum ConflictResolutionForSecurityObject
    {
        Merge = 0,
        OverWrite
        //MergefromInherited
    }

    public enum ConflictResolutionForPincipal
    {
        Merge = 0,
        OverWrite
    }
}
