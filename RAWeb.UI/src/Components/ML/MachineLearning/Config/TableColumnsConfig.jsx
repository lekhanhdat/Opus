import ColumnTemplate  from "./ColumnTemplate";

const IntelligentTermTableColumns = [
    {
        header: RMResx.RM_ML_IT_Column_TeamName,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: "Name"
    },
    {
        headerTemplate: <ColumnTemplate
            columnName={RMResx.RM_ML_IT_Column_Status}
            popoverContent={RMResx.RM_ML_IT_Column_Status_Tip}
        />,
        width: [200],
        resizeable: true,
    },
    {
        header: RMResx.RM_ML_IT_Column_Accuracy,
        resizeable: true,
        width: [200],
        sortable: true,
        valuePath: "Accuracy"
    },
    {
        header: RMResx.RM_ML_IT_Column_TrainingScope,
        resizeable: true,
        width: [200],
    },
    {
        header: <ColumnTemplate
            columnName={RMResx.RM_ML_IT_Column_AutoApply}
            popoverContent={RMResx.RM_ML_IT_Column_AutoApply_Tip}
        />,
        resizeable: true,
        width: [200],
    }
];

const TrainingScopeTableColumns = [
    {
        header: RMResx.RM_ML_TS_Column_DocumentName,
        width: [300],
        resizeable: true,
        sortable: true,
        valuePath: "Name"
    },
    {
        header: RMResx.RM_ML_TS_Column_Classification,
        width: [300],
        resizeable: true,
    },
    {
        header: RMResx.RM_ML_TS_Column_Status,
        resizeable: true,
        width: [300],
    }
];

const TrainingReportTableColumns = [
    {
        header: RMResx.RM_ML_TS_Column_DocumentName,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: "Name"
    },
    {
        header: RMResx.RM_MachineLearning_ReprotIntelligentClassification,
        width: [300],
        resizeable: true
    },
    {
        header: RMResx.RM_MachineLearning_ReprotCurrentClassification,
        resizeable: true,
        width: [250]
    },
    {
        header: RMResx.RM_JS_BCM_Explorer_Datagrid_UniqueID,
        resizeable: true,
        width: [250]
    },
    {
        header: RMResx.RM_JS_JMD_Grid_ApprovalStatus,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_JMD_Grid_Type,
        resizeable: true,
        width: [250]
    }, 
    {
        header: RMResx.RM_MachineLearning_ReprotPredictTime,
        resizeable: true,
        width: [250],
        sortable: true,
        valuePath: "PredictTime"
    }
];

const AddTermsTableColumns = [
    {
        header: RMResx.RM_ML_AddTerm_Column_TermName,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_ML_AddTerm_Column_FullPath,
        width: [180],
        resizeable: true
    },
    {
        header: RMResx.RM_ML_AddTerm_Column_Description,
        width: [180],
        resizeable: true
    }
];

const AddScopesTableColumns = [
    {
        header: RMResx.RM_ML_TrainingScope_AddPanel_FileNameColumn,
        width: [180],
        resizeable: true,
        sortable: true,
        valuePath: "FileName"
    },
    {
        header: RMResx.RM_ML_TrainingScope_AddPanel_FilePathColumn,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_ML_AddTerm_Column_TermName,
        width: [180],
        resizeable: true,
    }
];

export { 
    IntelligentTermTableColumns, 
    TrainingScopeTableColumns, 
    TrainingReportTableColumns, 
    AddTermsTableColumns,
    AddScopesTableColumns,
};