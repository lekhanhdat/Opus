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
    class AveRoleDefinitionBindingCollection : AveAbstractCommonCollection<IAveRoleDefinition>, IAveRoleDefinitionBindingCollection
    {
        private AveRoleAssignment mRoleAssignment;
        private AveWeb mWeb;
        private Guid mRoleDefintionWebId;
        private IAveRequest mRequest;

        public AveRoleDefinitionBindingCollection(AveRoleAssignment roleAssignment, IAveRequest request, Dictionary<string, object> roleDefinitionBindingColProperties)
        {
            mRoleAssignment = roleAssignment;
            mRequest = request;
            base.DataCache.AddPropertyies(roleDefinitionBindingColProperties);
            InitRoleDefinitionBindingCollection();
        }

        public AveRoleDefinitionBindingCollection(AveRoleAssignment roleAssignment, IAveRequest request)
        {
            mRoleAssignment = roleAssignment;
            mRequest = request;
            mListData = new List<IAveRoleDefinition>();
        }

        public AveRoleDefinitionBindingCollection(AveRoleAssignment roleAssignment, IAveWeb parentWeb, IAveRequest request)
        {
            mRoleAssignment = roleAssignment;
            mRequest = request;
            mWeb = parentWeb as AveWeb;
            mListData = new List<IAveRoleDefinition>();
        }

        public AveRoleDefinitionBindingCollection(AveRoleAssignment roleAssignment, IAveRequest request, IAveWeb parentWeb, Dictionary<string, object> roleDefinitionBindingColProperties)
        {
            mRoleAssignment = roleAssignment;
            mRequest = request;
            mWeb = parentWeb as AveWeb;
            base.DataCache.AddPropertyies(roleDefinitionBindingColProperties);
            InitRoleDefinitionBindingCollection();
        }

        public AveRoleDefinitionBindingCollection()
        {
            mListData = new List<IAveRoleDefinition>();
        }

        internal void InitRoleDefinitionBindingCollection()
        {
            List<Dictionary<string, object>> roleDefinitionPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveRoleDefinition>(roleDefinitionPropertiesList.Count);
            foreach (Dictionary<string, object> roleDefinitionProperties in roleDefinitionPropertiesList)
            {
                AveRoleDefinition roleDefinition = new AveRoleDefinition(mRequest, this, mWeb, roleDefinitionProperties);
                roleDefinition.DataCache.AddPropertyies(roleDefinitionProperties);
                mListData.Add(roleDefinition);
            }
        }

        #region IAveRoleDefinitionBindingCollection Members

        public void Add(IAveRoleDefinition roleDefinition)
        {                
            mListData.Add(roleDefinition);
        }

        public bool Contains(IAveRoleDefinition roleDefinition)
        {
            foreach (AveRoleDefinition roleDef in this.mListData)
            {
                if (roleDef.ID == roleDefinition.ID)
                {
                    return true;
                }
            }
            return false;
        }

        public void Remove(int index)
        {
            mListData.RemoveAt(index);
        }

        public void Remove(IAveRoleDefinition roleDefinition)
        {
            IAveRoleDefinition needDeletedRoleDef = mListData.Find(rd => rd.ID == roleDefinition.ID);
            if (needDeletedRoleDef != null)
            {
                mListData.Remove(needDeletedRoleDef);
            }
        }

        public void RemoveAll()
        {
            mListData.Clear();
        }

        #endregion       
    }
}
