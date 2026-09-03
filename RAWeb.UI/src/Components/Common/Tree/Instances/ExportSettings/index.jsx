import { Component } from "react";
import TermManagementNodeContent from "../../NodeContents/TermManagementNodeContent";
import { cloneDeep, isEqual } from "lodash";
import StringUtil from "../../../../../Utilities/StringUtil";

class ExportSettingsTree extends Component {
    constructor(props) {
        super(props);
        this.treeContext = this.getTreeContext();
        this.state = {
            treeData: [],
        };
        this.level = 0;
    }

    componentDidMount() {
        this.level = 0;
        this.cacheData = this.convertToTree(this.props.items, "ChildVEOInfo");
        this.setState({
            treeData: cloneDeep(this.cacheData),
        });
    }

    componentDidUpdate(prevProps) {
        const currentRootItem = this.props.items[0];
        const prevRootItem = prevProps.items[0];
        if (!isEqual(currentRootItem?.SourceFlag, prevRootItem?.SourceFlag) || !isEqual(currentRootItem?.ExportType, prevRootItem?.ExportType)) {
            this.level = 0;
            this.cacheData = this.convertToTree(this.props.items, "ChildVEOInfo");
            this.setState({
                treeData: cloneDeep(this.cacheData),
            });
        }
    }

    convertToTree = (tree, childrenKey) => {
        this.level++;
        return tree.map((node) => {
            const newNode = {
                ...node,
                Id: StringUtil.newGuid(),
                Level: this.level,
            };
            if (
                Array.isArray(node[childrenKey]) &&
                node[childrenKey].length > 0
            ) {
                newNode[childrenKey] = this.convertToTree(
                    node[childrenKey],
                    childrenKey
                );
            }
            return newNode;
        });
    };

    addNodeByParentId = (nodes, parentId, newNode) => {
        const updatedNewNode = {
            ...newNode,
            DefaultValue: null,
            ExchangeMetadata: null,
            ExchangeMetadataAsSource: null,
            ExportType: this.props.exportType,
            SharePointMetadata: null,
            SharePointMetadataAsSource: null,
            SourceFlag: this.props.sourceFlag,
        };
        const updatedNodes = nodes.map((node) => {
            if (node.Id === parentId) {
                return {
                    ...node,
                    ChildVEOInfo: node.ChildVEOInfo
                        ? [...node.ChildVEOInfo, updatedNewNode]
                        : [updatedNewNode],
                };
            }
            return {
                ...node,
                ChildVEOInfo: node.ChildVEOInfo?.length > 0
                        ? this.addNodeByParentId(
                              node.ChildVEOInfo,
                              parentId,
                              updatedNewNode
                          )
                        : node.ChildVEOInfo,
            };
        });
        // this.props.onActionNode?.(updatedNodes);
        return updatedNodes;
    };

    deleteNodeById = (nodes, Id) => {
        const updatedNodes = nodes
            .filter((node) => node.Id !== Id)
            .map((node) => ({
                ...node,
                ChildVEOInfo: node.ChildVEOInfo?.length > 0
                        ? this.deleteNodeById(node.ChildVEOInfo, Id)
                        : node.ChildVEOInfo,
            }));
        // this.props.onActionNode?.(updatedNodes);
        return updatedNodes;
    };

    uploadNodeByNewNode = (nodes, newNode) => {
        return nodes.map((node) => {
            if (node.Id === newNode.Id) {
                return { ...node, ...newNode };
            }
            return {
                ...node,
                ChildVEOInfo: node.ChildVEOInfo?.length > 0
                        ? this.uploadNodeByNewNode(node.ChildVEOInfo, newNode)
                        : node.ChildVEOInfo,
            };
        });
    };

    getTreeContext = () => {
        return {
            nodeContentComponent: TermManagementNodeContent,
            treeType: 5, //1:TermManagement, 2:LocationManagement, 3:Template, 4:RuleContainer, 5:ExportSettings
            singleSelection: true,
            transToTreeNodeObject: (oitem) => {
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Id,
                    level: oitem.Level,
                    isLeafNode: false,
                    text: oitem.TreeNodeName,
                    disableSelect: oitem.Level === 1,
                    loaded: true,
                    expanded: oitem.Level < 2,
                    items: oitem.ChildVEOInfo,
                    enableContextMenu: true,
                    itemsCount: oitem.ChildVEOInfo
                        ? oitem.ChildVEOInfo.length
                        : 0,
                    pagerSize: 100000000000000,
                    pagerIndex: oitem.PageIndex,
                    exactPaging: false,
                    hasNextPage: false,
                };
            },

            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },

            confirmOnNodeSelected: (item, funcAllow) => {
                this.props.onSelectedNode?.(this.findNodeById(this.cacheData, item.nodeKey), funcAllow);
            },

            getNewNode: (parentItem, newVal) => {
                let newItem = this.treeContext.transToTreeNodeObject({
                    Id: StringUtil.newGuid(),
                    Level: parentItem.origin.Level + 1,
                    TreeNodeName: newVal,
                    MetadataName: "ChildMeta1",
                    DefaultValue: null,
                    ExchangeMetadataAsSource: null,
                    ExchangeMetadata: null,
                    SharePointMetadataAsSource: null,
                    SharePointMetadata: null,
                    ChildVEOInfo: [],
                    ChildTable: [],
                });

                return newItem;
            },

            refreshSelectedNodeInfo: (actionType, currentItem, parentItem) => {
                if (actionType === "delete" && currentItem?.Id) {
                    this.cacheData = this.deleteNodeById(
                        this.cacheData,
                        currentItem.Id
                    );
                }
                if (actionType === "add" && currentItem?.Id && parentItem?.Id) {
                    this.cacheData = this.addNodeByParentId(
                        this.cacheData,
                        parentItem.Id,
                        currentItem
                    );
                }
                this.props.onActionNode?.(
                    this.filterEmptyIdNodes(this.cacheData),
                    actionType
                );
            },
        };
    };

    refreshSelectedNode = (newNode) => {
        let selectedNode = this.treeContext.selectedNodes;
        Object.assign(
            selectedNode[newNode.Id].props.item,
            this.treeContext.transToTreeNodeObject(newNode)
        );
        // selectedNode[newNode.Id].reRender();
        this.cacheData = this.uploadNodeByNewNode(this.cacheData, newNode);
    };

    findNodeById = (nodes, id) => {
        if (!Array.isArray(nodes)) return null;
        for (const node of nodes) {
            if (node.Id === id) {
                return node;
            }
            const childResult = this.findNodeById(node.ChildVEOInfo, id);
            if (childResult) {
                return childResult;
            }
        }
        return null;
    }

    filterEmptyIdNodes = (nodes) => {
        if (!Array.isArray(nodes)) return [];
        return nodes
            .filter((node) => node && node.Id)
            .map((node) => ({
                ...node,
                ChildVEOInfo: this.filterEmptyIdNodes(node.ChildVEOInfo),
            }));
    };

    getTreeData() {
        return this.filterEmptyIdNodes(this.cacheData);
    }

    render() {
        return (
            <div>
                <$g.TreeView
                    id="treeview"
                    items={this.state.treeData}
                    treeContext={this.treeContext}
                />
            </div>
        );
    }
}

export default ExportSettingsTree;
