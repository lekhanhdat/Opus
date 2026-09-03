import * as Constants from "./Constants";
import PeoplePicker from "../../PeoplePicker";
import PropTypes from "prop-types";

export default class EnableManualApproval extends R.Component {
    idAttr = true;
    componentCreate() {
        this.approvalData = {
            isRemove: false,
            isApproval: true,
            users: [],
            isUserOptionsShow: false,
            isSendEmail: false,                                                   //Send an e-mail notification to Record Owners
            userFilter: "",
            manualReviewType: Constants.ReviewType.Workflow,
            workflowId: ""
        };
        this.userFilter = "";
        this.state = {
            approvalData: this.approvalData,
            userOptions: [], // Enter usernames or group names: change下拉
            isLoading: false,
            noWorkflowSelect: false,
            workflowItems: this.props.workflowItems,
            addOwnersIsNoValidMsg: false,
            elementsEnable: false,
        };

        this.bind(["approvalClick", "sendEmailChange", "workflowMethodChange", "workflowSelectChange", "chooseOwnerMethodChange"]);
    }

    componentReceive(action, data, data1) {
        switch (action) {
            case Constants.dispatchAction.setData:
                this.setData(data);
                break;
            case Constants.dispatchAction.save: {
                let validation = this.verifyApprovalData();
                let approvalData = this.state.approvalData;
                this.props.getIsVerificationPassed(validation);
                this.props.getApprovalData(approvalData);
                break;
            }
            case Constants.dispatchAction.elementDisabled:
                this.setState({elementsEnable: data});
                break;
            case Constants.dispatchAction.clearData:
                this.conditionClearData();
                break;
            case Constants.dispatchAction.approvalCheckboxDisabledAndChecked:
                this.approvalData.approvalDisabled = data;
                this.approvalData.isApproval = data1;
                this.setState({
                    approvalData: this.deepCopy(this.approvalData)
                });
                break;
        }
    }

    updateWorkflowComboboxStatus(selectedWorkflowId){
        var items = RM.deepcopy(this.state.workflowItems);
        items.forEach(item=>{
            if(item.ReferenceId == selectedWorkflowId){
                item.Checked = true;
            }else{
                item.Checked = false;
            }
        });
        this.setState({workflowItems: items});
    }

    setData(data) {
        this.approvalData.isApproval = data.EnableManualApproval;
        if (data.EnableManualApproval) {
            this.approvalData.manualReviewType = data.ManualReviewType;
            if(data.ManualReviewType == Constants.ReviewType.Workflow){
                if (data.IsGControlManualApproval) {
                    const workflowItems = RM.deepcopy(this.state.workflowItems);
                    const newWorkflow = {
                        ReferenceId: data.WorkflowId,
                        Name: data.WorkflowName,
                        Checked: true,
                        disabled: true
                    }
                    workflowItems.push(newWorkflow);
        
                    let approvalData = this.approvalData;
                    approvalData.noSelectWorkflow = false;
                    approvalData.workflowId = newWorkflow.ReferenceId;
                    this.setState({workflowItems})
                } else {
                    this.approvalData.workflowId = data.WorkflowId;
                    this.updateWorkflowComboboxStatus(data.WorkflowId);
                }
            } else {
                this.approvalData.users = data.Users || [];
                this.approvalData.workflowId = "";
            }
            this.approvalData.isSendEmail = data.IsSendEmailToOwner;
        }
        this.setState({
            approvalData: this.deepCopy(this.approvalData)
        });
    }

    // Enable manual approval 点击
    approvalClick() {
        let item = this.approvalData;
        item.isApproval = !item.isApproval;
        this.setState({approvalData: this.deepCopy(item)});
    }

    verifyApprovalData(){
        let item = this.approvalData;
        if(item.isApproval){
            if(item.manualReviewType == Constants.ReviewType.RecordOwner){
                return this.validationAddingUsers();
            }else if(item.manualReviewType == Constants.ReviewType.Workflow){
                return this.verifyWorkflowInfo();
            }
        }
        return true;
    }

    verifyWorkflowInfo(){
        let item = this.approvalData;
        if(!item.workflowId && item.manualReviewType == Constants.ReviewType.Workflow){
            item.noSelectWorkflow = true;
            this.setState({
                approvalData: this.deepCopy(item)
            });
            return false;
        }
        return true;
    }

    validationAddingUsers(){
        let isValid = true;
        let addOwners = this.approvalData.users;
        let isSendEmail = this.approvalData.isSendEmail;
        if(addOwners.length == 0){
            isValid = !this.props.isSupportUserEmptyValidation && !isSendEmail ;
        }else{
            for(let item of addOwners){
                if(item.invalid){ isValid = false;  break;}
            }
        }
        this.setState({addOwnersIsNoValidMsg: !isValid});
        return isValid;
    }

    sendEmailChange(value) {
        this.approvalData.isSendEmail = value;
        this.setState({approvalData: this.deepCopy(this.approvalData)});
    }

    conditionClearData() {
        this.approvalData = {
            isRemove: false,
            isApproval: true,
            users: [],
            isUserOptionsShow: false,
            isSendEmail: false,                                                   //Send an e-mail notification to Record Owners
            userFilter: "",
            manualReviewType: Constants.ReviewType.Workflow,
            workflowId: ""
        };
        this.setState({
            userCount: 0
        });
        this.setState({
            approvalData: this.deepCopy(this.approvalData)
        });
        this.updateWorkflowComboboxStatus("");
    }

