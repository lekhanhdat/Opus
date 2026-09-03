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
using AngleSharp.Io;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.TermManagement.AuditHandler;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Contract.CloudService;
using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMRuleManageMent;
using System.Text;
using AvePoint.RA.RADataBroker;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.RAExchange.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.Contract.SharePoint.OnPrem;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.DB.SecurityTrimming;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Web;
using AvePoint.GCommon.Utility;
using System.Threading.Tasks;
using System.Threading;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.JPMC;
using System.Data.SqlTypes;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Label;
using RAExportCommon;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.Label;
using RATeams;
using AvePoint.RAI.Core.Models;
using RAChatCenter.Services;
using RAChatCenter.ChatCompletion;
using RAChatCenter.PromtUtil;
using System.Net.Http.Json;
using SkiaSharp;
using Aspose.Pdf.Operators;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SemanticKernel;
using LiteDB.Engine;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.GCommon.Contract.CommonFilter;
using RAGoogle.Util;
using AvePoint.RA.VectorDataCenter.Storage;
using AvePoint.RA.SharePoint.Common;
using RAArchiverCommon.Utility;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Common;

namespace AvePoint.RA.Service.TermManagement
{
    [Audit]
    public class TaxonomyService : RMServiceBase, ITaxonomyService
    {
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private ITermSetMembershipDao TermSetMembershipDao => PlatformWindsorManager.GetService<ITermSetMembershipDao>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private IRMExportTermsWithRulesDao RMExportTermsWithRulesDao => PlatformWindsorManager.GetService<IRMExportTermsWithRulesDao>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();
        private IRMChangeClassificationDao ChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        //public IMArchiverService ArchiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
        private RALogger logger = RALogger.GetInstance(typeof(TaxonomyService));
        private const double Difference = 1e-6;

        private BaseJobDto baseJobDto;
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao => PlatformWindsorManager.GetService<ISharePointOnPremiseSettingDao>();
        private IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();
        private IPhysicalRecordSettingDao PhysicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IAzureFileShareSettingDao AzureFileShareSettingDao => PlatformWindsorManager.GetService<IAzureFileShareSettingDao>();
        private IBoxSettingDao BoxSettingDao => PlatformWindsorManager.GetService<IBoxSettingDao>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private IRMSecurityTrimmingHelper RMSecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMStorageDeviceInfoDao StorageDeviceInfoDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();

        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private IFeatureUsageLimitDao FeatureUsageLimitDao => PlatformWindsorManager.GetService<IFeatureUsageLimitDao>();
        private static readonly string[] CountryCodeFields = { "[CountryCode]", "CountryCode" };
        public List<TermInfoWithRule> termInfos = new List<TermInfoWithRule>();
        public List<string> ruleIds = new List<string>();
        public List<RMRuleInfos> ruleInfos = new List<RMRuleInfos>();

        private readonly string ENVIRONMENT_NAME = "21V China North";

        private const int ErrorCode_AdvanceSettings_JsonFormat = 11;
        //private IMArchiverService mArchiverService;
        //public IMArchiverService ArchiverService
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (mArchiverService == null)
        //            {
        //                mArchiverService = DocAveServiceHelper.CreateServiceClient<IMArchiverService>();
        //            }
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return mArchiverService;
        //    }
        //}

        private Task<GeneralSettingModel> GeneralSetting
        {
            get
            {
                return GeneralSettingService.GetGeneralSettingAsync(); 
            }
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.DownloadTemplate, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public string GetTemplateFilePath()
        {
            return Path.Combine(WebUtil.GetInstallPath(), "Config", "File Plan Import Template.xlsx");
        }
        /// <summary>
        /// 获取tree children nodes
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="treeNodeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns>Jason字符串</returns> 
        [RACodeReview("Allen Yin")]
        public async Task<string> GetTaxonomyTreeDataAsync(string typeName, string treeNodeId, int pageIndex, int pageCount, List<RMSPTreeNode> spTreeNodes, int SettingType, FilterTermObjOption filterOption)
        {
            Trace.TraceError("begin to get term tree.");
            logger.Debug(string.Format("type:[{0}],nodeId:[{1}],pageIndex:[{2}],pageCount:[{3}]", typeName, treeNodeId, pageIndex, pageCount));

            try
            {
                string strResult = string.Empty;
                if (filterOption.userAndGroupUserIds == null)
                {
                    filterOption.userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                }
                switch (typeName)
                {
                    case "TermGroup":
                        if (filterOption.NeedCheckPermission)
                        {
                            filterOption.NeedCheckPermission = true;
                            strResult = GetJsonStrByObj(TermSetDao.GetTermSetsByGroupId(Guid.Parse(treeNodeId), DB.Model.TermSetType.Business, pageIndex, pageCount, filterOption));
                        }
                        else
                        {
                            strResult = GetJsonStrByObj(TermSetDao.GetTermSetsByGroupId(Guid.Parse(treeNodeId), DB.Model.TermSetType.Business, pageIndex, pageCount));
                        }
                        break;
                    case "TermSet":
                        strResult = GetJsonStrByObj(TermDao.GetTermFromTermSet(Convert.ToInt32(treeNodeId), pageIndex, pageCount));
                        break;
                    case "Term":
                        strResult = GetJsonStrByObj(TermDao.GetTermFromParentTerm(Convert.ToInt32(treeNodeId), pageIndex, pageCount));
                        break;
                    default:
                        strResult = GetJsonStrByObj(await GetTermGroupsAsync(spTreeNodes, SettingType, pageIndex, pageCount, filterOption));
                        break;
                }
                return strResult;
            }
            catch (Exception e)
            {
                logger.Error($"GetTaxonomyTreeData Exception: {e}");
                return string.Empty;
            }
        }

        public async Task<string> GetTaxonomyGoogleTermTreeDataAsync(FilterTermObjOption filterOption, int pageIndex, int pageCount)
        {
            try
            {
                List<Guid> groupUniqueIds = new List<Guid>();
                SecurityTermPermissionDto termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new QuerySecurityTermObjDto
                {
                    UserId = TenantLocalValue.LogonUserId,
                    Level = SecurityTermLevel.TermGroup,
                    FilterByContentSource = filterOption.FilterByContentSource,
                    ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                    SourceFlag = SourceFlag.Google
                });
                if (termPermissionInfo.TermPermissionType == TermPermissionMethod.All)
                {
                    return JsonConvert.SerializeObject(await TermGroupDao.LoadGoogleGroupsData([],[], filterOption, pageIndex, pageCount));
                }
                groupUniqueIds = termPermissionInfo.TermObjIds;
                List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                return JsonConvert.SerializeObject(await TermGroupDao.LoadGoogleGroupsData(groupUniqueIds, userAndGroupUserIds, filterOption , pageIndex, pageCount));
            }
            catch (Exception e)
            {
                logger.Error($"Get Google Term Tree Data Exception: {e}");
                return string.Empty;
            }
        }

        public async Task<string> GetTaxonomyAllGoogleTermTreeDataAsync(FilterTermObjOption filterOption, int pageIndex, int pageCount)
        {
            try
            {
                return JsonConvert.SerializeObject(await TermGroupDao.LoadGoogleGroupsData([], [], filterOption, pageIndex, pageCount));
            }
            catch (Exception e)
            {
                logger.Error($"Get Google Term Tree Data Exception: {e}");
                return string.Empty;
            }
        }

        public async Task<string> LoadTermGroupsAsync(FilterTermObjOption filterOption)
        {
            if (filterOption.NeedCheckPermission)
            {
                List<Guid> groupUniqueIds = new List<Guid>();
                SecurityTermPermissionDto termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new QuerySecurityTermObjDto
                {
                    UserId = TenantLocalValue.LogonUserId,
                    Level = SecurityTermLevel.TermGroup,
                    FilterByContentSource = filterOption.NeedCheckPermission,
                    ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                    ContainerId = filterOption.ContainerId,
                    SourceFlag = filterOption.SourceFlag
                });
                if (termPermissionInfo.TermPermissionType == TermPermissionMethod.All)
                {
                    return GetJsonStrByObj(TermGroupDao.LoadGroupsData(false));
                }
                else
                {
                    groupUniqueIds = termPermissionInfo.TermObjIds;
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    return GetJsonStrByObj(TermGroupDao.LoadGroupsData(false, groupUniqueIds, userAndGroupUserIds, filterOption));
                }
            }
            return GetJsonStrByObj(TermGroupDao.LoadGroupsData(false));
        }

        public async Task<string> LoadClassCodeGroupsAsync(FilterTermObjOption filterOption, Guid termSetId, string searchTerm = null, int pageIndex = 0, int pageSize = 0)
        {
            try
            {
                if (termSetId == Guid.Empty)
                {
                    logger.Warn("LoadClassCodeGroupsAsync: termSetId is empty, cannot resolve class code term group.");
                    return GetJsonStrByObj(new List<RMTermGroup>());
                }

                RMTermSet termSet = TermSetDao.GetRMTermSetByGuid(termSetId);
                if (termSet == null)
                {
                    logger.Warn("LoadClassCodeGroupsAsync: TermSet not found for TermSetId: {0}", termSetId);
                    return GetJsonStrByObj(new List<RMTermGroup>());
                }

                if (filterOption.NeedCheckPermission && filterOption.userAndGroupUserIds == null)
                {
                    filterOption.userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                }

                if (filterOption.NeedCheckPermission)
                {
                    var isPermitted = await ValidateTermGroupsPermissionAsync(new List<Guid> { termSet.TermGroupId }, filterOption);
                    if (!isPermitted)
                    {
                        logger.Warn("LoadClassCodeGroupsAsync: User has no permission to term group: {0}", termSet.TermGroupId);
                        return GetJsonStrByObj(new { NoTermPermission = true });
                    }
                }

                RMTermGroup termGroup = TermGroupDao.LoadTermDataById(termSet.TermGroupId, true, filterOption);
                if (termGroup == null || termGroup.IsRemoved)
                {
                    logger.Warn("LoadClassCodeGroupsAsync: TermGroup not found or removed for TermGroupId: {0}", termSet.TermGroupId);
                    return GetJsonStrByObj(new List<RMTermGroup>());
                }

                List<RMTerm> terms = TermDao.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    terms = terms
                        .Where(t => t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList(); 
                }

                int totalCount = terms.Count;
                List<RMTerm> pagedTerms = pageSize > 0
                    ? terms.Skip(pageIndex * pageSize).Take(pageSize).ToList()
                    : terms;

                int totalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;

                termSet.subTerms = pagedTerms;
                termSet.subTermCount = totalCount;
                termGroup.subTerms = new List<RMTermSet> { termSet };
                termGroup.subTermCount = 1;

                return GetJsonStrByObj(new
                {
                    Data = new List<RMTermGroup> { termGroup },
                    TotalCount = totalCount,
                    TotalPage = totalPage
                });
            }
            catch (Exception ex)
            {
                logger.Error("LoadClassCodeGroupsAsync error: {0}", ex.ToString());
                return GetJsonStrByObj(new List<RMTermGroup>());
            }
        }

        /// <summary>
        /// 获取tree children nodes
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="treeNodeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns>Jason字符串</returns> 
        public async Task<List<RMTermInfo>> GetTaxonomyTreeDataAsync(RMTermType typeName, string treeNodeId, int pageIndex, int pageCount)
        {
            Trace.TraceError("api begin to get term tree.");
            logger.Debug(string.Format("type:[{0}],nodeId:[{1}],pageIndex:[{2}],pageCount:[{3}]", typeName, treeNodeId, pageIndex, pageCount));
            List<RMTermInfo> allTerms = new List<RMTermInfo>();
            string strResult = string.Empty;
            switch (typeName)
            {
                case RMTermType.TermGroup:
                    var termSets = await TermSetDao.LoadTermSetAsync(DB.Model.TermSetType.Business, Guid.Parse(treeNodeId));
                    allTerms = termSets.ConvertAll(t => Convert2RMTermDto(t));
                    break;
                case RMTermType.TermSet:
                    var terms = TermDao.GetTermFromTermSet(Convert.ToInt32(treeNodeId), pageIndex, pageCount);
                    allTerms = terms.ConvertAll(t => Convert2RMTermDto(t));
                    break;
                case RMTermType.Term:
                    var subTerms = TermDao.GetTermFromParentTerm(Convert.ToInt32(treeNodeId), pageIndex, pageCount);
                    allTerms = subTerms.ConvertAll(t => Convert2RMTermDto(t));
                    break;
                default:

                    var groups = TermGroupDao.LoadTermGroup(false);
                    allTerms = groups.ConvertAll(t => Convert2RMTermDto(t));
                    break;
            }
            return allTerms;
        }

        private RMTermInfo Convert2RMTermDto(RMTerm term)
        {
            return new RMTermInfo()
            {
                Id = term.Id,
                UniqueId = term.UniqueId,
                Name = term.Name,
                Type = RMTermType.Term
            };
        }
        private RMTermInfo Convert2RMTermDto(RMTermGroup term)
        {
            return new RMTermInfo()
            {
                Id = term.Id,
                UniqueId = term.UniqueId,
                Name = term.Name,
                Type = RMTermType.TermGroup
            };
        }
        private RMTermInfo Convert2RMTermDto(RMTermSet term)
        {
            return new RMTermInfo()
            {
                Id = term.Id,
                UniqueId = term.UniqueId,
                Name = term.Name,
                Type = RMTermType.TermSet
            };
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.CreateTermGroup, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> CreateTermGroupAsync(string termGroupName)
        {
            string strResult = string.Empty;
            try
            {
                ValideNameLen(termGroupName);
                strResult = GetJsonStrByObj(TermGroupDao.CreateTermGroupByName(termGroupName));
                await RefreshTermPermissionCacheAsync();
            }
            catch
            {
                return string.Empty;
            }
            return strResult;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.CreateTermGroup, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> CreateTermGroupAsync(TermInfo termInfo)
        {
            if (termInfo.TermGroupUniqueId == Guid.Empty)
            {
                return await CreateTermGroupAsync(termInfo.TermGroupName);
            }

            try
            {
                ValideNameLen(termInfo.TermGroupName);
                TermGroupDao.CreateTermGroupById(
                    termInfo.TermGroupUniqueId,
                    termInfo.TermGroupName,
                    termInfo.Description ?? string.Empty,
                    termInfo.UsingMMSSpecified);
                await RefreshTermPermissionCacheAsync();
                return JsonConvert.SerializeObject(TermGroupDao.GetTermGroupByGuid(termInfo.TermGroupUniqueId));
            }
            catch
            {
                throw;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.CreateTermSet, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> CreateTermSetAsync(string name, Guid termGroupId)
        {
            string strResult = string.Empty;
            try
            {
                ValideNameLen(name);
                strResult = GetJsonStrByObj(TermSetDao.CreateTermSet(name, termGroupId));
                await RefreshTermPermissionCacheAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Equals(CreateTermSetErrorType.IsExists.ToDescription()))
                {
                    strResult = ((int)CreateTermSetErrorType.IsExists).ToString();
                }
                else if (ex.Message.ToString().Equals(CreateTermSetErrorType.HasSame.ToDescription()))
                {
                    strResult = ((int)CreateTermSetErrorType.HasSame).ToString();
                }
                else
                {
                    strResult = string.Empty;
                }
            }
            return strResult;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.CreateTermSet, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> CreateTermSetAsync(TermInfo termInfo)
        {
            if (termInfo.TermSetUniqueId == Guid.Empty)
            {
                return await CreateTermSetAsync(termInfo.TermSetName, termInfo.TermGroupUniqueId);
            }

            try
            {
                ValideNameLen(termInfo.TermSetName);
                TermSetDao.CreateTermSetByUniqueId(
                    termInfo.TermSetUniqueId,
                    termInfo.TermSetName,
                    termInfo.Description ?? string.Empty,
                    termInfo.TermGroupUniqueId);
                await RefreshTermPermissionCacheAsync();
                return JsonConvert.SerializeObject(TermSetDao.GetRMTermSetByGuid(termInfo.TermSetUniqueId));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 创建新的Term
        /// </summary>
        /// <param name="termName"></param>
        /// <param name="description"></param>
        /// <param name="parentTermId"></param>
        /// <param name="termSetId"></param>
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.CreateTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> CreateTermAsync(TermInfo dto)
        {
            try
            {
                ValideNameLen(dto.TermName);
                if (dto.TermUniqueId != Guid.Empty)
                {
                    var createdTerm = TermDao.CreateTermForImport(
                        dto.TermName,
                        dto.ParentTermId,
                        dto.TermSetId,
                        false,
                        dto.TermUniqueId,
                        dto.Description);
                    await RefreshTermPermissionCacheAsync();
                    return JsonConvert.SerializeObject(createdTerm);
                }

                await RefreshTermPermissionCacheAsync();
                return GetJsonStrByObj(TermDao.CreateTerm(dto));
            }
            catch(Exception ex)
            {
                logger.Error("An error occured while create Term,{0}", ex);
                return "1";
            }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termLabel"></param>
        /// <returns></returns>
        public async Task<string> SearchAsync(int termSetId, string termLabel, Guid termGroupId, bool withRuleName = false)
        {
            var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            FilterTermObjOption filterTermObjOption = new FilterTermObjOption();
            filterTermObjOption.NeedCheckPermission = true;
            filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
            return GetJsonStrByObj(TermDao.GetRMTermsBySearch(termLabel, termGroupId, withRuleName, filterTermObjOption));
        }

        public async Task<string> SearchAsync(int termSetId, string termLabel, Guid termGroupId, string containerId, SourceFlag sourceFlag)
        {
            var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            FilterTermObjOption filterTermObjOption = new FilterTermObjOption();
            filterTermObjOption.NeedCheckPermission = true;
            filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
            filterTermObjOption.FilterByContentSource = true;
            filterTermObjOption.ContainerId = containerId;
            filterTermObjOption.SourceFlag = sourceFlag;
            filterTermObjOption.ExcludeBuiltIn = true;
            return GetJsonStrByObj(TermDao.GetRMTermsBySearch(termLabel, termGroupId, false, filterTermObjOption));
        }

        /*private Guid GetSavedTreeTermSetId(string agentGroupId, SourceFlag sourceFlag)
        {
            Guid? result = null;
            if (sourceFlag == SourceFlag.SharePoint)
            {
                result = SharePointSettingDao.GetSettingInfoByAgentGroupId(agentGroupId)?.TermSetId;
            }
            else if (sourceFlag == SourceFlag.OneDrive)
            {
                result = OneDriveSettingDao.GetSettingInfoByAgentGroupId(agentGroupId)?.TermSetId;
            }
            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return Guid.Empty;
            }
        }*/

        public async Task<string> CommonGetSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, Func<CurrentSettingsInfo, Guid> getTermSetdelegate, SourceFlag sourceFlag, bool needCheckPermission = false)
        {
            ArgumentCheck.NotNull(settingInfo, nameof(settingInfo));
            //get term group
            RMTermGroup termGroup = new RMTermGroup();
            List<RMTermSet> termSets = new List<RMTermSet>();
            RMTermSet curTermSet = null;
            RMTerm curTerm = null;
            Stack<int> allTermIds = new Stack<int>();
            Guid curTermGuid = Guid.Empty;
            if (settingInfo != null && !string.IsNullOrEmpty(settingInfo.CurrentNodeId)) {
                curTermGuid = new Guid(settingInfo.CurrentNodeId);
            }

            if (curTermGuid != Guid.Empty)
            {
                curTerm = TermDao.GetRMTermByUniqueId(curTermGuid);

                int[] curTermId = { curTerm.Id };
                //get all parent termid /termset id
                string[] allIds = TermSetMembershipDao.GetRMTermSetMemberships(curTermId, true).First().Path.Split('/').Reverse().ToArray();
                //最后一个是TermSetId,过滤掉在后面单独处理
                for (int i = 0; i < allIds.Length - 1; i++)
                {
                    allTermIds.Push(Convert.ToInt32(allIds[i]));
                }
                int termSetId = Convert.ToInt32(allIds.Last());
                curTermSet = TermSetDao.GetRMTermSet(termSetId);
            }
            else
            {
                curTermSet = TermSetDao.GetRMTermSetByGuid(new Guid(settingInfo.TermSetId));
            }
            
            termGroup = TermGroupDao.GetTermGroupByGuid(curTermSet.TermGroupId);
            var filterOption = new FilterTermObjOption { NeedCheckPermission = false };
            if (needCheckPermission)
            {
                filterOption.NeedCheckPermission = needCheckPermission;
                filterOption.userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            }
            filterOption.FilterByContentSource = true;
            filterOption.ExcludeBuiltIn = true;
            if (settingInfo.spTreeNodes != null && settingInfo.spTreeNodes.Count > 0)
            {
                filterOption.ContainerId = settingInfo.spTreeNodes[0].SPObjectId;
            }
            else
            {
                filterOption.ContainerId = settingInfo.GroupId;
            }
            filterOption.SourceFlag = sourceFlag;

            bool selectedTermScopeNoPermission = false;
            if (string.IsNullOrEmpty(settingInfo.TermSetId))
            {
                //回显Container Level Term时，参数中没有TermSetId，也不需要进行此验证
                selectedTermScopeNoPermission = false;
            }
            else
            {
                selectedTermScopeNoPermission = !(await ValidateTermSetsPermissionAsync(new List<Guid> { new Guid(settingInfo.TermSetId) }, filterOption));
            }

            termSets = GetSavedTermSetsWithPageInfo(curTermSet.TermGroupId, curTermSet.Id, settingInfo.perPageCount, out int totalCount, filterOption, settingInfo.CurrentNodeId == Guid.Empty.ToString());
            foreach (var termSet in termSets)
            {
                termSet.subTermCount = TermSetMembershipDao.GetSubTermMembershipsByTermSetId(termSet.Id).Count();
                if (curTerm != null && termSet.Id == curTermSet.Id && !curTerm.IsRemoved)
                {
                    termSet.subTerms = GetSavedSubTermWithPageInfo(curTermSet.Id, "TermSet", allTermIds, settingInfo.perPageCount, curTermSet.subTermCount, allTermIds.LastOrDefault());
                }
            }
            termGroup.subTerms = termSets;
            termGroup.subTermCount = totalCount;

            if (settingInfo.SettingType == 1)
            {
                var termSetId = getTermSetdelegate(settingInfo);
                var oldtermGroup = GetBusinessTermGroupByDefaultTermSetId(termSetId, filterOption);
                if (oldtermGroup == null || !(await ValidateTermGroupsPermissionAsync(new List<Guid> { oldtermGroup.UniqueId }, filterOption)))
                {
                    return GetJsonStrByObj(new { NoTermPermission = true });
                }
                if (oldtermGroup.Id != termGroup.Id)
                {
                    return GetJsonStrByObj(new {
                        TermGroup = oldtermGroup,
                        IsChangeAnotherTermGroup = true,
                        SelectedTermScopeNoPermission = selectedTermScopeNoPermission
                    });
                }

                if (termGroup.IsRemoved)
                {
                    return "";
                }
                if (!(await ValidateTermGroupsPermissionAsync(new List<Guid> { termGroup.UniqueId }, filterOption)))
                {
                    return GetJsonStrByObj(new { NoTermPermission = true });
                }
                return GetJsonStrByObj(new {
                    TermGroup = termGroup,
                    IsChangeAnotherTermGroup = false,
                    SelectedTermScopeNoPermission = selectedTermScopeNoPermission
                });
            }
            else
            {
                //string agentGroupId = settingInfo.spTreeNodes[0].SPObjectId;
                //RMSharePointSetting setting = SharePointSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
                var termGroups = TermGroupDao.LoadTermGroup(false, filterOption);
                if (termGroups != null && termGroups.Count > 0)
                {
                    for (int i = 0; i < termGroups.Count; i++)
                    {
                        if (termGroups[i].UniqueId.Equals(termGroup.UniqueId))
                        {
                            termGroups[i] = termGroup;
                            break;
                        }
                    }
                }
                else
                {
                    //默认TermGroup不能删除，走到此逻辑只能是当前User没有任何Term权限。
                    return GetJsonStrByObj(new {
                        NoTermPermission = true
                    });
                }
                if (selectedTermScopeNoPermission)
                {
                    return GetJsonStrByObj(new
                    {
                        TermGroups = termGroups,
                        SelectedTermScopeNoPermission = true
                    });
                }
                else
                {
                    return GetJsonStrByObj(termGroups);
                }
            }
            //finally we get a tree from term group to selected term.
            //return GetJsonStrByObj(termGroup);
        }
        /// <summary>
        /// 此处算法：保存的只是一个展开的并被选择的节点，其他展开的节点不被保存。根据此节点反查各层父级节点。展示出tree。展示tree的过程中会
        /// 把被选中的节点的兄弟节点显示出来
        /// </summary>
        /// <param name="settingInfo"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        public Task<string> GetSPSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetSPTermSetId, SourceFlag.SharePoint, needCheckPermission);
        }

        public Task<string> GetOneDriveSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetOneDriveTermSetId, SourceFlag.OneDrive, needCheckPermission);
        }

        public Task<string> GetTeamsSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetTeamsTermSetId, SourceFlag.Teams, needCheckPermission);
        }

        private RMTermGroup GetTermGroupByDefaultTermSetId(Guid termSetId)
        {
            RMTermSet termSet = TermSetDao.GetRMTermSetByGuid(termSetId);
            if (termSet == null)
            {
                return null;
            }
            RMTermGroup termGroup = TermGroupDao.LoadTermDataById(termSet.TermGroupId);
            return termGroup;
        }
        private RMTermGroup GetBusinessTermGroupByDefaultTermSetId(Guid termSetId, FilterTermObjOption filterOption = null)
        {
            RMTermSet termSet = TermSetDao.GetRMTermSetByGuid(termSetId);
            try
            {
                RMTermGroup termGroup = TermGroupDao.LoadTermDataById(termSet.TermGroupId, true, filterOption);
                return termGroup;
            }
            catch (Exception e)
            {
                logger.Warn("TermGroupId is error, error: {0}", e.ToString());
                return null;
            }
        }
        /*private async Task<List<RMTermGroup>> GetTermGroupsByAgentGroupIdAsync(string id)
        {
            List<RMTermGroup> rmTermGroups = new List<RMTermGroup>();
            List<RMSPTreeNode> registeredSites = SPSettingTreeService.LoadFarm();
            var defaultGroups = await SPSettingTreeService.BrowseAsync(registeredSites[0]);
            List<RMSPTreeNode> browserSites = new List<RMSPTreeNode>();
            var allTermGroups = TermGroupDao.LoadTermGroup();

            var groupResult = defaultGroups.Where(dg => dg.SPObjectId.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (groupResult != null)
            {
                var groupInfos = TermGroupMembershipDao.GetTermGroupsByAgentGroupId(groupResult.SPObjectId);
                if (groupInfos == null || groupInfos.Count == 0)
                {
                    //var tempTermGroups = TermGroupDao.LoadTermData();
                    //if (tempTermGroups != null && tempTermGroups.Count == 1 && !tempTermGroups[0].UsingMMSSpecified)
                    //{
                    //    rmTermGroups = tempTermGroups;
                    //}
                    var allTermGroupMembers = TermGroupMembershipDao.GetAllTermGroupMembership();
                    foreach (var group in allTermGroups)
                    {
                        var isExist = allTermGroupMembers.AsQueryable().Where(g => g.TermGroupId.ToString().Equals(group.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (isExist == null && !group.IsRemoved)
                        {
                            rmTermGroups.Add(group);
                        }
                    }
                }
                else
                {
                    foreach (var group in allTermGroups)
                    {
                        var otherGroups = TermGroupMembershipDao.GetOtherGroupsByAgentGroupIdAndTermGroupId(groupResult.SPObjectId, group.UniqueId);
                        var myHasGroup = groupInfos.AsQueryable().Where(g => g.AgentGroupId.Equals(groupResult.SPObjectId) && g.TermGroupId.Equals(group.UniqueId)).FirstOrDefault();
                        if ((otherGroups == null || otherGroups.Count == 0) || myHasGroup != null)
                        {
                            var isExist = rmTermGroups.AsQueryable().Where(g => g.UniqueId.ToString().Equals(group.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (isExist == null && !group.IsRemoved)
                            {
                                var termGroup = TermGroupDao.LoadTermDataById(group.UniqueId);
                                rmTermGroups.Add(termGroup);
                            }
                        }
                    }
                }
            }
            return rmTermGroups;
        }*/
        private async Task<List<RMTermGroup>> GetTermGroupsAsync(List<RMSPTreeNode> spTreeNodes, int SettingType, int pageIndex, int pageSize, FilterTermObjOption filterOption)
        {
            List<RMTermGroup> rmTermGroups = new List<RMTermGroup>();
            if (spTreeNodes != null && spTreeNodes.Count > 0)
            {
                if (SettingType == 1)
                {
                    string id = spTreeNodes[0].SPObjectId;
                    var termSetId = Guid.Empty;
                    if (filterOption.SourceFlag == SourceFlag.Teams)
                    {
                        RMTeamsSetting setting = TeamsSettingDao.GetSettingInfoByAgentGroupId(id);
                        termSetId = setting.TermSetId;
                    }
                    else
                    {
                        RMSharePointSetting setting = SharePointSettingDao.GetSettingInfoByAgentGroupId(id);
                        termSetId = setting.TermSetId;
                    }
                    var termGroup = GetTermGroupByDefaultTermSetId(termSetId);
                    //添加TermSet权限判断
                    if (termGroup != null && !termGroup.IsRemoved && (await ValidateTermSetsPermissionAsync(new List<Guid> { termSetId }, filterOption)))
                    {
                        rmTermGroups.Add(termGroup);
                    }
                    if (pageSize <= 0)
                    {
                        return rmTermGroups;
                    }
                    else
                    {
                        var data = rmTermGroups.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                        return data;
                    }
                }
                if (filterOption.NeedCheckPermission)
                {
                    rmTermGroups = TermGroupDao.LoadTermGroup(false, filterOption);
                }
                else
                {
                    rmTermGroups = TermGroupDao.LoadTermGroup(false);
                }
            }
            else
            {
                if (filterOption.NeedCheckPermission)
                {
                    List<Guid> groupUniqueIds = new List<Guid>();
                    SecurityTermPermissionDto termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new QuerySecurityTermObjDto
                    {
                        UserId = TenantLocalValue.LogonUserId,
                        Level = SecurityTermLevel.TermGroup,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag,
                        ForPhysicalView = filterOption.ForPhysicalView
                    });
                    if (termPermissionInfo.TermPermissionType == TermPermissionMethod.All)
                    {
                        rmTermGroups = TermGroupDao.LoadGroupsData(pageIndex: pageIndex, pageSize: pageSize);
                    }
                    else
                    {
                        groupUniqueIds = termPermissionInfo.TermObjIds;
                        List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        rmTermGroups = TermGroupDao.LoadGroupsData(true, groupUniqueIds, userAndGroupUserIds, filterOption, pageIndex: pageIndex, pageSize: pageSize);
                    }
                }
                else
                {
                    rmTermGroups = TermGroupDao.LoadGroupsData(pageIndex: pageIndex, pageSize: pageSize);
                }

            }

            if (pageSize <= 0)
            {
                return rmTermGroups;
            }
            else
            {
                var data = rmTermGroups.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return data;
            }
        }

