export const RelatedRecordsAction = {
    None: -1,
    NotDestory: 0,
    Destory: 1
};

export const RelatedRecordsActionI18Ns = new Map([
    [RelatedRecordsAction.NotDestory, RMResx.RM_JS_RDM_RelatedRecordsAction_None],
    [RelatedRecordsAction.Destory, RMResx.RM_JS_RDM_RelatedRecordsAction_Both],
]);