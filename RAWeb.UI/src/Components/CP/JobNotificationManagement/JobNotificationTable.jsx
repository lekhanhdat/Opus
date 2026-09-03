import React from "react";
import { JobNotificationTableColumns, IntervalTypeI18N, IntervalType, WeeklyTypesI18N } from "./Constants/index";

class JobNotificationTableRow extends R.TableRow {

    onCheckedChange = () => {
        this.dispatch('checked');
    };

    onAction = (actionType) => {
        this.dispatch("onAction", actionType);
    };

    onCellClick = () =>{
        this.dispatch("onCellClick");
    }; 

    onCellKeyDown = (e) =>{
        if(e.keyCode == "13"){
            this.dispatch("onCellClick");
        }
    }

    render(Row, Cell) {
        
        const rowData = this.props.rowData;
        // const actionButtons = this.getActionButton(rowData, this.props.rootData);
        let userList = [];
        if(rowData.profileEmailReceivers != null){
            userList = rowData.profileEmailReceivers.map((user) => user.DisplayName);
        }

        return (
            <Row>
                <Cell>
                    <R.Checkbox
                        onChange={this.onCheckedChange}
                        checked={this.props.rowData.checked}
                    />
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        aria-label={rowData.profileName}
                    >
                        <a tabIndex={0} onClick={this.onCellClick} onKeyDown={this.onCellKeyDown}>
                            {rowData.profileName}
                        </a>
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label= {rowData.profileDes}
                    >
                        { rowData.profileDes }
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label= {userList.join('; ')}
                    >
                        { userList.join('; ') }
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label= {IntervalTypeI18N.get(rowData.profileInterval.intervalType)}
                    >
                        {rowData.profileInterval.intervalType === IntervalType.Weekly 
                            ? RMResx.RM_JS_JN_Every + " " + WeeklyTypesI18N.get(rowData.profileInterval.weeklyType)
                            : IntervalTypeI18N.get(rowData.profileInterval.intervalType) }
                    </div>
                </Cell>
                <Cell>
                    <div
                        className="reco-manual-review-table-text"
                        data-tooltip="ifneed"
                        data-tooltip-wrap="force"
                        aria-label= {rowData.profileCreatedTime}
                    >
                        { rowData.profileCreatedTime }
                    </div>
                </Cell>
            </Row>
        );
    }
}

export default class JobNotificationTable extends R.Component {

    componentCreate() {
        this.state = {
            items: this.props.items,
            isCheckedSelectedAll: false,
            isShowDeleteButton: false,
        };
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        const items = nextProps.items;
        if (items !== prevState.items) {
            return {
                isCheckedSelectedAll: items.length > 0 && (items.every(i => i.checked) || (items.some(i => i.checked) && 'mixed')),
                items: items,
            };
        }

        return null;
    }

    onItemCheckedChange = (item) => {
        const items = [...this.state.items];

        const existItem = items.find(i => i.profileId === item.profileId);
        existItem.checked = !existItem.checked;
        const needUpdateSelectedStatus = items.every(i => i.checked) || (items.some(i => i.checked) && 'mixed');
        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(items);
    }

    onCheckedSelectAll = () => {

        const needUpdateSelectedStatus = this.state.isCheckedSelectedAll === 'mixed' ||  !this.state.isCheckedSelectedAll;
        const items = [...this.state.items];
        items.forEach(item => {
            item.checked = needUpdateSelectedStatus;
        });

        this.setState({
            isCheckedSelectedAll: needUpdateSelectedStatus,
            items: items
        });

        if (!this.props.onChangeChecked) {
            return;
        }
        this.props.onChangeChecked(items);
    }

    onCellClick = (args) =>{
        this.props.onCellClick(args);
    }

    getColumns = () => {
        return [
            {
                headerTemplate: <R.Checkbox
                    checked={this.state.isCheckedSelectedAll}
                    onChange={this.onCheckedSelectAll}
                />,
                width: 60,
                visible: true,
            },
            ...JobNotificationTableColumns
        ];
    }

    onRowEvent = (args, actionType) => {
        switch (args.type) {
            case "checked":
                this.onItemCheckedChange(args.rowData);
                break;
            case "onAction":
                this.onExecuteAction(args.rowData, actionType);
                break;
            case "onCellClick":
                this.onCellClick(args.rowData);
                break;
            default:
                break;
        }
    }

    render() {
        return (
            <>
                <R.Table
                    id={"reco-manual-under-review-table"}
                    rowKeyField={"rowIndex"}
                    rowTemplate={JobNotificationTableRow}
                    items={this.state.items}
                    onRowEvent={this.onRowEvent}
                    columns={this.getColumns()}
                    frozenCount={1}
                    doSort={this.doSort}
                />
            </>
        );
    }
}