import React, { useEffect, useState, useRef } from "react";

import { useDidUpdateEffect } from "../../Common/Hooks/index";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RouterUrls from "../../../Constants/RouterUrls";
import { MultipleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";

const getViewRequestOption = (id) => ({
    url: "/api/DisposalReport/Get",
    data: id
});

const getEditRequestOption = (reportInfo) => ({
    url: "/api/DisposalReport/Edit",
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

    const [applyRuleBeforeTime, setApplyRuleBeforeTime] = useState(RM.TimeUtil.getCommonDateStr(new Date()));

    const [initCheckedTreeStructure, setInitCheckedTreeStructure] = useState(null);

    const [checkedTreeStructure, setCheckedTreeStructure] = useState(null);

    const [isShowMessageBar, setIsShowMessageBar] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            const requestOption = getViewRequestOption(id);
            const disposalReportModel = await fetchUtility(requestOption);
            if(disposalReportModel === null) {
                return;
            }

            setProfileName(disposalReportModel.profileName);
            setDescription(disposalReportModel.description);
            setApplyRuleBeforeTime(disposalReportModel.applyRuleBeforeTime);
            setCheckedTreeStructure(JSON.parse(disposalReportModel.checkedTreeStructure));
            setInitCheckedTreeStructure(JSON.parse(disposalReportModel.checkedTreeStructure));
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

    }, [profileName, description, checkedTreeStructure, applyRuleBeforeTime]);

    const onSelectedDateChange = (args) => {
        const date = args.newValue;
        const newSelectedDate = RM.TimeUtil.getCommonDateStr(date);
        setApplyRuleBeforeTime(newSelectedDate);
    };

    const onCancel = () => {
        history.push({
            pathname: RouterUrls.RC_DueDisposalReportManagement,
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
            CheckedTreeStructure: JSON.stringify(checkedTreeStructure),
            ApplyRuleBeforeTime: applyRuleBeforeTime,
        };

        const requestOption = getEditRequestOption(reportInfo);
        const success = await fetchUtility(requestOption);
        $$.loading(false);

        if(!success) {
            return;
        }

        RM.CommStatus.save(RM.CommStatus.EditSuccess);
        setHasChange(false);
        history.push({
            pathname: RouterUrls.RC_DueDisposalReportManagement,
        });
    };

    return (
        <div className="reco-disposal-edit-wrapper">
            <section className="reco-disposal-header">
                <Prompt
                    message={RMResx.RM_JS_RC_TUR_CancelMessage}
                    when={hasChange}
                />
                <$g.SiteMap
                    data={[SiteMapLinks.RC_DueDisposalReportManagement, {text: RMResx.RM_JS_Common_Edit}]} />
            </section>
            <section className="reco-disposal-form-card">
                <div className="reco-disposal-form">
                    <div className="reco-disposal-form-item">
                        <div className="reco-disposal-input-title require">
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
                    <div className="reco-disposal-form-item">
                        <div className="reco-disposal-input-title">
                            {RMResx.RM_RC_Profile_Description}
                        </div>
                        <R.Input
                            type="textarea"
                            value={description}
                            onChange={value => setDescription(value)}
                            aria={{ ariaLabel: RMResx.RM_JS_Profile_Description }}
                        />
                        <div className="reco-disposal-input-desc">
                            {RMResx.RM_RC_Profile_Description_Tips}
                        </div>
                    </div>
                    <div className="reco-disposal-form-item">
                        <div className="reco-disposal-input-title require">
                            {RMResx.RM_RC_DueDisposal_SelectDate.replace(':', "")}
                        </div>
                        <div>
                            <R.Datepicker
                                selectedDate={new Date(applyRuleBeforeTime)}
                                width={360}
                                data-part="vtWidget"
                                dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                                hasTimePicker={true}
                                onChange={onSelectedDateChange}
                            />
                        </div>
                    </div>
                </div>
                <div className="reco-disposal-tips">
                    <div className="reco-disposal-tips-header">
                        <div className="reco-disposal-tips-icon fia-light"></div>
                        <div className="reco-disposal-tips-title" tabIndex="1">
                            {RMResx.RM_Report_SectionTitle_Introduction}
                        </div>
                    </div>
                    <div className="reco-disposal-tips-content" tabIndex="1">
                        {RMResx.RM_RC_DueDisposal_PageDescription}
                    </div>
                    <div className="reco-disposal-tips-picture"></div>
                </div>
            </section>
            <section className="reco-disposal-tree-card">
                <div style={{ marginBottom: 16 }}>
                    <div
                        className="reco-disposal-input-title require"
                        aria-label={RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                        tabIndex="1"
                    >
                        {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                    </div>
                    <div className="reco-disposal-message-bar" style={{ marginTop: 8 }}>
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
                <div className="reco-disposal-tree">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        onChecked={(structure) => setCheckedTreeStructure(structure)}
                        checkedTreeStructure={initCheckedTreeStructure}
                    />
                </div>
            </section>
            <section className="reco-disposal-placeholder">
            </section>
            <section className="reco-disposal-actions">
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