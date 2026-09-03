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
namespace AvePoint.ObjectModel.ClientOM
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using System;
    using System.IO;
    using System.Text;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using static FileProcessorConsts;
    class FileCsomProcessor
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(FileCsomProcessor));
        public static void AddFileUsingPath(ClientContext context, ResourcePath filePath, Stream fileContentStream, Folder parentFolder, bool overWrite)
        {
            FileCollectionAddParameters filesAddParam = new FileCollectionAddParameters
            {
                Overwrite = overWrite
            };
            var file = parentFolder.Files.AddUsingPathV1(filePath, filesAddParam, fileContentStream);
            context.ExecuteQuery();
        }
        public static void UploadLargeFileWithSpecifyRetry(ClientContext context, string fileServerRelativeUrl, Stream contentStream, GetOrCreateFirstFile getOrCreateFirstFileMethod)
        {
            //mContext.RequestTimeout = 60 * 60 * 1000;
            ClientFile uploadFile = null;
            ClientResult<long> bytesUploaded = null;
            var uploadId = Guid.NewGuid();
            long fileOffSet = 0;//fileOffSet is the pointer where the next slice will be added
            bool first = true;
            mLogger.Info("Start to upload a large file with length:{0}.", contentStream.Length);
            while (true)
            {
                if (first)
                {
                    using (MemoryStream emptyContent = new MemoryStream())
                    {
                        try
                        {
                            uploadFile = getOrCreateFirstFileMethod();
                            context.ExecuteQuery();
                        }
                        catch (Exception ex)
                        {
                            if (ex != null && ex.Message.Contains("Illegal characters in path"))
                            {
                                mLogger.Info($"UploadLargeFileWithSpecifyRetry file contains Illegal characters in path.FileURL:{fileServerRelativeUrl}.");
                                uploadFile = context.Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                                context.Load(uploadFile, f => f.UniqueId);
                                context.ExecuteQuery();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        using (var firstStream = new AveChunkStream(contentStream, LARGE_FILE_BLOCK_SIZE))
                        {
                            bytesUploaded = uploadFile.StartUpload(uploadId, firstStream);
                            context.ExecuteQuery();
                            fileOffSet = bytesUploaded.Value;
                        }
                    }
                    first = false;
                }
                else
                {
                    using (var chunkStream = new AveChunkStream(contentStream, LARGE_FILE_BLOCK_SIZE))
                    {
                        uploadFile = context.Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                        if (contentStream.Position + LARGE_FILE_BLOCK_SIZE >= contentStream.Length)
                        {
                            uploadFile.FinishUpload(uploadId, fileOffSet, chunkStream);
                            context.Load(uploadFile, f => f.UniqueId);
                            context.ExecuteQuery();
                            break;
                        }
                        else
                        {
                            bytesUploaded = uploadFile.ContinueUpload(uploadId, fileOffSet, chunkStream);
                            context.ExecuteQuery();
                            fileOffSet = bytesUploaded.Value;
                        }
                    }
                }
            }
            mLogger.Info("Finished upload a large file");
        }

        public static void AddTextModeFile(ClientContext context, Stream stream, string serverRelativeUrl, Folder parentFolder, bool overWrite)
        {
            MemoryStream ms = new MemoryStream();
            AveIOHelper.Copy(stream, ms);
            ms.Position = 0;
            byte[] content = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(ms.ToArray()).Replace("16.0.0.0", "15.0.0.0"));
            FileCreationInformation fileCreationInfo = new FileCreationInformation
            {
                Content = content,
                Url = serverRelativeUrl,
                Overwrite = overWrite
            };
            var file = parentFolder.Files.Add(fileCreationInfo);
            context.ExecuteQuery();
        }

        public delegate ClientFile GetOrCreateFirstFile();

        public static void UploadLargeFile(ClientContext context, string fileServerRelativeUrl,Stream contentStream, GetOrCreateFirstFile getOrCreateFirstFileMethod)
        {
            //mContext.RequestTimeout = 60 * 60 * 1000;
            ClientFile uploadFile = null;
            ClientResult<long> bytesUploaded = null;
            var uploadId = Guid.NewGuid();
            long fileOffSet = 0;//fileOffSet is the pointer where the next slice will be added
            bool first = true;
            mLogger.Info("Start to upload a large file with length:{0}.", contentStream.Length);
            while (true)
            {
                if (first)
                {
                    using (MemoryStream emptyContent = new MemoryStream())
                    {
                        uploadFile = getOrCreateFirstFileMethod();

                        using (var firstStream = new AveChunkStream(contentStream, LARGE_FILE_BLOCK_SIZE))
                        {
                            bytesUploaded = uploadFile.StartUpload(uploadId, firstStream);
                            context.ExecuteQuery();
                            fileOffSet = bytesUploaded.Value;
                        }
                    }
                    first = false;
                }
                else
                {
                    using (var chunkStream = new AveChunkStream(contentStream, LARGE_FILE_BLOCK_SIZE))
                    {
                        uploadFile = context.Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                        if (contentStream.Position + LARGE_FILE_BLOCK_SIZE >= contentStream.Length)
                        {
                            uploadFile.FinishUpload(uploadId, fileOffSet, chunkStream);
                            context.ExecuteQuery();
                            break;
                        }
                        else
                        {
                            bytesUploaded = uploadFile.ContinueUpload(uploadId, fileOffSet, chunkStream);
                            context.ExecuteQuery();
                            fileOffSet = bytesUploaded.Value;
                        }
                    }
                }
            }
            mLogger.Info("Finished upload a large file.");
        }
    }
}
