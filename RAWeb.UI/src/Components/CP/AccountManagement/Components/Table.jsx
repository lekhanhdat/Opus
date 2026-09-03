import GroupTableRow from './RowTemplate';

export default class GroupTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            page: 1,
            pageSize: 10,
            disabled: false,
            isSelectAll: false,
            showActionBtn: false,
            nameVisibility: 'show',
            items: [],
            columns: [],
            rootData: {
                disabled: false,
            },
        };
    }

    componentReceive(datas, columnInfo) {
        let columns = columnInfo;
        this.setState({
            items: datas,
            columns: columns
        });
    }

    onRowEvent = (args, selectedOption) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellOperate':
                this.cellOperate(rowData, selectedOption);
                break;
            case 'cellClick':
                this.cellClick(rowData.Id);
                break;
            default:
                break;
        }
    }

    onSort = (args) => {
        this.props.onSort(args.status === "asc", args.column.valuePath);
    }

    cellOperate(rowData, selectedOption) {
        this.props.cellOperate(rowData, selectedOption);
    }

    cellClick(id) {
        this.props.cellClick(id);
    }
    deleteCurrentRow(rowIndex) {
        this.state.items.splice(rowIndex, 1);
        this.setState({ items: this.state.items.slice() });
    }

    render() {
        return <div>
            <R.Table
                id="groupTable1"
                disabled={false}
                rootData={this.state.rootData}
                columns={this.state.columns}
                rowTemplate={GroupTableRow}
                items={this.state.items}
                onRowEvent={this.onRowEvent}
                doSort={this.onSort}
            />
        </div>;
    }
}