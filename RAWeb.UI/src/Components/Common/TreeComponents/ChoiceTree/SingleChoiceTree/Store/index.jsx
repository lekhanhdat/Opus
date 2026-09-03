import React, { useRef, useState } from "react";
import PropTypes from "prop-types";

const InternalContext = React.createContext();
const ConfigureContext = React.createContext();


const InitalPagingConfigure = (configure) => {
    if (!configure.pagingConfigure) {
        configure.pagingConfigure = {
            pageIndex: 1,
            pageSize: 15,
        };
    }

    if (!configure.pagingConfigure.pageIndex) {
        configure.pagingConfigure.pageIndex = 1;
    }

    if (!configure.pagingConfigure.pageSize) {
        configure.pagingConfigure.pageSize = 15;
    }
};

const StoreProvider = ({ children, configure }) => {

    const funcs = useRef(new Map());

    const [selectedKey, setSelectedKey] = useState("");

    const keepKeyRef = useRef();

    const changeSelectedKey = (key) => {
        keepKeyRef.current = key;
        setSelectedKey(key);
    };

    const getSelectedKeyForUnmount = () => {
        return keepKeyRef.current;
    };

    const registerFunction = (key, func) => {
        funcs.current.set(key, func);
    };

    const invokeFunction = (key) => {
        const func = funcs.current.get(key);
        func();
    };

    InitalPagingConfigure(configure);

    return (
        <ConfigureContext.Provider value={configure}>
            <InternalContext.Provider value={{ selectedKey, changeSelectedKey, registerFunction, invokeFunction, getSelectedKeyForUnmount }}>
                {children}
            </InternalContext.Provider>
        </ConfigureContext.Provider>
    );

};

StoreProvider.propTypes = {
    children: PropTypes.oneOfType([
        PropTypes.element,
        PropTypes.arrayOf(PropTypes.element),
    ]),
    configure: PropTypes.object
};

export { InternalContext, ConfigureContext };

export default StoreProvider;