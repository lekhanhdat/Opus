import "./index.less";
import _ from "lodash";

export class GoogleDataTableRow extends R.TableRow {
    onCheckedChange = () => {
        this.dispatch("checked");
    };

    onCellClick = () => {
        this.dispatch("onCellClick");
    };

    onCellKeyDown = (e) => {
        if (e.keyCode == "13") {
            this.dispatch("onCellClick");
        }
    };

    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                {rowData[1] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            data-tooltip-wrap="force"
                            aria-label={rowData[1].value}
                        >
                            {rowData[1].isLink ? (
                                <a
                                    tabIndex={0}
                                    onClick={this.onCellClick}
                                    onKeyDown={this.onCellKeyDown}
                                >
                                    {rowData[1].value}
                                </a>
                            ) : (
                                <span tabIndex={0}>{rowData[1].value}</span>
                            )}
                        </div>
                    </Cell>
                )}
                {rowData[2] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[2].value}
                        >
                            {rowData[2].value}
                        </div>
                    </Cell>
                )}
                {rowData[3] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[3].value}
                        >
                            {rowData[3].value}
                        </div>
                    </Cell>
                )}
                {rowData[4] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[4].value}
                        >
                            {rowData[4].value}
                        </div>
                    </Cell>
                )}
                {rowData[5] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[5].value}
                        >
                            {rowData[5].value}
                        </div>
                    </Cell>
                )}
                {rowData[6] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[6].value}
                        >
                            {rowData[6].value}
                        </div>
                    </Cell>
                )}
                {rowData[7] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[7].value}
                        >
                            {rowData[7].value}
                        </div>
                    </Cell>
                )}
                {rowData[8] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[8].value}
                        >
                            {rowData[8].value}
                        </div>
                    </Cell>
                )}
                {rowData[9] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[9].value}
                        >
                            {rowData[9].value}
                        </div>
                    </Cell>
                )}
                {rowData[10] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[10].value}
                        >
                            {rowData[10].value}
                        </div>
                    </Cell>
                )}
                {rowData[11] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[11].value}
                        >
                            {rowData[11].value}
                        </div>
                    </Cell>
                )}
                {rowData[12] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[12].value}
                        >
                            {rowData[12].value}
                        </div>
                    </Cell>
                )}
                {rowData[13] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[13].value}
                        >
                            {rowData[13].value}
                        </div>
                    </Cell>
                )}
                {rowData[14] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[14].value}
                        >
                            {rowData[14].value}
                        </div>
                    </Cell>
                )}
                {rowData[15] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[15].value}
                        >
                            {rowData[15].value}
                        </div>
                    </Cell>
                )}
                {rowData[16] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[16].value}
                        >
                            {rowData[16].value}
                        </div>
                    </Cell>
                )}
                {rowData[17] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[17].value}
                        >
                            {rowData[17].value}
                        </div>
                    </Cell>
                )}
                {rowData[18] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[18].value}
                        >
                            {rowData[18].value}
                        </div>
                    </Cell>
                )}
                {rowData[19] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[19].value}
                        >
                            {rowData[19].value}
                        </div>
                    </Cell>
                )}
                {rowData[20] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[20].value}
                        >
                            {rowData[20].value}
                        </div>
                    </Cell>
                )}
                {rowData[21] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[21].value}
                        >
                            {rowData[21].value}
                        </div>
                    </Cell>
                )}
                {rowData[22] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[22].value}
                        >
                            {rowData[22].value}
                        </div>
                    </Cell>
                )}
                {rowData[23] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[23].value}
                        >
                            {rowData[23].value}
                        </div>
                    </Cell>
                )}
                {rowData[24] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[24].value}
                        >
                            {rowData[24].value}
                        </div>
                    </Cell>
                )}
                {rowData[25] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[25].value}
                        >
                            {rowData[25].value}
                        </div>
                    </Cell>
                )}
                {rowData[26] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[26].value}
                        >
                            {rowData[26].value}
                        </div>
                    </Cell>
                )}
                {rowData[27] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[27].value}
                        >
                            {rowData[27].value}
                        </div>
                    </Cell>
                )}
                {rowData[28] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[28].value}
                        >
                            {rowData[28].value}
                        </div>
                    </Cell>
                )}
                {rowData[29] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[29].value}
                        >
                            {rowData[29].value}
                        </div>
                    </Cell>
                )}
                {rowData[30] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[30].value}
                        >
                            {rowData[30].value}
                        </div>
                    </Cell>
                )}
                {rowData[31] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[31].value}
                        >
                            {rowData[31].value}
                        </div>
                    </Cell>
                )}
                {rowData[32] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[32].value}
                        >
                            {rowData[32].value}
                        </div>
                    </Cell>
                )}{rowData[33] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[33].value}
                        >
                            {rowData[33].value}
                        </div>
                    </Cell>
                )}
                {rowData[34] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[34].value}
                        >
                            {rowData[34].value}
                        </div>
                    </Cell>
                )}
                {rowData[35] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[35].value}
                        >
                            {rowData[35].value}
                        </div>
                    </Cell>
                )}{rowData[36] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[36].value}
                        >
                            {rowData[36].value}
                        </div>
                    </Cell>
                )}{rowData[37] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[37].value}
                        >
                            {rowData[37].value}
                        </div>
                    </Cell>
                )}
                {rowData[38] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[38].value}
                        >
                            {rowData[38].value}
                        </div>
                    </Cell>
                )}
                {rowData[39] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[39].value}
                        >
                            {rowData[39].value}
                        </div>
                    </Cell>
                )}
                {rowData[40] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[40].value}
                        >
                            {rowData[40].value}
                        </div>
                    </Cell>
                )}
                {rowData[41] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[41].value}
                        >
                            {rowData[41].value}
                        </div>
                    </Cell>
                )}
                {rowData[42] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[42].value}
                        >
                            {rowData[42].value}
                        </div>
                    </Cell>
                )}
                {rowData[43] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[43].value}
                        >
                            {rowData[43].value}
                        </div>
                    </Cell>
                )}
                {rowData[44] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[44].value}
                        >
                            {rowData[44].value}
                        </div>
                    </Cell>
                )}{rowData[45] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[45].value}
                        >
                            {rowData[45].value}
                        </div>
                    </Cell>
                )}
                {rowData[46] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[46].value}
                        >
                            {rowData[46].value}
                        </div>
                    </Cell>
                )}
                {rowData[47] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[47].value}
                        >
                            {rowData[47].value}
                        </div>
                    </Cell>
                )}
                {rowData[48] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[48].value}
                        >
                            {rowData[48].value}
                        </div>
                    </Cell>
                )}
                {rowData[49] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[49].value}
                        >
                            {rowData[49].value}
                        </div>
                    </Cell>
                )}
                {rowData[50] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[50].value}
                        >
                            {rowData[50].value}
                        </div>
                    </Cell>
                )}
                {rowData[51] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[51].value}
                        >
                            {rowData[51].value}
                        </div>
                    </Cell>
                )}
                {rowData[52] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[52].value}
                        >
                            {rowData[52].value}
                        </div>
                    </Cell>
                )}
                {rowData[53] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[53].value}
                        >
                            {rowData[53].value}
                        </div>
                    </Cell>
                )}
                {rowData[54] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[54].value}
                        >
                            {rowData[54].value}
                        </div>
                    </Cell>
                )}{rowData[55] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[55].value}
                        >
                            {rowData[55].value}
                        </div>
                    </Cell>
                )}
                {rowData[56] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[56].value}
                        >
                            {rowData[56].value}
                        </div>
                    </Cell>
                )}
                {rowData[57] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[57].value}
                        >
                            {rowData[57].value}
                        </div>
                    </Cell>
                )}
                {rowData[58] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[58].value}
                        >
                            {rowData[58].value}
                        </div>
                    </Cell>
                )}
                {rowData[59] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[59].value}
                        >
                            {rowData[59].value}
                        </div>
                    </Cell>
                )}
                {rowData[60] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[60].value}
                        >
                            {rowData[60].value}
                        </div>
                    </Cell>
                )}
                {rowData[61] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[61].value}
                        >
                            {rowData[61].value}
                        </div>
                    </Cell>
                )}
                {rowData[62] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[62].value}
                        >
                            {rowData[62].value}
                        </div>
                    </Cell>
                )}
                {rowData[63] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[63].value}
                        >
                            {rowData[63].value}
                        </div>
                    </Cell>
                )}
                {rowData[64] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[64].value}
                        >
                            {rowData[64].value}
                        </div>
                    </Cell>
                )}
                {rowData[65] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[65].value}
                        >
                            {rowData[65].value}
                        </div>
                    </Cell>
                )}
                {rowData[66] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[66].value}
                        >
                            {rowData[66].value}
                        </div>
                    </Cell>
                )}
                {rowData[67] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[67].value}
                        >
                            {rowData[67].value}
                        </div>
                    </Cell>
                )}
                {rowData[68] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[68].value}
                        >
                            {rowData[68].value}
                        </div>
                    </Cell>
                )}
                {rowData[69] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[69].value}
                        >
                            {rowData[69].value}
                        </div>
                    </Cell>
                )}
                {rowData[70] && (
                    <Cell>
                        <div
                            className="table-flex"
                            data-tooltip="ifneed"
                            aria-label={rowData[70].value}
                        >
                            {rowData[70].value}
                        </div>
                    </Cell>
                )}
            </Row>
        );
    }
}

