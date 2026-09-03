import * as Constants from "./Constants";
import { LicenseHelper, setCheckedStatus, showToast } from "../../../../Utilities/CommonUtil";
import SPDestinationTree from "../../Tree/Instances/SPTree/SPDestinationTree";
import { TabIndex } from "../../../BCM/ContentRepositoryManagement/CRMForSPO";
import { checkPermission } from "../../../../Utilities/permissionManager";
import TeamsDestinationTree from "../../Tree/Instances/TeamsTree/TeamsDestinationTree";

export const ExportLocationOption = {
    None: 0,
    Storage: 1,
    SPLibOrFolder: 2,
    SPLibOrFolderFromTree: 3,
}

export default class Export extends R.Component {
    idAttr = true;
    componentCreate() {
        this.exportTypeFilted = Constants.exportTypeFilted;
        this.exportTypeAll = Constants.exportTypeAll;
        this.ConfigMissingLinks = Constants.ConfigMissingLinks;
        this.exportData = {
            isCreateRuleExport: false,
            isExport: false,
            selectExport: {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
            noExportTypeValue: false,
            veoConfigMissing: false,
            nnaConfigMissing: false,
            naraConfigMissing: false,
            exportTypeValue: -1,
            exportBefore: true,
            exportWithout: true,
            exportTypes: this.getExportTypeItems(this.props.ruleLevel),
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
            destinationTreeData: [],
            noSelectNode: false,
            destinationActiveTab: 0,
            destinationTreeDataForTeams: [],
        };
        this.nodeItem = null;
        this.bind(["createRuleExportClick", "exportTypeSelectChanged"]);
    }

    componentReceive(action, data,status) {
        switch (action) {
            case Constants.dispatchAction.setData:
                this.setExportData(data);
                if (this.props.destinationActiveTab) {
                    this.setState({ destinationTreeDataForTeams: data.MoveDto?.SPTreeStr ? JSON.parse(data.MoveDto.SPTreeStr) : [] });
                } else {
                    this.setState({ destinationTreeData: data.MoveDto?.SPTreeStr ? JSON.parse(data.MoveDto.SPTreeStr) : [] });
                }
                this.setState({ destinationActiveTab: this.props.destinationActiveTab });
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

    getExportTypeItems(ruleLevel){
        switch(ruleLevel){
            case 64:
            case 16:
            case 65536:
                return this.exportTypeAll;
            default:
                return this.exportTypeFilted;
        }
    }

    setExportData(data) {
        this.exportData.exportTypes = this.getExportTypeItems(data.RuleLevel);
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
            
            if (data.MoveDto) {
                if (data.MoveDto.SPTreeStr) {
                    const nodeItem = JSON.parse(data.MoveDto.SPTreeStr).find((item) => item.CheckNumber);
                    this.exportData.exportLocationOption = ExportLocationOption.SPLibOrFolderFromTree;
                    this.nodeItem = nodeItem;
                    this.exportData.nodeItem = nodeItem;
                    this.exportData.nodeItemStr = data.MoveDto.SPTreeStr;
                    this.exportData.locationPath = data.MoveDto.LocationPath;
                } else {
                    this.exportData.exportLocationOption = ExportLocationOption.SPLibOrFolder;
                    this.exportData.locationPath = data.MoveDto.LocationPath;
                }
            } else {
                this.exportData.exportLocationOption = ExportLocationOption.Storage;
                // this.exportData.storageId = data.StoragePolicyId;
                // this.exportData.storageName = data.StoragePolicyName;
                
                this.exportData.storageId = data.ExportInfo.exportLocationId;
                this.exportData.storageName = data.ExportInfo.exportLocationName;
            }

            if (exportType == 3 || exportType == 4 || exportType == 5) {
                this.getValidateExport(exportType);
            } else {
                this.exportData.veoConfigMissing = false;
                this.exportData.nnaConfigMissing = false;
                this.exportData.naraConfigMissing = false;
            }
        }
        this.setState({
            exportData: this.exportData
        });
    }

    ExportClearData(data,isDisabled) {
        if (data == 64 || data == 16 || data == 65536) {
            this.exportData = {
                isCreateRuleExport: isDisabled,
                isExport: false,
                isExportOnly: false,
                selectExport: {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
                noExportTypeValue: false,
                veoConfigMissing: false,
                nnaConfigMissing: false,
                naraConfigMissing: false,
                exportTypeValue: -1,
                exportBefore: true,
                exportWithout: true,
                exportTypes: this.exportTypeAll,
                exportLocationOption: ExportLocationOption.Storage,
                storageId: "",
                storageName: "",
                locationPath: "",
                nodeItem: null,
                nodeItemStr: "",
            };
        } else {
            this.exportData = {
                isCreateRuleExport: isDisabled,
                isExport: false,
                isExportOnly: false,
                selectExport: {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
                noExportTypeValue: false,
                veoConfigMissing: false,
                nnaConfigMissing: false,
                naraConfigMissing: false,
                exportTypeValue: -1,
                exportBefore: true,
                exportWithout: true,
                exportTypes: this.exportTypeFilted,
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

            if (item.exportTypeValue == 3 || item.exportTypeValue == 4 || item.exportTypeValue == 5) {
                this.getValidateExport(item.exportTypeValue);
            } else {
                item.veoConfigMissing = false;
                item.nnaConfigMissing = false;
                item.naraConfigMissing = false;
            }
            this.exportCustomValidate();
        }
        this.setState({
            exportData: this.deepCopy(item)
        });
    }

    getValidateExport(exportTypeValue) {
        switch (exportTypeValue) {
            case 3:
                this.validateExportSetting();
                break;
            case 4:
                this.validateExportNAASetting();
                break;
            case 5:
                this.validateExportNARASetting();
                break;
        }
    }

    createRuleExportClick() {
        this.exportData.isExport = !this.exportData.isExport;
        this.setState({
            exportData: this.deepCopy(this.exportData)
        });
    }

    validateExportSetting() {
        let item = this.exportData;
        let urlData = "/API/RuleApi/validateExportSetting?sourceFlag=" + this.props.type;
        let option = {
            url: urlData,
            method: "get"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.Success) {
                item.veoConfigMissing = false;
            } else {
                item.veoConfigMissing = true;
            }
            item.nnaConfigMissing = false;
            item.naraConfigMissing = false;
            item.ConfigMissingLink = this.ConfigMissingLinks[0];
            this.setState({
                exportData: this.deepCopy(item)
            });

        }).catch((e) => {
            $$.loading(false);
        });
    }

    validateExportNAASetting() {
        let item = this.exportData;
        let urlData = "/API/RuleApi/validateNaaExportSetting?sourceFlag=" + this.props.type;
        let option = {
            url: urlData,
            method: "get"
        };
        $$.loading(true);
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res.Success) {
                item.nnaConfigMissing = false;
            } else {
                item.nnaConfigMissing = true;
            }
            item.veoConfigMissing = false;
            item.naraConfigMissing = false;
            item.ConfigMissingLink = this.ConfigMissingLinks[1];
            this.setState({
                exportData: this.deepCopy(item)
            });
        }).catch((e) => {
            $$.loading(false);
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
            item.veoConfigMissing = false;
            item.nnaConfigMissing = false;
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
            url: "/api/StorageDevice/GetSftpAndAzureStorageInfos",
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

    onExportLocationOptionChanged = (args) => {
        this.exportData.exportLocationOption = args;
        this.setState({
            exportData: this.deepCopy(this.exportData),
            noLocation: false,
            isLocationValidate: false,
            noSelectNode: false,
        });
    }

    onLocationChange = (args) => {
        this.exportData.storageId = args.newValue.Id;
        this.exportData.storageName = args.newValue.Name;
        this.setState({ exportData: this.deepCopy(this.exportData), storage: args.newValue, });
    }

    onLocationPathChanged = (value) => {
        this.exportData.locationPath = value;
        this.setState({
            exportData: this.deepCopy(this.exportData),
            isLocationValidate: false,
        })
    }

    onActionCustomValidate = () => {
        let isValid = true;

        // Export to location
        if (LicenseHelper.EnableRecordsArchiver() && (this.exportData.isExportOnly || this.exportData.isExport) && this.exportData.selectExport.id) {
            if (this.state.exportData.exportLocationOption == ExportLocationOption.Storage) {
                if (!$$.verify(this.allValidation)) {
                    isValid = false;
                }
            } else if (this.state.exportData.exportLocationOption == ExportLocationOption.SPLibOrFolder) {
                if (!this.state.exportData.locationPath) {
                    isValid = false;
                    this.setState({ noLocation: true });
                } else {
                    this.setState({ noLocation: false });
                }
            } else {
                if (!this.nodeItem) {
                    isValid = false;
                    this.setState({ noSelectNode: true });
                } else {
                    this.setState({ noSelectNode: false });
                }
            }
        }
        
        return isValid;
    }

    //test 按钮
    checkLocation = () => {
        this.onActionCustomValidate();

        $$.loading(true);
        let urlData = "/api/RecordsExplorerApi/CheckSPLocation4Rule";
        let option = {
            url: urlData,
            method: "POST",
            data: {
                LocationPath: this.state.exportData.locationPath,
                SPAccount: null
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res != "") {
                this.setState({
                    isLocationValidate: true,
                    locationValidateMsg: RMResx.RM_JS_CP_ES_SuccessToValidateDBSettings,
                    locationValidateType: "success"
                });
            } else {
                this.setState({
                    isLocationValidate: true,
                    locationValidateMsg: RMResx.RM_JS_Rule_SPDestUrlError,
                    locationValidateType: "error"
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onDestActiveTabChange = (index) => {
        this.setState({
            destinationActiveTab: index,
            noSelectNode: false,
        });
    }

    onDestTreeSelectedChanged = (nodeItem) => {
        this.nodeItem = nodeItem;
        this.exportData.nodeItem = nodeItem;
        this.exportData.nodeItemStr = JSON.stringify(this.ruleMoveTree.getTreeData());
        this.setState({
            exportData: this.deepCopy(this.exportData),
            noSelectNode: false,
        });
    }

    onDestTreeSelectedChangedForTeams = (nodeItem) => {
        this.nodeItem = nodeItem;
        this.exportData.nodeItem = nodeItem;
        this.exportData.nodeItemStr = JSON.stringify(this.ruleMoveTeamsTree.getTreeData());
        this.setState({
            exportData: this.deepCopy(this.exportData),
            noSelectNode: false,
        });
    }

    renderExportLocationForTeams = () => {
        return (
            <div className="flex flex-column margin-top-s">
                <span className="ra-createRule-title-exp" tabIndex={0}>
                    {RMResx.RM_RDM_CreateRule_Title_ExportTo}
                </span>
                <div role="radioGroup" aria-label={RMResx.RM_RDM_CreateRule_Title_ExportTo} className="margin-bottom-s">
                    <R.Validation>
                        <div ref={r => this.allValidation = r} style={{ marginTop: 10 }}>
                            <R.Validation
                                element="Combobox"
                                require={RMResx.RM_AR_CP_Common_SelEmpty}
                            >
                                <R.Combobox
                                    id="raExportLocationCom"
                                    tooltipField="Name"
                                    width='74%'
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
                                />
                            </R.Validation>
                            <input type="hidden" name={"storageId"} value={this.state.exportData.storageId || this.state.currentStorageId} />
                        </div>
                    </R.Validation>
                </div>
            </div>
        );
    }

    renderExportTree = () => {
        const isSupportTeamsTree = checkPermission("Source_Teams", RM.UserResources) && LicenseHelper.HasUpgradeTeams();
        if (isSupportTeamsTree) {
            return (
                <div style={{ padding: "12px 20px" }}>
                    <R.Tabcontrol active={this.state.destinationActiveTab} onChange={this.onDestActiveTabChange}>
                        <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_SharePoint_Tab}>
                            <div className="destination-tab">  
                                <SPDestinationTree
                                    ref={r => this.ruleMoveTree = r}
                                    treeData={this.state.destinationTreeData}
                                    mode={this.props.mode || TabIndex.Records}
                                    onSelectedNodeChanged={this.onDestTreeSelectedChanged} />
                            </div>
                        </R.TabPanel>
                        <R.TabPanel tab={RMResx.RM_JS_BCM_Explorer_Move_Teams_Tab}>
                            <div className="destination-tab">  
                                <TeamsDestinationTree
                                    ref={r => this.ruleMoveTeamsTree = r}
                                    treeData={this.state.destinationTreeDataForTeams}
                                    mode={this.props.mode || TabIndex.Records}
                                    onSelectedNodeChanged={this.onDestTreeSelectedChangedForTeams} />
                            </div>
                        </R.TabPanel>
                    </R.Tabcontrol>
                </div>
            );
        }

        return (
            <SPDestinationTree
                ref={r => this.ruleMoveTree = r}
                treeData={this.state.destinationTreeData}
                mode={this.props.mode || TabIndex.Records}
                onSelectedNodeChanged={this.onDestTreeSelectedChanged}
            />
        );
    }

    renderExportLocation() {
        if (this.props.type == Constants.RuleSourceTabIndex.Teams) {
            return this.renderExportLocationForTeams();
        }

        return (
            <div className="flex flex-column gap-s">
                <span className="ra-createRule-title-exp" tabIndex={0}>
                    {RMResx.RM_RDM_CreateRule_Title_ExportTo}
                </span>
                <div role="radioGroup" aria-label={RMResx.RM_RDM_CreateRule_Title_ExportTo} className="margin-bottom-s">
                    <R.Radio
                        name={`radioExportLocation${this.props.id}`}
                        text={RMResx.RM_RDM_CreateRule_Title_SelectStorage}
                        value={ExportLocationOption.Storage}
                        checked={this.state.exportData.exportLocationOption == ExportLocationOption.Storage}
                        onChange={this.onExportLocationOptionChanged}
                    />
                    {this.state.exportData.exportLocationOption == ExportLocationOption.Storage && (
                        <R.Validation>
                            <div ref={r => this.allValidation = r} style={{ marginTop: 10, marginLeft: 28 }}>
                                <R.Validation
                                    element="Combobox"
                                    require={RMResx.RM_AR_CP_Common_SelEmpty}
                                >
                                    <R.Combobox
                                        id="raExportLocationCom"
                                        tooltipField="Name"
                                        width='74%'
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
                                    />
                                </R.Validation>
                                <input type="hidden" name={"storageId"} value={this.state.exportData.storageId || this.state.currentStorageId} />
                            </div>
                        </R.Validation>
                    )}

                    <div className="margin-top-m">
                        <R.Radio
                            name={`radioExportLocation${this.props.id}`}
                            text={RMResx.RM_RDM_CreateRule_Title_ExportSPLibOrFolder}
                            value={ExportLocationOption.SPLibOrFolder}
                            checked={this.state.exportData.exportLocationOption == ExportLocationOption.SPLibOrFolder}
                            onChange={this.onExportLocationOptionChanged}
                        />
                        {this.state.exportData.exportLocationOption == ExportLocationOption.SPLibOrFolder && (
                            <div className="sub-options-container">
                                <div className="flex">
                                    <R.Input
                                        id="raCrSpoLocationPathIpt"
                                        className="location-path"
                                        type="text"
                                        aria-label={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                        placeholder={RMResx.RM_JS_BCM_Explorer_Move_SpecifyLocation_SP_WaterMark}
                                        value={this.state.exportData.locationPath}
                                        onChange={this.onLocationPathChanged}
                                        onBlur={this.onActionCustomValidate}
                                    />
                                    <R.Button
                                        className="margin-left-s"
                                        text={RMResx.RM_RDM_CreateRule_Test}
                                        onClick={this.checkLocation}
                                    />
                                </div>
                                <$g.ValidationMsg show={this.state.noLocation}>
                                    {RMResx.RM_JS_RDM_CreateRule_Validation_NoInputLocaltion}
                                </$g.ValidationMsg>
                                <div id='location-vlidat-msg'>
                                    <R.Messagebar
                                        message={this.state.locationValidateMsg}
                                        status={{ show: this.state.isLocationValidate }}
                                        classify={this.state.locationValidateType}
                                        onClose={() => this.setState({ isLocationValidate: false })} />
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="margin-top-m">
                        <R.Radio
                            name={`radioExportLocation${this.props.id}`}
                            text={RMResx.RM_RDM_CreateRule_Title_ExportSPLibOrFolder_FromTree}
                            value={ExportLocationOption.SPLibOrFolderFromTree}
                            checked={this.state.exportData.exportLocationOption == ExportLocationOption.SPLibOrFolderFromTree}
                            onChange={this.onExportLocationOptionChanged}
                        />
                         <div className={`ra-tree ${this.state.exportData.exportLocationOption == ExportLocationOption.SPLibOrFolderFromTree ? "block" : "none"}`}>
                            <div className="ra-tree-container">
                                {this.renderExportTree()}
                            </div>
                            <$g.ValidationMsg show={this.state.noSelectNode}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_NoSelectTreeNode}
                            </$g.ValidationMsg>
                        </div>
                    </div>
                </div>
            </div>
        )
    }
    
    renderExportContent(){
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
                            this.state.exportData.exportTypes.slice(0, 4),
                            this.exportData.selectExport)}
                        disabled={this.state.elementDisabled}
                        onChange={this.exportTypeSelectChanged}
                    />
                </div>
            </div>
            <div className="ra-validation rm_export_error">
                <$g.ValidationMsg show={this.state.exportData.noExportTypeValue}>
                    {RMResx.RM_JS_RDM_CreateRule_Validation_noExportValue}
                </$g.ValidationMsg>
                <div
                    className={(this.state.exportData.veoConfigMissing) ? "block ra-rule-tips" : "none ra-rule-tips"}>
                    {RMResx.RM_ES_VEO_ConfigMissing.split("{0}")[0]}
                    <a className="ra-link-a ra-cursor-pointer" tabIndex="0" onClick={this.gotoExportSetting.bind(this)} onKeyDown={this.gotoExportSettingByKey.bind(this)}>{RMResx.RM_ES_VEO_ConfigMissingLink}</a>
                    {RMResx.RM_ES_VEO_ConfigMissing.split("{0}")[1]}
                </div>
                <div
                    className={(this.state.exportData.nnaConfigMissing) ? "block ra-rule-tips" : "none ra-rule-tips"}>
                    {RMResx.RM_ES_NAA_ConfigMissing.split("{0}")[0]}
                    <a className="ra-link-a ra-cursor-pointer" tabIndex="0" onClick={this.gotoExportSetting.bind(this)} onKeyDown={this.gotoExportSettingByKey.bind(this)}>{RMResx.RM_ES_NAA_ConfigMissingLink}</a>
                    {RMResx.RM_ES_NAA_ConfigMissing.split("{0}")[1]}
                </div>
                <div
                    className={(this.state.exportData.naraConfigMissing) ? "block ra-rule-tips" : "none ra-rule-tips"}>
                    {RMResx.RM_ES_NARA_ConfigMissing.split("{0}")[0]}
                    <a className="ra-link-a ra-cursor-pointer" tabIndex="0" onClick={this.gotoExportSetting.bind(this)} onKeyDown={this.gotoExportSettingByKey.bind(this)}>{RMResx.RM_ES_NARA_ConfigMissingLink}</a>
                    {RMResx.RM_ES_NARA_ConfigMissing.split("{0}")[1]}
                </div>
            </div>
            {/* Export to location */}
            {LicenseHelper.EnableRecordsArchiver() && (this.exportData.isExportOnly || this.exportData.isExport) && this.exportData.selectExport.id > 0 && this.renderExportLocation()}
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