import StringUtil from "../../../../Utilities/StringUtil";
import "../../../../Less/BCM/ContentRepositoryManagement/uniqueIdSetting.less";
import { showToast } from "../../../../Utilities/CommonUtil";

export default class UniqueIdSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            uniqueIdData: {},
        };
    }

    componentInit() {
        this.loadUniqueIdSetting();
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
        let getUniqueIdData = this.state.uniqueIdData;
        if ((this.oldSettingPrefix == null) || (this.oldSettingPrefix == "") || (this.oldSettingPrefix == getUniqueIdData.Prefix)) {
            this.onFirstSave(callback);
        } else if (this.oldSettingPrefix != getUniqueIdData.Prefix) {
            this.prefixMessageBox(callback);
        }
    }

    onFirstSave(callback) {
        $$.messagedialog(false);
        $$.loading(true);
        let getUniqueIdData = this.state.uniqueIdData;
        getUniqueIdData.IsActived = true;
        getUniqueIdData.SourceFlag = this.props.sourceFlag;
        if (!this.props.supportCustomColumn && (getUniqueIdData.Name == null || getUniqueIdData.Name == "")) {
            getUniqueIdData.Name = RMResx.RM_JS_SP_UniqueSetting_SPOnPremColumnName;
        }
        let option = {
            url: '/API/BCMAdminSettingApi/UpdateUniqueIdSetting',
            method: "Post",
            data: getUniqueIdData
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                if(result.MessageType == 0){
                    callback(true, getUniqueIdData);
                    showToast.success(RMResx.RM_JS_SP_UniqueSetting_Save_Success);
                }
                else{
                    if(result.ErrorMessage){
                        showToast.error(result.ErrorMessage);
                    }
                    else{
                        showToast.error(RMResx.RM_JS_SP_UniqueSetting_SaveFailed);
                    }
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    prefixMessageBox(callback) {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_SP_UniqueIdSetting_PrefixChangeDes,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.onFirstSave.bind(this, callback)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    loadUniqueIdSetting() {
        let option = {
            url: "/API/BCMAdminSettingApi/LoadingUniqueIdSetting",
            method: "Post",
            data: {
                SourceFlag: this.props.sourceFlag,
            }
        };
        fetchUtility(option).then((res) => {
            if (res) {
                this.oldSettingPrefix = res.Prefix;
            }
            this.setState({
                uniqueIdData: res
            });
        }).catch((e) => {
        });
    }

    onColumnNameChanged(value) {
        this.state.uniqueIdData.Name = value;
        this.setState({ uniqueIdData: RM.deepcopy(this.state.uniqueIdData) });
    }

    onPrefixChanged(value) {
        this.state.uniqueIdData.Prefix = value;
        this.setState({ uniqueIdData: RM.deepcopy(this.state.uniqueIdData) });
    }

    onOverwriteSPChanged(checked) {
        this.state.uniqueIdData.OverrrideSPPrefix = checked;
        this.setState({ uniqueIdData: RM.deepcopy(this.state.uniqueIdData) });
    }

    verifyPrefix(value) {
        const regex = /^[A-Za-z0-9]{4,12}$/;
        if (!regex.test(value)) {
            return RMResx.RM_JS_SP_UniqueId_ErrorMsg;
        }
        return true;
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    {this.props.context.showMessagebar && <div className="ra-crm-form-content">
                        <R.Messagebar
                            message={RMResx.RM_JS_FS_UniqueIdNote}
                            classify="info"
                            hasClose={false}
                            status={{ show: true }}
                        />
                    </div>}
                    {this.props.context.showColumnName && <div className="ra-crm-form-content">
                        <div className="require ra-uniqueIdPanel-title" tabIndex="0">
                            <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SP_UniqueIdColumnName)} />
                        </div>
                        <$g.Popover>{this.props.context.columnNameDes}</$g.Popover>
                        {this.props.supportCustomColumn && <R.Validation
                            element="Input"
                            require={RMResx.RM_JS_SP_UniqueIdSetting_Require} >
                            <R.Input
                                id="raCrmUniqueIdColumnNameIpt"
                                type="text"
                                value={this.state.uniqueIdData.Name}
                                onChange={this.onColumnNameChanged.bind(this)}
                                aria={{ariaLabel:RMResx.RM_JS_SP_UniqueIdColumnName}}
                            />
                        </R.Validation>}
                        {!this.props.supportCustomColumn && <R.Input
                            id="raCrmUniqueIdColumnNameIpt"
                            type="text"
                            value={RMResx.RM_JS_SP_UniqueSetting_SPOnPremColumnName}
                            disabled={true}
                        />}
                    </div>}
                    <div className="ra-crm-form-content">
                        <div className="require ra-uniqueIdPanel-title" tabIndex="0">
                            <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SP_IdFomate_Prefix)} />
                        </div>
                        <R.Validation
                            element="Input"
                            rules={{ verifyPrefix: this.verifyPrefix }} >
                            <R.Input
                                id="raCrmUniqueIdPrefixIpt"
                                type="text"
                                value={this.state.uniqueIdData.Prefix}
                                onChange={this.onPrefixChanged.bind(this)}
                                aria={{ariaLabel:RMResx.RM_JS_SP_IdFomate_Prefix}}
                            />
                        </R.Validation>
                    </div>
                    <div className="ra-setting-panel-checkbox">
                        <R.Checkbox
                            id="raCrmOverrideSPIDPrefix"
                            text={this.props.context.checkboxText}
                            title={this.props.context.checkboxText}
                            checked={this.state.uniqueIdData.OverrrideSPPrefix}
                            onChange={this.onOverwriteSPChanged.bind(this)}
                        />
                        {this.props.context.showPopover && <$g.Popover>{RMResx.RM_JS_FS_StoreDocumentId_Description}</$g.Popover>}
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}