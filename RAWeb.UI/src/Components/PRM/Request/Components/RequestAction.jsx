import PeoplePicker from "../../../Common/PeoplePicker";
import { RequestTypeMode, ActionTypeMode } from "../../Constants";

export default class RequestAction extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.bind(['onMultipleTextChange', 'onDateTimeChange', 'onPeopleSelectionChanged', "showMessageTip", "hideMessageTip"]);
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.timeZoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        this.items = this.props.data.Items;
        this.state = {
            showMessageTip: this.showMessageTip,
            isSaving: false,
            actionType: this.props.data.ActionType,
            requestTypeMode: this.props.data.RequestTypeMode,
            comment: this.props.data.Comment,

            selDate: null,
            timeZoneId: null,
            autoAdjustClock: null,
            users: null,
            returnDateError: false,
            returnDateErrorMsg: "",
            isSendEmailToDestinationRM: false
            // formData : {
            //     ActionType:this.props.data.ActionType,
            // }

        };
        if (this.state.requestTypeMode == RequestTypeMode.Loan) {
            this.state.selDate = this.props.data.ReturnDate.DateTimeObj;
            this.state.timeZoneId = this.props.data.ReturnDate.TimeZoneId;
            this.state.autoAdjustClock = this.props.data.ReturnDate.AutoAdjustClock;
            this.state.users = this.props.data.OnBehalf;
        }
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.setState({ isSaving: true });
                args({
                    Items: this.items,
                    Comment: this.state.comment,
                    RequestTypeMode: this.state.requestTypeMode,
                    ReturnDate: {
                        DateTimeObj: this.state.selDate,
                        TimeZoneId: this.state.timeZoneId,
                        AutoAdjustClock: this.state.autoAdjustClock,
                    },
                    OnBehalf: this.state.users,
                    MoveDto: {
                        IsSendEmailToDestinationRM: !!this.state.isSendEmailToDestinationRM
                    }
                }, (erorMsg) => {
                    this.setState({
                        returnDateError: true,
                        returnDateErrorMsg: erorMsg
                    });
                });
                break;
            case "showMsgTip":
                this.showMessageTip(args.type, args.msg);
        }
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

    // initData(args) {
    //     this.setState({
    //         isSaving: false,
    //         showTip: false,
    //         users: null
    //     });
    //     this.formData = args;
    //     //this.timeZoneInfo = RM.TimeUtil.getTimezoneInfo(args.ReturnDate.TimeZoneId, args.ReturnDate.AutoAdjustClock);
    //     if (args.RequestTypeMode == RequestTypeMode.Loan) {
    //         this.setState({
    //             actionType: args.ActionType,
    //             requestTypeMode: args.RequestTypeMode,
    //             comment: args.Comment,
    //             selDate: args.ReturnDate.DateTimeObj,
    //             users: args.OnBehalf
    //         });
    //     } else {
    //         this.setState({
    //             actionType: args.ActionType,
    //             requestTypeMode: args.RequestTypeMode,
    //             comment: args.Comment
    //         });
    //     }
    // }

    //Events:
    onMultipleTextChange(value) {
        //console.log(args);
        // this.formData.Comment = args.value;
        this.setState({
            comment: value
        });
    }
    onDateTimeChange(args) {
        //console.log(args);
        var date = args.newValue;
        var zone = RM.TimeUtil.getGlobalTimezoneInfo();
        // this.formData.ReturnDate = {
        //     DateTimeObj: date,
        //     TimeZoneId: zone.id,
        //     AutoAdjustClock: zone.autoAdjustClock
        // };

        this.setState({
            selDate: date,
            timeZoneId: zone.id,
            autoAdjustClock: zone.autoAdjustClock,
            // returnDateError: false, 
            // returnDateErrorMsg: ""
        });
    }

    onPeopleSelectionChanged(args) {
        //console.log(args);
        // this.formData.OnBehalf = args;
        this.setState({
            users: args
        });
    }

    render() {
        return (
            <div id={this.props.id} className="template_item">
                <R.Messagebar
                    message={this.state.tipMsg} classify={this.state.tipType}
                    onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
                {this.state.actionType == ActionTypeMode.Approval && this.state.requestTypeMode == RequestTypeMode.Loan && this.items.length == 1 &&
                    <React.Fragment>
                        <$g.FormRow
                            label={RMResx.RM_PRM_PRE_NewRequest_OnBehalfOf.slice(0, RMResx.RM_PRM_PRE_NewRequest_OnBehalfOf.length -1)}
                            require={true}
                        >
                            <PeoplePicker
                                id="raPrmRequestActionLoanByUsers"
                                singleMode
                                items={this.state.users}
                                selectionChanged={this.onPeopleSelectionChanged}
                            />
                            <$g.ValidationMsg show={this.state.isSaving && (this.state.users == null || this.state.users.length == 0)}>
                                {RMResx.RM_JS_CP_AM_Edit_Msg_Required}
                            </$g.ValidationMsg>
                        </$g.FormRow>
                        <$g.FormRow
                            label={RMResx.RM_PRM_PRE_NewRequest_ReturnDate.slice(0, RMResx.RM_PRM_PRE_NewRequest_ReturnDate.length -1)}
                            require={false}
                        >
                            <R.Datepicker
                                id="raPrmRequestReturnDate"
                                selectedDate={this.state.selDate}
                                enableDates={{ start: new Date(), end: new Date(9999, 12, 31) }}
                                data-part="vtWidget"
                                width="300"
                                dateTimeFormat={this.defaultDateFormat}
                                // hasTimeZone={true}
                                hasTimePicker={true}
                                // selectedTimeZone={this.timeZoneInfo}
                                onChange={this.onDateTimeChange}
                                triggerBySource={true}
                                todayClick={this.todayClick}
                            />

                            <$g.ValidationMsg show={this.state.returnDateError}>
                                {this.state.returnDateErrorMsg}
                            </$g.ValidationMsg>
                        </$g.FormRow>
                    </React.Fragment>}


                <$g.FormRow
                    label={RMResx.RM_PRM_PRE_NewRequest_Comment}
                    require={false}
                >
                    <R.Input
                        type="textarea"
                        width="300"
                        value={this.state.comment || ""}
                        onChange={this.onMultipleTextChange}
                        aria={{ariaLabel:RMResx.RM_PRM_PRE_NewRequest_Comment}}
                    />
                </$g.FormRow>

                {this.state.requestTypeMode === RequestTypeMode.Movement
                    && this.state.actionType === ActionTypeMode.Approval && (
                    <$g.FormRow require={false}>
                        <R.Checkbox
                            id="chkSendEmailAction"
                            text={RMResx.RM_PRM_RequestManagement_SendEmail}
                            checked={this.state.isSendEmailToDestinationRM}
                            onChange={(e, checked) => this.setState({ isSendEmailToDestinationRM: typeof e === "boolean" ? e : checked })}
                        />
                    </$g.FormRow>
                )}
            </div>
        );
    }
}
