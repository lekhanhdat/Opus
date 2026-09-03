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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AvePoint.GCommon;
    using Microsoft365.Authentication;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using ClientFile = Microsoft.SharePoint.Client.File;
    using static AvePoint.ObjectModel.ClientOM.ContentSize;
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
    class DocumentContentProcessor
    {
        private static List<string> TextModeFileExtensionList = new List<string>() { ".master", ".evtx", ".cs", ".xoml", ".rules", ".aspx", ".wsp", ".js", ".css", ".html", ".htm" };
        private static List<string> OfficeFileExtensionList = new List<string>() { ".ppt", ".pptx", ".doc", ".docx", ".xls", ".xlsx" };

        private static AveLogger mLogger = AveLogger.GetInstance(typeof(DocumentContentProcessor));
        //private static bool
        private static List<string> SpecialCharacterList = new List<string>() { "�", "￼" };
        private static List<string> IllegalCharacterList = new List<string>() { "%" };

        public static bool IsSpecialCharacterFile(string fileNameOrUrl)
        {
            bool IsSpecialCharacterFile = false;
            if (!string.IsNullOrEmpty(fileNameOrUrl))
            {
                foreach (var specialCharacter in SpecialCharacterList)
                {
                    if (fileNameOrUrl.Contains(specialCharacter))
                    {
                        IsSpecialCharacterFile = true;
                        break;
                    }
                }
                return IsSpecialCharacterFile;
            }
            else
            {
                return IsSpecialCharacterFile;
            }
        }

        public static bool IsIllegalCharacterFile(string fileNameOrUrl)
        {
            bool IsIllegalCharacterFile = false;
            if (!string.IsNullOrEmpty(fileNameOrUrl))
            {
                foreach (var illegalCharacter in IllegalCharacterList)
                {
                    if (fileNameOrUrl.Contains(illegalCharacter))
                    {
                        IsIllegalCharacterFile = true;
                        break;
                    }
                }
                return IsIllegalCharacterFile;
            }
            else
            {
                return IsIllegalCharacterFile;
            }
        }

        public static bool IsTextModeFile(string fileNameOrUrl)
        {
            return TextModeFileExtensionList.Contains(Path.GetExtension(fileNameOrUrl));
        }

        public static bool IsOfficeFile(string fileNameOrUrl)
        {
            string fileExtension = Path.GetExtension(fileNameOrUrl);
            if (string.IsNullOrEmpty(fileExtension))
            {
                return false;
            }
            else
            {
                return OfficeFileExtensionList.Contains(fileExtension.ToLower());
            }
        }

        private static bool IsOneNoteFile(string fileNameOrUrl)
        {
            return string.Equals(Path.GetExtension(fileNameOrUrl), ".one", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMasterPageFile(string fileNameOrUrl)
        {
            return string.Equals(Path.GetExtension(fileNameOrUrl), ".master", StringComparison.OrdinalIgnoreCase);
        }
        public static List<int> RETRY_LIST_ERROR_CODE = new List<int>
        {
            //Throwing ServerException (HRESULT: 0x80131904) when Loading item will cause listItem version restoration error and listItem property uninitialized exception, so if the exception is caught here for Retry, SAAS-630 & SAAS-252
            AveSPErrorCode.ERROR_SQL_EXCEPTION,
            //ErrorTypeName:Microsoft.SharePoint.SPException, ErrorCode:-2146233083, ErrorMessage:Operation timed out. (Exception from HRESULT: 0x80131505) AOSBR-7943
            AveSPErrorCode.CHANNEL_TIME_OUT,
            //ErrorTypeName:Microsoft.SharePoint.SPException, ErrorCode:-2130246326, ErrorMessage:Save Conflict
            //Your changes conflict with those made concurrently by another user. If you want your changes to be applied, click Back in your Web browser, refresh the page, and resubmit your changes.
            AveSPErrorCode.V_LIST_VERSION_CONFLICT,
            //ErrorTypeName:Microsoft.SharePoint.SPException, ErrorCode:-2130575312, CorrelationId:1708b49e-604d-7000-454f-1ffdeb7390e9, ErrorMessage:The URL '*****' is invalid.  It may refer to a nonexistent file or folder, or refer to a valid file or folder that is not in the current Web.
            AveSPErrorCode.TP_E_INVALIDFILENAME,
            //System.ArgumentException, ErrorCode:-2147024809, CorrelationId:18f5ea9e-20f0-8000-fae4-3d041ae418a1
            //Microsoft.SharePoint.Client.ServerException: The object id "site:d7d746c1-c1f7-4460-a346-846d4d458251:web:1a41280a-f456-4025-a0cc-6abd4206f824:list:bf0016dc-ffb5-464c-bb95-ea50c07b72a3:item:2,1" is invalid.
            AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST,//AOSBR-11610, there is a small probability of error in restoring immediately after deletion, add retry 
            //Microsoft.SharePoint.SPException, ErrorCode:-2130575324, Microsoft.SharePoint.Client.ServerException: SQL Server Error. The SQL Server might not be started.
            AveSPErrorCode.TP_E_SQLDOWN,
            //Microsoft.SharePoint.Client.ServerException: There is no Web named "*****".
            AveSPErrorCode.V_BAD_SERVICE_NAME,
            //Microsoft.SharePoint.Client.ServerException: The file ***** has been modified by ***** on *****.
            AveSPErrorCode.TP_E_MD_VERSION_CONFLICT,
            //ErrorTypeName:Microsoft.SharePoint.SPException, ErrorCode:-2130575338, CorrelationId:85fa7a9f-4070-a000-6f48-390f4fedbb38 Microsoft.SharePoint.Client.ServerException: There is no file with URL 'Documents/AvePoint Restore/Geetanjali Goel/Documents/C10 Design - Roadmap/png/png/20x20/Games.png' in this Web.
            AveSPErrorCode.TP_E_LISTITEMDELETED,
            //ErrorTypeName:Microsoft.SharePoint.SPFileCheckOutException, ErrorCode:-2147024738, CorrelationId:6720a89f-80f6-b000-c237-06bcd047bf05
            AveSPErrorCode.ERROR_NOT_LOCKED,
            //ErrorTypeName:Microsoft.SharePoint.SPFileCheckOutException, ErrorCode:-2130575306, CorrelationId:3d96be9f-70c5-b000-f672-62ac126226d7 Microsoft.SharePoint.Client.ServerException: The file "xxx" is checked out for editing by (unknown).
            AveSPErrorCode.TP_E_DOCUMENTLOCKED,
        };
        public static void RetryWithSaveConflict(Action method)
        {
            mLogger.Info("Retry csom action for save conflict issue.");
            var keyValueException = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("ServerException", "Save Conflict"),
                new KeyValuePair<string, string>("ServerException", "The operation has timed out")
            };
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, keyValueException.ToArray());
            retryHelper.AddRetryExceptionDetail(RETRY_LIST_ERROR_CODE.ToArray());
            retryHelper.ExecuteWithRetryMechanism(method);
        }

        //the context must be the current web context,not site collection context
        public static void AddDocument(ClientContext context, ITokenProvider tokenProvider, string parentWebServerRelativeUrl, Folder parentFolder, string fileServerRelativeUrl, Stream fileContentStream, bool overWrite)
        {
            var folderId = (parentFolder != null && parentFolder.IsPropertyAvailable(FolderPropertyNames.UniqueId)) ? parentFolder.UniqueId.ToString() : string.Empty;
            mLogger.Info($"Add document.WebUrl:{parentWebServerRelativeUrl},ParentFolder:{folderId},FileUrl:{SensitiveLogExtension.FormatURLInLog(fileServerRelativeUrl)},ContentLength:{fileContentStream.Length},OverWrite:{overWrite}");
            ProcessPendingRequest(context);
            RetryWithSaveConflict(() =>
            {
                fileContentStream.Position = 0;
                if (IsOneNoteFile(fileServerRelativeUrl))
                {
                    ProcessOneNoteFile(context, tokenProvider, context.Url, parentWebServerRelativeUrl, parentFolder, fileServerRelativeUrl, fileContentStream, overWrite);
                }
                else
                {
                    ProcessGenericFile(context, tokenProvider, context.Url, parentWebServerRelativeUrl, parentFolder, fileServerRelativeUrl, fileContentStream, overWrite);
                }
            });
        }

        private static void ProcessPendingRequest(ClientContext context)
        {
            if (context.HasPendingRequest)
            {
                mLogger.Info("Context has pedning request before add document process.Execute pending request before add document.");
                try
                {
                    context.ExecuteQueryRetry();
                }
                catch (Exception ex)
                {
                    mLogger.Error("An error occurred while executing the pending request.Error:{0}", ex);
                }
                mLogger.Info("Execute Process Pending Request Complete.Next Stage:Add Document.");
            }
        }

        public static ClientFile LoadFile(ClientContext context, string fileServerRelativeUrl)
        {
            ClientFile file = null;
            using (new ContextCacheDisableScope(context))
            {
                try
                {
                    file = context.Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                    context.Load(file);
                    context.ExecuteQuery();
                }
                catch (Exception e)
                {
                    file = null;
                    mLogger.Warn($"Load file failed.Message:{e}.");
                }
            }
            return file;

        }

        private static void ProcessOneNoteFile(ClientContext context, ITokenProvider tokenProvider, string parentWebFullUrl, string parentWebServerRelativeUrl, Folder parentFolder, string fileServerRelativeUrl, Stream fileContentStream, bool overWrite)
        {
            if (fileContentStream.Length < 250 * MB)
            {
                FileRestProcessor.AddFileByRestApi(context, tokenProvider, parentWebFullUrl, GetFolderUniqueId(parentFolder), fileServerRelativeUrl, fileContentStream, overWrite);
            }
            else if (fileContentStream.Length < 1500 * MB && tokenProvider.TokenType == TokenType.IDCLR) // SAAS-40411 Modern token will throw 401 unauthorized when migrate onenote file(***.one)
            {
                FileRPCProcessor.AddFileByRPC(context, tokenProvider, parentWebFullUrl, parentWebServerRelativeUrl, fileServerRelativeUrl, fileContentStream, overWrite);
            }
            else
            {
                FileCsomProcessor.UploadLargeFile(context, fileServerRelativeUrl, fileContentStream, () =>
                 {
                     return parentFolder.Files.AddUsingPathV1(
                    ResourcePath.FromDecodedUrl(fileServerRelativeUrl),
                    new FileCollectionAddParameters { Overwrite = true },
                    new MemoryStream());

                 });
            }
        }

        private static Guid GetFolderUniqueId(Folder folder)
        {
            return (folder != null && folder.IsPropertyAvailable("UniqueId")) ? folder.UniqueId : Guid.Empty;
        }

        private static void ProcessGenericFile(ClientContext context, ITokenProvider tokenProvider, string parentWebFullUrl, string parentWebServerRelativeUrl, Folder parentFolder, string fileServerRelativeUrl, Stream fileContentStream, bool overWrite)
        {
            try
            {
                if (IsMasterPageFile(fileServerRelativeUrl))
                {
                    FileCsomProcessor.AddTextModeFile(context, fileContentStream, fileServerRelativeUrl, parentFolder, overWrite);
                }
                else if (IsIllegalCharacterFile(fileServerRelativeUrl))
                {
                    mLogger.Info($"ProcessGenericFileInternal.Current file:{SensitiveLogExtension.FormatURLInLog(fileServerRelativeUrl)} contains Illegal characters in path.So use UploadLargeFileWithSpecifyRetry.");
                    FileCsomProcessor.UploadLargeFileWithSpecifyRetry(context, fileServerRelativeUrl, fileContentStream, () =>
                    {
                        return parentFolder.Files.AddUsingPathV1(
                       ResourcePath.FromDecodedUrl(fileServerRelativeUrl),
                       new FileCollectionAddParameters { Overwrite = true },
                       new MemoryStream());
                    });

                }
                //特殊字符使用Rest API，微软本身会转义出错，因此对于指定字符，使用Large File方式还原
                else if (IsSpecialCharacterFile(fileServerRelativeUrl)|| IsOfficeFile(fileServerRelativeUrl))
                {
                    mLogger.Info($"ProcessGenericFileInternal.Current file:{SensitiveLogExtension.FormatURLInLog(fileServerRelativeUrl)} contains special character.So use UploadLargeFile.");
                    FileCsomProcessor.UploadLargeFile(context, fileServerRelativeUrl, fileContentStream, () =>
                    {
                        return parentFolder.Files.AddUsingPathV1(
                       ResourcePath.FromDecodedUrl(fileServerRelativeUrl),
                       new FileCollectionAddParameters { Overwrite = true },
                       new MemoryStream());
                    });
                }
                else if (IsTextModeFile(fileServerRelativeUrl) || fileContentStream.Length < 10 * MB)
                {
                    FileCsomProcessor.AddFileUsingPath(context, ResourcePath.FromDecodedUrl(fileServerRelativeUrl), fileContentStream, parentFolder, overWrite);
                }
                else if (fileContentStream.Length < 50 * MB)
                {
                    FileRestProcessor.AddFileByRestApi(context, tokenProvider, parentWebFullUrl, GetFolderUniqueId(parentFolder), fileServerRelativeUrl, fileContentStream, overWrite);
                }
                else
                {
                    FileCsomProcessor.UploadLargeFile(context, fileServerRelativeUrl, fileContentStream, () =>
                    {
                        return parentFolder.Files.AddUsingPathV1(
                       ResourcePath.FromDecodedUrl(fileServerRelativeUrl),
                       new FileCollectionAddParameters { Overwrite = true },
                       new MemoryStream());
                    });
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"ProcessGenericFile.Current file:{fileServerRelativeUrl} restore error:{ex.Message}.");
                if (ex.Message != null
                    //CSOM API
                    && (ex.Message.Contains("It may refer to a nonexistent file or folder, or refer to a valid file or folder that is not in the current Web.")
                        || ex.Message.Contains("它可能指向不存在的文件或文件夹，或者是指向不在当前网站中的有效文件或文件夹")//for 21V
                        || ex.Message.Contains("Elle fait peut-être référence à un fichier ou dossier inexistant. Elle se réfère peut-être également à un fichier ou dossier valide qui ne figure pas dans le site web actuel.")//for French
                        || ex.Message.Contains("Puede que haga referencia a una carpeta o un archivo que no existe, o a una carpeta o un archivo válido que no está en el sitio web actual.")//for Spanish
                        || ex.Message.Contains("CobaltAllZerosDetected")
                        || ex.Message.Contains("CobaltBlockOfLeadingZerosDetected")
                        //Rest API
                        || ex.Message.Contains("You must provide a request body if you set ContentLength>0 or SendChunked==true.")))
                {
                    mLogger.Error($"Current file:{fileServerRelativeUrl} has special error:{ex.Message} and need UploadLargeFile.");
                    var file = LoadFile(context, fileServerRelativeUrl);
                    //delete the dirty file if file exist in SharePoint.
                    if (file != null && file.Exists)

                    {
                        mLogger.Error($"Current file:{fileServerRelativeUrl} restore failed and use UploadLargeFile method to restore.FailedMessage:{ex}.");
                        file.DeleteObject();
                        context.ExecuteQuery();
                        mLogger.Info($"Current file:{fileServerRelativeUrl} delete success.");
                    }
                    fileContentStream.Position = 0;
                    FileCsomProcessor.UploadLargeFile(context, fileServerRelativeUrl, fileContentStream, () =>
                    {
                        return parentFolder.Files.AddUsingPathV1(
                       ResourcePath.FromDecodedUrl(fileServerRelativeUrl),
                       new FileCollectionAddParameters { Overwrite = true },
                       new MemoryStream());
                    });
                    mLogger.Info($"Current file:{SensitiveLogExtension.FormatURLInLog(fileServerRelativeUrl)} UploadLargeFile success.");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
