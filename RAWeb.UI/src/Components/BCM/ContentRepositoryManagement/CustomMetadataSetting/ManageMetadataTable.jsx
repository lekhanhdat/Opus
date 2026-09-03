import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import _ from "lodash";

import { MetadataColumnTypes } from "./Constants";
import { hasDuplicateName } from "../../../../Utilities/CommonUtil";

const buildInColumns = [
    {
        header: RMResx.RM_JS_SP_ManageMetadata_ColumnName,
        width: 220,
    },
    {
        header: RMResx.RM_JS_SP_ManageMetadata_ColumnType,
        width: 180,
    },
    {
        header: RMResx.RM_JS_SP_ManageMetadata_SortColumn,
        width: 80,
    },
    {
        header: RMResx.RM_JS_SP_CustomMetadata_ActionColumnName,
        width: 100,
    },
];

const metadataDefaultObj = {
    ColumnName: "",
    ColumnType: "",
    EnableSort: false,
    CanAction: true,
};

const initialValidState = {
    isTooLong: false,
    isEmpty: false,
    isDuplicated: false,
};

function ManageMetadataTable(props, ref) {
    const { metadataListProps, manageMetadataListProps, idColumnSelectedList } = props;

    const [manageMetadataList, setManageMetadataList] = useState([]);
    const [isValid, setIsValid] = useState(initialValidState);

    useImperativeHandle(ref, () => ({
        getManageMetadataList: () => manageMetadataList,
        isValid: () => {
            if (
                manageMetadataList.some(
                    (item) => item.ColumnName === "" || !item.ColumnType
                )
            ) {
                setIsValid((prev) => ({ ...prev, isEmpty: true }));
                return false;
            }
            if (hasDuplicateName(manageMetadataList, "ColumnName")) {
                setIsValid((prev) => ({ ...prev, isDuplicated: true }));
                return false;
            }
            return true;
        },
    }));

    useEffect(() => {
        if (manageMetadataListProps && manageMetadataListProps.length) {
            let clonedManageMetadataListProps = _.cloneDeep(
                manageMetadataListProps
            );
            // const disabledTypes = new Map(
            //     metadataListProps.map((o) => [`${o.ColumnType}_${o.TargetColumnId}`, true])
            // );
            clonedManageMetadataListProps = clonedManageMetadataListProps.map(
                (item) => {
                    return {
                        ...item,
                        ColumnName: item.ColumnName,
                        ColumnType: item.ColumnType,
                        CanAction: !idColumnSelectedList.includes(item.UniqueId),
                    };
                }
            );            
            setManageMetadataList(clonedManageMetadataListProps);
        } else {
            setManageMetadataList([_.cloneDeep(metadataDefaultObj)]);
        }
    }, [metadataListProps, manageMetadataListProps]);

    const onInitValid = () => {
        setIsValid(initialValidState);
    };

    const onRowEvent = (args) => {
        onInitValid();
        const rowData = args.rowData;
        if (!rowData.hasOwnProperty("CanAction")) {
            rowData.CanAction = true;
        }
        const rowIndex = args.rowIndex;
        const cloneManageMetadataList = _.cloneDeep(manageMetadataList);
        switch (args.type) {
            case "setRowData":
                cloneManageMetadataList[rowIndex] = rowData;
                if (
                    cloneManageMetadataList.some(
                        (item) => item.ColumnName === "" || !item.ColumnType
                    )
                ) {
                    setIsValid((prev) => ({ ...prev, isEmpty: true }));
                }
                if (hasDuplicateName(cloneManageMetadataList, "ColumnName")) {
                    setIsValid((prev) => ({ ...prev, isDuplicated: true }));
                }
                break;
            case "deleteData":
                cloneManageMetadataList.splice(rowIndex, 1);
                break;
            default:
                break;
        }
        setManageMetadataList(cloneManageMetadataList);
    };

    const onAddRowData = () => {
        onInitValid();
        const add = _.cloneDeep(metadataDefaultObj);
        const cloneManageMetadataList = _.cloneDeep(manageMetadataList);
        if (cloneManageMetadataList.length >= 10) {
            setIsValid((prev) => ({ ...prev, isTooLong: true }));
        } else {
            cloneManageMetadataList.push(add);
        }
        setManageMetadataList(cloneManageMetadataList);
    };

    return (
        <div>
            <div className="flex align-center margin-bottom-s">
                <p tabIndex={0} style={{ margin: 0 }}>
                    {RMResx.RM_JS_SP_ManageMetadata_Label}
                </p>
                <$g.Popover>
                    {RMResx.RM_JS_SP_ManageMetadata_LabelTips}
                </$g.Popover>
            </div>
            <div className="margin-bottom-m">
                <R.Table
                    id="raCrmManageMetadataTable"
                    columns={buildInColumns}
                    rowTemplate={ManageMetadataTableTemplate}
                    items={manageMetadataList}
                    onRowEvent={onRowEvent}
                />
            </div>
            <R.Button
                type="bald"
                icon="fia-plus"
                text={RMResx.RM_JS_SP_ManageMetadata_AddNewBtn}
                onClick={onAddRowData}
            />
            <div>
                <$g.ValidationMsg show={isValid.isTooLong}>
                    {RMResx.RM_JS_SP_ManageMetadata_AddNewTooLong}
                </$g.ValidationMsg>
                <$g.ValidationMsg show={!isValid.isTooLong && isValid.isEmpty}>
                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                </$g.ValidationMsg>
                <$g.ValidationMsg show={!isValid.isTooLong && !isValid.isEmpty && isValid.isDuplicated}>
                    {RMResx.RM_JS_SP_ManageMetadata_AddNew_DuplicatedName}
                </$g.ValidationMsg>
            </div>
        </div>
    );
}