export default class GoogleDataTable extends R.Component {
    componentCreate() {
        this.state = {
            items: this.props.items,
            isDesc: -1,
            columns: this.props.columns,
            tableItems: DataTableUtil.getItems(this.props.items, this.props.columns),
            tableColumns: DataTableUtil.getColumns(this.props.columns),
            tableId: `reco-discovery-${new Date().getMinutes()}-${new Date().getSeconds()}`,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const columns = nextProps.columns;
        if (columns !== prevState.columns) {
            return {
                columns: columns,
                tableColumns: DataTableUtil.getColumns(columns),
            };
        }

        const compareItemsWithoutChecked = (prevItems, nextItems) => {
            const _prevItems = _.cloneDeep(prevItems);
            const _nextItems = _.cloneDeep(nextItems);

            _prevItems.forEach(item => delete item.checked);
            _nextItems.forEach(item => delete item.checked);

            return _.isEqual(_prevItems, _nextItems);
        }

        const items = nextProps.items;
        const isItemChanged = !compareItemsWithoutChecked(items, prevState.items);
        if (isItemChanged) {
            return {
                items: items,
                tableItems: DataTableUtil.getItems(items, columns),
            };
        }

        return null;
    }

    onRowEvent = (args) => {
        switch (args.type) {
            case "onCellClick":
                this.props.onJoinInToContainer(args.rowData);
                break;
            default:
                break;
        }
    };

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];

