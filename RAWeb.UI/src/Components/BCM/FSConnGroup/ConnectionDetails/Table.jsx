import { NormalCell } from "../../../Common/TableTemplateCell";
import { FSAgentJobI18N, FSJobStatusI18N } from "./Constants";

export default class FSConnectionDetailsTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            items: [],
            pager: {},
        };
        this.cacheItems = [];
        this.uniqueKey = this.props.uniqueKey;
    }

    componentReceive(data) {
        this.setTableInfo(data);
    }

    setTableInfo(data) {
        let items = data.items;
        let isReset = data.isReset;
        if (items != undefined) {
            if (this.props.checkable) {
                if (isReset) {
                    this.cacheItems = [];
                    this.props.onChange(this.cacheItems);
                }
                let cacheItemsIds = this.cacheItems.map((item) => { return item[this.uniqueKey]; });
                for (let item of items) {
                    item.checked = cacheItemsIds.includes(item[this.uniqueKey]);
                }
            }
            this.setState({ items: items });
        }
    }

    selectChange = (args) => {
        let cacheItemsIds = this.cacheItems.map((item) => { return item[this.uniqueKey]; });
        let currentPageNotSelectedIds = this.state.items.filter((item) => { return !item.checked; }).map((obj) => { return obj[this.uniqueKey]; });
        for (let item of args) {
            if (!cacheItemsIds.includes(item[this.uniqueKey])) { this.cacheItems.push(item); }
        }
        this.cacheItems = this.cacheItems.filter((item) => { return !currentPageNotSelectedIds.includes(item[this.uniqueKey]); });
        if (this.props.onChange) {
            this.props.onChange(this.cacheItems);
        }
    }

    onRowEvent = (args) => {
        let rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.props.cellClick(rowData);
                break;
        }
    }

    onSort = (args) =>{
        if(this.props.onSort){
            this.props.onSort(args.status === "asc", args.column.valuePath);
        }
    }

    render() {
        return <div>
            <R.Table
                id="FSConnectionDetailsTable"
                columns={this.props.columns}
                rowTemplate={ConnectionDetailsTableRowTemplate}
                items={this.state.items}
                checkable={this.props.checkable}
                flexible={this.props.flexible}
                onCheck={this.selectChange}
                onRowEvent={this.onRowEvent}
                doSort={this.onSort}
            />
        </div>;
    }
}

class ConnectionDetailsTableRowTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onCellClick(event) {
        if (event == 'cellClick') {
            this.dispatch('cellClick');
        } else {
            if (event.keyCode == "13") {
                this.dispatch('cellClick');
            }
        }
    }

    render(Row, Cell) {
        let rowData = this.props.rowData;
        const status = FSJobStatusI18N[rowData.Status];
        const jobType = FSAgentJobI18N[rowData.JobType];
        return (
            <Row>
                <Cell>
                    <div className="text-overflow">
                        <a className="ra-main-cell-link" tabIndex='0' onClick={this.onCellClick.bind(this, 'cellClick')}
                            onKeyDown={this.onCellClick.bind(this)} data-tooltip aria-label={rowData.JobId}>
                            {rowData.JobId}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={jobType}>
                        {jobType}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.ConnectionGroupName}>
                        {rowData.ConnectionGroupName}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Path}>
                        {rowData.Path}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={status}>
                        {status}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.StartTime}>
                        {rowData.StartTime}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.EndTime}>
                        {rowData.EndTime}
                    </div>
                </Cell>
                <NormalCell Cell={Cell} contentText={rowData.JobRunBy} tooltip={rowData.JobRunBy} />
            </Row>
        );
    }
}