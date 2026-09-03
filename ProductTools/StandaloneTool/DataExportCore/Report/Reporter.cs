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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using DataExportCore.Cache;
using DataExportCore.Discover.Node;
using DataExportCore.Enum;
using DataExportCore.Report;
using DataExportCore.Utils;
using Storage;
using System.Text;
using static DataExportCore.SizeUtil;

namespace DataExportCore;

public class Reporter : IReporter
{
    private RALogger logger = RALogger.GetInstance(typeof(Reporter));
    public Guid ReportId { get; private set; }
    public event Action<double, string> ProgressChanged;
    public event Action<JobStatus, long> OnCompleted;

    Int64 totalCount;
    Int64 errorCount;
    Int64 skipCount;
    Int64 successfulCount;
    Int64 totalExportedSize;

    private string _currentFile = string.Empty;

    public string CurrentFile
    {
        get => _currentFile;
        set => Interlocked.Exchange(ref _currentFile, value);
    }

    Int32 _currentProgress;
    const Int32 PROGRESS_LIMIT = 100;
    bool isCompleted;

    readonly List<ExportDetailEntity> reports = new();
    private string _reportLocation;
    private readonly List<string> _reportHeader = new List<string>
    {
        I18NEntity.GetString("SATool_ReportHeader_DataType"),
        I18NEntity.GetString("SATool_ReportHeader_ObjectLevel"),
        I18NEntity.GetString("SATool_ReportHeader_Name"),
        I18NEntity.GetString("SATool_ReportHeader_SourceURL"),
        I18NEntity.GetString("SATool_ReportHeader_TargeURL"),
        string.Format(I18NEntity.GetString("SATool_ReportHeader_ObjectSize"),SizeUnit.KB),
        I18NEntity.GetString("SATool_ReportHeader_Status"),
        I18NEntity.GetString("SATool_ReportHeader_Comment"),
    };
    private readonly string REPORT_FILENAME = I18NEntity.GetString("SATool_Report_Name");
    private int _reportFileCount = 0;
    private string _siteExportPath = string.Empty;
    private bool _isNeedUpload = false;
    private DataType _dataType;
    const Int32 BATCH_SITE_LIMIT = 5000;

    Task updateProgressTask;
    static readonly object lockObj = new object();

    public Reporter(Guid reportId, string reportLocation, DataType dataType)
    {
        ReportId = reportId;
        _dataType = dataType;
        _reportLocation = reportLocation;
    }

    private async Task UpdateProgress()
    {
        try
        {
            do
            {
                GetCurrentProgress();
                ProgressChanged?.Invoke(this._currentProgress, this._currentFile);
                if (!isCompleted) await Task.Delay(200);
            }
            while (!isCompleted);
            logger.Info($"Finish to update progress, final progress [{this._currentProgress}%].");
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to update progress, Error: {ex}");
        }
    }

