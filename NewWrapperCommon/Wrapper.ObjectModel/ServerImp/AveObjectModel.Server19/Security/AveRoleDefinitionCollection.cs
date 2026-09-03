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
using System;

namespace AvePoint.ObjectModel.Server19
{
    class AveRoleDefinitionCollection : AveAbstractCommonCollection<IAveRoleDefinition>, IAveRoleDefinitionCollection
    {
        private SPRoleDefinitionCollection mRoleDefinitions;
        private AveWeb mWeb;

        public AveRoleDefinitionCollection(AveWeb web, SPRoleDefinitionCollection roleDefinitions)
            : base(roleDefinitions)
        {
            mWeb = web;
            mRoleDefinitions = roleDefinitions;
        }

        #region IAveRoleDefinitionCollection Members

        public IAveRoleDefinition Add(AveRoleDefinitionCreationInformation parameters)
        {
            SPRoleDefinition role = new SPRoleDefinition();
            role.Name = parameters.Name;
            role.Description = parameters.Description;
            role.Order = parameters.Order;
            role.BasePermissions = (SPBasePermissions)parameters.BasePermissions;
            mRoleDefinitions.Add(role);
            return GetByName(role.Name);
        }

        public void BreakInheritance(bool copyRoleDefinitions, bool keepRoleAssignments)
        {
            mRoleDefinitions.BreakInheritance(copyRoleDefinitions, keepRoleAssignments);
            mWeb.SetAllowUnsafeUpdate();
        }

        public IAveRoleDefinition GetById(int id)
        {
            return new AveRoleDefinition(mWeb, mRoleDefinitions.GetById(id));
        }

        public IAveRoleDefinition GetByName(string name)
        {
            return GetById(mRoleDefinitions[name].Id);
        }

        public IAveRoleDefinition GetByType(AveRoleType roleType)
        {
            return new AveRoleDefinition(mWeb, mRoleDefinitions.GetByType((SPRoleType)roleType));
        }

        public IAveRoleDefinition this[string name]
        {
            get
            {
                return new AveRoleDefinition(mWeb, mRoleDefinitions[name]);
            }
        }

        public IAveRoleDefinition Add(IAveRoleDefinition roleDefinition)
        {
            mRoleDefinitions.Add((roleDefinition as AveRoleDefinition).RoleDefinition);
            return roleDefinition;
        }

        public override IAveRoleDefinition this[int index]
        {
            get
            {
                return new AveRoleDefinition(mWeb, mRoleDefinitions[index]);
            }
        }

        public void Delete(IAveRoleDefinition roleDefinition)
        {
            AveAssemblyUtility.InvokeMethod(mRoleDefinitions, typeof(SPRoleDefinitionCollection), "Delete", new Type[] { typeof(SPRoleDefinition) }, new object[] { (roleDefinition as AveRoleDefinition).RoleDefinition });
        }

        public void DeleteById(int id)
        {
            mRoleDefinitions.DeleteById(id);
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveRoleDefinition(mWeb, t as SPRoleDefinition);
        }

        public override int Count
        {
            get { return mRoleDefinitions.Count; }
        }
    }
}
