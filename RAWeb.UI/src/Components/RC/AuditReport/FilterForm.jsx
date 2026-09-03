import {AuditReportFilterType} from "../Constants";

export default class Filter extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.filterParam = {};
        this.echoData = RM.deepcopy(this.props.data);
        this.state = {
            actionItems: [],
            moduleItems: [],
            statusItems: [],
            userItems: [],
        };
        this.bind(['filterColumnChanged']);
    }

    componentInit() {
        this.getFilterData();
    }

    componentReceive(data) {
        this.handleFilterButton(data);
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    handleFilterButton = (isClear) => {
        if (isClear) {
            this.filterParam = {};
        }
        this.props.onSave(this.filterParam);
    };

    onClearFilter = () => {
        this.echoData = {};
        this.props.onSave({}, false);
        this.getFilterData();
    }

    getFilterData() {
        let param = {};
        $$.loading(true);
        let option = {
            url: '/api/AuditApi/GetFiltersSource',
            data: param
        };
        fetchUtility(option).then((data) => {
            $$.loading(false);
            this.getMulticomboboxData(data);
        }).catch((e) => {
        });
    }

    getMulticomboboxData(data) {
        let userItems = this.formatMulticomboboxData(data.UserItems, 1),
            moduleItems = this.formatMulticomboboxData(data.ModuleItems, 3),
            actionItems = this.formatMulticomboboxData(data.ActionItems, 5),
            statusItems = this.formatMulticomboboxData(data.StatusItems, 6);
        this.setState({
            actionItems: actionItems,
            moduleItems: moduleItems,
            statusItems: statusItems,
            userItems: userItems,
        });
        this.filterParam = this.echoData;
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    formatMulticomboboxData(items, itemType) {
        let echoData = this.echoData[itemType];
        let formatData = [];
        for (let key in items) {
            if(items.hasOwnProperty(key)){
                let item = {};
                item.Id = key;
                item.value = items[key];
                item.displayText = itemType == 1? this.wrapperI18N(items[key]): items[key];
                item.isChecked = true;
                formatData.push(item);
            }
        }
        if (echoData) {
            for (let item of formatData) {
                if (itemType == 1) {
                    if (echoData.indexOf(item.value) == -1) {
                        item.isChecked = false;
                    }
                } else {
                    if (echoData.indexOf(parseInt(item.Id,10)) == -1) {
                        item.isChecked = false;
                    }
                }
            }
        }
        return formatData;
    }

    filterColumnChanged(key, args) {
        let values = [];
        for (let item of args.newValue) {
            if (key == AuditReportFilterType.User) {
                values.push(item.value);
            } else {
                values.push(parseInt(item.Id,10));
            }
        }
        if (args.isSelectAll) {
            if(this.filterParam[key]){
                delete this.filterParam[key];
            }
        }else{
            this.filterParam[key] = values;
        }
    }

    render() {
        return <div className='ra-phyExp-filterForm' id={this.props.id}>
            <div className="ra-flex-justify-end">
                <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClearFilter} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
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
                    items={this.state.userItems}
                    noneText= {RMResx.RM_JS_RC_Audit_ViewBy_Option_User}
                    onChange={this.filterColumnChanged.bind(this, AuditReportFilterType.User)}
                    triggerBySource={true}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_RC_Audit_ViewBy_Option_Module}>
                <R.Multicombobox
                    height={34}
                    width={"100%"}
                    checkedField="isChecked"
                    textField="value"
                    valueField="Id"
                    hasFilter={true}
                    required={true}
                    clearable={true}
                    items={this.state.moduleItems}
                    noneText= {RMResx.RM_JS_RC_Audit_ViewBy_Option_Module}
                    onChange={this.filterColumnChanged.bind(this, AuditReportFilterType.DocAveModule)}
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
                    items={this.state.actionItems}
                    noneText= {RMResx.RM_JS_RC_Audit_ViewBy_Option_Action}
                    onChange={this.filterColumnChanged.bind(this, AuditReportFilterType.Action)}
                    triggerBySource={true}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_RC_Audit_ViewBy_Option_Status}>
                <R.Multicombobox
                    height={34}
                    width={"100%"}
                    checkedField="isChecked"
                    textField="value"
                    valueField="Id"
                    hasFilter={true}
                    required={true}
                    items={this.state.statusItems}
                    clearable={true}
                    noneText= {RMResx.RM_JS_RC_Audit_ViewBy_Option_Status}
                    onChange={this.filterColumnChanged.bind(this, AuditReportFilterType.Status)}
                    triggerBySource={true}
                />
            </$g.FormRow>
        </div>;
    }
}