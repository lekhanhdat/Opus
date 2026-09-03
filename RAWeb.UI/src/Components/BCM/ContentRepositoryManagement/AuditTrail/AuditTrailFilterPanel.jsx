import { FilterOptions } from "../../../MT/MachineLearningReview/Constants";
import { CollectionTimeFilter } from "../../../MT/MachineLearningReview/Filters";

const FilterTypeMap = {
    31: "ExecutedBy",
    32: "AuditType",
    33: "ExecutedTime",
};

export default class AuditTrailFilterPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.cacheFilterData = RM.getSessionStorage("AuditTrailFilterData") || null;
        this.cacheTimeData = RM.getSessionStorage("AuditTrailFilterData_TimeRange") || null;
        this.filterParam = {};
        this.filterDefinitions = new Map();
        this.filterOptionsInfo = {};
        this.state = {
            user: [],
            action: [],
            time: this.getCacheFilterTimes(FilterTypeMap[FilterOptions.Time]),
        };
        this.bind(["filterColumnChanged"]);
    }

    componentReceive(type, callback) {
        switch (type) {
            case "initData":
                this.getFilterData();
                break;
            case "saveFilter":
                if (callback) callback(this.filterParam);
                break;
        }
    }

    getFilterData() {
        $$.loading(true);
        if (!this.cacheFilterData) {
            RM.setSessionStorage("AuditTrailFilterData", {});
        }
        let option = {
            url: "/api/FSSettingApi/GetJPMCAuditFilterSources",
            method: "GET",
        };
        fetchUtility(option).then((data) => {
            this.getMulticomboboxData(data);
            $$.loading(false);
        })
        .catch((e) => {
            $$.loading(false);
        });
    }

    getCacheFilterTimes(type) {
        let cachedFilterTime = new Map();
        if (this.cacheFilterData && this.cacheFilterData[type] && this.cacheTimeData?.length) {
            this.cacheTimeData.forEach(filterTime => {
                if (filterTime?.FilterOption !== undefined) {
                    cachedFilterTime.set(filterTime.FilterOption, filterTime);
                    this.filterDefinitions.set(filterTime.FilterOption, filterTime);
                }
            });
            this.filterOptionsInfo[type] = Array.from(cachedFilterTime.values());
            this.filterParam[type] = this.cacheFilterData[type];
        }
        return cachedFilterTime;
    }

    getMulticomboboxData(data) {
        let optionsFilter = {
            ExecutedBy: this.formatMulticomboboxData(data.UserItems, FilterOptions.User),
            AuditType: this.formatMulticomboboxData(data.ActionItems, FilterOptions.Action),
        };
        this.setState({
            user: optionsFilter.ExecutedBy,
            action: optionsFilter.AuditType,
        });
    }

    formatMulticomboboxData(items, itemType) {
        const cachedValues = this.cacheFilterData?.[FilterTypeMap[itemType]];
        let formatData = [];
        for (let key in items) {
            if(items.hasOwnProperty(key)){
                let item = {};
                item.Id = key;
                item.value = items[key];
                item.displayText = itemType === FilterOptions.User ? this.wrapperI18N(items[key]): items[key];
                item.isChecked = true;
                formatData.push(item);
            }
        }
        if (cachedValues?.length) {
            this.filterParam[FilterTypeMap[itemType]] = cachedValues;
            for (let item of formatData) {
                if (itemType === FilterOptions.User) {
                    if (cachedValues.indexOf(item.value) == -1) {
                        item.isChecked = false;
                    }
                } else {
                    if (cachedValues.indexOf(parseInt(item.Id,10)) == -1) {
                        item.isChecked = false;
                    }
                }
            }
        }
        return formatData;
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    filterColumnChanged(key, args) {
        let values = [];
        for (let item of args.newValue) {
            if (key === FilterOptions.User) {
                values.push(item.value);
            } else {
                values.push(parseInt(item.Id,10));
            }
        }
        if (args.isSelectAll || values.length === 0) {
            delete this.filterParam[FilterTypeMap[key]];    
        }else{
            this.filterParam[FilterTypeMap[key]] = values;
        }
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    onClearFilter = () => {
        this.cacheFilterData = {};
        this.filterParam = {};
        this.filterDefinitions = new Map();
        this.setState({ time: new Map() });
        this.getFilterData();
    };

    onTimeFilterChange = (filterValue, type) => {
        this.filterDefinitions.set(filterValue.FilterOption, filterValue);
        const filterDefinitions = Array.from(this.filterDefinitions, ([_, value]) => value).filter((item) => item.FilterOption == filterValue.FilterOption);

        this.filterOptionsInfo[FilterTypeMap[type]] = filterDefinitions;
        this.filterParam[FilterTypeMap[type]] = [filterDefinitions[0].Value];
        RM.setSessionStorage("AuditTrailFilterData_TimeRange", filterDefinitions);
    };

    onRemoveTimeFilter = (filterValue, type) => {
        this.filterDefinitions.delete(filterValue);
        this.filterOptionsInfo[FilterTypeMap[type]] = [];
        delete this.filterParam[FilterTypeMap[type]];
        this.setState({ time: new Map() });
    };

    render() {
        return (
            <div className="ra-phyExp-filterForm" id={this.props.id}>
                <div className="ra-flex-justify-end">
                    <a
                        className="ra-main-filter-clear fia-funnel-clear"
                        onClick={this.onClearFilter}
                        tabIndex="0"
                        onKeyDown={this.onKeyDown}
                    >
                        {" "}
                        {RMResx.RM_Common_ClearFilter}
                    </a>
                </div>
                <$g.FormRow label={RMResx.RM_JS_RC_Audit_ViewBy_Option_User}>
                    <R.Multicombobox
                        height={34}
                        width={"100%"}
                        checkedField="isChecked"
                        textField="displayText"
                        valueField="Id"
                        hasFilter={true}
                        required={true}
                        clearable={true}
                        items={this.state.user}
                        noneText={RMResx.RM_JS_RC_Audit_ViewBy_Option_User}
                        onChange={this.filterColumnChanged.bind(this, FilterOptions.User)}
                        triggerBySource={true}
                    />
                </$g.FormRow>
                <$g.FormRow label={RMResx.RM_JS_RC_Audit_ViewBy_Option_Action}>
                    <R.Multicombobox
                        height={34}
                        width={"100%"}
                        checkedField="isChecked"
                        textField="value"
                        valueField="Id"
                        hasFilter={true}
                        required={true}
                        clearable={true}
                        items={this.state.action}
                        noneText={RMResx.RM_JS_RC_Audit_ViewBy_Option_Action}
                        onChange={this.filterColumnChanged.bind(this, FilterOptions.Action)}
                        triggerBySource={true}
                    />
                </$g.FormRow>
                <CollectionTimeFilter
                    filterOption={FilterOptions.Time}
                    onFilterChange={(filterValue) => this.onTimeFilterChange(filterValue, FilterOptions.Time)}
                    onRemoveFilterChange={(filterValue) => this.onRemoveTimeFilter(filterValue, FilterOptions.Time)}
                    filterDefinitions={this.state.time}
                />
            </div>
        );
    }
}
