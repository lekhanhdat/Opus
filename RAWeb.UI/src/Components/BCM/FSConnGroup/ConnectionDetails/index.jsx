import { withRouter } from "react-router";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import FSConnectionDetailsTable from "./Table";
import TopButtonsComponent from "../../../Common/Util/TopButtonsComponent";
import "./index.less";
import ConnectionDetailsFilterForm from "./FilterForm";
import { cacheFilterDataType, filterCacheNamePrefix, FSAgentJobI18N, FSAgentJobTypes, FSJobStatusCode, FSJobStatusI18N, manageColumnCacheName } from "./Constants";
import RouterUrls from "../../../../Constants/RouterUrls";
import { showToast } from "../../../../Utilities/CommonUtil";
import { NodeLevel } from "../../../../Constants/DAEnums";

const defaultManagedColumns = [
    { isChecked: true, value: RMResx.RM_JS_JM_JobID, Id: 0, isDynamic: true },
    { isChecked: true, value: RMResx.RM_JS_JM_Module, Id: 1, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_GroupName, Id: 2, isDynamic: false },
    { isChecked: true, value: RMResx.RM_FS_Register_Path, Id: 3, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_Status, Id: 4, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_StartTime, Id: 5, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_EndTime, Id: 6, isDynamic: false },
    { isChecked: true, value: RMResx.RM_JS_JM_UserName, Id: 7, isDynamic: false }
];

const findConnectionNode = (node) => {
    if (!node) return null;
    if (node.Level === NodeLevel.SiteCollection) return node;
    return node.Parent ? findConnectionNode(node.Parent) : null;
}
class FSConnectionDetails extends R.Component {
    constructor(props) {
        super(props);
        const { location } = props;
        const currentNode = location?.state;
        let connectionNode = null;
        let path = null;
        this.isFolderNode = false;
        if (currentNode) {
            currentNode.Level = currentNode.Level ?? NodeLevel.SiteCollection;
            const isConnectionNode = currentNode.Level === NodeLevel.SiteCollection;
            this.isFolderNode = !isConnectionNode;
            connectionNode = isConnectionNode ? currentNode : findConnectionNode(currentNode);
            path = currentNode.FullPath ?? currentNode.UNCPath;
        }
        const previousNodePath = RM.getSessionStorage('FSConnectionDetails_FolderPath');
        const connectionId = connectionNode?.Id ?? RM.getSessionStorage('FSConnectionDetails_ConnectionId');
        const isNewNode = path !== previousNodePath;
        const cacheFilterData = isNewNode ? [] : RM.getSessionStorage(`${filterCacheNamePrefix}_FSFilterData`);

        this.cacheManagedColumnsIds = RM.getSessionStorage(manageColumnCacheName);
        this.folderPath = path ?? previousNodePath;
        this.state = {
            jobsCount: 0,
            jobsPagerIndex: 0,
            jobsPagerSize: 10,
            jobsChecked: [],
            isFiltered: false,
            showFilterPanel: false,
            filterOptionsInfo: {},
            pathOptions: [],
            connectionGroupOptions: [],
            items: [],
            allColumns: this.getColumns(),
            managedColumns: this.getCacheManagedColumns(),
            disabledPathFilter: location?.state?.Level !== NodeLevel.SiteCollection
        };
        this.filterData = {
            ConnectionId: connectionId,
            PageSize: 10,
            PageIndex: 0,
            SearchKey: "",
            Filters: cacheFilterData || [],
            Order: {
                ColumnName: 'StartTime',
                IsDesc: true
            }
        }
        this.menuBtnItems = [
            { name: RMResx.RM_JS_JM_Refresh_Btn, id: "raCrmRefreshBtn", icon: "fia-refresh", onClick: () => this.initData(true), isShown: true },
            { name: RMResx.RM_JS_JM_Download_Btn, id: "raCrmDownloadReportBtn", icon: "fia-download", onClick: () => this.downloadClick(), isShown: false },
        ];
        this.exportSettingType = 0;
        this.setPathOptions();
        this.setConnectionGroupOptions();
        this.initBinding();
    }

