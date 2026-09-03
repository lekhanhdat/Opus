import { Component } from "react";
import PropTypes from "prop-types";
import { NodeLevel, NodeType } from "../../../../../Constants/DAEnums";
import NodeContent from "../../NodeContents/PhysicalExplorerNodeContent";
import {PhysicalObjectStatus} from "../../../../../Constants/Constants";

class PhyDestinationTree extends Component {
    constructor(props) {
        super(props);

        this.searchKey = "";
        this.pagerSize = 15;
        this.selectedNodeItem = null;
        this.treeContext = this.getTreeContext();

        if(props.treeData) {
            if(props.treeData.length > 0) {
                this.processChildren(props.treeData[0]);
            }
            this.state = { items: props.treeData };
        } else {
            this.state = { items: [] };
        }
    }

    componentDidMount() {
        if(!this.props.treeData) {
            this.initData();
        }
    }

    componentWillUnmount() {
        this.isUnmounted = true;
    }

    UNSAFE_componentWillReceiveProps(nextProps) { 
        if(nextProps.treeData != this.props.treeData) {
            if(nextProps.treeData) {
                this.setTreeData(nextProps.treeData);
            } else {
                this.initData();
            }
        } 
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
    }

    getTreeContext() {
        let mainComponent = this;
        return {
            nodeContentComponent: NodeContent,
            singleSelection: true,
            spaceNoSelection: true,
            shadowInitialNodelevel: NodeLevel.PhysicalBottomLocation,
            transToTreeNodeObject(oitem) {
                const hasSearchKey = mainComponent.props.searchKey !== "";

                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.NodeType,
                    text: this.getText(oitem),
                    disableSelect: this.isDisableSelect(oitem),
                    checked: oitem.Checked,
                    expanded: oitem.Expanded,
                    loaded: hasSearchKey ? true : this.isLoaded(oitem),
                    hasChildren: oitem.HasChildren,
                    isLeafNode: oitem.NodeType == mainComponent.props.leafNodeType || oitem.NodeType == NodeType.PhyCustom,
                    enableContextMenu: oitem.NodeType != mainComponent.props.leafNodeType && oitem.NodeType != NodeType.PhyCustom ,
                    items: oitem.Children,
                    itemsCount:
                        oitem.ChildrenCount > 0
                            ? oitem.ChildrenCount
                            : oitem.HasChildren
                                ? 1
                                : 0,
                    pagerByServer: true,
                    exactPaging: false,
                    hasNextPage: hasSearchKey ? false : this.hasNextPage(oitem),
                    pagerSize: hasSearchKey ? 1000 : mainComponent.pagerSize,
                    pagerIndex: oitem.PagerIndex,
                    pagerAnchor: oitem.PagePosition
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.Checked = item.checked;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PagerIndex = item.pagerIndex;
                oitem.PagerSize = item.pagerSize;
                oitem.PagePosition = item.pagerAnchor;
            },
            getText(oitem) {
                return oitem.Name;
            },
            isLoaded(oItem) {
                if (!oItem.Children) {
                    return false;
                }
                return oItem.Children.length > 0;
            },
            isDisableSelect(oitem){
                let isDisabled = false;
                if(oitem.NodeType < NodeType.PhysicalBottomLocation || oitem.NodeType == NodeType.PhyCustom)
                {
                    isDisabled = true;
                }
                else if(oitem.NodeType == NodeType.PhyBox){
                    try{
                        let nodeStatus = oitem.RecordStatus;
                        if (nodeStatus == PhysicalObjectStatus.Destroyed || nodeStatus == PhysicalObjectStatus.Closed
                            || nodeStatus == PhysicalObjectStatus.Missing) {
                            isDisabled = true;
                        }
                    }catch(e){
                        isDisabled = false;
                    }
                }
                // For leaf node type is PhyFile, only folder is selectable.
                if(mainComponent.props.leafNodeType == NodeType.PhyFile) {
                    if(oitem.NodeType < NodeType.PhyFile) {
                        isDisabled = true;
                    }
                    else if(oitem.NodeType == NodeType.PhyFile) { 
                        try{
                            let nodeStatus = oitem.RecordStatus;
                            if (nodeStatus == PhysicalObjectStatus.Destroyed || nodeStatus == PhysicalObjectStatus.Closed
                                || nodeStatus == PhysicalObjectStatus.Missing) {
                                isDisabled = true;
                            }
                        }catch(e){
                            isDisabled = false;
                        }
                    }
                }
                return isDisabled;
            },
            hasNextPage(oitem) {
                let pSize = !oitem.PagerSize ? mainComponent.pagerSize : oitem.PagerSize;
                if(oitem.NodeType < NodeType.PhysicalBottomLocation) {
                    return oitem.ChildrenCount > (pSize * (oitem.PagerIndex + 1));
                } else {
                    return oitem.HasNextPage;
                }
            },
            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let poItem = parentItem.origin;
                poItem.LeafNodeType = mainComponent.props.leafNodeType;
                poItem.SearchKey = mainComponent.props.searchKey;
                if (poItem.SearchKey !== "") {
                    poItem.IsSearch = true;
                } else {
                    poItem.IsSearch = false;
                }
                fetchUtility({
                    url: "/api/PhysicalRecordApi/BrowseTree",
                    data: poItem
                }).then(res => {
                    if (res) {
                        funcSuccess(res.Children, res);
                    } else {
                        poItem.HasChildren = false;
                        poItem.Children = [];
                        poItem.ChildrenCount = 0;
                    }
                    
                }).catch(e => funcFail(e));
                //return children node items
                return [];
            },
            onNodeSelected(item) {
                let oItem = item.origin;
                oItem.Checked = true;
                if (mainComponent.selectedNodeItem) {
                    mainComponent.selectedNodeItem.Checked = false;
                }
                mainComponent.selectedNodeItem = oItem;
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(oItem, mainComponent.state.items);
                }
            }
        };
    }

    setTreeData(data) {
        this.treeCache = [];
        if (data && data.length > 0) {
            $.each(data, (idx, item) => {
                this.treeCache[item.Id] = item;
                this.rootItem = item;
            });
            this.processChildren(this.rootItem);
            this.setState({ items: [this.rootItem] });
        }
    }

    initData() {
        const options = {
            url: "/api/PhysicalRecordApi/InitTree",
            method: "POST",
        }
        if (this.props.leafNodeType == NodeType.PhyFile && !this.props.data.isGlobalSearch) {
            const sources = this.props.data.Source ?? [];
            const recordId = sources[0]?.Id;
            options.data = recordId ?? null;
        }

        fetchUtility(options).then(res => {
            if (!this.isUnmounted) {
                this.setTreeData(res);
            }
        });
    }

    processChildren(node) {
        if (node.Children && node.Children.length > 0 && node.NodeType < this.props.leafNodeType) {
            node.Expanded = true;
            node.Children.map(child => {
                this.processChildren(child);
            });
        } else if(node.NodeType >= this.props.leafNodeType) {
            node.HasChildren = false;
            node.Children = null;
            node.ChildrenCount = 0;
        }
    }

  search = async (keywords) => {
        this.searchKey = keywords.trim();
        this.treeContext.searchKey = this.searchKey;
        if (this.searchKey && this.searchKey.length > 0) {
            $$.loading(true);
            const url = "/api/PhysicalRecordApi/BrowseSearchTree";
            const rootItemForsearch = RM.deepcopy(this.rootItem);
            rootItemForsearch.SearchKey = keywords;
            rootItemForsearch.PagerIndex = 0;
            rootItemForsearch.PagerSize = 15;
            rootItemForsearch.Children = null;
            rootItemForsearch.IsSearch = true;
            rootItemForsearch.IsGlobalSearch = !!this.props.data.isGlobalSearch;
            rootItemForsearch.IsSearchFolder = this.props.leafNodeType == NodeType.PhyFile;
            const data = {
                url,
                data: rootItemForsearch,
            };
            const result = await fetchUtility(data);
            if (result.IsSearch) {
                this.props.onSetIsExceedLimitSearch(!result.CanSearch);
            }
            let treeData = [result];
            if (this.props.leafNodeType == NodeType.PhyFile && !this.props.data.isGlobalSearch) {
                treeData = result?.Children ?? [];
            }
            this.setState({ items: treeData });
            $$.loading(false);
        } else {
            this.props.onSetIsExceedLimitSearch(false);
            this.setState({ items: [this.rootItem] });
        }
    }

    render() {
        return (
            <div>
                <$g.TreeView
                    id="peTree"
                    items={this.state.items}
                    treeContext={this.treeContext}
                />
            </div>
        );
    }

    //public functions:
    getTreeData = () => {
        return this.state.items;
    };
}

PhyDestinationTree.propTypes = {
    searchKey: PropTypes.string,
    leafNodeType: PropTypes.number,
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func
};
PhyDestinationTree.defaultProps = {
    searchKey: PropTypes.string,
    leafNodeType: NodeType.PhyBox,
    treeData: null,
    onSelectedNodeChanged: null
};

export default PhyDestinationTree;
