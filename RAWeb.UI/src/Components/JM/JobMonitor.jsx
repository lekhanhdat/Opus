import StringUtil from "../../Utilities/StringUtil";
import { withRouter } from "react-router-dom";
import * as JMConstants from "./JMConstants";
import { setCheckedStatus } from '../../Utilities/CommonUtil';
import { EmptyGUID } from "../../Constants/Constants";
import { JobMounitorTemplate } from "./JMTableTemplate";
import JMTable from "./JMTable";
import { JobMonitorFilterForm } from "./JobMonitorFilterForm";
import { checkPermission } from '../../Utilities/permissionManager';
import TopButtonsComponent from "../Common/Util/TopButtonsComponent";
import { LicenseHelper } from "../../Utilities/CommonUtil";
import { showToast } from "../../Utilities/CommonUtil";
import _ from "lodash";

const defaultManagedColumns = [
    { isChecked: true, value: RMResx.RM_JS_JM_JobID, Id: 0, isDynamic: true },
    { isChecked: true, value: RMResx.RM_JS_JM_Module, Id: 1, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_Progress, Id: 2, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_Status, Id: 3, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_Priority, Id: 4, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JM_JS_Location, Id: 5, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_StartTime, Id: 6, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_EndTime, Id: 7, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_UserName, Id: 8, isDynamic: false }
];

const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();

class JobMonitor extends R.Component {
    constructor(props) {
        super(props);
        this.defaultShowActions = {
            showExportSettings: true,
            showPriority: false,
            showRefresh: true,
            showReport: false,
            showDownload: false,
            showDelete: false,
            showStop: false
        }; 
        this.cacheFilterData = RM.getSessionStorage(`${props.filterCacheNamePrefix}JMFilteData`);
        this.cacheManagedColumnsIds = RM.getSessionStorage(props.manageColumnCacheName);
        this.state = {
            jobsChecked: [],
            jobsCount: 0,             //分页数据总数
            jobsPagerIndex: 0,         //分页 页码 从1开始
            jobsPagerSize: 10,         //分页每页条数
            showActions: this.defaultShowActions,   //viewdetail, delete, stop, refresh, download, showreport, exportsetting
            ManagedColumns: this.getCacheManagedColumns(),
            allColumns: this.getColumns(),
            items: [],
            exportSettingShow: false,
            selectedExportLocation: {},
            isExportToBrowser: true,
            exportLocations: [],
            noDownLoadToValue: false,
            exportLocationNotFound: false,
            exportLocationNotFoundContent: '',
            showFilterPanel: false,
            jobTypeOptions: [],
            filterOptionsInfo: {},
			isFiltered: false,
			editPriorityShow: false,
			priorityValue: 0
        };
        this.selectedFinished = [];
        this.selectedNeedStopped = [];
        this.filterData = this.getDefaultPager();
        this.columnFilters = {}; //选中filter中checkbox的信息以及ColumnIndex和ColumnName
        this.exportSetting = 0;
        this.defaultShowBtnsCounter = 4;
        this.exportTypes = {
            Browser: "0",
            Location: "1"
        };
        this.setJobTypeOptions();
        this.initBinding();
    }

    getButtonsInfo() {
        return {
            "ExportSettings": { isStatic: true, name: RMResx.RM_JS_EL_ExportSettings, onClick: this.exportSettingEvent, isShow: this.state.showActions.showExportSettings },
            "Priority": { name: RMResx.RM_JS_JM_Priority_Btn, icon: "fia-edit", onClick: this.onEditPriority, isShow: this.state.showActions.showPriority },
            "Refresh": { name: RMResx.RM_JS_JM_Refresh_Btn, icon: "fia-refresh", onClick: this.onRefresh, isShow: this.state.showActions.showRefresh },
            "ShowReport": { name: RMResx.RM_JS_Common_ShowReport, icon: "fia-show-report", onClick: this.showReportEvent, isShow: this.state.showActions.showReport },
            "StaticDownloadReport": { isStatic: true, name: RMResx.RM_JS_JM_Download_Btn, onClick: this.downloadClick, isShow: this.state.showActions.showDownload },
            "DownloadReport": { name: RMResx.RM_JS_JM_Download_Btn, icon: "fia-download", onClick: this.downloadClick, isShow: this.state.showActions.showDownload },
            "Delete": { name: this.props.deleteButtonName, icon: "fia-delete", onClick: this.onDelete, isShow: this.state.showActions.showDelete },
            "Stop": { name: RMResx.RM_JS_JM_Stop_Btn, icon: "fia-stop", onClick: this.onStop, isShow: this.state.showActions.showStop },
        }
    }
    
