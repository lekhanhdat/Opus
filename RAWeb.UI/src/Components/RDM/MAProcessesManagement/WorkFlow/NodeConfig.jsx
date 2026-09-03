import StringUtil from "../../../../Utilities/StringUtil";

let BaseStyle = {
    isSource: true, //是否可以拖动（作为连线起点）
    isTarget: true, //是否可以放置（连线终点）
    connector: ['Flowchart',{midpoint: 0.02, cornerRadius: 5}], 
    maxConnections: -1,
    renderMode: "canvas",
    connectorOverlays: [],
    connectionsDetachable: false,
    endpointStyle: {
        fill: '#207580',
        radius: 7
    },
    paintStyle: {
        fill: '#207580',
        strokeStyle: '#61B7CF',
        strokeWidth: 1,
    },
    hoverPaintStyle: {
        strokeWidth: 2,
    },
    connectorStyle: {
        outlineStroke: '#207580',
        strokeWidth: 1,
    },
    connectorHoverStyle: {
        strokeWidth: 2,
        outlineStroke: '#07E9FF',
    }
};

const Status = {
    None: 0,
    Approve: 1,
    Reject: 2,
    Delay: 3,
    ApproveOrReject: 4
};

const NodeTypes = {
    Start: 0,
    BeginDisposalReview: 1,
    DisposalReview: 2,
    Destroy: 3,
    NotDestroy: 4,
    Delay: 5,
    End: 6,

};
const StatusName = {
    1: RMResx.RM_RDM_WorkFlow_Approve,
    2: RMResx.RM_RDM_WorkFlow_Reject,
    4: RMResx.RM_RDM_WorkFlow_ApprovOrReject
};

let WorkflowInitData = {
    "Name": "",
    "Description": '',
    "Content": {
        "WorkflowNodes": [
            {
                "Id": StringUtil.newGuid(),
                "Name": "1",
                "DisplayName": "",
                "Position_X": 640,
                "Position_Y": 215,
                "ChildrenIds": [],
                "Status": null,
                "NodeType": 0,
                "Reviewers": []
            },
            {
                "Id": StringUtil.newGuid(),
                "Name": "2",
                "DisplayName": RMResx.RM_RDM_WorkFlow_DefaultDisposalName,
                "Position_X": 721,
                "Position_Y": 210,
                "ChildrenIds": [],
                "Status": null,
                "NodeType": 1,
                "Reviewers": [],
                "ParentId": ""
            },
            {
                "Id": StringUtil.newGuid(),
                "Name": "3",
                "DisplayName": "",
                "Position_X": 415,
                "Position_Y": 650,
                "ChildrenIds": [],
                "Status": 1,
                "NodeType": 3,
                "Reviewers": [],
                "ParentId": ""
            },
            {
                "Id": StringUtil.newGuid(),
                "Name": "4",
                "DisplayName": "",
                "Position_X": 1019,
                "Position_Y": 650,
                "ChildrenIds": [],
                "Status": 2,
                "NodeType": 4,
                "Reviewers": [],
                "ParentId": ""
            },
            {
                "Id": StringUtil.newGuid(),
                "Name": "5",
                "DisplayName": "",
                "Position_X": 833,
                "Position_Y": 750,
                "ChildrenIds": [],
                "Status": null,
                "NodeType": 6,
                "Reviewers": [],
                "ParentId": ""
            },
        ],
        "ZoomNum": 1
    }
};
const NodeOptions = [
    {displayName: RMResx.RM_RDM_WorkFlow_Option_Configure, type: 1},
    {displayName: RMResx.RM_JS_Common_Delete, type: 2}
];
export {BaseStyle, Status, NodeTypes, StatusName, WorkflowInitData,NodeOptions};