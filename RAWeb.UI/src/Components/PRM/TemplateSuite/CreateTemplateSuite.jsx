import { Component } from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RouterUrls from "../../../Constants/RouterUrls";
import { SaveTemplateResult } from ".././Constants";
import { bindEvents } from "../../../Utilities/CommonUtil";

import '../../../Less/PRM/CreateTemplateSuite.less';
import { EmptyGUID } from "../../../Constants/Constants";

const TemplateCreateMethod = {
    New: 0,
    ExistingFolder: 1
};
export default class CreateTemplateSuite extends Component {
    constructor(props) {
        super(props);
        this.id = RM.Url.getParam(window.location.href, 'id');
        this.isEditMode = !!this.id;
        this.state = {
            fromTypes: [
                { text: RMResx.RM_PRM_TM_Suite_StartFromType_Box, value: "1", checked: true, disabled: false },
                { text: RMResx.RM_PRM_TM_Suite_StartFromType_Folder, value: "2", checked: false, disabled: false }
            ],
            suiteName: "",
            suiteDesc: "",
            selStartFromType: 1,
            selTemplateCreateMethod: "0",
            rooTemplateId: EmptyGUID,
            selTemplateId: EmptyGUID,
            rootTemplateName: "",
            suiteUniqueId: "",
            showTip: false,
            tipType: "success",
            tipMsg: "",
            templateFolderItems: [],
            disabledSelTemplate: false,
            invalidSuiteName: false,
            invalidSelFolderTemplate: false
        };
        this.initFolderTemplatesComboboxData();
        bindEvents(this, "handleRadioFromTypesChanged", "onClickSave", "onClickCancel", "onChangeSuiteName", "onChangeSuiteDesc", "loadData", "getStartFromTypeOptions",
            "getCreateTemplateMethodOptions", "getExistingTemplateItems"
        );
    }

    componentDidMount() {
        if (this.isEditMode) {
            this.loadSuiteData();
        }
    }

