import { Component } from "react";
import { SourceFlags } from "../../../../../Constants/Constants";
import { NodeLevel } from "../../../../../Constants/DAEnums";
import GoogleNodeContent from "../../NodeContents/CRM/GoogleNodeContent";
import PropTypes from "prop-types";

const checkNumberType = {
    noChecked: 0,
    checked: 1,
    mixChecked: 2,
}

class ReportGoogleTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            items: [],
        }
        this.treeCache = {};
        this.disableSelectNodeLevels = [NodeLevel.Root];
        this.checkboxMixStatus = "mixed";
        this.hasMixStatusNodeLevel = [NodeLevel.GoogleUserDriveContainer,NodeLevel.GoogleSharedDriveContainer];
        this.loadNodesByRefreshAction = true;

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

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            nodeContentComponent: GoogleNodeContent,
            multiSelection: true,
            readonly: mainComponent.props.readonly,
            transToTreeNodeObject(item) {
                let isLeaf = item.Level == NodeLevel.GoogleUserDrive || item.Level == NodeLevel.GoogleSharedDrive;
                let loaded = this.isLoaded(item);
                let pagedByServer = !!(item.Level == NodeLevel.GoogleUserDriveContainer || item.Level == NodeLevel.GoogleSharedDriveContainer || item.Level == NodeLevel.Root) && !item.IsNotInitItem;
                let isHasMixedStatus = mainComponent.hasMixStatusNodeLevel.includes(item.Level);
                let children = item.Children ? item.Children : [];
                let pageSize = 10;

                return {
                    origin: item,
                    nodeKey: item.Id,
                    nodeType: item.Level,
                    text: item.DisplayName,
                    isHasMixedStatus: isHasMixedStatus,
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(item.Level) >= 0,
                    isLeafNode: isLeaf,
                    enableIncludeNew: false,
                    checked: item.CheckNumber == checkNumberType.checked ? true : (item.CheckNumber == checkNumberType.mixChecked ? mainComponent.checkboxMixStatus : false),
                    loaded: loaded,
                    expanded: item.Expanded,
                    items: children,
                    itemsCount: pagedByServer ? item.ChildrenCount : (children ? children.length : 0),
                    hasChildren: true,
                    pagerByServer: pagedByServer,
                    pagerIndex: !item.PageIndex || item.PageIndex * pageSize >= children.length ? 0 : item.PageIndex,
                    pagerSize: pageSize,
                    enableContextMenu: !isLeaf,
                    treeSource: mainComponent.props.treeSource,
                    isReportTree: true,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.Loaded = item.loaded;
                // oitem.CheckNumber = item.checked ? 1 : 0;
                oitem.Expanded = item.expanded;
                oitem.PageIndex = item.pagerIndex;
                oitem.PageSize = item.pagerSize;

                if (mainComponent.hasMixStatusNodeLevel.includes(oitem.Level)) {
                    this.setParentCheckedStatusIncludeMix(item);
                }

                switch (item.checked) {
                    case false:
                        oitem.CheckNumber = checkNumberType.noChecked;
                        break;
                    case true:
                        oitem.CheckNumber = checkNumberType.checked;
                        break;
                    case mainComponent.checkboxMixStatus:
                        oitem.CheckNumber = checkNumberType.mixChecked;
                        break;
                }
            },
            getAllChildren(oitem) {
                let children = [];
                if (oitem.ChildrenIds && oitem.ChildrenIds.length > 0) {
                    for (let childId of oitem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            // child.Parent = oitem;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            setParentCheckedStatusIncludeMix(item) {
                if(item.checked == mainComponent.checkboxMixStatus){
                    let childrenInCache = [];
                    for (let key in mainComponent.treeCache) {
                        if (mainComponent.treeCache[key].ContainerId && mainComponent.treeCache[key].ContainerId != mainComponent.treeCache[key].Id && mainComponent.treeCache[key].ContainerId == item.nodeKey){
                            childrenInCache.push(mainComponent.treeCache[key]);
                        }
                    }
                    let selectedChildrenCount = childrenInCache.filter((item) => { 
                        return item.CheckNumber === checkNumberType.checked;
                    }).length;
    
                    if (childrenInCache.length === selectedChildrenCount){
                        item.checked = true;
                    } else {
                        item.checked = mainComponent.checkboxMixStatus;
                    }
                }
            },
            isLoaded(oitem) {
                if (oitem.Loaded) {
                    return oitem.Loaded;
                }
        
                return (oitem.ChildrenIds && (oitem.ChildrenIds.length > 0))
            },
            removeCache(poItem) {
                if (poItem.ChildrenIds) {
                    for(let key in mainComponent.treeCache){
                        let item = mainComponent.treeCache[key];
                        if (item.Parent && item.Parent.Id == poItem.Id){
                            delete mainComponent.treeCache[key];
                            this.removeCache(item);
                        }
                    }
                }
            },
            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let browseUrl = "/api/GoogleDriveSettingApi/BrowseSampleTree";
                let poItem = parentItem.origin;
                poItem.Expanded = true;
                poItem.Loaded = true;
                poItem.PageIndex = parentItem.pagerIndex;
                if (poItem.Level == NodeLevel.Root && !this.loadNodesByRefreshAction) {
                    poItem.CheckNumber = poItem.Children?.length == poItem.ChildrenCount ? checkNumberType.checked : checkNumberType.noChecked;
                }
                //refresh 逻辑
                if(this.loadNodesByRefreshAction){
                    let poItem = parentItem.origin;
                    if(poItem.CheckNumber == checkNumberType.mixChecked){
                        poItem.CheckNumber = checkNumberType.noChecked;
                        mainComponent.treeCache[poItem.Id].CheckNumber = checkNumberType.noChecked;
                        parentItem.checked = false;
                    }
                    this.removeCache(poItem);
                    this.loadNodesByRefreshAction = false;
                }

                let postData = Object.assign({}, poItem, { Children: null, ChildrenIds: null });
                $$.fetch.post(browseUrl, postData).then((res)=>{
                    let data = res;
                    let items = data ? data.Children : [];
                    if (data) {
                        for (let item of items) {
                            if (mainComponent.treeCache[item.Id]) {
                                Object.assign(item, mainComponent.treeCache[item.Id]);
                            }
                        }
                        if (items && items.length > 0) {
                            data.ChildrenIds = items.map(item => {
                                if (!mainComponent.treeCache[item.Id]) {
                                    item.CheckNumber = poItem.CheckNumber == checkNumberType.mixChecked ? checkNumberType.checked : poItem.CheckNumber;
                                }
                                item.Parent = Object.assign({}, poItem, { Children: null, ChildrenIds: null });
                                item.ParentId = poItem.Id;
                                mainComponent.treeCache[item.Id] = item;  
                                return item.Id;
                            });
                        } else {
                            data.ChildrenIds = [];
                        }
                        poItem.IsNotInitItem = false;
                    }
                    delete data.CheckNumber;
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
                if (mainComponent.treeCache && item.origin.Parent && mainComponent.hasMixStatusNodeLevel.includes(item.origin.Parent.Level)) {
                    for (let key in mainComponent.treeCache) {
                        if(mainComponent.treeCache[key].Id == item.nodeKey) {
                            mainComponent.treeCache[key].CheckNumber = item.origin.CheckNumber;
                        }
                    }
                }
            },
            onNodeRefreshAction(){
                this.loadNodesByRefreshAction = true;
            },
        };
    }

    initTreeData() {
        $.ajax({
            type: "GET",
            url: "/api/GoogleDriveSettingApi/GetGoogleDriveRootNode",
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
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Root) {
                this.rootItem = item;
            }
        });
        this.setTreeCacheData(data);
        this.setState({ items: data });
    }

    setTreeCacheData(items){
        for(let item of items){
            item.IsNotInitItem = true;
            item.PageIndex = 0;
            // if(parentItem && item["$id"] && parentItem.checkNumber == checkNumberType.checked){
            //     item.checkNumber = checkNumberType.checked;
            // }
            this.treeCache[item.Id] = item;
            if (item.Children) {
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
                let text = child.DisplayName;
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
        item.Children = children;
    }

    //public function
    getTreeData() {
        let treeItem = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                if (this.treeCache[itemId].Level == NodeLevel.Root) {
                    treeItem = RM.deepcopy(this.treeCache)[itemId];
                }
            }
        }
        let treeCacheList = Object.values(RM.deepcopy(this.treeCache));
        this.setTreeParam(treeItem, treeCacheList);
        return { items: treeItem, selected: this.getIsCheckedNode() };
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

    hasSelectedChildren(children){
        if (children && children.length > 0) {
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

    setTreeParam(treeItem, treeCacheList, allItems, key) {
        treeItem.Children = treeCacheList.filter((item) => {
            return item.ParentId == treeItem.Id; 
        });
        const isCheckTreeLevel = !!(treeItem.Level == NodeLevel.GoogleUserDriveContainer || treeItem.Level == NodeLevel.GoogleSharedDriveContainer)
        if (isCheckTreeLevel) {
            let hasSelectedChildren = this.hasSelectedChildren(treeItem.Children);
            treeItem.CheckNumber = treeItem.CheckNumber || 0;
            switch (treeItem.CheckNumber) {
                case checkNumberType.noChecked:
                    if (hasSelectedChildren) {
                        treeItem.Children = treeItem.Children.filter((item) => {
                            return item.CheckNumber == checkNumberType.checked || this.hasSelectedChildren(item.Children);
                        })
                    } else {
                        allItems && delete allItems[key];
                    }
                    break;
                case checkNumberType.checked:
                    treeItem.Children = null;
                    break;
                case checkNumberType.mixChecked:
                    treeItem.Children = treeItem.Children.filter((item) => {
                        return !item.CheckNumber
                    });
                    break;
                default:
            }
        }

        if (treeItem.Children) {
            treeItem.ChildrenIds = treeItem.Children.map((item) => item.Id);
            for (let key in treeItem.Children) {
                this.setTreeParam(treeItem.Children[key], treeCacheList, treeItem.Children, key);
                treeItem.Children[key] = RM.SimplifyObject(
                    treeItem.Children[key],    
                    null, 
                    ["Parent", "IsNotInitItem"]
                );
            }
            treeItem.Children = treeItem.Children.filter((item)=> !!item.Id);
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
        )
    }
}

ReportGoogleTree.propTypes = {
    data: PropTypes.object,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    treeSource: PropTypes.number
};

ReportGoogleTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    treeSource: SourceFlags.Google
};

export default ReportGoogleTree