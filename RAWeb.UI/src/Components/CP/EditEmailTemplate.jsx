import { Component } from "react";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import { bindEvents, isShowActionByDC, showToast } from "../../Utilities/CommonUtil";
import StringUtil from "../../Utilities/StringUtil";
import { ActionTypes, EmailTemplateInternalType, MovementEmailTemplateIds } from "./EmailTemplate/Contains";
import "../../Less/CP/EditEmailTemplate.less";


const isMultiGeoMainDC = isShowActionByDC();

const MovementTemplatePlaceholders = {
    [MovementEmailTemplateIds.EndUserSubmitted]: ["$Request.ID$", "$Request.Requester$"],
    [MovementEmailTemplateIds.RMAssigned]: ["$Request.ID$", "$Request.Assignee$", "$Request.Requester$", "$Request.Destination$", "$Request.Comment$"],
    [MovementEmailTemplateIds.ApprovedEndUser]: ["$Request.ID$", "$Request.Requester$", "$Request.Assignee$", "$Request.Successful.Count$", "$Request.Failed.Count$", "$Request.Comment$"],
    [MovementEmailTemplateIds.Rejected]: ["$Request.ID$", "$Request.Requester$", "$Request.Assignee$", "$Request.Assignee.Comment$"],
    [MovementEmailTemplateIds.ApprovedDestinationRM]: ["$Destination.RecordsManager$", "$Request.ID$", "$Request.Assignee$", "$Request.SourceLocation$", "$Request.Destination$", "$Request.Successful.Count$"],
    [MovementEmailTemplateIds.HoldManagerAssignment]: ["$Email.Recipient$", "$Hold.Title$"],
};

export default class EditEmailTemplate extends Component {
    constructor(props) {
        super(props);
        this.emailTemplateId = RM.Url.getParam(window.location.href, "id");
        this.copySourceId = RM.Url.getParam(window.location.href, "sourceid");
        this.isEditEmailTemplate = !!this.emailTemplateId; //Create does not pass a value.
        this.isCopyEmailTemplate = !!this.copySourceId;
        this.inputText = "";
        this.insertIndex = 0;
        this.selectInpurtId = "";
        this.templateType = EmailTemplateInternalType.ManualApproval;
        this.uniqueId = "";
        this.loadEmailTemplateApi = new Map(
            [
                [ActionTypes.COPY, "/Api/CPApi/GetAllEmailTemplateById?id=" + this.copySourceId + "&isCopy=true"],
                [ActionTypes.ADD, "/Api/CPApi/GetCustomDefaultEmailTemplate?type=" + this.templateType],
                [ActionTypes.EDIT, "/Api/CPApi/GetAllEmailTemplateById?id=" + this.emailTemplateId + "&isCopy=false"],
            ]
        );
        this.operateEmailTemplateApi = new Map(
            [
                [ActionTypes.ADD, "/Api/CPApi/CreateEmailTemplate"],
                [ActionTypes.EDIT, "/Api/CPApi/EditEamilTemplate"], 
            ]
        );
        this.state = {
            emailInfo: null,
            name: "",
            subject: "",
            cc: "",
            body: "",
            checked : false,
            showTip: false,
            tipType: "success",
            tipMsg: "",
            haveChange: false,
            imageList : [],
            isCustomTemplate: false,
            showTemplateTooLongMsg: false,
            showSubjectTooLongMsg: false,
            showCcTooLongMsg: false
        };
        this.initBingEvents();
        this.cachedBase64List = [];
        this.bodyStrLimitLength = 50000;
        this.uploadImgLimitNumForM = 10; 
    }

    componentDidMount() {
        this.loadEmailTemplate();
    }

    initBingEvents() {
        bindEvents(
            this,
            "handlePatameterDiv",
            "insertStr",
            "onBlur",
            "saveClick",
            "cancelClick",
            "showMessageTip",
            "showMsgToast",
            "hideMessageTip"
        );
    }

