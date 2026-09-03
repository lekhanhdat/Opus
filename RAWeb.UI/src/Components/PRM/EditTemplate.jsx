import { Component } from 'react';
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import { ColumnTable } from "./ColumnTable";
import * as Constants from "./Constants";
import { CategoryForm } from "../../Components/PRM/TemplateRightPanel/CategoryForm.jsx";
import { ColumnForm } from './TemplateRightPanel/ColumnForm';
import { Prompt } from 'react-router';

import '../../Less/PRM/EditTemplate.less';
import { EmptyGUID } from '../../Constants/Constants';

const PageI18N = {
    BoxSettingsTitle: RMResx.RM_EditTemplate_BoxSettingsTitle,
    FileSettingsTitle: RMResx.RM_EditTemplate_FileSettingsTitle,
    RecordSettingsTitle: RMResx.RM_EditTemplate_RecordSettingsTitle,
    Prefix: RMResx.RM_EditTemplate_Prefix + ':',
    NumberofDigits: RMResx.RM_EditTemplate_NumberofDigits + ':',
    BoxPageTitle: RMResx.RM_EditTemplate_BoxPageTitle,
    FilePageTitle: RMResx.RM_EditTemplate_FilePageTitle,
    RecordPageTitle: RMResx.RM_EditTemplate_RecordPageTitle,
    NewCategoryBtnText: RMResx.RM_EditTemplate_NewCategoryBtnText,
    NewColumnBtnText: RMResx.RM_EditTemplate_NewColumnBtnText,
    NewCategoryText: RMResx.RM_EditTemplate_NewCategoryText,
    NewColumnText: RMResx.RM_EditTemplate_NewColumnText,
    EditCategoryText: RMResx.RM_EditTemplate_EditCategoryText,
    EditColumnText: RMResx.RM_EditTemplate_EditColumnText,
    ColumnSettingsTitle: RMResx.RM_EditTemplate_ColumnSettingsTitle,
    ColumnSettingsDesc: RMResx.RM_EditTemplate_ColumnSettingsDesc,
    ConfirmDeleteCategory: RMResx.RM_EditTemplate_ConfirmDeleteCategory,
    CanNotDeleteCategory: RMResx.RM_EditTemplate_CanNotDeleteCategory,
    LeavePageMessage: RMResx.RM_EditTemplate_LeavePageMessage,
    SaveFailedMessage: RMResx.RM_EditTemplate_SaveFailedMessage,
    ValidateNumberOfDigitsErrorMessage: RMResx.RM_EditTemplate_ValidateNumberOfDigitsErrorMessage,
    ValidatePrefixErrorMessage: RMResx.RM_EditTemplate_ValidatePrefixErrorMessage,
    BtnYes: RMResx.RM_JS_Common_Yes,
    BtnOk: RMResx.RM_JS_Common_OK,
    BtnNo: RMResx.RM_JS_Common_No,
    MessageBoxTitle: RMResx.RM_JS_Common_Confirmation,
    NewBoxPageTitle: RMResx.RM_PRM_TM_NewBoxTemplate_PageTitle,
    NewFilePageTitle: RMResx.RM_PRM_TM_NewFolderTemplate_PageTitle,
    NewRecordPageTitle: RMResx.RM_PRM_TM_NewRecordTemplate_PageTitle,
};

const GuidEmpty = "00000000-0000-0000-0000-000000000000";
export default class EditTemplate extends R.Component {
    idAttr = true;
    componentCreate() {
        // super(props);
        this.bind(['updateCategoryItem', 'updateColumnItem', "onChangeTemplateName", "onChangeTemplateDesc"
        ]);
        this.templateId = RM.Url.getParam(window.location.href, "id");
        this.newTempType = RM.Url.getParam(window.location.href, "type");
        this.suiteUniqueId = RM.Url.getParam(window.location.href, "suiteId") || GuidEmpty;
        this.boxTemplateName = RM.Url.getParam(window.location.href, "bName");
        this.boxTemplateId = RM.Url.getParam(window.location.href, "bId") || GuidEmpty;
        this.folderTemplateId = RM.Url.getParam(window.location.href, "fId") || GuidEmpty;
        this.folderTemplateName = RM.Url.getParam(window.location.href, "fName");
        this.isEditMode = !!this.templateId;

        this.categoryPanelId = 'category-template-panel';
        this.columnPanelId = 'column-template-panel';
        this.parentBoxTemplateUrl = "";
        this.parentFolderTemplateUrl = "";
        this.state = {
            templateInfo: null,
            prefix: '',
            numberOfDigits: 0,
            templateName: '',
            templateDesc: '',
            templateType: null,
            categoryItems: {},
            childrenTemplates: [],
            //childCategoryItems:[],
            //childFolderCategoryItems:[],
            templateCategorys: null,
            selectedItem: null,
            panelTitle: '',
            // panelCreateMode: false,
            showPanel: false,
            pageTitle: '',
            settingTitle: '',
            panelType: null,
            createdOn: '',
            createrInfo: '',
            showTip: false,
            tipType: "success",
            tipMsg: "",
            haveChange: false,
            invalidPrefix: false,
            invalidNumberOfDigits: false,
            invalidMessagePrefix: '',
            invalidMessageNumberOfDigits: '',
            invalidTemplateName: false,
            showCategoryPanel: { show: false },
            showColumnPanel: { show: false },
            showMain: false,
            templateInfoOfBreadCrumbs: null,
            physicalUniqueIdSettingsDialogShow: false,
            uniqueIdMode: "0",
            isGlobalUnieuqIdSetting: false
        };
    }

