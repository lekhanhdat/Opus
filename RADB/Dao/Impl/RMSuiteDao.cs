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
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSuiteDao : BaseDao<RMSuite>, IRMSuiteDao
    {
        public bool CreateSuite(SuiteDto dto)
        {
            using (var ctx = this.GetNewContext())
            {
                var userId = TenantLocalValue.LogonUserId;
                var user = ctx.Account.Where(a => a.UserId == userId).FirstOrDefault();
                var suiteId = dto.UniqueId != Guid.Empty? dto.UniqueId: Guid.NewGuid();
                using (DbContextTransaction tran = ctx.Database.BeginTransaction())
                {
                    ctx.Suite.Add(new RMSuite()
                    {
                        UniqueId = suiteId,
                        Name = dto.Name,
                        Description = dto.Description,
                        StartFromType = dto.StartFromType,
                        RootTemplateCreateType = dto.RootTemplateCreateType,
                        Creater = user != null ? user.Id : 1,
                        Modifier = user != null ? user.Id : 1,
                        CreatedOn = DateTime.UtcNow,
                        LastModifiedOn = DateTime.UtcNow,
                    });

                    if (dto.RootTemplateCreateType != SuiteRootTemplateCreateType.New && dto.RootTemplateUniqueId != Guid.Empty) {
                        if (IsUsedAsStartTemplate(ctx, dto.RootTemplateUniqueId))
                        {
                            NotifyInvalidSuiteInfo();
                        }
                    }
                    AddRelationship(ctx, suiteId, dto.RootTemplateUniqueId, dto.StartFromType);
                    tran.Commit();
                }
                return ctx.SaveChanges() > 0;
            }
        }

        public bool UpdateSuite(SuiteDto dto)
        {
            using (var ctx = this.GetNewContext())
            {
                using (DbContextTransaction tran = ctx.Database.BeginTransaction())
                {
                    var userId = TenantLocalValue.LogonUserId;
                    var user = ctx.Account.Where(a => a.UserId == userId).First();
                    var entity = ctx.Suite.Where(s => s.UniqueId == dto.UniqueId).FirstOrDefault();
                    if (entity != null)
                    {
                        entity.Name = dto.Name;
                        entity.Description = dto.Description;
                        entity.Modifier = user.Id;
                        entity.LastModifiedOn = DateTime.UtcNow;

                        var isExistStartTemplate4Suite = ExistsStartTemplate4Suite(ctx, dto.UniqueId);
                        //已经Add Root template的suite不可以修改start from type
                        if (isExistStartTemplate4Suite && entity.StartFromType != dto.StartFromType) return false;

                        entity.StartFromType = dto.StartFromType;
                        if (!isExistStartTemplate4Suite && dto.RootTemplateCreateType != SuiteRootTemplateCreateType.New && dto.RootTemplateUniqueId != Guid.Empty)
                        {
                            if (IsUsedAsStartTemplate(ctx, dto.RootTemplateUniqueId))
                            {
                                NotifyInvalidSuiteInfo();
                            }
                        }
                        AddRelationship(ctx, dto.UniqueId, dto.RootTemplateUniqueId, dto.StartFromType);
                        #region
                        //var membership = ctx.SuiteMembership.Where(t => t.SuiteUniqueId == dto.UniqueId).FirstOrDefault();
                        //if (membership != null) {
                        //    //已经Add Root template的suite不可以修改start from type
                        //    if (membership.RootTemplateUniqueId != Guid.Empty && entity.StartFromType != dto.StartFromType) {
                        //        return false;
                        //    }
                        //}
                        //if (membership == null && dto.RootTemplateCreateType != SuiteRootTemplateCreateType.New && dto.RootTemplateUniqueId != Guid.Empty)
                        //{
                        //    entity.RootTemplateCreateType = dto.RootTemplateCreateType;
                        //    var othderMembership = ctx.SuiteMembership.Where(t => t.RootTemplateUniqueId == dto.RootTemplateUniqueId && t.SuiteUniqueId != dto.UniqueId).FirstOrDefault();
                        //    if (othderMembership != null) {
                        //        NotifyInvalidSuiteInfo();
                        //    }
                        //    ctx.SuiteMembership.Add(new RMSuiteMembership()
                        //    {
                        //        SuiteUniqueId = dto.UniqueId,
                        //        RootTemplateUniqueId = dto.RootTemplateUniqueId
                        //    });
                        //}
                        #endregion
                        tran.Commit();
                    }
                    return ctx.SaveChanges() > 0;
                }
            }
        }

        /// <summary>
        /// check if the template is used as start template in a suite
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        private bool IsUsedAsStartTemplate(RMDbContext ctx, Guid templateId)
        {
            var ancestorIds = ctx.TemplateRelationship.AsNoTracking().Where(o => o.Descendant == templateId && o.Distance == 1)
                .Select(o => o.Ancestor).ToList();
            if (ancestorIds.Count == 0) return false;
                return ctx.TemplateRelationship.Any(o => ancestorIds.Contains(o.Ancestor) && o.TemplateType == TemplateType.Suite);

        }

        /// <summary>
        /// check if the suite has a start template
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="suiteId"></param>
        /// <returns></returns>
        private bool ExistsStartTemplate4Suite(RMDbContext ctx, Guid suiteId)
        {
            return ctx.TemplateRelationship.Any(o => o.Ancestor == suiteId && o.Distance == 1);
        }

        private void AddRelationship(RMDbContext ctx, Guid suiteUniqueId, Guid templateUniqueId, SuiteStartFromType suiteStartFromType)
        {
            var suiteIdPath = suiteUniqueId.ToString() + TemplateUtil.IdPathSeprator;
            if (!ctx.TemplateRelationship.Any(o => o.IdPath == suiteIdPath && o.Distance == 0))
            {
                ctx.TemplateRelationship.Add(new RMTemplateRelationship
                {
                    IdPath = suiteIdPath,
                    Ancestor = suiteUniqueId,
                    Descendant = suiteUniqueId,
                    Distance = 0,
                    TemplateType = TemplateType.Suite
                });
            }

            if (templateUniqueId == Guid.Empty) return;

            var templateId = ctx.Template.First(o => o.UniqueId == templateUniqueId).Id;
            var idPath = TemplateUtil.Convert2Path(new List<string> { suiteUniqueId.ToString(), templateId.ToString()});

            if (!ctx.TemplateRelationship.Any(o => o.IdPath == idPath && o.Distance == 1))
            {
                ctx.TemplateRelationship.Add(new RMTemplateRelationship
                {
                    IdPath = idPath,
                    Ancestor = suiteUniqueId,
                    Descendant = templateUniqueId,
                    Distance = 1,
                    TemplateType = suiteStartFromType == SuiteStartFromType.Custom ? TemplateType.Custom : suiteStartFromType == SuiteStartFromType.Box ? TemplateType.Box : TemplateType.Folder
                });
            }
        }

        public List<Guid> GetSuiteIdsByLocationID(Guid location)
        {
            using (var ctx = this.GetNewContext())
            {
                return ctx.RMLocationSuiteAssociation.Where(lo => lo.LocationUniqueId == location).Select(s => s.SuiteUniqueId).ToList();

                //var assoiation = ctx.RMLocationSuiteAssociation.FirstOrDefault(lo => lo.LocationUniqueId == location);
                //if (assoiation == null)
                //{
                //    return null;
                //}
                //return ctx.Suite.Where(s => s.UniqueId == assoiation.SuiteUniqueId).Select(s => s.UniqueId).ToList();
            }
        }

        public List<RMSuite> GetAllSuite(SuiteTemplateQueryDto queryDto, out int totalCount)
        {
            using (var ctx = this.GetNewContext())
            {
                var pageIndex = queryDto.PagingInfo.PageIndex;
                var pageSize = queryDto.PagingInfo.PageSize;
                var searchValue = queryDto.SearchValue;
                if (string.IsNullOrEmpty(searchValue))
                {
                    //default data name is key in Db eg:RM_Template_Default_Box_Suite_Name/RM_Template_Default_Folder_Suite_Name
                    totalCount = ctx.Suite.Count();
                    return ctx.Suite.OrderBy(s => s.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    totalCount = ctx.Suite.Where(s => s.Name.Contains(searchValue)).Count();
                    return ctx.Suite.Where(s => s.Name.Contains(searchValue)).OrderBy(s => s.Name).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
            }
        }

        public RMSuite GetSuiteByName(string name)
        {
            using (var ctx = this.GetNewContext())
            {
                return ctx.Suite.FirstOrDefault(a => a.Name == name);
            }
        }

        private void NotifyInvalidSuiteInfo()
        {
            throw new Exception("Please check whether or not the suite info are correct.");
        }
    }
}
