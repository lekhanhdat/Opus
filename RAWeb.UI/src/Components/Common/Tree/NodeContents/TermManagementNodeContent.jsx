import { Link } from 'react-router-dom';
import { bindEvents, isShowActionByDC, showToast } from '../../../../Utilities/CommonUtil';
import { NodeType } from '../../../../Constants/DAEnums';
import RouterUrls from '../../../../Constants/RouterUrls';
import { TreeActionsPopup, TreeActionItem } from '../Components/TreeActions';
import {TemplateTreeNodeType, StartFromType, UITemplateTreeNodeType, TemplateTypes} from '../../../PRM/Constants';
import { checkPermission } from '../../../../Utilities/permissionManager';

const isMultiGeoMainDC = isShowActionByDC();
export default class TermManagementNodeContent extends React.Component {
    constructor(props) {
        super(props);
        let isCreating = !props.item.nodeKey;
        this.state = {
            isSelected: false,
            isEditing: isCreating,
            isCreating: isCreating,
            item: props.item
        };
        this.nodeClickTimer = null;
        this.treeContext = props.treeContext;
        bindEvents(this, "onNodeDoubleClick",
            "onActionBlur", "onEditInputKeyDown",
            "onCreateItemClick", "onRenameClick", "onDeleteItemClick",
            "onRetireItemClick", "onActiveItemClick", "onEditInputBlur",
            "onCreateSuiteItemClick", "onEditSuiteClick", "onEditTemplateClick",
            "onCreateTemplateClick", "onAddExistingTemplateClick", "onRefreshActionClick", 
            "refreshOperationNode", "updateSuiteItem", "updateTemplateItem",
            "ifChangeSelectedNode");
    }

    componentDidMount() {
        if (this.state.isCreating) {
            this.nodeNameInput.focus();
        }
    }

