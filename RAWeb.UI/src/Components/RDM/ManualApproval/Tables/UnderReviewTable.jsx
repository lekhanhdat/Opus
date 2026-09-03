import ApprovalAction from "../Actions/ApprovalAction";
import { EscalateAction, ReassignAction } from "../Actions/EscalateAction";
import { ApprovalStatus, OrderOptions, ManualReviewAction, ManualReviewActionI18Ns,Source, FilterOptions} from "../Constants/index";
import UnderReviewDetails from "../Details/UnderReviewDetails";
import ApprovalCommentSettingDialog from "../Panels/ApprovalCommentSettingDialog";
import { ManualTab } from "../Constants/ManualTable";
import { LicenseHelper, WrapperLinkUrl } from "../../../../Utilities/CommonUtil";
import Utility from "../Utility";
import React from "react";
import UnderReviewFilterPanel from "../FilterPanels/UnderReviewFilterPanel";
import { CustomColumnType } from "../../../BCM/ContentRepositoryManagement/CustomMetadataSetting/Constants";
import _ from "lodash";
import ReclassifyAction from "../Actions/ReclassifyAction";
import { SourceFlag } from "../../../Common/Constants";


class UnderReviewTableRow extends R.TableRow {

    onAction = (actionType) => {
        this.dispatch("onAction", actionType);
    };

    onCellClick = () =>{
        this.dispatch("onCellClick");
    }; 

    onCellKeyDown = (e) =>{
        if(e.keyCode == "13"){
            this.dispatch("onCellClick");
        }
    }
    onFilterClick = () =>{
        this.dispatch("onFilterClick");
    }; 

    onFilterKeyDown = (e) =>{
        if(e.keyCode == "13"){
            this.dispatch("onFilterClick");
        }
    }

    getCellData = (rowData) => {
        const data = {...rowData}
        data.isShowTermFullPath = rowData.isShowTermFullPath;
        data.termFullPath = rowData.termFullPath;
        return data;
    }

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

    isFilterorClear=(rootData)=>
    {
        let CurFolderPathFilter = false;
        let LoationFilter  = false;
        //正常user和特殊单location 以及 特殊多location
        if(rootData.isFilter.some(item => item.FilterOption === FilterOptions.FolderPath))
        {
            CurFolderPathFilter =  JSON.parse(rootData.isFilter.filter(item => item.FilterOption == FilterOptions.FolderPath)[0].Value).folderPathResults.length == 1 ;
        }
        //特殊多location 需要重新处理
        if(rootData.specialEndUser&&!rootData.specialEndUserForOneLocation) 
        {
            if(rootData.isFilter.some(item => item.FilterOption === FilterOptions.Workspace))
            {
                LoationFilter =  JSON.parse(rootData.isFilter.filter(item => item.FilterOption == FilterOptions.Workspace)[0].Value).WorkspaceIds.length == 1;
            }
            if(LoationFilter && rootData.isFilter.some(item => item.FilterOption === FilterOptions.FolderPath))
            {
                CurFolderPathFilter =  JSON.parse(rootData.isFilter.filter(item => item.FilterOption == FilterOptions.FolderPath)[0].Value).folderPathResults.length == 1 ;
            }  
            if(!LoationFilter)
            {
                CurFolderPathFilter = false;
            }     
        }
        
        return  !CurFolderPathFilter;
    }

