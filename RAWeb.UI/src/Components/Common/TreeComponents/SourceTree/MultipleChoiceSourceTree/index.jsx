import React, { useRef, useEffect, useState } from "react";
import { SourceFlag } from "../../../Constants";
import { MultipleChoiceTree } from "../../ChoiceTree/index";
import { CheckStatus } from "../../Constants";
import NodeParser from "./NodeParser";

const MultipleChoiceSourceTree = ({ sourceFlag, checkedTreeStructure, onChecked, isReadonly = false}) => {

    const nodeParserRef = useRef(new NodeParser(sourceFlag));

    const nodesCheckStatusMappingRef = useRef(new Map());

    const [refreshKey, setRefreshKey] = useState(Math.random());

    const [initialData, setInitialData] = useState([]);

    useEffect(() => {
        initNodeCheckStatusMapping();
        initRootNode();
    }, [sourceFlag, checkedTreeStructure]);

    const initRootNode = async () => {
        if (sourceFlag === SourceFlag.None || sourceFlag === SourceFlag.All) {
            return;
        }

        await onLoadRootNode();
    };

    const initNodeCheckStatusMapping = () => {
        if (checkedTreeStructure === null || checkedTreeStructure === undefined) {
            return;
        }

        let nodesQueue = [checkedTreeStructure];
        while (nodesQueue.length > 0) {
            const node = nodesQueue.shift();
            nodesCheckStatusMappingRef.current.set(node.key, node.checkStatus);
            nodesQueue = nodesQueue.concat(node.children);
        }
    };

    const processNeedAutoExpandNodes = (simpleNodes) => {
        for (const simpleNode of simpleNodes) {
            if (!nodesCheckStatusMappingRef.current.has(simpleNode.key)) {
                continue;
            }
            const checkStatus = nodesCheckStatusMappingRef.current.get(simpleNode.key);
            if (checkStatus === CheckStatus.Half || (checkStatus === CheckStatus.Checked && !simpleNode.checkable)) {
                simpleNode.autoExpand = true;
            }
        }
    };

    const onLoadRootNode = async () => {
        const requestOption = nodeParserRef.current.getRootNodeRequstOption();
        const rootNode = await fetchUtility(requestOption);
        const simpleNodes = nodeParserRef.current.convertToSimpleNode([rootNode]);
        processNeedAutoExpandNodes(simpleNodes);
        setInitialData([...simpleNodes]);
        setRefreshKey(Math.random());
    };

    const onLoadChildren = async (key) => {
        const node = nodeParserRef.current.getNodeByKey(key);
        const requestOption = nodeParserRef.current.getChildrenRequestOption(node);
        try {
            const children = await fetchUtility(requestOption);
            const simpleNodes = nodeParserRef.current.convertToSimpleNode(children);
            processNeedAutoExpandNodes(simpleNodes);
            return simpleNodes;
        } catch (error) {
            return [];
        }
    };

    const onLoadPagingChildren = async (key, pageIndex, pageSize) => {
        const node = nodeParserRef.current.getNodeByKey(key);
        const pagingNode = {
            pageIndex: pageIndex,
            pageSize: pageSize,
            node: node
        };
        const requestOption = nodeParserRef.current.getPagingChildrenRequestOption(pagingNode);
        try {
            const pagingChildren = await fetchUtility(requestOption);
            const children = pagingChildren.children;
            const simpleNodes = nodeParserRef.current.convertToSimpleNode(children);
            processNeedAutoExpandNodes(simpleNodes);
            return {
                pageCount: pagingChildren.pageCount,
                children: simpleNodes
            };
        } catch (error) {
            return {
                pageCount: 0,
                children: []
            };
        }

    };

    const onInternalChecked = async (checkedNodesStructure) => {
        if(checkedNodesStructure !== null && checkedNodesStructure !== undefined) {
            let nodeQueue = [checkedNodesStructure];
            while(nodeQueue.length > 0) {
                const checkedNode = nodeQueue.shift();
                nodeParserRef.current.internalAssignCheckedNodeValue(checkedNode);
                nodeQueue = nodeQueue.concat(checkedNode.children);
            }
        }
        onChecked(checkedNodesStructure);
    };

    const configure = {
        onLoadChildren: onLoadChildren,
        onChecked: onInternalChecked,
        isReadonly: isReadonly,
        pagingConfigure: {
            pageIndex: 1,
            pageSize: 3,
            onLoadPagingChildren: onLoadPagingChildren
        }
    };

    return (
        <MultipleChoiceTree
            key={refreshKey}
            initialData={initialData}
            configure={configure}
            checkedNodesStructure={checkedTreeStructure}
        />
    );

};

export default MultipleChoiceSourceTree;