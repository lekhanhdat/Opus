import { useState } from "react";
import "./index.less";
import { SimplePager } from "../../../../../Common/Pager";
import ProgressRequester from "../../../requests/ProgressRequester";
import { useEffect } from "react";
import Detial from "./Detial";
import { useRef } from "react";
import { showToast } from "../../../../../../Utilities/CommonUtil";

class DataTableRow extends R.TableRow {
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
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.optimizingTime}
                    >
                        <a
                            tabIndex={0}
                            onClick={this.onCellClick}
                            onKeyDown={this.onCellKeyDown}
                        >
                            {rowData.optimizingTime}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.scope}
                    >
                        {rowData.scope}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.timeRange}
                    >
                        {rowData.timeRange}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.sizeRange}
                    >
                        {rowData.sizeRange}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.fileType}
                    >
                        {rowData.fileType}
                    </div>
                </Cell>
            </Row>
        );
    }
}

class DataTable extends R.Component {
    componentCreate() {
        this.state = {
            items: this.props.items,
            tableColumns: [
                {
                    header: RMResx.RM_FA_Progress_PlannedTab_OptimizingTimeTitle,
                    width: 350,
                    resizeable: true,
                },
                {
                    header: RMResx.RM_FA_Progress_PlannedTab_ScopeTitle,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: RMResx.RM_FA_Inactive_ModifiedTitle,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: RMResx.RM_FA_Progress_PlannedTab_SizeRangeTitle,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: RMResx.RM_FA_Inactive_OptimizationTab_FileCategoryTitle,
                    width: 250,
                    resizeable: true,
                },
            ],
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
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
                this.props.onItemClick(args.rowData);
                break;
            default:
                break;
        }
    };

    onItemCheckedChange = (checkedItem) => {
        const checkedId = checkedItem.uniqueId;
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedId);
    };

    render() {
        return (
            <>
                <R.Table
                    id={"reco-discovery-planned-table"}
                    rowTemplate={DataTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.state.tableColumns}
                    onCheck={this.onItemCheckedChange}
                    checkable={1}
                    frozenCount={0}
                    onCheckByItems={false}
                />
            </>
        );
    }
}

const Planned = ({ o365TenantId }) => {
    const detailRef = useRef(null);

    const [checkedUniqueId, setCheckedUniqueId] = useState(null);

    const [totalCount, setTotalCount] = useState(0);

    const [items, setItems] = useState([]);

    const [pageInfo, setPageInfo] = useState({
        pageIndex: 0,
        pageSize: 10,
    });

    useEffect(() => {
        if (_.isNil(o365TenantId)) {
            return;
        }

        setPageInfo({
            pageIndex: 0,
            pageSize: pageInfo.pageSize,
        });
    }, [o365TenantId]);

    useEffect(() => {
        const fetchData = async () => {
            if (_.isNil(o365TenantId)) {
                return;
            }
            const res = await ProgressRequester.getOptimizationPlanInfoes({
                o365TenantId: o365TenantId,
                needCalculateCount: pageInfo.pageIndex === 0,
                pageIndex: pageInfo.pageIndex,
                pageSize: pageInfo.pageSize,
            });
            res.items.forEach(
                (item) => (item.checked = item.uniqueId === checkedUniqueId)
            );
            setItems(res.items);
            if (pageInfo.pageIndex === 0) {
                setTotalCount(res.count);
            }
        };

        fetchData();
    }, [pageInfo]);

    const onRemove = () => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_FA_Plan_DeleteTitle,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    },
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        $$.messagedialog(false);
                        var succeed = await ProgressRequester.requestCancelOptimizationJob(
                            o365TenantId,
                            checkedUniqueId
                        );
                        if (succeed) {
                            setCheckedUniqueId(null);
                            setPageInfo({ pageIndex: 0, pageSize: pageInfo.pageSize });
                            showToast.success(RMResx.RM_FA_Plan_Delete_Success);
                        } else {
                            showToast.error("Remove the optmization job failed");
                        }
                    }
                },
            ],
        });
    };

    const onPageChange = (pageIndex) => {
        if (pageIndex === pageInfo.pageIndex) {
            return;
        }
        setPageInfo({
            pageIndex: pageIndex,
            pageSize: pageInfo.pageSize,
        });
    };

    const onItemChecked = (id) => {
        const clonedItems = _.cloneDeep(items);
        clonedItems.forEach((item) => (item.checked = item.uniqueId === id));
        setItems(clonedItems);
        setCheckedUniqueId(id);
    };

    const onItemClick = (item) => {
        detailRef.current.onShow(o365TenantId, item.uniqueId);
    };

    return (
        <div className="reco-analysis-planned">
            <div className="reco-planned-action-bar">
                <div>
                    {!_.isNil(checkedUniqueId) && (
                        <R.Button
                            className="theme"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_FA_Progress_PlannedTab_Button_Remove}
                            type="button"
                            tooltip={RMResx.RM_FA_Progress_PlannedTab_Button_Remove}
                            onClick={onRemove}
                        />
                    )}
                </div>
            </div>
            <div className="reco-planned-table">
                <DataTable
                    items={items}
                    onChangeChecked={onItemChecked}
                    onItemClick={onItemClick}
                />
            </div>
            <div className="reco-planned-pager">
                <div className="reco-discovery-table-paging-total" tabIndex="0">
                    {`${RMResx.RM_FA_Table_TotalCount} ${totalCount}`}
                </div>
                <SimplePager
                    pagerIndex={pageInfo.pageIndex}
                    pagerSize={pageInfo.pageSize}
                    shownCount={items.length}
                    hasNext={
                        (pageInfo.pageIndex + 1) * pageInfo.pageSize <
                        totalCount
                    }
                    onChange={onPageChange}
                />
            </div>
            <Detial ref={detailRef} />
        </div>
    );
};

export default Planned;
