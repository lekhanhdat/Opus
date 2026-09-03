import { CollectionTimeFilter } from "../MT/MachineLearningReview/Filters";
import * as JMConstants from "./JMConstants";
import _ from 'lodash'
import { FilterOptions } from "../MT/MachineLearningReview/Constants";
import PeoplePicker from "../Common/PeoplePicker";
import { getMulticomboboxAllItems } from "../../Utilities/CommonUtil";
export class JobMonitorFilterForm extends R.Component {
    
    idAttr = true;
    constructor(props) {
        super(props);
        this.cacheFilteData = sessionStorage.getItem(`${this.props.filterCacheNamePrefix}JMFilteData`) ? JSON.parse(sessionStorage.getItem(`${this.props.filterCacheNamePrefix}JMFilteData`)) : null;
        this.cacheFilteDataObj = this.getCacheFilteDataObj();
        this.filterOptionsInfo = RM.deepcopy(this.props.filterOptionsInfo);
        this.filterDefinitions = new Map();
        this.state = {
            jobTypeOptions: this.props.filterOptionsInfo.JobType || this.getCacheFilterJobType(),
            statusOptions: this.props.filterOptionsInfo.Status || this.getCacheFilterStatus(),
            startTimeDefinitions: this.getCacheFilterTimes("StartTime"),
            endTimeDefinitions: this.getCacheFilterTimes("EndTime"),
            jobRunByOptions: this.props.filterOptionsInfo.UserName || this.getCacheFilterJobRunBy(),
        };
        this.addUserChanged = [];
    }

    getCacheFilteDataObj(){
        let cacheFilteDataObj = {};
        for(let item of this.cacheFilteData){
            cacheFilteDataObj[item.ColumnName] = item.ColumnValues;
        }
        return cacheFilteDataObj;
    }

    getCacheFilterStatus (){
        let statusOptions = RM.deepcopy(this.props.statusOptions);
        if(this.cacheFilteDataObj.Status){
            for(let item of statusOptions){
                item.checked = this.cacheFilteDataObj.Status.includes(item.id);
            } 
            this.filterOptionsInfo.Status = statusOptions;
        }
        return statusOptions;
    }

    getCacheFilterJobType (){
        let jobTypeOptions = RM.deepcopy(this.props.jobTypeOptions);
        if(this.cacheFilteDataObj.JobType){
            for(let item of jobTypeOptions){
                for(let id of this.cacheFilteDataObj.JobType){
                    item.checked = item.id.split(",").includes(id);
                    if(item.checked){
                        break;
                    }
                }
            }
            this.filterOptionsInfo.JobType = jobTypeOptions;
        }
        return jobTypeOptions;
    }

    getCacheFilterTimes(type) {
        let cachedFilterTime = new Map();
        if (this.cacheFilteDataObj[type]) {
            const jmFilterTime = RM.getSessionStorage(`${this.props.filterCacheNamePrefix}JMFilter${type}`);
            cachedFilterTime.set(jmFilterTime.FilterOption, jmFilterTime);
            this.filterOptionsInfo[type] = Array.from(cachedFilterTime, ([_, value]) => value).filter(item => item.FilterOption == jmFilterTime.FilterOption);
        }

        return cachedFilterTime;
    }

    getCacheFilterJobRunBy() {
        if (this.cacheFilteDataObj.UserName) {
            const jmFilterJobRunBy = RM.getSessionStorage(`${this.props.filterCacheNamePrefix}JMFilterJobRunBy`);
            this.filterOptionsInfo.UserName = jmFilterJobRunBy;
            return jmFilterJobRunBy;
        }

        return [];
    }

    componentReceive(callback) {
        callback(this.filterOptionsInfo);
    }

    onChangeJobType = (args) => {
        this.filterOptionsInfo.JobType = getMulticomboboxAllItems(args.newValue, this.state.jobTypeOptions, "id", "checked");
    }

