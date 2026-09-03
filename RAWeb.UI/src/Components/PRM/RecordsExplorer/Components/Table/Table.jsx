import { LocationExceptRoomAndFileRT, RoomAndBoxRT } from "./RowTemplate";
import { NodeType } from "../../../../../Constants/DAEnums";

const persistColumnWidth = (cells, uniqueStr) => {
    let columnWidths = window["columnWidths"];
    return cells.map((cell) => {
        const columnUniqueId = `${uniqueStr}-${cell.id}`;
        if (columnWidths && (columnWidths[columnUniqueId] !== undefined)) {
            if (cell.width.length === 1) {
                cell.width.push(columnWidths[columnUniqueId], "100%");
            } else {
                cell.width[1] = columnWidths[columnUniqueId];
            }
        }
        return cell;
    });
}

var cacheData = [];
export default class Table extends R.Component {
    idAttr = true;
    componentCreate() {
        this.termPaths = {};
        this.state = {
            page: 1,
            pageSize: 10,
            disabled: false,
            isSelectAll: false,
            nameVisibility: 'show',
            items: [],
            viewLocation: false,
            showCheckbox: false,
            showActions: false,
            pagerTotalCount: 0,
            checkedItems: []
        };
        this.bind('onSelectAllChanged', 'onRowEvent', 'onPager', 'onCheckChange', 'cellOperate', 'clearSelected');
        this.state.columns = this.props.columns;
    }

    componentReceive(type, data) {
        switch (type) {
            case 'setData':
                let cellInfo = data.currentPhysicalObjectList,
                    columnInfo = data.tableColumnInfo,
                    columns = [],
                    showCheckbox = data.showCheckbox,
                    showActions = data.showActions,
                    checkedCount = 0;

                if (showActions) {
                    columns.push({
                        headerTemplate: '',
                        resizeable: true,
                        width: 60
                    });
                }
                if (showCheckbox) {
                    columns.push({
                        headerTemplate:
                            <R.Checkbox
                                checked={this.state.isSelectAll}
                                disabled={this.state.disabled}
                                onChange={this.onSelectAllChanged}
                            />,
                        width: 60
                    });
                }
                columns = persistColumnWidth([...columns, ...columnInfo], 'physical-records');
                for (let item of cellInfo) {
                    for (let inItem of cacheData) {
                        if (item.Id == inItem.Id) {
                            item.isChecked = inItem.isChecked;
                        }
                    }
                }
                for (let item of cellInfo) {
                    if (item.isChecked) {
                        checkedCount++;
                    }
                }
                this.setState({
                    columns: columns,
                    items: cellInfo,
                    curNodeType: data.curNodeType,
                    showCheckbox: showCheckbox,
                    showActions: showActions,
                    pagerTotalCount: data.pagerTotalCount
                }, () => {
                    if (showCheckbox) {
                        if (checkedCount && cellInfo.length == checkedCount) {
                            this.updateSelectAll(true);
                        } else if (checkedCount == 0) {
                            this.updateSelectAll(false);
                        } else {
                            this.updateSelectAll(null);
                        }
                    }
                });
                break;
            case 'reset':
                this.resetTable(data);
                this.setState({ checkedItems: [] });
                break;
        }
    }

    resetTable(args) {
        if (args) {
            args.isChecked = false;
            this.onCheckChange(args);
        } else {
            cacheData = [];
            for (let item of this.state.items) {
                item.isChecked = false;
            }
            this.onCheckChange(this.state.items);
        }
    }

