import React, { useRef, useState, useEffect } from "react";

import { useDidUpdateEffect } from "../../Common/Hooks/index";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RouterUrls from "../../../Constants/RouterUrls";
import { MultipleChoiceSourceTree } from "../../Common/TreeComponents/SourceTree";
import { MultipleChoiceTermTree } from "../../Common/TreeComponents/TermTree";
import { TermUsageReportType } from "./Constants";

const getTermUsageReportType = (checkedTermUsageReportType) => [
    {
        text: RMResx.RM_JS_TermUsageReport_ActiveTermsReport,
        title: RMResx.RM_JS_TermUsageReport_ActiveTermsReport,
        value: TermUsageReportType.Active,
        checked: checkedTermUsageReportType === TermUsageReportType.Active
    },
    {
        text: RMResx.RM_JS_TermUsageReport_RetiredTermsReport,
        title: RMResx.RM_JS_TermUsageReport_RetiredTermsReport,
        value: TermUsageReportType.Retired,
        checked: checkedTermUsageReportType === TermUsageReportType.Retired
    },
    {
        text: RMResx.RM_JS_TermUsageReport_OrphanTermsReport,
        title: RMResx.RM_JS_TermUsageReport_OrphanTermsReport,
        value: TermUsageReportType.Orphaned,
        checked: checkedTermUsageReportType === TermUsageReportType.Orphaned
    }
];

const getViewRequestOption = (id) => ({
    url: "/api/TermUsageReport/Get",
    data: id
});

const getEditRequestOption = (reportInfo) => ({
    url: "/api/TermUsageReport/Edit",
    data: reportInfo
});

