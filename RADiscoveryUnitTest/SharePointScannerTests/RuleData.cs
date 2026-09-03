using System.Collections.Generic;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// JSON-serializable rule data for test scenarios.
    /// Each rule defines filter conditions that determine which items are processed.
    /// Maps to AvePoint.GCommon.Contract.StorageOptimization.Object.Rule at runtime.
    /// </summary>
    public class RuleData
    {
        public string Id { get; set; } = "1";
        public string Name { get; set; } = "Test Rule";

        /// <summary>
        /// Rule type: ADMIN, MANUAL, etc.
        /// Maps to AvePoint.GCommon.Contract.StorageOptimization.Object.RuleType
        /// </summary>
        public string Type { get; set; } = "ADMIN";

        /// <summary>
        /// PolicyLevel for the rule scope (Document=64, Folder=16, Item=32, List=8, Site=2, SiteCollection=1)
        /// </summary>
        public int PolicyLevel { get; set; }

        /// <summary>
        /// Filter policies defining the conditions for this rule.
        /// </summary>
        public List<FilterPolicyData> Filters { get; set; } = new();

        /// <summary>
        /// AND/OR expression per level. Key is PolicyLevel int value, value is expression like "(1)" or "(1)AND(2)".
        /// </summary>
        public Dictionary<int, string>? AndOrExpression { get; set; }
    }

    /// <summary>
    /// JSON-serializable filter policy data.
    /// Maps to AvePoint.GCommon.Contract.CommonFilter.FilterPolicy at runtime.
    /// </summary>
    public class FilterPolicyData
    {
        public int SequenceNo { get; set; } = 1;

        /// <summary>
        /// PolicyLevel: SiteCollection=1, Site=2, List=8, Folder=16, Item=32, Document=64
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// PolicyRuleType: None=0, Title=4, Name=8, Size=16, CreatedTime=64
        /// </summary>
        public int RuleType { get; set; }

        /// <summary>
        /// PolicyCondition: Exactly=1, Contains=8, StartWith=16, Match=128, GreaterThan=256, LessThan=512
        /// </summary>
        public int Condition { get; set; }

        /// <summary>
        /// The value used for rule matching (e.g., file name pattern, size threshold)
        /// </summary>
        public string? Value1 { get; set; }

        /// <summary>
        /// Optional second value (e.g., date range end)
        /// </summary>
        public string? Value2 { get; set; }
    }
}
