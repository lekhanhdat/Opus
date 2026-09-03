import { Component, Fragment } from "react";
import RouterUrls from "../../Constants/RouterUrls";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import { bindEvents, EnvironmentHelper, LicenseHelper } from "../../Utilities/CommonUtil";
import { RoleType } from '../../Constants/Constants';
import "../../Less/CP/TimerJobSettings.less";
import { checkPermission } from "../../Utilities/permissionManager";

const RunApplySettingMethod = {
    UpdatedScope: 1,
    AllScope: 2,
    Auto: 3 //A由后台自行判断是否跑sharepoint setting full job 
};

export default class TimerJobSettings extends Component {
    constructor(props) {
        super(props);
        bindEvents(this, "onCancel", "onRunNowClick", "showMessageTip", "onClickRun", "showMsgToast");
        this.state = {
            tipStatus: { show: false },
            tipType: "",
            tipMsg: "",
            enableDeduplication: false
        };
        this.checkIsEnableDedup();
    }

    checkIsEnableDedup() {
        $.ajax({
            type: 'GET',
            contentType: 'application/json;charset=utf-8',
            url: '/api/RetentionApi/IsEnableDeduplication',
            success: (result) => {
                if (result) {
                    this.setState({ enableDeduplication: true });
                }
            },
            error: (msg) => {
            }
        });
    }

    // showMessageTip (type, msg) {
    //     let tipOption = {
    //         tipStatus: { show: true },
    //         tipType: type,
    //         tipMsg: msg
    //     };
    //     this.setState(tipOption);
    // }
    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }
    onClickRun(funcJobComplate, message) {
        if (funcJobComplate) {
            this.showMsgToast(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>, "success", true);
        } else {
            this.showMsgToast(message, "error", true);
        }
    }

    onCancel() {
        this.props.history.push({
            pathname: RouterUrls.CP_Index
        });
    }

    render() {
        let isAdmin = RM.RoleType == RoleType.SupAdmin || RM.RoleType == RoleType.DelegateAdmin;
        let isSPOnPremAdmin = checkPermission("CP_Schedule_Settings_On_Prem", RM.UserResources);
        let isRecoLicense = checkPermission("Reco_CP_Schedule_Settings", RM.UserResources);
        let isArchiveLicense = checkPermission("Archiver_CP_Schedule_Settings", RM.UserResources);
        let isOpusILLicense = LicenseHelper.HasOpusILLicense();
        let enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
        let scheduleLists = [];
        if (isRecoLicense) {
            scheduleLists.push(<ScheduleSettingSection
                type={1}
                onClickRun={this.onClickRun}
            />);
            if(isOpusILLicense){
                if (checkPermission("Source_Teams", RM.UserResources) && LicenseHelper.HasUpgradeTeams()) {
                    scheduleLists.push(<ScheduleSettingSection
                        type={73}
                        onClickRun={this.onClickRun}
                    />);
                }
                scheduleLists.push(<ScheduleSettingSection
                    type={2}
                    onClickRun={this.onClickRun}
                />);
                scheduleLists.push(<ScheduleSettingSection
                    type={17}
                    onClickRun={this.onClickRun}
                />);
            }
            if (isSPOnPremAdmin && !EnvironmentHelper.IsGCPEnvironment) {
                scheduleLists.push(<ScheduleSettingSection
                    type={26}
                    onClickRun={this.onClickRun}
                />);
            }
            if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_Google", RM.UserResources)) {
                scheduleLists.push(<ScheduleSettingSection
                    type={61}
                    onClickRun={this.onClickRun}
                />)
            }
            if(isOpusILLicense)
            {
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_Teams", RM.UserResources) && LicenseHelper.HasUpgradeTeams()) {
                    scheduleLists.push(<ScheduleSettingSection
                        type={72}
                        onClickRun={this.onClickRun}
                    />);
                }
                scheduleLists.push(<ScheduleSettingSection
                    type={15}
                    onClickRun={this.onClickRun}
                />);
                scheduleLists.push(<ScheduleSettingSection
                    type={16}
                    onClickRun={this.onClickRun}
                />);
            }
            if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_FS", RM.UserResources) && !EnvironmentHelper.IsGCPEnvironment) {
                scheduleLists.push(<ScheduleSettingSection
                    type={21}
                    onClickRun={this.onClickRun}
                />);
            }
            if (isSPOnPremAdmin && !EnvironmentHelper.IsGCPEnvironment) {
                scheduleLists.push(<ScheduleSettingSection
                    type={27}
                    onClickRun={this.onClickRun}
                />);
            }
            if(isOpusILLicense)
            {
                scheduleLists.push(<ScheduleSettingSection
                    type={29}
                    onClickRun={this.onClickRun}
                />);
            }
            if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_AzureFile", RM.UserResources)) {
                scheduleLists.push(<ScheduleSettingSection
                    type={34}
                    onClickRun={this.onClickRun}
                />);
            }
            if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_Box", RM.UserResources)) {
                scheduleLists.push(<ScheduleSettingSection
                    type={45}
                    onClickRun={this.onClickRun}
                />)
            };
            if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && checkPermission("Source_Google", RM.UserResources)) {
                scheduleLists.push(<ScheduleSettingSection
                    type={60}
                    onClickRun={this.onClickRun}
                />);
            }
            if (isSPOnPremAdmin && !EnvironmentHelper.IsGCPEnvironment) {
                scheduleLists.push(<ScheduleSettingSection
                    type={24}
                    onClickRun={this.onClickRun}
                />);
            }
        }
        if (enableRecordsArchiver && (isArchiveLicense || checkPermission("Source_FS", RM.UserResources))) {
            scheduleLists.push(<ScheduleSettingSection
                type={39}
                onClickRun={this.onClickRun}
            />);
        }

        if (isArchiveLicense && RM.gData.enableDeleteRestoredDataFeature && checkPermission(RouterUrls.CP_Index, RM.UserResources)) {
            scheduleLists.push(<ScheduleSettingSection
                type={48}
                onClickRun={this.onClickRun}
            />);
        }
        
        if (this.state.enableDeduplication && isAdmin && (isArchiveLicense || isRecoLicense)) {
            scheduleLists.push(<ScheduleSettingSection
                type={50}
                onClickRun={this.onClickRun}
            />); 
        }
        
        if (enableRecordsArchiver) {
            // StubDisposalSchedule = 80
            scheduleLists.push(<ScheduleSettingSection
                type={80}
                onClickRun={this.onClickRun}
            />);
        }

        return <div id="rmTimeJobSettings">
            <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_TimerJobSettings]} />
            <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{ show: this.state.tipStatus.show }} />

            <div className="ra-page-main container-fluid">
                <div className="row-flex row-xlg-flex">
                    {scheduleLists.map(section => {
                        return section;
                    })}
                </div>
            </div>
        </div>;
    }
}


