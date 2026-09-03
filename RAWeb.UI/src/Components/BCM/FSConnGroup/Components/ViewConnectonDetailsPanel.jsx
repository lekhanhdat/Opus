const PAGE_SIZE = 10;
export default class ViewConnectionDetailsPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            connections: [],
            connectionGroupName: "",
            pageIndex: 0,
            pageSize: PAGE_SIZE,
            totalCount: 0,
            // searchKey: "",
        }
    }

    componentReceive(type, data) {
        switch (type) {
            case 'onInit':
                this.setState({ connectionGroupName: data }, () => this.getConnectionDetailsData());
                break;
            default:
                break;
        }
    }

    getViewConnectionDetailsColumns() {
        return [
            {
                header: RMResx.RM_FS_Register_ConnectionName,
                width: 250,
                sortable: false,
            },
            {
                header: RMResx.RM_FS_Register_Path,
                width: 250,
                sortable: false,
            },
        ];
    };

    getConnectionDetailsData = () => {
        let payload = {
            PageSize: this.state.pageSize,
            PageIndex: this.state.pageIndex + 1,
            Filters: [
                {
                    ColumnName: "GroupName",
                    ColumnValues: [this.state.connectionGroupName],
                },
            ],
            // searchKey: this.state.searchKey,
        };
        $$.loading(true);
        let option = {
            url: '/api/ConnectionRegisterApi/QueryConnectionsPager',
            method: "POST",
            data: payload,
        };
        return fetchUtility(option).then((res) => {
            const data = JSON.parse(res);
            if (data?.ConnectionList && data.TotalCount > 0) {
                this.setState({ connections: data.ConnectionList || [], totalCount: data.TotalCount });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handlePageChange = (pageIndex, pageSize, callback) => {
        this.setState({ pageIndex: pageIndex, pageSize: pageSize }, () => {
            this.getConnectionDetailsData().then(() => {
                callback(true);
            });
        });
    }

    render() {
        return (
            <div id={this.props.id}>
                <R.Table
                    id="viewConnectionDetailsTable"
                    rowTemplate={TableTemplate}
                    items={this.state.connections}
                    columns={this.getViewConnectionDetailsColumns()}
                />
                <div className="ra-main-footer">
                    <$g.Pager
                        itemsCount={this.state.totalCount}
                        pagerIndex={this.state.pageIndex}
                        pagerSize={this.state.pageSize}
                        showPagerCounter={true}
                        showPagerSize={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.handlePageChange}
                    />
                </div>
            </div>
        )
    };
}

class TableTemplate extends R.TableRow {
    render(Row, Cell) {
        const rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        aria-label={rowData.Name}
                    >
                        {rowData.Name}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        aria-label={rowData.UNCPath}
                    >
                        {rowData.UNCPath}
                    </div>
                </Cell>
            </Row>
        );
    }
}