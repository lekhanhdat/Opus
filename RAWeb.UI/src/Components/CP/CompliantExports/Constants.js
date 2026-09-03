const ConfiguratorStepStatus = {
    None: 0,
    Waiting: 1,
    Processing: 2,
    Completed: 3,
};

const ConfiguratorFormatValue = {
    VEO: 3,
    NAA: 4,
    NARA: 5,
};

const ConfiguratorSteps = [
    {
        step: 1,
        name: RMResx.RM_ES_CompliantExport_Wizard_Step01,
        status: ConfiguratorStepStatus.Processing,
    },
    {
        step: 2,
        name: RMResx.RM_ES_CompliantExport_Wizard_Step02,
        status: ConfiguratorStepStatus.Waiting,
    },
    {
        step: 3,
        name: RMResx.RM_ES_CompliantExport_Wizard_Step03,
        status: ConfiguratorStepStatus.Waiting,
    },
];

const ConfiguratorContentSource = {
    SPO_OD: 1, // 6
    EXO: 3,
    Google: 9,
};

const RAMessageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2,
};

const FormatType = {
    Date: 1,
    String: 2
}

export {
    ConfiguratorFormatValue,
    ConfiguratorStepStatus,
    ConfiguratorSteps,
    ConfiguratorContentSource,
    RAMessageType,
    FormatType,
};
