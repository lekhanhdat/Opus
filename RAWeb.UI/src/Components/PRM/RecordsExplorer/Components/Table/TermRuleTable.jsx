export class TermRuleTable extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.defaultPageSize = 5;
        this.allItems = this.props.RowItems;
        this.state = {
            items: this.allItems.slice(0, this.defaultPageSize),
            rootData: {
                items: this.allItems.slice(0, this.defaultPageSize),
            },
            columns: this.initColumns(),
            pagerIndex: 0,
            pagerSize: this.defaultPageSize,
            totalCount: this.allItems.length,
            // shownCount: RM.deepcopy(this.allItems).slice(0, this.defaultPageSize).length
        };

    }

    initColumns() {
        return [{
            header: RMResx.RM_JS_TM_RuleOrderLabel,
            width: 100,
            resizeable: true,
        }, {
            header: RMResx.RM_JS_TM_RuleNameLabel,
            width: 200,
            resizeable: true,
            visible: true,
        }, {
            header: RMResx.RM_JS_TM_RuleObjectLevelLabel,
            resizeable: true,
            width: 200,
        }];
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        let items = nextProps.RowItems.slice();
        this.allItems = items;
        let currentPageItems = this.allItems.slice(0, this.defaultPageSize);
        this.setState({
            items: currentPageItems,
            rootData: { items: currentPageItems },
            totalCount: items.length,
            // shownCount: RM.deepcopy(items).slice(0, this.defaultPageSize).length
        });
    }

    onRowEvent = (args, selectedOption) => {
        let rowIndex = args.rowIndex,
            rowData = args.rowData;
        switch (args.type) {
            case 'cellClick':
                this.cellClick(rowData, selectedOption);
                break;
            default:
                break;
        }
    }

    cellClick(data, selectedOption){
        this.props.cellClick(data, selectedOption);
    }

    handlePageChange = (pagerIndex, pagerSize, callback) => {
        let currentPageItems = this.allItems.slice(pagerIndex * pagerSize, (pagerIndex + 1) * pagerSize);
        this.setState({
            pagerIndex: pagerIndex,
            pagerSize: pagerSize,
            // shownCount: currentPageItems.length,
            items: currentPageItems,
            rootData: { items: currentPageItems },
        });
        callback(true);
    };

    render() {
        return <React.Fragment>
            <R.Table
                id="raPhyTermTable"
                disabled={this.state.disabled}
                rootData={this.state.rootData}
                columns={this.state.columns}
                rowTemplate={ColumnRow}
                minHeight={102}
                items={this.state.items}
                onRowEvent={this.onRowEvent}
            />
            <div className='table-pager'>
                <div className='table-foot-right'>
                    <$g.Pager
                        itemsCount={this.state.totalCount}
                        pagerIndex={this.state.pagerIndex}
                        pagerSize={this.state.pagerSize}
                        showPagerSize={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.handlePageChange} />
                </div>
            </div>
        </React.Fragment>;
    }
}

class ColumnRow extends R.TableRow {
    // cellClick(data, selectedOption){
    //     this.props.cellClick(data, selectedOption);
    // }
    cellClick(action){
        this.dispatch("cellClick", action);
    }
    render(Row, Cell) {
        let rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div title={rowData.RuleOrder}>{rowData.RuleOrder}</div>
                </Cell>
                <Cell>
                    <a className="ra-main-cell-link" onClick={this.cellClick.bind(this, 1)}>{rowData.RuleName}</a>
                </Cell>
                <Cell>
                    <div title={rowData.RuleLevel}>{rowData.RuleLevel}</div>
                </Cell>
            </Row>
        );
    }
}