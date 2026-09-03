import React, { useRef, forwardRef, useImperativeHandle } from "react";
import StoreProvider from "./Store/index";
import ActionStoreProvider from "./Store/ActionStore";
import TreeNodeList from "./TreeNodeList/index";

/**
 * 
 * @param {initialData : Array<nodeInfo>}
 *          nodeInfo: {
 *              key: string.require,
 *              title: string.require,
 *              icon: string.require,
 *              enablePagingChildren: bool.default.false,
 *              autoPagingChildren: bool.default.false,
 *              disableLoadChildren: bool.default.false,
 *              hiddenChoice: bool.default.false,
 *              enableBackground: bool.default.false,
 *              children: Array<nodeInfo>.default.undefind
 *          }
 * 
 * @param {configure : Object}
 *          configure: {
 *              onLoadChildren: async func(key: string).require : Array<nodeInfo>.return.require,
 *              onSelected: async func(key: string).require,
 *              pagingConfigure: {
 *                  pageIndex: number.default.1,
 *                  pageSize: number.default.15,
 *                  loadPagingChildren: async func(key: string.loadPagingNodeKey, pageIndex: number, pageSize: number).isRequire : pagingInfo
 *                      pagingInfo: {
 *                          pageCount: number.return.isRequire
 *                          children: Array<nodeInfo>.return.require
 *                      }
 *              }.default.this(If non automatic paging is set for node, the loadPagingChildren method must be configured)
 *          }
 */

const SingleChoiceTree = ({ initialData = [], configure = {} }, ref) => {

    const actionStoreRef = useRef();

    useImperativeHandle(ref, () => actionStoreRef);

    return (
        <div className="reco-single-choice-tree-wrapper" role="tree">
            <StoreProvider configure={configure}>
                <ActionStoreProvider ref={actionStoreRef}>
                    <TreeNodeList data={initialData} />
                </ActionStoreProvider>
            </StoreProvider>
        </div>
    );

};

export default forwardRef(SingleChoiceTree);