    //深复制
    deepCopy(value) {
        return JSON.parse(JSON.stringify(value));
    }

    workflowMethodChange(){
        let item = this.approvalData;
        item.manualReviewType = Constants.ReviewType.Workflow;
        this.setState({
            approvalData: this.deepCopy(item)
        });
    }

    chooseOwnerMethodChange(){
        let item = this.approvalData;
        item.manualReviewType = Constants.ReviewType.RecordOwner;
        this.setState({
            approvalData: this.deepCopy(item)
        });
    }

    workflowSelectChange(args){
        let item = args.newValue;
        let approvalData = this.approvalData;
        approvalData.noSelectWorkflow = false;
        approvalData.workflowId = item.ReferenceId;
        this.setState({
            approvalData: this.deepCopy(this.approvalData)
        });
    }

    onSelectionRecordsOwner = (items) =>{
        this.approvalData.users = items;
        this.setState({
            approvalData: this.deepCopy(this.approvalData)
        });
    }

    render() {
        let isUseWorkflow = this.state.approvalData.manualReviewType == Constants.ReviewType.Workflow,
            isUseRecordOwner = this.state.approvalData.manualReviewType == Constants.ReviewType.RecordOwner;
        let idPrefix = "raCr" + this.props.id;
        return <div>
            <div id="rm_createRule_manual" className={(!this.state.approvalData.approvalDisabled) ? "block" : "none"}>
                {
                    this.props.isShowTitle && <div className="ra-createRule-question flex ra-flex-align-center">
                        <div tabIndex={0} className="strong">{RMResx.RM_RDM_CreateRule_Title_EnableApproval}</div>
                        <$g.Popover>{RMResx.RM_JS_Rule_ManualAporovalDescription}</$g.Popover>
                    </div>
                }
                <div id="rm_crateRule_approval">
                    <R.Checkbox
                        id={idPrefix + "EnableChk"}
                        text={RMResx.RM_RDM_CreateRule_Options_EnableApproval}
                        disabled={this.state.approvalData.approvalDisabled || this.state.elementsEnable}
                        checked={this.state.approvalData.isApproval}
                        onChange={this.approvalClick}
                    />
                </div>
                <div id="roContent" className={(this.state.approvalData.isApproval) ? "cr-archive-action-children-selection" : "none"}>
                    <div className="margin-top-s">
                        <R.Radio
                            name={this.props.radioName}
                            text={RMResx.RM_RDM_CreateRule_Title_SelectProcess}
                            checked={isUseWorkflow}
                            onChange={this.workflowMethodChange}/>
                    </div>
                    {isUseWorkflow && <div id='rm_createRule_workflowSelector' className="cr-archive-action-children-selection margin-top-s">
                        <R.Combobox
                            id={idPrefix + "Workflow"}
                            width={"100%"}
                            textField='Name'
                            valueField='ReferenceId'
                            checkedField='Checked'
                            items={this.state.workflowItems}
                            onChange={this.workflowSelectChange}
                            searchPlaceholder={RMResx.RM_JS_RC_RUR_SelectRuleDefault}
                        />
                        <$g.ValidationMsg show={this.state.approvalData.noSelectWorkflow}>
                            {RMResx.RM_RDM_CreateRule_Msg_NoSelectProcess}
                        </$g.ValidationMsg>
                    </div>
                    }
                    <div className="sps-ro-summaryTitle margin-top-s">
                        <R.Radio
                            name={this.props.radioName}
                            text={RMResx.RM_SPS_MAChooseUsersTip}
                            checked={isUseRecordOwner}
                            onChange={this.chooseOwnerMethodChange}/>
                    </div>
                    {
                        isUseRecordOwner && <div className="cr-archive-action-children-selection margin-top-s">
                            <div className="user-title margin-bottom-s strong" tabIndex="0">{RMResx.RM_CP_AM_AddUser_Title}</div>
                            <PeoplePicker 
                                id={idPrefix + "ConfigureUsers"} 
                                height={"80"}
                                width={"100%"}
                                items={this.state.approvalData.users}
                                selectionChanged={this.onSelectionRecordsOwner}
                            />
                        </div>
                    }
                    <div className="cr-archive-action-children-selection">
                        <$g.ValidationMsg show={this.state.addOwnersIsNoValidMsg}>
                            {RMResx.RM_JS_CP_AM_AddUser_Nomatch}
                        </$g.ValidationMsg>
                    </div>
                    <div className="sps-ro-mailToOwner margin-top-s">
                        <R.Checkbox
                            id={idPrefix + "SendEmailChk"}
                            text={RMResx.RM_SPS_MASendMailToOwner}
                            checked={this.state.approvalData.isSendEmail}
                            name="archiveAction"
                            disabled={this.state.elementsEnable}
                            onChange={this.sendEmailChange}
                        />
                    </div>
                </div>
            </div>
        </div>;
    }
}

EnableManualApproval.propTypes = {
    isShowTitle: PropTypes.bool
};
EnableManualApproval.defaultProps = {
    isShowTitle: true
};

