import { Component } from "react";
import { bindEvents, isShowActionByDC } from "../../../Utilities/CommonUtil";
// import * as Constants from "./Constants";

import "../../../Less/RDM/MAProcessesManagement.less";

const isMultiGeoMainDC = isShowActionByDC();
const ReviewerType = {
    RecordsUsers: 0,
    SiteOwners: 1,
    SPUserGroup: 2,
    InformationOwner: 3,
};
export class MAProcessCardView extends Component {
    constructor(props){
        super(props);
        this.state = {
            item: this.props.item
        };
        bindEvents(this, "handleEditProcess", "handleDelProcess", "handleViewProcess");
    }

    handleEditProcess(){
        this.props.handleEditProcess(this.state.item.Id);
    }

    handleDelProcess(){
        this.props.handleDelProcess(this.state.item.Id);
    }

    handleViewProcess(){
        this.props.handleViewProcess(this.state.item.Id);
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    rendProcesses(){
        let processName = this.state.item.Name,
            desc = this.state.item.Description;
        return <div className={"template-card-main"}>
            <div className={"template-card-header"}>
                <div className="">
                    <div className="info-normal info-name" data-tooltip onClick={this.handleViewProcess} tabIndex="0" onKeyDown={this.onKeyDown}>{processName}</div>
                    <div className="info-normal info-desc" data-tooltip aria-label={desc}>{desc}</div>
                </div>
            </div>
            <div className="template-card-body">
                {this.renderProcessInfo()}
            </div>
            <div className="template-card-footer temp-info-footer-time">
                <span className="card-foot-left" data-tooltip="ifneed">{RMResx.RM_RDM_MAProcess_Created.replace("{0}",this.state.item.CreatedOnStr)}</span>
                {isMultiGeoMainDC && (
                    <span className='btn-group'>
                        {
                            <span className="process-btn">
                                <R.Button type="bald" tooltip={RMResx.RM_JS_Common_Edit} icon="fia-edit btn-edit" onClick={this.handleEditProcess} />
                            </span>
                        }
                        {
                            <span className="process-btn">
                                <R.Button type="bald" tooltip={RMResx.RM_JS_Common_Delete} icon="fia-delete btn-delete" onClick={this.handleDelProcess} />
                            </span>
                        }
                    </span>
                )}
            </div>
        </div>;
    }

    renderProcessInfo(){
        let userInfos = this.state.item.UserDisplayNames,
            userCount = userInfos? userInfos.length:0,
            userContent = JSON.parse(this.state.item.ContentStr),
            userType = -1;
        const ownerList = new Set();
        for(var i = 0;i<userContent.WorkflowNodes.length;i++){
            if (
                userContent.WorkflowNodes[i].ReviewerType ==
                ReviewerType.SiteOwners
            ) {
                userType = 1;
                ownerList.add(RMResx.RM_RDM_WorkFlow_RecordOwnerText);
            } else if (
                userContent.WorkflowNodes[i].ReviewerType ==
                ReviewerType.SPUserGroup
            ) {
                userType = 1;
                ownerList.add(userContent.WorkflowNodes[i].GroupName);
            } else if (
                userContent.WorkflowNodes[i].ReviewerType ==
                ReviewerType.InformationOwner
            ) {
                userType = 1;
                ownerList.add(RMResx.RM_RDM_WorkFlow_InformationOwnerText);
            }
        }
        let userDisplayNames = null;
        const ownerText = Array.from(ownerList).join(";");
        userCount += ownerList.size;
        if(userInfos && userType ==-1){
            userDisplayNames = userInfos.join(";");
        }else if(userInfos && userType ==1){
            userDisplayNames = userInfos.join(";") + ";" + ownerText;
        }else{
            userDisplayNames = ownerText;
        }
        let reviewerRow = <div className="temp-info-row">
            <div>
                <div className="temp-info-left" tabIndex="0">{RMResx.RM_RDM_WorkFlow_ReviewerText}</div>
                <div className="temp-info-right underLine" tabIndex="0" data-tooltip aria-label={userDisplayNames}>{userCount}</div>
            </div>
        </div>;
        let levelRow = <div className="temp-info-row">
            <div className="temp-info-left" tabIndex="0">{RMResx.RM_RDM_MAProcess_Word_ApprovalLevel}</div>
            <div className="temp-info-right" tabIndex="0">{this.state.item.LevelCount}</div>
        </div>;

        return <div className="temp-info-main">
            {reviewerRow}
            {levelRow}
        </div>;
    }

    render(){
        return <React.Fragment>
            <div className="col-xlg-3 col-xs-3">
                <div className="ra-section">
                    {this.rendProcesses()}
                </div>
            </div>
        </React.Fragment>;
    }
}