class ScheduleSettingSection extends R.Component {
    idAttr = true;
    componentCreate() {
        this.bind("onSaveBtnClick", "onCancelBtnClick",
            "onEditSettingsClick", "setData", "setTimeValue",
            "onRunNowClick", "getPropsType", "onCreateNewRuleDialogClose");
        this.settingPanelId = "schedulePanel_" + this.props.type;
        this.configWhitelistPanelId = "configWhitelistPanel_" + this.props.type;
        this.settings = { NoSchedule: true };
        this.state = {
            startTime: '',
            endTime: '',
            intervalValue: '',
            showSettingsPanel: { show: false },
            showConfigPanel: { show: false },
            timerJobDesc: '',
            jobDesc: '',
            ssContent: '',
            ssContainsHead: '',
        };
    }

    componentInit() {
        this.loadScheduleData();
    }

    loadScheduleData() {
        $.ajax({
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json;charset=utf-8',
            url: '/api/BCMAdminSettingApi/GetScheduleByType',
            data: JSON.stringify(this.props.type),
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (result) => {
                var data = result;
                if (data.length == 0) {
                    this.settings = { NoSchedule: true };
                } else {
                    this.settings = data[0];
                }
                this.initDetails();
            },
            error: (msg) => {
            }
        });
    }

    initDetails() {
        if (this.settings.NoSchedule) {
            this.setState({
                startTime: '',
                endTime: '',
                intervalValue: '',
            });
        } else {
            let timeZone = RM.TimeUtil.getTimezoneInfo(this.settings.TimeZoneId, this.settings.IsDaylightSaving);
            this.setState({
                startTime: RM.TimeUtil.dateToString(new Date(this.settings.StartTime), timeZone, true),
                intervalValue: this.settings.Interval + ' ' + this.getIntervalUnit(),
                endTime: this.getEndTimeDisplayStr(timeZone)
            });
        }
    }

