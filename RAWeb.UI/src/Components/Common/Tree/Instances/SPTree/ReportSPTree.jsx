import { Component } from "react";
import PropTypes from 'prop-types';
import { NodeLevel } from "../../../../../Constants/DAEnums";
import {JobType,TreeType, SourceFlags} from "../../../../../Constants/Constants";
import { bindEvents } from "../../../../../Utilities/CommonUtil";
import SPNodeContent from "../../NodeContents/RC/SPNodeContent";
import { TabIndex } from "../../../../BCM/ContentRepositoryManagement/CRMForSPO";

export default class ReportSPTree extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "");

        this.isFilterTree = props.treeType == TreeType.Filter;
        this.DesignLists = [];
        this.virtualLevels = [NodeLevel.RMIncludeNew, NodeLevel.RMSelectAll];
        this.supportIncludeNewLevels = this.isFilterTree || this.props.treeSource == SourceFlags.SPLocal ? [] : [NodeLevel.WebApplication, NodeLevel.Sites, NodeLevel.Lists];
        this.onlySupportSelectAllLevels = this.props.treeSource == SourceFlags.SPLocal ? [NodeLevel.WebApplication, NodeLevel.Sites, NodeLevel.Lists] : [];
        this.disableSelectNodeLevels = [NodeLevel.Farm, NodeLevel.WebApplication, NodeLevel.Sites, NodeLevel.Lists];
        if (this.isFilterTree) {
            this.disableSelectNodeLevels = [NodeLevel.Farm];
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
        let designListsUrl = "/api/DAMApi/GetSPDesignLists";
        // if (this.props.treeSource == SourceFlags.OneDrive) {
        //     designListsUrl = "/api/OneDriveSettingApi/GetSPDesignLists";
        // }
        $.ajax({
            type: "POST",
            url: designListsUrl,
            //contentType: 'application/json;charset=utf-8',
            data: [],
            async: true,
            success: (data) => {
                this.DesignLists = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            nodeContentComponent: SPNodeContent,
            multiSelection: true,
            readonly: mainComponent.props.readonly,
            transToTreeNodeObject(oitem) {
                let children = this.getViewChildren(oitem);
                let isLeaf = oitem.Level == (mainComponent.isFilterTree ? NodeLevel.SiteCollection : NodeLevel.List);
                if (mainComponent.props.treeType == TreeType.ActionReport) {
                    isLeaf = oitem.Level == NodeLevel.SiteCollection;
                }
                let loaded = this.isLoaded(oitem);
                let enableIncludeNew = mainComponent.supportIncludeNewLevels.indexOf(oitem.Level) >= 0;
                let checked = oitem.CheckNumber == 1 && (!enableIncludeNew || !loaded || oitem.IncludeNew == 1);
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    text: this.getText(oitem),
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(oitem.Level) >= 0,
                    isLeafNode: isLeaf,
                    enableIncludeNew: enableIncludeNew,
                    onlySupportSelectAll: mainComponent.onlySupportSelectAllLevels.indexOf(oitem.Level) >= 0,
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
                    treeSource: mainComponent.props.treeSource,
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
            getText(oitem) {
                let text = oitem.Name;
                if (text == "." && oitem.Level == NodeLevel.Site) {
                    text = RMResx.RM_JS_DAM_RootSiteName.format(oitem.Title);
                }
                return text;
            },
            isLoaded(oitem) {
                //RM 3.1 版本 没有Loaded属性,3.2添加该属性，该属性保存在数据库中，用于记录节点“是否加载过”
                if (oitem.Loaded === null || oitem.Loaded === undefined) {
                    return (oitem.ChildrenIds && (oitem.ChildrenIds.length > 0 || oitem.IncludeNew == 1));
                } else {
                    return oitem.Loaded;
                }
            },
            //sortChild(a, b) {
            //    if (a.Name == b.Name) {
            //        return 0;
            //    } else if (a.Name > b.Name) {
            //        return 1;
            //    } else {
            //        return -1;
            //    }
            //},
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
                let browseUrl = "/API/DAMApi/Browse";
                if (mainComponent.props.treeSource == SourceFlags.OneDrive) {
                    browseUrl = "/API/OneDriveSettingApi/BrowseOneDriveTree";
                }
                if (mainComponent.props.treeSource == SourceFlags.SPLocal) {
                    browseUrl = "/API/SPOnPremBrowse/BrowseSampleTree";
                }
                if (mainComponent.isFilterTree) {
                    browseUrl = "/api/SPSettingApi/BrowseSPAndODTree";
                }
                let poItem = parentItem.origin;
                poItem.Expanded = true;
                poItem.Loaded = true;
                poItem.PageIndex = 1;
                const isArchiverTree = mainComponent.props.mode == TabIndex.Archive ? true : false;
                let postData = Object.assign({}, poItem, { Children: null, ChildrenIds: null }, { IsArchiverTree: isArchiverTree });
                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
                    currentItem = currentItem.Parent;
                }
                $$.fetch.post(browseUrl, postData).then((data)=>{
                    this.removeCache(poItem);
                    let items = $.parseJSON(data);
                    items = mainComponent.filterItems(items);
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
                }).catch(funcFail);
                //return children node items
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
        let treeInitUrl ="/api/DAMApi/GetSPTreeInitData";
        if (this.props.treeSource == SourceFlags.SPLocal) {
            treeInitUrl = "/api/SPOnPremBrowse/GetSPTreeInitData";
        }
        $.ajax({
            type: "POST",
            url: treeInitUrl,
            //contentType: 'application/json;charset=utf-8',
            data: [],
            async: true,
            success: (data) => {
                this.setTreeData([$.parseJSON(data)]);  // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    setTreeData(data) {
        this.treeCache = [];
        let spOrOneDriveItemName = RMResx.RM_DAM_RootNode;
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Farm) {
                this.rootItem = item;
            }
        });
        if (this.props.treeSource == SourceFlags.OneDrive || this.props.treeSource == SourceFlags.SPLocal) {
            spOrOneDriveItemName = RMResx.RM_JS_SPS_OD_RootNode;
        }
        this.relateTreeItemChildren(this.rootItem);
        this.rootItem.Name = spOrOneDriveItemName;
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
        let matchChildrenIds = [];
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
        var filteredItems = new Array();
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            //过滤系统节点
            if (!item.Hidden && item.Level != NodeLevel.Apps) {
                //过滤List和配置文件中的designLists（有可能是library）
                var index = 0;
                var uniqueName = "";
                if (item.FullPath) {
                    index = item.FullPath.lastIndexOf("/");
                    uniqueName = item.FullPath.substr(index + 1) + item.TemplateId;
                    if (item.TemplateId == 600) {
                        continue;
                    }
                }
                if (item.Level == NodeLevel.List &&
                    $.inArray(uniqueName, this.DesignLists) != -1)//过滤designLists
                {
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

ReportSPTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    treeSource: PropTypes.number
};
ReportSPTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    treeSource: SourceFlags.SP
};