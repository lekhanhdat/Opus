import { Component } from "react";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import { bindEvents, LicenseHelper, setCheckedStatus } from "../../Utilities/CommonUtil";
import "../../Less/CP/generalSetting.less";
import StringUtil from "../../Utilities/StringUtil";
import PeoplePicker from "../Common/PeoplePicker";
import _ from "lodash";

const EmailSenderType = {
    Default: 0,
    O365: 1,
};

export default class GeneralSetting extends Component {
    constructor(props) {
        super(props);
        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: RMResx.RM_JS_GS_SaveSuccess,
            recordsLabelInput: "",
            invalidRecordsLabelInput: false,
            sessionTimeoutValidation: { show: false, msg: "" },
            emailSenderDefinitionValidation: {
                appProfileIdValid: {
                    valid: true,
                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                },
                emailSenderValid: {
                    valid: true,
                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                },
            },
            sessionTimeUnits: [],
            timeZones: [],
            dateFormats: [],
            timeFormats: [],
            gsModel: {},
            SessionTime: "",
            emailSenderAppProfileSelectorItems: [],
            emailSenderTypeRadioItems: [
                {
                    text: (RMResx.RM_GS_Email_Default).format("Opus_Team@avepointonlineservices.com"),
                    value: 0,
                    checked: true
                },
                {
                    text: RMResx.RM_GS_Email_O365,
                    value: 1,
                    checked: false
                }
            ],
            emailSenderDefinition: {
                emailSenderType: 0,
                appProfileId: 0,
                emailSender: null
            },
            securityProfileList: [],
        };

        this.supportRecordsLabel = !LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver();

