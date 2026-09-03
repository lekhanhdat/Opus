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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static AvePoint.RA.DB.Dao.GoogleSyncNodeDao.RMGoogleRemoteNodeDao;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermGroupDao : BaseDao<RMTermGroup>, ITermGroupDao
    {
        private RALogger Logger = RALogger.GetInstance(typeof(TermGroupDao));
        public ITermDao TermDao { get; set; }
        public ITermSetDao TermSetDao { get; set; }
        private IRMGoogleRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();
        private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();

        public IRMSecurityGroupDao SecurityGroupDao { get; set; }
        public void AddTermGroupInfo(string groupName, string groupDescription)
        {
            using (var context = GetNewContext())
            {
                if (context.TermGruops.AsQueryable().Where(ts => ts.Name.Equals(groupName)).FirstOrDefault() == null)
                {
                    context.TermSets.Add(new RMTermSet() { Name = groupName, Description = groupDescription, UniqueId = Guid.NewGuid() });
                    context.SaveChanges();
                }
            }
        }
        public void CreateTermGroupById(Guid termGroupId, string termGroupName, string description, bool usingMMSSpecified)
        {
            using (var context = GetNewContext())
            {
                context.TermGruops.Add(new RMTermGroup() { Name = termGroupName, Description = description, UniqueId = termGroupId, UsingMMSSpecified = usingMMSSpecified });
                context.SaveChanges();
            }
        }
        public RMTermGroup CreateTermGroupByName(string termGroupName)
        {
            if (HasSameNameTermGroup(termGroupName))
            {
                throw new Exception("TermGroup has same name");
            }
            RMTermGroup result = null;
            using (var context = GetNewContext())
            {
                result = context.TermGruops.Add(new RMTermGroup() { Name = termGroupName, Description = string.Empty, UniqueId = Guid.NewGuid(), UsingMMSSpecified = false });
                context.SaveChanges();
            }
            return result;
        }

        public RMTermGroup GetTermGroupById(int id)
        {
            using (var context = GetNewContext())
            {
                return context.TermGruops.Where(item => !item.IsRemoved && item.Id == id).FirstOrDefault();
            }
        }

        public RMTermGroup GetTermGroupByUniqueIdForGoogleOne(Guid uniqueId)
        {
            using (var context = GetNewContext())
            {
                return context.TermGruops.FirstOrDefault(item => !item.IsRemoved && item.UniqueId == uniqueId);
            }
        }

        public List<RMTermGroup> GetTermGroupsByIds(IEnumerable<int> ids)
        {
            using (var context = GetNewContext())
            {
                return context.TermGruops.AsNoTracking().Where(item => !item.IsRemoved && ids.Contains(item.Id)).ToList();
            }
        }

        public RMTermGroup GetTermGroupByGuid(Guid termGroupId)
        {
            using var context = GetNewContext();
            RMTermGroup result = context.TermGruops.AsNoTracking().Where(tg => tg.UniqueId.Equals(termGroupId)).OrderByDescending(ts => ts.Id).FirstOrDefault();
            return result;
        }

        public RMTermGroup GetTermGroupByName(string termGroupName)
        {
            using var context = GetNewContext();
            RMTermGroup result = context.TermGruops.AsQueryable().Where(tg => tg.IsRemoved == false && tg.Name.Equals(termGroupName)).OrderByDescending(ts => ts.Id).FirstOrDefault();
            return result;
        }

        public void DeleteTermGroup()
        {
            using (var context = GetNewContext())
            {
                var oldTermGroup = context.TermGruops.AsQueryable().ToList();
                if (oldTermGroup.Count > 0)
                {
                    context.TermGruops.RemoveRange(oldTermGroup);
                    context.SaveChanges();
                }
            }
        }
        public RMTermGroup GetRMTermGruop(int termGroupId)
        {
            // int subTermCount = SharedDbContext.TermSetMemberships.AsQueryable().Where(t => t.TermSetId.Equals(termSetId) && t.ParentTermId.Equals(0)).ToList().Count;
            RMTermGroup result = null;
            using (var context = GetNewContext())
            {
                result = context.TermGruops.AsQueryable().Where(ts => ts.Id.Equals(termGroupId)).First();
                result.subTermCount = 1;
            }
            return result;
        }
        /// <summary>
        /// add for sync term job, if need query ,please consider the permission check
        /// </summary>
        /// <returns></returns>
        public List<RMTermGroup> LoadSPTermGroup()
        {
            List<RMTermGroup> termGroups = new List<RMTermGroup>();
            using (var context = GetNewContext())
            {
                termGroups = context.TermGruops.Where(g => g.M365TermSyncOption != TermSyncOption.None && !g.IsRemoved).ToList();
            }
            return termGroups;
        }
        /// <summary>
        /// to do multi group next
        /// </summary>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        public List<RMTermGroup> LoadTermGroup(bool isWithDelGroup = true, FilterTermObjOption filterOption = null)
        {
            List<RMTermGroup> termGroups = new List<RMTermGroup>();
            using (var context = GetNewContext())
            {
                var needCheckPermission = filterOption != null ? filterOption.NeedCheckPermission : false;
                var permissionResult = new SecurityTermPermissionDto { TermPermissionType = TermPermissionMethod.All };
                if (needCheckPermission)
                {
                    permissionResult = SecurityGroupDao.GetSecurityTermObjInfo(new QuerySecurityTermObjDto
                    {
                        UserAndGroupIds = filterOption.userAndGroupUserIds,
                        Level = SecurityTermLevel.TermGroup,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag
                    });
                }

                if (permissionResult.TermPermissionType != TermPermissionMethod.None)
                {
                    var hasPermissionGroupIds = permissionResult.TermObjIds;
                    if (isWithDelGroup)
                    {
                        if (hasPermissionGroupIds != null && hasPermissionGroupIds.Count > 0)
                        {
                            termGroups = context.TermGruops.Where(o => hasPermissionGroupIds.Contains(o.UniqueId)).ToList();
                        }
                        else
                        {
                            termGroups = context.TermGruops.ToList();
                        }
                    }
                    else
                    {
                        if (hasPermissionGroupIds != null && hasPermissionGroupIds.Count > 0)
                        {
                            termGroups = context.TermGruops.Where(o => o.IsRemoved == false && hasPermissionGroupIds.Contains(o.UniqueId)).ToList();
                        }
                        else
                        {
                            termGroups = context.TermGruops.Where(o => o.IsRemoved == false).ToList();
                        }
                    }

                    foreach (var group in termGroups)
                    {
                        group.subTermCount = TermSetDao.GetRMTermSetsByGroupUniqueId(group.UniqueId, filterOption).Count;
                        if (group.M365TermSyncOption is not TermSyncOption.None)
                        {
                            group.M365TermSyncOption = group.UsingMMSSpecified ? TermSyncOption.Specified : TermSyncOption.All;
                        }
                    }
                }
            }
            return termGroups;
        }

        public async Task<RMTermGroup> UpdateTermGroupAsync(int termGroupId, string termGroupName, string description, bool usingMMSSpecified, int m365SyncOption, int googleSyncOption)
        {
            RMTermGroup result = null;
            using (var context = GetNewContext())
            {
                //int subTermCount = context.TermSetMemberships.AsQueryable().Where(t => t.TermSetId.Equals(termSetId) && t.ParentTermId.Equals(0)).ToList().Count;
                result = context.TermGruops.AsQueryable().Where(ts => ts.Id.Equals(termGroupId)).First();
                result.subTermCount = 1;
                result.Name = termGroupName;
                result.Description = description;
                result.UsingMMSSpecified = usingMMSSpecified;
                result.GoogleTermSyncOption = (TermSyncOption)googleSyncOption;
                result.M365TermSyncOption = (TermSyncOption)m365SyncOption;
                await this.UpdateAsync(result);
            }
            return result;
        }

        public async Task UpdateGoogleTermGroupSettingAsync(RMGoogleTermGroupSetting setting)
        {
            using var context = GetNewContext();
            var termGroup = await context.TermGruops.FirstAsync(termGroup => termGroup.UniqueId.ToString().Equals(setting.TermGroupId));
            termGroup.GoogleTermSyncOption = (TermSyncOption)setting.SyncOption;
            var termGroupMembership = await context.TermGroupMembership.Where(membership => membership.TermGroupId.Equals(termGroup.UniqueId)).ToListAsync();
            var needAddMembership = setting.GoogleTenants.Where(googleTenant => !termGroupMembership.Any(membership => membership.SiteUrl.Equals(googleTenant.Key)));
            var termGroupMemberships = needAddMembership.Select(membership => new RMTermGroupMembership
            {
                TermGroupId = termGroup.UniqueId,
                DisplayName = membership.Value,
                SiteUrl = membership.Key,
                AgentGroupId = membership.Key,
                TermStoreName = membership.Value,
                SiteType = SiteType.Google
            }).ToList();
            context.TermGroupMembership.AddRange(termGroupMemberships);
            await context.SaveChangesAsync();
        }

        public async Task<RMTermGroup> UpdateTermGroupUniqueIdAsync(int termGroupId, Guid termGroupUniqueId)
        {
            RMTermGroup result = null;
            using (var context = GetNewContext())
            {
                //int subTermCount = context.TermSetMemberships.AsQueryable().Where(t => t.TermSetId.Equals(termSetId) && t.ParentTermId.Equals(0)).ToList().Count;
                result = context.TermGruops.AsQueryable().Where(ts => ts.Id.Equals(termGroupId)).FirstOrDefault();
                result.UniqueId = termGroupUniqueId;
                await this.UpdateAsync(result);
            }
            return result;
        }

        public async Task<RMTermGroup> UpdateTermGroupAsync(int termGroupId, string termGroupName, string description)
        {
            RMTermGroup result = null;
            using (var context = GetNewContext())
            {
                result = context.TermGruops.AsQueryable().Where(ts => ts.Id.Equals(termGroupId)).First();
                result.subTermCount = 1;
                result.Name = termGroupName;
                result.Description = description;
                result.IsRemoved = false;
                await this.UpdateAsync(result);
            }
            return result;
        }

        public List<Guid> LoadTermGroupUniqueId(int termGroupId)
        {

            List<Guid> result = new List<Guid>();
            using (var context = GetNewContext())
            {
                result = context.TermGruops.AsQueryable().Where(ts => ts.Id.Equals(termGroupId)).Select(r => r.UniqueId).ToList();
            }
            return result;
        }
        public List<RMTermGroup> LoadGroupsData(bool containTerms = true, List<Guid> groupUniqueIds = null, List<string> userAndGroupUserIds = null, FilterTermObjOption filterOption = null, int pageIndex = 0, int pageSize = 0)
        {
            bool needCheckPermission = false;
            int defaulPageSize = 15;
            if (groupUniqueIds != null)
            {
                needCheckPermission = true;
            }
            List<RMTermGroup> termGroups = null;
            using (var context = GetNewContext())
            {
                if (needCheckPermission)
                {
                    termGroups = context.TermGruops.AsQueryable().Where(g => g.IsRemoved == false && groupUniqueIds.Contains(g.UniqueId)).ToList();
                }
                else
                {
                    termGroups = context.TermGruops.AsQueryable().Where(g => g.IsRemoved == false).ToList();
                }
                List<RMTermSet> termSets = new List<RMTermSet>();
                foreach (var termGroup in termGroups)
                {
                    if (needCheckPermission)
                    {
                        QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                        {
                            UserAndGroupIds = userAndGroupUserIds,
                            Level = SecurityTermLevel.TermSet,
                            ParentId = termGroup.UniqueId
                        };
                        if (filterOption != null)
                        {
                            dto.FilterByContentSource = filterOption.NeedCheckPermission;
                            dto.ExcludeBuiltIn = filterOption.ExcludeBuiltIn;
                            dto.ContainerId = filterOption.ContainerId;
                            dto.SourceFlag = filterOption.SourceFlag;
                            dto.ForPhysicalView = filterOption.ForPhysicalView;
                        }
                        SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                        if (result.TermPermissionType == TermPermissionMethod.All)
                        {
                            termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                        }
                        else
                        {
                            List<Guid> termsetUniqueIds = result.TermObjIds;
                            if (termsetUniqueIds != null)
                            {
                                termSets = context.TermSets.Where(t => !t.IsRemoved && termsetUniqueIds.Contains(t.UniqueId) && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                            }
                        }
                    }
                    else
                    {
                        termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    foreach (var termSet in termSets)
                    {
                        if (containTerms)
                        {
                            List<RMTerm> terms = new();
                            if (pageIndex >= 0)
                            {
                                terms = TermDao.GetTermFromTermSet(termSet.Id, pageIndex, pageSize > 0 ? pageSize : defaulPageSize);
                            }
                            else
                            {
                                terms = TermDao.GetTermFromTermSet(termSet.Id);
                            }
                            termSet.subTerms = terms;
                            termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
                        }
                        else
                        {
                            termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
                        }
                    }
                    termGroup.subTermCount = termSets.Count;
                    termGroup.subTerms = termSets;
                    if (termGroup.M365TermSyncOption is not TermSyncOption.None)
                    {
                        termGroup.M365TermSyncOption = termGroup.UsingMMSSpecified ? TermSyncOption.Specified : TermSyncOption.All;
                    }
                }
            }
            return termGroups;
        }

        public async Task<RMTermGroup> LoadGroupsData(Guid termGroupId)
        {
            int defaulPageSize = 15;
            using var context = GetNewContext();

            var termGroup = await context.TermGruops.AsQueryable().FirstOrDefaultAsync(g => g.IsRemoved == false && termGroupId == g.UniqueId);
            if (termGroup is null)
            {
                return null;
            }
            var termSets = await context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToListAsync();
            foreach (var termSet in termSets)
            {
                var terms = TermDao.GetTermFromTermSet(termSet.Id, 0, defaulPageSize);
                termSet.subTerms = terms;
                termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
            }
            termGroup.subTermCount = termSets.Count;
            termGroup.subTerms = termSets;
            if (termGroup.M365TermSyncOption is not TermSyncOption.None)
            {
                termGroup.M365TermSyncOption = termGroup.UsingMMSSpecified ? TermSyncOption.Specified : TermSyncOption.All;
            }

            return termGroup;
        }


        public async Task<List<RMTermGroup>> LoadGoogleGroupsData(List<Guid> groupIds, List<string> userAndGroupIds,FilterTermObjOption filterOption, int pageIndex, int pageSize)
        {
            List<RMTermGroup> termGroups = new();
            using var context = GetNewContext();
            var queryTermGroups = groupIds.IsNotNullOrEmpty()
                ? context.TermGruops.Where(termGroup => !termGroup.IsRemoved && groupIds.Contains(termGroup.UniqueId))
                : context.TermGruops.Where(termGroup => !termGroup.IsRemoved);
            var termGroupIds = await context.TermGroupMembership.Where(item => item.SiteType == SiteType.Google)
                .Join(queryTermGroups,
                    termGroupMembership => termGroupMembership.TermGroupId,
                    termGroup => termGroup.UniqueId,
                    (termGroupMembership, termGroup) => termGroupMembership.TermGroupId).Distinct().ToListAsync();
            foreach (var termGroupId in termGroupIds)
            {
                var termGroup = await GetTermGroupTreeDataAsync(context, termGroupId, userAndGroupIds, filterOption, pageIndex, pageSize);

                termGroups.Add(termGroup);
            }

            return termGroups;
        }

        private async Task<RMTermGroup> GetTermGroupTreeDataAsync(RMDbContext context, Guid termGroupId, List<string> userAndGroupIds, FilterTermObjOption filterOption, int pageIndex, int pageCount)
        {
            var termGroup = await context.TermGruops.FirstOrDefaultAsync(termGroup => termGroup.UniqueId == termGroupId);
            if (termGroup == null)
            {
                return null;
            }
            
            List<RMTermSet> termSets = [];
            if (userAndGroupIds.IsNotNullOrEmpty())
            {
                QuerySecurityTermObjDto dto = new ()
                {
                    UserAndGroupIds = userAndGroupIds,
                    Level = SecurityTermLevel.TermSet,
                    ParentId = termGroup.UniqueId,
                    FilterByContentSource = filterOption.FilterByContentSource,
                    ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                    SourceFlag = SourceFlag.Google
                };

                SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                if (result.TermPermissionType == TermPermissionMethod.All)
                {
                    termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    List<Guid> termSetUniqueIds = result.TermObjIds;
                    if (termSetUniqueIds != null)
                    {
                        termSets = context.TermSets.Where(t => !t.IsRemoved && termSetUniqueIds.Contains(t.UniqueId) && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                } 
            }
            else
            {
                termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();

            }
            
            foreach (var termSet in termSets)
            {
                var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSet.Id && a.ParentTermId == 0 && a.IsRemoved == false).OrderBy(a => a.TermName).Select(b => b.TermId).Skip((pageIndex - 1) * pageCount).Take(pageCount).ToList();
                List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(a => a.Name).ToList();
                if (terms != null)
                {
                    foreach (var term in terms)
                    {
                        TermDao.SetTermIsExpired(null, term);
                        if (term.TermSetId == 2 && Convert.ToInt32(term.AvailableSpace) != 0)
                        {
                            term.AvailableSpace = Math.Round(term.AvailableSpace, 2);
                        }
                        term.subTermCount = TermDao.SubTermCount(term.Id);
                    }
                }

                termSet.subTerms = terms;
                termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
            }
            termGroup.subTermCount = termSets.Count;
            termGroup.subTerms = termSets;

            return termGroup;
        }

        public async Task<Dictionary<string, List<string>>> GetTermGroupNameAndGoogleTenant(Guid termGroupId)
        {
            using var context = GetNewContext();
            Dictionary<string, List<string>> result = new();
            var returnList = await context.TermGruops.Where(item => item.UniqueId == termGroupId)
                .Join(context.TermGroupMembership.Where(termGroupMembership =>
                        termGroupMembership.TermGroupId == termGroupId &&
                        termGroupMembership.SiteType == SiteType.Google)
                    , termGroup => termGroup.UniqueId, termGroupMembership => termGroupMembership.TermGroupId,
                    (termGroup, termGroupMembership) => new
                    {
                        termGroupName = termGroup.Name,
                        googleTenant = termGroupMembership.SiteUrl
                    }).ToListAsync();
            foreach (var item in returnList)
            {
                if (!result.TryGetValue(item.termGroupName, out List<string> value))
                {
                    result.Add(item.termGroupName, [item.googleTenant]);
                }
                else
                {
                    value.Add(item.googleTenant);
                }
            }
            return result;
        }
        
        public async Task<RMTermGroup> GetTermGroupTreeDataAsync(RMDbContext context, string tenantId, int pageIndex, int pageCount, List<Guid> groupIds,
            List<string> userAndGroupIds, string searchKey = null)
        {
            var termGroupQuery = groupIds.IsNotNullOrEmpty()
                ? context.TermGruops.Where(termGroup => groupIds.Contains(termGroup.UniqueId))
                : context.TermGruops;
            var termGroup = await (from tg in termGroupQuery
                                   join tgm in context.TermGroupMembership
                                   on tg.UniqueId equals tgm.TermGroupId
                                   where tgm.SiteUrl == tenantId && !tg.IsRemoved
                                   select tg).FirstOrDefaultAsync();
            if (termGroup == null)
            {
                return null;
            }
            List<RMTermSet> termSets = [];
            if (userAndGroupIds.IsNotNullOrEmpty())
            {
                QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                {
                    UserAndGroupIds = userAndGroupIds,
                    Level = SecurityTermLevel.TermSet,
                    ParentId = termGroup.UniqueId
                };

                dto.FilterByContentSource = true;
                dto.ExcludeBuiltIn = false;
                dto.SourceFlag = SourceFlag.Google;

                SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                if (result.TermPermissionType == TermPermissionMethod.All)
                {
                    termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    List<Guid> termsetUniqueIds = result.TermObjIds;
                    if (termsetUniqueIds != null)
                    {
                        termSets = context.TermSets.Where(t => !t.IsRemoved && termsetUniqueIds.Contains(t.UniqueId) && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                }
            }
            else
            {
                termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();

            }
            foreach (var termSet in termSets)
            {
                List<int> termids;
                if (searchKey.IsNotNullOrEmpty())
                {
                    termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSet.Id && a.ParentTermId == 0 && a.IsRemoved == false).OrderBy(a => a.TermName).Select(b => b.TermId).ToList();
                }
                else
                {
                    termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSet.Id && a.ParentTermId == 0 && a.IsRemoved == false).OrderBy(a => a.TermName).Select(b => b.TermId).Skip((pageIndex - 1) * pageCount).Take(pageCount).ToList();
                }
                List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id) && (string.IsNullOrEmpty(searchKey) || t.Name.Contains(searchKey))).OrderBy(a => a.Name).ToList();
                if (terms != null)
                {
                    foreach (var term in terms)
                    {
                        TermDao.SetTermIsExpired(null, term);
                        if (term.TermSetId == 2 && Convert.ToInt32(term.AvailableSpace) != 0)
                        {
                            term.AvailableSpace = Math.Round(term.AvailableSpace, 2);
                        }
                        term.subTermCount = TermDao.SubTermCount(term.Id);
                    }
                }

                termSet.subTerms = terms;
                termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
            }
            termGroup.subTermCount = termSets.Count;
            termGroup.subTerms = termSets;

            return termGroup;
        }

        public async Task<bool> CheckIsAllTermGroupsBothNoneOption()
        {
            using var context = GetNewContext();
            return await context.TermGruops.Where(termGroup =>  !termGroup.IsRemoved).AllAsync(termGroup =>
                termGroup.GoogleTermSyncOption == TermSyncOption.None &&
                termGroup.M365TermSyncOption == TermSyncOption.None);
        }

        public List<RMTermGroup> LoadLocationData()
        {
            List<RMTermGroup> termGroups = null;
            using (var context = GetNewContext())
            {
                termGroups = context.TermGruops.Where(l => l.Id.Equals(2)).ToList();
                List<RMTermSet> termSets = new List<RMTermSet>();
                termSets = context.TermSets.Where(l => l.Id.Equals(2)).ToList();
                foreach (var termGroup in termGroups)
                {
                    foreach (var termSet in termSets)
                    {
                        List<RMTerm> terms = TermDao.GetTermFromTermSet(termSet.Id);
                        termSet.subTerms = terms;
                        termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
                    }
                    termGroup.subTermCount = 1;
                    termGroup.subTerms = termSets;
                }
            }
            return termGroups;
        }

        public List<RMTermSet> LoadLocationSet()
        {
            List<RMTermSet> termSets = null;
            using (var context = GetNewContext())
            {
                termSets = context.TermSets.Where(l => l.TermSetType == (TermSetType.Physical)).ToList();
                foreach (var termSet in termSets)
                {
                    List<RMTerm> terms = TermDao.GetTermFromTermSet(termSet.Id);
                    termSet.subTerms = terms;
                    termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
                }
            }
            return termSets;
        }

        public RMTermGroup LoadTermDataById(Guid termGroupId, bool isBussiness = false, FilterTermObjOption filterOption = null)
        {
            RMTermGroup termGroup = null;
            using (var context = GetNewContext())
            {
                var permissionResult = SecurityGroupDao.GetSecurityTermObjInfo(new QuerySecurityTermObjDto
                {
                    UserAndGroupIds = filterOption.userAndGroupUserIds,
                    Level = SecurityTermLevel.TermSet,
                    ParentId = termGroupId,
                    FilterByContentSource = filterOption.NeedCheckPermission,
                    ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                    ContainerId = filterOption.ContainerId,
                    SourceFlag = filterOption.SourceFlag
                });
                if (permissionResult.TermPermissionType != TermPermissionMethod.None)
                {
                    var hasPermissionGroupIds = permissionResult.TermObjIds;
                    termGroup = context.TermGruops.Where(t => t.UniqueId.Equals(termGroupId)).FirstOrDefault();
                    List<RMTermSet> termSets = new List<RMTermSet>();
                    Expression<Func<RMTermSet, bool>> hasPermissionPredicate = t => hasPermissionGroupIds.Contains(t.UniqueId);
                    if (isBussiness)
                    {
                        Expression<Func<RMTermSet, bool>> bussinessPredicate = t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase);
                        if (hasPermissionGroupIds != null && hasPermissionGroupIds.Count > 0)
                        {
                            termSets = context.TermSets.Where(bussinessPredicate).Where(hasPermissionPredicate).ToList();
                        }
                        else
                        {
                            termSets = context.TermSets.Where(bussinessPredicate).ToList();
                        }
                    }
                    else
                    {
                        Expression<Func<RMTermSet, bool>> normalPredicate = t => t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase);
                        if (hasPermissionGroupIds != null && hasPermissionGroupIds.Count > 0)
                        {
                            termSets = context.TermSets.Where(normalPredicate).Where(hasPermissionPredicate).ToList();
                        }
                        else
                        {
                            termSets = context.TermSets.Where(normalPredicate).ToList();
                        }
                    }

                    foreach (var termSet in termSets)
                    {
                        List<RMTerm> terms = TermDao.GetTermFromTermSet(termSet.Id);
                        termSet.subTerms = terms;
                        termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
                    }
                    termGroup.subTermCount = termSets.Count;
                    termGroup.subTerms = termSets;
                    if (termGroup.M365TermSyncOption is not TermSyncOption.None)
                    {
                        termGroup.M365TermSyncOption = termGroup.UsingMMSSpecified ? TermSyncOption.Specified : TermSyncOption.All;
                    }
                }
            }
            return termGroup;
        }

        public bool HasSameNameTermGroup(string termGroupName)
        {
            using var context = GetNewContext();
            var termGroup = context.TermGruops.AsQueryable().Where(t => t.Name.Equals(termGroupName) && t.IsRemoved == false).FirstOrDefault();
            if (termGroup != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ReNameHasSameNameTermGroup(int termGroupId, string termGroupName)
        {
            bool hasSame = false;
            try
            {
                using var context = GetNewContext();
                var termGroups = context.TermGruops.AsQueryable().Where(t => !t.Id.Equals(termGroupId));
                if (termGroups != null && termGroups.Count() > 0)
                {
                    List<int> termGroupIds = termGroups.Select(t => t.Id).ToList();
                    if (context.TermGruops.AsQueryable().Where(t => termGroupIds.Contains(t.Id) && t.Name.Equals(termGroupName) && t.IsRemoved == false).FirstOrDefault() != null)
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

        public async Task<RMTermGroup> RenameTermGroupAsync(int termGroupId, string termGroupName)
        {
            using var context = GetNewContext();
            if (ReNameHasSameNameTermGroup(termGroupId, termGroupName))
            {
                throw new Exception("Term Group has same name");
            }
            RMTermGroup termGroup = context.TermGruops.AsQueryable().Where(t => t.Id == termGroupId).FirstOrDefault();
            termGroup.Name = termGroupName;
            await this.UpdateAsync(termGroup);
            return termGroup;
        }

        public async Task DeleteTermGroupAsync(Guid termGroupId)
        {
            using var context = GetNewContext();
            var termSets = await TermSetDao.LoadTermSetAsync(TermSetType.Business, termGroupId);
            foreach (var termSet in termSets)
            {
                TermDao.DeleteTermByTermSetId(termSet.Id);
                await TermSetDao.DeleteTermSetAsync(termSet.Id);
            }
            var termGroup = context.TermGruops.AsQueryable().Where(g => g.UniqueId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            termGroup.IsRemoved = true;
            var termGroupMemberships = context.TermGroupMembership.Where(tgm => tgm.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase));
            if (termGroupMemberships.IsNotNullOrEmpty())
            {
                context.TermGroupMembership.RemoveRange(termGroupMemberships);
            }
            await this.UpdateAsync(termGroup);
            context.SaveChanges();
        }

        public List<RMTermGroup> GetTermGroups(PagerInfo pager, out int totalCount)
        {
            var termGroups = new List<RMTermGroup>();
            using (var context = GetNewContext())
            {
                totalCount = context.TermGruops.Where(o => !o.IsRemoved).Count();
                termGroups = context.TermGruops.Where(o => !o.IsRemoved).OrderBy(o => o.Name).Skip(pager.PagerIndex * pager.PagerSize).Take(pager.PagerSize).ToList();
                foreach (var group in termGroups)
                {
                    group.subTermCount = TermSetDao.GetRMTermSetsByGroupUniqueId(group.UniqueId).Count;
                }
            }
            return termGroups;
        }

        public List<string> GetFarmIdsBySpecificSites()
        {
            List<string> farmIds = new List<string>();
            using (var context = GetNewContext())
            {
                string sql = @" select distinct(n.FarmId) from {0}.RMLocalNodes as n where n.NodeLevel = 100 
                               and exists(select m.Id from {0}.RMTermGroupMemberships as m join {0}.RMTermGroups as t 
                               on m.TermGroupId = t.UniqueId
                               where t.IsRemoved = 0 and t.UsingMMSSpecified = 1 and m.SiteType = @siteType and n.Url = m.SiteUrl)";
                SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                farmIds.AddRange(context.Database.SqlQuery<string>(string.Format(sql, context.SchemaName), new SqlParameter("siteType", Contract.Object.SiteType.OnPrem)).ToList());
            }
            return farmIds;
        }

        /// <summary>
        /// 获取和指定Farm关联的TermGroupIds
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        public List<Guid> GetTermGroupIdsByFarmId(string farmId)
        {
            List<Guid> termGroupIds = new List<Guid>();
            using (var context = GetNewContext())
            {
                string sql = @"select distinct(m.TermGroupId) from {0}.RMTermGroupMemberships as m 
                            where exists (select Id from {0}.RMLocalNodes as n where m.SiteUrl = n.Url and n.NodeLevel = 100 and n.FarmId = @farmId) and m.TermGroupId != @emptyGuid";
                var paras = new List<SqlParameter>
                {
                    new SqlParameter("farmId", farmId),
                    new SqlParameter("emptyGuid", Guid.Empty)
                };
                SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                termGroupIds.AddRange(context.Database.SqlQuery<Guid>(string.Format(sql, context.SchemaName), paras.ToArray()).ToList());
            }
            return termGroupIds;
        }

        public bool IsExistNeedSyncTermGroup(SiteType siteType)
        {
            using (var context = GetNewContext())
            {
                var termGroups = context.TermGruops.Where(o => !o.IsRemoved);
                if (termGroups.Any(o => !o.UsingMMSSpecified))
                {
                    //存在需要同步到所有termstore的termgroup
                    return true;
                }
                var termGroupIds = termGroups.Where(o => o.UsingMMSSpecified).Select(o => o.UniqueId).ToList();
                if (termGroupIds.Count > 0)
                {
                    //根据sitetype查找指定termstore的termgroup
                    var members = context.TermGroupMembership.Where(o => termGroupIds.Contains(o.TermGroupId) && o.SiteType == siteType).ToList();
                    if (members.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool IsExistNeedSyncTermGroupGoogle()
        {
            using (var context = GetNewContext())
            {
                var termGroups = context.TermGruops.Where(o => !o.IsRemoved
                && (o.GoogleTermSyncOption == TermSyncOption.All
                || o.GoogleTermSyncOption == TermSyncOption.Specified)).ToList();
                if (termGroups.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public List<RMTermGroup> LoadNeedSyncTermGroups(List<SiteType> siteType)
        {
            List<RMTermGroup> termGroups = [];
            using (var context = GetNewContext())
            {
                var allTermGroups = context.TermGruops.Where(o => o.IsRemoved == false).ToList();
                var syncedTermGroupIds = context.TermGroupMembership.Where(o => siteType.Contains(o.SiteType)).Select(o => o.TermGroupId).ToList();
                foreach (var group in allTermGroups)
                {
                    if (!group.UsingMMSSpecified || syncedTermGroupIds.Contains(group.UniqueId))
                    {
                        termGroups.Add(group);
                    }
                }
            }
            return termGroups;
        }

        public async Task<Dictionary<string, string>> GetAllTermGroups()
        {
            using var context = GetNewContext();
            return await context.TermGruops.Where(item => !item.IsRemoved).OrderBy(item => item.Name).ToDictionaryAsync(termGroup => termGroup.UniqueId.ToString(), termGroup => termGroup.Name);
        }

        public async Task<Dictionary<string, string>> GetAllTermGroupsByMultipleNodes(RMClassificationGroupMultipleNodes nodes)
        {
            var nodeIds = nodes.NodeIds;

            List<string> ggTenantsUnderNodes = nodes.NodeLevel switch
            {
                (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer =>
                    await RemoteNodeDao.GetGoogleTenantIdsUnderContainers(nodeIds),

                (int)NodeLevel.GoogleMyDrive or (int)NodeLevel.GoogleSharedDrive =>
                  RemoteNodeDao.GetGoogleTenantIdsUnderNodes(nodeIds, NodeLevelExpressionType.ExpressionGoogleDrive),

                _ => new List<string>()
            }; 
            
            if (ggTenantsUnderNodes.Count == 0)
            {
                return new Dictionary<string, string>();
            }
            var termGroupIds = await TermGroupMembershipDao.GetTermGroupsBySiteUrlGroupIds(ggTenantsUnderNodes);

            if (termGroupIds.Count == 1)
            {
                var termGroupId = new Guid(termGroupIds.First());
                var termGroup = GetTermGroupByUniqueIdForGoogleOne(termGroupId);
                if (termGroup == null)
                {
                    throw new InvalidOperationException($"Term group not found for ID: {termGroupId}");
                }
                return new Dictionary<string, string>()
                {
                    {termGroup.UniqueId.ToString(), termGroup.Name }
                };
            }
            else if (termGroupIds.Count == 0)
            {
                using var context = GetNewContext();
                return await context.TermGruops.Where(item => !item.IsRemoved).ToDictionaryAsync(termGroup => termGroup.UniqueId.ToString(), termGroup => termGroup.Name);
            }

            return new Dictionary<string, string> { { "message", "GoogleOpusUI.error.selected-scope-conflict-term-groups" } };

        }

        public async Task<List<string>> GetSpecifiedGoogleTenants(Guid termGroupId)
        {
            using var context = GetNewContext();
            return await context.TermGroupMembership.Where(termGroup => termGroup.TermGroupId == termGroupId && termGroup.SiteType == SiteType.Google).Select(termGroup => termGroup.SiteUrl).ToListAsync();
        }

        public async Task<string> GetTermGroupIdByTermUniqueId(Guid termUniqueId)
        {
            using var context = GetNewContext();

            var termGroupId = await (from t in context.Terms
                                join ts in context.TermSets
                                    on t.TermSetId equals ts.Id
                                where t.UniqueId == termUniqueId
                                select ts.TermGroupId).FirstOrDefaultAsync();
            return termGroupId.ToString();
        }
        public async Task<IEnumerable<RMTermGroup>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.TermGruops.AsNoTracking().OrderBy(g => g.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertTermGroupTableAsync(IEnumerable<RMTermGroup> termGroups)
        {
            using var context = GetNewContext();
            string tableName = "RMTermGroups";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, UniqueId, Name, Description, UsingMMSSpecified, GoogleTermSyncOption, M365TermSyncOption, IsRemoved) VALUES ");
                int i = 0;
                foreach (var termGroup in termGroups)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", termGroup.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", termGroup.UniqueId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", termGroup.Name));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", (object)termGroup.Description ?? string.Empty));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", termGroup.UsingMMSSpecified));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", (int)termGroup.GoogleTermSyncOption));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", (int)termGroup.M365TermSyncOption));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", termGroup.IsRemoved));
                    paramIndex += 8;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert Term Group data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllTermGroupAsync()
        {
            return await TruncateAllDataInTableAsync("RMTermGroups");
        }

        public async Task<List<RMTermGroup>> LoadGoogleGroupsData(List<Guid> groupIds)
        {
            List<RMTermGroup> termGroups = new();
            using var context = GetNewContext();
            foreach (var groupId in groupIds)
            {
                var termGroup = await GetTermGroupTreeDataAsync(context, groupId);

                termGroups.Add(termGroup);
            }

            return termGroups;
        }

        private async Task<RMTermGroup> GetTermGroupTreeDataAsync(RMDbContext context, Guid termGroupId)
        {
            var termGroup = await context.TermGruops.FirstOrDefaultAsync(termGroup => termGroup.UniqueId == termGroupId);
            if (termGroup == null)
            {
                return null;
            }

            List<RMTermSet> termSets = [];
            termSets = context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroup.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var termSet in termSets)
            {
                var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSet.Id && a.ParentTermId == 0 && a.IsRemoved == false).OrderBy(a => a.TermName).Select(b => b.TermId).ToList();
                List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(a => a.Name).ToList();
                if (terms != null)
                {
                    foreach (var term in terms)
                    {
                        TermDao.SetTermIsExpired(null, term);
                        if (term.TermSetId == 2 && Convert.ToInt32(term.AvailableSpace) != 0)
                        {
                            term.AvailableSpace = Math.Round(term.AvailableSpace, 2);
                        }
                        term.subTermCount = TermDao.SubTermCount(term.Id);
                    }
                }

                termSet.subTerms = terms;
                termSet.subTermCount = TermDao.SubTermCountByTermSetId(termSet.Id);
            }
            termGroup.subTermCount = termSets.Count;
            termGroup.subTerms = termSets;

            return termGroup;
        }
    }
}
