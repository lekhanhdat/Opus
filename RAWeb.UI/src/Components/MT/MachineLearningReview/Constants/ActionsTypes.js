export const ActionTypes = {
    None: 0,
    Approve: 1,
    Reassign: 2,
    Reclassfy: 3
};

export const ActionI18Ns = new Map([
    [ActionTypes.Approve, RMResx.RM_MA_Approve],
    [ActionTypes.Reassign, RMResx.RM_MA_Reassign],
    [ActionTypes.Reclassfy, RMResx.RM_JS_Notifi_Action_Reclassification],
]);