    initBinding() {
        const eventsArr = ["onPagerChange", "selectChange", "managedColumnChanged",
            "onSearchStart", "initData", "routerTo", "exportSettingEvent", "hideFilterPanel", "onFilter",
            "didHide", "onCancelSetting", "exportRadioChange", "onSaveSetting", "onRefresh", "showReportEvent", "downloadClick", "openFilterPanel",
			"onDelete", "onStop", "exportLocationChange", "onDeleteSureClick", "onDeleteCancleClick", "onEditPriority", "hidePriorityPanel", "onSavePriority",
			"priorityValueChange"];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    componentInit() {
        this.initData(true);
        this.loadExportSetting();
        if(this.cacheManagedColumnsIds){
            this.setTableColumnByManagedColumns(this.cacheManagedColumnsIds);
        }
    }

    getCacheManagedColumns(){
        let managedColumns = RM.deepcopy(defaultManagedColumns);
        if( !RM.gData.enableRecordsArchiver )
        {
            managedColumns = managedColumns.filter(item => item.Id!= 5);
        }
        if(this.cacheManagedColumnsIds){
            managedColumns = managedColumns.map((item)=>{
                item.isChecked = this.cacheManagedColumnsIds.includes(item.Id);
                return item;
            });
        }
        return managedColumns; 
    }

    sortFun(property) {
        return function (item1, item2) {
            var value1 = item1[property];
            var value2 = item2[property];
            if (value1 < value2) {
                return -1;
            } else if (value1 > value2) {
                return 1;
            } else {
                return 0;
            }
        };
    }

    getStatusOptions() {
        let statusFilterData = [];
        let data = JMConstants.JobStatusI18N;
        if (this.props.filterSupportStatus) {
            let supportStatusObj = {};
            this.props.filterSupportStatus.forEach(key => {
                if (data.hasOwnProperty(key)) {
                    supportStatusObj[key] = data[key];
                }
            });
            data = supportStatusObj;
        }
        for (let key in data) {
            if (data.hasOwnProperty(key)) {
                let item = {};
                item.id = key;
                item.name = data[key];
                item.checked = true;
                statusFilterData.push(item);
            }
        }
        statusFilterData = statusFilterData.sort(this.sortFun('name'));
        return statusFilterData;
    }

    unSupportedJobType = (jobType) => {
        const values = [
            JMConstants.JobType.DiscoveryFileSystemV1,
            JMConstants.JobType.DownloadJobReportsForCOP
        ];
        return _.omit(jobType, values);
    }

    setJobTypeOptions() {
        let urlData = "/api/JMApi/QueryFilterList?filterValue=JobType";
        let option = {
            url: urlData,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            data = this.unSupportedJobType(data);
            let jobTypeOptions = [];

            if (this.props.filterSupportJobType) {
                let supportJobObj = {};
                this.props.filterSupportJobType.forEach(key => {
                    if (data.hasOwnProperty(key)) {
                        supportJobObj[key] = data[key];
                    }
                });
                data = supportJobObj;
            }
            for (let key in data) {
                if (data.hasOwnProperty(key)) {
                    let item = {};
                    item.id = key;
                    item.name = data[key];
                    item.checked = true;
                    jobTypeOptions.push(item);
                }
            }
            let obj = {};
            jobTypeOptions = jobTypeOptions.reduce((current, next) => {
                if (obj[next.name]) {
                    for (let item of current) {
                        if (next.name == item.name) {
                            item.id = item.id + ',' + next.id;
                        }
                    }
                } else {
                    obj[next.name] = true && current.push(next);
                }
                return current;
            }, []);
            this.setState({
                jobTypeOptions: jobTypeOptions.sort(this.sortFun('name'))
            });
        });
    }

    getColumns() {
        let columns = [
            {   id: 1 ,
                header: RMResx.RM_JS_JM_JobID,
                width: [240],
                resizeable: true,
                sortable: this.props.supportSort,
                valuePath: "Id",
            },
            {
                id: 2 ,
                header: RMResx.RM_JS_JM_Module,
                width: [300],
                resizeable: true,
                sortable: this.props.supportSort,
                valuePath: "JobType",
            },
            {
                id: 3 ,
                header: RMResx.RM_JS_JM_Progress,
                resizeable: true,
                width: 250,
                sortable: this.props.supportSort,
                valuePath: "Progress",
            },
            {
                id: 4 ,
                header: RMResx.RM_JS_JM_Status,
                resizeable: true,
                width: [200],
                sortable: this.props.supportSort,
                valuePath: "Status",
			},
			{
                id: 5 ,
                header: RMResx.RM_JS_JM_Priority,
                resizeable: true,
                width: [200],
                sortable: this.props.supportSort,
                valuePath: "JobPriority",
            },
            {
                id: 6 ,
                header: RMResx.RM_JM_JS_Location,
                resizeable: true,
                width: [300],
                valuePath: "Location",
            },
            {
                id: 7 ,
                header: RMResx.RM_JS_JM_StartTime,
                resizeable: true,
                width: [300],
                sortable: this.props.supportSort,
                valuePath: "StartTime",
            },
            {
                id: 8 ,
                header: RMResx.RM_JS_JM_EndTime,
                resizeable: true,
                width: [300],    
                sortable: this.props.supportSort,
                valuePath: "EndTime",   
            },
            {
                id: 9 ,
                header: RMResx.RM_JS_JM_UserName,
                resizeable: true,
                width: [250],
                sortable: this.props.supportSort,
                valuePath: "UserName",
            }
        ];
        columns = !RM.gData.enableRecordsArchiver? columns.filter(item => item.id !== 6) : columns;
        return columns;
    }

