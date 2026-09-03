import { Component, createRef } from "react";
import { Prompt } from 'react-router';
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { JobType, SourceFlags, TelemetryEventType, TelemetryModule,TreeType } from "../../../Constants/Constants";
import { bindEvents, showToast } from "../../../Utilities/CommonUtil";
import StringUtil from "../../../Utilities/StringUtil";
import SPTree from "../../../Components/Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates from "../../../Components/Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../../Components/Common/Tree/Instances/EXO/ReportEXOTree";
import FSTree from "../../../Components/Common/Tree/Instances/FSTree/ReportFSTree";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import RouterUrls from "../../../Constants/RouterUrls";
import "../../../Less/RC/commonReportProfile.less";
import { addTelemetryRecord } from '../../../Utilities/TelemetryUtil';
import { RangeTypes } from "../Constants";
import { TabIndex } from "../../BCM/ContentRepositoryManagement/CRMForSPO";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import { NewScheduleSetting } from "../../Common/NewScheduleSetting";
import CommonExportDestination, { ExportDestinationEnums } from "../CommonExportDestination";
import { SCHEDULE_TYPES } from "../../Common/NewScheduleSetting/constants";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "showMessageTip", "hideMessageTip", "onTriggerSourceTreeError", "onSearchSourceTree", "onStopSearchSourceTree",
            "onTreeChanged", "onNameChange", "onNameBlur", "onDescriptionChange", "onSelectTime", "onDateRangChange", "onSave", "onCancel", "onNodeSelectedChange", "onDestTreeSelectedChanged", "onTriggerDestinationTreeError", "handleScheduleTypeChange"
        );

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.isEdit = !!this.profileId;

        let profile = {
            Type: RM.Url.getParam(window.location.href, "type") || JobType.RestoreReport,
            RangeType: 1
        };

        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.timeInfo = RM.TimeUtil.getTodayStartEndTime();
        this.NameMaxLength = 250;
        this.DespMaxLength = 250;
        this.scheduledSettingsRef = createRef();
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
            dateRanges: this.getDateRanges(),
            showDatePicker: false,
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

    setSelectDateRangTime(item) {
        if (item) {
            this.timeInfo = { start: new Date(item.StartTime), end: new Date(item.EndTime) };
            this.setState({
                timeInfo: this.timeInfo
            });
        }
    }

    initProfileData() {
        let option = {
            url: "/api/RestoreReportApi/LoadProfileById",
            method:"POST",
            data: this.profileId,
        };
        fetchUtility(option).then((data) => {
            const destinationData = data.Extension3 ? $.parseJSON(data.Extension3) : null;
            if (!data.Extension1) {
                data.Extension1 = null;
            }
            this.setSelectDateRang(data.RangeType);
            this.setSelectDateRangTime(JSON.parse(data.Extension1));
            this.setState({
                profile: data,
                sourceTreeData: $.parseJSON(data.Extension2),
                destinationTreeData: destinationData,
                scheduleData: data.scheduleInfo
                    ? {
                          ...data.scheduleInfo,
                          scheduleType: SCHEDULE_TYPES.CONFIGURE
                      }
                    : {},
                exportDestination: data.Extension3 ? ExportDestinationEnums.SelectFromTree : ExportDestinationEnums.OpusDownloadCenter,
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
        let isSelectDestination = this.state.exportDestination === ExportDestinationEnums.SelectFromTree
        let destinationTreeData = isSelectDestination && this.ruleMoveTree.getTreeData();
        const isNoSchedule = this.state.scheduleData?.scheduleType === SCHEDULE_TYPES.NONE
        if (this.state.showRequireNameTooLongMsg || this.state.showDescriptionTooLongMsg || this.state.showRequireNameMsg) {
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

            if (this.state.showDatePicker) {
                extension1 = {
                    StartTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.start),
                    EndTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.end),
                };
                if (!this.timeInfo) {
                    validSuccess = false;
                }
            }
            profile.Extension1 = JSON.stringify(extension1);
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
                url: this.isEdit ? "/api/RestoreReportApi/EditProfile" : "/api/RestoreReportApi/CreateProfile",
                data: profile
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                if (res == "") {
                    if (this.isEdit) {
                        RM.CommStatus.save(RM.CommStatus.EditSuccess);
                    } else {
                        RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                        addTelemetryRecord(TelemetryModule.ReportCenter, TelemetryEventType.CreateContentDueProfile);
                    }
                    this.setState({ settingsChanged: false });
                    this.routerTo(RouterUrls.RC_RestoreReportManagement);
                } else {
                    let tipMsg = this.isEdit ? RMResx.RM_JS_RC_TUR_EditProfileFaild : RMResx.RM_JS_RC_TUR_CreateProfileFaild;
                    showToast.error(StringUtil.stringFormat(tipMsg, res));
                }
            }).catch((e) => {
            });
        } else {
            this.setState(newState);
            $$.loading(false);
        }
    }

    onCancel() {
        this.routerTo(RouterUrls.RC_RestoreReportManagement);
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
            if (profileType == JobType.RestoreReport) {
                SourceTree = TreeWithTreStates;
            } 
            else if (profileType == JobType.OneDriverRestoreReport) {
                SourceTree = TreeWithTreStates;
                sourceTreeFlags = SourceFlags.OneDrive;
            }
            else if (profileType == JobType.TeamsRestoreReport) {
                SourceTree = ReportTeamsTree;
                sourceTreeFlags = SourceFlags.Teams;
            }
            else if (profileType == JobType.GoogleRestoreReport) {
                SourceTree = ReportGoogleTree;
                sourceTreeFlags = SourceFlags.Google;
            }
            if (SourceTree) {
                return <SourceTree
                    ref={r => this.refSourceTree = r}
                    searchKey={this.state.sourceSearchKey}
                    data={this.state.sourceTreeData}
                    onTreeChanged={this.onTreeChanged}
                    onNodeSelectedChange={this.onNodeSelectedChange}
                    treeSource={sourceTreeFlags}
                    treeType={TreeType.ActionReport}
                    mode={TabIndex.Archive}
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

    onSelectTime(args) {
        this.timeInfo = args.newValue;
        this.setState({ settingsChanged: true });
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
        const isNoSchedule = scheduleType === SCHEDULE_TYPES.NONE; // thay 0 bằng giá trị thực tế

        this.setState((previousState) => ({
            scheduleData: {
                ...previousState.scheduleData,
                scheduleType,
            },
            ...(isNoSchedule
                ? {
                    exportDestination:
                        ExportDestinationEnums.OpusDownloadCenter,
                    destinationTreeData: null,
                    showDestinationTreeError: false,
                }
                : {}),
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
            showDestinationTreeError: false
        });
    }

    render() {
        return (
            <div className="reco-report-profile-wrapper">
                <section className="reco-report-profile-header">
                    <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.settingsChanged} />
                    <$g.SiteMap
                        data={[SiteMapLinks.RC_RestoreReportManagement, { text: this.isEdit ? RMResx.RM_JS_Common_Edit : RMResx.RM_JS_Common_Create }]} />
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
                            {RMResx.RM_RC_Restore_PageDescription}
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
                                        showDestinationTreeError: false,
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
                        <div className="reco-report-profile-tree-input-title require margin-top-l" tabIndex="0">
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
