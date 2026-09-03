import {
    ApprovalStatusI18Ns,
    OrderOptions,
    RelatedRecordsActionI18Ns,
    Source
} from "../Constants/index";
import { WrapperLinkUrl } from "../../../../Utilities/CommonUtil";

class HistoryTableRow extends R.TableRow {

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

    render(Row, Cell) {

        const rowData = this.props.rowData;

        return (
            <Row>
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
                            className="reco-manual-review-table-text padding-right-xs reco-manual-review-name"
                            data-tooltip
                            aria-label={rowData.leafName}
                        >
                            {rowData.leafName}
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
                        aria-label= {rowData.manualFolderPath}
                    >
                        {rowData.manualFolderPath}
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
                        data-tooltip="ifneed"
                        aria-label={ApprovalStatusI18Ns.get(rowData.internalApprovedStatus)}
                    >
                        {ApprovalStatusI18Ns.get(rowData.internalApprovedStatus)}
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
                                    href={item.Url}
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
                        {rowData.isRelatedRecords && RelatedRecordsActionI18Ns.get(rowData.relatedRecordsAction)}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.escalateFromDisplayName}
                    >
                        {rowData.escalateFromDisplayName}
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
                        aria-label={rowData.approvedByDisplayName}
                    >
                        {rowData.approvedByDisplayName}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.manualQuickReason}
                    >
                        {rowData.manualQuickReason}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.manualApprovalComment}
                    >
                        {rowData.manualApprovalComment}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text ra-pre-wrap"
                        data-tooltip="ifneed"
                        aria-label={rowData.escalatedComment}
                    >
                        {rowData.escalatedComment}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.modifiedBy}
                    >
                        {rowData.modifiedBy}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.createdBy}
                    >
                        {rowData.createdBy}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.actionTime}
                    >
                        {rowData.actionTime}
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
            </Row>
        );
    }
}

export default class HistoryTable extends R.Component {
    componentCreate() {
        this.escalateRef = React.createRef();
        this.reassignRef = React.createRef();
        this.extendRef = React.createRef();
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
                items: items,
            };
        }

        return null;
    }

    doSort = (args) => {
        const orderOption = args.column.orderOption;
        const isDesc = args.status === "desc";
        this.setState({
            orderOption: orderOption,
            isDesc: isDesc ? -1 : 1
        });
        this.props.onSort({
            orderOption,
            isDesc
        });
    }

    onColumnResize = (col, width) => {
        const columnId = `history-${col.id}`;
        let columnWidths = window["columnWidths"] || {};
        columnWidths[columnId] = width;
        window["columnWidths"] = columnWidths;
    };

    render() {
        return (
            <>
                <R.Table
                    id={"reco-manual-history-table"}
                    rowKeyField={"rowIndex"}
                    rowTemplate={HistoryTableRow}
                    items={this.state.items}
                    columns={this.props.columns}
                    doSort={this.doSort}
                    flexible={false}
                    onColumnResize={this.onColumnResize}
                />
            </>
        );
    }
}