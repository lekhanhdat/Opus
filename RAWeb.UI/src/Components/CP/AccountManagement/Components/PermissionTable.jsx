export default class PermissionTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            items: [],
            columns: [],
            selectedValue: this.props.selectedValue,
            disabled: false
        };
        this.bind('onRowEvent');
    }

    componentInit() {
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "init":
                this.setState({
                    items: args[0],
                    columns: args[1],
                    disabled: args[2]
                });
                break;
            default:
                break;
        }
    }

    onRowEvent = (args) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'checked':
                this.props.setPerSelectedValue(rowData.selectedValue);
                break;
            default:
                break;
        }
    };

    render() {
        return (
            <div>
                <R.Table
                    id="corr-table3"
                    disabled={this.state.disabled}
                    rootData={this.state.rootData}
                    columns={this.state.columns}
                    rowTemplate={ConfigPermissionTableRowTemplate}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                />
            </div>
        );
    }
}

class ConfigPermissionTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onCheckChange = (value) => {
        this.props.rowData.selectedValue = value;
        this.dispatch("checked");
        this.setState({});
    };

    getPermissionOptions(selectedValue) {
        let options = [
            { text: RMResx.RM_CP_AM_PhysicalPermission_Admin, value: "1" },
            { text: RMResx.RM_CP_AM_PhysicalPermission_EndUser, value: "2" }
        ];
        options.forEach((op) => {
            op.title = op.text;
            op.checked = selectedValue == op.value;
        });
        return options;
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div className="text-overflow">
                        <R.Radio.Group
                            name="radiogroup-permission"
                            items={this.getPermissionOptions(rowData.selectedValue)}
                            onChange={this.onCheckChange}
                            block={true}
                        />
                    </div>
                </Cell>
            </Row>
        );
    }
}
