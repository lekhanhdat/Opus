import { Component, Fragment } from "react";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { JobType, SourceFlags } from "../../../Constants/Constants";
import { bindEvents, showToast } from "../../../Utilities/CommonUtil";
import SPTree from "../../../Components/Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates from "../../../Components/Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import EXOTree from "../../../Components/Common/Tree/Instances/EXO/ReportEXOTree";
import FSTree from "../../../Components/Common/Tree/Instances/FSTree/ReportFSTree";
import TermTree from "../../../Components/Common/Tree/Instances/TermTree/ReportTermTree";
import ReportBoxTree from "../../Common/Tree/Instances/BoxTree/ReportBoxTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import RouterUrls from "../../../Constants/RouterUrls";
import "../../../Less/RC/commonReportProfile.less";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "showMessageTip", "hideMessageTip", "onSearchTermTree", "onStopSearchTermTree", "onSearchSourceTree", "onStopSearchSourceTree",
            "onTriggerSourceTreeError", "onTriggerTermTreeError", "onNameChange", "onNameBlur", "onDescriptionChange", "onReportTypeChange", "onSave",
            "onCancel", "routerTo", "onTreeChanged", "onTermTreeNodeSelectedChange", "onSourceTreeNodeSelectedChange"
        );

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;

        let profile = {
            Type: RM.Url.getParam(window.location.href, "type") || JobType.BCSTermUsageReport,
        };
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.state = {
            isRender: false,
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            sourceTreeData: null,
            termTreeData: null,
            showSourceTreeError: false,
            showTermTreeError: false,
            showRequireNameMsg: false,
            profile: profile,
            reportTypes: this.getReportTypes(),
            settingsChanged: false,
            showRequireNameTooLongMsg: false,
            showDescriptionTooLongMsg: false,
        };
    }

    componentDidMount() {
        if (this.isEdit) {
            this.initProfileData();
        } else {
            this.setState({ isRender: true });
        }
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }

    getReportTypes() {
        let defaultJobType = RM.Url.getParam(window.location.href, "type");
        let activeTermsReportValue = JobType.BCSTermUsageReport;
        let retiredTermsReportValue = JobType.RetiredTermReport;
        let orphanTermsReportValue = JobType.OrphanedTermReport;

        if (defaultJobType == JobType.EXOTermUsageReport
            || defaultJobType == JobType.EXOOrphanedTermUsageReport
            || defaultJobType == JobType.EXORetiredTermUsageReport) {
            activeTermsReportValue = JobType.EXOTermUsageReport;
            retiredTermsReportValue = JobType.EXORetiredTermUsageReport;
            orphanTermsReportValue = JobType.EXOOrphanedTermUsageReport;
        } else if (defaultJobType == JobType.PhysicalTermUsageReport
            || defaultJobType == JobType.PhysicalOrphanedTermUsageReport
            || defaultJobType == JobType.PhysicalRetiredTermUsageReport) {
            activeTermsReportValue = JobType.PhysicalTermUsageReport;
            retiredTermsReportValue = JobType.PhysicalRetiredTermUsageReport;
            orphanTermsReportValue = JobType.PhysicalOrphanedTermUsageReport;
        } else if (defaultJobType == JobType.FSBCSTermUsageReport
            || defaultJobType == JobType.FSOrphanedTermReport
            || defaultJobType == JobType.FSRetiredTermReport) {
            activeTermsReportValue = JobType.FSBCSTermUsageReport;
            retiredTermsReportValue = JobType.FSRetiredTermReport;
            orphanTermsReportValue = JobType.FSOrphanedTermReport;
        } else if (defaultJobType == JobType.OneDriveTermUsageReport
            || defaultJobType == JobType.OneDriveOrphanedTermReport
            || defaultJobType == JobType.OneDriveRetiredTermReport) {
            activeTermsReportValue = JobType.OneDriveTermUsageReport;
            retiredTermsReportValue = JobType.OneDriveRetiredTermReport;
            orphanTermsReportValue = JobType.OneDriveOrphanedTermReport;
        } else if (defaultJobType == JobType.SPOnPremiseTermUsageReport
            || defaultJobType == JobType.SPOnPremiseOrphanedTermUsageReport
            || defaultJobType == JobType.SPOnPremiseRetiredTermUsageReport) {
            activeTermsReportValue = JobType.SPOnPremiseTermUsageReport;
            retiredTermsReportValue = JobType.SPOnPremiseRetiredTermUsageReport;
            orphanTermsReportValue = JobType.SPOnPremiseOrphanedTermUsageReport;
        } else if (defaultJobType == JobType.BoxBCSTermUsageReport
            || defaultJobType == JobType.BoxOrphanedTermUsageReport
            || defaultJobType == JobType.BoxRetiredTermUsageReport) {
            activeTermsReportValue = JobType.BoxBCSTermUsageReport;
            retiredTermsReportValue = JobType.BoxRetiredTermUsageReport;
            orphanTermsReportValue = JobType.BoxOrphanedTermUsageReport;
        } else if (defaultJobType == JobType.GoogleBCSTermUsageReport
            || defaultJobType == JobType.GoogleOrphanedTermUsageReport
            || defaultJobType == JobType.GoogleRetiredTermUsageReport) {
            activeTermsReportValue = JobType.GoogleBCSTermUsageReport;
            retiredTermsReportValue = JobType.GoogleRetiredTermUsageReport;
            orphanTermsReportValue = JobType.GoogleOrphanedTermUsageReport;
        } else if (defaultJobType == JobType.TeamsBCSTermUsageReport
            || defaultJobType == JobType.TeamsOrphanedTermUsageReport
            || defaultJobType == JobType.TeamsRetiredTermUsageReport) {
            activeTermsReportValue = JobType.TeamsBCSTermUsageReport;
            retiredTermsReportValue = JobType.TeamsRetiredTermUsageReport;
            orphanTermsReportValue = JobType.TeamsOrphanedTermUsageReport;
        }

        return [
            {
                text: RMResx.RM_JS_TermUsageReport_ActiveTermsReport,
                title: RMResx.RM_JS_TermUsageReport_ActiveTermsReport,
                value: activeTermsReportValue + "",
                checked: true
            },
            {
                text: RMResx.RM_JS_TermUsageReport_RetiredTermsReport,
                title: RMResx.RM_JS_TermUsageReport_RetiredTermsReport,
                value: retiredTermsReportValue + "",
                checked: false
            },
            {
                text: RMResx.RM_JS_TermUsageReport_OrphanTermsReport,
                title: RMResx.RM_JS_TermUsageReport_OrphanTermsReport,
                value: orphanTermsReportValue + "",
                checked: false
            }
        ];
    }

    initProfileData() {
        $$.loading(true);
        fetchUtility({
            url: "/api/TermUsageReportApi/LoadProfileById",
            data: this.profileId
        }).then((data) => {
            $$.loading(false);
            this.setSelectReportType(data.Type);
            if (!data.Extension1) {
                data.Extension1 = null;
            }
            this.setState({
                profile: data,
                termTreeData: $.parseJSON(data.Extension1),
                sourceTreeData: $.parseJSON(data.Extension2),
                isRender: true
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onNameChange(value) {

        let profile = this.state.profile;
        if (value.length == 0)
        {
            this.setState({ showRequireNameMsg: true });
        }
        else
        {
            this.setState({ showRequireNameMsg: false });
        }         
        if (value.length > this.NameMaxLength)
        {
            this.setState({ showRequireNameTooLongMsg: true });
        }
        else
        {          
            this.setState({ showRequireNameTooLongMsg: false });  
            profile.ProfileName = value.trim();
            this.setState({ profile: profile, settingsChanged: true });
        }
    }

    onNameBlur(args) {

        // setTimeout(() => {
        //     let showRequireNameMsg = false;
        //     if ($.trim(args.value).length == 0) {
        //         showRequireNameMsg = true;
        //     }
        //     this.setState({ showRequireNameMsg: showRequireNameMsg });
        // }, 100);
    }

    onDescriptionChange(value) {

        let profile = this.state.profile;
        if (value.length > this.DespMaxLength)
        {
            this.setState({ showDescriptionTooLongMsg: true });
        }
        else
        {
            this.setState({ showDescriptionTooLongMsg: false }); 
            profile.Description = value;
            this.setState({ profile: profile, settingsChanged: true });
        }
    }

    onReportTypeChange(value) {
        let selValue = value;
        let profile = this.state.profile;
        // this.setSelectReportType(selValue);
        profile.Type = selValue;
        this.setState({
            profile: profile,
            // reportTypes: this.state.reportTypes,
            settingsChanged: true
        });
    }
    setSelectReportType(type) {
        let items = RM.deepcopy(this.state.reportTypes);
        for (const item of items) {
            if (parseInt(item.value) === type) {
                item.checked = true;
            } else {
                item.checked = false;
            }
        }
        this.setState({ reportTypes: items });
    }

    onSearchSourceTree(args) {
        this.setState({ sourceSearchKey: args });
    }

    onStopSearchSourceTree() {
        this.setState({ sourceSearchKey: "" });
    }

    onSearchTermTree(args) {
        this.setState({ termSearchKey: args });
    }

    onStopSearchTermTree() {
        this.setState({ termSearchKey: "" });
    }

    onSave() {
        let profile = this.state.profile;
        let hasTermScope = this.isShowTermTree();
        let extension1 = null;
        let validSuccess = true;
        let newState = {};


        if (this.state.showRequireNameTooLongMsg || this.state.showDescriptionTooLongMsg || this.state.showRequireNameMsg) {
            validSuccess = false;
        }
        else if (profile.ProfileName == null) {
            this.setState({ showRequireNameMsg: true });
            validSuccess = false;
        }
        if (hasTermScope) {
            let termTreeData = this.refTermTree.getTreeData();
            if (termTreeData.selected) {
                extension1 = JSON.stringify(termTreeData.items);
                newState.showTermTreeError = false;
            } else {
                validSuccess = false;
                newState.showTermTreeError = true;
            }
        }

        let sourceTreeData = this.refSourceTree.getTreeData();
        if (sourceTreeData.selected) {
            newState.showSourceTreeError = false;
        } else {
            validSuccess = false;
            newState.showSourceTreeError = true;
        }

        if (validSuccess) {
            profile.Extension1 = extension1;
            profile.Extension2 = JSON.stringify(sourceTreeData.items);
            let option = {
                url: this.isEdit ? "/api/TermUsageReportApi/EditProfile" : "/api/TermUsageReportApi/CreateProfile",
                data: profile
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((res) => {
                $$.loading(false);
                if (res == "") {
                    if (this.isEdit) {
                        RM.CommStatus.save(RM.CommStatus.EditSuccess);
                    } else {
                        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                    }
                    this.setState({ settingsChanged: false });
                    this.routerTo(RouterUrls.RC_TermUsageReportManagement);
                } else {
                    this.showMessageTip(
                        "error",
                        (this.isEdit ? RMResx.RM_JS_RC_TUR_EditProfileFaild : RMResx.RM_JS_RC_TUR_CreateProfileFaild).format(res));
                }
            }).catch((e) => {
                $$.loading(false);
            });
        }

        this.setState(newState);
    }

    handleError(response) {
        $$.loading(false);
        if (response.status == 403) {
            this.showMessageTip(
                "error",
                RMResx.RM_JS_RC_TermUsage_SaveReportFailed);
        }

    }

    getSourceFlagBySource() {
        let profileType = this.state.profile.Type;
        if (profileType == JobType.BCSTermUsageReport || profileType == JobType.OrphanedTermReport
            || profileType == JobType.RetiredTermReport) {
            return SourceFlags.SP;
        } else if (profileType == JobType.EXOTermUsageReport || profileType == JobType.EXOOrphanedTermUsageReport
            || profileType == JobType.EXORetiredTermUsageReport) {
            return SourceFlags.Exo;
        } else if (profileType == JobType.PhysicalTermUsageReport || profileType == JobType.PhysicalOrphanedTermUsageReport
            || profileType == JobType.PhysicalRetiredTermUsageReport) {
            return SourceFlags.Phy;
        } else if (profileType == JobType.FSBCSTermUsageReport || profileType == JobType.FSOrphanedTermReport
            || profileType == JobType.FSRetiredTermReport) {
            return SourceFlags.FS;
        } else if (profileType == JobType.OneDriveTermUsageReport || profileType == JobType.OneDriveOrphanedTermReport
            || profileType == JobType.OneDriveRetiredTermReport) {
            return SourceFlags.OneDrive;
        } else if (profileType == JobType.SPOnPremiseTermUsageReport || profileType == JobType.SPOnPremiseOrphanedTermUsageReport
            || profileType == JobType.SPOnPremiseRetiredTermUsageReport) {
            return SourceFlags.SPLocal;
        } else if (profileType == JobType.BoxBCSTermUsageReport || profileType == JobType.BoxOrphanedTermUsageReport
            || profileType == JobType.BoxRetiredTermUsageReport) {
            return SourceFlags.Box;
        } else if (profileType == JobType.GoogleBCSTermUsageReport || profileType == JobType.GoogleOrphanedTermUsageReport
            || profileType == JobType.GoogleRetiredTermUsageReport) {
            return SourceFlags.Google;
        } else if (profileType == JobType.TeamsBCSTermUsageReport || profileType == JobType.TeamsOrphanedTermUsageReport
            || profileType == JobType.TeamsRetiredTermUsageReport) {
            return SourceFlags.Teams;
        }
    }

    onCancel() {
        this.routerTo(RouterUrls.RC_TermUsageReportManagement);
    }

    onTreeChanged() {
        this.setState({ settingsChanged: true });
    }

    onSourceTreeNodeSelectedChange() {
        this.setState({ showSourceTreeError: false });
    }

    onTermTreeNodeSelectedChange() {
        this.setState({ showTermTreeError: false });
    }

    showMessageTip(type, msg) {
        showToast._showMsg(type, msg);
    }
    hideMessageTip() {
        this.setState({ tipStatus: { show: false } });
    }
    onTriggerSourceTreeError(show) {
        this.setState({ showSourceTreeError: show });
    }
    onTriggerTermTreeError(show) {
        this.setState({ showTermTreeError: show });
    }

    isShowTermTree() {
        let profileType = this.state.profile.Type;
        return profileType == JobType.BCSTermUsageReport
            || profileType == JobType.EXOTermUsageReport
            || profileType == JobType.PhysicalTermUsageReport
            || profileType == JobType.FSBCSTermUsageReport
            || profileType == JobType.OneDriveTermUsageReport
            || profileType == JobType.SPOnPremiseTermUsageReport
            || profileType == JobType.BoxBCSTermUsageReport
            || profileType == JobType.GoogleBCSTermUsageReport
            || profileType == JobType.TeamsBCSTermUsageReport;
    }

    renderSourceTree() {
        if (!this.isEdit || (this.isEdit && this.state.sourceTreeData)) {
            let SourceTree = null;
            let profileType = this.state.profile.Type;
            let sourceTreeFlags = SourceFlags.SP;
            if (profileType == JobType.BCSTermUsageReport || profileType == JobType.OrphanedTermReport
                || profileType == JobType.RetiredTermReport) {
                SourceTree = TreeWithTreStates;
            } else if (profileType == JobType.EXOTermUsageReport || profileType == JobType.EXOOrphanedTermUsageReport
                || profileType == JobType.EXORetiredTermUsageReport) {
                SourceTree = EXOTree;
            } else if (profileType == JobType.PhysicalTermUsageReport || profileType == JobType.PhysicalOrphanedTermUsageReport
                || profileType == JobType.PhysicalRetiredTermUsageReport) {
                SourceTree = LocationTree;
            } else if (profileType == JobType.FSBCSTermUsageReport || profileType == JobType.FSOrphanedTermReport
                || profileType == JobType.FSRetiredTermReport) {
                SourceTree = FSTree;
                sourceTreeFlags = SourceFlags.FS;
            } else if (profileType == JobType.OneDriveTermUsageReport || profileType == JobType.OneDriveOrphanedTermReport
                || profileType == JobType.OneDriveRetiredTermReport) {
                SourceTree = TreeWithTreStates;
                sourceTreeFlags = SourceFlags.OneDrive;
            } else if (profileType == JobType.SPOnPremiseTermUsageReport || profileType == JobType.SPOnPremiseOrphanedTermUsageReport
                || profileType == JobType.SPOnPremiseRetiredTermUsageReport) {
                SourceTree = SPTree;
                sourceTreeFlags = SourceFlags.SPLocal;
            } else if (profileType == JobType.BoxBCSTermUsageReport || profileType == JobType.BoxOrphanedTermUsageReport
                || profileType == JobType.BoxRetiredTermUsageReport) {
                SourceTree = ReportBoxTree;
                sourceTreeFlags = SourceFlags.Box;
            } else if (profileType == JobType.GoogleBCSTermUsageReport || profileType == JobType.GoogleOrphanedTermUsageReport
                || profileType == JobType.GoogleRetiredTermUsageReport) {
                SourceTree = ReportGoogleTree;
                sourceTreeFlags = SourceFlags.Google;
            } else if (profileType == JobType.TeamsBCSTermUsageReport || profileType == JobType.TeamsOrphanedTermUsageReport
                || profileType == JobType.TeamsRetiredTermUsageReport) {
                SourceTree = ReportTeamsTree;
                sourceTreeFlags = SourceFlags.Teams;
            }

            if (SourceTree) {
                return <SourceTree
                    ref={r => this.refSourceTree = r}
                    searchKey={this.state.sourceSearchKey}
                    data={this.state.sourceTreeData}
                    onTreeChanged={this.onTreeChanged}
                    onNodeSelectedChange={this.onSourceTreeNodeSelectedChange}
                    treeSource={sourceTreeFlags}
                />;
            }
        }
    }

    render() {
        let showTermTree = this.isShowTermTree();

        return (
            <Fragment>
                {this.state.isRender &&
                    <div className="reco-report-profile-wrapper">
                        <section className="reco-report-profile-header">
                            <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.settingsChanged} />
                            <$g.SiteMap data={[SiteMapLinks.RC_TermUsageReport, { text: this.isEdit ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
                        </section>
                        <section className="reco-report-profile-card">
                            <div className="reco-report-profile-form">
                                <div className="reco-report-profile-form-item">
                                    <span className="reco-report-profile-input-title-require">
                                        {RMResx.RM_JS_TermUsageReport_ProfileName}
                                    </span>
                                    <R.Input type="text" id="raRcTurProfileNameIpt"
                                        value={this.state.profile.ProfileName} onChange={this.onNameChange} onBlur={this.onNameBlur} aria={{ ariaLabel: RMResx.RM_JS_TermUsageReport_ProfileName }} />
                                    <$g.ValidationMsg show={this.state.showRequireNameMsg}>
                                        {RMResx.RM_RC_DueDisposal_NoProfileName}
                                    </$g.ValidationMsg>
                                    <$g.ValidationMsg show={this.state.showRequireNameTooLongMsg}>
                                        {RMResx.RM_RC_DueDisposal_ProfileNameTooLong}
                                    </$g.ValidationMsg>
                                </div>
                                <div className="reco-report-profile-form-item">
                                    <span className="reco-report-profile-input-title">
                                        {RMResx.RM_RC_Profile_Description}
                                    </span>
                                    <R.Input type="textarea" value={this.state.profile.Description} onChange={this.onDescriptionChange} aria={{ ariaLabel: RMResx.RM_JS_Profile_Description }} />
                                    <span className="reco-report-profile-input-desc">
                                        {RMResx.RM_RC_Profile_Description_Tips}
                                    </span>
                                    <$g.ValidationMsg show={this.state.showDescriptionTooLongMsg}>
                                        {RMResx.RM_RC_DueDisposal_DescriptionTooLong}
                                    </$g.ValidationMsg>
                                </div>
                                <div className="reco-report-profile-form-item">
                                    <span className="reco-report-profile-input-title-require">
                                        {RMResx.RM_JS_TermUsageReport_SelectReportType.replace(':', "")}
                                    </span>
                                    <R.Radio.Group
                                        block
                                        name="radiogroup-type"
                                        items={this.state.reportTypes}
                                        onChange={this.onReportTypeChange}
                                    />
                                </div>
                            </div>
                            <div className="reco-report-profile-tips">
                                <div className="reco-report-profile-tips-header">
                                    <span className="reco-report-profile-tips-icon fia-light">
                                    </span>
                                    <span className="reco-report-profile-tips-header-title" tabIndex="0">
                                        {RMResx.RM_Report_SectionTitle_Introduction}
                                    </span>
                                </div>
                                <div className="reco-report-profile-tips-content" tabIndex="0">
                                    {RMResx.RM_JS_RC_TUR_PageDescription}
                                </div>
                                <div className="reco-report-profile-tips-pic"></div>
                            </div>
                        </section>
                        <section className={showTermTree ? "reco-report-profile-tree-card" : "reco-report-profile-tree-single-card"}>
                            {
                                showTermTree &&
                                <div className="reco-report-profile-tree-left">
                                    <div className="reco-report-profile-tree-search-item">
                                        <div className="reco-report-profile-tree-input-title require" tabIndex="0">
                                            {RMResx.RM_JS_TermUsageReport_TermIncludeReport.replace(":", "")}
                                        </div>
                                        <R.Searchbox
                                            placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                                            disabled={false}
                                            width={360}
                                            onSearch={(args) => (args || "").trim() === "" ? this.onStopSearchTermTree() : this.onSearchTermTree(args)}
                                        />
                                        <div className="reco-report-profile-tree-search-message">
                                            <R.Messagebar
                                                message={RMResx.RM_JS_RC_TUR_NoTermSelected}
                                                classify={"error"}
                                                onClose={this.onTriggerTermTreeError.bind(this, false)}
                                                status={{ show: this.state.showTermTreeError }}
                                            />
                                        </div>
                                    </div>
                                    <div className="reco-report-profile-tree">
                                        <TermTree
                                            ref={r => this.refTermTree = r}
                                            searchKey={this.state.termSearchKey}
                                            data={this.state.termTreeData}
                                            onTreeChanged={this.onTreeChanged}
                                            onNodeSelectedChange={this.onTermTreeNodeSelectedChange}
                                            sourceFlag={this.getSourceFlagBySource()}
                                        />
                                    </div>
                                </div>
                            }
                            <div className={showTermTree ? "reco-report-profile-tree-right" : "reco-report-profile-tree-right-nonpadding"}>
                                <div className="reco-report-profile-tree-search-item">
                                    <div className="reco-report-profile-tree-input-title require" tabIndex="0">
                                        {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                                    </div>
                                    <R.Searchbox
                                        width={360}
                                        placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                                        disabled={false}
                                        onSearch={(args) => (args || "").trim() === "" ? this.onStopSearchSourceTree() : this.onSearchSourceTree(args)}
                                    />
                                    <div className="reco-report-profile-tree-search-message">
                                        <R.Messagebar
                                            message={RMResx.RM_JS_RC_TUR_NoScopeSelected}
                                            classify={"error"}
                                            onClose={this.onTriggerSourceTreeError.bind(this, false)}
                                            status={{ show: this.state.showSourceTreeError }}
                                        />
                                    </div>
                                </div>
                                <div className="reco-report-profile-tree">
                                    {this.renderSourceTree()}
                                </div>
                            </div>
                        </section>
                        <section className="reco-report-profile-placeholder"></section>
                        <section className="reco-report-profile-actions">
                            <R.Button
                                id="raRcTurProfileCancelBtn"
                                text={RMResx.RM_JS_Common_Cancel}
                                onClick={this.onCancel} />
                            <R.Button
                                id="raRcTurProfileSaveBtn"
                                primary={true}
                                classify="theme"
                                text={RMResx.RM_JS_Common_Save}
                                onClick={this.onSave} />
                        </section>
                    </div>
                }
            </Fragment>
        );
    }
}
