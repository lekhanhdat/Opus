export default class ManualReviewTableRow extends R.TableRow {

    render(Row, Cell) {

        const rowData = this.props.rowData;
        
        return <Row>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ReviewTime}>
                    {rowData.reviewTime}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ReviewBy}>
                    {rowData.reviewBy}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.Action}>
                    {rowData.action}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.QuickReason}>
                    {rowData.quickReason}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.Comment}>
                    {rowData.comment}
                </div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ExtendTime}>
                    {rowData.extendTime}
                </div>
            </Cell>
        </Row>;
    }
}