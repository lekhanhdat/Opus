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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using DocumentFormat.OpenXml.Spreadsheet;
using RADownloadCenter;
using RADownloadCentre.SettingExport.Model;
using System.Text;
using System.Threading.Tasks.Sources;
using RADownloadCentre.SettingExport.Base;
using OpenNLP.Tools.Util;

namespace RADownloadCentre.SettingExport.Helper
{
    public abstract class SettingCsv<T> : IAsyncDisposable
    {
        private readonly RALogger _logger;
        
        private readonly int _countOfOneSheet = 65535;
        
        protected abstract List<string> AssembleSettingHeaderTittle();
        
        protected abstract List<string> ConvertSettingToList(T settings);
        
        private FileStream _fileStream;

        private StreamWriter _writer;
        
        private readonly BaseJobDto _baseJobDto;

        private int _exportedCount;
        
        private int _sheetIndex;
        
        protected SettingCsv(string csvFilePath, BaseJobDto baseJobDto)
        {
            _logger = RALogger.GetInstance(GetType());
            _baseJobDto = baseJobDto;
            _fileStream = new FileStream(csvFilePath, FileMode.Create, FileAccess.Write);
            _writer = new StreamWriter(_fileStream, new UTF8Encoding(true)); // UTF-8 BOM
        }

        public async Task WriteHeaderAsync()
        {
            var headers = AssembleSettingHeaderTittle();
            await _writer.WriteLineAsync(string.Join(',', headers));
            _exportedCount++;

        }
        
        public async Task WriteAsync(T item)
        {
            try
            {
                if (_exportedCount >= _countOfOneSheet)
                {
                    await CreateNewSheet();
                }
                var generatedItem = ConvertSettingToList(item);
                await _writer.WriteLineAsync(StringUtils.ToCSVString(generatedItem?.ToArray() ?? []));
                _exportedCount++;
            }
            catch (Exception ex)
            {
                _logger.Error($"Occured error while writing data to CSV {ex}");
            }
        }

        private async Task CreateNewSheet()
        {
            await DisposeAsync();
            var filePath = JobReportUtility.GetDownloadReportDetailTempleFolder(_baseJobDto, $"_{_sheetIndex++}" + ".csv");
            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            _writer = new StreamWriter(_fileStream, new UTF8Encoding(true));
            _exportedCount = 0;
            await WriteHeaderAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await _fileStream.DisposeAsync();
        }
    }
}