    getResources() {
        switch (this.props.type) {
            case 1:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_TermSyn,
                    timerJobDesc: RMResx.RM_JS_TimerJob_TermSynDescription,
                    ssContent: RMResx.RM_BCM_TermSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 2:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_SPSetting,
                    timerJobDesc: RMResx.RM_JS_TimerJob_SPSettingDescription,
                    ssContent: RMResx.RM_CP_TimerJob_SPSetting_Desc,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 3:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_LocationSyn,
                    timerJobDesc: RMResx.RM_JS_TimerJob_LocationSynDescription,
                    ssContent: RMResx.RM_BCM_TermSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 4:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_UpdateLocation,
                    timerJobDesc: RMResx.RM_JS_TimerJob_UpdateLocationDescription,
                    ssContent: RMResx.RM_URL_Update_Options,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 7:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_UniqueIdSetting,
                    timerJobDesc: RMResx.RM_JS_TimerJob_UniqueIdSettingDescription,
                    ssContent: RMResx.RM_URL_Update_Options,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 9:
                return {
                    required: false,
                    jobDesc: RMResx.RM_CP_TimerJob_ManualApprove,
                    timerJobDesc: RMResx.RM_JS_TimerJob_ManualApproveDescription,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 13:
                return {
                    required: false,
                    jobDesc: RMResx.RM_CP_TimerJob_EnforceRetention,
                    timerJobDesc: RMResx.RM_TM_EnforceRetentionTimerDesc,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 15:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfSP,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfSPDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };

            case 16:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfEXO,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfEXODescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 17:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_EXOApplySetting,
                    timerJobDesc: RMResx.RM_CP_TimerJob_EXOApplySettingDescription,
                    ssContent: RMResx.RM_CP_TimerJob_SPSetting_Desc,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 21:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfFS,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfFSDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 24:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigSLNJobSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSLNJobScheduleDescription,
                    ssContent: RMResx.RM_CP_TimerJob_SLN_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 26:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_SPOnPremApplySetting,
                    timerJobDesc: RMResx.RM_CP_TimerJob_SPOnPremApplySettingDescription,
                    ssContent: RMResx.RM_CP_TimerJob_SPSetting_Desc,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 27:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfSPOnPrem,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfSPOnPremDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 29:
                return {
                    required: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfOneDrive,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfOneDriveDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 34:
                return {
                    required: true,
                    jobDesc: RMResx.RM_RC_Audit_Action_ConfigureAzureFileShareSyncDataSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfAzureFileShareDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 39:
                return {
                    required: true,
                    jobDesc: RMResx.RM_AR_CP_TimerJob_Retention,
                    timerJobDesc: RMResx.RM_AR_CP_TimerJob_RetentionDataDescription,
                    ssContent: RMResx.RM_AR_CP_TimerJob_Retention_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                    hasSupportConfigRetentionWhitelist: LicenseHelper.EnableRecordsArchiver() && RM.gData.enableCustomRetentionSettings,
                    ssConfigHead: RMResx.RM_CP_ConfigArchiveDataWhiteList
                };
            case 45:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_ConfigureBoxSyncDataSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_ConfigSyncDataScheduleOfBoxDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 48:
                return {
                    require: true,
                    jobDesc: RMResx.RM_RC_Audit_Action_ConfigureArchiverDeleteRestoredData,
                    timerJobDesc: RMResx.RM_RC_Audit_Action_ConfigureArchiverDeleteRestoredDataDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DeleteRestoredData_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                }
            case 60:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_GoogleDataSyncSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_GoogleDataSyncScheduleDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 61:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_GoogleSettingSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_GoogleSettingScheduleDescription,
                    ssContent: RMResx.RM_CP_TimerJob_SPSetting_Desc,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 72:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_TeamsSyncSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_TeamsDataSyncScheduleDescription,
                    ssContent: RMResx.RM_CP_TimerJob_DataSync_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 73:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_TeamsSettingSchedule,
                    timerJobDesc: RMResx.RM_CP_TimerJob_TeamsSettingScheduleDescription,
                    ssContent: RMResx.RM_CP_TimerJob_SPSetting_Desc,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 50:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_Deduplication,
                    timerJobDesc: RMResx.RM_CP_TimerJob_DeduplicationDescription,
                    ssContent: RMResx.RM_CP_TimerJob_Deduplication_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
            case 80:
                return {
                    require: true,
                    jobDesc: RMResx.RM_CP_TimerJob_DisposalStub,
                    timerJobDesc: RMResx.RM_CP_TimerJob_DisposalStub_Desc,
                    ssContent: RMResx.RM_CP_TimerJob_DisposalStub_ContentTitle,
                    ssContainsHead: RMResx.RM_CP_TimerJob_ConfigRecordSchedule,
                };
        }
    }

    getIntervalUnit() {
        if (this.settings.IntervalType == 1) {
            return RMResx.RM_JS_ScheduleSetting_Weeks;
        } else if (this.settings.IntervalType == 2) {
            return RMResx.RM_JS_ScheduleSetting_Days;
        } else {
            return RMResx.RM_JS_ScheduleSetting_Hours;
        }
    }

    getEndTimeDisplayStr(timeZone) {
        if (this.settings.EndType == 1) {
            return RM.TimeUtil.dateToString(new Date(this.settings.EndTime), timeZone, true);
        } else if (this.settings.EndType == 2) {
            return RMResx.RM_JS_ScheduleSetting_EndAfter + ' ' + this.settings.OccurrencesTotal + ' ' + RMResx.RM_JS_ScheduleSetting_Occurrences;
        } else {
            return RMResx.RM_JS_ScheduleSetting_NoEndDate;
        }
    }

    getRequestUrl() {
        var url = "";
        switch (this.props.type) {
            case 1:
                url = "/api/TermSynchronizationApi/RunSync";
                break;
            case 2:
                url = "/api/SPSettingApi/ApplySettings";
                break;
            case 3:
                url = "/api/LocationSynchronizationApi/RunSync";
                break;
            case 4:
                url = "/api/UpdateRecordLocationApi/RunSync";
                break;
            case 7:
                url = "/api/SPSettingApi/RunUniqueIdJob";
                break;
            case 9:
                url = "/api/TermSynchronizationApi/RunManualApprovalTimerJob";
                break;
            case 13:
                url = "/api/TermManagementApi/RunEnforceRetentionJob";
                break;
            case 15:
                url = "/api/SPSettingApi/RunSPSyncDataJob";
                break;
            case 16:
                url = "/api/EXOSettingApi/RunEXOSyncDataJob";
                break;
            case 17:
                url = "/api/EXOSettingApi/ApplyEXOSettings";
                break;
            case 21:
                url = "/api/FSSettingApi/RunFSSyncDataJob";
                break;
            case 24:
                url = "/api/SPOnPremSettingApi/RunScanLocalNodeJob";
                break;
            case 26:
                url = "/api/SPOnPremSettingApi/ApplySettings";
                break;
            case 27:
                url = "/api/SPOnPremSettingApi/RunSPSyncDataJob";
                break;
            case 29:
                url = "/api/OneDriveSettingApi/RunSPSyncDataJob";
                break;
            case 34:
                url = "/api/AzureFileSettingApi/RunDataSyncScheduleJob";
                break;
            case 39:
                url = "/api/RetentionApi/ManualRunRetentionJob";
                break;
            case 45:
                url = "/api/BoxSetting/RunDataSyncScheduleJob";
                break;
            case 48:
                url = "/api/RetentionApi/RunDeleteRestoredDataJob";
                break;
            // Google Drive
            case 60:
                url = "/api/GoogleDriveSettingApi/RunSyncDataJob";
                break;
            case 61:
                url = "/api/GoogleDriveSettingApi/ApplySettings";
                break;
            case 50:
                url = "/api/RetentionApi/RunArchiverDeduplicationJob";
                break;
            // Teams
            case 72:
                url = "/api/TeamsSettingApi/RunTeamsSyncDataJob";
                break;
            case 73:
                url = "/api/TeamsSettingApi/ApplySettings";
                break;
            case 80:
                url = "/api/StubSetting/RunStubDisposalJob";
                break;
        }
        return url;
    }

    onConfigSettingsClick = () => {
        this.setState({
            showConfigPanel: { show: true }
        }, () => {
            this.dispatch(this.configWhitelistPanelId, 'init');
        });
    }

    onEditSettingsClick() {
        this.setState({
            showSettingsPanel: { show: true }
        });
    }

    onConfigRetentionWhitelistSave = () => {
        this.dispatch(this.configWhitelistPanelId, 'save', (success) => {
            if (success) {
                this.onConfigRetentionWhitelistCancel();
            }
        });
        return false;
    }

    onConfigRetentionWhitelistCancel = () => {
        this.setState({
            showConfigPanel: { show: false },
        });
    }

    onSaveBtnClick() {
        this.dispatch(this.settingPanelId, 'save', (success, settings) => {
            if (success) {
                this.settings = settings;
                this.setState({
                    showSettingsPanel: { show: false },
                });
                this.initDetails();
            }
        });
        return false;
    }

    onCancelBtnClick() {
        this.setState({
            showSettingsPanel: { show: false },
        });
    }

    onRunNowClick() {
        $$.loading(true);
        const applySettingTypes = [2, 26, 17, 61, 73];
        let reqData = applySettingTypes.includes(this.props.type) ? { FromTimerJobPage: true, RunJobMethod: RunApplySettingMethod.Auto } : true;

        $.ajax({
            type: "post",
            dataType: "JSON",
            contentType: "application/json;charset=utf-8",
            url: this.getRequestUrl(),
            data: JSON.stringify(reqData),
            success: (data) => {
                $$.loading(false);
                if (data.MessageType == 0) {
                    this.props.onClickRun(true);
                }else{
                    this.props.onClickRun(false, data.ErrorMessage);
                }
            },
            error: (msg) => {
                $$.loading(false);
                this.props.onClickRun(false);
            }
        });
        return this;
    }

    onCreateNewRuleDialogClose() {
        this.setState({
            showSettingsPanel: { show: false },
        });
    }

    renderConfigPanel = (resources) => {
        return (
            <R.Panel
                header={resources.ssConfigHead}
                size={610}
                status={this.state.showConfigPanel}
                destroy={true}
            >
                <div style={{ marginTop: -16 }}>
                    <$g.ConfigArchiveDataWhitelistForm
                        id={this.configWhitelistPanelId}
                        type={this.props.type}
                        resources={resources}
                        chooseFileInputName="RetentionSettingsFileUp"
                        noChangeStatusHiddenInputName="IsNoChangeDirectSave"
                    />
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onConfigRetentionWhitelistCancel} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onConfigRetentionWhitelistSave} />
                </>
            </R.Panel>
        );
    }

    render() {
        let resources = this.getResources();

        return <React.Fragment>
            <div className="col-xlg-6 col-xs-12"><div className="ra-section">
                <div className="ra-section-head ra-inline-middle flex align-center ra-flex-justify-between">
                    <span tabIndex='0'>{resources.jobDesc}</span>
                    <div className="pull-right">
                        {resources.hasSupportConfigRetentionWhitelist && (
                            <R.Button
                                type="bald"
                                icon="fia-gear btn-gear-icon"
                                tooltip={RMResx.RM_CP_TimerJob_ConfigArchiveDataWhiteList_Button}
                                onClick={this.onConfigSettingsClick}
                            />
                        )}
                        <R.Button
                            type="bald"
                            icon="fia-edit btn-edit-icon"
                            tooltip={RMResx.RM_JS_TimerJob_EditSettings}
                            onClick={this.onEditSettingsClick} />
                        <R.Button
                            type="bald"
                            icon="fia-run btn-runjob-icon"
                            tooltip={RMResx.RM_DAM_RunNow}
                            onClick={this.onRunNowClick} />
                    </div>
                </div>
                <div className="timerjob-description ra-clearboth">
                    <span tabIndex="0">{resources.timerJobDesc}</span>
                </div>
                <div className="ra-section-headline" />
                <div className="ra-page-form">
                    <$g.DetailList labelWidth={150}>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_ScheduleSetting_StratTime}
                                value={this.state.startTime} />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_ScheduleSetting_Interval}
                                value={this.state.intervalValue} />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_ScheduleSetting_EndTime}
                                value={this.state.endTime} />
                        </$g.DetailRow>
                    </$g.DetailList>
                </div>
            </div></div>

            <R.Panel
                header={resources.ssContainsHead}
                size={610}
                status={this.state.showSettingsPanel}
                destroy={true}
            >
                <div>
                    <$g.ScheduleSetting
                        id={this.settingPanelId}
                        fromTimerJobPage={true}
                        type={this.props.type}
                        resources={resources}
                    />
                </div>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelBtnClick} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveBtnClick} />
                </>
            </R.Panel>
            {this.renderConfigPanel(resources)}
        </React.Fragment>;
    }
}
