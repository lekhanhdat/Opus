import { Component } from "react";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { JobType } from "../../../Constants/Constants";
import { bindEvents, showToast } from "../../../Utilities/CommonUtil";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/SingleModeLocationTree";
import RouterUrls from "../../../Constants/RouterUrls";
import StringUtil from "../../../Utilities/StringUtil";
import "../../../Less/RC/commonReportProfile.less";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "showMessageTip", "hideMessageTip", "onSearchSourceTree", "onStopSearchSourceTree", "onTreeChanged",
            "onNameChange", "onNameBlur", "onDescriptionChange", "onTriggerSourceTreeError", "onSave", "onCancel"
        );

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;

        let profile = {
            Type: RM.Url.getParam(window.location.href, "type") || JobType.AvailableSpaceReport,
        };
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            showSourceTreeError: false,
            sourceTreeData: null,
            selectedLocationId: null,
            showRequireNameMsg: false,
            profile: profile,
            settingsChanged: false,
            showRequireNameTooLongMsg: false,
            showDescriptionTooLongMsg: false,
        };
    }

    componentDidMount() {
        if (this.isEdit) {
            this.initProfileData();
        }
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    initProfileData() {
        let option = {
            url: "/api/AvailableSpaceReportApi/LoadProfileById",
            method:"POST",
            data: this.profileId,
        };
        fetchUtility(option).then((data) => {
            this.setState({
                profile: data,
                sourceTreeData: $.parseJSON(data.Extension1),
                selectedLocationId: data.Extension2
            });
            this.initDate = RM.deepcopy(data);
        }).catch((e) => {
            
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
        // setTimeout(()=>{
        //     let showRequireNameMsg = false;
        //     if ($.trim(args.value).length == 0) {
        //         showRequireNameMsg = true;
        //     }
        //     this.setState({showRequireNameMsg: showRequireNameMsg});
        // },100);
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


    onSearchSourceTree(args) {
        this.setState({ sourceSearchKey: args });
    }

    onStopSearchSourceTree() {
        this.setState({ sourceSearchKey: "" });
    }

    onSave() {
        let profile = this.state.profile;
        let validSuccess = true;
        let newState = {};
        let sourceTreeData = this.refSourceTree.getTreeData();

        if (this.state.showRequireNameTooLongMsg || this.state.showDescriptionTooLongMsg || this.state.showRequireNameMsg) {
            validSuccess = false;
        }
        else if (profile.ProfileName == null) {
            this.setState({ showRequireNameMsg: true });
            validSuccess = false;
        }

        if (sourceTreeData.selectedItemId) {
            newState.showSourceTreeError = false;
        } else {
            validSuccess = false;
            newState.showSourceTreeError = true;
        }
        if (validSuccess) {
            profile.Id = this.isEdit ? this.state.profile.Id : 0;
            profile.Modified = new Date();
            profile.Extension1 = JSON.stringify(sourceTreeData.items);
            profile.Extension2 = sourceTreeData.selectedItemId + "";
            let option = {
                url: this.isEdit ? "/api/AvailableSpaceReportApi/EditProfile" : "/api/AvailableSpaceReportApi/CreateProfile",
                data: profile
            };
            fetchUtility(option).then((res) => {
                if (res == "") {
                    if (this.isEdit) {
                        RM.CommStatus.save(RM.CommStatus.EditSuccess);
                    } else {
                        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                    }
                    this.setState({ settingsChanged: false });
                    this.routerTo(RouterUrls.RC_AvailableSpaceReportManagement);
                } else {
                    let tipMsg = this.isEdit ? RMResx.RM_JS_RC_TUR_EditProfileFaild : RMResx.RM_JS_RC_TUR_CreateProfileFaild;
                    showToast.error(StringUtil.stringFormat(tipMsg, res));
                }
            }).catch((e) => {
            });
        } else {
            this.setState(newState);
        }
    }

    onCancel() {
        this.routerTo(RouterUrls.RC_AvailableSpaceReportManagement);
    }

    onTreeChanged() {
        this.setState({ settingsChanged: true });
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

    renderSourceTree() {
        if (!this.isEdit || (this.isEdit && this.state.sourceTreeData)) {
            return <LocationTree
                ref={r => this.refSourceTree = r}
                selectedItemId={this.state.selectedLocationId}
                searchKey={this.state.sourceSearchKey}
                data={this.state.sourceTreeData}
                onTreeChanged={this.onTreeChanged}
            />;
        }
    }

    renderReportingScope() {
        return <div className="ra-section">
            <div className="ra-section-head ra-inline-middle">
                <span tabIndex='0'>{RMResx.RM_JS_TermUsageReport_ReportingScope}</span>
            </div>
            <div className="ra-form-label ra-require">
                <span tabIndex='0'>{RMResx.RM_JS_RC_AvailableSpace_ChooseLocation.replace(':', "")}</span>
            </div>
            <div className="ra-form-content ">
                <div className="tree-searchbox">
                    <R.Searchbox
                        width={320}
                        placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                        disabled={false}
                        onSearch={(args) => (args || "").trim() === "" ? this.onStopSearchSourceTree() : this.onSearchSourceTree(args)}
                    />
                </div>
                <R.Messagebar
                    message={RMResx.RM_JS_RC_AvailableSpace_SelectLocationTip}
                    classify={"error"}
                    onClose={this.onTriggerSourceTreeError.bind(this, false)}
                    status={{ show: this.state.showSourceTreeError }}
                />
                <div className="tree-container">
                    {this.renderSourceTree()}
                </div>
            </div>
        </div>;
    }

    renderReportDesc() {
        return <div className="introduction">
            <div className="introduction-title">
                <span tabIndex='0'>{RMResx.RM_Report_SectionTitle_Introduction}</span>
            </div>
            <div className="introduction-headline"></div>
            <div className="introduction-content">
                <span
                    tabIndex='0'>{RMResx.RM_RC_AvailableSpace_Desc}</span>
            </div>
        </div>;
    }

    render() {
        return (
            <div className="reco-report-profile-wrapper">
                <section className="reco-report-profile-header">
                    <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.settingsChanged} />
                    <$g.SiteMap
                        data={[SiteMapLinks.RC_AvailableSpaceReport, { text: this.isEdit ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
                </section>
                <section className="reco-report-profile-card">
                    <div className="reco-report-profile-form">
                        <div className="reco-report-profile-form-item">
                            <span className="reco-report-profile-input-title-require">
                                {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                            </span>
                            <R.Input
                                id="raRcAsrProfileNameIpt" type="text" value={this.state.profile.ProfileName}
                                onChange={this.onNameChange} onBlur={this.onNameBlur}
                                aria={{ ariaLabel: RMResx.RM_JS_RC_DueDisposal_ProfileName }} />
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
                            {RMResx.RM_RC_AvailableSpace_Desc}
                        </div>
                        <div className="reco-report-profile-tips-pic"></div>
                    </div>
                </section>
                <section className="reco-report-profile-tree-single-card">
                    <div className="reco-report-profile-tree-search-item">
                        <div className="reco-report-profile-tree-input-title require" tabIndex="0">
                            {RMResx.RM_RC_AvailableSpace_ElectronicScope}
                        </div>
                        <R.Searchbox
                            placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                            disabled={false}
                            width={360}
                            onSearch={(args) => (args || "").trim() === "" ? this.onStopSearchSourceTree() : this.onSearchSourceTree(args)}
                        />
                        <div className="reco-report-profile-tree-search-message">
                            <R.Messagebar
                                message={RMResx.RM_JS_RC_AvailableSpace_SelectLocationTip}
                                classify={"error"}
                                onClose={this.onTriggerSourceTreeError.bind(this, false)}
                                status={{ show: this.state.showSourceTreeError }}
                            />
                        </div>
                    </div>
                    <div className="reco-report-profile-tree">
                        {this.renderSourceTree()}
                    </div>
                </section>
                <section className="reco-report-profile-placeholder"></section>
                <section className="reco-report-profile-actions">
                    <R.Button
                        id="raRcAsrCancelBtn"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancel} />
                    <R.Button
                        id="raRcAsrSaveBtn"
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSave} />
                </section>
            </div>
        );
    }
}
