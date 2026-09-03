import { Component } from "react";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { JobType, SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import { ActionTypes, RangeTypes } from "../Constants";
import { bindEvents, showToast } from "../../../Utilities/CommonUtil";
import SPTree from "../../../Components/Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates from "../../../Components/Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../../Components/Common/Tree/Instances/EXO/ReportEXOTree";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import FSTree from "../../../Components/Common/Tree/Instances/FSTree/ReportFSTree";
import ReportBoxTree from "../../Common/Tree/Instances/BoxTree/ReportBoxTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import RouterUrls from "../../../Constants/RouterUrls";
import StringUtil from "../../../Utilities/StringUtil";
import "../../../Less/RC/commonReportProfile.less";
import { addTelemetryRecord } from '../../../Utilities/TelemetryUtil';
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "showMessageTip", "hideMessageTip", "onSearchSourceTree", "onStopSearchSourceTree",
            "onNameChange", "onNameBlur", "onDescriptionChange", "handleActionTypeChange", "onDateRangChange", "onSelectTime",
            "onTreeChanged", "onSave", "onCancel", "onNodeSelectedChange"
        );

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;
        let profile = {
            Type: RM.Url.getParam(window.location.href, "type") || JobType.CreateAndDestroyedFileReport,
            IsCreated: true,
            IsDestoryed: true,
            RangeType: 1
        };
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.timeInfo = RM.TimeUtil.getTodayStartEndTime();
        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            showSourceTreeError: false,
            sourceTreeData: null,
            timeInfo: this.timeInfo,
            showDatePicker: false,
            showRequireNameMsg: false,
            profile: profile,
            isCheckActionType: true,
            isRender: false,
            actionTypes: this.getActionTypes(),
            dateRanges: this.getDateRanges(),
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

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    getActionTypes() {
        return [
            {
                text: RMResx.RM_JS_RC_TimeFrame_Create,
                title: RMResx.RM_JS_RC_TimeFrame_Create,
                value: ActionTypes.Create,
                checked: true
            },
            {
                text: RMResx.RM_JS_RC_TimeFrame_Destroyed,
                title: RMResx.RM_JS_RC_TimeFrame_Destroyed,
                value: ActionTypes.Destroyed,
                checked: true
            }
        ];
    }

    getDateRanges() {
        return [
            {
                text: RMResx.RM_RC_Audit_Range_5D,
                title: RMResx.RM_RC_Audit_Range_5D,
                value: RangeTypes["5D"],
                checked: true
            },
            {
                text: RMResx.RM_RC_Audit_Range_1M,
                title: RMResx.RM_RC_Audit_Range_1M,
                value: RangeTypes["1M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_3M,
                title: RMResx.RM_RC_Audit_Range_3M,
                value: RangeTypes["3M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_6M,
                title: RMResx.RM_RC_Audit_Range_6M,
                value: RangeTypes["6M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_Custom,
                title: RMResx.RM_RC_Audit_Range_Custom,
                value: RangeTypes["Custom"],
                checked: false
            }
        ];
    }

    initProfileData() {
        let option = {
            url: "/api/TimeFrameProfileApi/LoadProfileById",
            method:"POST",
            data: this.profileId,
        };
        fetchUtility(option).then((data) => {
            if (!data.Extension1) {
                data.Extension1 = null;
            }
            this.setSelectDateRang(data.RangeType);
            this.setSelectActionType(data.IsCreated, data.IsDestoryed);
            this.setSelectDateRangTime(JSON.parse(data.Extension1));
            this.setState({
                profile: data,
                sourceTreeData: $.parseJSON(data.Extension2),
                isRender: true
            });
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

    onNameBlur(value) {
        // let showRequireNameMsg = false;
        // if ($.trim(value).length == 0) {
        //     showRequireNameMsg = true;
        // }
        // this.setState({ showRequireNameMsg: showRequireNameMsg });
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

    onSelectTime(args) {
        this.timeInfo = args.newValue;
        this.setState({ settingsChanged: true });
    }

    handleActionTypeChange(value) {
        let selValues = value;
        let profile = this.state.profile;
        let isCheckActionType = selValues.length > 0;
        profile.IsCreated = false;
        profile.IsDestoryed = false;
        if (selValues.length > 0) {
            for (let value of selValues) {
                if (value === ActionTypes.Create) {
                    profile.IsCreated = true;
                }
                if (value === ActionTypes.Destroyed) {
                    profile.IsDestoryed = true;
                }
            }
        }
        this.setState({
            profile: profile,
            isCheckActionType: isCheckActionType,
            settingsChanged: true
        });
    }

    onDateRangChange(value) {
        let selValue = value;
        let profile = this.state.profile;
        let showDatePicker = selValue == RangeTypes["Custom"] ? true : false;
        profile.RangeType = selValue;
        this.setState({
            profile: profile,
            showDatePicker: showDatePicker,
            settingsChanged: true
        });
    }

    setSelectDateRang(type) {
        let items = RM.deepcopy(this.state.dateRanges);
        for (let item of items) {
            if (parseInt(item.value) === type) {
                item.checked = true;
            } else {
                item.checked = false;
            }
        }
        let showDatePicker = type == RangeTypes["Custom"] ? true : false;
        this.setState({
            dateRanges: items,
            showDatePicker: showDatePicker
        });
    }

    setSelectActionType(isCreated, isDestoryed) {
        let items = RM.deepcopy(this.state.actionTypes);
        for (let item of items) {
            if (parseInt(item.value) == ActionTypes.Create) {
                item.checked = isCreated;
            }
            if (parseInt(item.value) == ActionTypes.Destroyed) {
                item.checked = isDestoryed;
            }
        }
        this.setState({ actionTypes: items });
    }

    setSelectDateRangTime(item) {
        if (item) {
            this.timeInfo = { start: new Date(item.StartTime), end: new Date(item.EndTime) };
            this.setState({
                timeInfo: this.timeInfo
            });
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
        let extension1 = null;
        let newState = {};
        let sourceTreeData = this.refSourceTree.getTreeData();

        if (this.state.showRequireNameTooLongMsg || this.state.showDescriptionTooLongMsg || this.state.showRequireNameMsg) {
            validSuccess = false;
        }
        else if (profile.ProfileName == null) {
            this.setState({ showRequireNameMsg: true });
            validSuccess = false;
        }

        if (this.state.showDatePicker) {
            extension1 = {
                StartTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.start),
                EndTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.end),
            };
            if (!this.timeInfo) {
                validSuccess = false;
            }
        }
        if (!this.state.isCheckActionType) {
            validSuccess = false;
        }

        if (sourceTreeData.selected) {
            newState.showSourceTreeError = false;
        } else {
            validSuccess = false;
            newState.showSourceTreeError = true;
        }
        if (validSuccess) {
            $$.loading(true);
            profile.Id = this.isEdit ? this.state.profile.Id : 0;
            profile.Modified = new Date();
            profile.Extension1 = JSON.stringify(extension1);
            profile.Extension2 = JSON.stringify(sourceTreeData.items);
            let option = {
                url: this.isEdit ? "/api/TimeFrameProfileApi/EditProfile" : "/api/TimeFrameProfileApi/CreateProfile",
                data: profile
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                if (res == "") {
                    if (this.isEdit) {
                        RM.CommStatus.save(RM.CommStatus.EditSuccess);
                    } else {
                        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                        addTelemetryRecord(TelemetryModule.ReportCenter, TelemetryEventType.CreateCreationAndDestructionProfile);
                    }
                    this.setState({ settingsChanged: false });
                    this.routerTo(RouterUrls.RC_CreationAndDestructionReport);
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
        this.routerTo(RouterUrls.RC_CreationAndDestructionReport);
    }

    onTreeChanged() {
        this.setState({ settingsChanged: true });
    }

    onNodeSelectedChange() {
        this.setState({ showSourceTreeError: false });
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
            let SourceTree = null;
            let profileType = this.state.profile.Type;
            let sourceTreeFlags = SourceFlags.SP;
            if (profileType == JobType.CreateAndDestroyedFileReport) {
                SourceTree = TreeWithTreStates;
            } else if (profileType == JobType.EXOCreateAndDestroyedFileReport) {
                SourceTree = EXOTree;
            } else if (profileType == JobType.PhysicalCreateAndDestroyedFileReport) {
                SourceTree = LocationTree;
            } else if (profileType == JobType.FSCreateAndDestroyedFileReport) {
                SourceTree = FSTree;
                sourceTreeFlags = SourceFlags.FS;
            } else if (profileType == JobType.OneDriveCreateAndDestroyedFileReport) {
                SourceTree = TreeWithTreStates;
                sourceTreeFlags = SourceFlags.OneDrive;
            } else if (profileType == JobType.SPOnPremiseCreateAndDestroyedFileReport) {
                SourceTree = SPTree;
                sourceTreeFlags = SourceFlags.SPLocal;
            } else if (profileType == JobType.BoxCreateAndDestroyedFileReport) {
                SourceTree = ReportBoxTree;
                sourceTreeFlags = SourceFlags.Box;
            } else if (profileType == JobType.GoogleDriveCreateAndDestroyedFileReport) {
                SourceTree = ReportGoogleTree;
                sourceTreeFlags = SourceFlags.Google;
            } else if (profileType == JobType.TeamsCreateAndDestroyedFileReport) {
                SourceTree = ReportTeamsTree;
                sourceTreeFlags = SourceFlags.Teams;
            }
            if (SourceTree) {
                return <SourceTree
                    ref={r => this.refSourceTree = r}
                    searchKey={this.state.sourceSearchKey}
                    data={this.state.sourceTreeData}
                    onTreeChanged={this.onTreeChanged}
                    onNodeSelectedChange={this.onNodeSelectedChange}
                    treeSource={sourceTreeFlags}
                />;
            }
        }
    }

    renderReportingScope() {
        return <div className="ra-section">
            <div className="ra-section-head ra-inline-middle ra-require">
                <span tabIndex='0'>{RMResx.RM_JS_TermUsageReport_ReportingScope}</span>
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
                    message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
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

    renderReportSettings() {
        return <div className="ra-section">
            <div className="ra-section-head ra-inline-middle">
                <span tabIndex='0'>{RMResx.RM_Report_SectionTitle_Settings}</span>
            </div>
            <div>
                {this.renderActionType()}
                {this.renderDateRang()}
            </div>
        </div>;
    }

    renderActionType() {
        let showRequireActionTypeMsg = !this.state.isCheckActionType;
        return <div>
            <div className="ra-form-label ra-require">
                <span tabIndex='0'>{RMResx.RM_JS_RC_TimeFrame_OprationType.replace(':', "")}</span>
            </div>
            <div className="ra-form-content">
                <R.Checkbox.Group
                    name="checkboxgroup-type"
                    items={this.state.actionTypes}
                    onChange={this.handleActionTypeChange}
                />
                <$g.ValidationMsg show={showRequireActionTypeMsg}>
                    {RMResx.RM_JS_RC_TimeFrame_ChooseActionType}
                </$g.ValidationMsg>
            </div>
        </div>;
    }

    renderDateRangDatePiker() {
        if (this.state.showDatePicker) {
            let timeInfo = this.state.timeInfo;
            if (timeInfo && timeInfo.start != null && timeInfo.end != null) {
                return <R.Rangepicker
                    id="raRcCdrCustomTime"
                    selectedDate={timeInfo}
                    data-part="vtWidget"
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    width={320}
                    onChange={this.onSelectTime}
                />;
            }
        }
    }

    renderDateRang() {
        return <div className='margin-top-20'>
            <div className="ra-form-label ra-require">
                <span tabIndex='0'>{RMResx.RM_JS_RC_TimeFrame_Range.replace(':', "")}</span>
            </div>
            <div className="ra-form-content">
                <div className="inline-block vertical-middle">
                    <R.Radio.Group
                        name="radiogroup-type"
                        items={this.state.dateRanges}
                        onChange={this.onDateRangChange}
                    />
                </div>
                <div className="inline-block vertical-middle margin-left-10">
                    {this.renderDateRangDatePiker()}
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
                    tabIndex='0'>{RMResx.RM_JS_RC_TimeFrame_Description}</span>
            </div>
        </div>;
    }

    render() {
        return (
            <div className="reco-report-profile-wrapper">
                <section className="reco-report-profile-header">
                    <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.settingsChanged} />
                    <$g.SiteMap
                        data={[SiteMapLinks.RC_CreationAndDestructionReport, { text: this.isEdit ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
                </section>
                <section className="reco-report-profile-card">
                    <div className="reco-report-profile-form">
                        <div className="reco-report-profile-form-item">
                            <span className="reco-report-profile-input-title-require">
                                {RMResx.RM_JS_TermUsageReport_ProfileName}
                            </span>
                            <R.Input
                                id="raRcCdrProfileNameIpt"
                                type="text" value={this.state.profile.ProfileName}
                                onChange={this.onNameChange} onBlur={this.onNameBlur}
                                aria={{ ariaLabel: RMResx.RM_JS_TermUsageReport_ProfileName }} />
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
                                {RMResx.RM_JS_RC_TimeFrame_OprationType.replace(':', "")}
                            </span>
                            <R.Checkbox.Group
                                block
                                name="checkboxgroup-type"
                                items={this.state.actionTypes}
                                onChange={this.handleActionTypeChange}
                            />
                            <$g.ValidationMsg show={!this.state.isCheckActionType}>
                                {RMResx.RM_JS_RC_TimeFrame_ChooseActionType}
                            </$g.ValidationMsg>
                        </div>
                        <div className="reco-report-profile-form-item">
                            <span className="reco-report-profile-input-title-require">
                                {RMResx.RM_JS_RC_TimeFrame_Range.replace(':', "")}
                            </span>
                            <R.Radio.Group
                                block
                                name="radiogroup-type"
                                items={this.state.dateRanges}
                                onChange={this.onDateRangChange}
                            />
                            <div className="reco-report-profile-datarange-selector">
                                {this.renderDateRangDatePiker()}
                            </div>
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
                            {RMResx.RM_JS_RC_TimeFrame_Description}
                        </div>
                        <div className="reco-report-profile-tips-pic"></div>
                    </div>
                </section>
                <section className="reco-report-profile-tree-single-card">
                    <div className="reco-report-profile-tree-left">
                        <div className="reco-report-profile-tree-search-item">
                            <div className="reco-report-profile-tree-input-title require" tabIndex="0">
                                {RMResx.RM_RC_Common_ElectronicScope.replace(':', "")}
                            </div>
                            <R.Searchbox
                                placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                                disabled={false}
                                width={360}
                                onSearch={(args) => (args || "").trim() === "" ? this.onStopSearchSourceTree() : this.onSearchSourceTree(args)}
                            />
                            <div className="reco-report-profile-tree-search-message">
                                <R.Messagebar
                                    message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
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
                        id="raRcCdrProfileCancelBtn"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancel} />
                    <R.Button
                        id="raRcCdrProfileSaveBtn"
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSave} />
                </section>
            </div>
        );
    }
}