        private List<RMTerm> GetSavedSubTermWithPageInfo(int parentId, string parentType, Stack<int> allTermId, int pageCount, int subTermsCount, int selectedTermId)
        {
            List<RMTerm> subTerms = new List<RMTerm>();
            List<int> termids = new List<int>();
            int selectParentTermId = 0;
            if (allTermId.Count != 0)
            {
                selectParentTermId = allTermId.Pop();
                int pageIndexTemp = 0;
                List<RMTermSetMembership> allSubTermstMembership = new List<RMTermSetMembership>();
                if (parentType == "TermSet")
                {
                    allSubTermstMembership = TermSetMembershipDao.GetSubTermMembershipsByTermSetId(parentId);
                }
                if (parentType == "Term")
                {
                    allSubTermstMembership = TermSetMembershipDao.GetSubTermMembershipByTermId(parentId);
                }
                for (int pageIndex = 0; pageIndex <= Math.Ceiling(Convert.ToDouble(subTermsCount / pageCount)); pageIndex++)
                {
                    pageIndexTemp = pageIndex + 1;

                    termids = allSubTermstMembership.OrderBy(a => a.TermName).Select(b => b.TermId).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                    if (termids.Contains(selectParentTermId))
                    {
                        break;
                    }
                }
                subTerms = TermDao.GetRMTermsByTermIds(termids.ToArray()).OrderBy(t => t.Name).ToList();
                if (subTerms.Count != 0)
                {
                    foreach (var item in subTerms)
                    {
                        item.IsChecked = item.Id == selectedTermId;
                        if (parentType != "TermSet")
                        {
                            RMTerm parentTerm = TermDao.GetRMTermByTermId(parentId);
                            TermDao.SetTermIsExpired(parentTerm, item);
                        }
                        else
                        {
                            item.IsExpired = TermDao.IsExpiredTerm(item.Id);
                        }
                        item.subTermCount = TermDao.SubTermCount(item.Id);
                    }
                    subTerms.Where(o => o.Id == selectParentTermId).First().pageIndex = pageIndexTemp;
                    int subTermCount = 0;
                    subTermCount = TermSetMembershipDao.GetSubTermMembershipByTermId(selectParentTermId).Count();
                    subTerms.Where(o => o.Id == selectParentTermId).First().subTermCount = subTermCount;
                    subTerms.Where(o => o.Id == selectParentTermId).First().subTerms = GetSavedSubTermWithPageInfo(selectParentTermId, "Term", allTermId, pageCount, subTermCount, selectedTermId);
                }
            }
            return subTerms;
        }

        private List<RMTermSet> GetSavedTermSetsWithPageInfo(Guid groupId, int selectedTermSetId, int pageCount, out int totalCount, FilterTermObjOption filterOption = null, bool needCheckedTermSet = false)
        {
            List<RMTermSet> subTermSets = new List<RMTermSet>();
            int pageIndexTemp = 0;
            var allTermSets = TermSetDao.GetRMTermSetsByGroupUniqueId(groupId, filterOption);
            if (needCheckedTermSet)
            {
                allTermSets.ForEach((termSet) =>
                {
                    if (termSet.Id == selectedTermSetId)
                    {
                        termSet.IsChecked = true;
                    }
                });
            }
            totalCount = allTermSets.Count;
            for (int pageIndex = 0; pageIndex <= Math.Ceiling(Convert.ToDouble(totalCount / pageCount)); pageIndex++)
            {
                pageIndexTemp = pageIndex + 1;
                subTermSets = allTermSets.OrderBy(a => a.Name).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                if (subTermSets.Any(o => o.Id == selectedTermSetId))
                {
                    break;
                }
            }
            var selectedTermSet = subTermSets.Where(o => o.Id == selectedTermSetId).FirstOrDefault();
            if (selectedTermSet != null)
            {
                selectedTermSet.pageIndex = pageIndexTemp;
            }
            return subTermSets;
        }

