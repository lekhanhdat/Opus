import { bindEvents, getMulticomboboxAllItems } from "../../../Utilities/CommonUtil";
import { Template } from "./TableTemplate";
import { AuditReportColumnInfo, AuditReportcolumnsWidth } from "../Constants";
import "../../../Less/RC/auditReport.less";
import FilterForm from "./FilterForm";

export default class Details extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        bindEvents(this, "managedColumnChanged", "onShowFilter", "onClearFilter", "onHide",
            "onFilter", "filterSaveClick", "closeFilterPanel", "onTimeSort", "onPageChange");
        this.detailParam = {
            Range: 0,
            StartTime: new Date(),
            EndTime: new Date(),
            viewBy: 0,
            viewByValue: null
        };
        this.sortBy = 0;
        this.isAscending = null;

        this.state = {
            columns: [],
            items: [],
            filterPanelTitle: RMResx.RM_Common_Filter,
            showFilterPanel: false,
            filterData: {},
            pagerCount: 0,
            pagerIndex: 0,
            pagerSize: 10,
            managedColumns: this.getManagedColumns(),
        };
    }

    getManagedColumns() {
        return [
            { isChecked: true, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_Time, Id: 0 },
            { isChecked: true, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_User, Id: 1 },
            { isChecked: true, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_Module, Id: 2 },
            // { isChecked: false, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_Function, Id: 3 },
            { isChecked: true, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_Action, Id: 3 },
            { isChecked: false, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_Object, Id: 4 },
            { isChecked: false, value: RMResx.RM_JS_RC_Audit_ManageCol_NewVal, Id: 5 },
            { isChecked: false, value: RMResx.RM_JS_RC_Audit_ManageCol_OldVal, Id: 6 },
            { isChecked: true, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_Status, Id: 7 },
            { isChecked: false, value: RMResx.RM_JS_RC_Audit_ViewBy_Option_ClientIP, Id: 8 }
        ];
    }

    componentInit() {
        this.setTableColumn();
    }

    componentReceive(type, data) {
        if (type == "selectTime") {
            this.detailParam = data;
            this.setState({
                pagerIndex : 0,
            });
        }
        this.setTableItems();
    }

    setTableColumn() {
        let tableColumns = [];
        let managedColumns = this.state.managedColumns;
        for (let key in AuditReportColumnInfo) {
            if (AuditReportColumnInfo.hasOwnProperty(key)) {
                let columnObj = {};
                columnObj.header = AuditReportColumnInfo[key];
                columnObj.width = AuditReportcolumnsWidth[key];
                columnObj.resizeable = true;
                tableColumns.push(columnObj);
            }
        }
        for (let idx in managedColumns) {
            if (idx == 0) {
                tableColumns[idx].sortable = true;
            }
            if (managedColumns[idx].isChecked) {
                tableColumns[idx].visible = true;
            } else {
                tableColumns[idx].visible = false;
            }
        }
        this.setState({ columns: tableColumns });
    }

    onShowFilter() {
        this.setState({ showFilterPanel: true });
    }

    closeFilterPanel() {
        this.setState({ showFilterPanel: false });
    }

    onTimeSort(args) {
        this.sortBy = args.columnIndex - 1;
        this.isAscending = args.status == "asc" ? true : false;
        this.setTableItems();
    }

    sortFun(property) {
        return function (item1, item2) {
            var value1 = item1[property];
            var value2 = item2[property];
            if (value1 < value2) {
                return -1;
            } else if (value1 > value2) {
                return 1;
            } else {
                return 0;
            }
        };
    }

    onFilter() {
        let isClear = false;
        this.dispatch('filter', isClear);
        this.setState({ showFilterPanel: false });
    }

    onClearFilter() {
        let isClear = true;
        this.dispatch('filter', isClear);
        this.setState({ showFilterPanel: false });
    }

    onHide() {
        this.setState({ showFilterPanel: false });
    }

    filterSaveClick(data, needRefresh = true) {
        this.setState({
            filterData: data,
            pagerIndex: 0
        }, () => {
            if(needRefresh) {
                this.setTableItems();
            }
        });
    }

    managedColumnChanged(args) {
        const allItems = this.getManagedColumns();
        this.setState({ managedColumns: getMulticomboboxAllItems(args.newValue, allItems, "Id") }, () => {
            this.setTableColumn();
        });
    }

    setTableItems() {
        $$.loading(true);
        let url = `/api/AuditApi/GetTableInfo`;
        let postData = {
            "Range": this.detailParam.Range,
            "StartTime": this.detailParam.StartTime,
            "EndTime": this.detailParam.EndTime,
            "PageIndex": this.state.pagerIndex + 1,
            "PageSize": this.state.pagerSize,
            "ViewBy": this.detailParam.viewBy,
            "ViewByValue": this.detailParam.viewByValue,
            "IsAscending": this.isAscending,
            "SortBy": this.sortBy,
            "FilterInfos": this.state.filterData
        };
        let option = {
            url: url,
            method: "POST",
            data: postData
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setState({
                items: res.TableInfo || [],
                // pagerCount: res.PageCount * this.state.pagerSize,
                pagerCount: res.PageCount,
            });
        });
    }

    onPageChange(index, size, callback) {
        this.setState({
            pagerIndex: index,
            pagerSize: size
        }, () => {
            this.setTableItems();
        });
        if (callback) {
            callback(true);
        }
    }

    renderTableBar() {
        return <div className='ra-tableBar'>
            <div className='ra-tableBar-content'>
                <div className='pull-left'>
                    <R.Multicombobox
                        height={34}
                        checkedField="isChecked"
                        textField="value"
                        valueField="Id"
                        hasFilter={false}
                        required={true}
                        hasSelectAll={false}
                        items={this.state.managedColumns}
                        noneText="Manage Columns"
                        onChange={this.managedColumnChanged}
                        triggerBySource={true}
                    />
                </div>
                <div className='ra-spliter'></div>
                <div className='pull-left'>
                    <R.Button
                        type="icon"
                        tooltip={RMResx.RM_PRM_PRE_Filter}
                        icon="fia-filter"
                        onClick={this.onShowFilter} />
                </div>
            </div>
        </div>;
    }

    renderTable() {
        return <R.Table
            id="raAuditReportTable"
            disabled={false}
            rootData={this.state.rootData}
            columns={this.state.columns}
            rowTemplate={Template}
            items={this.state.items}
            doSort={this.onTimeSort}
        />;
    }

    renderPager() {
        return <div className="ra-table-pager">
            <div className='pull-right'>
                <$g.Pager
                    itemsCount={this.state.pagerCount}
                    pagerIndex={this.state.pagerIndex}
                    pagerSize={this.state.pagerSize}
                    showPagerSize={true}
                    pagerSizeOptions={[5, 10, 15]}
                    onChange={this.onPageChange} />
            </div>
        </div>;
    }

    renderFilterPanel() {
        return <R.Panel
            id="filterPanel"
            header={this.state.filterPanelTitle}
            size={400}
            status={{ show: this.state.showFilterPanel }}
            destroy={true}
            onHide={this.onHide}
        >
            <div>
                <FilterForm
                    id='filter'
                    onSave={this.filterSaveClick}
                    data={this.state.filterData}
                ></FilterForm>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.closeFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onFilter} />
            </>
        </R.Panel>;
    }

    render() {
        return (
            <div className="reco-audit-detail-wrapper" id={this.props.id}>
                <div className="reco-audit-detail-action">
                    <R.Button
                        type="button"
                        primary={false}
                        classify="default"
                        tooltip={RMResx.RM_PRM_PRE_Filter}
                        icon="fia-filter"
                        text={RMResx.RM_RC_Btn_Filter}
                        onClick={this.onShowFilter} />
                    <span>
                        <R.Multicombobox
                            height={34}
                            checkedField="isChecked"
                            textField="value"
                            valueField="Id"
                            hasFilter={false}
                            required={true}
                            hasSelectAll={false}
                            items={this.state.managedColumns}
                            noneText="Manage Columns"
                            onChange={this.managedColumnChanged}
                            triggerBySource={true}
                            customTrigger={true}
                        >
                            <R.Button
                                text={RMResx.RM_RC_Btn_ManageColumn}
                                icon="fia-manage-column"
                                primary={false}
                                classify="default"
                            >
                            </R.Button>
                        </R.Multicombobox>
                    </span>
                </div>
                <R.Table
                    id="reco-audit-detail-table"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={Template}
                    items={this.state.items}
                    flexible={false}
                    doSort={this.onTimeSort}
                />
                <div className="reco-audit-detail-footer-section" key={new Date().getTime()}>
                    <$g.Pager
                        itemsCount={this.state.pagerCount}
                        pagerIndex={this.state.pagerIndex}
                        pagerSize={this.state.pagerSize}
                        showPagerSize={true}
                        showPagerCounter={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.onPageChange}
                    />
                </div>
                {this.renderFilterPanel()}
            </div>
        );
    }
}