export default forwardRef(ManageMetadataTable);

class ManageMetadataTableTemplate extends R.TableRow {
    constructor(props) {
        super(props);
        this.state = {
            columnTypeList: this.getColumnTypeList(),
        };
    }

    getColumnTypeList() {
        const { ColumnType } = this.props.rowData;
        const currentColumnTypeList = [];
        RM.deepcopy(MetadataColumnTypes).forEach((item) => {
            item.checked = item.value === ColumnType;
            currentColumnTypeList.push(item);
        });
        return currentColumnTypeList;
    }

    onChanged(args0, args1) {
        switch (args0) {
            case "ColumnName":
                this.props.rowData.ColumnName = args1;
                break;
            case "ColumnType":
                const value = args1.newValue.value;
                this.props.rowData.ColumnType = value;
                break;
            case "EnableSort":
                this.props.rowData.EnableSort = args1;
                break;
            default:
                break;
        }
        this.dispatch("setRowData");
    }

    removeData() {
        this.dispatch("deleteData");
    }

    render(Row, Cell) {
        const rowData = this.props.rowData;
        return (
            <Row>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Input
                                id="raCrmColumnNameIpt"
                                width={"100%"}
                                type="text"
                                maxlength={255}
                                value={rowData.ColumnName}
                                disabled={!rowData.CanAction}
                                tooltip={
                                    !rowData.CanAction
                                        ? RMResx.RM_JS_SP_ManageMetadata_CannotAction
                                        : "name"
                                }
                                onChange={this.onChanged.bind(
                                    this,
                                    "ColumnName"
                                )}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <div className="flex ra-flex-align-top">
                        <div className="ra-move-cell">
                            <R.Combobox
                                id="raCrmColumnTypeCmb"
                                width={"100%"}
                                items={this.state.columnTypeList}
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                disabled={!rowData.CanAction}
                                tooltipField="name"
                                tooltip={
                                    !rowData.CanAction
                                        ? RMResx.RM_JS_SP_ManageMetadata_CannotAction
                                        : ""
                                }
                                searchable={false}
                                onChange={this.onChanged.bind(
                                    this,
                                    "ColumnType"
                                )}
                            />
                        </div>
                    </div>
                </Cell>
                <Cell>
                    <R.Switch
                        id="raCrmEnableSortSwitch"
                        checked={rowData.EnableSort}
                        onChange={this.onChanged.bind(
                            this,
                            "EnableSort"
                        )}
                    />
                </Cell>
                <Cell>
                    {rowData.CanAction && (
                        <R.Button
                            type="bald"
                            icon="crm-criteria fia-delete"
                            tooltip={RMResx.RM_JS_Common_Delete}
                            onClick={this.removeData.bind(this)}
                        />
                    )}
                </Cell>
            </Row>
        );
    }
}