    loadEmailTemplate() {
        $$.loading(true);
        let emailTemplateUrl = "";
        if (this.isCopyEmailTemplate) {
            emailTemplateUrl = this.loadEmailTemplateApi.get(ActionTypes.COPY);
        } else if (this.isEditEmailTemplate) {
            emailTemplateUrl = this.loadEmailTemplateApi.get(ActionTypes.EDIT);
        } else {
            emailTemplateUrl = this.loadEmailTemplateApi.get(ActionTypes.ADD);
        }
        let option = {
            url: emailTemplateUrl,
            method: "POST",
        };
        fetchUtility(option)
            .then((result) => {
                $$.loading(false);
                let data = result;
                this.uniqueId = data.UniqueId;
                if(data.ImageList){
                    this.cachedBase64List = this.cachedBase64List.concat(data.ImageList);
                    data.Body = this.changeEditorContent(data.Body,data.ImageList);
                }
                if (data) {
                    data.Body = data.Body.replace(/\n/g,'<br/>');
                    this.setState({
                        emailInfo: data,
                        name: data.Name,
                        subject: data.Subject,
                        cc: data.CC,
                        body : data.Body,
                        type: data.Type,
                        checked : data.IsUseDefaultFooter === 0,
                        isCustomTemplate: data.IsCustomTemplate
                    });
                }

            })
            .catch((e) => { });
    }

    onChangeTemplateName = (value) => {
        if (value.length > 255) {
            this.setState({ showTemplateTooLongMsg: true });
        }
        else {
            this.setState({          
                showTemplateTooLongMsg: false,
            });
        }
        this.setState({
            name: value,
        });
             
    }

    onSubjectChange = (e) => {
        if (e.target.value.length > 255) {
            this.setState({ showSubjectTooLongMsg: true});
        }
        else {
            this.setState({
                showSubjectTooLongMsg: false,
            });
        }
        this.setState({
            subject: e.target.value,
            haveChange: true,
        });
        
    };

    onCCChange = (e) => {
        if (e.target.value.length > 255) {
            this.setState({ showCcTooLongMsg: true});
        }
        else {
            this.setState({ showCcTooLongMsg: false});
        }
        this.setState({
            cc: e.target.value,
            haveChange: true,
        });
    };

    onBlur(e) {
        if (e.currentTarget != null) {
            this.selectInpurtId = e.currentTarget.id;
            let textInput = document.getElementById(this.selectInpurtId);
            this.insertIndex = textInput.selectionStart;
        }
    }

    onEmailBodyEditorClick = () =>{
        this.selectInpurtId = "raEditEmailEditor";
    }

    onChange = () => {
        this.setState({
            checked : !this.state.checked,
        });
    }

    handlePatameterDiv(e) {
        if(this.selectInpurtId){
            if (this.selectInpurtId == "subjectInput" || this.selectInpurtId == "ccInput") {
                let inputIndex = this.insertIndex;
                let insetString = e.currentTarget.innerText;
                let id = this.selectInpurtId;
                let textInput = document.getElementById(id);
                let inputText = textInput.value;
                inputText = this.insertStr(inputText, inputIndex, insetString);
                if(this.selectInpurtId == "ccInput") {
                    return;
                }
                if(this.selectInpurtId == "subjectInput" && this.state.type === 3 && insetString === "$Request.Link$") {
                    return;
                }
                if (this.selectInpurtId == "subjectInput") {
                    this.setState({
                        subject: inputText,
                        haveChange: true,
                    });
                } 
            }else if(this.selectInpurtId == "raEditEmailEditor"){
                let insetString = e.currentTarget.innerText;
                this.editorRef.insertText(insetString);
            }
        }
    }

    insertStr(soure, index, insertStr) {
        return soure.substring(0, index) + insertStr + soure.substring(index);
    }

    getBodyEditorContent = () =>{
        let editorContent = this.editorRef.getValue();
        if(this.cachedBase64List.length > 0){
            for(let item of this.cachedBase64List){
                if(editorContent.includes(item.Base64)){
                    editorContent = editorContent.replace("data:image/"+ item.FileType +";base64," + item.Base64, "cid:"+item.ImageId);
                }
            }
        }
        return editorContent;
    }

