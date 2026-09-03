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
    class AveEventReceiverDefinitionCollection : AveAbstractCommonCollection<IAveEventReceiverDefinition>, IAveEventReceiverDefinitionCollection
    {
        private AveWeb mWeb;
        private AveList mList;
        private IAveRequest mRequest;
        private string mEventReceiverSource;

        public AveEventReceiverDefinitionCollection(AveWeb web, AveList list, IAveRequest request, string eventReciverSource, Dictionary<string, object> eventReceiverColProperties)
        {
            mWeb = web;
            mList = list;
            mRequest = request;
            mEventReceiverSource = eventReciverSource;
            base.DataCache.AddPropertyies(eventReceiverColProperties);
            InitEventReceiverCollection();
        }

        internal void InitEventReceiverCollection()
        {
            var eventReceiverPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveEventReceiverDefinition>(eventReceiverPropertiesList.Count);
            foreach (var eventReceiverProperties in eventReceiverPropertiesList)
            {
                AveEventReceiverDefinition eventReceiverDefinition = new AveEventReceiverDefinition(mWeb, mList, this, mRequest, mEventReceiverSource, eventReceiverProperties);
                mListData.Add(eventReceiverDefinition);
            }
        }

        #region IAveEventReceiverDefinitionCollection Members

        public IAveEventReceiverDefinition this[Guid eventReceiverId]
        {
            get
            {
                return mListData.Find(e => e.ID == eventReceiverId);
            }
        }

        public void Add(AveEventReceiverType receiverType, string assembly, string className)
        {
            if (!IsExsit(receiverType, assembly, className))
            {
                Dictionary<string, object> eventReceiverDefinitionProp = new Dictionary<string, object>();
                eventReceiverDefinitionProp = this.mRequest.AddEventReceiverDefinition(this.mWeb.ServerRelativeUrl, null, this.mList == null ? Guid.Empty : this.mList.ID, this.mList == null ? null : this.mList.Title, this.mEventReceiverSource, (int)receiverType, assembly, className, string.Empty);
                AveEventReceiverDefinition eventReceiverDefinition = new AveEventReceiverDefinition(this.mWeb, this.mList, this, this.mRequest, this.mEventReceiverSource, eventReceiverDefinitionProp);
                this.mListData.Add(eventReceiverDefinition);
            }
        }

        private bool IsExsit(AveEventReceiverType receiverType, string assembly, string className)
        {
            foreach (IAveEventReceiverDefinition eventReceiverDefinition in this.mListData)
            {
                if (eventReceiverDefinition.Assembly.Equals(assembly, StringComparison.OrdinalIgnoreCase)
                        && eventReceiverDefinition.Class.Equals(className, StringComparison.OrdinalIgnoreCase)
                        && eventReceiverDefinition.Type == receiverType)
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(AveEventReceiverType receiverType, string assembly, string className, string name)
        {
            Dictionary<string, object> eventReceiverDefinitionProp = new Dictionary<string, object>();
            eventReceiverDefinitionProp = this.mRequest.AddEventReceiverDefinition(this.mWeb.ServerRelativeUrl, null, this.mList == null ? Guid.Empty : this.mList.ID, this.mList == null ? null : this.mList.Title, this.mEventReceiverSource, (int)receiverType, assembly, className, name);
            AveEventReceiverDefinition eventReceiverDefinition = new AveEventReceiverDefinition(this.mWeb, this.mList, this, this.mRequest, this.mEventReceiverSource, eventReceiverDefinitionProp);
            this.mListData.Add(eventReceiverDefinition);
        }

        #endregion
    }
}