    componentDidUpdate(prevProps, prevState) {
        if (!prevState.isEditing && this.state.isEditing) {
            $(this.nodeNameInput).select();
        }
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.item != this.props.item) {
            this.setState({ item: nextProps.item });
        }
    }

    isUnsafeNodeName(name) {
        //Name cannot contain any of the following characters:";<>|and Tab.
        let filterString,
            treeType = this.treeContext.treeType;
        switch (treeType) {
            case 2:
                filterString = "~;<>|#%*:?/{}\\&\"";
                break;
            default:
                filterString = ";<>|";
                break;
        }

        let ch,
            i,
            temp,
            error = false;
        for (i = 0; i <= (filterString.length - 1); i++) {
            ch = filterString.charAt(i);
            temp = name.indexOf(ch);
            if (temp != -1) {
                error = true;
                break;
            }
        }
        return error;
    }

    isTermNameLenGt255(termName) {
        if (termName != '') {
            return termName.length > 255;
        }
        return false;
    }

    isValidateNodeName(nodeName) {
        if (!nodeName) return true;
        try {
            const xmlString = `<${nodeName}></${nodeName}>`;
            const parser = new DOMParser();
            const doc = parser.parseFromString(xmlString, "application/xml");
            const parserError = doc.getElementsByTagName("parsererror")[0];
            if (parserError) {
                return false;
            }
            return true;
        } catch (error) {
            console.error("Parse node name error!");
            return false;
        }
    }

    replaceSpecialSymbol(name) {
        var reg1 = new RegExp("&", "ig");
        var reg2 = new RegExp('"', "ig");
        name = name.replace(reg1, "＆");
        name = name.replace(reg2, '＂');
        return name;
    }

    repalceUnSafeChar(sHtml) {
        return sHtml.replace(/[;<>|]/g, function (c) {
            return { ';': '', "<": '', ">": '', "|": '', }[c];
        });
    }

    getExportSettingIconClass() {
        return "fia-term-group ra-verticalAlign-middle";
    }

    getNodeIconClass(item) {
        let nodeType = item.nodeType;
        let iconsStatus = this.getIconStatus(item);
        if(this.treeContext.treeType == 3)
        {
            return this.getTemplateTreeNodeIconClass(item);
        }
        if(this.treeContext.treeType == 2) {
            let oitem = item.origin;
            if(oitem){
                switch (oitem.NodeType) {
                    case NodeType.PhysicalRootLocation:
                        nodeType = "RootLocation";
                        break;
                    case NodeType.PhysicalBottomLocation:
                        nodeType = "MininumLocation";
                        break;
                    default:
                        nodeType = "NormalLocation";
                        break;
                }
            }
        }

        if(this.treeContext.treeType == 5){
            return this.getExportSettingIconClass();
        }

        switch (nodeType) {
            case 'Root':
            case 'TermGroup':
                return 'ra-tree-icon fia-term-group';
            case 'TermSet':
                return 'ra-tree-icon fia-term-set';
            case 'Term': {
                let iconclass = 'ra-tree-icon fia-term';
                if (item.origin) {
                    if (item.origin.IsDeprecated) {
                        iconclass += "-retired-b";
                    } else if (item.origin.IsExpired) {
                        iconclass += "-retired-b";
                    }
                }
                return iconclass;
            }
            case 'RootLocation':
                return 'ra-tree-node-icon fia-physical-record';
            case 'NormalLocation':
                return `ra-tree-node-icon fia-location${iconsStatus}`;
            case 'MininumLocation':
                return `ra-tree-node-icon fia-room${iconsStatus}`;
            case 'RuleContainerRoot':
                return 'ra-tree-node-icon fia-root-node';
            case 'RuleContainer':
                return 'ra-tree-node-icon fia-container';
            default:
                return '';
        }
    }

    getIconStatus(node) {
        switch (node.iconStatus) {
            case 1:
                return "-inherit-b";
            case 2:
                return "-unique-c";
            default:
                return "";
        }
    }

    getTemplateTreeNodeIconClass(item)
    {
        let oItem = item.origin;
        let uiNodeType = this.getUITemplateNodeType(oItem);
        let iconClassMap = {
            [UITemplateTreeNodeType.Root]: "ra-tree-node-icon fia-root-node",
            [UITemplateTreeNodeType.BoxSuite]: "ra-tree-node-icon fia-box-suite",
            [UITemplateTreeNodeType.FolderSuite]: "ra-tree-node-icon fia-folder",
            [UITemplateTreeNodeType.CustomSuite]: "ra-tree-node-icon fia-container",
            [UITemplateTreeNodeType.CustomTemplate]: "ra-tree-node-icon fia-container",
            [UITemplateTreeNodeType.BoxTemplate]: "ra-tree-node-icon fia-box-template",
            [UITemplateTreeNodeType.FolderTemplate]: "ra-tree-node-icon fia-folder-template",
            [UITemplateTreeNodeType.RecordTemplate]: "ra-tree-node-icon fia-records-template"
        };
        return iconClassMap[uiNodeType] || "";
    }

    getUITemplateNodeType(oItem) {
        if(oItem == undefined)
        {
            return -1;
        }
        let itemNodeType = oItem.Type;
        if(itemNodeType ==  TemplateTreeNodeType.Root)
        {
            return UITemplateTreeNodeType.Root;
        }
        if(itemNodeType == TemplateTreeNodeType.Suite && oItem.StartFromType == StartFromType.Box)
        {
            return UITemplateTreeNodeType.BoxSuite;
        }
        if(itemNodeType == TemplateTreeNodeType.Suite && oItem.StartFromType == StartFromType.Folder)
        {
            return UITemplateTreeNodeType.FolderSuite;
        }
        if(itemNodeType == TemplateTreeNodeType.Suite && oItem.StartFromType == StartFromType.CustomTemplate)
        {
            return UITemplateTreeNodeType.CustomSuite;
        }
        if(itemNodeType ==  TemplateTreeNodeType.Box)
        {
            return UITemplateTreeNodeType.BoxTemplate;
        }
        if(itemNodeType ==  TemplateTreeNodeType.Folder)
        {
            return UITemplateTreeNodeType.FolderTemplate;
        }
        if(itemNodeType ==  TemplateTreeNodeType.Records)
        {
            return UITemplateTreeNodeType.RecordTemplate;
        }
        if(itemNodeType == TemplateTreeNodeType.Custom)
        {
            return UITemplateTreeNodeType.CustomTemplate;
        }
    }

    isDefaultSuiteNode(oItem) {
        if(oItem && this.treeContext.isDefaultSuite)
        {
            return this.treeContext.isDefaultSuite(oItem.UniqueId);
        }
        return false;
    }

    isDefaultTemplateNode(oItem) {
        if(oItem && this.treeContext.isDefaultTemplate)
        {
            return this.treeContext.isDefaultTemplate(oItem.UniqueId);
        }
        return false;
    }

    getNodeTextClass(item) {
        switch (item.nodeType) {
            case 'Term':
            case 'NormalLocation':
            case 'MininumLocation':
                return 'ra-tree-tm-termText';
            case 'Root':
            case 'TermGroup':
            case 'TermSet':
            case 'RootLocation':
            default:
                return '';
        }
    }

    getChildNodeType(pType) {
        switch (pType) {
            case 'Root':
                return 'TermGroup';
            case 'TermGroup':
                return 'TermSet';
            case 'TermSet':
            case 'Term':
                return 'Term';
            case 'RootLocation':
            case 'NormalLocation':
                return 'NormalLocation';
            case 'RuleContainerRoot':
                return 'RuleContainer';
            default:
                return '';
        }
    }

    createExportSettingItem  = () => {
        let newVal = $.trim(this.nodeNameInput.value);
        if (newVal.length == 0) {
            this.props.parentItemComponent.removeEmptyNode();
            return;
        } else {
            newVal = this.replaceSpecialSymbol(newVal);
        }
       
        setTimeout(() => {
            this.props.parentItemComponent.removeEmptyNode();
            let newItem = this.treeContext.getNewNode(this.props.parentItem, newVal);
            this.props.parentItemComponent.appendNodeItem(newItem);
            this.treeContext.refreshSelectedNodeInfo("add", newItem.origin, this.props.parentItem.origin);
        }, 100);
    }

    createItem = () => {
        if(this.treeContext.treeType == 5){
            this.createExportSettingItem();
            return;
        }

        let self = this;
        let item = self.props.item;
        let pItem = self.props.parentItem;
        let newVal = $.trim(self.nodeNameInput.value);
        if (newVal.length == 0) {
            self.props.parentItemComponent.removeEmptyNode();
            return;
        } else {
            newVal = self.replaceSpecialSymbol(newVal);
        }

        let termObj = {};
        let locationObj = {};
        let dataObj = null;
        let ajaxUrl, errorMsg;
        let isUpdateTermGroupList = false;
        switch (item.nodeType) {
            case "Term":
                ajaxUrl = "/api/TermManagementApi/CreateTerm";
                dataObj = termObj;
                if (pItem.nodeType == "TermSet") {
                    termObj.TermSetId = pItem.origin.Id;
                    termObj.ParentTermId = 0;
                } else {
                    termObj.TermSetId = pItem.origin.TermSetId;
                    termObj.ParentTermId = this.props.parentItem.nodeKey;
                }
                termObj.TermName = newVal;
                errorMsg = RMResx.RM_JS_TM_TermSameNameErrorMsg;
                break;
            case "TermSet":
                ajaxUrl = "/api/TermManagementApi/CreateTermSet";
                dataObj = termObj;
                errorMsg = RMResx.RM_JS_TM_TermSetSameNameErrorMsg;
                termObj.TermGroupUniqueId = pItem.nodeKey;
                termObj.TermSetName = newVal;
                break;
            case "TermGroup":
                ajaxUrl = "/api/TermManagementApi/CreateTermGroup";
                dataObj = termObj;
                termObj.TermGroupName = newVal;
                errorMsg = RMResx.RM_JS_TM_TermGroupSameNameErrorMsg;
                isUpdateTermGroupList = true;
                break;
            case "RootLocation":
            case "NormalLocation":
                ajaxUrl = "/api/LocationManagementApi/CreateLocation";
                dataObj = locationObj;
                locationObj.Name = newVal;
                locationObj.ParentId = this.props.parentItem.nodeKey;
                errorMsg = RMResx.RM_JS_LM_LocationSameNameErrorMsg;
                break;
            case "RuleContainer":
                ajaxUrl = "/api/RuleApi/SaveRuleContainer";
                dataObj = { Name: newVal };
                errorMsg = RMResx.RM_RDM_RuleContainer_SameNameErrorMsg;//TODO Cyrus
                break;
        }
        $.ajax({
            type: "POST",
            url: ajaxUrl,
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(dataObj),
            async: true,
            beforeSend: () => {
                this.props.itemComponent.loading(true);
            },
            success: function (data) {
                self.props.parentItemComponent.removeEmptyNode();
                if (isUpdateTermGroupList) {
                    self.treeContext.updateTermGroupList();
                }

                let nodeType = self.props.item.nodeType;
                if (data == "" || data == "1") {
                    showToast.error(errorMsg);
                } else if (data == "0" && nodeType == "TermSet") {
                    showToast.error(RMResx.RM_JS_TM_CanOnlyCreateATermSetMsg);
                } else if(data == "-1" || data == "-2") {
                    showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                }
                else {
                    let newitem = self.treeContext.transToTreeNodeObject($.parseJSON(data));
                    // Fortify Issue Type: JSON Injection; Sink Details: tree node; Ignore Reason: 前后台对象存在对应关系
                    self.props.parentItemComponent.appendNodeItem(newitem);
                    if (self.continueCreate) {
                        self.props.parentItemComponent.appendNodeItem({ nodeType: nodeType });
                    }
                }
            },
            error: function (msg) {
                self.props.parentItemComponent.removeEmptyNode();
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    renameItem() {
        let self = this;
        let item = self.props.item;
        let oItem = item.origin;
        let newVal = $.trim(self.nodeNameInput.value);
        if (newVal.length == 0 || oItem.Name == newVal) {
            self.setState({ isEditing: false });
            return;
        } else {
            newVal = self.replaceSpecialSymbol(newVal);
        }

        let termObj = { TermId: oItem.Id, TermName: newVal };
        let locationObj = { LocationId: oItem.Id, Name: newVal };
        let ruleContainerObj = { ContainerId: oItem.ContainerId, Name: newVal };
        let dataObj = null;
        let ajaxUrl = "/api/TermManagementApi/RenameTerm";
        let errorMsg = '';
        let isUpdateTermGroupList = false;
        switch (item.nodeType) {
            case "Term":
                ajaxUrl = "/api/TermManagementApi/RenameTerm";
                dataObj = termObj;
                dataObj.TermSetId = oItem.TermSetId;
                errorMsg = RMResx.RM_JS_TM_TermSameNameErrorMsg;
                break;
            case "TermSet":
                ajaxUrl = "/api/TermManagementApi/RenameTermSet";
                dataObj = termObj;
                errorMsg = RMResx.RM_JS_TM_TermSetSameNameErrorMsg;
                dataObj.TermGroupUniqueId = oItem.TermGroupId;
                break;
            case "TermGroup":
                ajaxUrl = "/api/TermManagementApi/RenameTermGroup";
                dataObj = termObj;
                errorMsg = RMResx.RM_JS_TM_TermGroupSameNameErrorMsg;
                isUpdateTermGroupList = true;
                break;
            case "RootLocation":
                ajaxUrl = "/api/LocationManagementApi/RenameRootLocation";
                dataObj = locationObj;
                errorMsg = "";
                break;
            case "MininumLocation":
            case "NormalLocation":
                ajaxUrl = "/api/LocationManagementApi/RenameNormalLocation";
                dataObj = locationObj;
                errorMsg = RMResx.RM_JS_LM_LocationSameNameErrorMsg;
                break;
            case "RuleContainer":
                ajaxUrl = "/api/RuleApi/SaveRuleContainer";
                dataObj = ruleContainerObj;
                errorMsg = RMResx.RM_RDM_RuleContainer_SameNameErrorMsg;//TODO Cyrus
                break;
        }
        $.ajax({
            type: "POST",
            url: ajaxUrl,
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(dataObj),
            async: true,
            beforeSend: () => {
                this.props.itemComponent.loading(true);
            },
            complete: () => {
                this.props.itemComponent.loading(false);
            },
            success: function (data) {
                let showErrorMsg = false;
                if (isUpdateTermGroupList) {
                    self.treeContext.updateTermGroupList();
                }

                if (item.nodeType == "RuleContainer") {
                    if (data == "" || data == null) {
                        showErrorMsg = true;
                    } else if (data == "-2") {
                        showErrorMsg = true;
                        errorMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                    }
                } else {
                    let newItemMsg = $.parseJSON(data); // Fortify Issue Type: JSON Injection; Sink Details: rename; Ignore Reason: 前后台对象存在对应关系
                    if (newItemMsg.message == "-1") {
                        showErrorMsg = true;
                    }else if (newItemMsg.message == "-2") {
                        showErrorMsg = true;
                        errorMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                    }
                }

                if (showErrorMsg) {
                    showToast.error(errorMsg);
                    self.setState({
                        isEditing: false
                    });
                } else {
                    var newItem = $.parseJSON(data);    // Fortify Issue Type: JSON Injection; Sink Details: rename; Ignore Reason: 前后台对象存在对应关系
                    newItem = Object.assign(self.props.item.origin, newItem);
                    newItem = self.treeContext.transToTreeNodeObject(newItem);
                    self.setState({
                        isEditing: false,
                        item: newItem
                    });
                    self.treeContext.refreshSelectedNodeInfo(newItem.origin, 1);
                }
            },
            error: function (msg) {
                //alert(msg.responseText);
            },
            dataType: "json"
        });
    }

    // getRealPostURL(ajaxUrl) {
    //     if (this.treeContext.treeType == 2) {
    //         ajaxUrl = ajaxUrl.replace("TermManagementApi", "LocationManagementApi");
    //     }
    //     return ajaxUrl;
    // }

    hasActivationSetting() {
        let oitem = this.state.item.origin;
        if (oitem.TermExpirationFrom || oitem.TermExpirationTo || oitem.TimeZoneId) {
            return true;
        }
        return false;
    }

    confirmRetireOrActiveTerm(doFunc) {
        $$.messagedialog(true, {
            classify: "info",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_TM_RerireTermMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                        doFunc();
                        $$.messagedialog(false);
                    }
                }
            ],
        });
    }

    resetSelectedNode() {
        if (this.treeContext.selectNodeContent && this.treeContext.selectNodeContent != this) {
            this.treeContext.selectNodeContent.clearSelectedStatus();
        }
        this.treeContext.selectNodeContent = this;
    }
  
    onNodeDoubleClick(e) {
        if(!this.state.item.isAllowEditName){return;}
        clearTimeout(this.nodeClickTimer);
        let oitem = this.props.item.origin;
        if (!this.state.isEditing
            && !(oitem.Type == 'Root' || !oitem || oitem.IsDeprecated || oitem.IsExpired)) {
            this.onRenameClick();
        }
        e.stopPropagation();
    }

    onCreateItemClick(e) {
        let creatingItem = { nodeType: this.getChildNodeType(this.props.item.nodeType) };
        this.props.itemComponent.appendNodeItem(creatingItem);
        e.stopPropagation();
    }

    onCreateSuiteItemClick() {
        let oitem = this.props.item.origin;
        this.treeContext.createSuiteItem(oitem, () => { this.refreshOperationNode();});
    }

    onImportSuiteItemsClick() {
        this.treeContext.importSuiteItems();
    }

    onEditSuiteClick() {
        let oItem = this.props.item.origin;
        this.treeContext.editSuiteItem(oItem, (oNewItem) => {this.updateSuiteItem(oNewItem);});
    }

    onCreateTemplateClick(newTemplateType) {
        let oitem = this.props.item.origin;
        this.treeContext.createTemplateItem(oitem, newTemplateType, () =>{ this.refreshOperationNode();});
    }
    
    updateSuiteItem(oNewItem) {
        let self = this;
        let newItem = Object.assign(self.props.item.origin, oNewItem);
        newItem = self.treeContext.transToTreeNodeObject(newItem);
        self.setState({item: newItem});
    }

    updateTemplateItem(oNewItem) {
        let self = this;
        let newItem = Object.assign(self.props.item.origin, {Name: oNewItem.name});
        newItem = self.treeContext.transToTreeNodeObject(newItem);
        self.setState({item: newItem});
    }

    refreshOperationNode() {
        this.props.item.loaded = false;
        this.props.itemComponent.reload(0);
    }

    onAddExistingTemplateClick() {
        let oitem = this.props.item.origin;
        this.treeContext.addExistingTemplateItem(oitem, () =>{ this.refreshOperationNode();});
    }

    onEditTemplateClick() {
        let oItem = this.props.item.origin;
        this.treeContext.editTemplateItem(oItem, (oNewItem) => {this.updateTemplateItem(oNewItem);});
    }

    onRefreshActionClick(e) {
        this.props.item.loaded = false;
        this.props.itemComponent.loadNodes(0, this.props.item.pagerSize,(success) => {
            this.treeContext.onNodeRefresh();
        });
        e.stopPropagation();
    }


    onRenameClick(e) {
        this.setState({
            isEditing: true
        });
    }

    onDeleteExportSettingItemClick(e){
        this.props.parentItemComponent.removeChildrenNodeItem(this.props.item);
        this.treeContext.refreshSelectedNodeInfo("delete", this.props.item.origin);
    }

    onDeleteItemClick = (e) => {
        if(this.treeContext.treeType == 5){
            this.onDeleteExportSettingItemClick();
            return;
        }
        let self = this;
        let item = this.props.item;
        let ajaxUrl, confirmMsg, nodeKey, delSuccessMsg;
        let isUpdateTermGroupList = false;
        switch (item.nodeType) {
            case "Term":
                ajaxUrl = "/api/TermManagementApi/DeleteTerm";
                confirmMsg = RMResx.RM_TM_DeleteTermMsg;
                delSuccessMsg = RMResx.RM_TM_DeleteTermSuccessMsg
                break;
            case "TermSet":
                ajaxUrl = "/api/TermManagementApi/DeleteRootTerms";
                confirmMsg = RMResx.RM_TM_DeleteRootTermMsg;
                delSuccessMsg = RMResx.RM_TM_DeleteTermSetSuccessMsg
                break;
            case "TermGroup":
                ajaxUrl = "/api/TermManagementApi/DeleteTermGroup";
                confirmMsg = RMResx.RM_TM_DeleteTermGroupMsg;
                delSuccessMsg = RMResx.RM_TM_DeleteTermGroupSuccessMsg
                isUpdateTermGroupList = true;
                break;
            case "NormalLocation":
            case "MininumLocation":
                ajaxUrl = "/api/LocationManagementApi/DeleteLocation";
                delSuccessMsg = RMResx.RM_TM_DeleteLocationSuccessMsg
                break;
            case TemplateTreeNodeType.Suite:
                ajaxUrl = "/api/TemplateManagementApi/DeleteSuite";
                delSuccessMsg = RMResx.RM_TM_DeleteSuiteSuccessMsg
                break;
            case TemplateTreeNodeType.Records:
            case TemplateTreeNodeType.Folder:
            case TemplateTreeNodeType.Box:
            case TemplateTreeNodeType.Custom:
                ajaxUrl = "/api/TemplateManagementApi/DeleteTemplate";
                delSuccessMsg = RMResx.RM_TM_DeleteTemplateSuccessMsg
                break;
            case "RuleContainer":
                ajaxUrl = "/api/RuleApi/DeleteRuleContainer";
                delSuccessMsg = RMResx.RM_RDM_RuleContainer_DeleteSuccessMsg;//TODO Cyrus
                break;
            default:
                break;
        }
        let content = null;
        if (this.treeContext.treeType == 2) {
            nodeKey = item.origin.Id;
            content = RMResx.RM_LM_DeleteLocationMsg;
        }
        else if(this.treeContext.treeType == 3)
        {
            if(item.nodeType == TemplateTreeNodeType.Suite)
            {
                nodeKey = item.origin.UniqueId;
                content = RMResx.RM_PRM_TM_ConfigDelSuiteMsg;

            } else {
                nodeKey ={TemplateId: item.origin.UniqueId, TemplateIdList: item.origin.TemplateIdList} ;
                content = RMResx.RM_PRM_TM_ConfigDelTemplateMsg;
            }
        }
        else if (this.treeContext.treeType == 4) {
            if (item.nodeType == "RuleContainer") {
                nodeKey = item.origin.ContainerId;
                content = RMResx.RM_RDM_RuleContainer_DeleteMsg;
                // if (item.origin.SubTermCount != 0) {
                //     this.showDelRuleContainerErrorMsg();
                //     return;
                // } else {
                //     nodeKey = item.origin.ContainerId;
                //     content = RMResx.RM_RDM_RuleContainer_DeleteMsg;
                // }
            }
        }
        else {
            if (item.nodeType == "TermSet") {
                nodeKey = item.origin.Id;
            } else {
                nodeKey = item.nodeKey;
            }
            content = <div>
                <$g.I18NProvider msg={confirmMsg}>
                    <a className="ra-link-a" onClick={()=>{$$.messagedialog(false); self.props.treeContext.history.push({ pathname: RouterUrls.RC_TermUsageReportManagement });}}>
                        {RMResx.RM_TermUsageReport_PageTitle}
                    </a>
                </$g.I18NProvider>
            </div>;
        }

        let args = {
            // classify: "warn",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: content,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", id: "raTmDeleteTermMsgBoxOkBtn", onClick: () => {
                        $$.messagedialog(false);
                        //debugger  //Quality Issue
                        let loadingtimer = setTimeout(function () {
                            $$.loading(true);
                        }, 3000);
                        $.ajax({
                            type: "POST",
                            url: ajaxUrl,
                            contentType: 'application/json;charset=utf-8',
                            data: JSON.stringify(nodeKey),
                            async: true,
                            success: (data) => {
                                clearTimeout(loadingtimer);
                                $$.loading(false);
                                if (isUpdateTermGroupList) {
                                    self.treeContext.updateTermGroupList();
                                }

                                if (this.treeContext.treeType == 1) {
                                    if(data == "-1"){
                                        let failedMsg = RMResx.RM_Multi_Geo_Update_Common_ErrorMessage;
                                        showToast.error(failedMsg);
                                        return;
                                    }
                                }
  
                                if (this.treeContext.treeType == 2) {
                                    if (!data) {
                                        $$.alert(true, {
                                            title: RMResx.RM_JS_Common_RecourdAutomation,
                                            content: RMResx.RM_LM_DeleteLocationHasPhysicalDataErrorMsg,
                                            classify: "error",
                                            buttons: [
                                                { text: "OK", classify: "theme", onClick: function () {  } },
                                            ],
                                        }); 
                                        return;
                                    }
                                }
                                if (this.treeContext.treeType == 3) {
                                    if (data.MessageType == 1) {
                                        let failedMsg = item.nodeType == TemplateTreeNodeType.Suite? RMResx.RM_PRM_TM_FailedToDeleteSuite :RMResx.RM_PRM_TM_FailedToDeleteTemplate;
                                        self.treeContext.showErrorMessage(failedMsg);
                                        return;
                                    }
                                }
                                if (this.treeContext.treeType == 4) {
                                    if (data.MessageType == 1) {
                                        this.showDelRuleContainerErrorMsg();
                                        return;
                                    }

                                    if(data.MessageType == -1){
                                        showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                                        return;
                                    }
                                }
                                showToast.success(delSuccessMsg);
                                // if (item.nodeType == "TermSet") {
                                //     item.loaded = false;
                                //     self.props.itemComponent.reload(0);
                                // } else {
                                self.props.parentItemComponent.reload(-1);
                                // }
                                self.treeContext.refreshSelectedNodeInfo(item.origin, 4);
                            },
                            error: (msg) => {
                                clearTimeout(loadingtimer);
                                $$.loading(false);
                            },
                            dataType: "json"
                        });
                    }
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    showDelRuleContainerErrorMsg = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_RDM_RuleContainer_DeleteError,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: () => {
                        $$.messagedialog(false);
                    }
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    onRetireItemClick(e) {
        let self = this,
            item = this.state.item,
            oitem = item.origin,
            hasActivationSetting = this.hasActivationSetting(),
            tempFunc = () => {
                $.ajax({
                    type: "post",
                    url: "/api/TermManagementApi/DeprecateTerm",
                    contentType: 'application/json;charset=utf-8',
                    data: JSON.stringify(item.nodeKey),
                    success: function (data) {
                        if (data != "") {
                            if(data == "-1"){
                                showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                            }else{
                                oitem.IsDeprecated = true;
                                self.setState({ item: item });
                                let newItem = $.parseJSON(data);    // Fortify Issue Type: JSON Injection; Sink Details: retire term; Ignore Reason: 前后台对象存在对应关系
                                self.treeContext.refreshSelectedNodeInfo(newItem, 2);
                            }
                        }
                    },
                    error: function (msg) {
                        //alert(msg.responseText);
                    },
                    dataType: "json"
                });
            };
        if (hasActivationSetting) {
            this.confirmRetireOrActiveTerm(tempFunc);
        } else {
            tempFunc();
        }
    }

    onActiveItemClick(e) {
        let self = this,
            item = this.state.item,
            oitem = item.origin,
            hasActivationSetting = this.hasActivationSetting(),
            tempFunc = () => {
                $.ajax({
                    type: "POST",
                    url: "/api/TermManagementApi/EnableTerm",
                    contentType: 'application/json;charset=utf-8',
                    data: JSON.stringify(item.nodeKey),
                    success: function (data) {
                        if (data != "") {
                            if(data == "-1"){
                                showToast.error(RMResx.RM_Multi_Geo_Update_Common_ErrorMessage);
                            }else{
                                oitem.IsDeprecated = false;
                                oitem.IsExpired = false;
                                self.setState({ item: item });
                                let newItem = $.parseJSON(data);    // Fortify Issue Type: JSON Injection; Sink Details: enable retire; Ignore Reason: 前后台对象存在对应关系
                                self.treeContext.refreshSelectedNodeInfo(newItem, 3);
                            }                          
                        }
                    },
                    error: function (msg) {
                        //alert(msg.responseText);
                    },
                    dataType: "json"
                });
            };
        if (hasActivationSetting) {
            this.confirmRetireOrActiveTerm(
                tempFunc,
                () => $$.messagedialog(false)
            );
        } else {
            tempFunc();
        }
    }

    onEditInputClick(e) {
        e.stopPropagation();
    }

    onEditInputKeyDown(e) {
        if (e.keyCode == 13) {
            this.continueCreate = this.state.isCreating;
            e.target.blur();
        }
        e.stopPropagation();
    }

    onEditInputBlur(e) {
        let name = e.target.value,
            treeType = this.treeContext.treeType;
        if (this.isUnsafeNodeName(name)) {
            $g.showMsgBox(
                'e',
                treeType == 1 ? RMResx.RM_TM_IllegalCharacterMsg : RMResx.RM_LM_NameInvalid,
                [{ name: RMResx.RM_JS_Common_Close, isPrimary: true, onClick: null }],
                () => {
                    // this.nodeNameInput.value = this.repalceUnSafeChar(name);
                    this.setState({
                        isEditing: true
                    });
                    setTimeout(()=>{
                        $(this.nodeNameInput).focus().select();
                    }, 300);
                }
            );
        } else if (this.isTermNameLenGt255(name)) {
            $g.showMsgBox(
                'e',
                RMResx.RM_TM_NameLenTooLongMsg,
                [{ name: RMResx.RM_JS_Common_Close, isPrimary: true, onClick: null }],
                () => {
                    this.setState({
                        isEditing: true
                    });
                    setTimeout(()=>{
                        $(this.nodeNameInput).focus().select();
                    }, 300);
                }
            );
        } else if (treeType === 5 && !this.isValidateNodeName(name)) {
            $g.showMsgBox(
                'e',
                RMResx.RM_ES_CompliantExport_Metadata_NodeNameValidateMsg,
                [{ name: RMResx.RM_JS_Common_Close, primary: true, onClick: null }],
                () => {
                    this.setState({
                        isEditing: true
                    });
                    setTimeout(()=>{
                        $(this.nodeNameInput).focus().select();
                    }, 300);
                }
            );
        } else if (this.state.isCreating) {
            this.createItem();
        } else {
            this.renameItem();
        }
    }

    renderNodeText(text) {
        let searchKey = this.treeContext.searchKey;
        if (!searchKey) {
            return text;
        } else {
            var escapingSearchKey = searchKey.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
            var re = new RegExp(escapingSearchKey, "gi");
            if (re.test(text)) {
                var matchResult = [];
                let index = 0;
                return <$g.I18NProvider msg={text.replace(re, function (match) { matchResult.push(match); return `{${index++}}`; })}>
                    {matchResult.map((matchStr, idx) => {
                        return <span key={idx} className="ra-tree-node-text-red">{matchStr}</span>;
                    })}
                </$g.I18NProvider>;
            }
            return text;
        }
    }

    getExportSettingAction() {
        if(this.treeContext.treeType == 5){
            return (
                <>
                    <TreeActionItem
                    text={RMResx.RM_ES_CompliantExport_VEOTree_CreateButton}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateItemClick}
                    />
                    {this.state.item.level !== 1 && (
                        <TreeActionItem
                        text={RMResx.RM_ES_CompliantExport_VEOTree_DeleteButton}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick}
                        />
                    )}
                </>
            );
        }      
    }

    render() {
        let itemsCount = this.props.item.itemsCount;
        let hasChild = itemsCount != 0;
        let item = this.state.item;
        let itemDeprecated = item.origin && item.origin.IsDeprecated;
        let itemExpired = item.origin && item.origin.IsExpired;
        let itemDisabled = itemDeprecated || itemExpired;
        let itemMininum = item.origin && item.origin.NodeType == NodeType.PhysicalBottomLocation;
        let treeType = this.treeContext.treeType;
        let uiTemplateNodeType = this.getUITemplateNodeType(item.origin);
        let isDefaultSuiteNode = this.isDefaultSuiteNode(item.origin);
        let hideDelete = isDefaultSuiteNode || hasChild;

        if (treeType == 2) {
            itemDisabled = item.nodeType == "Term" && (item.origin && item.origin.IsDefaultTerm);
        }
        return (<React.Fragment>
            <div
                className={"ra-tree-node-content"} aria-label={item.text} data-tooltip="true" data-tooltip-wrap="force" ref={r => this.nodeContent = r}
                onDoubleClick={this.onNodeDoubleClick}>
                <$g.Icon className={this.getNodeIconClass(item)}></$g.Icon>
                {this.state.isEditing &&
                    <input type="text" className="ra-tree-node-text-edit" defaultValue={item.text}
                        ref={r => this.nodeNameInput = r}
                        onClick={this.onEditInputClick} onBlur={this.onEditInputBlur}
                        onKeyDown={this.onEditInputKeyDown} />
                }
                {!this.state.isEditing &&
                    <div className={"ra-tree-node-text ra-tree-tm-node-text " + this.getNodeTextClass(item)}>
                        {this.renderNodeText(item.text)}
                    </div>
                }
            </div>
            {!((item.nodeType === "RuleContainerRoot" || item.nodeType === "RuleContainer") && !isMultiGeoMainDC) && (
            <TreeActionsPopup
                recalculatePosition={this.props.recalculatePosition}
                itemComponent={this.props.itemComponent}
                treeContext={this.props.treeContext}
                disabled={this.state.isEditing} >
                {item.nodeType == "Root" && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_JS_TM_CreateGroup}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateItemClick} />
                </React.Fragment>}

                {item.nodeType == "TermGroup" && <React.Fragment>
                    {/* {(!itemsCount || itemsCount == 0) && */}
                    <TreeActionItem
                        text={RMResx.RM_JS_TM_CreateTermSet}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateItemClick} />
                    {/* } */}
                    <TreeActionItem
                        text={RMResx.RM_JS_TM_RenameTermGroup}
                        iconClass="fia-rename"
                        actionClass="ra-tree-action-rename"
                        onActionClick={this.onRenameClick} />
                    {item.origin && item.origin.Id != 1 &&
                        <TreeActionItem
                            text={RMResx.RM_JS_TM_DelGroup}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                </React.Fragment>}

                {item.nodeType == "TermSet" && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_JS_TM_CreateTerm}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateItemClick} />
                    <TreeActionItem
                        text={RMResx.RM_JS_TM_RenameTermSet}
                        iconClass="fia-rename"
                        actionClass="ra-tree-action-rename"
                        onActionClick={this.onRenameClick} />
                    {treeType == 1 && <TreeActionItem
                        text={RMResx.RM_JS_TM_DelRootTerm}
                        iconClass="fia-delete"
                        actionClass="ra-tree-action-delete"
                        onActionClick={this.onDeleteItemClick} />
                    }
                </React.Fragment>}

                {this.treeContext.componentType == "CRMPhyTree" && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_DAM_Refesh}
                        iconClass="fia-refresh"
                        onActionClick={this.onRefreshActionClick} />
                </React.Fragment>}

                {item.nodeType == "RootLocation" && <React.Fragment>
                    {checkPermission(RouterUrls.CP_Index, RM.UserResources) && (
                        <TreeActionItem
                            text={RMResx.RM_JS_LM_CreateLocation}
                            iconClass="fia-plus"
                            actionClass="ra-tree-action-new"
                            onActionClick={this.onCreateItemClick}
                        />
                    )}
                    {!itemDisabled && 
                        <TreeActionItem
                            text={RMResx.RM_JS_LM_RenameLocation}
                            iconClass="fia-rename"
                            actionClass="ra-tree-action-rename"
                            onActionClick={this.onRenameClick} />
                    }
                </React.Fragment>}

                {item.nodeType == "Term" && <React.Fragment>
                    {!itemDisabled && 
                        <TreeActionItem
                            text={RMResx.RM_JS_TM_CreateTerm}
                            iconClass="fia-plus"
                            actionClass="ra-tree-action-new"
                            onActionClick={this.onCreateItemClick} />
                    }
                    {!itemDisabled && 
                        <TreeActionItem
                            text={RMResx.RM_JS_TM_RenameTerm}
                            iconClass="fia-rename"
                            actionClass="ra-tree-action-rename"
                            onActionClick={this.onRenameClick} />
                    }
                    {treeType == 1 && <React.Fragment>
                        {!itemDisabled && 
                            <TreeActionItem
                                text={RMResx.RM_JS_TM_DepreTerm}
                                iconClass="fia-retire"
                                actionClass="ra-tree-action-retire"
                                onActionClick={this.onRetireItemClick} />
                        }
                        {itemDisabled && 
                            <TreeActionItem
                                text={RMResx.RM_JS_TM_EnabelTerm}
                                iconClass="fia-reactivate"
                                actionClass="ra-tree-action-active"
                                onActionClick={this.onActiveItemClick} />
                        }
                    </React.Fragment>}
                    <TreeActionItem
                        text={RMResx.RM_JS_TM_DelTerm}
                        iconClass="fia-delete"
                        actionClass="ra-tree-action-delete"
                        onActionClick={this.onDeleteItemClick} />
                </React.Fragment>}

                {(item.nodeType == "NormalLocation" || item.nodeType == "MininumLocation") && <React.Fragment>
                    {!itemMininum && 
                    <TreeActionItem
                        text={RMResx.RM_JS_LM_CreateLocation}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateItemClick} />
                    }
                    <TreeActionItem
                        disabled={false}
                        text={RMResx.RM_JS_LM_RenameLocation}
                        iconClass="fia-rename"
                        actionClass="ra-tree-action-rename"
                        onActionClick={this.onRenameClick} />
                    {!hasChild && 
                    <TreeActionItem
                        text={RMResx.RM_JS_LM_DelLocation}
                        iconClass="fia-delete"
                        actionClass="ra-tree-action-delete"
                        onActionClick={this.onDeleteItemClick} /> }

                </React.Fragment>}

                {/* Physical Template Tree Menu Action */}
                {uiTemplateNodeType == UITemplateTreeNodeType.Root && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_NewSuite}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateSuiteItemClick} />
                    <TreeActionItem
                        text={RMResx.RM_JS_Template_ImportTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onImportSuiteItemsClick} />
                    </React.Fragment>
                    
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.BoxSuite && <React.Fragment>
                    {
                        !hasChild && 
                        <TreeActionItem
                        text={RMResx.RM_PRM_TM_NewBoxTemplate_PageTitle}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.Box)} />
                    }
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_MenuBtn_EditSuite}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditSuiteClick} />
                    {
                        !hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_PRM_TM_MenuBtn_DeleteSuite}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.FolderSuite && <React.Fragment>
                    {
                        !hasChild &&
                        <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_NewFolderTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.Folder)} />
                    }
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_MenuBtn_EditSuite}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditSuiteClick} />
                    {
                        !hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_PRM_TM_MenuBtn_DeleteSuite}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.CustomSuite && <React.Fragment>
                    {
                        !hasChild &&
                        <TreeActionItem
                            text={RMResx.RM_PRM_TM_Btn_NewContainerTemplate}
                            iconClass="fia-plus"
                            actionClass="ra-tree-action-new"
                            onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.CustomTemplate)} />
                    }
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_MenuBtn_EditSuite}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditSuiteClick} />
                    {
                        !hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_PRM_TM_MenuBtn_DeleteSuite}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.BoxTemplate && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_NewFolderTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new-folderTpl"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.Folder)} />
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_AddExistingTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-add-existTpl"
                        onActionClick={this.onAddExistingTemplateClick.bind(this)} />
                    <TreeActionItem
                        text={RMResx.RM_EditTemplate_PateTitle}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditTemplateClick} />
                    {!hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_RC_Audit_Action_DeleteTemplate}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.FolderTemplate && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_NewRecordTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new-recordTpl"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.Records)} />
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_AddExistingTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-add-existTpl"
                        onActionClick={this.onAddExistingTemplateClick.bind(this)} />
                    <TreeActionItem
                        text={RMResx.RM_EditTemplate_PateTitle}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditTemplateClick} />
                    {!hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_RC_Audit_Action_DeleteTemplate}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.RecordTemplate && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_EditTemplate_PateTitle}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditTemplateClick} />
                    {!hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_RC_Audit_Action_DeleteTemplate}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {uiTemplateNodeType == UITemplateTreeNodeType.CustomTemplate && <React.Fragment>
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_NewContainerTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new-customTpl"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.CustomTemplate)} />
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_NewBoxTemplate_PageTitle}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new-boxTpl"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.Box)} />
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_NewFolderTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-new-folderTpl"
                        onActionClick={this.onCreateTemplateClick.bind(this, TemplateTypes.Folder)} />
                    <TreeActionItem
                        text={RMResx.RM_PRM_TM_Btn_AddExistingTemplate}
                        iconClass="fia-plus"
                        actionClass="ra-tree-action-add-existTpl"
                        onActionClick={this.onAddExistingTemplateClick.bind(this)} />
                    <TreeActionItem
                        text={RMResx.RM_EditTemplate_PateTitle}
                        iconClass="fia-edit"
                        actionClass="ra-tree-action-edit"
                        onActionClick={this.onEditTemplateClick} />
                    {!hideDelete && !item.origin.IsUnderDefaultSuite &&
                        <TreeActionItem
                            text={RMResx.RM_RC_Audit_Action_DeleteTemplate}
                            iconClass="fia-delete"
                            actionClass="ra-tree-action-delete"
                            onActionClick={this.onDeleteItemClick} />
                    }
                    </React.Fragment>
                }
                {item.nodeType == "RuleContainerRoot" && <React.Fragment>
                    {!itemDisabled &&
                        <TreeActionItem
                            text={RMResx.RM_RDM_CreateRuleContainer}//TODO Cyrus
                            iconClass="fia-plus"
                            actionClass="ra-tree-action-new"
                            onActionClick={this.onCreateItemClick} />
                    }
                </React.Fragment>}
                {item.nodeType == "RuleContainer" && <React.Fragment>
                    {!itemDisabled &&
                        <TreeActionItem
                            text={RMResx.RM_RDM_RenameRuleContainer}//TODO Cyrus
                            iconClass="fia-rename"
                            actionClass="ra-tree-action-rename"
                            onActionClick={this.onRenameClick} />
                    }
                    <TreeActionItem
                        text={RMResx.RM_RDM_DelRuleContainer}//TODO Cyrus
                        iconClass="fia-delete"
                        actionClass="ra-tree-action-delete"
                        onActionClick={this.onDeleteItemClick} />
                </React.Fragment>}
                {this.getExportSettingAction()}
            </TreeActionsPopup>
            )}
        </React.Fragment>);
    }
}
