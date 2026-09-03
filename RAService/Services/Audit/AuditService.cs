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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.RMReport.AuditHandler;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.Service.Services.Audit
{
    [Audit]
    public class AuditService : RMServiceBase, IAuditService
    {
        private IAuditDao AuditDao => PlatformWindsorManager.GetService<IAuditDao>();
        private RALogger logger = RALogger.GetInstance(typeof(AuditService));
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public bool AddAudits(List<RMAuditInfo> auditInfos)
        {
            return AuditDao.BatchCreate(auditInfos.Select(a => RMSecurityUtil.ConvertToDBAudit(a)).ToList()) >= auditInfos.Count;
        }

        public List<RMAuditInfo> FindAuditInfoByTimeInterval(int pageIndex, int pageSize, ref int dataCount, DateTime startTime, DateTime endTime, DisplayColumn columnName, string columnValue)
        {
            List<RMAuditInfo> result = new List<RMAuditInfo>();
            List<RMAudit> searchResult = null;
            switch (columnName)
            {
                case DisplayColumn.Time:
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks);
                    break;
                case DisplayColumn.User:
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks && columnValue.Equals(item.UserName, StringComparison.InvariantCulture));
                    break;
                case DisplayColumn.Role:
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks && columnValue.Equals(item.Role, StringComparison.InvariantCulture));
                    break;
                case DisplayColumn.DocAveModule:
                    int module = Int32.Parse(columnValue);
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks && module == item.Module);
                    break;
                case DisplayColumn.Object:
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks && columnValue.Equals(item.Object, StringComparison.InvariantCulture));
                    break;
                case DisplayColumn.Action:
                    int action = Int32.Parse(columnValue);
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks && item.Action == action);
                    break;
                case DisplayColumn.Status:
                    int status = Int32.Parse(columnValue);
                    searchResult = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks && item.Status == status);
                    break;
            }
            if (searchResult != null && searchResult.Count > 0)
            {
                searchResult.ForEach(item => result.Add(RMSecurityUtil.ConvertToAuditDto(item)));
            }
            return result;
        }

        //by sort filter auditinfos
        public List<RMAuditInfo> FindAuditInfoBySortFilter(int pageIndex, int pageSize, ref int dataCount, DateTime startTime, DateTime endTime, bool? isAscending, DisplayColumn sortBy, Dictionary<int, List<dynamic>> filterInfos, DisplayColumn viewBy, string ViewByValue)
        {
            List<RMAuditInfo> result = new List<RMAuditInfo>();
            List<RMAudit> searchResult = null;
            Expression<Func<RMAudit, bool>> whereLamdba = null;
            List<int> actionItems = new List<int>();
            List<int> moduleItems = new List<int>();
            List<int> statusItems = new List<int>();
            List<string> userNames = new List<string>();

            //view by
            int viewBycode = -1000;
            bool hasViewBy = false;
            bool viewByModule = false;
            bool viewByAction = false;
            bool viewByStatus = false;
            bool viewByUser = false;
            switch (viewBy)
            {
                case DisplayColumn.User:
                    viewByUser = true;
                    if(ViewByValue == I18NEntity.GetString("RM_TS_RunSchedule"))
                    {
                        ViewByValue = "RM_TS_RunSchedule";
                    }
                    break;
                case DisplayColumn.DocAveModule:
                    viewByModule = true;
                    break;
                case DisplayColumn.Action:
                    viewByAction = true;
                    break;
                case DisplayColumn.Status:
                    viewByStatus = true;
                    break;
            }
            if (!string.IsNullOrEmpty(ViewByValue) && !viewBy.Equals(DisplayColumn.Time))
            {
                hasViewBy = true;
                if (!viewBy.Equals(DisplayColumn.User))
                {
                    viewBycode = Convert.ToInt32(ViewByValue);
                }
            }
            //filter condition
            if (filterInfos != null && filterInfos.Count > 0)
            {
                if (filterInfos.ContainsKey((int)DisplayColumn.Action))
                {
                    actionItems = filterInfos[(int)DisplayColumn.Action].ConvertAll(item => (int)item);
                }
                if (filterInfos.ContainsKey((int)DisplayColumn.DocAveModule))
                {
                    moduleItems = filterInfos[(int)DisplayColumn.DocAveModule].ConvertAll(item => (int)item);
                }
                if (filterInfos.ContainsKey((int)DisplayColumn.Status))
                {
                    statusItems = filterInfos[(int)DisplayColumn.Status].ConvertAll(item => (int)item);
                }
                if (filterInfos.ContainsKey((int)DisplayColumn.User))
                {
                    var userNamesInFilter = filterInfos[(int)DisplayColumn.User].ConvertAll(item => (string)item);
                    userNames = GetUserNamesForSearch(userNamesInFilter);
                }

                if (hasViewBy)
                {
                    whereLamdba =
                    (item =>
                        (viewByModule ? (moduleItems.Count > 0 ? moduleItems.Contains(viewBycode) && item.Category.Equals(viewBycode) : item.Category.Equals(viewBycode)) : (moduleItems.Count > 0 ? moduleItems.Contains(item.Category) : true)) &&
                        (viewByAction ? (actionItems.Count > 0 ? actionItems.Contains(viewBycode) && item.Action.Equals(viewBycode) : item.Action.Equals(viewBycode)) : (actionItems.Count > 0 ? actionItems.Contains(item.Action) : true)) &&
                        (viewByStatus ? (statusItems.Count > 0 ? statusItems.Contains(viewBycode) && item.Status.Equals(viewBycode) : item.Status.Equals(viewBycode)) : (statusItems.Count > 0 ? statusItems.Contains(item.Status) : true)) &&
                        (viewByUser ? (userNames.Count > 0 ? userNames.Contains(ViewByValue) && item.UserName.Equals(ViewByValue) : item.UserName.Equals(ViewByValue)) : (userNames.Count > 0 ? userNames.Contains(item.UserName) : true)) &&
                         (item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                       );
                }
                else
                {
                    whereLamdba =
                   (item =>
                        (moduleItems.Count > 0 ? moduleItems.Contains(item.Category) : true) &&
                        (actionItems.Count > 0 ? actionItems.Contains(item.Action) : true) &&
                        (statusItems.Count > 0 ? statusItems.Contains(item.Status) : true) &&
                        (userNames.Count > 0 ? userNames.Contains(item.UserName) : true) &&
                        (item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks)
                      );
                }

            }
            else
            {
                switch (viewBy)
                {
                    case DisplayColumn.User:
                        whereLamdba = (item => item.UserName.Equals(ViewByValue) && item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks);
                        break;
                    case DisplayColumn.DocAveModule:
                        whereLamdba = (item => item.Category.Equals(viewBycode) && item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks);
                        break;
                    case DisplayColumn.Action:
                        whereLamdba = (item => item.Action.Equals(viewBycode) && item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks);
                        break;
                    case DisplayColumn.Status:
                        whereLamdba = (item => item.Status.Equals(viewBycode) && item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks);
                        break;
                    default:
                        whereLamdba = (item => item.ExecuteOn >= startTime.Ticks && item.ExecuteOn <= endTime.Ticks);
                        break;
                }

            }
            searchResult = AuditDao.FindAuditInfoByFilterAndSort(pageIndex, pageSize, ref dataCount, whereLamdba, sortBy, isAscending);

            if (searchResult != null && searchResult.Count > 0)
            {
                searchResult.ForEach(item => result.Add(RMSecurityUtil.ConvertToAuditDto(item)));
            }
            return result;
        }

        private List<string> GetUserNamesForSearch(List<string> filterUserNames)
        {
            var systemUserNames = GetRecordsSystemUserNames();
            if (filterUserNames.Any(o => systemUserNames.Contains(o)))
            {
                filterUserNames = filterUserNames.Except(systemUserNames).ToList();
                filterUserNames = filterUserNames.Concat(systemUserNames).ToList();
            }
            return filterUserNames;
        }

        public async Task<Dictionary<DateTime, int>> FindAuditInfoByTimeIntervalAndGroupByTimeAsync(DateTime startTime, DateTime endTime)
        {
            List<DateTime> distinctDateTime = new List<DateTime>();
            Dictionary<DateTime, int> result = new Dictionary<DateTime, int>();
            Dictionary<string, int> templeResult = new Dictionary<string, int>();
            List<long> searchReulst = AuditDao.FindAuditInfoByTimeIntervalAndGroupByTime(startTime, endTime);
            if (searchReulst == null) { return result; }
            var globalTimeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            foreach (var dateTimeTicks in searchReulst)
            {
                DateTime temple = new DateTime(dateTimeTicks);
                //该处是要取时间的年月日，计算总和，显示柱状图，此处需要以Local时间进行判断。
                TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
                var utcDateTime = DateTime.SpecifyKind(temple, DateTimeKind.Utc);
                temple = temple + cstZone.GetUtcOffset(utcDateTime);

                if (templeResult.ContainsKey(temple.ToString("yyyy.MM.dd")))
                {
                    templeResult[temple.ToString("yyyy.MM.dd")]++;
                }
                else
                {
                    templeResult[temple.ToString("yyyy.MM.dd")] = 1;
                    distinctDateTime.Add(temple);
                }
            }
            foreach (DateTime day in distinctDateTime)
            {
                if (templeResult.ContainsKey(day.ToString("yyyy.MM.dd")))
                {
                    result[day] = templeResult[day.ToString("yyyy.MM.dd")];
                }
            }
            return result;
        }

        public Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByUser(DateTime startTime, DateTime endTime)
        {
            return AuditDao.FindAuditInfoByTimeIntervalAndGroupByUser(startTime, endTime);
        }
        public Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByRole(DateTime startTime, DateTime endTime)
        {
            return AuditDao.FindAuditInfoByTimeIntervalAndGroupByRole(startTime, endTime);
        }
        public Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByModule(DateTime startTime, DateTime endTime)
        {
            return AuditDao.FindAuditInfoByTimeIntervalAndGroupByModule(startTime, endTime);
        }
        public Dictionary<string, int> FindAuditInfoByTimeIntervalAndGroupByObject(DateTime startTime, DateTime endTime)
        {
            return AuditDao.FindAuditInfoByTimeIntervalAndGroupByObject(startTime, endTime);
        }
        public Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByAction(DateTime startTime, DateTime endTime)
        {
            return AuditDao.FindAuditInfoByTimeIntervalAndGroupByAction(startTime, endTime);
        }

        public Dictionary<int, int> FindAuditInfoByTimeIntervalAndGroupByStatus(DateTime startTime, DateTime endTime)
        {
            return AuditDao.FindAuditInfoByTimeIntervalAndGroupByStatus(startTime, endTime);
        }

        /// <summary>
        /// download report用
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.AuditorReport, Action = AuditAction.ExportAuditorReport, AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<RAReturnMessage> GenerateReportForAuditReportAsync(string folderPath, string fileName, DateTime start, DateTime end)
        {
            RAReturnMessage rMessage = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful
            };
            string[][] datas = null;
            int maxCountOfOneSheet = 65535;
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + ".xlsx");
            //List<RMAudit> allAudits = AuditDao.FindAllAuditInfos();
            List<RMAudit> allAudits = new List<RMAudit>();
            int dataCount = 0;
            int pageIndex = 0;
            int pageSize = 1000;
            while (true)
            {
                pageIndex++;
                var auditisData = AuditDao.FindAuditInfoByTimeInterval(pageIndex, pageSize, ref dataCount, item => item.ExecuteOn >= start.Ticks && item.ExecuteOn <= end.Ticks);
                if (auditisData == null || auditisData.Count == 0)
                {
                    break;
                }
                allAudits.AddRange(auditisData);
            }

            List<RMAudit> templeAuditInfos = new List<RMAudit>();
            int jobReportTotalCount = allAudits.Count();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            try
            {
                if (jobReportTotalCount > 0)
                {
                    for (int i = 1; i < allAudits.Count() + 1; i++)
                    {
                        if (templeAuditInfos.Count != 0 && (templeAuditInfos.Count + 1) % maxCountOfOneSheet == 0)
                        {
                            templeAuditInfos.Add(allAudits[i - 1]);
                            templeAuditInfos = await InsertDataToExcelAsync(reportFilePath, templeAuditInfos, i, maxCountOfOneSheet);
                        }
                        else
                        {
                            templeAuditInfos.Add(allAudits[i - 1]);
                        }
                    }
                }
                else
                {
                    datas = new string[1][];
                    datas[0] = new string[] { I18NEntity.GetString("RM_RC_Audit_DownLoad_NoInformationInDB") };
                    ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                }
                if (templeAuditInfos.Count > 0)
                {
                    await InsertDataToExcelAsync(reportFilePath, templeAuditInfos, jobReportTotalCount, maxCountOfOneSheet);
                }
                //Quality Issue
                //GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
                //using (FileStream fileStrem = new FileStream(folderPath + ".zip",FileMode.Open))
                //{
                //    byte[] bytes = new byte[fileStrem.Length];
                //    fileStrem.ReadAsync(bytes, 0, (int)fileStrem.Length);
                //    resultStream = new MemoryStream(bytes);
                //}
                //Quality Issue
            }
            catch (Exception e)
            {
                rMessage.MessageType = RAMessageType.Failed;
                rMessage.ErrorMessage = e.Message;
                logger.Debug("generate Audit Report Erro Info:{0},{1}", e.Message, e.StackTrace);
            }
            return rMessage;
        }

        public async Task<List<RMAudit>> InsertDataToExcelAsync(string reportFilePath, List<RMAudit> ruleUsageInfos, int currentInsertCount, int maxCountOfOneSheet)
        {
            string[][] datas = new string[ruleUsageInfos.Count() + 1][];
            datas = AssembleAuditReportHeaderTittle(datas);
            datas = await ConvertAuditInfoToArrayAsync(ruleUsageInfos, datas);
            if (currentInsertCount <= maxCountOfOneSheet)
            {
                ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                ruleUsageInfos.Clear();
            }
            else
            {
                var floorValue = currentInsertCount / maxCountOfOneSheet;
                ReportUtil.InsertWorksheet(reportFilePath, "Sheet" + (currentInsertCount % maxCountOfOneSheet == 0 ? floorValue : floorValue + 1), datas);
                ruleUsageInfos.Clear();
            }
            return ruleUsageInfos;
        }

        private string ConvertXmlString(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }
            if (!IsValidXmlString(content))
            {
                return RemoveInvalidXmlChars(content);
            }
            else
            {
                return content;
            }
        }

        private string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

        private bool IsValidXmlString(string text)
        {
            try
            {
                XmlConvert.VerifyXmlChars(text);
                return true;
            }
            catch (Exception e)
            {
                logger.Info($"Find InvalidXmlChars, Exception {e}");
                return false;
            }
        }

        public async Task<string[][]> ConvertAuditInfoToArrayAsync(IEnumerable<RMAudit> reportDetails, string[][] datas)
        {

            RMAudit reportInfo = null;
            int rowCount = 1;
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (RMAudit report in reportDetails)
            {
                try
                {
                    reportInfo = report as RMAudit;
                    datas[rowCount] = new string[9];
                    datas[rowCount][0] = GeneralSettingService.ConvertTiksToDateTime(gls, reportInfo.ExecuteOn, true).SimplifyFormatTime;
                    datas[rowCount][1] = I18NEntity.GetString(reportInfo.UserName);
                    datas[rowCount][2] = ((AuditStatus)reportInfo.Status).ToDescription();
                    datas[rowCount][3] = ((AuditCategory)reportInfo.Category).ToDescription();
                    //datas[rowCount][4] = ((AuditCategory)reportInfo.Category).ToDescription();
                    datas[rowCount][4] = ((AuditAction)reportInfo.Action).ToDescription();
                    datas[rowCount][5] = ConvertXmlString(reportInfo.Object != null ? I18NEntity.GetString(reportInfo.Object.ToString()) : "");
                    datas[rowCount][6] = ConvertXmlString(this.ConvertAuditNewOrOldValueToString(reportInfo.Content, false, reportInfo.Action));
                    datas[rowCount][7] = ConvertXmlString(this.ConvertAuditNewOrOldValueToString(reportInfo.Content, true, reportInfo.Action));
                    datas[rowCount][8] = reportInfo.ClientIP;
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert audit info to Array failed,report action is: {reportInfo.Action},report content is:{reportInfo.Content},error is: {e}");
                }
            }
            return datas;
        }
        private string ConvertAuditNewOrOldValueToString(string xml, bool isNewValue, int Action)
        {
            var list = SerializerHelper.DeserializeFromXmlString<List<AuditItem>>(xml);
            StringBuilder str = new StringBuilder();
            int i = 0;
            foreach (AuditItem item in list)
            {
                StringBuilder tempTitle = new StringBuilder();
                StringBuilder tempValue = new StringBuilder();
                string val = isNewValue ? item.NewValue : item.OldValue;
                if (item.TargetSetting != null && item.TargetSetting != "")
                {
                    var targetSetting = item.TargetSetting.StartsWith("RM_") ? I18NEntity.GetString(item.TargetSetting) : item.TargetSetting;
                    tempTitle.Append(targetSetting).Replace(":", "").Append(":\n");
                }
                if (val != null && val != "")
                {
                    if (item.OldValue == "True" || item.OldValue == "False")
                    {
                        item.OldValue = item.OldValue == "True" ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                    }
                    if (item.NewValue == "True" || item.NewValue == "False")
                    {
                        item.NewValue = item.NewValue == "True" ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                    }
                    var newValue = "";
                    var oldValue = "";
                    if (isNewValue)
                    {
                        newValue = ModifyAuditContentByTargetSetting(I18NEntity.ReplaceI18NKey(item.NewValue, "RM_", new string[] { ";", ",", " " }), 
                            item.TargetSetting, (AuditAction)Action, true);
                    }
                    else
                    {
                        oldValue = ModifyAuditContentByTargetSetting(I18NEntity.ReplaceI18NKey(item.OldValue, "RM_", new string[] { ";", ",", " " }), 
                            item.TargetSetting, (AuditAction)Action);
                    }
                    tempValue.Append(isNewValue == true ? newValue : oldValue).Replace("<br>", "").Append("\n");
                }
                //term with out rule  for REC-828
                if (Action == 2004)
                {
                    str.Append(tempValue.ToString() == "" ? tempTitle.Append(I18NEntity.GetString("RM_JS_Rule_ObjectLevel_None")).Append("\n") : tempTitle.Append(tempValue));
                }
                else
                {
                    str.Append(tempValue.ToString() == "" ? tempValue : tempTitle.Append(tempValue));
                }
                // Configure Custom Setting  for REC-783
                if (Action == 2101 && isNewValue)
                {
                    i++;
                    if (i >= 2)
                    {
                        return str.ToString();
                    }
                }
                // hidden value for REC-942
                if (Action == 304 && item.NewValue == "True")
                {
                    str.Clear();
                }
            }
            return str.ToString();
        }

        // use to modify OriginalValue/NewValue of some audit item with complex value
        private string ModifyAuditContentByTargetSetting(string value, string targetSettingI18NKey, AuditAction auditAction, bool isNewValue = false)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(targetSettingI18NKey) || auditAction == AuditAction.Unknown)
            {
                return value;
            }
            var newValue = value;
            try
            {
                switch (auditAction)
                {
                    // oldValue and newValue is handled in the same way for StubSetting
                    case AuditAction.StubSettingCreate:
                    case AuditAction.StubSettingUpdate:
                        if (RMConstants.STUBCONTENT.Equals(targetSettingI18NKey))
                        {
                            //if (isNewValue)
                            newValue = LinkFileCommon.ReplaceStubTags(value, false);
                        }
                        else if (RMConstants.STUBRETENTIONPERIOD.Equals(targetSettingI18NKey))
                        {
                            newValue = value?.Trim();
                        }
                        break;
                    default:
                        break;
                }
                return newValue;
            }
            catch (Exception e)
            {
                logger.Error($"ModifyAuditContentByTargetSetting failed, value: {value}, targetSettingI18NKey: {targetSettingI18NKey}, auditAction: {auditAction}, isNewValue: {isNewValue}. Ex: {e}");
                return value;
            }
        }

        public string[][] AssembleAuditReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[9];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_Time");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_User");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_Status");
            datas[0][3] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_Module");
            //datas[0][4] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_Function");
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_Action");
            datas[0][5] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_Object");
            datas[0][6] = I18NEntity.GetString("RM_JS_RC_Audit_ManageCol_OldVal");
            datas[0][7] = I18NEntity.GetString("RM_JS_RC_Audit_ManageCol_NewVal");
            datas[0][8] = I18NEntity.GetString("RM_JS_RC_Audit_ViewBy_Option_ClientIP");
            return datas;
        }
        public Dictionary<int, string> GetActionItemsSource()
        {
            Dictionary<int, string> actionItems = new Dictionary<int, string>();
            var codes = AuditDao.GetAuditActionFromDB();
            foreach (var code in codes)
            {
                actionItems.Add((int)code, code.ToDescription());
            }
            return actionItems;
        }
        public Dictionary<int, string> GetModuleItemsSource()
        {
            Dictionary<int, string> moduleItems = new Dictionary<int, string>();
            var codes = AuditDao.GetAuditModuleFromDB();
            foreach (var code in codes)
            {
                moduleItems.Add((int)code, code.ToDescription());
            }
            return moduleItems;
        }
        public List<string> GetUserItemsSource()
        {
            var userNames = AuditDao.GetAuditUserFromDb();
            var systemUserNames = GetRecordsSystemUserNames();
            if (userNames.Any(o => systemUserNames.Contains(o)))
            {
                userNames = userNames.Except(systemUserNames).ToList();
                userNames.Add("RM_TS_RunSchedule");
            }
            return userNames;
        }

        public Dictionary<int, string> GetStatusItemsSource()
        {
            Dictionary<int, string> statusItems = new Dictionary<int, string>();
            foreach (AuditStatus code in Enum.GetValues(typeof(AuditStatus)))
            {
                statusItems.Add((int)code, code.ToDescription());
            }
            return statusItems;
        }

        public Task<int> DeleteAuditorBeforeTimeAsync(long ticks)
        {
            return AuditDao.BatchDeleteAsync((a) => a.ExecuteOn < ticks);
        }


        /// <summary>
        /// 包含RM_TS_RunSchedule，以及英语和日语国际化后的Value
        /// </summary>
        /// <returns></returns>
        private List<string> GetRecordsSystemUserNames()
        {
            return new List<string> {
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("en-US")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ja-JP")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ko-KR")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-FR")),
                I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-CA")),
                "RM_TS_RunSchedule"
            };
        }
    }
}
