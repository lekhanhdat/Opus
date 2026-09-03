import { bindEvents } from "../../Utilities/CommonUtil";
import { EmptyGUID } from "../../Constants/Constants";

const TemplateCreateMethod = {
    New: 0,
    ExistingFolder: 1
};

export default class TemplateSuiteSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            suiteItem: this.getInitSuiteItem()
        };
        this.isEditMode = false;
        bindEvents(this, "handleRadioFromTypesChanged", "onChangeSuiteName", "onChangeSuiteDesc", "loadData", "getStartFromTypeOptions",
            "getCreateTemplateMethodOptions", "getExistingTemplateItems", "updateObjectProperties"
        );
    }

    componentInit() {
    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onSave":
                if($$.verify(this.allValidation))
                {
                    let saveSuiteItem = args[0];
                    saveSuiteItem(this.getSuiteItemDto());
                }
                break;
            case "init":
                this.initData(args[0]);
                $$.verify(this.allValidation, false);
                break;
            case "new":
                this.id = EmptyGUID;
                this.initFolderTemplatesComboboxData();
                this.setState({suiteItem: this.getInitSuiteItem()})
                break;
        }
    }

    initData(suiteUniqueId) {
        this.id = suiteUniqueId;
        this.initFolderTemplatesComboboxData();
        this.loadSuiteData();
    }

    getInitSuiteItem() {
        return {
            suiteName: "",
            suiteDesc: "",
            selStartFromType: 1,
            selTemplateCreateMethod: "0",
            rooTemplateId: EmptyGUID,
            selTemplateId: EmptyGUID,
            rootTemplateName: "",
            suiteUniqueId: EmptyGUID,
            templateFolderItems: [],
            disabledSelTemplate: false
        };
    }

    initFolderTemplatesComboboxData() {
        let option = {
            url: `/api/TemplateManagementApi/GetExistingFolderTemplatesInfo?suiteId=${this.id || EmptyGUID}`,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            if(result.FolderTemplates)
            {
                this.updateSuiteItemStateProperties({ templateFolderItems: result.FolderTemplates });
            }
        }).catch((e) => {

        });
    }

    loadSuiteData() {
        let option = {
            url: `/api/TemplateManagementApi/LoadSuite?id=${this.id}`,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            if(result)
            {
                this.isEditMode = true;
                let hasRootTemplate = result.RootTemplateUniqueId != EmptyGUID;
                let selectedFolderTemplateId = result.RootTemplateCreateType == TemplateCreateMethod.ExistingFolder ? result.RootTemplateUniqueId : EmptyGUID;
                this.updateSuiteItemStateProperties({
                    suiteName: this.wrapperI18N(result.Name),
                    suiteDesc: result.Description,
                    selStartFromType: result.StartFromType,
                    suiteUniqueId: result.UniqueId,
                    selTemplateCreateMethod: result.RootTemplateCreateType,
                    rooTemplateId: result.RootTemplateUniqueId,
                    rootTemplateName: result.RootTemplateName,
                    selTemplateId: selectedFolderTemplateId,
                    disabledSelTemplate: hasRootTemplate
                });
            }
        }).catch((e) => {

        });
    }

    getSuiteItemDto()
    {
        let suiteItem = this.getSuiteItem();
        let dto = {
            Name: suiteItem.suiteName,
            Description: suiteItem.suiteDesc,
            StartFromType: suiteItem.selStartFromType,
            UniqueId: suiteItem.suiteUniqueId,
            RootTemplateCreateType: suiteItem.selTemplateCreateMethod,
            RootTemplateUniqueId: suiteItem.selTemplateId
        };
        return dto;
    }

    getSuiteItem()
    {
        return RM.deepcopy(this.state.suiteItem);
    }

    updateObjectProperties(sourceObj, propertyObj) {
        for (let key in propertyObj) {
            if(sourceObj.hasOwnProperty(key))
            {
                sourceObj[key] = propertyObj[key];
            }
        }
    }

    updateSuiteItemStateProperties(modifiedPropertyObj, callback) {
        let suiteItem = this.getSuiteItem();
        this.updateObjectProperties(suiteItem, modifiedPropertyObj);
        this.setState({suiteItem: suiteItem}, ()=> { typeof callback === "function" && callback(); });
    }

    onChangeSuiteName(value) {
        this.updateSuiteItemStateProperties({suiteName: $.trim(value)});
        this.props.notifySettingsChanged();
    }

    onChangeSuiteDesc(value) {
        this.updateSuiteItemStateProperties({suiteDesc: $.trim(value)});
        this.props.notifySettingsChanged();
    }

    onSelectTemplate(args) {
        this.updateSuiteItemStateProperties({selTemplateId: args.newValue.value});
        this.props.notifySettingsChanged();
    }

    handleRadioFromTypesChanged(value) {
        this.updateSuiteItemStateProperties({
            selStartFromType: value,
            selTemplateCreateMethod: "0",
            selTemplateId: EmptyGUID
        });
        this.props.notifySettingsChanged();
    }

    getStartFromTypeOptions() {
        let options = [
            { text: RMResx.RM_PRM_TM_Suite_StartFromType_Box, value: "1", disabled: false },
            { text: RMResx.RM_PRM_TM_Suite_StartFromType_Folder, value: "2", disabled: false },
            { text: RMResx.RM_PRM_TM_Suite_StartFromType_Custom, value: "3", disabled: false },
        ];
        let hasRootTemplate = this.state.suiteItem.rooTemplateId != EmptyGUID;
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.suiteItem.selStartFromType == op.value;
            op.disabled = hasRootTemplate;
            return op;
        });
    }

    getCreateTemplateMethodOptions() {
        let options = [
            { text: RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethod_New, value: "0", shown: true, disabled: false },
            { text: RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethod_AddExisting, value: "1", shown: true, disabled: false }
        ];

        let folderOption = options.find(op => parseInt(op.value, 10) == TemplateCreateMethod.ExistingFolder);
        folderOption.shown = this.state.suiteItem.selStartFromType == 2 ? true : false;

        let needOptions = options.filter(op => op.shown == true);
        return needOptions.map(op => {
            op.title = op.text;
            op.checked = this.state.suiteItem.selTemplateCreateMethod == op.value;
            op.disabled = this.state.suiteItem.rooTemplateId != EmptyGUID;
            return op;
        });
    }

    getExistingTemplateItems() {
        let resultItems = [];
        let existTemplateItems = [];
        if (this.state.suiteItem.selTemplateCreateMethod == "1") {
            existTemplateItems = this.state.suiteItem.templateFolderItems.slice();
        }

        existTemplateItems.forEach(r => {
            resultItems.push({
                title: r.Name,
                value: r.UniqueId,
                checked: r.UniqueId == this.state.suiteItem.selTemplateId
            });
        });

        if (this.isEditMode && resultItems.length == 0) {
            resultItems = [{ title: this.state.suiteItem.rootTemplateName, value: this.state.suiteItem.rooTemplateId, checked: true }];
        }
        return resultItems;
    }

    handleRadioCreateTemplateMethodChanged(value) {
        this.updateSuiteItemStateProperties({
            selTemplateCreateMethod: value,
            selTemplateId: EmptyGUID
        });
        this.props.notifySettingsChanged();
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    showFolderCreationMethodSettings() {
        return this.isEditMode && this.state.suiteItem.selTemplateCreateMethod == TemplateCreateMethod.ExistingFolder;
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

    render() {
        let showFolderCreationSettings = this.showFolderCreationMethodSettings();
        return <div id={this.props.id}>
            <R.Validation>
            <div ref={r => this.allValidation = r}>
                    <div style={{ marginBottom: "24px" }}>
                        <div className="ra-form-label" ><div className='require input-label'>{RMResx.RM_PRM_TM_Suite_Name}</div></div>
                        <R.Validation element="Input" rules={{
                            customVerify: this.verifyNameTitle
                        }}>
                            <R.Input
                                id="raPrmTplSuiteNameIpt"
                                name='iptSuiteName'
                                type='text'
                                value={this.state.suiteItem.suiteName}
                                onChange={this.onChangeSuiteName.bind(this)}
                                aria={{ ariaLabel: RMResx.RM_PRM_TM_Suite_Name, 'aria-required': true }}
                            />
                        </R.Validation>
                    </div>
                    <div className="ra-form-label"><div className='input-label'>{RMResx.RM_PRM_TM_Suite_Desc}</div></div>
                    <div className="ra-form-content">
                        <R.Input
                            name='iptSuiteDesc'
                            type='textarea'
                            height={88}
                            value={this.state.suiteItem.suiteDesc}
                            onChange={this.onChangeSuiteDesc.bind(this)}
                            aria={{ ariaLabel: RMResx.RM_PRM_TM_Suite_Desc }}
                        />
                    </div>
                <div id="suiteStructure">
                    {/* <div className="structure-desc">{RMResx.RM_PRM_TM_Suite_TemplateSuiteStructure}</div> */}
                    <div className="ra-form-label" >
                        <div className='require input-label'>
                            <span id="ariaStartFrom">{RMResx.RM_PRM_TM_Suite_StartFromTitle}</span>
                        </div>
                    </div>
                    <R.Radio.Group
                        block={true}
                        name="radiogroup-startFromTypes"
                        items={this.getStartFromTypeOptions()}
                        onChange={this.handleRadioFromTypesChanged.bind(this)}
                        aria="#ariaStartFrom"
                    />
                    {showFolderCreationSettings && this.state.suiteItem.selStartFromType == 2 && <div>
                        <div className="ra-form-label" ><div className='input-label require' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethodTip}</div></div>
                        <R.Radio.Group
                            block={true}
                            name="radiogroup-template-create-method"
                            items={this.getCreateTemplateMethodOptions()}
                            onChange={this.handleRadioCreateTemplateMethodChanged.bind(this)}
                        />
                    </div>}

                    {showFolderCreationSettings && this.state.suiteItem.selStartFromType == 2 && this.state.suiteItem.selTemplateCreateMethod == "1" &&
                        <div>
                            <div className="ra-form-label" ><div className='input-label require' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_SelectExistingFolderTip}</div></div>
                            <R.Validation element="Combobox" require={RMResx.RM_PRM_TM_Suite_NeedToSelectTemplateTip}>
                                <R.Combobox
                                    searchable={false}
                                    width="100%"
                                    height={32}
                                    textField='title'
                                    valueField='value'
                                    checkedField='checked'
                                    excludeChecked
                                    items={this.getExistingTemplateItems()}
                                    onChange={(args) => this.onSelectTemplate(args)}
                                    disabled={this.state.suiteItem.disabledSelTemplate}
                                />
                            </R.Validation>
                        </div>
                    }
                </div>
            </div>
            </R.Validation>
        </div>;
    }
}