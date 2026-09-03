import { RowTemplate } from './RowTemplate';
import { PhysicalDefaultColumnIDs } from "../../../../Constants/Constants";
import React from 'react';

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
export default class Table extends R.Component {
    idAttr = true;
    componentCreate() {
        this.cacheData = [];
        this.termPaths = {};
        this.state = {
            isSelectAll: false,
            items: [],
            columns: [],
            rootData: {
            },
            isSelectResult: false,
            allSelectedCount: 0,
            pagerTotalCount: 0,
            noneMessage: ""
        };
    }

    componentReceive(queryDataList, columns, isReset, otherParam) {
        if (queryDataList === null && isReset === null) {
            this.setState({
                columns: this.getColumns(columns),
            });
        } else {
            let items = queryDataList.Datas;
            let rootData = this.state.rootData;
            let pagerTotalCount = queryDataList.PagingInfo.Total;
            rootData.customColums = this.props.customColums;
            rootData.isContainsPhySource = this.props.isContainsPhySource;
            if (isReset) {
                this.cacheData = [];
            }
            this.setState({
                items: this.initCellCheckBoxStatus(items, isReset),
                columns: this.getColumns(columns),
                rootData: rootData,
                allSelectedCount: this.cacheData.filter(a => a.isChecked).length,
                isSelectResult: isReset ? false : this.state.isSelectResult,
                pagerTotalCount: pagerTotalCount
            }, () => {
                this.initSelectAllStatus(this.state.items);
                this.updateCacheData(items);
                this.notifyActionDataChanged();
            });
        }

        //noSearchConditions == true代表没有search条件，table中不显示数据；此参数为了区分没有search条件 和 有search条件table中无数据时显示水印。
        if (otherParam) {
            this.setNoneMessage(otherParam.noSearchConditions, otherParam.isHasNotOfflineJob, otherParam.isHasRunningJob);
            this.setSelectAllBtnTip(otherParam.isOneSourcePermission);
        }
    }

    setNoneMessage(noSearchConditions, isHasNotOfflineJob, isHasRunningJob) {
        let noneMessage = noSearchConditions ? RMResx.RM_HS_InitNoDataTableMsg : RMResx.RM_HS_NoSearchResultTableMsg;
        if(isHasNotOfflineJob && isHasRunningJob){
            noneMessage = RMResx.RM_HS_Offline_QueryNotCompleted;
        }
        if(isHasNotOfflineJob && !isHasRunningJob){
            noneMessage = RMResx.RM_HS_Offline_NotHasJob;
        }
        this.setState({ noneMessage: noneMessage });
    }

    setSelectAllBtnTip(isOneSourcePermission) {
        this.setState({ isOneSourcePermission: isOneSourcePermission });
    }

    updateCacheData(data) {
        let isExist = false;
        for (let item of data) {
            for (let inItem of this.cacheData) {
                if (item.Id == inItem.Id) {
                    isExist = true;
                }
            }
            if (!isExist) {
                item.isChecked = this.state.isSelectResult;
                this.cacheData.push(item);
            }
            isExist = false;
        }
    }

    getColumns(columns) {
        let newColumns = persistColumnWidth([...columns], 'global-search');

        newColumns = newColumns.map((item) => {
            if (item.headerTooltip) {
                return {
                    ...item,
                    headerTemplate:
                        <div className="flex align-center">
                            {item.header}
                            <$g.Popover>{item.headerTooltip}</$g.Popover>
                        </div>,
                }
            }

            return item;
        })

        return [
            {
                headerTemplate:
                    <R.Checkbox
                        checked={this.state.isSelectAll}
                        onChange={this.onSelectAllChanged}
                    />,
                width: 50
            }, ...newColumns];
    }

    initCellCheckBoxStatus(data, isReset) {
        for (let item of data) {
            for (let inItem of this.cacheData) {
                if (item.Id == inItem.Id) {
                    item.isChecked = inItem.isChecked;
                }
            }
            if(this.state.isSelectResult){
                item.isChecked = true;
            }
            if(isReset){
                item.isChecked = false;
            }
        }
        return data;
    }

    initSelectAllStatus(data) {
        let isAll = false;
        if (data && data.length > 0) {
            isAll = data.every(r => r.isChecked);
            if (isAll) {
                this.updateSelectAll(true);
            } else {
                let isAllUnchecked = data.every(r => !r.isChecked);
                this.updateSelectAll(isAllUnchecked ? false : 'mixed');
            }
        } else {
            this.updateSelectAll(false);
        }
    }

    updateSelectAll(checked) {
        if (this.state.isSelectResult) {
            checked = true;
        }
        this.state.columns[0] = Object.assign({}, this.state.columns[0], {
            headerTemplate:
                <R.Checkbox
                    checked={checked}
                    onChange={this.onSelectAllChanged}
                />
        });
        this.setState({ isSelectAll: checked, columns: this.state.columns.slice() });
    }

    onSelectAllChanged = (checked) => {
        let isExist = false;
        this.state.items.forEach(item => item.isChecked = checked);
        this.updateSelectAll(checked);
        this.setState({ items: this.state.items.slice() });
        for (let item of this.state.items) {
            for (let inItem of this.cacheData) {
                if (item.Id == inItem.Id) {
                    isExist = true;
                    inItem.isChecked = checked;
                }
            }
            if (!isExist) {
                this.cacheData.push(item);
            }
            isExist = false;
        }
        this.setState({
            isSelectResult: false,
            allSelectedCount: this.cacheData.filter(a => a.isChecked).length
        }, () => {
            this.notifyActionDataChanged();
        });
    }

