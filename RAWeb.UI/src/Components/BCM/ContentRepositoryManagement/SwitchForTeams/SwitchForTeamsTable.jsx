import { useEffect, useMemo, useState } from "react";

import ViewDetailDialog from "./ViewDetailDialog";

function SwitchForTeamsTable({ id, moduleType }) {
    const [items, setItems] = useState([]);
    const [scopeId, setScopeId] = useState("");
    const [lifecycleId, setlifecycleId] = useState("");
    const [soId, setSOId] = useState("");
    const [sortInfo, setSortInfo] = useState({ SortBy: "", IsAscending: false });
    const [pageIndex, setPageIndex] = useState(0);
    const [pageSize, setPageSize] = useState(10);
    const [hasNext, setHasNext] = useState(false);
    const [isShowSettingsDialog, setIsShowSettingsDialog] = useState(false);

    const columns = useMemo(() => {
        return [
            {
                header: RMResx.RM_AR_Teams_SwitchPage_ColumnUrl,
                width: [350],
                sortable: true,
                resizeable: true,
                valuePath: "FullPath",
            },
            {
                header: RMResx.RM_AR_Teams_SwitchPage_ColumnSameSetting,
                width: [250],
                sortable: true,
                resizeable: true,
                valuePath: "IsConflict",
            },
        ];
    }, []);

    useEffect(() => {
        getTeamsChannelConflictsList();
    }, [moduleType, pageIndex, pageSize, sortInfo]);

    const getTeamsChannelConflictsList = async () => {
        const option = {
            url: "/api/TeamsSettingApi/GetTeamsChannelConflictsList",
            method: "POST",
            data: {
                PageIndex: pageIndex,
                PageSize: pageSize,
                ModuleType: moduleType,
                SortBy: sortInfo.SortBy,
                IsAscending: sortInfo.IsAscending,
            },
        };
        $$.loading(true);
        const { Settings, TotalCount } = await fetchUtility(option);
        $$.loading(false);
        setItems(Settings);
        setHasNext((TotalCount - pageSize * (pageIndex * 1 + 1)) > 0);
    };

    useEffect(() => console.log("lifecycleId: ", lifecycleId), [lifecycleId])

    const onRowEvent = (args) => {
        switch (args.type) {
            case "viewSettings":
                setIsShowSettingsDialog(true);
                setScopeId(args.rowData.ScopeId);
                setlifecycleId(args.rowData.Id);
                setSOId(args.rowData.Id);
                break;
            default:
                break;
        }
    };

    const onTableSort = (args) => {
        setSortInfo(() => ({ SortBy: args.column.valuePath, IsAscending: args.status === "asc" ? true : false }));
        setPageIndex(0);
    }

    const handlePageChange = (currentPageIndex, currentPageSize) => {
        setPageIndex(currentPageIndex);
        setPageSize(currentPageSize);
    };

    // Render
    const renderDetailDialog = () => {
        return (
            <R.Dialog
                id="raCrmViewDetails"
                header={RMResx.RM_AR_Teams_SwitchPage_DetailDialog_Title}
                width={680}
                status={{ show: isShowSettingsDialog }}
                onHide={() => setIsShowSettingsDialog(false)}
            >
                <ViewDetailDialog scopeId={scopeId} lifecycleId={lifecycleId} soId={soId} moduleType={moduleType} />
            </R.Dialog>
        );
    };

    return (
        <>
            <R.Table
                id={id}
                rowTemplate={TableTemplate}
                columns={columns}
                items={items}
                onRowEvent={onRowEvent}
                doSort={onTableSort}
            />
            <div className="padding-top-m text-end">
                <$g.SimplePager
                    pagerIndex={pageIndex}
                    pagerSize={pageSize}
                    shownCount={items.length}
                    hasNext={hasNext}
                    showPagerSize={true}
                    pagerSizeOptions={[5, 10, 15, 50]}
                    onChange={handlePageChange}
                />
            </div>
            {renderDetailDialog()}
        </>
    );
}

class TableTemplate extends R.TableRow {
    viewSettings() {
        this.dispatch("viewSettings");
    }

    action = () => {
        return (
            <R.Button
                type={null}
                icon="fia-eye"
                tooltip={RMResx.RM_AR_Teams_SwitchPage_ViewDetail_Btn}
                classify="default"
                round={true}
                onClick={this.viewSettings.bind(this)}
            />
        );
    };

    render(Row, Cell) {
        const rowData = this.props.rowData;
        return (
            <Row action={this.action}>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label={rowData.FullPath}
                    >
                        {rowData.FullPath}
                    </div>
                </Cell>
                <Cell>
                    <div
                        aria-label={
                            rowData.IsConflict
                                ? RMResx.RM_JS_Common_Yes
                                : RMResx.RM_JS_Common_No
                        }
                    >
                        {rowData.IsConflict
                            ? RMResx.RM_JS_Common_Yes
                            : RMResx.RM_JS_Common_No}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export default SwitchForTeamsTable;