    getActionButton = (rowData, rootData) => {
        let buttonNames = Utility.getCustomButtonNames(rootData.needCustomButton, rootData.customButtonNames);
        const isHideReclassifyBtnByApiSetting = !!this.props.rootData.isHideReclassifyBtnByApiSetting;
        return (
            <>
                <R.Button
                    text={buttonNames.approveButtonName}
                    disabled={rowData.internalApprovedStatus === ApprovalStatus.WorkflowComplete}
                    onClick={() => this.onAction(ManualReviewAction.Approve)}
                />
                <R.Button
                    text={buttonNames.rejectButtonName}
                    disabled={rowData.internalApprovedStatus === ApprovalStatus.WorkflowComplete}
                    onClick={() => this.onAction(ManualReviewAction.Reject)}
                />
                {!isHideReclassifyBtnByApiSetting && rowData.retentionStatus !== 1 &&
                    (!([SourceFlag.Exchange, SourceFlag.OneDrive, SourceFlag.GoogleDrive].includes(rowData.sourceFlag)) || rowData.enableClassificationByOpus) &&
                    !(rowData.sourceFlag === SourceFlag.FileSystem && this.props.rootData.isFSSettingClassificationFolderLevel)
                    && LicenseHelper.EnableRecordsArchiver()
                    && (
                        <R.Button
                            text={ManualReviewActionI18Ns.get(ManualReviewAction.Reclassify)}
                            disabled={rowData.internalApprovedStatus === ApprovalStatus.WorkflowComplete}
                            onClick={() => this.onAction(ManualReviewAction.Reclassify)}
                        />
                    )
                }
                {
                    (rowData.internalApprovedStatus !== ApprovalStatus.Approved &&
                        rowData.internalApprovedStatus !== ApprovalStatus.WorkflowComplete) &&
                    <>
                        <R.Button
                            text={ManualReviewActionI18Ns.get(ManualReviewAction.Reassign)}
                            onClick={() => this.onAction(ManualReviewAction.Reassign)}
                        />
                        {
                            !rootData.disabledEscalate 
                            &&
                            <R.Button
                                text={ManualReviewActionI18Ns.get(ManualReviewAction.Escalate)}
                                onClick={() => this.onAction(ManualReviewAction.Escalate)}
                            />
                        }
                    </>
                }
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

    renderTermName = (rowData) => {
        if (rowData.isShowTermFullPath) {
            return (
                <div
                    className="reco-manual-review-table-text"
                    data-tooltip
                    data-tooltip-wrap="force"
                    aria-label={rowData.termFullPath}
                >
                    {rowData.termName}
                </div>
            );
        }

        return (
            <div
                className="reco-manual-review-table-text"
                onMouseEnter={() => this.dispatch("showTermFullPath")}
            >
                {rowData.termName}
            </div>
        );
    }

    render(Row, Cell) {
        const rowData = this.getCellData(this.props.rowData);
        const actionButtons = this.getActionButton(rowData, this.props.rootData);
        const isFilterClear =  this.isFilterorClear(this.props.rootData);
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
                            <a tabIndex={0} onClick={this.onCellClick} onKeyDown={this.onCellKeyDown}>
                                {rowData.leafName}
                            </a>
                        </span>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.recordsId}
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
                        { this.getItemFullPath(rowData) }
                    </div>
                </Cell>
                <Cell>
                    <div className="reco-manual-review-table-flex">
                       { 
                              
                            rowData.sourceFlag === Source.OneDrive 
                            &&
                            rowData.manualFolderPath  
                            &&
                            (
                                isFilterClear ?
                                (
                                    <span
                                    className={`reco-manual-review-icon  fia-funnel`}   
                                    data-tooltip
                                >
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                    <span className="path3"></span>
                                </span>
                                )
                                :
                                (
                                    <span
                                    className={`reco-manual-review-icon   fia-funnel-clear`}    
                                    data-tooltip
                                >
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                    <span className="path3"></span>
                                </span>
                                )
                            )
                        }
                        <span
                            className="reco-manual-review-table-text padding-right-xs"
                            data-tooltip
                            aria-label= {rowData.manualFolderPath}
                        >
                        {
                            rowData.sourceFlag === Source.OneDrive
                            ?
                            (
                                <a tabIndex={0} onClick={this.onFilterClick} onKeyDown={this.onFilterKeyDown}>
                                    {rowData.manualFolderPath}
                                </a>
                            ) 
                            :
                            (
                                <span>{rowData.manualFolderPath}</span>
                            )
                        } 
                        </span>

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
                    {this.renderTermName(rowData)}
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
                        aria-label={rowData.manualLastReasonForRejection}
                    >
                        {rowData.manualLastReasonForRejection}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.manualLastApproveRejectComment}
                    >
                        {rowData.manualLastApproveRejectComment}
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
                        aria-label={rowData.manualLastReviewedBy}
                    >
                        {rowData.manualLastReviewedBy}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.manualLastReviewTime}
                    >
                        {rowData.manualLastReviewTime}
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
                        aria-label={rowData.modifiedTime}
                    >
                        {rowData.modifiedTime}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.manualDisposalDueDate}
                    >
                        {rowData.manualDisposalDueDate}
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

