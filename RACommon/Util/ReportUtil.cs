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
using Amazon.Runtime.Internal.Transform;
using AngleSharp.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.I18N.Core;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Graph.CoreConstants;

namespace AvePoint.RA.Common.Util
{
    public class ReportUtil
    {
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ReportUtil));
        public const string ValidationSheetName = "Validations";
        public const int TermsSheetColumnCount = 19;
        public const int RulesSheetColumnCount = 52;
        public const string cellStartIndex = "2";
        public const string cellEndIndex = "1048576";
        public const string New_Action_KeepDataActionString = "Tag or lock content";
        public const string Action_KeepDataActionString = "Declare or tag content";

        public static Dictionary<String, int> KeyValues = new Dictionary<string, int>()
        {
            {"Parent folder name",(int)ArchiverFilterRuleType.ParentFolderName},
            {"Parent folder name (Including subfolders)",(int)ArchiverFilterRuleType.ParentFolderNameHeirarchically},
            {"SendDate",(int)ArchiverFilterRuleType.SendDateUTC },
            {"LastAccessedTime/ModifiedTime", (int)ArchiverFilterRuleType.LastActiveTime},
            {"Label property (Date)", (int)ArchiverFilterRuleType.DateTimeLabelProperty},
            {"Label property (Number)", (int)ArchiverFilterRuleType.NumberLabelProperty},
            {"Label property (Text)", (int)ArchiverFilterRuleType.TextLabelProperty},
            {"SensitivityLabel (full name)",(int)ArchiverFilterRuleType.SensitivityLabelFullName },
            {"SensitivityLabelFullName",(int)ArchiverFilterRuleType.SensitivityLabelFullName },
            {"SensitivityLabel (display name)", (int)ArchiverFilterRuleType.SensitivityLabel },
            {"SensitivityLabel", (int)ArchiverFilterRuleType.SensitivityLabel },            
            {"Parent library property (Text)", (int)ArchiverFilterRuleType.ParentLibraryText },
            {"Parent library property (Number)", (int)ArchiverFilterRuleType.ParentLibraryNumber },
            {"Parent library property (Yes/No)", (int)ArchiverFilterRuleType.ParentLibraryBoolean },
            {"Parent library property (Date and Time)", (int)ArchiverFilterRuleType.ParentLibraryDateTime },
            {"Parent site collection (Text)", (int)ArchiverFilterRuleType.ParentSiteCollectionText },
            {"Parent site collection (Number)", (int)ArchiverFilterRuleType.ParentSiteCollectionNumber },
            {"Parent site collection (Yes/No)", (int)ArchiverFilterRuleType.ParentSiteCollectionBoolean },
            {"Parent site collection (Date and Time)", (int)ArchiverFilterRuleType.ParentSiteCollectionDateTime },
            {"Parent site property (Text)", (int)ArchiverFilterRuleType.PropertyBagText },
            {"Parent site property (Number)", (int)ArchiverFilterRuleType.PropertyBagNumber },
            {"Parent site property (Yes/No)", (int)ArchiverFilterRuleType.PropertyBagBoolean },
            {"Parent site property (Date and Time)", (int)ArchiverFilterRuleType.PropertyBagDateTime },
            {"Latest subfolder action due date", (int)ArchiverFilterRuleType.LastestSubfolderDisposalDate },
            {"Orphaned folder", (int)ArchiverFilterRuleType.OrphanedFolderRule },
        };
        /// <summary>
        /// Fortify scan issue,RECO-20916
        /// path paramether should be generated by SecurityUtils.SafeCombinePath path method! 
        /// </summary>
        /// <param name="dbFilePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CreateExcel(string path, string sheetName, string[][] data)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                SharedStringTablePart shareStringPart;

                if (workbookpart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = workbookpart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = workbookpart.AddNewPart<SharedStringTablePart>();
                }
                shareStringPart.SharedStringTable = new SharedStringTable();
                shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text("50")));
                shareStringPart.SharedStringTable.Save();
                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                if (sheetName != null && data.Count() > 0)
                {
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = sheetName
                    };
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    for (int j = 0; j < data.Count(); j++)
                    {
                        if (data[j] == null)
                        {
                            continue;
                        }
                        sheetData.Append(CreateContentRow(data[j], j + 1));
                    }
                    sheets.Append(sheet);
                }
                workbookpart.Workbook.Save();
                worksheetPart.Worksheet.Save();
                spreadsheet.Dispose();
            }
        }

        public static int GetRowColumn(string sheetName)
        {
            int RowColumnCount = 0;
            if (sheetName == "Terms")
            {
                RowColumnCount = TermsSheetColumnCount;
            }
            else if (sheetName == "Rules")
            {
                RowColumnCount = Enum.GetNames(new ExcelHeadColumn().GetType()).Length;
            }
            else
            {
                RowColumnCount = 0;
            }
            return RowColumnCount;
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

        private static string GetCellReference(int columnIndex, int rowIndex)
        {
            try
            {
                string columnName = GetColumnName(columnIndex);
                return $"{columnName}{rowIndex}";
            }
            catch(Exception e)
            {
                logger.Error($"Get cell reference: column index: {columnIndex}, row index: {rowIndex},error: {e}");
                return null;
            }
        }

        private static string GetColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private static Cell createTextCell(int columnIndex, int rowIndex, object cellValue)
        {
            Cell cell = new Cell();
            cell.DataType = CellValues.InlineString;
            InlineString inlineString = new InlineString();
            Text t = new Text();
            t.Text = cellValue == null ? null : cellValue.ToString();
            cell.CellValue = new CellValue(cellValue == null ? null : cellValue.ToString());
            cell.CellReference = GetCellReference(columnIndex, rowIndex);
            inlineString.AppendChild(t);
            cell.AppendChild(inlineString);
            return cell;
        }

        public static void InsertDataToSheet(string docName, string[][] data,int sheetIndex, int startIndex = 1)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var workSheetPart = workBookPart.WorksheetParts;
                var workSheet = workSheetPart.ElementAt(sheetIndex);
                SheetData sheetData = workSheet.Worksheet.GetFirstChild<SheetData>();
                for (int i = startIndex; i < data.Count(); i++)
                {
                    sheetData.Append(CreateContentRow(data[i], sheetData.Elements<Row>().Count() + 1));
                }
                workSheet.Worksheet.Save();
                spreadSheet.WorkbookPart.Workbook.Save();
                spreadSheet.Dispose();
            }
        }

        public static void ManualInsertDataToSheet(string docName, string[][] data, int sheetIndex, string[] dropDownLists)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var workSheetPart = workBookPart.WorksheetParts;
                var workSheet = workSheetPart.ElementAt(sheetIndex);
                DataValidations dataValidations = workSheet.Worksheet.GetFirstChild<DataValidations>();
                SheetData sheetData = workSheet.Worksheet.GetFirstChild<SheetData>();
                for (int j = 1; j < data.Count(); j++)
                {
                    int rowIndex = sheetData.Elements<Row>().Count() + 1;
                    string[] cells = data[j];
                    Row row = new Row
                    {
                        RowIndex = (UInt32)rowIndex
                    };
                    for (int i = 0; i < cells.Length; i++)
                    {
                        int columnIndex = i + 1;
                        Cell dataCell = createTextCell(columnIndex, rowIndex, cells[i]);
                        row.AppendChild(dataCell);
                        if (columnIndex == 2 && rowIndex != 1)
                        {
                            var dataValidation = new DataValidation()
                            {
                                AllowBlank = true,
                                ShowErrorMessage = true,
                                ShowInputMessage = true,
                                SequenceOfReferences = new ListValue<StringValue>() { InnerText = $"B{rowIndex}" },
                                Formula1 = new Formula1("\"" + string.Join(",", dropDownLists) + "\""),
                                Type = DataValidationValues.List
                            };
                            dataValidations.Append(dataValidation);
                            dataValidations.Count = dataValidations.Count == null ? 1 : dataValidations.Count + 1;
                        }
                    }
                    sheetData.Append(row);
                }
                workSheet.Worksheet.Save();
                spreadSheet.WorkbookPart.Workbook.Save();
                spreadSheet.Dispose();
            }
        }

        public static void InsertWorksheet(string docName, string sheetName, string[][] data)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet(new SheetData());
                Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);
                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }
                Sheet sheet = new Sheet()
                {
                    Id = relationshipId,
                    SheetId = sheetId,
                    Name = sheetName
                };

                SheetData sheetData = newWorksheetPart.Worksheet.GetFirstChild<SheetData>();

                for (int i = 0; i < data.Count(); i++)
                {
                    sheetData.Append(CreateContentRow(data[i], i + 1));
                }

                sheets.Append(sheet);


                newWorksheetPart.Worksheet.Save();
                spreadSheet.WorkbookPart.Workbook.Save();
                spreadSheet.Dispose();
            }
        }

        public static List<string[]> ReadExcelFile(Stream stream)
        {
            List<string[]> result = new List<string[]>();
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(stream, false))
            {
                WorkbookPart workBookPart = spreadSheet.WorkbookPart;
                var stringTablePart = workBookPart.SharedStringTablePart;
                Sheets sheets = workBookPart.Workbook.GetFirstChild<Sheets>();
                foreach (Sheet sheet in sheets.Elements<Sheet>())
                {
                    WorksheetPart workSheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;
                    var sheetRows = workSheetPart.Worksheet.Descendants<Row>();
                    foreach (Row row in sheetRows)
                    {
                        if (row.RowIndex > 1)
                        {
                            result.Add(row.Elements<Cell>().Select(c => GetValue(c, stringTablePart)).ToArray());
                        }
                    }
                }

                spreadSheet.Dispose();
            }

            return result;
        }

        private static String GetValue(Cell cell, SharedStringTablePart stringTablePart)
        {
            String value = null;
            DateTime dt = new DateTime();
            if (cell.ChildElements.Count == 0)
                return string.Empty;
            //get cell value
            value = cell.CellValue.InnerText;
            //Look up real value from shared string table
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                value = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else if ((cell.DataType != null) && (cell.DataType == CellValues.Boolean))
            {
                if (value.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    value = "true";
                }
                else
                {
                    value = "false";
                }
            }
            else if (cell.DataType == null && cell.StyleIndex != null)
            {
                //if the value we got is a number,normally cell.DataType == null.
                //Then we shuold judge wether cell.StyleIndex is between 2 and 18(included)
                // this means the value is a number,we get its value originally,
                //or it means the value is a datetime,we shuold get the real datetime from the value
                if (cell.StyleIndex > 1 && cell.StyleIndex < 19)
                {
                    value = cell.CellValue.InnerText;
                }
                else
                {
                    dt = DateTime.FromOADate(double.Parse(value));
                    value = dt.ToString();
                }
            }
            return value;
        }

        public static void CreateManualApprovalExcel(string path, string sheetName, string[][] data, string[] dropDownLists)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                SharedStringTablePart shareStringPart;

                if (workbookpart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = workbookpart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = workbookpart.AddNewPart<SharedStringTablePart>();
                }
                shareStringPart.SharedStringTable = new SharedStringTable();
                shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text("50")));
                shareStringPart.SharedStringTable.Save();
                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                if (sheetName != null && data.Count() > 0)
                {
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = sheetName
                    };

                    DataValidations dataValidations = worksheetPart.Worksheet.GetFirstChild<DataValidations>();
                    if (dataValidations == null)
                    {
                        dataValidations = new DataValidations();
                    }

                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    for (int j = 0; j < data.Count(); j++)
                    {
                        int rowIndex = j + 1;
                        string[] cells = data[j];
                        Row row = new Row
                        {
                            RowIndex = (UInt32)rowIndex
                        };
                        for (int i = 0; i < cells.Length; i++)
                        {
                            int columnIndex = i + 1;
                            Cell dataCell = createTextCell(columnIndex, rowIndex, cells[i]);
                            var cellRef = Enum.GetValues(typeof(ExcelHeadColumn)).GetValue(columnIndex - 1).ToString() + rowIndex;
                            dataCell.CellReference = new StringValue(cellRef);
                            row.AppendChild(dataCell);
                            //header是不添加dataValidation的
                            if (columnIndex == 3 && rowIndex != 1)
                            {
                                var dataValidation = new DataValidation()
                                {
                                    AllowBlank = true,
                                    ShowErrorMessage = true,
                                    ShowInputMessage = true,
                                    SequenceOfReferences = new ListValue<StringValue>() { InnerText = $"C{rowIndex}" },
                                    Formula1 = new Formula1("\"" + string.Join(",", dropDownLists) + "\""),
                                    Type = DataValidationValues.List
                                };
                                dataValidations.Append(dataValidation);
                                dataValidations.Count = dataValidations.Count == null ? 1 : dataValidations.Count + 1;
                            }
                        }
                        sheetData.Append(row);
                    }
                    sheets.Append(sheet);
                    worksheetPart.Worksheet.AppendChild(dataValidations);
                }
                Columns idColumn = GenerateColumns(1U);
                Columns actionTimeColumn = GenerateColumns(2U);
                worksheetPart.Worksheet.InsertAfter(idColumn, worksheetPart.Worksheet.SheetFormatProperties);
                worksheetPart.Worksheet.InsertAfter(actionTimeColumn, worksheetPart.Worksheet.SheetFormatProperties);

                workbookpart.Workbook.Save();
                worksheetPart.Worksheet.Save();
                spreadsheet.Dispose();
            }
        }
        private static Columns GenerateColumns(UInt32Value ColumnIndex)
        {
            Columns columns1 = new Columns();
            Column column1 = new Column() { Min = ColumnIndex, Max = ColumnIndex, Width = 0D, Hidden = true, CustomWidth = true };
            columns1.Append(column1);
            return columns1;
        }

        public static void InsertManualApprovalWorksheet(string docName, string sheetName, string[][] data, string[] dropDownLists)
        {
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet(new SheetData());
                Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);
                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }
                Sheet sheet = new Sheet()
                {
                    Id = relationshipId,
                    SheetId = sheetId,
                    Name = sheetName
                };
                DataValidations dataValidations = newWorksheetPart.Worksheet.GetFirstChild<DataValidations>();
                if (dataValidations == null)
                {
                    dataValidations = new DataValidations();
                }

                SheetData sheetData = newWorksheetPart.Worksheet.GetFirstChild<SheetData>();
                for (int j = 0; j < data.Count(); j++)
                {
                    int rowIndex = j + 1;
                    string[] cells = data[j];
                    Row row = new Row
                    {
                        RowIndex = (UInt32)rowIndex
                    };
                    for (int i = 0; i < cells.Length; i++)
                    {
                        int columnIndex = i + 1;
                        Cell dataCell = createTextCell(columnIndex, rowIndex, cells[i]);
                        var cellRef = Enum.GetValues(typeof(ExcelHeadColumn)).GetValue(columnIndex - 1).ToString() + rowIndex;
                        dataCell.CellReference = new StringValue(cellRef);
                        row.AppendChild(dataCell);
                        if (columnIndex == 3 && rowIndex != 1)
                        {
                            var dataValidation = new DataValidation()
                            {
                                AllowBlank = true,
                                ShowErrorMessage = true,
                                ShowInputMessage = true,
                                SequenceOfReferences = new ListValue<StringValue>() { InnerText = $"C{rowIndex}" },
                                Formula1 = new Formula1("\"" + string.Join(",", dropDownLists) + "\""),
                                Type = DataValidationValues.List
                            };
                            dataValidations.Append(dataValidation);
                            dataValidations.Count = dataValidations.Count == null ? 1 : dataValidations.Count + 1;
                        }
                    }
                    sheetData.Append(row);
                }
                sheets.Append(sheet);
                newWorksheetPart.Worksheet.AppendChild(dataValidations);

                Columns idColumn = GenerateColumns(1U);
                Columns actionTimeColumn = GenerateColumns(2U);
                newWorksheetPart.Worksheet.InsertAfter(idColumn, newWorksheetPart.Worksheet.SheetFormatProperties);
                newWorksheetPart.Worksheet.InsertAfter(actionTimeColumn, newWorksheetPart.Worksheet.SheetFormatProperties);

                newWorksheetPart.Worksheet.Save();
                spreadSheet.WorkbookPart.Workbook.Save();
                spreadSheet.Dispose();
            }
        }

        public static void MergeCells(string path, string sheetName, Dictionary<ExcelHeadColumn, List<string>> mergeRanges)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(path, true))
            {
                Worksheet worksheet = GetWorksheet(document, sheetName);

                MergeCells mergeCells;
                if (worksheet.Elements<MergeCells>().Count() > 0)
                    mergeCells = worksheet.Elements<MergeCells>().First();
                else
                {
                    mergeCells = new MergeCells();
                    if (worksheet.Elements<CustomSheetView>().Count() > 0)
                        worksheet.InsertAfter(mergeCells, worksheet.Elements<CustomSheetView>().First());
                    else
                        worksheet.InsertAfter(mergeCells, worksheet.Elements<SheetData>().First());
                }
                // Create the merged cell and append it to the MergeCells collection.

                foreach (KeyValuePair<ExcelHeadColumn, List<string>> kv in mergeRanges)
                {
                    string cellHeadName = kv.Key.ToString();
                    foreach (var item in kv.Value)
                    {
                        string startCell = item.Split(',')[0];
                        string endCell = item.Split(',')[1];
                        MergeCell cell = new MergeCell()
                        {
                            Reference = new StringValue(string.Format("{2}{0}:{2}{1}", startCell, endCell, cellHeadName))
                        };
                        mergeCells.Append(cell);
                    }

                }
                worksheet.Save();
            }
        }

        // Get the specified worksheet.
        private static Worksheet GetWorksheet(SpreadsheetDocument document, string worksheetName)
        {
            IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook
                .Descendants<Sheet>().Where(s => s.Name == worksheetName);
            WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart
                .GetPartById(sheets.First().Id);
            return worksheetPart.Worksheet;
        }

        #region create terms rules validations sheets
        public static void CreateTermsAndRulesSheets(string path, List<string[]> ruleDatas, List<string[]> termsDatas, ExportAddition exportAddition = null)
        {
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                SharedStringTablePart shareStringPart;

                if (workbookpart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = workbookpart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = workbookpart.AddNewPart<SharedStringTablePart>();
                }
                shareStringPart.SharedStringTable = new SharedStringTable();
                shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text("50")));
                shareStringPart.SharedStringTable.Save();

                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                CreateValidationsSheet(ValidationSheetName, spreadsheet, sheets, worksheetPart, exportAddition);

                CreateStyleSheet(workbookpart, shareStringPart.SharedStringTable);
                CreateTermsSheet(spreadsheet, workbookpart, sheets, shareStringPart.SharedStringTable, termsDatas, exportAddition);
                CreateRulesSheet(spreadsheet, workbookpart, sheets, shareStringPart.SharedStringTable, ruleDatas, exportAddition);

                workbookpart.Workbook.Save();
                worksheetPart.Worksheet.Save();
                spreadsheet.Dispose();
            }
            ExcelUtil.SetCustomProperty(path, TermAndRuleTemplateVersion.PROPERTIES_KEY, TermAndRuleTemplateVersion.PROPERTIES_VALUE);
        }
        public static void CreateTermsSheet(SpreadsheetDocument spreadsheet, WorkbookPart workBookPart, Sheets sheets, SharedStringTable sharedStringTable, List<string[]> datas, ExportAddition exportAddition = null)
        {
            var termsSheetName = "Terms";
            UInt32 termsSheetId = 2;
            WorksheetPart worksheetPart_Term = workBookPart.AddNewPart<WorksheetPart>();
            worksheetPart_Term.Worksheet = new Worksheet(new SheetData());
            //生成Terms Sheet中下拉列表项
            ExcelValidaton termsValidation = new ExcelValidaton(worksheetPart_Term.Worksheet, ValidationSheetName, new List<string[]>() {
                    new string[] { ExcelHeadColumn.L.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.N.ToString() , cellStartIndex, "7"},
                    new string[] { ExcelHeadColumn.P.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.M.ToString() , cellStartIndex, "6"},
                    new string[] { ExcelHeadColumn.S.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.G.ToString() , cellStartIndex, "140"},
                });
            termsValidation.UpdateForSheet();
            CreateSheet(termsSheetName, spreadsheet, sheets, worksheetPart_Term, sharedStringTable, termsSheetId, SheetType.Terms, datas, exportAddition);
        }
        public static void CreateRulesSheet(SpreadsheetDocument spreadsheet, WorkbookPart workBookPart, Sheets sheets, SharedStringTable sharedStringTable, List<string[]> datas, ExportAddition exportAddition = null)
        {
            var rulesSheetName = "Rules";
            UInt32 rulesSheetId = 3;
            WorksheetPart worksheetPart_Rule = workBookPart.AddNewPart<WorksheetPart>();
            worksheetPart_Rule.Worksheet = new Worksheet(new SheetData());
            //生成Rules Sheet中下拉列表项
            int conditionCount = 14;
            if (exportAddition != null && exportAddition.ConditionArray != null)
            {
                conditionCount += exportAddition.ConditionArray.Count();
            }
            ExcelValidaton rulesValidation = exportAddition?.IsSupportRecordLabelFunction ?? false ?
                 new ExcelValidaton(worksheetPart_Rule.Worksheet, ValidationSheetName, new List<string[]>() {
                    new string[] { ExcelHeadColumn.D.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.A.ToString() , cellStartIndex, "8"},
                    new string[] { ExcelHeadColumn.F.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.I.ToString() , cellStartIndex, "13"},
                    new string[] { ExcelHeadColumn.G.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.B.ToString() , cellStartIndex, "4"},
                    new string[] { ExcelHeadColumn.H.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.C.ToString() , cellStartIndex, "59"},
                    new string[] { ExcelHeadColumn.J.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.D.ToString() , cellStartIndex, conditionCount.ToString()},
                    new string[] { ExcelHeadColumn.L.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.E.ToString() , cellStartIndex, "9"},
                    new string[] { ExcelHeadColumn.O.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.F.ToString() , cellStartIndex, "8"},
                    new string[] { ExcelHeadColumn.P.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.H.ToString() , cellStartIndex, "5"},

                    new string[] { ExcelHeadColumn.AE.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.K.ToString() , cellStartIndex, "6"},
                    new string[] { ExcelHeadColumn.AH.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.G.ToString() , cellStartIndex, "140"},
                    new string[] { ExcelHeadColumn.AM.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.L.ToString() , cellStartIndex, "5"},
                    new string[] { ExcelHeadColumn.AS.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.O.ToString() , cellStartIndex, "4"},
                    new string[] { ExcelHeadColumn.AW.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.H.ToString() , cellStartIndex, "5"},
                })
                : new ExcelValidaton(worksheetPart_Rule.Worksheet, ValidationSheetName, new List<string[]>() {
                    new string[] { ExcelHeadColumn.D.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.A.ToString() , cellStartIndex, "8"},
                    new string[] { ExcelHeadColumn.F.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.I.ToString() , cellStartIndex, "13"},
                    new string[] { ExcelHeadColumn.G.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.B.ToString() , cellStartIndex, "4"},
                    new string[] { ExcelHeadColumn.H.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.C.ToString() , cellStartIndex, "58"},
                    new string[] { ExcelHeadColumn.J.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.D.ToString() , cellStartIndex, conditionCount.ToString()},
                    new string[] { ExcelHeadColumn.L.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.E.ToString() , cellStartIndex, "9"},
                    new string[] { ExcelHeadColumn.O.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.F.ToString() , cellStartIndex, "7"},
                    new string[] { ExcelHeadColumn.P.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.H.ToString() , cellStartIndex, "5"},

                    new string[] { ExcelHeadColumn.AD.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.K.ToString() , cellStartIndex, "6"},
                    new string[] { ExcelHeadColumn.AG.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.G.ToString() , cellStartIndex, "140"},
                    new string[] { ExcelHeadColumn.AJ.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.L.ToString() , cellStartIndex, "5"},
                    new string[] { ExcelHeadColumn.AP.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.O.ToString() , cellStartIndex, "4"},
                    new string[] { ExcelHeadColumn.AT.ToString(), cellStartIndex, cellEndIndex, ValidationsHeadColumn.H.ToString() , cellStartIndex, "5"},
                });
            rulesValidation.UpdateForSheet();
            CreateSheet(rulesSheetName, spreadsheet, sheets, worksheetPart_Rule, sharedStringTable, rulesSheetId, SheetType.Rules, datas, exportAddition);
        }
        #region common sheet methods
        public static void CreateSheet(string name, SpreadsheetDocument spreadsheet, Sheets sheets, WorksheetPart worksheetPart, SharedStringTable sharedStringTable, UInt32 sheetId, SheetType sheetType, List<string[]> rowDatas, ExportAddition exportAddition = null)
        {
            Sheet sheet = new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = name
            };
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            CreateSheetHeadRow(sheetData, sharedStringTable, sheetType, exportAddition);
            int rowIndex = 1;
            foreach (var rowData in rowDatas)
            {
                try
                {
                    CreateSheetContentRow(sheetData, sharedStringTable, rowIndex, rowData.ToList(), sheetType, exportAddition);
                }
                catch (Exception ex)
                {
                    logger.Error("A error occur while create {0} sheet content row, message:{1}", rowIndex, ex.ToString());
                    throw;
                }
                rowIndex++;
            }
            sheets.Append(sheet);
        }
        public static void CreateStyleSheet(WorkbookPart workbookPart, SharedStringTable sharedStringTable)
        {
            var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            Stylesheet stylesheet1 = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac" } };
            stylesheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");

            Fonts fonts1 = new Fonts() { Count = (UInt32Value)1U, KnownFonts = true };

            Font font1 = new Font();
            FontSize fontSize1 = new FontSize() { Val = 11D };
            Color color1 = new Color() { Theme = (UInt32Value)1U };
            FontName fontName1 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

            font1.Append(fontSize1);
            font1.Append(color1);
            font1.Append(fontName1);
            font1.Append(fontFamilyNumbering1);
            font1.Append(fontScheme1);

            fonts1.Append(font1);

            Fills fills1 = new Fills() { Count = (UInt32Value)5U };

            // FillId = 0
            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.None };
            fill1.Append(patternFill1);

            // FillId = 1
            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };
            fill2.Append(patternFill2);

            // FillId = 2,RED
            Fill fill3 = new Fill();
            PatternFill patternFill3 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor1 = new ForegroundColor() { Rgb = "FFFF0000" };
            BackgroundColor backgroundColor1 = new BackgroundColor() { Indexed = (UInt32Value)64U };
            patternFill3.Append(foregroundColor1);
            patternFill3.Append(backgroundColor1);
            fill3.Append(patternFill3);

            // FillId = 3,BLUE
            Fill fill4 = new Fill();
            PatternFill patternFill4 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor2 = new ForegroundColor() { Rgb = "FF0070C0" };
            BackgroundColor backgroundColor2 = new BackgroundColor() { Indexed = (UInt32Value)64U };
            patternFill4.Append(foregroundColor2);
            patternFill4.Append(backgroundColor2);
            fill4.Append(patternFill4);

            // FillId = 4,YELLO
            Fill fill5 = new Fill();
            PatternFill patternFill5 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor3 = new ForegroundColor() { Rgb = "FFFFFF00" };
            BackgroundColor backgroundColor3 = new BackgroundColor() { Indexed = (UInt32Value)64U };
            patternFill5.Append(foregroundColor3);
            patternFill5.Append(backgroundColor3);
            fill5.Append(patternFill5);

            fills1.Append(fill1);
            fills1.Append(fill2);
            fills1.Append(fill3);
            fills1.Append(fill4);
            fills1.Append(fill5);

            Borders borders1 = new Borders() { Count = (UInt32Value)1U };

            Border border1 = new Border();
            //new Color(){ Auto = true }
            // ){ Style = BorderStyleValues.Thin
            LeftBorder leftBorder1 = new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin };
            RightBorder rightBorder1 = new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin };
            TopBorder topBorder1 = new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin };
            BottomBorder bottomBorder1 = new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin };
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            borders1.Append(border1);

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };

            cellStyleFormats1.Append(cellFormat1);

            CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)4U };
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFill = true, ApplyBorder = true };
            CellFormat cellFormat4 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)3U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFill = true, ApplyBorder = true };
            CellFormat cellFormat5 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)4U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFill = true };

            cellFormats1.Append(cellFormat2);
            cellFormats1.Append(cellFormat3);
            cellFormats1.Append(cellFormat4);
            cellFormats1.Append(cellFormat5);

            CellStyles cellStyles1 = new CellStyles() { Count = (UInt32Value)1U };
            CellStyle cellStyle1 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U, CustomBuiltin = true };
            cellStyles1.Append(cellStyle1);
            DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = (UInt32Value)0U };
            TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleMedium9" };

            //StylesheetExtensionList stylesheetExtensionList1 = new StylesheetExtensionList();

            //StylesheetExtension stylesheetExtension1 = new StylesheetExtension() { Uri = "{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}" };
            //stylesheetExtension1.AddNamespaceDeclaration("x14", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
            //X14.SlicerStyles slicerStyles1 = new X14.SlicerStyles() { DefaultSlicerStyle = "SlicerStyleLight1" };

            //stylesheetExtension1.Append(slicerStyles1);

            //stylesheetExtensionList1.Append(stylesheetExtension1);

            stylesheet1.Append(fonts1);
            stylesheet1.Append(fills1);
            stylesheet1.Append(borders1);
            stylesheet1.Append(cellStyleFormats1);
            stylesheet1.Append(cellFormats1);
            stylesheet1.Append(cellStyles1);
            stylesheet1.Append(differentialFormats1);
            stylesheet1.Append(tableStyles1);
            //stylesheet1.Append(stylesheetExtensionList1);
            workbookStylesPart.Stylesheet = stylesheet1;
            workbookStylesPart.Stylesheet.Save();
        }
        public static void CreateSheetHeadRow(SheetData sheetData, SharedStringTable sharedStringTable, SheetType sheetType, ExportAddition exportAddition = null)
        {
            Row headRow = new Row();
            var i = 0;
            int termsSheetColumnCount = TermsSheetColumnCount;
            if (exportAddition != null && exportAddition.TermColumArray != null)
            {
                termsSheetColumnCount += exportAddition.TermColumArray.Count();
            }
            var rulesSheetColumnCount = RulesSheetColumnCount;
            if (exportAddition != null && exportAddition.RuleColumArray != null)
            {
                rulesSheetColumnCount += exportAddition.RuleColumArray.Count();
            }
            if (exportAddition?.IsSupportRecordLabelFunction ?? false)
            {
                rulesSheetColumnCount += 3; 
            }
            foreach (ExcelHeadColumn item in Enum.GetValues(typeof(ExcelHeadColumn)))
            {
                if (i == termsSheetColumnCount && sheetType == SheetType.Terms)
                {
                    break;
                }
                if (i == rulesSheetColumnCount && sheetType == SheetType.Rules)
                {
                    break;
                }
                List<string> colNames = GetSheetHeadNames(sheetType, exportAddition);
                string colNameStr = I18NEntity.GetString(colNames[i]);
                colNameStr = string.IsNullOrEmpty(colNameStr) ? "" : colNameStr.TrimEnd(':');
                var cellRef = item.ToString() + 1;
                var index = InsertSharedStringItem(colNameStr, sharedStringTable);
                Cell cell = new()
                {
                    CellValue = new CellValue(index.ToString()),
                    StyleIndex = (UInt32Value)3U,
                    CellReference = new StringValue(cellRef),
                    DataType = new EnumValue<CellValues>(CellValues.SharedString)
                };
                headRow.Append(cell);
                i++;
            }
            sheetData.Append(headRow);
        }

        public static List<string> GetSheetHeadNames(SheetType sheetType, ExportAddition exportAddition = null)
        {
            switch (sheetType)
            {
                case SheetType.Terms:
                    var terms = new List<string>() {
                        "RM_TM_Excel_Group","RM_TM_Excel_TermSet","Level 1 term name","Level 2 term name","Level 3 term name",
                        "Level 4 term name","Level 5 term name","RM_JS_RDM_Rule_Description","RM_SPS_InheritGlobalSettings","RM_JS_RDM_Rule_RuleName",
                        "RM_TM_Excel_RetentionSetting","RM_TM_Excel_RetentionSourceType","RM_TM_Retension_SP_Label","RM_TM_Retension_Exchange_Label", "RM_TM_Retension_OneDrive_Label",
                        "RM_TM_Excel_TermActivationSettings","RM_TM_Excel_StartTime","RM_TM_Excel_EndTime","RM_TM_Excel_TimeZone"
                    };
                    terms.AddRange(exportAddition?.TermColumArray ?? []);
                    return terms;

                case SheetType.Rules:
                    var rules = new List<string>() {
                        "RM_JS_RDM_Rule_RuleName","RM_JS_RDM_Rule_Description","RM_JS_Rule_Detail_RuleContainer","RM_JS_Rule_Detail_RuleLevel","RM_JS_Rule_DisposalClass_Title","RM_TM_Excel_SourceType",
                        "Rule criteria combination","Criteria type","Criteria name","Criteria condition","Condition value",
                        "Condition value unit","Condition start time","Condition end time","RM_JS_Rule_Detail_DWSP","Export content format",
                        "RM_RDM_CreateRule_DeleteRelatedRecord","RM_RDM_CreateRule_Options_IncludeDeclaredFile","Leave stub","RM_TM_Excel_StubTemplate","RM_TM_Excel_BackupBeforeDestroying","Remove box when empty","RM_RDM_CreateRule_Options_IncludeRetentionLabels","RM_JS_BCM_Explorer_Button_DeclareAsSharePointRecord","RM_TM_Excel_DoTag",
                        "RM_TM_Excel_TagArchived","RM_TM_Excel_TagArchivedBy","RM_TM_Excel_TagArchivedTime","RM_TM_Excel_TagCustomColumn","RM_TM_Excel_CustomColumnType",
                        "RM_TM_Excel_CustomColumnName","RM_TM_Excel_CustomColumnValue","RM_TM_Excel_CustomColumnTimeZone","RM_TM_Excel_LabelName", "RM_TM_Excel_DestinationLocation",
                        "RM_TM_Excel_ConflictResolution","RM_TM_Excel_DeclareMove","RM_TM_Excel_EXORemoveSource","RM_TM_Excel_EXOKeepReclassify","RM_RDM_CreateRule_Options_EnableApproval","Send email notification","Manual approval type","Manual approval process name","RM_JS_JMD_Grid_RecordOwner",
                        "RM_TM_Excel_ExportBefore","Export format","RM_TM_Excel_ArchiveStorage", "Export to destination library", "Export location", "Delete to recycle bin", "Delete site collection to recycle bin", "RM_RDM_CreateRule_LockRecordBeforeDestroy",
                        };
                    rules.AddRange(exportAddition?.RuleColumArray ?? []);
                    if(exportAddition?.IsSupportRecordLabelFunction ?? false)
                    {
                        var indexOfDeclaredMoveDocument = rules.IndexOf("RM_TM_Excel_DeclareMove");
                        if(indexOfDeclaredMoveDocument != -1)
                        {
                            rules[indexOfDeclaredMoveDocument] = "RM_TM_Excel_LockRecordMove";
                        }
                        var indexOfIncludeDeclareFile = rules.IndexOf("RM_RDM_CreateRule_Options_IncludeDeclaredFile");
                        if(indexOfIncludeDeclareFile != -1)
                        {
                            rules.Insert(indexOfIncludeDeclareFile + 1, "RM_RDM_CreateRule_RecordsLabelOption");
                        }
                        var indexOfRetentionLabelName = rules.IndexOf("RM_TM_Excel_LabelName");
                        if(indexOfRetentionLabelName != -1)
                        {
                            rules.Insert(indexOfRetentionLabelName + 1, "RM_TM_Excel_RecordLabel");
                            rules.Insert(indexOfRetentionLabelName, "RM_TM_Excel_RetentionLabel");
                        }

                    }
                    return rules;

                default:
                    return null;
            }
        }

        public static void CreateSheetContentRow(SheetData sheetData, SharedStringTable sharedStringTable, int rowIndex, List<string> rowDatas, SheetType sheetType, ExportAddition exportAddition = null)
        {
            Row row = new Row();
            var i = 0;
            int termSheetColumnCount = TermsSheetColumnCount;
            if (exportAddition != null && exportAddition.TermColumArray != null)
            {
                termSheetColumnCount += exportAddition.TermColumArray.Count();
            }
            var rulesSheetColumnCount = RulesSheetColumnCount;
            if (exportAddition != null && exportAddition.RuleColumArray != null)
            {
                rulesSheetColumnCount += exportAddition.RuleColumArray.Count();
            }
            if(exportAddition?.IsSupportRecordLabelFunction ?? false)
            {
                rulesSheetColumnCount += 3;
                if(sheetType == SheetType.Rules && exportAddition.IsNeedAddRowData)
                {
                    rowDatas.Insert(18,null);
                    rowDatas.Insert(35,null);
                    rowDatas.Insert(34,null);
                }
            }
            foreach (var item in Enum.GetValues(typeof(ExcelHeadColumn)))
            {
                if (i == termSheetColumnCount && sheetType == SheetType.Terms)
                {
                    break;
                }
                if (i == rulesSheetColumnCount && sheetType == SheetType.Rules)
                {
                    break;
                }
                var cellRef = item.ToString() + (rowIndex + 1);
                var cellValueStr = ReplaceLowOrderASCIICharacters(rowDatas[i]);
                var index = InsertSharedStringItem(cellValueStr, sharedStringTable);
                Cell cell = null;
                if ((exportAddition?.IsSupportRecordLabelFunction ?? false) && sheetType == SheetType.Rules)
                {
                    if (!string.IsNullOrEmpty(cellValueStr) && cellValueStr.Equals(Action_KeepDataActionString, StringComparison.OrdinalIgnoreCase))
                    {
                        cell = new()
                        {
                            CellValue = new CellValue(New_Action_KeepDataActionString),
                            DataType = new EnumValue<CellValues>(CellValues.String)
                        };
                        row.Append(cell);
                        i++;
                        continue;
                    }
                }
                cell = new()
                {
                    CellValue = new CellValue(index.ToString()),
                    CellReference = new StringValue(cellRef),
                    DataType = new EnumValue<CellValues>(CellValues.SharedString),
                };
                row.Append(cell);
                i++;
            }
            sheetData.Append(row);
        }

        public static string ReplaceLowOrderASCIICharacters(string tmp)
        {
            if (string.IsNullOrEmpty(tmp))
            {
                return tmp;
            }
            StringBuilder info = new StringBuilder();
            foreach (char cc in tmp)
            {
                int ss = (int)cc;
                if (((ss >= 0) && (ss <= 8)) || ((ss >= 11) && (ss <= 12)) || ((ss >= 14) && (ss <= 32)))
                    info.AppendFormat(" ", ss);
                else info.Append(cc);
            }
            return info.ToString();
        }

        #endregion
        #region create validations sheet 
        public static void CreateValidationsSheet(string name, SpreadsheetDocument spreadsheet, Sheets sheets, WorksheetPart worksheetPart, ExportAddition exportAddition = null)
        {
            Sheet dvSheet = new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = name
            };
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            CreateValidationHeadRow(sheetData);
            CreateValidationCells(sheetData, exportAddition);
            dvSheet.State = SheetStateValues.VeryHidden;
            sheets.Append(dvSheet);
        }

        public static void CreateValidationHeadRow(SheetData sheetData)
        {
            Row headRow = new Row();
            //List<string> colHeadNames = new List<string>();
            foreach (ValidationsHeadColumn item in Enum.GetValues(typeof(ValidationsHeadColumn)))
            {
                var colName = I18NEntity.GetString(item.ToDescription());
                var cellRef = item.ToString() + 1;
                //colHeadNames.Add(colName);
                Cell cell = new Cell()
                {
                    DataType = CellValues.String,
                    CellReference = new StringValue(cellRef),
                    CellValue = new CellValue(colName)
                };
                headRow.Append(cell);
            }
            sheetData.Append(headRow);
        }

        public static void CreateValidationCells(SheetData sheetData, ExportAddition exportAddition = null)
        {
            List<ValidationsHeadColumn> colHeads = new List<ValidationsHeadColumn>() {
                ValidationsHeadColumn.A,
                ValidationsHeadColumn.B,
                ValidationsHeadColumn.C,
                ValidationsHeadColumn.D,
                ValidationsHeadColumn.E,
                ValidationsHeadColumn.F,
                ValidationsHeadColumn.G,
                ValidationsHeadColumn.H,
                ValidationsHeadColumn.I,
                ValidationsHeadColumn.J,
                ValidationsHeadColumn.K,
                ValidationsHeadColumn.L,
                ValidationsHeadColumn.M,
                ValidationsHeadColumn.N,
                ValidationsHeadColumn.O,
            };
            #region add null row for select null
            Row nullRow = new Row();
            foreach (var item in colHeads)
            {
                nullRow.Append(new Cell()
                {
                    DataType = CellValues.String,
                    //CellReference start from 1
                    CellReference = item.ToString() + "2",
                    CellValue = new CellValue("")
                });
            }
            sheetData.Append(nullRow);
            #endregion

            #region add validation cells
            foreach (var item in colHeads)
            {
                List<string> datas = GetValidationsCellSource(item, exportAddition);
                for (int i = 0; i < datas.Count; i++)
                {
                    var cellRef = item.ToString() + (i + 3);
                    var rowIndex = i + 2;
                    IEnumerable<Row> rows = sheetData.Descendants<Row>();
                    Row row = null;
                    bool isExistRow = false;
                    if (rows != null && rows.Count() > 0)
                    {
                        try
                        {
                            //RowIndex start from 0.
                            row = rows.ElementAt<Row>(rowIndex);
                            isExistRow = true;
                        }
                        catch (Exception ex)
                        {
                            isExistRow = false;
                        }
                    }

                    if (row == null)
                    {
                        row = new Row();
                    }
                    string cellValueStr = I18NEntity.GetString(datas[i]);
                    cellValueStr = string.IsNullOrEmpty(cellValueStr) ? "" : cellValueStr.TrimEnd(':');
                    Cell cell = new Cell()
                    {
                        DataType = CellValues.String,
                        //CellReference start from 1
                        CellReference = cellRef,
                        CellValue = new CellValue(cellValueStr)
                    };
                    row.Append(cell);
                    if (!isExistRow)
                    {
                        sheetData.Append(row);
                    }
                }
            }
            #endregion
        }

        public static List<string> GetValidationsCellSource(ValidationsHeadColumn colHead, ExportAddition exportAddition = null)
        {
            List<string> dataSources = null;
            switch (colHead)
            {
                case ValidationsHeadColumn.A:
                    dataSources = new List<string>() { "RM_JS_Rule_ObjectLevel_Document", "RM_JS_Rule_ObjectLevel_Item", "RM_JS_Rule_ObjectLevel_Folder", "RM_JS_Rule_ObjectLevel_List", "RM_JS_Rule_ObjectLevel_Site", "RM_JS_Rule_ObjectLevel_SiteCollection" };
                    break;
                case ValidationsHeadColumn.B:
                    dataSources = new List<string>() { "RM_JS_Rule_ConditionAnd", "RM_JS_Rule_ConditionOr" };
                    break;
                case ValidationsHeadColumn.C:
                    dataSources = new List<string>() {
                    "Name",
                    "DocumentSize",
                    "ModifiedTime",
                    "CreatedTime",
                    "CreatedBy",
                    "ModifiedBy",
                    "Label property (Date)",
                    "Label property (Number)",
                    "Label property (Text)",
                    "LabelName",
                    "ContentType",
                    "TextColumn",
                    "NumberColumn",
                    "BooleanColumn",
                    "DateTimeColumn",
                    "ParentListTypeID",
                    "LastAccessedTime",
                    "LastAccessedTime/ModifiedTime",
                    "Title",
                    "URL",
                    "TextCustomProperty",
                    "NumberCustomProperty",
                    "BooleanCustomProperty",
                    "DateTimeCustomProperty",
                    "PrimaryAdministrator",
                    "SiteCollectionSizeTrigger",
                    "Subject",
                    "Size",
                    "AttachmentCount",
                    "SendDate",
                    "SendFrom",
                    "SendTo",
                    "Parent folder name",
                    "Parent folder name (Including subfolders)",
                    "ParentLibraryName",
                    "MetadataTextColumn",
                    "MetadataNumberColumn",
                    "Type",
                    "Owner",
                    "FilePath",
                    "RetentionLabel",
                    "SensitivityLabel (display name)",
                    "SensitivityLabel (full name)",
                    "Parent library property (Text)",
                    "Parent library property (Number)",
                    "Parent library property (Yes/No)",
                    "Parent library property (Date and Time)",
                    "Parent site collection (Text)",
                    "Parent site collection (Number)",
                    "Parent site collection (Yes/No)",
                    "Parent site collection (Date and Time)",
                    "Parent site property (Text)",
                    "Parent site property (Number)",
                    "Parent site property (Yes/No)",
                    "Parent site property (Date and Time)",
                    "Latest subfolder action due date",
                    "Orphaned folder"
                    };
                    break;
                case ValidationsHeadColumn.D:
                    dataSources = new List<string>() {
                                                        "Matches",
                                                        "DoesNotMatch",
                                                        "Contains",
                                                        "DoesNotContain",
                                                        "Equals",
                                                        "DoesNotEqual",
                                                        "GreaterThanOrEqualTo",
                                                        "LessThanOrEqualTo",
                                                        "FromTo",
                                                        "Before",
                                                        "OlderThan",
                                                        "IsBlank",
                                                    };
                    dataSources.AddRange(exportAddition?.ConditionArray ?? []);
                    break;
                case ValidationsHeadColumn.E:
                    dataSources = new List<string>() { "Days", "Weeks", "Months", "Years", "KB", "MB", "GB" };
                    break;
                case ValidationsHeadColumn.F:
                    dataSources = new List<string>() { "RM_TM_Excel_RemoveContent", "RM_TM_Excel_KeepContent", "RM_TM_Excel_MoveContent", "Export content", "RM_TM_Excel_ArchiveContent", "RM_JS_RDM_CreateRule_Options_CalculateDisposalDate" };
                    if(exportAddition?.IsSupportRecordLabelFunction ?? false)
                    {
                        var indexOfKeepContent = dataSources.IndexOf("RM_TM_Excel_KeepContent");
                        if(indexOfKeepContent != -1)
                        {
                            dataSources[indexOfKeepContent] = "RM_JS_RDM_CreateRule_Options_TagOrLock";
                        }
                    }
                    break;
                case ValidationsHeadColumn.G:
                    dataSources = GeneralSettingConfig.TimeZones.Select(t => t.DisplayName).ToList();
                    break;
                case ValidationsHeadColumn.H:
                    dataSources = new List<string>() { "VEO", "NAA", "NARA" };
                    break;
                case ValidationsHeadColumn.I:
                    dataSources = new List<string>() { "RM_TM_Excel_SharePointOnline", "RM_JS_SPS_TabLabel_EXO", "RM_JS_SPS_TabLabel_Physical", "RM_JS_SPS_TabLabel_FS", "RM_JS_SPS_TabLabel_SPLocal", "RM_JS_SPS_TabLabel_OneDrive", "RM_JS_Common_ReportType_AzureFile", "Connector", "RM_JS_SPS_TabLabel_Box", "RM_JS_SPS_TabLabel_GoogleDrive" };
                    break;
                case ValidationsHeadColumn.J:
                    dataSources = new List<string>() { "Email" };
                    break;
                case ValidationsHeadColumn.K:
                    dataSources = new List<string>() { "RM_JS_RDM_CreateRule_TagType_Text", "RM_JS_RDM_CreateRule_TagType_Nubmer", "RM_JS_RDM_CreateRule_TagType_DateTime", "RM_JS_RDM_CreateRule_TagType_YesNo" };
                    break;
                case ValidationsHeadColumn.L:
                    dataSources = new List<string>() { "RM_TM_Excel_Skip", "RM_TM_Excel_Overwrite", "RM_TM_Excel_AddSuffix" };
                    break;
                case ValidationsHeadColumn.M:
                    dataSources = new List<string>() { "RM_TM_Excel_AlwaysActive", "RM_TM_Excel_TakeEffectFrom", "RM_TM_Excel_RetireAfter", "RM_TM_Excel_ActiveFrom" };
                    break;
                case ValidationsHeadColumn.N:
                    dataSources = new List<string>() { "RM_JS_SPS_AutoClassification_Any", "RM_TM_Excel_SharePointOnline", "RM_JS_Common_ReportType_Exchange", "RM_JS_SPS_TabLabel_OneDrive" };
                    if (exportAddition?.HasUpgradeTeams ?? false) dataSources.Add("RM_JS_SPS_TabLabel_Teams");
                    break;
                case ValidationsHeadColumn.O:
                    dataSources = new List<string>() { "Manual approval process", "Record owner" };
                    break;
                default:
                    break;
            }
            return dataSources;
        }

        static int InsertSharedStringItem(string text, SharedStringTable sharedStringTable)
        {
            int i = 0;
            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in sharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            sharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            sharedStringTable.Save();

            return i;
        }
        #endregion

        public class ExcelValidaton
        {
            public Worksheet workSheet { get; set; }
            public string validationSheetName { get; set; }
            public Dictionary<string, string> dropDownListSourceMapping { get; set; }
            public ExcelValidaton(Worksheet ws, string validationSheetName, List<string[]> validationSheetRelations)
            {
                this.workSheet = ws;
                this.validationSheetName = validationSheetName;
                this.dropDownListSourceMapping = ConvertToValidationRelationships(validationSheetRelations);
            }
            public Dictionary<string, string> ConvertToValidationRelationships(List<string[]> arrayRelationShips)
            {
                Dictionary<string, string> validationRelationships = new Dictionary<string, string>();
                if (arrayRelationShips != null && arrayRelationShips.Count > 0)
                {
                    foreach (var item in arrayRelationShips)
                    {
                        //"L", "2", "1048576", "N", "3", "5"
                        string sourceCellName = item[0];
                        string sourceStartIndex = item[1];
                        string sourceEndIndex = item[2];
                        string validationCellName = item[3];
                        string validationStartIndex = item[4];
                        string validationEndIndex = item[5];
                        validationRelationships.Add(
                            $"{sourceCellName}{sourceStartIndex}:{sourceCellName}{sourceEndIndex}",
                            $"'{validationSheetName}'!${validationCellName}${validationStartIndex}:${validationCellName}${validationEndIndex}"
                            );
                    }
                }
                return validationRelationships;
            }
            public void UpdateForSheet()
            {
                DataValidations dataValidations = new DataValidations();
                foreach (KeyValuePair<string, string> kv in dropDownListSourceMapping)
                {
                    var cellRef = kv.Key;
                    var validataionCellRefRange = kv.Value;
                    DataValidation dataValidation = new DataValidation
                    {
                        Type = DataValidationValues.List,
                        AllowBlank = true,
                        SequenceOfReferences = new ListValue<StringValue> { InnerText = cellRef }
                    };
                    Formula1 formula = new Formula1();
                    dataValidation.Append(
                        new Formula1(validataionCellRefRange)
                    );
                    dataValidations.Append(dataValidation);
                }
                var wsp = workSheet.WorksheetPart;
                wsp.Worksheet.AppendChild(dataValidations);
            }
        }
        #endregion

        #region Export CSV discovery data
        private static readonly Dictionary<string, Func<Dictionary<string, object>, string>> InactiveColumnMappings = new()
        {
            [I18NEntity.GetString("RM_FA_TableColumn_Container")] = row => row.GetValueOrDefault("Container")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_TableColumn_SiteCollection")] = row => row.GetValueOrDefault("SiteCollection")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_TableColumn_InScope")] = row => row.GetValueOrDefault("InScope")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_SF_Inactive_OptimizationTab_DataSizeTitle")] = row => ConvertToGB(row.GetValueOrDefault("FileTotalSize")),
            [I18NEntity.GetString("RM_FA_GoogleDrive_Inactive_TableColumn_FileCount")] = row => row.GetValueOrDefault("FileSumCount")?.ToString() ?? "",
            [I18NEntity.GetString("RM_DA_Profile_ProfileInactiveDataSizeGB")] = row => ConvertToGB(row.GetValueOrDefault("InactiveFileTotalSize")),
            [I18NEntity.GetString("RM_FA_Inactive_TableColumn_OptimizableFileCount")] = row => row.GetValueOrDefault("InactiveFileSumCount")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_TableColumn_Rate")] = row => CalculatePercentage(row.GetValueOrDefault("FileTotalSize"), row.GetValueOrDefault("InactiveFileTotalSize")),
            [I18NEntity.GetString("RM_FA_Inactive_SummaryTab_PHL") + " (" + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_GB") + ")"] = row => ConvertToGB(row.GetValueOrDefault("PHLTotalSize")),
            [I18NEntity.GetString("RM_FA_TableColumn_Saving") + " " + I18NEntity.GetString("RM_FA_TableColumn_Saving_Unit_Monthly")] = row => row.GetValueOrDefault("CostSaving")?.ToString() ?? "0"
        };

        private static readonly Dictionary<string, Func<Dictionary<string, object>, string>> RotColumnMappings = new()
        {
            [I18NEntity.GetString("RM_FA_TableColumn_Container")] = row => row.GetValueOrDefault("Container")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_TableColumn_SiteCollection")] = row => row.GetValueOrDefault("SiteCollection")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_TableColumn_InScope")] = row => row.GetValueOrDefault("InScope")?.ToString() ?? "",
            [I18NEntity.GetString("RM_FA_SF_Inactive_OptimizationTab_DataSizeTitle")] = row => ConvertToGB(row.GetValueOrDefault("FileTotalSize")),
            [I18NEntity.GetString("RM_FA_ROT_TableColumn_ROTTotalSize")] = row => ConvertToGB(row.GetValueOrDefault("RotFileTotalSize")),
            [I18NEntity.GetString("RM_FA_TableColumn_Rate")] = row => CalculatePercentage(row.GetValueOrDefault("FileTotalSize"), row.GetValueOrDefault("RotFileTotalSize")),
            [I18NEntity.GetString("RM_FA_TableColumn_Saving") + " " + I18NEntity.GetString("RM_FA_TableColumn_Saving_Unit_Monthly")] = row => row.GetValueOrDefault("CostSavingMonthlyBySize")?.ToString() ?? "0",
            [I18NEntity.GetString("RM_FA_ROT_TableColumn_Redundant")] = row => ConvertToGB(row.GetValueOrDefault("RCategoryFileTotalSize")),
            ["Cost saving redundant (monthly)"] = row => row.GetValueOrDefault("CostSavingMonthlyByRedundant")?.ToString() ?? "0",
            [I18NEntity.GetString("RM_FA_ROT_TableColumn_Obsolete")] = row => ConvertToGB(row.GetValueOrDefault("OCategoryFileTotalSize")),
            ["Cost saving obsolete (monthly)"] = row => row.GetValueOrDefault("CostSavingMonthlyByObsolete")?.ToString() ?? "0",
            [I18NEntity.GetString("RM_FA_ROT_TableColumn_Trivial")] = row => ConvertToGB(row.GetValueOrDefault("TCategoryFileTotalSize")),
            ["Cost saving trivial (monthly)"] = row => row.GetValueOrDefault("CostSavingMonthlyByTrivial")?.ToString() ?? "0"
        };

        private static readonly string Separator = ",";
        private static readonly string Quote = "\"";

        private static string ConvertToGB(object bytesObj)
        {
            const int numberOfDecimal = 2;
            if (bytesObj == null || !double.TryParse(bytesObj.ToString(), out var bytes)) return "0";
            var gb = bytes / 1024d / 1024d / 1024d;
            var rounded = Math.Round(gb, numberOfDecimal, MidpointRounding.AwayFromZero);

            var minimumDisplayValue = Math.Pow(10, -numberOfDecimal);

            if (gb > 0 && gb < minimumDisplayValue)
            {
                rounded = minimumDisplayValue;
            }

            return rounded.ToString($"F{numberOfDecimal}");
        }

        private static string CalculatePercentage(object totalObj, object partObj)
        {
            if (!double.TryParse(totalObj?.ToString(), out var totalBytes) || totalBytes < 1E-06)
                return "0%";
            if (!double.TryParse(partObj?.ToString(), out var partBytes))
                return "0%";

            const int numberOfDecimal = 2;
            var minimumDisplayValue = Math.Pow(10, -numberOfDecimal);

            static double ConvertBytesToDisplayedGb(double bytes, double minimumDisplayValue)
            {
                var gb = bytes / 1024d / 1024d / 1024d;
                var rounded = Math.Round(gb, numberOfDecimal, MidpointRounding.AwayFromZero);
                return gb > 0 && gb < minimumDisplayValue ? minimumDisplayValue : rounded;
            }

            var totalGB = ConvertBytesToDisplayedGb(totalBytes, minimumDisplayValue);
            var partGB = ConvertBytesToDisplayedGb(partBytes, minimumDisplayValue);

            if (totalGB < 1E-06) return "0%";

            var ratioText = (partGB / totalGB).ToString("0.00", CultureInfo.InvariantCulture);
            var temp = (int.TryParse(ratioText.Replace(".", string.Empty), out var percentage) ? percentage : 0) + "%";

            return temp == "0%" && partGB != 0 ? "1%" : temp;
        }

        public static void WriteCsvHeader(string path, List<string> columnOrder)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(stream, new UTF8Encoding(true)); // UTF-8 BOM
            writer.Write('\uFEFF');
            writer.WriteLine(string.Join(",", columnOrder.Select(EscapeCsv)));
        }

        public static void AppendDiscoveryDataToCsv(string path, List<Dictionary<string, object>> rows, List<string> ruleCols, Dictionary<string, string> ruleMap, RMDiscoveryProfileType type)
        {
            var allRuleNames = ruleCols ?? new();
            var mappings = type == RMDiscoveryProfileType.Inactive ? InactiveColumnMappings : RotColumnMappings;

            var dataRows = new List<Dictionary<string, string>>();
            foreach (var row in rows)
            {
                var formatted = new Dictionary<string, string>();
                foreach (var kvp in mappings)
                    formatted[kvp.Key] = kvp.Value(row);

                foreach (var ruleId in allRuleNames)
                {
                    if (ruleMap.TryGetValue(ruleId, out var displayName))
                    {
                        var colName = displayName + $" ({I18NEntity.GetString("RM_DSB_Unit_GB")})";
                        var value = row.TryGetValue(ruleId, out var v) ? ConvertToGB(v) : "0";
                        formatted[colName] = value;
                    }
                }
                dataRows.Add(formatted);
            }

            var columnOrder = GetColumnOrder(type, ruleCols, ruleMap);
            using var stream = new FileStream(path, FileMode.Append, FileAccess.Write);
            using var writer = new StreamWriter(stream, new UTF8Encoding(true));
            foreach (var row in dataRows)
            {
                var line = string.Join(",", columnOrder.Select(col => row.ContainsKey(col) ? EscapeCsv(row[col]) : ""));
                writer.WriteLine(line);
            }
        }

        public static List<string> GetColumnOrder(RMDiscoveryProfileType type, List<string> ruleCols, Dictionary<string, string> ruleMap)
        {
            var baseCols = type == RMDiscoveryProfileType.Inactive ? InactiveColumnMappings.Keys : RotColumnMappings.Keys;
            return baseCols.Concat(ruleCols.Where(ruleMap.ContainsKey).Select(id => ruleMap[id] + $" ({I18NEntity.GetString("RM_DSB_Unit_GB")})")).ToList();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            bool needQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (needQuote)
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
        #endregion

        #region Csv Method

        public static void ExportDataToCsv(string[][] datas, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath, true, System.Text.Encoding.UTF8))
            {
                foreach (var data in datas)
                {
                    if(data == null) continue;
                    var line = ToCSVString(data);
                    writer.WriteLine(line);
                }
            }
        }

        private static string ToCSVString(string[] fields)
        {
            var b = new StringBuilder();
            foreach (string fld in fields)
            {
                if (b.Length > 0)
                {
                    b.Append(',');
                }
                var tempFld = (fld.IsNullOrEmpty() ? "" : fld);
                string field = EscapeString(tempFld, new char[] { '\"' }, '\"');
                b.Append('\"').Append(field).Append('\"');
            }
            return b.ToString();
        }

        private static string EscapeString(string s, char[] charsToEscape, char escapeChar)
        {
            var result = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == escapeChar)
                {
                    result.Append(escapeChar);
                }
                else
                {
                    foreach (char charToEscape in charsToEscape)
                    {
                        if (c == charToEscape)
                        {
                            result.Append(escapeChar);
                            break;
                        }
                    }
                }
                result.Append(c);
            }
            return result.ToString();
        }

        #endregion
    }

    public enum ValidationsHeadColumn
    {
        [Description("Rule Level")]
        A,
        [Description("Combine Mode")]
        B,
        [Description("Criteria Type")]
        C,
        [Description("Criteria Condition")]
        D,
        [Description("Condition Value Unit")]
        E,
        [Description("Rule Action")]
        F,
        [Description("TimeZones")]
        G,
        [Description("Export Format")]
        H,
        [Description("Source Type")]
        I,
        [Description("Category")]
        J,
        [Description("RM_TM_Excel_CustomColumnType")]
        K,
        [Description("RM_TM_Excel_ConflictResolution")]
        L,
        [Description("RM_TM_Excel_TermActivationSettings")]
        M,
        [Description("RM_TM_Excel_RetentionSourceType")]
        N,
        [Description("Manual Approval Type")]
        O
    }

    public enum SheetType
    {
        Terms,
        Rules,
        Inactive,
        ROT
    }

    public enum ExcelHeadColumn
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, AA, AB, AC, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AM, AN, AO, AP, AQ, AR, AS, AT, AU, AV, AW, AX, AY, AZ, BA, BB, BC, BD
    }

    public static class TermAndRuleTemplateVersion
    {
        public const string PROPERTIES_KEY = "Opus Template Version";

        public const string PROPERTIES_VALUE = "3.0";
    }

    public static class JPMCTemplateColumn
    {
        public const string ADDITION_RULE_COL = "Notes";

        public const string ADDITION_CONTITION = "ListIn";
    }
}
