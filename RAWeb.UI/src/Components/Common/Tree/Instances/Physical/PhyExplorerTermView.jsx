import { Component } from 'react';
import PropTypes from 'prop-types';
import { SourceFlags, TypeString } from '../../../../../Constants/Constants.jsx';
import { NodeType } from "../../../../../Constants/DAEnums";
import TreeNodeContent from '../../NodeContents/PhysicalExplorerTermViewNodeContent';
import { PhysicalObjectStatus } from "../../../../../Constants/Constants";
import { PhysicalObjectStatusInherit, PhysicalObjectStatusBreakInherit } from "../Physical/PhysicalObjectStatusLegend";

class TreeReqObj {
    constructor(termId, nodeId, nodeType, pageIndex, pageSize, pagePosition, sourceFlag, containerId) {
        this.TermId = termId;
        this.NodeId = nodeId;
        this.NodeType = nodeType;
        this.PageIndex = !pageIndex ? 1 : (pageIndex + 1);
        this.PageSize = !pageSize ? 10 : pageSize;
        this.PagePosition = pagePosition;
        this.SourceFlag = SourceFlags.Phy;
        this.ContainerId = containerId;
        this.ExcludeBuiltIn = false;
    }
}

class PhyExplorerTermView extends Component {
    constructor(props) {
        super(props);
        this.state = {
            treeData: []
        };
        this.selectedNodeItem = null;
        this.pagerSize = 15;
        this.treeContext = this.getTreeContext(this);
        this.getTreeData(this);
    }

