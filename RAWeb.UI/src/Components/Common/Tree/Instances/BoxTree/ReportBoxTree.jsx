import React, { Component } from 'react'
import PropTypes from 'prop-types';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import { SourceFlags } from '../../../../../Constants/Constants';
import BoxDestinationTreeNodeContent from '../../NodeContents/RC/BoxNodeContent';

const checkNumberType = {
    noChecked: 0,
    checked: 1,
    mixChecked: 2,
}

export default class ReportBoxTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            items: []
        };
        this.disableSelectNodeLevels = [NodeLevel.Root];
        this.treeCache = {};
        this.checkboxMixStatus = "mixed";
        this.hasMixStatusNodeLevel = [NodeLevel.BoxConnectionGroup];

        this.initTreeContext();
    }

    componentDidMount() {
        if(this.props.data) {
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
            nodeContentComponent: BoxDestinationTreeNodeContent,
            multiSelection: true,
            readonly: mainComponent.props.readonly,
            transToTreeNodeObject(item) {
                let isLeaf = item.level == NodeLevel.BoxUser;
                let loaded = this.isLoaded(item);
                // let pagedByServer = item.level < NodeLevel.BoxDirectory && !item.isNotInitItem;
                let pagedByServer = item.level < NodeLevel.BoxConnection && !item.isNotInitItem;
                let isHasMixedStatus = mainComponent.hasMixStatusNodeLevel.includes(item.level);
                let children = item.children ? item.children : [];
                let pageSize = 10;

                return {
                    origin: item,
                    nodeKey: item.id,
                    nodeType: item.level,
                    text: item.displayName,
                    isHasMixedStatus: isHasMixedStatus,
                    disableSelect: mainComponent.disableSelectNodeLevels.indexOf(item.level) >= 0,
                    isLeafNode: isLeaf,
                    enableIncludeNew: false,
                    checked: item.checkNumber == checkNumberType.checked ? true : (item.checkNumber == checkNumberType.mixChecked ? mainComponent.checkboxMixStatus : false),
                    loaded: loaded,
                    expanded: item.expanded,
                    items: children,
                    itemsCount: pagedByServer ? item.childrenCount : (children ? children.length : 0),
                    hasChildren: true,
                    pagerByServer: pagedByServer,
                    pagerIndex: !item.pageIndex || item.pageIndex * pageSize >= children.length ? 0 : item.pageIndex,
                    pagerSize: pageSize,
                    enableContextMenu: !isLeaf,
                    treeSource: mainComponent.props.treeSource,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.loaded = item.loaded;
                oitem.expanded = item.expanded;
                oitem.pageIndex = item.pagerIndex;
                oitem.pageSize = item.pagerSize;
                if(mainComponent.hasMixStatusNodeLevel.includes(oitem.level)){
                    this.setParentCheckedStatusIncludeMix(item);
                }
                switch (item.checked) {
                    case false:
                        oitem.checkNumber = checkNumberType.noChecked;
                        break;
                    case true:
                        oitem.checkNumber = checkNumberType.checked;
                        break;
                    case mainComponent.checkboxMixStatus:
                        oitem.checkNumber = checkNumberType.mixChecked;
                        break;
                }
            },
            getAllChildren(oitem) {
                let children = [];
                if (oitem.childrenIds && oitem.childrenIds.length > 0) {
                    for (let childId of oitem.childrenIds) {
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
                        if(mainComponent.treeCache[key].containerId && mainComponent.treeCache[key].containerId == item.nodeKey){
                            childrenInCache.push(mainComponent.treeCache[key]);
                        }
                    }
                    let selectedChildrenCount = childrenInCache.filter((item)=>{ 
                        return item.checkNumber === checkNumberType.checked;
                    }).length;
    
                    if(childrenInCache.length === selectedChildrenCount){
                        item.checked = true;
                    }else{
                        item.checked = mainComponent.checkboxMixStatus;
                    }
                }
            },
            isLoaded(item) {
                if (item.loaded) {
                    return item.loaded;
                }
        
                return (item.childrenIds && (item.childrenIds.length > 0))
            },
            removeCache(poItem) {
                if (poItem.childrenIds) {
                    for(let key in mainComponent.treeCache){
                        let item = mainComponent.treeCache[key];
                        if(item.parent && item.parent.id == poItem.id){
                            delete mainComponent.treeCache[key];
                            this.removeCache(item);
                        }
                    }
                }
            },
            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.expanded = isExpanded;
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let browseUrl = "/api/BoxTreeQuery/BBrowserTreeByPager";
                let poItem = parentItem.origin;
                poItem.expanded = true;
                poItem.loaded = true;
                poItem.pageIndex = parentItem.pagerIndex;
                if (poItem.level === NodeLevel.Root && !this.loadNodesByRefreshAction) {
                    poItem.checkNumber = poItem.children?.length === poItem.childrenCount ? 1 : 0;
                }

                //refresh 逻辑
                if(this.loadNodesByRefreshAction){
                    let poItem = parentItem.origin;
                    if(poItem.checkNumber == checkNumberType.mixChecked){
                        poItem.checkNumber = checkNumberType.noChecked;
                        mainComponent.treeCache[poItem.id].checkNumber = checkNumberType.noChecked;
                        parentItem.checked = false;
                    }
                    this.removeCache(poItem);
                    this.loadNodesByRefreshAction = false;
                }

                let postData = Object.assign({}, poItem, { children: null, childrenIds: null });
                $$.fetch.post(browseUrl, postData).then((res)=>{
                    let data = res;
                    let items = data ? data.children : [];
                    if(data){
                        for(let item of items){
                            if(mainComponent.treeCache[item.id]){
                                Object.assign(item, mainComponent.treeCache[item.id]);
                            }
                        }
                        if (items && items.length > 0) {
                            data.childrenIds = [...data.childrenIds, items.map(item => {
                                if(!mainComponent.treeCache[item.id]){
                                    item.checkNumber = poItem.checkNumber == checkNumberType.mixChecked ? checkNumberType.checked : poItem.checkNumber;
                                }
                                item.parent.id = poItem.id;
                                mainComponent.treeCache[item.id] = item;  
                                return item.id;
                            })];
                        } else {
                            data.childrenIds = [];
                        }
                        poItem.isNotInitItem = false;
                    }
                    delete data.checkNumber;
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
                if(mainComponent.treeCache && item.origin.parent && mainComponent.hasMixStatusNodeLevel.includes(item.origin.parent.level)){
                    for(let key in mainComponent.treeCache){
                        if(mainComponent.treeCache[key].id == item.nodeKey){
                            mainComponent.treeCache[key].checkNumber = item.origin.checkNumber;
                        }
                    }
                }
            },
            onNodeRefreshAction(){
                this.loadNodesByRefreshAction = true;
            },
        }
    }

    initTreeData() {
        $.ajax({
            type: "POST",
            url: "/api/BoxTreeQuery/GetRootNode",
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
            this.treeCache[item.id] = item;
            if (item.level == NodeLevel.Root) {
                this.rootItem = item;
            }
        });

        this.rootItem.displayName = RMResx.RM_JS_SPS_FS_RootNode;
        this.setTreeCacheData(data);
        this.setState({ items: data });
    }

    setTreeCacheData(items, parentItem){
        for(let item of items){
            item.isNotInitItem = true;
            item.pageIndex = 0;
            if(parentItem && item["$id"] && parentItem.checkNumber == checkNumberType.checked){
                item.checkNumber = checkNumberType.checked;
            }
            this.treeCache[item.id] = item;
            if(item.children){
                this.setTreeCacheData(item.children, item);
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
        if (item && item.children) {
            for (let child of item.children) {
                let text = child.displayName;
                if (text == "." && child.level == NodeLevel.Site) {
                    text = child.title;
                }
                if (text.toUpperCase().indexOf(this.treeContext.searchKey.toUpperCase()) > -1
                    || this.relateTreeItemSearchChildren(child)) {
                    matchChildren.push(child);
                }
            }
            item.children = matchChildren;
        }
        return matchChildren.length > 0;
    }

    relateTreeItemChildren(item) {
        let children = [];
        if (item && item.childrenIds) {
            for (let childId of item.childrenIds) {
                let child = this.treeCache[childId];
                if (child) {
                    children.push(child);
                    if(child.childrenIds && child.childrenIds.length > 0){
                        this.relateTreeItemChildren(child);
                    }
                }
            }
        }
        item.children = children;
    }

    //public function
    getTreeData() {
        let treeItem = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                if (this.treeCache[itemId].level == NodeLevel.Root) {
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
                if (item.checkNumber && (item.checkNumber != checkNumberType.noChecked)) {
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
                    child.checkNumber == checkNumberType.checked 
                    || this.hasSelectedChildren(child.children)
                ){
                    return true;
                }
            }
        }
    }

    setTreeParam(treeItem, treeCacheList, allItems, key){
        treeItem.children = treeCacheList.filter((item) => {
            // if (item.level === NodeLevel.Root || item.level === NodeLevel.BoxConnectionGroup) {
            //     return item.checkNumber && treeItem.childrenIds && treeItem.childrenIds.includes(item.id);
            // }

            return treeItem.childrenIds && treeItem.childrenIds.includes(item.id);
        });

        if(treeItem.level == NodeLevel.Root){
            let hasSelectedChildren = this.hasSelectedChildren(treeItem.children);
            treeItem.checkNumber = treeItem.checkNumber || 0;
            switch (treeItem.checkNumber) {
                case checkNumberType.noChecked:
                    if(hasSelectedChildren){
                        treeItem.children = treeItem.children.filter(item => {
                            return item.checkNumber || this.hasSelectedChildren(item.children);
                        })
                    }else{
                        if (allItems) delete allItems[key];
                    }
                    break;
                case checkNumberType.checked:
                    // treeItem.children = null;
                    break;
                case checkNumberType.mixChecked:
                    treeItem.children = treeItem.children.filter((item)=>{
                        return item.checkNumber == checkNumberType.noChecked; 
                    });
                    break;
                default:
            }
        }

        if(treeItem.children){
            treeItem.childrenIds = treeItem.children.map((item)=>{ return item.id; });
            for(let key in treeItem.children){
                this.setTreeParam(treeItem.children[key], treeCacheList, treeItem.children, key);
                treeItem.children[key] = RM.SimplifyObject(
                    treeItem.children[key],    
                    null, 
                    ["parent", "isNotInitItem", "$id"]
                );
            }
            treeItem.children = treeItem.children.filter((item)=>{ return !!item.id; });
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

ReportBoxTree.propTypes = {
    data: PropTypes.object,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    treeSource: PropTypes.number
};
ReportBoxTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    treeSource: SourceFlags.Box
};