    onChangeStatus = (args) => {
        this.filterOptionsInfo.Status = getMulticomboboxAllItems(args.newValue, this.state.statusOptions, "id", "checked");
    }

    onClear = ()=> {
        // this.filterOptionsInfo.JobType = this.props.jobTypeOptions;
        // this.filterOptionsInfo.Status = this.props.statusOptions;
        const arr = ["JobType", "Status", "StartTime", "EndTime", "UserName"];
        this.filterDefinitions = new Map();
        arr.forEach(item => delete this.filterOptionsInfo[item])
        // this.filterOptionsInfo.StartTime = Array.from(new Map());
        // this.filterOptionsInfo.EndTime = Array.from(new Map());
        // this.filterOptionsInfo.UserName = [];
        this.setState({
            jobTypeOptions: RM.deepcopy(this.props.jobTypeOptions),
            statusOptions: RM.deepcopy(this.props.statusOptions),
            startTimeDefinitions: new Map(),
            endTimeDefinitions: new Map(),
            jobRunByOptions: [],
        });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    onFilterChange = (filterValue, type) => {
        this.filterDefinitions.set(filterValue.FilterOption, filterValue);
        const filterDefinitions = Array.from(this.filterDefinitions, ([_, value]) => value).filter(item => item.FilterOption == filterValue.FilterOption);

        this.filterOptionsInfo[type] = filterDefinitions;
        RM.setSessionStorage(`${this.props.filterCacheNamePrefix}JMFilter${type}`, filterValue);

        if (type == "StartTime") {
            this.setState({ startTimeDefinitions: this.filterDefinitions });
        } else {
            this.setState({ endTimeDefinitions: this.filterDefinitions });
        }
    };

    onRemovefilter = (filterValue, type) => {
        this.filterDefinitions.delete(filterValue);

        this.filterOptionsInfo[type] = [];
        // RM.setSessionStorage(`JMFilter${type}`, []);

        if (type == "StartTime") {
            this.setState({ startTimeDefinitions: _.cloneDeep(this.filterDefinitions) });
        } else {
            this.setState({ endTimeDefinitions: _.cloneDeep(this.filterDefinitions) });
        }
    };

    onAddUserSelectionChanged = (args) => {
        let selections = RM.deepcopy(args);
        this.filterOptionsInfo.UserName = selections;
        if (selections.length) {
            RM.setSessionStorage(`${this.props.filterCacheNamePrefix}JMFilterJobRunBy`, selections);
        }
        this.addUserChanged = selections;
        this.setState({
            jobRunByOptions: selections,
        });
    }

    render() {
        return <div id={this.props.id}>
            <div className="ra-flex-justify-end">
                <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClear} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
            </div>
            <$g.FormRow label={RMResx.RM_JS_JM_Module}>
                <R.Multicombobox
                    width={"100%"}
                    checkedField="checked"
                    textField="name"
                    valueField="id"
                    required={true}
                    // clearable={true}
                    items={this.state.jobTypeOptions}
                    onChange={this.onChangeJobType}
                    aria={{ ariaLabel: RMResx.RM_JS_JM_Module }}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_JM_Status}>
                <R.Multicombobox
                    width={"100%"}
                    checkedField="checked"
                    textField="name"
                    valueField="id"
                    required={true}
                    // clearable={true}
                    items={this.state.statusOptions}
                    onChange={this.onChangeStatus}
                    aria={{ ariaLabel: RMResx.RM_JS_JM_Status }}
                />
            </$g.FormRow>
            <CollectionTimeFilter
                filterOption={FilterOptions.StartTime}
                onFilterChange={(filterValue) => this.onFilterChange(filterValue, "StartTime")}
                onRemoveFilterChange={(filterValue) => this.onRemovefilter(filterValue, "StartTime")}
                filterDefinitions={this.state.startTimeDefinitions}
            />
            <CollectionTimeFilter
                filterOption={FilterOptions.EndTime}
                onFilterChange={(filterValue) => this.onFilterChange(filterValue, "EndTime")}
                onRemoveFilterChange={(filterValue) => this.onRemovefilter(filterValue, "EndTime")}
                filterDefinitions={this.state.endTimeDefinitions}
            />
            <$g.FormRow label={RMResx.RM_JM_JobRunBy} id="ariaJobRunBy">
                <PeoplePicker
                    width="100%"
                    isAllowCustomizeUser
                    items={this.state.jobRunByOptions}
                    selectionChanged={this.onAddUserSelectionChanged}
                    ariaId="ariaJobRunBy"
                />
            </$g.FormRow>
        </div>;
    }
}

