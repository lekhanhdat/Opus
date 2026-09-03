import React, { useState, useEffect, useRef } from "react";
import { EnvironmentHelper, isShowActionByDC, showToast } from "../../../../Utilities/CommonUtil";
import { useStableCallback } from "../../../Common/Hooks";
import { ManualReviewAction, ManualReviewActionI18Ns } from "../Constants/ManualReviewActions";
import _ from "lodash";
import EmailNotification from "../Settings/EmailNotification";
import EscalationSetting from "../Settings/EscalatationSetting";
import DisposalExtention from "../Settings/DisposalExtention";
import { ApprovalStatus, EndType, EscalateSettingType, ExtendType, IntervalType, NotificationType } from "../Constants";

const TermMaxLength = 255;

const CustomButtonMaxLength = 20;

const DefaultSettingModel = {
    EmailNotificationSetting: {
        Interval: 1,
        IntervalType: IntervalType.Days,
        EndType: EndType.EndOccurrences,
        OccurrencesTimes: 3,
        ManualApprovalSettingType: NotificationType.Interval,
        AdvanceEmailSetting: [
            {
                Interval: 1,
                IntervalType: IntervalType.Days,
                CurrentStep: 1,
            },
        ],
    },
    EscalationSetting: {
        EscalateSettingType: EscalateSettingType.WorkflowNextStep,
        ApprovalStatus: ApprovalStatus.Rejected,
        ReassignUsers: [],
    },
    DisposalExtentionSetting: {
        MaxDelayTimes: 3,
        LatestExtendType: ExtendType.Month,
        LatestExtendNumber: 1,
    },
};
const isMultiGeoMainDC = isShowActionByDC();
const ApprovalCommentSettingPanel = (props) => {
    const {
        show, onHide, ApprovalComment, CheckedCommentOption, NeedQuickReason, NeedCustomButton, IsRecheckRule, enableDeleteInvalidRecords,
        AutoApprovedProcess, ApprovalCommentQuickReason, InactiveRejects, CustomButtons, onSave, Duration,
        onChange, onChangeDisableTermInfo, onChangeCheckedTerm, onChangeCheckedCustom, onChangeCustomButtonName,
        onChangeAutoApprovedProcess, onChangeDuration, StayManualReviewOption, onReload, module, onRecheckRuleSetting, onEnableDeleteInvalidRecordsSetting
    } = props;

    const [checkedItemValue, setCheckedItemValue] = useState(0);

    const [requiredComment, setRequiredComment] = useState(false);

    const [termToLongMsg, setTermToLongMsg] = useState(false);

    const [isNoneReason, setIsNoneReason] = useState(false);

    const [isDuplicateReason, setIsDuplicateReason] = useState(false);

    const [requiredButtonName, setRequiredButtonName] = useState(false);

    const [requiredDuration, setRequiredDuration] = useState(false);

    const [buttonNameTooLongMsg, setButtonNameTooLongMsg] = useState(false);
    

    const [stayManualReviewType, setStayManualReviewType] = useState(StayManualReviewOption);

    const esclateSettingRef = useRef();

    const disposalExtentionRef = useRef();

    const beforeSettingModelRef = useRef(DefaultSettingModel);

    const [settingModel, setSettingModel] = useState(DefaultSettingModel);

    useEffect(() => {
        show && setCheckedItemValue(CheckedCommentOption);
        !show && setRequiredComment(false);
        !show && setTermToLongMsg(false);
        !show && setIsNoneReason(false);
        !show && setIsDuplicateReason(false);
        !show && setRequiredButtonName(false);
        !show && setButtonNameTooLongMsg(false);
    }, [show]);

    useEffect(() => {
        const fetchData = async () => {
            const setting = await fetchUtility({ url: "/api/ManualApproval/GetSettingInfo" });
            if (!(setting.EmailNotificationSetting.AdvanceEmailSetting && setting.EmailNotificationSetting.AdvanceEmailSetting.length > 0)) {
                setting.EmailNotificationSetting.AdvanceEmailSetting = [{ Interval: 1, IntervalType: IntervalType.Days, CurrentStep: 1 }];
            }
            setSettingModel(setting);
            beforeSettingModelRef.current = setting;
        };
        fetchData();
    }, []);
    
    const onChangeEmailNotificationSetting = (value) => {
        const clonedSettingModel = _.cloneDeep(settingModel);
        clonedSettingModel.EmailNotificationSetting = value;
        setSettingModel(clonedSettingModel);
    };

    const onChangeDisposalExtentionSetting = (value) => {
        const clonedSettingModel = _.cloneDeep(settingModel);
        clonedSettingModel.DisposalExtentionSetting = value;
        setSettingModel(clonedSettingModel);
    };

    const onChangeEscalationSetting = (value) => {
        const clonedSettingModel = _.cloneDeep(settingModel);
        clonedSettingModel.EscalationSetting = value;
        setSettingModel(clonedSettingModel);
    };
    
    const PreCheckIsRequestComment = useStableCallback(async () => {
        let isRequired = false;
        if(NeedQuickReason){
            const seen = new Set();

            ApprovalCommentQuickReason.map(item => {
                const trimmedItem = item.trim();

                if(trimmedItem === ""){
                    isRequired = true;
                    setRequiredComment(true);
                }
                else if (item.length > TermMaxLength){
                    isRequired = true;
                    setTermToLongMsg(true);
                } else if (['None', 'Aucun', '空', '없음', 'なし'].includes(trimmedItem)) {
                    isRequired = true;
                    setIsNoneReason(true);
                } else if (seen.has(trimmedItem)) {
                    isRequired = true;
                    setIsDuplicateReason(true);
                } else {
                    seen.add(trimmedItem);
                }
            });
        }

        if(NeedCustomButton){
            CustomButtons.map(item => {
                if(item.englishName === "" || item.japaneseName === "" || item.chineseName === ""|| item.korean === ""){
                    isRequired = true;
                    setRequiredButtonName(true);
                }else if(item.englishName.length > CustomButtonMaxLength || item.japaneseName.length > CustomButtonMaxLength || item.chineseName.length > CustomButtonMaxLength|| item.korean.length > CustomButtonMaxLength ){
                    isRequired = true;
                    setButtonNameTooLongMsg(true);
                }
            });
        }

        if(!Duration){
            isRequired = true;
            setRequiredDuration(true);
        }
        
        return isRequired;
    });

    const onSaveConfigration = useStableCallback(async () =>
    {

        if(await PreCheckIsRequestComment()){
            return false;
        }
        
        if (settingModel.EmailNotificationSetting.ManualApprovalSettingType === NotificationType.Advanced) {
            settingModel.EmailNotificationSetting.AdvanceEmailSetting.forEach((advance, index) => {
                advance.CurrentStep = index + 1;
            });

            settingModel.EmailNotificationSetting.Interval = 1;
            settingModel.EmailNotificationSetting.IntervalType = IntervalType.Days;
            settingModel.EmailNotificationSetting.EndType = EndType.EndOccurrences;
            settingModel.EmailNotificationSetting.OccurrencesTimes = 3;
        } else {
            settingModel.EmailNotificationSetting.AdvanceEmailSetting = [{
                Interval: 1,
                IntervalType: IntervalType.Days,
                CurrentStep: 1
            }];
        }

        const clonedSettingModel = _.cloneDeep(settingModel);
        
        const dataApprovalSettingInfo = {
            commentSettingInfo: {
                option : checkedItemValue,
                commentSetting : {
                    manualApprovalQuickReasonInfo : 
                    {
                        needQuickReason : NeedQuickReason,
                        quickReasonInfo : ApprovalCommentQuickReason,
                        incativeRejectBool: InactiveRejects,
                    }
                },
                modifyButtonName : {
                    manualApprovalModifyButton : {
                        enableModifyButtonName : NeedCustomButton,
                        modifiedButtonNames : CustomButtons
                    }
                },
                enableAutoApprovedProcess: AutoApprovedProcess,
                isRecheckRule: !!IsRecheckRule,
                enableDeleteInvalidRecords: !!enableDeleteInvalidRecords,
                duration : Duration,
                stayManualReviewOption: stayManualReviewType
            },
            approvalProcessSetting: settingModel,
            module
        }

        let option = {
            url: "/api/ManualApproval/SaveApprovalSettingInfo",
            data: dataApprovalSettingInfo
        };

        if (!esclateSettingRef.current.onValidate()) {
            return false;
        }
        
        if (!disposalExtentionRef.current.onValidate()) {
            return false;
        }
        if(clonedSettingModel.EscalationSetting.EscalateSettingType === EscalateSettingType.WorkflowNextStep) {
            clonedSettingModel.EscalationSetting.ReassignUsers = [];
        }
        else {
            clonedSettingModel.EscalationSetting.ApprovalStatus = ApprovalStatus.Rejected;
        }
        setSettingModel(clonedSettingModel);
        beforeSettingModelRef.current = clonedSettingModel;

        $$.loading(true);
        let result = await onSave(option);
        $$.loading(false);

        if(!result) {
            showToast.error(RMResx.RM_MA_ApprovalComment_Failed);
            return false;
        }

        showToast.success(RMResx.RM_MA_ApprovalComment_Successful);

        if(onReload){
            onReload();
        }
    });

    const onChangeChecked = useStableCallback((args) =>{
        setCheckedItemValue(args.newValue.value);
    });

    const onChangeCheckBox = useStableCallback((args) =>{
        onChangeCheckedTerm(args);
        setRequiredComment(false);
        setTermToLongMsg(false);
        setIsNoneReason(false);
        setIsDuplicateReason(false);
    });

    const onChangeCustomButtonChecked = useStableCallback((args) =>{
        onChangeCheckedCustom(args);
        setRequiredButtonName(false);
        setButtonNameTooLongMsg(false);
    });

    const onChangeAutoApprovedProcessChecked = useStableCallback((args) =>{
        onChangeAutoApprovedProcess(args);
    });

    const removeCondition = (index) => {
        const clonedTermInfo = _.cloneDeep(ApprovalCommentQuickReason);
        const clonedInactiveRejectInfo = _.cloneDeep(InactiveRejects);
        clonedTermInfo.splice(index, 1);
        clonedInactiveRejectInfo.splice(index, 1);
        onChange(clonedTermInfo);
        onChangeDisableTermInfo(clonedInactiveRejectInfo);
        setRequiredComment(false);
        setTermToLongMsg(false);
        setIsNoneReason(false);
        setIsDuplicateReason(false);
    };

    const disabledCondition = (index, value) => {
        const clonedInactiveRejectInfo = _.cloneDeep(InactiveRejects);
        clonedInactiveRejectInfo[index] = value;
        onChangeDisableTermInfo(clonedInactiveRejectInfo);
    };

    const addCondition = (index) => {
        const clonedTermInfo = _.cloneDeep(ApprovalCommentQuickReason);
        const clonedInactiveRejectInfo = _.cloneDeep(InactiveRejects);
        clonedTermInfo.splice(index + 1, 0, "");
        clonedInactiveRejectInfo.splice(index + 1, 0, false);
        onChange(clonedTermInfo);
        onChangeDisableTermInfo(clonedInactiveRejectInfo);
        setRequiredComment(false);
        setTermToLongMsg(false);
        setIsNoneReason(false);
        setIsDuplicateReason(false);
    };

    const onChangeInputValue = (index, value) => {
        const clonedTermInfo = _.cloneDeep(ApprovalCommentQuickReason);
        clonedTermInfo[index] = value;
        onChange(clonedTermInfo);
        setRequiredComment(false);
        setTermToLongMsg(false);
        setIsNoneReason(false);
        setIsDuplicateReason(false);
    };

    const onChangeLanguageInputValue = (index, type, value) => {
        const clonedCustomButtonName = _.cloneDeep(CustomButtons);
        switch(type){
            case "english":
                clonedCustomButtonName[index].englishName = value;
                break;
            case "japanese":
                clonedCustomButtonName[index].japaneseName = value;
                break;
            case "chinese" :
                clonedCustomButtonName[index].chineseName = value;
                break;
            case "korean" :
                clonedCustomButtonName[index].korean = value;
                break;
            default :
                break;
        }
        onChangeCustomButtonName(clonedCustomButtonName);
        setRequiredButtonName(false);
        setButtonNameTooLongMsg(false);
    };

    const onChangeDurationInputValue = (value) => {
        let clonedDuration = _.cloneDeep(Duration);
        clonedDuration = value;
        onChangeDuration(clonedDuration);
        setRequiredDuration(false);
    };

    const onStayManualReviewChanged = (args) => {
        setStayManualReviewType(args);
    };

    const onCancel = () => {
        const clonedSettingModel = _.cloneDeep(beforeSettingModelRef.current);
        setSettingModel(clonedSettingModel);
        onHide();
    }

    const mapAdvanced = (advanced, index) => {
        return <div className="ra-advance-group-popup-row" key={`advanced_${index}`}>
            <div>
                <R.Input
                    key={Math.random()}
                    type="text"
                    min={1}
                    width={"100%"}
                    value={advanced}
                    hasControl
                    onChange={onChangeInputValue.bind(this, index)}
                />
            </div>
            {ApprovalCommentQuickReason.length > 1 && <R.Button
                type="bald"
                icon="crm-criteria fia-close"
                tooltip={RMResx.RM_JS_Common_Delete}
                onClick={removeCondition.bind(this, index)}
            />}
            {InactiveRejects[index] ? (
                <R.Button
                    type="bald"
                    icon="crm-criteria fia-activate"
                    tooltip={RMResx.RM_JS_Common_Enable}
                    onClick={disabledCondition.bind(this, index, false)}
                />
            ) : (
                <R.Button
                    type="bald"
                    icon="crm-criteria fia-deactivate"
                    tooltip={RMResx.RM_JS_Common_DisAble}
                    onClick={disabledCondition.bind(this, index, true)}
                />
            )}
            
            <R.Button
                type="bald"
                icon="crm-criteria fia-plus"
                tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                onClick={addCondition.bind(this, index)}
            />
        </div>;
    };

    const renderAdvanced = () => {
        return <div className="ra-config-panel-terms">
            <div className={ApprovalCommentQuickReason.length === 1 ? "ra-comment-term-after" : "ra-comment-term-group"}>
                {ApprovalCommentQuickReason.map((advanced, index) => {
                    return mapAdvanced(advanced, index);
                })}
            </div>
            <$g.ValidationMsg show={isNoneReason}>
                {RMResx.RM_MA_ApprovalComment_NotAllowNoneInput}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={isDuplicateReason}>
                {RMResx.RM_MA_ApprovalComment_DuplicateInput}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={requiredComment}>
                {RMResx.RM_MA_ApprovalComment_TermInputRequire}
            </$g.ValidationMsg>
            <$g.ValidationMsg show={termToLongMsg}>
                {RMResx.RM_JS_Common_Msg_CannotExceed255}
            </$g.ValidationMsg>
        </div>;
    };

    const renderCustomButtonHeader = () => {
        return <div className="ra-config-custom-button-first">
            <div tabIndex={0} aria-label={RMResx.RM_MA_CustomButton_Button}>
                {RMResx.RM_MA_CustomButton_Button}
            </div>
            <div tabIndex={0} aria-label={RMResx.RM_MA_CustomButton_English}>
                {RMResx.RM_MA_CustomButton_English}
            </div>
            <div tabIndex={0} aria-label={RMResx.RM_MA_CustomButton_Japanese}>
                {RMResx.RM_MA_CustomButton_Japanese}
            </div>
            <div tabIndex={0} aria-label={RMResx.RM_MA_CustomButton_Chinese}>
                {RMResx.RM_MA_CustomButton_Chinese}
            </div>
            <div tabIndex={0} aria-label={RMResx.RM_MA_CustomButton_Korean}>
                {RMResx.RM_MA_CustomButton_Korean}
            </div>
        </div>;
    };

    const renderCustomButtom = (button, index) => {
        return <div className="ra-config-custom-button">
            <div tabIndex={0}>
                {index === 0 ? ManualReviewActionI18Ns.get(ManualReviewAction.Approve) : ManualReviewActionI18Ns.get(ManualReviewAction.Reject)}
            </div>
            <div>
                <R.Input
                    key={Math.random()}
                    type="text"
                    min={1}
                    width={"100%"}
                    value={button.englishName}
                    hasControl
                    onChange={onChangeLanguageInputValue.bind(this, index, "english")}
                />
            </div>
            <div>
                <R.Input
                    key={Math.random()}
                    type="text"
                    min={1}
                    width={"100%"}
                    value={button.japaneseName}
                    hasControl
                    onChange={onChangeLanguageInputValue.bind(this, index, "japanese")}
                />
            </div>
            <div>
                <R.Input
                    key={Math.random()}
                    type="text"
                    min={1}
                    width={"100%"}
                    value={button.chineseName}
                    hasControl
                    onChange={onChangeLanguageInputValue.bind(this, index, "chinese")}
                />
            </div>
            <div>
                <R.Input
                    key={Math.random()}
                    type="text"
                    min={1}
                    width={"100%"}
                    value={button.korean}
                    hasControl
                    onChange={onChangeLanguageInputValue.bind(this, index, "korean")}
                />
            </div>
        </div>;
    };
    const renderMyhubReviewDueDate = () => {
        return <>
            <div>
                <span className="ra-config-myhub-review-title">{RMResx.RM_MA_Myhub_Title}</span>
                <div className="ra-config-myhub-review-des">
                    {RMResx.RM_MA_Myhub_Description}
                </div>
            </div>
            <div className="ra-config-myhub-review-due">
                <span>{RMResx.RM_MA_Duration_Title}</span>
            </div>
            <div>
                <R.Input
                    key={Math.random()}
                    type="number"
                    min={1}
                    max={366}
                    width={200}
                    value={Duration}
                    hasControl
                    onChange={onChangeDurationInputValue.bind(this)}
                />
                {" " + RMResx.RM_JS_ScheduleSetting_Days}
            </div>
        </>;
    };

    return ( 
        <R.Panel
            header={RMResx.RM_MA_ApprovalComment_Configuration}
            size={670}
            status={{ show : show }}
            destroy={true}
            onHide={onCancel}
        >
            <div className="ra-config-panel">
                <div>
                    <EmailNotification
                        emailNotificationSetting={
                            settingModel.EmailNotificationSetting
                        }
                        onChange={onChangeEmailNotificationSetting}
                    />
                    <EscalationSetting
                        escalationSetting={settingModel.EscalationSetting}
                        onChange={onChangeEscalationSetting}
                        ref={esclateSettingRef}
                    />
                    <DisposalExtention
                        disposalExtentionSetting={
                            settingModel.DisposalExtentionSetting
                        }
                        onChange={onChangeDisposalExtentionSetting}
                        ref={disposalExtentionRef}
                    />
                </div>
                <div className="margin-top-l margin-bottom-l" style={{ backgroundColor: '#E8E9EA', height: 1}}></div>
                <div className="ra-config-panel-title require" tabIndex="0">
                    <$g.I18NProvider msg={RMResx.RM_MA_ApprovalComment_RequireTitle} />
                </div>
                <div>
                    <R.Combobox
                        id="option"
                        textField="text"
                        valueField="value"
                        checkedField="checked"
                        tooltipField="tooltip"
                        width="100%"
                        items={ApprovalComment}
                        onChange={onChangeChecked}
                        aria={{
                            ariaLabelledby: "ariaUrl",
                            ariaRequired: true
                        }}
                        searchable={false}
                    />
                </div>
                <div className="ra-config-panel-term" tabIndex="0">
                    <span className="flex align-center">
                        <R.Switch
                            checked={NeedQuickReason}
                            onChange={onChangeCheckBox}>
                        </R.Switch>
                        <div className="ra-config-panel-term-title">{RMResx.RM_MA_ApprovalComment_ConfigTerms}
                            <$g.Popover>{RMResx.RM_MA_ApprovalComment_QuickReasonMsg}</$g.Popover>
                        </div>
                    </span>
                </div>
                {NeedQuickReason && renderAdvanced()}
                <div className="ra-config-panel-term" tabIndex="0">
                    <span className="flex align-center">
                        <R.Switch
                            checked={NeedCustomButton}
                            onChange={onChangeCustomButtonChecked}>
                        </R.Switch>
                        <div className="ra-config-panel-term-title">
                            {RMResx.RM_MA_ApprovalComment_CustomButton}
                            <$g.Popover>{RMResx.RM_MA_ApprovalComment_CustomButtonMsg}</$g.Popover>
                        </div>
                    </span>
                </div>
                {NeedCustomButton && 
                    <div>
                        {renderCustomButtonHeader()}
                        {CustomButtons.map((button,index) => renderCustomButtom(button, index))}
                        <$g.ValidationMsg show={requiredButtonName}>
                            {RMResx.RM_MA_ApprovalComment_TermInputRequire}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg show={buttonNameTooLongMsg}>
                            {RMResx.RM_MA_ApprovalComment_ButtonNameLimit}
                        </$g.ValidationMsg>
                    </div>
                }
                {RM.gData.enableRecordsArchiver && <div className="ra-config-panel-term" tabIndex="0">
                    <span className="flex align-center">
                        <R.Switch
                            checked={AutoApprovedProcess}
                            onChange={onChangeAutoApprovedProcessChecked}>
                        </R.Switch>
                        <div className="ra-config-panel-term-title flex align-center">
                            <p data-tooltip="ifneed">{RMResx.RM_MA_ApprovalComment_AutoApproved}</p>
                            <$g.Popover>{RMResx.RM_MA_ApprovalComment_AutoApproved_Desc}</$g.Popover>
                        </div>
                    </span>
                    {AutoApprovedProcess && (
                        <div className="padding-left-l margin-left-xl flex flex-column align-start font-regular">
                            <div className="flex align-center">
                                <R.Checkbox
                                    text={RMResx.RM_MA_ApprovalComment_RecheckRule}
                                    title={RMResx.RM_MA_ApprovalComment_RecheckRule}
                                    checked={IsRecheckRule}
                                    onChange={(args) => onRecheckRuleSetting(args)}
                                />
                                <$g.Popover>{RMResx.RM_MA_ApprovalComment_RecheckRule_Desc}</$g.Popover>
                            </div>
                            <R.Checkbox
                                text={RMResx.RM_MA_ApprovalComment_DeleteInvalidRecord}
                                title={RMResx.RM_MA_ApprovalComment_DeleteInvalidRecord}
                                checked={enableDeleteInvalidRecords}
                                onChange={(args) => onEnableDeleteInvalidRecordsSetting(args)}
                            />
                        </div>
                    )}
                </div>}
                {!EnvironmentHelper.IsGCPEnvironment &&
                    <>
                        <div className="ra-config-line"></div>
                        <div>
                            {renderMyhubReviewDueDate()}
                            <$g.ValidationMsg show={requiredDuration}>
                                {RMResx.RM_MA_ApprovalComment_TermInputRequire}
                            </$g.ValidationMsg>
                        </div>
                    </>
                }
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onHide} />
                {isMultiGeoMainDC && <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSaveConfigration} />}
            </>
        </R.Panel>
    );
};

export default ApprovalCommentSettingPanel;