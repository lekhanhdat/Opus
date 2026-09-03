import AuditTrailFilterPanel from "./AuditTrailFilterPanel";
import { createRef } from "react";
import "../../../../Less/BCM/ContentRepositoryManagement/auditTrail.less";

const PAGE_SIZE = 10;

const NODE_LEVEL = {
    ConnectionGroup : 2,
    Connection: 100,
    Folder: 2100
}

const AUDIT_LEVEL = {
    ConnectionGroup : '1',
    Connection: '2',
    Folder: '3'
}

const LEVEL_FILTER_COLUMNS = ["ConnectionGroupId", "ConnectionId", "FullPath", "AuditLevel"];
const USER_FILTER_COLUMNS = ["ExecutedBy", "AuditType", "ExecutedTime"];

export default class AuditTrailPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.cacheFilterData = RM.getSessionStorage("AuditTrailFilterData") || {};
        const cachedData = this.getFilterFromCache();
        this.state = {
            filterData: {
                PageSize: PAGE_SIZE,
                PageIndex: 1,
                SearchKey: "",
                Filters: cachedData,
                Order: {
                    ColumnName: "",
                    IsDesc: true,
                },
            },
            auditTrail: { items: [], totalCount: 0 },
            isFiltered: cachedData.some((item) => USER_FILTER_COLUMNS.includes(item.ColumnName)),
            showFilterPanel: false,
        };
        this.filterButtonRef = createRef();
    }

    resolveFilterByLevel = ({Level, Id, FullPath}) => {
        switch (Level) {
            case NODE_LEVEL.ConnectionGroup:
                return [
                    this.buildFilter("ConnectionGroupId", Id),
                    this.buildFilter("AuditLevel", AUDIT_LEVEL.ConnectionGroup),
                ];

            case NODE_LEVEL.Connection:
                return [
                    this.buildFilter("ConnectionId", Id),
                    this.buildFilter("AuditLevel", AUDIT_LEVEL.Connection),
                ];

            case NODE_LEVEL.Folder:
                return [
                    this.buildFilter("FullPath", FullPath),
                    this.buildFilter("AuditLevel", AUDIT_LEVEL.Folder),
                ];

            default: return [];
        }
    };

    buildFilter = (columnName, value) => ({
        ColumnName: columnName,
        ColumnValues: [value],
    });

    componentReceive(type, args) {
        if (type == "initData") {
            this.cacheFilterData = {};
            RM.setSessionStorage("AuditTrailFilterData", {});
            this.cachedData = [];

            this.setState(prevState => ({
                filterData: {
                    ...prevState.filterData,
                    Filters: this.resolveFilterByLevel(args)
                },
                isFiltered: false,
            }), () => {
                this.getAuditTrailData(this.state.filterData);
            });
        }
    }

    getFilterFromCache() {
        let filters = [];
        if (this.cacheFilterData && Object.keys(this.cacheFilterData).length > 0) {
            for (let key in this.cacheFilterData) {
                filters.push({
                    ColumnName: key,
                    ColumnValues: this.cacheFilterData[key],
                });
            }
        }
        return filters;
    }

    getAuditTrailData = (payload) => {
        $$.loading(true);
        let option = {
            url: '/api/FSSettingApi/GetJPMCAuditByConnectionLevel',
            method: "POST",
            data: payload,
        };
        return fetchUtility(option).then((res) => {
            const auditData = res?.Items?.length ? res.Items.map(item => ({...item, Content: [...JSON.parse(item.Content)]})) : [];
            this.setState({ auditTrail: { items: auditData, totalCount: res?.TotalCount || 0 } });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getViewPermissionColumns = () => {
        return [
            {
                header: RMResx.RM_JS_RC_Audit_ViewBy_Option_Time,
                width: 250,
                sortable: true,
                valuePath: "FormattedTime",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ViewBy_Option_User,
                width: 250,
                sortable: false,
                valuePath: "UserName",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ViewBy_Option_Action,
                width: 250,
                sortable: false,
                valuePath: "AuditTypeStr",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ViewBy_Option_Object,
                width: 250,
                sortable: false,
                valuePath: "ObjectName",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ManageCol_NewVal,
                width: 250,
                sortable: false,
                valuePath: "newValue",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ManageCol_OldVal,
                width: 250,
                sortable: false,
                valuePath: "originValue",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ViewBy_Option_Status,
                width: 250,
                sortable: false,
                valuePath: "StatusStr",
            },
            {
                header: RMResx.RM_JS_RC_Audit_ViewBy_Option_ClientIP,
                width: 250,
                sortable: false,
                valuePath: "ClientIP",
            },
        ]
    }

    handlePageChange = (pagerIndex, pagerSize, callback) => {
        this.setState({
            filterData: {
                ...this.state.filterData,
                PageIndex: pagerIndex + 1,
                PageSize: pagerSize,
            }}, () => {
                this.getAuditTrailData(this.state.filterData);
                callback(true);
            },
        );
    };

    onSort = ({status, column}) => {
        const isDescending = status == "desc";
        this.setState({
            filterData: {
                ...this.state.filterData,
                Order: {
                    ColumnName: column.valuePath,
                    IsDesc: isDescending,
                },
            }}, () => { this.getAuditTrailData(this.state.filterData);},
        );
    };

    onSearchStart = (arg) => {
        const searchKey = (arg || "").trim();

        this.setState(
            {
                filterData: {
                    ...this.state.filterData,
                    SearchKey: searchKey,
                    PageIndex: 1,
                },
            },
            () => {
                this.getAuditTrailData(this.state.filterData);
            },
        );
    };

    openFilterPanel = () => {
        this.setState({ showFilterPanel: true }, () => {
            this.dispatch("auditTrailFilterPanelId", "initData");
        });
    };

    renderHeader() {
        return (
            <div className="flex justify-between align-center margin-bottom-m">
                <div>
                    <R.Searchbox
                        placeholder={RMResx.RM_FS_AuditTrail_Search_Placeholder}
                        disabled={false}
                        onSearch={this.onSearchStart}
                        width={280}
                    />
                </div>
                <div className="flex" style={{ columnGap: "8px" }}>
                    <R.Button
                        className="filtered-button"
                        icon="fia-filter"
                        primary={this.state.isFiltered}
                        classify={this.state.isFiltered ? "theme" : "default"}
                        text={this.state.isFiltered ? RMResx.RM_MA_Filtered : RMResx.RM_Common_Filter}
                        onClick={this.openFilterPanel}
                        ref={this.filterButtonRef}
                    />
                </div>
            </div>
        );
    }

    hideFilterPanel = () => {
        this.setState({ showFilterPanel: false }, () => this.filterButtonRef?.current?.focus());
    };

    onFilter = () => {
        const callback = (filterParam) => {
            const typeNode = this.state.filterData.Filters.find(item => ["ConnectionGroupId", "ConnectionId", "FullPath"].includes(item.ColumnName));
            const levelNode = this.state.filterData.Filters.find(item => item.ColumnName === "AuditLevel");
            let clonedFilter = [typeNode, levelNode].filter(item => item);
            
            if (Object.keys(filterParam).length > 0) {
                clonedFilter = this.updateFilter(clonedFilter, "ExecutedBy", filterParam.ExecutedBy);
                clonedFilter = this.updateFilter(clonedFilter, "AuditType", filterParam.AuditType);
                clonedFilter = this.updateFilter(clonedFilter, "ExecutedTime", filterParam.ExecutedTime);
            }

            const isFiltered = clonedFilter.some((item) => USER_FILTER_COLUMNS.includes(item.ColumnName));
            this.cacheFilterData = filterParam;
            RM.setSessionStorage("AuditTrailFilterData", this.cacheFilterData);
            this.setState({ 
                filterData: { ...this.state.filterData, Filters: clonedFilter, PageIndex: 1 }, 
                isFiltered: isFiltered, 
                showFilterPanel: false 
            }, () => {
                this.getAuditTrailData(this.state.filterData);
            });
        };
        this.filterButtonRef?.current?.focus();
        this.dispatch("auditTrailFilterPanelId", "saveFilter", callback);
    };

    updateFilter = (filters, columnName, values) => {
        const hasPrevFilter = filters.some(item => item.ColumnName === columnName);
        const hasNextFilter = values?.length > 0;

        if (hasPrevFilter && hasNextFilter) {
            return filters.map(item =>
                item.ColumnName === columnName
                    ? { ...item, ColumnValues: values }
                    : item
            );
        }

        if (!hasPrevFilter && hasNextFilter) {
            return [
                ...filters,
                {
                    ColumnName: columnName,
                    ColumnValues: values,
                },
            ];
        }

        return filters;
    };
    
    renderFilterPanel() {
        return (
            <R.Panel
                header={RMResx.RM_Common_Filter}
                size={670}
                onHide={this.hideFilterPanel}
                status={{ show: this.state.showFilterPanel }}
                destroy={true}
            >
                <AuditTrailFilterPanel
                    id="auditTrailFilterPanelId"
                />
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.hideFilterPanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onFilter}
                    />
                </>
            </R.Panel>
        );
    }

    render() {
        return (
            <div className="overflow-hidden">
                {this.renderHeader()}
                <R.Table
                    id="auditTrailTable"
                    rowTemplate={TableTemplate}
                    items={this.state.auditTrail.items}
                    columns={this.getViewPermissionColumns()}
                    doSort={this.onSort}
                />
                <div className="margin-top-m">
                    <$g.Pager
                        itemsCount={this.state.auditTrail.totalCount || 0}
                        pagerIndex={this.state.filterData.PageIndex - 1}
                        pagerSize={this.state.filterData.PageSize}
                        showPagerSize={true}
                        showPagerCounter={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.handlePageChange}
                    />
                </div>
                {this.state.showFilterPanel && this.renderFilterPanel()}
            </div>
        );
    }
}

class TableTemplate extends R.TableRow {

    getOriginOrNewValue = (attr, modifyContent) => {
        return <div>
            {
                modifyContent?.map((item, index) => {
                    let value = item[attr];
                    if (value == "True") {
                        value = RMResx.RM_JS_Common_Yes;
                    } else if (value == "False") {
                        value = RMResx.RM_JS_Common_No;
                    }
                    return value && <div className="reco-audit-cell-item" key={index}>
                        {
                            item.TargetSetting &&
                            <div className='reco-audit-cell-title' data-tooltip="ifneed" aria-label={this.resetTargetSetting(item.TargetSetting)}>{this.resetTargetSetting(item.TargetSetting)}</div>
                        }
                        <div className='reco-audit-cell-value' data-tooltip="ifneed" aria-label={value}>{value}</div>
                    </div>;
                })
            }
        </div>;
    }
    
    resetTargetSetting = (str) => {
        if(str.indexOf(":") > -1)
        {
            var req =/:$/gi;
            str = str.replace(req, "");
        }
        str += ":";
        return str;
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let newValue = this.getOriginOrNewValue('NewValue', rowData.Content);
        let originValue = this.getOriginOrNewValue('OldValue', rowData.Content);

        return (
            <Row>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.FormattedTime}
                    >
                        {rowData.FormattedTime}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.UserName}
                    >
                        {rowData.UserName}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.AuditTypeStr}
                    >
                        {rowData.AuditTypeStr}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.ObjectName}
                    >
                        {rowData.ObjectName}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={newValue}
                    >
                        {newValue}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={originValue}
                    >
                        {originValue}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.StatusStr}
                    >
                        {rowData.StatusStr}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.ClientIP}
                    >
                        {rowData.ClientIP}
                    </div>
                </Cell>
            </Row>
        );
    }
}