const entityTypeColumns = [
    { isChecked: true, value: JMConstants.ArchiverEntityTypeMap[0], isDynamic: true, Id: '0' },
    { isChecked: true, value: JMConstants.ArchiverEntityTypeMap[1], Id: '4' },
    { isChecked: true, value: JMConstants.ArchiverEntityTypeMap[2], Id: '5' },
    { isChecked: true, value: JMConstants.ArchiverEntityTypeMap[3], Id: '6' }
];

const statusColumns =  [
    { isChecked: true, value: RMResx.RM_JS_JMD_Status_Successful, Id: 0 },
    { isChecked: true, value: RMResx.RM_JS_JMD_Status_Failed, Id: 1 },
    { isChecked: true, value: RMResx.RM_JS_JMD_Status_Exception, Id: 4 },
    { isChecked: true, value: RMResx.RM_JS_JMD_Status_Skipped, Id: 2 }
];

const statusColumnsForSOSubJob = [
    { isChecked: true, value: RMResx.RM_JS_JM_Status_Wait, Id: 0 },
    { isChecked: true, value: RMResx.RM_JS_JM_Status_InProgerss, Id: 1 },
    { isChecked: true, value: RMResx.RM_JS_JM_Status_Finished, Id: 2 },
    { isChecked: true, value: RMResx.RM_JS_JM_Status_Failed, Id: 3 },
    { isChecked: true, value: RMResx.RM_JS_JM_Status_FinishWithException, Id: 4 },
    { isChecked: true, value: RMResx.RM_JS_JM_Status_Stopped, Id: 5 },
]

const tabColumns = [
    { isChecked: true, value: RMResx.RM_JM_Tab_DetailFilter_Scan, Id: 0 },
    { isChecked: true, value: RMResx.RM_JM_Tab_DetailFilter_Export, Id: 1 },
    { isChecked: true, value: RMResx.RM_JM_Tab_DetailFilter_Backup, Id: 2 },
    { isChecked: true, value: RMResx.RM_JM_Tab_DetailFilter_Action, Id: 3 },
];

const detailTabColumns = [
    { isChecked: true, value: RMResx.SO_Action_Delete, Id: "SO_Action_Delete" },
    { isChecked: true, value: RMResx.SO_Action_LevelStub, Id: "SO_Action_LevelStub" },
];

const DISCOVERY_SO_FILTER_ACTIONS = {
  [JMConstants.JobType.DiscoveryPreScan]: [
    { isChecked: true, value: RMResx.RM_FA_DataOptimize_File_ArchiveAndRemove, Id: "RM_FA_DataOptimize_File_ArchiveAndRemove" },
    { isChecked: true, value: RMResx.RM_FA_DataOptimize_File_RemoveFile, Id: "RM_FA_DataOptimize_File_RemoveFile" },
    { isChecked: true, value: RMResx.RM_FA_DataOptimize_File_ArchiveFile, Id: "RM_FA_DataOptimize_File_ArchiveFile" },
    { isChecked: true, value: RMResx.RM_FA_DataOptimize_Version_ArchiveAndRemove, Id: "RM_FA_DataOptimize_Version_ArchiveAndRemove" },
    { isChecked: true, value: RMResx.RM_FA_DataOptimize_Version_RemoveVersion, Id: "RM_FA_DataOptimize_Version_RemoveVersion" },
  ]
};

