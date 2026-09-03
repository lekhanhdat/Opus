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
using System.Text;
using System.Xml;
using System.Reflection;
using AvePoint.Wrapper.Common;

using AvePoint.GCommon;
using System.Threading;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    //public enum AveSPAlertHostType
    //{
    //    AveSPAlertHostTypeList,
    //    AveSPAlertHostTypeFolder,
    //    AveSPAlertHostTypeDoc,
    //    AveSPAlertHostTypeItem
    //}

    public class AveSPAlert : AvePoint.Wrapper.Restore.IAveSPAlert, IDisposable
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static bool StopAlerts = true;

        protected Guid mSiteId = Guid.Empty;
        protected Guid mWebId = Guid.Empty;
        protected Guid mListId = Guid.Empty;
        protected AveSPAlertHostType mHostType;
        protected IAveBackupRestoreQueryService mQueryService = null;
        protected AveEventType mEventType;
        protected Guid mNewAlertID = Guid.Empty;
        protected AveAlertDeliveryChannels mSourDC;
        protected AveAlertFrequency mFrequency;
        protected AveSPMembers mMembers = null;
        protected IAveUser mUser = null;
        protected AveSPList mAveList;
        protected Dictionary<Guid, Guid> mAlerts;
        protected IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }

        public static AveSPAlert CreateInstance(object obj)
        {
            AveSPAlert instance = null;

            if (obj is AveSPList)
            {
                instance = new AveSPListAlert((AveSPList)obj);
            }
            else if (obj is AveSPFolder)
            {
                instance = new AveSPFolderAlert((AveSPFolder)obj);
            }
            else if (obj is AveSPDoc)
            {
                instance = new AveSPDocAlert((AveSPDoc)obj);
            }
            else if (obj is AveSPListItem)
            {
                instance = new AveSPItemAlert((AveSPListItem)obj);
            }
            else
            {
                throw new Exception("Cannot construct an instance for this object type: " + obj.GetType().ToString());
            }

            return instance;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "viewtitle is a key")]
        protected void UpdateAlertProperty(IAveAlert alert, Dictionary<string, object> data)
        {
            if (mAveList.ParentSite.SPSite.IsOnlineSite)
            {
                // Online can not update properties
                return;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.UpdateAlertProperty"))
            {

                string strProperties = "Properties";
                if (data.ContainsKey(strProperties))
                {
                    if (!string.IsNullOrEmpty(data[strProperties].ToString()))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.PreserveWhitespace = true;
                        doc.LoadXml(data[strProperties].ToString());
                        alert.Properties.Clear();
                        string viewTitle = string.Empty;
                        XmlNodeList properties = doc.GetElementsByTagName("property");
                        if (mAveList.SPList.BaseTemplate == AveListTemplateType.Tasks && data.ContainsKey("viewtitle"))
                        {
                            viewTitle = data["viewtitle"].ToString();
                        }
                        foreach (XmlNode node in properties)
                        {
                            string value = node.Attributes["value"].Value;
                            string name = node.Attributes["name"].Value;
                            if (name.Equals("siteurl", StringComparison.OrdinalIgnoreCase))
                            {
                                value = GetServerUrl(mAveList.ParentWeb.ParentSite.SPSite);
                            }
                            else if (name.Equals("mobileurl", StringComparison.OrdinalIgnoreCase))
                            {
                                value = mAveList.ParentSite.ObjectModelFactory.CreateMobileUtility().GetApplicationPath(mAveList.ParentWeb.SPWeb);
                            }
                            else if (name.Equals("dispformurl"))
                            {
                                IAveForm form = mAveList.SPList.Forms[AvePAGETYPE.DisplayForm];
                                if (form != null)
                                {
                                    value = form.Url;
                                }
                                else
                                {
                                    value = mAveList.SPList.RootFolder.Url;
                                }
                            }
                            else if (name.Equals("filterpath"))
                            {
                                if (this is AveSPFolderAlert)
                                {
                                    value = ((AveSPFolderAlert)this).mFolder.SPFolder.ServerRelativeUrl.Trim('/') + "/";
                                }
                                else
                                {
                                    string url = "/" + value;
                                    value = AveReplaceProcessor.UrlReplace(url, mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListUrlMapping, new ReplaceOption(true), mAveList.ParentWeb.ParentSite.SourceSiteInfo, mAveList.ParentWeb.ParentSite.ServerRelativeUrl).Trim('/') + "/";
                                }
                            }
                            else if (name.Equals("viewid"))
                            {
                                if (mAveList.ParentWeb.ParentSite.MappingManager.ListMappingManager.ListViewMapping.ContainsKey(new Guid(value)))
                                {
                                    value = mAveList.ParentWeb.ParentSite.MappingManager.ListMappingManager.ListViewMapping[new Guid(value)].ToString();
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(viewTitle))
                                    {
                                        value = mAveList.SPList.Views[viewTitle].ID.ToString();
                                    }
                                }
                            }
                            alert.Properties[name] = value;
                        }
                    }
                }
                alert.Properties["ALERTOLDID"] = data["Id"].ToString();
                alert.Properties.Update();
                alert.Update(false);

            }

        }

        string GetServerUrl(IAveSite site)
        {
            StringBuilder builder = new StringBuilder(site.Protocol, 0x200);
            builder.Append("//");
            builder.Append(site.HostName);
            if ((site.Protocol.Equals("http:", StringComparison.OrdinalIgnoreCase) && (site.Port != 80))
                || (site.Protocol.Equals("https:", StringComparison.OrdinalIgnoreCase) && (site.Port != 0x1bb)))
            {
                builder.Append(":");
                builder.Append(site.Port);
            }
            return builder.ToString();
        }
        public virtual void RestoreAlerts(List<Dictionary<string, object>> iAlertInfoList, bool isSchedAlert)
        {
            foreach (Dictionary<string, object> iAlertInfo in iAlertInfoList)
            {
                try
                {
                    this.RestoreAlert(iAlertInfo, isSchedAlert);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Failed to restore alert. Error:{0}.", ex.ToString());
                }
            }
        }
        public virtual void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.ProcessItemByWeb"))
            {

                int userId = -1;
                try
                {
                    userId = (int)data["UserId"];
                    if (userId == 0)
                    {
                        return;
                    }
                    Guid alertId = (Guid)data["Id"];
                    IAvePrincipal p = mMembers.FindMember(userId, true, true);//ado-82539
                    if (p == null)
                    {
                        //throw new UserNotFoundException(userId);
                        log.Log(AveLogLevel.WARN, "Can not find user whose Id is {0}.", userId);
                        return;
                    }
                    mUser = (IAveUser)p;
                    mEventType = GetEventType((int)data["EventType"]);
                    IAveAlert alert = null;
                    try
                    {
                        Guid alertOldId = AveAlertUtility.GetAlertOldByProperty((string)data["Properties"]);
                        if (mAlerts.ContainsKey(alertId))
                        {
                            alert = mAveList.ParentWeb.SPWeb.Alerts[mAlerts[alertId]];
                        }
                        else if (mAlerts.ContainsValue(alertOldId))//ADO-101733 解决replicator的two way的问题
                        {
                            alert = mAveList.ParentWeb.SPWeb.Alerts[alertOldId];
                        }
                        else
                        {
                            //Alert在获取时需要判断源端和目的端是否在同一个Web下面
                            alert = mAveList.ParentWeb.SPWeb.Alerts[alertId];
                            if (alert != null && alert.ListID != mAveList.SPList.ID)
                            {
                                alert = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetAlertError, e.ToString());
                    }
                    if (alert == null)
                    {
                        alert = InnerRestoreAlert(data, isSchedAlert);
                    }
                    else
                    {
                        if (isSchedAlert)
                        {
                            alert.EventType = mEventType;
                            UpdateSchedProperties(alert, data);
                        }
                        else
                        {
                            alert.EventType = mEventType;
                            alert.AlertFrequency = AveAlertFrequency.Immediate;
                            alert.Update(false);
                        }
                        UpdatePrivateProperties(alert, data);
                        UpdateSharedProperties(alert, data, mAveList.ParentWeb);
                        UpdateAlertProperty(alert, data);
                    }

                    if (alert != null)
                    {
                        if (mHostType != AveSPAlertHostType.List && !mAveList.ListAlertIDs.Contains(alert.ID))
                        {
                            mAveList.ListAlertIDs.Add(alert.ID);
                        }
                    }
                }
                catch (Exception e)
                {
                    string username = mUser != null ? mUser.LoginName : userId.ToString();
                    string scopeUrl = data.ContainsKey("AlertTitle") ? data["AlertTitle"] as string : string.Empty;
                    report.AddDetail(new AveWrapperReportDto(username, scopeUrl, AveReportObjectType.Alert, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreAlertError, e.Message));
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreAlertFailedEventMessage(username, scopeUrl, e));
                }


            }

        }

        protected virtual IAveAlert InnerRestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {
            return null;
        }
        public AveEventType GetEventType(int mask)
        {
            AveEventType rs = AveEventType.All;
            switch (mask)
            {
                case -1:
                    rs = AveEventType.All;
                    break;
                case 1:
                    rs = AveEventType.Add;
                    break;
                case 2:
                    rs = AveEventType.Modify;
                    break;
                case 4:
                    rs = AveEventType.Delete;
                    break;
                default:
                    break;
            }
            return rs;
        }
        public AveAlertDeliveryChannels GetDeliveryChannel(int mask)
        {
            AveAlertDeliveryChannels rs = AveAlertDeliveryChannels.Email;
            if (mask != 1)
            {
                rs = AveAlertDeliveryChannels.Sms;
            }
            return rs;
        }
        public AveAlertFrequency GetFrequency(int mask)
        {
            switch (mask)
            {
                case 0:
                    return AveAlertFrequency.Immediate;
                case 1:
                    return AveAlertFrequency.Daily;
                case 2:
                default:
                    return AveAlertFrequency.Weekly;
            }
        }
        public void UpdateSharedProperties(IAveAlert alert, Dictionary<string, object> data, AveSPWeb aveWeb)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.UpdateSharedProperties"))
            {

                alert.DeliveryChannels = GetDeliveryChannel((int)data["DeliveryChannel"]);
                alert.Title = (string)data["AlertTitle"];
                if (Convert.ToInt32(data["Status"].ToString()) == 0)
                {
                    alert.Status = AveAlertStatus.Off;
                    aveWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableAlerts(aveWeb.SPWeb.ID, alert.ID);
                }
                else
                {
                    alert.Status = (AveAlertStatus)Convert.ToInt32(data["Status"].ToString());
                }
                int mOldUserId = (int)data["UserId"];
                IAvePrincipal mUser = mMembers.FindMember(mOldUserId, true, true);//ado-82539
                if (mUser != null)
                {
                    alert.User = (IAveUser)mUser;
                }
                alert.Update(false);


            }

        }
        public void UpdateSchedProperties(IAveAlert alert, Dictionary<string, object> data)
        {
            alert.AlertFrequency = GetFrequency((int)data["NotifyFreq"]);
            alert.AlertTime = (DateTime)data["NotifyTime"];
            alert.Update(false);
        }
        public virtual void UpdatePrivateProperties(IAveAlert alert, Dictionary<string, object> data)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.UpdatePrivateProperties"))
            {

                if (data.ContainsKey("Filter"))
                {
                    string filter = data["Filter"].ToString();
                    if (filter.Length > 0)
                    {
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.PreserveWhitespace = true;
                        xDoc.LoadXml(filter);
                        XmlNodeList nodes = xDoc.GetElementsByTagName("Value");
                        foreach (XmlNode node in nodes)
                        {
                            XmlElement parent = node.ParentNode as XmlElement;
                            XmlNodeList children = parent.GetElementsByTagName("FieldRef");
                            if (children.Count > 0)
                            {
                                XmlElement fieldElement = children[0] as XmlElement;
                                if (fieldElement.GetAttribute("Name").Equals("ItemFullUrl", StringComparison.OrdinalIgnoreCase))
                                {
                                    string url = "";
                                    if (mHostType == AveSPAlertHostType.Folder)
                                    {
                                        url = ((AveSPFolderAlert)this).mFolder.SPFolder.ServerRelativeUrl.Trim('/');
                                    }
                                    else
                                    {
                                        url = "/" + node.InnerText.Trim('/');
                                        url = AveReplaceProcessor.UrlReplace(url, mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListUrlMapping, new ReplaceOption(true), mAveList.ParentSite.SourceSiteInfo, mAveList.ParentSite.ServerRelativeUrl).Trim('/');
                                    }
                                    if (parent.Name.Equals("BeginsWith", StringComparison.OrdinalIgnoreCase))
                                    {
                                        node.InnerText = url + "/";
                                    }
                                    else
                                    {
                                        node.InnerText = url;
                                    }
                                }
                                else if (fieldElement.GetAttribute("Name").Equals("Editor/New", StringComparison.OrdinalIgnoreCase)
                                      || fieldElement.GetAttribute("Name").Equals("Editor/Old", StringComparison.OrdinalIgnoreCase)
                                      || fieldElement.GetAttribute("Name").Equals("Author/New", StringComparison.OrdinalIgnoreCase)
                                      || fieldElement.GetAttribute("Name").Equals("Author/Old", StringComparison.OrdinalIgnoreCase)
                                      || fieldElement.GetAttribute("Name").Equals("AssignedTo/New", StringComparison.OrdinalIgnoreCase)
                                      || fieldElement.GetAttribute("Name").Equals("AssignedTo/Old", StringComparison.OrdinalIgnoreCase))
                                {

                                    node.InnerText = mUser.Name;
                                }
                            }
                        }
                        alert.Filter = xDoc.InnerXml;
                        alert.Update(false);
                    }
                }

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "viewtitle is a key")]
        protected void UpdateBPOSAlertProperties(Dictionary<string, object> data)
        {
            int mOldUserId = (int)data["UserId"];
            IAvePrincipal mUser = mMembers.FindMember(mOldUserId, true);
            if (mUser != null)
            {
                IAveUser user = (IAveUser)mUser;
                Dictionary<string, object> userInfo = new Dictionary<string, object>();
                userInfo["UserId"] = mUser.ID;
                userInfo["Name"] = mUser.LoginName;
                userInfo["DisplayName"] = mUser.Name;
                userInfo["Email"] = user.Email;
                data["User"] = userInfo;
            }
            if (data.ContainsKey("Filter") && !string.IsNullOrEmpty(data["Filter"].ToString()))
            {
                string filter = UpdateFilterProperties(data["Filter"].ToString());
                //ADO-204745 暂时没发现有什么用处 先注释掉
                //filter = RemoveFilterPropertiesUser(filter);
                //filter = filter.Replace("FieldRef Name=", "FieldRef name=");
                data["Filter"] = filter;
            }
            object properties;
            if (data.TryGetValue("Properties", out properties))
            {
                string viewTitle = null;
                if (data.ContainsKey("viewtitle"))
                {
                    viewTitle = data["viewtitle"].ToString();
                }

                var propertyString = UpdateAlertPropertiesForBPOS(properties as string, viewTitle);
                data["Properties"] = propertyString;
            }
        }

        protected string UpdateAlertPropertiesForBPOS(string propertyString, string viewTitle)
        {
            if (string.IsNullOrEmpty(propertyString))
            {
                return string.Empty;
            }

            XmlNode viewNode = null;
            bool findView = false;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(propertyString);
            viewNode = xDoc.DocumentElement.SelectSingleNode("property[@name='viewid']");
            if (viewNode != null)
            {
                var viewId = new Guid(viewNode.Attributes["value"].Value);

                if (mAveList.ParentWeb.ParentSite.MappingManager.ListMappingManager.ListViewMapping.ContainsKey(viewId))
                {
                    findView = true;
                    viewNode.Attributes["value"].Value = mAveList.ParentWeb.ParentSite.MappingManager.ListMappingManager.ListViewMapping[viewId].ToString();
                }
                else
                {
                    if (!string.IsNullOrEmpty(viewTitle))
                    {
                        viewNode.Attributes["value"].Value = mAveList.SPList.Views[viewTitle].ID.ToString();
                        findView = true;
                    }
                }
                if (!findView)
                {
                    viewNode.Attributes["value"].Value = string.Empty;
                }
            }
            ReplacePropertyUrl(xDoc, "dispformurl");
            ReplacePropertyUrl(xDoc, "mobileurl");
            ReplacePropertyUrl(xDoc, "siteurl");
            return xDoc.InnerXml;
        }
        private void ReplacePropertyUrl(XmlDocument document, string propertyName)
        {
            var propertyNode = document.DocumentElement.SelectSingleNode(string.Format("property[@name='{0}']", propertyName));
            if (propertyNode != null)
            {
                propertyNode.Attributes["value"].Value = AveReplaceProcessor.UrlReplace(propertyNode.Attributes["value"].Value, mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListUrlMapping, new ReplaceOption(true), mAveList.ParentSite.SourceSiteInfo, mAveList.ParentSite.ServerRelativeUrl).Trim('/');
            }
        }

        protected string UpdateFilterProperties(string filter)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(filter);
            XmlNodeList nodes = xDoc.GetElementsByTagName("Value");
            foreach (XmlNode node in nodes)
            {
                XmlElement parent = node.ParentNode as XmlElement;
                XmlNodeList children = parent.GetElementsByTagName("FieldRef");
                if (children.Count > 0)
                {
                    XmlElement fieldElement = children[0] as XmlElement;
                    if (fieldElement.GetAttribute("Name").Equals("ItemFullUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        string url = "";
                        if (mHostType == AveSPAlertHostType.Folder)
                        {
                            url = ((AveSPFolderAlert)this).mFolder.SPFolder.ServerRelativeUrl.Trim('/');
                        }
                        else
                        {
                            url = "/" + node.InnerText.Trim('/');
                            url = AveReplaceProcessor.UrlReplace(url, mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListUrlMapping, new ReplaceOption(true), mAveList.ParentSite.SourceSiteInfo, mAveList.ParentSite.ServerRelativeUrl).Trim('/');
                        }
                        if (parent.Name.Equals("BeginsWith", StringComparison.OrdinalIgnoreCase))
                        {
                            node.InnerText = url + "/";
                        }
                        else
                        {
                            node.InnerText = url;
                        }
                    }
                    else if (fieldElement.GetAttribute("Name").Equals("Editor/New", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("Editor/Old", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("Author/New", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("Author/Old", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("AssignedTo/New", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("AssignedTo/Old", StringComparison.OrdinalIgnoreCase))
                    {

                        node.InnerText = mUser.Name;
                    }
                }
            }
            return xDoc.InnerXml;
        }

        private string RemoveFilterPropertiesUser(string filter)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(filter);
            XmlNodeList nodes = xDoc.GetElementsByTagName("Value");
            foreach (XmlNode node in nodes)
            {
                XmlElement parent = node.ParentNode as XmlElement;
                XmlNodeList children = parent.GetElementsByTagName("FieldRef");
                if (children.Count > 0)
                {
                    XmlElement fieldElement = children[0] as XmlElement;
                    if (fieldElement.GetAttribute("Name").Equals("Editor/New", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("Editor/Old", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("Author/New", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("Author/Old", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("AssignedTo/New", StringComparison.OrdinalIgnoreCase)
                          || fieldElement.GetAttribute("Name").Equals("AssignedTo/Old", StringComparison.OrdinalIgnoreCase))
                    {
                        node.Attributes["type"].Value = "integer";
                        node.InnerXml = "<UserID />";
                    }
                    else if (fieldElement.GetAttribute("Name").Equals("Priority/Old", StringComparison.OrdinalIgnoreCase)
                    || fieldElement.GetAttribute("Name").Equals("Priority/New", StringComparison.OrdinalIgnoreCase))
                    {
                        node.InnerText = "(1) High";
                    }
                    else if (fieldElement.GetAttribute("Name").Equals("Status/New", StringComparison.OrdinalIgnoreCase)
                        || fieldElement.GetAttribute("Name").Equals("Status/Old", StringComparison.OrdinalIgnoreCase))
                    {
                        node.InnerText = "Completed";
                    }
                }
            }

            if (xDoc.InnerXml.Contains("ItemFullUrl"))
            {
                string filterXml = string.Empty;
                XmlNode node = xDoc.DocumentElement.ChildNodes[0];
                if (node.Name != "And")
                {
                    filterXml = "Anything changes";
                }
                else
                {
                    filterXml = "<Query>";
                    XmlNodeList childs = node.ChildNodes;
                    foreach (XmlNode child in childs)
                    {
                        if (!child.InnerXml.Contains("ItemFullUrl"))
                        {
                            filterXml = filterXml + child.OuterXml;
                        }
                    }
                    filterXml = filterXml + "</Query>";
                }
                return filterXml;
            }
            else
            {
                return xDoc.InnerXml;
            }
        }

        public void Disposed()
        {

        }

        protected void Initial(int itemId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.InitAlert"))
            {

                try
                {
                    mSiteId = mAveList.ParentWeb.ParentSite.SPSite.ID;
                    mWebId = mAveList.ParentWeb.SPWeb.ID;
                    if (mAveList.SPList == null)
                    {
                        return;
                    }
                    mListId = mAveList.SPList.ID;

                    string folderUrl = string.Empty;
                    if (mHostType == AveSPAlertHostType.Folder)
                    {
                        folderUrl = GetItemFullUrl().Trim('/');
                    }

                    //mAlerts = mAveList.SPList.GetAlerts(folderUrl, itemId, mHostType);
                    if (mAveList.ParentWeb.ListAlertIdMappings.ContainsKey(mAveList.SPList.ID))
                    {
                        mAlerts = mAveList.ParentWeb.ListAlertIdMappings[mAveList.SPList.ID];
                    }
                    else
                    {
                        mAlerts = new Dictionary<Guid, Guid>();
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.InitialAlertFailed, e);
                }

            }

        }

        protected virtual string GetItemFullUrl()
        {
            return mAveList.RootFolder.ServerRelativeUrl.Trim('/');
        }
        public static void StopListAlerts(AveSPList aveSPList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.StopListAlerts"))
            {

                if (!StopAlerts || aveSPList.SPList == null)
                {
                    return;
                }
                try
                {
                    if (aveSPList.ParentWeb.SPWeb.Alerts != null)
                    {
                        foreach (IAveAlert alert in aveSPList.ParentWeb.SPWeb.Alerts)
                        {
                            if (alert.ListID != null && alert.Status == AveAlertStatus.On && alert.ListID == aveSPList.SPList.ID)
                            {
                                alert.Status = AveAlertStatus.Off;
                                alert.Update(false);
                                aveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableAlerts(aveSPList.ParentWeb.SPWeb.ID, alert.ID);
                            }
                        }
                        //mListAlertIDs.AddRange(tmpAlertIds);
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Stop List Alert exception: " + ex.ToString());
                }

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        public static void EnableAllAlerts(AveSPSite aveSPSite)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.EnableAlerts"))
            {

                try
                {

                    if (aveSPSite.MappingManager.SiteMappingManager.GetNeedEnableAlertsMappingOnlyForPostAction().Count > 0
                       || aveSPSite.MappingManager.SiteMappingManager.GetNeedEnableSendEmailListMappingOnlyForPostAction().Count > 0)
                    {
                        if (!aveSPSite.SPSite.IsOnlineSite)
                        {
                            foreach (IAveJobDefinition definition in aveSPSite.SPSite.WebApplication.JobDefinitions)
                            {
                                if (definition.Name.Equals("job-immediate-alerts", StringComparison.OrdinalIgnoreCase))
                                {
                                    using (var scope = new AvePerformanceScope("AvePoint.Wrapper.Restore.AveSPAlert.EnableAllAlerts.RunTimerJob"))
                                    {
                                        DateTime originalLastRunTime = definition.LastRunTime;
                                        definition.RunNow();
                                        //如果环境的timer job不起，会一直hang在这里，加入等待的最大时间 ADO-33776
                                        int mostWaitingTimeCount = 0;
                                        while (definition.LastRunTime == originalLastRunTime && mostWaitingTimeCount < 600)
                                        {
                                            Thread.Sleep(1000);
                                            mostWaitingTimeCount++;
                                        }
                                        if (mostWaitingTimeCount == 600)
                                        {
                                            log.Log(AveLogLevel.WARN, "Timer job job-immediate-alerts does not exist after 10 minutes. Disable Status:{0}", definition.IsDisabled.ToString());
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        foreach (Guid webId in aveSPSite.MappingManager.SiteMappingManager.GetNeedEnableAlertsMappingOnlyForPostAction().Keys)
                        {
                            using (IAveWeb web = aveSPSite.SPSite.OpenWeb(webId))
                            {
                                List<Guid> alertIds;
                                aveSPSite.MappingManager.SiteMappingManager.TryGetValueFromNeedEnableAlertsMapping(webId, out alertIds);
                                foreach (Guid alertId in alertIds)
                                {
                                    try
                                    {
                                        IAveAlert alert = web.Alerts[alertId];
                                        alert.Status = AveAlertStatus.On;
                                        //replace the alert view id
                                        try
                                        {
                                            if (alert.Properties != null && alert.Properties.ContainsKey("viewid"))
                                            {
                                                Guid viewID = new Guid(alert.Properties["viewid"]);
                                                Guid viewGuidMappingValue;
                                                if (aveSPSite.MappingManager.SiteMappingManager.GetViewGuidMappingValue(viewID, out viewGuidMappingValue))
                                                {
                                                    alert.Properties["viewid"] = viewGuidMappingValue.ToString();
                                                    alert.Properties.Update();
                                                    alert.Update(false);
                                                }
                                            }
                                        }
                                        catch (AveSecurityTrimingException)
                                        {
                                            throw;
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Error("Restore ViewID of Alert.Error:" + ex.ToString());
                                        }
                                        alert.Update(false);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateAlertFailed, e);
                                    }
                                }
                            }
                        }

                        foreach (var webId in aveSPSite.MappingManager.SiteMappingManager.GetNeedEnableSendEmailListMappingOnlyForPostAction().Keys)
                        {
                            using (IAveWeb web = aveSPSite.SPSite.OpenWeb(webId))
                            {
                                List<Guid> listIds;
                                aveSPSite.MappingManager.SiteMappingManager.TryGetValueFromNeedEnableSendEmailListMapping(webId, out listIds);

                                foreach (Guid listId in listIds)
                                {
                                    try
                                    {
                                        var list = web.Lists[listId];
                                        list.EnableAssignToEmail = true;
                                        list.Update();
                                    }
                                    catch (Exception e)
                                    {
                                        log.Warn("An error occurred while update list EnableAssignToEmail property, list id: {0}, error: {1}",listId, e);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "EnableAllAlerts Error.The message is:{0}", ex.ToString());
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "EnableAllAlerts Error.The message is:{0}", ex.ToString());
                }

            }

        }

        #region IAveSPAlert Members


        public void UpdateSharedProperties(IAveAlert alert, Dictionary<string, object> data, IAveSPWeb aveWeb)
        {
            UpdateSharedProperties(alert, data, aveWeb as AveSPWeb);
        }

        #endregion
        public void Dispose()
        {
            report.Dispose();
        }
    }
    public class AveSPListAlert : AveSPAlert
    {
        public AveSPListAlert(AveSPList list)
        {
            mHostType = AveSPAlertHostType.List;
            mAveList = list;
            mQueryService = list.QueryService;
            mMembers = list.ParentWeb.ParentSite.SPMembers;

            Initial(-1);
        }

        protected override IAveAlert InnerRestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.ListAlert"))
            {

                if (mAveList.ParentSite.SPContextKind == AveContextKind.ClientObjectModel && mAveList.ParentWeb.SPWeb.Site.GetAPIType() == AveAPIType.BPOS_S)
                {
                    UpdateBPOSAlertProperties(data);
                    IAveAlertCollection alerts = mAveList.ParentWeb.SPWeb.Alerts as IAveAlertCollection;
                    var alertId = alerts.AddAlert(mAveList.SPList, data);
                    mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableAlerts(this.mAveList.ParentWeb.SPWeb.ID, alertId);
                    return null;
                }

                mEventType = GetEventType((int)data["EventType"]);
                IAveAlert alert = null;
                if (isSchedAlert)
                {
                    mNewAlertID = mAveList.ParentWeb.SPWeb.Alerts.AddAlert(mAveList.SPList, mEventType, AveAlertFrequency.Daily);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mAveList.ParentWeb.SPWeb.Alerts[mNewAlertID];
                    }
                    if (alert != null)
                    {
                        UpdateSchedProperties(alert, data);
                    }
                }
                else
                {
                    mNewAlertID = mAveList.ParentWeb.SPWeb.Alerts.AddAlert(mAveList.SPList, mEventType, AveAlertFrequency.Immediate);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mAveList.ParentWeb.SPWeb.Alerts[mNewAlertID];
                    }
                }
                if (alert != null)
                {
                    UpdatePrivateProperties(alert, data);
                    UpdateSharedProperties(alert, data, mAveList.ParentWeb);
                    UpdateAlertProperty(alert, data);
                }
                return alert;

            }

        }

        //        public override void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ListAlert"))
        //            {
        //
        //                try
        //                {
        //                    mEventType = GetEventType((int)data["EventType"]);
        //                    IAveAlert mAlert = null;
        //                    if (isSchedAlert)
        //                    {
        //                        mNewAlertID = mList.ParentWeb.SPWeb.Alerts.Add(mList.SPList, mEventType, AveAlertFrequency.Daily);
        //                        if (mNewAlertID != Guid.Empty)
        //                        {
        //                            mAlert = mList.ParentWeb.SPWeb.Alerts[mNewAlertID];
        //                        }
        //                        if (mAlert != null)
        //                        {
        //                            UpdateSchedProperties(mAlert, data);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        mNewAlertID = mList.ParentWeb.SPWeb.Alerts.Add(mList.SPList, mEventType, AveAlertFrequency.Immediate);
        //                        if (mNewAlertID != Guid.Empty)
        //                        {
        //                            mAlert = mList.ParentWeb.SPWeb.Alerts[mNewAlertID];
        //                        }
        //                    }
        //                    if (mAlert != null)
        //                    {
        //                        UpdatePrivateProperties(mAlert, data);
        //                        UpdateSharedProperties(mAlert, data);
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    mLog.Log(AveLogLevel.WARN, "An error occurred while restore List Alert. error:{0}", e.ToString());
        //                    //mLog.Warn("An error occurred while restore List Alert. error:{0}", e.ToString());
        //                }
        //
        //            }
        //
        //        }

        //        public override void UpdatePrivateProperties(IAveAlert alert, Dictionary<string, object> data)
        //        {
        //            base.UpdatePrivateProperties(alert, data);
        //            if (data.ContainsKey("Filter"))
        //            {
        //                string filter = data["Filter"].ToString();
        //                if (filter.Length > 0)
        //                {
        //                    XmlDocument xDoc = new XmlDocument();
        //                    xDoc.LoadXml(filter);
        //                    XmlNodeList nodes = xDoc.GetElementsByTagName("Value");
        //                    foreach (XmlNode node in nodes)
        //                    {
        //                        string oldValue = node.InnerText;
        //                        if (oldValue != null)
        //                        {
        //                            node.InnerText = AveReplaceProcessor.UrlReplace(oldValue, mList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
        //                        }
        //                    }
        //                    alert.Filter = xDoc.InnerXml;
        //                }
        //            }
        //        }
    }

    public class AveSPFolderAlert : AveSPAlert
    {
        internal AveSPFolder mFolder = null;

        public AveSPFolderAlert(AveSPFolder folder)
        {
            mHostType = AveSPAlertHostType.Folder;
            mFolder = folder;
            mAveList = mFolder.ParentList;
            mQueryService = folder.QueryService;
            mMembers = folder.ParentList.ParentWeb.ParentSite.SPMembers;

            Initial(-1);
        }

        protected override IAveAlert InnerRestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.FolderAlert"))
            {

                if (mFolder.ParentSite.SPContextKind == AveContextKind.ClientObjectModel && mFolder.ParentList.ParentWeb.SPWeb.Site.GetAPIType() == AveAPIType.BPOS_S)
                {
                    UpdateBPOSAlertProperties(data);
                    IAveAlertCollection alerts = mFolder.ParentList.ParentWeb.SPWeb.Alerts as IAveAlertCollection;
                    var alertId = alerts.AddAlert(mFolder.ParentList.SPList, data);
                    mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableAlerts(this.mAveList.ParentWeb.SPWeb.ID, alertId);
                    return null;
                }

                mEventType = GetEventType((int)data["EventType"]);
                IAveAlert alert = null;
                if (isSchedAlert)
                {
                    mNewAlertID = mFolder.ParentList.ParentWeb.SPWeb.Alerts.AddAlert(mFolder.ParentList.SPList, mEventType, AveAlertFrequency.Daily);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mFolder.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
                    }
                    if (alert != null)
                    {
                        UpdateSchedProperties(alert, data);
                    }
                }
                else
                {
                    mNewAlertID = mFolder.ParentList.ParentWeb.SPWeb.Alerts.AddAlert(mFolder.ParentList.SPList, mEventType, AveAlertFrequency.Immediate);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mFolder.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
                    }
                }
                if (alert != null)
                {
                    UpdatePrivateProperties(alert, data);
                    UpdateSharedProperties(alert, data, mFolder.ParentList.ParentWeb);
                    UpdateAlertProperty(alert, data);
                }
                return alert;
            }

            //        public override void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
            //        {
            //
            //            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.FolderAlert"))
            //            {
            //
            //                try
            //                {
            //                    mEventType = GetEventType((int)data["EventType"]);
            //                    IAveAlert mAlert = null;
            //                    if (isSchedAlert)
            //                    {
            //                        mNewAlertID = mFolder.ParentList.ParentWeb.SPWeb.Alerts.Add(mFolder.SPFolder.Item, mEventType, AveAlertFrequency.Daily);
            //                        if (mNewAlertID != Guid.Empty)
            //                        {
            //                            mAlert = mFolder.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
            //                        }
            //                        if (mAlert != null)
            //                        {
            //                            UpdateSchedProperties(mAlert, data);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        mNewAlertID = mFolder.ParentList.ParentWeb.SPWeb.Alerts.Add(mFolder.SPFolder.Item, mEventType, AveAlertFrequency.Immediate);
            //                        if (mNewAlertID != Guid.Empty)
            //                        {
            //                            mAlert = mFolder.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
            //                        }
            //                    }
            //                    if (mAlert != null)
            //                    {
            //                        UpdatePrivateProperties(mAlert, data);
            //                        UpdateSharedProperties(mAlert, data);
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    mLog.Log(AveLogLevel.WARN, "An error occurred while restore folder Alert. error:{0}", e.ToString());
            //                }
            //
            //            }
            //
            //        }
            //        public override void UpdatePrivateProperties(IAveAlert alert, Dictionary<string, object> data)
            //        {
            //            base.UpdatePrivateProperties(alert, data);
            //            if (data.ContainsKey("Filter"))
            //            {
            //                string filter = data["Filter"].ToString();
            //                XmlDocument xDoc = new XmlDocument();
            //                xDoc.LoadXml(filter);
            //                XmlNodeList nodes = xDoc.GetElementsByTagName("Value");
            //                foreach (XmlNode node in nodes)
            //                {
            //                    string oldValue = node.InnerText;
            //                    if (oldValue != null)
            //                    {
            //                        node.InnerText = AveReplaceProcessor.UrlReplace(oldValue, mFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true));
            //                    }
            //                }
            //                alert.Filter = xDoc.InnerXml;
            //            }
            //        }

        }

        protected override string GetItemFullUrl()
        {
            return mFolder.ServerRelativeUrl.Trim('/');
        }

    }
    //EventType--Change Type--EventType--EventTypeBitmask
    //-1,All changes ,All,-1
    // 1,New items are added ,add,1
    // 2,Existing items are modified ,Modify,2
    // 4,Items are deleted ,Delete,4

    //<property name="filterindex" value="3" /> 
    // 0,all,Anything changes
    // 1,Someone else changes a document
    // 2,Someone else changes a document created by me
    // 3,Someone else changes a document last modified by me
    public class AveSPDocAlert : AveSPAlert
    {
        private AveSPDoc mDoc = null;
        public AveSPDocAlert(AveSPDoc doc)
        {
            mHostType = AveSPAlertHostType.Doc;
            mDoc = doc;
            mAveList = mDoc.ParentFolder.ParentList;
            mQueryService = doc.ParentFolder.QueryService;
            mMembers = doc.ParentFolder.ParentList.ParentWeb.ParentSite.SPMembers;

            if (mDoc.ParentFolder.ParentList.SPList != null)
            {
                if (mDoc.SPFile.Item != null)
                {
                    Initial(mDoc.SPFile.Item.ID);
                }
                else
                {
                    Initial(-1);
                }
            }
        }
        protected override IAveAlert InnerRestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.DocAlert"))
            {

                if (mDoc.ParentSite.SPContextKind == AveContextKind.ClientObjectModel && mDoc.SPFile.Web.Site.GetAPIType() == AveAPIType.BPOS_S)
                {
                    UpdateBPOSAlertProperties(data);
                    IAveAlertCollection alerts = mDoc.SPWeb.Alerts as IAveAlertCollection;
                    var alertId = alerts.AddAlert(mDoc.SPFile.Item, data);
                    mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableAlerts(this.mAveList.ParentWeb.SPWeb.ID, alertId);
                    return null;
                }
                mEventType = GetEventType((int)data["EventType"]);
                IAveAlert alert = null;
                if (isSchedAlert)
                {
                    mNewAlertID = mDoc.SPWeb.Alerts.AddAlert(mDoc.AveSPItem.SPListItem, mEventType, AveAlertFrequency.Daily);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mDoc.SPWeb.Alerts[mNewAlertID];
                    }
                    if (alert != null)
                    {
                        UpdateSchedProperties(alert, data);
                    }
                }
                else
                {
                    mNewAlertID = mDoc.SPWeb.Alerts.AddAlert(mDoc.AveSPItem.SPListItem, mEventType, AveAlertFrequency.Immediate);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mDoc.SPWeb.Alerts[mNewAlertID];
                    }
                }
                if (alert != null)
                {
                    UpdatePrivateProperties(alert, data);
                    UpdateSharedProperties(alert, data, mDoc.ParentFolder.ParentList.ParentWeb);
                    UpdateAlertProperty(alert, data);
                }
                return alert;
            }

            //        public override void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
            //        {
            //
            //            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.DocAlert"))
            //            {
            //
            //                try
            //                {
            //                    mEventType = GetEventType((int)data["EventType"]);
            //                    IAveAlert mAlert = null;
            //                    if (isSchedAlert)
            //                    {
            //                        mNewAlertID = mDoc.Web.Alerts.Add(mDoc.SPFile.Item, mEventType, AveAlertFrequency.Daily);
            //                        if (mNewAlertID != Guid.Empty)
            //                        {
            //                            mAlert = mDoc.Web.Alerts[mNewAlertID];
            //                        }
            //                        if (mAlert != null)
            //                        {
            //                            UpdateSchedProperties(mAlert, data);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        mNewAlertID = mDoc.Web.Alerts.Add(mDoc.SPFile.Item, mEventType, AveAlertFrequency.Immediate);
            //                        if (mNewAlertID != Guid.Empty)
            //                        {
            //                            mAlert = mDoc.Web.Alerts[mNewAlertID];
            //                        }
            //                    }
            //                    if (mAlert != null)
            //                    {
            //                        UpdatePrivateProperties(mAlert, data);
            //                        UpdateSharedProperties(mAlert, data);
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    mLog.Log(AveLogLevel.WARN, "An error occurred while restore document Alert. error:{0}", e.ToString());
            //                    //mLog.Warn("An error occurred while restore document Alert. error:{0}", e.ToString());
            //                }
            //
            //            }
            //
            //        }

        }

    }

    public class AveSPItemAlert : AveSPAlert
    {
        private AveSPListItem mItem = null;
        public AveSPItemAlert(AveSPListItem item)
        {
            mHostType = AveSPAlertHostType.Item;
            mItem = item;
            mAveList = mItem.ParentList;
            mQueryService = item.ParentList.QueryService;
            mMembers = item.ParentList.ParentWeb.ParentSite.SPMembers;

            Initial(mItem.SPListItem.ID);
        }
        protected override IAveAlert InnerRestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPAlert.ItemAlert"))
            {

                if (mItem.ParentSite.SPContextKind == AveContextKind.ClientObjectModel && mItem.SPWeb.Site.GetAPIType() == AveAPIType.BPOS_S)
                {
                    UpdateBPOSAlertProperties(data);
                    IAveAlertCollection alerts = mItem.SPWeb.Alerts as IAveAlertCollection;
                    var alertId = alerts.AddAlert(mItem.SPListItem, data);
                    mAveList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableAlerts(this.mAveList.ParentWeb.SPWeb.ID, alertId);

                    return null;
                }
                mEventType = GetEventType((int)data["EventType"]);
                IAveAlert alert = null;
                if (isSchedAlert)
                {
                    if (data.ContainsKey("NotifyFreq"))
                    {
                        if ((int)data["NotifyFreq"] == 1)
                        {
                            mNewAlertID = mItem.ParentList.ParentWeb.SPWeb.Alerts.AddAlert(mItem.SPListItem, mEventType, AveAlertFrequency.Daily);
                        }
                        else
                        {
                            mNewAlertID = mItem.ParentList.ParentWeb.SPWeb.Alerts.AddAlert(mItem.SPListItem, mEventType, AveAlertFrequency.Weekly);
                        }
                    }
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mItem.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
                    }
                    if (alert != null)
                    {
                        UpdateSchedProperties(alert, data);
                    }
                }
                else
                {
                    mNewAlertID = mItem.ParentList.ParentWeb.SPWeb.Alerts.AddAlert(mItem.SPListItem, mEventType, AveAlertFrequency.Immediate);
                    if (mNewAlertID != Guid.Empty)
                    {
                        alert = mItem.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
                    }
                }
                if (alert != null)
                {
                    UpdatePrivateProperties(alert, data);
                    UpdateSharedProperties(alert, data, mItem.ParentList.ParentWeb);
                    UpdateAlertProperty(alert, data);
                }
                return alert;
            }
            //        public override void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
            //        {
            //
            //            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.ItemAlert"))
            //            {
            //
            //                try
            //                {
            //                    mEventType = GetEventType((int)data["EventType"]);
            //                    IAveAlert mAlert = null;
            //                    if (isSchedAlert)
            //                    {
            //                        mNewAlertID = mItem.ParentList.ParentWeb.SPWeb.Alerts.Add(mItem.SPListItem, mEventType, AveAlertFrequency.Daily);
            //                        if (mNewAlertID != Guid.Empty)
            //                        {
            //                            mAlert = mItem.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
            //                        }
            //                        if (mAlert != null)
            //                        {
            //                            UpdateSchedProperties(mAlert, data);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        mNewAlertID = mItem.ParentList.ParentWeb.SPWeb.Alerts.Add(mItem.SPListItem, mEventType, AveAlertFrequency.Immediate);
            //                        if (mNewAlertID != Guid.Empty)
            //                        {
            //                            mAlert = mItem.ParentList.ParentWeb.SPWeb.Alerts[mNewAlertID];
            //                        }
            //                    }
            //                    if (mAlert != null)
            //                    {
            //                        UpdatePrivateProperties(mAlert, data);
            //                        UpdateSharedProperties(mAlert, data);
            //                    }
            //                }
            //                catch (Exception e)
            //                {
            //                    mLog.Log(AveLogLevel.WARN, "An error occurred while restore item Alert. error:{0}", e.ToString());
            //                }
            //
            //            }
            //
            //        }

        }

    }
}
