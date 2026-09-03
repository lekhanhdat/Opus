import { Component } from "react";
import TreeNodeContent from "../../NodeContents/TermManagementNodeContent";

const NodeType = {
    RuleContainerRoot: "RuleContainerRoot",
    RuleContainer: "RuleContainer"
};

const NodeTypeEnum = {
    RuleContainerRoot: 12000,
    RuleContainer: 12001
};
export default class RuleContainerTree extends Component {
    constructor(props) {
        super(props);
        this.getTreeData();
        this.treeContext = this.getTreeContext();
        this.state = {
            treeData: [],
            selectedItem: null, //selected item
            currentItem: null,  //current selected item, clone from "selectedItem"
            itemSettingChanged: false
        };
    }

    startSearch(searchKey) {
        this.searchData(searchKey);
    }

    stopSearch(args) {
        this.getTreeData();
    }

    copyProps(fromObj, toObj, propNames) {
        if (fromObj && toObj && propNames) {
            for (var i = 0; i < propNames.length; i++) {
                toObj[propNames[i]] = fromObj[propNames[i]];
            }
        }
    }

    getTreeContext() {
        return {
            treeType: 4,    //1:TermManagement, 2:LocationManagement, 3:Template, 4:RuleContainer
            searchKey: "",
            nodeContentComponent: TreeNodeContent,
            singleSelection: true,
            transToTreeNodeObject(oItem) {
                return {
                    origin: oItem,
                    nodeKey: oItem.ContainerId,
                    nodeType: oItem.NodeType == NodeTypeEnum.RuleContainerRoot ? NodeType.RuleContainerRoot : NodeType.RuleContainer,
                    text: oItem.Name,
                    disableSelect: this.isDisableSelect(oItem),
                    expanded: (!!this.searchKey) || oItem.NodeType == NodeTypeEnum.RuleContainerRoot ,
                    loaded: !!this.searchKey || oItem.TotalCount == 0 || !!oItem.RuleContainerList,
                    enableContextMenu: !oItem.IsDefault,
                    isAllowEditName: oItem.NodeType != NodeTypeEnum.RuleContainerRoot && !oItem.IsDefault,
                    items: oItem.RuleContainerList,
                    itemsCount: oItem.TotalCount,
                    hasChildren: oItem.TotalCount > 0,
                    pagerByServer: true,
                    pagerSize: 15,
                    pagerIndex: 0
                };
            },
            isDisableSelect(oItem){
                return oItem.NodeType == NodeTypeEnum.RuleContainerRoot;
            },
            sortChild(a, b) {
                
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let option = {
                    url: "/api/RuleApi/GetChildrenByDB",
                    method: "post",
                    data: { SearchKey: "", PageIndex: parentItem.pagerIndex + 1, PageSize: parentItem.pagerSize }
                };
                $$.loading(true);
                fetchUtility(option).then((data) => {
                    $$.loading(false);
                    let items = $.parseJSON(data);
                    funcSuccess(items[0].RuleContainerList);
                }).catch((e) => {
                    $$.loading(false);
                });
                return [];
            },
            confirmOnNodeSelected: (item, funcAllow) => this.onNodeSelected(item.origin, funcAllow),
            refreshSelectedNodeInfo: this.refreshSelectedNodeInfo.bind(this),
            onStopSearch: this.stopSearch.bind(this),
            showMessageTip: this.showMessageTip,
            hideMessageTip: this.hideMessageTip,
        };
    }

    getTreeData() {
        let option = {
            url: "/api/RuleApi/GetChildrenByDB",
            method: "post",
            data: { SearchKey: "", PageIndex: 1, PageSize: 15 }
        };
        $$.loading(true);
        fetchUtility(option).then((data) => {
            $$.loading(false);
            this.treeContext.searchKey = "";
            this.treeContext.pagerByServer = true;
            this.resetTreeData(data);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    searchData(searchKey) {
        let option = {
            url: "/api/RuleApi/GetChildrenByDB",
            method: "post",
            data: { SearchKey: searchKey, PageIndex: 1, PageSize: 15 }
        };
        $$.loading(true);
        fetchUtility(option).then((data) => {
            $$.loading(false);
            this.treeContext.searchKey = searchKey;
            this.treeContext.pagerByServer = false;
            this.resetTreeData(data);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    //actionType: 1=rename, 2=retire, 3=reactive, 4=delete item
    refreshSelectedNodeInfo(item, actionType) {
        let props;
        switch (actionType) {
            case 4:
                this.props.onDeleteRule();
                this.setState({
                    itemSettingChanged: false,
                    selectedItem: null,
                    currentItem: null
                });
                return;
            case 1:
                if (this.state.selectedItem){
                    if (this.state.selectedItem.ContainerId != item.ContainerId) {
                        return;
                    } else {
                        this.props.containerName(this.state.selectedItem.Name);
                    }
                }
                props = ["Name"];
                break;
            default:
                props = [];
                break;
        }

        this.copyProps(item, this.state.selectedItem, props);
        this.copyProps(item, this.state.currentItem, props);

        this.setState({
            selectedItem: this.state.selectedItem,
            currentItem: this.state.currentItem
        });
    }

    onNodeSelected(item, funcAllow) {
        if (this.state.itemSettingChanged) {
            this.showIfLeaveWithoutSaveMsg((allow) => {
                this.hideMessagebox();
                if (funcAllow) {
                    funcAllow(allow);
                }
                if (allow) {
                    this.setNewSelectedItem(item);
                }
            });
        } else {
            if (funcAllow) {
                funcAllow(true);
            }
            this.setNewSelectedItem(item);
        }
    }

    resetTreeData(data) {
        let treeData = $.parseJSON(data);
        if (this.treeContext.searchKey) {
            if (treeData) {
                this.processHasMatchChildren(treeData[0]);
            } else {
                treeData = [];
            }
        }
        this.setState({ treeData: treeData });
    }

    processHasMatchChildren(item) {
        let hasMatchChildren = false;
        if (item && item.RuleContainerList) {
            item.RuleContainerList.forEach((subitem) => {
                if (!hasMatchChildren && subitem.Name.indexOf(this.treeContext.searchKey) > -1) {
                    hasMatchChildren = true;
                }
                hasMatchChildren |= this.processHasMatchChildren(subitem);
            });
        }

        return item.hasMatchChildren = hasMatchChildren;
    }

    setNewSelectedItem(item) {
        this.setState({
            selectedItem: item,
            currentItem: JSON.parse(JSON.stringify(item)),
            itemSettingChanged: false
        });
        this.hideMessageTip();
        if (this.props.onTreeChanged) {
            this.props.onTreeChanged(item);
        }
    }

    hideMessageTip() {
        this.setState({ showTip: false });
    }

    showIfLeaveWithoutSaveMsg(funcAllow) {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_WithoutSavingMsg,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: () => funcAllow(false) },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => funcAllow(true) },
            ]
        };
        $$.messagedialog(true, args);
    }

    render() {
        return <$g.TreeView
            id="treeview"
            classicMode
            items={this.state.treeData}
            searchKey={this.state.searchKey}
            treeContext={this.treeContext}
        />;
    }
}