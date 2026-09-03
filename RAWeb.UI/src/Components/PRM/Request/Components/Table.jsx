import RequestTableRow from './RowTemplate';

export default class RequestTable extends R.Component {
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
                isAdmin: false
            },
        };
        this.bind('onSelectAllChanged', 'onAddRows', 'onChangeVisibility',
            'onClearRows', 'onDeleteRows', 'onRowEvent', 'onDisabledChanged', 'getOptionButton');
    }

    componentReceive(requestList, columnInfo, isRMAdmin) {
        let columns = isRMAdmin ? [...this.getOptionButton(), ...columnInfo] : [...this.getEnduserOption(),...columnInfo];
        // let columns = [...this.getOptionButton(), ...columnInfo];
        this.setState({
            items: requestList,
            rootData: {
                isAdmin: isRMAdmin
            },
            columns: columns
        }, () => {
            if(isRMAdmin){
                this.resetSelectAllStatus(requestList);
            }
        });
    }

    resetSelectAllStatus(requestList) {
        let isAll = false;
        if (requestList && requestList.length > 0) {
            isAll = requestList.every(r => r.isChecked);
            if (isAll) {
                this.updateSelectAll(true);
            } else {
                let isAllUnchecked = requestList.every(r => !r.isChecked);
                this.updateSelectAll(isAllUnchecked ? false : 'mixed');
            }
        } else {
            this.updateSelectAll(false);
        }
    }

    getOptionButton() {
        return [ {
            headerTemplate:
                <R.Checkbox
                    checked={this.state.isSelectAll}
                    disabled={this.state.disabled}
                    onChange={this.onSelectAllChanged}
                />,
            width: 55
        }];
    }

    getEnduserOption(){
        return [ {
            headerTemplate:'',
            align: "center",
            resizeable: true,
            width: 50
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
            this.updateSelectAll(isAll ? true : 'mixed');
        } else {
            let isAllUnchecked = this.state.items.every(item => !item.isChecked);
            this.updateSelectAll(isAllUnchecked ? false : 'mixed');
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

    onRowEvent = (args, selectedOption, selectedIndex) => {
        let rowIndex = args.rowIndex,
            rowData = args.rowData;
        switch (args.type) {
            case 'cellOperate':
                this.cellOperate(rowData, selectedOption);
                break;
            case 'cellClick':
                this.cellClick(rowData, selectedOption, selectedIndex);
                break;
            case 'checked':
                this.checkSelectAll(rowData);
                break;
            default:
                break;
        }
    }

    cellOperate(rowData, selectedOption) {
        this.props.cellOperate(rowData, selectedOption);
    }

    cellClick(data, selectedOption, selectedIndex) {
        this.props.cellClick(data, selectedOption, selectedIndex);
    }

    showFile(rowData) {
        this.props.showFile(rowData);
    }

    showRequest(rowData) {
        this.props.showRequest(rowData);
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
                    id="table1"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    //传递的是class类名，示例中为TableRow1
                    rowTemplate={RequestTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                />
            </div>
        );
    }
}