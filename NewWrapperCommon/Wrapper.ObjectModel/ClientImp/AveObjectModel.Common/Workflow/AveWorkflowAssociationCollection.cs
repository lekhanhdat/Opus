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
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowAssociationCollection : AveAbstractCommonCollection<IAveWorkflowAssociation>, IAveWorkflowAssociationCollection
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveWorkflowAssociationCollection));

        private string mWorkflowSource;
        private IAveWeb mWeb = null;
        private IAveList mList = null;
        private IAveRequest mRequest;
        private object privateLock = new object();
        private bool isDirty = false;
        private Dictionary<string, object> contentTypeProp;
        internal bool IsDirty
        {
            get
            {
                lock (privateLock)
                {
                    return isDirty;
                }
            }
            set
            {
                lock (privateLock)
                {
                    isDirty = value;
                }
            }
        }

        public AveWorkflowAssociationCollection(IAveWeb web, IAveList list,Dictionary<string,object> ContentTypeProp, string workflowSource)
        {
            lock (privateLock)
            {
                mWeb = web;
                mList = list;
                mWorkflowSource = workflowSource;
                contentTypeProp = ContentTypeProp;
                mRequest = ((AveSite)(mWeb.Site)).Request;
                Dictionary<string, object> prop = mRequest.GetWorkflowAssociations(mWeb.ServerRelativeUrl, mList == null ? null : mList.Title, mList == null ? Guid.Empty : mList.ID, mWorkflowSource, contentTypeProp);
                mListData = new List<IAveWorkflowAssociation>(prop.Count);
                base.DataCache.AddPropertyies(prop);
                InitWorkflowAssocCol();
            }
        }

        internal void UpdateCollectionInternally()
        {
            lock (privateLock)
            {
                Dictionary<string, object> prop = mRequest.GetWorkflowAssociations(mWeb.ServerRelativeUrl, mList == null ? null : mList.Title, mList == null ? Guid.Empty : mList.ID, mWorkflowSource, contentTypeProp);
                base.DataCache.RemoveProperty(AveObjectModelConstant.ChildrenProperties);
                mListData.Clear();
                base.DataCache.AddPropertyies(prop);
                InitWorkflowAssocCol();
                IsDirty = false;
            }
        }

        private void InitWorkflowAssocCol()
        {
            foreach ( Dictionary<string, object> dic in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties) )
            {
                AveWorkflowAssociation workflow = new AveWorkflowAssociation(mWeb, mList, this.mWorkflowSource, dic);
                mListData.Add(workflow);
            }
        }

        public void Add(AveWorkflowAssociation w)
        {
            lock (privateLock)
            {
                mListData.Add(w); 
            }
        }

        #region IAveWorkflowAssociationCollection Members

        public IAveWorkflowAssociation this[Guid workflowAssociationId]
        {
            get
            {
                lock (privateLock)
                {
                    return mListData.Find(
                               delegate(IAveWorkflowAssociation w)
                               {
                                   return w.ID.Equals(workflowAssociationId);
                               });
                } 
            }
        }

        public void Remove(IAveWorkflowAssociation association)
        {
            lock (privateLock)
            {
                mRequest.DeleteWorkflowAssociation(association, mWorkflowSource);
                mListData.Remove(association); 
            }
            //update?
        }

        public void RemoveAll()
        {
            lock (privateLock)
            {
                if (mListData != null && mListData.Count > 0)
                {
                    string ctId = Convert.ToString(mListData[0].ContentTypeId);
                    Guid listId = mList == null ? Guid.Empty : mList.ID;
                    mRequest.DeleteAllWorkflowAasociations(mWeb.ServerRelativeUrl, listId, ctId, mWorkflowSource);
                    mListData.Clear();
                }
            }
        }

        public void Update(IAveWorkflowAssociation workflowAssociation)
        {
            (workflowAssociation as AveWorkflowAssociation).Update();
        }

        public IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId)
        {
            throw new NotImplementedException();
        }

        public IAveWorkflowAssociation GetAssociationByBaseID(Guid baseTemplateId, bool ignoreStartSettings)
        {
            throw new NotImplementedException();
        }


        public IAveWorkflowAssociation GetAssociationByName(string name, System.Globalization.CultureInfo cultureInfo)
        {
            lock (privateLock)
            {
                return mListData.Find(
                       delegate(IAveWorkflowAssociation wfAsso)
                       {
                           return name.Equals(wfAsso.Name);
                       });
                //throw new NotImplementedException(); 
            }
        }

        #endregion




        public IAveWorkflowAssociation Add(IAveWorkflowAssociation workflowAssociation)
        {
            //throw new NotImplementedException();
            lock (privateLock)
            {
                Dictionary<string, object> props = mRequest.CreateWebAssociation(mWeb.ServerRelativeUrl, mWeb.ID, "web.workflowTemplates", workflowAssociation);
                AveWorkflowAssociation newWFAssociation = new AveWorkflowAssociation(this.mWeb, this.mList, string.Empty, props);
                this.mListData.Add(newWFAssociation);
                return newWFAssociation; 
            }
        }
        public bool UpdateAssociationsToLatestVersion()
        {
            //可以通过post实现,暂时没有需求，先不实现
            return false;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return mListData.GetEnumerator();
        }
    }
}