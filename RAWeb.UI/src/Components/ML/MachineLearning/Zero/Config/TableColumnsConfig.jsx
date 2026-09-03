import ColumnTemplate from "../../Config/ColumnTemplate";

const IntelligentTermTableColumns = [
    {
        header: RMResx.RM_ML_IT_Column_TeamName,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: "Name",
    },
    {
        headerTemplate: (
            <ColumnTemplate
                columnName={RMResx.RM_ML_IT_Column_Description}
                popoverContent={RMResx.RM_ML_IT_Column_Description_Tips}
            />
        ),
        width: [350],
        resizeable: true,
        sortable: true,
        valuePath: "Description",
    },
    {
        header: RMResx.RM_ML_IT_Column_TotalApprovedDocument,
        resizeable: true,
        width: [200],
        sortable: true,
        valuePath: "ZeroApprovalCount",
    },
    {
        header: RMResx.RM_ML_IT_Column_TotalReclassifyDocument,
        resizeable: true,
        width: [200],
        sortable: true,
        valuePath: "ZeroReclassifyCount",
    },
    {
        header: (
            <ColumnTemplate
                columnName={RMResx.RM_ML_IT_Column_AutoApply}
                popoverContent={RMResx.RM_ML_IT_Column_AutoApply_Tip}
            />
        ),
        resizeable: true,
        width: [200],
    },
];
export { IntelligentTermTableColumns };
