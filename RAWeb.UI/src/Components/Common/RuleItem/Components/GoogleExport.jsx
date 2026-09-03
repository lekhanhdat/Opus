import * as Constants from "./Constants";
import { LicenseHelper, setCheckedStatus, showToast } from "../../../../Utilities/CommonUtil";

export const ExportLocationOption = {
    None: 0,
    Storage: 1,
    SPLibOrFolder: 2,
    SPLibOrFolderFromTree: 3,
}

export default class GoogleExport extends R.Component {
    idAttr = true;
    componentCreate() {
        this.ConfigMissingLinks = Constants.ConfigMissingLinks;
        this.exportData = {
            isCreateRuleExport: false,
            isExport: false,
            selectExport: {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
            noExportTypeValue: false,
            naraConfigMissing: false,
            exportTypeValue: -1,
            exportBefore: true,
            exportWithout: true,
            exportTypes: Constants.exportTypeAll,
            isExportOnly: this.props.isExportOnly,
            exportLocationOption: ExportLocationOption.Storage,
            storageId: "",
            storageName: "",
            locationPath: "",
            nodeItem: null,
            nodeItemStr: "",
        };
        this.state = {
            exportData: this.deepCopy(this.exportData),
            exportLocationList: [],
            storage: null,
            isLocationValidate: false,
            locationValidateMsg: "",
            locationValidateType: "",
            noLocation: false,
            noSelectNode: false,
        };
        this.nodeItem = null;
        this.bind(["createRuleExportClick", "exportTypeSelectChanged"]);
    }

    componentReceive(action, data,status) {
        switch (action) {
            case Constants.dispatchAction.setData:
                this.setExportData(data);
                break;
            case Constants.dispatchAction.save: {
                let exportData = this.state.exportData;
                this.props.getIsVerificationPassed(this.exportCustomValidate());
                this.props.getIsVerificationLocationPassed(this.onActionCustomValidate());
                this.props.getExportDate(exportData);
                break;
            }
            case Constants.dispatchAction.elementDisabled:
                this.setState({elementDisabled: data});
                break;
            case Constants.dispatchAction.clearData:
                this.ExportClearData(data,status);
                break;
        }
    }

    setExportData(data) {
        if( data.EnableExport){
            let exportType = -1;
            if (!data.ExportInfo||!data.ExportInfo.exportType) {
                exportType = 0;
            } else {
                exportType = data.ExportInfo.exportType;
            }
            this.exportData.isExport = true;
            for (let key of this.exportData.exportTypes) {
                if (key.id == exportType) {
                    this.exportData.selectExport = key;
                    this.exportData.exportTypeValue = key.id;
                }
            }

            // Just support export to location for new logical account
            LicenseHelper.EnableRecordsArchiver() && this.getAllActiveExportLocation();
            
            this.exportData.exportLocationOption = ExportLocationOption.Storage;
            this.exportData.storageId = data.ExportInfo.exportLocationId;
            this.exportData.storageName = data.ExportInfo.exportLocationName;

            if (exportType == 5) {
                this.getValidateExport();
            } else {
                this.exportData.naraConfigMissing = false;
            }
        }
        this.setState({
            exportData: this.exportData
        });
    }

    ExportClearData(data,isDisabled) {
        if (data == 64) {
            this.exportData = {
                isCreateRuleExport: isDisabled,
                isExport: false,
                isExportOnly: false,
                selectExport: {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
                noExportTypeValue: false,
                naraConfigMissing: false,
                exportTypeValue: -1,
                exportBefore: true,
                exportWithout: true,
                exportTypes: Constants.exportTypeAll,
                exportLocationOption: ExportLocationOption.Storage,
                storageId: "",
                storageName: "",
                locationPath: "",
                nodeItem: null,
                nodeItemStr: "",
            };
        }
        this.setState({
            exportData: this.deepCopy(this.exportData)
        });
    }

    exportCustomValidate() {
        let item = this.exportData;
        let isValid = true;
        if (item.isExport || item.isExportOnly) {
            if (item.exportTypeValue == -1) {
                item.noExportTypeValue = true;
                isValid = false;
            } else {
                item.noExportTypeValue = false;
            }
        }
        this.setState({ exportData: item });
        return isValid;
    }

    gotoExportSetting() {
        this.props.jumpExportSettings();
    }

    gotoExportSettingByKey(e){
        if (e.keyCode == 13) {
            this.props.jumpExportSettings();
        }
    }

    exportTypeSelectChanged(args) {
        let item = this.exportData;
        item.selectExport = args.newValue;
        if (typeof (args.newValue) != "undefined") {
            item.exportTypeValue = args.newValue.id;
            
            // Just support export to location for new logical account
            LicenseHelper.EnableRecordsArchiver() && this.getAllActiveExportLocation();

            if (item.exportTypeValue == 5) {
                this.getValidateExport();
            } else {
                item.naraConfigMissing = false;
            }
            this.exportCustomValidate();
        }
        this.setState({
            exportData: this.deepCopy(item)
        });
    }

    getValidateExport() {
        this.validateExportNARASetting();
    }

    createRuleExportClick() {
        this.exportData.isExport = !this.exportData.isExport;
        this.setState({
            exportData: this.deepCopy(this.exportData)
        });
    }

    validateExportNARASetting() {
        let item = this.exportData;
        let urlData = "/API/RuleApi/validateNaraExportSetting?sourceFlag=" + this.props.type;
        let option = {
            url: urlData,
            method: "get"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.Success) {
                item.naraConfigMissing = false;
            } else {
                item.naraConfigMissing = true;
            }
            item.ConfigMissingLink = this.ConfigMissingLinks[2];
            this.setState({
                exportData: this.deepCopy(item)
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    //深复制
    deepCopy(value) {
        return JSON.parse(JSON.stringify(value));
    }

    getAllActiveExportLocation = () => {
        $$.loading(true);
        const option = {
            url: "/api/StorageDevice/GetGoogleStoragLocationInfos",
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                //export location
                const exportList = [];
                const currentExportLocationId = res.CurrentExportLocationId;
                res.StorageInfo.forEach((item) => {
                    if (this.exportData.storageId) {
                        item.checked = item.Id == this.exportData.storageId;
                        if (item.checked) {
                            this.exportData.storageId = item.Id;
                            this.exportData.storageName = item.Name;
                            this.setState({ storage: item })
                        }
                    } else {
                        item.checked = item.Id == currentExportLocationId;
                        if (item.checked) {
                            this.exportData.storageId = currentExportLocationId;
                            this.exportData.storageName = item.Name;
                            this.setState({ storage: item });
                        }
                    }
                    
                    exportList.push(item);
                });
                this.setState({
                    exportData: this.deepCopy(this.exportData),
                    exportLocationList: exportList,
                    currentStorageId: currentExportLocationId,
                });
            })
            .catch((e) => {
                showToast.error(RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed);
            })
            .finally(() => $$.loading(false));
    }

    onLocationChange = (args) => {
        this.exportData.storageId = args.newValue.Id;
        this.exportData.storageName = args.newValue.Name;
        this.setState({ exportData: this.deepCopy(this.exportData), storage: args.newValue, });
    }

    onActionCustomValidate = () => {
        let isValid = true;
        // Export to location
        if (LicenseHelper.EnableRecordsArchiver() && (this.exportData.isExportOnly || this.exportData.isExport) && this.exportData.selectExport.id) {
            if (!$$.verify(this.googleValidation)) {
                isValid = false;
            }
        }
        return isValid;
    }
    
    renderGoogleExportLocation() {
        return (
            <div>
                <span className="ra-createRule-title-exp" tabIndex='0'>
                    {RMResx.RM_RDM_CreateRule_GoogleTitle_ExportTo}
                </span>
                <R.Validation>
                    <div className="margin-top-s" ref={r => this.googleValidation = r}>
                        <R.Validation
                            element="Combobox"
                            require={RMResx.RM_AR_CP_Common_SelEmpty}
                        >
                            <R.Combobox
                                id="raGoogleExportLocationCom"
                                tooltipField="Name"
                                width='100%'
                                textField="Name"
                                valueField="Id"
                                checkedField="checked"
                                linkMode={false}
                                searchable={false}
                                items={setCheckedStatus(
                                    "id", "Checked",
                                    this.state.exportLocationList,
                                    this.state.storage)}
                                onChange={this.onLocationChange}
                                aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_GoogleTitle_ExportTo }}
                            />
                        </R.Validation>
                    </div>
                </R.Validation>
            </div>
        )
    }
    
    renderExportContent(){
        const {exportData, elementDisabled} = this.state;
        const {exportTypes, noExportTypeValue, naraConfigMissing} = exportData;

        return <div className="ra-createRule-Exptype cr-archive-action-children-selection">
            <div className="flex ra-flex-align-center">
                <span className="ra-createRule-title-exp" tabIndex='0'>
                    {RMResx.RM_RDM_CreateRule_Title_ExportType}
                </span>
                <div className="ra-createRule-export-type-error margin-left-s width-percent-100">
                    <R.Combobox
                        id={"raCr" + this.props.id + "Type"}
                        width={"100%"}
                        textField='Name'
                        checkedField='Checked'
                        searchable={false}
                        valueField='id'
                        items={setCheckedStatus(
                            "id", "Checked",
                            exportTypes.slice(0, 4).filter(item => item.id === -1 || item.id ===5),
                            this.exportData.selectExport)}
                        disabled={elementDisabled}
                        onChange={this.exportTypeSelectChanged}
                    />
                </div>
            </div>
            <div className="ra-validation rm_export_error">
                <$g.ValidationMsg show={noExportTypeValue}>
                    {RMResx.RM_JS_RDM_CreateRule_Validation_noExportValue}
                </$g.ValidationMsg>
                
                <div
                    className={(naraConfigMissing) ? "block ra-rule-tips" : "none ra-rule-tips"}>
                    {RMResx.RM_ES_NARA_ConfigMissing.split("{0}")[0]}
                    <a className="ra-link-a ra-cursor-pointer" tabIndex="0" onClick={this.gotoExportSetting.bind(this)} onKeyDown={this.gotoExportSettingByKey.bind(this)}>{RMResx.RM_ES_NARA_ConfigMissingLink}</a>
                    {RMResx.RM_ES_NARA_ConfigMissing.split("{0}")[1]}
                </div>
            </div>
            {/* Export to location */}
            {LicenseHelper.EnableRecordsArchiver() && (this.exportData.isExportOnly || this.exportData.isExport) && this.exportData.selectExport.id > 0 && this.renderGoogleExportLocation()}
        </div>;
    }

    renderExportBeforeArchiver(){
        return <div>
            <div className="flex ra-flex-align-center">
                <div className="ra-createRule-question strong" tabIndex='0'>
                    {RMResx.RM_RDM_CreateRule_Title_ArchiveExport}
                </div>
                <$g.Popover>{RMResx.RM_JS_Rule_BeforeArchiveDescription}</$g.Popover>
            </div>
            <div id="rm_createRule_exportContainer">
                <div id="rm_createRule_export">
                    <R.Checkbox
                        id={"raCr" + this.props.id + "Chk"}
                        text={RMResx.RM_JS_RDM_CreateRule_Options_ExportBefore}
                        disabled={this.state.exportData.isCreateRuleExport || this.state.elementDisabled}
                        checked={this.state.exportData.isExport}
                        onChange={this.createRuleExportClick}
                    />
                </div>
                {
                    this.state.exportData.isExport && this.renderExportContent()
                }
            </div>
        </div>;
    }

    renderExportOnly(){
        return <div id="rm_createRule_exportContainer">
            {this.renderExportContent()}
        </div>;
    }

    render() {
        let isShowExportOnly = this.state.exportData.isExportOnly;
        let isShowExportBeforeArchiver = !isShowExportOnly && !this.state.exportData.isCreateRuleExport;
        return <div>
            <div id="rm_crateRule_approveAndExport">
                {isShowExportBeforeArchiver && this.renderExportBeforeArchiver()}
                {isShowExportOnly && this.renderExportOnly()}
            </div>
        </div>;
    }
}