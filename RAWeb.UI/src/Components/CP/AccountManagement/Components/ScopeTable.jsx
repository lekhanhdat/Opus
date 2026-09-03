import { SourceFlags, PhyUserRoleType } from "../../../../Constants/Constants";

export default class ScopeTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            disabled: false,
            isSelectAll: false,
            items: [],
            columns: [],
            rootData: {
                disabled: false,
                isAdmin: false,
                selectedValue: "1"
            },
        };
        this.bind('onSelectAllChanged', 'onRowEvent', 'getOptionButton');
    }

    componentInit() {

    }

    componentReceive(dataList, columnInfo, disabledStatus) {
        let columns = [...this.getOptionButton(), ...columnInfo];
        for(let item of dataList){
            item.disabled = disabledStatus;
        }
        this.setState({
            items: dataList,
            columns: columns,
            disabled: disabledStatus
        }, () => {
            this.resetSelectAllStatus(dataList);
        });
    }

    resetSelectAllStatus(dataList) {
        let isAll = false;
        if (dataList && dataList.length > 0) {
            isAll = dataList.every(r => r.isChecked);
        }
        this.updateSelectAll(isAll);
    }

    getOptionButton() {
        return [{
            headerTemplate:
                <R.Checkbox
                    checked={this.state.isSelectAll}
                    disabled={this.state.disabled}
                    onChange={this.onSelectAllChanged}
                />,
            width: 50
        }];
    }

    updateSelectAll(checked) {
        const checkedItems = this.state.items.filter(item => item.isChecked);
        let checkedValue = checked;
        if (checkedItems.length) {
            checkedValue = checkedItems.length === this.state.items.length ? true : 'mixed';
        }
        this.state.columns[0] = Object.assign({}, this.state.columns[0], {
            headerTemplate:
                <R.Checkbox
                    checked={checkedValue}
                    disabled={this.state.disabled}
                    onChange={this.onSelectAllChanged}
                />
        });
        this.setState({ isSelectAll: checkedValue, columns: this.state.columns.slice() });
    }

    checkSelectAll(rowData) {
        let isAll = false;
        if (rowData.isChecked) {
            isAll = this.state.items.every(item => item.isChecked);
        }
        this.updateSelectAll(isAll);
        let items = RM.deepcopy(this.state.items);
        this.setState({
            items: items
        });
        this.props.onCheckChanged(items);
    }

    onSelectAllChanged(checked) {
        this.state.items.forEach(item => item.isChecked = checked);
        this.updateSelectAll(checked);
        this.setState({ isSelectAll: checked, items: this.state.items.slice() });
        this.props.onCheckChanged(RM.deepcopy(this.state.items));
    }

    onRowEvent = (args, selectedOption) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'checked':
                this.checkSelectAll(rowData);
                break;
            case 'cellClick':
                this.cellClick(rowData);
                break;
            default:
                break;
        }
    };

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
    }

    render() {
        let tableClass = this.state.disabled ? "record-table-disabled" : "";
        const normalizedRootData = {
            ...this.state.rootData,
            selectedValue: this.props.selectedValue ?? this.state.rootData.selectedValue
        };
        return (
            <div className="record-table-wrapper">
                <div className={tableClass}>
                    <R.Table
                        id="corr-table1"
                        rootData={normalizedRootData}
                        columns={this.state.columns}
                        rowTemplate={ScopeTableRowTemplate}
                        items={this.state.items}
                        onRowEvent={this.onRowEvent}
                    />
                </div>
            </div>
        );
    }
}

class ScopeTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
        this.setState({});
    };
 
    onEditScopeClick = (data) => {
        this.dispatch('cellClick', data);
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let rootData = this.props.rootData;
        
        let allowEditContainer = rowData.isChecked &&
            (rowData.SourceType == SourceFlags.SP ||
                rowData.SourceType == SourceFlags.Exo ||
                rowData.SourceType == SourceFlags.OneDrive ||
                rowData.SourceType == SourceFlags.Teams ||
                (rowData.SourceType == SourceFlags.Phy && rootData.selectedValue != PhyUserRoleType.EndUser));
        let index = this.props.index; 
        let containerNames = rowData.ContainerNames || "";
        if(rowData.isChecked && rowData.SourceType == SourceFlags.Phy && rootData.selectedValue == PhyUserRoleType.EndUser){
            containerNames = RMResx.RM_CP_AM_PhysicalContainer_Column;
        }
        return (
            <Row className="ra-scope-table-row">
                <Cell>
                    <R.Checkbox
                        id={"raCpAmScopeTableChk" + index}
                        checked={rowData.isChecked || false}
                        onChange={this.onCheckChange}
                        disabled = {rowData.disabled}
                    />
                </Cell>
                <Cell>
                    <div className="groupName-span" data-tooltip aria-label={rowData.Name}>
                        {rowData.Name}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={containerNames}>
                        {containerNames}
                    </div>
                </Cell>
                <Cell>
                    {allowEditContainer && <span>
                        <R.Button
                            disabled = {rowData.disabled}
                            type="bald"
                            icon="fia-edit"
                            tooltip={rowData.SourceType == SourceFlags.Phy ? RMResx.RM_CP_AM_EditPhyContainers_Title : RMResx.RM_CP_AM_EditContainers_Title}
                            className="padding-xs"
                            onClick={this.onEditScopeClick.bind(this, rowData)}/>
                    </span>
                    }
                </Cell>
            </Row>
        );
    }
}
