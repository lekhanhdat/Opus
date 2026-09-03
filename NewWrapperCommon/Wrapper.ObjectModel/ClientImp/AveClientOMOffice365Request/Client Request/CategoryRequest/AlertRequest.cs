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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {
        [ReplaceByAPI]
        public override Dictionary<string, object> GetAlerts(string webServerRelativeUrl)
        {
            List<Dictionary<string, object>> alertPropertiesList = new List<Dictionary<string, object>>();
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                context.Load(web.Alerts, alerts => alerts.IncludeWithDefaultProperties(alert => alert.ListID, alert => alert.ListUrl));
                context.ExecuteQuery();
                foreach (var alert in web.Alerts)
                {
                    LoadAlertSpecialProperty(context, alert);
                }
                if (context.HasPendingRequest)
                {
                    context.ExecuteQuery();
                }
                foreach (var alert in web.Alerts)
                {
                    Dictionary<string, object> alertProperties = LoadAlertProprty(alert);
                    alertPropertiesList.Add(alertProperties);
                }
            }
            return new Dictionary<string, object> { { AveObjectModelConstant.ChildrenProperties, alertPropertiesList } };
        }
        private void LoadAlertSpecialProperty(AveClientContext context, Alert alert)
        {
            if (alert.AlertType == AlertType.Item)
            {
                context.Load(alert, al => al.ItemID);
            }
            if (alert.AlertFrequency != AlertFrequency.Immediate)
            {
                context.Load(alert, al => al.AlertTime);
            }
        }
        private Dictionary<string, object> LoadAlertProprty(Alert alert)
        {
            Dictionary<string, object> alertProperties = new Dictionary<string, object>();
            CopyProperty(alertProperties, alert);

            #region Reset Properties 
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (var property in alert.Properties)
            {
                properties.Add(property.Key, property.Value);
            }
            alertProperties.Add("Properties" + AveObjectModelConstant.ObjectPropertySuffix, properties);
            alertProperties.Remove("Properties");
            #endregion
            return alertProperties;
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties)
        {
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                var alert = web.Alerts.GetById(alertId);
                AveObjectCopy.UpdateObjectBasicProperties(needUpdateAlertProperties, alert);
                alert.UpdateAlert();
                context.ExecuteQuery();
                return LoadAlertProprty(context, web, alertId);
            }
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            List<Dictionary<string, object>> alertPropertiesList = new List<Dictionary<string, object>>();
            ClientResult<Guid> alertId = null;
            using (AveClientContext context = CreateContext(this.WebAppName + webServerRelativeUrl))
            {
                Web web = context.Web;
                var list = web.Lists.GetById(listId);
                var item = itemId > 0 ? list.GetItemById(itemId) : null;
                var user = web.SiteUsers.GetById((int)(((Dictionary<string, object>)data["User"])["UserId"]));
                var alertCreateInfo = new AlertCreationInformation
                {
                    List = list,
                    Item = item,
                    User = user,
                    AlertType = (AlertType)(int.Parse(data["AlertType"].ToString())),
                    Title = (string)data["AlertTitle"],
                    AlertFrequency = data.ContainsKey("NotifyFreq") ? (AlertFrequency)data["NotifyFreq"] : AlertFrequency.Immediate,
                    AlertTime = data.ContainsKey("NotifyTime") ? (DateTime)data["NotifyTime"] : default(DateTime),
                    EventType = (AlertEventType)data["EventType"],
                    Status = AlertStatus.Off,
                    DeliveryChannels = (AlertDeliveryChannel)data["DeliveryChannel"],
                    Filter = data.ContainsKey("Filter") ? data["Filter"].ToString() : string.Empty,
                    Properties = (Dictionary<string, string>)data["Properties"],
                    AlertTemplateName = data.ContainsKey("AlertTemplateName") ? data["AlertTemplateName"].ToString() : string.Empty,
                };
                alertId = web.Alerts.Add(alertCreateInfo);
                context.ExecuteQuery();
                return LoadAlertProprty(context, web, alertId.Value);
            }
        }

    }
}