    componentInit() {
        if (this.isEditMode) {
            this.loadUniqueIdMode(() => {
                this.loadTemplateInfo();
            }, false);

        } else {
            this.loadUniqueIdMode(() => {
                this.setPageTitle(parseInt(this.newTempType, 10));
                this.initDefaultCategoriesAndColumns();
            }, true);
        }
    }
    setPageTitle(templateType) {
        let [pageTitle, settingTitle] = ['', ''];
        switch(templateType)
        {
            case Constants.TemplateTypes.Box:
                pageTitle = this.isEditMode ? PageI18N.BoxPageTitle : PageI18N.NewBoxPageTitle;
                settingTitle = PageI18N.BoxSettingsTitle;
                break;
            case Constants.TemplateTypes.Folder:
                pageTitle = this.isEditMode ? PageI18N.FilePageTitle : PageI18N.NewFilePageTitle;
                settingTitle = PageI18N.FileSettingsTitle;
                break;
            case Constants.TemplateTypes.Records:
                pageTitle = this.isEditMode ? PageI18N.RecordPageTitle : PageI18N.NewRecordPageTitle;
                settingTitle = PageI18N.RecordSettingsTitle;
                break;
        }
        document.title = pageTitle;
        if (this.boxTemplateId != GuidEmpty || this.folderTemplateId != GuidEmpty) {
            this.initTemplateInfoOfBreadCrumbs(pageTitle, settingTitle);
        } else {
            this.setState({
                pageTitle: pageTitle,
                settingTitle: settingTitle,
            });
        }
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
    }
    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }
    hideMessageTip = () => {
        this.setState({ showTip: false });
    }

    initTemplateInfoOfBreadCrumbs(pageTitle, settingTitle) {
        let option = {
            url: "/Api/TemplateManagementApi/GetTemplateInfoOfBreadCrumbs",
            method: "post",
            data: {
                SuiteUniqueId: this.suiteUniqueId,
                BoxTemplateUniqueId: this.boxTemplateId,
                FolderTemplateUniqueId: this.folderTemplateId,
            }
        };
        fetchUtility(option).then((result) => {
            if (result) {
                this.setState({
                    templateInfoOfBreadCrumbs: result,
                    pageTitle: pageTitle,
                    settingTitle: settingTitle,
                });
            }
        }).catch((e) => {

        });
    }

    loadUniqueIdMode(callback, showDialog) {
        let option = {
            url: "/Api/TemplateManagementApi/LoadingUniqueIdSetting",
            method: "get",
        };
        fetchUtility(option).then((result) => {
            let uniqueIdSetting = JSON.parse(result);
            if (uniqueIdSetting) {
                this.setState({ isGlobalUnieuqIdSetting: uniqueIdSetting.IsGlobalSetting });
            } else {
                if (showDialog) {
                    this.setState({ physicalUniqueIdSettingsDialogShow: true });
                }
            }
            if (callback) {
                callback();
            }
        }).catch((e) => {

        });
    }

    loadTemplateInfo() {
        let option = {
            url: "/Api/TemplateManagementApi/LoadTemplateDatas",
            method: "Post",
            data: {
                TemplateIdUniqueId: this.templateId,
                SuiteUniqueId: this.suiteUniqueId,
                BoxTemplateUniqueId: this.boxTemplateId,
                FolderTemplateUniqueId: this.folderTemplateId,
            }
        };
        fetchUtility(option).then((result) => {
            let data = JSON.parse(result);
            if(data)
            {
                let categoryItems = this.initCategoryItems(data.categories);
                let creater = data.creater || 'Admin';
                let createrInfo = `Create By ${creater.DisplayName} On`;
                this.setState({
                    templateInfo: data,
                    prefix: data.prefix,
                    numberOfDigits: data.numberOfDigits,
                    templateName: RMResx[data.name] ? RMResx[data.name] : data.name,
                    templateDesc: data.description,
                    templateType: data.type,
                    templateCategorys: data.categories,
                    childrenTemplates: data.childTemplateCategories,
                    categoryItems: categoryItems,
                    createdOn: data.createdOnStr,
                    createrInfo: createrInfo,
                    showMain: true,
                });
                this.setPageTitle(data.type);
            }
        }).catch((e) => {

        });
    }

    initDefaultCategoriesAndColumns() {
        let option = {
            url: "/Api/TemplateManagementApi/GetDefaultCategoryAndColumn?type=" + this.newTempType,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            let data = JSON.parse(result);
            if(data)
            {
                let categoryItems = this.initCategoryItems(data.categories);
                this.setState({
                    templateType: data.type,
                    categoryItems: categoryItems,
                    showMain: true,
                });
            }
        }).catch((e) => {

        });
    }

    initCategoryItems(templateCategories) {
        let categories = {};
        templateCategories.map(d => {
            let cId = d.id.toLowerCase();
            if (!categories[cId]) {
                categories[cId] = {};
            }
            categories[cId].isShow = true;
            categories[cId].columnItems = this.appendRowIndex(d.columns);
            categories[cId].name = d.name;
            categories[cId].allowEdit = d.allowEdit;
        });
        return categories;
    }

    appendRowIndex(columns) {
        let newColumns = [];
        columns.map(function (item, idx) {
            item.index = idx + 1;
            newColumns.push(item);
        });
        return newColumns.length > 0 ? newColumns : columns;
    }
    saveTemplate = (templateDto) => {
        let option = {
            url: "/Api/TemplateManagementApi/SaveTemplateWithColumns",
            method: "post",
            data: templateDto
        };
        $$.loading(true);
        fetchUtility(option).then((result) => {
            if (result.SaveTemplateResult == Constants.SaveTemplateResult.MissUniqueIdSettingMode) {
                this.setState({ physicalUniqueIdSettingsDialogShow: true });
                $$.loading(false);
            } else if (result.SaveTemplateResult == Constants.SaveTemplateResult.PrefixDuplicate || result.SaveTemplateResult == Constants.SaveTemplateResult.NameDuplicate) {
                let messageContent = result.SaveTemplateResult == Constants.SaveTemplateResult.PrefixDuplicate ?
                    RMResx.RM_Template_DuplicatePrefix : RMResx.RM_Template_TemplateNameDuplicate;
                let buttons = [{ text: PageI18N.BtnOk, onClick: this.hideMessageBox }];
                this.args = {
                    // classify: "warn",
                    width: "550px",
                    hideActions: false,
                    title: PageI18N.MessageBoxTitle,
                    content: messageContent,
                    buttons: buttons
                };
                $$.loading(false);
                $$.messagedialog(true, this.args);
            } else if (result.SaveTemplateResult == Constants.SaveTemplateResult.Success) {
                if (this.templateId) {
                    RM.CommStatus.save(Constants.NewOrEditTemplateCookieNames.EditSuccess);
                } else {
                    RM.CommStatus.save(Constants.NewOrEditTemplateCookieNames.CreateSuccess);
                }
                this.setState({ haveChange: false });
                setTimeout(() => {
                    $$.loading(false);
                    this.redirectParentPage();
                }, 500);
            } else if (result.SaveTemplateResult == Constants.SaveTemplateResult.Failed) {
                this.showMessageTip("error", PageI18N.SaveFailedMessage);
                $$.loading(false);
            }
        }).catch((e) => {
            this.showMessageTip("error", PageI18N.aveFailedMessage);
            $$.loading(false);
        });
    }

    onPrefixChange = (value) => {
        this.setState({
            prefix: $.trim(value),
            haveChange: true,
        });
    }

    onNumberDigitsChange = (value) => {
        this.setState({
            numberOfDigits: $.trim(value),
            haveChange: true,
        });
    }

    validateForm = () => {
        let isValidName = this.validateTemplateName(this.state.templateName);
        if (this.state.isGlobalUnieuqIdSetting) {
            return isValidName;
        } else {
            let prefixValue = this.state.prefix;
            let numberOfDigitsValue = this.state.numberOfDigits;
            let isValidNumberOfDigits = this.validateNumberOfDigits(numberOfDigitsValue);
            let isValidPrefix = this.validatePrefixValue(prefixValue);
            if (!isValidPrefix || !isValidNumberOfDigits || !isValidName) {
                return false;
            }
        }
        return true;
    }

    validateNumberOfDigits(val) {
        let [isValid, errorMessage, minValue, maxValue] = [true, '', 2, 15];
        var regExp = /(^[2-9]$)|(^1[0-5]$)/g;//2-15 number
        if (!regExp.test(val)) {
            isValid = false;
            errorMessage = PageI18N.ValidateNumberOfDigitsErrorMessage.format(minValue, maxValue);
        }
        if (!isValid) {
            document.getElementsByName('inputNumberOfDigits')[0].focus();
            this.setState({
                invalidMessageNumberOfDigits: errorMessage,
                invalidNumberOfDigits: !isValid
            });
        }
        return isValid;
    }

    validatePrefixValue(val) {
        let [isValid, errorMessage] = [true, ''];
        let maxLength = 10;
        if (!this.validateIsNotEmpty(val)) {
            isValid = false;
            errorMessage = RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue;
        } else if (val.length > maxLength) {
            isValid = false;
            errorMessage = PageI18N.ValidatePrefixErrorMessage.format(maxLength);
        }

        if (!isValid) {
            document.getElementsByName('inputPrefix')[0].focus();
            this.setState({
                invalidMessagePrefix: errorMessage,
                invalidPrefix: !isValid
            });
        }
        return isValid;
    }

    validateTemplateName(name) {
        let isValid = true;
        if (!this.validateIsNotEmpty(name)) {
            isValid = false;
            document.getElementsByName('iptTemplateName')[0].focus();
            this.setState({ invalidTemplateName: true });
        }
        return isValid;
    }

    validateIsNotEmpty(val) {
        return $.trim(val) != '';
    }

    onSaveClick = (e) => {
        if (this.validateForm()) {
            let templateInfo = this.isEditMode ? RM.deepcopy(this.state.templateInfo) : {};
            if (!this.state.isGlobalUnieuqIdSetting) {
                templateInfo.numberOfDigits = parseInt(this.state.numberOfDigits, 10);
                templateInfo.prefix = this.state.prefix;
            }
            templateInfo.name = this.state.templateName;
            templateInfo.description = this.state.templateDesc;
            if (!this.isEditMode) {
                templateInfo.suiteUniqueId = this.suiteUniqueId;
                templateInfo.uniqueId = GuidEmpty;
                templateInfo.type = this.newTempType;
                templateInfo.boxTemplateId = this.boxTemplateId;
                templateInfo.folderTemplateId = this.folderTemplateId;
            } else {
                templateInfo.uniqueId = this.templateId;
            }

            let categoryItems = RM.deepcopy(this.state.categoryItems);
            templateInfo.categories = [];
            for (let id in categoryItems) {
                if (categoryItems.hasOwnProperty(id)) {
                    let categoryItem = categoryItems[id];
                    categoryItem.columnItems = categoryItem.columnItems.filter(t => !t.inheritFromParent && !t.inheritFromParentFolder);
                    let columnItems = categoryItem.columnItems;
                    for (let c in columnItems) {
                        if (columnItems.hasOwnProperty(c)) {
                            delete columnItems[c].index;
                        }
                    }
                    templateInfo.categories.push({
                        id: id,
                        name: categoryItem.name,
                        allowEdit: categoryItem.allowEdit,
                        columns: columnItems,
                    });
                }
            }

            this.saveTemplate(templateInfo);
        }
    }

    onCancelClick = (e) => {
        this.redirectParentPage();
    }

    redirectParentPage() {
        if (this.boxTemplateId != GuidEmpty || this.folderTemplateId != GuidEmpty) {
            this.routerTo(this.parentFolderTemplateUrl ? this.parentFolderTemplateUrl : this.parentBoxTemplateUrl);
        } else {
            this.routerTo(RouterUrls.PRM_TemplateManagement);
        }
    }

    onNewColumnClick = (id) => {
        window.e.stopPropagation();
        this.createAndEditColumn(id, -1);
    }

    // updateItem = (item) => {
    //     //console.log('ta lai l');
    //     //console.log(item);
    //     switch(this.state.panelType){
    //         case Constants.panelType.Box:
    //         case Constants.panelType.Folder:
    //         case Constants.panelType.Record:
    //             this.updateColumnItem(item);
    //             break;
    //         case Constants.panelType.Category:
    //             this.updateCategoryItem(item);
    //             break;
    //     }
    // }

    updateColumnItem() {
        //validate same name in Parent Component
        let callback = (item) => {
            if (!this.checkSameColumn(item)) {
                let option = {
                    url: `/api/TemplateManagementApi/ValidateDuplidateColName`,
                    method: "post",
                    data: {
                        typeId: item.typeId,
                        columnName: item.columnName
                    }
                };
                fetchUtility(option).then((result) => {
                    if (result == true) {
                        let categoryItems = RM.deepcopy(this.state.categoryItems);
                        let categoryId = item.categoryId;
                        let categoryItem = categoryItems[categoryId];
                        if (!categoryItem) {
                            categoryItems[categoryId] = {};
                            categoryItem = categoryItems[categoryId];
                            categoryItem.columnItems = [];
                        }
                        if (!item.index) {
                            //new
                            item.uniqueId = this.newGuid();
                            let itemIndex = categoryItem.columnItems.length + 1;
                            item.index = itemIndex;
                            categoryItem.columnItems.push(item);
                        } else {
                            //edit
                            categoryItem.columnItems.map(t => {
                                if (t.index == item.index) {
                                    t.typeId = item.typeId;
                                    t.required = item.required;
                                    t.columnName = item.columnName;
                                    t.pushToChild = item.pushToChild === undefined ? false : item.pushToChild;
                                    t.childInheritsValue = item.childInheritsValue === undefined ? false : item.childInheritsValue;
                                    t.allowModifyValue = item.allowModifyValue === undefined ? false : item.allowModifyValue;
                                    t.categoryId = categoryId;
                                    t.uniqueId = item.uniqueId;
                                    t.optionsJSON = item.optionsJSON;
                                    t.optionsMaxIdReachedValue = item.optionsMaxIdReachedValue;
                                    //t.pushCategoryId = item.pushCategoryId === undefined ? EmptyGUID : item.pushCategoryId;
                                    //t.pushFolderCategoryId = item.pushFolderCategoryId === undefined ? EmptyGUID : item.pushFolderCategoryId;
                                    t.pushFoldTemplateCategoriesId = (item.pushFoldTemplateCategoriesId == null || item.pushFoldTemplateCategoriesId.length == 0) ? [] : item.pushFoldTemplateCategoriesId;
                                    t.pushRecordTemplateCategoriesId = (item.pushRecordTemplateCategoriesId == null || item.pushRecordTemplateCategoriesId.length == 0) ? [] : item.pushRecordTemplateCategoriesId;
                                }
                            });
                        }
                        categoryItem.isShow = true;
                        this.setState({
                            categoryItems: categoryItems,
                            showPanel: false,
                            haveChange: true,
                            showColumnPanel: { show: false }
                        });
                    } else {
                        //this.showMessageTip("error", RMResx.RM_EditTemplate_SameColumnNameErrorMessage);
                        this.dispatch(this.columnPanelId, 'duplicateError', callback);
                    }
                }).catch((e) => {
                    this.dispatch(this.columnPanelId, 'duplicateError', callback);
                });

                return true;
            } else {
                return false;
            }
        };
        this.dispatch(this.columnPanelId, 'onSave', callback);
        return false;
    }

    updateCategoryItem() {
        // let isEdit = !this.state.panelCreateMode;
        //validate same name in Parent Component
        let callback = (item) => {
            if (!this.checkSameCategory(item)) {
                let categoryItems = RM.deepcopy(this.state.categoryItems);
                if (!categoryItems[item.id]) {
                    categoryItems[item.id] = {};
                    categoryItems[item.id].columnItems = [];
                }
                categoryItems[item.id].id = item.id;
                categoryItems[item.id].name = item.categoryName;
                if (item.allowEdit == undefined) {
                    categoryItems[item.id].allowEdit = true;
                }
                // categoryItems[item.id].isShow = true;
                this.setState({
                    categoryItems: categoryItems,
                    showPanel: false,
                    haveChange: true,
                });
                this.setState({ showCategoryPanel: { show: false } });
                return true;
            } else {
                return false;
                // this.dispatch(this.categorypanelId, 'showMsgTip', { type: "error", msg: RMResx.RM_EditTemplate_SameCategoryNameErrorMessage });
            }
        };
        this.dispatch(this.categoryPanelId, 'onSave', callback);
        return false;
    }

    realDelCategory = (id) => {
        let cItems = RM.deepcopy(this.state.categoryItems);
        delete cItems[id];
        this.setState({
            categoryItems: cItems,
            haveChange: true,
        });
        this.hideMessageBox();
    }

    checkSameCategory = (item) => {
        let categoryItems = RM.deepcopy(this.state.categoryItems);
        let result = false;
        for (let id in categoryItems) {
            if (categoryItems.hasOwnProperty(id)) {
                if ((categoryItems[id].name == item.categoryName || RMResx[categoryItems[id].name] == item.categoryName) && id != item.id) {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    checkSameColumn = (item) => {
        let categoriesItems = RM.deepcopy(this.state.categoryItems);
        let columnItems = [];
        for (let cId in categoriesItems) {
            if (categoriesItems.hasOwnProperty(cId)) {
                for (let i = 0; i < categoriesItems[cId].columnItems.length; i++) {
                    columnItems.push(categoriesItems[cId].columnItems[i]);
                }
            }
        }
        let result = columnItems.find(t => (this.isSameColumn(t.columnName, item.columnName) || this.isSameColumn(RMResx[t.columnName], item.columnName)) && t.uniqueId != item.uniqueId);
        return result !== undefined;
    }

    isSameColumn(name1, name2) {
        if (name1 && name2) {
            return name1.toLowerCase() == name2.toLowerCase();
        }
        return false;
    }

    onNewCategoryClick = () => {
        this.createAndEditCategory();
    }

    onCategoryEdit(id) {
        this.createAndEditCategory(id);
    }

    onCategoryDelete(id) {
        this.showMessageBoxForDelCategory(id);
    }

    updateColumnTableItems = (categoryId, items) => {
        let cItems = RM.deepcopy(this.state.categoryItems);
        var categoryItem = cItems[categoryId];
        categoryItem.columnItems = items;
        // categoryItem.isShow = true;
        this.setState({
            categoryItems: cItems,
            haveChange: true,
        });
    }

    hasColumnsUnderCategory = (id) => {
        let cItems = RM.deepcopy(this.state.categoryItems);
        let columns = cItems[id].columnItems;
        if (columns && columns.length > 0) {
            return true;
        } else {
            return false;
        }
    }

    showMessageBoxForDelCategory = (id) => {
        let hasColumns = this.hasColumnsUnderCategory(id);
        let messageContent = !hasColumns ? PageI18N.ConfirmDeleteCategory : PageI18N.CanNotDeleteCategory;
        let buttons;
        if (!hasColumns) {
            buttons = [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.hideMessageBox },
                { text: PageI18N.BtnOk, primary: true, classify: "theme", onClick: this.realDelCategory.bind(this, id) },
            ];
        } else {
            buttons = [{ text: PageI18N.BtnOk, primary: true, classify: "theme", onClick: this.hideMessageBox }];
        }
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: PageI18N.MessageBoxTitle,
            content: messageContent,
            buttons: buttons
        };
        $$.messagedialog(true, this.args);
    }

    hideMessageBox = () => {
        $$.messagedialog(false);
    }

    createAndEditCategory = (id) => {
        let isEdit = !!id;
        let categoryId = isEdit ? id : this.newGuid();
        let item = {};
        let title = isEdit ? PageI18N.EditCategoryText : PageI18N.NewCategoryText;
        // let createMode = !isEdit;
        if (isEdit) {
            let cItems = RM.deepcopy(this.state.categoryItems);
            let cItem = cItems[categoryId];
            item.id = categoryId;
            item.categoryName = RMResx[cItem.name] ? RMResx[cItem.name] : cItem.name;
        } else {
            item.id = categoryId;
            item.categoryName = '';
            item.allowEdit = true;
        }
        this.setState({
            showCategoryPanel: { show: true },
            panelTitle: title,
            selectedItem: item,
            showPanel: true,
            panelType: Constants.panelType.Category,
        });
    }

    createAndEditColumn = (categoryId, itemIndex) => {
        let isEdit = itemIndex !== -1;
        let item = {};
        let title = isEdit ? PageI18N.EditColumnText : PageI18N.NewColumnText;
        // let createMode = !isEdit;
        if (isEdit) {
            let cItems = RM.deepcopy(this.state.categoryItems);
            let columnItems = cItems[categoryId].columnItems;
            item = columnItems.find(t => t.index == itemIndex);
            item.columnName = RMResx[item.columnName] ? RMResx[item.columnName] : item.columnName;
            item.childTemplate = [];
            item.childTemplate = this.state.childrenTemplates;
            if (item.childTemplate != null) {
                for (var i = 0; i < item.childTemplate.length; i++) {
                    let name = item.childTemplate[i].templateName;
                    item.childTemplate[i].templateName = RMResx[name] ? RMResx[name] : name;
                    if (item.childTemplate[i].currentCategories != null) {
                        if (item.childTemplate[i].currentCategories != null) {
                            for (var j = 0; j < item.childTemplate[i].currentCategories.length; j++) {
                                let name = item.childTemplate[i].currentCategories[j].name;
                                item.childTemplate[i].currentCategories[j].name = RMResx[name] ? RMResx[name] : name;
                                item.childTemplate[i].currentCategories[j].templateId = item.childTemplate[i].uniqueId;
                                item.childTemplate[i].currentCategories[j].checked = false;
                            }
                        }
                        if (item.childTemplate[i].childrenCategories != null) {
                            for (var j = 0; j < item.childTemplate[i].childrenCategories.length; j++) {
                                let name = item.childTemplate[i].childrenCategories[j].templateName;
                                item.childTemplate[i].childrenCategories[j].templateName = RMResx[name] ? RMResx[name] : name;
                                if (item.childTemplate[i].childrenCategories[j].currentCategories != null) {
                                    for (var k = 0; k < item.childTemplate[i].childrenCategories[j].currentCategories.length; k++) {
                                        let name = item.childTemplate[i].childrenCategories[j].currentCategories[k].name;
                                        item.childTemplate[i].childrenCategories[j].currentCategories[k].name = RMResx[name] ? RMResx[name] : name;
                                        item.childTemplate[i].childrenCategories[j].currentCategories[k].templateId = item.childTemplate[i].childrenCategories[j].uniqueId;
                                        item.childTemplate[i].childrenCategories[j].currentCategories[k].checked = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } else {
            item.categoryId = categoryId;
            item.allowEdit = true;
            item.childTemplate = [];
            item.childTemplate = this.state.childrenTemplates;
            if (item.childTemplate != null)
            {
                for (var i = 0; i < item.childTemplate.length; i++) {
                    let name = item.childTemplate[i].templateName;
                    item.childTemplate[i].templateName = RMResx[name] ? RMResx[name] : name;
                    if (item.childTemplate[i].currentCategories != null)
                    {
                    if (item.childTemplate[i].currentCategories != null) {
                            for (var j = 0; j < item.childTemplate[i].currentCategories.length; j++) {
                                let name = item.childTemplate[i].currentCategories[j].name;
                                item.childTemplate[i].currentCategories[j].name = RMResx[name] ? RMResx[name] : name;
                                item.childTemplate[i].currentCategories[j].templateId = item.childTemplate[i].uniqueId;
                                item.childTemplate[i].currentCategories[j].checked = false;
                            }
                        }
                        if (item.childTemplate[i].childrenCategories != null) {
                            for (var j = 0; j < item.childTemplate[i].childrenCategories.length; j++) {
                                let name = item.childTemplate[i].childrenCategories[j].templateName;
                                item.childTemplate[i].childrenCategories[j].templateName = RMResx[name] ? RMResx[name] : name;
                                if (item.childTemplate[i].childrenCategories[j].currentCategories != null) {
                                    for (var k = 0; k < item.childTemplate[i].childrenCategories[j].currentCategories.length; k++) {
                                        let name = item.childTemplate[i].childrenCategories[j].currentCategories[k].name;
                                        item.childTemplate[i].childrenCategories[j].currentCategories[k].name = RMResx[name] ? RMResx[name] : name;
                                        item.childTemplate[i].childrenCategories[j].currentCategories[k].templateId = item.childTemplate[i].childrenCategories[j].uniqueId;
                                        item.childTemplate[i].childrenCategories[j].currentCategories[k].checked = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        this.setState({
            showColumnPanel: { show: true },
            panelTitle: title,
            selectedItem: item,
            showPanel: true,
            panelType: this.getPanelType(),
        });
    }

    getPanelType = () => {
        let panelType = -1;
        switch(this.state.templateType)
        {
            case Constants.TemplateTypes.Box:
                panelType = Constants.panelType.Box;
                break;
            case Constants.TemplateTypes.Folder:
                panelType = Constants.panelType.Folder;
                break;
            case Constants.TemplateTypes.Records:
                panelType = Constants.panelType.Record;
                break;
        }
        return panelType;
    }

    toggleExpander(id, isShow) {
        let cItems = RM.deepcopy(this.state.categoryItems);
        cItems[id].isShow = isShow;
        this.setState({ categoryItems: cItems });
    }

    newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    rederUnieueId() {
        return <div className="col-xlg-4 col-xs-4">
            <div id='settingsContainer' className="ra-section">
                <div className='settings-desc' tabIndex='0'>{this.state.settingTitle}</div>
                <div className="ra-form-label ra-require" ><div className='input-label' tabIndex='0'>{PageI18N.Prefix}</div></div>
                <R.Input
                    name='inputPrefix'
                    type='text'
                    width={280}
                    value={this.state.prefix || ''}
                    onChange={this.onPrefixChange} aria={{ariaLabel:PageI18N.Prefix}} />
                {this.state.invalidPrefix && <div className='ra-validation-msg'>
                    {this.state.invalidMessagePrefix}
                </div>}
                <div className="ra-form-label ra-require"><div className='input-label' tabIndex='0'>{PageI18N.NumberofDigits}</div></div>
                <R.Input
                    name='inputNumberOfDigits'
                    type="text"
                    width={280}
                    value={this.state.numberOfDigits || ''}
                    onChange={this.onNumberDigitsChange} aria={{ariaLabel:PageI18N.NumberofDigits}}/>
                {this.state.invalidNumberOfDigits && <div className='ra-validation-msg'>
                    {this.state.invalidMessageNumberOfDigits}
                </div>}
            </div>
        </div>;
    }

    renderMainContent() {
        return <React.Fragment>
            <div className="row-flex row-xlg-flex">
                {this.renderTemplateBaseInfo()}
                {!this.state.isGlobalUnieuqIdSetting && this.rederUnieueId()}
            </div>
            <div id='columnsContainer' className="ra-section"   >
                <div className='columns-title' tabIndex='0'>{PageI18N.ColumnSettingsTitle}</div>
                <div className='columns-desc' tabIndex='0'>{PageI18N.ColumnSettingsDesc}</div>
                <div id='btnNewCategory'>
                    <R.Button primary={true} classify="theme" text={PageI18N.NewCategoryBtnText} onClick={this.onNewCategoryClick} />
                </div>
                <div>
                    {this.renderColumnTables()}
                </div>
            </div>
            <div id='btnSaveAndCancel'>
                <R.Button text={RMResx.RM_JS_Common_Cancel} onClick={this.onCancelClick} />
                <R.Button primary={true} classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveClick} />
            </div>
        </React.Fragment>;
    }

    renderTemplateBaseInfo() {
        let sectionTitle = "";
        let templateLabel = "";
        let uniqueIdContainerClassName = this.state.isGlobalUnieuqIdSetting ? "col-xlg-12 col-xs-12" : "col-xlg-8 col-xs-8";
        let templateType = this.isEditMode ? this.state.templateInfo.type : parseInt(this.newTempType, 10);
        switch (templateType) {
            case Constants.TemplateTypes.Box:
                sectionTitle = RMResx.RM_PRM_TM_BoxTemplateSectionTitle;
                templateLabel = RMResx.RM_PRM_TM_BoxTemplateName;
                break;
            case Constants.TemplateTypes.Folder:
                sectionTitle = RMResx.RM_PRM_TM_FolderTemplateSectionTitle;
                templateLabel = RMResx.RM_PRM_TM_FolderTemplateName;
                break;
            case Constants.TemplateTypes.Records:
                sectionTitle = RMResx.RM_PRM_TM_RecordTemplateSectionTitle;
                templateLabel = RMResx.RM_PRM_TM_RecordTemplateName;
                break;
        }

        return <div className={uniqueIdContainerClassName}>
            <div className="ra-section">
                <div className="section-title">{sectionTitle}</div>
                <div className="ra-form-label ra-require" ><div className='input-label' tabIndex='0'>{templateLabel}</div></div>
                <R.Input
                    name='iptTemplateName'
                    type='text'
                    width={500}
                    value={this.state.templateName}
                    onChange={this.onChangeTemplateName.bind(this)}
                    aria={{ariaLabel:templateLabel}}
                />
                {this.state.invalidTemplateName && <div className='ra-validation-msg'>
                    {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                </div>}
                <div className="ra-form-label" ><div className='input-label' tabIndex='0'>{RMResx.RM_PRM_TM_TemplateDesc}</div></div>
                <div className="ra-form-content">
                    <R.Input
                        name='iptTemplateDesc'
                        type='textarea'
                        width={500}
                        height={100}
                        value={this.state.templateDesc}
                        onChange={this.onChangeTemplateDesc.bind(this)}
                        aria={{ariaLabel:RMResx.RM_PRM_TM_TemplateDesc}}
                    />
                </div>
            </div>
        </div>
        ;
    }

    onChangeTemplateName(value){
        this.setState({
            templateName: $.trim(value)
        });
    }

    onChangeTemplateDesc(value) {
        this.setState({
            templateDesc: $.trim(value)
        });
    }

    onCancelPhysicalUniqueIdSetting() {
        this.setState({ physicalUniqueIdSettingsDialogShow: false });
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
                    physicalUniqueIdSettingsDialogShow: false,
                    isGlobalUnieuqIdSetting: this.state.uniqueIdMode === "1"
                });
            }
        }).catch((e) => {

        });
    }

    onPhysicalUniqueIdSettingChanged(val) {
        this.setState({ uniqueIdMode: val });
    }

    renderColumnTables() {
        let cItems = RM.deepcopy(this.state.categoryItems);
        let categoriesBlock = [];
        for(let id in cItems)
        {
            if (cItems.hasOwnProperty(id)) {
                let item = cItems[id];
                let tableId = 'table_' + id;
                let hasColumnItems = item.columnItems && item.columnItems.length > 0;
                let columnName = RMResx[item.name] ? RMResx[item.name] : item.name;
                categoriesBlock.push(<div className='ra-table-wrapper' key={id}><R.Expander status={{ show: this.state.categoryItems[id].isShow }} key={id} shown={this.toggleExpander.bind(this, id, true)} hidden={this.toggleExpander.bind(this, id, false)}>
                    <div className='category-content'>
                        <span className='category-label' data-tooltip aria-label={columnName}>{columnName}</span>
                        <span className='category-btn-group'>
                            {
                                item.allowEdit && <span className="aui-expander-title-widget category-btn">
                                    <R.Button type="bald" tooltip={RMResx.RM_JS_Common_Edit} icon="fia-edit btn-edit" onClick={this.onCategoryEdit.bind(this, id)} />
                                </span>
                                // item.allowEdit && <div className="ra-iconbtn-icon-edit aui-expander-title-widget" tabIndex='0' onClick={this.onCategoryEdit.bind(this, id)}></div>
                            }
                            {
                                item.allowEdit && <span className="aui-expander-title-widget category-btn">
                                    <R.Button type="bald" tooltip={RMResx.RM_JS_Common_Delete} icon="fia-delete btn-delete" onClick={this.onCategoryDelete.bind(this, id)} />
                                </span>
                                // item.allowEdit && <div className="ra-iconbtn-icon-del aui-expander-title-widget" tabIndex='0' onClick={this.onCategoryDelete.bind(this, id)}></div>
                            }
                        </span>
                        <span className='aui-expander-title-widget btn-newColumn'>
                            <R.Button primary={true} classify="theme" text={PageI18N.NewColumnBtnText} onClick={this.onNewColumnClick.bind(this, id)} />
                        </span>
                    </div>
                    <div>
                        {
                            hasColumnItems && <ColumnTable
                                columnTableId={tableId}
                                TemplateType={this.state.templateType}
                                RowItems={item.columnItems}
                                categoryId={id}
                                UpdateRowDataSource={this.updateColumnTableItems}
                                showEditColumnWindow={this.createAndEditColumn}
                            />
                        }
                    </div>
                </R.Expander></div>);
            }
        }
        return <div id='raColumnTables'>{categoriesBlock.length > 0 && categoriesBlock}</div>;
    }

    renderCategoryPanel() {
        return <R.Panel
            id="template-category-edit"
            header={this.state.panelTitle}
            size={600}
            status={this.state.showCategoryPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <CategoryForm
                    id={this.categoryPanelId}
                    panelType={this.state.panelType}
                    item={this.state.selectedItem}
                >
                </CategoryForm>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showCategoryPanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.updateCategoryItem} />
            </>
        </R.Panel>;
    }

    renderColumnPanel() {
        return <R.Panel
            id="template-column-edit"
            header={this.state.panelTitle}
            size={600}
            status={this.state.showColumnPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <ColumnForm
                    id={this.columnPanelId}
                    panelType={this.state.panelType}
                    item={this.state.selectedItem}
                >
                </ColumnForm>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                    this.setState({ showColumnPanel: { show: false } });
                }} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.updateColumnItem} />
            </>
        </R.Panel>;
    }

    renderPhyUnieueIdDialog() {
        return <R.Dialog
            id="PhyUnieueIdDialog"
            header={RMResx.RM_EditTemplate_PhysicalUniqueIdSettingsTitle}
            width={520}
            height={380}
            status={{ show: this.state.physicalUniqueIdSettingsDialogShow }}
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

    initSiteMapData() {
        let items = [SiteMapLinks.PRM_TemplateManagement];
        let info = this.state.templateInfoOfBreadCrumbs;
        if (info) {
            if (info.BoxTemplateName) {
                let boxMapLink = SiteMapLinks.PRM_BoxTemplate;
                boxMapLink.text = info.BoxTemplateName;
                boxMapLink.href = RouterUrls.PRM_FolderTemplateManagement + `/?cType=${Constants.TemplateTypes.Folder}&suiteId=${this.suiteUniqueId}&bId=${info.BoxTemplateId}`;
                items.push(boxMapLink);
                this.parentBoxTemplateUrl = boxMapLink.href;
            }
            if (info.FolderTemplateName) {
                let folderMapLink = SiteMapLinks.PRM_FolderTemplate;
                folderMapLink.text = info.FolderTemplateName;
                folderMapLink.href = RouterUrls.PRM_RecordTemplateManagement + `/?cType=${Constants.TemplateTypes.Records}&suiteId=${this.suiteUniqueId}&bId=${info.BoxTemplateId}&fId=${info.FolderTemplateId}`;
                items.push(folderMapLink);
                this.parentFolderTemplateUrl = folderMapLink.href;
            }
        }
        let editTemplateLink = SiteMapLinks.PRM_EditTemplate;
        editTemplateLink.text = this.state.pageTitle;
        items.push(editTemplateLink);
        return items;
    }

    render() {
        let haveChange = this.state.haveChange;
        return <div id='raEditTemplate'>
            <Prompt message={PageI18N.LeavePageMessage} when={haveChange} />
            <$g.SiteMap data={this.initSiteMapData()} />
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                status={{ show: this.state.showTip }}
            />
            {this.state.showMain && <div id='mainContainer' className="ra-page-main">
                {this.renderMainContent()}
                {this.renderCategoryPanel()}
                {this.renderColumnPanel()}
                {this.renderPhyUnieueIdDialog()}
            </div>}
        </div>;
    }
}