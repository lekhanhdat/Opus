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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using Org.BouncyCastle.Asn1.Tsp;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMMLTermDao : BaseDao<RMMLTerm>, IRMMLTermDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RMMLTermDao));
        public void AddOrUpdateTerms(List<MLTermDto> dtos, Guid trainingModeId, int maxTermCount)
        {
            using var context = GetNewContext();
            using var tran = context.Database.BeginTransaction();

            var dbAllTermIds = context.RMMLTerms.Select(o => o.Id).ToList();
            var needAddTerms = dtos.Where(o => !dbAllTermIds.Contains(o.Id)).ToList();
            var needAddTermIds = needAddTerms.Select(o => o.Id).ToList();
            var needUpdateTerms = dtos.Where(o=> !needAddTermIds.Contains(o.Id)).ToList();
            
            AddMLTerms(context, needAddTerms, trainingModeId);
            UpdateMLTerms(context, needUpdateTerms);    
            var notRemovedTermCount = context.RMMLTerms.Where(o => o.Status != (int)MLTermStatus.Removed).Count();
            if (notRemovedTermCount > maxTermCount)
            {
                throw new MLTermMaxCountExceededException("RM_MachineLearning_TermMaxCountExceeded");
            }
            tran.Commit();
        }

        private void AddMLTerms(RMDbContext context, List<MLTermDto> needAddTerms, Guid trainingModeId)
        {
            context.RMMLTerms.AddRange(needAddTerms.ConvertAll(o => Convert2RMMLTerm(o)));
            context.SaveChanges();
            var termModeMapping = new List<RMMLTermModeMapping>();
            needAddTerms?.ForEach(o =>
            {
                termModeMapping.Add(new RMMLTermModeMapping
                {
                    TermId = o.Id,
                    ModeId = trainingModeId
                });
            });
            context.RMMLTermModeMappings.AddRange(termModeMapping);
            context.SaveChanges();
        }

        private void UpdateMLTerms(RMDbContext context, List<MLTermDto> needUpdateTerms)
        {
            var needUpdateTermIds = needUpdateTerms.Select(o => o.Id).ToList();
            var dbTerms = context.RMMLTerms.Where(o => needUpdateTermIds.Contains(o.Id)).ToList();
            needUpdateTerms.ForEach(o =>
            {
                var dbTerm = dbTerms.Where(d => d.Id == o.Id).FirstOrDefault();
                if (dbTerm != null)
                {
                    dbTerm.Accuracy = 0;
                    dbTerm.AutoApply = o.AutoApply;
                    dbTerm.Status = (int)MLTermStatus.NotTrain;
                    dbTerm.ScopeChanged = 0;
                    dbTerm.Published = false;
                    dbTerm.ModifedTime = DateTime.UtcNow.Ticks;
                    dbTerm.Description = o.Description;
                }
            });
            this.BatchUpdate(context, dbTerms);
        }

        public async Task UpdateDescription(MLTermDto dto)
        {
            using var context = GetNewContext();
            if (context.RMMLTerms.Any(o => o.Id.Equals(dto.Id)))
            {
                var term = context.RMMLTerms.Where(o => o.Id.Equals(dto.Id)).FirstOrDefault();
                if (term != null)
                {
                    term.Description = dto.Description;
                    term.ModifedTime = DateTime.UtcNow.Ticks;
                    await this.UpdateAsync(term);
                }
            }
        }

        public void MarkTermRemoveStatus(List<Guid> ids)
        {
            using var context = GetNewContext();
            var needMarkRemovedTerms = context.RMMLTerms.Where(o => ids.Contains(o.Id)).ToList();
            needMarkRemovedTerms.ForEach(o =>
            {
                o.Status = (int)MLTermStatus.Removed;
            });
            this.BatchUpdate(needMarkRemovedTerms);
        }


        public void DeleteTerms(List<Guid> ids)
        {
            using var context = GetNewContext();
            var terms = context.RMMLTerms.Where(o => ids.Contains(o.Id)).ToList();
            this.BatchDelete(terms);
        }

        public async Task SetAutoApplyAsync(Guid termId, bool autoApply)
        {
            using var context = GetNewContext();
            if (context.RMMLTerms.Any(o => o.Id.Equals(termId)))
            {
                var term = context.RMMLTerms.Where(o => o.Id.Equals(termId)).FirstOrDefault();
                if (term != null)
                {
                    term.AutoApply = autoApply;
                    await this.UpdateAsync(term);
                }
            }
        }

        public List<MLTermDto> Query(MLTermQueryParam param, out int totalCount, bool isZeroShot = false)
        {
            var searchValue = param.SearchValue;
            var sortBy = "ModifedTime";
            var sortDirection = SortDirectionEnum.Descending;
            var thenSortBy = "";
            var thenSortByDirection = SortDirectionEnum.None;

            if (!string.IsNullOrEmpty(param.SortBy))
            {
                thenSortBy = sortBy;
                thenSortByDirection = sortDirection;
                sortBy = param.SortBy;
                sortDirection = param.IsAscending ? SortDirectionEnum.Ascending : SortDirectionEnum.Descending;
            }

            var filterConditions = GetFilterConditions(param);
            Expression<Func<RMMLTerm, bool>> statusCondition = filterConditions.ContainsKey(TermFilterColumn.Status) ? filterConditions[TermFilterColumn.Status] : null;
            Expression<Func<RMMLTerm, bool>> autoApplyCondition = filterConditions.ContainsKey(TermFilterColumn.AutoApply) ? filterConditions[TermFilterColumn.AutoApply] : null;
            Expression<Func<RMMLTerm, bool>> activeStatusCondition = o => o.Status != (int)MLTermStatus.Removed;
            var lowered = searchValue?.ToLower();
            using var context = GetNewContext();
            var result = (from m in context.RMMLTerms.AddWhere(activeStatusCondition).AddWhere(statusCondition).AddWhere(autoApplyCondition)
                          join t in context.Terms
                            on m.Id equals t.UniqueId
                          where string.IsNullOrEmpty(searchValue)
                              || (t.Name != null && t.Name.ToLower().Contains(lowered))
                              || (isZeroShot && m.Description != null && m.Description.ToLower().Contains(lowered))
                          orderby m.ModifedTime descending
                          select new MLTermDto
                          {
                              Id = m.Id,
                              Name = t.Name,
                              Status = (MLTermStatus)m.Status,
                              AutoApply = m.AutoApply,
                              Accuracy = isZeroShot
                                ? (
                                    (m.ZeroApprovalCount + m.ZeroReclassifyCount) == 0
                                        ? 0
                                        : (m.ZeroApprovalCount == 0
                                            ? 1
                                            : (double)m.ZeroApprovalCount / (m.ZeroApprovalCount + m.ZeroReclassifyCount) * 100)
                                  )
                                : m.Accuracy * 100,
                              TrainingScope = m.TrainingScopeCount,
                              ModifedTime = m.ModifedTime,
                              Description = m.Description,
                              ZeroApprovalCount = m.ZeroApprovalCount,
                              ZeroReclassifyCount = m.ZeroReclassifyCount,
                          });
            totalCount = result.Count();
            return result.SortBy(sortBy, sortDirection).ThenSortBy(thenSortBy, thenSortByDirection).Skip(param.PageIndex * param.PageSize).Take(param.PageSize).ToList();
        }

        public List<MLTermDto> GetAllMLTerm()
        {
            using var context = GetNewContext();
            var result = (from m in context.RMMLTerms
                          join t in context.Terms
                          on m.Id equals t.UniqueId
                          where m.Status != (int)MLTermStatus.Removed
                          orderby m.ModifedTime descending
                          select new MLTermDto
                          {
                              Id = m.Id,
                              Name = t.Name,
                              Status = (MLTermStatus)m.Status,
                              AutoApply = m.AutoApply,
                              Accuracy = m.Accuracy,
                              Description = m.Description,
                          });
            return result.ToList();
        }

        public List<Guid> GetAllMLTermIds()
        {
            using var context = GetNewContext();
            return (from m in context.RMMLTerms
                    where m.Status != (int)MLTermStatus.Removed
                    select m.Id).ToList();
        }

        private static Dictionary<TermFilterColumn, Expression<Func<RMMLTerm, bool>>> GetFilterConditions(MLTermQueryParam param)
        {
            var result = new Dictionary<TermFilterColumn, Expression<Func<RMMLTerm, bool>>>();
            param.Filters?.ForEach(o =>
            {
                if (o.Column == TermFilterColumn.Status)
                {
                    var filterTermStatusList = GetFilterTermStatusValue(o.ColumnValues);
                    if (filterTermStatusList != null && filterTermStatusList.Count > 0)
                    {
                        result.Add(TermFilterColumn.Status, o => filterTermStatusList.Contains(o.Status));
                    }
                }

                if (o.Column == TermFilterColumn.AutoApply)
                {
                    var filterAutoApplyList = GetFilterAutoApplyValue(o.ColumnValues);
                    if (filterAutoApplyList != null && filterAutoApplyList.Count > 0)
                    {
                        result.Add(TermFilterColumn.AutoApply, o => filterAutoApplyList.Contains(o.AutoApply));
                    }
                }
            });
            return result;
        }

        private static List<int> GetFilterTermStatusValue(List<string> columnValues)
        {
            var result = new List<int>();
            columnValues?.ForEach(o =>
            {
                if (Enum.TryParse(o, out MLTermStatus termStatus))
                {
                    result.Add((int)termStatus);
                }
            });
            return result;
        }

        private static List<bool> GetFilterAutoApplyValue(List<string> columnValues)
        {
            var result = new List<bool>();
            columnValues?.ForEach(o =>
            {
                if (bool.TryParse(o, out bool autoApply))
                {
                    result.Add(autoApply);
                }
            });
            return result;
        }

        private static RMMLTerm Convert2RMMLTerm(MLTermDto dto)
        {
            return new RMMLTerm
            {
                Id = dto.Id,
                Status = (int)dto.Status,
                AutoApply = dto.AutoApply,
                ModifedTime = DateTime.UtcNow.Ticks,
                Description = dto.Description,
            };
        }

        public MLTermDto GetTrainingTerm(Guid id)
        {
            using var context = GetNewContext();
            var result = from m in context.RMMLTerms
                         where m.Id == id
                         join t in context.Terms
                         on m.Id equals t.UniqueId
                         select new MLTermDto
                         {
                             Id = m.Id,
                             Name = t.Name,
                             Status = (MLTermStatus)m.Status,
                             AutoApply = m.AutoApply
                         };
            return result.FirstOrDefault();
        }

        public MLTermDto GetValidTrainingTerm(Guid id)
        {
            using var context = GetNewContext();
            var result = from m in context.RMMLTerms
                         where m.Id == id && m.Status != (int)MLTermStatus.Removed
                         join t in context.Terms
                         on m.Id equals t.UniqueId
                         select new MLTermDto
                         {
                             Id = m.Id,
                             Name = t.Name,
                             Status = (MLTermStatus)m.Status,
                             AutoApply = m.AutoApply
                         };
            return result.FirstOrDefault();
        }

        public async Task UpdateZeroApprovalCount(Guid id, long count)
        {
            using var context = GetNewContext();
            var term = context.RMMLTerms.Where(o => o.Id.Equals(id)).FirstOrDefault();
            if (term != null)
            {
                term.ZeroApprovalCount = count;
                await this.UpdateAsync(term);
            }
        }

        public async Task UpdateZeroReclassifyCount(Guid id, long count)
        {
            using var context = GetNewContext();
            var term = context.RMMLTerms.Where(o => o.Id.Equals(id)).FirstOrDefault();
            if (term != null)
            {
                term.ZeroReclassifyCount = count;
                await this.UpdateAsync(term);
            }
        }

        public async Task UpdateZeroApprovalReclassifyCount(Guid id, long approvalCount, long reclassifyCount)
        {
            using var context = GetNewContext();
            var term = context.RMMLTerms.Where(o => o.Id.Equals(id)).FirstOrDefault();
            if (term != null)
            {
                term.ZeroReclassifyCount = reclassifyCount;
                term.ZeroApprovalCount = approvalCount;
                await this.UpdateAsync(term);
            }
        }

        public async Task AddTermTrainingScopeCountValueAsync(Guid termId, int addValue)
        {
            using var context = GetNewContext();
            var term = context.RMMLTerms.Where(_ => _.Id == termId).FirstOrDefault();
            if(term != null)
            {
                term.TrainingScopeCount += addValue;
                await this.UpdateAsync(term);
            }
        }

        public async Task SubTermTrainingScopeCountValueAsync(Guid termId, int subValue)
        {
            using var context = GetNewContext();
            var term = context.RMMLTerms.Where(_ => _.Id == termId).FirstOrDefault();
            if (term != null)
            {
                term.TrainingScopeCount -= subValue;
                await this.UpdateAsync(term);
            }
        }

        public async Task<IEnumerable<RMMLTerm>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMMLTerms.AsNoTracking().OrderByDescending(o => o.ModifedTime).Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertMLTermTableAsync(IEnumerable<RMMLTerm> mLTerms)
        {
            using var context = GetNewContext();
            try
            {
                context.RMMLTerms.AddRange(mLTerms);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMMLTerms data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllMLTermAsync()
        {
            return await TruncateAllDataInTableAsync("RMMLTerms");
        }
    }
}
