import StringUtil from "../../../../Utilities/StringUtil";
import HSFilteType from "./HSFilterComponent/HSFilteType";
import HSFilteDateAndTime from "./HSFilterComponent/HSFilteDateAndTime";
import HSFilterClassification from "./HSFilterComponent/HSFilterClassification";
import HSFilterSource from "./HSFilterComponent/HSFilterSource";
import PeoplePicker from "../../../Common/PeoplePicker";
import HSFilterNumber from "./HSFilterComponent/HSFilterNumber";
import HSMultipleChoice from "./HSFilterComponent/HSMultipleChoice";
import HSSingleAndMultText from "./HSFilterComponent/HSSingleAndMultText";
import HSFileSystemFolder from "./HSFilterComponent/HSFileSystemFolder";
import HSPhyTemplates from "./HSFilterComponent/HSPhyTemplates";
import HSSearchView from "./HSFilterComponent/HSSearchView";
import HSOfflineSearchJobs from "./HSFilterComponent/HSOfflineSearchJobs";
import { PhysicalObjectColumnType, PhysicalDefaultColumnIDs, PhysicalDefaultArray, SourceFlags } from "../../../../Constants/Constants";
import { SearchViewTypes, BuildColumnIds, OperationLogicValues, SearchKeyOperationLogic, DateConditions, 
    ToSearchComponentDispatchType, MsgComponentType, SpecialSearchViewIds, 
    LocationOperationLogic} from './../Constants';
