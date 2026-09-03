import ChangeDisposalAction from "../Actions/ChangeDipsoalAction";
import {
    ApprovalStatus,
    Columns,
    OrderOptions,
    ManualReviewAction,
    ManualReviewActionI18Ns,
    RelatedRecordsActionI18Ns,
    Source,
} from "../Constants/index";
import RelatedRecordsDetails from "../Details/RelatedRecordsDetails";
import { WrapperLinkUrl } from "../../../../Utilities/CommonUtil";
import { CustomColumnType } from "../../../BCM/ContentRepositoryManagement/CustomMetadataSetting/Constants";

class RelatedRecordsTableRow extends R.TableRow {

    onAction = (actionType) => {
        this.dispatch("onAction", actionType);
    };

    onCellClick = () =>{
        this.dispatch("onCellClick");
    }; 

    getItemFullPath = (rowData) =>{
        const linkSources = new Set([Source.SharePoint, Source.OneDrive, Source.Teams]);
        if(rowData.fileExtension === RMResx.RM_RDM_RecordDetails_DataType_SPItem && rowData.retentionStatus === 0 && rowData.sourceFlag != Source.SharePointOnPrem){
            return (<a href={rowData.fullPath} target="_blank" rel='noreferrer noopener'>
                {rowData.fullPath}
            </a>);
        }else if(linkSources.has(rowData.sourceFlag) && rowData.retentionStatus === 0){
            return (<a href={WrapperLinkUrl(rowData.fullPath)} target="_blank" rel='noreferrer noopener'>
                {rowData.fullPath}
            </a>);
        }else{
            return rowData.fullPath;
        }
    }

    getActionButton = (rowData) => {
        return (
            <>
                <R.Button
                    text={ManualReviewActionI18Ns.get(ManualReviewAction.ChangeAction)}
                    onClick={() => this.onAction(ManualReviewAction.ChangeAction)}
                />
                <div></div>
            </>
        );
    };

    getCustomCells = () => {
        const customCellsInfo = RM.deepcopy(this.props.rootData.customColumns);
        const cellInfo = RM.deepcopy(this.props.rowData.customeColumnDic);
        for (let cell of customCellsInfo) {
            const customCell = cellInfo[cell.id];
            if (customCell) {
                switch (cell.columnType) {
                    case CustomColumnType.SingleText:
                    case CustomColumnType.Number:
                        cell.Name = customCell.Value;
                        break;
                    case CustomColumnType.YesOrNo:
                        cell.Name = customCell.YesOrNo;
                        break;
                    case CustomColumnType.DateTime:
                        cell.Name = RM.TimeUtil.dateToStringSimplifyTimeZone(customCell.Date, RM.TimeUtil.getGlobalTimezoneInfo());
                        break;
                }
            }
        }
        return customCellsInfo;
    }

