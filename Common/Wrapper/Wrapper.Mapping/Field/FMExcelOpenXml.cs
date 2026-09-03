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





namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using System.IO;
    using DocumentFormat.OpenXml;
    using System.Xml;
    using System.Text.RegularExpressions;
    using System.Collections;
    using System.Data;
    using AvePoint.Wrapper.Common;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Wrapper.Resource;

    public class FMExcelOpenXml : IDisposable
    {
        private static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public Dictionary<string, string> mInsensitiveTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> InsensitiveTableHeader
        {
            get { return mInsensitiveTableHeader; }
        }
        protected DocumentFormat.OpenXml.Spreadsheet.Workbook mCurrentWorkbook = null;
        protected Dictionary<string, int> mCurrentTableHeader = new Dictionary<string, int>();//(columnName,sequence)
        //protected Dictionary<string, string> mInsensitiveTableHeader = new Dictionary<string, string>();//(columnName,sequence)
        protected Dictionary<string, bool> mFilterTable = new Dictionary<string, bool>();
        protected int mCurrentSheetColCount = 0;
        protected int mCurrentSheetRowCount = 0;
        protected int mCurrentSheetCardRowsTotal = -1;
        protected int mCurrentRowIndexRead = 2;
        protected int mCurrentRowIndexWrite = 0;    //写入时API要求从0行开始写入
        protected int mCurrentSheetIndex = 0;
        protected DocumentFormat.OpenXml.Spreadsheet.Sheets mExcelSheets = null;
        protected WorkbookPart mCurrentWorkbooPart = null;
        protected SharedStringTablePart mSharedStringTablePart = null;
        protected WorksheetPart mCurrentWorksheetPart = null;
        protected Worksheet mCurrentWorksheet = null;
        protected List<string> mSheetIdList = null;
        protected SpreadsheetDocument mDoc = null;
        protected IEnumerable<Row> mReadLineRows = null;
        /// <summary>
        /// zj 缓存excel的cell和行数的关系，方便定位，否则多次foreach浪费效率,假设excel 5行5列
        /// 如果你没修改过excel，那么会是25个row，每个row1个cell
        /// 如果你修改过excel，那么会是5个row，每个row5个cell- -
        /// </summary>
        protected Dictionary<string, List<Cell>> mExcelCellsDic = new Dictionary<string, List<Cell>>();
        protected string mRelId = null;
        protected int mSheetIdListPosition = 0;
        private Dictionary<string, string> mHeader = new Dictionary<string, string>();
        private string mExcelPath = null;
        private List<string> mSheetNameList = null;
        private bool mIsCreateFailed = false;
        private int mFileCount = 0;
        private int dataCurrentSheet = 0;

        public FMExcelOpenXml()
        {

        }

        //~FMExcelOpenXml()
        //{
        //    Dispose();
        //}
        public void Dispose()
        {
            Quit();
        }
        private void Quit()
        {
            try
            {
                dataCurrentSheet = 0;
                Save();
                mDoc.Dispose();
            }
            catch (Exception e)
            {
                log.Error(" Quit Error.Exception:" + e.ToString());
            }
        }

        public void CreateExcel(string excelPath)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.CreateExcel"))
            {
#endif
                if (excelPath.Length >= 255)
                {
                    throw new Exception("PathTooLong");
                }

                //delete first
                try
                {
                    if (File.Exists(excelPath))
                    {
                        File.Delete(excelPath + ".bak");
                        File.Move(excelPath, excelPath + ".bak");
                    }
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Create(excelPath, SpreadsheetDocumentType.Workbook))
                    {
                        WorkbookPart workbookPart = doc.AddWorkbookPart();
                        workbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                        workbookPart.Workbook.AppendChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

                        SharedStringTablePart sharestringTablePart = workbookPart.AddNewPart<SharedStringTablePart>();
                        sharestringTablePart.SharedStringTable = new SharedStringTable();
                        sharestringTablePart.SharedStringTable.Save();

                        //下边的代码是创建 sheet
                        //uint count = fileCount == 0 ? 1 : (uint)Math.Ceiling((double)fileCount / CommonFunc.ROWSCOUNT);
                        uint count = 1;
                        for (uint sheetId = 1; sheetId <= count; sheetId++)
                        {
                            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                            newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());
                            //newWorksheetPart.Worksheet.Save();
                            DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                            string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);
                            // Get a unique ID for the new sheet.
                            //uint sheetId = 1;
                            string sheetName = "Sheet" + sheetId;
                            // Append the new worksheet and associate it with the workbook.
                            Sheet sheet = new Sheet();
                            sheet.Id = relationshipId;
                            sheet.SheetId = sheetId;
                            sheet.Name = sheetName;
                            sheets.Append(sheet);
                            //这个保存很重要
                            newWorksheetPart.Worksheet.Save();
                            dataCurrentSheet++;
                        }

                        workbookPart.Workbook.Save();
                        doc.Dispose();
                    }
                    Open(excelPath);//外面不open只能这里open了

                }
                catch (System.UnauthorizedAccessException e)
                {
                    mIsCreateFailed = true;
                    log.Error(" Create Excel Error.UnauthorizedAccessException:" + e.ToString());
                    throw new Exception("NoAuthority");
                }
                catch (Exception e)
                {
                    mIsCreateFailed = true;
                    log.Error(" Create Excel Error.Exception:" + e.ToString());
                    throw e;
                }
