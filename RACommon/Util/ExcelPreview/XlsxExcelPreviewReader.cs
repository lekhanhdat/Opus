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
using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvePoint.RA.Common.Util.ExcelPreview
{
    internal sealed class XlsxExcelPreviewReader : LightCellsDataHandler, IExcelPreviewReader
    {
        private readonly List<string[]> _rows = new List<string[]>();
        private readonly LoadFormat _loadFormat;
        private Dictionary<int, string> _currentValues = new Dictionary<int, string>();
        private string _firstSheetName;
        private int _headerColumnCount;
        private int _currentRowIndex = -1;
        private int _lastMaterializedRowIndex = -1;
        private bool _readingFirstSheet;
        private bool _shouldStop;

        public XlsxExcelPreviewReader(string extension)
        {
            _loadFormat = GetLoadFormat(extension);
        }

        public ExcelPreviewSheetData Read(Stream stream)
        {
            Reset();
            _firstSheetName = GetFirstSheetName(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var loadOptions = new LoadOptions(_loadFormat)
            {
                LightCellsDataHandler = this
            };

            using (var workbook = new Workbook(stream, loadOptions))
            {
            }

            FinalizeCurrentRow();

            if (_rows.Count == 0 || _headerColumnCount == 0)
            {
                return new ExcelPreviewSheetData(new string[0], new List<string[]>());
            }

            return new ExcelPreviewSheetData(_rows[0], _rows.Skip(1).Take(50).ToList());
        }

        public bool StartSheet(Worksheet sheet)
        {
            _readingFirstSheet = string.Equals(sheet.Name, _firstSheetName, StringComparison.Ordinal);
            return _readingFirstSheet;
        }

        public bool StartRow(int rowIndex)
        {
            if (!_readingFirstSheet || _shouldStop)
            {
                return false;
            }

            FinalizeCurrentRow();
            AppendMissingRowsUntil(rowIndex);
            if (_shouldStop || rowIndex > 50)
            {
                _shouldStop = true;
                return false;
            }

            _currentRowIndex = rowIndex;
            _currentValues.Clear();
            return true;
        }
        public bool ProcessRow(Row row)
        {
            return !_shouldStop;
        }

        public bool StartCell(int columnIndex)
        {
            return _readingFirstSheet && !_shouldStop;
        }

        public bool ProcessCell(Cell cell)
        {
            _currentValues[cell.Column] = cell.StringValue;
            return false;
        }

        private void Reset()
        {
            _rows.Clear();
            _currentValues.Clear();
            _firstSheetName = null;
            _headerColumnCount = 0;
            _currentRowIndex = -1;
            _lastMaterializedRowIndex = -1;
            _readingFirstSheet = false;
            _shouldStop = false;
        }

        private void AppendMissingRowsUntil(int nextRowIndex)
        {
            if (_headerColumnCount == 0 || _lastMaterializedRowIndex < 0)
            {
                return;
            }

            for (var rowIndex = _lastMaterializedRowIndex + 1; rowIndex < nextRowIndex && rowIndex <= 50; rowIndex++)
            {
                _rows.Add(new string[_headerColumnCount]);
                _lastMaterializedRowIndex = rowIndex;
                if (_rows.Count == 51)
                {
                    _shouldStop = true;
                    return;
                }
            }
        }

        private void FinalizeCurrentRow()
        {
            if (_currentRowIndex < 0)
            {
                return;
            }

            if (_currentRowIndex == 0)
            {
                _headerColumnCount = _currentValues
                    .Where(item => !string.IsNullOrEmpty(item.Value))
                    .Select(item => item.Key + 1)
                    .DefaultIfEmpty(0)
                    .Max();
            }

            if (_headerColumnCount == 0)
            {
                _currentRowIndex = -1;
                _currentValues.Clear();
                return;
            }

            var row = new string[_headerColumnCount];
            foreach (var item in _currentValues)
            {
                if (item.Key < _headerColumnCount)
                {
                    row[item.Key] = item.Value;
                }
            }

            _rows.Add(row);
            _lastMaterializedRowIndex = _currentRowIndex;
            _shouldStop = _rows.Count == 51 || _currentRowIndex >= 50;
            _currentRowIndex = -1;
            _currentValues.Clear();
        }

        private string GetFirstSheetName(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var workbook = new Workbook(stream, new LoadOptions(_loadFormat)
            {
                LoadFilter = new LoadFilter(LoadDataFilterOptions.Structure)
            }))
            {
                return workbook.Worksheets.Select(sheet => sheet.Name).FirstOrDefault();
            }
        }

        private static LoadFormat GetLoadFormat(string extension)
        {
            switch (extension.Trim('.').ToLowerInvariant())
            {
                case "xlsx":
                    return LoadFormat.Xlsx;
                case "xls":
                    return LoadFormat.Excel97To2003;
                case "xlsm":
                    return LoadFormat.Xlsx;
                case "xlsb":
                    return LoadFormat.Xlsb;
                case "xltx":
                    return LoadFormat.Xlsx;
                case "xlt":
                    return LoadFormat.Excel97To2003;
                default:
                    throw new ArgumentException("Unsupported Excel extension '" + extension + "'.", nameof(extension));
            }
        }
    }
}
