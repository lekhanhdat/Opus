import _ from "lodash";
import PeoplePicker from "../../../Common/PeoplePicker";
import { FilterOptions } from "../../../MT/MachineLearningReview/Constants";
import { CollectionTimeFilter } from "../../../MT/MachineLearningReview/Filters";
import { cacheFilterDataType, filterCacheNamePrefix } from "./Constants";
import { getMulticomboboxAllItems } from "../../../../Utilities/CommonUtil";

class ConnectionDetailsFilterForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        const { filterOptionsInfo } = this.props;

        this.cacheFilterData = sessionStorage.getItem(`${filterCacheNamePrefix}_FSFilterData`)
            ? JSON.parse(sessionStorage.getItem(`${filterCacheNamePrefix}_FSFilterData`)) : null;
        this.cacheFilterDataObj = this.getCachedFilterDataObj();
        this.filterOptionsInfo = RM.deepcopy(filterOptionsInfo);
        this.filterDefinitions = new Map();
        this.state = {
            pathOptions: filterOptionsInfo[cacheFilterDataType.path] || this.getCachedFilterDataByType(cacheFilterDataType.path),
            jobTypeOptions: filterOptionsInfo[cacheFilterDataType.jobType] || this.getCachedFilterDataByType(cacheFilterDataType.jobType),
            statusOptions: filterOptionsInfo[cacheFilterDataType.status] || this.getCachedFilterDataByType(cacheFilterDataType.status),
            connectionGroupOptions: filterOptionsInfo[cacheFilterDataType.connectionGroup] || this.getCachedFilterDataByType(cacheFilterDataType.connectionGroup),
            startTimeDefinitions: this.getCachedFilterDataByType(cacheFilterDataType.startTime),
            endTimeDefinitions: this.getCachedFilterDataByType(cacheFilterDataType.endTime),
            jobRunByOptions: filterOptionsInfo[cacheFilterDataType.jobRunBy] || this.getCachedFilterDataByType(cacheFilterDataType.jobRunBy),
        };
    }

    componentReceive(callback) {
        callback(this.filterOptionsInfo);
    }

    getCachedFilterDataObj(){
        let cacheFilterDataObj = {};
        if (this.cacheFilterData) {
            for(let item of this.cacheFilterData){
                cacheFilterDataObj[item.ColumnName] = item.ColumnValues;
            }
        }
        return cacheFilterDataObj;
    }

    getCachedFilterDataByType(type) {
        const optionObject = {
            [cacheFilterDataType.path]: RM.deepcopy(this.props.pathOptions),
            [cacheFilterDataType.jobType]: RM.deepcopy(this.props.jobTypeOptions),
            [cacheFilterDataType.status]: RM.deepcopy(this.props.statusOptions),
            [cacheFilterDataType.connectionGroup]: RM.deepcopy(this.props.connectionGroupOptions),
        };

        switch (type) { 
            case cacheFilterDataType.path:
            case cacheFilterDataType.status:
            case cacheFilterDataType.connectionGroup:
                const options = optionObject[type];
                if (this.cacheFilterDataObj[type]) {
                    for (let item of options) {
                        item.isChecked = this.cacheFilterDataObj[type].includes(item.id);
                    }
                    this.filterOptionsInfo[type] = options;
                }
                return options;
            case cacheFilterDataType.jobType:
                const jobTypeOptions = optionObject[type];
                if(this.cacheFilterDataObj.JobType){
                    for(let item of jobTypeOptions){
                        for(let id of this.cacheFilterDataObj.JobType){
                            item.isChecked = item.id.split(",").includes(id);
                            if(item.isChecked){
                                break;
                            }
                        }
                    }
                    this.filterOptionsInfo[type] = jobTypeOptions;
                }
                return jobTypeOptions;
            case cacheFilterDataType.startTime:
            case cacheFilterDataType.endTime:
                const cachedFilterTime = new Map();
                if (this.cacheFilterDataObj[type]) {
                    const filterTime = RM.getSessionStorage(`${filterCacheNamePrefix}_FSFilterData_${type}`);
                    cachedFilterTime.set(filterTime.FilterOption, filterTime);
                    this.filterOptionsInfo[type] = Array.from(
                        cachedFilterTime,
                        ([_, value]) => value).filter(item => item.FilterOption == filterTime.FilterOption
                    );
                }
                return cachedFilterTime;
            case cacheFilterDataType.jobRunBy:
                if (!this.cacheFilterDataObj.JobRunBy) return [];

                const filterJobRunBy = RM.getSessionStorage(`${filterCacheNamePrefix}_FSFilterData_${type}`);
                this.filterOptionsInfo[type] = filterJobRunBy;
                return filterJobRunBy;
        }
    }
    
    onClear = () => {
        const filterPropertyName = Object.values(cacheFilterDataType);
        this.filterDefinitions = new Map();
        filterPropertyName.forEach(item => delete this.filterOptionsInfo[item]);

        this.setState({
            pathOptions: RM.deepcopy(this.props.pathOptions),
            jobTypeOptions: RM.deepcopy(this.props.jobTypeOptions),
            statusOptions: RM.deepcopy(this.props.statusOptions),
            connectionGroupOptions: RM.deepcopy(this.props.connectionGroupOptions),
            startTimeDefinitions: new Map(),
            endTimeDefinitions: new Map(),
            jobRunByOptions: [],
        });
    }

    onKeyDown(e) { 
        if (e.key === 'Enter') {
            e.target.click();
        }
    }

    pathFilterChanged = (args) => {
        this.filterOptionsInfo[cacheFilterDataType.path] = getMulticomboboxAllItems(args.newValue, this.state.pathOptions, "id", "isChecked");
    }

    jobTypeFilterChanged = (args) => {
        this.filterOptionsInfo[cacheFilterDataType.jobType] = getMulticomboboxAllItems(args.newValue, this.state.jobTypeOptions, "id", "isChecked");
    }

    statusFilterChanged = (args) => {
        this.filterOptionsInfo[cacheFilterDataType.status] = getMulticomboboxAllItems(args.newValue, this.state.statusOptions, "id", "isChecked");
    }

    connectionGroupFilterChanged = (args) => {
        this.filterOptionsInfo[cacheFilterDataType.connectionGroup] = getMulticomboboxAllItems(args.newValue, this.state.connectionGroupOptions, "id", "isChecked");
    }

    onTimeFilterChange = (filterValue, type) => {
        this.filterDefinitions.set(filterValue.FilterOption, filterValue);
        const filterDefinitions = Array.from(this.filterDefinitions, ([_, value]) => value).filter(item => item.FilterOption == filterValue.FilterOption);

        this.filterOptionsInfo[type] = filterDefinitions;
        RM.setSessionStorage(`${filterCacheNamePrefix}_FSFilterData_${type}`, filterValue);

        if (type == cacheFilterDataType.startTime) {
            this.setState({ startTimeDefinitions: this.filterDefinitions });
        } else {
            this.setState({ endTimeDefinitions: this.filterDefinitions });
        }
    };

    onRemoveTimeFilter = (filterValue, type) => {
        this.filterDefinitions.delete(filterValue);
        this.filterOptionsInfo[type] = [];

        if (type == cacheFilterDataType.startTime) {
            this.setState({ startTimeDefinitions: _.cloneDeep(this.filterDefinitions) });
        } else {
            this.setState({ endTimeDefinitions: _.cloneDeep(this.filterDefinitions) });
        }
    };

    onAddUserSelectionChanged = (args) => {
        let selections = RM.deepcopy(args);
        this.filterOptionsInfo.JobRunBy = selections;
        if (selections.length) {
            RM.setSessionStorage(`${filterCacheNamePrefix}_FSFilterData_${cacheFilterDataType.jobRunBy}`, selections);
        }
        this.addUserChanged = selections;
        this.setState({
            jobRunByOptions: selections,
        });
    }

    render() {  
        return (
            <div id={this.props.id}>
                <div className="ra-flex-justify-end">
                    <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClear} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
                </div>

                <$g.FormRow label={RMResx.RM_FS_Register_Path}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="id"
                        width={"100%"}
                        items={this.state.pathOptions}
                        onChange={this.pathFilterChanged}
                        aria={{ ariaLabel: RMResx.RM_FS_Register_Path }}
                        disabled={this.props.disabledPathFilter}
                    />
                </$g.FormRow>
                <$g.FormRow label={RMResx.RM_JS_JM_Module}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="id"
                        width={"100%"}
                        searchable={false}
                        items={this.state.jobTypeOptions}
                        onChange={this.jobTypeFilterChanged}
                        aria={{ ariaLabel: RMResx.RM_JS_JM_Module }}
                    />
                </$g.FormRow>
                <$g.FormRow label={RMResx.RM_JS_JM_Status}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="id"
                        width={"100%"}
                        searchable={false}
                        items={this.state.statusOptions}
                        onChange={this.statusFilterChanged}
                        aria={{ ariaLabel: RMResx.RM_JS_JM_Status }}
                    />
                </$g.FormRow>
                <$g.FormRow label={RMResx.RM_FS_Register_GroupName}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="id"
                        width={"100%"}
                        items={this.state.connectionGroupOptions}
                        onChange={this.connectionGroupFilterChanged}
                        aria={{ ariaLabel: RMResx.RM_FS_Register_GroupName }}
                    />
                </$g.FormRow>
                <CollectionTimeFilter
                    filterOption={FilterOptions.StartTime}
                    onFilterChange={(filterValue) => this.onTimeFilterChange(filterValue, cacheFilterDataType.startTime)}
                    onRemoveFilterChange={(filterValue) => this.onRemoveTimeFilter(filterValue, cacheFilterDataType.startTime)}
                    filterDefinitions={this.state.startTimeDefinitions}
                />
                <CollectionTimeFilter
                    filterOption={FilterOptions.EndTime}
                    onFilterChange={(filterValue) => this.onTimeFilterChange(filterValue, cacheFilterDataType.endTime)}
                    onRemoveFilterChange={(filterValue) => this.onRemoveTimeFilter(filterValue, cacheFilterDataType.endTime)}
                    filterDefinitions={this.state.endTimeDefinitions}
                />
                <$g.FormRow label={RMResx.RM_JS_JM_UserName} id="ariaJobRunBy">
                    <PeoplePicker
                        width="100%"
                        isAllowCustomizeUser
                        items={this.state.jobRunByOptions}
                        selectionChanged={this.onAddUserSelectionChanged}
                        ariaId="ariaJobRunBy"
                    />
                </$g.FormRow>
            </div>
        )
    }
}

export default ConnectionDetailsFilterForm;