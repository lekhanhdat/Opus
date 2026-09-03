import { Component } from 'react';
import PropTypes from 'prop-types';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import { TreeType } from "../../../../../Constants/Constants";
import FSDestinationTreeNodeContent from '../../NodeContents/FSDestinationTreeNodeContent';

class FSDestinationTree extends Component {
    constructor(props) {
        super(props);
        this.state = {
            items: []
        };
        let mainComponent = this;
        this.initDataStr = null;
        this.isFilterTree = props.treeType == TreeType.Filter;
        this.treeCache = [];
        this.treeContext = {
            nodeContentComponent: FSDestinationTreeNodeContent,
            singleSelection: true,
            transToTreeNodeObject(oitem) {
                let children = this.getViewChildren(oitem);
                let isLeaf = oitem.Level > (mainComponent.filterTree ? NodeLevel.Farm : NodeLevel.WebApplication);
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Level,
                    isLeafNode: false,
                    disableSelect: !isLeaf,
                    text: this.getText(oitem),
                    checked: oitem.CheckNumber == 1,
                    loaded: this.isLoaded(oitem),
                    expanded: oitem.Expanded,
                    enableContextMenu: true,
                    items: children,
                    itemsCount: children.length > 0 ? children.length : 1,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerSize: 10,
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
                    return (oitem.ChildrenIds && oitem.ChildrenIds.length > 0)
                        || (oitem.IncludeNew == 1);
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
                let option = {
                    url: this.isFilterTree ? "/api/FSSettingApi/FSBrowseTreeWithoutSetting" : "/api/FSSettingApi/FSMoveBrowse",
                    method: "Post",
                    data: poItem
                };
                fetchUtility(option).then((data) => {
                    this.removeCache(poItem);
                    let items = data.Children;

                    if (items && items.length > 0) {
                        poItem.ChildrenIds = items.map(item => {
                            item.Parent = poItem;
                            mainComponent.treeCache[item.Id] = item;
                            return item.Id;
                        });
                    } else {
                        poItem.ChildrenIds = [];
                    }
                    funcSuccess(items);
                }).catch(msg => {
                    funcFail(msg.responseText);
                });
                // $.ajax({
                //     type: "POST",
                //     dataType: 'json',
                //     contentType: 'application/json',
                //     url: url,
                //     data: JSON.stringify(poItem),
                //     success: (data) => {
                        
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
            }
        };
    }

    componentDidMount() {
        this.initData(this.props.treeData);
    }


    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.treeData != this.props.treeData) {
            this.initData(nextProps.treeData);
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
                //contentType: 'application/json;charset=utf-8',
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

    render() {
        return <$g.TreeView
            id="fsTree"
            classicMode
            items={this.state.items}
            treeContext={this.treeContext}
        />;
    }
}

FSDestinationTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func
};
FSDestinationTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null
};

export default FSDestinationTree;