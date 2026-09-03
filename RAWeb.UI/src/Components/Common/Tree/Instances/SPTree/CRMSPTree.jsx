import { Component } from 'react';
import PropTypes from 'prop-types';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import SPNodeContent from '../../NodeContents/CRM/SPNodeContent';
import { SourceFlags } from "../../../../../Constants/Constants";
import { TabIndex } from '../../../../BCM/ContentRepositoryManagement/CRMForSPO';
import { pagerModes } from '../../Components/Constants';
import { LicenseHelper } from '../../../../../Utilities/CommonUtil';

class CRMSPTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            tipStatus: { show: false },
            tipType: 'success',
            tipMsg: '',
            items: []
        };

        let mainComponent = this;
        this.searchKey = "";
        this.initDataStr = null;
        this.selectedNodeItem = null;
        this.DesignLists = [];
        this.treeCache = [];
        this.updateProps = {};
        this.notAlloweSelected = [
            NodeLevel.Farm, 
            NodeLevel.Lists, 
            NodeLevel.Sites, 
            NodeLevel.Apps, 
            NodeLevel.RootFolder, 
            NodeLevel.Folders,
            NodeLevel.Items,
            NodeLevel.Groups,
            NodeLevel.Users,
        ];
        this.treeContext = {
            nodeContentComponent: SPNodeContent,
            singleSelection: true,
            showrRightArrow: true,
            shadowInitialNodelevel: NodeLevel.SiteCollection,
            isLoadFollowScrollbar: true,
            transToTreeNodeObject(oitem) {
                let isLoadMoreMode = mainComponent.props.treeSource == SourceFlags.SP && oitem.Level === NodeLevel.WebApplication;
                let pagerMode = isLoadMoreMode  ? pagerModes.loadMore : pagerModes.normal;
                let children =  this.getViewChildren(oitem);
                let isLeaf = !mainComponent.notAlloweSelected.includes(oitem.Level);
                let pagedByServer = mainComponent.props.treeSource != SourceFlags.SPLocal && oitem.Level < NodeLevel.SiteCollection;
                let isLeafNode = oitem.Level == NodeLevel.List && oitem.NodeType != 1;
                let pagerSize = 15;
                let hasNextPage = !!oitem.HasNextPage && mainComponent.props.treeSource == SourceFlags.SP;
                const hasSearchKey = mainComponent.props.searchKey !== "";
                const specialLevels = new Set([NodeLevel.Farm]);
                const shouldResetPagerIndex = !oitem.PageIndex || (!pagedByServer && oitem.PageIndex * pagerSize >= children.length);
                return {
                    origin: oitem,
                    treeId: `crm-spo-tree`,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    isLeafNode: isLeafNode,
                    disableSelect: !isLeaf,
                    text: this.getText(oitem),
                    checked: oitem.CheckNumber == 1,
                    loaded: (hasSearchKey && specialLevels.has(oitem.Level)) ? true : this.isLoaded(oitem),
                    expanded: oitem.Expanded,
                    enableContextMenu: !isLeafNode,
                    iconStatus: oitem.IconStatus,
                    items: children,
                    itemsCount: isLoadMoreMode ? (children ? children.length : 0) : (pagedByServer ? oitem.ChildrenCount : children ? (children.length > 0 ? children.length : 1) : 0),
                    hasChildren: true,
                    hasNextPage: hasNextPage,
                    treeSource: mainComponent.props.treeSource,
                    pagerMode: pagerMode,
                    pagerByServer: pagedByServer,
                    pagerSize: pagerSize,
                    pagerIndex: shouldResetPagerIndex ? 0 : oitem.PageIndex,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.CheckNumber = item.checked ? 1 : 0;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PageIndex = item.pagerIndex;
                oitem.PageSize = item.pagerSize;
            },
            getAllChildren(oitem) {
                let children = [];
                if (oitem.ChildrenIds && oitem.ChildrenIds.length > 0) {
                    for (let childId of oitem.childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = oitem;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            getViewChildren(oitem) {
                let children = [];
                if(oitem.Children){
                    for (let child of oitem.Children) {
                        child.Parent = oitem;
                        children.push(child);
                    }
                }else{
                    let childrenIds = oitem.ChildrenIds;
                    if (childrenIds) {
                        for (let childId of childrenIds) {
                            let child = mainComponent.treeCache[childId];
                            if (child) {
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
                if (oitem.TeamName) {
                    text = "(" + oitem.TeamName + ")" + oitem.Name;
                }
                if (oitem.OrphanNameSuffix) {
                    text = oitem.Name + oitem.OrphanNameSuffix;
                }
                return text;
            },
            isLoaded(oitem) {
                //RM 3.1 版本 没有Loaded属性,3.2添加该属性，该属性保存在数据库中，用于记录节点“是否加载过”
                if (oitem.Loaded == null || oitem.Loaded == undefined) {
                    return (oitem.ChildrenIds && oitem.ChildrenIds.length > 0)
                        || (oitem.IncludeNew == 1);
                }
                else {
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
            appendChildrenCache(parentOriginItem, items) {
                let existingChildIds = parentOriginItem.ChildrenIds ? [...parentOriginItem.ChildrenIds] : [];
                let childIdMap = Object.create(null);

                existingChildIds.forEach((childId) => {
                    childIdMap[childId] = true;
                });

                (items || []).forEach((item) => {
                    item.Parent = parentOriginItem;
                    mainComponent.treeCache[item.Id] = item;
                    if (!childIdMap[item.Id]) {
                        childIdMap[item.Id] = true;
                        existingChildIds.push(item.Id);
                    }
                });

                parentOriginItem.ChildrenIds = existingChildIds;
            },
            onExpandClick(parentItem, isExpanded) {
                parentItem.origin.Expanded = isExpanded;
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                this.updateOriginObject(parentItem);
                let poItem = parentItem.origin;
                let isLoadMoreMode = mainComponent.props.treeSource == SourceFlags.SP && poItem.Level === NodeLevel.WebApplication;
                let isArchiverTree = mainComponent.props.mode == TabIndex.Archive ? true : false;
                let postData = Object.assign({}, poItem, { Children: null, ChildrenIds: null }, { IsArchiverTree: isArchiverTree});
                postData.SearchKey = mainComponent.props.searchKey;
                if((postData.IsSearch || postData.Level === NodeLevel.Farm || postData.Level === NodeLevel.Root) && postData.SearchKey !== ""){
                    postData.IsSearch = true;
                    postData.Children = null;
                }else{
                    postData.IsSearch = false;
                }        
                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
                    currentItem = currentItem.Parent;
                }
                
                let url = "/API/SPOnPremBrowse/BrowseSampleTreePaged";
                if (mainComponent.props.treeSource == SourceFlags.SP) {
                    url = "/API/SPSettingApi/BrowseSampleTree";
                }
                if (mainComponent.props.treeSource == SourceFlags.OneDrive) {
                    url = "/API/OneDriveSettingApi/BrowseOneDriveTreePaged";
                }

                if(mainComponent.props.searchKey !== "" && poItem.Level === NodeLevel.Farm){
                    url = "/API/SPSettingApi/SearchContainerByPage";
                }

                if (isLoadMoreMode) {
                    url = "/API/SPOnPremBrowse/SearchSiteCollectionLazyLoad";
                    if (mainComponent.props.treeSource == SourceFlags.SP) {
                        url = "/API/SPSettingApi/SearchSiteCollectionLazyLoad";
                    }
                    if (mainComponent.props.treeSource == SourceFlags.OneDrive) {
                        url = "/API/OneDriveSettingApi/SearchSiteCollectionLazyLoad";
                    }

                    postData = {
                        IsArchiverTree: isArchiverTree,
                        SourceFlag: mainComponent.props.treeSource,
                        ContainerId: poItem.Id,
                        LastUrl: parentItem.pagerIndex == 0 ? null : poItem.LastUrl,
                        PageSize: 15
                    };
                }
                
                $$.fetch.post(url, postData).then((data)=>{
                    let items = data.Children;
                    if(mainComponent.updateProps.IconStatus){
                        data.IconStatus = RM.deepcopy(mainComponent.updateProps).IconStatus;
                        mainComponent.updateProps = {};
                    }
                    items = mainComponent.filterItems(items);
                    if (isLoadMoreMode && parentItem.pagerIndex > 0) {
                        this.appendChildrenCache(poItem, items);
                    } else {
                        this.removeCache(poItem);
                        if (items && items.length > 0) {
                            poItem.ChildrenIds = items.map(item => {
                                item.Parent = poItem;
                                mainComponent.treeCache[item.Id] = item;
                                return item.Id;
                            });
                        } else {
                            poItem.ChildrenIds = [];
                        }
                    }

                    if (isLoadMoreMode) {
                        poItem.Children = null;
                        data.Children = null;
                        data.ChildrenIds = poItem.ChildrenIds;
                    }
                    funcSuccess(items, data);  
                }).catch(funcFail);
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

        this.getDesignLists();
        this.initData(props.treeData);
    }


    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
    }

    getDesignLists() {
        let url = "/api/SPOnPremBrowse/GetSPDesignLists";
        if (this.props.treeSource == SourceFlags.SP) {
            url = "/api/DAMApi/GetSPDesignLists";
        }
        if (this.props.treeSource == SourceFlags.OneDrive) {
            url = "/api/OneDriveSettingApi/GetSPDesignLists";
        }
        $.ajax({
            type: "POST",
            url: url,
            data: [],
            async: true,
            success: (data) => {
                this.DesignLists = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: tree data; Ignore Reason: 前后台对象存在对应关系
            },
            error: (msg) => {
            },
            dataType: "json"
        });
    }

    initData(treeData) {
        if (treeData && treeData.length > 0) {
            this.setTreeData(treeData);
        } else if (this.initDataStr) {
            this.setTreeData([$.parseJSON(this.initDataStr)]);
        } else {
            let that = this;
            let url = "/api/SPOnPremBrowse/GetSPTreeInitData";
            if (this.props.treeSource == SourceFlags.SP) {
                url = "/api/SPSettingApi/GetSPTreeInitData";
            }
            if (this.props.treeSource == SourceFlags.OneDrive) {
                url = "/api/OneDriveSettingApi/GetSPTreeInitData";
            }
            $.ajax({
                type: "POST",
                url: url,
                data: [],
                async: true,
                success: (data) => {
                    that.initDataStr = data;
                    if (!that.props.treeData || that.props.treeData.length == 0) {
                        that.setTreeData([$.parseJSON(data)]);  // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
                    }
                },
                error: (msg) => {
                },
                dataType: "json"
            });
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

    filterItems(items) {
        var filteredItems = new Array();
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            //过滤系统节点
            if (!item.Hidden && item.Level != NodeLevel.Apps && item.Level != NodeLevel.Items) {
                //过滤List和配置文件中的designLists（有可能是library）
                var index = 0;
                var uniqueName = '';
                if (item.FullPath) {
                    index = item.FullPath.lastIndexOf('/');
                    uniqueName = item.FullPath.substr(index + 1) + item.TemplateId;
                    if (item.TemplateId == 600) {
                        continue;
                    }
                }
                if (item.Level == NodeLevel.List && $.inArray(uniqueName, this.DesignLists) != -1)//过滤designLists
                {
                    continue;
                }
                filteredItems.push(items[i]);
            }
        }
        return filteredItems;
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
        this.rootItem.Name = RMResx.RM_DAM_RootNode;
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

    updateChildrenRecursively(node) {
        if (!node || node.Level === 300) return;

        if (Array.isArray(node.Children)) {
            // Filter the current level
            node.Children = this.filterItems(node.Children);

            // Recurse into next level
            for (const child of node.Children) {
                this.updateChildrenRecursively(child);
            }
        }
    }

    search = async (keywords) => {
        this.searchKey = keywords.trim();
        if (this.searchKey && this.searchKey.length > 0) {
            $$.loading(true);
            let url = this.props.treeSource == SourceFlags.SP ? "/api/SPSettingApi/SearchContainerByPage" : "/API/OneDriveSettingApi/BrowseOneDriveTreePaged";
            let rootItemForsearch = RM.deepcopy(this.rootItem);
            let isArchiverTree = this.props.mode == TabIndex.Archive ? true : false;
            rootItemForsearch.SearchKey = keywords;
            rootItemForsearch.PageIndex = 0;
            rootItemForsearch.PageSize = 15;
            rootItemForsearch.Children = null;
            rootItemForsearch.IsSearch = true;
            rootItemForsearch.IsArchiverTree = isArchiverTree;
            let data = {
                url:  url,
                data: rootItemForsearch,
            };
            var result = await fetchUtility(data);
            if (result && LicenseHelper.EnableRecordsArchiver() && this.props.treeSource == SourceFlags.SP) {
                result.Name = RMResx.RM_JS_DAM_Container_Results;
            }
            this.setState({ items: [result] });
            $$.loading(false);
        } else {
            this.relateTreeItemChildren(this.rootItem);
            this.updateChildrenRecursively(this.rootItem);
            this.setState({ items: [this.rootItem] });
        }
    }

    refreshSelectedNode = (updateProps,isReload) => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
                if (updateProps) {
                    if(isReload){
                        selNode.props.item.loaded = false;
                        this.updateProps = updateProps;
                        selNode.reload(0);
                    }
                    Object.assign(selNode.props.item.origin, updateProps);
                    selNode.props.item.iconStatus = updateProps.IconStatus;
                    selNode.reRender();
                } 
            }
        }
    };


    render() {
        return <$g.TreeView
            id="spTree"
            classicMode
            items={this.state.items}
            treeContext={this.treeContext}
        />;
    }
}

CRMSPTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func,
    searchKey: PropTypes.string
};
CRMSPTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null,
    searchKey: PropTypes.string
};

export default CRMSPTree;