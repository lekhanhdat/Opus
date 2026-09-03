import TreeNodeContent from "../../../Common/Tree/NodeContents/DefaultNodeContent";
import RuleDetail from '../../../Common/RuleDetail/Index';
import StringUtil from "../../../../Utilities/StringUtil";
import { showToast } from "../../../../Utilities/CommonUtil";
import React from "react";

//import TreeNodeContent from "../Common/Tree/NodeContents/TermManagementNodeContent";

class TreeReqObj {
    constructor(nodeId, nodeType, pageIndex, pageSize, sourceFlag, containerId, showAllTerms = false) {
        this.NodeId = nodeId;
        this.NodeType = nodeType;
        this.PageIndex = !pageIndex ? 1 : (pageIndex + 1);
        this.PageSize = pageSize;
        this.SourceFlag = sourceFlag;
        this.ContainerId = containerId;
        this.ExcludeBuiltIn = true;
        this.ForPhysicalView = true;
        this.ShowAllTerms = showAllTerms;
    }
}

const TermType = {
    Root: 'Root',
    TermGroup: 'TermGroup',
    TermSet: 'TermSet',
    Term: 'Term'
};

export default class PhyReclassify extends R.Component {
    idAttr = true;
    componentCreate() {
        this.ruleItemsPagerSize = 10;
        this.state = {
            treeData: [],
            showTermInfo: false,
            showTreeView:false,
            termFullPath: "",
            termDescription: "",
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
                    if (!!this.props.isRequireComment && !$$.verify('allValidation')) return;
                    this.selectedNodeItem.isTopButton = this.state.isTopButton;
                    this.selectedNodeItem.Comment = this.state.commentText;
                    args(this.selectedNodeItem, this.errorCallBack);
                    break;
            }
        } else {
            this.showMessageTip('error', RMResx.RM_JS_PRM_Msg_ReclassifyNoSelectTerm);
        }
    }

    componentInit() {
        let isSelectOne = this.state.nodeItems.length == 1;
        this.setState({
            showTermInfo: isSelectOne,
            showTreeView:true,
        });
        if (isSelectOne) {
            $.ajax({
                type: "GET",
                url: "/api/TermManagementApi/GetTermWithPath",
                data: `termId=${this.state.nodeItems[0].TermId ? this.state.nodeItems[0].TermId : this.state.nodeItems[0].termId}`,
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
                let disableSelectTermTypes = [TermType.Root, TermType.TermGroup, TermType.TermSet];
                return {
                    origin: oitem,
                    nodeKey: oitem.UniqueId,
                    uniqueId: oitem.UniqueId,
                    nodeType: oitem.Type,
                    nodeClass: null,
                    iconClass: this.getNodeIconClass(oitem),
                    text: oitem.Name,
                    disableSelect: oitem.IsDeprecated || oitem.IsExpired || disableSelectTermTypes.includes(oitem.Type),
                    checked: oitem.Checked,
                    expanded: oitem.Type == TermType.Root || oitem.Type == TermType.TermGroup,
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
                    case TermType.Root:
                    case TermType.TermGroup:
                        return 'ra-tree-icon fia-term-group';
                    case TermType.TermSet:
                        return 'ra-tree-icon fia-term-set';
                    case TermType.Term:
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
                if (a.Type == TermType.TermGroup || a.Name == b.Name) {
                    return 0;
                } else if (a.Name.toLowerCase() > b.Name.toLowerCase()) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let isTermGroup = parentItem.nodeType == "TermGroup";
                let paramId = isTermGroup ? parentItem.uniqueId : parentItem.origin.Id;
                let url = "/api/BCMCommonSettingApi/GetChildrenTreeNodes";
                if(self.props.displayingPage === 'recordForReview'){
                    url = "/api/ManualApproval/GetChildrenTreeNodes";
                }
                let option = {
                    url: url,
                    data: new TreeReqObj(
                        paramId,
                        parentItem.nodeType,
                        parentItem.pagerIndex,
                        parentItem.pagerSize,
                        self.state.nodeItems[0].SourceFlag,
                        self.state.nodeItems[0].ContainerId,
                        self.props.displayingPage === 'recordForReview',
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
                //console.log("selected Item:", item.origin);
                self.refreshSelectedTermInfo(item.origin);
            },
        };
    }

    refreshSelectedTermInfo(item) {
        this.selectedNodeItem = item;
        if (item == null) {
            return;
        }
        if (item.Type != TermType.Term) {
            this.setState({
                showTermInfo: false,
                termFullPath: "",
                termDescription: "",
                rules: []
            });
        }
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetTermWithPath",
            data: "termId=" + item.UniqueId,
            async: true,
            success: (data) => {
                var term = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: init term; Ignore Reason: 前后台对象存在对应关系
                if (term) {
                    //console.log(term);
                    this.setState({
                        showTermInfo: true,
                        termFullPath: term.FullPath,
                        termDescription: term.Description
                    });
                }
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });

        let option = {
            url: `/api/TermManagementApi/GetTermRuleList?TermId=${item.Id}&sourceFlag=${this.sourceFlag}`,
            method: "get",
        };
        fetchUtility(option).then((data) => {
            var rulesData = $.parseJSON(data);
            let rules = [];
            if (rulesData) {
                if (rulesData.message != "") {
                    rules = rulesData;
                }
                let currentPageItems = rules.slice(0, this.ruleItemsPagerSize);
                this.setState({
                    rules: rules,
                    ruleItemsPagerTotal: rules.length,
                    ruleItemsPagerIndex: 0,
                    ruleItemsPagerSize: this.ruleItemsPagerSize,
                    currentPageItems: currentPageItems
                });
            }
        }).catch((e) => {

        });
    }

    initRootNodeData() {
        let firstContainerId = "";
        let nodeId = new Set();
        for (let element of this.state.nodeItems) {
            if (element.ContainerId && firstContainerId == "") {
                firstContainerId = element.ContainerId;
            }
            if (element.ScopeId && !nodeId.has(element.ScopeId)) {
                nodeId.add(element.ScopeId)
            }
        }
        let sourceFlag = this.state.nodeItems[0].SourceFlag || this.state.nodeItems[0].sourceFlag;
        if(sourceFlag > 999){
            sourceFlag = 0;
            nodeId = null;
        } else if (sourceFlag == 9) { //Google
            nodeId = JSON.stringify(Array.from(nodeId));
        } else { 
            nodeId = null;
        }
        let getTreeTermUrl = "/api/TermManagementApi/GetChildrenByDBForView";
        if(this.props.displayingPage === 'recordForReview'){
            getTreeTermUrl = "/api/ManualApproval/GetChildrenByDBForView";
        }
        $.ajax({
            type: "GET",
            url: getTreeTermUrl,
            contentType: "application/json;charset=utf-8",
            data: new TreeReqObj(
                nodeId,
                null,
                null,
                null,
                sourceFlag,
                firstContainerId,
                this.props.displayingPage === 'recordForReview',
            ),
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                $$.loading(false);
                let root = {
                    Name: RMResx.RM_JS_TM_RootTerms,
                    Type: TermType.Root,
                    Id: "Root",
                    subTerms: $.parseJSON(data)     // Fortify Issue Type: JSON Injection; Sink Details: init node; Ignore Reason: 前后台对象存在对应关系
                };
                root.subTermCount = root.subTerms.length;
                this.setState({treeData: [root]});
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
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
                <div className="change-term-row" id={this.props.id}>
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
                    <div className={`change-term-title ${!!this.props.isRequireComment && 'require'}`} tabIndex={0}>
                        {RMResx.RM_TM_TermComLabel}
                    </div>
                    <div className="margin-top-xs">
                        <R.Validation element='Input' require={!!this.props.isRequireComment}>
                            <R.Input
                                type="textarea"
                                width={285}
                                height={100}
                                className="resizable"
                                value={this.state.commentText}
                                onChange={this.handleCommentTextareaChange}
                                aria={{ariaLabel:RMResx.RM_JM_Comment}} />
                        </R.Validation>
                    </div>
                </div>
            </React.Fragment>;
        }
    }

    renderRuleDetail(){
        let isShowRuleDetail = !this.state.showTreeView && this.state.showTermInfo && !this.props.hideRuleInfo;
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
        return (
            <R.Validation>
                <div id='allValidation'>
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
                                    {RMResx.RM_PRM_Explorer_SelectTerm.replace(':', "")}
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
                                {this.state.showTreeView && this.state.showTermInfo && <span className={"term-span"}>
                                    {RMResx.RM_JS_PRM_Explorer_TermInformation}
                                </span>}
                                {!this.state.showTreeView && this.state.showTermInfo && <R.Button
                                    text={RMResx.RM_JS_Rule_Detail_Title}
                                    className="reclassify-back-btns"
                                    type="bald"
                                    icon="fia-arrow-line-left"
                                    onClick={this.backToTermView} />
                                }
                            </div>
                            {this.state.showTreeView && this.state.showTermInfo && <div className="right-term-panel">
                                <div className="change-term-row">
                                    <div className="change-term-title" tabIndex="0">
                                        {StringUtil.trimEndColon(RMResx.RM_TM_TermNameLabel)}
                                    </div>
                                    {this.state.termFullPath && <div className="change-term-value" tabIndex="0"  data-tooltip aria-label={this.state.termFullPath}>
                                        {this.state.termFullPath}
                                    </div>}
                                </div>

                                <div className="change-term-row">
                                    <div className="change-term-title" tabIndex="0">
                                        {StringUtil.trimEndColon(RMResx.RM_TM_TermDescLabel)}
                                    </div>
                                    {this.state.termDescription && <div className="change-term-value" tabIndex="0"  data-tooltip aria-label={this.state.termDescription}>
                                        {this.state.termDescription}
                                    </div>}
                                </div>
                                {this.renderRuleInfo()}
                            </div>}
                            { this.renderRuleDetail() }
                        </div>
                    </div>
                </div>
            </R.Validation>
        );
    }
}
