import { Component } from 'react';
import PropTypes from 'prop-types';
import { NodeLevel } from '../../../../../Constants/DAEnums';
import GoogleNodeContent from "../../NodeContents/GoogleDestinationTreeNodeContent";
import { SourceFlags } from "../../../../../Constants/Constants";

class LocationGoogleTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            items: [],
        }
        this.treeCache = {};

        this.initTreeContext();
    }

    componentDidMount() {
        if (this.props.data && this.props.data.length > 0) {
            this.setTreeData(this.props.data);
        } else {
            this.initData();
        }
    }
    
    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.data != this.props.data) {
            this.setTreeData(nextProps.data);
        }
    }

    initTreeContext() {
        let mainComponent = this;
        this.treeContext = {
            nodeContentComponent: GoogleNodeContent,
            multiSelection: true,
            transToTreeNodeObject(item) {
                let isLeafNode = item.Level == NodeLevel.GoogleUserDrive || item.Level == NodeLevel.GoogleSharedDrive;
                let loaded = this.isLoaded(item);
                let children = this.getViewChildren(item);
                let pageSize = 10;
                const enableIncludeNew = false;
                const checked = item.CheckNumber == 1 && (!enableIncludeNew || !loaded || item.IncludeNew == 1);

                return {
                    origin: item,
                    nodeKey: item.Id,
                    nodeType: item.Level,
                    text: item.DisplayName,
                    isHasMixedStatus: false,
                    disableSelect: item.Level == NodeLevel.Root,
                    isLeafNode,
                    enableIncludeNew,
                    checked,
                    loaded,
                    includeNew: checked || item.IncludeNew == 1,
                    selectAll: checked || item.CheckNumber == 1,
                    expanded: item.Expanded,
                    items: children,
                    itemsCount: children ? children.length : 0,
                    hasChildren: true,
                    pagerByServer: false,
                    pagerIndex: !item.PageIndex || item.PageIndex * pageSize >= children.length ? 0 : item.PageIndex,
                    pagerSize: pageSize,
                    enableContextMenu: !isLeafNode,
                    treeSource: mainComponent.props.treeSource,
                };
            },
            updateOriginObject(item) {
                const originItem = item.origin;
                if (item.enableIncludeNew) {
                    originItem.CheckNumber = item.selectAll ? 1 : 0;
                    originItem.IncludeNew = item.includeNew ? 1 : 0;
                } else {
                    originItem.CheckNumber = item.checked ? 1 : 0;
                }
                originItem.Loaded = item.loaded;
                originItem.Expanded = item.expanded;
                originItem.PageIndex = item.pagerIndex;
                originItem.PageSize = item.pagerSize;
            },
            getAllChildren(originItem) {
                let children = [];
                if (originItem.ChildrenIds && originItem.ChildrenIds.length > 0) {
                    for (let childId of originItem.ChildrenIds) {
                        let child = mainComponent.treeCache[childId];
                        if (child) {
                            child.Parent = originItem;
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
            isLoaded(originItem) {
                if (originItem.Loaded === null || originItem.Loaded === undefined) {
                    return (originItem.ChildrenIds && (originItem.ChildrenIds.length > 0 || originItem.IncludeNew == 1));
                } else {
                    return originItem.Loaded;
                }
            },
            removeCache(poItem) {
                if (poItem.ChildrenIds) {
                    for(let childId of poItem.ChildrenIds){
                        let child = mainComponent.treeCache[childId];
                        if (child){
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
                let browseUrl = "/api/GoogleDriveSettingApi/BrowseSampleTreeForFullLevel";
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

                $$.fetch.post(browseUrl, postData).then((data) => {
                    this.removeCache(poItem);
                    let items = $.parseJSON(data);
                    if (items && items.length > 0) { 
                        poItem.ChildrenIds = items.map(item => {
                            item.Parent = poItem;
                            item.CheckNumber = poItem.CheckNumber;
                            mainComponent.treeCache[item.Id] = item;
                            return item.Id;
                        });
                    } else {
                        poItem.ChildrenIds = [];
                    }
                    funcSuccess(items);
                }).catch(funcFail);

                return [];    
            },
            onTreeChanged() {
                if (mainComponent.props.onTreeChanged) {
                    mainComponent.props.onTreeChanged();
                }
            },
            onNodeSelectedChange() {
                if (mainComponent.props.onNodeSelectedChange) {
                    mainComponent.props.onNodeSelectedChange();
                }
            }
        };
    }

    initData() {
        $.ajax({
            type: "GET",
            url: "/api/GoogleDriveSettingApi/GetGoogleDriveRootNode",
            data: [],
            async: true,
            success: (data) => {
                this.setTreeData([$.parseJSON(data)]); // Fortify Issue Type: JSON Injection; Sink Details: init data; Ignore Reason: 前后台对象存在对应关系
            },
            error: () => {},
            dataType: "json",
        });
    }

    getTreeData() {
        let selected = false;
        const treeItems = [];
        for (let itemId in this.treeCache) {
            if (this.treeCache[itemId]) {
                let newItem = RM.SimplifyObject(this.treeCache[itemId], null, ["Parent", "SearchChildrenIds"]);
                if (newItem.CheckNumber == 1 || newItem.IncludeNew == 1) {
                    selected = true;
                }
                treeItems.push(newItem);
            }
        }
        return { items: treeItems, selected: selected };
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
        return (
            <$g.TreeView
                classicMode
                items={this.state.items}
                treeContext={this.treeContext}
            />
        );
    }
}

LocationGoogleTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func,
    treeSource: PropTypes.number
};
LocationGoogleTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null,
    treeSource: SourceFlags.Google
};

export default LocationGoogleTree;