#if PerformanceLog
            }
#endif
        }
        public void CreateSheet()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.CreateSheet"))
            {
#endif
                dataCurrentSheet++;
                WorkbookPart workbookPart = this.mDoc.WorkbookPart;
                WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());
                //newWorksheetPart.Worksheet.Save();
                DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);
                // Get a unique ID for the new sheet.
                //uint sheetId = 1;
                string sheetName = "Sheet" + dataCurrentSheet;
                // Append the new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet();
                sheet.Id = relationshipId;
                sheet.SheetId = (uint)dataCurrentSheet;
                sheet.Name = sheetName;
                sheets.Append(sheet);
                //这个保存很重要
                newWorksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
                this.mSheetIdList.Add(relationshipId);
#if PerformanceLog
            }
#endif
        }
        private List<String> GetSheets()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.GetSheets"))
            {
#endif
                // Fill this collection with a list of all the sheets.
                List<String> sheets = new List<String>();
                try
                {
                    WorkbookPart workbookPart = mCurrentWorkbooPart;
                    Stream workbookstr = workbookPart.GetStream();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(workbookstr);
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
                    nsManager.AddNamespace("default", doc.DocumentElement.NamespaceURI);
                    XmlNodeList nodelist = doc.SelectNodes("//default:sheets/default:sheet", nsManager);
                    foreach (XmlNode node in nodelist)
                    {
                        String sheetName = String.Empty;
                        sheetName = node.Attributes["name"].Value;
                        sheets.Add(sheetName);
                    }
                    workbookstr.Close();
                }
                catch (Exception e)
                {
                    log.Error("Get sheets error. Exception:" + e.ToString());
                }
                return sheets;
#if PerformanceLog
            }
#endif
        }
        public void Open(string excelPath)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.Open"))
            {
#endif
                try
                {
                    if (!File.Exists(excelPath))
                    {
                        CreateExcel(excelPath);
                    }
                    else
                    {
                        if (mSheetIdList == null)
                        {
                            mSheetIdList = new List<string>();
                        }
                        if (mSheetNameList == null)
                        {
                            mSheetNameList = new List<string>();
                        }
                        //mLog.Log(AveLogSeverity.Debug, "FileM2010Reader00140", DateTime.Now.ToString());
                        mDoc = SpreadsheetDocument.Open(excelPath, true);
                        mCurrentWorkbooPart = mDoc.WorkbookPart;
                        mSharedStringTablePart = mCurrentWorkbooPart.SharedStringTablePart;
                        //获得当前所有的 选项卡
                        List<string> namesList = GetSheets();
                        if (namesList.Count == 0)
                        {
                            ;//当没有找到 sheet 的时候应该处理
                        }
                        IEnumerable<Sheet> sheets = mCurrentWorkbooPart.Workbook.Descendants<Sheet>();
                        mCurrentWorksheetPart = (WorksheetPart)mCurrentWorkbooPart.GetPartById(string.Empty);
                        foreach (Sheet s in sheets)
                        {
                            if (namesList[0].Equals(s.Name))
                            {
                                mCurrentWorksheetPart = (WorksheetPart)mCurrentWorkbooPart.GetPartById(s.Id);
                                break;
                            }
                        }
                        mCurrentWorksheet = mCurrentWorksheetPart.Worksheet;
                        GetAllSheetId();
                        mExcelPath = excelPath;
                        //mLog.Log(AveLogSeverity.Debug, "FileM2010Reader00141", DateTime.Now.ToString());
                    }
                }
                catch (Exception e)
                {
                    log.Error("Open Excel Error.Exception:" + e.ToString());
                }
#if PerformanceLog
            }
