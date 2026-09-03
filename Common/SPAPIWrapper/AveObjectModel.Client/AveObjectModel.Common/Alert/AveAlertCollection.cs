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
    class AveAlertCollection : AveAbstractCommonCollection<IAveAlert>, IAveAlertCollection
    {
        private AveWeb mWeb;
        private IAveRequest mRequest;

        public AveAlertCollection(AveWeb web, IAveRequest request, Dictionary<string, object> alertColProperties)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(alertColProperties);                
            InitAlertCollection();
        }

        internal void InitAlertCollection()
        {
            var alertPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveAlert>(alertPropertiesList.Count);
            foreach (var alertProperites in alertPropertiesList)
            {
                AveAlert alert = new AveAlert(mWeb, this, mRequest, alertProperites);
                mListData.Add(alert);
            }
        }

        #region IAveAlertCollection Members

        public IAveAlert this[Guid alertId]
        {
            get 
            {
                return mListData.Find(a => a.ID == alertId);
            }
        }

        public Guid Add(IAveListItem listItem, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return this.AddAlert(listItem, eventType, alertFrequency, true);
        }

        public Guid Add(IAveList list, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return this.AddAlert(list, eventType, alertFrequency, true);
        }

        public Guid AddAlert(IAveListItem listItem, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return this.AddAlert(listItem, eventType, alertFrequency, false);
        }

        public Guid AddAlert(IAveList list, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return this.AddAlert(list, eventType, alertFrequency, false);
        }

        internal Guid AddAlert(IAveListItem listItem, AveEventType eventType, AveAlertFrequency alertFrequency, bool isSendEmail)
        {
            Dictionary<string, object> alertProperties = mRequest.AddAlert(mWeb.ServerRelativeUrl, listItem.ParentList.DefaultViewUrl, listItem.ParentList.Title, listItem.ID, (int)eventType, (int)alertFrequency, isSendEmail);
            AveAlert alert = new AveAlert(mWeb, this, mRequest, alertProperties);
            mListData.Add(alert);
            return alert.ID;
        }

        public Guid AddAlert(IAveList list, AveEventType eventType, AveAlertFrequency alertFrequency, bool isSendEmail)
        {
            Dictionary<string, object> alertProperties = mRequest.AddAlert(mWeb.ServerRelativeUrl, list.DefaultViewUrl, list.Title, (int)eventType, (int)alertFrequency, isSendEmail);
            AveAlert alert = new AveAlert(mWeb, this, mRequest, alertProperties);
            mListData.Add(alert);
            return alert.ID;
        }

        public void Delete(Guid idAlert)
        {
            throw new NotImplementedException();
        }

        public void Delete(int index)
        {
            throw new NotImplementedException();
        }

        #endregion       
    

        public IAveAlert Add()
        {
            throw new NotImplementedException();
        }      

        public IAveWeb Web
        {
            get { return mWeb; }
        }

     
        public List<Dictionary<string, object>> GetImmedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            List<Dictionary<string, object>> ImmedSubscriptions = new List<Dictionary<string, object>>();

            Dictionary<string, object> dataCache = null;
            foreach (IAveAlert alert in this)
            {
                if (alert.AlwaysNotify)
                {
                    continue;
                }
                if (alert.ListID == listId)
                {
                    if (itemRowId > 0 && alert.ItemID == itemRowId)
                    {
                        dataCache = new Dictionary<string, object>();
                        dataCache.Add("EventType", alert.EventType);
                        if (!string.IsNullOrEmpty(alert.Filter))
                        {
                            dataCache.Add("Filter", alert.Filter);
                        }
                        dataCache.Add("DeliveryChannel", (int)alert.DeliveryChannels);
                        dataCache.Add("AlertTitle", alert.Title);
                        dataCache.Add("UserId", alert.User != null ? alert.User.ID : alert.UserId);
                        dataCache.Add("NotifyFreq", (int)alert.AlertFrequency);
                        dataCache.Add("NotifyTime", alert.AlertTime);
                        dataCache.Add("Id", alert.ID);//restore时需要这些属性；
                        dataCache.Add("Status", (int)alert.Status);
                        ImmedSubscriptions.Add(dataCache);
                    }
                    else if (alert.ItemID <= 0)
                    {
                        dataCache = new Dictionary<string, object>();
                        dataCache.Add("EventType", alert.EventType);
                        if (!string.IsNullOrEmpty(alert.Filter))
                        {
                            dataCache.Add("Filter", alert.Filter);
                        }
                        dataCache.Add("DeliveryChannel", (int)alert.DeliveryChannels);
                        dataCache.Add("AlertTitle", alert.Title);
                        dataCache.Add("UserId", alert.User != null ? alert.User.ID : alert.UserId);
                        dataCache.Add("NotifyFreq", (int)alert.AlertFrequency);
                        dataCache.Add("NotifyTime", alert.AlertTime);
                        dataCache.Add("Id", alert.ID);
                        dataCache.Add("Status", (int)alert.Status);
                        ImmedSubscriptions.Add(dataCache);
                    }
                }
            }
            return ImmedSubscriptions;
        }

        public List<Dictionary<string, object>> GetScheddSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            List<Dictionary<string, object>> schedSubscriptions = new List<Dictionary<string, object>>();
            Dictionary<string, object> dataCache = null;
            foreach (IAveAlert alert in this)
            {
                if (!alert.AlwaysNotify)
                {
                    continue;
                }
                if (alert.ListID == listId)
                {
                    if (itemRowId > 0 && alert.ItemID == itemRowId)
                    {
                        dataCache = new Dictionary<string, object>();
                        dataCache.Add("EventType", alert.EventType);
                        if (!string.IsNullOrEmpty(alert.Filter))
                        {
                            dataCache.Add("Filter", alert.Filter);
                        }
                        dataCache.Add("DeliveryChannel", (int)alert.DeliveryChannels);
                        dataCache.Add("AlertTitle", alert.Title);
                        dataCache.Add("UserId", alert.User.ID);
                        dataCache.Add("NotifyFreq", (int)alert.AlertFrequency);
                        dataCache.Add("NotifyTime", alert.AlertTime);
                        dataCache.Add("Id", alert.ID);
                        dataCache.Add("Status", (int)alert.Status);
                        schedSubscriptions.Add(dataCache);
                    }
                    else if (alert.ItemID <= 0)
                    {
                        dataCache = new Dictionary<string, object>();
                        dataCache.Add("EventType", alert.EventType);
                        if (!string.IsNullOrEmpty(alert.Filter))
                        {
                            dataCache.Add("Filter", alert.Filter);
                        }
                        dataCache.Add("DeliveryChannel", (int)alert.DeliveryChannels);
                        dataCache.Add("AlertTitle", alert.Title);
                        dataCache.Add("UserId", alert.User.ID);
                        dataCache.Add("NotifyFreq", (int)alert.AlertFrequency);
                        dataCache.Add("NotifyTime", alert.AlertTime);
                        dataCache.Add("Id", alert.ID);
                        dataCache.Add("Status", (int)alert.Status);
                        schedSubscriptions.Add(dataCache);
                    }
                }
            }
            return schedSubscriptions;
        }
    }
}
