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
    using AvePoint.Wrapper.Common;
    using GCommon;
    using Microsoft.SharePoint.Client;
    using System;
    using System.IO;
    using System.Text;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using static FileProcessorConsts;

    public class ContentSize
    {
        public const int Byte = 1;
        public const int KB = 1024;
        public const int MB = 1024 * 1024;
        public const int GB = 1024 * 1024 * 1024;
    }
    class FileProcessorConsts
    {
        public const int LARGE_FILE_BLOCK_SIZE = 50 * 1024 * 1024;
    }

    class FileCsomProcessor
    {
        private static IAveLogger mLogger = AveLogger.GetInstance(typeof(FileCsomProcessor));
        public static void AddFileUsingPath(ClientContext context, ResourcePath filePath, Stream fileContentStream, Folder parentFolder, bool overWrite)
        {
            FileCollectionAddParameters filesAddParam = new FileCollectionAddParameters
            {
                Overwrite = overWrite
            };
            var file = parentFolder.Files.AddUsingPath(filePath, filesAddParam, fileContentStream);
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
            mLogger.Info("Start to upload a large file:{0} with length:{1}.", fileServerRelativeUrl, contentStream.Length);
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
            mLogger.Info("Finished upload a large file:{0}.", fileServerRelativeUrl);
        }
    }
}
