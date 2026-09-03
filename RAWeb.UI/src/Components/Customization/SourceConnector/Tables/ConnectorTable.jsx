class ConnectorTableRow extends R.TableRow {
    onCheckedChange = () => {
        this.dispatch('checked');
    };

    onConnectionClick = () => {
        this.dispatch('onEdit');
    };

    onConnectionKeyup = (e) => {
        if (e.keyCode === 13) {
            this.dispatch('onEdit');
        }
    }

    onDownloadScheme = () => {
        this.dispatch("onDownloadScheme");
    }

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
                    <a
                        className="table-flex table-link"
                        data-tooltip="true"
                        aria-label={rowData.name}
                        onClick={this.onConnectionClick}
                        onKeyUp={this.onConnectionKeyup}
                        tabIndex="0"
                    >
                        {rowData.name}
                    </a>
                </Cell>
                <Cell>
                    <div
                        className="table-flex"
                        data-tooltip="ifneed"
                        aria-label={rowData.description}
                    >
                        {rowData.description}
                    </div>
                </Cell>
                <Cell>
                    <R.Button
                        tooltip={RMResx.RM_Connector_Scheme}
                        icon="fia-download"
                        type="bald"
                        onClick={this.onDownloadScheme} />
                </Cell>
            </Row>
        );
    }
}

export default class ConnectorTable extends R.Component {
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
            case "onEdit":
                this.props.onEdit(args.rowData);
                break;
            case "onDownloadScheme":
                this.props.onDownloadScheme(args.rowData);
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
                header: RMResx.RM_Connector_ConnectorName,
                width: "20%",
                resizeable: true,
            },
            {
                header:  RMResx.RM_Connector_Description,
                width: "40%",
                resizeable: true
            },
            {
                header:  RMResx.RM_Connector_Scheme,
                width: "20%",
                resizeable: true
            }
        ];
    }

    render() {
        return (
            <>
                <R.Table
                    id={"reco-customize-connector-table"}
                    rowTemplate={ConnectorTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.getColumns()}
                    frozenCount={1}
                />
            </>
        );
    }
}