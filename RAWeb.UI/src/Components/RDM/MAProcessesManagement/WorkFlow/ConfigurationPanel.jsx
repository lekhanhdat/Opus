import { EmptyGUID } from "../../../../Constants/Constants";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import PeoplePicker from "../../../Common/PeoplePicker";
import { RMWorkflowStepUsedEmailTemplateMode } from "../Constants";

const  WorkflowReviewerType  = {
    RecordsUsers:0,
    SiteOwners:1,
    SPUserGroup:2,
    InformationOwner:3
};

export default class ConfigurationPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.displayName = RM.deepcopy(this.props.data).displayName;
        this.state = {
            reviewers: RM.deepcopy(this.props.data).reviewers,
            isViewReviewer: RM.deepcopy(this.props.data).isViewReviewer,
            displayName: this.displayName,
            showRequireViewReviewer: false,
            peoplePickerDis: false,
            displayNameDis: false,
            reviewerDis: false,
            reviewerType:RM.deepcopy(this.props.data).reviewerType || WorkflowReviewerType.RecordsUsers,
            templateList: [],
            emailTemplateMode: RM.deepcopy(this.props.data.selectNodeInfo).UsedEmailTemplateMode || RMWorkflowStepUsedEmailTemplateMode.Default,
            selectedEmailTemplateId: RM.deepcopy(this.props.data.selectNodeInfo).UsedEmailTemplateId,
            showRequireEmailTemplate: false,
            customIntervalSetting : RM.deepcopy(this.props.data.selectNodeInfo).CustomIntervalSetting || [{ Interval : 0, UsedEmailTemplateId : ""}],
            showValidateMessage: false,
            showRequireIntervalEmailTemplate: false, 
            showRequireSetInterval: false, 
            groupName: RM.deepcopy(this.props.data).groupName,
            showRequireGroupName: false,
            isAssignSiteOwnersChecked: RM.deepcopy(this.props.data).isAssignSiteOwnersChecked,
        };
        this.bind(["onPeopleSelectionChanged", "handleDisplayNameChange","onReviewerTypeChange"]);
    }

    componentInit() {
        this.getCustomEmailTemplateList();
    }

    getCustomEmailTemplateList(){
        let option = {
            url: "/Api/CPApi/GetAllCustomEmailTemplates",
            method: "post"
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let templateOptions = res.map((item)=>{
                item.checked = item.UniqueId === this.state.selectedEmailTemplateId;
                return item;
            });
            this.setState({
                templateList: templateOptions
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleDisplayNameChange(value) {
        this.displayName = value;
    }

    handleGroupNameChange = (value) => {
        this.setState({
            groupName: value,
            showRequireGroupName: value.trim() === "" || value === null,
        });
    }

    onAssignSiteOwnersChanged = (args) => {
        this.setState({ isAssignSiteOwnersChecked: args });
    }

    onPeopleSelectionChanged(users) {
        this.setState({
            reviewers: users,
            showRequireViewReviewer: users.length == 0
        });
    }

    componentReceive(optionType, data) {
        if (optionType == 'save') {
            this.onSave(data);
        }
    }

    getReviewersStr(reviewers) {
        let reviewersStr = RMResx.RM_RDM_WorkFlow_DefaultDisposalName;
        if (reviewers.length == 1) {
            reviewersStr = RMResx.RM_RDM_WorkFlow_DisposalNameByReviewer.replace("{0}", reviewers[0].DisplayName);
        }
        if (reviewers.length > 1) {
            reviewersStr = RMResx.RM_RDM_WorkFlow_DisposalNameByReviewer.replace("{0}", `${reviewers[0].DisplayName}...`);
        }   
        return reviewersStr;
    }

    getDispsoalName() {
        let reviewers = RM.deepcopy(this.props.data).reviewers;
        let displayName = '';
        let reviewersStr = '';
        if (this.state.reviewers) {
            reviewersStr = this.getReviewersStr(reviewers);
        }
        if(this.displayName == RMResx.RM_RDM_WorkFlow_DisposalNameByRecordOwner){
            reviewersStr = this.getReviewersStr(this.state.reviewers);
        }
        if (reviewersStr == this.displayName) {
            displayName = this.getReviewersStr(this.state.reviewers);
            if (this.state.reviewerType) {
                if (this.state.reviewerType == WorkflowReviewerType.SiteOwners) {
                    displayName = RMResx.RM_RDM_WorkFlow_DisposalNameByRecordOwner;
                } else if (this.state.reviewerType == WorkflowReviewerType.SPUserGroup) {
                    displayName = RMResx.RM_RDM_WorkFlow_DisposalNameByReviewer.replace("{0}", this.state.groupName.trim());
                } else if (this.state.reviewerType == WorkflowReviewerType.InformationOwner) {
                    displayName = RMResx.RM_RDM_WorkFlow_DisposalNameByInformationOwner;
                }
            }
        } else {
            if (this.state.reviewerType) {
                if (this.state.reviewerType == WorkflowReviewerType.RecordsUsers) {
                    displayName = this.getReviewersStr(this.state.reviewers);
                } else if (this.state.reviewerType == WorkflowReviewerType.SiteOwners) {
                    displayName = RMResx.RM_RDM_WorkFlow_DisposalNameByRecordOwner;
                } else if (this.state.reviewerType == WorkflowReviewerType.SPUserGroup) {
                    displayName = RMResx.RM_RDM_WorkFlow_DisposalNameByReviewer.replace("{0}", this.state.groupName.trim());
                } else if (this.state.reviewerType == WorkflowReviewerType.InformationOwner) {
                    displayName = RMResx.RM_RDM_WorkFlow_DisposalNameByInformationOwner;
                }
            } else {
                displayName = this.displayName;
            }
        }
        return displayName;
    }

    onSave(callBack) {
        let reviewerType = this.state.reviewerType * 1;
        let param = {
            reviewers: this.state.reviewers,
            displayName: this.getDispsoalName(),
            reviewerType : reviewerType,
            UsedEmailTemplateMode: this.state.emailTemplateMode,
            UsedEmailTemplateId: this.state.selectedEmailTemplateId,
            CustomIntervalSetting : this.state.customIntervalSetting,
            groupName: this.state.groupName,
            isAssignSiteOwnersChecked: this.state.isAssignSiteOwnersChecked,
        };
        let isNotSelectRecordsUsers = this.state.reviewerType == WorkflowReviewerType.RecordsUsers 
            && this.state.reviewers.length == 0;
        let isNotSelectEmailTemplate = this.state.emailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Specify 
            && !this.state.selectedEmailTemplateId; 
        let isNotSetGroupName = this.state.reviewerType == WorkflowReviewerType.SPUserGroup
            && (this.state.groupName.trim() === "" || this.state.groupName === null);

        if(this.state.emailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom){
            let result = false;
            if(this.state.customIntervalSetting.length < 2){
                callBack(false);
                this.setState({
                    showRequireSetInterval : true,
                });
                return false;
            }
            this.state.customIntervalSetting.map((item) => {
                if(item.UsedEmailTemplateId === ""){
                    result = true;
                }
            });
            if(result){
                callBack(false);
                this.setState({
                    showRequireIntervalEmailTemplate : true,
                });
                return false;
            }
        }
            
        if(isNotSelectRecordsUsers || isNotSelectEmailTemplate || isNotSetGroupName){
            callBack(false);
            this.setState({
                showRequireViewReviewer: isNotSelectRecordsUsers,
                showRequireEmailTemplate: isNotSelectEmailTemplate,
                showRequireGroupName: isNotSetGroupName,
            });
            return false;
        }
        callBack(true, param);
    }

    onReviewerTypeChange(val){
        if(val){
            this.setState({
                reviewerType:val, 
                reviewers:[],
                groupName: "",
                isAssignSiteOwnersChecked: true,
                showRequireViewReviewer: false,
                showRequireGroupName: false,
            });
        }
    }

    onChangeEmailTemplateMode = (value) =>{
        this.setState({ 
            emailTemplateMode: value,
            selectedEmailTemplateId: "",
            showRequireEmailTemplate: false
        });
    }

    onChangeEmailTemplate = (args) => {
        this.setState({ 
            selectedEmailTemplateId: args.newValue.UniqueId,
            showRequireEmailTemplate: false
        });
    }

    onChangeIntervalEmailTemplate = (index,args) => {
        const clonedSetting = RM.deepcopy(this.state.customIntervalSetting);
        clonedSetting[index].UsedEmailTemplateId = args.newValue.UniqueId;
        this.setState({
            customIntervalSetting : clonedSetting,
            showRequireIntervalEmailTemplate: false,
            showValidateMessage: false,
            showRequireSetInterval : false,
        });
    }

    removeCondition = (index) => {
        const clonedSetting = RM.deepcopy(this.state.customIntervalSetting);
        clonedSetting.splice(index, 1);
        this.setState({
            customIntervalSetting : clonedSetting,
            showValidateMessage: false,
            showRequireIntervalEmailTemplate: false,
            showRequireSetInterval: false
        });
    };

    addCondition = (index) => {
        const clonedSetting = RM.deepcopy(this.state.customIntervalSetting);
        if (clonedSetting.length < 5) {    
            clonedSetting.splice(index + 1, 0, { Interval: 1, UsedEmailTemplateId: "" });
            this.setState({
                customIntervalSetting : clonedSetting,
                showValidateMessage: false,
                showRequireIntervalEmailTemplate: false,
                showRequireSetInterval: false
            });
        } else {
            this.setState({
                showValidateMessage: true,
            });
        }
    };

    onChangeIndexInterval = (index, value) => {
        const clonedSetting = RM.deepcopy(this.state.customIntervalSetting);
        if (value === null || value === "") {
            value = "1";
        }
        clonedSetting[index].Interval = parseInt(value);
        this.setState({
            customIntervalSetting : clonedSetting,
        });
    }

    getSelectTemplate = (id) => {
        const templateList = RM.deepcopy(this.state.templateList);
        let templateOptions = [{
            checked: false,
            UniqueId : EmptyGUID,
            Name : RMResx.RM_CP_Email_ManualApprovalForRecordsReviewer + " " + RMResx.RM_RDM_WorkFlow_DefaultTemplate,
        }];

        templateList.forEach((item)=>{
            templateOptions.push(item);
        });

        templateOptions.forEach((item) => {
            item.checked = item.UniqueId === id;
        });

        return templateOptions;
    }

    renderBuildInColumn = (advanced, index) => {
        return <div className="ra-advance-group-popup-row" key={`advanced_${index}`}>
            <div className="ra-custom-build-title">
                <div className="ra-advance-group-text">
                    {`${index + 1}. `}
                </div>
                <div className="ra-advance-group-text">
                    {RMResx.RM_RDM_WorkFlow_IntoStage}
                </div>
            </div>
            <div className="ra-custom-build-interval">
                <div className="ra-custom-build-interval-style">
                    <R.Combobox
                        id="raApCustomNotificationCbx"
                        noneText={RMResx.RM_RDM_WorkFlow_SearchNoText}
                        width="100%"
                        textField="Name"
                        valueField="UniqueId"
                        items={this.getSelectTemplate(advanced.UsedEmailTemplateId)}
                        onChange={this.onChangeIntervalEmailTemplate.bind(this, index)}
                    />
                </div>
            </div>
            <R.Button
                type="bald"
                icon="crm-criteria fia-plus"
                tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                onClick={this.addCondition.bind(this, index)}
            />
        </div>;
    }

    mapAdvanced = (advanced, index) => {
        return <div className="ra-advance-group-popup-row" key={`advanced_${index}`}>
            <div className="ra-custom-build-title">
                {this.state.customIntervalSetting.length > 1 && <div className="ra-advance-group-text">
                    {`${index + 1}. `}
                </div>}
                <div className="ra-advance-group-text">
                    {RMResx.RM_MA_Setting_Advanced_After}
                </div>
            </div>
            <div className="ra-custom-interval-input">
                <R.Input
                    key={Math.random()}
                    type="number"
                    min={1}
                    width={"100%"}
                    value={advanced.Interval}
                    hasControl
                    onChange={this.onChangeIndexInterval.bind(this, index)}
                />
            </div>
            <div className="ra-advance-group-text">{RMResx.RM_JS_ScheduleSetting_Days}</div>
            <div className="ra-custom-interval-template">
                <div className="ra-custom-interval-template-style">
                    <R.Combobox
                        id="raApCustomNotificationCbx"
                        noneText={RMResx.RM_RDM_WorkFlow_SearchNoText}
                        width="100%"
                        textField="Name"
                        valueField="UniqueId"
                        items={this.getSelectTemplate(advanced.UsedEmailTemplateId)}
                        onChange={this.onChangeIntervalEmailTemplate.bind(this, index)}
                    />
                </div>
            </div>
            
            {this.state.customIntervalSetting.length > 1 && <R.Button
                type="bald"
                icon="crm-criteria fia-close"
                tooltip={RMResx.RM_JS_Common_Delete}
                onClick={this.removeCondition.bind(this, index)}
            />}
            <R.Button
                type="bald"
                icon="crm-criteria fia-plus"
                tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                onClick={this.addCondition.bind(this, index)}
            />
        </div>;
    };

    renderDetailRow(value){
        return <div>{value}</div>;
    }

    renderEmailSettingContent(){
        switch(this.state.emailTemplateMode){
            case RMWorkflowStepUsedEmailTemplateMode.Default:
                return this.renderDetailRow(RMResx.RM_RDM_WorkFlow_UseGlobal);
            case RMWorkflowStepUsedEmailTemplateMode.Specify:
                return this.renderDetailRow(this.state.templateList.find(
                    item => item.UniqueId == this.state.selectedEmailTemplateId)?.Name
                );
            case RMWorkflowStepUsedEmailTemplateMode.Custom:
                return this.renderDetailRow(RMResx.RM_RDM_WorkFlow_CustomInterval);
        }
    }

    renderEmailSettingView(){
        return <React.Fragment>          
            {this.renderEmailSettingContent()}
        </React.Fragment>;
    }

    renderCustomSettingView(){
        return <div className="ra-view-custom-interval">
            {this.state.customIntervalSetting.map((setting,index) =>{
                let templateName = RMResx.RM_CP_Email_ManualApprovalForRecordsReviewer + " " + RMResx.RM_RDM_WorkFlow_DefaultTemplate;
                this.state.templateList.forEach((item) => {
                    if(item.UniqueId == setting.UsedEmailTemplateId){
                        templateName = item?.Name;
                    }
                });
                var intervalUnit = setting.Interval > 1 ? RMResx.RM_RDM_WorkFlow_ViewDays : RMResx.RM_RDM_WorkFlow_ViewDay;
                if(index === 0){
                    return <div key={index}>
                        <span>
                            {`${index + 1}. ` + RMResx.RM_RDM_WorkFlow_IntoStage + "; " + templateName}
                        </span>
                    </div>;
                }
                return <div key={index}>
                    <span>
                        {`${index + 1}. ` + RMResx.RM_MA_Setting_Advanced_After + " " + setting.Interval + " " + intervalUnit + "; " + templateName}
                    </span>
                </div>;
            })}
        </div>;
    }

    renderViewConfigHtml() {
        let reviewers = this.state.reviewers;
        let reviewerType = this.state.reviewerType;
        return <div>
            <div className="ra-view-displayName">{this.state.displayName}</div>
            <$g.DetailList className="category-content" labelWidth={180}>
                <$g.DetailRow>
                    <$g.DetailCell label={RMResx.RM_RDM_WorkFlow_ReviewerText}>
                        {reviewers.length > 0 && <div>
                            {
                                reviewers.map((item,index) => {
                                    if(index !== reviewers.length - 1){
                                        return item.DisplayName + "; ";
                                    }
                                    return item.DisplayName;
                                })
                            }
                        </div>}
                        {
                            reviewerType == WorkflowReviewerType.SiteOwners && reviewers.length == 0 && <div className='margin-top-s'>
                                {RMResx.RM_RDM_WorkFlow_RecordOwnerText}
                            </div>
                        }
                        {
                            reviewerType == WorkflowReviewerType.SPUserGroup && <div className='margin-top-s'>
                                {this.props.data.selectNodeInfo.GroupName}
                            </div>
                        }
                        {
                            reviewerType == WorkflowReviewerType.InformationOwner && <div className='margin-top-s'>
                                {RMResx.RM_RDM_WorkFlow_InformationOwnerText}
                            </div>
                        }
                    </$g.DetailCell>
                </$g.DetailRow>
                <$g.DetailRow>
                    <$g.DetailCell label={RMResx.RM_RDM_WorkFlow_Notification}>
                        {this.renderEmailSettingView()}
                    </$g.DetailCell>
                </$g.DetailRow>
            </$g.DetailList>
            {this.state.emailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom && 
            <div>
                <div className="ra-view-custom-title">{RMResx.RM_RDM_WorkFlow_View_CustomInterval}</div>
                <div className="ra-view-custom-content">
                    {this.renderCustomSettingView()}
                </div>
            </div>}
        </div>;
    }

    renderEditConfigHtml() {
        return <div>
            <div className='ra-disposal-name-title'>{RMResx.RM_RDM_WorkFlow_DisposalNameText}</div>
            <R.Input
                width={"100%"}
                type="text"
                value={this.state.displayName}
                onChange={this.handleDisplayNameChange} aria={{ariaLabel:RMResx.RM_RDM_WorkFlow_DisposalNameText}}/>
            <div className='ra-reviewer-title require'>{RMResx.RM_RDM_WorkFlow_ReviewerText}</div>
            <$g.RadioGroup 
                name="ma-configure-reviewer"
                className="ra-reviewer-type" 
                onChange={this.onReviewerTypeChange} 
                value={this.state.reviewerType + ""}
            >
                <$g.RadioOption value = "0" text = {RMResx.RM_RDM_WorkFlow_AssignToUser}>
                    <div className={'ra-reviewer-input ' + (this.state.reviewerType == "0" ? "block" : "none")}>
                        <PeoplePicker
                            width='100%'
                            items={this.state.reviewers}
                            selectionChanged={this.onPeopleSelectionChanged}
                        />
                        <$g.ValidationMsg show={this.state.showRequireViewReviewer}>
                            {RMResx.RM_RDM_WorkFlow_Selecte_Reviewer_Valid}
                        </$g.ValidationMsg>
                    </div>
                </$g.RadioOption>
                <$g.RadioOption value = "1" text = {RMResx.RM_RDM_WorkFlow_AssignToSite}>
                    <$g.Popover>{RMResx.RM_RDM_WorkFlow_AssignToSiteDes}</$g.Popover>
                </$g.RadioOption>
                <$g.RadioOption value="2" text={RMResx.RM_RDM_WorkFlow_AssignToUserGroup}>
                    <$g.Popover>{RMResx.RM_RDM_WorkFlow_AssignToUserGroupDes}</$g.Popover>
                    <div className={'ra-reviewer-input ' + (this.state.reviewerType == "2" ? "block" : "none")}>
                        <R.Input
                            id="raGroupNameInput"
                            type="text"
                            placeholder={RMResx.RM_RDM_WorkFlow_AssignToUserGroup_Placeholder}
                            value={this.state.groupName}
                            onChange={this.handleGroupNameChange}
                        />
                        <$g.ValidationMsg show={this.state.showRequireGroupName}>
                            {RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]}
                        </$g.ValidationMsg>

                        <div className="margin-top-s">
                            <R.Checkbox
                                id="raUserGroupChk"
                                text={RMResx.RM_RDM_WorkFlow_AssignToSiteOwner}
                                checked={this.state.isAssignSiteOwnersChecked}
                                onChange={this.onAssignSiteOwnersChanged}
                            />
                        </div>
                    </div>
                </$g.RadioOption>
                {LicenseHelper.EnableJPMCFileSystemFeature() && (
                    <$g.RadioOption value="3" text={RMResx.RM_RDM_WorkFlow_AssignToInformationOwner}>
                        <$g.Popover>{RMResx.RM_RDM_WorkFlow_AssignToInformationOwnerDes}</$g.Popover>
                    </$g.RadioOption>
                )}
            </$g.RadioGroup>   
            {this.renderSendEmailType()}         
        </div>;
    }

    renderSendEmailType(){
        return <div id={this.props.id}>
            <div className='ra-reviewer-title require'>{RMResx.RM_RDM_WorkFlow_NotificationTitle}</div>
            <$g.RadioGroup 
                name="ma-send-email-type"
                className="ra-ma-send-email-type" 
                onChange={this.onChangeEmailTemplateMode} 
                value={this.state.emailTemplateMode}
            >
                <$g.RadioOption value={RMWorkflowStepUsedEmailTemplateMode.Default} text={RMResx.RM_RDM_WorkFlow_UseGlobal}></$g.RadioOption>
                <$g.RadioOption 
                    isBlock
                    value={RMWorkflowStepUsedEmailTemplateMode.Specify} 
                    text={RMResx.RM_RDM_WorkFlow_UseSpecify}>
                    {
                        this.state.emailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Specify && 
                        <div className="ra-custom-email-template">
                            <R.Combobox
                                id="raApCustomNotificationCbx"
                                noneText={RMResx.RM_RDM_WorkFlow_SearchNoText}
                                width="100%"
                                textField="Name"
                                valueField="UniqueId"
                                items={this.state.templateList}
                                onChange={this.onChangeEmailTemplate}
                            /> 
                            <$g.ValidationMsg show={this.state.showRequireEmailTemplate}>
                                {RMResx.RM_AR_CP_Common_SelEmpty}
                            </$g.ValidationMsg>
                        </div>
                    }
                </$g.RadioOption>
                <$g.RadioOption
                    value={RMWorkflowStepUsedEmailTemplateMode.Custom}
                    text={
                        <div className="ra_workflow_used_email_text">
                            {RMResx.RM_RDM_WorkFlow_CustomInterval}
                            <$g.Popover style={{margin: "0 8px"}}>
                                {RMResx.RM_RDM_WorkFlow_CustomIntervalMsg}
                            </$g.Popover>
                        </div>
                    }
                ></$g.RadioOption>
                {
                    this.state.emailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom && 
                    <>
                        <div className="ra-custom-email-inteval">
                            {this.state.customIntervalSetting.map((advanced, index) => {
                                if(index === 0){
                                    return <div key={index} className="ra-custom-after">
                                        {this.renderBuildInColumn(advanced, index)}
                                    </div>;
                                }
                            })}
                            <div className={"ra-custom-group"}>
                                {this.state.customIntervalSetting.map((advanced, index) => {
                                    if(index !== 0){
                                        return this.mapAdvanced(advanced, index);
                                    }
                                })}
                            </div>
                            <div className="ra-validation-msg" style={{ marginTop: "5px" }} tabIndex="0" hidden={!this.state.showValidateMessage}>{RMResx.RM_MA_Custom_Interval_AddError}</div>
                            <div className="ra-validation-msg" style={{ marginTop: "5px" }} tabIndex="0" hidden={!this.state.showRequireIntervalEmailTemplate}>{RMResx.RM_AR_CP_Common_SelEmpty}</div>
                            <div className="ra-validation-msg" style={{ marginTop: "5px" }} tabIndex="0" hidden={!this.state.showRequireSetInterval}>{RMResx.RM_MA_Custom_Interval_OnlyFirst}</div>
                        </div>
                    </>             
                }    
            </$g.RadioGroup>  
        </div>;
    }

    render() {
        if (this.state.isViewReviewer) {
            return this.renderViewConfigHtml();
        } else {
            return this.renderEditConfigHtml();
        }
    }
}