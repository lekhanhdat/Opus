import React, { useRef, useState } from "react";
import { showToast } from "../../../../Utilities/CommonUtil";
import { ManualReviewAction, ManualReviewActionI18Ns, ManualReviewActionIcons, ApprovalStatus, ExportType } from "../Constants/index";
import { ManualTab } from "../Constants/ManualTable";
import ApprovalAction from "./ApprovalAction";
import ExportAction from "./ExportAction";
import Utility from "../Utility";
import { Messagebox } from "../../../Common/Messagebox";
import ResetManualReviewStatusAction from "./ResetManualReviewStatusAction";

const WaitDisposalActions = ({ isCheckedAll, queryDefintion, checkedItems, unCheckedItems, itemCount, onReload, limitItemsCount, NeedCustomButton, CustomButtonNames }) => {

    const PreInspect = () => {
        if (checkedItems.length > limitItemsCount) {
            showToast.warn(RMResx.RM_RDM_MA_Msg_CheckMoreThan5000);
            return false;
        }
        return true;
    };

    const JobPreInspect = () => {
        if (unCheckedItems.length > 1000) {
            showToast.warn(RMResx.RM_MA_TasksDeSelected_Limited);
            return false;
        }
        return true;
    };


    const onApprove = () => {

        if (!PreInspect()) {
            return;
        }

        ApprovalAction.onApprove(Utility.getItemIds(checkedItems), "" , ManualTab.WaitDisposal, "",() => {
            onReload();
        });
    };

    const onReject = () => {

        if (!PreInspect()) {
            return;
        }

        ApprovalAction.onReject(Utility.getItemIds(checkedItems), "" , ManualTab.WaitDisposal, "", ExportType.None, new Date() ,() => {
            onReload();
        });
    };

    const onManualReview = () => {

        if (isCheckedAll) {
            if(!JobPreInspect()){
                return;
            }

            ResetManualReviewStatusAction.onRunResetManualWorkflowJob(ManualReviewAction.ResetManualWorkflow, queryDefintion, Utility.getItemIds(unCheckedItems) ,() => {
                onReload();
            });
            return;
        }

        if (!PreInspect()) {
            return;
        }

        ResetManualReviewStatusAction.onResetManualWorkflow(Utility.getItemIds(checkedItems), () => {
            onReload();
        });
    };

    const onExport = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: ExportAction.onExport.bind(this, queryDefintion) });
    };

    return (
        <div className="reco-manual-review-actions">
            <div
                className="reco-manual-review-actions-buttons"
                style={{
                    visibility: checkedItems.length === 0 && !isCheckedAll ? "hidden" : "visible",
                }}
            >
                <div style={{
                    visibility: "visible",
                }}
                >
                    <R.Button
                        primary={true}
                        classify="theme"
                        text={ManualReviewActionI18Ns.get(ManualReviewAction.Export)}
                        onClick={onExport}
                    />
                </div>
                {
                    !isCheckedAll && checkedItems.every(item => item.internalApprovedStatus !== ApprovalStatus.WorkflowComplete) &&
                    <>
                        <R.Button
                            primary={false}
                            classify="default"
                            text={Utility.getCustomButtonNames(NeedCustomButton, CustomButtonNames).approveButtonName}
                            icon={ManualReviewActionIcons.get(ManualReviewAction.Approve)}
                            onClick={onApprove}
                        />
                        <R.Button
                            primary={false}
                            classify="default"
                            text={Utility.getCustomButtonNames(NeedCustomButton, CustomButtonNames).rejectButtonName}
                            icon={ManualReviewActionIcons.get(ManualReviewAction.Reject)}
                            onClick={onReject}
                        />
                    </>
                }
                {
                    (
                        (!isCheckedAll && checkedItems.every(item => item.internalApprovedStatus === ApprovalStatus.WorkflowComplete))
                        || isCheckedAll
                    ) &&
                    <R.Button
                        primary={false}
                        classify="default"
                        icon={ManualReviewActionIcons.get(ManualReviewAction.ResetManualWorkflow)}
                        text={ManualReviewActionI18Ns.get(ManualReviewAction.ResetManualWorkflow)}
                        onClick={onManualReview}
                    />
                }
            </div>
            <div className="reco-manual-review-actions-desc">
                {
                    RMResx.RM_Common_SelectTableItemsCounter.format(isCheckedAll ? itemCount - unCheckedItems.length : checkedItems.length, itemCount)
                }
            </div>
        </div>
    );
};

export default WaitDisposalActions;