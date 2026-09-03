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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit.JPMC;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Permission;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.MyHub.Actions;
using AvePoint.RA.Service.Services.MyHub.NewMethods;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using Microsoft.PowerShell.Commands;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FSConnectionOwnerType = AvePoint.RA.DB.Model.FSConnectionOwnerType;


namespace AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler
{
    public class FileSystemServiceAuditHandler : FSAuditHandlerBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(FileSystemServiceAuditHandler));

        #region DAO / Service accessors
        private IFSConnectionGroupDao FSGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private IRMFSConnectionAndOwnerRelationshipDao ConnectionOwnerDao => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IManualProcessManagementService ManualProcessService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IUniqueIdSettingDao UniqueIdSettingDao => PlatformWindsorManager.GetService<IUniqueIdSettingDao>();
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private RMMyhubAuditTrialMethod _myhubAuditTrialMethod;
        private RMMyhubAuditTrialMethod MyhubAuditTrialMethod => _myhubAuditTrialMethod ??= new RMMyhubAuditTrialMethod();
        #endregion

        #region Metadata keys
        private const string MetaOldGroupName = "OldGroupName";
        private const string MetaOldDescription = "OldDescription";
        private const string MetaOldAccessType = "OldAccessType";
        private const string MetaOldConnName = "OldConnName";
        private const string MetaOldConnGroupName = "OldConnGroupName";
        private const string MetaOldUNCPath = "OldUNCPath";
        private const string MetaOldInfoOwners = "OldInformationOwners";
        private const string MetaOldRecOwners = "OldRecordOwners";
        private const string MetaDeletedNames = "DeletedNames";
        private const string MetaOldClassCode = "OldClassCode";
        private const string MetaOldCountryCode = "OldCountryCode";
        private const string MetaOldRetentionType = "OldRetentionType";
        private const string MetaOldStartDate = "OldStartDate";
        private const string MetaOldApplyToSub = "OldApplyToSubFolders";
        private const string MetaOldEnableRM = "OldEnableRM";
        private const string MetaOldEnableDownloadRCCReport = "OldEnableDownloadRCCReport";
        private const string MetaOldSubsetTerm = "OldSubsetTerm";
        private const string MetaOldApproval = "OldApproval";
        private const string MetaOldProcess = "OldProcess";
        private const string MetaOldEmail = "OldEmail";
        private const string MetaOldLocOwners = "OldLocationOwners";
        private const string MetaOldClassLevel = "OldClassificationLevel";
        private const string MetaOldCorrelatedConns = "OldCorrelatedConnections";
        private const string MetaOldScheduleStartTime = "OldScheduleStartTime";
        private const string MetaOldScheduleEndTime = "OldScheduleEndTime";
        private const string MetaOldScheduleInterval = "OldScheduleInterval";
        private const string MetaOldScheduleIsNoSchedule = "OldScheduleIsNoSchedule";
        private const string MetaOldUniqueIdIsActived = "OldUniqueIdIsActived";
        private const string MetaOldUniqueIdPrefix = "OldUniqueIdPrefix";
        private const string MetaOldUniqueIdStore = "OldUniqueIdStore";
        private const string MetaInheritSetting = "InheritSetting";
        private const string MetaOldTimeRange = "OldTimeRange";
        #endregion

        #region Main dispatch

        public override async Task<FSAuditContext> CollectBeforeAsync(FSAuditContext context, FSAuditType auditType, FSAuditLevel auditLevel, object[] arguments)
        {
            var enableJPMCFileSystemFeature = await RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false);
            if (!enableJPMCFileSystemFeature)
            {
                context.ErrorMessage = "JPMC FileSystem feature is disabled. Skipping FS audit.";
                return context;
            }
            ApplyDefaultExecutedBy(context);
            ResolveAuditLevel(context, auditLevel, arguments);
            ResolveObjectName(context, auditType, arguments);
            ResolveHierarchyIds(context, auditType, arguments);
            if (auditType == FSAuditType.PermissionChange && arguments[0] is RMConnectionRecordOwnerUpdateModel)
            {
                return await BeforePermissionChangeAsync(context, arguments);
            }
            if (auditType == FSAuditType.EditFSConnection && !IsEditConnectionChangeOnly(arguments))
            {
                context.AuditType = FSAuditType.PermissionChange;
                return await BeforePermissionChangeAsync(context, arguments);
            }

            try
            {
                return auditType switch
                {
                    FSAuditType.EditFSGroup => BeforeEditGroup(context, arguments),
                    //FSAuditType.DeleteFSGroup => BeforeDeleteGroup(context, arguments),
                    FSAuditType.EditFSConnection => await BeforeEditConnectionAsync(context, arguments),
                    //FSAuditType.DeleteFSConnection => BeforeDeleteConnection(context, arguments),
                    //FSAuditType.FSConnectionCorrelateGroup => BeforeCorrelateGroup(context, arguments),
                    FSAuditType.ApplyClassCodeSettings4FS => await BeforeApplyClassCodeAsync(context, arguments),
                    FSAuditType.MyhubClassify => await BeforeApplyClassCodeAsync(context, arguments),
                    FSAuditType.PermissionChange => await BeforePermissionChangeAsync(context, arguments),
                    FSAuditType.FSEditGeneralSettingForJPMC => BeforeEditGeneralSetting(context, arguments),
                    FSAuditType.FSEditDocLevelSettingForJPMC => BeforeEditDocLevelSetting(context, arguments),
                    FSAuditType.FSEditLocationOwnersSetting => await BeforeEditLocationOwnersAsync(context, arguments),
                    FSAuditType.FSEditInheritSetting => BeforeEditInheritSetting(context, arguments),
                    FSAuditType.ConfigureDisposalJobSchedule4FS => await BeforeConfigureDisposalScheduleAsync(context, arguments),
                    FSAuditType.FSActiveSetting => BeforeActiveSetting(context, arguments),
                    FSAuditType.GenerateDisposalHistory => BeforeGenerateDisposalHistory(context, arguments),
                    _ => context
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error in CollectBeforeAsync for {0}: {1}", auditType, ex);
                return context;
            }
        }

        public override async Task<FSAuditContext> CollectAfterAsync(FSAuditContext context, FSAuditType auditType, FSAuditLevel auditLevel, object[] arguments, object returnValue)
        {
            context.AuditType = GetMultiGeoAction(arguments, context.AuditType);
            var effectiveType = context.AuditType;
            try
            {
                return effectiveType switch
                {
                    FSAuditType.CreateFSGroup => AfterCreateGroup(context, arguments),
                    FSAuditType.EditFSGroup => AfterEditGroup(context, arguments),
                    FSAuditType.CreateFSConnection => AfterCreateConnection(context, arguments),
                    FSAuditType.EditFSConnection => AfterEditConnection(context, arguments, returnValue),
                    //FSAuditType.FSConnectionCorrelateGroup => AfterCorrelateGroup(context, arguments),
                    //FSAuditType.FSConnectionValidationTest => AfterConnectionValidationTest(context, arguments, returnValue),
                    FSAuditType.ApplyClassCodeSettings4FS => await AfterApplyClassCodeAsync(context, arguments, returnValue),
                    FSAuditType.MyhubClassify => await AfterApplyClassCodeAsync(context, arguments, returnValue),
                    FSAuditType.PermissionChange => AfterPermissionChange(context, arguments),
                    FSAuditType.FSEditGeneralSettingForJPMC => AfterEditGeneralSetting(context, arguments),
                    FSAuditType.FSEditDocLevelSettingForJPMC => AfterEditDocLevelSetting(context, arguments),
                    FSAuditType.FSEditLocationOwnersSetting => AfterEditLocationOwners(context, arguments),
                    //FSAuditType.RunEnforceRule => AfterJobRun(context, arguments, returnValue),
                    FSAuditType.RunEnforceRuleWithClassCode => AfterClassCodeJobRun(context, arguments, returnValue),
                    FSAuditType.RunFSDisposalJob => AfterJobRun(context, arguments, returnValue),
                    FSAuditType.RunFSCollectionJob => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.RunFSApplyClassCodeJob => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.RunFSReclassicfyJob => AfterJobRun(context, arguments, returnValue),
                    FSAuditType.RunFSRestoreJob => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.RunFSManageHoldJob => AfterJobRun(context, arguments, returnValue),
                    FSAuditType.RunFSDashboardJob => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.Reclassify => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.RunSyncJob => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.ImportSetting => AfterJobRun(context, arguments, returnValue),
                    //FSAuditType.ExportSetting => AfterJobRun(context, arguments, returnValue),
                    FSAuditType.ConfigureDisposalJobSchedule4FS => await AfterConfigureDisposalScheduleAsync(context, arguments, returnValue),
                    FSAuditType.FSActiveSetting => context,
                    FSAuditType.FSDeactiveSetting => context,
                    FSAuditType.DownloadRCCReport => AfterDownloadRCCReportJob(context, arguments, returnValue),
                    _ => context
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error in CollectAfterAsync for {0}: {1}", effectiveType, ex);
                return context;
            }
        }

        private static bool IsEditConnectionChangeOnly(object[] arguments)
        {
            return arguments.Length > 0
                && arguments[0] is ConnectionDto dto
                && dto.IsEditConnectionPage;
        }

        #endregion

        #region Level resolution

        private static void ApplyDefaultExecutedBy(FSAuditContext context)
        {
            if (context.ExecutedBy == FSAuditExecutedBy.Unknown)
                context.ExecutedBy = FSAuditExecutedBy.User;
        }

        private void ResolveAuditLevel(FSAuditContext context, FSAuditLevel attributeLevel, object[] arguments)
        {
            if (attributeLevel != FSAuditLevel.Unknown)
            {
                context.AuditLevel = attributeLevel;
                return;
            }
            context.AuditLevel = ResolveFromArguments(arguments);
        }

        private FSAuditLevel ResolveFromArguments(object[] arguments)
        {
            if (arguments.Length == 0) return FSAuditLevel.ConnectionGroup;

            switch (arguments[0])
            {
                case RMFSTreeNode node:
                    return MapNodeLevel(node.Level);
                case ClassCodePolicyInfo p:
                    return ResolveFromPolicy(p);
                case FSDisposalByClassCodeRequest request:
                    return FSAuditLevel.ConnectionGroup;
                case ConnectionGroupDto:
                    return FSAuditLevel.ConnectionGroup;
                case ConnectionDto:
                    return FSAuditLevel.Connection;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is RMFSTreeNode treeNode)
                    return MapNodeLevel(treeNode.Level);
                if (arguments[i] is ScheduleInfo scheduleInfo)
                    return MapNodeLevel(JsonConvert.DeserializeObject<RMFSTreeNode>(scheduleInfo.Extentions).Level);
                if (arguments[i] is ClassCodePolicyInfo policy)
                    return ResolveFromPolicy(policy);
                if (arguments[i] is string textString && !string.IsNullOrWhiteSpace(textString))
                {
                    var deserializedFsDashboard = TryDeserialize<FileSystemMyhubSelectedNodeDto>(textString);
                    if (deserializedFsDashboard != null)
                    {
                        return MapNodeLevel(deserializedFsDashboard.Level);
                    }
                    if (textString.StartsWith("<ApplyClassCodeSettingDto"))
                    {
                        var levelValue = ExtractLevelFromXmlSafe(textString);

                        if (!string.IsNullOrEmpty(levelValue))
                        {
                            if (int.TryParse(levelValue, out int levelInt))
                            {
                                return MapNodeLevel(levelInt);
                            }
                        }
                    }

                    var deserializedNode = TryDeserialize<RMFSTreeNode>(textString);
                    if (deserializedNode != null)
                        return MapNodeLevel(deserializedNode.Level);
                }
            }

            return FSAuditLevel.ConnectionGroup;
        }

        private string ExtractLevelFromXmlSafe(string xmlString)
        {
            try
            {
                var doc = XDocument.Parse(xmlString);

                var levelNode = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "RMFSTreeNode")
                                   ?.Elements().FirstOrDefault(e => e.Name.LocalName == "Level");

                return levelNode?.Value;
            }
            catch
            {
                return null;
            }
        }

        private static FSAuditLevel MapNodeLevel(int level) => level switch
        {
            (int)NodeLevel.WebApplication => FSAuditLevel.ConnectionGroup,
            (int)NodeLevel.SiteCollection => FSAuditLevel.Connection,
            _ => FSAuditLevel.Folder
        };

        private FSAuditLevel ResolveFromPolicy(ClassCodePolicyInfo policy)
        {
            var nodeId = Guid.Parse(policy.CurrentNodeId);
            var groupId = Guid.Parse(policy.ConnGroupId);
            if (nodeId == groupId) return FSAuditLevel.ConnectionGroup;
            var conn = FSConnectionDao.GetConnectionById(nodeId);
            return conn != null ? FSAuditLevel.Connection : FSAuditLevel.Folder;
        }

        #endregion

        #region CreateFSGroup

        private static FSAuditContext AfterCreateGroup(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not ConnectionGroupDto dto) return context;
            context.ObjectName = dto.Name;
            context.ConnectionGroupId = dto.Id;
            context
                .AddModifiedContent("RM_FS_Register_GroupName", string.Empty, dto.Name)
                .AddModifiedContent("RM_FS_Register_Description", string.Empty, dto.Description ?? string.Empty)
                .AddModifiedContent("RM_FS_Register_SpecifyAgentAccessConn_Type", string.Empty, FormatAccessType(dto.AccessConnectionType));
            return context;
        }

        #endregion

        #region EditFSGroup

        private FSAuditContext BeforeEditGroup(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not ConnectionGroupDto dto) return context;
            var db = FSGroupDao.GetGroupById(dto.Id);
            if (db == null) return context;
            context.ObjectName = dto.Name ?? db.Name;
            context.ConnectionGroupId = dto.Id;
            context.SetMetadata(MetaOldGroupName, db.Name ?? string.Empty);
            context.SetMetadata(MetaOldDescription, db.Description ?? string.Empty);
            context.SetMetadata(MetaOldAccessType, FormatAccessType(db.AccessConnectionType));
            return context;
        }

        private static FSAuditContext AfterEditGroup(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not ConnectionGroupDto dto) return context;
            AddIfChanged(context, "RM_FS_Register_GroupName", GetMeta(context, MetaOldGroupName), dto.Name ?? string.Empty);
            AddIfChanged(context, "RM_FS_Register_Description", GetMeta(context, MetaOldDescription), dto.Description ?? string.Empty);
            AddIfChanged(context, "RM_FS_Register_SpecifyAgentAccessConn_Type", GetMeta(context, MetaOldAccessType), FormatAccessType(dto.AccessConnectionType));
            return context;
        }

        #endregion

        #region DeleteFSGroup

        private FSAuditContext BeforeDeleteGroup(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not List<Guid> ids) return context;
            var groups = FSGroupDao.GetGroupByIds(ids);
            context.SetMetadata(MetaDeletedNames, string.Join("; ", groups.Select(g => g.Name)));
            context.ObjectName = string.Join("; ", groups.Select(g => g.Name));
            return context;
        }

        #endregion

        #region CreateFSConnection

        private FSAuditContext AfterCreateConnection(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not ConnectionDto dto) return context;
            context.ObjectName = dto.Name;
            context.ConnectionId = dto.Id;
            context.ConnectionGroupId = dto.GroupId;
            context
                .AddModifiedContent("RM_FS_Register_ConnectionName", string.Empty, dto.Name ?? string.Empty)
                .AddModifiedContent("RM_FS_Register_Description", string.Empty, dto.Description ?? string.Empty)
                .AddModifiedContent("RM_FS_Register_JPMCId", string.Empty, dto.JPMCConnectionId ?? string.Empty);

            AppendNewConnectionOwners(context, dto);
            context.AddModifiedContent("RM_FS_Register_Path", string.Empty, dto.UNCPath ?? string.Empty);
            AppendNewConnectionGroup(context, dto);
            return context;
        }

        private static void AppendNewConnectionOwners(FSAuditContext context, ConnectionDto dto)
        {
            context.AddModifiedContent("RM_FS_Register_Information_Owner", string.Empty,
                dto.InformationOwners != null ? string.Join("; ", dto.InformationOwners.Select(x => x.DisplayName)) : string.Empty);
            context.AddModifiedContent("RM_FS_Register_Records_Owner", string.Empty,
                dto.RecordOwners != null ? string.Join("; ", dto.RecordOwners.Select(x => x.DisplayName)) : string.Empty);
        }

        private void AppendNewConnectionGroup(FSAuditContext context, ConnectionDto dto)
        {
            if (dto.GroupId == Guid.Empty) return;
            var group = FSGroupDao.GetGroupById(dto.GroupId);
            if (group != null)
                context.AddModifiedContent("RM_FS_Register_AddToConnectionGroup", string.Empty, group.Name);
        }

        #endregion

        #region EditFSConnection

        private async Task<FSAuditContext> BeforeEditConnectionAsync(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not ConnectionDto dto) return context;
            var db = FSConnectionDao.GetConnectionById(dto.Id);
            if (db == null) return context;
            context.ObjectName = dto.Name ?? db.Name;
            context.SetMetadata(MetaOldConnName, db.Name ?? string.Empty);
            context.SetMetadata(MetaOldDescription, db.Description ?? string.Empty);
            context.SetMetadata(MetaOldUNCPath, db.UNCPath ?? string.Empty);
            context.SetMetadata(MetaOldConnGroupName, ResolveGroupName(db.GroupId));
            await StoreOldConnectionOwnersAsync(context, dto.Id);
            return context;
        }

        private async Task StoreOldConnectionOwnersAsync(FSAuditContext context, Guid connectionId)
        {
            var rels = ConnectionOwnerDao.GetOwnersByConnectionId(connectionId);
            var userIds = rels.Select(r => r.UserIntId).Distinct().ToList();
            var owners = await AccountDao.GetUserByIdsAsync(userIds);
            var infoNames = rels.Where(r => r.Type == FSConnectionOwnerType.InformationOwner)
                .Join(owners, r => r.UserIntId, o => o.Id, (_, o) => o.DisplayName).OrderBy(n => n);
            var recNames = rels.Where(r => r.Type == FSConnectionOwnerType.RecordOwner)
                .Join(owners, r => r.UserIntId, o => o.Id, (_, o) => o.DisplayName).OrderBy(n => n);
            context.SetMetadata(MetaOldInfoOwners, string.Join("; ", infoNames));
            context.SetMetadata(MetaOldRecOwners, string.Join("; ", recNames));
        }

        private FSAuditContext AfterEditConnection(FSAuditContext context, object[] arguments, object returnValue)
        {
            if (returnValue is int resultCode && resultCode != 1)
            {
                context.ErrorMessage = "EditFSConnection returned a non-success code. Skipping audit record.";
                return context;
            }

            if (arguments.Length == 0 || arguments[0] is not ConnectionDto dto) return context;
            AddIfChanged(context, "RM_FS_Register_ConnectionName", GetMeta(context, MetaOldConnName), dto.Name ?? string.Empty);
            AddIfChanged(context, "RM_FS_Register_Description", GetMeta(context, MetaOldDescription), dto.Description ?? string.Empty);

            var newInfo = dto.InformationOwners != null
                ? string.Join("; ", dto.InformationOwners.Select(x => x.DisplayName).OrderBy(n => n))
                : string.Empty;
            var newRec = dto.RecordOwners != null
                ? string.Join("; ", dto.RecordOwners.Select(x => x.DisplayName).OrderBy(n => n))
                : string.Empty;

            AddIfChanged(context, "RM_FS_Register_Information_Owner", GetMeta(context, MetaOldInfoOwners), newInfo);
            AddIfChanged(context, "RM_FS_Register_Records_Owner", GetMeta(context, MetaOldRecOwners), newRec);
            AddIfChanged(context, "RM_FS_Register_Path", GetMeta(context, MetaOldUNCPath), dto.UNCPath ?? string.Empty);
            AddIfChanged(context, "RM_FS_Register_AddToConnectionGroup", GetMeta(context, MetaOldConnGroupName), ResolveGroupName(dto.GroupId));
            return context;
        }

        #endregion

        #region DeleteFSConnection

        private FSAuditContext BeforeDeleteConnection(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not List<Guid> ids) return context;
            var conns = FSConnectionDao.GetConnectionByIds(ids);
            context.ObjectName = string.Join("; ", conns.Select(c => c.Name));
            context.SetMetadata(MetaDeletedNames, string.Join("; ", conns.Select(c => c.Name)));
            return context;
        }

        #endregion

        #region FSConnectionCorrelateGroup

        private FSAuditContext BeforeCorrelateGroup(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not CorrelateConnectionDto dto) return context;
            var group = FSGroupDao.GetGroupById(dto.GroupId);
            context.ObjectName = group?.Name ?? string.Empty;
            var dbConns = FSConnectionDao.GetAllConnectionsByGroupId(dto.GroupId);
            context.SetMetadata(MetaOldCorrelatedConns, string.Join(";", dbConns.Select(c => c.Name)));
            return context;
        }

        private FSAuditContext AfterCorrelateGroup(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not CorrelateConnectionDto dto) return context;
            var dtoConns = FSConnectionDao.GetConnectionByIds(dto.ConnectionIdList);
            var newConns = string.Join(";", dtoConns.Select(c => c.Name));
            AddIfChanged(context, "RM_FS_Register_EditCorrelateConnections", GetMeta(context, MetaOldCorrelatedConns), newConns);
            return context;
        }

        #endregion

        #region FSConnectionValidationTest

        private static FSAuditContext AfterConnectionValidationTest(FSAuditContext context, object[] arguments, object returnValue)
        {
            if (arguments.Length == 0 || arguments[0] is not ConnectionDto dto) return context;
            context.ObjectName = dto.UNCPath;
            context.Status = returnValue is true ? AuditStatus.Successful : AuditStatus.Failed;
            return context;
        }

        #endregion

        #region ApplyClassCode

        private async Task<FSAuditContext> BeforeApplyClassCodeAsync(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not ClassCodePolicyInfo policy) return context;
            var nodeId = Guid.Parse(policy.CurrentNodeId);
            var groupId = Guid.Parse(policy.ConnGroupId);

            context.ObjectName = ResolveNodeNameFromPolicy(policy);

            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            //string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), generalSetting.DataFormatId), true)];

            if (context.AuditType == FSAuditType.MyhubClassify)
            {
                if (arguments[1] is not RMMyhubClassifyQueryInfo queryInfo)
                {
                    return context;
                }
                else
                {
                    if (queryInfo.TimeZoneId != null)
                    {
                        generalSetting.TimeZoneId = DateTimeUtil.AllTimeZones[Convert.ToInt32(queryInfo.TimeZoneId)];
                        generalSetting.DayLight = queryInfo.IsDaylight;
                    }
                }
            }

            if(policy.FSTreeNode.Level == (int)NodeLevel.FSFile || policy.FSTreeNode.Level == (int)NodeLevel.FSFolder)
            {
                var record = await MyhubAuditTrialMethod.QueryForBeforeApplyClassCodeAudit(policy.FSTreeNode.Id);
                context.SetMetadata(MetaOldClassCode, record?.ClassCode ?? string.Empty);
                context.SetMetadata(MetaOldCountryCode, record?.CountryCode ?? string.Empty);
                context.SetMetadata(MetaOldRetentionType, (record != null && record.ClassCode != null) ? record.RetentionType == "1" ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType" : string.Empty);
                //context.SetMetadata(MetaOldStartDate, record != null ? await FormatTicksAsync(record.StartDate) : string.Empty);
                if (record.RetentionType == "1")
                {
                    context.SetMetadata(MetaOldStartDate, GeneralSettingService.ConvertTiksToDateTime(generalSetting, record.StartDate, true).SimplifyFormatTime);
                }
                return context;
            }

            var setting = FileSystemSettingDao.LoadFSSetting(nodeId, groupId);
            if (setting == null)
            {
                if (policy.FSTreeNode != null && policy.FSTreeNode.Level != (int)NodeLevel.WebApplication)
                {
                    context.SetMetadata(MetaInheritSetting, "RM_JS_TM_inherBreak");
                }
                var parentId = Guid.Empty;
                setting = FileSystemSettingDao.LoadInheritSetting(nodeId, groupId, ref parentId);
            }

            context.SetMetadata(MetaOldClassCode, setting?.ClassCode ?? string.Empty);
            context.SetMetadata(MetaOldCountryCode, setting?.CountryCode ?? string.Empty);
            context.SetMetadata(MetaOldRetentionType, (setting != null && setting.ClassCode != null) ? setting.RetentionScheduleType == RetentionScheduleType.Event ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType" : string.Empty);
            if(policy.FSTreeNode.Level == (int)NodeLevel.SiteCollection)
            {
                if (setting.RetentionScheduleType == RetentionScheduleType.Event)
                {
                    context.SetMetadata(MetaOldStartDate, GeneralSettingService.ConvertTiksToDateTime(generalSetting, setting.StartDate, true).SimplifyFormatTime);
                }
            }
            if (policy.FSTreeNode.Level != (int)NodeLevel.FSFile && policy.FSTreeNode.Level != (int)NodeLevel.FSFolder)
            {
                var oldEffectScopeValue = string.Empty;
                if (setting != null && !string.IsNullOrEmpty(setting.ClassCode))
                {
                    oldEffectScopeValue = setting.ApplyExistDocument ? "RM_FS_ClassCodePolicy_ApplyAllNodes" : "RM_FS_ClassCodePolicy_ApplySelectedNode";
                }

                context.SetMetadata(MetaOldApplyToSub, oldEffectScopeValue);
            }
            return context;
        }

        private async Task<FSAuditContext> AfterApplyClassCodeAsync(FSAuditContext context, object[] arguments, object returnValue)
        {
            if (arguments != null && arguments.Length > 4 && arguments[4] is List<RMFSTreeNode>)
            {
                return AfterJobRun(context, arguments, returnValue);
            }
            if (arguments.Length == 0 || arguments[0] is not ClassCodePolicyInfo policy)
            {
                if (DeserializeXml<ApplyClassCodeSettingDto>(arguments[2]?.ToString()) is ApplyClassCodeSettingDto applyDto)
                {
                    return AfterJobRun(context, arguments, returnValue);
                }
                return context;
            }

            RAReturnMessage msg = (RAReturnMessage)returnValue;
            if (msg != null)
            {
                context.Status = msg.MessageType == RAMessageType.Successful ? AuditStatus.Successful : AuditStatus.Failed;
            }
            var retentionType = policy.RetentionScheduleType == RetentionScheduleType.Event ? RetentionScheduleType.Event : RetentionScheduleType.Flat;
            context
                .AddModifiedContent("RM_FS_ClassCodePolicy_ClassCode", GetMeta(context, MetaOldClassCode), policy.ClassCode ?? string.Empty)
                .AddModifiedContent("RM_FS_ClassCodePolicy_CountryCode", GetMeta(context, MetaOldCountryCode), policy.CountryCode ?? string.Empty)
                .AddModifiedContent("RM_FS_ClassCodePolicy_RetentionType", GetMeta(context, MetaOldRetentionType), retentionType == RetentionScheduleType.Event ? "RM_FS_ClassCodePolicy_RetentionEventType" : "RM_FS_ClassCodePolicy_RetentionFlatType");
            context.AddModifiedContent(string.Empty, GetMeta(context, MetaInheritSetting), string.Empty);
            var oldRetentionWasEvent = GetMeta(context, MetaOldRetentionType) == "RM_FS_ClassCodePolicy_RetentionEventType";
            var newRetentionIsEvent = retentionType == RetentionScheduleType.Event;

            if (newRetentionIsEvent || oldRetentionWasEvent)
            {
                string newStartDate = string.Empty;
                if (newRetentionIsEvent)
                {
                    var ticks = policy.StartDate.Ticks;
                    newStartDate = ticks != 0
                        ? (await GeneralSettingService.ConvertTiksToDateTimeAsync(ticks, true)).SimplifyFormatTime
                        : string.Empty;
                    if (context.AuditType == FSAuditType.MyhubClassify)
                    {
                        if (arguments[1] is not RMMyhubClassifyQueryInfo queryInfo) return context;
                        var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                        string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), generalSetting.DataFormatId), true)];
                        newStartDate = (string.IsNullOrEmpty(queryInfo.TimeZoneId)
                            ? GeneralSettingService.ConvertTiksToDateTime(generalSetting, ticks, true).SimplifyFormatTime
                            : GeneralSettingService.ConvertTiksToDateTime(generalSetting, ticks, true, Convert.ToInt32(queryInfo.TimeZoneId), queryInfo.IsDaylight, dateFormat).SimplifyFormatTime);
                    }
                }
                context.AddModifiedContent("RM_FS_ClassCodePolicy_StartDate", GetMeta(context, MetaOldStartDate), newStartDate);
            }
            if (policy.FSTreeNode.Level != (int)NodeLevel.FSFile)
            {
                var effectScopeValue = policy.ApplyExistDocument ? "RM_FS_ClassCodePolicy_ApplyAllNodes" : "RM_FS_ClassCodePolicy_ApplySelectedNode";
                context.AddModifiedContent("RM_FS_Export_EffectScopeColumn", GetMeta(context, MetaOldApplyToSub), effectScopeValue);
            }
            return context;
        }

        private string ResolveNodeName(Guid nodeId, Guid connGroupId)
        {
            var conn = FSConnectionDao.GetConnectionById(nodeId);
            if (conn != null) return conn.Name ?? string.Empty;
            var group = FSGroupDao.GetGroupById(nodeId);
            if (group != null) return group.Name ?? string.Empty;
            return nodeId.ToString();
        }

        private string ResolveNodeNameFromPolicy(ClassCodePolicyInfo policy)
        {
            var nodeId = Guid.Parse(policy.CurrentNodeId);
            var conn = FSConnectionDao.GetConnectionById(nodeId);
            if (conn != null) return conn.UNCPath ?? conn.Name ?? string.Empty;
            var group = FSGroupDao.GetGroupById(nodeId);
            if (group != null) return group.Name ?? string.Empty;
            return policy.FSTreeNode?.FullPath ?? nodeId.ToString();
        }
        #endregion

        #region PermissionChange

        private async Task<FSAuditContext> BeforePermissionChangeAsync(FSAuditContext context, object[] arguments)
        {
            //if (arguments.Length == 0 || arguments[0] is not ConnectionDto connection) return context;
            //context.ObjectName = connection.Name;
            //await StoreOldConnectionOwnersAsync(context, connection.Id);
            //return context;
            if (arguments.Length == 0)
            {
                return context;
            }

            if (arguments[0] is ConnectionDto connection)
            {
                context.ObjectName = connection.Name;
                await StoreOldConnectionOwnersAsync(context, connection.Id);
                return context;
            }

            if (arguments[0] is RMConnectionRecordOwnerUpdateModel model)
            {
                var dbConnection = FSConnectionDao.GetConnectionById(model.ConnectionId);
                if (dbConnection == null)
                {
                    return context;
                }

                context.ObjectName = dbConnection.Name ?? string.Empty;
                context.ConnectionId = dbConnection.Id;
                context.ConnectionGroupId = dbConnection.GroupId;
                context.CurrentPath = dbConnection.UNCPath ?? string.Empty;

                await StoreOldConnectionOwnersAsync(context, dbConnection.Id);
            }

            return context;
        }

        private static FSAuditContext AfterPermissionChange(FSAuditContext context, object[] arguments)
        {
            //if (arguments.Length == 0 || arguments[0] is not ConnectionDto connection) return context;
            //var newInfo = connection.InformationOwners != null ? string.Join("; ", connection.InformationOwners.Select(x => x.DisplayName).OrderBy(n => n)) : string.Empty;
            //var newRec = connection.RecordOwners != null ? string.Join("; ", connection.RecordOwners.Select(x => x.DisplayName).OrderBy(n => n)) : string.Empty;
            //context
            //    .AddModifiedContent("RM_FS_Register_Information_Owner", GetMeta(context, MetaOldInfoOwners), newInfo)
            //    .AddModifiedContent("RM_FS_Register_Records_Owner", GetMeta(context, MetaOldRecOwners), newRec);
            //return context;
            if (arguments.Length == 0)
            {
                return context;
            }
            if (arguments[0] is ConnectionDto connection)
            {
                var newInfo = connection.InformationOwners != null ? string.Join("; ", connection.InformationOwners.Select(x => x.DisplayName).OrderBy(n => n)) : string.Empty;
                var newRec = connection.RecordOwners != null ? string.Join("; ", connection.RecordOwners.Select(x => x.DisplayName).OrderBy(n => n)) : string.Empty;
                context
                    .AddModifiedContent("RM_FS_Register_Information_Owner", GetMeta(context, MetaOldInfoOwners), newInfo)
                    .AddModifiedContent("RM_FS_Register_Records_Owner", GetMeta(context, MetaOldRecOwners), newRec);
                return context;
            }
            if (arguments[0] is RMConnectionRecordOwnerUpdateModel model)
            {
                var newRec = model.RecordOwners != null ? string.Join("; ", model.RecordOwners.Select(x => x.DisplayName).OrderBy(name => name)) : string.Empty;
                AddIfChanged(context, "RM_FS_Register_Records_Owner", GetMeta(context, MetaOldRecOwners), newRec);
            }
            return context;
        }

        #endregion

        #region EditGeneralSetting

        private FSAuditContext BeforeEditGeneralSetting(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
            var db = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
            if (db == null)
            {
                if (node.Level != (int)NodeLevel.WebApplication)
                {
                    context.SetMetadata(MetaInheritSetting, "RM_JS_TM_inherBreak");
                }
                var parentId = Guid.Empty;
                db = FileSystemSettingDao.LoadInheritSetting(node.Id, node.ConnGroupId, ref parentId);
            }
            bool oldEnableIL = db != null && db.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Enable;
            bool oldEnableDownloadRCCReport = db != null && db.IsAllowUserDownloadRCCReport == true;
            context.SetMetadata(MetaOldEnableRM, db != null ? YesOrNo(oldEnableIL) : string.Empty);
            context.SetMetadata(MetaOldEnableDownloadRCCReport, db != null ? YesOrNo(oldEnableDownloadRCCReport) : string.Empty);
            return context;
        }

        private FSAuditContext AfterEditGeneralSetting(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            bool newEnableIL = node.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Enable;
            bool newEnableDownloadRCCReport = node.IsAllowUserDownloadRCCReport == true;
            context.AddModifiedContent("RM_SPS_GS_ManagedScope", GetMeta(context, MetaOldEnableRM), YesOrNo(newEnableIL));
            context.AddModifiedContent("RM_JS_FS_DownloadRCCReport", GetMeta(context, MetaOldEnableDownloadRCCReport), YesOrNo(newEnableDownloadRCCReport));
            context.AddModifiedContent(string.Empty, GetMeta(context, MetaInheritSetting), string.Empty);
            return context;
        }

        #endregion

        #region EditDocLevelSetting

        private FSAuditContext BeforeEditDocLevelSetting(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
            var db = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
            if (db == null)
            {
                if (node.Level != (int)NodeLevel.WebApplication)
                {
                    context.SetMetadata(MetaInheritSetting, "RM_JS_TM_inherBreak");
                }
                var parentId = Guid.Empty;
                db = FileSystemSettingDao.LoadInheritSetting(node.Id, node.ConnGroupId, ref parentId);
            }
            context.SetMetadata(MetaOldSubsetTerm, db != null ? ResolveTermScopePath(db.TermId, db.TermSetId) : string.Empty);
            return context;
        }

        private FSAuditContext AfterEditDocLevelSetting(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            var newSubset = ResolveTermScopePath(node.TermId, node.TermSetId);
            AddIfChanged(context, "RM_JS_FS_EditKey_ClassCodeScope", GetMeta(context, MetaOldSubsetTerm), newSubset);
            context.AddModifiedContent(string.Empty, GetMeta(context, MetaInheritSetting), string.Empty);
            return context;
        }

        #endregion

        #region EditLocationOwnersSetting

        private async Task<FSAuditContext> BeforeEditLocationOwnersAsync(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
            var db = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
            if (db == null)
            {
                var parentId = Guid.Empty;
                db = FileSystemSettingDao.LoadInheritSetting(node.Id, node.ConnGroupId, ref parentId);
            }

            if (db != null)
            {
                context.SetMetadata(MetaOldApproval, YesOrNo(db.ApprovalType != ApprovalType.None));

                if (db.ApprovalType != ApprovalType.None)
                {
                    if (db.ApprovalType == ApprovalType.ApprovalProcess && !string.IsNullOrEmpty(db.WorkflowReferenceId))
                    {
                        var wf = ManualProcessService.GetWorkflow(new Guid(db.WorkflowReferenceId));
                        context.SetMetadata(MetaOldProcess, wf?.Name ?? string.Empty);
                    }
                    var ownerIds = RecordOwnerDao.GetRecordOwner(db.Id, RecordOwnerSettingType.FileSystem).Select(a => a.ObjectId).ToList();
                    var ownerAccounts = await AccountDao.FindListAsync(o => ownerIds.Contains(o.UserId));
                    context.SetMetadata(MetaOldLocOwners, string.Join(";", ownerAccounts.Select(a => a.DisplayName)));
                    context.SetMetadata(MetaOldEmail, YesOrNo(db.EMailToRecordOwner));
                }
            }
            else
            {
                context.SetMetadata(MetaOldApproval, string.Empty);
                context.SetMetadata(MetaOldProcess, string.Empty);
                context.SetMetadata(MetaOldLocOwners, string.Empty);
                context.SetMetadata(MetaOldEmail, string.Empty);
            }

            return context;
        }

        private FSAuditContext AfterEditLocationOwners(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;

            var newApproval = YesOrNo(node.ApprovalType != (int)ApprovalType.None);
            context.AddModifiedContent("RM_BCM_ManualApproval_Title_EnableApproval", GetMeta(context, MetaOldApproval), newApproval);

            string newProcess = string.Empty;
            if (node.ApprovalType == (int)ApprovalType.ApprovalProcess && !string.IsNullOrEmpty(node.WorkflowReferenceId))
            {
                var workflow = ManualProcessService.GetWorkflow(new Guid(node.WorkflowReferenceId));
                newProcess = workflow?.Name ?? string.Empty;
            }
            context.AddModifiedContent("RM_JS_RDM_ManualApproval_ProcessName", GetMeta(context, MetaOldProcess), newProcess);

            string newOwners = string.Empty;
            if (node.ApprovalType != (int)ApprovalType.None && node.RecordOwner != null)
                newOwners = string.Join(";", node.RecordOwner.Select(a => a.DisplayName));
            context.AddModifiedContent("RM_SPS_RecordOwners", GetMeta(context, MetaOldLocOwners), newOwners);

            string newEmail = node.ApprovalType != (int)ApprovalType.None ? YesOrNo(node.EMailToRecordOwner) : string.Empty;
            context.AddModifiedContent("RM_JS_SPS_EditKey_EmailNotifiation", GetMeta(context, MetaOldEmail), newEmail);

            return context;
        }

        #endregion

        #region ConfigureDisposalJobSchedule4FS

        private static bool IsFSScheduleType(ScheduleType jobCategory)
        {
            return jobCategory is ScheduleType.FSDisposalSchedule or ScheduleType.FSColletionDataSchedule;
        }

        private static bool IsFSSettingScheduleType(SettingScheduleType type)
        {
            return type == SettingScheduleType.Dispose;
        }

        private async Task<FSAuditContext> BeforeConfigureDisposalScheduleAsync(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0) return context;

            if (arguments[0] is SettingScheduleType settingType)
            {
                if (!IsFSSettingScheduleType(settingType))
                {
                    context.ErrorMessage = "Not a FileSystem schedule change. Skipping FS audit.";
                    return context;
                }

                if (arguments.Length > 1 && arguments[1] is string nodeIdStr && !string.IsNullOrEmpty(nodeIdStr)
                    && Guid.TryParse(nodeIdStr, out var nodeId) && nodeId != Guid.Empty)
                {
                    ResolveScheduleNodeHierarchy(context, nodeId);
                    var candidates = BuildFSScheduleProfileIdCandidates(nodeId);
                    await LoadOldScheduleValuesAsync(context, candidates);
                }
                return context;
            }

            if (arguments[0] is string firstArg && arguments[0] is not ScheduleInfo)
            {
                await StoreOldScheduleByIdAsync(context, firstArg);

                if (context.ConnectionGroupId == Guid.Empty
                    && arguments.Length > 1 && arguments[1] is string nodeIdentifier
                    && !string.IsNullOrEmpty(nodeIdentifier))
                {
                    if (Guid.TryParse(nodeIdentifier, out var nodeId) && nodeId != Guid.Empty)
                    {
                        ResolveScheduleNodeHierarchy(context, nodeId);
                        if (context.ConnectionGroupId == Guid.Empty)
                        {
                            var candidates = BuildFSScheduleProfileIdCandidates(nodeId);
                            await LoadOldScheduleValuesAsync(context, candidates);
                        }
                    }
                    else
                    {
                        context.CurrentPath = nodeIdentifier;
                        ResolveHierarchyFromPath(context, nodeIdentifier);
                    }
                }

                if (string.IsNullOrEmpty(context.ObjectName)
                    && arguments.Length > 1 && arguments[1] is string nodeLabel
                    && !string.IsNullOrEmpty(nodeLabel))
                {
                    context.ObjectName = nodeLabel;
                }
                return context;
            }

            if (arguments[0] is ScheduleInfo scheduleInfo)
            {
                if (!IsFSScheduleType(scheduleInfo.JobCategory))
                {
                    context.ErrorMessage = "Not a FileSystem schedule change. Skipping FS audit.";
                    return context;
                }

                var scheduleNode = ExtractNodeFromScheduleExtentions(scheduleInfo);
                if (scheduleNode != null)
                    ApplyNodeHierarchyToContext(context, scheduleNode);
                else if (arguments.Length > 1 && arguments[1] is string fullPath && !string.IsNullOrEmpty(fullPath))
                {
                    context.ObjectName = fullPath;
                    context.CurrentPath = fullPath;
                    ResolveHierarchyFromPath(context, fullPath);
                }
                await StoreOldScheduleByProfileAsync(context, scheduleInfo.ProfileId, scheduleInfo.JobCategory);
            }

            return context;
        }

        private async Task StoreOldScheduleByIdAsync(FSAuditContext context, string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId)) return;
            try
            {
                var old = await ScheduleService.GetScheduleByIdAsync(scheduleId);
                if (old == null || old.NoSchedule)
                {
                    context.SetMetadata(MetaOldScheduleIsNoSchedule, "true");
                    return;
                }

                if (context.ConnectionGroupId == Guid.Empty)
                {
                    var scheduleNode = ExtractNodeFromScheduleExtentions(old);
                    if (scheduleNode != null)
                        ApplyNodeHierarchyToContext(context, scheduleNode);
                }

                context.SetMetadata(MetaOldScheduleIsNoSchedule, "false");
                context.SetMetadata(MetaOldScheduleStartTime, await FormatScheduleStartTimeAsync(old));
                context.SetMetadata(MetaOldScheduleEndTime, await FormatScheduleEndTimeAsync(old));
                context.SetMetadata(MetaOldScheduleInterval, FormatScheduleInterval(old));
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to load old schedule by id {0}: {1}", scheduleId, ex.Message);
            }
        }

        private async Task StoreOldScheduleByProfileAsync(FSAuditContext context, string profileId, ScheduleType scheduleType)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            try
            {
                var old = await ScheduleService.GetScheduleAsync(profileId, scheduleType);
                if (old == null || old.NoSchedule)
                {
                    context.SetMetadata(MetaOldScheduleIsNoSchedule, "true");
                    return;
                }
                context.SetMetadata(MetaOldScheduleIsNoSchedule, "false");
                context.SetMetadata(MetaOldScheduleStartTime, await FormatScheduleStartTimeAsync(old));
                context.SetMetadata(MetaOldScheduleEndTime, await FormatScheduleEndTimeAsync(old));
                context.SetMetadata(MetaOldScheduleInterval, FormatScheduleInterval(old));
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to load old schedule by profile {0}: {1}", profileId, ex.Message);
            }
        }

        private async Task LoadOldScheduleValuesAsync(FSAuditContext context, List<string> profileIdCandidates)
        {
            foreach (var profileId in profileIdCandidates)
            {
                try
                {
                    var existing = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.FSDisposalSchedule);
                    if (existing == null)
                        continue;

                    if (existing.NoSchedule)
                    {
                        context.SetMetadata(MetaOldScheduleIsNoSchedule, "true");
                        return;
                    }

                    if (context.ConnectionGroupId == Guid.Empty)
                    {
                        var scheduleNode = ExtractNodeFromScheduleExtentions(existing);
                        if (scheduleNode != null && scheduleNode.ConnGroupId != Guid.Empty)
                        {
                            context.ConnectionGroupId = scheduleNode.ConnGroupId;
                            if (scheduleNode.Level == (int)NodeLevel.SiteCollection)
                                context.ConnectionId = scheduleNode.Id;
                        }
                    }

                    context.SetMetadata(MetaOldScheduleIsNoSchedule, "false");
                    context.SetMetadata(MetaOldScheduleStartTime, await FormatScheduleStartTimeAsync(existing));
                    context.SetMetadata(MetaOldScheduleEndTime, await FormatScheduleEndTimeAsync(existing));
                    context.SetMetadata(MetaOldScheduleInterval, FormatScheduleInterval(existing));
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Failed to load existing FS disposal schedule for audit (profileId={0}): {1}", profileId, ex.Message);
                }
            }

            context.SetMetadata(MetaOldScheduleIsNoSchedule, "true");
        }

        private List<string> BuildFSScheduleProfileIdCandidates(Guid nodeId)
        {
            var candidates = new List<string>();

            var conn = FSConnectionDao.GetConnectionById(nodeId);
            if (conn != null)
            {
                if (conn.GroupId != Guid.Empty)
                    candidates.Add($"{conn.GroupId}|{conn.Id}");
                candidates.Add(conn.Id.ToString());
                return candidates;
            }

            var group = FSGroupDao.GetGroupById(nodeId);
            if (group != null)
            {
                candidates.Add(group.Id.ToString());
                return candidates;
            }

            candidates.Add(nodeId.ToString());
            return candidates;
        }

        private void ResolveScheduleNodeHierarchy(FSAuditContext context, Guid nodeId)
        {
            var conn = FSConnectionDao.GetConnectionById(nodeId);
            if (conn != null)
            {
                context.ObjectName = conn.UNCPath ?? conn.Name ?? nodeId.ToString();
                context.ConnectionId = conn.Id;
                context.ConnectionGroupId = conn.GroupId;
                context.CurrentPath = conn.UNCPath ?? string.Empty;
                return;
            }

            var group = FSGroupDao.GetGroupById(nodeId);
            if (group != null)
            {
                context.ObjectName = group.Name ?? nodeId.ToString();
                context.ConnectionGroupId = group.Id;
                return;
            }

            context.ObjectName = nodeId.ToString();
            context.ItemId = nodeId;
        }

        private async Task<FSAuditContext> AfterConfigureDisposalScheduleAsync(FSAuditContext context, object[] arguments, object returnValue)
        {
            if (returnValue is string returnStr && returnStr == "-1")
            {
                context.ErrorMessage = "Schedule operation returned failure code. Skipping audit record.";
                return context;
            }

            if (arguments.Length == 0) return context;

            var noScheduleLabel = "RM_JS_ScheduleSetting_NoSchedule";

            if (arguments[0] is SettingScheduleType || (arguments[0] is string && arguments[0] is not ScheduleInfo))
            {
                bool hadSchedule = GetMeta(context, MetaOldScheduleIsNoSchedule) == "false";
                if (hadSchedule)
                {
                    context.AddModifiedContent("RM_JS_ScheduleSetting_StratTime", GetMeta(context, MetaOldScheduleStartTime), string.Empty);
                    context.AddModifiedContent("RM_JS_ScheduleSetting_EndTime", GetMeta(context, MetaOldScheduleEndTime), string.Empty);
                    context.AddModifiedContent("RM_TS_IntervalTime", GetMeta(context, MetaOldScheduleInterval), string.Empty);
                    context.AddModifiedContent(string.Empty, string.Empty, noScheduleLabel);
                }
                else
                {
                    context.AddModifiedContent(string.Empty, noScheduleLabel, noScheduleLabel);
                }
                return context;
            }

            ScheduleInfo newSchedule = arguments[0] as ScheduleInfo;
            if (newSchedule == null) return context;

            if (newSchedule.NoSchedule)
            {
                bool hadSchedule = GetMeta(context, MetaOldScheduleIsNoSchedule) == "false";
                if (hadSchedule)
                {
                    context.AddModifiedContent("RM_JS_ScheduleSetting_StratTime", GetMeta(context, MetaOldScheduleStartTime), string.Empty);
                    context.AddModifiedContent("RM_JS_ScheduleSetting_EndTime", GetMeta(context, MetaOldScheduleEndTime), string.Empty);
                    context.AddModifiedContent("RM_TS_IntervalTime", GetMeta(context, MetaOldScheduleInterval), string.Empty);
                    context.AddModifiedContent(string.Empty, string.Empty, noScheduleLabel);
                }
                else
                {
                    context.AddModifiedContent(string.Empty, noScheduleLabel, noScheduleLabel);
                }
                return context;
            }

            bool wasNoSchedule = GetMeta(context, MetaOldScheduleIsNoSchedule) == "true";
            var newStartTime = await FormatScheduleStartTimeAsync(newSchedule, startTimeIsUtc: true);
            var newEndTime = await FormatScheduleEndTimeAsync(newSchedule, endTimeIsUtc: true);
            var newInterval = FormatScheduleInterval(newSchedule);

            if (wasNoSchedule)
            {
                context.AddModifiedContent(string.Empty, noScheduleLabel, string.Empty);
                context.AddModifiedContent("RM_JS_ScheduleSetting_StratTime", string.Empty, newStartTime);
                context.AddModifiedContent("RM_JS_ScheduleSetting_EndTime", string.Empty, newEndTime);
                context.AddModifiedContent("RM_TS_IntervalTime", string.Empty, newInterval);
            }
            else
            {
                context.AddModifiedContent("RM_JS_ScheduleSetting_StratTime", GetMeta(context, MetaOldScheduleStartTime), newStartTime);
                context.AddModifiedContent("RM_JS_ScheduleSetting_EndTime", GetMeta(context, MetaOldScheduleEndTime), newEndTime);
                context.AddModifiedContent("RM_TS_IntervalTime", GetMeta(context, MetaOldScheduleInterval), newInterval);
            }

            return context;
        }

        private async Task<string> FormatScheduleStartTimeAsync(ScheduleInfo schedule, bool startTimeIsUtc = false)
        {
            if (string.IsNullOrWhiteSpace(schedule.StartTime)) return string.Empty;
            try
            {
                var ticks = ConvertScheduleTimeToUtcTicks(schedule.StartTime, schedule.TimeZoneId, startTimeIsUtc);
                if (ticks <= 0) return string.Empty;
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                return GeneralSettingService.ConvertTiksToDateTime(gls, ticks, true).SimplifyFormatTime;
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to format schedule start time '{0}': {1}", schedule.StartTime, ex.Message);
                return schedule.StartTime;
            }
        }

        private async Task<string> FormatScheduleEndTimeAsync(ScheduleInfo schedule, bool endTimeIsUtc = false)
        {
            switch (schedule.EndType)
            {
                case EndType.EndByOccurrences:
                    return $"RM_JS_ScheduleSetting_EndAfter {schedule.OccurrencesTotal} RM_JS_ScheduleSetting_Occurrences".Trim();

                case EndType.EndByTime:
                    if (string.IsNullOrWhiteSpace(schedule.EndTime)) return string.Empty;
                    try
                    {
                        var ticks = ConvertScheduleTimeToUtcTicks(schedule.EndTime, schedule.TimeZoneId, endTimeIsUtc);
                        if (ticks <= 0) return string.Empty;
                        var gls = await GeneralSettingService.GetGeneralSettingAsync();
                        return GeneralSettingService.ConvertTiksToDateTime(gls, ticks, true).SimplifyFormatTime;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Failed to format schedule end time '{0}': {1}", schedule.EndTime, ex.Message);
                        return schedule.EndTime;
                    }

                default:
                    return "RM_JS_ScheduleSetting_NoEndDate";
            }
        }

        private static long ConvertScheduleTimeToUtcTicks(string timeStr, string timeZoneId, bool isUtc)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return 0;
            var cleanTimeStr = StripTimeZoneSuffix(timeStr, out _);
            var parsed = DateTime.Parse(cleanTimeStr);
            if (isUtc)
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc).Ticks;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified), tz).Ticks;
        }

        private static string StripTimeZoneSuffix(string timeStr, out string suffix)
        {
            var idx = timeStr.IndexOf(" (UTC", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                suffix = timeStr.Substring(idx).Trim();
                return timeStr.Substring(0, idx).Trim();
            }

            suffix = string.Empty;
            return timeStr.Trim();
        }

        private static string FormatScheduleInterval(ScheduleInfo schedule)
        {
            return schedule.IntervalType switch
            {
                IntervalType.Hourly => $"{schedule.Interval} RM_JS_ScheduleSetting_Hours",
                IntervalType.Daily => $"{schedule.Interval} RM_JS_ScheduleSetting_Days",
                IntervalType.Weekly => $"{schedule.Interval} RM_JS_ScheduleSetting_Weeks",
                IntervalType.Monthly => $"{schedule.Interval} RM_JS_ScheduleSetting_Months",
                _ => string.Empty
            };
        }

        private void ApplyNodeHierarchyToContext(FSAuditContext context, RMFSTreeNode node)
        {
            context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
            context.ConnectionGroupId = node.ConnGroupId;
            context.CurrentPath = node.FullPath ?? string.Empty;

            if (node.Level == (int)NodeLevel.SiteCollection)
            {
                context.ConnectionId = node.Id;
            }
            else if (node.Level > (int)NodeLevel.SiteCollection)
            {
                context.ItemId = node.Id;
                if (Guid.TryParse(node.ConnectionId, out var connId) && connId != Guid.Empty)
                    context.ConnectionId = connId;
                else if (!string.IsNullOrEmpty(node.FullPath))
                {
                    var parentConn = FSConnectionDao.GetParentConnectionInfo(node.FullPath);
                    if (parentConn != null)
                        context.ConnectionId = parentConn.Id;
                }
            }
        }

        private static RMFSTreeNode ExtractNodeFromScheduleExtentions(object argument)
        {
            if (argument is not ScheduleInfo schedule || string.IsNullOrEmpty(schedule.Extentions))
                return null;
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<RMFSTreeNode>(schedule.Extentions);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to deserialize RMFSTreeNode from ScheduleInfo.Extentions: {0}", ex.Message);
                return null;
            }
        }

        private void ResolveHierarchyFromPath(FSAuditContext context, string fullPath)
        {
            var parentConn = FSConnectionDao.GetParentConnectionInfo(fullPath);
            if (parentConn == null) return;
            context.ConnectionId = parentConn.Id;
            var connDetail = FSConnectionDao.GetConnectionById(parentConn.Id);
            if (connDetail != null)
                context.ConnectionGroupId = connDetail.GroupId;
        }

        #endregion

        #region EditInheritSetting

        private static FSAuditContext BeforeEditInheritSetting(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
            return context;
        }

        #endregion

        #region Active folder

        private FSAuditContext BeforeActiveSetting(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || arguments[0] is not RMFSTreeNode node) return context;
            context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
            context.AuditType = node.IsActive ? FSAuditType.FSActiveSetting : FSAuditType.FSDeactiveSetting;
            return context;
        }

        #endregion

        #region Download RCC report

        private FSAuditContext AfterDownloadRCCReportJob(FSAuditContext context, object[] arguments, object returnValue)
        {
            if (arguments == null || arguments.Length < 3 || !(arguments[2] is string requestJson) || string.IsNullOrEmpty(requestJson))
            {
                return context;
            }
            var jobId = returnValue as string;
            if (!string.IsNullOrEmpty(jobId))
            {
                context.ObjectName = jobId;
            }
            var rccRequest = JsonConvert.DeserializeObject<RCCReportRequest>(requestJson);
            //if (rccRequest != null)
            //{
            //    if (rccRequest.Node != null)
            //    {
            //        switch (rccRequest.Node.Level)
            //        {
            //            case (int)NodeLevel.WebApplication:
            //                context.ConnectionGroupId = rccRequest.Node.ConnGroupId;
            //                context.CurrentPath = rccRequest.Node.FullPath;
            //                break;
            //            case (int)NodeLevel.SiteCollection:
            //                context.ConnectionId = rccRequest.Node.Id;
            //                context.CurrentPath = rccRequest.Node.FullPath;
            //                break;
            //            default:
            //                context.ItemId = rccRequest.Node.Id;
            //                context.CurrentPath = rccRequest.Node.FullPath;
            //                break;
            //        }
            //    }
            //}

            return context;
        }

        #endregion

        #region Pause/Resume Action in Myhub
        //private FSAuditContext BeforePauseSetting(FSAuditContext context, object[] arguments)
        //{
        //    if (arguments.Length == 0 || arguments[0] is not PauseOrResumeReq req) return context;
        //    context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
        //    context.AuditType = req.IsPause==1 ? FSAuditType.FSPauseDisposalForMyhub : FSAuditType.FSResumeDisposalForMyhub;
        //    return context;
        //}
        #endregion

        #region Disposal History

        private FSAuditContext BeforeGenerateDisposalHistory(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0 || JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(arguments[0]?.ToString()) is not ManualApprovalHistoryOption historyInfo)
            {
                return context;
            }
            context.ObjectName = historyInfo.DisplayName;
            context.ConnectionGroupId = FSConnectionDao.GetConnectionGroupIdByConnectionIdAsync(new Guid(historyInfo.Id)).GetAwaiter().GetResult();
            context.ConnectionId = new Guid(historyInfo.Id);
            context.CurrentPath = historyInfo.FullPath;
            context.AuditLevel = FSAuditLevel.Connection;

            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();

            var timeRange = HandleDisposalHistoryTimeRange(historyInfo?.CustomDate, historyInfo?.LatestExportType ?? 0, gls);
            if (!string.IsNullOrEmpty(timeRange))
            {
                context.AddModifiedContent("RM_FS_DateRangeCustom_Title", string.Empty, timeRange);
            }
            return context;
        }

        private string HandleDisposalHistoryTimeRange(ManualHistoryCustomDataTime timeRange, int type, GeneralSettingModel gls)
        {
            return type switch
            {
                1 => I18NEntity.GetString("RM_MA_EntendDisposalTime_3M"),
                2 => I18NEntity.GetString("RM_MA_EntendDisposalTime_6M"),
                3 => I18NEntity.GetString("RM_MA_EntendDisposalTime_1Y"),
                4 => BuildDisposalHistoryDateString(timeRange, gls),
                5 => I18NEntity.GetString("RM_MA_EntendDisposalTime_All"),
                _ => string.Empty
            };
        }

        private string BuildDisposalHistoryDateString(ManualHistoryCustomDataTime timeRange, GeneralSettingModel gls)
        {
            string fromLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_From");
            string toLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_To");
            var startStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.StartDateTimeTicks, true).SimplifyFormatTime;
            var endStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.EndDateTimeTicks, true).SimplifyFormatTime;
            return $"{fromLabel} {startStr} {toLabel} {endStr}";
        }

        #endregion

        #region Job-run types

        private FSAuditContext AfterJobRun(FSAuditContext context, object[] arguments, object returnValue)
        {
            ResolveJobRunHierarchyIds(context, arguments);

            var jobId = returnValue as string;
            if (!string.IsNullOrEmpty(jobId))
            {
                context.ObjectName = jobId;
            }
            return context;
        }

        private void ResolveJobRunHierarchyIds(FSAuditContext context, object[] arguments)
        {
            if (arguments.Length == 0) return;
            if (context.ConnectionGroupId != Guid.Empty) return;

            switch (arguments[0])
            {
                case RMFSTreeNode node:
                    ApplyNodeHierarchyToContext(context, node);
                    return;
                case FSDisposalByClassCodeRequest request:
                    ApplyFSDisposalByClassCodeRequestHierarchy(context, request);
                    return;
                case ClassCodePolicyInfo policy:
                    ApplyClassCodePolicyHierarchy(context, policy);
                    return;
            }

            for (int i = 1; i < arguments.Length; i++)
            {
                if (arguments[i] is RMFSTreeNode treeNode)
                {
                    ApplyNodeHierarchyToContext(context, treeNode);
                    return;
                }
                if (arguments[i] is FSDisposalByClassCodeRequest req)
                {
                    ApplyFSDisposalByClassCodeRequestHierarchy(context, req);
                    return;
                }
                if (arguments[i] is string json && !string.IsNullOrWhiteSpace(json))
                {
                    var deserializedRequest = TryDeserializeClassCodeRequest(json);
                    if (deserializedRequest != null)
                    {
                        ApplyFSDisposalByClassCodeRequestHierarchy(context, deserializedRequest);
                        return;
                    }
                    var deserializedNode = TryDeserializeTreeNode(json);
                    if (deserializedNode != null)
                    {
                        ApplyNodeHierarchyToContext(context, deserializedNode);
                        return;
                    }

                    var deserializedFsDashboard = TryDeserialize<FileSystemMyhubSelectedNodeDto>(json);
                    if (deserializedFsDashboard != null)
                    {
                        ApplyFSDashboardRequestHierarchy(context, deserializedFsDashboard);
                        return;
                    }
                }
            }
        }
        private static FSDisposalByClassCodeRequest TryDeserializeClassCodeRequest(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var request = SerializerHelper.DeserializeByDataContractSerializer<FSDisposalByClassCodeRequest>(json);
                return request?.ConnectionGroupID != Guid.Empty ? request : null;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Helpers

        private async Task<string> FormatTicksAsync(long ticks)
        {
            if (ticks <= 0) return string.Empty;
            try
            {
                var timeModel = await GeneralSettingService.ConvertTiksToDateTimeAsync(ticks, true);
                return timeModel.SimplifyFormatTime;
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to format ticks {0}: {1}", ticks, ex.Message);
                return new DateTime(ticks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private static string GetMeta(FSAuditContext context, string key)
        {
            return context.Metadata.TryGetValue(key, out var val) ? val?.ToString() ?? string.Empty : string.Empty;
        }

        private static void AddIfChanged(FSAuditContext context, string targetSetting, string oldValue, string newValue)
        {
            if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                context.AddModifiedContent(targetSetting, oldValue, newValue);
        }

        private static string YesOrNo(bool value) => value ? "RM_JS_Common_Yes" : "RM_JS_Common_No";

        private static string FormatAccessType(AccessConnectionType type) =>
            type == AccessConnectionType.All
                ? "RM_FS_Register_SpecifyAgentAccessConn_Type_All"
                : "RM_FS_Register_SpecifyAgentAccessConn_Type_Specify";

        private string ResolveGroupName(Guid groupId)
        {
            if (groupId == Guid.Empty) return string.Empty;
            var group = FSGroupDao.GetGroupById(groupId);
            return group?.Name ?? string.Empty;
        }

        private string ResolveTermScopePath(Guid termId, Guid termSetId)
        {
            if (termId != Guid.Empty) return TermDao.GetTermNamesPathByTermId(termId);
            if (termSetId != Guid.Empty) return TermDao.GetTermSetNamesPathByTermSetId(termSetId);
            return string.Empty;
        }

        private void ResolveObjectName(FSAuditContext context, FSAuditType auditType, object[] arguments)
        {
            if (arguments.Length == 0) return;

            context.ObjectName = arguments[0] switch
            {
                ConnectionGroupDto group => group.Name ?? string.Empty,
                ConnectionDto conn => conn.Name ?? string.Empty,
                ClassCodePolicyInfo policy => ResolveNodeNameFromPolicy(policy),
                RMFSTreeNode node => node.FullPath ?? node.Name ?? node.Id.ToString(),
                FSDisposalByClassCodeRequest request => request.ConnectionGroupID.ToString(),
                List<Guid> ids => ResolveDeletedNames(auditType, ids),
                string sourcePath when auditType == FSAuditType.MoveFile => System.IO.Path.GetFileName(sourcePath),
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(context.ObjectName)) return;

            for (int i = 1; i < arguments.Length; i++)
            {
                if (arguments[i] is RMFSTreeNode node)
                {
                    context.ObjectName = node.FullPath ?? node.Name ?? node.Id.ToString();
                    return;
                }
                if (arguments[i] is string json && !string.IsNullOrWhiteSpace(json))
                {
                    var deserializedNode = TryDeserializeTreeNode(json);
                    if (deserializedNode != null)
                    {
                        context.ObjectName = deserializedNode.FullPath ?? deserializedNode.Name ?? deserializedNode.Id.ToString();
                        return;
                    }
                }
            }
        }

        private string ResolveDeletedNames(FSAuditType auditType, List<Guid> ids)
        {
            //if (auditType == FSAuditType.DeleteFSGroup)
            //{
            //    var groups = FSGroupDao.GetGroupByIds(ids);
            //    return string.Join("; ", groups.Select(g => g.Name));
            //}
            //if (auditType == FSAuditType.DeleteFSConnection)
            //{
            //    var conns = FSConnectionDao.GetConnectionByIds(ids);
            //    return string.Join("; ", conns.Select(c => c.Name));
            //}
            return string.Empty;
        }
        private static T TryDeserialize<T>(string json, Func<T, bool> condition = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var obj = SerializerHelper.DeserializeByDataContractSerializer<T>(json);
                return (condition == null || condition(obj)) ? obj : null;
            }
            catch
            {
                return null;
            }
        }
        private void ResolveHierarchyIds(FSAuditContext context, FSAuditType auditType, object[] arguments)
        {
            if (arguments.Length == 0) return;

            switch (arguments[0])
            {
                case ConnectionGroupDto group:
                    context.ConnectionGroupId = group.Id;
                    return;

                case ConnectionDto conn:
                    context.ConnectionId = conn.Id;
                    context.ConnectionGroupId = conn.GroupId;
                    context.CurrentPath = conn.UNCPath ?? string.Empty;
                    return;

                case RMFSTreeNode node:
                    ApplyRMFSTreeNodeHierarchy(context, node);
                    return;

                case ClassCodePolicyInfo policy:
                    ApplyClassCodePolicyHierarchy(context, policy);
                    return;

                case FSDisposalByClassCodeRequest request:
                    ApplyFSDisposalByClassCodeRequestHierarchy(context, request);
                    return;

                    //case List<Guid> ids when ids.Count > 0:
                    //    if (auditType == FSAuditType.DeleteFSGroup)
                    //    {
                    //        context.ConnectionGroupId = ids[0];
                    //    }
                    //    else if (auditType == FSAuditType.DeleteFSConnection)
                    //    {
                    //        context.ConnectionId = ids[0];
                    //        var conn = FSConnectionDao.GetConnectionById(ids[0]);
                    //        if (conn != null)
                    //        {
                    //            context.ConnectionGroupId = conn.GroupId;
                    //            context.CurrentPath = conn.UNCPath ?? string.Empty;
                    //        }
                    //    }
                    //    return;
            }

            for (int i = 1; i < arguments.Length; i++)
            {
                if (arguments[i] is RMFSTreeNode treeNode)
                {
                    ApplyRMFSTreeNodeHierarchy(context, treeNode);
                    return;
                }
                if (arguments[i] is FSDisposalByClassCodeRequest req)
                {
                    ApplyFSDisposalByClassCodeRequestHierarchy(context, req);
                    return;
                }
                if (arguments[i] is string json && !string.IsNullOrWhiteSpace(json))
                {
                    var deserializedRequest = TryDeserializeClassCodeRequest(json);
                    if (deserializedRequest != null)
                    {
                        ApplyFSDisposalByClassCodeRequestHierarchy(context, deserializedRequest);
                        return;
                    }

                    var applyClassCodeSetting = TryDeserializeApplyClassCodeSetting(json);
                    if (applyClassCodeSetting != null)
                    {
                        ApplyApplyClassCodeSettingHierarchy(context, applyClassCodeSetting);
                        return;
                    }

                    var deserializedNode = TryDeserializeTreeNode(json);
                    if (deserializedNode != null)
                    {
                        ApplyRMFSTreeNodeHierarchy(context, deserializedNode);
                        return;
                    }
                }
            }
        }

        private void ApplyFSDashboardRequestHierarchy(FSAuditContext context, FileSystemMyhubSelectedNodeDto deserializedFsDashboard)
        {
            context.CurrentPath = deserializedFsDashboard.FullPath ?? string.Empty;
            context.ConnectionId = new Guid(deserializedFsDashboard.PartitionKeyId);
            var connectionInfo = FSConnectionDao.GetConnectionById(context.ConnectionId);
            context.ConnectionGroupId = connectionInfo?.GroupId ?? Guid.Empty;
        }

        private void ApplyRMFSTreeNodeHierarchy(FSAuditContext context, RMFSTreeNode node)
        {
            context.ConnectionGroupId = node.ConnGroupId;
            context.CurrentPath = node.FullPath ?? string.Empty;
            if (node.Level == (int)NodeLevel.SiteCollection)
            {
                context.ConnectionId = node.Id;
            }
            else if (node.Level > (int)NodeLevel.SiteCollection)
            {
                context.ItemId = node.Id;
                if (Guid.TryParse(node.ConnectionId, out var connId) && connId != Guid.Empty)
                    context.ConnectionId = connId;
                else if (!string.IsNullOrEmpty(node.FullPath))
                {
                    var parentConn = FSConnectionDao.GetParentConnectionInfo(node.FullPath);
                    if (parentConn != null)
                        context.ConnectionId = parentConn.Id;
                }
            }
        }

        private void ApplyClassCodePolicyHierarchy(FSAuditContext context, ClassCodePolicyInfo policy)
        {
            var groupId = Guid.Parse(policy.ConnGroupId);
            var nodeId = Guid.Parse(policy.CurrentNodeId);
            context.ConnectionGroupId = groupId;
            if (nodeId == groupId) return;

            var connection = FSConnectionDao.GetConnectionById(nodeId);
            if (connection != null)
            {
                context.ConnectionId = nodeId;
                context.CurrentPath = connection.UNCPath ?? string.Empty;
                return;
            }

            context.ItemId = nodeId;
            var treeNode = policy.FSTreeNode;
            if (treeNode == null) return;

            context.CurrentPath = treeNode.FullPath ?? string.Empty;
            if (Guid.TryParse(treeNode.ConnectionId, out var connId) && connId != Guid.Empty)
            {
                context.ConnectionId = connId;
            }
            else if (!string.IsNullOrEmpty(treeNode.FullPath))
            {
                var parentConn = FSConnectionDao.GetParentConnectionInfo(treeNode.FullPath);
                if (parentConn != null)
                    context.ConnectionId = parentConn.Id;
            }
        }

        private void ApplyFSDisposalByClassCodeRequestHierarchy(FSAuditContext context, FSDisposalByClassCodeRequest request)
        {
            context.ConnectionGroupId = request.ConnectionGroupID;
            if (request.NodeId == Guid.Empty || request.NodeId == request.ConnectionGroupID) return;

            if (request.Level == (int)NodeLevel.SiteCollection)
            {
                context.AuditLevel = FSAuditLevel.Connection;
                context.ConnectionId = request.NodeId;
                context.CurrentPath = request.FullPath;
            }
            else if (request.Level == (int)NodeLevel.WebApplication)
            {
                context.AuditLevel = FSAuditLevel.ConnectionGroup;
            }
        }

        private static ApplyClassCodeSettingDto TryDeserializeApplyClassCodeSetting(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var setting = SerializerHelper.DeserializeByDataContractSerializer<ApplyClassCodeSettingDto>(json);
                return setting?.FSTreeNode?.Count > 0 ? setting : null;
            }
            catch
            {
                return null;
            }
        }

        private void ApplyApplyClassCodeSettingHierarchy(FSAuditContext context, ApplyClassCodeSettingDto setting)
        {
            var firstNode = setting.FSTreeNode?.FirstOrDefault();
            if (firstNode == null) return;

            ApplyNodeHierarchyToContext(context, firstNode);
        }

        private static RMFSTreeNode TryDeserializeTreeNode(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var node = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(json);
                return node?.Id != Guid.Empty ? node : null;
            }
            catch
            {
                try
                {
                    var node = Newtonsoft.Json.JsonConvert.DeserializeObject<RMFSTreeNode>(json);
                    return node?.Id != Guid.Empty ? node : null;
                }
                catch
                {
                    return null;
                }
            }
        }
        private FSAuditContext AfterClassCodeJobRun(FSAuditContext context, object[] arguments, object returnValue)
        {
            ResolveJobRunHierarchyIds(context, arguments);

            var jobId = returnValue as string;
            if (!string.IsNullOrEmpty(jobId))
            {
                context.ObjectName = jobId;
            }

            FSDisposalByClassCodeRequest request = null;

            // args: [JobRunBy jobRunBy, string jobRunByUser, string param]
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is FSDisposalByClassCodeRequest directRequest)
                {
                    request = directRequest;
                    break;
                }
                if (arguments[i] is string json && !string.IsNullOrWhiteSpace(json))
                {
                    request = TryDeserializeClassCodeRequest(json);
                    if (request != null) break;
                }
            }

            if (request?.TermID != null && request.TermID.Count > 0)
            {
                var classCodeNames = request.TermID
                    .Select(termId => TermDao.GetRMTermByUniqueId(termId)?.Name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                if (classCodeNames.Count > 0)
                {
                    context.AddModifiedContent(
                        string.Empty,
                        string.Empty,
                        string.Join("; ", classCodeNames));
                }
            }

            return context;
        }
        private FSAuditType GetMultiGeoAction(object[] args, FSAuditType auditType)
        {
            if (args == null || args.Length == 0)
                return auditType;

            var operation = args[0] switch
            {
                ConnectionGroupDto dto => dto.MultiGeoOperation,
                ConnectionDto dto => dto.MultiGeoOperation,
                _ => MultiGeoOperation.None
            };
            return operation switch
            {
                MultiGeoOperation.MultiGeoCreateFSGroup => FSAuditType.CreateFSGroup,
                MultiGeoOperation.MultiGeoEditFSGroup => FSAuditType.EditFSGroup,
                MultiGeoOperation.MultiGeoCreateFSConnection => FSAuditType.CreateFSConnection,
                MultiGeoOperation.MultiGeoEditFSConnection => FSAuditType.EditFSConnection,
                MultiGeoOperation.None => auditType,
                _ => auditType,
            };
        }
        private T DeserializeXml<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            try
            {
                using var reader = System.Xml.XmlReader.Create(new System.IO.StringReader(xml));
                return new System.Runtime.Serialization.DataContractSerializer(typeof(T)).ReadObject(reader) as T;
            }
            catch { return null; }
        }
        #endregion
    }
}