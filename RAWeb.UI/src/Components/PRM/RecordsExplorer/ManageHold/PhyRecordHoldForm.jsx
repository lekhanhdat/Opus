﻿import React from "react";
import {bindEvents} from "../../../../Utilities/CommonUtil";
import {NodeType} from "../../../../Constants/DAEnums";
import { ExpiryEmailForm } from "./ExpiryEmailForm";
import PeoplePicker from "../../../Common/PeoplePicker";

export default class PhyRecordHoldForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.expiryEmailFormRef = React.createRef();
        bindEvents(this, "showMessageTip", "hideMessageTip", "onHoldRadioChange", "saveFormData", "onHoldTypeChange", "renderExtendForm",
            "reloadHoldProfile", "saveExtendData",);
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();

        this.isPhysicalRecords = props.type == 'phy';
        this.holdCategory = 0;
        this.state = {
            isExtendForm: false,
            showTip: false,
            tipType: "success",
            tipMsg: "",
            calenderTimeInvalid: false,
            holdNameIsExist: false,
            showDispClassPanel: {show: false},
            useExistingHold: true,
            isSaving: false,
            isSavingHoldNumber: false,
            isSavingHoldName: false,
            data: [],
            holdTypeItems: [],
            holdUnitItems: [],
            useExistingRadioVal: "0",
            holdProfileList: [],
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
                ProfileType: -1,
                HoldManagers: "",
                HoldUserManagers: [],
                IsHoldManagerEmailNotificationEnabled: false
            },
            extendSetting: {Number: "", Unit: 0},
            boxOverideDialogShow: false,
            conflictedOption: "0",
            hasLoanedItems: false,
            confirmMessage: "",
            isSendEmailToBorrower: false
        };
        this.isContainerHold = false;
        this.reuseProfile = null;
        this.isSendEmailToBorrower = false;
    }

    componentReceive(type, ...args) {
        switch (type) {
            case "onSavePhyHold":
                if (!this.isValidHoldUtilValue()) {
                    $$.loading(false);
                    return;
                };
                if (this.state.isExtendForm) {
                    this.saveExtendData(args[0]);//args[0],args[1]
                } else {
                    const callback = args[0];
                    const isValid = this.validateFormData(callback);
                    if (!isValid) return;
                    
                    if (this.state.hasLoanedItems) {
                        this.onHoldConfirming(callback);
                    } else {
                        this.saveFormData(callback);
                    }
                }
                break;
            case "onSaveElectronicHold":
                if (this.state.isExtendForm) {
                    this.saveElectronicExtendData(args[0],args[1]);
                } else {
                    this.saveElectronicFormData(args[0],args[1]);
                }
                break;
        }
    }

    componentInit() {
        this.initData(this.props.data);
    }


    initData(args) {
        this.formData = args;
        this.records = this.formData.records;
        this.treeNode = this.formData.treeNode;
        const recordIds = args.records.map(item => item.Id);

        const currentUser = this.getCurrentUserForHoldManager();
        const defaultHoldUserManagers = currentUser ? [currentUser] : [];
        const defaultHoldManagersName = currentUser ? currentUser.DisplayName : "";

        switch (args.formType) {
            case "new":
                this.setState({
                    holdFormType: "new",
                    holdProfile: {
                        ...this.state.holdProfile,
                        HoldUserManagers: defaultHoldUserManagers,
                        HoldManagers: defaultHoldManagersName
                    }
                });
                this.intHoldMeta(true);
                this.itemLoanedChecking(recordIds);
                break;
            case "change":
                this.setState({holdFormType: "change"});
                this.intHoldMeta(true);
                break;
            case "append":
                this.setState({
                    holdFormType: "append",
                    holdProfile: {
                        ...this.state.holdProfile,
                        HoldUserManagers: defaultHoldUserManagers,
                        HoldManagers: defaultHoldManagersName
                    }
                });
                this.intHoldMeta(true);
                this.itemLoanedChecking(recordIds);
                break;
            case "extend":
                this.setState({ extendSetting: {Number: "", Unit: 0}, isExtendForm: true });
                this.intHoldMeta();
                break;
            default:
                return;
        }
    }

    itemLoanedChecking(recordIds) {
        const option = {
            url: "/api/RecordsExplorerApi/CheckItemOnLoaned",
            method: "POST",
            data: recordIds
        };
        fetchUtility(option).then(result => {
            if (result.MessageType == 4) {
                this.setState({ hasLoanedItems: true, confirmMessage: result.ErrorMessage });
            } else {
                this.setState({ hasLoanedItems: false, confirmMessage: '' });
            }
        }).catch(e => {
            console.error("CheckItemOnLoaned error", e);
        });
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

    intHoldMeta(loadProfile) {
        this.setState({holdTypeItems: this.initHoldTypeCombo(), holdUnitItems: this.initHoldUnitCombo()});
        if (loadProfile) {
            let option = {
                url: "/api/RecordsExplorerApi/GetSampleAllHolds",
                method: "Get"
            };
            fetchUtility(option).then((result) => {
                if (result != null) {
                    let res = result;
                    this.setState({holdProfileList: res});
                    if (this.formData.formType == "append" && this.formData.records.length == 1) {
                        this.reloadHoldProfile();
                    }
                }
            }).catch((e) => {
                //console.log(e);
            });
        }
    }

    reloadHoldProfile() {
        let recordId = this.formData.records[0].Id;
        let url = "/api/RecordsExplorerApi/LoadElecHoldSetting?recordId=" + recordId;
        if (this.isPhysicalRecords) {
            url = "/api/RecordsExplorerApi/LoadPhyHoldSetting?recordId=" + recordId;
        }
        let option = {
            url: url,
            method: "Post"
        };
        fetchUtility(option).then((result) => {
            if (result != null) {
                this.usedHoldId = result.Id;
                let appendHoldProfileList = [];
                this.state.holdProfileList.forEach((item) => {
                    if (!result.find(i => i == item.Id)) {
                        appendHoldProfileList.push(item);
                    }
                });
                this.setState({ holdProfileList: appendHoldProfileList });
            } else {
                this.usedHoldId = '';
            }
        }).catch((e) => {
            this.usedHoldId = '';
            //console.log(e);
        });
    }

    initHoldTypeCombo() {
        return [{
            value: 0,
            title: RMResx.RM_JS_RDM_Hold_InputDurationNumber,
            checked: false,
        }, {
            value: 1,
            title: RMResx.RM_JS_RDM_Hold_Canlender,
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

    onCheckChangeSendEmailToBorrower = (args) => {
        this.isSendEmailToBorrower = args;
        this.setState({ isSendEmailToBorrower: args });
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

    onHoldConfirming = (callback) => {
        $$.messagedialog(true, {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className="margin-bottom-m">{this.state.confirmMessage}</div>
                <R.Checkbox
                    name="checkbox-send-email-to-borrower"
                    text={RMResx.RM_JS_RDM_SendEmailToBorrower}
                    title={RMResx.RM_JS_RDM_SendEmailToBorrower}
                    checked={this.state.isSendEmailToBorrower}
                    onChange={this.onCheckChangeSendEmailToBorrower}
                />
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        callback(false, this.formData)
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.saveFormData(callback);
                        $$.messagedialog(false);
                    },
                },
            ],
        });
    };

    saveElectronicExtendData(callback,isOverWrite) {
        if (!this.isAvailableNumber(this.state.extendSetting.Number)) {
            this.setState({isSaving: true});
            callback(false, this.formData);
            return;
        }
        let errorMsg = RMResx.RM_JS_RDM_Hold_SuspendHoldError;
        let profile = {};
        profile.Type = this.state.holdType;
        profile.Number = this.state.extendSetting.Number;
        profile.Unit = this.state.extendSetting.Unit;
        let recordIds = [];
        this.formData.records.map((item, index) => {
            recordIds.push(item.Id);
        });
        let postData = {HoldSetting: profile, ReletedIds: recordIds, HoldCategory: this.holdCategory,IsOverRide:isOverWrite};
        let option = {
            url: "/api/RecordsExplorerApi/SusPendRecords",
            method: "POST",
            data: postData
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (result == "") {
                callback(true, this.formData);
            } else {
                let tipMsg = result || errorMsg;
                this.showMessageTip("error", tipMsg);
                callback(false, this.formData);
            }
        }).catch((e) => {
            callback(false, this.formData);
        }).finally(() => $$.loading(false));
    }
    handleError(response){
        $$.loading(false);
        if(response.status == 403){
            $$.messagedialog(true, {
                // classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_Common_NoPermissionLicense,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () =>{ $$.messagedialog(false) }
                    }
                ]
            });
        }
    }
    saveExtendData(callback) {
        if (!this.isAvailableNumber(this.state.extendSetting.Number)) {
            this.setState({isSaving: true});
            callback(false, this.formData);
            return;
        }
        let errorMsg = RMResx.RM_JS_RDM_Hold_SuspendHoldError;
        let profile = {};
        profile.Type = this.state.holdType;
        profile.Number = this.state.extendSetting.Number;
        profile.Unit = this.state.extendSetting.Unit;
        let recordIds = [];
        this.formData.records.map((item, index) => {
            recordIds.push(item.Id);
        });
        let postData = {HoldSetting: profile, ReletedIds: recordIds, HoldCategory: this.holdCategory};
        let option = {
            url: "/api/RecordsExplorerApi/SusPendRecords",
            method: "POST",
            data: postData
        };
        $$.loading(true);
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((result) => {
            if (result == "") {
                callback(true, this.formData);
            } else {
                let tipMsg = result || errorMsg;
                this.showMessageTip("error", tipMsg);
                callback(false, this.formData);
            }
        }).catch((e) => {
            callback(false, this.formData);
        }).finally(() => $$.loading(false));
    }

    validateFormData = (callback) => {
        const expiryEmailForm = this.expiryEmailFormRef.current;
        if ((expiryEmailForm && !expiryEmailForm.validate()) || (this.state.useExistingHold == true && this.reuseProfile == null)) {
            this.setState({isSaving: true});
            callback(false, this.formData);
            return false;
        }
        if (this.state.useExistingHold == false && (this.state.holdProfile.Name == "" || (this.state.holdType == 1 && this.state.holdProfile.CalenderTime == null) || (this.state.holdType == 0 && !this.isAvailableNumber(this.state.holdProfile.Number)))) {
            this.setState({ isSaving: true, isSavingHoldNumber: true, isSavingHoldName: true });
            callback(false, this.formData);
            return false;
        }
        return true;
    };

    saveFormData(callback) {
        const expiryEmailFormData = this.expiryEmailFormRef?.current?.getPayload();
        let recordIds = [];
        this.formData.records.map((item, index) => {
            recordIds.push(item.Id);
        });
        let fileIds = [];
        //if (this.formData.records[0].NodeType == NodeType.PhyFile) {
        this.formData.records.map((item, index) => {
            fileIds.push({Id: item.Id, BoxId: item.BoxId, LocationId: item.LocationId, NodeType: item.NodeType});
        });
        //}

        if (this.formData.formType == "new" || this.formData.formType == "append") {
            this.prePlaceHold(this.formData.records, recordIds, fileIds, callback);
        }

        let errorMsg = '';
        if (this.formData.formType == "change") {
            errorMsg = RMResx.RM_PRM_PRE_Msg_ChangeHoldFailed;
            if (this.state.useExistingHold == true) {  //reuse
                let postData = {
                    HoldSetting: {...this.reuseProfile, EmailNotification: expiryEmailFormData},
                    ReletedIds: recordIds,
                    HoldCategory: this.holdCategory,
                    HoldAction: this.formData.formType,
                    FileIds: fileIds
                };
                let option = {
                    url: "/api/RecordsExplorerApi/ChangeHoldReuse",
                    method: "POST",
                    data: postData
                };
                $$.loading(true);
                fetchUtility(option, response => {
                    this.handleError(response);
                }).then((result) => {
                    if (result == "") {
                        callback(true, this.formData);
                    } else if (result == '-1') {
                        this.setState({calenderTimeInvalid: true, isSaving: true});
                        callback(false, this.formData);
                    } else {
                        let tipMsg = result || errorMsg;
                        this.showMessageTip("error", tipMsg);
                        callback(false, this.formData);
                    }
                }).catch((e) => {
                    callback(false, this.formData);
                }).finally(() => $$.loading(false));
            } else {  //new hold
                const isSelectFromCalendar = this.state.holdType == 1;
                let profile = this.state.holdProfile;
                profile.Type = this.state.holdType;
                profile.CalenderTime = RM.TimeUtil.getCommonDateStr(new Date(profile.CalenderTime));
                profile.ProfileType = -1;
                profile.Number = isSelectFromCalendar ? 0 : profile.Number || 0;
                let postData = {
                    HoldSetting: {...profile, EmailNotification: expiryEmailFormData},
                    ReletedIds: recordIds, 
                    HoldCategory: this.holdCategory, 
                    HoldAction: this.formData.formType,
                    FileIds: fileIds
                };
                let option = {
                    url: "/api/RecordsExplorerApi/ChangeHoldCreate",
                    method: "POST",
                    data: postData
                };
                $$.loading(true);
                fetchUtility(option, response => {
                    this.handleError(response);
                }).then((result) => {
                    if (result == "") {
                        callback(true, this.formData);
                    } else if (result == '-1') {
                        this.setState({calenderTimeInvalid: true, isSaving: true});
                        callback(false, this.formData);
                    } else if (result == RMResx.RM_JS_RDM_Hold_HoldNameExist) {
                        this.setState({holdNameIsExist: true, isSaving: true});
                        callback(false, this.formData);
                    } else {
                        let tipMsg = result || errorMsg;
                        this.showMessageTip("error", tipMsg);
                        callback(false, this.formData);
                    }
                }).catch((e) => {
                    callback(false, this.formData);
                }).finally(() => $$.loading(false));
            }
        }
    }

    // electronic hold save
    saveElectronicFormData(callback,isOverWrite) {
        const expiryEmailForm = this.expiryEmailFormRef.current;
        const expiryEmailFormData = this.expiryEmailFormRef?.current?.getPayload();
        if (expiryEmailForm && !expiryEmailForm.validate()) {
            this.setState({isSaving: true});
            callback(false, this.formData);
            return;
        }
        if (this.state.useExistingHold == true && this.reuseProfile == null) {
            this.setState({isSaving: true});
            callback(false, this.formData);
            return;
        } else if (this.state.useExistingHold == false && (this.state.holdProfile.Name == "" || (this.state.holdType == 1 && this.state.holdProfile.CalenderTime == null) || (this.state.holdType == 0 && !this.isAvailableNumber(this.state.holdProfile.Number)))) {
            this.setState({ isSaving: true, isSavingHoldNumber: true, isSavingHoldName: true });
            callback(false, this.formData); 
            return;
        }
        let recordIds = [];
        this.formData.records.map((item, index) => {
            recordIds.push(item.Id);
        });
        let elecHoldSetting = {};
        if(this.state.useExistingHold){
            let holdSettingInfo = RM.deepcopy(this.reuseProfile);
            elecHoldSetting = {
                Name: holdSettingInfo.Name,
                Type: holdSettingInfo.Type,
                Id: holdSettingInfo.Id, 
                ProfileType: holdSettingInfo.ProfileType            
            };
            if (elecHoldSetting.Type == 0) {
                elecHoldSetting.Number = holdSettingInfo.Number;
                elecHoldSetting.Unit = holdSettingInfo.Unit;
                elecHoldSetting.Description = holdSettingInfo.Description;
            } else {
                elecHoldSetting.IsDayLightSaving = holdSettingInfo.IsDayLightSaving;
                elecHoldSetting.TimeZoneId = holdSettingInfo.TimeZoneId;
                elecHoldSetting.CalenderTime = holdSettingInfo.CalenderTime;
                elecHoldSetting.Description = holdSettingInfo.Description;
            }
        }else{
            let profile = RM.deepcopy(this.state.holdProfile);
            elecHoldSetting = {
                Name: profile.Name,
                Type: this.state.holdType,
                ProfileType: -1,
                EmailNotification: expiryEmailFormData,
                HoldUserManagers: profile.HoldUserManagers,
                HoldManagers: profile.HoldManagers,
                IsHoldManagerEmailNotificationEnabled: profile.IsHoldManagerEmailNotificationEnabled
            };
            if (elecHoldSetting.Type == 0) {
                elecHoldSetting.Number = profile.Number || 0;
                elecHoldSetting.Unit = profile.Unit;
                elecHoldSetting.Description = profile.Description;
            } else {
                elecHoldSetting.IsDayLightSaving = profile.IsDayLightSaving;
                elecHoldSetting.TimeZoneId = profile.TimeZoneId;
                elecHoldSetting.CalenderTime = RM.TimeUtil.getCommonDateStr(new Date(profile.CalenderTime));
                elecHoldSetting.Description = profile.Description;
            }
        }
        this.elecHoldSetting = elecHoldSetting;
        if (this.formData.formType == "new") {
            this.placeHoldElectronic(recordIds, callback, isOverWrite);
        }
        if (this.formData.formType == "append") {
            this.placeHoldElectronic(recordIds, callback, false);
        }
        let errorMsg = '';
        if (this.formData.formType == "change") {
            errorMsg = RMResx.RM_PRM_PRE_Msg_ChangeHoldFailed;
            if (this.state.useExistingHold == true) {  //reuse
                let postData = {
                    HoldSetting: {...this.elecHoldSetting, EmailNotification: expiryEmailFormData},
                    HoldAction: this.formData.formType,
                    ReletedIds: recordIds,
                    IsOverRide: isOverWrite
                };
                let option = {
                    url: "/api/RecordsExplorerApi/ChangeHoldReuse",
                    method: "POST",
                    data: postData
                };
                fetchUtility(option, response => {
                    this.handleError(response);
                }).then((result) => {
                    if (result == "") {
                        callback(true, this.formData);
                    } else if (result == '-1') {
                        this.setState({calenderTimeInvalid: true, isSaving: true});
                        callback(false, this.formData);
                    } else {
                        let tipMsg = result || errorMsg;
                        this.showMessageTip("error", tipMsg);
                        callback(false, this.formData);
                    }
                }).catch((e) => {
                    callback(false, this.formData);
                });
            } else {  //new hold
                let postData = {
                    HoldSetting: {...this.elecHoldSetting, EmailNotification: expiryEmailFormData},
                    HoldAction: this.formData.formType,
                    ReletedIds: recordIds,
                    IsOverRide: isOverWrite
                };
                let option = {
                    url: "/api/RecordsExplorerApi/ChangeHoldCreate",
                    method: "POST",
                    data: postData
                };
                fetchUtility(option, response => {
                    this.handleError(response);
                }).then((result) => {
                    if (result == "") {
                        callback(true, this.formData);
                    } else if (result == '-1') {
                        this.setState({calenderTimeInvalid: true, isSaving: true});
                        callback(false, this.formData);
                    } else if (result == RMResx.RM_JS_RDM_Hold_HoldNameExist) {
                        this.setState({holdNameIsExist: true, isSaving: true});
                        callback(false, this.formData);
                    } else {
                        let tipMsg = result || errorMsg;
                        this.showMessageTip("error", tipMsg);
                        callback(false, this.formData);
                    }
                }).catch((e) => {
                    callback(false, this.formData);
                });
            }
        }
    }

    prePlaceHold(records, recordIds, fileIds, callback) {
        let boxNodes = records.filter(node => node.NodeType == NodeType.PhyBox);
        if (boxNodes.length == 0) {
            this.placeHold(recordIds, fileIds, false, false, callback);
        } else {
            let boxIds = boxNodes.map(node => node.Id);
            this.recordIds = recordIds;
            this.fileIds = fileIds;
            this.callback = callback;
            let url = "/api/PhysicalRecordApi/IsBoxHasHoldChildren";
            let option = {
                url: url,
                method: "POST",
                data: {NodeId: boxIds, NodeType: NodeType.PhyBox}
            };
            fetchUtility(option).then((result) => {
                if (result.HasChildrenHold) {
                    this.setState({boxOverideDialogShow: true});
                } else {
                    this.placeHold(this.recordIds, this.fileIds, false, false, this.callback);
                }
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    placeHold(recordIds, fileIds, override, needCheckOverride, callback) {
        let errorMsg = '';
        const expiryEmailFormData = this.expiryEmailFormRef?.current?.getPayload();
        if (this.state.useExistingHold == true) {  //reuse
            errorMsg = RMResx.RM_JS_RDM_Hold_CreateRecordHoldError;
            let postData = {
                HoldSetting: {...this.reuseProfile, EmailNotification: expiryEmailFormData},
                ReletedIds: recordIds,
                HoldCategory: this.holdCategory,
                HoldAction: this.formData.formType,
                FileIds: fileIds,
                NeedCheckOverride: needCheckOverride,
                IsOverRide: override,
                IsSendEmailToBorrower: this.isSendEmailToBorrower
            };
            let option = {
                url: "/api/RecordsExplorerApi/ReuseHoldTypeWithRecord",
                method: "POST",
                data: postData
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                $$.loading(false);
                if (result == "") {
                    callback(true, this.formData);
                } else if (result == '-1') {
                    this.setState({calenderTimeInvalid: true, isSaving: true});
                    callback(false, this.formData);
                } else {
                    let tipMsg = result || errorMsg;
                    this.showMessageTip("error", tipMsg);
                    callback(false, this.formData);
                }
            }).catch((e) => {
                $$.loading(false);
                callback(false, this.formData);
            }).finally(() => $$.loading(false));
        } else {  //new hold
            const isSelectFromCalendar = this.state.holdType == 1;
            errorMsg = RMResx.RM_JS_RDM_Hold_CreateRecordHoldError;
            let profile = this.state.holdProfile;
            profile.Type = this.state.holdType;
            profile.ProfileType = -1;
            profile.Number = isSelectFromCalendar ? 0 : profile.Number || 0;
            profile.CalenderTime = RM.TimeUtil.getCommonDateStr(new Date(profile.CalenderTime));
            let postData = {
                HoldSetting: {...profile, EmailNotification: expiryEmailFormData},
                ReletedIds: recordIds,
                HoldCategory: this.holdCategory,
                HoldAction: this.formData.formType,
                FileIds: fileIds,
                NeedCheckOverride: needCheckOverride,
                IsOverRide: override,
                IsSendEmailToBorrower: this.isSendEmailToBorrower
            };
            let option = {
                url: "/api/RecordsExplorerApi/CreateHoldTypeWithRecord",
                method: "POST",
                data: postData
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                $$.loading(false);
                if (result == "") {
                    callback(true, this.formData);
                } else if (result == '-1') {
                    this.setState({calenderTimeInvalid: true, isSaving: true});
                    callback(false, this.formData);
                } else if (result == RMResx.RM_JS_RDM_Hold_HoldNameExist) {
                    this.setState({holdNameIsExist: true, isSaving: true});
                    callback(false, this.formData);
                } else {
                    let tipMsg = result || errorMsg;
                    this.showMessageTip("error", tipMsg);
                    callback(false, this.formData);
                }
            }).catch((e) => {
                $$.loading(false);
                callback(false, this.formData);
            }).finally(() => $$.loading(false));
        }
    }

    placeHoldElectronic(recordIds, callback, isOverWrite ) {
        let errorMsg = '';
        const expiryEmailFormData = this.expiryEmailFormRef?.current?.getPayload();
        if (this.state.useExistingHold == true) {  //reuse
            errorMsg = RMResx.RM_JS_RDM_Hold_CreateRecordHoldError;
            let postData = {
                ReletedIds: recordIds,
                HoldSetting: {...this.elecHoldSetting, EmailNotification: expiryEmailFormData},
                HoldCategory: this.holdCategory,
                HoldAction: this.formData.formType,
                IsOverRide: isOverWrite,
            };
            let option = {
                url: "/api/RecordsExplorerApi/ReuseHoldTypeWithRecord",
                method: "POST",
                data: postData
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                $$.loading(false);
                if (result == "") {
                    callback(true, this.formData);
                } else if (result == '-1') {
                    this.setState({calenderTimeInvalid: true, isSaving: true});
                    callback(false, this.formData);
                } else {
                    let tipMsg = result || errorMsg;
                    this.showMessageTip("error", tipMsg);
                    callback(false, this.formData);
                }
            }).catch((e) => {
                callback(false, this.formData);
                $$.loading(false);
            });
        } else {  //new hold
            errorMsg = RMResx.RM_JS_RDM_Hold_CreateRecordHoldError;
            let postData = {
                HoldSetting: {...this.elecHoldSetting, EmailNotification: expiryEmailFormData},
                ReletedIds: recordIds,
                HoldCategory: this.holdCategory,
                HoldAction: this.formData.formType,
                IsOverRide: isOverWrite
            };
            let option = {
                url: "/api/RecordsExplorerApi/CreateHoldTypeWithRecord",
                method: "POST",
                data: postData
            };
            $$.loading(true);
            fetchUtility(option, response => {
                this.handleError(response);
            }).then((result) => {
                $$.loading(false);
                if (result == "") {
                    callback(true, this.formData);
                } else if (result == '-1') {
                    this.setState({calenderTimeInvalid: true, isSaving: true});
                    callback(false, this.formData);
                } else if (result == RMResx.RM_JS_RDM_Hold_HoldNameExist) {
                    this.setState({holdNameIsExist: true, isSaving: true});
                    callback(false, this.formData);
                } else {
                    let tipMsg = result || errorMsg;
                    this.showMessageTip("error", tipMsg);
                    callback(false, this.formData);
                }
            }).catch((e) => {
                callback(false, this.formData);
                $$.loading(false);
            });
        }
    }

    isAvailableNumber(inputText) {
        if (inputText == "" || inputText == "0") {
            return false;
        }
        let patt = new RegExp("^[0-9]*$", "g");
        let result = patt.test(inputText);
        if (patt) {
            if (inputText * 1 == 0) {
                result = false;
            }
        }
        return result;
    }

    onHoldTypeChange(args) {
        let holdType = args.newValue.value;
        this.setState({holdType: holdType, isSaving: false});
    }

    onHoldUnitChange(args) {
        let profile = this.state.holdProfile;
        let holdUnit = args.newValue.value;
        profile.Unit = holdUnit;
        this.setState({holdProfile: profile});
    }

    onHoldProfileChange(args) {
        this.reuseProfile = args.newValue;
        this.setState({
            isSaving: false,
            calenderTimeInvalid: false
        });
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
        this.setState({holdProfile: profile, isSaving: false});
        this.setState({isSaving: true});
    }

    isValidHoldUtilValue = () => {
        if (this.state.isExtendForm || this.state.holdType == 1) return true;

        const limits = {
            0: 1825000,
            1: 260000,
            2: 60000,
            3: 5000,
        };

        const max = limits[this.state.holdProfile.Unit];
        if (max && this.state.holdProfile.Number > max) {
            return false;
        }
        return true;
    };

    onHoldDateChange(args) {
        let profile = this.state.holdProfile;
        var date = args.newValue;
        var zone = RM.TimeUtil.getGlobalTimezoneInfo();
        profile.CalenderTime = date;
        profile.CalendarDate = date;
        profile.TimeZoneId = zone.id;
        profile.IsDayLightSaving = zone.autoAdjustClock;
        this.setState({
            holdProfile: profile, isSaving: false, calenderTimeInvalid: false
        });
        this.setState({isSaving: true});
    }

    renderHoldDatePicker() {
        let selDate = null;
        let timeZoneInfo = null;
        if (this.state.holdProfile.CalenderTime != null) {
            selDate = this.state.holdProfile.CalenderTime;
        } 
        return (
            <R.Datepicker
                id="raPhyHoldDate"
                selectedDate={selDate}
                data-part="vtWidget"
                width={300}
                dateTimeFormat={this.defaultDateFormat}
                hasTimeZone={true}
                hasTimePicker={true}
                selectedTimeZone={timeZoneInfo}
                onChange={this.onHoldDateChange.bind(this)}
                triggerBySource={true}
                todayClick={this.todayClick}
            />
        );
    }

    renderHoldPhyObjetForm() {
        return <React.Fragment>
            <$g.FormRow label={RMResx.RM_JS_RDM_Hold_HoldTypeTitle.replace(":","")} id="ariaHoldType" key="h1">
                <$g.RadioGroup
                    name="manage-hold-new-type"
                    onChange={this.onHoldRadioChange}
                    value={this.state.useExistingRadioVal}
                    aria="#ariaHoldType"
                    >
                    <$g.RadioOption value="0" text={RMResx.RM_JS_RDM_Hold_UseExist}/>
                    <$g.RadioOption value="1" text={RMResx.RM_JS_RDM_Hold_Create}/>
                </$g.RadioGroup>
            </$g.FormRow>
            {
                this.state.useExistingHold &&
                <$g.FormRow label={RMResx.RM_JS_PRM_Hold_RecordForm_SelectExistingHold.replace(":","")} require={true} id="ariaSelectHold" key="h2">
                    <R.Combobox
                        id="raPhyHoldProfiles"
                        checkedField="checked"
                        textField="Name"
                        valueField="Id"
                        width={300}
                        disabled={false}
                        items={this.state.holdProfileList}
                        onChange={this.onHoldProfileChange.bind(this)}
                        aria={{
                            ariaLabelledby: "ariaSelectHold",
                            ariaRequired: true
                        }}
                    />
                    <$g.ValidationMsg
                        show={this.state.isSaving == true && this.state.useExistingHold && this.reuseProfile == null}>
                        {RMResx.RM_JS_RDM_Hold_NeedSelectHold}
                    </$g.ValidationMsg>
                    <$g.ValidationMsg show={this.state.useExistingHold && this.state.calenderTimeInvalid}>
                        {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionErrorDateTime}
                    </$g.ValidationMsg>
                </$g.FormRow>
            }
            {
                !this.state.useExistingHold &&
                <React.Fragment>
                    <$g.FormRow label={RMResx.RM_JS_RDM_Hold_HoldName.replace(":","")} require={true} id="ariaHoldName" key="h3">
                        <R.Input
                            id="raPhyHoldProfileNameIpt"
                            type="text"
                            width={300}
                            value={this.state.holdProfile.Name}
                            onChange={this.onHoldTileChange.bind(this, "title")}
                            aria={{ 'aria-labelledby': 'ariaHoldName', 'aria-required': true }}
                        />
                        <$g.ValidationMsg show={this.state.isSaving && this.state.isSavingHoldName && this.state.holdProfile.Name == ""}>
                            {RMResx.RM_JS_RDM_Hold_NoName}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg show={this.state.isSaving && this.state.holdNameIsExist && this.state.holdProfile.Name}>
                            {RMResx.RM_JS_RDM_Hold_HoldNameExist}
                        </$g.ValidationMsg>
                    </$g.FormRow>
                    <$g.FormRow label={RMResx.RM_JS_RDM_Hold_Until.replace(":","")} require={true} id="ariaHoldUntil" key="h4">
                        <R.Combobox
                            id="raPhyHoldType"
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
                        <div style={{paddingTop: "5px"}} className="ra-inline-middle">
                            <R.Input
                                id="raPhyHoldNumberIpt"
                                type="text"
                                width={148}
                                value={this.state.holdProfile.Number}
                                onChange={this.onHoldTileChange.bind(this, "number")}
                                aria={{ 'aria-labelledby': 'ariaHoldUntil', 'aria-required': true }}
                            />
                            <span style={{width: '4px', display: 'inline-block'}}/>
                            <R.Combobox
                                id="raPhyPlaceHoldUnits"
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
                        {this.state.holdType == 1 &&
                        <div style={{paddingTop: "5px"}}>
                            {this.renderHoldDatePicker()}
                        </div>
                        }
                        <$g.ValidationMsg
                            show={this.state.holdType == 0 && !this.isValidHoldUtilValue()}>
                            {RMResx.RM_JS_RDM_Hold_Until_ValidateMsg}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg
                            show={this.state.isSaving && this.state.isSavingHoldNumber && this.state.holdType == 0 && !this.isAvailableNumber(this.state.holdProfile.Number)}>
                            {RMResx.RM_JS_RDM_NotNumber}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg show={this.state.holdType == 1 && this.state.calenderTimeInvalid}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionErrorDateTime}
                        </$g.ValidationMsg>
                        <$g.ValidationMsg
                            show={this.state.isSaving == true && this.state.holdType == 1 && this.state.holdProfile.CalenderTime == null}>
                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime}
                        </$g.ValidationMsg>
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
                    {
                        this.state.holdType === 1 && <ExpiryEmailForm ref={this.expiryEmailFormRef}/>
                    }
                </React.Fragment>
            }
        </React.Fragment>;
    }

    onHoldRadioChange(val) {
        let useExist = val == "0";
        this.reuseProfile = null;
        //let isUseExistingHold = this.state.useExistingHold;
        this.setState({useExistingHold: useExist, useExistingRadioVal: val, isSaving: false, calenderTimeInvalid: false});
    }

    onHoldExtendTitleChange(value) {
        let profile = this.state.extendSetting;
        let holdNumber = value;
        profile.Number = holdNumber;
        this.setState({extendSetting: profile, isSaving: false});
    }

    onHoldExtendUnitChange(args) {
        let profile = this.state.extendSetting;
        let holdUnit = args.newValue.value;
        profile.Unit = holdUnit;
        this.setState({extendSetting: profile});
    }

    onCancelBoxHold() {
        this.setState({boxOverideDialogShow: false});
    }

    onOKBoxHold() {
        this.placeHold(this.recordIds, this.fileIds, this.state.conflictedOption == "0", true, this.callback);
        this.setState({boxOverideDialogShow: false});
    }

    onHoldConflictedChange(val) {
        this.setState({conflictedOption: val});
    }

    renderBoxHoldDialog() {
        return <R.Dialog
            id="boxHoldConfirmDialog"
            header={RMResx.RM_PRM_Hold_Conflicted_FormTitle}
            width={500}
            status={{show: this.state.boxOverideDialogShow}}
            struct={{foot: true}}
            onHide={this.onCancelBoxHold.bind(this)}
            destroy={true}
        >
            <div id="boxConflictDialog_body">
                <$g.FormRow label={RMResx.RM_PRM_Hold_Conflicted_OptionHeader} key="h1">
                    <$g.RadioGroup
                        name="manage-hold-conflict-resolution-type"
                        onChange={this.onHoldConflictedChange.bind(this)}
                        value={this.state.conflictedOption}>
                        <$g.RadioOption value="0" text={RMResx.RM_PRM_Hold_Conflicted_OverrideWithParent}/>
                        <$g.RadioOption value="1" text={RMResx.RM_PRM_Hold_Conflicted_CompareChildren}/>
                    </$g.RadioGroup>
                </$g.FormRow>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelBoxHold.bind(this)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_OK} onClick={this.onOKBoxHold.bind(this)} />
            </>
        </R.Dialog>;
    }


    expanderShown() {
        this.setState({});
    }

    renderExtendForm() {
        let isShowByPhyDto = this.isPhysicalRecords;
        if (this.props.extend == "search") {
            isShowByPhyDto = false;
        }
        let expanderTitle = isShowByPhyDto ? RMResx.RM_JS_RDM_PhyExtendHoldDetailTitle : RMResx.RM_JS_BCM_ElecExtendHoldDetailTitle;
        let extendNameAttr = isShowByPhyDto ? 'Name' : 'LeafName';
        let extendIdAttr = isShowByPhyDto ? 'UniqueId' : 'RecordsId';
        
        return <div>
            <$g.FormRow label={RMResx.RM_JS_RDM_Hold_SusPendHoldDes.replace(':', "")} require={true} id="ariaExtendHold">
                <div style={{paddingTop: "5px"}} className="ra-inline-middle">
                    <R.Input
                        id="raPhyExtendHoldNumber"
                        type="text"
                        width={148}
                        value={this.state.extendSetting.Number}
                        onChange={this.onHoldExtendTitleChange.bind(this)}
                        aria={{ 'aria-labelledby': 'ariaExtendHold', 'aria-required': true }}
                    />
                    <span style={{width: '4px', display: 'inline-block'}}/>
                    <R.Combobox
                        id="raPhyExtendHoldUnit"
                        checkedField="checked"
                        textField="title"
                        valueField="value"
                        searchable={false}
                        width={148}
                        disabled={false}
                        items={this.state.holdUnitItems}
                        onChange={this.onHoldExtendUnitChange.bind(this)}
                    />
                </div>
                <$g.ValidationMsg
                    show={this.state.isSaving == true && !this.isAvailableNumber(this.state.extendSetting.Number)}>
                    {RMResx.RM_JS_RDM_NotNumber}
                </$g.ValidationMsg>
            </$g.FormRow>

            <$g.FormRow>
                <div className="phyhold-expander">
                    <R.Expander status={{show: true}} title={expanderTitle} onShow={this.expanderShown.bind(this)}>
                        <div className="phyhold-expander-list">
                            {
                                this.formData.records.map((item, index) => {
                                    return <div key={"item" + index} className="phyhold-expander-item" tabIndex="0">
                                        <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Dialog_WithReleaseTime}>
                                            <span>{item[extendNameAttr] + " (" + item[extendIdAttr] + ")"}</span>
                                            {
                                                isShowByPhyDto &&
                                                <span>{item.HoldReleaseTimeStr}</span>
                                            }
                                            {
                                                !isShowByPhyDto && <span>{item.ReleaseTime}</span>
                                            }
                                        </$g.I18NProvider>
                                    </div>;
                                })
                            }
                        </div>
                    </R.Expander>
                </div>
            </$g.FormRow>
        </div>;
    }

    render() {
        let formData = this.props.data;

        return (
            <div id="phyholdForm" className="phyobj-form">
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{show: this.state.showTip}}
                    onClose={this.hideMessageTip}
                />
                {
                    (formData.formType == "change" || formData.formType == "new" || formData.formType == "append")
                    &&
                    this.renderHoldPhyObjetForm()
                }
                {
                    this.state.isExtendForm &&
                    this.renderExtendForm()
                }
                {this.renderBoxHoldDialog()}
            </div>
        );
    }

}