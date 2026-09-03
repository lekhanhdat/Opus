export default class AddConnectionTable extends R.Component {
    componentCreate() {
        this.state = {
            items: this.props.items,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                items: items,
            };
        }

        return null;
    }

    getColumns = () => {
        return [
            {
                header: RMResx.RM_FS_Register_ConnectionName,
                width: 300,
                resizeable: true,
            },
        ];
    }

    onItemCheckedChange = (checkedItem) => {
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedItem);
    };

    render() {
        return (
            <>
                <R.Table
                    id={this.props.tableId}
                    checkable={true}
                    items={this.state.items}
                    columns={this.getColumns()}
                    onCheck={this.onItemCheckedChange}
                    rowTemplate={AddConnectionTableRow}
                    onCheckByItems={false}
                />
            </>
        );
    }
}

class AddConnectionTableRow extends R.TableRow {

    render(Row, Cell) {

        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div
                        className="reco-box-table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.name}
                    >
                        {rowData.name}
                    </div>
                </Cell>
            </Row>
        );
    }
}