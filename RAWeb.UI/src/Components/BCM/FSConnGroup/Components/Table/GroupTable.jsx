import { LicenseHelper, isEnableMultiGeoFeature } from "../../../../../Utilities/CommonUtil";

const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();
const enableMultiGeoFeature = isEnableMultiGeoFeature();
export default class GroupTable extends R.Component {
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
            default:
                break;
        }
    };

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
    }

    render() {
        return (
            <div className="ra-main-table">
                <R.Table
                    id="group-table1"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={GroupConnectionTableRowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    onSort={this.props.onSort}
                />
            </div>
        );
    }
}

class GroupConnectionTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    renderConnectionCell(rowData) {
        if(rowData.FSConnections && rowData.FSConnections.length > 0) {
            if (isEnableJPMCFeature) {
                return (
                    <a
                        className="ra-main-cell-link"
                        tabIndex='0'
                        onClick={this.onCellClick.bind(this, 3, 'cellClick')}
                        onKeyDown={this.onCellClick.bind(this, 3)}
                        data-tooltip aria-label={RMResx.RM_JS_FS_ConnectionCell_ViewDetails}
                        data-tooltip-wrap="force"
                    >
                        {RMResx.RM_JS_FS_ConnectionCell_ViewDetails}
                    </a>
                );
            }
            return (
                <span
                    className={"connections-list-span text-overflow"}
                    style={{ display: "inline-block", maxWidth: "80%" }}
                    data-tooltip-wrap="force"
                >
                    {rowData.FSConnections.map(c => c.Name).join("; ")}
                </span>
            );
        }
        return null;
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
        this.setState({});
    };

    onCellClick(operationOption, event) {
        if (event == 'cellClick') {
            this.dispatch('cellClick', operationOption);
        } else {
            if (event.keyCode == "13") {
                this.dispatch('cellClick', operationOption);
            }
        }
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        //     rootData = this.props.rootData;
        // let timeZone = RM.TimeUtil.getTimezoneInfo(rowData.TimeZoneId, rowData.IsDayLightSaving);
        // let duration = rowData.Type == 0 ?
        //     rowData.Number + " " + intervalType[rowData.Unit]
        //     : RM.TimeUtil.dateToString(new Date(rowData.CalenderTime), timeZone);
        let connectionCell = this.renderConnectionCell(rowData);

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
                        <a className="ra-main-cell-link" tabIndex='0' onClick={this.onCellClick.bind(this, 1, 'cellClick')}
                            onKeyDown={this.onCellClick.bind(this, 1)} data-tooltip aria-label={rowData.Name} data-tooltip-wrap="force">
                            {rowData.Name}
                        </a>
                    </div>

                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Description} data-tooltip-wrap="force">
                        {rowData.Description}
                    </div>
                </Cell>
                <Cell>
                    <div data-tooltip>
                        {connectionCell}
                        {/* <R.Button
                            type="bald"
                            icon="fia-edit"
                            tooltip={RMResx.RM_FS_Register_EditCorrelateConnections}
                            onClick={this.onCellClick.bind(this, 2, 'cellClick')} /> */}
                    </div>
                </Cell>
                {
                    enableMultiGeoFeature && 
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.DCDisplayName} data-tooltip-wrap="force">
                            {rowData.DCDisplayName}
                        </div>
                    </Cell>
                }
                <Cell>
                    <div data-tooltip>
                        {
                            rowData.AccessConnectionType === 0 ?
                                <div className="text-overflow" data-tooltip aria-label={RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type_All}>
                                    {RMResx.RM_FS_Register_SpecifyAgentAccessConn_Type_All}
                                </div> :
                                rowData.Agents && rowData.Agents.length > 0 &&
                                <span className={"connections-list-span text-overflow"} style={{ display: "inline-block", maxWidth: "80%" }} data-tooltip-wrap="force">
                                    {rowData.Agents.map(c => c.Name).join("; ")}
                                </span>
                        }
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTime}>
                        {rowData.LastModifiedTime}
                    </div>
                </Cell>
            </Row>
        );
    }
}