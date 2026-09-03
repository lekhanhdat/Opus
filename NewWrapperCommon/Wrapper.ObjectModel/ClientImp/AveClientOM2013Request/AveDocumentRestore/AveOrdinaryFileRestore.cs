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
using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using ClientFile = Microsoft.SharePoint.Client.File;
using AvePoint.Wrapper.Resource.Client;
using System.IO;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Restore;
using System.Globalization;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveOrdinaryFileRestore : BaseDocumentRestore
    {
        public AveOrdinaryFileRestore(AveClientContext context, AveClientOM2013Request request, object authentication, AveDocumentInfo docInfo, Stream fileStream)
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
                    File = Add();
                    addStream = false;               
                    RestoreResult = RestoreResult.AddNew;
                    UnlockFile(File);
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

        protected ClientFile Add()
        {
            bool forceCheckout = CheckForceCheckout();
            ListMemento.SetListSetting(true, DocInfo.OriginalVersion % 512 != 0, false, forceCheckout);
            ClientFile newFile = AddFile(true);
            if(!LoadFileInfo(newFile))
            {
                // ADO-154648 local07和local10在有video ContentType的list下的video document还原到365下结构有改变，要修改url。
                var fileExtension = Path.GetExtension(FileServerRelativeUrl);
                if (needProcessVideoExtension.Contains(fileExtension.ToLower(CultureInfo.InvariantCulture)) &&
                    ContainsVideoContentType())
                {
                    FileServerRelativeUrl = GetNewFileServerRelativeUrlForVideo(FileServerRelativeUrl);
                    newFile = ParentWeb.GetFileByServerRelativeUrl(FileServerRelativeUrl);
                    LoadFileInfo(newFile);
                }
            }
            return newFile;
        }

        protected override void PrepareRestore()
        {
            if (DocInfo.ListId == Guid.Empty)
            {
                throw new Exception("List does not exist.");
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

        protected override bool NeedDeleteByFileType(bool exist)
        {
            string reportingGalleryTemplateId = AveWeb.AllProperties.ContainsKey("_reportinggallerytemplateid") ? AveWeb.AllProperties["_reportinggallerytemplateid"] as string : null;
            if (DocInfo.ListId.ToString().Equals(reportingGalleryTemplateId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!exist || !DocInfo.HasStream || !DocInfo.SettingInfo.DELETE_ITEM)
            {
                return false;
            }
            return true;
        }

        protected override bool TryDeleteFile()
        {
            CancleWebWelcomePage();
            DeactiveSolution(File);
            return base.TryDeleteFile();
        }

        //Deactive solution
        private void DeactiveSolution(ClientFile file)
        {
            if (ParentList.BaseTemplate != 121 ||
                file.ListItemAllFields.FieldValues.Count <= 0)
            {
                return;
            }
            object status = -1;
            if (file.ListItemAllFields.FieldValues.TryGetValue("Status", out status) &&
                status != null &&
                ((FieldLookupValue)status).LookupValue.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                this.Request.OperateSolution("DEA", Context.Url, DocInfo.ParentWebRelativeUrl, file.ListItemAllFields.Id);
            }
        }
    }
}
