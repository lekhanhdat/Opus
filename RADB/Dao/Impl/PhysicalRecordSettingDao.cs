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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class PhysicalRecordSettingDao : BaseDao<RMPhysicalRecordSetting>, IPhysicalRecordSettingDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(PhysicalRecordSettingDao));
        public IRecordOwnerDao RecordOwnerDao { get; set; }
        public IAccountDao AccountDao { get; set; }
        public RMPhysicalRecordSetting GetPhysicalRecordSetting(Guid locationUID)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMPhysicalRecordSetting.OrderByDescending(s => s.Id).FirstOrDefault(s => s.LocationUniqueId == locationUID);
            }
        }

        public List<RMPhysicalRecordSetting> GetPhysicalRecordSetting(List<Guid> locationUIDs)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMPhysicalRecordSetting.OrderByDescending(s => s.Id).Where(s => locationUIDs.Contains(s.LocationUniqueId)).ToList();
            }
        }

        public List<RMPhysicalRecordSetting> GetAllPhysicalRecordSettings()
        {
            using (var ctx = GetNewContext())
            {
                var settings = (from l in ctx.RMLocation.Where(o => !o.IsRemoved)
                                join s in ctx.RMPhysicalRecordSetting
                                on l.UniqueId equals s.LocationUniqueId
                                select s).ToList();
                return settings;
            }
        }

        public void SaveColumnName(Guid locationUID, string columnName, bool columnRequired = true)
        {
            using (var ctx = GetNewContext())
            {
                var entity = new RMPhysicalRecordSetting();
                var isCreate = false;
                var location = ctx.RMPhysicalRecordSetting.Where(s => s.LocationUniqueId == locationUID).FirstOrDefault();
                if (location == null)
                {
                    isCreate = true;
                }
                else
                {
                    entity = location;
                }

                entity.LocationUniqueId = locationUID;
                entity.ColumnName = columnName;
                entity.ColumnRequired = columnRequired;


                if (isCreate)
                {
                    ctx.RMPhysicalRecordSetting.Add(entity);
                }
                ctx.SaveChanges();
            }
        }

        public void SaveTerm(RMPRSaveTermDto saveTermDto)
        {
            using (var ctx = GetNewContext())
            {
                RMPhysicalRecordSetting entity = new RMPhysicalRecordSetting();
                var isCreate = false;
                var setting = ctx.RMPhysicalRecordSetting.Where(s => s.LocationUniqueId == saveTermDto.UniqueId).FirstOrDefault();
                if (setting == null)
                {
                    isCreate = true;
                    //if (saveTermDto.IsTopLevelSetting)
                    //{
                    //    logger.Error("Please configure column settings to define a classification column before configuring other settings");
                    //    return;
                    //}
                    //else
                    //{
                    //    entity = new RMPhysicalRecordSetting()
                    //    {
                    //        LocationUniqueId = saveTermDto.UniqueId
                    //    };
                    //    ctx.RMPhysicalRecordSetting.Add(entity);
                    //}
                }
                else
                {
                    entity = setting;
                }
                /* remove configure column name logic*/
                if (saveTermDto.IsTopLevelSetting)
                {
                    entity.ColumnName = "(Classification Column)";
                    entity.ColumnRequired = true;
                }

                entity.LocationUniqueId = saveTermDto.UniqueId;
                entity.DefaultTermName = saveTermDto.DefaultTermName;
                entity.DefaultTermId = saveTermDto.DefaultTermId;
                entity.TermName = saveTermDto.TermName;
                entity.TermId = saveTermDto.TermId;
                entity.TermSetId = saveTermDto.TermSetId;
                entity.TermSetName = saveTermDto.TermSetName;
                entity.DeployTermMethod = (int)saveTermDto.DeployTermMethod;
                if (isCreate)
                {
                    ctx.RMPhysicalRecordSetting.Add(entity);
                }
                ctx.SaveChanges();
            }
        }

        public RMPhysicalRecordSetting GetAncestryPhysicalRecordSetting(List<string> locationIds)
        {
            using (var ctx = GetNewContext())
            {
                List<int> locationIntIds = locationIds.Select(l => Convert.ToInt32(l)).ToList();
                var ancestryLocations = ctx.RMLocation.Where(l => locationIntIds.Contains(l.Id)).ToList();
                var ancestryLocationIds = ancestryLocations.Select(l => l.UniqueId).ToList();
                var ancestrySettings = ctx.RMPhysicalRecordSetting.Where(p => ancestryLocationIds.Contains(p.LocationUniqueId)).ToList();
                for (int i = locationIntIds.Count - 1; i >= 0; i--)
                {
                    var currLocation = ancestryLocations.FirstOrDefault(l => l.Id == locationIntIds[i]);
                    if (currLocation == null)
                    {
                        continue;
                    }
                    var ancestrySetting = ancestrySettings.FirstOrDefault(s => s.LocationUniqueId == currLocation.UniqueId);
                    if (ancestrySetting != null)
                    {
                        return ancestrySetting;
                    }
                }
                return null;
            }
        }

        public async Task SaveRecordOwnerAsync(RMPRSaveRecordOwnerDto recordOwnerDto)
        {
            using (var ctx = GetNewContext())
            {
                RMPhysicalRecordSetting entity = new RMPhysicalRecordSetting();
                var isCreate = false;
                var setting = ctx.RMPhysicalRecordSetting.Where(s => s.LocationUniqueId == recordOwnerDto.UniqueId).FirstOrDefault();
                if (setting == null)
                {
                    isCreate = true;
                }
                else
                {
                    entity = setting;
                }

                if (recordOwnerDto.IsTopLevelSetting)
                {
                    entity.ColumnName = "(Classification Column)";
                    entity.ColumnRequired = true;
                }
                entity.LocationUniqueId = recordOwnerDto.UniqueId;
                entity.EMailToRecordOwner = recordOwnerDto.EMailToRecordOwner;
                entity.ApprovalType = (ApprovalType)recordOwnerDto.ApprovalType;
                entity.WorkflowReferenceId = recordOwnerDto.WorkflowReferenceId;

                if (isCreate)
                {
                    ctx.RMPhysicalRecordSetting.Add(entity);
                }
                ctx.SaveChanges();
                await RecordOwnerDao.UpdateRecordOwnersAsync(entity.Id, recordOwnerDto.RecordOwner, RecordOwnerSettingType.PhysicalRecord);
            }
        }

        public List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId)
        {
            using (var context = GetNewContext())
            {
                var owners = context.RecordOwner.Where(item => item.SPSettingId == settingId && item.SettingType == (int)RecordOwnerSettingType.PhysicalRecord).ToList();
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

        public List<RecordOwnerGroupDto> GetRecordOwners(List<Guid> locationIds)
        {
            var results = new List<RecordOwnerGroupDto>();
            using (var context = GetNewContext())
            {
                var settings = context.RMPhysicalRecordSetting.AsQueryable()
                .Where(s => locationIds.Contains(s.LocationUniqueId) && !s.IsRemoved)
                .Select(s => new RecordOwnerGroupDto()
                {
                    ScopeId = s.LocationUniqueId,
                    SPSettingId = s.Id,
                    SiteGroupId = s.LocationUniqueId,
                    MailToOwner = s.EMailToRecordOwner
                }).ToDictionary(s => s.SPSettingId);

                if (settings.Count > 0)
                {
                    var settingIds = settings.Keys;
                    var ownerGroups = context.RecordOwner.AsQueryable()
                        .Where(o => settingIds.Contains(o.SPSettingId) && o.SettingType == (int)RecordOwnerSettingType.PhysicalRecord)
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
            }
            return results;
        }

        public int InheritParentSetting(Guid locationUID)
        {
            using (var ctx = GetNewContext())
            {
                var entity = ctx.RMPhysicalRecordSetting.Where(s => s.LocationUniqueId == locationUID).FirstOrDefault();
                if (entity != null)
                {
                    ctx.RMPhysicalRecordSetting.Remove(entity);
                    var record = ctx.RecordOwner.Where(r => r.SPSettingId == entity.Id).FirstOrDefault();
                    if (record != null)
                    {
                        ctx.RecordOwner.Remove(record);
                    }
                }
                return ctx.SaveChanges();
            }
        }

        public List<RMPhysicalRecordSetting> LoadAllSetting()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMPhysicalRecordSetting.AsQueryable().Where(s => !s.IsRemoved).ToList();
            }
        }
    }
}
