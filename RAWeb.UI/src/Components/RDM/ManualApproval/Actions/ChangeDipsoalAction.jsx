import React, { useState, useImperativeHandle, forwardRef } from "react";
import { showToast } from "../../../../Utilities/CommonUtil";
import StringUtil from "../../../../Utilities/StringUtil";
import { ActionCompletedStatus, RelatedRecordsAction } from "../Constants/index";

const ChangeDisposalAction = forwardRef(({ onReload }, ref) => {

    const [isShow, setIsShow] = useState(false);

    const [checkedItemIds, setCheckedItemIds] = useState(new Set());

    const [isChecked, setIsChecked] = useState(false);

    const [disposalAction, setDisposalAction] = useState(RelatedRecordsAction.None);

    const [disposalActionLabel, setDisposalActionLabel] = useState("");

    useImperativeHandle(ref, () => ({
        onShow: (itemIds, disposalAction) => {
            setIsShow(true);
            setCheckedItemIds(itemIds);
            setDisposalAction(disposalAction);

            const isDesotry = disposalAction === RelatedRecordsAction.Destory;
            const actionLabel = isDesotry ? RMResx.RM_JS_MA_ChangeActionToOnly : RMResx.RM_JS_MA_ChangeActionToBoth;
            setDisposalActionLabel(actionLabel);
            setIsChecked(false);
        }
    }));

    const onCancel = () => {
        setIsShow(false);
    };

    const onExecuteAction = async () => {
        if (!isChecked) {
            setIsShow(false);
            return;
        }
        $$.loading(true);
        setIsShow(false);

        const requestOptions = {
            url: "/api/ManualApproval/ChangeDiposalAction",
            data: {
                ItemIds: Array.from(checkedItemIds),
                DisposalAction: disposalAction === RelatedRecordsAction.Destory ? RelatedRecordsAction.NotDestory : RelatedRecordsAction.Destory,
            }
        };
        fetchUtility(requestOptions)
            .then((result) => {
                if (result.completedStatus == ActionCompletedStatus.Failed) {
                    showToast.error(RMResx.RM_JS_MA_ChangeActionFailed);
                    return;
                }

                showToast.success(RMResx.RM_JS_MA_ChangeActionSucceed);
                if (onReload) {
                    onReload();
                }
            })
            .catch(() => {
                showToast.error(RMResx.RM_JS_MA_ChangeActionFailed);
            })
            .finally(() => $$.loading(false));
    };

    return (
        <R.Dialog
            id="changeActionDia"
            header={RMResx.RM_MA_ChangeActionTitle}
            width={550}
            height={300}
            status={{ show: isShow }}
            struct={{ foot: true }}
            onHide={onCancel}
            destroy={true}
        >
            <div>
                <$g.FormRow label={StringUtil.trimEndColon(RMResx.RM_MA_ChangeDisposalAction)}>
                    <div className="margin-top-20">
                        <R.Checkbox
                            checked={isChecked}
                            text={disposalActionLabel}
                            onChange={value => setIsChecked(value)}
                        />
                    </div>
                </$g.FormRow>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCancel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={onExecuteAction} />
            </>
        </R.Dialog>
    );
});

ChangeDisposalAction.displayName = "ChangeDisposalAction";

export default ChangeDisposalAction;