class TableTemplate extends R.TableRow {
    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        aria-label={rowData.Name}
                    >
                        {rowData.Name}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        aria-label={rowData.Description}
                    >
                        {rowData.Description}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export default TableTemplate