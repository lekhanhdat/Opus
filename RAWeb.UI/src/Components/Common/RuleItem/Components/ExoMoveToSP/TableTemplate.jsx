import { ExoColumnList } from "./Constants";

export class TableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    getExoColumnList() {
        let rowData = this.props.rowData;
        let currentExoColumnList = [];
        RM.deepcopy(ExoColumnList).forEach(item => {
            item.checked = item.value == rowData.ExoColumn;
            currentExoColumnList.push(item);
        });
        return currentExoColumnList;
    }

    onChanged(args0, args1) {
        switch (args0) {
            case 'ExoColumn':
                this.props.rowData.ExoColumn = args1.newValue.value;
                break;
            case 'SPColumn':
                this.props.rowData.SPColumn = args1;
                break;
            default:
                break;
        }
        this.dispatch('setRowData');
    }

    removeData(args) {
        this.dispatch('deleteData');
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let exoColumnList = this.getExoColumnList();
        return <Row>
            <Cell>
                <div className="flex ra-flex-align-top">
                    <div className="ra-move-cell">
                        <R.Combobox
                            id="raExoColumnCom"
                            tooltipField="name"
                            textField="name"
                            valueField="value"
                            checkedField="checked"
                            linkMode={false}
                            searchable={false}
                            items={exoColumnList}
                            onChange={this.onChanged.bind(this, "ExoColumn")}
                        />
                    </div>
                </div>
            </Cell>
            <Cell>
                <div className="flex ra-flex-align-top">
                    <div className="ra-move-cell">
                        <R.Input
                            id="raSPColumnIpt"
                            type="text"
                            maxlength={255}
                            value={rowData.SPColumn}
                            onChange={this.onChanged.bind(this, "SPColumn")}
                        />
                    </div>
                </div>
            </Cell>
            <Cell>
                <R.Button
                    type="bald"
                    icon="crm-criteria fia-close"
                    tooltip={RMResx.RM_JS_Common_Delete}
                    onClick={this.removeData.bind(this)}
                />
            </Cell>
        </Row>;
    }
}