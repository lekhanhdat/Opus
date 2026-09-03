import { LicenseHelper } from "../../../../../Utilities/CommonUtil";
const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();
export default class ConnectionTable extends R.Component {
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

    componentReceive(dataList, columnInfo) {
        let columns = [...this.getOptionButton(), ...columnInfo];
        this.setState({
            items: dataList,
            columns: columns
        }, () => {
            this.resetSelectAllStatus(dataList);
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
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData, selectedOption);
                break;
            case 'checked':
                this.checkSelectAll(rowData);
                break;
            case 'showDetails':
                this.showDetails(rowData);
                break;
            default:
                break;
        }
    };

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
    }

    showDetails(data) {
        this.props.showDetails(data);
    }

    onSort = (args) =>{
        this.props.onSort(args.status === "desc", args.column.valuePath);
    }

    render() {
        return (
            <div className="ra-main-table">
                <R.Table
                    id="conn-table1"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={ConnectionTableRowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    doSort={this.onSort}
                />
            </div>
        );
    }
}

class ConnectionTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        // this.bind("onCellClick", "onSelectChange");
    }

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

    onShowDetails(event) { 
        if (event == 'cellClick') {
            this.dispatch('showDetails');
        } else {
            if (event.keyCode == "13") {
                this.dispatch('showDetails');
            }
        }
    }

    renderDisplayName(data) {
        return data?.map(item => item.DisplayName)?.join("; ");
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        checked={rowData.isChecked || false}
                        onChange={this.onCheckChange}
                    />
                </Cell>
                <Cell>
                    <div className="text-overflow">
                        <a className="ra-main-cell-link" tabIndex='0' onClick={this.onCellClick.bind(this, 'cellClick')}
                            onKeyDown={this.onCellClick.bind(this)} data-tooltip aria-label={rowData.Name} data-tooltip-wrap="force">
                            {rowData.Name}
                        </a>
                    </div>
                </Cell>
                {
                    isEnableJPMCFeature && (
                        <Cell>
                            <div className="text-overflow" data-tooltip aria-label={rowData?.JPMCConnectionId} data-tooltip-wrap="force">
                                {rowData?.JPMCConnectionId}
                            </div>
                        </Cell>
                    )
                }
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Description} data-tooltip-wrap="force">
                        {rowData.Description}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.UNCPath} data-tooltip-wrap="force">
                        {rowData.UNCPath}
                    </div>
                </Cell>
                {
                    isEnableJPMCFeature && (
                        <>
                            <Cell>
                                <div className="text-overflow" data-tooltip aria-label={this.renderDisplayName(rowData?.InformationOwners)}>
                                    {this.renderDisplayName(rowData?.InformationOwners)}
                                </div>
                            </Cell>
                            <Cell>
                                <div className="text-overflow" data-tooltip aria-label={this.renderDisplayName(rowData?.RecordOwners)}>
                                    {this.renderDisplayName(rowData?.RecordOwners)}
                                </div>
                            </Cell>
                        </>
                    )
                }
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.GroupName} data-tooltip-wrap="force">
                        {rowData.GroupName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTime}>
                        {rowData.LastModifiedTime}
                    </div>
                </Cell>
                { isEnableJPMCFeature && (
                    <>
                        <Cell>
                            <div className="text-overflow flex align-center gap-inline-s" data-tooltip aria-label={rowData.Monitor}>
                                <span className="fia-status-warning ra-warn-color"></span>
                                {rowData.Monitor > 0 ? (
                                    <a
                                        className="ra-cell-link"
                                        tabIndex='0'
                                        onClick={this.onShowDetails.bind(this, 'cellClick')}
                                        onKeyDown={this.onShowDetails.bind(this)}
                                        aria-label={rowData.Monitor}
                                    >
                                        {rowData.Monitor}
                                    </a>
                                ): (
                                    <span aria-label={rowData.Monitor}>{rowData.Monitor ?? 0}</span>
                                )}
                            </div>
                        </Cell>
                        <Cell>
                            <div className="text-overflow" data-tooltip aria-label={rowData.LastSyncTime}>
                                {rowData.LastSyncTime}
                            </div>
                        </Cell>
                    </>
                )}
            </Row>
        );
    }
}