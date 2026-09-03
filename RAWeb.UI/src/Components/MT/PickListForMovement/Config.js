import { PickListForMoveStatusType } from "../../../Constants/Constants";

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
        header: RMResx.RM_MT_PickList_Column_ApproveBy,
        resizeable: true,
        width: [200],
    },
    {
        header: RMResx.RM_MT_PickList_Column_OriginalLocation,
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_PRM_Action_Move_DestinationSite, 
        resizeable: true,
        width: [250],
    },
    {
        header: RMResx.RM_MT_PickList_Column_Status,
        resizeable: true,
        width: [150],
    },
    {
        header: RMResx.RM_JS_JM_Comment,
        resizeable: true,
        width: [250],
    }
];

const StatusList = [
    {
        name: RMResx.RM_MT_PickList_Status_PendingMove,
        value: PickListForMoveStatusType.PendingMove,
        checked: false,
    },
    {
        name: RMResx.RM_MT_PickList_Status_Failed,
        value: PickListForMoveStatusType.Failed,
        checked: false,
    },
];

export { TableColumns, StatusList };