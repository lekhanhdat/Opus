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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.JobService
{
    public static class JobServiceUtility
    {
        public static int[] JobTypesHasSubJob = new int[]
        {
            (int)JobType.ApplySharePointSettings,
            (int)JobType.SharePointScheduleSetting,
            (int)JobType.UniqueIDSettingFullSchedule,
            (int)JobType.UniqueIDSettingIncrementalSchedule,
            (int)JobType.DataSynchronisation,
            (int)JobType.ItemsFilesDueDisposal,
            (int)JobType.RecordsExplorerMove,
            (int)JobType.BCSTermUsageReport,
            (int)JobType.OrphanedTermReport,
            (int)JobType.RetiredTermReport,
            (int)JobType.EnforceRetention,
            (int)JobType.CreateAndDestroyedFileReport,
            (int)JobType.EXOApplySetting,
            (int)JobType.EXOApplySettingSchedule,
            (int)JobType.EXODataSynchronisation,
            (int)JobType.EXORecordsDisposal,
            (int)JobType.EXOItemsFilesDueDisposalReport,
            (int)JobType.EXOTermUsageReport,
            (int)JobType.EXOOrphanedTermUsageReport,
            (int)JobType.EXORetiredTermUsageReport,
            (int)JobType.EXOEnforceRetention,
            (int)JobType.PhysicalSetPermission,
            (int)JobType.FSDataSynchronization,
            (int)JobType.FSDataSynchronizationSchedule,
            (int)JobType.FSDisposal,
            (int)JobType.FSDisposalSchedule,
            (int)JobType.FSDisposalByClassCode,
            (int)JobType.FSFolderChangeTerm,
            (int)JobType.FSFolderManageHold,
            (int)JobType.GlobalSearchAction,
            (int)JobType.SPOnPremTermSynchronization,
            (int)JobType.SPOnPremApplySetting,
            (int)JobType.SPOnPremApplySettingSchedule,
            (int)JobType.SPOnPremDataSync,
            (int)JobType.SPOnPremDataSyncSchedule,
            (int)JobType.SPOnPremUniqueIDSettingFullSchedule,
            (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule,
            (int)JobType.OneDriveDataSynchronisation,
            (int)JobType.OneDriveDataSynchronisationSchedule,
            (int)JobType.OneDriveEnforceRetention,
            (int)JobType.OneDriveItemsFilesDueDisposalReport,
            (int)JobType.OneDriveTermUsageReport,
            (int)JobType.OneDriveOrphanedTermUsageReport,
            (int)JobType.OneDriveRetiredTermUsageReport,
            (int)JobType.OneDriveCreateAndDestroyedFileReport,
            (int)JobType.SPOnPremScanLocalNodes,
            (int)JobType.Dashboard,
            (int)JobType.ExportSearchResult,
            (int)JobType.PhysicalLoanBox,
            (int)JobType.PhysicalReturnBox,
            (int)JobType.RecordsDisposal,
            (int)JobType.OneDriveRecordsDisposal,
            (int)JobType.RMArchiverBackup,
            (int)JobType.CleanUpDuplicateDatas,
            (int)JobType.RMEndUserArchiverBackup,
            (int)JobType.SpecifySitesArchiverBackup,
            (int)JobType.PhysicalLoanPick,
            (int)JobType.PhysicalDestructionPick,
            (int)JobType.MachineLearningReviewReclassify,
            (int)JobType.MachineLearningReviewApprove,
            (int)JobType.MachineLearningExportReportJob,
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverToSpoRestore,
            (int)JobType.SOPreScan,
            (int)JobType.ArchiverOutPlaceRestore,
            (int)JobType.DiscoverOptimization,
            (int)JobType.ArchiverByHSMXml,
            (int)JobType.DiscoveryAOSPOptimization,
            (int)JobType.DiscoveryPreScan,
            (int)JobType.DiscoveryPlanProOptimization,
            (int)JobType.DiscoveryPlanProScan,
            (int)JobType.StubOopRestore,
            (int)JobType.AOSPRestore,
            (int)JobType.ApprovalProcessArchive,
            (int)JobType.ExportSiteMetrics,
            (int)JobType.BoxDataSynchronisation,
            (int)JobType.BoxDataSynchronisationSchedule,
            (int)JobType.BoxRecordsDisposal,
            (int)JobType.BoxCreateAndDestroyedFileReport,
            (int)JobType.BoxItemsFilesDueDisposalReport,
            (int)JobType.BoxBCSTermUsageReport,
            (int)JobType.BoxRetiredTermUsageReport,
            (int)JobType.BoxOrphanedTermUsageReport,
            (int)JobType.ExportAdvanceSeachResult,
            (int)JobType.GoogleCreateAndDestroyedFileReport,
            (int)JobType.GoogleItemsFilesDueDisposalReport,
            (int)JobType.GoogleBCSTermUsageReport,
            (int)JobType.GoogleOrphanedTermUsageReport,
            (int)JobType.GoogleRetiredTermUsageReport,
            (int)JobType.ExportRestoreCenterSeachResult,
            (int)JobType.ConvertStub,
            (int)JobType.DeclaredRecordsMigration,
            (int)JobType.StubDisposal,
            (int)JobType.TeamsDataSynchronisation,
            (int)JobType.TeamsDataSynchronisationSchedule,
            (int)JobType.ApplyTeamsSettings,
            (int)JobType.TeamsUniqueIDSettingFullSchedule,
            (int)JobType.TeamsUniqueIDSettingIncrementalSchedule,
            (int)JobType.TeamsRecordsDisposal,
            (int)JobType.TeamsArchiverBackup,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.TeamsScheduleSetting,
            (int)JobType.TeamsArchiverRestore,
            (int)JobType.TeamsOutPlaceRestore,
            (int)JobType.MailBoxArchiverRestore,
            (int)JobType.TeamsEnforceRetention,
            (int)JobType.TeamsCreateAndDestroyedFileReport,
            (int)JobType.TeamsItemsFilesDueDisposalReport,
            (int)JobType.TeamsBCSTermUsageReport,
            (int)JobType.TeamsOrphanedTermUsageReport,
            (int)JobType.TeamsRetiredTermUsageReport,
            (int)JobType.TeamsPreScan,
            (int)JobType.GoogleArchiverRestore,
            (int)JobType.ApplyClassCode,
            (int)JobType.DeleteArchivedSiteCollection,
            (int)JobType.PhysicalMoveDataJob,
            (int)JobType.StubArchiverRestore,
            (int)JobType.M365InPlaceArchiverRestore,
        };
        public static int[] JobTypesHasSubJobAndDisposal = new int[]
        {
            (int)JobType.ApplySharePointSettings,
            (int)JobType.SharePointScheduleSetting,
            (int)JobType.UniqueIDSettingFullSchedule,
            (int)JobType.UniqueIDSettingIncrementalSchedule,
            (int)JobType.DataSynchronisation,
            (int)JobType.RecordsExplorerMove,
            (int)JobType.ItemsFilesDueDisposal,
            (int)JobType.BCSTermUsageReport,
            (int)JobType.OrphanedTermReport,
            (int)JobType.RetiredTermReport,
            (int)JobType.EnforceRetention,
            (int)JobType.CreateAndDestroyedFileReport,
            (int)JobType.EXOApplySetting,
            (int)JobType.EXOApplySettingSchedule,
            (int)JobType.EXODataSynchronisation,
            (int)JobType.EXORecordsDisposal,
            (int)JobType.EXOItemsFilesDueDisposalReport,
            (int)JobType.EXOTermUsageReport,
            (int)JobType.EXOOrphanedTermUsageReport,
            (int)JobType.EXORetiredTermUsageReport,
            (int)JobType.EXOEnforceRetention,
            (int)JobType.PhysicalSetPermission,
            (int)JobType.FSDataSynchronization,
            (int)JobType.FSDataSynchronizationSchedule,
            (int)JobType.FSDisposal,
            (int)JobType.FSDisposalSchedule,
            (int)JobType.FSDisposalByClassCode,
            (int)JobType.FSFolderChangeTerm,
            (int)JobType.FSFolderManageHold,
            (int)JobType.SPOnPremTermSynchronization,      
            (int)JobType.SPOnPremApplySetting,
            (int)JobType.SPOnPremApplySettingSchedule,
            (int)JobType.SPOnPremDataSync,
            (int)JobType.SPOnPremDataSyncSchedule,
            (int)JobType.SPOnPremUniqueIDSettingFullSchedule,
            (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule,
            (int)JobType.OneDriveDataSynchronisation,
            (int)JobType.OneDriveDataSynchronisationSchedule,
            (int)JobType.OneDriveEnforceRetention,
            (int)JobType.OneDriveItemsFilesDueDisposalReport,
            (int)JobType.OneDriveTermUsageReport,
            (int)JobType.OneDriveOrphanedTermUsageReport,
            (int)JobType.OneDriveRetiredTermUsageReport,
            (int)JobType.OneDriveCreateAndDestroyedFileReport,
            (int)JobType.SPOnPremScanLocalNodes,
            (int)JobType.Dashboard,
            (int)JobType.RecordsDisposal,
            (int)JobType.EXORecordsDisposal,
            (int)JobType.OneDriveRecordsDisposal,
            (int)JobType.RMArchiverBackup,
            (int)JobType.CleanUpDuplicateDatas,
            (int)JobType.RMEndUserArchiverBackup,
            (int)JobType.SpecifySitesArchiverBackup,
            (int)JobType.StubOopRestore,
            (int)JobType.AOSPRestore,
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverToSpoRestore,
            (int)JobType.PhysicalDestructionPick,
            (int)JobType.PhysicalDestructionPickExportJob,
            (int)JobType.PhysicalLoanPick,
            (int)JobType.PhysicalLoanPickExportJob,
            (int)JobType.PhysicalMovePickExportJob,
            (int)JobType.SOPreScan,
            (int)JobType.ArchiverOutPlaceRestore,
            (int)JobType.DiscoverOptimization,
            (int)JobType.ArchiverByHSMXml,
            (int)JobType.DiscoveryAOSPOptimization,
            (int)JobType.DiscoveryJob,
            (int)JobType.DiscoveryPreScan,
            (int)JobType.ApprovalProcessArchive,
            (int)JobType.ExportSiteMetrics,
            (int)JobType.BoxDataSynchronisation,
            (int)JobType.BoxDataSynchronisationSchedule,
            (int)JobType.BoxRecordsDisposal,
            (int)JobType.BoxCreateAndDestroyedFileReport,
            (int)JobType.BoxItemsFilesDueDisposalReport,
            (int)JobType.BoxBCSTermUsageReport,
            (int)JobType.BoxRetiredTermUsageReport,
            (int)JobType.BoxOrphanedTermUsageReport,
            (int)JobType.ExportAdvanceSeachResult,
            (int)JobType.GoogleCreateAndDestroyedFileReport,
            (int)JobType.GoogleItemsFilesDueDisposalReport,
            (int)JobType.GoogleBCSTermUsageReport,
            (int)JobType.ExportRestoreCenterSeachResult,
            (int)JobType.TeamsDataSynchronisation,
            (int)JobType.TeamsDataSynchronisationSchedule,
            (int)JobType.ApplyTeamsSettings,
            (int)JobType.TeamsUniqueIDSettingFullSchedule,
            (int)JobType.TeamsUniqueIDSettingIncrementalSchedule,
            (int)JobType.TeamsRecordsDisposal,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.TeamsArchiverBackup,
            (int)JobType.TeamsScheduleSetting,
            (int)JobType.MailBoxArchiverRestore,
            (int)JobType.TeamsArchiverRestore,
            (int)JobType.TeamsOutPlaceRestore,
            (int)JobType.TeamsEnforceRetention,
            (int)JobType.TeamsCreateAndDestroyedFileReport,
            (int)JobType.TeamsItemsFilesDueDisposalReport,
            (int)JobType.TeamsBCSTermUsageReport,
            (int)JobType.TeamsOrphanedTermUsageReport,
            (int)JobType.TeamsRetiredTermUsageReport,
            (int)JobType.TeamsPreScan,
            (int)JobType.GoogleArchiverRestore,
            (int)JobType.StubArchiverRestore,
            (int)JobType.M365InPlaceArchiverRestore,
        };

        public static int[] FSConnectionRelatedJobTypes = new int[]
        {
            (int)JobType.FSDataSynchronization,
            (int)JobType.FSDataSynchronizationSchedule,
            (int)JobType.FSDisposal,
            (int)JobType.FSDisposalSchedule,
            (int)JobType.FSDisposalByClassCode,
            (int)JobType.ApplyClassCode
        };

        public static int[] JobFinalStatus = new int[]
        {
            (int)JobStatus.Failed,
            (int)JobStatus.Finished,
            (int)JobStatus.FinishWithException,
            (int)JobStatus.Skipped,
            (int)JobStatus.Stopped
        };
       public static int[] JobFinalStatusAndCalculating = new int[]
       {
            (int)JobStatus.Failed,
            (int)JobStatus.Finished,
            (int)JobStatus.FinishWithException,
            (int)JobStatus.Skipped,
            (int)JobStatus.Stopped,
            (int)JobStatus.Calculating
       };

        private static JobStatus[] FinalState = new JobStatus[] {
            JobStatus.Failed,
            JobStatus.Finished,
            JobStatus.FinishWithException,
            JobStatus.Skipped,
            JobStatus.Stopped
        };
        private static Dictionary<int, IStatesObject> StateObjectDic = new Dictionary<int, IStatesObject>() {
            { (int)JobStatus.InProgress, new InProgressState()},
            { (int)JobStatus.Wait, new WaitingState()},
            { (int)JobStatus.Stopping, new StoppingState()},
            { (int)JobStatus.Stopped, new StoppedState()},
            { (int)JobStatus.Skipped, new SkipedState()},
            { (int)JobStatus.FinishWithException, new FinishWithExceptionState()},
            { (int)JobStatus.Finished, new FinishedState()},
            { (int)JobStatus.Failed, new FailedState()},
            { (int)JobStatus.None, new NoneState()}
             
        };

        public static HashSet<int> SkipMergeDetailsJobs = new HashSet<int>
        {
            (int)JobType.RMArchiverBackup,
            //(int)JobType.RMEndUserArchiverBackup,
            (int)JobType.SpecifySitesArchiverBackup,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.TeamsArchiverBackup,
            //(int)JobType.ArchiverByHSMXml,

            (int)JobType.DiscoveryPlanProOptimization,

            (int)JobType.RecordsDisposal,
            (int)JobType.OneDriveRecordsDisposal,
            (int)JobType.TeamsRecordsDisposal,
        };

        /// <summary>
        /// This is a set of job types that will have new job details and job progress.
        /// </summary>
        public static HashSet<int> NewJobDetailsJobs = new HashSet<int>
        {
            (int)JobType.RMArchiverBackup,
            //(int)JobType.RMEndUserArchiverBackup,
            (int)JobType.SpecifySitesArchiverBackup,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.TeamsArchiverBackup,
            //(int)JobType.ArchiverByHSMXml,

            (int)JobType.DiscoveryPlanProOptimization,

            (int)JobType.RecordsDisposal,
            (int)JobType.OneDriveRecordsDisposal,
            (int)JobType.TeamsRecordsDisposal,
        };

        public static HashSet<int> LowerProgressStatuses = new HashSet<int>
        {
            (int)ProgressStatus.Pending,
            (int)ProgressStatus.Finished,
            (int)ProgressStatus.Failed,
            (int)ProgressStatus.FinishWithException,
            (int)ProgressStatus.Stopped,
            (int)ProgressStatus.Skipped,
        };

    public static IStatesObject GetStateObject(int jobState)
        {
            if (StateObjectDic.ContainsKey(jobState))
            {
                return StateObjectDic[jobState];
            }
            else
            {
                return StateObjectDic[-1];
            }
        }

        public static bool IsSubJob(string jobId)
        {
            return jobId.IndexOf("_", StringComparison.OrdinalIgnoreCase) != -1;
        }

        public static bool IsFinalState(int state)
        {
            foreach (JobStatus m_type in FinalState)
            {
                if (state == (int)m_type)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFinalFailureState(int state)
        {
            return state == (int)JobStatus.Failed || state == (int)JobStatus.FinishWithException;
        }

        public static double CalcMainJobProgressIncrement(double subJobWeight, double subJobProgressIncrement)
        {
            double mainJobProgressIncrement = subJobWeight * (subJobProgressIncrement / 100);
            if (mainJobProgressIncrement < 0)
            {
                mainJobProgressIncrement = 0;
            }
            return mainJobProgressIncrement;
        }

        public static bool IsTeamsJob(int jobType)
        {
            return JobTypeConstants.TeamsJobTypes.Contains(jobType);
        }

        public static CacheNodeType GetCacheNodeType(int cacheNodeType)
        {
            CacheNodeType nodeType = CacheNodeType.Item;
            if (cacheNodeType == (int)CacheNodeType.Exception)
            {
                nodeType = CacheNodeType.Exception;
            }
            else if (cacheNodeType == (int)CacheNodeType.HSMItem)
            {
                nodeType = CacheNodeType.HSMItem;
            }
            else if (cacheNodeType == (int)CacheNodeType.HSMItemVersion)
            {
                nodeType = CacheNodeType.HSMItemVersion;
            }
            else if (cacheNodeType == (int)CacheNodeType.ArchiveBy365Item)
            {
                nodeType = CacheNodeType.ArchiveBy365Item;
            }
            else if (cacheNodeType > (int)CacheNodeType.ItemVersion)
            {
                nodeType = CacheNodeType.Attachment;
            }
            else if (cacheNodeType > (int)CacheNodeType.Item)
            {
                nodeType = CacheNodeType.ItemVersion;
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {
                nodeType = CacheNodeType.Item;
            }
            else if (cacheNodeType > (int)CacheNodeType.List)
            {
                nodeType = CacheNodeType.Folder;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                nodeType = CacheNodeType.List;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web)
            {
                if (cacheNodeType == (int)CacheNodeType.APP)
                {
                    nodeType = CacheNodeType.APP;
                }
                else
                {
                    nodeType = CacheNodeType.Web;
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                nodeType = CacheNodeType.SiteCollection;
            }
            return nodeType;
        }
    }

    #region State Object

    public interface IStatesObject
    {
        /// <summary>
        /// 改变job状态时，需要调用此方法来验证是否符合逻辑图，不符合则抛出异常，符合则返回此状态。
        /// </summary>
        /// <param name="toState">将要变成的状态</param>
        /// <returns>此状态通过验证，直接返回</returns>
        int validateState(int toState);
        /// <summary>
        ///  遍历所有子job来更新父job状态时，需要调用此方法。子类的实例是一个隐含的当前job的状态，通过每个子类的逻辑，逐个合并每个子job状态，遍历完后用最终的结果来更新父job状态。
        /// </summary>
        /// <param name="nextSubJobState">下一个子job的状态</param>
        /// <returns>合并后的新状态</returns>
        int coalesceState(int nextSubJobState);
    }
    /// <summary>
    /// 根据Sub Job计算Main job状态
    /// Lev1 InProgress, Started
    /// Lev2 Pending, Stopping, Pausing
    /// Lev3 Stopped, Paused
    /// Lev4 Finished, Failed, FinishWithException, Skipped
    /// Lev5 Waiting
    /// 
    /// 如果sub job中有Lev1的状态, 则main job就是lev1的状态;如果sub job中状态中有多个不同的Lev1状态, 则main job的状态是InProgress
    /// 如果sub job中无Lev1的状态, 有Lev2的状态, 则main job的状态是Lev2中的状态; 如果sub job中状态中有多个不同的Lev2状态, 则main job的状态是第一个参与计算的Lev2状态
    /// 如果sub job中无Lev1或Lev2的状态, 有Lev3的状态, 则main job的状态是Lev3中的状态; 如果sub job中状态中有多个不同的Lev3状态, 则main job的状态是FinishWithException
    /// 如果sub job中无Lev1, Lev2或Lev3的状态, 有Lev4的状态, 则main job的状态是Lev4中的状态; 如果sub job中状态中有多个不同的Lev4状态, 则main job的状态是第一个参与计算的Lev4状态
    /// 如果sub job中无Lev1, Lev2, Lev3或Lev4的状态, 有Lev5的状态, 则main job的状态是Lev5中的状态; 如果sub job中状态中有多个不同的Lev5状态, 则main job的状态是第一个参与计算的Lev5状态
    /// </summary>

    #region Lev1

    public class InProgressState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(InProgressState));

        public int validateState(int toState)
        {
            if (toState == (int)JobStatus.Stopped || toState == (int)JobStatus.Skipped)
            {
                logger.Warn("InProgressState change to " + toState);
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            return (int)JobStatus.InProgress;
        }
    }

    #endregion

    #region Lev2

    public class StoppingState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(StoppingState));

        public int validateState(int toState)
        {

            if (toState == (int)JobStatus.InProgress)
            {
                logger.Warn("StoppingState can not change to in progress.");
                return (int)JobStatus.Stopping;
            }
            else 
            {
                logger.Warn("StoppingState change to " + toState); 
            }
            
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.InProgress == nextSubJobState)
            {
                return nextSubJobState;
            }
            else
            {
                return (int)JobStatus.Stopping;
            }
        }
    }
    #endregion

    #region Lev3

    public class StoppedState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(StoppedState));

        public int validateState(int toState)
        {
            if (toState != (int)JobStatus.Stopped)
            {
                logger.Warn("StoppedState change to " + toState);
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.InProgress == nextSubJobState || (int)JobStatus.Stopping == nextSubJobState)
            {
                return nextSubJobState;
            }
            else if ((int)JobStatus.Wait == nextSubJobState)
            {
                return (int)JobStatus.InProgress;
            }
            else
            {
                return (int)JobStatus.Stopped;
            }
        }
    }
    #endregion

    #region Lev4

    public class FinishedState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(FinishedState));

        public int validateState(int toState)
        {
            if (toState != (int)JobStatus.Finished)
            {
                logger.Warn("FinishedState change to " + toState);

                return (int)JobStatus.Finished;
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.InProgress == nextSubJobState || (int)JobStatus.Stopping == nextSubJobState || (int)JobStatus.Stopped == nextSubJobState || (int)JobStatus.Skipped == nextSubJobState)
            {
                return nextSubJobState;
            }
            else
            {
                if ((int)JobStatus.Finished == nextSubJobState)
                {
                    return (int)JobStatus.Finished;
                }
                else if ((int)JobStatus.Failed == nextSubJobState || (int)JobStatus.FinishWithException == nextSubJobState)
                {
                    return (int)JobStatus.FinishWithException;
                }
                else if ((int)JobStatus.Wait == nextSubJobState)
                {
                    return (int)JobStatus.InProgress;
                }
                else
                {
                    return (int)JobStatus.Finished;
                }
            }

        }
    }
    public class FailedState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(FailedState));

        public int validateState(int toState)
        {
            if (toState != (int)JobStatus.Failed)
            {
                logger.Warn("FailedState change to " + toState);

                return (int)JobStatus.Failed;
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.InProgress == nextSubJobState || (int)JobStatus.Stopping == nextSubJobState || (int)JobStatus.Stopped == nextSubJobState)
            {
                return nextSubJobState;
            }
            else
            {
                if ((int)JobStatus.Finished == nextSubJobState || (int)JobStatus.FinishWithException == nextSubJobState)
                {
                    return (int)JobStatus.FinishWithException;
                }
                else if ((int)JobStatus.Skipped == nextSubJobState || (int)JobStatus.Failed == nextSubJobState)
                {
                    return (int)JobStatus.Failed;
                }
                else if ((int)JobStatus.Wait == nextSubJobState)
                {
                    return (int)JobStatus.InProgress;
                }
                else
                {
                    return (int)JobStatus.Failed;
                }
            }
        }
    }

    public class FinishWithExceptionState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(FinishWithExceptionState));

        public int validateState(int toState)
        {
            if (toState != (int)JobStatus.FinishWithException)
            {
                logger.Warn("FinishWithExceptionState change to " + toState);

                return (int)JobStatus.FinishWithException;
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.InProgress == nextSubJobState || (int)JobStatus.Stopping == nextSubJobState || (int)JobStatus.Stopped == nextSubJobState)
            {
                return nextSubJobState;
            }
            else
            {
                if ((int)JobStatus.Skipped == nextSubJobState || (int)JobStatus.Finished == nextSubJobState || (int)JobStatus.Failed == nextSubJobState || (int)JobStatus.FinishWithException == nextSubJobState || (int)JobStatus.Skipped == nextSubJobState)
                {
                    return (int)JobStatus.FinishWithException;
                }
                else if ((int)JobStatus.Wait == nextSubJobState)
                {
                    return (int)JobStatus.InProgress;
                }
                else
                {
                    return (int)JobStatus.FinishWithException;
                }
            }
        }
    }

    public class SkipedState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(SkipedState));

        public int validateState(int toState)
        {
            if (toState != (int)JobStatus.Skipped)
            {
                logger.Warn("Skipped State change to " + toState);

                return (int)JobStatus.Skipped;
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.InProgress == nextSubJobState || (int)JobStatus.Stopping == nextSubJobState ||
                (int)JobStatus.Stopped == nextSubJobState || (int)JobStatus.Skipped == nextSubJobState)
            {
                return nextSubJobState;
            }
            else
            {
                if ((int)JobStatus.Finished == nextSubJobState || (int)JobStatus.FinishWithException == nextSubJobState)
                {
                    return (int)JobStatus.FinishWithException;
                }
                else if ((int)JobStatus.Failed == nextSubJobState)
                {
                    return (int)JobStatus.Failed;
                }
                else if ((int)JobStatus.Wait == nextSubJobState)
                {
                    return (int)JobStatus.InProgress;
                }
                else
                {
                    return (int)JobStatus.Skipped;
                }
            }
        }
    }

    #endregion

    #region Lev5

    public class WaitingState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(WaitingState));

        public int validateState(int toState)
        {
            if (toState == (int)JobStatus.Stopped)
            {
                logger.Warn("WaitingState change to " + toState);
            }
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            if ((int)JobStatus.Finished == nextSubJobState || (int)JobStatus.Failed == nextSubJobState || (int)JobStatus.FinishWithException == nextSubJobState)
            {
                return (int)JobStatus.InProgress;
            }
            else
            {
                return nextSubJobState;
            }
        }
    }

    public class NoneState : IStatesObject
    {
        RALogger logger = RALogger.GetInstance(typeof(WaitingState));

        public int validateState(int toState)
        {
            logger.Warn("NoneState change to " + toState);
            return toState;
        }
        public int coalesceState(int nextSubJobState)
        {
            return nextSubJobState;
        }
    }
    #endregion 
    #endregion

}
