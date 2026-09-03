import { PickListForDestroyStatusType } from "../../../Constants/Constants";

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
        header: RMResx.RM_PRM_PRE_Column_DisposalClass,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_MT_PickList_Column_DateDestroyed,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_Template_Column_Name_HomeLocation,
        resizeable: true,
        width: [300],
    },
    {
        header: RMResx.RM_MT_PickList_Column_ApproveBy,
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
        name: RMResx.RM_MT_PickList_Status_PendingDestroy,
        value: PickListForDestroyStatusType.Pendding,
        checked: false,
    },
    {
        name: RMResx.RM_MT_PickList_Status_Destroyed,
        value: PickListForDestroyStatusType.Complete,
        checked: false,
    },
];

export { TableColumns, StatusList };