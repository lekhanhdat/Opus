import { Component, createRef } from "react";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { JobType, SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import { bindEvents, showToast } from "../../../Utilities/CommonUtil";
import StringUtil from "../../../Utilities/StringUtil";
import SPTree from "../../../Components/Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates from "../../../Components/Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../../Components/Common/Tree/Instances/EXO/ReportEXOTree";
import FSTree from "../../../Components/Common/Tree/Instances/FSTree/ReportFSTree";
import ReportBoxTree from "../../Common/Tree/Instances/BoxTree/ReportBoxTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import RouterUrls from "../../../Constants/RouterUrls";
import "../../../Less/RC/commonReportProfile.less";
import { addTelemetryRecord } from '../../../Utilities/TelemetryUtil';
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import CommonExportDestination, { ExportDestinationEnums } from "../CommonExportDestination";
import { NewScheduleSetting } from "../../Common/NewScheduleSetting";
import { SCHEDULE_TYPES } from "../../Common/NewScheduleSetting/constants";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "showMessageTip", "hideMessageTip", "onTriggerSourceTreeError", "onSearchSourceTree", "onStopSearchSourceTree",
            "onTreeChanged", "onNameChange", "onNameBlur", "onDescriptionChange", "onSelectTime", "onSave", "onCancel", "onNodeSelectedChange", "onDestTreeSelectedChanged", "onTriggerDestinationTreeError", "handleScheduleTypeChange"
        );

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;

        let profile = {
            Type: RM.Url.getParam(window.location.href, "type") || JobType.ItemsFilesDueDisposal,
        };
        this.scheduledSettingsRef = createRef();
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.timeInfo = this.defaultTime();
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.spNodeItem = null;
        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            showSourceTreeError: false,
            sourceTreeData: null,
            profile: profile,
            showRequireNameMsg: false,
            showRequireNameTooLongMsg: false,
            showDescriptionTooLongMsg: false,
            timeInfo: this.timeInfo,
            settingsChanged: false,
            exportDestination: ExportDestinationEnums.OpusDownloadCenter,
            destinationTreeData: null,
            scheduleData: {
                scheduleType: SCHEDULE_TYPES.NONE,
            },
            showDestinationTreeError: false,
        };
        window.initTree = () => {
            this.refTermTree.setTreeData(window.treeData.items);
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

    defaultTime() {
        return {
            StartTime: RM.TimeUtil.getCommonDateStr(new Date()),
            // TimeZoneId: RM.TimeSettingModel.TimeZoneId,
            // IsDayLightSaving: false
        };
    }

    initProfileData() {
        let option = {
            url: "/api/DueDisposalApi/LoadProfileById",
            method:"POST",
            data: this.profileId,
        };
        fetchUtility(option).then((data) => {
            const destinationData = data.Extension3 ? $.parseJSON(data.Extension3) : null;
            this.setState({
                profile: data,
                timeInfo: $.parseJSON(data.Extension1),
                sourceTreeData: $.parseJSON(data.Extension2),
                destinationTreeData: destinationData,
                scheduleData: data.scheduleInfo
                    ? {
                          ...data.scheduleInfo,
                          scheduleType: SCHEDULE_TYPES.CONFIGURE
                      }
                    : {},
                exportDestination: data.Extension3 ? ExportDestinationEnums.SelectFromTree : ExportDestinationEnums.OpusDownloadCenter
            });
            this.timeInfo = $.parseJSON(data.Extension1);
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

    onSelectTime(args) {
        this.isChangeTime(args);
        let date = args.newValue;
        //let timeZone = RM.TimeUtil.getGlobalTimezoneInfo();
        let timeInfo = {
            StartTime: RM.TimeUtil.getCommonDateStr(date),
            // IsDayLightSaving: timeZone.autoAdjustClock,
            // TimeZoneId: timeZone.id,
        };
        this.timeInfo = timeInfo;
        this.setState({ timeInfo: this.timeInfo });
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
        let isSelectDestination = this.state.exportDestination === ExportDestinationEnums.SelectFromTree
        let destinationTreeData = isSelectDestination && this.ruleMoveTree.getTreeData();
        const isNoSchedule = this.state.scheduleData?.scheduleType === SCHEDULE_TYPES.NONE
        if (this.state.showRequireNameTooLongMsg || this.state.showDescriptionTooLongMsg || this.state.showRequireNameMsg){
            validSuccess = false;
        }
        else if (profile.ProfileName == null) {
            this.setState({ showRequireNameMsg: true });
            validSuccess = false;
        }
        if (sourceTreeData.selected) {
            newState.showSourceTreeError = false;
        } else {
            validSuccess = false;
            newState.showSourceTreeError = true;
        }
        if (isSelectDestination) {
            if (destinationTreeData && this.spNodeItem) {
                newState.showDestinationTreeError = false;
            } else {
                validSuccess = false;
                newState.showDestinationTreeError = true;
            }
        }
        if (validSuccess) {
            $$.loading(true);
            profile.Id = this.isEdit ? this.state.profile.Id : 0;
            profile.Modified = new Date();
            profile.Extension1 = JSON.stringify(this.timeInfo);
            //sourceTreeData.items.map(node => node.DisposeScheduleInfo = null);
            profile.Extension2 = JSON.stringify(sourceTreeData.items);
            profile.Extension3 = !isNoSchedule && this.state.exportDestination == ExportDestinationEnums.SelectFromTree ? JSON.stringify(this.ruleMoveTree.getTreeData()) : null; 
            profile.FullPath = !isNoSchedule && this.state.exportDestination == ExportDestinationEnums.SelectFromTree ? this.spNodeItem.FullPath : null;
            profile.scheduleInfo = this.getScheduleData() ? this.isEdit
                ? {
                      ...this.getScheduleData(),
                      Id: profile.scheduleInfo?.Id,
                  }
                : this.getScheduleData() : null;
            let option = {
                url: this.isEdit ? "/api/DueDisposalApi/EditProfile" : "/api/DueDisposalApi/CreateProfile",
                data: profile
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                if (res.MessageType === 0) {
                    if (this.isEdit) {
                        RM.CommStatus.save(RM.CommStatus.EditSuccess);
                    } else {
                        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                        addTelemetryRecord(TelemetryModule.ReportCenter, TelemetryEventType.CreateContentDueProfile);
                    }
                    this.setState({ settingsChanged: false });
                    this.routerTo(RouterUrls.RC_DueDisposalReportManagement);
                } else {
                    let tipMsg = this.isEdit ? RMResx.RM_JS_RC_TUR_EditProfileFaild : RMResx.RM_JS_RC_TUR_CreateProfileFaild;
                    showToast.error(StringUtil.stringFormat(tipMsg, res.ErrorMessage));
                }
            }).catch((e) => {
            });
        } else {
            this.setState(newState);
            $$.loading(false);
        }
    }

    onCancel() {
        this.routerTo(RouterUrls.RC_DueDisposalReportManagement);
    }

    onTreeChanged() {
        this.setState({ settingsChanged: true });
    }

    onNodeSelectedChange() {
        this.setState({ showSourceTreeError: false });
    }

    isChangeTime(args) {
        let oldTimeValue = args.oldValue;
        let newTimeValue = args.newValue;
        if (Date.parse(oldTimeValue) != Date.parse(newTimeValue)) {
            this.setState({ settingsChanged: true });
        }
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

    onTriggerDestinationTreeError(show) {
        this.setState({ showDestinationTreeError: show });
    }

    renderSourceTree() {
        if (!this.isEdit || (this.isEdit && this.state.sourceTreeData)) {
            let SourceTree = null;
            let profileType = this.state.profile.Type;
            let sourceTreeFlags = SourceFlags.SP;
            if (profileType == JobType.ItemsFilesDueDisposal) {
                SourceTree = TreeWithTreStates;
            } else if (profileType == JobType.EXOItemsFilesDueDisposalReport) {
                SourceTree = EXOTree;
            } else if (profileType == JobType.PhysicalItemsFilesDueDisposalReport) {
                SourceTree = LocationTree;
            } else if (profileType == JobType.FSItemsFilesDueDisposal) {
                SourceTree = FSTree;
                sourceTreeFlags = SourceFlags.FS;
            } else if (profileType == JobType.OneDriveItemsFilesDueDisposal) {
                SourceTree = TreeWithTreStates;
                sourceTreeFlags = SourceFlags.OneDrive;
            } else if (profileType == JobType.SPOnPremiseItemsFilesDueDisposal) {
                SourceTree = SPTree;
                sourceTreeFlags = SourceFlags.SPLocal;
            } else if (profileType == JobType.BoxItemsFilesDueDisposal) {
                SourceTree = ReportBoxTree;
                sourceTreeFlags = SourceFlags.Box;
            } else if (profileType == JobType.GoogleDriveItemsFilesDueDisposal) {
                SourceTree = ReportGoogleTree;
                sourceTreeFlags = SourceFlags.Google;
            } else if (profileType == JobType.TeamsItemsFilesDueDisposalReport) {
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

    renderSelectTime() {
        let selDate = null;
        let timeInfo = this.state.timeInfo;
        selDate = new Date(timeInfo.StartTime);
        return (
            <div className="ra-section">
                <div className="ra-section-head ra-inline-middle">
                    <span tabIndex='0'>{RMResx.RM_Report_SectionTitle_Settings}</span>
                </div>
                <div className="ra-form-label ra-require">
                    <span tabIndex='0'>{RMResx.RM_RC_DueDisposal_SelectDate.replace(':', "")}</span>
                </div>
                <R.Datepicker
                    selectedDate={selDate}
                    data-part="vtWidget"
                    width={320}
                    dateTimeFormat={this.defaultDateFormat}
                    hasTimePicker={true}
                    onChange={this.onSelectTime}
                />
            </div>

        );
    }

    renderReportDesc() {
        return <div className="introduction">
            <div className="introduction-title">
                <span tabIndex='0'>{RMResx.RM_Report_SectionTitle_Introduction}</span>
            </div>
            <div className="introduction-headline"></div>
            <div className="introduction-content">
                <span tabIndex='0'>{RMResx.RM_RC_DueDisposal_PageDescription}</span>
            </div>
        </div>;
    }

    getScheduleData() {
        return this.scheduledSettingsRef.current?.getScheduleData();
    }

    handleScheduleTypeChange(scheduleType) {
        const isNoSchedule = scheduleType === SCHEDULE_TYPES.NONE

        this.setState((previousState) => ({
            scheduleData: {
                ...previousState.scheduleData,
                scheduleType,
            },
            ...(isNoSchedule
                ? {
                    exportDestination: ExportDestinationEnums.OpusDownloadCenter,
                    destinationTreeData: null,
                    showDestinationTreeError: false,
                } : {}),
        }));

        if (isNoSchedule) {
            this.spNodeItem = null;
            this.ruleMoveTree = null;
        }
    }

    renderScheduleSetting() {
        return <div style={{ paddingLeft: 21 }}>
            <div className="reco-report-profile-tree-input-title" tabIndex="0">
                    ^Scheduled report generation{" "}
                    <$g.Popover>
                        {"^Automatically generate and export reports based on a recurring schedule."}
                    </$g.Popover>
            </div>
            <div className="reco-report-profile-tree-input-title" tabIndex="0">
                ^How often would you like to generate the report?
            </div>
            <NewScheduleSetting
                ref={this.scheduledSettingsRef}
                scheduleData={this.state.scheduleData}
                onScheduleTypeChange={this.handleScheduleTypeChange}
            />

        </div>
    }

    onDestTreeSelectedChanged(nodeItem) {
        this.spNodeItem = nodeItem;
        this.setState({
            showDestinationTreeError: false,
        });
    }

    render() {
        return (
            <div className="reco-report-profile-wrapper">
                <section className="reco-report-profile-header">
                    <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.settingsChanged} />
                    <$g.SiteMap
                        data={[SiteMapLinks.RC_DueDisposalReportManagement, { text: this.isEdit ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
                </section>
                <section className="reco-report-profile-card">
                    <div className="reco-report-profile-form">
                        <div className="reco-report-profile-form-item">
                            <span className="reco-report-profile-input-title-require">
                                {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                            </span>
                            <R.Input
                                id="raRcDdrProfileNameIpt"
                                type="text" value={this.state.profile.ProfileName}
                                onChange={this.onNameChange} onBlur={this.onNameBlur}
                                aria={{ ariaLabel: RMResx.RM_JS_RC_DueDisposal_ProfileName }}
                            />
                            <$g.ValidationMsg show={this.state.showRequireNameMsg}>
                                {RMResx.RM_RC_DueDisposal_NoProfileName}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={this.state.showRequireNameTooLongMsg}>
                                {RMResx.RM_RC_DueDisposal_ProfileNameTooLong}
                            </$g.ValidationMsg>
                        </div>
                        <div className="reco-report-profile-form-item">
                            <span className="reco-report-profile-input-title">
                                {RMResx.RM_JS_RC_DueDisposal_Description}
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
                                {RMResx.RM_RC_DueDisposal_SelectDate.replace(':', "")}
                            </span>
                            <div>
                                <R.Datepicker
                                    id="raRcDdrMeetingRulesTime"
                                    selectedDate={new Date(this.state.timeInfo.StartTime)}
                                    width={360}
                                    data-part="vtWidget"
                                    dateTimeFormat={this.defaultDateFormat}
                                    hasTimePicker={true}
                                    onChange={this.onSelectTime}
                                />
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
                            {RMResx.RM_RC_DueDisposal_PageDescription}
                        </div>
                        <div className="reco-report-profile-tips-pic"></div>
                    </div>
                </section>
                <section className="reco-report-profile-tree-single-card">
                    {this.renderScheduleSetting()}
                    <div className="reco-report-profile-tree-search-item">
                        {this.state.scheduleData.scheduleType === SCHEDULE_TYPES.CONFIGURE && <div className="margin-top-l">
                            <CommonExportDestination
                                value={this.state.exportDestination}
                                onChange={(value) =>
                                    this.setState({
                                        exportDestination: value
                                    })
                                }
                                treeData={this.state.destinationTreeData}
                                treeRef={(r) => (this.ruleMoveTree = r)}
                                onSelectedNodeChanged={
                                    this.onDestTreeSelectedChanged
                                }
                            />
                            <R.Messagebar
                                message={"^Please specify a destination."}
                                classify={"error"}
                                onClose={this.onTriggerDestinationTreeError.bind(this, false)}
                                status={{ show: this.state.exportDestination === ExportDestinationEnums.SelectFromTree && this.state.showDestinationTreeError }}
                            />
                        </div>}
                        <div className="margin-top-l reco-report-profile-tree-input-title require" tabIndex="0">
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
                        <div className="reco-report-profile-tree">
                            {this.renderSourceTree()}
                        </div>
                    </div>
                </section>
                <section className="reco-report-profile-placeholder"></section>
                <section className="reco-report-profile-actions">
                    <R.Button
                        id="raRcDdrProfileCancelBtn"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancel} />
                    <R.Button
                        id="raRcDdrProfileSaveBtn"
                        primary={true}
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSave} />
                </section>
            </div>
        );
    }
}
