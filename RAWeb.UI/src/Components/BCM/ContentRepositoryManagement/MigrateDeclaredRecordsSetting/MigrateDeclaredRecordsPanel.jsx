import { showToast } from "../../../../Utilities/CommonUtil";
import { MessageType } from "../../../CP/CPConstants";

class MigrateDeclaredRecordsPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            showMessageBarInfo: true,
            recordsLabelInput: "",
        };
        this.source = this.props.source;
    }

    componentInit() {
        this.loadDeclaredRecords();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onValidate":
                this.onValidate(args);
                break;
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    loadDeclaredRecords() {
        $$.loading(true);
        const options = {
            url: "/api/CPApi/GetGeneralSetting",
        };
        fetchUtility(options)
            .then((res) => {
                if (res) {
                    const generalSettingModel = res.GeneralSettingModel;
                    this.setState({
                        recordsLabelInput:
                            generalSettingModel.RecordsLabel ?? "",
                    });
                }
            })
            .finally(() => $$.loading(false));
    }

    onValidate(callback) {
        if (!$$.verify("#allValidation")) {
            return false;
        }
        callback(true);
    }

    onSave(callback) {
        $$.loading(true);
        const options = {
            url: "/api/SPSettingApi/RunDeclaredRecordsMigrationJob",
            method: "POST",
            data: {
                RecordsLabel: this.state.recordsLabelInput,
                NodeSetting: this.props.treeData,
            },
        };
        fetchUtility(options)
            .then((res) => {
                if (res) {
                    if (res.MessageType == MessageType.Successful) {
                        callback(true);
                        const content = (
                            <$g.I18NProvider
                                msg={RMResx.RM_JS_SPS_RunJobSucceed}
                            >
                                <a className="ra-link-a" href="/Root/JM/Index">
                                    {RMResx.RM_JS_JM_Title}
                                </a>
                            </$g.I18NProvider>
                        );
                        showToast.success(content);
                    } else {
                        showToast.error(res.ErrorMessage);
                    }
                }
            })
            .finally(() => $$.loading(false));
    }

    customVerify = (value) => {
        const INVALID_CHARS_REGEX = /[%\\&<>|?:;*,/\x00\x08\x0B\x0C\x0E-\x1F]/;
        const MAX_LENGTH = 64;

        value = value.trim();
        if (value.length > MAX_LENGTH || INVALID_CHARS_REGEX.test(value)) {
            return RMResx.RM_GS_ConfigRecordLabel_ValidMsg;
        }
        
        return true;
    };

    render() {
        return (
            <div id={this.props.id}>
                <R.Validation>
                    <div id="allValidation">
                        <div
                            className="margin-bottom-l"
                            hidden={!this.state.showMessageBarInfo}
                        >
                            <R.Messagebar
                                classify="info"
                                message={
                                    RMResx.RM_JS_SP_MigrateDeclaredRecords_MsgBar
                                }
                                status={{ show: this.state.showMessageBarInfo }}
                                hasClose
                                onClose={() =>
                                    this.setState({ showMessageBarInfo: false })
                                }
                            />
                        </div>
                        <div tabIndex={0} className="margin-bottom-xs require">
                            {RMResx.RM_JS_SP_MigrateDeclaredRecords_RecordLabel}
                        </div>
                        <R.Validation
                            element="Input"
                            require
                            rules={{
                                customVerify: this.customVerify,
                            }}
                        >
                            <R.Input
                                id="raMigrateDeclaredRecordsIpt"
                                value={this.state.recordsLabelInput}
                                onChange={(value) => {
                                    this.setState({
                                        recordsLabelInput: value,
                                    });
                                }}
                            />
                        </R.Validation>
                        <div style={{ padding: 0 }} tabIndex={0} className="margin-top-xs ra-main-pager-counter">
                            <$g.I18NProvider
                                msg={
                                    RMResx.RM_JS_SP_MigrateDeclaredRecords_RecordLabel_Desc
                                }
                            >
                                <a
                                    className="ra-link-a"
                                    href="/Root/CP/GeneralSetting"
                                >
                                    {
                                        RMResx.RM_JS_SP_MigrateDeclaredRecords_GeneralSetting
                                    }
                                </a>
                            </$g.I18NProvider>
                        </div>
                    </div>
                </R.Validation>
            </div>
        );
    }
}

export default MigrateDeclaredRecordsPanel;
