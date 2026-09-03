
export default class ConnectionTable extends R.Component {
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
                width: 400,
                resizeable: true,
            },
            {
                header: RMResx.RM_FS_Register_Description,
                width: 400,
                resizeable: true
            },
            {
                header: RMResx.RM_FS_Register_LastModifiedTime,
                width: 400,
                resizeable: true
            }
        ];
    }

    onRowEvent = (args) => {
        switch (args.type) {
            case "onEdit":
                this.props.onEdit(args.rowData);
                break;
            default:
                break;
        }
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
                    id={"reco-box-conn-table"}
                    rowTemplate={ConnectionTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.getColumns()}
                    onCheck={this.onItemCheckedChange}
                    checkable={true}
                    onCheckByItems={false}
                />
            </>
        );
    }
}

class ConnectionTableRow extends R.TableRow {

    onConnectionClick = () => {
        this.dispatch('onEdit');
    };

    onConnectionKeyup = (e) => {
        if (e.keyCode === 13) {
            this.dispatch('onEdit');
        }
    }

    render(Row, Cell) {

        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <a
                        className="reco-box-table-flex reco-box-table-link"
                        data-tooltip="ifneed"
                        aria-label={rowData.name}
                        tabIndex="0"
                        onClick={this.onConnectionClick}
                        onKeyUp={this.onConnectionKeyup}
                    >
                        {rowData.name}
                    </a>
                </Cell>
                <Cell>
                    <div
                        className="reco-box-table-flex"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label={rowData.description}
                    >
                        {rowData.description}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-box-table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.modified}
                    >
                        {rowData.modified}
                    </div>
                </Cell>
            </Row>
        );
    }
}