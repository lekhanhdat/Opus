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

namespace AvePoint.ObjectModel.Server19
{
    class AveRoleDefinitionBindingCollection : AveAbstractCommonCollection<IAveRoleDefinition>, IAveRoleDefinitionBindingCollection
    {
        private SPRoleDefinitionBindingCollection mRoleDefinitionBindings;
        private AveWeb mWeb;

        public AveRoleDefinitionBindingCollection(AveWeb web, SPRoleDefinitionBindingCollection roleDefinitions)
            : base(roleDefinitions)
        {
            mWeb = web;
            mRoleDefinitionBindings = roleDefinitions;
        }

        public AveRoleDefinitionBindingCollection(SPRoleDefinitionBindingCollection roleDefinitionBindingCollection)
            : base(roleDefinitionBindingCollection)
        {
            mRoleDefinitionBindings = roleDefinitionBindingCollection;
        }

        public AveRoleDefinitionBindingCollection()
            : this(new SPRoleDefinitionBindingCollection())
        { }

        internal SPRoleDefinitionBindingCollection RoleDefinitionBindingCollection
        {
            get
            {
                return mRoleDefinitionBindings;
            }
        }

        #region IAveRoleDefinitionBindingCollection Members

        //allowed to add limited access
        public void Add(IAveRoleDefinition roleDefinition)
        {
            AveAssemblyUtility.InvokeMethod(mRoleDefinitionBindings, "AddInternal", new object[] { (roleDefinition as AveRoleDefinition).RoleDefinition });            
        }

        public void Remove(int index)
        {
            mRoleDefinitionBindings.Remove(index);
        }

        public void Remove(IAveRoleDefinition roleDefinition)
        {
            mRoleDefinitionBindings.Remove((roleDefinition as AveRoleDefinition).RoleDefinition);
        }

        public void RemoveAll()
        {
            mRoleDefinitionBindings.RemoveAll();
        }

        public bool Contains(IAveRoleDefinition roleDefinition)
        {
            return mRoleDefinitionBindings.Contains((roleDefinition as AveRoleDefinition).RoleDefinition);
        }

        public override IAveRoleDefinition this[int index]
        {
            get
            {
                return new AveRoleDefinition(mWeb, mRoleDefinitionBindings[index]);
            }
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveRoleDefinition(mWeb, t as SPRoleDefinition);
        }

        public override int Count
        {
            get { return mRoleDefinitionBindings.Count; }
        }
    }
}
