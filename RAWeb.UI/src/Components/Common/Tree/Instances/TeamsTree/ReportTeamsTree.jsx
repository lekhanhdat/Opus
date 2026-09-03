import { Component } from "react";
import PropTypes from 'prop-types';

import { SourceFlags, TreeType } from "../../../../../Constants/Constants";
import { NodeLevel } from "../../../../../Constants/DAEnums";
import { TabIndex } from "../../../../BCM/ContentRepositoryManagement/CRMForSPO";
import TeamsNodeContent from "../../NodeContents/RC/TeamsNodeContent";

const checkNumberType = {
    noChecked: 0,
    checked: 1,
    checkedMix: 2
};

export default class ReportTeamsTree extends Component {
    constructor(props) {
        super(props);
        this.state = {
            items: [],
        };

        this.designLists = [];
        this.treeCache = {};
        this.disableSelectNodeLevels = [NodeLevel.Farm];
        this.checkboxMixStatus = "mixed";
        this.hasMixStatusNodeLevel = [NodeLevel.WebApplication];
        this.loadNodesByRefreshAction = true;

        this.initDesignLists();
        this.initTreeContext();
    }

    componentDidMount() {
        if (this.props.data) {
            this.setTreeData([this.props.data]);
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
            url:  "/api/DAMApi/GetSPDesignLists",
            data: [],
            async: true,
            success: (data) => {
                this.designLists = data;
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
                let isLeafNode = originItem.Level == NodeLevel.List;
                if (mainComponent.props.treeType == TreeType.ActionReport) {
                    isLeafNode = originItem.Level == NodeLevel.SiteCollection;
                }
                const loaded = this.isLoaded(originItem);
                const pagerByServer = originItem.Level < NodeLevel.SiteCollection && !originItem.IsNotInitItem; // Edit: When echoing, it is paginated at the front end.
                const isHasMixedStatus = mainComponent.hasMixStatusNodeLevel.includes(originItem.Level);
                const children = originItem.Children ? mainComponent.filterItems(originItem.Children) : [];
                const pagerSize = 10;

                return {
                    origin: originItem,
                    nodeKey: originItem.Level === NodeLevel.Office365GroupEntire ? originItem.TeamsId : originItem.Id,
                    nodeType: originItem.Level,
                    text: this.getText(originItem),
                    isHasMixedStatus,
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(originItem.Level) >= 0,
                    isLeafNode,
                    enableIncludeNew: false,
                    checked: originItem.CheckNumber == checkNumberType.checked ? true : (originItem.CheckNumber == checkNumberType.checkedMix ? mainComponent.checkboxMixStatus : false),
                    loaded,
                    expanded: originItem.Expanded,
                    items: children,
                    itemsCount: pagerByServer ? originItem.ChildrenCount : (children ? children.length : 0) ,
                    hasChildren: true,
                    pagerByServer,
                    pagerIndex: !originItem.PageIndex || originItem.PageIndex * pagerSize >= children.length ? 0 : originItem.PageIndex,
                    pagerSize,
                    enableContextMenu: !isLeafNode,
                    treeSource: mainComponent.props.treeSource,
                };
            },
            isLoaded(originItem) {
                // The RM 3.1 version does not have the Loaded property.
                // The 3.2 version adds this property, which is stored in the database to record whether the node has been "loaded" or not.
                if (_.isNil(originItem.Loaded)) {
                    return originItem.ChildrenIds && originItem.ChildrenIds.length > 0;
                }
                return originItem.Loaded;
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
            setParentCheckedStatusIncludeMix(item){
                if (item.checked == mainComponent.checkboxMixStatus){
                    const childrenInCache = [];
                    for (let key in mainComponent.treeCache){
                        if (mainComponent.treeCache[key].ParentId == item.nodeKey){
                            childrenInCache.push(mainComponent.treeCache[key]);
                        }
                    }
                    const selectedChildrenCount = childrenInCache.filter((item) => { 
                        return item.CheckNumber === checkNumberType.checked;
                    }).length;
    
                    if (childrenInCache.length === selectedChildrenCount) {
                        item.checked = true;
                    } else {
                        item.checked = mainComponent.checkboxMixStatus;
                    }
                }
            },
            updateOriginObject(item) {
                const originItem = item.origin;
                originItem.Loaded = item.loaded;
                originItem.Expanded = item.expanded;
                originItem.PageIndex = item.pagerIndex;
                originItem.PageSize = item.pagerSize;
                if(mainComponent.hasMixStatusNodeLevel.includes(originItem.Level)){
                    this.setParentCheckedStatusIncludeMix(item);
                }
                switch (item.checked) {
                    case false:
                        originItem.CheckNumber = checkNumberType.noChecked;
                        break;
                    case true:
                        originItem.CheckNumber = checkNumberType.checked;
                        break;
                    case mainComponent.checkboxMixStatus:
                        originItem.CheckNumber = checkNumberType.checkedMix;
                        break;
                }
            },
            getAllChildren(originItem) {
                const children = [];
                if (originItem.ChildrenIds && originItem.ChildrenIds.length > 0) {
                    for (let childId of originItem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];

                        children.push(child);
                    }
                }
                return children;
            },
            removeCache(parentOriginItem) {
                if (parentOriginItem.ChildrenIds) {
                    for(let key in mainComponent.treeCache){
                        let item = mainComponent.treeCache[key];
                        if(item.ParentId == parentOriginItem.Id){
                            delete mainComponent.treeCache[key];
                            this.removeCache(item);
                        }
                    }
                }
            },
            getAncestorsItem(parentOriginItem, treeCache){
                if(parentOriginItem && parentOriginItem.ParentId){
                    Object.assign(treeCache[parentOriginItem.ParentId] , { Children: null, ChildrenIds: null }) ;
                    parentOriginItem.Parent = treeCache[parentOriginItem.ParentId];
                    this.getAncestorsItem(treeCache[parentOriginItem.ParentId], treeCache);
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
                parentOriginItem.PageIndex = parentItem.pagerIndex;
                if (parentOriginItem.Level === NodeLevel.Farm && !this.loadNodesByRefreshAction) {
                    parentOriginItem.CheckNumber = parentOriginItem.Children?.length === parentOriginItem.ChildrenCount ? 1 : 0;
                }

                // Refresh logic
                if (this.loadNodesByRefreshAction){
                    const parentOriginItem = parentItem.origin;
                    if(parentOriginItem.CheckNumber == checkNumberType.checkedMix){
                        parentOriginItem.CheckNumber = checkNumberType.noChecked;
                        mainComponent.treeCache[parentOriginItem.Id].CheckNumber = checkNumberType.noChecked;
                        parentItem.checked = false;
                    }
                    this.removeCache(parentOriginItem);
                    this.loadNodesByRefreshAction = false;
                }
                if (!parentOriginItem.Parent && parentOriginItem.ParentId){
                    let treeCache = RM.deepcopy(mainComponent.treeCache);
                    let parentItem = RM.deepcopy(mainComponent.treeCache[parentOriginItem.ParentId]);
                    this.getAncestorsItem(parentItem, treeCache);
                    parentOriginItem.Parent = Object.assign({}, parentItem , { Children: null, ChildrenIds: null });
                }
                const isArchiverTree = mainComponent.props.mode == TabIndex.Archive ? true : false;
                const postData = Object.assign({}, parentOriginItem, { Children: null, ChildrenIds: null }, { IsArchiverTree: isArchiverTree });
                $$.fetch.post(browseUrl, postData).then((data) => {
                    let items = data.Children || [];
                    items = mainComponent.filterItems(items);
                    for (let item of items) {
                        if (mainComponent.treeCache[item.Id]){
                            Object.assign(item, mainComponent.treeCache[item.Id]);
                        }
                    }
                    if (items && items.length > 0) {
                        data.ChildrenIds = items.map((item) => {
                            if (!mainComponent.treeCache[item.Id]) {
                                item.CheckNumber = parentOriginItem.CheckNumber == checkNumberType.checkedMix ? checkNumberType.checked : parentOriginItem.CheckNumber;
                            }
                            item.Parent = Object.assign({}, parentOriginItem, { Children: null, ChildrenIds: null });
                            item.ParentId = parentOriginItem.Id;
                            mainComponent.treeCache[item.Id] = item;  
                            return item.Id;
                        });
                    } else {
                        data.ChildrenIds = [];
                    }
                    delete data.CheckNumber;
                    parentOriginItem.IsNotInitItem = false;
                    funcSuccess(items, data);
                }).catch(funcFail);
                return [];
            },
            onTreeChanged() {
                if (mainComponent.props.onTreeChanged) {
                    mainComponent.props.onTreeChanged();
                }
            },
            onNodeSelectedChange(item) {
                if (mainComponent.props.onNodeSelectedChange) {
                    mainComponent.props.onNodeSelectedChange();
                }
                if (mainComponent.treeCache && mainComponent.hasMixStatusNodeLevel.includes(item.origin.Level)) {
                    for (let key in mainComponent.treeCache) {
                        if (mainComponent.treeCache[key].ParentId == item.nodeKey) {
                            mainComponent.treeCache[key].CheckNumber = item.origin.CheckNumber;
                        }
                    }
                }
            },
            onNodeRefreshAction(){
                this.loadNodesByRefreshAction = true;
            },
        }
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

    setTreeCacheData(items) {
        for (let item of items) {
            item.IsNotInitItem = true;
            item.PageIndex = 0;
            this.treeCache[item.Id] = item;
            if (item.Children) {
                this.setTreeCacheData(item.Children);
            }
        }
    }

    setTreeData(data) {
        this.treeCache = {};
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Farm) {
                this.rootItem = item;
            }
        });
        this.rootItem.Name = RMResx.RM_DAM_Teams_RootNode;
        this.setTreeCacheData(data);
        this.setState({ items: data });
    }

    initTreeData() {
        $.ajax({
            type: "POST",
            url: "/api/TeamsSettingApi/GetTeamsTreeInitData",
            data: [],
            async: true,
            success: (data) => {
                this.setTreeData([data]);
            },
            error: (msg) => {},
            dataType: "json"
        });
    }

    relateTreeItemSearchChildren(item) {
        const matchChildren = [];
        if (item && item.Children) {
            for (let child of item.Children) {
                let text = child.Name;
                if (text == "." && child.Level == NodeLevel.Site) {
                    text = child.Title;
                }
                if (text.toUpperCase().indexOf(this.treeContext.searchKey.toUpperCase()) > -1
                    || this.relateTreeItemSearchChildren(child)) {
                    matchChildren.push(child);
                }
            }
            item.Children = matchChildren;
        }
        return matchChildren.length > 0;
    }

    relateTreeItemChildren(item) {
        const children = [];
        if (item && item.ChildrenIds) {
            for (let childId of item.ChildrenIds) {
                let child = this.treeCache[childId];
                if (child) {
                    children.push(child);
                    if(child.ChildrenIds && child.ChildrenIds.length > 0){
                        this.relateTreeItemChildren(child);
                    }
                }
            }
        }
        item.Children = this.filterItems(children);
    }

    search(keywords) {
        this.treeContext.searchKey = keywords;
        if (keywords && keywords.length > 0) {
            this.relateTreeItemSearchChildren(this.rootItem);
        } else {
            this.relateTreeItemChildren(this.rootItem);
        }
        this.setState({ items: [this.rootItem] });
    }

    getIsCheckedNode() {
        let hasSelectedNode = false;
        for (let itemId in this.treeCache) {
            let item = this.treeCache[itemId];
            if (item) {
                if (item.CheckNumber && (item.CheckNumber != checkNumberType.noChecked)) {
                    hasSelectedNode = true;
                    break;
                }
            }
        }
        return hasSelectedNode;
    }

    getTreeData() {
        let treeItem = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                if (this.treeCache[itemId].Level == NodeLevel.Farm) {
                    treeItem = RM.deepcopy(this.treeCache)[itemId];
                }
            }
        }
        this.setTreeParam(treeItem);
        return { items: treeItem, selected: this.getIsCheckedNode() };
    }

    hasSelectedChildren(children){
        if (children && children.length > 0) {
            for (let child of children) {
                if (child.CheckNumber == checkNumberType.checked 
                    || this.hasSelectedChildren(child.Children)) {
                    return true;
                }
            }
        }
    }

    setTreeParam(treeItem, allItems, key) {
        const treeCacheList = Object.values(RM.deepcopy(this.treeCache));
        treeItem.Children = treeCacheList.filter((item) => { 
            return item.ParentId == treeItem.Id; 
        });
        if (treeItem.Level == NodeLevel.WebApplication) {
            const hasSelectedChildren = this.hasSelectedChildren(treeItem.Children);
            treeItem.CheckNumber = treeItem.CheckNumber || 0;
            switch (treeItem.CheckNumber) {
                case checkNumberType.noChecked:
                    if (hasSelectedChildren) {
                        treeItem.Children = treeItem.Children.filter((item) => {
                            return item.CheckNumber == checkNumberType.checked || this.hasSelectedChildren(item.Children); 
                        });
                    } else {
                        delete allItems[key];
                    }
                    break;
                case checkNumberType.checked:
                    treeItem.Children = null;
                    break;
                case checkNumberType.checkedMix:
                    treeItem.Children = treeItem.Children.filter((item) => {
                        return !item.CheckNumber || (item.CheckNumber == checkNumberType.noChecked); 
                    });
                    break;
                default:
            }
        }
        if (treeItem.Children) {
            treeItem.ChildrenIds = treeItem.Children.map((item) => item.Id);
            for (let key in treeItem.Children) {
                this.setTreeParam(treeItem.Children[key], treeItem.Children, key);
                treeItem.Children[key] = RM.SimplifyObject(
                    treeItem.Children[key], 
                    null, 
                    ["Parent", "IsNotInitItem"]
                );
            }
            treeItem.Children = treeItem.Children.filter((item) => !!item.Id);
        }
    }

    render() {
        return (
            <div className="ra-report-sptree">
                <$g.TreeView
                    classicMode
                    items={this.state.items}
                    treeContext={this.treeContext}
                />
            </div>
        );
    }
}

ReportTeamsTree.propTypes = {
    data: PropTypes.object,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    treeSource: PropTypes.number
};

ReportTeamsTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    treeSource: SourceFlags.Teams
};