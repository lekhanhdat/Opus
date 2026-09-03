import { Columns } from "../Constants";

class ConnectionSimpleTableRow extends R.TableRow {
    onCheckedChange = () => {
        this.dispatch('checked');
    };

    render(Row, Cell) {

        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        onChange={this.onCheckedChange}
                        checked={this.props.rowData.checked}
                    />
                </Cell>
                <Cell>
                    <div
                        className="reco-az-table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.name}
                        onClick={this.onConnectionClick}
                    >
                        {rowData.name}
                    </div>
                </Cell>
            </Row>
        );
    }
}

export default class ConnectionSimpleTable extends R.Component {
    componentCreate() {
        this.state = {
            isCheckedSelectedAll: this.props.items.length > 0 && !this.props.items.some(i => !i.checked),
            items: this.props.items,
            isDesc: -1,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                isCheckedSelectedAll: items.length > 0 && items.some(i => i.checked) && !items.some(i => !i.checked),
                items: items,
            };
        }

        return null;
    }

    onRowEvent = (args, actionType) => {
        switch (args.type) {
            case "checked":
                this.onItemCheckedChange(args.rowData);
                break;
            default:
                break;
        }
    }

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];

        const existItem = items.find(i => i.id === item.id);
        existItem.checked = !existItem.checked;
        const needUpdateSelectedStatus = !items.some(i => !i.checked);

        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked();
    }

    onCheckedSelectAll = () => {

        const needUpdateSelectedStatus = !this.state.isCheckedSelectedAll;

        const items = [...this.state.items];
        items.forEach(item => {
            item.checked = needUpdateSelectedStatus;
        });

        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked();
    }

    getColumns = () => {
        return [
            {
                headerTemplate: <R.Checkbox
                    checked={this.state.isCheckedSelectedAll}
                    onChange={this.onCheckedSelectAll}
                />,
                width: 60,
                visible: true,
            },
            {
                header: Columns.ConnectionName,
                width: 300,
                resizeable: true,
            },
        ];
    }

    render() {
        return (
            <>
                <R.Table
                    id={this.props.tableId}
                    rowTemplate={ConnectionSimpleTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.getColumns()}
                    frozenCount={1}
                />
            </>
        );
    }
}