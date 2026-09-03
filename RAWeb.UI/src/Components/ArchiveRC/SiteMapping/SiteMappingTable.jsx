export default class SiteMappingTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
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

    getColumns = () => {
        return [
            {
                header: RMResx.RM_AR_RC_TableCol_SourceSite,
                width: 250,
            },
            {
                header: RMResx.RM_AR_RC_TableCol_DestinationSite,
                width: 250,
            },
        ];
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
        return (
            <R.Table
                id={"raSiteMappingTable"}
                rowTemplate={TableTemplate}
                items={this.state.items}
                columns={this.getColumns()}
                onCheck={this.selectChange}
                checkable={this.props.checkable}
            />
        );
    }
}


class TableTemplate extends R.TableRow {

    render(Row, Cell) {

        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.SourceSiteUrl}>{rowData.SourceSiteUrl}</div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.TargetSiteUrl}>{rowData.TargetSiteUrl}</div>
                </Cell>
            </Row>
        );
    }
}