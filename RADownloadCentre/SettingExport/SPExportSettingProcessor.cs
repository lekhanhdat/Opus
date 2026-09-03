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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenNLP.Tools.Util;
using RADownloadCenter;
using RADownloadCentre.SettingExport.Base;
using System.Text;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;
using NodeLevel = AvePoint.GCommon.Contract.Tree.Object.NodeLevel;

namespace RADownloadCentre.SettingExport
{
    public class SPExportSettingProcessor : ExportSettingProcessor<ExportSPSetting>
    {
        private RALogger Logger = RALogger.GetInstance(typeof(SPExportSettingProcessor));
        #region Service and DAO 
        private readonly ITermDao TermDAO = PlatformWindsorManager.GetService<ITermDao>();
        private readonly ISPSettingTreeService SPSettingTreeService = PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private readonly ISharePointSettingDao SharePointSettingDao = PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private readonly IBrowseTreeService BrowseTreeService = PlatformWindsorManager.GetService<IBrowseTreeService>();
        #endregion

        #region Setting info
        private List<RMSPSampleTreeNode> WebApplications = new List<RMSPSampleTreeNode>();
        #endregion
        private Dictionary<string, RemoteSiteCollection> mRemoteSCCache;
        private Dictionary<string, AveBPOSAccountInfo> mAveBposInfoCache;
        private Dictionary<string, IAveWeb> mAveWebCache;
        private Dictionary<string, IAveSite> mAveSiteCache;
        private Dictionary<string, AveObjectModelFactory> mFactoryCache;
        private readonly int PageSize = 1000;
        private readonly string StartFileName = "ES_SharePointOnline_";
        #region Column and value in excel file
        private string ContainerColumn = I18NEntity.GetString("RM_JS_BCM_Export_ContainerColumn");
        private string SiteCollectionColumn = I18NEntity.GetString("RM_JS_BCM_Export_SiteCollectionColumn");
        private string SiteColumn = I18NEntity.GetString("RM_JS_BCM_Export_SiteColumn");
        private string ListColumn = I18NEntity.GetString("RM_JS_BCM_Export_LibraryColumn");
        private string FolderColumn = I18NEntity.GetString("RM_JS_BCM_Export_FolderColumn");
        private string ManualApprovalTypeColumn = $"{I18NEntity.GetString("RM_JS_BCM_Export_ManualApprovalTypeColumn")}";
        #endregion
        public SPExportSettingProcessor(RMExportSettingJobMessage jobMsg) : base(jobMsg.JobID, jobMsg.JobType)
        {
            mFactoryCache = new Dictionary<string, AveObjectModelFactory>();
            mAveSiteCache = new Dictionary<string, IAveSite>();
            mAveWebCache = new Dictionary<string, IAveWeb>();
            mRemoteSCCache = new Dictionary<string, RemoteSiteCollection>();
            mAveBposInfoCache = new Dictionary<string, AveBPOSAccountInfo>();
            FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, BaseJobDto.Id.Replace("ESSP", StartFileName), ".csv");
            exportSettingType = jobMsg.exportSettingType;
        }
        protected override async Task GenerateDataAsync()
        {
            Logger.Info($"Current export setting type is {exportSettingType}");
            List<ExportSPSetting> sheetData = new List<ExportSPSetting>();
            bool isExportAll = exportSettingType == ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode;
            bool isSiteCollectionNode = false;
            foreach (var webApplication in WebApplications) //Group site
            {
                var webApplicationSetting = SharePointSettingDao.LoadSharePointSetting(new Guid(webApplication.SPObjectId), Guid.Empty);
                //check webApplication has term setting and column settings
                if (webApplicationSetting == null || (webApplicationSetting.IsUsingExistColumnName && !webApplicationSetting.SetDocLevelTermForExistColumn) || (string.IsNullOrEmpty(webApplicationSetting.ColumnName) && !webApplicationSetting.IsUsingExistColumnName)
                    || (webApplicationSetting.TermSetId == Guid.Empty))
                {
                    Logger.Info($"The web application {webApplication.Name} setting is invalid setting");
                    await HandleContainerInvalidSetting(sheetData, webApplication);
                    continue;
                }
                if (CheckSetting(webApplicationSetting, !isExportAll))
                {
                    sheetData.Add(ConvertSetting(webApplicationSetting, webApplication.Name));
                    GenerateJobDetail(webApplication.Name, webApplication.Name);
                }
                else if (isExportAll)
                {
                    HandleExportContainerSettingForExportAll(sheetData, webApplicationSetting, webApplication.Name);
                }
                //Get site
                webApplication.PageSize = int.MaxValue;
                var siteOfWebApplication = (await BrowseTreeService.BrowseSPOTreeAsync(webApplication, RMBrowseTreeNodeSourceType.SharepointOnline, true)).Children;
                if (siteOfWebApplication.Any())
                {
                    foreach (var site in siteOfWebApplication)
                    {
                        var settingUnderSite = SharePointSettingDao.LoadSPSettingsUnderSite(new Guid(site.SPObjectId));
                        if (settingUnderSite.Any())
                        {
                            var orderExportSetting = settingUnderSite.OrderBy(_ => _.FullPath).ThenBy(_ => _.WebId).ToList();
                            foreach (var setting in orderExportSetting)
                            {
                                try
                                {
                                    isSiteCollectionNode = setting.WebId == Guid.Empty;
                                    if (!CheckSetting(setting, !(isExportAll && isSiteCollectionNode)) && (!isExportAll || !isSiteCollectionNode))
                                    {
                                        continue;
                                    }
                                    var remoteSC = GetRemoteSite(site.FullPath);
                                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                                    SettingLevel level = ConvertNodeLevelToSettingLevel(node);
                                    //Check site of setting exist in SP
                                    if (FindAveSPObject(setting, level, site, await GetAveBPOSInfoAsync(remoteSC)))
                                    {
                                        if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                                            sheetData.Add(ConvertEmptySetting(setting.FullPath, webApplication.Name));
                                        else
                                            sheetData.Add(ConvertSetting(setting, webApplication.Name));
                                        GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1), setting.FullPath);
                                        continue;
                                    }
                                    GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1), setting.FullPath,
                                        "RM_JS_BCM_ImportSetting_NoSPObject", false);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error($"Validate site failed ,error: {e}");
                                    GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1), setting.FullPath,
                                            "RM_JS_BCM_ImportSetting_NoSPObject", false);
                                    continue;
                                }
                            }
                        }
                        if(isExportAll && (settingUnderSite == null || !settingUnderSite.Any(_ => _.WebId == Guid.Empty)))
                        {
                            var siteExportSetting = ConvertSetting(webApplicationSetting, webApplication.Name, true);
                            siteExportSetting.FullPath = site.FullPath;
                            sheetData.Add(siteExportSetting);
                            GenerateJobDetail(site.FullPath, site.FullPath);
                        }
                    }
                }
            }

            if (!sheetData.Any())
            {
                Logger.Info($"There is nothing to export SP settings");
                return;
            }

            //Convert data to csv
            int sheetIndex = 0;
            bool isCreateFile = true;
            var currentCount = 0;
            int pageIndex = 0;
            var settings = sheetData.Skip(pageIndex * PageSize).Take(PageSize).ToList();
            do
            {
                try
                {
                    currentCount += settings.Count;
                    var datas = new string[settings.Count + 1][];
                    pageIndex++;
                    if (isCreateFile)
                    {
                        datas = await GenerateSettingsAsync(BaseJobDto.JobType, datas, settings, true);
                        ExportToCsv(datas, FilePath);
                        isCreateFile = false;
                        Logger.Info($"Create Excel with header success,current count is {currentCount}");
                        continue;
                    }

                    if (currentCount >= CountOfOneSheet)
                    {
                        sheetIndex++;
                        datas = await GenerateSettingsAsync(BaseJobDto.JobType, datas, settings, true);
                        FilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto, $"_{sheetIndex}" + ".csv");
                        ExportToCsv(datas, FilePath);
                        currentCount = settings.Count;
                        Logger.Info($"Insert Excel with header success,current count is {currentCount},current sheet index is {sheetIndex}");
                        continue;
                    }
                    datas = await GenerateSettingsAsync(BaseJobDto.JobType, datas, settings, false);
                    AppendToCsv(datas, FilePath);
                    Logger.Info($"Insert data to sheet success,current count is {currentCount},current sheet index is {sheetIndex}");
                }
                catch (Exception e)
                {
                    Logger.Error($"Generate report detail to Excel error,currrent sheet index is {sheetIndex},error : {e}");
                    GenerateAndUploadFileManager.HasFailed = true;
                    throw;
                }
            } while ((settings = sheetData.Skip(pageIndex * PageSize).Take(PageSize).ToList()).Any());
        }

        private void HandleExportContainerSettingForExportAll(List<ExportSPSetting> sheetData, RMSharePointSetting webApplicationSetting, string containerName)
        {
            if(webApplicationSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                sheetData.Add(ConvertEmptySetting(string.Empty, containerName));
            }
            else
            {
                sheetData.Add(ConvertSetting(webApplicationSetting, containerName));
            }
            GenerateJobDetail(containerName, containerName);
        }

        protected override async Task GetListGroupHasSetting()
        {
            try
            {
                var farmNode = SPSettingTreeService.LoadFarmSampleTree()[0];
                farmNode.PageSize = int.MaxValue;
                var returnNode = await BrowseTreeService.BrowseSPOTreeAsync(farmNode, RMBrowseTreeNodeSourceType.SharepointOnline, true);
                SPSettingTreeService.TransChildrenNodeName(returnNode);
                var webApplications = returnNode.Children;
                if (webApplications != null && webApplications.Count > 0)
                {
                    var groupWebApplicationsByName = webApplications.GroupBy(w => w.Name).ToList();
                    var uniqueWebApplications = groupWebApplicationsByName.Where(g => g.Count() == 1)
                                                    .Select(g => g.First())
                                                    .ToList();
                    var duplicateNameWebApplications = groupWebApplicationsByName.Where(g => g.Count() > 1)
                                                    .Select(g => g.First())
                                                    .ToList();
                    WebApplications.AddRange(uniqueWebApplications);
                    if(duplicateNameWebApplications.Count > 0)
                    {
                        foreach(var webApplication in duplicateNameWebApplications)
                        {
                            GenerateJobDetail(webApplication.Name, "", "RM_JS_BCM_ImportSetting_DuplicateContainerName", false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        protected override async Task<string[][]> ConvertSettingToArrayAsync(List<ExportSPSetting> settings, string[][] datas)
        {
            int rowCount = 1;
            foreach (var setting in settings)
            {
                int colCount = 0;
                try
                {
                    var splitPath = SplitFullPath(setting);
                    datas[rowCount] = new string[16];
                    for (int i = 0; i < 16; i++)
                    {
                        datas[rowCount][i] = ProcessCol("");
                    }
                    datas[rowCount][colCount++] = ProcessCol(setting.ContainerName);
                    datas[rowCount][colCount++] = ProcessCol(splitPath.SiteCollection);
                    datas[rowCount][colCount++] = ProcessCol(splitPath.Site);
                    datas[rowCount][colCount++] = ProcessCol(splitPath.List);
                    datas[rowCount][colCount++] = ProcessCol(splitPath.Folder);
                    if (setting.IsEmptySetting)
                    {
                        Logger.Info($"Current setting is empty setting {setting.FullPath}");
                        if (setting.IsInheritSettingNode)
                        {
                            datas[rowCount][15] = ProcessCol("TRUE");
                        }
                        rowCount++;
                        continue;
                    }
                    if (setting.DeployTermMethod == (int)DeployTermMethod.NoDefaultTerm)
                    {
                        datas[rowCount][colCount++] = ProcessCol(ManuallyChooseATerm);
                    }
                    else if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                    {
                        datas[rowCount][colCount++] = ProcessCol(SetADefaultTerm);
                    }
                    else if(setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification || setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification)
                    {
                        datas[rowCount][colCount++] = ProcessCol(setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification ? AutoPopulate : SmartClassification);
                        if (setting.IsInheritSettingNode)
                        {
                            datas[rowCount][15] = ProcessCol("TRUE");
                        }
                        rowCount++;
                        continue;
                    }
                    string termScope = setting.TermId != Guid.Empty ? TermDAO.GetTermNamesPathByTermId(setting.TermId)
                        : TermDAO.GetTermSetNamesPathByTermSetId(setting.TermSetId);
                    datas[rowCount][colCount++] = ProcessCol(termScope.Replace('/', PathSeparator).Replace(@"""", "\"\""));
                    string defaultTerm = setting.DefaultTermId == Guid.Empty ? string.Empty
                        : TermDAO.GetTermNamesPathByTermId(setting.DefaultTermId).Replace(termScope, "").Replace('/', PathSeparator);
                    datas[rowCount][colCount++] = ProcessCol(defaultTerm.TrimStart(PathSeparator).Replace(@"""", "\"\""));
                    datas[rowCount][colCount++] = ProcessCol(setting.NeedCheckDefaultValue ? "TRUE" : "");
                    datas[rowCount][colCount++] = ProcessCol(setting.IncludeDeclaredRecords ? "TRUE" : "");
                    datas[rowCount][colCount++] = ProcessCol(setting.ApplyTermIncludeFolder == null ? "" : (setting.ApplyTermIncludeFolder == true ? "TRUE" : ""));
                    datas[rowCount][colCount++] = ProcessCol(setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite && (setting.NeedCheckDefaultValue == true || setting.ApplyTermIncludeFolder == true) ? "TRUE" : "");
                    if (setting.ApprovalType == ApprovalType.None)
                    {
                        datas[rowCount][colCount++] = ProcessCol(NoManualSetting);
                        datas[rowCount][colCount++] = ProcessCol("");
                    }
                    else if (setting.ApprovalType == ApprovalType.ApprovalProcess)
                    {
                        var workflow = RMWorkflowDefinitionDAO.GetWorkflowByReferenceId(new Guid(setting.WorkflowReferenceId));
                        if (workflow == null)
                        {
                            GenerateJobDetail(setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"\") + 1), setting.FullPath,
                                    "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty", false);
                            continue;
                        }
                        datas[rowCount][colCount++] = ProcessCol(WorkflowProcess);
                        datas[rowCount][colCount++] = ProcessCol(workflow.Name.Replace(@"""", "\"\""));
                    }
                    else if (setting.ApprovalType == ApprovalType.RecordOwners)
                    {
                        datas[rowCount][colCount++] = ProcessCol(RecordOwner);
                        var usernames = (await RecordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.SharePoint)).Select(_ => _.UserPrincipalName);
                        datas[rowCount][colCount++] = ProcessCol(string.Join(PathSeparator, usernames).Replace(@"""", "\"\""));
                    }
                    else
                    {
                        datas[rowCount][colCount++] = ProcessCol(AutoApprove);
                        datas[rowCount][colCount++] = ProcessCol("");
                    }
                    datas[rowCount][colCount++] = ProcessCol(setting.EMailToRecordOwner ? "TRUE" : "FALSE");
                    datas[rowCount][colCount++] = ProcessCol(setting.IsInheritSettingNode ? "TRUE" : "FALSE");
                    rowCount++;
                }
                catch (Exception e)
                {
                    Logger.Error($"Convert SharePoint setting To Array failed {e}");
                    rowCount++;
                    throw;
                }

            }
            return datas;
        }

        private async Task HandleContainerInvalidSetting(List<ExportSPSetting> sheetData, RMSPSampleTreeNode webApplication)
        {
            if (exportSettingType == AvePoint.RA.Contract.FunctionSetting.ExportSettingType.ExportAllSiteCollectionNodesAndCustomSettingNode)
            {
                sheetData.Add(ConvertEmptySetting(string.Empty, webApplication.Name));
                GenerateJobDetail(webApplication.Name, webApplication.Name);
                webApplication.PageSize = int.MaxValue;
                var siteUnderWebApplication = (await BrowseTreeService.BrowseSPOTreeAsync(webApplication, RMBrowseTreeNodeSourceType.SharepointOnline, true)).Children;
                if (siteUnderWebApplication.Any())
                {
                    foreach (var site in siteUnderWebApplication)
                    {
                        GenerateJobDetail(site.FullPath, site.FullPath);
                        sheetData.Add(ConvertEmptySetting(site.FullPath, webApplication.Name, true));
                    }
                }
            }
        }

        private string ProcessCol(string col)
        {
            //return $"\"=\"\"{col}\"\"\"";
            return col;
        }
        protected override string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName)
        {
            int colCount = 0;
            datas[0] = new string[16];
            datas[0][colCount++] = ContainerColumn;
            datas[0][colCount++] = SiteCollectionColumn;
            datas[0][colCount++] = SiteColumn;
            datas[0][colCount++] = ListColumn;
            datas[0][colCount++] = FolderColumn;
            datas[0][colCount++] = ApplyTermByColumn;
            datas[0][colCount++] = TermScopeColumn;
            datas[0][colCount++] = DefaultTermColumn;
            datas[0][colCount++] = ApplyToExistingDocumentsColumn;
            datas[0][colCount++] = ApplyToExistingDeclaredRecordsColumn;
            datas[0][colCount++] = ApplyToDocumentSetsAndFoldersColumn;
            datas[0][colCount++] = OverwriteTheExistingTermColumn;
            datas[0][colCount++] = ManualApprovalTypeColumn;
            datas[0][colCount++] = SendEmailForPersonColumn;
            datas[0][colCount++] = SendEmailNotificationColumn;
            datas[0][colCount++] = IsInheritSetting;
            return datas;
        }

        private bool CheckSetting(RMSharePointSetting setting, bool isShowJobDetail = true)
        {
            string comment = GetSettingValidationComment(setting);
            if (isShowJobDetail && !string.IsNullOrEmpty(comment))
            {
                var deployDoNotSupportMethods = new[] { (int)DeployTermMethod.UseIntelligenceClassification, (int)DeployTermMethod.UseAutoClassification };
                string objectName = setting.FullPath.Substring(setting.FullPath.LastIndexOf(@"/") + 1);
                if (deployDoNotSupportMethods.Contains(setting.DeployTermMethod))
            {
                    GenerateJobDetailWithStatus(objectName, setting.FullPath, AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, comment);
                    return false;
            }
                GenerateJobDetail(objectName, setting.FullPath, comment, false);
            }
            return string.IsNullOrEmpty(comment);
            }

        private string GetSettingValidationComment(RMSharePointSetting setting)
            {
            if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                return "RM_JS_BCM_ExportSetting_DisableState";

            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
                return "RM_JS_BCM_ExportSetting_AutoClassificationSupport";

            if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification)
                return "RM_JS_BCM_ExportSetting_SmartClassificationSupport";

            if (setting.ApprovalType == ApprovalType.ApprovalProcess && !Guid.TryParse(setting.WorkflowReferenceId, out _))
                return "RM_JS_BCM_ExportSetting_WorkFlowProcessEmpty";

            return string.Empty;
        }

        private SettingLevel ConvertNodeLevelToSettingLevel(RMSPTreeNode node)
        {
            switch ((NodeLevel)node.Level)
            {
                case NodeLevel.Folder:
                case NodeLevel.DesignFolder:
                    return SettingLevel.Folder;
                case NodeLevel.List:
                    return SettingLevel.List;
                case NodeLevel.Site:
                    if (node.Name == ".")
                        return SettingLevel.RootWeb;
                    return SettingLevel.SubWeb;
                case NodeLevel.SiteCollection:
                    return SettingLevel.SiteCollection;
                default:
                    return SettingLevel.None;

            }
        }

        private void ExportToCsv(string[][] datas, string csvFilePath)
        {
            var csvContent = new StringBuilder();

            foreach (var row in datas)
            {
                if (row != null)
                {
                    var rowContent = StringUtils.ToCSVString(row);
                    csvContent.AppendLine(rowContent);
                }
            }
            File.WriteAllText(csvFilePath, csvContent.ToString(), Encoding.UTF8);
        }

        private void AppendToCsv(string[][] datas, string csvFilePath)
        {
            var csvContent = new StringBuilder();

            foreach (var row in datas)
            {
                if (row != null)
                {
                    var rowContent = string.Join(",", row);
                    csvContent.AppendLine(rowContent);
                }
            }

            File.AppendAllText(csvFilePath, csvContent.ToString(), Encoding.UTF8);
        }

        private (string SiteCollection, string Site, string List, string Folder) SplitFullPath(ExportSPSetting setting)
        {
            if (setting.IsInheritSettingNode || setting.IsEmptySetting)
            {
                return (setting.FullPath, "", "", "");
            }

            (string SiteCollection, string Site, string List, string Folder) splitPath = ("", "", "", "");
            var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
            do
            {
                var level = ConvertNodeLevelToSettingLevel(node);
                switch (level)
                {
                    case SettingLevel.Folder:
                        splitPath.Folder = string.IsNullOrEmpty(splitPath.Folder) ? node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) : node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) + "/" + splitPath.Folder;
                        break;
                    case SettingLevel.List:
                        splitPath.List = string.IsNullOrEmpty(splitPath.List) ? node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) : node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) + "/" + splitPath.List;
                        splitPath.List = node.FullPath.Contains("Lists") ? "Lists/" +  splitPath.List : splitPath.List;
                        break;
                    case SettingLevel.SubWeb:
                        splitPath.Site = string.IsNullOrEmpty(splitPath.Site) ? node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) : node.FullPath.Substring(node.FullPath.LastIndexOf(@"/") + 1) + "/" + splitPath.Site;
                        break;
                    case SettingLevel.RootWeb:
                        splitPath.Site = string.IsNullOrEmpty(splitPath.Site) ? "." : splitPath.Site;
                        break;
                    case SettingLevel.SiteCollection:
                        splitPath.SiteCollection = node.FullPath;
                        return splitPath;
                    default:
                        break;
                }
                node = node.Parent;
            } while (node != null);
            return splitPath;
        }

        private RemoteSiteCollection GetRemoteSite(string scUrl)
        {
            RemoteSiteCollection site = null;
            if (!mRemoteSCCache.TryGetValue(scUrl, out site))
            {
                //DAOAPIClientV1 test = new DAOAPIClientV1();
                //site = test.GetRemoteSiteCollectionByUrl(scUrl);
                site = RABrowserClient.GetRemoteSiteCollectionByUrl(scUrl);
                if (site == null)
                {
                    Logger.Warn($"Can not find sitecollection.Url: {scUrl}");
                    throw new Exception("RM_JS_BCM_ImportSetting_NoSC");
                }
                mRemoteSCCache.Add(scUrl, site);
            }
            return site;
        }
        private async Task<AveBPOSAccountInfo> GetAveBPOSInfoAsync(RemoteSiteCollection sc)
        {
            AveBPOSAccountInfo result = null;
            if (!mAveBposInfoCache.TryGetValue(sc.id, out result))
            {
                result = await PoolUserUtil.GetBPOSInfoAsync(sc);
                if (result != null)
                {
                    mAveBposInfoCache.Add(sc.id, result);
                }
            }
            return result;
        }
        private bool FindAveSPObject(RMSharePointSetting setting, SettingLevel level, RMSPSampleTreeNode site, AveBPOSAccountInfo userInfo)
        {
            object result = null;
            try
            {
                Logger.Info("Start to get sp object");
                AveObjectModelFactory factory = GetFactory(site, userInfo);
                IAveSite aveSite = GetAveSite(factory, site.FullPath);
                IAveWeb aveWeb = null;
                if ((int)level > (int)SettingLevel.RootWeb)
                {
                    string webServerRelativeUrl = string.Empty;
                    if (level == SettingLevel.SubWeb)
                    {
                        webServerRelativeUrl = WebUtil.MakeServerRelativeUrl(setting.FullPath);
                    }
                    else
                    {
                        webServerRelativeUrl = WebUtil.MakeServerRelativeUrl(factory.CreateSiteServiceHelper().TryToRectifySiteUrl(setting.FullPath, userInfo));
                    }
                    aveWeb = GetAveWeb(aveSite, webServerRelativeUrl);
                    Logger.Info($"Web Url:{aveWeb.Url}");
                    if (aveWeb == null || !aveWeb.Exists)
                    {
                        Logger.Error($"Cannot find web in SharePoint Online");
                        throw new Exception("RM_JS_BCM_ImportSetting_NoSPObject");
                    }
                }
                switch (level)
                {
                    case SettingLevel.SiteCollection:
                        result = aveSite;
                        break;
                    case SettingLevel.RootWeb:
                        //result = aveSite.RootWeb;
                        var web = GetAveWeb(aveSite, WebUtil.MakeServerRelativeUrl(site.FullPath));
                        return web.ID == setting.ScopeId;
                    case SettingLevel.SubWeb:
                        return aveWeb?.ID == setting.ScopeId;
                    case SettingLevel.List:
                        ArgumentCheck.CheckNotNull(aveWeb);
                        var list = aveWeb?.GetList(WebUtil.MakeServerRelativeUrl(setting.FullPath));
                        return list?.ID == setting.ScopeId;
                    case SettingLevel.Folder:
                        ArgumentCheck.CheckNotNull(aveWeb);
                        var folder = aveWeb?.GetFolder(WebUtil.MakeServerRelativeUrl(setting.FullPath));
                        return folder?.Exists == true && folder?.UniqueId == setting.ScopeId ? true : false;
                    default:
                        result = null;
                        break;
                }
                if (result == null)
                {
                    Logger.Error($"Cannot find SharePoint Online object with url.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }
            return true;
        }
        private IAveSite GetAveSite(AveObjectModelFactory factory, string scUrl)
        {
            IAveSite aveSite = null;
            if (!mAveSiteCache.TryGetValue(scUrl, out aveSite))
            {
                aveSite = factory.CreateSite(scUrl);
                if (aveSite != null)
                {
                    mAveSiteCache.Add(scUrl, aveSite);
                }
            }
            return aveSite;
        }
        private IAveWeb GetAveWeb(IAveSite aveSite, string serverRelativeUrl)
        {
            IAveWeb aveWeb = null;
            if (!mAveWebCache.TryGetValue(serverRelativeUrl, out aveWeb))
            {
                aveWeb = aveSite.OpenWeb(serverRelativeUrl);
                if (aveWeb != null && aveWeb.Exists)
                {
                    mAveWebCache.Add(serverRelativeUrl, aveWeb);
                }
            }
            return aveWeb;
        }

        private AveObjectModelFactory GetFactory(RMSPSampleTreeNode site, AveBPOSAccountInfo userInfo)
        {
            AveObjectModelFactory factory = null;
            if (!mFactoryCache.TryGetValue(site.FullPath, out factory))
            {
                factory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, userInfo, AveContextKind.ClientObjectModel);
                mFactoryCache.Add(site.FullPath, factory);
            }
            return factory;
        }

        private ExportSPSetting ConvertSetting(RMSharePointSetting setting, string ContainerName, bool isInheritSettingNode = false)
        {
            return new ExportSPSetting
            {
                Id = setting.Id,
                NodeInfo = setting.NodeInfo,
                TermId = setting.TermId,
                TermSetId = setting.TermSetId,
                DefaultTermId = setting.DefaultTermId,
                NeedCheckDefaultValue = setting.NeedCheckDefaultValue,
                IncludeDeclaredRecords = setting.IncludeDeclaredRecords,
                ApplyTermIncludeFolder = setting.ApplyTermIncludeFolder,
                ApplyExistType = setting.ApplyExistType,
                ApprovalType = setting.ApprovalType,
                EMailToRecordOwner = setting.EMailToRecordOwner,
                WorkflowReferenceId = setting.WorkflowReferenceId,
                DeployTermMethod = setting.DeployTermMethod,
                ContainerName = ContainerName,
                FullPath = setting.FullPath,
                IsInheritSettingNode = isInheritSettingNode
            };
        }

        private ExportSPSetting ConvertEmptySetting(string fullPath, string containerName, bool isInheritSetting = false)
        {
            return new ExportSPSetting
            {
                FullPath = fullPath,
                IsEmptySetting = true,
                ContainerName = containerName,
                IsInheritSettingNode = isInheritSetting
            };
        }
    }

    public enum SettingLevel
    {
        None = 0,
        SiteCollection = 1,
        RootWeb = 2,
        SubWeb = 3,
        List = 4,
        Folder = 5
    }

    public class ExportSPSetting
    {
        public int Id { set; get; }
        public string NodeInfo { get; set; }
        public Guid TermId { get; set; }
        public Guid TermSetId { get; set; }
        public Guid DefaultTermId { get; set; }
        public bool NeedCheckDefaultValue { get; set; }
        public bool IncludeDeclaredRecords { get; set; }
        public bool? ApplyTermIncludeFolder { get; set; }
        public int ApplyExistType { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public bool EMailToRecordOwner { get; set; }
        public string WorkflowReferenceId { get; set; }
        public int DeployTermMethod { get; set; }
        public string ContainerName { get; set; }
        public string FullPath { get; set; }
        public bool IsInheritSettingNode { get; set; }
        public bool IsEmptySetting { get; set; }
    }
}
