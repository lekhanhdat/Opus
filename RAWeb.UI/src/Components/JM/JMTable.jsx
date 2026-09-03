export default class JMTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            columns: [],
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
        this.setState({ columns: data.columns });
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
            case 'SubJobIDClicked':
            case 'SuccessfulCountClicked':
            case 'FailedCountClicked':
            case 'SkippedCountClicked':
                this.props.onCellClick(args.type, rowData);
                break;
        }
    }

    onSort = (args) =>{
        if(this.props.onSort){
            this.props.onSort(args.status === "asc", args.column.valuePath);
        }
    }

    render() {
        return <div id={this.props.id}>
            <R.Table
                id="JMTable"
                columns={this.state.columns}
                rowTemplate={this.props.template}
                rootData={{
                    jobType: this.props.jobType,
                }}
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