    getTreeContext (that) {
        let mainComponent = that;
        let self = this;
        let rootItem = self.props.rootItem;
        let termNodeType = [TypeString.ROOT, TypeString.TERM_GROUP, TypeString.TERM_SET];
        let virtualNodeType = [TypeString.BOXES, TypeString.FILES, TypeString.SUB_TERM];
        let physicalNodeType = [TypeString.PhyBox, TypeString.PhyFile];
        let termLeafNodeType = [TypeString.TERM];
        return {
            nodeContentComponent: TreeNodeContent,
            singleSelection: true,
            transToTreeNodeObject (oItem) {
                let isRoot = oItem.Id == rootItem.nodeId;
                if (physicalNodeType.indexOf(oItem.Type) >= 0) {
                    return {
                        origin: oItem,
                        nodeKey: oItem.Id,
                        nodeType: oItem.NodeType,
                        text: oItem.Name,
                        checked: oItem.Checked,
                        expanded: oItem.Expanded,
                        //loaded: !oItem.Children ? false : oItem.Children.length > 0,
                        loaded: true,
                        iconClass: this.getNodeIconClass(oItem),
                        showStatus: oItem.NodeType >= NodeType.PhyBox,
                        nodeStatusInfo: this.getStatusInfo(oItem),
                        hasChildren: this.hasChildren(oItem),
                        isLeafNode: oItem.NodeType == NodeType.PhyFile,
                        items: oItem.Children,
                        breakInheritance: oItem.BreakInheritance,
                        itemsCount:
                            oItem.ChildrenCount > 0
                                ? oItem.ChildrenCount
                                : oItem.HasChildren
                                    ? 1
                                    : 0,
                        pagerByServer: true,
                        exactPaging: false,
                        hasNextPage: oItem.HasNextPage,
                        pagerSize: mainComponent.pagerSize,
                        pagerIndex: oItem.PagerIndex,
                        pagerAnchor: oItem.PagePosition,
                        enableContextMenu: false
                    };
                } else if (virtualNodeType.indexOf(oItem.Type) >= 0) {
                    return {
                        origin: oItem,
                        nodeKey: oItem.Id,
                        nodeType: oItem.Type,
                        termId: oItem.TermId,
                        iconClass: this.getNodeIconClass(oItem),
                        text: oItem.Name,
                        checked: false,
                        hasChildren: virtualNodeType.indexOf(oItem.Type) >= 0,
                        pagerByServer: true,
                        exactPaging: false,
                        hasNextPage: oItem.hasNextPage,
                        pagerIndex: oItem.PagerIndex,
                        pagerSize: 15,
                        enableContextMenu: true,
                        clickNodeExpand: true,
                        disableSelect: true
                    };
                } else {
                    return {
                        origin: oItem,
                        nodeKey: this.getNodeKey(oItem),
                        nodeId: this.getNodeId(oItem),
                        nodeType: oItem.Type,
                        nodeClass: null,
                        iconClass: this.getNodeIconClass(oItem),
                        text: oItem.Name,
                        expanded: isRoot && rootItem.expandDefault,
                        checked: oItem.Checked,
                        loaded: oItem.Type == TypeString.TERM ? false : oItem.subTermCount == 0 || !!oItem.subTerms,
                        items: oItem.subTerms,
                        hasChildren: oItem.Type == TypeString.TERM ? true : oItem.subTermCount > 0,
                        pagerByServer: true,
                        itemsCount: oItem.subTermCount ? oItem.subTermCount : 0,
                        pagerIndex: oItem.PagerIndex,
                        pagerSize: 15,
                        enableContextMenu: true,
                        clickNodeExpand: true,
                        disableSelect: true
                    };
                }
            },
            getNodeKey (oItem) {
                if (oItem) {
                    return oItem.Type == TypeString.TERM_GROUP || oItem.Type == TypeString.TERM ? oItem.UniqueId : oItem.Id;
                } else {
                    return null;
                }
            },
            getNodeId (oItem) {
                if (oItem) {
                    return oItem.Type == TypeString.TERM_GROUP ? oItem.UniqueId : oItem.Id;
                } else {
                    return null;
                }
            },
            hasChildren (oItem) {
                if (oItem.Loaded) {
                    return oItem.Children && oItem.Children.length > 0;
                } else {
                    return oItem.HasChildren;
                }
            },
            updateOriginObject (item) {
                let oItem = item.origin;
                oItem.Checked = item.checked;
                oItem.Loaded = item.loaded;
                oItem.Expanded = item.expanded;
                oItem.PagerIndex = item.pagerIndex;
                oItem.PagerSize = item.pagerSize;
            },
            getNodeIconClass (oItem) {
                if (oItem.NodeType == NodeType.PhyBox) {
                    oItem.Type = TypeString.PhyBox;
                }
                if (oItem.NodeType == NodeType.PhyFile) {
                    oItem.Type = TypeString.PhyFile;
                }
                switch (oItem.Type) {
                    case TypeString.ROOT:
                    case TypeString.TERM_GROUP:
                        return 'fia-term-group';
                    case TypeString.TERM_SET:
                        return 'fia-term-set';
                    case TypeString.TERM:
                        return 'fia-term';
                    case TypeString.BOXES:
                        return 'ra-tree-node-icon fia-box-suite';
                    case TypeString.FILES:
                        return 'ra-tree-node-icon fia-folder';
                    case TypeString.SUB_TERM:
                        return 'ra-tree-node-icon fia-term-set';
                    case TypeString.PhyBox:
                        return 'ra-tree-node-icon fia-box-suite';
                    case TypeString.PhyFile:
                        return 'ra-tree-node-icon fia-folder';

                }
            },
            getStatusInfo (oitem) {
                //显示图标的优先级destroy > loan > open/close/missing
                if (!oitem.BreakInheritance) {
                    return mainComponent.getStatusIconInfo(oitem, PhysicalObjectStatusInherit);
                } else {
                    return mainComponent.getStatusIconInfo(oitem, PhysicalObjectStatusBreakInherit);
                }
            },
            sortChild (a, b) {
                if (a.Type == TypeString.TERM_GROUP || a.Name == b.Name) {
                    return 0;
                } else if (a.Name.toLowerCase() > b.Name.toLowerCase()) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes (parentItem, funcSuccess, funcFail) {
                if ((termLeafNodeType.concat(virtualNodeType)).indexOf(parentItem.nodeType) >= 0) {
                    let termId;
                    switch (parentItem.nodeType) {
                        case TypeString.TERM:
                            termId = parentItem.origin.Id;
                            break;
                        default:
                            termId = parentItem.origin.TermId;
                            break;
                    }
                    let option = {
                        url: `/api/PhysicalRecordApi/GetTermTreeViewChildrenTreeNodes`,
                        data: new TreeReqObj(
                            termId,
                            parentItem.nodeKey,
                            parentItem.nodeType,
                            parentItem.pagerIndex,
                            parentItem.pagerSize,
                            parentItem.pagerAnchor,
                        )
                    };
                    fetchUtility(option).then((res) => {
                        if (res) {
                            if (parentItem.nodeType == TypeString.BOXES || parentItem.nodeType == TypeString.FILES) {
                                let obj = $.parseJSON(res);
                                parentItem.hasNextPage = obj.HasNextPage;
                                parentItem.pagerAnchor = obj.PagePosition;
                                funcSuccess(obj.Children, parentItem);
                            } else {
                                funcSuccess($.parseJSON(res));
                            }
                        } else {
                            funcSuccess([]);
                        }
                    }).catch((e) => {
                        console.error(e);
                        funcFail(e);
                    });
                } else if (termNodeType.indexOf(parentItem.nodeType) >= 0) {
                    let oItem = parentItem.origin;
                    var nId = oItem.Type == TypeString.TERM_GROUP ? oItem.UniqueId : oItem.Id;
                    let option = {
                        url: "/api/TermManagementApi/GetAllChildren",
                        data: new TreeReqObj(
                            nId,
                            nId,
                            parentItem.nodeType,
                            parentItem.pagerIndex
                        )
                    };
                    fetchUtility(option).then((res) => {
                        let oItems = $.parseJSON(res);
                        let pagerIndex = parentItem.pagerIndex;
                        if (!pagerIndex) {
                            pagerIndex = 0;
                        }
                        oItems = oItems.slice(pagerIndex * parentItem.pagerSize, (pagerIndex + 1) * parentItem.pagerSize);
                        oItems.filter(oItem => {
                            oItem.IsChecked = parentItem.checked;
                            return true;
                        });
                        parentItem.origin.subTerms = oItems;
                        parentItem.origin.subTermCount = oItems.length;
                        parentItem.hasChildren = oItems.length > 0;
                        funcSuccess(oItems);
                    }).catch((e) => {
                        funcFail(e);
                    });
                }
            },
            onNodeSelected (item) {
                let funcChange = self.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },
            onNodeSelectedChange (selectedItems) {
            }
        };
    }

    getTreeData () {
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetChildrenByDBForView",
            contentType: "application/json;charset=utf-8",
            data: { SourceFlag: SourceFlags.Phy, ExcludeBuiltIn: false },
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                setTimeout(() => {
                    $$.loading(false);
                }, 500);
            },
            success: (data) => {
                this.resetTreeData(data);
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    resetTreeData (data) {
        let root = {
            Name: RMResx.RM_JS_TM_RootTerms,
            Type: "Root",
            Id: "Root",
            Checked: true,
            subTerms: $.parseJSON(data) // Fortify Issue Type: JSON Injection; Sink Details: reset tree data; Ignore Reason: 前后台对象存在对应关系
        };
        root.subTermCount = root.subTerms.length;
        this.setState({ treeData: [root] });
        if (this.props.onSelectedNodeChanged) {
            this.props.onSelectedNodeChanged(root);
        }
    }

    //public functions:
    refreshSelectedNode = (updateProps) => {
        let selectedNodes = this.treeContext.selectedNodes;
        if (selectedNodes) {
            for (const key in selectedNodes) {
                const selNode = selectedNodes[key];
                if (updateProps) {
                    Object.assign(selNode.props.item.origin, updateProps);
                    selNode.props.item.text = updateProps.Name;
                    selNode.props.item.breakInheritance = updateProps.BreakInheritance;
                    selNode.props.item.nodeStatusInfo = this.treeContext.getStatusInfo(updateProps);
                }
                // if (selNode.props.item.nodeType != NodeType.PhyFile) {
                //     selNode.reload(0);
                // } else {
                //     selNode.reRender();
                // }
            }
        }
    };

    refreshSelectedParentNode = () => {
        let selectedNodes = this.treeContext.selectedNodes;
        if (selectedNodes) {
            for (const key in selectedNodes) {
                const selNode = selectedNodes[key];
                if (selNode.props.item.nodeType > NodeType.PhysicalBottomLocation) {
                    let parentNode = selNode.props.parentItemComponent;
                    this.selectedNodeItem = parentNode.props.item.origin;
                    parentNode.setSelectedStatus();
                    parentNode.reload(0);
                    if (this.props.onSelectedNodeChanged) {
                        this.props.onSelectedNodeChanged(parentNode.props.item.origin, true);
                    }
                }
            }
        }
    };

    deleteSelectedNode = () => {
        let selectedNodes = this.treeContext.selectedNodes;
        if (selectedNodes) {
            for (const key in selectedNodes) {
                const selNode = selectedNodes[key];
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

    
    getStatusIconInfo(oitem, iconList){
        if(oitem.NodeType < NodeType.PhyCustom ){
            if(oitem.BreakInheritance){
                return iconList.filter((item)=>{return item.statusKey == PhysicalObjectStatus.Open;})[0];
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
                return iconList.filter((item)=>{return item.statusKey == oitem.RecordStatus;})[0];
            }
        }
    }

    render () {
        return <React.Fragment>
            <$g.TreeView
                id="teTree"
                items={this.state.treeData}
                treeContext={this.treeContext}
            />
        </React.Fragment>;
    }
}

PhyExplorerTermView.propTypes = {
    rootItem: PropTypes.object,
    onSelectedNodeChanged: PropTypes.func
};
PhyExplorerTermView.defaultProps = {
    rootItem: {
        nodeId: "Root",
        nodeType: TypeString.ROOT,
        allowSelected: false,
        expandDefault: true
    },
    onSelectedNodeChanged: (terms) => {
        //console.log("all selected terms: ", terms);
    }
};

export default PhyExplorerTermView;