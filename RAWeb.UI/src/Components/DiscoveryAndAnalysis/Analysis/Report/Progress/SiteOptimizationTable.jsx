import { useEffect } from "react";
import { useState } from "react";
import { SimplePager } from "../../../../Common/Pager";
import ProgressRequester from "../../requests/ProgressRequester";
import { CalculateUtil } from "../../Utils";

class DataTableRow extends R.TableRow {
    render(Row, Cell) {
        const rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.siteCollection}
                    >
                        {rowData.url}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.optimizingTime}
                    >
                        {rowData.nextOptimizationTimeString}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.fileTotalSize}
                    >
                        {rowData.fileTotalSize}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.fileSumCount}
                    >
                        {rowData.fileSumCount}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.nextOptimizableFileTotalSize}
                    >
                        {rowData.nextOptimizableFileTotalSize}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.nextOptimizableVersionTotalSize}
                    >
                        {rowData.nextOptimizableVersionTotalSize}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.archived}
                    >
                        {rowData.archived}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.deleted}
                    >
                        {rowData.deleted}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.saving}
                    >
                        {rowData.saving}
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
                    header: RMResx.RM_FA_TableColumn_SiteCollection,
                    width: 350,
                    resizeable: true,
                },
                {
                    header: RMResx.RM_FA_Progress_TableColumn_OptimizingTime,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: `${RMResx.RM_FA_Inactive_SummaryTab_TotalSize}(${RMResx.RM_DSB_Unit_GB})`,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: RMResx.RM_FA_Inactive_TableColumn_FileCount,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: `${RMResx.RM_FA_Progress_SummaryTab_NextFile}(${RMResx.RM_DSB_Unit_GB})`,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: `${RMResx.RM_FA_Progress_SummaryTab_NextVersion}(${RMResx.RM_DSB_Unit_GB})`,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: `${RMResx.RM_FA_Progress_SummaryTab_Archived}(${RMResx.RM_DSB_Unit_GB})`,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: `${RMResx.RM_FA_Progress_SummaryTab_Deleted}(${RMResx.RM_DSB_Unit_GB})`,
                    width: 250,
                    resizeable: true,
                },
                {
                    header: `${RMResx.RM_FA_TableColumn_Saving} ${RMResx.RM_FA_TableColumn_Saving_Unit_Monthly}`,
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

    render() {
        return (
            <>
                <R.Table
                    id={"reco-discovery-progress-table"}
                    rowTemplate={DataTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.state.tableColumns}
                    checkable={false}
                    frozenCount={0}
                />
            </>
        );
    }
}

const SiteOptimizationTable = ({ o365TenantId }) => {
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
            const res = await ProgressRequester.getSiteOptimizedInfoesAsync({
                o365TenantId: o365TenantId,
                needCalculateCount: pageInfo.pageIndex === 0,
                pageIndex: pageInfo.pageIndex,
                pageSize: pageInfo.pageSize,
            });
            const calculatedItems = await CalculateUtil.CalculateProgressOptimizedSiteInfoes(res.items);
            setItems(calculatedItems);
            if (pageInfo.pageIndex === 0) {
                setTotalCount(res.count);
            }
        };

        fetchData();
    }, [pageInfo]);

    const onPageChange = (pageIndex) => {
        setPageInfo({
            pageIndex: pageIndex,
            pageSize: pageInfo.pageSize,
        });
    };

    return (
        <div className="reco-site-optimization-table">
            <DataTable items={items} />
            <div className="reco-discovery-pager">
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
        </div>
    );
};

export default SiteOptimizationTable;
