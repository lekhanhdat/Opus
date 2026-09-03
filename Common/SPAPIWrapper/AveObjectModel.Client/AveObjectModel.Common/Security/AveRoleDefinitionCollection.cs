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
        private Dictionary<int, IAveRoleDefinition> idIndex;

        public AveRoleDefinitionCollection(AveWeb web, IAveRequest request, Dictionary<string, object> roleDefinitionColProperties)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(roleDefinitionColProperties);
            InitRoleDefinitionCollection();
        }

        internal void InitRoleDefinitionCollection()
        {
            var roleDefinitionPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveRoleDefinition>();
            idIndex = new Dictionary<int, IAveRoleDefinition>(roleDefinitionPropertiesList.Count);
            foreach (var roleDefinitonProperties in roleDefinitionPropertiesList)
            {
                AveRoleDefinition roleDefinition = new AveRoleDefinition(mRequest, this, mWeb, roleDefinitonProperties);
                mListData.Add(roleDefinition);
                idIndex[roleDefinition.ID] = roleDefinition;
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
            lock(idIndex)
            {
                mListData.Add(newRoleDefinition);
                idIndex[newRoleDefinition.ID] = newRoleDefinition;
            }
            return newRoleDefinition;
        }

        public IAveRoleDefinition Add(IAveRoleDefinition roleDefinition)
        {
            Dictionary<string, object> newRoleDefinitionProperties = mRequest.AddRoleDefinition(mWeb.ServerRelativeUrl, (roleDefinition as AveRoleDefinition).DataCache.ChangedProperties);
            AveRoleDefinition newRoleDefinition = new AveRoleDefinition(mRequest, this, newRoleDefinitionProperties);
            newRoleDefinition.Web = mWeb;
            //mListData.Add(newRoleDefinition);
            lock (idIndex)
            {
                mListData.Add(newRoleDefinition);
                idIndex[newRoleDefinition.ID] = newRoleDefinition;
            }
            return newRoleDefinition;
        }

        //should reload web.roledefintions and web.roleassignments
        public void BreakInheritance(bool copyRoleDefinitions, bool keepRoleAssignments)
        {
            Dictionary<string, object> roleDefinitionsProperties = mRequest.BreakRoleDefinitionInheritance(mWeb.ServerRelativeUrl, copyRoleDefinitions, keepRoleAssignments);
            base.DataCache.ResetProperties();
            base.DataCache.AddPropertyies(roleDefinitionsProperties);
            InitRoleDefinitionCollection();
            mWeb.DataCache.AddProperty("RoleDefinitions",this);
        }

        public IAveRoleDefinition GetById(int id)
        {
            lock (idIndex)
            {
                IAveRoleDefinition definition;
                if (!idIndex.TryGetValue(id, out definition))
                {
                    definition = null;
                }

                return definition;
            }
            //return mListData.Find(rd => rd.ID == id);
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
                    throw new Exception("Cannot Find Role");
                }
                return roleDefintion;
            }
        }

        public void Delete(IAveRoleDefinition roleDefinition)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(int id)
        {
            IAveRoleDefinition roleDefinition = this.GetById(id);
            roleDefinition.DeleteObject();
            lock (idIndex)
            {
                mListData.Remove(roleDefinition);
                idIndex.Remove(roleDefinition.ID);
            }
        }

        #endregion
    }
}
