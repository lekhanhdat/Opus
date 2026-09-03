import { Prompt } from 'react-router';
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import TreeNodeContent from "../Common/Tree/NodeContents/TermManagementNodeContent";
import { bindEvents, showToast, getRequestVerificationToken } from "../../Utilities/CommonUtil";
import StringUtil from '../../Utilities/StringUtil';
import * as Constants from "./Constants";
import TemplateSuiteSettings  from "./TemplateSuiteSettings";
import TemplateSettings from "./TemplateSettings";
import ViewTemplateSettings from "./ViewTemplateSettings";
import ViewSuiteSettings from "./ViewSuiteSettings";
import ExistingTemplatesForm from "./ExistingTemplatesForm";
import GlobalUniqueIdSettingForm from "./GlobalUniqueIdSettingForm";

import "../../Less/PRM/LocationManagement.less";
import "../../Less/PRM/TemplateManagement.less";

const EmptyGUID = "00000000-0000-0000-0000-000000000000";

export default class TemplateManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.initBindings();
        this.getTreeData();
        this.treeContext = this.getTreeContext();
        this.state = {
            treeData: [],
            selectedItem: null, //selected item
            currentItem: null,  //current selected item, clone from "selectedItem"
            itemSettingChanged: false, //if current selected item's setting changed
            allSuiteItems: [],
            showAddTemplatePanel: {show: false},
            pageDisplayMode: Constants.TemplateDisplayMode.None,
            existingTemplateItems: [],
            parentUniqueId: "",
            templateIdList: [],
            checkedTemplateIds: [],
            showUniqueIDSettingsPanel: { show: false },
            showUniqueIdDialog: false,
            uniqueIdMode: "0",
            isGlobalUniqueIdSetting: false,
            uniqueIdSettingInfo: null,
            showImportTemplatePanel:{ show: false },
        };
        this.templateSuiteSettingsComponentId = "raTemplateSuiteSettings";
        this.templateSettingsComponentId = "raTemplateSettings";
        this.templateDetailsComponentId = "raTemplateSettingsDetail";
        this.suiteDetailsComponentId = "raSuiteSettingsDetail";
        this.existingTemplatePanelId = "raExistingTemplatePanelId";
        this.globalUniqueIdPanelId = "raGlobalUniqueIdPanelId";
        this.defaultCategoryAndColumnsCache = [];
    }

    componentInit () {
        this.getAllDefaultCategoriesAndColumns();
        this.initIsGlobalUniqueIdSetting();
    }

    initBindings() {
        bindEvents(this, "onSearch", "onStopSearch","onSaveSettingClick", "onCancelChangedClick", "notifySettingsChanged",
        "getValidationResult", "getSuiteItemData", "showNewSuiteSettings", "showImportTemplatePanle", "showEditSuiteSettings", "showEditTemplateSettings",
        "getTemplateItemData", "showNewTemplateSettings", "onBackClick", "saveExistingTemplates", "showExistingTemplatePanel",
        "onExistingTemplateChanged", "showUniqueIdSettings", "saveUniqueIdSettings", "isDefaultSuite", "isDefaultTemplate",
        "onCancelPhysicalUniqueIdSetting", "onSavePhysicalUniqueIdSetting", "onPhysicalUniqueIdSettingChanged",
        "showErrorMessage"
        );
    }

    initIsGlobalUniqueIdSetting() {
        let option = {
            url: "/api/TemplateManagementApi/LoadingUniqueIdSetting",
            method: "get",
        };
        fetchUtility(option).then((result) => {
            let uniqueIdSetting = JSON.parse(result);
            if (uniqueIdSetting) {
                this.setState({ 
                    showUniqueIdDialog: false,
                    isGlobalUniqueIdSetting: uniqueIdSetting.IsGlobalSetting,
                    uniqueIdSettingInfo: uniqueIdSetting
                });
            }
        }).catch((e) => {

        });
    }

    copyProps(fromObj, toObj, propNames) {
        if (fromObj && toObj && propNames) {
            for (var i = 0; i < propNames.length; i++) {
                toObj[propNames[i]] = fromObj[propNames[i]];
            }
        }
    }

    isDefaultSuite(uniqueId){
        return Constants.DefaultSuiteUniqueIds.includes(uniqueId);
    }

    isDefaultTemplate(uniqueId)
    {
        return Constants.DefaultTemplateUniqueIds.includes(uniqueId);
    }

    getTreeContext() {
        var getBrowseTreeReqDto = function (parentItem) {
            let poItem = parentItem.origin;
            let dto = {Node: null,  PagingInfo: {PageIndex: parentItem.pagerIndex + 1, PageSize: parentItem.pagerSize} };
            dto.Node = {
                UniqueId: poItem.UniqueId || EmptyGUID,
                Name: poItem.Name,
                Type: poItem.Type,
                StartFromType: poItem.StartFromType || Constants.StartFromType.None,
                TemplateIdList: poItem.TemplateIdList || [],
                IsUnderDefaultSuite : poItem.IsUnderDefaultSuite
            };
            return dto;
        }
        return {
            treeType: 3,    //1:TermManagement, 2:LocationManagement, 3: TemplateManagement
            searchKey: "",
            nodeContentComponent: TreeNodeContent,
            singleSelection: true,
            showrRightArrow: true,
            transToTreeNodeObject(oitem) {
                let itemsCount = !this.pagerByServer ? (!oitem.Children ? 0 : oitem.Children.length) : oitem.ChildrenCount;
                let nodeName = oitem.Name;
                let nodeText = RMResx[nodeName] ? RMResx[nodeName] : nodeName;
                return {
                    origin: oitem,
                    nodeKey: this.getTemplateIdsPath(oitem),
                    nodeType: oitem.Type,
                    text: nodeText,
                    disableSelect: oitem.Type == Constants.TemplateTreeNodeType.Root, 
                    expanded: (!!this.searchKey && oitem.hasMatchChildren) || oitem.Type == Constants.TemplateTreeNodeType.Root,
                    loaded: !!this.searchKey || oitem.ChildrenCount == 0 || !!oitem.Children,
                    enableContextMenu: true,
                    isAllowEditName: false,
                    items: oitem.Children,
                    itemsCount: itemsCount,
                    hasChildren: itemsCount > 0,
                    pagerByServer: true,
                    pagerSize: 15,
                    pagerIndex: 0,
                };
            },
            getTemplateIdsPath(oitem) {
                if(oitem.Type == Constants.TemplateTreeNodeType.Root) {
                    return oitem.UniqueId; //guid.empty
                }
                var ids = oitem.TemplateIdList;
                if(ids && ids.length > 0) {
                    return  ids.join(";").toLowerCase();
                }
                return StringUtil.newGuid();
            },
            onLoadNodes (parentItem, funcSuccess, funcFail) {
                let poItem = parentItem.origin;
                fetchUtility({
                    url: "/api/TemplateManagementApi/Browser",
                    data: getBrowseTreeReqDto(parentItem)
                }).then(res => {
                    if (res) {
                        funcSuccess(res.Children, res);
                    } 
                }).catch(e => funcFail(e));
                return [];
            },
            confirmOnNodeSelected: (item, funcAllow) => this.onNodeSelected(item.origin, funcAllow),
            refreshSelectedNodeInfo: this.refreshSelectedNodeInfo.bind(this),
            createSuiteItem: this.showNewSuiteSettings,
            importSuiteItems: this.showImportTemplatePanle,
            addExistingTemplateItem: this.showExistingTemplatePanel,
            editSuiteItem: this.showEditSuiteSettings,
            createTemplateItem: this.showNewTemplateSettings,
            editTemplateItem: this.showEditTemplateSettings,
            isDefaultSuite: this.isDefaultSuite,
            isDefaultTemplate: this.isDefaultTemplate,
            showErrorMessage: this.showErrorMessage.bind(this),
            showMessageTip: this.showMessageTip,
            hideMessageTip: this.hideMessageTip,
            
        };
    }

    getTreeData() {
        let option = {
            url: "/Api/TemplateManagementApi/InitTree",
            method: "get"
        };
        fetchUtility(option).then((data) => {
            if (data) {
                    this.treeContext.searchKey = "";
                    this.treeContext.pagerByServer = true;
                    this.resetTreeData([data]);
                };
            })
        .catch((e) => {

        });
    }

    getAllDefaultCategoriesAndColumns() {
        let option = {
            url: "/Api/TemplateManagementApi/GetAllDefaultCategoryAndColumn",
            method: "get",
        };
        fetchUtility(option).then((result) => {
            let data = JSON.parse(result);
            if(data)
            {
                this.defaultCategoryAndColumnsCache = data;
            }
        }).catch((e) => {
        });
    }

    showErrorMessage(msg) {
        showToast.error(msg);
    }

    hideMessagebox() {
        $$.messagedialog(false);
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
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.setNewSelectedItem(this.state.selectedItem);
                        this.hideMessagebox();
                    }
                }                
            ]
        };
        $$.messagedialog(true, args);
    }

    onBackClick() {
        this.setNewSelectedItem(RM.deepcopy(this.state.selectedItem));
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

    onSaveSettingClick(e) {
        let pageMode = this.state.pageDisplayMode;
        this.isSuiteSettingsPageMode(pageMode) && this.saveSuiteSettings();
        this.isTemplateSettingsPageMode(pageMode) && this.saveTemplateSettings();
    }

    isSuiteSettingsPageMode(mode) {
        return [Constants.TemplateDisplayMode.NewSuiteSettings,Constants.TemplateDisplayMode.EditSuiteSettings].includes(mode);
    }

    isTemplateSettingsPageMode(mode) {
        return [Constants.TemplateDisplayMode.NewTemplateSettings, Constants.TemplateDisplayMode.EditTemplateSettings].includes(mode);
    }

    notifySettingsChanged(isChanged = true)
    {
        this.setState({itemSettingChanged: isChanged});
    }

    onSearch(args) {
        this.searchData(args.value);
    }

    onStopSearch(args) {
        this.getTreeData();
    }

    processHasMatchChildren(item) {
        let hasMatchChildren = false;
        if (item && item.SubLocations) {
            item.SubLocations.forEach((subitem) => {
                if (!hasMatchChildren && subitem.Name.indexOf(this.treeContext.searchKey) > -1) {
                    hasMatchChildren = true;
                }
                hasMatchChildren |= this.processHasMatchChildren(subitem);
            });
        }
        return item.hasMatchChildren = hasMatchChildren;
    }

    refreshSelectedNodeInfo(item, actionType) {
        let selItem = this.state.selectedItem;
        if (!selItem || (item.UniqueId != selItem.UniqueId)) {
            return;
        }
        let props;
        switch (actionType) {
            case 4: //delete item
                this.setState({
                    itemSettingChanged: false,
                    selectedItem: null,
                    currentItem: null,
                    pageDisplayMode: Constants.TemplateDisplayMode.None
                });
                return;
            case 1:
                props = ["Name"];
                break;
            default:
                props = [];
                break;
        }

        this.copyProps(item, this.state.selectedItem, props);
        this.copyProps(item, this.state.currentItem, props);

        this.setState({
            selectedItem: this.state.selectedItem,
            currentItem: this.state.currentItem
        });
    }

    refreshSelectedNode = (updateProps, isReload) => {
        let selectNodes = this.treeContext.selectedNodes;
        if (selectNodes) {
            for (const key in selectNodes) {
                const selNode = selectNodes[key];
                if (updateProps) {
                    if(isReload){
                        selNode.props.item.loaded = false;
                        selNode.reload(0);
                    }
                    Object.assign(selNode.props.item.origin, updateProps);
                    selNode.reRender();
                }
            }
        }
    };

    showImportTemplatePanle(){
        this.setState({
            showImportTemplatePanel : {show : true}
        });
    }

    showNewSuiteSettings(oItem, refreshOperationTreeNode) {
        this.refreshOperationNode = refreshOperationTreeNode;
        this.setState({
            pageDisplayMode: Constants.TemplateDisplayMode.NewSuiteSettings,
        }, ()=> {
            this.dispatch(this.templateSuiteSettingsComponentId, "new");
        })
    }

    showEditSuiteSettings(oItem, updateTreeFunc)
    {
        this.updateSuiteItemTreeNode = updateTreeFunc;
        this.setState({
            pageDisplayMode: Constants.TemplateDisplayMode.EditSuiteSettings,
        }, ()=> {
            this.dispatch(this.templateSuiteSettingsComponentId, "init", oItem.UniqueId);
        })
    }

    showNewTemplateSettings(oItem, newTemplateType, refreshOperationTreeNode) {
        this.refreshOperationNode = refreshOperationTreeNode;
        this.setState({
            pageDisplayMode: Constants.TemplateDisplayMode.NewTemplateSettings
        }, ()=> {
            this.dispatch(this.templateSettingsComponentId, "new", this.getNewTemplateRelationInfo(oItem, newTemplateType));
        });
        this.updateUniqueIdSettingDialogShowStatus(!this.state.uniqueIdSettingInfo);
    }

    getNewTemplateRelationInfo(oItem, newTemplateType) {
        return {
            TemplateIdList: oItem.TemplateIdList,
            TemplateType: newTemplateType,
            DefaultCategoryAndColumnsInfo: this.defaultCategoryAndColumnsCache[newTemplateType]
        };
    }

    showEditTemplateSettings(oItem, updateTreeFunc) {
        this.updateTemplateItemTreeNode = updateTreeFunc;
        this.setState({
            pageDisplayMode: Constants.TemplateDisplayMode.EditTemplateSettings,
            itemSettingChanged: false
        }, ()=> {
            this.dispatch(this.templateSettingsComponentId, "init",  oItem.UniqueId, oItem.TemplateIdList);
        })
    }

    replaceSpecialCharacters(str) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp("\"", "ig");
        str = str.replace(reg1, "＆");
        str = str.replace(reg2, "＂");
        return str;
    }

    resetTreeData(data) {
        let treeData = typeof(data) == "string" ?$.parseJSON(data): data;   // Fortify Issue Type: JSON Injection; Sink Details: init tree data; Ignore Reason: 前后台对象存在对应关系
        if (this.treeContext.searchKey) {
            if (treeData) {
                this.processHasMatchChildren(treeData);
                treeData = [treeData];
            } else {
                treeData = [];
            }
        }
        this.setState({ treeData: treeData });
    }

    searchData(key) {
        key = !key ? "" : key.trim();
        if (key.length == 0) {
            this.getTreeData();
        } else {
            $.ajax({
                type: "GET",
                url: "/api/LocationManagementApi/Search",
                //contentType: 'application/json;charset=utf-8',
                data: "locationStr=" + this.replaceSpecialCharacters(key),
                async: true,
                beforeSend: function () {
                    $$.loading(true);
                },
                complete: function () {
                    $$.loading(false);
                },
                success: (data) => {
                    this.treeContext.searchKey = key;
                    this.treeContext.pagerByServer = false;
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
        this.setState({
            pageDisplayMode: this.getDefaultPageDisplayModeByItem(item),
            selectedItem: item,
            currentItem: JSON.parse(JSON.stringify(item)),
            itemSettingChanged: false
        }, ()=> {
            this.initItemSettings();
        });
    }

    getDefaultPageDisplayModeByItem(item) {
        if(!item) 
        {
            return Constants.TemplateDisplayMode.None;
        }
        if(this.isSuiteNode(item.Type))
        {
            return Constants.TemplateDisplayMode.ViewSuiteDetails;
        }
        if(this.isTemplateNode(item.Type))
        {
            return Constants.TemplateDisplayMode.ViewTemplateDetails;
        }
    }

    initItemSettings() {
        let selectedItem = this.state.selectedItem;
        if(selectedItem)
        {
            let nodeType = selectedItem.Type;
            if(this.isSuiteNode(nodeType))
            {
                this.initSuiteItemSettings();
            }
            if(this.isTemplateNode(nodeType))
            {
                this.initTemplateDetails();
            }
        }
        this.setState({TemplateDisplayMode: Constants.TemplateDisplayMode.None});
    }

    initSuiteItemSettings() {
        this.dispatch(this.suiteDetailsComponentId, "init", this.state.selectedItem.UniqueId);
    }

    initTemplateDetails() {
        this.dispatch(this.templateDetailsComponentId, "init", this.state.selectedItem.UniqueId, this.state.selectedItem.TemplateIdList);
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

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    isSuiteNode(nodeType)
    {
        let suiteNodeTypes = [Constants.TemplateTreeNodeType.Suite];
        return suiteNodeTypes.indexOf(nodeType) > -1;
    }

    isTemplateNode(nodeType)
    {
        let templateNodeTypes = [Constants.TemplateTreeNodeType.Box, 
            Constants.TemplateTreeNodeType.Folder, Constants.TemplateTreeNodeType.Records,  Constants.TemplateTreeNodeType.Custom];
        return templateNodeTypes.indexOf(nodeType) > -1;
    }

    saveSuiteSettings() {
        let callBack = (dto) => {
            let isEditMode = dto.UniqueId != EmptyGUID;
            let reqUrl = isEditMode? "/api/TemplateManagementApi/UpdateSuite":"/api/TemplateManagementApi/CreateSuite";
            let option = {
                url: reqUrl,
                method: "POST",
                data: dto
            };
            $$.loading(true);
            fetchUtility(option).then((res) => {
                switch (res) {
                    case Constants.SaveTemplateResult.Success:
                        showToast.success(!isEditMode? RMResx.RM_PRM_TM_CreateSuite_Success : RMResx.RM_PRM_TM_EditSuite_Success);
                        this.setState({
                            itemSettingChanged: false, 
                            pageDisplayMode: !isEditMode? Constants.TemplateDisplayMode.None: this.state.pageDisplayMode},
                            () => {
                                if(!isEditMode)
                                {
                                    this.refreshOperationNode();
                                }else {
                                    if(typeof(this.updateSuiteItemTreeNode) === "function") {
                                        this.updateSuiteItemTreeNode(dto);
                                    }else {
                                        this.refreshSelectedNode({Name: dto.Name, StartFromType: dto.StartFromType, Type: Constants.TemplateTreeNodeType.Suite}, true);
                                    }
                                }
                            });
                        break;
                    case Constants.SaveTemplateResult.NameDuplicate:
                        showToast.error(RMResx.RM_Template_SuiteNameDuplicate);
                        break;
                    default:
                        showToast.error(RMResx.RM_PRM_TM_Suite_SaveFailed);
                        break;
                }
                $$.loading(false);
            }).catch((e) => {
                showToast.error(RMResx.RM_PRM_TM_Suite_SaveFailed);
                $$.loading(false);
            });
        };
        this.dispatch(this.templateSuiteSettingsComponentId, "onSave", callBack);
    }

    getQueryExistingTemplatesDto(oItem) {
        let dto = {
            UniqueId: oItem.UniqueId,
            TemplateIdList: oItem.TemplateIdList
        };
        return dto;
    }

    showExistingTemplatePanel(oItem, refreshOperationTreeNode) {
        this.refreshOperationNode = refreshOperationTreeNode;
        let option = {
            url: `/api/TemplateManagementApi/GetExistingTemplatesInfo`,
            method: "post",
            data: this.getQueryExistingTemplatesDto(oItem)
        };
        fetchUtility(option).then((result) => {
            if (result && result.Templates) {
                this.setState({
                    existingTemplateItems: this.initExitsTemplateItems(result.Templates) || [],
                    parentUniqueId: oItem.UniqueId,
                    templateIdList: oItem.TemplateIdList,
                    checkedTemplateIds: [],
                    showAddTemplatePanel: { show: true }
                });
            } else {
                showToast.warn(RMResx.RM_PRM_TM_NoExistingTemplates);
            }
        }).catch((e) => {

        });
    }

    initExitsTemplateItems(templates) {
        return templates.map(item => {
            return {
                name: item.Name,
                value: item.UniqueId,
                tooltip: item.Name,
                checked: false,
                group: this.getMultiComboBoxGroupName(item.Type)
            };
        });
    }

    getMultiComboBoxGroupName(itemType) {
        let mapping = {
            [Constants.TemplateTreeNodeType.Custom]: RMResx.RM_PRM_TM_ExistingCustomTemplate_GroupTitle,
            [Constants.TemplateTreeNodeType.Box]: RMResx.RM_PRM_TM_ExistingBoxTemplate_GroupTitle,
            [Constants.TemplateTreeNodeType.Folder]: RMResx.RM_PRM_TM_ExistingFolderTemplate_GroupTitle,
            [Constants.TemplateTreeNodeType.Records]: RMResx.RM_PRM_TM_ExistingRecordTemplate_GroupTitle
        };
        return mapping[itemType] || "";
    }
    
    saveExistingTemplates() {
        let callBack = (dto) => {
            let option = {
                url: `/api/TemplateManagementApi/AddExistingTemplates`,
                method: "post",
                data: dto
            };
            fetchUtility(option).then((result) => {
                if(result)
                {
                    this.setState({ showAddTemplatePanel: { show: false } });
                    showToast.success(RMResx.RM_PRM_TM_AddExistingTemplate_Success);
                    this.refreshOperationNode();
                } else {
                    this.dispatch(this.existingTemplatePanelId, "showErrorMessage", RMResx.RM_PRM_TM_AddExistingTemplate_Fail);
                }
            }).catch((e) => {
                
            });
        };
        this.dispatch(this.existingTemplatePanelId, "onSave", callBack);
        return false;
    }

    saveUniqueIdSettings() {
        let callBack = (dto) => {
            let option = {
                url: `/api/TemplateManagementApi/SaveGlobalUniqueIdSettings`,
                method: "post",
                data: dto
            };
            fetchUtility(option).then((result) => {
                if (result) {
                    let uniqueIdSettingInfo = RM.deepcopy(this.state.uniqueIdSettingInfo);
                    Object.assign(uniqueIdSettingInfo, dto);
                    this.setState({ showUniqueIDSettingsPanel: { show: false }, uniqueIdSettingInfo: uniqueIdSettingInfo });
                    showToast.success(RMResx.RM_PRM_TM_SaveUniqueId_SuccessMsg);
                } else {
                    showToast.error(RMResx.RM_PRM_TM_SaveUniqueId_FailedMsg);
                }
            }).catch((e) => {
    
            });
        };
        this.dispatch(this.globalUniqueIdPanelId, "onSave", callBack);
        return false;
    }

    saveTemplateSettings() {
        let callBack = (dto) => {
            let option = {
                url: "/Api/TemplateManagementApi/SaveTemplateWithColumns",
                method: "post",
                data: dto
            };
            $$.loading(true);
            fetchUtility(option).then((result) => {
                switch (result.SaveTemplateResult) {
                    case Constants.SaveTemplateResult.Success:
                        let isNewTemplate = this.state.pageDisplayMode == Constants.TemplateDisplayMode.NewTemplateSettings;
                        showToast.success(isNewTemplate? RMResx.RM_PRM_TM_CreateTemplate_Success : RMResx.RM_PRM_TM_EditTemplate_Success);
                        if(isNewTemplate)
                        {
                            this.refreshOperationNode && this.refreshOperationNode();
                        }else {
                            
                            if(typeof(this.updateTemplateItemTreeNode) === "function")
                            {
                                this.updateTemplateItemTreeNode(dto)
                            }else {
                                this.refreshSelectedNode({Name: dto.name}, true);
                            }
                        }
                        this.setState({ itemSettingChanged: false, pageDisplayMode: isNewTemplate? Constants.TemplateDisplayMode.None: this.state.pageDisplayMode});
                        break;
                    case Constants.SaveTemplateResult.MissUniqueIdSettingMode:
                        this.setState({ showUniqueIdDialog: true });
                        break;
                    case Constants.SaveTemplateResult.PrefixDuplicate:
                        showToast.warn(RMResx.RM_Template_DuplicatePrefix);
                        break;
                    case Constants.SaveTemplateResult.NameDuplicate:
                        showToast.warn(RMResx.RM_Template_TemplateNameDuplicate);
                        break;
                    case Constants.SaveTemplateResult.Failed:
                        showToast.error(RMResx.RM_EditTemplate_SaveFailedMessage);
                        break;
                    case Constants.SaveTemplateResult.CustomTemplateExceedMaxDepth:
                        showToast.error(RMResx.RM_Template_ExceedMaxDepthError);
                        break;
                    default:
                        break;
                }
                $$.loading(false);
            }).catch((e) => {
                showToast.error(RMResx.RM_EditTemplate_SaveFailedMessage);
                $$.loading(false);
            });
        };
        this.dispatch(this.templateSettingsComponentId, "onSave", callBack);
    }

    showUniqueIdSettings() {
        this.setState({
            showUniqueIDSettingsPanel: { show: true }});
    }

    onCancelPhysicalUniqueIdSetting() {
        this.setState({ showUniqueIdDialog: false });
    }

    onSavePhysicalUniqueIdSetting() {
        let isGlobal = this.state.uniqueIdMode === "1" ? "true" : "false";
        let option = {
            url: "/Api/TemplateManagementApi/ToggleGlobalUniqueIdSettings",
            method: "post",
            data: isGlobal
        };
        fetchUtility(option).then((result) => {
            if (result) {
                this.setState({
                    showUniqueIdDialog: false,
                    isGlobalUniqueIdSetting: this.state.uniqueIdMode === "1"
                }, ()=> {
                    this.initIsGlobalUniqueIdSetting();
                });
            }
        }).catch((e) => {

        });
    }

    updateUniqueIdSettingDialogShowStatus(status) {
        this.setState({showUniqueIdDialog: status});
    }

    onPhysicalUniqueIdSettingChanged(val) {
        this.setState({ uniqueIdMode: val });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    renderSuiteSettings() {
        let needRender = [Constants.TemplateDisplayMode.NewSuiteSettings, Constants.TemplateDisplayMode.EditSuiteSettings].includes(this.state.pageDisplayMode);
        return needRender && <div>
                        <TemplateSuiteSettings
                                id={this.templateSuiteSettingsComponentId}
                                notifySettingsChanged={this.notifySettingsChanged}
                        /></div>;
    }

    renderTemplateSettings() {
        return  (this.state.pageDisplayMode == Constants.TemplateDisplayMode.NewTemplateSettings
            || this.state.pageDisplayMode == Constants.TemplateDisplayMode.EditTemplateSettings) &&
                <div>
                    <TemplateSettings
                        id={this.templateSettingsComponentId}
                        notifySettingsChanged={this.notifySettingsChanged}
                        isGlobalUniqueIdSetting={this.state.isGlobalUniqueIdSetting}
                    /></div>;
    }

    renderViewTemplateDetails() {
        return this.state.pageDisplayMode == Constants.TemplateDisplayMode.ViewTemplateDetails && 
                <div>
                    <ViewTemplateSettings
                        id={this.templateDetailsComponentId}
                        uniqueIdSettings={this.state.uniqueIdSettingInfo}
                    /></div>;
    }

    renderViewSuiteDetails() {
        return this.state.pageDisplayMode == Constants.TemplateDisplayMode.ViewSuiteDetails && <ViewSuiteSettings
                    id={this.suiteDetailsComponentId}
                />;
    }

    renderSettingHeaderContent() {
        let pageDisplayMode = this.state.pageDisplayMode;
        if(pageDisplayMode == Constants.TemplateDisplayMode.ViewTemplateDetails)
        {
            return this.getTemplateDetailsHeader();
        }
        if(pageDisplayMode == Constants.TemplateDisplayMode.ViewSuiteDetails) {
            return this.getSuiteDetailsHeader();
        }
        return this.getTemplateSettingHeader();
    }

    getTemplateDetailsHeader() {
        return this.state.selectedItem && <div >
                <div className="ra-splitter-head-title" style={{paddingBottom:0}}>{RMResx.RM_PRM_PRE_PanelTitle_ViewDetail}</div>
                <div className="ra-splitter-header-name header-name-row">
                        <div className="row-left">
                            {this.state.selectedItem.Name && <span className="fia-folder type-icon"></span>}
                            <span>{StringUtil.toI18N(this.state.selectedItem.Name)}</span>
                        </div>
                <div tabIndex="0" className="row-right fia-edit edit-icon" onClick={this.showEditTemplateSettings.bind(this, this.state.selectedItem)} onKeyDown={this.onKeyDown} aria-label={RMResx.RM_JS_Common_Edit}></div>
                </div>
            </div>;
    }

    getSuiteDetailsHeader() {
        return this.state.selectedItem && <div >
                <div className="ra-splitter-head-title" style={{paddingBottom:0}}>{RMResx.RM_PRM_PRE_PanelTitle_ViewDetail}</div>
                <div className="ra-splitter-header-name header-name-row">
                        <div className="row-left">
                            {this.state.selectedItem.Name && <span className="fia-folder type-icon"></span>}
                            <span>{StringUtil.toI18N(this.state.selectedItem.Name)}</span>
                        </div>
                        <div tabIndex="0" className="row-right fia-edit edit-icon" onClick={this.showEditSuiteSettings.bind(this, this.state.selectedItem)} onKeyDown={this.onKeyDown} aria-label={RMResx.RM_JS_Common_Edit}></div>
                </div>
            </div>;
    }

    getTemplateSettingHeader()
    {
        return <div >
                <div className="ra-splitter-head-title">
                    {this.isShowBackIcon() && <span tabIndex="0" className="fia-arrow-line-left back-icon" onClick={this.onBackClick} onKeyDown={this.onKeyDown} aria-label={RMResx.RM_JS_BCM_Explorer_Button_Back}></span>}
                    <span>{this.getSettingHeaderTitle()}</span>
                </div>
            </div>;
    }

    isShowBackIcon() {
        return [
                Constants.TemplateDisplayMode.NewTemplateSettings, 
                Constants.TemplateDisplayMode.EditTemplateSettings,
                Constants.TemplateDisplayMode.NewSuiteSettings,
                Constants.TemplateDisplayMode.EditSuiteSettings
            ].includes(this.state.pageDisplayMode) && this.state.selectedItem;
    }

    getSettingHeaderTitle() {
        let mapping = {
            [Constants.TemplateDisplayMode.NewTemplateSettings]: RMResx.RM_NewTemplate_PateTitle,
            [Constants.TemplateDisplayMode.EditTemplateSettings]: RMResx.RM_EditTemplate_PateTitle,
            [Constants.TemplateDisplayMode.NewSuiteSettings]: RMResx.RM_PRM_TM_Btn_NewSuite,
            [Constants.TemplateDisplayMode.EditSuiteSettings]: RMResx.RM_PRM_TM_MenuBtn_EditSuite,
        };
        return mapping[this.state.pageDisplayMode] || RMResx.RM_TM_GSetingLabel;
    }

    isShowSaveAndCancelButtons()
    {
        let hideButtonPageMode = [Constants.TemplateDisplayMode.None, Constants.TemplateDisplayMode.ViewTemplateDetails, Constants.TemplateDisplayMode.ViewSuiteDetails];
        return !(hideButtonPageMode.indexOf(this.state.pageDisplayMode) > -1);
    }

    renderAddExistingTemplatePanel() {
        return <R.Panel
            id="addExistingTemplatesPanel"
            header={RMResx.RM_PRM_TM_Btn_AddExistingTemplate}
            size={600}
            status={this.state.showAddTemplatePanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <ExistingTemplatesForm
                    id={this.existingTemplatePanelId}
                    items={this.state.existingTemplateItems}
                    parentUniqueId={this.state.parentUniqueId}
                    templateIdList={this.state.templateIdList}
                />
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showAddTemplatePanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveExistingTemplates} />
            </>
        </R.Panel>;
    }

    handleDownloadTemplate = (e) => {
        let downloadTemplate = StringUtil.newGuid();
        var $downloadStatusKey = $("#importDownloadFlag");
        $downloadStatusKey.val(downloadTemplate);

        $("#tm-form-download")
            .attr("action", "/api/TemplateManagementApi/DownloadTemplate")
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

    handleImportFromTemplateFile = () => {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        const formData = new FormData();
        formData.append('fileUp', this.files.file, this.files.fileName);
        fetch('/api/TemplateManagementApi/ImportData', {
            method: 'POST',
            body: formData,
        })
            .then(function (data) {
                $$.loading(false);
                if (data) {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_Template_ImportSuccess}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                }else{
                    showToast.error(RM_JS_Template_ImportFailed);
                }
            });
        this.setState({ showImportTemplatePanel: { show: false } });
    }

    renderImportTemplatePanel() {
        const requestVerificationToken = getRequestVerificationToken();
        return <R.Panel
            id="importTemplatesPanel"
            header={RMResx.RM_JS_Template_ImportTemplate}
            size={600}
            status={this.state.showImportTemplatePanel}
            destroy={true}
        >
            <div className='template-import'>
                <R.Validation>
                    <div ref={r => this.allValidation = r}>
                        <div className="template-import-download">
                            <form id="tm-form-download" method="POST" action="">
                                <input type="hidden" id="importDownloadFlag" name="importDownloadFlag" value="" />
                                <input name='RequestVerificationToken' type='hidden' value={requestVerificationToken} readOnly />
                            </form>
                            <span className="template-import-download-span" onClick={this.handleDownloadTemplate} tabIndex="0" onKeyDown={this.onKeyDown}>{RMResx.RM_JS_TM_DownLoadTemplate}</span>
                        </div>
                        <div>
                            <div className="template-import-title" tabIndex="0">
                                <$g.I18NProvider msg={StringUtil.trimEndColon(RMResx.RM_JS_TM_SelectImportFile)} />
                            </div>
                            <div>
                                <R.Validation
                                    element="Uploader"
                                    require={RMResx.RM_SPS_Location_NoImportFile}>
                                    <R.Uploader
                                        ref={this.uploaderRef}
                                        files={this.state.files}
                                        fileTypes={["XLSX"]}
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
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showImportTemplatePanel: {show : false}});
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.handleImportFromTemplateFile} />
            </>
        </R.Panel>;
    }

    renderGlobalUniqueIdSettingPanel() {
        return <R.Panel
            id="uniqueIdPanel"
            header={RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle}
            size={600}
            status={this.state.showUniqueIDSettingsPanel}
            destroy={true}
        >
            
            <div id="uniqueId-panel-container" className="ra-panel-content reclassify-panel">
            <GlobalUniqueIdSettingForm
                id={this.globalUniqueIdPanelId}
            />
                
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showUniqueIDSettingsPanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveUniqueIdSettings} />
            </>
        </R.Panel>;

    }

    renderPhyUnieueIdDialog() {
        return <R.Dialog
            id="PhyUnieueIdDialog"
            header={RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle}
            width={520}
            status={{ show: this.state.showUniqueIdDialog }}
            struct={{ foot: true }}
            onHide={this.onCancelPhysicalUniqueIdSetting.bind(this)}
            destroy={true}
        >
            <div>
                <div className="inline-block margin-bottom-15">
                    <div className="phy-uniqueid-dialog-image faui-info-solid text-blue"></div>
                    <div className="phy-uniqueid-dialog-text" style={{ width: '300px' }}>{RMResx.RM_Template_ToggleUniqueIdDialogMainMessage}</div>
                </div>
                <$g.FormRow label={RMResx.RM_Template_ToggleUniqueIdOptionTitle} key="h1">
                    <$g.RadioGroup
                        name="phy-uniqueid-setting"
                        onChange={this.onPhysicalUniqueIdSettingChanged.bind(this)}
                        value={this.state.uniqueIdMode}>
                        <$g.RadioOption value="1" text={RMResx.RM_Template_ToggleUniqueIdOptionGlobal} />
                        <$g.RadioOption value="0" text={RMResx.RM_Template_ToggleUniqueIdOptionEach} />
                    </$g.RadioGroup>
                </$g.FormRow>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelPhysicalUniqueIdSetting.bind(this)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Confirm} onClick={this.onSavePhysicalUniqueIdSetting.bind(this)} />
            </>
        </R.Dialog>;
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    renderActionButtons() {
        return <div className='navbar'>
            <div className='navbar-right'>
                <div className='navbar-actions-button'>
                {this.state.isGlobalUniqueIdSetting && 
                    <R.Button
                        id="raPrmTplUniqueSettingBtn"  
                        primary={true}
                        classify="theme" 
                        text={RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle} 
                        onClick={this.showUniqueIdSettings} 
                    />
                }
                </div>
            </div>
        </div>;
    }

    render() {
        let changeSetting = this.state.itemSettingChanged;
        return <div id="rmTemplateManagementNew" className="rm-tm-main-container">
            <section className="rm-tm-header">
                <Prompt message={RMResx.RM_TM_WithoutSavingMsg} when={changeSetting} />
                <$g.SiteMap data={[SiteMapLinks.PRM_RecordsManagement]}>
                    {this.renderActionButtons()}
                </$g.SiteMap>
            </section>
            <section id="tmContainer" className="rm-tm-content">
                <div className="rm-tm-splitter-container">
                    <R.Splitter minAsize="25%" minBsize="58%" defaultAsize="40%">
                        <div className="ra-splitter-left">
                            <div>
                                <div className="ra-splitter-head-title">
                                    <span tabIndex="0">{RMResx.RM_PRM_TM_TemplatesTitle}</span>
                                </div>
                            </div>
                            <div className="rmTemplateManagementTree">
                                <$g.TreeView
                                    id="treeview"
                                    classicMode
                                    items={this.state.treeData}
                                    searchKey={this.state.searchKey}
                                    treeContext={this.treeContext}
                                    ref={r => this.refTemplateTree = r}
                                />
                            </div>
                        </div>
                        <div className="ra-splitter-right rm-settings-container">
                            <div className="rm-settings-header">
                                {this.renderSettingHeaderContent()}
                            </div>
                            <div className="rm-settings-content">
                                {this.renderSuiteSettings()}
                                {this.renderTemplateSettings()}
                                {this.renderViewTemplateDetails()}
                                {this.renderViewSuiteDetails()}
                            </div>
                            
                            {this.isShowSaveAndCancelButtons() &&
                                <div className="rm-settings-footer">
                                    <div className="tm-settings-footer-button">
                                        <R.Button
                                            text={RMResx.RM_JS_Common_Cancel}
                                            disabled={!changeSetting}
                                            onClick={this.onCancelChangedClick} />
                                        <R.Button
                                            id="raPrmTemplateSaveBtn"
                                            primary={true}
                                            classify="theme"
                                            text={RMResx.RM_JS_Common_Save}
                                            disabled={!changeSetting}
                                            onClick={this.onSaveSettingClick} />
                                    </div>
                                </div>
                            }
                        </div>
                    </R.Splitter>
                </div>
            </section>
            {this.renderAddExistingTemplatePanel()}
            {this.renderGlobalUniqueIdSettingPanel()}
            {this.renderPhyUnieueIdDialog()}
            {this.renderImportTemplatePanel()}
        </div>;
    }
}

