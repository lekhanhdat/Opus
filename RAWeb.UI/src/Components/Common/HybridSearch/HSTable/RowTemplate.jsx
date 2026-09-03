import { YesOrNo } from "../../../PRM/Constants";
import { PhysicalDefaultColumnIDs, SPDisposalAction, PhysicalObjectColumnType, GoogleDisposalAction, SourceFlags } from "../../../../Constants/Constants";
import { bindEvents, LicenseHelper } from "../../../../Utilities/CommonUtil";
import RuleUtil from "../../../../Utilities/RuleUtil";

export class RowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        bindEvents(this, "showTermFullPath");
    }

    onCellClick() {
        this.dispatch('cellClick');
    }

    onKeyDown = (e) =>{
        if(e.keyCode == 13 || e.keyCode == 32){
            e.target.click();
        }
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
    }

    getRuleAction(rowData) {
        let ruleAction = RuleUtil.parseDisposalActionForSP(rowData.DisposalAction, rowData.SourceFlag);//sp,
        switch (rowData.SourceFlag) {
            case 2://fs
                ruleAction = RuleUtil.parseDisposalActionForFS(rowData.DisposalAction);
                break;
            case 3: //exo
                ruleAction = SPDisposalAction[rowData.ExchangeDisposalAction];
                break;
            case 4://phy
                ruleAction = RuleUtil.parseDisposalAction(rowData.DisposalAction);
                break;
            case 9: //google
                ruleAction = GoogleDisposalAction[rowData.DisposalAction]
        }
        return ruleAction;
    }

    getCellData(rowData) {
        let d = {},
            isPhyObj = rowData.SourceFlag == 4;
        d.UniqueId = rowData.RecordsId;
        d.ModifiedBy = rowData.ModifiedBy;
        d.CreatedBy = rowData.CreatedBy;
        d.RuleAction = this.getRuleAction(rowData);
        d.OnHold = this.getHoldCellValue(rowData.HoldStatus);
        d.SourceFlag = rowData.SourceFlag;
        d.SourceName = rowData.SourceName;
        d.ModifiedTime = rowData.TimeLastModified;
        d.ModifiedTimeStr = rowData.TimeLastModifiedStr;
        d.ArchivedTime = rowData.TimeArchived;
        d.ArchivedTimeStr = rowData.TimeArchivedStr;
        if (isPhyObj && rowData.NodeType != 9500) {
            let physicalColTerm = rowData.CustomColumnDic[PhysicalDefaultColumnIDs.Classification];
            d.TermName = physicalColTerm ? physicalColTerm.Name : "";
        } else {
            d.TermName = rowData.TermName;
        }
        d.NameOrTitle = rowData.LeafName;
        d.RecordType = rowData.ExtensionForFile;
        d.CustomColumnDic = rowData.CustomColumnDic;
        d.IsShowTermFullPath = rowData.IsShowTermFullPath;
        d.TermFullPath = rowData.TermFullPath;
        return d;
    }

    getHoldCellValue(isHold) {
        return isHold ? YesOrNo[0] : YesOrNo[1];
    }

    getSourceIcon(rowData) {
        switch (rowData.SourceFlag) {
            case SourceFlags.SP:
                return 'fi-ms-sharepoint';
            case SourceFlags.FS:
                return "fia-fs";
            case SourceFlags.Exo:
                return 'fi-ms-exchange';
            case SourceFlags.Phy:
                return 'fia-physical-record';
            case SourceFlags.SPLocal:
                return "fia-sharepoint";
            case SourceFlags.OneDrive:
                return "fi-ms-onedrive";
            case SourceFlags.AzureFile:
                return "fi-ms-azure-file-share";
            case SourceFlags.Box:
                return "fia-box-blue-b";
            case SourceFlags.Google:
                return "fia-google-drive-f";
            case SourceFlags.Teams:
                return "fi-ms-teams";
            default:
                if(rowData.SourceFlag >= 1000){ return "fia-connecter";}
        }
    }

    getSourceToolTip(rowData) {
        let flag = rowData.SourceFlag;
        switch (flag) {
            case SourceFlags.SP:
                return RMResx.RM_JS_SPS_TabLabel_SP;
            case SourceFlags.FS:
                return RMResx.RM_JS_SPS_TabLabel_FS;
            case SourceFlags.Exo:
                return RMResx.RM_JS_SPS_TabLabel_EXO;
            case SourceFlags.Phy:
                return RMResx.RM_JS_SPS_TabLabel_Physical;
            case SourceFlags.SPLocal:
                return RMResx.RM_Common_SharePointOnPremise;
            case SourceFlags.AzureFile:
                return RMResx.RM_JS_SPS_TabLabel_AF;
            case SourceFlags.Box:
                return RMResx.RM_JS_SPS_TabLabel_Box;
            default:
                if(flag >= 1000){ return rowData.SourceName;}
        }
    }

    getCustomCells(customColums, cellInfo) {
        let customCellsInfo = RM.deepcopy(customColums);
        for (let cell of customCellsInfo) {
            let customCell = this.getCellFromeCellInfo(cell, cellInfo);  //cellInfo[cell.UniqueId];
            if (customCell) {
                let userNames = "";
                let multipleChoiceText = "";
                switch (cell.ColumnType) {
                    case PhysicalObjectColumnType.SingleText:
                    case PhysicalObjectColumnType.MutipleText:
                    case PhysicalObjectColumnType.Number:
                        cell.Name = customCell.Value;
                        break;
                    case PhysicalObjectColumnType.YesOrNo:
                        cell.Name = customCell.YesOrNo;
                        break;
                    case PhysicalObjectColumnType.DateTime:
                        //cell.Name = $$.date.format(customCell.Date, RM.TimeUtil.getGlobalAuiFormat());
                        cell.Name = RM.TimeUtil.dateToStringSimplifyTimeZone(customCell.Date, RM.TimeUtil.getGlobalTimezoneInfo());
                        break;
                    case PhysicalObjectColumnType.PeopleOrGroup:
                        userNames = (customCell.Users || []).map((item) => { return item.DisplayName; });
                        cell.Name = userNames.toString();
                        break;
                    case PhysicalObjectColumnType.SingleChoice:
                    case PhysicalObjectColumnType.Taxonomy:
                        if (customCell.IsShowToolValue) {
                            cell.IsShowToolValue = customCell.IsShowToolValue;
                            cell.ToolValue = customCell.ToolValue;
                        }
                        cell.Name = customCell.Name;
                        break;
                    case PhysicalObjectColumnType.MultipleChoice:
                        if (customCell.MultiChoice) {
                            multipleChoiceText = customCell.MultiChoice.map((item) => { return item.Name; });
                        }
                        cell.Name = multipleChoiceText.toString();
                        break;
                }
            }
        }
        return customCellsInfo;
    }

    getCellFromeCellInfo(cell, cellInfo) {
        if (cell.IdsWithDuplicateName && cell.IdsWithDuplicateName.length > 0) {
            for (let id of cell.IdsWithDuplicateName) {
                let customCell = cellInfo[id];
                if (customCell) {
                    return customCell;
                }
            }
        }
        return cellInfo[cell.UniqueId];
    }

    showTermFullPath = (event) => {
        this.dispatch("showTermFullPath");
    };
    showHomeLocationFullPath = (event) => {
        this.dispatch("showHomeLocationFullPath");
    };

    render(Row, Cell) {
        let oriRowData = this.props.rowData;
        let rowData = this.getCellData(oriRowData);
        let modifiedTime = rowData.ModifiedTimeStr;
        let archivedTime = rowData.ArchivedTimeStr;
        let isDeclareAsRecord = oriRowData.DeclareAsRecord;
        let declareAsRecordValue = oriRowData.DeclareAsRecord ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
        let isLockedWithRecordsLabel = oriRowData.LockedByRecordLabel;
        let lockWithRecordsLabelValue = oriRowData.LockedByRecordLabel ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
        let createTime = oriRowData.TimeCreatedStr;
        let sourceToolTip = this.getSourceToolTip(rowData.SourceFlag);
        let customCellInfo = this.props.rootData.customColums;
        let isContainsPhySource = this.props.rootData.isContainsPhySource;
        let commonPhyUniqueCells = [];
        let cellIdx = 0;
        if (rowData.CustomColumnDic) {
            customCellInfo = this.getCustomCells(this.props.rootData.customColums, rowData.CustomColumnDic);
        }
        let customCells = customCellInfo.map((item) => {
            if (item.NameHash == PhysicalDefaultColumnIDs.HomeLocation) {
                return <Cell key={cellIdx++}>
                    {
                        item.IsShowToolValue && <div className="text-overflow" data-tooltip={true} aria-label={item.ToolValue}>
                            {item.Name}
                        </div>
                    }
                    {
                        !item.IsShowToolValue && <div className="text-overflow" onMouseEnter={this.showHomeLocationFullPath}>
                            {item.Name}
                        </div>
                    }
                </Cell>;
            } else {
                return <Cell key={cellIdx++}>
                    <div className="text-overflow ra-pre-wrap" data-tooltip aria-label={item.Name}>
                        {item.Name}
                    </div>
                </Cell>;
            }
        });
        let commonCells = [
            <Cell key={cellIdx++}>
                <R.Checkbox
                    checked={this.props.rowData.isChecked || false}
                    onChange={this.onCheckChange}
                />
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="flex ra-flex-align-center">
                    <div className={this.getSourceIcon(rowData) + " name-column-content"} data-tooltip aria-label={rowData.SourceName}>
                        {(isDeclareAsRecord || isLockedWithRecordsLabel) && <div>
                            <div className='ra-lock-head fia-declare'></div>
                            <div className='ra-lock-shadow fia-declare'></div>
                        </div>}
                        <span className="path1"></span>
                        <span className="path2"></span>
                        <span className="path3"></span>
                        <span className="path4"></span>
                        <span className="path5"></span>
                        <span className="path6"></span>
                    </div>
                    <a className="ra-main-cell-link text-overflow ra-common-pre name-padding" tabIndex="0" onKeyDown={this.onKeyDown} onClick={this.onCellClick.bind(this)} data-tooltip aria-label={rowData.NameOrTitle} data-tooltip-wrap="force">
                        {rowData.NameOrTitle}
                    </a>
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.UniqueId}>
                    {rowData.UniqueId}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.RecordType}>
                    {rowData.RecordType}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                {
                    rowData.IsShowTermFullPath && <div className="text-overflow" data-tooltip={true} aria-label={rowData.TermFullPath}>
                        {rowData.TermName}
                    </div>
                }
                {
                    !rowData.IsShowTermFullPath && <div className="text-overflow" onMouseEnter={this.showTermFullPath}>
                        {rowData.TermName}
                    </div>
                }
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={oriRowData.RuleName}>
                    {oriRowData.RuleName}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.RuleAction}>
                    {rowData.RuleAction}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.OnHold}>
                    {rowData.OnHold}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={oriRowData.HoldBy}>
                    {oriRowData.HoldBy}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={oriRowData.HoldTitle}>
                    {oriRowData.HoldTitle}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={oriRowData.ReleaseTime}>
                    {oriRowData.ReleaseTime}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={oriRowData.DisposalDueDate}>
                    {oriRowData.DisposalDueDate}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={oriRowData.RecordOwner}>
                    {oriRowData.RecordOwner}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={createTime}>
                    {createTime}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={declareAsRecordValue}>
                    {declareAsRecordValue}
                </div>
            </Cell>,
            !LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver() && (
                <Cell key={cellIdx++}>
                    <div className="text-overflow" data-tooltip aria-label={lockWithRecordsLabelValue}>
                        {lockWithRecordsLabelValue}
                    </div>
                </Cell>
            ),
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.CreatedBy}>
                    {rowData.CreatedBy}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.ModifiedBy}>
                    {rowData.ModifiedBy}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={modifiedTime}>
                    {modifiedTime}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={archivedTime}>
                    {archivedTime}
                </div>
            </Cell>,
            
        ].filter(Boolean);

        if(isContainsPhySource){
            commonPhyUniqueCells = [
                <Cell key={cellIdx++}>
                    <div className="text-overflow" data-tooltip aria-label={oriRowData.PersonHoldBy}>
                        {oriRowData.PersonHold ? RMResx.RM_PRM_PRE_Cell_HoldStatusYes : RMResx.RM_PRM_PRE_Cell_HoldStatusNo}
                    </div>
                </Cell>,
                <Cell key={cellIdx++}>
                    <div className="text-overflow" data-tooltip aria-label={oriRowData.PersonHoldBy}>
                        {oriRowData.PersonHoldBy}
                    </div>
                </Cell>
            ];
        }

        let allColumns = [...commonCells,...commonPhyUniqueCells, ...customCells];
        return (
            <Row>
                {allColumns}
            </Row>
        );
    }
}