    notifyActionDataChanged() {
        let checkedItem = [];
        this.cacheData.filter((item) => {
            if (item.isChecked) {
                checkedItem.push(item);
            }
        });
        this.props.onCheckChanged(checkedItem, this.state.isSelectResult);
    }

    onCheckChanged = (rowData) => {
        let isAll = false;
        let isNone = true;
        let isExist = false;
        if (this.cacheData.length > 0) {
            for (let item of this.cacheData) {
                if (item.Id == rowData.Id) {
                    item.isChecked = rowData.isChecked;
                    isExist = true;
                    break;
                }
            }
            if (!isExist) {
                this.cacheData.push(rowData);
            }
        } else {
            this.cacheData.push(rowData);
        }

        this.setState({
            items: JSON.parse(JSON.stringify(this.state.items)),
            isSelectResult: false,
            allSelectedCount: this.cacheData.filter(a => a.isChecked).length
        }, () => {
            isAll = this.state.items.length > 0 && this.state.items.every(item => item.isChecked);
            isNone = this.state.items.every(item => !item.isChecked);
            if (isAll) {
                this.updateSelectAll(isAll);
            } else if (isNone) {
                this.updateSelectAll(false);
            } else {
                this.updateSelectAll('mixed');
            }
            this.notifyActionDataChanged();
        });
    }

    onRowEvent = (args, selectedOption) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData, selectedOption);
                break;
            case 'checked':
                this.onCheckChanged(rowData);
                break;
            case 'showTermFullPath':
                this.setTermFullPath(args);
                break;
            case 'showHomeLocationFullPath':
                this.showHomeLocationFullPath(args);
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

    showHomeLocationFullPath(args) {
        let option = {
            method: "GET",
            url: `/api/PhysicalRecordApi/GetPhysicalObjectFullPathById/?id=${args.rowData.Id}`
        };
        fetchUtility(option).then((res) => {
            this.state.items[args.rowIndex].CustomColumnDic[PhysicalDefaultColumnIDs.HomeLocation].IsShowToolValue = true;
            this.state.items[args.rowIndex].CustomColumnDic[PhysicalDefaultColumnIDs.HomeLocation].ToolValue = res;
            this.setState({
                items: JSON.parse(JSON.stringify(this.state.items)),
            });
        }).catch((e) => {
        });
    }

    cellClick = (data, selectedOption) => {
        this.props.cellClick(data, selectedOption);
    }

    onSort = (args) => {
        this.props.onSort(args.status === "asc", args.column.valuePath);
    }


    selectResult = (checked) => {
        for (let inItem of this.cacheData) {
            inItem.isChecked = checked;
        }
        this.setState({
            isSelectResult: checked,
            items: this.state.items.slice(),
            allSelectedCount: this.cacheData.filter(a => a.isChecked).length
        }, () => {
            this.notifyActionDataChanged();
            this.updateSelectAll(checked);
        });
        this.state.items.forEach(item => item.isChecked = checked);
    }

    onSelectResult = () => {
        this.selectResult(true);
    }

    clearSelectedResult = () => {
        this.selectResult(false);
    }

    onSelectResultByKeyDown = (e) =>{
        if (e.keyCode == 13 || e.keyCode == 32) {
            e.target.click();
        }
    }

    onColumnResize = (col, width) => {
        const columnId = `global-search-${col.id}`;
        let columnWidths = window["columnWidths"] || {};
        columnWidths[columnId] = width;
        window["columnWidths"] = columnWidths;
    };

    render() {
        let frozenCount = this.state.items && this.state.items.length > 0 ? 1 : 0;
        let isShowSelectAllBtnTip = !this.state.isOneSourcePermission;
        return (
            <div>
                <div className="ra-main-table">
                    <R.Table
                        id="tableCom"
                        frozenCount={frozenCount}
                        rootData={this.state.rootData}
                        flexible={false}
                        columns={this.state.columns}
                        rowTemplate={RowTemplate}
                        items={this.state.items}
                        noneMessage={this.state.noneMessage}
                        onRowEvent={this.onRowEvent}
                        doSort={this.onSort}
                        onColumnResize={this.onColumnResize}
                    />
                </div>
                <div className="ra-main-footer">
                    <div tabIndex="0" className="flex ra-flex-align-center">
                        {/* {this.state.allSelectedCount == 0 &&
                            <span>{RMResx.RM_Common_TotalCount.format(this.state.pagerTotalCount)}</span>
                        } */}
                        {!this.state.isSelectResult && this.state.pagerTotalCount != 0 &&
                            <React.Fragment>
                                <a className='ra-main-italics-link' tabIndex='0' role='button' onClick={this.onSelectResult} onKeyDown={this.onSelectResultByKeyDown}>{RMResx.RM_PRM_PRE_GlobalSearch_SelectAllResult}</a>
                                { isShowSelectAllBtnTip && <$g.Popover>{RMResx.RM_HS_Tip_SelectedAllAction}</$g.Popover> }
                            </React.Fragment>
                        }
                        {this.state.isSelectResult &&
                            <span className="ra-main-selected-counter">{RMResx.RM_PRM_PRE_GlobalSearch_ResultSelected}</span>
                        }
                        {this.state.isSelectResult &&
                            <a className='ra-main-italics-link margin-left-xs' tabIndex='0' role='button' onClick={this.clearSelectedResult} onKeyDown={this.onSelectResultByKeyDown}>{RMResx.RM_PRM_PRE_GlobalSearch_AllResultClear}</a>
                        }
                    </div>
                    {this.props.children}
                </div>
            </div>
        );
    }
}