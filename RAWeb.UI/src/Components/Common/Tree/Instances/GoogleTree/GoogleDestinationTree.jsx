import { Component } from 'react';
import PropTypes from 'prop-types';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import GoogleNodeContent from "../../NodeContents/GoogleDestinationTreeNodeContent";
import CRMCommonUtil from '../../../../BCM/ContentRepositoryManagement/Common/CRMCommonUtil';

class GoogleDestinationTree extends Component {
    constructor(props) {
        super(props);

        this.state = { items: [] };

        this.initDataStr = null;
        this.treeCache = [];
        this.initTreeContext();
    }

    componentDidMount() {
        if (this.props.treeData.length) {
            this.setTreeData(this.props.treeData);
        } else {
            this.initData();
        }
    }

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            browseTreeReqUrl: "/api/GoogleDriveSettingApi/BrowseSampleTreeForRule",
            nodeContentComponent: GoogleNodeContent,
            shadowInitialNodelevel: NodeLevel.GoogleUserDriveContainer || NodeLevel.GoogleSharedDriveContainer,
            singleSelection: true,
            transToTreeNodeObject(item) {
                let children = this.getViewChildren(item);
                let isLeaf = false;
                return {
                    origin: item,
                    nodeKey: item.Id,
                    nodeType: item.Level,
                    isLeafNode: isLeaf,
                    disableSelect: item.Level == NodeLevel.Root || CRMCommonUtil.isGoogleContainer(item),
                    text: item.DisplayName,
                    checked: item.CheckNumber == 1,
                    loaded: item.Loaded,
                    expanded: item.Expanded,
                    enableContextMenu: !isLeaf,
                    items: children,
                    itemsCount: children.length > 0 ? children.length : 1,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerSize: 10,
                    pagerIndex: !item.PageIndex || item.PageIndex * 10 >= children.length ? 0 : item.PageIndex,
                };
            },
            updateOriginObject(item) {
                let oItem = item.origin;
                oItem.CheckNumber = item.checked ? 1 : 0;
                oItem.Loaded = item.loaded;
                oItem.Expanded = item.expanded;
                oItem.PageIndex = item.pagerIndex;
                oItem.PageSize = item.pagerSize;
            },
            getAllChildren(item) {
                let children = [];
                if (item.ChildrenIds && item.ChildrenIds.length > 0) {
                    for (let childId of item.childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = item;
                            children.push(child);
                        }
                    }
                }
                return children;
            },
            getViewChildren(item) {
                let children = [];
                let childrenIds = item.ChildrenIds;
                if (childrenIds) {
                    for (let childId of childrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = item;
                            children.push(child);
                        }
                    }
                }
                return children;
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
                
                let postData = Object.assign({}, poItem, { Children: null, ChildrenIds: null });
                let currentItem = postData;
                while (currentItem.Parent) {
                    currentItem.Parent = Object.assign({}, currentItem.Parent, {
                        Children: null,
                        ChildrenIds: null,
                    });
                    currentItem = currentItem.Parent;
                }
                $$.fetch.post(this.browseTreeReqUrl, postData).then((data)=>{
                    this.removeCache(poItem);
                    let items = $.parseJSON(data);
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
                }).catch(funcFail);
                //return children node items
                return [];
            },
            onNodeSelected(item) {
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
            },
        };
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.treeData != this.props.treeData) {
            this.initData(nextProps.treeData);
        }
    }

    initData() {
        if (this.initDataStr) {
            this.setTreeData([$.parseJSON(this.initDataStr)]);
        } else {
            let that = this;
            $.ajax({
                type: "GET",
                url: "/api/GoogleDriveSettingApi/GetGoogleDriveRootNode",
                data: [],
                async: true,
                success: (data) => {
                    that.initDataStr = data;
                    if (!that.props.treeData || that.props.treeData.length == 0) {
                        that.setTreeData([$.parseJSON(data)]);  // Fortify Issue Type: JSON Injection; Sink Details: init data; Ignore Reason: 前后台对象存在对应关系
                    }
                },
                error: (msg) => {
                    //alert(msg.responseText);
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
        }
    }

    render() {
        return <$g.TreeView
            classicMode
            items={this.state.items}
            treeContext={this.treeContext}
        />;
    }
}

GoogleDestinationTree.propTypes = {
    treeData: PropTypes.array,
    onSelectedNodeChanged: PropTypes.func
};
GoogleDestinationTree.defaultProps = {
    treeData: [],
    onSelectedNodeChanged: null
};

export default GoogleDestinationTree;