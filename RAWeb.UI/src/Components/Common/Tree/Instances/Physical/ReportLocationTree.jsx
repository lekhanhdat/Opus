import { Component } from "react";
import PropTypes from "prop-types";
import { NodeType, NodeLevel } from "../../../../../Constants/DAEnums";
import NodeContent from "../../NodeContents/PhysicalExplorerNodeContent";

class ReportLocationTree extends Component {
    constructor(props) {
        super(props);

        this.virtualLevels = [NodeLevel.RMIncludeNew, NodeLevel.RMSelectAll];
        this.treeCache = {};
        
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
    }

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            nodeContentComponent: NodeContent,
            multiSelection: true,
            readonly: mainComponent.props.readonly,
            transToTreeNodeObject(oitem) {
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.NodeType,
                    text: oitem.Name,
                    // iconClass: this.getIconClass(oitem),
                    disableSelect: this.isDisableSelect(oitem),
                    checked: oitem.Checked,
                    enableIncludeNew: oitem.NodeType == NodeLevel.PhysicalNormalLocation,
                    includeNew: oitem.Checked || oitem.IncludeNew,
                    selectAll: this.isSelectAll(oitem),
                    selectAllBefore: oitem.Checked || oitem.SelectAllBefore,
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
                if(oitem.SelectAllBefore != item.selectAllBefore) {
                    for (const key in oitem.ChildStates) {
                        let idx = oitem.ChildStates[key][0];
                        oitem.ChildStates[key] = item.selectAllBefore ? [idx, 1] : [idx];
                    } 
                }

                let pItem = mainComponent.treeCache[oitem.ParentId];
                if(pItem && pItem.ChildStates) {
                    let arr = pItem.ChildStates[oitem.Id];
                    if(item.checked && arr.length == 1) {
                        arr.push(1);
                    } else if(!item.checked && arr.length > 1) {
                        pItem.ChildStates[oitem.Id] = [arr[0]];
                    }
                }
                
                oitem.Checked = item.checked;
                oitem.IncludeNew = item.includeNew;
                oitem.SelectAllBefore = item.checked || item.selectAllBefore;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PagerIndex = item.pagerIndex;
                oitem.PagerSize = 10;
            },
            getAllChildren(oitem) {
                let children = [...oitem.Children];
                
                return children;
            },
            isSelectAll(oitem) {
                if(oitem.Checked) {
                    return true;
                }
                if(oitem.ChildStates) {
                    for (const key in oitem.ChildStates) {
                        if(oitem.ChildStates[key].length == 1) {
                            return false;
                        }
                    }
                    return true;
                } else if(oitem.Children) {
                    for (const child of oitem.Children) {
                        if(!child.Checked) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            },
            isDisableSelect(oitem){
                let isDisabled = false;
                if(oitem.NodeType == NodeType.PhysicalRootLocation)
                {
                    isDisabled = true;
                }
                return isDisabled;
            },
            sortChild(a, b) {
                if (a.Name == b.Name) {
                    return 0;
                } else if (a.Name > b.Name) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let poItem = parentItem.origin;
                mainComponent.loadChildren(poItem, res => {
                    if (res) {
                        try {
                            //将当前页已加载过字节点的Children添加到OtherChildren里
                            res.OtherChildren = poItem.OtherChildren || {};
                            for (const child of poItem.Children) {
                                if(res.ChildStates[child.Id] && (child.Children && child.Children.length > 0)) {
                                    res.OtherChildren[child.Id] = child;
                                }
                            }
                            
                            for (const child of res.Children) {
                                mainComponent.addToCache(child);
                                let arr = poItem.ChildStates[child.Id];
                                child.Checked = arr && arr.length > 1;  //为新Load的Child设置Checked状态
                                let temp = res.OtherChildren[child.Id];
                                if(temp) {  //如果OtherChildren中存在此Child，表示Child被加载过，将之前的Child信息同步到新Child
                                    Object.assign(child, temp);
                                    delete res.OtherChildren[child.Id];
                                }
                            }

                            //删除OtherChildren中已经不存在的子节点
                            for (const id in res.OtherChildren) {
                                if(!res.ChildStates[id]) {
                                    delete res.OtherChildren[id];
                                }
                            }

                            //同步ChildStates中所有子节点的勾选状态
                            for (const key in res.ChildStates) {
                                let arr = poItem.ChildStates[key];
                                if(arr && arr.length > 1) {
                                    res.ChildStates[key].push(1);
                                }
                            }

                            funcSuccess(res.Children, res);
                        } catch (error) {
                            funcFail && funcFail();
                        }
                    } else {
                        poItem.HasChildren = false;
                        poItem.Children = [];
                        poItem.ChildrenCount = 0;
                    }
                    
                });
            },
            onTreeChanged() {
                if (mainComponent.props.onTreeChanged) {
                    mainComponent.props.onTreeChanged();
                }
            },
            onNodeSelectedChange() {
                if (mainComponent.props.onNodeSelectedChange) {
                    mainComponent.props.onNodeSelectedChange();
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
            this.processSearchData(data);
        } else {
            this.treeCache = {};
            this.treeData = data;
            this.addToCache(data);
        }
        this.setState({ items: [data] });
    }

    addToCache(oitem) {
        this.removeChildrenCache(oitem);
        this.treeCache[oitem.Id] = oitem;
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
                pItem.Children = [];
            }
            if(!pItem.ChildStates) {
                pItem.ChildStates = {};
                pItem.ChildrenCount = 0;
            } 
            if(pItem.Children.length < 10) {
                pItem.Children.push(oitem);
            } else {
                if(!pItem.OtherChildren) { pItem.OtherChildren = {}; }
                pItem.OtherChildren[oitem.Id] = oitem;
            }

            let arr = pItem.ChildStates[oitem.Id];
            if(!arr) {
                arr = pItem.ChildStates[oitem.Id] = [pItem.ChildrenCount];
                pItem.ChildrenCount += 1;
                if(pItem.Checked || oitem.Checked) {
                    arr.push(1);
                }
            } else {
                if(pItem.Checked || oitem.Checked) {
                    pItem.ChildStates[oitem.Id] = [arr[0], 1];
                } else {
                    pItem.ChildStates[oitem.Id] = [arr[0]];
                }
            }

            this.addToCache(oitem);
            this.setChildStates(oitem);
        }
    }
    //for search node item
    setChildStates(oitem) {
        if(oitem.Children) {
            let idx = 0;
            oitem.ChildStates = {};
            for (const child of oitem.Children) {
                oitem.ChildStates[child.Id] = (oitem.Checked || child.Checked) ? [idx, 1] : [idx];
                idx++;
                this.setChildStates(child);
            }
        }
    }

