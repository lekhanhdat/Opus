import { Component } from "react";
import PropTypes from "prop-types";
import { NodeType,NodeLevel } from "../../../../../Constants/DAEnums";
import { PhysicalObjectStatus } from "../../../../../Constants/Constants";
import NodeContent from "../../NodeContents/PhysicalExplorerNodeContent";
import { PhysicalObjectStatusInherit, PhysicalObjectStatusBreakInherit } from "../Physical/PhysicalObjectStatusLegend";

class PhysicalExplorerTree extends Component {
    constructor(props) {
        super(props);

        this.pagerSize = 15;
        let mainComponent = this;
        this.selectedNodeItem = null;
        this.treeContext = {
            isMoveToRefresh: true,
            nodeContentComponent: NodeContent,
            showrRightArrow: true,
            // shadowInitialNodelevel: NodeLevel.PhysicalBottomLocation,
            singleSelection: true,
            treeCacheNodes: [],
            transToTreeNodeObject (oitem) {
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.NodeType,
                    showStatus: oitem.NodeType >= NodeType.PhysicalNormalLocation,
                    nodeStatusInfo: this.getStatusInfo(oitem),
                    text: this.getText(oitem),
                    checked: oitem.Checked,
                    expanded: oitem.Expanded,
                    loaded: !oitem.Children ? false : oitem.Children.length > 0,
                    hasChildren: this.hasChildren(oitem),
                    isLeafNode: oitem.NodeType == NodeType.PhyFile,
                    enableContextMenu: false,
                    items: oitem.Children,
                    breakInheritance: oitem.BreakInheritance,
                    itemsCount:
                        oitem.ChildrenCount > 0
                            ? oitem.ChildrenCount
                            : oitem.HasChildren
                                ? 1
                                : 0,
                    pagerByServer: true,
                    exactPaging: false,
                    hasNextPage: this.hasNextPage(oitem),
                    pagerSize: mainComponent.pagerSize,
                    pagerIndex: oitem.PagerIndex,
                    pagerAnchor: oitem.PagePosition
                };
            },
            updateOriginObject (item) {
                let oitem = item.origin;
                oitem.Checked = item.checked;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PagerIndex = item.pagerIndex;
                oitem.PagerSize = item.pagerSize;
                oitem.PagePosition = item.pagerAnchor;
            },
            getText (oitem) {
                return oitem.Name;
            },
            getStatusInfo (oitem) {
                //显示图标的优先级destroy/missing loan > open/close/
                if (!oitem.BreakInheritance) {
                    return mainComponent.getStatusIconInfo(oitem, PhysicalObjectStatusInherit);
                } else {
                    return mainComponent.getStatusIconInfo(oitem, PhysicalObjectStatusBreakInherit);
                }
            },
            hasChildren (oitem) {
                if (oitem.Loaded) {
                    return oitem.Children && oitem.Children.length > 0;
                } else {
                    return oitem.HasChildren;
                }
            },
            hasNextPage (oitem) {
                let pSize = !oitem.PagerSize ? mainComponent.pagerSize : oitem.PagerSize;
                if (oitem.NodeType < NodeType.PhysicalBottomLocation) {
                    return oitem.ChildrenCount > (pSize * (oitem.PagerIndex + 1));
                } else {
                    return oitem.HasNextPage;
                }
            },
            onExpandClick (parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },
            onLoadNodes (parentItem, funcSuccess, funcFail) {
                let poItem = parentItem.origin;
                fetchUtility({
                    url: "/api/PhysicalRecordApi/BrowseTree",
                    data: poItem
                }).then(res => {
                    if (res) {
                        if (mainComponent.selectedNodeItem) {
                            let selId = mainComponent.selectedNodeItem.Id;
                            for (const child of res.Children) {

                                this.addTreeCacheNodes(child);
                                if (child.Id == selId) {
                                    child.Checked = true;
                                    break;
                                }
                            }
                        }
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
            onNodeSelected (item) {
                let oItem = item.origin;
                oItem.Checked = true;
                if (mainComponent.selectedNodeItem) {
                    mainComponent.selectedNodeItem.Checked = false;
                }
                mainComponent.selectedNodeItem = oItem;
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(oItem);
                }
            },
            addTreeCacheNodes (node){
                if(this.treeCacheNodes.find(o => o.Id == node.Id) === undefined)
                {
                    this.treeCacheNodes.push({
                        Name: node.Name,
                        NodeType: node.NodeType,
                        Id: node.Id,
                        ParentId: node.ParentId,
                        LocationId: node.LocationId
                    });
                }
            },
            
        };
        this.state = { items: [] };
    }

    componentDidMount () {
        if (!this.props.data) {
            this.initData();
        } else if (this.props.data.length > 0) {
            this.setTreeData(this.props.data);
        }
    }

    UNSAFE_componentWillReceiveProps (nextProps) {
        if (this.props.data != nextProps.data && nextProps.data && nextProps.data.length > 0) {
            this.setTreeData(nextProps.data);
        }
    }

    getTreeCacheNodes = () => {
        return this.treeContext.treeCacheNodes;
    }

    initData () {
        setTimeout(() => $$.loading(true), 200);
        fetchUtility({
            url: "/api/PhysicalRecordApi/InitTree"
        }).then(res => {
            setTimeout(() => $$.loading(false), 200);
            if (res && res.length > 0) {
                let rootItem = res[0];
                rootItem.Checked = true;
                this.setTreeData(res);
                if (this.props.rootSelectedDefault && this.props.onSelectedNodeChanged) {
                    this.props.onSelectedNodeChanged(rootItem);
                }
            }
        });
    }

    processChildren (node) {
        if (node.Checked) {
            this.selectedNodeItem = node;
        }
        if (node.Children && node.Children.length > 0) {
            node.Expanded = true;
            node.Children.map(child => {
                this.treeContext.addTreeCacheNodes(child);
                this.processChildren(child);

            });
        }
    }

    setTreeData (data) {
        this.processChildren(data[0]);
        this.setState({ items: data });
    }

    getStatusIconInfo(oitem, iconList){
        if(oitem.NodeType < NodeType.PhyCustom){
            if(oitem.BreakInheritance){
                return iconList.filter((item)=>{ return item.statusKey == PhysicalObjectStatus.Open; })[0];
            }else{
                return "";
            }
        }else{
            if(oitem.OnLoan){
                if(oitem.RecordStatus == PhysicalObjectStatus.Destroyed || oitem.RecordStatus == PhysicalObjectStatus.Missing){
                    return iconList.filter((item)=>{return item.statusKey == oitem.RecordStatus;})[0];
                }else{
                    return iconList.filter((item)=>{return item.statusKey == "loaned";})[0];
                }
            }else{
                return iconList.filter((item)=>{ return item.statusKey == oitem.RecordStatus; })[0];
            }
        }
    }

    //public functions:
    refreshSelectedNode = (updateProps) => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
                if (updateProps) {
                    Object.assign(selNode.props.item.origin, updateProps);
                    selNode.props.item.text = updateProps.Name;
                    selNode.props.item.breakInheritance = updateProps.BreakInheritance;
                    selNode.props.item.nodeStatusInfo = this.treeContext.getStatusInfo(updateProps);
                }
                if (selNode.props.item.nodeType != NodeType.PhyFile) {
                    selNode.reload(0);
                } else {
                    selNode.reRender();
                }
            }
        }
    };

    refreshSelectedParentNode = () => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
                let parentNode = selNode.props.parentItemComponent;
                this.selectedNodeItem = parentNode.props.item.origin;
                parentNode.setSelectedStatus();
                parentNode.reload(0);
                if (this.props.onSelectedNodeChanged) {
                    this.props.onSelectedNodeChanged(parentNode.props.item.origin, true);
                }
            }
        }
    };

    refreshMoveToNode = (id) => {
        this.treeContext.expandBottomLocationAndBoxNodes.forEach((item) => {
            if (item.props.item.origin.Id == id) {
                item.reload(0);
            }
        });
    }
    deleteSelectedNode = () => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
                if (selNode.props.item.nodeType > NodeType.PhysicalBottomLocation) {
                    let parentNode = selNode.props.parentItemComponent;
                    parentNode.setSelectedStatus();
                    parentNode.reload(0);
                    if (this.props.onSelectedNodeChanged) {
                        this.props.onSelectedNodeChanged(parentNode.props.item.origin);
                    }
                }
            }
        }
    };

    render () {
        return (<React.Fragment>
            <$g.TreeView
                id="peTree"
                items={this.state.items}
                treeContext={this.treeContext}
            />
        </React.Fragment>);
    }
}

PhysicalExplorerTree.propTypes = {
    data: PropTypes.array,
    rootSelectedDefault: PropTypes.bool,
    onSelectedNodeChanged: PropTypes.func
};
PhysicalExplorerTree.defaultProps = {
    data: null,
    rootSelectedDefault: false,
    onSelectedNodeChanged: null
};

export default PhysicalExplorerTree;
