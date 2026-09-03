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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Level = AvePoint.RA.SharePoint.Common.Setting.Model.SettingLevel;

namespace AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting.Helper
{
    public class SettingCsv
    {
        private const string ManuallyChooseATerm = "manually choose a term";
        private const string SetADefaultTerm = "set a default term";
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ImportSettingHelper));
        public (List<ImportTeamsSettingData>, List<JMImportSPSettingDetail>, string) ReadCsv(string path)
        {
            List<ImportTeamsSettingData> datas = [];
            List<JMImportSPSettingDetail> datasFail = [];
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (path.EndsWith("csv"))
                    {
                        byte[] header = new byte[2];
                        int bytesRead = fs.Read(header, 0, header.Length);
                        fs.Seek(0, SeekOrigin.Begin);
                        if (bytesRead == header.Length && header[0] == 0x50 && header[1] == 0x4B)
                        {
                            var illegalCharactersErrorMessage = I18NEntity.GetString("RM_JS_BCM_ImportSetting_InvalidFileFormat");
                            return (datas, datasFail, illegalCharactersErrorMessage);
                        }
                        var importSettingHelper = new ImportSettingHelper();
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            int count = 0;
                            while (!sr.EndOfStream)
                            {
                                JMImportSPSettingDetail detail = new JMImportSPSettingDetail();
                                try
                                {
                                    string csvLine = sr.ReadLine();
                                    if (csvLine != null)
                                    {
                                        count++;
                                        if (csvLine.StartsWith("\"=\""))
                                        {
                                            string[] cols = csvLine.Substring(3).Split("\"\",\"=\"");

                                            csvLine = string.Join(",", cols);
                                        }
                                        string[] currentRow = CSVHelper.AnalyseCSVRow2Array(csvLine);

                                        foreach (var contentCell in currentRow)
                                        {
                                            if (contentCell.Contains("\t"))
                                            {
                                                var illegalCharactersErrorMessage = string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_IllegalCharacters"), count, contentCell.Replace("\t", "\\t"));
                                                return (datas, datasFail, illegalCharactersErrorMessage);
                                            }
                                        }

                                        if (currentRow.Length >= 17 && currentRow[16].EndsWith("\""))
                                        {
                                            currentRow[16] = currentRow[16].Substring(0, currentRow[16].Length - 1);
                                        }

                                        if (count == 1 || string.IsNullOrEmpty(currentRow[0]))
                                        {
                                            _logger.Info("Skip header and empty row.");
                                            continue;
                                        }
                                        else if ((currentRow.Length >= 17 && bool.TryParse(currentRow[16], out var isInherit) && isInherit))
                                        {
                                            var settingObj = importSettingHelper.ConvertPathObject(currentRow, detail);
                                            detail.ObjectName = settingObj.SettingLevel == Level.Container ? settingObj.ContainerName : settingObj.FullUrl.Substring(settingObj.FullUrl.LastIndexOf('/') + 1);
                                            detail.Url = settingObj.FullUrl;
                                            _logger.Info("Skip inherit row.");
                                            throw new Exception("RM_JS_BCM_ImportSetting_SkipInherit");
                                        }
                                        else if (currentRow.Length == 6 || (!SetADefaultTerm.EqualsIgnoreCase(currentRow[6]) && !ManuallyChooseATerm.EqualsIgnoreCase(currentRow[6])))
                                        {
                                            var settingObj = importSettingHelper.ConvertPathObject(currentRow, detail);
                                            detail.ObjectName = settingObj.SettingLevel == Level.Container ? settingObj.ContainerName : settingObj.FullUrl.Substring(settingObj.FullUrl.LastIndexOf('/') + 1);
                                            detail.Url = settingObj.FullUrl;
                                            _logger.Info("The option deploy term method is not deploy default term or manual");
                                            throw new Exception("RM_JS_BCM_ImportSetting_SkipDoesNotMethod");
                                        }
                                        else
                                        {
                                            datas.Add(importSettingHelper.ConvertToSettingObject(currentRow, detail));
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    detail.Comment = e.Message;
                                    detail.Status = JobDetailsStatus.Failed;
                                    if (e.Message.Equals("RM_JS_BCM_ImportSetting_SkipDoesNotMethod") || e.Message.Equals("RM_JS_BCM_ImportSetting_SkipInherit"))
                                        detail.Status = JobDetailsStatus.Skipped;
                                    _logger.Error($"Convert csv line to object error:{e.ToString()}");
                                    datasFail.Add(detail);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw new Exception("Failed to read file conntent");
            }
            return (datas, datasFail, string.Empty);
        }
    }
}
