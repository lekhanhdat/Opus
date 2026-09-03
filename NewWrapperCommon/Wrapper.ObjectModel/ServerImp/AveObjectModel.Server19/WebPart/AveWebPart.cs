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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.Visio.Server.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Applications.GroupBoard.WebPartPages;
using Microsoft.SharePoint.Publishing.WebControls;
using Microsoft.SharePoint.WebPartPages;
using AvePoint.Common;
using Microsoft.Office.Server.WebControls;
using Microsoft.Office.Server.Internal.Charting.Data;
using System.Web.Script.Serialization;
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server19
{
    #region Remove class AveSPWeb
    //[SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    //[AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_10, CodeReviewConstants.CHECK_LIST_ID_CO_12 }, null, true, null)]
    //class AveSPWebPart : AveWebPart
    //{
    //    protected WebPart mWebPart;

    //    public AveSPWebPart(WebPart spWebPart)
    //        : base(spWebPart)
    //    {
    //        mWebPart = spWebPart;
    //    }

    //    public AveSPWebPart(AveLimitedWebPartManager manager)
    //        : base(manager)
    //    {
    //        //mWebPart = spWebPart;
    //    }

    //    public AveSPWebPart()
    //    {
    //    }

    //    internal WebPart WebPart
    //    {
    //        get
    //        {
    //            return mWebPart;
    //        }
    //    }

    //    #region IAveWebPart Members

    //    public override Guid StorageKey
    //    {
    //        get
    //        {
    //            return mWebPart.StorageKey;
    //        }
    //    }

    //    public override string ZoneID
    //    {
    //        get
    //        {
    //            return mWebPart.ZoneID;
    //        }
    //        set
    //        {
    //            mWebPart.ZoneID = value;
    //        }
    //    }

    //    #endregion
    //}
    #endregion

    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    public class AveWebPart : IAveWebPart
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(AveWebPart));

        protected AveLimitedWebPartManager manager;
        protected AveWebPartBaseInfo webPartBaseInfo;
        protected string assemblyName;
        protected string webPartType;
        protected Guid webPartId = Guid.Empty;
        private Dictionary<string, object> webPartProperties;
        protected bool isMossWebPart;
        protected bool isIListWebPart = false;
        protected bool isClientWebPart = false;
        protected bool isProjectSummaryWebPart = false;
        protected bool internalAdd = false;
        protected bool isShared = true;
        protected bool isViewWebPart = false;
        //WebPartBaseInfo.UserId对应的目的端UserId
        //由于结构限制，先将destUserId设定成internal
        protected int destUserId = 0;
        protected IReport mReport = null;

        [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
        public AveWebPart(AveLimitedWebPartManager manager)
        {
            this.manager = manager;
            mReport = manager.Report;
        }

        public AveWebPart(AveLimitedWebPartManager manager, System.Web.UI.WebControls.WebParts.WebPart webPart, int userId)
        {
            if (webPart == null) throw new ArgumentNullException("webPart");
            //ADO-149421 RC Search Service中有一处调用manager为null 故进行处理来排除空引用，此处manager 为空不影响逻辑。
            if (manager != null)
            {
                this.manager = manager;
                mReport = manager.Report;
            }
            this.internalWebPart = webPart;
            //init properties
            if (webPart is WebPart)
            {
                webPartId = (webPart as WebPart).StorageKey;
            }
            destUserId = userId;
            isShared = userId <= 0;
        }

        public AveLimitedWebPartManager Manager
        {
            get { return this.manager; }
        }

        protected System.Web.UI.WebControls.WebParts.WebPart internalWebPart;
        internal System.Web.UI.WebControls.WebParts.WebPart WebPart
        {
            get { return internalWebPart; }
        }

        internal Dictionary<string, object> WebPartProperties
        {
            get
            {
                EnsureWebPartProperties();
                return webPartProperties;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
        public static IAveWebPart CreateInstance(AveLimitedWebPartManager manager, System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            if (webPart == null)
            {
                return null;
            }
            string type = webPart.GetType().Name;
            AveWebPart aveWebPart;
            switch (type)
            {
                case "XsltListViewWebPart":
                    aveWebPart = new AveXsltListViewWebPart(manager, webPart as XsltListViewWebPart);
                    break;
                case "ListViewWebPart":
                    aveWebPart = new AveListViewWebPart(manager, webPart as ListViewWebPart);
                    break;
                case "ListFormWebPart":
                    aveWebPart = new AveListFormWebPart(manager, webPart as ListFormWebPart);
                    break;
                case "DataFormWebPart":
                    aveWebPart = new AveDataFormWebPart(manager, webPart as DataFormWebPart);
                    break;
                case "XsltListFormWebPart":
                    aveWebPart = new AveXsltListFormWebPart(manager, webPart as XsltListFormWebPart);
                    break;
                case "MembersWebPart":
                    aveWebPart = new AveMembersWebPart(manager, webPart as MembersWebPart);
                    break;
                case "ContentEditorWebPart":
                    aveWebPart = new AveContentEditorWebPart(manager, webPart as ContentEditorWebPart);
                    break;
                default:
                    aveWebPart = new AveWebPart(manager, webPart, -1);
                    break;
            }
            return aveWebPart;
        }

        #region IAveWebPart Members

        public string Height
        {
            get
            {
                return WebPart.Height.ToString();
            }
            set
            {
                WebPart.Height = AveWebPartUtility.ConvertStringToUnit(value);
            }
        }

        public string Width
        {
            get
            {
                return WebPart.Width.ToString();
            }
            set
            {
                WebPart.Width = AveWebPartUtility.ConvertStringToUnit(value);
            }
        }

        public string TitleUrl
        {
            get
            {
                return WebPart.TitleUrl;
            }
            set
            {
                WebPart.TitleUrl = value;
            }
        }

        public string Title
        {
            get
            {
                return WebPart.Title;
            }
            set
            {
                WebPart.Title = value;
            }
        }

        public string ZoneID
        {
            get
            {
                if (this.WebPart != null && (this.WebPart is WebPart))
                {
                    return (this.WebPart as WebPart).ZoneID;
                }
                return string.Empty;
            }
            set
            {
                if (this.WebPart != null && (this.WebPart is WebPart))
                {
                    (this.WebPart as WebPart).ZoneID = value;
                }
            }
        }

        public bool Hidden
        {
            get
            {
                return WebPart.Hidden;
            }
            set
            {
                WebPart.Hidden = value;
            }
        }

        public bool AllowClose
        {
            get { return WebPart.AllowClose; }
            set { WebPart.AllowClose = value; }
        }

        public bool AllowEdit
        {
            get { return WebPart.AllowEdit; }
            set { WebPart.AllowEdit = value; }
        }

        public bool AllowHide
        {
            get { return WebPart.AllowHide; }
            set { WebPart.AllowHide = value; }
        }

        public System.Web.UI.WebControls.WebParts.PartChromeType ChromeType
        {
            get
            {
                return WebPart.ChromeType;
            }
            set
            {
                WebPart.ChromeType = value;
            }
        }

        public string ID
        {
            get
            {
                return WebPart.ID;
            }
            set
            {
                WebPart.ID = value;
            }
        }

        public void SetWebPartProperty(string propertyName, object value)
        {
            Type objType = WebPart.GetType();
            PropertyInfo property = objType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                if (value != null && value.GetType() != property.PropertyType)
                {
                    Type propertyType = property.PropertyType;
                    #region
                    if (propertyType == typeof(int))
                    {
                        value = Convert.ToInt32(value);
                    }
                    else if (propertyType == typeof(string))
                    {
                        value = Convert.ToString(value);
                    }
                    else if (propertyType == typeof(long))
                    {
                        value = Convert.ToInt64(value);
                    }
                    else if (propertyType == typeof(uint))
                    {
                        value = Convert.ToUInt32(value);
                    }
                    else if (propertyType == typeof(bool))
                    {
                        value = Convert.ToBoolean(value);
                    }
                    else if (propertyType == typeof(Guid))
                    {
                        value = new Guid(value.ToString());
                    }
                    else if (propertyType == typeof(short))
                    {
                        value = Convert.ToInt16(value);
                    }
                    else if (propertyType.BaseType.ToString().Equals("System.Enum"))
                    {
                        value = Enum.Parse(propertyType, value.ToString());
                    }
                    else if (propertyType == typeof(XmlElement))
                    {
                        XmlElement realvalue = property.GetValue(WebPart, null) as XmlElement;
                        if (realvalue != null)
                        {
                            realvalue.InnerText = value.ToString();
                        }
                        value = realvalue;
                    }
                    #endregion
                }
                property.SetValue(WebPart, value, null);
            }
            else
            {
                //add for client webpart property
                if (WebPart is ClientWebPart)
                {
                    ClientWebPart clientWebPart = WebPart as ClientWebPart;
                    clientWebPart.Properties.Add(new ClientWebPartProperty() { Name = propertyName, Value = value.ToString() });
                }
            }
        }

        public string GetWebPartStringProperty(string propertyName)
        {
            Type objType = WebPart.GetType();
            PropertyInfo property = objType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return property.GetValue(WebPart, null).ToString();
        }

        public string AuthorizationFilter
        {
            get
            {
                return WebPart.AuthorizationFilter;
            }
            set
            {
                WebPart.AuthorizationFilter = value;
            }
        }

        public int ZoneIndex
        {
            get
            {
                return WebPart.ZoneIndex;
            }
        }

        public string RealWebPartType
        {
            get
            {
                return WebPart.GetType().ToString();
            }
        }

        public bool IsClosed
        {
            get { return WebPart.IsClosed; }
        }

        #endregion

        #region IWebPart Members

        public string CatalogIconImageUrl
        {
            get
            {
                return WebPart.CatalogIconImageUrl;
            }
            set
            {
                WebPart.CatalogIconImageUrl = value;
            }
        }

        public string Description
        {
            get
            {
                return WebPart.Description;
            }
            set
            {
                WebPart.Description = value;
            }
        }

        public string Subtitle
        {
            get { return WebPart.Subtitle; }
        }

        public string TitleIconImageUrl
        {
            get
            {
                return WebPart.TitleIconImageUrl;
            }
            set
            {
                WebPart.TitleIconImageUrl = value;
            }
        }

        #endregion

        public Guid StorageKey
        {
            get
            {
                if (webPartId != Guid.Empty)
                {
                    return webPartId;
                }
                if (this.WebPart != null && (this.WebPart is WebPart))
                {
                    webPartId = (this.WebPart as WebPart).StorageKey;
                }
                return webPartId;
            }
        }

        internal bool UpdateWebPartByType(bool beforeAdd, out bool needReloadList)
        {
            needReloadList = false;
            var updater = SpecialWebPartUpdater.GetWebPartUpdater(this.internalWebPart, this);
            if (updater == null)
            {
                return true;
            }
            try
            {
                //return beforeAdd ? updater.DoUpateBeforeAdd(webPartBaseInfo) : updater.DoUpateAfterAdd(webPartBaseInfo);
                if (beforeAdd)
                {
                    return updater.DoUpateBeforeAdd(webPartBaseInfo);
                }
                else
                {
                    // 不能先给needReloadList赋值在调用DoUpateAfterAdd，NeedReloadList会在DoUpateAfterAdd里面改变
                    var rtnValue = updater.DoUpateAfterAdd(webPartBaseInfo);
                    needReloadList = updater.NeedReloadList;
                    return rtnValue;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Can't update web part: {0}.", ex);
                return true;
            }
        }


        public bool AddUnRestoreWebPartInfo(Guid webId, Guid listId, string file, AveWebPartBaseInfo baseInfo)
        {
            if (this.Manager.Cache.IsSitePostRestore)
            {
                throw new Exception(string.Format("Failed to restore web part because of missing web part data. File Url: {0}, WebPartId: {1}", file, baseInfo.ID));
            }
            this.manager.Cache.SiteMappingManager.AddUnRestoreWebPartInfo(webId, listId, file, baseInfo);
            return true;
        }

        public bool AddUnRestoreWebPartInfo(Guid webId, Guid listId, string file, Guid wpId)
        {
            if (this.Manager.Cache.IsSitePostRestore)
            {
                throw new Exception(string.Format("Failed to restore web part because of missing web part data. File Url: {0}, WebPartId: {1}", file, wpId));
            }
            AveWebPartPostActionInfo info = new AveWebPartPostActionInfo() { WebPartId = wpId, UserId = destUserId };
            this.manager.Cache.SiteMappingManager.AddUnRestoreWebPartInfo(webId, listId, file, info);
            return true;
        }

        internal Guid GetMappingWebId(Guid webId)
        {
            if (webId == Guid.Empty)
            {
                return Guid.Empty;
            }
            Guid mappingId = Guid.Empty;
            if (this.Manager.Cache.SiteMappingManager.WebIDMapping.TryGetValue(webId, out mappingId))
            {
                return mappingId;
            }
            //说明已经替换过了，例如PostAction中还原WebPart
            if (this.Manager.Cache.SiteMappingManager.WebIDMapping.ContainsValue(webId))
            {
                return webId;
            }

            return mappingId;
        }

        internal Guid GetMappingListId(Guid webId, Guid listId, string title)
        {
            if (listId == Guid.Empty && string.IsNullOrEmpty(title))
            {
                return Guid.Empty;
            }

            Guid mappingId = Guid.Empty;
            if (listId != Guid.Empty && this.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out mappingId))
            {
                return mappingId;
            }
            if (listId != Guid.Empty && this.Manager.Cache.SiteMappingManager.ListIdMappingContainsValue(listId))
            {
                return listId;
            }

            return GetListIdByTitle(webId, title);
        }

        internal Guid GetListIdByTitle(Guid webId, string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return Guid.Empty;
            }
            return manager.Web.Site.GetListId(webId, title);
        }

        public AveWebPartBaseInfo WebPartInfo
        {
            get
            {
                return webPartBaseInfo;
            }
            set
            {
                webPartBaseInfo = value;
            }
        }

        public string WebPartTypeID
        {
            get
            {
                return (string)AveAssemblyUtility.GetPropertyValue(WebPart, "WebPartTypeID");
            }

        }

        protected bool EnsureViewInfo(SPList list = null)
        {
            if (webPartBaseInfo.View == null) return true;

            if (list == null && webPartBaseInfo.ListId != Guid.Empty)
            {
                try
                {
                    list = (this.Manager.Web.GetList(webPartBaseInfo.ListId) as AveList).List;
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, "Cannot get list related to web part. List Id: {0}, exception: {1}", webPartBaseInfo.ListId, ex);
                }
            }
            if (list != null)
            {
                IAveFieldMapping fieldMapping = null;
                bool needPostRestore = false;
                this.manager.Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(list.ID, out fieldMapping);
                webPartBaseInfo.View = ReplaceViewFields(webPartBaseInfo.View, fieldMapping, list, ref needPostRestore);
                if (needPostRestore)
                {
                    AddUnRestoreWebPartInfo(manager.Web.ID, webPartBaseInfo.ListId, manager.File.ServerRelativeUrl, webPartBaseInfo);
                    return false;
                }
            }
            else if (webPartBaseInfo.ListId != Guid.Empty)
            {
                if (!this.Manager.Cache.IsSitePostRestore)
                {
                    logger.Log(AveLogLevel.DEBUG, "No fields mapping to replace WebPart view fields");
                    AddUnRestoreWebPartInfo(manager.Web.ID, webPartBaseInfo.ListId, manager.File.ServerRelativeUrl, webPartBaseInfo);
                    return false;
                }
                logger.Log(AveLogLevel.DEBUG, "Try to restore IListWebPart without fields mapping on file:{0}. Version:{1}",
                    this.Manager.File.ServerRelativeUrl, webPartBaseInfo.PageVersion > 0 ? webPartBaseInfo.PageVersion : this.Manager.File.UIVersion);
            }
            return true;
        }
        private void CacheCalendarSettingsInfo()
        {

            if (webPartBaseInfo.View == null || webPartBaseInfo.ListId == Guid.Empty)
            {
                return;
            }

            try
            {
                SPList list = (this.Manager.Web.GetList(webPartBaseInfo.ListId) as AveList).List;
                string viewString = AveCompressedUtility.GetTCompressedString(webPartBaseInfo.View);
                XmlDocument xDoc = new XmlDocument();
                if (!string.IsNullOrEmpty(viewString))
                {
                    viewString = "<root>" + viewString + "</root>";
                    xDoc.LoadXml(viewString);
                    if (xDoc.GetElementsByTagName("CalendarSettings").Count > 0)
                    {
                        Guid webId = manager.Web.ID;
                        Guid listId = list.ID;
                        Guid viewId = this.webPartId;
                        manager.AddToNeedResetCalendarSettingsViews(webId, listId, viewId);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while cache webpart calendar settings, error: {0}", e);
            }
        }


        public virtual bool RealRestore()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebPart.RealRestore"))
            {
                bool result = false;
                try
                {
                    if (!VerifyWebPartTypeId())
                    {
                        return false;
                    }

                    EnsureAssemblyInfo();

                    if (!AveEnv.IsMoss
                        && WrapperConfiguration.SkipInWssWebPartLists.Exists(type => string.Equals(type, webPartType.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        logger.Log(AveLogLevel.WARN, "Skip to restore this Web Part in SharePoint Foundation. Web Part Type: {0}.", webPartType);
                        mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Skipped, AveReportResource.Wrapper_Report_CannotGetViewByID, AveReportResource.Wrapper_Report_CannotFindWebPartAssembly, webPartBaseInfo.WebPartTypeId));
                        return false;
                    }
                    if (!VerifyWebPartData())
                    {
                        AddUnRestoreWebPartInfo(manager.Web.ID, webPartBaseInfo.ListId, manager.File.ServerRelativeUrl, webPartBaseInfo);
                        return false;
                    }
                    if (!VerifyBaseViewID())
                    {
                        return false;
                    }
                    if (isIListWebPart && !EnsureViewInfo())
                    {
                        return false;
                    }

                    bool needReload = false;
                    if (this.internalWebPart == null)
                    {
                        if (isIListWebPart && !this.Manager.HasFullControlPermission)
                        {
                            InternalAddIListWebPart();
                            if (this.internalWebPart == null)
                            {
                                logger.Warn("Can't get the web part object in internal add WebPart. Web part type: {0}", webPartType);
                                mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotGetWebPartObject));
                                return false;
                            }
                            return result |= needReload;
                        }

                        this.internalWebPart = manager.CreateWebPartInstance(assemblyName, webPartType);
                        //Only IListWebPart, ClientWebPart and ProjectSummaryWebPart need update before add.
                        if (isIListWebPart || isClientWebPart || isProjectSummaryWebPart)
                        {
                            needReload = false;
                            UpdateWebPartByType(true, out needReload);
                            result |= needReload;
                        }
                        manager.AddWebPart(this.internalWebPart, webPartBaseInfo.ZoneID, int.MaxValue - 370, isShared, destUserId);
                        //mapping中目的端的的id存储GetStorageKey得到的tp_ID,以保持与GetWebPart中取法保持一致
                        if (this.internalWebPart != null)
                        {
                            webPartId = manager.GetStorageKey(this.internalWebPart, isShared, destUserId);
                            manager.Cache.SiteMappingManager.AddWebPartMapping(manager.File.ServerRelativeUrl, webPartBaseInfo.ID, webPartId);
                        }
                        else
                        {
                            logger.Warn("Can't get the web part object in RealRestore. Web part type: {0}", webPartType);
                            mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotGetWebPartObject));
                            return result;
                        }
                    }

                    needReload = RealRestoreCore();
                    result |= needReload;

                    if (webPartBaseInfo.WebPartTypeId.Equals(new Guid("8bc619d2-cd95-2e79-eae8-95302188e7fb")) || webPartBaseInfo.WebPartTypeId.Equals(new Guid("53f08f81-f1b3-460b-448a-645677de15df")))
                    {
                        //对KPIListWebPart需要重新load一次，否则还原到目的端后，要刷新页面才能正常显示
                        this.internalWebPart = manager.ReloadWebPart(this.webPartId, isShared, destUserId);
                    }

                    //ADO-112421：原端BaseViewID为Null的ListFormWebPart,在AddWebPart会被还成0。添加判断更新BaseViewID。
                    if (isIListWebPart)
                    {
                        //DisplayName表示的是DataFormWebPart显示的当前的View的Title，用API更新不上
                        UpdateView();
                    }
                    if (this.Manager.HasFullControlPermission)
                    {
                        if (destUserId > 0)
                        {
                            UpdateUserID(webPartId, destUserId, false);
                        }
                        //Post Action中还原历史Version上的WebPart需要调用这个方法
                        if ((webPartBaseInfo.PageVersion != 0 && webPartBaseInfo.PageVersion < manager.File.UIVersion)
                            || (!Manager.ClearExsitingWebPart && webPartBaseInfo.PageVersion == 0 && webPartBaseInfo.Level != (byte)Manager.File.Level))//当page有checkout Version时,仅通过PageVersion判断不够,需要加上Level联合判断。
                        {
                            Manager.UpdateWebPartInfo(this.webPartId, Manager.Web.Site.ID, Manager.File.UniqueId, webPartBaseInfo.PageVersion, (byte)Manager.File.Level, webPartBaseInfo.Level, webPartBaseInfo.IsCurrentVersion, Manager.File.UIVersion);
                        }
                    }
                    CacheCalendarSettingsInfo();

                    #region Remove
                    //if (newAdd && webPartId != Guid.Empty && this.Manager.Web.Site.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                    //{
                    //    manager.UpdateWebPartInfo(webPartGuid, webPartId);
                    //}
                    //cache中存在的webpartId是API初始化时的，如果不进行更新，可能会对后来的程序产生隐藏的问题
                    //mManager.AddWebPartMapping(mManager.File.ServerRelativeUrl, mWebPartInfo.ID, AveLimitedWebPartManager.GetWebPartID(mWebPart.ID));
                    #endregion
                }
                catch (Exception ex)
                {
                    mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreWebPartError, assemblyName + "|" + webPartType, ex.Message));
                    logger.Warn("An error occurred while restoring web part {0}, error: {1}", assemblyName + "|" + webPartType, ex);
                }
                finally
                {
                    //manager.Web.SetSPContextNull();
                }
                return result;
            }
        }

        protected bool RealRestoreCore()
        {
            bool result = false;
            bool reload = false;
            //RestorePersonalization方法中改变了manager，需要重新reload
            if (RestorePersonalization())
            {
                this.internalWebPart = manager.ReloadWebPart(this.webPartId, isShared, destUserId);
            }

            RestoreWebPartProperties();
            if (!internalAdd)
            {
                manager.MoveWebPart(this.internalWebPart, webPartBaseInfo.ZoneID, webPartBaseInfo.PartOrder, isShared, destUserId);
                if (!string.IsNullOrEmpty(webPartBaseInfo.WebPartIdProperty))
                {
                    this.internalWebPart.ID = webPartBaseInfo.WebPartIdProperty;
                }
                else
                {
                    //ADO-59000,对personal view的webpart修改ID，可能导致webpart上的属性更新成不正确的属性值。
                    if (isShared)
                    {
                        string id = "g_" + webPartBaseInfo.ID.ToString();
                        id = id.Replace("-", "_");
                        this.internalWebPart.ID = id;
                    }
                }
            }
            reload = false;
            UpdateWebPartByType(false, out reload);
            result |= reload;

            manager.SaveChanges(this.internalWebPart, isShared, destUserId);

            //如果源端的Properties都为null，但是目的端的不一定为null，清空一下目的端的，不然多余的数据可能导致显示不一致。
            if (!internalAdd && this.Manager.HasFullControlPermission)
            {
                if (webPartBaseInfo.AllUsersProperties == null && webPartBaseInfo.PerUserProperties == null
                //Office 365 Properties 存在 DicAllUserPerUserPros 中
                && (webPartBaseInfo.DicAllUserPerUserPros == null || webPartBaseInfo.DicAllUserPerUserPros.Count == 0))
                {
                    UpdatePropertiesByNative(this.webPartId);
                }
            }
            return result;
        }

        //Add WebPartMapping
        protected bool InternalAddIListWebPart()
        {
            string dirName = string.Empty;
            string leafName = string.Empty;
            AveUrlUtility.SplitUrl(this.Manager.File.ServerRelativeUrl, out dirName, out leafName);
            this.webPartId = Guid.NewGuid();
            if (!this.manager.Cache.SiteMappingManager.TryGetWebPartMappingId(this.Manager.File.ServerRelativeUrl, webPartBaseInfo.ID, out this.webPartId))
            {
                this.webPartId = Guid.NewGuid();
            }
            if (string.IsNullOrEmpty(webPartBaseInfo.WebPartIdProperty))
            {
                webPartBaseInfo.WebPartIdProperty = "g_" + webPartBaseInfo.ID.ToString().Replace("-", "_");
            }
            webPartBaseInfo.Level = (byte)this.Manager.File.Level;
            if (!isShared)
            {
                webPartBaseInfo.UserID = destUserId;
            }
            //为了还原View相关WebPart，AddWebPart之前可以更改ListId等属性
            if (isMossWebPart)
            {
                webPartBaseInfo.AllUsersProperties = null;
                webPartBaseInfo.PerUserProperties = null;
            }
            this.Manager.InternalAddWebPart(webPartBaseInfo, this.Manager.Web.Site.ID, dirName, leafName, this.webPartId);
            this.internalWebPart = this.Manager.ReloadWebPart(this.webPartId, isShared, destUserId);
            if (this.internalWebPart == null)
            {
                this.webPartId = Guid.Empty;
                return false;
            }
            this.internalAdd = true;
            manager.Cache.SiteMappingManager.AddWebPartMapping(manager.File.ServerRelativeUrl, webPartBaseInfo.ID, this.webPartId);

            bool needReload = RealRestoreCore();
            return needReload;
        }

        protected virtual void ReplaceViewFieldsString(XmlDocument xDoc, IAveFieldMapping fieldMapping, SPList list, ref bool change, ref bool needPostRestore)
        {
            if (!ReplaceViewFields(xDoc, fieldMapping, list, ref change))
            {
                needPostRestore = true;
                return;
            }
            if (!ReplaceOrderFields(xDoc, fieldMapping, list, ref change))
            {
                needPostRestore = true;
                return;
            }
            if (!ReplaceFilterFields(xDoc, fieldMapping, list, ref change))
            {
                needPostRestore = true;
                return;
            }
        }

        private bool ReplaceFilterFields(XmlDocument xDoc, IAveFieldMapping fieldMapping, SPList list, ref bool change)
        {
            try
            {
                XmlElement node = xDoc.SelectSingleNode("//Query/Where") as XmlElement;
                if (node != null)
                {
                    XmlNodeList nodes = node.GetElementsByTagName("FieldRef");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (nodes[i].Attributes["Name"] != null)
                        {
                            string fieldName = nodes[i].Attributes["Name"].Value;
                            string mappingName = fieldMapping != null ? fieldMapping.GetMappingRestoredFieldInternalName(fieldName) : string.Empty;
                            if (!string.IsNullOrEmpty(mappingName))
                            {
                                nodes[i].Attributes["Name"].Value = mappingName;
                                change = true;
                            }
                            else if (list != null)
                            {
                                try
                                {
                                    list.Fields.GetFieldByInternalName(fieldName);
                                }
                                catch (Exception ex)
                                {
                                    if (!this.manager.Cache.IsSitePostRestore)
                                    {
                                        //ADO-179494:当WF Status Column作View的Filter条件时，此时WF Status Column还没转移，需放到post action
                                        logger.Log(AveLogLevel.DEBUG, "Can not get field by internal name while replace view fields string, it will be replaced in site post action. view file url: {0}, field internal name:{1}.", this.Manager.File.ServerRelativeUrl, fieldName);
                                        return false;
                                    }
                                    logger.Log(AveLogLevel.WARN, "Remove the view field node due to missing column. Error:{0}", ex);

                                    #region 此处逻辑是将view filter中不存在的field从Where语句中移除，并使剩下的语句成立，否则view页会显示出错
                                    XmlNode nodeA = nodes[i].ParentNode.ParentNode;
                                    if (nodeA.Name.Equals("Where", StringComparison.OrdinalIgnoreCase))
                                    {
                                        nodeA.RemoveChild(nodes[i].ParentNode);
                                    }
                                    else
                                    {
                                        XmlNode nodeB = nodes[i].ParentNode.ParentNode.ParentNode;
                                        nodeA.RemoveChild(nodes[i].ParentNode);
                                        var childs = nodeA.ChildNodes;
                                        for (int j = 0; j < childs.Count; j++)
                                        {
                                            nodeB.AppendChild(childs[j]);
                                        }
                                        nodeB.RemoveChild(nodeA);
                                        if (i < nodes.Count)
                                        {
                                            ReplaceFilterFields(xDoc, fieldMapping, list, ref change);
                                        }
                                        break;
                                    }
                                    change = true;
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while Replace view filter fileds for the web part. error: {1}", e);
            }
            return true;
        }

        private bool ReplaceOrderFields(XmlDocument xDoc, IAveFieldMapping fieldMapping, SPList list, ref bool change)
        {
            try
            {
                XmlElement node = xDoc.SelectSingleNode("//Query/OrderBy") as XmlElement;
                if (node != null)
                {
                    XmlNodeList nodes = node.GetElementsByTagName("FieldRef");
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (nodes[i].Attributes["Name"] != null)
                        {
                            string fieldName = nodes[i].Attributes["Name"].Value;
                            string mappingName = fieldMapping != null ? fieldMapping.GetMappingRestoredFieldInternalName(fieldName) : string.Empty;
                            if (!string.IsNullOrEmpty(mappingName))
                            {
                                nodes[i].Attributes["Name"].Value = mappingName;
                                change = true;
                            }
                            else if (list != null)
                            {
                                try
                                {
                                    list.Fields.GetFieldByInternalName(fieldName);
                                }
                                catch (Exception ex)
                                {
                                    if (!this.manager.Cache.IsSitePostRestore)
                                    {
                                        //ADO-179494:当WF Status Column作View的Filter条件时，此时WF Status Column还没转移，需放到post action
                                        logger.Log(AveLogLevel.DEBUG, "Can not get field by internal name while replace view fields string, it will be replaced in site post action. view file url: {0}, field internal name:{1}.", this.Manager.File.ServerRelativeUrl, fieldName);
                                        return false;
                                    }
                                    logger.Log(AveLogLevel.WARN, "Remove the view field node due to missing column. Error:{0}", ex);
                                    nodes[i].ParentNode.RemoveChild(nodes[i]);
                                    change = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while Replace view order fileds for the web part. Error: {1}", e);
            }
            return true;
        }

        private bool ReplaceViewFields(XmlDocument xDoc, IAveFieldMapping fieldMapping, SPList list, ref bool change)
        {
            try
            {
                XmlElement node = xDoc.SelectSingleNode("//ViewFields") as XmlElement;
                if (node != null)
                {
                    XmlNodeList nodes = node.GetElementsByTagName("FieldRef");
                    if (nodes.Count > 0)
                    {
                        List<XmlElement> addNodes = new List<XmlElement>();
                        bool isFlatViewInDiscussionBoard = IsFlatViewInDiscussionBoard(list, nodes);
                        bool isBodyChecked = false;
                        bool isTrimmedBodyChecked = false;
                        bool isPreviewOnFormChecked = false;
                        for (int i = nodes.Count - 1; i >= 0; i--)
                        {
                            if (nodes[i].Attributes["Name"] != null)
                            {
                                string fieldName = nodes[i].Attributes["Name"].Value;
                                string mappingName = fieldMapping != null ? fieldMapping.GetMappingRestoredFieldInternalName(fieldName) : string.Empty;
                                if (!string.IsNullOrEmpty(mappingName))
                                {
                                    nodes[i].Attributes["Name"].Value = mappingName;
                                    change = true;
                                }
                                else if (list != null)
                                {
                                    try
                                    {
                                        list.Fields.GetFieldByInternalName(fieldName);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!this.manager.Cache.IsSitePostRestore)
                                        {
                                            //ADO-179494:当WF Status Column作View的Filter条件时，此时WF Status Column还没转移，需放到post action
                                            logger.Log(AveLogLevel.DEBUG, "Can not get field by internal name while replace view fields string, it will be replaced in site post action. view file url: {0}, field internal name:{1}.", this.Manager.File.ServerRelativeUrl, fieldName);
                                            return false;
                                        }
                                        logger.Log(AveLogLevel.WARN, "Remove the view field node due to missing column. Error:{0}", ex);
                                        nodes[i].ParentNode.RemoveChild(nodes[i]);
                                        change = true;
                                    }
                                }
                                if (fieldName.Equals("PreviewOnForm", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (nodes[i].Attributes["Explicit"] != null &&
                                        string.Equals(nodes[i].Attributes["Explicit"].Value, "TRUE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isPreviewOnFormChecked = true;
                                    }
                                    else
                                    {
                                        logger.Debug("Remove PreviewOnForm node");
                                        nodes[i].ParentNode.RemoveChild(nodes[i]);
                                        change = true;
                                    }
                                }
                                if (isFlatViewInDiscussionBoard)
                                {
                                    if (fieldName.Equals("Body", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isBodyChecked = true;
                                    }
                                    if (fieldName.Equals("TrimmedBody", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isTrimmedBodyChecked = true;
                                    }
                                }
                            }
                        }
                        //Flat view必须check body 与TrimmedBody两个column。ADO-113652.
                        if (isFlatViewInDiscussionBoard)
                        {
                            if (!isTrimmedBodyChecked)
                            {
                                XmlElement trimmedBodyElement = xDoc.CreateElement("FieldRef");
                                trimmedBodyElement.SetAttribute("Name", "TrimmedBody");
                                addNodes.Add(trimmedBodyElement);
                            }
                            if (!isBodyChecked)
                            {
                                XmlElement bodyElement = xDoc.CreateElement("FieldRef");
                                bodyElement.SetAttribute("Name", "Body");
                                addNodes.Add(bodyElement);
                            }
                        }
                        //SharePoint 2013 PictureLibrary view中必须选中PreviewOnForm Column，否则某些column无法正确显示。[ADO-155438]
                        //SharePoint 2007 & 2010不需要此Column
                        if (list != null && (list.BaseTemplate == SPListTemplateType.PictureLibrary || list.BaseTemplate == (SPListTemplateType)AveListTemplateType.ImagesLibrary) && !isPreviewOnFormChecked)
                        {
                            XmlElement element = xDoc.CreateElement("FieldRef");
                            element.SetAttribute("Name", "PreviewOnForm");
                            //element.SetAttribute("Explicit", "TRUE");
                            addNodes.Add(element);
                        }
                        if (addNodes.Count > 0)
                        {
                            AddFieldRefNodes(xDoc, addNodes, ref change);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while do replace view fields for the web part. Error: {0}", e);
            }
            return true;
        }

        protected byte[] ReplaceViewFields(byte[] view, IAveFieldMapping fieldMapping, SPList list, ref bool needPostRestore)
        {
            if (view == null)
            {
                return null;
            }
            string viewString = AveCompressedUtility.GetTCompressedString(view);
            XmlDocument xDoc = new XmlDocument();
            if (!string.IsNullOrEmpty(viewString))
            {
                bool change = false;
                try
                {
                    viewString = "<root>" + viewString + "</root>";
                    xDoc.LoadXml(viewString);
                    ReplaceViewFieldsString(xDoc, fieldMapping, list, ref change, ref needPostRestore);
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "Failed to load view string. Error:{0}", ex);
                }
                if (!needPostRestore && change)
                {
                    return AveCompressedUtility.GetTCompressedBytes(xDoc.FirstChild.InnerXml);
                }
            }
            return view;
        }

        //flat view :has StatusBar attribute and Explicit is true.
        private bool IsFlatViewInDiscussionBoard(SPList list, XmlNodeList fieldRefNodes)
        {
            bool isFlatView = false;
            if (list != null && list.BaseTemplate == SPListTemplateType.DiscussionBoard)
            {
                foreach (XmlNode node in fieldRefNodes)
                {
                    string fieldRefName = node.Attributes["Name"] == null ? string.Empty : node.Attributes["Name"].Value;
                    string ExplicitValue = node.Attributes["Explicit"] == null ? string.Empty : node.Attributes["Explicit"].Value;
                    if (fieldRefName.Equals("StatusBar", StringComparison.OrdinalIgnoreCase) && ExplicitValue.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                    {
                        isFlatView = true;
                        break;
                    }
                }
            }
            else
            {
                isFlatView = false;
            }
            return isFlatView;
        }

        private void AddFieldRefNodes(XmlDocument xDoc, List<XmlElement> nodes, ref bool changed)
        {
            XmlNode node = xDoc.SelectSingleNode("root");
            XmlNodeList nodelist = node.SelectNodes("ViewFields");
            foreach (var childnode in nodelist)
            {
                XmlElement rootElement = (XmlElement)childnode;
                if (rootElement == null) continue;

                foreach (XmlElement ele in nodes)
                {
                    rootElement.AppendChild(ele);
                    changed = true;
                }
            }
        }

        private void ReplaceAudienceId()
        {
            try
            {
                string audienceIds = WebPart.AuthorizationFilter;
                if (string.IsNullOrEmpty(audienceIds))
                {
                    return;
                }
                logger.Log(AveLogLevel.DEBUG, "Web Part audience Id: {0}", audienceIds);
                WebPart.AuthorizationFilter = manager.ReplaceAudienceId(audienceIds);
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, "Failed to replace web part audience id. Error: {0}", ex.ToString());
            }
        }

        private bool IsSandBoxSolutionWebPart()
        {
            bool flag = true;
            if (this.webPartBaseInfo.SolutionId != Guid.Empty)
            {
                // add for app part
                if (this.webPartBaseInfo.Class.Equals("Microsoft.SharePoint.WebPartPages.ClientWebPart", StringComparison.OrdinalIgnoreCase))
                {
                    flag = false;
                }
            }
            else
            {
                flag = false;
            }
            if (flag)
            {
                if (!SPUserCodeService.Local.IsEnabled)
                {
                    throw new Exception("UserCode service is not running");
                }
            }
            return flag;
        }

        protected bool VerifyWebPartTypeId()
        {
            //replace webpart type id first
            Guid tempWebPartId;
            //ADO-167783 siteurl包含特殊字符，AdvancedSearchLayout和PeopleSearchResults页上为ErrorWebPart,由于只有SP16支持特殊字符，只处理SP16
            if (WebPartInfo.WebPartTypeId == new Guid("3439D141-116C-A462-DB7A-ABE1CCA8A5DC"))
            {
                logger.Warn("This is ErrorWebPart, we will not restore it.");
                mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotFindWebPartAssembly, webPartBaseInfo.WebPartTypeId));
                return false;
            }
            if (this.Manager.Cache.SiteMappingManager.TryGetNeedWebPartIDMappingId(WebPartInfo.WebPartTypeId, out tempWebPartId))
            {
                webPartBaseInfo.WebPartTypeId = tempWebPartId;
            }

            if ((webPartBaseInfo.Assembly == null || webPartBaseInfo.Class == null) && !this.Manager.Cache.SiteMappingManager.ContainsKeyForWebPartTypeIDMapping(webPartBaseInfo.WebPartTypeId))
            {
                logger.Warn("Can't find the web part assembly info: {0}", webPartBaseInfo.WebPartTypeId);
                mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotFindWebPartAssembly, webPartBaseInfo.WebPartTypeId));
                return false;
            }

            return true;
        }

        protected bool VerifyBaseViewID(SPList list = null)
        {
            if (!webPartBaseInfo.BaseViewID.HasValue)
            {
                return true;
            }
            try
            {
                //ListId 在之前已经被替换过
                if (list == null)
                {
                    if (webPartBaseInfo.ListId == Guid.Empty) return true;

                    list = (this.Manager.Web as AveWeb).Web.Lists[webPartBaseInfo.ListId];
                }
                string strValidBaseViewID = this.Manager.GetValidBaseViewIdStr(list);
                if (!strValidBaseViewID.Contains("|" + webPartBaseInfo.BaseViewID.Value.ToString() + "|"))
                {
                    //ADO-86387,使用list mapping会有大量的failed report，对于源端和目的端BaseViewID不相等的case，暂时不加到report中。
                    logger.Warn("Source base View ID is not equal with view base View ID. List title: {0}, source view ID: {1}, valid view ID: {2}", webPartBaseInfo.ListTitle, webPartBaseInfo.BaseViewID, strValidBaseViewID);
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to verify base view ID. Error: {0}", e);
            }
            return true;
        }

        protected void EnsureAssemblyInfo()
        {
            string webPartTypeIdMappingValue;
            if (IsSandBoxSolutionWebPart())
            {//Sandboxed Solutions Webpart.
                assemblyName = "Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";
                webPartType = "Microsoft.SharePoint.WebPartPages.SPUserCodeWebPart";
            }
            else if (Manager.Cache.SiteMappingManager.TryGetValueFromWebPartTypeIDMapping(webPartBaseInfo.WebPartTypeId, out webPartTypeIdMappingValue))
            {
                string[] assembly = webPartTypeIdMappingValue.Split('|');
                assemblyName = assembly[0];
                webPartType = assembly[1];
            }
            else if (!string.IsNullOrEmpty(webPartBaseInfo.Assembly) && !string.IsNullOrEmpty(webPartBaseInfo.Class))
            {
                assemblyName = webPartBaseInfo.Assembly;
                webPartType = webPartBaseInfo.Class;
            }

            if (string.Equals(webPartType, "Microsoft.SharePoint.WebPartPages.ClientWebPart", StringComparison.Ordinal))
            {
                isClientWebPart = true;
            }
            if (string.Equals(webPartType, "Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart", StringComparison.OrdinalIgnoreCase))
            {
                isProjectSummaryWebPart = true;
            }
            //if (this.isViewWebPart)
            //{
            //    //365-local,Replace version.
            //    if (assemblyName.Contains("Version=16.0.0.0"))
            //    {
            //        assemblyName = assemblyName.Replace("Version=16.0.0.0", "Version=15.0.0.0");
            //    }
            //}

            Type type = AveAssemblyUtility.GetType(assemblyName, webPartType);
            if (type.GetInterface("IListWebPart") != null)
            {
                isIListWebPart = true;
            }
        }

        private bool RestorePersonalization()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebPart.RestorePersonalization"))
            {
                if (webPartBaseInfo.Personalization == null || webPartBaseInfo.Personalization.Count == 0)
                {
                    return false;
                }
                if (this.Manager.HasFullControlPermission)
                {
                    AvePersonalizationInfo currentUserPersonalizationInfo = null;
                    System.Web.UI.WebControls.WebParts.WebPart personalWebPart = manager.ReloadWebPart(this.webPartId, false);
                    foreach (AvePersonalizationInfo personInfo in webPartBaseInfo.Personalization)
                    {
                        try
                        {
                            int userId = manager.FindMemberId(personInfo.UserID);
                            if (userId == manager.Web.CurrentUser.ID)
                            {
                                currentUserPersonalizationInfo = personInfo;
                                continue;
                            }
                            RealRestorePersonalization(personalWebPart, personInfo, userId);
                            personalWebPart = manager.ReloadWebPart(WebPart.ID, false);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Get the web part info with url: {0}, id: {1} failed: {2}.", webPartBaseInfo.TitleUrl, webPartBaseInfo.ID, e.ToString());
                        }
                    }
                    if (currentUserPersonalizationInfo != null)
                    {
                        RealRestorePersonalization(personalWebPart, currentUserPersonalizationInfo, manager.Web.CurrentUser.ID);
                    }
                }
                else
                {
                    foreach (AvePersonalizationInfo personInfo in webPartBaseInfo.Personalization)
                    {
                        int userId = manager.FindMemberId(personInfo.UserID);
                        if (userId <= 0)
                        {
                            logger.Log(AveLogLevel.WARN, "Can not find user that personalized this web part. Source User Id:{0}. WebPart ID:{1}. File Url:{2}", personInfo.UserID, this.webPartId, this.Manager.File.ServerRelativeUrl);
                            continue;
                        }
                        System.Web.UI.WebControls.WebParts.WebPart personalWebPart = manager.GetWebPart(this.webPartId, false, userId);
                        RealRestorePersonalization(personalWebPart, personInfo, userId);
                    }
                }
                return true;
            }
        }

        protected virtual void UpdateView()
        {
            //如果不是View的话，不需要更新WebPart的ContentTypeId属性
            int baseViewID = webPartBaseInfo.BaseViewID.HasValue ? Convert.ToInt32(webPartBaseInfo.BaseViewID.Value) : -1;
            //Manager.UpdateView(this.webPartId, Manager.Web.Site.ID, Manager.File.UniqueId, baseViewID, webPartBaseInfo.View, null, webPartBaseInfo.DisplayName);
            //ADO-168906 此Case 中webpart 的contenttypeid 也需要替换，才能link 到discussion 的subject 上
            bool needUpdateContentType = false;
            if (webPartBaseInfo.ListId != Guid.Empty)
            {
                //List 在之前已经验证过
                var list = (this.Manager.Web as AveWeb).Web.Lists[webPartBaseInfo.ListId];
                needUpdateContentType = CheckViewContentType(webPartBaseInfo, list);
            }

            if (needUpdateContentType)
            {
                Manager.UpdateView(this.webPartId, Manager.Web.Site.ID, Manager.File.UniqueId, baseViewID, webPartBaseInfo.View, webPartBaseInfo.ContentTypeId, webPartBaseInfo.DisplayName);
            }
            else
            {
                Manager.UpdateView(this.webPartId, Manager.Web.Site.ID, Manager.File.UniqueId, baseViewID, webPartBaseInfo.View, null, webPartBaseInfo.DisplayName);
            }
        }

        protected virtual bool CheckViewContentType(AveWebPartBaseInfo webPartInfo, SPList list)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in webPartInfo.ContentTypeId)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                string sourceId = "0x" + sb.ToString();
                if (sourceId.Equals("0x") || sourceId.Equals("0x012001"))//0x: in all folders 0x012001:in top-level folder
                {
                    return true;
                }
                else if (Manager.Cache.ListLevelCTIdMapping.ContainsKey(sourceId))//in other folder
                {
                    string destId = Manager.Cache.ListLevelCTIdMapping[sourceId].ToString().TrimStart('0').TrimStart('x');
                    if ((destId.Length % 2) != 0)
                        destId += " ";
                    byte[] returnBytes = new byte[destId.Length / 2];
                    for (int i = 0; i < returnBytes.Length; i++)
                    {
                        returnBytes[i] = Convert.ToByte(destId.Substring(i * 2, 2), 16);
                    }
                    webPartInfo.ContentTypeId = returnBytes;
                    return true;
                }
                SPContentTypeId ctId = new SPContentTypeId(sourceId);
                if (SPBuiltInContentTypeId.Contains(ctId))
                {
                    return true;
                }
                foreach (SPContentType tmp in list.ContentTypes)
                {
                    if (tmp.Id == ctId)
                    {
                        return true;
                    }
                    if (tmp.Parent.Id == ctId.Parent)
                    {
                        string hexString = tmp.Id.ToString();
                        byte[] buffer = new byte[(hexString.Length - 2) / 2];
                        for (int i = 2; i < hexString.Length; i += 2)
                        {
                            buffer[(i / 2) - 1] = Convert.ToByte(hexString.Substring(i, 2), 0x10);
                        }
                        webPartInfo.ContentTypeId = buffer;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info("Cannot get the list. List ID: {0}, exception: {1}", webPartInfo.ListId, ex);
            }
            return false;
        }

        protected void UpdatePropertiesByNative(Guid webPartId)
        {
            manager.UpdatePropertiesByNative(webPartId, manager.Web.Site.ID, manager.File.UniqueId, webPartBaseInfo.AllUsersProperties, webPartBaseInfo.PerUserProperties);
        }

        protected void RealRestorePersonalization(System.Web.UI.WebControls.WebParts.WebPart webPart, AvePersonalizationInfo personalInfo, int userId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebPart.RealRestorePersonalization"))
            {
                int result = 0;
                bool hasChanged = false;
                Dictionary<string, object> properties = AveWebPartUtility.GetProperties(null, personalInfo.PerUserProperties, out result);

                if (result == 0 && properties.Count > 0)
                {
                    this.internalWebPart = webPart;
                    RestoreWebPartProperties(properties);
                    hasChanged = true;
                }

                if (!personalInfo.IsIncluded)
                {
                    this.Manager.CloseWebPart(webPart, false);
                    hasChanged = true;
                }

                if (personalInfo.PartOrder != null)
                {
                    if (webPart.ZoneIndex != personalInfo.PartOrder.Value || !string.Equals(webPartBaseInfo.ZoneID, personalInfo.ZoneID, StringComparison.OrdinalIgnoreCase))
                    {
                        this.Manager.MoveWebPart(webPart, personalInfo.ZoneID, personalInfo.PartOrder.Value, false);
                        hasChanged = true;
                    }
                }

                if (personalInfo.FrameState != (byte)webPart.ChromeState)
                {
                    webPart.ChromeState = (System.Web.UI.WebControls.WebParts.PartChromeState)Enum.Parse(typeof(System.Web.UI.WebControls.WebParts.PartChromeState), personalInfo.FrameState.ToString());
                    hasChanged = true;
                }

                if (hasChanged)
                {
                    this.Manager.SaveChanges(webPart, false);
                }

                if (result != 0 && personalInfo.PerUserProperties != null)
                {
                    if (!this.Manager.HasFullControlPermission)
                    {
                        logger.Log(AveLogLevel.WARN, "Skip to restore personalized WebPart properties because of permission issue. File Url:{0}. WebPart Info:{1}", this.Manager.File.ServerRelativeUrl, assemblyName + "|" + webPartType);
                    }
                    else
                    {
                        UpdatePersonalPropertiesByNative(this.manager.GetStorageKey(webPart, true), personalInfo.PerUserProperties);
                    }
                }

                if (this.Manager.HasFullControlPermission && userId != manager.Web.CurrentUser.ID)
                {
                    UpdateUserID(this.webPartId, userId, true);
                }
            }
        }

        protected void UpdateUserID(Guid webPartId, int userId, bool isPersonal)
        {
            manager.UpdateUserID(webPartId, manager.Web.Site.ID, manager.File.UniqueId, manager.Web.CurrentUser.ID, userId, isPersonal);
            manager.Dispose();
        }

        protected void UpdatePersonalPropertiesByNative(Guid webPartId, byte[] perUserBytes)
        {
            manager.UpdatePersonalPropertiesByNative(webPartId, manager.Web.Site.ID, manager.Web.CurrentUser.ID, perUserBytes);
        }

        private void RestoreCommonProperties()
        {
            if (!webPartBaseInfo.IsIncluded)
            {
                manager.CloseWebPart(this.internalWebPart, isShared, destUserId);
            }
            //ChromeState属性控制webpart是否为最小化，需要设置。
            this.internalWebPart.ChromeState = (System.Web.UI.WebControls.WebParts.PartChromeState)Enum.Parse(typeof(System.Web.UI.WebControls.WebParts.PartChromeState), webPartBaseInfo.FrameState.ToString());
            ReplaceAudienceId();
        }

        protected void EnsureWebPartProperties()
        {
            //说明已经初始化过WebPartProperties
            if (webPartProperties != null)
            {
                return;
            }
            isMossWebPart = LoadWebPartProperties();
        }

        private Dictionary<string, object> GetWebPartProperties(out int resultCode)
        {
            resultCode = 0;
            if (webPartBaseInfo != null)
            {

                if (webPartBaseInfo.AllUsersProperties != null || webPartBaseInfo.PerUserProperties != null)
                {
                    return AveWebPartUtility.GetProperties(webPartBaseInfo.AllUsersProperties, webPartBaseInfo.PerUserProperties, out resultCode);
                }
                //Office 365 Properties 存在 DicAllUserPerUserPros 中
                if (webPartBaseInfo.DicAllUserPerUserPros != null)
                {
                    return webPartBaseInfo.DicAllUserPerUserPros;
                }
            }
            return new Dictionary<string, object>();
        }


        private bool LoadWebPartProperties()
        {
            int resultCode = 0;
            try
            {
                webPartProperties = GetWebPartProperties(out resultCode);
                if (webPartProperties.Count > 0)
                {
                    webPartProperties = ChangePageType(webPartProperties);
                    AddParameterBindings(webPartProperties);
                    //放在VerifyPictureLibrarySlideshowWebPart方法中，找不到List，则放在PostAction中处理，所以ViewGuid不存在的情况就不存在了
                    //当ViewGuid对应的view不存在时，webPart不能add成功。该属性暂时需要跳过
                    if (!string.Equals("Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart", this.webPartType, StringComparison.OrdinalIgnoreCase))
                    {
                        if (webPartProperties.ContainsKey("ViewGuid"))
                        {
                            webPartProperties.Remove("ViewGuid"); //重构产生错误，10 会在afterupdater 处理，但是13 和 16 把这个webpart 特殊的afterupdater 的逻辑去掉了。
                        }
                    }
                    //赋该值会导致目的端webparts表中的tp_FrameState 值发生变化而导致ContentEditor Webpart不显示内容
                    webPartProperties.Remove("ChromeState");
                }
            }
            catch (Exception ex)
            {
                logger.Info("Can't analyze web part info, is not moss Web Part: {0}", ex);
            }
            return resultCode == 0;
        }

        private Dictionary<string, object> ChangePageType(Dictionary<string, object> tmpDic)
        {
            if (!string.IsNullOrEmpty(this.webPartBaseInfo.DefinitionXml))
            {
                XmlDocument dc = new XmlDocument();
                dc.LoadXml(this.webPartBaseInfo.DefinitionXml);
                try
                {
                    XmlNamespaceManager nsMgr = new XmlNamespaceManager(dc.NameTable);
                    var temp = dc.NamespaceURI;
                    nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/WebPart/v3");
                    XmlNode xmlNode1 = dc.SelectSingleNode("/webParts/ns:webPart/ns:data/ns:properties", nsMgr);
                    if (xmlNode1 != null)
                    {
                        XmlNode xn = xmlNode1.SelectSingleNode("//ns:property[@name='PageType']", nsMgr);

                        if (xn != null)
                        {
                            string value = xn.InnerText;
                            if (tmpDic.ContainsKey("PageType"))
                                if (tmpDic["PageType"].ToString() != value)
                                {
                                    tmpDic["PageType"] = value;
                                    logger.Warn("Web Part PageType is not same");
                                }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while loading PageType: {0}", e);
                }
            }
            return tmpDic;
        }

        private bool RestoreWebPartProperties()
        {
            EnsureWebPartProperties();

            if (isMossWebPart)
            {
                if (webPartProperties.Count > 0)
                {
                    RestoreWebPartProperties(webPartProperties);
                }
            }
            else
            {
                if (internalAdd)
                {
                    return false;
                }

                if (this.Manager.HasFullControlPermission)
                {
                    if (webPartBaseInfo.AllUsersProperties != null || webPartBaseInfo.PerUserProperties != null)
                    {
                        UpdatePropertiesByNative(this.webPartId);
                    }
                    this.internalWebPart = manager.ReloadWebPart(this.internalWebPart.ID, isShared);
                    if (this.internalWebPart == null && !string.IsNullOrEmpty(webPartBaseInfo.WebPartIdProperty))
                    {
                        this.internalWebPart = manager.ReloadWebPart(webPartBaseInfo.WebPartIdProperty, isShared);
                    }
                    if (this.internalWebPart == null)
                    {
                        logger.Warn("Reload WebPart failed.");
                        return false;
                    }
                }
                else
                {
                    logger.Log(AveLogLevel.WARN, "Skip to restore WebPart properties because of permission issue. File Url: {0}", this.Manager.File.ServerRelativeUrl);
                }

            }
            RestoreCommonProperties();

            return isMossWebPart;
        }

        private bool RestoreWebPartProperties(Dictionary<string, object> properties)
        {
            WebPart mossPart = this.internalWebPart as WebPart;
            bool hasInplaceSearchEnabled = false;
            foreach (KeyValuePair<string, object> pair in properties)
            {
                try
                {
                    if (mossPart != null)
                    {
                        if (pair.Key.Equals("Title", StringComparison.OrdinalIgnoreCase))
                        {
                            mossPart.Title = pair.Value.ToString();
                            continue;
                        }
                        if (pair.Key == "Height")
                        {
                            mossPart.Height = pair.Value.ToString();
                            continue;
                        }
                        if (pair.Key == "Width")
                        {
                            mossPart.Width = pair.Value.ToString();
                            continue;
                        }
                        if (pair.Key == "TitleUrl")
                        {
                            mossPart.TitleUrl = AveReplaceProcessor.UrlReplace(pair.Value.ToString(), manager.Cache.SiteManagedMappings, new ReplaceOption(true), manager.Cache.SourceSiteInfo, manager.Cache.DestSiteInfo.ServerRelativeUrl);
                            continue;
                        }
                        //SP07 WebPartConnection
                        if (pair.Key == "AttachedPropertiesShared")
                        {
                            Manager.AddWebPartConnections(pair.Value.ToString());
                            continue;
                        }
                        //SP10 & SP13 WebPartConnection
                        if (pair.Key == "SPConnectionsShared")
                        {
                            Manager.AddWebPartConnections(pair.Value);
                            continue;
                        }
                    }

                    #region Remove to CheckListId()
                    //if (pair.Key == "ListId")
                    //{
                    //    object obj = pair.Value;
                    //    if (webPartBaseInfo.ListId != Guid.Empty)
                    //    {
                    //        obj = webPartBaseInfo.ListId.ToString();
                    //    }
                    //    SetWebPartProperty(pair.Key, obj);
                    //}
                    //else if (pair.Key == "ListName")
                    //{
                    //    object obj = pair.Value;
                    //    if (webPartBaseInfo.ListId != Guid.Empty)
                    //    {
                    //        obj = webPartBaseInfo.ListId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                    //    }
                    //    Guid oldListNameGuid = Guid.Empty;
                    //    Guid newListNameGuid = Guid.Empty;
                    //    if (obj != null && Guid.TryParse(obj.ToString(), out oldListNameGuid))
                    //    {
                    //        if (this.Manager.Cache.ListIdMapping.TryGetValue(oldListNameGuid, out newListNameGuid))
                    //        {
                    //            // 这个地方要把Guid转化为string,因为在WebPart内部ListName这个属性是string类型，如果用Guid在反射赋值的时候会出错。
                    //            obj = newListNameGuid.ToString();
                    //        }
                    //    }
                    //    SetWebPartProperty(pair.Key, obj);
                    //}
                    //else if (pair.Key.Equals("WebId", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    object obj = pair.Value;
                    //    if (pair.Value != null)
                    //    {
                    //        Guid sourceWebId = new Guid(pair.Value.ToString());
                    //        if (this.Manager.Cache.WebIDMapping.ContainsKey(sourceWebId))
                    //        {
                    //            obj = this.Manager.Cache.WebIDMapping[sourceWebId].ToString();
                    //        }
                    //        SetWebPartProperty(pair.Key, obj);
                    //    }
                    //}
                    #endregion

                    if (pair.Key == "MembershipGroupId" && WebPart.GetType().ToString().Equals("Microsoft.SharePoint.WebPartPages.MembersWebPart"))
                    {
                        int originGroupId = Convert.ToInt32(pair.Value);
                        int newGroupId = -1;
                        if (manager.Cache.SiteUserIDMapping.ContainsKey(originGroupId))
                        {
                            object obj = manager.Cache.SiteUserIDMapping[originGroupId];
                            if (obj != null && obj.GetType().Name.Equals("AveSPMemberInfo"))
                            {
                                newGroupId = (int)AveAssemblyUtility.GetFieldValue(obj, "NewId");
                            }
                        }
                        if (newGroupId < 0)
                        {
                            //mLog.Info("Can't find mapping Group Id while restore MembersWebPart property.");
                            SetWebPartProperty(pair.Key, originGroupId);
                        }
                        else
                        {
                            SetWebPartProperty(pair.Key, newGroupId);
                        }
                    }
                    else if (pair.Key == "IsIncludedFilter")
                    {
                        string audience = pair.Value.ToString();
                        audience = manager.ReplaceAudienceId(audience);
                        SetWebPartProperty(pair.Key, audience);
                    }
                    else if (pair.Key.Equals("Contact") && WebPart.GetType().ToString().Equals("Microsoft.SharePoint.Portal.WebControls.ContactFieldControl"))
                    {
                        int userId = Convert.ToInt32(pair.Value);
                        int destUserId = -1;
                        if (manager.Cache.SiteUserIDMapping.ContainsKey(userId))
                        {
                            object user = manager.Cache.SiteUserIDMapping[userId];
                            if (user != null && user.GetType().Name.Equals("AveSPMemberInfo"))
                            {
                                destUserId = (int)AveAssemblyUtility.GetFieldValue(user, "NewId");
                            }
                        }
                        if (destUserId < 0)
                        {
                            SetWebPartProperty(pair.Key, userId);
                        }
                        else
                        {
                            SetWebPartProperty(pair.Key, destUserId);
                        }
                    }
                    else if (pair.Key.Equals("ContactLoginName", StringComparison.OrdinalIgnoreCase) && manager.Cache.SiteUserNameMapping != null)
                    {
                        string loginName = pair.Value.ToString();
                        if (!string.IsNullOrEmpty(loginName))
                        {
                            if (manager.Cache.SiteUserNameMapping.ContainsKey(loginName))
                            {
                                loginName = manager.Cache.SiteUserNameMapping[loginName];
                            }
                            try
                            {
                                manager.Web.EnsureAvailableUser(loginName);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.INFO, "An exception occurred while ensuring user: {0} to current site. Exception: {1}", loginName, ex.ToString());
                                loginName = pair.Value.ToString();
                            }
                        }

                        SetWebPartProperty(pair.Key, loginName);
                    }
                    else if ((pair.Key.Equals("CategoryUrl", StringComparison.OrdinalIgnoreCase) || pair.Key.Equals("PostUrl", StringComparison.OrdinalIgnoreCase))
                            && WebPart.GetType().ToString().Equals("Microsoft.SharePoint.WebPartPages.BlogAdminWebPart")
                        || (pair.Key.Equals("DateUrl", StringComparison.OrdinalIgnoreCase) || pair.Key.Equals("ArchiveUrl", StringComparison.OrdinalIgnoreCase))
                            && WebPart.GetType().ToString().Equals("Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch"))
                    {
                        //ADO-60021,BlogAdminWebPart上的CategoryUrl和PostUrl属性值是相对Web的Url(如"Lists/Posts/Post.aspx"),不需要进行替换。
                        SetWebPartProperty(pair.Key, pair.Value);
                    }
                    else
                    {
                        string propertyName = pair.Key;
                        object value = pair.Value;
                        if (propertyName.Equals("XML"))
                        {
                            propertyName = "Xml";
                        }
                        if (propertyName.Equals("XSLLink"))
                        {
                            propertyName = "XslLink";
                        }
                        //Add "Image" for TitleBarWebPart
                        if (propertyName.EndsWith("Link", StringComparison.OrdinalIgnoreCase)
                            || propertyName.EndsWith("URL", StringComparison.OrdinalIgnoreCase)
                            || propertyName.EndsWith("Uri", StringComparison.OrdinalIgnoreCase)
                            || propertyName.EndsWith("Path", StringComparison.OrdinalIgnoreCase)
                            || propertyName.Equals("FormLocation", StringComparison.OrdinalIgnoreCase)
                            || string.Equals("PartImageLarge", propertyName, StringComparison.OrdinalIgnoreCase)
                            || string.Equals("PartImageSmall", propertyName, StringComparison.OrdinalIgnoreCase)
                            || string.Equals("Image", propertyName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (value.ToString().Contains("/"))
                            {
                                //We need to replace the absolute URL for TitleBarWebPart
                                value = AveReplaceProcessor.UrlReplace(value.ToString(), manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), manager.Cache.SourceSiteInfo, manager.Cache.DestSiteInfo.ServerRelativeUrl);
                            }
                        }
                        if (propertyName.Equals("InplaceSearchEnabled", StringComparison.OrdinalIgnoreCase) && value != null)
                        {
                            hasInplaceSearchEnabled = true;
                        }
                        SetWebPartProperty(propertyName, value);
                    }
                }
                catch (Exception aE)
                {
                    logger.Warn("Error happened when Setting the property of WebPart, property: {0}, WebPart Type: {1}, File Url:{2}. Reason: {3}", pair.Key, WebPart.GetType().ToString(), this.Manager.File.ServerRelativeUrl, aE);
                }
            }
            var xsltListViewWebpart = internalWebPart as XsltListViewWebPart;
            if (!hasInplaceSearchEnabled && xsltListViewWebpart != null)//Disable the search box setting if the backup data is SP10.
            {
                xsltListViewWebpart.InplaceSearchEnabled = false;
                logger.Debug("Disable webpart InplaceSearchEnabled.");
            }
            return false;
        }

        private void AddParameterBindings(Dictionary<string, object> tmpDic)
        {
            if (!tmpDic.ContainsKey("ParameterBindings"))
            {
                if (!string.IsNullOrEmpty(webPartBaseInfo.DefinitionXml))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(webPartBaseInfo.DefinitionXml);
                        XmlNode node = doc.SelectSingleNode(".//*[@name='ParameterBindings']");
                        if (node != null)
                        {
                            tmpDic.Add("ParameterBindings", node.InnerText);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, ServerAPIResource.WebpartDefinitionXmlLoadFailed,
                             webPartBaseInfo.DisplayName, webPartBaseInfo.ZoneID, webPartBaseInfo.ListTitle, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 用于Check WebPart相关DataSource是否在目的端已经存在
        /// 如果需要在PostAction中传入WebPartBaseInfo的WebPart，最好在这里就可以Check完毕
        /// 否则，需要往PostAction中传入对应的WebPartId即可
        /// </summary>
        /// <returns></returns>
        protected bool VerifyWebPartData()
        {
            if (!CheckListId())
            {
                return false;
            }
            if (string.Equals("Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart", this.webPartType, StringComparison.OrdinalIgnoreCase))
            {
                return VerifyPictureLibrarySlideshowWebPart();
            }
            if (string.Equals("Microsoft.Office.InfoPath.Server.Controls.WebUI.BrowserFormWebPart", this.webPartType, StringComparison.OrdinalIgnoreCase))
            {
                return VerifyBrowserFormWebPart();
            }
            if (webPartBaseInfo.UserID > 0)
            {
                isShared = false;
                destUserId = this.Manager.FindMemberId(webPartBaseInfo.UserID);
                if (destUserId <= 0)
                {
                    string msg = string.Format("Can not find user. Source User ID: {0}", webPartBaseInfo.UserID);
                    throw new Exception(msg);
                }
                //当Agent Account有Full Control权限的时候，全部的WebPart都会当成Shared类型去还原
                //在还原Personalization的时候，会特殊处理，不使用isShared属性标识获取哪个WebPartManager
                //ViewWebPart除外
                if (!this.isViewWebPart && this.Manager.HasFullControlPermission)
                {
                    isShared = true;
                }
            }
            return true;
        }

        /// <summary>
        /// Check WebPart关联的Picture Library能否在目的端找到
        /// 如果Library找不到的话，因为ViewGuid错误，所以添加WebPart会有问题，因此Library找不到的话，需要放到PostAction中
        /// </summary>
        /// <param name="reload"></param>
        /// <returns></returns>
        private bool VerifyPictureLibrarySlideshowWebPart()
        {
            EnsureWebPartProperties();
            object libObj = null;
            if (webPartProperties.TryGetValue("LibraryGuid", out libObj))
            {
                Guid destGuid = Guid.Empty;
                Guid libraryGuid = new Guid(libObj.ToString());
                if (!manager.Cache.SiteMappingManager.GetValueFromListIdMapping(libraryGuid, out destGuid))
                {
                    string title;
                    if (webPartBaseInfo.ExtensionProperties != null && webPartBaseInfo.ExtensionProperties.TryGetValue("ExtensionLibraryTitle", out title))
                    {
                        destGuid = GetListIdByTitle(manager.Web.ID, title);
                    }
                }
                if (destGuid == Guid.Empty) return false;

                webPartProperties["LibraryGuid"] = destGuid;
                IAveList list = manager.Web.Lists[destGuid];
                Guid destViewId = Guid.Empty;
                object viewObj = null;
                if (webPartProperties.TryGetValue("ViewGuid", out viewObj))
                {
                    Guid viewId = new Guid(viewObj.ToString());
                    if (!manager.Cache.SiteMappingManager.GetViewGuidMappingValue(viewId, out destViewId))
                    {
                        string viewTitle;
                        if (webPartBaseInfo.ExtensionProperties != null && webPartBaseInfo.ExtensionProperties.TryGetValue("ExtensionViewTitle", out viewTitle))
                        {
                            //为了不想产生异常
                            foreach (var view in list.Views)
                            {
                                if (view.Title.Equals(viewTitle, StringComparison.OrdinalIgnoreCase))
                                {
                                    destViewId = view.ID;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (destViewId == Guid.Empty)
                {
                    destViewId = list.DefaultView.ID;
                }
                webPartProperties["ViewGuid"] = destViewId;
            }
            return true;
        }
        private bool VerifyBrowserFormWebPart()
        {
            EnsureWebPartProperties();
            object formLocation = null;
            object contentTypeId = null;
            if (webPartProperties.TryGetValue("FormLocation", out formLocation)   //FormLocation="/sites/new01/formlilb01" 
                && webPartProperties.TryGetValue("ContentTypeId", out contentTypeId)
                && formLocation != null && !formLocation.ToString().StartsWith("~", StringComparison.OrdinalIgnoreCase))
            {
                string destContentypeListUrl = null;
                if (manager.Cache.SiteMappingManager.GetValueFromListUrlMapping(formLocation.ToString(), out destContentypeListUrl))
                {
                    int index = destContentypeListUrl.TrimEnd('/').LastIndexOf('/');
                    string destContentypeListName = destContentypeListUrl.Substring(index + 1);
                    IAveList list = manager.Web.GetListByName(destContentypeListName, false);
                    if (list != null)
                    {
                        ReplaceBrowserFormContentTypeId(list.ID, contentTypeId.ToString());
                    }
                    else
                    {
                        logger.Warn("An error occurred while getting the existed list {0} for verify the Browser Form web part.", destContentypeListName.ToString());
                    }
                }
                else
                {
                    //目的端关联contenttype的list还没有还原或者是inplace job,再拿源端list取下
                    int index = formLocation.ToString().TrimEnd('/').LastIndexOf('/');
                    string destContentypeListName = formLocation.ToString().Substring(index + 1);
                    IAveList list = manager.Web.GetListByName(destContentypeListName, false);
                    if (list != null)
                    {
                        ReplaceBrowserFormContentTypeId(list.ID, contentTypeId.ToString());
                    }
                    else
                    {
                        //目的端关联contenttype的list还没有还原，此时需要在postaction中处理
                        logger.Warn("Can not found list {0} for verify the Browser Form web part.", destContentypeListName.ToString());
                        return false;
                    }
                }
            }
            return true;
        }
        private void ReplaceBrowserFormContentTypeId(Guid listId, string contentTypeId)
        {
            IAveContentTypeId destContentTypeId = null;
            if (manager.Cache.SiteMappingManager.TryGetValueFromListLevelContentTypeIdMapping(listId, contentTypeId, out destContentTypeId))
            {
                webPartProperties["ContentTypeId"] = destContentTypeId;
            }
            else
            {
                logger.Warn("Can not found the source list content type id {0} mapping for verify the Browser Form web part.", contentTypeId.ToString());
            }
        }
        protected virtual bool CheckListId()
        {
            if (isIListWebPart)
            {
                EnsureWebPartProperties();
                #region Check Web Id
                Guid destWebId = Guid.Empty;
                if (webPartBaseInfo.WebPartList != null && webPartBaseInfo.WebPartList.Count > 0)
                {
                    Guid tmpId = GetMappingWebId(webPartBaseInfo.WebPartList[0].WebId);
                    bool needUpdate = false;
                    #region ADO-5344 针对跨site级别的WebPart会存在问题，所以需要从WebPart properties中获取真正的WebId
                    object tmpObj = null;
                    if (webPartProperties.TryGetValue("WebId", out tmpObj))
                    {
                        if (tmpObj != null)
                        {
                            Guid webId = new Guid(tmpObj.ToString());
                            if (webId != Guid.Empty)
                            {
                                needUpdate = true;
                                if (!webPartBaseInfo.WebPartList[0].WebId.Equals(webId))
                                {
                                    if (!manager.Cache.SiteMappingManager.WebIDMapping.TryGetValue(webId, out webId))
                                    {
                                        //因为没有还原该web，不知道映射关系。另外webpart没有该web url，所以不知道映射关系
                                        logger.Log(AveLogLevel.DEBUG, "Failed to find the web that in web part properties. Web Id:{0}", tmpObj);
                                        return false;
                                    }
                                    tmpId = webId;
                                }
                            }
                        }
                    }
                    #endregion
                    if (tmpId == Guid.Empty)
                    {
                        logger.Log(AveLogLevel.DEBUG, "Failed to find the web that related to web part. Web Id: {0}", webPartBaseInfo.WebPartList[0].WebId.ToString());
                        return false;
                    }
                    if (needUpdate)
                    {
                        webPartProperties["WebId"] = tmpId;
                    }
                    destWebId = tmpId;
                }
                else
                {
                    destWebId = manager.Web.ID;
                }
                #endregion

                #region For language mapping
                string mappingTitle = string.Empty;
                if (manager.Cache.LanguageProcesser != null && !string.IsNullOrEmpty(webPartBaseInfo.ListTitle))
                {
                    if (manager.Cache.LanguageProcesser.ListMapping.TryGetValue(webPartBaseInfo.ListTitle, out mappingTitle))
                    {
                        webPartBaseInfo.ListTitle = mappingTitle;
                    }
                }
                #endregion

                #region Check List Id
                Guid destListId = GetMappingListId(destWebId, webPartBaseInfo.ListId, webPartBaseInfo.ListTitle);
                if (!Guid.Empty.Equals(destListId))
                {
                    webPartBaseInfo.OriginalListId = webPartBaseInfo.ListId;
                    webPartBaseInfo.ListId = destListId;
                    if (webPartBaseInfo.WebPartList == null)
                    {
                        webPartBaseInfo.WebPartList = new List<AveWebPartListInfo>();
                        webPartBaseInfo.WebPartList.Add(new AveWebPartListInfo());
                    }
                    webPartBaseInfo.WebPartList[0].WebId = destWebId;
                    if (webPartProperties.ContainsKey("ListId"))
                    {
                        webPartProperties["ListId"] = destListId.ToString();
                    }
                    if (webPartProperties.ContainsKey("ListName"))
                    {
                        webPartProperties["ListName"] = destListId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                    }
                    return true;
                }
                //WebPartBaseInfo.ListId为Guid.Empty，但是WebPartProperties中ListId或者是ListName有值的情况
                //也需要替换Properties中的Value. eg:ADO-141411
                else
                {
                    #region 替换Properties中List相关Value
                    object listObj = null;
                    if (webPartProperties.TryGetValue("ListId", out listObj))
                    {
                        Guid guidResult;
                        if (listObj != null && Guid.TryParse(listObj.ToString(), out guidResult))
                        {
                            Guid realId;
                            if (manager.Cache.SiteMappingManager.GetValueFromListIdMapping(guidResult, out realId))
                            {
                                webPartProperties["ListId"] = realId.ToString();
                            }
                        }
                    }
                    if (webPartProperties.TryGetValue("ListName", out listObj))
                    {
                        Guid guidResult;
                        if (listObj != null && Guid.TryParse(listObj.ToString(), out guidResult))
                        {
                            Guid realId;
                            if (manager.Cache.SiteMappingManager.GetValueFromListIdMapping(guidResult, out realId))
                            {
                                webPartProperties["ListName"] = realId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    #endregion
                }
                if (!Guid.Empty.Equals(webPartBaseInfo.ListId))
                {
                    return false;
                }
                #endregion
            }
            return true;
        }

        public void Dispose()
        {
            if (this.internalWebPart != null)
            {
                this.internalWebPart.Dispose();
                this.internalWebPart = null;
            }
        }

        public System.Web.UI.WebControls.WebParts.WebPartExportMode ExportMode
        {
            get
            {
                return WebPart.ExportMode;
            }
            set
            {
                WebPart.ExportMode = value;
            }
        }

        public bool AllowConnect
        {
            get
            {
                return WebPart.AllowConnect;
            }
            set
            {
                WebPart.AllowConnect = value;
            }
        }

        public bool AllowMinimize
        {
            get
            {
                return WebPart.AllowMinimize;
            }
            set
            {
                WebPart.AllowMinimize = value;
            }
        }

        public bool AllowZoneChange
        {
            get
            {
                return WebPart.AllowZoneChange;
            }
            set
            {
                WebPart.AllowZoneChange = value;
            }
        }

        public System.Web.UI.WebControls.WebParts.PartChromeState ChromeState
        {
            get
            {
                return WebPart.ChromeState;
            }
            set
            {
                WebPart.ChromeState = value;
            }
        }

        public System.Web.UI.WebControls.ContentDirection Direction
        {
            get
            {
                return WebPart.Direction;
            }
            set
            {
                WebPart.Direction = value;
            }
        }

        public System.Web.UI.WebControls.WebParts.WebPartHelpMode HelpMode
        {
            get
            {
                return WebPart.HelpMode;
            }
            set
            {
                WebPart.HelpMode = value;
            }
        }

        public string HelpUrl
        {
            get
            {
                return WebPart.HelpUrl;
            }
            set
            {
                WebPart.HelpUrl = value;
            }
        }

        public string MissingAssembly
        {
            get
            {
                return WebPart.ImportErrorMessage;
            }
            set
            {
                WebPart.ImportErrorMessage = value;
            }
        }
    }

    public class SpecialWebPartUpdater
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected System.Web.UI.WebControls.WebParts.WebPart mWebPart;
        protected AveWebPart mAveWebPart;
        protected bool needPostAction = true;

        protected SpecialWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
        {
            mWebPart = webPart;
            mAveWebPart = aveWebPart;
        }

        public bool NeedReloadList { get; protected set; }

        public static SpecialWebPartUpdater GetWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
        {
            try
            {
                if (webPart == null)
                {
                    return null;
                }
                var updater = AveCustomWebPartUpdaterUtility.GetWebPartUpdater(webPart, aveDoc);
                if (updater == null)
                {
                    Type subType = Type.GetType(string.Format("AvePoint.ObjectModel.Server19.Ave{0}Updater", webPart.GetType().Name), false, true);
                    if (subType != null)
                    {
                        updater = subType.GetConstructor(new Type[] { typeof(System.Web.UI.WebControls.WebParts.WebPart), typeof(AveWebPart) }).Invoke(new object[] { webPart, aveDoc }) as SpecialWebPartUpdater;
                    }
                }
                return updater;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while getting special web part updater. Exception: {0}", ex.ToString());
                return null;
            }
        }

        public virtual bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo) { return true; }

        public virtual bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            return true;
        }
    }

    #region IListWebPartUpdater

    class IListWebPartUpdater : SpecialWebPartUpdater
    {
        public IListWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            if (webPartInfo.Flags > 0)
            {
                ((IListWebPart)mWebPart).ViewFlags = GetViewFlags(webPartInfo.Flags);
                this.mAveWebPart.WebPartProperties.Remove("ViewFlags");
            }
            if (webPartInfo.Type > 0)
            {
                ((IListWebPart)mWebPart).PageType = (PAGETYPE)(webPartInfo.Type);
                this.mAveWebPart.WebPartProperties.Remove("PageType");
            }
            if (webPartInfo.ListId != Guid.Empty)
            {
                ((IListWebPart)mWebPart).ListId = webPartInfo.ListId;
            }
            if (webPartInfo.BaseViewID.HasValue)
            {
                ((IListWebPart)mWebPart).ViewId = webPartInfo.BaseViewID.Value;
            }
            else
            {
                ((IListWebPart)mWebPart).ViewId = 0;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            if (webPartInfo != null)
            {
                var flag = GetViewFlags(webPartInfo.Flags);
                var viewWebPart = mAveWebPart as AveSPViewWebPart;
                if (viewWebPart != null)
                {
                    if (!viewWebPart.View.DefaultView && (flag & SPViewFlags.Default) == SPViewFlags.Default
                        && ((((IListWebPart)mWebPart).ViewFlags & SPViewFlags.Default) != SPViewFlags.Default))
                    {
                        flag -= SPViewFlags.Default;
                    }
                    if (!viewWebPart.View.MobileDefaultView && (flag & SPViewFlags.DefaultMobile) == SPViewFlags.DefaultMobile
                        && ((((IListWebPart)mWebPart).ViewFlags & SPViewFlags.DefaultMobile) != SPViewFlags.DefaultMobile))
                    {
                        flag -= SPViewFlags.DefaultMobile;
                    }
                }
                ((IListWebPart)mWebPart).ViewFlags = flag;

                if (webPartInfo.Type.HasValue)
                {
                    var pageType = (PAGETYPE)(webPartInfo.Type);
                    //不能修改view webpart的PageType，否则会引起default view的不正常情况。
                    if (viewWebPart == null)
                    {
                        ((IListWebPart)mWebPart).PageType = pageType;
                    }
                }
                if (!string.IsNullOrEmpty(mWebPart.TitleUrl))
                {
                    string url = AveReplaceProcessor.UrlReplace(mWebPart.TitleUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    if (!mWebPart.TitleUrl.Equals(url))
                    {
                        mWebPart.TitleUrl = url;
                    }
                }
                if (!string.IsNullOrEmpty(mWebPart.TitleIconImageUrl))
                {
                    string iconUrl = AveReplaceProcessor.UrlReplace(mWebPart.TitleIconImageUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    if (!mWebPart.TitleIconImageUrl.Equals(iconUrl))
                    {
                        mWebPart.TitleIconImageUrl = iconUrl;
                    }
                }
                if (!string.IsNullOrEmpty(mWebPart.CatalogIconImageUrl))
                {
                    string catalogIconUrl = AveReplaceProcessor.UrlReplace(mWebPart.CatalogIconImageUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    if (!mWebPart.CatalogIconImageUrl.Equals(catalogIconUrl))
                    {
                        mWebPart.CatalogIconImageUrl = catalogIconUrl;
                    }
                }
            }
            //Move to EnsureViewInfo method
            //ReplaceViewFields(webPartInfo);
            return base.DoUpateAfterAdd(webPartInfo);
        }

        #region Move to EnsureViewInfo method
        //private void ReplaceViewFields(AveWebPartBaseInfo webPartInfo)
        //{
        //    if (webPartInfo == null)
        //    {
        //        ReplaceViewFields();
        //        return;
        //    }
        //    if (webPartInfo.View == null)
        //    {
        //        return;
        //    }
        //    try
        //    {
        //        if (!mAveWebPart.Manager.Cache.ListFieldsMapping.ContainsKey(webPartInfo.ListId))
        //        {
        //            mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, webPartInfo.OriginalListId, mAveWebPart.Manager.File.ServerRelativeUrl, this.mWebPart,webPartInfo.UserID);
        //            return;
        //        }
        //        IAveList list = null;
        //        try
        //        {
        //            list = mAveWebPart.Manager.Web.GetList(webPartInfo.ListId);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Warn("Cannot get list related to web part. List Id: {0}, exception: {1}", webPartInfo.ListId, ex.ToString());
        //        }
        //        IAveFieldMapping fieldMapping = mAveWebPart.Manager.Cache.ListFieldsMapping[webPartInfo.ListId];
        //        webPartInfo.View = ReplaceViewFields(webPartInfo.View, fieldMapping, list);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("An error occurred while do replace view fields for the web part. Error: {0}", e.ToString());
        //    }
        //}

        //private void ReplaceViewFields()
        //{
        //    byte[] view = GetIListWebPartView();
        //    if (view == null)
        //    {
        //        return;
        //    }
        //    Guid listId = ((IListWebPart)mWebPart).ListId;
        //    IAveList list = null;
        //    if (mAveWebPart.Manager.Cache.ListFieldsMapping.ContainsKey(listId))
        //    {
        //        try
        //        {
        //            list = mAveWebPart.Manager.Web.GetList(listId);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Warn("Cannot get list related to web part. List Id: {0}, exception: {1}", listId, ex.ToString());
        //        }
        //        IAveFieldMapping fieldMapping = mAveWebPart.Manager.Cache.ListFieldsMapping[listId];
        //        view = ReplaceViewFields(view, fieldMapping, list);
        //        SetIListWebPartView(view);
        //    }
        //}

        //private byte[] GetIListWebPartView()
        //{
        //    Guid webPartId = mAveWebPart.Manager.GetStorageKey(mWebPart, true);
        //    return mAveWebPart.Manager.GetIListWebPartView(mAveWebPart.Manager.Web.Site.ID, mAveWebPart.Manager.File.UniqueId, webPartId);
        //}

        //private void SetIListWebPartView(byte[] view)
        //{
        //    Guid webPartId = mAveWebPart.Manager.GetStorageKey(mWebPart, true);
        //    mAveWebPart.Manager.SetIListWebPartView(mAveWebPart.Manager.Web.Site.ID, mAveWebPart.Manager.File.UniqueId, webPartId, view);
        //}

        //private byte[] ReplaceViewFields(byte[] view, IAveFieldMapping fieldMapping, IAveList list)
        //{
        //    if (view == null)
        //    {
        //        return null;
        //    }
        //    try
        //    {
        //        string viewString = AveCompressedUtility.GetTCompressedString(view);
        //        if (!string.IsNullOrEmpty(viewString))
        //        {
        //            XmlDocument xDoc = new XmlDocument();
        //            viewString = "<root>" + viewString + "</root>";
        //            xDoc.LoadXml(viewString);
        //            XmlNodeList nodes = xDoc.GetElementsByTagName("FieldRef");
        //            if (nodes.Count <= 0)
        //            {
        //                return view;
        //            }
        //            bool isFlatViewInDiscussionBoard = IsFlatViewInDiscussionBoard(list, nodes);
        //            bool isBodyChecked = false;
        //            bool isTrimmedBodyChecked = false;
        //            for (int i = nodes.Count - 1; i >= 0; i--)
        //            {
        //                if (nodes[i].Attributes["Name"] != null)
        //                {
        //                    string fieldName = nodes[i].Attributes["Name"].Value;
        //                    string mappingName = fieldMapping.GetMappingRestoredFieldInternalName(fieldName);
        //                    if (!string.IsNullOrEmpty(mappingName))
        //                    {
        //                        nodes[i].Attributes["Name"].Value = mappingName;
        //                    }
        //                    else if (list != null && (list.Fields.GetFieldByInternalName(fieldName, false) == null))
        //                    {
        //                        nodes[i].ParentNode.RemoveChild(nodes[i]);
        //                    }
        //                    if (isFlatViewInDiscussionBoard)
        //                    {
        //                        if (fieldName.Equals("Body", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            isBodyChecked = true;
        //                        }
        //                        if (fieldName.Equals("TrimmedBody", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            isTrimmedBodyChecked = true;
        //                        }
        //                    }
        //                }
        //            }
        //            if (isFlatViewInDiscussionBoard)
        //            {
        //                ReplaceFlatViewFields(xDoc, isBodyChecked, isTrimmedBodyChecked);
        //            }
        //            viewString = xDoc.FirstChild.InnerXml;
        //            return AveCompressedUtility.GetTCompressedBytes(viewString);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("An error occurred while do replace view fields for the web part. Error: {0}", e.ToString());
        //    }
        //    return view;
        //}

        ////flat view :has StatusBar attribute and Explicit is true.
        //private bool IsFlatViewInDiscussionBoard(IAveList list, XmlNodeList fieldRefNodes)
        //{
        //    bool isFlatView = false;
        //    if (list != null && list.BaseTemplate == AveListTemplateType.DiscussionBoard)
        //    {
        //        foreach (XmlNode node in fieldRefNodes)
        //        {
        //            string fieldRefName = node.Attributes["Name"] == null ? string.Empty : node.Attributes["Name"].Value;
        //            string ExplicitValue = node.Attributes["Explicit"] == null ? string.Empty : node.Attributes["Explicit"].Value;
        //            if (fieldRefName.Equals("StatusBar", StringComparison.OrdinalIgnoreCase) && ExplicitValue.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
        //            {
        //                isFlatView = true;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        isFlatView = false;
        //    }
        //    return isFlatView;
        //}

        ////Flat view必须check body 与TrimmedBody两个column。ADO-113652.
        //private void ReplaceFlatViewFields(XmlDocument xDoc, bool isBodyChecked, bool isTrimmedBodyChecked)
        //{
        //    XmlNode node = xDoc.SelectSingleNode("root");
        //    XmlNodeList nodelist = node.SelectNodes("ViewFields");
        //    foreach (var childnode in nodelist)
        //    {
        //        XmlElement rootElement = (XmlElement)childnode;
        //        if (!isTrimmedBodyChecked)
        //        {
        //            XmlElement trimmedBodyElement = xDoc.CreateElement("FieldRef");
        //            trimmedBodyElement.SetAttribute("Name", "TrimmedBody");
        //            rootElement.AppendChild(trimmedBodyElement);
        //        }
        //        if (!isBodyChecked)
        //        {
        //            XmlElement bodyElement = xDoc.CreateElement("FieldRef");
        //            bodyElement.SetAttribute("Name", "Body");
        //            rootElement.AppendChild(bodyElement);
        //        }
        //    }
        //}
        #endregion

        private SPViewFlags GetViewFlags(int flags)
        {
            SPViewFlags flag = SPViewFlags.None;
            foreach (SPViewFlags viewFlag in Enum.GetValues(typeof(SPViewFlags)))
            {
                if ((flags & (int)viewFlag) != 0)
                {
                    flag = flag | viewFlag;
                }
            }
            return flag;
        }
    }

    class AveListViewWebPartUpdater : IListWebPartUpdater
    {
        public AveListViewWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart avePart)
            : base(webPart, avePart)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            ListViewWebPart part = mWebPart as ListViewWebPart;
            part.ListName = webPartInfo.ListId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                if (!Guid.Equals(mAveWebPart.Manager.Web.ID, webPartInfo.WebPartList[0].WebId))
                {
                    part.WebId = webPartInfo.WebPartList[0].WebId;
                }
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            if (webPartInfo != null)
            {
                Guid listId = webPartInfo.ListId;
                Guid destinationListId;
                if (mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(listId, out destinationListId))
                {
                    listId = destinationListId;
                }
                ListViewWebPart part = mWebPart as ListViewWebPart;
                part.ListName = listId.ToString("B").ToUpperInvariant();
                if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
                {
                    Guid currentWebId = mAveWebPart.Manager.Web.ID;
                    part.WebId = Guid.Equals(currentWebId, webPartInfo.WebPartList[0].WebId) ?
                        Guid.Empty : webPartInfo.WebPartList[0].WebId;
                }
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveXsltListViewWebPartUpdater : IListWebPartUpdater
    {
        public AveXsltListViewWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            XsltListViewWebPart part = mWebPart as XsltListViewWebPart;
            //ADO-160937 Access List 的pendingreq.aspx 上的webpart需要这样才可以正确还原
            var mappingId = base.mAveWebPart.GetMappingListId(Guid.Empty, webPartInfo.ListId, webPartInfo.ListTitle);
            var list = mappingId != Guid.Empty ? base.mAveWebPart.Manager.Web.Lists.GetListById(mappingId, false) : base.mAveWebPart.Manager.Web.Lists.TryGetList(webPartInfo.ListTitle);
            if (list != null && list.BaseTemplate == AveListTemplateType.AccessRequest)
            {
                part.ListName = webPartInfo.ListId.ToString();
                part.XmlDefinition = webPartInfo.XmlDefinition ?? string.Empty;
                part.InplaceSearchEnabled = false;
                part.ChromeType = System.Web.UI.WebControls.WebParts.PartChromeType.None;
                part.DisableSaveAsNewViewButton = true;
                part.DisableViewSelectorMenu = true;
            }
            else
            {
                part.ListName = webPartInfo.ListId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            if (webPartInfo != null)
            {
                ReplaceWithIndexID(webPartInfo);
                UpdateJSLink(webPartInfo);
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }

        private void UpdateJSLink(AveWebPartBaseInfo webPartInfo)
        {
            if (string.IsNullOrEmpty(webPartInfo.DefinitionXml))
            {
                return;
            }
            try
            {
                XsltListViewWebPart part = mWebPart as XsltListViewWebPart;
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webPartInfo.DefinitionXml);
                XmlNode defNode = doc.SelectSingleNode(".//*[@name = 'JSLink']");
                part.JSLink = defNode.InnerText;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while update JSLink for webpart, error: {0}", e);
            }
        }

        private void ReplaceWithIndexID(AveWebPartBaseInfo webPartInfo)
        {
            if (webPartInfo.View != null)
            {
                try
                {
                    string viewString = AveCompressedUtility.GetTCompressedString(webPartInfo.View);
                    bool changed = false;
                    if (!string.IsNullOrEmpty(viewString))
                    {
                        XmlDocument xDoc = new XmlDocument();
                        viewString = "<root>" + viewString + "</root>";
                        xDoc.LoadXml(viewString);
                        IAveList list = null;
                        if (webPartInfo.ExtensionProperties != null && webPartInfo.ExtensionProperties.ContainsKey("WithIndex"))
                        {
                            string withIndex = webPartInfo.ExtensionProperties["WithIndex"];
                            XmlNodeList nodes = xDoc.GetElementsByTagName("WithIndex");
                            if (nodes != null && nodes.Count > 0)
                            {
                                string indexID = nodes[0].Attributes["ID"].Value;
                                list = mAveWebPart.Manager.Web.GetList(webPartInfo.ListId);
                                foreach (var index in list.FieldIndexes)
                                {
                                    Guid field1 = index.GetField(0);
                                    Guid field2 = index.GetField(1);
                                    string indexStr = field1.ToString() + "#" + field2.ToString();
                                    if (indexStr.Equals(withIndex, StringComparison.OrdinalIgnoreCase))
                                    {
                                        nodes[0].Attributes["ID"].Value = index.ID.ToString();
                                        changed = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (xDoc.GetElementsByTagName("CalendarSettings").Count > 0)
                        {
                            Guid webId = base.mAveWebPart.Manager.Web.ID;
                            Guid listId = webPartInfo.ListId;
                            Guid viewId = list != null ? list.Views[base.mAveWebPart.StorageKey].ID : mAveWebPart.Manager.Web.GetList(webPartInfo.ListId).Views[base.mAveWebPart.StorageKey].ID;
                            base.mAveWebPart.Manager.AddToNeedResetCalendarSettingsViews(webId, listId, viewId);
                        }
                        if (changed)
                        {
                            viewString = xDoc.FirstChild.InnerXml;
                            webPartInfo.View = AveCompressedUtility.GetTCompressedBytes(viewString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("An exception occurred while ReplaceWithIndexID. Exception: {0}", ex.ToString());
                }
            }
        }
    }

    // add the method for DataFormWebPart update
    class AveDataFormWebPartUpdater : IListWebPartUpdater
    {
        public AveDataFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart avePart)
            : base(webPart, avePart)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            DataFormWebPart part = mWebPart as DataFormWebPart;
            part.ListName = webPartInfo.ListId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            UpdateDataFormWebPartBinding(webPartInfo);
            return base.DoUpateAfterAdd(webPartInfo);
        }
        //DOC-64411 DOC-62257
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        private void UpdateDataFormWebPartBinding(AveWebPartBaseInfo webPartInfo)
        {
            string bindingString = string.Empty;
            string dataSourceString = string.Empty;
            string dataFieldsString = string.Empty;

            DataFormWebPart mDataFormWP = this.mWebPart as DataFormWebPart;
            bindingString = mDataFormWP.ParameterBindings;
            dataSourceString = mDataFormWP.DataSourcesString;
            dataFieldsString = mDataFormWP.DataFields;
            Dictionary<string, List<string>> bindingAndDataSourceDict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(bindingString))
            {
                XmlDocument docBinding = new XmlDocument();
                docBinding.LoadXml("<root>" + bindingString + "</root>");
                foreach (XmlElement node in docBinding.GetElementsByTagName("ParameterBinding"))
                {
                    string strName = node.GetAttribute("Name");
                    string value = node.GetAttribute("DefaultValue");
                    if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                    {
                        if (!bindingAndDataSourceDict.ContainsKey(strName))
                        {
                            List<string> temp = new List<string>();
                            temp.Add(value);
                            bindingAndDataSourceDict.Add(strName, temp);
                        }
                        else if (!bindingAndDataSourceDict[strName].Contains(value))
                        {
                            bindingAndDataSourceDict[strName].Add(value);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(dataSourceString))
            {
                XmlDocument docDataSource = new XmlDocument();
                int index = dataSourceString.LastIndexOf("%>", StringComparison.Ordinal) + 2;
                List<string> tagNames = GetTagNames(dataSourceString.Substring(0, index));
                dataSourceString = dataSourceString.Substring(index);
                dataSourceString = dataSourceString.Replace(':', '_');
                docDataSource.LoadXml("<root>" + dataSourceString + "</root>");
                foreach (string tagName in tagNames)
                {
                    foreach (XmlElement node in docDataSource.GetElementsByTagName(tagName))
                    {
                        string strName = node.GetAttribute("Name");
                        string value = node.GetAttribute("DefaultValue");
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(value))
                        {
                            if (!bindingAndDataSourceDict.ContainsKey(strName))
                            {
                                List<string> temp = new List<string>();
                                temp.Add(value);
                                bindingAndDataSourceDict.Add(strName, temp);
                            }
                            else if (!bindingAndDataSourceDict[strName].Contains(value))
                            {
                                bindingAndDataSourceDict[strName].Add(value);
                            }
                        }
                    }
                }
            }

            if (bindingAndDataSourceDict.ContainsKey("ListID"))
            {
                foreach (string listId in bindingAndDataSourceDict["ListID"])
                {
                    Guid tempListId = new Guid(listId);
                    Guid destinationListId;
                    if (mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(tempListId, out destinationListId))
                    {
                        if (!string.IsNullOrEmpty(mDataFormWP.ParameterBindings))
                        {
                            mDataFormWP.ParameterBindings = Regex.Replace(mDataFormWP.ParameterBindings, tempListId.ToString(), destinationListId.ToString().ToUpper(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
                        }
                        if (!string.IsNullOrEmpty(mDataFormWP.DataSourcesString))
                        {
                            mDataFormWP.DataSourcesString = Regex.Replace(mDataFormWP.DataSourcesString, tempListId.ToString(), destinationListId.ToString().ToUpper(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
                        }
                        IAveFieldMapping fieldMapping;
                        if (mAveWebPart.Manager.Cache.SiteMappingManager.TryGetValueFromListFieldsMapping(destinationListId, out fieldMapping))
                        {
                            ReplaceXslContent(dataFieldsString, mDataFormWP, fieldMapping);
                        }
                    }
                    else if (webPartInfo != null && webPartInfo.OriginalListId == tempListId)
                    {
                        Guid destListId = webPartInfo.ListId;
                        if (!string.IsNullOrEmpty(mDataFormWP.ParameterBindings))
                        {
                            mDataFormWP.ParameterBindings = Regex.Replace(mDataFormWP.ParameterBindings, tempListId.ToString(), destListId.ToString().ToUpper(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
                        }
                        if (!string.IsNullOrEmpty(mDataFormWP.DataSourcesString))
                        {
                            mDataFormWP.DataSourcesString = Regex.Replace(mDataFormWP.DataSourcesString, tempListId.ToString(), destListId.ToString().ToUpper(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);
                        }
                    }
                    else if (!mAveWebPart.Manager.Cache.SiteMappingManager.ListIdMappingContainsValue(tempListId))
                    {
                        mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, tempListId, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                        break;
                    }
                }
            }
            if (bindingAndDataSourceDict.ContainsKey("WebURL"))
            {
                foreach (string webUrl in bindingAndDataSourceDict["WebURL"])
                {
                    string dKey = "\"" + webUrl + "\"";
                    string dValue = "\"" + AveReplaceProcessor.UrlReplace(webUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true),
                        mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl) + "\"";
                    if (!dKey.Equals(dValue, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(mDataFormWP.ParameterBindings))
                        {
                            mDataFormWP.ParameterBindings = mDataFormWP.ParameterBindings.Replace(dKey, dValue);
                        }
                        if (!string.IsNullOrEmpty(mDataFormWP.DataSourcesString))
                        {
                            mDataFormWP.DataSourcesString = mDataFormWP.DataSourcesString.Replace(dKey, dValue);
                        }
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        private List<string> GetTagNames(string dataSourceString)
        {
            string startSymbol = "<%@ Register TagPrefix=\"";
            string endSymbol = "\" Namespace=\"";
            List<string> tags = new List<string> { "asp_Parameter" };
            try
            {
                int startIndex = dataSourceString.IndexOf(startSymbol, StringComparison.Ordinal);
                int endIndex = dataSourceString.IndexOf(endSymbol, StringComparison.Ordinal);
                while (startIndex > -1)
                {
                    startIndex += startSymbol.Length;
                    if (endIndex > startIndex)
                    {
                        string prefix = dataSourceString.Substring(startIndex, endIndex - startIndex);
                        if (!prefix.Equals("sharepoint", StringComparison.OrdinalIgnoreCase))
                        {
                            tags.Add(prefix + "_DataFormParameter");
                        }
                    }
                    startIndex = dataSourceString.IndexOf(startSymbol, startIndex, StringComparison.Ordinal);
                    endIndex = dataSourceString.IndexOf(endSymbol, endIndex + endSymbol.Length, StringComparison.Ordinal);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Failed to get the tag prefix for DataFormWebPart. Error: {0}", ex.ToString());
            }
            return tags;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-vh is a key")]
        private void ReplaceXslContent(string dataFieldsString, DataFormWebPart mDataFormWP, IAveFieldMapping fieldMapping)
        {
            if (!string.IsNullOrEmpty(mDataFormWP.Xsl) && fieldMapping != null)
            {
                IEnumerable<KeyValuePair<string, string>> internalNameMapping = fieldMapping.EnumFieldInternalNameMapping();
                if (!string.IsNullOrEmpty(dataFieldsString))
                {
                    mDataFormWP.DataFields = string.Empty;
                    string[] result = dataFieldsString.Split(';');
                    foreach (string key in result)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            string temp = string.Empty;

                            foreach (KeyValuePair<string, string> pair in internalNameMapping)
                            {
                                if (key.Contains(pair.Key))
                                {
                                    temp = key.Replace(pair.Key, pair.Key);
                                    break;
                                }
                            }
                            if (temp != string.Empty)
                            {
                                mDataFormWP.DataFields += temp + ";";
                            }
                            else
                            {
                                mDataFormWP.DataFields += key + ";";
                            }
                        }
                    }
                }
                XmlDocument docXml = new XmlDocument();
                docXml.LoadXml(mDataFormWP.Xsl);
                XmlNodeList nodeList = docXml.GetElementsByTagName("xsl:value-of");
                foreach (XmlNode node in nodeList)
                {
                    string internalName = node.Attributes["select"].Value.Trim().TrimStart('@');
                    string mappedName = fieldMapping.GetMappingRestoredFieldInternalName(internalName);
                    if (!string.IsNullOrEmpty(internalName) && !string.IsNullOrEmpty(mappedName))
                    {
                        node.Attributes["select"].Value = "@" + mappedName;
                    }
                }

                nodeList = docXml.GetElementsByTagName("th");
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes["class"].Value.Equals("ms-vh", StringComparison.OrdinalIgnoreCase))
                    {
                        string displayName = node.InnerText;
                        string mappedName = fieldMapping.GetMappingRestoredFieldDisplayName(displayName);
                        if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(mappedName))
                        {
                            node.InnerText = mappedName;
                        }
                    }
                }
                //CI-12175
                nodeList = docXml.GetElementsByTagName("SharePoint:AppendOnlyHistory");
                foreach (XmlNode node in nodeList)
                {
                    string fieldName = node.Attributes["FieldName"].Value;
                    node.Attributes["FieldName"].Value = mAveWebPart.Manager.File.ParentFolder.ParentList.Fields[fieldName].InternalName;
                }

                nodeList = docXml.GetElementsByTagName("xsl:when");
                foreach (XmlNode node in nodeList)
                {
                    string internalName = node.Attributes["test"].Value.Trim();
                    if (!string.IsNullOrEmpty(internalName))
                    {
                        foreach (KeyValuePair<string, string> pair in internalNameMapping)
                        {
                            if (internalName.Contains(pair.Key))
                            {
                                node.Attributes["test"].Value = internalName.Replace("@" + pair.Key, "@" + pair.Value);
                            }
                        }
                    }
                }

                nodeList = docXml.GetElementsByTagName("xsl:value-of");
                foreach (XmlNode node in nodeList)
                {
                    string internalName = node.Attributes["select"].Value.Trim();
                    if (!string.IsNullOrEmpty(internalName))
                    {
                        foreach (KeyValuePair<string, string> pair in internalNameMapping)
                        {
                            if (internalName.Contains(pair.Key))
                            {
                                node.Attributes["select"].Value = internalName.Replace("@" + pair.Key, "@" + pair.Value);
                            }
                        }
                    }
                }

                //Replace DataFormWebPart FieldInternalName With InternalNameMapping
                nodeList = docXml.GetElementsByTagName("SharePoint:FormField");
                foreach (XmlNode node in nodeList)
                {
                    string fieldName = node.Attributes["FieldName"].Value;
                    if (fieldName.Equals("Comment", StringComparison.OrdinalIgnoreCase))
                    {
                        node.Attributes["FieldName"].Value = mAveWebPart.Manager.File.ParentFolder.ParentList.Fields[fieldName].InternalName;
                    }
                    string mappedName = fieldMapping.GetMappingRestoredFieldInternalName(fieldName);
                    if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(mappedName))
                    {
                        node.Attributes["FieldName"].Value = mappedName;
                        node.Attributes["__designer:bind"].Value = node.Attributes["__designer:bind"].Value.Replace(fieldName, node.Attributes["FieldName"].Value);
                    }
                }
                nodeList = docXml.GetElementsByTagName("SharePoint:FieldDescription");
                foreach (XmlNode node in nodeList)
                {
                    string fieldName = node.Attributes["FieldName"].Value;
                    if (fieldName.Equals("Comment", StringComparison.OrdinalIgnoreCase))
                    {
                        node.Attributes["FieldName"].Value = mAveWebPart.Manager.File.ParentFolder.ParentList.Fields[fieldName].InternalName;
                    }
                    string mappedName = fieldMapping.GetMappingRestoredFieldInternalName(fieldName);
                    if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(mappedName))
                    {
                        node.Attributes["FieldName"].Value = mappedName;
                    }
                }
                mDataFormWP.Xsl = docXml.InnerXml;
                docXml.RemoveAll();
            }
        }
    }

    class AveExcelWebRendererUpdater : SpecialWebPartUpdater
    {
        public AveExcelWebRendererUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                string url = AveAssemblyUtility.GetPropertyValue(this.mWebPart, "WorkbookUri") as string;
                if (!String.IsNullOrEmpty(url))
                {
                    AveAssemblyUtility.SetPropertyValue(this.mWebPart, "WorkbookUri", AveReplaceProcessor.UrlReplace(url, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl));
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetWeBPartAttError, e.ToString());
            }
            if (!string.IsNullOrEmpty(mWebPart.TitleIconImageUrl))
            {
                string iconUrl = AveReplaceProcessor.UrlReplace(mWebPart.TitleIconImageUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                if (!mWebPart.TitleIconImageUrl.Equals(iconUrl))
                {
                    mWebPart.TitleIconImageUrl = iconUrl;
                }
            }
            if (!string.IsNullOrEmpty(mWebPart.CatalogIconImageUrl))
            {
                string catalogIconUrl = AveReplaceProcessor.UrlReplace(mWebPart.CatalogIconImageUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                if (!mWebPart.CatalogIconImageUrl.Equals(catalogIconUrl))
                {
                    mWebPart.CatalogIconImageUrl = catalogIconUrl;
                }
            }
            if (!string.IsNullOrEmpty(mWebPart.HelpUrl))
            {
                string helpUrl = AveReplaceProcessor.UrlReplace(mWebPart.HelpUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                if (!mWebPart.HelpUrl.Equals(helpUrl))
                {
                    mWebPart.HelpUrl = helpUrl;
                }
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveXsltListFormWebPartUpdater : IListWebPartUpdater
    {
        public AveXsltListFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            XsltListFormWebPart part = mWebPart as XsltListFormWebPart;
            part.ListName = webPartInfo.ListId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
            if (webPartInfo.WebPartList != null && webPartInfo.WebPartList.Count > 0)
            {
                part.WebId = webPartInfo.WebPartList[0].WebId;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveListFormWebPartUpdater : IListWebPartUpdater
    {
        public AveListFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            Guid listId = webPartInfo.ListId;
            ListFormWebPart part = mWebPart as ListFormWebPart;
            part.ListName = listId.ToString("B").ToUpper(CultureInfo.InvariantCulture);
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveBrowserFormWebPartUpdater : IListWebPartUpdater
    {
        public AveBrowserFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveBlogLinksWebPartUpdater : SpecialWebPartUpdater
    {
        public AveBlogLinksWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            BlogLinksWebPart blogLinksWebPart = mWebPart as BlogLinksWebPart;
            Guid destinationListId;
            if (mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(blogLinksWebPart.ListId, out destinationListId))
            {
                blogLinksWebPart.ListId = destinationListId;
                needPostAction = false;
            }
            else if (webPartInfo != null && webPartInfo.ExtensionProperties != null && webPartInfo.ExtensionProperties.ContainsKey("ExtensionListTitle"))
            {
                string listTitle = webPartInfo.ExtensionProperties["ExtensionListTitle"];
                try
                {
                    // NeedReloadList: 使用List["Title"]取list的时候会重新load web下的lists，会导致web下的list和缓存的list不一致，需要重新load一遍。
                    NeedReloadList = true;
                    blogLinksWebPart.ListId = mAveWebPart.Manager.Web.Lists[listTitle].ID;
                    needPostAction = false;
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, "Can not get the list by title. List Title: {0}. Error: {1}", listTitle, ex.ToString());
                }
            }
            if (needPostAction)
            {
                mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, blogLinksWebPart.ListId, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                return false;
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    #endregion

    #region SpecialWebPartUpdater

    /// <summary>
    /// In SharePoint API class TableOfContentsWebPart is parent class of TOCPart,
    /// and they have the same behavior in AfterAdd Options,
    /// so keep this relations in DocAve API;
    /// </summary>
    class AveTableOfContentsWebPartUpdater : SpecialWebPartUpdater
    {
        public AveTableOfContentsWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
            : base(webPart, aveWebPart)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                TableOfContentsWebPart webPart = mWebPart as TableOfContentsWebPart;
                string areaURL = webPart.AnchorLocation;
                if (!string.IsNullOrEmpty(areaURL))
                {
                    areaURL = AveReplaceProcessor.UrlReplace(areaURL, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    webPart.AnchorLocation = areaURL;
                }
                #region update IncludeContentFromStartingLocation to webPart 
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(webPartInfo.DefinitionXml);
                XmlNode defNode = xmlDoc.SelectSingleNode(".//*[@name = 'IncludeContentFromStartingLocation']");
                if (defNode != null)
                {
                    var boolValue = true;
                    if (Boolean.TryParse(defNode.InnerText, out boolValue))
                    {
                        webPart.IncludeContentFromStartingLocation = boolValue;
                    }
                }
                #endregion
                return base.DoUpateAfterAdd(webPartInfo);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateWebpartInfoError, e.ToString());
                return false;
            }
        }
    }

    class AveTOCPartUpdater : AveTableOfContentsWebPartUpdater
    {
        public AveTOCPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
            : base(webPart, aveWebPart)
        { }
    }

    class AveXmlWebpartUpdater : SpecialWebPartUpdater
    {
        public AveXmlWebpartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "bgsound: Symbol used in xml or html")]
        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                XmlWebPart tempWebPart = mWebPart as XmlWebPart;
                if (tempWebPart != null)
                {
                    if (!string.IsNullOrEmpty(tempWebPart.Xml.InnerText))
                    {
                        StringBuilder sb = new StringBuilder(tempWebPart.Xml.InnerText);

                        Dictionary<int, string> UrlList = null;
                        //替换img标签内的src
                        UrlList = GetTagList(tempWebPart.Xml.InnerText, "img", "src");
                        foreach (KeyValuePair<int, string> kvp in UrlList)
                        {
                            sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
                        }
                        //替换image标签内的src
                        UrlList = GetTagList(tempWebPart.Xml.InnerText, "image", "src");
                        foreach (KeyValuePair<int, string> kvp in UrlList)
                        {
                            sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
                        }
                        //替换a标签内的href
                        UrlList = GetTagList(sb.ToString(), "a", "href");
                        foreach (KeyValuePair<int, string> kvp in UrlList)
                        {
                            sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
                        }
                        UrlList = GetTagList(sb.ToString(), "a", "rel");
                        foreach (KeyValuePair<int, string> kvp in UrlList)
                        {
                            sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
                        }

                        //替换embed标签内的src
                        UrlList = GetTagList(tempWebPart.Xml.InnerText, "embed", "src");
                        foreach (KeyValuePair<int, string> kvp in UrlList)
                        {
                            sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
                        }
                        //替换bgsound标签内的src
                        UrlList = GetTagList(tempWebPart.Xml.InnerText, "bgsound", "src");
                        foreach (KeyValuePair<int, string> kvp in UrlList)
                        {
                            sb.Replace(kvp.Value, AveReplaceProcessor.UrlReplace(kvp.Value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl), kvp.Key, kvp.Value.Length);
                        }
                        Type t = tempWebPart.GetType();
                        PropertyInfo pi = t.GetProperty("Xml", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (pi != null)
                        {
                            XmlElement XML = pi.GetValue(tempWebPart, null) as XmlElement;
                            if (XML != null)
                            {
                                XML.InnerText = sb.ToString();
                                pi.SetValue(tempWebPart, XML, null);
                            }
                        }
                    }
                }
                return base.DoUpateAfterAdd(webPartInfo);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateWebpartInfoError, e.ToString());
                return false;
            }
        }

        public static Dictionary<int, string> GetTagList(string sHtmlText, string tag, string attr)
        {
            Regex regImg = new Regex(@"<" + tag + @"\b[^<>]*?\b" + attr.Trim() + @"[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            MatchCollection matches = regImg.Matches(sHtmlText);

            SortedDictionary<int, string> TagList = new SortedDictionary<int, string>();

            foreach (Match match in matches)
            {
                TagList.Add(match.Groups["imgUrl"].Index, match.Groups["imgUrl"].Value.TrimEnd(new char[] { '/' }));
            }
            Dictionary<int, string> tempTagList = new Dictionary<int, string>();
            List<int> keyList = new List<int>();
            foreach (KeyValuePair<int, string> kvp in TagList)
            {
                keyList.Add(kvp.Key);
            }
            for (int i = keyList.Count - 1; i >= 0; i--)
            {
                tempTagList.Add(keyList[i], TagList[keyList[i]]);
            }
            return tempTagList;
        }
    }

    class AveContentEditorWebPartUpdater : SpecialWebPartUpdater
    {
        public AveContentEditorWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            ContentEditorWebPart webPart = mWebPart as ContentEditorWebPart;
            if (webPart.Content != null)
            {
                XmlElement xe = webPart.Content;
                XmlElement tmpXE = ReplaceContentLinks(xe, webPartInfo);
                webPart.Content = tmpXE;
            }
            if (webPart.ContentLink != null) // add this for replace the ContentLink  url
            {
                webPart.ContentLink = AveReplaceProcessor.UrlReplace(webPart.ContentLink, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
        private XmlElement ReplaceContentLinks(XmlElement xe, AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                foreach (XmlNode node in xe.GetElementsByTagName("a"))
                {
                    string value = node.Attributes["href"] == null ? string.Empty : node.Attributes["href"].Value;
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    node.Attributes["href"].Value = AveReplaceProcessor.UrlReplace(value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                }
                foreach (XmlNode node in xe.GetElementsByTagName("img"))
                {
                    string value = node.Attributes["src"] == null ? string.Empty : node.Attributes["src"].Value;
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    node.Attributes["src"].Value = AveReplaceProcessor.UrlReplace(value, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                }
                foreach (XmlNode node in xe.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.CDATA)
                    {
                        string innerText = node.InnerText.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&apos;", "'").Replace("&quot;", "\"");
                        innerText = AveReplaceProcessor.ReplaceStringLinks(innerText, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                        if (innerText.IndexOf(mAveWebPart.Manager.Web.Site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            //"/sites/webpart/_layouts/listform.aspx?PageType=8&amp;ListId={BA13A879-4854-4F87-AE5A-9CCDDB2B775E}&amp;RootFolder="
                            Guid oldListId = Guid.Empty;
                            string newInnerText = AveReplaceProcessor.ReplaceStringListId(innerText, mAveWebPart.Manager.Cache.SiteMappingManager.GetListIdMappingForWebPart(), out oldListId);
                            if (oldListId != Guid.Empty)
                            {
                                mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, oldListId, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                            }
                            else
                            {
                                node.InnerText = newInnerText;
                            }
                        }
                        else
                        {
                            node.InnerText = innerText;
                        }
                    }
                }
                return xe;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ContentReplaceFailed, string.Empty, ex);
                return xe;
            }
        }
    }

    class AveContentBySearchWebPartUpdater : SpecialWebPartUpdater
    {
        public AveContentBySearchWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                Microsoft.Office.Server.Search.WebControls.ContentBySearchWebPart contentBySearch = this.mWebPart as Microsoft.Office.Server.Search.WebControls.ContentBySearchWebPart;
                string dataProviderJSON = contentBySearch.DataProviderJSON;
                if (!string.IsNullOrEmpty(dataProviderJSON))
                {
                    dataProviderJSON = AveContentBySearchWebPartUtility.UpdateDataProviderJsonProperty(dataProviderJSON, mAveWebPart.Manager.Cache);
                    contentBySearch.DataProviderJSON = dataProviderJSON;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to update ContentBySearchWebPart. Error:{0}", ex);
                return false;
            }
            return true;
        }

    }
    #region 还原WebPart之前已经进行Check了，这里面不需要进行替换了
    //class AvePictureLibrarySlideshowWebPartUpdater : SpecialWebPartUpdater
    //{
    //    public AvePictureLibrarySlideshowWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
    //        : base(webPart, aveWebPart)
    //    { }

    //    public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
    //    {
    //        PictureLibrarySlideshowWebPart webPart = mWebPart as PictureLibrarySlideshowWebPart;
    //        Guid listId = webPart.LibraryGuid;
    //        IAveList tmpList = null;
    //        //the default view doesn't have a version id
    //        if (listId != Guid.Empty)
    //        {
    //            Guid destListId = Guid.Empty;
    //            if (mAveWebPart.Manager.Cache.ListIdMapping.ContainsKey(listId))
    //            {
    //                destListId = mAveWebPart.Manager.Cache.ListIdMapping[listId];
    //                needAddPostAction = false;
    //            }
    //            else if (webPartInfo != null && webPartInfo.ExtensionProperties != null && webPartInfo.ExtensionProperties.ContainsKey("ExtensionLibraryTitle"))
    //            {
    //                string listTitle = webPartInfo.ExtensionProperties["ExtensionLibraryTitle"];
    //                try
    //                {
    //                    // NeedReloadList: 使用List["Title"]取list的时候会重新load web下的lists，会导致web下的list和缓存的list不一致，需要重新load一遍。
    //                    NeedReloadList = true;
    //                    tmpList = mAveWebPart.Manager.Web.Lists[listTitle];
    //                    destListId = tmpList.ID;
    //                    needAddPostAction = false;
    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.Log(AveLogLevel.DEBUG, "Cannot get the list by title. List Title: {0}. Error: {1}", listTitle, ex.ToString());
    //                }
    //            }
    //            if (needAddPostAction)
    //            {
    //                //ADO-22039
    //                webPartInfo.IsCreated = true;
    //                webPartInfo.WebPartIdProperty = webPart.ID;
    //                webPart.ViewGuid = webPartInfo.ViewGuid;
    //                mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, listId, mAveWebPart.Manager.File.ServerRelativeUrl, webPartInfo);
    //                return false;
    //            }
    //            webPart.LibraryGuid = destListId;

    //            if (tmpList == null)
    //            {
    //                tmpList = mAveWebPart.Manager.Web.Lists[destListId];
    //            }
    //            Guid sourceViewGuid = webPartInfo.ViewGuid;
    //            bool resetToDefaultView = false;
    //            if (mAveWebPart.Manager.Cache.ViewGuidMapping.ContainsKey(sourceViewGuid))
    //            {
    //                webPart.ViewGuid = mAveWebPart.Manager.Cache.ViewGuidMapping[sourceViewGuid];
    //            }
    //            else if (webPartInfo != null && webPartInfo.ExtensionProperties != null && webPartInfo.ExtensionProperties.ContainsKey("ExtensionViewTitle"))
    //            {
    //                string viewTitle = webPartInfo.ExtensionProperties["ExtensionViewTitle"];
    //                try
    //                {
    //                    webPart.ViewGuid = tmpList.Views[viewTitle].ID;
    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.Log(AveLogLevel.DEBUG, "Can not get the view by title: {0}. Error: {1}", viewTitle, ex.ToString());
    //                    resetToDefaultView = true;
    //                }
    //            }
    //            if (resetToDefaultView)
    //            {
    //                webPart.ViewGuid = tmpList.DefaultView.ID;
    //            }
    //        }
    //        return base.DoUpateAfterAdd(webPartInfo);
    //    }
    //}
    #endregion

    class AveKPIListWebPartUpdater : SpecialWebPartUpdater
    {
        public AveKPIListWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }
        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                Type objType = mWebPart.GetType();
                PropertyInfo property = objType.GetProperty("ListURL", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                string listURL = Convert.ToString(property.GetValue(mWebPart, null));
                //在AveLImitedWebPartManager.RestoreWebParts方法中，已经调用FakeHttpContext，Save KPIListWebPart的时候，HttpContext不能为null，里面会有引用
                if (!string.IsNullOrEmpty(listURL))
                {
                    listURL = AveReplaceProcessor.UrlReplace(listURL, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    property.SetValue(mWebPart, listURL, null);
                }
                return base.DoUpateAfterAdd(webPartInfo);
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.KPIWebPartUpdateFailed, e);
                return false;
            }
        }
    }

    class AveWhereaboutsWebPartUpdater : SpecialWebPartUpdater
    {
        public AveWhereaboutsWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
            : base(webPart, aveWebPart)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            WhereaboutsWebPart webPart = mWebPart as WhereaboutsWebPart;
            bool needContinue;
            #region EventListId
            webPart.EventListId = DoAddInPostAction(webPart.EventListId, out needContinue);
            if (!needContinue)
            {
                return false;
            }
            #endregion

            #region CallTrackingListId
            webPart.CallTrackingListId = DoAddInPostAction(webPart.CallTrackingListId, out needContinue);
            if (!needContinue)
            {
                return false;
            }
            #endregion

            return base.DoUpateAfterAdd(webPartInfo);
        }

        private Guid DoAddInPostAction(Guid oldSourceId, out bool needContinue)
        {
            Guid tempId;
            Guid destListId = oldSourceId;
            needContinue = true;
            var mappingManager = mAveWebPart.Manager.Cache.SiteMappingManager;
            var listIdInCache = mappingManager.GetValueFromListIdMapping(oldSourceId, out tempId);
            var listIdInDestCache = mappingManager.ListIdMappingContainsValue(oldSourceId);
            if (!listIdInCache && !listIdInDestCache)
            {
                mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, oldSourceId, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                needContinue = false;
            }
            else if (!listIdInDestCache)
            {
                destListId = tempId;
            }
            return destListId;
        }
    }

    class AveMediaWebPartUpdater : SpecialWebPartUpdater
    {
        public AveMediaWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
            : base(webPart, aveWebPart)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            MediaWebPart webPart = mWebPart as MediaWebPart;
            string oldUrl = string.Empty;
            if (webPart.MediaSource != null)
            {
                oldUrl = webPart.MediaSource;
                webPart.MediaSource = AveReplaceProcessor.UrlReplace(oldUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            }
            if (webPart.PreviewImageSource != null)
            {
                oldUrl = webPart.PreviewImageSource;
                webPart.PreviewImageSource = AveReplaceProcessor.UrlReplace(oldUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            }
            if (webPart.TemplateSource != null)
            {
                oldUrl = webPart.TemplateSource;
                webPart.TemplateSource = AveReplaceProcessor.UrlReplace(oldUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveContentByQueryWebPartUpdater : SpecialWebPartUpdater
    {
        public AveContentByQueryWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special URL")]
        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            const string siteCollectionPrefix = "~sitecollection";
            ContentByQueryWebPart webPart = mWebPart as ContentByQueryWebPart;
            if (!string.IsNullOrEmpty(webPart.ListGuid))
            {
                Guid destListId;
                if (this.mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(new Guid(webPart.ListGuid), out destListId))
                {
                    webPart.ListGuid = destListId.ToString();
                }
                else
                {
                    bool needPost = true;
                    if (!string.IsNullOrEmpty(webPart.ListName) && !string.IsNullOrEmpty(webPart.WebUrl))
                    {
                        string webUrl = webPart.WebUrl;
                        if (webUrl.StartsWith(siteCollectionPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            webUrl = mAveWebPart.Manager.Cache.SourceSiteInfo.ServerRelativeUrl + webUrl.Substring(15);
                            if (webUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                            {
                                webUrl = webUrl.Substring(1);
                            }
                        }
                        webUrl = AveReplaceProcessor.UrlReplace(webUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                        string listName = webPart.ListName;
                        Guid ListGuid = Guid.Empty;
                        try
                        {
                            using (IAveWeb tWeb = this.mAveWebPart.Manager.File.Web.Site.OpenWeb(webUrl))
                            {
                                // NeedReloadList: 使用List["Title"]取list的时候会重新load web下的lists，会导致web下的list和缓存的list不一致，需要重新load一遍。
                                NeedReloadList = true;
                                ListGuid = tWeb.Lists[listName].ID;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, "Cannot get list Ids by list title: {0} from the Web: {1}, Exception: {2}", listName, webUrl, ex.ToString());
                        }
                        if (ListGuid != Guid.Empty)
                        {
                            webPart.ListGuid = ListGuid.ToString();
                            needPost = false;
                        }
                    }
                    if (needPost)
                    {
                        mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, new Guid(webPart.ListGuid), mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                        return false;
                    }
                }
            }
            var propertiesValues = new List<string>()
            {
                webPart.FilterDisplayValue1,
                webPart.FilterDisplayValue2,
                webPart.FilterDisplayValue3,
                webPart.FilterValue1,
                webPart.FilterValue2,
                webPart.FilterValue3,
            };

            var setProperties = new List<Action<string>>()
            {
                (value)=>webPart.FilterDisplayValue1 = value,
                (value)=>webPart.FilterDisplayValue2 = value,
                (value)=>webPart.FilterDisplayValue3 = value,
                (value)=>webPart.FilterValue1 = value,
                (value)=>webPart.FilterValue2 = value,
                (value)=>webPart.FilterValue3 = value,
            };

            Regex regex = new Regex(AveRegexCommon.GUIDREG, RegexOptions.IgnoreCase);

            MatchEvaluator replacer = (match) =>
                {
                    var id = new Guid(match.Value);
                    if (mAveWebPart.Manager.Cache.TermIdMapping.ContainsKey(id))
                    {
                        return mAveWebPart.Manager.Cache.TermIdMapping[id].ToString();
                    }
                    return match.Value;
                };

            for (int i = 0; i < propertiesValues.Count; ++i)
            {
                string value = propertiesValues[i];
                if (!string.IsNullOrEmpty(value) && value.Length >= 36)
                {
                    string replacedValue = regex.Replace(value, replacer);
                    if (!value.Equals(replacedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        setProperties[i](replacedValue);
                    }
                }
            }

            if (!string.IsNullOrEmpty(webPart.WebUrl))
            {
                if (!webPart.WebUrl.StartsWith(siteCollectionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    webPart.WebUrl = AveReplaceProcessor.UrlReplace(webPart.WebUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                }
                else
                {
                    string webServerRelativeUrl = string.Empty;
                    string siteServerRelativeUrl = mAveWebPart.Manager.Cache.SourceSiteInfo.ServerRelativeUrl;//mAveWebPart.Manager.Web.Site.ServerRelativeUrl;
                    if (webPart.WebUrl == siteCollectionPrefix)
                    {
                        if (siteServerRelativeUrl == "/")
                        {
                            webServerRelativeUrl = "/";
                        }
                        else
                        {
                            webServerRelativeUrl = siteServerRelativeUrl;
                        }
                    }
                    else if (webPart.WebUrl.StartsWith(siteCollectionPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (siteServerRelativeUrl == "/")
                        {
                            webServerRelativeUrl = webPart.WebUrl.Substring(15);
                        }
                        else
                        {
                            webServerRelativeUrl = siteServerRelativeUrl + webPart.WebUrl.Substring(15);
                        }
                    }
                    else { }

                    string destWebUrl = AveReplaceProcessor.UrlReplace(webServerRelativeUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);

                    if (mAveWebPart.Manager.Web.Site.ServerRelativeUrl == "/")
                    {
                        if (destWebUrl == "/")
                        {
                            destWebUrl = string.Empty;//root sitecollection, root web 
                        }
                    }
                    else
                    {
                        if (destWebUrl.Equals(mAveWebPart.Manager.Web.Site.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            destWebUrl = string.Empty;// root web 
                        }
                        else
                        {
                            destWebUrl = destWebUrl.Substring(mAveWebPart.Manager.Web.Site.ServerRelativeUrl.Length);
                        }
                    }

                    webPart.WebUrl = siteCollectionPrefix + destWebUrl;
                }
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveSummaryLinkWebPartUpdater : SpecialWebPartUpdater
    {
        public AveSummaryLinkWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }
        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            SummaryLinkWebPart webPart = mWebPart as SummaryLinkWebPart;
            if (!string.IsNullOrEmpty(webPart.SummaryLinkStore))
            {
                string value = AveReplaceProcessor.ReplaceUrlInXml(webPart.SummaryLinkStore, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                webPart.SummaryLinkValue = new Microsoft.SharePoint.Publishing.Fields.SummaryLinkFieldValue(value);
                webPart.SummaryLinkStore = value;
            }
            if (webPart.ManagedLinks != null)
            {
                for (int i = 0; i < webPart.ManagedLinks.Count; i++)
                {
                    string link = webPart.ManagedLinks[i] as string;
                    if (!string.IsNullOrEmpty(link))
                    {
                        webPart.ManagedLinks[i] = AveReplaceProcessor.UrlReplace(link, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    }
                }
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveSPUserCodeWebPartUpdater : SpecialWebPartUpdater
    {
        public AveSPUserCodeWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
            : base(webPart, aveWebPart)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            SPUserCodeWebPart webPart = mWebPart as SPUserCodeWebPart;
            webPart.AssemblyFullName = webPartInfo.Assembly;
            webPart.SolutionId = webPartInfo.SolutionId;
            webPart.TypeFullName = webPartInfo.Class;
            RestoreUserCodeWebPartProperties(webPart, webPartInfo);
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        private void RestoreUserCodeWebPartProperties(SPUserCodeWebPart spUserCodeWebPart, AveWebPartBaseInfo webPartInfo)
        {
            foreach (KeyValuePair<string, object> property in mAveWebPart.WebPartProperties)
            {
                if (string.Equals(property.Key, "Title", StringComparison.OrdinalIgnoreCase))
                {
                    spUserCodeWebPart.Title = property.Value.ToString();
                }
                var sPUserCodeProperty = new SPUserCodeProperty { Name = property.Key, Value = property.Value.ToString() };
                if (!spUserCodeWebPart.Properties.Contains(sPUserCodeProperty))
                {
                    if (sPUserCodeProperty.Name.EndsWith("Link", StringComparison.OrdinalIgnoreCase)
                        || sPUserCodeProperty.Name.EndsWith("URL", StringComparison.OrdinalIgnoreCase)
                        || sPUserCodeProperty.Name.EndsWith("Path", StringComparison.OrdinalIgnoreCase)
                        || sPUserCodeProperty.Name.Equals("FormLocation", StringComparison.OrdinalIgnoreCase)
                        || string.Equals("PartImageLarge", sPUserCodeProperty.Name, StringComparison.OrdinalIgnoreCase)
                        || string.Equals("PartImageSmall", sPUserCodeProperty.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (sPUserCodeProperty.Value.Contains("/"))
                        {
                            sPUserCodeProperty.Value = AveReplaceProcessor.UrlReplace(sPUserCodeProperty.Value, this.mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), this.mAveWebPart.Manager.Cache.SourceSiteInfo, this.mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                        }
                    }
                    spUserCodeWebPart.Properties.Add(sPUserCodeProperty);

                }
            }
        }
    }
    class AveChartWebPartUpdater : SpecialWebPartUpdater
    {
        private static readonly AveLogger Log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public AveChartWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
            : base(webPart, aveWebPart)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            Type webpartType = mWebPart.GetType();
            IList dataBindings = AveAssemblyUtility.GetPropertyValue(mWebPart, "DataBindings") as IList;
            foreach (object dataBinding in dataBindings)
            {
                try
                {
                    object dataSource = AveAssemblyUtility.GetPropertyValue(dataBinding, "DataSource");
                    if (dataSource != null)
                    {
                        switch (dataSource.GetType().Name)
                        {
                            case "DataSourceWebList":
                                string webName = AveAssemblyUtility.GetPropertyValue(dataSource, "SiteName") as string;
                                string listTitle = AveAssemblyUtility.GetPropertyValue(dataSource, "ListTitle") as string;
                                string listUrl = AveAssemblyUtility.GetPropertyValue(dataSource, "ListUrl") as string;
                                string dataProviderPageUrl = AveAssemblyUtility.GetPropertyValue(dataSource, "DataProviderPageUrl") as string;
                                listUrl = AveReplaceProcessor.UrlReplace(listUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                                AveAssemblyUtility.SetPropertyValue(dataSource, "SiteName", AveReplaceProcessor.UrlReplace(webName, mAveWebPart.Manager.Cache.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl));
                                AveAssemblyUtility.SetPropertyValue(dataSource, "DataProviderPageUrl", AveReplaceProcessor.UrlReplace(dataProviderPageUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl));
                                AveAssemblyUtility.SetPropertyValue(dataSource, "ListTitle", AveReplaceProcessor.UrlReplace(listTitle, mAveWebPart.Manager.Cache.SiteMappingManager.GetListUrlMappingForWebPart(), new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl));
                                AveAssemblyUtility.SetPropertyValue(dataSource, "ListUrl", listUrl);
                                AveAssemblyUtility.SetPropertyValue(mWebPart, "ListUrl", listUrl);
                                break;
                            case "DataSourceExcelService":
                                ChartWebPart chartWebPart = mWebPart as ChartWebPart;
                                DataSourceExcelService dataSourceExcelService = dataSource as DataSourceExcelService;
                                Microsoft.Office.Server.Internal.Charting.Data.ExcelWebService service = new Microsoft.Office.Server.Internal.Charting.Data.ExcelWebService();
                                string serviceUrl = AveAssemblyUtility.GetPropertyValue(dataSource, "ServiceUrl") as string;
                                string workbookUrl = AveAssemblyUtility.GetPropertyValue(dataSource, "WorkbookUrl") as string;
                                string rangeName = AveAssemblyUtility.GetPropertyValue(dataSource, "RangeName") as string;
                                bool useFirstRowAsColumnNames = AveAssemblyUtility.GetPropertyValue(dataSource, "UseFirstRowAsColumnNames").ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
                                bool useExcelFormatting = AveAssemblyUtility.GetPropertyValue(dataSource, "UseExcelFormatting").ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
                                //如果源端和目的端是在不同的web app下的时候，因为这个webpart的property是直接全部复制过去的，这时从dataSource获取的workbookUrl的HostHeader已经变成目的端的了，所以需要替换一下，然后在进行Url替换。
                                string sourceHostHeader = AveReplaceProcessor.GetHostHeader(mAveWebPart.Manager.Cache.SourceSiteInfo.Url);
                                string destHostHeader = AveReplaceProcessor.GetHostHeader(mAveWebPart.Manager.Cache.DestSiteInfo.Url);
                                workbookUrl = workbookUrl.Replace(destHostHeader, sourceHostHeader);
                                workbookUrl = AveReplaceProcessor.UrlReplace(workbookUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                                //如果新的workbookurl所指向的文件不存在的话，这句话就会抛出异常（limitation）
                                string sessionId = string.Empty;
                                try
                                {
                                    sessionId = service.OpenWorkbook(workbookUrl, Thread.CurrentThread.CurrentUICulture.Name, Thread.CurrentThread.CurrentUICulture.Name);
                                }
                                catch (TargetInvocationException ex)
                                {
                                    mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, Guid.Empty, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                                    Log.Info(string.Format("Cannot open the target workbook. URL: {0}. Exception: {1}", workbookUrl, ex.ToString()));
                                    return false;
                                }
                                try
                                {
                                    service.GetRangeA1(sessionId, string.Empty, rangeName, useExcelFormatting);
                                }
                                finally
                                {
                                    if (!string.IsNullOrEmpty(sessionId))
                                    {
                                        service.CloseWorkbook(sessionId);
                                    }
                                }
                                chartWebPart.WorkBookUrl = "";
                                dataSourceExcelService.ServiceUrl = AveReplaceProcessor.UrlReplace(serviceUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                                dataSourceExcelService.WorkbookUrl = workbookUrl;
                                dataSourceExcelService.UseFirstRowAsColumnNames = useFirstRowAsColumnNames;
                                dataSourceExcelService.UseExcelFormatting = useExcelFormatting;
                                AveAssemblyUtility.SetPropertyValue(dataBinding, "DataSource", dataSourceExcelService);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(string.Format("An error occurred while updating chart web part, title: {0}, ID: {1}, Error: {2}", webPartInfo.TitleUrl, webPartInfo.ID, e));
                }
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveVisioWebAccessUpdater : SpecialWebPartUpdater
    {
        public AveVisioWebAccessUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            VisioWebAccess webpart = mWebPart as VisioWebAccess;
            webpart.DiagramPath = AveReplaceProcessor.UrlReplace(
                webpart.DiagramPath, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true),
                mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            return base.DoUpateAfterAdd(webPartInfo);

        }
    }

    class AveWikiContentWebpartUpdater : SpecialWebPartUpdater
    {
        public AveWikiContentWebpartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }
        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            WikiContentWebpart webPart = mWebPart as WikiContentWebpart;
            List<string> attributesNeedReplace = new List<string>() { "ImageUrl", "PostBackUrl" };
            webPart.Content = AveReplaceProcessor.ReplaceAspContent(webPart.Content, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true),
                mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl, attributesNeedReplace);
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }


    /// <summary>
    /// ado-24257中遇到了simpleformwebpart的content中url没有替换的问题，因此加这个特殊处理。
    /// </summary>
    class AveSimpleFormWebPartUpdater : SpecialWebPartUpdater
    {
        public AveSimpleFormWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }
        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            SimpleFormWebPart webPart = mWebPart as SimpleFormWebPart;
            webPart.Content = AveReplaceProcessor.ReplaceStringLinks(webPart.Content, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveSiteDocumentsUpdater : SpecialWebPartUpdater
    {
        public AveSiteDocumentsUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                Microsoft.SharePoint.Portal.WebControls.SiteDocuments documents = this.mWebPart as Microsoft.SharePoint.Portal.WebControls.SiteDocuments;
                if (documents != null && !string.IsNullOrEmpty(documents.UserTabs))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Microsoft.SharePoint.Portal.WebControls.SerializableTab>));
                    List<Microsoft.SharePoint.Portal.WebControls.SerializableTab> tabs = (List<Microsoft.SharePoint.Portal.WebControls.SerializableTab>)serializer.Deserialize(new StringReader(documents.UserTabs));
                    if (tabs.Count > 0)
                    {
                        foreach (Microsoft.SharePoint.Portal.WebControls.SerializableTab tab in tabs)
                        {
                            string url = tab.Pair.UrlPersist;
                            tab.Pair.UrlPersist = AveReplaceProcessor.UrlReplace(url, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                        }

                        StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                        serializer.Serialize((TextWriter)writer, tabs);
                        documents.UserTabs = writer.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while trying to updater Site Documents Web Part tabs property. Exception: {0}", ex.ToString());
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveProjectSummaryWebPartUpdater : SpecialWebPartUpdater
    {
        public AveProjectSummaryWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart projectSummaryWebPart = mWebPart as Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart;

            if (!string.IsNullOrEmpty(projectSummaryWebPart.PrimaryTaskListUrl))
            {
                string primaryUrl = projectSummaryWebPart.PrimaryTaskListUrl;
                projectSummaryWebPart.PrimaryTaskListUrl = AveReplaceProcessor.UrlReplace(primaryUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            }
            Guid destListId;
            if (mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(projectSummaryWebPart.ListId, out destListId))
            {
                projectSummaryWebPart.ListId = destListId;
                needPostAction = false;
            }
            else if (webPartInfo != null && webPartInfo.ExtensionProperties != null && webPartInfo.ExtensionProperties.ContainsKey("ExtensionListTitle"))
            {
                string listTitle = webPartInfo.ExtensionProperties["ExtensionListTitle"];
                try
                {
                    // NeedReloadList: 使用List["Title"]取list的时候会重新load web下的lists，会导致web下的list和缓存的list不一致，需要重新load一遍。
                    Guid listGuid = mAveWebPart.GetListIdByTitle(mAveWebPart.Manager.Web.ID, listTitle);
                    if (listGuid != Guid.Empty)
                    {
                        projectSummaryWebPart.ListId = listGuid;
                        needPostAction = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, "Cannot get the list by title. List Title: {0}. Error: {1}", listTitle, ex);
                }
            }
            if (needPostAction)
            {
                mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, projectSummaryWebPart.ListId, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                return false;
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart projectSummaryWebPart = mWebPart as Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart;
            projectSummaryWebPart.Panels = new List<Microsoft.SharePoint.Portal.WebControls.ProjectSummaryPanelConfig>();
            object panels;
            if (mAveWebPart.WebPartProperties.TryGetValue("Panels", out panels) && panels != null)
            {
                projectSummaryWebPart.Panels = panels as List<Microsoft.SharePoint.Portal.WebControls.ProjectSummaryPanelConfig>;
            }
            return base.DoUpateBeforeAdd(webPartInfo);
        }
    }

    class AveSPTimelineWebPartUpdater : SpecialWebPartUpdater
    {
        public AveSPTimelineWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            SPTimelineWebPart spTimelineWebPart = mWebPart as SPTimelineWebPart;
            Guid listId = Guid.Empty;

            if (mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(new Guid(spTimelineWebPart.ListId), out listId))
            {
                needPostAction = false;
            }
            else if (webPartInfo != null && webPartInfo.ExtensionProperties != null && webPartInfo.ExtensionProperties.ContainsKey("ExtensionListTitle"))
            {
                string listTitle = webPartInfo.ExtensionProperties["ExtensionListTitle"];
                try
                {
                    listId = mAveWebPart.GetListIdByTitle(mAveWebPart.Manager.Web.ID, listTitle);
                    if (listId == Guid.Empty)
                    {
                        needPostAction = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, "Cannot get the list by title. List Title: {0}. Error: {1}", listTitle, ex);
                }
            }
            if (needPostAction)
            {
                mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, new Guid(spTimelineWebPart.ListId), mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                return false;
            }
            else
            {
                spTimelineWebPart.ListId = listId.ToString();
                spTimelineWebPart.SourceSelection = listId.ToString();
                spTimelineWebPart.CurrentTaskListWebAddress = AveReplaceProcessor.UrlReplace(spTimelineWebPart.CurrentTaskListWebAddress, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                spTimelineWebPart.PageAddress = AveReplaceProcessor.UrlReplace(spTimelineWebPart.PageAddress, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveRSSAggregatorWebPartUpdater : SpecialWebPartUpdater
    {
        public AveRSSAggregatorWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                Microsoft.SharePoint.Portal.WebControls.RSSAggregatorWebPart rssAggregator = this.mWebPart as Microsoft.SharePoint.Portal.WebControls.RSSAggregatorWebPart;
                if (rssAggregator != null)
                {
                    string feedUrl = rssAggregator.FeedUrl;
                    if (!string.IsNullOrEmpty(feedUrl))
                    {
                        rssAggregator.FeedUrl = AveReplaceProcessor.UrlReplace(feedUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true, true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while trying to updater RSS View Web Part Feed Url property. Exception: {0}", ex.ToString());
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }

    class AveClientWebPartUpdater : SpecialWebPartUpdater
    {
        public AveClientWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateBeforeAdd(AveWebPartBaseInfo webPartInfo)
        {
            var clientWebPart = this.mWebPart as ClientWebPart;
            UpdateClientWebPartProperty(clientWebPart, webPartInfo);
            return base.DoUpateBeforeAdd(webPartInfo);
        }

        private void UpdateClientWebPartProperty(ClientWebPart clientWebPart, AveWebPartBaseInfo webPartInfo)
        {
            if (clientWebPart == null || string.IsNullOrEmpty(webPartInfo.DefinitionXml))
            {
                return;
            }
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(webPartInfo.DefinitionXml);
            XmlNode xNodeFeatureId = xmlDocument.SelectSingleNode(".//*[@name = 'FeatureId']");
            if (xNodeFeatureId != null && !string.IsNullOrEmpty(xNodeFeatureId.InnerText))
            {
                clientWebPart.FeatureId = new Guid(xNodeFeatureId.InnerText);
            }

            XmlNode xNodeSolutionWebId = xmlDocument.SelectSingleNode(".//*[@name = 'ProductWebId']");
            if (xNodeSolutionWebId != null && !string.IsNullOrEmpty(xNodeSolutionWebId.InnerText))
            {
                var originalWebId = new Guid(xNodeSolutionWebId.InnerText);
                var destWebId = Guid.Empty;
                if (mAveWebPart.Manager.Cache.SiteMappingManager.WebIDMapping.TryGetValue(originalWebId, out destWebId))
                {
                    clientWebPart.ProductWebId = destWebId;
                }
                else if (clientWebPart.ProductWebId.Equals(Guid.Empty) && !originalWebId.Equals(Guid.Empty))
                {
                    clientWebPart.ProductWebId = originalWebId;
                }
            }
            XmlNode xNodeWebPartName = xmlDocument.SelectSingleNode(".//*[@name = 'WebPartName']");
            if (xNodeWebPartName != null && !string.IsNullOrEmpty(xNodeWebPartName.InnerText))
            {
                clientWebPart.WebPartName = xNodeWebPartName.InnerText;
            }
        }
    }

    class AveTermPropertyUpdater : SpecialWebPartUpdater
    {
        public AveTermPropertyUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc)
        { }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {
            try
            {
                TermProperty termPropertyWebPart = this.mWebPart as TermProperty;
                Guid termStoreID = termPropertyWebPart.TermStoreID;
                Guid destTermStoreID = Guid.Empty;
                Guid termSetID = termPropertyWebPart.TermSetID;
                Guid destTermSetID = Guid.Empty;
                Guid termID = termPropertyWebPart.TermID;
                Guid destTermID = Guid.Empty;
                if (mAveWebPart.Manager.Cache.TermStoreIdMapping.TryGetValue(termStoreID, out destTermStoreID)
                    && termStoreID != destTermStoreID)
                {
                    termPropertyWebPart.TermStoreID = destTermStoreID;
                }
                if (mAveWebPart.Manager.Cache.TermSetIdMapping.TryGetValue(termSetID, out destTermSetID)
                    && termSetID != destTermSetID)
                {
                    termPropertyWebPart.TermSetID = destTermSetID;
                }
                if (mAveWebPart.Manager.Cache.TermIdMapping.TryGetValue(termID, out destTermID)
                    && termID != destTermID)
                {
                    termPropertyWebPart.TermID = destTermID;
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while trying to updater Term Property Web Part property. Exception: {0}", ex);
            }
            return base.DoUpateAfterAdd(webPartInfo);
        }
    }
    #endregion
}
