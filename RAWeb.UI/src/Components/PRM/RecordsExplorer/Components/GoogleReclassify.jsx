import TreeNodeContent from "../../../Common/Tree/NodeContents/DefaultNodeContent";
import RuleDetail from '../../../Common/RuleDetail/Index';
import StringUtil from "../../../../Utilities/StringUtil";
import { showToast } from "../../../../Utilities/CommonUtil";
import React from "react";

//import TreeNodeContent from "../Common/Tree/NodeContents/TermManagementNodeContent";

const NodeType = {
    Root: "Root",
    TermGroup: "TermGroup",
    TermSet: "TermSet",
    Term: "Term",
};

export default class GoogleReclassify extends R.Component {
    idAttr = true;
    componentCreate() {
        this.ruleItemsPagerSize = 10;
        this.state = {
            treeData: [],
            showLabelInfo: false,
            showTreeView:false,
            labelName: "",
            labelDescription: "",
            rules: [],

            ruleDetailId: "",
            ruleDetailName: "",
            showTip: false,
            tipType: "success",
            tipMsg: "",
            nodeItems: this.props.data.Source || this.props.data || [],
            isTopButton: this.props.data.isTopButton,
            commentText:"",
            currentPageItems: [],

            ruleItemsPagerTotal: 0,
            ruleItemsPagerIndex: 0,
            ruleItemsPagerSize: this.ruleItemsPagerSize
        };
        this.bind(['refreshSelectedTermInfo', 'cellClick', 'backToTermView','handleCommentTextareaChange']);
        this.sourceFlag = this.props.type == 'phy' ? 4 : 0;
        this.selectedNodeItem = null;
        this.treeContext = this.getTreeContext();

        this.initRootNodeData();
    }

    componentReceive(type, args) {
        if (this.selectedNodeItem && this.selectedNodeItem.Id) {
            switch (type) {
                case "onSave":
                    this.selectedNodeItem.isTopButton = this.state.isTopButton;
                    this.selectedNodeItem.Comment = this.state.commentText;
                    args(this.selectedNodeItem, this.errorCallBack);
                    break;
            }
        } else {
            this.showMessageTip('error', RMResx.RM_JS_PRM_Msg_ReclassifyNoSelectTerm_Label);
        }
    }

    componentInit() {
        let isSelectOne = this.state.nodeItems.length == 1;
        this.setState({
            showLabelInfo: isSelectOne,
            showTreeView:true,
        });
        if (isSelectOne) {
            $.ajax({
                type: "GET",
                url: "/api/LabelManagement/GetLabelByUniqueId",
                data: "labelId=" + this.state.nodeItems[0].TermId,
                async: true,
                success: (data) => {
                    var term = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: init; Ignore Reason: 前后台对象存在对应关系
                    this.refreshSelectedTermInfo(term);
                },
                error: function (msg) {

                },
                dataType: "json"
            });
        }
    }

