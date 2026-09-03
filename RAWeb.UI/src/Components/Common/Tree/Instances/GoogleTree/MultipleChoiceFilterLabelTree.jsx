import { Component } from 'react';
import PropTypes from 'prop-types';
import TreeNodeContent from '../../NodeContents/RC/TermNodeContent';

const NodeType = {
    Root: "Root",
    TermGroup: "TermGroup",
    TermSet: "TermSet",
    Term: "Term",
};


class MultipleChoiceFilterLabelTree extends Component {
    constructor(props) {
        super(props);

        this.treeCache = new Map();
        this.initTreeContext();

        this.state = {
            treeData: []
        };
    }

    componentDidMount() {
        this.isAllowSearchChange = false;
        if(this.props.data) {
            this.setTreeData(this.props.data);
        } else {
            this.initTreeData();
        }
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if ((nextProps.searchKey != this.props.searchKey) && this.isAllowSearchChange) {
        
           
            if(nextProps.searchKey){
                this.initTreeData(()=>{
                    this.search(nextProps.searchKey);
                });
            }else{
                this.initTreeData();
            }
        }
    }

    componentDidUpdate(){
        this.isAllowSearchChange = true;
    }

    initTreeContext() {
        let self = this;
        this.treeContext = {
            multiSelection: true,
            // readonly: self.props.readonly,
            allowSelectedWithoutChildren: true,
            searchKey: "",
            nodeContentComponent: TreeNodeContent,
            transToTreeNodeObject(oitem) {
                let loaded = oitem.labels?.length > 0;
                let itemsCount = oitem.labelCount;
                let isLeafNode = oitem.Type != NodeType.Root
                return {
                    origin: oitem,
                    nodeKey: oitem.Id,
                    nodeType: oitem.Type,
                    UniqueId : oitem.UniqueId,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: oitem.Type == NodeType.Root,
                    checked: oitem.IsChecked,
                    expanded: !!this.searchKey || oitem.Type == NodeType.Root,
                    loaded: loaded,
                    items: oitem.labels,
                    hasChildren: itemsCount > 0,
                    pagerByServer: true,
                    itemsCount: itemsCount,
                    pagerIndex: !oitem.pageIndex ? 0 : oitem.pageIndex,
                    pagerSize: 20,
                    enableContextMenu: !isLeafNode,
                    isLeafNode
                };
            },
            updateOriginObject(item) {
                let oitem = item.origin;
                oitem.IsChecked = !item.disableSelect && item.checked;
                oitem.expand = item.expanded;
                oitem.pageIndex = item.pagerIndex;
                oitem.PageSize = item.pagerSize;
                const cachedItem = self.treeCache.get(item.UniqueId);
                self.treeCache.set(item.UniqueId, { ...cachedItem, IsChecked: !item.disableSelect && item.checked });
            },
            getNodeIconClass(oitem) {
                switch (oitem.Type) {
                    case 'Root':
                        return 'ra-tree-icon fia-term-group';
                    case 'Label':
                        return 'ra-tree-icon fia-term';
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
                let query = "PageSize=" + parentItem.pagerSize + "&pageNumber=" + (parentItem.pagerIndex + 1);
                if (!!self.treeContext.searchKey && self.treeContext.searchKey?.length >0){
                    query += "&searchKey=" + self.replaceSpecialCharacters(self.treeContext.searchKey)
                }
                $.ajax({
                    type: "GET",
                    url: "/api/LabelManagement/GetPaginatedLabels",
                    contentType: "application/json;charset=utf-8",
                    data: query,
                    async: true,
                    beforeSend: function () {
                    //    $$.loading(true);
                    },
                    complete: function () {
                       $$.loading(false);
                    },
                    success: function (data) {
                        let items = $.parseJSON(data); 
                        items.Labels?.forEach(item => {
                            const checked = self.treeCache.get(item?.UniqueId)?.IsChecked;
                            if (checked){
                                item.IsChecked = checked;
                            }
                            self.treeCache.set(item?.UniqueId,item);
                        });
                        funcSuccess(items.Labels);
                    },
                    error: function (msg) {
                        funcFail(msg.responseText);
                    },
                    dataType: "json"
                });
                //return children node items
                return [];
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
            },
            onTreeChanged() {
                if(self.props.onTreeChanged) {
                    self.props.onTreeChanged();
                }
            }
        };
    }

    initTreeData(callback) {
        let option = {
            url: `/api/LabelManagement/GetPaginatedLabels`,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            this.treeContext.searchKey = "";
            let data = $.parseJSON(res);
           
            data?.Labels.forEach(oitem => {
                oitem.expand = true;
                this.treeCache.set(oitem?.UniqueId, oitem)
            });
            this.setTreeData(data, callback);

        }).catch((e) => {

        });
    }

    setTreeData(data, callback) {
        let root = {
            Name: RMResx.RM_JS_LM_RootLabel,
            Type: NodeType.Root,
            Id: 'GoogleRoot',
            UniqueId: NodeType.Root,
            expand: true,
            labels: data.Labels,
            labelCount: data.TotalCount

        };
        this.treeData = root;
        this.setState({treeData: [root]},()=>{
            if(callback){
                callback();
            }
        });
    }

    search(key) {
        key = !key ? "" : key.trim();
        if (key.length == 0) {
            this.getTreeData();
        } else {
            $.ajax({
                type: "GET",
                url: "/api/LabelManagement/GetPaginatedLabels",
                //contentType: 'application/json;charset=utf-8',
                data: "searchKey=" + this.replaceSpecialCharacters(key),
                async: true,
                beforeSend: function () {
                    $$.loading(true);
                },
                complete: function () {
                    $$.loading(false);
                },
                success: (data) => {
                    this.treeContext.searchKey = key;
                    this.setTreeData(data);
                },
                error: (msg) => {
                    //alert(msg.responseText);
                },
                dataType: "json"
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

    getTreeData() {
        let results = {items: [...this.treeCache.values()], selected: false };
    
        return results;
    }

    //public function
  

    render() {
        return <div>
            <$g.TreeView
                classicMode
                items={this.state.treeData}
                treeContext={this.treeContext}
            />
        </div>;
    }
}

MultipleChoiceFilterLabelTree.propTypes = {
    data: PropTypes.array,
    searchKey: PropTypes.string,
    readonly: PropTypes.bool,
    onTreeChanged: PropTypes.func
};
MultipleChoiceFilterLabelTree.defaultProps = {
    data: null,
    searchKey: null,
    readonly: false,
    onTreeChanged: null
};

export default MultipleChoiceFilterLabelTree;