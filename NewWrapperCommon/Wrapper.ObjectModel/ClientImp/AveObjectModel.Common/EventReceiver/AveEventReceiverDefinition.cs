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
    class AveEventReceiverDefinition : AveClientObject, IAveEventReceiverDefinition
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private AveList mList;
        private IAveRequest mRequest;
        private string mEventReceiverDefinitionSource;
        private AveEventReceiverDefinitionCollection mEventReceiverDefinitionCol;

        public AveEventReceiverDefinition(AveWeb web, AveList list, AveEventReceiverDefinitionCollection eventReceiverDefinitionCol, IAveRequest request, string erdSource, Dictionary<string, object> eventReceiverDefProperties)
        {
            mWeb = web;
            mList = list;
            mRequest = request;
            mEventReceiverDefinitionCol = eventReceiverDefinitionCol;
            mEventReceiverDefinitionSource = erdSource;
            base.DataCache.AddPropertyies(eventReceiverDefProperties);
        }

        public AveEventReceiverDefinition(AveSite site, AveEventReceiverDefinitionCollection eventReceiverDefinitionCol, IAveRequest request, string erdSource, Dictionary<string, object> eventReceiverDefProperties)
        {
            mSite = site;
            mRequest = request;
            mEventReceiverDefinitionCol = eventReceiverDefinitionCol;
            mEventReceiverDefinitionSource = erdSource;
            base.DataCache.AddPropertyies(eventReceiverDefProperties);
        }


        #region IAveEventReceiverDefinition Members

        public string Assembly
        {
            get
            {
                return base.DataCache.GetProperty<string>("Assembly");
            }
            set
            {
                base.DataCache.AddChangedProperty("Assembly", value);
            }
        }

        public string Class
        {
            get
            {
                return base.DataCache.GetProperty<string>("Class");
            }
            set
            {
                base.DataCache.AddChangedProperty("Class", value);
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                base.DataCache.AddChangedProperty("Name", value);
            }
        }

        public Guid ID
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public AveEventReceiverType Type
        {
            get
            {
                return base.DataCache.GetProperty<AveEventReceiverType>("Type");
            }
            set
            {
                base.DataCache.AddChangedProperty("Type", (int)value);
            }
        }

        public AveEventHostType HostType
        {
            //在server上实现，Client没有实现,返回默认值
            get
            {
                return default(AveEventHostType);
            }
            set
            {
                
            }
        }

        public void Delete()
        {
            mRequest.DeleteEventReceiverDefinition(mWeb.ServerRelativeUrl, mList == null ? null : mList.DefaultViewUrl, mList == null ? null : mList.Title, mList == null ? Guid.Empty : mList.ID, mEventReceiverDefinitionSource, this.ID);
            mEventReceiverDefinitionCol.ListData.Remove(this);
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> eventReceiverDefProperties = mRequest.UpdateEventReceiver(mWeb.ServerRelativeUrl, mList == null ? null : mList.DefaultViewUrl, mList == null ? null : mList.Title, mList == null ? Guid.Empty : mList.ID, mEventReceiverDefinitionSource, this.ID, base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(eventReceiverDefProperties);
            }
        }

        #endregion

        public System.Xml.XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }

        public int Synchronization
        {
            get
            {
                return base.DataCache.GetProperty<int>("Synchronization");
            }
            set
            {
                base.DataCache.AddChangedProperty("Synchronization", value);
            }

        }

        public int SequenceNumber
        {
            get
            {
                return base.DataCache.GetProperty<int>("SequenceNumber");
            }
            set
            {
                base.DataCache.AddChangedProperty("SequenceNumber", value);
            }
        }
    }
}
