export default class ConfigScopeTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            disabled: false,
            isSelectAll: false,
            items: [],
            columns: [],
            rootData: {
                disabled: false,
                isAdmin: false
            },
        };
        this.bind('onSelectAllChanged', 'onRowEvent', 'onDisabledChanged', 'getOptionButton');
    }

    componentInit() {

    }

    componentReceive(action, ...args) {
        switch(action)
        {
            case "init":
                var [items, columnInfo] = args;
                var columns = [...this.getOptionButton(), ...columnInfo];
                this.setState({
                    items: items,
                    columns: columns
                }, () => {
                    this.resetSelectAllStatus(items);
                });
                break;
            case "save":
                args[0](RM.deepcopy(this.state.items));
                break;
        }
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
            width: 60
        }];
    }

    updateSelectAll(checked) {
        this.state.columns[0] = Object.assign({}, this.state.columns[0], {
            headerTemplate:
                <R.Checkbox
                    checked={checked}
                    disabled={this.state.disabled}
                    onChange={this.onSelectAllChanged}
                />
        });
        this.setState({ isSelectAll: checked, columns: this.state.columns.slice() });
    }

    checkSelectAll(rowData) {
        var isAll = false;
        if (rowData.isChecked) {
            isAll = this.state.items.every(item => item.isChecked);
        }
        if (isAll !== this.state.isSelectAll) {
            this.updateSelectAll(isAll);
        }
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
            default:
                break;
        }
    };

    render() {
        return (
            <div>
                <R.Table
                    id="corr-table2"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={ConfigScopeTableRowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                />
            </div>
        );
    }
}

class ConfigScopeTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
        this.setState({});
    };

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let index = this.props.index; 
        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        id={"raCpAmConfigScopeTableChk" + index}
                        checked={rowData.isChecked || false}
                        onChange={this.onCheckChange}
                    />
                </Cell>
                <Cell>
                    <div className="groupName-span" data-tooltip aria-label={rowData.Name}>
                        {rowData.Name}
                    </div>
                </Cell>
            </Row>
        );
    }
}
