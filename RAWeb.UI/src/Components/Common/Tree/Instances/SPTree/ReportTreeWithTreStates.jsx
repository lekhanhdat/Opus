 import { Component } from "react";
import PropTypes from 'prop-types';
import { NodeLevel } from "../../../../../Constants/DAEnums";
import { SourceFlags, TreeType } from "../../../../../Constants/Constants";
import SPNodeContent from "../../NodeContents/RC/SPNodeContent";
import { TabIndex } from "../../../../BCM/ContentRepositoryManagement/CRMForSPO";

const checkNumberType = {
    nochecked: 0,
    checked: 1,
    ckeckedMix: 2
};

export default class ReportSPTree extends Component {
    constructor(props) {
        super(props);
        this.state = {
            items: []
        };

        this.designLists = [];
        this.treeCache = {};
        this.disableSelectNodeLevels = [NodeLevel.Farm];
        this.checkboxMixStatus = "mixed";
        this.hasMixStatusNodeLevel = [ NodeLevel.WebApplication ];
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
            error: (msg) => {
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
                let isLeaf = oitem.Level == NodeLevel.List;
                if (mainComponent.props.treeType == TreeType.ActionReport) {
                    isLeaf = oitem.Level == NodeLevel.SiteCollection;
                }
                let loaded = this.isLoaded(oitem);
                let pagedByServer = oitem.Level < NodeLevel.SiteCollections && !oitem.IsNotInitItem; //Edit 回显时为前台分页
                let isHasMixedStatus = mainComponent.hasMixStatusNodeLevel.includes(oitem.Level);
                let children = oitem.Children ? mainComponent.filterItems(oitem.Children) : [];
                let pageSize = 10;
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    text: this.getText(oitem),
                    isHasMixedStatus: isHasMixedStatus,
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(oitem.Level) >= 0,
                    isLeafNode: isLeaf,
                    enableIncludeNew: false,
                    checked: oitem.CheckNumber == checkNumberType.checked ? true : (oitem.CheckNumber == checkNumberType.ckeckedMix ? mainComponent.checkboxMixStatus : false),
                    loaded: loaded,
                    expanded: oitem.Expanded,
                    items: children,
                    itemsCount: pagedByServer ? oitem.ChildrenCount : (children ? children.length : 0) ,
                    hasChildren: true,
                    pagerByServer: pagedByServer,
                    pagerIndex: !oitem.PageIndex || oitem.PageIndex * pageSize >= children.length ? 0 : oitem.PageIndex,
                    pagerSize: pageSize,
                    enableContextMenu: !isLeaf,
                    treeSource: mainComponent.props.treeSource,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PageIndex = item.pagerIndex;
                oitem.PageSize = item.pagerSize;
                if(mainComponent.hasMixStatusNodeLevel.includes(oitem.Level)){
                    this.setParentCheckedStatusIncludeMix(item);
                }
                switch (item.checked) {
                    case false:
                        oitem.CheckNumber = checkNumberType.nochecked;
                        break;
                    case true:
                        oitem.CheckNumber = checkNumberType.checked;
                        break;
                    case mainComponent.checkboxMixStatus:
                        oitem.CheckNumber = checkNumberType.ckeckedMix;
                        break;
                }
            },

            getAllChildren(oitem) {
                let children = [];
                if (oitem.ChildrenIds && oitem.ChildrenIds.length > 0) {
                    for (let childId of oitem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];

                        children.push(child);
                    }
                }
                return children;
            },

