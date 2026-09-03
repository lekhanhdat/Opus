import { LicenseHelper } from "../../../../../Utilities/CommonUtil";
import { SourceType, AgentSErviceStatusI18Ns, AgentServiceStatusIcon } from "./Constants";

class ConnectionRow extends R.TableRow {

    onCheckedChange = () => {
        this.dispatch('checked');
    };

    render(Row, Cell) {
        var rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        onChange={this.onCheckedChange}
                        checked={this.props.rowData.checked}
                    />
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Name}>
                        {rowData.Name}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.UNCPath}>
                        {rowData.UNCPath}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow">
                        <div className="reco-conn-cfg-conn-icon-center">
                            {
                                (rowData.ValidateStatus !== null && rowData.ValidateStatus !== undefined) ?
                                    <div>
                                        {
                                            rowData.ValidateStatus ?
                                                <span className="fia-status-successful" data-tooltip aria-label={RMResx.RM_JS_JMD_Status_Successful} style={{ color: "#008A2A" }}></span> :
                                                <span className="fia-status-failed" data-tooltip aria-label={RMResx.RM_JS_JMD_Status_Failed} style={{color: "#B82025"}}></span>
                                        }
                                    </div> :
                                    <></>
                            }
                        </div>
                    </div>
                </Cell>
            </Row>
        );
    }

}

class NoStatusConnectionRow extends R.TableRow {

    onCheckedChange = () => {
        this.dispatch('checked');
    };

    render(Row, Cell) {
        var rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        onChange={this.onCheckedChange}
                        checked={this.props.rowData.checked}
                    />
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Name}>
                        {rowData.Name}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.UNCPath}>
                        {rowData.UNCPath}
                    </div>
                </Cell>
            </Row>
        );
    }

}

export class ConnectionTable extends R.Component {

    componentCreate() {
        this.state = {
            isCheckedSelectedAll: false,
            items: this.props.items,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                isCheckedSelectedAll: items.length > 0 && items.some(i => i.checked) && !items.some(i => !i.checked),
                items: items,
            };
        }

        if (LicenseHelper.EnableJPMCFileSystemFeature()) {
            return {
                isCheckedSelectedAll: items.length > 0 && items.every(i => i.checked),
            };
        }

        return null;
    }
    onRowEvent = (args) => {
        switch (args.type) {
            case 'checked':
                this.onItemCheckedChange(args.rowData);
                break;
            default:
                break;
        }
    }

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];
        const existItem = items.find(i => i.Id === item.Id);
        existItem.checked = !existItem.checked;
        const needUpdateSelectedStatus = !items.some(i => !i.checked);
        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });
        const checkedItems = items.filter(i => i.checked);
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedItems);
    }

    onCheckedSelectAll = () => {

        const needUpdateSelectedStatus = !this.state.isCheckedSelectedAll;

        const items = [...this.state.items];
        items.forEach(item => item.checked = needUpdateSelectedStatus);

        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });
        const checkedItems = items.filter(i => i.checked);
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedItems);
    }

    getColumns = () => {
        const columns = [
            {
                headerTemplate: <R.Checkbox checked={this.state.isCheckedSelectedAll} onChange={this.onCheckedSelectAll} />,
                width: 60,
                visible: true,
            },
            {
                header: RMResx.RM_FS_Register_ConnectionTable_ConnectionName,
                width: 150,
                resizeable: true
            },
            {
                header: LicenseHelper.EnableJPMCFileSystemFeature() ? RMResx.RM_FS_Register_Path : RMResx.RM_FS_Register_UNCPath,
                width: 200,
                resizeable: true
            },
        ];
        if (!this.props.isAddConnPanel) {
            columns.push({
                header: RMResx.RM_CP_Agent_Column_Status,
                width: 80,
                resizeable: true
            });
        }
        return columns;
    }

    render() {
        return (
            <R.Table
                id={this.props.id}
                rowTemplate={this.props.isAddConnPanel ? NoStatusConnectionRow : ConnectionRow}
                items={this.state.items}
                onRowEvent={this.onRowEvent}
                columns={this.getColumns()}
            />
        );
    }

}

