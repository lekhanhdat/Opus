import { ManageRelatedTableRow } from './RowTemplate';

export default class RelatedTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            page: 1,
            pageSize: 10,
            disabled: false,
            isSelectAll: false,
            showActionBtn: false,
            nameVisibility: 'show',
            items: [],
            columns: [],
            rootData: {
                disabled: false,
                isAdmin: false,
                showCheckBox: this.props.showCheckBox,
            },
            tableId: this.props.tableId,
            frozenCount: this.props.frozenCount || 0,
        };
        this.bind('onSelectAllChanged', 'onAddRows', 'onChangeVisibility',
            'onClearRows', 'onDeleteRows', 'onRowEvent', 'onDisabledChanged', 'getOptionButton');
    }

    componentReceive(action, data) {
        switch (action) {
            case "initPageData":
                this.setState({
                    items: data,
                }, () => {
                    if (this.props.showCheckBox) {
                        this.resetSelectAllStatus(data);
                    }
                });
                break;
        }
    }

    componentInit() {
        let columns = this.props.showCheckBox ? [...this.getBasicColumns(), ...this.props.columns] : this.props.columns;
        this.setState({
            columns: columns,
        });
    }

    resetSelectAllStatus(data) {
        let isAll = false;
        if (data && data.length > 0) {
            isAll = data.every(r => r.isChecked);
        }
        this.updateSelectAll(isAll);
    }
    getBasicColumns() {
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
        if (this.props.showCheckBox) {
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

    onRowEvent = (args) => {
        let rowIndex = args.rowIndex,
            rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData);
                break;
            case 'checked':
                this.checkSelectAll(rowData);
                break;
            default:
                break;
        }
    }

    cellClick(data) {
        this.props.cellClick(data);
    }

    onPager(e, args) {
        var page = args.newValue.selectedPage,
            pageSize = args.newValue.pageSize;
        this.setState({ page, pageSize, items: [] });
    }

    deleteCurrentRow(rowIndex) {
        this.state.items.splice(rowIndex, 1);
        this.setState({ items: this.state.items.slice() });
    }

    render() {
        return (
            <div id={this.props.id}>
                <R.Table
                    id={this.state.tableId}
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    frozenCount={this.state.frozenCount}
                    rowTemplate={ManageRelatedTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                />
            </div>
        );
    }
}