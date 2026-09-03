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
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveO365OrdinaryFileRestore : AveO365BaseDocumentRestore
    {
        public AveO365OrdinaryFileRestore(AveClientContext context, AveClientOMOffice365Request request, FederationToken tokenProvider, AveDocumentInfo docInfo, Stream fileStream)
            : base(context, request, tokenProvider, docInfo, fileStream)
        { }

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
            if (!LoadFileInfo(newFile))
            {
                // ADO-154648 local07和local10在有video ContentType的list下的video document还原到365下结构有改变，要修改url。
                var fileExtension = Path.GetExtension(FileServerRelativeUrl);
                if (needProcessVideoExtension.Contains(fileExtension.ToLower(CultureInfo.InvariantCulture)) &&
                    ContainsVideoContentType())
                {
                    FileServerRelativeUrl = GetNewFileServerRelativeUrlForVideo(FileServerRelativeUrl);
                    var path = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
                    newFile = ParentWeb.GetFileByServerRelativePath(path);
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

        protected override ClientFile AddFile(bool overwriteIfExists)
        {
            if (this.Context.HasPendingRequest)
            {
                this.Context.ExecuteQuery();
            }
            string fileType = Path.GetExtension(FileServerRelativeUrl);
            ClientFile newFile = null;
            if (SpecialFileList.Contains(fileType, StringComparer.OrdinalIgnoreCase) || FileStream.Length < WrapperConfiguration.BPOS_S.UploadLimit)
            {
                FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
                fileAddParameters.Overwrite = overwriteIfExists;
                var filePath = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
                var folderPath = ResourcePath.FromDecodedUrl(DocInfo.ParentFolderRelativeUrl);
                newFile = ParentWeb.GetFolderByServerRelativePath(folderPath).Files.AddUsingPath(filePath, fileAddParameters, FileStream);
            }

            else
            {
                if (FileServerRelativeUrl.EndsWith(".one", StringComparison.OrdinalIgnoreCase))
                {
                    if (FileStream.Length > 1024 * 1024 * 1024 * 1.5 - 1)
                    {
                        newFile = UploadLargeFileWithCSOM();
                        //throw new WebException("The OneNote File May Can Not Open.");
                    }
                    else if (FileStream.Length > 1024 * 1024 * 250 - 1)
                    {
                        Log.Info("Start to upload a large file by RPC:{0} with length:{1}.", FileServerRelativeUrl, FileStream.Length);
                        Request.AddFileByRPC(DocInfo.ParentWebRelativeUrl, FileServerRelativeUrl, FileStream, overwriteIfExists);
                    }
                    else
                    {
                        Log.Info("Start to upload a large file by RestAPI:{0} with length:{1}.", FileServerRelativeUrl, FileStream.Length);
                        Request.AddFileByRestApi(DocInfo.ParentWebRelativeUrl, FileServerRelativeUrl, FileStream, overwriteIfExists, DocInfo.ParentId);
                    }
                }
                else
                {
                    newFile = UploadLargeFileWithCSOM();
                }

            }

            ClientObjectData objData = AveAssemblyUtility.GetPropertyValue(ParentWeb, "ObjectData") as ClientObjectData;
            objData.MethodReturnObjects.Clear();

            var path = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
            newFile = ParentWeb.GetFileByServerRelativePath(path);

            RevertWebWelcomePage();
            return newFile;
        }

        private ClientFile UploadLargeFileWithCSOM()
        {
            Log.Info("Upload file by slice");
            ClientFile newFile = null;
            ClientResult<long> bytesUploaded = null;
            try
            {
                Guid uploadId = Guid.NewGuid();
                using (BinaryReader br = new BinaryReader(FileStream))
                {
                    byte[] buffer = new byte[2 * 1024 * 1024];
                    Byte[] lastBuffer = null;
                    long fileoffset = 0;
                    long totalBytesRead = 0;
                    int bytesRead;
                    bool first = true;
                    bool last = false;

                    // Read data from filesystem in blocks 
                    while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytesRead = totalBytesRead + bytesRead;

                        // We've reached the end of the file
                        if (totalBytesRead == FileStream.Length)
                        {
                            last = true;
                            // Copy to a new buffer that has the correct size
                            lastBuffer = new byte[bytesRead];
                            Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);
                        }

                        if (first)
                        {
                            using (MemoryStream contentStream = new MemoryStream())
                            {
                                // Add an empty file.
                                FileCollectionAddParameters fileAddParameters = new FileCollectionAddParameters();
                                fileAddParameters.Overwrite = true;
                                var filePath = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
                                var folderPath = ResourcePath.FromDecodedUrl(DocInfo.ParentFolderRelativeUrl);
                                newFile = ParentWeb.GetFolderByServerRelativePath(folderPath).Files.AddUsingPath(filePath, fileAddParameters, contentStream);

                                // Start upload by uploading the first slice. 
                                using (MemoryStream s = new MemoryStream(buffer))
                                {
                                    // Call the start upload method on the first slice
                                    bytesUploaded = newFile.StartUpload(uploadId, s);
                                    this.Context.ExecuteQuery();
                                    // fileoffset is the pointer where the next slice will be added
                                    fileoffset = bytesUploaded.Value;
                                }

                                // we can only start the upload once
                                first = false;
                            }
                        }
                        else
                        {
                            // Get a reference to our file
                            var filePath = ResourcePath.FromDecodedUrl(FileServerRelativeUrl);
                            newFile = ParentWeb.GetFileByServerRelativePath(filePath);

                            if (last)
                            {
                                // Is this the last slice of data?
                                using (MemoryStream s = new MemoryStream(lastBuffer))
                                {
                                    // End sliced upload by calling FinishUpload
                                    newFile.FinishUpload(uploadId, fileoffset, s);
                                    this.Context.ExecuteQuery();

                                    // return the file object for the uploaded file
                                    break;
                                }
                            }
                            else
                            {
                                using (MemoryStream s = new MemoryStream(buffer))
                                {
                                    // Continue sliced upload
                                    bytesUploaded = newFile.ContinueUpload(uploadId, fileoffset, s);
                                    this.Context.ExecuteQuery();
                                    // update fileoffset for the next slice
                                    fileoffset = bytesUploaded.Value;
                                }
                            }
                        }

                    } // while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                }
            }
            finally
            {
                if (FileStream != null)
                {
                    FileStream.Dispose();
                }
            }
            return newFile;
        }
    }
}
