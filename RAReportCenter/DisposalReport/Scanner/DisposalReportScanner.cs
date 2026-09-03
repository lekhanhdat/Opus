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
using Newtonsoft.Json;
using RAReportCenter.Manager;
using RAReportCenter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RAReportCenter.DisposalReport.Scanner
{
    public abstract class DisposalReportScanner<T> where T : SourceNeedReportNode, IChildrenNeedReportNode<T>
    {

        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IExplorerQueryService ExplorerQueryService = PlatformWindsorManager.GetService<IExplorerQueryService>();

        protected readonly DisposalReportModel reportInfo;

        protected abstract SourceFlag Source { get; }

        protected abstract IEnumerable<T> RemoveNoNeedReportNodes(IEnumerable<T> needReportNodes);

        protected abstract IEnumerable<T> ExpandHasChildrenNeedReportNodes(IEnumerable<T> needReportNodes);

        protected abstract List<ExplorerSearchOptionV3> GetNeedReportNodeQueryOptions(T reportNode);

        protected abstract string GetReportDataFullPath(BaseRecordDto data);

        protected abstract string GetReportLevelI18NKeyByNodeLevel(NodeLevel level);

        protected abstract RMReportObjectLevel GetReportDataObjectLvel(BaseRecordDto data);

        public DisposalReportScanner(DisposalReportModel reportInfo)
        {
            this.reportInfo = reportInfo;
        }

        public async Task ScanAsync()
        {
            var reportTreeNode = JsonConvert.DeserializeObject<T>(reportInfo.CheckedTreeStructure);
            var needReportNodes = FlattenAndFilterReportTreeNode(reportTreeNode);
            needReportNodes = ExpandHasChildrenNeedReportNodes(needReportNodes);
            needReportNodes = RemoveNoNeedReportNodes(needReportNodes);

            DisposalReportJobManager.IncreaseBase(needReportNodes.Count());

            foreach (var needReportNode in needReportNodes)
            {
                try
                {
                    await ScanNeedReportNodeAsync(needReportNode);
                    DisposalReportJobManager.AddSucceedJobDetail(needReportNode, GetReportLevelI18NKeyByNodeLevel(needReportNode.Level));
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while scan [{Source}] report node: [{needReportNode.Id} - {needReportNode.FullPath}]. Error: {e}");
                    DisposalReportJobManager.AddFailedJobDetail(needReportNode, GetReportLevelI18NKeyByNodeLevel(needReportNode.Level), e.Message);
                }
                finally
                {
                    DisposalReportJobManager.Increase();
                }
            }
        }

        private async Task ScanNeedReportNodeAsync(T reportNode)
        {
            Logger.Info($"Start scan [{Source}] report node: [{reportNode.Id} - {reportNode.FullPath}].");
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
            var queryOptions = GetNeedReportNodeQueryOptions(reportNode);
            var basicQueryOptions = GetBasicQueryOptions();
            queryDto.QueryOption.Values.AddRange(queryOptions);
            queryDto.QueryOption.Values.AddRange(basicQueryOptions);

            bool hasNextPage;
            do
            {
                var explorerResult = await ExplorerQueryService.QueryDataListWithoutTotalAsync(queryDto);
                var explorerData = explorerResult.Datas;
                await ProcessScannedDatasAsync(explorerData);

                hasNextPage = explorerResult.PagingInfo.HasNextPage;

            } while (hasNextPage);

            Logger.Info($"Successful scan [{Source}] report node: [{reportNode.Id} - {reportNode.FullPath}].");
        }

        private async Task ProcessScannedDatasAsync(List<BaseRecordDto> datas)
        {
            Logger.Info($"Start process scanned datas: [{datas.Count}].");
            foreach (var data in datas)
            {
                var report = new DueDisposalReport
                {
                    AppliedRuleId = data.RuleId.ToString(),
                    AppliedRuleName = data.RuleName,
                    TitleOrName = data.LeafName,
                    Url = GetReportDataFullPath(data),
                    BCSTermId = data.TermId.ToString(),
                    BCSTermName = data.TermName,
                    ObjectLevel = (int)GetReportDataObjectLvel(data),
                    CreatedBy = data.CreatedBy,
                    CreatedTime = data.TimeCreated,
                    LastModifiedBy = data.ModifiedBy,
                    LastModifiedTime = data.TimeLastModified,
                    ManualApproval = AvePoint.RA.Contract.RMRuleManageMent.RMDisposalManualApproval.No,
                    ExportType = AvePoint.RA.Contract.Object.RMExportTypeValue.None,
                    DisposalAction = -1,
                    DisposalClass = string.Empty,
                    RelatedRecords = data.RelatedRecords
                };

                ReportRuleModel ruleInfo;
                bool hasRule = false;
                (hasRule, ruleInfo) = await ReportRuleInfoManager.TryGetAsync(Source, data.RuleId.ToString());
                if (hasRule)
                {
                    report.ManualApproval = ruleInfo.EnableManualApprova ? AvePoint.RA.Contract.RMRuleManageMent.RMDisposalManualApproval.Yes :
                        AvePoint.RA.Contract.RMRuleManageMent.RMDisposalManualApproval.No;
                    report.ExportType = ruleInfo.ExportType;
                    report.DisposalAction = ruleInfo.RuleAction;
                    report.DisposalClass = ruleInfo.RuleDisposalClass;
                }

                DisposalReportJobManager.AddJobReport(report);
            }

            Logger.Info($"Successful process scanned datas.");
        }

        private List<ExplorerSearchOptionV3> GetBasicQueryOptions()
        {
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
                    Value = JsonConvert.SerializeObject(new DateInfo
                    {
                        Condition = DateCondition.Before,
                        Value1 = reportInfo.ApplyRuleBeforeTime,
                        TimeZoneId = "UTC"
                    }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.TimeCreated
                    }
                },
                new ExplorerSearchOptionV3
                {
                    Value = JsonConvert.SerializeObject(new List<string>
                    {
                        Guid.Empty.ToString()
                    }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.RuleId
                    }
                }
            };
        }

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
