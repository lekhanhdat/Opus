using Newtonsoft.Json;
using System;

namespace AvePoint.RA.Api.Contract
{
    public class RestoreExecutionRequest
    {
        public string Scope { get; set; }
        public int ConflictResolution { get; set; } = 1;

        public int AppsConflictResolution { get; set; } = 1;

        [JsonConverter(typeof(StrictBooleanJsonConverter))]
        public bool IncludeWorkflowDefinition { get; set; } = false;

        [JsonConverter(typeof(StrictBooleanJsonConverter))]
        public bool IncludeSharingLink { get; set; } = false;

        [JsonConverter(typeof(StrictBooleanJsonConverter))]
        public bool IsSkipRestoreConversation { get; set; } = false;

        public int RestoreConversationType { get; set; } = 0;

        public int RestoreVersionOption { get; set; } = 2;

        public int KeepVersionsNumber { get; set; } = 1;

        public int Priority { get; } = 0;


        public string SiteAdministratorUserPrincipalName { get; set; }
        public long? DeleteArchivedDataDaysAfterRestore { get; set; }

        [JsonIgnore]
        public bool IsPublicRestoreApiRequest { get; set; }

        public bool IsSupportLockedSite { get; set; } = false;
    }

    public class RestoreCommonResponse
    {
        [JsonIgnore]
        public RestoreErrorType ErrorType { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class RestoreExecutionResponse : RestoreCommonResponse
    {
        public string JobId { get; set; }
    }

    public class RestoreArchivedDataCheckResponse : RestoreCommonResponse
    {
        public bool HasArchivedData { get; set; }
        public string Scope { get; set; }
    }

    public class RestoreJobStatusResponse : RestoreCommonResponse
    {
        public JobDto Job { get; set; }
    }

    public class JobDto
    {
        public string Id { get; set; }
        public int Status { get; set; }
        public int Progress { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
    }

    public class DeleteRestoredArchivedDataSettings
    {
        public long DayNum { get; set; }
    }

    public enum RestoreErrorType
    {
        None = 0,
        JobIdIsRequired = 1,
        JobNotFound = 2,
        ScopeIsRequired = 3,
        ScopeNotFound = 4,
        UserNotFound = 5,
        UnknowError = 6,
        UnvalidDeleteArchivedData = 7,
        DoNotHavePermission = 8,
    }
}