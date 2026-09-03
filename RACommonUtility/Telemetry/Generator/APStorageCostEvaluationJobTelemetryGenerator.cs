using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class APStorageCostEvaluationJobTelemetryGenerator : TelemetryGenerator
    {
        public override TelemetryModule Module => TelemetryModule.StorageCostEvaluation;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping => new()
        {
            { TelemetryEventType.RunJob, RunJob }
        };

        public CloudRecordsCommonRecord RunJob(IList<object> args)
        {
            return args[0] as CloudRecordsStorageCostEvaluationRecord;
        }
    }
}
