import { Component } from "react";
import PropTypes from "prop-types";
import _ from "lodash";

import TeamsNodeContent from "../../NodeContents/CRM/TeamsNodeContent";
import { NodeLevel } from "../../../../../Constants/DAEnums";
import { SourceFlags } from "../../../../../Constants/Constants";
import { TabIndex } from "../../../../BCM/ContentRepositoryManagement/CRMForTeams";
import { pagerModes } from "../../Components/Constants";
import { LicenseHelper } from "../../../../../Utilities/CommonUtil";

class CRMTeamsTree extends Component {
    constructor(props) {
        super(props);
        this.state = {
            items: [],
        };
        this.searchKey = "";
        this.initDataStr = null;
        this.selectedNodeItem = null;
        this.designLists = [];
        this.treeCache = [];
        this.updateProps = {};
        this.notAllowSelected = [
            NodeLevel.Farm,
            NodeLevel.SiteCollections,
            NodeLevel.Lists,
            NodeLevel.Sites,
            NodeLevel.Apps,
            NodeLevel.RootFolder,
            NodeLevel.Folders,
            NodeLevel.Items,
            NodeLevel.Users,
        ];
        this.treeContext = this.initTreeContext();
        this.getDesignLists();
        this.initTreeData(props.treeData);
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
    }

