import React from "react";

const InputItemContainer = ({
    name,
    require,
    message,
    isShowMessage,
    children
}) => {

    return (
        <div className="reco-input-item">
            <div className="input-label" >
                {name}
                <span className="input-require" hidden={!require}>*</span>
            </div>
            {
                children
            }
            <div className="input-message" hidden={!isShowMessage} tabIndex="0">
                {message}
            </div>
        </div>
    );
};

export default InputItemContainer;