export class JobDetailFilterForm extends R.Component {
    idAttr = true;
   
    constructor(props) {
        super(props);
        this.state = {
            entityTypeColumns: RM.deepcopy(entityTypeColumns),
            statusColumns: this.props.isSOSubJobFilter ? RM.deepcopy(statusColumnsForSOSubJob) : RM.deepcopy(statusColumns),
            tabColumns: RM.deepcopy(tabColumns),
            detailsActionColumns: this.getDetailsActionColumns(),
        };
        // this.filterOptionsInfo = RM.deepcopy(this.props.filterOptionsInfo);
    }

    resetFilterData(filterOptionsInfo) {
        this.filterOptionsInfo = RM.deepcopy(filterOptionsInfo);
        this.componentInit();
    }

    componentInit() {
        this.filterOptionsInfo = RM.deepcopy(this.props.filterOptionsInfo);
        let entityTypeIds = this.filterOptionsInfo.EntityTypeFilters;
        let statusIds = this.props.isSOSubJobFilter ? this.filterOptionsInfo.SubJobStatusFilters : this.filterOptionsInfo.StatusFilters;
        let tabIds = this.filterOptionsInfo.ActionTabFilters;
        const actionIds = this.filterOptionsInfo.ArchiverActionFilters;
        if (entityTypeIds) {
            for (let column of this.state.entityTypeColumns) {
                if (entityTypeIds.length == 0) {
                    column.isChecked = true;
                } else {
                    column.isChecked = entityTypeIds.includes(column.Id);
                }
            }
        }
        if (statusIds) {
            for (let column of this.state.statusColumns) {
                if (statusIds.length == 0) {
                    column.isChecked = true;
                } else {
                    column.isChecked = statusIds.includes(column.Id);
                }
            }
        }
        if (tabIds) {
            var tabColumnsList = RM.deepcopy(this.state.tabColumns);
            if (this.filterOptionsInfo.JobType === JMConstants.JobType.DiscoverOptimization) {
                tabColumnsList = tabColumnsList.filter(i => i.Id !== 1);
            }
            for (let column of tabColumnsList) {
                if (tabIds.length == 0) {
                    column.isChecked = true;
                } else {
                    column.isChecked = tabIds.includes(column.Id);
                }
            }
        }
        if (actionIds) {
            var detailsActionColumnList = this.getDetailsActionColumns();
            for (let column of detailsActionColumnList) {
                if (actionIds.length == 0) {
                    column.isChecked = true;
                } else {
                    column.isChecked = actionIds.includes(column.Id);
                }
            }
        }
        this.setState({
            entityTypeColumns: RM.deepcopy(this.state.entityTypeColumns),
            statusColumns: RM.deepcopy(this.state.statusColumns),
            tabColumns: tabColumnsList,
            detailsActionColumns: detailsActionColumnList,
        });
    }

    getFilterData() {
        return this.filterOptionsInfo;
    }
    
    getDetailsActionColumns() {
        if (this.filterOptionsInfo?.JobType === JMConstants.JobType.DiscoveryPreScan) {
            return DISCOVERY_SO_FILTER_ACTIONS[this.filterOptionsInfo?.JobType] ?? [];
        }

        const clonedDetailTabColumns = RM.deepcopy(detailTabColumns);
        const newColumns = [
            { isChecked: true, value: RMResx.SO_Action_Keep, Id: "SO_Action_Keep" },
            { isChecked: true, value: RMResx.SO_Action_Move, Id: "SO_Action_Move" },
            { isChecked: true, value: RMResx.SO_Action_ExportOnly, Id: "SO_Action_ExportOnly" },
        ];
        return [...clonedDetailTabColumns, ...newColumns];
    }

    entityTypeChanged = (args) => {
        let entityTypeIds = [];
        args.newValue.map((item, index) => { entityTypeIds.push(item.Id); });
        if (args.isSelectAll) {
            this.filterOptionsInfo.EntityTypeFilters = [];
        } else {
            this.filterOptionsInfo.EntityTypeFilters = entityTypeIds;
        }
    }

