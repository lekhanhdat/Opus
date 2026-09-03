import React, { forwardRef, useImperativeHandle, useRef, useState } from "react";
import _ from "lodash";
import { ColumnType, CustomizeConnectorOrigin, CustomizeConnectorScope } from "../Common/Constants";
import { useStableCallback } from "../../../Common/Hooks/index";
import { InputItem, InputItemCombobox } from "../../../Common/InputItem/index";
import { ColumnTypeI18ns } from "../Common/I18ns";
import Extention from "./Extention";
import Utility from "../Common/Utility";
import StringUtil from '../../../../Utilities/StringUtil';

const ColumnTypeOptions = [
    { name: ColumnTypeI18ns.get(ColumnType.SingleText), value: ColumnType.SingleText },
    { name: ColumnTypeI18ns.get(ColumnType.MultipleText), value: ColumnType.MultipleText },
    { name: ColumnTypeI18ns.get(ColumnType.DateTime), value: ColumnType.DateTime },
    { name: ColumnTypeI18ns.get(ColumnType.SingleChoice), value: ColumnType.SingleChoice },
    { name: ColumnTypeI18ns.get(ColumnType.MultipleChoice), value: ColumnType.MultipleChoice },
    { name: ColumnTypeI18ns.get(ColumnType.PeopleOrGroup), value: ColumnType.PeopleOrGroup },
    { name: ColumnTypeI18ns.get(ColumnType.Number), value: ColumnType.Number },
];

export default forwardRef(({ onChange }, ref) => {

    const extentionRef = useRef();

    const [columnInfo, setColumnInfo] = useState({});

    const [isShow, setIsShow] = useState(false);

    const [validated, setValidated] = useState(false);

    const [isEdit, setIsEdit] = useState(false);

    const [hasRepeatName, setHasRepeatName] = useState(false);

    useImperativeHandle(ref, () => ({
        onShow: (columnInfo) => {
            let clonedColumnInfo = null;
            if (_.isNil(columnInfo)) {
                setIsEdit(false);
                clonedColumnInfo = {
                    name: "",
                    type: ColumnType.SingleText,
                    isRequire: false,
                    extention: "",
                    scope: CustomizeConnectorScope.Template,
                    origin: CustomizeConnectorOrigin.ExternalCustomize,
                };
            }
            else {
                setIsEdit(true);
                clonedColumnInfo = _.cloneDeep(columnInfo);
            }
            setColumnInfo(clonedColumnInfo);
            setIsShow(true);
            setValidated(false);
            setHasRepeatName(false);
        }
    }));

    const onSave = useStableCallback(() => {
        setValidated(true);
        const extentionValidate = extentionRef.current.onValidate();
        if (_.isEmpty(columnInfo.name) || !extentionValidate) {
            return false;
        }

        const clonedColumnInfo = _.cloneDeep(columnInfo);
        if (_.isEmpty(clonedColumnInfo.id)) {
            clonedColumnInfo.internalName = Utility.UnicodeEncode(clonedColumnInfo.name);
        }
        if (!onChange(clonedColumnInfo)) {
            setHasRepeatName(true);
            return false;
        }
        setIsShow(false);
        return true;
    });

    const onValueChange = (name, value) => {
        const clonedColumnInfo = _.cloneDeep(columnInfo);
        clonedColumnInfo[name] = value;
        if(name === "type") {
            clonedColumnInfo["extention"] = null;
        }
        setColumnInfo(clonedColumnInfo);
    };

    return (
        <R.Panel
            id="reco-manual-review-filter-panel"
            header={isEdit ? RMResx.RM_EditTemplate_EditColumnText: RMResx.RM_EditTemplate_NewColumnText}
            size={660}
            status={{ show: isShow }}
            onHide={() => setIsShow(false)}
            destroy={true}
        >
            <div>
                <InputItem
                    name={RMResx.RM_EditTemplate_ColumnName}
                    value={columnInfo.name}
                    type="text"
                    height={34}
                    require={true}
                    message={hasRepeatName ? RMResx.RM_EditTemplate_SameColumnNameErrorMessage : RMResx.RM_FS_Register_NameInputValidateMessage}
                    isShowMessage={(validated && _.isEmpty(columnInfo.name)) || hasRepeatName}
                    onChange={value => {onValueChange("name", value); setHasRepeatName(false);}}
                />
                <InputItemCombobox
                    name={StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnType)}
                    options={ColumnTypeOptions}
                    selectedOptionValue={columnInfo.type}
                    height={34}
                    require={true}
                    onChange={value => onValueChange("type", value)}
                    disabled={isEdit}
                />
                <Extention
                    ref={extentionRef}
                    columnType={columnInfo.type}
                    extentionDefinition={columnInfo.extention}
                    onChange={value => onValueChange("extention", value)}
                />
            </div>
            <>
                <R.Button
                    slot="buttons"
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={() => setIsShow(false)}
                />
                <R.Button
                    slot="buttons"
                    primary
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={onSave}
                />
            </>
        </R.Panel>
    );
});

