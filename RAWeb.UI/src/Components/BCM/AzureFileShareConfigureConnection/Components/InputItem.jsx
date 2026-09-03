import React from "react";

const InputItem = ({
    name,
    value,
    type,
    height,
    require,
    message,
    isShowMessage,
    onChange,
    placeholder }) => {

    return (
        <div className="reco-az-input-item">
            <div className="reco-az-input-label" >
                {name}
                <span className="reco-az-input-require" hidden={!require}>*</span>
            </div>
            <R.Input
                name={name}
                type={type}
                width={"100%"}
                height={height}
                value={value}
                onChange={onChange}
                aria={{ ariaLabel: name }}
                placeholder={placeholder}
            />
            <div className="reco-az-input-message" hidden={!isShowMessage} tabIndex="0">
                {message}
            </div>
        </div>
    );

};

export default InputItem;