    processSearchData(oitem, checked) {
        let cacheItem = this.treeCache[oitem.Id];
        if(cacheItem) {
            checked = checked || cacheItem.Checked;
        } 
        oitem.Checked = checked;

        if(oitem.Children && (cacheItem || checked)) {
            for (const child of oitem.Children) {
                this.processSearchData(child, checked);
            }
        }
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
        this.syncTreeData(this.searchTreeData);
        this.setState({items: [this.treeData]});
    }

    syncTreeData(oitem) {
        let cacheItem = this.treeCache[oitem.Id];
        if(cacheItem) {
            cacheItem.Checked = oitem.Checked;
            cacheItem.Expanded = oitem.Expanded;

            if (oitem.Children && oitem.Children.length > 0) {
                for (const sub of oitem.Children) {
                    this.syncTreeData(sub);
                }
            }
        } else {
            this.appendToParent(oitem);
        }
    }

    recursiveTreeItem(oitem, results) {
        if(!results.selected && oitem.ChildStates) {
            for (const id in oitem.ChildStates) {
                if(oitem.ChildStates[id].length > 1) {
                    results.selected = true;
                    break;
                }
            }
        }

        if(oitem.Children) {
            for (const child of oitem.Children) {
                this.recursiveTreeItem(child, results);
            }
        }
        
        if(oitem.OtherChildren) {
            for (const id in oitem.OtherChildren) {
                this.recursiveTreeItem(oitem.OtherChildren[id], results);
            }
        }
    }
    
    //public functions:
    getTreeData = () => {
        if(this.treeContext.searchKey) {
            this.syncTreeData(this.searchTreeData);
        }
        
        var treeItem = RM.SimplifyObject(this.treeData, ["Children", "OtherChildren"]);
        let results = {items: treeItem, selected: false};
        this.recursiveTreeItem(treeItem, results);
        return results;
    };

    render() {
        return (
            <div>
                <$g.TreeView
                    classicMode
                    items={this.state.items}
                    treeContext={this.treeContext}
                />
            </div>
        );
    }
}

ReportLocationTree.propTypes = {
    data: PropTypes.object,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func
};
ReportLocationTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null
};

export default ReportLocationTree;
