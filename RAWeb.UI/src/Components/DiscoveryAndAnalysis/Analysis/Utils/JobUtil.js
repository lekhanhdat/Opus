import { DiscoveryJobStatus } from "../Constants";

class JobUtil {
    static isRunning(jobInfo) {
        return !(
            jobInfo.status === DiscoveryJobStatus.Finished ||
            jobInfo.status === DiscoveryJobStatus.Failed ||
            jobInfo.status === DiscoveryJobStatus.Exception
        );
    }

    static isFailed(jobInfo) {
        return jobInfo.status === DiscoveryJobStatus.Failed;
    }
}

export default JobUtil;
