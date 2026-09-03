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

using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using Google;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Util;
using RAGoogle.Util.StreamUtil;
using System.Net;
using System.Reflection;
using Google.Apis.Auth.OAuth2.Responses;
using File = Google.Apis.Drive.v3.Data.File;

namespace RAGoogle.API
{

    internal class DriveApi : IDisposable
    {
        private readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private DriveService _service;
        private int RetriedCount;
        private File NewFile;
        private CancellationToken UploadCancellationToken;

        private static readonly Dictionary<string, string> usernameEmailAddressMapping = new();

        private static readonly object _syncObj = new object();

        internal DriveApi(BaseClientService.Initializer initializer)
        {
            _service = new DriveService(initializer);
            _service.HttpClient.Timeout = TimeSpan.FromMinutes(5);
        }
        //https://developers.google.com/drive/api/reference/rest/v3/about
        //https://developers.google.com/drive/api/reference/rest/v3/about/get
        internal async Task<About> GetAboutAsync()
        {
            var request = _service.About.Get();
            request.Fields = "*";
            var response = await request.ExecuteExAsync();
            return response;
        }

        internal async Task<File> GetRootFolderAsync(FileQuery query = null)
        {
            var request = _service.Files.Get("root");//"My Drive" folder
            request.Fields = "*";
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var response = await request.ExecuteExAsync();
            return response;
        }

        internal async Task<List<Drive>> ListSharedDrivesAsync(FileQuery query = null)
        {
            var result = new List<Drive>();
            var request = _service.Drives.List();
            request.Fields = "drives(id,name,themeId,colorRgb,backgroundImageFile,hidden,restrictions),nextPageToken";
            request.PageSize = 100;
            request.UseDomainAdminAccess = query?.UseDomainAdminAccess;//list all shared drive in the domain
            request.Q = query?.QueryString;

            do
            {
                var response = await request.ExecuteExAsync();
                if (response == null)
                {
                    throw new CommonException("There is something wrong when listing shared drives, the api response is null");
                }
                result.AddRange(response.Drives);
                request.PageToken = response.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));

            return result;
        }

