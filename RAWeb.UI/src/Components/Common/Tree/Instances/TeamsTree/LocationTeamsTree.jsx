import { Component } from "react";
import PropTypes from 'prop-types';
import { NodeLevel } from "../../../../../Constants/DAEnums";
import { TreeType, SourceFlags} from "../../../../../Constants/Constants";
import { bindEvents } from "../../../../../Utilities/CommonUtil";
import { TabIndex } from "../../../../BCM/ContentRepositoryManagement/CRMForSPO";
import TeamsNodeContent from "../../NodeContents/RC/TeamsNodeContent";

export default class LocationTeamsTree extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "");

        this.isFilterTree = props.treeType == TreeType.Filter;
        this.designLists = [];
        this.virtualLevels = [NodeLevel.RMIncludeNew, NodeLevel.RMSelectAll];
        this.supportIncludeNewLevels = this.isFilterTree || this.props.treeSource == SourceFlags.SPLocal ? [] : [NodeLevel.WebApplication, NodeLevel.Office365GroupEntire, NodeLevel.Sites, NodeLevel.Lists];
        this.onlySupportSelectAllLevels = [];
        this.disableSelectNodeLevels = [NodeLevel.Farm, NodeLevel.WebApplication, NodeLevel.SiteCollections, NodeLevel.Sites, NodeLevel.Lists];
        if (this.isFilterTree) {
            this.disableSelectNodeLevels = [NodeLevel.Farm, NodeLevel.SiteCollections];
        }
        this.treeCache = [];

        this.initDesignLists();
        this.initTreeContext();

        this.state = {
            items: []
        };
    }

    componentDidMount() {
        if (this.props.data && this.props.data.length > 0) {
            this.setTreeData(this.props.data);
        } else {
            this.initTreeData();
        }
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
        if (nextProps.data != this.props.data) {
            this.setTreeData(nextProps.data);
        }
    }

    initDesignLists() {
        $.ajax({
            type: "POST",
            url: "/api/DAMApi/GetSPDesignLists",
            data: [],
            async: true,
            success: (data) => {
                this.designLists = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
            },
            error: (msg) => {},
            dataType: "json"
        });
    }

    initTreeContext() {
        const mainComponent = this;
        this.treeContext = {
            nodeContentComponent: TeamsNodeContent,
            multiSelection: true,
            readonly: mainComponent.props.readonly,
            transToTreeNodeObject(originItem) {
                const children = this.getViewChildren(originItem);
                let isLeafNode = originItem.Level == (mainComponent.isFilterTree ? NodeLevel.SiteCollection : NodeLevel.List);
                if (mainComponent.props.treeType == TreeType.ActionReport) {
                    isLeafNode = originItem.Level == NodeLevel.SiteCollection;
                }
                const loaded = this.isLoaded(originItem);
                const enableIncludeNew = mainComponent.supportIncludeNewLevels.indexOf(originItem.Level) >= 0;
                const checked = originItem.CheckNumber == 1 && (!enableIncludeNew || !loaded || originItem.IncludeNew == 1);
                return {
                    origin: originItem,
                    nodeKey: originItem.Level === NodeLevel.Office365GroupEntire ? originItem.TeamsId : originItem.Id,
                    nodeType: originItem.Level,
                    text: this.getText(originItem),
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(originItem.Level) >= 0,
                    isLeafNode,
                    enableIncludeNew,
                    onlySupportSelectAll: mainComponent.onlySupportSelectAllLevels.indexOf(originItem.Level) >= 0,
                    checked: checked,
                    includeNew: checked || originItem.IncludeNew == 1,
                    selectAll: checked || originItem.CheckNumber == 1,
                    loaded,
                    expanded: originItem.Expanded,
                    items: children,
                    itemsCount: children.length,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerIndex: (originItem.PageIndex || 1) - 1, //start index: 1 
                    pagerSize: 10,
                    enableContextMenu: !isLeafNode,
                    treeSource: mainComponent.props.treeSource,
                };
            },
            updateOriginObject(item) {
                const originItem = item.origin;
                if (item.enableIncludeNew) {
                    originItem.CheckNumber = item.selectAll ? 1 : 0;
                    originItem.IncludeNew = item.includeNew ? 1 : 0;
                } else {
                    originItem.CheckNumber = item.checked ? 1 : 0;
                }
                originItem.Loaded = item.loaded;
                originItem.Expanded = item.expanded;
                originItem.PageIndex = item.pagerIndex;
                originItem.PageSize = item.pagerSize;
            },
            getAllChildren(originItem) {
                const children = [];
                if (mainComponent.virtualLevels.indexOf(originItem.Level) < 0
                    && originItem.ChildrenIds && originItem.ChildrenIds.length > 0) {
                    for (let childId of originItem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child && mainComponent.virtualLevels.indexOf(child.Level) < 0) {
                            child.Parent = originItem;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            getViewChildren(originItem) {
                const children = [];
                if (mainComponent.virtualLevels.indexOf(originItem.Level) < 0) {
                    const childrenIds = this.searchKey && originItem.SearchChildrenIds
                        ? originItem.SearchChildrenIds : originItem.ChildrenIds;
                    if (childrenIds) {
                        for (let childId of childrenIds) {
                            let child = mainComponent.treeCache[childId];
                            if (child && mainComponent.virtualLevels.indexOf(child.Level) < 0) {
                                child.Parent = originItem;
                                children.push(child);
                            }
                        }
                    }
                }
                return children;
            },
            getText(originItem) {
                let text = originItem.Name;
                if (text == "." && originItem.Level == NodeLevel.Site) {
                    text = RMResx.RM_JS_DAM_RootSiteName.format(originItem.Title);
                }
                if (originItem.TeamName) {
                    text = "(" + originItem.TeamName + ") " + originItem.Name;
                }
                if (originItem.OrphanNameSuffix) {
                    text = originItem.Name + originItem.OrphanNameSuffix;
                }
                return text;
            },
            isLoaded(originItem) {
                // The RM 3.1 version does not have the Loaded property.
                // The 3.2 version adds this property, which is stored in the database to record whether the node has been "loaded" or not.
                if (originItem.Loaded === null || originItem.Loaded === undefined) {
                    return (originItem.ChildrenIds && (originItem.ChildrenIds.length > 0 || originItem.IncludeNew == 1));
                } else {
                    return originItem.Loaded;
                }
            },
            removeCache(parentOriginItem) {
                if (parentOriginItem.ChildrenIds) {
                    for (let childId of parentOriginItem.ChildrenIds) {
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
                const browseUrl = "/API/TeamsSettingApi/BrowseSampleTree";
                const parentOriginItem = parentItem.origin;
                parentOriginItem.Expanded = true;
                parentOriginItem.Loaded = true;
                const isArchiverTree = mainComponent.props.mode == TabIndex.Archive ? true : false;
                const postData = Object.assign({}, parentOriginItem, { Children: null, ChildrenIds: null }, { IsArchiverTree: isArchiverTree }, { PageSize: 2147483647 });
                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
                    currentItem = currentItem.Parent;
                }
                $$.fetch.post(browseUrl, postData).then((data)=>{
                    this.removeCache(parentOriginItem);
                    let items = $.parseJSON(data).Children;
                    items = mainComponent.filterItems(items);
                    if (items && items.length > 0) {
                        parentOriginItem.ChildrenIds = items.map(item => {
                            item.CheckNumber = parentOriginItem.CheckNumber;
                            if (mainComponent.supportIncludeNewLevels.indexOf(item.Level) != -1) {
                                item.IncludeNew = parentOriginItem.CheckNumber;
                            }
                            item.Parent = parentOriginItem;
                            mainComponent.treeCache[item.Id] = item;
                            return item.Id;
                        });
                    } else {
                        parentOriginItem.ChildrenIds = [];
                    }
                    funcSuccess(items);
                }).catch(funcFail);
                // Return children node items
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
        $.ajax({
            type: "POST",
            url: "/api/TeamsSettingApi/GetTeamsTreeInitData",
            data: [],
            async: true,
            success: (data) => {
                this.setTreeData([$.parseJSON(data)]);  // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
            },
            error: (msg) => {},
            dataType: "json"
        });
    }

    setTreeData(data) {
        this.treeCache = [];
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Farm) {
                this.rootItem = item;
            }
        });
        this.relateTreeItemChildren(this.rootItem);
        this.rootItem.Name = RMResx.RM_DAM_Teams_RootNode;
        this.setState({ items: [this.rootItem] });
    }

    relateTreeItemChildren(item) {
        if (item && item.ChildrenIds) {
            for (let childId of item.ChildrenIds) {
                let child = this.treeCache[childId];
                if (child && this.virtualLevels.indexOf(child.Level) < 0) {
                    child.Parent = item;
                    this.relateTreeItemChildren(child);
                }
            }
            delete item.SearchChildrenIds;
        }
    }

    relateTreeItemSearchChildren(item) {
        const matchChildrenIds = [];
        if (item && item.ChildrenIds) {
            for (let childId of item.ChildrenIds) {
                let child = this.treeCache[childId];
                let text = child.Name;
                if (text == "." && child.Level == NodeLevel.Site) {
                    text = child.Title;
                }
                if (text.toUpperCase().indexOf(this.treeContext.searchKey.toUpperCase()) > -1
                    || this.relateTreeItemSearchChildren(child)) {
                    matchChildrenIds.push(childId);
                }
            }
            item.SearchChildrenIds = matchChildrenIds;
        }
        return matchChildrenIds.length > 0;
    }

    filterItems(items) {
        const filteredItems = new Array();
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            // Filtering system node
            if (!item.Hidden && item.Level != NodeLevel.Apps) {
                // Filter designLists (which could be libraries) in the List and configuration files.
                let index = 0;
                let uniqueName = "";
                if (item.FullPath) {
                    index = item.FullPath.lastIndexOf("/");
                    uniqueName = item.FullPath.substr(index + 1) + item.TemplateId;
                    if (item.TemplateId == 600) { // ExternalList
                        continue;
                    }
                }

                // Filter designLists
                if (item.Level == NodeLevel.List && $.inArray(uniqueName, this.designLists) != -1) {
                    continue;
                }
                filteredItems.push(items[i]);
            }
        }
        return filteredItems;
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


    // Public function
    getTreeData() {
        let selected = false;
        const treeItems = [];
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

LocationTeamsTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    treeSource: PropTypes.number
};

LocationTeamsTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    treeSource: SourceFlags.SP
};