    getTreeContext() {
        let self = this;
        return {
            singleSelection: true,
            spaceNoSelection: true,
            nodeContentComponent: TreeNodeContent,
            transToTreeNodeObject(oitem) {
                let loaded = oitem.labels > 0;
                let itemsCount = oitem.labelCount;
                return {
                    origin: oitem,
                    nodeKey: oitem.UniqueId,
                    uniqueId: oitem.UniqueId,
                    nodeType: oitem.Type,
                    nodeClass: null,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: oitem.Type == NodeType.Root,
                    checked: oitem.Checked,
                    expanded: oitem.Type == NodeType.Root,
                    loaded: loaded,
                    items: oitem.labels,
                    hasChildren: itemsCount > 0,
                    pagerByServer: true,
                    itemsCount:itemsCount,
                    pagerIndex: 0,
                    pagerSize: 15,
                    enableContextMenu: false,
                    isLeafNode: false
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
                    default: {
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
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                $.ajax({
                    type: "GET",
                    url: "/api/LabelManagement/GetPaginatedLabels",
                    contentType: "application/json;charset=utf-8",
                    data: "PageSize=" + parentItem.pagerSize + "&pageNumber=" + (parentItem.pagerIndex + 1),
                    async: true,
                    beforeSend: function () {
                    //    $$.loading(true);
                    },
                    complete: function () {
                       $$.loading(false);
                    },
                    success: function (data) {
                        let items = $.parseJSON(data); 
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
                self.refreshSelectedTermInfo(item.origin);
            },
        };
    }

    refreshSelectedTermInfo(item) {
        this.selectedNodeItem = item;
        $.ajax({
            type: "GET",
            url: "/api/LabelManagement/GetLabelRuleInfo",
            data: "labelId=" + item?.Id,
            async: true,
            success: (data) => {
                let labelRuleInfo = $.parseJSON(data);
                
                if (labelRuleInfo) {
                    let rules= labelRuleInfo?.RuleInfos || [];

                    let currentPageItems = rules.slice(0, this.ruleItemsPagerSize);
                    this.setState({
                        showLabelInfo: true,
                        labelName: item.Name,
                        labelDescription: item.Description,
                        rules: rules,
                        ruleItemsPagerTotal: rules.length,
                        ruleItemsPagerIndex: 0,
                        ruleItemsPagerSize: this.ruleItemsPagerSize,
                        currentPageItems: currentPageItems
                    });
                }
            },
            error: function (msg) {
            },
            dataType: "json"
        });

    }

    initRootNodeData() {
        let option = {
            url: `/api/LabelManagement/GetPaginatedLabels`,
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let data = $.parseJSON(res);
           
            data?.Labels.forEach(oitem => {
                oitem.expand = true;
            });
          
            this.setTreeData(data);

        }).catch((e) => {

        });
    }
    setTreeData(data, ) {
        let root = {
            Name: RMResx.RM_JS_LM_RootLabel,
            Type: NodeType.Root,
            Id: NodeType.Root,
            UniqueId: NodeType.Root,
            expand: true,
            labels: data.Labels,
            labelCount: data.TotalCount

        };
        this.treeData = root;
        this.setState({treeData: [root]});
    }

    cellClick(data, action) {
        switch (action) {
            case 1: //rule detail
                this.setState({
                    showTreeView: false,
                    ruleDetailId: data.RuleId,
                    ruleDetailName: data.RuleName,
                }, () => { this.ruleDetail.load({ ruleId: data.RuleId }); });
                break;
        }
    }
    
    onShowRuleDetails(rule){
        this.setState({
            ruleDetailId: rule.RuleId,
            ruleDetailName: rule.RuleName,
        },()=>{
            this.ruleDetail.load({ ruleId: rule.RuleId, callback: this.loadRuleCallback });
        });
    }

    loadRuleCallback = (isSuccess) => {
        this.setState({ showTreeView: !isSuccess});
    }

    onChangeRuleItemsPager = (pagerIndex, pagerSize, callback) =>{
        let currentPageItems = this.state.rules.slice(pagerIndex * pagerSize, (pagerIndex + 1) * pagerSize);
        this.setState({
            ruleItemsPagerTotal: this.state.rules.length,
            ruleItemsPagerSize: pagerSize,
            ruleItemsPagerIndex: pagerIndex,
            currentPageItems: currentPageItems
        });
        callback(true);
    }

    backToTermView() {
        this.setState({
            showTreeView:true,
        });
    }

    showMessageTip = (type, msg) => {
        showToast.error(msg);
    };

    hideMessageTip = () => {
        this.setState({showTip: false});
    };

    handleCommentTextareaChange(args){
        this.setState({ commentText: args });
    }

    errorCallBack = (msg) => {
        this.showMessageTip("error", msg);
    };

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderRuleInfo(){
        if(!this.props.hideRuleInfo){
            return <React.Fragment>
                <div className="change-term-row">
                    <div className="change-term-title" tabIndex="0">
                        {StringUtil.trimEndColon(RMResx.RM_TM_TermRuleLabel)}
                    </div>
                    {this.state.currentPageItems.map((item, index) => {
                        return <div 
                            key={index} 
                            className = "change-rule-value"
                            style = {index%2==0?{background: "#F2F3F4"}:{background: "#FFFFFF"}}
                        >
                            <a className="rule-name ra-main-cell-link" onClick={this.onShowRuleDetails.bind(this,item)} aria-label = {item.RuleName} data-tooltip="ifneed" tabIndex="0" onKeyDown={this.onKeyDown}>{item.RuleName}</a>
                            <div className="rule-level">{item.RuleLevel}</div>
                        </div>;
                    })}
                    {
                        this.state.currentPageItems.length > 0 && <div className="ra-flex-justify-end margin-top-s">
                            <$g.Pager
                                itemsCount={this.state.ruleItemsPagerTotal}
                                pagerIndex={this.state.ruleItemsPagerIndex}
                                pagerSize={this.state.ruleItemsPagerSize}
                                showPagerCounter={false}
                                showPagerSize={false}
                                pagerSizeOptions={[5, 10, 15]}
                                onChange={this.onChangeRuleItemsPager} />
                        </div>
                    }
                </div>
                <div className="change-term-row">
                    <div className="change-term-title" tabIndex={0}>
                        {RMResx.RM_TM_TermComLabel}
                    </div>
                    <div className="margin-top-xs">
                        <R.Input
                            type="textarea"
                            width={285}
                            height={100}
                            className="resizable"
                            value={this.state.commentText}
                            onChange={this.handleCommentTextareaChange}
                            aria={{ariaLabel: RMResx.RM_JM_Comment}} />
                    </div>
                </div>
            </React.Fragment>;
        }
    }

    renderRuleDetail(){
        let isShowRuleDetail = !this.state.showTreeView && this.state.showLabelInfo && !this.props.hideRuleInfo;
        let rdClassName = isShowRuleDetail ? "show" : "hide";
        return <div className={rdClassName}>
            <RuleDetail
                ref={r => this.ruleDetail = r}
                isExistPanel={false}
            >
            </RuleDetail>
        </div>;
    }

    render() {
        return <div id={this.props.id}>
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                status={{show: this.state.showTip}}
                onClose={this.hideMessageTip}
            />
            <div className="reclassify-form">
                <div className="reclassify-left">
                    <div className={"reclassify-title"}>
                        {this.state.showTreeView && <div className={"term-tree-title"}>
                            {RMResx.RM_PRM_Explorer_SelectLabel.replace(':', "")}
                        </div>}
                    </div>
                    {this.state.showTreeView && <div className="tree-view-container">
                        <$g.TreeView
                            items={this.state.treeData}
                            treeContext={this.treeContext}
                        />
                    </div>}
                </div>
                <div className="reclassify-right">
                    <div className={"right-info-title"}>
                        {this.state.showTreeView && this.state.showLabelInfo && <span className={"term-span"}>
                            {RMResx.RM_JS_PRM_Explorer_LabelInformation}
                        </span>}
                        {!this.state.showTreeView && this.state.showLabelInfo && <R.Button
                            text={RMResx.RM_JS_Rule_Detail_Title}
                            className="reclassify-back-btns"
                            type="bald"
                            icon="fia-arrow-line-left"
                            onClick={this.backToTermView} />
                        }
                    </div>
                    {this.state.showTreeView && this.state.showLabelInfo && <div className="right-term-panel">
                        <div className="change-term-row">
                            <div className="change-term-title" tabIndex="0">
                                {StringUtil.trimEndColon(RMResx.RM_LM_LabelNameLabel)}
                            </div>
                            {this.state.labelName && <div className="change-term-value" tabIndex="0"  data-tooltip aria-label={this.state.labelName}>
                                {this.state.labelName}
                            </div>}
                        </div>

                        <div className="change-term-row">
                            <div className="change-term-title" tabIndex="0">
                                {StringUtil.trimEndColon(RMResx.RM_TM_TermDescLabel)}
                            </div>
                            {this.state.labelDescription && <div className="change-term-value" tabIndex="0"  data-tooltip aria-label={this.state.labelDescription}>
                                {this.state.labelDescription}
                            </div>}
                        </div>
                        {this.renderRuleInfo()}
                    </div>}
                    { this.renderRuleDetail() }
                </div>
            </div>
        </div>;
    }
}
