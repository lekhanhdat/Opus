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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;


namespace AvePoint.RA.Web.Controllers.RetentionDisposal
{
    [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any, preferred: false)]
    public class GlobalSearchApiController : BaseApiController
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #region interface
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IPermissionManagementService _PermissionManagementService;
        private IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService(ref _PermissionManagementService);
        private ITermDao _TermDao;
        private ITermDao TermDao => PlatformWindsorManager.GetService(ref _TermDao);
        private ILabelDao _LabelDao;
        private ILabelDao LabelDao => PlatformWindsorManager.GetService(ref _LabelDao);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);



        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);
        

        #endregion

        [HttpPost]
        [ValidateGlobalSearchActionFilter]
        public async Task<RAReturnMessage> DoAction([FromBody] GlobalSearchActionDto actionDto)
        {
            RAReturnMessage message = await VialidateParameterAsync(actionDto);
            if (message.MessageType == RAMessageType.Successful)
            {
                if (actionDto.IsRealTimeAction)
                {
                    message = ExplorerService.DoGlobalSearchRealTimeAction(actionDto);
                }
                else
                {
                    message = ExplorerService.StartGlobalSearchActionJob(actionDto);
                }
            }
            return message;
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public async Task<ActionResult> ExportSearchResult()
        {
            Logger.Debug("ExportSearchResult controller");
            try
            {
                string globalSearchExport = HttpUtility.UrlDecode(Request.Form["globalSearchExport"]);
                GlobalSearchExportDto dto = JsonConvert.DeserializeObject<GlobalSearchExportDto>(globalSearchExport);
                var exportLimitCount = TenantService.GetExportResultLimit();
                var defaultLimitCount = 5000;
                Guid exportUniqueId = Guid.Empty;
                if (exportLimitCount > defaultLimitCount)
                {
                    string exportFlag = HttpUtility.UrlDecode(Request.Form["exportFlag"]);
                    exportUniqueId = !string.IsNullOrEmpty(exportFlag) ? new Guid(exportFlag) : Guid.Empty;
                    TaxonomyService.CreateExportStatusRecord(exportUniqueId);
                }

                DateTime nowTime = DateTime.UtcNow;
                string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
                string fileName = I18NEntity.GetString("RM_JM_ExportSearchResultReport") + "_" + nowTimeStr + ".xlsx";
                string filePath = await ExplorerService.ExportSearchResultAsync(dto);

                //            if (string.IsNullOrWhiteSpace(filePath))
                //            {
                //                return new StatusCodeResult((int)HttpStatusCode.NoContent);
                //}
                //            FileTransferStream stream = new FileTransferStream(filePath, filePath.Substring(0, filePath.LastIndexOf('\\')), FileMode.Open);
                
                //            if (stream == null)
                //            {
                //	return new StatusCodeResult((int)HttpStatusCode.NoContent);
                //}
                if (exportLimitCount > defaultLimitCount)
                { 
                    TaxonomyService.UpdateExportStatus(exportUniqueId, ExportTermsWithRulesStatus.Finished);
                }
                return File(StreamUtl.ReadFile(filePath), "application/octet-stream", fileName);
				//HttpResponseMessage response = new HttpResponseMessage();
				//response.Content = new StreamContent(stream);
				//response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				//response.Content.Headers.ContentDisposition.FileName = WebUtil.ConvertFileName(fileName + ".xlsx");
				//response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				//response.Content.Headers.ContentLength = stream.Length;
				//return response;
			}
            catch (Exception e)
            {
                Logger.Error("An error occurred while ExportSearchResult, error:{0}", e.ToString());
				return new StatusCodeResult((int)HttpStatusCode.NoContent);
			}
            //Logger.Debug("ExportSearchResult finish.");
            //return new StatusCodeResult((int)HttpStatusCode.NoContent);
		}

		[HttpPost]
        public Task<RAReturnMessage> StartExportSearchResultJob([FromBody] GlobalSearchExportDto dto)
        {
            return ExplorerService.StartExportSearchResultJobAsync(dto);
        }

        [HttpGet]
        public Task<string> GetJobExportSetting()
        {
            return JobMonitorService.GetExportSettingsAsync(true);
        }

        //[HttpPost]
        //public string GetRecordReleaseTime(List<Guid> recordIds)
        //{
        //    return JsonConvert.SerializeObject(ExplorerService.GetRecordReleaseTime(recordIds));
        //}

        private async Task<RAReturnMessage> VialidateParameterAsync(GlobalSearchActionDto actionDto)
        {
            var returnMessage = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                switch (actionDto.Action)
                {
                    case GlobalSearchAction.AccessControl:
                        GSPermissionSimpleDto simpleDto = JsonConvert.DeserializeObject<GSPermissionSimpleDto>(actionDto.ActionExtension.ToString());
                        var syncUserResult = await PermissionManagementService.SyncADUsersAsync(simpleDto.Accounts);
                        if (syncUserResult.MessageType != RAMessageType.Successful)
                        {
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = syncUserResult.ErrorMessage;
                            return returnMessage;
                        }
                        //actionDto.ForceDiscoverAll = false;
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(await GetJobContextDtoAsync(simpleDto));
                        break;
                    case GlobalSearchAction.DeclareRecords:
                    case GlobalSearchAction.UnDeclareRecords:
                        actionDto.ActionExtension = WebUtil.LogOnUserName;
                        break;
                    case GlobalSearchAction.MoveTo:
                        if ((SourceFlag)actionDto.SourceFlag == SourceFlag.Physical)
                        {
                            PhysicalMoveDto physicalMoveDto = JsonConvert.DeserializeObject<PhysicalMoveDto>(actionDto.ActionExtension.ToString());
                            actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(GetPhysicalMoveOption(physicalMoveDto));
                        }
                        else if ((SourceFlag)actionDto.SourceFlag == SourceFlag.SharePoint || (SourceFlag)actionDto.SourceFlag == SourceFlag.OneDrive || (SourceFlag)actionDto.SourceFlag == SourceFlag.Teams || (SourceFlag)actionDto.SourceFlag == SourceFlag.Groups)
                        {
                            MoveToDto physicalMoveDto = JsonConvert.DeserializeObject<MoveToDto>(actionDto.ActionExtension.ToString());
                            actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(ConvertToRMExplorerMoveJobMessage(physicalMoveDto));
                        }
                        break;
                    case GlobalSearchAction.Reclassify:
                        ChangeTermDto changeTermDto = JsonConvert.DeserializeObject<ChangeTermDto>(actionDto.ActionExtension.ToString());                
                        RMTerm selectedTerm = new();                    
                        selectedTerm = TermDao.GetRMTermByUniqueId(changeTermDto.TermInfo.UniqueId, false);
                        if (selectedTerm.IsDeprecated || selectedTerm.IsExpired || changeTermDto.TermInfo == null)
                        {
                            string message = I18NEntity.GetString("RM_JS_JMD_Comment_Auto_TermNotAvailable");
                            returnMessage.ErrorMessage = message;
                            returnMessage.MessageType = RAMessageType.Failed;
                            return returnMessage;
                        }
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(GetChangeTermOption(changeTermDto));                                                             
                        break;
                    case GlobalSearchAction.PhysicalBulkUpdate:
                        Dictionary<string, string> physicalUpdateDto = JsonConvert.DeserializeObject<Dictionary<string, string>>(actionDto.ActionExtension.ToString());
                        if (physicalUpdateDto.Keys != null)
                        {
                            if (DefaultColumnIDs.HideForBulkUpdateIDs.Any(c => physicalUpdateDto.Keys.Contains(c)))
                            {
                                string message = "Edit Column Invalid Physical Objects";//Not displayed in gui
                                returnMessage.ErrorMessage = message;
                                returnMessage.MessageType = RAMessageType.Failed;
                                return returnMessage;
                            }
                        }
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(physicalUpdateDto);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while validating parameter. Error{e.ToString()}");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        private async Task<ScopePermissionJobContextDto> GetJobContextDtoAsync(GSPermissionSimpleDto dto)
        {
            var jd = new ScopePermissionJobContextDto
            {
                GSJobContextDto = new GSPermissionJobContextDto
                {
                    UserId = TenantLocalValue.LogonUserId,
                    //目前只支持打破继承
                    IsInheritSave = false,
                    //权限类型暂时是All权限
                    PermissionType = RMScopePermissionEnum.All,
                    //Search Result方式设置权限，Query参数
                    QueryDto = dto.QueryDto,

                    QueryV3Dto = dto.QueryV3Dto,
                    //UI选中的Physical数据
                    NodeIds = dto.NodeIds,
                    //对于已经打破继承的数据，批量设置权限User时，是Append还是Overwrite
                    UserConflictOption = dto.UserConflictOption
                }
            };

            var accountIds = new List<int>();
            var uiAccounts = dto.Accounts;
            if (uiAccounts != null && uiAccounts.Count > 0)
            {
                accountIds = uiAccounts.Select(o => o.RMUserId).Distinct().ToList();
            }
            //UI设置的Permission Users
            jd.GSJobContextDto.AccountIds = accountIds;

            if (jd.GSJobContextDto.QueryDto != null)
            {
                //获取EndUser权限Id集合赋值到QueryDto中，确保Job中查询的数据都是EndUser有权限的
                jd.GSJobContextDto.QueryDto.PermissionIds = await ExplorerService.GetPermissionConditionAsync();
                jd.GSJobContextDto.QueryDto.IsForGlobalSearchJob = true;
            }
            return jd;
        }

        private PhysicalMoveOption GetPhysicalMoveOption(PhysicalMoveDto moveDto)
        {
            PhysicalMoveOption physicalMoveOption = new PhysicalMoveOption()
            {
                SourcePhyRecordIds = moveDto.SourcePhyRecordIds,
                LocationId = moveDto.LocationId,
                BoxId = moveDto.BoxId,
                NameConflictOption = (AvePoint.RA.Contract.Object.RealTime.NameConflictOption)moveDto.NameConflictOption,
                HoldConflictOption = (AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption)moveDto.HoldConflictOption

            };
            return physicalMoveOption;
        }

        private RMExplorerMoveJobMessage ConvertToRMExplorerMoveJobMessage(MoveToDto dto)
        {
            var moveRecordsOption = new RMExplorerMoveJobMessage();
            //var moveRecordsOption = new ARE.MoveOption();

            #region Source Records
            if (dto.SourceRecords != null && dto.SourceRecords.Count > 0)
            {
                if (dto.SourceRecords[0].SourceFlag == (int)RecordFlag.SP || dto.SourceRecords[0].SourceFlag == (int)RecordFlag.OneDrive || dto.SourceRecords[0].SourceFlag == (int)RecordFlag.Teams || dto.SourceRecords[0].SourceFlag == (int)RecordFlag.Groups)
                {
                    moveRecordsOption.SourceFlag = RecordFlag.SP;
                }
                else if (dto.SourceRecords[0].SourceFlag == 2)
                {
                    moveRecordsOption.SourceFlag = RecordFlag.FS;
                }
            }

            moveRecordsOption.SourceRecords = new List<SourceRecord>();
            Dictionary<string, RemoteSiteCollection> sitesDic = new Dictionary<string, RemoteSiteCollection>();
            // DAOAPIClientV1 docAveClient = new DAOAPIClientV1();
            foreach (var rec in dto.SourceRecords)
            {
                RemoteSiteCollection site = null;
                if (sitesDic.ContainsKey(rec.AveSiteId))
                {
                    site = sitesDic[rec.AveSiteId];
                }
                else
                {
                    site = RABrowserClient.GetRemoteSiteCollectionById(rec.AveSiteId);
                    if (site != null)
                    {
                        sitesDic.Add(site.id, site);
                    }
                }
                var siteUrl = string.Empty;
                var password = string.Empty;
                var username = string.Empty;
                if (site != null)
                {
                    siteUrl = site.url;
                    //username = site.username;
                    //var bposInfo = PoolUserUtil.GetBPOSInfo(site);
                }
                else
                {
                    Logger.Warn("get source record site error, site id: {0}, full path: {1}", rec.AveSiteId, rec.FullPath);
                }
                moveRecordsOption.SourceRecords.Add(new SourceRecord()
                {
                    SourceFlag = (RecordFlag)rec.SourceFlag,
                    AveSiteId = rec.AveSiteId,
                    DeclareAsRecord = rec.DeclareAsRecord,
                    DirPath = rec.DirPath,
                    DisposalAction = rec.DisposalAction,
                    DisposalDueDate = rec.DisposalDueDate,
                    FolderId = rec.FolderId,
                    FullPath = rec.FullPath,
                    HoldStatus = rec.HoldStatus,
                    Id = rec.Id,
                    ItemId = rec.ItemId,
                    ItemRowId = rec.ItemRowId,
                    LeafName = rec.LeafName,
                    ListId = rec.ListId,
                    MetaInfo = rec.MetaInfo,
                    NodeId = rec.NodeId,
                    NodeType = rec.NodeType,
                    RecordsId = rec.RecordsId,
                    ReleaseTime = rec.ReleaseTime,
                    RuleId = rec.RuleId,
                    RuleName = rec.RuleName,
                    ScopeId = rec.ScopeId,
                    TermId = rec.TermId,
                    TermName = rec.TermName,
                    TimeCreated = rec.TimeCreated,
                    TimeLastModified = rec.TimeLastModified,
                    WebId = rec.WebId,
                    SiteUrl = siteUrl,
                    UserName = username,
                    Password = password
                });
            }
            #endregion

            #region Destination Records
            moveRecordsOption.DestFlag = dto.DestMode == Contract.RMWeb.DestMode.SharePoint ? RecordFlag.SP : RecordFlag.FS;
            moveRecordsOption.MoveDestination = new MoveDestination();
            if (dto.DestMode == Contract.RMWeb.DestMode.SharePoint)
            {
                if (dto.IsSpecifyLocation)
                {
                    //if (dto.SPAccount.AccountType == AccountType.Local)
                    //{
                    //    //local sp url is FSAccount
                    //    msg.MoveRecordsOption.MoveDestination.AccountType = DestAccountType.DomainAccount;
                    //    IMArchiverService ArchiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
                    //    msg.MoveRecordsOption.MoveDestination.FSAccount = ArchiverService.GetAccountProfileById(dto.SPAccount.Id);
                    //    msg.MoveRecordsOption.MoveDestination.FSAccountProfileId = dto.SPAccount.Id;
                    //}
                    //else
                    //{
                    //    //o365 sp url is SPAccount
                    //    msg.MoveRecordsOption.MoveDestination.AccountType = DestAccountType.O365Account;
                    //    IMOffice365AccountService Office365AccountService = DocAveServiceHelper.CreateServiceClient<IMOffice365AccountService>();
                    //    msg.MoveRecordsOption.MoveDestination.SPAccount = Office365AccountService.GetOffice365AccountById(dto.SPAccount.Id);
                    //    msg.MoveRecordsOption.MoveDestination.SPAccountProfileId = dto.SPAccount.Id;
                    //}
                    moveRecordsOption.MoveDestination.DestMode = Contract.Explorer.DestMode.UrlMode;
                    moveRecordsOption.MoveDestination.SPUrl = dto.LocationPath;
                    moveRecordsOption.MoveDestination.AccountType = DestAccountType.O365Account;
                    moveRecordsOption.MoveDestination.AveSiteId = dto.CheckLocationObject.AveSiteId;
                    moveRecordsOption.MoveDestination.RootSiteUrl = dto.CheckLocationObject.DestRootPath;
                    moveRecordsOption.MoveDestination.ContainerId = dto.CheckLocationObject.ContainerId;
                }
                else
                {
                    moveRecordsOption.MoveDestination.DestMode = Contract.Explorer.DestMode.TreeMode;
                    moveRecordsOption.MoveDestination.SPTreeNode = RMDtoConverter.ConvertRMTree2SPTree(dto.SPTree);
                    string containerId = string.Empty;
                    var connNode = GetSiteCollectionNode(dto.SPTree);
                    if (dto.SPTree.Type == ContentSourceType.Teams)
                    {
                        containerId = TreeNodeUtil.GetTeamsNode(dto.SPTree).ParentId;
                    }
                    else
                    {
                        containerId = connNode.ParentId;
                    }
                    if (connNode != null)
                    {
                        moveRecordsOption.MoveDestination.AveSiteId = new Guid(connNode.Id);
                        moveRecordsOption.MoveDestination.RootSiteUrl = connNode.FullPath;

                        moveRecordsOption.MoveDestination.AccountType = DestAccountType.O365Account;
                        moveRecordsOption.MoveDestination.ContainerId = containerId;
                    }
                    else
                    {
                        throw new Exception("can not get AveScopeId/RootSiteUrl from tree.");
                    }
                }
            }

            moveRecordsOption.MoveDestination.KeepSourceClassification = dto.isKeepClassification;
            #region remvoe fs code
            //else
            //{
            //    if (dto.IsSpecifyLocation)
            //    {
            //        moveRecordsOption.MoveDestination.DestMode = Contract.Explorer.DestMode.UrlMode;
            //        moveRecordsOption.MoveDestination.FSPath = dto.LocationPath;
            //        moveRecordsOption.MoveDestination.FSAccount = new Office365AccountInfo
            //        {
            //            UserName = dto.CheckLocationObject.UserInfoName,
            //        };
            //        moveRecordsOption.MoveDestination.AveScopeId = dto.CheckLocationObject.AveScopeId;
            //        moveRecordsOption.MoveDestination.RootSiteUrl = dto.CheckLocationObject.DestRootPath;
            //    }
            //    else
            //    {
            //        //moveRecordsOption.MoveDestination.DestMode = Contract.Explorer.DestMode.TreeMode;
            //        //moveRecordsOption.MoveDestination.FSTreeNode = RMDtoConverter.ConvertRMTree2FSTree(dto.FSTree, null, true);

            //        //var connNode = GetConnectionNode(dto.FSTree);
            //        //if (connNode != null)
            //        //{
            //        //    moveRecordsOption.MoveDestination.AveScopeId = connNode.Id;
            //        //    moveRecordsOption.MoveDestination.RootSiteUrl = connNode.FullPath;
            //        //}
            //        //else
            //        //{
            //        //    throw new Exception("can not get AveScopeId/RootSiteUrl from tree.");
            //        //}
            //    }
            //}
            #endregion
            #endregion

            #region Move Settings

            #region remove code
            //public ConflictType ConflictType { get; set; }
            //public ConflictOption ContainerLevelConflictOption { get; set; }
            //public ConflictOption ItemLevelConflictOption { get; set; }

            //public bool FileInherit { get; set; }
            //public NameConflictOption FileNameConflictOption { get; set; }
            //public NameConflictOption FolderFilesNameConflictOption { get; set; }
            //public bool FolderInherit { get; set; }
            //public NameConflictOption FolderNameConflictOption { get; set; }
            //public bool FolderUnderInherit { get; set; }



            //msg.MoveRecordsOption.MoveSetting.FolderNameConflictOption = (NameConflictOption)dto.FolderNameConflictOption;
            //msg.MoveRecordsOption.MoveSetting.FolderFilesNameConflictOption = (NameConflictOption)dto.FolderFilesNameConflictOption;
            //msg.MoveRecordsOption.MoveSetting.FolderInherit = dto.FolderInherit;
            //msg.MoveRecordsOption.MoveSetting.FolderUnderInherit = dto.FolderUnderInherit;

            //msg.MoveRecordsOption.MoveSetting.FileNameConflictOption = (NameConflictOption)dto.FileNameConflictOption;
            //msg.MoveRecordsOption.MoveSetting.FileInherit = dto.FileInherit;
            #endregion

            moveRecordsOption.MoveSetting = new MoveRecordSetting
            {
                ConflictType = dto.DestMode == Contract.RMWeb.DestMode.SharePoint ? ConflictType.SharePointConflict : ConflictType.FileSystemConflict,

                ContainerLevelConflictOption = ConflictOption.Merge
            };

            var itemLevelConflictOption = dto.FileNameConflictOption;
            switch (itemLevelConflictOption)
            {
                case FileNameConflictOption.Skip:
                    moveRecordsOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Skip;
                    break;
                case FileNameConflictOption.Overwrite:
                    moveRecordsOption.MoveSetting.ItemLevelConflictOption = ConflictOption.Overwrite;
                    break;
                case FileNameConflictOption.Rename:
                    moveRecordsOption.MoveSetting.ItemLevelConflictOption = ConflictOption.AppendByName;
                    break;
                default:
                    //msg.MoveRecordsOption.MoveSetting.ItemLevelConflictOption = default(ExplorerMove.ConflictOption);
                    break;
            }

            #endregion

            #region FS to SP mapping
            //moveRecordsOption.MoveSetting.FilePropertiesMapping = new FilePropertiesMapping();
            //var mappingItems = new List<PropertiesMappingItem>();
            //moveRecordsOption.MoveSetting.FilePropertiesMapping.PropertiesMappingItems = mappingItems;

            //moveRecordsOption.MoveSetting.FileCommonMapping = new MoveFileCommonMapping();
            //moveRecordsOption.MoveSetting.FileCommonMapping.IllegalCharReplaceMappings = new List<Contract.Explorer.IllegalCharReplaceMappingItem>();
            //try
            //{
            //    string configFilePath = AppDomain.CurrentDomain.BaseDirectory + "Configs\\FilePropertiesMapping\\FilePropertiesMapping.config";
            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(configFilePath);

            //    try
            //    {
            //        var lengthHandleNode = (XmlElement)doc.SelectSingleNode("/MoveSetting/FileMove/LengthHandle");
            //        moveRecordsOption.MoveSetting.FileCommonMapping.LengthItem = new Contract.Explorer.LengthItem()
            //        {
            //            IsCheckedMaxFileName = bool.Parse(lengthHandleNode.GetAttribute("cbMaxFileName")),
            //            IsCheckedMaxForlderName = bool.Parse(lengthHandleNode.GetAttribute("cbMaxFolderName")),
            //            MaxFileNameLength = int.Parse(lengthHandleNode.GetAttribute("MaxFileNameLen")),
            //            MaxForlderNameLength = int.Parse(lengthHandleNode.GetAttribute("MaxFolderNameLen")),
            //        };
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Warn("parse LengthHandle config error: {0}", ex.ToString());
            //    }


            //    foreach (var node in doc.SelectNodes("/MoveSetting/FileMove/IllegalReplace/Item"))
            //    {
            //        XmlElement xe = (XmlElement)node;
            //        try
            //        {
            //            moveRecordsOption.MoveSetting.FileCommonMapping.IllegalCharReplaceMappings.Add(new Contract.Explorer.IllegalCharReplaceMappingItem()
            //            {
            //                IllegalChar = xe.GetAttribute("IllegalChar"),
            //                ReplaceChar = xe.GetAttribute("ReplaceChar"),
            //                Type = int.Parse(xe.GetAttribute("type")),
            //            });
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.Warn("parse config item error: {0}", xe.OuterXml);
            //        }
            //    }

            //    foreach (var node in doc.SelectNodes("/MoveSetting/PropertiesMapping/MappingItem"))
            //    {
            //        XmlElement xe = (XmlElement)node;
            //        try
            //        {
            //            var isInclude = false;
            //            if (!bool.TryParse(xe.GetAttribute("include"), out isInclude) || !isInclude)
            //            {
            //                continue;
            //            }
            //            mappingItems.Add(new PropertiesMappingItem()
            //            {
            //                FileSystemProperty = xe.GetAttribute("fsPropertiesTxt"),
            //                SharePointProperty = xe.GetAttribute("spProperties"),

            //                //fsProperties = xe.GetAttribute("fsProperties"),
            //                ColumnType = StringConvertToEnum(xe.GetAttribute("spColumnType"))
            //            });
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.Warn("parse config item error: {0}", xe.OuterXml);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.Warn("Get File Properties Mapping config file error {0}", ex.ToString());
            //}
            #endregion
            return moveRecordsOption;
        }

        private RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        private ChangeTermOption GetChangeTermOption(ChangeTermDto changeTermInfo)
        {
            ChangeTermOption ChangeTermOption = new ChangeTermOption()
            {
                SourceRecordIds = changeTermInfo.RecordIds,
                SourceFSRecordIds = changeTermInfo.FSRecordIds,
                SourceEXORecordIds = changeTermInfo.EXORecordIds,
                SourcePhyRecordIds = changeTermInfo.PhyRecordIds,
                SourceSPOnPremRecordIds = changeTermInfo.SPOnPremRecordIds,
                SourceOneDriveRecordIds = changeTermInfo.OneDriveRecordIds,
                GoogleDriveRecordIds = changeTermInfo.GoogleDriveRecordIds,
                SourceTeamsRecordIds = changeTermInfo.TeamsRecordIds,
                TargetTermId = changeTermInfo.TermInfo.Id,
                TargetTermName = changeTermInfo.TermInfo.Name,
                TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId,
                OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
                ReclassifySubFiles = changeTermInfo.ReclassifySubFiles,
                LogonUser = WebUtil.LogOnUserName,
                Comment = changeTermInfo.Comment
            };
            return ChangeTermOption;
        }

        private ChangeLabelOption GetChangeLabelOption(ChangeLabelDto changeLabelInfo)
        {
            ChangeLabelOption ChangeLabelOption = new ChangeLabelOption()
            {
                GoogleDriveRecordIds = changeLabelInfo.GoogleDriveRecordIds,
                TargetLabelId = changeLabelInfo.LabelInfo.LabelId,
                TargetLabelName = changeLabelInfo.LabelInfo.LabelName,
                TargetLabelUniqueId = Guid.Parse(changeLabelInfo.LabelInfo.UniqueLabelId),
                OverWriteSubFiles = changeLabelInfo.OverWriteSubFiles,
                ReclassifySubFiles = changeLabelInfo.ReclassifySubFiles,
                LogonUser = WebUtil.LogOnUserName,
                Comment = changeLabelInfo.Comment
            };
            return ChangeLabelOption;
        }

        [HttpGet]
        public ExportTermsWithRulesStatus CheckSearchExportStatus(Guid exportUniqueId)
        {
            return TaxonomyService.CheckExportStatus(exportUniqueId);
        }
    }
}