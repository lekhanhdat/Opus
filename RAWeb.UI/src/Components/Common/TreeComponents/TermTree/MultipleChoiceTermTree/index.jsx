import React, { useRef, useEffect, useState } from "react";
import { MultipleChoiceTree } from "../../ChoiceTree/index";
import { CheckStatus } from "../../Constants/index";
import NodeParser from "./NodeParser";

const getChildrenRequestOption = (node) => ({
    url: "/api/TermUsageReportApi/GetAllChildren",
    data: node
});

const MultipleChoiceTermTree = ({ checkedTreeStructure, onChecked, isReadonly = false}) => {

    const nodeParserRef = useRef(new NodeParser());
    
    const nodesCheckStatusMappingRef = useRef(new Map());

    const [initialData, setInitialData] = useState([]);

    const [refreshKey, setRefreshKey] = useState(Math.random());

    useEffect(() => {
        initNodeCheckStatusMapping();
        initRootNode();
    }, [checkedTreeStructure]);

    const initRootNode = async () => {
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
        const simpleNodes = nodeParserRef.current.convertToSimpleNode([{
            UniqueId: "Root",
            Type: "Root",
            Name: RMResx.RM_JS_TM_RootTerms
        }]);
        processNeedAutoExpandNodes(simpleNodes);
        setInitialData([...simpleNodes]);
        setRefreshKey(Math.random());
    };

    const onLoadChildren = async (key) => {
        const node = nodeParserRef.current.getNodeByKey(key);
        const requestOption = getChildrenRequestOption({
            NodeId: (node.Type === "Root" || node.Type === "TermGroup") ? node.UniqueId : node.Id,
            NodeType: node.Type,
        });
        try {
            const children = await fetchUtility(requestOption);
            const simpleNodes = nodeParserRef.current.convertToSimpleNode(JSON.parse(children));
            processNeedAutoExpandNodes(simpleNodes);
            return simpleNodes;
        } catch (error) {
            return [];
        }
    };

    const onInternalChecked = async (checkedNodesStructure) => {
        if(checkedNodesStructure !== null && checkedNodesStructure !== undefined) {
            let nodeQueue = [checkedNodesStructure];
            while(nodeQueue.length > 0) {
                const node = nodeQueue.shift();
                const realNode = nodeParserRef.current.getNodeByKey(node.key);
                node.type = realNode.Type;
                node.id = realNode.Id;
                nodeQueue = nodeQueue.concat(node.children);
            }
        }
        
        onChecked(checkedNodesStructure);
    };

    const configure = {
        onLoadChildren: onLoadChildren,
        onChecked: onInternalChecked,
        isReadonly: isReadonly,
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

export default MultipleChoiceTermTree;