        public Task<bool> ValidateTermGroupsPermissionAsync(List<Guid> termGroupIds, FilterTermObjOption filterOption)
        {
            return ValidateTermObjPermissionAsync(termGroupIds, SecurityTermLevel.TermGroup, filterOption);
        }
        public Task<bool> ValidateTermSetsPermissionAsync(List<Guid> termSetIds, FilterTermObjOption filterOption)
        {
            return ValidateTermObjPermissionAsync(termSetIds, SecurityTermLevel.TermSet, filterOption);
        }
        private Task<bool> ValidateTermObjPermissionAsync(List<Guid> termObjIds, SecurityTermLevel level, FilterTermObjOption filterOption)
        {
            return SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, level, termObjIds, filterOption);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.DeprecateTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public string DeprecateTerm(int termId)
        {
            try
            {
                return GetJsonStrByObj(TermDao.DeprecateTerm(termId));
            }
            catch
            {
                return "";
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.EnableTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public string EnableTerm(int termId)
        {
            try
            {
                return GetJsonStrByObj(TermDao.EnableTerm(termId));
            }
            catch
            {
                return "";
            }
        }
        public string GetTermByTermId(string termId)
        {
            Guid gTermId;
            int iTermId;
            if (Guid.TryParse(termId, out gTermId))
            {
                iTermId = TermDao.GetRMTermByGuId(gTermId).Id;
            }
            else
            {
                iTermId = Convert.ToInt32(termId);
            }
            return GetJsonStrByObj(TermDao.GetRMTermByTermId(iTermId));
        }

        public string GetTermSetByTermSetId(string termSetId)
        {
            Guid gTermSetId;
            int iTermSetId;
            if(Guid.TryParse(termSetId, out gTermSetId))
            {
                iTermSetId = TermSetDao.GetRMTermSetByGuid(gTermSetId).Id;
            }
            else
            {
                iTermSetId = Convert.ToInt32(termSetId);
            }
            return GetJsonStrByObj(TermSetDao.GetRMTermSet(iTermSetId));
        }

        public string GetTermSetDescByTermSetId(string termSetId)
        {
            var ts = TermSetDao.GetRMTermSet(Convert.ToInt32(termSetId));
            if (ts != null)
            {
                return ts.Description;
            }
            return string.Empty;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RenameTerm, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> RenameTermAsync(int termId, string termName, int termSetId)
        {
            try
            {
                ValideNameLen(termName);
                return GetJsonStrByObj(await TermDao.RenameTermAsync(termId, termName, termSetId));
            }
            catch
            {
                return GetJsonStrByObj(new { message = "-1" });
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RenameTermGroup, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> RenameTermGroupAsync(int termGroupId, string termGroupName)
        {
            try
            {
                ValideNameLen(termGroupName);
                return GetJsonStrByObj(await TermGroupDao.RenameTermGroupAsync(termGroupId, termGroupName));
            }
            catch
            {
                return GetJsonStrByObj(new { message = "-1" });
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.RenameTermSet, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> RenameTermSetAsync(int termSetId, string termSetName, Guid termGroupId)
        {
            try
            {
                ValideNameLen(termSetName);
                return GetJsonStrByObj(await TermSetDao.RenameTermSetAsync(termSetId, termGroupId, termSetName));
            }
            catch
            {
                return GetJsonStrByObj(new { message = "-1" });
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.DeleteTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> DeleteTermAsync(int termId)
        {
            try
            {
                List<Guid> mlTermIds = new List<Guid>();
                List<Guid> deleteTerms = new List<Guid>();
                await TermDao.DeleteTermAsync(termId, deleteTerms);
                foreach(var deleteTerm in deleteTerms)
                {
                    var mlTerm = RMMLTermDao.GetTrainingTerm(deleteTerm);
                    if (mlTerm != null)
                    {
                        mlTermIds.Add(mlTerm.Id);
                    }
                }
                if (mlTermIds.Count > 0)
                {
                    RMMLTermDao.DeleteTerms(mlTermIds);
                    IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                    await vectorStore.DeleteVectorsByIdsAsync(mlTermIds);
                }
                return "1";
            }
            catch
            {
                return "";
            }
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.DeleteRootTerms, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public string DeleteRootTerms(int termSetId)
        {
            try
            {
                var terms = TermDao.GetActiveTermsByTermSetId(termSetId);
                List<Guid> mlTermIds = new List<Guid>();
                foreach (var term in terms)
                {
                    var mlTerm = RMMLTermDao.GetTrainingTerm(term.UniqueId);
                    if (mlTerm != null)
                    {
                        mlTermIds.Add(mlTerm.Id);
                    }
                }
                TermDao.DeleteTermByTermSetId(termSetId);
                if (mlTermIds.Count > 0)
                {
                    RMMLTermDao.MarkTermRemoveStatus(mlTermIds);
                    IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                    vectorStore.DeleteVectorsByIdsAsync(mlTermIds);
                }
                return "1";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取TreeJson数据字符串前台分页
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="treeNodeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public async Task<string> GetTaxonomyTreeDataAsync(string typeName, string treeNodeId, bool fetchDeprecated = true, bool needCheckPermission = false)
        {
            string strResult = string.Empty;
            switch (typeName)
            {
                case "TermGroup":
                    if (needCheckPermission)
                    {
                        var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        FilterTermObjOption filterTermObjOption = new FilterTermObjOption();
                        filterTermObjOption.NeedCheckPermission = true;
                        filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
                        strResult = GetJsonStrByObj(await TermSetDao.LoadTermSetAsync(DB.Model.TermSetType.Business, Guid.Parse(treeNodeId), filterTermObjOption));
                    }
                    else
                    {
                        strResult = GetJsonStrByObj(await TermSetDao.LoadTermSetAsync(DB.Model.TermSetType.Business, Guid.Parse(treeNodeId)));
                    }
                    break;
                case "TermSet":
                    List<RMTerm> terms = TermDao.GetTermFromTermSetWithoutDeletedTerm(Convert.ToInt32(treeNodeId));
                    if (!fetchDeprecated)
                    {
                        terms = GetFilterTerms(terms);
                    }
                    strResult = GetJsonStrByObj(terms);

                    break;
                case "Term":
                    List<RMTerm> subTerms = TermDao.GetTermFromParentTermWithoutDeletedTerm(Convert.ToInt32(treeNodeId));
                    if (!fetchDeprecated)
                    {
                        subTerms = GetFilterTerms(subTerms);
                    }
                    strResult = GetJsonStrByObj(subTerms);
                    break;
                default:
                    if (needCheckPermission)
                    {
                        var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        FilterTermObjOption filterTermObjOption = new FilterTermObjOption();
                        filterTermObjOption.NeedCheckPermission = true;
                        filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
                        strResult = GetJsonStrByObj(TermGroupDao.LoadTermGroup(false, filterTermObjOption));
                    }
                    else
                    {
                        strResult = GetJsonStrByObj(TermGroupDao.LoadTermGroup(false));
                    }
                    break;
            }
            return strResult;
        }

        public async Task<string> GetTaxonomyTreeDataAsync(string typeName, string treeNodeId, FilterTermObjOption filterTermObjOption, bool fetchDeprecated = true)
        {
            string strResult = string.Empty;
            switch (typeName)
            {
                case "TermGroup":
                    if (filterTermObjOption.NeedCheckPermission)
                    {
                        var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
                        strResult = GetJsonStrByObj(await TermSetDao.LoadTermSetAsync(DB.Model.TermSetType.Business, Guid.Parse(treeNodeId), filterTermObjOption));
                    }
                    else
                    {
                        strResult = GetJsonStrByObj(await TermSetDao.LoadTermSetAsync(DB.Model.TermSetType.Business, Guid.Parse(treeNodeId)));
                    }
                    break;
                case "TermSet":
                    List<RMTerm> terms = TermDao.GetTermFromTermSetWithoutDeletedTerm(Convert.ToInt32(treeNodeId));
                    if (!fetchDeprecated)
                    {
                        terms = GetFilterTerms(terms);
                    }
                    strResult = GetJsonStrByObj(terms);

                    break;
                case "Term":
                    List<RMTerm> subTerms = TermDao.GetTermFromParentTermWithoutDeletedTerm(Convert.ToInt32(treeNodeId));
                    if (!fetchDeprecated)
                    {
                        subTerms = GetFilterTerms(subTerms);
                    }
                    strResult = GetJsonStrByObj(subTerms);
                    break;
                default:
                    if (filterTermObjOption.NeedCheckPermission)
                    {
                        var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
                        strResult = GetJsonStrByObj(TermGroupDao.LoadTermGroup(false, filterTermObjOption));
                    }
                    else
                    {
                        strResult = GetJsonStrByObj(TermGroupDao.LoadTermGroup(false));
                    }
                    break;
            }
            return strResult;
        }

       /* private List<RMTermSet> ReplaceLocationSetName(List<RMTermSet> locationSets)
        {
            if (locationSets != null && locationSets.Count > 0)
            {
                locationSets[0].Name = I18NEntity.GetString("RM_LM_LocationSet");
            }
            return locationSets;
        }*/

        public TermGroupAuditInfo GetTermGroupInfoById(int termGroupId)
        {
            TermGroupAuditInfo termGroupAuditInfo = new TermGroupAuditInfo();
            RMTermGroup termGroup = TermGroupDao.GetRMTermGruop(termGroupId);
            termGroupAuditInfo.Id = termGroup.Id;
            termGroupAuditInfo.Description = termGroup.Description;
            termGroupAuditInfo.GoogleTermSyncOption = termGroup.GoogleTermSyncOption.ToString();
            termGroupAuditInfo.M365TermSyncOption = termGroup.M365TermSyncOption.ToString();
            termGroupAuditInfo.UniqueId = termGroup.UniqueId;
            List<RMSiteInfo> siteInfos = GetRelativedSiteMMSInfo(termGroup.UniqueId);
            switch (termGroupAuditInfo.M365TermSyncOption)
            {
                case "Specified":
                {
                    
                    termGroupAuditInfo.UsingpecificMMSSMessage = string.Join("\n", siteInfos.Where(site => site.SiteType != SiteType.Google).Select(s => s.SiteUrl));
                    break;
                }
                case "All":
                    termGroupAuditInfo.UsingAllMMSSMessage = I18NEntity.GetString("RM_TM_AllMMS");
                    break;
                default:
                    termGroupAuditInfo.UsingNoneMMSSMessage = I18NEntity.GetString("RM_JS_Common_None");
                    break;
            }
            switch (termGroupAuditInfo.GoogleTermSyncOption)
            {
                case "Specified":
                {
                    termGroupAuditInfo.UsingSpecificGoogleMessage = string.Join("\n", siteInfos.Where(site => site.SiteType == SiteType.Google).Select(s => s.DisplayName));
                    break;
                }
                case "All":
                    termGroupAuditInfo.UsingAllGoogleMessage = I18NEntity.GetString("RM_TM_AllMMS");
                    break;
                default:
                    termGroupAuditInfo.UsingNoneGoogleMessage = I18NEntity.GetString("RM_JS_Common_None");
                    break;
            }
            return termGroupAuditInfo;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ConfigureTermGeneralSetting, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> SaveTermSettingInheritToParentAsync(int termId, TermSettingsInfo setting)
        {
            try
            {
                //check retirement date
                long lBeginTime = 0;
                long lEndTime = 0;
                long dbBeginTime = 0;//from db
                long dbEndTime = 0;//from db
                string strError = string.Empty;
                RMTerm term = TermDao.GetRMTermByTermId(termId);
                RMTerm oldParentInertTerm = TermDao.GetParentInhertSetting(termId);
                dbBeginTime = term.TermExpirationFrom;
                dbEndTime = term.TermExpirationTo;
                var selDateType = setting.selDateType;
                switch (selDateType)
                {
                    //only start time
                    case DateType.startTime:
                        if (string.IsNullOrEmpty(setting.beginTime))
                        {
                            strError = Convert.ToInt32(SaveTimeErrorType.sTimeIsNull).ToString();
                        }
                        else
                        {
                            DateTime dtBegin = DateTime.Parse(setting.beginTime);
                            dtBegin = DateTimeUtil.ConvertTimeToUtcDate(dtBegin, await GeneralSetting);
                            lBeginTime = dtBegin.Ticks;
                            if (lBeginTime != dbBeginTime && DateTime.Compare(dtBegin, DateTime.UtcNow) < 0)
                            {
                                strError = Convert.ToInt32(SaveTimeErrorType.startTimeIsEarlierNow).ToString();
                            }
                        }
                        break;
                    //only retire time
                    case DateType.endTime:
                        if (string.IsNullOrEmpty(setting.endTime))
                        {
                            strError = Convert.ToInt32(SaveTimeErrorType.eTimeIsNull).ToString();
                        }
                        else
                        {
                            DateTime dtEnd = DateTime.Parse(setting.endTime);
                            dtEnd = DateTimeUtil.ConvertTimeToUtcDate(dtEnd, await GeneralSetting);
                            lEndTime = dtEnd.Ticks;

                            if (lEndTime != dbEndTime && DateTime.Compare(dtEnd, DateTime.UtcNow) < 0)
                            {
                                strError = Convert.ToInt32(SaveTimeErrorType.endTimeIsEarlierNow).ToString();
                            }
                        }
                        break;
                    //from to time
                    case DateType.fromTimeAndToTime:
                        if (string.IsNullOrEmpty(setting.beginTime) || string.IsNullOrEmpty(setting.endTime))
                        {
                            strError = Convert.ToInt32(SaveTimeErrorType.fTimeAndToTimeIsNull).ToString();
                        }
                        else
                        {
                            DateTime dtBegin = DateTime.Parse(setting.beginTime);
                            dtBegin = DateTimeUtil.ConvertTimeToUtcDate(dtBegin, await GeneralSetting);
                            lBeginTime = dtBegin.Ticks;

                            DateTime dtEnd = DateTime.Parse(setting.endTime);
                            dtEnd = DateTimeUtil.ConvertTimeToUtcDate(dtEnd, await GeneralSetting);
                            lEndTime = dtEnd.Ticks;

                            if ((lBeginTime != dbBeginTime || lEndTime != dbEndTime) && DateTime.Compare(dtBegin, dtEnd) > 0)
                            {
                                strError = Convert.ToInt32(SaveTimeErrorType.fromTimeGtToTime).ToString();
                            }
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(strError))
                {
                    return strError;
                }
                setting.beginTimeForDB = lBeginTime;
                setting.endTimeForDB = lEndTime;
                var newTerm = TermDao.InheritSettingToParent(termId, setting);
                string result = GetJsonStrByObj(newTerm);
                try
                {
                    var newParentInertTerm = TermDao.GetParentInhertSetting(termId);
                    if ((newParentInertTerm != null && oldParentInertTerm.EnforceRetention != newParentInertTerm.EnforceRetention)
                        || (newParentInertTerm == null && oldParentInertTerm.EnforceRetention != 0))
                    {
                        ChangeClassificationDao.AddChange(new List<Guid> { term.UniqueId }, (int)TermChangeType.Retention);
                    }
                    ChangeClassificationDao.AddChange(new List<Guid> { term.UniqueId }, (int)TermChangeType.TermRule);
                }
                catch (Exception ee)
                {
                    logger.Warn("Add change term failed {0}:{1}", termId, ee.ToString());
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error("Save Term of InheritRuleToParent  Error. Term Id:{0}, Message:{1}.", termId, e.ToString());
                return string.Empty;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ConfigureTermGeneralSetting, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> SaveTermSettingAsync(TermSettingsInfo termSettingInfo)
        {
            var termId = termSettingInfo.tId;
            try
            {
                var infos = termSettingInfo.infos;
                var selDateType = termSettingInfo.selDateType;
                var beginTime = termSettingInfo.beginTime;
                var endTime = termSettingInfo.endTime;
                var isDayLight = (await GeneralSetting).DayLight;
                var timeZoneId = (await GeneralSetting).TimeZoneId;
                if (!string.IsNullOrEmpty(termSettingInfo.TimeZoneId))
                {
                    if (!string.IsNullOrEmpty(termSettingInfo.beginTime))
                    {
                        beginTime = DateTimeUtil.ConvertTimeZone(termSettingInfo.beginTime, termSettingInfo.TimeZoneId, timeZoneId);
                    }
                    if (!string.IsNullOrEmpty(termSettingInfo.endTime))
                    {
                        endTime = DateTimeUtil.ConvertTimeZone(termSettingInfo.endTime, termSettingInfo.TimeZoneId, timeZoneId);
                    }
                }
                var termDescription = termSettingInfo.des;
                List<RuleDisplayInfo> list = termSettingInfo.infos;
               
                //check retirement date
                long lBeginTime = 0;
                long lEndTime = 0;
                long dbBeginTime = 0;//from db
                long dbEndTime = 0;//from db
                string strError = string.Empty;
                RMTerm oldTerm = TermDao.GetRMTermByTermId(termId);
                RMTerm oldParentInhert = TermDao.GetParentInhertSetting(oldTerm.Id);
                if (oldParentInhert == null)
                {
                    oldParentInhert = oldTerm;
                }
                dbBeginTime = oldTerm.TermExpirationFrom;
                dbEndTime = oldTerm.TermExpirationTo;
                if (TenantService.IsCustomizationAppTenant() && !string.IsNullOrEmpty(termSettingInfo.advanceSettings))
                {
                    try
                    {
                        var advanceSettingsObject = JsonConvert.DeserializeObject<TermAdvanceSettings>(termSettingInfo.advanceSettings);
                        if (string.IsNullOrEmpty(advanceSettingsObject.SiteType))
                        {
                            return ErrorCode_AdvanceSettings_JsonFormat.ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Deserialize advance settigs error, advance settings: {termSettingInfo.advanceSettings}, error: {e}");
                        return ErrorCode_AdvanceSettings_JsonFormat.ToString();
                    }
                }

                switch (selDateType)
                {
                    //only start time
                    case DateType.startTime:
                        if (string.IsNullOrEmpty(beginTime))
                        {
                            strError = Convert.ToInt32(SaveTimeErrorType.sTimeIsNull).ToString();
                        }
                        else
                        {
                            DateTime dtBegin = DateTime.Parse(beginTime);
                            dtBegin = DateTimeUtil.ConvertTimeToUtcDate(dtBegin, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLight);
                            lBeginTime = dtBegin.Ticks;
                            if (lBeginTime != dbBeginTime && DateTime.Compare(dtBegin, DateTime.UtcNow) < 0)
                            {
                                strError = Convert.ToInt32(SaveTimeErrorType.startTimeIsEarlierNow).ToString();
                            }
                        }
                        break;
                    //only retire time
                    case DateType.endTime:
                        if (string.IsNullOrEmpty(endTime))
                        {
                            strError = Convert.ToInt32(SaveTimeErrorType.eTimeIsNull).ToString();
                        }
                        else
                        {
                            DateTime dtEnd = DateTime.Parse(endTime);
                            dtEnd = DateTimeUtil.ConvertTimeToUtcDate(dtEnd, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLight);
                            lEndTime = dtEnd.Ticks;

                            if (lEndTime != dbEndTime && DateTime.Compare(dtEnd, DateTime.UtcNow) < 0)
                            {
                                strError = Convert.ToInt32(SaveTimeErrorType.endTimeIsEarlierNow).ToString();
                            }
                        }
                        break;
                    //from to time
                    case DateType.fromTimeAndToTime:
                        if (string.IsNullOrEmpty(beginTime) || string.IsNullOrEmpty(endTime))
                        {
                            strError = Convert.ToInt32(SaveTimeErrorType.fTimeAndToTimeIsNull).ToString();
                        }
                        else
                        {
                            DateTime dtBegin = DateTime.Parse(beginTime);
                            dtBegin = DateTimeUtil.ConvertTimeToUtcDate(dtBegin, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLight);
                            lBeginTime = dtBegin.Ticks;

                            DateTime dtEnd = DateTime.Parse(endTime);
                            dtEnd = DateTimeUtil.ConvertTimeToUtcDate(dtEnd, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId), !isDayLight);
                            lEndTime = dtEnd.Ticks;

                            if ((lBeginTime != dbBeginTime || lEndTime != dbEndTime) && DateTime.Compare(dtBegin, dtEnd) > 0)
                            {
                                strError = Convert.ToInt32(SaveTimeErrorType.fromTimeGtToTime).ToString();
                            }
                        }
                        break;
                }
                if (!string.IsNullOrEmpty(strError))
                {
                    return strError;
                }
                termSettingInfo.beginTimeForDB = lBeginTime;
                termSettingInfo.endTimeForDB = lEndTime;
                
                var newTerm = await TermDao.SaveTermSettingAsync(termId, termSettingInfo);
                string result = GetJsonStrByObj(newTerm);
                try
                {
                    
                    await TrackForChangedTermSettingAsync(oldTerm, newTerm, oldParentInhert, termSettingInfo.breakInhert);
                }
                catch (Exception ee)
                {
                    logger.Warn("Add change term failed {0}:{1}", termId, ee.ToString());
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error("Save Term Error.Term Id:{0}, Message:{1}.", termId, e.ToString());
                return string.Empty;
            }
        }

        private async Task<bool> TrackForChangedTermSettingAsync(RMTerm oldTerm, RMTerm newTerm, RMTerm oldInertTerm, bool isBreakInert = false)
        {
            try
            {
                bool result = false;
                TermChangeType type = TermChangeType.None;
                var newInhertTerm = TermDao.GetParentInhertSetting(newTerm.Id);
                newInhertTerm = newInhertTerm == null ? newTerm : newInhertTerm;
                //validate
                if (string.IsNullOrEmpty(oldTerm.Name) || string.IsNullOrEmpty(newTerm.Name))
                {
                    logger.Warn("The name of the term shouldnt be null or empty.");
                    return result;
                }
                // ONLY NAME CHANGE OR RULE CHANGE, NEXT JOB WILL BE FULL JOB
                if (!oldTerm.Name.Equals(newTerm.Name, StringComparison.OrdinalIgnoreCase)
                    || oldInertTerm.RuleInfo != newInhertTerm.RuleInfo)
                {
                    result = true;
                    type = TermChangeType.TermRule;
                    ChangeClassificationDao.AddChange(new List<Guid> { newTerm.UniqueId }, (int)type);
                }
                if (oldInertTerm.EnforceRetention != newInhertTerm.EnforceRetention || isBreakInert)
                {
                    result = true;
                    type = TermChangeType.Retention;
                    ChangeClassificationDao.AddChange(new List<Guid> { newTerm.UniqueId }, (int)type);
                   
                }
                bool hasUpdateAllLabel = false;
                if (!string.IsNullOrEmpty(newInhertTerm.SPRetentionLabel))
                {
                    type = TermChangeType.Retention;
                    IRMEXOLabelDao RMLabelDao = new DB.Dao.Impl.RMEXOLabelDao();
                    var label = RMLabelDao.GetLabel((int)RMRetentionSourceType.SharePoint, (int)RMRetentionLabelStatus.FromGUI);
                    if (label != null && !label.LabelName.Equals(oldInertTerm.SPRetentionLabel))
                    {
                        var termIds = TermDao.GetAllValidEnforceRetentionTermIds();
                        if (termIds != null && termIds.Count > 0)
                        {
                            hasUpdateAllLabel = true;
                            ChangeClassificationDao.AddChange(termIds, (int)type);
                        }
                        logger.Info($"label changed:{oldInertTerm.SPRetentionLabel} 2 {label?.LabelName}, {termIds?.Count}");
                    }
                }

                if (!string.IsNullOrEmpty(newInhertTerm.EXORetentionLabel) && !hasUpdateAllLabel)
                {
                    type = TermChangeType.Retention;
                    IRMEXOLabelDao RMLabelDao = new DB.Dao.Impl.RMEXOLabelDao();
                    var label = RMLabelDao.GetLabel((int)RMRetentionSourceType.Exchange, (int)RMRetentionLabelStatus.FromGUI);
                    if (label != null && !label.LabelName.Equals(oldInertTerm.EXORetentionLabel))
                    {
                        var termIds = TermDao.GetAllValidEnforceRetentionTermIds();
                        if (termIds != null && termIds.Count > 0)
                        {
                            ChangeClassificationDao.AddChange(termIds, (int)type);
                        }
                        logger.Info($"label changed:{oldInertTerm.EXORetentionLabel} 2 {label?.LabelName}, {termIds?.Count}");
                    }
                }

                if (!string.IsNullOrEmpty(newInhertTerm.OneDriveRetentionLabel) && !hasUpdateAllLabel)
                {
                    type = TermChangeType.Retention;
                    IRMEXOLabelDao RMLabelDao = new DB.Dao.Impl.RMEXOLabelDao();
                    var label = RMLabelDao.GetLabel((int)RMRetentionSourceType.OneDrive, (int)RMRetentionLabelStatus.FromGUI);
                    if (label != null && !label.LabelName.Equals(oldInertTerm.OneDriveRetentionLabel))
                    {
                        var termIds = TermDao.GetAllValidEnforceRetentionTermIds();
                        if (termIds != null && termIds.Count > 0)
                        {
                            ChangeClassificationDao.AddChange(termIds, (int)type);
                        }
                        logger.Info($"label changed:{oldInertTerm.OneDriveRetentionLabel} 2 {label?.LabelName}, {termIds?.Count}");
                    }
                }

                if (!string.IsNullOrEmpty(newInhertTerm.TeamsRetentionLabel) && !hasUpdateAllLabel)
                {
                    type = TermChangeType.Retention;
                    IRMEXOLabelDao RMLabelDao = new DB.Dao.Impl.RMEXOLabelDao();
                    var label = RMLabelDao.GetLabel((int)RMRetentionSourceType.Teams, (int)RMRetentionLabelStatus.FromGUI);
                    if (label != null && !label.LabelName.Equals(oldInertTerm.TeamsRetentionLabel))
                    {
                        var termIds = TermDao.GetAllValidEnforceRetentionTermIds();
                        if (termIds != null && termIds.Count > 0)
                        {
                            ChangeClassificationDao.AddChange(termIds, (int)type);
                        }
                        logger.Info($"label changed:{oldInertTerm.TeamsRetentionLabel} 2 {label?.LabelName}, {termIds?.Count}");
                    }
                }

                await CheckEnforceRetentionScheduleAsync(newInhertTerm.EnforceRetention);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to confirm if we need to update the term change time for the next collect job. we'll run full to ensure the consistance of the data. Exception:{0}", ex.ToString());
                return true;
            }
        }

        private async System.Threading.Tasks.Task CheckEnforceRetentionScheduleAsync(int retentionOption)
        {
            var environmentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
            if (!environmentName.Equals(ENVIRONMENT_NAME, StringComparison.OrdinalIgnoreCase))
            {
                if ((retentionOption & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint)
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.EnforceRetention);
                }
                if ((retentionOption & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange)
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.EXOEnforceRetention);
                }
                if ((retentionOption & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.OneDriveEnforceRetention);
                }
                if ((retentionOption & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams && TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.TeamsEnforceRetention);
                }
            }           
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ConfigureTermSetSetting, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> UpdateTermSetAsync(int termSetId, string termSetName, string des)
        {
            try
            {
                return GetJsonStrByObj(await TermSetDao.UpdateTermSetAsync(termSetId, termSetName, des));
            }
            catch (Exception e)
            {
                logger.Error("Save TermSet Error.Term Set Id:{0}, Message:{1}.", termSetId, e.ToString());
                return string.Empty;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ConfigureTermGroupSetting, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> UpdateTermGroupAsync(int termGroupId, string termGroupName, string des, List<RMSiteInfo> siteInfo, bool usingMMSSpecified, int m365SyncOption, int googleSyncOption)
        {
            RAReturnMessage result = new();
            try
            {
                var (validateFailed, failMessage) = await ValidateGoogleTenants(googleSyncOption, termGroupId, siteInfo);
                if (validateFailed)
                {
                    return failMessage;
                }

                var termGroupDB = TermGroupDao.GetTermGroupById(termGroupId);

                var hasLicenseGoogle = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusGoogle);

                var hasOpusIL = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);

                m365SyncOption = hasOpusIL ? m365SyncOption : (int)termGroupDB.M365TermSyncOption;
                googleSyncOption = hasLicenseGoogle ? googleSyncOption : (int)termGroupDB.GoogleTermSyncOption;

                var termGroup = await TermGroupDao.UpdateTermGroupAsync(termGroupId, termGroupName, des, usingMMSSpecified, m365SyncOption, googleSyncOption);
                foreach (var info in siteInfo.Where(siteInfo => siteInfo.SiteType != SiteType.Google))
                {
                    await HandleM365SiteInfo(info, (TermSyncOption)m365SyncOption, usingMMSSpecified);
                }
                if (hasLicenseGoogle)
                {
                    await HandleGoogleTenants(siteInfo.Where(siteInfo => siteInfo.SiteType == SiteType.Google).ToList(), termGroup.UniqueId, (TermSyncOption) googleSyncOption);
                }

                return result;
            }
            catch (Exception e)
            {
                logger.Error("Save TermGroup Error.Term Group Id:{0}, Message:{1}.", termGroupId, e.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = e.Message;
                result.Extension = "Exception";
                return result;
            }
        }

        private async Task RefreshTermPermissionCacheAsync()
        {
            await SecurityTrimmingHelper.RemovePermissionCacheAsync();
            RedisCacheService.CacheProvider.KeyDel(CacheKeyPrefix.SecurityTermCacheKeyPrefix + TenantLocalValue.LogonGroupId);
        }

        private async Task<(bool, RAReturnMessage)> ValidateGoogleTenants(int googleSyncOption, int termGroupId, List<RMSiteInfo> siteInfos)
        {
            if ((TermSyncOption)googleSyncOption == TermSyncOption.None)
            {
                return (false, null);
            }
            RAReturnMessage result = new();
            var tenants = await RMAosApiClient.GetGoogleTenants(TenantLocalValue.LogonGroupId);
            var notExistedGoogleProfile = siteInfos.Where(siteInfo => siteInfo.SiteType == SiteType.Google && (!tenants.ContainsKey(siteInfo.SiteUrl) || !tenants.ContainsValue(siteInfo.DisplayName))).ToList();
            if (notExistedGoogleProfile.Count != 0)
            {
                result.MessageType = RAMessageType.Failed;
                return (true, result);
            }
            if ((TermSyncOption)googleSyncOption == TermSyncOption.All)
            {
                var termGrUniqueId = TermGroupDao.GetRMTermGruop(termGroupId).UniqueId;
                var allGoogleTenants = tenants.Select(tenant => new RMSiteInfo
                {
                    TermGroupId = termGrUniqueId, 
                    DisplayName = tenant.Value, 
                    SiteUrl = tenant.Key, 
                    SiteType = SiteType.Google, 
                    Action = SiteAction.Add
                }).ToList();
                siteInfos.AddRange(allGoogleTenants);
            }
            var googleTenants = siteInfos.Where(siteInfo => siteInfo.SiteType == SiteType.Google).ToList();
            var googleTenantsExistInOtherTermGroup = googleSyncOption != 0
                ? await TermGroupMembershipDao.GetGoogleTenantsExisted(
                    googleTenants.Select(googleTenant => googleTenant.SiteUrl).ToList(), googleTenants[0].TermGroupId)
                : [];
            if (googleTenantsExistInOtherTermGroup.IsNotNullOrEmpty())
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = JsonConvert.SerializeObject(googleTenantsExistInOtherTermGroup);
                result.Extension = "ExistedGoogleTenants";
                return (true, result);
            }
            return (false, null);
        }

        private async Task HandleM365SiteInfo(RMSiteInfo info, TermSyncOption m365Option, bool usingMMSSpecified)
        {
            var id = info.Id;
            var groupId = info.TermGroupId;
            var url = info.SiteUrl;
            var dispalyName = info.DisplayName;
            var termStoreId = info.TermStoreId;
            var termStoreName = info.TermStoreName;
            var agentGroupId = info.AgentGroupId;
            var siteType = info.SiteType;
            bool alreadyExist;
            switch (info.Action)
            {
                case SiteAction.Add:
                    alreadyExist = TermGroupMembershipDao.ExistTermGroupInfo(groupId, termStoreId);
                    if (!alreadyExist)
                    {
                        TermGroupMembershipDao.AddTermGroupInfo(groupId, url, dispalyName, termStoreName, termStoreId,
                            agentGroupId, siteType);
                        logger.Info("add termgroup relatived mms info success,groupId:{0},url:{1},name:{2}", groupId,
                            url, dispalyName);
                    }

                    break;
                case SiteAction.Update:
                    await TermGroupMembershipDao.UpdateTermGroupInfoAsync(id, groupId, url, dispalyName, termStoreName,
                        termStoreId, agentGroupId, siteType);
                    break;
                case SiteAction.Delete:
                    alreadyExist = TermGroupMembershipDao.ExistTermGroupInfo(groupId, termStoreId);
                    if (alreadyExist)
                    {
                        TermGroupMembershipDao.DeleteTermGroupInfo(groupId, termStoreId);
                        logger.Info("delete termgroup relatived mms info success,groupId:{0},url:{1},name:{2}", groupId,
                            url, dispalyName);
                    }

                    break;
                default:
                    logger.Warn("update termgroup relatived mms info faild,groupId:{0},url:{1},name:{2}", groupId, url,
                        dispalyName);
                    break;
            }

            if (!usingMMSSpecified || m365Option == TermSyncOption.None)
            {
                alreadyExist = TermGroupMembershipDao.ExistTermGroupInfo(groupId, termStoreId);
                if (alreadyExist)
                {
                    TermGroupMembershipDao.DeleteTermGroupInfo(groupId, termStoreId);
                    logger.Info("delete termgroup relatived mms info success,groupId:{0},url:{1},name:{2}", groupId,
                        url, dispalyName);
                }
            }
        }

        private async Task HandleGoogleTenants(List<RMSiteInfo> siteInfo, Guid termGrId, TermSyncOption googleTermSync)
        {
            if (googleTermSync == TermSyncOption.None)
            {
                await TermGroupMembershipDao.DeleteGoogleTenantsByTermGroupId(termGrId);
                return;
            }
            var allExistGoogleTenantInTermGr = await TermGroupDao.GetSpecifiedGoogleTenants(termGrId);
            var needAddGoogleTenants = siteInfo.ExceptBy(allExistGoogleTenantInTermGr, siteInfo => siteInfo.SiteUrl);
            foreach (var needAddGoogleTenant in needAddGoogleTenants)
            {
                await TermGroupMembershipDao.AddGoogleTenantTermGroup(termGrId, needAddGoogleTenant.SiteUrl,
                    needAddGoogleTenant.DisplayName, needAddGoogleTenant.DisplayName, Guid.Empty,
                    needAddGoogleTenant.SiteUrl, SiteType.Google);
            }
            var needDeleteGoogleTenants = allExistGoogleTenantInTermGr.Except(siteInfo.Select(site => site.SiteUrl),
                StringComparer.OrdinalIgnoreCase).ToList();
            await TermGroupMembershipDao.DeleteGoogleTenantsByTermGroupIdAndSiteUrl(needDeleteGoogleTenants, termGrId);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.DeleteTermGroup, BeforeHandler = typeof(TermManagementBeforeAuditHandler), AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> DeleteTermGroupAsync(Guid termGroupId)
        {
            try
            {
                var termSets = TermSetDao.GetRMTermSetsByGroupUniqueId(termGroupId);
                List<Guid> mlTermIds = new List<Guid>();
                foreach (var termSet in termSets)
                {
                    var terms = TermDao.GetActiveTermsByTermSetId(termSet.Id);
                    foreach(var term in terms)
                    {
                        var mlTerm = RMMLTermDao.GetTrainingTerm(term.UniqueId);
                        if(mlTerm != null)
                        {
                            mlTermIds.AddRange(terms.Select(t => t.UniqueId));
                        }
                    }
                }
                await TermGroupDao.DeleteTermGroupAsync(termGroupId);
                if (mlTermIds.Count > 0)
                {
                    RMMLTermDao.DeleteTerms(mlTermIds);
                    IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                    await vectorStore.DeleteVectorsByIdsAsync(mlTermIds);
                }
                return "1";
            }
            catch
            {
                return "";
            }
        }

        public async Task<string> GetParentInhertSettingAsync(int termId)
        {
            try
            {
                RMTerm term = TermDao.GetParentInhertSetting(termId);
                var scopeRuleContainers = RMSecurityTrimmingHelper.GetRuleScopeByTermId(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId, termId.ToString());
                var associateAvailableRule = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(scopeRuleContainers);
                var availableRuleIds = associateAvailableRule.Select(r => new Guid(r.RuleId));
                List<RMTermRuleAssociation> listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId)
                    .Where(r => availableRuleIds.Contains(r.RuleId)).ToList();
                if (!listRule.Any())
                {
                    listRule = new List<RMTermRuleAssociation>();
                }
                return GetJsonStrByObj(new { term = term, rule = listRule , associateAvailableRule = associateAvailableRule });
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get parent term setting,{0}", ex.ToString());
            }
            return GetJsonStrByObj(new { message = "" });

        }

        public async Task<string> GetTermSettingWithGoogleRuleAsync(int termId)
        {
            try
            {
                RMTerm term = TermDao.GetTermTimeSettings(termId);
                var (associateAvailableGoogleRule, mixedRuleIds) = await RuleManagerService.GetGoogleRulesAndMixedRuleIdsAsync();
                var googleRuleIds = associateAvailableGoogleRule.Select(r => new Guid(r.RuleId)).ToList();

                List<RMTermRuleAssociation> listRules = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId).Where(r => googleRuleIds.Contains(r.RuleId)).ToList();
                var associateMixedRuleIds = mixedRuleIds.Where(id => listRules.Select(rule => rule.RuleId).Contains(id));
                if (!listRules.Any())
                {
                    listRules = new List<RMTermRuleAssociation>();
                }
                return GetJsonStrByObj(new { term = term, rule = listRules, associateAvailableRule = associateAvailableGoogleRule, mixedRuleIds = associateMixedRuleIds });
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get parent term setting,{0}", ex.ToString());
            }
            return GetJsonStrByObj(new { message = "" });

        }

        public async Task<string> GetRuleAssicationWithTermIdAsync(int termId)
        {
            try
            {
                var associateAvailableRule = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                var availableRuleIds = associateAvailableRule.Select(r => new Guid(r.RuleId));
                List<RMTermRuleAssociation> listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId)
                    .Where(r => availableRuleIds.Contains(r.RuleId)).ToList();
                if (!listRule.Any())
                {
                    listRule = new List<RMTermRuleAssociation>();
                }
                return GetJsonStrByObj(listRule);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get parent term setting,{0}", ex.ToString());
            }
            return GetJsonStrByObj(new { message = "" });

        }

        public string GetTermTimeSettings(int termId)
        {
            return GetJsonStrByObj(TermDao.GetTermTimeSettings(termId));
        }


        public string GetParentTermTimeSettings(int termId)
        {
            return GetJsonStrByObj(TermDao.GetParentTermTimeSettings(termId));
        }


        public async Task<string> GetTermRuleInfoByTermidAsync(int termId)
        {
            var scopeRuleContainers = RMSecurityTrimmingHelper.GetRuleScopeByTermId(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId, termId.ToString());
            var associateAvailableRuleIds = (await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(scopeRuleContainers)).Select(r => new Guid(r.RuleId));
            List<RMTermRuleAssociation> listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId)
                .Where(r => associateAvailableRuleIds.Contains(r.RuleId)).ToList();

            if (null == listRule || listRule.Count == 0)
            {
                return GetJsonStrByObj(new { message = "" });
            }
            return GetJsonStrByObj(listRule);
        }

        public string GetTermRuleInfoByTermIdAndSourceFlag(int termId, SourceFlag sourceFlag = SourceFlag.All)
        {
            List<RMTermRuleAssociation> listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId, sourceFlag);
            if (null == listRule || listRule.Count == 0)
            {
                return GetJsonStrByObj(new { message = "" });
            }
            return GetJsonStrByObj(listRule);
        }

        public bool GetTermPermanentByTermId(int termId, bool onlyParent)
        {
            return TermDao.GetTermPermanentByTermId(termId, onlyParent);
        }

        public string GetParentSettingInfoByTermId(int termId)
        {

            var termSeting = TermRuleAssociationDao.GetParentSettingsByTermId(termId);
            if (null == termSeting.infos)
            {
                return GetJsonStrByObj(new { message = "" });
            }
            return GetJsonStrByObj(termSeting);
        }

        public string GetTermWithPathByTermId(Guid termId)
        {
            string result = string.Empty;
            try
            {
                var term = TermDao.GetRMTermWithPathByTermId(termId);
                result = GetJsonStrByObj(term);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get term with path:{0}", ex.ToString());
            }
            return result;

        }
        public string GetTermPathByTermId(Guid termId, bool forExport = false)
        {
            try
            {
                var term = TermDao.GetRMTermWithPathByTermId(termId, forExport);
                if (term != null)
                {
                    return term.FullPath;
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get term with path:{0}", ex.ToString());
            }
            return string.Empty;

        }

        public string GetTermNamesPathByTermId(int termId)
        {
            return TermDao.GetTermNamePath(termId);
        }
        public string GetGermSetNamesPathByTermSetId(int termSetId)
        {
            return TermDao.GetTermSetNamesPathByTermSetId(termSetId);
        }

        public string GetTermNameByTermId(int termId)
        {
            return TermDao.GetTermNameByTermId(termId);
        }
        public string GetTermDescriptionByTermId(int termId)
        {
            return TermDao.GetRMTermByTermId(termId).Description;
        }
        public string GetTermAdvancedSettingsByTermId(int termId)
        {
            return TermDao.GetRMTermByTermId(termId).AdvanceSettings;
        }

        public string GetTermGroupNameById(int groupId)
        {
            return TermDao.GetTermGroupNameById(groupId);
        }
        public string GetTermGroupNameById(Guid termGroupId)
        {
            RMTermGroup termGroup = TermGroupDao.GetTermGroupByGuid(termGroupId);
            return termGroup.Name;
        }
        public string GetTermSetNameById(int termSetId)
        {
            return TermDao.GetTermSetNameById(termSetId);
        }
        public async Task<TermAuditInfo> GetTermRuleInfosByTermIdAsync(int termId)
        {
            string strRuleNames = string.Empty;
            List<string> listRuleName = new List<string>();
            List<RuleDisplayInfo> listRule = new List<RuleDisplayInfo>();
            TermAuditInfo info = new TermAuditInfo();
            RMTerm term = TermDao.GetTermTimeSettings(termId);
            //bool isPermanentTerm;
            int isEnforceRetention;
            if (!term.BreakInheritFromParent && !term.IsRootTerm)
            {
                var t = TermRuleAssociationDao.GetParentSettingsByTermId(termId);
                listRule = t.infos;
                isEnforceRetention = t.EnforceRetention;
                //listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId);
                //isPermanentTerm = TermDao.GetTermPermanentByTermId(termId, true);

            }
            else
            {
                
                var rList = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId);
                if (rList != null && rList.Count > 0)
                {
                    listRule = rList.ConvertAll(r => { return new RuleDisplayInfo() { RuleName = r.RuleName }; });
                }
                isEnforceRetention = term.EnforceRetention;
                //isPermanentTerm = term.IsPermanent;
            }

            if (null != listRule && listRule.Count > 0)
            {
                listRuleName = listRule.Select(l => l.RuleName).ToList();
                strRuleNames = string.Join(",", listRuleName.ToArray());
            }
            info.IsRootTerm = term.IsRootTerm;
            info.IsBreakInheritance = term.BreakInheritFromParent;
            info.RuleNames = strRuleNames;
            var timezoneId = (await GeneralSetting).TimeZoneId;
            var timeZoneName = DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == timezoneId).FirstOrDefault()?.Zone;
            info.BeginTime = string.IsNullOrEmpty(term.TermExpirationFromStr) ? string.Empty : string.Format("{0} {1}", term.TermExpirationFromStr, timeZoneName);
            info.EndTime = string.IsNullOrEmpty(term.TermExpirationToStr) ? string.Empty : string.Format("{0} {1}", term.TermExpirationToStr, timeZoneName);
            //info.Permanent = isPermanentTerm;
            info.EnfoceRentention = isEnforceRetention;
            info.ExchangeLabel = (isEnforceRetention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange ? term.EXORetentionLabel : "";
            info.SPLabel = (isEnforceRetention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint ? term.SPRetentionLabel : "";
            info.OneDriveLabel = (isEnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive ? term.OneDriveRetentionLabel : "";
            info.TeamsLabel = (isEnforceRetention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams ? term.TeamsRetentionLabel : "";
            return info;
        }
        
        public void AddActiveTermToList(RMTerm term, List<RMTerm> listActiveTerms)
        {
            ArgumentCheck.NotNull(listActiveTerms, nameof(listActiveTerms));
            if (listActiveTerms == null || listActiveTerms.Count == 0)
            {
                if (!term.IsDeprecated && !TermDao.IsExpiredTerm(term.Id))
                {
                    listActiveTerms.Add(term);
                }
                else
                {
                    //判斷是否存在active狀態的子term 存在則加到listActiveTerms
                    List<RMTerm> subTerms = TermDao.GetTermFromParentTermWithoutDeletedTerm(term.Id);
                    if (subTerms != null && subTerms.Count > 0 && listActiveTerms.Count == 0)
                    {
                        foreach (var subTerm in subTerms)
                        {
                            if (!subTerm.IsDeprecated && !TermDao.IsExpiredTerm(subTerm.Id))
                            {
                                listActiveTerms.Add(term);
                                break;
                            }
                            else
                            {
                                AddActiveTermToList(subTerm, listActiveTerms);
                            }
                        }
                    }
                }
            }
        }

        public List<RMTerm> GetFilterTerms(List<RMTerm> terms)
        {
            List<RMTerm> filterTerms = new List<RMTerm>();
            Dictionary<RMTerm, List<RMTerm>> dic = new Dictionary<RMTerm, List<RMTerm>>();
            foreach (var term in terms)
            {
                var listActiveTerms = new List<RMTerm>();
                AddActiveTermToList(term, listActiveTerms);
                dic.Add(term, listActiveTerms);
            }
            foreach (KeyValuePair<RMTerm, List<RMTerm>> item in dic)
            {
                if (item.Value != null && item.Value.Count > 0)
                {
                    if (item.Key.IsDeprecated || TermDao.IsExpiredTerm(item.Key.Id))
                    {
                        item.Key.IsDeprecated = true;
                    }
                    filterTerms.Add(item.Key);
                }
            }
            return filterTerms;
        }

        /// <summary>
        /// 将对象转成Json字符串
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        private string GetJsonStrByObj(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public bool IsOrphanedTerm(Guid id)
        {
            var term = TermDao.GetRMTermByGuId(id);
            bool result;
            if (term.IsDeprecated || term.IsRemoved)
            {
                result = true;
            }
            else
            {
                result = TermDao.IsExpiredTerm(term.Id); 
            }
            return result;
        }

        public string RunImportTermStructure(JobRunBy jobRunBy, string extension, string strBytes, bool isControlPlus = false)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportTermStructure,
                    Parameters = string.Format("{0} {1} {2}", extension, strBytes, isControlPlus),
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunSyncLocationTreeToSharePoint,ERROR:{0}", ex.ToString());
            }

            return id;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ImportTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public string RealRunImportTermStructureJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes, bool isControlPlus = false)
        {
            Trace.TraceError("enterRunImportTermStructure.....");

            string id = string.Empty;

            if (jobRunBy == JobRunBy.Control)
            {
                id = JobMonitorService.CreateJob(JobType.ImportTermStructure, jobRunByUser);
                logger.Info("Begin control Import Term Job {0}", id);
            }

            Trace.TraceError("jobmonitorId:{0}", id);
            baseJobDto = new BaseJobDto() { Id = id, JobType = (int)JobType.ImportTermStructure };
            //查询当前还没有结束的Term Sync Job
            List<string> importJobs = JobMonitorService.GetRunningJobs(JobType.ImportTermStructure);

            //Import Term Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = false;
            if (importJobs != null && importJobs.Count > 0)
            {
                var otherImportJobs = importJobs.Where(j => !j.Equals(id)).ToList();
                if (otherImportJobs != null && otherImportJobs.Count > 0)
                {
                    isSkip = true;
                }
            }
            if (!isSkip)
            {
                //新起线程起Job
                StartImportTermStructureJob(id, extension, strBytes, isControlPlus);
            }
            else
            {
                JobMonitorService.UpdateJobStatus(id, JobStatus.Skipped, "RM_ImportTerm_JobSkip");
                logger.Info(I18NEntity.GetString("RM_ImportTerm_JobSkip"));
            }

            return id;
        }

        public async Task<RAReturnMessage> RunExportTermStructure(JobRunBy jobRunBy)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportTermStructure,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Empty,
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportTermStructure,ERROR:{0}", ex.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }

            return returnMessage;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ExportTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<string> RealRunExportTermStructureJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            Trace.TraceError("enter RunExportTermStructure.....");

            var jobId = string.Empty;

            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

            if (jobRunBy == JobRunBy.Control)
            {
                jobId = JobMonitorService.CreateJob(JobType.ExportTermStructure, jobRunByUser);
                logger.Info("Begin control Export Term Job {0}", jobId);
            }

            Trace.TraceError("jobmonitorId:{0}", jobId);
            baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.ExportTermStructure };
            StartExportTermStructureJob(jobId, account.UserId);
            return jobId;
        }

        private void StartImportTermStructureJob(string jobId, string extension, string strBytes, bool isControlPlus = false)
        {
            string content = "\"" + strBytes + "\"";
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportTermStructure,
                CommandLine = string.Format("{0} {1} {2} {3} {4}", JobType.ImportTermStructure, jobId, extension, content, isControlPlus),
            });
        }
        private void StartExportTermStructureJob(string jobId, string userId)
        {
            DownloadDataInfoDao.Create(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = jobId,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = userId,
                Name = jobId + ".zip",
                DownloadType = DownloadContentType.ExportTermStructure,
            });

            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ExportTermStructure,
                CommandLine = string.Format("{0} {1} ", JobType.ExportTermStructure, jobId),
            });
        }
        public async Task<List<RMSiteInfo>> GetRegisteredSiteMMSInfoAsync()
        {

            List<RMSiteInfo> siteInfoList = new List<RMSiteInfo>();
            List<RMSPTreeNode> registeredSites = SPSettingTreeService.LoadFarm();
            var defaultSites = await SPSettingTreeService.BrowseAsync(registeredSites[0]);
            foreach (var defaultSite in defaultSites)
            {
                List<RMSPTreeNode> sites = await SPSettingTreeService.BrowseAsync(defaultSite);
                foreach (var site in sites)
                {
                    try
                    {
                        AvePoint.RA.SharePoint.RMSharePointTaxnomy.RMSharePointTaxonomy m_Taxonomy = new AvePoint.RA.SharePoint.RMSharePointTaxnomy.RMSharePointTaxonomy();
                        m_Taxonomy.InitClientContext(site);
                        var termStoreId = m_Taxonomy.GetDefaultTermStoreId();
                        var termStoreName = m_Taxonomy.GetDefaultTermStoreName();
                        siteInfoList.Add(new RMSiteInfo()
                        {
                            SiteUrl = site.FullPath,
                            DisplayName = termStoreName + "(" + termStoreId + ")",
                            TermStoreId = termStoreId,
                            TermStoreName = termStoreName,
                            AgentGroupId = defaultSite.SPObjectId
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error("an error occurred while get registered site info,ERROR:{0}", ex.ToString());
                    }


                }
            }
            return siteInfoList;
        }

        public List<RMSiteInfo> GetRelativedSiteMMSInfo(Guid termGroupId)
        {
            List<RMSiteInfo> result = new List<RMSiteInfo>();
            var mmsSiteInfos = TermGroupMembershipDao.GetTermGroupInfoById(termGroupId);
            foreach (var info in mmsSiteInfos)
            {
                result.Add(new RMSiteInfo()
                {
                    Id = info.Id,
                    DisplayName = info.DisplayName,
                    TermGroupId = info.TermGroupId,
                    SiteUrl = info.SiteUrl,
                    AgentGroupId = info.AgentGroupId,
                    TermStoreId = info.TermStoreId,
                    SiteType = info.SiteType
                });
            }
            return result;
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByUrl(string siteUrl)
        {
            RemoteSiteCollection site = new RemoteSiteCollection();
            try
            {
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    //var client = new DAOAPIClientV1();
                    //site = client.GetRemoteSiteCollectionByUrl(siteUrl);
                    site = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("GetRemoteSiteCollectionByUrl Failed,SC url is {0}, message:{1}", siteUrl, ex.ToString()));
            }
            return site;
        }

        public async Task<RMSiteInfo> GetRegisteredSiteMMSInfoByUrlAsync(string url)
        {
            var siteInfo = GetSPOnlineSiteCollection(url);
            if (siteInfo == null)
            {
                siteInfo = await GetSPOnpremSiteCollectionAsync(url);
            }
            return siteInfo;
        }

        private RMSiteInfo GetSPOnlineSiteCollection(string url)
        {
            try
            {
                var site = GetRemoteSiteCollectionByUrl(url);
                if (site != null)
                {
                    RMSharePointTaxonomy m_Taxonomy = new RMSharePointTaxonomy();
                    m_Taxonomy.InitClientContext(site);
                    var termStoreId = m_Taxonomy.GetDefaultTermStoreId();
                    var termStoreName = m_Taxonomy.GetDefaultTermStoreName();
                    var siteInfo = new RMSiteInfo
                    {
                        SiteUrl = url,
                        DisplayName = termStoreName + "(" + termStoreId + ")",
                        TermStoreId = termStoreId,
                        TermStoreName = termStoreName,
                        AgentGroupId = site.parentId,
                        SiteType = SiteType.Online
                    };
                    logger.Info("success to GetSPOnlineSiteCollection SiteUrl:{0},Site AgentGroupId:{1}", site.url, string.Join(",", site.AvailableAgentIds == null ? new List<string>() : site.AvailableAgentIds));
                    return siteInfo;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"an error occurred while GetSPOnlineSiteCollection from url:[{url}],ERROR:{0}", ex.ToString());
            }
            return null;
        }

        private async Task<RMSiteInfo> GetSPOnpremSiteCollectionAsync(string url)
        {
            try
            {
                url = url.TrimEnd('/');
                var site = SharePointOnPremClient.GetLocalSiteCollectionByUrl(url);
                if (site != null)
                {
                    var termStoreInfo = await SharePointOnPremClient.GetTermStoreInfoBySiteUrlAsync(url);
                    var termStoreId = termStoreInfo.TermStoreId;
                    var termStoreName = termStoreInfo.TermStoreName;

                    var siteInfo = new RMSiteInfo
                    {
                        SiteUrl = url,
                        DisplayName = termStoreName + "(" + termStoreId + ")",
                        TermStoreId = termStoreId,
                        TermStoreName = termStoreName,
                        AgentGroupId = site.ParentId,
                        SiteType = SiteType.OnPrem
                    };
                    logger.Info($"success to GetSPOnpremSiteCollection, url:[{url}]");

                    //var termStoreName = "Managed Metadata Service";
                    //var termStoreId = new Guid("8979b8caf78e46b7bd8c0ce5e7589151");
                    //var siteInfo = new RMSiteInfo
                    //{
                    //    SiteUrl = url,
                    //    DisplayName = termStoreName + "(" + termStoreId + ")",
                    //    TermStoreId = termStoreId,
                    //    TermStoreName = termStoreName,
                    //    AgentGroupId = site.ParentId,
                    //    SiteType = SiteType.OnPrem
                    //};
                    return siteInfo;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"an error occurred while GetSPOnpremSiteCollection from url:[{url}],ERROR:{0}", ex.ToString());
            }
            return null;
        }
        public string GetTermTree(CurrentSettingsInfo settingInfo)
        {
            //get term group
            RMTermGroup termGroup = new RMTermGroup();
            List<RMTermSet> allTermSet = new List<RMTermSet>();
            RMTermSet termSet = new RMTermSet();
            Guid curTermGuid = new Guid(settingInfo.CurrentNodeId);
            RMTerm curTerm = TermDao.GetRMTermByUniqueId(curTermGuid);
            Stack<int> termIdOfFullPath = new Stack<int>();
            int[] curTermId = { curTerm.Id };
            //get all parent termid /termset id
            string[] allIds = TermSetMembershipDao.GetRMTermSetMemberships(curTermId, true).First().Path.Split('/').Reverse().ToArray();
            //最后一个是TermSetId,过滤掉在后面单独处理
            for (int i = 0; i < allIds.Length - 1; i++)
            {
                termIdOfFullPath.Push(Convert.ToInt32(allIds[i]));
            }
            int termSetId = Convert.ToInt32(allIds.Last());
            termSet = TermSetDao.GetRMTermSet(termSetId);
            termGroup = TermGroupDao.GetTermGroupByGuid(termSet.TermGroupId);
            if (curTerm != null && !curTerm.IsRemoved)
            {

                termSet.subTermCount = TermSetMembershipDao.GetSubTermMembershipsByTermSetId(termSetId).Count();
                termSet.subTerms = GetSavedSubTermWithPageInfo(termSetId, "TermSet", termIdOfFullPath, settingInfo.perPageCount, termSet.subTermCount, termIdOfFullPath.LastOrDefault());
                allTermSet.Add(termSet);
                termGroup.subTerms = allTermSet;
                termGroup.subTermCount = 1;
            }

            if (settingInfo.SettingType == 1)
            {
                //string agentGroupId = settingInfo.spTreeNodes[0].SPObjectId;
                string agentGroupId = settingInfo.AgentGroupId;
                RMSharePointSetting setting = SharePointSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
                //termSet = TermSetDao.GetRMTermSetByGuid(setting.TermSetId);
                var oldtermGroup = GetBusinessTermGroupByDefaultTermSetId(setting.TermSetId);
                if (oldtermGroup.Id != termGroup.Id)
                {
                    return GetJsonStrByObj(new
                    {
                        TermGroup = oldtermGroup,
                        IsChangeAnotherTermGroup = true
                    });
                }

                if (termGroup.IsRemoved)
                {
                    return "";
                }
                return GetJsonStrByObj(new
                {
                    TermGroup = termGroup,
                    IsChangeAnotherTermGroup = false
                });
            }
            else
            {
                var termGroups = TermGroupDao.LoadTermGroup(false);
                if (termGroups != null && termGroups.Count > 0)
                {
                    for (int i = 0; i < termGroups.Count; i++)
                    {
                        if (termGroups[i].UniqueId.Equals(termGroup.UniqueId))
                        {
                            termGroups[i] = termGroup;
                            break;
                        }
                    }
                }
                return GetJsonStrByObj(termGroups);
            }
            //finally we get a tree from term group to selected term.
            //return GetJsonStrByObj(termGroup);
        }

        public DeclarationSetting GetTermRetentionInfoByTermId(Guid termId)
        {
            RMTerm curTerm = TermDao.GetRMTermByUniqueId(termId);
            return new DeclarationSetting();
        }

        #region Export Term
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ExportTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async System.Threading.Tasks.Task GenerateReportForTermInfoAsync(string folderPath, string fileName, string sheetName)
        {
            string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".xlsx";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            try
            {
                using (RA.Common.PerformanceScope scope = new RA.Common.PerformanceScope("termmanagement.exportterms"))
                {
                    List<string[]> termDatas = new List<string[]>();
                    var hasUpgradeTeams = TeamsPermissionHelper.HasUpgradeTeamsFeature();
                    using (new RA.Common.PerformanceScope("termmanagement.exportterms.buildTerm"))
                    {
                        var termGroups = TermGroupDao.LoadTermGroup(false);
                        GeneralSettingConfig.Reset();
                        var zoneDisplayName = GeneralSettingConfig.GetTimeZoneInforById((await GeneralSetting).TimeZoneId).DisplayName;
                        Dictionary<int, List<RMTermRuleAssociation>> ruleInfos = TermRuleAssociationDao.GetTermWithRule().GroupBy(t => t.TermId)
                            .ToDictionary(t => t.Key, v => v.OrderBy(r => r.RuleOrder).ToList());
                        var isJPMCOpen = KeyValueDao.GetValueByKey("JPMC_Customization") != null;
                        foreach (var termGroup in termGroups)
                        {
                            var termSets = await this.BuildTermSetsTreeAsync(DB.Model.TermSetType.Business, termGroup.UniqueId, termGroup.Name);
                            termSets = termSets.OrderBy(ts => ts.Name).ToList();
                            if (termSets != null && termSets.Count > 0)
                            {
                                foreach (RMTermSet termset in termSets)
                                {
                                    await ConvertRMTermToArrayAsync(termset.RMTerms, termGroup.Name, termset.Name, zoneDisplayName, ruleInfos, termDatas , isJPMCOpen, hasUpgradeTeams);
                                }
                                //var deafultTermSet = termSets[0];
                                //ConvertRMTermToArray(deafultTermSet.RMTerms, termGroup.Name, deafultTermSet.Name, ref termDatas);
                            }
                            else
                            {
                                logger.Info("{0} has no term set.", termGroup.Name);
                            }
                        }
                    }

                    var ruleDatas = await RuleManagerService.ConvertRuleInfosToListAsync();
                    bool isJpmcOpem = KeyValueDao.GetValueByKey("JPMC_Customization") != null;
                    ExportAddition exportAddition = new ExportAddition(); ;
                    List<string> termAdditionalColumn = new();
                    if (isJpmcOpem)
                    {
                        termAdditionalColumn.Add("RM_TM_AdvanceSetting");
                        exportAddition.ConditionArray = new string[] { "ListIn" };
                    }
                    if(hasUpgradeTeams)
                    {
                        termAdditionalColumn.Add("RM_TM_Retension_Teams_Label");
                        exportAddition.HasUpgradeTeams = true;
                    }
                    if (AccountUtility.IsSupportRecordLabel())
                    {
                        exportAddition.IsSupportRecordLabelFunction = true;
                    }
                    exportAddition.TermColumArray = termAdditionalColumn.ToArray();
                    using (new RA.Common.PerformanceScope("termmanagement.exportterms.buildfile"))
                    {
                        ReportUtil.CreateTermsAndRulesSheets(reportFilePath, ruleDatas, termDatas, exportAddition);
                    }
                        
                }
            }
            catch (Exception e)
            {
                logger.Error("generate term info report error Info:{0},{1}", e.Message, e.StackTrace);
            }

            //string[][] datas = null;
            //int countOfOneSheet = 65535;
            //PrepareTermInfos();
            //List<TermInfoWithRule> tempTermInfoList = new List<TermInfoWithRule>();
            //int termInfoTotalCount = termInfos == null ? 0 : termInfos.Count;
            //if (!Directory.Exists(folderPath))
            //{
            //    Directory.CreateDirectory(folderPath);
            //}
            //try
            //{
            //    if (termInfoTotalCount > 0)
            //    {
            //        for (int i = 1; i < termInfos.Count + 1; i++)
            //        {
            //            if (tempTermInfoList.Count > 0 && tempTermInfoList.Count % countOfOneSheet == 0)
            //            {
            //                tempTermInfoList.Add(termInfos[i - 1]);
            //                tempTermInfoList = InsertDataToExcel(reportFilePath, tempTermInfoList, i, countOfOneSheet, sheetName);

            //            }
            //            else
            //            {
            //                tempTermInfoList.Add(termInfos[i - 1]);
            //            }
            //        }
            //        if (tempTermInfoList.Count > 0)
            //        {
            //            InsertDataToExcel(reportFilePath, tempTermInfoList, termInfoTotalCount, countOfOneSheet, sheetName);
            //        }

            //        Dictionary<ExcelHeadColumn, List<string>> infos = GetMergeCellRangeInfo();
            //        if (infos.Count > 0)
            //        {
            //            ReportUtil.MergeCells(reportFilePath, sheetName, infos);
            //        }
            //    }
            //    else
            //    {
            //        datas = new string[1][];
            //        datas[0] = new string[] { I18NEntity.GetString("RM_Common_NoReport") };
            //        ReportUtil.CreateExcel(reportFilePath, sheetName + tempTermInfoList.Count / countOfOneSheet, datas);
            //    }

            //}
            //catch (Exception e)
            //{
            //    logger.Error("generate term info report error Info:{0},{1}", e.Message, e.StackTrace);
            //}
        }

        public List<TermInfoWithRule> InsertDataToExcel(string reportFilePath, List<TermInfoWithRule> tempTermInfoList, int currentInsertCount, int maxCountOfOneSheet, string sheetName)
        {
            if (termInfos != null)
            {
                string[][] datas = new string[termInfos.Count() + 1][];
                datas = AssembleTermInfoHeaderTittle(datas);
                datas = ConvertTermInfoToArray(tempTermInfoList, datas);
                if (currentInsertCount <= maxCountOfOneSheet)
                {
                    ReportUtil.CreateExcel(reportFilePath, sheetName, datas);
                    tempTermInfoList.Clear();
                }
                else
                {
                    ReportUtil.InsertWorksheet(reportFilePath, sheetName + tempTermInfoList.Count / maxCountOfOneSheet, datas);
                    tempTermInfoList.Clear();
                }
            }
            return tempTermInfoList;
        }
        public string[][] AssembleTermInfoHeaderTittle(string[][] datas)
        {
            datas[0] = new string[18];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_BCSTermName");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermDes");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus");
            datas[0][3] = I18NEntity.GetString("RM_JS_JM_JobType_EnforceRetention");
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_ReportColumn_AppliedRuleName");
            datas[0][5] = I18NEntity.GetString("RM_JS_RC_ReportColumn_RuleDes");
            datas[0][6] = I18NEntity.GetString("RM_JS_RDM_Rule_ObjectLevel");
            datas[0][7] = I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title");
            datas[0][8] = I18NEntity.GetString("RM_JS_RC_Common_ReportType");
            datas[0][9] = I18NEntity.GetString("RM_JS_Rule_Detail_Criteria");
            datas[0][10] = I18NEntity.GetString("RM_JS_TM_RuleActionLabel");
            datas[0][11] = I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeDeclaredFile");
            datas[0][12] = I18NEntity.GetString("RM_RDM_CreateRule_Options_EnableApproval");
            datas[0][13] = I18NEntity.GetString("RM_JS_MA_Grid_SendEmailRecordOwner");
            datas[0][14] = I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_RecordsOwner");
            datas[0][15] = I18NEntity.GetString("RM_JS_Rule_Detail_EXSP");
            datas[0][16] = I18NEntity.GetString("RM_JS_Rule_Detail_EXFormat");
            datas[0][17] = I18NEntity.GetString("RM_RC_Audit_WhetherDeleteRelatedRecord");
            return datas;
        }
        public string[][] ConvertTermInfoToArray(List<TermInfoWithRule> termInfos, string[][] datas)
        {
            int rowCount = 1;
            foreach (TermInfoWithRule termInfo in termInfos)
            {
                datas[rowCount] = new string[18];
                datas[rowCount][0] = termInfo.TermName;
                datas[rowCount][1] = termInfo.TermDescription;
                datas[rowCount][2] = termInfo.TermStatus;
                datas[rowCount][3] = termInfo.EnforceRetention;
                datas[rowCount][4] = !string.IsNullOrEmpty(termInfo.RuleName) ? termInfo.RuleName : "";
                datas[rowCount][5] = !string.IsNullOrEmpty(termInfo.RuleDescription) ? termInfo.RuleDescription : "";
                datas[rowCount][6] = !string.IsNullOrEmpty(termInfo.RuleLevel) ? termInfo.RuleLevel : "";
                datas[rowCount][7] = !string.IsNullOrEmpty(termInfo.DisposalClass) ? termInfo.DisposalClass : "";
                datas[rowCount][8] = GetExcelContent(termInfo, ExcelColumnType.SourceType);
                datas[rowCount][9] = GetExcelContent(termInfo, ExcelColumnType.Criteria);
                datas[rowCount][10] = GetExcelContent(termInfo, ExcelColumnType.Action);
                datas[rowCount][11] = !string.IsNullOrEmpty(termInfo.DeleteRecords) ? termInfo.DeleteRecords : "";
                datas[rowCount][12] = GetExcelContent(termInfo, ExcelColumnType.EnableManualApproval);
                datas[rowCount][13] = GetExcelContent(termInfo, ExcelColumnType.SendEmail);
                datas[rowCount][14] = GetExcelContent(termInfo, ExcelColumnType.RecordOwner);
                datas[rowCount][15] = GetExcelContent(termInfo, ExcelColumnType.ExportSharePointContent);
                datas[rowCount][16] = GetExcelContent(termInfo, ExcelColumnType.ExportFormat);
                datas[rowCount][17] = GetExcelContent(termInfo, ExcelColumnType.IncludeRelatedRecord);
                rowCount++;
            }
            return datas;
        }
        public string ExportTypeToString(GCommon.Contract.StorageOptimization.Object.ExportTypeValue ExType)
        {
            string tempEnableExportType = string.Empty;
            switch (ExType)
            {
                case GCommon.Contract.StorageOptimization.Object.ExportTypeValue.Autonomy:
                     tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_Autonomy");
                    break;
                case GCommon.Contract.StorageOptimization.Object.ExportTypeValue.Concordance:
                     tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_Concordance");
                    break;
                case GCommon.Contract.StorageOptimization.Object.ExportTypeValue.EDRM:
                     tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_EDRM");
                    break;
                case GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO:
                     tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_VEO");
                    break;
                case GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA:
                     tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_NAA");
                    break;
                case GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA:
                    tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_NARA");
                    break;
                default:
                     tempEnableExportType = I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_None");
                    break;
            }
            return tempEnableExportType;
        }
        public async System.Threading.Tasks.Task PrepareTermInfosAsync()
        {
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup(false);
            foreach (var termGroup in termGroups)
            {
                List<RMTermSet> termSets = new List<RMTermSet>();
                termSets = await this.BuildTermSetsTreeAsync(DB.Model.TermSetType.BusinessTerm, termGroup.UniqueId, termGroup.Name);
                if (termSets.Count == 0)
                {
                    logger.Info("{0}[Term Group] has no term set ", termGroup.Name);
                }
                else
                {
                    foreach (var termSet in termSets)
                    {
                        if (termSet.RMTerms != null)
                        {
                            await this.ProcessTermsAsync(termSet.RMTerms);
                        }
                    }
                }
            }
        }
        public async Task<List<RMTermSet>> BuildTermSetsTreeAsync(DB.Model.TermSetType termSetType, Guid termGroupId, string termGroupName)
        {
            try
            {
                logger.Info("begin BuildTermSetsTree");
                List<RMTermSet> termSets = await TermSetDao.LoadTermSetAsync(termSetType, termGroupId);
                if (termSets.Count == 0)
                {
                    return termSets;
                }
                foreach (RMTermSet termSet in termSets)
                {
                    List<RMTerm> rootTerms = TermDao.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                    if (rootTerms.Count != 0)
                    {
                        termSet.RMTerms = rootTerms.Where(t => !t.IsRemoved).ToList();
                        foreach (var term in rootTerms)
                        {
                            term.FullPath = string.Format("{0}/{1}/{2}", termGroupName, termSet.Name, term.Name);
                            term.FullPathList = new List<string>() { termGroupName, termSet.Name, term.Name };
                            this.BuildTerm(term);
                        }
                    }
                }
                logger.Info("BuildTermSetsTree Complete.");
                return termSets;
            }
            catch (Exception e)
            {
                logger.Error("There are some error in BuildTermSetsTree {0}", e.ToString());
                return new List<RMTermSet>();
            }
        }
        private void BuildTerm(RMTerm term)
        {
            List<RMTerm> subTerms = TermDao.GetTermFromParentTerm(term);
            if (subTerms.Count != 0)
            {
                term.subTerms = subTerms.Where(t => !t.IsRemoved).ToList();
                foreach (RMTerm subTerm in subTerms)
                {
                    if (!subTerm.BreakInheritFromParent)
                    {
                        subTerm.EnforceRetention = term.EnforceRetention;
                        subTerm.SPRetentionLabel = term.SPRetentionLabel;
                        subTerm.EXORetentionLabel = term.EXORetentionLabel;
                        subTerm.OneDriveRetentionLabel = term.OneDriveRetentionLabel;
                        subTerm.TeamsRetentionLabel = term.TeamsRetentionLabel;
                    }
                    subTerm.FullPath = string.Format("{0}/{1}", term.FullPath, subTerm.Name);
                    subTerm.FullPathList = new List<string>(term.FullPathList)
                    {
                        subTerm.Name
                    };
                    this.BuildTerm(subTerm);
                }
            }
        }
        public async System.Threading.Tasks.Task ProcessTermsAsync(List<RMTerm> terms)
        {
            foreach (var term in terms)
            {
                //var isPermanentTerm = TermDao.GetTermPermanentByTermId(term.Id, false);
                //if (isPermanentTerm)
                //{
                //    TermInfoWithRule termInfo = new TermInfoWithRule();
                //    termInfo.TermName = term.FullPath;
                //    termInfo.TermDescription = HttpUtility.HtmlDecode(term.Description);
                //    termInfo.TermStatus = GetTermStatus(term);
                //    termInfo.Permanent = isPermanentTerm;
                //    termInfos.Add(termInfo);
                //    if (term.subTerms != null && term.subTerms.Count > 0)
                //    {
                //        this.ProcessTerms(term.subTerms);
                //    }
                //    continue;
                //}

                List<RMTermRuleAssociation> termRules = TermRuleAssociationDao.GetTermRuleInfoByTermid(term.Id);
                var parentInert = TermDao.GetParentInhertSetting(term.Id);
                var retention = parentInert == null ? term.EnforceRetention : parentInert.EnforceRetention;
                if (termRules.Count == 0)
                {
                    TermInfoWithRule termInfo = new TermInfoWithRule();
                    termInfo.TermName = term.FullPath;
                    termInfo.TermDescription = HttpUtility.HtmlDecode(term.Description);
                    termInfo.TermStatus = GetTermStatus(term);
                    termInfo.EnforceRetention = GetEnforceRetention(retention);
                    //termInfo.Permanent = isPermanentTerm;
                    termInfos.Add(termInfo);
                }
                else
                {
                    int count = 1;
                    foreach (var termRule in termRules)
                    {
                        //TermInfoWithRule termInfo = new TermInfoWithRule();
                        //if (count == 1)
                        //{
                        //    termInfo.TermName = term.FullPath;
                        //    termInfo.TermDescription = HttpUtility.HtmlDecode(term.Description);
                        //    termInfo.TermStatus = GetTermStatus(term);
                        //    termInfo.EnforceRetention = GetEnforceRetention(retention);
                        //    //termInfo.Permanent = isPermanentTerm;
                        //}
                        string ruleId = termRule.RuleId.ToString();
                        if (!ruleIds.Contains(ruleId))
                        {
                            try
                            {
                                RMRuleInfos ruleInfo = await RuleManagerService.LoadRuleAsync(ruleId);
                                ruleIds.Add(ruleId);
                                ruleInfos?.Add(ruleInfo);
                            }
                            catch (Exception ex)
                            {
                                if (count == 1)
                                {
                                    TermInfoWithRule termInfo1 = new TermInfoWithRule();
                                    termInfo1.TermName = term.FullPath;
                                    termInfo1.TermDescription = HttpUtility.HtmlDecode(term.Description);
                                    termInfo1.TermStatus = GetTermStatus(term);
                                    termInfo1.EnforceRetention = GetEnforceRetention(retention);
                                    termInfos.Add(termInfo1);
                                }
                                count++;
                                logger.Warn("export term get rule error:{0}", ex.Message);
                                continue;
                            }
                        }
                        if (ruleInfos != null && ruleInfos.Count > 0)
                        {
                            RMRuleInfos rule = ruleInfos.Where(r => r.RuleId.Equals(ruleId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            if (rule != null)
                            {
                                #region SP Rule Row record
                                if (rule.IsSpSource)
                                {
                                    TermInfoWithRule spRuleRowInfo = new TermInfoWithRule();
                                    if (count == 1)
                                    {
                                        spRuleRowInfo.TermName = term.FullPath;
                                        spRuleRowInfo.TermDescription = HttpUtility.HtmlDecode(term.Description);
                                        spRuleRowInfo.TermStatus = GetTermStatus(term);
                                        spRuleRowInfo.EnforceRetention = GetEnforceRetention(retention);
                                    }
                                    spRuleRowInfo.RuleName = rule.RuleName;
                                    spRuleRowInfo.RuleDescription = rule.Description;
                                    spRuleRowInfo.RuleLevel = RuleManagerService.ConvertPolicyLevelToI18NStr(rule.RuleLevel);
                                    spRuleRowInfo.DisposalClass = !string.IsNullOrEmpty(rule.DisposalClass) ? rule.DisposalClass : "";
                                    spRuleRowInfo.IsSPSource = true;
                                    if (rule.MoveToRecordCenterSettings != null && rule.MoveToRecordCenterSettings.DestinationLocation != null)
                                    {
                                        if (!rule.MoveToRecordCenterSettings.DelaredRecord)
                                        {
                                            rule.ArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord") + "; " + I18NEntity.GetString(AccountUtility.IsSupportRecordLabel() ? "RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabel" : "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord");
                                        }
                                        else
                                        {
                                            rule.ArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord");
                                        }
                                    }
                                    spRuleRowInfo.Action = rule.ArchiverActions;
                                    StringBuilder tempCretias = new StringBuilder();
                                    if (!rule.RuleCretias.IsNullOrEmpty())
                                    {
                                        for (int i = 0; i < rule.RuleCretias.Count; i++)
                                        {
                                            tempCretias.Append(rule.RuleCretias[i] + "\n");
                                        }
                                    }
                                    spRuleRowInfo.Criteria = tempCretias.Append(rule.FilterCombineMode).ToString();
                                    spRuleRowInfo.ExportSharePointContent = rule.EnableExport ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    spRuleRowInfo.ExportFormat = rule.EnableExport ? ExportTypeToString(rule.ExportInfo.exportType) : "";
                                    spRuleRowInfo.DeleteRecords = rule.DeleteRecords ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    spRuleRowInfo.IncludeDeleteRecordLabel = rule.IncludeDeleteRecordLabel ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    spRuleRowInfo.DeleteSiteCollectionToRecycleBin = rule.IsDeleteSiteCollectionToRecycleBin() ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    spRuleRowInfo.EnableManualApproval = rule.EnableManualApproval ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    spRuleRowInfo.SendEmailRecordOwner = rule.IsSendEmailToOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    spRuleRowInfo.IncludeRelatedRecord = rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    if (rule.Users != null)
                                    {
                                        foreach (var user in rule.Users)
                                        {
                                            if (!string.IsNullOrEmpty(spRuleRowInfo.RecordOwner))
                                                spRuleRowInfo.RecordOwner += "; " + user.DisplayName;
                                            else
                                            {
                                                spRuleRowInfo.RecordOwner += user.DisplayName;
                                            }
                                        }
                                    }
                                    termInfos.Add(spRuleRowInfo);
                                }
                                #endregion

                                #region EXO Rule Row record
                                if (rule.IsExoSource)
                                {
                                    TermInfoWithRule emailRuleRowInfo = new TermInfoWithRule();
                                    if (!rule.IsSpSource)
                                    {
                                        if (count == 1)
                                        {
                                            emailRuleRowInfo.TermName = term.FullPath;
                                            emailRuleRowInfo.TermDescription = HttpUtility.HtmlDecode(term.Description);
                                            emailRuleRowInfo.TermStatus = GetTermStatus(term);
                                            emailRuleRowInfo.EnforceRetention = GetEnforceRetention(retention);
                                        }
                                        emailRuleRowInfo.RuleName = rule.RuleName;
                                        emailRuleRowInfo.RuleDescription = rule.Description;
                                        emailRuleRowInfo.RuleLevel = RuleManagerService.ConvertPolicyLevelToI18NStr(rule.RuleLevel);
                                        emailRuleRowInfo.DisposalClass = !string.IsNullOrEmpty(rule.DisposalClass) ? rule.DisposalClass : "";
                                    }
                                    emailRuleRowInfo.IsEXOSource = true;
                                    emailRuleRowInfo.Action = rule.EXORule.ArchiverActions;
                                    StringBuilder tempEmailCretias = new StringBuilder();
                                    if (!rule.EXORule.RuleCretias.IsNullOrEmpty())
                                    {
                                        for (int i = 0; i < rule.EXORule.RuleCretias.Count; i++)
                                        {
                                            tempEmailCretias.Append(rule.EXORule.RuleCretias[i] + "\n");
                                        }
                                    }
                                    emailRuleRowInfo.Criteria = tempEmailCretias.Append(rule.EXORule.FilterCombineMode).ToString();
                                    emailRuleRowInfo.ExportSharePointContent = rule.EXORule.EnableExport ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    emailRuleRowInfo.ExportFormat = rule.EXORule.EnableExport ? ExportTypeToString(rule.EXORule.ExportInfo.exportType) : "";
                                    emailRuleRowInfo.EnableManualApproval = rule.EXORule.EnableManualApproval ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    emailRuleRowInfo.SendEmailRecordOwner = rule.EXORule.IsSendEmailToOwner ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    //emailRuleRowInfo.IncludeRelatedRecord = rule.EXORule.RelatedRecordOption == RelatedRecordOption.Both ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                                    if (rule.EXORule.Users != null)
                                    {
                                        foreach (var user in rule.EXORule.Users)
                                        {
                                            if (!string.IsNullOrEmpty(emailRuleRowInfo.RecordOwner))
                                                emailRuleRowInfo.RecordOwner += "; " + user.DisplayName;
                                            else
                                            {
                                                emailRuleRowInfo.RecordOwner += user.DisplayName;
                                            }
                                        }
                                    }
                                    termInfos.Add(emailRuleRowInfo);
                                }
                                #endregion
                            }
                        }
                        count++;
                    }
                }
                if (term.subTerms != null && term.subTerms.Count > 0)
                {
                    await this.ProcessTermsAsync(term.subTerms);
                }
            }
        }


        private string GetTermStatus(RMTerm term)
        {
            string status;
            long utcNow = DateTime.UtcNow.Ticks;
            if (term.IsDeprecated || (term.TermExpirationFrom > 0 && utcNow < term.TermExpirationFrom) || (term.TermExpirationTo > 0 && utcNow > term.TermExpirationTo))
            {
                status = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Retired");
            }
            else
            {
                status = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Avaliable");
            }

            return status;
        }

        private string GetEnforceRetention(int retention)
        {
            string msg = string.Empty;
            if (retention == 0)
            {
                msg = I18NEntity.GetString("RM_JS_Common_No");
            }
            else
            {
                msg = I18NEntity.GetString("RM_JS_Common_Yes");
            }
            return msg;
        }

        public Dictionary<ExcelHeadColumn, List<string>> GetMergeCellRangeInfo()
        {
            Dictionary<ExcelHeadColumn, List<string>> cellMergeRange = new Dictionary<ExcelHeadColumn, List<string>>();
            if (termInfos != null && termInfos.Count > 0)
            {
                initMergeCellRange(cellMergeRange, MergeCellConditionType.TermName, new List<ExcelHeadColumn> { ExcelHeadColumn.A, ExcelHeadColumn.B, ExcelHeadColumn.C, ExcelHeadColumn.D });
                initMergeCellRange(cellMergeRange, MergeCellConditionType.RuleName, new List<ExcelHeadColumn> { ExcelHeadColumn.E, ExcelHeadColumn.F, ExcelHeadColumn.G, ExcelHeadColumn.H });
            }
            return cellMergeRange;
        }
        public void initMergeCellRange(Dictionary<ExcelHeadColumn, List<string>> cellMergeRange, MergeCellConditionType condition, List<ExcelHeadColumn> mergedColumns)
        {
            List<string> cellIndexs = new List<string>();
            int curIndex = 0;
            int i = 0;
            foreach (TermInfoWithRule termInfo in termInfos)
            {
                bool result = false;
                switch (condition)
                {
                    case MergeCellConditionType.TermName:
                        result = !string.IsNullOrEmpty(termInfo.TermName);
                        break;
                    case MergeCellConditionType.RuleName:
                        result = !string.IsNullOrEmpty(termInfo.RuleName) || (/*string.IsNullOrEmpty(termInfo.Criteria) &&*/ string.IsNullOrEmpty(termInfo.Criteria)); //Quality Issue
                        break;
                }
                if (result)
                {
                    curIndex = i;
                    cellIndexs.Add(i.ToString());
                }
                else
                {
                    cellIndexs.Add(curIndex.ToString());
                }
                i++;
            }
            var newCellIndexs = cellIndexs.Distinct();
            foreach (var newCellIndex in newCellIndexs)
            {
                int startIndex = cellIndexs.IndexOf(newCellIndex) + 2;
                int endIndex = cellIndexs.LastIndexOf(newCellIndex) + 2;
                if (startIndex != endIndex)
                {
                    foreach (var item in mergedColumns)
                    {
                        if (!cellMergeRange.ContainsKey(item))
                        {
                            cellMergeRange.Add(item, new List<string>() { });
                        }
                        cellMergeRange[item].Add(string.Format("{0},{1}", startIndex, endIndex));
                    }
                }
            }
        }

        public async Task<string> GetTaxonomyTermAsync(string typeName, string treeNodeId, int pageIndex, int pageCount, string groupId, int SettingType, bool needCheckPermission = false)
        {
            logger.Debug(string.Format("type:[{0}],nodeId:[{1}],pageIndex:[{2}],pageCount:[{3}]", typeName, treeNodeId, pageIndex, pageCount));

            string strResult = string.Empty;
            switch (typeName)
            {
                case "TermGroup":
                    if (needCheckPermission)
                    {
                        var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        strResult = GetJsonStrByObj(TermSetDao.GetTermSetsByGroupId(Guid.Parse(treeNodeId), DB.Model.TermSetType.Business, pageIndex, pageCount, new FilterTermObjOption { userAndGroupUserIds = userAndGroupIds, NeedCheckPermission = true }));
                    }
                    else
                    {
                        strResult = GetJsonStrByObj(TermSetDao.GetTermSetsByGroupId(Guid.Parse(treeNodeId), DB.Model.TermSetType.Business, pageIndex, pageCount));
                    }
                    break;
                case "TermSet":
                    strResult = GetJsonStrByObj(TermDao.GetTermFromTermSet(Convert.ToInt32(treeNodeId), pageIndex, pageCount));
                    break;
                case "Term":
                    strResult = GetJsonStrByObj(TermDao.GetTermFromParentTerm(Convert.ToInt32(treeNodeId), pageIndex, pageCount));
                    break;
                default:
                    strResult = GetJsonStrByObj(await GetTermGroupsAsync(groupId, SettingType, needCheckPermission));
                    break;
            }
            return strResult;
        }
        private async Task<List<RMTermGroup>> GetTermGroupsAsync(string groupId, int SettingType, bool needCheckPermission = false)
        {
            List<RMTermGroup> rmTermGroups = new List<RMTermGroup>();
            if (!string.IsNullOrEmpty(groupId))
            {
                if (SettingType == 1)
                {
                    Guid termSetId = Guid.Empty;// setting.TermSetId
                    var termGroup = GetTermGroupByDefaultTermSetId(termSetId);
                    if (termGroup != null && !termGroup.IsRemoved)
                    {
                        rmTermGroups.Add(termGroup);
                    }
                    return rmTermGroups;
                }
                if (needCheckPermission)
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    rmTermGroups = TermGroupDao.LoadTermGroup(false, new FilterTermObjOption { userAndGroupUserIds = userAndGroupIds, NeedCheckPermission = true });
                }
                else
                {
                    rmTermGroups = TermGroupDao.LoadTermGroup(false);
                }
                
            }
            else
            {
                if (needCheckPermission)
                {
                    List<Guid> groupUniqueIds = new List<Guid>();
                    SecurityTermPermissionDto termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new QuerySecurityTermObjDto
                    {
                        UserId = TenantLocalValue.LogonUserId,
                        Level = SecurityTermLevel.TermGroup,
                    });
                    if (termPermissionInfo.TermPermissionType == TermPermissionMethod.All)
                    {
                        rmTermGroups = TermGroupDao.LoadGroupsData();
                    }
                    else
                    {
                        groupUniqueIds = termPermissionInfo.TermObjIds;
                        List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        rmTermGroups = TermGroupDao.LoadGroupsData(false, groupUniqueIds, userAndGroupUserIds);
                    }
                }
                else
                {
                    rmTermGroups = TermGroupDao.LoadGroupsData();
                }
            }
            return rmTermGroups;
        }

        public RMTermInfo GetDefaultTermByMailBox(string mailBox)
        {
            RMTermInfo term = null;

            //DAOAPIClientV1 client = new DAOAPIClientV1();
            //var nodeInfo = client.GetExchangeNodeByMailBox(mailBox);
            var nodeInfo = RABrowserClient.GetExchangeNodeByMailBox(mailBox);
            if (nodeInfo != null)
            {
                var setting = GetEXOSetting(nodeInfo);
                if (setting == null)
                {
                    logger.Warn("default term mailbox have no setting:{0}.", mailBox);
                    throw new Exception("setting not found.");
                }
                if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                {
                    term = new RMTermInfo()
                    {
                        UniqueId = setting.DefaultTermId,
                        Name = setting.DefaultTermName
                    };
                }

            }
            else
            {
                throw new Exception($"mailbox not found: {TenantLocalValue.LogonGroupId}, {mailBox}.");
            }
            return term;
        }

        public Contract.RMReport.TermTreeNode GetTermTreeByMailBox(string mailBox)
        {
            Contract.RMReport.TermTreeNode treeNode = null;

            //DAOAPIClientV1 client = new DAOAPIClientV1();
            //var nodeInfo = client.GetExchangeNodeByMailBox(mailBox);
            var nodeInfo = RABrowserClient.GetExchangeNodeByMailBox(mailBox);
            if (nodeInfo != null)
            {
                var setting = GetEXOSetting(nodeInfo);
                if (setting == null)
                {
                    logger.Warn("mailbox have no setting:{0}.", mailBox);
                    return treeNode;
                }
                treeNode = GetTermTreeBySetting(setting);
            }
            return treeNode;
        }

        private Contract.RMReport.TermTreeNode GetTermTreeBySetting(RMExchangeOnlineSetting setting)
        {
            Contract.RMReport.TermTreeNode termTree = null;
            if (setting.TermId.Equals(Guid.Empty))
            {
                //termset scope
                logger.Info("get by term set Id:{0}", setting.TermSetId);
                var termset = TermSetDao.GetRMTermSetByGuid(setting.TermSetId);
                if (termset != null && !termset.IsRemoved)
                {
                    termTree = TermDao.GetRATermSetTree(termset.UniqueId);
                }
                else
                {
                    logger.Info("get by term set, term set is not avaible:{0}", setting?.TermSetId);
                }
            }
            else
            {
                //term scope
                logger.Info("get by term Id:{0}", setting.TermId);
                var term = TermDao.GetRMTermByGuId(setting.TermId);
                if (term != null && !term.IsRemoved)
                {
                    termTree = TermDao.GetSubTermTreeNode(term, Guid.Empty);
                }
                else
                {
                    logger.Info("get by term set, term set is not avaible:{0}", setting?.TermSetId);
                }
               
            }
            
            return termTree;
        }

        private RMExchangeOnlineSetting GetEXOSetting(GCommon.Contract.Tree.Object.ExchangeOnlineTreeNodeDto treeNode)
        {
            using (var performance = new PerformanceScope("EXO.GetEXOSetting"))
            {
                var mailboxId = Guid.Empty;
                var scopeId = Guid.Parse(treeNode.ID);
                var groupId = Guid.Parse(treeNode.ParentId);
                if (treeNode.Level != NodeLevel.ExchangeOnlineMailbox && treeNode.Level != NodeLevel.ExchangeOnlineO365Group)
                {
                    mailboxId = new Guid(TreeManagement.GetMailboxNode(treeNode).ID);
                }

                logger.Info("Get setting from db.");
                var setting = EXOSettingDao.GetSettingInfoByScope(groupId, mailboxId, new Guid(treeNode.ID));

                if (setting == null)
                {
                    // get group setting
                    setting = EXOSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
                    if (setting == null)
                    {
                        logger.Warn("Setting not available {0}", treeNode.ID);
                    }

                }
                logger.Info("Setting Info Id:{0}, Name:{1}", setting?.Id, setting?.Name);
                return setting;
            }
        }

        public async System.Threading.Tasks.Task ConvertRMTermToArrayAsync(List<RMTerm> terms, string termGroupName, string termSetName, string zoneDisplayName, Dictionary<int, List<RMTermRuleAssociation>> ruleInfos, List<string[]> termDatas,bool isJPMCOpen, bool hasUpgradeTeams)
        {
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    var termInfos = await BuildTermInfosAsync(term, termGroupName, termSetName, zoneDisplayName, ruleInfos, isJPMCOpen, hasUpgradeTeams);
                    if (termInfos != null)
                    {
                        termDatas.Add(termInfos);
                        if (term.subTerms != null && term.subTerms.Count > 0)
                        {
                            await ConvertRMTermToArrayAsync(term.subTerms, termGroupName, termSetName, zoneDisplayName, ruleInfos, termDatas, isJPMCOpen, hasUpgradeTeams);
                        }
                    }
                }
            }
        }

        public async Task<string[]> BuildTermInfosAsync(RMTerm term, string termGroupName, string termSetName, string zoneDisplayName, Dictionary<int, List<RMTermRuleAssociation>> ruleInfos,bool isJPMCOpen, bool hasUpgradeTeams)
        {
            string trueStr = "TRUE";
            string falseStr = "FALSE";
            List<string> termInfos = new List<string>();
            termInfos.Add(termGroupName);
            termInfos.Add(termSetName);
            //var termFullPath = term.FullPath;
            //string[] termNodes = termFullPath.Split('/');
            string[] termNodes = term.FullPathList.ToArray();
            if (termNodes.Length > 7)
            {
                //over 5 layer term
                return null;
            }
            for (int i = 2; i < termNodes.Length; i++)
            {
                termInfos.Add(termNodes[i]);
            }
            for (int j = 0; j < 5 - (termNodes.Length - 2); j++)
            {
                termInfos.Add("");
            }
            termInfos.Add(term.Description);
            termInfos.Add(term.IsRootTerm ? "" : (!term.BreakInheritFromParent ? trueStr : falseStr));

            var rList = TermRuleAssociationDao.GetTermRuleInfoByTermid(term.Id, ruleInfos);

            var strRuleNames = "";
            if (rList != null && rList.Count > 0)
            {
                var ruleNames = rList.Select(r => r.RuleName).ToList();
                strRuleNames = string.Join("; ", ruleNames);
            }
            termInfos.Add(strRuleNames);

            #region Enforce Retention
            var IsEnforceRetention = term.EnforceRetention != 0;
            termInfos.Add(IsEnforceRetention ? trueStr : falseStr);
            string spLabelName = "";
            string exoLabelName = "";
            string oneDriveLabelName = "";
            string teamsLabelName = "";

            var allEnforceRetentionTypes = new List<EnforceRetentionType> { EnforceRetentionType.SharePoint, EnforceRetentionType.Exchange, EnforceRetentionType.OneDrive, EnforceRetentionType.Teams };
            var rententionSourceTypes = new List<string>();
            if ((term.EnforceRetention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint)
            {
                    spLabelName = term.SPRetentionLabel;
                rententionSourceTypes.Add(I18NEntity.GetString("RM_TM_Excel_SharePointOnline"));
            }
            if ((term.EnforceRetention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange)
            {
                    exoLabelName = term.EXORetentionLabel;
                rententionSourceTypes.Add(I18NEntity.GetString("RM_JS_SPS_TabLabel_EXO"));
            }
            if ((term.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
            {
                oneDriveLabelName = term.OneDriveRetentionLabel;
                rententionSourceTypes.Add(I18NEntity.GetString("RM_JS_SPS_TabLabel_OneDrive"));
            }
            if(hasUpgradeTeams && (term.EnforceRetention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams)
            {
                teamsLabelName = term.TeamsRetentionLabel;
                rententionSourceTypes.Add(I18NEntity.GetString("RM_JS_SPS_TabLabel_Teams"));
            }
            if (rententionSourceTypes.Count == 1)
            {
                termInfos.Add(rententionSourceTypes.FirstOrDefault());
            }
            else if(rententionSourceTypes.Any())
            {
                termInfos.Add("Any");
            }
            else
            {
                termInfos.Add("");
            }
            termInfos.Add(spLabelName);
            termInfos.Add(exoLabelName);
            termInfos.Add(oneDriveLabelName);
            #endregion

            #region Term Activation Settings
            if (!term.IsDeprecated)
            {
                if (term.TermExpirationFrom == 0 && term.TermExpirationTo == 0)
                {
                    termInfos.Add("Always active");
                    fillNullCellValue(ref termInfos, 3);
                }
                else if (term.TermExpirationFrom == 0 && term.TermExpirationTo != 0)
                {
                    termInfos.Add("Retire after");
                    termInfos.Add(await GetStrDateTimeAsync(term.TermExpirationTo));
                    termInfos.Add("");
                    termInfos.Add(zoneDisplayName);
                }
                else if (term.TermExpirationFrom != 0 && term.TermExpirationTo == 0)
                {
                    termInfos.Add("Take effect from");
                    termInfos.Add(await GetStrDateTimeAsync(term.TermExpirationFrom));
                    termInfos.Add("");
                    termInfos.Add(zoneDisplayName);
                }
                else
                {
                    termInfos.Add("Active from...to...");
                    termInfos.Add(await GetStrDateTimeAsync(term.TermExpirationFrom));
                    termInfos.Add(await GetStrDateTimeAsync(term.TermExpirationTo));
                    termInfos.Add(zoneDisplayName);
                }
            }
            else
            {
                fillNullCellValue(ref termInfos, 4);
            }
            #endregion
            #region Term Advince Settings
            //jpmc-只有开启时才会导出Advince settings
            if (isJPMCOpen)
            {
                termInfos.Add(term.AdvanceSettings);
            }
            #endregion
            if(hasUpgradeTeams)
            {
                termInfos.Add(teamsLabelName);
            }
            return termInfos.ToArray();
        }
        private void fillNullCellValue(ref List<string> sourceArrary, int repeat)
        {
            if (sourceArrary != null)
            {
                for (int i = 0; i < repeat; i++)
                {
                    sourceArrary.Add("");
                }
            }
        }
        private async Task<string> GetStrDateTimeAsync(long ticks)
        {
            if (0 == ticks)
            {
                return "";
            }
            var dt = DateTimeUtil.ConvertTimeFromUtc(ticks, await GeneralSetting);
            return dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
        }
        #endregion
        public string GetExcelContent(TermInfoWithRule termInfo, ExcelColumnType colType)
        {
            StringBuilder sb = new StringBuilder();
            switch (colType)
            {
                case ExcelColumnType.Criteria:
                    sb.Append(termInfo.Criteria);
                    break;
                case ExcelColumnType.Action:
                    sb.Append(termInfo.Action);
                    break;
                case ExcelColumnType.SourceType:
                    if (termInfo.IsSPSource || termInfo.IsEXOSource)
                    {
                        sb.Append(termInfo.IsSPSource ? I18NEntity.GetString("RM_JS_Common_ReportType_SharePoint") : I18NEntity.GetString("RM_JS_SPS_TabLabel_EXO"));
                    }
                    break;
                case ExcelColumnType.EnableManualApproval:
                    sb.Append(termInfo.EnableManualApproval);
                    break;
                case ExcelColumnType.SendEmail:
                    sb.Append(termInfo.SendEmailRecordOwner);
                    break;
                case ExcelColumnType.RecordOwner:
                    sb.Append(termInfo.RecordOwner);
                    break;
                case ExcelColumnType.ExportSharePointContent:
                    sb.Append(termInfo.ExportSharePointContent);
                    break;
                case ExcelColumnType.ExportFormat:
                    sb.Append(termInfo.ExportFormat);
                    break;
                case ExcelColumnType.IncludeRelatedRecord:
                    sb.Append(termInfo.IncludeRelatedRecord);
                    break;
            }
            return sb.ToString();
        }

        public void CreateExportStatusRecord(Guid uniqueId)
        {
            if (uniqueId != Guid.Empty)
            {
                try
                {
                    RMExportTermsWithRulesDao.CreateExportRecord(uniqueId);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to create export term and rules flag. error:{ex.Message}");
                }
            }
        }

        public void UpdateExportStatus(Guid uniqueId, ExportTermsWithRulesStatus status)
        {
            try
            {
                RMExportTermsWithRulesDao.UpdateExportRecordStatus(uniqueId, status);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to update export term and rules flag status. error:{ex.Message}");
            }
        }

        public ExportTermsWithRulesStatus CheckExportStatus(Guid uniqueId)
        {
            return RMExportTermsWithRulesDao.CheckExportStatus(uniqueId);
        }

        public string GetTermTreeForSecurityGroup(QueryTermObjDto queryDto)
        {
            var result = "";
            var mapped = RMSecurityGroupDao.GetMappedTermByOtherGroups(queryDto.GroupId);
            var mappedGroups = mapped.Where(m => m.Level == SecurityTermLevel.TermGroup).Select(m => m.TermObjId);
            var mappedSets = mapped.Where(m => m.Level == SecurityTermLevel.TermSet).Select(m => m.TermObjId);
            switch (queryDto.ParentType)
            {
                case RMTermType.Root:
                    var isExistMappedAll = mapped.Any(m => m.Level == SecurityTermLevel.All);
                    if (isExistMappedAll)
                    {
                        return result;
                    }
                    var dbTermGroups = TermGroupDao.LoadTermGroup(false);
                    dbTermGroups = dbTermGroups.Where(g => !mappedGroups.Contains(g.UniqueId)).ToList();
                    var termGroupItems = dbTermGroups.ConvertAll(t => Convert2SecurityTermInfo(t));
                    foreach (var termGroupItem in termGroupItems)
                    {
                        var dbTermSets = TermSetDao.GetRMTermSetsByGroupUniqueId(termGroupItem.UniqueId);
                        dbTermSets = dbTermSets.Where(g => !mappedSets.Contains(g.UniqueId)).ToList();
                        var termSetItems = dbTermSets.ConvertAll(t => Convert2SecurityTermInfo(t));
                        if (termSetItems.Count > 0)
                        {
                            termGroupItem.SubTerms = termSetItems;
                            termGroupItem.SubTermCount = termSetItems.Count;
                            termGroupItem.IsLoaded = true;
                        }
                        termGroupItem.SubPerSize = 10;
                    }
                    result =  GetJsonStrByObj(new QueryTermObjResultDto
                    {
                        TermObjItems = termGroupItems
                    });
                    break;
                case RMTermType.TermGroup:
                    var termSets = TermSetDao.GetRMTermSetsByGroupUniqueId(Guid.Parse(queryDto.ParentId));
                    termSets = termSets.Where(g => !mappedSets.Contains(g.UniqueId)).ToList();
                    result = GetJsonStrByObj(new QueryTermObjResultDto
                    {
                        TermObjItems = termSets.ConvertAll(t => Convert2SecurityTermInfo(t))
                    });
                    break;
                default:
                    break;
            }
            return result;
        }

        private SecurityTermInfo Convert2SecurityTermInfo(object termObj)
        {
            if (termObj is RMTermGroup)
            {
                var termGroup = termObj as RMTermGroup;
                return new SecurityTermInfo
                {
                    Id = termGroup.Id,
                    UniqueId = termGroup.UniqueId,
                    ParentId = Guid.Empty,
                    Name = termGroup.Name,
                    Type = RMTermType.TermGroup,
                    SubTermCount = termGroup.subTermCount
                };
            }
            else if (termObj is RMTermSet)
            {
                var termSet = termObj as RMTermSet;
                return new SecurityTermInfo
                {
                    Id = termSet.Id,
                    UniqueId = termSet.UniqueId,
                    ParentId = termSet.TermGroupId,
                    Name = termSet.Name,
                    Type = RMTermType.TermSet
                };
            }
            return null;
        }

        private SecurityRuleInfo Convert2SecurityRuleInfo(RMRuleContainer ruleContainer)
        {
            return new SecurityRuleInfo
            {
                Id = ruleContainer.Id,
                UniqueId = ruleContainer.ContainerId,
                ParentId = Guid.Empty,
                Name = I18NEntity.GetString(ruleContainer.Name),
                Type = RMRuleType.RuleContainer,
            };
        }

        private SecurityRuleInfo Convert2SecurityRuleInfo(RMRule rule, Guid parentId)
        {
            return new SecurityRuleInfo
            {
                Id = rule.Id,
                UniqueId = rule.RuleId,
                Name = rule.RuleName,
                Type = RMRuleType.Rule,
                ParentId = parentId
            };
        }

        public SecurityTermInfo BuildSecurityTermTree(SecurityTermInfo dbRootNode, int groupId)
        {
            var rootNode = SecurityGroupManagementService.GetSecurityTermRootNode();
            try
            {
                if (dbRootNode.IsChecked)
                {
                    //select all termgroups
                    rootNode.IsChecked = true;
                }
                else
                {
                    int pageSize = 10;
                    var currentGroupMapped = RMSecurityGroupDao.GetMappedTermByGroup(groupId);
                    var mapped = RMSecurityGroupDao.GetMappedTermByOtherGroups(groupId);
                    //var isExistMappedAll = mapped.Any(m => m.Level == SecurityTermLevel.All);
                    //if (isExistMappedAll)
                    //{

                    //}
                    var mappedGroups = mapped.Where(m => m.Level == SecurityTermLevel.TermGroup).Select(m => m.TermObjId);
                    var mappedSets = mapped.Where(m => m.Level == SecurityTermLevel.TermSet).Select(m => m.TermObjId);

                    var termGroups = TermGroupDao.LoadTermGroup(false);
                    termGroups = termGroups.Where(g => !mappedGroups.Contains(g.UniqueId)).ToList();
                    var securityTermGroups = termGroups.ConvertAll(t => Convert2SecurityTermInfo(t));
                    rootNode.SubPerIndex = dbRootNode.SubPerIndex;
                    rootNode.SubPerSize = pageSize;
                    rootNode.SubTermCount = securityTermGroups.Count;

                    if ((Math.Ceiling(Convert.ToDouble(securityTermGroups.Count / dbRootNode.SubPerSize)) - Math.Ceiling(Convert.ToDouble(dbRootNode.SubTermCount / dbRootNode.SubPerSize))) > 1E-06)
                    {
                        rootNode.SubPerIndex = 0; //TermGroup被删除，导致DB中保存的PageIndex不准确，重置为第一页
                    }

                    foreach (var sGroup in securityTermGroups)
                    {
                        var termSets = TermSetDao.GetRMTermSetsByGroupUniqueId(sGroup.UniqueId);
                        termSets = termSets.Where(g => !mappedSets.Contains(g.UniqueId)).ToList();
                        var dbTermGroupNode = dbRootNode.SubTerms.Where(o => o.UniqueId == sGroup.UniqueId).FirstOrDefault();
                        if (dbTermGroupNode != null)
                        {
                            //JSON缓存中数据的IsChecked已经不够准确，需要再去Mapping表中获取
                            //var termGroupIsChecked = dbTermGroupNode.IsChecked;
                            var termGroupIsChecked = currentGroupMapped.Any(t => t.TermObjId == dbTermGroupNode.UniqueId);
                            sGroup.IsLoaded = true;
                            sGroup.IsExpand = !termGroupIsChecked && dbTermGroupNode.IsExpand; //TermGroup选中，默认不展开TermSet
                            sGroup.IsChecked = termGroupIsChecked;
                            if (termSets != null && termSets.Count > 0)
                            {
                                var securityTermSets = termSets.ConvertAll(t => Convert2SecurityTermInfo(t));
                                sGroup.SubPerIndex = dbTermGroupNode.SubPerIndex;
                                if ((Math.Ceiling(Convert.ToDouble(securityTermSets.Count / pageSize)) - Math.Ceiling(Convert.ToDouble(dbTermGroupNode.SubTermCount / pageSize))) > 1E-06)
                                {
                                    sGroup.SubPerIndex = 0; //TermGroup被删除，导致DB中保存的PageIndex不准确，重置为第一页
                                }
                                sGroup.SubPerSize = pageSize;

                                foreach (var sTermSet in securityTermSets)
                                {
                                    if (dbTermGroupNode.SubTerms != null)
                                    {
                                        var dbTermSetNode = dbTermGroupNode.SubTerms.Where(o => o.UniqueId == sTermSet.UniqueId).FirstOrDefault();
                                        if (dbTermSetNode != null)
                                        {
                                            //JSON缓存中数据的IsChecked已经不够准确，需要再去Mapping表中获取
                                            //sTermSet.IsChecked = termGroupIsChecked || dbTermSetNode.IsChecked;
                                            sTermSet.IsChecked = termGroupIsChecked || currentGroupMapped.Any(t => t.TermObjId == dbTermSetNode.UniqueId);
                                            sTermSet.IsExpand = dbTermSetNode.IsExpand;
                                        }
                                        else 
                                        {
                                            sTermSet.IsChecked = termGroupIsChecked;
                                        }
                                    }
                                    else
                                    {
                                        sTermSet.IsChecked = termGroupIsChecked;
                                    }
                                }
                                sGroup.SubTerms = securityTermSets;
                                sGroup.SubTermCount = securityTermSets.Count;
                            }
                        }
                        else
                        {
                            //TreeNode信息不存在这个TermGroup
                            if (termSets != null && termSets.Count > 0)
                            {
                                var securityTermSets = termSets.ConvertAll(t => Convert2SecurityTermInfo(t));
                                sGroup.SubPerIndex = 0;
                                sGroup.SubPerSize = pageSize;
                                sGroup.IsLoaded = true;
                                sGroup.SubTerms = securityTermSets;
                                sGroup.SubTermCount = securityTermSets.Count;
                            }
                        }
                    }
                    rootNode.SubTerms = securityTermGroups;
                }
            }
            catch (Exception ex)
            {

                logger.Warn($"An error while BuildSecurityTermTree, message:{ex}");
            }
            
            return rootNode;
        }

        public SecurityRuleInfo BuildSecurityRuleTree(SecurityRuleInfo dbRootNode, int groupId)
        {
            var rootNode = SecurityGroupManagementService.GetSecurityRuleRootNode();
            try
            {
                if (dbRootNode.IsChecked)
                {
                    //select all termgroups
                    rootNode.IsChecked = true;
                }
                else
                {
                    var mapped = RMSecurityGroupDao.GetMappedRuleByOtherGroups(groupId);
                    var isExistMappedAll = mapped.Any(m => m.Level == SecurityRuleLevel.All);//TODO Cyrus
                    var mappedRuleContainers = mapped.Where(m => m.Level == SecurityRuleLevel.RuleContainer).Select(m => m.RuleObjId);
                    var mappedRules = mapped.Where(m => m.Level == SecurityRuleLevel.Rule).Select(m => m.RuleObjId);

                    int pageSize = 10;
                    var allContainers = RMRuleDao.GetAllRuleContainers();
                    allContainers = allContainers.Where(c => !mappedRuleContainers.Contains(c.ContainerId)).ToList();
                    var securityRuleContainers = allContainers.ConvertAll(t => Convert2SecurityRuleInfo(t));
                    rootNode.SubPerIndex = dbRootNode.SubPerIndex;
                    rootNode.SubPerSize = pageSize;
                    rootNode.SubItemCount = securityRuleContainers.Count;

                    if (Math.Abs(Math.Ceiling(Convert.ToDouble(securityRuleContainers.Count / dbRootNode.SubPerSize)) - Math.Ceiling(Convert.ToDouble(dbRootNode.SubItemCount / dbRootNode.SubPerSize))) > Difference)
                    {
                        rootNode.SubPerIndex = 0; //RuleContainer被删除，导致DB中保存的PageIndex不准确，重置为第一页
                    }

                    foreach (var dbRuleContainer in securityRuleContainers)
                    {
                        var dbRules = RMRuleDao.GetAvailableRules(new List<Guid> { dbRuleContainer.UniqueId });
                        dbRules = dbRules.Where(g => !mappedRules.Contains(g.RuleId)).ToList();
                        var dbRuleContainerNode = dbRootNode.SubItems.Where(o => o.UniqueId == dbRuleContainer.UniqueId).FirstOrDefault();
                        if (dbRuleContainerNode != null)
                        {
                            var ruleContainerIsChecked = dbRuleContainerNode.IsChecked;
                            dbRuleContainer.IsLoaded = true;
                            dbRuleContainer.IsExpand = !ruleContainerIsChecked && dbRuleContainerNode.IsExpand; //TermGroup选中，默认不展开TermSet
                            dbRuleContainer.IsChecked = ruleContainerIsChecked;
                            if (dbRules != null && dbRules.Count > 0)
                            {
                                var rules = dbRules.ConvertAll(t => Convert2SecurityRuleInfo(t, dbRuleContainer.UniqueId));
                                dbRuleContainer.SubPerIndex = dbRuleContainerNode.SubPerIndex;
                                if (Math.Abs(Math.Ceiling(Convert.ToDouble(rules.Count / pageSize)) - Math.Ceiling(Convert.ToDouble(dbRuleContainerNode.SubItemCount / pageSize))) > 0)
                                {
                                    dbRuleContainer.SubPerIndex = 0; //TermGroup被删除，导致DB中保存的PageIndex不准确，重置为第一页
                                }
                                dbRuleContainer.SubPerSize = pageSize;

                                foreach (var sTermSet in rules)
                                {
                                    if (dbRuleContainerNode.SubItems != null)
                                    {
                                        var dbTermSetNode = dbRuleContainerNode.SubItems.Where(o => o.UniqueId == sTermSet.UniqueId).FirstOrDefault();
                                        if (dbTermSetNode != null)
                                        {
                                            sTermSet.IsChecked = ruleContainerIsChecked || dbTermSetNode.IsChecked;
                                            sTermSet.IsExpand = dbTermSetNode.IsExpand;
                                        }
                                        else
                                        {
                                            sTermSet.IsChecked = ruleContainerIsChecked;
                                        }
                                    }
                                    else
                                    {
                                        sTermSet.IsChecked = ruleContainerIsChecked;
                                    }
                                }
                                dbRuleContainer.SubItems = rules;
                                dbRuleContainer.SubItemCount = rules.Count;
                            }
                        }
                        else
                        {
                            //TreeNode信息不存在这个RuleContainer
                            if (dbRules != null && dbRules.Count > 0)
                            {
                                var securityTermSets = dbRules.ConvertAll(t => Convert2SecurityRuleInfo(t, dbRuleContainer.UniqueId));
                                dbRuleContainer.SubPerIndex = 0;
                                dbRuleContainer.SubPerSize = pageSize;
                                dbRuleContainer.IsLoaded = true;
                                dbRuleContainer.SubItems = securityTermSets;
                                dbRuleContainer.SubItemCount = securityTermSets.Count;
                            }
                        }
                    }
                    rootNode.SubItems = securityRuleContainers;
                }
            }
            catch (Exception ex)
            {

                logger.Warn($"An error while BuildSecurityTermTree, message:{ex}");
            }

            return rootNode;
        }

        private void ValideNameLen(string name, int len = 255)
        {
            WebUtil.CheckStringLen(name, len);
        }

        #region fs
        public string GetAllSubLocationTerm(int termId)
        {
            return JsonConvert.SerializeObject(TermDao.GetAllSubLocationTerm(termId));
        }

        public List<FSTermDto> GetAllTermsForce()
        {
            List<FSTermDto> terms = new List<FSTermDto>();
            TermDao.GetAllTermsForce().ForEach(t =>
            {
                terms.Add(ConvertRMTerm2FSTermDto(t));
            });
            return terms;
        }
        public OlderThanTimeDto GetTheRetentionUnitByClassCode(ApplyClassCodeSettingDto dto)
        {
            try
            {
                var termRuleMapping = TermRuleAssociationDao.GetTermRuleInfoByTermUniqueId(new Guid(dto.TermId));
                var ruleIds = termRuleMapping.Select(trm => trm.RuleId).Distinct().ToList();
                logger.Info($"CaculateEndTimeByClassCode,the termId is :{dto.TermId}，country code is:{dto.CountryCode}，termRuleMapping count is:{termRuleMapping?.Count},ruleIds count:{ruleIds?.Count}");
                var allRules = RuleManagerService.GetRulesByIds(ruleIds).ToDictionary(d => d.Id);
                List<string> fitRuleIds = new List<string>();
                foreach (var termRule in termRuleMapping)
                {
                    var currentRuleId = termRule.RuleId.ToString();
                    if (allRules.TryGetValue(currentRuleId, out Rule rule))
                    {
                        logger.Info($"CaculateEndTimeByClassCode,will check the class code ,this rule id :{currentRuleId}");
                        var fsRuleCountryCodeColumn = rule.FSRule.Filters.FirstOrDefault(f => (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals)
                            && f.Rule is ColumnTextRule);


                        var fsRuleRetentionType = rule.FSRule.Filters.FirstOrDefault(f => (f.Condition == PolicyCondition.Equals)
                            && f.Rule is ColumnTextRule && string.Equals(f?.Rule?.Value1, $"[RetentionType]", StringComparison.OrdinalIgnoreCase));
                        var fsRuleCountryCodeCriteriaValue = fsRuleCountryCodeColumn?.Value?.Value1;
                        var fsRuleRetentionTypeCriteriaValue = fsRuleRetentionType?.Value?.Value1;

                        List<string> retentionTypes = [RMConstants.RETENTIONTYPE_EVENT, RMConstants.RETENTIONTYPE_FLAT];
                        var tempRetentionType = retentionTypes.FirstOrDefault(type => type.Equals(fsRuleRetentionTypeCriteriaValue, StringComparison.OrdinalIgnoreCase));
                        if (string.IsNullOrEmpty(tempRetentionType))
                        {
                            logger.Warn($"CaculateEndTimeByClassCode,the tempRetentionType is null,will process next rule,current rule:{currentRuleId}");
                            continue;
                        }
                        logger.Info($"CaculateEndTimeByClassCode,the current rule id:{currentRuleId},the fsRule.Value.Value1 is :{fsRuleCountryCodeCriteriaValue}");
                        var countryCodes = fsRuleCountryCodeCriteriaValue.Split(";", StringSplitOptions.RemoveEmptyEntries)?.ToList();

                        if (countryCodes != null && countryCodes.Count > 0 && !string.IsNullOrEmpty(fsRuleRetentionTypeCriteriaValue) && ((RetentionScheduleType)dto.RetentionType).ToString() == fsRuleRetentionTypeCriteriaValue)
                        {
                            logger.Info($"CaculateEndTimeByClassCode,the current rule id:{currentRuleId},countryCodes has value");
                            if (countryCodes.Contains(dto.CountryCode)) 
                            {
                                fitRuleIds.Add(currentRuleId);
                            }
                        }
                        else
                        {
                            logger.Info($"CaculateEndTimeByClassCode,the current rule id:{currentRuleId},countryCodes not has value,fsRuleRetentionTypeCriteriaValue:{fsRuleRetentionTypeCriteriaValue}");
                        }
                    }
                    else
                    {
                        logger.Warn($"CaculateEndTimeByClassCode,this rule id not included in all term rules,ruleId :{termRule.RuleId.ToString()}");
                    }
                }

                if (fitRuleIds?.Count == 1)
                {
                    if (allRules.TryGetValue(fitRuleIds.FirstOrDefault(), out Rule rule))
                    {
                        logger.Info($"CaculateEndTimeByClassCode,the fitRuleIds count is 1,will check the retentiuon unit time,rule id :{fitRuleIds.FirstOrDefault()}");
                        var fsRuleTimeTypeColumn = rule.FSRule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan
                            && (f.Rule is ColumnDateTimeRule || f.Rule is ModifiedRule))?.ToList();
                        if (fsRuleTimeTypeColumn != null && fsRuleTimeTypeColumn.Count == 1)
                        {
                            var tempColumn = fsRuleTimeTypeColumn.FirstOrDefault();
                            var tempNumber = Convert.ToInt32(tempColumn.Value.Value1);
                            var tempPolicyValueUnit = tempColumn.Value.Value1Unit;
                            logger.Info($"CaculateEndTimeByClassCode,the fitRuleIds count is 1,and can return the OlderThanTimeDto,number:{tempNumber},unit:{tempPolicyValueUnit.ToString()}");
                            return new OlderThanTimeDto() {
                                Number = tempNumber,
                                PolicyValueUnit = tempPolicyValueUnit
                            };
                        }
                        else
                        {
                            logger.Warn($"CaculateEndTimeByClassCode,fsRuleTimeTypeColumn is null or count more than one ,return null");
                            return null;
                        }
                    }
                    logger.Info($"CaculateEndTimeByClassCode,the fitRuleIds count is 1,but can not get the rule anymore,return null");
                    return null;
                }
                else
                {
                    logger.Warn($"CaculateEndTimeByClassCode,the fitRuleIds count not 1,the count is :{fitRuleIds?.Count}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to CaculateEndTimeByClassCode. error:{ex}");
                return null;
            }
        }
        public async Task<List<ClassCodeCascadeDataDto>> GetClassCodeCascadeDataAsync(CurrentSettingsInfo settingsInfo)
        {
            try
            {
                List<ClassCodeCascadeDataDto> result = new List<ClassCodeCascadeDataDto>();
                var terms = await TermDao.GetTermFromTermSetUniqueId(new Guid(settingsInfo.TermSetId));
                var termIds = terms.Select(term => term.Id).ToList();
                var termRuleMapping = TermRuleAssociationDao.GetTermRuleInfoByTermIds(termIds);
                var ruleIds = termRuleMapping.Select(trm => trm.RuleId).Distinct().ToList();
                var allRules = RuleManagerService.GetRulesByIds(ruleIds).ToDictionary(d => d.Id);
                foreach (var term in terms)
                {
                    logger.Info($"[Customization4JPMC]Assembly term: {term.Name}");
                    //string canNotConvertRule = null;
                    var cascadeData = new ClassCodeCascadeDataDto
                    {
                        ClassCode = term.Name,
                        TermUniqueId = term.UniqueId.ToString(), // term unique id
                    };
                    var rules4CurrentTerm = termRuleMapping.Where(mp => mp.TermId == term.Id).ToList();
                    var countryCodes = new List<string>();
                    foreach (var termRule in rules4CurrentTerm)
                    {
                        if (allRules.TryGetValue(termRule.RuleId.ToString(), out Rule rule) && rule.FSRule != null)
                        {
                            var fsRule = rule.FSRule.Filters.FirstOrDefault(f => (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals)
                                && CountryCodeFields.Contains(f.Rule?.Value1)
                                && f.Rule is ColumnTextRule);
                            if(fsRule != null && !string.IsNullOrEmpty(fsRule.Value.Value1))
                            {
                                countryCodes.AddRange(fsRule.Value.Value1.Split(";", StringSplitOptions.RemoveEmptyEntries));
                            }
                        }
                    }
                    cascadeData.CountryCode = countryCodes.Distinct().OrderBy(code => code == "US" ? 0 : 1).ThenBy(code => code).ToList();
                    result.Add(cascadeData);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get class code cascade data from SP. Message:{ex}");
                return new List<ClassCodeCascadeDataDto>();
            }
        }
        public async Task<List<RMMyhubClassCodeCascadeDataDto>> RMMyhubGetClassCodeCascadeDataAsync(string termSetId)
        {
            try
            {
                List<RMMyhubClassCodeCascadeDataDto> result = new List<RMMyhubClassCodeCascadeDataDto>();
                var terms = await TermDao.GetTermFromTermSetUniqueId(new Guid(termSetId));
                var termIds = terms.Select(term => term.Id).ToList();
                var termRuleMapping = TermRuleAssociationDao.GetTermRuleInfoByTermIds(termIds);
                var ruleIds = termRuleMapping.Select(trm => trm.RuleId).Distinct().ToList();
                var allRules = RuleManagerService.GetRulesByIds(ruleIds).ToDictionary(d => d.Id);
                foreach (var term in terms)
                {
                    logger.Info($"[Myhub Customization4JPMC]Assembly term: {term.Name}");
                    //string canNotConvertRule = null;
                    var cascadeData = new RMMyhubClassCodeCascadeDataDto
                    {
                        ClassCode = term.Name,
                        TermUniqueId = term.UniqueId.ToString(), // term unique id
                        TermFullPath= TermDao.GetTermNamesPathByTermId(term.UniqueId)
                    };
                    var rules4CurrentTerm = termRuleMapping.Where(mp => mp.TermId == term.Id).ToList();
                    var countryCodes = new List<string>();
                    foreach (var termRule in rules4CurrentTerm)
                    {
                        if (allRules.TryGetValue(termRule.RuleId.ToString(), out Rule rule) && rule.FSRule != null)
                        {
                            var fsRule = rule.FSRule.Filters.FirstOrDefault(f => (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals)
                                && CountryCodeFields.Contains(f.Rule?.Value1)
                                && f.Rule is ColumnTextRule);
                            if (fsRule != null && !string.IsNullOrEmpty(fsRule.Value.Value1))
                            {
                                countryCodes.AddRange(fsRule.Value.Value1.Split(";", StringSplitOptions.RemoveEmptyEntries));
                            }
                        }
                    }
                    cascadeData.CountryCode = countryCodes.Distinct().OrderBy(code => code == "US" ? 0 : 1).ThenBy(code => code).ToList();
                    result.Add(cascadeData);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get class code cascade data from Myhub. Message:{ex}");
                return new List<RMMyhubClassCodeCascadeDataDto>();
            }
        }
        public List<AgentTermSetDto> GetAllTermSetsForce()
        {
            List<AgentTermSetDto> terms = new List<AgentTermSetDto>();
            TermDao.GetAllTermSetsForce().ForEach(t =>
            {
                terms.Add(ConvertRMTermSet2AgentTermSetDto(t));
            });
            return terms;
        }

        public List<AgentTermSetMembershipDto> GetAllTermSetMemberShipsForce()
        {
            List<AgentTermSetMembershipDto> terms = new List<AgentTermSetMembershipDto>();
            TermDao.GetAllTermSetMemberShipsForce().ForEach(t =>
            {
                terms.Add(ConvertRMTermSetMembership2AgentAgentTermSetMembershipDto(t));
            });
            return terms;
        }

        public Task<Dictionary<string, string>> GetAllTermGroups()
        {
            try
            {
                return TermGroupDao.GetAllTermGroups();
            }
            catch (Exception e)
            {
                logger.Error("Save All Term Group Setting. Message:{0}.", e.ToString());
            }
            return Task.FromResult<Dictionary<string, string>>(null);
        }


        public async Task<List<RMSiteInfo>> GetGoogleTermGroupSettingAsync()
        {
            var googleTenantSetting = await TermGroupMembershipDao.GetGoogleTermGroupMemberships();
            var googleTenantSettingIds = googleTenantSetting.Select(g => g.SiteUrl).ToList();
            var aosGoogleTenants = await RMAosApiClient.GetGoogleTenants(TenantLocalValue.LogonGroupId);
            var newGoogleTenant = aosGoogleTenants.Where(aosGoogleTenant => !googleTenantSettingIds.Contains(aosGoogleTenant.Key));
            var newGoogleTenantList= newGoogleTenant.Select(tenant => new RMSiteInfo{ DisplayName = tenant.Value, SiteUrl = tenant.Key, SiteType = SiteType.Google}).ToList();
            List<RMSiteInfo> result = [];
            result.AddRange(googleTenantSetting.Select(info => new RMSiteInfo()
            {
                Id = info.Id,
                DisplayName = info.DisplayName,
                TermGroupId = info.TermGroupId,
                SiteUrl = info.SiteUrl,
                AgentGroupId = info.AgentGroupId,
                TermStoreId = info.TermStoreId,
                SiteType = info.SiteType
            }));
            return [..result,..newGoogleTenantList];
        }

        public Task<Dictionary<string, List<string>>> GetTermGroupNameAndGoogleTenantsAsync(Guid termGroupId)
        {
            return TermGroupDao.GetTermGroupNameAndGoogleTenant(termGroupId);
        }

        public string GetAllTermsUnderTermSet(int termSetId)
        {
            return JsonConvert.SerializeObject(TermDao.FSGetAllTermsUnderTermSet(termSetId));
        }

        public string GetRMTermByGuId(Guid termId)
        {
            return JsonConvert.SerializeObject(TermDao.GetRMTermByGuId(termId));
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.AIRecommendation, Action = AuditAction.AIRecommendation, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> AIRecomendationAsync(AIRecomentdation aIRecomentdation)
        {
            var msg = new RAReturnMessage();
            var prompt = string.Empty;
            var enableReference = KeyValueDao.GetValueByKey("AI_Recommendation_Reference_Enable");
            if (enableReference != null && bool.TryParse(enableReference.Value, out var isEnableReference) && isEnableReference)
            {
                prompt = PromptUtil.BuildClassificationPromptCommonWithReference(aIRecomentdation);
            }
            else
            {
                prompt = PromptUtil.BuildClassificationPromptCommon(aIRecomentdation);
            }

            var featureUsage = await FeatureUsageLimitDao.GetFeatureUsageLimit(FeatureType.AIRecommendation);
            if (featureUsage != null && featureUsage.LimitUsage * 2 <= featureUsage.Usaged)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_ML_Zero_CheckUsageLimit_Msg")
                };
            }
            try
            {
                IChatCompletionProvider provider = await ChatCompletionServices.CreateWithRAIProvider();
                ChatCompletionResponse response = await provider.GetChatCompletionResponseAsync(prompt);
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var content = response.Content.Trim();
                    if (content.StartsWith("```json"))
                    {
                        content = content.Substring(7);
                    }
                    else if (content.StartsWith("```"))
                    {
                        content = content.Substring(3);
                    }
                    if (content.EndsWith("```"))
                    {
                        content = content.Substring(0, content.Length - 3);
                    }
                    var categories = JsonConvert.DeserializeObject<List<RecordCategory>>(content);
                    if (categories != null)
                    {
                        categories = categories
                            .OrderBy(c => c?.Name, StringComparer.OrdinalIgnoreCase)
                            .ToList();
                    }
                    msg.MessageType = RAMessageType.Successful;
                    msg.Extsion1 = categories;
                    FeatureUsageLimitDao.AddOrUpdate(FeatureType.AIRecommendation);
                    return msg;
                }
            }
            catch (Exception ex)
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_TM_AI_Recommendations_AI_Response_Message");
                logger.Warn($"An error while GetChatCompletionResponseAsync, message:{ex}");
            }
            return msg;
        }

        public async Task<MemoryStream> GetStreamAIRecommendation(string industry, List<RecordCategory> records, bool isControlPlus = false)
        {
            string tempFilePath = null;
            var stream = new MemoryStream();
            try
            {
                ExportAddition exportAddition = null;
                if (!isControlPlus)
                {
                    if (KeyValueDao.GetValueByKey("JPMC_Customization") != null || TeamsPermissionHelper.HasUpgradeTeamsFeature())
                    {
                        exportAddition = new ExportAddition();
                        exportAddition.TermColumArray = GetAdditionTermColumnArray(exportAddition);
                        exportAddition.RuleColumArray = new string[] { "Notes" };
                        exportAddition.ConditionArray = new string[] { "ListIn" };
                    }
                }
               
                if (AccountUtility.IsSupportRecordLabel())
                {
                    if (exportAddition == null) exportAddition = new ExportAddition();
                    exportAddition.IsSupportRecordLabelFunction = true;
                }
                var tempFolderPath = Path.Combine(WebUtil.GetInstallPath(), "Temp", "Config");
                if (!Directory.Exists(tempFolderPath))
                {
                    logger.Info("Temp path not find Create Path");
                    Directory.CreateDirectory(tempFolderPath);
                }
                var fileName = $"Temp excel for AI Recommendation {Guid.NewGuid().ToString("N")}.xlsx";
                tempFilePath = Path.Combine(tempFolderPath, fileName);
                List<string[]> termsData = new List<string[]>();
                List<string[]> rulesData = new List<string[]>();
                List<string> distinctTerm = new List<string>();
                foreach (var record in records)
                {
                    if (record == null) continue;
                    termsData.AddRange(ConvertTermAIRecommendationRow(record, industry, exportAddition, distinctTerm, isControlPlus));
                    if (record.RetentionPolicy == null || record.RetentionPolicy.RetentionTime == null) continue;
                    rulesData.AddRange(isControlPlus ? await ConvertRuleAIRecommendationRowForGoogleOne(record, exportAddition) : await ConvertRuleAIRecommendationRow(record, exportAddition));
                }
                termsData = termsData.Distinct().ToList();
                ReportUtil.CreateTermsAndRulesSheets(tempFilePath,rulesData, termsData, exportAddition);
                await using var tempStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
                await tempStream.CopyToAsync(stream);
            }
            catch (Exception e)
            {
                logger.Error("Export AI Recommendation failed", e);
                throw;
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempFilePath))
                    {
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Dispose temp file path Failed");
                }
            }
            return stream;
        }

        private List<string[]> ConvertTermAIRecommendationRow(RecordCategory record, string industry, ExportAddition exportAddition, List<string> distinctTerm, bool isControlPlus = false)
        {
            List<string[]> terms = new List<string[]>();
            int termLength = exportAddition != null && exportAddition.TermColumArray != null ? ReportUtil.TermsSheetColumnCount + exportAddition.TermColumArray.Count() : ReportUtil.TermsSheetColumnCount;
            string[] term = new string[termLength];
            term[TermPropertyIndex.TermGroupName] = industry;
            term[TermPropertyIndex.TermSetName] = !isControlPlus ? "TermSet" : I18NEntity.GetString("RM_TM_DefaultGoogleTermSet");
            var splitTermLevelName = record.Name.Split('/');
            term[TermPropertyIndex.Inherit] = "TRUE";
            for (int i = 0; i < splitTermLevelName.Length; i++)
            {
                if (i == 5) break;
                term[TermPropertyIndex.Level1 + i] = splitTermLevelName[i];
                if(i == splitTermLevelName.Length - 1 || i == 4)
                {
                    var splitName = record.Name.Split("/");
                    term[TermPropertyIndex.Description] = record?.Description ?? string.Empty;
                    term[TermPropertyIndex.RuleName] = record.RetentionPolicy == null || record.RetentionPolicy.RetentionTime == null ? string.Empty :
                        string.Format(I18NEntity.GetString("RM_TM_Excel_TermRuleNameValue"), splitName[splitName.Length - 1], record.RetentionPolicy.RetentionTime.RetentionTimeNumber, record.RetentionPolicy.RetentionTime.Unit);
                    term[TermPropertyIndex.Inherit] = "FALSE";
                }
                var termPath = BuildTermPath(term);
                if(!distinctTerm.Contains(termPath))
                {
                    string[] tempTerm = term.ToArray();
                    terms.Add(tempTerm);
                    distinctTerm.Add(termPath);
                }
            }
            return terms;
        }

        private string BuildTermPath(string[] term)
        {
            List<string> termPath = new();
            for(int i = 0; i < 5; i++)
            {
                if (string.IsNullOrEmpty(term[TermPropertyIndex.Level1 + i]))
                    break;
                termPath.Add(term[TermPropertyIndex.Level1 + i]);
            }
            return string.Join("|", termPath);
        }

        private async Task<List<string[]>> ConvertRuleAIRecommendationRow(RecordCategory record, ExportAddition exportAddition)
        {
            int IndexChangeFromIncludeDeclaredRecord = 0, IndexChangeFromLabel = 0, increateRuleLength = 0;
            if(exportAddition?.IsSupportRecordLabelFunction ?? false)
            {
                IndexChangeFromIncludeDeclaredRecord = 1;
                IndexChangeFromLabel = 3;
                increateRuleLength = 3;
            }
            List<string[]> rules = new List<string[]>();
            int ruleLength = exportAddition != null && exportAddition.RuleColumArray != null ? ReportUtil.RulesSheetColumnCount + exportAddition.RuleColumArray.Count() : ReportUtil.RulesSheetColumnCount;
            string[] rule = new string[ruleLength + increateRuleLength];
            var splitName = record.Name.Split("/");
            rule[RulePropertyIndex.Name] = string.Format(I18NEntity.GetString("RM_TM_Excel_TermRuleNameValue"), splitName[splitName.Length - 1], record.RetentionPolicy.RetentionTime.RetentionTimeNumber, record.RetentionPolicy.RetentionTime.Unit);
            rule[RulePropertyIndex.Description] = record.RetentionPolicy.RetentionTime.PolicyDescription ?? string.Empty;
            rule[RulePropertyIndex.ContainerName] = "Default rule container";
            rule[RulePropertyIndex.RuleLevel] = "Document/Email";
            rule[RulePropertyIndex.SourceType] = "SharePoint Online";
            rule[RulePropertyIndex.CriteriaType] = ArchiverFilterRuleType.ModifiedTime.ToString();
            rule[RulePropertyIndex.CriteriaCondition] = ArchiverFilterCondition.OlderThan.ToString();
            rule[RulePropertyIndex.ConditionValue] = record.RetentionPolicy.RetentionTime.RetentionTimeNumber.ToString();
            rule[RulePropertyIndex.ConditionValueUnit] = (record.RetentionPolicy.RetentionTime.Unit.Equals("years") ? PolicyValueUnit.Years : PolicyValueUnit.Months).ToString();
            rule[RulePropertyIndex.RuleAction] = "Remove content";
            var isArchiver = string.IsNullOrEmpty(record.RetentionPolicy.Action) || record.RetentionPolicy.Action.Equals("archive");
            rule[RulePropertyIndex.ArchiverBeforeDestory + IndexChangeFromIncludeDeclaredRecord] = isArchiver ? "TRUE" : string.Empty;
            var isEnableManualApprove = !(string.IsNullOrEmpty(record.RetentionPolicy.ManualReview) || record.RetentionPolicy.ManualReview.Equals("no"));
            rule[RulePropertyIndex.EnableMannualApprove + IndexChangeFromLabel] = isEnableManualApprove ? "TRUE" : "FALSE";
            if (isEnableManualApprove)
            {
            var currentUser = await AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId);
                if (currentUser != null)
            {
                rule[RulePropertyIndex.ReviewType + IndexChangeFromLabel] = "Record owner";
                rule[RulePropertyIndex.RecordOwner + IndexChangeFromLabel] = currentUser.UserPrincipalName ?? string.Empty;
            }
            }
            if(isArchiver)
            {
                SettingProfileDto indexDto = new SettingProfileDto()
                {
                    Type = (int)SettingProfilesType.IndexDevice,
                    Name = "UsingIndexDevice"
                };
                var indexDDto = SettingProfileDao.Load(indexDto);
                string storageName = string.Empty;
                if (indexDDto != null)
                {
                    var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(indexDDto);
                if(tempDto != null && Guid.TryParse(tempDto.Settings, out var defaultStorageId))
                    {
                        var defaultStorage = StorageDeviceInfoDao.GetStorageDevicesById(defaultStorageId);
                        storageName = defaultStorage != null ? defaultStorage.Name : string.Empty;
                    }
                }
                rule[RulePropertyIndex.ArchiveDataStorage + IndexChangeFromLabel] = storageName;
            }
            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            if (isILLicense)
            {
                rules.Add(rule);
                string[] oneDriveRule = rule.ToArray();
                oneDriveRule[RulePropertyIndex.SourceType] = "OneDrive";
                rules.Add(oneDriveRule);
            }
            //Current the google does not support Archiver content rule
            if (isGoogleLicense)
            {
                string[] googleRule = rule.ToArray();
                googleRule[RulePropertyIndex.SourceType] = "Google Drive";
                googleRule[RulePropertyIndex.ArchiverBeforeDestory + IndexChangeFromIncludeDeclaredRecord] = string.Empty;
                googleRule[RulePropertyIndex.ArchiveDataStorage + IndexChangeFromLabel] = string.Empty;
                rules.Add(googleRule);
            }
            return rules;
        }

        private async Task<List<string[]>> ConvertRuleAIRecommendationRowForGoogleOne(RecordCategory record, ExportAddition exportAddition)
        {
            int IndexChangeFromLabel = 0, increateRuleLength = 0;
            if (exportAddition?.IsSupportRecordLabelFunction ?? false)
            {
                IndexChangeFromLabel = 3;
                increateRuleLength = 3;
            }
            List<string[]> rules = new List<string[]>();
            int ruleLength = exportAddition != null && exportAddition.RuleColumArray != null ? ReportUtil.RulesSheetColumnCount + exportAddition.RuleColumArray.Count() : ReportUtil.RulesSheetColumnCount;
            string[] rule = new string[ruleLength + increateRuleLength];
            var splitName = record.Name.Split("/");
            rule[RulePropertyIndex.Name] = string.Format(I18NEntity.GetString("RM_TM_Excel_TermRuleNameValue"), splitName[splitName.Length - 1], record.RetentionPolicy.RetentionTime.RetentionTimeNumber, record.RetentionPolicy.RetentionTime.Unit);
            rule[RulePropertyIndex.Description] = record.RetentionPolicy.RetentionTime.PolicyDescription ?? string.Empty;
            rule[RulePropertyIndex.ContainerName] = I18NEntity.GetString("RM_RDM_DefaultRuleContainer");
            rule[RulePropertyIndex.RuleLevel] = "Document/Email";
            rule[RulePropertyIndex.SourceType] = "Google Drive";
            rule[RulePropertyIndex.CriteriaType] = ArchiverFilterRuleType.ModifiedTime.ToString();
            rule[RulePropertyIndex.CriteriaCondition] = ArchiverFilterCondition.OlderThan.ToString();
            rule[RulePropertyIndex.ConditionValue] = record.RetentionPolicy.RetentionTime.RetentionTimeNumber.ToString();
            rule[RulePropertyIndex.ConditionValueUnit] = (record.RetentionPolicy.RetentionTime.Unit.Equals("years") ? PolicyValueUnit.Years : PolicyValueUnit.Months).ToString();
            rule[RulePropertyIndex.RuleAction] = "Remove content";
            rule[RulePropertyIndex.ArchiverBeforeDestory] = string.Empty;
            var isEnableManualApprove = !(string.IsNullOrEmpty(record.RetentionPolicy.ManualReview) || record.RetentionPolicy.ManualReview.Equals("no"));
            rule[RulePropertyIndex.EnableMannualApprove] = isEnableManualApprove ? "TRUE" : "FALSE";
            if (isEnableManualApprove)
            {
                var currentUser = await AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId);
                if (currentUser != null)
                {
                    rule[RulePropertyIndex.ReviewType + IndexChangeFromLabel] = "Record owner";
                    rule[RulePropertyIndex.RecordOwner + IndexChangeFromLabel] = currentUser.UserPrincipalName ?? string.Empty;
                }
            }
            rules.Add(rule);


            return rules;
        }

        private string[] GetAdditionTermColumnArray(ExportAddition exportAddition)
        {
            List<string> result = new();
            if (KeyValueDao.GetValueByKey("JPMC_Customization") != null) result.Add("RM_TM_AdvanceSetting");
            if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
            {
                exportAddition.HasUpgradeTeams = true;
                result.Add("RM_TM_Retention_Teams_Label");
            }
            result.Add("Notes");
            return result.ToArray();
        }

        private FSTermDto ConvertRMTerm2FSTermDto(RMTerm term)
        {
            FSTermDto dto = new FSTermDto()
            {
                //AvailableSpace = term.AvailableSpace,
                //BreakInheritFromParent = term.BreakInheritFromParent,
                Id = term.Id,
                //IsDayLight = term.IsDayLight,
                //IsDefaultTerm = term.IsDefaultTerm,
                //IsPermanent = term.IsPermanent,
                //IsRootTerm = term.IsRootTerm,
                Name = term.Name,
                //RuleInfo = term.RuleInfo,
                TermExpirationFrom = term.TermExpirationFrom,
                TermExpirationTo = term.TermExpirationTo,
                TermSetId = term.TermSetId,
                //TimeZoneId = term.TimeZoneId,
                UniqueId = term.UniqueId,
                IsDeprecated = term.IsDeprecated,
                IsRemoved = term.IsRemoved,
            };
            return dto;
        }

        private AgentTermSetDto ConvertRMTermSet2AgentTermSetDto(RMTermSet termSet)
        {
            AgentTermSetDto agentTermSetDto = new AgentTermSetDto()
            {
                Id = termSet.Id,
                UniqueId = termSet.UniqueId,
                Name = termSet.Name,
                Description = termSet.Description,
                TermGroupId = termSet.TermGroupId,
                IsRemoved = termSet.IsRemoved,
                TermSetType = (AvePoint.RA.Contract.OnPremiseSharePoint.TermSetType)termSet.TermSetType,
            };
            return agentTermSetDto;
        }

        private AgentTermSetMembershipDto ConvertRMTermSetMembership2AgentAgentTermSetMembershipDto(RMTermSetMembership termSetMembership)
        {
            AgentTermSetMembershipDto agentTermSetMembershipDto = new AgentTermSetMembershipDto()
            {
                TermId = termSetMembership.TermId,
                TermSetId = termSetMembership.TermSetId,
                ParentTermId = termSetMembership.ParentTermId,
                TermName = termSetMembership.TermName,
                Path = termSetMembership.Path,
                IsSource = termSetMembership.IsSource,
                IsRemoved = termSetMembership.IsRemoved,
            };
            return agentTermSetMembershipDto;
        }

        public Dictionary<int, List<Guid>> GetTermRuleMapping()
        {
            return TermRuleAssociationDao.GetTermWithRule().GroupBy(t => t.TermId).ToDictionary(t => t.Key, v => v.OrderBy(r => r.RuleOrder).Select(w => w.RuleId).ToList());
        }

        #endregion

        public Task<string> GetEXOSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetEXOTermSetId, SourceFlag.Exchange, needCheckPermission);
        }

        public Task<string> GetPRSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetPhyTermSetId, SourceFlag.Physical, needCheckPermission);
        }

        public Task<string> GetFSSavedTermAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetFSTermSetId, SourceFlag.FileSystem, needCheckPermission);
        }

        public Task<string> GetSPOnPremSettingSavedTreeAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetSPOnPremTermSetId, SourceFlag.SharePointOnPrem, needCheckPermission);
        }
        public Guid GetSPTermSetId(CurrentSettingsInfo settingInfo)
        {
            string agentGroupId = settingInfo.spTreeNodes[0].SPObjectId;
            RMSharePointSetting setting = SharePointSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
            return setting.TermSetId;
        }

        public Guid GetEXOTermSetId(CurrentSettingsInfo settingInfo)
        {
            //string agentGroupId = settingInfo.spTreeNodes[0].Id;
            //RMExchangeOnlineSetting setting = EXOSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
            RMExchangeOnlineSetting setting = EXOSettingDao.GetSettingInfoByAgentGroupId(settingInfo.GroupId);
            return setting.TermSetId;
        }

        public Guid GetOneDriveTermSetId(CurrentSettingsInfo settingInfo)
        {
            string agentGroupId = settingInfo.spTreeNodes[0].SPObjectId;
            RMOneDriveSetting setting = OneDriveSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
            return setting.TermSetId;
        }

        public Guid GetTeamsTermSetId(CurrentSettingsInfo settingInfo)
        {
            string agentGroupId = settingInfo.spTreeNodes[0].SPObjectId;
            RMTeamsSetting setting = TeamsSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
            return setting.TermSetId;
        }

        public Guid GetPhyTermSetId(CurrentSettingsInfo settingInfo)
        {
            var setting = PhysicalRecordSettingDao.GetPhysicalRecordSetting(new Guid(settingInfo.GroupId));
            return setting.TermSetId;
        }

        public Guid GetFSTermSetId(CurrentSettingsInfo settingInfo)
        {
            RMFileSystemSetting setting = FileSystemSettingDao.GetSettingByConnGroupId(new Guid(settingInfo.ConnGroupId));
            return setting.TermSetId;
        }

        public Guid GetSPOnPremTermSetId(CurrentSettingsInfo settingInfo)
        {
            string agentGroupId = settingInfo.spTreeNodes[0].SPObjectId;
            var setting = SharePointOnPremiseSettingDao.GetSettingInfoByAgentGroupId(agentGroupId);
            return setting.TermSetId;
        }

        public Guid GetAzureFileTermSetId(CurrentSettingsInfo settingInfo)
        {
            RMAzureFileShareSetting setting = AzureFileShareSettingDao.GetSetting(new Guid(settingInfo.ConnGroupId));
            return setting.TermSetId;
        }

        public Task<string> GetAzureFileSavedTermAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetAzureFileTermSetId, SourceFlag.AzureFileShare, needCheckPermission);
        }

        private Guid GetBoxTermSetId(CurrentSettingsInfo settingInfo)
        {
            RMBoxSetting setting = BoxSettingDao.GetSettingByConnGroupId(new Guid(settingInfo.ConnGroupId));
            return setting.TermSetId;
        }

        public Task<string> GetBoxSavedTermAsync(CurrentSettingsInfo settingInfo, bool needCheckPermission = false)
        {
            return this.CommonGetSettingSavedTreeAsync(settingInfo, this.GetBoxTermSetId, SourceFlag.Box, needCheckPermission);
        }

        public async Task<string> GetTaxonomyGoogleTermTreeApplySettingDataAsync(string nodeId, int pageIndex, int pageCount, string searchKey)
        {
            var result = string.Empty;
            try
            {
                List<Guid> groupUniqueIds = new List<Guid>();
                SecurityTermPermissionDto termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new QuerySecurityTermObjDto
                {
                    UserId = TenantLocalValue.LogonUserId,
                    Level = SecurityTermLevel.TermGroup,
                    FilterByContentSource = true,
                    ExcludeBuiltIn = false,
                    SourceFlag = SourceFlag.Google
                });
                if (termPermissionInfo.TermPermissionType == TermPermissionMethod.All)
                {
                    return JsonConvert.SerializeObject(await TermDao.GetPaginatedTermsStructureAsync(nodeId, pageIndex, pageCount, [], [], searchKey));
                }
                groupUniqueIds = termPermissionInfo.TermObjIds;
                List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                return JsonConvert.SerializeObject(await TermDao.GetPaginatedTermsStructureAsync(nodeId, pageIndex, pageCount, groupUniqueIds, userAndGroupUserIds, searchKey));
            }
            catch (Exception ex)
            {
                logger.Error($"error occur while getting paginated labels, {ex}");
            }
            return result;
        }
        public async Task<string> AddFirstTermSetAsync(Guid termGroupId)
        {
            var result = await TermSetDao.CreateGoogleTermSet(I18NEntity.GetString(I18NResource.DefaultGoogleTermSet), termGroupId);
            return GetJsonStrByObj(result);
        }

        public async Task<int> FindOrAddFirstTermSetAsync(Guid termGroupId)
        {
            try
            {
                var firstTermSet = TermSetDao.GetFirstTermSetByTermGroupId(termGroupId);
                if (firstTermSet == null)
                {
                    var result = await TermSetDao.CreateGoogleTermSet(I18NEntity.GetString(I18NResource.DefaultGoogleTermSet), termGroupId);
                    return result.Id;
                }
                return firstTermSet.Id;
            }
            catch (Exception ex)
            {
                logger.Error($"Error in FindOrAddFirstTermSetAsync: {ex.Message}");
            }
            return 0;
        }

        public async Task<string> GetTermsByGroupId(Guid termGroupId)
        {
            var result = await TermGroupDao.LoadGroupsData(termGroupId);
            return GetJsonStrByObj(result);
        }

        public Task<Dictionary<string, string>> GetAllTermGroupsByMultipleNodes(RMClassificationGroupMultipleNodes nodes)
        {
            try
            {
                return TermGroupDao.GetAllTermGroupsByMultipleNodes(nodes);
            }
            catch (Exception e)
            {
                logger.Error("Save All Term Group Setting. Message:{0}.", e.ToString());
            }
            return Task.FromResult<Dictionary<string, string>>(null);
        }

        public string GetTermRuleInfoByTermIdAndSourceFlagForGoogleOne(int termId, SourceFlag sourceFlag = SourceFlag.All)
        {
            List<RMTermRuleAssociation> listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId, sourceFlag);
            return GetJsonStrByObj(listRule);
        }
        public async Task<string> GetTermSetsAsync(List<Guid> termGroupIds)
        {
            return GetJsonStrByObj(await TermGroupDao.LoadGoogleGroupsData(termGroupIds));
        }

        public async Task<string> SearchAsync(int termSetId, string termLabel, List<Guid> termGroupIds, bool withRuleName = false)
        {
            var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            FilterTermObjOption filterTermObjOption = new FilterTermObjOption();
            filterTermObjOption.NeedCheckPermission = true;
            filterTermObjOption.userAndGroupUserIds = userAndGroupIds;
            return GetJsonStrByObj(TermDao.GetRMTermsBySearch(termLabel, termGroupIds, withRuleName, filterTermObjOption));
        }

        public List<RMTermInfo> SearchTermWithLimit(string searchValue, int limit)
        {
            List<RMTerm> terms = TermDao.SearchTermWithLimit(searchValue, limit);
            return terms.ConvertAll(Convert2RMTermDto);
        }

        public async Task<List<RMTermInfo>> SearchLabelWithLimit(string searchValue, int limit)
        {
            List<RMTerm> terms = await TermDao.SearchLabelWithLimit(searchValue, limit);
            return terms.ConvertAll(Convert2RMTermDto);
        }

        public bool HasTermGroupName(string termGroupName)
        {
            return TermGroupDao.HasSameNameTermGroup(termGroupName);    
        }

        public bool HasTermSetName(string termSetName, Guid termGroupId)
        {
            return TermSetDao.HasSameNameTermSet(termSetName, termGroupId);
        }
    }

    public enum ExcelColumnType
    {
        TermName, TermDescription, TermStatus, EnforceRetention
       , RuleName, RuleDescription, RuleLevel, DisposalClass
       , SourceType, Criteria, Action, DeleteRecords, EnableManualApproval, SendEmail, RecordOwner, ExportSharePointContent, ExportFormat, IncludeRelatedRecord
    }
    public enum MergeCellConditionType
    {
        TermName,
        RuleName
    }
}

