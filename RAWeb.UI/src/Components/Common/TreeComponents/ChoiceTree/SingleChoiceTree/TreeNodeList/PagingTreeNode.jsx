import React, { useState, useContext, useRef, useEffect } from "react";
import PropTypes from "prop-types";

import NodeIndent from "../../../NodeIndent/index";
import RadioButton from "./RadioButton";
import TreeNodeList from "./index";
import { ConfigureContext, InternalContext } from "../Store/index";
import { ActionContext } from "../Store/ActionStore";
import { LoadStatus } from "../../../Constants/index";
import Pager from "../../../Pager/index";
import Menu from "../../../Menu/index";

const PagingTreeNode = ({ nodeInfo, distance }) => {

    const configure = useContext(ConfigureContext);

    const { selectedKey } = useContext(InternalContext);

    const { registerUpdateNodeInfoAction, removeUpdateNodeInfoAction } = useContext(ActionContext);

    const nodeRef = useRef();

    const childrenCache = useRef(new Map());

    const [internalNodeInfo, setInternalNodeInfo] = useState(nodeInfo);

    const [pageIndex, setPageIndex] = useState(0);

    const [pageCount, setPageCount] = useState(1);

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

    const onNodeActionClick = (e) => {
        e.stopPropagation();
        e.preventDefault();
        setIsShowPopupMenu(true);
    };

    const processAutoPagingChldren = async (pageIndex) => {

        if (childrenCache.current.has(pageIndex)) {
            setPageIndex(pageIndex);
            return;
        }

        const loadedChildren = await configure.onLoadChildren(internalNodeInfo.key);
        const childCount = loadedChildren.length;

        if (childCount === 0) {
            return;
        }

        const pageSize = configure.pagingConfigure.pageSize;
        setPageCount(Number.parseInt((childCount + pageSize - 1) / pageSize));

        for (let i = 0; i < childCount; i += pageSize) {
            const children = loadedChildren.slice(i, i + pageSize);
            const currentPageIndex = Number.parseInt(i / pageSize + 1);
            childrenCache.current.set(currentPageIndex, children);
        }

        setPageIndex(pageIndex);
    };

    const processPagingChildren = async (pageIndex) => {

        if (childrenCache.current.has(pageIndex)) {
            setPageIndex(pageIndex);
            return;
        }

        const pagingConfigure = configure.pagingConfigure;
        const pagingInfo = await pagingConfigure.onLoadPagingChildren(internalNodeInfo.key, pageIndex, pagingConfigure.pageSize);
        setPageCount(pagingInfo.pageCount);
        if(pagingInfo.children.length > 0) {
            childrenCache.current.set(pageIndex, pagingInfo.children);
        }
        setPageIndex(pageIndex);
    };

    const onNodeClick = async () => {

        if (loadStatus === LoadStatus.Loading) {
            return;
        }

        if (loadStatus === LoadStatus.unLoad) {
            setLoadStatus(LoadStatus.Loading);
            if (nodeInfo.autoPagingChildren) {
                await processAutoPagingChldren(configure.pagingConfigure.pageIndex);
            }
            else {
                await processPagingChildren(configure.pagingConfigure.pageIndex);
            }
            setLoadStatus(LoadStatus.Loaded);
        }

        setExpand(!expand);
    };

    const onPageIndexChange = async (index) => {

        nodeRef.current.focus();

        setLoadStatus(LoadStatus.Loading);

        if (nodeInfo.autoPagingChildren) {
            await processAutoPagingChldren(index);
        }
        else {
            await processPagingChildren(index);
        }
        setLoadStatus(LoadStatus.Loaded);
    };

    const onRefresh = async () => {
        setExpand(false);
        setPageIndex(0);
        setPageCount(0);
        childrenCache.current.clear();
        setLoadStatus(LoadStatus.Loading);
        if (nodeInfo.autoPagingChildren) {
            await processAutoPagingChldren(configure.pagingConfigure.pageIndex);
        }
        else {
            await processPagingChildren(configure.pagingConfigure.pageIndex);
        }
        setLoadStatus(LoadStatus.Loaded);
        setExpand(true);
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
        if (expand && childrenCache.current.size > 0) {
            nodeWrapper.children(".reco-node-children-wrapper").children(":not(:hidden)").first()
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
                    className={`reco-node ${selectedKey == internalNodeInfo.key && "reco-node-selected"}`}
                    tabIndex="0"
                    onClick={onNodeClick}
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
                        selectedKey === internalNodeInfo.key &&
                        <div className="reco-node-selected-arrow fia-long-arrow"></div>
                    }
                </div>
            </div>
            <div className="reco-node-children-wrapper" style={{ display: expand ? "" : "none" }} role="group">
                {
                    pageIndex > 0 &&
                    Array.from(childrenCache.current).map(item => {
                        return <TreeNodeList data={item[1]} distance={distance + 1} key={item[0]} isHidden={pageIndex !== item[0]} />;
                    })
                }
                {
                    pageCount > 1 &&
                    <Pager
                        distance={distance + 1}
                        pageIndex={configure.pagingConfigure.pageIndex}
                        pageCount={pageCount}
                        onPageIndexChange={onPageIndexChange}
                    />
                }
            </div>
        </div>
    );
};

PagingTreeNode.propTypes = {
    nodeInfo: PropTypes.object,
    distance: PropTypes.number
};

export default PagingTreeNode;