        internal async Task<(List<File> files, string? pageToken)> ListFilesAsync(FileQuery query = null)
        {
            var files = new List<File>();
            var startTime = DateTime.UtcNow;
            string? pageToken = query?.PageToken;
            var request = _service.Files.List();
            request.Q = query?.QueryString;
            request.PageSize = (query != null && query.PageSize > 0) ? query.PageSize : 1000;
            request.Fields = "*";
            request.PageToken = pageToken;
            if (query != null && query.OrderBy.IsNotNullOrEmpty())
            {
                request.OrderBy = query.OrderBy;
            }
            if (query != null && query.SearchTrashFile)
            {
                request.DriveId = query?.SharedDriveId;
                request.Corpora = "drive";
            }

            if (query != null && query.SearchInDrive)
            {
                request.Spaces = "drive";
                request.DriveId = query?.SharedDriveId;
                request.Corpora = query?.SharedDriveId != null ? "drive" : null;
            }
            if (query != null && query.IncludeLabels.IsNotNullOrEmpty())
            {
                request.IncludeLabels = query.IncludeLabels;
            }
            request.IncludeItemsFromAllDrives = query?.IncludeItemsFromAllDrives;
            request.SupportsAllDrives = query?.SupportsAllDrives;
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var response = await request.ExecuteExAsync() ?? throw new CommonException("There is something wrong when listing files, the api response is null");
            if (response is not null && response.Files.IsNotEmptyCollection())
            {
                files.AddRange(response.Files);
                pageToken = response.NextPageToken;
            }
            else
            {
                pageToken = null;
            }
            if (startTime.AddMinutes(10) < DateTime.UtcNow)
            {
                logger.Debug($"Current list files:{files.Count}.");
                startTime = DateTime.UtcNow;
            }

            return (files, pageToken);
        }
        internal async IAsyncEnumerable<List<File>> ListAllFilesAsync(FileQuery query = null)
        {
            var files = new List<File>();
            var startTime = DateTime.UtcNow;
            string? pageToken = query?.PageToken;
            do
            {
                var request = _service.Files.List();
                request.Q = query?.QueryString;
                request.PageSize = (query != null && query.PageSize > 0) ? query.PageSize : 1000;
                request.Fields = "*";
                request.PageToken = pageToken;
                if (query != null && query.OrderBy.IsNotNullOrEmpty())
                {
                    request.OrderBy = query.OrderBy;
                }
                if (query != null && query.SearchTrashFile)
                {
                    request.DriveId = query?.SharedDriveId;
                    request.Corpora = "drive";
                }

                if (query != null && query.SearchInDrive)
                {
                    request.Spaces = "drive";
                    request.DriveId = query?.SharedDriveId;
                    request.Corpora = query?.SharedDriveId != null ? "drive" : null;
                }
                if (query != null && query.IncludeLabels.IsNotNullOrEmpty())
                {
                    request.IncludeLabels = query.IncludeLabels;
                }
                request.IncludeItemsFromAllDrives = query?.IncludeItemsFromAllDrives;
                request.SupportsAllDrives = query?.SupportsAllDrives;
                if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
                {
                    request.QuotaUser = query.QuotaUser;
                }
                var response = await request.ExecuteExAsync() ?? throw new CommonException("There is something wrong when listing files, the api response is null");
                if (response is not null && response.Files.IsNotEmptyCollection())
                {
                    pageToken = response.NextPageToken;
                }
                else
                {
                    break;
                }
                if (startTime.AddMinutes(10) < DateTime.UtcNow)
                {
                    logger.Debug($"Current list files:{files.Count}.");
                    startTime = DateTime.UtcNow;
                }
                yield return response.Files.ToList();
            } while (pageToken.IsNotNullOrEmpty());
        }
        internal async Task<List<Permission>> ListPermissionAsync(string fileId, FileQuery query = null)
        {
            var result = new List<Permission>();
            string pageToken = null;
            do
            {
                var request = _service.Permissions.List(fileId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.Fields = "nextPageToken, permissions(id, displayName, type, emailAddress, domain, role, photoLink, allowFileDiscovery, expirationTime, permissionDetails)";
                request.SupportsAllDrives = true;
                request.UseDomainAdminAccess = query?.UseDomainAdminAccess;
                if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
                {
                    request.QuotaUser = query.QuotaUser;
                }
                var response = await request.ExecuteExAsync();
                if (response != null && response.Permissions != null)
                {
                    result.AddRange(response.Permissions);
                    pageToken = response.NextPageToken;
                }
                else
                {
                    break;
                }
            } while (!string.IsNullOrEmpty(pageToken));
            return result;
        }

        internal async Task<Permission> CreatePermissionAsync(Permission permission, string fileId, FileQuery query = null)
        {
            var request = _service.Permissions.Create(permission, fileId);
            request.SendNotificationEmail = false;

            request.Fields = "id";
            request.SupportsAllDrives = true;
            request.UseDomainAdminAccess = query?.UseDomainAdminAccess;
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var response = await request.ExecuteExAsync();
            return response;
        }

        internal async Task DeletePermissionByMemberEmailAsync(string memberEmail, string fileId, FileQuery query = null)
        {
            var permissions = await ListPermissionAsync(fileId);
            var permission = permissions.Find(permission => permission.EmailAddress.Equals(memberEmail, StringComparison.OrdinalIgnoreCase));
            var request = _service.Permissions.Delete(fileId, permission!.Id);
            request.Fields = "id";
            request.SupportsAllDrives = true;
            request.UseDomainAdminAccess = query?.UseDomainAdminAccess;
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            await request.ExecuteExAsync();
        }

        internal async Task<Permission> CreatePermissionAsync(Permission permission, string fileId)
        {
            var request = _service.Permissions.Create(permission, fileId);
            request.SupportsAllDrives = true;
            request.SendNotificationEmail = false;
            request.Fields = "*";
            var response = await request.ExecuteAsync();
            return response;
        }

        internal async Task RestoreFileLabelAsync(ModifyLabelsRequest body, string fileId, FileQuery query = null)
        {
            var request = _service.Files.ModifyLabels(body, fileId);
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var response = await request.ExecuteExAsync();
        }

        internal async Task<List<Label>> AppliedLabelOnFileAsync(string labelId, string fileId)
        {
            LabelModification modification = new()
            {
                LabelId = labelId
            };

            ModifyLabelsRequest body = new()
            {
                LabelModifications = [modification]
            };
            var request = _service.Files.ModifyLabels(body, fileId);

            var response = await request.ExecuteExAsync();

            return response.ModifiedLabels.ToList();
        }

        internal async Task<List<Label>> BatchRemoveLabelOnFileAsync(List<string> labelIds, string fileId)
        {
            List<LabelModification> modifications = [];
            foreach (var labelId in labelIds)
            {
                modifications.Add(new()
                {
                    LabelId = labelId,
                    RemoveLabel = true
                });
            }

            ModifyLabelsRequest body = new()
            {
                LabelModifications = modifications
            };
            var request = _service.Files.ModifyLabels(body, fileId);

            var response = await request.ExecuteExAsync();

            return response.ModifiedLabels.ToList();
        }

        internal async Task<List<Revision>> GetAllFileVersionsAsync(string docId)
        {
            var request = _service.Revisions.List(docId);
            request.Fields = "*";
            request.PageSize = 100;
            var response = await request.ExecuteExAsync();
            var revisions = response.Revisions;

            return revisions.ToList();
        }
        internal async Task<bool> UpdateFileVersion(string docId, string revisionId, Revision body)
        {
            var request = _service.Revisions.Update(body,docId,revisionId);
            request.Fields = "*";
            var response = await request.ExecuteExAsync();
          
            return true;
        }
        internal async Task<Permission> UpdatePermissionAsync(Permission body, string fileId, string permissionId, FileQuery query = null)
        {
            var request = _service.Permissions.Update(body, fileId, permissionId);
            request.SupportsAllDrives = true;
            request.UseDomainAdminAccess = query?.UseDomainAdminAccess;
            request.TransferOwnership = query?.TransferOwnership;
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var response = await request.ExecuteExAsync();
            return response;
        }

        internal async Task<string> DeletePermissionAsync(string fileId, string permissionId, FileQuery query = null)
        {
            var request = _service.Permissions.Delete(fileId, permissionId);
            request.SupportsAllDrives = true;
            request.UseDomainAdminAccess = query?.UseDomainAdminAccess;
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var response = await request.ExecuteExAsync();
            return response;
        }

        internal async Task<File> GetFileInfoAsync(string fileId, FileQuery query = null)
        {
            var request = _service.Files.Get(fileId);
            request.SupportsAllDrives = query?.SupportsAllDrives;
            request.Fields = "id, name, size, modifiedTime, version,createdTime, webViewLink, parents,fileExtension, mimeType, owners, lastModifyingUser,shared,ownedByMe";
            if (!string.IsNullOrEmpty(query?.QuotaUser) && query.QuotaUser.Length < 40)
            {
                request.QuotaUser = query.QuotaUser;
            }
            var file = await request.ExecuteExAsync();
            return file;
        }

        internal async Task<File> GetFileAsync(string fileId, string labelIds = "")
        {
            var request = _service.Files.Get(fileId);
            request.SupportsAllDrives = true;
            request.Fields = "*";
            if (labelIds.IsNotNullOrEmpty())
            {
                request.IncludeLabels = labelIds;
            }
            var response = await request.ExecuteExAsync();
            return response;
        }

        //internal async Task<File> GetFileBasicInfoAsync(string fileId)
        //{
        //    var request = _service.Files.Get(fileId);
        //    request.SupportsAllDrives = true;
        //    request.Fields = "*";
        //    var response = await request.ExecuteExAsync();
        //    return response;
        //}

        internal async Task<bool> DeleteFileAsync(string fileId)
        {
            var request = _service.Files.Delete(fileId);
            request.SupportsAllDrives = true;
            // If successful, the response body is empty
            var result = await request.ExecuteExAsync();
            return result.Equals(string.Empty);
        }

        internal async Task<File> MoveToNewFolderAsync(string fileId, string oldFolderId, string newFolderId)
        {
            var updateRequest = _service.Files.Update(new File(),
                fileId);
            updateRequest.Fields = "*";
            updateRequest.AddParents = newFolderId;
            updateRequest.RemoveParents = oldFolderId;
            updateRequest.SupportsAllDrives = true;
            var file = await updateRequest.ExecuteExAsync();
            return file;
        }

        internal async Task DownloadFileAsync(string fileId, string path)
        {
            var request = _service.Files.Get(fileId);
            using (var stream = new MemoryStream())
            {
                await request.DownloadAsync(stream);
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
        }

        private async Task<Stream> GetBigFileAsync(string fileId, long fileSize, Func<long, long, Stream, Task<Stream>> func)
        {
            return new PartlyRequestStream((long offset, int size, out int readLength) =>
            {
                using var performanceReportScope =
                    new PerformanceScope("DriveApi.GetFileData.PartlyRequestStream");
                readLength = 0;
                long endPos = offset + size - 1;
                endPos = endPos > fileSize - 1 ? fileSize - 1 : endPos;
                if (offset >= endPos)
                {
                    return new MemoryStream();
                }

                int time = 0;
                while (time < GoogleConstant.MAX_RESUME_RETRIES)
                {
                    var outStream = new System.IO.MemoryStream();
                    try
                    {
                        time++;
                        func(offset, endPos, outStream).ConfigureAwait(false).GetAwaiter().GetResult();
                        outStream.Position = 0;
                        readLength = (int)outStream.Length;
                        return outStream;
                    }
                    catch (GoogleApiException gex)
                    {
                        if (time == GoogleConstant.MAX_RESUME_RETRIES)
                        {
                            throw;
                        }

                        if (GoogleRequestExtension.NeedRetry(gex, time))
                        {
                            logger.Warn(
                                $"[PartlyRequestStream]:Retry count:{time}.Download file {fileId} exception:{gex}.");
                            continue;
                        }

                        throw;
                    }
                    catch (TokenResponseException te)//Google.Apis.Auth.OAuth2.Responses.TokenResponseException: Error:"internal_failure", Description:"", Uri:""
                    {
                        if (time == GoogleConstant.MAX_RESUME_RETRIES)
                        {
                            throw;
                        }
                        if (te.Message.Contains("internal_failure"))
                        {
                            logger.Warn($"[PartlyRequestStream]:Google request TokenResponse exception. retry count {time} error:{te}");
                            Thread.Sleep(15 * 1000);
                            continue;
                        }
                        if (te.Message.Contains("unauthorized_client"))
                        {
                            logger.Warn($"[PartlyRequestStream]: Unauthorized Client. retry count {time} error:{te}");
                            Thread.Sleep(15 * 1000);
                            continue;
                        }
                        logger.Error($"retry count {time}.ExecuteEx request failed: {te}");
                        throw;
                    }
                    catch (Exception e)
                    {
                        if (time == GoogleConstant.MAX_RESUME_RETRIES)
                        {
                            throw;
                        }
                        else if (GoogleRequestExtension.IsNetworkException(e))
                        {
                            logger.Warn(
                                $"[PartlyRequestStream]:Network error download file {fileId}, retry count:{time}. Detail: {e}.");
                            Thread.Sleep(1 * 60 * 1000);
                            continue;
                        }
                        else if (e.InnerException != null)
                        {
                            var ex = e.InnerException as WebException;
                            if (ex is { Status: WebExceptionStatus.ProtocolError })
                            {
                                logger.Warn(
                                    $"[PartlyRequestStream]:Retry count:{time}.Download file {fileId} exception:{ex}, error code:{(int)ex.Status}.");
                                Thread.Sleep(15 * 1000);
                                continue;
                            }

                            if (ex != null && (int)ex.Status == 429)
                            {
                                logger.Warn(
                                    $"[PartlyRequestStream]:Retry count:{time}.Download file {fileId} exception:{ex}, error code:{(int)ex.Status}.");
                                Thread.Sleep(GoogleRequestExtension.GetThrottlingTime(time));
                                continue;
                            }

                            if (e.InnerException.Message.Contains("A task was canceled") ||
                                e.InnerException.Message.Contains("Transferred a partial file"))
                            {
                                logger.Warn(
                                    $"[PartlyRequestStream]:Api inner error download file {fileId}. retry count:{time}. Detail: {e}.");
                                Thread.Sleep(1 * 60 * 1000);
                                continue;
                            }
                        }
                        // just to be sure
                        else if (e is HttpRequestException && e.Message.Contains("429"))
                        {
                            logger.Warn(
                                $"[PartlyRequestStream]:Retry count:{time}.Download file {fileId} exception:{e}.");
                            Thread.Sleep(GoogleRequestExtension.GetThrottlingTime(time));
                            continue;
                        }
                        else if (e is HttpRequestException && e.Message.Contains("413"))
                        {
                            logger.Warn(
                                $"[PartlyRequestStream]:Retry count:{time}.Download file {fileId} exception:{e}.");
                            Thread.Sleep(GoogleRequestExtension.GetThrottlingTime(time));
                            continue;
                        }
                        else if (e.Message.Contains("A task was canceled") ||
                                 e.Message.Contains("Transferred a partial file"))
                        {
                            logger.Warn(
                                $"[PartlyRequestStream]:Api inner error download file {fileId}, retry count:{time}. Detail: {e}.");
                            Thread.Sleep(1 * 60 * 1000);
                            continue;
                        }
                        throw;
                    }
                }

                throw new CommonException("[PartlyRequestStream]:File download failed.");
            });
        }

        internal async Task DownloadFileToStreamAsync(string fileId, Stream outputStream)
        {
            try
            {
                if (string.IsNullOrEmpty(fileId) || outputStream == null)
                {
                    throw new ArgumentException("Invalid arguments provided.");
                }

                var request = _service.Files.Get(fileId);
                request.SupportsAllDrives = true;

                await request.DownloadAsync(outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);

                logger.Info($"Successfully downloaded file {fileId} to stream.");
            }
            catch (GoogleApiException ex)
            {
                logger.Error($"Failed to download file {fileId}. Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected error in DownloadFileToStreamAsync: {ex.Message}");
                throw;
            }
        }
        internal async Task DownloadFileVersionToStreamAsync(GoogleItemData file, string revisionId, string localPath)
        {
            if (string.IsNullOrEmpty(file.Id) || string.IsNullOrEmpty(revisionId) || string.IsNullOrEmpty(localPath))
            {
                throw new ArgumentException("Invalid arguments provided.");
            }

            var request = _service.Revisions.Get(file.Id, revisionId);
            request.Fields = "*";

            await using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);

            if (GoogleConstant.GoogleExportMimeType.TryGetValue(file.MimeType, out var exportMimeType) && exportMimeType != null)
            {
                var res = await request.ExecuteExAsync();
                DownloadFileByExportLink(file.Id, res, fileStream);
            }
            else
            {
                IDownloadProgress downloadProgress = await request.DownloadAsync(fileStream);
                if (downloadProgress.Status != DownloadStatus.Completed)
                {
                    throw downloadProgress.Exception ?? new CommonException($"File {file.Id} download failed.");
                }
            }

            logger.Info($"Successfully downloaded version {revisionId} of file {file.Id} to stream.");
        }

        internal void DownloadFileByExportLink(string fileId, Revision currentFile, Stream stream)
        {
            try
            {
                logger.Info("DownloadFileByExportLink start");
                logger.Info($"revision: {currentFile?.Id}, exportLinks count: {currentFile?.ExportLinks?.Count}");
                if (currentFile == null || currentFile.ExportLinks == null)
                {
                    throw new Exception($"File {fileId} has no export links available for download.");
                }

                if (!GoogleConstant.GoogleExportMimeType.TryGetValue(currentFile.MimeType, out var googleType))
                {
                    googleType = currentFile.MimeType;
                }

                if (currentFile.ExportLinks.TryGetValue(googleType, out var exportLink))
                {
                    logger.Info($"[DownloadFileByExportLink]: Downloading file {fileId} in format {googleType}, MimeType: {currentFile.MimeType}.");
                    DownloadByHttpClient(exportLink, stream);
                }
                else
                {
                    throw new Exception($"Export link not found for file {fileId} with format {googleType}.");
                }
            }
            catch (GoogleApiException ex)
            {
                logger.Error($"Failed to download revision of file {fileId}. Google API Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected error in DownloadFileByExportLink: {ex.Message}");
                throw;
            }
        }

        private void DownloadByHttpClient(string downloadLink, Stream stream)
        {
            var downloader = Download(downloadLink);
            var result = downloader.GetAwaiter().GetResult();
            if (downloader.Exception != null)
            {
                throw downloader.Exception;
            }
            if (downloader.IsCanceled)
            {
                throw new CommonException("Download file task cancelled.");
            }
            if (downloader.IsFaulted)
            {
                throw new CommonException("Download file task fault.");
            }
            result.CopyTo(stream);
            stream.Position = 0;
            result.Dispose();
        }

        internal async Task DownloadBigFileVersionToStreamAsync(string fileId, string revisionId, string localPath, long fileSize)
        {
            if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(revisionId) || localPath == null)
            {
                throw new ArgumentException("Invalid arguments provided.");
            }


            var data = await GetBigFileAsync(fileId, fileSize, async (offset, endPos, stream) =>
            {
                var request = _service.Revisions.Get(fileId, revisionId);
                using CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(new TimeSpan(2, 0, 0));
                IDownloadProgress downloadProgress = await request.DownloadRangeAsync(stream, new System.Net.Http.Headers.RangeHeaderValue(offset, endPos), cts.Token);
                logger.Info($"offset: {offset}, endPos: {endPos}");
                if (downloadProgress.Status != DownloadStatus.Completed)
                {
                    if (downloadProgress.Exception != null)
                    {
                        throw downloadProgress.Exception;
                    }
                    throw new CommonException($"File {fileId} download failed.");
                }

                return null;
            });

            lock (_syncObj)
            {
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);
                using (data)
                {
                    int dataLength;
                    int bufferSize = 1024 * 1024 * 4;

                    byte[] bytes = new byte[bufferSize];
                    while ((dataLength = data.Read(bytes, 0, bufferSize)) > 0)
                    {
                        fileStream.Write(bytes, 0, dataLength);
                    }
                }
            }

