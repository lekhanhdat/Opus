import React, { useState, useContext, useEffect, useRef } from "react";
import PropTypes from "prop-types";

import TreeNodeList from "./index";
import { ConfigureContext, InternalContext } from "../Store/index";
import { ActionContext } from "../Store/ActionStore";
import { LoadStatus } from "../../../Constants/index";
import RadioButton from "./RadioButton";
import NodeIndent from "../../../NodeIndent/index";
import Menu from "../../../Menu/index";

const TreeNode = ({ nodeInfo, distance }) => {

    const nodeRef = useRef();

    const configure = useContext(ConfigureContext);

    const { registerUpdateNodeInfoAction, removeUpdateNodeInfoAction } = useContext(ActionContext);

    const { selectedKey } = useContext(InternalContext);

    const [internalNodeInfo, setInternalNodeInfo] = useState(nodeInfo);

    const [children, setChildren] = useState(
        nodeInfo.children ? nodeInfo.children : []
    );

    const [expand, setExpand] = useState(
        nodeInfo.children !== null && nodeInfo.children !== undefined
    );

    const [loadStatus, setLoadStatus] = useState(
        (!nodeInfo.disableLoadChildren &&
            (nodeInfo.children !== null && nodeInfo.children !== undefined)) ?
            LoadStatus.Loaded :
            LoadStatus.unLoad
    );

    const [isShowPopupMenu, setIsShowPopupMenu] = useState(false);

    useEffect(() => {
        registerUpdateNodeInfoAction(nodeInfo.key, updateNodeInfo);

        return () => {
            removeUpdateNodeInfoAction(nodeInfo.key);
        };
    }, []);

    useEffect(() => {
        setInternalNodeInfo({ ...nodeInfo });
    }, [nodeInfo]);

    const onNodeClick = async () => {
        if (loadStatus === LoadStatus.Loading) {
            return;
        }

        if (loadStatus === LoadStatus.unLoad) {
            setLoadStatus(LoadStatus.Loading);
            const loadedChildren = await configure.onLoadChildren(internalNodeInfo.key);
            setChildren([...loadedChildren]);
            setLoadStatus(LoadStatus.Loaded);
        }

        setExpand(!expand);
    };

    const onRefresh = async () => {
        setExpand(false);
        setChildren([]);
        setLoadStatus(LoadStatus.Loading);
        const loadedChildren = await configure.onLoadChildren(internalNodeInfo.key);
        setChildren([...loadedChildren]);
        setLoadStatus(LoadStatus.Loaded);
        setExpand(true);
    };

    const onNodeActionClick = (e) => {
        e.stopPropagation();
        e.preventDefault();
        setIsShowPopupMenu(true);
    };

    const updateNodeInfo = (node, needRefresh) => {
        setInternalNodeInfo({ ...node });
        if(needRefresh) {
            onRefresh();
        }
    };

    const onNodeKeyUp = (e) => {
        if (e.keyCode !== 13) {

            if (e.keyCode === 38) {
                onNodeKeyArrowUp(e);
            }
            else if (e.keyCode === 40) {
                onNodeKeyArrowDown(e);
            }

            return;
        }

        onNodeClick();
    };

    const onNodeKeyArrowUp = (e) => {
        e.preventDefault();

        const prevNode = $(nodeRef.current).closest(".reco-node-wrapper").prev(".reco-node-wrapper");
        if (prevNode.length !== 0) {
            prevNode.children().first().children(".reco-node").focus();
            return;
        }

        const parentNode = $(nodeRef.current).closest(".reco-node-wrapper").parent().closest(".reco-node-wrapper");
        if (parentNode.length === 0) {
            return;
        }

        parentNode.children().first().children(".reco-node").focus();
    };

    const onNodeKeyArrowDown = (e) => {
        e.preventDefault();

        const nodeWrapper = $(nodeRef.current).closest(".reco-node-wrapper");
        if (expand && children.length > 0) {
            nodeWrapper.children(".reco-node-children-wrapper").children().first()
                .children().first()
                .children().first()
                .children(".reco-node").focus();
            return;
        }

        const nextNode = nodeWrapper.next();
        if (nextNode.length !== 0) {
            nextNode.children().first().children(".reco-node").focus();
            return;
        }

        let parentNode = nodeWrapper.parent().closest(".reco-node-wrapper");
        while (parentNode.length !== 0) {

            const nextNode = parentNode.next(".reco-node-wrapper");
            if (nextNode.length !== 0) {
                nextNode.children().first().children(".reco-node").focus();
                return;
            }

            parentNode = parentNode.parent().closest(".reco-node-wrapper");
        }
    };

    const onNodeActionKeyUp = (e) => {

        if (e.keyCode !== 13) {
            return;
        }

        onNodeActionClick(e);
    };

    const onPopupClose = (needFocus = true) => {
        setIsShowPopupMenu(false);
        if (needFocus) {
            nodeRef.current.focus();
        }
    };

    return (
        <div className="reco-node-wrapper">
            <div style={{ display: "flex" }}>
                <NodeIndent distance={distance} />
                <div
                    className={`reco-node ${selectedKey === internalNodeInfo.key && "reco-node-selected"}`}
                    onClick={onNodeClick}
                    tabIndex="0"
                    role="treeitem"
                    aria-expanded={expand}
                    onKeyUp={onNodeKeyUp}
                    ref={nodeRef}
                    onContextMenu={onNodeActionClick}
                >
                    <RadioButton
                        nodeKey={internalNodeInfo.key}
                        loadStatus={loadStatus}
                        isShow={internalNodeInfo.selectable === undefined || internalNodeInfo.selectable === null ?
                            true : internalNodeInfo.selectable}
                    />
                    <div className={`reco-node-icon ${internalNodeInfo.icon}`} aria-hidden="true">
                        <span className="path1"></span>
                        <span className="path2"></span>
                        <span className="path3"></span>
                    </div>
                    <div className="reco-node-title">
                        {internalNodeInfo.title}
                    </div>
                    <div className="reco-node-action"
                        onClick={onNodeActionClick}
                        onKeyUp={onNodeActionKeyUp}
                    >
                        <div
                            className="reco-node-action-icon-wrapper"
                            tabIndex="0"
                            aria-haspopup="true"
                        >
                            <div
                                className={`reco-node-action-icon fia-triangle-down 
                                ${isShowPopupMenu ? "reco-node-action-icon-show" : "reco-node-action-icon-hidden"}`}
                                aria-hidden="true"
                            ></div>
                        </div>
                    </div>
                    {isShowPopupMenu && <Menu onPopupClose={onPopupClose} onRefresh={onRefresh} />}
                    <div className="reco-node-selected-arrow-placeholder"></div>
                    {
                        selectedKey === nodeInfo.key &&
                        <div className="reco-node-selected-arrow fia-long-arrow"></div>
                    }
                </div>
            </div>
            <div className="reco-node-children-wrapper" style={{ display: expand ? "" : "none" }} role="group">
                <TreeNodeList data={children} distance={distance + 1} />
            </div>
        </div>
    );
};

TreeNode.propTypes = {
    nodeInfo: PropTypes.object,
    distance: PropTypes.number
};

export default TreeNode;