import {bindEvents} from "../../../Utilities/CommonUtil";
import {UserTypeNames} from "../../../Constants/Constants";
export default class TableRow extends R.TableRow {
    constructor(props) {
        super(props);
        bindEvents(this,'onCellClick');
    }

    onCellClick(event) {
        if (event == 'cellClick') {
            this.dispatch('cellClick');
        } else {
            if (event.keyCode == "13") {
                this.dispatch('cellClick');
            }
        }
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let groupNamesStr = rowData.SecurityGroupNames.map(o => { return RMResx[o] ? RMResx[o] : o; }).join(", ");
        return <Row>
            <Cell>
                <div
                    className="text-overflow"
                    data-tooltip aria-label={rowData.UserPrincipalName}>
                    {rowData.DisplayName}
                </div>
            </Cell>
            <Cell>
                <div
                    className="text-overflow"
                    data-tooltip aria-label={groupNamesStr}>
                    {groupNamesStr}
                </div>
            </Cell>
            <Cell>
                <R.Button
                    type="link"
                    icon="fia-permission"
                    title={RMResx.RM_CP_AccountManagement_ViewAccessLocation}
                    text={RMResx.RM_CP_AccountManagement_ViewAccessLocation}
                    className="table-btn"
                    onClick={this.onCellClick.bind(this, 'cellClick')}
                />
            </Cell>
        </Row>;
    }
}