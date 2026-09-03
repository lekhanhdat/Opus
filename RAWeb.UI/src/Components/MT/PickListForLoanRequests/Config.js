import { PickListForLoanStatusType } from "../../../Constants/Constants";

const TableColumns = [
    {
        header: RMResx.RM_PRM_MyRequest_ItemName,
        width: [200],
        resizeable: true,
    },
    {
        header: RMResx.RM_PRM_RequestManagement_UniqueId,
        width: [300],
        resizeable: true,
    },
    {
        header: RMResx.RM_MT_PickList_Column_RequestLoanBy,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_Template_Column_Name_HomeLocation,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_MT_PickList_Column_Status,
        resizeable: true,
        width: [250],
    }
];

const StatusList = [
    {
        name: RMResx.RM_MT_PickList_Status_PendingLoan,
        value: PickListForLoanStatusType.Pendding,
        checked: false,
    },
    {
        name: RMResx.RM_MT_PickList_Status_Loaned,
        value: PickListForLoanStatusType.Complete,
        checked: false,
    },
];

const ReturnHistoryTableColumns = [
    {
        header: RMResx.RM_PRM_MyRequest_ItemName,
        width: [200],
        resizeable: true,
    },
    {
        header: RMResx.RM_PRM_RequestManagement_UniqueId,
        width: [300],
        resizeable: true,
    },
    {
        header: RMResx.RM_MT_History_Column_ReturnBy,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_Template_Column_Name_HomeLocation,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_Template_Column_Name_ReturnTime,
        resizeable: true,
        width: [250],
    }
];


export { TableColumns, StatusList, ReturnHistoryTableColumns };