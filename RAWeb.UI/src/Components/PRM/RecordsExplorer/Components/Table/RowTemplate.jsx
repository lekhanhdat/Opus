import {
    PhysicalDefaultColumnIDs,
    PhysicalObjectStatusNames,
    PhysicalObjectStatus
} from "../../../../../Constants/Constants";
import {
    PhyNodeTypeNames,
    YesOrNo
} from "../../../Constants";
import { NodeType } from "../../../../../Constants/DAEnums";
import { bindEvents } from "../../../../../Utilities/CommonUtil";
import RuleUtil from "../../../../../Utilities/RuleUtil";

const groupItems = [
    { displayName: RMResx.RM_PRM_PRE_TableOption_Edit, index: 1 },
    { displayName: RMResx.RM_PRM_PRE_TableOption_Delete, index: 2 }
];

let isAllowDelete = function (item) {
    let allow = true;
    if (item.DisposalHold || item.PersonHold) {
        allow = false;
    }
    return allow;
};

let isAllowEdit = function (item) {
    let allow = true;
    if (item.Status === PhysicalObjectStatus.Destroyed) {
        allow = false;
    }
    return allow;
};

let showBtnsInCell = function (rowData) {
    let newGroupItems = groupItems.slice(0);
    if (!isAllowDelete(rowData)) {
        newGroupItems = [groupItems[0]];
    }
    if (!isAllowEdit(rowData)) {
        newGroupItems = [groupItems[1]];
    }
    return newGroupItems;
};

export class LocationExceptRoomAndFileRT extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        bindEvents(this, "onCellClick", "onSelectChange");
    }

    onSelectChange(item) {
        this.dispatch('cellOperate', item);
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
        this.setState({});
    };

    onCellClick() {
        this.dispatch('cellClick');
    }

    render(Row, Cell) {
        let rowData = this.props.rowData,
            MetaInfo = rowData.MetaInfo,
            rootData = this.props.rootData,
            NameOrTitle = '';
        let modifiedTime = rowData.ModifiedTime == 0 ? "" : rowData.ModifiedTimeStr;
        let createTime = rowData.CreateTimeStr;
        let isPhyFileNode = rootData.curNodeType == NodeType.PhyFile;
        let btnsInCell = showBtnsInCell(rowData);
        if (isPhyFileNode) {
            NameOrTitle = <a className="ra-main-cell-link"
                onClick={this.onCellClick.bind(this)}>{MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle]}</a>;
        } else {
            NameOrTitle = MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle];
        }
        let cellIdx = 0;
        let cells = [
            rootData.showActions &&
            <Cell key={cellIdx++}>
                <div className="ra-table-frozen-mar">
                    <R.ButtonGroup type="action" height={200}>
                        {
                            btnsInCell.map((item, key) => (
                                <R.Button
                                    key={key}
                                    onClick={this.onSelectChange.bind(this, item)}
                                    text={item.displayName} />
                            ))
                        }
                    </R.ButtonGroup>
                </div>
            </Cell>,
            rootData.showCheckbox &&
            <Cell key={cellIdx++}>
                <R.Checkbox
                    checked={rowData.isChecked || false}
                    onChange={this.onCheckChange}
                />
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow ra-table-frozen-mar" data-tooltip aria-label={MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle]}>
                    {NameOrTitle}
                </div>
            </Cell>,
            isPhyFileNode && <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.UniqueId}>
                    {rowData.UniqueId}
                </div>
            </Cell>,
            !isPhyFileNode && <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={MetaInfo[PhysicalDefaultColumnIDs.Capability]}>
                    {MetaInfo[PhysicalDefaultColumnIDs.Capability]}
                </div>
            </Cell>,
            isPhyFileNode && <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={PhysicalObjectStatusNames[rowData.Status]}>
                    {PhysicalObjectStatusNames[rowData.Status]}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.CreatedBy}>
                    {rowData.CreatedBy}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={createTime}>
                    {createTime}
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
        ];
        cells = cells.filter(cell => !!cell);
        return (
            <Row>
                {cells.map(cell => cell)}
            </Row>
        );
    }
}