#endif
        }
        private static String GetValue(Cell cell, SharedStringTablePart stringTablePart)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.GetValue"))
            {
#endif
                String value = null;
                DateTime dt = new DateTime();
                try
                {
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
                            try
                            {
                                dt = DateTime.FromOADate(double.Parse(value));
                                value = dt.ToString();
                            }
                            catch(Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperMappingResource.AWMConverValueToDateTimeError, e.ToString());
                                //value = "wring: " + value;
                                value = cell.CellValue.InnerText;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(" Get Value Error.Exception:" + e.ToString());
                }
                return value;
#if PerformanceLog
            }
#endif
        }

        private List<string> GetAllSheetId() //获得当前所有 符合 filter的 选项卡
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.GetAllSheetId"))
            {
#endif
                try
                {
                    List<string> namesList = GetSheets();
                    bool meetFilter = true;
                    mSheetIdList.Clear();
                    mSheetNameList.Clear();
                    if (namesList.Count == 0)
                    {
                        ;//当没有找到 sheet 的时候应该处理
                    }
                    IEnumerable<Sheet> sheets = mCurrentWorkbooPart.Workbook.Descendants<Sheet>();
                    foreach (string name in namesList)
                    {
                        foreach (Sheet s in sheets)
                        {
                            if (name.Equals(s.Name) && !mSheetIdList.Contains(s.Id))
                            {
                                mSheetIdList.Add(s.Id);
                                mSheetNameList.Add(name);
                            }
                        }
                    }
                    if (mSheetIdList.Count == 0 || mSheetIdList == null)
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    log.Error(" Get AllSheetId Error.Exception:" + e.ToString());
                }
                return mSheetIdList;
#if PerformanceLog
            }
