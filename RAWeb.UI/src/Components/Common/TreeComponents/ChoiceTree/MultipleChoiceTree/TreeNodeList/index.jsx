import React from "react";
import TreeNode from "./TreeNode";
import PagingTreeNode from "./PagingTreeNode";

const TreeNodeList = ({ data = [], distance = 0, isHidden = false }) => {
    return (
        <div className="reco-node-list-wrapper" hidden={isHidden} aria-hidden={isHidden}>
            {
                data.map((nodeInfo, index) => {
                    if (nodeInfo.enablePagingChildren) {
                        return <PagingTreeNode
                            key={index}
                            nodeInfo={nodeInfo}
                            distance={distance}
                        />;
                    }

                    return <TreeNode
                        key={index}
                        nodeInfo={nodeInfo}
                        distance={distance}
                    />;
                })
            }
        </div>
    );
};

export default TreeNodeList;