    public void RecordFailed(DiscoverNode node, string comment)
    {
        Interlocked.Increment(ref errorCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Failed, _dataType, comment));
    }

    public void RecordFailed(ExchangeDiscoverNode node, string comment,string teamsGroupAddress = "", string mailBoxName = "")
    {
        Interlocked.Increment(ref errorCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Failed, _dataType, comment, teamsGroupAddress, mailBoxName));
    }

    public void RecordFailed(TeamsDiscoveryNode node, string comment, string teamsGroupAddress = "")
    {
        Interlocked.Increment(ref errorCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Failed, _dataType, comment, teamsGroupAddress));
    }

    public void RecordSuccessful(DiscoverNode node, string comment = "")
    {
        Interlocked.Add(ref totalExportedSize, node.ExportedFileSize);
        Interlocked.Increment(ref successfulCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Successful, _dataType, comment));
    }

    public void RecordSuccessful(ExchangeDiscoverNode node, string exportPath = "", string comment = "", string teamsGroupAddress = "", string mailBoxName = "")
    {
        Interlocked.Add(ref totalExportedSize, node.ExportedFileSize);
        Interlocked.Increment(ref successfulCount);
        node.ExportPath = string.IsNullOrEmpty(exportPath) ? ExportUtility.BuildTargetUrl(node.ExportPath) : ExportUtility.BuildTargetUrl(exportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Successful, _dataType, comment, teamsGroupAddress, mailBoxName));
    }

    public void RecordSuccessful(TeamsDiscoveryNode node, string comment = "", string teamsGroupAddress = "")
    {
        Interlocked.Add(ref totalExportedSize, node.ExportedFileSize);
        Interlocked.Increment(ref successfulCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Successful, _dataType, comment, teamsGroupAddress));
    }

    public void RecordSkipped(DiscoverNode node, string comment = "")
    {
        Interlocked.Increment(ref skipCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Skipped, _dataType, comment));
    }

    public void RecordSkipped(ExchangeDiscoverNode node, string comment = "", string teamsGroupAddress = "", string mailBoxName = "")
    {
        Interlocked.Increment(ref skipCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Skipped, _dataType, comment, teamsGroupAddress, mailBoxName));
    }


    public void RecordSkipped(TeamsDiscoveryNode node, string comment = "", string teamsGroupAddress = "")
    {
        Interlocked.Increment(ref skipCount);
        node.ExportPath = ExportUtility.BuildTargetUrl(node.ExportPath);
        RecordDetail(node.ConvertToExportDetail(ExportStatus.Skipped, _dataType, comment, teamsGroupAddress));
    }

    void RecordDetail(ExportDetailEntity detail)
    {
        List<ExportDetailEntity>? tempReports = null;
        lock (lockObj)
        {
            reports.Add(detail);
            logger.Info($"Added export detail, name [{detail.Name}], status [{detail.Status}], comment [{detail.Comment}].");
            if (reports.Count >= BATCH_SITE_LIMIT)
            {
                tempReports = new List<ExportDetailEntity>(reports);
                reports.Clear();
            }
        }
        if (tempReports != null && tempReports.Count > 0)
        {
            this.RecordToFile(tempReports);
        }
    }

    public void ConfigForReport(string siteExportPath, bool isNeedUpload)
    {
        _isNeedUpload = isNeedUpload;
        if (isNeedUpload)
        {
            _siteExportPath = siteExportPath;
        }
        logger.Info($"config for report site path [{siteExportPath}], isNeedUpload [{isNeedUpload}]");
    }

    void RecordToFile(List<ExportDetailEntity> batchReports)
    {
        try
        {
            var csvContent = new StringBuilder();
            csvContent.AppendLine(string.Join(",", _reportHeader));
            var reportNameWithExt = string.Empty;

            if (_reportFileCount == 0)
            {
                reportNameWithExt = REPORT_FILENAME + ".csv";
            }
            else
            {
                reportNameWithExt = REPORT_FILENAME + _reportFileCount + ".csv";
            }
            Interlocked.Increment(ref _reportFileCount);
            foreach (var row in batchReports)
            {
                if (row != null)
                {
                    var rowContent = string.Join(",", row.GetType()
                        .GetProperties()
                        .Select(p =>
                        {
                            var value = p.GetValue(row)?.ToString() ?? string.Empty;

                            if (value.Contains(",") || value.Contains("\""))
                            {
                                value = $"\"{value.Replace("\"", "\"\"")}\"";
                            }

                            return value.Trim('\\').Trim('/');
                        }));
                    csvContent.AppendLine(rowContent);
                }
            }

            byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvContent.ToString())).ToArray();
            int bufferSize = 64 * 1024;
            int offset = 0;

            if (_isNeedUpload)
            {
                _siteExportPath = _siteExportPath.Trim('\\');
                StorageInfo info = XConvert.FromNames(ExportUtility.ReplaceInvalidChar(_siteExportPath, false), reportNameWithExt);

                using (XStream stream = GlobalDeviceCache.GetDestinationDevice().OpenStream(info, FileMode.OpenOrCreate))
                {
                    while (offset < buffer.Length)
                    {
                        int bytesToWrite = Math.Min(bufferSize, buffer.Length - offset);
                        stream.Write(buffer, offset, bytesToWrite);
                        offset += bytesToWrite;
                    }
                }
                logger.Info($"CSV file was successfully written to: {new Uri(Path.Combine(GlobalDeviceCache.GetDestinationDevice().SystemPath, _siteExportPath)).AbsoluteUri}");
            }
            else
            {
                if(!Directory.Exists(_reportLocation))
                {
                    Directory.CreateDirectory(_reportLocation);
                }
                using (var fileStream = new FileStream(Path.Combine(_reportLocation, reportNameWithExt), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
                {
                    while (offset < buffer.Length)
                    {
                        int bytesToWrite = Math.Min(bufferSize, buffer.Length - offset);
                        fileStream.Write(buffer, offset, bytesToWrite);
                        offset += bytesToWrite;
                    }
                }

                logger.Info($"CSV file was successfully written to: {_reportLocation}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while writing report details to CSV file. Error: {ex}");
        }
    }

    public void StartProgress(Int64 totalCount)
    {
        this.totalCount = totalCount;
        if (this.updateProgressTask == null || this.updateProgressTask.IsCompleted)
        {
            this.updateProgressTask = Task.Run(() => UpdateProgress());
        }
    }

    public void Complete()
    {
        var count = reports.Count;
        if (reports.Count > 0)
        {
            var tempReports = new List<ExportDetailEntity>(reports);
            RecordToFile(tempReports);
            reports.Clear();
        }
        this.isCompleted = true;
        this.OnCompleted.Invoke(GetJobStatus(), totalExportedSize);
        logger.Info($"Flush all reports successfully, count {count}");
    }


    public JobStatus GetJobStatus()
    {
        if (errorCount > 0)
        {
            if (successfulCount > 0)
            {
                return JobStatus.FinishWithException;
            }
            else
            {
                return JobStatus.Failed;
            }
        }
        else if (successfulCount > 0)
        {
            return JobStatus.Finished;
        }

        return JobStatus.Finished;
    }

    public void GetCurrentProgress()
    {
        var totalProcessed = successfulCount + errorCount + skipCount;

        var localProgress = this._currentProgress;

        if (localProgress >= PROGRESS_LIMIT) return;

        localProgress = (Int32)(totalProcessed * 100 / this.totalCount);

        if (localProgress > this._currentProgress)
        {
            this._currentProgress = localProgress;
        }
    }
}
