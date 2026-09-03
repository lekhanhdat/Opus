import React, { useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";

import { CheckStatus } from "../../../Constants";

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

const StoreProvider = ({ children, configure, initialData, checkedNodesStructure = null }) => {

    InitalPagingConfigure(configure);

    const [updateSignal, setUpdateSignal] = useState(0);

    const nodeStructureRef = useRef();

    const nodeMappingRef = useRef(new Map());

    const pagingNodeInfoRef = useRef(new Map());


    useEffect(() => {

        initNodeStructureRef();

        if (initialData.length === 0) {
            return;
        }
        initRootNodeStatus(initialData[0]);

    }, [initialData, checkedNodesStructure]);

    const initNodeStructureRef = () => {
        if (!checkedNodesStructure) {
            return;
        }
        nodeStructureRef.current = buildCheckedNodesStructre(checkedNodesStructure);
        const tempNode = nodeStructureRef.current;
        let nodeQueue = [tempNode];
        while (nodeQueue.length > 0) {
            const node = nodeQueue.shift();
            nodeMappingRef.current.set(node.key, node);
            nodeQueue = nodeQueue.concat(node.children);
        }

    };

    const getNodeCheckStatus = (key) => {
        const node = nodeMappingRef.current.get(key);
        return node.checkStatus;
    };

    const setNodeCheckStatus = (key) => {
        const node = nodeMappingRef.current.get(key);
        node.checkStatus = node.checkStatus === CheckStatus.Checked ? CheckStatus.Unchecked : CheckStatus.Checked;

        const children = node.children;
        let nodeQueue = [...children];
        while (nodeQueue.length > 0) {
            const childNode = nodeQueue.shift();
            childNode.checkStatus = node.checkStatus;
            nodeQueue = nodeQueue.concat([...childNode.children]);
        }

        if (node.checkStatus === CheckStatus.Checked) {
            let parentKey = node.parentKey;
            while (parentKey !== null) {
                const parentNode = nodeMappingRef.current.get(parentKey);
                const hasNotCheckedChldren = parentNode.children.some(item => item.checkStatus !== CheckStatus.Checked)
                    || (pagingNodeInfoRef.current.has(parentNode.key) && pagingNodeInfoRef.current.get(parentNode.key).size > 0);
                if (hasNotCheckedChldren) {
                    parentNode.checkStatus = CheckStatus.Half;
                }
                else {
                    parentNode.checkStatus = CheckStatus.Checked;
                }

                parentKey = parentNode.parentKey;
            }
        }
        else {
            let parentKey = node.parentKey;
            while (parentKey !== null) {
                const parentNode = nodeMappingRef.current.get(parentKey);
                const hasCheckedChldren = parentNode.children.some(item => item.checkStatus === CheckStatus.Checked || item.checkStatus === CheckStatus.Half);
                if (hasCheckedChldren) {
                    parentNode.checkStatus = CheckStatus.Half;
                }
                else {
                    parentNode.checkStatus = CheckStatus.Unchecked;
                }

                parentKey = parentNode.parentKey;
            }
        }

        configure.onChecked(buildCheckedNodesStructre());
        setUpdateSignal(Math.random());
    };

    const removeChildrenNodesCheckStatus = (key) => {
        pagingNodeInfoRef.current.delete(key);
        const node = nodeMappingRef.current.get(key);
        const nodeCurrentCheckedStatus = node.checkStatus;
        node.checkStatus = CheckStatus.Unchecked;

        let childrenQueue = node.children;
        while (childrenQueue.length > 0) {
            const child = childrenQueue.shift();
            nodeMappingRef.current.delete(child.key);
            childrenQueue = childrenQueue.concat(child.children);
        }

        if (nodeCurrentCheckedStatus !== CheckStatus.Unchecked || node.isRoot) {
            let parentKey = node.parentKey;
            while (parentKey !== null) {
                const parentNode = nodeMappingRef.current.get(parentKey);
                const hasCheckedChildren = parentNode.children.some(item => item.checkStatus === CheckStatus.Checked);
                parentNode.checkStatus = hasCheckedChildren ? CheckStatus.Half : CheckStatus.Unchecked;
                parentKey = parentNode.parentKey;
            }

            configure.onChecked(buildCheckedNodesStructre());
        }

        node.children = [];
        setUpdateSignal(Math.random());
    };

    const initNodesCheckStatus = (parentKey, nodes) => {

        const internalParentNode = nodeMappingRef.current.get(parentKey);

        for (const node of nodes) {

            if (nodeMappingRef.current.has(node.key)) {
                continue;
            }

            const nodeCheckStatus = {
                key: node.key,
                checkStatus: CheckStatus.Unchecked,
                checkable: node.checkable,
                parentKey: parentKey,
                children: []
            };

            if ((internalParentNode.checkStatus === CheckStatus.Checked) && (node.checkable)) {
                nodeCheckStatus.checkStatus = CheckStatus.Checked;
            }

            internalParentNode.children.push(nodeCheckStatus);
            nodeMappingRef.current.set(node.key, nodeCheckStatus);
        }

        setUpdateSignal(Math.random());
    };

    const initPagingNodesCheckStatus = (parentKey, nodes, pageIndex, pageCount) => {
        if (!pagingNodeInfoRef.current.has(parentKey)) {
            const pageIndexArr = Array.from(Array(pageCount), (v, k) => k + 1);
            pagingNodeInfoRef.current.set(parentKey, new Set(pageIndexArr));
        }

        pagingNodeInfoRef.current.get(parentKey).delete(pageIndex);
        initNodesCheckStatus(parentKey, nodes);
    };

    const initRootNodeStatus = (node) => {

        if (nodeMappingRef.current.has(node.key)) {
            return;
        }

        const nodeCheckStatus = {
            key: node.key,
            checkStatus: CheckStatus.Unchecked,
            checkable: node.checkable,
            parentKey: null,
            children: [],
            isRoot: true,
        };

        nodeMappingRef.current.set(node.key, nodeCheckStatus);
        nodeStructureRef.current = nodeCheckStatus;

        let tempNodes = [node];
        while (tempNodes.length !== 0) {
            const tempNode = tempNodes.shift();
            if (tempNode.children === undefined || tempNode.children === null) {
                continue;
            }
            initNodesCheckStatus(tempNode.key, tempNode.children);
            tempNodes = tempNodes.concat(tempNode.children);
        }
    };

    const buildCheckedNodesStructre = (tempStructure = nodeStructureRef.current) => {

        if (tempStructure === null
            || tempStructure === undefined) {
            return null;
        }

        if(!tempStructure.children || !tempStructure.children.some(item => item.checkStatus !== CheckStatus.Unchecked)) {
            return null;
        }

        const checkedNodesStrutre = {
            key: tempStructure.key,
            checkStatus: tempStructure.checkStatus,
            checkable: tempStructure.checkable,
            parentKey: null,
            isRoot: true,
            children: [],
        };
        let nodes = [tempStructure];
        let checkedNodes = [checkedNodesStrutre];
        while (nodes.length > 0) {
            const node = nodes.shift();
            const checkedNode = checkedNodes.shift();

            if (node.checkStatus === CheckStatus.Unchecked && node.checkable) {
                continue;
            }

            for (const child of node.children) {
                if (child.checkStatus === CheckStatus.Unchecked && child.checkable) {
                    continue;
                }

                const tempCheckedNode = {
                    key: child.key,
                    checkStatus: child.checkStatus,
                    checkable: child.checkable,
                    parentKey: checkedNode.key,
                    isRoot: false,
                    children: []
                };

                checkedNode.children.push(tempCheckedNode);

                if (!child.checkable || child.checkStatus === CheckStatus.Half) {
                    checkedNodes.push(tempCheckedNode);
                    nodes.push(child);
                }
            }
        }

        return checkedNodesStrutre;
    };

    return (
        <ConfigureContext.Provider value={configure}>
            <InternalContext.Provider value={{
                updateSignal, checkAction: {
                    getNodeCheckStatus,
                    setNodeCheckStatus,
                    initNodesCheckStatus,
                    initPagingNodesCheckStatus,
                    removeChildrenNodesCheckStatus
                }
            }}>
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
    configure: PropTypes.object,
    initialData: PropTypes.array,
    checkedNodesStructure: PropTypes.object
};

export { InternalContext, ConfigureContext };

export default StoreProvider;