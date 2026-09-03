import "../../../Less/CP/stubSettings.less";
import { EnvironmentHelper, getUserGuildTagPage, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import { storageKeys } from "../../../Utilities/Constant";
import { MessageType, StubFileType, StubFileTypeColNoASPX, StubFileTypeCol } from "../CPConstants";

export const RetentionConditionUnit = {
    Days: 0,
    Week: 1,
    Month: 2,
    Year: 3,
};

export default class StubPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.textareaId = "";
        this.insertIndex = 0;
        this.state = {
            stubName: "",
            stubFileType: StubFileType.Txt,
            stubTypeList: RM.deepcopy(StubFileTypeColNoASPX),
            stubContent: RMResx["StorageOptimization.Gui_A1AA2887-13C3-44B6-B26B-01E7DC580F21"] + "\n" + `[${RMResx.RM_AR_CP_Stub_Panel_RestoreLink}]`,
            tagItems: [],
            declareStub: false,
            showTagOptions: false,
            hasConfigStubRetention: false,
            retentionPeriodValue: "1",
            retentionPeriodOptions: [],
            retentionPeriodUnit: RetentionConditionUnit.Year,
        };
        this.enableRecordsArchiver = LicenseHelper.EnableRecordsArchiver();
    }

    componentInit() {
        if (EnvironmentHelper.IsGCPEnvironment) {
            const newStubTypeList = this.state.stubTypeList.filter(stub => stub.value !== StubFileType.Url);
            const newStubContent = RMResx["StorageOptimization.Gui_A1AA2887-13C3-44B6-B26B-01E7DC580F21"];

            this.setState({
                stubTypeList: newStubTypeList,
                stubContent: newStubContent
            })
        }

        this.loadStubSetting();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    getTagItems(selectStubType) {
        let allSupportStubTyps = [StubFileType.Aspx, StubFileType.Txt, StubFileType.Html, StubFileType.Url];
        const tagItems = [
            {
                checked: false,
                name: RMResx["StorageOptimization.Gui_9FE3A6A6-DB1B-478A-9C84-3793B070A958"],
                disabled: false,
                supportStubFileTypes: allSupportStubTyps
            },
            {
                checked: false,
                name: RMResx["StorageOptimization.Gui_FB4CF4C0-AA67-43A7-9C37-97719E9B97A3"],
                disabled: false,
                supportStubFileTypes: allSupportStubTyps
            },
            {
                checked: false,
                name: RMResx["StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B"],
                disabled: false,
                supportStubFileTypes: allSupportStubTyps
            },
            {
                checked: false,
                name: RMResx["StorageOptimization.Gui_AE414513-8007-44BC-98B9-8E6B1212C257"],
                disabled: false,
                supportStubFileTypes: allSupportStubTyps
            },
        ];
        if (!EnvironmentHelper.IsGCPEnvironment) {
            tagItems.push({
                checked: false,
                name: RMResx.RM_AR_CP_Stub_Panel_RestoreLink,
                disabled: false,
                supportStubFileTypes: allSupportStubTyps
            });
        }
        if (RM.gData.enableRecordsArchiver) {
            let externalLinkItem = {
                checked: false,
                name: RMResx.RM_AR_CP_Stub_Panel_ExternalLink,
                disabled: false,
                extentionLabel: `[${RMResx.RM_AR_CP_Stub_Panel_ExternalLink_LinkTitle}|${RMResx.RM_AR_CP_Stub_Panel_ExternalLink_LinkExample}]`,
                supportStubFileTypes: [StubFileType.Aspx, StubFileType.Html, StubFileType.Url]
            };
            tagItems.push(externalLinkItem);
        }
        return [...tagItems.filter(item => item.supportStubFileTypes.some(t => t == selectStubType))]
    }

    getRetentionPeriodOptions(selectedRetentionPeriodUnit) {
        const options = [
            { text: RMResx.RM_JS_RDM_CreateRule_Unit_Days, value: RetentionConditionUnit.Days },
            { text: RMResx.RM_JS_RDM_CreateRule_Unit_Weeks, value: RetentionConditionUnit.Week },
            { text: RMResx.RM_JS_RDM_CreateRule_Unit_Months, value: RetentionConditionUnit.Month },
            { text: RMResx.RM_JS_RDM_CreateRule_Unit_Years, value: RetentionConditionUnit.Year },
        ];
        return options.map((item) => ({
            ...item,
            checked: selectedRetentionPeriodUnit === item.value,
        }));
    }

    loadStubSetting() {
        if (this.props.cellStubId) {
            $$.loading(true);
            let option = {
                url: "/api/StubSetting/GetStubSettingById",
                method: "POST",
                data: this.props.cellStubId,
            };
            fetchUtility(option).then((res) => {
                $$.loading(false);

                let loadStubTypeList = [];
                RM.deepcopy(StubFileTypeCol).forEach((item) => {
                    item.checked = item.value == res.StubType ? true : false;
                    loadStubTypeList.push(item);
                });

                if (res.StubType !== StubFileType.Aspx) {
                    loadStubTypeList = loadStubTypeList.filter((item) => item.value != StubFileType.Aspx); // Remove ASPX option
                }

                if (EnvironmentHelper.IsGCPEnvironment) {
                    loadStubTypeList = loadStubTypeList.filter((item) => item.value != StubFileType.Url); // Remove Url option in GCP env
                }

                let content = "";
                if (res.StubContent == "") {
                    content = this.state.stubContent;
                } else {
                    content = res.StubContent;
                }
                const periodUnit = !!res.IsEnabledRetention ? res.RetentionUnit : RetentionConditionUnit.Year;
                this.setState({
                    stubName: res.Name,
                    stubFileType: res.StubType,
                    stubTypeList: loadStubTypeList,
                    stubContent: content,
                    declareStub: res.IsDeclareStubAsRecords,
                    tagItems: this.getTagItems(res.StubType),
                    hasConfigStubRetention: res.IsEnabledRetention,
                    retentionPeriodValue: res.RetentionValue || 1,
                    retentionPeriodOptions: this.getRetentionPeriodOptions(periodUnit),
                    retentionPeriodUnit: periodUnit,
                });
            }).catch((e) => {
                $$.loading(false);
            });
        } else {
            this.setState({
                tagItems: this.getTagItems(this.state.stubFileType),
                retentionPeriodOptions: this.getRetentionPeriodOptions(this.state.retentionPeriodUnit),
            });
        }
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }

        if (this.state.stubFileType == StubFileType.Url) {
            this.setState({ stubContent: "" });
        }
        let stubDataObj = {
            Name: this.state.stubName,
            StubType: this.state.stubFileType,
            StubContent: this.state.stubContent,
            IsDeclareStubAsRecords: this.state.declareStub,
            IsEnabledRetention: this.state.hasConfigStubRetention,
            RetentionValue: Number(this.state.retentionPeriodValue),
            RetentionUnit: this.state.retentionPeriodUnit,
        };
        if (this.props.cellStubId) {
            stubDataObj.Id = this.props.cellStubId;
        }
        $$.loading(true);
        let option = {
            url: '/api/StubSetting/CreateOrEditStubSetting',
            method: "Post",
            data: stubDataObj
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == MessageType.Successful) {
                callback(true, stubDataObj);
                showToast.success(RMResx.RM_AR_CP_Stub_SaveSuccessful);
            } else {
                showToast.error(result.ErrorMessage);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onStubNameChanged = (value) => {
        this.setState({ stubName: value });
    }

    onStubFileTypeChanged = (args) => {
        this.setState({ stubFileType: args.newValue.value, tagItems: this.getTagItems(args.newValue.value) });
    }

    onStubContentChange = (value) => {
        this.setState({ stubContent: value });
    }

    onShowTagList = (value) => {
        this.setState({ showTagOptions: !this.state.showTagOptions });
    }

    onTagChange = (args) => {
        let inputText = "";
        let insetString = "[" + args.newValue.name + "]";
        if (args.newValue.extentionLabel) {
            insetString = args.newValue.extentionLabel;
        }
        if (this.textareaId != "") {
            let textInput = document.getElementById(this.textareaId);
            let text = textInput.value;
            text = text.substring(0, this.insertIndex) + insetString + text.substring(this.insertIndex);
            inputText = text;
            this.insertIndex += insetString.length;
        } else {
            inputText = this.state.stubContent + insetString;
        }
        this.setState({ stubContent: inputText, showTagOptions: !this.state.showTagOptions });
    }

    onConfigStubRetentionChange = (checked) => {
        this.setState({ hasConfigStubRetention: checked });
    }

    onRetentionPeriodValueChange = (value) => {
        this.setState({ retentionPeriodValue: value });
    }

    onRetentionPeriodUnitChange = (args) => {
        this.setState({ retentionPeriodUnit: args.newValue.value }, () => {
            $$.verify('raCPLastNumIptValidation');
        });
    }

     onDeclareStubChanged = (args) => {
        this.setState({ declareStub: args }, () => {
            const ref = this.refRecordLabelValid?.ref?.current;
            if (ref) {
                $$.verify(ref);
            }
        });
    };

    onBlur = (e) => {
        if (e.currentTarget != null) {
            this.textareaId = e.currentTarget.id;
            let textInput = document.getElementById(this.textareaId);
            this.insertIndex = textInput.selectionStart;
        }
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    customVerify = (value) => {
        const { retentionPeriodUnit } = this.state;
        const retentionPeriodNumber = Number(value);

        const maxValueByUnit = {
            [RetentionConditionUnit.Days]: 1825000,
            [RetentionConditionUnit.Week]: 260714,
            [RetentionConditionUnit.Month]: 60000,
            [RetentionConditionUnit.Year]: 5000,
        };
        
        if (retentionPeriodNumber > maxValueByUnit[retentionPeriodUnit]) {
            return RMResx.RM_AR_CP_Stub_Panel_SpecificRetentionPeriodValid;
        }

        return true;
    }

    recordLabelValidation = () => {
        if (this.state.declareStub && !this.props.isConfiguredGeneral) {
            return RMResx.RM_AR_CP_Stub_Panel_RecordsLabel_Error;
        }
        return true;
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-stubPanel-content">
                        <div className="ra-stubPanel-title require">{RMResx.RM_AR_CP_Stub_Panel_Name}</div>
                        <R.Validation
                            element="Input"
                            require={RMResx.RM_AR_CP_Common_NameEmpty} >
                            <R.Input
                                id="raStubSettingsNameIpt"
                                type="text"
                                value={this.state.stubName}
                                onChange={this.onStubNameChanged}
                                aria={{ ariaLabel: RMResx.RM_AR_CP_Stub_Panel_Name }}
                            />
                        </R.Validation>
                    </div>
                    <div className="ra-stubPanel-content">
                        <div className="ra-stubPanel-title require">{RMResx.RM_AR_CP_Stub_Panel_StubType}</div>
                        <R.Validation
                            element="Combobox"
                            require={RMResx.RM_AR_CP_Common_SelEmpty} >
                            <R.Combobox
                                id="raStubTypeCom"
                                tooltipField="name"
                                width='100%'
                                textField="name"
                                valueField="value"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={this.state.stubTypeList}
                                onChange={this.onStubFileTypeChanged}
                                aria={{ ariaLabel: RMResx.RM_AR_CP_Stub_Panel_StubType }}
                            />
                        </R.Validation>
                        {this.state.stubFileType == StubFileType.Aspx && <div className="flex flex-column gap-s margin-top-s">
                            <$g.I18NProvider msg={RMResx.RM_AR_CP_Stub_Type_AspxDes}>
                                <a className="ra-link-a" href={getUserGuildTagPage(storageKeys.stubManagement)} target="_blank">
                                    {RMResx.RM_AR_CP_Stub_Type_AspxGuide}
                                </a>
                            </$g.I18NProvider>
                            <R.Messagebar
                                message={RMResx.RM_AR_CP_Stub_Type_AspxWarning}
                                classify="warn"
                                status={{ show: this.state.stubFileType == StubFileType.Aspx }}
                                hasClose={false}
                            />
                        </div>}
                    </div>
                    {this.state.stubFileType != StubFileType.Url && <div className="ra-stubPanel-content">
                        <div id="add-tag-container" className="ra-stubPanel-tag">
                            <div className="ra-stubPanel-title require">{RMResx.RM_AR_CP_Stub_Panel_StubContent}</div>
                            <div id="add_tag"
                                role="combobox"
                                aria-haspopup="listbox"
                                aria-expanded="false"
                                onClick={this.onShowTagList}
                                onKeyDown={this.onKeyDown}
                                tabIndex="0" >
                                <div id="add_tag_icon" aria-hidden="true">
                                    <div className="fia-plus"></div>
                                </div>
                                <span id="add_tag_text">{RMResx.RM_AR_CP_Stub_Panel_Addtags}</span>
                                <span id="add_tag-down" className="fia-triangle-down"></span>
                            </div>
                            {this.state.showTagOptions &&
                                <div id="add_options">
                                    <R.Selection
                                        id="raStubSettingsTagOptions"
                                        items={this.state.tagItems}
                                        disabled={false}
                                        type="single"
                                        textField="name"
                                        valueField="value"
                                        checkedField="checked"
                                        tooltipField="tooltip"
                                        disabledField="disabled"
                                        searchable={false}
                                        excludeChecked={false}
                                        linkMode={false}
                                        onChange={this.onTagChange} />
                                </div>
                            }
                        </div>
                        <R.Validation
                            element="Input"
                            require={RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]} >
                            <R.Input
                                id="raStubSettingsContentIpt"
                                type="textarea"
                                className="resizable"
                                value={this.state.stubContent}
                                onChange={this.onStubContentChange}
                                onBlur={this.onBlur}
                                aria={{ ariaLabel: RMResx.RM_AR_CP_Stub_Panel_StubContent }}
                            />
                        </R.Validation>
                    </div>}
                    {this.enableRecordsArchiver && (
                        <div className="ra-stubPanel-content">
                            <R.Checkbox
                                id="raCrDeleteStubChk"
                                text={RMResx.RM_AR_CP_Stub_Panel_ConfigStubRetention}
                                checked={this.state.hasConfigStubRetention}
                                onChange={this.onConfigStubRetentionChange}
                            />
                            {this.state.hasConfigStubRetention && (
                                <div style={{ marginLeft: 22 }} className="margin-top-xs">
                                    <div className="font-semibold require">{RMResx.RM_AR_CP_Stub_Panel_SpecificRetentionPeriod}</div>
                                    <div className="flex align-center gap-s margin-top-xs">
                                        <R.Validation id='raCPLastNumIptValidation' element="Input" require rules={{ customVerify: this.customVerify }} errorby="#retentionPeriodError">
                                            <R.Input
                                                id="raCPLastNumIpt"
                                                type="number"
                                                hasControl
                                                width={100}
                                                min={1}
                                                value={this.state.retentionPeriodValue}
                                                onChange={this.onRetentionPeriodValueChange}
                                                aria={{ ariaLabel: RMResx.RM_AR_CP_Stub_Panel_SpecificRetentionPeriod }}
                                            />
                                        </R.Validation>
                                        <R.Combobox
                                            id="raCPLastCom"
                                            width={170}
                                            searchable={false}
                                            textField='text'
                                            valueField='value'
                                            checkedField='checked'
                                            items={this.state.retentionPeriodOptions}
                                            onChange={this.onRetentionPeriodUnitChange}
                                        />
                                    </div>
                                    <div id="retentionPeriodError"></div>
                                </div>
                            )}
                        </div>
                    )}
                    {!LicenseHelper.Is21VEnv() && this.enableRecordsArchiver ? (
                        <div className="ra-stubPanel-content">
                            <R.Checkbox
                                id="raRecordsLabelChk"
                                text={(
                                    <$g.I18NProvider msg={RMResx.RM_AR_CP_Stub_Panel_RecordsLabel}>
                                        <span>
                                            <a
                                                className="ra-link-a"
                                                href="/Root/CP/GeneralSetting"
                                            >
                                                {RMResx.RM_JS_SP_MigrateDeclaredRecords_GeneralSetting}
                                            </a>
                                            <span tabIndex={0}>{`: ${this.props.recordsLabelValue}`}</span>
                                        </span>
                                    </$g.I18NProvider>
                                )}
                                checked={this.state.declareStub}
                                onChange={this.onDeclareStubChanged}
                            />
                            <R.ValidationFaker
                                ref={(r) => (this.refRecordLabelValid = r)}
                                valid={this.recordLabelValidation}
                                of="raRecordsLabelChk"
                            />
                        </div>
                    ) : (
                        <div className="ra-stubPanel-content">
                            <R.Checkbox
                                id="raDeclareChk"
                                text={RMResx.RM_AR_CP_Stub_Panel_Declare}
                                title={RMResx.RM_AR_CP_Stub_Panel_Declare}
                                checked={this.state.declareStub}
                                onChange={this.onDeclareStubChanged}
                            />
                            {this.state.declareStub && <div className="ra-stubPanel-validation-msg" tabIndex="0">
                                {RMResx.RM_AR_CP_Stub_Panel_Declare_WarnForOD}
                            </div>}
                        </div>
                    )}
                </div>
            </R.Validation>
        </div>;
    }
}