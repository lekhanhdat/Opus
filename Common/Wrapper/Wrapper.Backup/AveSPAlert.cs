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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public abstract class AveSPAlert
    {

        public static AveSPAlert CreateInstance(object obj)
        {
            switch (obj.GetType().Name)
            {
                case "AveSPList":
                    return new AveSPListAlert((AveSPList)obj);
                case "AveSPDoc":
                    return new AveSPDocAlert((AveSPDoc)obj);
                case "AveSPListItem":
                    return new AveSPItemAlert((AveSPListItem)obj);
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }
        }

        public AveSPAlert()
        {
        }

        protected Guid mSiteId = Guid.Empty;
        protected Guid mWebId = Guid.Empty;
        protected Guid mListId = Guid.Empty;
        protected AveSPAlertHostType mHostType;
        protected AveSPList mList = null;
        protected AveSPFolder mFolder = null;
        protected AveSPDoc mDoc = null;
        protected AveSPListItem mItem = null;
        protected IAveWeb mWeb = null;
        protected int mItemId = 0;
        protected string mQueryString = "<Query><Or><Eq><FieldRef Name=\"ItemFullUrl\"/><Value type=\"string\"></Value></Eq><BeginsWith><FieldRef Name=\"ItemFullUrl\"/><Value type=\"string\"></Value></BeginsWith></Or></Query>";
        //"<Query><Or><Eq><FieldRef Name="ItemFullUrl"/><Value type="string">sites/01/shared documents/123</Value></Eq><BeginsWith><FieldRef Name="ItemFullUrl"/><Value type="string">sites/01/shared documents/123/</Value></BeginsWith></Or></Query>";
        //WHERE SiteId=@SiteId AND ListId=@ListId AND ItemId=@ItemId AND AlertType=1 AND Deleted=0

        public void Initial(Guid siteId, Guid webId, Guid listId)
        {
            mSiteId = siteId;
            mWebId = webId;
            mListId = listId;

            switch (mHostType)
            {
                case AveSPAlertHostType.List:
                    mWeb = mList.ParentWeb.SPWeb;
                    break;
                case AveSPAlertHostType.Doc:
                    mWeb = mDoc.AveSPWeb.SPWeb;
                    mItemId = mDoc.AveSPItem.RowId;
                    break;
                case AveSPAlertHostType.Item:
                    mWeb = mItem.AveSPWeb.SPWeb;
                    mItemId = mItem.AveSPItem.RowId;
                    break;
                default:
                    break;
            }
        }

        //private string GetFolderFilterString()
        //{
        //    return mQueryString;
        //}

        public List<AveAlertInfo> GetAlertInfos()
        {
            var immed = GetImmedSubscriptions().Select(dic => ConvertToAveAlertInfo(dic));
            var sched = GetSchedSubscriptions().Select(dic => ConvertToAveAlertInfo(dic));
            return immed.Concat(sched).ToList();
        }

        public List<Dictionary<string, object>> GetImmedSubscriptions()
        {
            if (mWeb == null || mListId == Guid.Empty)
            {
                return new List<Dictionary<string, object>>();
            }
            if (mWeb.Alerts != null)
            {
                return mWeb.Alerts.GetImmedSubscriptions(mSiteId, mWebId, mListId, mItemId, mHostType);
            }
            return new List<Dictionary<string, object>>();
        }

        public List<Dictionary<string, object>> GetSchedSubscriptions()
        {
            if (mWeb == null || mListId == Guid.Empty)
            {
                return new List<Dictionary<string, object>>();
            }
            if (mWeb.Alerts != null)
            {
                return mWeb.Alerts.GetScheddSubscriptions(mSiteId, mWebId, mListId, mItemId, HostType);
            }
            return new List<Dictionary<string, object>>();
        }

        public abstract void ExportImmedSubscriptions(IAveBackupStream output);

        public abstract void ExportSchedSubscriptions(IAveBackupStream output);

        public abstract void Export(IAveBackupStream output);

        public AveSPAlertHostType HostType
        {
            get { return mHostType; }
        }

        private AveAlertInfo ConvertToAveAlertInfo(Dictionary<string, object> dic)
        {
            var info = new AveAlertInfo();
            foreach (var key in dic.Keys)
            {
                if (dic[key] != null)
                {
                    AveAssemblyUtility.SetFieldValue(info, key, dic[key]);
                }
            }
            if (ParentWeb != null && info.UserId > 0)
            {
                AveUserInfo userInfo = ParentWeb.ParentSite.DataCache.GetPrincipalInfo(info.UserId) as AveUserInfo;
                info.UserLogin = userInfo != null ? userInfo.Login : null;
            }
            return info;
        }

        private AveSPWeb ParentWeb
        {
            get
            {
                AveSPWeb web = null;
                switch (mHostType)
                {
                    case AveSPAlertHostType.List:
                        web = mList.ParentWeb;
                        break;
                    case AveSPAlertHostType.Doc:
                        web = mDoc.AveSPWeb;
                        break;
                    case AveSPAlertHostType.Item:
                        web = mItem.AveSPWeb;
                        break;
                    default:
                        break;
                }
                return web;
            }
        }
    }

    public class AveSPListAlert : AveSPAlert
    {
        public AveSPListAlert(AveSPList list)
        {
            mHostType = AveSPAlertHostType.List;
            mList = list;
            Initial(list.ParentWeb.ParentSite.SPSite.ID, list.ParentWeb.SPWeb.ID, list.Id);
        }

        public override void ExportImmedSubscriptions(IAveBackupStream output)
        {
            List<Dictionary<string, object>> dataCache = mList.ImmedSubscriptionsCache;
            if (dataCache == null)
            {
                dataCache = GetImmedSubscriptions();
            }
            if (dataCache != null && dataCache.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.DocImmedSubscriptions, dataCache);
            }
        }

        public override void ExportSchedSubscriptions(IAveBackupStream output)
        {
            List<Dictionary<string, object>> dataCache = mList.SchedSubscriptionsCache;
            if (dataCache == null)
            {
                dataCache = GetSchedSubscriptions();
            }
            if (dataCache != null && dataCache.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.DocSchedSubscriptions, dataCache);
            }
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.Alerts"))
            {
                ExportImmedSubscriptions(output);
                ExportSchedSubscriptions(output);
            }
        }
    }

    public class AveSPDocAlert : AveSPAlert
    {
        public AveSPDocAlert(AveSPDoc doc)
        {
            mHostType = AveSPAlertHostType.Doc;
            mDoc = doc;
            Initial(doc.ParentSite.SPSite.ID, doc.AveSPWeb.SPWeb.ID, doc.AveSPItem.ListId);
        }

        public override void ExportImmedSubscriptions(IAveBackupStream output)
        {
            List<Dictionary<string, object>> dataCache = mDoc.AveSPItem.ImmedSubscriptionsCache;
            if (dataCache == null)
            {
                dataCache = GetImmedSubscriptions();
            }
            if (dataCache.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.DocImmedSubscriptions, dataCache);
            }
        }

        public override void ExportSchedSubscriptions(IAveBackupStream output)
        {
            List<Dictionary<string, object>> dataCache = mDoc.AveSPItem.SchedSubscriptionsCache;
            if (dataCache == null)
            {
                dataCache = GetSchedSubscriptions();
            }
            if (dataCache.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.DocSchedSubscriptions, dataCache);
            }
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.Alerts"))
            {
                ExportImmedSubscriptions(output);
                ExportSchedSubscriptions(output);
            }
        }
    }

    public class AveSPItemAlert : AveSPAlert
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPItemAlert(AveSPListItem item)
        {
            mHostType = AveSPAlertHostType.Item;
            mItem = item;
            Initial(item.AveSPSite.SPSite.ID, item.AveSPWeb.SPWeb.ID, item.AveSPItem.ListId);
        }

        public override void ExportImmedSubscriptions(IAveBackupStream output)
        {
            List<Dictionary<string, object>> dataCache = mItem.AveSPItem.ImmedSubscriptionsCache;
            if (dataCache == null)
            {
                dataCache = GetImmedSubscriptions();
            }
            if (dataCache != null && dataCache.Count > 0)
            {
                if (mItem.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.Tasks)
                {
                    ExtensionInfoBackupForAlert(dataCache);
                }
                output.WriteMetadata(AveMetadataType.DocImmedSubscriptions, dataCache);
            }
        }

        public override void ExportSchedSubscriptions(IAveBackupStream output)
        {
            List<Dictionary<string, object>> dataCache = mItem.AveSPItem.SchedSubscriptionsCache;
            if (dataCache == null)
            {
                dataCache = GetSchedSubscriptions();
            }
            if (dataCache != null && dataCache.Count > 0)
            {
                if (mItem.AveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.Tasks)
                {
                    ExtensionInfoBackupForAlert(dataCache);
                }
                output.WriteMetadata(AveMetadataType.DocSchedSubscriptions, dataCache);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "viewtitle is a key of dictionary")]
        public void ExtensionInfoBackupForAlert(List<Dictionary<string, object>> dataCache)
        {
            for (int i = 0; i < dataCache.Count; i++)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(dataCache[i]["Properties"].ToString());
                    XmlNodeList nodeList = xmlDoc.GetElementsByTagName("property");
                    for (int j = 0; j < nodeList.Count; j++)
                    {
                        if (nodeList[j].Attributes["name"].Value.Equals("viewid"))
                        {
                            string viewTitle = mItem.AveSPItem.AveSPList.SPList.Views[new Guid(nodeList[j].Attributes["value"].Value)].Title;
                            dataCache[i].Add("viewtitle", viewTitle);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn(string.Format("An error occurred while get the view's title that related to alert{0}:{1}", dataCache[i]["Id"].ToString(), ex.ToString()));
                }
            }
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.Alerts"))
            {
                ExportImmedSubscriptions(output);
                ExportSchedSubscriptions(output);
            }
        }
    }
}