import PeoplePicker from "../../../Common/PeoplePicker";

export default class PhyLoanRequest extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.bind(['onMultipleTextChange', 'onDateTimeChange', 'onPeopleSelectionChanged']);
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.state = {
            isSaving: false,
            formData: this.props.data,
            returnDateError: false,
            returnDateErrorMsg: ""
        };
    }
    componentInit() {
        let url = `/api/PhysicalRecordApi/CurrentUser`;
        let option = {
            url: url,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            this.state.formData.OnBehalf = [{
                UserId: result.UserId,
                DisplayName: result.DisplayName,
                UserPrincipalName: result.UserPrincipalName,
                Checked: true
            }];
            this.setState({
                formData: RM.deepcopy(this.state.formData)
            });
        }).catch((e) => {

        });
    }
    componentReceive(type, args) {
        switch (type) {
            // case "init":
            //     this.initData(args);
            //     break;
            case "onSave":
                this.setState({
                    isSaving: true
                });
                
                if(this.state.formData.ReturnDate != null){
                    this.state.formData.ReturnDate.DateTimeStr = RM.TimeUtil.getCommonDateStr(new Date(this.state.formData.ReturnDate.DateTimeObj));
                }
                args(this.state.formData, (response, callback) => {
                    if (Number(response.FailedType) === 2) {
                        if (callback) {
                            callback();
                        }
                        this.onLoanConfirming(response.ErrorMsg);
                    } else {
                        this.setState({
                            returnDateError: true,
                            returnDateErrorMsg: response.ErrorMsg
                        });
                    }
                });
                break;
        }
    }

    // initData(args) {
    //     this.state.formData = args;
    // }

    //Events:
    onMultipleTextChange(value) {
        //console.log(args);
        this.state.formData.Comment = value;
        this.setState({
            formData: RM.deepcopy(this.state.formData)
        });
    }

    onDateTimeChange(args) {
        //console.log(args);
        var date = args.newValue;
        var zone = RM.TimeUtil.getGlobalTimezoneInfo();
        this.state.formData.ReturnDate = {
            DateTimeObj: date,
            TimeZoneId: zone.id,
            AutoAdjustClock: zone.autoAdjustClock
        };
        this.setState({
            formData: RM.deepcopy(this.state.formData),
            // returnDateError: false, 
            // returnDateErrorMsg: ""
        });
    }

    onPeopleSelectionChanged(args) {
        //console.log(args);
        this.state.formData.OnBehalf = args;
        this.setState({
            formData: RM.deepcopy(this.state.formData)
        });
    }

    onLoanConfirming = (errorMsg) => {
        $$.messagedialog(true, {
            width: "550px",
            title: RMResx.RM_LR_LoanRequest_Confirm,
            content: errorMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                }
            ]
        });
    }

    render() {
        // let selDate = new Date();
        return (
            <div id={this.props.id} className="template_item">
                <$g.FormRow
                    label={RMResx.RM_PRM_PRE_NewRequest_OnBehalfOf.slice(0, RMResx.RM_PRM_PRE_NewRequest_OnBehalfOf.length -1)}
                    require={true}
                >
                    {this.state.formData.OnBehalf != null &&
                        <PeoplePicker
                            id="raPhyLoanRequstLoanByUsers"
                            items={this.state.formData.OnBehalf}
                            singleMode
                            selectionChanged={this.onPeopleSelectionChanged}
                        />}
                    <$g.ValidationMsg show={this.state.isSaving && (this.state.formData.OnBehalf == null || this.state.formData.OnBehalf.length == 0)}>
                        {RMResx.RM_JS_CP_AM_AddUser_Nomatch}
                    </$g.ValidationMsg>
                </$g.FormRow>

                <$g.FormRow
                    label={RMResx.RM_PRM_PRE_NewRequest_ReturnDate.slice(0, RMResx.RM_PRM_PRE_NewRequest_ReturnDate.length -1)}
                    require={false}
                >
                    <R.Datepicker
                        id="raPhyLoanRequstReturnDate"
                        selectedDate={null}
                        enableDates={{ start: new Date(), end: new Date(9999, 12, 31) }}
                        data-part="vtWidget"
                        width="300"
                        dateTimeFormat={this.defaultDateFormat}
                        hasTimePicker={true}
                        onChange={this.onDateTimeChange}
                        triggerBySource={true}
                        todayClick={this.todayClick}
                    />
                    {
                        <$g.ValidationMsg show={this.state.returnDateError}>
                            {this.state.returnDateErrorMsg}
                        </$g.ValidationMsg>}
                </$g.FormRow>

                <$g.FormRow
                    label={RMResx.RM_PRM_PRE_NewRequest_Comment}
                    require={false}
                >
                    <R.Input
                        type="textarea"
                        width="300"
                        onChange={this.onMultipleTextChange}
                        aria={{ariaLabel:RMResx.RM_PRM_PRE_NewRequest_Comment}}
                    />
                </$g.FormRow>
            </div>
        );
    }
}
