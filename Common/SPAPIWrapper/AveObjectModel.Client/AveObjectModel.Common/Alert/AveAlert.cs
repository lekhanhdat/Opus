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
using AvePoint.ObjectModel.Common.Alert;

namespace AvePoint.ObjectModel.Common
{
    class AveAlert : AveClientObject, IAveAlert
    {
        private AveWeb mWeb;
        private AveAlertCollection mAlertCollection;
        private IAveRequest mRequest;
        private AveAlertTemplate alertTemplate;

        public AveAlert(AveWeb web, AveAlertCollection alertCollection, IAveRequest request, IDictionary<string, object> alertProperties)
        {
            mWeb = web;
            mRequest = request;
            mAlertCollection = alertCollection;
            base.DataCache.AddPropertyies(alertProperties);
        }

        #region IAveAlert Members

        public AveAlertFrequency AlertFrequency
        {
            get
            {
                return base.DataCache.GetProperty<AveAlertFrequency>("AlertFrequency");
            }
            set
            {
                base.DataCache.AddChangedProperty("AlertFrequency", (int)value);
            }
        }

        public DateTime AlertTime
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("AlertTime");
            }
            set
            {
                base.DataCache.AddChangedProperty("AlertTime", value);
            }
        }

        public IAveAlertTemplate AlertTemplate
        {
            get
            {
                if (alertTemplate == null)
                {
                    Dictionary<string, object> alertTemplateProperties = base.DataCache.GetProperty<Dictionary<string, object>>("AlertTemplate" + AveObjectModelConstant.ObjectPropertySuffix);
                    alertTemplate = new AveAlertTemplate(mWeb, mAlertCollection, mRequest, alertTemplateProperties);
                }
                return alertTemplate;
            }
            set
            {
                alertTemplate = value as AveAlertTemplate;
            }
        }

        public AveAlertType AlertType
        {
            get
            {
                return base.DataCache.GetProperty<AveAlertType>("AlertType");
            }
            set
            {
                base.DataCache.AddChangedProperty("AlertType", (int)value);
            }
        }

        public bool AlwaysNotify
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AlwaysNotify");
            }
            set
            {
                base.DataCache.AddChangedProperty("AlwaysNotify", value);
            }
        }

        public AveEventType EventType
        {
            get
            {
                return base.DataCache.GetProperty<AveEventType>("EventType");
            }
            set
            {
                base.DataCache.AddChangedProperty("EventType", (int)value);
            }
        }

        public int EventTypeBitmask
        {
            get
            {
                return base.DataCache.GetProperty<int>("EventTypeBitmask");
            }
            set
            {
                base.DataCache.AddChangedProperty("EventTypeBitmask", value);
            }
        }

        public string Filter
        {
            get
            {
                return base.DataCache.GetProperty<string>("Filter");
            }
            set
            {
                base.DataCache.AddChangedProperty("Filter", value);
            }
        }

        public Guid ID
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("ID");
            }
        }

        public int ItemID
        {
            get 
            {
                return base.DataCache.GetProperty<int>("ItemID");
            }
        }

        public AveAlertDeliveryChannels DeliveryChannels
        {
            get
            {
                return base.DataCache.GetProperty<AveAlertDeliveryChannels>("DeliveryChannels");
            }
            set
            {
                base.DataCache.AddChangedProperty("DeliveryChannels", (int)value);
            }
        }

        public Guid ListID
        {
            get 
            {
                return base.DataCache.GetProperty<Guid>("ListID");
            }
        }

        public AveAlertStatus Status
        {
            get
            {
                return base.DataCache.GetProperty<AveAlertStatus>("Status");
            }
            set
            {
                base.DataCache.AddChangedProperty("Status", (int)value);
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                base.DataCache.AddChangedProperty("Title", value);
            }
        }

        public IAveUser User
        {
            get
            {
                return DataCache.EnsureLoadProperty("User",
                    () =>
                    {
                        string loginName = base.DataCache.GetProperty<string>("UserLoginName");
                        IAveUser user = mWeb.SiteUsers.GetByLoginName(loginName);
                        return user;
                    });
            }
            set
            {
                DataCache.AddProperty("User", value);
                DataCache.AddChangedProperty("UserUpdateId", this.User.ID);
            }
        }

        public int UserId
        {
            get 
            { 
                return base.DataCache.GetProperty<int>("UserId"); 
            }
        }

        public IAveListItem Item
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveList List
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Guid MatchId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("MatchId");
            }
            set
            {
                base.DataCache.AddChangedProperty("MatchId", value);
            }
        }

        public IAvePropertyBag Properties
        {
            get
            {
                return DataCache.EnsureLoadProperty("Properties",
                    () => 
                    {
                        IAvePropertyBag propertyBag = default(IAvePropertyBag);
                        var propertyBagObject = default(Dictionary<string, object>);
                        if (DataCache.TryGetProperty("Properties" + AveObjectModelConstant.ObjectPropertySuffix,out propertyBagObject))
                        {
                            propertyBag = new AvePropertyBag(this, this.mRequest, propertyBagObject);
                        }
                        return propertyBag;
                    });
            }
        }

        public void Update()
        {
            Update(false);
        }

        public void Update(bool bSendMail)
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> alertProperties = mRequest.UpdateAlert(mWeb.ServerRelativeUrl, this.ID, bSendMail, base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(alertProperties);
            }
        }

        #endregion


        public string ListUrl
        {
            get { throw new NotImplementedException(); }
        }
    }
}
