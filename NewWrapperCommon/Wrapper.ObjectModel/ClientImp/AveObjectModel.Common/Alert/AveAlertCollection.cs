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
using System.Xml;

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
            List<Dictionary<string, object>> alertPropertiesList = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            mListData = new List<IAveAlert>(alertPropertiesList.Count);
            foreach (Dictionary<string, object> alertProperites in alertPropertiesList)
            {
                AveAlert alert = new AveAlert(mWeb, this, mRequest, alertProperites);
                mListData.Add(alert);
            }
        }
        public AveAlertCollection(AveWeb web, IAveRequest request)
        {
            mWeb = web;
            mRequest = request as IAveRequest;
            mListData = new List<IAveAlert>();
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
            throw new NotImplementedException();
        }

        public Guid AddAlert(IAveList list, AveEventType eventType, AveAlertFrequency alertFrequency, bool isSendEmail)
        {
            throw new NotImplementedException();
        }
        public Guid AddAlert(IAveListItem listItem, Dictionary<string, object> data)
        {
            return AddAlert(mWeb.ServerRelativeUrl, listItem.ParentList.DefaultViewUrl, listItem.ParentList.Title, listItem.ParentList.ID, listItem.ID, data);
        }

        public Guid AddAlert(IAveList list, Dictionary<string, object> data)
        {
            return AddAlert(mWeb.ServerRelativeUrl, list.DefaultViewUrl, list.Title, list.ID, -2, data);
        }
        private Guid AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            ChangeAlertData(data);
            var alertProperites = mRequest.AddAlert(webServerRelativeUrl, listUrl, listTitle, listId, itemId, data);
            var alert = new AveAlert(mWeb, this, mRequest, alertProperites);
            mListData.Add(alert);
            return alert.ID;
        }

        private void ChangeAlertData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("Properties"))
            {
                var properties = ConvertXmlPropertyToDictionary(data);

                properties["AlertOldId"] = data["Id"].ToString();
                data["Properties"] = properties;

            }
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
            List<Dictionary<string, object>> alertInfos = new List<Dictionary<string, object>>();
            foreach (IAveAlert alert in this)
            {
                if (alert.AlertFrequency != AveAlertFrequency.Immediate)
                {
                    continue;
                }
                var alertInfo = GetAlertInfo(listId, itemRowId, hostType, alert);
                if (alertInfo != null)
                {
                    alertInfos.Add(alertInfo);
                }
            }
            return alertInfos;
        }

        public List<Dictionary<string, object>> GetScheddSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
            List<Dictionary<string, object>> alertInfos = new List<Dictionary<string, object>>();
            foreach (IAveAlert alert in this)
            {
                if (alert.AlertFrequency == AveAlertFrequency.Immediate)
                {
                    continue;
                }
                var alertInfo =  GetAlertInfo(listId, itemRowId, hostType, alert);
                if (alertInfo != null)
                {
                    alertInfos.Add(alertInfo);
                }
            }
            return alertInfos;
        }

        private Dictionary<string, object> GetAlertInfo(Guid listId, int itemRowId, AveSPAlertHostType hostType, IAveAlert alert)
        {
            Dictionary<string, object> dataCache = null;
            if (alert.ListID == listId)
            {
                if (alert.ItemID == itemRowId && itemRowId > 0 && (hostType == AveSPAlertHostType.Doc || hostType== AveSPAlertHostType.Item) )//Item level
                {
                    dataCache = GenerateAlertInfo(alert);
                }
                else if (alert.ItemID <= 0 && itemRowId <= 0 && (hostType == AveSPAlertHostType.List || hostType == AveSPAlertHostType.Folder))//List&Folder Level   这里有问题，folder 或者list 会多还原Alert，现在区分不开folder 和list  的alert。
                {
                    dataCache = GenerateAlertInfo(alert);
                }
                return dataCache;
            }
            return null;
        }

        /// <summary>
        /// 此处是为了保持和On-premise sql 备份保持一致
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        private string CovertPropertiesToXml(AvePropertyBag properties)
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("miscellaneous"));
            foreach (var property in properties.DataCache.PropertiesCache)
            {
                var propertyNode = document.CreateElement("property");

                var attributeName = document.CreateAttribute("name");
                attributeName.Value = property.Key;
                propertyNode.Attributes.Append(attributeName);

                var attributeValue = document.CreateAttribute("value");
                attributeValue.Value = property.Value.ToString();

                propertyNode.Attributes.Append(attributeValue);
                document.DocumentElement.AppendChild(propertyNode);
            }
            return document.InnerXml;
        }

        private Dictionary<string, string> ConvertXmlPropertyToDictionary(Dictionary<string, object> data)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            string xmlProperty = (string)data["Properties"];
            XmlDocument document = new XmlDocument();
            document.LoadXml(xmlProperty);
            var nodes = document.SelectNodes("//property");
            foreach (XmlNode property in nodes)
            {
                var key = property.Attributes["name"].Value;
                var value = property.Attributes["value"].Value;
                properties[key] = value;
            }
            return properties;
        }

        private Dictionary<string, object> GenerateAlertInfo(IAveAlert alert)
        {
            var dataCache = new Dictionary<string, object>();
            dataCache.Add("EventType", (int)alert.EventType);
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
            dataCache.Add("AlertType", (int)alert.AlertType);
            dataCache.Add("AlertTemplateName", ((AveAlert)alert).AlertTemplateName);
            dataCache.Add("Properties", CovertPropertiesToXml((AvePropertyBag)alert.Properties));
            return dataCache;
        }
    }
}
