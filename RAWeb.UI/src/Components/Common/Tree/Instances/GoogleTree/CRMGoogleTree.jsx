import { Component } from "react";
import PropTypes from "prop-types";
import { NodeLevel } from "../../../../../Constants/DAEnums";
import GoogleNodeContent from "../../NodeContents/CRM/GoogleNodeContent";
import CRMCommonUtil from "../../../../BCM/ContentRepositoryManagement/Common/CRMCommonUtil";

const pageSize = 15;
class CRMGoogleTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            items: [],
        };

        let mainComponent = this;
        this.searchKey = "";
        this.initDataStr = null;
        this.selectedNodeItem = null;
        this.treeCache = [];
        this.updateProps = {};
        this.notAllowedSelected = [
            NodeLevel.Root,
        ];
        this.treeContext = {
            nodeContentComponent: GoogleNodeContent,
            singleSelection: true,
            showrRightArrow: true,
            transToTreeNodeObject(oitem) {
                let children = this.getViewChildren(oitem);
                let isLeaf = !mainComponent.notAllowedSelected.includes(
                    oitem.Level
                );
                let pagedByServer = oitem.Level == NodeLevel.Root || CRMCommonUtil.isGoogleContainer(oitem);
                let isLeafNode = CRMCommonUtil.isGoogleDriveItem(oitem);
                let pagerSize = pageSize;
                const hasSearchKey = mainComponent.props.searchKey !== "";
                const specialLevels = new Set([NodeLevel.Root, NodeLevel.GoogleUserDriveContainer, NodeLevel.GoogleSharedDriveContainer]);

                return {
                    origin: oitem,
                    treeId: `crm-google-tree`,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    isLeafNode: isLeafNode,
                    disableSelect: !isLeaf,
                    text: this.getText(oitem),
                    checked: oitem.CheckNumber == 1,
                    loaded: (hasSearchKey && specialLevels.has(oitem.Level)) ? true : this.isLoaded(oitem),
                    expanded: oitem.Expanded,
                    enableContextMenu: ![NodeLevel.GoogleUserDrive, NodeLevel.GoogleSharedDrive].includes(oitem.Level),
                    iconStatus: oitem.IconStatus,
                    items: children,
                    itemsCount: pagedByServer
                        ? oitem.ChildrenCount
                        : children
                        ? children.length > 0
                            ? children.length
                            : 1
                        : 0,
                    hasChildren: true,
                    treeSource: mainComponent.props.treeSource,
                    pagerByServer: pagedByServer,
                    pagerSize: pagerSize,
                    pagerIndex:
                        !oitem.PageIndex ||
                        oitem.PageIndex * pagerSize >= children.length
                            ? 0
                            : oitem.PageIndex,
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
                if (oitem.Children) {
                    for (let child of oitem.Children) {
                        child.Parent = oitem;
                        children.push(child);
                    }
                } else {
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
                let text = oitem.DisplayName;
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
                this.updateOriginObject(parentItem);
                let poItem = parentItem.origin;
                let postData = Object.assign(
                    {},
                    poItem,
                    { Children: null, ChildrenIds: null },
                );
                postData.SearchKey = mainComponent.props.searchKey;
                if (
                    postData.Level === NodeLevel.Root &&
                    postData.SearchKey !== ""
                ) {
                    postData.IsSearch = true;
                    postData.Children = null;
                } else {
                    postData.IsSearch = false;
                }
                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, {
                        Children: null,
                        ChildrenIds: null,
                    });
                    currentItem = currentItem.Parent;
                }

                let url = "/api/GoogleDriveSettingApi/BrowseSampleTree";

                $$.fetch
                    .post(url, postData)
                    .then((data) => {
                        this.removeCache(poItem);
                        let items = data.Children || [];
                        if (mainComponent.updateProps.IconStatus) {
                            data.IconStatus = RM.deepcopy(
                                mainComponent.updateProps
                            ).IconStatus;
                            mainComponent.updateProps = {};
                        }
                        if (items && items.length > 0) {
                            poItem.ChildrenIds = items.map((item) => {
                                item.Parent = poItem;
                                mainComponent.treeCache[item.Id] = item;
                                return item.Id;
                            });
                        } else {
                            poItem.ChildrenIds = [];
                        }
                        funcSuccess(items, data);
                    })
                    .catch(funcFail);
                return [];
            },
            onNodeSelected(item) {
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },

            onNodeRefresh() {
                let treeCacheDataList = mainComponent.getTreeData();
                let exitSelectedNode = false;
                for (let item of treeCacheDataList) {
                    if (item.CheckNumber == 1) {
                        exitSelectedNode = true;
                        break;
                    }
                }
                let funcChange = mainComponent.props.onNodeRefresh;
                if (funcChange) {
                    funcChange(exitSelectedNode);
                }
            },
        };

        this.initData(props.treeData);
        this.checkTenantKindWithM365AndGoogleLicense();
    }

    checkTenantKindWithM365AndGoogleLicense() {
        const option = {
            url: "/api/GoogleDriveSettingApi/CheckTenantKindWithM365AndGoogleLicense",
            method: "GET",
        };
        $$.loading(true);
        fetchUtility(option)
            .then((result) => {
                $$.loading(false);
                if (result) {
                    const args = {
                        width: "550px",
                        hideActions: false,
                        title: RMResx.RM_JS_Google_Guide_Title,
                        content: RMResx.RM_JS_Google_Guide_Content,
                        buttons: [
                            {
                                text: RMResx.RM_JS_Common_OK,
                                onClick: () => {
                                    $$.messagedialog(false);
                                    this.props.history.goBack();
                                },
                                primary: true
                            }
                        ],
                    };
                    $$.messagedialog(true, args);
                }
            })
            .catch((e) => {
                $$.loading(false);
            });
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
    }

    initData(treeData) {
        if (treeData && treeData.length > 0) {
            this.setTreeData(treeData);
        } else if (this.initDataStr) {
            this.setTreeData([$.parseJSON(this.initDataStr)]);
        } else {
            let that = this;
            let url = "/api/GoogleDriveSettingApi/GetGoogleDriveRootNode";
            $.ajax({
                type: "GET",
                url: url,
                data: [],
                async: true,
                success: (data) => {
                    that.initDataStr = data;
                    if (
                        !that.props.treeData ||
                        that.props.treeData.length == 0
                    ) {
                        that.setTreeData([$.parseJSON(data)]); // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
                    }
                },
                error: (msg) => {},
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
                treeData.push(newItem);
            }
        }
        return treeData;
    }

    setTreeData(data) {
        this.treeCache = [];
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Root) {
                this.rootItem = item;
            }
        });
        this.relateTreeItemChildren(this.rootItem);
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

    search = async (keywords) => {
        keywords = keywords.trim();
        $$.loading(true);
        let url = "/api/GoogleDriveSettingApi/BrowseSampleTree";
        let rootItemForsearch = RM.deepcopy(this.rootItem);
        rootItemForsearch.SearchKey = keywords;
        rootItemForsearch.PageIndex = 0;
        rootItemForsearch.PageSize = pageSize;
        rootItemForsearch.Children = null;
        rootItemForsearch.IsSearch = !!keywords;
        let data = {
            url: url,
            data: rootItemForsearch,
        };
        var result = await fetchUtility(data);
        this.setState({ items: [result] });
        $$.loading(false);
    };

    refreshSelectedNode = (updateProps, isReload) => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
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
                id="googleTree"
                classicMode
                items={this.state.items}
                treeContext={this.treeContext}
            />
        );
    }
}

CRMGoogleTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func,
    searchKey: PropTypes.string,
};
CRMGoogleTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null,
    searchKey: PropTypes.string,
};

export default CRMGoogleTree;
