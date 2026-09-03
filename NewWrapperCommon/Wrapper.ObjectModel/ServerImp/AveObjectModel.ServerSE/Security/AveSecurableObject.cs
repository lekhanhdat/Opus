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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    public abstract class AveSecurableObject : AveServerObject, IAveSecurableObject
    {
        private SPSecurableObject mSecurableObject;

        public AveSecurableObject(SPSecurableObject securableObj)
        {
            Reload(securableObj);
        }

        protected void Reload(SPSecurableObject securableObj)
        {
            mSecurableObject = securableObj;
        }

        #region IAveSecurableObject Members

        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            mSecurableObject.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
            if (!copyRoleAssignments)
            {
                int count = mSecurableObject.RoleAssignments.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    mSecurableObject.RoleAssignments.Remove(i);
                }
            }
        }

        public void BreakRoleInheritance(bool copyRoleAssignments)
        {
            mSecurableObject.BreakRoleInheritance(copyRoleAssignments);
            if (!copyRoleAssignments)
            {
                int count = mSecurableObject.RoleAssignments.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    mSecurableObject.RoleAssignments.Remove(i);
                }
            }
        }

        public void ResetRoleInheritance()
        {
            mSecurableObject.ResetRoleInheritance();
        }

        public bool HasUniqueRoleAssignments
        {
            get { return mSecurableObject.HasUniqueRoleAssignments; }
        }

        public IAveRoleAssignmentCollection RoleAssignments
        {
            get
            {
                return this.SecurableObjectImpl.RoleAssignments;
            }
        }

        public bool DoesUserHavePermissions(AveBasePermissions permissionMask)
        {
            return mSecurableObject.DoesUserHavePermissions((SPBasePermissions)permissionMask);
        }

        public abstract IAveSecurableObjectImpl SecurableObjectImpl
        { get; }

        public IAvePermissionInfo GetUserEffectivePermissionInfo(string userName)
        {
            SPPermissionInfo permissionInfo = mSecurableObject.GetUserEffectivePermissionInfo(userName);
            if (permissionInfo == null)
            {
                return null;
            }
            return new AvePermissionInfo(this, permissionInfo);
        }

        public AveBasePermissions GetUserEffectivePermissions(string userName)
        {
            return (AveBasePermissions)mSecurableObject.GetUserEffectivePermissions(userName);
        }

        #endregion
    }
}