    initBinding() {
        const eventsArr = ["onPagerChange", "selectChange", "managedColumnChanged", "onSearchStart",
            "initData", "hideFilterPanel", "onFilter", "openFilterPanel", "onCellClick", "onSort"
        ];
        eventsArr.forEach((ev) => {
            this[ev] = this[ev].bind(this);
        });
    }

    componentInit() {
        if (this.props.location?.state) {
            const currentNode = this.props.location.state;
            currentNode.Level = currentNode.Level ?? NodeLevel.SiteCollection;
            const isConnectionNode = currentNode.Level === NodeLevel.SiteCollection;
            const connectionNode = isConnectionNode ? currentNode : findConnectionNode(currentNode);
            if (!isConnectionNode) {
                this.isFolderNode = true;
                this.filterData.Filters = [];
                this.filterData.Filters.push({ ColumnName: 'FolderPath', ColumnValues: [currentNode.FullPath] });
            }
            RM.setSessionStorage('FSConnectionDetails_ConnectionId', connectionNode?.Id);
            RM.setSessionStorage('FSConnectionDetails_FolderPath', currentNode.FullPath ?? currentNode.UNCPath);
        }
        this.initData(true);
        this.loadExportSetting();
        if(this.cacheManagedColumnsIds) {
            this.setTableColumnByManagedColumns(this.cacheManagedColumnsIds);
        };
    }

    getCacheManagedColumns(){
        let managedColumns = RM.deepcopy(defaultManagedColumns);

        if(this.cacheManagedColumnsIds){
            managedColumns = managedColumns.map((item)=>{
                item.isChecked = this.cacheManagedColumnsIds.includes(item.Id);
                return item;
            });
        }
        return managedColumns; 
    }