    statusFilterChanged = (args) => {
        let statusIds = [];
        args.newValue.map((item) => { statusIds.push(item.Id); });
        if (this.props.isSOSubJobFilter) { 
            this.filterOptionsInfo.SubJobStatusFilters = statusIds;
        } else {
            this.filterOptionsInfo.StatusFilters = statusIds;
        }
    }

    tabsFilterChanged = (args) => {
        let tabIds = [];
        args.newValue.map((item) => { tabIds.push(item.Id); });
        this.filterOptionsInfo.ActionTabFilters = tabIds;
    }

    detailsActionFilterChanged = (args) => {
        const actionIds = [];
        args.newValue.map((item) => actionIds.push(item.Id));
        if (actionIds.length === this.state.detailsActionColumns.length) {
            this.filterOptionsInfo.ArchiverActionFilters = [];
        } else {
            this.filterOptionsInfo.ArchiverActionFilters = actionIds;
        }
    }

    onClear = () => {
        this.setState({
            entityTypeColumns: RM.deepcopy(entityTypeColumns),
            statusColumns: this.props.isSOSubJobFilter ? RM.deepcopy(statusColumnsForSOSubJob) : RM.deepcopy(statusColumns),
            tabColumns: RM.deepcopy(tabColumns),
            detailsActionColumns: this.getDetailsActionColumns(),
        });
        this.filterOptionsInfo.EntityTypeFilters = [];
        this.filterOptionsInfo.StatusFilters = [];
        this.filterOptionsInfo.ActionTabFilters = [];
        this.filterOptionsInfo.ArchiverActionFilters = [];
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render() {
        if (this.props.isSOSubJobFilter) { 
            return (
                <div id={this.props.id}>
                    <div className="ra-flex-justify-end">
                        <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClear} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
                    </div>
                    <$g.FormRow label={RMResx.RM_JM_JD_FilterColumn_Status}>
                        <R.Multicombobox
                            checkedField="isChecked"
                            textField="value"
                            valueField="Id"
                            width={"100%"}
                            hasSelectAll={true}
                            searchable={false}
                            items={this.state.statusColumns}
                            noneText={RMResx.RM_JS_JMD_Status_Filter}
                            onChange={this.statusFilterChanged}
                        />
                    </$g.FormRow>
                </div>
            )
        };
        
        return <div>
            <div className="ra-flex-justify-end">
                <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClear} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
            </div>
            <$g.FormRow label={RMResx.RM_JM_JD_FilterColumn_Status}>
                <R.Multicombobox
                    checkedField="isChecked"
                    textField="value"
                    valueField="Id"
                    width={"100%"}
                    hasSelectAll={true}
                    searchable={false}
                    items={this.state.statusColumns}
                    noneText={RMResx.RM_JS_JMD_Status_Filter}
                    onChange={this.statusFilterChanged}
                />
            </$g.FormRow>
            {this.props.isShowTabsFilter &&
                <$g.FormRow label={RMResx.RM_JM_Tab_DetailFilter_TabTitle}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="Id"
                        width={"100%"}
                        hasSelectAll={true}
                        searchable={false}
                        items={this.state.tabColumns}
                        noneText={RMResx.RM_JM_Tab_DetailFilter_Placeholder}
                        onChange={this.tabsFilterChanged}
                    />
                </$g.FormRow>
            }
            {this.props.isShowDiscoveryPreScanFilter && (
                <$g.FormRow label={RMResx.RM_JM_Tab_DetailFilter_DetailsActionTitle}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="Id"
                        width={"100%"}
                        hasSelectAll={true}
                        searchable={false}
                        items={this.state.detailsActionColumns}
                        noneText={RMResx.RM_JM_Tab_DetailFilter_Placeholder}
                        onChange={this.detailsActionFilterChanged}
                    />
                </$g.FormRow>
            )}
        </div>;
    }
}