export class RoomAndBoxRT extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        bindEvents(this, "showTermFullPath");
    }

    onSelectChange(item) {
        this.dispatch('cellOperate', item);
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
        this.setState({});
    };

    onCellClick() {
        this.dispatch('cellClick');
    }

    cellKeyDown = (event) => {
        if (event.keyCode == "13") {
            this.dispatch("cellClick");
        }
    }

    getHoldCellValue(isHold) {
        return isHold ? YesOrNo[0] : YesOrNo[1];
    }

    showTermFullPath = (event) => {
        this.dispatch("showTermFullPath");
    };

    render(Row, Cell) {
        let rowData = this.props.rowData,
            MetaInfo = rowData.MetaInfo,
            rootData = this.props.rootData,
            btnsInCell = showBtnsInCell(rowData);
        let classification = MetaInfo[PhysicalDefaultColumnIDs.Classification] ? JSON.parse(MetaInfo[PhysicalDefaultColumnIDs.Classification]).Name : '';
        let modifiedTime = rowData.ModifiedTime == 0 ? "" : rowData.ModifiedTimeStr;
        let holdUntil = rowData.HoldReleaseTime == 0 ? "" : rowData.HoldReleaseTimeStr;
        let cellIdx = 0;
        let cells = [
            rootData.showActions && <Cell key={cellIdx++}>
                <div className="ra-table-frozen-mar">
                    <R.ButtonGroup type="action" height={200}>
                        {
                            btnsInCell.map((item, key) => (
                                <R.Button
                                    key={key}
                                    onClick={this.onSelectChange.bind(this, item)}
                                    text={item.displayName} />
                            ))
                        }
                    </R.ButtonGroup>
                </div>
            </Cell>,
            rootData.showCheckbox && <Cell key={cellIdx++}>
                <R.Checkbox
                    checked={rowData.isChecked || false}
                    onChange={this.onCheckChange}
                />
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow ra-table-frozen-mar" data-tooltip aria-label={MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle]}>
                    <a className="ra-main-cell-link" tabIndex="0" onClick={this.onCellClick.bind(this)} onKeyDown={this.cellKeyDown}>
                        {MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle]}
                    </a>
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={PhyNodeTypeNames[rowData.NodeType]}>
                    {PhyNodeTypeNames[rowData.NodeType]}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.UniqueId}>
                    {rowData.UniqueId}
                </div>
            </Cell>,
            [NodeType.PhysicalBottomLocation, NodeType.PhyCustom].includes(rootData.curNodeType) && <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={MetaInfo[PhysicalDefaultColumnIDs.Capability]}>
                    {MetaInfo[PhysicalDefaultColumnIDs.Capability]}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={PhysicalObjectStatusNames[rowData.Status]}>
                    {PhysicalObjectStatusNames[rowData.Status]}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                {
                    rowData.IsShowTermFullPath && <div className="text-overflow" data-tooltip={true} aria-label={rowData.TermFullPath}>
                        {classification}
                    </div>
                }
                {
                    !rowData.IsShowTermFullPath && <div className="text-overflow" onMouseEnter={this.showTermFullPath}>
                        {classification}
                    </div>
                }
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.RuleName}>
                    {rowData.RuleName}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={RuleUtil.parseDisposalAction(rowData.RuleAction)}>
                    {RuleUtil.parseDisposalAction(rowData.RuleAction)}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.DisposalDueDate}>
                    {rowData.DisposalDueDate}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.RecordOwner}>
                    {rowData.RecordOwner}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={PhysicalObjectStatusNames[rowData.PersonHold]}>
                    {this.getHoldCellValue(rowData.PersonHold)}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={PhysicalObjectStatusNames[rowData.PersonHoldBy]}>
                    {rowData.PersonHoldBy || RMResx.RM_JS_PRM_PRE_UserIsNull}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={PhysicalObjectStatusNames[rowData.DisposalHold]}>
                    {this.getHoldCellValue(rowData.DisposalHold)}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.HoldBy}>
                    {rowData.HoldBy || RMResx.RM_JS_PRM_PRE_UserIsNull}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={rowData.HoldProfileTitle}>
                    {rowData.HoldProfileTitle}
                </div>
            </Cell>,
            <Cell key={cellIdx++}>
                <div className="text-overflow" data-tooltip aria-label={holdUntil}>
                    {holdUntil}
                </div>
            </Cell>,
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
            </Cell>
        ];
        cells = cells.filter(cell => !!cell);
        return (
            <Row>
                {cells.map(cell => cell)}
            </Row>
        );
    }
}

