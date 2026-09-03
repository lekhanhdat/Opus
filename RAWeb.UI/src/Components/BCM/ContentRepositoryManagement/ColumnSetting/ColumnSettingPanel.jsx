import "../../../../Less/BCM/ContentRepositoryManagement/columnSetting.less";
import { getRequestVerificationToken, showToast } from "../../../../Utilities/CommonUtil";
import StringUtil from "../../../../Utilities/StringUtil";
import Enviroments from "../../../../Constants/Enviroments";

const KeepSPDefaultValueOption = {
    Yes: "1",
    No: "0"
};

export default class ColumnSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            radioColumn: [
                { text: RMResx.RM_JS_SPS_UseExistingColDesc, value: true, checked: this.props.data.IsUsingExistColumnName },
                { text: RMResx.RM_JS_SPS_CreateNewColDesc, value: false, checked: !this.props.data.IsUsingExistColumnName },
            ],
            radioTermSettings: [
                { text: RMResx.RM_JS_SPS_UseTermSettingsDefinedInSP, value: false, checked: !this.props.data.SetDocLevelTermForExistColumn },
                { text: RMResx.RM_JS_SPS_UseTermSettingsDefinedInRecords, value: true, checked: this.props.data.SetDocLevelTermForExistColumn },
            ],
            radioSPUniqueIsShow: [
                { text: RMResx.RM_JS_SPS_UniqueIsShow_OptionYes, value: true, checked: this.props.data.IsShowUniqueId },
                { text: RMResx.RM_JS_SPS_UniqueIsShow_OptionNo, value: false, checked: !this.props.data.IsShowUniqueId },   
            ],
            radioSPOnPremUniqueIsShow: [
                { text: RMResx.RM_JS_SPS_UniqueIsShow_OptionYes_OnlyDocument, value: true, checked: this.props.data.IsShowUniqueId },
                { text: RMResx.RM_JS_SPS_UniqueIsShow_OptionNo, value: false, checked: !this.props.data.IsShowUniqueId },
            ],
            radioHiddenColumnSettings: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.ColumnHidden },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.ColumnHidden },
            ],
            radioColumnRequired: [
                { text: RMResx.RM_JS_Common_Yes, value: true, checked: this.props.data.ColumnRequired },
                { text: RMResx.RM_JS_Common_No, value: false, checked: !this.props.data.ColumnRequired },
            ],
            descriptionTextarea: this.props.data.Description,
            existedColumnName: this.props.data.ExistColumnName,
            createdColumnName: this.props.isCSDTenant ? "CSD Class" : this.props.data.ColumnName,
            isCheckedExistedCol: this.props.data.IsUsingExistColumnName,
            enableRelatedRecords: this.props.data.EnableRelatedRecords,
            setHiddenColumn: this.props.data.ColumnHidden,
            columnRequired: this.props.data.ColumnRequired,
            setDocLevelTermForExistColumn: this.props.data.SetDocLevelTermForExistColumn,
            isShowUniqueId: this.props.data.IsShowUniqueId,
            isKeepSharePointDefaultValue: this.props.data.IsKeepSharePointDefaultValue,
            setTermForEmptyDefaultValue: this.props.data.SetTermForEmptyDefaultValue,
            uniqueIdData: {},
            disableColumnRequired: this.props.data.ColumnHidden,
        };
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    componentInit() {
        this.loadUniqueIdSetting();
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
            this.setState({
                uniqueIdData: res
            });
        }).catch((e) => {
        });
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        let settingNode = this.props.data;
        RM.deepcopy(this.props.data);
        if (this.state.isCheckedExistedCol == true) {
            if (this.state.existedColumnName == "") {
                return false;
            } else {
                settingNode.IsUsingExistColumnName = this.state.isCheckedExistedCol;
                settingNode.SetDocLevelTermForExistColumn = this.state.setDocLevelTermForExistColumn;
                settingNode.ExistColumnName = this.state.existedColumnName;
                settingNode.IsShowUniqueId = this.state.isShowUniqueId;
                settingNode.IsKeepSharePointDefaultValue = this.state.isKeepSharePointDefaultValue;
                settingNode.SetTermForEmptyDefaultValue = this.state.setTermForEmptyDefaultValue;
                settingNode.EnableRelatedRecords = this.state.enableRelatedRecords;
                let option = {
                    url: this.props.context.saveExistColumnData,
                    method: "Post",
                    data: settingNode
                };
                fetchUtility(option).then((result) => {
                    $$.loading(false);
                    if (result) {
                        callback(true, settingNode);
                        showToast.success(RMResx.RM_JS_BCM_SaveSettingsSuccess);
                    }
                }).catch((e) => {
                    $$.loading(false);
                });
            }
        } else {
            if (this.state.createdColumnName == "") {
                return false;
            } else {
                settingNode.IsUsingExistColumnName = false;
                settingNode.ColumnName = this.state.createdColumnName;
                settingNode.ColumnHidden = this.state.setHiddenColumn;
                settingNode.ColumnRequired = this.state.columnRequired;
                settingNode.Description = this.state.descriptionTextarea;
                settingNode.IsShowUniqueId = this.state.isShowUniqueId;
                settingNode.IsKeepSharePointDefaultValue = this.state.isKeepSharePointDefaultValue;
                settingNode.SetTermForEmptyDefaultValue = this.state.setTermForEmptyDefaultValue;
                settingNode.EnableRelatedRecords = this.state.enableRelatedRecords;
                let option = {
                    url: this.props.context.saveCreateColumnData,
                    method: "Post",
                    data: settingNode
                };
                fetchUtility(option).then((result) => {
                    $$.loading(false);
                    if (result) {
                        callback(true, settingNode);
                        showToast.success(RMResx.RM_JS_BCM_SaveSettingsSuccess);
                    }
                }).catch((e) => {
                    $$.loading(false);
                });
            }
        }
    }

    cancel() {
        return true;
    }

    handleColumnChanged = (args) => {
        this.setState({ isCheckedExistedCol: args });
        // this.useExistingColumn = args;
    }

    handleTermSettingsChanged = (args) => {
        this.setState({ setDocLevelTermForExistColumn: args });
    }

    handleInputColNameChanged = (column, value) => {
        if (column == "exist") {
            this.setState({ existedColumnName: value, isSaving: false });
        } else if (column == "create") {
            this.setState({ createdColumnName: value, isSaving: false });
        }
        this.setState({ isSaving: true });
    }

    handleUniqueChanged = (args) => {
        this.setState({ isShowUniqueId: args }, () => { $$.verify(this.refUniqueIdValid.ref.current); });
    }

    handleRelatedRecordsChanged = (args) => {
        this.setState({ enableRelatedRecords: args });
    }

    handleDescriptionChange = (args) => {
        this.setState({ descriptionTextarea: args });
    }

    handleHiddenColumnChanged = (args) => {
        let cloneRadioColumnRequired = RM.deepcopy(this.state.radioColumnRequired);
        let radios = cloneRadioColumnRequired.map(columnRadio => {
            columnRadio.checked = args ? columnRadio.value === false : columnRadio.value === true;
            return columnRadio;
        });
        this.setState({
            radioColumnRequired: radios,
            columnRequired: !args,
            disableColumnRequired: args,
            setHiddenColumn: args,
        });
    }

    handleColumnRequiredChanged = (args) => {
        this.setState({ columnRequired: args });
    }

    handleDownloadRelatedApp = (e) => {
        let downloadUniqueId = StringUtil.newGuid();
        var $downloadStatusKey = $("#downloadFlag");
        $downloadStatusKey.val(downloadUniqueId);

        $("#crm-form-download")
            .attr("action", this.props.context.downloadRelatedApp)
            .submit();
    }

    handleKeepSPDefaultValueChanged = (value) => {
        let keepSPDefaultValue = value == KeepSPDefaultValueOption.Yes;
        if(keepSPDefaultValue)
        {
            this.setState({ isKeepSharePointDefaultValue: keepSPDefaultValue} );
        }
        else
        {
            this.setState({ isKeepSharePointDefaultValue: keepSPDefaultValue, setTermForEmptyDefaultValue : false} );
        }
    }

    handleUseDefaultTermIfSPIsEmptyChanged = (args) => {
        this.setState({ setTermForEmptyDefaultValue: args} );
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    displayUniqueIdValid = () => {
        return this.state.isShowUniqueId && this.state.uniqueIdData.Id == 0 && !this.state.uniqueIdData.IsActived ? RMResx.RM_JS_SPS_UniqueIdDisplay_ErrorMsg : true;
    }

    render() {
        let requestVerificationToken = getRequestVerificationToken();
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div>
                        <div className="ra-crm-form-content">
                            <div className="require ra-setting-panel-title">
                                <span id="ariaRadioColumn"><$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SPS_ChooseColumnDesc)} /></span>
                            </div>
                            <R.Radio.Group
                                block={true}
                                name="radioColumnGroup"
                                items={this.state.radioColumn}
                                onChange={this.handleColumnChanged}
                                aria="#ariaRadioColumn"
                            />
                        </div>
                        {this.state.isCheckedExistedCol && <div>
                            <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">
                                    <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EnterColNameDesc)} />
                                </div>
                                <R.Validation
                                    element="Input"
                                    require={RMResx.RM_Common_FillOut} >
                                    <R.Input
                                        id="raCrmCsExistedColumnNameIpt"
                                        type="text"
                                        value={this.state.existedColumnName}
                                        onChange={this.handleInputColNameChanged.bind(this, "exist")}
                                        aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_JS_SPS_EnterColNameDesc) }}
                                    />
                                </R.Validation>
                            </div>

                            <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">
                                    <span id="ariaTermSetting">{RMResx.RM_JS_SPS_TermSettings}</span>
                                </div>
                                <R.Radio.Group
                                    block={true}
                                    name="radioTermSettingsGroup"
                                    items={this.state.radioTermSettings}
                                    onChange={this.handleTermSettingsChanged}
                                    aria="#ariaTermSetting"
                                />
                            </div>
                        </div>}
                        {!this.state.isCheckedExistedCol && <div>
                            <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">
                                    <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SPS_EnterColNameDesc)} />
                                </div>
                                <R.Validation
                                    element="Input"
                                    require={RMResx.RM_Common_FillOut} >
                                    <R.Input
                                        id="raCrmCsCreatedColumnNameIpt"
                                        disabled={this.props.isCSDTenant}
                                        type="text"
                                        value={this.state.createdColumnName}
                                        onChange={this.handleInputColNameChanged.bind(this, "create")}
                                        aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_JS_SPS_EnterColNameDesc) }}
                                    />
                                </R.Validation>
                            </div>
                            <div className="ra-crm-form-content">
                                <div className="ra-setting-panel-title">
                                    {RMResx.RM_JS_SPS_EditKey_ColumnNameDescription}
                                </div>
                                <R.Input
                                    id="raCrmCsDescriptionIpt"
                                    type="textarea"
                                    className="resizable"
                                    value={this.state.descriptionTextarea}
                                    onChange={this.handleDescriptionChange}
                                    aria={{ ariaLabel: StringUtil.trimEndColon(RMResx.RM_JS_SPS_EditKey_ColumnNameDescription) }}
                                />
                            </div>
                            {!this.props.isCSDTenant && this.props.context.configurations.enableHiddenColumn && <div className="ra-crm-form-content">
                                <div className="ra-setting-panel-title">
                                    <span id="ariaHiddenColumn" className="require">{this.props.context.configurations.hiddenColumnTitle}</span>
                                    <$g.Popover>{this.props.context.configurations.hiddenColumnMsg}</$g.Popover>
                                </div>
                                <R.Radio.Group
                                    block={true}
                                    name="radioHiddenColumnGroup"
                                    items={this.state.radioHiddenColumnSettings}
                                    onChange={this.handleHiddenColumnChanged}
                                    aria="#ariaHiddenColumn"
                                />
                            </div>}
                            <div className="ra-crm-form-content">
                                <div className="require ra-setting-panel-title">
                                    <span id="ariaColumnRequired"><$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_SPS_ColumnRequired)} /></span>
                                </div>
                                <R.Radio.Group
                                    block={true}
                                    name="radioColumnRequiredGroup"
                                    disabled={this.state.disableColumnRequired}
                                    items={this.state.radioColumnRequired}
                                    onChange={this.handleColumnRequiredChanged}
                                    aria="#ariaColumnRequired"
                                />
                            </div>
                        </div>}
                    </div>
                    <div className="ra-crm-form-content">
                        <div id="ariaUniqueId" className="ra-setting-panel-title">
                            {this.props.context.configurations.uniqueIdTitle}
                        </div>
                        {this.props.context.configurations.SPDisplayUniqueId && <R.Radio.Group
                            block={true}
                            name="radioSPUniqueIsShow"
                            items={this.state.radioSPUniqueIsShow}
                            onChange={this.handleUniqueChanged}
                            aria="#ariaUniqueId"
                        />}
                        {!this.props.context.configurations.SPDisplayUniqueId && <R.Radio.Group
                            block={true}
                            name="radioSPOnPremUniqueIsShow"
                            items={this.state.radioSPOnPremUniqueIsShow}
                            onChange={this.handleUniqueChanged}
                            aria="#ariaUniqueId"
                        />}
                        
                        <div className="margin-top-s margin-left-l">
                            <R.ValidationFaker valid={this.displayUniqueIdValid} ref={r => this.refUniqueIdValid = r} />
                        </div>
                    </div>
                    {!this.props.isCSDTenant && ((this.state.isCheckedExistedCol && this.state.setDocLevelTermForExistColumn) || !this.state.isCheckedExistedCol) 
                        && this.props.context.configurations.enableKeepSharePointDefaultValue 
                        && <div className="ra-crm-form-content">
                            <div id="ariaKeepDefaultValue" className="ra-setting-panel-title">
                                {this.props.context.configurations.defaultTermTitle}
                            </div>
                            <$g.RadioGroup
                                name="radioSPKeepDefaultValue"
                                value={this.state.isKeepSharePointDefaultValue ? KeepSPDefaultValueOption.Yes : KeepSPDefaultValueOption.No}
                                onChange={this.handleKeepSPDefaultValueChanged}>
                                <$g.RadioOption value={KeepSPDefaultValueOption.Yes} text={this.props.context.configurations.defaultTermYesText} isFlex={!this.state.isKeepSharePointDefaultValue}>
                                    {
                                        this.state.isKeepSharePointDefaultValue == true &&
                                            <div className="ra-setting-panel-checkbox ra-setting-checkbox-wrapper">
                                                <R.Checkbox
                                                    id="raCrmSetTermForEmptyDefaultValue"
                                                    text={this.props.context.configurations.defaultTermCheckboxText}
                                                    title={this.props.context.configurations.defaultTermCheckboxText}
                                                    checked={this.state.setTermForEmptyDefaultValue}
                                                    onChange={this.handleUseDefaultTermIfSPIsEmptyChanged}
                                                />
                                            </div>
                                    }
                                    <div style={{ marginTop: this.state.isKeepSharePointDefaultValue ? 4 : -6 }}>
                                        <$g.Popover>
                                            {this.props.context.configurations.defaultTermMsg}
                                        </$g.Popover>
                                    </div>
                                </$g.RadioOption>
                                <$g.RadioOption value={KeepSPDefaultValueOption.No} text={RMResx.RM_JS_SPS_KeepSPDefaultValue_Option_No}>
                                </$g.RadioOption>
                            </$g.RadioGroup>
                        </div>}
                    {/* Site Collection Document Level, Need Related Record App Setting */}
                    {/* This part will also be promoted to components later */}
                    {(RM.gData.enviromentName !== Enviroments.ChinaNorth && this.props.context.configurations.enableRelatedRecords) && <div>
                        <div className="ra-setting-panel-checkbox">
                            <R.Checkbox
                                id="raCrmCsRelatedRecordsChk"
                                text={RMResx.RM_SP_SettingRelatedRecords}
                                title={RMResx.RM_SP_SettingRelatedRecords}
                                checked={this.state.enableRelatedRecords}
                                onChange={this.handleRelatedRecordsChanged}
                            />
                            <$g.Popover>{RMResx.RM_SP_Download_APPSolution}</$g.Popover>
                        </div>
                        {this.state.enableRelatedRecords && <div>
                            <div className="ra-setting-panel-enable">
                                <form id="crm-form-download" method="post" action="">
                                    <input type="hidden" id="downloadFlag" name="downloadFlag" value="" />
                                    <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                                </form>
                                <span onClick={this.handleDownloadRelatedApp} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_SP_Download_APPPackage}</span>
                            </div>
                        </div>}
                    </div>}
                </div>
            </R.Validation>
        </div>;
    }
}