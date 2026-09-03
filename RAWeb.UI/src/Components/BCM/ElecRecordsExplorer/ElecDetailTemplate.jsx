export class ManualReviewTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return <Row>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ReviewTime}>
                    {rowData.ReviewTime}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ReviewBy}>
                    {rowData.ReviewBy}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.Action}>
                    {rowData.Action}
                </div>
            </Cell>
        </Row>;
    }
}

export class RecordHistoryTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return <Row>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.DisplayTime}>
                    {rowData.DisplayTime}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.User}>
                    {rowData.User}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.Action}>
                    {rowData.Action}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.Comment}>
                    {rowData.Comment}
                </div>
            </Cell>
        </Row>;
    }
}
