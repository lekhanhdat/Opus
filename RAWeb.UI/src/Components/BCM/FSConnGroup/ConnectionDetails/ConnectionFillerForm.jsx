import _ from "lodash";
import { FilterOptions } from "../../../MT/MachineLearningReview/Constants";
import { CollectionTimeFilter } from "../../../MT/MachineLearningReview/Filters";
import { cacheFilterDataType, filterConnectionCacheNamePrefix } from "./Constants";
import { getMulticomboboxAllItems } from "../../../../Utilities/CommonUtil";

export default class ConnectionFilterForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        const { filterOptionsInfo } = this.props;

        this.cacheFilterData = sessionStorage.getItem(`${filterConnectionCacheNamePrefix}_FSFilterData`)
            ? JSON.parse(sessionStorage.getItem(`${filterConnectionCacheNamePrefix}_FSFilterData`)) : null;
        this.cacheFilterDataObj = this.getCachedFilterDataObj();
        this.filterOptionsInfo = RM.deepcopy(filterOptionsInfo);
        this.filterDefinitions = new Map();
        this.state = {
            connectionGroupOptions: filterOptionsInfo[cacheFilterDataType.groupName] || this.getCachedFilterDataByType(cacheFilterDataType.groupName),
            modifiedTimeDefinitions: this.getCachedFilterDataByType(cacheFilterDataType.modifiedTime),
            lastSyncTimeDefinitions: this.getCachedFilterDataByType(cacheFilterDataType.lastSyncTime),
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
            [cacheFilterDataType.groupName]: RM.deepcopy(this.props.connectionGroupOptions),
        };

        switch (type) { 
            case cacheFilterDataType.groupName:
                const options = optionObject[type];
                if (this.cacheFilterDataObj[type]) {
                    for (let item of options) {
                        item.isChecked = this.cacheFilterDataObj[type].includes(item.id);
                    }
                    this.filterOptionsInfo[type] = options;
                }
                return options;
            case cacheFilterDataType.modifiedTime:
            case cacheFilterDataType.lastSyncTime:
                const cachedFilterTime = new Map();
                if (this.cacheFilterDataObj[type]) {
                    const filterTime = RM.getSessionStorage(`${filterConnectionCacheNamePrefix}_FSFilterData_${type}`);
                    cachedFilterTime.set(filterTime.FilterOption, filterTime);
                    this.filterOptionsInfo[type] = Array.from(
                        cachedFilterTime,
                        ([_, value]) => value).filter(item => item.FilterOption == filterTime.FilterOption
                    );
                }
                return cachedFilterTime;
        }
    }
    
    onClear = () => {
        const filterPropertyName = Object.values(cacheFilterDataType);
        this.filterDefinitions = new Map();
        filterPropertyName.forEach(item => delete this.filterOptionsInfo[item]);

        this.setState({
            connectionGroupOptions: RM.deepcopy(this.props.connectionGroupOptions),
            modifiedTimeDefinitions: new Map(),
            lastSyncTimeDefinitions: new Map(),
        });
    }

    onKeyDown(e) {
        if (e.key === 'Enter') {
            e.target.click();
        }
    }

    connectionGroupFilterChanged = (args) => {
        this.filterOptionsInfo[cacheFilterDataType.groupName] = getMulticomboboxAllItems(args.newValue, this.state.connectionGroupOptions, "id", "isChecked");
    }

    onTimeFilterChange = (filterValue, type) => {
        this.filterDefinitions.set(filterValue.FilterOption, filterValue);
        const filterDefinitions = Array.from(this.filterDefinitions, ([_, value]) => value).filter(item => item.FilterOption == filterValue.FilterOption);

        this.filterOptionsInfo[type] = filterDefinitions;
        RM.setSessionStorage(`${filterConnectionCacheNamePrefix}_FSFilterData_${type}`, filterValue);

        if (type == cacheFilterDataType.modifiedTime) {
            this.setState({ startTimeDefinitions: this.filterDefinitions });
        } else {
            this.setState({ endTimeDefinitions: this.filterDefinitions });
        }
    };

    onRemoveTimeFilter = (filterValue, type) => {
        this.filterDefinitions.delete(filterValue);
        this.filterOptionsInfo[type] = [];

        if (type == cacheFilterDataType.modifiedTime) {
            this.setState({ startTimeDefinitions: _.cloneDeep(this.filterDefinitions) });
        } else {
            this.setState({ endTimeDefinitions: _.cloneDeep(this.filterDefinitions) });
        }
    };

    render() {  
        return (
            <div id={this.props.id}>
                <div className="ra-flex-justify-end">
                    <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClear} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
                </div>
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
                    filterOption={FilterOptions.ModifiedTime}
                    onFilterChange={(filterValue) => this.onTimeFilterChange(filterValue, cacheFilterDataType.modifiedTime)}
                    onRemoveFilterChange={(filterValue) => this.onRemoveTimeFilter(filterValue, cacheFilterDataType.modifiedTime)}
                    filterDefinitions={this.state.modifiedTimeDefinitions}
                />
                <CollectionTimeFilter
                    filterOption={FilterOptions.LastSyncTime}
                    onFilterChange={(filterValue) => this.onTimeFilterChange(filterValue, cacheFilterDataType.lastSyncTime)}
                    onRemoveFilterChange={(filterValue) => this.onRemoveTimeFilter(filterValue, cacheFilterDataType.lastSyncTime)}
                    filterDefinitions={this.state.lastSyncTimeDefinitions}
                />
            </div>
        )
    }
}