#endif
        }

        public Dictionary<string, string> ReadLine()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.ReadLine"))
            {
#endif
                Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                int maxLine = 0;

                //找到第一个选项卡
                try
                {
                    if (mCurrentSheetCardRowsTotal < 0)
                    {
                        mRelId = mSheetIdList[0];
                        mCurrentWorksheet = ((WorksheetPart)mCurrentWorkbooPart.GetPartById(mRelId)).Worksheet;
                        mReadLineRows = mCurrentWorksheet.Descendants<Row>();
                        mExcelCellsDic.Clear();
                        maxLine = 0;
                        mCurrentSheetCardRowsTotal = 0;
                        foreach (Row w in mReadLineRows)
                        {
                            string key = w.RowIndex.ToString();
                            if (!mExcelCellsDic.ContainsKey(key))
                            {
                                mExcelCellsDic[key] = new List<Cell>();
                            }
                            mExcelCellsDic[key].AddRange(w.Cast<Cell>());
                            if (maxLine < w.RowIndex)
                            {
                                maxLine = Int32.Parse(key);
                            }
                        }
                        mCurrentSheetCardRowsTotal = maxLine;
                        mCurrentRowIndexRead = 2;
                    }

                }
                catch (Exception e)
                {
                    log.Error(" Read First Line Error.Exception:" + e.ToString());
                }

                // 遍历其他选项卡
                try
                {
                    if (mCurrentRowIndexRead > mCurrentSheetCardRowsTotal && (mSheetIdListPosition + 2) <= mSheetIdList.Count)  //当前选项卡已经遍历完了,开始遍历下一个选项卡
                    {
                        mSheetIdListPosition = mSheetIdList.IndexOf(mRelId);
                        mRelId = mSheetIdList[++mSheetIdListPosition];
                        mCurrentWorksheet = ((WorksheetPart)mCurrentWorkbooPart.GetPartById(mRelId)).Worksheet;

                        maxLine = 0;
                        mReadLineRows = mCurrentWorksheet.Descendants<Row>();
                        mExcelCellsDic.Clear();
                        mCurrentSheetCardRowsTotal = 0;
                        foreach (Row w in mReadLineRows)
                        {
                            string key = w.RowIndex.ToString();
                            if (!mExcelCellsDic.ContainsKey(key))
                            {
                                mExcelCellsDic[key] = new List<Cell>();
                            }
                            mExcelCellsDic[key].AddRange(w.Cast<Cell>());
                            if (maxLine < w.RowIndex)
                            {
                                maxLine = Int32.Parse(key);
                            }
                        }
                        mCurrentSheetCardRowsTotal = maxLine;
                        mCurrentRowIndexRead = 2;
                    }
                }
                catch (Exception e)
                {
                    log.Error(" Read Other Line Error.Exception:" + e.ToString());
                }
                if (mCurrentSheetCardRowsTotal >1 && mCurrentRowIndexRead > mCurrentSheetCardRowsTotal && (mSheetIdListPosition + 2) > mSheetIdList.Count)  //遍历完了所有的选项卡
                {
                    return null;
                }

                //对当前选项卡操作
                try
                {
                    string tempStr = string.Empty;
                    Dictionary<string, string> fristLine = new Dictionary<string, string>();
                    Dictionary<string, string> nowLine = new Dictionary<string, string>();
                    Regex reg = new Regex(@"\d");
                    string tempCol = string.Empty;
                    dic.Clear();
                    mInsensitiveTableHeader.Clear();
                    //读第一行
                    if (mExcelCellsDic.ContainsKey("1"))
                    {
                        foreach (Cell cell in mExcelCellsDic["1"]) 
                        {
                            //往mInsenstiveTableHeader里面添加表头
                            tempStr = GetValue(cell, mSharedStringTablePart);
                            if (!string.IsNullOrEmpty(tempStr.Trim()))
                            {
                                if (!mInsensitiveTableHeader.ContainsKey(tempStr.ToLower()))
                                {
                                    mInsensitiveTableHeader.Add(tempStr.ToLower(), tempStr);
                                }
                                tempCol = reg.Replace(cell.CellReference.ToString(), "");
                                fristLine.Add(tempCol, tempStr.Trim());
                            }
                        }
                        
                    }


                    if (mExcelCellsDic.Count >= mCurrentRowIndexRead && mExcelCellsDic.ContainsKey(mCurrentRowIndexRead.ToString()))
                    {
                        foreach (Cell cell in mExcelCellsDic[mCurrentRowIndexRead.ToString()])
                        {
                            tempStr = GetValue(cell, mSharedStringTablePart);
                            tempCol = reg.Replace(cell.CellReference.ToString(), "");
                            nowLine.Add(tempCol, tempStr.Trim());
                        }
                    }
                          
                    //遍历第一行
                    string tempVal = "";
                    foreach (KeyValuePair<string, string> k in fristLine)
                    {
                        if (nowLine.ContainsKey(k.Key))//如果对应的位置有值 那么加入
                        {
                            if (nowLine.TryGetValue(k.Key, out tempVal))
                            {
                                dic[k.Value] = tempVal;
                            }
                            else
                            {
                                //报错
                            }
                        }
                        else //如果对应位置没有值，那么加入 空字符串
                        {
                            dic[k.Value] = "";
                        }
                    }
                    mCurrentRowIndexRead++;
                }
                catch (Exception e)
                {
                    log.Error(" Read Current Sheet Error.Exception:" + e.ToString());
                    mCurrentRowIndexRead++;
                }
                return dic;
#if PerformanceLog
            }
