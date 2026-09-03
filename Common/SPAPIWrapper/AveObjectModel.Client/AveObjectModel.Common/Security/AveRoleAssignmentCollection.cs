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
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveRoleAssignmentCollection : AveAbstractCommonCollection<IAveRoleAssignment>, IAveRoleAssignmentCollection
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(AveRoleAssignmentCollection));
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList mList;
        private int mItemId;
        private IAveRequest mRequest;
        private AveSecurableObject mSecuableObject;
        private string mSource;
        private Guid mId;

        public AveRoleAssignmentCollection(AveSecurableObject securableObject, IAveRequest request, AveSite site, AveWeb web, AveList list, int itemId, string source, Dictionary<string, object> roleAssignmentColProperties)
        {
            mSecuableObject = securableObject;
            mSite = site;
            mWeb = web;
            mList = list;
            mItemId = itemId;
            mRequest = request;
            mSource = source;
            //base.DataCache.AddPropertyies(roleAssignmentColProperties);
            InitRoleAssignmentCollection(roleAssignmentColProperties);
        }

        internal void InitRoleAssignmentCollection(Dictionary<string, object> roleAssignmentColProperties)
        {
            var roleAssignmentPropertiesList = roleAssignmentColProperties.GetChildren();
            if (roleAssignmentPropertiesList != null)
            {
                //List<Dictionary<string, object>> roleAssignmentPropertiesList = obj as List<Dictionary<string, object>>;
                mListData = new List<IAveRoleAssignment>(roleAssignmentPropertiesList.Count);
                foreach (var roleAssignmentProperties in roleAssignmentPropertiesList)
                {
                    AveRoleAssignment roleAssignment = new AveRoleAssignment(this, mRequest, mSite, mWeb, mList, mItemId, mSource, roleAssignmentProperties);
                    //mSecuableObject.InitRoleAssignmentProperties(roleAssignment.DataCache.ChangedProperties);
                    mListData.Add(roleAssignment);
                }
            }
            else
            {
                mListData = new List<IAveRoleAssignment>(0);
            }
            object obj;
            if (roleAssignmentColProperties.TryGetValue("Id", out obj))
            {
                mId = (Guid)obj;
            }
        }

        internal AveSecurableObject SecurableObject
        {
            get
            {
                return mSecuableObject;
            }
        }

        #region IAveRoleAssignmentCollection Members

        public Guid ID
        {
            get
            {
                return mId;
            }
        }

        public void Add(IAveRoleAssignment roleAssignment)
        {
            IAveRoleAssignment originalRoleAssignment = this.GetByPrincipalId(roleAssignment.PrincipalId);
            AveRoleAssignment newRoleAssignment = null;
            if (originalRoleAssignment != null)  //更新已经存在的RoleAssignment的permission
            {
                int LIMITACCESS_ID = 1073741825;
                newRoleAssignment = originalRoleAssignment as AveRoleAssignment;
                IAveRoleDefinitionBindingCollection roleDefinitionBindings = newRoleAssignment.RoleDefinitionBindings;
                foreach (AveRoleDefinition roleDefinition in roleAssignment.RoleDefinitionBindings)
                {
                    if (roleDefinition != null && roleDefinition.ID != LIMITACCESS_ID && !roleDefinitionBindings.Contains(roleDefinition))
                    {
                        roleDefinitionBindings.Add(roleDefinition);
                    }
                }

                newRoleAssignment.Update();
            }
            else
            {
                newRoleAssignment = roleAssignment as AveRoleAssignment;
                newRoleAssignment.Init(this, mRequest, mSite, mWeb, mList, mItemId, mSource, true);
                newRoleAssignment.Update();
                mListData.Add(newRoleAssignment);
            }
        }

        public void ShareLink(int linkKind, string loginName, bool isDomainGroup, string parentWebUrl, Guid listId, int itemId)
        {
            mRequest.ShareLinkByRestApi(linkKind, loginName, isDomainGroup, parentWebUrl, listId, itemId, "");
        }

        public void ShareObjectExternal(int linkKind, string loginName, bool isDomainGroup, string parentWebUrl, Guid listId, int itemId, string roleId)
        {
            mRequest.ShareLinkByRestApi(linkKind, loginName, isDomainGroup, parentWebUrl, listId, itemId, roleId);
        }

        public IAveRoleAssignment Add(IAvePrincipal principal, IAveRoleDefinitionBindingCollection bindingCol)
        {
            AveRoleAssignment roleAssigment = new AveRoleAssignment(principal as AvePrincipal, mSite, mWeb, mList, mItemId, mSource);
            roleAssigment.ImportRoleDefinitionBindings(bindingCol);
            this.Add(roleAssigment);
            return roleAssigment;
        }

        public IAveRoleAssignment GetAssignmentByPrincipal(IAvePrincipal principal)
        {
            return mListData.Find(r => r.PrincipalId == principal.ID);
        }

        public IAveRoleAssignment GetByPrincipalId(int principalId)
        {
            return mListData.Find(r => r.PrincipalId == principalId);
        }

        public List<AveRoleAssignmentInfo> GetRoleAssignments(Guid siteId)
        {
            List<AveRoleAssignmentInfo> roleAssignmentsInfo = null;
            if (this.Count > 0)
            {
                roleAssignmentsInfo = new List<AveRoleAssignmentInfo>(this.Count);
            }
            foreach (IAveRoleAssignment roleAssignment in mListData)
            {
                IAveRoleDefinitionBindingCollection roleDefinitonBindingCol = roleAssignment.RoleDefinitionBindings;
                foreach (IAveRoleDefinition roleDef in roleDefinitonBindingCol)
                {
                    //limit access can't be restored.
                    if (roleDef != null && roleDef.Type != AveRoleType.Guest)
                    {
                        AveRoleAssignmentInfo roleAssignmentInfo = new AveRoleAssignmentInfo();
                        roleAssignmentInfo.PrincipalId = roleAssignment.PrincipalId;
                        roleAssignmentInfo.RoleId = roleDef.ID;
                        roleAssignmentsInfo?.Add(roleAssignmentInfo);
                    }
                    else
                    {
                        _logger.Info($"GetRoleAssignments.Current Role:{roleDef.ID}. is Limited Access.Item ID:{mItemId}.PrincipalId:{roleAssignment.PrincipalId}.");
                    }
                }
            }
            return roleAssignmentsInfo;
        }

        public int GetRoleAssignmentCount(Guid scopeId, int roleId, int principalId)
        {
            int count = 0;
            IAveRoleAssignment roleAssignment = this.GetByPrincipalId(principalId);

            if (roleAssignment != null && roleAssignment.PrincipalId == principalId)
            {
                foreach (IAveRoleDefinition roleDefinition in roleAssignment.RoleDefinitionBindings)
                {
                    if (roleDefinition.ID == roleId)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void Remove(int index)
        {
            this.RemoveById(this[index].PrincipalId);
        }

        public void RemoveById(int Id)
        {
            mSecuableObject.RemoveRoleAssignment(Id);
            mListData.Remove(GetByPrincipalId(Id));
        }

        public void Remove(IAvePrincipal member)
        {
            RemoveById(member.ID);
        }

        public void RemoveFromCurrentScopeOnly(IAvePrincipal member)
        {
            //office365不支持RemoveFromCurrentScopeOnly方法，暂时用RemoveAll代替,但是这种方法无法删除Limited Access
            IAveRoleAssignment roleAssignment = GetByPrincipalId(member.ID);
            if (roleAssignment != null)
            {
                roleAssignment.RoleDefinitionBindings.RemoveAll();
                roleAssignment.Update();
                mListData.Remove(roleAssignment);
            }
        }

        public IAveRoleAssignment CreateRoleAssignment(IAvePrincipal principal)
        {
            return new AveRoleAssignment(principal as AvePrincipal);//给外界提供方法可以创建RoleAssignment
        }

        public void RestoreSharingLink(string parentWebServerRelativeUrl, Guid listId, int itemId, IEnumerable<IAvePrincipal> avePrincipals, AveSharingLinkInfo shareLinkInfo)
        {
            mRequest.RestoreSharingLink(shareLinkInfo, avePrincipals, parentWebServerRelativeUrl, listId, itemId);
        }

        #endregion
    }
}
