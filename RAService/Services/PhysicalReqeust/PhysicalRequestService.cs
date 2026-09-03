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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Extension;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.Service.LocationManagement;
using AvePoint.RA.Service.Services.PhysicalReqeust.AuditHandler;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Util.Security;

namespace AvePoint.RA.Service.Services.PhysicalReqeust
{
    [Audit]
    public class PhysicalRequestService : RMServiceBase, IPhysicalReqeustService
    {
        #region IOC Properties
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(PhysicalRequestService));
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IPhysicalRequestDao PhysicalRequestDao => PlatformWindsorManager.GetService<IPhysicalRequestDao>();
        private IRecordLoanAllianceDao RecordLoanAllianceDao => PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        protected IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();

        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        private IRMScopeRoleAssignmentDao _rMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();

        #endregion

        #region GUI Page Query
        public async Task<PhysicalRequestDto> GetRequestAsync(int id)
        {
            var domain = PhysicalRequestDao.GetRequest(id);
            var phyDto = await ConvertDomain2DtoAsync(domain);
            if (!(phyDto.Status != PhysicalRequestStatus.Approved && phyDto.Type == PhysicalRequestType.Creation))
            {
                var physicalFileInfos = new List<PhysicalObjectDto>();
                if (phyDto.PhysicalFileInfos != null)
                {
                    foreach (var physicalFileInfo in phyDto.PhysicalFileInfos)
                    {
                        if (physicalFileInfo != null)
                        {
                            var fileInfo = await GetPhysicalObjectById(physicalFileInfo.Id, physicalFileInfo, "", (int)physicalFileInfo.NodeType);
                            if (fileInfo == null) continue;
                            var loanAllian = RecordLoanAllianceDao.GetPhyRecordAllianceById(physicalFileInfo.Id).FirstOrDefault();
                            if (loanAllian != null)
                            {
                                fileInfo.HoldBy = loanAllian.HoldBy;
                            }
                            physicalFileInfos.Add(fileInfo);
                        }
                    }
                    phyDto.PhysicalFileInfos = physicalFileInfos;
                }
                else if (phyDto.PhysicalFileInfo != null)
                {
                    var fileInfo = await GetPhysicalObjectById(phyDto.PhysicalFileInfo.Id, phyDto.PhysicalFileInfo, "", (int)phyDto.PhysicalFileInfo.NodeType);
                    var loanAllian = RecordLoanAllianceDao.GetPhyRecordAllianceById(phyDto.PhysicalFileInfo.Id).FirstOrDefault();
                    if (loanAllian != null)
                    {
                        fileInfo.HoldBy = loanAllian.HoldBy;
                    }
                    phyDto.PhysicalFileInfo = fileInfo;
                }
            }
            await this.AssembleUserNameAsync(phyDto);
            return phyDto;
        }

        private async Task<PhysicalObjectDto> GetPhysicalObjectById(Guid id, PhysicalObjectDto phyNodeInfo, string templateIdPath, int nodeType)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var nodeInfo = phyNodeInfo;
                if (nodeType <= (int)RMNodeLevel.PhysicalBottomLocation)
                {
                    var nodeId = 0;
                    if (nodeId != 0)
                    {
                        result = await LocationManagementService.GetPhysicalObjectByIdAsync(nodeId);
                        result.ChildTemplates = await TemplateManagementService.GetAllTemplatesByLocationId4ExplorerAsync(result.Id);
                    }
                }
                else
                {
                    var nodeId = id;
                    if (nodeId != Guid.Empty)
                    {
                        result = await ExplorerService.GetPhysicalObjectByIdAsync(nodeId, true);
                        result.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(nodeId);
                        result.TermFullPath = TaxonomyService.GetTermPathByTermId(result.TermId);
                        if (result.MetaInfo == null)
                        {
                            result.MetaInfo = new Dictionary<string, string>();
                        }
                        if (result.Id != Guid.Empty)
                        {
                            if (nodeInfo != null)
                            {
                                result.Template = await TemplateManagementService.LoadTemplateDtoAsync(result.TemplateId, nodeInfo);
                            }
                            else
                            {
                                result.Template = await TemplateManagementService.LoadTemplateDtoAsync(result.TemplateId, result);
                            }
                            await ExplorerService.ConvertDateTimeColumnValueTimeZoneAsync(result);

                            if (result.NodeType == RMNodeType.PhyBox || result.NodeType == RMNodeType.PhyFile)
                            {
                                await ExplorerService.GetPhysicalBarcodeInfoAsync(result);
                            }
                            //result.ChildTemplates = TemplateManagementService.GetTemplatesByPhysicalObject4Explorer(result);
                            if (result.NodeType != RMNodeType.PhyRecord)
                            {
                                result.ChildTemplates = await TemplateManagementService.GetTemplatesByIdPathAsync(result.Template.uniqueId, templateIdPath, Convert2TemplateType(nodeType));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error, [{ex.ToString()}]");
                result = null;
            }
            return result;
        }

        private List<TemplateType> Convert2TemplateType(int nodeType)
        {
            switch (nodeType)
            {
                case (int)RMNodeLevel.PhysicalCustom:
                    return new List<TemplateType> { TemplateType.Box, TemplateType.Folder, TemplateType.Custom };
                case (int)RMNodeLevel.PhysicalBox:
                    return new List<TemplateType> { TemplateType.Folder };
                case (int)RMNodeLevel.PhysicalFile:
                    return new List<TemplateType> { TemplateType.Records };
                default:
                    throw new ArgumentException("nodeType is invalid");
            }
        }

        public async Task<PhysicalRequestDto> GetRequestByRecordIdAsync(string recordId)
        {
            var domain = PhysicalRequestDao.GetRequestByPhysicalRecordId(recordId);
            var phyDto = await ConvertDomain2DtoAsync(new List<RMPhysicalRequest> { domain });
            await this.AssembleUserNameAsync(phyDto);
            return phyDto;
        }

        public async Task<PhysicalRequestResult> QueryAsync(PhysicalRequestParam query)
        {
            PhysicalRequestResult result = new PhysicalRequestResult();
            try
            {
                int totalCount = 0;
                //Add permission Control
                var userPermission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId);
                var isPhysicalAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
                if (userPermission.IsAdmin || !isPhysicalAdmin)
                {
                    Expression<Func<RMPhysicalRequest, bool>> queryExpr = this.GetExpression(query);
                    (List<PhysicalQueryRequestDto> domainList, totalCount) = await PhysicalRequestDao.QueryAuthorizedAsync(query, query.PageIndex, query.PageSize, isPhysicalAdmin, queryExpr);
                    List<PhysicalRequestDto> physicalRequestDtos = await this.ConvertQueryDto2RequestDtoAsync(domainList);
                    result.TotalCount = physicalRequestDtos.Count;
                    result.RequestList = physicalRequestDtos.Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToList();
                }
                else
                {
                    result = await QueryPhysicalRequestWithScopePermission(query, result, userPermission, isPhysicalAdmin);
                    totalCount = result.TotalCount;
                }

                var requestPhysicalIds = new List<Guid>();
                foreach (var request in result.RequestList)
                {
                    if (request.PhysicalFileInfos != null)
                    {
                        foreach (var physicalFileInfo in request.PhysicalFileInfos)
                        {
                            if (physicalFileInfo != null)
                                requestPhysicalIds.Add(physicalFileInfo.Id);
                        }
                    }
                }
                var loanAllians = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(requestPhysicalIds);

                foreach (PhysicalRequestDto request in result.RequestList)
                {
                    if (request.PhysicalFileInfos != null)
                    {
                        foreach (var physicalFileInfo in request.PhysicalFileInfos)
                        {
                            if (physicalFileInfo != null)
                            {
                                var loanAllian = RecordLoanAllianceDao.GetPhyRecordAllianceById(physicalFileInfo.Id).FirstOrDefault();
                                if (loanAllian != null)
                                {
                                    physicalFileInfo.HoldBy = loanAllian.HoldBy;
                                }
                            }
                        }
                    }
                }

                await this.AssembleUserNameAsync(result.RequestList);
                await this.AssembleTimeDisplayAsync(result.RequestList, true);
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = e.Message;
                logger.Warn(e.Message, e);
            }
            return result;
        }

        private async Task<PhysicalRequestResult> QueryPhysicalRequestWithScopePermission(PhysicalRequestParam query, PhysicalRequestResult result, SecurityUserPermissionsDto userPermission, bool isPhysicalAdmin)
        {
            var physicalPermission = userPermission.ScopePermissionInfo.Where(_ => _.DataSourceType == SourceFlag.Physical).FirstOrDefault();
            var locationPermissionIds = physicalPermission?.ScopeIds ?? new List<Guid>();
            var bottomLocationIds = LocationDao.LoadAllLocationBottomIdUnderTopLocation(locationPermissionIds);
            Expression<Func<RMPhysicalRequest, bool>> queryExpr = this.GetExpression(query);
            (List<PhysicalQueryRequestDto> domainList, int totalCount) = await PhysicalRequestDao.QueryPhyRequestByBottomLocationIdsAsync(query, query.PageIndex, query.PageSize, isPhysicalAdmin, bottomLocationIds, queryExpr);
            List<PhysicalRequestDto> physicalRequestDtos = await this.ConvertQueryDto2RequestDtoAsync(domainList);
            result.TotalCount = physicalRequestDtos.Count;
            result.RequestList = physicalRequestDtos.Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToList();
            return result;
        }

        private Expression<Func<RMPhysicalRequest, bool>> GetExpression(PhysicalRequestParam query)
        {
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMPhysicalRequest), "c");

