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
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using ClientFile = Microsoft.SharePoint.Client.File;
using AveClientRequest.Common;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using System.IO;
using AvePoint.Wrapper.Restore;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveO365SystemFileRestore : AveO365BaseDocumentRestore
    {
        public AveO365SystemFileRestore(AveClientContext context, AveClientOMOffice365Request request, FederationToken tokenProvider, AveDocumentInfo docInfo, Stream fileStream) :
            base(context, request, tokenProvider, docInfo, fileStream)
        { }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "property key")]
        public override Dictionary<string, object> Restore()
        {
            PrepareRestore();
            var restoreResult = new Dictionary<string, object>();

            if (this.DocInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                var parentFolder = this.AveWeb.GetFolder(DocInfo.ParentFolderRelativeUrl);
                if (parentFolder != null &&
                    parentFolder.Exists &&
                    parentFolder.Properties.ContainsKey("_ipfs_infopathenabled") &&
                    ((string)parentFolder.Properties["_ipfs_infopathenabled"]).Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    // ADO-128220 InfoPath sharepoint list view的WebPart不能还原出来，需要使用web service的模拟InfoPath的publish创建出来。skip view的还原。
                    //throw new AveRestoreException(AveRestoreResult.Omit, "Sharepoint InfoPath view WebPart will be created by publishing the list");
                    return restoreResult;
                }
            }

            var path = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
            ClientFile file = ParentWeb.GetFileByServerRelativePath(path);
            bool exist = LoadFileInfo(file);
            if (exist)
            {
                restoreResult["ConflictWithDocument"] = true;
            }
            //Check OverWrite By Last Modified.
            if (NeedSkipByLastModifiedTime(file, exist))
            {
                RestoreResult = RestoreResult.SkippedByLastModifiedTime;
            }
            //Skip.
            if (exist && !IsNewCreated && !DocInfo.SettingInfo.DELETE_ITEM && DocInfo.RestoreOption == AveRestoreMode.Default)
            {
                RestoreResult = RestoreResult.SkipConflict;
            }
            if (RestoreResult != RestoreResult.None)
            {
                GenerateItemProperties(file, restoreResult);
                return restoreResult;
            }
            if (DocInfo.SettingInfo.DELETE_ITEM || !exist)
            {
                RestoreSystemFile(ref file, exist);
            }
            SetPropertiesForXSN();
            SetProperties();
            GenerateItemProperties(file, restoreResult);
            return restoreResult;
        }

        protected void RestoreSystemFile(ref ClientFile file, bool exist)
        {
            bool needReload = false;
            if (!exist &&
                DocInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) &&
                DocInfo.OriginalPageStatus != AveCustomizedPageStatus.None &&
                this.ParentList != null)
            {
                file = AddTemplateFile((int)TemplateFileType.FormPage);
                RestoreResult = RestoreResult.AddNew;
                exist = true;
            }
            if (!exist || (GhostPageNeedAddStream(file)) && (DocInfo.HasStream || FileStream.Length > 0))
            {
                //If not Ghost Page or Ghost Page's stream has been changed, Save Stream. e.g. /forms/template.dotx
                file = AddFile(true);
                LoadFileInfo(file, false);
                RestoreResult = RestoreResult.AddNew;
            }
            RestoreWebParts(file);
        }

        protected bool GhostPageNeedAddStream(ClientFile file)
        {
            if (file.CustomizedPageStatus == CustomizedPageStatus.Uncustomized &&
                DocInfo.OriginalPageStatus == AveCustomizedPageStatus.Uncustomized)
            {
                return false;
            }
            return true;
        }

        protected override void PrepareRestore()
        {
            base.PrepareRestore();
            IsNewCreated = DocInfo.RestoringItem.IsNewItem;
            FileServerRelativeUrl = AveUrlUtility.CombineUrl(DocInfo.ParentFolderRelativeUrl, DocInfo.Name);
            ParentWeb = Context.Site.OpenWeb(DocInfo.ParentWebRelativeUrl);
            if (DocInfo.ListId == Guid.Empty)
            {
                return;
            }
            ParentList = ParentWeb.Lists.GetById(DocInfo.ListId);
            Context.Load(ParentList);
            Context.Load(ParentList, l => l.BaseTemplate);
        }

        private void SetProperties()
        {
            if (DocInfo.MetaInfoDic != null && DocInfo.MetaInfoDic.ContainsKey("ContentTypeId"))
            {
                var properties = new Dictionary<string, object>();
                var values = new Dictionary<string, object>();
                values.Add("ContentTypeId", DocInfo.MetaInfoDic["ContentTypeId"]);
                properties.Add("ChangedMetaInfo", values);
                Request.UpdateFile(DocInfo.ParentWebRelativeUrl, DocInfo.ParentListTitle, DocInfo.ServerRelativeUrl, properties);
            }
        }


    }
}
