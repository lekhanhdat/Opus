import { bindEvents } from "../../../../Utilities/CommonUtil";
import { ExpiryEmailForm } from "./ExpiryEmailForm";
import PeoplePicker from "../../../Common/PeoplePicker";

export default class PhyHoldForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.expiryEmailFormRef = React.createRef();
        bindEvents(this, "showMessageTip", "hideMessageTip", "onHoldRadioChange", "onHoldTypeChange", "renderExtendForm");
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.state = {
            isExtendForm: false,
            isEdit: false,
            showTip: false,
            tipType: "success",
            tipMsg: "",
            isSaving: false,
            isSavingHoldNumber: false,
            isSavingHoldName: false,
            calenderTimeInvalid: false,
            holdNameIsExist: false,
            data: [],
            holdTypeItems: [],
            holdUnitItems: [],
            holdType: 1,   //0 for custom, 1 for calendar
            holdProfile: {
                Id: null,
                Name: "",
                Number: "",
                Unit: 0,
                Description: "",
                CalenderTime: null,
                TimeZoneId: null,
                IsDayLightSaving: false,
                ProfileType: props.data.profileType,
                HoldManagers: "",
                HoldUserManagers: [],
                IsHoldManagerEmailNotificationEnabled: false
            },
            extendSetting: {Number: "", Unit: 0},
        };
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                if (this.state.isExtendForm) {
                    this.saveExtendData(args);
                } else {
                    this.saveFormData(args);
                }
                break;
        }
    }

    componentInit() {
        this.initData(RM.deepcopy(this.props.data));
    }

    getCurrentUserForHoldManager() {
        const gData = RM && RM.gData ? RM.gData : {};
        const userId = gData.userId || "";
        const userName = gData.userName || "";
        const userPrincipalName = gData.emailAddress || "";
        const displayName = userName || userPrincipalName;

        if (!userId && !displayName) {
            return null;
        }

        return {
            UserId: userId,
            UserName: userName || null,
            UserPrincipalName: userPrincipalName || null,
            Email: userPrincipalName || null,
            DisplayName: displayName,
            InviteType: 0
        };
    }

    normalizeHoldProfile(holdItem) {
        const profile = holdItem ? RM.deepcopy(holdItem) : {};
        if (typeof profile.IsHoldManagerEmailNotificationEnabled !== "boolean") {
            profile.IsHoldManagerEmailNotificationEnabled = !!profile.NotifyHoldManager;
        }
        if (!Array.isArray(profile.HoldUserManagers)) {
            profile.HoldUserManagers = [];
        }
        return profile;
    }


    initData(args) {
        this.formData = args;
        switch (args.formType) {
            case "new": {
                const currentUser = this.getCurrentUserForHoldManager();
                const HoldUserManagers = currentUser ? [currentUser] : [];
                const holdProfile = {
                    ...this.state.holdProfile,
                    HoldUserManagers,
                    HoldManagers: currentUser ? currentUser.DisplayName : ""
                };
                this.setState({ holdFormType: "new", isEdit: false, holdProfile });
                this.intHoldMeta();
                break;
            }
            case "edit": {
                const holdProfile = this.normalizeHoldProfile(args.holdItem);
                this.setState({ holdProfile, holdType: holdProfile.Type, holdFormType: "new", isEdit: true });
                this.intEidtHoldMeta(holdProfile.Type, holdProfile.Unit);
                break;
            }
            case "extend": {
                const holdProfile = this.normalizeHoldProfile(args.holdItem);
                this.setState({ holdProfile, extendSetting: { Number: "", Unit: 0 }, isExtendForm: true, isEdit: false });
                this.intHoldMeta();
                break;
            }
            default:
                return;
        }
    }
    intHoldMeta() {
        this.setState({ holdTypeItems: this.initHoldTypeCombo(), holdUnitItems: this.initHoldUnitCombo() });
    }

    intEidtHoldMeta(type, Unit) {
        this.setState({ holdTypeItems: this.initEditHoldTypeCombo(type), holdUnitItems: this.initEditHoldUnitCombo(Unit) });
    }


    initHoldTypeCombo() {
        return [{
            value: 0,
            title: RMResx.RM_JS_RDM_Hold_Duration,
            checked: false,
        }, {
            value: 1,
            title: RMResx.RM_JS_RDM_Hold_Canlender ,
            checked: true,
        }];
    }
    initHoldUnitCombo() {
        return [{
            value: 0,
            title: RMResx.RM_JS_ScheduleSetting_Days,
            checked: true,
        }, {
            value: 1,
            title: RMResx.RM_JS_ScheduleSetting_Weeks,
            checked: false,
        }, {
            value: 2,
            title: RMResx.RM_JS_RDM_Explorer_Months,
            checked: false,
        }, {
            value: 3,
            title: RMResx.RM_JS_RDM_Explorer_Years,
            checked: false,
        }];
    }
    initEditHoldTypeCombo(type) {
        if (type == 0) {
            return [{
                value: 0,
                title: RMResx.RM_JS_RDM_Hold_Duration,
                checked: true,
            }, {
                value: 1,
                title: RMResx.RM_JS_RDM_Hold_Canlender,
                checked: false,
            }];
        } else {
            return [{
                value: 0,
                title: RMResx.RM_JS_RDM_Hold_Duration,
                checked: false,
            }, {
                value: 1,
                title: RMResx.RM_JS_RDM_Hold_Canlender,
                checked: true,
            }];
        }
    }
    initEditHoldUnitCombo(Unit) {
        if (Unit == 0) {
            return [{
                value: 0,
                title: RMResx.RM_JS_ScheduleSetting_Days,
                checked: true,
            }, {
                value: 1,
                title: RMResx.RM_JS_ScheduleSetting_Weeks,
                checked: false,
            }, {
                value: 2,
                title: RMResx.RM_JS_RDM_Explorer_Months,
                checked: false,
            }, {
                value: 3,
                title: RMResx.RM_JS_RDM_Explorer_Years,
                checked: false,
            }];
        } else if (Unit == 1) {
            return [{
                value: 0,
                title: RMResx.RM_JS_ScheduleSetting_Days,
                checked: false,
            }, {
                value: 1,
                title: RMResx.RM_JS_ScheduleSetting_Weeks,
                checked: true,
            }, {
                value: 2,
                title: RMResx.RM_JS_RDM_Explorer_Months,
                checked: false,
            }, {
                value: 3,
                title: RMResx.RM_JS_RDM_Explorer_Years,
                checked: false,
            }];
        } else if (Unit == 2) {
            return [{
                value: 0,
                title: RMResx.RM_JS_ScheduleSetting_Days,
                checked: false,
            }, {
                value: 1,
                title: RMResx.RM_JS_ScheduleSetting_Weeks,
                checked: false,
            }, {
                value: 2,
                title: RMResx.RM_JS_RDM_Explorer_Months,
                checked: true,
            }, {
                value: 3,
                title: RMResx.RM_JS_RDM_Explorer_Years,
                checked: false,
            }];
        } else {
            return [{
                value: 0,
                title: RMResx.RM_JS_ScheduleSetting_Days,
                checked: false,
            }, {
                value: 1,
                title: RMResx.RM_JS_ScheduleSetting_Weeks,
                checked: false,
            }, {
                value: 2,
                title: RMResx.RM_JS_RDM_Explorer_Months,
                checked: false,
            }, {
                value: 3,
                title: RMResx.RM_JS_RDM_Explorer_Years,
                checked: true,
            }];
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

    isAvailableNumber(inputText) {
        if (inputText == "" || inputText * 1 == 0) {
            return false;
        }
        let patt = new RegExp("^[0-9]*$", "g");
        let result = patt.test(inputText);
        if (inputText > 2147483647 && result == true)
        {
            result = false;
        }
        return result;
    }

    saveExtendData(callback) {
        this.hideMessageTip();
        if (!this.isAvailableNumber(this.state.extendSetting.Number)) {
            this.setState({ isSaving: true });
            callback(false, this.formData);
            return;
        }
        let errorMsg = RMResx.RM_JS_RDM_Hold_SuspendHoldError;
        let profile = {};
        //profile.Type = this.state.holdType;
        profile.Number = this.state.extendSetting.Number.trim();
        profile.Unit = this.state.extendSetting.Unit;
        let holdIds = [];
        this.formData.holdItem.map((item, index) => { holdIds.push(item.Id); });
        let postData = {HoldSetting: profile, HoldIds: holdIds, HoldCategory: this.formData.holdCategory};
        let option = {
            url: "/api/RecordsExplorerApi/SusPendHolds",
            method: "POST",
            data: postData
        };
        fetchUtility(option).then((result) => {
            if (result == "") {
                callback(true, this.formData);
            } else {
                let tipMsg = result || errorMsg;
                this.showMessageTip("error", tipMsg);
                callback(false, this.formData);
            }
        }).catch((e) => {
            callback(false, this.formData);
        });
    }

    saveFormData(callback) {
        //let timezoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        this.hideMessageTip();
        const expiryEmailForm = this.expiryEmailFormRef.current;

        if (expiryEmailForm && typeof expiryEmailForm.setReminderErrorMessage === 'function') {
            expiryEmailForm.setReminderErrorMessage("");
        }
        if (expiryEmailForm && !expiryEmailForm.validate()) {
            this.setState({ isSaving: true });
            callback(false, this.formData);
            return;
        }
        if (this.state.isUseExistingHold == true && this.reuseProfile == null) {
            this.setState({ isSaving: true });
            callback(false, this.formData);
            return;
        } else if (
            $.trim(this.state.holdProfile.Name) == "" ||
            (this.state.holdType == 1 && this.state.holdProfile.CalenderTime == null) ||
            (this.state.holdType == 0 && !this.isAvailableNumber(this.state.holdProfile.Number)) ||
            (!this.state.holdProfile.HoldUserManagers || this.state.holdProfile.HoldUserManagers.length === 0)
        ) {
            this.setState({ isSaving: true, isSavingHoldNumber: true, isSavingHoldName: true });
            callback(false, this.formData);
            return;
        }
        let errorMsg = '';
        const relatedIds =  this.formData.Id? [this.formData.Id] : [];
        const expiryEmailFormData = this.expiryEmailFormRef?.current?.getPayload();
        if (this.formData.formType == "new") {
            const isSelectFromCalendar = this.state.holdType == 1;
            errorMsg = RMResx.RM_PRM_PRE_Msg_HoldFailed;
            let profile = this.state.holdProfile;
            profile.Name = $.trim(profile.Name);
            profile.Type = this.state.holdType;
            profile.CalenderTime = RM.TimeUtil.getCommonDateStr(new Date(profile.CalenderTime));
            profile.Number = isSelectFromCalendar ? 0 : profile.Number || 0;
            let postData = {HoldSetting: {...profile, EmailNotification: expiryEmailFormData}, ReletedIds: relatedIds, HoldCategory: this.formData.holdCategory};
            let option = {
                url: "/api/RecordsExplorerApi/CreateHold",
                method: "POST",
                data: postData
            };
            fetchUtility(option).then((result) => {
                if (result == "") {
                    callback(true, this.formData);
                } else if (result == '-1') {
                    this.setState({ calenderTimeInvalid: true, isSaving: true });
                    callback(false, this.formData);
                } else if (result == RMResx.RM_JS_RDM_Hold_HoldNameExist) {
                    this.setState({ holdNameIsExist: true, isSaving: true });
                    callback(false, this.formData);
                }  else {
                    let tipMsg = result || errorMsg;
                    if (tipMsg === RMResx.RM_PRM_PRE_Msg_ErrorReminderDuration){
                        this.expiryEmailFormRef?.current?.setReminderErrorMessage(tipMsg);
                        callback(false, this.formData);
                    } else {
                        this.showMessageTip("error", tipMsg);
                        callback(false, this.formData);
                    }
                }
            }).catch((e) => {
                callback(false, this.formData);
            });
        } else if (this.formData.formType == "edit") {
            const isSelectFromCalendar = this.state.holdType == 1;
            errorMsg = RMResx.RM_PRM_PRE_Msg_EditHoldFailed;
            let profile = this.state.holdProfile;
            profile.Type = this.state.holdType;
            profile.CalenderTime = RM.TimeUtil.getCommonDateStr(new Date(profile.CalenderTime));
            profile.Number = isSelectFromCalendar ? 0 : profile.Number || 0;
            //electronic 参数
            // if(this.formData.holdCategory == 0){
            //     let elecHoldInfo = RM.deepcopy(this.state.holdProfile);
            //     let elecHoldSetting = {
            //         Name: profile.Name,
            //         Type: this.state.holdType,
            //         ProfileType: 0,
            //         Id: profile.Id
            //     };
            //     if (elecHoldSetting.Type == 0) {
            //         elecHoldSetting.Number = elecHoldInfo.Number;
            //         elecHoldSetting.Unit = elecHoldInfo.Unit;
            //         elecHoldSetting.Description = elecHoldInfo.Description;
            //     } else {
            //         elecHoldSetting.IsDayLightSaving = timezoneInfo.autoAdjustClock;
            //         elecHoldSetting.TimeZoneId = timezoneInfo.id;
            //         elecHoldSetting.CalenderTime = RM.TimeUtil.getCommonDateStr(new Date(elecHoldInfo.CalenderTime));
            //         elecHoldSetting.Description = elecHoldInfo.Description;
            //     }
            //     profile = elecHoldSetting;
            // }
            let postData = {HoldSetting: {...profile, EmailNotification: expiryEmailFormData}, ReletedIds: relatedIds, HoldCategory: this.formData.holdCategory};
            let option = {
                url: "/api/RecordsExplorerApi/EditHold",
                method: "POST",
                data: postData
            };
            fetchUtility(option).then((result) => {
                if (result == "") {
                    callback(true, this.formData);
                } else {
                    let tipMsg = result || errorMsg;
                    if (tipMsg === RMResx.RM_PRM_PRE_Msg_ErrorReminderDuration){
                        this.expiryEmailFormRef?.current?.setReminderErrorMessage(tipMsg);
                        callback(false, this.formData);
                    } else {
                        this.showMessageTip("error", tipMsg);
                        callback(false, this.formData);
                    }
                }
            }).catch((e) => {
                callback(false, this.formData);
            });
        }
    }

    onHoldTypeChange(args) {
        let holdType = args.newValue.value;
        this.setState({ holdType: holdType, isSaving: false });
    }

    onHoldUnitChange(args) {
        let profile = this.state.holdProfile;
        let holdUnit = args.newValue.value;
        profile.Unit = holdUnit;
        this.setState({ holdProfile: profile });
    }

    onHoldTileChange(column, value) {
        let profile = this.state.holdProfile;
        if (column == "title") {
            profile.Name = $.trim(value);
            this.setState({ holdNameIsExist: false, isSavingHoldName: true });
        } else if (column == "number") {
            profile.Number = value;
            this.setState({ isSavingHoldNumber: true });
        } else if (column == "comment") {
            profile.Description = value;
        }
        this.setState({ holdProfile: profile, isSaving: false });
        this.setState({ isSaving: true });
    }

    onHoldUsersChange = (items) => {
        let profile = this.state.holdProfile;
        const uniqueRecipients = Array.from(
            new Map(items.map((item) => [item.UserId, item])).values(),
        );
        profile.HoldUserManagers = uniqueRecipients;
        this.setState({ holdProfile: profile, isSaving: false });
        this.setState({ isSaving: true });
    };

    onNotifyHoldManagerChange = (e, checked) => {
        let profile = this.state.holdProfile;
        let isChecked = typeof e === "boolean" ? e : (typeof checked === "boolean" ? checked : (e && e.target ? e.target.checked : !profile.IsHoldManagerEmailNotificationEnabled));
        profile.IsHoldManagerEmailNotificationEnabled = isChecked;
        this.setState({ holdProfile: profile });
    }

    onHoldDateChange(args) {
        let profile = this.state.holdProfile;
        var date = args.newValue;
        var zone = RM.TimeUtil.getGlobalTimezoneInfo();
        profile.CalenderTime = date;
        profile.CalendarDate = date;
        profile.TimeZoneId = zone.id;
        profile.IsDayLightSaving = zone.autoAdjustClock;
        this.setState({
            holdProfile: profile, isSaving: false
        });
        this.setState({isSaving: true});
        if ( date && date.getTime() > new Date().getTime()) {
            this.setState({
                calenderTimeInvalid: false
            });
        }
    }
    renderHoldDatePicker() {
        let selDate = null;
        if (this.state.holdProfile.CalenderTime != null) {
            selDate = new Date(this.state.holdProfile.CalenderTime);
        }
        return (
            <R.Datepicker
                id="raManageHoldDate"
                selectedDate={selDate}
                data-part="vtWidget"
                width={300}
                dateTimeFormat={this.defaultDateFormat}
                hasTimePicker={true}
                onChange={this.onHoldDateChange.bind(this)}
                triggerBySource={true}
                todayClick={this.todayClick}
            />
        );
    }
    renderHoldPhyObjetForm() {
        return <React.Fragment>
            <$g.FormRow label={RMResx.RM_JS_RDM_Hold_HoldName.replace(":","")} require={true} id="ariaHoldName" key="h3">
                <R.Validation> 
                    <R.Input
                        id="raManageHoldNameIpt"
                        type="text"
                        width={300}
                        value={this.state.holdProfile.Name}
                        onChange={this.onHoldTileChange.bind(this, "title")}
                        disabled={this.state.isEdit}
                        aria={{ 'aria-labelledby': 'ariaHoldName', 'aria-required': true }}
                    />
                    <R.ValidationFaker valid={!(this.state.isSaving && this.state.isSavingHoldName && this.state.holdProfile.Name == "")} of="#raManageHoldNameIpt" message= {RMResx.RM_JS_RDM_Hold_NoName} />
                    <R.ValidationFaker valid={!(this.state.isSaving && this.state.holdNameIsExist && this.state.holdProfile.Name)} of="#raManageHoldNameIpt" message= {RMResx.RM_JS_RDM_Hold_HoldNameExist} />
                    {/* <$g.ValidationMsg show={this.state.isSaving && this.state.holdProfile.Name == ""} >
                        {RMResx.RM_JS_RDM_Hold_NoName}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={this.state.isSaving && this.state.holdNameIsExist && this.state.holdProfile.Name} >
                        {RMResx.RM_JS_RDM_Hold_HoldNameExist}
                    </$g.ValidationMsg> */}
                </R.Validation>
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_RDM_Hold_Until.replace(":","")} require={true} id="ariaHoldUntil" key="h4">
                <R.Validation> 
                    <R.Combobox
                        id="raManageHoldType"
                        checkedField="checked"
                        textField="title"
                        valueField="value"
                        searchable={false}
                        width={300}
                        disabled={false}
                        items={this.state.holdTypeItems}
                        onChange={this.onHoldTypeChange.bind(this)}
                        aria={{
                            ariaLabelledby: "ariaHoldUntil",
                            ariaRequired: true
                        }}
                    />
                    {this.state.holdType == 0 &&
                    <div className="ra-inline-middle margin-top-s margin-bottom-s">
                        <R.Input
                            id="raManageHoldUnitNumIpt"
                            type="text"
                            width={148}
                            value={this.state.holdProfile.Number}
                            onChange={this.onHoldTileChange.bind(this, "number")}
                            aria={{ 'aria-labelledby': 'ariaHoldUntil', 'aria-required': true }}
                        />
                        <span style={{ width: '4px', display: 'inline-block' }} />
                        <R.Combobox
                            id="raManageHoldUnit"
                            checkedField="checked"
                            textField="title"
                            valueField="value"
                            searchable={false}
                            width={148}
                            disabled={false}
                            items={this.state.holdUnitItems}
                            onChange={this.onHoldUnitChange.bind(this)}
                        />
                    </div>
                    }
                    {this.state.holdType == 1 && <div id="hold-datapicker" className="margin-top-s margin-bottom-s">
                        <R.Validation>
                            {this.renderHoldDatePicker()}
                            <R.ValidationFaker valid={!(this.state.holdType == 1 && this.state.calenderTimeInvalid)} of="#hold-datapicker" message={RMResx.RM_JS_RDM_CreateRule_Validation_ConditionErrorDateTime} />
                            <R.ValidationFaker valid={!(this.state.isSaving == true && this.state.holdType == 1 && this.state.holdProfile.CalenderTime == null)} of="#hold-datapicker" message={RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime} />
                        </R.Validation>
                    </div>
                    }
                    <R.ValidationFaker valid={!(this.state.isSaving && this.state.isSavingHoldNumber && this.state.holdType == 0 && !this.isAvailableNumber(this.state.holdProfile.Number))} of="#raManageHoldUnitNumIpt" message= {RMResx.RM_JS_RDM_NotNumber} />
                </R.Validation>
               
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_JM_Comment} require={false} key="h5">
                <R.Input
                    type="textarea"
                    value={this.state.holdProfile.Description}
                    width={300}
                    onChange={this.onHoldTileChange.bind(this, "comment")}
                    aria={{ariaLabel:RMResx.RM_JS_JM_Comment}}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_JM_HoldManager} require={true} id="ariaHoldConfigure" key="h6">
                <div style={{ fontSize: "13px" }}>
                    <div>{ RMResx.RM_JS_HoldManager_Configure }</div>
                    <div className="margin-top-xs font-semibold">{ RMResx.RM_JS_HoldManager_UserOrGroupName_Title }</div>
                    <div className="margin-top-s">
                        <R.Validation>
                            <div id="holdManagerPickerWrapper">
                                <PeoplePicker
                                    id="raHoldManagersPicker"
                                    height={78}
                                    width={300}
                                    items={this.state.holdProfile.HoldUserManagers || []}
                                    selectionChanged={this.onHoldUsersChange}
                                    searchUsersByPermissionScope
                                />
                            </div>
                            <R.ValidationFaker 
                                valid={!(this.state.isSaving && (!this.state.holdProfile.HoldUserManagers || this.state.holdProfile.HoldUserManagers.length === 0))} 
                                of="#holdManagerPickerWrapper" 
                                message={RMResx.RM_JS_JM_HoldManager} 
                            />
                        </R.Validation>
                    </div>
                    <div className="margin-top-s">
                        <R.Checkbox
                            id="notifyHoldManagerChk"
                            text={RMResx.RM_JS_HoldManager_Email_Notification}
                            checked={this.state.holdProfile.IsHoldManagerEmailNotificationEnabled}
                            onChange={this.onNotifyHoldManagerChange}
                        />
                    </div>
                </div>
            </$g.FormRow>
        </React.Fragment>;
    }
    onHoldExtendTitleChange(value) {
        let profile = this.state.extendSetting;
        let holdNumber = value;
        profile.Number = holdNumber;
        this.setState({ extendSetting: profile, isSaving: false });
    }
    onHoldExtendUnitChange(args) {
        let profile = this.state.extendSetting;
        let holdUnit = args.newValue.value;
        profile.Unit = holdUnit;
        this.setState({ extendSetting: profile });
    }
    renderExtendForm() {
        return  <div>
            <$g.FormRow label={RMResx.RM_JS_RDM_Hold_SusPendHoldDes.replace(':',"")} require={true} id="ariaExtendHold">
                <div style={{ paddingTop: "5px" }} className="ra-inline-middle">
                    <R.Validation> 
                        <R.Input
                            type="text"
                            id="raManageExtendHoldUnitNumIpt"
                            width={148}
                            value={this.state.extendSetting.Number}
                            onChange={this.onHoldExtendTitleChange.bind(this)}
                            aria={{ 'aria-labelledby': 'ariaExtendHold', 'aria-required': true }}
                        />
                        <span style={{ width: '4px', display:'inline-block' }} />
                        <R.Combobox
                            id="raManageExtendHoldUnit"
                            checkedField="checked"
                            textField="title"
                            valueField="value"
                            searchable={false}
                            width={148}
                            disabled={false}
                            items={this.state.holdUnitItems}
                            onChange={this.onHoldExtendUnitChange.bind(this)}
                        />
                        <R.ValidationFaker valid={!(this.state.isSaving == true && !this.isAvailableNumber(this.state.extendSetting.Number))} of="#raManageExtendHoldUnitNumIpt" message={RMResx.RM_JS_RDM_NotNumber} />
                    </R.Validation>
                </div>
            </$g.FormRow>
        </div>;
    }

    render() {
        let formData = this.props.data;

        return <div id="phyholdForm">
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                status={{show: this.state.showTip}}
                onClose={this.hideMessageTip}
            />
            {
                (formData.formType == "edit" || formData.formType == "new")
                &&
                this.renderHoldPhyObjetForm()
            }
            {
                this.state.isExtendForm &&
                this.renderExtendForm()
            }
            {(formData.formType == "edit" || formData.formType == "new") && this.state.holdType == 1 &&  (
                <ExpiryEmailForm
                    ref={this.expiryEmailFormRef}
                    data={formData?.holdItem?.EmailNotification}
                />
            )}
        </div>;
    }

}