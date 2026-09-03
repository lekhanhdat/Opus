import { DataTableRow } from "../../../../Components/DiscoveryDataView/DataTable";
import "./index.less";
import _ from "lodash";

export default class SFDataTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            items: this.props.items,
            isDesc: -1,
            columns: this.props.columns,
            tableColumns: DataTableUtil.getColumns(this.props.columns),
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

        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                items: items,
            };
        }

        return null;
    }

    onRowEvent = (args) => {
        switch (args.type) {
            case "onCellClick":
                this.props?.onJoinInToContainer(args.rowData);
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
            items: items,
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked();
    };

    getItems = (items, columns) => {
        const res = [];
        for (const item of items) {
            const row = [item.id];
            for (const column of columns) {
                const value = item[column.internalName];
                const isLink = column.isLink;
                if (_.isNil(value)) {
                    row.push({
                        value: "",
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
                    value = "";
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
                    id={this.props.id}
                    rowTemplate={DataTableRow}
                    items={this.getItems(this.state.items, this.state.columns)}
                    onRowEvent={this.onRowEvent}
                    onCheck={this.onItemCheckedChange}
                    columns={this.state.tableColumns}
                    footer={this.footer}
                    checkable={true}
                    frozenCount={0}
                    doSort={this.doSort}
                    onCheckByItems={false}
                />
            </>
        );
    }
}

class DataTableUtil {
    static getColumns = (columns) => {
        const nameColumnInfo = {
            header: columns[0].displayName,
            width: 150,
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
}
