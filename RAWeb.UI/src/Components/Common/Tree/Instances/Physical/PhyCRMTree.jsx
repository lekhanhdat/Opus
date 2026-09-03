import { Component } from "react";
import { NodeType } from "../../../../../Constants/DAEnums";
import TreeNodeContent from "../../NodeContents/TermManagementNodeContent";
const NodeTypeEnum = {
    Root: 9000,
    Normal: 9100,
    Min: 9200
};

class PhyCRMTree extends Component {
    constructor(props) {
        super(props);

        this.treeContext = this.getTreeContext();
        this.state = {
            treeData: [],
        };
        this.selectedTreeNode = {};
    }

    componentDidMount() {
        this.getTreeData();
    }

    UNSAFE_componentWillReceiveProps(nextProps) { 
        if (nextProps.searchKey != this.props.searchKey) {
            this.search(nextProps.searchKey);
        }
    }

    getTreeContext() {
        let mainComponent = this;
        return {
            treeType: 2,    //1:TermManagement, 2:LocationManagement
            componentType: "CRMPhyTree",
            searchKey: "",
            nodeContentComponent: TreeNodeContent,
            singleSelection: true,
            showrRightArrow: true,
            transToTreeNodeObject(oitem) {
                let itemsCount = !this.pagerByServer ? (!oitem.SubLocations ? 0 : oitem.SubLocations.length) : oitem.SubLocationCount;
                return {
                    origin: oitem,
                    nodeKey: oitem.UniqueId,
                    nodeType: oitem.NodeType == NodeTypeEnum.Root ? NodeType.Root : oitem.NodeType == NodeTypeEnum.Normal ? NodeType.Normal : NodeType.Min,
                    text: oitem.Name,
                    expanded: (!!this.searchKey && oitem.hasMatchChildren),
                    disableSelect: oitem.NodeType == NodeType.PhysicalRootLocation,
                    loaded: !!this.searchKey || oitem.SubLocationCount == 0 || !!oitem.SubLocations,
                    iconStatus: oitem.IconStatus,
                    enableContextMenu: true,
                    isAllowEditName: false,
                    items: oitem.SubLocations,
                    itemsCount: itemsCount,
                    hasChildren: itemsCount > 0,
                    pagerByServer: true,
                    pagerSize: 15,
                    pagerIndex: 0
                };
            },

            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let oItem = parentItem.origin;
                mainComponent.loadTeeNode = oItem;
                $.ajax({
                    type: "GET",
                    url: "/api/LocationManagementApi/GetChildrenByDB",
                    contentType: "application/json;charset=utf-8",
                    data: "PageIndex=" + (parentItem.pagerIndex + 1) + "&PageSize=" + parentItem.pagerSize
                        + "&NodeId=" + oItem.Id + "&NodeType=" + oItem.Type + "&IconStatus=true",
                    async: true,
                    //beforeSend: function () {
                    //    $$.loading(true);
                    //},
                    //complete: function () {
                    //    $$.loading(false);
                    //},
                    success: function (data) {
                        let items = $.parseJSON(data);  // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
                        funcSuccess(items);
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
                let funcChange = mainComponent.props.onSelectedNodeChanged;
                if (funcChange) {
                    funcChange(item.origin);
                }
                mainComponent.selectedTreeNode = RM.deepcopy(item);
            },

            onNodeRefresh(){
                let oitem = mainComponent.selectedTreeNode.origin;
                let loadTeeNode = mainComponent.loadTeeNode;
                let exitSelectedNode = false;
                //后台返回的父级id字符串转成数组。
                if(oitem && oitem.DirPath){
                    let selectedTreeNodeParentIds = oitem.DirPath.split("/").filter((id)=>{return id;});
                    if(!selectedTreeNodeParentIds.includes(loadTeeNode.Id.toString())){
                        exitSelectedNode = true;
                    }
                }
                if(!exitSelectedNode){
                    mainComponent.selectedTreeNode = {}; 
                }
                let funcChange = mainComponent.props.onNodeRefresh;
                if(funcChange){
                    funcChange(exitSelectedNode);
                }
            }         
        };
    }

    getTreeData() {
        let getListData = "PageIndex=1&PageSize=20&NodeId=Root&NodeType=Root&IconStatus=true";
        $.ajax({
            type: "GET",
            url: "/api/LocationManagementApi/GetChildrenByDB",
            contentType: "application/json;charset=utf-8",
            data: getListData,
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                this.treeContext.searchKey = "";
                this.treeContext.pagerByServer = true;
                this.resetTreeData(data);
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    search(key) {
        key = !key ? "" : key.trim();
        if (key.length == 0) {
            this.getTreeData();
        } else {
            $.ajax({
                type: "GET",
                url: "/api/LocationManagementApi/Search",
                //contentType: 'application/json;charset=utf-8',
                data: "locationStr=" + this.replaceSpecialCharacters(key),
                async: true,
                beforeSend: function () {
                    $$.loading(true);
                },
                complete: function () {
                    $$.loading(false);
                },
                success: (data) => {
                    this.treeContext.searchKey = key;
                    this.treeContext.pagerByServer = false;
                    this.resetTreeData(data);
                },
                error: (msg) => {
                    //alert(msg.responseText);
                },
                dataType: "json"
            });
        }
    }

    
    processHasMatchChildren(item) {
        let hasMatchChildren = false;
        if (item && item.SubLocations) {
            item.SubLocations.forEach((subitem) => {
                if (!hasMatchChildren && subitem.Name.indexOf(this.treeContext.searchKey) > -1) {
                    hasMatchChildren = true;
                }
                hasMatchChildren |= this.processHasMatchChildren(subitem);
            });
        }
        return item.hasMatchChildren = hasMatchChildren;
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        return str;
    }

    resetTreeData(data) {
        let treeData = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: reset tree data; Ignore Reason: 前后台对象存在对应关系
        if (this.treeContext.searchKey) {
            if (treeData) {
                this.processHasMatchChildren(treeData);
                treeData = [treeData];
            } else {
                treeData = [];
            }
        }
        this.setState({ treeData: treeData });
    }
    
    refreshSelectedNode = (updateProps, isReload) => {
        let selctedNodes = this.treeContext.selectedNodes;
        if (selctedNodes) {
            for (const key in selctedNodes) {
                const selNode = selctedNodes[key];
                if (updateProps) {
                    if(isReload){
                        selNode.props.item.loaded = false;
                        selNode.reload(0);
                    }
                    Object.assign(selNode.props.item.origin, updateProps);
                    selNode.props.item.iconStatus = updateProps.IconStatus;
                    selNode.reRender();
                } else {
                    selNode.reload(0);
                }
            }
        }
    };

    render() {
        return (
            <$g.TreeView
                id="treeview"
                classicMode
                items={this.state.treeData}
                searchKey={this.state.searchKey}
                treeContext={this.treeContext}
            />
        );
    }
}

export default PhyCRMTree;
