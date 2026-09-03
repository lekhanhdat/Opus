import { NodeType } from "../../../Constants/DAEnums";
import { bindEvents } from "../../../Utilities/CommonUtil";
import StringUtil from '../../../Utilities/StringUtil';
import { PhysicalObjectColumnType, PhysicalDefaultColumnIDs, EmptyGUID, PhysicalObjectStatus, TelemetryModule, TelemetryEventType, SourceFlags } from "../../../Constants/Constants";
import { ColumnTypesEnum, PhyCategoryBaseInfoId } from "../Constants";
import TermTree from '../../Common/Tree/Instances/TermTree/SelectTermTree';
import PeoplePicker from "../../Common/PeoplePicker";
import { PhyObjFormType } from "../RecordsExplorer/RecordsExplorer";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import RouterUrls from "../../../Constants/RouterUrls";
import { RegexUtil } from "../../../Utilities/RegexUtil";

const readonlyColumns = [PhysicalDefaultColumnIDs.HomeLocation];

export default class PhyObjectForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        bindEvents(this, "onSelectedClassChanged", "onClassSelectorClick", "onReuestCommentChange", "showMessageTip",
            "hideMessageTip", "inheritanceClick", "breakInheritance", "onAccessPermissionChanged", "onKeyDown");
        this.defaultDateFormat = RM.TimeUtil.getGlobalAuiFormat();

        this.requestDto = null; //only for New/Edit Request Form.
        this.formData = {
            Name: null,
            NodeType: null,
            Id: null,
            TermId: null,
            LocationId: null,
            LocationName: null,
            HomeLocationFullPath: null,
            BoxId: null,
            FileId: null,
            TemplateId: null,
            IsLocked: null,
            MetaInfo: null,
            ExportToRECO: null,
            isBreakInheritance: false,
            selectedUsersValid: false,
            UniqueId: null,
        };

        this.classificationColumn = {};

        this.state = {
            selectedPermissionUsers: [],
            hasClassifySetting: true,
            showMessageTip: this.showMessageTip,
            showDispClassPanel: { show: false },
            isSaving: false,
            dispClassRoot: null,
            data: [],
            showMsgBar: false,
            barcodeValue: "",
        };
        this.props.setPanelTitle("");
    }

    componentReceive(type, ...args) {
        switch (type) {
            case "onSave":
                this.parentIdList = args[1];
                this.saveFormData(args[0]);
                break;
        }
    }

    componentInit() {
        this.initData(this.props.data);
    }


    initData(args) {
        this.formData = args;
        switch (args.formType) {
            case PhyObjFormType.NewRequest:
            case PhyObjFormType.CreatePhyObj:
                this.initNewFormData();
                this.initPermissionInfo();
                break;
            case PhyObjFormType.EditPhyObj:
                this.initEditPhyObjFormData();
                break;
            case PhyObjFormType.EditRequest:
                this.initEditRequestFormData();
                break;
            default:
                return;
        }
    }

    showMessageTip(type, msg) {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip() {
        this.setState({
            showTip: false
        });
    }

    initNewFormData() {
        let locationId = this.formData.LocationId;
        let nodeType = this.formData.NodeType;
        let templateId = this.formData.TemplateId;
        let boxId = this.formData.BoxId;
        let foldId = this.formData.FileId;
        this.formData.IsLocked = false;
        this.formData.ExportToRECO = false;
        $$.loading(true);
        if (this.formData.formType == PhyObjFormType.NewRequest) {
            this.requestDto = {
                Type: 1,
                PhysicalRequestStatus: 0
            };
        }
        let url = `/api/PhysicalRecordApi/GetTemplateDataById`;
        let option = {
            url: url,
            method: "Post",
            data: {
                TemplateId: templateId,
                LocationUid: locationId,
                PhyNodeInfo:{
                    NodeType: this.formData.NodeType,
                    BoxId: this.formData.BoxId,
                    FileId: this.formData.FileId,
                }
            } 
        };
        fetchUtility(option)
            .then(result => {
                for(let categorie of result.Template.categories){
                    let newColumns = [];
                    for(let column of categorie.columns){
                        if(column.uniqueId != PhysicalDefaultColumnIDs.LoanedBy ){
                            newColumns.push(column);
                        }
                    }
                    categorie.columns = newColumns;
                }
                this.initInheritColumnValues(result.Template.categories);
                this.classificationColumn = this.getClassificationColumn(result.Template.categories);
                this.initClassificationSetting(result.Settings, true);
                this.formData.TemplateId = result.Template.id;
                this.setState({
                    data: result.Template.categories
                });
                result.Template.name = RMResx[result.Template.name] ? RMResx[result.Template.name] : result.Template.name;
                this.props.setPanelTitle(result.Template.name);
                $$.loading(false);
            })
            .catch(e => {
            });
    }

    initEditPhyObjFormData() {
        let nodeId = this.formData.Id;
        let nodeType = this.formData.NodeType;
        $$.loading(true);
        let url = `/api/PhysicalRecordApi/GetPhysicalObjectById`;
        let option = {
            url: url,
            method: "POST",
            data: {
                Id: nodeId,
                NodeType: nodeType,
                TemplateIdPath: '',
                PhyNodeInfo: {
                    NodeType: this.formData.NodeType,
                    BoxId: this.formData.BoxId,
                    FileId: this.formData.FileId,
                }
            }
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then(res => {
            $$.loading(false);
            let phyObj = JSON.parse(res);
            this.initEditFormData(phyObj);
        });
    }

    initEditRequestFormData() {
        let requestId = this.props.requestId;
        $$.loading(true);
        let url = `/api/PhysicalRequestApi/GetRequest?id=${requestId}`;
        let option = {
            url: url,
            method: "GET"
        };
        fetchUtility(option).then(reqDto => {
            $$.loading(false);
            this.requestDto = reqDto;
            const physicalFileInfo = reqDto.PhysicalFileInfo ? reqDto.PhysicalFileInfo : reqDto.PhysicalFileInfos[0];
            this.initEditFormData(physicalFileInfo);
        });
    }

    initEditFormData(phyObj) {
            let values = phyObj.MetaInfo;
            let categories = phyObj.Template.categories;
            for (const category of categories) {
                for (const column of category.columns) {
                    //通过template得到修改数据的信息
                    if (column.typeId == ColumnTypesEnum.SingleChoice || column.typeId == ColumnTypesEnum.MultipleChoice) {
                        if (values[column.uniqueId]) {
                            let oldColumnValue = JSON.parse(values[column.uniqueId]);
                            let newSingleColumnValue = {};
                            let newMulColumnValue = [];
                            if (column.typeId == ColumnTypesEnum.SingleChoice) {
                                newSingleColumnValue['Value'] = oldColumnValue.Value;
                                newSingleColumnValue['Name'] = JSON.parse(column.optionsJSON)[oldColumnValue.Value];
                                column.columnValue = JSON.stringify(newSingleColumnValue);
                            } else {
                                newMulColumnValue = oldColumnValue.filter((item) => {
                                    return item['Name'] = JSON.parse(column.optionsJSON)[item.Value];
                                });
                                column.columnValue = JSON.stringify(newMulColumnValue);
                            }
                        }
                    } else {
                        column.columnValue = values[column.uniqueId];
                        if (phyObj.NodeType > NodeType.PhysicalBottomLocation && column.uniqueId == PhysicalDefaultColumnIDs.HomeLocation) {
                            let oldValue = JSON.parse(column.columnValue);
                            let newHomeLocationColumnValue = {
                                Id: oldValue.Id,
                                Name: phyObj.HomeLocationFullPath
                            };
                            column.columnValue = JSON.stringify(newHomeLocationColumnValue);
                        }
                    }
                }
            }

        if (this.formData.formType != PhyObjFormType.EditRequest && (phyObj.NodeType == NodeType.PhyFile || phyObj.NodeType == NodeType.PhyBox)) {
            let loanedByInfo = phyObj.MetaInfo[PhysicalDefaultColumnIDs.LoanedBy];
            let loanedByDisabled = phyObj.Status == PhysicalObjectStatus.Destroyed || phyObj.Status == PhysicalObjectStatus.Missing;
            // categories[0].columns.push({
            //     categoryId: categories[0].id,
            //     uniqueId: PhysicalDefaultColumnIDs.LoanedBy,
            //     columnName: RMResx.RM_PRM_PRE_Column_LoanBy,
            //     columnValue: phyObj.PersonHold && loanedByInfo && !loanedByDisabled && JSON.parse(loanedByInfo).length > 0 ? JSON.stringify([JSON.parse(loanedByInfo)[0]]) : "",
            //     typeId: PhysicalObjectColumnType.PeopleOrGroup,
            //     required: false,
            //     disabled: loanedByDisabled,
            // });
        }
        this.formData.TemplateId = phyObj.TemplateId;
        this.formData.IsLocked = phyObj.IsLocked;
        this.formData.Name = phyObj.Name;
        this.formData.NodeType = phyObj.NodeType;
        this.formData.TermId = phyObj.TermId;
        this.formData.LocationId = phyObj.LocationId;
        this.formData.BoxId = phyObj.BoxId;
        this.formData.FileId = phyObj.FileId;
        this.formData.ExportToRECO = phyObj.ExportToRECO;
        this.formData.ParentId = phyObj.ParentId;
        this.formData.Ancestors = phyObj.Ancestors;
        this.formData.UniqueId = phyObj.UniqueId;
        this.classificationColumn = this.getClassificationColumn(categories);
        this.loadClassificationSetting();
        this.setState({
            data: categories,
            showMsgBar: true,
        });
        //权限回显
        if (phyObj.NodeType == NodeType.PhyFile || phyObj.NodeType == NodeType.PhyBox) {
            let scopePerDto = phyObj.ScopePerDto;
            if (scopePerDto) {
                let accounts = scopePerDto.Accounts || [];
                if (!scopePerDto.IsInheritSave) {
                    for (let item of accounts) {
                        item.Checked = true;
                    }
                    this.setState({
                        isBreakInheritance: true,
                        selectedPermissionUsers: accounts
                    });
                    this.setInheritPermissionInfo(phyObj, true);
                } else {
                    this.setInheritPermissionInfo(phyObj, false);
                }
            } else {
                //兼容老数据
                this.setInheritPermissionInfo(phyObj, false);
            }
        }
        phyObj.Template.name = RMResx[phyObj.Template.name] ? RMResx[phyObj.Template.name] : phyObj.Template.name;
        this.props.setPanelTitle(phyObj.Template.name);
    }

    setInheritPermissionInfo(data, isBreakInheritance) {
        $$.loading(true);
        //ScopePerDto如果无数据的时候，说明是继承，取父级id
        let scopeId = data.LocationId;
        if (NodeType.PhyFile) {
            if (data.BoxId) {
                scopeId = data.BoxId;
            } else {
                scopeId = data.LocationId;
            }
        }
        let option = {
            url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${scopeId}&includeSelf=${true}`,
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            let res = JSON.parse(result);
            res.Accounts = res.Accounts || [];
            for (let item of res.Accounts) {
                item.Checked = true;
            }
            if (!isBreakInheritance) {
                this.setState({
                    isBreakInheritance: false,
                    selectedPermissionUsers: res.Accounts
                });
                this.superiorIsBreakInheritance = res.BreakInheritStatus;
            }
            this.inheritanceuserList = RM.deepcopy(res.Accounts);
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    loadClassificationSetting() {
        let locationId = this.formData.LocationId;
        let url = `/api/PhysicalRecordApi/GetTermSettingForLocation?locationId=${locationId}`;
        let option = {
            url: url,
            method: "GET"
        };
        fetchUtility(option).then(settings => {
            this.initClassificationSetting(settings, false);
        });
    }

    getInheritColumnIDs(categories) {
        let inheritColumnIDs = [];
        if (this.formData.NodeType > NodeType.PhyBox && categories) {
            for (const category of categories) {
                for (const column of category.columns) {
                    if (column.inheritFromParent) {
                        inheritColumnIDs.push({ ColumnUniqueId: column.uniqueId, PhyObjId: this.formData.BoxId });
                    } else if (column.inheritFromParentFolder) {
                        inheritColumnIDs.push({ ColumnUniqueId: column.uniqueId, PhyObjId: this.formData.FileId });
                    }
                }
            }
        }
        return inheritColumnIDs;
    }

    initInheritColumnValues(categories) {
        let inheritColumnIDs = this.getInheritColumnIDs(categories);
        if (inheritColumnIDs.length == 0) {
            return;
        }

        let boxId = this.formData.BoxId;
        let folderId = this.formData.FileId;
        let url = `/api/PhysicalRecordApi/GetPhysicalPushedColumnValues?parentPhyBoxId=${boxId}&parentPhyFolderId=${folderId}`;
        let option = {
            url: url,
            data: inheritColumnIDs,
            method: "POST"
        };
        fetchUtility(option).then(res => {
            if (res) {
                for (var category of categories) {
                    for (var column of category.columns) {
                        if (column.inheritFromParent || column.inheritFromParentFolder) {
                            column.columnValue = res[column.uniqueId];
                        }
                    }
                }

                this.setState({
                    data: categories.slice(0)
                });
            }
        });
    }

    getParentNodeType() {
        switch (this.formData.NodeType) {
            case NodeType.PhyRecord:
                return NodeType.PhyFile;
            default:
                return NodeType.PhyBox;
        }
    }

    initClassificationSetting(settings, setDefaultValue) {
        let hasClassifySetting = settings && settings.TermSetId && settings.TermSetId != EmptyGUID;
        if (hasClassifySetting) {
            let nodeType = null,
                nodeId = null;
            if (!settings.TermId || settings.TermId == EmptyGUID) {
                nodeType = "TermSet";
                nodeId = settings.TermSetId;
            } else {
                nodeType = "Term";
                nodeId = settings.TermId;
            }

            if (setDefaultValue && this.classificationColumn) {
                if (settings.DefaultTermId && settings.DefaultTermId != EmptyGUID) {
                    this.classificationColumn.columnValue = JSON.stringify({
                        Id: settings.DefaultTermId,
                        Name: settings.DefaultTermName
                    });
                }
                // this.classificationColumn.required = settings.ColumnRequired;
            }

            this.setState({
                dispClassRoot: {
                    nodeId: nodeId,
                    nodeType: nodeType,
                    allowSelected: false,
                    expandDefault: true
                }
            });
        } else {
            let tipMsg =
                <$g.I18NProvider msg={RMResx.RM_PRM_PRE_Msg_NoClassification}>
                    <a className="ra-link-a" href={RouterUrls.BCM_ContentRepositoryManagement_Phy}>{`${RMResx.RM_JS_SPS_TabLabel_Physical} ${RMResx.RM_Nav_ContentRepository}`}</a>
                </$g.I18NProvider>;
            this.showMessageTip("warn", tipMsg);
        }
        this.setState({ hasClassifySetting: hasClassifySetting });
    }

    getClassificationColumn(categories) {
        if (categories) {
            for (const category of categories) {
                for (const column of category.columns) {
                    if (column.uniqueId == PhysicalDefaultColumnIDs.Classification) {
                        return column;
                    }
                }
            }
        }
        return null;
    }

    saveFormData(callback) {
        let metaInfo = {};
        for (const category of this.state.data) {
            for (const column of category.columns) {
                metaInfo[column.uniqueId] = column.columnValue;
                //(column.columnValue && column.columnValue.length) > 255
                if ((column.required && !column.columnValue) || (column.columnName == "RM_PRM_PRE_Column_Barcode" && this.state.barcodeValue && (!RegexUtil.IsMath(this.state.barcodeValue) || this.state.barcodeValue.length > 26))) {
                    //validation: has empty required column
                    this.setState({
                        isSaving: true,
                        data: JSON.parse(JSON.stringify(this.state.data))
                    });
                    callback(false, this.formData);
                    return;
                }
            }
        }
        let statusInfoStr = metaInfo[PhysicalDefaultColumnIDs.Status];
        let statusValue = statusInfoStr ? JSON.parse(statusInfoStr).Value : null;
        if (statusValue == PhysicalObjectStatus.Destroyed) {
            this.showSetRecordStatusToDestroyMsgBox(metaInfo, callback);
        } else {
            this.saveColumnsData(metaInfo, callback);
        }
    }

    saveColumnsData(metaInfo, callback) {
        this.formData.Name = metaInfo[PhysicalDefaultColumnIDs.NameOrTitle];
        //打破继承传user和继承状态，继承直传继承状态。
        if (this.state.isBreakInheritance) {
            // if(this.state.selectedPermissionUsers.length == 0){
            //     callback(false, this.formData);
            //     this.setState({selectedUsersValid: true});
            //     return;
            // }
            let accounts = [];
            if (this.state.selectedPermissionUsers && Array.isArray(this.state.selectedPermissionUsers)) {
                for (let item of this.state.selectedPermissionUsers) {
                    let userObj = {};
                    userObj.UserId = item.UserId;
                    userObj.UserName = item.UserName;
                    userObj.UserPrincipalName = item.UserPrincipalName;
                    userObj.Email = item.Email;
                    userObj.DisplayName = item.DisplayName;
                    userObj.InviteType = item.InviteType;
                    userObj.RMUserId = item.RMUserId;
                    userObj.Id = item.Id;
                    userObj.SurName = item.SurName;
                    userObj.GivenName = item.GivenName;
                    userObj.TenantId = item.TenantId;
                    accounts.push(userObj);
                }
            }
            this.formData.ScopePerDto = {
                Accounts: accounts,
                IsInheritSave: false
            };
        } else {
            this.formData.ScopePerDto = {
                IsInheritSave: true
            };
        }
        this.formData.MetaInfo = metaInfo;
        if (this.classificationColumn && this.classificationColumn.columnValue) {
            this.formData.TermId = JSON.parse(this.classificationColumn.columnValue).Id;
        }
        if (metaInfo && metaInfo[PhysicalDefaultColumnIDs.LoanedBy]) {
            this.formData.PersonHoldBy = JSON.parse(metaInfo[PhysicalDefaultColumnIDs.LoanedBy])[0].DisplayName;
            this.formData.PersonHold = true;
        } else {
            this.formData.PersonHold = false;
            this.formData.PersonHoldBy = "";
        }
        if (metaInfo && metaInfo[PhysicalDefaultColumnIDs.HomeLocation]) {
            var oldHomeLocationColumn = JSON.parse(metaInfo[PhysicalDefaultColumnIDs.HomeLocation]);
            var index = oldHomeLocationColumn.Name.lastIndexOf('/');
            var newHomeLocationValue = index > 0 ? oldHomeLocationColumn.Name.substring(index + 1) : oldHomeLocationColumn.Name;
            let newHomeLocationColumn = {
                Id: oldHomeLocationColumn.Id,
                Name: newHomeLocationValue
            };
            metaInfo[PhysicalDefaultColumnIDs.HomeLocation] = JSON.stringify(newHomeLocationColumn);
        }

        let postData = null;
        let url = null;
        let errorMsg = '';
        let newItemErrorMsg = RMResx.RM_PRM_PRE_Msg_NewItemError;
        let editItemErrorMsg = RMResx.RM_PRM_PRE_Msg_EditItemError;
        let newRequestErrorMsg = RMResx.RM_PRM_PRE_Msg_NewRequestError;
        let editRequestErrorMsg = RMResx.RM_PRM_PRE_Msg_EditRequestError;
        switch (this.formData.formType) {
            case PhyObjFormType.NewRequest:
                postData = this.requestDto;
                var phyObjData = this.formData;
                this.addParentIdListProperty(phyObjData);
                postData.PhysicalFileInfo = phyObjData;
                url = this.getNewRequestUrl(phyObjData);
                errorMsg = newRequestErrorMsg;
                break;
            case PhyObjFormType.EditRequest:
                postData = this.requestDto;
                postData.PhysicalFileInfo = this.formData;
                postData.PhysicalFileInfos = null;
                url = `/api/PhysicalRequestApi/Modify`;
                errorMsg = editRequestErrorMsg;
                break;
            case PhyObjFormType.CreatePhyObj:
                postData = this.formData;
                this.addParentIdListProperty(postData);
                url = `/api/PhysicalRecordApi/AddOrUpdatePhysicalObject`;
                errorMsg = newItemErrorMsg;
                break;
            case PhyObjFormType.EditPhyObj:
                postData = this.formData;
                url = `/api/PhysicalRecordApi/EditPhysicalObject`;
                errorMsg = editItemErrorMsg;
                break;
            default:
                return;
        }
        let option = {
            url:url,
            method :"POST",
            data:postData,
        };
        fetchUtility(option).then((result) =>{
            if (result.success || result.HasError === false) {
                callback(true, this.formData);
                if (this.formData.formType === PhyObjFormType.EditRequest && this.props.loadData) {
                    this.props.loadData();
                }
            } else {
                let tipMsg = result.message || result.ErrorMsg || errorMsg;
                // this.showMessageTip("error", tipMsg);
                this.openErrorMessageBox(tipMsg);
                callback(false, this.formData);
            }
        }).catch((e) =>{
            if(e.status == 403 || e.status == 404) {
                this.handleError(e);
                // callback(false, this.formData);
            }
        });
    }

    openErrorMessageBox(msg) {
        let args = {
            classify: "error",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: msg,
            buttons: [
                { text: RMResx.RM_JS_Common_Close, primary: true, classify: "theme", onClick: this.closeErrorMessageBox },
            ]
        };
        $$.messagedialog(true, args);
    }

    closeErrorMessageBox(){
        $$.messagedialog(false);
    }

    addParentIdListProperty(data) {
        if(this.parentIdList && this.parentIdList.length > 0){
            data.Ancestors = this.parentIdList;
        }
    }

    showSetRecordStatusToDestroyMsgBox(metaInfo, callback) {
        //当设置status的状态为Destroy时，弹出的message;
        $$.loading(false);
        $$.messagedialog(true, {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_PRM_PRE_SetDestroyStatusMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        this.saveColumnsData(metaInfo, callback);
                        $$.loading(true);
                        $$.messagedialog(false);
                    }
                }
            ]
        });
    }

    getNewRequestUrl(data)
    {
        if(data.NodeType == NodeType.PhyBox)
        {
            addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.BoxCreationRequest);
            return `/api/PhysicalRequestApi/NewBoxRequest`;
        }
        else if(data.NodeType == NodeType.PhyFile)
        {
            addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.FolderCreationRequest);
            return `/api/PhysicalRequestApi/NewFolderRequest`;
        }
        else if(data.NodeType == NodeType.PhyRecord)
        {
            addTelemetryRecord(TelemetryModule.PhysicalRecordsExplorer, TelemetryEventType.RecordCreationRequest);
            return `/api/PhysicalRequestApi/NewRecordRequest`;
        }
        return "";
    }

    isDisableColumn(column) {
        return readonlyColumns.indexOf(column.uniqueId) >= 0
            || ((column.inheritFromParent || column.inheritFromParentFolder) && !column.allowModifyValue);
    }


    //Events:
    onSelectedClassChanged(items) {
        this.classificationColumn = this.getClassificationColumn(this.state.data);
        if (items && items.length > 0) {
            let selTerm = items[0];
            this.classificationColumn.columnValue = JSON.stringify({
                Id: selTerm.UniqueId,
                Name: selTerm.Name
            });
            this.setState({
                showDispClassPanel: { show: false },
                data: this.state.data
            });
        }
    }

    onClassSelectorClick() {
        this.setState({
            showDispClassPanel: { show: true }
        });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    onSingleChoiceChange(column, args) {
        let columnValueObj = {
            Name: args.newValue.value,
            Value: args.newValue.key
        };
        column.columnValue = JSON.stringify(columnValueObj);

        if (column.uniqueId == PhysicalDefaultColumnIDs.Status) {
            for (let index = 0; index < this.state.data.length; index++) {
                const category = this.state.data[index];
                for (let index = 0; index < category.columns.length; index++) {
                    const loanedByColumn = category.columns[index];
                    if (loanedByColumn.uniqueId == PhysicalDefaultColumnIDs.LoanedBy) {
                        loanedByColumn.disabled = args.newValue.key == PhysicalObjectStatus.Destroyed || args.newValue.key == PhysicalObjectStatus.Missing;
                        if (loanedByColumn.disabled) {
                            loanedByColumn.columnValue = "";
                        }
                        break;
                    }
                }
                break;
            }
        }
        this.setState({
            data: RM.deepcopy(this.state.data)
        });
    }

    multipleChoiceChange(column, args) {
        let columnValues = [];
        for (let arg of args.newValue) {
            let columnValueObj = {};
            columnValueObj.Name = arg.value;
            columnValueObj.Value = arg.key;
            columnValues.push(columnValueObj);
        }
        column.columnValue = JSON.stringify(columnValues);
        this.setState({
            data: this.state.data
        });
    }

    onSingleTextChange(column, value) {
        column.columnValue = $.trim(value);
        
        if (column.columnName == "RM_PRM_PRE_Column_Barcode") {
            this.setState({ barcodeValue: column.columnValue })  
        }

        this.setState({
            data: this.state.data
        });
    }

    onMultipleTextChange(column, value) {
        column.columnValue = value;
        this.setState({
            data: this.state.data
        });
    }

    onDateTimeChange(column, args) {
        let timezoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        if (args.newValue) {
            let dateStr = RM.TimeUtil.getCommonDateStr(args.newValue);
            let zoneId = timezoneInfo.id;
            let autoAdjustClock = timezoneInfo.autoAdjustClock;
            column.columnValue = JSON.stringify({
                Date: dateStr,
                TimeZoneId: zoneId,
                IsSetDayLight: autoAdjustClock
            });
        } else {
            column.columnValue = null;
        }

        this.setState({
            data: RM.deepcopy(this.state.data)
        });
    }

    onClearCloseDate = () => {
        for (let item of this.state.data) {
            for (let inItem of item.columns) {
                if (inItem.uniqueId == PhysicalDefaultColumnIDs.DataClosed) {
                    inItem.columnValue = null;
                    break;
                }
            }
        }
        this.setState({
            data: RM.deepcopy(this.state.data)
        });
    }

    onNumberChange(column, value) {
        column.columnValue = value;
        this.setState({
            data: this.state.data
        });
    }

    onPeopleSelectionChanged(column, users) {
        let newVal = null;
        if (users && users.length > 0) {
            let selUsers = users.filter(user => user.Checked);
            newVal = JSON.stringify(selUsers);
        }
        column.columnValue = newVal;
        this.setState({
            data: this.state.data
        });
    }

    onReuestCommentChange(value) {
        this.requestDto.Comment = value;
        this.setState({
            data: this.state.data
        });
    }

    getValidContent(column) {
        // if (column.typeId) {
        //     if (column.typeId == PhysicalObjectColumnType.SingleText || column.typeId == PhysicalObjectColumnType.MutipleText) {
        //         if (column.columnValue && column.columnValue.length > 255) {
        //             return RMResx.RM_JS_Common_Msg_CannotExceed255;
        //         }
        //     }
        // }
        if (column.columnName == "RM_PRM_PRE_Column_Barcode" && this.state.barcodeValue) {
            if (!RegexUtil.IsMath(this.state.barcodeValue)) {
                return RMResx.RM_PRM_Barcode_Invalid_Message;
            }
            
            if (this.state.barcodeValue.length > 26) {
                return RMResx.RM_PRM_Barcode_TooLong_Message;
            }
        }

        if (column.required && !column.columnValue) {
            switch (column.typeId) {
                case PhysicalObjectColumnType.SingleText:
                case PhysicalObjectColumnType.MutipleText:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireText;
                case PhysicalObjectColumnType.Number:
                    if (column.uniqueId == PhysicalDefaultColumnIDs.Capability) {
                        return RMResx.RM_PRM_PRE_ColumnValid_RequireNumber;
                    } else {
                        return RMResx.RM_PRM_PRE_ColumnValid_RequireText;
                    }
                case PhysicalObjectColumnType.DateTime:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireDateTime;
                case PhysicalObjectColumnType.SingleChoice:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireSingleChoice;
                case PhysicalObjectColumnType.PeopleOrGroup:
                    return RMResx.RM_JS_CP_AM_AddUser_Nomatch;
                case PhysicalObjectColumnType.MultipleChoice:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireMultipleChoice;
                case PhysicalObjectColumnType.Taxonomy:
                    return RMResx.RM_PRM_PRE_ColumnValid_RequireTreeNode;
            }
        }
    }

    initPermissionInfo() {
        let parentNodeInfo = this.props.parentNodeInfo;
        if (
            parentNodeInfo.NodeType == NodeType.PhysicalBottomLocation ||
            parentNodeInfo.NodeType == NodeType.PhyBox ||
            parentNodeInfo.NodeType == NodeType.PhyFile
        ) {
            $$.loading(true);
            let option = {
                url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${parentNodeInfo.Id}&includeSelf=${true}`,
                method: "GET",
            };
            fetchUtility(option).then((result) => {
                let res = JSON.parse(result);
                for (let item of res.Accounts) {
                    item.Checked = true;
                }
                this.setState({
                    selectedPermissionUsers: res.Accounts,
                    isBreakInheritance: false
                });
                this.superiorIsBreakInheritance = res.BreakInheritStatus;
                this.inheritanceuserList = RM.deepcopy(res.Accounts);
                $$.loading(false);
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    inheritanceClick() {
        this.setState({
            selectedUsersValid: false,
            isBreakInheritance: false,
            selectedPermissionUsers: RM.deepcopy(this.inheritanceuserList)
        });
    }

    breakInheritance() {
        this.setState({
            isBreakInheritance: true,
            selectedPermissionUsers: RM.deepcopy(this.inheritanceuserList)
        });
    }

    onAccessPermissionChanged(args) {
        if (this.state.isBreakInheritance) {
            this.setState({ selectedPermissionUsers: args });
        }
    }

    handleError(response) {
        $$.loading(false);
        if (response.status == 403) {
            $$.messagedialog(true, {
                classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_Common_NoPermissionLicense,
                buttons: [
                    {   
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
        } else if (response.status == 404)
        {
            $$.messagedialog(true, {
                classify: "warn",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_NotPermission_CurrentTermDifferentScope,
                buttons: [
                    {   
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
        }
    }

    //render contents:
    renderSingleTextColumn(column) {
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        return (
            <R.Input
                type="text"
                value={column.columnValue}
                width={300}
                disabled={this.isDisableColumn(column)}
                onChange={this.onSingleTextChange.bind(this, column)}
                aria={{ariaLabel:columnName}}
            />
        );
    }

    renderMutipleTextColumn(column) {
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        return (
            <R.Input
                type="textarea"
                value={column.columnValue}
                width={300}
                disabled={this.isDisableColumn(column)}
                onChange={this.onMultipleTextChange.bind(this, column)}
                aria={{ariaLabel:columnName}}
            />
        );
    }

    renderNumberColumn(column) {
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        let props = {};
        if (column.uniqueId == PhysicalDefaultColumnIDs.Capability) {
            props.min = 0.01;
        }
        return (
            <div>
                <R.Input
                    {...props}
                    type="number"
                    hasControl
                    value={column.columnValue}
                    width={300}
                    float={2}
                    fixFloat={false}
                    disabled={this.isDisableColumn(column)}
                    onChange={this.onNumberChange.bind(this, column)}
                    aria={{ariaLabel:columnName}}
                />
            </div>
        );
    }

    renderDateTimeColumn(column) {
        let selDate = null;
        let isShowClearBtn = column.uniqueId == PhysicalDefaultColumnIDs.DataClosed;
        if (column.columnValue) {
            let dt = JSON.parse(column.columnValue);
            selDate = new Date(dt.Date);
        }

        return (<React.Fragment>
            <R.Datepicker
                selectedDate={selDate}
                data-part="vtWidget"
                width={300}
                disabled={this.isDisableColumn(column)}
                dateTimeFormat={this.defaultDateFormat}
                // hasTimeZone={true}
                hasTimePicker={true}
                // selectedTimeZone={timeZoneInfo}
                onChange={this.onDateTimeChange.bind(this, column)}
                triggerBySource={true}
                todayClick={this.todayClick}
            />
            {
                isShowClearBtn && selDate && <a  className="ra-link-a margin-s" onClick={this.onClearCloseDate}>{RMResx.RM_Common_Clear}</a>
            }

        </React.Fragment>
        );
    }

    renderSingleChoiceColumn(column) {
        let optionsObj = JSON.parse(column.optionsJSON);
        let options = [];
        let selId = null;
        if (column.columnValue) {
            selId = JSON.parse(column.columnValue).Value;
        } else if (optionsObj) {
            let optionsObjKeyArr = Object.keys(optionsObj);
            if (optionsObjKeyArr.length > 0) {
                selId = optionsObjKeyArr[0];
                let columnValueObj = {};
                columnValueObj.Value = selId;
                columnValueObj.Name = optionsObj[selId];
                column.columnValue = JSON.stringify(columnValueObj);
            }
        }

        if (optionsObj) {
            for (const oId in optionsObj) {
                if (optionsObj.hasOwnProperty(oId)) {
                    let opValue = optionsObj[oId];
                    options.push({
                        key: oId,
                        value: opValue,
                        checked: oId === selId,
                        tooltip: opValue,
                    });
                }
            }
        }
        return (
            <R.Combobox
                checkedField="checked"
                searchable={false}
                textField="value"
                valueField="key"
                tooltipField="tooltip"
                width={300}
                disabled={this.isDisableColumn(column)}
                items={options}
                onChange={this.onSingleChoiceChange.bind(this, column)}
            />
        );
    }

    renderMultipleChoiceColumn(column) {
        let optionsObj = JSON.parse(column.optionsJSON);
        let options = [];
        let selectedValues = [];
        if (column.columnValue) {
            for (let selectedOption of JSON.parse(column.columnValue)) {
                selectedValues.push(selectedOption.Value);
            }
        }
        for (const oId in optionsObj) {
            if (optionsObj.hasOwnProperty(oId)) {
                let opValue = optionsObj[oId];
                options.push({
                    key: oId,
                    value: opValue,
                    checked: selectedValues.indexOf(oId) != -1,
                    tooltip: opValue
                });
            }
        }

        return (
            <div>
                <R.Multicombobox
                    items={options}
                    width={300}
                    textField='value'
                    valueField='key'
                    tooltipField="tooltip"
                    disabled={this.isDisableColumn(column)}
                    onChange={this.multipleChoiceChange.bind(this, column)}
                />
            </div>
        );
    }

    renderPeopleOrGroupColumn(column) {
        let isLoanByColumn = column.uniqueId == PhysicalDefaultColumnIDs.LoanedBy;
        let users = [];
        if (column.columnValue) {
            users = JSON.parse(column.columnValue);
        }
        return (
            <div>
                <PeoplePicker
                    items={users}
                    singleMode={isLoanByColumn}
                    disabled={column.disabled || this.isDisableColumn(column)}
                    selectionChanged={this.onPeopleSelectionChanged.bind(this, column)}
                />
            </div>
        );
    }

    renderTaxonomyColumn(column) {
        let termName = "";
        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
        if (column.columnValue) {
            termName = JSON.parse(column.columnValue).Name;
        }
        if (column.uniqueId == PhysicalDefaultColumnIDs.HomeLocation) {
            if (!termName) {
                termName = this.formData.HomeLocationFullPath || this.formData.LocationName;
                column.columnValue = JSON.stringify({
                    Id: this.formData.LocationId,
                    Name: termName
                });
            }
            return <R.Input
                type="text"
                value={termName}
                width={300}
                disabled={true}
                aria={{ariaLabel:columnName}}
            // onChange={this.onHomeLocationChange}
            />;
        } else if (column.uniqueId == PhysicalDefaultColumnIDs.Classification) {
            return <div className={"class-selector"} onClick={this.onClassSelectorClick} tabIndex="0" onKeyDown={this.onKeyDown}>
                <div className="class-selector-value text-overflow">{termName}</div>
                <div className="class-selector-icon fia-gear"></div>
            </div>;
        }
    }

    renderColumn(column) {
        switch (column.typeId) {
            case PhysicalObjectColumnType.SingleText:
                return this.renderSingleTextColumn(column);
            case PhysicalObjectColumnType.MutipleText:
                return this.renderMutipleTextColumn(column);
            case PhysicalObjectColumnType.Number:
                return this.renderNumberColumn(column);
            case PhysicalObjectColumnType.DateTime:
                return this.renderDateTimeColumn(column);
            case PhysicalObjectColumnType.SingleChoice:
                return this.renderSingleChoiceColumn(column);
            case PhysicalObjectColumnType.PeopleOrGroup:
                return this.renderPeopleOrGroupColumn(column);
            case PhysicalObjectColumnType.MultipleChoice:
                return this.renderMultipleChoiceColumn(column);
            case PhysicalObjectColumnType.Taxonomy:
                return this.renderTaxonomyColumn(column);
        }
    }

    renderNewRequestComment() {
        let isEditReq = this.formData.formType == PhyObjFormType.EditRequest;
        if (this.formData.formType == PhyObjFormType.NewRequest || isEditReq) {
            let comment = isEditReq && this.requestDto ? this.requestDto.Comment : "";
            return <React.Fragment>
                <div className={"template_item_document_title margin-top-20"}>
                    {RMResx.RM_PRM_PRE_NewRequest_Comment}
                </div>
                <R.Input
                    type="textarea"
                    value={comment}
                    width={300}
                    onChange={this.onReuestCommentChange}
                    aria={{ariaLabel:RMResx.RM_PRM_PRE_NewRequest_Comment}}
                />
                {/*<$g.ValidationMsg show={this.state.isSaving && (this.requestDto.Comment && this.requestDto.Comment.length > 255)}>*/}
                {/*    {RMResx.RM_JS_Common_Msg_CannotExceed255}*/}
                {/*</$g.ValidationMsg>*/}
            </React.Fragment>;
        }
    }

    renderValidationMsg(column) {
        return <div>
            <$g.ValidationMsg show={this.state.isSaving}>
                {this.getValidContent(column)}
            </$g.ValidationMsg>
        </div>;
    }

    renderDisposalClassPanel() {
        let renderClassPanel = this.renderClassPanel;
        this.renderClassPanel = true;
        return renderClassPanel && <R.Panel
            id="selectDispClassPanel"
            header={RMResx.RM_Template_Column_Name_Classification}
            size={600}
            actionType="back"
            status={this.state.showDispClassPanel}
            destroy={true}
        >
            <div className="ra-panel-content">
                <TermTree
                    rootItem={this.state.dispClassRoot}
                    onSelectedNodeChanged={this.onSelectedClassChanged}
                    sourceFlag={SourceFlags.Phy}
                    containerId={this.formData.LocationId}
                    // forPhysicalView={"true"}
                />
            </div>
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => {
                this.setState({ showDispClassPanel: { show: false } });
            }} />
        </R.Panel>;
    }

    renderPermissionInfo() {
        let isBreakInheritance = this.state.isBreakInheritance;
        let isAllHasPermission = !this.state.isBreakInheritance && this.state.selectedPermissionUsers.length == 0 && !this.superiorIsBreakInheritance;
        let permissionEnterUserTitle = isBreakInheritance ? RMResx.RM_PRM_PRE_NewItemPermissionEnterUserTitle : '';
        let permissionIntroduce = isBreakInheritance ?
            RMResx.RM_PRM_PRE_BreakInheritance_PermissIntroForBoxsAndFolders : RMResx.RM_PRM_PRE_PermissionIntroduceForBoxsAndFolders;
        if (this.formData.formType != PhyObjFormType.EditPhyObj) {
            if (this.formData.NodeType == NodeType.PhyFile || this.formData.NodeType == NodeType.PhyBox || this.formData.NodeType == NodeType.PhyCustom) {
                return <div className='ra-permissionInfo'>
                    <div className="ra-section-head" tabIndex='0'>{RMResx.RM_PRM_PRE_PermissionTitle}</div>
                    <div className="permission-introduce" tabIndex='0'>{permissionIntroduce}</div>
                    <div>
                        {
                            !isBreakInheritance &&
                            <div className="permission-inheritance-btn">
                                <R.Button
                                    type="link"
                                    text={RMResx.RM_PRM_PRE_BreakInheriteance}
                                    onClick={this.breakInheritance}
                                    icon='fia-lock-open' />
                            </div>
                        }
                        {
                            isBreakInheritance &&
                            <div className="permission-inheritance-btn">
                                <R.Button
                                    type="link"
                                    text={RMResx.RM_PRM_PRE_Inheritance}
                                    icon='fia-lock'
                                    onClick={this.inheritanceClick} />

                            </div>
                        }
                        {
                            isAllHasPermission &&
                            <div className='inherit-parent-text' tabIndex='0'>{RMResx.RM_PRM_PRE_AllUsersHasPermission}</div>
                        }
                        {
                            !isAllHasPermission &&
                            <div>
                                <$g.FormRow
                                    label={permissionEnterUserTitle}
                                    require={false}
                                >
                                    <PeoplePicker
                                        items={this.state.selectedPermissionUsers}
                                        selectionChanged={this.onAccessPermissionChanged}
                                        disabled={!this.state.isBreakInheritance}
                                    />
                                    <$g.ValidationMsg show={this.state.selectedUsersValid}>
                                        {RMResx.RM_PRM_PRE_PermissionNotEnterUserValid}
                                    </$g.ValidationMsg>
                                </$g.FormRow>
                            </div>
                        }
                    </div>
                </div>;
            }
        }
    }

    render() {
        let showForm = this.state.hasClassifySetting && this.state.data;
        return (
            <div id={this.props.id} className="phyobj-form">
                <R.Messagebar
                    message={this.state.tipMsg} classify={this.state.tipType}
                    onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
                {showForm && <div>
                    {this.state.showMsgBar && this.props.showMsgBar && <div style={{ marginBottom: "8px" }}>
                        <R.Messagebar
                            classify="info"
                            message={RMResx.RM_PRM_PRE_Msg_MsgBar}
                            status={{ show: true }}
                            hasClose={true}
                        />
                    </div>}
                    {
                        this.state.data.map((item, categoryIndex) => {
                            let categoryName = RMResx[item.name] ? RMResx[item.name] : item.name;
                            return (
                                <div key={categoryIndex} className="phyobj-category">
                                    <div className="ra-section-head">{categoryName}</div>
                                    {item.columns.map((column, index) => {
                                        let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
                                        return (
                                            <$g.FormRow
                                                key={index}
                                                label={columnName}
                                                require={column.required}
                                            >   
                                                <div className="ra-phyobj-column">
                                                    {this.renderColumn(column)}
                                                    {this.renderValidationMsg(column)}
                                                </div>
                                            </$g.FormRow>
                                        );
                                    })}
                                </div>
                            );
                        })
                    }
                    {this.renderPermissionInfo()}
                </div>
                }
                {showForm && this.renderNewRequestComment()}
                {this.renderDisposalClassPanel()}
            </div>

        );
    }
}
