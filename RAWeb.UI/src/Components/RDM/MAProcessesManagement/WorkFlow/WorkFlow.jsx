import jsplumb from 'jsplumb';
import ConfigurationPanel from "./ConfigurationPanel";
import StringUtil from "../../../../Utilities/StringUtil";
import {bindEvents, isShowActionByDC} from "../../../../Utilities/CommonUtil";
import {BaseStyle, NodeTypes, StatusName, WorkflowInitData, NodeOptions} from './NodeConfig';
import "../../../../Less/RDM/workFlow.less";
import RouterUrls from "../../../../Constants/RouterUrls";
import {Prompt} from 'react-router';

const jsPlumbIn = jsplumb.jsPlumb;
let idePageJsPlumstance = '';

let lineOverlayType = {
    Approve: {
        type: "1",
        text: RMResx.RM_RDM_WorkFlow_Approve
    },
    Reject: {
        type: "2",
        text: RMResx.RM_RDM_WorkFlow_Reject
    },
    ApproveOrReject: {
        type: "4",
        text: RMResx.RM_RDM_WorkFlow_ApprovOrReject
    }
};

const isMultiGeoMainDC = isShowActionByDC();

class WorkFlow extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            profileName: '',
            description: '',
            isShowWholeMenuPanel: true,  //左侧节点菜单切换（完整或者部分显示）
            flowChartInfo: [],
            isShowEmptySpace: false,
            disposalOpts: NodeOptions,
            destroyOpts: [NodeOptions[1]],
            showDisConfigurationPanel: {show: false},
            showViewDisConfigurationPanel: {show: false},
            reviewers: [],
            isViewReviewer: false,
            tipStatus: {show: false},
            tipType: "",
            tipMsg: "",
            zoomNum: 100,
            isWorkflowChanged: false,
            reviewerType: 0,
            selectNodeInfo: {},
            groupName: "",
            isAssignSiteOwnersChecked: true,
        };

        this.zoomNum = 1;               //缩放比例
        this.currentWorkflowId = props.workFlowId;
        this.isViewWorkFlow = props.optionType == "viewDetail";
        this.isSetUpWorkFlow = props.optionType == "setUpWorkFlow";
        this.ReferenceId = null;

        bindEvents(this, "onConnectLine", "onDeleteLine", "onBeforeDrop", "onSaveWorkFlow", "switchMenuPanel",
            "hideMessageTip", "onPeopleSelectionChanged", "onCancelWorkFlow", "viewDisConfiguration",
            "onChangeProfileName", "onChangeDescription", "onEditWorkFlow", "onClick", "onLoseFocus", "plusZoom",
            "reduceZoom"
        );
    }

    componentInit() {
        this.initJsplumb();
        this.initWorkFlow();                 //回显workflow数据
        this.registerOverflowEvent();        //注册workflow图标中的事件
        if (this.isSetUpWorkFlow) {
            this.onDragMenuNode();           //拖拽左侧菜单的方法
            this.onDeleteLineByKeyDown();    //通过快捷键删除连线
            this.setZoom();
        }
        document.addEventListener('click', this.onLoseFocus);
        document.addEventListener('click', this.resetContainerWidth);
    }

    componentDestroy () {
        document.removeEventListener('click', this.onLoseFocus);
        document.removeEventListener('click', this.resetContainerWidth);
    }

    initJsplumb() {
        idePageJsPlumstance = jsPlumbIn.getInstance();
        idePageJsPlumstance.setContainer('raWorkFlowDiagramContainer');
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }
    showMsgToast(content,type){
        let option = {
            content : content,
            classify : type
        };
        $$.toast(option);
    }

    showMessageTip(type, msg) {
        let tipOption = {
            tipStatus: {show: true},
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip() {
        this.setState({
            tipStatus: {show: false}
        });
    }

    switchMenuPanel(isShowWholeMenuPanel) {
        let nodePanelWidth = isShowWholeMenuPanel ? 350 : 120;
        $("#raWorkFlowDiagramContainer").width(($("#raWorkFlow").width() - nodePanelWidth) / this.zoomNum + 'px');
        $('#raWorkFlowDragNodes').width(nodePanelWidth + 'px');
        this.setState({
            isShowWholeMenuPanel: isShowWholeMenuPanel
        });
        // $('#raWorkFlowDiagramContainer').width(`calc(100% - ${nodePanelWidth}px)`);
    }

    getNodeClass(NodeType) {
        let nodeClass = 'start-node';
        switch (NodeType) {
            case NodeTypes.BeginDisposalReview:
            case NodeTypes.DisposalReview:
                nodeClass = 'dr-node';
                break;
            case NodeTypes.Destroy:
                nodeClass = 'do-des-node';
                break;
            case NodeTypes.NotDestroy:
                nodeClass = 'no-des-node';
                break;
            case NodeTypes.End:
                nodeClass = 'end-node';
                break;
        }
        return nodeClass;
    }

    getNodeType(nodeClass) {
        let nodeType = "";
        if (nodeClass.includes("start-node")) {
            nodeType = NodeTypes.Start;
        } else if (nodeClass.includes("dr-node")) {
            nodeType = NodeTypes.DisposalReview;
        } else if (nodeClass.includes("do-des-node")) {
            nodeType = NodeTypes.Destroy;
        } else if (nodeClass.includes("no-des-node")) {
            nodeType = NodeTypes.NotDestroy;
        } else {
            nodeType = NodeTypes.End;
        }
        return nodeType;
    }

    initWorkFlow() {
        let workflowNodes = [];
        let WorkflowCreateData = RM.deepcopy(WorkflowInitData);
        if (!this.currentWorkflowId) {
            workflowNodes = WorkflowCreateData.Content.WorkflowNodes;
            for (let item of workflowNodes) {
                item.Id = StringUtil.newGuid();
            }
            //初始化需要连接start和beginDisposal
            workflowNodes[0].ChildrenIds.push(workflowNodes[1].Id);
            //初始化需要连接Destroy,Do Not Destro与endNode
            workflowNodes[2].ChildrenIds.push(workflowNodes[4].Id);
            workflowNodes[3].ChildrenIds.push(workflowNodes[4].Id);
            this.createFlowChart(WorkflowCreateData);
            this.setState({isShowEmptySpace: true});
        } else {
            let option = {
                url: `/Api/CPApi/LoadManualProcess?id=${this.currentWorkflowId}`,
                method: "get"
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);
                this.ReferenceId = res.ReferenceId;
                this.createFlowChart(res);
            }).catch((e) => {
                $$.loading(false);
            });
        }

    }

    createFlowChart(workFlowInfo) {
        let flowChartInfo = workFlowInfo.Content.WorkflowNodes;
        for (let item of flowChartInfo) {
            item.class = this.getNodeClass(item.NodeType);
            item.style = {
                left: (item.Position_X - 350) + 'px',
                top: (item.Position_Y - 190) + 'px',
                position: 'absolute',
            };
        }
        this.setState({flowChartInfo: flowChartInfo}, () => {
            //回显连接点
            for (let item of flowChartInfo) {
                this.addNodeToFlowChart(item);
            }

            //回显连线
            for (let item of flowChartInfo) {
                if (item.ChildrenIds) {
                    for (let id of item.ChildrenIds) {
                        let parentUuid = item.Id + 'out';    //连线需要的uuid
                        let currentUuid = id + 'in';         //连线需要的uuid
                        idePageJsPlumstance.connect({uuids: [parentUuid, currentUuid]});
                    }
                }
            }

            //回显status
            for (let item of flowChartInfo) {
                $('#selected' + item.Id).attr('value', item.Status).text(StatusName[item.Status]);
            }

            //回显缩放
            this.zoomNum = workFlowInfo.Content.ZoomNum;
            this.setFlowChartZoom(this.zoomNum);

            //回显profileName和description
            this.setState({
                profileName: workFlowInfo.Name,
                description: workFlowInfo.Description,
            });
        });
    }


    onDragMenuNode() {
        let self = this;
        $(".dr-node").draggable(
            {
                helper: 'clone',
                zIndex: 1,
                containment: '#raWorkFlow'
            }
        );
        $(".do-des-node").draggable(
            {
                helper: 'clone',
                zIndex: 1,
                containment: '#raWorkFlow'
            }
        );
        $(".no-des-node").draggable(
            {
                helper: 'clone',
                zIndex: 1,
                containment: '#raWorkFlow'
            }
        );
        $('#raWorkFlowDiagramContainer').droppable({
            drop: (event, ui) => {
                let flowChartInfo = self.state.flowChartInfo;
                let currentDragNodeId = ui.helper.attr('id');  //获取左侧拖拽节点的id
                if (!currentDragNodeId) {
                    //Name属性的最大值
                    let maxNameNum = Math.max.apply(Math, flowChartInfo.map(item => {
                        return parseInt(item.Name,10);
                    }));
                    let nodeId = StringUtil.newGuid();
                    let offsetLeft = parseInt(ui.offset.left,10);
                    let offsetTop = parseInt(ui.offset.top,10);
                    let currentDropNodeInfo = {
                        Id: nodeId,
                        class: ui.helper.attr('class'),
                        Status: '',
                        NodeType: self.getNodeType(ui.helper.attr('class')),
                        Name: (maxNameNum + 1),
                        DisplayName: RMResx.RM_RDM_WorkFlow_DefaultDisposalName,
                        Reviewers: [],
                        ReviewerType:0,
                        style: {
                            left: self.state.isShowWholeMenuPanel ? (offsetLeft - 391 - $(".reco-layout-sider").width()) / self.zoomNum + 'px' : (offsetLeft - 161 - $(".reco-layout-sider").width()) / self.zoomNum + 'px',
                            top: (offsetTop - 151) / self.zoomNum + 'px',
                            position: 'absolute',
                        },
                        ParentId: '',
                    };
                    flowChartInfo.push(currentDropNodeInfo);
                    self.setState(
                        {
                            flowChartInfo: RM.deepcopy(flowChartInfo),
                            isShowEmptySpace: false
                        }, () => {
                            self.addNodeToFlowChart(currentDropNodeInfo);
                            self.setState({
                                isWorkflowChanged: true
                            });
                        });
                }
            }
        });
    }

    addNodeToFlowChart(nodeInfo) {
        let nodeId = nodeInfo.Id;
        let NodeType = nodeInfo.NodeType;
        switch (NodeType) {
            case NodeTypes.Start:
                this.addEndpointToNode(nodeInfo, 'Right', nodeInfo.Id + 'out');
                break;
            case NodeTypes.DisposalReview:
                this.addEndpointToNode(nodeInfo, 'Top', nodeInfo.Id + 'in');
                this.addEndpointToNode(nodeInfo, 'Bottom', nodeInfo.Id + 'out');
                break;
            case NodeTypes.Destroy:
            case NodeTypes.NotDestroy:
                this.addEndpointToNode(nodeInfo, 'Top', nodeInfo.Id + 'in');
                this.addEndpointToNode(nodeInfo, 'Bottom', nodeInfo.Id + 'out');
                break;
            case NodeTypes.End:
                this.addEndpointToNode(nodeInfo, 'Top', nodeInfo.Id + 'in');
                break;
            case NodeTypes.BeginDisposalReview:
                this.addEndpointToNode(nodeInfo, 'Left', nodeInfo.Id + 'in');
                this.addEndpointToNode(nodeInfo, 'Bottom', nodeInfo.Id + 'out');
        }
        //只有create和edit可以拖拽
        if (this.isSetUpWorkFlow) {
            let self = this;
            idePageJsPlumstance.draggable(nodeId, {
                containment: "raWorkFlowDiagramContainer",
                drag: function (info) {
                    self.setState({
                        isWorkflowChanged: true
                    });
                }
            });
        }
    }

    addEndpointToNode(nodeInfo, direction, uuid) {
        let nodeId = nodeInfo.Id;
        let NodeType = nodeInfo.NodeType;
        let baseStyleInfo = RM.deepcopy(BaseStyle);  //获取初始化配置
        let maxConnections = 1;                      //最大连接数
        if (uuid.indexOf('out') != -1 && (NodeType == NodeTypes.BeginDisposalReview || NodeType == NodeTypes.DisposalReview)) {
            maxConnections = 2;
            baseStyleInfo.connectorOverlays[0] = [
                "Custom",
                {
                    create: function () {
                        return $("<div></div>");
                    },
                    location: 0.6,
                    id: nodeId
                }

            ];
            baseStyleInfo.connectorOverlays[1] = [
                "Custom",
                {
                    create: function () {
                        return $(`<div class="ra-overlay-endPoint">
                           <div class="fia-arrow-down"></div>
                      </div>`);
                    },
                    location: 1,
                    id: 'ra-overlay-endPoint'
                }
            ];
        }
        //END 可以接入任意多条线
        if (NodeType == NodeTypes.End || NodeType == NodeTypes.NotDestroy) {
            maxConnections = -1;
        }
        baseStyleInfo.maxConnections = maxConnections;
        //切换连线样式
        idePageJsPlumstance.registerConnectionTypes({
            "basic": {
                paintStyle: {
                    strokeWidth: 1,
                    outlineStroke: '#207580',
                },
                connectorHoverStyle: {
                    strokeWidth: 2,
                    outlineStroke: '#07E9FF',
                }
            },
            "selected": {
                paintStyle: {
                    strokeWidth: 2,
                    outlineStroke: '#07E9FF',
                },
                connectorHoverStyle: {
                    strokeWidth: 2,
                    outlineStroke: '#07E9FF',
                }
            },
            "error": {
                paintStyle: {
                    strokeWidth: 2,
                    outlineStroke: 'red',
                },
                hoverPaintStyle: {
                    strokeWidth: 2,
                    outlineStroke: 'red',
                },
            }
        });
        idePageJsPlumstance.addEndpoint(nodeId, {
            anchor: direction,
            uuid: uuid
        }, baseStyleInfo);
    }

    registerOverflowEvent() {
        let self = this;
        if (this.isSetUpWorkFlow) {
            idePageJsPlumstance.bind('dblclick', self.onDeleteLine);
            idePageJsPlumstance.bind('beforeDrop', self.onBeforeDrop);
            idePageJsPlumstance.bind('click', self.onClick);
        }
        idePageJsPlumstance.bind('connection', self.onConnectLine);
    }

    onClick(connection, e) {
        e.stopPropagation();
        let sourceType = this.getNodeType(connection.source.className);
        for (let item of idePageJsPlumstance.getAllConnections()) {
            if (connection.id == item.id && sourceType != NodeTypes.Start) {
                item.setType("selected");
            } else {
                item.setType("basic");
            }
        }
        this.connection = connection;
    }

    onDeleteLine(currentConnectLine) {
        let startNodeInfo = this.state.flowChartInfo.find((item) => {
            return item.NodeType === NodeTypes.Start;
        });
        let endNodeInfo = this.state.flowChartInfo.find((item) => {
            return item.NodeType === NodeTypes.End;
        });
        if(currentConnectLine.sourceId === startNodeInfo.Id || currentConnectLine.targetId === endNodeInfo.Id){
            this.showMsgToast(RMResx.RM_RDM_WorkFlow_DeleteStartOrEndLine_Tip,'warn');
            return;
        }
        this.resetSiblingConnectLineOverlay(currentConnectLine);
        idePageJsPlumstance.deleteConnection(currentConnectLine);
        this.connection = '';
        this.setState({ isWorkflowChanged: true}); 
    }

    resetSiblingConnectLineOverlay(currentConnectLine){
        let allConnectionsInfo = idePageJsPlumstance.getAllConnections();
        let sourceType = this.getNodeType(currentConnectLine.source.className);
        let targetType = this.getNodeType(currentConnectLine.target.className);
        if(sourceType === NodeTypes.DisposalReview && targetType === NodeTypes.NotDestroy){
            let siblingConnectLine = allConnectionsInfo.filter(item => item.sourceId === currentConnectLine.sourceId && item.id !== currentConnectLine.id);
            if(siblingConnectLine && siblingConnectLine.length > 0){
                let siblingTargetType = this.getNodeType(siblingConnectLine[0].target.className);
                if(siblingConnectLine.length > 0 && siblingTargetType == NodeTypes.DisposalReview){
                    this.setConnectLineOverlayContent(siblingConnectLine[0], lineOverlayType.ApproveOrReject);
                }
            }
        }
    }

    onBeforeDrop(info) {
        //不让自己连接自己
        if (info.sourceId === info.targetId) {
            return false;
        } else {
            return true;
        }
    }
  
    isValidForConnectLine(info, sourceType, targetType){
        let isDelCurrentConnectLine = false;
        if (info.sourceEndpoint.anchor.type === 'Top' || info.targetEndpoint.anchor.type === 'Bottom') {
            isDelCurrentConnectLine = true;
        }
        if (targetType == NodeTypes.DisposalReview && info.targetEndpoint.anchor.type == 'Top') {
            if (sourceType != NodeTypes.DisposalReview && sourceType != NodeTypes.BeginDisposalReview) {
                isDelCurrentConnectLine = true;
            }
        }
        if (targetType == NodeTypes.NotDestroy || targetType == NodeTypes.Destroy) {
            if (sourceType != NodeTypes.BeginDisposalReview && sourceType != NodeTypes.DisposalReview) {
                isDelCurrentConnectLine = true;
            }
        }
        if (targetType == NodeTypes.End) {
            if (sourceType == NodeTypes.Start || sourceType == NodeTypes.BeginDisposalReview || sourceType == NodeTypes.DisposalReview) {
                isDelCurrentConnectLine = true;
            }
            let firstSourceEndpoint = this.getNodeType(info.targetEndpoint.connections[0].source.className);
            if(info.targetEndpoint.connections[1]){
                let secSourceEndpoint = this.getNodeType(info.targetEndpoint.connections[1].source.className);
                if ((firstSourceEndpoint == NodeTypes.Destroy && secSourceEndpoint != NodeTypes.NotDestroy) ||
                    (firstSourceEndpoint == NodeTypes.NotDestroy && secSourceEndpoint != NodeTypes.Destroy)) {
                    isDelCurrentConnectLine = true;
                }
            }
            if(info.targetEndpoint.connections.length > 2){
                isDelCurrentConnectLine = true;
            }
        }
        if (sourceType == NodeTypes.DisposalReview) {
            let allowTargetEndpointTypeForDR = [NodeTypes.DisposalReview, NodeTypes.NotDestroy, NodeTypes.Destroy];
            let connectionsTargetTypesForDR = info.sourceEndpoint.connections.map(item => this.getNodeType(item.target.className));
            for(let connectionsTargetType of connectionsTargetTypesForDR){
                if(!allowTargetEndpointTypeForDR.includes(connectionsTargetType)){
                    isDelCurrentConnectLine = true;
                }
            }
            if(connectionsTargetTypesForDR.length > 1){
                if(connectionsTargetTypesForDR[0] === connectionsTargetTypesForDR[1] || !connectionsTargetTypesForDR.includes(NodeTypes.NotDestroy)){
                    isDelCurrentConnectLine = true;
                }
            }
        }
        return isDelCurrentConnectLine;
    }

    setConnectLineOverlayContent(connectLineInfo, lineOverlayType){
        let {type, text} = lineOverlayType;
        let currentConnectLine = connectLineInfo.connection ?? connectLineInfo;
        let lineOverlayId = currentConnectLine.getOverlay(connectLineInfo.sourceId).canvas.id;
        $(`#${lineOverlayId}`).text(text).attr('value', type);
    }

    setConnectLineOverlay(connectLineInfo){
        let sourceEndpointConnections = connectLineInfo.sourceEndpoint.connections;
        let sourceType = this.getNodeType(connectLineInfo.source.className);
        let targetType = this.getNodeType(connectLineInfo.target.className);
        if(sourceType == NodeTypes.DisposalReview){
            this.setConnectLineOverlayContent(connectLineInfo, lineOverlayType.ApproveOrReject);
            if(sourceEndpointConnections.length > 1){
                for(let connectLine of sourceEndpointConnections){
                    if(this.getNodeType(connectLine.target.className) == NodeTypes.DisposalReview){
                        this.setConnectLineOverlayContent(connectLine, lineOverlayType.Approve);
                    }
                }
            }
        }
        if(targetType == NodeTypes.Destroy){
            this.setConnectLineOverlayContent(connectLineInfo, lineOverlayType.Approve);
        }
        if(targetType == NodeTypes.NotDestroy){
            this.setConnectLineOverlayContent(connectLineInfo, lineOverlayType.Reject);
        }
        $('#' + connectLineInfo.connection.getOverlay(connectLineInfo.sourceId).canvas.id).attr('id', 'selected' + connectLineInfo.targetId).attr('class', 'ra-approvalStatus-select');
    }

    onConnectLine(info) {
        let allConnectionsInfo = idePageJsPlumstance.getAllConnections();
        let sourceType = this.getNodeType(info.source.className);
        let targetType = this.getNodeType(info.target.className);
        let isDelCurrentConnectLine = this.isValidForConnectLine(info, sourceType, targetType);
        if (isDelCurrentConnectLine) {
            //连线变红，然后消失 
            info.connection.setType("error");
            setTimeout(() => {
                idePageJsPlumstance.deleteConnection(info.connection);
            }, 100);
            //popover，2秒后消失
            $('#' + info.targetId).children(".ra-wf-node-popover").css('display', "block");
            setTimeout(() => {
                $('#' + info.targetId).children(".ra-wf-node-popover").css('display', "none");
            }, 2000);
        }
        //设置连线上status
        if(!isDelCurrentConnectLine){
            this.setConnectLineOverlay(info);
        }
        if (allConnectionsInfo.length > 1) {
            this.setState({isShowEmptySpace: false});
        }
        if (!this.currentWorkflowId && idePageJsPlumstance.getAllConnections().length > 1) {
            this.setState({isWorkflowChanged: true});
        }
    }

    onDeleteLineByKeyDown() {
        let self = this;
        $(document).on("keydown", function (e) {
            if (e.keyCode == 8 || e.keyCode == 46) {
                if (self.connection) {
                    self.onDeleteLine(self.connection);
                }
            }
        });
    }

    onLoseFocus() {
        if (this.connection) {
            this.connection.setType("basic");
            this.connection = '';
        }
    }

    resetContainerWidth = (e) => {
        if(e.target.className.includes("button-icon-part faui-angle-up-s")){
            this.setFlowChartZoom(this.zoomNum);
        }
    }

    onSelectOption(type, id) {
        //1 Configure 2 Delete
        if (type == 1) {
            this.currentNodeId = id;
            let currentNodeInfo = {};
            for (let item of this.state.flowChartInfo) {
                if (item.Id == id) {
                    currentNodeInfo = item;
                }
            }
            for (let item of currentNodeInfo.Reviewers) {
                item.Checked = true;
            }
            this.setState({
                selectNodeInfo: currentNodeInfo,
                reviewers: currentNodeInfo.Reviewers,
                displayName: currentNodeInfo.DisplayName,
                reviewerType: currentNodeInfo.ReviewerType,
                groupName: currentNodeInfo.GroupName,
                isAssignSiteOwnersChecked: currentNodeInfo.IsAssignSiteOwnersChecked,
                showDisConfigurationPanel: {show: true},
                isViewReviewer: false,
            });
        }

        if (type == 2) {
            idePageJsPlumstance.remove(id);
            idePageJsPlumstance.removeAllEndpoints(id);
            this.setState({
                isWorkflowChanged: true
            });
        }
    }

    plusZoom() {
        if (this.isSetUpWorkFlow && this.zoomNum < 1) {
            this.zoomNum += 0.1;
            this.setFlowChartZoom(this.zoomNum);
        }
    }

    reduceZoom() {
        if (this.isSetUpWorkFlow && this.zoomNum > 0.71) {
            this.zoomNum -= 0.1;
            this.setFlowChartZoom(this.zoomNum);
        }
    }

    setZoom() {
        let self = this;
        $('#raWorkFlowDiagramContainer').mousewheel(function (event, delta) {
            if (delta > 0) {
                self.plusZoom();
            } else {
                self.reduceZoom();
            }
            self.setState({
                isWorkflowChanged: true
            });
            return false;
        });
    }

    setFlowChartZoom(scale) {
        $("#raWorkFlowDiagramContainer").css({
            "-webkit-transform": `scale(${scale})`,
            "-moz-transform": `scale(${scale})`,
            "-ms-transform": `scale(${scale})`,
            "-o-transform": `scale(${scale})`,
            "transform": `scale(${scale})`,
            "transform-origin": '0 0'
        });
        idePageJsPlumstance.setZoom(scale);
        this.zoomNum = scale;
        this.setState({zoomNum: parseInt(this.zoomNum * 100,10)});
        $("#raWorkFlowDiagramContainer").width(($("#raWorkFlow").width() - 120) / scale + 'px');
        if (this.state.isShowWholeMenuPanel) {
            $("#raWorkFlowDiagramContainer").width(($("#raWorkFlow").width() - 350) / scale + 'px');
        }
        $("#raWorkFlowDiagramContainer").height($('.main-left').height() / scale + 'px');
    }

    onChangeProfileName(value) {
        this.setState({isWorkflowChanged: true});
        this.setState({profileName: value.trim()});
    }

    onChangeDescription(value) {
        this.setState({isWorkflowChanged: true});
        this.setState({description: value.trim()});
    }

    getChildrenId(id) {
        let connectionInfo = idePageJsPlumstance.getAllConnections();
        let childrenIds = [];
        for (let item of connectionInfo) {
            if (item.sourceId == id) {
                childrenIds.push(item.targetId);
            }
        }
        return childrenIds;
    }

    getParentId(id){
        let connectionInfo = idePageJsPlumstance.getAllConnections();
        let parentIds = [];
        for (let item of connectionInfo) {
            if (item.targetId == id) {
                parentIds.push(item.sourceId);
            }
        }
        return parentIds;
    }

    onSaveWorkFlow() {
        let flowChartInfo = [];
        let workFlowInfo = {};
        if (!this.state.profileName) {
            this.showMsgToast(RMResx.RM_RDM_WorkFlow_Msg_NotFillName,'error');
            return;
        } else {
            this.hideMessageTip();
        }
        let destroyNodeId = "";
        let notDestroyNodeId = "";
        for (let flowChartNode of this.state.flowChartInfo) {
            let param = {};
            param.Id = flowChartNode.Id;
            param.Name = flowChartNode.Name + '';
            param.DisplayName = flowChartNode.DisplayName || flowChartNode.Id;
            param.Position_X = parseInt($('#' + param.Id).css('left'),10) + 350;
            param.Position_Y = parseInt($('#' + param.Id).css('top'),10) + 190;
            param.ChildrenIds = this.getChildrenId(flowChartNode.Id);
            param.Status = $('#selected' + param.Id).attr('value') || 0;
            param.NodeType = flowChartNode.NodeType;
            param.UsedEmailTemplateMode = flowChartNode.UsedEmailTemplateMode;
            if(flowChartNode.UsedEmailTemplateId){
                param.UsedEmailTemplateId = flowChartNode.UsedEmailTemplateId;
            }
            if(flowChartNode.CustomIntervalSetting){
                param.CustomIntervalSetting = flowChartNode.CustomIntervalSetting;
            }
            param.Reviewers = [];
            param.ReviewerType = flowChartNode.ReviewerType;
            if (flowChartNode.ReviewerType === 2) {
                param.GroupName = flowChartNode.GroupName;
                param.IsAssignSiteOwnersChecked = flowChartNode.IsAssignSiteOwnersChecked;
            }
            if (flowChartNode.Reviewers && flowChartNode.Reviewers.length > 0) {
                for (let item of flowChartNode.Reviewers) {
                    let reviewerInfo = {};
                    reviewerInfo.UserId = item.UserId;
                    reviewerInfo.UserName = item.UserName;
                    reviewerInfo.UserPrincipalName = item.UserPrincipalName;
                    reviewerInfo.Email = item.Email;
                    reviewerInfo.DisplayName = item.DisplayName;
                    reviewerInfo.InviteType = item.InviteType;
                    reviewerInfo.RMUserId = item.RMUserId;
                    reviewerInfo.Id = item.Id;
                    reviewerInfo.SurName = item.SurName;
                    reviewerInfo.GivenName = item.GivenName;
                    reviewerInfo.TenantId = item.TenantId;
                    param.Reviewers.push(reviewerInfo);
                }
            }
            if (param.Position_X && param.Position_Y) {
                flowChartInfo.push(param);
            }
            if(flowChartNode.NodeType === NodeTypes.Destroy){
                destroyNodeId = flowChartNode.Id;
            }
            if(flowChartNode.NodeType === NodeTypes.NotDestroy){
                notDestroyNodeId = flowChartNode.Id;
            }
        }

        let needFullConnectNodeTypes = [
            NodeTypes.BeginDisposalReview,
            NodeTypes.DisposalReview,
            NodeTypes.Destroy,
            NodeTypes.NotDestroy
        ];
        for (let item of flowChartInfo) {
            
            if( needFullConnectNodeTypes.includes(item.NodeType)){
                let parentIds = this.getParentId(item.Id);
                if(item.ChildrenIds.length === 0 || parentIds.length === 0){
                    this.showMsgToast(RMResx.RM_RDM_WorkFlow_Msg_Unconnected,'error');
                    return false;
                }
            }

            if(item.NodeType == 1 || item.NodeType == 2){
                //最后一级disposal review必须连接destroy和not destroy.
                if( item.ChildrenIds.includes(destroyNodeId) && !item.ChildrenIds.includes(notDestroyNodeId)){
                    this.showMsgToast(RMResx.RM_RDM_WorkFlow_Msg_NotConnectedDestroyNode, 'error');
                    return false;
                }

                if(item.Reviewers.length == 0 && item.ReviewerType && item.ReviewerType == 0){
                    this.showMsgToast( RMResx.RM_RDM_WorkFlow_Msg_NotFillReviewers,'error');
                    return false;
                }
            }

            for (let item1 of RM.deepcopy(flowChartInfo)) {
                if (item1.ChildrenIds.indexOf(item.Id) != -1) {
                    item.ParentId = item1.Id;
                    break;
                }
            }
        }
        workFlowInfo.Name = this.state.profileName || null;
        workFlowInfo.Description = this.state.description || null;
        workFlowInfo.Content = {};
        workFlowInfo.Content.WorkflowNodes = flowChartInfo;
        workFlowInfo.Content.ZoomNum = this.zoomNum;
        //编辑时的id
        this.setState({isWorkflowChanged: false});
        if (this.currentWorkflowId) {
            workFlowInfo.Id = this.currentWorkflowId;
            workFlowInfo.ReferenceId = this.ReferenceId;
            this.getIsUpgradeWorkflowVersion(workFlowInfo);
        } else {
            this.saveManualProcess(workFlowInfo);
        }
    }

    getIsUpgradeWorkflowVersion(workFlowInfo) {
        let option = {
            url: "/Api/CPApi/IsUpgradeWorkflowVersion",
            method: "post",
            data: workFlowInfo
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            if (res) {
                this.showEditMessageBox(workFlowInfo);
            } else {
                this.saveManualProcess(workFlowInfo);
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    showEditMessageBox(workFlowInfo) {
        $$.messagedialog(true, {
            // classify: "info",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_RDM_Msg_UpgradeWorkflowVersion,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.saveManualProcess(workFlowInfo);
                        $$.messagedialog(false);
                    }
                }
            ]
        });
    }

    saveManualProcess(workFlowInfo) {
        $$.loading(true);
        let option = {
            url: "/Api/CPApi/SaveManualProcess",
            method: "post",
            data: workFlowInfo
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.MessageType === 0) {
                if (this.currentWorkflowId) {
                    RM.CommStatus.save(RM.CommStatus.EditSuccess);
                } else {
                    RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                }
                this.routerTo(RouterUrls.RDM_WorkFlowManagement);
            } else {
                this.showMsgToast(res.ErrorMessage,'error');
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onCancelWorkFlow() {
        if (this.state.isWorkflowChanged) {
            $$.messagedialog(true, {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_RC_DueDisposal_CancelPopup,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel,
                        onClick: () => {
                            $$.messagedialog(false);
                        }
                    },
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => {
                            this.setState({isWorkflowChanged: false});
                            this.routerTo(RouterUrls.RDM_WorkFlowManagement);
                            $$.messagedialog(false);
                        }
                    }
                ]
            });
        } else {
            this.routerTo(RouterUrls.RDM_WorkFlowManagement);
        }
    }

    onEditWorkFlow() {
        this.routerTo(`${RouterUrls.RDM_CreateWorkFlow}/?id=${this.currentWorkflowId}`);
    }

    onSaveDisConfiguration() {
        let callback = (success, data) => {
            if (success) {
                for (let item of this.state.flowChartInfo) {
                    if (this.currentNodeId == item.Id) {
                        item.Reviewers = data.reviewers;
                        item.DisplayName = data.displayName;
                        item.ReviewerType = data.reviewerType;
                        item.UsedEmailTemplateMode = data.UsedEmailTemplateMode;
                        item.UsedEmailTemplateId = data.UsedEmailTemplateId;
                        item.CustomIntervalSetting = data.CustomIntervalSetting;
                        item.GroupName = data.groupName;
                        item.IsAssignSiteOwnersChecked = data.isAssignSiteOwnersChecked;
                    }
                }
                this.setState({
                    showDisConfigurationPanel: {show: false},
                });
                this.setState({isWorkflowChanged: true});
            }
            this.setState({
                flowChartInfo: RM.deepcopy(this.state.flowChartInfo),
            });
        };
        this.dispatch('raWorkFlowConfiguration', 'save', callback);
        return false;
    }

    onCloseDisConfigurationPanel() {
        this.setState({
            showDisConfigurationPanel: { show: false },
            showViewDisConfigurationPanel: { show: false },
        });
    }

    viewDisConfiguration(reviewers, displayName, nodeInfo, reviewerType) {
        for (let item of reviewers) {
            item.Checked = true;
        }
        this.setState({
            showViewDisConfigurationPanel: {show: true},
            reviewers: reviewers,
            displayName: displayName,
            isViewReviewer: true,
            selectNodeInfo: nodeInfo,
            reviewerType: reviewerType
        });
    }
 
    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    // onClickShellInLine(dom) {
    //     $('.approve_select_action').css('display', 'none');
    //     $(dom).siblings('.approve_select_action').css('display', 'block');
    //     this.stopPropagation();
    // }

    // onSelectOptInLine(dom) {
    //     let jqDom = $(dom).parent().siblings('.approve_select_shell');
    //     jqDom.html($(dom).text());
    //     jqDom.attr('value', $(dom).attr('id'));
    //     $(dom).parent().css('display', 'none');
    //     this.stopPropagation();
    // }

    // stopPropagation() {
    //     $(document).ready(function () {
    //         $('.ra-approvalStatus-select').mouseover(function (event) {
    //             event.stopPropagation();
    //         });
    //         $('.ra-approvalStatus-select').dblclick(function (event) {
    //             event.stopPropagation();
    //         });
    //     });
    // }

    renderStartNode(id, className, style) {
        return <div className={className} id={id} style={style}>
            <div className='fia-event-calendar-arrow-right'></div>
        </div>;
    }

    renderPopover(isEndNode) {
        return <div className='ra-wf-node-popover' style={{ left: isEndNode ? "8px" : "150px" }}>
            <div className='ra-wf-upArrow'></div>
            <div className="ra-wf-popover-content">{RMResx.RM_RDM_WorkFlow_Msg_ErrorConnection}</div>    
        </div>;
    }

    renderDisposalReviewNode(id, className, style, nodeInfo) {
        nodeInfo = nodeInfo || {};
        let reviewers = nodeInfo.Reviewers;
        let displayName = nodeInfo.DisplayName;
        let disposalOpts = nodeInfo.NodeType == NodeTypes.BeginDisposalReview ? [this.state.disposalOpts[0]] : this.state.disposalOpts;
        let isMenuNode = id == "disposalReviewNode" || id == 'abb-disposalReviewNode';
        let reviewerType = nodeInfo.ReviewerType;

        let reviewersDom = <div>{RMResx.RM_RDM_WorkFlow_DefaultDisposalName}</div>;
        if (reviewers && reviewers.length > 0) {
            reviewersDom =
                <a className='ra-link-a ra-reviewer-text' onClick={this.viewDisConfiguration.bind(this, reviewers, displayName, nodeInfo, reviewerType)} tabIndex="0" onKeyDown={this.onKeyDown}>
                    {displayName}
                </a>;
        }else{
            if (nodeInfo.ReviewerType == 1 || nodeInfo.ReviewerType == 2 || nodeInfo.ReviewerType == 3) {
                reviewersDom =
                <a className='ra-link-a ra-reviewer-text' onClick={this.viewDisConfiguration.bind(this, reviewers, displayName, nodeInfo, reviewerType)} tabIndex="0" onKeyDown={this.onKeyDown}>
                    {displayName}
                </a>;  
            }
        }
        return <div id={id} className={className} style={style}>
            <div className='dr-node-left'>
                <div className='dr-node-icon'></div>
            </div>
            {
                <div className='dr-node-contain'>
                    <div className='dr-node-content'>
                        <div className='dr-node-content-title'>
                            <div
                                style={{width: (!isMenuNode ? '174px' : '220px')}}
                                className="text-overflow">{reviewersDom}</div>
                            {
                                isMenuNode && <div className='dr-node-right-icon'></div>
                            }
                        </div>
                    </div>
                    {
                        !isMenuNode && this.isSetUpWorkFlow && <div className='dr-node-option'>
                            <R.ButtonGroup type="action" height={200} tooltip={RMResx.RM_PRM_PRE_More}>
                                {
                                    disposalOpts.map((item, key) => (
                                        <R.Button
                                            key={key}
                                            onClick={this.onSelectOption.bind(this, item.type, id)}
                                            text={item.displayName}/>
                                    ))
                                }
                            </R.ButtonGroup>
                        </div>
                    }
                </div>
            }
            {
                !isMenuNode && this.renderPopover()
            }
        </div>;
    }

    renderDestroyNode(id, className, style) {
        let isMenuNode = id == "destroyNode" || id == 'abb-destroyNode';
        return <div id={id} className={className} style={style}>
            <div className='des-node-left'>
                <div className='des-node-icon'></div>
            </div>
            {
                <div className='des-node-text'>
                    {/* {
                        !isMenuNode && this.isSetUpWorkFlow && <div className='des-node-option'>
                            <R.ButtonGroup type="action"  height={200} tooltip={RMResx.RM_PRM_PRE_More}>
                                {
                                    this.state.destroyOpts.map((item, key) => (
                                        <R.Button
                                            key={key}
                                            onClick={this.onSelectOption.bind(this, item.type, id)}
                                            text={item.displayName}/>
                                    ))
                                }
                            </R.ButtonGroup>
                        </div>
                    } */}
                    <div style={{width: (!isMenuNode ? '174px' : '220px')}}>{RMResx.RM_RDM_WorkFlow_Destory}</div>
                    {
                        isMenuNode && <div className='des-node-right-icon'></div>
                    }
                </div>
            }
            {
                !isMenuNode && this.renderPopover()
            }
        </div>;
    }

    renderNoDestroyNode(id, className, style, nodeInfo) {
        let isMenuNode = id == "noDestroyNode" || id == 'abb-noDestroyNode';

        return <div id={id} className={className} style={style}>
            <div className='no-des-node-left'>
                <div className='no-des-node-icon'></div>
            </div>
            {
                <div className='no-des-node-text'>
                    {/* <div className='des-node-option'>
                        {
                            !isMenuNode && this.isSetUpWorkFlow && <R.ButtonGroup type="action" height={200} tooltip={RMResx.RM_PRM_PRE_More}>
                                {
                                    this.state.destroyOpts.map((item, key) => (
                                        <R.Button
                                            key={key}
                                            onClick={this.onSelectOption.bind(this, item.type, id, nodeInfo.NodeType)}
                                            text={item.displayName}/>
                                    ))
                                }
                            </R.ButtonGroup>
                        }
                    </div> */}
                    <div style={{width: (!isMenuNode ? '174px' : '220px')}}>{RMResx.RM_RDM_WorkFlow_DoNotDestory}</div>
                    {
                        isMenuNode && <div className='des-node-right-icon'></div>
                    }
                </div>
            }
            {
                !isMenuNode && this.renderPopover()
            }
        </div>;
    }

    renderEndNode(id, className, style) {
        return <div className={className} id={id} style={style}>
            <div className='fia-checkbox-three-state-device'></div>
            {this.renderPopover(true)}
        </div>;
    }

    renderWholeDragNodes() {
        return <div className='main-left' style={{display: (this.state.isShowWholeMenuPanel) ? 'block' : 'none'}}>
            <div className='whole-switch-icon' onClick={this.switchMenuPanel.bind(this, false)}>
                <span className='fia-calendar-arrow-left'></span>
            </div>
            <div className='node-title'>{RMResx.RM_RDM_WorkFlow_Word_Component}</div>
            <div className='dr-node-parent'>
                {this.renderDisposalReviewNode('disposalReviewNode', 'dr-node')}
            </div>
            {/* <div className='des-node-parent'>
                {this.renderDestroyNode('destroyNode', 'do-des-node')}
            </div>
            <div className='des-node-parent'>
                {this.renderNoDestroyNode('noDestroyNode', 'no-des-node')}
            </div> */}
            <div>
                {this.renderProfile()}
            </div>
            <div className="ra-wf-foot-btns">
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCancelWorkFlow} />
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSaveWorkFlow}/>               
            </div>
        </div>;
    }

    renderAbbDragNodes() {
        return <div className='abb-main-left' style={{display: (this.state.isShowWholeMenuPanel) ? 'none' : 'block'}}>
            <div className='aab-switch-icon' onClick={this.switchMenuPanel.bind(this, true)}>
                <span className='fia-calendar-arrow-right'></span>
            </div>
            <div className='dr-node-parent marginL35'>
                {this.renderDisposalReviewNode('abb-disposalReviewNode', 'dr-node')}
            </div>
            {/* <div className='des-node-parent marginL35'>
                {this.renderDestroyNode('abb-destroyNode', 'do-des-node')}
            </div>
            <div className='des-node-parent marginL35'>
                {this.renderNoDestroyNode('abb-noDestroyNode', 'no-des-node')}
            </div> */}
            <div className="abb-btns">
                <div className="abb-btn">
                    <R.Button
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={this.onCancelWorkFlow}/>
                </div>
                <div>
                    <R.Button
                        primary={true}
                        classify="theme"
                        className="abb-btns-save"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={this.onSaveWorkFlow}/>
                </div> 
            </div>
        </div>;
    }

    renderViewProfileInfo() {
        return <div className='main-left'>
            {this.renderDetailProfile()}
            {isMultiGeoMainDC && <div className="ra-wf-foot-btns">
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Edit}
                    onClick={this.onEditWorkFlow} />
            </div>}
        </div>;
    }

    renderProfile() {
        return <div className='profile-main'>
            <div className='profile-title'>{RMResx.RM_RDM_WorkFlow_Word_SaveProfile}</div>
            <div>
                <h4 className="profile-label require">{RMResx.RM_RDM_WorkFlow_Word_ProfileName}</h4>
                <R.Input
                    type="text"
                    width=""
                    value={this.state.profileName}
                    onChange={this.onChangeProfileName}
                    placeholder=""
                    aria={{ ariaLabel: RMResx.RM_RDM_WorkFlow_Word_ProfileName, ariaRequired: true }}
                />
            </div>
            <div>
                <h4 className="profile-label">{RMResx.RM_RDM_WorkFlow_Word_Description}</h4>
                <R.Input
                    type='textarea'
                    value={this.state.description}
                    onChange={this.onChangeDescription}
                    placeholder=""
                    aria={{ariaLabel:RMResx.RM_RDM_WorkFlow_Word_Description}}
                />
            </div>
        </div>;
    }

    renderDetailProfile() {
        return <div className='view-profile-main'>
            <div className='profile-title'>{RMResx.RM_RDM_WorkFlow_Word_Profile}</div>
            <div>
                <h4 className="profile-label">{RMResx.RM_RDM_WorkFlow_Word_ProfileName}</h4>
                <div className="profile-name">{this.state.profileName}</div>
            </div>
            <div>
                <h4 className="profile-label">{RMResx.RM_RDM_WorkFlow_Word_Description}</h4>
                <div className="profile-des">{this.state.description}</div>
            </div>
        </div>;
    }

    renderDragNodes() {
        if (this.isSetUpWorkFlow) {
            return <div id='raWorkFlowDragNodes'>
                {this.renderWholeDragNodes()}
                {this.renderAbbDragNodes()}
            </div>;
        }
        if (this.isViewWorkFlow) {
            return <div id='raWorkFlowDragNodes'>
                {this.renderViewProfileInfo()}
            </div>;
        }
    }

    renderEmptySpace() {
        return <div className='ra-wf-empty-space'>
            {RMResx.RM_RDM_WorkFlow_SpaceWord}
        </div>;
    }

    renderDragMenu() {
        return <div
            className='main-right'
            style={{
                width: this.state.isShowWholeMenuPanel ? 'calc(100% - 350px)' : 'calc(100% - 120px)',
                height: $('.main-left').height() + 'px',
            }}>
            <div className='ra-workflow-scaler'>
                <div className='ra-workflow-scaler-content'>
                    <div className='fia-plus zoom-btn' onClick={this.plusZoom}></div>
                    <div>{this.state.zoomNum}%</div>
                    <div className='fia-minus zoom-btn' onClick={this.reduceZoom}></div>
                </div>
            </div>
            <div className='main-right-content-wrapper'>
                <div id='raWorkFlowDiagramContainer' className='main-right-content'>
    
                    {
                        this.state.flowChartInfo.map((item, key) => {
                            return <React.Fragment key={key}>
                                {
                                    item.class.includes('start-node') &&
                                    this.renderStartNode(item.Id, item.class, item.style, item)
                                }
                                {
                                    item.class.includes('dr-node') &&
                                    this.renderDisposalReviewNode(item.Id, item.class, item.style, item)
                                }
                                {
                                    item.class.includes('do-des-node') &&
                                    this.renderDestroyNode(item.Id, item.class, item.style)
                                }
                                {
                                    item.class.includes('no-des-node') &&
                                    this.renderNoDestroyNode(item.Id, item.class, item.style)
                                }
                                {
                                    item.class.includes('end-node') &&
                                    this.renderEndNode(item.Id, item.class, item.style)
                                }
                            </React.Fragment>;
                        })
                    }
                    {
                        this.state.isShowEmptySpace &&
                        this.renderEmptySpace()
                    }
                </div>
            </div>
        </div>;

    }

    renderDisConfigurationPanel() {
        let isViewReviewer = this.state.isViewReviewer;
        let data = {
            reviewers: this.state.reviewers,
            displayName: this.state.displayName,
            isViewReviewer: isViewReviewer,
            reviewerType: this.state.reviewerType,
            selectNodeInfo: this.state.selectNodeInfo,
            groupName: this.state.groupName,
            isAssignSiteOwnersChecked: this.state.isAssignSiteOwnersChecked,
        };
        return <R.Panel
            id="raConfigurationPanel"
            header={RMResx.RM_RDM_WorkFlow_ConfigurationPanelTitle}
            size={600}
            status={this.state.showDisConfigurationPanel}
            destroy={true}
        >
            <div>
                <ConfigurationPanel
                    id='raWorkFlowConfiguration'
                    data={data}
                ></ConfigurationPanel>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCloseDisConfigurationPanel.bind(this)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveDisConfiguration.bind(this)} />
            </>
        </R.Panel>;
    }

    renderViewDisConfigurationPanel() {
        let isViewReviewer = this.state.isViewReviewer;
        let data = {
            reviewers: this.state.reviewers,
            displayName: this.state.displayName,
            isViewReviewer: isViewReviewer,
            selectNodeInfo: this.state.selectNodeInfo,
            reviewerType: this.state.reviewerType,
        };
        return <R.Panel
            id="raViewConfigurationPanel"
            header={RMResx.RM_RDM_WorkFlow_ConfigurationPanelTitle}
            size={600}
            status={this.state.showViewDisConfigurationPanel}
            destroy={true}
        >
            <div>
                <ConfigurationPanel
                    data={data}
                ></ConfigurationPanel>
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.onCloseDisConfigurationPanel.bind(this)} />
        </R.Panel>;
    }

    render() {
        return <div id='raWorkFlow'>
            <Prompt message={RMResx.RM_JS_RC_TUR_CancelMessage} when={this.state.isWorkflowChanged}/>
            {/*<$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_WorkFlow,SiteMapLinks.CP_WorkFlow]}/>*/}
            <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{show: this.state.tipStatus.show}}/>
            {this.renderDragNodes()}
            {this.renderDragMenu()}
            {this.renderDisConfigurationPanel()}
            {this.renderViewDisConfigurationPanel()}
        </div>;
    }
}

export default WorkFlow;