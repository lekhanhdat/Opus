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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.RuleUsageReport;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.RuleUsageReport.AuditHandler;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Common;
using AvePoint.GCommon.Utility;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RuleUsageReport
{
    [RACodeReview("Allen yin",comment:"一些不合理的地方需要代兴改完之后再review")]
    [Audit]
    public class RMRuleUsageReportService : RMServiceBase, IRMRuleUsageReportService
    {
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private ITermSetMembershipDao TermSetMembershipDao => PlatformWindsorManager.GetService<ITermSetMembershipDao>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        
        private RALogger logger = RALogger.GetInstance(typeof(RMRuleUsageReportService));

        private List<RMTerm> AllTerms;

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.RuleUsageSearch, BeforeHandler = typeof(RMRuleUsageReportBeforeAuditHandler), AfterHandler = typeof(RMRuleUsageReportAfterAuditHandler))]
        public async Task<List<RuleUsageInfo>> GetRuleUsageInfoByRuleIdAsync(string ruleId,string ruleName)
        {
            AllTerms = new List<RMTerm>();
            logger.Info("Get rule usage info by rule id,{0}",ruleId);
            List<RuleUsageInfo> returnList = new List<RuleUsageInfo>();
            List<int> termIds = TermRuleAssociationDao.GetTermIdsByRuleId(ruleId);
            if (termIds.Count == 0)
            {
                //returnList = await GetGoogleRuleUsageInfoByRuleIdAsync(ruleId, ruleName);
                //if(returnList.Count == 0)
                //{ 
                    logger.Info("Cannot get any rule usage info by rule id,{0}", ruleId); 
                //}
                return returnList;
            }
            Dictionary<int, RMTerm> totalTermIdMap = new Dictionary<int, RMTerm>();
            this.GetTotalRMTermIdByRootTermIds(termIds.ToArray<int>());
            List<int> allTermsetId = new List<int>();
            List<int> hasPermissionTermsetId = new List<int>();
            foreach (RMTerm totalTerm in AllTerms)
            {
                if (await CheckTermSetPermissionAsync(totalTerm.TermSetId, allTermsetId, hasPermissionTermsetId))
                {
                    totalTermIdMap.Add(totalTerm.Id, totalTerm);
                }
            }
            logger.Info("Get member ships.");
            List<RMTermSetMembership> memberShips = TermSetMembershipDao.GetRMTermSetMemberships(totalTermIdMap.Keys.ToArray<int>());

            Dictionary<int, TermInfo> id_Infos = this.QueryTermsByMemberShips(memberShips, totalTermIdMap);
            Dictionary<int, RMTermSet> termSetDic = new Dictionary<int, RMTermSet>();
            Dictionary<Guid, RMTermGroup> termGroupDic = new Dictionary<Guid, RMTermGroup>();
            //List<RMTermSet> rmTermSets = TermSetDao.LoadTermSet(TermSetType.Business);
            //if (rmTermSets == null || rmTermSets.Count == 0)
            //{
            //    logger.Error("There is no termSet in RMDB.");
            //    return new List<RuleUsageInfo>();
            //}
            foreach (RMTermSetMembership memberShip in memberShips)
            {
                string termName = string.Empty;
                string termStatus = string.Empty;
                string[] ids = memberShip.Path.Split('/');
                int termSetId = int.Parse(ids[0]);
                RMTermSet rt = null;
                RMTermGroup rg = null;
                if (!termSetDic.ContainsKey(termSetId))
                {
                    rt = TermSetDao.GetRMTermSet(termSetId);
                    if (rt != null)
                    {
                        if (!termGroupDic.ContainsKey(rt.TermGroupId))
                        {
                            rg = TermGroupDao.GetTermGroupByGuid(rt.TermGroupId);
                            termGroupDic.Add(rt.TermGroupId, rg);
                        }
                        else
                        {
                            rg = termGroupDic[rt.TermGroupId];
                        }

                    }
                    termSetDic.Add(termSetId, rt);
                }
                else
                {
                    rt = termSetDic[termSetId];
                    rg = termGroupDic[rt.TermGroupId];
                }
                if (rt != null && rg != null)
                {
                    StringBuilder fullPath = new StringBuilder(rg.Name + "/" + rt.Name);

                    if (ids.Length > 1)
                    {
                        for (int i = 1; i < ids.Length; i++)
                        {
                            TermInfo ti;
                            id_Infos.TryGetValue(int.Parse(ids[i]), out ti);
                            fullPath.Append("/" + ti.Name);
                            if (i == ids.Length - 1)
                            {
                                termName = ti.Name;
                                termStatus = ti.Status;
                            }
                        }
                        logger.Info("Build term full path,{0}", termName);
                    }
                    RuleUsageInfo ruleUsageInfo = new RuleUsageInfo();
                    ruleUsageInfo.RuleId = ruleId;
                    ruleUsageInfo.TermId = memberShip.TermId;
                    ruleUsageInfo.TermName = termName;
                    ruleUsageInfo.TermPath = fullPath.ToString();
                    ruleUsageInfo.TermStatus = termStatus;
                    returnList.Add(ruleUsageInfo);
                }

            }
            returnList = returnList.OrderBy(r => r.TermPath).ToList();
            return returnList;
        }

        async Task<bool> CheckTermSetPermissionAsync(int termsetId, List<int> allTermsetId, List<int> hasPermissionTermsetId)
        {
            bool isHasPermission = false;
            if (!allTermsetId.Contains(termsetId))
            {
                allTermsetId.Add(termsetId);
                RMTermSet termset = TermSetDao.GetRMTermSet(termsetId);
                if (termset != null)
                {
                    List<Guid> termSetId = new List<Guid>();
                    termSetId.Add(termset.UniqueId);
                    bool hasPermission = await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, SecurityTermLevel.TermSet, termSetId);
                    if (hasPermission)
                    {
                        hasPermissionTermsetId.Add(termsetId);
                    }
                }
            }
            if (allTermsetId.Contains(termsetId))
            {
                if (hasPermissionTermsetId.Contains(termsetId))
                {
                    isHasPermission = true;
                }
                else
                {
                    isHasPermission = false;
                }
            }
            return isHasPermission;
        }

        private struct TermInfo
        {
            public string Name;
            public string Status;
        }

        private Dictionary<int, TermInfo> QueryTermsByMemberShips(List<RMTermSetMembership> memberShips, Dictionary<int, RMTerm> totalTerms)
        {
            List<int> needQueryTermIds = new List<int>();
            foreach (RMTermSetMembership memberShip in memberShips)
            {
                string[] ids = memberShip.Path.Split('/');
                if (ids.Length > 1)
                {
                    for (int i = 1; i < ids.Length; i++)
                    {
                        if (!needQueryTermIds.Contains(int.Parse(ids[i])))
                        {
                            needQueryTermIds.Add(int.Parse(ids[i]));
                        }
                    }
                }
            }
            logger.Info("Query RMTerms by term ids.");
            List<RMTerm> rmTerms = TermDao.GetRMTermsByTermIds(needQueryTermIds.ToArray<int>());

            Dictionary<int, TermInfo> id_names = new Dictionary<int, TermInfo>();
            logger.Info("Build dictionary key = id , value = name .");
            foreach (RMTerm rmTerm in rmTerms)
            {
                if (totalTerms.Keys.Contains(rmTerm.Id))
                {
                    id_names.Add(rmTerm.Id, new TermInfo() { Name = rmTerm.Name, Status = GetTermStatus(totalTerms[rmTerm.Id]) });
                }
                else
                {
                    id_names.Add(rmTerm.Id, new TermInfo() { Name = rmTerm.Name, Status = GetTermStatus(rmTerm) });
                }
            }
            return id_names;
        }
        private string GetTermStatus(RMTerm term)
        {
            string status;
            long utcNow = DateTime.UtcNow.Ticks;
            if (term.IsDeprecated || (term.TermExpirationFrom > 0 && utcNow < term.TermExpirationFrom) || (term.TermExpirationTo > 0 && utcNow > term.TermExpirationTo))
            {
                status = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Retired");
            }
            else
            {
                status = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Avaliable");
            }

            return status;
        }

        /// <summary>
        /// 以一个termId数组为输入，获取所有继承rule的子term id 集合
        /// </summary>
        /// <param name="termIds"></param>
        /// <returns></returns>
        private void GetTotalRMTermIdByRootTermIds(int[] termIds)
        {
            List<RMTerm> rootTerms = TermDao.GetRMTermsByTermIds(termIds).AsQueryable().Where(t => !t.IsPermanent).ToList();
            AllTerms.AddRange(rootTerms);
            logger.Info("Get all termid with rule.");
            List<int> termWithRuleIds = TermRuleAssociationDao.GetTermIdWithRule();
            foreach (RMTerm rootTerm in rootTerms)
            {
                List<RMTerm> rmTerms = TermDao.GetTermFromParentTermForRuleUsageReport(rootTerm);
                if (rmTerms == null || rmTerms.Count == 0)
                {
                    continue;
                }
                List<RMTerm> selectTerms = new List<RMTerm>();
                foreach (RMTerm rmTerm in rmTerms)
                {
                    if (rmTerm.IsRemoved || rmTerm.BreakInheritFromParent || (termWithRuleIds != null && termWithRuleIds.Contains(rmTerm.Id)))
                    {
                        continue;
                    }
                    //if (rmTerm.TermExpirationFrom > 0 && rmTerm.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < rmTerm.TermExpirationFrom || DateTime.UtcNow.Ticks > rmTerm.TermExpirationTo)
                    //    {
                    //        continue;
                    //    }
                    //}
                    //else if (rmTerm.TermExpirationFrom > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < rmTerm.TermExpirationFrom)
                    //    {
                    //        continue;
                    //    }
                    //}
                    //else if (rmTerm.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks > rmTerm.TermExpirationTo)
                    //    {
                    //        continue;
                    //    }
                    //}
                    selectTerms.Add(rmTerm);
                    AllTerms.Add(rmTerm);
                }
                this.GetRMTerm(selectTerms, termWithRuleIds);
            }
            logger.Info("Finish get all terms.");
        }

        private void GetRMTerm(List<RMTerm> rmTerms, List<int> termWithRuleIds)
        {
            foreach (RMTerm rmTerm in rmTerms)
            {
                List<RMTerm> subTerms = TermDao.GetTermFromParentTermForRuleUsageReport(rmTerm);
                if (subTerms == null || subTerms.Count == 0)
                {
                    continue;
                }
                List<RMTerm> selectSubTerm = new List<RMTerm>();
                foreach (RMTerm subTerm in subTerms)
                {
                    if ( subTerm.IsRemoved || (termWithRuleIds != null && termWithRuleIds.Contains(subTerm.Id)))
                    {
                        continue;
                    }
                    //if (subTerm.TermExpirationFrom > 0 && subTerm.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < subTerm.TermExpirationFrom || DateTime.UtcNow.Ticks > subTerm.TermExpirationTo)
                    //    {
                    //        continue;
                    //    }
                    //}
                    //else if (subTerm.TermExpirationFrom > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks < subTerm.TermExpirationFrom)
                    //    {
                    //        continue;
                    //    }
                    //}
                    //else if (subTerm.TermExpirationTo > 0)
                    //{
                    //    if (DateTime.UtcNow.Ticks > subTerm.TermExpirationTo)
                    //    {
                    //        continue;
                    //    }
                    //}
                    selectSubTerm.Add(subTerm);
                    AllTerms.Add(subTerm);
                }
                this.GetRMTerm(selectSubTerm, termWithRuleIds);
            }
        }
        /// <summary>
        /// Rule Usage Report download
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="RuleId"></param>
        /// <param name="ruleName"></param>
        public async Task GenerateReportForRuleUsageReportAsync(string reportFilePath, string ruleId, string ruleName)
        {

            string[][] datas = null;
            int maxCountOfOneSheet = 65535;
            List<RuleUsageInfo> ruleUsageInfos = await this.GetRuleUsageInfoByRuleIdAsync(ruleId, ruleName);
            List<RuleUsageInfo> templeRuleUsageInfos = new List<RuleUsageInfo>();
            int jobReportTotalCount = ruleUsageInfos == null ? 0 : ruleUsageInfos.Count();
            logger.Debug("End load Rule Info:{0},{1},{2}", ruleId, ruleName, jobReportTotalCount);
            if (!File.Exists(reportFilePath))
            {
                File.Create(reportFilePath);
            }
            try
            {
                if (jobReportTotalCount > 0)
                {
                    for (int i = 1; i < ruleUsageInfos.Count() + 1; i++)
                    {
                        ArgumentCheck.NotNull(ruleUsageInfos, nameof(ruleUsageInfos));
                        if (templeRuleUsageInfos.Count != 0 && templeRuleUsageInfos.Count % maxCountOfOneSheet == 0)
                        {
                            templeRuleUsageInfos.Add(ruleUsageInfos[i - 1]);
                            templeRuleUsageInfos = InsertDataToExcel(reportFilePath, templeRuleUsageInfos, i, maxCountOfOneSheet, ruleName);
                        }
                        else
                        {
                            templeRuleUsageInfos.Add(ruleUsageInfos[i - 1]);
                        }
                    }
                    if (templeRuleUsageInfos.Count > 0)
                    {
                        InsertDataToExcel(reportFilePath, templeRuleUsageInfos, jobReportTotalCount, maxCountOfOneSheet, ruleName);
                    }
                }
                else
                {
                    datas = new string[1][];
                    datas[0] = new string[] { I18NEntity.GetString("RM_JS_RC_RUR_NoTermWithRule") };

                    ReportUtil.InsertWorksheet(reportFilePath, ruleName, datas);
                }
            }
            catch (Exception e)
            {
                logger.Debug("generate Report Erro Info:{0},{1}", e.Message, e.StackTrace);
            }
        }

        public List<RuleUsageInfo> InsertDataToExcel(string reportFilePath, List<RuleUsageInfo> ruleUsageInfos, int currentInsertCount, int maxCountOfOneSheet, string ruleName) 
        {
            string[][] datas = new string[ruleUsageInfos.Count()+1][];
            datas = AssembleRuleUsageReportHeaderTittle(datas);
            datas = ConvertRuleUsageReportToArray(ruleUsageInfos, datas);
            if (currentInsertCount <= maxCountOfOneSheet)
            {
                ReportUtil.InsertWorksheet(reportFilePath, ruleName, datas);
                ruleUsageInfos.Clear();
            }
            else
            {
                ReportUtil.InsertWorksheet(reportFilePath, ruleName + ruleUsageInfos.Count / maxCountOfOneSheet, datas);
                ruleUsageInfos.Clear();
            }
            return ruleUsageInfos;
        }

        public string[][] ConvertRuleUsageReportToArray(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            RuleUsageInfo reportInfo = null;
            int rowCount = 1;
            foreach (BaseReport report in reportDetails)
            {
                reportInfo = report as RuleUsageInfo;
                datas[rowCount] = new string[3];
                datas[rowCount][0] = reportInfo.TermName;
                datas[rowCount][1] = reportInfo.TermPath;
                datas[rowCount][2] = reportInfo.TermStatus;
                rowCount++;
            }
            return datas;
        }

        public string[][] AssembleRuleUsageReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_RUR_TermName");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_RUR_TermPath");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus");

            return datas;
        }
    }
}