            if (query.Filters != null)
            {
                //用OR合并一个Filter选的多个值的表达式
                foreach (var f in query.Filters)
                {
                    var exps = f.ColumnValues.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMPhysicalRequest), param, this.GetFilterColumnName(f.Column), c));
                    var filterExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(filterExpression);
                }
            }
            if (!string.IsNullOrEmpty(query.SearchText))
            {
                if (query.SearchText.Length > 4 && query.SearchText.StartsWith(RecordsConstants.RequestIdPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    string integerText = query.SearchText.Substring(3, query.SearchText.Length - 3);  //"RC-456" --> "456"
                    int tempId = 0;
                    bool isIntText = int.TryParse(integerText, out tempId);
                    //search text is RC-XXXX
                    var exps = this.GetStaticSearchKeys(isIntText).Select(searchKey =>
                    {
                        if (searchKey == "Id")
                        {
                            return Expression4DynamicQuery.GetEqualExpression(typeof(RMPhysicalRequest), param, "Id", integerText);
                        }
                        else
                        {
                            return Expression4DynamicQuery.GetContainsExpression(typeof(RMPhysicalRequest), param, searchKey, query.SearchText);
                        }
                    });
                    var searchExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(searchExpression);
                }
                else if ("RC".Equals(query.SearchText, StringComparison.OrdinalIgnoreCase) || RecordsConstants.RequestIdPrefix.Equals(query.SearchText, StringComparison.OrdinalIgnoreCase))
                {
                    //search text is RC or RC-
                    //do not add any further expression
                }
                else
                {
                    IEnumerable<Expression> exps = null;
                    int tempId = 0;
                    if (int.TryParse(query.SearchText, out tempId))
                    {
                        exps = this.GetStaticSearchKeys(true).Select(searchKey =>
                        {
                            if (searchKey == "Id")
                            {
                                return Expression4DynamicQuery.GetEqualExpression(typeof(RMPhysicalRequest), param, "Id", tempId);
                            }
                            else
                            {
                                return Expression4DynamicQuery.GetContainsExpression(typeof(RMPhysicalRequest), param, searchKey, query.SearchText);
                            }
                        });
                    }
                    else
                    {
                        exps = this.GetStaticSearchKeys(false).Select(searchKey => Expression4DynamicQuery.GetContainsExpression(typeof(RMPhysicalRequest), param, searchKey, query.SearchText));
                    }
                    if (exps != null)
                    {
                        var searchExpression = exps.Aggregate(Expression.OrElse);
                        allExpressionList.Add(searchExpression);
                    }
                }
            }
            if (allExpressionList.Count > 0)
            {
                //将多个Filter和search都用AND合并
                Expression queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                var lambda = Expression.Lambda<Func<RMPhysicalRequest, bool>>(queryExpr, param);
                return lambda;
            }
            else
            {
                return null;
            }
        }

        private List<string> GetStaticSearchKeys(bool hasIdColumn)
        {
            //Column Name in DM Model
            if (hasIdColumn)
            {
                return new List<string>() { "Id", "PhysicalFileId", "Title" };
            }
            else
            {
                return new List<string>() { "PhysicalFileId", "Title" };
            }
        }
        private string GetFilterColumnName(PhysicalRequestFilterColumn column)
        {
            switch (column)
            {
                case PhysicalRequestFilterColumn.Status:
                    return "Status";
                case PhysicalRequestFilterColumn.Type:
                    return "Type";
                case PhysicalRequestFilterColumn.RequestBy:
                    return "CreatedUserId";
                default:
                    return "";
            }
        }

        public Dictionary<int, object> GetFilterDataSource()
        {
            var requestByList = PhysicalRequestDao.GetRequestBy();
            Dictionary<int, object> filterSource = new Dictionary<int, object>
            {
                { (int)PhysicalRequestFilterColumn.RequestBy, requestByList }
            };
            return filterSource;
        }
        #endregion

        #region Create or Update

        private void Validate(PhysicalRequestDto dto)
        {
            dto.ValidateName();
            dto.ValidateSize(false);
            if (dto.PhysicalFileInfos != null)
            {
                foreach (var physicalFileInfos in dto.PhysicalFileInfos)
                {
                    if (physicalFileInfos.NodeType == RMNodeType.PhyBox || physicalFileInfos.NodeType == RMNodeType.PhyFile)
                    {
                        dto.ValidateStatus();
                        dto.ValidateTerm(TermDao);
                    }
                }
            }
            if (dto.PhysicalFileInfo != null)
            {
                if (dto.PhysicalFileInfo.NodeType == RMNodeType.PhyBox || dto.PhysicalFileInfo.NodeType == RMNodeType.PhyFile)
                {
                    dto.ValidateStatus();
                    dto.ValidateTerm(TermDao);
                }
            }
            dto.ValidateHomeLocation(LocationDao);
        }

        [Audit(Action = AuditAction.SavePhysicalRequest, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> CreateAsync(PhysicalRequestDto dto)
        {
            PhysicalRequestResult result = new PhysicalRequestResult();
            List<RMPhysicalRequest> mails = new List<RMPhysicalRequest>();
            Guid groupRequestId = Guid.NewGuid();
            try
            {
                if (dto.Type == PhysicalRequestType.Creation)
                {
                    if (dto.PhysicalFileInfos == null && dto.PhysicalFileInfo == null)
                    {
                        logger.Warn("No physical file infomation in the creation request {0}", dto.Title);
                        result.HasError = true;
                        result.ErrorMsg = "Physical File Info is null"; //不需要国际化
                        return result;
                    }
                    Validate(dto);
                    dto.Title = dto.PhysicalFileInfo.Name;
                    dto.RecordId = dto.PhysicalFileInfo.UniqueId;
                }
                var entries = ConvertUtil.ConvertDto2Domain(dto);
                entries.ForEach(e =>
                {
                    e.Item2.CreatedTime = DateTime.UtcNow.Ticks;
                    e.Item2.ModifiedTime = e.Item2.CreatedTime;
                    e.Item2.CreatedUserId = Contract.Tenant.TenantLocalValue.LogonUserId;
                });
                foreach (var entry in entries) // key is physical file info and value is physical request
                {
                    if (entry.Item1 != null && entry.Item1.ScopePerDto != null)
                    {
                        var syncUserResult = await PermissionManagementService.SyncADUsersAsync(entry.Item1.ScopePerDto.Accounts);
                        if (syncUserResult.MessageType != RAMessageType.Successful)
                        {
                            result.HasError = false;
                            result.ErrorMsg = syncUserResult.ErrorMessage;
                        }
                        else
                        {
                            entry.Item2.ScopePermissionInfo = SerializerHelper.SerializeByDataContractSerializer(entry.Item1.ScopePerDto);
                        }
                    }
                    if (!result.HasError)
                    {
                        entry.Item2.GroupRequestId = groupRequestId;
                        mails.Add(PhysicalRequestDao.Create(entry.Item2));
                    }
                    logger.Info("Create physcial request sucessfull, title {0}, type {2}, id {3}", entry.Item2.Title, entry.Item2.Type, entry.Item2.Id);
                }
            }
            catch (ClassificationInvalidException e)
            {
                logger.Error($"An error while new request, messsage:{e}");
                result.HasError = true;
                result.ErrorMsg = I18NEntity.GetString(e.Message);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                result.HasError = true;
                result.ErrorMsg = e.Message;
            }
            if (!result.HasError)
            {
                PhysicalRequestDto mailTempDto = await this.ConvertDomain2DtoAsync(mails);
                if (dto.Type == PhysicalRequestType.Creation)
                {
                    await this.SendEmailNotificationAsync(EmailTemplateInternalType.CreationRequestToEndUser, mailTempDto);
                    await this.SendEmailNotificationAsync(EmailTemplateInternalType.CreationRequestToRM, mailTempDto);
                }
                if (dto.Type == PhysicalRequestType.Move)
                {
                    await this.SendEmailNotificationAsync(EmailTemplateInternalType.MoveRequestToEndUser, mailTempDto);
                    await this.SendEmailNotificationAsync(EmailTemplateInternalType.MoveRequestToRM, mailTempDto);
                }
                else
                {
                    await this.SendEmailNotificationAsync(EmailTemplateInternalType.LoanRequsetToEndUser, mailTempDto);
                    await this.SendEmailNotificationAsync(EmailTemplateInternalType.LoanRequsetToRM, mailTempDto);
                }
            }
            return result;
        }

        [Audit(Action = AuditAction.UpdatePhysicalRequest, Category = AuditCategory.PhyscialRequestManagement, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler), BeforeHandler = typeof(PhysicalRequestBeforeAuditHandler))]
        public async Task<PhysicalRequestResult> UpdateAsync(PhysicalRequestDto dto)
        {

            PhysicalRequestResult result = new PhysicalRequestResult();
            try
            {
                if (dto.Type == PhysicalRequestType.Creation && dto.PhysicalFileInfo == null)
                {
                    logger.Warn("No physical file infomation in the creation request {0}", dto.Title);
                    result.HasError = true;
                    result.ErrorMsg = "Physical File Info is null"; //不需要国际化
                    return result;
                }
                var entries = ConvertUtil.ConvertDto2Domain(dto);
                entries.ForEach(e =>
                {
                    e.Item2.ModifiedTime = DateTime.UtcNow.Ticks;
                });
                foreach (var entry in entries)
                {
                    if (entry.Item1 != null && entry.Item1.ScopePerDto != null)
                    {
                        var syncUserResult = await PermissionManagementService.SyncADUsersAsync(entry.Item1.ScopePerDto.Accounts);
                        if (syncUserResult.MessageType != RAMessageType.Successful)
                        {
                            result.HasError = false;
                            result.ErrorMsg = syncUserResult.ErrorMessage;
                        }
                        else
                        {
                            entry.Item2.ScopePermissionInfo = SerializerHelper.SerializeByDataContractSerializer(entry.Item1.ScopePerDto);
                        }
                    }
                    if (!result.HasError)
                    {
                        await PhysicalRequestDao.UpdateAsync(entry.Item2);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                result.HasError = true;
                result.ErrorMsg = e.Message;
            }
            return result;
        }
        [Audit(Action = AuditAction.LoanPhysicalRequest, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> LoanRequestAsync(LoanRequestDto dto)
        {
            var listRecord = new List<RecordDto>();
            IExplorerDao explorerDao = new ExplorerDao();
            PhysicalRequestResult result = new PhysicalRequestResult();
            DateTime endDateTime = DateTime.MinValue;
            result = await CheckRequestInfoAsync(dto);
            if (result.HasError)
            {
                return result;
            }
            if (dto.ReturnDate != null)
            {
                //this.ConvertTimeToUtc(dto.ReturnDate.DateTimeStr, dto.ReturnDate.TimeZoneId, dto.ReturnDate.AutoAdjustClock);
                endDateTime = DateTime.Parse(dto.ReturnDate.DateTimeStr);
                endDateTime = DateTime.SpecifyKind(endDateTime, DateTimeKind.Unspecified);
                endDateTime = DateTimeUtil.ConvertTimeToUtcDate(endDateTime, GeneralSettingConfig.FindSystemTimeZoneById(dto.ReturnDate.TimeZoneId), !dto.ReturnDate.AutoAdjustClock);
                DateTime utcNow = DateTime.UtcNow;
                if (endDateTime < utcNow)
                {
                    result.HasError = true;
                    result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_JS_Common_AUI_Datepicker_Earlier");
                    return result;
                }
            }
            var requestDto = new PhysicalRequestDto
            {
                Type = PhysicalRequestType.Loan,
                CreatedUserId = Contract.Tenant.TenantLocalValue.LogonUserId,
                HoldUserId = dto.OnBehalf[0].UserId,
                HoldUserDisplay = dto.OnBehalf[0].DisplayName,
                Comment = dto.Comment,
                DisposalClass = new PhysicalRequestDisposal
                {
                    HoldCategory = HoldCategory.Before,
                    EndTime = endDateTime.Ticks,
                    EndTimeStr = dto.ReturnDate != null ? dto.ReturnDate.DateTimeStr : "",
                    TimeZoneId = dto.ReturnDate != null ? dto.ReturnDate.TimeZoneId : "",
                    IsDaylightSavingTime = dto.ReturnDate != null ? dto.ReturnDate.AutoAdjustClock : false
                }
            };
            requestDto.PhysicalFileInfos = new List<PhysicalObjectDto>();

            foreach (var item in dto.Items)
            {
                Guid.TryParse(item.Id, out Guid itemId);
                var phyFileInfo = explorerDao.GetPhysicalRecordById(itemId);
                requestDto.PhysicalFileInfos.Add(new PhysicalObjectDto() { Id = itemId, NodeType = item.NodeType, Name = item.Name, UniqueId = item.UniqueId, LocationId = phyFileInfo?.LocationId ?? Guid.Empty });
                listRecord.Add(ConvertUtil.ConvertRecord2RecordDto(phyFileInfo));
            }
            if (CheckOnHoldOrLoanedPhysicalItems(listRecord, explorerDao))
            {
                result.HasError = true;
                result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_LR_Common_Refusal");
                result.FailedType = EPhysicalRequestType.PopupMessage;
                return result;
            }
            var itemRst = await this.CreateAsync(requestDto);
            if (itemRst.HasError)
            {
                result.HasError = true;
                result.ErrorMsg = itemRst.ErrorMsg;
            }
            return result;
        }
        public bool CheckItemOnHold(List<Guid> ids)
        {
            try
            {
                var listRecord = new List<RecordDto>();
                IExplorerDao explorerDao = new ExplorerDao();
                foreach (var id in ids)
                {
                    var phyFileInfo = explorerDao.GetPhysicalRecordById(id);
                    listRecord.Add(ConvertUtil.ConvertRecord2RecordDto(phyFileInfo));
                }
                return this.CheckOnHoldOrLoanedPhysicalItems(listRecord, explorerDao);
            }
            catch (Exception ex)
            {
                logger.Error("check item on hold error:{0}", ex.ToString());
                return false;
            }
        }

        private bool CheckLoanedPhysicalItems(List<RecordDto> items,IExplorerDao explorerDao)
        {
            var itemIds = items.Select(item => item.Id).ToList();

            var boxIds = items.Where(item => item.NodeType == (int)RMNodeType.PhyBox).Select(item => item.Id).ToList();

            if (boxIds.Count > 0)
            {
                var foldersUnderBoxes = explorerDao.GetChildRecordsByBoxIds(boxIds);

                itemIds.AddRange(foldersUnderBoxes.Select(folder => folder.Id));
            }

            var recordLoanAlliances = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(itemIds);

            return recordLoanAlliances?.Any() == true;
        }

        private bool CheckOnHoldOrLoanedPhysicalItems(List<RecordDto> listItems, IExplorerDao explorerDao)
        {
            if (!listItems.Any()) return false;
            var listTypeBoxes = listItems.Where(x => x.NodeType == (int)RMNodeType.PhyBox).ToList();
            var listTypeFolders = listItems.Where(x => x.NodeType == (int)RMNodeType.PhyFile).ToList();
            if (listTypeBoxes.Any())
            {
                var listChild = explorerDao.GetChildRecordsByBoxIds(listItems.Select(x => x.Id).ToList());
                var childsOnHold = listChild.Where(x => x.HoldStatus == true).ToList();
                if (this.GetItemsOnHold(listItems).Any() || childsOnHold.Any())
                {
                    return true;
                }
            }
            if (listTypeFolders.Any())
            {
                var IsParentExistHold = explorerDao.GetHoldRecordsByIds(new List<Guid>() { listItems.First().Id, listItems.First().BoxId });
                if (IsParentExistHold.Any() || this.GetItemsOnHold(listItems).Any())
                {
                    return true;
                }
            }
            return false;
        }

        private List<RecordDto> GetItemsOnHold(List<RecordDto> listItems)
        {
            var listItemOnHold = listItems
               .Where(x => x.HoldStatus == true)
               .ToList();
            return listItemOnHold;
        }

        #endregion

        #region Review


        [Audit(Action = AuditAction.ApprovePhysicalRequest, Category = AuditCategory.PhyscialRequestManagement, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> ApproveAsync(PhysicalRequestParam param)
        {
            IExplorerDao explorerDao = new ExplorerDao();
            var isBatchOperations = param.Requests.Count > 1;
            PhysicalRequestResult result = new PhysicalRequestResult();
            Dictionary<string, string> requestUserCache = new Dictionary<string, string>();
            DateTime endDateTime = DateTime.MinValue;
            //string holdUserId = string.Empty;

            var checkInvalid = false;
            checkInvalid = ValidateApprove(param, result, ref endDateTime);
            if (checkInvalid)
            {
                return result;
            }
            try
            {
                Dictionary<int, TemplateDto> templates = new Dictionary<int, TemplateDto>();
                if (param.Requests != null && param.Requests.Count > 0)
                {
                    logger.Info("Approve request {0}", string.Join(",", param.Requests.Select(a => a.Id).ToArray()));
                    //Approval可以修改Hold信息以及Comments, 需要统计失败的Item
                    List<int> failedId = new List<int>();
                    //页面传的Dto少大量的属性, 缓存DB中的Request用于发邮件
                    Dictionary<Guid, List<RMPhysicalRequest>> mailTempList = new();
                    if (param.ResendIdList != null && param.ResendIdList.Count > 0)
                    {
                        logger.Info("Approve request resend ids {0}", string.Join(",", param.ResendIdList));
                        param.Requests = param.Requests.Where(r => param.ResendIdList.Contains(r.Id)).ToList();
                    }

                    List<RMPhysicalRequest> rmRequest = ConvertUtil.ConvertDto2Domain(param.Requests).OrderBy(r => r.Id).ToList();
                    var requestIds = rmRequest.Select(_ => _.Id).ToList();
                    var dbReuqests = PhysicalRequestDao.GetRequestByIds(requestIds);
                    var cannotOperateRequests = dbReuqests.Where(r => r.Status != (int)PhysicalRequestStatus.WaitingForApproval).ToList();
                    var physicalField = dbReuqests.Select(r => r.PhysicalFileId).ToList();
                    var listItem = explorerDao.GetRecordByRecordsIds(physicalField);
                    var listItemOnHold = listItem.Where(x => x.HoldStatus == true).ToList();
                    var recordOnHold = new List<RecordDto>();
                    foreach (var itemRecord in listItem)
                    {
                        recordOnHold.Add(ConvertUtil.ConvertRecord2RecordDto(itemRecord));
                    }

                    if(param.Requests.First().Type == PhysicalRequestType.Move)
                    {
                        return HandleMoveApprove(rmRequest, param);
                    };

                    if (listItemOnHold.Any() || this.CheckOnHoldOrLoanedPhysicalItems(recordOnHold, explorerDao))
                    {
                        result.HasError = true;
                        result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_PRE_Request_UnderHoldProtection");
                        result.FailedType = EPhysicalRequestType.PopupMessage;
                        return result;
                    }
                    if (cannotOperateRequests.Count > 0)
                    {
                        result.HasError = true;
                        result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_PRE_Request_ApprovalRequestStatusError", string.Join(",", cannotOperateRequests.Select(r => RecordsConstants.RequestIdPrefix + r.Id)));
                        return result;
                    }
                    var loanBoxIds = new List<Guid>();
                    var loanFolderIds = new List<Guid>();
                    var allLoanRequest = dbReuqests.Where(l => l.Status == (int)PhysicalRequestStatus.WaitingForApproval && l.Type == (int)PhysicalRequestType.Loan);
                    var loanBoxRequests = new List<Tuple<Guid, AOSUserDto, long>>();
                    foreach (var request in allLoanRequest)
                    {
                        try
                        {
                            var physicalObjectMetaData = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(request.MetaData);
                            if (physicalObjectMetaData.NodeType == RMNodeType.PhyBox)
                            {
                                loanBoxIds.Add(physicalObjectMetaData.Id);
                            }
                            else if (physicalObjectMetaData.NodeType == RMNodeType.PhyFile)
                            {
                                loanFolderIds.Add(physicalObjectMetaData.Id);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"deserialize physical file info error, {e}");
                        }
                    }
                    if (RecordsConstants.PhysicalLoanOrReturnBatchOperationMaxCount < ExplorerService.GetPhyBoxAndFileCountByBoxIds(loanBoxIds) + loanFolderIds.Count)
                    {
                        var jobParam = new BoxLoanJobMessage()
                        {
                            LoanAction = LoanAction.Loan,
                            RequestsParam = param
                        };
                        StartLoanOrReturnBoxJob(JobType.PhysicalLoanBox, jobParam);
                        result.StartLoanBoxJob = true;
                        return result;
                    }

                    var approveErrorType4ResultMessage = PhysicalRequestFailType.None;
                    foreach (RMPhysicalRequest request in rmRequest)
                    {
                        if (param.IgnoreReturnDateExpired && param.ResendIdList != null && !param.ResendIdList.Contains(request.Id))
                        {
                            continue;
                        }
                        RMPhysicalRequest dbRequest = dbReuqests.FirstOrDefault(a => a.Id == request.Id);
                        if (dbRequest == null)
                        {
                            logger.Warn("Request {0}, id {1} has been approved or rejected already", request.Title, request.Id);
                            failedId.Add(request.Id);
                            continue;
                        }
                        List<RMPhysicalRequest> groupRequest = dbRequest.GroupRequestId == Guid.Empty ? new List<RMPhysicalRequest> { dbRequest }
                                        : dbReuqests.Where(_ => _.GroupRequestId == dbRequest.GroupRequestId).ToList();
                        if (groupRequest != null && groupRequest.Count > 0 && groupRequest[0].Status == (int)PhysicalRequestStatus.WaitingForApproval)
                        {
                            foreach (var phyRequest in groupRequest)
                            {
                                var metaInfo = phyRequest.MetaData;
                                AOSUserDto aosHoldUser = null;
                                if (phyRequest.Type == (int)PhysicalRequestType.Creation)
                                {
                                    metaInfo = await ApproveCreationRequestAsync(templates, phyRequest, metaInfo, result, requestUserCache);
                                }
                                else if (phyRequest.Type == (int)PhysicalRequestType.Loan)
                                {
                                    if (isBatchOperations)
                                    {
                                        if (phyRequest.EndTime > 0)
                                        {
                                            if (phyRequest.EndTime < DateTime.UtcNow.Ticks && !param.IgnoreReturnDateExpired)
                                            {
                                                failedId.Add(request.Id);
                                                result.HasError = true;
                                                approveErrorType4ResultMessage = PhysicalRequestFailType.ReturnTimeExpired;
                                                break;
                                            }
                                            else
                                            {
                                                endDateTime = new DateTime(phyRequest.EndTime);
                                            }
                                        }
                                        else
                                        {
                                            endDateTime = DateTime.MinValue;
                                        }
                                        if (!string.IsNullOrEmpty(phyRequest.HoldUserId))
                                        {
                                            aosHoldUser = (await AccountDao.GetUserByUserIdAsync(phyRequest.HoldUserId))?.Convert2AOSUser();
                                        }
                                        else
                                        {
                                            aosHoldUser = new AOSUserDto() { DisplayName = phyRequest.HoldByDisplayName };
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(request.HoldUserId))
                                        {
                                            aosHoldUser = (await AccountDao.GetUserByUserIdAsync(request.HoldUserId))?.Convert2AOSUser();
                                        }
                                        else
                                        {
                                            aosHoldUser = new AOSUserDto() { DisplayName = request.HoldByDisplayName };
                                        }
                                    }
                                    //更新RecordAlliance, Insert or Update PersonalHold的记录
                                    //更新PhysicalFile的状态到Hold
                                    ExplorerService.UpdatePhysicalRecordState2Hold(new List<string> { phyRequest.PhysicalFileId }, aosHoldUser, endDateTime.Ticks);
                                    var loanRequest = param.Requests.FirstOrDefault(r => r.Id == dbRequest.Id);
                                    try
                                    {
                                        loanRequest.PhysicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(phyRequest.MetaData);
                                    }
                                    catch (Exception e)
                                    {
                                        logger.Warn($"deserialize physical file info error, {e}");
                                    }
                                    if (loanRequest.PhysicalFileInfo != null)
                                    {
                                        if (loanRequest.PhysicalFileInfo != null && loanRequest.PhysicalFileInfo.NodeType == RMNodeType.PhyBox)
                                        {
                                            loanBoxRequests.Add(new Tuple<Guid, AOSUserDto, long>(loanRequest.PhysicalFileInfo.Id, aosHoldUser, endDateTime.Ticks));
                                        }
                                    }
                                }
                                phyRequest.Status = (int)PhysicalRequestStatus.Approved;
                                phyRequest.HoldCategory = request.HoldCategory;
                                phyRequest.HoldNumber = request.HoldNumber;
                                phyRequest.HoldUnit = request.HoldUnit;
                                phyRequest.MetaData = metaInfo;
                                phyRequest.ReviewComment = request.ReviewComment;
                                if (phyRequest.Type != (int)PhysicalRequestType.Loan || !isBatchOperations)
                                {
                                    phyRequest.HoldUserId = request.HoldUserId;
                                    phyRequest.HoldByDisplayName = request.HoldByDisplayName;
                                    phyRequest.EndTime = this.CalculateEndtime(request);
                                    phyRequest.EndTimeStr = request.EndTimeStr;
                                    phyRequest.TimeZoneId = request.TimeZoneId;
                                    phyRequest.IsDaylightSavingTime = request.IsDaylightSavingTime;
                                }
                                phyRequest.ModifiedTime = DateTime.UtcNow.Ticks;
                            }
                            PhysicalRequestDao.BatchUpdate(groupRequest);
                            if (mailTempList.ContainsKey(dbRequest.GroupRequestId))
                                mailTempList[dbRequest.GroupRequestId].AddRange(groupRequest);
                            else
                                mailTempList[dbRequest.GroupRequestId] = groupRequest;
                        }
                        else
                        {
                            logger.Warn("Request {0}, id {1} has been approved or rejected already", request.Title, request.Id);
                            failedId.Add(request.Id);
                        }
                    }
                    if (loanBoxRequests.Count > 0)
                    {
                        using (PerformanceScope scope = new PerformanceScope("Approve.Loan.Box"))
                        {
                            await ApproveLoanBoxRequestAsync(loanBoxRequests);
                        }
                    }
                    if (failedId.Count > 0)
                    {
                        result.FailedIdList = failedId;
                        result.HasError = true;
                        switch (approveErrorType4ResultMessage)
                        {
                            case PhysicalRequestFailType.None:
                                break;
                            //case PhysicalRequestFailType.IsLoanedRecord:
                            //    result.ErrorMsg = string.Format(result.ErrorMsg, string.Join(",", failedId.Select(id => RecordsConstants.RequestIdPrefix + id)));
                            //    break;
                            case PhysicalRequestFailType.ReturnTimeExpired:
                                //result.ErrorMsg = string.Format(I18N.Core.I18NEntity.GetString("RM_PRE_Request_ReturnTimeExpired"), string.Join(", ", failedId.Select(id => RecordsConstants.RequestIdPrefix + id)));
                                result.HasError = false;
                                result.NeedConfirmIgnoreReturnDate = true;
                                break;
                            default:
                                break;
                        }
                    }
                    if (failedId.Count != rmRequest.Count)
                    {
                        foreach (var re in mailTempList)
                        {
                            if (!failedId.Any(id => re.Value.Any(_ => _.Id == id)))
                            {
                                if (re.Key == Guid.Empty)
                                {
                                    foreach (var request in re.Value)
                                    {
                                        await this.SendEmailNotificationAsync(request.Type == (int)PhysicalRequestType.Creation ? EmailTemplateInternalType.CreationRequestApproved : EmailTemplateInternalType.LoanRequsetApproved,
                                    await this.ConvertDomain2DtoAsync(new List<RMPhysicalRequest> { request }));
                                    }
                                }
                                else
                                {
                                    await this.SendEmailNotificationAsync(re.Value[0].Type == (int)PhysicalRequestType.Creation ? EmailTemplateInternalType.CreationRequestApproved : EmailTemplateInternalType.LoanRequsetApproved,
                                    await this.ConvertDomain2DtoAsync(re.Value));
                                } 
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = e.Message;
                logger.Warn(e.Message, e);
            }
            return result;
        }

        private PhysicalRequestResult HandleMoveApprove(List<RMPhysicalRequest> rmRequests, PhysicalRequestParam param)
        {
            var result = new PhysicalRequestResult();

            var requestIds = rmRequests.Select(r => r.Id).ToList();
            var dbRequests = PhysicalRequestDao.GetRequestByIds(requestIds);

            var invalidRequests = dbRequests.Where(r => r.Status != (int)PhysicalRequestStatus.WaitingForApproval).ToList();

            if (invalidRequests.Any())
            {
                result.HasError = true;
                result.ErrorMsg = I18NEntity.GetString("RM_PRE_Request_ApprovalRequestStatusError",string.Join(",",invalidRequests.Select(r => RecordsConstants.RequestIdPrefix + r.Id)));

                return result;
            }
            var modifiedTicks = DateTime.UtcNow.Ticks;
            var reviewComments = rmRequests.ToDictionary(r => r.Id, r => r.ReviewComment);
            var moveInfos = param.Requests.ToDictionary(r => r.Id, r => r.MoveDto);

            foreach (var dbRequest in dbRequests)
            {
                dbRequest.Status = (int)PhysicalRequestStatus.Approved;
                dbRequest.ModifiedTime = modifiedTicks;
                if (reviewComments.TryGetValue(dbRequest.Id, out var reviewComment))
                {
                    dbRequest.ReviewComment = reviewComment;
                }
                if (moveInfos.TryGetValue(dbRequest.Id, out var moveInfo))
                {
                    var moveDtoDB = SerializerHelper.DeserializeByDataContractSerializer<PhysicalMoveOption>(dbRequest.MoveInfo);
                    moveDtoDB.IsSendEmailToDestinationRM = moveInfo.IsSendEmailToDestinationRM;
                    dbRequest.MoveInfo = SerializerHelper.SerializeByDataContractSerializer(moveDtoDB);
                }

            }
            PhysicalRequestDao.BatchUpdate(dbRequests);
            try
            {
                logger.Info("Start creating move job.");
                var groupedRequests = dbRequests.GroupBy(x => x.GroupRequestId).OrderBy(g => g.Min(x => x.CreatedTime)).ToList();
                var physicalMoveRequests = groupedRequests.Select(r => new PhysicalMoveRequest
                {
                    GroupRequestId = r.Key,
                    PhysicalMoveOption = SerializerHelper.DeserializeByDataContractSerializer<PhysicalMoveOption>(r.First().MoveInfo),
                }).ToList();
                if (RecordsConstants.PhysicalMoveBatchOperationMaxCount < dbRequests.Count)
                {
                    StartMoveJob(physicalMoveRequests);
                    result.MoveResult = new()
                    {
                        IsStartJob = true,
                        JobId = string.Empty,
                    };
                    return result;
                }
                var message = ExplorerService.PhysicalMoves(physicalMoveRequests);

                result.MoveResult = new()
                {
                    IsStartJob = false,
                    JobId = message.Extension
                }
                ;

                logger.Info($"Move job created successfully. JobId: {message.Extension}");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to create move job after approving requests.", ex);

                result.HasError = true;
                result.ErrorMsg = ex.Message;
            }

            return result;
        }
        private bool ValidateApprove(PhysicalRequestParam param, PhysicalRequestResult result, ref DateTime endDateTime)
        {
            var isBatchOperations = param.Requests.Count > 1;
            bool checkFailed = false;
            //When batch processing, verify that the same objects are included
            if (param.Requests.FirstOrDefault()?.Type == PhysicalRequestType.Loan && isBatchOperations)
            {
                var groups = param.Requests.Select(r => new
                {
                    Request = r,
                    RecordIds = r.RecordIds != null && r.RecordIds.Any()
                        ? r.RecordIds
                        : new List<string> { r.RecordId }
                })
                    .SelectMany(x => x.RecordIds.Select(id => new { RecordId = id, Request = x.Request })).GroupBy(g => g.RecordId)
                    .Select(g => new { Name = g.Key, Count = g.Count(), Requests = g.Select(r => r.Request.RequestId) });
                var sameObjectGroup = groups.FirstOrDefault(g => g.Count > 1);
                if (sameObjectGroup != null)
                {
                    result.HasError = true;
                    result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_PRE_Request_ApprovalSameObject", string.Join(",", sameObjectGroup.Requests));
                    checkFailed = true;
                }
            }

            //If the return time is not ignored during single selection, you need to verify whether the return time is earlier than the current time
            if (param.Requests.FirstOrDefault()?.Type == PhysicalRequestType.Loan && !isBatchOperations && !param.IgnoreReturnDateExpired)
            {
                var disposalClass = param.Requests.First().DisposalClass;
                if (!string.IsNullOrEmpty(disposalClass.EndTimeStr))
                {
                    endDateTime = DateTime.Parse(disposalClass.EndTimeStr);
                    endDateTime = DateTime.SpecifyKind(endDateTime, DateTimeKind.Unspecified);
                    endDateTime = DateTimeUtil.ConvertTimeToUtcDate(endDateTime, GeneralSettingConfig.FindSystemTimeZoneById(disposalClass.TimeZoneId), !disposalClass.IsDaylightSavingTime);
                    DateTime utcNow = DateTime.UtcNow;
                    if (endDateTime < utcNow)
                    {
                        result.HasError = true;
                        result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_JS_Common_AUI_Datepicker_Earlier");
                        checkFailed = true;
                    }
                }
            }
            return checkFailed;
        }

        private async System.Threading.Tasks.Task<string> ApproveCreationRequestAsync(Dictionary<int, TemplateDto> templates, RMPhysicalRequest dbRequest, string metaInfo, PhysicalRequestResult result, Dictionary<string, string> requestUserCache)
        {
            TemplateDto tempTemplate = null;
            //根据MetaDAta, 创建一条File记录
            PhysicalObjectDto physicalRecord = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(metaInfo);
            if (!templates.TryGetValue(physicalRecord.TemplateId, out tempTemplate))
            {
                tempTemplate = await TemplateManagementService.LoadTemplateDtoAsync(physicalRecord.TemplateId);
                templates[physicalRecord.TemplateId] = tempTemplate;
            }
            physicalRecord.Template = tempTemplate;
            physicalRecord.ParentId = physicalRecord.Ancestors.Last();
            string requestUserName = await GetOrCacheRequestUserName(requestUserCache, dbRequest.CreatedUserId);

            physicalRecord.CreatedBy = requestUserName.IsNullOrEmpty() ? TenantLocalValue.DisplayName : requestUserName;

            if (!string.IsNullOrEmpty(dbRequest.ScopePermissionInfo) && physicalRecord.NodeType != Contract.RMWeb.Tree.Base.RMNodeType.PhyRecord)
            {
                //PhysicalRecord不能单独设置权限需要过滤掉
                //如果Request中设置过权限，创建Physical数据时需要带上权限信息
                AppendScopePermissionInfo(physicalRecord, dbRequest.ScopePermissionInfo);

                //添加权限记录并给PhysicalObjectDto赋值ScopePermissionId
                var permissionDto = PermissionManagementService.ConvertToScopePermissionDto(physicalRecord);
                var addPermissionResult = await PermissionManagementService.SavePermissionForNewPhysicalAsync(permissionDto, physicalRecord);
                if (addPermissionResult.MessageType != RAMessageType.Successful)
                {
                    result.HasError = true;
                    result.ErrorMsg = addPermissionResult.ErrorMessage;
                    return metaInfo;
                }
            }
            else
            {
                try
                {
                    //老数据request直接Approve逻辑
                    var scopeFullPath = PermissionManagementService.GetScopeIdFullPath(physicalRecord);
                    physicalRecord.ScopePermissionId = PermissionManagementService.GetScopePermissionId(scopeFullPath, false);
                }
                catch (Exception ex)
                {
                    logger.Warn($"An error when set permissionId for compatible with old request data, name:{physicalRecord.Name},id:{physicalRecord.Id},message:{ex.ToString()}");
                }
            }

            var response = await ExplorerService.AddOrUpdatePhysicalObjectAsync(physicalRecord);
            logger.Info("Creation request, create file info result {0}", response.MessageType);
            if (response.MessageType != Contract.Object.RAMessageType.Successful)
            {
                PermissionManagementService.DeletePermissionInfo(physicalRecord.Id.ToString());
                logger.Warn("Automaticly add physical file record failed. {0}", response.ErrorMessage);
                result.HasError = true;
                result.ErrorMsg = response.ErrorMessage;
                if (response.ErrorMessage == I18NEntity.GetString("RM_Phy_Import_BarcodeDuplicateError"))
                {
                    throw new Exception(I18NEntity.GetString("RM_Phy_Import_BarcodeDuplicateError"));
                }
                return metaInfo;
            }
            dbRequest.PhysicalFileId = physicalRecord.UniqueId;
            physicalRecord.Template = null;
            AddLoanInfo(physicalRecord);
            metaInfo = GCommon.Utility.SerializerHelper.SerializeByDataContractSerializer(physicalRecord);
            return metaInfo;
        }

        private async Task<string> GetOrCacheRequestUserName(Dictionary<string, string> userCache, string userId)
        {
            if (userId.IsNullOrEmpty())
                return userId;

            if (userCache.TryGetValue(userId, out string cachedName))
                return cachedName;

            var requestedUser = await AccountDao.GetUserByUserIdAsync(userId);
            if (requestedUser != null)
            {
                userCache[userId] = requestedUser.DisplayName;
                return requestedUser.DisplayName;
            }

            return string.Empty;
        }

        private void AddLoanInfo(PhysicalObjectDto dto)
        {
            try
            {
                if (dto.PersonHold && !string.IsNullOrEmpty(dto.PersonHoldBy) && !dto.Id.Equals(Guid.Empty))
                {
                    RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = dto.Id, HoldBy = dto.PersonHoldBy, HoldReleaseTime = DateTime.MaxValue.Ticks, ParentId = dto.BoxId });
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error while add loan info, message: {ex}");
            }
        }



        private async Task<List<Tuple<ItemActionResult, PhysicalObjectDto>>> ApproveLoanBoxRequestAsync(List<Tuple<Guid, AOSUserDto, long>> requests)
        {
            List<Tuple<ItemActionResult, PhysicalObjectDto>> resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            foreach (var request in requests)
            {
                resultList.AddRange(await ExplorerService.UpdatePhyFilesHoldStateByBoxIdAsync(request));
            }
            return resultList;
        }

        public RAReturnMessage StartLoanOrReturnBoxJob(JobType jobType, BoxLoanJobMessage param)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(param),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }
        public RAReturnMessage StartMoveJob(List<PhysicalMoveRequest> param)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalMoveDataJob,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(param),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhyscialRequestManagement, Action = AuditAction.PhyLoanBoxJob, AfterHandler = typeof(PhysicalRequestAfterAuditHandler), BeforeHandler = typeof(PhysicalRequestBeforeAuditHandler))]
        public async Task<string> RealRunStartLoanOrReturnBoxJobAsync(JobType jobType, string param)
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType.ToString(), subJobId),
                });

                logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunGlobalSearchActionJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                Runable = jobState == JobStatus.InProgress ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }


        public async Task<List<Guid>> GetLoanObjectIdsAsync(long fromTicks, long toTicks)
        {
            var s = (await RecordLoanAllianceDao.FindListAsync(o => o.HoldReleaseTime <= toTicks && o.HoldReleaseTime > fromTicks))
                .Select(o => o.RecordsId)
                .Distinct()
                .ToList();
            return s;
        }

        public List<PhysicalObjectDto> GetLoanFolderByBoxIds(List<Guid> guids)
        {
            var allLoanedPhyFolders = new List<PhysicalObjectDto>();
            var allLoanedFolderIds = RecordLoanAllianceDao.GetPhyFoldersIdByBoxIds(guids);
            var pageSize = 100;
            for (int pageIndex = 0; pageIndex <= allLoanedFolderIds.Count / pageSize; pageIndex++)
            {
                var loanedIds = allLoanedFolderIds.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                allLoanedPhyFolders.AddRange(ExplorerService.GetAllLoanedFolders(loanedIds));
            }
            return allLoanedPhyFolders;
        }

        [Audit(Action = AuditAction.MobileApprovalLoanRequest, Category = AuditCategory.Mobile, Module = AuditModule.Mobile, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> ApproveLoanForMobileAsync(MobileApprovalLoanDto requestDto)
        {
            PhysicalRequestResult result = new PhysicalRequestResult
            {
                //RequestList = param.Requests
            };
            DateTime endDateTime = DateTime.MinValue;
            var isBatchOperations = requestDto.RequestDtos.Count > 1;
            if (!isBatchOperations)
            {
                try
                {
                    logger.Info($"requst dto {requestDto.RequestDtos[0].ReturnTime}");
                    endDateTime = new DateTime(requestDto.RequestDtos[0].ReturnTime, DateTimeKind.Utc);
                    DateTime utcNow = DateTime.UtcNow;
                    if (endDateTime < utcNow)
                    {
                        List<int> failedId = new List<int>();
                        failedId.Add(requestDto.RequestDtos[0].RequestId);
                        result.HasError = true;
                        result.FailedIdList = failedId;
                        result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_JS_Common_AUI_Datepicker_Earlier");
                        return result;
                    }
                }
                catch (Exception de)
                {
                    logger.Info($"init return time failed {de}");
                    result.HasError = true;
                    result.ErrorMsg = "Invalid return time";
                    return result;
                }
            }

            try
            {
                logger.Info("Approve request {0}", string.Join(",", requestDto.RequestDtos.Select(a => a.RequestId).ToArray()));
                List<int> failedId = new List<int>();
                //页面传的Dto少大量的属性, 缓存DB中的Request用于发邮件
                List<RMPhysicalRequest> mailTempList = new();
                var approveErrorType4ResultMessage = PhysicalRequestFailType.None;
                var loanBoxRequests = new List<Tuple<Guid, AOSUserDto, long>>();
                foreach (var request in requestDto.RequestDtos)
                {
                    RMPhysicalRequest temp = PhysicalRequestDao.Find(r => r.Id == request.RequestId);

                    if (temp != null && temp.Status == (int)PhysicalRequestStatus.WaitingForApproval)
                    {
                        if (temp.Type == (int)PhysicalRequestType.Loan)
                        {
                            if (isBatchOperations)
                            {
                                if (temp.EndTime < DateTime.UtcNow.Ticks)
                                {
                                    //If it is an error that the requested document has been loaned, this error will be displayed first.
                                    if (approveErrorType4ResultMessage != PhysicalRequestFailType.IsLoanedRecord)
                                    {
                                        failedId.Add(request.RequestId);
                                        result.HasError = true;
                                        approveErrorType4ResultMessage = PhysicalRequestFailType.ReturnTimeExpired;
                                    }
                                    continue;
                                }
                                else
                                {
                                    endDateTime = new DateTime(temp.EndTime);
                                }
                            }
                            //更新RecordAlliance, 插入一条PersonalHold的记录
                            //更新PhysicalFile的状态到Hold
                            var userHoldResult = ExplorerService.UpdatePhysicalRecordState2Hold(new List<string> { temp.PhysicalFileId }, (await AccountDao.GetUserByUserIdAsync(request.HoldUserId))?.Convert2AOSUser(), endDateTime.Ticks); //RECO-5067
                            if (userHoldResult.MessageType == Contract.Object.RAMessageType.Failed)
                            {
                                failedId.Add(request.RequestId);
                                result.HasError = true;
                                approveErrorType4ResultMessage = PhysicalRequestFailType.IsLoanedRecord;
                                result.ErrorMsg = userHoldResult.ErrorMessage;
                                continue;
                            }
                        }
                        else
                        {
                            logger.Info($"Request : Id is {temp.Id}, Group request id is {temp.GroupRequestId}, type is {temp.Type}, no need to process.");
                            continue;
                        }
                        PhysicalObjectDto physicalFileInfo = null;
                        try
                        {
                            physicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(temp.MetaData);
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"deserialize physical file info error, {e}");
                        }
                        AOSUserDto aosHoldUser = null;
                        if (!string.IsNullOrEmpty(request.HoldUserId))
                        {
                            aosHoldUser = (await AccountDao.GetUserByUserIdAsync(request.HoldUserId))?.Convert2AOSUser();
                        }
                        if (aosHoldUser == null)
                        {
                            aosHoldUser = new AOSUserDto { Id = request.HoldUserId };
                        }
                        if (physicalFileInfo != null)
                        {
                            if (physicalFileInfo != null && physicalFileInfo.NodeType == RMNodeType.PhyBox)
                            {
                                loanBoxRequests.Add(new Tuple<Guid, AOSUserDto, long>(physicalFileInfo.Id, aosHoldUser, endDateTime.Ticks));
                            }
                        }
                        temp.Status = (int)PhysicalRequestStatus.Approved;
                        //HoldCategory ，holdNumber， HoldUnit 属性目前不使用，所以不进行更新
                        //temp.HoldCategory = request.HoldCategory;
                        //temp.HoldNumber = request.HoldNumber;
                        //temp.HoldUnit = request.HoldUnit;
                        temp.HoldUserId = request.HoldUserId;
                        temp.ReviewComment = requestDto.ReviewComments;
                        temp.EndTime = request.ReturnTime;
                        //temp.EndTimeStr = this.ConvertToTimeString(request.ReturnTime,temp.TimeZoneId);
                        //由于Mobile 端不显示时区以及时令，所以approval 的时候不需要更新
                        //temp.TimeZoneId = request.TimeZoneId;
                        //temp.IsDaylightSavingTime = request.IsDaylightSavingTime;
                        temp.ModifiedTime = DateTime.UtcNow.Ticks;
                        temp.GroupRequestId = Guid.NewGuid();
                        await PhysicalRequestDao.UpdateAsync(temp);
                        mailTempList.Add(temp);
                    }
                    else
                    {
                        logger.Warn($"Request {request.RequestId} has been approved or rejected.");
                        failedId.Add(request.RequestId);
                    }
                }
                if (loanBoxRequests.Count > 0)
                {
                    using (PerformanceScope scope = new PerformanceScope("Approve.Loan.Box"))
                    {
                        await ApproveLoanBoxRequestAsync(loanBoxRequests);
                    }
                }
                if (failedId.Count > 0)
                {
                    result.FailedIdList = failedId;
                    result.HasError = true;
                    switch (approveErrorType4ResultMessage)
                    {
                        case PhysicalRequestFailType.None:
                            break;
                        case PhysicalRequestFailType.IsLoanedRecord:
                            result.ErrorMsg = string.Format(result.ErrorMsg, string.Join(",", failedId.Select(id => RecordsConstants.RequestIdPrefix + id)));
                            break;
                        case PhysicalRequestFailType.ReturnTimeExpired:
                            result.ErrorMsg = string.Format(I18N.Core.I18NEntity.GetString("RM_PRE_Request_ReturnTimeExpired"), string.Join(", ", failedId.Select(id => RecordsConstants.RequestIdPrefix + id)));
                            break;
                        default:
                            break;
                    }
                }
                if (failedId.Count != requestDto.RequestDtos.Count)
                {
                    foreach (var re in mailTempList)
                    {
                        if (!failedId.Contains(re.Id))
                        {
                            await this.SendEmailNotificationAsync(EmailTemplateInternalType.LoanRequsetApproved,
                        await this.ConvertDomain2DtoAsync(new List<RMPhysicalRequest> { re }));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = e.Message;
                logger.Warn($"An error occure in approve Loan request for mobile, reason : {e.ToString()}.");
            }
            return result;
        }

        [Audit(Action = AuditAction.RejectPhysicalRequest, Category = AuditCategory.PhyscialRequestManagement, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> RejectAsync(PhysicalRequestParam param)
        {
            PhysicalRequestResult result = new PhysicalRequestResult();
            try
            {
                if (param.Requests != null && param.Requests.Count > 0)
                {
                    logger.Info("Reject request {0}", string.Join(",", param.Requests.Select(a => a.Id).ToArray()));
                    List<int> failedId = new List<int>();

                    //页面传的Dto少大量的属性, 缓存DB中的Request用于发邮件
                    Dictionary<Guid, List<RMPhysicalRequest>> mailTempList = new Dictionary<Guid, List<RMPhysicalRequest>>();

                    //Approval可以修改Hold信息以及Comments, 需要统计失败的Item
                    List<RMPhysicalRequest> rmRequest = ConvertUtil.ConvertDto2Domain(param.Requests);
                    var requestIds = rmRequest.Select(_ => _.Id).ToList();
                    var dbReuqests = PhysicalRequestDao.GetRequestByIds(requestIds);
                    var cannotOperateRequests = dbReuqests.Where(r => r.Status != (int)PhysicalRequestStatus.WaitingForApproval).ToList();
                    if (cannotOperateRequests.Count > 0)
                    {
                        result.HasError = true;
                        result.ErrorMsg = I18N.Core.I18NEntity.GetString("RM_PRE_Request_ApprovalRequestStatusError", string.Join(",", cannotOperateRequests.Select(r => RecordsConstants.RequestIdPrefix + r.Id)));
                        return result;
                    }
                    foreach (RMPhysicalRequest request in rmRequest)
                    {
                        RMPhysicalRequest temp = dbReuqests.FirstOrDefault(a => a.Id == request.Id);
                        List<RMPhysicalRequest> groupRequest = temp.GroupRequestId == Guid.Empty ? new List<RMPhysicalRequest> { temp }
                                    : dbReuqests.Where(r => r.GroupRequestId == temp.GroupRequestId).ToList();
                        if (groupRequest != null && groupRequest.Count > 0 && groupRequest[0].Status == (int)PhysicalRequestStatus.WaitingForApproval)
                        {
                            //reject只更新状态和Comment
                            foreach (var phyRequest in groupRequest)
                            {
                                phyRequest.Status = (int)PhysicalRequestStatus.Rejected;
                                phyRequest.ReviewComment = request.ReviewComment;
                                phyRequest.ModifiedTime = DateTime.UtcNow.Ticks;
                            }
                            PhysicalRequestDao.BatchUpdate(groupRequest);
                            if (mailTempList.ContainsKey(temp.GroupRequestId))
                                mailTempList[temp.GroupRequestId].AddRange(groupRequest);
                            else
                                mailTempList[temp.GroupRequestId] = groupRequest;
                        }
                        else
                        {
                            logger.Warn("Request {0}, id {1} has been approved or rejected already", request.Title, request.Id);
                            failedId.Add(request.Id);
                        }
                    }
                    if (failedId.Count > 0)
                    {
                        result.FailedIdList = failedId;
                        result.HasError = true;
                    }
                    if (failedId.Count != rmRequest.Count)
                    {
                        foreach (var re in mailTempList)
                        {
                            if (!re.Value.Any(_ => failedId.Contains(_.Id)))
                            {
                                if (re.Key == Guid.Empty)
                                {
                                    foreach (var request in re.Value)
                                    {
                                        await this.SendEmailNotificationAsync(GetRejectedTemplate(request.Type),
                                            await this.ConvertDomain2DtoAsync(new List<RMPhysicalRequest> { request }));
                                    }
                                }
                                else
                                {
                                    await this.SendEmailNotificationAsync(GetRejectedTemplate(re.Value[0].Type),
                                    await this.ConvertDomain2DtoAsync(re.Value));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = e.Message;
                logger.Warn(e.Message, e);
            }
            return result;
        }
        private EmailTemplateInternalType GetRejectedTemplate(int requestType)
        {
            return requestType switch
            {
                (int)PhysicalRequestType.Creation => EmailTemplateInternalType.CreationRequestRejected,
                (int)PhysicalRequestType.Move => EmailTemplateInternalType.MoveRequestRejected,
                _ => EmailTemplateInternalType.LoanRequsetRejected
            };
        }


        [Audit(Action = AuditAction.CancelRequest, Category = AuditCategory.PhyscialRequestManagement, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> CancelRequestAsync(PhysicalRequestParam param)
        {
            PhysicalRequestResult result = new PhysicalRequestResult();
            try
            {
                if (param.Requests != null && param.Requests.Count > 0)
                {
                    logger.Info("Reject request {0}", string.Join(",", param.Requests.Select(a => a.Id).ToArray()));
                    List<int> failedId = new List<int>();
                    List<RMPhysicalRequest> rmRequest = ConvertUtil.ConvertDto2Domain(param.Requests);
                    foreach (RMPhysicalRequest request in rmRequest)
                    {
                        var temps = PhysicalRequestDao.GetRequest(request.Id);
                        if (temps != null && temps[0].Status == (int)PhysicalRequestStatus.WaitingForApproval)
                        {
                            //CancelRequest只更新状态
                            foreach (var temp in temps)
                            {
                                temp.Status = (int)PhysicalRequestStatus.CancelRequest;
                                temp.ModifiedTime = DateTime.UtcNow.Ticks;
                                await PhysicalRequestDao.UpdateAsync(temp);
                            }
                        }
                        else
                        {
                            logger.Warn("Request {0}, id {1} has been approved or rejected already", request.Title, request.Id);
                            failedId.Add(request.Id);
                        }
                    }
                    if (failedId.Count > 0)
                    {
                        result.FailedIdList = failedId;
                        result.HasError = true;
                    }
                }
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorMsg = e.Message;
                logger.Warn(e.Message, e);
            }
            return result;
        }

        #endregion

        #region Convert Func

        private async Task<PhysicalRequestDto> ConvertDomain2DtoAsync(List<RMPhysicalRequest> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return null;
            }
            var domain = requests[0];
            PhysicalRequestDto dto = new PhysicalRequestDto();
            dto.Id = domain.Id;
            dto.RequestId = RecordsConstants.RequestIdPrefix + domain.Id;  //display as REC-123
            dto.Type = (PhysicalRequestType)domain.Type;
            dto.Status = (PhysicalRequestStatus)domain.Status;
            dto.CreatedTime = domain.CreatedTime;
            dto.ModifiedTime = domain.ModifiedTime;
            dto.CreatedUserId = domain.CreatedUserId;
            dto.ManagerUserId = domain.ManagerUserId;
            dto.HoldUserId = domain.HoldUserId;
            dto.HoldUserDisplay = domain.HoldByDisplayName;
            dto.Comment = domain.Comment;
            dto.DisposalClass = new PhysicalRequestDisposal();
            dto.DisposalClass.HoldCategory = (HoldCategory)domain.HoldCategory;
            dto.DisposalClass.HoldNumber = domain.HoldNumber;
            dto.DisposalClass.HoldUnit = (HoldUnit)domain.HoldUnit;
            dto.GroupRequestId = domain.GroupRequestId;
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var endDateTime = DateTimeUtil.ConvertTimeFromUtc(domain.EndTime, gls);

            dto.DisposalClass.TimeZoneId = gls.TimeZoneId;
            dto.DisposalClass.IsDaylightSavingTime = gls.DayLight;
            dto.DisposalClass.EndTimeStr = domain.EndTime > 0 ? endDateTime.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT) : "";

            dto.DisposalClass.EndTime = domain.EndTime;
            dto.DisposalClass.ReviewComment = domain.ReviewComment;
            if (domain.GroupRequestId == Guid.Empty)
            {
                dto.Title = domain.Title;
                dto.RecordId = domain.PhysicalFileId;
                if (domain.MetaData != null)
                {
                    dto.PhysicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(domain.MetaData);
                }
                if (dto.PhysicalFileInfo != null)
                {
                    if (!string.IsNullOrEmpty(domain.ScopePermissionInfo))
                        dto.PhysicalFileInfo.ScopePerDto = SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectPermissionDto>(domain.ScopePermissionInfo);
                    dto.PhysicalFileInfo.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(dto.PhysicalFileInfo);
                    dto.PhysicalFileInfo.TermFullPath = TaxonomyService.GetTermPathByTermId(dto.PhysicalFileInfo.TermId);
                }
            }
            else
            {
                if (dto.PhysicalFileInfos == null) dto.PhysicalFileInfos = new List<PhysicalObjectDto>();
                dto.Titles = requests.Select(_ => _.Title).ToList();
                dto.RecordIds = requests.Select(_ => _.PhysicalFileId).ToList();
                foreach (var request in requests)
                {
                    PhysicalObjectDto physicalFileInfo = null;
                    if (request.MetaData != null)
                    {
                        physicalFileInfo = GCommon.Utility.SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(request.MetaData);
                    }
                    if (physicalFileInfo != null)
                    {
                        if (!string.IsNullOrEmpty(request.ScopePermissionInfo))
                            physicalFileInfo.ScopePerDto = SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectPermissionDto>(request.ScopePermissionInfo);
                        physicalFileInfo.HomeLocationFullPath = ExplorerService.GetPhysicalObjectFullPath(physicalFileInfo);
                        physicalFileInfo.TermFullPath = TaxonomyService.GetTermPathByTermId(physicalFileInfo.TermId);
                        dto.PhysicalFileInfos.Add(physicalFileInfo);
                    }
                }
            }
            if(domain.MoveInfo != null)
            {
                dto.MoveDto = SerializerHelper.DeserializeByDataContractSerializer<PhysicalMoveOption>(domain.MoveInfo);

            }
            return dto;
        }

        private async Task<List<PhysicalRequestDto>> ConvertQueryDto2RequestDtoAsync(List<PhysicalQueryRequestDto> queryDto)
        {
            List<PhysicalRequestDto> result = new List<PhysicalRequestDto>();
            Dictionary<Guid, List<PhysicalQueryRequestDto>> dicRequest = new();
            foreach (var dom in queryDto)
            {
                if (dicRequest.ContainsKey(dom.GroupRequestId))
                    dicRequest[dom.GroupRequestId].Add(dom);
                else 
                    dicRequest[dom.GroupRequestId] = new List<PhysicalQueryRequestDto> { dom };
            }
            if (queryDto == null || queryDto.Count == 0)
            {
                return result;
            }
            foreach (var dom in dicRequest)
            {
                result.AddRange(await this.ConvertQueryDto2RequestDtoAsync(dom));
            }
            return result;
        }

        private async Task<List<PhysicalRequestDto>> ConvertQueryDto2RequestDtoAsync(KeyValuePair<Guid, List<PhysicalQueryRequestDto>> dicQueryDto)
        {
            List<PhysicalRequestDto> result = new List<PhysicalRequestDto>();
            if (dicQueryDto.Key == Guid.Empty)
            {
                foreach (var queryDto in dicQueryDto.Value)
                {
                    if (queryDto == null)
                    {
                        return null;
                    }
                    PhysicalRequestDto dto = new PhysicalRequestDto
                    {
                        Id = queryDto.Id,
                        RequestId = RecordsConstants.RequestIdPrefix + queryDto.Id,
                        Type = (PhysicalRequestType)queryDto.Type,
                        Title = queryDto.Title,
                        Status = (PhysicalRequestStatus)queryDto.Status,
                        RecordId = queryDto.PhysicalFileId,
                        CreatedTime = queryDto.CreatedTime,
                        CreatedUserId = queryDto.CreatedUserId,
                        HoldUserId = queryDto.HoldUserId,
                        ManagerUserId = queryDto.ManagerUserId,
                        ModifiedTime = queryDto.ModifiedTime,
                        HoldUserDisplay = queryDto.HoldByDisplayName,
                        GroupRequestId = queryDto.GroupRequestId,
                        MoveDto = queryDto.MoveInfo != null ? SerializerHelper.DeserializeByDataContractSerializer<PhysicalMoveOption>(queryDto.MoveInfo) : null
                    };
                    if (queryDto.MetaData != null)
                    {
                        dto.PhysicalFileInfo = SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(queryDto.MetaData);
                    }
                    GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                    var endTimeStr = DateTimeUtil.ConvertTimeFromUtc(queryDto.EndTime, gls).ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);

                    dto.DisposalClass = new PhysicalRequestDisposal
                    {
                        HoldCategory = (HoldCategory)queryDto.HoldCategory,
                        HoldNumber = queryDto.HoldNumber,
                        HoldUnit = (HoldUnit)queryDto.HoldUnit,
                        IsDaylightSavingTime = gls.DayLight,
                        TimeZoneId = gls.TimeZoneId,
                        EndTimeStr = endTimeStr,
                        EndTime = queryDto.EndTime
                    };
                    result.Add(dto);
                }
            }
            else
            {
                if (dicQueryDto.Value == null || dicQueryDto.Value.Count == 0)
                {
                    return null;
                }
                var queryDto = dicQueryDto.Value;
                queryDto.Reverse();
                PhysicalRequestDto dto = new PhysicalRequestDto
                {
                    Id = queryDto[0].Id,
                    RequestId = RecordsConstants.RequestIdPrefix + queryDto[0].Id,
                    Type = (PhysicalRequestType)queryDto[0].Type,
                    Status = (PhysicalRequestStatus)queryDto[0].Status,
                    CreatedTime = queryDto[0].CreatedTime,
                    CreatedUserId = queryDto[0].CreatedUserId,
                    HoldUserId = queryDto[0].HoldUserId,
                    ManagerUserId = queryDto[0].ManagerUserId,
                    ModifiedTime = queryDto[0].ModifiedTime,
                    HoldUserDisplay = queryDto[0].HoldByDisplayName,
                    GroupRequestId = queryDto[0].GroupRequestId,
                    MoveDto = queryDto[0].MoveInfo != null ? SerializerHelper.DeserializeByDataContractSerializer<PhysicalMoveOption>(queryDto[0].MoveInfo) : null
                };
                if (dto.Titles == null) dto.Titles = new List<string>();
                if (dto.RecordIds == null) dto.RecordIds = new List<string>();
                if (dto.PhysicalFileInfos == null) dto.PhysicalFileInfos = new List<PhysicalObjectDto>();
                foreach (var request in queryDto)
                {
                    dto.Titles.Add(request.Title);
                    dto.RecordIds.Add(request.PhysicalFileId);
                    if (request.MetaData != null)
                    {
                        dto.PhysicalFileInfos.Add(SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectDto>(request.MetaData));
                    }
                }
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                var endTimeStr = DateTimeUtil.ConvertTimeFromUtc(queryDto[0].EndTime, gls).ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);

                dto.DisposalClass = new PhysicalRequestDisposal
                {
                    HoldCategory = (HoldCategory)queryDto[0].HoldCategory,
                    HoldNumber = queryDto[0].HoldNumber,
                    HoldUnit = (HoldUnit)queryDto[0].HoldUnit,
                    IsDaylightSavingTime = gls.DayLight,
                    TimeZoneId = gls.TimeZoneId,
                    EndTimeStr = endTimeStr,
                    EndTime = queryDto[0].EndTime
                };
                result.Add(dto);
            }
            return result;
        }

        private string AssembleDisposalDetail(string CreatedUser, GeneralSettingModel gls, PhysicalRequestDisposal disClass)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CreatedUser).Append(" ");
            if (disClass.HoldCategory == HoldCategory.Last)
            {
                sb.Append("keep for").Append(" ");
                sb.Append(disClass.HoldNumber).Append(" ");
                sb.Append(GetHoldUnitStr(disClass.HoldUnit));
            }
            else
            {
                sb.Append("keep util").Append(" ");
                sb.Append(GeneralSettingService.ConverTiksToDateTime(gls, gls.TimeZoneId, disClass.EndTime));
            }
            return sb.ToString();
        }

        private string GetHoldUnitStr(HoldUnit unit)
        {
            switch (unit)
            {
                case HoldUnit.Day:
                    return "Day(s)";
                case HoldUnit.Month:
                    return "Month(s)";
                case HoldUnit.Year:
                    return "Year(s)";
                default:
                    return "";
            }
        }




        private long ConvertTimeToUtc(string timeStr, string timeZoneId, bool isDaylightSaving)
        {
            DateTime temp = DateTime.Parse(timeStr);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            try
            {
                timeZoneInfo = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception e)
            {
                logger.Error("Get time zone failed by timezoneid {0}", timeZoneId);
                logger.Error(e.Message, e);
            }
            DateTime dest = DateTimeUtil.ConvertTimeToUtcDate(temp, timeZoneInfo, !isDaylightSaving);
            return dest.Ticks;
        }

        private long CalculateEndtime(RMPhysicalRequest disposal)
        {
            if (disposal.HoldCategory == (int)HoldCategory.Before)
            {
                if (!string.IsNullOrEmpty(disposal.EndTimeStr))
                {
                    return this.ConvertTimeToUtc(disposal.EndTimeStr, disposal.TimeZoneId, disposal.IsDaylightSavingTime);
                }
            }
            else if (disposal.HoldCategory == (int)HoldCategory.Last)
            {
                DateTime temp = new DateTime(disposal.CreatedTime, DateTimeKind.Utc);
                if (disposal.HoldUnit == (int)HoldUnit.Year)
                {
                    temp.AddYears(disposal.HoldNumber);
                }
                else if (disposal.HoldUnit == (int)HoldUnit.Month)
                {
                    temp.AddMonths(disposal.HoldNumber);
                }
                else if (disposal.HoldUnit == (int)HoldUnit.Day)
                {
                    temp.AddDays(disposal.HoldNumber);
                }
                else
                {
                    return 0;
                }
                return temp.Ticks;
            }
            return 0;
        }
        #endregion

        #region Assemble User Display Name
        private async System.Threading.Tasks.Task AssembleUserNameAsync(PhysicalRequestDto dto)
        {
            if (dto == null)
            {
                return;
            }
            List<string> userIdList = new List<string>();
            if (dto.CreatedUserId != null)
            {
                userIdList.Add(dto.CreatedUserId);
            }
            if (dto.HoldUserId != null)
            {
                userIdList.Add(dto.HoldUserId);
            }
            if (dto.ManagerUserId != null)
            {
                userIdList.Add(dto.ManagerUserId);
            }
            if (userIdList.Count > 0)
            {
                List<RMAccount> accounts = await this.GetAccountAsync(userIdList);

                RMAccount created = accounts.FirstOrDefault(a => a.UserId == dto.CreatedUserId);
                if (created != null)
                {
                    dto.CreatedUserDisplay = created.DisplayName;
                }
                RMAccount managed = accounts.FirstOrDefault(a => a.UserId == dto.ManagerUserId);
                if (managed != null)
                {
                    dto.ManagerUserDisplay = managed.DisplayName;
                }
                RMAccount hold = accounts.FirstOrDefault(a => a.UserId == dto.HoldUserId);
                if (hold != null)
                {
                    dto.HoldUserDisplay = hold.DisplayName;
                }
            }
        }

        private async System.Threading.Tasks.Task AssembleUserNameAsync(List<PhysicalRequestDto> dtos)
        {
            List<string> userIdList = new List<string>();
            foreach (PhysicalRequestDto dto in dtos)
            {
                if (dto.CreatedUserId != null && !userIdList.Contains(dto.CreatedUserId))
                {
                    userIdList.Add(dto.CreatedUserId);
                }
                if (dto.HoldUserId != null && !userIdList.Contains(dto.HoldUserId))
                {
                    userIdList.Add(dto.HoldUserId);
                }
                if (dto.ManagerUserId != null && !userIdList.Contains(dto.ManagerUserId))
                {
                    userIdList.Add(dto.ManagerUserId);
                }
            }
            if (userIdList.Count > 0)
            {
                List<RMAccount> accounts = await this.GetAccountAsync(userIdList);
                foreach (PhysicalRequestDto dto in dtos)
                {
                    RMAccount created = accounts.FirstOrDefault(a => a.UserId == dto.CreatedUserId);
                    if (created != null)
                    {
                        dto.CreatedUserDisplay = created.DisplayName;
                    }
                    RMAccount managed = accounts.FirstOrDefault(a => a.UserId == dto.ManagerUserId);
                    if (managed != null)
                    {
                        dto.ManagerUserDisplay = managed.DisplayName;
                    }
                    RMAccount hold = accounts.FirstOrDefault(a => a.UserId == dto.HoldUserId);
                    if (hold != null)
                    {
                        dto.HoldUserDisplay = hold.DisplayName;
                    }
                }
            }
        }

        private Task<List<RMAccount>> GetAccountAsync(List<string> userIds)
        {
            ///TODO
            return AccountDao.GetUserByUserIdsAsync(userIds);
        }
        #endregion

        #region Assemble DateTime Display
        private async System.Threading.Tasks.Task AssembleTimeDisplayAsync(List<PhysicalRequestDto> dtos, bool addDetail)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (PhysicalRequestDto dto in dtos)
            {
                dto.CreatedTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, dto.CreatedTime, true).SimplifyFormatTime;
                dto.ModifiedTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, dto.ModifiedTime, true).SimplifyFormatTime;
                if (addDetail)
                {
                    dto.DisposalDetail = this.AssembleDisposalDetail(dto.CreatedUserDisplay, gls, dto.DisposalClass);
                }
            }
        }
        #endregion

        #region Send Email Notification
        private async Task<List<ToUserInfo>> GetMailReceiverAccountAsync(PhysicalRequestDto dto, EmailTemplateInternalType templateInternalType, ParameterDto parameter)
        {
            List<Contract.RMWeb.ReportCenter.ToUserInfo> userList = new List<Contract.RMWeb.ReportCenter.ToUserInfo>();
            switch (templateInternalType)
            {
                case EmailTemplateInternalType.CreationRequestToRM:
                    parameter.Requester = Contract.Tenant.TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(Contract.Tenant.TenantLocalValue.LogonUserId);
                    userList.AddRange(this.GetAdminUsers());
                    break;
                case EmailTemplateInternalType.CreationRequestToEndUser:
                    parameter.Requester = Contract.Tenant.TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(Contract.Tenant.TenantLocalValue.LogonUserId);
                    userList.Add(new Contract.RMWeb.ReportCenter.ToUserInfo() { UserPrincipalName = Contract.Tenant.TenantLocalValue.LogonUserEmail });
                    break;
                case EmailTemplateInternalType.LoanRequsetToEndUser:
                    parameter.Requester = Contract.Tenant.TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(Contract.Tenant.TenantLocalValue.LogonUserId);
                    userList.Add(new Contract.RMWeb.ReportCenter.ToUserInfo() { UserPrincipalName = Contract.Tenant.TenantLocalValue.LogonUserEmail });
                    break;
                case EmailTemplateInternalType.LoanRequsetToRM:
                    parameter.Requester = Contract.Tenant.TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(Contract.Tenant.TenantLocalValue.LogonUserId);
                    userList.AddRange(this.GetAdminUsers());
                    break;
                case EmailTemplateInternalType.CreationRequestApproved:
                case EmailTemplateInternalType.CreationRequestRejected:
                case EmailTemplateInternalType.LoanRequsetApproved:
                case EmailTemplateInternalType.LoanRequsetRejected:
                    RMAccount enduser = await AccountDao.GetUserByUserIdAsync(dto.CreatedUserId);
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(dto.CreatedUserId);
                    parameter.Requester = enduser.DisplayName;
                    userList.Add(new Contract.RMWeb.ReportCenter.ToUserInfo() { UserPrincipalName = enduser.UserPrincipalName });
                    userList.Add(new Contract.RMWeb.ReportCenter.ToUserInfo() { UserPrincipalName = Contract.Tenant.TenantLocalValue.LogonUserEmail });
                    break;
                case EmailTemplateInternalType.MoveRequestToEndUser:
                    parameter.Requester = TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(TenantLocalValue.LogonUserId);
                    userList.Add(new Contract.RMWeb.ReportCenter.ToUserInfo() { UserPrincipalName = TenantLocalValue.LogonUserEmail });
                    break;
                case EmailTemplateInternalType.MoveRequestToRM:
                    parameter.Requester = TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(TenantLocalValue.LogonUserId);
                    userList.AddRange(this.GetAdminUsers());
                    userList.AddRange(await this.GetRecordManagersHavePermissionAsync(dto));
                    break;
                case EmailTemplateInternalType.MoveRequestApprovedToEndUser:
                case EmailTemplateInternalType.MoveRequestRejected:
                    RMAccount endUser = await AccountDao.GetUserByUserIdAsync(dto.CreatedUserId);
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(dto.CreatedUserId);
                    parameter.Requester = endUser.DisplayName;
                    userList.Add(new ToUserInfo() { UserPrincipalName = endUser.UserPrincipalName });
                    break;
                case EmailTemplateInternalType.MoveRequestApprovedToDestinationRM:
                    parameter.Requester = TenantLocalValue.DisplayName;
                    parameter.RequestRequesterFirstname = UserService.GetReviewerFirstName(TenantLocalValue.LogonUserId);
                    userList.AddRange(await this.GetRecordManagersHavePermissionAsync(dto, true));
                    if (!userList.Any()) userList.AddRange(this.GetAdminUsers());
                    break;

            }

            return userList;
        }
        #endregion
        private List<Contract.RMWeb.ReportCenter.ToUserInfo> GetAdminUsers()
        {
            List<Contract.RMWeb.ReportCenter.ToUserInfo> userList = new List<Contract.RMWeb.ReportCenter.ToUserInfo>();
            List<RMAccount> admins = AccountDao.GetAppAdminAccounts();
            admins.ForEach(a =>
            {
                if (a.ObjectType == RMActiveDirectoryObjectType.Group && (a.UserPrincipalName == null || !a.UserPrincipalName.Contains('@')))
                {
                    logger.Warn($"Invalid address '{a.UserPrincipalName}' in group {a.DisplayName}");
                }
                else if (a.ObjectType == RMActiveDirectoryObjectType.UserInGroup)
                {
                    logger.Warn($"Skip user in group {a.DisplayName}");
                }
                else
                {
                    userList.Add(new Contract.RMWeb.ReportCenter.ToUserInfo() { UserPrincipalName = a.UserPrincipalName , DisplayName = a.DisplayName});
                }
            });
            return userList;
        }
        private async Task<List<ToUserInfo>> GetRecordManagersHavePermissionAsync(PhysicalRequestDto dto, bool onlyDestination = false)
        {
            var scopeIds = new List<Guid>();
            if (onlyDestination)
            {
                if (!string.IsNullOrEmpty(dto.MoveDto?.LocationId))
                {
                    scopeIds.Add(new Guid(dto.MoveDto.LocationId));
                }
            }
            else
            {
                scopeIds = dto.PhysicalFileInfos.Select(x => x.LocationId).Distinct().ToList();
            }
            var topLocationIds = new List<Guid>();
            foreach (var scopeId in scopeIds)
            {
                topLocationIds.Add(LocationDao.LoadTopLocationIdBySubLocation(scopeId));
            }

            var result = new List<ToUserInfo>();
            var userIds = _rMScopeRoleAssignmentDao.GetUserIdsByScopeIds(topLocationIds, (int)SourceFlag.Physical);
            var accounts = await AccountDao.GetUserByUserIdsAsync(userIds);
            result.AddRange(accounts.Select(account => new ToUserInfo { UserPrincipalName = account.UserPrincipalName, DisplayName = account.DisplayName }));
            return result;
        }
        /// <summary>
        /// 发送邮件的方法, 在方法执行成功后
        /// </summary>
        /// <param name="templateInternalType"></param>
        /// <param name="requestDto"></param>
        public async System.Threading.Tasks.Task SendEmailNotificationAsync(EmailTemplateInternalType templateInternalType, PhysicalRequestDto requestDto, ParameterMoveDto moveDto = null)
        {
            try
            {
                ParameterDto param = new ParameterDto();
                param.RequestID = requestDto.RequestId;
                param.RequsetComment = templateInternalType == EmailTemplateInternalType.CreationRequestToRM || templateInternalType == EmailTemplateInternalType.CreationRequestToEndUser || templateInternalType == EmailTemplateInternalType.LoanRequsetToEndUser || templateInternalType == EmailTemplateInternalType.LoanRequsetToRM || templateInternalType == EmailTemplateInternalType.MoveRequestToEndUser || templateInternalType == EmailTemplateInternalType.MoveRequestToRM
                    ? requestDto.Comment : requestDto.DisposalClass.ReviewComment;
                param.PhscicalRecordName = string.IsNullOrEmpty(requestDto.Title) ? (requestDto.Titles == null ? string.Empty : string.Join(", ", requestDto.Titles)) : requestDto.Title;
                param.PhscicalRecordUID = string.IsNullOrEmpty(requestDto.RecordId) ? (requestDto.RecordIds == null ? string.Empty : string.Join(", ", requestDto.RecordIds)) : requestDto.RecordId;
                //只有审批操作有Assignee
                param.CurrentDate = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.Ticks);
                param.Assignee = templateInternalType == EmailTemplateInternalType.CreationRequestApproved ||
                    templateInternalType == EmailTemplateInternalType.CreationRequestRejected ||
                    templateInternalType == EmailTemplateInternalType.LoanRequsetApproved ||
                    templateInternalType == EmailTemplateInternalType.LoanRequsetRejected ||
                    templateInternalType == EmailTemplateInternalType.MoveRequestApprovedToEndUser ||
                    templateInternalType == EmailTemplateInternalType.MoveRequestRejected ?
                   Contract.Tenant.TenantLocalValue.DisplayName : null;
                if (templateInternalType == EmailTemplateInternalType.LoanRequsetToRM || templateInternalType == EmailTemplateInternalType.CreationRequestToRM || templateInternalType == EmailTemplateInternalType.MoveRequestToRM)
                {
                    param.Assignee = I18NEntity.GetString("RM_CP_AccountManagement_Admin");
                }
                List<Contract.RMWeb.ReportCenter.ToUserInfo> users = await GetMailReceiverAccountAsync(requestDto, templateInternalType, param);
                MapMoveInfoToEmail(templateInternalType, param, moveDto, requestDto);
                if (users == null || users.Count == 0)
                {
                    logger.Error("No mail receiver found , request id {0}, requester {1}", requestDto.RequestId, requestDto.CreatedUserDisplay);
                    return;
                }
                logger.Info($"Start to send email, id: {param.RequestID}, requester: {param.Requester}, file id: {param.PhscicalRecordUID}, file title: {param.PhscicalRecordName}");
                await this.SendEmailNotificationAsync(templateInternalType, param, users);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }

        }
        private void MapMoveInfoToEmail(EmailTemplateInternalType type, ParameterDto paramDto, ParameterMoveDto moveDto, PhysicalRequestDto requestDto)
        {
            paramDto.MoveInfo = new ParameterMoveDto
            {
                DestinationLocation = requestDto?.MoveDto?.DestinationPath
            };
            var moveTemplate = new List<EmailTemplateInternalType>
            {
                EmailTemplateInternalType.MoveRequestApprovedToEndUser,
                EmailTemplateInternalType.MoveRequestApprovedToDestinationRM,
            };
            if (moveTemplate.Contains(type))
            {
                paramDto.MoveInfo = new ParameterMoveDto
                {
                    SuccessfullCount = moveDto.SuccessfullCount,
                    FailedCount = moveDto.FailedCount,
                    DestinationLocation = moveDto.DestinationLocation,
                    OriginalLocation = moveDto.OriginalLocation,
                    DestinationRM = I18NEntity.GetString("RM_CP_AccountManagement_Admin"),
                };
                paramDto.Assignee = TenantLocalValue.DisplayName ?? AccountDao.GetUserWithRemovedByPrincipalNames([TenantLocalValue.LogonUserEmail]).FirstOrDefault()?.DisplayName;
            }
        }

        private async System.Threading.Tasks.Task SendEmailNotificationAsync(EmailTemplateInternalType templateInternalType, ParameterDto param, List<Contract.RMWeb.ReportCenter.ToUserInfo> users)
        {
            var glsSetting = await GeneralSettingService.GetGeneralSettingAsync();
            EmailTemplateDto template = EmailTemplateService.GetEmailTemplateByInternalType(templateInternalType);
            MailUtil.SendEmailTemplate(template, param, users, glsSetting.EmailSenderDefinition);
        }

        private void AppendScopePermissionInfo(PhysicalObjectDto dto, string permissionInfo)
        {
            if (!string.IsNullOrEmpty(permissionInfo))
            {
                var permissionDto = SerializerHelper.DeserializeByDataContractSerializer<PhysicalObjectPermissionDto>(permissionInfo);
                dto.ScopePerDto = permissionDto;
            }
        }

        private async Task<PhysicalRequestResult> CheckRequestInfoAsync(LoanRequestDto dto)
        {
            var result = new PhysicalRequestResult
            {
                HasError = false
            };
            try
            {
                #region 验证传过来dto中是否有request items
                if (dto.Items == null || dto.Items.Count == 0)
                {
                    result.HasError = true;
                }
                #endregion

                #region 验证OnBehalf是否非空
                if (dto.OnBehalf.Count != 1 || dto.OnBehalf[0] == null)
                {
                    result.HasError = true;
                }
                #endregion

                //#region Register user to AOS
                //UserService.SyncUsers(TenantLocalValue.LogonGroupId, dto.OnBehalf);
                //#endregion

                #region 验证end user对physical folder是否有权限申请loan request.
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
                if (!isAdmin)
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);

                    var nodeIds = dto.Items.Select(o => new Guid(o.Id)).ToList();
                    var nodePermissionIds = ExplorerService.GetPhysicalObjectPermissionIds(nodeIds);
                    if (nodePermissionIds.Count > 0)
                    {
                        var scopePermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                        if (nodePermissionIds.Any(o => !scopePermissionIds.Contains(o) && o != 0))
                        {
                            result.HasError = true;
                        }
                    }
                }


                #endregion

                result.ErrorMsg = result.HasError ? "Illegal request information." : "";
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred when check loan request info, message:{ex.ToString()}");
            }
            return result;
        }

        private async Task<PhysicalRequestResult> CheckMoveRequestInfoAsync(MoveRequestDto dto)
        {
            var result = new PhysicalRequestResult
            {
                HasError = false
            };
            try
            {
                var requestsExist = PhysicalRequestDao.GetMoveRequestsWaitingByPhysicalFileIds(dto.Items.Select(x => x.UniqueId).ToList());
                if (requestsExist.Any())
                {
                    result.HasError = true;
                    var itemNames = string.Join(", ", requestsExist.Select(x => x.Title));
                    result.ErrorMsg = string.Format(I18NEntity.GetString("RM_DSB_PhysicalMove_ExistMoveRequest"), itemNames);

                }
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
                if (!isAdmin)
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);

                    var nodeIds = dto.Items.Select(o => new Guid(o.Id)).ToList();
                    var nodePermissionIds = ExplorerService.GetPhysicalObjectPermissionIds(nodeIds);
                    if (nodePermissionIds.Count > 0)
                    {
                        var scopePermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                        if (nodePermissionIds.Any(o => !scopePermissionIds.Contains(o) && o != 0))
                        {
                            result.HasError = true;
                            result.ErrorMsg = "Illegal request information.";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred when check loan request info, message:{ex.ToString()}");
            }
            return result;
        }

        [Audit(Action = AuditAction.MovePhysicalRequest, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalRequestAfterAuditHandler))]
        public async Task<PhysicalRequestResult> MoveRequestAsync(MoveRequestDto dto)
        {
            var listRecord = new List<RecordDto>();
            IExplorerDao explorerDao = new ExplorerDao();
            PhysicalRequestResult result = new PhysicalRequestResult();
            result = await CheckMoveRequestInfoAsync(dto);
            if (result.HasError)
            {
                return result;
            }
            dto.MoveDto.SourcePhyRecordIds = dto.Items.Select(x => new Guid(x.Id)).ToList();
            var locationId = string.IsNullOrEmpty(dto.MoveDto.LocationId) ? Guid.Empty : Guid.Parse(dto.MoveDto.LocationId);
            var boxId = string.IsNullOrEmpty(dto.MoveDto.BoxId) ? Guid.Empty : Guid.Parse(dto.MoveDto.BoxId);
            var folderId = string.IsNullOrEmpty(dto.MoveDto.FolderId) ? Guid.Empty : Guid.Parse(dto.MoveDto.FolderId);
            dto.MoveDto.DestinationPath = new PhysicalMoveBuilder(explorerDao).BuildDestinationPath(locationId, boxId, folderId);
            dto.MoveDto.IsSendEmailToDestinationRM = false;
            dto.MoveDto.FromModule = (int)AuditCategory.PhysicalExplorerMoveRequest;

            var requestDto = new PhysicalRequestDto
            {
                Type = PhysicalRequestType.Move,
                CreatedUserId = TenantLocalValue.LogonUserId,
                Comment = dto.Comment,
                MoveDto = dto.MoveDto,
            };
            requestDto.PhysicalFileInfos = new List<PhysicalObjectDto>();

            foreach (var item in dto.Items)
            {
                Guid.TryParse(item.Id, out Guid itemId);
                var phyFileInfo = explorerDao.GetPhysicalRecordById(itemId);
                requestDto.PhysicalFileInfos.Add(new PhysicalObjectDto() { Id = itemId, NodeType = item.NodeType, Name = item.Name, UniqueId = item.UniqueId, LocationId = phyFileInfo?.LocationId ?? Guid.Empty, Ancestors = phyFileInfo.Ancestors });
                listRecord.Add(ConvertUtil.ConvertRecord2RecordDto(phyFileInfo));
            }
            if (CheckLoanedPhysicalItems(listRecord, explorerDao))
            {
                result.HasError = true;
                result.ErrorMsg = I18NEntity.GetString("RM_LR_Common_SkipMoveRequest");
                result.FailedType = EPhysicalRequestType.PopupMessage;
                return result;
            }
            var itemRst = await this.CreateAsync(requestDto);
            if (itemRst.HasError)
            {
                result.HasError = true;
                result.ErrorMsg = itemRst.ErrorMsg;
            }
            return result;
        }
        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhyscialRequestManagement, Action = AuditAction.PhyMoveDataJob, AfterHandler = typeof(PhysicalRequestAfterAuditHandler), BeforeHandler = typeof(PhysicalRequestBeforeAuditHandler))]
        public async Task<string> RealRunStartMoveDataJobAsync(string param)
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = RMJobService.CreateJob(JobType.PhysicalMoveDataJob, jobRunByUser);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, JobType.PhysicalMoveDataJob, JobStatus.InProgress, 1, param);

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = JobType.PhysicalMoveDataJob,
                    CommandLine = string.Format("{0} {1}", JobType.PhysicalMoveDataJob.ToString(), subJobId),
                });

                logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunStartMoveDataJobAsync, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        public async Task<PhysicalRequestDto> GetRequestDtoByGroupIdAndStatusAsync(Guid groupRequestId, PhysicalRequestStatus status)
        {
            var requests = PhysicalRequestDao.GetRequestsByGroupRequestId(groupRequestId, status);
            if (requests == null || requests.Count == 0)
            {
                return null;
            }
            return await ConvertDomain2DtoAsync(requests);
        }

        public bool CheckItemsOnLoan(PhysicalRequestParam param)
        {  
            List<RMPhysicalRequest> rmRequest = ConvertUtil.ConvertDto2Domain(param.Requests).OrderBy(r => r.Id).ToList();
            var requestIds = rmRequest.Select(_ => _.Id).ToList();
            var dbReuqests = PhysicalRequestDao.GetRequestByIds(requestIds);
            var cannotOperateRequests = dbReuqests.Where(r => r.Status != (int)PhysicalRequestStatus.WaitingForApproval).ToList();
            var physicalField = dbReuqests.Select(r => r.PhysicalFileId).ToList();
            IExplorerDao explorerDao = new ExplorerDao();
            var listItem = explorerDao.GetRecordByRecordsIds(physicalField);
            var itemIds = listItem.SelectMany(item => item.NodeType == (int)RMNodeLevel.PhysicalRecord ? new[] { item.Id, item.ParentId } : new[] { item.Id }) .Distinct() .ToList();
            return RecordLoanAllianceDao.IsRecordsLoan(itemIds);
        }
    }
}