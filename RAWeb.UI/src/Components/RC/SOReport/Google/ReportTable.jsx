import { NormalCell } from "../../../Common/TableTemplateCell";

export default class GoogleReportTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            columns: this.props.columns,
            items: [],
        };
        this.cacheItems = [];
        this.uniqueKey = this.props.uniqueKey;
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

    render() {
        return <div>
            <R.Table
                id="GoogleReportTable"
                columns={this.state.columns}
                rowTemplate={Template}
                items={this.state.items}
                checkable={this.props.checkable}
                onCheck={this.selectChange}
            />
        </div>;
    }
}


class Template extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    render(Row, Cell) {
        let {
            SiteUrl,
            TotalSize,
            TotalDeleteSize,
        } = this.props.rowData;

        return <Row>
            <NormalCell Cell={Cell} contentText={SiteUrl} />
            <NormalCell Cell={Cell} contentText={TotalSize} />
            <NormalCell Cell={Cell} contentText={TotalDeleteSize} />
        </Row>;
    }
}