            logger.Info($"Successfully downloaded version {revisionId} of file {fileId} to stream.");
        }

        internal async Task DownloadMediaAsync(string fileId, string mimeType, string path)
        {
            var downloadRequest = _service.Files.Download(fileId);
            downloadRequest.MimeType = mimeType;
            var downloadResponse = await downloadRequest.ExecuteExAsync();

            if (downloadResponse.Response != null &&
                downloadResponse.Response.TryGetValue("downloadUri", out var uri))
            {
                await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                await DownloadByHttpClientAsync(uri as string, fileStream);
            }
            else
            {
                logger.Error($"Cannot find download uri for format {mimeType}.");
                throw new Exception("File download failed.");
            }
        }

        internal async Task ExportFileAsync(string fileId, string path, string mimeType)
        {
            var request = _service.Files.Export(fileId, mimeType);
            await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            IDownloadProgress downloadProgress = await request.DownloadAsync(fileStream);
            if (downloadProgress.Status != DownloadStatus.Completed)
            {
                if (downloadProgress.Exception != null)
                {
                    throw downloadProgress.Exception;
                }
                throw new CommonException($"File {fileId} download failed.");
            }
        }

        internal async Task ExportBigFileAsync(string fileId, string path, string mimeType)
        {
            var currentFileGetRequest = _service.Files.Get(fileId);
            currentFileGetRequest.Fields = "exportLinks";
            currentFileGetRequest.SupportsAllDrives = true;
            var currentFile = await currentFileGetRequest.ExecuteExAsync();

            await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            try
            {
                logger.Info($"Export links {currentFile.ExportLinks} with current mimeType {mimeType}");
                if (currentFile is { ExportLinks: not null } && currentFile.ExportLinks.TryGetValue(mimeType, out var exportLink))
                {
                    logger.Info($"[GetBigFile]: Download file {fileId} by format {mimeType}, MimeType:{currentFile.MimeType}.");
                    await DownloadByHttpClientAsync(exportLink, fileStream);
                }
                else
                {
                    throw new Exception($"File {fileId} download failed.");
                }
            }
            catch (Exception e)
            {
                if (e is HttpRequestException && (e.Message.Contains("413") || e.Message.Contains("400")))
                {
                    if (GoogleConstant.GoogleSecondExportMimeType.TryGetValue(currentFile.MimeType, out mimeType))
                    {
                        logger.Warn($"[GetBigFile]: Download file {fileId} failed, MimeType:{currentFile.MimeType},try {mimeType} format, exception:{e}.");
                        if (currentFile is { ExportLinks: not null } && currentFile.ExportLinks.TryGetValue(mimeType, out var exportLink))
                        {
                            await DownloadByHttpClientAsync(exportLink, fileStream);
                        }
                        else
                        {
                            throw new CommonException($"File {fileId} download failed.");
                        }
                    }
                }
                throw;
            }
        }

        private async Task DownloadByHttpClientAsync(string exportLink, Stream fileStream)
        {
            var downloader = Download(exportLink);
            var result = await downloader;
            if (downloader.Exception != null)
            {
                throw downloader.Exception;
            }
            if (downloader.IsCanceled)
            {
                throw new CommonException("Download file task cancelled.");
            }
            if (downloader.IsFaulted)
            {
                throw new CommonException("Download file task fault.");
            }

            await result.CopyToAsync(fileStream);
            fileStream.Position = 0;
            await result.DisposeAsync();
        }

        private async Task<Stream> Download(string downloadLink)
        {
            Stream stream;
            var response = await _service.HttpClient.GetAsync(downloadLink, HttpCompletionOption.ResponseHeadersRead);
            try
            {
                response.EnsureSuccessStatusCode();
                stream = await response.Content.ReadAsStreamAsync();
            }
            catch (Exception e)
            {
                logger.Warn($"[Download] request message: {response.RequestMessage?.RequestUri}. response status code:{response.StatusCode}. download error:{e}");
                throw;
            }
            return stream;
        }

        internal async Task<File> CreateFileAsync(File file, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var request = _service.Files.Create(file, fileStream, "application/octet-stream");
                request.SupportsAllDrives = true;
                request.Fields = "*";
                request.UploadType = "multipart";
                var response = await request.UploadAsync();
                response.ThrowOnFailure();
                return request.ResponseBody;
            }

        }
        internal async Task<File> CreateFileAsync(File file)
        {
            var request = _service.Files.Create(file);
            request.Fields = "*";
            request.SupportsAllDrives = true;
            var response = await request.ExecuteExAsync();
            return response;
        }
        internal async Task<File> UploadFileAsync(File file, string existFileId, string mimeType, bool restoreRevision, Stream stream)
        {
            this.RetriedCount = 0;
            this.UploadCancellationToken = new CancellationToken();
            ResumableUpload<File, File> request;
            if (restoreRevision)
            {
                var updateRequest = _service.Files.Update(file, existFileId, stream, mimeType);
                updateRequest.Fields = "*";
                updateRequest.SupportsAllDrives = true;
                request = updateRequest;
            }
            else
            {
                stream.Position = 0;
                var createRequest = _service.Files.Create(file, stream, mimeType);
                createRequest.Fields = "*";
                createRequest.SupportsAllDrives = true;
                request = createRequest;
            }
            request.ProgressChanged += ProgressChanged;
            request.ResponseReceived += ResponseReceived;

            try
            {
                Task.Run(async () =>
                {
                    await request.UploadAsync(this.UploadCancellationToken);
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                logger.Warn("Upload file failed with resume method:{0}", e.ToString());
                Thread.Sleep(1000);
            }
            //continue transferring from breakpoint
            bool isNeedResumeUpload = false;
            do
            {
                isNeedResumeUpload = false;
                if (NeedResumeUpload())
                {
                    this.RetriedCount++;
                    isNeedResumeUpload = true;
                    try
                    {
                        Task.Run(async () =>
                        {
                            await request.ResumeAsync(this.UploadCancellationToken);
                        }).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        logger.Info("retry to upload file failed:{0}", ex.ToString());
                    }
                }
            } while (isNeedResumeUpload && NeedResumeUpload());

            int retry = 0;
            while (retry < 10)
            {
                if (request.ResponseBody != null)
                {
                    break;
                }
                Thread.Sleep(1000);
                retry++;
            }
            if (request.ResponseBody != null)
            {
                file = request.ResponseBody;
            }
            else
            {
                logger.Error($"File ResponseBody is null, retry count {retry}.");
                throw new CommonException("File ResponseBody is null");
            }

            return file;
        }
        private void ProgressChanged(IUploadProgress uploadStatusInfo)
        {
            switch (uploadStatusInfo.Status)
            {
                case UploadStatus.Completed:
                    this.RetriedCount = Int32.MaxValue; // Ensure that Do..While loop terminates
                    logger.Info(String.Format("Status: Complete,Uploading {0:N} bytes.", uploadStatusInfo.BytesSent));
                    break;
                case UploadStatus.Starting:
                    logger.Debug($"Status: Starting");
                    break;
                case UploadStatus.NotStarted:
                    logger.Debug($"Status: Not Started");
                    break;
                case UploadStatus.Uploading:
                    this.RetriedCount = 0;
                    logger.Debug(String.Format("Status: Uploading {0:N} bytes.", uploadStatusInfo.BytesSent));
                    break;
                case UploadStatus.Failed:
                    ProgressChanged_Failed(uploadStatusInfo);
                    break;
            }
        }
        private void ResponseReceived(File fileResource)
        {
            this.NewFile = fileResource;
        }
        private bool NeedResumeUpload()
        {
            return ((this.RetriedCount < GoogleConstant.MAX_RESUME_RETRIES) && (!this.UploadCancellationToken.IsCancellationRequested));
        }
        private void ProgressChanged_Failed(IUploadProgress uploadStatusInfo)
        {
            if ((!(uploadStatusInfo.Exception is GoogleApiException APIException)) || (APIException.Error == null))
            {
                logger.Error(string.Format("Upload Failed: {0}, Uploading {1:N} bytes.", uploadStatusInfo.Exception.Message, uploadStatusInfo.BytesSent));
                this.RetriedCount = Int32.MaxValue;
                throw new CommonException(uploadStatusInfo.Exception.Message);
            }
            else
            {
                logger.Debug(string.Format("[GoogleApiException]Upload Failed: {0}, Uploading {1:N} bytes.", APIException.Error.ToString(), uploadStatusInfo.BytesSent));
                if (GoogleRequestExtension.NeedRetry(APIException, this.RetriedCount))
                {
                    logger.Warn($"[Drive]Retry count:{this.RetriedCount}. exception:{APIException}.");
                }
                else
                {
                    int StatusCode = (int)APIException.HttpStatusCode;//https://developers.google.com/drive/api/v3/manage-uploads#errors
                    if (StatusCode == 404)
                    {
                        logger.Debug($"Error Code:{StatusCode}.");
                        //TODO:restart upload
                        throw new CommonException(APIException.Error.ToString());
                    }
                    else if (StatusCode == 403 || StatusCode == 410 || StatusCode == 500 || StatusCode == 502 || StatusCode == 503 || StatusCode == 504)
                    {
                        logger.Debug($"Error Code:{StatusCode}, Retried upload count:{this.RetriedCount}.");
                        Thread.Sleep(GoogleRequestExtension.GetThrottlingTime(this.RetriedCount));
                    }
                    else
                    {
                        logger.Error($"Error Code:{StatusCode}, stop upload.");
                        this.RetriedCount = Int32.MaxValue;
                        throw new CommonException(APIException.Error.ToString());
                    }
                }
            }
        }
        internal async Task<Drive> GetDriveAsync(FileQuery query)
        {
            var request = _service.Drives.Get(query?.SharedDriveId);
            request.Fields = "*";
            request.UseDomainAdminAccess = query?.UseDomainAdminAccess;
            var response = await request.ExecuteExAsync();
            return response;
        }
        internal async Task<Drive> CreateDrivesync(Drive drive)
        {
            var request = _service.Drives.Create(drive, Guid.NewGuid().ToString());
            var response = await request.ExecuteExAsync();
            return response;
        }
        internal async Task<Drive> UpdateDriveAsync(Drive drive, string driveId)
        {
            var request = _service.Drives.Update(drive, driveId);
            request.UseDomainAdminAccess = true;
            var response = await request.ExecuteExAsync();
            return response;
        }
        internal async Task<List<Label>> ListFileLabelsAsync(string fileId)
        {
            var result = new List<Label>();
            try
            {
                var request = _service.Files.ListLabels(fileId);
                request.Fields = "*";
                var response = await request.ExecuteExAsync();
                if (response == null)
                {
                    throw new CommonException("There is something wrong when listing Labels, the api response is null");
                }
                if (response.Labels != null)
                {
                    result.AddRange(response.Labels);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Failed to list file labels. fileId: {fileId} Error: {e}");
                throw;
            }
            return result;
        }

        internal async Task<string> GetUserEmailAsync(string fileId, User userInfo)
        {
            try
            {
                if (userInfo == null)
                {
                    return string.Empty;
                }
                if (!string.IsNullOrEmpty(userInfo.EmailAddress))
                {
                    if (!usernameEmailAddressMapping.ContainsKey(userInfo.DisplayName))
                    {
                        usernameEmailAddressMapping.Add(userInfo.DisplayName, userInfo.EmailAddress);
                    }
                    return userInfo.EmailAddress;
                }
                else if (!string.IsNullOrEmpty(userInfo.DisplayName))
                {
                    return await GetUserEmailByDisplayNameAsync(userInfo, fileId);
                }

            }
            catch (Exception ex)
            {
                logger.Debug($"[GetUserEmail] Error: {ex}");
            }
            return string.Empty;
        }

        internal async Task ExportFileToStreamAsync(string fileId, Stream outputStream, string mimeType)
        {
            var request = _service.Files.Export(fileId, mimeType);
            using (var memoryStream = new MemoryStream())
            {
                await request.DownloadAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(outputStream);
                await outputStream.FlushAsync();
            }
        }

        internal async Task<File> CreateFolderAsync(File newItem)
        {
            var request = _service.Files.Create(newItem);
            request.Fields = "*";
            request.SupportsAllDrives = true;

            var result = await request.ExecuteExAsync();
            return result;
        }

        internal async Task<File> UpdateFolderAsync(File updateInfo, string folderId)
        {
            var request = _service.Files.Update(updateInfo, folderId);
            request.Fields = "*";
            request.SupportsAllDrives = true;
            var result = await request.ExecuteExAsync();
            return result;
        }

        private async Task<string> GetUserEmailByDisplayNameAsync(User userInfo, string fileId)
        {
            var emailAddress = string.Empty;
            if (usernameEmailAddressMapping.TryGetValue(userInfo.DisplayName, out emailAddress))
            {
                return emailAddress;
            }
            else if (!string.IsNullOrEmpty(userInfo.PermissionId))
            {
                var permission = await GetFilePermissionAsync(userInfo.PermissionId, fileId);
                if (!string.IsNullOrEmpty(permission.EmailAddress))
                {
                    if (!usernameEmailAddressMapping.ContainsKey(userInfo.DisplayName))
                    {
                        usernameEmailAddressMapping.Add(userInfo.DisplayName, userInfo.EmailAddress);
                    }
                    emailAddress = permission.EmailAddress;
                }
            }
            return emailAddress;
        }

        private async Task<Permission> GetFilePermissionAsync(string permissionId, string fileId)
        {
            var req = _service.Permissions.Get(fileId, permissionId);
            req.Fields = "emailAddress";
            req.SupportsAllDrives = true;
            return await req.ExecuteExAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _service?.Dispose();
            _service = null;
        }

        ~DriveApi()
        {
            Dispose(false);
        }
    }
}
