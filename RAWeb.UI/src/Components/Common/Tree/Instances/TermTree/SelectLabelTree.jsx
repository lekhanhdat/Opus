import { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNodeContent from '../../NodeContents/DefaultNodeContent';
import { TypeString } from '../../../../../Constants/Constants';

const NodeType = {
    Root: "Root",
    Label: "Label",
    TermGroup: 'TermGroup',
    TermSet: 'TermSet',
    Term: 'Term'
};

class LabelTree extends Component {
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
        return {
            nodeContentComponent: TreeNodeContent,
            spaceNoSelection: true,
            singleSelection: true,
            transToTreeNodeObject(oitem) {
                const disableSelectTermTypes = [NodeType.Root, NodeType.TermGroup, NodeType.TermSet];
                let itemsCount = oitem.subTermCount;
                if(this.searchKey){
                    itemsCount = !oitem.subTerms ? 0 : oitem.subTerms.length;
                }

                return {
                    origin: oitem,
                    treeId: `default-label-tree`,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Type,
                    nodeClass: null,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: disableSelectTermTypes.includes(oitem.Type) || oitem.IsDeprecated || oitem.IsExpired,
                    // isLeafNode: true,
                    checked: oitem.Checked || oitem.UniqueId === self.props.uniqueId,
                    expanded: oitem.Type == NodeType.Root || oitem.Type == NodeType.TermGroup || this.searchKey,
                    loaded: oitem.subTermCount == 0 || !!oitem.subTerms,
                    items: oitem.subTerms,
                    hasChildren: oitem.subTermCount > 0,
                    pagerByServer: !this.searchKey,
                    itemsCount: itemsCount,
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
                    case NodeType.Root:
                    case NodeType.TermGroup:
                        return 'ra-tree-icon fia-term-group';
                    case NodeType.TermSet:
                        return 'ra-tree-icon fia-term-set';
                    case NodeType.Term:
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
                if (a.Type == NodeType.TermGroup || a.Name == b.Name) {
                    return 0;
                } else if (a.Name.toLowerCase() > b.Name.toLowerCase()) {
                    return 1;
                } else {
                    return -1;
                }
            },

            onNodeSelected(item) {
                let funcChange = self.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange([item.origin]);
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                const pageIndex = !parentItem.pagerIndex ? 1 : parentItem.pagerIndex + 1
                const option = {
                    type: "POST",
                    url: "/api/BCMCommonSettingApi/GetChildrenTreeNodes",
                    data: {
                        NodeId: parentItem.nodeKey,
                        NodeType: parentItem.nodeType,
                        PageSize: parentItem.pagerSize,
                        PageIndex: pageIndex,
                        SourceFlag: self.props.sourceFlag,
                        ContainerId: self.props.nodeId
                    }
                }
                fetchUtility(option)
                    .then(data => {
                        let items = $.parseJSON(data); 
                        funcSuccess(items);
                    })
                    .catch((msg) => {
                        funcFail(msg.responseText);
                    });
            },
        };
    }
   
    resetTreeData(data) {
        let root = {
            Name: RMResx.RM_JS_TM_RootTerms,
            Type: TypeString.ROOT,
            Id: TypeString.ROOT,
            subTerms: $.parseJSON(data),
        };
        root.subTermCount = root.subTerms.length;
        this.setState({ treeData: [root] });
    }

    initRootNodeData() {
        let option = {
            url: `/api/TermManagementApi/GetGoogleTermsTreeApplySetting`,
            method: "POST",
            data: {
                NodeId: this.props.nodeId
            }
        };
        fetchUtility(option).then((res) => {
        const result = $.parseJSON(res);  
        if (result) {
            this.treeContext.searchKey = "";
            this.resetTreeData(result);
        }
        }).catch((e) => {

        });
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        var reg3 = new RegExp("#", "ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        str = str.replace(reg3, "%23");
        return str;
    }

    onSearch = (args) => {
        let searchValue = (args || "").trim();

        if (searchValue === "") { 
            this.initRootNodeData();
        } else {
            this.searchData(args);
        }
    };

    searchData(key) {
        key = !key ? "" : key.trim();
        if (key.length == 0) {
            this.initRootNodeData();
        } else {
            const option = {
                type: "POST",
                url: "/api/TermManagementApi/GetGoogleTermsTreeApplySetting",
                data: {
                    SearchKey: this.replaceSpecialCharacters(key),
                    NodeId: this.props.nodeId
                }
            }

            $$.loading(true);
            fetchUtility(option)
                .then(data => {
                    $$.loading(false);
                    this.treeContext.searchKey = key;
                    this.resetTreeData(JSON.parse(data));
                })
                .catch(() => {
                    $$.loading(false);
                });
        }
    }

    render() {
        return <div>          
            <R.Searchbox width={380} height={34} placeholder={RMResx.RM_BCM_SearchByLabel} disabled={false} onSearch={this.onSearch}/>
            <$g.TreeView
                items={this.state.treeData}
                treeContext={this.treeContext}
            />
        </div>;
    }
}

LabelTree.propTypes = {
    rootItem: PropTypes.object,
    onSelectedNodeChanged: PropTypes.func,
    sourceFlag: PropTypes.number,
    containerId: PropTypes.string,
    nodeId: PropTypes.string
};
LabelTree.defaultProps = {
    rootItem: {
        nodeId: 1,
        nodeType: NodeType.Root,
        allowSelected: false,
        expandDefault: true,
        sourceFlag: null,
        containerId: null,
    },
    onSelectedNodeChanged: (terms) => { 
        //console.log("all selected terms: ", terms);
    }
};

export default LabelTree;