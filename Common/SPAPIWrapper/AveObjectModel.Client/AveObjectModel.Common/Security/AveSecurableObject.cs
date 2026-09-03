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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    abstract class AveSecurableObject : AveClientObject, IAveSecurableObject
    {
        protected IAveRequest mRequest;

        public AveSecurableObject(IAveRequest request)
        {
            mRequest = request;
        }

        internal abstract void InitRoleAssignmentProperties(Dictionary<string, object> roleAssignmentProperties);

        internal abstract Dictionary<string, object> AddRoleAssignment(Dictionary<string, object> roleAssignmentProperties);

        internal abstract Dictionary<string, object> UpdateRoleAssignment(int principalId, Dictionary<string, object> roleAssignmentProperties);

        protected abstract IAveRoleAssignmentCollection InternalBreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes);

        protected abstract IAveRoleAssignmentCollection InternalResetRoleInheritance();

        #region IAveSecurableObject Members

        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            AveRoleAssignmentCollection roleAssignmentCol = InternalBreakRoleInheritance(copyRoleAssignments, clearSubscopes) as AveRoleAssignmentCollection;
            base.DataCache.AddProperty("HasUniqueRoleAssignments",true);
            base.DataCache.AddProperty("RoleAssignments",roleAssignmentCol);
        }

        public void BreakRoleInheritance(bool copyRoleAssignments)
        {
            BreakRoleInheritance(copyRoleAssignments, false);
        }

        public void ResetRoleInheritance()
        {
            AveRoleAssignmentCollection roleAssignmentCol = InternalResetRoleInheritance() as AveRoleAssignmentCollection;
            base.DataCache.AddProperty("HasUniqueRoleAssignments", false);
            base.DataCache.AddProperty("RoleAssignments", roleAssignmentCol);
        }

        public abstract void RemoveRoleAssignment(int principalId);

        public bool HasUniqueRoleAssignments
        {
            get { return base.DataCache.GetProperty<bool>("HasUniqueRoleAssignments"); }
        }

        public abstract IAveRoleAssignmentCollection RoleAssignments
        {
            get;
        }

        #endregion

        public bool DoesUserHavePermissions(AveBasePermissions permissionMask)
        {
            if (this is IAveSite)
            {
                return mRequest.DoesUserHavePermissions((this as IAveSite).RootWeb.ServerRelativeUrl, (int)permissionMask);
            }
            else if (this is IAveWeb)
            {
                return mRequest.DoesUserHavePermissions((this as IAveWeb).ServerRelativeUrl, (int)permissionMask);
            }
            else if (this is IAveList)
            {
                return mRequest.DoesUserHavePermissions((this as IAveList).ParentWeb.ServerRelativeUrl, (int)permissionMask);
            }
            else if (this is IAveListItem)
            {
                return mRequest.DoesUserHavePermissions((this as IAveListItem).Web.ServerRelativeUrl, (int)permissionMask);
            }
            throw new NotImplementedException();
        }

        public IAvePermissionInfo GetUserEffectivePermissionInfo(string userName)
        {
            throw new NotImplementedException();
        }

        public AveBasePermissions GetUserEffectivePermissions(string userName)
        {
            AveBasePermissions basePermissions = AveBasePermissions.EmptyMask;
            if (this is IAveSite)
            {
                basePermissions = mRequest.GetUserEffectivePermissions("Site", (this as IAveSite).RootWeb.ServerRelativeUrl, (this as IAveSite).ID, userName);
            }
            else if (this is IAveWeb)
            {
                basePermissions = mRequest.GetUserEffectivePermissions("Web", (this as IAveWeb).ServerRelativeUrl, (this as IAveWeb).ID, userName);
            }
            else if (this is IAveList)
            {
                basePermissions = mRequest.GetUserEffectivePermissions("List", (this as IAveList).ParentWeb.ServerRelativeUrl, (this as IAveList).ID, userName);
            }
            else if (this is IAveListItem)
            {
                basePermissions = mRequest.GetUserEffectivePermissions("Item", (this as IAveListItem).Web.ServerRelativeUrl, (this as IAveListItem).ParentList.ID, userName, (this as IAveListItem).ID);
            }
            else
            {
                throw new NotImplementedException();
            }
            return basePermissions;
        }

        public abstract IAveSecurableObjectImpl SecurableObjectImpl
        {
            get;
        }
    }
}