export default class UnderReviewTable extends R.Component {
    componentCreate() {
        this.escalateRef = React.createRef();
        this.reassignRef = React.createRef();
        this.viewDetailRef = React.createRef();
        this.reclassifyActionRef = React.createRef();
        this.termPaths = {};
        this.state = {
            items: this.props.items,
            orderOption: OrderOptions.None,
            isDesc: -1,
            isShow: false,
            approvalAction: ApprovalStatus.Approved,
            checkedItemId: [],
            approvalCommentQuickReason: "",
            isFilterClear: false ,
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

    getColumns = (columns) => {
        let newColumns = [...columns];

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
        });

        return newColumns;
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
            case "onFilterClick":
                this.onFilterClick(args.rowData);
                break;
            case 'showTermFullPath':
                this.setTermFullPath(args);
                break;
            default:
                break;
        }
    }

    onCellClick = (item) => {
        this.viewDetailRef.current.onShow(item);
    }

    onFilterClick = (item) => {
        
        let isClicked = false;
        //页面上点击的。
        var SourcesFilter = 
        {
            FilterOption: FilterOptions.Source,
            Value: JSON.stringify([item.sourceFlag == Source.OneDrive ? Source.OneDrive : Source.SharePoint])
        };
        var LocationFilter  =
        {
            FilterOption: FilterOptions.Workspace,
            Value: 
            JSON.stringify
            (
                {
                    WorkspaceIds : [item.manualSiteUrlId],
                    WorkspacePaths :[item.manualSiteUrl],
                    ContentSource : item.sourceFlag == Source.OneDrive ? Source.OneDrive : Source.SharePoint
                }
            ),
            AttacheValue:
            [
                {
                    workspacePath : item.manualSiteUrl,
                    workspaceId : item.manualSiteUrlId,
                    checked : true,
                    tooltip : "",
                    disabled : false,
                    undefined : false
                }
            ]
        };
        var FolderPathFilter = 
            {
                FilterOption: FilterOptions.FolderPath,
                Value: JSON.stringify
                (
                   {
                        folderPathResults :[item.manualFolderPath] ,
                        ContentSource : item.sourceFlag == Source.OneDrive ? Source.OneDrive : Source.SharePoint,
                        WorkSpace :[item.manualSiteUrl]
                   }
                ),
                AttacheValue:
                [
                    {
                        name : item.manualFolderPath,
                        value : item.manualFolderPath,
                        checked : true,
                        tooltip : "",
                        disabled : false,
                        undefined : false
                    }
                ]
        };
        //记录下之前选的
        let PreWorkSpacePaths = [];
        let PreFolderPath =[];
        if(this.props.defaultFilterDefinitions.some(item => item.FilterOption === FilterOptions.Workspace))
        {
             PreWorkSpacePaths = JSON.parse(this.props.defaultFilterDefinitions.filter(item => item.FilterOption == FilterOptions.Workspace)[0].Value).WorkspacePaths;
        }
        if(this.props.defaultFilterDefinitions.some(item => item.FilterOption === FilterOptions.FolderPath))
        {
            PreFolderPath = JSON.parse(this.props.defaultFilterDefinitions.filter(item => item.FilterOption == FilterOptions.FolderPath)[0].Value).folderPathResults;
        }
        //添加新的
        if(!this.props.SpecialEnableReviewDefinitions)
        {
            _.remove(this.props.defaultFilterDefinitions, item => item.FilterOption === FilterOptions.Source); 
            _.remove(this.props.defaultFilterDefinitions, item => item.FilterOption === FilterOptions.Workspace); 
            _.remove(this.props.defaultFilterDefinitions, item => item.FilterOption === FilterOptions.FolderPath);
            if(PreFolderPath.length == 1  && PreFolderPath[0] == item.manualFolderPath && PreWorkSpacePaths[0]== item.manualSiteUrl)
            {
                this.props.defaultFilterDefinitions.push(SourcesFilter);
                this.props.defaultFilterDefinitions.push(LocationFilter);
                isClicked = true;
            }
            if(!isClicked)
            {
                this.props.defaultFilterDefinitions.push(SourcesFilter);
                this.props.defaultFilterDefinitions.push(LocationFilter);
                this.props.defaultFilterDefinitions.push(FolderPathFilter);
            }
        }
        else
        {
            _.remove(this.props.defaultFilterDefinitions, item => item.FilterOption === FilterOptions.FolderPath);
            _.remove(this.props.defaultFilterDefinitions, item => item.FilterOption === FilterOptions.Workspace);          
            if(!this.props.SpeciallEnableReviewOnlyOneLocationDefinitions)
            {
              
                if(PreFolderPath.length == 1  && PreFolderPath[0] == item.manualFolderPath && PreWorkSpacePaths[0]== item.manualSiteUrl)
                {  
                    this.props.defaultFilterDefinitions.push(LocationFilter);
                    isClicked = true;
                }
                if(!isClicked)
                {
                    this.props.defaultFilterDefinitions.push(LocationFilter);
                    this.props.defaultFilterDefinitions.push(FolderPathFilter);
                }
            }
            else
            {
                if(PreFolderPath.length == 1  && PreFolderPath[0] == item.manualFolderPath)
                {  
                    isClicked = true;
                }
                if(!isClicked)
                {
                    this.props.defaultFilterDefinitions.push(FolderPathFilter);
                }
            }
        }  

        this.state.isFilterClear = isClicked;
        this.props.onFilter(this.props.defaultFilterDefinitions);
        this.props.onReload();   
    }

    setTermFullPath(args) {
        const termId = args.rowData.termId;
        const option = {
            url: `/api/TermManagementApi/GetTermWithPath/?termId=${termId}`,
            method: "GET",
        };
        const newItems = [...this.state.items];
        if (this.termPaths[termId]) {
            newItems[args.rowIndex].isShowTermFullPath = true;
            newItems[args.rowIndex].termFullPath = this.termPaths[termId];
            this.props.onChangeItems(newItems);
        } else {
            fetchUtility(option).then((res) => {
                const data = JSON.parse(res);
                newItems[args.rowIndex].isShowTermFullPath = true;
                newItems[args.rowIndex].termFullPath = data.FullPath;
                this.termPaths[termId] = data.FullPath;
                this.props.onChangeItems(newItems);
            }).catch((e) => {
            });
        }
    }

    onApprove = (itemId) =>{
        this.setState({
            isShow : true,
            approvalAction : ApprovalStatus.Approved,
            checkedItemId : itemId
        });
    }

    realApprove = (approveComment) =>{

        ApprovalAction.onApprove(this.state.checkedItemId, approveComment, ManualTab.UnderReview, "", () => {
            this.props.onReload();
        });

    }

    onReject = (itemId) =>{
        const notExtendItems = this.props.items.filter(item =>Array.from(itemId).includes(item.id) ).filter(item => 
            item.extendCount >= this.props.settingModel.DisposalExtentionSetting.MaxDelayTimes 
        );

        if (notExtendItems.length > 0) {
            $$.messagedialog(true,
                {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: (
                        <div>
                            <div className="reco-manual-message-box-comment">
                                {RMResx.RM_MA_Extended_ExtendLimitForOne}
                            </div>
                            <div className="reco-manual-message-box-associated">
                                {RMResx.RM_MA_AssociatedRecords}
                            </div>
                            <div className="reco-manual-message-box-associated-items">
                                {
                                    notExtendItems.map(item =>
                                        <div key={item.id} className="reco-manual-message-box-associated-item">{item.leafName}</div>
                                    )
                                }
                            </div>
                        </div>
                    ),
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: async () => {
                                $$.messagedialog(false);
                            },
                        },
                    ],
                }
            );
            return;
        }
        
        this.setState({
            isShow : true,
            approvalAction : ApprovalStatus.Rejected,
            checkedItemId : itemId
        });
    }

    realReject = (rejectComment,selectedItem,customeExtendDate) =>{
        
        ApprovalAction.onReject(this.state.checkedItemId, rejectComment, ManualTab.UnderReview, this.state.approvalCommentQuickReason,selectedItem,customeExtendDate, () => {
            this.props.onReload();
        });
    }

    onHide = () => {
        this.setState({
            isShow : false,
            approvalCommentQuickReason : ""
        });
    }

    onChange = (args) => {
        this.setState({
            approvalCommentQuickReason : args,
        });
    }

    onExecuteAction = (item, actionType) => {
        switch (actionType) {
            case ManualReviewAction.Approve:
                this.onApprove(new Set([item.id]));
                break;
            case ManualReviewAction.Reject:
                this.onReject(new Set([item.id]));
                break;
            case ManualReviewAction.Escalate:
                this.escalateRef.current.onShow(new Set([item.id]));
                break;
            case ManualReviewAction.Reassign:
                this.reassignRef.current.onShow(new Set([item.id]));
                break;
            case ManualReviewAction.Reclassify:
                this.setState({
                    checkedItemId: new Set([item.id])
                }, () => {
                    this.reclassifyActionRef.current?.onOpenReclassifyPanel();
                });
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
        const columnId = `under-review-${col.id}`;
        let columnWidths = window["columnWidths"] || {};
        columnWidths[columnId] = width;
        window["columnWidths"] = columnWidths;
    };

    render() {
        return (
            <>
                <R.Table
                    id={"reco-manual-under-review-table"}
                    rowKeyField={"rowIndex"}
                    rowTemplate={UnderReviewTableRow}
                    rootData={{
                        disabledEscalate: this.props.disabledEscalate,
                        isFilterClear : this.state.isFilterClear,
                        isFilter : this.props.defaultFilterDefinitions,
                        needCustomButton : this.props.needCustomButton,
                        customButtonNames : this.props.customButtonNames,
                        specialEndUser : this.props.SpecialEnableReviewDefinitions,
                        specialEndUserForOneLocation : this.props.SpeciallEnableReviewOnlyOneLocationDefinitions,
                        customColumns: this.props.customColumns,
                        isFSSettingClassificationFolderLevel: this.props.isFSSettingClassificationFolderLevel,
                        isHideReclassifyBtnByApiSetting: this.props.isHideReclassifyBtnByApiSetting,
                    }}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.getColumns(this.props.columns)}
                    checkable={true}
                    onCheck={this.onItemCheckedChange}
                    frozenCount={1}
                    doSort={this.doSort}
                    flexible={false}
                    onColumnResize={this.onColumnResize}
                />
                <EscalateAction ref={this.escalateRef} onReload={this.props.onReload} />
                <ReassignAction ref={this.reassignRef} onReload={this.props.onReload} />
                <UnderReviewDetails ref={this.viewDetailRef} onReload={this.props.onReload}/>
                <ApprovalCommentSettingDialog 
                    show={this.state.isShow} 
                    onHide={this.onHide}
                    checkedCommentOption={this.props.checkedCommentOption}
                    action={this.state.approvalAction}
                    onApprove={this.realApprove}
                    onReject={this.realReject}
                    NeedQuickReason={this.props.NeedQuickReason}
                    ApprovalCommentQuickReason={this.props.ApprovalCommentQuickReason}
                    InactiveRejects={this.props.InactiveRejects}
                    CheckQuickReason={this.state.approvalCommentQuickReason}
                    onChange={this.onChange}
                    LatestExtendType = {this.props.settingModel.DisposalExtentionSetting.LatestExtendType}
                    LatestExtendNumber = {this.props.settingModel.DisposalExtentionSetting.LatestExtendNumber}
                    needCustomButton = {this.props.needCustomButton}
                    customButtonNames = {this.props.customButtonNames}
                    checkedItems={this.props.items.filter(item => Array.from(this.state.checkedItemId).includes(item.id))}
                />
                <UnderReviewFilterPanel
                    onFilter={this.props.onFilter}
                    defaultFilterDefinitions={this.props.defaultFilterDefinitions} 
                    filterAvailableOptions = {this.props.filterAvailableOptions}
                    SpecialEnableReviewDefinitions={this.props.SpecialEnableReviewDefinitions}
                    approvalCommentQuickReasons = {this.props.ApprovalCommentQuickReason}
                    SpeciallEnableReviewOnlyOneLocationDefinitions={this.props.SpeciallEnableReviewOnlyOneLocationDefinitions}
                />
                <ReclassifyAction
                    ref={this.reclassifyActionRef}
                    checkedItems={this.props.items.filter(item => Array.from(this.state.checkedItemId).includes(item.id))}
                    isCheckedAll={false}
                    canDoActionForReclassify={this.props.canDoActionForReclassify}
                    filterDefinitions={this.props.filterDefinitions}
                    searchFilterDefinition={this.props.searchFilterDefinition}
                    onReload={this.props.onReload}
                />
            </>
        );
    }
}