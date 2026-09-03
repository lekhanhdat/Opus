export default class WhitelistTable extends R.Component {
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
                header: RMResx.RM_AR_RC_Whitelist_TableCol_SiteCollectionUrl,
                width: 480,
            },
            {
                header: "",
                width: 50,
            },
        ];
    }

    selectChange = (args) => {
        const cacheItemsIds = this.cacheItems.map((item) => { return item[this.uniqueKey]; });
        const currentPageNotSelectedIds = this.state.items.filter((item) => { return !item.checked; }).map((obj) => { return obj[this.uniqueKey]; });
        for (let item of args) {
            if (!cacheItemsIds.includes(item[this.uniqueKey])) { this.cacheItems.push(item); }
        }
        this.cacheItems = this.cacheItems.filter((item) => { return !currentPageNotSelectedIds.includes(item[this.uniqueKey]); });
        if (this.props.onChange) {
            this.props.onChange(this.cacheItems);
        }
    }

    onRowEvent = (args) => {
        const { Id } = args.rowData;
        if (args.type === "removeData") {
            this.props.onDelete(Id);
        }
    }

    render() {
        return (
            <R.Table
                id={"raWhitelistTable"}
                rowTemplate={TableTemplate}
                items={this.state.items}
                columns={this.getColumns()}
                onCheck={this.selectChange}
                checkable={this.props.checkable}
                onRowEvent={this.onRowEvent}
            />
        );
    }
}


class TableTemplate extends R.TableRow {
    removeData(args) {
        this.dispatch("removeData");
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.SiteCollectionUrl} tabIndex={0}>{rowData.SiteCollectionUrl}</div>
                </Cell>
                <Cell>
                    <R.Button
                        type="bald"
                        icon="fia-delete"
                        tooltip={RMResx.RM_AR_RC_Whitelist_RemoveBtn}
                        onClick={this.removeData.bind(this)}
                    />
                </Cell>
            </Row>
        );
    }
}