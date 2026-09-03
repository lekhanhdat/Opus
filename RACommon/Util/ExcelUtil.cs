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
using AvePoint.RA.Common.Util.ExcelPreview;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.CustomProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AvePoint.RA.Common.Util
{
    public class ExcelUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ExcelUtil));

        public static int GetColumnIndexFromName(string columnName)
        {
            string name = columnName;
            int number = 0;
            int pow = 1;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                //number += (name[i] - 'A') * pow;
                pow *= 26;
            }
            number = number > 0 ? number - 1 : number;
            return number;
        }

        public static string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);

            return match.Value;
        }

        private static String GetValue(UInt32Value rowNumber, int columnNumber, Cell cell, SharedStringTablePart stringTablePart, Stylesheet styleSheet, bool isFSValues = false)
        {
            try
            {
                if (cell.ChildElements.Count == 0)
                    return string.Empty;

                var cellInnerText = cell.CellValue.InnerText.Trim();

                if (cell.DataType != null)
                {
                    if (cell.DataType == CellValues.SharedString)
                    {
                        return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(cell.CellValue.InnerText.Trim())].InnerText.Trim();
                    }
                    else if (cell.DataType == CellValues.Boolean)
                    {
                        if (cell.CellValue.InnerText.Trim().Equals("1", StringComparison.OrdinalIgnoreCase))
                        {
                            return "true";
                        }
                        else
                        {
                            return "false";
                        }
                    }
                }
                else
                {
                    if (cell.StyleIndex != null && !isFSValues && IsCellFormattedAsDate(cell, styleSheet))
                    {
                        if (TryParseOADateTime(cellInnerText, rowNumber, columnNumber, cell.StyleIndex, out DateTime? date))
                        {
                            return date?.ToString();
                        }
                    }
                }
                return cellInnerText;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while reading at row {rowNumber}, column {columnNumber}. StyleIndex:[{cell.StyleIndex}], InnerText:[{cell.CellValue?.InnerText}], Error: {ex}");
                throw;
            }
        }

        private static bool TryParseOADateTime(string cellText, UInt32Value rowNumber, int colNumber, string styleIndex, out DateTime? result)
        {
            try
            {
                result = DateTime.FromOADate(double.Parse(cellText));
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while parsing OADate at row {rowNumber}, column {colNumber}, styleIndex:[{styleIndex}], innerText:[{cellText}] error: {ex}");
                result = null;
                return false;
            }
        }

        public static string[] GetRowValue(Row row, int columnCount, SharedStringTablePart stringTablePart, Stylesheet styleSheet, bool isFSValue = false)
        {
            string[] values = new string[columnCount];
            foreach (var cell in row.Descendants<Cell>())
            {
                int index = GetColumnIndexFromName(GetColumnName(cell.CellReference));
                if (index < columnCount)
                {
                    var columnNumber = index + 1;
                    values[index] = GetValue(row.RowIndex, columnNumber, cell, stringTablePart, styleSheet, isFSValue);
                }
            }
            return values;
        }

        public static int GetSheetCount(Stream stream)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                var sheetCount = sheets.Elements<Sheet>().Count();
                return sheetCount;
            }
        }

        public static Dictionary<string, List<string[]>> ReadExcelHeader(Stream stream, Dictionary<string, int> sheetNameCountDic = null)
        {
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                var styleSheet = workBookPart.WorkbookStylesPart?.Stylesheet;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    List<string[]> sheetResult = new List<string[]>();
                    WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                    int columnCount = sheetRows.First().Descendants<Cell>().Count();
                    if (sheetNameCountDic != null && sheetNameCountDic.ContainsKey(sheet.Name))
                    {
                        columnCount = sheetNameCountDic[sheet.Name];
                    }
                    if (sheetRows.Any())
                    {
                        Row firstRow = sheetRows.First();
                        if (firstRow != null)
                        {
                            sheetResult.Add(GetRowValue(firstRow, columnCount, stringTablePart, styleSheet, true));
                        }
                    }
                    if (!result.ContainsKey(sheet.Name))
                    {
                        result.Add(sheet.Name, sheetResult);
                    }
                }

                spreadSheet.Dispose();
            }

            return result;
        }

        public static Dictionary<string, List<string[]>> ReadExcel(Stream stream, Dictionary<string, int> sheetNameCountDic = null)
        {
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                var styleSheet = workBookPart.WorkbookStylesPart?.Stylesheet;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    List<string[]> sheetResult = new List<string[]>();
                    WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                    int columnCount = sheetRows.First().Descendants<Cell>().Count();
                    if (sheetNameCountDic != null && sheetNameCountDic.ContainsKey(sheet.Name))
                    {
                        columnCount = sheetNameCountDic[sheet.Name];
                    }
                    int rowIndex = 0;
                    foreach (Row row in sheetRows)
                    {
                        if (rowIndex >= 1)
                        {
                            sheetResult.Add(GetRowValue(row, columnCount, stringTablePart, styleSheet, true));
                        }
                        rowIndex++;
                    }
                    if (!result.ContainsKey(sheet.Name))
                    {
                        result.Add(sheet.Name, sheetResult);
                    }
                }

                spreadSheet.Dispose();
            }

            return result;
        }
        public static ManualApprovalCountResult ReadExcelRowCount(Stream stream)
        {
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();


            var waitingI18N = I18NEntity.GetString("RM_DAM_ManualApproval_WaitingApproveStatus", new CultureInfo(1033)).ToLower();
            var approvedI18N = I18NEntity.GetString("RM_MA_Approve", new CultureInfo(1033)).ToLower();
            var rejectedI18N = I18NEntity.GetString("RM_MA_Reject", new CultureInfo(1033)).ToLower();
            var statusI18Ns = new HashSet<string>
                {
                    waitingI18N,
                    approvedI18N,
                    rejectedI18N
                };

            var manualCountResult = new ManualApprovalCountResult();

            using var reader = new StreamReader(stream);

            var line = string.Empty;
            var rowStr = string.Empty;
            var special = false;
            while (!string.IsNullOrWhiteSpace((line = reader.ReadLine())))
            {
                rowStr += line;
                int remainder = (line.Split(new char[] { '"' }, StringSplitOptions.None).Length - 1) % 2;
                if (remainder != 0)
                {
                    if (special)
                    {
                        special = false;
                    }
                    else
                    {
                        rowStr += System.Environment.NewLine;
                        special = true;
                        continue;
                    }
                }
                else
                {
                    if (special)
                    {
                        rowStr += System.Environment.NewLine;
                        continue;
                    }
                }

                var lineFields = CSVHelper.AnalyseCSVRow2ArrayForManualImport(rowStr);
                rowStr = null;
                if (!lineFields.Any())
                {
                    continue;
                }

                var value = lineFields[0]?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(value) || !statusI18Ns.Contains(value))
                {
                    continue;
                }

                manualCountResult.TotalCount++;

                if (value == approvedI18N)
                {
                    manualCountResult.ApproveCount++;
                }
                else if (value == rejectedI18N)
                {
                    manualCountResult.RejectCount++;
                }
            }
            return manualCountResult;
        }


        public static Dictionary<string, List<string[]>> ReadExcelForFS(Stream stream, Dictionary<string, int> sheetNameCountDic = null)
        {
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                var styleSheet = workBookPart.WorkbookStylesPart?.Stylesheet;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    List<string[]> sheetResult = new List<string[]>();
                    WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                    int columnCount = 8;
                    if (sheetNameCountDic != null && sheetNameCountDic.ContainsKey(sheet.Name))
                    {
                        columnCount = sheetNameCountDic[sheet.Name];
                    }
                    int rowIndex = 0;
                    foreach (Row row in sheetRows)
                    {
                        if (rowIndex == 0)
                        {
                            sheetResult.Add(GetRowValue(row, 2, stringTablePart, styleSheet, true));
                        }
                        if (rowIndex >= 1)
                        {
                            sheetResult.Add(GetRowValue(row, columnCount, stringTablePart, styleSheet));
                        }
                        rowIndex++;
                    }
                    if (!result.ContainsKey(sheet.Name))
                    {
                        result.Add(sheet.Name, sheetResult);
                    }
                }

                spreadSheet.Dispose();
            }

            return result;
        }

        public static Dictionary<string, List<string[]>> ReadExcelForFSJPMC(Stream stream, Dictionary<string, int> sheetNameCountDic = null)
        {
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                var styleSheet = workBookPart.WorkbookStylesPart?.Stylesheet;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    List<string[]> sheetResult = new List<string[]>();
                    WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                    int columnCount = 13;
                    if (sheetNameCountDic != null && sheetNameCountDic.ContainsKey(sheet.Name))
                    {
                        columnCount = sheetNameCountDic[sheet.Name];
                    }
                    int rowIndex = 0;
                    foreach (Row row in sheetRows)
                    {
                        if (rowIndex == 0)
                        {
                            sheetResult.Add(GetRowValue(row, 2, stringTablePart, styleSheet, true));
                        }
                        if (rowIndex >= 1)
                        {
                            sheetResult.Add(GetRowValue(row, columnCount, stringTablePart, styleSheet));
                        }
                        rowIndex++;
                    }
                    if (!result.ContainsKey(sheet.Name))
                    {
                        result.Add(sheet.Name, sheetResult);
                    }
                }

                spreadSheet.Dispose();
            }

            return result;
        }
        public static Dictionary<string, List<string[]>> ReadExcelWithHeader(Stream stream, int skipRowsCount = 0)
        {
            Dictionary<string, List<string[]>> result = new Dictionary<string, List<string[]>>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                var styleSheet = workBookPart.WorkbookStylesPart?.Stylesheet;
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    var sheetname = sheet.Name;
                    logger.Info($"Starting to process sheet [{sheetname}]");
                    List<string[]> sheetResult = new List<string[]>();
                    WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                    int columnCount = sheetRows.Skip(skipRowsCount).First().Descendants<Cell>().Count();
                    foreach (Row row in sheetRows)
                    {
                        sheetResult.Add(GetRowValue(row, columnCount, stringTablePart, styleSheet));
                    }
                    if (!result.ContainsKey(sheet.Name))
                    {
                        result.Add(sheet.Name, sheetResult);
                    }
                    logger.Info($"[{sheetname}] has been processed.");
                }

                spreadSheet.Dispose();
            }

            return result;
        }
        public static List<string[]> ReadExcel(Stream stream, string sheetName, bool skipFirstRow)
        {
            List<string[]> result = new List<string[]>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                var styleSheet = workBookPart.WorkbookStylesPart.Stylesheet;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    if (string.Equals(sheetName, sheet.Name, StringComparison.OrdinalIgnoreCase))
                    {

                        WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                        var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                        int columnCount = sheetRows.First().Descendants<Cell>().Count();
                        int rowIndex = 0;
                        foreach (Row row in sheetRows)
                        {
                            if (rowIndex == 0 && skipFirstRow)
                            {
                                continue;
                            }
                            result.Add(GetRowValue(row, columnCount, stringTablePart, styleSheet));
                            rowIndex++;
                        }
                    }
                }
                spreadSheet.Dispose();
            }
            return result;
        }

        public static string ReadExcelPreviewAsCsv(Stream stream, string fileName, int maxChars = int.MaxValue)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("The input stream must be readable and seekable.", nameof(stream));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("The file name is required.", nameof(fileName));
            }

            if (maxChars <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxChars), "The maximum number of characters must be greater than zero.");
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("The file name must include an extension.", nameof(fileName));
            }

            var sheetData = ExcelPreviewReaderFactory.Create(extension).Read(stream);
            return ExcelPreviewCsvSerializer.Serialize(sheetData, maxChars);
        }

        public static bool CanReadExcelPreviewAsCsv(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var extension = Path.GetExtension(fileName);
            return !string.IsNullOrWhiteSpace(extension) && ExcelPreviewReaderFactory.IsSupported(extension);
        }

        public static void CreateExcel(string path, Dictionary<string, List<string[]>> data)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                //WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                //worksheetPart.Worksheet = new Worksheet(new SheetData());

                //SharedStringTablePart shareStringPart;

                //if (workbookpart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                //{
                //    shareStringPart = workbookpart.GetPartsOfType<SharedStringTablePart>().First();
                //}
                //else
                //{
                //    shareStringPart = workbookpart.AddNewPart<SharedStringTablePart>();
                //}
                //shareStringPart.SharedStringTable = new SharedStringTable();
                //shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text("50")));
                //shareStringPart.SharedStringTable.Save();
                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                if (data.Count() > 0)
                {
                    uint index = 0;
                    foreach (var item in data)
                    {
                        WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                        worksheetPart.Worksheet = new Worksheet(new SheetData());
                        Sheet sheet = new Sheet()
                        {
                            Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                            SheetId = ++index,
                            Name = item.Key
                        };
                        sheets.Append(sheet);
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                        for (int j = 0; j < item.Value.Count(); j++)
                        {
                            sheetData.Append(CreateContentRow(item.Value[j], j + 1));
                        }
                        worksheetPart.Worksheet.Save();     
                    }
                    workbookpart.Workbook.Save();
                }
                spreadsheet.Dispose();
            }
        }

        private static Row CreateContentRow(string[] cells, int rowIndex)
        {
            Row row = new Row
            {
                RowIndex = (UInt32)rowIndex
            };
            for (int i = 0; i < cells.Length; i++)
            {
                Cell dataCell = createTextCell(i + 1, rowIndex, cells[i]);
                row.AppendChild(dataCell);
            }
            return row;
        }

        private static Cell createTextCell(int columnIndex, int rowIndex, object cellValue)
        {
            Cell cell = new Cell();
            cell.DataType = CellValues.InlineString;
            InlineString inlineString = new InlineString();
            Text t = new Text();
            t.Text = cellValue == null ? null : cellValue.ToString();
            cell.CellValue = new CellValue(cellValue == null ? null : cellValue.ToString());
            inlineString.AppendChild(t);
            cell.AppendChild(inlineString);
            return cell;
        }

        private static bool IsCellFormattedAsDate(Cell cell, Stylesheet stylesheet)
        {
            if (stylesheet == null)
            {
                return false;
            }
            if (cell.StyleIndex != null)
            {
                uint styleIndex = cell.StyleIndex.Value;
                CellFormat cellFormat = stylesheet.CellFormats.ChildElements.Cast<CellFormat>().ElementAt((int)styleIndex);

                if (cellFormat != null && cellFormat.NumberFormatId != null)
                {
                    uint numberFormatId = cellFormat.NumberFormatId.Value;

                    // Check if the number format ID is a standard date format or a custom date format.  
                    return IsStandardDateFormat(numberFormatId) || IsCustomDateFormat(numberFormatId, stylesheet);
                }
            }
            // If there is no style information for the cell, assume it's not a date.  
            return false;
        }

        private static bool IsStandardDateFormat(uint numberFormatId)
        {
            // These are standard number format IDs for dates in Excel.  
            // The full list can be found in the ECMA-376 Part 1 specification.  
            return numberFormatId >= 14 && numberFormatId <= 22; //Common Date/Time formats  
        }

        private static bool IsCustomDateFormat(uint numberFormatId, Stylesheet stylesheet)
        {
            if (stylesheet.NumberingFormats == null) return false;

            NumberingFormat numberingFormat = stylesheet.NumberingFormats.Elements<NumberingFormat>().FirstOrDefault(nf => nf.NumberFormatId == numberFormatId);
            if (numberingFormat != null)
            {
                // Check if the format code contains date/time format specifiers  
                string formatCode = numberingFormat.FormatCode?.Value?.ToLower();
                return !string.IsNullOrEmpty(formatCode) && (formatCode.Contains("d") || formatCode.Contains("m") || formatCode.Contains("y") || formatCode.Contains("h") || formatCode.Contains("s"));
            }
            return false;
        }

        public static string GetCustomProperty(string filePath, string propertyName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                var customPropsPart = document.CustomFilePropertiesPart;
                if (customPropsPart == null)
                    return null;

                var prop = customPropsPart.Properties?
                    .Elements<CustomDocumentProperty>()
                    .FirstOrDefault(p => p.Name.HasValue && p.Name.Value == propertyName);

                return prop?.InnerText;
            }
        }

        public static void SetCustomProperty(string filePath, string propertyName, string propertyValue)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, true))
            {
                var customPropsPart = document.CustomFilePropertiesPart;
                if (customPropsPart == null)
                {
                    customPropsPart = document.AddCustomFilePropertiesPart();
                    customPropsPart.Properties = new Properties();
                }

                var props = customPropsPart.Properties;
                var prop = props.Elements<CustomDocumentProperty>()
                    .FirstOrDefault(p => p.Name.HasValue && p.Name.Value == propertyName);

                if (prop != null)
                {
                    prop.VTLPWSTR = new DocumentFormat.OpenXml.VariantTypes.VTLPWSTR(propertyValue);
                }
                else
                {
                    var pid = (props.Elements<CustomDocumentProperty>().Select(p => (int?)p.PropertyId?.Value).Max() ?? 1) + 1;
                    var newProp = new CustomDocumentProperty
                    {
                        Name = propertyName,
                        FormatId = "{D5CDD505-2E9C-101B-9397-08002B2CF9AE}",
                        PropertyId = pid,
                        VTLPWSTR = new DocumentFormat.OpenXml.VariantTypes.VTLPWSTR(propertyValue)
                    };
                    props.AppendChild(newProp);
                }
                props.Save();
            }
        }
    }
    public class CSVHelper
    {
        public static string[] AnalyseCSVRow2Array(string csvRow)
        {
            string strItem = string.Empty;
            int quoteFlag = 0;//引号数量标记, 单数时肯定不是某一列的结束位置
            List<string> lstStr = new List<string>();

            for (int i = 0; i < csvRow.Length; i++)
            {
                char strA = csvRow[i];

                if (strA == '"') { quoteFlag += 1; }  //计算引号个数

                if (quoteFlag == 2) { quoteFlag = 0; } //判断奇偶, 0为偶, 1为奇

                if (strA == ',' && quoteFlag == 0)   //确认逗号不是处于""之间, 即为单元分隔符
                {
                    if (strItem.Contains("\""))
                    {
                        if (strItem.StartsWith("\"") && strItem.EndsWith("\"") && strItem.Length >= 2)
                        {
                            strItem = strItem.Substring(1, strItem.Length - 2);   //取值时去掉两端标记字符的""号
                        }
                        strItem = strItem.Replace("\"\"", @"""");//CSV中引号也会有转义,单引号会转换为双引号
                    }

                    lstStr.Add(strItem);
                    strItem = string.Empty;
                }
                else
                {
                    strItem += strA;
                }
            }

            if (strItem.Length > 0)     //最后一个,后的单元会有遗漏, 再处理一遍
            {
                if (strItem.Contains("\""))
                {
                    strItem = strItem.Replace("\"\"", @"""");//CSV中引号也会有转义,单引号会转换为双引号
                    if (strItem.StartsWith("\"") && strItem.EndsWith("\"") && strItem.Length >= 2)
                    {
                        strItem = strItem.Substring(1, strItem.Length - 2);
                    }
                }
                lstStr.Add(strItem);
            }

            return lstStr.ToArray();
        }

        public static string[] AnalyseCSVRow2ArrayForManualImport(string csvRow)
        {
            string strItem = string.Empty;
            int quoteFlag = 0;//引号数量标记, 单数时肯定不是某一列的结束位置
            List<string> lstStr = new List<string>();

            for (int i = 0; i < csvRow.Length; i++)
            {
                char strA = csvRow[i];

                if (strA == '"') { quoteFlag += 1; }  //计算引号个数

                if (quoteFlag == 2) { quoteFlag = 0; } //判断奇偶, 0为偶, 1为奇

                if (strA == ',' && quoteFlag == 0)   //确认逗号不是处于""之间, 即为单元分隔符
                {
                    if (strItem.Contains("\""))
                    {
                        strItem = strItem.Replace("\"\"", @"""");//CSV中引号也会有转义,单引号会转换为双引号
                        if (strItem.StartsWith("\"") && strItem.EndsWith("\""))
                        {
                            if ((strItem.Length - 2) <= 1)
                            {
                                strItem = "";
                            }
                            else
                            {
                                strItem = strItem.Substring(1, strItem.Length - 2);
                            }   //取值时去掉两端标记字符的""号
                        }
                    }

                    lstStr.Add(strItem);
                    strItem = string.Empty;
                }
                else
                {
                    strItem += strA;
                }
            }

            if (strItem.Length > 0)     //最后一个,后的单元会有遗漏, 再处理一遍
            {
                if (strItem.Contains("\""))
                {
                    strItem = strItem.Replace("\"\"", @"""");//CSV中引号也会有转义,单引号会转换为双引号
                    if (strItem.StartsWith("\"") && strItem.EndsWith("\""))
                    {
                        if((strItem.Length - 2) <= 1)
                        {
                            strItem = "";
                        }
                        else
                        {
                            strItem = strItem.Substring(1, strItem.Length - 2);
                        }
                    }
                }
                lstStr.Add(strItem);
            }

            return lstStr.ToArray();
        }
    }
    public class UriFixer
    {
        public static Uri FixUri(string brokenUri)
        {
            return new Uri("http://broken-link/");
        }
        public static void FixInvalidUri(Stream fs, Func<string, Uri> invalidUriHandler)
        {
            XNamespace relNs = "http://schemas.openxmlformats.org/package/2006/relationships";
            using (ZipArchive za = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                foreach (var entry in za.Entries.ToList())
                {
                    if (!entry.Name.EndsWith(".rels"))
                        continue;
                    bool replaceEntry = false;
                    XDocument entryXDoc = null;
                    using (var entryStream = entry.Open())
                    {
                        try
                        {
                            entryXDoc = XDocument.Load(entryStream);
                            if (entryXDoc.Root != null && entryXDoc.Root.Name.Namespace == relNs)
                            {
                                var urisToCheck = entryXDoc
                                    .Descendants(relNs + "Relationship")
                                    .Where(r => r.Attribute("TargetMode") != null && (string)r.Attribute("TargetMode") == "External");
                                if(urisToCheck == null)
                                {
                                    continue;
                                }
                                foreach (var rel in urisToCheck)
                                {
                                    var target = (string)rel.Attribute("Target");
                                    if (target != null)
                                    {
                                        try
                                        {
                                            Uri uri = new Uri(target);
                                        }
                                        catch (UriFormatException)
                                        {
                                            Uri newUri = invalidUriHandler(target);
                                            rel.Attribute("Target").Value = newUri.ToString();
                                            replaceEntry = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (XmlException)
                        {
                            continue;
                        }
                    }
                    if (replaceEntry)
                    {
                        var fullName = entry.FullName;
                        entry.Delete();
                        var newEntry = za.CreateEntry(fullName);
                        using (StreamWriter writer = new StreamWriter(newEntry.Open()))
                        using (XmlWriter xmlWriter = XmlWriter.Create(writer))
                        {
                            entryXDoc.WriteTo(xmlWriter);
                        }
                    }
                }
            }
        }
    }
}
