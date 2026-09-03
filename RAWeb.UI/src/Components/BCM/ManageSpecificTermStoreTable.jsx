
export default class ManageSpecificTermStoreTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            disabled: false,
            items: [],
            columns: [],
            rootData: {
                disabled: false,
                isAdmin: false
            },
        };
        this.bind('onRowEvent');
    }

    componentInit() {

    }

    componentReceive(dataList, columnInfo, disabledStatus) {
        let columns = columnInfo;
        this.setState({
            items: dataList,
            columns: columns,
            disabled: disabledStatus
        });
    }

    onRowEvent = (args, selectedOption) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData);
                break;
            default:
                break;
        }
    };

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
    }

    render() {
        return (
            <div id={this.props.id}>
                <R.Table
                    id="corr-table-termStore"
                    disabled={this.state.disabled}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={SpecificTermStoreTableRowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                />
            </div>
        );
    }
}

class SpecificTermStoreTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onDeleteClick = (data) => {
        this.dispatch('cellClick', data);
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        let termStoreName = rowData.DisplayName? rowData.DisplayName:  rowData.TermStoreName + "(" + rowData.TermStoreId + ")";
        let isShow = rowData.Action != 2;
        return (
            isShow &&  <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.SiteUrl}>
                        {rowData.SiteUrl}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={termStoreName}>
                        {termStoreName}
                    </div>
                </Cell>
                <Cell>
                    {<span>
                        <R.Button
                            id="raTmTermStoreDeleteBtn"
                            type="bald"
                            icon="fia-delete"
                            tooltip={RMResx.RM_JS_TM_RemoveRuleLabel}
                            className="padding-xs"
                            onClick={this.onDeleteClick.bind(this, rowData)} />
                    </span>
                    }
                </Cell>
            </Row>
        );
    }
}
