import React, { useEffect, useState } from "react";
import _ from "lodash";

const InputItemCombobox = ({
    name,
    options,
    selectedOptionValue,
    height,
    require,
    message,
    isShowMessage,
    onChange,
    disabled
}) => {

    const [internalOptions, setInternalOptions] = useState([]);

    useEffect(() => {
        const clonedOptions = _.cloneDeep(options);
        clonedOptions.forEach(item => {
            item.checked = false;
            if(item.value === selectedOptionValue) {
                item.checked = true;
            }
        });
        setInternalOptions(clonedOptions);
    }, [options, selectedOptionValue]);

    return (
        <div className="reco-input-item">
            <div className="input-label" >
                {name}
                <span className="input-require" hidden={!require}>*</span>
            </div>
            <R.Combobox
                disabled={disabled}
                searchable={false}
                width="100%"
                height={height}
                textField="name"
                items={internalOptions}
                onChange={args => onChange(args.newValue.value)}
                aria={{ ariaLabel: name }}
            />
            <div className="input-message" hidden={!isShowMessage} tabIndex="0">
                {message}
            </div>
        </div>
    );
};

export default InputItemCombobox;