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
using Microsoft.SharePoint.WebPartPages;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    class AveSPViewWebPart : AveWebPart
    {
        SPView view;
        private SPList parentList;
        internal SPView View { get { return view; } }

        public AveSPViewWebPart(AveLimitedWebPartManager manager, WebPart spWebPart)
            : base(manager, spWebPart, -1)
        {
            this.parentList = (manager.File.ParentFolder.ParentList as AveList).List;
            this.isViewWebPart = true;
        }

        public AveSPViewWebPart(AveLimitedWebPartManager manager)
            : base(manager)
        {
            this.parentList = (manager.File.ParentFolder.ParentList as AveList).List;
            this.isViewWebPart = true;
        }

        public override bool RealRestore()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveSPViewWebPart.RealRestore"))
            {
                bool result = false;
                try
                {
                    if (!VerifyWebPartTypeId())
                    {
                        return false;
                    }

                    EnsureAssemblyInfo();

                    if (!VerifyWebPartData())
                    {
                        AddUnRestoreWebPartInfo(manager.Web.ID, webPartBaseInfo.ListId, manager.File.ServerRelativeUrl, webPartBaseInfo);
                        return false;
                    }
                    //remove to VerifyWebPartData()
                    //如果存在UserID，判断其属于 persional view.
                    //isShared = webPartBaseInfo.UserID <= 0;
                    try
                    {
                        if (!isShared)
                        {
                            view = manager.GetPersonalView(this.parentList, this.Manager.Cache.ViewInfo.Views[webPartBaseInfo.ID], destUserId);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.PersonalViewWebPartGetError, e);
                    }

                    //这里可能有两种情况　１．PersonalView第一次还原的时候　２．不是PersonalView的时候
                    if (view == null)
                    {
                        view = this.parentList.Views[manager.Cache.ViewInfo.Views[webPartBaseInfo.ID]];
                    }
                    if (view != null)
                    {
                        if (!VerifyBaseViewID(this.parentList))
                        {
                            logger.Warn("Source base View ID is not equal with view base View ID. view title: {0}", view.Title);
                            return false;
                        }
                        //对于ViewWebPart，理论上不会返回False，但是函数里面会替换View信息
                        if (!EnsureViewInfo(this.parentList))
                        {
                            return false;
                        }

                        if (!this.Manager.HasFullControlPermission)
                        {
                            InternalRestoreViewWebPart();
                            bool reload = RealRestoreCore();
                            result |= reload;
                            return result;
                        }
                        this.webPartId = view.ID;
                        this.internalWebPart = manager.GetWebPart(this.webPartId, isShared);
                    }
                    else
                    {
                        logger.Warn("Can't get this view, view ID: {0}, view Title: {1}", webPartBaseInfo.ID, webPartBaseInfo.DisplayName);
                        mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotGetViewByID, webPartBaseInfo.ID, webPartBaseInfo.DisplayName));
                        return false;
                    }

                    bool needReload = RealRestoreCore();
                    result |= needReload;

                    UpdateView();

                    //PersonalView已经是用对应的User获取View，不需要在去修改User Id了
                    //if (!isShared)
                    //{
                    //    UpdateUserID(this.webPartId, destUserId, false);
                    //}
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while restoring web part {0}, error: {1}", assemblyName + "|" + webPartType, ex.ToString());
                    mReport.AddDetail(new AveWrapperWebpartReportDto(webPartBaseInfo.DisplayName, webPartBaseInfo.DisplayName, webPartBaseInfo, assemblyName, webPartType, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreWebPartError, assemblyName + "|" + webPartType, ex.Message));
                }
                return result;
            }
        }

        private void InternalRestoreViewWebPart()
        {
            XmlDocument sourceViewDoc = GenerateViewXml();
            view.SetViewXml(sourceViewDoc.FirstChild.OuterXml);
            view.Update();
            this.webPartId = view.ID;
            this.internalWebPart = manager.GetWebPart(this.webPartId, isShared);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "RecurrenceRowset is a property of SPView")]
        private XmlDocument GenerateViewXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement viewXml = doc.CreateElement("View");
            viewXml.SetAttribute("Name", view.ID.ToString());
            viewXml.SetAttribute("Url", view.Url);
            PAGETYPE pageType = PAGETYPE.PAGE_NORMALVIEW; //value is 1
            if(webPartBaseInfo.Type.HasValue)
            {
                pageType = (PAGETYPE)Convert.ToInt32(webPartBaseInfo.Type.Value);
            }
            viewXml.SetAttribute("DefaultView", (pageType == PAGETYPE.PAGE_DEFAULTVIEW).ToString());
            viewXml.SetAttribute("Flags", webPartBaseInfo.Flags.ToString());
            string viewType = GetViewTypeFromFlags(webPartBaseInfo.Flags);
            if (viewType != null)
            {
                viewXml.SetAttribute("Type", viewType);
            }
            viewXml.SetAttribute("Hidden", GetBoolValueFromFlags(webPartBaseInfo.Flags, 8).ToString());
            viewXml.SetAttribute("Threaded", GetBoolValueFromFlags(webPartBaseInfo.Flags, 65536).ToString());
            viewXml.SetAttribute("FPModified", GetBoolValueFromFlags(webPartBaseInfo.Flags, 2).ToString());
            viewXml.SetAttribute("ReadOnly", GetBoolValueFromFlags(webPartBaseInfo.Flags, 32).ToString());
            SPViewScope scope = GetViewScopeFromFlags(webPartBaseInfo.Flags);
            if (scope != SPViewScope.Default)
            {
                viewXml.SetAttribute("Scope", scope.ToString());
            }
            viewXml.SetAttribute("RecurrenceRowset", GetBoolValueFromFlags(webPartBaseInfo.Flags, 8192).ToString());
            viewXml.SetAttribute("ModerationType", GetModerationTypeFromFlags(webPartBaseInfo.Flags));
            viewXml.SetAttribute("Personal", GetBoolValueFromFlags(webPartBaseInfo.Flags, 262144).ToString());
            viewXml.SetAttribute("OrderedView", GetBoolValueFromFlags(webPartBaseInfo.Flags, 4194304).ToString());
            viewXml.SetAttribute("MobileView", GetBoolValueFromFlags(webPartBaseInfo.Flags, 8388608).ToString());
            viewXml.SetAttribute("MobileDefaultView", GetBoolValueFromFlags(webPartBaseInfo.Flags, 16777216).ToString());
            viewXml.SetAttribute("DefaultViewForContentType", GetBoolValueFromFlags(webPartBaseInfo.Flags, 268435456).ToString());
            viewXml.SetAttribute("HackLockWeb", GetBoolValueFromFlags(webPartBaseInfo.Flags, 16).ToString());
            viewXml.SetAttribute("FailIfEmpty", GetBoolValueFromFlags(webPartBaseInfo.Flags, 64).ToString());
            viewXml.SetAttribute("FreeForm", GetBoolValueFromFlags(webPartBaseInfo.Flags, 128).ToString());
            viewXml.SetAttribute("FileDialog", GetBoolValueFromFlags(webPartBaseInfo.Flags, 256).ToString());
            viewXml.SetAttribute("TabularView", GetBoolValueFromFlags(webPartBaseInfo.Flags, 4).ToString());
            viewXml.SetAttribute("AggregateView", GetBoolValueFromFlags(webPartBaseInfo.Flags, 1024).ToString());
            viewXml.SetAttribute("IncludeRootFolder", GetBoolValueFromFlags(webPartBaseInfo.Flags, 134217728).ToString());
            viewXml.SetAttribute("IncludeVersions", GetBoolValueFromFlags(webPartBaseInfo.Flags, 33554432).ToString());
            if (!string.IsNullOrEmpty(webPartBaseInfo.DisplayName))
            {
                viewXml.SetAttribute("DisplayName", webPartBaseInfo.DisplayName);
            }
            if (CheckViewContentType(webPartBaseInfo, this.parentList))
            {
                StringBuilder builder = new StringBuilder();
                foreach (byte b in webPartBaseInfo.ContentTypeId)
                {
                    builder.AppendFormat("{0:x2}", b);
                }
                viewXml.SetAttribute("ContentTypeID", "0x" + builder.ToString());
            }
            if (webPartBaseInfo.BaseViewID.HasValue)
            {
                viewXml.SetAttribute("BaseViewID", webPartBaseInfo.BaseViewID.Value.ToString());
            }
            string viewString = string.Empty;
            if (webPartBaseInfo.View != null)
            {
                viewString = AveCompressedUtility.GetTCompressedString(webPartBaseInfo.View);
            }
            if (!string.IsNullOrEmpty(viewString))
            {
                doc.LoadXml("<ViewFields>" + viewString + "</ViewFields>");
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    XmlNode cloneNode = node.Clone();
                    viewXml.AppendChild(cloneNode);
                }
                doc.RemoveAll();
            }
            doc.AppendChild(viewXml);

            return doc;
        }

        protected override bool CheckListId()
        {
            if (isIListWebPart)
            {
                EnsureWebPartProperties();
                if (webPartBaseInfo.WebPartList == null)
                {
                    webPartBaseInfo.WebPartList = new List<AveWebPartListInfo>();
                    webPartBaseInfo.WebPartList.Add(new AveWebPartListInfo());
                }
                webPartBaseInfo.WebPartList[0].WebId = this.manager.Web.ID;
                this.webPartBaseInfo.ListId = this.parentList.ID;
                if (WebPartProperties.ContainsKey("WebId"))
                {
                    WebPartProperties["WebId"] = this.manager.Web.ID;
                }
                if (WebPartProperties.ContainsKey("ListId"))
                {
                    WebPartProperties["ListId"] = this.parentList.ID;
                }
                if (WebPartProperties.ContainsKey("ListName"))
                {
                    WebPartProperties["ListName"] = this.parentList.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                }
            }
            return true;
        }

        /// <summary>
        /// View WebPart需要处理ContentTypeId
        /// </summary>
        /// <param name="webPartInfo"></param>
        /// <param name="webPartId"></param>
        protected override void UpdateView()
        {
            bool needUpdateContentType = CheckViewContentType(webPartBaseInfo, this.parentList);

            int baseViewID = webPartBaseInfo.BaseViewID.HasValue ? Convert.ToInt32(webPartBaseInfo.BaseViewID.Value) : -1;
            //对于view webpart，display name已经在还原view时做了替换,此时更新只需要用view上的Title属性即可，由于有view title mapping的存在，如果用备份的信息则可能不对
            string displayName = webPartBaseInfo.DisplayName;
            if (displayName != null)
            {
                //以$Resources 开头的是Title Resource的Resx Id，可以直接更新，不需要做处理（local 备份是此种情况）
                //365备份的是当前语言的title，会走mapping,所以更新时需要根据还原过的view的title来赋值
                //if (!displayName.StartsWith("$Resources", StringComparison.Ordinal))       ADO-209292 
                //{
                if (view != null && !string.IsNullOrEmpty(view.Title))
                {
                    displayName = view.Title;
                }
                //}
            }

            if (needUpdateContentType)
            {
                Manager.UpdateView(this.webPartId, Manager.Web.Site.ID, Manager.File.UniqueId, baseViewID, webPartBaseInfo.View, webPartBaseInfo.ContentTypeId, displayName);
            }
            else
            {
                Manager.UpdateView(this.webPartId, Manager.Web.Site.ID, Manager.File.UniqueId, baseViewID, webPartBaseInfo.View, null, displayName);
            }
        }

        private string GetViewTypeFromFlags(int flags)
        {
            string result = null;
            if ((flags & 2048) != 0)
            {
                result = SPViewCollection.SPViewTypeToString(SPViewCollection.SPViewType.Grid);
            }
            else
            {
                if ((flags & 131072) != 0)
                {
                    result = SPViewCollection.SPViewTypeToString(SPViewCollection.SPViewType.Chart);
                }
                else
                {
                    if ((flags & 524288) != 0)
                    {
                        result = SPViewCollection.SPViewTypeToString(SPViewCollection.SPViewType.Calendar);
                    }
                    else
                    {
                        if ((flags & 67108864) != 0)
                        {
                            result = SPViewCollection.SPViewTypeToString(SPViewCollection.SPViewType.Gantt);
                        }
                        else
                        {
                            if ((flags & 8193) != 0)
                            {
                                result = SPViewCollection.SPViewTypeToString(SPViewCollection.SPViewType.Recurrence);
                            }
                            else
                            {
                                if ((flags & 1) != 0)
                                {
                                    result = SPViewCollection.SPViewTypeToString(SPViewCollection.SPViewType.Html);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private SPViewScope GetViewScopeFromFlags(int flags)
        {
            if (GetBoolValueFromFlags(flags, 4096) && GetBoolValueFromFlags(flags, 2097152))
            {
                return SPViewScope.Recursive;
            }
            if (GetBoolValueFromFlags(flags, 4096) && !GetBoolValueFromFlags(flags, 2097152))
            {
                return SPViewScope.RecursiveAll;
            }
            if (!GetBoolValueFromFlags(flags, 4096) && GetBoolValueFromFlags(flags, 2097152))
            {
                return SPViewScope.FilesOnly;
            }

            return SPViewScope.Default;
        }

        private string GetModerationTypeFromFlags(int flags)
        {
            if (GetBoolValueFromFlags(flags, 16384))
            {
                return "Contributor";
            }
            if (GetBoolValueFromFlags(flags, 32768))
            {
                return "Moderator";
            }
            return string.Empty;
        }

        private bool GetBoolValueFromFlags(int flags, int value)
        {
            return (flags & value) != 0;
        }

        protected override void ReplaceViewFieldsString(XmlDocument xDoc, IAveFieldMapping fieldMapping, SPList list, ref bool change, ref bool needPostRestore)
        {
            base.ReplaceViewFieldsString(xDoc, fieldMapping, list, ref change, ref needPostRestore);
            try
            {
                if (xDoc.GetElementsByTagName("CalendarSettings").Count > 0)
                {
                    Guid webId = manager.Web.ID;
                    Guid listId = manager.File.ParentFolder.ParentListId;
                    Guid viewId = view.ID;
                    manager.AddToNeedResetCalendarSettingsViews(webId, listId, viewId);
                }
                //[ADO-55829]10-13 "InlineEdit" in 13theme site not exsits and migrate this option will cause an error, so we not migrate it.
                if (view.ParentList.ParentWeb.Site.CompatibilityLevel == 15 && xDoc.GetElementsByTagName("InlineEdit").Count > 0)
                {
                    XmlElement rootElement = (XmlElement)xDoc.GetElementsByTagName("root")[0];
                    rootElement.RemoveChild(xDoc.GetElementsByTagName("InlineEdit")[0]);
                    change = true;
                }
                #region[ADO-55505] Fist GroupBy&Then GroupBy can be same, thus we need't delete Then GroupBy
                //if (xDoc.GetElementsByTagName("GroupBy").Count > 0)
                //{
                //    XmlNode groupNode = xDoc.GetElementsByTagName("GroupBy")[0];
                //    if (groupNode.ChildNodes.Count > 1)
                //    {
                //        string firGp = groupNode.ChildNodes[0].Attributes["Name"].Value;
                //        string secGp = groupNode.ChildNodes[1].Attributes["Name"].Value;
                //        if (!string.IsNullOrEmpty(firGp) && firGp.Equals(secGp, StringComparison.OrdinalIgnoreCase))
                //        {
                //            groupNode.RemoveChild(groupNode.ChildNodes[1]);
                //        }
                //    }
                //}
                #endregion
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while checking view web part field mappings. Error: {0}", e.ToString());
            }
        }
        
    }
}
