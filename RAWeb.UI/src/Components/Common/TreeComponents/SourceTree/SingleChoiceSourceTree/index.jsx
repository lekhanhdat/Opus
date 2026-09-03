import React, { useEffect, useState, useRef, useImperativeHandle, forwardRef } from "react";

import { SingleChoiceTree } from "../../ChoiceTree/index";
import NodeParser from "./NodeParser";
import { ActionTypes, NodeLevel } from "../../Constants";
import _ from "lodash";

const SingleChoiceSourceTree = ({ sourceFlag, onSelected }, ref) => {

    const nodeParserRef = useRef(new NodeParser(sourceFlag));

    const actionRef = useRef();

    const [initialData, setInitialData] = useState([]);

    useImperativeHandle(ref, () => ({
        onUpdateNodeInfo
    }));

    useEffect(() => {
        onLoadRootNode();
    }, []);

    const onLoadRootNode = async () => {
        const requestOption = nodeParserRef.current.getRootNodeRequstOption();
        const rootNode = await fetchUtility(requestOption);
        const simpleNodes = nodeParserRef.current.convertToSimpleNode([rootNode]);
        setInitialData([...simpleNodes]);
    };

    const onLoadChildren = async (key) => {
        const node = nodeParserRef.current.getNodeByKey(key);
        const requestOption = nodeParserRef.current.getChildrenRequestOption(node);
        try {
            const children = await fetchUtility(requestOption);
            const simpleNodes = nodeParserRef.current.convertToSimpleNode(children);
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

    const onInternalSelected = async (key) => {
        const node = nodeParserRef.current.getNodeByKey(key);
        if (node !== null && node !== undefined) {
            node.key = key;
        }

        if(!_.isNil(onSelected)) {
            const clonedNode = _.cloneDeep(node);
            await onSelected(clonedNode);
        }
    };

    const onUpdateNodeInfo = (nodeInfo) => {
        const beforeNode = nodeParserRef.current.getNodeByKey(nodeInfo.key);
        const needRefresh = beforeNode.level === NodeLevel.AzureFileShareGroup && beforeNode.iconStatus !== nodeInfo.iconStatus;
        const key = nodeInfo.key;
        const simpleNode = nodeParserRef.current.convertToSimpleNode([nodeInfo])[0];
        simpleNode.key = key;
        actionRef.current.current.invokeMethod(ActionTypes.UpdateNodeInfo, key, simpleNode, needRefresh);
    };

    const configure = {
        onLoadChildren: onLoadChildren,
        onSelected: onInternalSelected,
        pagingConfigure: {
            pageIndex: 1,
            pageSize: 15,
            onLoadPagingChildren: onLoadPagingChildren
        }
    };

    return (
        <SingleChoiceTree initialData={initialData} configure={configure} ref={actionRef} />
    );
};

export default forwardRef(SingleChoiceSourceTree);