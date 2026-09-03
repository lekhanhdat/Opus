import React, { useState, useEffect, useRef } from "react";

import { useDidUpdateEffect } from "../../Common/Hooks/index";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RouterUrls from "../../../Constants/RouterUrls";
import { MultipleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";

import { ActionType, DateFrameType } from "./Constants";

const GetActionTypeCheckboxGroup = (actionType) => [
    {
        text: RMResx.RM_JS_RC_TimeFrame_Create,
        title: RMResx.RM_JS_RC_TimeFrame_Create,
        value: ActionType.Creation,
        checked: (ActionType.Creation | actionType) === actionType
    },
    {
        text: RMResx.RM_JS_RC_TimeFrame_Destroyed,
        title: RMResx.RM_JS_RC_TimeFrame_Destroyed,
        value: ActionType.Destruction,
        checked: (ActionType.Destruction | actionType) === actionType
    }
];

const GetDateFrameRadioGroup = (dateFrame) => [
    {
        text: RMResx.RM_RC_Audit_Range_5D,
        title: RMResx.RM_RC_Audit_Range_5D,
        value: DateFrameType.CurrentWeek,
        checked: dateFrame === DateFrameType.CurrentWeek
    },
    {
        text: RMResx.RM_RC_Audit_Range_1M,
        title: RMResx.RM_RC_Audit_Range_1M,
        value: DateFrameType.CurrentMonth,
        checked: dateFrame === DateFrameType.CurrentMonth
    },
    {
        text: RMResx.RM_RC_Audit_Range_3M,
        title: RMResx.RM_RC_Audit_Range_3M,
        value: DateFrameType.Last3Months,
        checked: dateFrame === DateFrameType.Last3Months
    },
    {
        text: RMResx.RM_RC_Audit_Range_6M,
        title: RMResx.RM_RC_Audit_Range_6M,
        value: DateFrameType.Last6Months,
        checked: dateFrame === DateFrameType.Last6Months
    },
    {
        text: RMResx.RM_RC_Audit_Range_Custom,
        title: RMResx.RM_RC_Audit_Range_Custom,
        value: DateFrameType.Custom,
        checked: dateFrame === DateFrameType.Custom
    }
];

const getViewRequestOption = (id) => ({
    url: "/api/CreateAndDestryoedReport/Get",
    data: id
});

const getEditRequestOption = (reportInfo) => ({
    url: "/api/CreateAndDestryoedReport/Edit",
    data: reportInfo
});

const Edit = ({ history }) => {

    const source = Number.parseInt(RM.Url.getParam(window.location.href, "source"));

    const id = Number.parseInt(RM.Url.getParam(window.location.href, "id"));

    const isInitChange = useRef(false);

    const [isCommited, setIsCommited] = useState(false);

    const [hasChange, setHasChange] = useState(false);

    const [profileName, setProfileName] = useState("");

    const [description, setDescription] = useState("");

    const [initCheckedTreeStructure, setInitCheckedTreeStructure] = useState(null);

    const [checkedTreeStructure, setCheckedTreeStructure] = useState(null);

    const [isShowMessageBar, setIsShowMessageBar] = useState(false);

    const [actionType, setActionType] = useState(ActionType.Creation | ActionType.Destruction);

    const [dateFrameType, setDateFrameType] = useState(DateFrameType.CurrentWeek);

    const [customStartDate, setCustomStartDate] = useState(RM.TimeUtil.getCommonDateStr(new Date()));

    const [customEndDate, setCustomEndDate] = useState(RM.TimeUtil.getCommonDateStr(new Date()));

    useEffect(() => {
        const fetchData = async () => {
            const requestOption = getViewRequestOption(id);
            const reportModel = await fetchUtility(requestOption);
            if (reportModel === null) {
                return;
            }

            setProfileName(reportModel.profileName);
            setDescription(reportModel.description);
            setActionType(reportModel.actionType);
            setDateFrameType(reportModel.dateFrameType);
            setCustomStartDate(reportModel.customStartDate);
            setCustomEndDate(reportModel.customEndDate);
            setCheckedTreeStructure(JSON.parse(reportModel.checkedTreeStructure));
            setInitCheckedTreeStructure(JSON.parse(reportModel.checkedTreeStructure));
            isInitChange.current = true;
        };

        fetchData();
    }, []);

    useDidUpdateEffect(() => {

        if(isInitChange.current) {
            isInitChange.current = false;
            return;
        }

        if (!hasChange) {
            setHasChange(true);
        }

        if (checkedTreeStructure === null || checkedTreeStructure === undefined) {
            setIsShowMessageBar(true);
        }

    }, [profileName, description, actionType, dateFrameType, checkedTreeStructure, customStartDate, customEndDate]);

    const onActionTypeCheckedChange = (checkedKeys) => {
        let actionType = ActionType.None;
        if (checkedKeys.some(item => item === ActionType.Creation)) {
            actionType |= ActionType.Creation;
        }

        if (checkedKeys.some(item => item === ActionType.Destruction)) {
            actionType |= ActionType.Destruction;
        }

        setActionType(actionType);
    };

    const onChangeCustomeDataRange = (args) => {
        const customDateRange = args.newValue;
        const startDate = RM.TimeUtil.getCommonDateStr(customDateRange.start);
        const endDate = RM.TimeUtil.getCommonDateStr(customDateRange.end);
        setCustomStartDate(startDate);
        setCustomEndDate(endDate);
    };

    const onCancel = () => {
        history.push({
            pathname: RouterUrls.RC_CreationAndDestructionReport,
        });
    };

    const onSave = async () => {
        const valid = (profileName !== null && profileName.length > 0) && checkedTreeStructure !== null;

        if (!valid) {
            setIsCommited(true);
            setIsShowMessageBar(true);
            return;
        }

        $$.loading(true);

        const reportInfo = {
            Source: source,
            Id: id,
            ProfileName: profileName,
            Description: description,
            ActionType: actionType,
            DateFrameType: dateFrameType,
            CustomStartDate: customStartDate,
            CustomEndDate: customEndDate,
            CheckedTreeStructure: JSON.stringify(checkedTreeStructure),
        };

        const requestOption = getEditRequestOption(reportInfo);
        const success = await fetchUtility(requestOption);
        $$.loading(false);

        if (!success) {
            return;
        }

        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
        setHasChange(false);
        history.push({
            pathname: RouterUrls.RC_CreationAndDestructionReport,
        });
    };

    return (
        <div className="reco-cad-create-wrapper">
            <section className="reco-cad-header">
                <Prompt
                    message={RMResx.RM_JS_RC_TUR_CancelMessage}
                    when={hasChange}
                />
                <$g.SiteMap
                    data={[SiteMapLinks.RC_CreationAndDestructionReport, { text: RMResx.RM_JS_Common_Create }]} />
            </section>
            <section className="reco-cad-form-card">
                <div className="reco-cad-form">
                    <div className="reco-cad-form-item">
                        <div className="reco-cad-input-title require">
                            {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                        </div>
                        <R.Input
                            type="text"
                            value={profileName}
                            onChange={value => setProfileName(value)}
                            aria={{ ariaLabel: RMResx.RM_JS_RC_DueDisposal_ProfileName }} />
                        <$g.ValidationMsg
                            show={isCommited && (profileName === null || profileName.length === 0)}
                        >
                            {RMResx.RM_RC_DueDisposal_NoProfileName}
                        </$g.ValidationMsg>
                    </div>
                    <div className="reco-cad-form-item">
                        <div className="reco-cad-input-title">
                            {RMResx.RM_RC_Profile_Description}
                        </div>
                        <R.Input
                            type="textarea"
                            value={description}
                            onChange={value => setDescription(value)}
                            aria={{ ariaLabel: RMResx.RM_JS_Profile_Description }}
                        />
                        <div className="reco-cad-input-desc">
                            {RMResx.RM_RC_Profile_Description_Tips}
                        </div>
                    </div>
                    <div className="reco-cad-form-item">
                        <div className="reco-cad-input-title require">
                            {RMResx.RM_JS_RC_TimeFrame_OprationType.replace(':', "")}
                        </div>
                        <R.Checkbox.Group
                            block
                            name="checkboxgroup-type"
                            items={GetActionTypeCheckboxGroup(actionType)}
                            onChange={onActionTypeCheckedChange}
                        />
                        <$g.ValidationMsg show={isCommited && actionType === ActionType.None}>
                            {RMResx.RM_JS_RC_TimeFrame_ChooseActionType}
                        </$g.ValidationMsg>
                    </div>
                    <div className="reco-cad-from-item">
                        <div className="reco-cad-input-title require">
                            {RMResx.RM_JS_RC_TimeFrame_Range.replace(':', "")}
                        </div>
                        <R.Radio.Group
                            block
                            name="radiogroup-type"
                            items={GetDateFrameRadioGroup(dateFrameType)}
                            onChange={(value) => setDateFrameType(value)}
                        />
                        <div className="recoc-cad-daterange-selector" style={{ marginTop: 8 }} hidden={dateFrameType !== DateFrameType.Custom}>
                            <R.Rangepicker
                                selectedDate={{
                                    start: new Date(customStartDate),
                                    end: new Date(customEndDate)
                                }}
                                data-part="vtWidget"
                                width={320}
                                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                                onChange={onChangeCustomeDataRange}
                            />
                        </div>
                    </div>
                </div>
                <div className="reco-cad-tips">
                    <div className="reco-cad-tips-header">
                        <div className="reco-cad-tips-icon fia-light"></div>
                        <div className="reco-cad-tips-title" tabIndex="1">
                            {RMResx.RM_Report_SectionTitle_Introduction}
                        </div>
                    </div>
                    <div className="reco-cad-tips-content" tabIndex="1">
                        {RMResx.RM_RC_DueDisposal_PageDescription}
                    </div>
                    <div className="reco-cad-tips-picture"></div>
                </div>
            </section>
            <section className="reco-cad-tree-card">
                <div style={{ marginBottom: 16 }}>
                    <div
                        className="reco-cad-input-title require"
                        aria-label={RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                        tabIndex="1"
                    >
                        {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                    </div>
                    <div className="reco-cad-message-bar" style={{ marginTop: 8 }}>
                        <R.Messagebar
                            message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
                            classify={"error"}
                            onClose={() => setIsShowMessageBar(false)}
                            status={{
                                show: isCommited &&
                                    isShowMessageBar &&
                                    (checkedTreeStructure === null)
                            }}
                        />
                    </div>
                </div>
                <div className="reco-cad-tree">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        onChecked={(structure) => setCheckedTreeStructure(structure)}
                        checkedTreeStructure={initCheckedTreeStructure}
                    />
                </div>
            </section>
            <section className="reco-cad-placeholder">
            </section>
            <section className="reco-cad-actions">
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={onCancel}
                />
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSave}
                />
            </section>
        </div>
    );
};

export default Edit;