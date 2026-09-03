import { showToast } from "../../../../Utilities/CommonUtil";
import { ActionCompletedStatus } from "../Constants/index";

export default class ExtendRestoreAction {
    static Restore(checkedItemsIds, succeedCallback) {
        $$.messagedialog(true,
            {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_MA_Extended_EnsureRestoreRecords,
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
                        onClick: async () => {
                            $$.loading(true);
                            $$.messagedialog(false);
                            const requestOption = {
                                url: "/api/ManualApproval/RestoreExtended",
                                data: Array.from(checkedItemsIds)
                            };

                            fetchUtility(requestOption)
                                .then((result) => {
                                    if (result.completedStatus == ActionCompletedStatus.Failed) {
                                        showToast.error(RMResx.RM_MA_Extended_RestoreFailed);
                                        return;
                                    }

                                    showToast.success(RMResx.RM_MA_Extended_RestoreSucceed);
                                    if (succeedCallback) {
                                        succeedCallback();
                                    }
                                })
                                .catch(() => {
                                    showToast.error(RMResx.RM_MA_Extended_RestoreFailed);
                                })
                                .finally(() => $$.loading(false));
                        },
                    },
                ],
            }
        );
    }
}