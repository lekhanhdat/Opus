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
using System.IO;
using System.Linq;
using System.Text;
using AveClientRequest.Common;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Application;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.WebParts;
using ClientFile = Microsoft.SharePoint.Client.File;
using AvePoint.Wrapper.Resource.Client;
using System.Threading;
using System.Net;
using AvePoint.Wrapper.Restore;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveMasterPageDocumentRestore : BaseDocumentRestore
    {
        public AveMasterPageDocumentRestore(AveClientContext context, AveClientOM2013Request request, object authentication, AveDocumentInfo docInfo, Stream fileStream)
            : base(context, request, authentication, docInfo, fileStream) { }

        public override Dictionary<string, object> Restore()
        {
            PrepareRestore();
            var restoreResult = new Dictionary<string, object>();
            try
            {
                bool exist = ProcessConflictResolution(restoreResult);
                AddOrUpdate(ref exist);
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
                return;
            }
            base.PrepareRestore();
            IsNewCreated = DocInfo.RestoringItem.IsNewItem;
            FileServerRelativeUrl = AveUrlUtility.CombineUrl(DocInfo.ParentFolderRelativeUrl, DocInfo.Name);

            ParentWeb = Context.Site.OpenWeb(DocInfo.ParentWebRelativeUrl);
            ParentList = ParentWeb.Lists.GetById(DocInfo.ListId);
            Context.Load(ParentList);
            Context.Load(ParentList, l => l.BaseTemplate);
            ListMemento = new AveListMemento(ParentList);
            //Do not need execute here.Execute when LoadFileInfo.
        }

        private bool CanDeleteInMasterPage()
        {
            //If overwrite, documents in master page gallery which has no content should be overwrite.
            return DocInfo.Name.Equals("PeopleSearchResults.aspx", StringComparison.OrdinalIgnoreCase) ||
                   DocInfo.Name.Equals("SearchResults.aspx", StringComparison.OrdinalIgnoreCase);
        }



        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "startermaster.html is a part of url")]
        private bool ReadOnlyMasterPage()
        {
            //Some master page can not be deleted, renamed , edited.
            if (string.Equals(DocInfo.Name, "startermaster.html", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        protected override bool NeedDeleteByFileType(bool exist)
        {
            return exist &&
                DocInfo.SettingInfo.DELETE_ITEM
                && (DocInfo.HasStream || CanDeleteInMasterPage())
                && GhostPageNeedAddStream(File) && !ReadOnlyMasterPage();
        }

        private bool GhostPageNeedAddStream(ClientFile file)
        {
            if (file.CustomizedPageStatus == CustomizedPageStatus.Uncustomized &&
                DocInfo.OriginalPageStatus == AveCustomizedPageStatus.Uncustomized)
            {
                return false;
            }
            return true;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = ".js is the extension of a kind of document")]
        protected override bool CheckFileLockStatus(ClientFile file)
        {
            if (ReadOnlyMasterPage() || CheckFileLockStatusInMetaInfo(file))
            {
                throw new RestoreResultException(RestoreResult.SkipConflict, string.Format("Skip to restore this file. This file may not be moved, deleted, renamed, or otherwise edited. File URL:{0}", FileServerRelativeUrl));
            }
            bool exist = false;
            try
            {
                if (!FileServerRelativeUrl.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                Log.Log(AveLogLevel.DEBUG, "Check file lock status by finding html file. File Url:{0}", FileServerRelativeUrl);
                string htmlFileUrl = ReplaceExtention(FileServerRelativeUrl, ".html");
                ClientFile htmlFile = ParentWeb.GetFileByServerRelativeUrl(htmlFileUrl);
                Context.Load(htmlFile, f => f.Exists);

                string htmFileUrl = ReplaceExtention(FileServerRelativeUrl, ".htm");
                ClientFile htmFile = ParentWeb.GetFileByServerRelativeUrl(htmFileUrl);
                Context.Load(htmFile, f => f.Exists);
                Context.ExecuteQuery();
                exist = htmlFile.Exists || htmFile.Exists;
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.WARN, "Failed to check file that in master page library lock status. File Url:{0}. Error:{1}", FileServerRelativeUrl, ex.ToString());
            }
            if (exist)
            {
                throw new RestoreResultException(RestoreResult.SkipConflict, string.Format("Skip to restore this file. This file may not be moved, deleted, renamed, or otherwise edited. File URL:{0}", FileServerRelativeUrl));
            }
            return exist;
        }

        private string ReplaceExtention(string value, string newExtension)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            int num = value.LastIndexOf('.');
            string result;
            if (num == -1)
            {
                result = value + "." + newExtension;
            }
            else
            {
                result = value.Substring(0, num) + newExtension;
            }
            return result;
        }

        private bool CheckFileLockStatusInMetaInfo(ClientFile file)
        {
            string metaInfo = TryGetMetaInfo(file);
            if (string.IsNullOrEmpty(metaInfo))
            {
                return false;
            }
            var metaDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfo);
            return metaDic.ContainsKey("HtmlDesignLockedFile");
        }

        private string TryGetMetaInfo(ClientFile file)
        {
            try
            {
                return file.ListItemAllFields["MetaInfo"].ToString();
            }
            catch (Exception ex)
            {
                Log.Log(AveLogLevel.DEBUG, "Failed to get file MetaInfo, error:{0}.", ex);
                return null;
            }
        }

        private void AddOrUpdate(ref bool exist)
        {
            bool addStream = true;
            if (!exist)
            {
                File = Add();
                addStream = false;
                exist = true;
                RestoreResult = RestoreResult.AddNew;
            }
            UpdateVersion(File, addStream);
        }

        protected override void UpdateVersion(ClientFile file, bool addStream, bool needIncrease = true)
        {
            //local -> Office365 subsite will add this file and then update version
            if (ReadOnlyMasterPage())
            {
                throw new RestoreResultException(RestoreResult.SkipConflict, string.Format("Skip to restore this file. This file may not be moved, deleted, renamed, or otherwise edited. File URL:{0}", FileServerRelativeUrl));
            }
            base.UpdateVersion(file, addStream, needIncrease);
        }

        private ClientFile Add()
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
            ClientFile newFile = AddFile(true);
            LoadFileInfo(newFile);
            return newFile;
        }

        protected override ClientFile AddFileAndKeepVersion(bool overwriteIfExists, ClientFile file)
        {
            if (this.AveWeb.Template.Equals("SRCHCENTERLITE#0", StringComparison.OrdinalIgnoreCase))
            {
                return file;
            }
            return base.AddFileAndKeepVersion(overwriteIfExists, file);
        }
    }
}
