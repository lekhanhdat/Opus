import React, { useRef, useImperativeHandle, forwardRef } from "react";
import { ActionTypes } from "../../../Constants/index";

const ActionContext = React.createContext();

const ActionStoreProvider = ({ children }, ref) => {

    const internalUpdateNodeInfoActions = useRef(new Map());

    const registerUpdateNodeInfoAction = (key, func) => {
        internalUpdateNodeInfoActions.current.set(key, func);
    };

    const removeUpdateNodeInfoAction = (key) => {
        internalUpdateNodeInfoActions.current.delete(key);
    };

    const invokeMethod = (actionType, ...param) => {
        if (actionType === ActionTypes.UpdateNodeInfo) {
            const func = internalUpdateNodeInfoActions.current.get(param[0]);
            func(param[1], param[2]);
        }
    };

    useImperativeHandle(ref, () => ({
        invokeMethod
    }));

    return (
        <ActionContext.Provider value={{ registerUpdateNodeInfoAction, removeUpdateNodeInfoAction }}>
            {children}
        </ActionContext.Provider>
    );

};

export { ActionContext };

export default forwardRef(ActionStoreProvider);