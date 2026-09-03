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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermSetDao : BaseDao<RMTermSet>, ITermSetDao
    {
        private RALogger Logger = RALogger.GetInstance(typeof(TermSetDao));
        public ITermDao TermDao { get; set; }
        public IRMSecurityGroupDao SecurityGroupDao { get; set; }
        public async Task<List<RMTermSet>> LoadTermSetAsync(TermSetType termSetType, Guid parentTermGroupId, FilterTermObjOption filterOption = null)
        {
            if (!parentTermGroupId.Equals(Guid.Empty))
            {
                await UpdateTermSetByGroupIdAsync(parentTermGroupId);
            }
            List<RMTermSet> termSets = new List<RMTermSet>();
            using var context = GetNewContext();
            if (termSetType.Equals(TermSetType.Business))
            {
                if (filterOption != null && filterOption.NeedCheckPermission)
                {
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        UserAndGroupIds = filterOption.userAndGroupUserIds,
                        Level = SecurityTermLevel.TermSet,
                        ParentId = parentTermGroupId,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag
                    };
                    SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                    if (result.TermPermissionType == TermPermissionMethod.All)
                    {
                        termSets = context.TermSets.Where(ts => !ts.IsRemoved && ts.TermGroupId.ToString().Equals(parentTermGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && ((int)ts.TermSetType == (int)termSetType || (int)ts.TermSetType == (int)TermSetType.BusinessTerm)).ToList();
                    }
                    else
                    {
                        List<Guid> termsetUniqueIds = result.TermObjIds;
                        if (termsetUniqueIds != null)
                        {
                            termSets = context.TermSets.Where(ts => !ts.IsRemoved && termsetUniqueIds.Contains(ts.UniqueId) && ts.TermGroupId.ToString().Equals(parentTermGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && ((int)ts.TermSetType == (int)termSetType || (int)ts.TermSetType == (int)TermSetType.BusinessTerm)).ToList();
                        }
                    }
                }
                else
                {
                    termSets = context.TermSets.Where(ts => !ts.IsRemoved && ts.TermGroupId.ToString().Equals(parentTermGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && ((int)ts.TermSetType == (int)termSetType || (int)ts.TermSetType == (int)TermSetType.BusinessTerm)).ToList();
                }
                foreach (var termSet in termSets)
                {
                    List<RMTerm> terms = TermDao.GetTermFromTermSet(termSet.Id);
                    termSet.subTerms = terms;
                    termSet.subTermCount = terms.Count;
                }
            }
            else
            {
                if (parentTermGroupId.Equals(Guid.Empty))
                {
                    termSets = context.TermSets.Where(ts => !ts.IsRemoved && (int)ts.TermSetType == (int)termSetType).ToList();
                }
                else
                {
                    termSets = context.TermSets.Where(ts => !ts.IsRemoved && (int)ts.TermSetType == (int)termSetType && ts.TermGroupId.ToString().Equals(parentTermGroupId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            for (int index = 0; index < termSets.Count; index++)
            {
                int termSetId = termSets[index].Id;
                //干什么用的？
                termSets[index].subTermCount = TermDao.SubTermCountByTermSetId(termSetId);
            }

            return termSets;
        }

        public List<RMTermSet> GetTermSetsByGroupId(Guid termGroupId, TermSetType type, int pageIndex, int pageSize, FilterTermObjOption filterOption = null)
        {
            var termSets = new List<RMTermSet>();
            using (var context = GetNewContext())
            {
                var needCheckPermission = filterOption != null ? filterOption.NeedCheckPermission : false;
                var termSetPermissionResult = new SecurityTermPermissionDto { TermPermissionType = TermPermissionMethod.All };
                if (needCheckPermission)
                {
                    termSetPermissionResult = SecurityGroupDao.GetSecurityTermObjInfo(new QuerySecurityTermObjDto
                    {
                        UserAndGroupIds = filterOption.userAndGroupUserIds,
                        Level = SecurityTermLevel.TermSet,
                        ParentId = termGroupId,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag
                    });
                }
                if (termSetPermissionResult.TermPermissionType != TermPermissionMethod.None)
                {
                    var hasPermissionTermSetIds = termSetPermissionResult.TermObjIds;
                    if (hasPermissionTermSetIds != null && termSetPermissionResult.TermPermissionType == TermPermissionMethod.SpecifyScope)
                    {
                        termSets = context.TermSets.Where(o => !o.IsRemoved && o.TermGroupId.Equals(termGroupId) && (o.TermSetType == type || o.TermSetType == TermSetType.BusinessTerm) && hasPermissionTermSetIds.Contains(o.UniqueId)).OrderBy(o => o.Name).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        termSets = context.TermSets.Where(o => !o.IsRemoved && o.TermGroupId.Equals(termGroupId) && (o.TermSetType == type || o.TermSetType == TermSetType.BusinessTerm)).OrderBy(o => o.Name).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    }
                }
                foreach (var termSet in termSets)
                {
                    List<RMTerm> terms = TermDao.GetTermFromTermSet(termSet.Id);
                    termSet.subTerms = terms;
                    termSet.subTermCount = terms.Count;
                }
                return termSets;
            }
        }

        public async Task<List<RMTermSet>> LoadTermSetWithDeletedItemsAsync(TermSetType termSetType, Guid parentTermGroupId)
        {
            if (!parentTermGroupId.Equals(Guid.Empty))
            {
                await UpdateTermSetByGroupIdAsync(parentTermGroupId);
            }
            List<RMTermSet> termSets = new List<RMTermSet>();
            using var context = GetNewContext();
            if (termSetType.Equals(TermSetType.Business))
            {
                termSets = context.TermSets.Where(ts => ts.TermGroupId.ToString().Equals(parentTermGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && ((int)ts.TermSetType == (int)termSetType || (int)ts.TermSetType == (int)TermSetType.BusinessTerm)).ToList();
                foreach (var termSet in termSets)
                {
                    List<RMTerm> terms = TermDao.GetTermFromTermSet(termSet.Id);
                    termSet.subTerms = terms;
                    termSet.subTermCount = terms.Count;
                }
            }
            else
            {
                if (parentTermGroupId.Equals(Guid.Empty))
                {
                    termSets = context.TermSets.Where(ts => (int)ts.TermSetType == (int)termSetType).ToList();
                }
                else
                {
                    termSets = context.TermSets.Where(ts => (int)ts.TermSetType == (int)termSetType && ts.TermGroupId.ToString().Equals(parentTermGroupId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            for (int index = 0; index < termSets.Count; index++)
            {
                int termSetId = termSets[index].Id;
                //干什么用的？
                termSets[index].subTermCount = TermDao.SubTermCountByTermSetId(termSetId);
            }

            return termSets;
        }

        public RMTermSet CreateTermSet(string termSetName, Guid termGroupId, string desc)
        {
            //if (HasExistsTermSet(termGroupId))
            //{
            //    throw new Exception("Has Exists Term Set");
            //}
            if (HasSameNameTermSet(termSetName, termGroupId))
            {
                throw new Exception("Term Set has same name");
            }
            RMTermSet result = null;
            using var context = GetNewContext();
            RMTermSet termSet = new RMTermSet() { Name = termSetName, Description = desc, UniqueId = Guid.NewGuid(), TermGroupId = termGroupId };
            result = context.TermSets.Add(termSet);
            context.SaveChanges();
            return result;
        }

        public async Task<RMTermSet> CreateGoogleTermSet(string termSetName, Guid termGroupId)
        {
            if (HasSameGoogleNameTermSet(termSetName, termGroupId))
            {
                throw new Exception("Google Term Set has same name");
            }
            using var context = GetNewContext();
            RMTermSet termSet = new()
            {
                Name = termSetName,
                UniqueId = Guid.NewGuid(),
                TermGroupId = termGroupId,
                Description = "",
                IsGoogle = true
            };
            var result = context.TermSets.Add(termSet);
            await context.SaveChangesAsync();
            return result;
        }

        public void CreateTermSetByUniqueId(Guid termSetId, string termSetName, string description, Guid termGroupId)
        {
            using var context = GetNewContext();
            RMTermSet termSet = new RMTermSet() { Name = termSetName, Description = description, UniqueId = termSetId, TermGroupId = termGroupId };
            context.TermSets.Add(termSet);
            context.SaveChanges();
        }

        /// <summary>
        /// add termset info
        /// </summary>
        public RMTermSet AddTermSetInfo(string termSetName, string termDescription)
        {
            using var context = GetNewContext();
            if (context.TermSets.AsQueryable().Where(ts => ts.Name.Equals(termSetName)).FirstOrDefault() == null)
            {
                RMTermSet returnTermSet = new RMTermSet() { Name = termSetName, Description = termDescription, UniqueId = Guid.NewGuid() };
                context.TermSets.Add(returnTermSet);
                context.SaveChanges();
                return returnTermSet;
            }
            return null;
        }

        public RMTermSet GetRMTermSetByGuid(Guid termSetId)
        {
            RMTermSet result = null;
            using (var ctx = GetNewContext())
            {
                result = ctx.TermSets.AsQueryable().Where(ts => ts.UniqueId.Equals(termSetId)).OrderByDescending(ts => ts.Id).FirstOrDefault();
            }
            return result;
        }

        public List<RMTermSet> GetTermSetsByGroupUniqueIdsAndIds(IEnumerable<Guid> groupIds, IEnumerable<int> termSetIds)
        {
            using (var context = GetNewContext())
            {
                return context.TermSets.Where(item => !item.IsRemoved && groupIds.Contains(item.TermGroupId) && termSetIds.Contains(item.Id)).ToList();
            }
        }

        public List<RMTermSet> GetRMTermSetsByGroupUniqueId(Guid groupId, FilterTermObjOption filterOption = null)
        {
            using var context = GetNewContext();
            var result = new List<RMTermSet>();
            var needCheckPermission = filterOption != null ? filterOption.NeedCheckPermission : false;
            var termSetPermissionResult = new SecurityTermPermissionDto { TermPermissionType = TermPermissionMethod.All };
            if (needCheckPermission)
            {
                termSetPermissionResult = SecurityGroupDao.GetSecurityTermObjInfo(new QuerySecurityTermObjDto
                {
                    UserAndGroupIds = filterOption.userAndGroupUserIds,
                    Level = SecurityTermLevel.TermSet,
                    ParentId = groupId,
                    FilterByContentSource = filterOption.NeedCheckPermission,
                    ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                    ContainerId = filterOption.ContainerId,
                    SourceFlag = filterOption.SourceFlag
                });
            }
            if (termSetPermissionResult.TermPermissionType != TermPermissionMethod.None)
            {
                var hasPermissionTermSetIds = termSetPermissionResult.TermObjIds;
                if (hasPermissionTermSetIds != null && hasPermissionTermSetIds.Count > 0)
                {
                    result = context.TermSets.AsQueryable().Where(ts => ts.IsRemoved == false && (ts.TermSetType == TermSetType.Business || ts.TermSetType == TermSetType.BusinessTerm) && ts.TermGroupId.Equals(groupId) && hasPermissionTermSetIds.Contains(ts.UniqueId)).ToList();
                }
                else
                {
                    result = context.TermSets.AsQueryable().Where(ts => ts.IsRemoved == false && (ts.TermSetType == TermSetType.Business || ts.TermSetType == TermSetType.BusinessTerm) && ts.TermGroupId.Equals(groupId)).ToList();
                }
            }
            return result;
        }

        public List<RMTermSet> GetRMTermSetsByGroupUniqueIdAndTermSetName(Guid groupId, string termsetName)
        {
            using var context = GetNewContext();
            List<RMTermSet> result = context.TermSets.AsQueryable().Where(ts => ts.IsRemoved == false && ts.TermGroupId.Equals(groupId) && ts.Name.Equals(termsetName)).ToList();
            return result;
        }

        public RMTermSet GetGoogleTermSetByGroupUniqueId(Guid groupId)
        {
            using var context = GetNewContext();
            return context.TermSets.FirstOrDefault(ts => ts.IsRemoved == false && ts.TermGroupId.Equals(groupId) && ts.IsGoogle);
        }

        public void DeleteAllTermSet()
        {
            using var context = GetNewContext();
            var oldTermSet = context.TermSets.ToList();
            if (oldTermSet.Count > 0)
            {
                context.TermSets.RemoveRange(oldTermSet);
                context.SaveChanges();
            }
        }

        public RMTermSet GetRMTermSet(int termSetId)
        {
            using var context = GetNewContext();
            int subTermCount = context.TermSetMemberships.AsQueryable().Where(t => !t.IsRemoved && t.TermSetId.Equals(termSetId) && t.ParentTermId.Equals(0)).ToList().Count;
            RMTermSet result = context.TermSets.AsQueryable().Where(ts => ts.Id.Equals(termSetId)).First();
            result.subTermCount = subTermCount;
            return result;
        }

        public async Task<RMTermSet> UpdateTermSetAsync(int termSetId, string termSetName, string description)
        {
            using var context = GetNewContext();
            int subTermCount = context.TermSetMemberships.AsQueryable().Where(t => t.TermSetId.Equals(termSetId) && t.ParentTermId.Equals(0)).ToList().Count;
            RMTermSet result = context.TermSets.AsQueryable().Where(ts => ts.Id.Equals(termSetId)).First();
            result.subTermCount = subTermCount;
            result.Name = termSetName;
            result.Description = description;
            result.IsRemoved = false;
            await this.UpdateAsync(result);
            return result;
        }

        public RMTermSet GetTermSetByName(string name)
        {
            using (var context = RMDBContextManager.GetNewDBContext())
            {
                RMTermSet result = context.TermSets.AsQueryable().Where(ts => ts.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (result != null)
                {
                    result.subTermCount = TermDao.SubTermCountByTermSetId(result.Id);
                }
                return result;
            }
        }

        public async Task UpdateGroupIdOfTermSetAsync(int termSetId, Guid termGroupId)
        {
            using var context = GetNewContext();
            var termSet = context.TermSets.Where(t => t.Id == termSetId).FirstOrDefault();
            if (termSet != null)
            {
                termSet.TermGroupId = termGroupId;
                await this.UpdateAsync(termSet);
            }
        }

        public async Task UpdateTermSetByGroupIdAsync(Guid termGroupId)
        {
            using var context = GetNewContext();
            var termSet = context.TermSets.Where(t => t.TermGroupId.Equals(Guid.Empty)).FirstOrDefault();
            if (termSet != null)
            {
                termSet.TermGroupId = termGroupId;
                await this.UpdateAsync(termSet);
            }
        }

        public bool HasSameNameTermSet(string termSetName, Guid termGroupId)
        {
            using var context = GetNewContext();
            var termSet = context.TermSets.AsQueryable().Where(t => !t.IsRemoved && t.Name.Equals(termSetName) && t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && (int)t.TermSetType == (int)TermSetType.BusinessTerm).FirstOrDefault();
            if (termSet != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasSameGoogleNameTermSet(string termSetName, Guid termGroupId)
        {
            using var context = GetNewContext();
            var termSet = context.TermSets.FirstOrDefault(t => !t.IsRemoved && t.Name.Equals(termSetName) && t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && (int)t.TermSetType == (int)TermSetType.BusinessTerm);
            return termSet != null;
        }

        public bool ReNameHasSameNameTermSet(int termSetId, string termSetName, Guid termGroupId)
        {
            bool hasSame = false;
            try
            {
                using var context = GetNewContext();
                var termSets = context.TermSets.AsQueryable().Where(t => !t.Id.Equals(termSetId));
                if (termSets != null && termSets.Count() > 0)
                {
                    List<int> termSetIds = termSets.Select(t => t.Id).ToList();
                    if (context.TermSets.AsQueryable().Where(t => termSetIds.Contains(t.Id) && t.Name.Equals(termSetName) && t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && (int)t.TermSetType == (int)TermSetType.BusinessTerm).FirstOrDefault() != null)
                    {
                        hasSame = true;
                    }
                }
            }
            catch
            {
                hasSame = false;
            }
            return hasSame;
        }

        public async Task<RMTermSet> RenameTermSetAsync(int termSetId, Guid termGroupId, string termSetName)
        {
            using var context = GetNewContext();
            if (ReNameHasSameNameTermSet(termSetId, termSetName, termGroupId))
            {
                throw new Exception("Term Set has same name");
            }
            RMTermSet termSet = context.TermSets.AsQueryable().Where(t => t.Id == termSetId).FirstOrDefault();
            termSet.Name = termSetName;
            await this.UpdateAsync(termSet);
            return termSet;
        }

        public bool HasExistsTermSet(Guid termGroupId)
        {
            using var context = GetNewContext();
            var isExistsTermSet = false;
            var termSets = context.TermSets.AsQueryable().Where(t => t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && (int)t.TermSetType == (int)TermSetType.BusinessTerm).ToList();
            if (termSets != null && termSets.Count > 0)
            {
                isExistsTermSet = true;
            }
            else
            {
                isExistsTermSet = false;
            }
            return isExistsTermSet;
        }

        public bool HasOtherTermSet(Guid termGroupId, Guid termSetId)
        {
            using var context = GetNewContext();
            var isExistsTermSet = false;
            var termSets = context.TermSets.AsQueryable().Where(t => t.IsRemoved == false && t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && !t.UniqueId.ToString().Equals(termSetId.ToString(), StringComparison.OrdinalIgnoreCase) && (int)t.TermSetType == (int)TermSetType.BusinessTerm).ToList();
            if (termSets != null && termSets.Count > 0)
            {
                isExistsTermSet = true;
            }
            else
            {
                isExistsTermSet = false;
            }
            return isExistsTermSet;
        }

        public async Task DeleteTermSetAsync(int termSetId)
        {
            using var context = GetNewContext();
            var termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
            if (termSet != null)
            {
                termSet.IsRemoved = true;
                await this.UpdateAsync(termSet);
            }
            context.SaveChanges();
        }

        public List<RMTermSet> GetTermSets(Guid termGroupId, PagerInfo pager, out int totalCount)
        {
            using (var context = GetNewContext())
            {
                totalCount = context.TermSets.Where(o => !o.IsRemoved && o.TermGroupId.Equals(termGroupId) && (o.TermSetType == TermSetType.Business || o.TermSetType == TermSetType.BusinessTerm)).Count();
                var termSets = context.TermSets.Where(o => !o.IsRemoved && o.TermGroupId.Equals(termGroupId) && (o.TermSetType == TermSetType.Business || o.TermSetType == TermSetType.BusinessTerm)).OrderBy(o => o.Name).Skip(pager.PagerIndex * pager.PagerSize).Take(pager.PagerSize).ToList();
                return termSets;
            }
        }

        public List<RMTermSet> LoadTermSetNodes(Guid parentTermGroupId)
        {
            using (var context = GetNewContext())
            {
                return context.TermSets.Where(ts => ts.TermSetType == TermSetType.BusinessTerm && ts.TermGroupId == parentTermGroupId).ToList();
            }
        }

        public RMTermSet GetFirstTermSetByTermGroupId(Guid parentTermGroupId)
        {
            using var context = GetNewContext();
            return context.TermSets.FirstOrDefault(ts => ts.TermGroupId == parentTermGroupId && ts.IsGoogle && !ts.IsRemoved);
        }

        public async Task<IEnumerable<RMTermSet>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.TermSets.AsNoTracking().OrderBy(ts => ts.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertTermSetTableAsync(IEnumerable<RMTermSet> termSets)
        {
            using var context = GetNewContext();
            string tableName = "RMTermSets";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, UniqueId, Name, Description, TermGroupId, IsRemoved, TermSetType, IsGoogle) VALUES ");
                int i = 0;
                foreach (var item in termSets)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.UniqueId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", item.Name));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", (object)item.Description ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", item.TermGroupId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", item.IsRemoved));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", (int)item.TermSetType));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", item.IsGoogle));
                    paramIndex += 8;
                    i++;
                }

                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray()); ;
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert Term Set data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllTermSetAsync()
        {
            return await TruncateAllDataInTableAsync("RMTermSets");
        }
        public async Task<List<RMTermSet>> GetTermSetsByTermSetIds(List<Guid> termSetIds)
        {
            using var context = GetNewContext();
            return await context.TermSets.AsQueryable().Where(s => termSetIds.Contains(s.UniqueId)).ToListAsync();
        }
    }

}
