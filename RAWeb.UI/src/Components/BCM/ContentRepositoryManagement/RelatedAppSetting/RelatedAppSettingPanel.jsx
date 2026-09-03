import { showToast } from "../../../../Utilities/CommonUtil";

const DEFAULT_SETTING_STATE = {
    isLink: false,
    clientId: null,
    thumbPrint: null
};

export default class RelatedAppSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            relatedAppSetting: RM.deepcopy(DEFAULT_SETTING_STATE),
            radioOptionGroup: this.getRadioOptionGroup(false)
        };
    }

    getRadioOptionGroup(isLinked) {
        return [
            { text: RMResx.RM_JS_Common_Yes, value: true, checked: isLinked },
            { text: RMResx.RM_JS_Common_No, value: false, checked: !isLinked },
        ];
    }

    componentInit() {
        this.loadSetting();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        
        this.onSaveSetting(callback);
    }

    onSaveSetting(callback) {
        const setting = RM.deepcopy(this.state.relatedAppSetting);
        let option = {
            url: '/API/SPOnPremSettingApi/SaveApiAuth',
            method: "Post",
            data: {
                IsLink: setting.isLink,
                ClientId: setting.isLink ? setting.clientId : null,
                ThumbPrint: setting.isLink ? setting.thumbPrint : null
            }
        };
        $$.loading(true);
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                callback(true);
                const res = JSON.parse(result);
                if (res.MessageType === 0) {
                    showToast.success(RMResx.RM_JS_LSP_RelatedRecordsAppSetting_Saved);
                } else {
                    showToast.error(res.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    loadSetting() {
        $$.loading(true);
        let option = {
            url: "/API/SPOnPremSettingApi/GetApiAuthInfo",
            method: "GET"
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res) {
                const setting = JSON.parse(res);
                this.setState({
                    relatedAppSetting: setting ?? {},
                    radioOptionGroup: this.getRadioOptionGroup(setting?.isLink)
                });
            }
        }).catch((e) => {
            $$.loading(false);
            console.error("Failed to load related app setting:", e);
        });
    }

    onLinkRelatedAppOptionGroupChanged(args) {
        const relatedAppSetting = RM.deepcopy(this.state.relatedAppSetting);
        relatedAppSetting.isLink = args;
        this.setState({ relatedAppSetting });
    }

    onClientIdChanged(value) {
        const relatedAppSetting = RM.deepcopy(this.state.relatedAppSetting);
        relatedAppSetting.clientId = value;
        this.setState({ relatedAppSetting });
    }

    onCertificateThumbprintChanged(value) {
        const relatedAppSetting = RM.deepcopy(this.state.relatedAppSetting);
        relatedAppSetting.thumbPrint = value;
        this.setState({ relatedAppSetting });
    }

    verifyClientId(value) {
        const regex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/;
        if (!value) {
            return RMResx.RM_JS_LSP_ClientID_Validation_Required;
        }
        if (!regex.test(value)) {
            return RMResx.RM_JS_LSP_ClientID_Validation_Invalid;
        }
        return true;
    }

    verifyCertificateThumbprint(value) {
        const regex = /^[0-9a-fA-F]{40}$/;
        if (!value) {
            return RMResx.RM_JS_LSP_CertificateThumbprint_Validation_Required;
        }
        if (!regex.test(value)) {
            return RMResx.RM_JS_LSP_CertificateThumbprint_Validation_Invalid;
        }
        return true;
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-crm-form-content">
                        <div
                            id="linkPhysicalRecordInSPOPRelatedApp"
                            className="ra-setting-panel-title"
                        >
                            {RMResx.RM_JS_LSP_LinkRecordsApp_Title}
                        </div>
                        <R.Radio.Group
                            aria={{ "aria-labelledby": "linkPhysicalRecordInSPOPRelatedApp" }}
                            name="isLinkRelatedAppOptionGroup"
                            items={this.state.radioOptionGroup}
                            onChange={this.onLinkRelatedAppOptionGroupChanged.bind(this)}
                        />
                    </div>

                    {this.state.relatedAppSetting?.isLink && (
                        <>
                            <div className="ra-crm-form-content">
                                <div className="require ra-input-field-title" tabIndex="0">
                                    {RMResx.RM_JS_LSP_ClientID_Title}
                                </div>
                                <$g.Popover>{RMResx.RM_JS_LSP_ClientID_Desc}</$g.Popover>
                                <R.Validation
                                    element="Input"
                                    rules={{ verifyClientId: this.verifyClientId }}
                                >
                                    <R.Input
                                        id="raCrmApplicationIdIpt"
                                        type="text"
                                        value={this.state.relatedAppSetting.clientId}
                                        onChange={this.onClientIdChanged.bind(this)}
                                        aria={{ariaLabel: RMResx.RM_JS_LSP_ClientID_Title}}
                                    />
                                </R.Validation>
                            </div>

                            <div className="ra-crm-form-content">
                                <div className="require ra-input-field-title" tabIndex="0">
                                    {RMResx.RM_JS_LSP_CertificateThumbprint_Title}
                                </div>
                                <$g.Popover>{RMResx.RM_JS_LSP_CertificateThumbprint_Desc}</$g.Popover>
                                <R.Validation
                                    element="Input"
                                    rules={{ verifyCertificateThumbprint: this.verifyCertificateThumbprint }}
                                >
                                    <R.Input
                                        id="raCrmCertificateThumbprintIpt"
                                        type="text"
                                        value={this.state.relatedAppSetting.thumbPrint}
                                        onChange={this.onCertificateThumbprintChanged.bind(this)}
                                        aria={{ariaLabel: RMResx.RM_JS_LSP_CertificateThumbprint_Title}}
                                    />
                                </R.Validation>
                            </div>
                        </>
                    )}
                </div>
            </R.Validation>
        </div>;
    }
}