import { Component } from "react";
import PropTypes from "prop-types";

import { NodeLevel } from "../../../../../Constants/DAEnums";
import TeamsDestinationTreeNodeContent from "../../NodeContents/TeamsDestinationTreeNodeContent";
import { TabIndex } from "../../../../BCM/ContentRepositoryManagement/CRMForTeams";
import { DataSourceType } from "../../../../ArchiveRC/Constants";
import { checkPermission } from "../../../../../Utilities/permissionManager";
import { LicenseHelper } from "../../../../../Utilities/CommonUtil";

class TeamsDestinationTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            items: [],
        };

        const mainComponent = this;
        this.initDataStr = null;
        this.selectedNodeItem = null;
        this.DesignLists = [];
        this.treeCache = [];
        this.initRequestUrl();
        this.treeContext = {
            browseTreeReqUrl: this.browseTreeReqUrl,
            nodeContentComponent: TeamsDestinationTreeNodeContent,
            shadowInitialNodelevel: NodeLevel.Office365GroupEntire,
            singleSelection: true,
            transToTreeNodeObject(originItem) {
                const children = this.getViewChildren(originItem);
                const isLeafNode = originItem.Level === NodeLevel.List;
                const isLeaf = originItem.Level === NodeLevel.List || originItem.Level === NodeLevel.SiteCollection || originItem.Level === NodeLevel.Site;
                return {
                    origin: originItem,
                    nodeKey:
                        originItem.Level === NodeLevel.Office365GroupEntire
                            ? originItem.TeamsId
                            : originItem.Id,
                    nodeType: originItem.Level,
                    isLeafNode: isLeafNode,
                    disableSelect: mainComponent.props.restoreTree ? !isLeaf : !isLeafNode,
                    text: this.getText(originItem),
                    checked: originItem.CheckNumber == 1,
                    loaded: this.isLoaded(originItem),
                    expanded: originItem.Expanded,
                    enableContextMenu: !isLeafNode,
                    items: children,
                    itemsCount: children.length > 0 ? children.length : 1,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerSize: 10,
                    pagerIndex:
                        !originItem.PageIndex ||
                        originItem.PageIndex * 10 >= children.length
                            ? 0
                            : originItem.PageIndex,
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
            getViewChildren(originItem) {
                const children = [];
                const childrenIds =
                    this.searchKey && originItem.SearchChildrenIds
                        ? originItem.SearchChildrenIds
                        : originItem.ChildrenIds;
                if (childrenIds) {
                    for (let childId of childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = originItem;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            getText(originItem) {
                let text = originItem.Name;
                if (text == "." && originItem.Level == NodeLevel.Site) {
                    text = RMResx.RM_JS_DAM_RootSiteName.format(
                        originItem.Title
                    );
                }
                if (originItem.TeamName) {
                    text = "(" + originItem.TeamName + ") " + originItem.Name;
                }
                if (originItem.OrphanNameSuffix) {
                    text = originItem.Name + originItem.OrphanNameSuffix;
                }
                return text;
            },
            isLoaded(oitem) {
                //RM 3.1 版本 没有Loaded属性,3.2添加该属性，该属性保存在数据库中，用于记录节点“是否加载过”
                if (oitem.Loaded == null || oitem.Loaded == undefined) {
                    return (
                        (oitem.ChildrenIds && oitem.ChildrenIds.length > 0) ||
                        oitem.IncludeNew == 1
                    );
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
                poItem.PageIndex = parentItem.pagerIndex;
                const isArchiverTree =
                    mainComponent.props.mode == TabIndex.Archive ? true : false;
                const isEnableTeams = checkPermission("Source_Teams", RM.UserResources) && LicenseHelper.HasUpgradeTeams();
                let postData = Object.assign(
                    {},
                    poItem,
                    { Children: null, ChildrenIds: null },
                    { IsArchiverTree: isArchiverTree },
                    { IsTeams: true },
                    { IsEnableTeams: isEnableTeams },
                );
                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, {
                        Children: null,
                        ChildrenIds: null,
                    });
                    currentItem = currentItem.Parent;
                }

                $$.fetch
                    .post(this.browseTreeReqUrl, postData)
                    .then((data) => {
                        this.removeCache(poItem);
                        let items = $.parseJSON(data);
                        // let items = data.Children; // Update later
                        items = mainComponent.filterItems(items);
                        if (items && items.length > 0) {
                            poItem.ChildrenIds = items.map((item) => {
                                item.Parent = poItem;
                                mainComponent.treeCache[item.Id] = item;
                                return item.Id;
                            });
                        } else {
                            poItem.ChildrenIds = [];
                        }
                        funcSuccess(items);
                    })
                    .catch(funcFail);
                //return children node items
                return [];
            },
            onNodeSelected(item) {
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    const param = { ...item.origin, Type: DataSourceType.Teams };
                    funcChange(param);
                }
            },
            showRadio(item) {
                return item.nodeType == NodeLevel.List;
            },
        };

        this.getDesignLists();
        // this.initData(props.treeData);
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.treeData != this.props.treeData) {
            this.initData(nextProps.treeData);
        }
    }

    componentDidMount() {
        this.initData(this.props.treeData);
    }

    initRequestUrl() {
        const apiPrefix = this.getRequestApiPrefix();
        this.browseTreeReqUrl = `${apiPrefix}/BrowseAllTree`;
        this.getInitTreeDataReqUrl = `/api/TeamsSettingApi/GetTeamsTreeInitData`;
        this.getDesignListsReqUrl = `/api/DAMApi/GetSPDesignLists`;
    }

    getDesignLists() {
        const reqUrl = this.getDesignListsReqUrl;
        $.ajax({
            type: "POST",
            url: reqUrl,
            //contentType: 'application/json;charset=utf-8',
            data: [],
            async: true,
            success: (data) => {
                this.DesignLists = $.parseJSON(data); // Fortify Issue Type: JSON Injection; Sink Details: tree data; Ignore Reason: 前后台对象存在对应关系
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json",
        });
    }

    getRequestApiPrefix() {
        if (!checkPermission("Source_SP", RM.UserResources) && !checkPermission("Source_OneDrive", RM.UserResources)) {
            return "/API/TeamsSettingApi";
        }
        if (checkPermission("Source_OneDrive", RM.UserResources)) {
            return "/API/OneDriveSettingApi";
        }
        return "/API/SPSettingApi";
    }

    initData(treeData) {
        if (treeData && treeData.length > 0) {
            this.setTreeData(treeData);
        } else if (this.initDataStr) {
            this.setTreeData([$.parseJSON(this.initDataStr)]);
        } else {
            let that = this;
            $.ajax({
                type: "POST",
                url: that.getInitTreeDataReqUrl,
                //contentType: 'application/json;charset=utf-8',
                data: [],
                async: true,
                success: (data) => {
                    that.initDataStr = data;
                    if (
                        !that.props.treeData ||
                        that.props.treeData.length == 0
                    ) {
                        that.setTreeData([$.parseJSON(data)]); // Fortify Issue Type: JSON Injection; Sink Details: init data; Ignore Reason: 前后台对象存在对应关系
                    }
                },
                error: (msg) => {
                    //alert(msg.responseText);
                },
                dataType: "json",
            });
        }
    }
    getTreeData() {
        let treeData = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                let newItem = Object.assign({}, this.treeCache[itemId]);
                newItem.Parent = null;
                newItem.Type = DataSourceType.Teams;
                treeData.push(newItem);
            }
        }
        return treeData;
    }

    filterItems(items) {
        var filteredItems = new Array();
        let listTemplateIdWhiteList = this.getListTemplateIdWhiteList();
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            //过滤系统节点
            if (!item.Hidden && item.Level != NodeLevel.Apps) {
                //过滤List和配置文件中的designLists（有可能是library）
                var index = 0;
                var uniqueName = "";
                if (item.FullPath) {
                    index = item.FullPath.lastIndexOf("/");
                    uniqueName =
                        item.FullPath.substr(index + 1) + item.TemplateId;
                    if (item.TemplateId == 600) {
                        continue;
                    }
                }
                if (
                    item.Level == NodeLevel.List &&
                    ($.inArray(item.TemplateId, listTemplateIdWhiteList) ==
                        -1 ||
                        $.inArray(uniqueName, this.DesignLists) != -1)
                ) {
                    //过滤designLists
                    continue;
                }
                filteredItems.push(items[i]);
            }
        }
        return filteredItems;
    }

    getListTemplateIdWhiteList() {
        return [
            101, //DocumentLibrary
            1302, //RecordLib
            700, //MySiteDocumentLibrary
        ];
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
                if (text == "." && child.Level == NodeLevel.Site) {
                    text = child.Title;
                }
                if (
                    text.indexOf(this.treeContext.searchKey) > -1 ||
                    this.relateTreeItemSearchChildren(child)
                ) {
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

TeamsDestinationTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func,
};
TeamsDestinationTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null,
};

export default TeamsDestinationTree;
