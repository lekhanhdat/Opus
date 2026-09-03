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
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Duplication
{
    public class RMDiscoveryOffice365IngestorDuplicationReportProcessor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365IngestorDuplicationReportProcessor));
        private Func<IDictionary<string, string>, RMDiscoveryOffice365DuplicationReportInfo> _rowMapper;
        private Func<RMDiscoveryOffice365DuplicationReportInfo, IReadOnlyDictionary<string, string>, RMDiscoveryOffice365DuplicationDataValidationResult> _validator;
        private string _destinationFolderPath;

        private static readonly Dictionary<string, RMDiscoveryOffice365DuplicationDataAction> ActionMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Archive"] = RMDiscoveryOffice365DuplicationDataAction.Archive,
            ["Destroy"] = RMDiscoveryOffice365DuplicationDataAction.Destroy,
            [""] = RMDiscoveryOffice365DuplicationDataAction.Keep
        };

        public RMDiscoveryOffice365IngestorDuplicationReportProcessor(string destinationFolderPath)
        {
            _destinationFolderPath = destinationFolderPath;
            Initialize();
        }

        private void Initialize()
        {
            _logger.Info("Initialize import duplication report processor.");
            _rowMapper = row => new RMDiscoveryOffice365DuplicationReportInfo
            {
                DuplicatedGroup = int.TryParse(row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_DuplicatedGroup")], out var groupIndex) ? groupIndex : 0,
                Name = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileName")],
                ObjectId = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_ItemId")],
                FullUrl = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileUrl")],
                SiteUrl = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_SiteCollection")],
                ModifiedTime4Display = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_LastModifedTime")],
                FileExtension = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileType")],
                VersionSize = row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_VersionSize")],
                Action = TryGetDuplicationDataAction(row[I18NEntity.GetString("RM_JS_JM_Discovery_Report_Action")])
            };
            _validator = OnValidation;
            _logger.Info("Finish initializing import duplication report processor.");
        }

        private RMDiscoveryOffice365DuplicationDataAction TryGetDuplicationDataAction(string rawString)
        {
            if (!ActionMap.TryGetValue(rawString, out var action))
                return RMDiscoveryOffice365DuplicationDataAction.Other;
            return action;
        }

        private RMDiscoveryOffice365DuplicationDataValidationResult OnValidation(RMDiscoveryOffice365DuplicationReportInfo record, IReadOnlyDictionary<string, string> rawRow)
        {
            return record.Action switch
            {
                RMDiscoveryOffice365DuplicationDataAction.Other => RMDiscoveryOffice365DuplicationDataValidationResult.Invalid("The action column value is invalid."),
                _ => RMDiscoveryOffice365DuplicationDataValidationResult.Valid()
            };
        }


        public async IAsyncEnumerable<RMDiscoveryOffice365DuplicationReportInfo> DrainDuplicationReportAsync()
        {
            string[] reportFiles;
            try
            {
                reportFiles = Directory.GetFiles(_destinationFolderPath, "*.csv");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to scan report folder {_destinationFolderPath}. Error: {ex}");
                yield break;
            }

            if (reportFiles.Length == 0)
            {
                _logger.Warn("There is no report file detected so skip draining process.");
                yield break;
            }

            int totalCount = 0;

            foreach (var reportFile in reportFiles)
            {
                var dataIngestor = new RMDiscoveryOffice365DuplicationDataIngestor<RMDiscoveryOffice365DuplicationReportInfo>(reportFile, _rowMapper, _validator);
                foreach (var record in dataIngestor.DrainAllReports())
                {
                    totalCount++;
                    yield return record;
                }
                _logger.Info($"Successfully drained report file: [{Path.GetFileName(reportFile)}] with failed count: [{dataIngestor.FailedCount}]");
            }

            try
            {
                Directory.Delete(_destinationFolderPath, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to delete folder {_destinationFolderPath}. Error: {ex}");
            }
            _logger.Info($"Finish draining report files. Total records: {totalCount}");
        }
    }
}