#endif
        }

        public void closeActiveFile()
        {
            closeActiveFile(false);
        }

        public void closeActiveFile(bool isSaveFile)
        {
            if (mCurrentTableHeader.Count > 0)
            {
                mCurrentTableHeader.Clear();
            }
            mHeader.Clear();
            mCurrentRowIndexWrite = 0;
            if (isSaveFile)
            {
                Save();
            }
        }

        private string GetColStr(int position) //从0开始计数/////////////////////////
        {
            string name = "";
            char[] columnNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            int num = position;
            do
            {
                int i = num % 26;
                name = columnNames[i] + name;
                num = num / 26 - 1;
            } while (num > -1);
            if (string.IsNullOrEmpty(name))
            {
                name = "A";
            }
            return name;
        }


        #region new function
        string mTempSheetId = string.Empty;
        DataTable dt = new DataTable();
        int mCurrentRow = 1;
        SharedStringTablePart mShareStringPart = null;
        int mSharedStringTableCount = -1;
        public void WriteLine(Dictionary<string, string> values)
        {
            mFileCount++;
            bool needToAddHead = false;
            if (mIsCreateFailed)
            {
                mIsCreateFailed = false;
                throw new Exception("Create excel file failed.");
            }
            //string sheetId = mSheetIdList[dataCurrentSheet - 1];
            if (!mSheetIdList[dataCurrentSheet - 1].Equals(mTempSheetId, StringComparison.OrdinalIgnoreCase))
            {
                if (mCurrentWorksheetPart != null)
                {
                    mCurrentWorksheetPart.Worksheet.Save();
                }
                mCurrentWorksheetPart = (WorksheetPart)mDoc.WorkbookPart.GetPartById(mSheetIdList[dataCurrentSheet - 1]);
                mTempSheetId = mSheetIdList[dataCurrentSheet - 1];
                mCurrentRow = 1;
                needToAddHead = true;
            }

            if (mShareStringPart == null)
            {
                IEnumerable<SharedStringTablePart> IEnumShareStringPart = mDoc.WorkbookPart.GetPartsOfType<SharedStringTablePart>();
                //foreach (SharedStringTablePart sharePointPart in IEnumShareStringPart)
                //{
                //    mShareStringPart = sharePointPart;
                //    break;
                //}
                mShareStringPart = IEnumShareStringPart.FirstOrDefault();
            }
            FillInDataTable(values, needToAddHead);
            WriteDataIntoWorkSheet(mCurrentRow, 0, dt);
            mCurrentRow++;

        }

        private void FillInDataTable(Dictionary<string, string> values, bool needToAddHead)
        {
            DataRow row = dt.NewRow();
            dt.Rows.Clear();
            int i = 0;
            foreach (KeyValuePair<string, string> keyValue in values)
            {
                if (!string.IsNullOrEmpty(keyValue.Key.Trim()))
                {
                    if (!dt.Columns.Contains(keyValue.Key))
                    {
                        dt.Columns.Add(keyValue.Key);
                        WriteData(0, dt.Columns.Count - 1, keyValue.Key);
                    }
                    else
                    {
                        if (needToAddHead)
                        {
                            WriteData(0, i, keyValue.Key);
                        }
                    }
                    row[keyValue.Key] = keyValue.Value;
                    i++;
                }
            }
            dt.Rows.Add(row);
        }

        public void WriteData(int x, int y, string strContent)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Column1");
            dt.Rows.Add(strContent);
            WriteDataIntoWorkSheet(x, y, dt);
        }

        public void WriteDataIntoWorkSheet(int startx, int starty, DataTable dt)
        {
            //if (startx < 1)
            //    startx = 1;
            //if (starty < 1)
            //    starty = 1;
            WorksheetPart worksheetPart = mCurrentWorksheetPart;
            //starty -= 1;
            int j = 0;
            foreach (DataRow dr in dt.Rows)
            {
                j++;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string name = GetColStr(i + starty);
                    string text = Convert.IsDBNull(dr[i]) ? "" : dr[i].ToString();
                    int index = InsertSharedStringItem(text, mShareStringPart);
                    Cell cell = InsertCellInWorksheet(name, Convert.ToUInt32(j + startx), worksheetPart);
                    cell.CellValue = new CellValue(index.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    //worksheetPart.Worksheet.Save();
                }
            }
        }

        private int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
                shareStringPart.SharedStringTable.Count = 1;
                shareStringPart.SharedStringTable.UniqueCount = 1;
            }
            //int i = shareStringPart.SharedStringTable.Elements<SharedStringItem>().Count();

            mSharedStringTableCount++;

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            //shareStringPart.SharedStringTable.Save();
            return mSharedStringTableCount;
        }

        private Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;
            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row = null;
            //foreach (Row tempRow in sheetData.Elements<Row>())
            //{
            //    if (tempRow.RowIndex.Equals(rowIndex))
            //    {
            //        row = tempRow;
            //        break;
            //    }
            //}
            //if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            //{
            //    row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            //}
            //else
            if (null == row)
            {
                row = new Row() { RowIndex = rowIndex };
                Row refRow = null;
                //foreach (Row temoRow in sheetData.Elements<Row>())
                //{
                //    if (temoRow.RowIndex > rowIndex)
                //    {
                //        refRow = temoRow;
                //        break;
                //    }
                //}
                sheetData.InsertBefore<Row>(row, refRow);
            }
            // If there is not a cell with the specified column name, insert one.  
            //foreach (Cell tempCell in row.Elements<Cell>())
            //{
            //    if (tempCell.CellReference.Value.Equals(cellReference, StringComparison.OrdinalIgnoreCase))
            //    {
            //        return tempCell;
            //    }
            //}
            //if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            //{
            //    return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            //}
            //else
            //{
            // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
            Cell refCell = null;
            //foreach (Cell cell in row.Elements<Cell>())
            //{
            //    if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
            //    {
            //        refCell = cell;
            //        break;
            //    }
            //}
            Cell newCell = new Cell() { CellReference = cellReference };
            row.InsertBefore(newCell, refCell);
            //worksheet.Save();
            return newCell;
            //}
        }

        public void Save()
        {
            try
            {
                if (mShareStringPart != null)//不为null，说明是走write方法，需要save
                {
                    mShareStringPart.SharedStringTable.Save();
                    mCurrentWorksheetPart.Worksheet.Save();
                }
            }
            catch(Exception e) 
            {
                log.Log(AveLogLevel.DEBUG, WrapperMappingResource.AWMSaveExcelOrSheetError, e.ToString());
            }
        }

        #endregion

        public void WriteHeader(Dictionary<string, string> values)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.WriteHeader"))
            {
#endif
            bool needToAddHead = false;
            if (mIsCreateFailed)
            {
                mIsCreateFailed = false;
                throw new Exception("Create excel file failed.");
            }
            //string sheetId = mSheetIdList[(int)Math.Ceiling((double)mFileCount / CommonFunc.ROWSCOUNT) - 1];
            if (!mSheetIdList[dataCurrentSheet - 1].Equals(mTempSheetId, StringComparison.OrdinalIgnoreCase))
            {
                if (mCurrentWorksheetPart != null)
                {
                    mCurrentWorksheetPart.Worksheet.Save();
                }
                mCurrentWorksheetPart = (WorksheetPart)mDoc.WorkbookPart.GetPartById(mSheetIdList[dataCurrentSheet - 1]);
                mTempSheetId = mSheetIdList[dataCurrentSheet - 1];
                mCurrentRow = 1;
                needToAddHead = true;
            }

            if (mShareStringPart == null)
            {
                IEnumerable<SharedStringTablePart> IEnumShareStringPart = mDoc.WorkbookPart.GetPartsOfType<SharedStringTablePart>();
                //foreach (SharedStringTablePart sharePointPart in IEnumShareStringPart)
                //{
                //    mShareStringPart = sharePointPart;
                //    break;
                //}
                mShareStringPart = IEnumShareStringPart.FirstOrDefault();
            }
            FillInDataTableForHeader(values, needToAddHead);
            WriteDataIntoWorkSheet(mCurrentRow, 0, dt);
#if PerformanceLog
            }
#endif
        }

        private void FillInDataTableForHeader(Dictionary<string, string> values, bool needToAddHead)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.FMExcelOpenXml.FillInDatatableForheader"))
            {
#endif
                int i = 0;
                foreach (KeyValuePair<string, string> keyValue in values)
                {
                    if (!string.IsNullOrEmpty(keyValue.Key.Trim()))
                    {
                        if (!dt.Columns.Contains(keyValue.Key))
                        {
                            dt.Columns.Add(keyValue.Key);
                            WriteData(0, dt.Columns.Count - 1, keyValue.Key);
                        }
                        else
                        {
                            if (needToAddHead)
                            {
                                WriteData(0, i, keyValue.Key);
                            }
                        }
                        i++;
                    }
                }
#if PerformanceLog
            }
#endif
        }
    }
}
