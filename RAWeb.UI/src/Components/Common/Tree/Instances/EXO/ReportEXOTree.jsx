import { Component } from "react";
import PropTypes from 'prop-types';
import { NodeLevel, NodeIconClass } from "../../../../../Constants/DAEnums";
import { bindEvents } from "../../../../../Utilities/CommonUtil";
import NodeContent from "../../NodeContents/RC/TermNodeContent";

export default class ReportEXOTree extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "");

        this.rootItem = null;
        this.virtualLevels = [NodeLevel.RMIncludeNew, NodeLevel.RMSelectAll];
        this.supportIncludeNewLevels = [NodeLevel.ExchangeOnlineMailboxGroup];
        this.disableSelectNodeLevels = [NodeLevel.ExchangeOnlineFarm];
        this.treeCache = {};

        this.initTreeContext();

        this.state = {
            items: []
        };
    }

    componentDidMount() {
        if (this.props.data) {
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
            shadowInitialNodelevel: NodeLevel.ExchangeOnlineMailboxGroup,
            transToTreeNodeObject(oitem) {
                let children = this.getViewChildren(oitem);
                let isLeaf = oitem.Level == NodeLevel.ExchangeOnlineMailbox;
                let loaded = this.isLoaded(oitem);
                let enableIncludeNew = mainComponent.supportIncludeNewLevels.indexOf(oitem.Level) >= 0;
                let checked = oitem.CheckNumber == 1 && (!enableIncludeNew || !loaded || oitem.IncludeNew == 1);
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    iconClass: this.getIconClass(oitem),
                    text: oitem.DisplayName,
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(oitem.Level) >= 0,
                    isLeafNode: isLeaf,
                    enableIncludeNew: enableIncludeNew,
                    checked: checked,
                    includeNew: checked || oitem.IncludeNew == 1,
                    selectAll: checked || oitem.CheckNumber == 1,
                    loaded: loaded,
                    expanded: oitem.Expanded,
                    items: children,
                    itemsCount: children.length,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerIndex: (oitem.PageIndex || 1) - 1, //start index: 1 
                    pagerSize: 10,
                    enableContextMenu: !isLeaf,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                if (item.enableIncludeNew) {
                    oitem.CheckNumber = item.selectAll ? 1 : 0;
                    oitem.IncludeNew = item.includeNew ? 1 : 0;
                } else {
                    oitem.CheckNumber = item.checked ? 1 : 0;
                }
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
                return iconClass;
            },
            getAllChildren(oitem) {
                let children = [];
                if (mainComponent.virtualLevels.indexOf(oitem.Level) < 0
                    && oitem.ChildrenIds && oitem.ChildrenIds.length > 0) {
                    for (let childId of oitem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child && mainComponent.virtualLevels.indexOf(child.Level) < 0) {
                            child.Parent = oitem;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            getViewChildren(oitem) {
                let children = [];
                if (mainComponent.virtualLevels.indexOf(oitem.Level) < 0) {
                    let childrenIds = this.searchKey && oitem.SearchChildrenIds
                        ? oitem.SearchChildrenIds : oitem.ChildrenIds;
                    if (childrenIds) {
                        for (let childId of childrenIds) {
                            let child = mainComponent.treeCache[childId];
                            if (child && mainComponent.virtualLevels.indexOf(child.Level) < 0) {
                                child.Parent = oitem;
                                children.push(child);
                            }
                        }
                    }
                }
                return children;
            },
            isLoaded(oitem) {
                if (oitem.Loaded === null || oitem.Loaded === undefined) {
                    return (oitem.ChildrenIds && oitem.ChildrenIds.length > 0)
                        || (oitem.CheckNumber != 1 && oitem.IncludeNew == 1);
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
                            item.CheckNumber = poItem.CheckNumber;
                            if (mainComponent.supportIncludeNewLevels.indexOf(item.Level) != -1) {
                                item.IncludeNew = poItem.CheckNumber;
                            }
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
        let option = {
            url: "/api/EXOSettingApi/GetEXORootNode",
            method: "get"
        };
        fetchUtility(option).then((res) => {
            this.setTreeData([res]);
        }).catch((e) => {
        });
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
        this.setState({ items: [this.rootItem] });
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
        this.setState({ items: [this.rootItem] });
    }


    //public function
    getTreeData() {
        let selected = false;
        let treeItems = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                let newItem = RM.SimplifyObject(this.treeCache[itemId], null, ["Parent", "SearchChildrenIds"]);
                if (newItem.CheckNumber == 1 || newItem.IncludeNew == 1) {
                    selected = true;
                }
                treeItems.push(newItem);
            }
        }
        return { items: treeItems, selected: selected };
    }

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

ReportEXOTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func
};
ReportEXOTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null
};