import {TemplateTreeNodeType} from "../../../PRM/Constants";
import HSSPOLocation from "./HSFilterComponent/HSSPOLocation";
import HSTeamsLocation from "./HSFilterComponent/HSTeamsLocation";
import { EnvironmentHelper, LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import { RoleType } from '../../../../Constants/Constants';
import { checkPermission } from "../../../../Utilities/permissionManager";
import _ from "lodash";
import {getActionDueDateI18n} from "../../../../Utilities/CommonUtil";
import RouterUrls from "../../../../Constants/RouterUrls";
import HSGoogleLocation from "./HSFilterComponent/HSGoogleLocation";

// const sourceItems = [
//     { name: RMResx.RM_JS_SPS_TabLabel_SP, id: SourceFlags.SP, isChecked: false },
//     { name: RMResx.RM_JS_SPS_TabLabel_FS, id: SourceFlags.FS, isChecked: false },
//     { name: RMResx.RM_JS_SPS_TabLabel_EXO, id: SourceFlags.Exo, isChecked: false },
//     { name: RMResx.RM_JS_SPS_TabLabel_Physical, id: SourceFlags.Phy, isChecked: false },
//     { name: RMResx.RM_Common_SharePointOnPremise, id: SourceFlags.SPLocal, isChecked: false },
//     { name: RMResx.RM_JS_SPS_TabLabel_OneDrive, id: SourceFlags.OneDrive, isChecked: false },
//     { name: RMResx.RM_JS_SPS_TabLabel_AF, id: SourceFlags.AzureFile, isChecked: false },
// ];

export default class HSFilter extends R.Component {
    idAttr = true;
    componentCreate() {
        this.defaultColumnIds = RM.deepcopy(BuildColumnIds);
        this.defaultColumnIdsList = this.getDefaultColumnIdsList();
        this.defautBasedSearchColumns = this.getDefautBasedSearchColumns();
        this.defautAdvancedSearchColumn = this.getDefautAdvancedSearchColumn();
        this.isStandardUser = RM.RoleType == RoleType.StandardUser;
        this.isStandardReviewUser = RM.RoleType == RoleType.StandardReviewUser;
        this.state = {
            allSearchColumns: [],
            selectedSearchColumns: RM.deepcopy(this.defautAdvancedSearchColumn),
            showSearchView: false,
            showSearchColumns: false,
            isAdvancedSearch: false,
            backBaseBtnDisabled: false,
            sourceColumnOptions: [],
            phyTemplates: [],
            showOfflineSearchJobsCombobox: false,
            showWildcardsMsgBar: false,
            hasRunningJob: false,
            backBasicSearchAria: `${RMResx.RM_HS_BackBasicSearch} ${RMResx.RM_HS_ToBaseSearchBtnDisabledMsg}`,
        };
        this.logicOptions = [
            { name: RMResx.RM_HS_SearchKeywordAnd, value: OperationLogicValues.And, checked: true },
            { name: RMResx.RM_HS_SearchKeywordOr, value: OperationLogicValues.Or, checked: false }
        ];
        this.yesOrNoOptions = {
            0: RMResx.RM_PRM_PRE_Cell_HoldStatusYes,
            1: RMResx.RM_PRM_PRE_Cell_HoldStatusNo
        };
        this.isExpireReturnDateSearch = this.props.isExpireReturnDateSearch;
        this.jumpParam = this.props.jumpParam;
        this.isShowAll = this.props.isShowAll;
    }

    componentReceive(type, data) {
        if (type == ToSearchComponentDispatchType.DisableBackBaseBtn) {
            let isDisabled = data;
            if (!isDisabled && this.state.isAdvancedSearch) {
                this.refSearchbox.clear();
            }
        }
        if (type == ToSearchComponentDispatchType.SourceType) {
            let avaliableSourceFlags = data.map((item)=>{ return item.Value; });
            this.isContainsPhySource = avaliableSourceFlags.includes(SourceFlags.Phy);
            this.isContainsFSSource = avaliableSourceFlags.includes(SourceFlags.FS);
            // let sourceColumnOptions = RM.deepcopy(sourceItems).filter((item) => {
            //     return avaliableSourceFlags.includes(item.id);
            // });
            this.setState({ sourceColumnOptions: data });
            this.setAllSearchColumns();
            this.setAllPhyTemplates();
        }
        if (type == ToSearchComponentDispatchType.TransSelectedTableIds) {
            this.selectedTableColumnsIds = data;
        }
        if (type == ToSearchComponentDispatchType.SortColumn) {
            this.currentSortColumnInfo = data;
        }
    }

    getDefautBasedSearchColumns() {
        return [
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_Name,
                ColumnType: PhysicalObjectColumnType.SingleText,
                OptionsJSON: null,
                Templates: [],
                UniqueId: PhysicalDefaultColumnIDs.NameOrTitle,
                ComponentKey: StringUtil.newGuid(),
                CurrentCriteriaId: StringUtil.newGuid(),
                ColumnsLogic: OperationLogicValues.Or,
                ColumnOperationLogic: SearchKeyOperationLogic.Contains
            },
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_ID,
                ColumnType: PhysicalObjectColumnType.SingleText,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.UniqueId,
                ComponentKey: StringUtil.newGuid(),
                CurrentCriteriaId: StringUtil.newGuid(),
                ColumnOperationLogic: SearchKeyOperationLogic.Contains
            }
        ];
    }

    getDefautAdvancedSearchColumn() {
        return [
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_Name,
                ColumnType: PhysicalObjectColumnType.SingleText,
                OptionsJSON: null,
                Templates: [],
                UniqueId: PhysicalDefaultColumnIDs.NameOrTitle,
                ComponentKey: StringUtil.newGuid(), //只用于前台，向后台传值去掉
                CurrentCriteriaId: StringUtil.newGuid(),
                ColumnOperationLogic: SearchKeyOperationLogic.Contains
            }
        ];
    }

    getDefautSearchColumns() {
        var hasFSPermission = checkPermission("Source_FS", RM.UserResources); 
        var defaultColumns = [
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_Name,
                ColumnType: PhysicalObjectColumnType.SingleText,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.NameOrTitle,
            },
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_ID,
                ColumnType: PhysicalObjectColumnType.SingleText,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.UniqueId,
            },
            {
                ColumnName: RMResx.RM_JS_RC_Common_ReportType,
                ColumnType: null,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.SourceFlag,
                OldParamKey: "SourceFlags"
            },
            {
                ColumnName: RMResx.RM_JS_BCM_Explorer_Datagrid_FileType,
                ColumnType: null,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.Type,
                OldParamKey: "FileExtensions"
            },
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_ModifiedTime,
                ColumnType: PhysicalObjectColumnType.DateTime,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.ModifiedTime,
                OldParamKey: "ModifiedDateInfo"
            },
            {
                ColumnName: RMResx.RM_JS_JMD_Grid_Classification,
                ColumnType: null,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.Classification,
                OldParamKey: "TermIds"
            },
            {
                ColumnName: RMResx.RM_JS_Common_CreatedBy,
                ColumnType: PhysicalObjectColumnType.PeopleOrGroup,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.CreatedBy,
                OldParamKey: "CreatedBy"
            },
            {
                ColumnName: RMResx.RM_TemplateManage_ModifiedBy,
                ColumnType: PhysicalObjectColumnType.PeopleOrGroup,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.ModifiedBy,
                OldParamKey: "ModifiedBy"
            },
            {
                ColumnName: getActionDueDateI18n(),
                ColumnType: PhysicalObjectColumnType.DateTime,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.ActionDueDate,
                OldParamKey: "DisposalDateInfo"
            },
            {
                ColumnName: RMResx.RM_JS_BCM_Explorer_Datagrid_RecordsOwner,
                ColumnType: PhysicalObjectColumnType.PeopleOrGroup,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.Owners,
                OldParamKey: "Owners"
            },
            {
                ColumnName: RMResx.RM_JS_BCM_Explorer_Datagrid_OnHold,
                ColumnType: PhysicalObjectColumnType.MultipleChoice,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.HoldStatus,
                OldParamKey: "HoldStatus"
            },
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_HoldBy,
                ColumnType: PhysicalObjectColumnType.PeopleOrGroup,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.HoldByUsersId,
                OldParamKey: "HoldBy"
            },
            {
                ColumnName: RMResx.RM_JS_RDM_Explorer_CreateTime,
                ColumnType: PhysicalObjectColumnType.DateTime,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.CreatedDateInfo,
                OldParamKey: "CreatedDateInfo"
            }
        ];
        if(!LicenseHelper.HasOpusGoogleLicenseOnly()) {
            defaultColumns.push(
                {
                    ColumnName: RMResx.RM_JS_BCM_Explorer_Datagrid_Declared,
                    ColumnType: PhysicalObjectColumnType.MultipleChoice,
                    OptionsJSON: null,
                    Templates: [],
                    UniqueId: this.defaultColumnIds.DeclaredRecord,
                    OldParamKey: "DeclaredRecord"
                }
            );

            if (!LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver()) {
                defaultColumns.push({
                    ColumnName: RMResx.RM_JS_BCM_Explorer_Datagrid_RecordsLabel,
                    ColumnType: PhysicalObjectColumnType.MultipleChoice,
                    OptionsJSON: null,
                    Templates: [],
                    UniqueId: this.defaultColumnIds.LockedByRecordLabel,
                    OldParamKey: "LockedByRecordLabel"
                });
            }
            
            defaultColumns.push(
                {
                    ColumnName: RMResx.RM_JS_BCM_Explorer_Details_Archived,
                    ColumnType: PhysicalObjectColumnType.MultipleChoice,
                    OptionsJSON: null,
                    Templates: [],
                    UniqueId: this.defaultColumnIds.ContentArchived,
                },
            );
        }
        if (checkPermission("Source_SP", RM.UserResources) || checkPermission("Source_OneDrive", RM.UserResources)) {
            defaultColumns.push(
                {
                    ColumnName: RMResx.RM_JS_BCM_Explorer_Filter_SharePointOnlineLabel,
                    ColumnType: null,
                    OptionsJSON: null,
                    Templates: [],
                    UniqueId: this.defaultColumnIds.SPOLocation,
                },
            )
        }
        if (LicenseHelper.HasUpgradeTeams() && checkPermission("Source_Teams", RM.UserResources)) {
            defaultColumns.push(
                {
                    ColumnName: RMResx.RM_JS_BCM_Explorer_Filter_TeamsLabel,
                    ColumnType: null,
                    OptionsJSON: null,
                    Templates: [],
                    UniqueId: this.defaultColumnIds.TeamsLocation,
                },
            )
        }
        if (LicenseHelper.HasOpusGoogleLicense() && checkPermission("Source_Google", RM.UserResources)) {
            defaultColumns.push({
                ColumnName: RMResx.RM_JS_BCM_Explorer_Filter_GoogleLabel,
                ColumnType: null,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.GoogleLocation,
            });
        }
        if(hasFSPermission && !EnvironmentHelper.IsGCPEnvironment){
            defaultColumns.push({          
                ColumnName: RMResx.RM_JS_BCM_Explorer_Filter_FolderLabel,
                ColumnType: null,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.FileSystem,
                OldParamKey: "NodeId"            
            });
        }
        return defaultColumns;
    }

    getDefaultColumnIdsList() {
        let defaultColumnIdsList = [];
        for (let key in this.defaultColumnIds) {
            defaultColumnIdsList.push(this.defaultColumnIds[key]);
        }
        return defaultColumnIdsList;
    }

    getPhyUniqueBuildColumn() {
        //此方法中返回的数据是physical特有的column，而且不在physical template中，BuildIn的column。
        return [
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_LoanBy,
                ColumnType: PhysicalObjectColumnType.PeopleOrGroup,
                IdsWithDuplicateName: [this.defaultColumnIds.LoanBy],
                NameHash: this.defaultColumnIds.LoanBy,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.LoanBy,
            },
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_Templates,
                ColumnType: this.defaultColumnIds.PhyTemplates,
                IdsWithDuplicateName: [this.defaultColumnIds.PhyTemplates],
                NameHash: this.defaultColumnIds.PhyTemplates,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.PhyTemplates,
            },
            {
                ColumnName: RMResx.RM_PRM_PRE_Column_ReturnDate,
                ColumnType: PhysicalObjectColumnType.DateTime,
                IdsWithDuplicateName: [this.defaultColumnIds.ReturnDate],
                NameHash: this.defaultColumnIds.ReturnDate,
                OptionsJSON: null,
                Templates: [],
                UniqueId: this.defaultColumnIds.ReturnDate,
            }];
    }

    setAllSearchColumns() {
        let defautSearchColumns = this.getDefautSearchColumns();
        let phyUniqueBuildColumn = this.getPhyUniqueBuildColumn();
        if (this.isContainsPhySource) {
            $$.loading(true);
            let param = {
                url: '/api/TemplateManagementApi/LoadAllColumns',
                method: "post",
                data: {
                    LoadAll: true,
                }
            };
            fetchUtility(param).then((res) => {
                //这些defaut column已存在因此删除；HomeLocation不是基础column type，因此删除。
                let unnecessaryColumnIds = [
                    PhysicalDefaultColumnIDs.NameOrTitle,
                    PhysicalDefaultColumnIDs.Classification,
                    PhysicalDefaultColumnIDs.HomeLocation,
                    PhysicalDefaultColumnIDs.LoanedBy,
                ];
                for (let columnId of unnecessaryColumnIds) {
                    res.splice(res.findIndex(item =>
                        item.UniqueId === columnId
                    ), 1);
                }

                let allSearchColumns = [...defautSearchColumns, ...phyUniqueBuildColumn, ...res];
                this.setAllSearchColumnsList(allSearchColumns);
                $$.loading(false);
            });
        } else {
            this.setAllSearchColumnsList(defautSearchColumns);
        }
    }

    setAllPhyTemplates(){
        if(this.isContainsPhySource){
            $$.loading(true);
            let param = {
                url: '/api/TemplateManagementApi/GetAllExistingTemplatesInfo',
                method: "Post",
            };
            fetchUtility(param).then((res) => {
                this.setState({phyTemplates: this.getMultiComboBoxWithGroupName(res.Templates)});
                $$.loading(false);
            });
        }
    }

    getMultiComboBoxWithGroupName(templates) {
        let mapping = {
            [TemplateTreeNodeType.Custom]: RMResx.RM_PRM_TM_ExistingCustomTemplate_GroupTitle,
            [TemplateTreeNodeType.Box]: RMResx.RM_PRM_TM_ExistingBoxTemplate_GroupTitle,
            [TemplateTreeNodeType.Folder]: RMResx.RM_PRM_TM_ExistingFolderTemplate_GroupTitle,
            [TemplateTreeNodeType.Records]: RMResx.RM_PRM_TM_ExistingRecordTemplate_GroupTitle
        };
        for(let item of templates){
            item.group = mapping[item.Type] || "";
        }
        return templates;
    }

    setAllSearchColumnsList(allSearchColumns) {
        for (let item of allSearchColumns) {
            item.name = RMResx[item.ColumnName] || item.ColumnName;
            if (Object.values(this.defaultColumnIds).includes(item.UniqueId) || PhysicalDefaultArray.includes(item.UniqueId)) {
                item.title = RMResx.RM_PRM_BarcodeTemp_AreaF_BuildInColumn;
            } else {
                item.title = item.Templates.map((template) => { return template.Name; }).join(', ');
            }
        }
        this.setState({
            allSearchColumns: allSearchColumns
        }, () => {
            this.setState({ showSearchView: true });
        });
    }

    onOpenSearchView = () =>{
        this.dispatch("raHSSearchView", "showOfflineSearchTip", this.allowRunSearchJob());
    }

    onOperateSearchView = (operateViewParam) => {
        let actionType = operateViewParam.actionType;
        let selectedViewInfo = operateViewParam.selectedViewInfo;
        let savedViewNewName = operateViewParam.savedViewNewName;
        switch (actionType) {
            case SearchViewTypes.Save:
                this.onSaveSearchView(selectedViewInfo.Id, savedViewNewName);
                break;
            case SearchViewTypes.SaveAs:
                this.onSaveSearchView(null, savedViewNewName);
                break;
            case SearchViewTypes.SaveAsDefaut:
                this.onSaveSearchView(selectedViewInfo.Id, selectedViewInfo.Name, true, selectedViewInfo.IsBuiltIn);
                break;
            case SearchViewTypes.Delete:
                this.onDeleteSearchView(selectedViewInfo.Id);
                break;
            case SearchViewTypes.View:
                this.loadDataBySearchView(selectedViewInfo.Id);
                break;
            case SearchViewTypes.Share:
                this.onShareSearchView(operateViewParam);
                break;
        }
    }

    isInValid(data, isSave) {
        setTimeout(() => { $$.verify("hsFilter"); }, 200);
        let inValid = true;
        for (let item of data) {
            let verifiedItem = isSave ? JSON.parse(item.ContentStr) : item;
            if (
                !verifiedItem.Value
                || (verifiedItem.Value && !JSON.parse(verifiedItem.Value) && JSON.parse(verifiedItem.Value) != false)
                || (verifiedItem.Value && JSON.parse(verifiedItem.Value).length == 0)
            ) {
                inValid = false;
            }
        }
        return inValid;
    }

    onSaveSearchView(selectdViewId, savedViewNewName, isSaveDefautView, isBuiltIn) {
        //既是build in 又是default，初始化
        let url = "/api/PersonalSettinggApi/SetAsDefault";
        let saveViewParam = selectdViewId;
        if (!isSaveDefautView) {
            let savedSearchData = this.getSearchParam(true);
            if (this.state.isAdvancedSearch) {
                let isInValid = this.isInValid(savedSearchData, true);
                if (!isInValid) { 
                    this.setState({ showSearchColumns: true }); 
                    return; 
                }
            } else {
                savedSearchData = this.getBaseSearchParam();  //base search不需要验证都可以存。
            }
            url = "/api/PersonalSettinggApi/SaveGlobalSearchCriteria";
            saveViewParam = {
                Setting: {
                    "ContentStr": null,
                    "IsAdvancedSearch": this.state.isAdvancedSearch,
                    "AdvancedSearchs": savedSearchData,
                    "ColumnsStr": this.selectedTableColumnsIds ? JSON.stringify(this.selectedTableColumnsIds) : null,
                    "ColumnSortSetting": JSON.stringify(this.currentSortColumnInfo) || null
                },
                IsBuiltIn: isBuiltIn,
                Name: savedViewNewName,
            };
            if (selectdViewId) { saveViewParam.Id = selectdViewId; }
            if (isSaveDefautView) { saveViewParam.IsDefault = true; }
        }

        $$.loading(true);
        let option = {
            url: url,
            method: "POST",
            data: saveViewParam
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            //SetAsDefault 和 SaveGlobalSearchCriteria两个接口返回值不同，统一改为对象。
            if(res === true){
                res = {ErrorCode: 0};
            }
            if(res === false){
                res = {ErrorCode: 2};
            }
            switch (res.ErrorCode) {
                case 0:
                    if(this.allowRunSearchJob() && !isSaveDefautView){
                        this.startOfflineSearchJob(res.Id);
                    }else{
                        this.dispatch("raHSSearchView", "loadSearchView", selectdViewId || res.Id);
                    }
                    this.setSaveSuccessfulMsg(selectdViewId, isSaveDefautView);
                    this.noNeedCloseMsg = true;
                    break;
                case 1:
                    this.props.onShowMsg("error", RMResx.RM_HS_Criteria_View_Msg_ValidDuplicateViewName);
                    break;
                case 2:
                    this.props.onShowMsg("error", RMResx.RM_HS_Criteria_View_Msg_ValidOtherError);
                    break;
            }
        });
    }

    startOfflineSearchJob = (profileId) =>{
        $$.loading(true);
        let option = {
            url: "/api/PersonalSettinggApi/StartOfflineSearchJob?profileId=" + profileId,
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let messageContent = <$g.I18NProvider msg={RMResx.RM_HS_Offline_StartJobTipForAdmin_Success}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>;
            if(this.isStandardUser || this.isStandardReviewUser){
                messageContent = RMResx.RM_HS_Offline_StartJobTipForEndUser_Success;
            }
            showToast.success(messageContent);
            this.dispatch("raHSSearchView", "loadSearchView", profileId);
        }).catch((e) => {
            $$.loading(false);
            showToast.error(RMResx.RM_HS_Offline_StartJobTip_Fail);
        });
    }

    onSearchByOfflineJob = (profileId, jobId) =>{
        this.props.onOfflineSearch(profileId, jobId, this.state.hasRunningJob);
        let url =  "/api/PersonalSettinggApi/GetGlobalSearchCriteria";
        let option = {
            url: url,
            method: "POST",
            data: profileId
        };
        fetchUtility(option).then((res) => {
            if (res.Setting.ContentStr) {
                this.convertOldSerachSetting(res.Setting);
            } else {
                if (res.Setting.AdvancedSearchs) {
                    this.echoSearchColumns(res.Setting);
                } else {
                    this.echoDefaultViewColumns(res);
                }
            }
            let isOfflineSearch = res.IsOffline;
            this.setState({
                showOfflineSearchJobsCombobox: isOfflineSearch,
                showWildcardsMsgBar: isOfflineSearch,
                hasRunningJob: res.HasRunningJob,
            },()=>{
                if(isOfflineSearch){
                    this.props.onShowMsg("info", RMResx.RM_HS_Offline_ActionUpdateTip, MsgComponentType.MsgBar);
                }
            });
        }); 
    }

    setSaveSuccessfulMsg(selectdViewId, isSaveDefautView) {
        let saveSuccessMsg = RMResx.RM_HS_Criteria_View_Msg_SaveAsDefaultSuccess;
        if (!isSaveDefautView) {
            saveSuccessMsg = selectdViewId ? RMResx.RM_HS_Criteria_View_Msg_SaveSuccess : RMResx.RM_HS_Criteria_View_Msg_SaveAsSuccess;
        }
        this.props.onShowMsg("success", saveSuccessMsg);
    }

    getJumpViewId(jumpParam){
        var data = jumpParam;
        switch(jumpParam){
            case SpecialSearchViewIds.Active:
                data = SpecialSearchViewIds.Active;
                break;
            case SpecialSearchViewIds.Archived:
                data = SpecialSearchViewIds.Archived;
                break;
            case SpecialSearchViewIds.SharePoint:
                data = SpecialSearchViewIds.SharePoint;
                break;
            case SpecialSearchViewIds.Exchange:
                data = SpecialSearchViewIds.Exchange;
                break;
            case SpecialSearchViewIds.FileSystem:
                data = SpecialSearchViewIds.FileSystem;
                break;
            case SpecialSearchViewIds.OneDrive:
                data = SpecialSearchViewIds.OneDrive;
                break;
            case SpecialSearchViewIds.Physical:
                data = SpecialSearchViewIds.Physical;
                break;
            case SpecialSearchViewIds.SharePointOnPrem:
                data = SpecialSearchViewIds.SharePointOnPrem;
                break;
                
        }
        return data;
    }

    loadDataBySearchView(selectdViewId) {
        $$.loading(true);
        let url =  "/api/PersonalSettinggApi/GetGlobalSearchCriteria";
        let data =  this.isExpireReturnDateSearch ? SpecialSearchViewIds.ReturnDate : selectdViewId;
        if(this.jumpParam && !this.isExpireReturnDateSearch){
            url = "/api/PersonalSettinggApi/GetDSBActiveOrArchivedCriteria";
            data = {id: this.getJumpViewId(this.jumpParam),ShowAll:this.isShowAll};
        }
        let option = {
            url: url,
            method: "POST",
            data: data
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.isExpireReturnDateSearch = false;//Return Date SearchView只有dashboard点击过期数据进入页面显示，其他view正常根据ViewId显示。
            if (res.Setting.ContentStr) {
                this.convertOldSerachSetting(res.Setting); //为了兼容老数据
            } else {
                if (res.Setting.AdvancedSearchs) {
                    this.echoSearchColumns(res.Setting);
                } else {
                    this.echoDefaultViewColumns(res);
                }
            }
            this.setOfflineSearch(res);
        });
    }

    setOfflineSearch(selectedViewInfo){
        let isOfflineSearch = selectedViewInfo.IsOffline;
        this.setState({
            showOfflineSearchJobsCombobox: isOfflineSearch,
            showWildcardsMsgBar: isOfflineSearch,
            hasRunningJob: selectedViewInfo.HasRunningJob,
        },()=>{
            if(isOfflineSearch){
                this.dispatch("raHsOfflineSearchJobs", selectedViewInfo);
                this.props.onShowMsg("info", RMResx.RM_HS_Offline_ActionUpdateTip, MsgComponentType.MsgBar);
            }
        });
        //后台返回profile list的isOffline不准，需要前台重组profile list；
        this.dispatch("raHSSearchView", "initSearchProfiles", selectedViewInfo);
    }

    onShareSearchView(operateViewParam){
        let selectedViewId = operateViewParam.selectedViewInfo.Id;
        let sharedGroupIds = operateViewParam.sharedGroupIds;
        let isCancelShare = operateViewParam.isCancelShare;
        let url = "/api/PersonalSettinggApi/ShareGlobalSearchCriteria";
        let param = {
            id: selectedViewId,
            securitygroups: sharedGroupIds
        };
        if(isCancelShare){
            param = selectedViewId;
            url =  "/api/PersonalSettinggApi/UnShareGlobalSearchCriteria";
        }
        let successfulMsg = isCancelShare ? RMResx.RM_HS_SearchView_CancelShareSuccess : RMResx.RM_HS_SearchView_ShareSuccess;
        $$.loading(true);
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if(!res.hasError){
                showToast.success(successfulMsg);
            }else{
                showToast.error(RMResx.RM_HS_Criteria_View_Msg_ValidOtherError);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    convertOldSerachSetting(oldSetting) {
        let oldContentStr = oldSetting.ContentStr;
        let queryOption = JSON.parse(oldContentStr).QueryOption;
        let allSearchColumns = RM.deepcopy(this.state.allSearchColumns);
        let defautSearchColumns = this.getDefautSearchColumns();
        let oldSearchOptions = queryOption.SearchOption ? queryOption.SearchOption.Columns : null;
        let oldFilterOptions = queryOption.FilterOption;
        let oldCustomColumns = queryOption.FilterOption.CustomColumns;
        let savedTableColumns = oldSetting.ColumnsStr ? JSON.parse(oldSetting.ColumnsStr) : null;

        // Old Hold Title Column Id : 6CEC839E-5FC3-4DBC-B3FA-8DD8454F1E6
        // New Hold Title Column Id : 3667DC37-36EE-40FD-AEE3-7BFE0F80A123
        if(!_.isNil(savedTableColumns)) {
            const oldHoldTitleColumnIndex = savedTableColumns.findIndex((i) => i === "6CEC839E-5FC3-4DBC-B3FA-8DD8454F1E6");
            if(oldHoldTitleColumnIndex > 0) {
                savedTableColumns[oldHoldTitleColumnIndex] = "3667DC37-36EE-40FD-AEE3-7BFE0F80A123";
            }
        }

        //search option回显
        let newSearchOptions = [];
        if (oldSearchOptions && queryOption.SearchOption.Key) {
            let newSearchOptionsIds = oldSearchOptions.map(item => { return item.value; });
            newSearchOptions = allSearchColumns.filter((item) => { return newSearchOptionsIds.includes(item.UniqueId); });
            for (let key in newSearchOptions) {
                let item = newSearchOptions[key];
                item.Value = queryOption.SearchOption.Key;
                item.ColumnsLogic = OperationLogicValues.And;
                item.ColumnOperationLogic = SearchKeyOperationLogic.Contains;
                item.idsWithDuplicateName = oldSearchOptions.find((oldItem) => {
                    return oldItem.value == item.UniqueId;
                }).idsWithDuplicateName;
            }
        }

        //回显default option
        let newBuildInColumns = [];
        if (oldFilterOptions) {
            let oldFilterOptionsKeys = [];
            for (let key in RM.deepcopy(oldFilterOptions)) {
                let oldFilterOption = RM.deepcopy(oldFilterOptions)[key];
                if (key == "ModifiedDateInfo") {
                    if (oldFilterOption.Condition != 0) { oldFilterOptionsKeys.push(key); }
                } else {
                    if (oldFilterOption !== null && oldFilterOption !== undefined) {
                        oldFilterOptionsKeys.push(key);
                    }
                }
            }

            //当数据存在WithOutTerms，TermIds属性都需要回显，term column，新数据中term的OldParamKey设置的是TermIds，因此转换。
            if (oldFilterOptionsKeys.includes("WithOutTerms") && !oldFilterOptionsKeys.includes("TermIds")) {
                oldFilterOptionsKeys.push("TermIds");
            }

            for (let item of defautSearchColumns) {
                if (item.OldParamKey && oldFilterOptionsKeys.includes(item.OldParamKey)) {
                    item.Value = oldFilterOptions[item.OldParamKey];
                    if (item.UniqueId == this.defaultColumnIds.Classification) {
                        item.TreeData = JSON.parse(oldSetting.TermTreeStr);
                        item.Value = { TermIds: oldFilterOptions[item.OldParamKey], WithOutTerms: oldFilterOptions["WithOutTerms"] };
                    }
                    if (item.UniqueId == this.defaultColumnIds.FileSystem) {
                        item.TreeData = JSON.parse(oldSetting.FSTreeStr);
                    }
                    newBuildInColumns.push(item);
                }
            }
        }

        //回显Custom Columns
        let newCustomColumns = [];
        if (oldCustomColumns) {
            for (let oldColumn of oldCustomColumns) {
                for (let column of allSearchColumns) {
                    if (column.NameHash && (column.NameHash == oldColumn.Column.NameHash)) {
                        oldColumn.Column.Value = JSON.parse(oldColumn.Value);
                        let newCustomColumn = Object.assign(column, oldColumn.Column);
                        if (
                            column.OptionsJSON && (column.ColumnType == PhysicalObjectColumnType.SingleChoice || column.ColumnType == PhysicalObjectColumnType.MultipleChoice)
                            && (column.UniqueId != PhysicalDefaultColumnIDs.Status)
                        ) {
                            let options = JSON.parse(column.OptionsJSON);
                            let optionNames = [];
                            for (let key in options) {
                                optionNames.push(options[key]);
                            }
                            let columnValue = column.Value.filter((item) => { return optionNames.includes(item.Value); });
                            column.Value = columnValue;
                        }
                        newCustomColumns.push(newCustomColumn);
                    }
                }
            }
        }

        //数据重组，老数据(1 Or 2) And 4 And 5转化为(1 And 4 And 5) Or (2 And 4 And 5)
        let selectedSearchColumns = [...newBuildInColumns, ...newCustomColumns];
        if (newSearchOptions.length > 0) {
            selectedSearchColumns = [];
            for (let searchOption of newSearchOptions) {
                let newFilterColumns = RM.deepcopy([...newBuildInColumns, ...newCustomColumns]);
                let lastFilterColumn = newFilterColumns[newFilterColumns.length - 1];
                lastFilterColumn.ColumnsLogic = OperationLogicValues.Or;
                selectedSearchColumns.push(searchOption, ...newFilterColumns);
            }
        }

        for (let item of selectedSearchColumns) {
            item.ComponentKey = StringUtil.newGuid();
            item.CurrentCriteriaId = StringUtil.newGuid();
        }

        this.setState({
            selectedSearchColumns: selectedSearchColumns,
            isAdvancedSearch: true
        }, () => {
            for (let searchColumn of this.state.selectedSearchColumns) {
                switch (searchColumn.UniqueId) {
                    case this.defaultColumnIds.Classification:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn.TreeData, searchColumn.Value);
                        break;
                    case this.defaultColumnIds.FileSystem:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn.TreeData);
                        break;
                    default:
                        this.dispatch(searchColumn.ComponentKey, searchColumn);
                }
            }
            this.setSearchColumnLogicText();
            this.onAdvancedSearch(savedTableColumns);
        });
    }

    echoSearchColumns(viewSetting) {
        let viewSearchColumns = viewSetting.AdvancedSearchs;
        let isAdvancedSearch = viewSetting.IsAdvancedSearch;
        let allSearchColumns = RM.deepcopy(this.state.allSearchColumns);
        let savedTableColumns = viewSetting.ColumnsStr ? JSON.parse(viewSetting.ColumnsStr) : null;
        let currentSortColumnInfo = viewSetting.ColumnSortSetting ? JSON.parse(viewSetting.ColumnSortSetting) : null;
        
        // Old Hold Title Column Id : 6CEC839E-5FC3-4DBC-B3FA-8DD8454F1E6
        // New Hold Title Column Id : 3667DC37-36EE-40FD-AEE3-7BFE0F80A123
        if(!_.isNil(savedTableColumns)) {
            const oldHoldTitleColumnIndex = savedTableColumns.findIndex((i) => i === "6CEC839E-5FC3-4DBC-B3FA-8DD8454F1E6");
            if(oldHoldTitleColumnIndex > 0) {
                savedTableColumns[oldHoldTitleColumnIndex] = "3667DC37-36EE-40FD-AEE3-7BFE0F80A123";
            }
        }     

        //将保存的参数字符串格式数据转换为数组格式
        let savedSearchParamList = [];
        for (let item of viewSearchColumns) {
            let searchContentObj = JSON.parse(item.ContentStr);
            if (item.TermTreeStr) {
                searchContentObj.TreeData = JSON.parse(item.TermTreeStr);
            }
            if (item.FSTreeStr) {
                searchContentObj.TreeData = JSON.parse(item.FSTreeStr);
            }
            if (item.SPTreeStr) {
                searchContentObj.TreeData = JSON.parse(item.SPTreeStr);
            }
            if (item.TeamsTreeStr) {
                searchContentObj.TreeData = JSON.parse(item.TeamsTreeStr);
            }
            if (item.GoogleTreeStr) {
                searchContentObj.TreeData = JSON.parse(item.GoogleTreeStr);
            }
            savedSearchParamList.push(searchContentObj);
        }

        //将保存的参数转换全信息的column data（与allSearchColumns比对获取）
        let selectedSearchColumns = [];
        let modifiedColNames = [];
        for (let savedColumn of RM.deepcopy(savedSearchParamList)) {
            let isCurrentColModified = true; //此参数判断当前回显的column在template是否被修改过。
            for (let column of RM.deepcopy(allSearchColumns)) {
                if (savedColumn.NameHash) {
                    if (savedColumn.NameHash == column.NameHash) {
                        isCurrentColModified = false;
                        let newColumn = Object.assign(column, savedColumn);
                        if (
                            newColumn.OptionsJSON && newColumn.Value && (newColumn.ColumnType == PhysicalObjectColumnType.SingleChoice || newColumn.ColumnType == PhysicalObjectColumnType.MultipleChoice)
                            && (newColumn.UniqueId != PhysicalDefaultColumnIDs.Status)
                        ) {
                            let options = JSON.parse(newColumn.OptionsJSON);
                            let optionNames = [];
                            for (let key in options) { optionNames.push(options[key]); }
                            let columnValueByPhyTemplate = JSON.parse(newColumn.Value).filter((item) => {
                                if (!optionNames.includes(item.Value)) { isCurrentColModified = true; }
                                return optionNames.includes(item.Value);
                            });
                            let columnValue = JSON.parse(newColumn.Value) ? columnValueByPhyTemplate : [];
                            newColumn.Value = JSON.stringify(columnValue);
                        }
                        selectedSearchColumns.push(newColumn);
                    }
                } else {
                    if (savedColumn.Column.Id == column.UniqueId) {
                        let newColumn = Object.assign(column, savedColumn);
                        selectedSearchColumns.push(newColumn);
                        isCurrentColModified = false;
                    }
                }
            }
            if (isCurrentColModified) {
                modifiedColNames.push(savedColumn.SavedColumnName);
            }
        }

        this.showFilterColModifiedInPhyTemplateMsg(modifiedColNames);

        for (let item of selectedSearchColumns) {
            item.ComponentKey = StringUtil.newGuid();
            item.Value = item.Value ? JSON.parse(item.Value) : "";
            item.CurrentCriteriaId = StringUtil.newGuid();
        }
        this.setState({
            selectedSearchColumns: selectedSearchColumns,
            isAdvancedSearch: isAdvancedSearch
        }, () => {
            for (let searchColumn of this.state.selectedSearchColumns) {
                switch (searchColumn.UniqueId) {
                    case this.defaultColumnIds.Classification:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn.TreeData, searchColumn.Value);
                        break;
                    case this.defaultColumnIds.FileSystem:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn.TreeData);
                        break;
                    case this.defaultColumnIds.SPOLocation:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn);
                        break;
                    case this.defaultColumnIds.TeamsLocation:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn.TreeData);
                        break;
                    case this.defaultColumnIds.GoogleLocation:
                        this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.InitData, searchColumn.TreeData);
                        break;
                    default:
                        this.dispatch(searchColumn.ComponentKey, searchColumn);
                }
            }
            this.setSearchColumnLogicText();
            if (!isAdvancedSearch) {
                this.isSearchBySetValue = true;
                this.refSearchbox.setValue(this.state.selectedSearchColumns[0].Value);
            }
            this.onAdvancedSearch(savedTableColumns, currentSortColumnInfo);
        });
    }

    showFilterColModifiedInPhyTemplateMsg(modifiedColNames) {
        if (modifiedColNames && modifiedColNames.length > 0) {
            let modifiedColMsg = modifiedColNames.join(", ");
            this.props.onShowMsg("warn", RMResx.RM_HS_Msg_ModifySearchConditionInPhyTemplate.format(modifiedColMsg), MsgComponentType.MsgBar);
        } else {
            if (!this.noNeedCloseMsg) {
                this.props.onShowMsg(null, null, MsgComponentType.MsgBar);
            }
            this.noNeedCloseMsg = false;
        }
    }

    echoDefaultViewColumns(echoSearchInfo) {
        let defautAdvancedSearchColumn = RM.deepcopy(this.defautAdvancedSearchColumn);
        defautAdvancedSearchColumn[0].ComponentKey = StringUtil.newGuid();
        this.setState({
            selectedSearchColumns: defautAdvancedSearchColumn,
            isAdvancedSearch: echoSearchInfo.IsAdvancedSearch
        }, () => {
            this.setSearchColumnLogicText();
            this.refSearchbox.clear();
        });
        this.props.onSearch(null, null);
    }

    onDeleteSearchView(selectdViewId) {
        $$.loading(true);
        let option = {
            url: "/api/PersonalSettinggApi/Delete",
            method: "POST",
            data: selectdViewId
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res) {
                this.dispatch("raHSSearchView", "loadSearchView");
                this.props.onShowMsg("success", RMResx.RM_HS_Criteria_View_Msg_DeleteSuccess);
            } else {
                this.props.onShowMsg("error", RMResx.RM_HS_Criteria_View_Msg_ValidOtherError);
            }
        });
    }

    showSaveViewDialogWithOfflineSearchTip(){
        this.hsSearchViewRef.onOperateSearchView(SearchViewTypes.SaveAs);
        this.dispatch("raHSSearchView", "showOfflineSearchTip", true);
    }

    onBaseSearch = (args) => {
        if (args === "") {
            this.onSearchStop();
            return;
        }

        if (!this.isSearchBySetValue) {
            let basedSearchColumns = RM.deepcopy(this.defautBasedSearchColumns);
            for (let column of basedSearchColumns) {
                column.Value = args;
            }
            this.setState({ selectedSearchColumns: basedSearchColumns }, () => {
                for (let searchColumn of basedSearchColumns) {
                    this.dispatch(searchColumn.ComponentKey, searchColumn);
                }
                if(this.allowRunSearchJob()){
                    this.showSaveViewDialogWithOfflineSearchTip();
                }else{
                    let searchParam = this.getSearchParam();
                    this.props.onSearch(searchParam);
                }
            });
            this.setSearchColumnLogicText();
        }
        this.isSearchBySetValue = false;
    }

    onSearchStop = () => {
        this.props.onSearch(null);
        this.setSearchColumnLogicText();
    }

    onAdvancedSearch = (savedTableColumns, currentSortColumnInfo) => {
        let searchParam = this.getSearchParam();
        if (searchParam && searchParam.length > 0) {
            if (this.state.isAdvancedSearch) {
                this.setState({ showSearchColumns: true });
                if (!this.isInValid(searchParam, false)) { return; }
            } else {
                if (!this.refSearchbox.getValue(true)) { searchParam = null; }
            }
            let allowRunSearchJob = this.allowRunSearchJob();
            if(savedTableColumns === undefined && allowRunSearchJob ){
                this.showSaveViewDialogWithOfflineSearchTip();
            }else{
                this.props.onSearch(searchParam, savedTableColumns, currentSortColumnInfo, allowRunSearchJob);
            }
            this.setState({ showSearchColumns: false });
        } else {
            //兼容在选中的只选了physical filter column，并且在template里面全部删除或修改了。
            this.setState({ selectedSearchColumns: RM.deepcopy(this.defautAdvancedSearchColumn) });
            this.props.onSearch(null, savedTableColumns, currentSortColumnInfo);
        }
    }

    allowRunSearchJob(){
        let selectedSearchColumns = this.state.selectedSearchColumns;
        let isAllowRunSearchJobColumnType = [PhysicalObjectColumnType.SingleText, PhysicalObjectColumnType.MutipleText];
        let isAllowRunSearchJob = selectedSearchColumns.some((item)=>{
            if(isAllowRunSearchJobColumnType.includes(item.ColumnType) && item.ColumnOperationLogic == SearchKeyOperationLogic.Contains){
                return item.Value && item.Value.includes("*") && (item.Value.split("*").length - 1 != item.Value.length);
            }
        });
        return isAllowRunSearchJob;
    }

    getSearchParam(isSaveView, baseSearchColumns) {
        let selectedSearchColumns = baseSearchColumns || RM.deepcopy(this.state.selectedSearchColumns);
        let searchColumnsParam = [];
        for (let index in selectedSearchColumns) {
            let searchColumn = selectedSearchColumns[index];
            let columnParam = {};
            columnParam.Value = JSON.stringify(searchColumn.Value);
            if (searchColumn.ColumnOperationLogic != undefined) {
                columnParam.ColumnOperationLogic = searchColumn.ColumnOperationLogic;
            }
            if (index != selectedSearchColumns.length - 1) {
                columnParam.ColumnsLogic = searchColumn.ColumnsLogic;
            }

            let columnParamInner = {};
            columnParamInner.Id = searchColumn.UniqueId;
            if (!this.defaultColumnIdsList.includes(searchColumn.UniqueId) || searchColumn.UniqueId == this.defaultColumnIds.LoanBy) {
                columnParamInner.Type = searchColumn.ColumnType;
                columnParamInner.IdsWithDuplicateName = searchColumn.IdsWithDuplicateName;
            }
            columnParam.Column = columnParamInner;
            columnParam.NameHash = searchColumn.NameHash;
            //进行SaveView需要给后台传tree的数据
            if (isSaveView) {
                let saveViewColumnObj = {};
                columnParam.SavedColumnName = searchColumn.ColumnName;
                if (searchColumn.UniqueId == this.defaultColumnIds.Classification) {
                    saveViewColumnObj.TermTreeStr = JSON.stringify(this[searchColumn.ComponentKey].treeData);
                }
                if (searchColumn.UniqueId == this.defaultColumnIds.FileSystem) {
                    saveViewColumnObj.FSTreeStr = JSON.stringify(searchColumn.TreeData);
                }
                if (searchColumn.UniqueId == this.defaultColumnIds.SPOLocation && searchColumn.ColumnOperationLogic == LocationOperationLogic.Contains) {
                    saveViewColumnObj.SPTreeStr = JSON.stringify(searchColumn.TreeData);
                }
                if (searchColumn.UniqueId == this.defaultColumnIds.TeamsLocation) {
                    saveViewColumnObj.TeamsTreeStr = JSON.stringify(searchColumn.TreeData);
                }
                if (searchColumn.UniqueId == this.defaultColumnIds.GoogleLocation) {
                    saveViewColumnObj.GoogleTreeStr = JSON.stringify(searchColumn.TreeData);
                }
                saveViewColumnObj.ContentStr = JSON.stringify(columnParam);
                searchColumnsParam.push(saveViewColumnObj);
            } else {
                searchColumnsParam.push(columnParam);
            }
            this.validFilterComponent(searchColumn);
        }
        return searchColumnsParam;
    }

    getBaseSearchParam() {
        let defautBasedSearchColumns = RM.deepcopy(this.defautBasedSearchColumns);
        let defautAdvancedSearchColumn = RM.deepcopy(this.defautAdvancedSearchColumn);
        let searchboxValue = this.refSearchbox.getValue(true);
        let searchColumns = searchboxValue ? defautBasedSearchColumns : defautAdvancedSearchColumn;
        for (let item of searchColumns) { item.Value = searchboxValue; }
        return this.getSearchParam(true, searchColumns);
    }

    validFilterComponent(searchColumn) {
        if (!searchColumn.Value) {
            if (searchColumn.UniqueId == this.defaultColumnIds.Classification
                || searchColumn.UniqueId == this.defaultColumnIds.Type
                || searchColumn.UniqueId == this.defaultColumnIds.FileSystem
                || searchColumn.UniqueId == this.defaultColumnIds.SPOLocation
            ) {
                this.dispatch(searchColumn.ComponentKey, ToSearchComponentDispatchType.Valid);
            }
        }
    }

    onSearchSourceChange = (column, args) => {
        column.Value = args;
    }

    onSearchTypeChange = (column, args) => {
        column.Value = args;
    }

    onSearchTermsChange = (column, termsInfo, termTreeData) => {
        column.Value = termsInfo;
        column.TreeData = termTreeData;
    }

    onSearchSingleAndMultText = (column, args) => {
        column.ColumnOperationLogic = args.ColumnOperationLogic;
        column.Value = args.Value;
        this.isShowWildcardsMsgBar();
    }

    isShowWildcardsMsgBar(){
        let showWildcardsMsgBar = this.allowRunSearchJob();
        this.setState({showWildcardsMsgBar: showWildcardsMsgBar});
    }

    onSearchDateTimeColChange = (column, args) => {
        if (args) {
            let dateTimeInfo = RM.deepcopy(args);
            let timeZoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
            dateTimeInfo.Condition = dateTimeInfo.Condition * 1; //需要传int类型
            if (dateTimeInfo.Condition == DateConditions.Before
                || dateTimeInfo.Condition == DateConditions.After
                || dateTimeInfo.Condition == DateConditions.FromTo
            ) {
                dateTimeInfo.TimeZoneId = timeZoneInfo.id;
                dateTimeInfo.IsDayLight = timeZoneInfo.autoAdjustClock;
            }
            column.Value = dateTimeInfo;
        } else {
            column.Value = args;
        }
    }

    onSearchMultipleColChange = (column, args) => {
        if (column.ColumnType === PhysicalObjectColumnType.YesOrNo) {
            column.Value = args.newValue ? args.newValue.id === "0" : null;
            return;
        }
        switch (column.UniqueId) {
            case this.defaultColumnIds.HoldStatus:
            case this.defaultColumnIds.DeclaredRecord:
            case this.defaultColumnIds.LockedByRecordLabel:
            case this.defaultColumnIds.ContentArchived:
                if (args.newValue) {
                    column.Value = args.newValue.id === "0";
                } else {
                    column.Value = null;
                }
                break;
            default:
                if (column.UniqueId == PhysicalDefaultColumnIDs.Status) {
                    column.Value = args.newValue.map((item) => { return { "Value": item.id }; });
                } else {
                    column.Value = args.newValue.map((item) => { return { "Value": item.name }; });
                }
        }
    }

    getDefaultPeopleItem() {
        return {
            UserId: null,
            UserName: null,
            UserPrincipalName: null,
            Email: null,
            DisplayName: null,
            InviteType: null,
            RMUserId: null,
            Id: null,
            SurName: null,
            GivenName: null,
            TenantId: null,
        };
    }

    getPeopleItems(userInfoList, attr) {
        let newUserInfoList = [];
        for (let userInfo of userInfoList) {
            let newUserInfo = RM.deepcopy(this.getDefaultPeopleItem());
            if (userInfo.UserId || userInfo.Id) {
                for(let key in newUserInfo){
                    newUserInfo[key] = userInfo[key];
                }
            } else {
                newUserInfo.RMUserId = 0;
                newUserInfo.InviteType = 0;
                newUserInfo.DisplayName = userInfo.DisplayName;
                newUserInfo[attr] = userInfo.DisplayName; //手动输入时的赋值
            }
            newUserInfoList.push(newUserInfo);
        }
        return newUserInfoList;
    }

    onSearchPeopleColChange = (column, args) => {
        switch (column.UniqueId) {
            case this.defaultColumnIds.CreatedBy:
            case this.defaultColumnIds.LoanedBy:
                column.Value = this.getPeopleItems(args, "DisplayName");
                break;
            case this.defaultColumnIds.ModifiedBy:
                column.Value = this.getPeopleItems(args, "DisplayName");
                break;
            case this.defaultColumnIds.Owners:
                column.Value = this.getPeopleItems(args, "RMUserId");
                break;
            default:
                column.Value = this.getPeopleItems(args, "UserPrincipalName");
        }
    }

    onSearchNumberColChange = (column, args) => {
        column.Value = args;
    }

    onSearchFileSystem = (column, args, fsTreeData) => {
        column.Value = args;
        column.TreeData = fsTreeData;
    }

    onSearchSPOLocation = (column, args, treeData) => {
        column.ColumnOperationLogic = args.ColumnOperationLogic
        column.Value = args.Value;
        column.TreeData = treeData;
    }

    onSearchTeamsLocation = (column, args, teamsTreeData) => {
        column.Value = args;
        column.TreeData = teamsTreeData;
    }

    onSearchGoogleLocation = (column, args, googleTreeData) => {
        column.Value = args;
        column.TreeData = googleTreeData;
    }

    onSearchPhyTemplates = (column, args) => {
        column.Value = args;
    }

    onSelectFilterColumn(args, index) {
        let newSearchColumn = args.newValue;
        newSearchColumn.CurrentCriteriaId = this.state.selectedSearchColumns[index].CurrentCriteriaId;
        this.state.selectedSearchColumns[index] = newSearchColumn;
        this.state.selectedSearchColumns[index].ComponentKey = StringUtil.newGuid();
        this.state.selectedSearchColumns[index].Value = [];
        this.setState({ selectedSearchColumns: RM.deepcopy(this.state.selectedSearchColumns) }, () => {
            this.setSearchColumnLogicText();
        });
        this.isShowWildcardsMsgBar();
    }

    getMultipleColumnOption(options) {
        let mulColumnOption = [];
        if (options) {
            for (let key in options) {
                mulColumnOption.push({
                    id: key,
                    name: StringUtil.toI18N(options[key]),
                });
            }
        }
        return mulColumnOption;
    }

    onSwicthSearchType = (toAdvanceSearch) => {
        if (!toAdvanceSearch && this.state.backBaseBtnDisabled) {
            return;
        }
        this.setState({
            isAdvancedSearch: toAdvanceSearch,
            showSearchColumns: toAdvanceSearch
        });

        if (!toAdvanceSearch) {
            this.onSearchStop();
            this.setState({ selectedSearchColumns: RM.deepcopy(this.defautAdvancedSearchColumn) });
            this.onClearSearchColumn();
            this.dispatch("raHSSearchView", "refreshSearchViews");
        }
    }

    onKeyDown = (e) => {
        if (e.keyCode === 13) {
            e.target.click();
        }
    }

    onAdvanceSearchCounterClick = () => {
        this.setState({ showSearchColumns: !this.state.showSearchColumns });
    }

    onAddFilterColumn = () => {
        let selectedSearchColumns = RM.deepcopy(this.state.selectedSearchColumns);
        let defautAdvancedSearchColumn = RM.deepcopy(this.defautAdvancedSearchColumn);
        defautAdvancedSearchColumn[0].ComponentKey = StringUtil.newGuid();
        defautAdvancedSearchColumn[0].CurrentCriteriaId = StringUtil.newGuid();
        selectedSearchColumns.push(defautAdvancedSearchColumn[0]);
        this.setState({ selectedSearchColumns: selectedSearchColumns }, () => {
            this.setSearchColumnLogicText();
        });
    }

    onDeleteFilterColumn(index) {
        let selectedSearchColumns = RM.deepcopy(this.state.selectedSearchColumns);
        selectedSearchColumns.splice(index, 1);
        this.setState({ selectedSearchColumns: selectedSearchColumns }, () => {
            this.setSearchColumnLogicText();
        });
        this.isShowWildcardsMsgBar();
    }

    swicthLogicButton(item, logicValue) {
        let selectedSearchColumns = this.state.selectedSearchColumns;
        item.ColumnsLogic = logicValue;
        this.setState({ selectedSearchColumns: selectedSearchColumns }, () => {
            this.setSearchColumnLogicText();
        });
    }

    getSearchColumnsNamesOptions(column) {
        let searchColumnsNamesOptions = RM.deepcopy(this.state.allSearchColumns);
        for (let option of searchColumnsNamesOptions) {
            option.checked = option.UniqueId == column.UniqueId;
        }
        return searchColumnsNamesOptions;
    }

    getSearchColumnLogicOptions(value) {
        let logicOptions = RM.deepcopy(this.logicOptions);
        if (value) {
            for (let option of logicOptions) {
                option.checked = option.value == value;
            }
        } else {
            logicOptions[0].checked = true;
        }
        return logicOptions;
    }

    setSearchColumnLogicText() {
        let logicOptions = RM.deepcopy(this.logicOptions);
        let selectedSearchColumns = this.state.selectedSearchColumns;
        let logicNames = [];
        for (let index in selectedSearchColumns) {
            let item = selectedSearchColumns[index];
            if (!item.ColumnsLogic) {
                item.ColumnsLogic = OperationLogicValues.And;
            }
            let selectedLogicOption = logicOptions.find(option => { return item.ColumnsLogic === option.value; });
            if (selectedSearchColumns.length > 1) {
                if (index != selectedSearchColumns.length - 1) {
                    logicNames.push(`${(index * 1 + 1)} ${selectedLogicOption.name}`);
                } else {
                    logicNames.push(`${(index * 1 + 1)}`);
                }
            }
        }
        let searchColumnLogicText = logicNames.join(" ");
        if (searchColumnLogicText.includes(RMResx.RM_HS_SearchKeywordOr)) {
            searchColumnLogicText = "( " + logicNames.join(" ").replace(new RegExp(RMResx.RM_HS_SearchKeywordOr, 'g'), `) ${RMResx.RM_HS_SearchKeywordOr} (`) + " )";
        }
        this.setState({ searchColumnLogicText: searchColumnLogicText });
    }

    onClearSearchColumn = () => {
        this.defautAdvancedSearchColumn[0].ComponentKey = StringUtil.newGuid();
        this.setState({ 
            selectedSearchColumns: RM.deepcopy(this.defautAdvancedSearchColumn),
            showWildcardsMsgBar: false
        }, () => {
            this.setSearchColumnLogicText();
        });
    }

    renderOfflineSearchJobsCombobox(){
        if(this.state.showOfflineSearchJobsCombobox){
            return <HSOfflineSearchJobs 
                id="raHsOfflineSearchJobs"
                onSearchByProfileId={this.startOfflineSearchJob}
                onSearchByOfflineJob={this.onSearchByOfflineJob}>
            </HSOfflineSearchJobs>;
        }
    }

    renderSearchHeader() {
        let isAdvancedSearch = this.state.isAdvancedSearch;
        let selectedSearchColumnCount = this.state.selectedSearchColumns.length;
        let backBaseBtnColor = this.state.backBaseBtnDisabled ? "hs-back-baselink-color" : "";
        // let backSearchBtnTooltip = this.state.backBaseBtnDisabled ? RMResx.RM_HS_ToBaseSearchBtnDisabledMsg : ""; 
        let backSearchBtnTooltip = RMResx.RM_HS_ToBaseSearchBtnDisabledMsg; // "^This will clear all filters except the Name filter."
        let searchboxWidth = window.screen.width > 1366 ? 380 : 190;
        return < div className='ra-main-header'>
            <div className="hs-search-column">
                {
                    <div className={!isAdvancedSearch ? "hs-search-column" : "none"}>
                        <div className="margin-right-l">
                            <R.Searchbox
                                ref={r => this.refSearchbox = r}
                                placeholder={RMResx.RM_HS_BaseSearchBoxPlaceholder}
                                onSearch={this.onBaseSearch}
                                width={searchboxWidth}
                            />
                        </div>
                        <div id="raHsSwicthSearchTypeBtn" 
                            className="hs-swicth-searchtype-link" 
                            role="button" 
                            tabIndex="0" 
                            onClick={this.onSwicthSearchType.bind(this, true)}
                            onKeyDown={this.onKeyDown}>
                            {RMResx.RM_HS_AdvancedSearchText}
                        </div>
                    </div>
                }
                {
                    <div className={isAdvancedSearch ? "hs-search-column" : "none"}>
                        <div id="raHsSwicthSearchTypeBtn"
                            role="button"
                            className={`hs-swicth-searchtype-link ${backBaseBtnColor}`}
                            tabIndex="0"
                            onClick={this.onSwicthSearchType.bind(this, false)}
                            onKeyDown={this.onKeyDown}
                            data-tooltip
                            aria-label={this.state.backBasicSearchAria}
                            onMouseEnter={() => {
                                this.setState({ backBasicSearchAria: `${backSearchBtnTooltip}` })
                            }}
                            onMouseLeave={() => {
                                this.setState({ backBasicSearchAria: `${RMResx.RM_HS_BackBasicSearch} ${backSearchBtnTooltip}` })
                            }}
                        >
                            {RMResx.RM_HS_BackBasicSearch}
                        </div>
                        <div 
                            id="raHsOpenSearchConditionBtn"
                            tabIndex="0"
                            className="hs-swicth-searchtype-link margin-left-l" 
                            onKeyDown={this.onKeyDown}
                            onClick={this.onAdvanceSearchCounterClick}
                        >
                            {RMResx.RM_HS_SearchConditionCounter.format(selectedSearchColumnCount)}
                        </div>
                    </div>
                }
            </div>
            <div className="flex">
                {this.props.children}
                {this.state.showSearchView && 
                <HSSearchView 
                    id="raHSSearchView"
                    ref={r=> this.hsSearchViewRef = r } 
                    onOpenSearchView={this.onOpenSearchView}
                    onOperate={this.onOperateSearchView} 
                    isExpireReturnDateSearch={this.props.isExpireReturnDateSearch}
                />}
                {this.renderOfflineSearchJobsCombobox()}
            </div>
        </div >;
    }

    renderSingleAndMultTextColumn(column) {
        return <HSSingleAndMultText
            id={column.ComponentKey}
            onChange={this.onSearchSingleAndMultText.bind(this, column)}
        >
        </HSSingleAndMultText>;
    }

    renderDateTimeColumn(column) {
        let isOnlyShowDateAndTime = (column.UniqueId != this.defaultColumnIds.ActionDueDate);
        let otherConditions = "";
        if(column.UniqueId == this.defaultColumnIds.ReturnDate ){
            otherConditions =[
                { name: RMResx.RM_HS_ReturnDateExpired, value: DateConditions.Overdue, checked: false },
                { name: RMResx.RM_HS_ReturnDateNotSpecified, value: DateConditions.NotSpecified, checked: false }
            ];
        }
        return <HSFilteDateAndTime
            id={column.ComponentKey}
            otherConditions={otherConditions}
            onlyShowDateAndTime={isOnlyShowDateAndTime}
            onChange={this.onSearchDateTimeColChange.bind(this, column)}
        />;
    }

    renderMultipleChoiceColumn(column) {
        let options = [];
        let isSingleChoice = false;
        let boolSingleChoiceColumnTypeList = [this.defaultColumnIds.HoldStatus, this.defaultColumnIds.DeclaredRecord, this.defaultColumnIds.LockedByRecordLabel, this.defaultColumnIds.ContentArchived];
        if (boolSingleChoiceColumnTypeList.includes(column.UniqueId) || column.ColumnType === PhysicalObjectColumnType.YesOrNo) {
            // column.ColumnType === PhysicalObjectColumnType.YesOrNo: Only use for custom column yes/no (new: August 2025)
            options = this.getMultipleColumnOption(this.yesOrNoOptions);
            isSingleChoice = true;
        } else {
            options = this.getMultipleColumnOption(JSON.parse(column.OptionsJSON));
        }
        return <HSMultipleChoice
            id={column.ComponentKey}
            isSingleChoice={isSingleChoice}
            options={options}
            onChange={this.onSearchMultipleColChange.bind(this, column)}
        />;
    }

    renderPeopleOrGroupColumn(column) {
        return <div className="flex">
            <div className="flex-1">
                <R.Input
                    type="text"
                    value={RMResx.RM_HS_Contains}
                    width={"100%"}
                    height={40}
                    readonly={true}
                />
            </div>
            <div className="flex-1 margin-left-m">
                <R.Validation element="RichCombobox" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                    <PeoplePicker
                        height={40}
                        width={"100%"}
                        items={column.Value}
                        selectionChanged={this.onSearchPeopleColChange.bind(this, column)}
                        isAllowCustomizeUser={true} 
                    />
                </R.Validation>
            </div>
        </div>;
    }

    renderNumberColumn(column) {
        return <HSFilterNumber
            id={column.ComponentKey}
            onChange={this.onSearchNumberColChange.bind(this, column)}
        >
        </HSFilterNumber>;
    }


    renderSearchActionBtn(key) {
        let searchColumnsCount = this.state.selectedSearchColumns.length;
        let isLastColumn = key == searchColumnsCount - 1;
        let isShowDeleteBtn = searchColumnsCount > 1;
        let isDisabledAddBtn = searchColumnsCount > 9;
        let addBtnTooltip = isDisabledAddBtn ? RMResx.RM_HS_SearchColumnLimitCount : RMResx.RM_UI_Detail_Add;
        return <div className="margin-left-m hs-filter-column-btn" >
            {
                isLastColumn && <R.Button
                    type="bald"
                    icon="fia-plus"
                    className="add-btn"
                    tooltip={addBtnTooltip}
                    disabled={isDisabledAddBtn}
                    onClick={this.onAddFilterColumn} />
            }
            {
                isShowDeleteBtn && <R.Button
                    type="bald"
                    icon="fia-close"
                    className="delete-btn"
                    tooltip={RMResx.RM_HS_Criteria_View_Btn_Delete_View}
                    onClick={this.onDeleteFilterColumn.bind(this, key)} />
            }
        </div>;
    }

    renderColumn(column, key) {
        let searchColumnsCount = this.state.selectedSearchColumns.length;
        let isLastColumn = key == searchColumnsCount - 1;
        let orderNumber = `${key * 1 + 1}.`;
        let columnHtml = "";
        switch (column.ColumnType) {
            case PhysicalObjectColumnType.SingleText:
            case PhysicalObjectColumnType.MutipleText:
                columnHtml = this.renderSingleAndMultTextColumn(column, key);
                break;
            case PhysicalObjectColumnType.DateTime:
                columnHtml = this.renderDateTimeColumn(column, key);
                break;
            case PhysicalObjectColumnType.PeopleOrGroup:
                columnHtml = this.renderPeopleOrGroupColumn(column);
                break;
            case PhysicalObjectColumnType.SingleChoice:
            case PhysicalObjectColumnType.MultipleChoice:
            case PhysicalObjectColumnType.YesOrNo:
                columnHtml = this.renderMultipleChoiceColumn(column);
                break;
            case PhysicalObjectColumnType.Number:
                columnHtml = this.renderNumberColumn(column, key);
                break;
        }
        switch (column.UniqueId) {
            case this.defaultColumnIds.SourceFlag:
                columnHtml = <HSFilterSource id={column.ComponentKey} index={key} options={this.state.sourceColumnOptions} onChange={this.onSearchSourceChange.bind(this, column)} />;
                break;
            case this.defaultColumnIds.Type:
                columnHtml = <HSFilteType id={column.ComponentKey} onChange={this.onSearchTypeChange.bind(this, column)} />;
                break;
            case this.defaultColumnIds.Classification:
                columnHtml = <HSFilterClassification id={column.ComponentKey} ref={r => this[column.ComponentKey] = r} onChange={this.onSearchTermsChange.bind(this, column)} />;
                break;
            case this.defaultColumnIds.FileSystem:
                columnHtml = <HSFileSystemFolder id={column.ComponentKey} onChange={this.onSearchFileSystem.bind(this, column)} />;
                break;
            case this.defaultColumnIds.SPOLocation:
                columnHtml = <HSSPOLocation id={column.ComponentKey} onChange={this.onSearchSPOLocation.bind(this, column)} />;
                break;
            case this.defaultColumnIds.TeamsLocation:
                columnHtml = <HSTeamsLocation id={column.ComponentKey} onChange={this.onSearchTeamsLocation.bind(this, column)} />;
                break;
            case this.defaultColumnIds.GoogleLocation:
                columnHtml = <HSGoogleLocation id={column.ComponentKey} onChange={this.onSearchGoogleLocation.bind(this, column)} />;
                break;
            case this.defaultColumnIds.PhyTemplates:
                columnHtml = <HSPhyTemplates id={column.ComponentKey} templateList={this.state.phyTemplates} onChange={this.onSearchPhyTemplates.bind(this, column)} />;
                break;
        }

        let searchColumnsNamesOptions = this.getSearchColumnsNamesOptions(column);
        let searchColumnlogicOptions = this.getSearchColumnLogicOptions(column.ColumnsLogic);
        return <div key={column.CurrentCriteriaId}>
            <div className="flex-start">
                <div className="order-number">{orderNumber}</div>
                <div className="flex-1 width-0">
                    <R.Combobox
                        id={"raHsFilterColumnName" + key}
                        height={40}
                        width={"100%"}
                        textField='ColumnName'
                        valueField='UniqueId'
                        checkedField='checked'
                        tooltipField="title"
                        items={searchColumnsNamesOptions}
                        onChange={(args) => this.onSelectFilterColumn(args, key)}
                    />
                    {
                        !isLastColumn && <div className="hs-search-column-logic">
                            {searchColumnlogicOptions.map((item, index) => {
                                return <div
                                    tabIndex="0"
                                    role="button"
                                    key={index}
                                    className={item.checked ? "logic-btn-ckecked" : "logic-button"}
                                    onClick={this.swicthLogicButton.bind(this, column, item.value)}
                                >
                                    {item.name}
                                </div>;
                            })}
                        </div>
                    }
                </div>
                <div key={column.ComponentKey} className="flex-2 margin-left-m">
                    {columnHtml}
                </div>
                {this.renderSearchActionBtn(key)}
            </div>
        </div>;
    }

    renderFilterColumns() {
        let selectedSearchColumns = this.state.selectedSearchColumns;
        return selectedSearchColumns.length > 0 && <div className="ra-hs-filter-selected-columns">
            {selectedSearchColumns.map((column, key) => {
                return this.renderColumn(column, key);
            })}
            <div className="wildcards-messagebar">
                <$g.MessageBar show={this.state.showWildcardsMsgBar}>
                    {RMResx.RM_HS_Offline_SearchWithWildcardTip}
                </$g.MessageBar>
            </div>
        </div>;
    }

    renderSearchFoot() {
        return <div className="flex-between margin-top-l">
            <div className="hs-filter-logic-text" tabIndex="0">
                {this.state.searchColumnLogicText}
            </div>
            <div className="flex hs-filter-logic-btn">
                <R.Button text={RMResx.RM_JS_Common_ClearAll} icon="fia-clear" onClick={this.onClearSearchColumn} />
                <R.Button text={RMResx.RM_JS_TM_SearchTxt} id="raHsSearchBtn" primary={true} classify="theme" icon="fia-search" onClick={() => { this.onAdvancedSearch(); }} />
            </div>
        </div>;
    }

    render() {
        let showSearchColumns = this.state.isAdvancedSearch && this.state.showSearchColumns;
        let hideClass = showSearchColumns ? "" : "none";
        return < React.Fragment >
            <R.Validation>
                <div id="hsFilter">
                    {this.renderSearchHeader()}
                    {
                        <React.Fragment>
                            <div className={`hs-advanced-search-popup ${hideClass}`}>
                                {this.renderFilterColumns()}
                                {this.renderSearchFoot()}
                                <div className="hs-search-popup-perchBlock"></div>
                            </div>
                        </React.Fragment>
                    }
                </div>
            </R.Validation>
        </React.Fragment >;
    }
}