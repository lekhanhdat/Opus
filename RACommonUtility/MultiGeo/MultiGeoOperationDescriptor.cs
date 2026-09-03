namespace AvePoint.RA.RACommonUtility.MultiGeo;

internal sealed class MultiGeoOperationDescriptor
{
    public MultiGeoOperationDescriptor(MultiGeoOperationType operationType, string replicaApiPath, bool isJobAction = false)
    {
        OperationType = operationType;
        ReplicaApiPath = replicaApiPath;
        IsJobAction = isJobAction;
    }

    public MultiGeoOperationType OperationType { get; }

    public string ReplicaApiPath { get; }

    public bool IsJobAction { get; }
}