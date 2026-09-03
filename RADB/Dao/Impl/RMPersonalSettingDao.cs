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
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Extension;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMPersonalSettingDao : BaseDao<RMPersonalSetting>, IPersonalSettingDao
    {
        public int CreateOrUpdate(RMPersonalSettingDto dto)
        {
            #region Obsolete code
            //var needClearDefaultFalg4Others = false; //if current setting is set as default, need to cancel default for other settings
            //var entity = base.Find(o => o.Id == dto.Id);
            //if (entity == null)
            //{
            //    needClearDefaultFalg4Others = dto.IsDefault;
            //    entity = base.Create(dto.Convert2Entity());
            //}
            //else
            //{
            //    needClearDefaultFalg4Others = dto.IsDefault && !entity.IsDefault;
            //    dto.Assemble2Entity(entity);
            //    base.Update(entity);
            //}

            //if (needClearDefaultFalg4Others)
            //{
            //    ClearDefault(entity.Id, entity.Type, entity.Owner);
            //}
            #endregion
            using var context = GetNewContext();
            var entity = base.Find(o => o.Id == dto.Id);
            if (entity == null)
            {
                entity = dto.Convert2Entity();
                context.RMPersonalSetting.Add(entity);
            }
            else
            {
                dto.Assemble2Entity(entity);
                context.RMPersonalSetting.AddOrUpdate(entity);
            }
            using (DbContextTransaction tran = context.Database.BeginTransaction())
            {
                context.SaveChanges(); //in order to get the id if it is a new entity
                if (dto.IsDefault)
                {
                    SetAsDefault(context, dto.Owner, entity.Id, dto.Type);
                    context.SaveChanges();
                }
                tran.Commit();
            }
            return entity.Id;
        }

        public void UpgradeDefaultSetting(string owner, PersonalSettingType type)
        {
            using var context = GetNewContext();
            var entities = context.RMPersonalSetting.Where(o => o.Owner == owner && o.Type == type && o.IsDefault).ToList();
            if (entities.Count == 0) return;
            using (DbContextTransaction tran = context.Database.BeginTransaction())
            {
                var first = entities.First();
                entities.ForEach(o => o.IsDefault = false);
                SetAsDefault(context,first.Owner, first.Id, first.Type);
                context.SaveChanges();
                tran.Commit();
            }
        }

        public bool ExistSameNameEntity(RMPersonalSettingDto dto)
        {
            return base.Exist(o => o.Id != dto.Id && (o.Owner == dto.Owner && o.Name == dto.Name && o.Type == dto.Type));
        }

        public Task<int> DeleteByIdsAsync(string owner, List<int> ids)
        {
            return base.BatchDeleteAsync(o => owner == o.Owner && ids.Contains(o.Id));
        }

        public RMPersonalSettingDto GetById(int id, bool includeContent = true)
        {
            if (!includeContent) return GetByIdWithoutContent(id);

            var entity = base.Find(o => o.Id == id);
            if (entity != null)
            {
                var dto = entity.Convert2Dto(includeContent);
                using var context = GetNewContext();
                dto.IsDefault = IsDefaultSetting(context,entity.Owner, entity.Type, entity.Id);
                return dto;
            }
            return null;
        }

        private RMPersonalSettingDto GetByIdWithoutContent(int id)
        {
            using var context = GetNewContext();
            var entity = context.RMPersonalSetting.Where(o => o.Id == id)
                .Select(o => new { o.Id, o.Name, o.Owner, /*o.IsDefault, */o.IsBuiltIn, o.Type })
                .FirstOrDefault();

            return entity != null ? new RMPersonalSettingDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Owner = entity.Owner,
                Type = entity.Type,
                IsDefault = IsDefaultSetting(context, entity.Owner, entity.Type, entity.Id),
                IsBuiltIn = entity.IsBuiltIn,
            }
            : null;
        }

        private List<RMPersonalSettingDto> GetByOwnerAndTypeWithoutContent(string owner, PersonalSettingType type)
        {
            using var context = GetNewContext();
            var result = new List<RMPersonalSettingDto>();
            var entities = context.RMPersonalSetting.Where(o => o.Owner == owner && o.Type == type)
                .Select(o => new { o.Id, o.Name, o.Owner, o.IsBuiltIn, o.Type }).ToList();

            var defaultSetting = GetDefaultSetting(context, owner, type);
            foreach (var entity in entities)
            {
                result.Add(new RMPersonalSettingDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Owner = entity.Owner,
                    Type = entity.Type,
                    IsDefault = defaultSetting?.SettingId == entity.Id,
                    IsBuiltIn = entity.IsBuiltIn,
                });
            }

            return result;
        }

        private List<RMPersonalSettingDto> GetByOwnerAndTypeWithContent(string owner, PersonalSettingType type)
        {
            using var context = GetNewContext();
            var result = new List<RMPersonalSettingDto>();
            var entities = context.RMPersonalSetting.Where(o => o.Owner == owner && o.Type == type)
                .Select(o => new { o.Id, o.Name, o.Owner, o.IsBuiltIn, o.Type, o.ContentStr }).ToList();

            var defaultSetting = GetDefaultSetting(context, owner, type);
            foreach (var entity in entities)
            {
                result.Add(new RMPersonalSettingDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Owner = entity.Owner,
                    Type = entity.Type,
                    IsDefault = defaultSetting?.SettingId == entity.Id,
                    IsBuiltIn = entity.IsBuiltIn,
                    ContentStr = entity.ContentStr
                });
            }

            return result;
        }

        public List<RMPersonalSettingDto> GetByOwnerAndType(string owner, PersonalSettingType type, bool includeContent = false)
        {
            if (!includeContent) return GetByOwnerAndTypeWithoutContent(owner, type);
            using var context = GetNewContext();
            var entities = context.RMPersonalSetting
                .Where(o => o.Owner == owner && o.Type == type).ToList();
            var dtos = entities.Select(o => o.Convert2Dto(includeContent)).ToList();
            return dtos;
        }

        private IQueryable<int> GetSharedSettingIds(RMDbContext dbContext, string userId)
        {
            var userGroupIds = dbContext.LnkUserGroup.Where(o => o.UserId == userId).Select(o => o.GroupId).Distinct();
            var securityGroupIds = dbContext.RMSecurityGroupMembership.Where(o => o.UserId == userId || userGroupIds.Contains(o.UserId)).Select(o => o.GroupId).Distinct();
            return dbContext.RMPersonalSettingShareMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupOrUserId)).Select(o => o.SettingId).Distinct();
        }

        public List<RMPersonalSettingDto> GetSharedSettings(string userId, PersonalSettingType type)
        {
			var result = new List<RMPersonalSettingDto>();
            using var context = GetNewContext();
			var settingIds = GetSharedSettingIds(context, userId);
			var entities = context.RMPersonalSetting.Where(o => settingIds.Contains(o.Id) && o.Type == type && o.Owner != userId)
				.Select(o => new { o.Id, o.Name, o.Owner, o.Type }).ToList();
			var defaultSetting = GetDefaultSetting(context,userId, type);
			foreach (var entity in entities)
			{
				result.Add(new RMPersonalSettingDto
				{
					Id = entity.Id,
					Name = entity.Name,
					Owner = entity.Owner,
					Type = entity.Type,
					IsDefault = entity.Id == defaultSetting?.SettingId
				});
			}
			
			return result;
        }

        public bool IsSharedToUser(string userId, int settingId)
        {
			using var context = GetNewContext();
			var settingIds = GetSharedSettingIds(context, userId);
            return settingIds.Contains(settingId);
        }

        public bool SetAsDefault(int settingId, string userId)
        {
            using var context = GetNewContext();
            var entity = context.RMPersonalSetting.Find(settingId);
            if (entity == null) return false;

            SetAsDefault(context, userId, settingId, entity.Type);

            context.SaveChanges();
            return true;
        }

        private void SetAsDefault(RMDbContext context, string userId, int settingId, PersonalSettingType type)
        {
            var defaultSetting = GetDefaultSetting(context,userId, type);
            if (defaultSetting == null)
            {
                defaultSetting = new RMDefaultPersonalSetting { SettingId = settingId, Type = type, UserId = userId };
                context.RMDefaultPersonalSetting.Add(defaultSetting);
            }
            else if (defaultSetting.SettingId != settingId)
            {
                defaultSetting.SettingId = settingId;
            }
        }

        public bool ExistsBuiltIn(string owner, PersonalSettingType type)
        {
            return base.Exist(o => o.IsBuiltIn && o.Owner == owner && o.Type == type);
        }

        public bool ExistsDefault(string owner, PersonalSettingType type)
        {
            //return base.Exist(o => o.IsDefault && o.Owner == owner && o.Type == type);
            var context = GetNewContext();
            return GetDefaultSetting(context,owner, type) != null;
        }

        public void SetBuiltInAsDefault(string owner, PersonalSettingType type)
        {
            using var context = GetNewContext();
            var builtEntity = base.Find(o => o.Owner == owner && o.Type == type && o.IsBuiltIn/* && !o.IsDefault*/);
            if (builtEntity != null)
            {
                //builtEntity.IsDefault = true;
                //base.Update(builtEntity);
                SetAsDefault(context,owner, builtEntity.Id, type);
                context.SaveChanges();
            }
        }

        private RMDefaultPersonalSetting GetDefaultSetting(RMDbContext context, string userId, PersonalSettingType type)
        {
            return context.RMDefaultPersonalSetting.FirstOrDefault(o => o.UserId == userId && o.Type == type);
        }

        private bool IsDefaultSetting(RMDbContext context, string userId, PersonalSettingType type, int settingId)
        {
            var defaultSetting = GetDefaultSetting(context, userId, type);
            return settingId == defaultSetting?.SettingId;
        }

        private void DeleteSecurityGroupMapping(int id)
        {
            using var context = GetNewContext();
            var entities = context.RMPersonalSettingShareMapping.Where(o => o.SettingId == id);
            context.RMPersonalSettingShareMapping.RemoveRange(entities);
            context.SaveChanges();
        }

        public void Share(int id, List<int> securityGroups)
        {
            using var context = GetNewContext();
            using (DbContextTransaction tran = context.Database.BeginTransaction())
            {
                DeleteSecurityGroupMapping(id);
                foreach (var groupId in securityGroups)
                {
                    context.RMPersonalSettingShareMapping.Add(new RMPersonalSettingShareMapping { SettingId = id, SecurityGroupOrUserId = groupId });
                }
                context.SaveChanges();
                tran.Commit();
            }
        }

        public List<int> GetSharedGroups(int id)
        {
            using var context = GetNewContext();
            return context.RMPersonalSettingShareMapping.AsNoTracking().Where(o => o.SettingId == id).Select(o => o.SecurityGroupOrUserId).ToList();
        }

        public void CancelShare(int id)
        {
            using var context = GetNewContext();
            DeleteSecurityGroupMapping(id);
            context.SaveChanges();
        }

        public async Task<bool> SetAsDefaultForGoogleOne(int settingId, string userId)
        {
            using var context = GetNewContext();
            var entity =  await context.RMPersonalSetting.FindAsync(settingId);
            if (entity == null) return false;

            SetAsDefault(context, userId, settingId, entity.Type);

            context.SaveChanges();
            return true;
        }

        public List<RMPersonalSettingDto> GetByOwnerAndTypeForGoogleOne(string owner, PersonalSettingType type)
        {
            return GetByOwnerAndTypeWithContent(owner, type);
        }
    }
}
