import TableRow from './ElecTemplate';
import { NodeType } from "../../../Constants/DAEnums";

export default class Table extends R.Component {
    idAttr = true;
    componentCreate() {
        this.cacheData = [];
        this.termPaths = {};
        this.state = {
            page: 1,
            pageSize: 10,
            isSelectAll: false,
            items: [],
            columns: [],
            pagerTotalCount: 0,
            checkedItem: []
        };
        this.bind('onSelectAllChanged', 'onRowEvent', 'clearSelected');
    }

    componentReceive(isReset, data, columns, pagerTotalCount) {
        if (isReset) {
            this.cacheData = [];
            this.setState({ checkedItem: [] });
        }
        this.setState({
            items: this.initCellCheckBoxStatus(data),
            columns: this.getColumns(columns),
            pagerTotalCount: pagerTotalCount
        }, () => {
            this.initSelectAllStatus(this.state.items);
        });
    }

    getColumns(columns) {
        return [{
            headerTemplate: '',
            align: "center",
            resizeable: true,
            width: 50
        }, {
            headerTemplate: <R.Checkbox checked={this.state.isSelectAll} onChange={this.onSelectAllChanged} />,
            align: "center",
            resizeable: true,
            width: 50
        }, ...columns];
    }

    initCellCheckBoxStatus(data) {
        for (let item of data) {
            for (let inItem of this.cacheData) {
                if (item.Id == inItem.Id) {
                    item.isChecked = inItem.isChecked;
                }
            }
        }
        return data;
    }

    initSelectAllStatus(data) {
        let checkedCount = 0;
        let isSelectedAll = false;
        for (let item of data) {
            if (item.isChecked) {
                checkedCount++;
            }
        }
        if (checkedCount && data.length == checkedCount) {
            isSelectedAll = true;
        } else if (checkedCount == 0) {
            isSelectedAll = false;
        } else {
            isSelectedAll = null;
        }
        this.updateSelectAll(isSelectedAll);
    }

    updateSelectAll(checked) {
        this.state.columns[1] = Object.assign({}, this.state.columns[1], {
            headerTemplate: <R.Checkbox checked={checked} onChange={this.onSelectAllChanged} />
        });
        this.setState({ isSelectAll: checked, columns: this.state.columns.slice() });
    }

    onSelectAllChanged(checked) {
        let isHasCurrentItem = false;
        let checkedItem = [];
        this.state.items.forEach(item => item.isChecked = checked);
        this.updateSelectAll(checked);
        this.setState({ isSelectAll: checked, items: this.state.items.slice() });
        for (let item of this.state.items) {
            for (let inItem of this.cacheData) {
                if (item.Id == inItem.Id) {
                    isHasCurrentItem = true;
                    inItem.isChecked = checked;
                }
            }
            if (!isHasCurrentItem) {
                this.cacheData.push(item);
            }
            isHasCurrentItem = false;
        }
        this.cacheData.filter((item) => {
            if (item.isChecked) {
                checkedItem.push(item);
            }
        });
        this.setState({ checkedItem: checkedItem });
        this.props.onCheckChange(checkedItem);
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
        if (this.cacheData.length > 0) {
            for (let item of this.cacheData) {
                if (item.Id == rowData.Id) {
                    item.isChecked = rowData.isChecked;
                    isHasCurrentItem = true;
                    break;
                }
            }
            if (!isHasCurrentItem) {
                this.cacheData.push(rowData);
            }
        } else {
            this.cacheData.push(rowData);
        }
        let checkedItem = [];
        for (let obj of this.cacheData) {
            if (obj.isChecked) {
                checkedItem.push(obj);
            }
        }
        this.setState({ checkedItem: checkedItem });
        this.props.onCheckChange(checkedItem);
    }

    onRowEvent = (args, selectedOption) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData, selectedOption);
                break;
            case 'checked':
                this.onCheckChange(rowData);
                break;
            case 'cellOperate':
                this.cellOperate(rowData, selectedOption);
                break;
            case 'showTermFullPath':
                this.setTermFullPath(args);
                break;
            default:
                break;
        }
    };

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

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
    }

    clearSelected() {
        this.cacheData = [];
        for (let item of this.state.items) {
            item.isChecked = false;
        }
        this.setState({ checkedItem: [] });
        this.onCheckChange(this.state.items);
    }

    renderPagerCounter() {
        let checkedItemsCount = this.state.checkedItem.length;
        let pagerCounterContent = <span>{RMResx.RM_Common_TotalCount.format(this.state.pagerTotalCount)}</span>;
        if (checkedItemsCount > 0) {
            pagerCounterContent = <React.Fragment>
                <span>{RMResx.RM_Common_SelectedAndTotalCount.format(checkedItemsCount, this.state.pagerTotalCount)}</span>
                <a
                    className="ra-link-a"
                    tabIndex='0'
                    role='button'
                    onClick={this.clearSelected}>
                    {RMResx.RM_JS_JM_ClearSelected}
                </a>
            </React.Fragment>;
        }
        return pagerCounterContent;
    }

    render() {

        return (
            <div id={this.props.id}>
                <R.Table
                    id="elecRecordTable"
                    frozenCount={4}
                    columns={this.state.columns}
                    rowTemplate={TableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                />
                <div className='ra-table-pager-counter'>
                    {this.renderPagerCounter()}
                </div>
            </div>
        );
    }
}