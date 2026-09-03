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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveAlert : IAveAlert, IDisposable
    {
        private SPAlert mAlert;
        private AveUser mUser;
        private AveListItem mItem;
        private AveList mList;
        private AvePropertyBag mProperties;
        private AveAlertTemplate mAlertTemplate;
        private AveAlertCollection mAlerts;

        public AveAlert(AveAlertCollection alerts, SPAlert alert)
        {
            mAlerts = alerts;
            mAlert = alert;
        }

        #region IAveAlert Members

        public AveAlertFrequency AlertFrequency
        {
            get
            {
                return (AveAlertFrequency)mAlert.AlertFrequency;
            }
            set
            {
                mAlert.AlertFrequency = (SPAlertFrequency)value;
            }
        }

        public DateTime AlertTime
        {
            get
            {
                return mAlert.AlertTime;
            }
            set
            {
                mAlert.AlertTime = value;
            }
        }

        public bool AlwaysNotify
        {
            get
            {
                return mAlert.AlwaysNotify;
            }
            set
            {
                mAlert.AlwaysNotify = value;
            }
        }

        public string Filter
        {
            get
            {
                return mAlert.Filter;
            }
            set
            {
                mAlert.Filter = value;
            }
        }

        public Guid ID
        {
            get { return mAlert.ID; }
        }

        public int ItemID
        {
            get { return mAlert.ItemID; }
        }

        public AveAlertDeliveryChannels DeliveryChannels
        {
            get
            {
                return (AveAlertDeliveryChannels)mAlert.DeliveryChannels;
            }
            set
            {
                mAlert.DeliveryChannels = (SPAlertDeliveryChannels)value;
            }
        }

        public Guid ListID
        {
            get { return mAlert.ListID; }
        }

        public string ListUrl
        {
            get { return mAlert.ListUrl; }
        }

        public AveAlertStatus Status
        {
            get
            {
                return (AveAlertStatus)mAlert.Status;
            }
            set
            {
                mAlert.Status = (SPAlertStatus)value;
            }
        }

        public string Title
        {
            get
            {
                return mAlert.Title;
            }
            set
            {
                mAlert.Title = value;
            }
        }

        public IAveUser User
        {
            get
            {
                if (mUser == null)
                {
                    SPUser user = mAlert.User;
                    if (user != null)
                    {
                        mUser = new AveUser(mAlerts.Web as AveWeb, user);
                    }
                }
                return mUser;
            }
            set
            {
                mUser = value as AveUser;
                if (mUser != null)
                {
                    mAlert.User = mUser.User;
                }
                else
                {
                    mAlert.User = null;
                }
            }
        }

        public void Update()
        {
            mAlert.Update();
        }

        public void Update(bool bSendMail)
        {
            mAlert.Update(bSendMail);
        }

        public AveEventType EventType
        {
            get
            {
                return (AveEventType)mAlert.EventType;
            }
            set
            {
                mAlert.EventType = (SPEventType)value;
            }
        }

        public IAveListItem Item
        {
            get
            {
                if (mItem == null)
                {
                    SPListItem item = mAlert.Item;
                    if (item != null)
                    {
                        mItem = new AveListItem(this.List.Items as AveListItemCollection, item);
                    }
                }
                return mItem;
            }
            set
            {
                mItem = (value as AveListItem);
                if (mItem != null)
                {
                    mAlert.Item = mItem.ListItem;
                }
                else
                {
                    mAlert.Item = null;
                }
            }
        }

        public IAveList List
        {
            get
            {
                if (mList == null)
                {
                    SPList list = mAlert.List;
                    if (list != null)
                    {
                        mList = ((mAlerts.Web as AveWeb).Lists as AveListCollection).CreateListByType(list);
                    }
                }
                return mList;
            }
            set
            {
                mList = (value as AveList);
                if (mList != null)
                {
                    mAlert.List = mList.List;
                }
                else
                {
                    mAlert.List = null;
                }
            }
        }

        public IAvePropertyBag Properties
        {
            get
            {
                if (mProperties == null)
                {
                    mProperties = new AvePropertyBag(mAlert.Properties);
                }
                return mProperties;
            }
        }

        public IAveAlertTemplate AlertTemplate
        {
            get
            {
                if (mAlertTemplate == null)
                {
                    SPAlertTemplate alertTemplate = mAlert.AlertTemplate;
                    if (alertTemplate != null)
                    {
                        mAlertTemplate = new AveAlertTemplate(alertTemplate);
                    }
                }
                return mAlertTemplate;
            }
            set
            {
                mAlertTemplate = value as AveAlertTemplate;
                if (mAlertTemplate != null)
                {
                    mAlert.AlertTemplate = mAlertTemplate.AlertTemplate;
                }
                else
                {
                    mAlert.AlertTemplate = null;
                }
            }
        }

        public int UserId
        {
            get { return mAlert.UserId; }
        }

        public AveAlertType AlertType
        {
            get
            {
                return (AveAlertType)mAlert.AlertType;
            }
            set
            {
                mAlert.AlertType = (SPAlertType)value;
            }
        }

        public int EventTypeBitmask
        {
            get
            {
                return mAlert.EventTypeBitmask;
            }
            set
            {
                mAlert.EventTypeBitmask = value;
            }
        }

        public Guid MatchId
        {
            get
            {
                return mAlert.MatchId;
            }
            set
            {
                mAlert.MatchId = value;
            }
        }

        public string AlertTemplateName
        {
            get
            {
                return mAlert.AlertTemplateName;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (mAlertTemplate != null)
            {
                mAlertTemplate.Dispose();
                mAlertTemplate = null;
            }
        }

        #endregion
    }
}
