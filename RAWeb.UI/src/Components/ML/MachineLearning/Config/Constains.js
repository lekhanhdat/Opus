const MLTabIndex = {
    IntelligentTerm: 0,
    TrainingScope: 1,
    TrainingReport: 2
};

const MLModelStatus = {
    None: 0,
    Running: 1,
    Succeeded: 2,
    Failed: 3,
    Exception: 4
};

const RenderTermsContainer = {
    Dialog: 1,
    Panel: 2
};

const TrainingStatusType = {
    NotReady: 0,
    Ready: 2,
};

const TrainingMode = {
    Auto: 0,
    Location: 1,
    Manual: 2,
}

const MTSSourceFlag = {
    None: 0,
    SPO: 1,
    GoogleDrive: 2,
}

const RAMessageType = {
    Successful: 0,
    Failed: 1,
}

const TrainingStatus = new Map([
    [
        TrainingStatusType.NotReady, 
        { name: RMResx.RM_ML_TrainStatus_NotReady, status: "Error" }
    ],
    [
        TrainingStatusType.Ready, 
        { name: RMResx.RM_ML_TrainStatus_Ready, status: "Success" }
    ],
]);

const IntelligentTermStatusType = {
    NotTrain: 0,
    Training: 1,
    Trained: 2,
    WillRemoved: 3
};

const IntelligentTermStatus = new Map([
    [ IntelligentTermStatusType.NotTrain, RMResx.RM_ML_TrainStatus_NotTrain ],
    [ IntelligentTermStatusType.Training, RMResx.RM_ML_TrainStatus_Training ],
    [ IntelligentTermStatusType.Trained, RMResx.RM_ML_TrainStatus_Trained ],
    [ IntelligentTermStatusType.WillRemoved, RMResx.RM_ML_TrainStatus_WillRemove]
]);

const TrainingScopeStatusType = {
    NotTrain: 1,
    Training: 2,
    Trained: 3
};

const TrainingScopeStatus = new Map([
    [ TrainingScopeStatusType.NotTrain, RMResx.RM_ML_TrainStatus_NotTrain ],
    [ TrainingScopeStatusType.Training, RMResx.RM_ML_TrainStatus_Training ],
    [ TrainingScopeStatusType.Trained, RMResx.RM_ML_TrainStatus_Trained ],
]);

const AutoApplyStatusType = {
    None: 0,
    NotAutoApply: "true",
    AutoApply: "false"
};

const AutoApplyStatus = new Map([
    [ AutoApplyStatusType.NotAutoApply,  RMResx.RM_JS_Common_Yes ],
    [ AutoApplyStatusType.AutoApply, RMResx.RM_JS_Common_No ]
]);

const StatusByAccuracyCount = {
    NotApplicable: 0,
    Bad: 50,
    Normal: 70,
};

const StatusByAccuracyStatus = {
    [StatusByAccuracyCount.NotApplicable]: RMResx.RM_ML_Accuracy_NotApplicable,
    [StatusByAccuracyCount.Bad]: RMResx.RM_ML_Accuracy_Bad,
    [StatusByAccuracyCount.Normal]: RMResx.RM_ML_Accuracy_Normal
};

const TermFilterColumnType = {
    Status: 1,
    AutoApply: 2,
    IntelligentTerms: 3,
    Reclassify: 4,
    ApprovalStatus: 5,
    PredictTime: 6,
};

const DocumnetFilterColumnType = {
    Status: 1,
    Classification: 2
};
    
export { 
    MLTabIndex, 
    MLModelStatus,
    RenderTermsContainer, 
    TrainingStatusType,
    TrainingStatus,
    IntelligentTermStatusType,
    IntelligentTermStatus,
    StatusByAccuracyCount,
    StatusByAccuracyStatus,
    AutoApplyStatus,
    TermFilterColumnType,
    DocumnetFilterColumnType,
    TrainingScopeStatusType,
    TrainingScopeStatus,
    TrainingMode,
    MTSSourceFlag,
    RAMessageType,
};