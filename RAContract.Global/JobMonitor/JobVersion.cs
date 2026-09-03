namespace AvePoint.RA.Contract.JobMonitor
{
    public enum JobVersion
    {
        None = 0, // Default value for old job which is created before this field is added, and it means the job doesn't have version info.
        Merged = 1,
        UnMerged = 2,
    }
}