    initTreeContext = () => {
        const mainComponent = this;
        return {
            nodeContentComponent: TeamsNodeContent,
            singleSelection: true,
            showrRightArrow: true,   
            shadowInitialNodelevel: NodeLevel.Office365GroupEntire,
            isLoadFollowScrollbar: true,
            transToTreeNodeObject(originItem) {
                const isLoadMoreMode = originItem.Level === NodeLevel.WebApplication;
                const pagerMode = isLoadMoreMode
                    ? pagerModes.loadMore : pagerModes.normal
                const children =  this.getViewChildren(originItem);
                const isLeaf = !mainComponent.notAllowSelected.includes(originItem.Level);
                const pagerByServer = originItem.Level < NodeLevel.SiteCollection;
                const isLeafNode = originItem.Level == NodeLevel.List && originItem.NodeType != 1;
                const pagerSize = 15;
                const hasNextPage = !!originItem.HasNextPage;
                const hasSearchKey = mainComponent.props.searchKey !== "";
                const specialLevels = new Set([NodeLevel.Farm]);
                const shouldResetPagerIndex = !originItem.PageIndex || (!pagerByServer && originItem.PageIndex * pagerSize >= children.length);
                
                return {
                    origin: originItem,
                    treeId: `crm-teams-tree`,
                    nodeKey: originItem.Level === NodeLevel.Office365GroupEntire ? originItem.TeamsId : originItem.Id,
                    nodeType: originItem.Level, 
                    isLeafNode,
                    disableSelect: !isLeaf,
                    text: this.getText(originItem),
                    checked: originItem.CheckNumber == 1,
                    loaded: (hasSearchKey && specialLevels.has(originItem.Level)) ? true : this.isLoaded(originItem),
                    expanded: originItem.Expanded,
                    enableContextMenu: !isLeafNode,
                    iconStatus: originItem.IconStatus,
                    items: children,
                    itemsCount: isLoadMoreMode ? (children ? children.length : 0) : (pagerByServer ? originItem.ChildrenCount : children ? (children.length > 0 ? children.length : 1) : 0),
                    hasChildren: true,
                    hasNextPage,
                    treeSource: SourceFlags.Teams,
                    pagerMode,
                    pagerByServer,
                    pagerSize,
                    pagerIndex: shouldResetPagerIndex ? 0 : originItem.PageIndex,
                };
            },
            updateOriginObject(item) {
                const originItem = item.origin;
                originItem.CheckNumber = item.checked ? 1 : 0;
                originItem.Loaded = item.loaded;
                originItem.Expanded = item.expanded;
                originItem.PageIndex = item.pagerIndex;
                originItem.PageSize = item.pagerSize;
            },
            getAllChildren(originItem) {
                const children = [];
                if (
                    originItem.ChildrenIds &&
                    originItem.ChildrenIds.length > 0
                ) {
                    for (let childId of originItem.childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = originItem;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            getViewChildren(originItem) {
                const children = [];
                if (originItem.Children) {
                    for (let child of originItem.Children) {
                        child.Parent = originItem;
                        children.push(child);
                    }
                } else {
                    let childrenIds = originItem.ChildrenIds;
                    if (childrenIds) {
                        for (let childId of childrenIds) {
                            let child = mainComponent.treeCache[childId];
                            if (child) {
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
                if (_.isNil(originItem.Loaded)) {
                    return (originItem.ChildrenIds && originItem.ChildrenIds.length > 0) || (originItem.IncludeNew == 1);
                }
                return originItem.Loaded;
            },
            removeCache(parentOriginItem) {
                if (parentOriginItem.ChildrenIds) {
                    for (let childId of parentOriginItem.ChildrenIds) {
                        const child = mainComponent.treeCache[childId];
                        if (child) {
                            delete mainComponent.treeCache[childId];
                            this.removeCache(child);
                        }
                    }
                }
            },
            appendChildrenCache(parentOriginItem, items) {
                const existingChildIds = parentOriginItem.ChildrenIds ? [...parentOriginItem.ChildrenIds] : [];
                const childIdMap = {};

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

                const parentOriginItem = parentItem.origin;
                const isLoadMoreMode = parentOriginItem.Level === NodeLevel.WebApplication;
                const isArchiverTree = mainComponent.props.mode == TabIndex.Archive ? true : false;
                let postData = Object.assign({}, parentOriginItem, { Children: null, ChildrenIds: null }, { IsArchiverTree: isArchiverTree});
                postData.SearchKey = mainComponent.props.searchKey;
                if ((postData.IsSearch || postData.Level === NodeLevel.Farm || postData.Level === NodeLevel.Root) && postData.SearchKey !== "") {
                    postData.IsSearch = true;
                    postData.Children = null;
                } else {
                    postData.IsSearch = false;
                }

                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
                    currentItem = currentItem.Parent;
                }

                let url = "/API/TeamsSettingApi/BrowseSampleTree";     
                
                if(mainComponent.props.searchKey !== "" && parentOriginItem.Level === NodeLevel.Farm){
                    url = "/API/TeamsSettingApi/SearchContainerByPage";
                }

                if (isLoadMoreMode) {
                    url = "/API/TeamsSettingApi/SearchSiteCollectionLazyLoad";  
                    postData = {
                        IsArchiverTree: isArchiverTree,
                        SourceFlag: SourceFlags.Teams,
                        ContainerId: parentOriginItem.Id,    
                        LastUrl: parentItem.pagerIndex == 0 ? null : parentOriginItem.LastUrl,
                        PageSize: 15,
                    } 
                }
                        
                $$.fetch.post(url, postData).then((data)=>{
                    let items = data.Children;
                    if (mainComponent.updateProps.IconStatus) {
                        data.IconStatus = RM.deepcopy(mainComponent.updateProps).IconStatus;
                        mainComponent.updateProps = {};
                    }
                    items = mainComponent.filterItems(items);
                    if (isLoadMoreMode && parentItem.pagerIndex > 0) {
                        this.appendChildrenCache(parentOriginItem, items);
                    } else {
                        this.removeCache(parentOriginItem);
                        if (items && items.length > 0) {
                            parentOriginItem.ChildrenIds = items.map(item => {
                                item.Parent = parentOriginItem;
                                mainComponent.treeCache[item.Id] = item;
                                return item.Id;
                            });
                        } else {
                            parentOriginItem.ChildrenIds = [];
                        }
                    }

                    if (isLoadMoreMode) {
                        parentOriginItem.Children = null;
                        data.Children = null;
                        data.ChildrenIds = parentOriginItem.ChildrenIds;
                    }

                    funcSuccess(items, data);  
                }).catch(funcFail);

                return [];
            },
            onNodeSelected(item) {
                const funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },
            onNodeRefresh(){
                const treeCacheDataList = mainComponent.getTreeData();
                let exitSelectedNode = false;
                for (let item of treeCacheDataList) {
                    if (item.CheckNumber == 1) {
                        exitSelectedNode = true;
                        break;
                    }
                }
                const funcChange = mainComponent.props.onNodeRefresh;
                if (funcChange) {
                    funcChange(exitSelectedNode);
                }
            }
        };
    };

    getDesignLists() {
        const url = "/api/DAMApi/GetSPDesignLists";
        $.ajax({
            type: "POST",
            url,
            data: [],
            async: true,
            success: (data) => {
                this.designLists = $.parseJSON(data); // Fortify Issue Type: JSON Injection; Sink Details: tree data; Ignore Reason: There is a correspondence between front-end and back-end objects
            },
            error: (msg) => {},
            dataType: "json"
        });
    }

    initTreeData(treeData) {
        if (treeData && treeData.length > 0) {
            this.setTreeData(treeData);
        } else if (this.initDataStr) {
            this.setTreeData([$.parseJSON(this.initDataStr)]);
        } else {
            const that = this;
            const url = "/api/TeamsSettingApi/GetTeamsTreeInitData";
            $.ajax({
                type: "POST",
                url,
                data: [],
                async: true,
                success: (data) => {
                    that.initDataStr = data;
                    if (!that.props.treeData || that.props.treeData.length == 0) {
                        that.setTreeData([$.parseJSON(data)]);  // Fortify Issue Type: JSON Injection; Sink Details: init tree data;  Ignore Reason: There is a correspondence between the front-end and back-end objects.
                    }
                },
                error: (msg) => {},
                dataType: "json"
            });
        }
    }

    getTreeData() {
        const treeData = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                const newItem = Object.assign({}, this.treeCache[itemId]);
                newItem.Parent = null;
                treeData.push(newItem);
            }
        }
        return treeData;
    }

    filterItems(items) {
        const filteredItems = new Array();
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            // Filtering system node
            if (!item.Hidden && item.Level != NodeLevel.Apps && item.Level != NodeLevel.Items) {
                // Filter designLists (which could be libraries) in the List and configuration files.
                let index = 0;
                let uniqueName = '';
                if (item.FullPath) {
                    index = item.FullPath.lastIndexOf('/');
                    uniqueName = item.FullPath.substr(index + 1) + item.TemplateId;
                    if (item.TemplateId == 600) {
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
                const child = this.treeCache[childId];
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
            const url = "/api/TeamsSettingApi/SearchContainerByPage";   
            const rootItemForsearch = RM.deepcopy(this.rootItem);
            const isArchiverTree = this.props.mode == TabIndex.Archive ? true : false;
            rootItemForsearch.SearchKey = keywords;
            rootItemForsearch.PageIndex = 0;
            rootItemForsearch.PageSize = 15;
            rootItemForsearch.Children = null;
            rootItemForsearch.IsSearch = true;
            rootItemForsearch.IsArchiverTree = isArchiverTree;
            const data = {
                url,
                data: rootItemForsearch,
            }; 
            const result = await fetchUtility(data);
            if (result && LicenseHelper.EnableRecordsArchiver()) {
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
        let selectedNodes = this.treeContext.selectedNodes;
        if (selectedNodes) {
            for (const key in selectedNodes) {
                const selNode = selectedNodes[key];
                if (updateProps) {
                    if (isReload) {
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
        return (
            <$g.TreeView
                id="teamsTree"
                classicMode
                items={this.state.items}
                treeContext={this.treeContext}
            />
        );
    }
}

CRMTeamsTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func,
    searchKey: PropTypes.string,
};

CRMTeamsTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null,
    searchKey: PropTypes.string,
};

export default CRMTeamsTree;
