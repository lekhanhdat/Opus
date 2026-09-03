import { bindEvents, showToast } from '../../Utilities/CommonUtil';
import Upload from "../CP/Upload";
import '../../Less/Common/ScheduleSetting.less';

class ScheduleSetting extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.changeStatus = false;
        this.noSchedule = false;
        this.defaultTimeZone = RM.TimeUtil.getGlobalTimezoneInfo();

        this.postForm = {
            Id: "1",
            StartTime: 0,
            EndTime: 0,
            StartTimeDate: new Date(),
            EndTimeDate: new Date(),
            TimeZoneId: "China Standard Time",
            Interval: 1,
            IntervalType: 0,
            EndType: 0,
            OccurrencesTotal: 1,
            JobCategory: this.props.type,
            ProfileId: "",
            IsDaylightSaving: false
        };
        this.state = {
            dedupUploadLists: [],               //tem信息
            //判断文件是否有变动                （true 变更    false 未变更）
            dedupConfigFileChanged: false,
            intervalSelect: '1',
            selectedDate: new Date(),
            selScheduleType: '1',
            selEndTimeType: '0',
            occurInputDisabled: true,
            endDatepickDisabled: true,
            intervalValue: "",
            occurrencesValue: 1,
            selectedTimeZone: this.defaultTimeZone,
            dateTimeFormat: RM.TimeUtil.getGlobalAuiFormat(),
            selectedEndDate: new Date(),
            showTermSyncTimeError: false,
            showIntervalNumberError: false,
            isShowUnderStartTime: false,
            showOccurrencesNumberError: false,
        };
        this.bind(this, 'onScheduleTypeChange', 'handleEndbyChanged', 'onEndTypeChange',
            'handleIntervalChange', 'onOccurrencesChange', 'byEndSeleChange', 'onIntSeleChange'
        );
        this.loadScheduleData();
    }


    componentReceive(type, args) {
        switch (type) {
            case "init":
                this.loadScheduleData();
                break;
            case "save":
                this.save(args, false);
                break;
            case "saveAndRun":
                this.save(args, true);
                break;
            default:
                break;
        }
    }

    GetSavedDedupSettingFromServer() {
        let option = {
            url: "/api/CPApi/GetSavedDedupTemplate"
        };
        fetchUtility(option)
            .then((res) => {
                this.setState({
                    dedupConfigFileChanged: false,
                    dedupUploadLists: !(res && res.FileName) ? [] :  [res]
                });
                $$.loading(false);
            })
            .catch((e) => {
                showToast.error(RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed);
                $$.loading(false);
            });
    }

    loadScheduleData() {
        let self = this;
        $.ajax({
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json;charset=utf-8',
            //url: '/api/TermSynchronizationApi/GetScheduleByType',
            url: '/api/BCMAdminSettingApi/GetScheduleByType',
            data: JSON.stringify(this.props.type),
            beforeSend: function () {
                $$.loading(true);
            },
            complete: () => {
                if (self.props.type == 50) {
                    self.GetSavedDedupSettingFromServer();
                } else {
                    $$.loading(false);
                }
            },
            success: (result) => {
                var schedule = result;
                if (schedule.length == 0) {
                    this.setState({
                        selScheduleType: '1',
                        selEndTimeType: '0',
                    });
                    this.noSchedule = true;
                    return false;
                } else {
                    this.noSchedule = false;
                    this.postForm = schedule[0];
                    this.setState({
                        selScheduleType: '2',
                        intervalSelect: this.postForm.IntervalType,
                        selectedDate: new Date(this.postForm.StartTime),
                        intervalValue: this.postForm.Interval,
                    });
                    switch (this.postForm.EndType) {
                        case 0:
                            this.setState({
                                selEndTimeType: '0',
                                occurInputDisabled: true,
                                endDatepickDisabled: true,
                            });
                            break;
                        case 1:
                            this.setState({
                                selEndTimeType: '1',
                                endDatepickDisabled: false,
                                occurInputDisabled: true,
                                selectedEndDate: new Date(this.postForm.EndTime),
                            });
                            $(".ra-schedule-endBy-datepicker").children().removeClass('aui-datepicker-disabled');
                            break;
                        case 2:
                            this.setState({
                                selEndTimeType: '2',
                                occurrencesValue: this.postForm.OccurrencesTotal,
                                occurInputDisabled: false,
                                endDatepickDisabled: true,
                            });
                            break;
                    }
                }
            },
            error: (msg) => {
            }
        });
    }

    getScheduleTypeOptions() {
        let options = [
            { text: RMResx.RM_JS_ScheduleSetting_NoSchedule, value: "1" },
            { text: RMResx.RM_ScheduleSetting_ConfigureSchedule, value: "2" }
        ];
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.selScheduleType == op.value;
            return op;
        });
    }

    getIntervalOptions() {
        let options = [
            { text: RMResx.RM_JS_ScheduleSetting_Weeks, value: "1" },
            { text: RMResx.RM_JS_ScheduleSetting_Days, value: "2" },
            { text: RMResx.RM_JS_ScheduleSetting_Hours, value: "3" }
        ];
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.intervalSelect == op.value;
            return op;
        });
    }

    handleIntervalChange(value) {
        this.setState({
            intervalValue: value,
        });
    }

    onIntSeleChange(args) {
        this.setState({
            intervalSelect: args.newValue.value,
        });
    }

    onEndTypeChange(val) {
        this.setState({
            selEndTimeType: val
        });
        if (val == "2") {
            this.setState({ occurInputDisabled: false });
        } else {
            this.setState({ occurInputDisabled: true });
        }
        if (val == "1") {
            this.setState({ endDatepickDisabled: false });
            $(".ra-schedule-endBy-datepicker").children().removeClass('aui-datepicker-disabled');
        } else {
            this.setState({ endDatepickDisabled: true });
        }
    }

    onScheduleTypeChange(value) {
        this.setState({
            selScheduleType: value,
        });
    }

    handleEndbyChanged(args) {
        this.setState({
            selectedDate: args.newValue,
        });
    }

    byEndSeleChange(args) {
        this.setState({
            selectedEndDate: args.newValue,
        });
    }

    onOccurrencesChange(value) {
        this.setState({
            occurrencesValue: value,
        });
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
            case 24:
                url = "/api/SPOnPremSettingApi/RunScanLocalNodeJob";
                break;
            case 26:
                url = "/api/SPOnPremSettingApi/ApplySettings";
                break;
            case 27:
                url = "/api/SPOnPremSettingApi/RunSPSyncDataJob";
                break;
            case 39:
                url = "/api/RetentionApi/ManualRunRetentionJob";
                break;
            case 45:
                url = "/api/BoxSetting/RunDataSyncScheduleJob";
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
        }
        return url;
    }

    //选择文件成功的状态
    chooseFileSuccess(file) {
        //判断状态是否有更改
        if (file.element_name == "fileUp") {
            this.setState({
                dedupConfigFileChanged: true
            });
        }
        if (file.fileMessage) {
            this.showMsgToast(file.fileMessage,"error",true);
        }
    }

    //删除（不算变更）
    temDeleteFileSuccess() {
        this.setState({
            dedupConfigFileChanged: true
        });
    }

    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    save(callback, runJob) {
        var postUrl = "",
            postData = "",
            regx = /\D/g;
        const googleDriveTypes = [60, 61];
        const isSettingGoogleDriveSchedule = googleDriveTypes.includes(this.props.type);
        const scheduleServiceUrl = {
            CreateSchedule: isSettingGoogleDriveSchedule
                ? "/api/GoogleDriveSettingApi/CreateSchedule"
                : "/api/TermSynchronizationApi/CreateSchedule",
            UpdateScheduleService: isSettingGoogleDriveSchedule
                ? "/api/GoogleDriveSettingApi/UpdateScheduleService"
                : "/api/TermSynchronizationApi/UpdateScheduleService",
            DeleteScheduleService: isSettingGoogleDriveSchedule
                ? "/api/GoogleDriveSettingApi/DeleteScheduleService"
                : "/api/TermSynchronizationApi/DeleteScheduleService"
        }

        this.setState({
            showTermSyncTimeError: false,
            showIntervalNumberError: false,
            showOccurrencesNumberError: false,
        });
        let postStartTime = RM.TimeUtil.getCommonDateStr(this.state.selectedDate);
        let postInterval = this.state.intervalValue - 0;
        let postOccurrencesTotal = this.state.occurrencesValue - 0;
        let postEndTime = RM.TimeUtil.getCommonDateStr(this.state.selectedEndDate);
        let postEndType = this.state.selEndTimeType;
        // judge user change schedule begin
        if (new Date(this.postForm.StartTime).getTime()
            != new Date(postStartTime).getTime()) {
            // the frist load
            this.changeStatus = true;
        }
        if (this.postForm.Interval != postInterval) {
            this.changeStatus = true;
        }

        if (this.postForm.IntervalType != postInterval) {
            this.changeStatus = true;
        }
        if (this.postForm.EndType != postEndType) {
            this.changeStatus = true;
        }
        if (this.postForm.EndType == postEndType &&
            this.postForm.OccurrencesTotal != postOccurrencesTotal) {
            this.changeStatus = true;
        }
        if (this.postForm.EndType == postEndType && postEndType == 1
            && new Date(this.postForm.EndTime).getTime() != new Date(postEndTime).getTime()) {
            this.changeStatus = true;
        }
        // judge user change schedule end
        if (this.state.selScheduleType != '1' && this.changeStatus) {
            if (new Date(postStartTime).getTime() > new Date(postEndTime).getTime() && postEndType == 1) {
                this.setState({
                    showTermSyncTimeError: true,
                    isShowUnderStartTime: false
                });
                return false;
            } else {
                this.setState({
                    showTermSyncTimeError: false,
                    isShowUnderStartTime: true,
                });
            }

            if (regx.test(postInterval) || postInterval < 1 || postInterval > 65535) {
                this.setState({
                    showIntervalNumberError: true,
                });
                return false;
            } else {
                this.setState({
                    showIntervalNumberError: false,
                });
            }

            if (postEndType == 2 && (regx.test(postOccurrencesTotal) ||
                postOccurrencesTotal < 1 || postOccurrencesTotal > 65535)) {
                this.setState({
                    showOccurrencesNumberError: true,
                });
                return false;
            } else if (postEndType == 2) {
                this.postForm.OccurrencesTotal = this.state.occurrencesValue - 0;
                this.setState({
                    showOccurrencesNumberError: false,
                });
            } else if (postEndType != 2) {
                this.postForm.OccurrencesTotal = 1;
                this.setState({
                    showOccurrencesNumberError: false,
                });
            }
        }
        this.postForm.NoSchedule = this.state.selScheduleType == '1';
        this.postForm.StartTime = RM.TimeUtil.getCommonDateStr(this.state.selectedDate) + ':00';
        this.postForm.StartTimeDate = this.state.selectedDate;
        this.postForm.EndTime = RM.TimeUtil.getCommonDateStr(this.state.selectedEndDate) + ':00';
        this.postForm.EndTimeDate = this.state.selectedEndDate;
        this.postForm.Interval = this.state.intervalValue - 0;
        this.postForm.OccurrencesTotal = this.state.occurrencesValue;
        this.postForm.IntervalType = this.state.intervalSelect;
        this.postForm.TimeZoneId = this.state.selectedTimeZone.id;
        this.postForm.IsDaylightSaving = this.state.selectedTimeZone.autoAdjustClock;
        this.postForm.EndType = this.state.selEndTimeType;
        if (!(this.noSchedule && this.state.selScheduleType == '1')) {
            if (this.state.selScheduleType == '1') {
                postUrl = scheduleServiceUrl.DeleteScheduleService;
                postData = this.postForm.Id;
            } else {
                if (this.postForm.Id != "1") {
                    postUrl = scheduleServiceUrl.UpdateScheduleService;
                    postData = this.postForm;
                } else {
                    postUrl = scheduleServiceUrl.CreateSchedule;
                    postData = this.postForm;
                }
            }
            $$.loading(true);
            let option = {
                url: postUrl,
                method: "POST",
                data: postData
            };
            fetchUtility(option).then((id) => {
                if (id == "-1") {
                    this.setState({
                        showTermSyncTimeError: true,
                    });
                    $$.loading(false);
                    return;
                } else {
                    this.postForm.Id = id;
                }
                if (this.state.selScheduleType == '1') {
                    this.postForm.Id = "1";
                    this.changeStatus = true;
                    this.noSchedule = true;
                } else {
                    this.changeStatus = false;
                    this.noSchedule = false;
                }

                let funcSuccess = () => {
                    $$.loading(false);
                    if (runJob) {
                        this.runNow();
                    }
                    callback(true, this.postForm);
                };
                if (this.props.type == 50 && this.state.dedupConfigFileChanged) {
                    this.UpdateDedupSettingFile(funcSuccess);
                } else {
                    funcSuccess();
                }
            }).catch((e) => {
                callback(false);
                $$.loading(false);
            });
        } else {
            if (this.props.type == 50 && this.state.dedupConfigFileChanged) {
                this.UpdateDedupSettingFile(() => callback(true, { NoSchedule: true }));
            } else {
                callback(true, { NoSchedule: true });
            }
        }
        return this;
    }

    //保存接口
    UpdateDedupSettingFile(callback) {
        let self = this;
        let ajaxFormOption = {
            type: "POST",
            url: "/CPApi/UpdateDedupSettingFile",
            success: function (data) {
                callback();
                if (!data.success) {
                    self.showMsgToast(data.message,"error",true);
                }
            },
            error: function (dataObj) {
                callback();
                self.showMsgToast("Request Error","error",true);
            },
        };
        $("#form-import").ajaxSubmit(ajaxFormOption);
    }

    runNow() {
        $$.loading(true);
        $.ajax({
            type: "post",
            dataType: "JSON",
            contentType: "application/json;charset=utf-8",
            url: this.getRequestUrl(),
            data: JSON.stringify(this.props.fromTimerJobPage),
            success: () => {
                $$.loading(false);
            },
            error: (msg) => {
                $$.loading(false);
            }
        });
        return this;
    }

    renderDedupConfigUploader() {
        return this.props.type == 50 && <form id="form-import" encType="multipart/form-data" action="" method="post">
            <div className="ra-page-form">
                <div className="ra-form-label">
                    <span tabIndex='0'>{RMResx.RM_ES_UploadConfiguration}</span>
                </div>
                <div className="ra-form-content">
                    <Upload
                        fileTypes={"XLSX"}
                        fileSize={5}
                        downLoadUrl='/api/CPApi/DownloadDedupTemplate'
                        uploadLists={this.state.dedupUploadLists}
                        multiple={false}
                        savedFileUrl='/api/CPApi/DownloadSavedDedupSettingFile'
                        chooseFileInputName='fileUp'
                        noChangeStatusHiddenInputName='dedupIsNoChangeDirectSave'
                        chooseFileSuccess={this.chooseFileSuccess.bind(this)}
                        deleteFileSuccess={this.temDeleteFileSuccess.bind(this)}
                    >
                    </Upload>
                </div>
            </div>
        </form>;
    }

    render() {
        return <div className="ra-schedule">
            {this.renderDedupConfigUploader()}
            <div className="ra-form-label require">
                <span tabIndex='0'>{this.props.resources.ssContent}</span>
            </div>
            <div className="ra-form-content">
                <div className="margin-bottom-m">
                    <R.Radio.Group
                        name="radiogroup-schedule"
                        items={this.getScheduleTypeOptions()}
                        onChange={this.onScheduleTypeChange}
                        block={true}
                    />
                </div>
                <div className={"schedule-body " + (this.state.selScheduleType == "2" ? "block" : "none")}>
                    <div className="ra-inline-middle margin-bottom-m">
                        <div tabIndex="0" className="schedule-label">
                            {RMResx.RM_JS_ScheduleSetting_StratTime}:
                        </div>
                        <R.Datepicker
                            id="raCpScheduleStratDate"
                            width={318}
                            dateTimeFormat={this.state.dateTimeFormat}
                            selectedDate={this.state.selectedDate}
                            disabled={false}
                            hasTimePicker={true}
                            // hasTimeZone={true}
                            // timezones={this.state.timezones}
                            onChange={this.handleEndbyChanged} />
                        <$g.ValidationMsg show={this.state.showTermSyncTimeError && this.state.isShowUnderStartTime}>
                            {RMResx.RM_JS_ScheduleSetting_TimeError}
                        </$g.ValidationMsg>
                    </div>
                    <div className="ra-inline-middle margin-bottom-m">
                        <div tabIndex="0" className="schedule-label" >
                            {RMResx.RM_JS_ScheduleSetting_Interval}:
                        </div>
                        <R.Input id="raCpScheduleIntervalNumIpt" type="number" hasControl width={155} min={1} max={9999}
                            value={this.state.intervalValue} onChange={this.handleIntervalChange}  aria={{ariaLabel:RMResx.RM_JS_ScheduleSetting_Interval}} />
                        <div className='margin-left-8'>
                            <R.Combobox
                                id="raCpScheduleTimeIntervalUnit"
                                width={155}
                                searchable={false}
                                textField='text'
                                valueField='value'
                                checkedField='checked'
                                items={this.getIntervalOptions()}
                                onChange={this.onIntSeleChange}
                            />
                        </div>
                        <$g.ValidationMsg show={this.state.showIntervalNumberError}>
                            {RMResx.RM_JS_ScheduleSetting_NumberError}
                        </$g.ValidationMsg>
                    </div>
                    <div className="schedule-endtime ra-form-content ra-inline-top">
                        <div tabIndex="0" className="schedule-label">
                            {RMResx.RM_JS_ScheduleSetting_EndTime}:
                        </div>

                        <$g.RadioGroup
                            name="cp-schedule-end-type"
                            onChange={this.onEndTypeChange}
                            value={this.state.selEndTimeType}>
                            <$g.RadioOption value="0" text={RMResx.RM_JS_ScheduleSetting_NoEndDate} />
                            <$g.RadioOption value="2" text={RMResx.RM_JS_ScheduleSetting_EndAfter}>
                                <div className="margin-left-8">
                                    <R.Input id="raCpScheduleOccurrencesNumIpt" type="number" hasControl width={130} min={1} max={9999}
                                        value={this.state.occurrencesValue} disabled={this.state.occurInputDisabled}
                                        onChange={this.onOccurrencesChange} aria={{ariaLabel:RMResx.RM_JS_ScheduleSetting_EndTime}} />
                                </div>
                                <span className="margin-left-8">
                                    {RMResx.RM_JS_ScheduleSetting_Occurrences}
                                </span>

                                <$g.ValidationMsg show={this.state.showOccurrencesNumberError}>
                                    {RMResx.RM_JS_ScheduleSetting_NumberError}
                                </$g.ValidationMsg>
                            </$g.RadioOption>
                            <$g.RadioOption value="1" text={RMResx.RM_JS_ScheduleSetting_EndByDate}>
                                <div className='ra-schedule-endBy-datepicker'>
                                    <R.Datepicker
                                        id="raCpScheduleEndDate"
                                        width={220}
                                        dateTimeFormat={this.state.dateTimeFormat}
                                        selectedDate={this.state.selectedEndDate}
                                        disabled={this.state.endDatepickDisabled}
                                        hasTimePicker={true}
                                        onChange={this.byEndSeleChange} />
                                </div>
                                <div className='ra-schedule-endBy-inValid-msg'>
                                    <$g.ValidationMsg
                                        show={this.state.showTermSyncTimeError && !this.state.isShowUnderStartTime}>
                                        {RMResx.RM_JS_ScheduleSetting_TimeError}
                                    </$g.ValidationMsg>
                                </div>
                            </$g.RadioOption>
                        </$g.RadioGroup>
                    </div>
                </div>

            </div>
        </div>;
    }
}

const propTypes = {};

const defaultProps = {};

ScheduleSetting.propTypes = propTypes;
ScheduleSetting.defaultProps = defaultProps;

export { ScheduleSetting };