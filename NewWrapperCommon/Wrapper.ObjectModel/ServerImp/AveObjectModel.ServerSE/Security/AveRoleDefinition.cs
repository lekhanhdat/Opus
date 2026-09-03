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
    class AveRoleDefinition : AveServerObject, IAveRoleDefinition
    {
        private SPRoleDefinition mRoleDefinition;
        private AveWeb mParentWeb;

        public AveRoleDefinition(AveWeb web, SPRoleDefinition roleDefinition)
        {
            mParentWeb = web;
            mRoleDefinition = roleDefinition;
        }

        public AveRoleDefinition()
        {
            mRoleDefinition = new SPRoleDefinition();
        }

        internal SPRoleDefinition RoleDefinition
        {
            get
            {
                return mRoleDefinition;
            }
        }

        #region IAveRoleDefinition Members

        public void DeleteObject()
        {

        }

        public void Update()
        {
            mRoleDefinition.Update();
        }

        public AveBasePermissions BasePermissions
        {
            get
            {
                return (AveBasePermissions)mRoleDefinition.BasePermissions;
            }
            set
            {
                mRoleDefinition.BasePermissions = (SPBasePermissions)value;
            }
        }

        public string Description
        {
            get
            {
                return mRoleDefinition.Description;
            }
            set
            {
                mRoleDefinition.Description = value;
            }
        }

        public bool Hidden
        {
            get { return mRoleDefinition.Hidden; }
        }

        public int ID
        {
            get { return mRoleDefinition.Id; }
        }

        public string Name
        {
            get
            {
                return mRoleDefinition.Name;
            }
            set
            {
                mRoleDefinition.Name = value;
            }
        }

        public int Order
        {
            get
            {
                return mRoleDefinition.Order;
            }
            set
            {
                mRoleDefinition.Order = value;
            }
        }

        public AveRoleType Type
        {
            get { return (AveRoleType)mRoleDefinition.Type; }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                return mParentWeb;
            }
        }

        public string NameInternal
        {
            get { return (string) AveAssemblyUtility.GetPropertyValue(mRoleDefinition, "NameInternal"); }
        }

        #endregion
    }
}