    changeEditorContent = (content,imageList) => {
        if(imageList.length > 0){
            for(let item of imageList){
                if(content.includes(item.ImageId)){
                    content = content.replace("cid:" + item.ImageId,"data:image/"+ item.FileType +";base64," + item.Base64);
                }
            }
        }
        return content;
    }

    saveClick() {
        if(!$$.verify("rmEditEmailTemplateBody")){
            return; 
        }
        if (this.state.showTemplateTooLongMsg || this.state.showSubjectTooLongMsg || this.state.showCcTooLongMsg) {
            return;
        }
        let emailInfo = Object.assign({}, this.state.emailInfo);
        emailInfo.Name = this.state.name;
        emailInfo.Subject = this.state.subject;
        emailInfo.CC = this.state.cc;
        emailInfo.Body = this.getBodyEditorContent();
        emailInfo.IsUseDefaultFooter = this.state.checked ? 0 : 1; 
        if(emailInfo.Body && (emailInfo.Body.length > this.bodyStrLimitLength)){
            showToast.error(RMResx.RM_CP_EamilTemplate_LimitSize);
            return;
        }       
        if (this.isCopyEmailTemplate) {
            emailInfo.Id = 0;
            emailInfo.CopySourceId = this.copySourceId;
        }
        let option = {
            url: this.isEditEmailTemplate ? 
                this.operateEmailTemplateApi.get(ActionTypes.EDIT) : 
                this.operateEmailTemplateApi.get(ActionTypes.ADD),
            method: "POST",
            data: emailInfo,
        };
        $$.loading(true);
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if(result === ""){
                this.props.history.push({
                    pathname: RouterUrls.CP_EmailTemplate,
                    state: this.isEditEmailTemplate 
                });
                showToast.success(this.isEditEmailTemplate ? RMResx.RM_JS_CP_EamilTemplate_Success : RMResx.RM_JS_CP_EamilTemplate_CreateSuccess);
                return;
            } else if (result === "-2") {
                showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
            }
            showToast.error(result);
        }).catch((e) => {
            showToast.error(RMResx.RM_JS_CP_EamilTemplate_Failed);
            $$.loading(false);
        });
    }
    cancelClick() {
        this.props.history.push({
            pathname: RouterUrls.CP_EmailTemplate,
        });
    }
    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg,
        };
        this.setState(tipOption);
    };
    showMsgToast(content, type) {
        let option = {
            content: content,
            classify: type,
        };
        $$.toast(option);
    }
    hideMessageTip = () => {
        this.setState({ showTip: false });
    };

    doUpload = (args) => {
        if (Array.isArray(args) || args.toString() === '[object FileList]' || args.toString() === '[object File]') {
            let file = args[0];
            if(!file){
                showToast.success(RMResx.RM_SPS_Location_NoImportFile);
                return;
            }
            if(file.size > this.uploadImgLimitNumForM * 1024 * 1024){
                showToast.error(RMResx.RM_CP_EamilTemplate_LimitFileSize.format(this.uploadImgLimitNumForM));
                return;
            }
            const formData = new FormData();
            formData.append('fileUp', file, file.fileName);
            formData.append('templateId', this.state.isCustomTemplate ? this.uniqueId : this.emailTemplateId);
            return fetch("/api/CPApi/UploadImage", {
                method: 'POST',
                body: formData
            }).then(function(response){
                return response.text().then(function (dataString) {
                    return {
                        responseStatus: response.status,
                        responseString: JSON.parse(dataString)
                    };
                });
            })
                .then((result)=>{
                    let fileInfo = {
                        isSucceed: true,
                        file: Object.assign({}, file, {
                            fileId: StringUtil.newGuid(),
                            fileName: file.name,
                            url: "data:image/"+ result.responseString.FileType +";base64," + result.responseString.Base64,
                            fileExtension: file.name.split('.').reverse()[0],
                        }),
                        message: '',
                    };
                    this.cachedBase64List.push(result.responseString);
                    return fileInfo;
                });  
        }
    }

    doCopyUpload = (base64Url)=> {
        let file = this.base64ToFile(base64Url, StringUtil.newGuid());
        this.doUpload([file]);
    }

    base64ToFile(base64, fileName) {
        let data = base64.split(',');
        let type = data[0].match(/:(.*?);/)[1];
        let suffix = type.split('/')[1];
        const bstr = window.atob(data[1]);
        let n = bstr.length;
        const u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        const file =  new File([u8arr], `${fileName}.${suffix}`, {
            type: type
        });
        return file;
    }
  
    renderOthersParameters = () => {
        return (
            <div className="ra-editEmail-parameters">
                <div className="ra-editEmail-parameters-tooltip">
                    <div
                        className="ra-editEmail-parameters-title"
                        tabIndex="0"
                    >
                        {RMResx.RM_CP_EditEmainTemplate_SelectParameter}
                    </div>
                    <$g.Popover>{RMResx.RM_CP_EditEmainTemplate_Parameter}</$g.Popover>
                </div>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.ID$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Comment$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Requester$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Assignee$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Requester.FirstName$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$PhysicalRecords.Name$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$PhysicalRecords.UID$"}
                    </div>
                </a>
            </div>
        );
    };

    renderManualParameters = () => {
        return (
            <div className="ra-editEmail-parameters">
                <div className="ra-editEmail-parameters-tooltip">
                    <div
                        className="ra-editEmail-parameters-title"
                        tabIndex="0"
                    >
                        {RMResx.RM_CP_EditEmainTemplate_SelectParameter}
                    </div>
                    <$g.Popover>{RMResx.RM_CP_EditEmainTemplate_Parameter}</$g.Popover>
                </div>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Reviewer$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Comment$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Link$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Reviewer.FirstName$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Current.Date$"}
                    </div>
                </a>
            </div>
        );
    }

    renderExportParameters = () => {
        return (
            <div className="ra-editEmail-parameters">
                <div className="ra-editEmail-parameters-tooltip">
                    <div
                        className="ra-editEmail-parameters-title"
                        tabIndex="0"
                    >
                        {RMResx.RM_CP_EditEmainTemplate_SelectParameter}
                    </div>
                    <$g.Popover>{RMResx.RM_CP_EditEmainTemplate_Parameter}</$g.Popover>
                </div>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Reviewer$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.JobId$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Location$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Password$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Reviewer.FirstName$"}
                    </div>
                </a>
            </div>
        );
    }

    renderJobNotificationParameters = () => {
        return (
            <div className="ra-editEmail-parameters">
                <div className="ra-editEmail-parameters-tooltip">
                    <div
                        className="ra-editEmail-parameters-title"
                        tabIndex="0"
                    >
                        {RMResx.RM_CP_EditEmainTemplate_SelectParameter}
                    </div>
                    <$g.Popover>{RMResx.RM_CP_EditEmainTemplate_Parameter}</$g.Popover>
                </div>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Request.Reviewer$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Notification.Summary$"}
                    </div>
                </a>
            </div>
        );
    }

    renderHoldManageParameters = () => {
        return (
            <div className="ra-editEmail-parameters">
                <div className="ra-editEmail-parameters-tooltip">
                    <div
                        className="ra-editEmail-parameters-title"
                        tabIndex="0"
                    >
                        {RMResx.RM_CP_EditEmainTemplate_SelectParameter}
                    </div>
                    <$g.Popover>{RMResx.RM_CP_EditEmainTemplate_Parameter}</$g.Popover>
                </div>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Email.Recipient$"}
                    </div>
                </a>
                <a className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                    <div className="ra-editEmail-patameter-content">
                        {"$Hold.Reminder.Summary$"}
                    </div>
                </a>
            </div>
        ); 
    }

    renderDynamicParameters = (placeholders) => {
        return (
            <div className="ra-editEmail-parameters">
                <div className="ra-editEmail-parameters-tooltip">
                    <div className="ra-editEmail-parameters-title" tabIndex="0">
                        {RMResx.RM_CP_EditEmainTemplate_SelectParameter}
                    </div>
                    <$g.Popover>{RMResx.RM_CP_EditEmainTemplate_Parameter}</$g.Popover>
                </div>
                {placeholders.map((ph, index) => (
                    <a key={index} className="ra-link-a" tabIndex="0" onClick={this.handlePatameterDiv}>
                        <div className="ra-editEmail-patameter-content">
                            {ph}
                        </div>
                    </a>
                ))}
            </div>
        );
    };

    renderParameters() {
        const safeUniqueId = (this.uniqueId || "").toLowerCase();
        const dynamicPlaceholders = MovementTemplatePlaceholders[safeUniqueId];

        if (dynamicPlaceholders) {
            return this.renderDynamicParameters(dynamicPlaceholders);
        }

        if (this.state.type === 3 || this.state.type === 4) {
            return this.renderManualParameters();
        } else if (this.state.type === 5) {
            return this.renderExportParameters();
        } else if (this.state.type === 6){
            return this.renderJobNotificationParameters();
        } else if (this.state.type === 7){
            return this.renderHoldManageParameters();
        }
        else {
            return this.renderOthersParameters();
        }
    }

    renderCustomFooter(){
        return (
            <>
                <div className="row a">
                    <div className="col-md-10">
                        <span className="ra-editEmail-body-title" tabIndex={0}>
                            {RMResx.RM_CP_EamilTemplate_UseDefaultFooter}
                        </span>
                    </div>
                </div>
                <R.Switch
                    checked={this.state.checked}
                    onChange={this.onChange}>
                </R.Switch>
            </>
        );
    }

    renderTemplateName(){
        if(!this.state.isCustomTemplate){
            return <span className="ra-editEmail-body-content" tabIndex={0}>
                {this.state.name}
            </span>;
        }
        return <R.Validation element="Input" require={RMResx.RM_JS_CP_EamilTemplate_NameEmptyError}>
            <R.Input
                id="raCpEmailTemplateNameIpt"
                name='iptScName'
                type='text'
                value={this.state.name}
                onChange={this.onChangeTemplateName.bind(this)}
            />
            <$g.ValidationMsg show={this.state.showTemplateTooLongMsg}>
                {RMResx.RM_JS_Common_Msg_CannotExceed255}
            </$g.ValidationMsg>
        </R.Validation>;
    }

    render() {
        return (
            <div id="rmEditEmailTemplate">
                <$g.SiteMap
                    data={[
                        SiteMapLinks.CP,
                        SiteMapLinks.CP_EmailTemplate,
                        this.isEditEmailTemplate ? SiteMapLinks.CP_EditEmailTemplate : SiteMapLinks.CP_CreateEmailTemplate,
                    ]}
                />
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                />
                <R.Validation>
                    <div id="rmEditEmailTemplateBody">
                        {this.renderParameters()}
                        <div className="wrapper">
                            <div className="row a">
                                <div className="col-md-10">
                                    <span className="ra-editEmail-body-title require" tabIndex={0}>
                                        <$g.I18NProvider
                                            msg={StringUtil.trimEndColon(
                                                RMResx.RM_JS_CP_EamilTemplate_EmailTemplateName
                                            )}
                                        />
                                    </span>
                                </div>
                            </div>
                            <div className="row">
                                <div className="col-md-10">
                                    {this.renderTemplateName()}
                                </div>
                            </div>
                            <div className="row a">
                                <div className="col-md-10">
                                    <span className="ra-editEmail-body-title" tabIndex={0}>
                                        <$g.I18NProvider
                                            msg={StringUtil.trimEndColon(
                                                RMResx.RM_JS_CP_EamilTemplate_EmailSubject
                                            )}
                                        />
                                    </span>
                                </div>
                            </div>
                            <div className="row">
                                <div className="col-md-10">
                                    <input
                                        type="text"
                                        className="ra-editEmail-input-emailSubject a"
                                        tabIndex="0"
                                        id="subjectInput"
                                        value={this.state.subject || ""}
                                        onChange={this.onSubjectChange}
                                        onBlur={this.onBlur}
                                    />
                                    <$g.ValidationMsg show={this.state.showSubjectTooLongMsg}>
                                        {RMResx.RM_JS_Common_Msg_CannotExceed255}
                                    </$g.ValidationMsg>
                                </div>
                            </div>
                            <div className="row a">
                                <div className="col-md-10">
                                    <span className="ra-editEmail-body-title" tabIndex={0}>
                                        <$g.I18NProvider
                                            msg={StringUtil.trimEndColon(
                                                RMResx.RM_JS_CP_EamilTemplate_CC
                                            )}
                                        />
                                    </span>
                                </div>
                            </div>
                            <div className="row">
                                <div className="col-md-10">
                                    <input
                                        type="text"
                                        id="ccInput"
                                        className="ra-editEmail-input-cc a"
                                        tabIndex="0"
                                        value={this.state.cc || ""}
                                        onChange={this.onCCChange}
                                        onBlur={this.onBlur}
                                    />
                                    <$g.ValidationMsg show={this.state.showCcTooLongMsg}>
                                        {RMResx.RM_JS_Common_Msg_CannotExceed255}
                                    </$g.ValidationMsg>
                                </div>
                            </div>
                            <div className="row a">
                                <div className="col-md-10">
                                    <span className="ra-editEmail-body-title require" tabIndex={0}>
                                        <$g.I18NProvider
                                            msg={StringUtil.trimEndColon(
                                                RMResx.RM_JS_CP_EamilTemplate_Body
                                            )}
                                        />
                                    </span>
                                </div>
                            </div>
                            <div className="row">
                                <div className="col-md-10">
                                    <R.Validation element="Editor" require={RMResx.RM_CP_EamilTemplate_EmptyBody}>
                                        <R.Editor
                                            ref={r => this.editorRef = r}
                                            args_upload={{
                                                doUpload: this.doUpload,
                                                fileTypes: {
                                                    image: ["png", "jpg", "gif", "bmp"],
                                                }
                                            }}
                                            height={500}
                                            toolbar={[
                                                { name: 'font', items: ['FontFamily', 'FontSize', 'Format'] },
                                                { name: 'styles', items: ['Bold', 'Italic', 'Underline', 'StrikeThrough'] },
                                                { name: 'typography', items: ['Superscript', 'Subscript'] },
                                                { name: 'alignment', items: ['AlignLeft', 'AlignCenter', 'AlignRight', 'AlignJustify'] },
                                                { name: 'indent', items: ['Outdent', 'Indent'] },
                                                { name: 'list', items: ['BulletedList', 'NumberedList'] },
                                                { name: 'colors', items: ['FontColor', 'BgColor'] },
                                                { name: 'link', items: ['Link', 'Unlink'] },
                                                { name: 'insert', items: ['Table', 'Image'] },
                                                { name: 'document', items: ['SourceCode'] },
                                            ]}
                                            value={this.state.body}
                                            plugins={['Image']}
                                            wordcount={5000}
                                            placeholder={RMResx.RM_CP_EamilTemplate_EditorPlaceholder}
                                            doCopyUpload={this.doCopyUpload}
                                            nativeSpellChecker={false}
                                            onFocus={this.onEmailBodyEditorClick}
                                            maxSize="5GB"
                                        />
                                    </R.Validation>
                                </div>
                            </div>
                            {this.renderCustomFooter()}
                            <div id="ra_control_btn">
                                <div className="ra_control_btn_cancel">
                                    <R.Button
                                        id="raCpEmailTemplateCancelBtn"
                                        text={RMResx.RM_JS_Common_Cancel}
                                        onClick={this.cancelClick}
                                    />
                                </div>
                                {isMultiGeoMainDC && <div className="ra_control_btn_save">
                                    <R.Button
                                        id="raCpEmailTemplateSaveBtn"
                                        primary={true}
                                        classify="theme"
                                        text={RMResx.RM_JS_Common_Save}
                                        onClick={this.saveClick}
                                    />
                                </div>}
                            </div>
                        </div>
                    </div>
                </R.Validation>
            </div>
        );
    }
}
