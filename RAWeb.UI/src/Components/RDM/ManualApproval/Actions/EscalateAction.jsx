import React, { useState, useImperativeHandle, forwardRef, useEffect } from "react";
import StringUtil from "../../../../Utilities/StringUtil";
import PeoplePicker from "../../../Common/PeoplePicker";
import { ActionCompletedStatus } from "../Constants/index";
import { showToast } from "../../../../Utilities/CommonUtil";
import { useStableCallback } from "../Hooks/index";

const BuildRequestOptions = (checkedItemIds, toUsers, needSendEmail, emailContent, methodName) => {
    const data = {
        ToUsers: toUsers,
        ItemIds: Array.from(checkedItemIds),
        NeedSendEmail: needSendEmail,
        Comment: emailContent
    };

    return {
        url: `/api/ManualApproval/${methodName}`,
        data: data
    };
};

const EscalateAction = forwardRef(({ onReload }, ref) => {

    const [isShow, setIsShow] = useState(false);

    const [checkedItemIds, setCheckedItems] = useState(new Set());

    const [toUsers, setToUsers] = useState([]);

    const [needSendEmail, setNeedSendEmail] = useState(true);

    const [emailContent, setEmailContent] = useState("");

    const [showValidateMessage, setShowValidateMessage] = useState(false);

    useImperativeHandle(ref, () => ({
        onShow: (itemIds) => {
            setIsShow(true);
            setCheckedItems(itemIds);
            setToUsers([]);
            setNeedSendEmail(true);
            setEmailContent("");
            setShowValidateMessage(false);
        }
    }));

    const onCancel = () => {
        setIsShow(false);
    };

    const onExecuteAction = useStableCallback(async () => {
        if (toUsers.length === 0) {
            setShowValidateMessage(true);
            return;
        }

        $$.loading(true);
        setIsShow(false);

        const requestOptions = BuildRequestOptions(checkedItemIds, toUsers, needSendEmail, emailContent, "Escalate");
        const result = await fetchUtility(requestOptions);
        $$.loading(false);

        if (result.completedStatus == ActionCompletedStatus.Failed) {
            showToast.error(RMResx.RM_JS_MA_EscalateFailed);
            return;
        }

        showToast.success(RMResx.RM_JS_MA_EscalateSucceed);
        if (onReload) {
            onReload();
        }
    });

    return (
        <R.Dialog
            id="escalateDia"
            header={RMResx.RM_MA_Escalate}
            width={550}
            height={430}
            status={{ show: isShow }}
            struct={{ foot: true }}
            onHide={onCancel}
            destroy={true}
        >
            <div>
                <$g.FormRow label={RMResx.RM_MA_Email_PopupTitle} require={true}>
                    <PeoplePicker
                        height="auto"
                        width="100%"
                        items={toUsers}
                        selectionChanged={(value) => setToUsers(value)}
                    />
                    <$g.ValidationMsg show={showValidateMessage}>
                        {RMResx.RM_MA_Email_PopupUserEmptyError}
                    </$g.ValidationMsg>
                </$g.FormRow>
                <div className="reco-manual-review-form-item">
                    <div className="reco-manual-review-form-label">
                        <R.Checkbox
                            checked={needSendEmail}
                            text={StringUtil.trimEndColon(RMResx.RM_MA_Email_PopupEmail)}
                            onChange={checked => setNeedSendEmail(checked)}
                        />
                    </div>
                    <div className="reco-manual-review-form-input">
                        <R.Input
                            type="textarea"
                            className="resizable"
                            onChange={value => setEmailContent(value)}
                            aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_MA_Email_PopupEmail) }}
                        />
                    </div>
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCancel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={onExecuteAction} />
            </>
        </R.Dialog>
    );

});

EscalateAction.displayName = "EscalateAction";

const ReassignAction = forwardRef(({ onReload }, ref) => {

    const [isShow, setIsShow] = useState(false);

    const [checkedItemIds, setCheckedItems] = useState(new Set());

    const [toUsers, setToUsers] = useState([]);

    const [needSendEmail, setNeedSendEmail] = useState(true);

    const [emailContent, setEmailContent] = useState("");

    const [showValidateMessage, setShowValidateMessage] = useState(false);

    useImperativeHandle(ref, () => ({
        onShow: (itemIds) => {
            setIsShow(true);
            setCheckedItems(itemIds);
            setToUsers([]);
            setNeedSendEmail(true);
            setEmailContent("");
            setShowValidateMessage(false);
        }
    }));

    const onCancel = () => {
        setIsShow(false);
    };

    const onExecuteAction = useStableCallback(async () => {
        if (toUsers.length === 0) {
            setShowValidateMessage(true);
            return;
        }

        $$.loading(true);
        setIsShow(false);

        const requestOptions = BuildRequestOptions(checkedItemIds, toUsers, needSendEmail, emailContent, "Reassign");
        const result = await fetchUtility(requestOptions);
        $$.loading(false);

        if (result.completedStatus == ActionCompletedStatus.Failed) {
            showToast.error(RMResx.RM_JS_MA_ReassignFailed);
            return;
        }

        showToast.success(RMResx.RM_JS_MA_ReassignSucceed);
        if (onReload) {
            onReload();
        }
    });

    return (
        <R.Dialog
            id="reassignDia"
            header={RMResx.RM_MA_Reassign}
            width={550}
            height={430}
            status={{ show: isShow }}
            struct={{ foot: true }}
            onHide={onCancel}
            destroy={true}
        >
            <div>
                <$g.FormRow label={RMResx.RM_MA_Reassign_Email_PopupTitle} require={true}>
                    <PeoplePicker
                        height="auto"
                        width="100%"
                        items={toUsers}
                        selectionChanged={(value) => setToUsers(value)}
                    />
                    <$g.ValidationMsg show={showValidateMessage}>
                        {RMResx.RM_MA_Email_PopupUserEmptyError}
                    </$g.ValidationMsg>
                </$g.FormRow>
                <div className="reco-manual-review-form-item">
                    <div className="reco-manual-review-form-label">
                        <R.Checkbox
                            checked={needSendEmail}
                            text={StringUtil.trimEndColon(RMResx.RM_MA_Email_PopupEmail)}
                            onChange={checked => setNeedSendEmail(checked)}
                        />
                    </div>
                    <div className="reco-manual-review-form-input">
                        <R.Input
                            type="textarea"
                            className="resizable"
                            onChange={value => setEmailContent(value)}
                            aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_MA_Email_PopupEmail) }}
                        />
                    </div>
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCancel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={onExecuteAction} />
            </>
        </R.Dialog>
    );

});

ReassignAction.displayName = "ReassignAction";

export {
    EscalateAction,
    ReassignAction
};