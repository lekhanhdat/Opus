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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Export.Duplication
{
    public class RMDiscoveryOffice365DuplicationDataIngestor<T>
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DuplicationDataIngestor<T>));
        private readonly string _filePath;
        private readonly Func<IDictionary<string, string>, T> _rowMapper;
        private readonly Func<T, IReadOnlyDictionary<string, string>, RMDiscoveryOffice365DuplicationDataValidationResult> _validator;
        private long _failedCount = 0;

        public RMDiscoveryOffice365DuplicationDataIngestor(string filePath,
            Func<IDictionary<string, string>, T> rowMapper,
            Func<T, IReadOnlyDictionary<string, string>, RMDiscoveryOffice365DuplicationDataValidationResult> validator)
        {
            _filePath = filePath;
            _rowMapper = rowMapper;
            _validator = validator;
        }

        public long FailedCount => _failedCount;

        public IEnumerable<T> DrainAllReports()
        {
            using var reader = new StreamReader(_filePath);

            string[] headers = null;
            int rowNumber = 0;

            var headerLine = reader.ReadLine();
            headers = ParseLine(headerLine).ToArray();
            if(headers.Count() <= 0)
            {
                _logger.Error("No headers found in the CSV file.");
                yield break;
            }

            rowNumber++;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = ParseLine(line).ToArray();
                var rawRow = new Dictionary<string, string>(headers.Length);

                for (int i = 0; i < headers.Length; i++)
                    rawRow[headers[i]] = i < values.Length ? values[i] : string.Empty;

                T record;
                try
                {
                    record = _rowMapper(rawRow);
                }
                catch (Exception ex) 
                {  
                    _logger.Error($"Failed to map data at row {rowNumber}. Error: {ex}. \n Row Data: {FormatRawRow(rawRow)}");
                    continue;
                }

                if (_validator != null)
                {
                    var validationRes = _validator(record, null);
                    if (!validationRes.IsValid)
                    {
                        _logger.Error($"Data validation failed at row {rowNumber}: {validationRes.ErrorMessage}. \n Row Data: {FormatRawRow(rawRow)}");
                        _failedCount++;
                         continue;
                    }
                }
                yield return record;
            }
        }

        private static IEnumerable<string> ParseLine(string line, char separator = ',')
        {
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == separator && !inQuotes)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            yield return sb.ToString();
        }

        private string FormatRawRow(IReadOnlyDictionary<string, string> row)
        {
            return string.Join(", ", row.Select(kv => $"{kv.Key}=\"{kv.Value}\""));
        }
    }

    public sealed class RMDiscoveryOffice365DuplicationDataValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private RMDiscoveryOffice365DuplicationDataValidationResult(bool isValid, string error)
        {
            IsValid = isValid;
            ErrorMessage = error;
        }

        public static RMDiscoveryOffice365DuplicationDataValidationResult Valid() => new(true, null);

        public static RMDiscoveryOffice365DuplicationDataValidationResult Invalid(string error) => new(false, error);
    }
}
