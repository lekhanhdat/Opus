import { showToast } from "../../../../Utilities/CommonUtil";
import { ActionCompletedStatus } from "../Constants/index";
import { RoleType } from "../Constants/RoleType";
import { ManualTab } from "../Constants/ManualTable";

export default class ApprovalAction {

    static onShowMessageBox(content, callback) {
        $$.messagedialog(true,
            {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: content,
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

    static onApprove(checkedItemsIds, approvalComment, approveFrom, quickReason, succeedCallback) {

        const executeApproveAction = async () => {
            $$.loading(true);
            $$.messagedialog(false);
            const data = {
                NeedActionIds: Array.from(checkedItemsIds),
                ApprovalComment : approvalComment,
                ManualFromTab : approveFrom,
                QuickReason : quickReason,
            };
            const requestOption = {
                url: "/api/ManualApproval/Approve",
                data: data
            };

            const result = await fetchUtility(requestOption);

            $$.loading(false);
            
            if(result.completedStatus == ActionCompletedStatus.Failed) {
                showToast.error(RMResx.RM_JS_MA_ApprovalFailed);
                return;
            }

            showToast.success(RMResx.RM_JS_MA_ApprovalSucceed);
            if(succeedCallback) {
                succeedCallback();
            }
        };

        if(approveFrom === ManualTab.WaitDisposal){
            this.onShowMessageBox(RMResx.RM_RDM_MA_EnsureApproveDatas, executeApproveAction);
        }else{
            executeApproveAction();
        }
    }

    static onReject(checkedItemsIds, rejectComment, rejectFrom , quickReason , extendType , customeExtendDate , succeedCallback) {
        
        const executeRejectAction = async () => {
            $$.loading(true);
            $$.messagedialog(false);
            const data = {
                NeedActionIds: Array.from(checkedItemsIds),
                ApprovalComment : rejectComment,
                ManualFromTab : rejectFrom,
                QuickReason : quickReason,
                ExtendType : extendType,
                CustomeExtendDate : customeExtendDate
            };
            const requestOption = {
                url: "/api/ManualApproval/Reject",
                data: data
            };

            const result = await fetchUtility(requestOption);

            $$.loading(false);

            if(result.completedStatus == ActionCompletedStatus.Failed) {
                showToast.error(RMResx.RM_JS_MA_RejectFailed);
                return;
            }

            showToast.success(RMResx.RM_JS_MA_RejectSucceed);
            if(succeedCallback) {
                succeedCallback();
            }
        };

        if(rejectFrom === ManualTab.WaitDisposal){
            this.onShowMessageBox(RMResx.RM_RDM_MA_EnsureRejectDatas, executeRejectAction);
        }else{
            executeRejectAction();
        }
    }

    static onRunApproveJob(actionType, queryDefintion, approvalComment, quickReason,unCheckedItemIds ,extendType  , customeExtendDate , succeedCallback) {

        const executeRunApproveJob = async() =>{
            $$.loading(true);
            $$.messagedialog(false);
            const data = {
                ApprovalAction: actionType,
                queryDefintion: queryDefintion,
                ApprovalComment: approvalComment,
                QuickReason : quickReason,
                UncheckedItemIds : unCheckedItemIds,
                ExtendType : extendType,
                CustomeExtendDate : customeExtendDate
            };
            const requestOption = {
                url: "/api/ManualApproval/RunBulkActionJob",
                data: data
            };

            const result = await fetchUtility(requestOption);

            $$.loading(false);

            if(result.messageType == "0"){
                if (RM.RoleType != RoleType.StandardUser) {
                    showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                } else {
                    showToast.success(RMResx.RM_JS_MA_JobSucessMessage);
                }
            }else{
                showToast.error(result.errorMessage);
            }
            
            if(succeedCallback) {
                succeedCallback();
            }
        };

        executeRunApproveJob();
    }
}