            setParentCheckedStatusIncludeMix(item){
                if(item.checked == mainComponent.checkboxMixStatus){
                    let childrenInCache = [];
                    for(let key in mainComponent.treeCache){
                        if(mainComponent.treeCache[key].ParentId == item.nodeKey){
                            childrenInCache.push(mainComponent.treeCache[key]);
                        }
                    }
                    let selectedChildrenCount = childrenInCache.filter((item)=>{ 
                        return item.CheckNumber === checkNumberType.checked;
                    }).length;
    
                    if(childrenInCache.length === selectedChildrenCount){
                        item.checked = true;
                    }else{
                        item.checked = mainComponent.checkboxMixStatus;
                    }
                }
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
                    return (oitem.ChildrenIds && (oitem.ChildrenIds.length > 0));
                } else {
                    return oitem.Loaded;
                }
            },
  
            removeCache(poItem) {
                if (poItem.ChildrenIds) {
                    for(let key in mainComponent.treeCache){
                        let item = mainComponent.treeCache[key];
                        if(item.ParentId == poItem.Id){
                            delete mainComponent.treeCache[key];
                            this.removeCache(item);
                        }
                    }
                }
            },

            getAncestorsItem(poItem, treeCache){
                if(poItem && poItem.ParentId){
                    Object.assign(treeCache[poItem.ParentId] , { Children: null, ChildrenIds: null }) ;
                    poItem.Parent = treeCache[poItem.ParentId];
                    this.getAncestorsItem(treeCache[poItem.ParentId], treeCache);
                }
            },

            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },

            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let browseUrl = "/API/SPSettingApi/BrowseSampleTree";
                if (mainComponent.props.treeSource == SourceFlags.OneDrive) {
                    browseUrl = "/API/OneDriveSettingApi/BrowseOneDriveTreePaged";
                }
                let poItem = parentItem.origin;
                poItem.Expanded = true;
                poItem.Loaded = true;
                poItem.PageIndex = parentItem.pagerIndex;
                if (poItem.Level === NodeLevel.Farm && !this.loadNodesByRefreshAction) {
                    poItem.CheckNumber = poItem.Children?.length === poItem.ChildrenCount ? 1 : 0;
                }
                
                //refresh 逻辑
                if(this.loadNodesByRefreshAction){
                    let poItem = parentItem.origin;
                    if(poItem.CheckNumber == checkNumberType.ckeckedMix){
                        poItem.CheckNumber = checkNumberType.nochecked;
                        mainComponent.treeCache[poItem.Id].CheckNumber = checkNumberType.nochecked;
                        parentItem.checked = false;
                    }
                    this.removeCache(poItem);
                    this.loadNodesByRefreshAction = false;
                }
                if(!poItem.Parent && poItem.ParentId){
                    let treeCache = RM.deepcopy(mainComponent.treeCache);
                    let parentItem = RM.deepcopy(mainComponent.treeCache[poItem.ParentId]);
                    this.getAncestorsItem(parentItem, treeCache);
                    poItem.Parent = Object.assign({}, parentItem , { Children: null, ChildrenIds: null });
                }
                const isArchiverTree = mainComponent.props.mode == TabIndex.Archive ? true : false;
                let postData = Object.assign({}, poItem, { Children: null, ChildrenIds: null }, { IsArchiverTree: isArchiverTree });
                $$.fetch.post(browseUrl, postData).then((data)=>{
                    let items = data.Children || []; 
                    items = mainComponent.filterItems(items);   
                    for(let item of items){
                        if(mainComponent.treeCache[item.Id]){
                            Object.assign(item, mainComponent.treeCache[item.Id]);
                        }
                    }
                    if (items && items.length > 0) {
                        data.ChildrenIds = items.map(item => {
                            if(!mainComponent.treeCache[item.Id]){
                                item.CheckNumber = poItem.CheckNumber == checkNumberType.ckeckedMix ? checkNumberType.checked : poItem.CheckNumber;
                            }
                            item.Parent = Object.assign({}, poItem, { Children: null, ChildrenIds: null });
                            item.ParentId = poItem.Id;
                            mainComponent.treeCache[item.Id] = item;  
                            return item.Id;
                        });
                    } else {
                        data.ChildrenIds = [];
                    }
                    delete data.CheckNumber; //RECO-15345
                    poItem.IsNotInitItem = false;
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
                if(mainComponent.treeCache && mainComponent.hasMixStatusNodeLevel.includes(item.origin.Level)){
                    for(let key in mainComponent.treeCache){
                        if(mainComponent.treeCache[key].ParentId == item.nodeKey){
                            mainComponent.treeCache[key].CheckNumber = item.origin.CheckNumber;
                        }
                    }
                }
            },

            onNodeRefreshAction(){
                this.loadNodesByRefreshAction = true;
            }
        };
    }

    initTreeData() {
        $.ajax({
            type: "POST",
            url: "/api/DAMApi/GetSPTreeInitData",
            data: [],
            async: true,
            success: (data) => {
                this.setTreeData([data]);
            },
            error: (msg) => {
            },
            dataType: "json"
        });
    }

    setTreeData(data) {
        this.treeCache = {};
        let spOrOneDriveItemName = RMResx.RM_DAM_RootNode;
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Farm) {
                this.rootItem = item;
            }
        });
        this.rootItem.Name = spOrOneDriveItemName;
        this.setTreeCacheData(data);
        this.setState({ items: data});
    }

    setTreeCacheData(items){
        for(let item of items){
            item.IsNotInitItem = true;
            item.PageIndex = 0;
            this.treeCache[item.Id] = item;
            if(item.Children){
                this.setTreeCacheData(item.Children);
            }
        }
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

    relateTreeItemSearchChildren(item) {
        let matchChildren = [];
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
        let children = [];
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
                    $.inArray(uniqueName, this.designLists) != -1)//过滤designLists
                {
                    continue;
                }
                filteredItems.push(items[i]);
            }
        }
        return filteredItems;
    }

    //public function
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

    getIsCheckedNode() {
        let hasSelectedNode = false;
        for (let itemId in this.treeCache) {
            let item = this.treeCache[itemId];
            if (item) {
                if (item.CheckNumber && (item.CheckNumber != checkNumberType.nochecked)) {
                    hasSelectedNode = true;
                    break;
                }
            }
        }
        return hasSelectedNode;
    }

    hasSelectedChildren(children){
        if(children && children.length > 0){
            for(let child of children){
                if(
                    child.CheckNumber == checkNumberType.checked 
                    || this.hasSelectedChildren(child.Children)
                ){
                    return true;
                }
            }
        }
    }

    setTreeParam(treeItem, allItems, key){
        let treeCacheList = Object.values(RM.deepcopy(this.treeCache));
        treeItem.Children = treeCacheList.filter((item)=>{ 
            return item.ParentId == treeItem.Id; 
        });
        if(treeItem.Level == NodeLevel.WebApplication){
            let hasSelectedChildren = this.hasSelectedChildren(treeItem.Children);
            treeItem.CheckNumber = treeItem.CheckNumber || 0;
            switch (treeItem.CheckNumber) {
                case checkNumberType.nochecked:
                    if(hasSelectedChildren){
                        treeItem.Children = treeItem.Children.filter((item)=>{
                            return item.CheckNumber == checkNumberType.checked || this.hasSelectedChildren(item.Children); 
                        });
                    }else{
                        delete allItems[key];
                    }
                    break;
                case checkNumberType.checked:
                    treeItem.Children = null;
                    break;
                case checkNumberType.ckeckedMix:
                    treeItem.Children = treeItem.Children.filter((item)=>{
                        return !item.CheckNumber || (item.CheckNumber == checkNumberType.nochecked); 
                    });
                    break;
                default:
            }
        }
        if(treeItem.Children){
            treeItem.ChildrenIds = treeItem.Children.map((item)=>{ return item.Id; });
            for(let key in treeItem.Children){
                this.setTreeParam(treeItem.Children[key], treeItem.Children, key);
                treeItem.Children[key] = RM.SimplifyObject(
                    treeItem.Children[key], 
                    null, 
                    ["Parent", "IsNotInitItem"]
                );
            }
            treeItem.Children = treeItem.Children.filter((item)=>{ return !!item.Id; });
        }
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
    data: PropTypes.object,
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