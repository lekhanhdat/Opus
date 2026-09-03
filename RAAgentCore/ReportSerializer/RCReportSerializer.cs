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
using AvePoint.GCommon;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using RAFileSystemCore.ReportSerializer.Details;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RAFileSystemCore.ReportSerializer
{

    /// <summary>
    /// save report entries to sqlite 
    /// usage:
    /// RCReportSerializer.Instance.Register(TYPE,"C:\TEST\ABC.RPT")
    /// RCReportSerializer.Save(details)
    /// </summary>
    public class ReportSerializer
    {  //写SQLITE db,支持并发比较麻烦，所以用单例模式
        private static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static RALogger RAlogger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static object locker = new object();
        private static ReportSerializer _instance;
        private AbstractReportWorker _underlayerReportWorkder;
        private AbstractDetailWorker _underlayerDetailsWorkder;
        private readonly static SimpleLocker _simpleLocker = new SimpleLocker(RAlogger);
        public string Location { get; set; }


        private ReportSerializer()
        {
        }
        public static ReportSerializer Instance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new ReportSerializer();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="location"></param>
        public void Register(JobType type, string location,string jobid)
        {
            Location = location;
            switch (type)
            {
                case JobType.FSBCSTermUsageReport:
                    _underlayerReportWorkder = new BCSTermUsageReportWorker();
                    break;
                case JobType.FSItemsFilesDueDisposal:
                    _underlayerReportWorkder = new DueDisposalReportWorker();
                    _underlayerDetailsWorkder = new DueDisposalDetailWorkder();
                    break;
                case JobType.FSCreateAndDestroyedFileReport:
                    _underlayerReportWorkder = new CreationAndDestructionReportWorker();
                    _underlayerDetailsWorkder = new CreationAndDestructionDetailWorker();
                    break;
                case JobType.FSDataSynchronization:
                case JobType.FSDataSynchronizationSchedule:
                    _underlayerDetailsWorkder = new FSCollectionDetailWorker();
                    break;
                case JobType.FSDisposal:
                case JobType.FSDisposalSchedule:
                case JobType.FSDisposalByClassCode:
                    _underlayerDetailsWorkder = new FSDisposalDetailWorker();
                    break;
                case JobType.SPOnPremTermSynchronization:
                case JobType.SPOnPremTermSynchronizationSchedule:
                    _underlayerDetailsWorkder = new TermSynchronizatoinJobDetailWorker();
                    break;
                case JobType.SPOnPremApplySetting:
                case JobType.SPOnPremApplySettingSchedule:
                    _underlayerDetailsWorkder = new SharePointSettingsJobDetailWorker();
                    break;
                case JobType.SPOnPremEnforceRuleAction:
                case JobType.SPOnPremEnforceRuleActionSchedule:
                    _underlayerDetailsWorkder = new OnPremiseSPEnforceRuleActionDetailWorker();
                    break;
                case JobType.SPOnPremDataSync:
                case JobType.SPOnPremDataSyncSchedule:
                    _underlayerDetailsWorkder = new CollectionDataJobDetailWorker();
                    break;
                case JobType.SPOnPremUniqueIDSettingFullSchedule:
                case JobType.SPOnPremUniqueIDSettingIncrementalSchedule:
                    _underlayerDetailsWorkder = new UniqueIDSettingJobDetailWorker();
                    break;
                case JobType.GlobalSearchAction:
                    _underlayerDetailsWorkder = new GlobalSearchActionJobDetailWorker();
                    break;
                case JobType.SPOnPremScanLocalNodes:
                    _underlayerDetailsWorkder = new OnPremiseSPScanLocalNodeJobDetialWorker();
                    break;
                case JobType.FSArchiverRestore:
                    _underlayerDetailsWorkder = new FSRestoreDetailWorker();
                    break;
                case JobType.FSRetain:
                    _underlayerDetailsWorkder = new FSRetainDetailWorker();
                    break;
                case JobType.FSRetainSimulate:
                    _underlayerDetailsWorkder = new FSRetainDashboardDetailWorker();
                    break;
                default:
                    break;
            }

        }


        public void SyncReport(IEnumerable<BaseReport> details)
        {
            if (details == null || details.Count() == 0) return;
            SimpleLocker.Locker syncLocker = _simpleLocker.GetLocker(Location);
            lock (syncLocker)
            {
                try
                {
                    _underlayerReportWorkder.SaveReportJobDatas(details, Location);
                }
                catch (Exception e)
                {
                    logger.Error($"sync report error:{e.ToString()}");
                }
            }

        }

        public void SyncDetail(IEnumerable<JMJobDetails> details)
        {
            if (details == null || details.Count() == 0) return;
            SimpleLocker.Locker syncLocker = _simpleLocker.GetLocker(Location);
            lock (syncLocker)
            {
                try
                {
                    _underlayerDetailsWorkder.SaveReportJobDatas(details, Location);
                }
                catch (Exception e)
                {
                    logger.Error($"sync detail error:{e.ToString()}");
                }
            }

        }

    }
}