const Edit = ({ history }) => {

    const source = Number.parseInt(RM.Url.getParam(window.location.href, "source"));

    const id = Number.parseInt(RM.Url.getParam(window.location.href, "id"));

    const isInitChange = useRef(false);

    const termUsageReportTypeSourceTreeCache = useRef(new Map([[
        [TermUsageReportType.Active, null],
        [TermUsageReportType.Retired, null],
        [TermUsageReportType.Orphaned, null],
    ]]));

    const [isCommited, setIsCommited] = useState(false);

    const [hasChange, setHasChange] = useState(false);

    const [defaultCheckedTremUsageReportType, setDefaultCheckedTremUsageReportType] = useState(TermUsageReportType.None);

    const [profileName, setProfileName] = useState("");

    const [description, setDescription] = useState("");

    const [checkedTermUsageReportType, setCheckedTermUsageReportType] = useState(TermUsageReportType.Active);

    const [initCheckedSourceTreeStructure, setInitCheckedSourceTreeStructure] = useState(null);

    const [checkedSourceTreeStructure, setCheckedSourceTreeStructure] = useState(null);

    const [initCheckedTermTreeStructure, setInitCheckedTermTreeStructure] = useState(null);

    const [checkedTermTreeStructure, setCheckedTermTreeStructure] = useState(null);

    const [isShowTermMessageBar, setIsShowTermMessageBar] = useState(false);

    const [isShowMessageBar, setIsShowMessageBar] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            const requestOption = getViewRequestOption(id);
            const reportModel = await fetchUtility(requestOption);
            if (reportModel === null) {
                return;
            }

            isInitChange.current = true;
            setProfileName(reportModel.profileName);
            setDescription(reportModel.description);
            setCheckedTermUsageReportType(reportModel.termUsageReportType);
            setInitCheckedSourceTreeStructure(JSON.parse(reportModel.checkedSourceTreeStructure));
            setCheckedSourceTreeStructure(JSON.parse(reportModel.checkedSourceTreeStructure));
            setInitCheckedTermTreeStructure(JSON.parse(reportModel.checkedTermTreeStructure));
            setCheckedTermTreeStructure(JSON.parse(reportModel.checkedTermTreeStructure));
            setDefaultCheckedTremUsageReportType(reportModel.termUsageReportType);
            isInitChange.current = true;
        };

        fetchData();
    }, []);

    useDidUpdateEffect(() => {

        if (isInitChange.current) {
            isInitChange.current = false;
            return;
        }

        if (!hasChange) {
            setHasChange(true);
        }

        if (checkedSourceTreeStructure === null) {
            setIsShowMessageBar(true);
        }

        if (checkedTermTreeStructure === null) {
            setIsShowTermMessageBar(true);
        }

    }, [profileName, description, checkedSourceTreeStructure, checkedTermTreeStructure]);

    const onChangeTermUsageReportType = (checkedTermUsageReportType, oldCheckedTermUsageReportType) => {
        termUsageReportTypeSourceTreeCache.current.set(oldCheckedTermUsageReportType, checkedSourceTreeStructure);
        setCheckedSourceTreeStructure(termUsageReportTypeSourceTreeCache.current.get(checkedTermUsageReportType));
        setCheckedTermUsageReportType(checkedTermUsageReportType);
    };

    const onCancel = () => {
        history.push({
            pathname: RouterUrls.RC_TermUsageReportManagement,
        });
    };

    const onSave = async () => {
        const valid = (profileName !== null && profileName.length > 0) && checkedSourceTreeStructure !== null &&
            (checkedTermUsageReportType !== TermUsageReportType.Active || checkedTermTreeStructure !== null);

        if (!valid) {
            setIsCommited(true);
            setIsShowMessageBar(true);
            setIsShowTermMessageBar(true);
            return;
        }

        $$.loading(true);

        const reportInfo = {
            Source: source,
            Id: id,
            ProfileName: profileName,
            Description: description,
            TermUsageReportType: checkedTermUsageReportType,
            CheckedSourceTreeStructure: JSON.stringify(checkedSourceTreeStructure),
        };

        if (checkedTermUsageReportType === TermUsageReportType.Active) {
            reportInfo.CheckedTermTreeStructure = JSON.stringify(checkedTermTreeStructure);
        }

        const requestOption = getEditRequestOption(reportInfo);
        const success = await fetchUtility(requestOption);
        $$.loading(false);

        if (!success) {
            return;
        }

        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
        setHasChange(false);
        history.push({
            pathname: RouterUrls.RC_TermUsageReportManagement,
        });
    };

    return (
        <div className="reco-termusage-create-wrapper">
            <section className="reco-termusage-header">
                <Prompt
                    message={RMResx.RM_JS_RC_TUR_CancelMessage}
                    when={hasChange}
                />
                <$g.SiteMap
                    data={[SiteMapLinks.RC_TermUsageReport, { text: RMResx.RM_JS_Common_Create }]} />
            </section>
            <section className="reco-termusage-form-card">
                <div className="reco-termusage-form">
                    <div className="reco-termusage-form-item">
                        <div className="reco-termusage-input-title require">
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
                    <div className="reco-termusage-form-item">
                        <div className="reco-termusage-input-title">
                            {RMResx.RM_RC_Profile_Description}
                        </div>
                        <R.Input
                            type="textarea"
                            value={description}
                            onChange={value => setDescription(value)}
                            aria={{ ariaLabel: RMResx.RM_JS_Profile_Description }}
                        />
                        <div className="reco-termusage-input-desc">
                            {RMResx.RM_RC_Profile_Description_Tips}
                        </div>
                    </div>
                    <div className="reco-termusage-from-item">
                        <div className="reco-termusage-input-title require">
                            {RMResx.RM_JS_TermUsageReport_SelectReportType.replace(':', "")}
                        </div>
                        <R.Radio.Group
                            block
                            name="radiogroup-type"
                            items={getTermUsageReportType(checkedTermUsageReportType)}
                            onChange={onChangeTermUsageReportType}
                        />
                    </div>
                </div>
                <div className="reco-termusage-tips">
                    <div className="reco-termusage-tips-header">
                        <div className="reco-termusage-tips-icon fia-light"></div>
                        <div className="reco-termusage-tips-title" tabIndex="1">
                            {RMResx.RM_Report_SectionTitle_Introduction}
                        </div>
                    </div>
                    <div className="reco-termusage-tips-content" tabIndex="1">
                        {RMResx.RM_RC_DueDisposal_PageDescription}
                    </div>
                    <div className="reco-termusage-tips-picture"></div>
                </div>
            </section>
            <section className="reco-termusage-tree-card" hidden={checkedTermUsageReportType !== TermUsageReportType.Active}>
                <div className="reco-termusage-tree-left">
                    <div style={{ marginBottom: 16 }}>
                        <div
                            className="reco-termusage-input-title require"
                            aria-label={RMResx.RM_JS_TermUsageReport_TermIncludeReport.replace(":", "")}
                            tabIndex="1"
                        >
                            {RMResx.RM_JS_TermUsageReport_TermIncludeReport.replace(":", "")}
                        </div>
                        <div className="reco-termusage-message-bar" style={{ marginTop: 8 }}>
                            <R.Messagebar
                                message={RMResx.RM_JS_RC_TUR_NoTermSelected}
                                classify={"error"}
                                onClose={() => setIsShowMessageBar(false)}
                                status={{
                                    show: isCommited &&
                                        isShowTermMessageBar &&
                                        (checkedTermTreeStructure === null)
                                }}
                            />
                        </div>
                    </div>
                    <div className="reco-termusage-tree">
                        <MultipleChoiceTermTree
                            onChecked={(structure) => setCheckedTermTreeStructure(structure)}
                            checkedTreeStructure={defaultCheckedTremUsageReportType === TermUsageReportType.Active ? initCheckedTermTreeStructure : null}
                        />
                    </div>
                </div>
                <div className="reco-termusage-tree-right">
                    <div style={{ marginBottom: 16 }}>
                        <div
                            className="reco-termusage-input-title require"
                            aria-label={RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                            tabIndex="1"
                        >
                            {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                        </div>
                        <div className="reco-termusage-message-bar" style={{ marginTop: 8 }}>
                            <R.Messagebar
                                message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
                                classify={"error"}
                                onClose={() => setIsShowMessageBar(false)}
                                status={{
                                    show: isCommited &&
                                        isShowMessageBar &&
                                        (checkedSourceTreeStructure === null)
                                }}
                            />
                        </div>
                    </div>
                    <div className="reco-termusage-tree">
                        <MultipleChoiceSourceTree
                            sourceFlag={source}
                            onChecked={(structure) => setCheckedSourceTreeStructure(structure)}
                            checkedTreeStructure={defaultCheckedTremUsageReportType === TermUsageReportType.Active ? initCheckedSourceTreeStructure : null}
                        />
                    </div>
                </div>
            </section>
            <section className="reco-termusage-single-tree-card" hidden={checkedTermUsageReportType !== TermUsageReportType.Retired}>
                <div style={{ marginBottom: 16 }}>
                    <div
                        className="reco-termusage-input-title require"
                        aria-label={RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                        tabIndex="1"
                    >
                        {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                    </div>
                    <div className="reco-termusage-message-bar" style={{ marginTop: 8 }}>
                        <R.Messagebar
                            message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
                            classify={"error"}
                            onClose={() => setIsShowMessageBar(false)}
                            status={{
                                show: isCommited &&
                                    isShowMessageBar &&
                                    (checkedSourceTreeStructure === null)
                            }}
                        />
                    </div>
                </div>
                <div className="reco-termusage-tree">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        onChecked={(structure) => setCheckedSourceTreeStructure(structure)}
                        checkedTreeStructure={defaultCheckedTremUsageReportType === TermUsageReportType.Retired ? initCheckedSourceTreeStructure : null}
                    />
                </div>
            </section>
            <section className="reco-termusage-single-tree-card" hidden={checkedTermUsageReportType !== TermUsageReportType.Orphaned}>
                <div style={{ marginBottom: 16 }}>
                    <div
                        className="reco-termusage-input-title require"
                        aria-label={RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                        tabIndex="1"
                    >
                        {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                    </div>
                    <div className="reco-termusage-message-bar" style={{ marginTop: 8 }}>
                        <R.Messagebar
                            message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
                            classify={"error"}
                            onClose={() => setIsShowMessageBar(false)}
                            status={{
                                show: isCommited &&
                                    isShowMessageBar &&
                                    (checkedSourceTreeStructure === null)
                            }}
                        />
                    </div>
                </div>
                <div className="reco-termusage-tree">
                    <MultipleChoiceSourceTree
                        sourceFlag={source}
                        onChecked={(structure) => setCheckedSourceTreeStructure(structure)}
                        checkedTreeStructure={defaultCheckedTremUsageReportType === TermUsageReportType.Orphaned ? initCheckedSourceTreeStructure : null}
                    />
                </div>
            </section>
            <section className="reco-termusage-placeholder">
            </section>
            <section className="reco-termusage-actions">
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