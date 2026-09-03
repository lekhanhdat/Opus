using AvePoint.GCommon.Contract.Common;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.CommonFilter.Rules
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OrphanedFolderRule : PolicyRuleBase
    {
    }
}
