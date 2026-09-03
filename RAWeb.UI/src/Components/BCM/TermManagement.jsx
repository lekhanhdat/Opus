import { Prompt } from 'react-router';
import { bindEvents, setCheckedStatus, getRequestVerificationToken, showToast, LicenseHelper, isShowActionByDC } from "../../Utilities/CommonUtil";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import TreeNodeContent from "../Common/Tree/NodeContents/TermManagementNodeContent";
import CreateRule from "../Common/RuleItem/CreateRule";
import RuleDetail from "../Common/RuleDetail/Index";
import "../../Less/BCM/tm.less";
import RouterUrls from "../../Constants/RouterUrls";
import { checkPermission } from '../../Utilities/permissionManager';
import StringUtil from '../../Utilities/StringUtil';
import ManageSpecificTermStoreTable from './ManageSpecificTermStoreTable';
import Enviroments from "../../Constants/Enviroments";
import { Messagebox } from '../Common/Messagebox';
import React from 'react';
import AIRecommendationsDialog from './AIRecommendationsDialog';
import { RAMessageType } from './ContentRepositoryManagement/Common/CRMCommonUtil';
import { UsageLimitType } from '../ML/MachineLearning/Zero/Config/Constants';

const SiteAction = {
    Add: 1,
    Delete: 2,
    Update: 3
};
const EnforceRetentionType = {
    SP: 1,
    EXO: 2,
    OneDrive: 4,
    Teams: 16,
};
const ObjectLevel = {
    //None: { name: RMResx.RM_JS_Rule_ObjectLevel_None, value: 0 },
    //WebApplication: { name: RMResx.RM_JS_Rule_ObjectLevel_WebApplication, value: 1 },
    SiteCollection: { name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection, value: 2 },
    Site: { name: RMResx.RM_JS_Rule_ObjectLevel_Site, value: 4 },
    List: { name: RMResx.RM_JS_Rule_ObjectLevel_List, value: 8 },
    Folder: { name: RMResx.RM_JS_Rule_ObjectLevel_Folder, value: 16 },
    Item: { name: RMResx.RM_JS_Rule_ObjectLevel_Item, value: 32 },
    Document: { name: RMResx.RM_JS_Rule_ObjectLevel_Document, value: 64 },
    // PhysicalBox: {name: 'PhysicalBox', value: 10001},
    // PhysicalFile: {name: 'PhysicalFile', value: 1000},
    //Attachment: { name: RMResx.RM_JS_Rule_ObjectLevel_Attachment, value: 128 },
    //DocumentVersion: { name: RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion, value: 256 },
    //ItemVersion: { name: RMResx.RM_JS_Rule_ObjectLevel_ItemVersion, value: 512 }
};
const TermExpireSettingType = {
    TakeEffectFrom: "0",
    RetireAfter: "1",
    ActiveFromTo: "2"
};

const ExportTermsStatus = {
    None: 0,
    InProgress: 1,
    Finished: 2
};

const TermSyncOptionType = {
    None: 0,
    Specified: 1,
    All: 2
}

const TermImportOptionType = {
    FromTemplateFile: 1,
    FromGoogleLabel: 2
}

const SiteType = {
    Online: 0,
    OnPrem: 1,
    Google: 2
}

const SpecifiedType = {
    MMS: 1,
    GoogleTenant: 2
}

