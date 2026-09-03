import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import AddCorrelateConnectionTable from "./Table/AddCorrelateConnectionTable";
export default class AddCorrelateConnections extends R.Component {
    idAttr = true;
    componentCreate() {
        this.addCorrelateTableId = "ra-add-correlate-conn-table";
        this.tableColumns = this.getColums();
        this.state = {
            isHidden: true,
            actionButtonsDisable: true
        };
    }

    componentInit() {

    }

    componentReceive(action, ...args) {
        let newList = [];
        switch (action) {
            case "onInit":
                //args[0] -- No Group
                //args[1] -- Current
                args[0].forEach(noGroupConn => {
                    if (!args[1].find(c => c.Id == noGroupConn.Id)) {
                        newList.push(noGroupConn);
                    }
                });
                this.dispatch(this.addCorrelateTableId, RM.deepcopy(newList), this.tableColumns);
                break;
            case "onAdd":
                if (this.selectedItems) {
                    args[0](this.selectedItems.filter(t => t.isChecked));
                }
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

    onDelete = ()=>{

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
            {/* To prevent table Receive Panel Dispatch */}
            {this.state.isHidden && <div></div>} 
            <AddCorrelateConnectionTable
                id={this.addCorrelateTableId}
                columnInfo={this.tableColumns}
                onCheckChanged={this.onCheckChanged}
                onSort={this.onSort}
            />
        </div>;
    }
}