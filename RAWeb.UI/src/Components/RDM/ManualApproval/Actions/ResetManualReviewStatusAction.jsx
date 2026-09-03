import { showToast } from "../../../../Utilities/CommonUtil";
import { ActionCompletedStatus } from "../Constants/index";
import { RoleType } from "../Constants/RoleType";

export default class ResetManualReviewStatusAction {
    static onShowMessageBox(cotnent, callback) {
        $$.messagedialog(true,
            {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: cotnent,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel,
                        onClick: () => {
                            $$.messagedialog(false);
                        },
                    },
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: callback,
                    },
                ],
            }
        );
    }

    static onResetManualWorkflow(checkedItemsIds, succeedCallback) {
        const executeAction = async () => {
            try {
                $$.loading(true);
                $$.messagedialog(false);
                const requestOption = {
                    url: "/api/ManualApproval/ResetManualReviewForWorkflow",
                    data: Array.from(checkedItemsIds)
                };

                const result = await fetchUtility(requestOption);

                $$.loading(false);

                if (result.completedStatus === ActionCompletedStatus.Failed || result.completedStatus === ActionCompletedStatus.HasException) {
                    const failedItems = result.effectItems.filter(i => !i.isSucceed).map(i => i.effectItemFullPath);
                    $$.messagedialog(true,
                        {
                            width: "550px",
                            hideActions: false,
                            title: RMResx.RM_JS_Common_Confirmation,
                            content: (
                                <div>
                                    <div className="reco-manual-message-box-comment">
                                        {failedItems.length > 1 ?
                                            RMResx.RM_JS_MA_ItemsDisposal :
                                            RMResx.RM_JS_MA_ItemDisposal
                                        }
                                    </div>
                                    <div className="reco-manual-message-box-associated">
                                        {RMResx.RM_MA_AssociatedRecords}
                                    </div>
                                    <div className="reco-manual-message-box-associated-items">
                                        {
                                            failedItems.map(item =>
                                                <div key={item} className="reco-manual-message-box-associated-item">{item}</div>
                                            )
                                        }
                                    </div>
                                </div>
                            ),
                            buttons: [
                                {
                                    text: RMResx.RM_JS_Common_OK,
                                    primary: true,
                                    classify: "theme",
                                    onClick: async () => {
                                        $$.messagedialog(false);
                                        if (result.completedStatus === ActionCompletedStatus.HasException) {
                                            succeedCallback();
                                        }
                                    },
                                },
                            ],
                        }
                    );
                    // showToast.error(RMResx.RM_JS_MA_ResetManualWorkflowFailed);
                    return;
                }

                showToast.success(RMResx.RM_JS_MA_ResetManualWorkflowSucceed);
                if (succeedCallback) {
                    succeedCallback();
                }
            }
            catch {
                $$.loading(false);
                showToast.error(RMResx.RM_JS_MA_ResetManualWorkflowFailed);
                return;
            }
        };

        this.onShowMessageBox(RMResx.RM_RDM_MA_EnsureResetManualWorkflow, executeAction);
    }

    static onRunResetManualWorkflowJob(actionType, queryDefintion, unCheckedItemIds, succeedCallback) {
        const executeAction = async () => {
            $$.loading(true);
            $$.messagedialog(false);
            const data = {
                ApprovalAction: actionType,
                queryDefintion: queryDefintion,
                UncheckedItemIds : unCheckedItemIds
            };
            const requestOption = {
                url: "/api/ManualApproval/RunBulkActionJob",
                data: data,
            };

            const result = await fetchUtility(requestOption);

            $$.loading(false);

            if (result.messageType == "0") {
                if (RM.RoleType != RoleType.StandardUser) {
                    showToast.success(
                        <$g.I18NProvider
                            msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}
                        >
                            <a className="ra-link-a" href="/Root/JM/Index">
                                {RMResx.RM_JS_JM_Title}
                            </a>
                        </$g.I18NProvider>
                    );
                } else {
                    showToast.success(RMResx.RM_JS_MA_JobSucessMessage);
                }
            } else {
                showToast.error(result.rrorMessage);
            }

            if (succeedCallback) {
                succeedCallback();
            }
        };

        this.onShowMessageBox(
            RMResx.RM_RDM_MA_EnsureResetManualWorkflow,
            executeAction
        );
    }
}