import { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNodeContent from '../../NodeContents/DefaultNodeContent';

class TreeReqObj {
    constructor(nodeId, nodeType, pageIndex, pageSize, sourceFlag, containerId, forPhysicalView) {
        this.NodeId = nodeId;
        this.NodeType = nodeType;
        this.PageIndex = !pageIndex ? 1 : (pageIndex+1);
        this.PageSize = 15;
        this.SourceFlag = sourceFlag;
        this.ContainerId = containerId;
        this.ExcludeBuiltIn = true;
        this.ForPhysicalView = forPhysicalView;
    }
}

class TermTree extends Component {
    constructor(props) {
        super(props);

        this.state = {
            treeData: []
        };
        
        this.selectedNodeItem = null;
        this.treeContext = this.getTreeContext();

        this.initRootNodeData();
    }


    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.rootItem != this.props.rootItem) {
            this.initRootNodeData();
        }
    }

    getTreeContext() {
        let self = this;
        let rootItem = self.props.rootItem;
        return {
            nodeContentComponent: TreeNodeContent,
            singleSelection: true,
            spaceNoSelection: true,
            transToTreeNodeObject(oitem) {
                let isRoot = oitem.UniqueId == rootItem.nodeId || (oitem.Type == rootItem.nodeType && oitem.Id == rootItem.nodeId);
                return {
                    origin: oitem,
                    treeId: `default-term-tree`,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Type,
                    nodeClass: null,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: (isRoot && !rootItem.allowSelected) || oitem.IsDeprecated || oitem.IsExpired,
                    checked: oitem.Checked || oitem.UniqueId === self.props.uniqueId,
                    expanded: isRoot && rootItem.expandDefault,
                    loaded: oitem.subTermCount == 0 || !!oitem.subTerms,
                    items: oitem.subTerms,
                    hasChildren: oitem.subTermCount > 0,
                    pagerByServer: true,
                    itemsCount: oitem.subTermCount,
                    pagerIndex: 0,
                    pagerSize: 15,
                    enableContextMenu: false
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.Checked = item.checked;
                oitem.Loaded = item.loaded;
                oitem.Expanded = item.expanded;
                oitem.PagerIndex = item.pagerIndex;
                oitem.PagerSize = item.pagerSize;
            },
            getNodeIconClass(oitem) {
                switch (oitem.Type) {
                    case 'TermSet':
                        return 'ra-tree-icon fia-term-set';
                    case 'Term':
                    default: 
                    {
                        let iconclass = 'ra-tree-icon fia-term';
                        if (oitem.IsDeprecated) {
                            iconclass += "-retired-b";
                        } else if (oitem.IsExpired) {
                            iconclass += "-retired-b";
                        }
                        return iconclass;
                    }
                }
            },
            sortChild(a, b) {
                if (a.Name == b.Name) {
                    return 0;
                } else if (a.Name.toLowerCase() > b.Name.toLowerCase()) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let option = {
                    url: `/api/BCMCommonSettingApi/GetChildrenTreeNodes`,
                    data: new TreeReqObj(
                        parentItem.nodeKey, 
                        parentItem.nodeType,
                        parentItem.pagerIndex,
                        parentItem.pagerSize,
                        self.props.sourceFlag,
                        self.props.containerId,
                        self.props.forPhysicalView
                    )
                };
                fetchUtility(option).then((res) => {
                    let oitems = $.parseJSON(res);
                    funcSuccess(oitems);
                }).catch((e) => {
                    funcFail(e);
                });
            },
            onNodeSelected(item) {
                let funcChange = self.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange([item.origin]);
                }
            },
            onNodeSelectedChange(selectedItems) {
                // let funcChange = self.props.onSelectedNodeChanged;
                // if (funcChange) {
                //     funcChange(selectedItems.map(item => {
                //         return item.origin;
                //     }));
                // }
            }
        };
    }

    initRootNodeData() {
        let rootItemInfo = this.props.rootItem;
        if(!rootItemInfo) {
            return;
        }
        let option = {
            url: `/api/BCMCommonSettingApi/GetRootNodeOfDefaultTermTree`,
            data: new TreeReqObj(
                rootItemInfo.nodeId, 
                rootItemInfo.nodeType,
                null,
                null,
                this.props.sourceFlag,
                this.props.containerId,
                this.props.forPhysicalView
            )
        };
        fetchUtility(option).then((res) => {
            let rootNodeItem = $.parseJSON(res);
            if(rootItemInfo.expandDefault) {
                let option = {
                    url: `/api/BCMCommonSettingApi/GetChildrenTreeNodes`,
                    data: new TreeReqObj(
                        rootNodeItem.Id, 
                        rootNodeItem.Type,
                        null,
                        null,
                        this.props.sourceFlag,
                        this.props.containerId,
                        this.props.forPhysicalView
                    )
                };
                fetchUtility(option).then((res) => {
                    let childNodeItems = $.parseJSON(res);
                    rootNodeItem.subTerms = childNodeItems;
                    this.setState({treeData: [rootNodeItem]});
                }).catch((e) => {
                    
                });
            } else {
                this.setState({treeData: [rootNodeItem]});
            }
        }).catch((e) => {

        });
    }

    getTreeData() {
        let option = {
            url: `/api/BCMCommonSettingApi/GetChildrenTreeNodes`,
            data: new TreeReqObj(
                this.props.rootItem.nodeId,
                this.props.rootItem.nodeType,
                null,
                null,
                this.props.sourceFlag,
                this.props.containerId,
                this.props.forPhysicalView
            )
        };
        fetchUtility(option).then((res) => {
            let item = $.parseJSON(res);
            this.setState({treeData: [item]});
        }).catch((e) => {

        });
        $.ajax({
            type: "GET",
            url: "/api/BCMCommonSettingApi/GetChildrenTreeNodes",
            contentType: "application/json;charset=utf-8",
            data: [],
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                this.treeContext.searchKey = "";
                this.resetTreeData(data);
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    render() {
        return <div>          
            <$g.TreeView
                items={this.state.treeData}
                treeContext={this.treeContext}
            />
        </div>;
    }
}

TermTree.propTypes = {
    rootItem: PropTypes.object,
    onSelectedNodeChanged: PropTypes.func,
    sourceFlag: PropTypes.number,
    containerId: PropTypes.string,
    forPhysicalView: PropTypes.string
};
TermTree.defaultProps = {
    rootItem: {
        nodeId: 1,
        nodeType: "TermSet",
        allowSelected: false,
        expandDefault: true,
        sourceFlag: null,
        containerId: null,
        forPhysicalView: null
    },
    onSelectedNodeChanged: (terms) => { 
        //console.log("all selected terms: ", terms);
    }
};

export default TermTree;