import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import CorrelateConnectionTable from "./Table/CorrelateConnectionTable";
export default class CorrelateConnections extends R.Component {
    idAttr = true;
    componentCreate() {
        this.correlateTableId = "ra-correlate-conn-table";
        this.tableColumns = this.getColums();
        this.state = {
            actionButtonsDisable: true,
            groupName: ''
        };
    }

    componentInit() {

    }

    componentReceive(action, ...args) {
        // let newList = [];
        switch (action) {
            case "onInit":
                this.groupId = args[1].Id;
                this.setState({ groupName: args[1].Name });
                this.connectionList = args[1].FSConnections;
                this.dispatch(this.correlateTableId, RM.deepcopy(this.connectionList), this.tableColumns);
                break;
            case "onPushConnListToCorrPanel":
                args[0].forEach(conn => {
                    if (!this.connectionList.find(c => c.Id == conn.Id)) {
                        conn.isChecked = false;
                        this.connectionList.push(conn);
                    }
                });
                // this.connectionList = newList;
                this.dispatch(this.correlateTableId, this.connectionList, this.tableColumns);
                break;
            case "onSave":
                args[0](this.connectionList, this.groupId);//TODO xwwang
                break;
        }
    }

    getColums() {
        return [
            {
                header: RMResx.RM_FS_Register_ConnectionTable_ConnectionName,
                width: 150,
                resizeable: true
            }, {
                header: LicenseHelper.EnableJPMCFileSystemFeature() ? RMResx.RM_FS_Register_Path : RMResx.RM_FS_Register_UNCPath,
                width: 240,
                resizeable: true
            }];
    }

    onAdd = ()=>{
        this.props.addAction(this.connectionList);
    }

    onDelete = ()=>{
        let newList =[];
        let removedList = this.selectedItems.filter(t => t.isChecked);
        this.connectionList.forEach(conn => {
            if (!removedList.find(t => t.Id == conn.Id)) {
                newList.push(conn);
            }
        });
        this.connectionList = newList;
        this.dispatch(this.correlateTableId, this.connectionList, this.tableColumns);
        this.setState({
            actionButtonsDisable: true
        });
    }

    onCheckChanged = (items)=> {
        this.selectedItems = items.slice();
        this.setState({
            actionButtonsDisable: this.selectedItems.filter(t => t.isChecked) == 0
        });
    }

    render() {
        return <div id={this.props.id}>
            {/* <div className="margin-top-25"></div> */}
            <div className="margin-top-m">
                {this.state.actionButtonsDisable &&
                <R.Button
                    icon="fia-plus"
                    text={RMResx.RM_FS_Register_Add}
                    onClick={this.onAdd} />
                }
            
                {!this.state.actionButtonsDisable &&
                <R.Button
                    icon="fia-delete"
                    text={RMResx.RM_FS_Register_Remove}
                    onClick={this.onDelete} />
                }

                <div className="margin-top-m">
                    <CorrelateConnectionTable
                        id={this.correlateTableId}
                        columnInfo={this.tableColumns}
                        onCheckChanged={this.onCheckChanged}
                        onSort={this.onSort}
                    />
                </div>
            </div>
        </div>;
    }
}