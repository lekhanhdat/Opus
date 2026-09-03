import { ToSearchComponentDispatchType } from './../../Constants';
import { checkPermission } from './../../../../../Utilities/permissionManager';
import { EnvironmentHelper, LicenseHelper } from '../../../../../Utilities/CommonUtil';

let idCount = 0;
export default class HSFilteType extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            commonTypes: [
                { text: "docx", value: "docx", checked: false },
                { text: "pdf", value: "pdf", checked: false },
                { text: "xlsx", value: "xlsx", checked: false },
                { text: "msg", value: "msg", checked: false },
                { text: RMResx.RM_RDM_RecordDetails_DataType_SPFolder, value: "400", checked: false },
                {
                    text: RMResx.RM_RDM_RecordDetails_DataType_SPItem,
                    value: "RM_RDM_RecordDetails_DataType_SPItem",
                    checked: false
                },
                {
                    text: RMResx.RM_RDM_RecordDetails_DataType_SPDocument,
                    value: "RM_RDM_RecordDetails_DataType_SPDocument",
                    checked: false
                },
                { text: RMResx.RM_PRM_PRE_TableItemType_Container, value: "9250", checked: false },
                { text: RMResx.RM_PRM_PRE_Filter_PhysicalBox, value: "9300", checked: false },
                { text: RMResx.RM_PRM_PRE_Filter_PhysicalFile, value: "9400", checked: false },
                { text: RMResx.RM_PRM_PRE_Filter_PhysicalRecord, value: "9500", checked: false },
                { text: RMResx.RM_HS_Filter_Folder, value: "2100", checked: false },
                { text: RMResx.RM_RDM_RecordDetails_DataType_AzureFileDirectory, value: "7002", checked: false },
                { text: RMResx.RM_HS_Filter_Folder_Google, value: "7202", checked: false },
                { text: RMResx.RM_HS_Filter_File_Google, value: "7203", checked: false },
            ],
            otherTypes: [],
            selectedTypeText: RMResx.RM_JS_Common_None,
            selectAllChecked: false,
            typeValid: true,
            applyBtnDisabled: true,
            showFilterType: false
        };
        this.commonTypeValues = [
            "docx",
            "msg",
            "pdf",
            "RM_RDM_RecordDetails_DataType_SPItem",
            "RM_RDM_RecordDetails_DataType_SPDocument",
            "xlsx",
            "400",
            "9250",
            "9300",
            "9400",
            "9500",
            "2100",
            "7002",
        ];
        this.selectedCommonTypeValues = [];
        this.selectedOtherTypeValues = [];
        this.realFilteTypeValues = [];
        this.typeId = "type" + idCount++;
    }

    componentInit() {
        if (LicenseHelper.HasOpusGoogleLicense()) {
            this.commonTypeValues.push("7202", "7203");
        }

        if (EnvironmentHelper.IsGCPEnvironment) {
            const commonTypes = this.state.commonTypes.filter(type => type.value !== "2100");
            this.commonTypeValues = this.commonTypeValues.filter(type => type !== "2100");

            this.setState({ commonTypes });
        }

        if (LicenseHelper.HasOpusGoogleLicenseOnly()) {
            const hiddenTypeValues = new Set(["RM_RDM_RecordDetails_DataType_SPItem", "RM_RDM_RecordDetails_DataType_SPDocument"]);
            if (!checkPermission("Source_FS", RM.UserResources)) {
                hiddenTypeValues.add("2100");
            }
            if (!checkPermission("Source_AzureFile", RM.UserResources)) {
                hiddenTypeValues.add("7002");
            }
            const commonTypes = this.state.commonTypes.filter(type => !hiddenTypeValues.has(type.value));
            this.commonTypeValues = this.commonTypeValues.filter(type => !hiddenTypeValues.has(type));

            this.setState({ commonTypes });
        }
    }

    componentReceive(data) {
        if (data == ToSearchComponentDispatchType.Valid) {
            this.showValidMsg();
        } else {
            this.realFilteTypeValues = data.Value || [];
            this.setSelectedTypeText();
            this.setState({ applyBtnDisabled: false });
        }
    }

    onCommonTypesChanged = (value) => {
        this.selectedCommonTypeValues = value;
        let selectAllChecked = value.length == this.state.commonTypes.length;
        this.setState({ selectAllChecked: selectAllChecked });
        this.setApplyBtnDisabled();
    }

    onOtherTypesChange = (args) => {
        this.selectedOtherTypeValues = [];
        for (let item of args.newValue) {
            this.selectedOtherTypeValues.push(item.value);
        }
        this.setApplyBtnDisabled();
    }

    setApplyBtnDisabled() {
        let selectedAllTypesCount = [...this.selectedOtherTypeValues, ...this.selectedCommonTypeValues].length;
        this.setState({ applyBtnDisabled: selectedAllTypesCount == 0 });
    }

    onSelectAll = (checked) => {
        let commonTypes = RM.deepcopy(this.state.commonTypes);
        let isSelectAll = checked;
        for (let item of commonTypes) {
            item.checked = isSelectAll;
        }
        this.selectedCommonTypeValues = isSelectAll ? this.commonTypeValues : [];
        this.setState({
            commonTypes: commonTypes,
            selectAllChecked: isSelectAll
        });
        this.setApplyBtnDisabled();
    }

    onApplyClick = () => {
        this.realFilteTypeValues = [...this.selectedCommonTypeValues, ...this.selectedOtherTypeValues];
        let isNoSelect = this.realFilteTypeValues.length == 0;
        let searchTypeParam = isNoSelect ? null : RM.deepcopy(this.realFilteTypeValues);
        if(!isNoSelect){
            this.realFilteTypeValues = [...new Set(this.realFilteTypeValues)];
        }
        this.setSelectedTypeText();
        this.setState({
            typeValid: !isNoSelect,
        });
        this.props.onChange(searchTypeParam);
    }

    onClearClick = () => {
        this.selectedCommonTypeValues = [];
        this.selectedOtherTypeValues = [];
        let commonTypes = RM.deepcopy(this.state.commonTypes);
        for (let item of commonTypes) {
            item.checked = false;
        }
        this.setState({
            commonTypes: commonTypes,
            otherTypes: [],
            selectAllChecked: false,
            applyBtnDisabled: true
        });
        this.onApplyClick();
    }

    onShowTypeFilterPopup = () => {
        //common type 回显
        let commonTypes = RM.deepcopy(this.state.commonTypes);
        if (!checkPermission("Source_Google", RM.UserResources)) {
            commonTypes = RM.deepcopy(this.state.commonTypes.filter(item => item.value != '7202' && item.value != '7203'));
        }
        let selectedCommonTypeValues = [];
        let selectAllChecked = true;
        for (let item of commonTypes) {
            item.checked = false;
            if (this.realFilteTypeValues.indexOf(item.value) != -1) {
                item.checked = true;
                selectedCommonTypeValues.push(item.value);
            } else {
                selectAllChecked = false;
            }
        }
        //other type 回显
        let selectedOtherTypes = [];
        let selectedOtherTypeValues = [];
        for (let value of this.realFilteTypeValues) {
            if (this.commonTypeValues.indexOf(value) == -1) {
                selectedOtherTypeValues.push(value);
            }
        }
        selectedOtherTypes = this.getOtherTypesItems(selectedOtherTypeValues);
        //回归数据源。
        this.selectedCommonTypeValues = selectedCommonTypeValues;
        this.selectedOtherTypeValues = selectedOtherTypeValues;

        this.setState({
            commonTypes: commonTypes,
            otherTypes: selectedOtherTypes,
            selectAllChecked: selectAllChecked,
            applyBtnDisabled: this.realFilteTypeValues.length == 0,
            showFilterType: true
        });
    }

    onCancelClick = () =>{
        this.setState({
            showFilterType: false
        })
    }

    getOtherTypesItems(values) {
        let items = [];
        for (let value of values) {
            let item = {};
            item.invalid = false;  //RichCombobox不需要验证，都置为false
            item.checked = true;
            item.text = value;
            item.value = value;
            items.push(item);
        }
        return items;
    }

    onMatchOtherTypes = (args) => {
        return this.getOtherTypesItems(args.list);
    }

    setSelectedTypeText() {
        let selectedTypeText = RMResx.RM_JS_Common_None;
        let selectedTypeCount = this.realFilteTypeValues.length;
        let selectedTypeValue = this.realFilteTypeValues[0];
        switch (selectedTypeCount) {
            case 0:
                selectedTypeText = RMResx.RM_JS_Common_None;
                break;
            case 1:
                if (selectedTypeValue == "400" ||
                    selectedTypeValue == "9250" ||
                    selectedTypeValue == "9300" ||
                    selectedTypeValue == "9400" ||
                    selectedTypeValue == "9500" ||
                    selectedTypeValue == "2100" ||
                    selectedTypeValue == "7002" ||
                    selectedTypeValue == "7202" ||
                    selectedTypeValue == "7203"
                ) {
                    for (let item of this.state.commonTypes) {
                        if (item.value == selectedTypeValue) {
                            selectedTypeText = item.text;
                            break;
                        }
                    }
                } else {
                    selectedTypeText = RMResx[selectedTypeValue] || this.realFilteTypeValues[0];
                }
                break;
            default:
                selectedTypeText = RMResx.RM_Common_Combobox_SelectedXItems.format(selectedTypeCount);
        }
        this.setState({ selectedTypeText: selectedTypeText });
    }

    showValidMsg() {
        this.setState({ typeValid: false });
    }

    renderCommonTypes() {
        return <div className="hs-filter-common-type">
            <div className="type-title">{RMResx.RM_JS_BCM_Explorer_SpecifyTypes_Title}</div>
            <div className="type-content margin-bottom-m">
                <div className="type-select-all">
                    <R.Checkbox
                        text={RMResx.RM_JS_RC_Report_SelectAll}
                        title={RMResx.RM_JS_RC_Report_SelectAll}
                        name="selecte-all"
                        checked={this.state.selectAllChecked}
                        onChange={this.onSelectAll}
                    />
                </div>
                <R.Checkbox.Group
                    block={true}
                    name="common-type"
                    items={this.state.commonTypes}
                    onChange={this.onCommonTypesChanged}
                />
            </div>
        </div>;
    }

    renderOtherTypes() {
        return <div className="hs-filter-other-type">
            <div className="type-title">
                {RMResx.RM_JS_BCM_Explorer_OtherTypes_Title}
            </div>
            <div className="type-content">
                <R.RichCombobox
                    silence={true}
                    textField="text"
                    valueField="value"
                    checkedField='checked'
                    invalidField="invalid"
                    value={this.state.otherTypes}
                    enableSearch={true}
                    searchPlaceholder={RMResx.RM_HS_Filter_OtherTypeWatermark}
                    items={this.state.otherTypes}
                    onChange={this.onOtherTypesChange}
                    doMatch={this.onMatchOtherTypes}
                />
            </div>
        </div>;
    }

    render() {
        let selectedTypeText = this.state.selectedTypeText;
        let noSelectItem = selectedTypeText == RMResx.RM_JS_Common_None;
        return <div className="flex">
            <div className="flex-1">
                <R.Input
                    type="text"
                    value={RMResx.RM_HS_Contains}
                    width={"100%"}
                    height={40}
                    readonly={true}
                />
            </div>
            <div className="flex-1 margin-left-m width-0">
                <R.ComboboxShell
                    content={selectedTypeText}
                    id={this.typeId}
                    height={40}
                    width={"100%"}
                    block={false}
                    triggerType="all"
                    offClose={true}
                    clearable={!noSelectItem}
                    status={{ show: this.state.showFilterType }}
                    compact={true}
                    onShow={this.onShowTypeFilterPopup}
                    onClear={this.onClearClick}
                >
                    <div className="hs-filter-type-popup padding-m">
                        {this.renderCommonTypes()}
                        {this.renderOtherTypes()}
                    </div>
                    <>
                        <R.Button
                            slot="buttons"
                            name="cancel"
                            text={RMResx.RM_JS_Common_Cancel}
                            value="close"
                            onClick={this.onCancelClick}
                        />
                        <R.Button
                            slot="buttons"
                            name="save"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_JS_Common_Save}
                            value="close"
                            disabled={this.state.applyBtnDisabled}
                            onClick={this.onApplyClick}
                        />
                    </>
                </R.ComboboxShell>
                <R.ValidationFaker valid={this.state.typeValid} of={`#${this.typeId}`} message={RMResx.RM_HS_NoSearchColValValidMsg} />
            </div>
        </div>;
    }
}