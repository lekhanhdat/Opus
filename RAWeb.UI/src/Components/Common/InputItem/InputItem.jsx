import React from "react";

const InputItem = ({
    name,
    value,
    type,
    width,
    height,
    require,
    message,
    isShowMessage,
    onChange,
}) => {
    return (
        <div className="reco-input-item">
            <div className="input-label" >
                {name}
                <span className="input-require" hidden={!require}>*</span>
            </div>
            <R.Input
                name={name}
                type={type}
                width={width || "100%"}
                height={height}
                value={value}
                onChange={onChange}
                aria={{ ariaLabel: name }}
            />
            <div className="input-message" hidden={!isShowMessage} tabIndex="0">
                {message}
            </div>
        </div>
    );
};

export default InputItem;