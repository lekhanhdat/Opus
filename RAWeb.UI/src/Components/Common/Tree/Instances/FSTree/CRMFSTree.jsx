import { Component } from 'react';
import PropTypes from 'prop-types';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import FSNodeContent from '../../NodeContents/CRM/FSNodeContent';
import { LicenseHelper } from '../../../../../Utilities/CommonUtil';

const pageSize = 15;
class CRMFSTree extends Component {
    constructor(props) {
        super(props);
        this.state = {
            items: []
        };
        this.searchKey = "";
        let mainComponent = this;
        this.initDataStr = null;
        this.treeCache = [];
        this.updateProps = {};
        this.treeContext = {
            nodeContentComponent: FSNodeContent,
            singleSelection: true,
            showrRightArrow: true,
            transToTreeNodeObject(oitem) {
                let children = this.getViewChildren(oitem);
                let isLeaf = oitem.Level > NodeLevel.Farm;
                let isPagerBySever = LicenseHelper.EnableJPMCFileSystemFeature() ? oitem.Level == NodeLevel.WebApplication : false;
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    isLeafNode: !oitem.IsActive && oitem.Level == NodeLevel.FSFolder && oitem.IsCustomSetting,
                    disableSelect: !isLeaf,
                    text: this.getText(oitem),
                    checked: oitem.CheckNumber == 1,
                    loaded: this.isLoaded(oitem),
                    expanded: oitem.Expanded,
                    enableContextMenu: true,
                    disableSelect: !isLeaf || (isLeaf && oitem.IsDeletedFromLocal),
                    items: children,
                    iconStatus: oitem.IconStatus,
                    itemsCount: isPagerBySever ? oitem.ChildrenCount : (children.length > 0 ? children.length : 1),
                    hasChildren: true,
                    pagerByServer: isPagerBySever,
                    pagerSize: 15,
                    pagerIndex: !oitem.PageIndex || oitem.PageIndex * 10 >= children.length ? 0 : oitem.PageIndex
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
                if (oitem.Children && oitem.Children.length > 0) {
                    for (let child of oitem.Children) {
                        child.Parent = oitem;
                        children.push(child);
                    }
                    return children;
                }
                let childrenIds = this.searchKey && oitem.SearchChildrenIds
                    ? oitem.SearchChildrenIds : oitem.ChildrenIds;
                if (childrenIds) {
                    for (let childId of childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = oitem;
                            children.push(child);
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
                if (oitem.Loaded == null || oitem.Loaded == undefined) {
                    return ((oitem.Children && oitem.Children.length > 0) ||
                        (oitem.ChildrenIds && oitem.ChildrenIds.length > 0)
                        || (oitem.IncludeNew == 1));
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
                let postData = Object.assign({}, poItem, { Children: null, ChildrenIds: null });
                postData.SearchKey = mainComponent.props.searchKey;
                if (postData.Level === NodeLevel.Root && postData.SearchKey !== "") {
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
                
                fetchUtility({
                    url: "/api/FSSettingApi/FSBrowse",
                    method: "Post",
                    data: postData
                }).then((data) => {
                    this.removeCache(poItem);
                    let items = data.Children;
                    if (mainComponent.updateProps.IconStatus) {
                        data.IconStatus = RM.deepcopy(mainComponent.updateProps).IconStatus;
                        mainComponent.updateProps = {};
                    }
                    if (items && items.length > 0) {
                        poItem.ChildrenIds = items.map(item => {
                            item.Parent = poItem;
                            mainComponent.treeCache[item.Id] = item;
                            return item.Id;
                        });
                    } else {
                        poItem.ChildrenIds = [];
                    }
                    funcSuccess(items, data);
                }).catch(msg => {
                    funcFail(msg.responseText);
                });
                // $.ajax({
                //     type: "POST",
                //     // dataType: 'json',
                //     contentType: 'application/json',
                //     url: url,
                //     data: JSON.stringify(poItem),
                //     success: (data) => {
                //         this.removeCache(poItem);
                //         let items = $.parseJSON($.parseJSON(data).Extension);

                //         if (items && items.length > 0) {
                //             poItem.ChildrenIds = items.map(item => {
                //                 item.Parent = poItem;
                //                 mainComponent.treeCache[item.Id] = item;
                //                 return item.Id;
                //             });
                //         } else {
                //             poItem.ChildrenIds = [];
                //         }
                //         funcSuccess(items);
                //     },
                //     error: (msg) => {
                //         funcFail(msg.responseText);
                //     }
                // });
                //return children node items
                return [];
            },
            onNodeSelected(item) {
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },
            showRadio(item) {
                return item.nodeType == NodeLevel.List;
            },
            onActiveClick(isActive, item){
                if(mainComponent.props){
                    mainComponent.props.onActiveClick(isActive, item.origin);
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
    }

    componentDidMount() {
        this.initData(this.props.treeData);
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
            $.ajax({
                type: "POST",
                url: "/api/FSSettingApi/GetFSTreeInitData",
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

    setTreeData(data) {
        this.treeCache = [];
        $.each(data, (idx, item) => {
            this.treeCache[item.Id] = item;
            if (item.Level == NodeLevel.Farm) {
                this.rootItem = item;
            }
        });
        this.relateTreeItemChildren(this.rootItem);
        this.rootItem.Name = RMResx.RM_JS_SPS_FS_RootNode;
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
                if (text.toUpperCase().indexOf(this.treeContext.searchKey.toUpperCase()) > -1
                    || this.relateTreeItemSearchChildren(child)) {
                    matchChildrenIds.push(childId);
                }
            }
            item.SearchChildrenIds = matchChildrenIds;
        }
        return matchChildrenIds.length > 0;
    }

    search = async (keywords) => {
        this.searchKey = keywords.trim();
        if (this.searchKey && this.searchKey.length > 0) {
            $$.loading(true);
            let url = "/api/FSSettingApi/FSBrowse";
            let rootItemForsearch = RM.deepcopy(this.rootItem);
            rootItemForsearch.SearchKey = keywords;
            rootItemForsearch.PageIndex = 0;
            rootItemForsearch.PageSize = pageSize;
            rootItemForsearch.Children = null;
            rootItemForsearch.IsSearch = true;
            let data = {
                url: url,
                data: rootItemForsearch,
            };
            var result = await fetchUtility(data);
            this.setState({ items: [result] });
            $$.loading(false);
        } else {
            this.relateTreeItemChildren(this.rootItem);
            this.setState({ items: [this.rootItem] });
        }
    }

    refreshSelectedNode = (updateProps, isReload, isActive) => {
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
                    if(selNode.props.item.origin.Level == NodeLevel.FSFolder){
                        updateProps.IsActive = isActive;
                        updateProps.Expanded = isActive;
                        selNode.props.item.isLeafNode = !isActive;
                        selNode.props.item.expanded = isActive;
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
            id="fsTree"
            classicMode
            items={this.state.items}
            treeContext={this.treeContext}
        />;
    }
}

CRMFSTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func
};
CRMFSTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null
};

export default CRMFSTree;