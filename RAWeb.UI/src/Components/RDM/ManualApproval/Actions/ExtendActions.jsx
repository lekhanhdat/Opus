import React from "react";
import { showToast } from "../../../../Utilities/CommonUtil";
import { ManualReviewAction, ManualReviewActionI18Ns, ManualReviewActionIcons } from "../Constants/index";
import Utility from "../Utility";
import ExtendRestoreAction from "./ExtendRestoreAction";
import ExportAction from "./ExportAction";
import { Messagebox } from "../../../Common/Messagebox";

const ExtendActions = ({ checkedItems, itemCount, onReload, limitItemsCount, queryDefintion}) => {

    const PreInspect = () => {
        if (checkedItems.length > limitItemsCount) {
            showToast.warn(RMResx.RM_RDM_MA_Msg_CheckMoreThan5000);
            return false;
        }
        return true;
    };

    const onExtendRestore = () => {

        if(!PreInspect()) {
            return;
        }

        ExtendRestoreAction.Restore(Utility.getItemIds(checkedItems), () => {
            onReload();
        });
    };

    const onExport = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: ExportAction.onExport.bind(this, queryDefintion )});
    };

    return (
        <div className="reco-manual-review-actions">
            <div
                className="reco-manual-review-actions-buttons"
                style={{
                    visibility: checkedItems.length === 0 ? "hidden" : "visible",
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
                <R.Button
                    primary={false}
                    classify="default"
                    icon={ManualReviewActionIcons.get(ManualReviewAction.ExtendRestore)}
                    text={ManualReviewActionI18Ns.get(ManualReviewAction.ExtendRestore)}
                    onClick={onExtendRestore}
                />
            </div>
            <div className="reco-manual-review-actions-desc">
                {
                    RMResx.RM_Common_SelectTableItemsCounter.format(checkedItems.length, itemCount)
                }
            </div>
        </div>
    );
};

export default ExtendActions;