    initData(isResetPagerIndex) {
        $$.loading(true);
        if (isResetPagerIndex) {
            this.filterData.PageIndex = 0;
            this.setState({jobsPagerIndex: 0});
        }
        let urlData = "/api/FSConnectionMonitorApi/QueryConnectionMonitorByPager";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData
        };
        fetchUtility(option).then((res) => {
            if (res.ConnectionMonitorList) {
                this.setState({
                    items: res.ConnectionMonitorList,
                    jobsCount: res.TotalCount
                });
            }
            this.dispatch("fsConnectionDetailsTable", { items: res.ConnectionMonitorList, isReset: isResetPagerIndex });
            RM.setSessionStorage(`${filterCacheNamePrefix}_FSFilterData`, this.filterData.Filters);
            if (this.filterData.Filters.length > 0) {
                this.setState({ isFiltered: true });
            } else {
                this.setState({ isFiltered: false });
            }
            $$.loading(false);
        }).catch((e) => {
            console.error("Error fetching data:", e);
            $$.loading(false);
        });
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
                this.exportSettingType = data;
            }
        }).catch((e) => {
            console.error("Error fetching export setting:", e);
        });
    }

    downloadClick() {
        let jobsChecked = this.state.jobsChecked;
        if (jobsChecked && jobsChecked.length > 15) {
            this.getDownloadReportLimit();
            return;
        }
        $$.loading(true);
        let jobIds = [];
        jobsChecked.forEach((item) => {
            jobIds.push(item.JobId);
        });

        if (this.exportSettingType == 0) {
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
            let option = {
                url: "/api/JMApi/StartJobExport",
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

    setPathOptions() {
        let urlData = "/api/FSConnectionMonitorApi/QueryAllConnectionPathRelatedJob";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData.ConnectionId
        };
        fetchUtility(option).then((res) => {
            let checkedAll = true;
            const paths = [...res];
            if (this.isFolderNode && this.folderPath) { 
                if (!paths.includes(this.folderPath)) {
                    paths.push(this.folderPath);
                }
                paths.push('_'); // to prevent dropdown showing "All" option when there is only one path which is the same as folderPath
                checkedAll = false;
            }
            let pathOptions = [];

            paths.forEach((item) => {
                let optionItem = {};
                optionItem.id = item;
                optionItem.value = item;
                optionItem.isChecked = checkedAll || item === this.folderPath;
                pathOptions.push(optionItem);
            });
            this.setState({
                pathOptions: pathOptions,
                disabledPathFilter: !checkedAll
            });
        });
    }

    setConnectionGroupOptions() {
        let urlData = "/api/FSConnectionMonitorApi/QueryAllConnectionGroupByRelatedJob";
        let option = {
            url: urlData,
            method: "POST",
            data: this.filterData.ConnectionId
        };
        fetchUtility(option).then((res) => {;
            let connectionGroupOptions = [];
            res.forEach((item) => {
                let optionItem = {};
                optionItem.id = item;
                optionItem.value = item;
                optionItem.isChecked = true;
                connectionGroupOptions.push(optionItem);
            });
            connectionGroupOptions.sort((prevOption, nextOption) => prevOption.value.localeCompare(nextOption.value));
            this.setState({
                connectionGroupOptions: connectionGroupOptions
            });
        });
    }

    getColumns() {
        let columns = [
            {   id: 1,
                header: RMResx.RM_JS_JM_JobID,
                width: [240],
                resizeable: true,
                sortable: true,
                valuePath: "JobId",
            },
            {   id: 2,
                header: RMResx.RM_JS_JM_Module,
                width: [300],
                resizeable: true,
                sortable: true,
                valuePath: "JobType",
            },
            {
                id: 3,
                header: RMResx.RM_FS_Register_GroupName,
                width: 250,
                resizeable: true,
                sortable: true,
                valuePath: "ConnectionGroupName",
            },
            {
                id: 4,
                header: RMResx.RM_FS_Register_Path,
                width: 350,
                resizeable: true,
                sortable: true,
                valuePath: "Path",
            },
            {
                id: 5,
                header: RMResx.RM_JS_JM_Status,
                resizeable: true,
                width: [200],
                sortable: true,
                valuePath: "Status",
			},
            {
                id: 6,
                header: RMResx.RM_JS_JM_StartTime,
                resizeable: true,
                width: [300],
                sortable: true,
                valuePath: "StartTime",
            },
            {
                id: 7,
                header: RMResx.RM_JS_JM_EndTime,
                resizeable: true,
                width: [300],    
                sortable: true,
                valuePath: "EndTime",   
            },
            {
                id: 8,
                header: RMResx.RM_JS_JM_UserName,
                resizeable: true,
                width: [250],
                sortable: true,
                valuePath: "JobRunBy",
            }
        ];
        return columns;
    }

    getStatusOptions() {
        const options = [];
        for (let key in FSJobStatusI18N) {
            if (key === FSJobStatusCode.FinishWithException.toString() || key === FSJobStatusCode.Failed.toString()) {
                options.push({ id: key, value: FSJobStatusI18N[key], isChecked: true });
            }
        }
        return options.sort((prevOption, nextOption) => prevOption.value.localeCompare(nextOption.value));
    }

    getJobTypeOptions() {
        const options = [];
        // Currently, only jobs related to data synchronization, apply class code and disposal are supported.
        const supportedJobTypes = [
            FSAgentJobTypes.FSDataSynchronization,
            FSAgentJobTypes.FSDataSynchronizationSchedule,
            FSAgentJobTypes.FSDisposal,
            FSAgentJobTypes.FSDisposalSchedule,
            FSAgentJobTypes.ApplyClassCode,
            FSAgentJobTypes.FSDisposalByClassCode,
        ];
        for (let key in FSAgentJobI18N) {
            if (supportedJobTypes.includes(parseInt(key))) {
                options.push({ id: key, value: FSAgentJobI18N[key], isChecked: true });
            }
        }
        const obj = {};
        const jobTypeOptions = options.reduce((current, next) => {
            if (obj[next.value]) {
                for (let item of current) {
                    if (next.value == item.value) {
                        item.id = item.id + ',' + next.id;
                    }
                }
            } else {
                obj[next.value] = true && current.push(next);
            }
            return current;
        }, []);
        return jobTypeOptions.sort((prevOption, nextOption) => prevOption.value.localeCompare(nextOption.value));
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

    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }

    onSearchStart(args) {
        let searchValue = args;
        if (searchValue && searchValue != "") {
            this.filterData.SearchKey = searchValue;
            this.initData(true);
        } else {
            this.filterData.SearchKey = '';
            this.initData(false);
        }
    }

    openFilterPanel() {
        this.setState({ showFilterPanel: true });
    }

    hideFilterPanel() {
        this.setState({ showFilterPanel: false });
    }

    onFilter() {
        let callback = (filterOptionsInfo) => {
            const existingFolderPathFilter = this.filterData.Filters.find(
                (f) => f.ColumnName === 'FolderPath'
            );
            this.filterData.Filters = [];
            const jobTypeOptions = this.getJobTypeOptions();
            const statusOptions = this.getStatusOptions();
            if (filterOptionsInfo) {
                if (filterOptionsInfo[cacheFilterDataType.jobType]
                    && filterOptionsInfo[cacheFilterDataType.jobType].every((item) => item.isChecked)
                    && filterOptionsInfo[cacheFilterDataType.jobType].length === jobTypeOptions.length
                ) {
                    delete filterOptionsInfo[cacheFilterDataType.jobType];
                }

                if (filterOptionsInfo[cacheFilterDataType.status]
                    && filterOptionsInfo[cacheFilterDataType.status].every((item) => item.isChecked)
                    && filterOptionsInfo[cacheFilterDataType.status].length === statusOptions.length
                ) {
                    delete filterOptionsInfo[cacheFilterDataType.status];
                }

                if (filterOptionsInfo[cacheFilterDataType.path]
                    && filterOptionsInfo[cacheFilterDataType.path].every((item) => item.isChecked)
                    && filterOptionsInfo[cacheFilterDataType.path].length === this.state.pathOptions.length
                ) {
                    delete filterOptionsInfo[cacheFilterDataType.path];
                }

                if (filterOptionsInfo[cacheFilterDataType.connectionGroup]
                    && filterOptionsInfo[cacheFilterDataType.connectionGroup].every((item) => item.isChecked)
                    && filterOptionsInfo[cacheFilterDataType.connectionGroup].length === this.state.connectionGroupOptions.length
                ) {
                    delete filterOptionsInfo[cacheFilterDataType.connectionGroup];
                }
            }

            for (let key in filterOptionsInfo) {
                let filterParam = { ColumnName: key, ColumnValues: [] };
                let filterOptions = filterOptionsInfo[key];
                let filterOptionValues;

                // Case for Start time and End time
                if (key == cacheFilterDataType.startTime || key == cacheFilterDataType.endTime) {
                    filterOptionValues = filterOptions.length ? filterOptions.map(item => item.Value) : [];
                    filterParam.ColumnValues = filterOptions.length ? [...filterOptionValues] : filterOptionValues;
                } else {
                    // For others case
                    filterOptionValues = filterOptions.filter((item) => item.isChecked || item.Checked).map((option) => {
                        const returnValue = {
                            [cacheFilterDataType.path]: option.id,
                            [cacheFilterDataType.jobType]: option.id,
                            [cacheFilterDataType.status]: option.id,
                            [cacheFilterDataType.connectionGroup]: option.id,
                            [cacheFilterDataType.jobRunBy]: option.UserPrincipalName || option.DisplayName,
                        }

                        return returnValue[key];
                    });
                    for (let value of filterOptionValues) {
                        if (value && value.includes(',')) { 
                            filterParam.ColumnValues.push(...value.split(','));
                        } else if (value) {
                            filterParam.ColumnValues.push(value);
                        }
                    } 
                }

                if (filterOptionValues.length > 0) {
                    this.filterData.Filters.push(filterParam);
                }
            }

            if (existingFolderPathFilter) {
                this.filterData.Filters.push(existingFolderPathFilter);
            }
            this.setState({ filterOptionsInfo: filterOptionsInfo });
            this.initData(true);
        };
        this.dispatch("connectionDetailsFilterForm", callback);
        this.setState({ showFilterPanel: false });
    }

    managedColumnChanged(args) {
        let managedColumnIds = args.newValue.map((item) => { return item.Id; });
        this.setTableColumnByManagedColumns(managedColumnIds);
        RM.setSessionStorage(manageColumnCacheName, managedColumnIds);
    }

    setTableColumnByManagedColumns(managedColumnIds){
        let allColumn = RM.deepcopy(this.getColumns());
        allColumn.map((item) => { item.visible = managedColumnIds.includes(item.id - 1); });
        this.setState({ allColumns: allColumn });
    }

    selectChange(items) {
        this.setState({
            jobsChecked: items
        }, () => {
            let showButtons = this.menuBtnItems.map((item) => {
                if (item.id === "raCrmDownloadReportBtn") {
                    item.isShown = items.length > 0;
                }
                return item;
            });
            showButtons = showButtons.filter(item => item.isShown);
            this.refTopButtons.updateButtons(showButtons);
        });
    }

    onSort = (isAsc, columnName) => {
        this.filterData.Order.IsDesc = !isAsc;
        this.filterData.Order.ColumnName = columnName;
        this.initData(true);
    }

    onCellClick(args) {
        let jobId = args.JobId;
        let jobType = args.JobType;
        this.props.history.push({
            pathname: RouterUrls.BCM_FSConnection_JobDetails,
            state: { jobId, jobType }
        });
    }

    onPagerChange(pagerIndex, pagerSize, callback) {
        this.filterData.PageIndex = pagerIndex;
        this.filterData.PageSize = pagerSize;
        this.setState({
            jobsPagerIndex: pagerIndex,
            jobsPagerSize: pagerSize
        });
        this.initData(false);
        callback(true);
    }

    renderHeader() {
        return <div className="ra-main-header">
            <div>
                <R.Searchbox
                    placeholder={RMResx.RM_JS_JM_SearchKeyWord}
                    disabled={false}
                    onSearch={this.onSearchStart}
                    width={380}
                />
            </div>
            <div className="flex" style={{ columnGap: "8px" }}>
                <R.Button
                    className="filtered-button"
                    icon="fia-filter"
                    primary={this.state.isFiltered}
                    classify={this.state.isFiltered ? "theme" : "default"}
                    text={this.state.isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_Common_Filter}
                    onClick={this.openFilterPanel}
                />
                <R.Multicombobox
                    checkedField="isChecked"
                    textField="value"
                    valueField="Id"
                    hasFilter={false}
                    required={true}
                    hasSelectAll={true}
                    clearable={true}
                    customTrigger={true}
                    items={this.state.managedColumns}
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

    renderToolBar() {
        let selectJobItemsCount = RMResx.RM_Common_SelectTableItemsCounter.format(this.state.jobsChecked.length, this.state.jobsCount);
        const menuBtnItems = this.menuBtnItems.filter(item => item.isShown);
        return <div className="ra-main-navbar">
            <div className="flex">
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...menuBtnItems] }}
                />
            </div>
            <div className="ra-main-selected-counter">{selectJobItemsCount}</div>
        </div>;
    }

    renderTable() {
        return (
            <div className="ra-main-table">
                <FSConnectionDetailsTable
                    id="fsConnectionDetailsTable"
                    uniqueKey={"JobId"}
                    checkable={true}
                    columns={this.state.allColumns}
                    onChange={this.selectChange}
                    cellClick={this.onCellClick}
                    onSort={this.onSort}
                />
            </div>
        );
    }
    
    renderFooter(){
        return (
            <div className="ra-main-footer">
                <$g.Pager
                    itemsCount={this.state.jobsCount}
                    pagerIndex={this.state.jobsPagerIndex}
                    pagerSize={this.state.jobsPagerSize}
                    showPagerSize={true}
                    showPagerCounter={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.onPagerChange}
                />
            </div>
        );
    }

    renderFilterPanel() {
        return <R.Panel
            header={RMResx.RM_Common_Filter}
            size={664}
            onHide={this.hideFilterPanel}
            status={{ show: this.state.showFilterPanel }}
            destroy={true}
        >
            <ConnectionDetailsFilterForm
                id="connectionDetailsFilterForm"
                filterOptionsInfo={this.state.filterOptionsInfo}
                jobTypeOptions={this.getJobTypeOptions()}
                statusOptions={this.getStatusOptions()}
                pathOptions={this.state.pathOptions}
                connectionGroupOptions={this.state.connectionGroupOptions}
                disabledPathFilter={this.state.disabledPathFilter}
            />
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.hideFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    render() {
        return (
            <>
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_FS, SiteMapLinks.BCM_FSConnGroup, SiteMapLinks.BCM_FSConnection_JobMonitor]} />
                <div className="ra-page-container" id="fsJobMonitor">
                    {this.renderHeader()}
                    {this.renderToolBar()}
                    {this.renderTable()}
                    {this.renderFooter()}
                    {this.renderFilterPanel()}
                </div>
            </>
        )
    }

}

export default FSConnectionDetails;