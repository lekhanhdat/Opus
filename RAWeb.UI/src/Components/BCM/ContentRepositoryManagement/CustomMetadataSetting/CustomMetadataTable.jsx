import _ from "lodash";

import { MetadataColumnTypes } from "./Constants";
import { hasDuplicateName } from "../../../../Utilities/CommonUtil";
import { SourceFlags } from "../../../../Constants/Constants";
import { ExoColumnList } from "../../../Common/RuleItem/Components/ExoMoveToSP/Constants";

const metadataDefaultObj = {
    SourceColumnName: "",
    ColumnType: 0,
    ColumnTypeList: [],
};

class CustomMetadataTable extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
    }

    componentCreate() {
        this.state = {
            metadataList: this.getMetadataList(),
            isTooLong: false,
            isEmpty: false,
            isDuplicated: false,
        };
        this.sourceFlag = this.props.sourceFlag;
        this.columns = this.getColumns();
    }

    componentReceive(type) {
        if (type === "didUpdate") {
            this.setState({
                metadataList: this.getMetadataList(),
            });
        }
    }

    getColumns() {
        return [
            {
                header: this.sourceFlag === SourceFlags.Exo ? RMResx.RM_JS_SP_CustomMetadata_ExchangeColumnName : RMResx.RM_JS_SP_CustomMetadata_SharePointColumnName,
                width: 240,
            },
            {
                header: RMResx.RM_JS_SP_CustomMetadata_NameInSearchColumnName,
                width: 240,
            },
            {
                header: RMResx.RM_JS_SP_CustomMetadata_ActionColumnName,
                width: 100,
            }
        ];
    };

    getManageMetadataList() {
        const manageMetadataListProps = this.props.manageMetadataList;
        let nameInSearchList = [];
        const typeMap = new Map(
            MetadataColumnTypes.map((t) => [t.value, t.name])
        );
        if (manageMetadataListProps && manageMetadataListProps.length) {
            nameInSearchList = manageMetadataListProps.map((item) => ({
                name: `${item.ColumnName} (${
                    typeMap.get(item.ColumnType) || ""
                })`,
                value: item.ColumnType,
                uniqueId: item.UniqueId,
                checked: false,
            }));
        }
        return nameInSearchList;
    }

    getMetadataList() {
        const metadataListProps = this.props.metadataList;
        const columnTypeList = this.getManageMetadataList();
        const nameInSearchMap = new Map(
            columnTypeList.map((o) => [o.value, o])
        );
        if (metadataListProps.length) {
            const list = metadataListProps.map((item) => {
                const selected = nameInSearchMap.get(item.ColumnType);
                return {
                    ...item,
                    SourceColumnName: item.SourceColumnName,
                    TargetColumnName:
                        item.TargetColumnName || selected?.name.split(" (")[0],
                    ColumnType: selected?.value || "",
                    ColumnTypeList: columnTypeList.map((o) => ({
                        ...o,
                        checked: o.value === item.ColumnType && o.uniqueId === item.TargetColumnId,
                    })),
                };
            })
            return list;
        }
        return [];
    }

    onInitValid = () => {
        this.setState({
            isTooLong: false,
            isEmpty: false,
            isDuplicated: false,
        });
    };

    isValid() {
        const metadataList = this.state.metadataList;
        if (
            metadataList.some(
                (item) =>
                    item.SourceColumnName === "" || !item.ColumnType
            )
        ) {
            this.setState({ isEmpty: true });
            return false;
        }
        if (hasDuplicateName(metadataList, "SourceColumnName") ||
            hasDuplicateName(metadataList, "TargetColumnId")) {
            this.setState({ isDuplicated: true });
            return false;
        }
        return true;
    }

    onRowEvent = (args) => {
        this.onInitValid();
        const rowData = args.rowData;
        const rowIndex = args.rowIndex;
        const cloneMetadataList = _.cloneDeep(this.state.metadataList);
        switch (args.type) {
            case "setRowData":
                cloneMetadataList[rowIndex] = rowData;
                if (
                    cloneMetadataList.some(
                        (item) =>
                            item.SourceColumnName === "" || !item.ColumnType
                    )
                ) {
                    this.setState({ isEmpty: true });
                }
                if (hasDuplicateName(cloneMetadataList, "SourceColumnName")) {
                    this.setState({ isDuplicated: true });
                }
                break;
            case "deleteData":
                cloneMetadataList.splice(rowIndex, 1);
                break;
            case "manageNameInSearchList":
                this.dispatch(
                    "customMetadataPanel",
                    "isOpenManageMetadataPanel",
                    true
                );
                break;
            default:
                break;
        }
        this.setState({ metadataList: cloneMetadataList });
        this.props.setMetadataList(cloneMetadataList);
    };

    onAddRowData = () => {
        this.onInitValid();
        const add = _.cloneDeep(metadataDefaultObj);
        add.ColumnTypeList = this.getManageMetadataList();
        const cloneMetadataList = _.cloneDeep(this.state.metadataList);
        if (cloneMetadataList.length >= 10) {
            this.setState({ isTooLong: true });
        } else {
            cloneMetadataList.push(add);
        }
        this.setState({ metadataList: cloneMetadataList });
    };

    render() {
        return (
            <div id={this.props.id}>
                <div className="margin-bottom-m">
                    <R.Table
                        id={this.props.id}
                        columns={this.columns}
                        rowTemplate={this.sourceFlag === SourceFlags.Exo ? CustomMetadataTableExoTemplate : CustomMetadataTableTemplate}
                        items={this.state.metadataList}
                        onRowEvent={this.onRowEvent}
                    />
                </div>
                <R.Button
                    type="bald"
                    icon="fia-plus"
                    text={RMResx.RM_JS_SP_CustomMetadata_AddNewBtn}
                    onClick={this.onAddRowData}
                />
                <div>
                    <$g.ValidationMsg show={this.state.isTooLong}>
                        {RMResx.RM_JS_SP_CustomMetadata_AddNewTooLong}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={!this.state.isTooLong && this.state.isEmpty}>
                        {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={ !this.state.isTooLong && !this.state.isEmpty && this.state.isDuplicated}>
                        {RMResx.RM_JS_SP_CustomMetadata_AddNew_DuplicatedName}
                    </$g.ValidationMsg>
                </div>
            </div>
        );
    }
}

