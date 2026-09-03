import * as Constants from "./Constants";
import { RuleModuleTypes } from "./Constants";
import Enviroments from '../../../../Constants/Enviroments';
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import { filterRuleTypesByLicense } from "../../../../Utilities/RuleTypeUtil";

export default class RuleCriteria extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.Matchs1 = Constants.Matchs1;
        this.levels = Constants.levels;
        this.criteriaIndex = 0;
        this.rulTypes = filterRuleTypesByLicense(Constants.rulTypes);
        this.spLocalRulTypes = Constants.SPLocalRulTypes;
        this.oneDriveRuleTypes = filterRuleTypesByLicense(Constants.oneDriveRuleTypes4SO);
        this.teamsRuleTypes = this.filter21VCriteriaForTeams(Constants.TeamsRuleTypes);
        this.exoRulTypes = this.filter21Vcriteria(Constants.exoRulTypes);
        this.phyRulTypes = Constants.phyRulTypes;
        this.fsRulTypes = this.getFsRuleTypes();
        this.azureFileRulTypes = Constants.AzureFileRulTypes[4194304];
        this.boxRulTypes = Constants.BoxRulTypes[8388608];
        this.googleDriveRuleTypes = Constants.GoogleDriveRuleTypes[16777216];
        this.connectorRulTypes = Constants.ConnectorRulTypes;
        this.phyLevelIds = Constants.phyLevelIds;
        this.Regexs = Constants.Regexs;
        this.RuleType = Constants.RuleType;
        this.dateOption = Constants.dateOption;
        this.ConditionType = Constants.ConditionType;
        this.unitSize = Constants.unitSize;
        this.compare = Constants.compare;
        this.Condition = {
            id: 0,
            isColumn: false, //Archive the content when  All of these criteria are met. 第一个 input
            isText: true,  //第二个 input
            isDate1: false,
            isDate2: false,
            isMath1: true,  //content 等
            isMath2: false,  //kb，gb 等
            isValid: false,
            isDateValid: false,
            isArray: false,
            noDateValue: false,
            conditionId: 8,
            ruleTypeId: 40,
            currentTimeZone: RM.TimeUtil.getGlobalTimezoneInfo(),
            currentTimeZoneId: RM.TimeSettingModel.TimeZoneId,
            filterName: "",
            Value1: "",  //第二个input value
            valueUnit: 1,
            Value2: "",
            Value3: "",
            curLevelId: this.levels[0].id,
            conditionTypes: this.exoRulTypes[6553601],
            Matchs1: this.Matchs1,
            Matchs2: [],
            currentType: this.exoRulTypes[6553601][0],
            currentMatch1: this.Regexs[0],
            currentMatch2: this.Regexs[0],
            CombineMode: 0,
            notNumber: false,
            isExceedFiveThousandsYears: false,
            currentDate1: null,
            currentDate2:null,
            isConflict: false,
            isConflictValue: false,
            RuleType: 1,
            columnNamePlaceholder: RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue,
            columnValuePlaceholder: RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue,
        };
        this.ruleTypeId = 1;  //name 选中的type
        this.dateTimeFormat = RM.TimeUtil.getGlobalAuiFormat();//时间格式
        this.hasChanged = false;
        this.levelId = 64;  //默认的levelId
        this.criterias = [];
        this.autoColumnWidth = 80;
        this.autoNameColumnWidth = 100;
        this.logicOptions = [
            { name: RMResx.RM_HS_SearchKeywordAnd, value: Constants.OperationLogicValues.And, checked: true },
            { name: RMResx.RM_HS_SearchKeywordOr, value: Constants.OperationLogicValues.Or, checked: false }
        ];
        this.state = {
            criterias: [],
            noCondition: false,
            TrueOrFaseOptions: Constants.TrueOrFaseOptions
        };
    }

    componentCreate() {
        this.bind(["dateTimeBeforeSelectChange", "dateTimeAfterSelectChange"]);
    }

    componentReceive(action, data, type) {
        switch (action) {
            case Constants.dispatchAction.save: {
                let saveData = null;
                let isVerificationPassed = this.archiveContentCustomValidate();
                if (isVerificationPassed) {
                    saveData = this.convertRuleFilter(this.props.itemId);
                    this.props.getIsVerificationPassed(isVerificationPassed);
                } else {
                    this.props.getIsVerificationPassed(false);
                }
                this.props.getCriteriaData(saveData);
                break;
            }
            case Constants.dispatchAction.setData:
                this.levelId = data.RuleLevel;
                this.moduleType = data.ModelType;
                
                if (data.ModelType == RuleModuleTypes.SOArchiver) {
                    this.rulTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.rulTypes4SO21V : Constants.rulTypes4SONormal);
                    this.oneDriveRuleTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.oneDriveRuleTypes4SO21V : Constants.oneDriveRuleTypes4SO);
                } else {
                    this.rulTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.rulTypes21V : Constants.rulTypes);
                    this.oneDriveRuleTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.oneDriveRuleTypes21V : Constants.oneDriveRuleTypes);
                }

                // this.rulTypes = data.ModelType == RuleModuleTypes.SOArchiver ? Constants.rulTypes4SO : Constants.rulTypes;
                this.setConditionData(data);
                this.setCriteriaColumnLogicText();
                if (this.props.onChange) {
                    this.props.onChange(this.criterias);
                }
                break;
            case Constants.dispatchAction.elementDisabled:
                this.setState({elementsEnable: data});
                break;
            case Constants.dispatchAction.clearData:
                this.criterias = [];
                this.levelId = data;
                this.moduleType = type;
                this.setState({noCondition: false,});
                this.addCondition({}, 0, true);
                break;
            case Constants.dispatchAction.selectedModuleType:
                this.moduleType = data;

                if (data == RuleModuleTypes.SOArchiver) {
                    this.rulTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.rulTypes4SO21V : Constants.rulTypes4SONormal);
                    this.oneDriveRuleTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.oneDriveRuleTypes4SO21V : Constants.oneDriveRuleTypes4SO);
                } else {
                    this.rulTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.rulTypes21V : Constants.rulTypes);
                    this.oneDriveRuleTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.oneDriveRuleTypes21V : Constants.oneDriveRuleTypes);
                }

                // this.rulTypes = data == RuleModuleTypes.SOArchiver ? Constants.rulTypes4SO : Constants.rulTypes;
                this.criterias = [];
                this.addCondition({}, 0, true);
                break;
        }
    }

    filter21VCriteriaForTeams(criterias) {
        if (RM.gData.enviromentName == Enviroments.ChinaNorth) {
            criterias[33554432] = criterias[33554432].filter((item) => item.id !== 49);
        }
        return criterias;
    }

    filter21Vcriteria(criterias){
        if(RM.gData.enviromentName == Enviroments.ChinaNorth){
            criterias[6553601] = criterias[6553601].filter((item) => item.id !== 47);
        }        
        return criterias;
    }

    getFsRuleTypes() { 
        return LicenseHelper.EnableJPMCFileSystemFeature() ? Constants.JPMCFsRuleTypes[64] : Constants.FsRulTypes[64];
    }

    criteriaClick(index) {
        this.criteriaIndex = index;
    }

    setConditionData(data) {
        let RuleFilters = [];
        this.criterias = [];
        switch (this.props.itemId) {
            case 'sp':
                if (data.RuleFilters != null && data.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.RuleFilters);
                    for (let key in RuleFilters) {
                        if (RuleFilters[key]) {
                            this.addCondition(RuleFilters[key], key);
                        }
                    }
                }
                break;
            case 'spLocal':
                if (data.SPLocalRule != null && data.SPLocalRule.RuleFilters != null && data.SPLocalRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.SPLocalRule.RuleFilters);
                    for (let key in data.SPLocalRule.RuleFilters) {
                        if (data.SPLocalRule.RuleFilters[key]) {
                            this.addCondition(RuleFilters[key], key);
                        }
                    }
                }
                break;
            case 'oneDrive':
                if (data.OneDriveRule != null && data.OneDriveRule.RuleFilters != null && data.OneDriveRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.OneDriveRule.RuleFilters);
                    for (let key in data.OneDriveRule.RuleFilters) {
                        if (data.OneDriveRule.RuleFilters[key]) {
                            this.addCondition(RuleFilters[key], key);
                        }
                    }
                }
                break;
            case 'teams':
                if (data.TeamsRule != null && data.TeamsRule.RuleFilters != null && data.TeamsRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.TeamsRule.RuleFilters);
                    for (let key in RuleFilters) {
                        if (RuleFilters[key]) {
                            this.addCondition(RuleFilters[key], key);
                        }
                    }
                }
                break;
            case 'exo':
                if (data.EXORule != null && data.EXORule.RuleFilters != null && data.EXORule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.EXORule.RuleFilters);
                    for (let inKey in data.EXORule.RuleFilters) {
                        if (data.EXORule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
            case 'phy':
                if (data.PhysicalRule != null && data.PhysicalRule.RuleFilters != null && data.PhysicalRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.PhysicalRule.RuleFilters);
                    for (let inKey in data.PhysicalRule.RuleFilters) {
                        if (data.PhysicalRule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
            case 'fs':
                if (data.FSRule != null && data.FSRule.RuleFilters != null && data.FSRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.FSRule.RuleFilters);
                    for (let inKey in data.FSRule.RuleFilters) {
                        if (data.FSRule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
            case 'azureFile':
                if (data.AzureFileRule != null && data.AzureFileRule.RuleFilters != null && data.AzureFileRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.AzureFileRule.RuleFilters);
                    for (let inKey in data.AzureFileRule.RuleFilters) {
                        if (data.AzureFileRule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
            case 'box':
                if (data.BoxRule != null && data.BoxRule.RuleFilters != null && data.BoxRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.BoxRule.RuleFilters);
                    for (let inKey in data.BoxRule.RuleFilters) {
                        if (data.BoxRule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
            case 'google':
                if (data.GoogleDriveRule != null && data.GoogleDriveRule.RuleFilters != null && data.GoogleDriveRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.GoogleDriveRule.RuleFilters);
                    for (let inKey in data.GoogleDriveRule.RuleFilters) {
                        if (data.GoogleDriveRule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
            case 'connector':
                if (data.ConnectorRule != null && data.ConnectorRule.RuleFilters != null && data.ConnectorRule.RuleFilters.length > 0) {
                    RuleFilters = this.deepCopy(data.ConnectorRule.RuleFilters);
                    for (let inKey in data.ConnectorRule.RuleFilters) {
                        if (data.ConnectorRule.RuleFilters[inKey]) {
                            this.addCondition(RuleFilters[inKey], inKey);
                        }
                    }
                }
                break;
        }
    }

    // name选择
    conditionTypeClick(index, args) {
        let item = args.newValue;
        this.hasChanged = true;
        let criteria = this.criterias[index];
        criteria.currentType = item;
        criteria.ruleTypeId = item.id;
        criteria.isConditionTypesShow = false;
        criteria.Value1 = "";
        this.checkOption(item, index);
        this.clearMsg(index);
        this.conflictOwnCheck(index);
        this.conflictAllCheck();
        this.setState({
            criterias: this.deepCopy(this.criterias)
        }, () => {
            if (this.props.onChange) {
                this.props.onChange(this.state.criterias);
            }
        });
    }

    //点击currentTypeName  点击name
    //duplicate property name 'currentTypeNameClick'.
    //currentTypeNameClick(index, event) {
    //    this.stopPropagation(event);
    //    this.criterias[index].isConditionTypesShow = true;
    //    this.setState({
    //        criterias: this.deepCopy(this.criterias)
    //    });
    //}
    getRealPhysicalLevelId() {
        return this.levelId == Constants.RuleLevel.Folder ? Constants.phyLevelIds.PhysicalFile : Constants.phyLevelIds.PhysicalBox;
    }

    checkOption(data, index) {
        let itemId = this.props.itemId;
        let item = {};
        let type, hasData = false, dataRes = this.Regexs[0];
        if (data.RuleType) {
            type = data.RuleType;
            item = this.criterias[index];
            item.type = data.RuleType;
            item.CombineMode = data.CombineMode;
            item.ruleTypeId = type;
            this.setState({
                CombineMode: data.CombineMode
            });
            switch (itemId) {
                case 'sp':
                    item.currentType = this.findDataById(this.rulTypes[this.levelId], type);
                    item.conditionTypes = this.rulTypes[this.levelId];
                    break;
                case 'spLocal':
                    item.currentType = this.findDataById(this.spLocalRulTypes[this.levelId], type);
                    item.conditionTypes = this.spLocalRulTypes[this.levelId];
                    break;
                case 'oneDrive':
                    item.currentType = this.findDataById(this.oneDriveRuleTypes[this.levelId], type);
                    item.conditionTypes = this.oneDriveRuleTypes[this.levelId];
                    break;
                case 'teams':
                    item.currentType = this.findDataById(this.teamsRuleTypes[33554432], type);
                    item.conditionTypes = this.teamsRuleTypes[33554432];
                    break;
                case 'phy':
                    item.currentType = this.findDataById(this.phyRulTypes[this.getRealPhysicalLevelId()], type);
                    //console.log("real:"+this.getRealPhysicalLevelId());
                    item.conditionTypes = this.phyRulTypes[this.getRealPhysicalLevelId()];
                    break;
                case 'exo':
                    item.currentType = this.findDataById(this.exoRulTypes[6553601], type);
                    item.conditionTypes = this.exoRulTypes[6553601];
                    break;
                case 'fs':
                    item.currentType = this.findDataById(this.fsRulTypes, type);
                    item.conditionTypes = this.fsRulTypes;
                    break;
                case 'azureFile':
                    item.currentType = this.findDataById(this.azureFileRulTypes, type);
                    item.conditionTypes = this.azureFileRulTypes;
                    break;
                case 'box':
                    item.currentType = this.findDataById(this.boxRulTypes, type);
                    item.conditionTypes = this.boxRulTypes;
                    break;
                case 'google':
                    item.currentType = this.findDataById(this.googleDriveRuleTypes, type);
                    item.conditionTypes = this.googleDriveRuleTypes;
                    break;
            }

            if (this.props.allowedRuleTypes && Array.isArray(this.props.allowedRuleTypes)) {
                item.conditionTypes = item.conditionTypes.filter(t => this.props.allowedRuleTypes.includes(t.id));
            }
            
            hasData = true;
        } else {
            type = data.id;
        }

        // RECO-24897
        // if (type == this.RuleType.ColumnText || type == this.RuleType.ColumnNumber || type == this.RuleType.ColumnBoolean || type == this.RuleType.ColumnDateTime
        //     || type == this.RuleType.MetadataNumberColumn || type == this.RuleType.MetadataTextColumn) {
        //     item.columnNamePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnName;
        //     item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_ColumnValue;
        // } else if (type == this.RuleType.TextCustomProperty || type == this.RuleType.NumberCustomProperty || type == this.RuleType.BooleanCustomProperty || type == this.RuleType.DateTimeCustomProperty) {
        //     item.columnNamePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_PropertyName;
        //     item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_PropertyValue;
        // } else {
        //     item.columnNamePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue;
        //     item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue;
        // }

        let Matchs1 = [];
        let Matchs2 = [];
        if (this.props.itemId == "sp" || this.props.itemId == "fs" || this.props.itemId == "spLocal" || this.props.itemId == "oneDrive" || this.props.itemId == "teams" || this.props.itemId == "azureFile" || this.props.itemId == "box" || this.props.itemId == "google" || this.props.itemId == "connector") {
            item = this.criterias[index];
            item.valueUnit= 0;
            switch (type) {
                case this.RuleType.Name:
                case this.RuleType.CreateBy:
                case this.RuleType.ModifiedBy:
                case this.RuleType.RetentionLabelRule:
                case this.RuleType.SensitiveLabel:
                case this.RuleType.SensitiveLabelFullName:
                case this.RuleType.ContentType:
                case this.RuleType.ParentListId:
                case this.RuleType.Title:
                case this.RuleType.KeepTheLatestVersion:
                case this.RuleType.URL:
                case this.RuleType.Type:
                case this.RuleType.Owner:
                case this.RuleType.Path:
                case this.RuleType.PrimaryAdministrator:
                case this.RuleType.ParentFolderName:
                case this.RuleType.ParentFolderNameHeirarchically:
                case this.RuleType.ParentLibraryName:
                case this.RuleType.Classification:
                case this.RuleType.DisplayName:
                case this.RuleType.Member:
                case this.RuleType.LabelName:
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    item.isArray = false;
                    if (type == this.RuleType.Name || type == 7 || type == this.RuleType.Title ||
                        type == this.RuleType.URL || type == this.RuleType.ParentFolderName || type == this.RuleType.ParentFolderNameHeirarchically ||
                        type == this.RuleType.Type || type == this.RuleType.Owner || type == this.RuleType.Path || type == this.RuleType.ParentLibraryName ||
                        type == this.RuleType.Classification || type == this.RuleType.DisplayName || type == this.RuleType.LabelName) {
                        for (let key of this.Regexs) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    } else if (type == 5 || type == 6 || type == this.RuleType.PrimaryAdministrator) {//modified by & created by
                        Matchs1.push(this.Regexs[0], this.Regexs[4]);
                        if (hasData && data.Condition == this.Regexs[4].id) {
                            dataRes = this.Regexs[4];
                        }
                    } else if (type == this.RuleType.Member) {
                        const newRegexs = [...this.Regexs].slice(0, 4);
                        dataRes = newRegexs[0];
                        for (let key of newRegexs) {
                            Matchs1.push(key);
                        }
                        Matchs1.push(Constants.SpecialRegexs[0]);
                        if (hasData && data.Condition) {
                            dataRes = Matchs1.find((item) => item.id == data.Condition);
                        }
                        if (dataRes.id == this.ConditionType.IsEmpty) {
                            hasData = false;
                        }
                        this.checkConditionId(dataRes.id, data, hasData, index);
                    } else if (type == this.RuleType.RetentionLabelRule || type == this.RuleType.SensitiveLabel || type == this.RuleType.SensitiveLabelFullName) {
                        const newRegexs = [...this.Regexs].slice(2, 6);
                        dataRes = newRegexs[0];
                        for (let key of newRegexs) {
                            Matchs1.push(key);
                        }
                        Matchs1.push(Constants.SpecialRegexs[0]);
                        if(hasData && data.Condition){
                            dataRes = Matchs1.find((item) => item.id == data.Condition);
                        }
                        if (dataRes.id == this.ConditionType.IsEmpty) {
                            hasData = false;
                        }
                        this.checkConditionId(dataRes.id, data, hasData, index);
                    } else if(type == this.RuleType.KeepTheLatestVersion){
                        if (this.levelId == 512) {
                            Matchs1 = [Constants.KeepVersionConditions[0]];
                        } else {
                            Matchs1 = Constants.KeepVersionConditions;
                        }
                        if(hasData){
                            dataRes = Matchs1.filter(option => option.id === data.Condition)[0];
                        }else{
                            dataRes = Matchs1[0];
                        }
                    } else {//parent list
                        dataRes = this.Regexs[4];
                        Matchs1.push(this.Regexs[4], this.Regexs[5]);
                        if (hasData && data.Condition == this.Regexs[5].id) {
                            dataRes = this.Regexs[5];
                        }
                    }
                    item.Value1 = hasData ? data.Value1 : "";
                    item.Value2 = hasData ? data.Value2 : "";
                    item.filterName = hasData ? data.filterName : "";
                    break;
                case this.RuleType.Privacy:
                case this.RuleType.TeamsStatus:
                case this.RuleType.TeamsType: {
                    item.isColumn = false;
                    item.isText = false;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = true;
                    item.isArray = false;
                    let newRegexs = [];
                    let unit = {};
                    let teamsMatchs2 = [];

                    if (type == this.RuleType.Privacy) {
                        newRegexs = [...this.Regexs].slice(-2);
                        unit = Constants.TeamsPrivacy[0];
                        teamsMatchs2 = Constants.TeamsPrivacy;
                    } else if (type == this.RuleType.TeamsStatus) {
                        newRegexs = [this.Regexs[4]];
                        unit = Constants.TeamsStatus[0];
                        teamsMatchs2 = Constants.TeamsStatus;
                    } else {
                        newRegexs = [...this.Regexs].slice(-2);
                        unit = Constants.TeamsType[0];
                        teamsMatchs2 = Constants.TeamsType;
                    }

                    dataRes = newRegexs[0];
                    for (let key of newRegexs) {
                        Matchs1.push(key);
                    }
                    if (hasData && data.Condition) {
                        dataRes = Matchs1.find((item) => item.id == data.Condition);
                    }
                    for (let key of teamsMatchs2) {
                        Matchs2.push(key);
                        if (hasData && key.id == data.Value1Unit) {
                            unit = key;
                        }
                    }
                    item.Matchs2 = Matchs2;
                    item.currentMatch2 = unit;
                    item.valueUnit = unit.id;
                    break;
                } 
                case this.RuleType.DocumentSize:// size
                case this.RuleType.SiteCollectionSizeTrigger: {
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = true;
                    item.isArray = false;
                    let unit = this.unitSize[0];
                    dataRes = this.compare[0];
                    item.filterName = "";
                    if (type == this.RuleType.DocumentSize) {
                        Matchs1.push(this.compare[0], this.compare[1]);
                    } else {
                        Matchs1.push(this.compare[0]);
                    }

                    if (hasData) {
                        if (data.Condition == this.compare[1].id) {
                            dataRes = this.compare[1];
                        }
                        item.Value1 = data.Value1;
                    } else {
                        item.Value1 = "";
                    }
                    for (let key of this.unitSize) {
                        Matchs2.push(key);
                        Matchs2 = Matchs2.slice(0, 3);
                        if (hasData && key.id == data.Value1Unit) {
                            unit = key;
                        }
                    }
                    item.Matchs2 = Matchs2;
                    item.currentMatch2 = unit;
                    item.valueUnit = unit.id;
                    break;
                }
                case this.RuleType.Modified://modified time
                case this.RuleType.DocumentModifiedTime://modified time
                case this.RuleType.CreateTime://create time
                case this.RuleType.LastAccessTime:
                case this.RuleType.LastActiveTime:
                    item.isColumn = false;
                    item.isArray = false;
                    dataRes = this.dateOption[0];
                    item.filterName = "";
                    for (let key of this.dateOption) {
                        Matchs1.push(key);
                        if (hasData && key.id == data.Condition) {
                            dataRes = key;
                        }
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index, 0);
                    break;
                case this.RuleType.LatestSubfolderDisposalDate:
                    item.isColumn = false;
                    item.isArray = false;
                    dataRes = this.dateOption[2];
                    item.filterName = "";
                    Matchs1.push(dataRes);
                    this.checkConditionId(dataRes.id, data, hasData, index, 0);
                    break;
                case this.RuleType.ColumnText:
                case this.RuleType.LabelPropertyText:
                case this.RuleType.ColumnNumber:
                case this.RuleType.LabelPropertyNumber:
                case this.RuleType.TextCustomProperty:
                case this.RuleType.NumberCustomProperty:
                case this.RuleType.MetadataTextColumn:
                case this.RuleType.MetadataNumberColumn:
                case this.RuleType.ParentLibraryText:
                case this.RuleType.ParentSiteCollectionText:
                case this.RuleType.ParentLibraryNumber:
                case this.RuleType.ParentSiteCollectionNumber:
                case this.RuleType.PropertyBagText:
                case this.RuleType.PropertyBagNumber:
                    item.isColumn = true;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    item.isArray = false;
                    if (type == this.RuleType.ColumnText || type == this.RuleType.LabelPropertyText || type == this.RuleType.TextCustomProperty || type == this.RuleType.MetadataTextColumn
                        || type == this.RuleType.ParentLibraryText || type == this.RuleType.ParentSiteCollectionText || type == this.RuleType.PropertyBagText
                    ) {//cloumn text
                        let tempRegexs = [...this.Regexs];

                        if (LicenseHelper.EnableRecordsArchiver() && type == this.RuleType.ColumnText || LicenseHelper.EnableRecordsArchiver() && type == this.RuleType.LabelPropertyText) {
                            tempRegexs.push(Constants.SpecialRegexs[0]);
                        }
                        if (
                            RM.gData.enableCustomizationApp
                            && LicenseHelper.EnableRecordsArchiver()
                            && this.moduleType == RuleModuleTypes.Records
                            && type == this.RuleType.ColumnText
                            && ["sp", "oneDrive", "teams", "fs"].includes(this.props.itemId)
                        ) {
                            if (this.props.itemId === "fs") {
                                //only support equals and in conditions for file system column text
                                tempRegexs = tempRegexs.filter(regex => regex.id === Constants.ConditionType.Equals);
                                dataRes = tempRegexs[0];
                            }
                            tempRegexs.push(Constants.SpecialRegexs[1]);
                        }

                        for (let key of tempRegexs) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    }
                    //else if (type == this.Ruletype.MetadataNumberColumn){
                    //    Matchs1.push(this.compare[0], this.compare[1]);
                    //}
                    else {//column number
                        dataRes = this.compare[0];
                        for (let key of this.compare) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    }
                    if (hasData) {
                        item.Value1 = data.Value1;
                        item.Value2 = data.Value2;
                        item.filterName = data.filterName;
                    } else {
                        item.Value1 = "";
                        item.Value2 = "";
                        item.filterName = "";
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index);
                    break;
                case this.RuleType.ColumnBoolean://column yes
                case this.RuleType.BooleanCustomProperty:
                case this.RuleType.ParentLibraryYestNo:
                case this.RuleType.ParentSiteCollectionYestNo:
                case this.RuleType.PropertyBagBoolean: {
                    let match2 = this.state.TrueOrFaseOptions[0];
                    item.isColumn = true;
                    item.isText = false;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = true;
                    item.isArray = false;
                    dataRes = this.Regexs[4];
                    Matchs1.push(this.Regexs[4]);

                    let tempTrueOrFaseOptions = [...this.state.TrueOrFaseOptions];
                    if (
                        LicenseHelper.EnableRecordsArchiver() && this.moduleType == RuleModuleTypes.SOArchiver &&
                        this.props.itemId == "sp" && this.levelId == Constants.RuleLevel.Document &&
                        (type == this.RuleType.ColumnBoolean) // || type == this.RuleType.ParentLibraryYestNo || type == this.RuleType.ParentSiteCollectionYestNo
                    ) {
                        tempTrueOrFaseOptions.push({ id: 2, Name: RMResx.RM_FA_Discovery_RuleCondition_IsEmpty, value: "empty" });
                    }
                    for (let key of tempTrueOrFaseOptions) {
                        Matchs2.push(key);
                    }
                    if (hasData) {
                        item.filterName = data.filterName;
                        for (let key of tempTrueOrFaseOptions) {
                            if (key.Name == data.Value1 || key.value == data.Value1) {
                                match2 = key;
                            }
                        }
                    } else {
                        item.filterName = "";
                    }
                    item.Matchs2 = Matchs2;
                    item.currentMatch2 = match2;
                    item.Value1 = match2.id;
                    break;
                }
                case this.RuleType.OrphanedFolder: {
                    let match2 = this.state.TrueOrFaseOptions[0];
                    item.isColumn = false;
                    item.isText = false;
                    item.isMath1 = true;
                    item.isMath2 = true;
                    item.isArray = false;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    dataRes = this.Regexs[4];
                    Matchs1.push(this.Regexs[4]);

                    let tempTrueOrFaseOptions = [...this.state.TrueOrFaseOptions];
                    for (let key of tempTrueOrFaseOptions) {
                        Matchs2.push(key);
                    }
                    if (hasData) {
                        item.filterName = data.filterName;
                        for (let key of tempTrueOrFaseOptions) {
                            if (key.Name == data.Value1 || key.value == data.Value1) {
                                match2 = key;
                            }
                        }
                    }
                    item.Matchs2 = Matchs2;
                    item.currentMatch2 = match2;
                    item.Value1 = match2.id;
                    break;
                }
                case this.RuleType.ColumnDateTime: //column date
                case this.RuleType.LabelPropertyDate:
                case this.RuleType.DateTimeCustomProperty:
                case this.RuleType.ParentLibraryDateTime:
                case this.RuleType.ParentSiteCollectionDateTime:
                case this.RuleType.PropertyBagDateTime:
                    item.isColumn = true;
                    item.isArray = false;

                    dataRes = this.dateOption[0];
                    for (let key of this.dateOption) {
                        Matchs1.push(key);
                        if (hasData && key.id == data.Condition) {
                            dataRes = key;
                        }
                    }

                    if (hasData) {
                        item.filterName = data.filterName;
                        item.Value1 = data.Value1;
                        item.Value2 = data.Value2;
                        item.Value3 = data.Value3;
                    } else {
                        item.filterName = "";
                        item.Value1 = "";
                        item.Value2 = "";
                        item.Value3 = "";
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index, 0);
                    break;

                default:

            }
        } else if (this.props.itemId == 'exo') {
            item = this.criterias[index];
            item.valueUnit= 0;
            switch (type) {
                case this.RuleType.Subject:
                case this.RuleType.SendFrom:
                case this.RuleType.SendTo:
                case this.RuleType.RetentionLabelRule:
                case this.RuleType.SensitiveLabel:
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    if (type == this.RuleType.SendFrom || type == this.RuleType.SendTo) {
                        item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue_SendFromOrTo;
                    }
                    if (type == this.RuleType.Name || type == 7 || type == this.RuleType.Title || type == this.RuleType.URL 
                        || type == this.RuleType.Type || type == this.RuleType.Owner || type == this.RuleType.Path 
                        || type == this.RuleType.Subject) {
                        for (let key of this.Regexs) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    } else if (type == 5 || type == 6 || type == this.RuleType.PrimaryAdministrator || type == this.RuleType.SendFrom) {//modified by & created by
                        Matchs1.push(this.Regexs[0], this.Regexs[4]);
                        if (hasData && data.Condition == this.Regexs[4].id) {
                            dataRes = this.Regexs[4];
                        }
                    } else if (type == this.RuleType.SendTo) {
                        Matchs1.push(this.Regexs[0]);
                    } else if (type == this.RuleType.RetentionLabelRule || type == this.RuleType.SensitiveLabel) {
                        dataRes = this.Regexs[4];
                        let retentionLabelRegexTypes = [this.ConditionType.Equals];
                        if(this.props.isNestleCustomize){
                            retentionLabelRegexTypes.push(this.ConditionType.IsExactlyNot);
                        }
                        Matchs1 = this.Regexs.filter( regexItem => retentionLabelRegexTypes.includes(regexItem.id));
                        Matchs1.push(Constants.SpecialRegexs[0]);
                        if(hasData && data.Condition){
                            dataRes = Matchs1.find((item)=>{return item.id == data.Condition;});
                        }
                        if (dataRes.id == this.ConditionType.IsEmpty) {
                            hasData = false;
                        }
                        this.checkConditionId(dataRes.id, data, hasData, index);
                    }else {//parent list
                        dataRes = this.Regexs[4];
                        Matchs1.push(this.Regexs[4], this.Regexs[5]);
                        if (hasData && data.Condition == this.Regexs[5].id) {
                            dataRes = this.Regexs[5];
                        }
                    }
                    item.Value1 = hasData ? data.Value1 : item.Value1;
                    break;
                case this.RuleType.AttachmentCount:
                case this.RuleType.Size: {
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    if (type != this.RuleType.AttachmentCount) {
                        item.isMath2 = true;
                    }else{
                        item.isMath2 = false; 
                    }
                    let unit = this.unitSize[0];
                    dataRes = this.compare[0];
                    item.filterName = "";
                    Matchs1.push(this.compare[0], this.compare[1]);
                    if (hasData) {
                        if (data.Condition == this.compare[1].id) {
                            dataRes = this.compare[1];
                        }
                        item.Value1 = data.Value1;
                    } else {
                        item.Value1 = "";
                    }
                    for (let key of this.unitSize) {
                        Matchs2.push(key);
                        Matchs2 = Matchs2.slice(0, 2);
                        if (hasData && key.id == data.Value1Unit) {
                            unit = key;
                        }
                    }
                    item.Matchs2 = Matchs2;
                    item.currentMatch2 = unit;
                    item.valueUnit = unit.id;
                    break;
                }
                case this.RuleType.SendDateUTC:
                    item.isColumn = false;
                    dataRes = this.dateOption[0];
                    item.filterName = "";
                    for (let key of this.dateOption) {
                        Matchs1.push(key);
                        if (hasData && key.id == data.Condition) {
                            dataRes = key;
                        }
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index, 1);
                    break;
                default:
                    item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue;
            }
            if(type == this.RuleType.SendFrom || type == this.RuleType.SendTo){
                item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue_SendFromOrTo;
            }else{
                item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue; 
            }
        } else {
            item = this.criterias[index];
            item.valueUnit= 0;
            switch (type) {
                case this.RuleType.Name:
                case this.RuleType.CreateBy:
                case this.RuleType.ModifiedBy:
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    if (type == this.RuleType.Name) {
                        for (let key of this.Regexs) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    } else if (type == 5 || type == 6) {//modified by & created by
                        Matchs1.push(this.Regexs[0], this.Regexs[4]);
                        if (hasData && data.Condition == this.Regexs[4].id) {
                            dataRes = this.Regexs[4];
                        }
                    } else {
                        dataRes = this.Regexs[4];
                        Matchs1.push(this.Regexs[4], this.Regexs[5]);
                        if (hasData && data.Condition == this.Regexs[5].id) {
                            dataRes = this.Regexs[5];
                        }
                    }
                    item.Value1 = hasData ? data.Value1 : item.Value1;
                    break;
                case this.RuleType.Modified://modified time
                case this.RuleType.DocumentModifiedTime:
                case this.RuleType.CreateTime://create time
                    item.isColumn = false;
                    dataRes = this.dateOption[0];
                    item.filterName = "";
                    for (let key of this.dateOption) {
                        Matchs1.push(key);
                        if (hasData && key.id == data.Condition) {
                            dataRes = key;
                        }
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index, 0);
                    break;
                case this.RuleType.LatestSubfolderDisposalDate:
                    item.isColumn = false;
                    dataRes = this.dateOption[2];
                    item.filterName = "";
                    Matchs1.push(dataRes);
                    this.checkConditionId(dataRes.id, data, hasData, index, 0);
                    break;
                case this.RuleType.ColumnText:
                case this.RuleType.LabelPropertyText:
                case this.RuleType.TextCustomProperty:
                case this.RuleType.NumberCustomProperty:
                    item.isColumn = true;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    if (type == this.RuleType.ColumnText || type == this.RuleType.LabelPropertyText || type == this.RuleType.TextCustomProperty) {//cloumn text
                        for (let key of this.Regexs) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    } else {//column number
                        dataRes = this.compare[0];
                        for (let key of this.compare) {
                            Matchs1.push(key);
                            if (hasData && key.id == data.Condition) {
                                dataRes = key;
                            }
                        }
                    }
                    if (hasData) {
                        item.Value1 = data.Value1;
                        item.filterName = data.filterName;
                    } else {
                        item.Value1 = "";
                        item.filterName = "";
                    }
                    break;
                case this.RuleType.ColumnDateTime: //column date
                    item.isColumn = true;
                    dataRes = this.dateOption[0];
                    for (let key of this.dateOption) {
                        Matchs1.push(key);
                        if (hasData && key.id == data.Condition) {
                            dataRes = key;
                        }
                    }
                    if (hasData) {
                        item.filterName = data.filterName;
                    } else {
                        item.filterName = "";
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index, 0);
                    break;
                default:

            }
        }
        item.currentMatch1 = dataRes;
        item.conditionId = dataRes.id;
        item.Matchs1 = Matchs1;
        item.conditionTypes = RM.deepcopy(item.conditionTypes);
        item.conditionTypes.forEach(conditionType => {
            if (conditionType.id == type) {
                conditionType.checked = true;
            } else {
                conditionType.checked = false;
            }
        });

        if (item.isMath1) {
            item.Matchs1 = RM.deepcopy(item.Matchs1);
            item.Matchs1.forEach(match => {
                if (match.id == item.currentMatch1.id) {
                    match.checked = true;
                } else {
                    match.checked = false;
                }
            });
        }
        if (item.isMath2) {
            item.Matchs2 = RM.deepcopy(item.Matchs2);
            item.Matchs2.forEach(match => {
                if (match.id == item.currentMatch2?.id) {
                    match.checked = true;
                } else {
                    match.checked = false;
                }
            });
        }
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    match1Click(index, args) {
        let item = args.newValue;
        let criteria = this.criterias[index];
        criteria.Matchs1.forEach(match1 => {
            if (match1.id == item.id) {
                match1.checked = true;
            } else {
                match1.checked = false;
            }
        });
        this.hasChanged = true;
        criteria.conditionId = item.id;  //match1Id
        criteria.currentMatch1 = item;
        criteria.isMath1popupShow = false;
        this.checkConditionId(item.id, item, false, index);
        this.clearMsg(index);
        this.conflictOwnCheck(index);
        this.setState({
            criterias: this.deepCopy(this.criterias)
        }, () => {
            if (this.props.onChange) {
                this.props.onChange(this.state.criterias);
            }
        });
    }

    //验证重复规则
    conflictOwnCheck(index) {
        let isValid = true;
        let item = this.criterias;
        let item1 = item[index];
        for (let key in item) {
            if (item.hasOwnProperty(key)) {
                let item2 = item[key];
                if (item2) {
                    if (index != key) {
                        if (this.checkSameCondition(item1, item2)) {
                            item1.isConflict = true;
                            isValid = false;
                            break;
                        } else {
                            item1.isConflict = false;
                        }
                    }
                }
            }
        }
        return isValid;
    }

    checkSameCondition(item1, item2) {
        let result = false;
        if (this.props.itemId == "sp" || this.props.itemId == 'fs' || this.props.itemId == "spLocal" || this.props.itemId == "oneDrive" || this.props.itemId == "teams" || this.props.itemId == 'google' || this.props.itemId == 'connector') {
            if (item1.ruleTypeId == item2.ruleTypeId && item1.conditionId == item2.conditionId) {
                switch (item1.ruleTypeId) {
                    case this.RuleType.Name:
                    case this.RuleType.CreateBy:
                    case this.RuleType.ModifiedBy:
                    case this.RuleType.RetentionLabelRule:
                    case this.RuleType.SensitiveLabel:
                    case this.RuleType.SensitiveLabelFullName:
                    case this.RuleType.ContentType:
                    case this.RuleType.ParentListId:
                    case this.RuleType.Title:
                    case this.RuleType.URL:
                    case this.RuleType.PrimaryAdministrator:
                    case this.RuleType.ParentFolderName:
                    case this.RuleType.ParentFolderNameHeirarchically:
                    case this.RuleType.Type:
                    case this.RuleType.Owner:
                    case this.RuleType.Path:
                    case this.RuleType.KeepTheLatestVersion:
                    case this.RuleType.Classification:
                    case this.RuleType.DisplayName:
                    case this.RuleType.Member:
                    case this.RuleType.LabelName:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1);
                        break;
                    case this.RuleType.DocumentSize:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1) && item1.currentMatch1.id == item2.currentMatch1.id && item1.currentMatch2.id == item2.currentMatch2.id;
                        break;
                    case this.RuleType.Privacy:
                    case this.RuleType.TeamsStatus:
                    case this.RuleType.TeamsType:
                        result = item1.currentMatch1.id == item2.currentMatch1.id && item1.currentMatch2.id == item2.currentMatch2.id
                        break;
                    case this.RuleType.Modified:
                    case this.RuleType.DocumentModifiedTime:
                    case this.RuleType.CreateTime:
                    case this.RuleType.ColumnDateTime:
                    case this.RuleType.LastAccessTime:
                    case this.RuleType.LastActiveTime:
                    case this.RuleType.ParentLibraryDateTime:
                    case this.RuleType.ParentSiteCollectionDateTime:
                    case this.RuleType.PropertyBagDateTime:
                        switch (item1.conditionId) {
                            case this.ConditionType.FromTo://from to
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime() && new Date(item1.currentDate2).getTime() == new Date(item2.currentDate2).getTime();
                                //result = result && item1.currentTimeZone.id == item2.currentTimeZone.id && item1.currentTimeZone.autoAdjustClock == item2.currentTimeZone.autoAdjustClock;
                                break;
                            case this.ConditionType.OlderThan://older than
                                result = item1.Value1 == item2.Value1 && item1.currentMatch2.id == item2.currentMatch2.id;
                                break;
                            case this.ConditionType.Before://before
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime();
                                //result = result && item1.currentTimeZone.id == item2.currentTimeZone.id && item1.currentTimeZone.autoAdjustClock == item2.currentTimeZone.autoAdjustClock;
                                break;
                            default:
                        }
                        if (item1.ruleTypeId == this.RuleType.ColumnDateTime || item1.ruleTypeId == this.RuleType.ParentLibraryDateTime || item1.ruleTypeId == this.RuleType.ParentSiteCollectionDateTime ||
                            item1.ruleTypeId == this.RuleType.PropertyBagDateTime
                        ) {
                            result = result && item1.filterName == item2.filterName;
                        }
                        break;
                    case this.RuleType.ColumnText:
                    case this.RuleType.ColumnNumber:
                    case this.RuleType.TextCustomProperty:
                    case this.RuleType.NumberCustomProperty:
                    case this.RuleType.MetadataTextColumn:
                    case this.RuleType.MetadataNumberColumn:
                    case this.RuleType.ParentLibraryText:
                    case this.RuleType.ParentSiteCollectionText:
                    case this.RuleType.ParentLibraryNumber:
                    case this.RuleType.ParentSiteCollectionNumber:
                    case this.RuleType.PropertyBagText:
                    case this.RuleType.PropertyBagNumber:
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1;
                        break;
                    case this.RuleType.ColumnBoolean:
                    case this.RuleType.BooleanCustomProperty:
                    case this.RuleType.ParentLibraryYestNo:
                    case this.RuleType.ParentSiteCollectionYestNo:
                    case this.RuleType.PropertyBagBoolean:
                        result = item1.filterName == item2.filterName && item1.currentMatch2.id == item2.currentMatch2.id;
                        break;
                    case this.RuleType.DateTimeCustomProperty:
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1;
                        break;
                    default:
                    //return false;
                }
            }
        } else if (this.props.itemId == 'exo') {
            if (item1.ruleTypeId == item2.ruleTypeId && item1.conditionId == item2.conditionId) {
                switch (item1.ruleTypeId) {
                    case this.RuleType.Subject:
                    case this.RuleType.SendFrom:
                    case this.RuleType.SendTo:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1);
                        break;
                    case this.RuleType.Size:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1) && item1.currentMatch2.id == item2.currentMatch2.id;
                        break;
                    case this.RuleType.SendDateUTC:
                        switch (item1.conditionId) {
                            case this.ConditionType.FromTo://from to
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime() && new Date(item1.currentDate2).getTime() == new Date(item2.currentDate2).getTime();
                                //result = result && item1.currentTimeZone.id == item2.currentTimeZone.id && item1.currentTimeZone.autoAdjustClock == item2.currentTimeZone.autoAdjustClock;
                                break;
                            case this.ConditionType.OlderThan://older than
                                result = item1.Value1 == item2.Value1 && item1.currentMatch2.id == item2.currentMatch2.id;
                                break;
                            case this.ConditionType.Before://before
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime();
                                //result = result && item1.currentTimeZone.id == item2.currentTimeZone.id && item1.currentTimeZone.autoAdjustClock == item2.currentTimeZone.autoAdjustClock;
                                break;
                            default:
                        }
                        if (item1.ruleTypeId == this.RuleType.ColumnDateTime) {
                            result = result && item1.filterName == item2.filterName;
                        }
                        break;
                    default:
                }
            }
        }  else {
            if (item1.ruleTypeId == item2.ruleTypeId && item1.conditionId == item2.conditionId) {
                switch (item1.ruleTypeId) {
                    case this.RuleType.Title:
                    case this.RuleType.CreateBy:
                    case this.RuleType.ModifiedBy:
                    case this.RuleType.Name: // update case for Google, azureFile, box
                    case this.RuleType.Type: // update case for azureFile, box
                    case this.RuleType.Path: // update case for azureFile, box
                        result = $.trim(item1.Value1) == $.trim(item2.Value1);
                        break;
                    case this.RuleType.DocumentSize:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1) && item1.currentMatch2.id == item2.currentMatch2.id;
                        break;
                    case this.RuleType.Modified:
                    case this.RuleType.DocumentModifiedTime:
                    case this.RuleType.CreateTime:
                    case this.RuleType.ColumnDateTime:
                    case this.RuleType.LastAccessTime: // update case for azureFile
                    case this.RuleType.LatestSubfolderDisposalDate:
                        switch (item1.conditionId) {
                            case this.ConditionType.FromTo://from to
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime() && new Date(item1.currentDate2).getTime() == new Date(item2.currentDate2).getTime();
                                //result = result && item1.currentTimeZone.id == item2.currentTimeZone.id && item1.currentTimeZone.autoAdjustClock == item2.currentTimeZone.autoAdjustClock;
                                break;
                            case this.ConditionType.OlderThan://older than
                                result = item1.Value1 == item2.Value1 && item1.currentMatch2.id == item2.currentMatch2.id;
                                break;
                            case this.ConditionType.Before://before
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime();
                                //result = result && item1.currentTimeZone.id == item2.currentTimeZone.id && item1.currentTimeZone.autoAdjustClock == item2.currentTimeZone.autoAdjustClock;
                                break;
                            default:
                        }
                        if (item1.ruleTypeId == this.RuleType.ColumnDateTime || item1.ruleTypeId == this.RuleType.LabelPropertyDate || item1.ruleTypeId == this.RuleType.ParentLibraryDateTime || item1.ruleTypeId == this.RuleType.ParentSiteCollectionDateTime ||
                            item1.ruleTypeId == this.RuleType.PropertyBagDateTime
                        ) {
                            result = result && item1.filterName == item2.filterName;
                        }
                        break;
                    case this.RuleType.ColumnText:
                    case this.RuleType.TextCustomProperty:
                    case this.RuleType.NumberCustomProperty:
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1;
                        break;
                    case this.RuleType.LabelPropertyNumber:
                    case this.RuleType.LabelPropertyText:
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1 && item1.Value2 == item2.Value2;
                        break;
                    case this.RuleType.LabelPropertyDate:
                        switch (item1.conditionId) {
                            case this.ConditionType.FromTo://from to
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime() && new Date(item1.currentDate2).getTime() == new Date(item2.currentDate2).getTime();
                                break;
                            case this.ConditionType.OlderThan://older than
                                result = item1.currentMatch2.id == item2.currentMatch2.id;
                                break;
                            case this.ConditionType.Before://before
                                result = new Date(item1.currentDate1).getTime() == new Date(item2.currentDate1).getTime();
                                break;
                            default:
                        }
                        result = result && item1.filterName == item2.filterName && item1.Value1 == item2.Value1 ;
                        break;
                    case this.RuleType.ColumnBoolean:
                    case this.RuleType.BooleanCustomProperty:
                    case this.RuleType.ParentLibraryYestNo:
                    case this.RuleType.ParentSiteCollectionYestNo:
                    case this.RuleType.PropertyBagBoolean:
                        result = item1.filterName == item2.filterName && item1.currentMatch2.id == item2.currentMatch2.id;
                        break;
                    case this.RuleType.DateTimeCustomProperty:
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1;
                        break;
                    default:
                    //return false;
                }
            }
        }
        return result;
    }

    checkConditionId(id, data, hasData, index) {
        let item = this.criterias[index];
        switch (id) {
            case this.ConditionType.FromTo://from to
                item.isDate2 = true;
                item.isText = false;
                item.isMath2 = false;
                item.isDate1 = true;
                if (hasData) {
                    //item.currentTimeZone = RM.TimeUtil.getTimezoneInfo(data.StartTimeInfo.TimeZoneId, data.StartTimeInfo.IsDayLightSaving);
                    item.currentDate1 = new Date(data.StartTimeInfo.StartTime);
                    item.currentDate2 = new Date(data.EndTimeInfo.StartTime);
                }else{
                    item.currentDate1 = "";
                    item.currentDate2 = "";
                }
                item.valueUnit = 0;
                break;
            case this.ConditionType.OlderThan: {//older than
                item.isDate2 = false;
                item.isDate1 = false;
                item.isMath2 = true;
                item.isText = true;
                let dataSize = this.unitSize.slice(3);
                item.Matchs2 = dataSize;
                let curUnit = this.unitSize[3];
                if (hasData) {
                    const isLabelPropertyDateType = item.currentType.id == this.RuleType.LabelPropertyDate;
                    item.Value1 = data.Value1;
                    item.Value2 = data.Value2;
                    curUnit = this.findDataById(dataSize, isLabelPropertyDateType ? data.Value2Unit : data.Value1Unit);
                    item.currentMatch2 = curUnit;
                } else {
                    if (item.currentType.id != this.RuleType.LabelPropertyDate) {
                        item.Value1 = "";
                    }
                    item.Value2 = "";
                    item.currentMatch2 = curUnit;
                }
                item.valueUnit = curUnit?.id;
                if (item.isMath2) {
                    item.Matchs2 = RM.deepcopy(item.Matchs2);
                    item.Matchs2.forEach(match => {
                        if (match.id == item.currentMatch2?.id) {
                            match.checked = true;
                        } else {
                            match.checked = false;
                        }
                    });
                }
                break;
            }
            case this.ConditionType.Before://before
                item.isDate2 = false;
                item.isText = false;
                item.isMath2 = false;
                item.isDate1 = true;
                item.valueUnit = 0;
                if (hasData) {
                    //item.currentTimeZone = RM.TimeUtil.getTimezoneInfo(data.StartTimeInfo.TimeZoneId, data.StartTimeInfo.IsDayLightSaving);
                    item.currentDate1 = new Date(data.StartTimeInfo.StartTime);
                }else{
                    item.currentDate1 = "";
                }
                break;
            case this.ConditionType.IsEmpty: //Empty
                item.Value1 = hasData ? item.Value1 : "";
                item.isColumn = false;
                item.isDate2 = false;
                item.isText = false;
                item.isMath2 = false;
                item.isDate1 = false;
                item.currentDate1 = null;
                item.currentDate2 = null;
                if (item.ruleTypeId == this.RuleType.ColumnText || item.ruleTypeId == this.RuleType.LabelPropertyText) {
                    item.isColumn = true;
                    item.isArray = false;
                }
                break;
            case this.ConditionType.Equals: 
            case this.ConditionType.IsExactlyNot:
                if (item.ruleTypeId == this.RuleType.RetentionLabelRule || item.ruleTypeId == this.RuleType.SensitiveLabel || item.ruleTypeId == this.RuleType.Member || item.ruleTypeId == this.RuleType.SensitiveLabelFullName) {
                    item.isText = true;
                }
                if (item.ruleTypeId == this.RuleType.ColumnText || item.ruleTypeId == this.RuleType.LabelPropertyText || item.ruleTypeId == this.RuleType.ParentLibraryText || item.ruleTypeId == this.RuleType.ParentSiteCollectionText ||
                    item.ruleTypeId == this.RuleType.PropertyBagText
                ) {
                    item.isText = true;
                    item.isArray = false;
                }
                break;
            case this.ConditionType.Contains:
            case this.ConditionType.DoesNotContains:
            case this.ConditionType.Maths:
            case this.ConditionType.DoesNtoMath:
                if (item.ruleTypeId == this.RuleType.RetentionLabelRule || item.ruleTypeId == this.RuleType.SensitiveLabel || item.ruleTypeId == this.RuleType.Member || item.ruleTypeId == this.RuleType.SensitiveLabelFullName) {
                    item.isText = true;
                }
                if (item.ruleTypeId == this.RuleType.ColumnText || item.ruleTypeId == this.RuleType.LabelPropertyText || item.ruleTypeId == this.RuleType.ParentLibraryText || item.ruleTypeId == this.RuleType.ParentSiteCollectionText ||
                    item.ruleTypeId == this.RuleType.PropertyBagText
                ) {
                    item.isText = true;
                    item.isArray = false;
                }
                if (hasData) {
                    item.Value2 = data.Value2;
                } else {
                    item.Value2 = null;
                }
                break;
            case this.ConditionType.ListIn:
                item.isText = false;
                item.isArray = true;
                break;
            default:
        }
    }

    getListInItems(values) {
        let items = [];
        for (let value of values) {
            if(value){
                let item = {};
                item.name = value;
                item.checked = true;
                item.invalid = false;
                item.tooltip = value;
                items.push(item);
            }
        }
        return items;
    }

    doMatchArray = (args) => {
        return this.getListInItems(args.list);
    }

    //全部验证
    conflictAllCheck() {
        let isValid = true;
        let ruleCriterias = [];
        ruleCriterias = this.criterias;
        ruleCriterias.forEach((item, index) => {
            if (item.isConflict) {
                if (!this.conflictOwnCheck(index)) {
                    isValid = false;
                } else {
                    item.isConflict = false;
                }
            }
        });
        return isValid;
    }

    dateTimeRangeSelectChange(index,args){
        let item = {};
        this.criteriaIndex = index;
        item = this.criterias[this.criteriaIndex];
        if (args.newValue.start) {
            item.currentDate1 = args.newValue.start;
        }
        if (args.newValue.end) {
            item.currentDate2 = args.newValue.end;
        }
        this.dateTimeSelectChange(this.criteriaIndex);
    }

    dateTimeBeforeSelectChange(index,args) {
        let item = "";
        this.criteriaIndex = index;
        item = this.criterias[this.criteriaIndex];
        item.currentDate1 = args.newValue;
        //item.currentTimeZone = args.newValue.zone;
        this.dateTimeSelectChange(this.criteriaIndex);

    }

    dateTimeAfterSelectChange(index,args) {
        let item = "";
        this.criteriaIndex = index;
        item = this.criterias[this.criteriaIndex];
        item.currentDate2 = args.newValue;
        //item.currentTimeZone = args.newValue.zone;
        this.dateTimeSelectChange(this.criteriaIndex);
    }

    //验证时间前后时间比对
    dateTimeSelectChange(index) {
        this.hasChanged = true;
        let item = "";
        item = this.criterias[index];
        if (item.conditionId == this.ConditionType.FromTo) {
            if (item.currentDate1 && item.currentDate2) {
                let dt1 = new Date(item.currentDate1).getTime();
                let dt2 = new Date(item.currentDate2).getTime();
                if (dt1 >= dt2) {
                    item.isDateValid = true;
                } else {
                    item.isDateValid = false;
                }
                item.noDateValue = false;
            }

        }
        this.conflictOwnCheck(index);
        this.conflictAllCheck();
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    validateDateTime(index) {
        let item = this.criterias[index];
        switch (item.conditionId) {
            case this.ConditionType.FromTo://from to
                if (!item.currentDate1 || !item.currentDate2) {
                    item.noDateValue = true;
                } else {
                    item.noDateValue = false;
                    var dt1 = new Date(item.currentDate1).getTime();
                    var dt2 = new Date(item.currentDate2).getTime();
                    if (dt1 >= dt2) {
                        item.isDateValid = true;
                    } else {
                        item.isDateValid = false;
                    }
                }
                break;
            case this.ConditionType.OlderThan://older than
                break;
            case this.ConditionType.Before://before
                if (!item.currentDate1) {
                    item.noDateValue = true;
                } else {
                    item.noDateValue = false;
                }
                break;
            default:
        }
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
        return !item.noDateValue && !item.isDateValid;
    }

    validateNumber(value, excludeZero) {
        let regStr = excludeZero? "^[1-9][0-9]*$": "^[0-9]*$";
        return new RegExp(regStr).test(value);
    }

    IsWeekMonthYearUnit(curUnit)
    {
        let units = this.unitSize.slice(4, 7); //week/month/year
        return units.find(o => o.id == curUnit) !== undefined;
    }

    //清除验证信息
    clearMsg(index) {
        let item = this.criterias[index];
        item.isValid = false;
        item.isDateValid = false;
        item.noDateValue = false;
        item.notNumber = false;
        item.isExceedFiveThousandsYears = false;
        item.isConflict = false;
        item.numberLessThan1 =  false;
    }

    archiveContentvalidateInput(index) {
        return this.validateOwn(index) && this.conflictOwnCheck(index) && this.conflictAllCheck();
    }

    //点击add按钮
    addCondition(data, index, isInit) {
        let condition;
        condition = JSON.parse(JSON.stringify(this.Condition));
        //console.log(this.levelId)
        if (this.props.itemId == "sp" && this.rulTypes[this.levelId]) {
            condition.currentType = this.rulTypes[this.levelId][0];
            condition.ruleTypeId = this.rulTypes[this.levelId][0].id;
            condition.conditionTypes = this.rulTypes[this.levelId];
        } else if (this.props.itemId == "phy") {
            let pLevelId = this.getRealPhysicalLevelId();
            //console.log(pLevelId)
            condition.currentType = this.phyRulTypes[pLevelId][0];
            condition.ruleTypeId = this.phyRulTypes[pLevelId][0].id;
            condition.conditionTypes = this.phyRulTypes[pLevelId];
        }else if (this.props.itemId == "fs") {
            condition.currentType = this.fsRulTypes[0];
            condition.ruleTypeId = 1;
            condition.conditionTypes = this.fsRulTypes;
        } else if (this.props.itemId == "spLocal" && this.spLocalRulTypes[this.levelId]) {
            condition.currentType = this.spLocalRulTypes[this.levelId][0];
            condition.ruleTypeId = this.spLocalRulTypes[this.levelId][0].id;
            condition.conditionTypes = this.spLocalRulTypes[this.levelId];
        }else if (this.props.itemId == "oneDrive" && this.oneDriveRuleTypes[this.levelId]) {
            condition.currentType = this.oneDriveRuleTypes[this.levelId][0];
            condition.ruleTypeId = this.oneDriveRuleTypes[this.levelId][0].id;
            condition.conditionTypes = this.oneDriveRuleTypes[this.levelId];
        } else if (this.props.itemId == "teams" && this.teamsRuleTypes[this.levelId]) {
            condition.currentType = this.teamsRuleTypes[this.levelId][0];
            condition.ruleTypeId = this.teamsRuleTypes[this.levelId][0].id;
            condition.conditionTypes = this.teamsRuleTypes[this.levelId];
        }else if (this.props.itemId == "azureFile" && this.azureFileRulTypes) {
            condition.currentType = this.azureFileRulTypes[0];
            condition.ruleTypeId = this.azureFileRulTypes[0].id;
            condition.conditionTypes = this.azureFileRulTypes;
        } else if (this.props.itemId == "box" && this.boxRulTypes) {
            condition.currentType = this.boxRulTypes[0];
            condition.ruleTypeId = this.boxRulTypes[0].id;
            condition.conditionTypes = this.boxRulTypes;
        } else if (this.props.itemId == "google" && this.googleDriveRuleTypes) {
            condition.currentType = this.googleDriveRuleTypes[0];
            condition.ruleTypeId = this.googleDriveRuleTypes[0].id;
            condition.conditionTypes = this.googleDriveRuleTypes;
        }
        else if (this.props.itemId == "connector" && this.connectorRulTypes) {
            condition.currentType = this.connectorRulTypes[0];
            condition.ruleTypeId = this.connectorRulTypes[0].id;
            condition.conditionTypes = this.connectorRulTypes;
        }

        if (this.props.allowedRuleTypes && Array.isArray(this.props.allowedRuleTypes)) {
            condition.conditionTypes = condition.conditionTypes.filter(t => this.props.allowedRuleTypes.includes(t.id));
            if (condition.conditionTypes.length > 0) {
                condition.currentType = condition.conditionTypes[0];
                condition.ruleTypeId = condition.conditionTypes[0].id;
            }
        }

        if (typeof (data.RuleType) != "undefined") {
            this.criterias.push(
                condition
            );
            this.checkOption(data, index);
        } else {
            if (!this.state.elementsEnable) {
                let needAddCondition = true;
                if (isInit && this.moduleType == RuleModuleTypes.SOArchiver) {
                    if (this.levelId == Constants.RuleLevel.Document && (this.props.itemId == "sp" || this.props.itemId == "oneDrive" || this.props.itemId == "teams")) {
                        this.defaultSODocLevelCriteria(RM.deepcopy(condition));
                    } else if (this.levelId == Constants.RuleLevel.SiteCollection && this.props.itemId == "oneDrive") {
                        this.defaultSOSCLevelCriteria4OneDrive(RM.deepcopy(condition));
                        needAddCondition = false;
                    }
                }
                if (needAddCondition) {
                    condition.conditionTypes[0].checked = true;
                    condition.Matchs1[0].checked = true;
                    this.criterias.push(condition);
                }
                this.setState({
                    noCondition: false,
                    criterias: this.deepCopy(this.criterias)
                }, () => {
                    this.setCriteriaColumnLogicText();
                });
            }
        }
    }

    defaultSODocLevelCriteria = (condition) => {
        let documentSizeData = {
            Condition: 32,
            CombineMode: 0,
            RuleType: this.RuleType.DocumentSize,
            Value1: "1",
            Value2: "",
            Value1Unit: 2,
        };
        this.criterias.push(condition);
        this.checkOption(documentSizeData, 0);
    }

    defaultSOSCLevelCriteria4OneDrive = (condition) => {
        let urlData = {
            Condition: Constants.ConditionType.Maths,
            CombineMode: 0,
            RuleType: this.RuleType.URL,
            Value1: "*",
            Value2: "",
            Value1Unit: 1,
        };
        this.criterias.push(condition);
        this.checkOption(urlData, 0);
    }

    validateOwn(index) {
        let isValid = true;
        let condition = this.criterias[index];
        if ((condition.isText || condition.isArray) && $.trim(condition.Value1) == "" || condition.isColumn && $.trim(condition.filterName) == "") {
            condition.isValid = true;
            isValid = false;
        } else {
            condition.isValid = false;
        }
        switch (condition.ruleTypeId) {
            case this.RuleType.DocumentSize:
            case this.RuleType.Modified:
            case this.RuleType.DocumentModifiedTime:
            case this.RuleType.CreateTime:
            case this.RuleType.LastAccessTime:
            case this.RuleType.LastActiveTime:
            case this.RuleType.ColumnDateTime:
            case this.RuleType.DateTimeCustomProperty:
            case this.RuleType.SiteCollectionSizeTrigger:
            case this.RuleType.ParentLibraryDateTime:
            case this.RuleType.ParentSiteCollectionDateTime:
            case this.RuleType.PropertyBagDateTime:
                if (condition.isText) {
                    if($.trim(condition.Value1) && !this.validateNumber(condition.Value1, this.IsWeekMonthYearUnit(condition.valueUnit))) {
                        isValid = false;
                        condition.notNumber = true;
                    } else if(condition.valueUnit == 4 && Number(condition.Value1) > 1825000) { // 4 means option Days
                        isValid = false;
                        condition.isExceedFiveThousandsYears = true;
                    } else if(condition.valueUnit == 5 && Number(condition.Value1) > 260714) { // 5 means option Weeks
                        isValid = false;
                        condition.isExceedFiveThousandsYears = true;
                    } else if(condition.valueUnit == 6 && Number(condition.Value1) > 60000) { // 6 means option Months
                        isValid = false;
                        condition.isExceedFiveThousandsYears = true;
                    } else if(condition.valueUnit == 7 && Number(condition.Value1) > 5000) { // 7 means option Years
                        isValid = false;
                        condition.isExceedFiveThousandsYears = true;
                    } else {
                        condition.notNumber = false;
                        condition.isExceedFiveThousandsYears = false;
                    }
                } else {
                    condition.notNumber = false;
                    condition.isExceedFiveThousandsYears = false;
                }
                break;
            case this.RuleType.LabelPropertyText: 
                if ($.trim(condition.filterName) == "" || $.trim(condition.Value1) == "" || (condition.conditionId != Constants.ConditionType.IsEmpty) && $.trim(condition.Value2) == "") {
                    condition.isValid = true; 
                    isValid = false;
                } else {
                    condition.isValid = false;
                }
                break;
            case this.RuleType.LabelPropertyDate:
                if ($.trim(condition.filterName) == "" || $.trim(condition.Value1) == "" || (condition.conditionId == Constants.ConditionType.OlderThan) && $.trim(condition.Value2) == "") {
                    condition.isValid = true; 
                    isValid = false;
                } else if (condition.conditionId == Constants.ConditionType.OlderThan && !this.validateNumber(condition.Value2)) {
                    condition.notNumber = true;
                    isValid = false;
                } else {
                    condition.isValid = false;
                    condition.notNumber = false;
                }
                break;
            case this.RuleType.ColumnNumber:
            case this.RuleType.NumberCustomProperty:
            case this.RuleType.MetadataNumberColumn:
            case this.RuleType.ParentLibraryNumber:
            case this.RuleType.ParentSiteCollectionNumber:
            case this.RuleType.PropertyBagNumber:
                if (condition.isText) {
                    if (isNaN(condition.Value1)) {
                        isValid = false;
                        condition.notNumber = true;
                    } else {
                        condition.notNumber = false;
                    }
                } else {
                    condition.notNumber = false;
                }
                break;
            case this.RuleType.KeepTheLatestVersion:
                if (condition.isText) {
                    // isNaN(condition.Value1) || condition.Value1 < 0 || condition.Value1 % 1 !== 0
                    if (!this.validateNumber(condition.Value1)) {
                        isValid = false;
                        condition.notNumber = true;
                    } else {
                        condition.notNumber = false;
                    }
                } else {
                    condition.notNumber = false;
                }
                break;
            case this.RuleType.LabelPropertyNumber:
                if ($.trim(condition.filterName) == "" || $.trim(condition.Value1) == "" || $.trim(condition.Value2) === "") {
                    condition.isValid = true; 
                    isValid = false;
                } else {
                    condition.notNumber = !this.validateNumber(condition.Value2);
                    isValid = this.validateNumber(condition.Value2);
                    condition.isValid = false;
                }
                break;
            case this.RuleType.AttachmentCount:
                condition.numberLessThan1 = false;
                if(condition.isText){
                    if (isNaN(condition.Value1) || condition.Value1 < 1 || condition.Value1 % 1 !== 0) {
                        isValid = false;
                        condition.numberLessThan1 = true;
                    }
                }
                break;
            default:
                break;
        }
        if (condition.isDate1) {
            if(!this.validateDateTime(index)){
                isValid = false;
            }
        } else {
            condition.noDateValue = false;
            condition.isDateValid = false;
        }
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
        return isValid;
    }

    archiveContentCustomValidate() {
        let isVaild = true;
        let conditions = [];
        conditions = this.criterias;
        if (conditions.length == 0) {
            this.setState({
                noCondition: true
            });
            isVaild = false;
        } else {
            this.setState({
                noCondition: false
            });
            for (let i = 0; i < conditions.length; i++) {
                isVaild = this.validateOwn(i) && isVaild;
            }

            if (isVaild) {
                isVaild = this.conflictAllCheck();
            }
        }
        return isVaild;
    }

    // //点击name
    // currentTypeNameClick(index, event) {
    //     this.stopPropagation(event);
    //     this.criterias[index].isConditionTypesShow = true;
    //     this.criterias[index].isMath1popupShow = false;
    //     this.criterias[index].isMath2Selected = false;
    //     this.setState({
    //         criterias: this.deepCopy(this.criterias)
    //     });
    // }

    // Contains
    // currentMatch1Click(index, event) {
    //     this.stopPropagation(event);
    //     this.criterias[index].isMath1popupShow = true;
    //     this.criterias[index].isConditionTypesShow = false;
    //     this.criterias[index].isMath2Selected = false;
    //     this.conflictOwnCheck(index);
    //     this.setState({
    //         criterias: this.deepCopy(this.criterias)
    //     });
    // }

    // 删除红叉点击
    removeCondition(index) {
        this.hasChanged = true;
        this.criterias.splice(index, 1);
        this.setState({
            criterias: this.deepCopy(this.criterias)
        }, () => {
            this.setCriteriaColumnLogicText();
            if (this.props.onChange) {
                this.props.onChange(this.state.criterias);
            }
        });
    }

    //condtion change
    match1CondtionInputChange(index, value) {
        this.criterias[index].filterName = value.trim();
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    match2CondtionInputChange(index, value) {
        this.criterias[index].Value1 = value.trim();
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    onChangeLabelName(index, value) {
        this.criterias[index].filterName = value.trim();
        this.setState({criterias: this.deepCopy(this.criterias)});
    }

    onChangePropertyLabel(index, value) {
        this.criterias[index].Value1 = value.trim();
        this.setState({criterias: this.deepCopy(this.criterias)});
    }

    onChangePropertyLabelValue2(index, value) {
        this.criterias[index].Value2 = value.trim();
        this.setState({criterias: this.deepCopy(this.criterias)});
    }

    // //kb,gb
    // currentMatch2Click(index, event) {
    //     this.stopPropagation(event);
    //     this.criterias[index].isMath2Selected = true;
    //     this.criterias[index].isConditionTypesShow = false;
    //     this.criterias[index].isMath1popupShow = false;
    //     this.setState({
    //         criterias: this.deepCopy(this.criterias)
    //     });
    // }

    match2Click(index, args) {
        let item = args.newValue;
        let criteria = this.criterias[index];
        criteria.Matchs2.forEach(match2 => {
            if (match2.id == item.id) {
                match2.checked = true;
            } else {
                match2.checked = false;
            }
        });
        criteria.currentMatch2 = item;
        criteria.valueUnit = item.id;
        criteria.isMath2Selected = false;
        this.checkConditionId(item.id, item, false, index);
        this.clearMsg(index);
        this.conflictOwnCheck(index);
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    onArrayChange(index, args) {
        let criteria = this.criterias[index];
        let listIn = [];
        for (let item of args.newValue) {
            listIn.push(item.name);
        }
        criteria.Value1 = listIn.join(";");
        this.clearMsg(index);
        this.archiveContentvalidateInput(index);
        this.conflictOwnCheck(index);
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    convertRuleFilter(type) {
        let timeZoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        let filters = [];
        let items = this.deepCopy(this.state.criterias);
        let num = 1;
        let LevelId = "";
        if (type == "sp" || type == "phy" || type == "spLocal" || type == "oneDrive" || type == "teams" || type == "connector") {
            LevelId = this.levelId;
        }
        if (type == "exo") {
            LevelId = 6553601;
        }

        if (type == "fs") {
            LevelId = '1048576';
        }

        if (type == "azureFile") {
            LevelId = '4194304';
        }

        if (type == "box") {
            LevelId = '8388608';
        }

        if (type == "google") {
            LevelId = '16777216';
        }

        let CombineMode = this.state.CombineMode;
        for (let key of items) {
            let Value1 = key.Value1, value2 = key.Value2, value3 = "", unit1 = key.valueUnit, ConditionType = this.ConditionType, unit2 = key.valueUnit;
            switch (key.conditionId) {
                case ConditionType.FromTo://from to
                    if (key.currentType.isGoogle) {
                        value2 = RM.TimeUtil.getCommonDateStr(key.currentDate1);
                        value3 = RM.TimeUtil.getCommonDateStr(key.currentDate2);
                    } else {
                        Value1 = RM.TimeUtil.getCommonDateStr(key.currentDate1);
                        value2 = RM.TimeUtil.getCommonDateStr(key.currentDate2);
                    }
                    break;
                case ConditionType.OlderThan://older than
                    if (key.currentType.isGoogle) {
                        unit1 = 0;
                    } else {
                        unit2 = 0;
                    }
                    break;
                case ConditionType.Before://before
                    if (key.currentType.isGoogle) {
                        value2 = RM.TimeUtil.getCommonDateStr(key.currentDate1);
                    } else {
                        Value1 = RM.TimeUtil.getCommonDateStr(key.currentDate1);
                    }
                    break;
                case ConditionType.IsEmpty: 
                    value2 = "";
                    break;
                default:
            }
            if (key.ruleTypeId == this.RuleType.ColumnBoolean
                || key.ruleTypeId == this.RuleType.BooleanCustomProperty
                || key.ruleTypeId == this.RuleType.ParentLibraryYestNo
                || key.ruleTypeId == this.RuleType.ParentSiteCollectionYestNo
                || key.ruleTypeId == this.RuleType.PropertyBagBoolean
                || key.ruleTypeId == this.RuleType.OrphanedFolder) {//column yes
                Value1 = key.currentMatch2.value;
            }
            if (key.ruleTypeId == this.RuleType.ColumnNumber) {//column yes
                value2 = key.currentMatch1.value;
            }
            
            let filter = {
                Level: LevelId,
                CombineMode: key.CombineMode,
                filterName: key.filterName,
                Condition: key.conditionId,
                RuleType: key.ruleTypeId,
                Value1: Value1,
                Value2: value2,
                Value3: value3,
                Value1Unit: unit1,// documnet size
                SequenceNo: num++,
                Value2Unit: unit2,
            };
            filter.StartTimeInfo = {
                IsDayLightSaving: timeZoneInfo.autoAdjustClock,
                TimeZoneId: timeZoneInfo.id
            };
            filter.EndTimeInfo = {
                IsDayLightSaving: timeZoneInfo.autoAdjustClock,
                TimeZoneId: timeZoneInfo.id
            };
            filters.push(
                filter
            );
        }
        return filters;
    }

    //阻止冒泡
    stopPropagation(e) {
        e.nativeEvent.stopImmediatePropagation();
    }

    //深复制
    deepCopy(value) {
        return [...value];
    }

    getCriteriaColumnLogicOptions(value) {
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

    swicthLogicButton(index, logicValue) {
        let item = this.criterias[index];
        item.CombineMode = logicValue;
        this.setState({ criterias: RM.deepcopy(this.criterias) }, () => {
            this.setCriteriaColumnLogicText();
        });
    }

    setCriteriaColumnLogicText() {
        let logicOptions = RM.deepcopy(this.logicOptions);
        let criterias = this.state.criterias;
        let lastAndOrOperator = -1;
        let andOrExpression = "(";
        for (let index in criterias) {
            let item = criterias[index];
            let selectedLogicOption = logicOptions.find(option => { return item.CombineMode === option.value; });
            let sequenceNo = (index * 1 + 1);
            let operatorAndOr = selectedLogicOption.name;

            if (index == criterias.length - 1) {
                // last
                andOrExpression = `${andOrExpression}${sequenceNo})`;
                continue;
            }
            if (lastAndOrOperator != -1 && lastAndOrOperator != item.CombineMode) {
                andOrExpression = `(${andOrExpression}${sequenceNo}) ${operatorAndOr} `;
            }
            else {
                andOrExpression = `${andOrExpression}${sequenceNo} ${operatorAndOr} `;
            }
            lastAndOrOperator = item.CombineMode;
        }
        this.setState({ criteriaColumnLogicText: andOrExpression });
    }

    findDataById(data, id) {
        let result;
        for (let i = 0; i < data.length; i++) {
            if (data[i].id == id) {
                result = data[i];
                break;
            }
        }
        return result;
    }

    renderLastAccessTimeMsg(criteria){
        let showTipSourceIds = ["sp", "oneDrive", "teams"];
        if((criteria.ruleTypeId === this.RuleType.LastAccessTime || criteria.ruleTypeId === this.RuleType.LastActiveTime) && showTipSourceIds.includes(this.props.itemId)){
            let validMsgClass = this.state.criterias.length > 1 ? "cr-short-valid-msg" : "cr-normal-valid-msg";
            return (
                <>
                    <div className={validMsgClass} tabIndex="0">
                        {RMResx.RM_RDM_CR_Filte_LastAccessTime_ValidMsg}
                        {this.props.lastAccessTimeCollection && <div>
                            <$g.I18NProvider msg={RMResx.RM_RDM_CR_Filte_LastAccessTime_CollectionMsg}>
                                {this.props.lastAccessTimeCollection}
                            </$g.I18NProvider>
                        </div>}
                    </div>
                </>
            )
        }
    }

    render() {
        return <div>
            <div className="ra-createRule-criteria-content" style={{background: (this.state.elementsEnable) ? "#e6e6e6" : "#fff"}}>
                {this.state.criterias.map((criteria, index) => {
                    let criteriasCount = this.state.criterias.length;
                    let isLastColumn = index == criteriasCount - 1;
                    let isExitDateRange = criteria.isDate1 && criteria.isDate2;
                    let isExitArray = criteria.isArray;
                    let idPrefix = "raCr" + this.props.id;
                    const isGoogleSupportLabel = criteria.currentType?.isGoogle;
                    //当只有一行时只有一个+button，两行会多一个-button，因此当分两行时，第一行的宽度不同。
                    let firstRowWidth = (isGoogleSupportLabel || isExitDateRange || isExitArray) ? ( criteriasCount > 1 ? "calc(100% - 70px)" : "calc(100% - 34px)") : "100%";
                    let criteriaColumnLogicOptions = this.getCriteriaColumnLogicOptions(criteria.CombineMode);
                    return <div key={index} onClick={this.criteriaClick.bind(this, index)} className="rule-criteria-column">
                            {isGoogleSupportLabel &&
                                <div className="criteria-column margin-bottom-s" style={{width: firstRowWidth}}>
                                    <R.Combobox
                                        id={idPrefix + "FilterType" + index}
                                        width={0}
                                        searchable={false}
                                        textField='Name'
                                        valueField='id'
                                        checkedField='checked'
                                        items={criteria.conditionTypes}
                                        onChange={this.conditionTypeClick.bind(this, index)}
                                    />
                                    <R.Input
                                        id={idPrefix + "FilterType" + index}
                                        placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterLabelName}
                                        value={criteria.filterName || ""}
                                        onChange={this.onChangeLabelName.bind(this, index)}
                                        onBlur={this.archiveContentvalidateInput.bind(this, index, 1)}
                                    />
                                    <R.Input
                                        id={idPrefix + "FilterType" + index}
                                        placeholder={RMResx.RM_RDM_CreateRule_PlaceHolder_EnterPropertyName}
                                        value={criteria.Value1 || ""}
                                        onChange={this.onChangePropertyLabel.bind(this, index)}
                                        onBlur={this.archiveContentvalidateInput.bind(this, index, 1)}
                                    />
                                </div>
                            }
                        <div className= {(isExitDateRange || isExitArray) ? "margin-top-s" : "flex margin-top-s"} >
                            <div className="criteria-column"  
                                style={{width: firstRowWidth}}>
                                {!isGoogleSupportLabel && <R.Combobox
                                    id={idPrefix + "FilterType" + index}
                                    width={0}
                                    searchable={false}
                                    textField='Name'
                                    valueField='id'
                                    checkedField='checked'
                                    items={criteria.conditionTypes}
                                    onChange={this.conditionTypeClick.bind(this, index)}
                                />}
                                {
                                    !isGoogleSupportLabel && criteria.isColumn && <R.Input
                                        id={idPrefix + "FilterFirMatchIpt" + index}
                                        placeholder={criteria.columnNamePlaceholder}
                                        value={criteria.filterName || ""}
                                        onChange={this.match1CondtionInputChange.bind(this, index)}
                                        onBlur={this.archiveContentvalidateInput.bind(this, index)}
                                    />
                                }
                                {
                                    <R.Combobox
                                        id={idPrefix + "FilterFirMatchCmb" + index}
                                        width={0}
                                        searchable={false}
                                        textField='Name'
                                        valueField='id'
                                        checkedField='checked'
                                        items={criteria.Matchs1}
                                        onChange={this.match1Click.bind(this, index)}
                                    />
                                }
                                {
                                    criteria.isText && <R.Input
                                        id={`${idPrefix}FilterSecMatchIpt${index}`}
                                        placeholder={criteria.columnValuePlaceholder}
                                        value={isGoogleSupportLabel ? criteria.Value2 : criteria.Value1 || ""}
                                        onChange={isGoogleSupportLabel ? 
                                            this.onChangePropertyLabelValue2.bind(this, index) : 
                                            this.match2CondtionInputChange.bind(this, index)
                                        }
                                        onBlur={this.archiveContentvalidateInput.bind(this, index, 1)}
                                    />
                                }
                                {
                                    criteria.isMath2 && <R.Combobox
                                        id={idPrefix + "FilterSecMatchCmb" + index}
                                        width={0}
                                        textField='Name'
                                        valueField='id'
                                        checkedField='checked'
                                        items={criteria.Matchs2}
                                        searchable={false}
                                        onChange={this.match2Click.bind(this, index)}/>
                                }
                                {
                                    criteria.isDate1 && !criteria.isDate2 && <R.Datepicker
                                        id={idPrefix + "FilterFirDate" + index}
                                        selectedDate={criteria.currentDate1}
                                        data-part="vtWidget"
                                        width={0}
                                        dateTimeFormat={this.dateTimeFormat}
                                        disabled={false}
                                        hasTimePicker={true}
                                        onChange={this.dateTimeBeforeSelectChange.bind(this,index)}
                                    />
                                }
                                {
                                    criteria.isDate2 && !criteria.isDate1 && <R.Datepicker
                                        id={idPrefix + "FilterSecDate" + index}
                                        selectedDate={criteria.currentDate2}
                                        data-part="vtWidget"
                                        width={0}
                                        dateTimeFormat={this.dateTimeFormat}
                                        disabled={false}
                                        hasTimePicker={true}
                                        onChange={this.dateTimeAfterSelectChange.bind(this,index)}
                                    />
                                }
                            </div>
                            {
                                isExitDateRange && this.renderLastAccessTimeMsg(criteria)
                            }
                            
                            <div className= {(isExitDateRange || isExitArray) ? "criteria-column margin-top-s" : "flex"}>
                                { criteria.isDate1 && criteria.isDate2 && <$g.DateAndTimeRangePicker
                                    startPickerInfo={{selectedDate: criteria.currentDate1, verifyMsg: false}}
                                    endPickerInfo={{selectedDate: criteria.currentDate2, verifyMsg: false}}
                                    onChange={this.dateTimeRangeSelectChange.bind(this, index)}
                                /> 
                                }
                                {
                                    isExitArray && <R.RichCombobox
                                        textField="name"
                                        valueField="id"
                                        silence={true}
                                        items={criteria.Value1 ? this.getListInItems(criteria.Value1.split(";")) : []}
                                        doMatch={this.doMatchArray}
                                        searchPlaceholder={RMResx.RM_FA_Discovery_ArrayConditionWatermark}
                                        onChange={this.onArrayChange.bind(this, index)}
                                    />
                                }
                                {this.state.criterias.length > 1 && <R.Button
                                    type="bald"
                                    icon="crm-criteria fia-close"
                                    tooltip={RMResx.RM_JS_Common_Delete}
                                    onClick={this.removeCondition.bind(this, index)} />}
                                <R.Button
                                    id={idPrefix + "AddBtn" + index}
                                    type="bald"
                                    icon="crm-criteria fia-plus"
                                    tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                                    onClick={this.addCondition.bind(this, {}, index, false)} />
                            </div>
                        </div>                         
                        <div>
                            {
                                !isExitDateRange && this.renderLastAccessTimeMsg(criteria)
                            }
                            <$g.ValidationMsg show={criteria.isValid}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={criteria.isDateValid}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionDateTime}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={criteria.noDateValue}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionBlankDateTime}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={criteria.isConflict}>
                                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionConflict}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={criteria.notNumber}>
                                {RMResx.RM_JS_RDM_NotNumber}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={criteria.isExceedFiveThousandsYears}>
                                {RMResx.RM_JS_RDM_ExceedFiveThousandsYears}
                            </$g.ValidationMsg>
                            <$g.ValidationMsg show={criteria.numberLessThan1}>
                                {RMResx.RM_Common_Valid_NoLessThan.format("1")}
                            </$g.ValidationMsg>
                            {!isLastColumn && <div className="criteria-column-logic">
                                {criteriaColumnLogicOptions.map((item, index2) => {
                                    return <div
                                        tabIndex="0"
                                        role="button"
                                        key={index2}
                                        className={item.checked ? "logic-btn-ckecked" : "logic-button"}
                                        onClick={this.swicthLogicButton.bind(this, index, item.value)}
                                    >
                                        {item.name}
                                    </div>;
                                })}
                            </div>}
                        </div>
                    </div>;
                })}
                <div className="criteria-filter-logic-text" tabIndex="0">
                    {this.state.criteriaColumnLogicText}
                </div>
            </div>
            <$g.ValidationMsg show={this.state.noCondition}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_noCriteria}
            </$g.ValidationMsg>
        </div>;
    }
}
