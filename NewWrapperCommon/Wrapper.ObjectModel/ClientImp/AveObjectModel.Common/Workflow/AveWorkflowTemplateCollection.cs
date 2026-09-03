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
using System.Globalization;

namespace AvePoint.ObjectModel.Common
{
    class AveWorkflowTemplateCollection : AveAbstractCommonCollection<IAveWorkflowTemplate>, IAveWorkflowTemplateCollection
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveContentTypeCollection));
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private AveList mParentList;

        private object privateLock = new object();
        private bool isDirty = false;
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

        [Obsolete]
        public AveWorkflowTemplateCollection(IAveRequest request, IAveWeb parentWeb, IAveList parentList, string contentTypeSource, Dictionary<string, object> workflowTemplatesPro)
        {
            mRequest = request;
            mParentWeb = parentWeb as AveWeb;
            mParentList = parentList as AveList;
          //  mContentTypeSource = contentTypeSource;
            base.DataCache.AddPropertyies(workflowTemplatesPro);
           InitWorkflowTemplates();
        }

        public AveWorkflowTemplateCollection(IAveWeb parentWeb, IAveList parentList, string contentTypeSource)
        {
            lock (privateLock)
            {
                mParentWeb = parentWeb as AveWeb;
                mParentList = parentList as AveList;
                mRequest = (mParentWeb.Site as AveSite).Request;
                Dictionary<string, object> workflowTemplatesPro = mRequest.GetWorkflowTemplates(mParentWeb.ServerRelativeUrl, mParentWeb.Name, mParentWeb.ID, "web.workflowTemplates", null);
                base.DataCache.AddPropertyies(workflowTemplatesPro);
                InitWorkflowTemplates(); 
            }
        }

        internal void UpdateCollectionInternally()
        {
            lock (privateLock)
            {
                Dictionary<string, object> workflowTemplatesPro = mRequest.GetWorkflowTemplates(mParentWeb.ServerRelativeUrl, mParentWeb.Name, mParentWeb.ID, "web.workflowTemplates", null);
                base.DataCache.RemoveProperty(AveObjectModelConstant.ChildrenProperties);
                mListData.Clear();
                base.DataCache.AddPropertyies(workflowTemplatesPro);
                InitWorkflowTemplates(); 
                IsDirty = false;
            }
        }

        private void InitWorkflowTemplates()
        {
            mListData = new List<IAveWorkflowTemplate>();
            foreach (Dictionary<string, object> properties in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveWorkflowTemplate template = new AveWorkflowTemplate(properties);
                this.mListData.Add(template);
            }
        }

        public IAveWorkflowTemplate this[int index] 
        {
            get
            {
                lock (privateLock)
                {
                    return mListData[index]; 
                }
            }
        }

        public IAveWorkflowTemplate this[Guid guid] 
        {
            get
            {
                lock (privateLock)
                {
                    return mListData.Find(
                        delegate(IAveWorkflowTemplate workflowTemplate)
                        {
                            return workflowTemplate.ID.Equals(guid);
                        });
                }
            }
        }

        public void Add(IAveWorkflowTemplate template) 
        {
            throw new NotImplementedException();
        }

        public IAveWorkflowTemplate GetTemplateByBaseID(Guid baseTemplateId)
        {
            lock (privateLock)
            {
                return mListData.Find(
                    delegate(IAveWorkflowTemplate workflowTemplate)
                    {
                        return workflowTemplate.ID.Equals(baseTemplateId);
                    });
            }
        }
        public IAveWorkflowTemplate GetTemplateByNmae(string templateName, CultureInfo cultureInfo)
        {
            lock (privateLock)
            {
                return mListData.Find(
                    delegate (IAveWorkflowTemplate workflowTemplate)
                    {
                        return string.Compare(workflowTemplate.Name, templateName, true, cultureInfo) == 0;
                    });
            }
        }
    }
}
