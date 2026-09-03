using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMFileSystemSettingsCreateSubJobService
    {
        string CreateAndExecuteSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool canExecuteNow, string fullPath, string settingData);
        string CreateAndExecuteSubJobWithAudit(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool canExecuteNow, string fullPath, string settingData, out string subJobId);
        string CreateAndExecuteMyhubSubJobWithAudit(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool canExecuteNow, string fullPath, string settingData, out string subJobId);
    }
}