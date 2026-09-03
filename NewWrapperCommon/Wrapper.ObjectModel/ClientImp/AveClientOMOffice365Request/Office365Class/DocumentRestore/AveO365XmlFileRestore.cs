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
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    class AveO365XmlFileRestore : AveO365OrdinaryFileRestore
    {
        public AveO365XmlFileRestore(AveClientContext context, AveClientOMOffice365Request request, FederationToken tokenProvider, AveDocumentInfo docInfo, Stream fileStream) : 
            base(context, request, tokenProvider, docInfo, fileStream) { }

        protected override void UpdateVersion(Microsoft.SharePoint.Client.File file, bool addStream, bool needIncrease = true)
        {
            bool needReload = false;
            if (file.UIVersion > DocInfo.OriginalVersion)
            {
                RestoreResult = RestoreResult.VersionConflict;
                throw new Exception(string.Format("Update file version failed, destination version bigger than source. Source: {0}, Destination: {1}"
                    , DocInfo.OriginalVersion, File.UIVersion));
            }
            bool isSourceCheckout = DocInfo.IsOrignialCheckOut; //|| _DocInfo.IsCheckOut;
            bool isDestinationCheckout = file.CheckOutType != CheckOutType.None;
            if (file.UIVersion == DocInfo.OriginalVersion)
            {
                if (isSourceCheckout && !isDestinationCheckout)
                {
                    RestoreResult = RestoreResult.VersionConflict;
                    throw new Exception(string.Format("Update file version failed, check out status is mismatch. Source: {0}, Destination: {1}"
                        , isSourceCheckout, isDestinationCheckout));
                }
                if (addStream)
                {
                    file = AddFileAndKeepVersion(true, file);
                    LoadFileInfo(file);
                }
                AveListItemRestore.SetFieldValues(file.ListItemAllFields, DocInfo.FieldsInfo.Fields);
                SetUserDataJunctionFieldValues(file.ListItemAllFields);
                if (!isSourceCheckout)
                {
                    Update(file, DocInfo.CheckinComment, true);
                    needReload = true;
                }
                else //if (isSourceCheckout && isDestinationCheckout)
                {
                    file.ListItemAllFields.Update();
                    needReload = true;
                }
                UpdateXmlFileTwice(file, isSourceCheckout, ref needReload);
                if (needReload)
                {
                    LoadFileInfo(file, false);
                }
                UpdateModifiedAndModeration(DocInfo.FieldsInfo.Fields, file);
            }
            else if (needIncrease)
            {
                SPDocVersion sourceVersion = new SPDocVersion(DocInfo.OriginalVersion / 512, DocInfo.OriginalVersion % 512);
                SPDocVersion destinationVersion = new SPDocVersion(file.UIVersion / 512, file.UIVersion % 512);
                var versionHandler = new AveFileVersionHandler(isSourceCheckout, isDestinationCheckout, sourceVersion, destinationVersion, DocInfo.CheckinComment, ListMemento);
                versionHandler.AddNewFileNeedDeleteVersion(RestoreResult);
                versionHandler.IncreaseVersion(file);
                LoadFileInfo(file);
                UpdateVersion(file, addStream, false);
            }
            RestoreResult = RestoreResult.AddNew;
        }

        /// <summary>
        /// ADO-123228: Xml File need update the title twice.
        /// </summary>
        protected void UpdateXmlFileTwice(Microsoft.SharePoint.Client.File file, bool isSourceCheckout, ref bool needReload)
        {
            object title = string.Empty;
            if (!DocInfo.FieldsInfo.Fields.TryGetValue("Title", out title))
            {
                return;
            }
            var needUpdateFields = new Dictionary<string, object>();
            needUpdateFields["Title"] = title;
            needUpdateFields["Modified"] = DocInfo.FieldsInfo.Fields.ContainsKey("Modified") ? DocInfo.FieldsInfo.Fields["Modified"] : DateTime.Now;
            if (DocInfo.FieldsInfo.Fields.ContainsKey("Editor"))
            {
                needUpdateFields["Editor"] = DocInfo.FieldsInfo.Fields["Editor"];
            }
            AveListItemRestore.SetFieldValues(file.ListItemAllFields, needUpdateFields);
            if (!isSourceCheckout)
            {
                Update(file, DocInfo.CheckinComment, true);
                needReload = true;
            }
            else //if (isSourceCheckout && isDestinationCheckout)
            {
                file.ListItemAllFields.Update();
                needReload = true;
            }
        }
    }
}
