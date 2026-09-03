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
using AvePoint.RA.Contract.Services;
using RAGoogle.Common;
using RAGoogle.Models;
using RAGoogle.Services;
using RAGoogle.Util;
using System.Diagnostics;

namespace RAGoogle.RecordsDisposal
{
    public class DownloadProcessor
    {
        #region properties
        private IRALogger _logger = RALogger.GetInstance(typeof(DownloadProcessor));
        private GoogleConfiguration _configuration;
        #endregion

        public DownloadProcessor(GoogleConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> ProcessDownloadItemAsync(GoogleItemData item)
        {
            using (PerformanceScope performance = new("MoveToController:ProcessDownloadItem", "", true))
            using (GoogleDriveService service = new(_configuration.AppProfile, item.MemberEmail))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                _logger.Info("Start process download item '{0}'.", item.Name);
                string filePath = GenerateLocalTempPath(item);
                if (item.Size == 0)
                {
                    using (System.IO.File.Create(filePath))
                    {
                        _logger.Info("The item '{0}' has file size = 0. Create empty file in '{1}'.", item.Name, filePath);
                    }
                }
                else
                {
                    try
                    {
                        if (GoogleConstant.GoogleExportMimeType.TryGetValue(item.MimeType, out string? mimeType))
                        {
                            mimeType ??= "application/zip";
                            await service.ExportFileAsync(item.Id, filePath, mimeType);
                            _logger.Info("File '{0}' is Google Workspace files type '{1}. " +
                                "Export file successfully. local temp path: {2}", item.Name, item.MimeType, filePath);
                        }
                        else if (GoogleConstant.GoogleVideoMimeType.Contains(item.MimeType))
                        {
                            //await service.DownloadMediaAsync(item.Id, filePath);
                            _logger.Info("Download media '{0}' successfully. Local temp path: {1}", item.Name, filePath);
                        }
                        else
                        {
                            await service.DownloadFileAsync(item.Id, filePath);
                            _logger.Info("Download file '{0}' successfully. Local temp path: {1}", item.Name, filePath);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error("Error occurred while downloading file '{0}'. Inner exception: {1}", item.Name, e.ToString());
                        sw.Stop();
                        throw;
                    }
                }
                sw.Stop();
                _logger.Info("Finish process download item '{0}'. Cost: '{1}' ms.", item.Name, sw.ElapsedMilliseconds);
                return filePath;
            }
        }

        private string GenerateLocalTempPath(GoogleItemData file)
        {
            string folderPath = GooglePathUtil.GenerateDisposalTempPath(_configuration.JobId);
            string filePath = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(file.Name) + $"_{file.Id}" + $".dat");
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    _logger.Info("Create temp folder. Path: {0}", folderPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot create temp folder `{0}`. Inner exception: {1}", folderPath, ex.ToString());
                throw;
            }
            return filePath;
        }
    }
}
