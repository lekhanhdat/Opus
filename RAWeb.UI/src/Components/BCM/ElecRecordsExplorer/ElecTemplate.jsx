import { bindEvents } from "../../../Utilities/CommonUtil";
import { SourceFlags } from "../../../Constants/Constants";

const groupItems = [
    { displayName: RMResx.RM_JS_BCM_Explorer_MRR_Button_ViewDetails, action: 'ViewDetail' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_ChangeTerm, action: 'Reclassify' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_PutOnHold, action: 'PlaceHold' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_ChangeHold, action: 'ChangeHold' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_CancelHold, action: 'RemoveHold' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_SuspendHold, action: 'ExtendHold' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_MoveRecords, action: 'Move' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_ManageRelatedRecords, action: 'Relate' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_DeclareAsSharePointRecord, action: 'Declare' },
    { displayName: RMResx.RM_JS_BCM_Explorer_Button_UndeclareAsSharePointRecord, action: 'UnDeclare' },
];

export default class RuleTableRow extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        bindEvents(this, "cellClick", "cellKeyDown", "onSelectBtnOption", "showTermFullPath");
    }

    getCellOptBtnsShow(rowData) {
        let cellOptBtns = RM.deepcopy(groupItems);
        let actOfShowBtns = [];
        let isSP = rowData.NodeType == 500 && rowData.SourceFlag == SourceFlags.SP;
        let isExo = rowData.NodeType == 5110;
        let isFS = rowData.SourceFlag == SourceFlags.FS;
        let isFolder = rowData.NodeType == 2100;
        let isFile = rowData.NodeType == 2200;
        let isSPOnPrem = rowData.SourceFlag == SourceFlags.SPLocal;
        let isOneDrive = rowData.NodeType == 500 && rowData.SourceFlag == SourceFlags.OneDrive;
        let isDeclareAsRecord = rowData.DeclareAsRecord;
        let isHolded = rowData.HoldStatus;
        let isSPList = rowData.ExtensionForFile == RMResx.RM_RDM_RecordDetails_DataType_SPItem;
        if (isSP && !isDeclareAsRecord) {
            actOfShowBtns.push('Relate');
        }
        if (!isSPList && (isSP || isOneDrive)) {
            actOfShowBtns.push('Move');
        }
        if (isSP || isExo || isFile || isFolder || isSPOnPrem || isOneDrive) {
            actOfShowBtns.push('Reclassify');
        }
        if (!isDeclareAsRecord && (isSP || isSPOnPrem || isOneDrive)) {
            actOfShowBtns.push('Declare');
        }
        if (isDeclareAsRecord && (isSP || isSPOnPrem || isOneDrive)) {
            actOfShowBtns.push('UnDeclare');
        }
        //FS folder table中view detail是跳转到下一级，因此在button group中添加view detail按钮；
        if (isFS && isFolder) {
            actOfShowBtns.push('ViewDetail');
        }

        if (isHolded) {
            actOfShowBtns.push('ChangeHold', 'RemoveHold', 'ExtendHold');
        } else {
            actOfShowBtns.push('PlaceHold');
        }

        cellOptBtns = cellOptBtns.filter((item) => {
            return actOfShowBtns.indexOf(item.action) != -1;
        });
        return cellOptBtns;
    }

    getSourceIconClass(flag) {
        switch (flag) {
            case SourceFlags.SP:
                return "fi-ms-sharepoint";
            case SourceFlags.FS:
                return "electronic-fs-icon";
            case SourceFlags.Exo:
                return "fi-ms-exchange";
            case SourceFlags.Phy:
                return "fia-physical-record";
            case SourceFlags.SPLocal:
                return "electronic-sp-onPrem-icon";
            case SourceFlags.OneDrive:
                return "fi-ms-onedrive";
        }
    }

    getSourceToolTip(flag) {
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
        }
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
    };

    onSelectBtnOption(item) {
        this.dispatch('cellOperate', item);
    }

    cellClick(action) {
        this.dispatch("cellClick", action);
    }

    cellKeyDown = (event) => {
        if (event.keyCode == "13") {
            this.dispatch("cellClick");
        }
    };

    showTermFullPath = (event) => {
        this.dispatch("showTermFullPath");
    };

    renderRowCell(Row, Cell) {
        let rowData = this.props.rowData;
        let sourceIcon = this.getSourceIconClass(rowData.SourceFlag);
        let sourceToolTip = this.getSourceToolTip(rowData.SourceFlag);
        let isDeclareAsRecord = rowData.DeclareAsRecord;
        let cellOptBtns = this.getCellOptBtnsShow(rowData);
        return <Row>
            <Cell>
                <div className="ra-table-frozen-mar">
                    <R.ButtonGroup type="action" height={200}>
                        {
                            cellOptBtns.map((item, key) => (
                                <R.Button
                                    key={key}
                                    onClick={this.onSelectBtnOption.bind(this, item)}
                                    text={item.displayName} />
                            ))
                        }
                    </R.ButtonGroup>
                </div>
            </Cell>
            <Cell>
                <div className="ra-table-checkbox ra-table-frozen-mar">
                    <R.Checkbox
                        checked={this.props.rowData.isChecked || false}
                        onChange={this.onCheckChange}
                    />
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow ra-position-relative ra-table-frozen-mar">
                    <div tabIndex="0" className={sourceIcon} data-tooltip aria-label={sourceToolTip}>
                        {isDeclareAsRecord && <div>
                            <div className='ra-lock-head fia-declare'></div>
                            <div className='ra-lock-shadow fia-declare'></div>
                        </div>}
                    </div>
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow ra-table-frozen-mar" data-tooltip aria-label={rowData.LeafName} onClick={this.cellClick} onMouseDown={this.cellKeyDown}>
                    <a style={{ color: "#0072d0" }}>{rowData.LeafName}</a>
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ExtensionForFile}>
                    {rowData.ExtensionForFile}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.RecordsId}>
                    {rowData.RecordsId}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.CreatedBy}>
                    {rowData.CreatedBy}
                </div>
            </Cell>
            <Cell>
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
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.RuleName}>
                    {rowData.RuleName}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.DisposalAction}>
                    {rowData.DisposalAction}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.DisposalDueDate}>
                    {rowData.DisposalDueDate}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.RecordOwner}>
                    {rowData.RecordOwner}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.HoldStatusName}>
                    {rowData.HoldStatusName}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.DeclareAsRecordString}>
                    {rowData.DeclareAsRecordString}
                </div>
            </Cell>
        </Row>;
    }

    render(Row, Cell) {
        return this.renderRowCell(Row, Cell);
    }
}