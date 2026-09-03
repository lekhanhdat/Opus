import { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNodeContent from '../../NodeContents/RC/TermNodeContent';

const DEFAULT_PAGE_SIZE = 15;

const NodeType = {
    Root: "Root",
    TermGroup: "TermGroup",
    TermSet: "TermSet",
    Term: "Term",
};

class ClassCodeTree extends Component {
    constructor(props) {
        super(props);

        this.selectedClassCodeIds = new Set();
        this.initTreeContext();

        this.state = {
            treeData: []
        };
    }

    componentDidMount() {
        this.isAllowSearchChange = false;
        this.initTreeData();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if ((nextProps.searchKey != this.props.searchKey) && this.isAllowSearchChange) {
            if (nextProps.searchKey) {
                this.search(nextProps.searchKey);
            } else {
                this.initTreeData();
            }
        }
    }

    componentDidUpdate(){
        this.isAllowSearchChange = true;
    }

    applyCheckedState(items = []) {
        items.forEach((item) => {
            item.IsChecked = this.selectedClassCodeIds.has(item.UniqueId);
            if (item.subTerms && item.subTerms.length > 0) {
                this.applyCheckedState(item.subTerms);
            }
        });
    }

    initTreeContext() {
        let self = this;
        this.treeContext = {
            multiSelection: true,
            allowSelectedWithoutChildren: true,
            nodeContentComponent: TreeNodeContent,
            transToTreeNodeObject(oitem) {
                let itemsCount = (!oitem.subTerms ? 0 : oitem.subTerms.length) || oitem.subTermCount || 0;
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Type,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: oitem.Type !== NodeType.Term || oitem.IsDeprecated || oitem.IsExpired,
                    checked: oitem.IsChecked,
                    expanded: true,
                    loaded: oitem.Type !== NodeType.TermSet,
                    items: oitem.subTerms,
                    hasChildren: itemsCount > 0,
                    pagerByServer: false,
                    itemsCount: oitem.subTermCount,
                    pagerIndex: !oitem.pageIndex ? 0 : oitem.pageIndex,
                    pagerSize: DEFAULT_PAGE_SIZE,
                    enableContextMenu: false,
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.IsChecked = !item.disableSelect && item.checked;
                oitem.expand = item.expanded;
                oitem.pageIndex = item.pagerIndex;
                oitem.PageSize = item.pagerSize;
            },
            getAllChildren(oitem) {
                let children = [];
                let pId = oitem.UniqueId;
                for (const nodeId in this.treeCache) {
                    let child = this.treeCache[nodeId];
                    if (child && pId === child.ParentId) {
                        children.push(child);
                    }
                }
                if(oitem.subTerms) {
                    return children.concat(oitem.subTerms);
                } else {
                    return children;
                }
            },
            getNodeIconClass(oitem) {
                switch (oitem.Type) {
                    case 'Root':
                    case 'TermGroup':
                        return 'ra-tree-icon fia-term-group';
                    case 'TermSet':
                        return 'ra-tree-icon fia-term-set';
                    case 'Term': {
                        let iconclass = 'ra-tree-icon fia-term';
                        if (oitem) {
                            if (oitem.IsDeprecated) {
                                iconclass += "-retired-b";
                            } else if (oitem.IsExpired) {
                                iconclass += "-retired-b";
                            }
                        }
                        return iconclass;
                    }
                    default:
                        return '';
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
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                const currentPageIndex = (parentItem.pagerIndex ?? 0) + 1;
                let option = {
                    url: "/api/TermManagementApi/GetClassCodeGroup",
                    data: {
                        TermSetId: self.props.termSetId,
                        SearchKey: self.treeContext.searchKey,
                        PageIndex: currentPageIndex,
                        PageSize: DEFAULT_PAGE_SIZE
                    }
                };
                fetchUtility(option).then((res) => {
                    let result = $.parseJSON(res);
                    const data = result && result.Data ? result.Data : [];
                    const termSetItems = data[0]?.subTerms ?? [];
                    const oitems = termSetItems[0]?.subTerms ?? [];
                    const totalCount = termSetItems[0]?.subTermCount ?? 0;

                    oitems.forEach(oitem => {
                        oitem.IsChecked = self.selectedClassCodeIds.has(oitem.UniqueId);
                    });

                    parentItem.origin.subTerms = oitems;
                    parentItem.origin.subTermCount = totalCount;
                    parentItem.hasChildren = oitems.length > 0;
                    parentItem.pagerByServer = true;
                    parentItem.itemsCount = totalCount;
                    parentItem.pagerIndex = currentPageIndex;

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
            onNodeSelectedChange(selectedItem) {
                const isChecked = selectedItem.checked;
                if (isChecked) {
                    self.selectedClassCodeIds.add(selectedItem.origin.UniqueId);
                } else {
                    self.selectedClassCodeIds.delete(selectedItem.origin.UniqueId);
                }
                let funcChange = self.props.onNodeSelectedChange;
                if (funcChange) {
                    funcChange(selectedItem);
                }
            },
            onTreeChanged() {
                if(self.props.onTreeChanged) {
                    self.props.onTreeChanged();
                }
            }
        };
    }

    initTreeData(callback) {
        const payload = {
            TermSetId: this.props.termSetId,
            SearchKey: "",
            PageIndex: 1,
            PageSize: DEFAULT_PAGE_SIZE
        }
        let option = {
            url: `/api/TermManagementApi/GetClassCodeGroup`,
            method: "POST",
            data: payload
        };
        fetchUtility(option).then((res) => {
            this.treeContext.searchKey = "";
            const result = $.parseJSON(res);
            const data = result && result.Data ? result.Data : [];
            data.forEach(oitem => {
                oitem.expand = true;
            });
            this.applyCheckedState(data);
            this.setTreeData(data);

        }).catch((e) => {

        });
    }

    setTreeData(data) {
        let root = {
            Name: RMResx.RM_JS_TM_RootTerms,
            Type: NodeType.Root,
            Id: NodeType.Root,
            UniqueId: NodeType.Root,
            expand: true,
            subTerms: data
        };
        this.setState({treeData: [root]});
    }

    search(key) {
        key = !key ? "" : key.trim();
        if (key.length > 0) {
            const payload = {
                TermSetId: this.props.termSetId,
                SearchKey: key,
                PageIndex: 1,
                PageSize: DEFAULT_PAGE_SIZE
            }
            let option = {
                url: `/api/TermManagementApi/GetClassCodeGroup`,
                method: "POST",
                data: payload
            };
            fetchUtility(option).then((res) => {
                this.treeContext.searchKey = key;
                const result = $.parseJSON(res);
                const data = result && result.Data ? result.Data : [];
                this.applyCheckedState(data);
                this.setTreeData(data);
            }).catch((e) => {

            });
        }
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        return str;
    }

    render() {
        return (
            <div>
                <$g.TreeView
                    items={this.state.treeData}
                    treeContext={this.treeContext}
                />
            </div>
        );
    }
}

ClassCodeTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func
};
ClassCodeTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null
};

export default ClassCodeTree;