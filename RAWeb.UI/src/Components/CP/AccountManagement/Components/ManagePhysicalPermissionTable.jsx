// import { SourceFlags } from "../../../../Constants/Constants";

export default class ManagePhysicalPermissionTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            disabled: false,
            // isSelectAll: false,
            items: [],
            columns: [],
            rootData: {
                disabled: false,
                isAdmin: false
            },
        };
        this.bind('onRowEvent');
    }

    componentInit() {

    }

    componentReceive(dataList, columnInfo, disabledStatus) {
        let columns = [...columnInfo];
        this.setState({
            items: dataList,
            columns: columns,
            disabled: disabledStatus
        }, () => {
            // this.resetSelectAllStatus(dataList);
        });
    }

    onRowEvent = (args, selectedOption) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData);
                break;
            default:
                break;
        }
    };

    cellClick(data, selectedOption) {
        this.props.cellClick(data, selectedOption);
    }

    render() {
        let tableClass = this.state.disabled? "record-table-disabled" : "";
        return (
            <div className="record-table-wrapper">
                <div className={tableClass}>
                    <R.Table
                        id="corr-table-manage-permission"
                        rootData={this.state.rootData}
                        columns={this.state.columns}
                        rowTemplate={ManagePhyPermissionTableRowTemplate}
                        items={this.state.items}
                        onRowEvent={this.onRowEvent}
                    />
                </div>
            </div>
        );
    }
}

class ManagePhyPermissionTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onEditModuleClick = (data) =>{
        this.dispatch('cellClick', data);
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        return (
            <Row className="ra-scope-table-row">
                <Cell>
                    <R.Button
                        type="bald"
                        icon="fia-edit"
                        tooltip={RMResx.RM_CP_AM_EditPermission_Title}
                        className="padding-xs"
                        onClick={this.onEditModuleClick.bind(this, rowData)} />
                </Cell>
                <Cell>
                    {/* {<span> */}
                    <span className="extra-long-text" data-tooltip aria-label={rowData.Name}>
                        {rowData.Name}
                    </span>
                    {/* </span> */}
                    {/* } */}
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.PermissionNames}>
                        {rowData.PermissionNames}
                    </div>
                </Cell>
            </Row>
        );
    }
}
