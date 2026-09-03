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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveRoleDefinitionBindingCollection : AveClientObject, IAveRoleDefinitionBindingCollection
    {
        //private AveRoleAssignment mRoleAssignment;
        private AveWeb mWeb;
        //private Guid mRoleDefintionWebId;
        //private IAveRequest mRequest;
        private List<int> mListData;

        public int Count
        {
            get
            {
                return mListData.Count;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public IAveRoleDefinition this[int index]
        {
            get
            {
                ArgumentCheck.CheckBoundary(index, mListData);
                if (mWeb == null)
                {
                    throw new ArgumentNullException("mWeb");
                }
                return mWeb.RoleDefinitions.GetById(mListData[index]);
            }
        }

        //public AveRoleDefinitionBindingCollection(AveRoleAssignment roleAssignment, IAveRequest request, List<int> bindings)
        //{
        //    mRoleAssignment = roleAssignment;
        //    mRequest = request;
        //    mListData = bindings;
        //    //base.DataCache.AddPropertyies(roleDefinitionBindingColProperties);
        //    //InitRoleDefinitionBindingCollection();
        //}

        //public AveRoleDefinitionBindingCollection(AveRoleAssignment roleAssignment, IAveRequest request)
        //{
        //    mRoleAssignment = roleAssignment;
        //    mRequest = request;
        //    mListData = new List<int>();
        //}

        public AveRoleDefinitionBindingCollection(AveWeb parentWeb)
        {
            //mRoleAssignment = roleAssignment;
            //mRequest = request;
            mWeb = parentWeb;
            mListData = new List<int>();
        }

        public AveRoleDefinitionBindingCollection(AveWeb parentWeb, List<int> bindings)
        {
            //mRoleAssignment = roleAssignment;
            //mRequest = request;
            mWeb = parentWeb;
            mListData = bindings;
            //base.DataCache.AddPropertyies(roleDefinitionBindingColProperties);
            //InitRoleDefinitionBindingCollection();
        }

        public AveRoleDefinitionBindingCollection()
        {
            mListData = new List<int>();
        }

        //internal void InitRoleDefinitionBindingCollection()
        //{
        //    List<Dictionary<string, object>> roleDefinitionPropertiesList = base.DataCache.GetChildren();
        //    mListData = new List<int>(roleDefinitionPropertiesList.Count);
        //    foreach (Dictionary<string, object> roleDefinitionProperties in roleDefinitionPropertiesList)
        //    {
        //        AveRoleDefinition roleDefinition = new AveRoleDefinition(mRequest, this, mWeb, roleDefinitionProperties);
        //        //roleDefinition.DataCache.AddPropertyies(roleDefinitionProperties);
        //        mListData.Add(roleDefinition.ID);
        //    }
        //}

        #region IAveRoleDefinitionBindingCollection Members

        public void Add(IAveRoleDefinition roleDefinition)
        {
            if (!mListData.Contains(roleDefinition.ID))
            {
                mListData.Add(roleDefinition.ID);
            }

            if(mWeb == null)
            {
                mWeb = roleDefinition.ParentWeb as AveWeb;
            }
        }

        public bool Contains(IAveRoleDefinition roleDefinition)
        {
            return mListData.Contains(roleDefinition.ID);
            //foreach (AveRoleDefinition roleDef in this.mListData)
            //{
            //    if (roleDef.ID == roleDefinition.ID)
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }

        public void Remove(IAveRoleDefinition roleDefinition)
        {
            mListData.Remove(roleDefinition.ID);
            //IAveRoleDefinition needDeletedRoleDef = mListData.Find(rd => rd.ID == roleDefinition.ID);
            //if (needDeletedRoleDef != null)
            //{
            //    mListData.Remove(needDeletedRoleDef);
            //}
        }

        public void RemoveAll()
        {
            mListData.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int startIndex = index;
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], startIndex + i);
            }
        }

        public void CopyTo(List<int> bindings)
        {
            if (bindings == null)
            {
                throw new ArgumentNullException("bindings");
            }
            bindings.AddRange(mListData);
        }

        public IEnumerator<IAveRoleDefinition> GetEnumerator()
        {
            return new AveCommonEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AveCommonEnumerator(this);
        }

        #endregion

        private sealed class AveCommonEnumerator : IEnumerator<IAveRoleDefinition>
        {
            private AveRoleDefinitionBindingCollection m_data;
            private int index = -1;

            public AveCommonEnumerator(AveRoleDefinitionBindingCollection bindings)
            {
                m_data = bindings;
            }

            #region IEnumerator<IAveRoleDefinition> Members

            public IAveRoleDefinition Current
            {
                get { return m_data[index]; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                index = -1;
                m_data = null;
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return m_data[index]; }
            }

            public bool MoveNext()
            {
                return ++index < m_data.Count;
            }

            public void Reset()
            {
                index = -1;
            }

            #endregion
        }
    }

}
