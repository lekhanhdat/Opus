import { LevelType } from "./Constants";

export default class RestoreCenterTable extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            items: [],
            pager: {},
            columns: this.props.columns || [],
        };
        this.cacheItems = [];
        this.uniqueKey = this.props.uniqueKey;
    }

    componentReceive(action, data) {
        switch (action) {
            case "seletedAll":
                this.setState({ items: data.items });
                break;
            default:
                this.setTableInfo(data);
                break;
        }
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
        }
        this.setState({ items: items || [], columns: this.props.columns });
    }

    setCachedItems(items) {
        this.cacheItems = items;
    }

    selectChange = (args, isAll) => {
        let cacheItemsIds = this.cacheItems.map((item) => { return item[this.uniqueKey]; });
        let currentPageNotSelectedIds = this.state.items.filter((item) => { return !item.checked; }).map((obj) => { return obj[this.uniqueKey]; });
        for (let item of args) {
            if (!cacheItemsIds.includes(item[this.uniqueKey])) { this.cacheItems.push(item); }
        }
        if (args.length && !isAll) {
            this.cacheItems = [];
        } else {
            this.cacheItems = this.cacheItems.filter((item) => { return !currentPageNotSelectedIds.includes(item[this.uniqueKey]); });
        }
        if (this.props.onChange) {
            this.props.onChange(this.cacheItems);
        }
    }

    render() {
        return <div id={this.props.id}>
            <R.Table
                id="ARCTable"
                columns={this.state.columns}
                rowTemplate={RestoreCenterTemplate}
                rootData={{
                    showLink: this.props.showLink,
                    searchingLevel: this.props.searchingLevel,
                }}
                items={this.state.items}
                checkable={this.props.checkable}
                onCheck={this.selectChange}
                onRowEvent={this.props.onRowEvent}
                onColumnResize={(column, width) => {
                    const updatedColumns = this.state.columns.map((col) => col.header === column.header ? { ...col, width } : col);
                    this.setState({ columns: updatedColumns });
                    this.props.onResizeColumn(column, width);
                }}
            />
        </div>;
    }
}

class RestoreCenterTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    clickSiteCollection() {
        this.dispatch('getSCData');
    }

    onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    replaceObjectName = (objectName) => {
        const { searchingLevel } = this.props.rootData;
        if (searchingLevel == LevelType.List && (objectName.includes("%1") || objectName.includes("%2"))) {
            return objectName.replace(/\%1/g, '%').replace(/\%2/g, "'\'");
        }
        return objectName;
    }

    render(Row, Cell) {
        const { rowData, rootData } = this.props;
        const { showLink, searchingLevel } = rootData;
        const location = rowData.Location;
        let objectName = this.replaceObjectName(rowData.Origin?.ObjectName ?? rowData.ObjectName);

        if (searchingLevel == LevelType.SiteCollection) {
            if (location.includes("/sites/")) {
                objectName = location.split("/sites/")[1];
            } else if (location.includes("/personal/")) {
                objectName = this.replaceObjectName(rowData.Origin?.ObjectName ?? rowData.ObjectName);
            } else {
                objectName = 'Communication site';
            }
        }

        return <Row>
            <Cell>
                {showLink ? (
                    <div tabIndex={0} className="ra-main-cell-link text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={objectName} onKeyDown={this.onKeyDown} onClick={this.clickSiteCollection.bind(this)}>{objectName}</div>
                ) : (
                    <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={objectName}>{objectName}</div>
                )}
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip data-tooltip-wrap="force" aria-label={rowData.Location}>{rowData.Location}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.CreatedDate}>{rowData.CreatedDate}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.LastModifiedTime}>{rowData.LastModifiedTime}</div>
            </Cell>
            <Cell>
                <div className="text-overflow" data-tooltip aria-label={rowData.ArchivedTime}>{rowData.ArchivedTime}</div>
            </Cell>
            {rowData.HasNewCriteras && (
                <>
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.Author}>{rowData.Author}</div>
                    </Cell>
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.ModifiedBy}>{rowData.ModifiedBy}</div>
                    </Cell>
                    <Cell>
                        <div className="text-overflow" data-tooltip aria-label={rowData.MainJobId}>{rowData.MainJobId}</div>
                    </Cell>
                </>
            )}
            {RM.gData.enableSoftDelete && rowData.HasSoftDelete && (
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.IsSoftDeleted ? RMResx.RM_AR_RC_SearchTitle_SoftDeleted_Yes : RMResx.RM_AR_RC_SearchTitle_SoftDeleted_No}>{rowData.IsSoftDeleted ? RMResx.RM_AR_RC_SearchTitle_SoftDeleted_Yes : RMResx.RM_AR_RC_SearchTitle_SoftDeleted_No}</div>
                </Cell>
            )}
        </Row>;
    }
}