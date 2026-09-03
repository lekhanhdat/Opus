const GetStoreDataItems = (type) => [
    {
        text: RMResx.RM_RDM_CreateRule_DefaultTier,
        title: RMResx.RM_RDM_CreateRule_DefaultTier,
        value: TierTypes.DefaultTier,
        checked: type === TierTypes.DefaultTier,
    },
    {
        text: RMResx.RM_RDM_CreateRule_ColdTier,
        title: RMResx.RM_RDM_CreateRule_ColdTier,
        value: TierTypes.ColdTier,
        checked: type === TierTypes.ColdTier,
    },
    {
        text: RMResx.RM_RDM_CreateRule_ArchivedTier,
        title: RMResx.RM_RDM_CreateRule_ArchivedTier,
        value: TierTypes.ArchiveTier,
        checked: type === TierTypes.ArchiveTier,
    },
];

const ArchiveDataType = {
    None: 0,
    All: 1,
    Special: 2
};

const MS365DataType = {
    None: 0,
    Default: 1,
    Phl: 2
};

const ArchiveOrRemoveFileType = {
    None: 0,
    ArchiveAndRemove: 1,
    Remove: 2,
    Archive: 3,
};

const ArchiveOrRemoveVersionType = {
    None: 0,
    ArchiveAndRemove: 1,
    Remove: 2,
};

const ScheduleType = {
    None: 0,
    Now: 1,
    ConfigSchedule: 2,
};

const TierTypes = {
    DefaultTier: 0,
    ArchiveTier: 3,
    ColdTier: 4,
};

export {
    GetStoreDataItems,
    ArchiveDataType,
    MS365DataType,
    ArchiveOrRemoveFileType,
    ArchiveOrRemoveVersionType,
    ScheduleType,
    TierTypes,
};