        bindEvents(this, "onSave", "onCancel", "hideMessageTip",
            "handleTimeZoneChange", "handleSessionTimeChange", "handleSessionTimeUnitChange",
            "handleDataFormatChange", "handleTimeFormatChange", "handleDatalightChange");
    }

    componentDidMount() {
        //$('#rmBody').css("background-color", "#ffffff");
        this.getSetting();
        this.changePageByResize();
        //RM.setBaseFontChangeCallback(this.changePageByResize);
    }

    componentWillUnmount() {
        //$('#rmBody').css("background-color", "#fafafa");
        //RM.clearBaseFontChangeCallback();
    }
    showMsgToast(content,type){
        let option = {
            content : content,
            classify : type
        };
        $$.toast(option);
    }
    changePageByResize() {
        var titleWidth = $("#rm_cp_session_title").width();
        var width = 550 - titleWidth - 200;
        $("#rm_cp_gs_unit").css("width", width);
    }

    getSetting() {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/GetGeneralSetting",
        };
        fetchUtility(option).then((res) => {
            if (res) {
                const clonedEmailSenderRadioItems = _.cloneDeep(this.state.emailSenderTypeRadioItems);
                clonedEmailSenderRadioItems.forEach(item => 
                    item.checked = (item.value === res.GeneralSettingModel.EmailSenderDefinition.EmailSenderType))
                this.setState({
                    sessionTimeUnits: setCheckedStatus(
                        "Key",
                        "Checked",
                        res.SessionTimeUnits,
                        { Key: res.GeneralSettingModel.SessionTimeUnitId }),
                    timeZones: setCheckedStatus(
                        "Id",
                        "Checked",
                        res.TimeZones,
                        { Id: res.GeneralSettingModel.TimeZoneId }),
                    dateFormats: setCheckedStatus(
                        "Key",
                        "Checked",
                        res.DateFormats,
                        { Key: res.GeneralSettingModel.DataFormatId }),
                    timeFormats: setCheckedStatus(
                        "Key",
                        "Checked",
                        res.TimeFormats,
                        { Key: res.GeneralSettingModel.TimeFormatId }),
                    gsModel: res.GeneralSettingModel,
                    SessionTime: res.GeneralSettingModel.SessionTime,
                    emailSenderTypeRadioItems: clonedEmailSenderRadioItems,
                    emailSenderDefinition: {
                        emailSenderType: res.GeneralSettingModel.EmailSenderDefinition.EmailSenderType,
                        appProfileId: res.GeneralSettingModel.EmailSenderDefinition.AppProfileId,
                        emailSender: res.GeneralSettingModel.EmailSenderDefinition.EmailSender,
                    },
                    recordsLabelInput: res.GeneralSettingModel.RecordsLabel ?? "",
                }, () => {
                    this.getSecurityProfileList(res.GeneralSettingModel.SecurityProfileId);
                });
            }

            fetchUtility({
                url: "/api/BCMCommonSettingApi/LoadAppProfiles"
            }).then(apps => {
                const appProfiles = apps.map(item => ({
                    key: item.Id,
                    value: item.Name,
                    checked: item.Id === res.GeneralSettingModel.EmailSenderDefinition.AppProfileId
                }));
                this.setState({
                    emailSenderAppProfileSelectorItems: appProfiles
                });
                $$.loading(false);
            }).catch(e => {
                $$.loading(false);    
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getSecurityProfileList(profileId) {
        let option = {
            url: "/api/StorageDevice/GetEncryptionProfileNames",
            method: "GET",
        };
        fetchUtility(option).then((res) => {
            let allSecurityProfiles = [
                this.getNoneItem(),
                ...res.SecurityProfiles,
            ];
            let foundChecked = false;
            allSecurityProfiles.forEach(item => {
                if (item.Id == profileId) {
                    foundChecked = true;
                }
                item.checked = item.Id == profileId;
            });
            if (!foundChecked) {
                let getGSModel = this.state.gsModel;
                allSecurityProfiles[0].checked = true;
                getGSModel.SecurityProfileId = allSecurityProfiles[0].Id;
                getGSModel.SecurityProfileName = allSecurityProfiles[0].Name;
                this.setState({
                    gsModel: getGSModel
                });
            }
            this.setState({ securityProfileList: allSecurityProfiles });
        }).catch((e) => {
        });
    }

    getNoneItem() {
        return {
            Id: "00000000-0000-0000-0000-000000000000",
            Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None
        };
    }

    onSave(e) {
        if (!LicenseHelper.HasDiscoveryLicenseOnly() && !LicenseHelper.EnableRecordsArchiver() && !$$.verify(this.securityProfileValidation)) {
            return false;
        }
        let isValid = true;
        const emailSenderDefinition = this.state.emailSenderDefinition;
        if(emailSenderDefinition.emailSenderType === EmailSenderType.O365)  {
            const definitionValid = {
                appProfileIdValid: {
                    valid: true,
                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                },
                emailSenderValid: {
                    valid: true,
                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                },
            };
            if(_.isNil(emailSenderDefinition.appProfileId) || _.isEmpty(emailSenderDefinition.appProfileId)) {
                definitionValid.appProfileIdValid =  {
                    valid: false,
                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                };
            }
    
            if(_.isNil(emailSenderDefinition.emailSender)) {
                definitionValid.emailSenderValid = {
                    valid: false,
                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                };
            }
    
            if(!definitionValid.appProfileIdValid.valid || !definitionValid.emailSenderValid.valid) {
                this.setState({
                    emailSenderDefinitionValidation: definitionValid
                });
                isValid = false;
            }
        }

        if (this.supportRecordsLabel && !this.handleValidateRecordsLabel()) {
            isValid = false;
        }

        if (!isValid) return;

        let data = this.getSaveData();

        $$.loading(true);
        fetchUtility({
            url: "/api/CPApi/CheckEmailSenderDefinition",
            data: data
        }).then(res => {
            if(!res) {
                $$.loading(false);
                const clonedValid = _.cloneDeep(this.state.emailSenderDefinitionValidation);
                clonedValid.appProfileIdValid.message = RMResx.RM_GS_Email_App_Permission_Message;
                clonedValid.appProfileIdValid.valid = false;
                this.setState({
                    emailSenderDefinitionValidation: clonedValid
                });
                return;    
            }

            $$.loading(false);
            $$.loading(true);
            if(data.SessionTime == ""){
                this.setState(
                    {
                        tipStatus: { show: true },
                        tipType: "error",
                        tipMsg: RMResx.RM_CP_GS_NoSessionTime
                    }
                );
                $$.loading(false);    
                return;
            }
            let option = {
                url: "/api/CPApi/SaveOrUpdateGeneralSetting",
                data: data
            };

            fetchUtility(option)
                .then((res) => {
                    // let tipOption = {
                    //     tipStatus: { show: true },
                    // };
                    if (res) {
                        // tipOption.tipType = "success";
                        // tipOption.tipMsg = RMResx.RM_JS_GS_SaveSuccess;
                        this.showMsgToast(RMResx.RM_JS_GS_SaveSuccess,"success",true);
                        RM.TimeSettingModel = res;
                        RM.TimeUtil.init();
                    } else {
                        // tipOption.tipType = "error";
                        // tipOption.tipMsg = RMResx.RM_JS_GS_SaveFailed;
                        this.showMsgToast(RMResx.RM_JS_GS_SaveFailed,"error",true);
                    }
                    // this.setState(tipOption);
                    setTimeout(function () {
                        $$.loading(false);
                    }, 300);
                })
                .catch((e) => {
                    $$.loading(false);     
                });
        }).catch(e => {
            $$.loading(false);
        });
    }

    onCancel(e) {
        this.props.history.push({
            pathname: RouterUrls.CP_Index
        });
    }

    getSaveData() {
        const res = this.state.gsModel;
        res.EmailSenderDefinition = {
            EmailSenderType: this.state.emailSenderDefinition.emailSenderType,
            AppProfileId: this.state.emailSenderDefinition.appProfileId,
            EmailSender: this.state.emailSenderDefinition.emailSender,
        }
        res.RecordsLabel = this.state.recordsLabelInput;

        return res;
    }

    getMaxSessionTimeoutValue() {
        if (this.state.gsModel.SessionTimeUnitId == 0) {
            return 35791394;    //hours
        } else {
            return 2147483647;  //minutes
        }
    }

    handleTimeZoneChange(args) {
        let val = args.newValue.Id;
        let gsModel = this.state.gsModel;
        gsModel.TimeZoneId = val;
        this.state.timeZones.forEach(function (value, key) {
            if (value.Id == val) {
                gsModel.isShowDayLight = value.SupportsDaylightSavingTime;
                gsModel.DayLight = true;
            }
        });
        this.setState({
            gsModel: gsModel
        });
    }
    handleSessionTimeChange(val) {
        let gsModel = this.state.gsModel;
        gsModel.SessionTime = val;
        this.setState({
            gsModel: gsModel,
            SessionTime: val
        });
    }
    handleSessionTimeUnitChange(args) {
        let val = args.newValue.Key;
        let gsModel = this.state.gsModel;
        gsModel.SessionTimeUnitId = val;
        this.setState({
            gsModel: gsModel
        });
    }
    handleDataFormatChange(args) {
        let val = args.newValue.Key;
        let gsModel = this.state.gsModel;
        gsModel.DataFormatId = val;
        this.setState({
            gsModel: gsModel
        });
    }
    handleTimeFormatChange(args) {
        let val = args.newValue.Key;
        let gsModel = this.state.gsModel;
        gsModel.TimeFormatId = val;
        this.setState({
            gsModel: gsModel
        });
    }

    handleDatalightChange(checked) {
        let gsModel = this.state.gsModel;
        gsModel.DayLight = checked;
        this.setState({
            gsModel: gsModel
        });
    }

    setSessionTimeoutErrorMsg(show, msg) {
        this.setState({ sessionTimeoutValidation: { show: show, msg: msg } });
    }

    hideMessageTip() {
        this.setState({
            tipStatus: { show: false }
        });
    }

    handleSecurityProfileChange = (args) => {
        let gsModel = this.state.gsModel;
        gsModel.SecurityProfileId = args.newValue.Id;
        gsModel.SecurityProfileName = args.newValue.Name;
        this.setState({
            gsModel: gsModel
        });
    }

    handleValidateRecordsLabel = () => {
        const INVALID_CHARS_REGEX = /[%\\&<>|?:;*,/\x00\x08\x0B\x0C\x0E-\x1F]/;
        const MAX_LENGTH = 64;
        const value = this.state.recordsLabelInput.trim();

        if (value.length > MAX_LENGTH || INVALID_CHARS_REGEX.test(value)) {
            this.setState({ invalidRecordsLabelInput: true });
            return false;
        }
        return true;
    };

    render() {
        return (
            <div id="raGeneralSetting">
                <$g.SiteMap
                    data={[SiteMapLinks.CP, SiteMapLinks.CP_GeneralSetting]}
                />
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    onClose={this.hideMessageTip}
                    status={{ show: this.state.tipStatus.show }}
                />
                <div className="ra-page-main">
                    {this.supportRecordsLabel && (
                        <>
                            <div className="ra-form-label">
                                <span className="ra-general-span" tabIndex="0">
                                    {RMResx.RM_GS_ConfigRecordLabel_Title}
                                </span>
                            </div>
                            <div className="ra-form-content">
                                <R.Input
                                    id="raMigrateDeclaredRecordsIpt"
                                    width="556px"
                                    value={this.state.recordsLabelInput}
                                    onChange={(value) => {
                                        this.setState({
                                            recordsLabelInput: value,
                                            invalidRecordsLabelInput: false,
                                        });
                                    }}
                                />
                                <$g.ValidationMsg
                                    show={this.state.invalidRecordsLabelInput}
                                >
                                    {RMResx.RM_GS_ConfigRecordLabel_ValidMsg}
                                </$g.ValidationMsg>
                            </div>
                        </>
                    )}
                    <div className="ra-form-label require">
                        <span className="ra-general-span" tabIndex="0">
                            {StringUtil.trimEndColon(RMResx.RM_GS_SessionTimeOut_Title)}
                        </span>
                    </div>
                    <div className="ra-form-content">
                        <div className="ra-inline-middle flex gap-s align-center ra-session-timeout">
                            <span
                                className="vertical-middle ra-session-timeout-title"
                                tabIndex="0"
                            >
                                {RMResx.RM_GS_SessionTimeOut_Context}
                            </span>
                            <div className="ra-session-timeout-unit flex gap-s justify-between">
                                <R.Input
                                    id="raCpGsSessionTimeNumIpt"
                                    type="number"
                                    hasControl
                                    min={1}
                                    max={this.getMaxSessionTimeoutValue()}
                                    value={this.state.SessionTime}
                                    onChange={this.handleSessionTimeChange}
                                    aria={{ariaLabel:RMResx.RM_GS_SessionTimeOut_Context}}
                                />
                                <R.Combobox
                                    id="raCpGsTimeout"
                                    searchable={false}
                                    valueField="Key"
                                    textField="Value"
                                    checkedField="Checked"
                                    excludeChecked
                                    items={this.state.sessionTimeUnits}
                                    onChange={this.handleSessionTimeUnitChange}
                                />
                            </div>                           
                        </div>
                        <$g.ValidationMsg
                            show={this.state.sessionTimeoutValidation.show}
                        >
                            {this.state.sessionTimeoutValidation.msg}
                        </$g.ValidationMsg>
                    </div>
                    <div className="ra-form-label require">
                        <span className="ra-general-span" tabIndex="0">{StringUtil.trimEndColon(RMResx.RM_GS_TimeZone_title)}</span>
                    </div>
                    <div className="ra-form-content">
                        <R.Combobox
                            id="raCpGsTimeZones"
                            width="556px"
                            valueField="Id"
                            textField="DisplayName"
                            checkedField="Checked"
                            items={this.state.timeZones}
                            onChange={this.handleTimeZoneChange}
                        />
                        {this.state.gsModel.isShowDayLight && (
                            <div className="margin-top-8">
                                <R.Checkbox
                                    id="raCpGsShowDayLightCheckbox"
                                    text={RMResx.RM_GS_SupportDaylight}
                                    title={RMResx.RM_GS_SupportDaylight}
                                    checked={this.state.gsModel.DayLight}
                                    onChange={this.handleDatalightChange}
                                />
                            </div>
                        )}
                    </div>
                    <div className="ra-form-label require">
                        <span className="ra-general-span" tabIndex="0">
                            {StringUtil.trimEndColon(RMResx.RM_GS_DateFormat_Title)}
                        </span>
                    </div>
                    <div className="ra-form-content">
                        <R.Combobox
                            id="raCpGsDateFormats"
                            width="556px"
                            valueField="Key"
                            textField="Value"
                            checkedField="Checked"
                            items={this.state.dateFormats}
                            onChange={this.handleDataFormatChange}
                        />
                    </div>
                    <div className="ra-form-label require">
                        <span className="ra-general-span" tabIndex="0">
                            {StringUtil.trimEndColon(RMResx.RM_GS_TimeFormat_Title)}
                        </span>
                    </div>
                    <div className="ra-form-content">
                        <R.Combobox
                            id="raCpGsTimeFormats"
                            width="556px"
                            searchable={false}
                            valueField="Key"
                            textField="Value"
                            checkedField="Checked"
                            items={this.state.timeFormats}
                            onChange={this.handleTimeFormatChange}
                        />
                    </div>
                    {!LicenseHelper.HasDiscoveryLicenseOnly() && !LicenseHelper.EnableRecordsArchiver() && !LicenseHelper.HasOpusGoogleLicenseOnly() && <div>
                        <div className="ra-form-label require">
                            <span className="ra-general-span" tabIndex="0">
                                {StringUtil.trimEndColon(RMResx.RM_CP_GSS_SecurityProfile)}
                            </span>
                        </div>
                        <R.Validation>
                            <div ref={r => this.securityProfileValidation = r} className="ra-form-content">
                                <R.Validation
                                    element="Combobox"
                                    require={RMResx.RM_AR_CP_Common_SelEmpty}
                                >
                                    <R.Combobox
                                        id="raSecurityProfile"
                                        width="556px"
                                        searchable={false}
                                        textField="Name"
                                        valueField="Id"
                                        checkedField="checked"
                                        items={this.state.securityProfileList}
                                        onChange={this.handleSecurityProfileChange}
                                    />
                                </R.Validation>
                            </div>
                        </R.Validation>
                    </div>}
                    <div className="ra-form-label">
                        <span className="ra-general-span">
                            {RMResx.RM_GS_Email_Notification_Sender}
                        </span>
                    </div>
                    <div className="ra-form-content">
                        <R.Radio.Group
                            name="email-sender-type"
                            items={this.state.emailSenderTypeRadioItems}
                            block={true}
                            onChange={(value, oldValue) => {
                                const clonedEmailSenderDefinition = _.cloneDeep(this.state.emailSenderDefinition);
                                clonedEmailSenderDefinition.emailSenderType = value;

                                const clonedEmailSenderValid = _.cloneDeep(this.state.emailSenderDefinitionValidation);
                                clonedEmailSenderValid.appProfileIdValid = {
                                    valid: true,
                                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                                };
                                clonedEmailSenderValid.emailSenderValid = {
                                    valid: true,
                                    message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                                };

                                this.setState({
                                    emailSenderDefinition: clonedEmailSenderDefinition,
                                    emailSenderDefinitionValidation: clonedEmailSenderValid
                                })
                            }}
                        />
                        {
                            this.state.emailSenderDefinition.emailSenderType === EmailSenderType.O365 &&
                            <div className="ra-email-365-setting">
                                <div className="ra-email-365-setting-label">
                                    {RMResx.RM_GS_Email_App_Profile}
                                </div>
                                <div className="ra-email-365-setting-content">
                                    <R.Combobox
                                        id="email-setting-app"
                                        width="526px"
                                        searchable={false}
                                        valueField="key"
                                        textField="value"
                                        checkedField="checked"
                                        items={this.state.emailSenderAppProfileSelectorItems}
                                        onChange={(args) => {
                                            const clonedEmailSenderDefinition = _.cloneDeep(this.state.emailSenderDefinition);
                                            clonedEmailSenderDefinition.appProfileId = args.newValue.key;
                                            clonedEmailSenderDefinition.emailSender = null;

                                            const clonedEmailSenderValid = _.cloneDeep(this.state.emailSenderDefinitionValidation);
                                            clonedEmailSenderValid.appProfileIdValid = {
                                                valid: true,
                                                message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                                            };

                                            const clonedSelectorItems = _.cloneDeep(this.state.emailSenderAppProfileSelectorItems);
                                            clonedSelectorItems.forEach(item => item.checked = item.key === args.newValue.key);

                                            this.setState({
                                                emailSenderDefinition: clonedEmailSenderDefinition,
                                                emailSenderDefinitionValidation: clonedEmailSenderValid,
                                                emailSenderAppProfileSelectorItems: clonedSelectorItems,
                                            })
                                        }}
                                    />
                                    <$g.ValidationMsg
                                        show={!this.state.emailSenderDefinitionValidation.appProfileIdValid.valid}
                                    >
                                        {this.state.emailSenderDefinitionValidation.appProfileIdValid.message}
                                    </$g.ValidationMsg>
                                </div>
                                <div className="ra-email-365-setting-label">
                                    {RMResx.RM_GS_Email_Sender}
                                </div>
                                <div className="ra-email-365-setting-content">
                                    <PeoplePicker
                                        height="auto"
                                        width="526px"
                                        disabled={_.isNil(this.state.emailSenderDefinition.appProfileId) || _.isEmpty(this.state.emailSenderDefinition.appProfileId)}
                                        items={_.isNil(this.state.emailSenderDefinition.emailSender) ? [] : [this.state.emailSenderDefinition.emailSender]}
                                        singleMode={true}
                                        specifyAppProfile={true}
                                        getSpecifyAppProfileId={() => this.state.emailSenderDefinition.appProfileId}
                                        selectionChanged={(value) => {
                                            const clonedEmailSenderDefinition = _.cloneDeep(this.state.emailSenderDefinition);
                                            clonedEmailSenderDefinition.emailSender = value[0];

                                            const clonedEmailSenderValid = _.cloneDeep(this.state.emailSenderDefinitionValidation);
                                            clonedEmailSenderValid.emailSenderValid = {
                                                valid: true,
                                                message: RMResx.RM_PRM_PRE_ColumnValid_RequireText
                                            };

                                            this.setState({
                                                emailSenderDefinition: clonedEmailSenderDefinition,
                                                emailSenderDefinitionValidation: clonedEmailSenderValid
                                            })
                                        }}
                                    />
                                    <$g.ValidationMsg
                                        show={!this.state.emailSenderDefinitionValidation.emailSenderValid.valid}
                                    >
                                        {this.state.emailSenderDefinitionValidation.emailSenderValid.message}
                                    </$g.ValidationMsg>
                                </div>
                            </div>
                        }
                    </div>

                    <div className="ra-foot-btns flex justify-end align-center gap-s">
                        <R.Button
                            text={RMResx.RM_JS_Common_Cancel}
                            onClick={this.onCancel}
                        />
                        <R.Button
                            id="raCpGsSaveBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            onClick={this.onSave}
                        />
                    </div>
                </div>
            </div>
        );
    }
}
