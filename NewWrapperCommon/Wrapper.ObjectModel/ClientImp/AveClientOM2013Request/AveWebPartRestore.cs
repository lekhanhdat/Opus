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
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using AvePoint.GCommon;
using System.Xml;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2013WebPartRestore : AveWebPartRestore, IDisposable
    {
        public Ave2013WebPartRestore(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, bool clearAll, ClientContext context, AveWebPartCache mapping, IAveWeb web,IReport report, object obj)
            : base(webServerRelativeUrl, listTitle, listId, fileServerRelativeUrl, scope, clearAll, context, mapping, web,report, obj)
        { }
        public Ave2013WebPartRestore(ClientContext context, IAveWeb cachedWeb, Web web, List list, File page, LimitedWebPartManager limitedWebPartManager, ListItem item, AveWebPartCache mapping, IReport report, object obj)
            : base(context, cachedWeb, web, list, page, limitedWebPartManager, item, mapping, report, obj)
        { }

        public void RestoreWebPartsOnly(List<AveWebPartBaseInfo> webpartBaseInfoList)
        {
            if (!NeedSkipRestoreWebPart())
            {
                if (webpartBaseInfoList == null)
                {
                    return;
                }
                View view;
                WebPartDefinition webPartDef;
                DeleteAllWebParts(out view, out webPartDef);
                if (webpartBaseInfoList.Count <= 0)
                {
                    return;
                }
                foreach (AveWebPartBaseInfo webpartInfo in webpartBaseInfoList)
                {
                    InternalRestoreWebPart(webpartInfo, view, webPartDef);
                }
            }
        }
        protected override void UpdateCalendarSettings(View view, AveXmlView xmlView)
        {
            XmlDocument viewDocument = new XmlDocument();
            viewDocument.LoadXml(view.ListViewXml);
            var node = viewDocument.CreateElement("CalendarSettings");
            node.InnerXml = xmlView.CalendarSettings;
            viewDocument.DocumentElement.InsertBefore(node, viewDocument.DocumentElement.FirstChild);
            var content = viewDocument.DocumentElement.InnerXml;
            view.ListViewXml = content;
            mMapping.SiteMappingManager.AddToNeedResetCalendarSettingsViews(mCachedWeb.ID, mListId, view.Id);
        }

        public override void RestoreWebParts(List<AveWebPartBaseInfo> webpartBaseInfoList)
        {
            if (!NeedSkipRestoreWebPart())
            {
                base.RestoreWebParts(webpartBaseInfoList);
            }
        }

        // 真实365中EditForm.aspx, DispForm.aspx和Upload.aspx这3个view，在没有AddAndCustomizePages权限的时候要skip WebPart还原。
        // 真实365中Category.aspx的webpart涉及到关联discussion broad的问题，需要skip webpart 还原。
        private bool NeedSkipRestoreWebPart()
        {
            if (string.IsNullOrEmpty(mFileServerRelativeUrl))
            {
                var message = "Skip restoring WebPart, the page's server relative url is null or empty.";
                Logger.Warn(message);

                mReport.AddDetail(new AveWrapperWebpartReportDto("WebPart", "WebPart", null, string.Empty, string.Empty, AveStatus.Skipped, message));
                return true;
            }

            bool isNeedCheckSkipPageFile = IsNeedCheckSkipPageFile();
            if (isNeedCheckSkipPageFile && !mCachedWeb.HaveAddAndCustomizePagesPermission)
            {
                // TODO: add restore to upper module
                var errorMessage = string.Format("Skip restoring webpart, because the user does not have AddAndCustomizePages permission. Please check user permissions. File server relative url: {0}.", mFileServerRelativeUrl);
                Logger.Warn(errorMessage);

                mReport.AddDetail(new AveWrapperWebpartReportDto("WebPart", "WebPart", null, string.Empty, string.Empty, AveStatus.Skipped, errorMessage));
                return true;
            }

            if(IsNeedCheckSkipCategoryPageFile())
            {
                var errorMessage = string.Format("Skip restoring webpart, because it could make page unavailable. Page url: {0}.", mFileServerRelativeUrl);
                Logger.Warn(errorMessage);

                mReport.AddDetail(new AveWrapperWebpartReportDto("WebPart", "WebPart", null, string.Empty, string.Empty, AveStatus.Skipped, errorMessage));
                return true;
            }
            
            return false;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "name for specify document.")]
        private bool IsNeedCheckSkipPageFile()
        {
            if (mFileServerRelativeUrl.EndsWith("/Forms/EditForm.aspx", StringComparison.OrdinalIgnoreCase) ||
                mFileServerRelativeUrl.EndsWith("/Forms/DispForm.aspx", StringComparison.OrdinalIgnoreCase) ||
                mFileServerRelativeUrl.EndsWith("/Forms/Upload.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        
        private bool IsNeedCheckSkipCategoryPageFile()
        {
            if(mFileServerRelativeUrl.EndsWith("/SitePages/Category.aspx",StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        public override void UpdateWebPartProperties(WebPartDefinition webpart, XmlDocument doc)
        {
            if (webpart == null)
            {
                return;
            }
            bool changeProperties = false;
            XmlNode viewNode = doc.SelectSingleNode("//*[name()='ViewFlag']");
            if (viewNode != null && !string.IsNullOrEmpty(viewNode.InnerText))
            {
                var sourceType = (ViewType)Enum.Parse(typeof(ViewType), viewNode.InnerText);
                //ADO-163622 只要包含Grid,还原到365都需要过滤到
                if (!sourceType.HasFlag(ViewType.Grid))//ADO-162776 DataSheetView 在365中需要HTML(1)+TabularView(4)+Grid(2048)=2053,而从07备份的只有HTML(1)+Grid(2048)=2049，暂时将2049这种情况过滤，在还原view的时候已经将view flag还原正确了。
                {
                    webpart.WebPart.Properties["ViewFlags"] = sourceType;
                    changeProperties = true;
                }
            }
            XmlNode templateNameNode = doc.SelectSingleNode("//*[name()='TemplateName']");
            if (templateNameNode != null && !string.IsNullOrEmpty(templateNameNode.InnerText))
            {
                webpart.WebPart.Properties["TemplateName"] = templateNameNode.InnerText;
                changeProperties = true;
            }
            else
            {//只有修改过TemplateName DefinitionXML上才能获取到TemplateName
                XmlNode formTypeNode = doc.SelectSingleNode("//*[name()='FormType']");
                // PageType:
                // displayForm == 4
                // editForm == 6
                // newForm == 8
                if (formTypeNode != null && (formTypeNode.InnerText.Equals("4") || formTypeNode.InnerText.Equals("6") || formTypeNode.InnerText.Equals("8")))
                {
                    webpart.WebPart.Properties["TemplateName"] = null;
                    changeProperties = true;
                }
            }
            XmlNode titleNode = doc.SelectSingleNode("//*[name()='Title']");
            if(titleNode!=null && !string.IsNullOrEmpty(titleNode.InnerText))
            {
                webpart.WebPart.Properties["Title"] = titleNode.InnerText;
                changeProperties = true;
            }
            if (changeProperties)
            {
                webpart.SaveWebPartChanges();
            }
        }

        public override void CheckWebPartTitle(WebPartDefinition webPart, XmlDocument definitionXmlDoc)
        {
            XmlNode titleNode = definitionXmlDoc.SelectSingleNode("//*[name() = 'Title']");
            if (titleNode != null && !string.IsNullOrEmpty(titleNode.InnerText))
            {
                webPart.WebPart.Properties["Title"] = titleNode.InnerText;
                webPart.SaveWebPartChanges();
            }
        }
    }
}
