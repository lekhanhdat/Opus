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
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json;
using RAReportCenter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.CreateAndDestryoedReport.Scanner
{
    public abstract class CreateAndDestryoedReportScanner<T> where T : SourceNeedReportNode, IChildrenNeedReportNode<T>
    {

        protected readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IExplorerQueryService ExplorerQueryService = PlatformWindsorManager.GetService<IExplorerQueryService>();

        protected abstract SourceFlag Source { get; }

        protected abstract IEnumerable<T> RemoveNoNeedReportNodes(IEnumerable<T> needReportNodes);

        protected abstract IEnumerable<T> ExpandHasChildrenNeedReportNodes(IEnumerable<T> needReportNodes);

        protected abstract List<ExplorerSearchOptionV3> GetNeedReportNodeQueryOptions(T reportNode);

        protected abstract string GetReportDataFullPath(BaseRecordDto data);

        protected abstract string GetReportLevelI18NKeyByNodeLevel(NodeLevel level);

        protected abstract RMReportObjectLevel GetReportDataObjectLvel(BaseRecordDto data);

        protected readonly CreateAndDestryoedReportModel reportInfo;

        public CreateAndDestryoedReportScanner(CreateAndDestryoedReportModel reportInfo)
        {
            this.reportInfo = reportInfo;
        }

        public async Task ScanAsync()
        {
            var reportTreeNode = JsonConvert.DeserializeObject<T>(reportInfo.CheckedTreeStructure);
            var needReportNodes = FlattenAndFilterReportTreeNode(reportTreeNode);
            needReportNodes = ExpandHasChildrenNeedReportNodes(needReportNodes);
            needReportNodes = RemoveNoNeedReportNodes(needReportNodes);

            CreateAndDestryoedReportJobManager.IncreaseBase(needReportNodes.Count());

            var dataRange = GetNeedQueryDateRanage();
            var basicCreationQueryOptions = GetCreationQueryOptions(dataRange);
            var basicDestructionQueryOptions = GetDestructionQueryOptions(dataRange);

            foreach (var needReportNode in needReportNodes)
            {
                try
                {
                    if (reportInfo.ActionType.HasFlag(ActionType.Creation))
                    {
                        await ScanCreationDatasAsync(needReportNode, basicCreationQueryOptions);
                    }

                    if (reportInfo.ActionType.HasFlag(ActionType.Destruction))
                    {
                        await ScanDestructionDatasAsync(needReportNode, basicDestructionQueryOptions);
                    }
                    CreateAndDestryoedReportJobManager.AddSucceedJobDetail(needReportNode, GetReportLevelI18NKeyByNodeLevel(needReportNode.Level));
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while scan [{Source}] report node: [{needReportNode.Id} - {needReportNode.FullPath}]. Error: {e}");
                    CreateAndDestryoedReportJobManager.AddFailedJobDetail(needReportNode, GetReportLevelI18NKeyByNodeLevel(needReportNode.Level), e.Message);
                }
                finally
                {
                    CreateAndDestryoedReportJobManager.Increase();
                }
            }
        }

        private async Task ScanCreationDatasAsync(T needReportNode, List<ExplorerSearchOptionV3> basicCreationQueryOptions)
        {
            Logger.Info($"Start scan [{Source}] creation report node: [{needReportNode.Id} - {needReportNode.FullPath}].");

            var needReportNodeQueryOptions = GetNeedReportNodeQueryOptions(needReportNode);
            var queryDto = BuildBasicQueryDto(basicCreationQueryOptions, needReportNodeQueryOptions);

            bool hasNextPage;
            do
            {
                var explorerResult = await ExplorerQueryService.QueryDataListWithoutTotalAsync(queryDto);
                var explorerData = explorerResult.Datas;
                ProcessScannedDatas(explorerData, ActionType.Creation);
                hasNextPage = explorerResult.PagingInfo.HasNextPage;

            } while (hasNextPage);

            Logger.Info($"Successfule scan scan [{Source}] creation report node: [{needReportNode.Id} - {needReportNode.FullPath}].");
        }

        private async Task ScanDestructionDatasAsync(T needReportNode, List<ExplorerSearchOptionV3> basicDestructionQueryOptions)
        {
            Logger.Info($"Start scan [{Source}] destryoed report node: [{needReportNode.Id} - {needReportNode.FullPath}].");

            var needReportNodeQueryOptions = GetNeedReportNodeQueryOptions(needReportNode);
            var queryDto = BuildBasicQueryDto(basicDestructionQueryOptions, needReportNodeQueryOptions);

            bool hasNextPage;
            do
            {
                var explorerResult = await ExplorerQueryService.QueryDataListWithoutTotalAsync(queryDto);
                var explorerData = explorerResult.Datas;
                ProcessScannedDatas(explorerData, ActionType.Destruction);
                hasNextPage = explorerResult.PagingInfo.HasNextPage;

            } while (hasNextPage);

            Logger.Info($"Successfule scan scan [{Source}] destryoed report node: [{needReportNode.Id} - {needReportNode.FullPath}].");
        }

        private void ProcessScannedDatas(List<BaseRecordDto> datas, ActionType actionType)
        {
            Logger.Info($"Start process [{actionType}] scanned datas: [{datas.Count}].");

            string GetOperationTime(BaseRecordDto data)
            {
                if(actionType == ActionType.Creation)
                {
                    return data.TimeCreated.ToString();
                }
                else if(actionType == ActionType.Destruction)
                {
                    return data.DestryoedTime.ToString();
                }

                return "";
            }

            string GetOperationBy(BaseRecordDto data)
            {
                if (actionType == ActionType.Creation)
                {
                    return data.CreatedBy;
                }
                else if (actionType == ActionType.Destruction)
                {
                    return data.ModifiedBy;
                }

                return "";
            }

            int GetOperation()
            {
                if (actionType == ActionType.Creation)
                {
                    return 0;
                }
                else if (actionType == ActionType.Destruction)
                {
                    return 1;
                }

                return -1;
            }

            foreach (var data in datas)
            {
                var report = new CreateAndDestroyedFileReport
                {
                    Title = data.LeafName,
                    OperationTime = GetOperationTime(data),
                    OperationBy = GetOperationBy(data),
                    Operation = GetOperation(),
                    TermName = data.TermName,
                    DisposalClass = "",
                    Url = GetReportDataFullPath(data),
                    LevelStr = (int)GetReportDataObjectLvel(data)
                };

                CreateAndDestryoedReportJobManager.AddJobReport(report);
            }

            Logger.Info($"Successful process [{actionType}] scanned datas.");
        }

        public ExplorerQueryV3Dto BuildBasicQueryDto(List<ExplorerSearchOptionV3> basicQueryOptions, List<ExplorerSearchOptionV3> needReportNodeQueryOptions)
        {
            var queryOptions = new List<ExplorerSearchOptionV3>
            {
                GetSourceQueryOption()
            };

            queryOptions.AddRange(basicQueryOptions);
            queryOptions.AddRange(needReportNodeQueryOptions);

            return new ExplorerQueryV3Dto
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = queryOptions
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = "",
                    PageSize = 1000
                }
            };
        }

        private (string StartDateStr, string EndDateStr) GetNeedQueryDateRanage()
        {
            var now = DateTime.UtcNow;
            var endDate = new DateTime(now.Year, now.Month, now.Day);

            var startDateStr = now.ToString();
            var endDateStr = now.ToString();

            switch (reportInfo.DateFrameType)
            {
                case DateFrameType.CurrentWeek:
                    var day = (int)endDate.DayOfWeek - 1;
                    startDateStr = endDate.Subtract(TimeSpan.FromDays(day)).ToString();
                    break;
                case DateFrameType.CurrentMonth:
                    startDateStr = new DateTime(now.Year, now.Month, 1).ToString();
                    break;
                case DateFrameType.Last3Months:
                    startDateStr = endDate.AddMonths(-3).ToString();
                    break;
                case DateFrameType.Last6Months:
                    startDateStr = endDate.AddMonths(-6).ToString();
                    break;
                case DateFrameType.Custom:
                    startDateStr = reportInfo.CustomStartDate;
                    endDateStr = reportInfo.CustomEndDate;
                    break;
            }

            return (startDateStr, endDateStr);
        }

        private ExplorerSearchOptionV3 GetSourceQueryOption()
        {
            return new ExplorerSearchOptionV3
            {
                Value = JsonConvert.SerializeObject(new List<SourceFlag> { Source }),
                Column = new ExplorerQueryColumn
                {
                    Id = QueryCloumnIds.SourceFlag,
                }
            };
        }

        private List<ExplorerSearchOptionV3> GetCreationQueryOptions((string StartDateStr, string EndDateStr) dateRange)
        {
            if (!reportInfo.ActionType.HasFlag(ActionType.Creation))
            {
                return new List<ExplorerSearchOptionV3>();
            }

            return new List<ExplorerSearchOptionV3>
            {
                new ExplorerSearchOptionV3
                {
                    Value = JsonConvert.SerializeObject(new DateInfo
                    {
                        Condition = DateCondition.FromTo,
                        Value1 = dateRange.StartDateStr,
                        Value2 = dateRange.EndDateStr,
                        TimeZoneId = "UTC"
                    }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.TimeCreated
                    }
                }
            };
        }

        private List<ExplorerSearchOptionV3> GetDestructionQueryOptions((string StartDateStr, string EndDateStr) dateRange)
        {
            if (!reportInfo.ActionType.HasFlag(ActionType.Destruction))
            {
                return new List<ExplorerSearchOptionV3>();
            }

            return new List<ExplorerSearchOptionV3>
            {
                new ExplorerSearchOptionV3
                {
                    Value = JsonConvert.SerializeObject(new DateInfo
                    {
                        Condition = DateCondition.FromTo,
                        Value1 = dateRange.StartDateStr,
                        Value2 = dateRange.EndDateStr,
                        TimeZoneId = "UTC"
                    }),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.DestroyedTime
                    }
                },
                new ExplorerSearchOptionV3
                {
                    Value = JsonConvert.SerializeObject((int)RecordStatus.Destoryed),
                    Column = new ExplorerQueryColumn
                    {
                        Id = QueryCloumnIds.RecordStatus,
                    },
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
