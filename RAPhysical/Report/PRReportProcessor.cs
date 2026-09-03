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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RAPhysical.Tree.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.RAPhysical.Report.Interface;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.JobMonitor;
using Newtonsoft.Json;
using AvePoint.RA.Common;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Report
{
    public class PRReportProcessor : IPRReportProcessor
    {
        private ReportOptions _options;
        private bool _jobHasException = false;
        private bool _jobHasStopped = false;
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(PRReportProcessor));

        private List<Func<IPhysicalRecord, Task>> _recordActions = new List<Func<IPhysicalRecord, Task>>();

        private List<Func<IPRTreeService,Task>> _treeActions = new List<Func<IPRTreeService, Task>>();

        private Func<string, Task<List<RMLocationProfileNode>>> _getTreeFun = null;
        public IRMReportService mRMReportService { get; set; }

        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        public IPRTreeService PRTreeService { get; set; }
        public ILocationManagementService LocationManagementService { get; set; }

        public IPRReportProcessor ConfigRecordAction(Func<IPhysicalRecord, Task> action)
        {
            _recordActions.Add(action);
            return this;
        }

        public IPRReportProcessor ConfigTreeAction(Func<IPRTreeService, Task> action)
        {
            _treeActions.Add(action);
            return this;
        }

        public IPRReportProcessor ConfigGetTreeFun(Func<string,Task<List<RMLocationProfileNode>>> getTreeFun)
        {
            _getTreeFun = getTreeFun;
            return this;
        }

        public void AddJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);
            ReportManager.Increase(1);
        }

        public void AddJobReport(BaseReport report)
        {
            ReportManager.SendJobReport(report);
            ReportManager.Increase(1);
        }

        public void BatchAddJobDetail(IEnumerable<JMJobDetails> details)
        {
            ReportManager.BatchSendJobDetail(details);
        }

        public async Task ProcessAsync(ReportOptions options)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    //init
                    Init(options);
                    //add other details, eg: term info details
                    if (options.OtherDetails != null && options.OtherDetails.Count > 0) {
                        BatchAddJobDetail(options.OtherDetails);
                    }
                    //process tree
                    List<RMLocationProfileNode> treeNodes = null;
                    if (options.IsUseBuildInGetTreeNodesFunc)
                    {
                        treeNodes = await GetTreeNodesAsync(options.ProfileId);
                    }
                    else
                    {
                        treeNodes = _getTreeFun == null ? null : await _getTreeFun(options.ProfileId);
                    }

                    if (treeNodes ==null || treeNodes.Count == 0)
                    {
                        mLog.Warn("No tree nodes found.");
                        return;
                    }

                    await ProcessTreeNodesAsync(treeNodes);
                }
            }
            catch (JobStopException)
            {
                mLog.Warn("This Job is stopped.");
                _jobHasStopped = true;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while runnning. ", e.ToString());
                _jobHasException = true;
                throw;
            }
            finally
            {
                var finalStatus = _jobHasStopped ? JobStatus.Stopped : _jobHasException ? JobStatus.FinishWithException : JobStatus.Finished;
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        public async Task ProcessRootLocationItself(IPhysicalLocation location)
        {
            //目前基本上所有的Physical Report不需要处理Location, 故不需要有逻辑；
            //如果以后需要加common逻辑，可以在这里添加逻辑
            ReportManager.IncreaseBase(1);
            //add logic here....
            ReportManager.Increase();
            //throw new NotImplementedException();
        }

        public async Task ProcessNormalLocationItself(IPhysicalLocation location)
        {
            //目前基本上所有的Physical Report不需要处理Location, 故不需要有逻辑；
            //如果以后需要加common逻辑，可以在这里添加逻辑
            ReportManager.IncreaseBase(1);
            //add logic here....
            ReportManager.Increase();
            //throw new NotImplementedException();
        }

        public async Task ProcessBottomLocationItself(IPhysicalLocation location)
        {
            //目前基本上所有的Physical Report不需要处理Location, 故不需要有逻辑；
            //如果以后需要加common逻辑，可以在这里添加逻辑
            ReportManager.IncreaseBase(1);
            //add logic here....
            ReportManager.Increase();
            //throw new NotImplementedException();
        }

        public async Task ProcessBoxItself(IPhysicalBox locatbox)
        {
            //目前各个report处理box和file的方式都不同，所以这目前不需要common方法，如果以后需要可以加在这里
            throw new NotImplementedException();
        }

        public async Task ProcessFileItself(IPhysicalFile file)
        {
            //目前各个report处理box和file的方式都不同，所以这目前不需要common方法，如果以后需要可以加在这里
            throw new NotImplementedException();
        }

        /// <summary>
        /// deal with the record in parallel.
        /// </summary>
        /// <param name="items"></param>
        public async Task ProcessRecordItems(IEnumerable<IPhysicalRecord> items)
        {
            if (items == null || items.Count() == 0) return;

            mLog.Info($"items count: {items.Count()}");
            ReportManager.IncreaseBase(items.LongCount());

            if (_options.ProcessRecordItemsInParallel)
            {
                AveTenantTasks.RunParallel(items,
                new System.Threading.CancellationTokenSource(),
                async item => {
                    await ProcessOneRecordItemAsync(item);
                });
            }
            else
            {
                foreach (var item in items)
                {
                    await ProcessOneRecordItemAsync(item);
                }
            }

        }

        #region private
        private void Init(ReportOptions options)
        {
            _options = options;

            //start report manager
            //ReportMangerFactory.Instance.Init(options.JobId, options.JobType, true);
            ReportManager.StartUpdateJobProgress();

            #region config builtin actions
            if (options.IsUseBuiltInRootLocationAction)
            {
                PRTreeService.ConfigRootLocationAction(ProcessRootLocationItself);
            }

            if (options.IsUseBuiltInNormalLocationAction)
            {
                PRTreeService.ConfigNormalLocationAction(ProcessNormalLocationItself);
            }

            if (options.IsUseBuiltInBottomLocationAction)
            {
                PRTreeService.ConfigBottomLocationAction(ProcessBottomLocationItself);
            }

            if (options.IsUseBuiltInBoxAction)
            {
                PRTreeService.ConfigBoxAction(ProcessBoxItself);
            }

            if (options.IsUseBuiltInFileAction)
            {
                PRTreeService.ConfigFileAction(ProcessFileItself);
            }

            if (options.IsUseBuiltInRecordsGroupAction)
            {
                PRTreeService.ConfigRecordGroupAction(ProcessRecordItems);
            }
            #endregion
        }

        private async Task<List<RMLocationProfileNode>> GetTreeNodesAsync(string profileId)
        {
            var profileDto = await mRMReportService.GetProfileByIdAsync(profileId);
            RMLocationProfileNode profileNode = JsonConvert.DeserializeObject<RMLocationProfileNode>(profileDto.Extension2);
            return new List<RMLocationProfileNode>() { profileNode };
        }

        private async Task ProcessTreeNodesAsync(List<RMLocationProfileNode> nodes)
        {
            await ActionHelper.ExecuteAsync(_treeActions, PRTreeService);

            await PRTreeService.ProcessAsync(nodes, _options.BrowseOptions);
        }

        private async Task ProcessOneRecordItemAsync(IPhysicalRecord item)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    await ActionHelper.ExecuteAsync(_recordActions, item);
                    ReportManager.Increase();
                }
            }
            catch (JobStopException)
            {
                _jobHasStopped = true;
                throw;
            }
            catch (Exception ex)
            {
                _jobHasException = true;
                mLog.Error($"An error occurred while processing physical record '{item?.Id}', reason : {ex.ToString()}.");
            }
        }

        #endregion
    }
}
