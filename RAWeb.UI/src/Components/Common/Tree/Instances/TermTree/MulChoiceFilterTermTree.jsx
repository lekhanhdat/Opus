import { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNodeContent from '../../NodeContents/RC/TermNodeContent';

class TreeReqObj {
    constructor(nodeId, nodeType, pageIndex, pageSize) {
        this.NodeId = nodeId;
        this.NodeType = nodeType;
        this.PageIndex = !pageIndex ? 1 : (pageIndex+1);
        this.PageSize = !pageSize ? 20 : 0;
    }
}

const NodeType = {
    Root: "Root",
    TermGroup: "TermGroup",
    TermSet: "TermSet",
    Term: "Term",
};

//由于termgroup,termset,term 的真实ID可能相同.所以保存TermTree时，TermSet Id存成：-Id，TermGroup Id存成：TermGourpIdRanges + Id
const TermGourpIdRanges = -1000000;

class MulChoiceFilterTermTree extends Component {
    constructor(props) {
        super(props);

        this.treeCache = {};
        this.initTreeContext();

        this.state = {
            treeData: []
        };
    }

    componentDidMount() {
        this.isAllowSearchChange = false;
        if(this.props.data) {
            this.setTreeData(this.props.data);
        } else {
            this.initTreeData();
        }
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if ((nextProps.searchKey != this.props.searchKey) && this.isAllowSearchChange) {
            if(nextProps.searchKey){
                this.initTreeData(()=>{
                    this.search(nextProps.searchKey);
                });
            }else{
                this.initTreeData();
            }
        }
    }

    componentDidUpdate(){
        this.isAllowSearchChange = true;
    }

    initTreeContext() {
        let self = this;
        this.treeContext = {
            multiSelection: true,
            readonly: self.props.readonly,
            allowSelectedWithoutChildren: true,
            nodeContentComponent: TreeNodeContent,
            transToTreeNodeObject(oitem) {
                let loaded = !!oitem.subTerms && oitem.subTerms.length > 0;
                let itemsCount = (!oitem.subTerms ? 0 : oitem.subTerms.length) || oitem.subTermCount || 0;
                return {
                    origin: oitem,
                    nodeKey: oitem.Type == NodeType.TermGroup ? oitem.UniqueId : self.getNodeId(oitem, true),
                    nodeType: oitem.Type,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: (oitem.Type == NodeType.TermGroup || oitem.Type == NodeType.Root),
                    checked: oitem.IsChecked,
                    expanded: loaded,
                    loaded: loaded,
                    items: oitem.subTerms,
                    hasChildren: itemsCount > 0,
                    pagerByServer: false,
                    itemsCount: itemsCount,
                    pagerIndex: !oitem.pageIndex ? 0 : oitem.pageIndex,
                    pagerSize: 20,
                    enableContextMenu: true
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.IsChecked = !item.disableSelect && item.checked;
                oitem.expand = item.expanded;
                oitem.pageIndex = item.pagerIndex;
                oitem.PageSize = item.pagerSize;
            },
            getAllChildren(oitem) {
                let children = [];
                let pId = oitem.UniqueId;
                for (const nodeId in this.treeCache) {
                    let child = this.treeCache[nodeId];
                    if (child && pId === child.ParentId) {
                        children.push(child);
                    }
                }
                if(oitem.subTerms) {
                    return children.concat(oitem.subTerms);
                } else {
                    return children;
                }
            },
            getNodeIconClass(oitem) {
                switch (oitem.Type) {
                    case 'Root':
                    case 'TermGroup':
                        return 'ra-tree-icon fia-term-group';
                    case 'TermSet':
                        return 'ra-tree-icon fia-term-set';
                    case 'Term': {
                        let iconclass = 'ra-tree-icon fia-term';
                        if (oitem) {
                            if (oitem.IsDeprecated) {
                                iconclass += "-retired-b";
                            } else if (oitem.IsExpired) {
                                iconclass += "-retired-b";
                            }
                        }
                        return iconclass;
                    }
                    default:
                        return '';
                }
            },

            sortChild(a, b) {
                if (a.Type == NodeType.TermGroup || a.Name == b.Name) {
                    return 0;
                } else if (a.Name.toLowerCase() > b.Name.toLowerCase()) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let option = {
                    url: "/api/TermManagementApi/GetAllChildren",
                    data: new TreeReqObj(
                        parentItem.nodeKey,
                        parentItem.nodeType,
                        parentItem.pagerIndex
                    )
                };
                fetchUtility(option).then((res) => {
                    let oitems = $.parseJSON(res);
                    oitems.filter(oitem => {
                        oitem.IsChecked = parentItem.checked;
                        return true;
                    });
                    parentItem.origin.subTerms = oitems;
                    parentItem.origin.subTermCount = oitems.length;
                    parentItem.hasChildren = oitems.length > 0;
                    self.addToCache(parentItem.origin);
                    funcSuccess(oitems);
                }).catch((e) => {
                    funcFail(e);
                });
            },
            onNodeSelected(item) {
                let funcChange = self.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange([item.origin]);
                }
            },
            onNodeSelectedChange(selectedItems) {
                // let funcChange = self.props.onSelectedNodeChanged;
                // if (funcChange) {
                //     funcChange(selectedItems.map(item => {
                //         return item.origin;
                //     }));
                // }
            },
            onTreeChanged() {
                if(self.props.onTreeChanged) {
                    self.props.onTreeChanged();
                }
            }
        };
    }

    initTreeData(callback) {
        let option = {
            url: `/api/TermManagementApi/GetGroups`,
            method: "get"
        };
        fetchUtility(option).then((res) => {
            this.treeContext.searchKey = "";
            let data = $.parseJSON(res);
            data.forEach(oitem => {
                oitem.expand = true;
            });
            this.setTreeData(data, false, callback);

        }).catch((e) => {

        });
    }

    setTreeData(data, isSearch, callback) {
        let root = {
            Name: RMResx.RM_JS_TM_RootTerms,
            Type: NodeType.Root,
            Id: NodeType.Root,
            UniqueId: NodeType.Root,
            expand: true,
            subTerms: data
        };
        if(isSearch) {
            this.searchTreeData = root;
            this.processSearchData(root);
        } else {
            this.treeCache = {};
            this.treeData = root;
            this.addToCache(root);
        }

        root.subTermCount = root.subTerms.length;
        this.setState({treeData: [root]},()=>{
            if(callback){
                callback();
            }
        });
    }

    //data:数据, real:是否取真实ID
    getNodeId(oitem, real) {
        if (real) {
            switch (oitem.Type) {
                case NodeType.TermGroup:
                    return oitem.Id > 0 ? oitem.Id : oitem.Id - TermGourpIdRanges;
                case NodeType.TermSet:
                    return oitem.Id > 0 ? oitem.Id : -oitem.Id;
                case NodeType.Term:
                default:
                    return oitem.Id;
            }
        } else {
            switch (oitem.Type) {
                case NodeType.TermGroup:
                    return oitem.Id > 0 ? TermGourpIdRanges + oitem.Id : oitem.Id;
                case NodeType.TermSet:
                    return oitem.Id > 0 ? -oitem.Id : oitem.Id;
                case NodeType.Term:
                default:
                    return oitem.Id;
            }
        }
    }

    addToCache(oitem, appendToParent) {
        let containChildren = oitem.subTerms && oitem.subTerms.length > 0;
        // if(containChildren || oitem.subTermCount > 0 || (!oitem.IsDeprecated && !oitem.IsExpired)) {
        this.removeChildrenCache(oitem);
        this.treeCache[oitem.UniqueId] = oitem;
        if(appendToParent) {
            this.appendToParent(oitem);
        }
        if(containChildren) {
            for (const sub of oitem.subTerms) {
                sub.ParentId = oitem.UniqueId;
                this.addToCache(sub);
            }
        }
        // }
    }

    removeChildrenCache(oitem) {
        let tempItem = this.treeCache[oitem.UniqueId];
        if(tempItem) {
            while(tempItem.subTerms && tempItem.subTerms > 0) {
                for (const sub of tempItem.subTerms) {
                    this.removeChildrenCache(sub);
                    delete this.treeCache[sub.UniqueId];
                }
            }
        }
    }

    appendToParent(oitem) {
        let pItem = this.treeCache[oitem.ParentId];
        if(pItem) {
            if(!pItem.subTerms) {
                pItem.subTerms = [];
            }
            pItem.subTerms.push(oitem);
        }
    }

    processSearchData(oitem) {
        let cacheItem = this.treeCache[oitem.UniqueId];
        if(cacheItem) {
            oitem.IsChecked = cacheItem.IsChecked;
        } else {
            this.addToCache(oitem, true);
        }

        oitem.expand = true;
        if (oitem.subTerms && oitem.subTerms.length > 0) {
            for (const sub of oitem.subTerms) {
                sub.ParentId = oitem.UniqueId;
                this.processSearchData(sub);
            }
        }
    }

    search(key) {
        key = !key ? "" : key.trim();
        if (key.length > 0) {
            let option = {
                url: `/api/TermManagementApi/Search?termLabel=${this.replaceSpecialCharacters(key)}&termGroupId=00000000-0000-0000-0000-000000000000&withRuleName=false`,
                method: "get"
            };
            fetchUtility(option).then((res) => {
                this.treeContext.searchKey = key;
                this.setTreeData(JSON.parse(res), true);
            }).catch((e) => {

            });
        } else {
            this.stopSearch();
        }
    }

    stopSearch() {
        this.treeContext.searchKey = "";
        this.syncTreeData(this.searchTreeData);
        this.setState({treeData: [this.treeData]});
    }

    syncTreeData(oitem) {
        let cacheItem = this.treeCache || this.treeCache[oitem.UniqueId];
        if(cacheItem) {
            cacheItem.IsChecked = oitem.IsChecked;
            cacheItem.expand = oitem.expand;
        } else {
            this.addToCache(oitem, true);
        }

        if (oitem.subTerms && oitem.subTerms.length > 0) {
            for (const sub of oitem.subTerms) {
                this.syncTreeData(sub);
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

    recursiveTreeItem(oitem, results) {
        if(oitem.subTerms) {
            for (const sub of oitem.subTerms) {
                sub.IsLeafNode = !sub.subTerms || sub.subTerms.length == 0;
                let newItem = RM.SimplifyObject(sub, null, ["subTerms"]);
                newItem.Id = this.getNodeId(newItem);
                if(newItem.IsChecked) {
                    results.selected = true;
                }
                results.items[newItem.Id] = newItem;
                this.recursiveTreeItem(sub, results);
            }
        }
    }

    //public function
    getTreeData() {
        let results = {items: {}, selected: false };
        this.recursiveTreeItem(this.treeData, results);
        return results;
    }


    render() {
        return <div>
            <$g.TreeView
                classicMode
                items={this.state.treeData}
                treeContext={this.treeContext}
            />
        </div>;
    }
}

MulChoiceFilterTermTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func
};
MulChoiceFilterTermTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null
};

export default MulChoiceFilterTermTree;