    initFolderTemplatesComboboxData() {
        let option = {
            url: `/api/TemplateManagementApi/GetExistingFolderTemplatesInfo?suiteId=${this.id || EmptyGUID}`,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            if(result.FolderTemplates)
            {
                this.setState({ templateFolderItems: result.FolderTemplates });
            }
        }).catch((e) => {

        });
    }

    loadSuiteData() {
        let option = {
            url: "/api/TemplateManagementApi/LoadSuite?id=" + this.id,
            method: "get",
        };
        fetchUtility(option).then((result) => {
            if(result)
            {
                let hasRootTemplate = result.RootTemplateUniqueId != EmptyGUID;
                let selectedFolderTemplateId = result.RootTemplateCreateType == TemplateCreateMethod.ExistingFolder ? result.RootTemplateUniqueId : EmptyGUID;
                this.setState({
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

    onClickSave() {
        let dto = {};
        dto.Name = this.state.suiteName;
        dto.Description = this.state.suiteDesc;
        dto.StartFromType = this.state.selStartFromType;
        dto.UniqueId = this.state.suiteUniqueId;
        dto.RootTemplateCreateType = this.state.selTemplateCreateMethod;
        dto.RootTemplateUniqueId = this.state.selTemplateId;

        if (this.validateForm(dto)) {
            if (!this.isEditMode) {
                this.createTemplateSuite(dto);
            } else {
                this.editTemplateSuite(dto);
            }
        }
    }

    onClickCancel() {
        this.routerTo(RouterUrls.PRM_TemplateManagement);
    }

    validateForm(dto) {
        let result = true,
            invalidName = false,
            invalidSelFolderTemplate = false;
        if (!this.validateIsNotEmpty(dto.Name)) {
            invalidName = true;
            result = false;
        }
        if (dto.RootTemplateCreateType == TemplateCreateMethod.ExistingFolder && this.state.selTemplateId == EmptyGUID) {
            invalidSelFolderTemplate = true;
            result = false;
        }

        if (!result) {
            this.setState({
                invalidSuiteName: invalidName,
                invalidSelFolderTemplate: invalidSelFolderTemplate
            });
        }
        return result;
    }

    validateIsNotEmpty(val) {
        return $.trim(val) != '';
    }

    createTemplateSuite = (dto) => {
        $$.loading(true);
        let urlData = "/api/TemplateManagementApi/CreateSuite";
        let option = {
            url: urlData,
            method: "POST",
            data: dto
        };

        fetchUtility(option).then((res) => {
            if (res == SaveTemplateResult.Success) {
                RM.CommStatus.save(RM.CommStatus.CreateSuccess);
                setTimeout(() => {
                    $$.loading(false);
                    this.routerTo(RouterUrls.PRM_TemplateManagement);
                }, 500);
            } else if (res == SaveTemplateResult.NameDuplicate) {
                this.showMessageTip("error", RMResx.RM_Template_SuiteNameDuplicate);
                $$.loading(false);
            } else {
                this.showMessageTip("error", RMResx.RM_PRM_TM_Suite_SaveFailed);
                $$.loading(false);
            }
        }).catch((e) => {
            this.showMessageTip("error", RMResx.RM_PRM_TM_Suite_SaveFailed);
            $$.loading(false);
        });
    }

    editTemplateSuite = (dto) => {
        $$.loading(true);
        let urlData = "/api/TemplateManagementApi/UpdateSuite";
        let option = {
            url: urlData,
            method: "POST",
            data: dto
        };

        fetchUtility(option).then((res) => {
            if (res) {
                RM.CommStatus.save(RM.CommStatus.EditSuccess);
                setTimeout(() => {
                    $$.loading(false);
                    this.routerTo(RouterUrls.PRM_TemplateManagement);
                }, 500);
            } else {
                this.showMessageTip("error", RMResx.RM_PRM_TM_Suite_SaveFailed);
                $$.loading(false);
            }
        }).catch((e) => {
            this.showMessageTip("error", RMResx.RM_PRM_TM_Suite_SaveFailed);
            $$.loading(false);
        });
    }

    onChangeSuiteName(value) {
        this.setState({
            suiteName: $.trim(value)
        });
    }

    onChangeSuiteDesc(value) {
        this.setState({
            suiteDesc: $.trim(value)
        });
    }

    onSelectTemplate(args) {
        this.setState({
            selTemplateId: args.newValue.value
        });
    }

    handleRadioFromTypesChanged(value) {
        this.setState({
            selStartFromType: value,
            selTemplateCreateMethod: "0",
            selTemplateId: EmptyGUID
        });
        this.hideMessageTip();
    }

    getStartFromTypeOptions() {
        let options = [
            { text: RMResx.RM_PRM_TM_Suite_StartFromType_Box, value: "1", disabled: false },
            { text: RMResx.RM_PRM_TM_Suite_StartFromType_Folder, value: "2", disabled: false }
        ];
        let hasRootTemplate = this.state.rooTemplateId != EmptyGUID;
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.selStartFromType == op.value;
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
        folderOption.shown = this.state.selStartFromType == 2 ? true : false;

        let needOptions = options.filter(op => op.shown == true);
        return needOptions.map(op => {
            op.title = op.text;
            op.checked = this.state.selTemplateCreateMethod == op.value;
            op.disabled = this.state.rooTemplateId != EmptyGUID;
            return op;
        });
    }

    getExistingTemplateItems() {
        let resultItems = [];
        let existTemplateItems = [];
        if (this.state.selTemplateCreateMethod == "1") {
            existTemplateItems = this.state.templateFolderItems.slice();
        }

        existTemplateItems.forEach(r => {
            resultItems.push({
                title: r.Name,
                value: r.UniqueId,
                checked: r.UniqueId == this.state.selTemplateId
            });
        });

        if (this.isEditMode && resultItems.length == 0) {
            resultItems = [{ title: this.state.rootTemplateName, value: this.state.rooTemplateId, checked: true }];
        }
        return resultItems;
    }

    handleRadioCreateTemplateMethodChanged(value) {
        this.setState({
            selTemplateCreateMethod: value,
            selTemplateId: EmptyGUID
        });
        this.hideMessageTip();
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

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    render() {
        let pageMapLink = !this.isEditMode ? SiteMapLinks.PRM_CreateTemplateSuite : SiteMapLinks.PRM_EditTemplateSuite;
        return <React.Fragment>
            <$g.SiteMap data={[SiteMapLinks.PRM_TemplateManagement, pageMapLink]} />
            <div>
                <R.Messagebar
                    message={this.state.tipMsg}
                    classify={this.state.tipType}
                    status={{ show: this.state.showTip }}
                />
            </div>
            <div id="newTemplateSuiteContainer">
                <div id="left" className="ra-section">
                    <div className="ra-form-label ra-require" ><div className='input-label' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_Name}</div></div>
                    <div>
                        <R.Input
                            name='iptSuiteName'
                            type='text'
                            width={500}
                            value={this.state.suiteName}
                            onChange={this.onChangeSuiteName.bind(this)}
                            aria={{ariaLabel:RMResx.RM_PRM_TM_Suite_Name}}
                        />
                        {this.state.invalidSuiteName && <div className='ra-validation-msg' id="invalid_msg_name">
                            {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                        </div>}
                    </div>

                    <div className="ra-form-label"><div className='input-label' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_Desc}</div></div>
                    <div className="ra-form-content">
                        <R.Input
                            name='iptSuiteDesc'
                            type='textarea'
                            width={500}
                            height={100}
                            value={this.state.suiteDesc}
                            onChange={this.onChangeSuiteDesc.bind(this)}
                            aria={{ariaLabel:RMResx.RM_PRM_TM_Suite_Desc}}
                        />
                    </div>
                </div>
                <div id="right">
                    <div className="ra-form-label suite-title">{RMResx.RM_PRM_TM_Suite_TemplateSuite_Title}</div>
                    <div className="suite-desc">
                        {RMResx.RM_PRM_TM_Suite_TemplateSuite_Desc}
                    </div>
                </div>
                <div className="ra-section" id="suiteStructure">
                    <div className="structure-desc">{RMResx.RM_PRM_TM_Suite_TemplateSuiteStructure}</div>
                    <div className="ra-form-label ra-require" ><div className='input-label' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_StartFromTitle}</div></div>
                    <R.Radio.Group
                        block={true}
                        name="radiogroup-startFromTypes"
                        items={this.getStartFromTypeOptions()}
                        onChange={this.handleRadioFromTypesChanged.bind(this)}
                    />
                    {this.state.selStartFromType == 2 && <div>
                        <div className="ra-form-label ra-require" ><div className='input-label' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_CreateFolderTemplateMethodTip}</div></div>
                        <R.Radio.Group
                            block={true}
                            name="radiogroup-template-create-method"
                            items={this.getCreateTemplateMethodOptions()}
                            onChange={this.handleRadioCreateTemplateMethodChanged.bind(this)}
                        />
                    </div>}

                    {this.state.selStartFromType == 2 && this.state.selTemplateCreateMethod == "1" &&
                        <div>
                            <div className="ra-form-label ra-require" ><div className='input-label' tabIndex='0'>{RMResx.RM_PRM_TM_Suite_SelectExistingFolderTip}</div></div>
                            <R.Combobox
                                searchable={false}
                                width="600"
                                popupWidth="600"
                                textField='title'
                                valueField='value'
                                checkedField='checked'
                                excludeChecked
                                items={this.getExistingTemplateItems()}
                                onChange={(args) => this.onSelectTemplate(args)}
                                disabled={this.state.disabledSelTemplate}
                            />
                            {this.state.invalidSelFolderTemplate && <div className='ra-validation-msg'>
                                {RMResx.RM_PRM_TM_Suite_NeedToSelectTemplateTip}
                            </div>}
                        </div>
                    }
                </div>
                <div id="footer" className="ra-form-foot-btns">
                    <R.Button text={RMResx.RM_JS_Common_Cancel} onClick={this.onClickCancel} /> 
                    <R.Button primary={true} classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onClickSave} />    
                </div>
            </div></React.Fragment>;
    }
}