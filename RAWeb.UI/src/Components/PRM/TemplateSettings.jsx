import { Component } from 'react';
import { bindEvents, showToast } from "../../Utilities/CommonUtil";
import { ColumnTable } from "./ColumnTable";
import * as Constants from "./Constants";
import { CategoryForm } from "../../Components/PRM/TemplateRightPanel/CategoryForm.jsx";
import { ColumnForm } from './TemplateRightPanel/ColumnForm';
import StringUtil from '../../Utilities/StringUtil';
import { EmptyGUID } from '../../Constants/Constants';
import { RegexUtil } from "../../Utilities/RegexUtil";
import '../../Less/PRM/EditTemplate.less';


export default class TemplateSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.bind(["updateCategoryItem", "updateColumnItem", "onChangeTemplateName", "onChangeTemplateDesc", "onChangeCategoryName",
        "validateNumberOfDigits", "validatePrefixValue", "onNewCategoryClick"
        ]);
        this.state = {
            templateInfo: null,
            prefix: '',
            numberOfDigits: null,
            templateName: '',
            templateDesc: '',
            templateType: null,
            newCategoryName: '',
            categoryItems: {},
            childrenTemplates: [],
            templateCategorys: null,
            selectedItem: null,
            panelTitle: '',
            showPanel: false,
            settingTitle: '',
            panelType: null,
            haveChange: false,
            showCategoryPanel: { show: false },
            showColumnPanel: { show: false },
            showMain: false,
            templateInfoOfBreadCrumbs: null,
            categoryAddedStatus: false,
            showCategoryNameError: false, 
            categoryNameErrorMsg: ""
        };
        this.templateId = "";
        this.newTempType = "";
        this.isEditMode = false;
        this.categoryPanelId = 'category-template-panel';
        this.columnPanelId = 'column-template-panel';
    }

    componentInit() {
        
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onSave":
                if($$.verify(this.allValidation) && this.validateCategoryName()) {
                    let saveTemplateItem = args[0];
                    saveTemplateItem(this.getTemplateDto());
                }
                break;
            case "init":
                this.loadTemplateInfo(args[0], args[1]);
                break;
            case "new":
                this.clearData();
                this.initNewTemplateData(args[0]);
                break;
        }
    }

    clearData() {
        this.templateId = "";
        this.newTempType = "";
        this.isEditMode = false;
        this.setState({
            templateInfo: null,
            prefix: '',
            numberOfDigits: null,
            templateName: '',
            templateDesc: '',
            templateType: null,
            newCategoryName: '',
            categoryItems: {},
            childrenTemplates: [],
            templateCategorys: null,
            selectedItem: null,
            haveChange: false,
            showMain: false,
            categoryAddedStatus: false,
            showCategoryNameError: false, 
            categoryNameErrorMsg: ""
        });
    }

    initNewTemplateData(relationInfo) {
        this.newTempType = relationInfo.TemplateType;
        this.parentIdList = relationInfo.TemplateIdList;
        this.defaultCategoryAndColumnsInfo = relationInfo.DefaultCategoryAndColumnsInfo;
        this.initDefaultCategoriesAndColumns();
    }

    getTemplateDto() {
        let templateInfo = this.isEditMode ? RM.deepcopy(this.state.templateInfo) : {};
            if (!this.props.isGlobalUniqueIdSetting) {
                templateInfo.numberOfDigits = parseInt(this.state.numberOfDigits, 10);
                templateInfo.prefix = this.state.prefix;
            }
            templateInfo.name = this.state.templateName;
            templateInfo.description = this.state.templateDesc;
            if (!this.isEditMode) {
                templateInfo.uniqueId = EmptyGUID;
                templateInfo.type = this.newTempType;
            } else {
                templateInfo.uniqueId = this.templateId;
            }
            templateInfo.ParentTemplateIdList = this.parentIdList;
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
            return templateInfo;
    }

    loadTemplateInfo(uniqueId, templateIdList) {
        this.isEditMode = true;
        this.templateId = uniqueId;
        let option = {
            url: "/Api/TemplateManagementApi/LoadTemplateDatas",
            method: "Post",
            data: {
                TemplateIdUniqueId: uniqueId,
                TemplateIdList: templateIdList
            }
        };
        fetchUtility(option).then((result) => {
            let data = JSON.parse(result);
            if(data)
            {
                let categoryItems = this.initCategoryItems(data.categories);
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
                    showMain: true,
                });
            }
        }).catch((e) => {

        });
    }

    initDefaultCategoriesAndColumns() {
        let data = this.defaultCategoryAndColumnsInfo;
        if(data)
        {
            let categoryItems = this.initCategoryItems(data.categories);
            this.setState({
                templateType: data.type,
                categoryItems: categoryItems,
                showMain: true,
            });
        }
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

    onPrefixChange = (value) => {
        this.setState({
            prefix: $.trim(value),
            haveChange: true,
        });
        this.props.notifySettingsChanged();
    }

    onNumberDigitsChange = (value) => {
        this.setState({
            numberOfDigits: $.trim(value),
            haveChange: true,
        });
        this.props.notifySettingsChanged();
    }

    validateNumberOfDigits(val) {
        var regExp = /(^[2-9]$)|(^1[0-5]$)/g;//2-15 number
        if (!regExp.test(val)) {
            return  RMResx.RM_EditTemplate_ValidateNumberOfDigitsErrorMessage.format(2, 15);
        }
        return true;
    }

    validatePrefixValue(val) {
        let maxLength = 10;
        if (val.length > maxLength) {
            return RMResx.RM_EditTemplate_ValidatePrefixErrorMessage.format(maxLength);
        }
        if(!RegexUtil.IsMath(val))
        {
            return RMResx.RM_PRM_UniqueId_Invalid_Message;
        }
        return true;
    }


    onNewColumnClick = (id) => {
        window.e?.stopPropagation();
        this.createAndEditColumn(id, -1);
    }

    onNewColumnKeyDown = (id, e) => {
        if(e.keyCode == 13) {
            this.createAndEditColumn(id, -1);
        }
    }

    updateColumnItem() {
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
                            item.uniqueId = StringUtil.newGuid();
                            let itemIndex = categoryItem.columnItems.length + 1;
                            item.index = itemIndex;
                            categoryItem.columnItems.push(item);
                        } else {
                            //edit
                            categoryItem.columnItems.map(t => {
                                if (t.index == item.index) {
                                    t.typeId = item.typeId;
                                    t.required = item.required;
                                    t.allowSort = item.allowSort;
                                    t.allowEditSort = item.allowEditSort;
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
        this.props.notifySettingsChanged();
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

    onNewCategoryClick() {
        if(!$.trim(this.state.newCategoryName))
        {
            this.setState({categoryAddedStatus: false, showCategoryNameError: true, categoryNameErrorMsg: RMResx.RM_Template_Column_ValueValidate});
        }else {
            this.createAndEditCategory();
        } 
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
        this.props.notifySettingsChanged();
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
        let messageContent = !hasColumns ?  RMResx.RM_EditTemplate_ConfirmDeleteCategory : RMResx.RM_EditTemplate_CanNotDeleteCategory;
        let buttons;
        if (!hasColumns) {
            buttons = [{ text: RMResx.RM_JS_Common_Cancel, onClick: this.hideMessageBox },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.realDelCategory.bind(this, id) },
            ];
        } else {
            buttons = [{ text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.hideMessageBox }];
        }
        this.args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
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
        let categoryId = isEdit ? id : StringUtil.newGuid();
        let item = {};
        let title = isEdit ? RMResx.RM_EditTemplate_EditCategoryText : RMResx.RM_EditTemplate_NewCategoryText;
        // let createMode = !isEdit;
        let cItems = RM.deepcopy(this.state.categoryItems);
        
        if (isEdit) {
            let cItem = cItems[categoryId];
            item.id = categoryId;
            item.categoryName = RMResx[cItem.name] ? RMResx[cItem.name] : cItem.name;
            this.setState({
                showCategoryPanel: { show: true },
                panelTitle: title,
                selectedItem: item,
                showPanel: true,
                panelType: Constants.panelType.Category,
            });
        } else {
            item.id = categoryId;
            item.categoryName = this.state.newCategoryName;
            if(!this.checkSameCategory(item))
            {
                cItems[categoryId]= {
                    id: item.id,
                    name: item.categoryName,
                    allowEdit: true,
                    columnItems: []
                };
                this.setState({categoryAddedStatus: false, categoryItems: cItems, haveChange: true, newCategoryName: "", showCategoryNameError: false}, ()=> {
                    setTimeout(()=> {$(".column_add_text:last")[0].scrollIntoView();}, 50);
                });
            }else {
                this.setState({categoryAddedStatus: true, showCategoryNameError: true, categoryNameErrorMsg: RMResx.RM_EditTemplate_SameCategoryNameErrorMessage});
            }
        }
    }

    createAndEditColumn = (categoryId, itemIndex) => {
        let isEdit = itemIndex !== -1;
        let item = {};
        let title = isEdit ? RMResx.RM_EditTemplate_EditColumnText : RMResx.RM_EditTemplate_NewColumnText;
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

    renderUniqueIdSettings() {
        return <div id='settingsContainer'>
                    <div className='settings-desc' tabIndex='0'>{this.state.settingTitle}</div>
                    <div className="ra-form-label"><div className='input-label require' tabIndex='0'>{RMResx.RM_EditTemplate_Prefix}</div></div>
                    <div className="margin-bottom-24">
                    <R.Validation element="Input" require={RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue} rules={{verifyPrefix: this.validatePrefixValue}}>
                        <R.Input
                            id="raPrmTplColumnUniqueIdPrefix"
                            name='inputPrefix'
                            type='text'
                            value={this.state.prefix}
                            onChange={this.onPrefixChange.bind(this)}
                            aria={{ariaLabel:RMResx.RM_EditTemplate_Prefix}}
                        />
                    </R.Validation>
                    </div>
                    <div className="ra-form-label"><div className='input-label require' tabIndex='0'>{RMResx.RM_EditTemplate_NumberofDigits}</div></div>
                    <div className="margin-bottom-24">
                        <R.Validation element="Input" require={RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue} rules={{verifyNumberOfDigits: this.validateNumberOfDigits}}>
                            <R.Input
                                id="raPrmTplColumnUniqueIdDigitsNum"
                                name='inputNumberOfDigits'
                                type='text'
                                value={this.state.numberOfDigits}
                                onChange={this.onNumberDigitsChange}
                                aria={{ariaLabel:RMResx.RM_EditTemplate_NumberofDigits}}
                            />
                    </R.Validation>
                    </div>
                </div>;
    }

    renderMainContent() {
        return <React.Fragment>
            {this.renderTemplateBaseInfo()}
            {!this.props.isGlobalUniqueIdSetting && this.renderUniqueIdSettings()}
            <div id='columnsContainer'>
                <div className='columns-title'>{RMResx.RM_PRM_TM_CatetorySection_Title}</div>
                <div id="new_category_container">
                    <div id="category_input">
                        <R.Input
                            id="raPrmTplCategoryNameIpt"
                            name='iptCategoryName'
                            type='text'
                            value={this.state.newCategoryName}
                            onChange={this.onChangeCategoryName.bind(this)}
                            aria={{ariaLabel:RMResx.RM_PRM_TM_CatetorySection_Title}}
                        />
                    </div>
                    <R.Button ghost={true} id="raPrmTplAddCategoryBtn" className="btn-category" text={RMResx.RM_EditTemplate_NewCategoryText} onClick={this.onNewCategoryClick} />
                    {this.state.showCategoryNameError && <div tabIndex="0" className={"ra-validation-msg"}>{this.state.categoryNameErrorMsg}</div>}
                </div>
                <div>
                    {this.renderColumnTables()}
                </div>
            </div>
        </React.Fragment>;
    }

    getTemplateTitleInfo() {
        let templateType = this.isEditMode ? this.state.templateInfo.type : parseInt(this.newTempType, 10);
        if(templateType == Constants.TemplateTypes.Box)
        {
            return {settingTitle: RMResx.RM_PRM_TM_BoxTemplateSectionTitle, nameTitle: RMResx.RM_PRM_TM_BoxTemplateName};
        }
        if(templateType == Constants.TemplateTypes.Folder)
        {
            return {settingTitle: RMResx.RM_PRM_TM_FolderTemplateSectionTitle, nameTitle: RMResx.RM_PRM_TM_FolderTemplateName};
        }
        if(templateType == Constants.TemplateTypes.Records)
        {
            return {settingTitle: RMResx.RM_PRM_TM_RecordTemplateSectionTitle, nameTitle: RMResx.RM_PRM_TM_RecordTemplateName};
        }
        if(templateType == Constants.TemplateTypes.CustomTemplate)
        {
            return {settingTitle: RMResx.RM_PRM_TM_CustomTemplateSectionTitle, nameTitle: RMResx.RM_PRM_TM_CustomTemplateName};
        }
        return {settingTitle: "", nameTitle: ""};
    }

    renderTemplateBaseInfo() {
        let {settingTitle, nameTitle} = this.getTemplateTitleInfo();
        return <div>
                <div className="setting-title">{settingTitle}</div>
                <div className="margin-bottom-24">
                    <div className="ra-form-label " ><div className='input-label require'>{nameTitle}</div></div>
                    <R.Validation element="Input" rules={{
                        customVerify: this.verifyNameTitle
                    }}>
                        <R.Input
                            id="raPrmTplNameIpt"
                            name='iptTemplateName'
                            type='text'
                            value={this.state.templateName}
                            onChange={this.onChangeTemplateName.bind(this)}
                            aria={{ariaLabel:nameTitle}}
                        />
                    </R.Validation>
                </div>
                <div className="ra-form-label" ><div className='input-label'>{RMResx.RM_PRM_TM_TemplateDesc}</div></div>
                <div className="margin-bottom-24">
                    <R.Input
                        name='iptTemplateDesc'
                        type='textarea'
                        height={88}
                        value={this.state.templateDesc}
                        onChange={this.onChangeTemplateDesc.bind(this)}
                        aria={{ariaLabel:RMResx.RM_PRM_TM_TemplateDesc}}
                    />
                </div>
                </div>;
    }

    verifyNameTitle(value) {
        if (!value) {
            return RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue;
        }

        if (value.length > 2000) {
            return RMResx.RM_PRM_TM_TemplateName_TooLongErrorMsg;
        }

        return true
    }

    onChangeTemplateName(value){
        this.setState({
            templateName: $.trim(value),
        });
        this.props.notifySettingsChanged();
    }

    onChangeTemplateDesc(value) {
        this.setState({
            templateDesc: $.trim(value)
        });
        this.props.notifySettingsChanged();
    }

    onChangeCategoryName(value) {
        this.setState({
            newCategoryName: $.trim(value),
            categoryAddedStatus: false,
            showCategoryNameError: false,
            categoryNameErrorMsg: ""
        });
        this.props.notifySettingsChanged();
    }

    validateCategoryName = ()=> {
        if(this.state.showCategoryNameError) {
            return false;
        }
        if(!this.state.categoryAddedStatus && $.trim(this.state.newCategoryName))  {
            //已输入category name，但是没点击new category button
            this.setState({showCategoryNameError: true, categoryNameErrorMsg: RMResx.RM_PRM_TM_NotSavedEnterCategoryNameMsg});
            return false;
        }
        return true;
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
                                    <R.Button type="bald" tooltip={RMResx.RM_JS_Common_Edit} icon="fia-edit icon-option-item" onClick={this.onCategoryEdit.bind(this, id)} />
                                </span>
                            }
                            {
                                item.allowEdit && <span className="aui-expander-title-widget category-btn">
                                    <R.Button type="bald" tooltip={RMResx.RM_JS_Common_Delete} icon="fia-delete icon-option-item" onClick={this.onCategoryDelete.bind(this, id)} />
                                </span>
                            }
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
                        <div tabIndex="0" className="column_add_content" onClick={this.onNewColumnClick.bind(this, id)} onKeyDown={this.onNewColumnKeyDown.bind(this, id)}>
                            <div className="column_add_icon">
                                <div className="fia-plus"></div>
                            </div>
                            <span className="column_add_text">{RMResx.RM_PRM_TM_Btn_NewColumn}</span>
                        </div>
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
                    notifySettingsChanged={this.props.notifySettingsChanged}
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
                    notifySettingsChanged={this.props.notifySettingsChanged}
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

    render() {
        let haveChange = this.state.haveChange;
        return <div id={this.props.id}>
            <R.Validation>
            <div id='raEditTemplate' ref={r => this.allValidation = r}>
                {/* <Prompt message={PageI18N.LeavePageMessage} when={haveChange} /> */}
                {/* <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                /> */}
                {this.state.showMain && <div id='mainContainer' className="ra-page-main">
                    {this.renderMainContent()}
                    {this.renderCategoryPanel()}
                    {this.renderColumnPanel()}
                </div>}
            </div>
            </R.Validation>
        </div>;  
    }
}