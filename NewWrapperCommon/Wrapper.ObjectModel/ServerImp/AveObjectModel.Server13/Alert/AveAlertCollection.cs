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
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace AvePoint.ObjectModel.Server13
{
    class AveAlertCollection : AveAbstractCommonCollection<IAveAlert>, IAveAlertCollection
    {
        private SPAlertCollection mAlertCollection;
        private AveWeb mWeb;
        private AveSite mSite;
        Dictionary<Guid, bool> mListContainsAlerts = new Dictionary<Guid, bool>();

        public AveAlertCollection(AveWeb web, SPAlertCollection alertCollection)
            : base(alertCollection)
        {
            mWeb = web;
            mSite = web.Site as AveSite;
            mAlertCollection = alertCollection;
        }

        internal SPAlertCollection AlertCollection
        {
            get
            {
                return mAlertCollection;
            }
        }

        #region IAveAlertCollection Members

        public IAveAlert this[Guid alertId]
        {
            get
            {
                return new AveAlert(this, mAlertCollection[alertId]);
            }
        }

        public IAveAlert Add()
        {
            return new AveAlert(this, mAlertCollection.Add());
        }

        public Guid AddAlert(IAveListItem listItem, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return AddAlert((listItem as AveListItem).ListItem, (SPEventType)eventType, (SPAlertFrequency)alertFrequency);
        }

        public Guid AddAlert(IAveList list, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return AddAlert((list as AveList).List, (SPEventType)eventType, (SPAlertFrequency)alertFrequency);
        }

        public Guid Add(IAveListItem listItem, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return mAlertCollection.Add((listItem as AveListItem).ListItem, (SPEventType)eventType, (SPAlertFrequency)alertFrequency);
        }

        public Guid Add(IAveList list, AveEventType eventType, AveAlertFrequency alertFrequency)
        {
            return mAlertCollection.Add((list as AveList).List, (SPEventType)eventType, (SPAlertFrequency)alertFrequency);
        }

        public void Delete(Guid idAlert)
        {
            mAlertCollection.Delete(idAlert);
        }

        public void Delete(int index)
        {
            mAlertCollection.Delete(index);
        }

        private Guid AddAlert(string alertTitle, Guid guidKey, uint uintKey, int eventType, SPAlertFrequency alertFrequency, DateTime alertTime, SPAlertStatus status,
            SPUser recipient, string bstrItemDocUrl, int AlertTypeAndScopeBits, string strAlertTemplateName, string filter, string bstrProperties, bool bSendMail, SPAlertDeliveryChannels deliveryChannels)
        {
            Guid alertId = (Guid)AveAssemblyUtility.InvokeMethod(mWeb.Web.Alerts, typeof(SPAlertCollection), "Add",
                new Type[] { typeof(string), typeof(Guid), typeof(uint), typeof( int), typeof( SPAlertFrequency), typeof( DateTime), 
                    typeof( SPAlertStatus), typeof( SPUser), typeof( string), typeof( int), typeof( string), 
                    typeof( string), typeof( string), typeof( bool), typeof( SPAlertDeliveryChannels )},
                new object[] { alertTitle, guidKey, uintKey, eventType, alertFrequency, alertTime, status, recipient, bstrItemDocUrl, AlertTypeAndScopeBits, strAlertTemplateName, filter, bstrProperties, bSendMail, deliveryChannels });
            AveAssemblyUtility.SetFieldValue(mWeb.Web.Alerts, "m_user", null);
            return alertId;
        }

        protected Guid AddAlert(SPListItem item, SPEventType type, SPAlertFrequency fre)
        {
            string bstrItemDocUrl = null;
            object obj2 = item["FileRef"];
            if ((item.ParentList.BaseType == SPBaseType.DocumentLibrary) && (obj2 != null))
            {
                bstrItemDocUrl = (string)AveAssemblyUtility.InvokeMethod(mWeb.Web, typeof(SPWeb), "GetServerRelativeUrlFromUrl", new Type[] { typeof(string) }, new object[] { obj2.ToString() });//mAveList.ParentWeb.SPWeb.GetServerRelativeUrlFromUrl(obj2.ToString());
            }
            return AddAlert(null, item.ParentList.ID, Convert.ToUInt32(item.ID, CultureInfo.InvariantCulture), (int)type, fre, DateTime.Now, SPAlertStatus.Off, mWeb.Web.CurrentUser, bstrItemDocUrl, 1, (item.ParentList.AlertTemplate != null) ? item.ParentList.AlertTemplate.Name : "", null, null, false, SPAlertDeliveryChannels.Email);
        }

        protected Guid AddAlert(SPList list, SPEventType type, SPAlertFrequency fre)
        {
            return AddAlert(null, new Guid(list.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture)), 0, (int)type, fre, DateTime.Now, SPAlertStatus.Off, list.ParentWeb.CurrentUser, null, 0, (list.AlertTemplate != null) ? list.AlertTemplate.Name : "", null, null, false, SPAlertDeliveryChannels.Email);
        }

        #endregion

        public override IAveAlert this[int index]
        {
            get
            {
                return new AveAlert(this, mAlertCollection[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveAlert(this, t as SPAlert);
        }

        public override int Count
        {
            get { return mAlertCollection.Count; }
        }

        public IAveWeb Web
        {
            get { return mWeb; }
        }

        public List<Dictionary<string, object>> GetImmedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            if (!ListContainsAlerts(siteId, listId))
            {
                return new List<Dictionary<string, object>>();
            }
            return mSite.QueryService.GetImmedSubscriptions(siteId, webId, listId, itemRowId, hostType);
        }

        public List<Dictionary<string, object>> GetScheddSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            if (!ListContainsAlerts(siteId, listId))
            {
                return new List<Dictionary<string, object>>();
            }
            return mSite.QueryService.GetSchedSubscriptions(siteId, webId, listId, itemRowId, hostType);
        }


        #region For Performance
        private bool ListContainsAlerts(Guid siteId, Guid listId)
        {
            if (!mListContainsAlerts.ContainsKey(listId))
            {
                mListContainsAlerts.Add(listId, mSite.QueryService.ListHasLerts(siteId, listId));
            }
            return mListContainsAlerts[listId];
        }

        #endregion


        public Guid AddAlert(IAveListItem listItem, Dictionary<string, object> data)
        {
            Guid id = AddAlert(listItem, (AveEventType)data["EventType"], (AveAlertFrequency)data["NotifyFreq"]);
            IAveAlert alert = this[id];
            alert.Title = data["AlertTitle"].ToString();
            Dictionary<string, object> userInfo = data["User"] as Dictionary<string, object>;
            string userName = userInfo["Name"] as string;
            IAveUser user = this.mWeb.SiteUsers[userName];
            if (user != null)
            {
                alert.User = user;
            }
            alert.Update();
            return alert.ID; 
        }

        public Guid AddAlert(IAveList list, Dictionary<string, object> data)
        {
            Guid id = AddAlert(list, (AveEventType)data["EventType"], (AveAlertFrequency)data["NotifyFreq"]);
            IAveAlert alert = this[id];
            alert.Title = data["AlertTitle"].ToString();
            Dictionary<string, object> userInfo = data["User"] as Dictionary<string, object>;
            string userName = userInfo["Name"] as string;
            IAveUser user = this.mWeb.SiteUsers[userName];
            if (user != null)
            {
                alert.User = user;
            }
            alert.Update();
            return alert.ID;
        }



        public new System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