        items.forEach(i => {
            let find = false;
            item.forEach(j => {
                if (item.length > 0 && i.id === j[0]) {
                    i.checked = true;
                    find = true;
                }
            });
            if (!find) {
                i.checked = false;
            }
        });

        this.setState({
            items: items,
        });

        const checkedItems = items.filter(i => i.checked);
        const checkedIds = items.filter(i => i.checked).map(i => i.id);
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedIds, checkedItems);
    };

    onCheckedSelectAll = () => {
        const needUpdateSelectedStatus = !this.state.isCheckedSelectedAll;

        const items = [...this.state.items];
        items.forEach((item) => {
            item.checked = needUpdateSelectedStatus;
        });

        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            // items: items,
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked();
    };

    doSort = (column) => {
        this.props.onSort(column);
    }

    footer = () => {
        return [
            <span className="total-aggregate-item" data-tooltip="ifneed" key={0}>
                {RMResx.RM_FA_Tabel_Total}
            </span>,
            ...this.state.columns.map((item, index) => {
                if (!item.isAggregateField) {
                    return;
                }

                if (item.isPlaceholder) {
                    return (
                        <span className="total-aggregate-item" data-tooltip="ifneed" key={index + 1}>
                        </span>
                    );
                }

                let value = this.props.totalAggregateInfos[item.internalName];
                if (_.isNil(value)) {
                    value = "N/A";
                }

                return (
                    <span className="total-aggregate-item" data-tooltip="ifneed" key={index + 1}>
                        {value}
                    </span>
                );
            }).filter(item=>item != undefined)
        ];
    }

    render() {
        return (
            <>
                <R.Table
                    id={this.state.tableId}
                    rowTemplate={GoogleDataTableRow}
                    items={this.state.tableItems}
                    onRowEvent={this.onRowEvent}
                    onCheck={this.onItemCheckedChange}
                    columns={this.state.tableColumns}
                    footer={this.footer}
                    checkable={Number.POSITIVE_INFINITY}
                    frozenCount={0}
                    doSort={this.doSort}
                />
            </>
        );
    }
}

class DataTableUtil {
    static getColumns = (columns) => {
        const nameColumnInfo = {
            header: columns[0].displayName,
            width: 350,
            resizeable: true,
        };
        if(columns[0].sortable) {
            nameColumnInfo.sortable = true;
            nameColumnInfo.sortField = columns[0].sortField;
        }
        const res = [
            nameColumnInfo
        ];

        for (let i = 1; i < columns.length; i++) {
            const column = columns[i];
            const columnInfo = {
                header: column.displayName,
                width: 200,
                resizeable: true,
                
            };
            if(column.sortable) {
                columnInfo.sortable = true;
                columnInfo.sortField = column.sortField;
            }
            res.push(columnInfo);
        }

        return res;
    }

    static getItems = (items, columns) => {
        const res = [];
        for (const item of items) {
            const row = [item.id];
            for (const column of columns) {
                const value = item[column.internalName];
                const isLink = column.isLink;
                if (_.isNil(value)) {
                    row.push({
                        value: "N/A",
                        isLink: false,
                    });
                } else {
                    row.push({
                        value: value,
                        isLink: isLink,
                    });
                }
            }
            row.checked = item.checked;
            res.push(row);
        }
        return res;
    };
}
