import { Component, createRef } from "react";
import { Prompt } from "react-router";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import { JobType, SourceFlags } from "../../../../Constants/Constants";
import {
    LicenseHelper,
    showToast,
} from "../../../../Utilities/CommonUtil";
import SPTree from "../../../../Components/Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import RouterUrls from "../../../../Constants/RouterUrls";
import StringUtil from "../../../../Utilities/StringUtil";
import "../../../../Less/RC/commonReportProfile.less";
import ReportTeamsTree from "../../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import ReportGoogleTree from "../../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import { NewScheduleSetting, SCHEDULE_TYPES } from "../../../Common/NewScheduleSetting";
import CommonExportDestination, {
    ExportDestinationEnums,
} from "../../CommonExportDestination";
import { ReportType } from "../config";
import {
    googleObjectLevelItems,
    objectLevelItems,
    TIME_FRAME_TYPES,
} from "./constants";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        this.scheduledSettingsRef = createRef();
        this.exportDestinationTreeRef = createRef();

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;

        const defaultTime = RM.TimeUtil.getCommonDateStr(new Date());
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.state = {
            id: 0,
            profileName: "",
            description: "",
            type: RM.Url.getParam(window.location.href, "type"),
            rangeType: TIME_FRAME_TYPES.ALL,
            objectLevel: ReportType.AllItem,
            extension1: JSON.stringify({
                StartTime: defaultTime,
                EndTime: defaultTime,
            }),
            scheduleId: null,
            scheduleInfo: {
                scheduleType: SCHEDULE_TYPES.NONE,
            },
            showSourceTreeError: false,
            sourceTreeData: null,
            showRequireNameMsg: false,
            settingsChanged: false,
            showRequireNameTooLongMsg: false,
            showDescriptionTooLongMsg: false,
            exportDestinationType: ExportDestinationEnums.OpusDownloadCenter,
            exportDestinationTreeData: [],
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
            state: param,
        });
    }

    getExportLevelOptions() {
        let options;
        switch (Number(this.state.type)) {
            case JobType.ArchivedSiteReportTerm:
                options = LicenseHelper.HasUpgradeTeams()
                    ? objectLevelItems
                    : googleObjectLevelItems;
                break;
            case JobType.ArchivedSiteReportGoogle:
                options = googleObjectLevelItems;
                break;
            default:
                options = LicenseHelper.HasOpusGoogleLicenseOnly()
                    ? googleObjectLevelItems
                    : objectLevelItems;
                break;
        }

        return options.map((item) => ({
            ...item,
            checked: item.value == this.state.objectLevel,
        }));
    }

    getDefineATimeFrame() {
        return [
            {
                text: "^^Export all",
                value: TIME_FRAME_TYPES.ALL,
                checked: this.state.rangeType == TIME_FRAME_TYPES.ALL,
            },
            {
                text: "^^Custom",
                value: TIME_FRAME_TYPES.CUSTOM,
                checked: this.state.rangeType == TIME_FRAME_TYPES.CUSTOM,
            },
        ];
    }

    initProfileData() {
        let option = {
            url: "/api/Dashboard/LoadProfileById",
            method: "POST",
            data: this.profileId,
        };
        fetchUtility(option)
            .then((data) => {
                const destinationData = data.Extension3
                    ? $.parseJSON(data.Extension3)
                    : null;
                data.scheduleInfo = {
                    ...data.scheduleInfo,
                    scheduleType:
                        data.scheduleInfo?.NoSchedule === false
                            ? SCHEDULE_TYPES.CONFIGURE
                            : SCHEDULE_TYPES.NONE,
                };
                this.setState({
                    id: data.Id,
                    profileName: data.ProfileName || "",
                    description: data.Description || "",
                    type: data.Type,
                    rangeType: data.RangeType,
                    objectLevel: data.ObjectLevel,
                    extension1: data.Extension1,
                    scheduleId: data.ScheduleId,
                    scheduleInfo: data.scheduleInfo,
                    sourceTreeData: $.parseJSON(data.Extension2),
                    exportDestinationType: data.Extension3
                        ? ExportDestinationEnums.SelectFromTree
                        : ExportDestinationEnums.OpusDownloadCenter,
                    exportDestinationTreeData: destinationData,
                });
            });
    }

    onNameChange = (value) => {
        const isTooLong = value.length > this.NameMaxLength;
        this.setState({
            showRequireNameMsg: value.length == 0,
            showRequireNameTooLongMsg: isTooLong,
            ...(!isTooLong
                ? { profileName: value.trim(), settingsChanged: true }
                : {}),
        });
    };

    onDescriptionChange = (value) => {
        const isTooLong = value.length > this.DespMaxLength;
        this.setState({
            showDescriptionTooLongMsg: isTooLong,
            ...(!isTooLong
                ? { description: value, settingsChanged: true }
                : {}),
        });
    };

    onSearchSourceTree = (args) => {
        this.setState({ sourceSearchKey: args });
    };

    onStopSearchSourceTree = () => {
        this.setState({ sourceSearchKey: "" });
    };

    onSave = () => {
        let validSuccess = true;
        let newState = {};
        const sourceTreeData = this.refSourceTree.getTreeData();

        if (
            this.state.showRequireNameTooLongMsg ||
            this.state.showDescriptionTooLongMsg ||
            this.state.showRequireNameMsg
        ) {
            validSuccess = false;
        } else if (!this.state.profileName) {
            this.setState({ showRequireNameMsg: true });
            validSuccess = false;
        }

        if (sourceTreeData.selected) {
            newState.showSourceTreeError = false;
        } else {
            validSuccess = false;
            newState.showSourceTreeError = true;
        }
        if (validSuccess) {
            const scheduleInfo = this.getScheduleData();
            const payload = {
                Id: this.isEdit ? this.state.id : 0,
                ProfileName: this.state.profileName,
                Description: this.state.description,
                Type: this.state.type,
                ObjectLevel: this.state.objectLevel,
                RangeType: this.state.rangeType,
                Extension1:
                    this.state.rangeType === TIME_FRAME_TYPES.ALL
                        ? "{}"
                        : this.state.extension1,
                Extension2: JSON.stringify(sourceTreeData.items),
                Extension3:
                    !this.isScheduleTypeNone() &&
                    this.state.exportDestinationType ===
                    ExportDestinationEnums.SelectFromTree
                        ? JSON.stringify(
                              this.exportDestinationTreeRef.current?.getTreeData() ||
                                  [],
                          )
                        : null,
                ScheduleId: this.state.scheduleId,
                scheduleInfo: scheduleInfo || {},
            };
            let option = {
                url: this.isEdit
                    ? "/api/Dashboard/EditProfile"
                    : "/api/Dashboard/CreateProfile",
                data: payload,
            };
            fetchUtility(option)
                .then((res) => {
                    if (res.ErrorMessage == null) {
                        if (this.isEdit) {
                            RM.CommStatus.save(RM.CommStatus.EditSuccess);
                        } else {
                            RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                        }
                        this.setState({ settingsChanged: false });
                        this.routerTo(
                            RouterUrls.RC_StorageOptimizationReportManagement,
                        );
                    } else {
                        let tipMsg = this.isEdit
                            ? RMResx.RM_JS_RC_TUR_EditProfileFaild
                            : RMResx.RM_JS_RC_TUR_CreateProfileFaild;
                        showToast.error(StringUtil.stringFormat(tipMsg, res));
                    }
                });
        } else {
            this.setState(newState);
        }
    };

    onCancel = () => {
        this.routerTo(RouterUrls.RC_StorageOptimizationReportManagement);
    };

    onTreeChanged = () => {
        this.setState({ settingsChanged: true });
    };

    onTriggerSourceTreeError = (show) => {
        this.setState({ showSourceTreeError: show });
    };

    handleScheduleTypeChange = (scheduleType) => {
        const isNoSchedule = scheduleType === SCHEDULE_TYPES.NONE;
        this.setState((previousState) => ({
            scheduleInfo: {
                ...previousState.scheduleInfo,
                scheduleType,
            },
            settingsChanged: true,
            ...(isNoSchedule
                ? {
                      exportDestinationType: ExportDestinationEnums.OpusDownloadCenter,
                      exportDestinationTreeData: [],
                  }
                : {}),
        }));
    };

    getScheduleData() {
        return this.scheduledSettingsRef.current?.getScheduleData();
    }

    handleExportDestinationTypeChange = (exportDestinationType) => {
        this.setState({
            exportDestinationType,
            settingsChanged: true,
        });
    };

    handleExportLevelChange = (args) => {
        this.setState({
            objectLevel: args.newValue.value,
            settingsChanged: true,
        });
    };

    handleTimeFrameTypeChange = (timeFrameType) => {
        this.setState((previousState) => {
            const defaultTime = RM.TimeUtil.getCommonDateStr(new Date());
            let extension1 = previousState.extension1;
            if (timeFrameType === TIME_FRAME_TYPES.CUSTOM && !extension1) {
                extension1 = JSON.stringify({
                    StartTime: defaultTime,
                    EndTime: defaultTime,
                });
            }
            return {
                rangeType: timeFrameType,
                extension1,
                settingsChanged: true,
            };
        });
    };

    handleTimeFrameDateChange = (args) => {
        if (!args.newValue) {
            return;
        }

        this.setState({
            extension1: JSON.stringify({
                StartTime: RM.TimeUtil.getCommonDateStr(args.newValue.start),
                EndTime: RM.TimeUtil.getCommonDateStr(args.newValue.end),
            }),
            settingsChanged: true,
        });
    };

    isScheduleTypeNone() {
        return this.state.scheduleInfo.scheduleType === SCHEDULE_TYPES.NONE;
    }

    renderDateRangePicker() {
        const timeFrame = JSON.parse(this.state.extension1 || "{}");

        const start = timeFrame.StartTime
            ? new Date(timeFrame.StartTime)
            : new Date();
        const end = timeFrame.EndTime ? new Date(timeFrame.EndTime) : start;
        if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
            return null;
        }

        return (
            <R.Rangepicker
                id="so-report-time-frame-range"
                selectedDate={{ start, end }}
                data-part="vtWidget"
                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                width={320}
                onChange={this.handleTimeFrameDateChange}
            />
        );
    }

    renderHeaderSection() {
        return (
            <section className="reco-report-profile-header">
                <Prompt
                    message={RMResx.RM_JS_RC_TUR_CancelMessage}
                    when={this.state.settingsChanged}
                />
                <$g.SiteMap
                    data={[
                        SiteMapLinks.RC_StorageOptimizationReportManagement,
                        {
                            text: this.isEdit
                                ? RMResx.RM_JS_Common_Edit
                                : RMResx.RM_JS_Common_Create,
                        },
                    ]}
                />
            </section>
        );
    }

    renderSourceTree() {
        if (!this.isEdit || (this.isEdit && this.state.sourceTreeData)) {
            let SourceTree = null;
            let profileType = this.state.type;
            let sourceTreeFlags = SourceFlags.SP;
            if (profileType == JobType.ArchivedSiteReportSharePointOnline) {
                SourceTree = SPTree;
            } else if (profileType == JobType.ArchivedSiteReportSOneDrive) {
                SourceTree = SPTree;
                sourceTreeFlags = SourceFlags.OneDrive;
            } else if (profileType == JobType.ArchivedSiteReportGoogle) {
                SourceTree = ReportGoogleTree;
                sourceTreeFlags = SourceFlags.Google;
            } else if (profileType == JobType.ArchivedSiteReportTerm) {
                SourceTree = ReportTeamsTree;
                sourceTreeFlags = SourceFlags.Teams;
            }
            if (SourceTree) {
                return (
                    <SourceTree
                        ref={(r) => (this.refSourceTree = r)}
                        searchKey={this.state.sourceSearchKey}
                        data={this.state.sourceTreeData}
                        onTreeChanged={this.onTreeChanged}
                        treeSource={sourceTreeFlags}
                    />
                );
            }
        }
    }

    renderSchedulerSetting() {
        return (
            <div>
                <div className="reco-report-profile-tree-input-title">
                    {"^^How often would you like to generate the report?"}
                </div>
                <NewScheduleSetting
                    ref={this.scheduledSettingsRef}
                    scheduleData={this.state.scheduleInfo}
                    onScheduleTypeChange={this.handleScheduleTypeChange}
                />
            </div>
        );
    }

    renderSpecifyAnExportDestination() {
        if (this.isScheduleTypeNone()) {
            return null;
        }
        const showDestinationTree = this.state.exportDestinationType === ExportDestinationEnums.SelectFromTree;

        return (
            <div className={showDestinationTree ? "reco-report-profile-tree" : ""}>
                <CommonExportDestination
                    value={this.state.exportDestinationType}
                    onChange={this.handleExportDestinationTypeChange}
                    treeData={this.state.exportDestinationTreeData}
                    treeRef={this.exportDestinationTreeRef}
                />
            </div>
        );
    }

    renderSpecifyAnExportLevels() {
        return (
            <div>
                <div
                    className="reco-report-profile-tree-input-title require"
                    style={{ marginBottom: 4 }}
                >
                    {
                        "^^Specify the object level that you want to include in the reports"
                    }
                </div>
                <R.Combobox
                    id="so-report-export-level"
                    width={360}
                    searchable={false}
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    items={this.getExportLevelOptions()}
                    onChange={this.handleExportLevelChange}
                />
            </div>
        );
    }

    renderDefineATimeFrame() {
        return (
            <div className="reco-report-profile-form-item reco-report-profile-time-frame">
                <span className="reco-report-profile-input-title-require">
                    {RMResx.RM_JS_RC_TimeFrame_Range}
                </span>
                <R.Radio.Group
                    name="so-report-time-frame"
                    items={this.getDefineATimeFrame()}
                    value={this.state.rangeType}
                    onChange={this.handleTimeFrameTypeChange}
                    block={true}
                />
                {this.state.rangeType === TIME_FRAME_TYPES.CUSTOM && (
                    <div className="reco-report-profile-time-frame-picker">
                        {this.renderDateRangePicker()}
                    </div>
                )}
            </div>
        );
    }

    renderProfileSection() {
        return (
            <section className="reco-report-profile-card">
                <div className="reco-report-profile-form">
                    <div className="reco-report-profile-form-item">
                        <span className="reco-report-profile-input-title-require">
                            {RMResx.RM_JS_RC_DueDisposal_ProfileName}
                        </span>
                        <R.Input
                            id="raRcAsrProfileNameIpt"
                            type="text"
                            value={this.state.profileName}
                            onChange={this.onNameChange}
                            aria={{
                                ariaLabel:
                                    RMResx.RM_JS_RC_DueDisposal_ProfileName,
                            }}
                        />
                        <$g.ValidationMsg show={this.state.showRequireNameMsg}>
                            {RMResx.RM_RC_DueDisposal_NoProfileName}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg
                            show={this.state.showRequireNameTooLongMsg}
                        >
                            {RMResx.RM_RC_DueDisposal_ProfileNameTooLong}
                        </$g.ValidationMsg>
                    </div>
                    <div className="reco-report-profile-form-item">
                        <span className="reco-report-profile-input-title">
                            {RMResx.RM_RC_Profile_Description}
                        </span>
                        <R.Input
                            type="textarea"
                            value={this.state.description}
                            onChange={this.onDescriptionChange}
                            aria={{
                                ariaLabel: RMResx.RM_JS_Profile_Description,
                            }}
                        />
                        <span className="reco-report-profile-input-desc">
                            {RMResx.RM_RC_Profile_Description_Tips}
                        </span>
                        <$g.ValidationMsg
                            show={this.state.showDescriptionTooLongMsg}
                        >
                            {RMResx.RM_RC_DueDisposal_DescriptionTooLong}
                        </$g.ValidationMsg>
                    </div>
                    {this.renderDefineATimeFrame()}
                </div>
                <div className="reco-report-profile-tips">
                    <div className="reco-report-profile-tips-header">
                        <span className="reco-report-profile-tips-icon fia-light"></span>
                        <span
                            className="reco-report-profile-tips-header-title"
                            tabIndex="0"
                        >
                            {RMResx.RM_Report_SectionTitle_Introduction}
                        </span>
                    </div>
                    <div
                        className="reco-report-profile-tips-content"
                        tabIndex="0"
                    >
                        {
                            "^^This report is used to display the data that has been archived in the reporting scope within a specific time range.   "
                        }
                    </div>
                    <div className="reco-report-profile-tips-pic"></div>
                </div>
            </section>
        );
    }

    renderReportSettingsSection() {
        return (
            <section className="reco-report-profile-tree-wrapper">
                <div className="strong">
                    {"^^Scheduled report generation"}
                    <$g.Popover>
                        {"^^Scheduled report generation details"}
                    </$g.Popover>
                </div>
                {this.renderSchedulerSetting()}
                {this.renderSpecifyAnExportDestination()}
                {this.renderSpecifyAnExportLevels()}
                <div>
                    <div
                        className="reco-report-profile-tree-input-title require"
                        tabIndex="0"
                    >
                        {RMResx.RM_RC_Common_ElectronicScope.replace(":", "")}
                    </div>
                    <R.Searchbox
                        placeholder={RMResx.RM_PRM_PRE_SearchPlaceholder}
                        disabled={false}
                        width={360}
                        onSearch={(args) =>
                            (args || "").trim() === ""
                                ? this.onStopSearchSourceTree()
                                : this.onSearchSourceTree(args)
                        }
                    />
                    <div className="reco-report-profile-tree-search-message">
                        <R.Messagebar
                            message={RMResx.RM_JS_RC_DueDisposal_NoSelectedTree}
                            classify={"error"}
                            onClose={this.onTriggerSourceTreeError.bind(
                                this,
                                false,
                            )}
                            status={{
                                show: this.state.showSourceTreeError,
                            }}
                        />
                    </div>
                    <div className="reco-report-profile-tree">
                        {this.renderSourceTree()}
                    </div>
                </div>
            </section>
        );
    }

    renderActionsSection() {
        return (
            <section className="reco-report-profile-actions">
                <R.Button
                    id="raRcAsrCancelBtn"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCancel}
                />
                <R.Button
                    id="raRcAsrSaveBtn"
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSave}
                />
            </section>
        );
    }

    render() {
        return (
            <div className="reco-report-profile-wrapper">
                {this.renderHeaderSection()}
                {this.renderProfileSection()}
                {this.renderReportSettingsSection()}
                {this.renderActionsSection()}
            </div>
        );
    }
}
