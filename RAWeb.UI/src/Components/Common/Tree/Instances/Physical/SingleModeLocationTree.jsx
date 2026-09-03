import { Component } from "react";
import PropTypes from "prop-types";
import { NodeType, NodeLevel } from "../../../../../Constants/DAEnums";
import NodeContent from "../../NodeContents/PhysicalExplorerNodeContent";

class SingleModeLocationTree extends Component {
    constructor(props) {
        super(props);

        this.treeCache = {};

        this.selectedItemId = this.props.selectedItemId;
        
        this.initTreeContext();

        this.state = {
            items: []
        };
    }

    componentDidMount() {
        if(this.props.data) {
            this.setTreeData(this.props.data);
        } else {
            this.initTreeData();
        }
    }

    UNSAFE_componentWillReceiveProps(nextProps) { 
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
        if(!this.selectedItemId && nextProps.selectedItemId != this.props.selectedItemId) {
            this.selectedItemId = nextProps.selectedItemId;
        }
    }

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            nodeContentComponent: NodeContent,
            singleSelection: true,
            readonly: mainComponent.props.readonly,
            transToTreeNodeObject(oitem) {
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.NodeType,
                    text: oitem.Name,
                    //iconClass: this.getIconClass(oitem),
                    disableSelect: this.isDisableSelect(oitem),
                    checked: oitem.Checked,
                    expanded: oitem.Expanded,
                    loaded: !oitem.Children ? false : oitem.Children.length > 0,
                    hasChildren: oitem.HasChildren,
                    isLeafNode: oitem.NodeType == NodeLevel.PhysicalBottomLocation,
                    enableContextMenu: !this.searchKey && oitem.NodeType != NodeLevel.PhysicalBottomLocation,
                    items: oitem.Children,
                    itemsCount: oitem.ChildrenCount,
                    pagerByServer: !this.searchKey,
                    pagerSize: 10,
                    pagerIndex: oitem.PagerIndex
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.Checked = item.checked;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PagerIndex = item.pagerIndex;
                oitem.PagerSize = 10;
            },
            isDisableSelect(oitem){
                let isDisabled = false;
                if(oitem.NodeType == NodeType.PhysicalRootLocation)
                {
                    isDisabled = true;
                }
                return isDisabled;
            },
            onNodeSelected(item) {
                let preSelItem = mainComponent.treeCache[mainComponent.selectedItemId];
                if(preSelItem) {
                    preSelItem.Checked = false;
                }
                mainComponent.selectedItemId = item.nodeKey;
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let poItem = parentItem.origin;
                mainComponent.loadChildren(poItem, res => {
                    if (res) {
                        for (const child of res.Children) {
                            child.Checked = child.Id == mainComponent.selectedItemId;
                        }
                        parentItem.itemsCount = res.ChildrenCount;
                        funcSuccess(res.Children, res);
                    } else {
                        poItem.HasChildren = false;
                        poItem.Children = [];
                        poItem.ChildrenCount = 0;
                    }
                    
                });
            },
            onTreeChanged() {
                if(mainComponent.props.onTreeChanged) {
                    mainComponent.props.onTreeChanged();
                }
            }
        };
    }

    initTreeData() {
        fetchUtility({
            url: "/api/LocationManagementApi/GetChildren",
            data: {NodeType: NodeLevel.PhysicalRootLocation, PagerIndex: 0, PagerSize: 10}
        }).then(res => {
            this.treeContext.searchKey = "";
            this.treeContext.pagerByServer = true;
            this.setTreeData(res);
        });
    }
    
    setTreeData(data, isSearch) {
        if(isSearch) {
            this.searchTreeData = data;
            if(this.selectedItemId) {
                this.processSearchData(data);
            }
        } else {
            this.treeCache = {};
            this.treeData = data;
            this.addToCache(data);
        }
        this.setState({ items: [data] });
    }

    addToCache(oitem, appendToParent) {
        this.removeChildrenCache(oitem);
        this.treeCache[oitem.Id] = oitem;
        if(appendToParent) {
            this.appendToParent(oitem);
        }
        if(oitem.Children) {
            for (const sub of oitem.Children) {
                this.addToCache(sub);
            }
        }
    }

    removeChildrenCache(oitem) {
        let tempItem = this.treeCache[oitem.Id];
        if(tempItem) {
            while(tempItem.Children && tempItem.Children > 0) {
                for (const sub of tempItem.Children) {
                    this.removeChildrenCache(sub);
                    delete this.treeCache[sub.Id];
                }
            }
        }
    }

    loadChildren(oitem, funcSuccess) {
        var data = Object.assign({}, oitem);
        data.Children = null;
        data.ChildStates = null;
        fetchUtility({
            url: "/api/LocationManagementApi/GetChildren",
            data: data
        }).then(funcSuccess).catch(e => {
        });
    }
    
    appendToParent(oitem) {
        let pItem = this.treeCache[oitem.ParentId];
        if(pItem) {
            if(!pItem.Children) {
                pItem.Children = [oitem];
            } else {
                pItem.Children.push(oitem);
            }
            
            this.addToCache(oitem);
        }
    }

    processSearchData(oitem) {
        if(oitem.Children) {
            for (const child of oitem.Children) {
                if(child.Id == this.selectedItemId) {
                    child.Checked = true;
                    return false;
                }
               
                if(!this.processSearchData(child)) {
                    return false;
                }
            }
        }
        return true;
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        return str;
    }

    search(key) {
        key = !key ? "" : key.trim();
        if (key.length > 0) {
            let option = {
                url: `/api/LocationManagementApi/SearchTree?searchKey=${this.replaceSpecialCharacters(key)}`,
                method: "get"
            };
            fetchUtility(option).then((res) => {
                this.treeContext.searchKey = key;
                this.treeContext.pagerByServer = false;
                this.setTreeData(res, true);
            }).catch((e) => {
    
            });
        } else {
            this.stopSearch();
        }
    }

    stopSearch() {
        this.treeContext.searchKey = "";
        this.checkAndSyncSearchTreeData();
        this.setState({items: [this.treeData]});
    }

    setSearchTreeCache(oitem) {
        this.searchTreeCache[oitem.Id] = oitem;
        if (oitem.Children && oitem.Children.length > 0) {
            for (const sub of oitem.Children) {
                this.setSearchTreeCache(sub);
            }
        }
    }

    checkAndSyncSearchTreeData() {
        let selItem = this.treeCache[this.selectedItemId];
        if(selItem) {
            selItem.Checked = true;
            this.expandParent(selItem);
        } else {
            this.searchTreeCache = {};
            this.setSearchTreeCache(this.searchTreeData);
            let selItem = this.searchTreeCache[this.selectedItemId];
            if(selItem) {
                this.syncSearchTreeData(selItem);
            }
        }
    }
    syncSearchTreeData(oitem) {
        let parent = this.treeCache[oitem.ParentId];
        if(parent) {
            parent.Expanded = true;
            parent.Loaded = true;
            if (!parent.Children || parent.Children.length == 0) {
                parent.Children = [oitem];
                parent.PagerIndex = 0;
                parent.ChildrenCount = 1;
            } else {
                parent.Children = [oitem, ...parent.Children];
                parent.ChildrenCount += 1;
            }
        } else {
            let srchParent = this.searchTreeCache[oitem.ParentId];
            srchParent.Children = srchParent.Children.slice(10 * (srchParent.PagerIndex || 0), 10);
            this.syncSearchTreeData(srchParent);
        }
    }
    expandParent(oitem) {
        let parent = this.treeCache[oitem.ParentId];
        if(parent) {
            parent.Expanded = true;
        }
    }

    //public functions:
    getTreeData = () => {
        if(this.treeContext.searchKey) {
            this.checkAndSyncSearchTreeData();
        }

        var treeItem = RM.SimplifyObject(this.treeData, ["Children", "OtherChildren"]);
        let results = {items: treeItem, selectedItemId: this.selectedItemId};
        return results;
    };

    renderTree() {
        return <$g.TreeView
            classicMode
            items={this.state.items}
            treeContext={this.treeContext}
        />;
    }

    render() {
        return (
            <div>
                {this.renderTree()}
            </div>
        );
    }
}

SingleModeLocationTree.propTypes = {
    data: PropTypes.object,
    searchKey: PropTypes.string,
    selectedItemId: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func
};
SingleModeLocationTree.defaultProps = {
    data: null,
    searchKey: null,
    selectedItemId: null,
    readonly: false,
    onTreeChanged: null
};

export default SingleModeLocationTree;