    updateSelectAll(checked) {
        if (this.state.showCheckbox) {
            // let ckIdx = this.state.showActions ? 1 : 0;
            this.state.columns[0] = Object.assign({}, this.state.columns[0], {
                width: 60,
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

    onSelectAllChanged(checked) {
        var isHasCurrentItem = false;
        let checkedItems = [];
        this.state.items.forEach(item => item.isChecked = checked);
        this.updateSelectAll(checked);
        this.setState({ isSelectAll: checked, items: this.state.items.slice() });
        for (let item of this.state.items) {
            for (let inItem of cacheData) {
                if (item.Id == inItem.Id) {
                    isHasCurrentItem = true;
                    inItem.isChecked = checked;
                }
            }
            if (!isHasCurrentItem) {
                cacheData.push(item);
            }
            isHasCurrentItem = false;
        }
        cacheData.forEach((item) => {
            if (item.isChecked) {
                checkedItems.push(item);
            }
        });
        this.setState({ checkedItems: checkedItems });
        this.props.onCheckChange(checkedItems);
    }

    onCheckChange(rowData) {
        let isAll = false;
        let isNone = true;
        let isHasCurrentItem = false;
        this.setState({
            items: JSON.parse(JSON.stringify(this.state.items))
        }, () => {
            isAll = this.state.items.length > 0 && this.state.items.every(item => item.isChecked);
            isNone = this.state.items.every(item => !item.isChecked);
            if (isAll) {
                this.updateSelectAll(isAll);
            } else if (isNone) {
                this.updateSelectAll(false);
            } else {
                this.updateSelectAll(null);
            }
        });
        if (cacheData.length > 0) {
            for (let item of cacheData) {
                if (item.Id == rowData.Id) {
                    item.isChecked = rowData.isChecked;
                    isHasCurrentItem = true;
                    break;
                }
            }
            if (!isHasCurrentItem) {
                cacheData.push(rowData);
            }
        } else {
            cacheData.push(rowData);
        }
        let checkedItems = [];
        for (let obj of cacheData) {
            if (obj.isChecked) {
                checkedItems.push(obj);
            }
        }
        this.setState({ checkedItems: checkedItems });
        this.props.onCheckChange(checkedItems);
    }

    clearSelected() {
        this.resetTable();
        this.setState({ checkedItems: [] });
    }

    cellClick(rowData) {
        this.props.cellClick(rowData);
    }

    onRowEvent(args, selectedOption) {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellOperate':
                this.cellOperate(rowData, selectedOption);
                break;
            case 'checked':
                this.onCheckChange(rowData);
                break;
            case 'cellClick':
                this.cellClick(rowData);
                break;
            case 'showTermFullPath':
                this.setTermFullPath(args);
                break;
            default:
                break;
        }
    }

    setTermFullPath(args) {
        let option = {
            method: "GET",
            url: `/api/TermManagementApi/GetTermWithPath/?termId=${args.rowData.TermId}`
        };
        let termId = args.rowData.TermId;
        if (this.termPaths[termId]) {
            this.state.items[args.rowIndex].IsShowTermFullPath = true;
            this.state.items[args.rowIndex].TermFullPath = this.termPaths[termId];
            this.setState({
                items: JSON.parse(JSON.stringify(this.state.items))
            });
        } else {
            fetchUtility(option).then((res) => {
                let data = JSON.parse(res);
                this.state.items[args.rowIndex].IsShowTermFullPath = true;
                this.state.items[args.rowIndex].TermFullPath = data.FullPath;
                this.termPaths[termId] = data.FullPath;
                this.setState({
                    items: JSON.parse(JSON.stringify(this.state.items)),
                });
            }).catch((e) => {
            });
        }
    }

    cellOperate(rowData, selectedOption) {
        this.props.cellOperate(rowData, selectedOption);
    }

    onPager(e, args) {
        var page = args.newValue.selectedPage,
            pageSize = args.newValue.pageSize;
        this.setState({ page, pageSize, items: [] });
    }

    onColumnResize = (col, width) => {
        const columnId = `physical-records-${col.id}`;
        let columnWidths = window["columnWidths"] || {};
        columnWidths[columnId] = width;
        window["columnWidths"] = columnWidths;
    };

    renderPagerCounter() {
        let pagerCounterContent = <span tabIndex="0">{RMResx.RM_Common_TotalCount.format(this.state.pagerTotalCount)}</span>;
        if (this.state.curNodeType >= NodeType.PhysicalBottomLocation) {
            let checkedItemsCount = this.state.checkedItems.length;
            if (checkedItemsCount > 0) {
                pagerCounterContent = <React.Fragment>
                    <span>{RMResx.RM_Common_SelectedAndTotalCount.format(checkedItemsCount, this.state.pagerTotalCount)}</span>
                    {this.props.isShowClear && <a
                        className="ra-link-a"
                        tabIndex='0'
                        role='button'
                        onClick={this.clearSelected}>
                        {RMResx.RM_JS_JM_ClearSelected}
                    </a>}
                </React.Fragment>;
            }
        }
        return pagerCounterContent;
    }

    render() {
        let RowTemplate = [NodeType.PhysicalBottomLocation,NodeType.PhyBox, NodeType.PhyCustom].includes(this.state.curNodeType)
            ? RoomAndBoxRT : LocationExceptRoomAndFileRT;
        return (
            <div id={this.props.id}>
                <R.Table
                    id="table1"
                    minHeight={200}
                    disabled={this.state.disabled}
                    frozenCount={this.state.showCheckbox ? 1 : 0}
                    rootData={{
                        showCheckbox: this.state.showCheckbox,
                        showActions: this.state.showActions,
                        curNodeType: this.state.curNodeType
                    }}
                    columns={this.state.columns}
                    rowTemplate={RowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    onColumnResize={this.onColumnResize}
                />
                <div className='ra-table-pager-counter'>
                    {this.renderPagerCounter()}
                </div>
            </div>
        );
    }
}
