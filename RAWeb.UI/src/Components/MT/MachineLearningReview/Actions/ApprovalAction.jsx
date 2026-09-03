import { showToast } from "../../../../Utilities/CommonUtil";
import { RoleType } from "../Constants/RoleType";
import { ApprovalStatus } from "../Constants/index";
export default class ApprovalAction {

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

    static onApprove(checkedItemsIds, succeedCallback) {

        const executeApproveAction = async () => {
            $$.loading(true);
            $$.messagedialog(false);
            const requestOption = {
                url: "/api/MLManualApproval/Approve",
                data: Array.from(checkedItemsIds)
            };
            $$.loading(false);
            const resultStr = await fetchUtility(requestOption);
            const result = JSON.parse(resultStr);
            if(result.MessageType == 1) {
                showToast.error(RMResx.RM_JS_MA_ApprovalFailed);
                return;
            }
            if(succeedCallback) {
                succeedCallback(result.Extension);
            }
        };

        this.onShowMessageBox(RMResx.RM_RDM_MA_EnsureApproveDatas, executeApproveAction);
    }

    static onRunApproveJob(ActionType, queryDefintion, succeedCallback) {

        const executeRunApproveJob = async() =>{
            $$.loading(true);
            $$.messagedialog(false);
            const requestOption = {
                url: "/api/MLManualApproval/StartApproveJob",
                data: queryDefintion
            };
            $$.loading(false);
            const resultStr = await fetchUtility(requestOption);
            let result = JSON.parse(resultStr);
            if(result.MessageType == "0"){
                if (RM.RoleType != RoleType.StandardUser) {
                    showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>);
                } else {
                    showToast.success(RMResx.RM_JS_MA_JobSucessMessage);
                }
            }else{
                showToast.error(result.ErrorMessage);
            }
            
            if(succeedCallback) {
                succeedCallback();
            }
        };

        this.onShowMessageBox(ActionType == ApprovalStatus.Rejected?RMResx.RM_RDM_MA_EnsureRejectDatas:RMResx.RM_RDM_MA_EnsureApproveDatas, executeRunApproveJob);
    }
}