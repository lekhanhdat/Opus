class LoanedRecordsTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;
        return (
            <Row>
                <Cell>{rowData.uniqueId}</Cell>
                <Cell>{rowData.requestedBy}</Cell>
                <Cell>{rowData.requestId}</Cell>
            </Row>
        );
    }
}

export default LoanedRecordsTableTemplate;