const GetSourceTypeIconNames = (sourceType) => {
    const iconName = [];

    if ((SourceType.FileSystem | sourceType) === sourceType) {
        iconName.push("reco-conn-cfg-icon-filesytem");
    }

    if ((SourceType.SharePointOnPremise | sourceType) === sourceType) {
        iconName.push("reco-conn-cfg-icon-sharepoint-onpremise");
    }

    return iconName;
};

class AgentRow extends R.TableRow {
    onCheckedChange = () => {
        this.dispatch('checked');
    };

    render(Row, Cell) {


        var rowData = this.props.rowData;

        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        onChange={this.onCheckedChange}
                        checked={this.props.rowData.checked}
                    />
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={rowData.Name}>
                        {rowData.Name}
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow">
                        <div className="reco-conn-cfg-agent-icon-center">
                            {
                                GetSourceTypeIconNames(rowData.SourceType).map((iconName, index) => {
                                    return (
                                        <div key={index} className={`reco-conn-cfg-icon ${iconName}`}>
                                            <span className="path1"></span>
                                            <span className="path2"></span>
                                            <span className="path3"></span>
                                        </div>
                                    );
                                })
                            }
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <div className="text-overflow" data-tooltip aria-label={AgentSErviceStatusI18Ns.get(rowData.Status)}>
                        {
                            <div className="reco-conn-cfg-agent-icon">
                                <span className={`reco-conn-cfg-icon ${AgentServiceStatusIcon.get(rowData.Status)}`}>
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                    <span className="path3"></span>
                                </span>
                                <span className="text-overflow">
                                    {AgentSErviceStatusI18Ns.get(rowData.Status)}
                                </span>
                            </div>
                        }
                    </div>
                </Cell>
            </Row>
        );
    }
}

export class AgentTable extends R.Component {
    componentCreate() {
        this.state = {
            isCheckedSelectedAll: false,
            items: this.props.items,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                isCheckedSelectedAll: items.length > 0 && items.some(i => i.checked) && !items.some(i => !i.checked),
                items: items,
            };
        }

        return null;
    }
    onRowEvent = (args) => {
        switch (args.type) {
            case 'checked':
                this.onItemCheckedChange(args.rowData);
                break;
            default:
                break;
        }
    }

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];
        const existItem = items.find(i => i.Id === item.Id);
        existItem.checked = !existItem.checked;
        const needUpdateSelectedStatus = !items.some(i => !i.checked);
        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });

        const checkedIds = items.filter(i => i.checked).map(i => i.Id);
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedIds);
    }

    onCheckedSelectAll = () => {

        const needUpdateSelectedStatus = !this.state.isCheckedSelectedAll;

        const items = [...this.state.items];
        items.forEach(item => item.checked = needUpdateSelectedStatus);

        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });

        const checkedIds = items.filter(i => i.checked).map(i => i.Id);
        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(checkedIds);
    }

    getColumns = () => {
        return [
            {
                headerTemplate: <R.Checkbox checked={this.state.isCheckedSelectedAll} onChange={this.onCheckedSelectAll} />,
                align: 'center',
                width: 60,
                visible: true,
            },
            {
                header: RMResx.RM_CP_Agent_Column_DisplayName,
                width: 150,
                resizeable: true
            },
            {
                header: RMResx.RM_CP_Agent_Column_Source,
                width: 200,
                resizeable: false
            },
            {
                header: RMResx.RM_CP_Agent_Column_Status,
                width: 140,
                resizeable: true
            }
        ];
    }

    render() {
        return (
            <R.Table
                id={this.props.id}
                rowTemplate={AgentRow}
                items={this.state.items}
                onRowEvent={this.onRowEvent}
                columns={this.getColumns()}
            />
        );
    }
}