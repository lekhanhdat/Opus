import { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNodeContent from '../../NodeContents/RC/TermNodeContent';
import { SourceFlags } from '../../../../../Constants/Constants';
import { LicenseHelper } from '../../../../../Utilities/CommonUtil';

const isEnableJPMCFeature = LicenseHelper.EnableJPMCFileSystemFeature();
class TreeReqObj {
    constructor(nodeId, nodeType, pageIndex, pageSize, sourceFlag, containerId) {
        this.NodeId = nodeId;
        this.NodeType = nodeType;
        this.PageIndex = !pageIndex ? 1 : (pageIndex+1);
        this.PageSize = pageSize;
        this.SourceFlag = sourceFlag,
        this.ContainerId = containerId,
        this.ExcludeBuiltIn = true;
    }
}

const NodeType = {
    Root: "Root",
    TermGroup: "TermGroup",
    TermSet: "TermSet",
    Term: "Term",
};

class CRMTeamTree extends Component {
    constructor(props) {
        super(props);

        this.treeCache = {};
        this.initTreeContext();

        this.state = {
            treeData: [],
            containerLevel: 2,
        };
    }

    componentDidMount() {
        if(this.props.data && this.props.data.length > 0) {
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
        let self = this;
        let termSetMapping = {};
        this.treeContext = {
            singleSelection: true,
            spaceNoSelection: true,
            readonly: self.props.readonly,
            onNodeLevel: self.props.onNodeLevel,
            allowSelectedWithoutChildren: true,
            nodeContentComponent: TreeNodeContent,
            transToTreeNodeObject(oitem) {
                if (oitem.Type == "TermSet") {
                    termSetMapping[oitem.Id] = { UniqueId: oitem.UniqueId, Name: oitem.Name };
                }
                let loaded = !!oitem.subTerms && oitem.subTerms.length > 0;
                let itemsCount = oitem.subTermCount;
                let pagerIndex = !oitem.subTerms ? 0 : ((self.getPageIndex(oitem) == 0) ? 0 : self.getPageIndex(oitem) - 1);
                if(this.searchKey){
                    itemsCount = !oitem.subTerms ? 0 : oitem.subTerms.length;
                    pagerIndex = 0;
                }
                let disableSelectTermTypes = [NodeType.Root, NodeType.TermGroup];
                if (this.onNodeLevel == self.state.containerLevel) {
                    disableSelectTermTypes.push(NodeType.TermSet);
                }
                const isJPMCTearmNode = self.props.sourceFlag === SourceFlags.FS && isEnableJPMCFeature && oitem.Type == NodeType.Term;
                return {
                    origin: oitem,
                    treeId: `term-scope-tree`,
                    nodeKey: oitem.UniqueId,
                    nodeType: oitem.Type,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: oitem.IsDeprecated || oitem.IsExpired || disableSelectTermTypes.indexOf(oitem.Type) >= 0 || isJPMCTearmNode,
                    checked: oitem.IsChecked || oitem.UniqueId === self.selectedNodeUniqueId,
                    expanded: loaded,
                    loaded: loaded,
                    items: oitem.subTerms,
                    hasChildren: itemsCount > 0,
                    pagerByServer: !this.searchKey,
                    itemsCount: itemsCount,
                    pagerIndex: pagerIndex,
                    pagerSize: 15,
                    enableContextMenu: false
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
                let isTermGroup = parentItem.nodeType == "TermGroup";
                let paramId = isTermGroup ? parentItem.origin.UniqueId : parentItem.origin.Id;
                let option = {
                    url: "/api/BCMCommonSettingApi/GetChildrenTreeNodes",
                    data: new TreeReqObj(
                        paramId,
                        parentItem.nodeType,
                        parentItem.pagerIndex,
                        parentItem.pagerSize,
                        self.props.sourceFlag,
                        self.props.containerId
                    )
                };
                fetchUtility(option).then((res) => {
                    let oitems = $.parseJSON(res);
                    //RECO-38710: Class code tree should not be expanded term set level as default when loading children from Root node
                    if (self.props.sourceFlag === SourceFlags.FS && isEnableJPMCFeature && parentItem.nodeType === NodeType.Root) { 
                        this.removeSubTermsFromTermSet(oitems);
                    }
                    // if(self.selectedNodeUniqueId){
                    for(let item of oitems){
                        if(item.UniqueId === self.selectedNodeUniqueId){
                            item.IsChecked = true;
                            break;
                        }
                    }
                    // }
                    parentItem.origin.subTerms = oitems;
                    parentItem.origin.subTermCount = oitems.length;
                    parentItem.hasChildren = oitems.length > 0;
                    self.addToCache(parentItem.origin);
                    funcSuccess(oitems);
                }).catch((e) => {
                    funcFail(e);
                });
            },
            removeSubTermsFromTermSet(items) {
                for (let item of items) { 
                    if (item.Type === NodeType.TermGroup && item.subTerms?.length) {
                        for (let subItem of item.subTerms) { 
                            subItem.subTerms = [];
                        }
                    }
                }
            },
            onNodeSelected(item) {
                self.selectedNodeId = item.origin.Id;
                self.selectedNodeType = item.origin.Type;
                self.selectedNode = item.origin;
                self.selectedNodeUniqueId = item.origin.UniqueId;
                if (item.origin.Type == "Term") {
                    item.origin.TermSetUniqueId = termSetMapping[item.origin.TermSetId].UniqueId;
                    item.origin.TermSetName = termSetMapping[item.origin.TermSetId].Name;
                }
                // if (item.origin.Type == "TermSet") {
                //     item.origin.TermSetUniqueId = item.origin.UniqueId;
                //     item.origin.TermSetName = item.origin.Name;
                // }
                let funcChange = self.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange([item.origin]);
                }
            },
        };
    }

    initTreeData() {
        let option = {
            url: `/api/TermManagementApi/LoadGroups?containerId=${this.props.containerId}&sourceFlag=${this.props.sourceFlag}`,
            method: "get"
        };
        fetchUtility(option).then((res) => {
            this.treeContext.searchKey = "";
            let data = $.parseJSON(res);
            data.forEach(oitem => {
                oitem.expand = true;
            });
            this.setTreeData(data);

        }).catch((e) => {

        });
    }

    getPageIndex(oitem){
        let pageIndex = 0;
        if(oitem.subTerms){
            for (var i = 0; i < oitem.subTerms.length; i++) {
                if (pageIndex < oitem.subTerms[i].pageIndex) {
                    pageIndex = oitem.subTerms[i].pageIndex;
                }
            }
        }
        return pageIndex;
    }

    setTreeData(data, isSearch) {
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
        this.setState({treeData: [root]});
    }

    addToCache(oitem, appendToParent) {
        if (oitem.IsChecked) {
            this.selectedNodeId = oitem.Id;
            this.selectedNodeType = oitem.Type;
            this.selectedNodeUniqueId = oitem.UniqueId;
            this.selectedNode = oitem;
        }
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
                url: `/api/TermManagementApi/SearchForCRM?termLabel=${this.replaceSpecialCharacters(key)}&termGroupId=00000000-0000-0000-0000-000000000000&containerId=${this.props.containerId}&sourceFlag=${this.props.sourceFlag}`,
                method: "get"
            };
            fetchUtility(option).then((res) => {
                var reg1 = new RegExp("&", "ig");
                key = key.replace(reg1, "＆");
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
        this.initTreeData();
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
        var reg3 = new RegExp("#","ig");
        var reg4 = new RegExp(/\+/,"ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        str = str.replace(reg3,"%23");
        str = str.replace(reg4,"%2b");

        return str;
    }

    recursiveTreeItem(oitem, results) {
        if(oitem.subTerms) {
            for (const sub of oitem.subTerms) {
                sub.IsLeafNode = !sub.subTerms || sub.subTerms.length == 0;
                let newItem = RM.SimplifyObject(sub, null, ["subTerms"]);
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

    getSelectedTreeNode() {
        return {
            nodeId: this.selectedNodeId,
            nodeType: this.selectedNodeType,
            node: this.selectedNode
        };
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

CRMTeamTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    onNodeLevel: PropTypes.number,
    containerId: PropTypes.string,
    sourceFlag: PropTypes.number,
};
CRMTeamTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    onNodeLevel: null,
    containerId: null,
    sourceFlag: null,
};

export default CRMTeamTree;