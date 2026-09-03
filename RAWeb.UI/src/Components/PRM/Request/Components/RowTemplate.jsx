import { RequestStatus, PhysicalRequestType } from "../../Constants";

export default class RequestTableRow extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onCheckChange = (checked) => {
        this.props.rowData.isChecked = checked;
        this.dispatch("checked");
    }

    onSelectChange = (item) => {
        this.dispatch("cellOperate", item);
    }

    cellClick(action, index) {
        this.dispatch("cellClick", action, index);
    }

    cellKeyDown(action, event) {
        if (event.keyCode == "13") {
            this.dispatch("cellClick", action);
        }
    }

    convertOjbStr(source, status) {
        let state = source.filter(item => { return item.id == status; });
        return state.length > 0 ? state[0].name : "";
    }

    getTableActionBtns(groupItems) {
        let rowData = this.props.rowData;
        return <React.Fragment>
            {groupItems.map((item, key) => (
                <R.Button
                    key={key}
                    onClick={this.onSelectChange.bind(this, item)}
                    disabled={rowData.Status != 0}
                    text={item.displayName} />
            ))}
        </React.Fragment>;
    }

    renderAdminRowCell(Row, Cell) {
        let rowData = this.props.rowData;
        let groupItems = [
            { displayName: RMResx.RM_RC_Request_Action_ApprovalRequest, index: 1 },
            { displayName: RMResx.RM_RC_Request_Action_RejectRequest, index: 2 },
        ];
        let tableActionBtns = this.getTableActionBtns(groupItems);
        const title = rowData.Title;
        const titles = rowData.Titles;
        const recordId = rowData.RecordId;
        const recordIds = rowData.RecordIds;
        let allTitles = [title];
        let allRecordsIds = [recordId];
        if(titles && recordIds && titles.length > 0 && recordIds.length > 0){
            allTitles = [...titles];
            allRecordsIds = [...recordIds];
        }
        return <Row action= {tableActionBtns}>
            <Cell>
                <R.Checkbox
                    id={"raPrmRequestTableCheckbox" + this.props.index}
                    checked={this.props.rowData.isChecked || false}
                    onChange={this.onCheckChange}
                />
            </Cell>
            <Cell><a className="ra-main-cell-link" tabIndex="0" onClick={this.cellClick.bind(this, 1)} onKeyDown={this.cellKeyDown.bind(this, 1)} >{rowData.RequestId}</a></Cell>
            <Cell>
                <div className="ra-request-uniqueID" data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={allTitles.length == 1 ? allTitles[0] : allTitles.join(', ')}>
                    {allTitles.map((item, index) => (
                        <span key={index}>
                            {item}
                            {index < allTitles.length - 1 && ", "}
                        </span>
                    ))}
                </div>
            </Cell>
            <Cell>
                {allRecordsIds.length > 0 && (
                <div className="ra-request-uniqueID" data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={allTitles.length == 1 ? allRecordsIds[0] : allRecordsIds.join(', ')}>
                    {allRecordsIds.map((item, index) => (
                        <>
                            <a key={index} className="ra-main-cell-link" tabIndex="0" onClick={this.cellClick.bind(this, 2, index)} onKeyDown={this.cellKeyDown.bind(this, 2)}>
                                {item}
                                {index < allRecordsIds.length - 1 && ","}
                            </a>{" "}
                        </>
                    ))}
                </div>
            )}
            </Cell>
            <Cell>{this.convertOjbStr(PhysicalRequestType, rowData.Type)}</Cell>
            <Cell>{rowData.CreatedTimeStr}</Cell>
            <Cell>{this.convertOjbStr(RequestStatus, rowData.Status)}</Cell>
            <Cell>{rowData.CreatedUserDisplay}</Cell>
        </Row>;
    }

    renderUserRowCell(Row, Cell) {
        let rowData = this.props.rowData;
        let groupItems = [
            { displayName: RMResx.RM_RC_Request_Action_CancelRequest, index: 3 }
        ];
        let tableActionBtns = this.getTableActionBtns(groupItems);
        const title = rowData.Title;
        const titles = rowData.Titles;
        const recordId = rowData.RecordId;
        const recordIds = rowData.RecordIds;
        let allTitles = [title];
        let allRecordsIds = [recordId];
        if(titles && recordIds && titles.length > 0 && recordIds.length > 0){
            allTitles = [...titles];
            allRecordsIds = [...recordIds];
        }
        return <Row action = {tableActionBtns}>
            <Cell>
                <R.Checkbox
                    checked={this.props.rowData.isChecked || false}
                    onChange={this.onCheckChange}
                />
            </Cell>
            <Cell><a className="ra-main-cell-link" tabIndex="0" onClick={this.cellClick.bind(this, 1)} onKeyDown={this.cellKeyDown.bind(this, 1)} >{rowData.RequestId}</a></Cell>
            <Cell>
                <div className="ra-request-uniqueID" data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={allTitles.length == 1 ? allTitles[0] : allTitles.join(', ')}>
                    {allTitles.map((item, index) => (
                        <span key={index}>
                            {item}
                            {index < allRecordsIds.length - 1 && ", "}
                        </span>
                    ))}
                </div>
            </Cell>
            <Cell>
                {allRecordsIds.length && (
                <div className="ra-request-uniqueID" data-tooltip="ifneed" data-tooltip-wrap="force" aria-label={allTitles.length == 1 ? allRecordsIds[0] : allRecordsIds.join(', ')}>
                    {allRecordsIds.map((item, index) => (
                        <>
                            <a key={index} className="ra-main-cell-link" tabIndex="0" onClick={this.cellClick.bind(this, 2, index)} onKeyDown={this.cellKeyDown.bind(this, 2)}>
                                {item}
                                {index < allRecordsIds.length - 1 && ","}
                            </a>{" "}
                        </>
                    ))}
                </div>
                )}
            </Cell>
            <Cell>{this.convertOjbStr(PhysicalRequestType, rowData.Type)}</Cell>
            <Cell>{rowData.CreatedTimeStr}</Cell>
            <Cell>{this.convertOjbStr(RequestStatus, rowData.Status)}</Cell>
        </Row>;
    }

    render(Row, Cell) {
        let rootData = this.props.rootData;
        // const style = {width: "100%", height: "40px", padding: "0 10px"}
        return (
            rootData.isAdmin ? this.renderAdminRowCell(Row, Cell) : this.renderUserRowCell(Row, Cell)
        );
    }
}