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
using Microsoft.SharePoint;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.ServerSE
{
    class AvePermissionInfo : IAvePermissionInfo
    {
        private SPPermissionInfo mPermissionInfo;
        private Collection<IAveRoleAssignment> mRoleAssignments;
        private AveSecurableObject mSecurableObject;

        public AvePermissionInfo(AveSecurableObject sObj, SPPermissionInfo permissionInfo)
        {
            mPermissionInfo = permissionInfo;
            mSecurableObject = sObj;
        }

        public AveBasePermissions Permissions
        {
            get
            {
                return (AveBasePermissions)mPermissionInfo.Permissions;
            }
        }

        public Collection<IAveRoleAssignment> RoleAssignments
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Server.AvePermissionInfo.Get_RoleAssignments"))
                {
                    if (mRoleAssignments == null)
                    {
                        Collection<SPRoleAssignment> spRoleAssignments = mPermissionInfo.RoleAssignments;
                        if (spRoleAssignments != null)
                        {
                            mRoleAssignments = new Collection<IAveRoleAssignment>();
                            foreach (SPRoleAssignment roleAgt in spRoleAssignments)
                            {
                                IAveWeb parentWeb = null;
                                if (mSecurableObject is AveList)
                                {
                                    AveList list = (AveList)mSecurableObject;
                                    parentWeb = list.ParentWeb;
                                }
                                else if (mSecurableObject is AveListItem)
                                {
                                    AveListItem item = (AveListItem)mSecurableObject;
                                    parentWeb = item.ParentList.ParentWeb;
                                }
                                else
                                {
                                    if (!(mSecurableObject is AveWeb))
                                    {
                                        throw new ArgumentException();
                                    }
                                    parentWeb = (AveWeb)mSecurableObject;
                                }
                                if (roleAgt != null)
                                {
                                    mRoleAssignments.Add(new AveRoleAssignment((parentWeb as AveWeb), roleAgt));
                                }
                                else
                                {
                                    mRoleAssignments.Add(null);
                                }
                            }
                        }
                    }
                    return mRoleAssignments;
                }
            }
        }
    }
}
