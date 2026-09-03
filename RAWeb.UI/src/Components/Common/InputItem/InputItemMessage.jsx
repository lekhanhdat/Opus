import React from "react";

const MessageStatus = {
    Succeed: 1,
    Error: 2,
    Exception: 3,
};

const MessageColors = new Map([
    [Error, "#cc0000"]
]);

const InputItemMessage = ({
    status,
    message,
    isShowMessage,
}) => {
    return (
        <div className="reco-input-item-message" style={{color: MessageColors.get(status)}} hidden={!isShowMessage} tabIndex="0">
            {message}
        </div>
    );
};

InputItemMessage.MessageStatus = MessageStatus;

export default InputItemMessage;