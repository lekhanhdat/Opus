import OrderOptions from "../Constants/OrderOptions";
const TableColumns = [
    {
        header: RMResx.RM_JS_MA_Grid_Title,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: OrderOptions.LeafName
    },
    {
        header: RMResx.RM_ML_TS_Column_Classification,
        width: [200],
        resizeable: true,
    },
    {
        header: RMResx.RM_JS_MA_Grid_RecordsId,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_BCM_Explorer_Datagrid_FileType,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_MA_Grid_EscalateOrReassignFrom,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_MA_Grid_RecordOwner,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_MA_Grid_Comment,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_MA_Grid_ModifiedBy,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_MA_Grid_ModifiedTime,
        resizeable: true,
        width: [250],
        sortable: true,
        valuePath: OrderOptions.ModifiedTime
    },
    {
        header: RMResx.RM_JS_MA_Grid_CreatedBy,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_JS_MA_Grid_CreatedDate,
        resizeable: true,
        width: [250],
        sortable: true,
        valuePath: OrderOptions.CreatedTime
    },
    {
        header: RMResx.RM_JS_MA_Grid_CreatedTime,
        resizeable: true,
        width: [250],
        sortable: true,
        valuePath: OrderOptions.PredictTime
    }
];

export { TableColumns };