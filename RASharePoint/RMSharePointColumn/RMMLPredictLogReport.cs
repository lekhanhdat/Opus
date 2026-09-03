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
using Aspose.Email.Storage.Olm;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMLPredictLogReport
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMMLPredictLogReport));
        private bool isDevEnv = RMGlobalConfiguration.EnvSetting.IsDevEnvironment;
        private string JobLogFolderPath;
        private string CurrentPredictFilePath;
        private string CurrentTermSheetName;
        private string CurrentPredictSheetName;
        private int CountOfOneSheet = 10;
        private int TermSheetIndex;
        private int PredictSheetIndex;
        private int CurrentRow;
        private bool IsCreateExcel;
        private bool IsCreatePredictSheet = false;
        private string[] TermColumns = new string[3]
        {
            "Term Name",
            "Term ID",
            "Document characteristics(Term Description AI)"
        };
        private string[] PredictColumns = new string[5]
        {
            "File path",
            "File ID",
            "Predict Term",
            "File summary",
            "Prediction scores(top 5)"
        };
        private SpreadsheetDocument PredictStreamingDocument;
        private WorkbookPart PredictStreamingWorkbookPart;
        private OpenXmlWriter PredictStreamingWriter;
        private uint PredictStreamingRowIndex = 1;
        public void Init(string tenantId, string jobId)
        {
            try
            {
                JobLogFolderPath = GetJobLogFolderPath(tenantId, jobId);
                CurrentPredictFilePath = Path.Combine(JobLogFolderPath, $"Predict_Report_{jobId}_.xlsx");
                if (!Directory.Exists(JobLogFolderPath))
                {
                    Directory.CreateDirectory(JobLogFolderPath);
                }
                CurrentTermSheetName = "Terms";
                CurrentPredictSheetName = "Files";
            }
            catch (Exception e)
            {
                logger.Error($"Init Predict log report have errors:{e}");
            }
        }

        public static string GetJobLogFolderPath(string tenantId, string jobId)
        {
            string pattern = @"^[a-zA-Z0-9-_]+$";
            Regex regex = new Regex(pattern);

            if (!regex.IsMatch(jobId))
            {
                throw new ArgumentException("Invalid args jobId.");
            }

            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                if (!Guid.TryParse(tenantId, out var parsedGuid))
                {
                    throw new ArgumentException("Invalid args tenantId.");
                }

                return Path.Combine(Path.DirectorySeparatorChar + "logs", tenantId, jobId);
            }

            return Path.Combine(Path.DirectorySeparatorChar + "logs", "Reports");
        }

        public void WriteTermNewRow(List<TermReportInfo> termInfo)
        {
            string[] ConvertTermInfoToArray(TermReportInfo termReportInfo)
            {
                string[] data = new string[3];
                data[0] = termReportInfo.TermName;
                data[1] = termReportInfo.TermID;
                data[2] = termReportInfo.AITermDescription;
                return data;
            }
            string[][] datas;
            CurrentRow += termInfo.Count + 1;
            if(!IsCreateExcel)
            {
                IsCreateExcel = true;
                datas = ConvertDataToArray(termInfo, ConvertTermInfoToArray, TermColumns);
                ReportUtil.CreateExcel(CurrentPredictFilePath, CurrentTermSheetName, datas);
                return;
            }
            if(CurrentRow >= CountOfOneSheet)
            {
                datas = ConvertDataToArray(termInfo, ConvertTermInfoToArray, TermColumns);
                CurrentTermSheetName = TermSheetIndex == 0 ? CurrentTermSheetName :
                    (CurrentTermSheetName.Contains("_") ? CurrentTermSheetName.Replace($"{TermSheetIndex}", $"{++TermSheetIndex}") : (CurrentTermSheetName + $"_{++TermSheetIndex}"));
                ReportUtil.InsertWorksheet(CurrentPredictFilePath, CurrentTermSheetName, datas);
                TermSheetIndex++;
                return;
            }
            datas = ConvertDataToArray(termInfo, ConvertTermInfoToArray, null);
            ReportUtil.InsertDataToSheet(CurrentPredictFilePath, datas, TermSheetIndex);
        }

        public void AppendPredictFileBatch(List<PredictFileInfo> predictFileInfoBatch)
        {
            if (predictFileInfoBatch == null || predictFileInfoBatch.Count == 0)
            {
                return;
            }

            EnsurePredictStreamingWriterInitialized();
            foreach (var predictFileInfo in predictFileInfoBatch)
            {
                WriteInlineRow(PredictStreamingWriter, PredictStreamingRowIndex++, CreatePredictRow(predictFileInfo));
            }
        }

        public void CompletePredictFileStreaming()
        {
            try
            {
                if (PredictStreamingWriter != null)
                {
                    PredictStreamingWriter.WriteEndElement();
                    PredictStreamingWriter.WriteEndElement();
                    PredictStreamingWriter.Dispose();
                    PredictStreamingWriter = null;
                }

                PredictStreamingWorkbookPart?.Workbook?.Save();
            }
            finally
            {
                PredictStreamingDocument?.Dispose();
                PredictStreamingDocument = null;
                PredictStreamingWorkbookPart = null;
                PredictStreamingRowIndex = 1;
            }
        }

        private string[][] ConvertDataToArray<T>(List<T> datas, Func<T,string[]> func, string[] firstRow = null) 
            where T : class
        {
            var result = new string[firstRow != null ? datas.Count + 1 : datas.Count][];
            int index = firstRow != null ? 1 : 0;
            if (firstRow != null) result[0] = firstRow;
            datas.ForEach(data =>
            {
                result[index] = func(data);
                index++;
            });
            return result;
        }

        public void ResetIndex()
        {
            CurrentRow = 0;
        }

        private void EnsurePredictStreamingWriterInitialized()
        {
            if (PredictStreamingWriter != null)
            {
                return;
            }

            EnsurePredictWorkbookExists();
            var sheetName = GetNextPredictSheetNameForStreaming();
            PredictStreamingDocument = SpreadsheetDocument.Open(CurrentPredictFilePath, true);
            PredictStreamingWorkbookPart = PredictStreamingDocument.WorkbookPart;
            if (PredictStreamingWorkbookPart == null)
            {
                PredictStreamingWorkbookPart = PredictStreamingDocument.AddWorkbookPart();
                PredictStreamingWorkbookPart.Workbook = new Workbook();
            }

            if (PredictStreamingWorkbookPart.Workbook == null)
            {
                PredictStreamingWorkbookPart.Workbook = new Workbook();
            }

            Sheets sheets = PredictStreamingWorkbookPart.Workbook.GetFirstChild<Sheets>();
            if (sheets == null)
            {
                sheets = PredictStreamingWorkbookPart.Workbook.AppendChild(new Sheets());
            }

            WorksheetPart worksheetPart = PredictStreamingWorkbookPart.AddNewPart<WorksheetPart>();
            string relationshipId = PredictStreamingWorkbookPart.GetIdOfPart(worksheetPart);
            uint sheetId = sheets.Elements<Sheet>().Any()
                ? sheets.Elements<Sheet>().Max(_ => _.SheetId.Value) + 1
                : 1;

            sheets.Append(new Sheet
            {
                Id = relationshipId,
                SheetId = sheetId,
                Name = sheetName
            });

            PredictStreamingWriter = OpenXmlWriter.Create(worksheetPart);
            PredictStreamingWriter.WriteStartElement(new Worksheet());
            PredictStreamingWriter.WriteStartElement(new SheetData());
            WriteInlineRow(PredictStreamingWriter, PredictStreamingRowIndex++, PredictColumns);
        }

        private void EnsurePredictWorkbookExists()
        {
            if (File.Exists(CurrentPredictFilePath))
            {
                return;
            }

            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(CurrentPredictFilePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = spreadsheet.AddWorkbookPart();
                workbookPart.Workbook = new Workbook(new Sheets());
                workbookPart.Workbook.Save();
            }
        }

        private string GetNextPredictSheetNameForStreaming()
        {
            if (!IsCreateExcel)
            {
                IsCreateExcel = true;
                return CurrentPredictSheetName;
            }

            if (!IsCreatePredictSheet)
            {
                IsCreatePredictSheet = true;
                PredictSheetIndex = 1;
                return CurrentPredictSheetName;
            }

            PredictSheetIndex++;
            return $"{CurrentPredictSheetName}_{PredictSheetIndex}";
        }

        private static string[] CreatePredictRow(PredictFileInfo predictFileInfo)
        {
            return new[]
            {
                predictFileInfo?.FilePath ?? string.Empty,
                predictFileInfo?.FileID ?? string.Empty,
                predictFileInfo?.PredictTerm ?? string.Empty,
                predictFileInfo?.FileSummary ?? string.Empty,
                predictFileInfo?.PredictionScores ?? string.Empty
            };
        }

        private static void WriteInlineRow(OpenXmlWriter writer, uint rowIndex, IReadOnlyList<string> values)
        {
            writer.WriteStartElement(new Row { RowIndex = rowIndex });

            foreach (var value in values)
            {
                writer.WriteStartElement(new Cell { DataType = CellValues.InlineString });
                writer.WriteElement(new InlineString(new Text(value ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }

    public class TermReportInfo
    {
        public string TermName { get; set; }
        public string TermID { get; set; }
        public string AITermDescription { get; set; }
    }

    public class PredictFileInfo
    {
        public string FilePath { get; set; }
        public string FileID { get; set; }
        public string PredictTerm { get; set; }
        public string FileSummary { get; set; }
        public string PredictionScores {get; set;}
    }
}
