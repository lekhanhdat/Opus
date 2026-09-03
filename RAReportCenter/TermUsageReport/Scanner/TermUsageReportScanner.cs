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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ReportCenter.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using RAReportCenter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.TermUsageReport.Scanner
{
    public abstract class TermUsageReportScanner<T> where T : SourceNeedReportNode, IChildrenNeedReportNode<T>
    {

        protected readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IExplorerQueryService ExplorerQueryService = PlatformWindsorManager.GetService<IExplorerQueryService>();

        protected readonly TermUsageReportModel reportInfo;

        protected abstract SourceFlag Source { get; }

        private readonly ITermGroupDao TermGroupDao = PlatformWindsorManager.GetService<ITermGroupDao>();

        private readonly ITermSetDao TermSetDao = PlatformWindsorManager.GetService<ITermSetDao>();

        private readonly ITermDao TermDao = PlatformWindsorManager.GetService<ITermDao>();

        protected abstract IEnumerable<T> RemoveNoNeedReportNodes(IEnumerable<T> needReportNodes);

        protected abstract IEnumerable<T> ExpandHasChildrenNeedReportNodes(IEnumerable<T> needReportNodes);

        protected abstract List<ExplorerSearchOptionV3> GetNeedReportNodeQueryOptions(T reportNode);

        protected abstract string GetReportDataFullPath(BaseRecordDto data);

        protected abstract string GetReportLevelI18NKeyByNodeLevel(NodeLevel level);

        protected abstract RMReportObjectLevel GetReportDataObjectLvel(BaseRecordDto data);

        private Dictionary<Guid, string> TermFullPathDic = new Dictionary<Guid, string>();

        private readonly RMTermStatus TermStatus;

        public TermUsageReportScanner(TermUsageReportModel reportInfo)
        {
            this.reportInfo = reportInfo;

            if(reportInfo.TermUsageReportType == TermUsageReportType.Active)
            {
                TermStatus = RMTermStatus.Avaliable;
            }
            else if(reportInfo.TermUsageReportType == TermUsageReportType.Orphaned)
            {
                TermStatus = RMTermStatus.Removed;
            }
            else if(reportInfo.TermUsageReportType == TermUsageReportType.Retired)
            {
                TermStatus = RMTermStatus.Retired;
            }
            else
            {
                TermStatus = RMTermStatus.Invalid;
            }
        }

        public async Task ScanAsync()
        {
            var reportSourceTree = JsonConvert.DeserializeObject<T>(reportInfo.CheckedSourceTreeStructure);
            var needReportNodes = FlattenAndFilterReportTreeNode(reportSourceTree);
            needReportNodes = ExpandHasChildrenNeedReportNodes(needReportNodes);
            needReportNodes = RemoveNoNeedReportNodes(needReportNodes);
            var needFilterTerms = GetNeedFilterTerms();
            TermUsageReportJobManager.SendTermUsageDetails(needFilterTerms);
            TermFullPathDic = needFilterTerms.ToDictionary(item => item.UniqueId, item => item.FullPath);

            var basicQueryOptions = GetBasicQueryOptions(needFilterTerms);

            TermUsageReportJobManager.IncreaseBase(needReportNodes.Count());

            foreach (var needReportNode in needReportNodes)
            {
                try
                {
                    await ScanNeedReportNodeAsync(needReportNode, basicQueryOptions);
                    TermUsageReportJobManager.AddSucceedJobDetail(needReportNode, GetReportLevelI18NKeyByNodeLevel(needReportNode.Level));
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while scan [{Source}] report node: [{needReportNode.Id} - {needReportNode.FullPath}]. Error: {e}");
                    TermUsageReportJobManager.AddFailedJobDetail(needReportNode, GetReportLevelI18NKeyByNodeLevel(needReportNode.Level), e.Message);
                }
                finally
                {
                    TermUsageReportJobManager.Increase();
                }
            }
        }

        private async Task ScanNeedReportNodeAsync(T needReportNode, List<ExplorerSearchOptionV3> basicQueryOptions)
        {
            Logger.Info($"Start scan [{Source}] report node: [{needReportNode.Id} - {needReportNode.FullPath}].");
            var queryDto = BuildExplorerQueryDto(needReportNode, basicQueryOptions);

            bool hasNextPage;
            do
            {
                var explorerResult = await ExplorerQueryService.QueryDataListWithoutTotalAsync(queryDto);
                var explorerData = explorerResult.Datas;
                ProcessScannedDatas(explorerData);

                hasNextPage = explorerResult.PagingInfo.HasNextPage;

            } while (hasNextPage);

            Logger.Info($"Successful scan [{Source}] report node: [{needReportNode.Id} - {needReportNode.FullPath}].");
        }

        private void ProcessScannedDatas(List<BaseRecordDto> datas)
        {
            Logger.Info($"Start process scanned datas: [{datas.Count}].");
            foreach (var data in datas)
            {
                var report = new BCSTermUsageReport
                {
                    TitleOrName = data.LeafName,
                    Url = GetReportDataFullPath(data),
                    BCSTermId = data.TermId.ToString(),
                    BCSTermName = data.TermName,
                    BCSTermFullPath = TermFullPathDic[data.TermId],
                    TermStatus = TermStatus,
                    ObjectLevel = (int)GetReportDataObjectLvel(data),
                    CreatedBy = data.CreatedBy,
                    CreatedTime = data.TimeCreated,
                    LastModifiedBy = data.ModifiedBy,
                    LastModifiedTime = data.TimeLastModified,
                };

                TermUsageReportJobManager.AddJobReport(report);
            }

            Logger.Info($"Successful process scanned datas.");
        }

        private ExplorerQueryV3Dto BuildExplorerQueryDto(T needReportNode, List<ExplorerSearchOptionV3> basicQueryOptions)
        {

            var queryDto = new ExplorerQueryV3Dto
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = new List<ExplorerSearchOptionV3>()
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = "",
                    PageSize = 1000
                }
            };
            var queryOptions = GetNeedReportNodeQueryOptions(needReportNode);
            queryDto.QueryOption.Values.AddRange(queryOptions);
            queryDto.QueryOption.Values.AddRange(basicQueryOptions);

            return queryDto;
        }

        private List<ExplorerSearchOptionV3> GetBasicQueryOptions(List<RMTerm> needFilterTerms)
        {
            var termUniqueIds = needFilterTerms.Select(item => item.UniqueId).ToList();
            return new List<ExplorerSearchOptionV3>
            {
                new ExplorerSearchOptionV3
                {
                    Value = JsonConvert.SerializeObject(new List<SourceFlag> { Source }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.SourceFlag,
                    }
                },
                new ExplorerSearchOptionV3
                {
                    Value = JsonConvert.SerializeObject(new ExplorerFilterOptionV2
                    {
                        TermIds = termUniqueIds
                    }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.Term
                    }
                }
            };
        }

        private List<RMTerm> GetNeedFilterTerms()
        {
            if (reportInfo.TermUsageReportType == TermUsageReportType.Active)
            {
                return GetActiveTerms();
            }
            else if (reportInfo.TermUsageReportType == TermUsageReportType.Orphaned)
            {
                return GetOrphanedTerms();
            }
            else if (reportInfo.TermUsageReportType == TermUsageReportType.Retired)
            {
                return GetRetiredTerms();
            }
            return new List<RMTerm>();
        }

        #region Get Orphaned Terms Logic

        public List<RMTerm> GetOrphanedTerms()
        {
            return TermDao.GetOprhanedTerms();
        }

        #endregion

        #region Get Retired Terms Logic

        public List<RMTerm> GetRetiredTerms()
        {
            return TermDao.GetRetiredTerms();
        }

        #endregion

        #region Get Active Terms Logic

        private List<RMTerm> GetActiveTerms()
        {

            var result = new List<RMTerm>();

            var termTreeJson = reportInfo.CheckedTermTreeStructure;

            var termTree = JsonConvert.DeserializeObject<TermTreeNode>(termTreeJson);
            var termTreeNodeQueue = new Queue<TermTreeNode>();
            termTree.Children.ForEach(termTreeNodeQueue.Enqueue);
            while (termTreeNodeQueue.Any())
            {
                var node = termTreeNodeQueue.Dequeue();

                if (!ValidateTermTreeNode(node))
                {
                    continue;
                }

                if (node.Type == "TermSet" && node.CheckStatus == CheckStatus.Checked)
                {
                    var activeTerms = GetTermSetSubActiveTerms(node.Id);
                    result.AddRange(activeTerms);
                    continue;
                }
                else if (node.Type == "Term" && node.CheckStatus == CheckStatus.Checked)
                {
                    var activeTerms = GetTermAndSubActiveTerms(node.Id);
                    result.AddRange(activeTerms);
                    continue;
                }

                node.Children.ForEach(termTreeNodeQueue.Enqueue);
            }

            return result;
        }

        private bool ValidateTermTreeNode(TermTreeNode node)
        {
            bool ValidateTerm(int termId)
            {
                var term = TermDao.GetActiveTermById(termId);
                return term != null;
            }

            bool ValidateTermSet(int termSetId)
            {
                var termSet = TermSetDao.GetRMTermSet(termSetId);
                return termSet != null;
            }

            bool ValidateTermGroup(int termGroupId)
            {
                var termGroup = TermGroupDao.GetTermGroupById(termGroupId);
                return termGroup != null;
            }

            if (node.Type == "TermGroup" && !ValidateTermGroup(node.Id))
            {
                return false;
            }
            else if (node.Type == "TermSet" && !ValidateTermSet(node.Id))
            {
                return false;
            }
            else if (node.Type == "Term" && !ValidateTerm(node.Id))
            {
                return false;
            }

            return true;
        }

        private List<RMTerm> GetTermSetSubActiveTerms(int termSetId)
        {
            var result = new List<RMTerm>();
            var activeTerms = TermDao.GetActiveTermByTermSetId(termSetId);
            foreach (var activeTerm in activeTerms)
            {
                var terms = GetTermAndSubActiveTerms(activeTerm);
                result.AddRange(terms);
            }
            return result;
        }

        private List<RMTerm> GetTermAndSubActiveTerms(RMTerm term)
        {
            var result = new List<RMTerm>
            {
                term
            };

            var termQueue = new Queue<RMTerm>();
            termQueue.Enqueue(term);

            while (termQueue.Any())
            {
                var activeTerm = termQueue.Dequeue();
                var subActiveTerms = TermDao.GetActiveTermByParentId(activeTerm.Id);
                result.AddRange(subActiveTerms);
                subActiveTerms.ForEach(termQueue.Enqueue);
            }

            return result;
        }

        private List<RMTerm> GetTermAndSubActiveTerms(int termId)
        {
            var activeTerm = TermDao.GetActiveTermById(termId);
            return GetTermAndSubActiveTerms(activeTerm);
        }

        #endregion

        private IEnumerable<T> FlattenAndFilterReportTreeNode(T reportTreeNode)
        {
            var result = new List<T>();
            var nodeQueue = new Queue<T>();
            nodeQueue.Enqueue(reportTreeNode);

            while (nodeQueue.Count > 0)
            {
                var node = nodeQueue.Dequeue();
                if (node.Checkable && node.CheckStatus == CheckStatus.Checked)
                {
                    result.Add(node);
                }

                node.Children.ToList().ForEach(nodeQueue.Enqueue);
            }

            Logger.Info($"Need process disposal report node count: [{result.Count}].");

            return result;
        }
    }
}