    render(Row, Cell) {

        const rowData = this.props.rowData;
        const actionButtons = this.getActionButton(rowData);
        let customCellInfos = this.props.rootData.customColumns;
        if (rowData.customeColumnDic) {
            customCellInfos = this.getCustomCells();
        }

        return (
            <Row action={actionButtons}>
                <Cell>
                    <div className="reco-manual-review-table-flex">
                        <span
                            className={`reco-manual-review-icon ${rowData.sourceIcon}`}
                            data-tooltip
                            aria-label={rowData.sourceName}
                        >
                            <span className="path1"></span>
                            <span className="path2"></span>
                            <span className="path3"></span>
                            <span className="path4"></span>
                            <span className="path5"></span>
                            <span className="path6"></span>
                        </span>
                        <span
                            className="reco-manual-review-table-text padding-right-xs"
                            data-tooltip
                            aria-label={rowData.leafName}
                        >
                            <a onClick={this.onCellClick}>
                                {rowData.leafName}
                            </a>
                        </span>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label= {rowData.recordsId}
                    >
                        {rowData.recordsId}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label= {rowData.fullPath}
                    >
                        {
                            this.getItemFullPath(rowData)
                        }
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={`${rowData.fileExtension}${rowData.retentionStatus === 1 ? ` (${RMResx.RM_MA_Extended_RetentionStatus})` : ""}`}
                    >
                        {`${rowData.fileExtension}${rowData.retentionStatus === 1 ? ` (${RMResx.RM_MA_Extended_RetentionStatus})` : ""}`}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip
                        aria-label={`${RMResx.RM_JS_RDM_Rule_RuleName} : ${rowData.ruleName}; ${RMResx.RM_JS_Rule_Detail_Criteria} : ${rowData.ruleCriteria};`}
                    >
                        {rowData.ruleName}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.ruleDisposalClass}
                    >
                        {rowData.ruleDisposalClass}
                    </div>
                </Cell>
                <Cell>
                    <div>
                        {
                            rowData.relatedRecords.map((item, index) =>
                                <a
                                    key={index}
                                    href={WrapperLinkUrl(item.Url)}
                                    data-tooltip
                                    aria-label={item.Url.indexOf("Root/PRM/RecordsExplorer") != -1 ? item.Name : item.Url}
                                    className="reco-manual-review-table-link"
                                    target="_blank"
                                    rel='noreferrer noopener'
                                >
                                    {item.Name}
                                </a>
                            )
                        }
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={RelatedRecordsActionI18Ns.get(rowData.relatedRecordsAction)}
                    >
                        {RelatedRecordsActionI18Ns.get(rowData.relatedRecordsAction)}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.reviewerDisplayNames.join("; ")}
                    >
                        {rowData.reviewerDisplayNames.join("; ")}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.modifiedTime}
                    >
                        {rowData.modifiedTime}
                    </div>
                </Cell>
                {customCellInfos.map((item, index) => {
                    return (
                        <Cell key={index}>
                            <div className="reco-manual-review-table-text" data-tooltip="ifneed" aria-label={item.Name}>
                                {item.Name}
                            </div>
                        </Cell>
                    );
                })}
            </Row>
        );
    }
}

export default class RelatedRecordsTable extends R.Component {
    componentCreate() {
        this.changeActionRef = React.createRef();
        this.viewDetailRef = React.createRef();
        this.state = {
            items: this.props.items,
            orderOption: OrderOptions.None,
            isDesc: -1,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                isCheckedSelectedAll: items.length > 0 && (items.every(i => i.checked) || (items.some(i => i.checked) && 'mixed')),
                items: items,
            };
        }

        return null;
    }

    onRowEvent = (args, actionType) => {
        switch (args.type) {
            case "checked":
                this.onItemCheckedChange(args.rowData);
                break;
            case "onAction":
                this.onExecuteAction(args.rowData, actionType);
                break;
            case "onCellClick":
                this.onCellClick(args.rowData);
                break;
            default:
                break;
        }
    }

    onCellClick = (item) => {
        this.viewDetailRef.current.onShow(item);
    }

    onExecuteAction = (item, actionType) => {
        switch (actionType) {
            case ManualReviewAction.ChangeAction:
                this.changeActionRef.current.onShow(new Set([item.id]), item.relatedRecordsAction);
                break;
        }
    };

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];

        items.forEach(item => {
            if (item.internalApprovedStatus === ApprovalStatus.WorkflowComplete) {
                item.checked = false;
            }
        });

        this.setState({
            items: items
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked();
    }

    doSort = (args) => {
        const orderOption = args.column.orderOption;
        const isDesc = args.status === "desc";
        const customColumnId = typeof args.column.id === "string" ? args.column.id : "";
        this.setState({
            orderOption: orderOption,
            isDesc: isDesc ? -1 : 1
        });
        this.props.onSort({
            orderOption,
            isDesc,
            customColumnId,
        });
    }

    onColumnResize = (col, width) => {
        const columnId = `related-records-${col.id}`;
        let columnWidths = window["columnWidths"] || {};
        columnWidths[columnId] = width;
        window["columnWidths"] = columnWidths;
    };

    render() {
        return (
            <>
                <R.Table
                    id={"reco-manual-related-records-table"}
                    rowKeyField={"rowIndex"}
                    rowTemplate={RelatedRecordsTableRow}
                    rootData={{
                        customColumns: this.props.customColumns,
                    }}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.props.columns}
                    checkable={true}
                    onCheck={this.onItemCheckedChange}
                    frozenCount={1}
                    doSort={this.doSort}
                    flexible={false}
                    onColumnResize={this.onColumnResize}
                />
                <ChangeDisposalAction ref={this.changeActionRef} onReload={this.props.onReload} />
                <RelatedRecordsDetails ref={this.viewDetailRef} onReload={this.props.onReload}/>
            </>
        );
    }
}