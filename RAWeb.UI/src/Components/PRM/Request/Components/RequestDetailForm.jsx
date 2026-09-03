import PeoplePicker from "../../../Common/PeoplePicker";
import { RequestStatus, PhysicalRequestType, RequestTypeMode, PhyNodeTypeNames, RequestStatusMode } from "../../Constants";
import StringUtil from "../../../../Utilities/StringUtil";

export default class RequestDetailForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            requestData: null,
            returnDate: null,
            timeZoneId: null,
            autoAdjustClock: false,
            showMessageTip: this.showMessageTip,
            returnDateDisplay: "",
            HoldBy: null,
            RMComment: null,
            fileDetailParam: {},
            isSendEmailToDestinationRM: false
        };
        this.fileDetailFormId = "fileDetailFormId";
        this.fileEditFormId = "fileEditFormId";
        this.formData = {
            Requests: [this.requestItem]
        };
        this.requestItem = {
            Id: null,
            HoldUserId: null,
            DisposalClass: {
                HoldCategory: 1,
                EndTime: null,
                ReviewComment: null
            },
            PhysicalFileInfo: {
                Template: null,
            }
        };
        let eventArr = ["onFileDetail", "convertOjbStr", "onRMCommentChange", "onFileDetail", "onReturnDateChange", "onHoldBySelectionChanged", "hideMessageTip"];
        this.editField = ["EndTime", "ReviewComment"];
        this.linkField = ["RecordId"];
        this.bind(eventArr);
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.timeZoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        this.isAdmin = RM.gData.isPhysicalAdmin;
        this.editEnable = false;
        this.initData(this.props.data);
        //bindEvents(this, "renderContent", "onSavePanel", "onClosePanel");
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.updateFormData();
                args(this.formData);
                break;
            case "showMsgTip":
                this.showMessageTip(args.type, args.msg);
                //this.showMsgToast(args.msg,args.type);
                break;
        }
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
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip() {
        this.setState({
            showTip: false
        });
    }

    updateFormData() {
        if (this.state.requestData.Type == RequestTypeMode.Creation) {
            if(this.requestItem.PhysicalFileInfo){
                this.requestItem.PhysicalFileInfo.Template = null;
            }else{
                this.requestItem.PhysicalFileInfos[0].Template = null;
            }
        }
        
        if (this.requestItem) {
            if (!this.requestItem.MoveDto) {
                this.requestItem.MoveDto = {};
            }
            this.requestItem.MoveDto.IsSendEmailToDestinationRM = this.state.isSendEmailToDestinationRM;
        }
        
        this.formData.Requests[0] = this.requestItem;
        this.formData.IsSendEmailToDestinationRM = this.state.isSendEmailToDestinationRM;
    }

    convertOjbStr(source, status) {
        let state = source.filter(item => { return item.id == status; });
        return state.length > 0 ? state[0].name : "";
    }

    initData(args) {
        this.requestType = args.Type;
        //only edit waiting for approval file
        this.editEnable = this.isAdmin && args.Status == 0;
        // this.setState({
        //     HoldBy: null
        // });
        $$.loading(true);
        let url = `/api/PhysicalRequestApi/GetRequest?id=${args.Id}`;
        let option = {
            url: url,
            method: "Get",
        };
        fetchUtility(option).then(requestObj => {
            let disposalClass = requestObj.DisposalClass;
            $$.loading(false);
            this.initFormData(requestObj);
            let user = (requestObj.HoldUserId == null && requestObj.HoldUserDisplay == null) ?
                [] : [{
                    UserId: requestObj.HoldUserId,
                    DisplayName: requestObj.HoldUserDisplay,
                    UserPrincipalName: "",
                    Checked: true
                }];
            let reDate =  null;
            let reDateDisplay = "";
            if (disposalClass.EndTime != 0) {
                reDate = !disposalClass.EndTimeStr ?
                    null : new Date(disposalClass.EndTimeStr);
                reDateDisplay = !disposalClass.EndTimeStr ?
                    "" : RM.TimeUtil.dateToString(new Date(disposalClass.EndTimeStr), null, true);
            }
            this.setState({
                requestData: requestObj,
                returnDate: reDate,
                timeZoneId: requestObj.DisposalClass.TimeZoneId,
                autoAdjustClock: requestObj.DisposalClass.IsDaylightSavingTime,
                returnDateDisplay: reDateDisplay,
                HoldBy: user,
                RMComment: requestObj.DisposalClass.ReviewComment,
                isSendEmailToDestinationRM: requestObj.MoveDto ? !!requestObj.MoveDto.IsSendEmailToDestinationRM : false
            });
            if (requestObj.Status == RequestStatusMode.WaitingForAproval && requestObj.PhysicalFileInfo && requestObj.PhysicalFileInfo.HoldBy) {
                this.showMessageTip('info',
                    <$g.I18NProvider msg={RMResx.RM_RC_Request_Msg_ApproveLoanedObject}>
                        {requestObj.PhysicalFileInfo.HoldBy}
                    </$g.I18NProvider>
                );

            }
        });

    }

    processDetailData(requestObj) {
        $$.loading(false);
        this.initFormData(requestObj);
        
        let disposalClass = requestObj.DisposalClass;
        let user = (requestObj.HoldUserId == null && requestObj.HoldUserDisplay == null) ?
            [] : [{
                UserId: requestObj.HoldUserId,
                DisplayName: requestObj.HoldUserDisplay,
                UserPrincipalName: "",
                Checked: true
            }];
            
        let reDate =  null;
        let reDateDisplay = "";
        if (disposalClass && disposalClass.EndTime != 0) {
            reDate = !disposalClass.EndTimeStr ? null : new Date(disposalClass.EndTimeStr);
            reDateDisplay = !disposalClass.EndTimeStr ? "" : RM.TimeUtil.dateToString(new Date(disposalClass.EndTimeStr), null, true);
        }

        this.setState({
            requestData: requestObj,
            returnDate: reDate,
            timeZoneId: disposalClass ? disposalClass.TimeZoneId : null,
            autoAdjustClock: disposalClass ? disposalClass.IsDaylightSavingTime : false,
            returnDateDisplay: reDateDisplay,
            HoldBy: user,
            RMComment: disposalClass ? disposalClass.ReviewComment : null,
            isSendEmailToDestinationRM: requestObj.MoveDto ? !!requestObj.MoveDto.IsSendEmailToDestinationRM : false
        });

        if (requestObj.Status == RequestStatusMode.WaitingForAproval && requestObj.PhysicalFileInfo && requestObj.PhysicalFileInfo.HoldBy) {
            this.showMessageTip('info',
                <$g.I18NProvider msg={RMResx.RM_RC_Request_Msg_ApproveLoanedObject}>
                    {requestObj.PhysicalFileInfo.HoldBy}
                </$g.I18NProvider>
            );
        }
    }

    initFormData(data) {
        this.requestItem = data;
    }

    // action: 1, loan file detail, 2, creation file detail
    onFileDetail(data, action) {
        if (action == 2 && this.props.onShowFileDetail) {
            this.props.onShowFileDetail(this.state.requestData);
        }
    }

    onReturnDateChange(args) {
        var date = args.newValue;
        var zone = RM.TimeUtil.getGlobalTimezoneInfo();
        if (date) {
            // this.requestItem.DisposalClass.EndTime = date.getTime() * 10000 + 621355968000000000;
            this.requestItem.DisposalClass.EndTimeStr = RM.TimeUtil.getCommonDateStr(date);
            this.requestItem.DisposalClass.TimeZoneId = zone.id;
            this.requestItem.DisposalClass.IsDaylightSavingTime = zone.autoAdjustClock;
        }
    }
    onRMCommentChange(value) {
        this.requestItem.DisposalClass.ReviewComment = value;
        this.setState({
            RMComment: value
        });
    }
    onHoldBySelectionChanged(args) {
        if (args.length > 0) {
            this.requestItem.HoldUserId = args[0].UserId;
            this.requestItem.HoldUserDisplay = args[0].DisplayName;
        } else {
            this.requestItem.HoldUserId = null;
            this.requestItem.HoldUserDisplay = null;
        }

        this.setState({
            HoldBy: args
        });
    }

    getNodeTypeI18N(nodeType) {
        return PhyNodeTypeNames[nodeType];
    }

    renderLoanInfo() {
        let requestInfo = this.state.requestData;

        // if (this.state.timeZoneId != null) {
        //     this.timeZoneInfo = RM.TimeUtil.getTimezoneInfo(this.state.timeZoneId, this.state.autoAdjustClock);
        //     this.timeZoneInfo.autoAdjustClock = this.state.autoAdjustClock;
        // }

        if (requestInfo.Type == RequestTypeMode.Creation || requestInfo.Type == RequestTypeMode.Movement) {
            return null;
        } else if (this.editEnable) {
            return <React.Fragment>
                <$g.DetailRow>
                    <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_NewRequest_OnBehalfOf)} require={true}>
                        <PeoplePicker
                            singleMode
                            id="raPrmRequestDetailLoanByUsers"
                            items={this.state.HoldBy}
                            selectionChanged={this.onHoldBySelectionChanged}
                        />
                        <$g.ValidationMsg show={(this.state.HoldBy == null || this.state.HoldBy.length == 0)}>
                            {RMResx.RM_JS_CP_AM_Edit_Msg_Required}
                        </$g.ValidationMsg>
                    </$g.DetailCell>
                </$g.DetailRow>
                <$g.DetailRow>
                    <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_NewRequest_ReturnDate)}>
                        <div>
                            <R.Datepicker
                                id="raPrmRequestDetailReturnDate"
                                selectedDate={this.state.returnDate}
                                data-part="vtWidget"
                                width="300"
                                dateTimeFormat={this.defaultDateFormat}
                                // hasTimeZone={true}
                                hasTimePicker={true}
                                // selectedTimeZone={this.timeZoneInfo}
                                onChange={this.onReturnDateChange}
                                triggerBySource={true}
                            /></div>
                    </$g.DetailCell>
                </$g.DetailRow>
            </React.Fragment>;
        } else {
            return <React.Fragment>
                <$g.DetailRow>
                    <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_NewRequest_ReturnDate)} value={this.state.returnDateDisplay} />
                </$g.DetailRow>
                <$g.DetailRow>
                    <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_NewRequest_OnBehalfOf)} value={requestInfo.HoldUserDisplay} />
                </$g.DetailRow>
            </React.Fragment>;
        }
    }

    render() {
        let requestInfo = this.state.requestData;
        if(requestInfo){
            if(requestInfo.Titles && requestInfo.Titles.length > 0){
                let title = requestInfo.Titles.join(', ');
                requestInfo.Title = title;
            }
            if(requestInfo.RecordIds && requestInfo.RecordIds.length > 0){
                let recordId = requestInfo.RecordIds.join(', ');
                requestInfo.RecordId = recordId;
            }
        }

        const isMovement = requestInfo && requestInfo.Type == RequestTypeMode.Movement;

        return <div id={this.props.id}>
            <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
            {requestInfo && <div id="raRequestForm">
                <$g.DetailList labelWidth={150}>
                    <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_MyRequest_RequestType } value={this.convertOjbStr(PhysicalRequestType, requestInfo.Type)} /></$g.DetailRow>
                    <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_MyRequest_RequestId } value={requestInfo.RequestId} /></$g.DetailRow>
                    <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_MyRequest_ItemName} value={requestInfo.Title} /></$g.DetailRow>
                    {requestInfo.PhysicalFileInfo &&
                        <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_PRE_Column_Type } value={this.getNodeTypeI18N(requestInfo.PhysicalFileInfo.NodeType)} /></$g.DetailRow>}
                    <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_RequestManagement_UniqueId} value={requestInfo.RecordId} clickFun={this.onFileDetail.bind(this, requestInfo, 1)} /></$g.DetailRow>
                    <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_RequestManagement_Status} value={this.convertOjbStr(RequestStatus, requestInfo.Status)} /></$g.DetailRow>
                    <$g.DetailRow><$g.DetailCell label={RMResx.RM_PRM_MyRequest_RequestBy} value={requestInfo.CreatedUserDisplay} /></$g.DetailRow>

                    {isMovement && (
                        <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_PRM_MyRequest_DestinationLocation}
                                value={requestInfo.MoveDto?.DestinationPath}
                            />
                        </$g.DetailRow>
                    )}

                    {this.renderLoanInfo()}

                    <$g.DetailRow><$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_MA_Comment)} value={requestInfo.Comment} /></$g.DetailRow>
                
                    {isMovement && !this.editEnable && (
                         <$g.DetailRow>
                            <$g.DetailCell 
                                label={RMResx.RM_PRM_RequestManagement_EmailStatus}
                                value={requestInfo.MoveDto ? (requestInfo.MoveDto.IsSendEmailToDestinationRM ? 'Yes' : 'No') : ''}
                            />
                        </$g.DetailRow>
                    )}
                </$g.DetailList>

                <div className="ra-requestd-showDetailBtn">
                    {this.state.requestData.Type == RequestTypeMode.Creation &&
                        <a className="ra-link-a request-item-details" tabIndex={0} onClick={this.onFileDetail.bind(this, requestInfo, 2)}>
                            {RMResx.RM_PRM_RequestManagement_CreationFileDetail}
                        </a>
                    }
                </div>
                <div className="ra-requestd-line"></div>
                <div>
                    <div className='ra-requestd-title' tabIndex="0">{RMResx.RM_PRM_RequestManagement_RMComment.slice(0, RMResx.RM_PRM_RequestManagement_RMComment.length -1)}</div>
                    {
                        this.editEnable ?
                            <R.Input
                                type="textarea"
                                width={500}
                                height={115}
                                value={this.state.RMComment || ""}
                                onChange={this.onRMCommentChange}
                                aria={{ariaLabel:RMResx.RM_PRM_RequestManagement_RMComment}}
                            />
                            : <div className="ra-requestd-value">{this.state.RMComment}</div>
                    }
                    {
                        this.editEnable && isMovement && (
                            <div className="margin-top-s">
                                <R.Checkbox 
                                    id="chkSendEmailDetail" 
                                    text={RMResx.RM_PRM_RequestManagement_SendEmail}
                                    checked={this.state.isSendEmailToDestinationRM} 
                                    onChange={(e, checked) => this.setState({ isSendEmailToDestinationRM: typeof e === "boolean" ? e : checked })} 
                                />
                            </div>
                        )
                    }
                </div>

            </div>}
        </div>;

    }
}