    onSort = (isAsc, columnName) => {
        this.filterData.IsDesc = !isAsc;
        this.filterData.SortBy = columnName;
        this.initData(true);
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    openFilterPanel() {
        this.setState({ showFilterPanel: true });
    }

    hideFilterPanel() {
        this.setState({ showFilterPanel: false });
    }

    hidePriorityPanel() {
        this.setState({ editPriorityShow: false });
    }

    onDelete() {
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div>
                    {this.props.deleteJobConfirmContent}
                </div>
                <div className='ra-jobMonitor-deleted-jobs'>
                    {
                        this.state.jobsChecked.map((item, index) => {
                            return <div key={index}>
                                <span>{item.JobId}</span>
                                {this.state.jobsChecked.length - 1 != index &&
                                    <span>,</span>
                                }
                            </div>;
                        })
                    }
                </div>
            </div>,
            buttons: [
                {text: RMResx.RM_JS_Common_Cancel, onClick: this.onDeleteCancleClick},
                {text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSureClick} 
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onDeleteSureClick() {
        $$.messagedialog(false);
        $$.loading(true);
        let urlData = this.props.deleteJobUrl;
        let idList = [];
        for (let key of this.state.jobsChecked) {
            idList.push(key.JobId);
        }
        let option = {
            url: urlData,
            method: "POST",
            data: idList
        };
        fetchUtility(option).then((res) => {
            if (res.MessageType === 0) {
                this.onRefresh();
                this.showMsgToast(this.props.deleteJobSuccessMsg, 'success', true);
            } else {
                if (res.ErrorMessage) {
                    this.showMsgToast(res.ErrorMessage, 'error', true);
                }
                this.showMsgToast(RMResx.RM_JS_JM_SelectedJobActionError, 'error', true);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDeleteCancleClick() {
        $$.messagedialog(false);
    }

    viewDetail = (args) => {
        let jobId = args.JobId;
        let jobType = args.JobTypeCode;
        let isUnmergedJob = args.IsUnMergedJob ?? false;
        if (jobType == 20 || jobType == 8120) {
            this.routerTo(`/Root/JM/PlanDetails/?id=${jobId}&type=${jobType}`);
        } else {
            this.routerTo("/Root/JM/Detail", {id: jobId, type: jobType, isUnmergedJob: isUnmergedJob});
        }
    }

    showReportEvent(jobs) {
        let jobId = this.state.jobsChecked[0].JobId;
        let jobType = this.state.jobsChecked[0].JobTypeCode;
        let url = "";
        let reportJobType = 1;
        switch (jobType) {
            case 1:
            case 1000:
            case 2100:
            case 4100:
            case 5004:
            case 6103:
            case 5510:
            case 10102:
            case 10208:
            case 10306:
                url = "DueDisposalReport/ShowReport";
                reportJobType = 1;
                break;
            case 2:
            case 6:
            case 19:
            case 1001:
            case 1002:
            case 1003:
            case 2101:
            case 2102:
            case 2103:
            case 4101:
            case 4102:
            case 4103:
            case 5010:
            case 5011:
            case 5012:
            case 5512:
            case 5513:
            case 5514:
            case 6100:
            case 6101:
            case 6102:
            case 10104:
            case 10105:
            case 10106:
            case 10209:
            case 10210:
            case 10211:
            case 10307:
            case 10308:
            case 10309:
                url = "TermUsageReport/ShowReport";
                reportJobType = 2;
                break;
            case 13:
            case 1004:
            case 2104:
            case 4104:
            case 5006:
            case 6104:
            case 5511:
            case 10103:
            case 10205:
            case 10305:
                url = "TimeFrameFileReport/ShowReport";
                reportJobType = 13;
                break;
            case 14:
                url = "AvailableSpaceReport/ShowReport";
                reportJobType = 14;
                break;
            case 8000:
            case 8019:
            case 10310:
                url = "ActionAuditReport/ShowReport";
                reportJobType = 8000;
                break;
            case 21:
            case 6113:
            case 10311:
            case 10214:
                url = "RestoreReport/ShowReport";
                reportJobType = 21;
                break;
        }
        window.location.href = "/root/RC/" + url + "?type=" + jobType + "&jobid=" + jobId;
    }

    getDownloadReportLimit() {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_JM_DownloadReportLimit,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false, args);
                    }
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    downloadClick() {
        let jobsChecked = this.state.jobsChecked;
        if (jobsChecked && jobsChecked.length > 15) {
            this.getDownloadReportLimit();
            return;
        }
        $$.loading(true);
        let jobIds = [];
        // let requestVerificationToken = getRequestVerificationToken();
        jobsChecked.forEach((item) => {
            jobIds.push(item.JobId);
        });
        if (this.exportSetting == 0) {
            let option = {
                url: "/api/JMApi/DownloadLogFile",
                method: "POST",
                data: jobIds,
            };
            fetchUtility(option)
                .then((result) => {
                    let resultData = JSON.parse(result);
                    if (resultData.MessageType == 0) {
                        showToast.success(
                            <$g.I18NProvider
                                msg={RMResx.RM_MA_HistoryExport_JobStart}
                            >
                                <a className="ra-link-a" href="/Root/JM/Index">
                                    {RMResx.RM_JS_JM_Title}
                                </a>
                                <a
                                    className="ra-link-a"
                                    href="/Root/DC/Download"
                                >
                                    {RMResx.RM_JS_DC_Title}
                                </a>
                            </$g.I18NProvider>
                        );
                    } else {
                        if (resultData.ErrorMessage) {
                            showToast.error(resultData.ErrorMessage);
                        }
                    }
                    $$.loading(false);
                })
                .catch((e) => {
                    $$.loading(false);
                });
        } else {
            let urlData = "/api/JMApi/StartJobExport";
            let option = {
                url: urlData,
                method: "POST",
                data: jobIds
            };
            fetchUtility(option).then((data) => {
                var resultData = JSON.parse(data);
                if (resultData.MessageType === 0) 
                {
                    this.showMsgToast(RMResx.RM_JS_EL_RunJob_Succeed, 'success', true);                    
                }
                else
                {
                    this.showMsgToast(resultData.ErrorMessage, 'error', true);  
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    onStop(jobs) {
        this.args = {
            classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div>
                    {RMResx.RM_JS_JM_ConfirmStopJobs}
                </div>
                {
                    this.state.jobsChecked.map((item, index) => {
                        return <div key={index}>
                            <span>{item.JobId}</span>
                            {this.state.jobsChecked.length - 1 != index &&
                            <span>,</span>
                            }
                        </div>;
                    })
                }
            </div>,
            buttons: [
                {text: RMResx.RM_JS_Common_Cancel, onClick: this.onStopCancel},
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.onStopOk(jobs);
                    }
                }
            ]
        };
        $$.messagedialog(true, this.args);
    }

    onStopOk() {
        $$.messagedialog(false);
        $$.loading(true);
        let jobIds = [];
        this.state.jobsChecked.forEach((item, index) => {
            jobIds.push(item.JobId);
        });
        let urlData = "/api/JMApi/StopJobs";
        let option = {
            url: urlData,
            method: "POST",
            data: jobIds
        };
        fetchUtility(option).then((data) => {
            if (data > 0) {
                this.onRefresh();
                this.showMsgToast(RMResx.RM_JS_JM_StopJobSuccess, 'success', true);
            } else {
                this.showMsgToast(RMResx.RM_JS_JM_SelectedJobStopError, 'error', true);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onStopCancel() {
        $$.messagedialog(false);
    }

    onRefresh() {
        this.setState({ showActions: this.defaultShowActions });
        this.initData(true);
    }

    exportSettingEvent() {
        this.setState({ exportSettingShow: true });
        this.loadExportSettingWithLocation();
	}
	
	onEditPriority() {
		this.setState({ editPriorityShow: true });
	}

    loadExportSetting() {
        let urlData = "/api/JMApi/GetJobDownloadSetting";
        let option = {
            url: urlData,
            method: "Get"
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            if (data) {
                this.exportSetting = data;
            }
        }).catch((e) => {

        });
    }

    loadExportSettingWithLocation() {
        $$.loading(true);
        let urlData = "/api/JMApi/GetJobExportSetting";
        let option = {
            url: urlData,
            method: "Get"
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            let locationId = data.JobExportSetting.ExportLocationId;
            let locationIds = [];
            let checkedItem = {
                ID: data.JobExportSetting.ExportLocationId,
                Name: data.JobExportSetting.ExportLocationId,
            };
            for (let item of data.AllExportLocation) {
                locationIds.push(item.ID);
            }
            if (data && data.AllExportLocation) {
                let AllExportLocation = setCheckedStatus("ID", "Checked", data.AllExportLocation, checkedItem);
                this.setState({
                    exportLocations: AllExportLocation,
                    isExportToBrowser: data.JobExportSetting.ExportSetting == 0,
                    selectedExportLocation: checkedItem
                });
                if (data.JobExportSetting.ExportSetting == 1 && locationId && locationIds.indexOf(locationId) < 0) {
                    this.setState({
                        exportLocationNotFound: true,
                        exportLocationNotFoundContent: StringUtil.stringFormat(RMResx.RM_JS_SPS_ExportSettting_ExportLocationNotFound, data.JobExportSetting.LocationName),
                        Name: data.JobExportSetting.LocationName,
                        ID: data.JobExportSetting.ExportLocationId
                    });
                } else {
                    this.setState({exportLocationNotFound: false});
                }
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onFilter() {
        let callback = (filterOptionsInfo) => {
            this.filterData.Filters = [];
            if (filterOptionsInfo) {
                if (filterOptionsInfo.JobType && filterOptionsInfo.JobType.every((item) => item.checked) && filterOptionsInfo.JobType.length === this.state.jobTypeOptions.length) {
                    delete filterOptionsInfo.JobType;
                }

                if (filterOptionsInfo.Status && filterOptionsInfo.Status.every((item) => item.checked) && filterOptionsInfo.Status.length === this.getStatusOptions().length) {
                    delete filterOptionsInfo.Status;
                }
            }

            for (let key in filterOptionsInfo) {
                let filterParam = { ColumnName: key, ColumnValues: [] };
                let filterOptions = filterOptionsInfo[key];
                let filterOptionValues;

                // Case for Start time and End time
                if (key == "StartTime" || key == "EndTime") {
                    filterOptionValues = filterOptions.length ? filterOptions.map(item => item.Value) : [];
                    filterParam.ColumnValues = filterOptions.length ? [...filterOptionValues] : filterOptionValues;
                } else {
                    // For others case
                    filterOptionValues = filterOptions.filter((item) => item.checked || item.Checked).map((option) => {
                        const returnValue = {
                            "JobType": option.id,
                            "Status": option.id,
                            "UserName": option.UserPrincipalName || option.DisplayName,
                        }

                        return returnValue[key];
                    });
                    for (let value of filterOptionValues) {
                        filterParam.ColumnValues.push(...value.split(','));
                    } 
                }

                if (filterOptionValues.length > 0) {
                    this.filterData.Filters.push(filterParam);
                }
            }
            this.setState({ filterOptionsInfo: filterOptionsInfo });
            this.initData(true);
        };
        this.dispatch("jmFilterForm", callback);
        this.setState({ showFilterPanel: false });
    }

    exportRadioChange(value) {
        let isExportToBrowser = this.state.isExportToBrowser;
        this.setState({ isExportToBrowser: !isExportToBrowser });
        if (value == '0') {
            this.setState({ noDownLoadToValue: false });
        }
    }

    exportLocationChange(args) {
        let exportLocation = args.newValue;
        this.setState({
            selectedExportLocation: exportLocation,
            noDownLoadToValue: false,
            exportLocationNotFound: false
        });
    }

	priorityValueChange(value) {
        this.setState({priorityValue: value?.newValue?.value});
    }

    didHide() {
        this.setState({
            exportSettingShow: false,
            noDownLoadToValue: false,
            exportLocationNotFoundContent: false,
        });
    }

    onSaveSetting() {
        let setting = { ExportSetting: this.state.isExportToBrowser ? 0 : 1 };
        if (!this.state.isExportToBrowser) {
            if (this.state.exportLocationNotFound) {
                return false;
            }
            if (!this.state.selectedExportLocation.ID || this.state.selectedExportLocation.ID == EmptyGUID) {
                this.setState({
                    noDownLoadToValue: true,
                });
                return false;
            } else {
                setting.ExportLocationId = this.state.selectedExportLocation.ID;
                setting.LocationName = this.state.selectedExportLocation.Name;
                this.setState({noDownLoadToValue: false});
            }
        }
        $$.loading(true);
        let urlData = "/api/JMApi/SaveJobExportSetting";
        let option = {
            url: urlData,
            method: "POST",
            data: setting
        };
        fetchUtility(option).then((data) => {           
            if (data.MessageType === 0) {
                this.exportSetting = setting.ExportSetting;
            } else {
                this.showMsgToast(data.ErrorMessage, 'error', true);
            }
            $$.loading(false);
            $$.messagedialog(false);

            this.setState({exportSettingShow: false});
        }).catch((e) => {
            $$.loading(false);
        });
    }

	onSavePriority() {
		if (!$$.verify('#combobox-value')) {
			return;
		}
		let setting = { 
			JobIds: this.selectedEditPriority,
			JobPriority: this.state.priorityValue
		};
        $$.loading(true);
        let urlData = "/api/JMApi/UpdateJobMonitorPriority";
        let option = {
            url: urlData,
            method: "POST",
            data: setting
		};
        fetchUtility(option).then((res) => {           
			
            $$.loading(false);
            $$.messagedialog(false);

			this.setState({ editPriorityShow: false });
			if (res) {
				this.initData(true);
                this.showMsgToast(RMResx.RM_JS_JM_SavePrioritySuccess, 'success');
            } else {
                this.showMsgToast(RMResx.RM_JS_JM_SavePriorityFailed, 'error');
            }
        }).catch((e) => {
            $$.loading(false);
        }).finally(() => {
            $$.loading(false);
			this.state.priorityValue = 0;
		});
    }

    onCancelSetting() {
        this.setState({ exportSettingShow: false });
    }

    onSearchStart(args) {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            this.filterData.SearchValue = searchValue;
            this.initData(true);
        } else {
            this.filterData.SearchValue = '';
            this.initData(false);
        }
    }

    managedColumnChanged(args) {
        let managedColumnIds = args.newValue.map((item) => { return item.Id; });
        this.setTableColumnByManagedColumns(managedColumnIds);
        RM.setSessionStorage(this.props.manageColumnCacheName, managedColumnIds);
    }

    setTableColumnByManagedColumns(managedColumnIds){
        let allColumn = RM.deepcopy(this.getColumns());
        allColumn.map((item, index) => { item.visible = managedColumnIds.includes(item.id - 1); });
        this.setState({ allColumns: allColumn });
        this.dispatch("JobMonitorTable", { columns: allColumn });
    }

    getDefaultPager() {
        let param = {
            PageSize: 10,
            JumpPage: 1,
            CurrentPage: 0,
            IsSort: true,
            IsDesc: true,
            SortBy: 'StartTime',
            SearcheKeys: ["Id"],
            Filters: this.cacheFilterData || [],
            SearchValue: '',
        };
        return param;
    }

    initData(isResetPagerIndex) {
        $$.loading(true);
        //将PagerIndex设为0
        if (isResetPagerIndex) {
            this.filterData.JumpPage = 1;
            this.filterData.CurrentPage = 0;
            this.setState({jobsPagerIndex: 0});
        }
        let urlData = this.props.queryPagerUrl;
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData
        };
        fetchUtility(option).then((res) => {
            //刷新列表
            let data = JSON.parse(res);
            if (data.Result) {
                data.Result.forEach(item => {
                    item.EnableJobIdColLink = this.props.enableJobIdColLink;
                });
                this.setState({
                    items: data.Result,
                    jobsCount: data.TotalNumber,
                    exportSettingShow: false    //点X关掉Dialog, 没有更新State, 再次SetState就会导致Dialog出现.
                });
            }
            this.dispatch("JobMonitorTable", { columns: this.state.allColumns, items: data.Result, isReset: isResetPagerIndex });
            RM.setSessionStorage(`${this.props.filterCacheNamePrefix}JMFilteData`, this.filterData.Filters);
            if (this.filterData.Filters.length > 0) {
                this.setState({ isFiltered: true });
            } else {
                this.setState({ isFiltered: false });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onPagerChange(pagerIndex, pagerSize, callback) {
        this.filterData.CurrentPage = JSON.parse(JSON.stringify(this.filterData.JumpPage));
        this.filterData.JumpPage = pagerIndex + 1;
        this.filterData.PageSize = pagerSize;
        this.setState({
            jobsPagerIndex: pagerIndex,
            jobsPagerSize: pagerSize
        });
        this.initData(false);
        callback(true);
    }

    selectChange(items) {
        let isHasManualJob = false;
        let hasDownloadReportJob = false;
        let stopDis = true;
        let downloadDis = true;
        let deleteDis = true;
        let showreportDis = true;
        let exportSettingDis = false;
        let refreshDis = false;
        let priorityDis = true;
        let isFsJPMCJob = false;
        let statusCode = JMConstants.StatusCode;
        this.selectedFinished = [];
        this.selectedNeedStopped = [];
        this.selectedEditPriority = [];
        if (!checkPermission("JM_DownloadSettings", RM.UserResources)) {
            exportSettingDis = true;
        }
        for (let item of items) {
            if (item.Status != statusCode.Wait && item.Status != statusCode.InProgerss && item.Status != statusCode.Stopping && item.Status != statusCode.Pending) {
				this.selectedFinished.push(item.jobId);
            }
            if (item.Status == statusCode.Wait || item.Status == statusCode.InProgerss) {
				this.selectedNeedStopped.push(item.jobId);
				this.selectedEditPriority.push(item.JobId);
            }
            // if (item.JobTypeCode == 15 || item.JobTypeCode == 20 || item.JobTypeCode == 40 || item.JobTypeCode == 41 ||
            //     item.JobTypeCode == 30 || item.JobTypeCode == 50 || item.JobTypeCode == 51 || item.JobTypeCode == 52 ||
            //     item.JobTypeCode == 53 || item.JobTypeCode == 501 || item.JobTypeCode == 2001 || item.JobTypeCode == 2153 ||
            //     item.JobTypeCode == 3000 || item.JobTypeCode == 3001 || item.JobTypeCode == 4000) {
            //     isHasManualJob = true;
            // }
            //let canNotStopJob = [12, 15, 20, 33, 40, 41, 30, 50, 51, 52, 81, 101, 102, 200, 1020, 1021, 2001, 2153, 3000, 3001, 4000, 5000, 5001, 5002, 5003, 5004, 5006, 5010, 5100, 5200, 5201, 5500, 5503, 5504, 5505, 5506, 5515, 5507, 5516, 1006, 5510, 5511, 4107, 4108, 5512, 7000, 7001, 8000, 8019, 4110, 4111, 4112, 4113, 6109, 6110, 6111, 8100, 8101, 8102, 8103, 8026, 8029];
            let canNotStopJob = [
                12, 15, 20, 33, 30, 50, 51, 52, 81, 101, 102, 103, 200, 1020, 1021, 1022, 3000, 3001, 4000, 4200, 5000, 5001,
                5002, 5003, 5004, 5006, 5010, 5100, 5200, 5201, 5500, 5503, 5504, 5505, 5506, 5515, 5507, 5516, 1006, 5510, 5511,
                4107, 4108, 5512, 8000, 8019, 4110, 4111, 4112, 4113, 6000, 6106, 6109, 6110, 6111, 6112, 8100, 8101, 8102, 8103,
                8104, 8026, 8029, 8040, 8044, 8059, 8060, 10010, 10005, 10006, 10007, 10009, 10011, 10014, 100012, 8039, 10013,
                10019, 10310, 10332, 10333, 10334, 10016, 11111, 5202, 11203, 1027, 4116, 4117, 4118
            ];
            let fsJPMCJob = new Set([1027, 1028, 5000, 5001, 5002, 5003, 5202, 5203]);
            if (isEnableJPMCFeature) {
                canNotStopJob = canNotStopJob.filter(jobType => !fsJPMCJob.has(jobType))
            }
            isHasManualJob = canNotStopJob.indexOf(item.JobTypeCode) > -1;
            if (item.JobTypeCode == 30) {
                hasDownloadReportJob = true;
            }
        }
        if (this.selectedFinished.length > 0 && this.selectedFinished.length == items.length) {
            deleteDis = false;
            if (hasDownloadReportJob) {
                downloadDis = true;
            } else {
                downloadDis = false;
            }
            stopDis = true;
        } else {
            deleteDis = true;
            downloadDis = true;
            stopDis = false;
        }
        if (this.selectedNeedStopped.length > 0 && this.selectedNeedStopped.length == items.length && !isHasManualJob) {
            stopDis = false;
        } else {
            stopDis = true;
        }
        if (items.length == 1) {
            const tempReportTypeList = new Set([1, 2, 6, 13, 14, 19, 5004, 5006, 5010, 5011, 5012,
                2100, 2101, 2102, 2103, 2104, 4100, 4101, 4102, 4103, 4104, 6100, 6101, 6102,
                6103, 6104, 5510, 5511, 5512, 5513, 5514, 8000, 8019, 21, 6113, 10102, 10103,
                10104, 10105, 10106, 10205, 10208, 10209, 10210, 10211, 10305, 10306, 10307, 10308, 10309, 10310, 10311, 10214]);

            if (items[0]) {
                let tempReportType = items[0].JobTypeCode;
                let tempJobStatus = items[0].Status;
                if (tempReportTypeList.has(tempReportType) && tempJobStatus != statusCode.Wait && tempJobStatus != statusCode.InProgerss) {
                    showreportDis = false;
                }
            }
        } else {
            showreportDis = true;
		}
		if (this.selectedEditPriority.length > 0 && this.selectedEditPriority.length == items.length) {
			priorityDis = false;
		} else {
			priorityDis = true;
		}
		
        this.setState({
            showActions: {
                showExportSettings: !exportSettingDis,
                showPriority: !priorityDis,
                showRefresh: !refreshDis,
                showReport: !showreportDis,
                showDownload: !downloadDis,
                showDelete: !deleteDis,
                showStop: !stopDis,
            },
            jobsChecked: items
        },()=>{
            let showButtons = this.getShowActions();
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    getShowActions() {
        let buttonsInfo = [];
        this.props.buttonNames.forEach(buttonName => {
            buttonsInfo.push(this.getButtonsInfo()[buttonName])
        });
        let showButtons = buttonsInfo.filter((item) => { return item.isShow; });
        return showButtons;
    }

    renderJMHeader() {
        return <div className="ra-main-header">
            <div>
                {this.props.showSearchbox && <R.Searchbox
                    placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                    disabled={false}
                    onSearch={this.onSearchStart}
                    width={380}
                />}
            </div>
            <div className="flex" style={{ columnGap: "8px" }}>
                <R.Button
                    className="filtered-button"
                    icon="fia-filter"
                    primary={this.state.isFiltered}
                    classify={this.state.isFiltered ? "theme" : "default"}
                    text={this.state.isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_Common_Filter}
                    onClick={this.openFilterPanel} />
                <R.Multicombobox
                    checkedField="isChecked"
                    textField="value"
                    valueField="Id"
                    hasFilter={false}
                    required={true}
                    hasSelectAll={true}
                    clearable={true}
                    customTrigger={true}
                    items={this.state.ManagedColumns}
                    noneText={RMResx.RM_JS_JM_CustomColumns}
                    allText={RMResx.RM_JS_JM_CustomColumns}
                    selectedItemsTemplate={RMResx.RM_JS_JM_CustomColumns}
                    selectedItemTemplate={RMResx.RM_JS_JM_CustomColumns}
                    disabledField='isDynamic'
                    onChange={this.managedColumnChanged}
                    triggerBySource={true}
                >
                    <R.Button icon="fia-manage-column" text={RMResx.RM_JS_JM_CustomColumns} tooltip={RMResx.RM_JS_JM_CustomColumns} />
                </R.Multicombobox>
            </div>
        </div>;
    }

    renderNavBar() {
        let selectJobItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.jobsChecked.length, this.state.jobsCount);
        return < div className="ra-main-navbar">
            <div className="flex">
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: this.getShowActions()}}
                    showCount={4}
                ></TopButtonsComponent>
            </div>
            <div className="ra-main-selected-counter">{selectJobItemsCount}</div>
        </div >;
    }

    renderTable() {
        return <div className="ra-main-table">
            <JMTable
                id="JobMonitorTable"
                template={JobMounitorTemplate}
                uniqueKey={"JobId"}
                checkable={true}
                onChange={this.selectChange}
                cellClick={this.viewDetail}
                onSort={this.onSort}
            />
        </div>;
    }

    renderFooter(){
        return <div className="ra-main-footer">
            <$g.Pager
                itemsCount={this.state.jobsCount}
                pagerIndex={this.state.jobsPagerIndex}
                pagerSize={this.state.jobsPagerSize}
                showPagerSize={true}
                showPagerCounter={true}
                pagerSizeOptions={[5, 10, 15]}
                onChange={this.onPagerChange} />
        </div>;
    }

    renderExportSettingForm() {
        let content = LicenseHelper.EnableRecordsArchiver() ?
            <$g.I18NProvider msg={RMResx.RM_Common_ExportLocationTipForNewUserWithSpecialStorage}>
                <a className="ra-link-a" href="/Root/CP/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
            </$g.I18NProvider> :
            RMResx.RM_Common_ExportLocationTipForOldUserWithSpecialStorage;
        return <div>
            <div className='strong'>{RMResx.RM_EL_RadioTitle}</div>
            <div role="radiogroup">
                <div>
                    <R.Radio
                        name="radio-browser"
                        text={RMResx.RM_EL_Radio_Browser}
                        value={this.exportTypes.Browser}
                        checked={this.state.isExportToBrowser}
                        onChange={this.exportRadioChange}
                    />
                </div>
                <div>
                    <R.Radio
                        name="radio-browser"
                        text={RMResx.RM_EL_Radio_Location}
                        value={this.exportTypes.Location}
                        checked={!this.state.isExportToBrowser}
                        onChange={this.exportRadioChange}
                    />
                    <$g.Popover>{content}</$g.Popover>
                </div>
            </div>
            <div className='jm-locations-combobox'>
                <R.Validation>
                    <R.Combobox
                        id="locationsCombobox"
                        textField='Name'
                        valueField='ID'
                        checkedField='Checked'
                        waterMark='Select a Location'
                        items={this.state.exportLocations}
                        width={"100%"}
                        searchable={false}
                        onChange={this.exportLocationChange}
                        disabled={this.state.isExportToBrowser}
                        triggerBySource={true}
                    />
                    <R.ValidationFaker valid={!this.state.noDownLoadToValue} of="#locationsCombobox" message={RMResx.RM_JS_SPS_ExportSettting_ConfigureExportLocation} />
                    <R.ValidationFaker valid={!this.state.exportLocationNotFound} of="#locationsCombobox" message={this.state.exportLocationNotFoundContent} />
                </R.Validation>
            </div>
        </div>;
    }

    renderExportSettingPanel() {
        return <R.Panel
            id="exportSettingContainer"
            header={RMResx.RM_JS_EL_ExportSettings}
            size={664}
            onHide={this.didHide}
            status={{ show: this.state.exportSettingShow }}
            destroy={true}
        >
            {this.renderExportSettingForm()}
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelSetting} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveSetting} />
            </>
        </R.Panel>;
	}
	
	renderEditPriorityForm() {
        const items = JMConstants.JobPriority;
        const labelWithPopover = (
            <span>
                {RMResx.RM_EL_EditPriorityTitle}
                <$g.Popover>{RMResx.RM_JS_JM_Priority_Tooltip}</$g.Popover>
            </span>
        );
        return <div>
			<$g.FormRow label={labelWithPopover}>
				<R.Validation id="combobox-value">
					<R.Validation
						element="Combobox"
						require={RMResx.RM_JS_JM_Priority_ErrorMsg}
					>
						<R.Combobox
							id="priorityCombobox"
							textField='name'
							valueField='value'
							checkedField='checked'
							waterMark='Select a Location'
							items={items}
							width={"100%"}
							searchable={false}
							onChange={this.priorityValueChange}
							triggerBySource={true}
							aria="tooltip_demo_labelledby"
						/>
					</R.Validation>
				</R.Validation>
			</$g.FormRow>
        </div>;
    }

	renderEditPriorityPanel() {
        return <R.Panel
            id="editpriorityContainer"
            header={RMResx.RM_JS_EL_EditPriority}
            size={664}
            onHide={this.hidePriorityPanel}
            status={{ show: this.state.editPriorityShow }}
            destroy={true}
        >
            {this.renderEditPriorityForm()}
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hidePriorityPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSavePriority} />
            </>
        </R.Panel>;
    }

    renderFilterPanel() {
        return <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            onHide={this.hideFilterPanel}
            status={{ show: this.state.showFilterPanel }}
            destroy={true}
        >
            <JobMonitorFilterForm
                id="jmFilterForm"
                jobTypeOptions={this.state.jobTypeOptions}
                statusOptions={this.getStatusOptions()}
                filterOptionsInfo={this.state.filterOptionsInfo}
                filterCacheNamePrefix={this.props.filterCacheNamePrefix}
            ></JobMonitorFilterForm>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hideFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    render() {
        return <section>
            {this.renderJMHeader()}
            {this.renderNavBar()}
            {this.renderTable()}
            {this.renderFooter()}
            {this.renderFilterPanel()}
            {this.renderExportSettingPanel()}
            {this.renderEditPriorityPanel()}
            <div id='downloadDiv' style={{ display: "none" }} />
        </section>;
    }

}
export default withRouter(JobMonitor);

