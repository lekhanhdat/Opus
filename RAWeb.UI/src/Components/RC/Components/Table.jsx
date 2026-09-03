import { Fragment } from "react";

export default class ReportTable extends R.Component {
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
        this.bind('onSelectAllChanged', 'getOptionButton');
    }

    componentInit() {

    }

    componentReceive(requestList, columnInfo) {
        let columns = [...this.getOptionButton(), ...columnInfo];
        this.setState({
            items: requestList,
            columns: columns
        }, () => {
            this.resetSelectAllStatus(requestList);
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

    onRowEvent = (args, selectedOption) => {
        const rowData = args.rowData;
        switch (args.type) {
            case 'cellOperate':
                this.cellOperate(rowData, selectedOption);
                break;
            case 'cellClick':
                this.cellClick(rowData, selectedOption);
                break;
            case 'checked':
                this.checkSelectAll(rowData);
                break;
            default:
                break;
        }
    };

    cellOperate(rowData, selectedOption) {
        this.props.cellOperate(rowData, selectedOption);
    }

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
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
            <div>
                <R.Table
                    id="reco-report-management-table"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={ReportTableRowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    doSort={this.props.onSort}
                />
            </div>
        );
    }
}

let groupItems = [
    { displayName: RMResx.RM_JS_Common_Edit, index: 1 },
    { displayName: RMResx.RM_JS_Common_GenerateReport, index: 3 },
    { displayName: RMResx.RM_JS_Common_ShowReport, index: 4 },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_Delete, index: 2 },
];

class ReportTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        // this.bind("onCellClick", "onSelectChange");
    }

    onSelectChange = (item) => {
        this.dispatch('cellOperate', item);
    };

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
        this.setState({});
    };

    onCellClick(event) {
        if (event == 'cellClick') {
            this.dispatch('cellClick');
        } else {
            if (event.keyCode == "13") {
                this.dispatch('cellClick');
            }
        }
    }

    action = () => {
        return (
            <Fragment>
                {
                    groupItems.map(item =>
                        <R.Button
                            key={item.index}
                            onClick={this.onSelectChange.bind(this, item)}
                            text={item.displayName}
                        />
                    )
                }
            </Fragment>
        );
    };

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let index = this.props.index; 
        return (
            <Row action={this.action}>
                <Cell>
                    <R.Checkbox
                        id={"raRcTableChk" + index}
                        checked={rowData.isChecked || false}
                        onChange={this.onCheckChange}
                    />
                </Cell>
                <Cell>
                    <div className="text-overflow">
                        <a tabIndex='0' className="ra-main-cell-link" onClick={this.onCellClick.bind(this, 'cellClick')}
                            onKeyDown={this.onCellClick.bind(this)} data-tooltip aria-label={rowData.ProfileName}>
                            {rowData.ProfileName}
                        </a>
                    </div>

                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.ReportType}>
                        {rowData.ReportType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Description}>
                        {rowData.Description}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Modified}>
                        {rowData.Modified}
                    </div>
                </Cell>
            </Row>
        );
    }
}