export default CustomMetadataTable;

class CustomMetadataTableExoTemplate extends R.TableRow {
    constructor(props) {
        super(props);
    }

    getExoColumnList() {
        const ExoColumnForCustomMetaDataList = ExoColumnList.filter(item => item.value !== "Flag Completed Date");
        ExoColumnForCustomMetaDataList.push({
            name: RMResx.RM_JS_BCM_Explorer_ExoMoveToSP_ExoCol_HasAttachment,
            value: "Has Attachment",
            checked: false,
        });
        ExoColumnForCustomMetaDataList.push({
            name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel,
            value: "Retention Label",
            checked: false,
        });
        let rowData = this.props.rowData;
        let currentExoColumnList = [];
        RM.deepcopy(ExoColumnForCustomMetaDataList).forEach(item => {
            item.checked = item.value == rowData.SourceColumnName;
            currentExoColumnList.push(item);
        });
        return currentExoColumnList;
    }
    
    onChanged(args0, args1) {
        switch (args0) {
            case 'SourceColumnName':
                this.props.rowData.SourceColumnName = args1.newValue.value;
                break;
            case "ColumnTypeList":
                this.props.rowData.ColumnType = args1.newValue.value;
                this.props.rowData.TargetColumnName =
                    args1.newValue.name.split(" (")[0];
                this.props.rowData.TargetColumnId = args1.newValue.uniqueId;
                this.props.rowData.ColumnTypeList =
                    this.props.rowData.ColumnTypeList.map((item) => ({
                        ...item,
                        checked: item.uniqueId === args1.newValue.uniqueId,
                    }));
                break;
            default:
                break;
        }
        this.dispatch("setRowData");
    }

    removeData() {
        this.dispatch("deleteData");
    }

    onManage() {
        this.dispatch("manageNameInSearchList");
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;
        const exoColumnList = this.getExoColumnList();
        return (
            <Row>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Combobox
                                id="raExoColumnCom"
                                tooltipField="name"
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={exoColumnList}
                                onChange={this.onChanged.bind(this, "SourceColumnName")}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Combobox
                                id="raCrmColumnTypeListCmb"
                                width={"100%"}
                                items={rowData.ColumnTypeList}
                                textField="name"
                                valueField="uniqueId"
                                checkedField="checked"
                                tooltipField="name"
                                createNewText={RMResx.RM_JS_SP_CustomMetadata_ManageBtn}
                                doCreateNew={this.onManage.bind(this)}
                                onChange={this.onChanged.bind(
                                    this,
                                    "ColumnTypeList"
                                )}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <R.Button
                        type="bald"
                        icon="crm-criteria fia-delete"
                        tooltip={RMResx.RM_JS_Common_Delete}
                        onClick={this.removeData.bind(this)}
                    />
                </Cell>
            </Row>
        );
    }
}

class CustomMetadataTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {};
    }

    onChanged(args0, args1) {
        switch (args0) {
            case "SourceColumnName":
                this.props.rowData.SourceColumnName = args1;
                break;
            case "ColumnTypeList":
                this.props.rowData.ColumnType = args1.newValue.value;
                this.props.rowData.TargetColumnName =
                    args1.newValue.name.split(" (")[0];
                this.props.rowData.TargetColumnId = args1.newValue.uniqueId;
                this.props.rowData.ColumnTypeList =
                    this.props.rowData.ColumnTypeList.map((item) => ({
                        ...item,
                        checked: item.uniqueId === args1.newValue.uniqueId,
                    }));
                break;
            default:
                break;
        }
        this.dispatch("setRowData");
    }

    removeData() {
        this.dispatch("deleteData");
    }

    onManage() {
        this.dispatch("manageNameInSearchList");
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Input
                                id="raCrmSourceColumnNameIpt"
                                width={"100%"}
                                type="text"
                                maxlength={255}
                                value={rowData.SourceColumnName}
                                onChange={this.onChanged.bind(
                                    this,
                                    "SourceColumnName"
                                )}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Combobox
                                id="raCrmColumnTypeListCmb"
                                width={"100%"}
                                items={rowData.ColumnTypeList}
                                textField="name"
                                valueField="uniqueId"
                                checkedField="checked"
                                tooltipField="name"
                                createNewText={RMResx.RM_JS_SP_CustomMetadata_ManageBtn}
                                doCreateNew={this.onManage.bind(this)}
                                onChange={this.onChanged.bind(
                                    this,
                                    "ColumnTypeList"
                                )}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <R.Button
                        type="bald"
                        icon="crm-criteria fia-delete"
                        tooltip={RMResx.RM_JS_Common_Delete}
                        onClick={this.removeData.bind(this)}
                    />
                </Cell>
            </Row>
        );
    }
}