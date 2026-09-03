export default class ViewPermissionPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
    }

    getViewPermissionColumns = () => {
        return [
            {
                header: RMResx.RM_JS_FS_UserName,
                width: 250,
                sortable: false,
                valuePath: "DisplayName",
            },
            {
                header: RMResx.RM_JS_FS_Email,
                width: 250,
                sortable: false,
                valuePath: "UserPrincipalName",
            },
        ];
    };

    onSortInfoOwners = ({ status, column }) => {
        const sortedItems = this.sortItems(
            this.props.connection.InformationOwners,
            column.valuePath,
            status,
        );
        this.props.onSort("InformationOwners", sortedItems);
    };

    onSortRecordsOwners = ({ status, column }) => {
        const sortedItems = this.sortItems(
            this.props.connection.RecordOwners,
            column.valuePath,
            status,
        );
        this.props.onSort("RecordOwners", sortedItems);
    };

    sortItems = (items, valuePath, status) => {
        const sortedItems = [...items].sort((prevItem, nextItem) => {
            const prevValue = (prevItem[valuePath] || "").toLowerCase();
            const nextValue = (nextItem[valuePath] || "").toLowerCase();
            if (prevValue < nextValue) return status === "asc" ? -1 : 1;
            if (prevValue > nextValue) return status === "asc" ? 1 : -1;
            return 0;
        });
        return sortedItems;
    };

    render() {
        return (
            <div>
                <div className="margin-bottom-l">
                    <div className="margin-bottom-s strong" tabIndex="0">
                        {RMResx.RM_FS_Register_Information_Owner}
                    </div>
                    <R.Table
                        id="viewPermissionInfoOwnersTable"
                        rowTemplate={TableTemplate}
                        items={this.props.connection.InformationOwners}
                        columns={this.getViewPermissionColumns()}
                        doSort={this.onSortInfoOwners}
                    />
                </div>
                <div>
                    <div className="margin-bottom-s strong" tabIndex="0">
                        {RMResx.RM_FS_Register_Records_Owner}
                    </div>
                    <R.Table
                        id="viewPermissionRecordsOwnersTable"
                        rowTemplate={TableTemplate}
                        items={this.props.connection.RecordOwners}
                        columns={this.getViewPermissionColumns()}
                        doSort={this.onSortRecordsOwners}
                    />
                </div>
            </div>
        );
    }
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
                        tabIndex="0"
                        aria-label={rowData.DisplayName}
                    >
                        {rowData.DisplayName}
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="text-overflow"
                        data-tooltip
                        data-tooltip-wrap="force"
                        tabIndex="0"
                        aria-label={rowData.UserPrincipalName}
                    >
                        {rowData.UserPrincipalName}
                    </div>
                </Cell>
            </Row>
        );
    }
}
