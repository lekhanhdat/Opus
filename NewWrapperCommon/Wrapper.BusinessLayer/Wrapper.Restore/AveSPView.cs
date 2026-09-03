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
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPView : AvePoint.Wrapper.Restore.IAveSPView
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IAveList mSPList;
        private AveSPList mAveList;

        private string mViewUrl;

        public string ViewUrl
        {
            get { return mViewUrl; }
        }

        private Dictionary<Guid, Guid> mViews = new Dictionary<Guid, Guid>();

        public Dictionary<Guid, Guid> Views
        {
            get { return mViews; }
        }

        private uint mFlags;
        //private int mUserId;

        //private AveWebPartBaseInfo m_MWebPartInfo;


        public AveSPView(AveSPList list)
        {
            mAveList = list;
            mSPList = mAveList.SPList;

            //this.mSqlCon = mAveList.SqlConn;

        }

        internal static AveViewType GetViewType(int viewType)
        {
            AveViewType enumViewType;
            if ((viewType & 0x4000000) == 0x4000000)
            {
                enumViewType = AveViewType.Gantt;
            }
            else if ((viewType & 0x80000) == 0x80000)
            {
                enumViewType = AveViewType.Calendar;
            }
            else if ((viewType & 0x20000) == 0x20000)
            {
                enumViewType = AveViewType.Chart;
            }
            else if ((viewType & 0x800) == 0x800)
            {
                enumViewType = AveViewType.Grid;
            }
            else if ((viewType & 0x1) == 0x1)
            {
                enumViewType = AveViewType.Html;
            }
            else
                enumViewType = AveViewType.None;
            return enumViewType;
        }

        internal static AveViewType GetViewType(string viewType)
        {
            return GetViewType(AveViewInfo.GetViewType(viewType));
        }

        internal static AveViewType GetViewType(object viewType)
        {
            int intViewType;
            if (Int32.TryParse(viewType.ToString(), out intViewType))
            {
                return GetViewType(intViewType);
            }
            return GetViewType(viewType.ToString());
        }

        public void SetFlags(IAveView view, uint value)
        {
            this.mFlags = value;
            //this.PersonalView = (mFlags & 0x40000) != 0;

            //if ((mFlags & 0x80000) != 0)
            //{
            //    this.Type = AveViewType.Calendar;
            //}
            //else if ((mFlags & 0x20000) != 0)
            //{
            //    this.Type = AveViewType.Chart;
            //}
            //else if ((mFlags & 0x4000000) != 0)
            //{
            //    this.Type = AveViewType.Gantt;
            //}
            //else if ((mFlags & 1) != 0)
            //{
            //    this.Type = AveViewType.Grid;
            //}
            //else if ((mFlags & 0x2001) != 0)
            //{
            //    this.Type = AveViewType.Recurrence;
            //}

            if ((mFlags & 0x1000) != 0)
            {
                if ((mFlags & 0x200000) != 0)
                {
                    view.Scope = (AveViewScope)1;
                }
                else
                {
                    view.Scope = (AveViewScope)2;
                }
            }
            else if ((mFlags & 0x200000) != 0)
            {
                view.Scope = (AveViewScope)3;
            }
            else
            {
                view.Scope = (AveViewScope)0;
            }

            view.IncludeRootFolder = (mFlags & 0x8000000) != 0;
            view.Hidden = (mFlags & 8) != 0;
            view.DefaultViewForContentType = (mFlags & 0x10000000) != 0;
            view.EditorModified = (mFlags & 2) != 0;
            try
            {
                if (view.MobileDefaultView != ((mFlags & 0x1000000) != 0))
                {
                    view.MobileDefaultView = (mFlags & 0x1000000) != 0;
                }
                if (view.MobileView != ((mFlags & 0x800000) != 0))
                {
                    view.MobileView = (mFlags & 0x800000) != 0;
                }
            }
            catch (AveException e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetViewFieldFailed, view.Title, e);
            }
        }

        private void Init(AveWebPartBaseInfo webPartInfo, IAveView view)
        {
            //mUserId = webPartInfo.UserID;
            //this.m_MWebPartInfo = webPartInfo;
            this.SetFlags(view, (uint)webPartInfo.Flags);

            view.DefaultView = webPartInfo.Type == 0;

            if (webPartInfo.View != null)
            {
                ParseSchemaXml(view, AveCompressedUtility.GetTCompressedString(webPartInfo.View));
            }
        }

        private void ParseSchemaXml(IAveView view, string xml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPView.ParseSchemaXml"))
            {

                if (String.IsNullOrEmpty(xml))
                {
                    return;
                }

                xml = "<View>" + xml + "</View>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                foreach (XmlElement element in doc.DocumentElement.ChildElements())
                {
                    switch (element.Name)
                    {
                        case "Query":
                            view.Query = element.InnerXml;
                            break;
                        case "ViewFields":
                            view.ViewFields.RemoveAll();
                            foreach (XmlElement fieldRef in element.ChildElements())
                            {
                                //AveFieldInfo field = new AveFieldInfo();
                                //field.Name = fieldRef.GetAttribute("Name");
                                view.ViewFields.Add(fieldRef.GetAttribute("Name"));
                            }
                            break;
                        case "RowLimit":
                            view.RowLimit = Convert.ToUInt32(element.InnerText);
                            if (element.HasAttribute("Paged"))
                            {
                                view.Paged = Convert.ToBoolean(element.GetAttribute("Paged"));
                            }
                            break;
                        case "RowLimitExceeded":
                            view.RowLimitExceeded = element.InnerXml;
                            break;
                        case "Formats":
                            view.Formats = element.InnerXml;
                            break;
                        case "ViewStyle":
                            view.ApplyStyle(mSPList.ParentWeb.ViewStyles[int.Parse(element.GetAttribute("ID"))]);
                            //mSPView.StyleID = element.GetAttribute("ID");
                            break;
                        case "GroupByFooter":
                            view.GroupByFooter = element.InnerXml;
                            break;
                        case "GroupByHeader":
                            view.GroupByHeader = element.InnerXml;
                            break;
                        case "Aggregations":
                            view.Aggregations = element.InnerXml;
                            if (element.HasAttribute("Value"))
                            {
                                view.AggregationsStatus = element.GetAttribute("Value");
                            }
                            break;
                        case "OpenApplicationExtension":
                            view.OpenApplicationExtension = element.InnerXml;
                            break;
                        case "ViewData":
                            view.ViewData = element.InnerXml;
                            break;
                        case "ViewBody":
                            view.ViewBody = element.InnerXml;
                            break;
                        case "ViewEmpty":
                            view.ViewEmpty = element.InnerXml;
                            break;
                        case "ViewFooter":
                            view.ViewFooter = element.InnerXml;
                            break;
                        case "ViewHeader":
                            view.ViewHeader = element.InnerXml;
                            break;
                        case "Toolbar":
                            view.Toolbar = element.InnerXml;
                            //Toolbar Type
                            break;
                        case "ParameterBindings":
                            view.ParameterBindings = element.InnerXml;
                            break;
                        case "Joins":
                            view.Joins = element.InnerXml;
                            break;
                        case "InlineEdit":
                            view.InlineEdit = element.InnerXml;
                            break;
                        case "XslLink":
                            view.XslLink = element.InnerXml;
                            break;
                        case "Xsl":
                            view.Xsl = element.InnerXml;
                            break;
                        default:
                            break;
                    }
                }

            }

        }
        public void RestoreViewProperties(AveWebPartBaseInfo webPartInfo, IAveView view)
        {
            try
            {
                Init(webPartInfo, view);

                view.Update();
            }
            catch (Exception ex)
            {
                //mLog.Log(AveLogLevel.WARN, string.Format("An error occurred when restore view of web part.\n error message:{0}", ex));
                log.Warn("An error occurred when restore view of web part.", ex);
            }
        }
    }
}
