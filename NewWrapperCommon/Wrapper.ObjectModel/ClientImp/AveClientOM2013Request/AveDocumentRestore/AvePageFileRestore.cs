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
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClientFile = Microsoft.SharePoint.Client.File;
using Microsoft.SharePoint.Client;
using AvePoint.Wrapper.Restore;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AvePageFileRestore : BaseDocumentRestore
    {
        public AvePageFileRestore(AveClientContext context, AveClientOM2013Request request, object authentication, AveDocumentInfo docInfo, Stream fileStream)
            : base(context, request, authentication, docInfo, fileStream) { }

        public override Dictionary<string, object> Restore()
        {
            PrepareRestore();
            var restoreResult = new Dictionary<string, object>();

            try
            {
                bool exist = ProcessConflictResolution(restoreResult);

                bool addStream = true;
                if (!exist)
                {
                    File = Add(ref addStream);
                    RestoreResult = RestoreResult.AddNew;
                }
                UpdateVersion(File, addStream);
            }
            catch (RestoreResultException restoreResultException)
            {
                restoreResult.Add("RestoreMessage", restoreResultException.RestoreErrorMessage);
                RestoreResult = restoreResultException.Result;
            }

            GenerateItemProperties(File, restoreResult);

            return restoreResult;
        }

        protected override void PrepareRestore()
        {
            if (DocInfo.ListId == Guid.Empty)
            {
                throw new Exception("Page List does not exist.");
            }
            base.PrepareRestore();
            IsNewCreated = DocInfo.RestoringItem.IsNewItem;
            FileServerRelativeUrl = AveUrlUtility.CombineUrl(DocInfo.ParentFolderRelativeUrl, DocInfo.Name);

            ParentWeb = Context.Site.OpenWeb(DocInfo.ParentWebRelativeUrl);
            ParentList = ParentWeb.Lists.GetById(DocInfo.ListId);
            Context.Load(ParentList);
            Context.Load(ParentList, l => l.BaseTemplate);
            ListMemento = new AveListMemento(ParentList);
        }

        private ClientFile Add(ref bool needAddStream)
        {
            bool forceCheckout = CheckForceCheckout();
            if (DocInfo.OriginalVersion % 512 == 0)
            {
                ListMemento.SetListSetting(true, false, false, forceCheckout);
            }
            else
            {
                ListMemento.SetListSetting(true, true, false, forceCheckout);
            }
            ClientFile newFile = null;
            PageType pageType = GetPageType();
            switch (pageType)
            {
                case PageType.WikiPage:
                case PageType.ClientSidePage:
                    newFile = AddTemplateFile((int)pageType);
                    return newFile;
                case PageType.WebPartPage:
                case PageType.PublishingPage:
                case PageType.Invalid:
                default:
                    newFile = AddFile(true);
                    LoadFileInfo(newFile);
                    needAddStream = false;
                    break;
            }
            return newFile;
        }

        protected override Dictionary<string, string> RestoreWebParts(ClientFile webPartPage)
        {
            Dictionary<string, string> idMapping = base.RestoreWebParts(webPartPage);
            ReplaceWebPartIdInWikiContent(idMapping);
            return idMapping;
        }

        protected override bool NeedDeleteByFileType(bool exist)
        {
            bool needSkipBySpecialCase = false;
            needSkipBySpecialCase |= NeedSkipDeleteOperationToKeepWebPart();
            return base.NeedDeleteByFileType(exist) && !needSkipBySpecialCase;
        }

        // [ADO-170687]由于365的API中没有List.FieldIndexs，导致无法替换WebPart中的WithIndex属性，导致WebPart显示出错，所以这种情况下，不还原WebPart，所以要跳过删除的操作。
        private bool NeedSkipDeleteOperationToKeepWebPart()
        {
            bool needSkipDeleteOperation = false;
            if (DocInfo.WebParts != null)
            {
                foreach (var webPartInfo in DocInfo.WebParts)
                {
                    if (!string.IsNullOrEmpty(webPartInfo.DefinitionXml))
                    {
                        if (webPartInfo.DefinitionXml.Contains("WithIndex"))
                        {
                            needSkipDeleteOperation = true;
                            break;
                        }
                    }
                }
            }
            if (DocInfo.ServerRelativeUrl.EndsWith("/SitePages/Category.aspx", StringComparison.OrdinalIgnoreCase))
            {
                // 由于有的模块没有把restore WebPart放到和restoreSelf一起，导致这个时候check不到WebPart，所以只有直接过滤目前遇到的情况，即为Community Site下的Category.aspx
                // 如果遇到客户问题，在SitePage下有名为Category.aspx的page需要还原再考虑其他的判断条件。
                needSkipDeleteOperation =  true;
            }

            if(needSkipDeleteOperation)
            {
                // 由于不还原WebPart，所以不能还原WikiField属性，不然会导致页面WebPart不显示。
                if (DocInfo.FieldsInfo.Fields.ContainsKey("NeedSetNullFields"))
                {
                    List<string> setToNullFields = DocInfo.FieldsInfo.Fields["NeedSetNullFields"] as List<string>;
                    if(setToNullFields.Contains("WikiField"))
                    {
                        setToNullFields.Remove("WikiField");
                    }
                }

                if(DocInfo.FieldsInfo.Fields.ContainsKey("WikiField"))
                {
                    DocInfo.FieldsInfo.Fields.Remove("WikiField");
                }
            }
            return needSkipDeleteOperation;
        }

        protected override bool TryDeleteFile()
        {
            //删除Page之前需要确认当前page是否为welcome page
            CancleWebWelcomePage();
            return base.TryDeleteFile();
        }

        /// <summary>
        /// Wiki Page and Publishing Page must restore its wiki field, otherwise the page cannot display the webparts.
        /// </summary>
        private void ReplaceWebPartIdInWikiContent(Dictionary<string, string> idMapping)
        {
            if (idMapping == null || idMapping.Count <= 0)
            {
                return;
            }
            string fieldName = string.Empty;
            if (ParentList.BaseTemplate == (int)ListTemplateType.WebPageLibrary &&
                DocInfo.FieldsInfo.Fields.ContainsKey("WikiField") &&
                !string.IsNullOrEmpty(DocInfo.FieldsInfo.Fields["WikiField"] as string))
            {
                fieldName = "WikiField";
            }
            else if (ParentList.BaseTemplate == 850 &&
                    DocInfo.FieldsInfo.Fields.ContainsKey("PublishingPageContent") &&
                    !string.IsNullOrEmpty(DocInfo.FieldsInfo.Fields["PublishingPageContent"] as string))
            {
                //Publishing page.
                fieldName = "PublishingPageContent";
            }
            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }
            StringBuilder sb = new StringBuilder(DocInfo.FieldsInfo.Fields[fieldName] as string);
            foreach (KeyValuePair<string, string> webpartId in idMapping)
            {
                sb.Replace(webpartId.Key, webpartId.Value);
            }
            DocInfo.FieldsInfo.Fields[fieldName] = sb.ToString();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = @"DocumentTemplates\wkpstd.aspx")]
        private PageType GetPageType()
        {
            if (!DocInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                return PageType.Invalid;
            }
            if (DocInfo.FieldsInfo.Fields.ContainsKey("HTML_x0020_File_x0020_Type") &&
                "SharePoint.WebPartPage.Document".Equals(DocInfo.FieldsInfo.Fields["HTML_x0020_File_x0020_Type"]))
            {
                return PageType.WebPartPage;
            }
            else if (this.ParentList.BaseTemplate == (int)ListTemplateType.WebPageLibrary)
            {
                if (!string.IsNullOrEmpty(DocInfo.SetupPath) &&
                (DocInfo.SetupPath.Equals(@"Features\\GroupHomepage\\Home.aspx", StringComparison.OrdinalIgnoreCase)
                ||DocInfo.SetupPath.Equals(@"Features\\SitePagePublishing\\Home.aspx", StringComparison.OrdinalIgnoreCase)))
                {
                    return PageType.ClientSidePage;
                }
                else if (!DocInfo.HasStream || (!string.IsNullOrEmpty(DocInfo.SetupPath) &&
                DocInfo.SetupPath.Equals(@"DocumentTemplates\wkpstd.aspx", StringComparison.OrdinalIgnoreCase)))
                {
                    return PageType.WikiPage;
                } 
            }
            else if (DocInfo.FieldsInfo.Fields.ContainsKey("PublishingPageContent"))
            {
                return PageType.PublishingPage;
            }
            return PageType.Invalid;
        }
    }
}
