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
    class AveRoleDefinitionCollection : AveAbstractCommonCollection<IAveRoleDefinition>, IAveRoleDefinitionCollection
    {
        private IAveRequest mRequest;
        private AveWeb mWeb;

        public AveRoleDefinitionCollection(AveWeb web, IAveRequest request, Dictionary<string, object> roleDefinitionColProperties)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(roleDefinitionColProperties);
            InitRoleDefinitionCollection();
        }

        internal void InitRoleDefinitionCollection()
        {
            List<Dictionary<string, object>> roleDefinitionPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveRoleDefinition>();
            foreach (Dictionary<string, object> roleDefinitonProperties in roleDefinitionPropertiesList)
            {
                AveRoleDefinition roleDefinition = new AveRoleDefinition(mRequest, this, mWeb, roleDefinitonProperties);
                mListData.Add(roleDefinition);
            }
        }

        #region IAveRoleDefinitionCollection Members

        public IAveRoleDefinition Add(AveRoleDefinitionCreationInformation roleDefCreationInfo)
        {
            Dictionary<string, object> newRoleDefinitionInfo = new Dictionary<string, object>();
            AveObjectCopy.GetObjectBasicProperties(newRoleDefinitionInfo, roleDefCreationInfo);
            Dictionary<string, object> newRoleDefinitionProperties = mRequest.AddRoleDefinition(mWeb.ServerRelativeUrl, newRoleDefinitionInfo);
            AveRoleDefinition newRoleDefinition = new AveRoleDefinition(mRequest, this);
            newRoleDefinition.Web = mWeb;
            newRoleDefinition.Description = roleDefCreationInfo.Description;
            newRoleDefinition.BasePermissions = roleDefCreationInfo.BasePermissions;
            newRoleDefinition.Name = roleDefCreationInfo.Name;
            newRoleDefinition.Order = roleDefCreationInfo.Order;
            newRoleDefinition.DataCache.UpdateProperties(newRoleDefinitionProperties);
            mListData.Add(newRoleDefinition);
            return newRoleDefinition;
        }

        public IAveRoleDefinition Add(IAveRoleDefinition roleDefinition)
        {
            Dictionary<string, object> newRoleDefinitionProperties = mRequest.AddRoleDefinition(mWeb.ServerRelativeUrl, (roleDefinition as AveRoleDefinition).DataCache.ChangedProperties);
            AveRoleDefinition newRoleDefinition = new AveRoleDefinition(mRequest, this, newRoleDefinitionProperties);
            newRoleDefinition.Web = mWeb;
            mListData.Add(newRoleDefinition);
            return newRoleDefinition;
        }

        //should reload web.roledefintions and web.roleassignments
        public void BreakInheritance(bool copyRoleDefinitions, bool keepRoleAssignments)
        {
            throw new NotImplementedException();//没实现
            //Dictionary<string, object> roleDefinitionsProperties = mRequest.BreakRoleDefinitionInheritance(mWeb.ServerRelativeUrl, copyRoleDefinitions, keepRoleAssignments);
            //base.DataCache.PropertiesCache.Clear();
            //base.DataCache.AddPropertyies(roleDefinitionsProperties);
            //InitRoleDefinitionCollection();
            //mWeb.DataCache.PropertiesCache["RoleDefinitions"] = this;
        }

        public IAveRoleDefinition GetById(int id)
        {
            return mListData.Find(rd => rd.ID == id);
        }

        public IAveRoleDefinition GetByName(string name)
        {
            return mListData.Find(rd => rd.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IAveRoleDefinition GetByType(AveRoleType roleType)
        {
            return mListData.Find(rd => rd.Type == roleType);
        }

        public IAveRoleDefinition this[string name]
        {
            get
            {
                IAveRoleDefinition roleDefintion = mListData.Find(rd => rd.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (roleDefintion == null)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_NotFindRole);
                }
                return roleDefintion;
            }
        }

        public void Delete(IAveRoleDefinition roleDefinition)
        {
            roleDefinition.DeleteObject();
            mListData.Remove(roleDefinition);
        }

        public void DeleteById(int id)
        {
            IAveRoleDefinition roleDefinition = this.GetById(id);
            roleDefinition.DeleteObject();
            mListData.Remove(roleDefinition);
        }

        #endregion
    }
}
