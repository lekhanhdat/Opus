import React, { useRef } from "react";
import { showToast } from "../../../../Utilities/CommonUtil";
import { ManualReviewAction, ManualReviewActionI18Ns, ManualReviewActionIcons } from "../Constants/index";
import Utility from "../Utility";
import ExportAction from "./ExportAction";
import { Messagebox } from "../../../Common/Messagebox";
import { ManualTab } from "../Constants/ManualTable";
import ChangeDisposalAction from "./ChangeDipsoalAction";

const RelatedRecordsActions = ({ checkedItems, itemCount, onReload, limitItemsCount ,queryDefintion }) => {

    const disposalActionRef = useRef();

    const PreInspect = () => {
        if (checkedItems.length > limitItemsCount) {
            showToast.warn(RMResx.RM_RDM_MA_Msg_CheckMoreThan5000);
            return false;
        }
        return true;
    };

    const onChangeAction = () => {

        if(!PreInspect()) {
            return;
        }

        disposalActionRef.current.onShow(Utility.getItemIds(checkedItems), checkedItems[0].relatedRecordsAction);
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
                    icon={ManualReviewActionIcons.get(ManualReviewAction.ChangeAction)}
                    text={ManualReviewActionI18Ns.get(ManualReviewAction.ChangeAction)}
                    onClick={onChangeAction}
                />
            </div>
            <div className="reco-manual-review-actions-desc">
                {
                    RMResx.RM_Common_SelectTableItemsCounter.format(checkedItems.length, itemCount)
                }
            </div>

            <ChangeDisposalAction ref={disposalActionRef} onReload={onReload} />
        </div>
    );
};

export default RelatedRecordsActions;