import ExtendRestoreAction from "../Actions/ExtendRestoreAction";
import {
    ApprovalStatus,
    OrderOptions,
    ManualReviewAction,
    ManualReviewActionI18Ns,
    Source,
    FilterOptions
} from "../Constants/index";
import ExtendReviewDetails from "../Details/ExtendReviewDetails";
import { WrapperLinkUrl } from "../../../../Utilities/CommonUtil";
import React from "react";
import ExtendFilterPanel from "../FilterPanels/ExtendFilterPanel";
import { CustomColumnType } from "../../../BCM/ContentRepositoryManagement/CustomMetadataSetting/Constants";

class ExtendTableRow extends R.TableRow {

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


    getActionButton = (rowData) => {
        return (
            <>
                <R.Button
                    text={ManualReviewActionI18Ns.get(ManualReviewAction.ExtendRestore)}
                    onClick={() => this.onAction(ManualReviewAction.ExtendRestore)}
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
                        aria-label={rowData.extendTime}
                    >
                        {rowData.extendTime}
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

export default class ExtendTable extends R.Component {
    componentCreate() {
        this.changeActionRef = React.createRef();
        this.viewDetailRef = React.createRef();
        this.state = {
            items: this.props.items,
            orderOption: OrderOptions.None,
            isDesc: -1,
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
            default:
                break;
        }
    }

    onExecuteAction = (item, actionType) => {
        switch (actionType) {
            case ManualReviewAction.ExtendRestore:
                ExtendRestoreAction.Restore(new Set([item.id]), this.props.onReload);
                break;
        }
    };

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

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];
/** 
        items.forEach(item => {
           if (item.internalApprovedStatus === ApprovalStatus.WorkflowComplete) {
                item.checked = false;
            }
       });
*/

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
        const columnId = `extend-${col.id}`;
        let columnWidths = window["columnWidths"] || {};
        columnWidths[columnId] = width;
        window["columnWidths"] = columnWidths;
    };

    render() {
        return (
            <>
                <R.Table
                    id={"reco-manual-extend-table"}
                    rowKeyField={"rowIndex"}
                    rowTemplate={ExtendTableRow}
                    rootData={{
                        isFilterClear : this.state.isFilterClear,
                        isFilter : this.props.defaultFilterDefinitions, 
                        specialEndUser : this.props.SpecialEnableReviewDefinitions,
                        specialEndUserForOneLocation : this.props.SpeciallEnableReviewOnlyOneLocationDefinitions,
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
                <ExtendReviewDetails ref={this.viewDetailRef} onReload={this.props.onReload}/>
                <ExtendFilterPanel
                    onFilter={this.props.onFilter}
                    defaultFilterDefinitions={this.props.defaultFilterDefinitions} 
                    filterAvailableOptions = {this.props.filterAvailableOptions}
                    SpecialEnableReviewDefinitions={this.props.SpecialEnableReviewDefinitions}
                    approvalCommentQuickReasons = {this.props.approvalCommentQuickReasons}
                    SpeciallEnableReviewOnlyOneLocationDefinitions={this.props.SpeciallEnableReviewOnlyOneLocationDefinitions}
                />
            </>
        );
    }
}