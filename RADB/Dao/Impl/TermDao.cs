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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.MachineLearning;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.DB.Core;
namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermDao : BaseDao<RMTerm>, ITermDao
    {
        public IGeneralSettingDao GeneralSettingDao { get; set; }
        public ITermRuleAssociationDao TermRuleInfosDao { get; set; }
        public ITermSetMembershipDao TermSetMemebership { get; set; }
        public IRMSecurityGroupDao SecurityGroupDao { get; set; }

        public IRMEXOLabelDao mEXOLabelDao;
        protected IRMEXOLabelDao EXOLabelDao
        {
            get
            {
                if (mEXOLabelDao == null)
                {
                    mEXOLabelDao = (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao));
                }
                return mEXOLabelDao;
            }
        }
        private static readonly object lockCreateTerm = new object();
        private RALogger logger = RALogger.GetInstance(typeof(TermDao));
        private GeneralSettingModel GeneralSetting
        {
            get
            {
                return GeneralSettingDao.GetCurrentGeneralSetting(); ;
            }
        }

        private ITenantService TenantService => (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
        private ITermGroupDao TermGroupDao => (ITermGroupDao) PlatformWindsorManager.GetService(typeof(ITermGroupDao));

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();

        private const string DASHBOARD_SYNC_CHANGE_INFO = "DASHBOARD_SYNC_CHANGE_INFO";

        public TermTreeNode GetRATermSetTree(Guid termSetId)
        {
            TermTreeNode termNode = null;
            using (var ctx = GetNewContext())
            {
                var termSet = ctx.TermSets.AsQueryable().FirstOrDefault(a => a.UniqueId == termSetId);
                if (termSet != null)
                {
                    termNode = new TermTreeNode() { ID = termSetId, Type = RMTermType.TermSet, Name = termSet.Name, Children = new Dictionary<Guid, TermTreeNode>() };
                    var termids = ctx.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSet.Id && a.ParentTermId == 0 && !a.IsRemoved).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();
                    List<RMTerm> rootTerms = ctx.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).ToList();
                    if (rootTerms != null)
                    {
                        foreach (var term in rootTerms)
                        {
                            termNode.Children.Add(term.UniqueId, GetSubTermTreeNode(term, termSetId));
                        }
                    }
                }
            }

            return termNode;
        }
        /// <summary>
        /// 包含除Remove状态的所有Sub Terms
        /// </summary>
        public TermTreeNode GetSubTermTreeNode(RMTerm term, Guid parentId)
        {
            TermTreeNode termNode = null;
            if (term != null && !term.IsRemoved)
            {
                termNode = new TermTreeNode() { ID = term.UniqueId, Name = term.Name, Type = RMTermType.Term, ParentID = parentId, Children = new Dictionary<Guid, TermTreeNode>() };
                using (var context = GetNewContext())
                {
                    var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId == term.Id && t.IsRemoved == false);
                    foreach (var subTermMembership in subTermMemberships)
                    {
                        RMTerm subTerm = this.Find(t => t.Id == subTermMembership.TermId);
                        var subTermNode = GetSubTermTreeNode(subTerm, term.UniqueId);
                        if (subTermNode != null)
                        {
                            termNode.Children.Add(subTermNode.ID, subTermNode);
                        }
                    }
                }
            }
            return termNode;
        }
        public TermTreeNode GetRATermSetTreeOfOrphanedTerm(Guid termSetId)
        {
            TermTreeNode termNode = null;
            using var context = GetNewContext();
            var termSet = context.TermSets.AsQueryable().FirstOrDefault(a => a.UniqueId == termSetId);
            if (termSet != null)
            {
                termNode = new TermTreeNode() { ID = termSetId, Children = new Dictionary<Guid, TermTreeNode>() };
                var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSet.Id && a.ParentTermId == 0).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();
                List<RMTerm> rootTerms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).ToList();
                if (rootTerms != null)
                {
                    foreach (var term in rootTerms)
                    {
                        termNode.Children.Add(term.UniqueId, GetTermTreeNodeOfOrphanedTerm(term, termSetId));
                    }
                }
            }
            return termNode;
        }
        /// <summary>
        /// 判断一个term的所有parent是否有rule存在
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public bool ParentTermHasSetting(int termId)
        {
            bool result = false;
            using (var context = GetNewContext())
            {
                var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).First();
                var termPath = termMembership.Path;
                List<string> parentTermIds = termPath.Split('/').ToList();
                List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });
                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true))
                {
                    //var rule = context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true);
                    result = true;
                }
            }
            return result;
        }
        public int GetParentTermIdByPath(string path, int termSetId)
        {
            using var context = GetNewContext();
            List<string> parentNames = path.Split('|').ToList();
            parentNames.RemoveAt(parentNames.Count - 1);
            int parentTermId = 0;
            int index = 0;
            foreach (var parentName in parentNames)
            {
                if(index < 2)
                {
                    index++;
                    continue;
                }
                int termId = context.TermSetMemberships.AsQueryable().Where(t => t.TermSetId == termSetId
                                                                                && t.ParentTermId == parentTermId
                                                                                && t.TermName.Equals(parentName, StringComparison.OrdinalIgnoreCase)
                                                                                && t.IsRemoved == false)
                                                                    .Select(t => t.TermId).FirstOrDefault();
                parentTermId = termId;
            }
            return parentTermId;
        }
        public string EncodingStringUsingBase64(string content)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(content);
            return Convert.ToBase64String(buffer);
        }
        public List<Guid> GetAllSubTermUniqueIds(Guid termId)
        {
            List<Guid> allSubIds = new List<Guid>();
            using (var context = GetNewContext())
            {
                var term = context.Terms.AsQueryable().Where(t => t.UniqueId.Equals(termId)).First();
                GetInheritSubTerms(term.Id, ref allSubIds);
            }
            return allSubIds;
        }

        public void GetAllInheritTermsByRootTerm(int termId, ref List<RMTerm> terms, long timePoint = 0)
        {
            if (timePoint == 0)
            {
                timePoint = DateTime.UtcNow.Ticks;
            }
            using (var context = GetNewContext())
            {
                var term = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).FirstOrDefault();
                if (term != null)
                {
                    if (term.IsRemoved || term.IsPermanent)
                    {
                        return;
                    }
                    terms.Add(term);
                    GetInheritSubTerms(termId, ref terms);
                    //if (!term.IsDeprecated)
                    //{
                    //    if ((term.TermExpirationFrom > 0 && timePoint < term.TermExpirationFrom) || (term.TermExpirationTo > 0 && timePoint > term.TermExpirationTo))
                    //    {
                    //        return;
                    //        //GetInheritSubTerms(termId, ref terms);
                    //    }
                    //    else
                    //    {
                    //        terms.Add(term);
                    //        GetInheritSubTerms(termId, ref terms);
                    //    }
                    //}
                    //else
                    //{
                    //    GetInheritSubTerms(termId, ref terms);
                    //}
                }
                else
                {
                    logger.Error("termid not exist in RMDB");
                    return;
                }
            }

        }
        public int SubTermCount(int termId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Count(tm => tm.ParentTermId == termId && tm.IsRemoved == false);
        }
        public int SubTermCountByTermSetId(int termSetId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Count(tm => tm.TermSetId == termSetId && tm.IsRemoved == false && tm.ParentTermId == 0);
        }
        /// <summary>
        /// use for get search tree
        /// </summary>
        /// <param name="termsetId"></param>
        /// <param name="termLable"></param>
        /// <returns></returns>
        public List<RMTermGroup> GetRMTermsBySearch(string termLable, Guid termGroupId, bool withRuleName, FilterTermObjOption filterOption = null)
        {
            List<RMTermGroup> termGroups = new List<RMTermGroup>();
            List<RMTermSet> termsets = new List<RMTermSet>();
            List<RMTerm> termTree = new List<RMTerm>();
            using var context = GetNewContext();
            List<int> termSetIds = new List<int>();
            logger.Info("search term lable is {0}",termLable);
            var loadAllTerms = termGroupId.Equals(Guid.Empty) ? true : false;
            if (loadAllTerms)
            {
                if (filterOption != null && filterOption.NeedCheckPermission)
                {
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        UserAndGroupIds = filterOption.userAndGroupUserIds,
                        Level = SecurityTermLevel.TermGroup,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag
                    };
                    SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                    if (result.TermPermissionType == TermPermissionMethod.All)
                    {
                        termSetIds = context.TermSets.AsQueryable().Where(ts => (int)ts.TermSetType == (int)TermSetType.BusinessTerm && !ts.IsRemoved).Select(ts => ts.Id).ToList();
                    }
                    else if (result.TermPermissionType == TermPermissionMethod.SpecifyScope)
                    {
                        if (!result.TermObjIds.IsNullOrEmpty())
                        {
                            foreach (var groupId in result.TermObjIds)
                            {
                                List<RMTermSet> termset = GetRMTermSetsByGroupUniqueId(groupId, filterOption);
                                if (!termset.IsNullOrEmpty())
                                {
                                    List<int> tempTermSetIds = termset.Select(ts => ts.Id).ToList();
                                    termSetIds.AddRange(tempTermSetIds);
                                }
                            }
                        }
                    }
                }
                else
                {
                    termSetIds = context.TermSets.AsQueryable().Where(ts => (int)ts.TermSetType == (int)TermSetType.BusinessTerm && !ts.IsRemoved).Select(ts => ts.Id).ToList();
                }
            }
            else
            {
                termSetIds = context.TermSets.AsQueryable().Where(t => (int)t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && !t.IsRemoved).Select(ts => ts.Id).ToList();
            }

            var terms = new List<int>();
            var tmPaths = new List<string>();

            if (withRuleName)
            {
                var rules = context.RMTermRuleAssociations.AsQueryable().Where(r => r.RuleName.Contains(termLable)).Select(t => t.TermId).ToList();
                terms = context.Terms.AsQueryable().Where(tm => (tm.Name.Contains(termLable) || tm.Description.Contains(termLable) || rules.Contains(tm.Id)) && termSetIds.Contains(tm.TermSetId) && !tm.IsRemoved).Select(t => t.Id).ToList();
                tmPaths = context.TermSetMemberships.AsQueryable().Where(t => terms.Contains(t.TermId)).OrderBy(o => o.TermSetId).ThenBy(t => t.ParentTermId).Select(ts => ts.Path).ToList();
            }
            else
            {
                terms = context.Terms.AsQueryable().Where(tm => tm.Name.Contains(termLable) && termSetIds.Contains(tm.TermSetId) && tm.IsRemoved == false).Select(t => t.Id).ToList();
                tmPaths = context.TermSetMemberships.AsQueryable().Where(t => terms.Contains(t.TermId)).OrderBy(o => o.TermSetId).ThenBy(t => t.ParentTermId).Select(ts => ts.Path).ToList();
                
            }

            var matchTermSets = context.TermSets.Where(o => termSetIds.Contains(o.Id) && o.Name.Contains(termLable) && !o.IsRemoved).ToList();

            int termSetId;
            foreach (var tmPath in tmPaths)
            {
                logger.Info("init tmPath:{0}", tmPath);
                #region init termset
                termSetId = Convert.ToInt32(tmPath.Split('/')[0]);
                if (matchTermSets.Any(o => o.Id.Equals(termSetId)))
                {
                    continue;
                }
                RMTermSet termSet;
                if (termsets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault() == null)
                {
                    termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(termSet);
                    termSet.subTermCount = SubTermCountByTermSetId(termSet.Id);
                    termsets.Add(termSet);
                    termTree = new List<RMTerm>();
                    termSet.subTerms = termTree;
                }
                else
                {
                    termSet = termsets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(termSet);
                    termSet.subTerms = termTree;
                }
                logger.Info("init termset success,termset id is :{0}", termSet.Id);
                #endregion
                List<string> termIds = tmPath.Split('/').Skip(1).ToList();
                RMTerm rootTerm;
                bool haveParentSetting = false;
                int rootTermId = Convert.ToInt32(termIds[0]);
                logger.Info("Get rootTerm id :{0}", rootTermId);
                if (!termTree.AsQueryable().Any(t => t.Id.Equals(rootTermId)))
                {
                    rootTerm = context.Terms.AsQueryable().Where(tm => tm.Id.Equals(rootTermId)).FirstOrDefault();
                    rootTerm.subTermCount = SubTermCount(rootTermId);
                    logger.Info("Get rootTerm sub Term Count:{0}", rootTerm.subTermCount);
                    #region set str_timecolumn not mapped
                    rootTerm.TermExpirationFromStr = GetStrDateTime(rootTerm.TermExpirationFrom);
                    rootTerm.TermExpirationToStr = GetStrDateTime(rootTerm.TermExpirationTo);
                    #endregion
                    termTree.Add(rootTerm);
                }
                else
                {
                    rootTerm = termTree.AsQueryable().Where(t => t.Id.Equals(rootTermId)).FirstOrDefault();
                }
                SetTermIsExpired(null, rootTerm);
                haveParentSetting = rootTerm.BreakInheritFromParent;
                #region build term tree
                var tempTerm = new RMTerm();
                //last term node is rootterm load sun nodes
                if (tmPath == tmPaths[tmPaths.Count - 1] && 1 == termIds.Count)
                {
                    rootTerm.subTerms = GetTermFromParentTermWithoutDeletedTerm(rootTermId);
                }
                for (int i = 1; i < termIds.Count; i++)
                {
                    int subTermId = Convert.ToInt32(termIds[i]);
                    var subTerm = context.Terms.AsQueryable().Where(tf => tf.Id.Equals(subTermId)).FirstOrDefault();
                    logger.Info("Get subTerm name:{0},id:{1},fullpath:{2}", subTerm.Name,subTerm.Id,subTerm.FullPath);
                    subTerm.HaveParentSetting = haveParentSetting;
                    subTerm.subTermCount = SubTermCount(subTermId);
                    //last term node load sun nodes
                    if (tmPath == tmPaths[tmPaths.Count - 1] && i == termIds.Count - 1)
                    {
                        //subTerm.subTerms = GetTermFromParentTermWithoutDeletedTerm(subTermId);
                        BuildLastTermTree(subTerm);
                    }
                    #region set str_timecolumn not mapped
                    subTerm.TermExpirationFromStr = GetStrDateTime(subTerm.TermExpirationFrom);
                    subTerm.TermExpirationToStr = GetStrDateTime(subTerm.TermExpirationTo);
                    #endregion
                    if (!haveParentSetting)
                    {
                        haveParentSetting = ParentTermHasSetting(subTerm.Id);
                    }
                    if (i == 1)
                    {
                        tempTerm = BuildTermTree(rootTerm, subTerm);
                    }
                    else
                    {
                        tempTerm = BuildTermTree(tempTerm, subTerm);
                    }
                }
                logger.Info("Build Tree Success");
                #endregion
            }

            foreach (var matchTermSet in matchTermSets)
            {
                //TermSet匹配SearchKey，则把TermSet下的RootTerms全返回
                matchTermSet.subTerms = GetTermFromTermSet(matchTermSet.Id);
                matchTermSet.subTermCount = matchTermSet.subTerms.Count;
            }
            //匹配SearchKey的TermSet和匹配SearchKey的Term的ParentTermSet集合
            termsets = matchTermSets.Concat(termsets).ToList();

            List<RMTermGroup> groups = new List<RMTermGroup>();
            if (loadAllTerms)
            {
                groups = context.TermGruops.ToList();
            }
            else
            {
                groups = context.TermGruops.Where(g => g.UniqueId.Equals(termGroupId) && !g.IsRemoved).ToList();
            }

            foreach (var group in groups)
            {
                var termSets = termsets.Where(t => t.TermGroupId.Equals(group.UniqueId)).ToList();
                if (loadAllTerms && termSets.Count == 0)
                {
                    continue;
                }
                group.subTerms = termSets;
                termGroups.Add(group);
            }
            return termGroups;
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
        private RMTerm BuildTermTree(RMTerm term, RMTerm subTerm)
        {
            //need to give default value to do next
            if (term.subTerms == null)
            {
                term.subTerms = new List<RMTerm>();
            }
            if (term.subTerms.AsQueryable().Where(t => t.Id.Equals(subTerm.Id)).FirstOrDefault() == null)
            {
                SetTermIsExpired(term, subTerm);
                term.subTerms.Add(subTerm);
                term.subTermCount = term.subTerms.Count;
            }
            return subTerm;
        }
        public void BuildLastTermTree(RMTerm term)
        {
            using var context = GetNewContext();
            var subTermMeberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId.Equals(term.Id)).ToList();
            if (subTermMeberships != null && subTermMeberships.Count > 0)
            {
                foreach (var subTermMebership in subTermMeberships)
                {
                    var subTerm = context.Terms.AsQueryable().Where(s => s.Id.Equals(subTermMebership.TermId) && s.IsRemoved == false).FirstOrDefault();
                    //subTerm.subTermCount = SubTermCount(subTerm.Id);
                    if (term.subTerms == null)
                    {
                        term.subTerms = new List<RMTerm>();
                    }
                    if (subTerm != null)
                    {
                        term.subTerms.Add(subTerm);
                        BuildLastTermTree(subTerm);
                    }
                }
            }
            term.IsLastLayTermBySearch = true;
            term.subTermCount = SubTermCount(term.Id);
        }
        public string GetStrDateTime(long ticks)
        {
            if (0 == ticks)
            {
                return "";
            }
            var dt = DateTimeUtil.ConvertTimeFromUtc(ticks, GeneralSetting);
            return dt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT);
        }
        public string GetTimeZoneNameById(string timeZoneId)
        {
            string timeZoneName = string.Empty;
            if (!string.IsNullOrEmpty(timeZoneId))
            {

                timeZoneName = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId).DisplayName;
            }
            return timeZoneName;
        }
        public string GetTermNameByTermId(int termId)
        {
            using var context = GetNewContext();
            var term = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).FirstOrDefault();
            return term?.Name;
        }
        public string GetTermGroupNameById(int groupId)
        {
            using var context = GetNewContext();
            var termGroup = context.TermGruops.AsQueryable().Where(t => t.Id.Equals(groupId)).FirstOrDefault();
            return termGroup?.Name;
        }
        public string GetTermGroupUniqueIdByTermId(int termId)
        {
            using var context = GetNewContext();
            var termGroupId = (from ts in context.TermSets
                               join tsm in context.TermSetMemberships
                               on ts.Id equals tsm.TermSetId
                               where tsm.TermId == termId
                               select ts.TermGroupId).FirstOrDefault();
            return termGroupId.ToString();
        }
        public string GetTermSetNameById(int termSetId)
        {
            using var context = GetNewContext();
            var termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
            return termSet?.Name;
        }
        public bool IsExpiredTerm(int termId)
        {
            bool isExpired = false;
            try
            {
                long utcNow = DateTime.UtcNow.Ticks;
                RMTerm term = GetTermTimeSettings(termId);
                long beginTime = term.TermExpirationFrom;
                long endTime = term.TermExpirationTo;

                if (beginTime != 0 || endTime != 0)
                {
                    // from to 
                    if (beginTime != 0 && endTime != 0)
                    {
                        if (beginTime > utcNow || endTime < utcNow)
                        {
                            isExpired = true;
                        }
                    }
                    //only begintime
                    else if (endTime == 0)
                    {
                        if (beginTime > utcNow)
                        {
                            isExpired = true;
                        }
                    }
                    //only endtime
                    else if (beginTime == 0)
                    {
                        if (endTime < utcNow)
                        {
                            isExpired = true;
                        }
                    }
                }
            }
            catch
            {
                isExpired = false;
                //throw;
            }
            return isExpired;
        }
        private bool GetIsExpiredByTermRetireSetting(long beginTime, long endTime)
        {
            long utcNow = DateTime.UtcNow.Ticks;
            bool isExpired = false;
            if (beginTime != 0 || endTime != 0)
            {
                // from to 
                if (beginTime != 0 && endTime != 0)
                {
                    if (beginTime > utcNow || endTime < utcNow)
                    {
                        isExpired = true;
                    }
                }
                //only begintime
                else if (endTime == 0)
                {
                    if (beginTime > utcNow)
                    {
                        isExpired = true;
                    }
                }
                //only endtime
                else if (beginTime == 0)
                {
                    if (endTime < utcNow)
                    {
                        isExpired = true;
                    }
                }
            }
            return isExpired;
        }
        public List<int> GetAllTermIds()
        {
            using var context = GetNewContext();
            List<int> terms = context.Terms.AsQueryable().OrderBy(tt => tt.Id).Select(tt => tt.Id).ToList();
            return terms;
        }
        public List<RMTerm> GetAllTermsForce()
        {
            using var context = GetNewContext();
            List<RMTerm> terms = context.Terms.AsQueryable().ToList();
            return terms;
        }
        public List<RMTerm> GetAllNotRemoveTermsForce()
        {
            using var context = GetNewContext();
            List<RMTerm> terms = context.Terms.Where(item => !item.IsRemoved).AsQueryable().ToList();
            return terms;
        }

        public Dictionary<Guid, string> GetTermUniqueIdAndNameMapping()
        {
            using var context = GetNewContext();
            List<RMTerm> terms = context.Terms.AsQueryable().ToList();
            return terms.ToDictionary(t => t.UniqueId, t => t.Name);
        }

        public List<RMTermSet> GetAllTermSetsForce()
        {
            using var context = GetNewContext();
            List<RMTermSet> termSets = context.TermSets.AsQueryable().ToList();
            return termSets;
        }

        public List<RMTermSetMembership> GetAllTermSetMemberShipsForce()
        {
            using var context = GetNewContext();
            List<RMTermSetMembership> termSetMemberShips = context.TermSetMemberships.AsQueryable().ToList();
            return termSetMemberShips;
        }

        public Dictionary<Guid, string> GetTermIdAndNameMapping()
        {
            using var context = GetNewContext();
            return context.Terms.AsQueryable().ToDictionary(t => t.UniqueId, t => t.Name);
        }

        public Dictionary<Guid, string> GetExistingTermIdAndNameMapping()
        {
            using var context = GetNewContext();
            return context.Terms.AsQueryable().Where(t => !t.IsRemoved).ToDictionary(t => t.UniqueId, t => t.Name);
        }
        public List<RMTerm> GetAllSubLocationTerm(int id)
        {
            using var context = GetNewContext();
            List<RMTerm> subTerms = new List<RMTerm>();
            var termSelectedMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(id) && t.IsRemoved == false).FirstOrDefault();
            if (termSelectedMembership != null)
            {
                string termPath = termSelectedMembership.Path;
                var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => (t.Path.StartsWith(termPath)) && t.IsRemoved == false).ToList();
                foreach (var subTermMembership in subTermMemberships)
                {
                    var curLocationTerm = context.Terms.AsQueryable().Where(t => t.Id == subTermMembership.TermId).FirstOrDefault();
                    subTerms.Add(curLocationTerm);
                }
            }
            return subTerms;
        }
        public bool GetTermPermanentByTermId(int termId, bool onlyParent)
        {
            using var context = GetNewContext();
            RMTerm term = null;
            if (!onlyParent)
            {
                term = context.Terms.AsQueryable().Where(t => t.Id == termId).First();
                if (term.IsPermanent)
                {
                    return true;
                }
                else if (term.BreakInheritFromParent)
                {
                    return false;
                }
            }
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            if (termMembership != null)
            {
                var termPath = termMembership.Path;
                List<string> parentTermIds = termPath.Split('/').ToList();
                List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                ids.Reverse();
                foreach (var id in ids)
                {
                    term = context.Terms.AsQueryable().Where(t => t.Id.ToString() == id).First();
                    if (term.IsPermanent)
                    {
                        return true;
                    }
                    if (term.BreakInheritFromParent)
                    {
                        break;
                    }
                }
            }
            return false;
        }
        public Dictionary<Guid, TermSettingsInfo> GetRetetionTermDic(List<Guid> termIds)
        {
            Dictionary<Guid, TermSettingsInfo> tSettings = new Dictionary<Guid, TermSettingsInfo>();
            using (var ctx = GetNewContext())
            {
                var ts = ctx.Terms.Where(t => termIds.Contains(t.UniqueId)).Select(t => new { Id = t.Id, UniqueId = t.UniqueId, EnforceRetention = t.EnforceRetention, RootTerm = t.IsRootTerm, ExoLabel = t.EXORetentionLabel, SPLabel = t.SPRetentionLabel, OneDriveLabel = t.OneDriveRetentionLabel , TeamsLabel = t.TeamsRetentionLabel}).ToList();
                foreach (var t in ts)
                {
                    List<Guid> subTermIds = new List<Guid>();
                    int retentionStatus = -1;
                    string exoLabel = string.Empty, spLabel;
                    var oneDriveLabel = string.Empty;
                    var teamsLabel = string.Empty;
                    if (t.RootTerm)
                    {
                        retentionStatus = t.EnforceRetention;
                        exoLabel = t.ExoLabel;
                        spLabel = t.SPLabel;
                        oneDriveLabel = t.OneDriveLabel;
                        teamsLabel = t.TeamsLabel;
                    }
                    else
                    {
                        var parentTerm = GetParentInhertSetting(t.Id);
                        retentionStatus = parentTerm == null ? t.EnforceRetention : parentTerm.EnforceRetention;
                        exoLabel = parentTerm == null ? t.ExoLabel : parentTerm.EXORetentionLabel;
                        spLabel = parentTerm == null ? t.SPLabel : parentTerm.SPRetentionLabel;
                        oneDriveLabel = parentTerm == null ? t.OneDriveLabel : parentTerm.OneDriveRetentionLabel;
                        teamsLabel = parentTerm == null ? t.TeamsLabel : parentTerm.TeamsRetentionLabel;
                    }
                    GetInheritSubTerms(t.Id, ref subTermIds);
                    var termInfo = new TermSettingsInfo()
                    {
                        EnforceRetention = retentionStatus,
                        EXORetentionLabel = exoLabel,
                        SPRetentionLabel = spLabel,
                        OneDriveRetentionLabel = oneDriveLabel
                    };
                    if (!tSettings.ContainsKey(t.UniqueId))
                    {
                        tSettings.Add(t.UniqueId, termInfo);
                    }
                    foreach (var sId in subTermIds)
                    {
                        if (!tSettings.ContainsKey(sId))
                        {
                            tSettings.Add(sId, termInfo);
                        }
                    }

                }
            }
            return tSettings;
        }
        public List<Guid> GetAllValidEnforceRetentionTermIds()
        {
            using var context = GetNewContext();
            var result = new List<Guid>();

            if (context.Terms.Any(t => t.IsRemoved == false && t.EnforceRetention > 0))
            {
                result.AddRange(context.Terms.Where(t => t.IsRemoved == false && t.EnforceRetention > 0).Select(t => t.UniqueId));
            }
            return result;
        }
        public List<RMTerm> FSGetAllTermsUnderTermSet(int id)
        {
            using var context = GetNewContext();
            return context.Terms.AsQueryable().Where(t => t.TermSetId == id).ToList();
        }
        public List<Guid> GetAllSubTermUniqueIdsByTermSetId(Guid termSetId)
        {
            using (var context = GetNewContext())
            {
                var iTermSetId = context.TermSets.Where(t => t.UniqueId == termSetId).Select(t => t.Id).FirstOrDefault();
                if (iTermSetId > 0)
                {
                    return context.Terms.Where(t => t.IsRemoved == false && t.TermSetId == iTermSetId).Select(t => t.UniqueId).ToList();
                }
            }
            return new List<Guid>();
        }
        public List<Guid> GetAllSubTermUniqueIdsByTermId(Guid termId)
        {
            using (var context = GetNewContext())
            {
                var iTermId = context.Terms.Where(t => t.UniqueId == termId).Select(t => t.Id).FirstOrDefault();
                if (iTermId > 0)
                {
                    string partPath = "/" + iTermId.ToString() + "/";
                    return (from a in context.TermSetMemberships
                            join b in context.Terms on a.TermId equals b.Id
                            where a.IsRemoved == false && a.Path.Contains(partPath)
                            select b.UniqueId).ToList();
                }
            }
            return new List<Guid>();
        }

        public List<RMTerm> GetAllTermHasAdvanceSettingsTerms()
        {
            using var context = GetNewContext();
            List<RMTerm> terms = context.Terms.Where(tt => !string.IsNullOrEmpty(tt.AdvanceSettings) && !tt.IsRemoved).OrderBy(tt => tt.Id).ToList();
            return terms;
        }

        #region Get Term Method
        public RMTerm GetTermTimeSettings(int termId)
        {
            using (var context = GetNewContext())
            {
                RMTerm returnTerm = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).First();
                DealWithRetentionLabel(returnTerm);
                if (returnTerm.TermExpirationFrom != 0 || returnTerm.TermExpirationTo != 0)
                {
                    returnTerm.TermExpirationFromStr = GetStrDateTime(returnTerm.TermExpirationFrom);
                    returnTerm.TermExpirationToStr = GetStrDateTime(returnTerm.TermExpirationTo);
                }
                return returnTerm;
            }
        }
        [RACodeReview("Allen Yin")]
        public RMTerm GetRMTermByTermId(int termId,bool needRetentionLable=true)
        {
            using (var context = GetNewContext())
            {
                RMTerm term = context.Terms.AsQueryable().Where(tm => tm.Id.Equals(termId)).First();
                if(needRetentionLable)
                {
                    DealWithRetentionLabel(term);
                }
                term.subTermCount = SubTermCount(termId);
                var termStr = SerializerHelper.SerializeByDataContractSerializer(term);
                term = SerializerHelper.DeserializeByDataContractSerializer<RMTerm>(termStr);
                return term;
            }

        }
        public RMTerm GetRMTermByUniqueId(Guid uniqueId, bool needCheckExpired = true)
        {
            using var context = GetNewContext();
            RMTerm termTemp = context.Terms.AsQueryable().Where(tm => tm.UniqueId.Equals(uniqueId)).FirstOrDefault();
            if (termTemp != null && needCheckExpired)
            {
                termTemp.IsExpired = IsExpiredTerm(termTemp.Id);
            }
            return termTemp;
        }
        public RMTerm GetRMTermByGuId(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Terms.AsQueryable().Where(tm => tm.UniqueId.Equals(id)).FirstOrDefault();
            }
        }        
        
        public RMTerm GetAvailableTermByGuId(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.Terms.AsQueryable().Where(tm => tm.UniqueId.Equals(id) && !tm.IsRemoved).FirstOrDefault();
            }
        }

        public RMTermSet GetRMTermSetByGuid(Guid id)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.TermSets.AsQueryable().Where(ts => ts.UniqueId.Equals(id)).FirstOrDefault();
            }
        }
        /// <summary>
        /// use for view term detail tree (not finish)
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public RMTerm GetRMTermWithPathByTermId(Guid termId, bool forExport = false)
        {
            using var context = GetNewContext();
            var term = context.Terms.AsQueryable().Where(tm => tm.UniqueId.Equals(termId)).FirstOrDefault();
            if (term != null)
            {
                term.FullPath = GetTermNamePath(term.Id, forExport);
            }

            return term;
        }
        public RMTerm GetParentTermTimeSettings(int termId)
        {
            using var context = GetNewContext();
            RMTerm returnTerm = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).First();
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).First();
            var termPath = termMembership.Path;
            List<string> parentTermIds = termPath.Split('/').ToList();
            List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
            ids.Reverse();
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    if (context.Terms.AsQueryable().Where(t => t.Id.ToString().Equals(id) && !string.IsNullOrEmpty(t.RuleInfo)).FirstOrDefault() != null)
                    {
                        RMTerm ValueTerm = context.Terms.AsQueryable().Where(t => t.Id.ToString().Equals(id)).First();
                        if (ValueTerm != null)
                        {
                            returnTerm.TermExpirationFrom = ValueTerm.TermExpirationFrom;
                            returnTerm.TermExpirationTo = ValueTerm.TermExpirationTo;
                            #region set str_timecolumn not mapped
                            returnTerm.TermExpirationFromStr = GetStrDateTime(ValueTerm.TermExpirationFrom);
                            returnTerm.TermExpirationToStr = GetStrDateTime(ValueTerm.TermExpirationTo);
                            //returnTerm.TimeZoneId = ValueTerm.TimeZoneId;
                            //returnTerm.IsDayLight = ValueTerm.IsDayLight;
                            #endregion
                            return returnTerm;
                        }
                    }
                }
            }
            return returnTerm;
        }
        public RMTerm GetParentInhertSetting(Guid termId)
        {
            using var context = GetNewContext();
            RMTerm result = null;
            int tId = -1;

            if (context.Terms.Any(t => t.UniqueId == termId && t.BreakInheritFromParent))
            {
                //item has own setting
                return context.Terms.Where(t => t.UniqueId == termId).FirstOrDefault();
            }
            if (context.Terms.Any(t => t.UniqueId == termId))
            {
                tId = context.Terms.Where(t => t.UniqueId == termId).Select(t => t.Id).First();
            }
            else
            {
                return result;
            }
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId == tId).First();
            var termPath = termMembership.Path;
            List<string> parentTermIds = termPath.Split('/').ToList();
            List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
            var tIds = ids.ConvertAll(i => { return int.Parse(i); });
            if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true))
            {
                //find parent item setting
                result = context.Terms.AsQueryable().Where(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true).OrderByDescending(t => t.Id).First();
            }
            else
            {
                //return item 
                result = context.Terms.Where(t => t.UniqueId == termId).FirstOrDefault();
            }

            return result;
        }
        /// <summary>
        /// 获取term的继承 setting 或者自身设置的setting，没有返回Null
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        public RMTerm GetParentInhertSetting(int termId)
        {
            RMTerm result = null;

            using (var context = GetNewContext())
            {
                if (context.Terms.Any(t => t.Id == termId && t.BreakInheritFromParent))
                {
                    result = context.Terms.Where(t => t.Id == termId).First();
                    DealWithRetentionLabel(result);
                    result.HaveParentSetting = ParentTermHasSetting(termId);
                    return result;
                }
                var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).First();
                var termPath = termMembership.Path;
                List<string> parentTermIds = termPath.Split('/').ToList();
                List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });
                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true))
                {
                    result = context.Terms.AsQueryable().Where(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true).OrderByDescending(t => t.Id).First();
                    DealWithRetentionLabel(result);
                    result.HaveParentSetting = true;
                }
            }
            return result;
        }

        public List<RMTerm> GetActiveTermsByTermSetId(int termSetId)
        {
            using var context = GetNewContext();
            List<RMTerm> terms = context.Terms.Where(t => t.TermSetId == termSetId && t.IsRemoved == false ).ToList();
            return terms;
        }
        #endregion

        #region Get Term Collection Method
        /// <summary>
        /// 获取termset下的第一层term，支持分页
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns>按term name排序的term集合</returns>
        [RACodeReview("Allen Yin", comment: @"感觉此处有可以优化的空间,多次查询可合并为一个，
subTermCount 可以延迟到需要展开此term时再取？
")]
        public List<RMTerm> GetTermFromTermSet(int termSetId, int pageIndex, int pageCount)
        {
            using var context = GetNewContext();

            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSetId && a.ParentTermId == 0 && a.IsRemoved == false).OrderBy(a => a.TermName).Select(b => b.TermId).Skip(pageIndex * pageCount).Take(pageCount).ToList();

            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(a => a.Name).ToList();

            if (terms != null)
            {
                foreach (var term in terms)
                {
                    #region set str_timecolumn not mapped
                    term.TermExpirationFromStr = GetStrDateTime(term.TermExpirationFrom);
                    term.TermExpirationToStr = GetStrDateTime(term.TermExpirationTo);
                    SetTermIsExpired(null, term);
                    //如果是location类型的  availablespace取前两个值
                    if (term.TermSetId == 2 && Convert.ToInt32(term.AvailableSpace) != 0)
                    {
                        term.AvailableSpace = Math.Round(term.AvailableSpace, 2);
                    }
                    #endregion
                    #region no need check time logic
                    //if (term.TermExpirationFrom > 0 && term.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < term.TermExpirationFrom || DateTime.UtcNow.Ticks > term.TermExpirationTo)
                    //    {
                    //        term.IsDeprecated = true;
                    //        this.Update(term, t => t.Id == term.Id);
                    //    }
                    //}
                    //else if (term.TermExpirationFrom > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < term.TermExpirationFrom)
                    //    {
                    //        term.IsDeprecated = true;
                    //        this.Update(term, t => t.Id == term.Id);
                    //    }
                    //}
                    //else if (term.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks > term.TermExpirationTo)
                    //    {
                    //        term.IsDeprecated = true;
                    //        this.Update(term, t => t.Id == term.Id);
                    //    }
                    //}
                    #endregion
                    term.subTermCount = SubTermCount(term.Id);
                }
            }
            return terms;
        }
        /// <summary>
        /// 获取termset下的第一层term
        /// </summary>
        /// <param name="termSetId"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin", comment: @"感觉此处有可以优化的空间,多次查询可合并为一个，
subTermCount 可以延迟到需要展开此term时再取？
")]
        public List<RMTerm> GetTermFromTermSet(int termSetId, bool containRemovedTerm = false)
        {
            using (var ctx = GetNewContext())
            {

                var terms = (from a in ctx.TermSetMemberships
                             join b in ctx.Terms on a.TermId equals b.Id
                             where a.ParentTermId == 0 && a.TermSetId == termSetId
                                 && (!containRemovedTerm ? a.IsRemoved == containRemovedTerm : true)
                             select b).Distinct().ToList();

                foreach (var term in terms)
                {
                    if (term.TermSetId == 2 && Convert.ToInt32(term.AvailableSpace) != 0)
                    {
                        term.AvailableSpace = Math.Round(term.AvailableSpace, 2);
                    }
                    term.subTermCount = SubTermCount(term.Id);
                    term.IsExpired = GetIsExpiredByTermRetireSetting(term.TermExpirationFrom, term.TermExpirationTo);
                }
                return terms.OrderBy(t => t.Name).ToList();
            }
        }
        [RACodeReview("Allen Yin", comment: @"感觉此处有可以优化的空间,多次查询可合并为一个，
            判断一个term的祖先是否有rule貌似可以在前台从上层入手？前台记录此节点祖先是否有rule然后传递给下层节点即可
            ")]
        public List<RMTerm> GetTermFromParentTerm(int parentTermId, int pageIndex, int pageCount)
        {
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.ParentTermId == parentTermId && a.IsRemoved == false).OrderBy(a => a.TermName).Select(b => b.TermId).Skip(pageIndex * pageCount).Take(pageCount).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(a => a.Name).ToList();
            var parentTerm = context.Terms.AsQueryable().Where(p => p.Id.Equals(parentTermId)).First();
            parentTerm.IsExpired = IsExpiredTerm(parentTermId);
            if (terms != null)
            {
                //check 一下parent term是否有rule 关联
                bool checkParent = false;
                bool HaveParentSetting = false;
                foreach (var term in terms)
                {
                    if (!checkParent)
                    {
                        HaveParentSetting = ParentTermHasSetting(term.Id);
                        checkParent = true;
                    }
                    term.HaveParentSetting = HaveParentSetting;
                    #region set str_timecolumn not mapped
                    term.TermExpirationFromStr = GetStrDateTime(term.TermExpirationFrom);
                    term.TermExpirationToStr = GetStrDateTime(term.TermExpirationTo);
                    #endregion
                    #region no need check time logic
                    //if (term.TermExpirationFrom > 0 && term.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < term.TermExpirationFrom || DateTime.UtcNow.Ticks > term.TermExpirationTo)
                    //    {
                    //        term.IsDeprecated = true;
                    //        this.Update(term, t => t.Id == term.Id);
                    //    }
                    //}
                    //else if (term.TermExpirationFrom > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < term.TermExpirationFrom)
                    //    {
                    //        term.IsDeprecated = true;
                    //        this.Update(term, t => t.Id == term.Id);
                    //    }
                    //}
                    //else if (term.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks > term.TermExpirationTo)
                    //    {
                    //        term.IsDeprecated = true;
                    //        this.Update(term, t => t.Id == term.Id);
                    //    }
                    //}
                    #endregion
                    SetTermIsExpired(parentTerm, term);
                    term.subTermCount = SubTermCount(term.Id);
                    if (term.TermSetId == 2 && Convert.ToInt32(term.AvailableSpace) != 0)
                    {
                        term.AvailableSpace = Math.Round(term.AvailableSpace, 2);
                    }
                }
            }
            return terms;
        }
        public List<RMTerm> GetTermFromTermSetWithoutDeletedTerm(int termSetId)
        {
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSetId && a.ParentTermId == 0 && !a.IsRemoved).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();

            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(a => a.Name).ToList();
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    SetTermIsExpired(null, term);
                    DealWithRetentionLabel(term);
                    term.subTermCount = SubTermCount(term.Id);
                }
            }
            return terms;
        }
        public List<RMTerm> GetTermFromParentTermWithoutDeletedTerm(int parentTermId)
        {
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.ParentTermId == parentTermId && !a.IsRemoved).OrderBy(a => a.TermName).Select(b => b.TermId).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(a => a.Name).ToList();
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    SetTermIsExpired(null, term);
                    term.subTermCount = SubTermCount(term.Id);
                }
            }
            return terms;
        }

        public List<RMTerm> GetActiveTermByTermSetIds(IEnumerable<int> termSetIds)
        {
            using (var context = GetNewContext())
            {
                var termids = context.TermSetMemberships.Where(a => termSetIds.Contains(a.TermSetId) && a.ParentTermId == 0 && !a.IsRemoved).Select(b => b.TermId).ToHashSet();
                var terms = context.Terms.Where(t => termids.Contains(t.Id)).ToList();
                var activeTerms = terms.Where(item => !GetIsExpiredByTermRetireSetting(item.TermExpirationFrom, item.TermExpirationTo)).ToList();
                return activeTerms;
            }
        }

        public List<RMTerm> GetActiveTermByTermSetId(int termsetId)
        {
            using (var context = GetNewContext())
            {
                var termids = context.TermSetMemberships.Where(a => a.TermSetId == termsetId && a.ParentTermId == 0 && !a.IsRemoved).Select(b => b.TermId).ToHashSet();
                var terms = context.Terms.Where(t => termids.Contains(t.Id)).ToList();
                var activeTerms = terms.Where(item => !GetIsExpiredByTermRetireSetting(item.TermExpirationFrom, item.TermExpirationTo)).ToList();
                foreach (var activeTerm in activeTerms)
                {
                    activeTerm.FullPath = GetTermNamesPathByTermId(activeTerm.UniqueId);
                }
                return activeTerms;
            }
        }

        public List<RMTerm> GetActiveTermByParentId(int parentTermId)
        {
            using (var context = GetNewContext())
            {
                var termids = context.TermSetMemberships.Where(a => a.ParentTermId == parentTermId && !a.IsRemoved).Select(b => b.TermId).ToHashSet();
                var terms = context.Terms.Where(t => termids.Contains(t.Id)).ToList();
                var activeTerms = terms.Where(item => !GetIsExpiredByTermRetireSetting(item.TermExpirationFrom, item.TermExpirationTo)).ToList();
                foreach (var activeTerm in activeTerms)
                {
                    activeTerm.FullPath = GetTermNamesPathByTermId(activeTerm.UniqueId);
                }
                return activeTerms;
            }
        }

        public List<RMTerm> GetTermFromParentTermForRuleUsageReport(RMTerm parentTerm)
        {
            int parentTermId = parentTerm.Id;
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.ParentTermId == parentTermId).OrderBy(a => a.TermName).Select(b => b.TermId).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).ToList();
            List<RMTerm> cloneTerms = new List<RMTerm>();
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    RMTerm cloneTerm = this.CloneTerm(term);
                    cloneTerms.Add(cloneTerm);
                    if (cloneTerm.BreakInheritFromParent || cloneTerm.IsDeprecated)
                    {
                        continue;
                    }
                    cloneTerm.TermExpirationFrom = term.TermExpirationFrom;
                    cloneTerm.TermExpirationTo = term.TermExpirationTo;
                    //cloneTerm.IsDayLight = term.IsDayLight;
                    //cloneTerm.TimeZoneId = term.TimeZoneId;
                }
            }
            return cloneTerms;
        }
        public List<RMTerm> GetTermFromParentTerm(RMTerm parentTerm)
        {
            int parentTermId = parentTerm.Id;
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.ParentTermId == parentTermId).OrderBy(a => a.TermName).Select(b => b.TermId).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).ToList();
            List<RMTerm> cloneTerms = new List<RMTerm>();
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    RMTerm cloneTerm = this.CloneTerm(term);
                    cloneTerms.Add(cloneTerm);
                    if (cloneTerm.BreakInheritFromParent)
                    {
                        continue;
                    }
                    cloneTerm.TermExpirationFrom = term.TermExpirationFrom;
                    cloneTerm.TermExpirationTo = term.TermExpirationTo;
                    //cloneTerm.IsDayLight = term.IsDayLight;
                    //cloneTerm.TimeZoneId = term.TimeZoneId;
                }
            }
            return cloneTerms;
        }

        public List<RMTerm> GetTermFromParentId(int parentTermId)
        {
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.ParentTermId == parentTermId).OrderBy(a => a.TermName).Select(b => b.TermId).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).ToList();
            List<RMTerm> cloneTerms = new List<RMTerm>();
            if (terms != null)
            {
                foreach (var term in terms)
                {
                    RMTerm cloneTerm = this.CloneTerm(term);
                    cloneTerms.Add(cloneTerm);
                    if (cloneTerm.BreakInheritFromParent)
                    {
                        continue;
                    }
                    cloneTerm.TermExpirationFrom = term.TermExpirationFrom;
                    cloneTerm.TermExpirationTo = term.TermExpirationTo;
                }
            }
            return cloneTerms;
        }
        public List<RMTerm> GetRMTermsByTermIds(int[] termsIds)
        {
            using var context = GetNewContext();
            var term = context.Terms.AsQueryable().Where(tm => Enumerable.Contains(termsIds, tm.Id) && tm.IsRemoved == false).ToList();
            return term;
        }
        public List<RMTerm> GetRMTermsByTermIds(List<Guid> termsIds)
        {
            using var context = GetNewContext();
            var term = context.Terms.AsQueryable().Where(tm => termsIds.Contains(tm.UniqueId) && tm.IsRemoved == false).ToList();
            return term;
        }
        public List<RMTerm> GetOrphanedTerms(int termSetId)
        {
            using var context = GetNewContext();
            List<RMTerm> oTerms = new List<RMTerm>();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSetId).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();
            List<RMTerm> allTerms = context.Terms.AsQueryable().Where(tt => termids.Contains(tt.Id)).OrderBy(t => t.Id).ToList();
            foreach (var term in allTerms)
            {
                //REC-2668 retired term不算做orphan term
                //if (term.IsRemoved || term.IsDeprecated || IsExpiredTerm(term.Id))
                if (term.IsRemoved)
                {
                    oTerms.Add(term);
                }
            }
            return oTerms;
        }

        public List<RMTerm> GetOprhanedTerms()
        {
            using(var context = GetNewContext())
            {
                var terms = context.Terms.Where(item => item.IsRemoved).ToList();
                foreach(var term in terms)
                {
                    term.FullPath = GetTermFullPathByTermId(term.UniqueId);
                }
                return terms;
            }
        }

        public List<RMTerm> GetRetiredTerms()
        {
            using(var context = GetNewContext())
            {
                var terms = context.Terms.Where(item => !item.IsRemoved).ToList();
                terms = terms.Where(item => item.IsDeprecated || GetIsExpiredByTermRetireSetting(item.TermExpirationFrom, item.TermExpirationTo)).ToList();
                foreach (var term in terms)
                {
                    term.FullPath = GetTermFullPathByTermId(term.UniqueId);
                }
                return terms;
            }
        }

        public List<RMTerm> GetretiredTerms(int termSetId)
        {
            using var context = GetNewContext();
            List<RMTerm> oTerms = new List<RMTerm>();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSetId).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();
            List<RMTerm> allTerms = context.Terms.AsQueryable().Where(tt => termids.Contains(tt.Id)).OrderBy(t => t.Id).ToList();
            foreach (var term in allTerms)
            {
                if (!term.IsRemoved && (term.IsDeprecated || IsExpiredTerm(term.Id)))
                {
                    oTerms.Add(term);
                }
            }
            return oTerms;
        }
        public List<RMTerm> GetAllTerms(int termSeId = 1)
        {
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => a.TermSetId == termSeId).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(tt => tt.Id).ToList();
            return terms;
        }
        public List<RMTerm> GetAllTerms(List<int> termSeIds)
        {
            using var context = GetNewContext();
            var termids = context.TermSetMemberships.AsQueryable().Where(a => termSeIds.Contains(a.TermSetId)).OrderBy(a => a.TermId).Select(b => b.TermId).ToList();
            List<RMTerm> terms = context.Terms.AsQueryable().Where(t => termids.Contains(t.Id)).OrderBy(tt => tt.Id).ToList();
            return terms;
        }
        #endregion

        #region Opreate term Method
        public RMTerm CreateTerm(TermInfo dto)
        {
            using (var ctx = GetNewContext())
            {
                CheckTermInfo(dto);
                var pTermId = dto.ParentTermId;
                RMTerm term = new RMTerm
                {
                    Name = dto.TermName,
                    TermSetId = dto.TermSetId,
                    Description = dto.Description,
                    UniqueId = Guid.NewGuid(),
                    IsRootTerm = pTermId == 0,
                    AdvanceSettings = dto.AdvanceSetting,
                };
                lock (lockCreateTerm)
                {
                    var pTermMembership = ctx.TermSetMemberships.Where(o => o.TermSetId == term.TermSetId && o.TermId == pTermId).FirstOrDefault();
                    using (var tran = ctx.Database.BeginTransaction())
                    {
                        term = ctx.Terms.Add(term);
                        var termSetId = term.TermSetId;
                        ctx.SaveChanges();
                        var termId = term.Id;
                        var termMembership = new RMTermSetMembership()
                        {
                            TermName = term.Name,
                            ParentTermId = pTermId,
                            TermId = termId,
                            TermSetId = termSetId,
                            Path = pTermMembership == null ? $"{termSetId}/{termId}" : $"{pTermMembership.Path}/{termId}"
                        };
                        ctx.TermSetMemberships.Add(termMembership);
                        ctx.SaveChanges();
                        tran.Commit();
                    }
                }
                term.HaveParentSetting = ParentTermHasSetting(term.Id);
                UpdateDashboardChangeInfoAsync().GetAwaiter().GetResult();
                return term;
            }
        }
        public RMTerm CreateTermForImport(string termName, int parentTermId, int termSetId, bool isDeprecated, Guid termUniqueId, string description = null)
        {
            RMTerm term = new RMTerm() { Name = termName, TermSetId = termSetId, Description = description, UniqueId = termUniqueId, IsDeprecated = isDeprecated };
            if (parentTermId == 0)
            {
                term.IsRootTerm = true;
            }

            if (HasSameNameTerm(termName, parentTermId, termSetId))
            {
                throw new Exception("Term has same name");
            }
            using (var context = GetNewContext())
            {
                RMTermSetMembership parentMembership = context.TermSetMemberships.AsQueryable().Where(ts => ts.TermSetId == termSetId && ts.TermId == parentTermId).FirstOrDefault();
                using (var tran = context.Database.BeginTransaction())
                {
                    RMTerm newTerm = context.Terms.Add(term);
                    context.SaveChanges();
                    // RMTerm newTerm = context.Terms.AsQueryable().Where(tm => tm.Name.Equals(termName) && tm.IsRemoved == false).OrderByDescending(t => t.Id).FirstOrDefault();
                    int newTermId = newTerm.Id;
                    RMTermSetMembership tmMembership = new RMTermSetMembership()
                    {
                        TermName = termName,
                        ParentTermId = parentTermId,
                        TermId = newTermId,
                        TermSetId = termSetId,
                        Path = parentMembership == null ? termSetId.ToString() + "/" + newTermId : parentMembership.Path + "/" + newTermId
                    };
                    context.TermSetMemberships.Add(tmMembership);
                    context.SaveChanges();
                    tran.Commit();
                    newTerm.HaveParentSetting = ParentTermHasSetting(newTermId);
                    UpdateDashboardChangeInfoAsync().GetAwaiter().GetResult();
                    return newTerm;
                }
                //check same termname
            }
        }
        public async Task<RMTerm> UpdateTermAsync(string termName, int parentTermId, int termSetId, bool isDeprecated, Guid termUniqueId, string description = null)
        {
            using var context = GetNewContext();
            RMTerm termFromDB = context.Terms.AsQueryable().Where(tm => tm.UniqueId.Equals(termUniqueId)).OrderByDescending(t => t.Id).FirstOrDefault();
            if (termFromDB != null)
            {
                termFromDB.Name = termName;
                termFromDB.Description = description;
                termFromDB.IsDeprecated = isDeprecated;
                termFromDB.IsRemoved = false;
            }
            await this.UpdateAsync(termFromDB);
            context.SaveChanges();
            RMTermSetMembership termMembership = context.TermSetMemberships.AsQueryable().Where(ts => ts.TermSetId == termSetId && ts.TermId == termFromDB.Id).FirstOrDefault();
            termMembership.IsRemoved = false;
            context.SaveChanges();

            return termFromDB;
        }
        public RMTerm UpdateTerm(string termName, int termId, int parentTermId, bool breakInherit, int termSetId, string description = null)
        {
            using (var context = GetNewContext())
            {
                RMTerm termFromDB = context.Terms.AsQueryable().Where(tm => tm.Id.Equals(termId)).FirstOrDefault();
                if (termFromDB != null)
                {
                    termFromDB.Name = termName;
                    termFromDB.Description = description;
                    //termFromDB.IsDeprecated = false;
                    termFromDB.IsRemoved = false;
                    termFromDB.BreakInheritFromParent = breakInherit; 
                }
                this.ApplyCurrentValues(context, termFromDB);
                RMTermSetMembership termMembership = context.TermSetMemberships.AsQueryable().Where(ts => ts.TermSetId == termSetId && ts.TermId == termFromDB.Id).FirstOrDefault();
                if (termMembership != null)
                {
                    termMembership.IsRemoved = false;
                }
                context.SaveChanges();
                return termFromDB;
            }
        }
        public RMTerm UpdateTermForJPMC(string termName, int termId, int parentTermId, bool breakInherit, int termSetId, string advancedSettings = null)
        {
            using (var context = GetNewContext())
            {
                RMTerm termFromDB = context.Terms.AsQueryable().Where(tm => tm.Id.Equals(termId)).FirstOrDefault();
                if (termFromDB != null)
                {
                    termFromDB.Name = termName;
                    termFromDB.AdvanceSettings = advancedSettings;
                    //termFromDB.IsDeprecated = false;
                    termFromDB.IsRemoved = false;
                    termFromDB.BreakInheritFromParent = breakInherit; 
                }
                this.ApplyCurrentValues(context, termFromDB);
                RMTermSetMembership termMembership = context.TermSetMemberships.AsQueryable().Where(ts => ts.TermSetId == termSetId && ts.TermId == termFromDB.Id).FirstOrDefault();
                termMembership.IsRemoved = false;
                context.SaveChanges();
                return termFromDB;
            }
        }
        public void DeleteAllTerm()
        {
            using var context = GetNewContext();
            var terms = context.Terms.ToList();
            if (terms.Count > 0)
            {
                context.Terms.RemoveRange(terms);
                context.SaveChanges();
            }
        }
        public void DeleteTermByTermSetId(int termSetId)
        {
            using var context = GetNewContext();
            var termsetMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.TermSetId.Equals(termSetId)).ToList();

            foreach (var termSetMemberShip in termsetMemberships)
            {
                termSetMemberShip.IsRemoved = true;
            }
            context.SaveChanges();

            var termIds = context.Terms.AsQueryable().Where(t => t.TermSetId.Equals(termSetId)).Select(t => t.Id).ToList();

            var termRuleInfos = context.RMTermRuleAssociations.AsQueryable().Where(t => termIds.Contains(t.TermId)).ToList();
            context.RMTermRuleAssociations.RemoveRange(termRuleInfos);
            context.SaveChanges();

            var terms = context.Terms.AsQueryable().Where(t => t.TermSetId.Equals(termSetId)).ToList();
            foreach (var term in terms)
            {
                term.IsRemoved = true;
            }

            var rmLabels = from l in context.RMGoogleLabelInfo
                           join t in context.Terms
                           on l.TermUniqueId equals t.UniqueId
                           where termIds.Contains(t.Id)
                           select l;
            if (rmLabels.IsNotNullOrEmpty())
            {
                rmLabels.ForEach(rmLabel => rmLabel.State = (int) State.Deleted);
            }
            var termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termSetId)).ToList();
            foreach (var term in termSet)
            {
                term.IsRemoved = true;
            }
            context.SaveChanges();
            UpdateDashboardChangeInfoAsync().GetAwaiter().GetResult();
        }
        public async Task DeleteTermAsync(int termId, List<Guid> deletedTermIds)
        {
            using var context = GetNewContext();
            var termsetMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            var termRuleInfo = context.RMTermRuleAssociations.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            var term = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).FirstOrDefault();
            if (termsetMembership != null)
            {
                termsetMembership.IsRemoved = true;
                await TermSetMemebership.UpdateAsync(termsetMembership);
            }
            if (termRuleInfo != null)
            {
                context.RMTermRuleAssociations.Remove(termRuleInfo);
            }
            term.IsRemoved = true;
            // soft delete label info
            var rmLabel = context.RMGoogleLabelInfo.Where(l => l.TermUniqueId == term.UniqueId).FirstOrDefault();
            if (rmLabel != null)
            {
                rmLabel.State = (int)State.Deleted;
            }
            await this.UpdateAsync(term);
            context.SaveChanges();
            deletedTermIds.Add(term.UniqueId);
            var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId.Equals(termId)).ToList();
            foreach (var subTermMembership in subTermMemberships)
            {
                var subTerm = context.Terms.AsQueryable().Where(t => t.Id.Equals(subTermMembership.TermId)).FirstOrDefault();
                if (subTerm != null)
                {
                    await DeleteTermAsync(subTerm.Id, deletedTermIds);
                }
            }
            await UpdateDashboardChangeInfoAsync();
        }

        public async Task<RMTerm> SaveTermSettingAsync(int termId, TermSettingsInfo settingInfo)
        {
            using (var context = GetNewContext())
            {
                if (null == context)
                {
                    logger.Error("dbContext instance is null");
                }
                ArgumentCheck.NotNull(context, nameof(context));
                var term = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).FirstOrDefault();

                if (null == term)
                {
                    logger.Error("query db term is null where termId = {0}", termId);
                }
                bool isInhretSettingChanged = false;
                bool isBaseSettingChanged = false;
                ParentTermSettings pSetting = null;
                var infos = settingInfo.infos;
                var selDateType = settingInfo.selDateType;
                var beginTime = settingInfo.beginTime;
                var endTime = settingInfo.endTime;
                var isDayLight = GeneralSetting.DayLight;
                var timeZoneId = GeneralSetting.TimeZoneId;
                var termDescription = settingInfo.des;
                var enforceRetention = settingInfo.EnforceRetention;
                var advanceSettings = settingInfo.advanceSettings;
                StringBuilder builder = new StringBuilder();

                List<RMTermRuleAssociation> termRuleInfos = new List<RMTermRuleAssociation>();
                try
                {
                    ArgumentCheck.NotNull(term, nameof(term));
                    if (!term.IsRootTerm)
                    {
                        pSetting = GetParentTermSettings(termId);
                    }

                }
                catch
                {
                    logger.Error("GetParentTermSettings method throw error");
                }
                if (infos != null && infos.Count > 0)
                {
                    foreach (var ruleInfo in infos)
                    {
                        logger.Info("RuleInfo RuleId {0},RuleLevel {1},RuleName {2},RuleOrder {3},TermId {4},TermName {5}"
                            , ruleInfo.RuleId, ruleInfo.RuleLevel, ruleInfo.RuleName, ruleInfo.RuleOrder, termId, term.Name);
                        termRuleInfos.Add(new RMTermRuleAssociation()
                        {
                            RuleId = new Guid(ruleInfo.RuleId),
                            RuleLevel = ruleInfo.RuleLevel,
                            RuleName = ruleInfo.RuleName,
                            RuleOrder = ruleInfo.RuleOrder,
                            TermId = termId,
                            TermName = term.Name
                        });
                        builder.Append(ruleInfo.RuleId);
                    }
                    //builder.Append(beginTime);
                    //builder.Append(endTime);
                }

                logger.Info("builder is {0} ", builder.ToString());
                logger.Info($"EnforceReteion: {settingInfo.EnforceRetention}, splabel:{settingInfo.SPRetentionLabel}, exolabel:{settingInfo.EXORetentionLabel}, onedrivelabel:{settingInfo.OneDriveRetentionLabel}");
                //当前节点有自己的setting
                if (term.BreakInheritFromParent || pSetting == null)
                {
                    if (!EncodingStringUsingBase64(builder.ToString()).Equals(term.RuleInfo))
                    {
                        isInhretSettingChanged = true;
                        await UpdateDashboardChangeInfoAsync();
                    }
                    else if (settingInfo.EnforceRetention != term.EnforceRetention
                        || settingInfo.EXORetentionLabel != term.EXORetentionLabel
                        || settingInfo.OneDriveRetentionLabel != term.OneDriveRetentionLabel
                        || settingInfo.SPRetentionLabel != term.SPRetentionLabel
                        || settingInfo.TeamsRetentionLabel != term.TeamsRetentionLabel)
                    {
                        isInhretSettingChanged = true;
                    }
                }
                else if (settingInfo.breakInhert)// break inhret from UIs
                {
                    isInhretSettingChanged = true;
                }
                logger.Info($"isInhretSettingChanged: {isInhretSettingChanged}");
                if (settingInfo.des != term.Description
                    || term.TermExpirationFrom != settingInfo.beginTimeForDB ||
                term.TermExpirationTo != settingInfo.endTimeForDB)
                //|| term.IsDayLight != GeneralSetting.isShowDayLight || term.TimeZoneId != GeneralSetting.TimeZoneId
                {
                    isBaseSettingChanged = true;
                }

                if (TenantService.IsCustomizationAppTenant() && settingInfo.advanceSettings != term.AdvanceSettings)
                {
                    isBaseSettingChanged = true;
                }

                if (isBaseSettingChanged)
                {
                    logger.Info("start save term ,{0}:{1}", term.Id, term.Name);
                    term.Description = termDescription;
                    term.TermExpirationFrom = settingInfo.beginTimeForDB;
                    term.TermExpirationTo = settingInfo.endTimeForDB;
                    //term.IsDayLight = isDayLight;
                    //term.TimeZoneId = timeZoneId;
                    term.AdvanceSettings = advanceSettings;

                }
                if (isInhretSettingChanged)
                {
                    if (null == TermRuleInfosDao)
                    {
                        throw new Exception("TermRuleInfosDao is null ");
                    }
                    TermRuleInfosDao.DeleteTermRuleInfos(termId);
                    using (var newContext = GetNewContext())
                    {
                        newContext.RMTermRuleAssociations.AddRange(termRuleInfos);
                        newContext.SaveChanges();
                        logger.Info("Save Rule Change Success {0}", termId);
                    }
                    if (!string.IsNullOrEmpty(builder.ToString()))
                    {
                        term.RuleInfo = EncodingStringUsingBase64(builder.ToString());
                    }
                    else
                    {
                        term.RuleInfo = string.Empty;
                    }
                    term.EnforceRetention = settingInfo.EnforceRetention;
                    term.SPRetentionLabel = settingInfo.SPRetentionLabel;
                    term.EXORetentionLabel = settingInfo.EXORetentionLabel;
                    term.OneDriveRetentionLabel = settingInfo.OneDriveRetentionLabel;
                    term.TeamsRetentionLabel = settingInfo.TeamsRetentionLabel;
                }
                if (settingInfo.breakInhert || pSetting == null)
                {
                    if (pSetting == null && string.IsNullOrEmpty(term.RuleInfo) && settingInfo.EnforceRetention == 0)//自身及以上都没有setting，将BreakInheritFromParent置成false
                    {
                        term.BreakInheritFromParent = false;
                    }
                    else
                    {
                        term.BreakInheritFromParent = true;
                    }
                }
                try
                {
                    //this.Update(term);
                    context.SaveChanges();
                    if ((settingInfo.EnforceRetention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange)
                    {
                        await CreateOrUpdateRetentionLabelAsync(settingInfo.EXORetentionLabel, RMRetentionSourceType.Exchange);
                    }

                    if ((settingInfo.EnforceRetention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint)
                    {
                        await CreateOrUpdateRetentionLabelAsync(settingInfo.SPRetentionLabel, RMRetentionSourceType.SharePoint);
                    }

                    if ((settingInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                    {
                        await CreateOrUpdateRetentionLabelAsync(settingInfo.OneDriveRetentionLabel, RMRetentionSourceType.OneDrive);
                    }

                    if ((settingInfo.EnforceRetention & (int)EnforceRetentionType.Teams) == (int)EnforceRetentionType.Teams)
                    {
                        await CreateOrUpdateRetentionLabelAsync(settingInfo.TeamsRetentionLabel, RMRetentionSourceType.Teams);
                    }

                    logger.Info("save term is success");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            logger.Error($"Property: {validationError.PropertyName},Error: {validationError.ErrorMessage}");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("update term throw error {0}", e);
                }
                term.IsExpired = IsExpiredTerm(term.Id);
                DeleteUselessAttributes(term);
                return term;
            }

        }
        public async Task<RMTerm> RenameTermAsync(int termId, string termName, int termSetId)
        {
            using var context = GetNewContext();
            RMTerm term = context.Terms.AsQueryable().Where(t => t.Id == termId).First();
            if (term.IsDeprecated || IsExpiredTerm(termId))
            {
                throw new Exception("Term is Deprecated");
            }
            CheckTermInSet(termId, termSetId);
            RMTermSetMembership ship = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).First();
            if (ReNameHasSameNameTerm(termId, termName, ship.ParentTermId, termSetId))
            {
                throw new Exception("Term has same name");
            }
            ship.TermName = termName;
            await TermSetMemebership.UpdateAsync(ship);
            term.Name = termName;
            await this.UpdateAsync(term);
            term.subTermCount = SubTermCount(termId);
            return term;
        }
        public RMTerm EnableTerm(int termId)
        {
            using (var context = GetNewContext())
            {
                RMTerm term = context.Terms.AsQueryable().Where(t => t.Id == termId).FirstOrDefault();
                term.IsDeprecated = false;
                if (term.TermExpirationTo != 0 || term.TermExpirationFrom != 0)
                {
                    term.TermExpirationFrom = 0;
                    term.TermExpirationTo = 0;
                    //    term.TimeZoneId = string.Empty;
                    //    term.IsDayLight = false;
                }
                this.ApplyCurrentValues(context, term);
                return term;
            }
        }
        public RMTerm SetTermIsExpired(RMTerm term, RMTerm subTerm)
        {
            subTerm.IsExpired = GetIsExpiredByTermRetireSetting(subTerm.TermExpirationFrom, subTerm.TermExpirationTo);
            return subTerm;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="ruleId"></param>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        public RMTerm InheritSettingToParent(int termId, TermSettingsInfo settingInfo)
        {
            using (var ctx = GetNewContext())
            {
                var term = ctx.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).FirstOrDefault();
                List<RMTermRuleAssociation> rules = ctx.RMTermRuleAssociations.AsQueryable().Where(r => r.TermId.Equals(termId)).ToList();
                ctx.RMTermRuleAssociations.RemoveRange(rules);
                ctx.SaveChanges();
                if (settingInfo.des == null)
                {
                    term.Description = string.Empty;
                }
                else
                {
                    term.Description = settingInfo.des;
                }
                term.BreakInheritFromParent = false;
                term.RuleInfo = string.Empty;
                term.TermExpirationFrom = settingInfo.beginTimeForDB;
                term.TermExpirationTo = settingInfo.endTimeForDB;
                //term.IsDayLight = GeneralSetting.isShowDayLight;
                //term.TimeZoneId = GeneralSetting.TimeZoneId;
                term.EnforceRetention = 0;
                this.ApplyCurrentValues(ctx, term);
                term.subTermCount = SubTermCount(termId);
                //term.TermExpirationFromStr = string.Empty;
                //term.TermExpirationToStr = string.Empty;
                term.IsExpired = IsExpiredTerm(termId);
                return term;
            }

        }
        public RMTerm DeprecateTerm(int termId)
        {
            using (var context = GetNewContext())
            {
                RMTerm term = context.Terms.AsQueryable().Where(t => t.Id == termId).First();
                if (term.TermExpirationTo != 0 || term?.TermExpirationFrom != 0)
                {
                    term.TermExpirationFrom = 0;
                    term.TermExpirationTo = 0;
                    //term.TimeZoneId = string.Empty;
                    //term.IsDayLight = false;
                }
                term.IsDeprecated = true;
                this.ApplyCurrentValues(context, term);
                return term;
            }
        }

        public List<Guid> GetTermSetIdListByTermIds(List<int> termIds)
        {
            using (var context = GetNewContext())
            {
                var termSetIds = context.TermSetMemberships.Where(t => termIds.Contains(t.TermId) && !t.IsRemoved).Select(t => t.TermSetId);
                return context.TermSets.Where(t => termSetIds.Contains(t.Id)).Select(t => t.UniqueId).Distinct().ToList();
            }
        }
        #endregion

        #region Get full path (Id/Name) Method
        public string GetTermSetNamesPathByTermSetId(int termSetId)
        {
            using var context = GetNewContext();
            var termSetNamesPath = string.Empty;
            var termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
            if (termSet?.TermSetType == TermSetType.Physical)
            {
                termSetNamesPath = termSet.Name;
            }
            else
            {
                var termGroup = context.TermGruops.AsQueryable().Where(g => g.UniqueId.Equals(termSet.TermGroupId)).FirstOrDefault();
                termSetNamesPath = termGroup?.Name + "/" + termSet.Name;
            }
            return termSetNamesPath;
        }
        public string GetTermSetNamesPathByTermSetId(Guid termSetId)
        {
            using var context = GetNewContext();
            var termSetNamesPath = string.Empty;
            if (termSetId != Guid.Empty)
            {
                var termSet = context.TermSets.AsQueryable().Where(t => t.UniqueId.Equals(termSetId)).FirstOrDefault();
                if (termSet?.TermSetType == TermSetType.Physical)
                {
                    termSetNamesPath = termSet.Name;
                }
                else
                {
                    var termGroup = context.TermGruops.AsQueryable().Where(g => g.UniqueId.Equals(termSet.TermGroupId)).FirstOrDefault();
                    termSetNamesPath = termGroup?.Name + "/" + termSet.Name;
                }
            }
            return termSetNamesPath;
        }
        public string GetTermNamePath(int termId, bool forExport = false)
        {
            using var ctx = GetNewContext();
            var fullPath = "";
            try
            {
                var termMembership = ctx.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).First();
                var idPath = termMembership.Path;
                var termIds = idPath.Substring(idPath.IndexOf('/') + 1).Split('/').ToList().Select(o => Convert.ToInt32(o));
                var termSet = (from s in ctx.TermSets where s.Id == termMembership.TermSetId select s).FirstOrDefault();
                var termGroup = (from g in ctx.TermGruops where g.UniqueId == termSet.TermGroupId select g).FirstOrDefault();
                var termNames = from m in ctx.TermSetMemberships
                                where termIds.Contains(m.TermId)
                                orderby m.TermId
                                select m.TermName;
                if (forExport)
                {
                    fullPath = $"{termGroup?.Name}|{termSet?.Name}|{string.Join("|", termNames)}";
                }
                else
                {
                    fullPath = $"{termGroup?.Name}/{termSet?.Name}/{string.Join("/", termNames)}";
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error when get term name full path, id:{termId}, message:{ex}");
            }
            return fullPath;
        }
        public string GetTermNamesPathByTermId(Guid termId)
        {
            using (var ctx = GetNewContext())
            {
                var fullPath = "";
                try
                {
                    var membership = (from t in ctx.Terms
                                      join m in ctx.TermSetMemberships on t.Id equals m.TermId
                                      where t.UniqueId == termId
                                      select m).First();
                    var idPath = membership.Path;//start with termSetId, eg: 1/2/3
                    var termIds = idPath.Substring(idPath.IndexOf('/') + 1).Split('/').ToList().Select(o => Convert.ToInt32(o));
                    var termSet = (from s in ctx.TermSets where s.Id == membership.TermSetId select s).FirstOrDefault();
                    var termGroup = (from g in ctx.TermGruops where g.UniqueId == termSet.TermGroupId select g).FirstOrDefault();
                    var termNames = from m in ctx.TermSetMemberships
                                    where termIds.Contains(m.TermId)
                                    orderby m.TermId
                                    select m.TermName;
                    fullPath = $"{termGroup?.Name}/{termSet?.Name}/{string.Join("/", termNames)}";
                }
                catch (Exception ex)
                {
                    logger.Error($"An error when get term name full path, id:{termId}, message:{ex}");
                }
                return fullPath;
            }
        }
        public string GetTermIdPath(Guid termId)
        {
            using (var context = GetNewContext())
            {
                return (from a in context.TermSetMemberships
                        join b in context.Terms on a.TermId equals b.Id
                        where b.UniqueId == termId
                        select a.Path).FirstOrDefault();
            }
        }
        public string GetTermFullPathForDestroyReport(Guid termId)
        {
            using var context = GetNewContext();
            string termNamesPathTemp = string.Empty;
            var term = context.Terms.AsQueryable().Where(t => t.UniqueId.Equals(termId)).FirstOrDefault();
            if (term == null || term.IsExpired || term.IsRemoved)
            {
                var termStatus = string.Empty;
                if (term == null)
                {
                    termStatus = "Cannot find current term.";
                }
                else if (term.IsExpired)
                {
                    termStatus = "Current term is expired.";
                }
                else if (term.IsRemoved)
                {
                    termStatus = "Current term is removed.";
                }
                logger.Warn("Current term is not in valid state, skip report item. Term id: {0}, term status: {1}", termId.ToString(), termStatus);
                return string.Empty; ;
            }
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(term.Id)).First();
            var termPath = termMembership.Path;
            List<string> parentTermIds = termPath.Split('/').ToList();
            //var termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termMembership.TermSetId)).FirstOrDefault();
            //var termGroup = context.TermGruops.AsQueryable().Where(g => g.UniqueId.Equals(termSet.TermGroupId)).FirstOrDefault();
            //termNamesPathTemp = termGroup.Name + "/" + termSet.Name + "/";
            List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 1).ToList();
            var Terms = context.Terms.AsQueryable().Where(t => ids.Contains(t.Id.ToString())).ToList();
            foreach (var item in Terms)
            {
                termNamesPathTemp += item.Name + "/";
            }
            if (!string.IsNullOrEmpty(termNamesPathTemp))
            {
                termNamesPathTemp = termNamesPathTemp.TrimEnd('/');
            }
            return termNamesPathTemp;
        }
        public string GetTermFullPathByTermId(Guid termId)
        {
            using var context = GetNewContext();
            string termNamesPathTemp = string.Empty;
            var term = context.Terms.AsQueryable().Where(t => t.UniqueId.Equals(termId)).FirstOrDefault();
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(term.Id)).First();
            var termPath = termMembership.Path;
            List<string> parentTermIds = termPath.Split('/').ToList();
            //var termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termMembership.TermSetId)).FirstOrDefault();
            //var termGroup = context.TermGruops.AsQueryable().Where(g => g.UniqueId.Equals(termSet.TermGroupId)).FirstOrDefault();
            //termNamesPathTemp = termGroup.Name + "/" + termSet.Name + "/";
            List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 1).ToList();
            var Terms = context.Terms.AsQueryable().Where(t => ids.Contains(t.Id.ToString())).ToList();
            foreach (var item in Terms)
            {
                termNamesPathTemp += item.Name + ":";
            }
            if (!string.IsNullOrEmpty(termNamesPathTemp))
            {
                termNamesPathTemp = termNamesPathTemp.TrimEnd(':');
            }
            return termNamesPathTemp;
        }

        public Dictionary<int, string> GetTermFullPathByTermIds(List<int> termIds)
        {
            var termFullPathDic = new Dictionary<int, string>();
            using var context = GetNewContext();
            var terms = context.Terms.AsQueryable().Where(t => termIds.Contains(t.Id)).ToList();
            var termMembershipsDic = context.TermSetMemberships.AsQueryable().Where(t => termIds.Contains(t.TermId)).ToDictionary(t => t.TermId);
            var termSetsIdForBuildPath = termMembershipsDic.Values.Select(m => m.TermSetId);
            var termSetsForBuildPathDic = context.TermSets.AsQueryable().Where(ts => termSetsIdForBuildPath.Contains(ts.Id)).ToDictionary(ts => ts.Id);
            var termGroupsIdForBuildPath = termSetsForBuildPathDic.Values.Select(ts => ts.TermGroupId).ToList();
            var termGroupsForBuildPathDic = context.TermGruops.AsQueryable().Where(tg => termGroupsIdForBuildPath.Contains(tg.UniqueId)).ToDictionary(tg => tg.UniqueId);

            var termsIdForBuildPath = new List<string>();
            foreach (var term in terms)
            {
                if (termMembershipsDic.TryGetValue(term.Id, out var termMembership))
                {
                    List<string> parentTermIds = termMembership.Path.Split('/').ToList();
                    termsIdForBuildPath.AddRange(parentTermIds.Skip(1).Take(parentTermIds.Count - 1).ToList());
                }
            }
            termsIdForBuildPath = termsIdForBuildPath.Distinct().ToList();
            var termsForBuildPath = context.Terms.AsQueryable().Where(t => termsIdForBuildPath.Contains(t.Id.ToString())).ToList();

            foreach (var term in terms)
            {
                try
                {
                    if (termMembershipsDic.TryGetValue(term.Id, out var termMembership))
                    {
                        List<string> parentTermIds = termMembership.Path.Split('/').ToList();
                        List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 1).ToList();
                        var pathTerms = termsForBuildPath.Where(t => ids.Contains(t.Id.ToString())).OrderBy(t => t.Id).ToList();
                        var termNamesPathTemp = string.Empty;
                        var buildPathTermSet = termSetsForBuildPathDic[int.Parse(parentTermIds.First())];
                        termNamesPathTemp += termGroupsForBuildPathDic[buildPathTermSet.TermGroupId].Name + "/"; //build term group name
                        termNamesPathTemp += buildPathTermSet.Name + "/"; //build term set name
                        foreach (var item in pathTerms)
                        {
                            termNamesPathTemp += item.Name + "/";
                        }
                        if (!string.IsNullOrEmpty(termNamesPathTemp))
                        {
                            termNamesPathTemp = termNamesPathTemp.TrimEnd('/');
                        }
                        termFullPathDic.TryAdd(term.Id, termNamesPathTemp);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"build term full path error: {e}");
                }
            }
            return termFullPathDic;
        }

        public RMTerm GetActiveTermById(int termId)
        {
            using (var context = GetNewContext())
            {
                var term = context.Terms.FirstOrDefault(item => item.Id == termId && !item.IsRemoved);
                if (term == null)
                {
                    return null;
                }

                if (GetIsExpiredByTermRetireSetting(term.TermExpirationFrom, term.TermExpirationTo))
                {
                    return null;
                }

                term.FullPath = GetTermNamesPathByTermId(term.UniqueId);

                return term;
            }
        }
        #endregion

        #region Operate Google term

        public RMTerm CreateGoogleTerm(TermInfo dto, RMGoogleLabelInfo labelInfo)
        {
            using (var ctx = GetNewContext())
            {
                CheckTermInfo(dto, true);
                var pTermId = dto.ParentTermId;
                RMTerm term = new RMTerm
                {
                    Name = dto.TermName,
                    TermSetId = dto.TermSetId,
                    Description = dto.Description,
                    UniqueId = Guid.NewGuid(),
                    IsRootTerm = pTermId == 0,
                    IsDeprecated = (labelInfo.State == (int)State.Disabled),
                    AdvanceSettings = dto.AdvanceSetting,
                };
                lock (lockCreateTerm)
                {
                    var pTermMembership = ctx.TermSetMemberships.Where(o => o.TermSetId == term.TermSetId && o.TermId == pTermId).FirstOrDefault();
                    using (var tran = ctx.Database.BeginTransaction())
                    {
                        term = ctx.Terms.Add(term);
                        var termSetId = term.TermSetId;
                        ctx.SaveChanges();
                        var termId = term.Id;
                        UpdateTermName(term, ctx, true);
                        var termMembership = new RMTermSetMembership()
                        {
                            TermName = term.Name,
                            ParentTermId = pTermId,
                            TermId = termId,
                            TermSetId = termSetId,
                            Path = pTermMembership == null ? $"{termSetId}/{termId}" : $"{pTermMembership.Path}/{termId}"
                        };
                        labelInfo.TermUniqueId = term.UniqueId;
                        labelInfo.TermId = termId;
                        ctx.RMGoogleLabelInfo.Add(labelInfo);
                        ctx.TermSetMemberships.Add(termMembership);
                        ctx.SaveChanges();
                        tran.Commit();
                    }
                }
                term.HaveParentSetting = ParentTermHasSetting(term.Id);
                return term;
            }
        }

        public RMTerm UpdateGoogleTerm(int termId, bool breakInherit, TermInfo newDto, RMGoogleLabelInfo labelInfo = null)
        {
            using (var context = GetNewContext())
            {
                RMTerm termFromDB = context.Terms.FirstOrDefault(tm => tm.Id.Equals(termId));
                int oldTermSetId = -1;
                if (termFromDB != null)
                {
                    oldTermSetId = termFromDB.TermSetId;
                    termFromDB.Name = newDto.TermName;
                    termFromDB.Description = newDto.Description;
                    termFromDB.IsRemoved = false;
                    termFromDB.BreakInheritFromParent = breakInherit;
                    termFromDB.IsDeprecated = (labelInfo.State == (int)State.Disabled);
                    termFromDB.TermSetId = newDto.TermSetId;
                }
                UpdateTermName(termFromDB);
                this.ApplyCurrentValues(context, termFromDB);
                RMTermSetMembership termSetMembership = context.TermSetMemberships.FirstOrDefault(ts => ts.TermSetId == oldTermSetId && ts.TermId == termFromDB.Id);
                if (termSetMembership != null)
                {
                    termSetMembership.IsRemoved = false;
                    termSetMembership.TermName = newDto.TermName;
                    termSetMembership.TermSetId = termFromDB.TermSetId;
                    termSetMembership.Path = $"{termFromDB.TermSetId}/{termId}";
                }
                if (labelInfo != null)
                {
                    RMGoogleLabelInfo googleLabelInfo = context.RMGoogleLabelInfo.FirstOrDefault(li => li.LabelId == labelInfo.LabelId && li.TermUniqueId == termFromDB.UniqueId);
                    if (googleLabelInfo != null)
                    {
                        googleLabelInfo.LabelName = labelInfo.LabelName;
                        googleLabelInfo.LabelType = labelInfo.LabelType;
                        googleLabelInfo.Extension = labelInfo.Extension;
                        googleLabelInfo.State = labelInfo.State;
                    }
                }
                context.SaveChanges();
                return termFromDB;
            }
        }

        public List<RMTerm> GetRMTermsByLabelId(string labelId, bool includeRemoved = false)
        {
            using var context = GetNewContext();
            var terms = (from t in context.Terms
                        join l in context.RMGoogleLabelInfo
                        on t.UniqueId equals l.TermUniqueId
                        where l.LabelId == labelId && (includeRemoved || !t.IsRemoved)
                        select t).ToList();
            return terms;
        }

        public RMTerm GetRMTermByLabelId(string labelId, string tenantId, bool includeRemoved = false)
        {
            using var context = GetNewContext();
            var rmTermGroup = (from tg in context.TermGruops
                             join tgm in context.TermGroupMembership
                             on tg.UniqueId equals tgm.TermGroupId
                             where tgm.SiteUrl.Equals(tenantId, StringComparison.OrdinalIgnoreCase)
                             select tg)
                             .FirstOrDefault();
            if (rmTermGroup != null)
            {
                var rmTerm = (from t in context.Terms
                              join ts in context.TermSets
                              on t.TermSetId equals ts.Id
                              join l in context.RMGoogleLabelInfo
                              on t.UniqueId equals l.TermUniqueId
                              where ts.TermGroupId.Equals(rmTermGroup.UniqueId)
                              && l.LabelId == labelId && (includeRemoved || !t.IsRemoved)
                              select t)
                              .FirstOrDefault();
                return rmTerm;
            }

            return null;
        }

        public List<Guid> GetDeletedLableUniqueIds(string tenantId, Guid termGroupId, List<string> availableLabelIds)
        {
            List<Guid> deletedLabelIds = [];
            using var context = GetNewContext();
            deletedLabelIds = (from l in context.RMGoogleLabelInfo
                               join t in context.Terms
                               on l.TermUniqueId equals t.UniqueId
                               join ts in context.TermSets
                               on t.TermSetId equals ts.Id
                               where !availableLabelIds.Contains(l.LabelId)
                               && ts.TermGroupId == termGroupId && l.TenantId == tenantId
                               select l.UniqueId).ToList();

            return deletedLabelIds;
        }

        public void UpdateLabelState(Guid labelUniqueId, State state)
        {
            using var context = GetNewContext();
            var labelInfo = context.RMGoogleLabelInfo.AsQueryable().FirstOrDefault(l => l.UniqueId == labelUniqueId);
            if (labelInfo != null)
            {
                labelInfo.State = (int)state;
            }
            context.SaveChanges();
        }

        public bool TryGetGoogleLabelInfo(string uniqueTermId, out RMGoogleLabelInfo labelInfo, string tenantId, bool includeRemoved = false)
        {
            using var context = GetNewContext();
            labelInfo = (from t in context.Terms
                      join l in context.RMGoogleLabelInfo
                      on t.UniqueId equals l.TermUniqueId
                      where t.UniqueId == new Guid(uniqueTermId)
                      && (includeRemoved || t.IsRemoved == false)
                      && l.TenantId == tenantId
                      && l.State == (int)State.Published
                         select l).FirstOrDefault();
            if (labelInfo != null)
            {
                return true;
            }

            return false;
        }
        
        public Dictionary<Tuple<Guid, string>, RMGoogleLabelInfo> GetGoogleLabelInfos()
        {
            using var context = GetNewContext();
            return (from t in context.Terms
                join l in context.RMGoogleLabelInfo
                    on t.UniqueId equals l.TermUniqueId
                where l.State == (int)State.Published && t.IsRemoved == false
                select l).ToDictionary(item => new Tuple<Guid, string>(item.TermUniqueId, item.TenantId), item => item);
        }

        #endregion

        #region Private Method
        /// <summary>
        /// 包含所有的状态Sub Terms
        /// </summary>
        private TermTreeNode GetTermTreeNodeOfOrphanedTerm(RMTerm term, Guid parentId)
        {
            TermTreeNode termNode = null;
            if (term != null)
            {
                termNode = new TermTreeNode() { ID = term.UniqueId, ParentID = parentId, Children = new Dictionary<Guid, TermTreeNode>() };
                using (var context = GetNewContext())
                {
                    var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId == term.Id);
                    foreach (var subTermMembership in subTermMemberships)
                    {
                        RMTerm subTerm = this.Find(t => t.Id == subTermMembership.TermId);
                        var subTermNode = GetTermTreeNodeOfOrphanedTerm(subTerm, term.UniqueId);
                        if (subTermNode != null)
                        {
                            termNode.Children.Add(subTermNode.ID, subTermNode);
                        }
                    }
                }
            }
            return termNode;
        }
        private ParentTermSettings GetParentTermSettings(int termId)
        {
            ParentTermSettings ps = null;
            using var context = GetNewContext();
            if (null == context)
            {
                logger.Error("GetParentTermSettings method dbContext is null");
            }
            ArgumentCheck.NotNull(context, nameof(context));
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            if (null == termMembership)
            {
                logger.Error("GetParentTermSettings method termMembership is null");
            }
            ArgumentCheck.NotNull(termMembership, nameof(termMembership));
            var termPath = termMembership.Path;
            if (string.IsNullOrEmpty(termPath))
            {
                logger.Error("GetParentTermSettings method termPath is null ");
            }
            else
            {
                List<string> parentTermIds = termPath.Split('/').ToList();
                List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                if (null == ids || ids.Count == 0)
                {
                    return ps;
                }
                ids.Reverse();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });

                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent))
                {
                    var parTerm = context.Terms.AsQueryable().First(t => tIds.Contains(t.Id) && t.BreakInheritFromParent);
                    if (parTerm != null)
                    {
                        ps = new ParentTermSettings();
                        ps.RuleInfos = parTerm.RuleInfo;
                        ps.EnforceRetention = parTerm.EnforceRetention;
                        return ps;
                    }

                }


            }
            return ps;
        }
        private void DealWithRetentionLabel(RMTerm result)
        {
            var tempLabels = EXOLabelDao.GetLabelByStatus((int)RMRetentionLabelStatus.FromGUI).ToList();
            foreach (var item in tempLabels)
            {
                if (item.Type == (int)RMRetentionSourceType.Exchange)
                {
                    result.EXORetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                else if (item.Type == (int)RMRetentionSourceType.SharePoint)
                {
                    result.SPRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                else if (item.Type == (int)RMRetentionSourceType.OneDrive)
                {
                    result.OneDriveRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                if (item.Type == (int)RMRetentionSourceType.Teams)
                {
                    result.TeamsRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                else
                {
                    switch(item.Type)
                    {
                        case (int)RMRetentionSourceType.Teams:
                            result.TeamsRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private RMTerm CloneTerm(RMTerm oldTerm)
        {
            RMTerm newTerm = new RMTerm();
            newTerm.Name = oldTerm.Name;
            newTerm.Description = oldTerm.Description;
            newTerm.IsDeprecated = oldTerm.IsDeprecated;
            //newTerm.IsDayLight = oldTerm.IsDayLight;
            newTerm.IsRemoved = oldTerm.IsRemoved;
            newTerm.TermExpirationFrom = oldTerm.TermExpirationFrom;
            newTerm.TermExpirationTo = oldTerm.TermExpirationTo;
            //newTerm.TimeZoneId = oldTerm.TimeZoneId;
            newTerm.UniqueId = oldTerm.UniqueId;
            newTerm.TermSetId = oldTerm.TermSetId;
            newTerm.BreakInheritFromParent = oldTerm.BreakInheritFromParent;
            newTerm.Id = oldTerm.Id;
            newTerm.IsRootTerm = oldTerm.IsRootTerm;
            newTerm.EnforceRetention = oldTerm.EnforceRetention;
            DealWithRetentionLabel(newTerm);
            return newTerm;

        }
        private void GetInheritSubTerms(int termId, ref List<Guid> subTermIds)
        {
            using (var context = GetNewContext())
            {
                var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId == termId);
                foreach (var subTermMembership in subTermMemberships)
                {
                    int subTermId = subTermMembership.TermId;
                    var subTerm = this.Find(t => t.Id.Equals(subTermId));
                    if (subTerm != null && !subTerm.BreakInheritFromParent && !subTerm.IsRemoved)
                    {
                        //if (!subTerm.IsDeprecated)
                        //{
                        subTermIds.Add(subTerm.UniqueId);
                        //}
                        GetInheritSubTerms(subTermId, ref subTermIds);
                    }
                }
            }
        }

        private void GetInheritSubTerms(int termId, ref List<RMTerm> subTerms)
        {
            using (var context = GetNewContext())
            {
                var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId == termId);
                foreach (var subTermMembership in subTermMemberships)
                {
                    int subTermId = subTermMembership.TermId;
                    var subTerm = this.Find(t => t.Id.Equals(subTermId));
                    if (subTerm != null && !subTerm.BreakInheritFromParent && !subTerm.IsRemoved)
                    {
                        //if (!subTerm.IsDeprecated)
                        //{
                        subTerms.Add(subTerm);
                        //}
                        GetInheritSubTerms(subTermId, ref subTerms);
                    }
                }
            }
        }
        private void DeleteUselessAttributes(RMTerm term)
        {
            term.Description = "";
            term.EXORetentionLabel = "";
            term.SPRetentionLabel = "";
            term.OneDriveRetentionLabel = "";
        }
        private async Task CreateOrUpdateRetentionLabelAsync(string labelName, RMRetentionSourceType type)
        {

            var tempLabel = EXOLabelDao.GetLabel((int)type, (int)RMRetentionLabelStatus.FromGUI);
            if (tempLabel != null)
            {
                tempLabel.LabelName = labelName;
                tempLabel.SavedTime = DateTime.UtcNow.Ticks;
                await EXOLabelDao.UpdateAsync(tempLabel);
            }
            else
            {
                var label = new RMEXOLabel();
                label.LabelName = labelName;
                label.Status = (int)RMRetentionLabelStatus.FromGUI;
                label.Type = (int)type;
                label.SavedTime = DateTime.UtcNow.Ticks;
                EXOLabelDao.Create(label);
            }

        }

        private async Task<List<Guid>> QueryTermGroupIdsByNodeId(string nodeId, RMDbContext context)
        {
            var googleNode = await context.RMRemoteNodes.FirstOrDefaultAsync(node => node.Id == nodeId);
            if (googleNode == null)
            {
                return null;
            }

            if (googleNode.NodeLevel is (int)NodeLevel.GoogleMyDriveContainer
                or (int)NodeLevel.GoogleSharedDriveContainer)
            {
                var tenantIds = await context.RMRemoteNodes.Where(node => node.ParentId == nodeId)
                    .Select(item => item.TenantId).Distinct().ToListAsync();
                if (tenantIds.IsNullOrEmpty())
                {
                    var allTermGroupIds = await context.TermGroupMembership
                        .Where(item => item.SiteType == SiteType.Google).Select(item => item.TermGroupId)
                        .ToListAsync();
                    return allTermGroupIds.Count == 0 ? null : allTermGroupIds;
                }

                return await context.TermGroupMembership.Where(item => tenantIds.Contains(item.SiteUrl))
                    .Select(item => item.TermGroupId).ToListAsync();
            }

            var termGroupId =
                (await context.TermGroupMembership.FirstOrDefaultAsync(item => item.SiteUrl == googleNode.TenantId))
                ?.TermGroupId;
            if (termGroupId == null)
            {
                return null;
            }

            return [termGroupId.Value];
        }

        #endregion

        #region Validate Method
        public bool CheckTermExist(int parentTermId, string termName, int termSetId, out int termId)
        {
            bool exist = false;
            using var context = GetNewContext();
            termId = context.TermSetMemberships.AsQueryable().Where(t => t.TermSetId == termSetId
                                                                && t.ParentTermId == parentTermId
                                                                && t.TermName.Equals(termName, StringComparison.OrdinalIgnoreCase)
                                                                && t.IsRemoved == false)
                                                    .Select(t => t.TermId).FirstOrDefault();
            if (termId > 0)
            {
                exist = true;
            }
            return exist;
        }

        public bool CheckTermExistByLabelId(string labelId, Guid termGroupId, out int termId)
        {
            bool exist = false;
            using var context = GetNewContext();
            termId = (from l in context.RMGoogleLabelInfo
                     join t in context.Terms
                     on l.TermUniqueId equals t.UniqueId
                     join ts in context.TermSets
                     on t.TermSetId equals ts.Id
                     where l.LabelId == labelId && ts.TermGroupId == termGroupId
                     select t.Id).FirstOrDefault();
            // termId = context.RMGoogleLabelInfo.AsQueryable().FirstOrDefault(l => l.Id.Equals(labelId))?.TermId ?? -1;
            if (termId > 0)
            {
                exist = true;
            }
            return exist;
        }

        public RMTerm GetGoogleTermExistByTermId(int termId, int termSetId)
        {
            using var context = GetNewContext();
            var term = (from t in context.Terms
                        join l in context.RMGoogleLabelInfo
                        on t.UniqueId equals l.TermUniqueId
                        where l.TermId == termId && t.TermSetId == termSetId
                              select t).FirstOrDefault();

            return term;
        }

        public bool HasSameNameTerm(string termName, int parentTermId, int termSetId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Any(o => o.ParentTermId == parentTermId && o.TermName.Equals(termName) && o.TermSetId == termSetId && !o.IsRemoved);
        }
        public bool ReNameHasSameNameTerm(int termId, string termName, int parentTermId, int termSetId)
        {
            using var context = GetNewContext();
            return context.TermSetMemberships.AsQueryable().Any(t => t.ParentTermId == parentTermId && !t.TermId.Equals(termId) && t.TermName.Equals(termName) && t.TermSetId == termSetId && !t.IsRemoved);
        }

        public async Task<RMListLabelsResponse> GetPaginatedTermsAsync(LabelDisplayConditions conds, string tenantId)
        {
            var result = new RMListLabelsResponse();
            using (var context = GetNewContext())
            {
                var query = (from t in context.Terms
                             join l in context.RMGoogleLabelInfo
                             on t.UniqueId equals l.TermUniqueId
                             where l.TenantId == tenantId && !t.IsRemoved
                             select t);
                
                var count = query.Count();

                query = query.OrderBy(l => l.Name).Skip((conds.PageNumber - 1) * conds.PageSize).Take(conds.PageSize);
                result.Terms = await query.ToListAsync();
                result.TotalCount = count;
                result.TotalPage = count % conds.PageSize == 0
                                    ? count / conds.PageSize
                                    : count / conds.PageSize + 1;
                result.CurrentPage = conds.PageNumber;
                return result;
            }
        }

        private void CheckTermInSet(int termId, int termSetId)
        {
            if (termId > 0)
            {
                using var context = GetNewContext();
                if (!context.TermSetMemberships.Any(o => o.TermId == termId && o.TermSetId == termSetId && !o.IsRemoved))
                {
                    throw new Exception("Illegal params information");
                }
            }
        }
        private void CheckTermInfo(TermInfo dto, bool isGogleTerm = false)
        {
            using var ctx = GetNewContext();
            //check parent term status
            var pTerm = ctx.Terms.AsQueryable().Where(t => t.Id == dto.ParentTermId).FirstOrDefault();
            if (pTerm != null && (pTerm.IsDeprecated || IsExpiredTerm(dto.ParentTermId)))
            {
                throw new Exception("RM_TM_ParentTermRetire");
            }
            //check same name
            if (HasSameNameTerm(dto.TermName, dto.ParentTermId, dto.TermSetId) && !isGogleTerm)
            {
                throw new Exception("Term has same name");
            }
            CheckTermInSet(dto.ParentTermId, dto.TermSetId);
        }

        private void UpdateTermName(RMTerm term, Core.RMDbContext ctx = null, bool isCreatedNew = false)
        {
            // update Google term name if duplicate
            if (HasSameNameTerm(term.Name, 0, term.TermSetId) && isCreatedNew)
            {
                term.Name = $"{term.Name}_{term.Id}";
                if (ctx != null)
                {
                    this.ApplyCurrentValues(ctx, term);
                    ctx.SaveChanges();
                    return;
                }
            }
            else if (ReNameHasSameNameTerm(term.Id, term.Name, 0, term.TermSetId))
            {
                term.Name = $"{term.Name}_{term.Id}";
            }
        }
        #endregion

        #region Machine learning
        public List<RMTerm> GetWillTrainingTerms(string termLable, int pageIndex, int pageSize, out int totalCount, FilterTermObjOption filterOption = null)
        {
            List<RMTerm> terms = new List<RMTerm>();
            using var context = GetNewContext();
            var termSetIds = GetSecurityTermSetIds(Guid.Empty, filterOption);
            var exceptTermIds = context.RMMLTerms.Where(o => o.Status != (int)MLTermStatus.Removed).Select(o => o.Id).ToList();
            var query = context.Terms.AsQueryable().Where(tm => (tm.Name.Contains(termLable) || tm.Description.Contains(termLable)) && termSetIds.Contains(tm.TermSetId) && !exceptTermIds.Contains(tm.UniqueId) && !tm.IsRemoved);

            totalCount = query.Count();
            terms = query.OrderBy(o => o.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize).ToList();
            terms.ForEach(o => { o.FullPath = GetTermNamePath(o.Id); });
            return terms;
        }
        private List<int> GetSecurityTermSetIds(Guid termGroupId, FilterTermObjOption filterOption = null)
        {
            using var context = GetNewContext();
            List<int> termSetIds = new List<int>();
            var loadAllTerms = termGroupId.Equals(Guid.Empty) ? true : false;
            if (loadAllTerms)
            {
                if (filterOption != null && filterOption.NeedCheckPermission)
                {
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        UserAndGroupIds = filterOption.userAndGroupUserIds,
                        Level = SecurityTermLevel.TermGroup,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag
                    };
                    SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                    if (result.TermPermissionType == TermPermissionMethod.All)
                    {
                        termSetIds = context.TermSets.AsQueryable().Where(ts => (int)ts.TermSetType == (int)TermSetType.BusinessTerm && !ts.IsRemoved).Select(ts => ts.Id).ToList();
                    }
                    else if (result.TermPermissionType == TermPermissionMethod.SpecifyScope)
                    {
                        if (!result.TermObjIds.IsNullOrEmpty())
                        {
                            foreach (var groupId in result.TermObjIds)
                            {
                                List<RMTermSet> termset = GetRMTermSetsByGroupUniqueId(groupId, filterOption);
                                if (!termset.IsNullOrEmpty())
                                {
                                    List<int> tempTermSetIds = termset.Select(ts => ts.Id).ToList();
                                    termSetIds.AddRange(tempTermSetIds);
                                }
                            }
                        }
                    }
                }
                else
                {
                    termSetIds = context.TermSets.AsQueryable().Where(ts => (int)ts.TermSetType == (int)TermSetType.BusinessTerm && !ts.IsRemoved).Select(ts => ts.Id).ToList();
                }
            }
            else
            {
                termSetIds = context.TermSets.AsQueryable().Where(t => (int)t.TermSetType == (int)TermSetType.BusinessTerm && t.TermGroupId.ToString().Equals(termGroupId.ToString(), StringComparison.OrdinalIgnoreCase) && !t.IsRemoved).Select(ts => ts.Id).ToList();
            }
            return termSetIds;
        }

        public List<string> GetSettingDefaultTermNames(List<Guid> termIds)
        {
            using var context = GetNewContext();
            var sp_defaultTermIds = context.RMSharePointSettings.Where(o => !o.IsRemoved && o.EnableRecordManagement == 1 && o.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && termIds.Contains(o.DefaultTermId)).Select(o => o.DefaultTermId);
            var od_defaultTermIds = context.RMOneDriveSettings.Where(o => !o.IsRemoved && o.EnableRecordManagement == 1 && o.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && termIds.Contains(o.DefaultTermId)).Select(o => o.DefaultTermId);
            var allDefaultTermIds = sp_defaultTermIds.Concat(od_defaultTermIds).Distinct();
            return context.Terms.Where(o => allDefaultTermIds.Contains(o.UniqueId)).Select(o => o.Name).ToList();
        }

        public List<RMTerm> GetTermByTermGroupIdIncludeTermRemoved(Guid termGroupId)
        {
            using (var context = GetNewContext())
            {
                var terms = context.Terms
                    .Where(term => context.TermSets
                        .Where(termSet => termSet.TermGroupId == termGroupId)
                        .Select(termSet => termSet.Id)
                        .Contains(term.TermSetId))
                    .ToList();

                return terms;
            }
        }

        public async Task<RMGoogleLabelInfo> GetGoogleTermInfoByUniqueId(string uniqueId, string tenantId)
        {
            using var context = GetNewContext();
            var term = await context.RMGoogleLabelInfo.AsQueryable().Where(tm => tm.TermUniqueId.Equals(new Guid(uniqueId)) && tm.TenantId == tenantId).FirstOrDefaultAsync();
            if (term == null)
            {
                return null;
            }

            return term;
        }

        public RMTerm GetTermByNameAndScopeId(string termName, Guid scopeId)
        {
            using var context = GetNewContext();
            var remoteNode = context.RMRemoteNodes.Where(node => node.Id.Equals(scopeId.ToString())).FirstOrDefault();

            if (remoteNode == null) return null;

            var tenantId = remoteNode.TenantId;

            var termIds = context.Terms.AsQueryable().Where(tm => tm.Name.Equals(termName)).Select(t => t.Id).ToList();
            if (!termIds.Any()) return null;
            
            var labelInfo = context.RMGoogleLabelInfo.AsQueryable().Where(tm => tm.TenantId.Equals(tenantId) && termIds.Contains(tm.TermId)).FirstOrDefault();
            if (labelInfo == null)
            {
                return null;
            }

            var term = context.Terms.AsQueryable().Where(tm => tm.UniqueId.Equals(labelInfo.TermUniqueId)).First();
            return term;
        }
        
        public async Task<List<RMTermGroup>> GetPaginatedTermsStructureAsync(string nodeId, int pageIndex, int pageCount, List<Guid> groupids,
            List<string> userAndGroupIds, string searchKey = null)
        {
            List<RMTermGroup> termGroups = new();
            RMTermGroup termGroup = new();
            using var context = GetNewContext();
            var googleNode = await context.RMRemoteNodes.FirstOrDefaultAsync(node => node.Id == nodeId);
            if (googleNode == null)
            {
                return null;
            }

            if (googleNode.NodeLevel is (int)NodeLevel.GoogleMyDriveContainer
                or (int)NodeLevel.GoogleSharedDriveContainer)
            {
                var tenantIds = await context.RMRemoteNodes.Where(node => node.ParentId == nodeId)
                    .Select(item => item.TenantId).Distinct().ToListAsync();
                var addedTermGroupIds = new HashSet<Guid>();
                foreach (var tenantChildId in tenantIds)
                {
                    termGroup = await TermGroupDao.GetTermGroupTreeDataAsync(context, tenantChildId, pageIndex,
                        pageCount, groupids, userAndGroupIds, searchKey);
                    if (termGroup != null && !addedTermGroupIds.Contains(termGroup.UniqueId))
                    {
                        termGroups.Add(termGroup);
                        addedTermGroupIds.Add(termGroup.UniqueId);
                    }
                }

                return termGroups;
            }

            var tenantId = googleNode.TenantId;
            if (tenantId == null)
            {
                return null;
            }

            termGroup = await TermGroupDao.GetTermGroupTreeDataAsync(context, tenantId, pageIndex, pageCount, groupids, userAndGroupIds,
                searchKey);
            if (termGroup != null)
            {
                termGroups.Add(termGroup);
            }

            return termGroups;
        }
        
        public async Task<bool> CheckTermExistGoogleLabelInfor(List<Guid> scopeIds, Guid termUniqueId)
        {
            if (scopeIds == null || !scopeIds.Any())
            {
                return false;
            }

            using (var context = GetNewContext())
            {
                var tenantIds = await context.RMRemoteNodes.Where(node => scopeIds.Select(scopeId => scopeId.ToString()).Contains(node.Id))
                                                           .Select(node => node.TenantId)
                                                           .ToListAsync();

                if (!tenantIds.Any())
                {
                    return false;
                }

                var googleLabelExists = await context.RMGoogleLabelInfo.Where(label => label.TermUniqueId == termUniqueId && tenantIds.Contains(label.TenantId)).AnyAsync();

                return googleLabelExists;
            }
        }

        public async Task<Dictionary<Guid, string>> GetAllDeletedTermAndLabelByTenantId(string tenantId)
        {
            using var context = GetNewContext();
            return await context.Terms.Where(term => term.IsRemoved).Join(
                context.RMGoogleLabelInfo.Where(
                    label => label.TenantId == tenantId && label.State == (int)State.Deleted), 
                term => term.Id,
                label => label.TermId, 
                (term, label) => label).ToDictionaryAsync(label => label.TermUniqueId, label => label.LabelId);
        }

        public async Task<List<int>> GetActiveTermSets(List<int> termSetIds)
        {
            using var context = GetNewContext();
            return await context.TermSets.Where(ts => termSetIds.Contains(ts.Id) && !ts.IsRemoved)
                .Select(ts => ts.Id).ToListAsync();
        }

        #endregion

        private async Task UpdateDashboardChangeInfoAsync()
        {
            try
            {
                var dashboardSyncInfo = RMKeyValueDao.GetValueByKey(DASHBOARD_SYNC_CHANGE_INFO);
                var dashbaordChangeInfo = new DashboardSyncChangeLogger();
                if (dashboardSyncInfo == null)
                {
                    dashbaordChangeInfo.HasRuleAppliedTermChange = true;
                }
                else
                {
                    dashbaordChangeInfo = SerializerHelper.DeserializeByDataContractSerializer<DashboardSyncChangeLogger>(dashboardSyncInfo.Value);
                    dashbaordChangeInfo.HasRuleAppliedTermChange = true;
                }
                await RMKeyValueDao.SaveOrUpdateAsync(new RMKeyValue { Key = DASHBOARD_SYNC_CHANGE_INFO, Value = SerializerHelper.SerializeByDataContractSerializer(dashbaordChangeInfo) });
            }
            catch(Exception e)
            {
                logger.Error($"Update dashboard change info failed, error :{e}");
            }
        }

        public bool CheckTermDeletedByIds(List<Guid> termsIds)
        {
            using var context = GetNewContext();
            return context.Terms.Where(tm => termsIds.Contains(tm.UniqueId)).Any(t => t.IsRemoved);
        }

        public List<RMTermGroup> GetRMTermsBySearch(string termLable, List<Guid> termGroupIds, bool withRuleName, FilterTermObjOption filterOption = null)
        {
            List<RMTermGroup> termGroups = new List<RMTermGroup>();
            List<RMTermSet> termsets = new List<RMTermSet>();
            List<RMTerm> termTree = new List<RMTerm>();
            using var context = GetNewContext();
            List<int> termSetIds = new List<int>();
            logger.Info("search term lable is {0}", termLable);
            var loadAllTerms = !termGroupIds.Any();
            if (loadAllTerms)
            {
                if (filterOption != null && filterOption.NeedCheckPermission)
                {
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        UserAndGroupIds = filterOption.userAndGroupUserIds,
                        Level = SecurityTermLevel.TermGroup,
                        FilterByContentSource = filterOption.NeedCheckPermission,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag
                    };
                    SecurityTermPermissionDto result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
                    if (result.TermPermissionType == TermPermissionMethod.All)
                    {
                        termSetIds = context.TermSets.AsQueryable().Where(ts => (int)ts.TermSetType == (int)TermSetType.BusinessTerm && !ts.IsRemoved).Select(ts => ts.Id).ToList();
                    }
                    else if (result.TermPermissionType == TermPermissionMethod.SpecifyScope)
                    {
                        if (!result.TermObjIds.IsNullOrEmpty())
                        {
                            foreach (var groupId in result.TermObjIds)
                            {
                                List<RMTermSet> termset = GetRMTermSetsByGroupUniqueId(groupId, filterOption);
                                if (!termset.IsNullOrEmpty())
                                {
                                    List<int> tempTermSetIds = termset.Select(ts => ts.Id).ToList();
                                    termSetIds.AddRange(tempTermSetIds);
                                }
                            }
                        }
                    }
                }
                else
                {
                    termSetIds = context.TermSets.AsQueryable().Where(ts => (int)ts.TermSetType == (int)TermSetType.BusinessTerm && !ts.IsRemoved).Select(ts => ts.Id).ToList();
                }
            }
            else
            {
                termSetIds = context.TermSets.AsQueryable().Where(t => (int)t.TermSetType == (int)TermSetType.BusinessTerm && termGroupIds.Contains(t.TermGroupId) && !t.IsRemoved).Select(ts => ts.Id).ToList();
            }

            var terms = new List<int>();
            var tmPaths = new List<string>();

            if (withRuleName)
            {
                var rules = context.RMTermRuleAssociations.AsQueryable().Where(r => r.RuleName.Contains(termLable)).Select(t => t.TermId).ToList();
                terms = context.Terms.AsQueryable().Where(tm => (tm.Name.Contains(termLable) || tm.Description.Contains(termLable) || rules.Contains(tm.Id)) && termSetIds.Contains(tm.TermSetId) && !tm.IsRemoved).Select(t => t.Id).ToList();
                tmPaths = context.TermSetMemberships.AsQueryable().Where(t => terms.Contains(t.TermId)).OrderBy(o => o.TermSetId).ThenBy(t => t.ParentTermId).Select(ts => ts.Path).ToList();
            }
            else
            {
                terms = context.Terms.AsQueryable().Where(tm => tm.Name.Contains(termLable) && termSetIds.Contains(tm.TermSetId) && tm.IsRemoved == false).Select(t => t.Id).ToList();
                tmPaths = context.TermSetMemberships.AsQueryable().Where(t => terms.Contains(t.TermId)).OrderBy(o => o.TermSetId).ThenBy(t => t.ParentTermId).Select(ts => ts.Path).ToList();

            }

            var matchTermSets = context.TermSets.Where(o => termSetIds.Contains(o.Id) && o.Name.Contains(termLable) && !o.IsRemoved).ToList();

            int termSetId;
            foreach (var tmPath in tmPaths)
            {
                logger.Info("init tmPath:{0}", tmPath);
                #region init termset
                termSetId = Convert.ToInt32(tmPath.Split('/')[0]);
                if (matchTermSets.Any(o => o.Id.Equals(termSetId)))
                {
                    continue;
                }
                RMTermSet termSet;
                if (termsets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault() == null)
                {
                    termSet = context.TermSets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(termSet);
                    termSet.subTermCount = SubTermCountByTermSetId(termSet.Id);
                    termsets.Add(termSet);
                    termTree = new List<RMTerm>();
                    termSet.subTerms = termTree;
                }
                else
                {
                    termSet = termsets.AsQueryable().Where(t => t.Id.Equals(termSetId)).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(termSet);
                    termSet.subTerms = termTree;
                }
                logger.Info("init termset success,termset id is :{0}", termSet.Id);
                #endregion
                List<string> termIds = tmPath.Split('/').Skip(1).ToList();
                RMTerm rootTerm;
                bool haveParentSetting = false;
                int rootTermId = Convert.ToInt32(termIds[0]);
                logger.Info("Get rootTerm id :{0}", rootTermId);
                if (!termTree.AsQueryable().Any(t => t.Id.Equals(rootTermId)))
                {
                    rootTerm = context.Terms.AsQueryable().Where(tm => tm.Id.Equals(rootTermId)).FirstOrDefault();
                    rootTerm.subTermCount = SubTermCount(rootTermId);
                    logger.Info("Get rootTerm sub Term Count:{0}", rootTerm.subTermCount);
                    #region set str_timecolumn not mapped
                    rootTerm.TermExpirationFromStr = GetStrDateTime(rootTerm.TermExpirationFrom);
                    rootTerm.TermExpirationToStr = GetStrDateTime(rootTerm.TermExpirationTo);
                    #endregion
                    termTree.Add(rootTerm);
                }
                else
                {
                    rootTerm = termTree.AsQueryable().Where(t => t.Id.Equals(rootTermId)).FirstOrDefault();
                }
                SetTermIsExpired(null, rootTerm);
                haveParentSetting = rootTerm.BreakInheritFromParent;
                #region build term tree
                var tempTerm = new RMTerm();
                //last term node is rootterm load sun nodes
                if (tmPath == tmPaths[tmPaths.Count - 1] && 1 == termIds.Count)
                {
                    rootTerm.subTerms = GetTermFromParentTermWithoutDeletedTerm(rootTermId);
                }
                for (int i = 1; i < termIds.Count; i++)
                {
                    int subTermId = Convert.ToInt32(termIds[i]);
                    var subTerm = context.Terms.AsQueryable().Where(tf => tf.Id.Equals(subTermId)).FirstOrDefault();
                    logger.Info("Get subTerm name:{0},id:{1},fullpath:{2}", subTerm.Name, subTerm.Id, subTerm.FullPath);
                    subTerm.HaveParentSetting = haveParentSetting;
                    subTerm.subTermCount = SubTermCount(subTermId);
                    //last term node load sun nodes
                    if (tmPath == tmPaths[tmPaths.Count - 1] && i == termIds.Count - 1)
                    {
                        //subTerm.subTerms = GetTermFromParentTermWithoutDeletedTerm(subTermId);
                        BuildLastTermTree(subTerm);
                    }
                    #region set str_timecolumn not mapped
                    subTerm.TermExpirationFromStr = GetStrDateTime(subTerm.TermExpirationFrom);
                    subTerm.TermExpirationToStr = GetStrDateTime(subTerm.TermExpirationTo);
                    #endregion
                    if (!haveParentSetting)
                    {
                        haveParentSetting = ParentTermHasSetting(subTerm.Id);
                    }
                    if (i == 1)
                    {
                        tempTerm = BuildTermTree(rootTerm, subTerm);
                    }
                    else
                    {
                        tempTerm = BuildTermTree(tempTerm, subTerm);
                    }
                }
                logger.Info("Build Tree Success");
                #endregion
            }

            foreach (var matchTermSet in matchTermSets)
            {
                //TermSet匹配SearchKey，则把TermSet下的RootTerms全返回
                matchTermSet.subTerms = GetTermFromTermSet(matchTermSet.Id);
                matchTermSet.subTermCount = matchTermSet.subTerms.Count;
            }
            //匹配SearchKey的TermSet和匹配SearchKey的Term的ParentTermSet集合
            termsets = matchTermSets.Concat(termsets).ToList();

            List<RMTermGroup> groups = new List<RMTermGroup>();
            if (loadAllTerms)
            {
                groups = context.TermGruops.ToList();
            }
            else
            {
                groups = context.TermGruops.Where(g => termGroupIds.Contains(g.UniqueId) && !g.IsRemoved).ToList();
            }

            foreach (var group in groups)
            {
                var termSets = termsets.Where(t => t.TermGroupId.Equals(group.UniqueId)).ToList();
                if (loadAllTerms && termSets.Count == 0)
                {
                    continue;
                }
                group.subTerms = termSets;
                termGroups.Add(group);
            }
            return termGroups;
        }

        public List<RMTerm> SearchTermWithLimit(string searchValue, int limit)
        {
            using var context = GetNewContext();
            return context.Terms.Where(_ => !_.IsRemoved && !_.IsDeprecated && _.Name.Contains(searchValue)).AsEnumerable().Where(_ => !IsExpiredTerm(_.Id)).Take(limit).ToList();
        }

        public async Task<List<RMTerm>> GetTermFromTermSetUniqueId(Guid termSetId)
        {
            using (var context = GetNewContext())
            {
                var iTermSetId = context.TermSets.Where(t => t.UniqueId == termSetId).Select(t => t.Id).FirstOrDefault();
                if (iTermSetId > 0)
                {
                    return await context.Terms.Where(t => t.IsRemoved == false && t.TermSetId == iTermSetId && t.IsDeprecated == false).ToListAsync();
                }
            }
            return new List<RMTerm>();
        }

        public async Task<RMTerm> GetTermFromTermSetUniqueIdAndName(Guid termSetId, string termName)
        {
            using (var context = GetNewContext())
            {
                var iTermSetId = context.TermSets.Where(t => t.UniqueId == termSetId).Select(t => t.Id).FirstOrDefault();
                if (iTermSetId > 0)
                {
                    return await context.Terms.Where(t => t.IsRemoved == false && t.TermSetId == iTermSetId && t.IsDeprecated == false && t.Name == termName).FirstOrDefaultAsync();
                }
            }
            return null;
        }


        public async Task<RMTermSet> GetTermSetFromTermUniqueId(Guid termUniqueId)
        {
            using (var context = GetNewContext())
            {
                var query = from t in context.Terms
                            where t.UniqueId == termUniqueId
                            join ts in context.TermSets on t.TermSetId equals ts.Id
                            where !ts.IsRemoved
                            select ts;
                return await query.FirstOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<RMTerm>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.Terms.AsNoTracking().OrderBy(t => t.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertTermTableAsync(IEnumerable<RMTerm> terms)
        {
            using var context = GetNewContext();
            string tableName = "RMTerms";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = AvePoint.GCommon.Utility.SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);

                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, TermSetId, UniqueId, Name, Description, IsDeprecated, IsRemoved, BreakInheritFromParent, TimeZoneId, RuleInfo, TermExpirationFrom, TermExpirationTo, IsRootTerm, IsDayLight, AvailableSpace, IsDefaultTerm, EnforceRetention, EXORetentionLabel, SPRetentionLabel, OneDriveRetentionLabel, TeamsRetentionLabel, IsPermanent, AdvanceSettings) VALUES ");
                int i = 0;
                foreach (var item in terms)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10}, @p{paramIndex + 11}, @p{paramIndex + 12}, @p{paramIndex + 13}, @p{paramIndex + 14}, @p{paramIndex + 15}, @p{paramIndex + 16}, @p{paramIndex + 17}, @p{paramIndex + 18}, @p{paramIndex + 19}, @p{paramIndex + 20}, @p{paramIndex + 21}, @p{paramIndex + 22})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.TermSetId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", item.UniqueId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", item.Name));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", (object)item.Description ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", item.IsDeprecated));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", item.IsRemoved));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 7}", item.BreakInheritFromParent));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 8}", (object)item.TimeZoneId ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 9}", (object)item.RuleInfo ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 10}", item.TermExpirationFrom));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 11}", item.TermExpirationTo));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 12}", item.IsRootTerm));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 13}", item.IsDayLight));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 14}", item.AvailableSpace));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 15}", item.IsDefaultTerm));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 16}", item.EnforceRetention));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 17}", (object)item.EXORetentionLabel ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 18}", (object)item.SPRetentionLabel ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 19}", (object)item.OneDriveRetentionLabel ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 20}", (object)item.TeamsRetentionLabel ?? DBNull.Value));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 21}", item.IsPermanent));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 22}", (object)item.AdvanceSettings ?? DBNull.Value));
                    paramIndex += 23;
                    i++;
                }

                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                logger.Error($"Insert RMTerms data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllTermAsync()
        {
            return await TruncateAllDataInTableAsync("RMTerms");
        }

        public async Task<List<RMTerm>> SearchLabelWithLimit(string searchValue, int limit)
        {
            using var context = GetNewContext();
            var queryTermGroups = context.TermGruops.Where(termGroup => !termGroup.IsRemoved);
            var termGroupIds = await context.TermGroupMembership.Where(item => item.SiteType == SiteType.Google)
                .Join(queryTermGroups,
                    termGroupMembership => termGroupMembership.TermGroupId,
                    termGroup => termGroup.UniqueId,
                    (termGroupMembership, termGroup) => termGroupMembership.TermGroupId).Distinct().ToListAsync();
            var termSetIds = await context.TermSets.Where(t => !t.IsRemoved && t.TermSetType == (int)TermSetType.BusinessTerm && termGroupIds.Contains(t.TermGroupId)).Select(_ => _.Id).Distinct().ToListAsync();
            return context.Terms.Where(_ => termSetIds.Contains(_.TermSetId) && !_.IsRemoved && !_.IsDeprecated && _.Name.Contains(searchValue)).AsEnumerable().Where(_ => !IsExpiredTerm(_.Id)).Take(limit).ToList();
        }
    }
}