export class ManageRelatedTableRow extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onSelectChange(item) {
        this.dispatch('cellOperate', item);
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
    }

    onCellClick() {
        this.dispatch('cellClick');
    }

    cellKeyDown = (event) => {
        if (event.keyCode == "13") {
            this.dispatch("cellClick");
        }
    }

    getHoldCellValue(isHold) {
        return isHold ? YesOrNo[0] : YesOrNo[1];
    }

    getCellData(rowData) {
        let d = {},
            isPhyObj = rowData.SourceFlag == 4,
            MetaInfo = JSON.parse(rowData.MetaInfo);
        d.UniqueId = rowData.RecordsId;
        d.ModifiedBy = rowData.ModifiedBy;
        d.RuleAction = isPhyObj ? RuleUtil.parseDisposalAction(rowData.DisposalAction) : RuleUtil.parseDisposalActionForSP(rowData.DisposalAction);
        d.OnHold = this.getHoldCellValue(rowData.HoldStatus);
        if (isPhyObj) {
            d.NameOrTitle = MetaInfo[PhysicalDefaultColumnIDs.NameOrTitle];
            d.TermName = MetaInfo[PhysicalDefaultColumnIDs.Classification] ? JSON.parse(MetaInfo[PhysicalDefaultColumnIDs.Classification]).Name : '';
            d.Source = RMResx.RM_PRM_PRE_MRR_PhysicalSource;
            if (rowData.NodeType == NodeType.PhyFile) {
                d.RecordType = RMResx.RM_PRM_PRE_Filter_PhysicalFile;
            }
            if (rowData.NodeType == NodeType.PhyRecord) {
                d.RecordType = RMResx.RM_PRM_PRE_Filter_PhysicalRecord;
            }
        } else {
            d.NameOrTitle = rowData.LeafName;
            d.TermName = rowData.TermName;
            d.Source = RMResx.RM_PRM_PRE_MRR_ElectricSource;
            d.RecordType = RMResx[rowData.ExtensionForFile] || rowData.ExtensionForFile;
        }
        return d;
    }

    render(Row, Cell) {
        let rowData = this.props.rowData,
            rootData = this.props.rootData,
            d = this.getCellData(rowData);
        return (
            <Row>
                {
                    rootData.showCheckBox && <Cell>
                        <R.Checkbox
                            id={"raPhyRecordTableChk" + this.props.index}
                            checked={rowData.isChecked || false}
                            onChange={this.onCheckChange}
                        />
                    </Cell>
                }
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.Source}>
                        {d.Source}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.NameOrTitle} tabIndex="0" onKeyDown={this.cellKeyDown}>
                        <a className="ra-main-cell-link" onClick={this.onCellClick.bind(this)}>
                            {d.NameOrTitle}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.UniqueId}>
                        {d.UniqueId}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.RecordType}>
                        {d.RecordType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.ModifiedBy}>
                        {d.ModifiedBy}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.TermName}>
                        {d.TermName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.RuleAction}>
                        {d.RuleAction}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={d.OnHold}>
                        {d.OnHold}
                    </div>
                </Cell>
            </Row>
        );
    }
}


