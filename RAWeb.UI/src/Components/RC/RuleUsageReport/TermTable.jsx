export default class TermTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            items: [],
            columns: [],
            rootData: {
                disabled: false,
                isAdmin: false
            },
        };
    }

    render() {
      
        return (
            <div id={this.props.id ?? ""}>
                <R.Table
                    id="raRuleUsageReportTable"
                    disabled={false}
                    rootData={this.state.rootData}
                    columns={this.props.columnInfo}
                    items={this.props.items}
                    rowTemplate={TermTableRowTemplate}
                />
            </div>
        );
    }
}

class TermTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }
    render(Row, Cell) {
        let rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TermName}>
                        {rowData.TermName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TermPath}>
                        {rowData.TermPath}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.TermStatus}>
                        {rowData.TermStatus}
                    </div>
                </Cell>
            </Row>
        );
    }
}