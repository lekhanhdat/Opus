using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.Wrapper.Common;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// FakeDiscoverNodeWorker inherits from DiscoverNodeWorkerBase to test real scanner business logic.
    /// Does NOT override ProcessContainerAsync or ProcessItemAsync - those run real code.
    /// Only overrides IsSystemList (to avoid deep IAveList checks) and replaces mApprovalReportProxy
    /// with a no-op cache to prevent DB write attempts during testing.
    /// </summary>
    public class FakeDiscoverNodeWorker : DiscoverNodeWorkerBase
    {
        /// <summary>
        /// Tracks items that were transmitted to the approval report layer.
        /// </summary>
        public List<ArchiveApproveReport> TransmittedReports { get; } = new();

        public FakeDiscoverNodeWorker(
            ScanJobSettings jobSettings, 
            ScheduleConfiguration paraConfig, 
            IBackwardDependencyNodeCache<object> dependencyObjs)
            : base(jobSettings, paraConfig, dependencyObjs)
        {
            // Replace mApprovalReportProxy with a no-op cache to avoid DB writes
            // mApprovalReportProxy is internal, accessible via InternalsVisibleTo
            mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport>(
                new NoOpApprovalReportContainer());
        }

        /// <summary>
        /// Override IsSystemList to use only the IsSystemObject flag from test data.
        /// Avoids accessing real IAveList from mDependencyObjs for BaseTemplate/Hidden/Title checks.
        /// For tests that need to exercise the full IsSystemList logic, set DiscoverSPObject 
        /// on the node and populate mDependencyObjs with a FakeAveList instead.
        /// </summary>
        public override bool IsSystemList(ArchiverNodeItem item)
        {
            if (item.Cache_NodeType == (int)CacheNodeType.List)
            {
                if (item.IsSystemObject)
                {
                    return true;
                }

                // If a FakeAveList is in the dependency cache, use real logic
                var tmpList = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
                if (tmpList != null)
                {
                    if (tmpList.BaseTemplate.ToString().Equals("ExternalList", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    return (tmpList.Hidden || tmpList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase)) 
                        || (!tmpList.AllowDeletion && !systemListTable.Contains((int)tmpList.BaseTemplate));
                }

                return false;
            }
            return false;
        }
        public override bool ProcessListTypeRule(ArchiverNodeItem item)
        {
            return true;
        }
        // Hide Flush to prevent any lingering DB operations
        public new void Flush()
        {
        }
        
        // Hide Dispose for minimal cleanup in tests
        public new void Dispose()
        {
        }
    }

    /// <summary>
    /// No-op container for BackwardDependenceNodeCache that captures reports without writing to DB.
    /// Implements IScheduleContainer to satisfy BackwardDependenceNodeCache constructor.
    /// </summary>
    internal class NoOpApprovalReportContainer : IScheduleContainer<ArchiveApproveReport>
    {
        public List<ArchiveApproveReport> StoredReports { get; } = new();

        public void Store(ArchiveApproveReport node, bool hasReported)
        {
            StoredReports.Add(node);
        }

        public void AddReport(ArchiveApproveReport node)
        {
            StoredReports.Add(node);
        }

        public BackwardDependenceNode<ArchiveApproveReport> FetchNext()
        {
            return null;
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }
}
