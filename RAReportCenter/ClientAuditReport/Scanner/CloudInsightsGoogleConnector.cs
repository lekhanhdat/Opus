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
using Aspose.Pdf.Operators;
using AvePoint.GCommon;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using Azure.Storage.Blobs;
using Google.Cloud.Storage.V1;
using Media.Common.ClassicStorageApi;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    public class CloudInsightsGoogleConnector : CloudInsightsConnectorBase
    {
        private static readonly IRALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IXSystem mStorageSystem;
        private string mStorageXri;
        private DateTime mStartTime;
        private DateTime mEndTime;
        private bool SPAuditPacketStorage;
        private List<string> nodes = new();

        public CloudInsightsGoogleConnector(string jobid, string storageXri, DateTime startTime, DateTime endTime) : base(jobid)
        {
            this.mStorageXri = storageXri;
            this.mStartTime = startTime;
            this.mEndTime = endTime;
            InitStorageSystem();
        }

        public override void SetAuditPacketStore(bool packetStore, List<string> siteUrls)
        {
            SPAuditPacketStorage = packetStore;
            nodes = siteUrls;
        }

        public override void Run()
        {
            var prefixes = new List<string>();
            mLog.Info($"[ListStorageItems] Sharepoint audit packet storage {SPAuditPacketStorage.ToString()}");
            if (SPAuditPacketStorage)
            {
                var scPrefixes = GetSCFirstTwoLetters(nodes);
                mLog.Info($"Prefixes : {string.Join(",", scPrefixes)}");
                foreach (var sc in scPrefixes)
                {
                    prefixes.Add($"SPAudit/{sc}");
                }
                prefixes.Add("O365Audit/SharePoint");
            }
            else
            {
                prefixes.Add("O365Audit/SharePoint");
            }

            var filesToProcess = new List<StorageInfo>();

            foreach (var prefix in prefixes)
            {
                var tenantPrefix = TenantLocalValue.LogonGroupId +"/" + prefix;
                mLog.Info($"[ListStorageItems] prefix to get list files '{tenantPrefix}'");
                CollectFilesByTimeRange(tenantPrefix, filesToProcess).GetAwaiter().GetResult();
                mLog.Info($"get Storages count is {filesToProcess.Count}");
            }

            if (SetRportCount != null)
            {
                SetRportCount(this, new SetCalculatedCountEventArgs() { Count = filesToProcess.Count });
            }

            foreach (var file in filesToProcess)
            {
                try
                {
                    mLog.Info($"[Run] Processing file: {file.HighName}/{file.LowName}");
                    string tempFolder = GetNewFolderInTemp();
                    string zipPath = System.IO.Path.Combine(tempFolder, $"SharePoint_{Guid.NewGuid()}.zip");

                    AveRetryPolicy.DefaultExponential.ExecuteAction((Action)(() =>
                    {
                        using (var fileStream = new FileStream(zipPath, FileMode.Create))
                        {
                            this.mStorageSystem.DownloadFile(file, (Stream)fileStream);
                        }
                    }));

                    mLog.Info($"[Run] File downloaded successfully to: {zipPath}");

                    var unzipWatch = Stopwatch.StartNew();
                    ZipUtil.UnZipFile(zipPath, tempFolder);
                    unzipWatch.Stop();

                    File.Delete(zipPath);
                    mLog.Info($"[Run] Unzip completed in {unzipWatch.Elapsed.TotalSeconds:F2} seconds");

                    Add2Queue(new AuditDownloadDataInfo { FileFolder = tempFolder });
                    IncreaseProgress?.Invoke(this, new IncreaseProgressEventArgs());
                }
                catch (Exception ex)
                {
                    IncreaseProgress?.Invoke(this, new IncreaseProgressEventArgs() { HasError = true });
                    mLog.Error($"[Run] Failed processing file '{file.LowName}': {ex}");
                }
            }
        }

        private async Task CollectFilesByTimeRange(string prefix, List<StorageInfo> resultFiles)
        {
            try
            {
                var subDirectories = await mStorageSystem.ListDirectoryAsync(new StorageInfo { HighName = prefix });
                
                mLog.Info($"[Collect] In prefix '{prefix}' returned {subDirectories.Count} sub directories");
                
                mLog.Info($@"[Collect] Sub directories: Name: {string.Join(", ", subDirectories.Select(d => d.LowName))}");

                if (TryParseDate(prefix))
                {
                    var files = await mStorageSystem.ListFileAsync(new StorageInfo { HighName = prefix });
                    mLog.Info($"[Collect] In prefix '{prefix}' returned {files.Count} files");

                    resultFiles.AddRange(files);
                }
                
                foreach (var subDir in subDirectories)
                {
                    mLog.Info($"[Collect] Sub directory: {subDir.HighPlusLowName}");
                    await CollectFilesByTimeRange(subDir.HighPlusLowName, resultFiles);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"[Collect] Failed to collect files in prefix {prefix}: {ex}");
            }
        }

        private bool TryParseDate(string path)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 6 &&
                int.TryParse(parts[3], out int year) &&
                int.TryParse(parts[4], out int month) &&
                int.TryParse(parts[5], out int day))
            {
                try
                {
                    var date = new DateTime(year, month, day);
                    mLog.Info($"[Deploy] Convert to Date Successfully ! {date} ticks: {date.Ticks}");
                    if (date.Ticks >= mStartTime.Ticks && date.Ticks <= mEndTime.Ticks)
                    {                    
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error($"[Deploy] Convert to Date Fail ! : {ex}");
                    return false;
                }
            }

            return false;
        }

        public override Dictionary<string, string> GetUserMappings()
        {
            throw new NotImplementedException();
        }

        private void InitStorageSystem()
        {
            try
            {
                mStorageSystem = XFactoryCommon.InstanceSystem(mStorageXri);

                mStorageSystem.Open();
                var result = mStorageSystem.Validate();

                if (result == null)
                {
                    mLog.Error($"[Validate] Validation returned null. Init failed for: {mStorageXri}");
                    return;
                }

                mLog.Info($"[Validate] Permission: Read = {result.IsReadAble}, Write = {result.IsWriteAble}, Delete = {result.IsDeleteAble}");
            }
            catch (Exception e)
            {
                mLog.Error($"[Init] Failed to initialize Google storage from XRI: {e}");
            }
        }
    }
}
