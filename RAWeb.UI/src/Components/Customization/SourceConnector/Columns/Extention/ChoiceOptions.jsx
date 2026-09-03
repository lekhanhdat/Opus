import React, { useState, forwardRef, useImperativeHandle } from "react";
import _ from "lodash";
import { InputItemContainer } from "../../../../Common/InputItem";
import StringUtil from "../../../../../Utilities/StringUtil";
import { GetIsExistDuplicate, GetDuplicateValues } from "../../../../../Utilities/CommonUtil";


const ChoiceOption = ({ option, optionsCount, onEdit, onDelete, onAdd }) => {

    const onInternalChange = (value) => {
        const clonedOption = _.cloneDeep(option);
        clonedOption.name = value;
        onEdit(clonedOption);
    };

    const onInternalAdd = () => {
        onAdd({
            name: "",
            value: option.value + 1,
            order: option.order + 1
        });
    };

    return (
        <div className="choice-option">
            <div>
                <R.Input
                    name={StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnOptions)}
                    type={"text"}
                    width={"100%"}
                    height={34}
                    value={_.isNil(option.name) ? "" : option.name}
                    onChange={onInternalChange}
                    aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnOptions) }}
                />
            </div>
            <div className="choice-option-actions">
                {
                    optionsCount > 1 &&
                    <R.Button
                        type="bald"
                        icon="fia-delete"
                        onClick={e => onDelete(option)}
                        tooltip={RMResx.RM_JS_Common_Delete}
                    />
                }
                {
                    option.order === optionsCount &&
                    <R.Button
                        type="bald"
                        icon="fia-plus"
                        onClick={onInternalAdd}
                        tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Button_Add}
                    />
                }
            </div>
        </div>
    );
};

const ChoiceOptions = forwardRef(({ definitionOptions = [], onChange }, ref) => {

    const [validated, setValidated] = useState(false);

    const [options, setOptions] = useState(definitionOptions.length === 0 ? [{
        name: "",
        value: 1,
        order: 1
    }] : definitionOptions);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            setValidated(true);
            const index = options.findIndex(item => _.isEmpty(item.name));
            const isExistDuplicate = GetIsExistDuplicate(options.map(item => item.name));
            return index === -1 && !isExistDuplicate;
        }
    }));

    const onEdit = (option) => {
        const clonedOptions = _.cloneDeep(options);
        const index = clonedOptions.findIndex(item => item.value == option.value);
        clonedOptions[index] = option;
        setOptions(clonedOptions);
        onChange(clonedOptions);
    };

    const onAdd = (option) => {
        const clonedOptions = _.cloneDeep(options);
        clonedOptions.push(option);
        setOptions(clonedOptions);
        onChange(clonedOptions);
    };

    const onDelete = (option) => {
        const filteredOptions = options.filter(item => item.value !== option.value);
        const newlyOptions = filteredOptions.map((v, i) => {
            v.value = i + 1;
            v.order = i + 1;
            return v;
        });
        setOptions(newlyOptions);
        onChange(newlyOptions);
    };

    const getValueValidate = (options) => {
        let isExsitEmpty = options.findIndex(item => _.isEmpty(item.name)) > -1;
        let duplicateValues = GetDuplicateValues(options.map(item => item.name));
        if(isExsitEmpty){
            return RMResx.RM_Template_Column_ValueValidate;
        }
        if(duplicateValues?.length > 0){
            let duplicateValuesStr = duplicateValues.toString();
            return RMResx.RM_EditTemplate_Valid_ChoiceDuplicate.format(duplicateValuesStr);
        }
    };

    return (
        <div className="reco-connector-column-definitoin-choice">
            <InputItemContainer
                name={StringUtil.trimEndColon(RMResx.RM_EditTemplate_ColumnOptions)}
                require={true}
                message={getValueValidate(options)}
                isShowMessage={validated}
            >
                {
                    options.map(item =>
                        <ChoiceOption
                            key={item.value}
                            option={item}
                            optionsCount={options.length}
                            onEdit={onEdit}
                            onAdd={onAdd}
                            onDelete={onDelete}
                        />
                    )
                }
            </InputItemContainer>
        </div>
    );
});

ChoiceOptions.displayName = "ChoiceOptions";

export default ChoiceOptions;