import React, { useEffect, useRef, useState } from "react";
import _ from "lodash";
import { checkPermission } from "../../../../Utilities/permissionManager";
import RouterUrls from "../../../../Constants/RouterUrls";

import { StayManualReviewOption as StayManualReviewType } from "../Constants/ApprovalStatus";
import { ApprovalCommentOptions } from "../Constants/ConfigOptions";
import ApprovalCommentSettingPanel from "../Panels/ApprovalCommentSettingPanel";
import Utility from "../Utility";
import { ApprovalSettingModule } from "../Constants/Module";


const ManualApproveSettings = () => {

    const beforeQuickReasonRef = useRef([]);

    const beforeInactiveRejectRef = useRef([]);

    const beforeModifiedButtonNameRef = useRef([]);

    const beforeCheckedCustomButtonRef = useRef(false);

    const beforeDuration = useRef(0);

    const beforeCheckedAutoApprovedProcessRef = useRef(false);

    const beforeCheckedQuickReasonRef = useRef(false);

    const beforeCheckedRecheckRuleRef = useRef(true);

    const beforeEnableDeleteInvalidRecordsRef = useRef(false);

    const [showSettingPanel, setShowSettingPanel] = useState(false);

    const [approvalCommentOptions, setApprovalCommentOptions] = useState(
        ApprovalCommentOptions
    );

    const [checkedCommentOption, setCheckedCommentOption] = useState(0);

    const [approvalCommentQuickReasons, setApprovalCommentQuickReasons] =
        useState([]);

    const [inactiveRejects, setInactiveRejects] = useState([]);

    const [customButtonNames, setCustomButtonNames] = useState([]);

    const [duration, setDuration] = useState(0);

    const [isCheckingRuleBeforeDispose, setIsCheckingRuleBeforeDispose] = useState(true);

    const [enableDeleteInvalidRecords, setEnableDeleteInvalidRecords] = useState(false);

    const [needQuickReason, setNeedQuickReason] = useState(false);

    const [needCustomButton, setNeedCustomButton] = useState(false);

    const [autoApprovedProcess, setAutoApprovedProcess] = useState(false);

    const [stayManualReview, setStayManualReview] = useState(
        StayManualReviewType.Stay
    );

    const setCheckedOption = async () => {
        const res = await fetchUtility({
            url: "/api/ManualApproval/GetApprovalCommentOption",
        });
        const quickReasonInfo =
            res.commentSetting.manualApprovalQuickReasonInfo.quickReasonInfo;
        const inactiveRejectInfo =
            res.commentSetting.manualApprovalQuickReasonInfo
                .incativeRejectBool || [];

        setApprovalCommentOptions(
            Utility.setCheckedOption(ApprovalCommentOptions, res.option)
        );
        setNeedQuickReason(
            res.commentSetting.manualApprovalQuickReasonInfo.needQuickReason
        );
        setApprovalCommentQuickReasons(quickReasonInfo);
        setInactiveRejects(inactiveRejectInfo);
        setCheckedCommentOption(res.option);
        setNeedCustomButton(
            res.modifyButtonName.manualApprovalModifyButton
                .enableModifyButtonName
        );
        setCustomButtonNames(
            res.modifyButtonName.manualApprovalModifyButton.modifiedButtonNames
        );
        setAutoApprovedProcess(res.enableAutoApprovedProcess);
        setIsCheckingRuleBeforeDispose(res.isRecheckRule);
        setEnableDeleteInvalidRecords(!!res.enableDeleteInvalidRecords);
        setDuration(res.duration);
        setStayManualReview(res.stayManualReviewOption);
        beforeQuickReasonRef.current =
            res.commentSetting.manualApprovalQuickReasonInfo.quickReasonInfo;
        beforeInactiveRejectRef.current = inactiveRejectInfo;
        beforeCheckedQuickReasonRef.current =
            res.commentSetting.manualApprovalQuickReasonInfo.needQuickReason;
        beforeCheckedCustomButtonRef.current =
            res.modifyButtonName.manualApprovalModifyButton.enableModifyButtonName;
        beforeModifiedButtonNameRef.current =
            res.modifyButtonName.manualApprovalModifyButton.modifiedButtonNames;
        beforeCheckedAutoApprovedProcessRef.current =
            res.enableAutoApprovedProcess;
        beforeCheckedRecheckRuleRef.current = res.isRecheckRule;
        beforeEnableDeleteInvalidRecordsRef.current = !!res.enableDeleteInvalidRecords;
        beforeDuration.current = res.duration;

        // Check if inactiveRejects state is empty array
        if (
            quickReasonInfo &&
            quickReasonInfo.length &&
            (!inactiveRejectInfo || !inactiveRejectInfo.length)
        ) {
            const fillBoolArray = Array(quickReasonInfo.length).fill(false);
            beforeInactiveRejectRef.current = fillBoolArray;
            setInactiveRejects(fillBoolArray);
        }
    };

    const onSaveConfigration = async (option) => {
        if (!needQuickReason) {
            const clonedTermInfo = _.cloneDeep(beforeQuickReasonRef.current);
            const clonedInactiveRejectInfo = _.cloneDeep(
                beforeInactiveRejectRef.current
            );
            setApprovalCommentQuickReasons(clonedTermInfo);
            setInactiveRejects(clonedInactiveRejectInfo);
        }
        if (!needCustomButton) {
            const clonedCustomButtonName = _.cloneDeep(
                beforeModifiedButtonNameRef.current
            );
            setCustomButtonNames(clonedCustomButtonName);
        }
        const result = await fetchUtility(option);
        if (result) {
            setCheckedOption();
            setShowSettingPanel(false);
        }
        return result;
    };

    const onChangeCommentTermInfo = (value) => {
        let clonedTermInfo = _.cloneDeep(approvalCommentQuickReasons);
        clonedTermInfo = value;
        setApprovalCommentQuickReasons(clonedTermInfo);
    };

    const onChangeDisableTermInfo = (value) => {
        let clonedInactiveRejectInfo = _.cloneDeep(inactiveRejects);
        clonedInactiveRejectInfo = value;
        setInactiveRejects(clonedInactiveRejectInfo);
    };

    const onChangeIsCheckTerm = (value) => {
        const clonedTermInfo = _.cloneDeep(beforeQuickReasonRef.current);
        let clonedIsCheckedTerm = _.cloneDeep(needQuickReason);
        clonedIsCheckedTerm = value;
        setNeedQuickReason(clonedIsCheckedTerm);
        setApprovalCommentQuickReasons(clonedTermInfo);
    };

    const onChangeCheckedCustom = (value) => {
        const clonedCustomButtonName = _.cloneDeep(
            beforeModifiedButtonNameRef.current
        );
        let clonedIsCustomButtonName = _.cloneDeep(needCustomButton);
        clonedIsCustomButtonName = value;
        setNeedCustomButton(clonedIsCustomButtonName);
        setCustomButtonNames(clonedCustomButtonName);
    };

    const onChangeCustomButtonName = (value) => {
        let clonedCustomButtonName = _.cloneDeep(customButtonNames);
        clonedCustomButtonName = value;
        setCustomButtonNames(clonedCustomButtonName);
    };

    const onChangeAutoApprovedProcess = (value) => {
        let clonedAutoApprovedProcess = _.cloneDeep(autoApprovedProcess);
        clonedAutoApprovedProcess = value;
        setAutoApprovedProcess(clonedAutoApprovedProcess);
    };

    const onChangeDuration = (value) => {
        let clonedDuration = _.cloneDeep(duration);
        clonedDuration = value;
        setDuration(clonedDuration);
    };

    useEffect(() => {
        setCheckedOption();
    }, []);

    const onCancel = () => {
        setShowSettingPanel(false);
        const clonedQuickReasonInfo = _.cloneDeep(beforeQuickReasonRef.current);
        const clonedInactiveRejectInfo = _.cloneDeep(beforeInactiveRejectRef.current);
        const clonedCheckedQuickReason = _.cloneDeep(beforeCheckedQuickReasonRef.current);
        const clonedCheckedCustomButtonName = _.cloneDeep(beforeCheckedCustomButtonRef.current);
        const clonedCustomButtonName = _.cloneDeep(beforeModifiedButtonNameRef.current);
        const clonedCheckedAutoApprovedProcess = _.cloneDeep(beforeCheckedAutoApprovedProcessRef.current);
        const clonedDuration = _.cloneDeep(beforeDuration.current);
        setApprovalCommentQuickReasons(clonedQuickReasonInfo);
        setInactiveRejects(clonedInactiveRejectInfo);
        setNeedQuickReason(clonedCheckedQuickReason);
        setNeedCustomButton(clonedCheckedCustomButtonName);
        setAutoApprovedProcess(clonedCheckedAutoApprovedProcess);
        setCustomButtonNames(clonedCustomButtonName);
        setDuration(clonedDuration);
        setIsCheckingRuleBeforeDispose(beforeCheckedRecheckRuleRef.current);
        setEnableDeleteInvalidRecords(beforeEnableDeleteInvalidRecordsRef.current);
    };

    return (
        <div className="reco-manual-review-settings" hidden={!(checkPermission(RouterUrls.CP_Index) || checkPermission("RDM_ApprovalSetting", RM.UserResources))}>
            <R.Button
                primary={true}
                classify="theme"
                text={RMResx.RM_RDM_MA_ApprovalSettings}
                onClick={e => setShowSettingPanel(true)}
            />
            <ApprovalCommentSettingPanel
                show={showSettingPanel}
                onHide={onCancel}
                ApprovalComment={approvalCommentOptions}
                CheckedCommentOption={checkedCommentOption}
                ApprovalCommentQuickReason={approvalCommentQuickReasons}
                InactiveRejects={inactiveRejects}
                CustomButtons={customButtonNames}
                Duration={duration}
                NeedQuickReason={needQuickReason}
                NeedCustomButton={needCustomButton}
                AutoApprovedProcess={autoApprovedProcess}
                IsRecheckRule={isCheckingRuleBeforeDispose}
                enableDeleteInvalidRecords={enableDeleteInvalidRecords}
                StayManualReviewOption={stayManualReview}
                onSave={onSaveConfigration}
                onChange={onChangeCommentTermInfo}
                onChangeDisableTermInfo={onChangeDisableTermInfo}
                onChangeCheckedTerm={onChangeIsCheckTerm}
                onChangeCheckedCustom={onChangeCheckedCustom}
                onChangeCustomButtonName={onChangeCustomButtonName}
                onChangeAutoApprovedProcess={onChangeAutoApprovedProcess}
                onChangeDuration={onChangeDuration}
                onReload={onCancel}
                onRecheckRuleSetting={setIsCheckingRuleBeforeDispose}
                onEnableDeleteInvalidRecordsSetting={setEnableDeleteInvalidRecords}
                module={ApprovalSettingModule.ApprovalProcess}
            />
        </div>
    );
};

export default ManualApproveSettings;