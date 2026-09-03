import {Component} from "react";
import PropTypes from 'prop-types';
import {NodeLevel, NodeIconClass} from "../../../../../Constants/DAEnums";
import NodeContent from "../../NodeContents/RC/TermNodeContent";

export default class EXOCRMTree extends Component {
    constructor(props) {
        super(props);

        this.rootItem = null;
        this.disableSelectNodeLevels = [NodeLevel.ExchangeOnlineFarm];
        this.treeCache = {};
        
        this.initTreeContext();

        this.state = {
            items: []
        };
    }

    componentDidMount() {
        this.initTreeData();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
    }

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            treeType: "CRM",
            nodeContentComponent: NodeContent,
            singleSelection: true,
            showrRightArrow: true,
            transToTreeNodeObject(oitem) {
                let children = this.getViewChildren(oitem);
                let isLeaf = oitem.Level == NodeLevel.ExchangeOnlineMailbox;
                let loaded = this.isLoaded(oitem);
                let checked = oitem.CheckNumber == 1 && !loaded;
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    iconClass: this.getIconClass(oitem),
                    text: oitem.DisplayName,
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(oitem.Level) >= 0,
                    isLeafNode: isLeaf,
                    checked: checked,
                    loaded: loaded,
                    expanded: oitem.Expanded,
                    enableContextMenu: !isLeaf,
                    items: children,
                    itemsCount: children.length,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerIndex: (oitem.PageIndex || 1) - 1, //start index: 1 
                    pagerSize: 15,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.CheckNumber = item.checked ? 1 : 0;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PageIndex = item.pagerIndex + 1;
                oitem.PageSize = item.pagerSize;
            },
            getIconClass(oitem) {
                let iconClass = "ra-tree-icon ";
                switch (oitem.Level) {
                    case NodeLevel.ExchangeOnlineFarm:
                        iconClass += NodeIconClass.EXOFarm;
                        break;
                    case NodeLevel.ExchangeOnlineMailboxGroup: 
                        iconClass += NodeIconClass.EXOMailBoxGroup;
                        break;
                    case NodeLevel.ExchangeOnlineMailbox:
                        iconClass += NodeIconClass.EXOMailBox;
                        break;
                    default:
                        iconClass += NodeIconClass.TempIcon;
                        break;
                }
                return `${iconClass}${mainComponent.getIconStatus(oitem.IconStatus)}`;
            },
            getAllChildren(oitem) {
                let children = [];
                if (oitem.ChildrenIds && oitem.ChildrenIds.length > 0) {
                    for (let childId of oitem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];
                        child.Parent = oitem;
                        children.push(child);
                    }
                }
                return children;
            },
            getViewChildren(oitem) {
                let children = [];
                let childrenIds = this.searchKey && oitem.SearchChildrenIds
                    ? oitem.SearchChildrenIds : oitem.ChildrenIds;
                if (childrenIds) {
                    for (let childId of childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        child.Parent = oitem;
                        children.push(child);
                    }
                }
                return children;
            },
            isLoaded(oitem) {
                if (oitem.Loaded === null || oitem.Loaded === undefined) {
                    return oitem.ChildrenIds && oitem.ChildrenIds.length > 0;
                } else {
                    return oitem.Loaded;
                }
            },
            removeCache(poItem) {
                if (poItem.ChildrenIds) {
                    for (let childId of poItem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            delete mainComponent.treeCache[childId];
                            this.removeCache(child);
                        }
                    }
                }
            },
            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let poItem = parentItem.origin;
                poItem.Expanded = true;
                poItem.Loaded = true;
                poItem.PageIndex = 1;
                let option = {
                    url: "/api/EXOSettingApi/BrowseExchange",
                    data: poItem
                };
                fetchUtility(option).then((data) => {
                    this.removeCache(poItem);
                    let items = $.parseJSON(data);
                    if (items && items.length > 0) {
                        poItem.ChildrenIds = items.map(item => {
                            item.Parent = poItem;
                            mainComponent.treeCache[item.Id] = item;
                            return item.Id;
                        });
                    } else {
                        poItem.ChildrenIds = [];
                    }
                    
                    funcSuccess(items);
                }).catch((e) => {
                    funcFail(e);
                });
                return [];
            },
            onNodeSelected(item) {
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },
            onNodeRefresh(){
                let treeCacheDataList = mainComponent.getTreeData();
                let exitSelectedNode = false;
                for(let item of treeCacheDataList){
                    if(item.CheckNumber == 1){
                        exitSelectedNode = true;
                        break;
                    }
                }
                let funcChange = mainComponent.props.onNodeRefresh;
                if(funcChange){
                    funcChange(exitSelectedNode);
                }
            }
        };
    }

    initTreeData() {
        let option = {
            url: "/api/EXOSettingApi/GetEXORootNode",
            method: "get"
        };
        fetchUtility(option).then((res) => {
            this.setTreeData([res]);
        }).catch((e) => {
        });
    }

    getIconStatus(iconStatus) {
        switch (iconStatus) {
            case 1:
                return "-inherit-b";
            case 2:
                return "-unique-c";
            default:
                return "";
        }
    }

    getTreeData() {
        let treeData = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                let newItem = Object.assign({}, this.treeCache[itemId]);
                newItem.Parent = null;
                treeData.push(newItem);
            }
        }
        return treeData;
    }

    setTreeData(data) {
        this.treeCache = {};
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.ExchangeOnlineFarm) {
                this.rootItem = item;
            }
        });
        this.relateTreeItemChildren(this.rootItem);
        this.rootItem.Name = RMResx.RM_JS_SPS_EXO_RootNode;
        this.setState({items: [this.rootItem]});
    }

    relateTreeItemChildren(item) {
        if (item && item.ChildrenIds) {
            for (let childId of item.ChildrenIds) {
                let child = this.treeCache[childId];
                if (child) {
                    child.Parent = item;
                    this.relateTreeItemChildren(child);
                }
            }
            delete item.SearchChildrenIds;
        }
    }

    relateTreeItemSearchChildren(item) {
        let matchChildrenIds = [];
        if (item && item.ChildrenIds) {
            for (let childId of item.ChildrenIds) {
                let child = this.treeCache[childId];
                let text = child.Name;
                if (text.toUpperCase().indexOf(this.treeContext.searchKey.toUpperCase()) > -1
                    || this.relateTreeItemSearchChildren(child)) {
                    matchChildrenIds.push(childId);
                }
            }
            item.SearchChildrenIds = matchChildrenIds;
        }
        return matchChildrenIds.length > 0;
    }

    search(keywords) {
        keywords = keywords.trim();
        this.treeContext.searchKey = keywords;
        if (keywords && keywords.length > 0) {
            this.relateTreeItemSearchChildren(this.rootItem);
        } else {
            this.relateTreeItemChildren(this.rootItem);
        }
        this.setState({items: [this.rootItem]});
    }

    refreshSelectedNode = (updateProps, isReload) => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
                if (updateProps) {
                    if(isReload && selNode.props.item.origin.Level != NodeLevel.ExchangeOnlineMailbox){
                        selNode.props.item.loaded = false;
                        selNode.reload(0);
                    }
                    Object.assign(selNode.props.item.origin, updateProps);
                    selNode.props.item.iconClass = this.treeContext.getIconClass(selNode.props.item.origin);
                    selNode.reRender();
                }
            }
        }
    };

    render() {
        return <div className="ra-report-sptree">
            <$g.TreeView
                classicMode
                items={this.state.items}
                treeContext={this.treeContext}
            />
        </div>;
    }
}

EXOCRMTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
};
EXOCRMTree.defaultProps = {
    data: null,
    searchKey: null,
};