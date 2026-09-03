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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class EXOSettingDao : BaseDao<RMExchangeOnlineSetting>, IEXOSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(EXOSettingDao));
        public IScheduleService ScheduleService { get; set; }
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        private IRMMailboxDao mRMMailboxDao;
        public IRMMailboxDao RMMailboxDao
        {
            get
            {
                if (mRMMailboxDao == null)
                {
                    mRMMailboxDao = (IRMMailboxDao)PlatformWindsorManager.GetService(typeof(IRMMailboxDao));
                }
                return mRMMailboxDao;
            }

        }
        public IEXOSettingRuleDao EXOSettingRuleDao { get; set; }

        public async Task AddOrUpdateCustomSettingAsync(RMEXOTreeNode node, Guid siteId)
        {
            EnsureTermName(node);
            using var context = GetNewContext();
            var groupId = GetGroupIdById(new Guid(node.Id));
            //SaveEXOLoactionOwners
            var groupSetting = await context.RMExchangeOnlineSettings.Where(o => o.GroupId == groupId && o.ScopeId == groupId).FirstOrDefaultAsync();
            //if(groupSetting.IsNullClassificationSetting)
            RMExchangeOnlineSetting exoSetting = await context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId.Equals(new Guid(node.Id)) && !s.IsRemoved).FirstOrDefaultAsync();
            if (exoSetting == null)
            {
                //add this for RA 3.1 old data.
                exoSetting = await context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId.Equals(new Guid(node.Id)) && !s.IsRemoved).FirstOrDefaultAsync();
            }
            if (exoSetting != null)
            {
                exoSetting.DefaultTermId = node.DefaultTermId;
                exoSetting.DefaultTermName = node.DefaultTermName;
                //exoSetting.FullPath = node.FullPath;
                exoSetting.Name = node.Name;
                exoSetting.MailBoxId = node.MailBoxId;
                exoSetting.ScopeId = new Guid(node.Id);
                exoSetting.TermId = node.TermId;
                exoSetting.TermName = node.TermName;
                exoSetting.TermSetId = node.TermSetId;
                exoSetting.TermSetName = node.TermSetName;
                exoSetting.EnableRecordManagement = node.EnableRecordManagement;
                exoSetting.Level = node.Level;
                exoSetting.ParentId = new Guid(node.ParentId);
                exoSetting.GroupId = node.GroupId;
                exoSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                exoSetting.SettingTime = 0;
                exoSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                //exoSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                exoSetting.ApplyExistType = node.ApplyExistType;
                exoSetting.IsNewEdited = true;
                exoSetting.DeployTermMethod = (int)node.DeployTermMethod;
                exoSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                    null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                exoSetting.RunAutoFullJob = node.RunAutoFullJob;
                exoSetting.AutoJobOption = (int)node.AutoJobOption;
                //exoSetting.IdPath = node.ProfileId;
                exoSetting.IsSyncData = node.IsSyncData;
                exoSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                exoSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                await this.UpdateAsync(exoSetting);
                await RecordOwnerDao.UpdateRecordOwnersAsync(exoSetting.Id, node.RecordOwner, RecordOwnerSettingType.ExchangeOnline);
            }
            else
            {
                if (groupSetting != null && groupSetting.IsNullClassificationSetting)
                {
                    node.TermSetId = Guid.Empty;
                    node.TermId = Guid.Empty;
                    node.DefaultTermId = Guid.Empty;
                    node.TermSetName = string.Empty;
                }
                RMExchangeOnlineSetting settings = new RMExchangeOnlineSetting()
                {
                    DefaultTermId = node.DefaultTermId,
                    DefaultTermName = node.DefaultTermName,
                    //FullPath = node.FullPath,
                    Name = node.Name,
                    MailBoxId = node.MailBoxId,
                    ScopeId = new Guid(node.Id),
                    TermId = node.TermId,
                    TermName = node.TermName,
                    TermSetId = node.TermSetId,
                    TermSetName = node.TermSetName,
                    EnableRecordManagement = node.EnableRecordManagement,
                    Level = node.Level,
                    ParentId = new Guid(node.ParentId),
                    GroupId = node.GroupId,
                    EMailToRecordOwner = node.EMailToRecordOwner,
                    SettingTime = 0,
                    //NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                    ApplyExistType = node.ApplyExistType,
                    IsNewEdited = true,
                    //IdPath = node.ProfileId,
                    NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                    DeployTermMethod = (int)node.DeployTermMethod,
                    AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                    RunAutoFullJob = node.RunAutoFullJob,
                    AutoJobOption = (int)node.AutoJobOption,
                    IsSyncData = node.IsSyncData,
                    ApprovalType = (ApprovalType)node.ApprovalType,
                    WorkflowReferenceId = node.WorkflowReferenceId
                };
                //New Dispose Job Schedule & Collection Job Schedule
                //if (node.DisposeScheduleInfo != null)
                //{
                //    node.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                //    node.DisposeScheduleInfo.ProfileId = node.ProfileId;
                //    //REC-3945, Start Time和End Time中包含时区, 界面操作截取了字符串, 但其他Setting打破继承没有截取非法字符.
                //    node.DisposeScheduleInfo.StartTime = node.DisposeScheduleInfo.StartTime.Substring(0, 19);
                //    node.DisposeScheduleInfo.EndTime = node.DisposeScheduleInfo.EndTime.Substring(0, 19);
                //    var dSchedule = ScheduleService.CopyCreateScheduleService(node.DisposeScheduleInfo, false, GetNodeFullPath(node));
                //    if (string.IsNullOrEmpty(dSchedule))
                //    {
                //        node.DisposeScheduleInfo.Id = string.Empty;
                //    }

                //    settings.DisposalJobId = node.DisposeScheduleInfo.Id;
                //}
                //if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SiteCollection)
                //{
                //    if (node.CollectionScheduleInfo != null)
                //    {
                //        node.CollectionScheduleInfo.Id = Guid.NewGuid().ToString();
                //        node.CollectionScheduleInfo.ProfileId = node.ProfileId;
                //        //REC-3945, Start Time和End Time中包含时区, 界面操作截取了字符串, 但其他Setting打破继承没有截取非法字符.
                //        node.CollectionScheduleInfo.StartTime = node.CollectionScheduleInfo.StartTime.Substring(0, 19);
                //        node.CollectionScheduleInfo.EndTime = node.CollectionScheduleInfo.EndTime.Substring(0, 19);
                //        var cSchedule = ScheduleService.CopyCreateScheduleService(node.CollectionScheduleInfo, false, GetNodeFullPath(node));
                //        if (string.IsNullOrEmpty(cSchedule))
                //        {
                //            node.CollectionScheduleInfo.Id = string.Empty;
                //        }

                //        settings.CollectionJobId = node.CollectionScheduleInfo.Id;
                //    }
                //}
                //else
                //{
                //    settings.CollectionJobId = string.Empty;
                //}
                context.RMExchangeOnlineSettings.Add(settings);
                await context.SaveChangesAsync();
                exoSetting = await context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId == settings.ScopeId && !s.IsRemoved).FirstAsync();
                await RecordOwnerDao.AddRecordOwnersAsync((exoSetting.Id), node.RecordOwner, RecordOwnerSettingType.ExchangeOnline);
            }
        }
        /// <summary>
        /// method for upgrade
        /// </summary>
        /// <param name="exoSetting">exist sp setting</param>
        public async Task AddOrUpdateCustomSettingAsync(RMExchangeOnlineSetting exoSetting)
        {
            using var context = GetNewContext();
            using (var ctx = GetNewContext())
            {
                var setting = await ctx.RMExchangeOnlineSettings.Where(s => s.ScopeId == exoSetting.ScopeId).FirstOrDefaultAsync();
                if (setting != null)
                {
                    setting.DefaultTermId = exoSetting.DefaultTermId;
                    setting.DefaultTermName = exoSetting.DefaultTermName;
                    //setting.FullPath = exoSetting.FullPath;
                    setting.Name = exoSetting.Name;
                    setting.MailBoxId = exoSetting.MailBoxId;
                    setting.ScopeId = exoSetting.ScopeId;
                    setting.Level = exoSetting.Level;
                    setting.ParentId = exoSetting.ParentId;
                    setting.TermId = exoSetting.TermId;
                    setting.TermName = exoSetting.TermName;
                    setting.TermSetId = exoSetting.TermSetId;
                    setting.TermSetName = exoSetting.TermSetName;
                    setting.EnableRecordManagement = exoSetting.EnableRecordManagement;
                    setting.GroupId = exoSetting.GroupId;
                    setting.SettingTime = 0;
                    setting.IsNewEdited = true;
                    //setting.NeedCheckDefaultValue = exoSetting.NeedCheckDefaultValue;
                    setting.ApplyExistType = exoSetting.ApplyExistType;
                    //setting.CollectionJobId = exoSetting.CollectionJobId;
                    //setting.DisposalJobId = exoSetting.DisposalJobId;
                    //setting.IdPath = exoSetting.IdPath;
                    setting.NodeInfo = exoSetting.NodeInfo;
                    await UpdateAsync(setting);
                }
                else
                {

                    ctx.RMExchangeOnlineSettings.Add(exoSetting);
                    ctx.SaveChanges();
                }

            }
        }
        
        public async Task AddOrUpdateGlobalSettingAsync(RMEXOTreeNode node)
        {
            EnsureTermName(node);
            using var context = GetNewContext();
            RMExchangeOnlineSetting exoSetting = await context.RMExchangeOnlineSettings.AsQueryable().Where(s => !s.IsRemoved && s.ScopeId.Equals(new Guid(node.Id))).FirstOrDefaultAsync();
            if (exoSetting != null)
            {
                exoSetting.DefaultTermId = node.DefaultTermId;
                exoSetting.DefaultTermName = node.DefaultTermName;
                //exoSetting.FullPath = node.FullPath;
                exoSetting.Name = node.Name;
                exoSetting.MailBoxId = node.MailBoxId;
                exoSetting.ScopeId = new Guid(node.Id);
                exoSetting.ParentId = new Guid(node.ParentId);
                exoSetting.Level = node.Level;
                exoSetting.TermId = node.TermId;
                exoSetting.TermName = node.TermName;
                exoSetting.TermSetId = node.TermSetId;
                exoSetting.TermSetName = node.TermSetName;
                //exoSetting.IdPath = node.ProfileId;
                exoSetting.EnableRecordManagement = node.EnableRecordManagement;
                exoSetting.GroupId = node.GroupId;
                exoSetting.SettingTime = 0;
                //exoSetting.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
                exoSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                exoSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                exoSetting.ApplyExistType = node.ApplyExistType;
                exoSetting.IsNewEdited = true;
                exoSetting.DeployTermMethod = (int)node.DeployTermMethod;
                exoSetting.AutoClassificationRules = node.AutoClassificationRules == null ?
                    null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
                exoSetting.RunAutoFullJob = node.RunAutoFullJob;
                exoSetting.AutoJobOption = (int)node.AutoJobOption;
                exoSetting.IsSyncData = node.IsSyncData;
                exoSetting.ApprovalType = (ApprovalType)node.ApprovalType;
                exoSetting.WorkflowReferenceId = node.WorkflowReferenceId;
                exoSetting.IsNullClassificationSetting = node.IsNullClassificationSetting;
                await this.UpdateAsync(exoSetting);
                await RecordOwnerDao.UpdateRecordOwnersAsync(exoSetting.Id, node.RecordOwner, RecordOwnerSettingType.ExchangeOnline);
            }
            else
            {
                RMExchangeOnlineSetting settings = new RMExchangeOnlineSetting()
                {
                    DefaultTermId = node.DefaultTermId,
                    DefaultTermName = node.DefaultTermName,
                    //FullPath = node.FullPath,
                    Name = node.Name,
                    MailBoxId = node.MailBoxId,
                    ScopeId = new Guid(node.Id),
                    ParentId = new Guid(node.ParentId),
                    Level = node.Level,
                    TermId = node.TermId,
                    TermName = node.TermName,
                    TermSetId = node.TermSetId,
                    TermSetName = node.TermSetName,
                    //IdPath = node.ProfileId,
                    EnableRecordManagement = node.EnableRecordManagement,
                    GroupId = node.GroupId,
                    SettingTime = 0,
                    //NeedCheckDefaultValue = node.NeedCheckDefaultValue,
                    ApplyExistType = node.ApplyExistType,
                    EMailToRecordOwner = node.EMailToRecordOwner,
                    IsNewEdited = true,
                    NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                    DeployTermMethod = (int)node.DeployTermMethod,
                    AutoClassificationRules = node.AutoClassificationRules == null ?
                        null : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules),
                    RunAutoFullJob = node.RunAutoFullJob,
                    AutoJobOption = (int)node.AutoJobOption,
                    IsSyncData = node.IsSyncData,
                    ApprovalType = (ApprovalType)node.ApprovalType,
                    WorkflowReferenceId = node.WorkflowReferenceId,
                    IsNullClassificationSetting = node.IsNullClassificationSetting
                };
                context.RMExchangeOnlineSettings.Add(settings);
                context.SaveChanges();
                exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                await RecordOwnerDao.AddRecordOwnersAsync((exoSetting.Id), node.RecordOwner, RecordOwnerSettingType.ExchangeOnline);
            }
            EXOSettingRuleDao.SaveMappingRules(node);
        }
        /// <summary>
        /// 获取Global或Custom Setting
        /// </summary>
        /// <param name="id"></param>
        /// <param name="siteId"></param>
        /// <param name="includeOnlySetPhysicalNode">是否获取只设置了“Mark the Physical Library”的节点。
        /// (界面回显需要传true，其他获取SharePoint Setting的情况不需要传值)</param>
        /// <returns></returns>
        public RMExchangeOnlineSetting LoadSharePointSetting(Guid id, Guid siteId, bool includeOnlySetPhysicalNode = false)
        {
            using var context = GetNewContext();
            RMExchangeOnlineSetting exoSetting = null;
            if (siteId != Guid.Empty)
            {
                var mailbox = RMMailboxDao.GetEmailById(siteId.ToString());
                var groupId = mailbox?.ParentId;
                if (!string.IsNullOrEmpty(groupId))
                {
                    exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.GroupId.Equals(new Guid(groupId)) && !s.IsRemoved).FirstOrDefault();
                }
                else
                {
                    exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && !s.IsRemoved).FirstOrDefault();
                }
            }
            if (exoSetting == null)
            {
                exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(id) && s.MailBoxId.Equals(Guid.Empty) && !s.IsRemoved).FirstOrDefault();
            }
            return exoSetting;
        }

        public RMExchangeOnlineSetting LoadExchangeOnlineSetting(Guid currentNodeId, Guid parentId)
        {
            using var context = GetNewContext();
            RMExchangeOnlineSetting exoSetting = null;

            exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(currentNodeId) && !s.IsRemoved).FirstOrDefault();

            if (exoSetting == null)
            {
                exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(parentId) && !s.IsRemoved).FirstOrDefault();
            }
            return exoSetting;
        }

        public async Task DeleteSharePointSettingAsync(Guid id, Guid siteId)
        {
            using var context = GetNewContext();
            var groupId = GetGroupIdById(siteId);
            RMExchangeOnlineSetting exoSetting = await context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId.Equals(id) && !s.IsRemoved).FirstOrDefaultAsync();
            if (exoSetting != null)
            {
                context.RMExchangeOnlineSettings.Remove(exoSetting);
                await RecordOwnerDao.BatchDeleteAsync(o => o.SPSettingId == exoSetting.Id);
                await context.SaveChangesAsync();
            }
        }

        public static string ForeachClassProperties<T>(T model)
        {
            var builder = new StringBuilder();
            builder.Append("{");
            Type t = model.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                object value = item.GetValue(model, null);
                builder.AppendFormat(@"""{0}"":""{1}"", ", name, value?.ToString().Replace("\"", "\\\""));
            }
            builder.Remove(builder.Length - 2, 2);//remove , and space
            builder.Append("}");
            return builder.ToString();
        }

        public void MarkRemovedSharePointSetting(Guid scopeId)
        {
            using var context = GetNewContext();
            RMExchangeOnlineSetting exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
            if (exoSetting != null)
            {
                logger.Info("mark removed SharePoint setting dirty data:{0}", ForeachClassProperties(exoSetting));
                exoSetting.IsRemoved = true;
                //context.RMExchangeOnlineSettings.Remove(exoSetting);
                //var deletes = RecordOwnerDao.FindList(o => o.SPSettingId == exoSetting.Id);
                //foreach (var item in deletes)
                //{
                //    logger.Info("remove record owner dirty data:{0}", ForeachClassProperties(item));
                //}
                //RecordOwnerDao.BatchDelete(deletes);
                context.SaveChanges();
            }
        }

        public async Task MarkRemovedSharePointSettingUnderCurrentAsync(Expression<Func<RMExchangeOnlineSetting, bool>> lambda)
        {
            using var context = GetNewContext();
            var deletes = await FindListAsync(lambda);
            foreach (var item in deletes)
            {
                logger.Info("mark removed SharePoint setting dirty data:{0}", ForeachClassProperties(item));
                item.IsRemoved = true;
            }
            context.SaveChanges();
            //BatchDelete(deletes);
        }

        public List<RMExchangeOnlineSetting> GetColumnInfos(string[] ids)
        {
            using var context = GetNewContext();
            List<RMExchangeOnlineSetting> settings = null;
            var groupId = GetGroupIdById(new Guid(ids.FirstOrDefault()));
            if (groupId == Guid.Empty)
            {
                settings = context.RMExchangeOnlineSettings.AsQueryable().Where(t => Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
            }
            else
            {
                settings = context.RMExchangeOnlineSettings.AsQueryable().Where(t => t.GroupId == groupId && Enumerable.Contains(ids, t.ScopeId.ToString()) && !t.IsRemoved).ToList();
            }
            if (!settings.Any())
            {
                return new List<RMExchangeOnlineSetting>();
            }
            return settings;
        }
        public List<RMExchangeOnlineSetting> LoadExchangeOnlineGroupSetting()
        {
            using var context = GetNewContext();
            return context.RMExchangeOnlineSettings.AsNoTracking().Where(s => s.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup && !s.IsRemoved).ToList();
        }
        public void DeleteCustomSettingUsingExistColumn(Guid groupId)
        {
            using var context = GetNewContext();
            var entities = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && !s.IsRemoved);

            this.BatchDelete(entities.ToList());
        }

        public RMExchangeOnlineSetting GetSettingInfoByAgentGroupId(string id)
        {
            using var context = GetNewContext();
            RMExchangeOnlineSetting exoSetting = context.RMExchangeOnlineSettings.AsNoTracking().Where(s => s.ScopeId.Equals(new Guid(id)) && !s.IsRemoved).FirstOrDefault();
            return exoSetting;
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == 1).ToList();
                return owners.ConvertAll(item =>
                {
                    var owner = AccountDao.Find(s => s.UserId == item.ObjectId);
                    return new GCommon.Contract.StorageOptimization.Object.UserInfo
                    {
                        UserId = owner.UserId,
                        UserPrincipalName = owner.UserPrincipalName,
                        DisplayName = owner.DisplayName,
                        Email = owner.UserPrincipalName,
                        InviteType = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? GCommon.Contract.Server.Login.InviteType.Group : GCommon.Contract.Server.Login.InviteType.User
                    };
                });
            }
        }

        public List<RecordOwnerGroupDto> GetRecordOwners(HashSet<Guid> groupIds, HashSet<Guid> siteIds)
        {
            var results = new List<RecordOwnerGroupDto>();
            using var context = GetNewContext();
            var settings = context.RMExchangeOnlineSettings.AsQueryable()
                //.Where(s => (siteIds.Contains(s.SiteId) || groupIds.Contains(s.ScopeId)) && !s.IsRemoved)
                .Where(s => groupIds.Contains(s.ScopeId) && !s.IsRemoved)
                .Select(s => new RecordOwnerGroupDto()
                {
                    SPSettingId = s.Id,
                    ScopeId = s.ScopeId,
                    SiteGroupId = s.GroupId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

            if (settings.Count > 0)
            {
                var settingIds = settings.Keys;
                var ownerGroups = context.RecordOwner.AsQueryable()
                    .Where(o => settingIds.Contains(o.SPSettingId))
                    .GroupBy(o => o.SPSettingId).ToList();
                foreach (var setting in settings)
                {
                    try
                    {
                        var groupDto = ownerGroups.Where(t => t.Key == setting.Key).FirstOrDefault();
                        if (groupDto != null)
                        {
                            setting.Value.AddOwnerRange(groupDto.Select(o =>
                            {
                                var objectId = o.ObjectId;
                                var owner = AccountDao.Find(s => s.UserId == objectId);
                                if (owner == null)
                                {
                                    return null;
                                }
                                return new RecordOwnerDto()
                                {
                                    LnkId = owner.Id,
                                    ObjectId = o.ObjectId,
                                    DisplayName = owner.DisplayName,
                                    UserPrincipalName = owner.UserPrincipalName,
                                    Type = owner.ObjectType == Contract.RMWeb.RMActiveDirectoryObjectType.Group ? AccountType.Group : AccountType.User,
                                };
                            }));
                        }
                        results.Add(setting.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("get record owner {0} error:{1}", setting.Value.ScopeId, ex.ToString());
                    }
                }

            }

            return results;
        }

        public void UpdateRecordOwnerUserPrincipalName(RecordOwnerDto owner)
        {
            using var context = GetNewContext();
            var entities = context.RecordOwner.AsQueryable().Where(o => o.ObjectId == owner.ObjectId);
            foreach (var entity in entities)
            {
                //entity.UserPrincipalName = owner.UserPrincipalName;
            }
            RecordOwnerDao.BatchUpdate(entities.ToList());
        }

        public async Task<bool> CleanSettingJobTimeAsync(RMEXOTreeNode node)
        {
            try
            {
                using var context = GetNewContext();
                var groupId = Guid.Empty;
                var scopeId = new Guid(node.Id);
                if (node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
                {
                    groupId = scopeId;
                }
                else
                {
                    groupId = GetGroupIdById(scopeId);
                }

                var setting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId.Equals(new Guid(node.Id)) && !s.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    setting.SettingTime = 0;
                    await UpdateAsync(setting);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                //to do log 
                return false;
            }
        }

        private Guid GetGroupIdById(Guid id)
        {
            var mailBox = RMMailboxDao.GetEmailById(id.ToString());
            if (mailBox == null)
            {
                return Guid.Empty;
            }
            else
            {
                return mailBox.NodeLevel == NodeLevel.ExchangeOnlineMailboxGroup ? new Guid(mailBox.Id) : new Guid(mailBox.ParentId);
            }
        }
        public async Task SetSettingInfoAsync(Guid scopeId, long timeTicks, bool runAutoFullJob)
        {
            try
            {
                using var context = GetNewContext();
                var groupId = GetGroupIdById(scopeId);
                var setting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    setting.SettingTime = timeTicks;
                    setting.RunAutoFullJob = runAutoFullJob;
                    setting.UpdateDate = DateTime.UtcNow.Ticks;
                }
                await UpdateAsync(setting);
            }
            catch (Exception ex)
            {
                logger.Error($"Error in SetSettingInfo, reason : {ex.ToString()}");
            }
        }

        public async Task SetSettingInfoAsync(Guid groupId, Guid scopeId, long timeTicks, bool runAutoFullJob)
        {
            try
            {
                using var context = GetNewContext();
                var setting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.ScopeId.Equals(scopeId) && !s.IsRemoved).FirstOrDefault();
                if (setting != null)
                {
                    setting.SettingTime = timeTicks;
                    setting.RunAutoFullJob = runAutoFullJob;
                    setting.UpdateDate = DateTime.UtcNow.Ticks;
                }
                await UpdateAsync(setting);
            }
            catch (Exception ex)
            {
                logger.Error($"Error in SetSettingInfo, reason : {ex.ToString()}");
            }
        }

        public List<RMExchangeOnlineSetting> LoadRunJobSetting()
        {
            using var context = GetNewContext();
            return context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.SettingTime.Equals(0) && s.NodeInfo != null && !s.IsRemoved).ToList();
        }

        public List<RMExchangeOnlineSetting> LoadAllGroupSettings()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == s.ScopeId && s.NodeInfo != null && !s.IsRemoved).ToList();
            }
        }

        public List<RMExchangeOnlineSetting> LoadAllSettingForAS()
        {
            using var context = GetNewContext();
            return context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
        }
        public List<RMExchangeOnlineSetting> LoadAllSettingForDS()
        {
            using var context = GetNewContext();
            return context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved && s.IsSyncData).ToList();
        }

        public List<RMExchangeOnlineSetting> LoadAllSetting()
        {
            using var context = GetNewContext();
            return context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.NodeInfo != null && !s.IsRemoved).ToList();
        }
        public List<RMExchangeOnlineSetting> LoadExcludeSiteCollectionSetting()//TODO Leon
        {
            using var context = GetNewContext();
            //return context.RMExchangeOnlineSettings.AsQueryable().Where(s => !s.NodeInfo.Equals(null) && s.ScopeId.Equals(s.SiteId)).ToList();
            return context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.NodeInfo != null).ToList();
        }

        #region re sps
        public List<RMExchangeOnlineSetting> GetDescendantsDisableNodes(RMEXOTreeNode node)
        {
            var scopeId = new Guid(node.Id);
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMExchangeOnlineSettings.Where(s => s.GroupId == scopeId && s.ScopeId != scopeId && s.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable).ToList();
            }

        }
        public List<RMExchangeOnlineSetting> GetDescendantsBreakNodesForNullClassification(RMEXOTreeNode node)
        {
            var scopeId = new Guid(node.Id);
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                return context.RMExchangeOnlineSettings.Where(s => s.GroupId == scopeId && s.ScopeId != scopeId && s.TermSetId != Guid.Empty).ToList();
            }
        }


        public RMExchangeOnlineSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId)
        {
            using (var ctx = GetNewContext())
            {
                //return ctx.RMExchangeOnlineSettings.Where(s => s.GroupId == groupId && s.SiteId == siteId && s.ScopeId == scopeId).FirstOrDefault();
                return ctx.RMExchangeOnlineSettings.Where(s => s.GroupId == groupId && s.ScopeId == scopeId).FirstOrDefault();
            }
        }

        public async Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMEXOTreeNode node, bool isNewEditd = false)
        {
            EnsureTermName(node);
            using var context = GetNewContext();
            RMExchangeOnlineSetting exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId.Equals(new Guid(node.Id))).FirstOrDefault();
            if (exoSetting != null)
            {
                exoSetting.SettingTime = 0;
                exoSetting.EnableRecordManagement = node.EnableRecordManagement;
                exoSetting.EMailToRecordOwner = node.EMailToRecordOwner;
                exoSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
                //exoSetting.IdPath = node.ProfileId;
                exoSetting.ParentId = new Guid(node.ParentId);
                exoSetting.Level = node.Level;
                exoSetting.IsSyncData = node.IsSyncData;
                if (isNewEditd)
                {
                    exoSetting.IsNewEdited = true;
                }
                await this.UpdateAsync(exoSetting);
                await RecordOwnerDao.UpdateRecordOwnersAsync(exoSetting.Id, node.RecordOwner, RecordOwnerSettingType.ExchangeOnline);
            }
            else
            {
                RMExchangeOnlineSetting settings = new RMExchangeOnlineSetting()
                {
                    //FullPath = node.FullPath,
                    Name = node.Name,
                    MailBoxId = node.MailBoxId,
                    ScopeId = new Guid(node.Id),
                    ParentId = new Guid(node.ParentId),
                    Level = node.Level,
                    GroupId = node.GroupId,
                    TermSetId = Guid.Empty,
                    TermId = Guid.Empty,
                    DefaultTermId = Guid.Empty,
                    EnableRecordManagement = node.EnableRecordManagement,
                    EMailToRecordOwner = node.EMailToRecordOwner,
                    SettingTime = 0,
                    NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node),
                    //IdPath = node.ProfileId,
                    IsSyncData = node.IsSyncData
                };
                if (isNewEditd)
                {
                    settings.IsNewEdited = true;
                }
                context.RMExchangeOnlineSettings.Add(settings);
                context.SaveChanges();
                exoSetting = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.ScopeId == settings.ScopeId).First();
                await RecordOwnerDao.AddRecordOwnersAsync((exoSetting.Id), node.RecordOwner, RecordOwnerSettingType.ExchangeOnline);
            }
            /*
             * 
             * 
             * REC-3771
            //remove all custom setting node
            DeleteCustomSettingUsingExistColumn(new Guid(node.SPObjectId));
             * 现在由于即使应用了Exist Column，子节点在保存schedule的时候，也会有打破继承的情况，
             * 故不可以直接将所有子节点删除，只能将其设置成IsNewEdit=false,IsUsingExistColumnName=true,
             * 在跑job的时候进行判断
             */
            SetCustomSettingUsingExistColumnByGroup(node);
        }
        
        private void EnsureTermName(RMEXOTreeNode node)
        {
            if (!string.IsNullOrEmpty(node.TermName) && node.TermName.Contains(":"))
            {
                node.TermName = node.TermName.Substring(node.TermName.LastIndexOf(":") + 1);
            }
            if (!string.IsNullOrEmpty(node.DefaultTermName) && node.DefaultTermName.Contains(":"))
            {
                node.DefaultTermName = node.DefaultTermName.Substring(node.DefaultTermName.LastIndexOf(":") + 1);
            }
        }
        public string GetNodeFullPath(RMEXOTreeNode node)
        {
            return node.FullPath;
        }

        public void SetCustomSettingUsingExistColumnByGroup(RMEXOTreeNode gNode)
        {
            using var context = GetNewContext();
            var entities = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == gNode.GroupId).ToList();

            foreach (var entity in entities)
            {
                entity.IsNewEdited = false;
                entity.SettingTime = 0;
                entity.EnableRecordManagement = gNode.EnableRecordManagement;
                entity.EMailToRecordOwner = gNode.EMailToRecordOwner;

            }

            this.BatchUpdate(entities);
        }
        #endregion

        #region check job skip
        public void UpdateRunningJobStartStatus(List<int> spIds, string startJobId)
        {
            //using (var context = GetNewContext())
            //{
            //    var dbSettings = context.RMExchangeOnlineSettings.AsQueryable().Where(s => spIds.Contains(s.Id)).ToList();
            //    foreach (var setting in dbSettings)
            //    {
            //        setting.IsNewEdited = false;
            //        setting.IsRunning = true;
            //        setting.SharePointSettingJobId = startJobId;
            //        Update(setting);
            //    }
            //}
            throw new NotImplementedException();
        }

        public void UpdateRunningJobFinishStatus(List<int> spIds)
        {
            //using (var context = GetNewContext())
            //{
            //    var dbSettings = context.RMExchangeOnlineSettings.AsQueryable().Where(s => spIds.Contains(s.Id)).ToList();
            //    foreach (var setting in dbSettings)
            //    {
            //        setting.IsNewEdited = false;
            //        setting.IsRunning = false;
            //        setting.SharePointSettingJobId = "";
            //        Update(setting);
            //    }
            //}
            throw new NotImplementedException();
        }
        public bool CheckJobIsSkip()
        {
            //var context = SharedDbContext;
            //return context.RMExchangeOnlineSettings.AsQueryable().Any(s => s.SettingTime.Equals(0) && s.IsRunning && s.IsNewEdited);
            throw new NotImplementedException();
        }
        public void FlagCustomSettingNewColumn(Guid groupId)
        {
            using var context = GetNewContext();
            //var entities = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.SiteId != Guid.Empty).ToList();
            var entities = context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId).ToList();
            foreach (var entity in entities)
            {
                entity.IsNewEdited = true;
                entity.SettingTime = 0;
            }

            this.BatchUpdate(entities);
        }


        #endregion
        public Dictionary<Guid, int> GetDisableDocClassification()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMExchangeOnlineSettings.Where(s => s.EnableRecordManagement == 2).Select(s => new { ScopeId = s.ScopeId, DocSeting = s.EnableRecordManagement }).ToDictionary(s => s.ScopeId, o => o.DocSeting);
            }
        }
        public void RemoveDescendantsSetting(RMEXOTreeNode node, string profileIdPath)
        {
            if (node.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                ScheduleService.DeleteSchedules(ScheduleType.EXODisposalSchedule, profileIdPath);
                var deleteDescendantsSql = "Delete From {0}.[RMExchangeOnlineSettings] Where {1} = @scopeId And ScopeId <> @scopeId";
                //var deleteScheduleSql = "Delete From {0}.[RMSchedules] Where Id In (SELECT {1} From {0}.[RMExchangeOnlineSettings] Where {2} = @scopeId)";
                var IdLevel = "";
                switch ((NodeLevel)node.Level)
                {
                    case NodeLevel.ExchangeOnlineMailboxGroup:
                        IdLevel = "GroupId";
                        break;
                    case NodeLevel.ExchangeOnlineMailbox:
                        IdLevel = "ScopeId";
                        break;
                }
                int result = 0;
                using (var context = RMDBContextManager.GetNewDBContext())
                {
                    AvePoint.GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    var sql = string.Format(deleteDescendantsSql, context.SchemaName, IdLevel);
                    //var deleteSql1 = string.Format(deleteScheduleSql, context.SchemaName, "DisposalJobId", IdLevel);
                    //var deleteSql2 = string.Format(deleteScheduleSql, context.SchemaName, "CollectionJobId", IdLevel);
                    using (var tran = context.Database.BeginTransaction())
                    {
                        //result = context.Database.ExecuteSqlCommand(deleteSql1, new SqlParameter("@scopeId", node.Id));
                        //result = context.Database.ExecuteSqlCommand(deleteSql2, new SqlParameter("@scopeId", node.Id));
                        result = context.Database.ExecuteSqlCommand(sql, new SqlParameter("@scopeId", node.Id));
                        tran.Commit();
                    }
                }
            }
        }
        public List<RMExchangeOnlineSetting> GetAllSettingsForGroup(RMEXOTreeNode current)
        {

            using var context = GetNewContext();
            var groupId = new Guid(current.Id);
            return context.RMExchangeOnlineSettings.AsQueryable().Where(s => s.GroupId == groupId && s.Level == (int)NodeLevel.ExchangeOnlineMailbox && !s.IsRemoved).ToList();
        }
    }
}