const isMultiGeoMainDC = isShowActionByDC();
export default class TermManagement extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.hasOpusGoogleLicense = LicenseHelper.HasOpusGoogleLicense();
        this.hasOpusILOrSOLicense = LicenseHelper.HasOpusILLicense() || LicenseHelper.HasOpusSOLicense();
        this.initBindings();
        this.getRecordsRuleListFromDA();
        this.getTreeData();
        this.getAllGoogleTenants();
        this.getAllTermGroups();
        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            treeData: [],
            selectedItem: null, //selected item(origin item)
            currentItem: null,  //current selected item view model, clone from "selectedItem"
            itemSettingChanged: false, //if current selected item's setting changed
            showImportPanel: { show: false },
            showInheritBar: false,
            isTermBreakInherit: false,
            enforceRetention: false,
            mmsMsgbar: {
                type: "error",
                show: false,
                msg: ""
            },
            siteInfoResult: [],
            selectedRuleLevel: {},
            rulesGroupByLevel: {},
            termRulesGroupByLevel: {},
            termExpireSettings: this.getDefaultTermExpireSettings(),
            showCreateRuleDialog: false,
            createRuleUrl: "",
            createRuleStyle: { width: "1060px", height: "500px", borderWidth: "0" },
            ruleDetailId: null,
            currentRowRuleLevelId: '',
            exoRetentionActionLabel: '',
            spRetentionActionLabel: '',
            teamsRetentionActionLabel: '',
            isTeamsRetentionChecked: false,
            noExoLabelValueVerify: false,
            noSpLabelValueVerify: false,
            noTeamsLabelValueVerify: false,
            teamsRetentionActionLabelDis: false,
            noSelectSpOrExoVerify: false,
            isTeamsRetentionDis: false,
            isSpRetentionDis: false,
            isExoRetentionDis: false,
            exoRetentionActionLabelDis: false,
            oneDriveRetentionActionLabel: '',
            noOneDriveLabelValueVerify: false,
            oneDriveRetentionActionLabelDis: false,
            isOneDriveRetentionDis: false,
            ruleLevelItems: this.getRuleLevelItems(),
            showRuleLevelOptions: false,
            scUrlInput: '',
            files: [],
            termImportType: TermImportOptionType.FromTemplateFile,
            tenantList: [],
            termGroupList: [],
            importTenantSelected: {},
            settingTenantSelected: [],
            termGroupSelected: null,
            noTenantSelectedVerify: false,
            noTermGroupSelectedVerify: false,
            showAIRecommendationDialog: false,
            aiRecommendationResult: null,
        };
        this.allRules = {};
        this.creatingTermRule = null;
        this.newTermRuleNum = -1;
        this.ruleLevels = this.getRuleLevelSource();
        this.treeContext = this.getTreeContext();
        this.defaultZone = RM.TimeUtil.getGlobalTimezoneInfo();
        window.RM.TM = { hideCreateRulePopup: this.onCreateNewRuleDialogClose };
        this.datetimeFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.termStoreTableId = "raTermStoreTable";
        this.termStoreColumns = this.getTermStoreTableColumns();
        this.createRuleComponentId = "raCreateRuleItem";
        this.uploaderRef = React.createRef();
        this.creatingTermRuleRef = React.createRef(null);
        this.aiRecommendationsDialogRef = React.createRef(null);
    }

    componentUpdate(_prevProps, prevState) {
        if (prevState.termImportType !== this.state.termImportType && this.state.termImportType == TermImportOptionType.FromGoogleLabel) {
            this.getAllGoogleTenants();
        }
    }

    componentInit() {
        document.addEventListener('click', this.hideRuleLevelPopUp);
    }

    componentDestroy() {
        document.removeEventListener("click", this.hideRuleLevelPopUp, false);
    }

    initBindings() {
        bindEvents(this, "onSearch", "handleTermSync", "handleTermExport", "handleTermImport", "handleExportBtn",
            "synchronise", "showMessageTip", "hideMessageTip", "handleImportOKClick", "handleImportCancelClick",
            "onDescriptionChange", "onTermInheritClick", "onTermBreakClick",
            "onSaveSettingClick", "onCancelChangedClick", "onChooseMMSChanged", "onAddSiteInfo", "handleRuleLevelChange",
            "onAddRuleLevelClick", "showMmsMsgbar", "hideMmsMsgbar", "onCreateNewRuleClick", "handleTermRuleOrderChanged",
            "handleTermRuleNameChanged", "onTermRuleDelClick", "onTermRuleViewClick", "onEnforceRetentionChange", "handleShowRuleLevelList", "handleDelSiteClick",
            "onNoTermExpireSettingChange", "handleEffectFromChanged", "handleRetireAfterChanged", "handleActiveFromChanged",
            "handleActiveToChanged", "onExpireOptionChanged", "onRuleOperated", "onCreateNewRuleDialogClose", "hideRuleLevelPopUp",
            "jumpExportSettings", 'exoRetentionActionLabelChange', 'exoRetentionActionLabelBlur', 'spRetentionActionLabelChange', 'spRetentionActionLabelBlur', 'spRetentionChange', 'exoRetentionChange',
            'changeLabelSureClick', 'changeLabelCancleClick', 'oneDriveRetentionActionLabelChange', 'oneDriveRetentionActionLabelBlur', 'oneDriveRetentionChange', 'handleExpireOptionChange','onAdvanceSettingsChange',
            "handleTenantSelect", "handleTermGroupSelect", "onChooseImportType", "onChooseMMSChanged", "onChooseGoogleTenantChanged", "saveTermGroup");
    }

    compareUrl(sUrl, oUrl) {
        var reg = /[/]$/gi;
        sUrl = sUrl.toLowerCase().replace(reg, "");
        oUrl = oUrl.toLowerCase().replace(reg, "");
        return sUrl == oUrl;
    }

    copyProps(fromObj, toObj, propNames) {
        for (var i = 0; i < propNames.length; i++) {
            toObj[propNames[i]] = fromObj[propNames[i]];
        }
    }

    cloneObject(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    deleteSC(siteInfo) {
        siteInfo.Action = SiteAction.Delete;
        this.setState({
            itemSettingChanged: true
        });
    }

    existMmsInfo(siteInfo) {
        var exist = false;
        if (siteInfo == null) {
            return exist;
        }
        for (var i = 0; i < this.state.siteInfoResult.length; i++) {
            var info = this.state.siteInfoResult[i];
            if (info.Action != SiteAction.Delete &&
                info.TermStoreId.toLocaleLowerCase() == siteInfo.TermStoreId.toLocaleLowerCase()) {
                exist = true;
                break;
            }
        }
        return exist;
    }

    getAddedSiteCount() {
        let sourceData = this.state.siteInfoResult;
        let count = 0;
        for (var i = 0; i < sourceData.length; i++) {
            if (sourceData[i].SiteType != SiteType.Google && sourceData[i].Action != SiteAction.Delete) {
                count++;
            }
        }
        return count;
    }

    getTermStoreTableColumns() {
        return [
            {
                header: StringUtil.trimEndColon(RMResx.RM_TM_SiteCollection),
                width: [200]
            },
            {
                header: RMResx.RM_TM_ManagedMetadataService,
                width: [200]
            }, {
                header: "",//RMResx.RM_TM_Action
                width: 80,
            }];
    }

    termRulesAllHasRule() {
        let allHasRule = true;
        for (let level in this.state.termRulesGroupByLevel) {
            if (this.state.termRulesGroupByLevel[level]) {
                let termRules = this.state.termRulesGroupByLevel[level];
                if (termRules.length > 0) {
                    for (let tr of termRules) {
                        if (!tr.RuleName) {
                            allHasRule = false;
                        }
                    }
                }
            }
        }
        return allHasRule;
    }

    getTermRules() {
        let trList = [];
        for (let level in this.state.termRulesGroupByLevel) {
            if (this.state.termRulesGroupByLevel[level]) {
                let termRules = this.state.termRulesGroupByLevel[level];
                if (termRules.length > 0) {
                    for (let tr of termRules) {
                        if (tr.RuleName) {
                            trList.push(tr);
                        }
                    }
                }
            }
        }
        return trList;
    }

    getDefaultTermExpireSettings() {
        return {
            noExpireSetting: true,
            selectedOption: null,
            fromDateTime: null,
            endDateTime: null,
            timeZone: this.defaultZone
        };
    }

    getDisplayStyle(show) {
        return { display: show ? "block" : "none" };
    }

    getMmsInfo(uniqueId) {
        this.setState({
            siteInfoResult: [],
            mmsMsgbar: {
                show: false
            }
        });
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetRelativedMmsInfo",
            //contentType: 'application/json;charset=utf-8',
            data: "termGroupId=" + uniqueId,
            async: true,
            beforeSend: () => {
                this.settingsLoading(true);
            },
            complete: () => {
                this.settingsLoading(false);
            },
            success: (data) => {
                const specificTermStore = [];

                data?.forEach(item => {
                    if (item.SiteType != SiteType.Google) {
                        specificTermStore.push(item);
                    }
                });
                
                this.setState({ siteInfoResult: data, scUrlInput: "" },
                    () => {this.dispatch(this.termStoreTableId, specificTermStore, this.termStoreColumns)}
                );
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    getRuleLevelSource() {
        var levelSource = [];
        for (var key in ObjectLevel) {
            if (ObjectLevel[key]) {
                levelSource.push({ "name": ObjectLevel[key].name, "value": key });
            }
        }
        return levelSource;
    }

    getRuleLevelItems() {
        var levelSource = [];
        for (var key in ObjectLevel) {
            if (ObjectLevel[key]) {
                var ruleName = ObjectLevel[key].name;
                levelSource.push(
                    // { "name": ObjectLevel[key].name, "value": key }
                    {
                        checked: false,
                        name: ruleName,
                        value: key,
                        disabled: false,
                        tooltip: ruleName,
                        // group: 'group1',
                    }
                );
            }
        }
        return levelSource;
    }

    getTermActiveOptionItems() {

        return [
            { text: StringUtil.trimEndColon(RMResx.RM_TM_STime), value: TermExpireSettingType.TakeEffectFrom, checked: false },
            { text: StringUtil.trimEndColon(RMResx.RM_TM_ETime), value: TermExpireSettingType.RetireAfter, checked: false },
            { text: RMResx.RM_TM_Title_TimeRange, value: TermExpireSettingType.ActiveFromTo, checked: false }
        ];
    }

    getSelectedTermActiveOptionItem(selectedValue)
    {
        var selectedItem = this.getTermActiveOptionItems().find(o => o.value == selectedValue);
        if(selectedItem !==  undefined)
        {
            selectedItem.checked = true;
        }
        return selectedItem;
    }

    getRecordsRuleListFromDA() {
        $$.loading(true);
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetRecordsRuleListFromDA",
            data: [],
            async: true,
            beforeSend: () => {
                // this.settingsLoading(true);
            },
            complete: () => {
                // this.settingsLoading(false);
            },
            success: (data) => {
                $$.loading(false);
                let ruleInfos = $.parseJSON(data);  // Fortify Issue Type: JSON Injection; Sink Details: get rule list; Ignore Reason: 前后台对象存在对应关系
                this.allRules = {};
                let rulesGroup = {};
                for (let key in ObjectLevel) {
                    rulesGroup[ObjectLevel[key].value] = [];
                }
                for (let rule of ruleInfos) {
                    this.allRules[rule.RuleId] = rule;
                    rulesGroup[rule.RuleLevel].push(rule);
                }
                this.setState({ rulesGroupByLevel: rulesGroup, showCreateRuleDialog: false });
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    getAvailableRuleList() {
        $$.loading(true);
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetAvailableRuleList",
            data: "termId=" + this.state.currentItem.Id,
            async: true,
            beforeSend: () => {
                // this.settingsLoading(true);
            },
            complete: () => {
                // this.settingsLoading(false);
            },
            success: (data) => {
                $$.loading(false);
                let result = $.parseJSON(data);     // Fortify Issue Type: JSON Injection; Sink Details: get rule list; Ignore Reason: 前后台对象存在对应关系
                let ruleInfos = result.allRules;
                let availableRulesInfos = result.availableRules;
                this.allRules = {};
                let rulesGroup = {};
                for (let key in ObjectLevel) {
                    rulesGroup[ObjectLevel[key].value] = [];
                }
                for (let rule of ruleInfos) {
                    this.allRules[rule.RuleId] = rule;
                }
                for (let rule of availableRulesInfos) {
                    rulesGroup[rule.RuleLevel].push(rule);
                }
                this.setState({ 
                    rulesGroupByLevel: rulesGroup, 
                    showCreateRuleDialog: false,
                    itemSettingChanged: this.state.currentRowRuleLevelId == this.creatingTermRule.RuleLevel
                });
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    getParentInhertSetting(oItem) {
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetParentInhertSetting",
            data: "termId=" + oItem.Id,
            async: true,
            beforeSend: () => {
                this.settingsLoading(true);
            },
            complete: () => {
                this.settingsLoading(false);
            },
            success: (data) => {
                let updateState = {
                    termSettingDisabled: false,
                    showInheritBar: false,
                    isTeamsRetentionDis: false,
                    isSpRetentionDis: false,
                    isExoRetentionDis: false,
                    exoRetentionActionLabelDis: false,
                    noSpLabelValueVerify: false,
                    noTeamsLabelValueVerify: false,
                    teamsRetentionActionLabelDis: false,
                    noExoLabelValueVerify: false,
                    isOneDriveRetentionDis: false,
                    oneDriveRetentionActionLabelDis: false,
                    noOneDriveLabelValueVerify: false,
                };
                let newItem = $.parseJSON(data);    // Fortify Issue Type: JSON Injection; Sink Details: get parent data; Ignore Reason: 前后台对象存在对应关系
                if ("" == newItem.message) {//后台异常时的情况
                    updateState.termSettingDisabled = true;
                } else {


                    let rulesGroup = {};
                    for (let key in ObjectLevel) {
                        rulesGroup[ObjectLevel[key].value] = [];
                    }
                    for (let rule of newItem.associateAvailableRule) {
                        this.allRules[rule.RuleId] = rule;
                        rulesGroup[rule.RuleLevel].push(rule);
                    }
                    this.setState({ rulesGroupByLevel: rulesGroup, showCreateRuleDialog: false });


                    updateState.termRulesGroupByLevel = this.getTermRulesGroupsByLevel(newItem.rule);
                    let term = newItem.term;
                    if (term == null) {
                        //自身以及parent没有rule或者retention
                        updateState.enforceRetention = false;
                    } else {
                        //if (term.HaveParentSetting && !oItem.IsRootTerm) {
                        if (!oItem.IsRootTerm) {
                            updateState.showInheritBar = true;
                            if (oItem.BreakInheritFromParent || (oItem.RuleInfo != null && oItem.RuleInfo != "")) {
                                updateState.isTermBreakInherit = true;
                            } else {
                                updateState.isTermBreakInherit = false;
                                updateState.termSettingDisabled = true;
                                updateState.isTeamsRetentionDis = true;
                                updateState.isSpRetentionDis = true;
                                updateState.isExoRetentionDis = true;
                                updateState.exoRetentionActionLabelDis = true;
                                updateState.isOneDriveRetentionDis = true;
                                updateState.oneDriveRetentionActionLabelDis = true;
                                updateState.teamsRetentionActionLabelDis = true;
                                this.copyProps(
                                    term,
                                    this.state.selectedItem,
                                    ["EnforceRetention"]);
                                this.copyProps(
                                    term,
                                    this.state.currentItem,
                                    ["EnforceRetention"]);
                            }
                        }
                        this.getEnforceRetentionCheckedStatue(term);
                    }
                }
                this.setState(updateState, () => {
                    this.resetTermRulesOrder();
                });
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    async getAllGoogleTenants() {
        if (!this.hasOpusGoogleLicense || !checkPermission("Source_Google")) {
            return;
        }

        const option = {
            url: "/api/TermManagementApi/GetAllGoogleTenants",
            method: "GET"
        }
        const data = await fetchUtility(option);
        const { currentItem } = this.state;
        const tenantList = data.map(item => ({
            ...item,
            checked: currentItem ? currentItem?.UniqueId == item.TermGroupId : false,
            name: item.DisplayName,
            title: item.DisplayName,
            value: item.SiteUrl
        }));
        this.setState({tenantList})
    }

    async getAllTermGroups() {
        if (!this.hasOpusGoogleLicense || !checkPermission("Source_Google")) {
            return;
        }
        
        const option = {
            url: "/api/TermManagementApi/GetAllTermGroups",
            method: "GET"
        }
        const data = await fetchUtility(option);
        let termGroupList = [];
        Object.entries(data).forEach(([id, text]) => {
            termGroupList.push({
                checked: false,
                name: text,
                title: text,
                value: id
            })
        });
        this.setState({termGroupList})
    }

    getSiteInfo(url) {
        let sourceData = this.state.siteInfoResult;
        let result = null;
        for (var i = 0; i < sourceData.length; i++) {
            var sInfo = sourceData[i];
            if (this.compareUrl(sInfo.SiteUrl, url)) {
                result = sInfo;
                break;
            }
        }
        return result;
    }

    getTermRulesGroupsByLevel(termRules) {
        let termRulesGroupByLevel = {};
        for (let key in ObjectLevel) {
            termRulesGroupByLevel[ObjectLevel[key].value] = [];
        }
        for (let termRule of termRules) {
            let ruleItem = this.allRules[termRule.RuleId];
            if (ruleItem) {
                termRulesGroupByLevel[ruleItem.RuleLevel].push(termRule);
            }
        }
        return termRulesGroupByLevel;
    }

    getTermRuleLevelValue(termRule) {
        if (termRule.RuleId) {
            return this.allRules[termRule.RuleId].RuleLevel;
        } else {
            return ObjectLevel[termRule.RuleLevel].value;
        }
    }

    getTermRuleLevelName(ruleLevel) {
        switch (ruleLevel) {
            case 1:
                return RMResx.RM_JS_Rule_ObjectLevel_WebApplication;
            case 2:
                return RMResx.RM_JS_Rule_ObjectLevel_SiteCollection;
            case 4:
                return RMResx.RM_JS_Rule_ObjectLevel_Site;
            case 8:
                return RMResx.RM_JS_Rule_ObjectLevel_List;
            case 16:
                return RMResx.RM_JS_Rule_ObjectLevel_Folder;
            case 32:
                return RMResx.RM_JS_Rule_ObjectLevel_Item;
            case 64:
                return RMResx.RM_JS_Rule_ObjectLevel_Document;
            case 128:
                return RMResx.RM_JS_Rule_ObjectLevel_Attachment;
            case 256:
                return RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion;
            case 512:
                return RMResx.RM_JS_Rule_ObjectLevel_ItemVersion;
            case 0:
            default:
                return RMResx.RM_JS_Rule_ObjectLevel_None;
        }
    }

    getTermTimeSettings(oItem) {
        $.ajax({
            type: "POST",
            url: "/api/TermManagementApi/GetTermTimeSettings",
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify(oItem.Id),
            async: true,
            beforeSend: () => {
                this.settingsLoading(true);
            },
            complete: () => {
                this.settingsLoading(false);
            },
            success: (data) => {
                let expireSetting = $.parseJSON(data),  // Fortify Issue Type: JSON Injection; Sink Details: get term data; Ignore Reason: 前后台对象存在对应关系
                    noExpireSetting = false,
                    timeZone = this.defaultZone,
                    hasBeginTime = expireSetting.TermExpirationFrom > 0,
                    hasEndTime = expireSetting.TermExpirationTo > 0,
                    beginTime = hasBeginTime ? new Date(expireSetting.TermExpirationFromStr) : null,
                    endTime = hasEndTime ? new Date(expireSetting.TermExpirationToStr) : null,
                    selectedOption = null;
                // if (expireSetting.TimeZoneId) {
                //     timeZone = RM.TimeUtil.getTimezoneInfo(expireSetting.TimeZoneId, expireSetting.IsDayLight);
                // }
                if (hasBeginTime && hasEndTime) {
                    selectedOption = TermExpireSettingType.ActiveFromTo;
                } else if (hasBeginTime) {
                    selectedOption = TermExpireSettingType.TakeEffectFrom;
                } else if (hasEndTime) {
                    selectedOption = TermExpireSettingType.RetireAfter;
                } else {
                    noExpireSetting = true;
                }
                this.originalEXOLabel = expireSetting.EXORetentionLabel;//从接口取到的label
                this.originalSPLabel = expireSetting.SPRetentionLabel;
                this.originalTeamsLabel = expireSetting.TeamsRetentionLabel;
                this.originalOneDriveLabel = expireSetting.OneDriveRetentionLabel;
                this.setState({
                    exoRetentionActionLabel: expireSetting.EXORetentionLabel,
                    spRetentionActionLabel: expireSetting.SPRetentionLabel,
                    teamsRetentionActionLabel: expireSetting.TeamsRetentionLabel,
                    oneDriveRetentionActionLabel: expireSetting.OneDriveRetentionLabel,
                    termExpireSettings: {
                        noExpireSetting: noExpireSetting,
                        selectedOption: selectedOption,
                        fromDateTime: beginTime,
                        endDateTime: endTime,
                        timeZone: timeZone
                    }
                });
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    getTreeContext() {
        let self = this;
        return {
            treeType: 1,    //1:TermManagement, 2:LocationManagement
            searchKey: "",
            singleSelection: true,
            nodeContentComponent: TreeNodeContent,
            recalculatePosition: true,
            transToTreeNodeObject(oitem) {
                let pagerByServer = !this.searchKey;
                let itemsCount =
                    (!pagerByServer || oitem.Type == "TermGroup")
                        ? (!oitem.subTerms ? 0 : oitem.subTerms.length)
                        : oitem.subTermCount;
                if(this.searchKey && oitem.Type == "Term")
                {
                    //search情况下，term展开子节点从后台现查
                    itemsCount = oitem.subTermCount;
                    pagerByServer = true;
                }
                return {
                    origin: oitem,
                    nodeKey: this.getNodeKey(oitem),
                    nodeType: oitem.Type,
                    text: oitem.Name,
                    disableSelect: this.isDisableSelect(oitem),
                    checked: self.state && this.getNodeKey(oitem) == this.getNodeKey(self.state.selectedItem),
                    expanded: !!this.searchKey || oitem.Type == "Root",
                    loaded: !!this.searchKey || oitem.subTermCount == 0 || !!oitem.subTerms,
                    hasChildren: itemsCount > 0,
                    items: oitem.subTerms,
                    isAllowEditName: true,
                    pagerByServer: pagerByServer,
                    itemsCount: itemsCount,
                    pagerIndex: 0,
                    pagerSize: 15,
                    enableContextMenu: true
                };
            },
            isDisableSelect(oitem){
                return oitem.Type == "Root";
            },
            getNodeKey(oitem) {
                if (oitem) {
                    return oitem.Type == "TermGroup" || oitem.Type == "TermSet" ? oitem.UniqueId : oitem.Id;
                } else {
                    return null;
                }
            },
            sortChild(a, b) {
                if (a.Type == "TermGroup" || a.Type == "Term" ||  a.Name == b.Name) {
                    return 0;
                } else if (a.Name.toLowerCase() > b.Name.toLowerCase()) {
                    return 1;
                } else {
                    return -1;
                }
            },
            onLoadNodes(parentItem, funcSuccess, funcFail) {
                let oItem = parentItem.origin;
                var nId = oItem.Type == "TermGroup" ? oItem.UniqueId : oItem.Id;
                $.ajax({
                    type: "GET",
                    url: "/api/TermManagementApi/GetChildrenByDB",
                    contentType: "application/json;charset=utf-8",
                    data: "PageIndex=" + (parentItem.pagerIndex + 1) + "&PageSize=" + parentItem.pagerSize
                        + "&NodeId=" + nId + "&NodeType=" + oItem.Type,
                    async: true,
                    //beforeSend: function () {
                    //    $$.loading(true);
                    //},
                    //complete: function () {
                    //    $$.loading(false);
                    //},
                    success: function (data) {
                        let items = $.parseJSON(data);  // Fortify Issue Type: JSON Injection; Sink Details: get tree data; Ignore Reason: 前后台对象存在对应关系
                        //oItem.subTerms = items;
                        //oItem.subTermCount = items.length;
                        funcSuccess(items);
                    },
                    error: function (msg) {
                        funcFail(msg.responseText);
                    },
                    dataType: "json"
                });
                //return children node items
                return [];
            },
            confirmOnNodeSelected: (item, funcAllow) => this.onNodeSelected(item.origin, funcAllow),
            refreshSelectedNodeInfo: this.refreshSelectedNodeInfo.bind(this),
            showMessageTip: this.showMessageTip,
            hideMessageTip: this.hideMessageTip,
            history: this.props.history,
            updateTermGroupList: () => this.getAllTermGroups(), // Update Term group list when creating, updating, deleting
        };
    }

    getTreeData() {
        $.ajax({
            type: "GET",
            url: "/api/TermManagementApi/GetChildrenByDB",
            contentType: "application/json;charset=utf-8",
            data: [],
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                setTimeout(() => {
                    $$.loading(false);
                }, 500);
            },
            success: (data) => {
                this.treeContext.searchKey = "";
                this.resetTreeData(data);
            },
            error: (msg) => {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    handleEffectFromChanged(args) {
        let expireSettings = this.state.termExpireSettings;
        if (expireSettings.selectedOption == TermExpireSettingType.TakeEffectFrom
            && (expireSettings.fromDateTime != args.newValue)) {
            // expireSettings.timeZone = args.newValue.zone;
            expireSettings.fromDateTime = args.newValue;
            this.setState({
                itemSettingChanged: true,
                termExpireSettings: {...expireSettings}
            });
        }
    }

    handleRetireAfterChanged(args) {
        let expireSettings = this.state.termExpireSettings;
        if (expireSettings.selectedOption == TermExpireSettingType.RetireAfter
            && (JSON.stringify(args.newValue) != JSON.stringify(args.oldValue))) {
            // expireSettings.timeZone = args.newValue.zone;
            expireSettings.endDateTime = args.newValue;
            this.setState({
                itemSettingChanged: true,
                termExpireSettings: {...expireSettings}
            });
        }
    }

    handleActiveFromChanged(args) {
        let expireSettings = this.state.termExpireSettings;
        if (expireSettings.selectedOption == TermExpireSettingType.ActiveFromTo
            && (expireSettings.fromDateTime != args.newValue)) {
            // expireSettings.timeZone = args.newValue.zone;
            expireSettings.fromDateTime = args.newValue;
            this.setState({
                itemSettingChanged: true,
                termExpireSettings: {...expireSettings}
            });
        }
    }

    handleActiveToChanged(args) {
        let expireSettings = this.state.termExpireSettings;
        if (expireSettings.selectedOption == TermExpireSettingType.ActiveFromTo
            && expireSettings.endDateTime != args.newValue) {
            expireSettings.endDateTime = args.newValue;
            this.setState({
                itemSettingChanged: true,
                termExpireSettings: {...expireSettings}
            });
        }
    }

    handleImportCancelClick(e) {
        this.setState({
            showImportPanel: { show: false }, 
            termImportType: TermImportOptionType.FromTemplateFile,
            importTenantSelected: {},
            termGroupSelected: null,
            noTenantSelectedVerify: false,
            noTermGroupSelectedVerify: false,
        });
    }

    handleImportSaveClick = () => {
        const {termImportType} = this.state;
        switch (termImportType) {
            case TermImportOptionType.FromTemplateFile:
                this.handleImportFromTemplateFile();
                break;
            case TermImportOptionType.FromGoogleLabel:
                this.handleImportFromGoogleLabel();
                break;
        }
    }

    onShowImportPanel = () => {
        const tenantList = this.state.tenantList.map(tenant => ({
            ...tenant,
            checked: false,
        }))
        this.setState({tenantList})
    }

    onHideImportPanel = () => {
        const { currentItem } = this.state;
        const tenantList = this.state.tenantList.map(tenant => ({
            ...tenant,
            checked: currentItem?.UniqueId == tenant.TermGroupId,
            disabled: false
        }))
        this.setState({
            tenantList,
            termImportType: TermImportOptionType.FromTemplateFile,
            noTenantSelectedVerify: false,
        })
    }

    handleImportFromTemplateFile = () => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        const formData = new FormData();
        formData.append('fileUp', this.files.file, this.files.fileName);
        fetch('/api/TermManagementApi/ImportData', {
            method: 'POST',
            body: formData,
        })
            // .then(function (res) {
            //     //忽略这个，使用原生fetch需要多写一个then.
            //     return res.json();
            // })
            .then(function (data) {
                $$.loading(false);
                if (data) {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                }
                // return result;
            });
        this.setState({ showImportPanel: { show: false } });
    }

    handleImportFromGoogleLabel = async () => {
        const {importTenantSelected, termGroupSelected} = this.state;
        if (!Object.keys(importTenantSelected).length || !termGroupSelected) {
            this.setState({
                noTenantSelectedVerify: Boolean(!Object.keys(importTenantSelected).length),
                noTermGroupSelectedVerify: Boolean(!termGroupSelected)
            })
            return;
        }

        const option = {
            url: "/api/TermManagementApi/ImportGoogleData",
            data: {
                SyncOption: TermSyncOptionType.Specified,
                GoogleTenants: importTenantSelected,
                TermGroupId: termGroupSelected.value,
            }
        }
        $$.loading(true);
        const data = await fetchUtility(option);
        $$.loading(false);
        if (data && data?.MessageType == 0) {
            this.getTreeData();
            this.getAllGoogleTenants();
            
            let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>;
            showToast.success(content);

            const currentItem = RM.deepcopy(this.state.currentItem);
            if (currentItem && currentItem?.UniqueId == termGroupSelected.value) {
                currentItem.GoogleTermSyncOption = TermSyncOptionType.Specified;
            }

            this.setState({
                showImportPanel: { show: false }, 
                termImportType: TermImportOptionType.FromTemplateFile,
                importTenantSelected: {},
                termGroupSelected: null,
                noTenantSelectedVerify: false,
                noTermGroupSelectedVerify: false,
                currentItem
            });
        }
    }

    handleRuleLevelChange(args) {
        let ruleLevel = args.newValue.value;
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        let iLevel = ObjectLevel[ruleLevel].value;
        let termRules = allTermRules[iLevel];
        termRules.push({ Id: this.newTermRuleNum--, RuleLevel: ruleLevel });
        this.setState({
            itemSettingChanged: true,
            termRulesGroupByLevel: allTermRules,
            showRuleLevelOptions: !this.state.showRuleLevelOptions
        }, () => {
            this.resetTermRulesOrder();
        });
    }

    handleShowRuleLevelList(disabledStatus, e)
    {
        if(disabledStatus){
            e.preventDefault();
        } else {
            this.setState({ showRuleLevelOptions: !this.state.showRuleLevelOptions });
        }
        e.nativeEvent.stopImmediatePropagation();
    }

    hideRuleLevelPopUp()
    {
        this.setState({ showRuleLevelOptions: false });
    }

    handleExpireOptionChange(args) {
        let expireSettings = RM.deepcopy(this.state.termExpireSettings);
        expireSettings.selectedOption = args.newValue.value;
        expireSettings.fromDateTime = null;
        expireSettings.endDateTime = null;
        this.setState({
            itemSettingChanged: true,
            termExpireSettings: expireSettings
        });
    }

    handleTermRuleOrderChanged(args, termRule) {
        let newItem = args.newValue;
        if (termRule.RuleOrder != newItem.RuleOrder) {
            let oldOrder = args.oldValue.RuleOrder;
            let newOrder = args.newValue.RuleOrder;
            let iLevel = this.getTermRuleLevelValue(termRule);
            let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
            let termRules = allTermRules[iLevel];
            for (let tr of termRules) {
                if (tr.Id == termRule.Id) {
                    tr.RuleOrder = newOrder;
                } else {
                    if (newOrder > oldOrder) {
                        if (newOrder >= tr.RuleOrder && tr.RuleOrder > oldOrder) {
                            tr.RuleOrder -= 1;
                        }
                    } else {
                        if (newOrder <= tr.RuleOrder && tr.RuleOrder < oldOrder) {
                            tr.RuleOrder += 1;
                        }
                    }
                }
            }
            termRules.sort((a, b) => a.RuleOrder > b.RuleOrder ? 1 : -1);
            this.setState({ itemSettingChanged: true, termRulesGroupByLevel: allTermRules });
            setTimeout(()=>{ this.forceUpdate();},100);
        }
    }

    handleTermRuleNameChanged(args, termRule) {
        let newItem = args.newValue;
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        let termRules = allTermRules[newItem.RuleLevel];
        let ruleItem = termRules.find(o => o.RuleId == termRule.RuleId);

        if (ruleItem.RuleId != newItem.RuleId) {
            ruleItem.RuleId = newItem.RuleId;
            ruleItem.RuleName = newItem.RuleName;
            this.setState({ itemSettingChanged: true, termRulesGroupByLevel: allTermRules });
        }

    }

    handleDelSiteClick (siteInfo)
    {
        let siteItems = RM.deepcopy(this.state.siteInfoResult);
        let curSiteInfo = siteItems.find(o => o.SiteUrl == siteInfo.SiteUrl);
        if(curSiteInfo !== undefined)
        {
            curSiteInfo.Action = SiteAction.Delete;
        }
        let specificTermStore = siteItems.filter(item => item.SiteType != SiteType.Google);
        this.setState({
            siteInfoResult: siteItems,
            itemSettingChanged: true
        }, () => {
            this.dispatch(this.termStoreTableId, specificTermStore, this.termStoreColumns);
        });
    }

    checkTermExportStatusTimer() {
        let that = this;
        var $hExportStatusKey = $("#exportFlag");
        setTimeout(() => {
            $.ajax({
                url: "/api/TermManagementApi/CheckExportTermStatus",
                async: true,
                cache: false,
                type: "get",
                data: "exportUniqueId=" + $hExportStatusKey.val(),
                success: (data) => {
                    if (data == ExportTermsStatus.InProgress) {
                        that.checkTermExportStatusTimer();
                    } else {
                        $hExportStatusKey.val("");
                        $$.loading(false);
                    }
                },
                error: (data) => {
                    $hExportStatusKey.val("");
                    $$.loading(false);
                }
            });
        }, 1000);
    }

    handleExportBtn() {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: this.handleTermExport });
    }

    handleTermExport = async (e) => {
        let requestOption = {
            url: "/api/TermManagementApi/DownLoadReportJob",
        };
        $$.loading(true);
        const result = await fetchUtility(requestOption); 
        $$.loading(false);
        if (result.MessageType === 0) {
            showToast.success(<$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                <a className="ra-link-a" href="/Root/DC/Download">{RMResx.RM_JS_DC_Title}</a>
            </$g.I18NProvider>); 
        } else {
            showToast.error(result.ErrorMessage);
        }    
    }

    handleTermImport(e) {
        this.setState({ showImportPanel: { show: true } });
    }

    handleAIRecommendationClick = () => {
        const option = {
            url: "/api/FeatureUsageLimit/CheckUsageLimit",
            method: "POST",
            data: UsageLimitType.AIRecommendation
        }
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                if (res) {
                    this.setState({ showAIRecommendationDialog: true });
                } else {
                    $$.messagedialog(true, {
                        width: "550px",
                        hideActions: false,
                        title: RMResx.RM_JS_Common_Confirmation,
                        content: RMResx.RM_ML_Zero_CheckUsageLimit_Msg,
                        buttons: [
                            {
                                text: RMResx.RM_JS_Common_OK,
                                primary: true,
                                classify: "theme",
                                onClick: () => $$.messagedialog(false),
                            },
                        ],
                    });
                }
            })
            .finally(() => $$.loading(false));
    }

    handleCloseAIRecommendationDialog = () => {
        this.setState({ 
            aiRecommendationResult: null,
            showAIRecommendationDialog: false,
        });
    }

    handleBackAIRecommendationDialog = () => {
        this.setState({ aiRecommendationResult: null });
    }

    handleExportRecommendationResult = () => {
        $$.loading(true);
        const divElement = document.getElementById("downloadResult");
        const downloadUrl = "/api/TermManagementApi/ExportAIRecommendation";
        const data = this.aiRecommendationsDialogRef.current.getAllData();
        ReactDOM.render(
            <form action={downloadUrl} method="POST">
                <input
                    id="exportIndustry"
                    name="industry"
                    type="hidden"
                    value={data.industry}
                    readOnly
                />
                <input
                    id="exportRecords"
                    name="records"
                    type="hidden"
                    value={JSON.stringify(this.state.aiRecommendationResult)}
                    readOnly
                />
            </form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
        $$.loading(false);
    }

    handleNextAIRecommendationDialog = () => {
        if (!this.aiRecommendationsDialogRef.current.isValidIndustry() && !$$.verify("aiRecommendationsValidation")) {
            return false;
        }

        const data = this.aiRecommendationsDialogRef.current.getAllData();
        const formData = new FormData();
        formData.append('industry', data.industry);
        formData.append('country', data.country);
        formData.append('requirement', data.requirement);
        if (data.file) {
            formData.append('fileUp', data.file, data.file.fileName);
        } else {
            formData.append('fileUp', data.file);
        }
        $$.loading(true);
        fetch("/api/TermManagementApi/AIRecommendation", {
            method: "POST",
            body: formData,
        })
        .then(function (response) {
            return response.text().then(function (dataString) {
                return {
                    responseStatus: response.status,
                    responseString: JSON.parse(dataString),
                };
            });
        })
        .then((res) => {
            $$.loading(false);
            if (res) {
                const data = res.responseString;
                if (data) {
                    if (data.MessageType === RAMessageType.Successful) {
                        this.setState({ aiRecommendationResult: data.Extsion1 });
                    } else {
                        showToast.error(data.ErrorMessage);
                    }
                }
            }
        });
    }

    handleTermSync(e) {
        let contentMgs = "";
        if (LicenseHelper.HasOpusILLicense() && !LicenseHelper.HasOpusGoogleLicense()) {
            contentMgs = RMResx.RM_TM_ConfirmSynchroniseMsg;
        } else if (LicenseHelper.HasOpusGoogleLicense() && !LicenseHelper.HasOpusILLicense()) {
            contentMgs = RMResx.RM_TM_ConfirmSynchroniseMsg_Only_Google;
        } else {
            contentMgs = RMResx.RM_TM_ConfirmSynchroniseMsg_With_Google;
        }
        $$.messagedialog(true, {
            // classify: "info",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: contentMgs,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.hideMessagebox },
                { text: RMResx.RM_JS_Common_OK, id: "raTmSyncSureBtn", primary: true, classify: "theme", onClick: this.synchronise }
            ]
        });
    }

    handleDownloadTemplate = (e) => {
        let downloadTemplate = StringUtil.newGuid();
        var $downloadStatusKey = $("#importDownloadFlag");
        $downloadStatusKey.val(downloadTemplate);

        $("#tm-form-download")
            .attr("action", "/api/TermManagementApi/DownloadTemplate")
            .submit();
    }

    handleUpload(args) {
        const isSucceed = args.isSucceed;
        $$.log(isSucceed ? 'uploadSuccess:' : 'uploadError', args);
        if (isSucceed) {
            args.files[0].fileId = StringUtil.newGuid();
            this.files = args.files[0];
        }
    }

    handleDelete(args) {
        if (args.isSucceed) {
            this.files = null;
        }
    }

    handleTenantSelect(args) {
        const tenantSelected = {};
        args.newValue.forEach(item => tenantSelected[item.value] = item.name);
        const { showImportPanel: {show} } = this.state;
        this.setState({
            importTenantSelected: tenantSelected,
            settingTenantSelected: args.newValue,
            noTenantSelectedVerify: false,
            itemSettingChanged: !show,
        });
    }

    handleTermGroupSelect(args) {
        let tenantList = [];
        let importTenantSelected = {};
        if (args.newValue == null) {
            tenantList = this.state.tenantList.map(tenant => ({
                ...tenant,
                checked: false,
            }))
        } else {
            const termGroupId = args.newValue?.value;
            tenantList = this.state.tenantList.map(tenant => {
                if (tenant.TermGroupId == termGroupId) {
                    importTenantSelected[tenant.value] = tenant.name
                }
                const isSelectable = tenant.TermGroupId == "00000000-0000-0000-0000-000000000000";
                return {
                    ...tenant,
                    checked: tenant.TermGroupId == termGroupId,
                    disabled: !isSelectable,
                }
            })
        }
        this.setState({
            termGroupSelected: args.newValue,
            noTermGroupSelectedVerify: false,
            tenantList,
            importTenantSelected
        })
    }

    hideMessagebox() {
        $$.messagedialog(false);
    }

    hideMessageTip() {
        this.setState({ showTip: false });
    }

    hideMmsMsgbar() {
        this.setState({
            mmsMsgbar: {
                show: false
            }
        });
    }

    onAddSiteInfo(e) {
        let siteUrl = $.trim(this.state.scUrlInput);
        if (siteUrl != "") {
            var reg = /^((https|http|ftp|rtsp|mms):\/\/)[\S]+/;
            if (!reg.test(siteUrl)) {
                this.showMmsMsgbar(RMResx.RM_JS_TM_MMSUrlInvalid);
            } else {
                // this.loadSiteInfoTimer = setTimeout(function () {
                //     $$.loading(true);
                // }, 1000);
                $$.loading(true);
                let option = {
                    url:"/api/TermManagementApi/GetMmsInfoByUrl",
                    method:"POST",
                    data: siteUrl,
                };
                fetchUtility(option).then((result) =>{
                    $$.loading(false);
                    var siteInfo = result ? JSON.parse(result) : null;
                    if (this.existMmsInfo(siteInfo)) {
                        this.showMmsMsgbar(RMResx.RM_JS_TM_MMSExitsMsg);
                        return;
                    }

                    let siteItems = RM.deepcopy(this.state.siteInfoResult);
                    let tempSiteInfo = siteItems.find(o => this.compareUrl(o.SiteUrl, siteUrl));

                    if (tempSiteInfo != null && tempSiteInfo.Action == SiteAction.Delete) {
                        tempSiteInfo = Object.assign(tempSiteInfo, siteInfo);
                        tempSiteInfo.Action = tempSiteInfo.Id > 0 ? SiteAction.Update : SiteAction.Add;

                        let specificTermStore = siteItems.filter(item => item.SiteType != SiteType.Google);
                        this.setState({
                            siteInfoResult: siteItems,
                            itemSettingChanged: true,
                            scUrlInput: ""
                        }, () => {
                            this.dispatch(this.termStoreTableId, specificTermStore, this.termStoreColumns);
                        });
                        this.hideMmsMsgbar();
                        return;
                    }
                    if (siteInfo != null) {
                        if (siteInfo.TermStoreId == "00000000-0000-0000-0000-000000000000") {
                            this.showMmsMsgbar(RMResx.RM_JS_TM_FailedGetTermStore);
                            return;
                        }
                        if (siteInfo.SiteUrl == null) {
                            this.showMmsMsgbar(RMResx.RM_JS_TM_SiteNotRegister);
                            return;
                        }
                        siteInfo.Action = SiteAction.Add;
                        var uniqueId = this.state.currentItem.UniqueId;
                        siteInfo.TermGroupId = uniqueId;
                        siteInfo.DisplayName = siteInfo.TermStoreName + "(" + siteInfo.TermStoreId + ")";
                        let siteItems = RM.deepcopy(this.state.siteInfoResult);
                        siteItems.push(siteInfo);
                        // this.state.siteInfoResult.push(siteInfo);
                        let specificTermStore = siteItems.filter(item => item.SiteType != SiteType.Google);
                        this.setState({
                            itemSettingChanged: true,
                            siteInfoResult: siteItems,
                            scUrlInput: ""
                        }, () => {
                            this.dispatch(this.termStoreTableId, specificTermStore, this.termStoreColumns);
                        });
                        this.hideMmsMsgbar();
                    } else {
                        this.showMmsMsgbar(RMResx.RM_JS_TM_ResxHostWarning);
                    }
                    // clearTimeout(this.loadSiteInfoTimer);
                }).catch((e) =>{
                    clearTimeout(this.loadSiteInfoTimer);
                    $$.loading(false);
                });
            }
        }
    }

    onCancelChangedClick(e) {
        let args = {
            // classify: "info",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_CancelClickMsg,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.hideMessagebox },
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme",
                    onClick: () => {
                        this.setNewSelectedItem(this.state.selectedItem);
                        this.hideMessagebox();
                    }
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    onCreateNewRuleClick(termRule, ruleLevel) {
        this.creatingTermRuleRef.current = termRule;
        this.setState({
            currentRowRuleLevelId: ruleLevel
        });
        this.dispatch(this.createRuleComponentId, 2 , ruleLevel);//1 RuleManagement, 2 TermManagement
    }

    onCreateNewRuleDialogClose(e) {
        this.setState({
            showCreateRuleDialog: false,
            createRuleUrl: ""
        });
        if (e == 1) {
            this.getAvailableRuleList();
        }
    }

    onChooseMMSChanged(value) {
        const usingMMSSpecified = value == TermSyncOptionType.Specified;
        let curItem = RM.deepcopy(this.state.currentItem);
        if (usingMMSSpecified) {
            this.getMmsInfo(curItem.UniqueId);
        }
        curItem.UsingMMSSpecified = usingMMSSpecified;
        curItem.M365TermSyncOption = value;
        this.setState({
            currentItem: curItem,
            itemSettingChanged: true
        });
    }

    onChooseGoogleTenantChanged(value) {
        let curItem = RM.deepcopy(this.state.currentItem);
        if (value != TermSyncOptionType.Specified) {
            this.setState({settingTenantSelected: []})
        }

        curItem.GoogleTermSyncOption = value;
        this.setState({
            currentItem: curItem,
            itemSettingChanged: true,
            noTenantSelectedVerify: false,
        });
    }

    onChooseImportType(value) {
        this.setState({
            termImportType: value,
            importTenantSelected: {},
            termGroupSelected: null,
            noTenantSelectedVerify: false,
            noTermGroupSelectedVerify: false,
        })
    }

    onDescriptionChange(value) {
        let curItem = this.state.currentItem;
        curItem.Description = value;
        this.setState({
            currentItem: curItem,
            itemSettingChanged: true
        });
    }
    
    onAdvanceSettingsChange(value) {
        let curItem = this.state.currentItem;
        curItem.AdvanceSettings = value;
        this.setState({
            currentItem: curItem,
            itemSettingChanged: true
        });
    }


    onChangeSCUrl = (value) => {
        this.setState({scUrlInput: value});
    }

    onEnforceRetentionChange(checked) {
        let curItem = this.state.currentItem;
        curItem.EnforceRetention = checked ? 1 : 0;
        this.setState({
            enforceRetention: checked,
            itemSettingChanged: true,
            noExoLabelValueVerify: false,
            noSpLabelValueVerify: false,
            noTeamsLabelValueVerify: false,
            isTeamsRetentionChecked: false,
            isSpRetentionChecked: false,
            isExoRetentionChecked: false,
            isOneDriveRetentionChecked: false,
            noOneDriveLabelValueVerify: false,
            noSelectSpOrExoVerify: false
        });
    }

    teamsRetentionChange = (checked) => {
        this.setState({
            isTeamsRetentionChecked: checked,
            itemSettingChanged: true,
            noSelectSpOrExoVerify: false,
            teamsRetentionActionLabel: this.originalTeamsLabel
        });
    }

    spRetentionChange(checked) {
        this.setState({
            isSpRetentionChecked: checked,
            itemSettingChanged: true,
            noSelectSpOrExoVerify: false,
            spRetentionActionLabel: this.originalSPLabel
        });

    }

    exoRetentionChange(checked) {
        this.setState({
            isExoRetentionChecked: checked,
            itemSettingChanged: true,
            noSelectSpOrExoVerify: false,
            exoRetentionActionLabel: this.originalEXOLabel
        });
    }

    exoRetentionActionLabelChange(value) {
        this.setState({
            exoRetentionActionLabel: value,
            itemSettingChanged: true
        });
    }

    exoRetentionActionLabelBlur() {
        this.retentionVerify();
    }

    oneDriveRetentionChange(checked) {
        this.setState({
            isOneDriveRetentionChecked: checked,
            itemSettingChanged: true,
            noSelectSpOrExoVerify: false,
            oneDriveRetentionActionLabel: this.originalOneDriveLabel
        });
    }

    oneDriveRetentionActionLabelChange(value) {
        this.setState({
            oneDriveRetentionActionLabel: value,
            itemSettingChanged: true
        });
    }

    oneDriveRetentionActionLabelBlur() {
        this.retentionVerify();
    }

    teamsRetentionActionLabelChange = (value) => {
        this.setState({
            teamsRetentionActionLabel: value,
            itemSettingChanged: true,
        });
    }

    teamsRetentionActionLabelBlur = () => {
        this.retentionVerify();
    }

    spRetentionActionLabelChange(value) {
        this.setState({
            spRetentionActionLabel: value,
            itemSettingChanged: true
        });
    }

    spRetentionActionLabelBlur() {
        this.retentionVerify();
    }

    onExpireOptionChanged(value) {
        let expireSettings = this.state.termExpireSettings;
        expireSettings.selectedOption = value;
        expireSettings.fromDateTime = null;
        expireSettings.endDateTime = null;
        this.setState({
            itemSettingChanged: true,
            termExpireSettings: expireSettings
        });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    onNodeSelected(item, funcAllow) {
        if (this.state.itemSettingChanged) {
            this.showIfLeaveWithoutSaveMsg((allow) => {
                this.hideMessagebox();
                if (funcAllow) {
                    funcAllow(allow);
                }
                if (allow) {
                    this.setNewSelectedItem(item);
                }
            });
        } else {
            if (funcAllow) {
                funcAllow(true);
            }
            this.setNewSelectedItem(item);
        }
    }

    onNoTermExpireSettingChange(checked) {
        let expireSettings = RM.deepcopy(this.state.termExpireSettings);
        expireSettings.noExpireSetting = checked;
        expireSettings.selectedOption = null;
        expireSettings.fromDateTime = null;
        expireSettings.endDateTime = null;
        this.setState({
            itemSettingChanged: true,
            termExpireSettings: expireSettings
        });
    }

    getSelectedRuleLevelId() {
        for (let key in ObjectLevel) {
            if (ObjectLevel[key]) {
                let item = ObjectLevel[key];
                if (item.name == this.state.selectedRuleLevel.name) {
                    return item.value;
                }
            }
        }
    }

    onRuleOperated(data) {
        if (this.state.currentRowRuleLevelId == data.RuleLevel) {
            this.creatingTermRule.RuleId = data.RuleId;
            this.creatingTermRule.RuleName = data.RuleName;
            this.creatingTermRule = data;
        }
        this.getAvailableRuleList();
    }

    needSetNewRuleSelected(termRule) {
        return  this.creatingTermRule && this.creatingTermRule.Id < 0 && this.creatingTermRule.RuleId 
                                && this.creatingTermRule.RuleOrder == termRule.RuleOrder 
                                && this.creatingTermRule.RuleLevel == termRule.RuleLevel;
    }

    jumpExportSettings() {
        this.props.history.push({
            pathname: RouterUrls.CP_ExportSettings
        });
    }

    onSaveSettingClick(e) {
        let curItem = this.state.currentItem;
        if (curItem.Description && curItem.Description.length > 5000) {
            return;
        }
        if(/\t/.test(curItem.Description)){
            showToast.error(RMResx.RM_JS_TM_DesFailedMsg);
            return;
        }
        switch (curItem.Type) {
            case "TermGroup":
                this.saveTermGroup(curItem);
                break;
            case "TermSet":
                this.saveTermSet(curItem);
                break;
            case "Term":
                this.saveTerm(curItem);
                break;
            default:
                break;
        }
    }

    onSearch(args) {
        this.searchData(args.value || args);
    }

    onSelectImportFileKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    onTermBreakClick(e) {
        this.setState({
            itemSettingChanged: true,
            isTermBreakInherit: true,
            termSettingDisabled: false,
            isTeamsRetentionDis: false,
            isSpRetentionDis: false,
            isExoRetentionDis: false,
            exoRetentionActionLabelDis: false,
            isOneDriveRetentionDis: false,
            oneDriveRetentionActionLabelDis: false,
            teamsRetentionActionLabelDis: false,
        }, ()=> {
            this.setFocusForMessageBar();
        });
    }

    onTermInheritClick(e) {
        $.ajax({
            //get parent rule from db
            type: "GET",
            url: "/api/TermManagementApi/GetParentSettingInfoByTermId",
            //contentType: 'application/json;charset=utf-8',
            data: "termId=" + this.state.currentItem.Id,
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                data = $.parseJSON(data);   // Fortify Issue Type: JSON Injection; Sink Details: init term data; Ignore Reason: 前后台对象存在对应关系
                if ("" == data.message) {
                    return;
                }
                this.getEnforceRetentionCheckedStatue(data);
                let parentNoRulesAndEnforceSettings = !data.EnforceRetention && data.infos.length == 0;
                let showInheritBar = !this.state.currentItem.IsRootTerm && parentNoRulesAndEnforceSettings? false: this.state.showInheritBar;
                let disabledStatus =  parentNoRulesAndEnforceSettings? false: true;
                this.setState({
                    itemSettingChanged: true,
                    isTermBreakInherit: false,
                    termSettingDisabled: disabledStatus,
                    isTeamsRetentionDis: disabledStatus,
                    isSpRetentionDis: disabledStatus,
                    isExoRetentionDis: disabledStatus,
                    exoRetentionActionLabelDis: disabledStatus,
                    isOneDriveRetentionDis: disabledStatus,
                    oneDriveRetentionActionLabelDis: disabledStatus,
                    teamsRetentionActionLabelDis: disabledStatus,
                    termRulesGroupByLevel: this.getTermRulesGroupsByLevel(data.infos),
                    showInheritBar: showInheritBar
                }, () => {
                    this.resetTermRulesOrder();
                    this.setFocusForMessageBar();
                });
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    setFocusForMessageBar() {
        $(".ra-topMessageBar-message:eq(0)").focus();
    }

    getEnforceRetentionCheckedStatue(term) {
        let isCheckedSP = this.existsEnforceRetentionType(term.EnforceRetention, EnforceRetentionType.SP);
        let spLabel = isCheckedSP ? term.SPRetentionLabel : "";
        const isCheckedTeams = this.existsEnforceRetentionType(term.EnforceRetention, EnforceRetentionType.Teams);
        const teamsLabel = isCheckedTeams ? term.TeamsRetentionLabel : "";
        let isCheckedEXO = this.existsEnforceRetentionType(term.EnforceRetention, EnforceRetentionType.EXO);
        let exoLabel = isCheckedEXO ? term.EXORetentionLabel : "";
        let isCheckedOneDrive = this.existsEnforceRetentionType(term.EnforceRetention, EnforceRetentionType.OneDrive);
        let oneDriveLabel = isCheckedOneDrive ? term.OneDriveRetentionLabel : "";
        this.setState({
            isSpRetentionChecked: isCheckedSP,
            isExoRetentionChecked: isCheckedEXO,
            isOneDriveRetentionChecked: isCheckedOneDrive,
            isTeamsRetentionChecked: isCheckedTeams,
            spRetentionActionLabel: spLabel,
            teamsRetentionActionLabel: teamsLabel,
            exoRetentionActionLabel: exoLabel,
            oneDriveRetentionActionLabel: oneDriveLabel,
            enforceRetention: isCheckedSP || isCheckedEXO || isCheckedOneDrive || isCheckedTeams,
        });
    }

    existsEnforceRetentionType(termRetention, needCheckEnforceRetentionType)
    {
        return (termRetention & needCheckEnforceRetentionType) == needCheckEnforceRetentionType;
    }

    onTermRuleDelClick(termRule) {
        let iLevel = this.getTermRuleLevelValue(termRule);
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        let termRules = allTermRules[iLevel];
        let trIndex = -1;
        for (let i = 0, len = termRules.length; i < len; i++) {
            let tr = termRules[i];
            if (tr.Id == termRule.Id) {
                trIndex = i;
            }
        }
        if (trIndex > -1) {
            termRules.splice(trIndex, 1);
            this.setState({ itemSettingChanged: true, termRulesGroupByLevel: allTermRules }, () => {
                this.resetTermRulesOrder();
            });
        }
    }

    onTermRuleViewClick(termRule) {
        this.ruleDetail.load({ ruleId: termRule.RuleId });
    }  

    processHasMatchChildren(item) {
        let hasMatchChildren = false;
        if (item.subTerms) {
            item.subTerms.forEach((subitem) => {
                if (!hasMatchChildren && subitem.Name.toLocaleLowerCase().indexOf(this.treeContext.searchKey.toLocaleLowerCase()) > -1) {
                    hasMatchChildren = true;
                }
                hasMatchChildren |= this.processHasMatchChildren(subitem);
            });
        }
        return item.hasMatchChildren = hasMatchChildren;
    }

    //actionType: 1=rename, 2=retire, 3=reactive, 4=delete item
    refreshSelectedNodeInfo(item, actionType) {
        let selItem = this.state.selectedItem;
        if (actionType != 4) {
            if (!selItem || (item.Type == "TermGroup" && item.UniqueId != selItem.UniqueId)
                || (item.Type != "TermGroup" && item.Id != selItem.Id)) {
                return;
            }
        }
        let props;
        switch (actionType) {
            case 4:
                this.setState({
                    itemSettingChanged: false,
                    selectedItem: null,
                    currentItem: null,
                });
                if (this.treeContext.searchKey != "") {
                    this.onSearch(this.treeContext.searchKey);
                }
                return;
            case 1:
                props = ["Name"];
                break;
            case 2:
            case 3:
                props = ["IsExpired", "IsDeprecated", "TermExpirationFrom", "TermExpirationTo", "TimeZoneId"];
                this.setState({
                    termExpireSettings: this.getDefaultTermExpireSettings()
                });
                break;
            default:
                props = [];
                break;
        }

        this.copyProps(item, this.state.selectedItem, props);
        this.copyProps(item, this.state.currentItem, props);

        this.setState({
            selectedItem: RM.deepcopy(this.state.selectedItem),
            currentItem: RM.deepcopy(this.state.currentItem)
        });
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        var reg3 = new RegExp("#","ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        str = str.replace(reg3,"%23");
        return str;
    }

    resetTermRulesOrder() {
        let order = 1;
        let allTermRules = RM.deepcopy(this.state.termRulesGroupByLevel);
        for (let key in allTermRules) {
            if (allTermRules[key]) {
                for (let rule of allTermRules[key]) {
                    rule.RuleOrder = order++;
                }
            }
        }
        this.setState({ termRulesGroupByLevel: allTermRules }, ()=> {
            $(".tbContent div[role='combobox']:eq(1)").focus();
        });
    }

    resetTreeData(data) {
        let root = {
            Name: RMResx.RM_JS_TM_RootTerms,
            Type: "Root",
            Id: "Root",
            subTerms: $.parseJSON(data)     // Fortify Issue Type: JSON Injection; Sink Details: tree data; Ignore Reason: 前后台对象存在对应关系
        };
        if (this.treeContext.searchKey) {
            this.processHasMatchChildren(root);
        }
        root.subTermCount = root.subTerms.length;
        this.setState({ treeData: [root] });
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }

    retentionVerify() {
        let verify = true;
        this.setState({
            noSelectSpOrExoVerify: false,
            noExoLabelValueVerify: false,
            noSpLabelValueVerify: false,
            noTeamsLabelValueVerify: false,
            noOneDriveLabelValueVerify: false
        });
        if (this.state.enforceRetention) {
            if (!this.state.isExoRetentionChecked && !this.state.isSpRetentionChecked && !this.state.isOneDriveRetentionChecked && !this.state.isTeamsRetentionChecked) {
                this.setState({ noSelectSpOrExoVerify: true });
                verify = false;
            }
            if (this.state.isExoRetentionChecked && !this.state.exoRetentionActionLabel) {
                this.setState({ noExoLabelValueVerify: true });
                verify = false;
            }
            if (this.state.isSpRetentionChecked && !this.state.spRetentionActionLabel) {
                this.setState({ noSpLabelValueVerify: true });
                verify = false;
            }
            if (this.state.isTeamsRetentionChecked && !this.state.teamsRetentionActionLabel) {
                this.setState({ noTeamsLabelValueVerify: true });
                verify = false;
            }
            if (this.state.isOneDriveRetentionChecked && !this.state.oneDriveRetentionActionLabel) {
                this.setState({ noOneDriveLabelValueVerify: true });
                verify = false;
            }
        }
        return verify;
    }

    activeVerify() {
        let verify = true;
        let expireSetting = this.state.termExpireSettings,
            beginTime = expireSetting.fromDateTime,
            endTime = expireSetting.endDateTime,
            todayTime = new Date().getTime();
        if (beginTime && endTime) {
            let beginTimeMs = beginTime.getTime();
            let endTimeMs = endTime.getTime();
            if (beginTimeMs < todayTime || endTimeMs < todayTime || beginTimeMs > endTimeMs) {
                showToast.error(RMResx.RM_JS_TM_TimeError);
                verify = false;
            }
        }
        return verify;
    }

    reviseLabelMessage(curItem) {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className='revise_msg_content'>
                    <div className='revise_msg_up'>{RMResx.RM_JS_TM_ChangeLabelConfirmMsg1}</div>
                    <div className='revise_msg_down'>{RMResx.RM_JS_TM_ChangeLabelConfirmMsg2}</div>
                </div>
            </div>,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.changeLabelCancleClick },
                { text: RMResx.RM_JS_Common_OK, id: "raTmChangeLabelConfirmSureBtn", primary: true, classify: "theme", onClick: this.changeLabelSureClick.bind(this, curItem) },
            ]
        };
        $$.messagedialog(true, args);
    }

    changeLabelSureClick(curItem) {
        this.saveTermInfo(curItem);
        this.originalEXOLabel = this.state.exoRetentionActionLabel;
        this.originalSPLabel = this.state.spRetentionActionLabel;
        this.originalTeamsLabel = this.state.teamsRetentionActionLabel;
        this.originalOneDriveLabel = this.state.oneDriveRetentionActionLabel;
        $$.messagedialog(false);
    }

    changeLabelCancleClick() {
        $$.messagedialog(false);
    }

    saveTermInfo(curItem) {
        let termRulesAllHasRule = this.termRulesAllHasRule();
        if (!termRulesAllHasRule) {
            showToast.error(RMResx.RM_JS_BCM_Msg_TermRulesNotAllHasRule);
            $(window).scrollTop(0);
            return;
        }
        let termExpireSettings = this.state.termExpireSettings,
            beginTime = null,
            endTime = null,
            // isDayLight = false,
            // timeZoneId = "",
            selDateType = "3",
            isSpRetentionChecked = this.state.isSpRetentionChecked,
            isExoRetentionChecked = this.state.isExoRetentionChecked,
            isOneDriveRetentionChecked = this.state.isOneDriveRetentionChecked,
            isTeamsRetentionChecked = this.state.isTeamsRetentionChecked,
            enforceRetention = 0;

        if (!termExpireSettings.noExpireSetting) {
            if(termExpireSettings.selectedOption === null){
                showToast.error(RMResx.RM_TM_NoSelectActiveTimeType);
                return;
            }
            switch (termExpireSettings.selectedOption) {
                case TermExpireSettingType.TakeEffectFrom:
                case TermExpireSettingType.RetireAfter:
                case TermExpireSettingType.ActiveFromTo:
                    selDateType = termExpireSettings.selectedOption;
                    beginTime = termExpireSettings.fromDateTime;
                    endTime = termExpireSettings.endDateTime;
                    // timeZoneId = termExpireSettings.timeZone.id;
                    // isDayLight = termExpireSettings.timeZone.autoAdjustClock;
                    break;
            }
        }

        let postUrl = null,
            termObj = {
                tId: curItem.Id,
                des: curItem.Description,
                advanceSettings: curItem.AdvanceSettings,
                selDateType: selDateType,
                beginTime: RM.TimeUtil.getCommonDateStr(beginTime),
                endTime: RM.TimeUtil.getCommonDateStr(endTime),
                // IsDayLight: isDayLight,
                // TimeZoneId: timeZoneId
            };
        if (this.state.enforceRetention) {
            if(isSpRetentionChecked)
            {
                enforceRetention |= EnforceRetentionType.SP;
            }
            if(isExoRetentionChecked)
            {
                enforceRetention |= EnforceRetentionType.EXO;
            }
            if(isOneDriveRetentionChecked)
            {
                enforceRetention |= EnforceRetentionType.OneDrive;
            }
            if (isTeamsRetentionChecked) {
                enforceRetention |= EnforceRetentionType.Teams;
            }
        } else {
            enforceRetention = 0;
        }
        //showInheritBar=true表示ParentTerm有setting
        let isSaveTermSettings = curItem.IsRootTerm || !this.state.showInheritBar || (this.state.showInheritBar && this.state.isTermBreakInherit);
        if (isSaveTermSettings) {
            Object.assign(
                termObj,
                {
                    tName: curItem.Name,
                    infos: this.getTermRules(),
                    EnforceRetention: enforceRetention,
                    breakInhert: this.state.isTermBreakInherit,
                    isPermanent: false,
                    EXORetentionLabel: this.state.exoRetentionActionLabel,
                    SPRetentionLabel: this.state.spRetentionActionLabel,
                    TeamsRetentionLabel: this.state.teamsRetentionActionLabel,
                    OneDriveRetentionLabel: this.state.oneDriveRetentionActionLabel
                });
            postUrl = "/api/TermManagementApi/SaveTermSettings";
        } else {
            postUrl = "/api/TermManagementApi/InheritSettingToParent";
        }
        $.ajax({
            type: "POST",
            url: postUrl,
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify(termObj),
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                switch (data) {
                    case 1:
                    case 2:
                        showToast.error(RMResx.RM_PRM_PRE_Msg_BeforeCurrentTime);
                        $(window).scrollTop(0);
                        return;
                    case 3:
                        showToast.error(RMResx.RM_JS_TM_FromGtToTimeMsg);
                        $(window).scrollTop(0);
                        return;
                    case 4:
                    case 5:
                    case 6:
                        showToast.error(RMResx.RM_JS_TM_ChooseExpirDateMsg);
                        $(window).scrollTop(0);
                        return;
                    case 11:
                        showToast.error(RMResx.RM_JS_TM_AdvancedSettingsFormatErrorMessage);
                        $(window).scrollTop(0);
                        return;
                    case -1:
                        showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                        $(window).scrollTop(0);
                        return;
                }
                let newItem = $.parseJSON(data);    // Fortify Issue Type: JSON Injection; Sink Details: save term; Ignore Reason: 前后台对象存在对应关系
                this.UpdateTermProps(curItem, newItem, isSaveTermSettings);

                this.setState({
                    itemSettingChanged: false,
                    isTermBreakInherit: newItem.BreakInheritFromParent,
                    showInheritBar: (newItem.IsRootTerm ? false : ((!newItem.RuleInfo && !newItem.EnforceRetention && !this.state.showInheritBar) ? false : true))
                });
                showToast.success(RMResx.RM_JS_TM_SaveSucessMsg);
                $(window).scrollTop(0);
            },
            error: (msg) => {
                showToast.error(RMResx.RM_JS_TM_SaveFailedMsg);
                $(window).scrollTop(0);
            },
            dataType: "json"
        });
    }

    UpdateTermProps(curItem, newItem, isSaveTermSettings) {
        if (isSaveTermSettings) {
            newItem.Description = curItem.Description;
            newItem.AdvanceSettings = curItem.AdvanceSettings;
            newItem.EXORetentionLabel = this.state.exoRetentionActionLabel;
            newItem.SPRetentionLabel = this.state.spRetentionActionLabel;
            newItem.TeamsRetentionLabel = this.state.teamsRetentionActionLabel;
            newItem.OneDriveRetentionLabel = this.state.oneDriveRetentionActionLabel;
        }
        this.copyProps(
            newItem,
            this.state.selectedItem,
            ["BreakInheritFromParent", "Description", "IsExpired", "EnforceRetention",
                "TermExpirationFrom", "TermExpirationTo", "TimeZoneId", "RuleInfo", "AdvanceSettings"]);
        this.copyProps(
            newItem,
            this.state.currentItem,
            ["IsExpired"]);
    }

    saveTerm(curItem) {
        //Retention Setting验证
        let retentionVerify = this.retentionVerify();
        //Term Activation Settings actionTime 验证
        let activeVerify = this.activeVerify();
        if (!retentionVerify || !activeVerify) {
            return false;
        }
        var exoLabelChange = this.state.exoRetentionActionLabel != this.originalEXOLabel && this.state.isExoRetentionChecked;
        var spLabelChange = this.state.spRetentionActionLabel != this.originalSPLabel && this.state.isSpRetentionChecked;
        const teamsLabelChange = this.state.teamsRetentionActionLabel != this.originalTeamsLabel && this.state.isTeamsRetentionChecked;
        var oneDriveLabelChange = this.state.oneDriveRetentionActionLabel != this.originalOneDriveLabel && this.state.isOneDriveRetentionChecked;
        if (exoLabelChange || spLabelChange || oneDriveLabelChange || teamsLabelChange) {
            this.reviseLabelMessage(curItem);
            return;
        }
        this.saveTermInfo(curItem);
    }

    saveTermGroup(curItem) {
        let isValidInput = true;
        if (LicenseHelper.HasOpusILLicense() && curItem.UsingMMSSpecified && this.getAddedSiteCount() == 0) {
            this.showMmsMsgbar(RMResx.RM_JS_TM_NoAddSiteCollectionMsg);
            isValidInput = false;
        }
        if (LicenseHelper.HasOpusGoogleLicense() && curItem.GoogleTermSyncOption == TermSyncOptionType.Specified && !this.state.settingTenantSelected.length) {
            this.setState({ noTenantSelectedVerify: Boolean(!this.state.settingTenantSelected.length) })
            isValidInput = false;
        }
        if (!isValidInput) return;

        this.hideMmsMsgbar();
        const { settingTenantSelected, siteInfoResult } = this.state;
        const specifiedTenantList = settingTenantSelected.map(item => ({
            ...item,
            TermGroupId: curItem.UniqueId
        }))
        const siteInfoResultWithoutGoogle = siteInfoResult.filter(site => site.SiteType != SiteType.Google)
        const ReSiteInfosList = [...siteInfoResultWithoutGoogle, ...specifiedTenantList];
        var termGroupObj = {
            TermGroupId: curItem.Id,
            TermGroupName: this.replaceSpecialCharacters(curItem.Name),
            Description: curItem.Description,
            ReSiteInfos: ReSiteInfosList,
            UsingMMSSpecified: curItem.UsingMMSSpecified,
            M365TermSyncOption: LicenseHelper.HasOpusILLicense() ? curItem.M365TermSyncOption : TermSyncOptionType.None,
            GoogleTermSyncOption: LicenseHelper.HasOpusGoogleLicense() ? curItem.GoogleTermSyncOption : TermSyncOptionType.None,
        };
        $.ajax({
            type: "POST",
            url: "/api/TermManagementApi/SaveTermGroup",
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify(termGroupObj),
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                this.setState({ itemSettingChanged: false, scUrlInput: "" });

                if (data && data.MessageType == 0) {
                    showToast.success(RMResx.RM_JS_TM_SaveSucessMsg);
                    this.updateTermGroupItemProps(curItem);
                    this.getAllGoogleTenants();
                } else if (data && data?.Extension == "ExistedGoogleTenants") {
                    const tenantList = JSON.parse(data.ErrorMessage);
                    const errorMessage = Object.entries(tenantList);

                    errorMessage.forEach(([tenant, term]) => {
                        let content = <$g.I18NProvider msg={RMResx.RM_JS_TM_ExistedTenantErrorMgs}>
                                        <span>"{tenant}"</span>
                                        <span>"{term}"</span>
                                    </$g.I18NProvider>
                        showToast.error(content)
                    })
                }else{
                    data.ErrorMessage && showToast.error(data.ErrorMessage);
                }
            },
            error: (msg) => {
                showToast.error(RMResx.RM_JS_TM_SaveFailedMsg);
            },
            dataType: "json"
        });
    }

    updateTermGroupItemProps(curItem) {
        let propNames = ["Description", "UsingMMSSpecified", "ReSiteInfos", "GoogleTermSyncOption", "M365TermSyncOption"],
            newItem = {
                Description: curItem.Description,
                UsingMMSSpecified: curItem.UsingMMSSpecified,
                ReSiteInfos: this.state.siteInfoResult,
                GoogleTermSyncOption: curItem.GoogleTermSyncOption,
                M365TermSyncOption: curItem.M365TermSyncOption
            };
        this.copyProps(
            newItem,
            this.state.selectedItem,
            propNames);
    }

    saveTermSet(curItem) {
        var termSetObj = {
            TermSetId: curItem.Id,
            TermSetName: this.replaceSpecialCharacters(curItem.Name),
            Description: curItem.Description
        };
        $.ajax({
            type: "POST",
            url: "/api/TermManagementApi/SaveTermSet",
            contentType: "application/json;charset=utf-8",
            data: JSON.stringify(termSetObj),
            async: true,
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (data) => {
                this.setState({ itemSettingChanged: false });
                if (data && data == "-1") {
                    showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                    return;
                }
                showToast.success(RMResx.RM_JS_TM_SaveSucessMsg);
                var newItem = $.parseJSON(data);    // Fortify Issue Type: JSON Injection; Sink Details: save term set; Ignore Reason: 前后台对象存在对应关系
                this.copyProps(
                    newItem,
                    this.state.selectedItem,
                    ["Description"]);
            },
            error: (msg) => {
                showToast.error(RMResx.RM_JS_TM_SaveFailedMsg);
            },
            dataType: "json"
        });
    }

    searchData(key) {
        key = !key ? "" : key.trim();
        if (key.length == 0) {
            this.getTreeData();
        } else {
            $.ajax({
                type: "GET",
                url: "/api/TermManagementApi/Search",
                //contentType: 'application/json;charset=utf-8',
                data: "termLabel=" + this.replaceSpecialCharacters(key) + "&termGroupId=00000000-0000-0000-0000-000000000000" + "&withRuleName=true",
                async: true,
                beforeSend: function () {
                    $$.loading(true);
                },
                complete: function () {
                    $$.loading(false);
                },
                success: (data) => {
                    this.treeContext.searchKey = key;
                    this.resetTreeData(data);
                },
                error: (msg) => {
                    //alert(msg.responseText);
                },
                dataType: "json"
            });
        }
    }

    setNewSelectedItem(item) {
        const settingTenantSelected = [];
        const tenantList = this.state.tenantList.map(tenant => {
            if (tenant.TermGroupId == item.UniqueId) {
                settingTenantSelected.push(tenant);
            }
            return {
                ...tenant,
                checked: tenant.TermGroupId == item.UniqueId
            }
        })
        this.setState({
            selectedItem: item,
            currentItem: JSON.parse(JSON.stringify(item)),
            selectedRuleLevel: { name: "", value: "" },
            itemSettingChanged: false,
            noTenantSelectedVerify: false,
            tenantList,
            settingTenantSelected,
            siteInfoResult: [],
        });
        this.hideMessageTip();
        this.hideMmsMsgbar();
        if (item.Type == "TermGroup" && item.UsingMMSSpecified) {
            this.getMmsInfo(item.UniqueId);
        }
        if (item.Type == "Term") {
            this.getParentInhertSetting(item);
            this.getTermTimeSettings(item);
        }
    }

    showIfLeaveWithoutSaveMsg(funcAllow) {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_WithoutSavingMsg,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: () => funcAllow(false) },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => funcAllow(true) },    
            ]
        };
        $$.messagedialog(true, args);
    }

    showMessageTip(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    synchronise() {
        $$.messagedialog(false);
        $.ajax({
            type: "post",
            dataType: "JSON",
            url: "/api/TermSynchronizationApi/RunSync",
            contentType: "application/json;charset=utf-8",
            data: "false",
            beforeSend: function () {
                $$.loading(true);
            },
            complete: function () {
                $$.loading(false);
            },
            success: (response) => {
                if (response != null) {
                    if (response.MessageType == 0) {
                        //TODO: a to Link
                        showToast.success(<$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>);
                    } else {
                        // [RECO-27759] new error message
                        if (response.FaildType == 34) {
                            showToast.error(response.ErrorMessage);
                        } else {
                            showToast.error(RMResx.RM_JS_BCM_TermSync_NoSC);
                        }
                    }
                }
            },
            error: (msg) => {
                showToast.error(RMResx.RM_JS_BCM_TermSync_SyncFailMessage);
            }
        });
    }

    showMmsMsgbar(msg, type) {
        this.setState({
            mmsMsgbar: {
                type: !type ? "error" : type,
                show: true,
                msg: msg
            }
        });
    }

    settingsLoading(show) {
        $$.elementLoading("tmSettingsContainer", show);
    }

    newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    // render jsx below
    renderInheritBar() {
        if (this.state.isTermBreakInherit) {
            return <$g.TopMessageBar show={this.state.showInheritBar} type="warning" className="tm-inheritMsg">
                {RMResx.RM_JS_TM_breakInherLabel}&nbsp;
                <$g.I18NProvider msg={RMResx.RM_JS_TM_ChangeBreakInherLabel}>
                    <a className="ra-link-a" id='inherLink' tabIndex="0" onClick={this.onTermInheritClick} onKeyDown={this.onKeyDown}>
                        {RMResx.RM_JS_TM_inherBreak}
                    </a>
                </$g.I18NProvider>
            </$g.TopMessageBar>;
        } else {
            return <$g.TopMessageBar show={this.state.showInheritBar} type="info" className="tm-inheritMsg">
                {RMResx.RM_JS_TM_inherLabel}&nbsp;
                <$g.I18NProvider msg={RMResx.RM_JS_TM_ChangeInherLabel}>
                    <a  className="ra-link-a" id='breakInherLink' tabIndex="0" onClick={this.onTermBreakClick} onKeyDown={this.onKeyDown}>
                        {RMResx.RM_JS_TM_inher}
                    </a>
                </$g.I18NProvider>
            </$g.TopMessageBar>;
        }
    }

    renderSelectedItemName() {
        let selItem = this.state.currentItem;
        if (!selItem) {
            return null;
        }
        let labelName = null;
        switch (selItem.Type) {
            case "TermGroup":
                labelName = RMResx.RM_TM_TermGroupNameLabel;
                break;
            case "TermSet":
                labelName = RMResx.RM_TM_TermSetNameLabel;
                break;
            case "Term":
                labelName = RMResx.RM_TM_TermNameLabel;
                break;
            default:
                return null;
        }
        let typeName = labelName.replace(":", "");
        return <div tabIndex="0" data-tooltip aria-label={typeName} className="tm-tree-right-form-label-font">
            <label>{selItem.Name}</label>
        </div>;
    }

    renderTermActivationSettings(selItem) {
        let isDeprecated = selItem.IsDeprecated,
            isExpired = selItem.IsExpired,
            expireSetting = this.state.termExpireSettings,
            noExpireSetting = expireSetting.noExpireSetting,
            // timeZone = expireSetting.timeZone,
            beginTime = expireSetting.fromDateTime,
            endTime = expireSetting.endDateTime,
            selectedOption = expireSetting.selectedOption,
            termActiveOptionItems = this.getTermActiveOptionItems();
        if(selectedOption)
        {
            setCheckedStatus("value", "checked", termActiveOptionItems, this.getSelectedTermActiveOptionItem(selectedOption));
        }
        return <div id="termActivationSettings" className={isDeprecated || isExpired? "rm-noActive-setting": ""}>
            <span id="activation_setting_desc" className="tm-tree-right-form-label-font" tabIndex="0">
                {StringUtil.trimEndColon(RMResx.RM_TM_ExpirDate)}
            </span>
            <div id="isRetired" style={this.getDisplayStyle(isDeprecated || isExpired)}>
                <div className="divNodeTemplate">
                    <div className="img-treeNodePre img-term">
                        <div className="img-forbidden"></div>
                    </div>
                    <div className="tm-rule-details-content-font">
                        {RMResx.RM_JS_TM_Retired}
                    </div>
                </div>
            </div>
            <div style={this.getDisplayStyle(!isDeprecated)} className='margin-top-8'>
                <div>
                    <span>
                        <R.Switch
                            id="raTmActiveSettingsSwitch"
                            checked={noExpireSetting}
                            onChange={this.onNoTermExpireSettingChange} />
                    </span>
                    <span className="tm-normal-title margin-left-8" tabIndex="0">
                        {RMResx.RM_TM_NoExpirDate}
                    </span>
                </div>
                {!noExpireSetting && 
                <div id="activation_setting_options">
                    <div id="activation_setting_time_desc" className="tm-normal-title" tabIndex="0">{RMResx.RM_TM_Title_SpecifyTimeRange}</div>
                    <div style={{ display: "inline-block" }} className='timeActive-combobox'>
                        <R.Combobox
                            id="raTmTimeActive"
                            width="160px"
                            height={34}
                            searchable={false}
                            textField='text'
                            valueField='value'
                            checkedField='checked'
                            noneText={RMResx.RM_JS_TM_SelectRuleLabel}
                            items={termActiveOptionItems}
                            onChange={this.handleExpireOptionChange}
                        />
                    </div>
                    <div style={{ display: "inline-block" }}>
                        <div id="time-picker1" className="divDate" style={this.getDisplayStyle(selectedOption == TermExpireSettingType.TakeEffectFrom)}>
                            <R.Datepicker
                                id="raTmTakeEffectFromTime"
                                width="180"
                                dateTimeFormat={this.datetimeFormat}
                                selectedDate={selectedOption == TermExpireSettingType.TakeEffectFrom ? beginTime : null}
                                disabled={selectedOption != TermExpireSettingType.TakeEffectFrom}
                                hasTimePicker={true}
                                onChange={this.handleEffectFromChanged} />
                        </div>
                        <div id="time-picker2" className="divDate" style={this.getDisplayStyle(selectedOption == TermExpireSettingType.RetireAfter)}>
                            <R.Datepicker
                                id="raTmRetireAfterTime"
                                width="180"
                                dateTimeFormat={this.datetimeFormat}
                                selectedDate={selectedOption == TermExpireSettingType.RetireAfter ? endTime : null}
                                disabled={selectedOption != TermExpireSettingType.RetireAfter}
                                hasTimePicker={true}
                                onChange={this.handleRetireAfterChanged} />
                        </div>
                        <div style={this.getDisplayStyle(selectedOption == TermExpireSettingType.ActiveFromTo)}>
                            <div id="time-picker3" className="divDate">
                                <R.Datepicker
                                    id="raTmActiveBeginTime"
                                    width="180"
                                    dateTimeFormat={this.datetimeFormat}
                                    placeholder={RMResx.RM_Common_StartDate}
                                    selectedDate={selectedOption == TermExpireSettingType.ActiveFromTo ? beginTime : null}
                                    disabled={selectedOption != TermExpireSettingType.ActiveFromTo}
                                    hasTimePicker={true}
                                    onChange={this.handleActiveFromChanged} />
                            </div>
                            <div id="time-picker4" className="divDate">
                                <R.Datepicker
                                    id="raTmActiveEndTime"
                                    width="180"
                                    dateTimeFormat={this.datetimeFormat}
                                    placeholder={RMResx.RM_Common_EndDate}
                                    selectedDate={selectedOption == TermExpireSettingType.ActiveFromTo ? endTime : null}
                                    disabled={selectedOption != TermExpireSettingType.ActiveFromTo}
                                    hasTimePicker={true}
                                    onChange={this.handleActiveToChanged} />
                            </div>
                        </div>
                    </div>
                </div>}
            </div>
            <div id="tm-right-shade"
                style={this.getDisplayStyle(isDeprecated || isExpired)}></div>
        </div>;
    }

    renderSelectionTermOption() {
        const chooseMMSTitle = StringUtil.trimEndColon(RMResx.RM_TM_ChooseMMS);
        const { currentItem } = this.state;
        return (
            <>
                <div className="tm-title-label tm-tree-right-form-label-font" tabIndex="0">
                    <span className='require'>{chooseMMSTitle}</span>
                </div>
                <div className='margin-top-s margin-bottom-m flex flex-column gap-s'>
                    <R.Radio 
                        name="radioChooseMMS"
                        text={RMResx.RM_TM_MapTermByDefault}
                        tooltip={RMResx.RM_TM_MapTermByDefault}
                        value={TermSyncOptionType.All}
                        checked={currentItem.M365TermSyncOption == TermSyncOptionType.All}
                        onChange={this.onChooseMMSChanged}
                    />
                    <R.Radio 
                        name="radioChooseMMS"
                        text={RMResx.RM_TM_MapTermBySpecify}
                        tooltip={RMResx.RM_TM_MapTermBySpecify}
                        value={TermSyncOptionType.Specified}
                        checked={currentItem.M365TermSyncOption == TermSyncOptionType.Specified}
                        onChange={this.onChooseMMSChanged}
                    />
                    {currentItem.M365TermSyncOption == TermSyncOptionType.Specified && this.renderMMSSpecified()}
                    <R.Radio 
                        name="radioChooseMMS"
                        text={RMResx.RM_TM_NoneOption}
                        tooltip={RMResx.RM_TM_NoneOption}
                        value={TermSyncOptionType.None}
                        checked={currentItem.M365TermSyncOption == TermSyncOptionType.None}
                        onChange={this.onChooseMMSChanged}
                    />
                </div>
            </>
        )
    }
    
    renderMMSSpecified() {
        return (
            <div id="tm_mms_container" className='margin-top-m'>
                <div className="tm-tree-right-form-label-font" tabIndex="0" style={{ marginBottom: "5px" }}>{RMResx.RM_TM_SiteCollection}</div>
                <div className="top_setting">
                    <div id="setting_input">
                        <R.Input
                            id="raTmSiteCollectionUrlIpt"
                            name='iptScName'
                            type='text'
                            width="100%"
                            value={this.state.scUrlInput}
                            onChange={this.onChangeSCUrl.bind(this)}
                            aria={{ ariaLabel: RMResx.RM_TM_SiteCollection }}
                        />
                    </div>
                    <R.Button id="raTmAddSiteCollectionBtn" text={RMResx.RM_TM_Add} ghost onClick={this.onAddSiteInfo} />
                </div>
                <$g.TopMessageBar className="tm-sc-mmsMsg" show={this.state.mmsMsgbar.show}
                    showClose={true} type={this.state.mmsMsgbar.type} didClose={this.hideMmsMsgbar}>
                    {this.state.mmsMsgbar.msg}
                </$g.TopMessageBar>
                <div>
                    <ManageSpecificTermStoreTable
                        id={this.termStoreTableId}
                        columnInfo={this.termStoreColumns}
                        cellClick={this.handleDelSiteClick}
                    />
                </div>
            </div>
        )
    }
    
    renderSelectionGoogleTenantOption() {
        const chooseGoogleTenantTitle = StringUtil.trimEndColon(RMResx.RM_TM_ChooseGoogleTenant);
        const { currentItem, showImportPanel: {show} } = this.state;
        let id = StringUtil.newGuid();
        return (
            <>
                <div className="tm-title-label tm-tree-right-form-label-font" tabIndex="0">
                    <span className='require'>{chooseGoogleTenantTitle}</span>
                    <div className="margin-left-xs inline-block font-m">
                        <R.Popover width="500px" classify="gray" >
                            <span className="fia-status-info icon-tip-info" tabIndex={0} aria-label="infos" aria-describedby={"aria-popover-content-" + id} />
                            <span id={"aria-popover-content-" + id}>{RMResx.RM_TM_ChooseGoogleTenantDesc}</span>
                        </R.Popover>
                    </div>
                </div>
                <div className='margin-top-s margin-bottom-m flex flex-column gap-s'>
                    <R.Radio 
                        name="radioChooseGoogleTenant"
                        text={RMResx.RM_TM_AllGoogleTenantsOption}
                        tooltip={RMResx.RM_TM_AllGoogleTenantsOption}
                        value={TermSyncOptionType.All}
                        checked={currentItem.GoogleTermSyncOption == TermSyncOptionType.All}
                        onChange={this.onChooseGoogleTenantChanged}
                    />
                    <R.Radio 
                        name="radioChooseGoogleTenant"
                        text={RMResx.RM_TM_SpecifyGoogleTenantOption}
                        tooltip={RMResx.RM_TM_SpecifyGoogleTenantOption}
                        value={TermSyncOptionType.Specified}
                        checked={currentItem.GoogleTermSyncOption == TermSyncOptionType.Specified}
                        onChange={this.onChooseGoogleTenantChanged}
                    />
                    {currentItem.GoogleTermSyncOption == TermSyncOptionType.Specified && this.renderChooseGoogleTenant(show)}
                    <R.Radio 
                        name="radioChooseGoogleTenant"
                        text={RMResx.RM_TM_NoneOption}
                        tooltip={RMResx.RM_TM_NoneOption}
                        value={TermSyncOptionType.None}
                        checked={currentItem.GoogleTermSyncOption == TermSyncOptionType.None}
                        onChange={this.onChooseGoogleTenantChanged}
                    />
                </div>
            </>
        )
    }    
    
    checkPermissionForM365() {
        const permission = ["Source_SP", "Source_EXO", "Source_FS", "Source_Phy", "Source_LSP", "Source_OneDrive", "Source_AzureFile", "Source_Box", "Source_Teams"];
        return permission.some(item => checkPermission(item, RM.UserResources));
    }

    renderTermGroup(selItem) {
        if (selItem.Type == "TermGroup") {
            return <div id="tm_termgroup_setting">
                {this.checkPermissionForM365() && LicenseHelper.HasOpusILLicense() && this.renderSelectionTermOption()}
                {checkPermission("Source_Google",RM.UserResources) && this.hasOpusGoogleLicense && this.renderSelectionGoogleTenantOption()}
            </div>;
        } else {
            return null;
        }
    }

    existsTermRules() {
        let result = false;
        let allTermRules = this.state.termRulesGroupByLevel;
        for (let level in allTermRules) {
            let termRulesByLevel = allTermRules[level];
            if(termRulesByLevel && termRulesByLevel.length > 0) {
                result = true;
                break;
            }
        }
        return result;
    }

    renderTermRules() {
        let rulesGroups = [];
        let ordersGroups = {};
        let ruleOptionsOfLevels = {};
        for (let level in this.state.termRulesGroupByLevel) {
            if (this.state.termRulesGroupByLevel[level]) {
                let termRules = this.state.termRulesGroupByLevel[level];
                if (termRules.length > 0) {
                    ordersGroups[level] = termRules;
                    let ruleIds = termRules.map((rule) => rule.RuleId);
                    let allRulesOfLevel = this.state.rulesGroupByLevel[level];
                    ruleOptionsOfLevels[level] = allRulesOfLevel.filter((rule) => {
                        return ruleIds.indexOf(rule.RuleId) < 0;
                    });
                    rulesGroups.push(termRules);
                }
            }
        }
        return rulesGroups.map((termRules, i) => {
            let ruleLevel = this.getTermRuleLevelValue(termRules[0]);
            let ruleLevelName = this.getTermRuleLevelName(ruleLevel);
            let ruleOrders = ordersGroups[ruleLevel];
            let ruleOptions = ruleOptionsOfLevels[ruleLevel];
            return <tr key={"tr_" + ruleLevel} className="trTable" style={{ display: "table-row" }}>
                <td colSpan="4" valign="top">
                    <table className="tbContent" cellPadding="0" cellSpacing="0">
                        <tbody>
                            {termRules.map((termRule, k) => {
                                let isNew = !termRule.RuleName;
                                let tempRuleOptions = this.cloneObject(ruleOptions);
                                if (!isNew) {
                                    tempRuleOptions.push(termRule);
                                }
                                if(this.needSetNewRuleSelected(termRule) && this.creatingTermRule) {
                                    termRule.RuleName = this.creatingTermRule.RuleName;
                                    termRule.RuleId = this.creatingTermRule.RuleId;
                                    tempRuleOptions.push(termRule);
                                }
                                setCheckedStatus("RuleId", "Checked", tempRuleOptions, termRule);
                                let tempRuleOrders = setCheckedStatus("RuleOrder", "Checked", this.cloneObject(ruleOrders), termRule);
                                return <tr key={k} className={"tm-rules-tr"}>
                                    <td className="cbOrder" style={{ minWidth: "50px", width: "15%" }}>
                                        <R.Combobox
                                            id="raTmRuleItemOrder"
                                            searchable={false}
                                            width="100%"
                                            height={32}
                                            // popupWidth="100%"
                                            disabled={this.state.termSettingDisabled || ruleOrders.length == 1 || isNew}
                                            textField='RuleOrder'
                                            valueField='RuleOrder'
                                            checkedField='Checked'
                                            excludeChecked
                                            items={tempRuleOrders}
                                            onChange={(args) => this.handleTermRuleOrderChanged(args, termRule)} />
                                    </td>
                                    <td style={{ width: "32%" }}>
                                        <div className="sp-level" tabIndex="0" data-tooltip aria-label={ruleLevelName}>{ruleLevelName}</div>
                                    </td>
                                    <td className="cbRule" style={{ width: "23%" }}>
                                        <R.Combobox
                                            id="raTmRuleName"
                                            width={"100%"}
                                            height={32}
                                            disabled={this.state.termSettingDisabled}
                                            textField='RuleName'
                                            valueField='RuleId'
                                            checkedField='Checked'
                                            excludeChecked
                                            // createNewText={RMResx.RM_JS_TM_CreateNewRule}
                                            items={tempRuleOptions}
                                            noneText={RMResx.RM_JS_TM_NoSelectRuleTip}
                                            // doCreateNew={() => this.onCreateNewRuleClick(termRule, ruleLevel)}
                                            onChange={(args) => this.handleTermRuleNameChanged(args, termRule)} />
                                    </td>
                                    <td className="tm-rule-actions" style={{ width: "30%" }}>
                                        <R.Button
                                            id="raTmRuleItemAddBtn"
                                            type="bald"
                                            icon="fia-plus icon-option-item"
                                            onClick={(e) => this.onCreateNewRuleClick(termRule, ruleLevel)}
                                            tooltip={RMResx.RM_JS_TM_CreateNewRule}
                                            className="margin-right-xs"
                                            disabled={this.state.termSettingDisabled}
                                        />
                                        <R.Button
                                            type="bald"
                                            icon="fia-eye icon-option-item"
                                            onClick={(e) => this.onTermRuleViewClick(termRule, ruleLevel)}
                                            disabled={isNew}
                                            tooltip={RMResx.RM_JS_TM_ViewRuleLabel}
                                            className="margin-right-xs"
                                        />
                                        <R.Button
                                            type="bald"
                                            icon="fia-delete icon-option-item"
                                            onClick={(e) => this.onTermRuleDelClick(termRule)}
                                            tooltip={RMResx.RM_JS_TM_RemoveRuleLabel}
                                            disabled={this.state.termSettingDisabled}
                                        />
                                    </td>
                                </tr>;
                            })}
                        </tbody>
                    </table>
                </td>
            </tr>;
        });
    }

    renderRetentionOptions() {
        return this.state.enforceRetention &&
            <div className='retention_options'>
                <div>
                    {LicenseHelper.HasUpgradeTeams() && (
                        <div className='tm-RetentionSetting-teams'>
                            <div className="retention-options-item">
                                <span tabIndex="0">{RMResx.RM_JS_SPS_TabLabel_Teams}</span>
                                <R.Switch
                                    id="raTmTeamsRetentionSwitch"
                                    checked={this.state.isTeamsRetentionChecked}
                                    disabled={this.state.isTeamsRetentionDis}
                                    onChange={this.teamsRetentionChange} />
                            </div>

                            {this.state.isTeamsRetentionChecked &&
                                <div className='tm-RetentionSetting-Retention-label'>
                                    <div className='tm-RetentionSetting-label-Des tm-normal-title' tabIndex="0">{RMResx.RM_JS_TM_Retention_EXO_Prompt}</div>
                                    <div>
                                        <R.Input
                                            id="raTmExoRetentionSettingIpt"
                                            type="text"
                                            placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                            value={this.state.teamsRetentionActionLabel}
                                            disabled={this.state.teamsRetentionActionLabelDis}
                                            height={40}
                                            onChange={this.teamsRetentionActionLabelChange}
                                            onBlur={this.teamsRetentionActionLabelBlur}
                                            aria={{ ariaLabel: RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName }}
                                        />
                                    </div>
                                    <div className='no-exoRetention-verification'>
                                        <$g.ValidationMsg show={this.state.noTeamsLabelValueVerify}>
                                            {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                        </$g.ValidationMsg>
                                    </div>
                                </div>
                            }
                        </div>
                    )}
                    <div>
                        <div className="retention-options-item">
                            <span tabIndex="0">{RMResx.RM_JS_SPS_TabLabel_SP}</span>
                            <R.Switch
                                id="raTmSpRetentionSwitch"
                                checked={this.state.isSpRetentionChecked}
                                disabled={this.state.isSpRetentionDis}
                                onChange={this.spRetentionChange} />
                        </div>

                        {this.state.isSpRetentionChecked &&
                            <div className='tm-RetentionSetting-Retention-label'>
                                <div className='tm-RetentionSetting-label-Des tm-normal-title' tabIndex="0">{RMResx.RM_JS_TM_Retention_EXO_Prompt}</div>
                                <div>
                                    <R.Input
                                        id="raTmSPRetentionSettingIpt"
                                        type="text"
                                        placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                        value={this.state.spRetentionActionLabel}
                                        disabled={this.state.exoRetentionActionLabelDis}
                                        height={40}
                                        onChange={this.spRetentionActionLabelChange}
                                        onBlur={this.spRetentionActionLabelBlur}
                                        aria={{ariaLabel:RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}}
                                    />
                                </div>
                                <div className='no-exoRetention-verification'>
                                    <$g.ValidationMsg show={this.state.noSpLabelValueVerify}>
                                        {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                    </$g.ValidationMsg>
                                </div>
                            </div>
                        }
                    </div>
                    <div className='tm-RetentionSetting-exo'>
                        <div className="retention-options-item">
                            <span tabIndex="0">{RMResx.RM_JS_SPS_TabLabel_EXO}</span>
                            <R.Switch
                                id="raTmExoRetentionSwitch"
                                checked={this.state.isExoRetentionChecked}
                                disabled={this.state.isExoRetentionDis}
                                onChange={this.exoRetentionChange} />
                        </div>

                        {this.state.isExoRetentionChecked &&
                            <div className='tm-RetentionSetting-Retention-label'>
                                <div className='tm-RetentionSetting-label-Des tm-normal-title' tabIndex="0">{RMResx.RM_JS_TM_Retention_EXO_Prompt}</div>
                                <div>
                                    <R.Input
                                        id="raTmExoRetentionSettingIpt"
                                        type="text"
                                        placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                        value={this.state.exoRetentionActionLabel}
                                        disabled={this.state.exoRetentionActionLabelDis}
                                        height={40}
                                        onChange={this.exoRetentionActionLabelChange}
                                        onBlur={this.exoRetentionActionLabelBlur}
                                        aria={{ariaLabel:RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}}
                                    />
                                </div>
                                <div className='no-exoRetention-verification'>
                                    <$g.ValidationMsg show={this.state.noExoLabelValueVerify}>
                                        {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                    </$g.ValidationMsg>
                                </div>
                            </div>
                        }
                    </div>
                    <div className='tm-RetentionSetting-exo'>
                        <div className="retention-options-item">
                            <span tabIndex="0">{RMResx.RM_JS_SPS_TabLabel_OneDrive}</span>
                            <R.Switch
                                id="raTmOneDriveRetentionSwitch"
                                checked={this.state.isOneDriveRetentionChecked}
                                disabled={this.state.isOneDriveRetentionDis}
                                onChange={this.oneDriveRetentionChange} />
                        </div>
                        {this.state.isOneDriveRetentionChecked &&
                            <div className='tm-RetentionSetting-Retention-label'>
                                <div className='tm-RetentionSetting-label-Des tm-normal-title' tabIndex="0">{RMResx.RM_JS_TM_Retention_OneDrive_Prompt}</div>
                                <div>
                                    <R.Input
                                        id="raTmOneDriveRetentionSettingIpt"
                                        type="text"
                                        placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}
                                        value={this.state.oneDriveRetentionActionLabel}
                                        disabled={this.state.oneDriveRetentionActionLabelDis}
                                        height={40}
                                        onChange={this.oneDriveRetentionActionLabelChange}
                                        onBlur={this.oneDriveRetentionActionLabelBlur}
                                        aria={{ariaLabel:RMResx.RM_RDM_CreateRule_PlaceHolder_LabelName}}
                                    />
                                </div>
                                <div className='no-exoRetention-verification'>
                                    <$g.ValidationMsg show={this.state.noOneDriveLabelValueVerify}>
                                        {RMResx.RM_JS_RDM_CreateRule_Validation_noRetentionValue}
                                    </$g.ValidationMsg>
                                </div>
                            </div>
                        }
                    </div>
                </div>
                <div>
                    <$g.ValidationMsg show={this.state.noSelectSpOrExoVerify}>
                        {RMResx.RM_TM_Validation_EnforceRetentionNoLocation}
                    </$g.ValidationMsg>
                </div>
            </div>;
    }

    renderTermSettings(selItem) {
        if (selItem.Type == "Term") {
            this.ruleLevels = this.getRuleLevelSource();
            let isDisableOperation = this.state.termSettingDisabled;
            setCheckedStatus("value", "checked", this.ruleLevels, this.state.selectedRuleLevel);
            return <div id="termSettings">
                <div id="divRule">
                    <div>
                        <span className="tm-tree-right-form-label-font" id="ruleLabel" tabIndex="0"
                            data-tooltip aria-label={RMResx.RM_TM_RuleDescTip}
                        >
                            {StringUtil.trimEndColon(RMResx.RM_TM_TermRuleLabel)}&nbsp;
                        </span>
                    </div>
                    {/* <div id="divRuleLevel">
                        <span className="font-color-red">*</span>
                        <span className="tm-rule-details-content-font" tabIndex="0">
                            {RMResx.RM_TM_ObjectLevelTitle}
                        </span>
                        <R.Combobox
                            attrs={{ className: "ruleLevel-combobox" }}
                            width="200px"
                            height={34}
                            searchable={false}
                            textField='name'
                            valueField='name'
                            checkedField='checked'
                            noneText={RMResx.RM_JS_TM_SelectObjectLevelWarter}
                            items={this.ruleLevels}
                            onChange={this.handleRuleLevelChange}
                        />
                        <R.Button
                            classify="theme"
                            text={RMResx.RM_JS_TM_RuleAddLabel}
                            disabled={this.state.termSettingDisabled || !this.state.selectedRuleLevel.value}
                            onClick={this.onAddRuleLevelClick} />
                    </div> */}
                </div>
                <div className="div-table-rule" style={{ display: "" }}>
                    <table id="tbMain" cellPadding="0" cellSpacing="0" tabIndex={this.existsTermRules()? 0 : -1}>
                        <tbody>
                            {this.renderTermRules()}
                        </tbody>
                    </table>
                </div>
                <div id="tm-rule-add-container">
                    <div id="tm_rule_add_content" className={isDisableOperation? "record-table-disabled": ""} 
                        role="combobox" 
                        aria-haspopup="listbox"
                        aria-expanded="false"
                        aria-disabled={isDisableOperation? "true" : "false"} 
                        onClick={this.handleShowRuleLevelList.bind(this, isDisableOperation)} 
                        onKeyDown={this.onKeyDown} 
                        tabIndex="0" >
                        <div id="rule_add_icon" aria-hidden="true">
                            <div className="fia-plus"></div>
                        </div>
                        <span id="rule_add_text">{RMResx.RM_TM_Title_NewRule}</span>
                        <span className="fia-triangle-down"></span>
                    </div>
                    {this.state.showRuleLevelOptions &&
                        <div id="tm_rule_options">
                            <R.Selection
                                id="raTmRuleOptions"
                                items={this.state.ruleLevelItems}
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
                                onChange={this.handleRuleLevelChange} />
                        </div>
                    }
                </div>

                {RM.gData.enviromentName !== Enviroments.ChinaNorth && LicenseHelper.HasOpusILLicense() && 
                    <div id="retention_container">
                        <div id="enforceRetention_title" className="tm-tree-right-form-label-font" tabIndex="0">
                            {StringUtil.trimEndColon(RMResx.RM_TM_EnforceRetention)}
                        </div>
                        <div id="enforceRetention_content">
                            <div id="enforceRetention_switch">
                                <span>
                                    <R.Switch
                                        id="raTmEnforceRetentionSwitch"
                                        checked={this.state.enforceRetention}
                                        disabled={this.state.termSettingDisabled}
                                        onChange={this.onEnforceRetentionChange} />
                                </span>
                                <span id="enforceRetention_switch_desc" className="tm-normal-title" tabIndex="0">
                                    {RMResx.RM_TM_RetentionSwitch_Title}
                                </span>
                                <$g.Popover>{RMResx.RM_TM_RetentionSwitch_Desc}</$g.Popover>
                            </div>
                            {this.renderRetentionOptions()}
                        </div>
                    </div>
                }
                {this.renderTermActivationSettings(selItem)}
            </div>;
        }
        return null;
    }

    renderCreateRulePanel(){
        let selectedTerm = this.state.currentItem || {};
        return  (
            <CreateRule
                id={this.createRuleComponentId}
                callback={this.onRuleOperated}
                history={this.props.history}
                termId={selectedTerm.Id}
                onClose={() => this.creatingTermRuleRef.current = null}
                onSave={() => this.creatingTermRule = this.creatingTermRuleRef.current}
            />
        );
    }

    renderImportPanel() {
        return <R.Panel
            header={RMResx.RM_TM_ImportDialogTitle}
            size={670}
            status={this.state.showImportPanel}
            destroy={true}
            onShow={this.onShowImportPanel}
            onHide={this.onHideImportPanel}
        >
            {this.renderImportOption()}
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleImportCancelClick} />
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleImportSaveClick} />
        </R.Panel>;
    }

    renderImportOption() {
        const {termImportType} = this.state;
        return (
            <div id="importSettingPanel" className='flex flex-column gap-s margin-top-m'>
                <R.Radio 
                    name="radioChooseImport"
                    text={RMResx.RM_JS_TM_ImportFromTemplate}
                    tooltip={RMResx.RM_JS_TM_ImportFromTemplate}
                    value={TermImportOptionType.FromTemplateFile}
                    checked={termImportType == TermImportOptionType.FromTemplateFile}
                    onChange={this.onChooseImportType}
                />
                {termImportType == TermImportOptionType.FromTemplateFile && this.renderImportFromTemplate()}

                {checkPermission("Source_Google", RM.UserResources) && this.hasOpusGoogleLicense &&
                    <>
                        <R.Radio 
                            name="radioChooseImport"
                            text={RMResx.RM_JS_TM_ImportFromGoogle}
                            tooltip={RMResx.RM_JS_TM_ImportFromGoogle}
                            value={TermImportOptionType.FromGoogleLabel}
                            checked={termImportType == TermImportOptionType.FromGoogleLabel}
                            onChange={this.onChooseImportType}
                        />
                        {termImportType == TermImportOptionType.FromGoogleLabel && this.renderImportFromGoogle()}
                    </>
                }
            </div>
        )
    }

    renderImportFromTemplate() {
        const requestVerificationToken = getRequestVerificationToken();
        return (
            <div className='margin-left-xl margin-block-s'>
                <R.Validation>
                    <div ref={r => this.allValidation = r}>
                        <div className="tm-import-download">
                            <form id="tm-form-download" method="POST" action="">
                                <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                                <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                            </form>
                            <span className="tm-import-download-span" onClick={this.handleDownloadTemplate} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_TM_DownLoadTemplate}</span>
                        </div>
                        <div>
                            <div className="tm-import-title" tabIndex="0">
                                <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                            </div>
                            <div>
                                <R.Validation
                                    element="Uploader"
                                    require={RMResx.RM_JS_TM_NoImportFile}>
                                    <R.Uploader
                                        ref={this.uploaderRef}
                                        files={this.state.files}
                                        fileTypes={["CSV", "XLSX", "XML"]}
                                        onUpload={this.handleUpload.bind(this)}
                                        onDelete={this.handleDelete.bind(this)}
                                        multiple={false}
                                        maxSize="20MB"
                                        showMaxSize={true}
                                    />
                                </R.Validation>
                            </div>
                        </div>
                    </div>
                </R.Validation>
            </div>
        )
    }

    renderImportFromGoogle() {
        return (
            <div className='flex flex-column gap-m margin-left-xl'>
                <R.Messagebar 
                    classify="info"
                    message={RMResx.RM_JS_TM_ImportFromGoogleDesc}
                    status={{show: true}}
                    hasClose={false}
                />
                {this.renderChooseTermGroup()}
                {this.renderChooseGoogleTenant()}
            </div>
        )
    }

    renderChooseGoogleTenant(hideErrorMgs = false) {
        const {tenantList, noTenantSelectedVerify, termGroupSelected, showImportPanel: {show}} = this.state;
        return (
            <div>
                <div className='font-semibold margin-bottom-xs' tabIndex={0}>{RMResx.RM_JS_TM_SelectGoogleTenant}</div>
                <R.Multicombobox
                    id="tenantSpecified"
                    width={"100%"}
                    textField="name"
                    items={tenantList}
                    disabledField="disabled"
                    tooltipField="name"
                    hasSelectAll={true}
                    searchable={false}
                    onChange={this.handleTenantSelect}
                    clearable={false}
                    noneText={RMResx.RM_JS_TM_SelectTenantPlaceholder}
                    disabled={show && !termGroupSelected}
                />
                <$g.ValidationMsg show={noTenantSelectedVerify && !hideErrorMgs}>
                    {RMResx.RM_JS_TM_SelectTenantErrorMgs}
                </$g.ValidationMsg>
            </div>
        )
    }

    renderChooseTermGroup() {
        const {termGroupList, tenantList, termGroupSelected} = this.state;
        let termGroupData = [];
        if (tenantList.some(tenant => tenant.TermGroupId == "00000000-0000-0000-0000-000000000000")) {
            termGroupData = termGroupList;
        } else {
            termGroupData = termGroupList.filter(termGroup => tenantList.find(tenant => tenant.TermGroupId == termGroup.value));
        }

        return (
            <div>
                <div className='font-semibold margin-bottom-xs' tabIndex={0}>{RMResx.RM_JS_TM_SelectTermGroup}</div>
                <R.Combobox 
                    width={"100%"}
                    popupMaxHeight={400}
                    textField="name"
                    items={termGroupData}
                    onChange={this.handleTermGroupSelect}
                    searchable={true}
                    clearable={true}
                    noneText={RMResx.RM_JS_TM_SelectTermPlaceholder}
                    value={termGroupSelected?.name}
                />
                <$g.ValidationMsg show={this.state.noTermGroupSelectedVerify}>
                    {RMResx.RM_JS_TM_SelectTermErrorMgs}
                </$g.ValidationMsg>
            </div>
        )
    }

    renderAIRecommendationDialog = () => {
        return (
            <R.Dialog
                id="raTermAIRecommendations"
                header={RMResx.RM_TM_AI_Recommendations_Title}
                width={700}
                status={{ show: this.state.showAIRecommendationDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={this.handleCloseAIRecommendationDialog}
            >
                <R.Validation>
                    <div id="aiRecommendationsValidation">
                        <AIRecommendationsDialog ref={this.aiRecommendationsDialogRef} result={this.state.aiRecommendationResult} />
                    </div>
                    <div id='downloadResult' style={{ display: "none" }} />
                </R.Validation>
                <R.Button slot="buttons" classify="blank" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleCloseAIRecommendationDialog} />
                {this.state.aiRecommendationResult ? (
                    <>
                        <R.Button slot="buttons" classify="blank" text={RMResx.RM_JS_Common_Back} onClick={this.handleBackAIRecommendationDialog} />
                        <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_TM_AI_Recommendations_ExportBtn} onClick={this.handleExportRecommendationResult} />
                    </>
                ) : (
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Next} onClick={this.handleNextAIRecommendationDialog} />
                )}
            </R.Dialog>
        );
    }

    render() {
        let selItem = this.state.currentItem || {};
        let showItemSetting = this.state.currentItem && selItem.Type != "Root";
        let changeSetting = this.state.itemSettingChanged;
        let requestVerificationToken = getRequestVerificationToken();
        return <div id="tmTermManagement" className="rm-tm-main-container">
            <section className="rm-tm-header">
                <Prompt message={RMResx.RM_TM_WithoutSavingMsg} when={changeSetting} />
                <$g.SiteMap data={[SiteMapLinks.BCM_TermManagement]} />
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                />
                <form id="tm-form-export" method="post" action="">
                    <input type="hidden" id="exportFlag" name="exportFlag" value="" />
                    <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                </form>

                {checkPermission("BCM_TermManagement_Admin", RM.UserResources) &&
                    <div id="tm-menu" className="margin-top-m margin-bottom-m">
                        {isMultiGeoMainDC && (
                            RM.gData.enableAIRecommendationFeature ? (
                                <>
                                    <R.Button
                                        id="raAIRecommendationsBtn"
                                        primary={true}
                                        classify="theme"
                                        text={RMResx.RM_TM_AI_Recommendations_Btn}
                                        onClick={this.handleAIRecommendationClick} />

                                    <R.Button
                                        id="raTmSyncBtn"
                                        icon="fia-sync"
                                        text={RMResx.RM_TM_SyncLabel}
                                        onClick={this.handleTermSync} />
                                </>
                            ) : (
                                <R.Button
                                    id="raTmSyncBtn"
                                    primary={true}
                                    classify="theme"
                                    text={RMResx.RM_TM_SyncLabel}
                                    onClick={this.handleTermSync} />
                            )
                        )}
                        <R.Button
                            id="raTmExportTermBtn"
                            icon="fia-export-settings"
                            text={RMResx.RM_JS_TM_ExportTerm}
                            onClick={this.handleExportBtn} />

                        {isMultiGeoMainDC && (
                            <R.Button
                                id="raTmImportTermBtn"
                                icon="fia-import"
                                text={RMResx.RM_JS_TM_Import}
                                onClick={this.handleTermImport} />
                        )}
                    </div>
                }
            </section>
            <section className="rm-tm-content">
                <div className="rm-tm-splitter-container">
                    <R.Splitter minAsize="25%" minBsize="60%" defaultAsize="40%">
                        <div className="ra-splitter-left">
                            <div>
                                <div className="ra-splitter-search">
                                    <R.Searchbox
                                        title=''
                                        width='100%'
                                        placeholder={RMResx.RM_JS_TM_AllSearchTxt}
                                        disabled={false}
                                        onSearch={this.onSearch}
                                    />
                                </div>
                            </div>
                            <$g.TreeView
                                id="treeview"
                                classicMode
                                items={this.state.treeData}
                                searchKey={this.state.searchKey}
                                treeContext={this.treeContext}
                            />
                        </div>
                        <div className="ra-splitter-right rm-settings-container" id="tmSettingsContainer">
                            <div className="rm-settings-header">
                                <div className="ra-splitter-head-title" tabIndex="0">{RMResx.RM_TM_GSetingLabel}</div>
                            </div>
                            <$g.Container show={showItemSetting} className="rm-settings-content">
                                {selItem.Type == "Term" && this.renderInheritBar()}
                                {this.renderSelectedItemName()}
                                <div tabIndex="0" className="tm-tree-right-form-label-font"
                                    style={{ margin: "24px 0 8px 0" }}>
                                    {StringUtil.trimEndColon(RMResx.RM_TM_TermDescLabel)}
                                </div>
                                <div>
                                    <R.Input
                                        type="textarea"
                                        className={"tm-textarea"}
                                        value={!selItem.Description ? "" : selItem.Description}
                                        // width={606}
                                        height={88}
                                        onChange={this.onDescriptionChange}
                                        aria={{ariaLabel:RMResx.RM_TM_TermDescLabel}}
                                    />

                                    <$g.ValidationMsg
                                        show={!!selItem.Description && selItem.Description.length > 5000}>
                                        {RMResx.RM_TM_CustomProperties_NameLengthLimit}
                                    </$g.ValidationMsg>
                                </div>
                                {selItem.Type == "Term" && RM.gData.enableCustomizationApp && <div>
                                    <div tabIndex="0" className="tm-tree-right-form-label-font"
                                        style={{ margin: "24px 0 8px 0" }}>
                                        {RMResx.RM_TM_AdvanceSetting}
                                    </div>
                                    <div>
                                        <R.Input
                                            type="textarea"
                                            className={"tm-textarea"}
                                            value={!selItem.AdvanceSettings ? "" : selItem.AdvanceSettings}
                                            height={88}
                                            onChange={this.onAdvanceSettingsChange}
                                            aria={{ ariaLabel: RMResx.RM_TM_AdvanceSetting }}
                                        />

                                        <$g.ValidationMsg
                                            show={!!selItem.AdvanceSettings && selItem.AdvanceSettings.length > 5000}>
                                            {RMResx.RM_TM_CustomProperties_NameLengthLimit}
                                        </$g.ValidationMsg>
                                    </div>
                                </div>}
                                {this.renderTermGroup(selItem)}
                                {this.renderTermSettings(selItem)}
                            </$g.Container>
                            <$g.Container show={showItemSetting} className="rm-settings-footer">
                                <div className="tm-settings-footer-button">
                                    <R.Button
                                        text={RMResx.RM_JS_Common_Cancel}
                                        disabled={!changeSetting}
                                        onClick={this.onCancelChangedClick} />
                                    {isMultiGeoMainDC && (
                                        <R.Button
                                            id="raTmSaveBtn"
                                            primary={true}
                                            classify="theme"
                                            text={RMResx.RM_JS_Common_Save}
                                            disabled={!changeSetting}
                                            onClick={this.onSaveSettingClick} />
                                    )}
                                </div>
                            </$g.Container>
                        </div>
                    </R.Splitter>
                </div>
                
                <RuleDetail
                    ref={r => this.ruleDetail = r}
                >
                </RuleDetail>
            </section>
            {this.renderCreateRulePanel()}
            {this.renderImportPanel()}
            {this.renderAIRecommendationDialog()}
        </div>;
    }
}
