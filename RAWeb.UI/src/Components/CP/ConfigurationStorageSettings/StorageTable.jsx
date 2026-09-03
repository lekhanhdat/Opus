import { LicenseHelper } from "../../../Utilities/CommonUtil";

export default class StorageTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            columns: this.props.columns,
            items: [],
            pager: {},
            indexDeviceId: "",
        };
        this.cacheItems = [];
        this.uniqueKey = this.props.uniqueKey;
    }

    componentReceive(storageData) {
        this.setTableInfo(storageData);
    }

    setTableInfo(data) {
        this.setState({ columns: data.columns });
        let items = data.items;
        let indexDeviceId = data.indexDeviceId;
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
                    item.isIndex = item.Id == indexDeviceId;
                }
            }
            this.setState({ items: items, indexDeviceId: indexDeviceId });
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
        this.props.cellClick(rowData);
    }

    render() {
        return <div id={this.props.id}>
            <R.Table
                id="StorageTable"
                columns={this.state.columns}
                rowTemplate={StorageTableTemplate}
                items={this.state.items}
                checkable={this.props.checkable}
                onCheck={this.selectChange}
                onRowEvent={this.onRowEvent}
            />
        </div>;
    }
}

class StorageTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
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
        return <Row>
            {this.enableRecordsArchiver && <Cell>
                <div className="text-overflow ra-table-namecol">
                    <div className="ra-table-name">
                        <a className="ra-main-cell-link" data-tooltip aria-label={rowData.Name} tabIndex='0' onClick={this.onCellClick.bind(this, 'cellClick')} onKeyDown={this.onCellClick.bind(this)} >
                            {rowData.Name}
                        </a>
                    </div>
                    {rowData.isIndex && <div className="ra-table-index">{RMResx.RM_AR_CP_GSS_SetIndex}</div>}
                </div>
            </Cell>}
            {!this.enableRecordsArchiver && <Cell>
                <div className="text-overflow ra-table-namecol">
                    <div className="ra-table-name">{rowData.Name}</div>
                    {rowData.isIndex && <div className="ra-table-index">{RMResx.RM_AR_CP_GSS_SetIndex}</div>}
                </div>
            </Cell>}
            {this.enableRecordsArchiver && <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTime}>{rowData.LastModifiedTime}</div>
            </Cell>}
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.LastArchivedTime}>{rowData.LastArchivedTime}</div>
            </Cell>
        </Row>;
    }
}