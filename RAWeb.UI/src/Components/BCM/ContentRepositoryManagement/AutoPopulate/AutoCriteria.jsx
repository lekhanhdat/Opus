import * as Constants from "./Constants/Constants";
import * as AutoRuleConstants from "./Constants/AutoRuleConstants";
import Enviroments from "../../../../Constants/Enviroments"
import { filterRuleTypesByLicense } from "../../../../Utilities/RuleTypeUtil";

export default class AutoCriteria extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.Matchs1 = AutoRuleConstants.Matchs1;
        this.levels = Constants.levels;
        this.criteriaIndex = 0;
        this.rulTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.rulTypes21V : Constants.rulTypesNormal);
        this.spLocalRulTypes = Constants.SPLocalRulTypes;
        this.oneDriveRuleTypes = filterRuleTypesByLicense(Constants.oneDriveRuleTypes);
        this.teamsRuleTypes = filterRuleTypesByLicense(RM.gData.enviromentName == Enviroments.ChinaNorth ? Constants.rulTypes21V : Constants.rulTypesNormal);
        this.exoRulTypes = this.filter21Vcriteria(Constants.exoRulTypes);
        this.phyRulTypes = Constants.phyRulTypes;
        this.fsRulTypes = Constants.FsRulTypes[64];
        this.azureFileRulTypes = Constants.azureFileRulTypes[64];
        this.boxRulTypes = Constants.boxRulTypes[64];
        this.googleDriveRuleTypes = Constants.GoogleDriveRuleTypes[64];
        this.phyLevelIds = Constants.phyLevelIds;
        this.Regexs = AutoRuleConstants.Regexs;
        this.RuleType = Constants.RuleType;
        this.dateOption = Constants.dateOption;
        this.ConditionType = Constants.ConditionType;
        this.unitSize = Constants.unitSize;
        this.compare = Constants.compare;
        this.Condition = AutoRuleConstants.Condition;
        this.ruleTypeId = 1;  //name 选中的type
        this.dateTimeFormat = RM.TimeUtil.getGlobalAuiFormat();//时间格式
        this.hasChanged = false;
        this.levelId = 64;  //默认的levelId
        this.criterias = [];
        this.autoColumnHeight = 34;
        this.state = {
            criterias: [],
            noCondition: false,
            AllOrAny: Constants.AllOrAny,
            CombineMode: 0,
            currentConditionSeletced: RMResx.RM_JS_RDM_CreateRule_AllOrAny_All,
            TrueOrFaseOptions: Constants.TrueOrFaseOptions,
            groupCount: this.props.groupCount
        };
    }

    componentCreate() {
        this.bind(["dateTimeBeforeSelectChange", "dateTimeAfterSelectChange", "actionMenuSelectedClick"]);//, "hideDocument"
    }

    componentReceive(action, data) {
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
                // this.levelId = data.RuleLevel;
                this.setConditionData(data);
                break;
            case Constants.dispatchAction.elementDisabled:
                this.setState({ elementsEnable: data });
                break;
            case Constants.dispatchAction.clearData:
                this.criterias = [];
                this.levelId = data;
                this.setState({ criterias: [], noCondition: false, });
                break;
        }
    }

    componentInit() {
        if (this.props.data.Filters.length == 0) {
            this.addCondition({}, 0);
        } else {
            this.setConditionData(this.props.data);
        }
        // document.addEventListener("click", this.hideDocument);
    }

    componentUpdate(prevProps, prevState) {
        // if (this.changed) {
        //     this.setConditionData(this.props.data);
        //     this.changed = false;
        // }
        // if (prevState.data !== this.props.data) {
        //     this.setConditionData(prevProps.data);
        // }
    }

    componentDestroy() {
        this.isUnmounted = true;
    }

    filter21Vcriteria(criterias){
        if(RM.gData.enviromentName == Enviroments.ChinaNorth){
            criterias[6553601] = criterias[6553601].filter((item) => item.id !== 47);
        }        
        return criterias;
    }

    //public method
    getFiltersData(indexData) {
        let saveData = null;
        let isVerificationPassed = this.archiveContentCustomValidate();
        if (isVerificationPassed) {
            saveData = this.convertRuleFilter(this.props.itemId, indexData);
            // this.props.getIsVerificationPassed(isVerificationPassed);
        } else {
            // this.props.getIsVerificationPassed(false);
        }
        // this.props.getCriteriaData(saveData);
        return saveData;
    }

    getCombineMode() {
        return this.state.CombineMode;
    }

    updateGroupCount(groupCount) {
        this.setState({ groupCount: groupCount });
    }

    criteriaClick(index) {
        this.criteriaIndex = index;
    }

    setConditionData(data) {
        let RuleFilters = [];
        let ruleData = this.getRuleDataBySourceType(data);
        RuleFilters = this.deepCopy(ruleData.Filters);
        for (let key in ruleData.Filters) {
            if (ruleData.Filters[key]) {
                this.addCondition(RuleFilters[key], key);
            }
        }
    }

    getRuleDataBySourceType(data) {
        return data;
    }

    // name选择
    conditionTypeClick(index, args) {
        let item = args.newValue;
        this.hasChanged = true;
        let criteria = this.criterias[index];
        criteria.currentType = item;
        criteria.ruleTypeId = item.id;
        criteria.isConditionTypesShow = false;
        this.checkOption(item, index);
        this.clearMsg(index);
        this.conflictOwnCheck(index);
        this.conflictAllCheck();
        this.setState({
            criterias: this.deepCopy(this.criterias)
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
        return this.levelId == 64 ? Constants.phyLevelIds.PhysicalFile : Constants.phyLevelIds.PhysicalBox;
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
                currentConditionSeletced: this.deepCopy(this.state.AllOrAny[data.CombineMode].Name),
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
                    item.currentType = this.findDataById(this.teamsRuleTypes[this.levelId], type);
                    item.conditionTypes = this.teamsRuleTypes[this.levelId];
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
                case 'googleDrive':
                    item.currentType = this.findDataById(this.googleDriveRuleTypes, type);
                    item.conditionTypes = this.googleDriveRuleTypes;
                    break;
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
        if (this.props.itemId == "sp" || this.props.itemId == "fs" || this.props.itemId == "spLocal" || this.props.itemId == "oneDrive" || this.props.itemId == "teams" || this.props.itemId == "azureFile" || this.props.itemId == "box" || this.props.itemId == "googleDrive") {
            item = this.criterias[index];
            switch (type) {
                case this.RuleType.Name:
                case this.RuleType.CreateBy:
                case this.RuleType.ModifiedBy:
                case this.RuleType.ContentType:
                case this.RuleType.ParentListId:
                case this.RuleType.Title:
                case this.RuleType.URL:
                case this.RuleType.Type:
                case this.RuleType.Owner:
                case this.RuleType.Path:
                case this.RuleType.PrimaryAdministrator:
                case this.RuleType.ParentFolderName:
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    if (type == this.RuleType.Name || type == 7 || type == this.RuleType.Title ||
                        type == this.RuleType.URL || type == this.RuleType.ParentFolderName ||
                        type == this.RuleType.Type || type == this.RuleType.Owner || type == this.RuleType.Path) {
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
                    } else {//parent list
                        dataRes = this.Regexs[4];
                        Matchs1.push(this.Regexs[4], this.Regexs[5]);
                        if (hasData && data.Condition == this.Regexs[5].id) {
                            dataRes = this.Regexs[5];
                        }
                    }
                    item.Value1 = hasData ? data.Value1 : "";
                    break;
                case this.RuleType.DocumentSize:// size
                case this.RuleType.SiteCollectionSizeTrigger: {
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = true;
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
                case this.RuleType.CreateTime://create time
                case this.RuleType.LastAccessTime:
                case this.RuleType.LastActiveTime:
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
                    item.isColumn = true;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    if (type == this.RuleType.ColumnText || type == this.RuleType.TextCustomProperty || type == this.RuleType.MetadataTextColumn
                        || type == this.RuleType.ParentLibraryText || type == this.RuleType.ParentSiteCollectionText
                    ) {//cloumn text
                        let tempRegexs = [...this.Regexs];

                        if (type == this.RuleType.ColumnText) {
                            tempRegexs.push(Constants.SpecialRegexs[0]);
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
                        item.filterName = data.filterName;
                    } else {
                        item.Value1 = "";
                        item.filterName = "";
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index);
                    break;
                case this.RuleType.ColumnBoolean://column yes
                case this.RuleType.BooleanCustomProperty:
                case this.RuleType.ParentLibraryYestNo:
                case this.RuleType.ParentSiteCollectionYestNo: {
                    let match2 = this.state.TrueOrFaseOptions[0];
                    item.isColumn = true;
                    item.isText = false;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = true;
                    dataRes = this.Regexs[4];
                    Matchs1.push(this.Regexs[4]);
                    for (let key of this.state.TrueOrFaseOptions) {
                        Matchs2.push(key);
                    }
                    if (hasData) {
                        item.filterName = data.filterName;
                        if (data.Value1 == this.state.TrueOrFaseOptions[1].Name) {
                            match2 = this.state.TrueOrFaseOptions[1];
                        }
                    } else {
                        item.filterName = "";
                    }
                    item.Matchs2 = Matchs2;
                    item.currentMatch2 = match2;
                    item.Value1 = match2.id;
                    break;
                }
                case this.RuleType.ColumnDateTime: //column date
                case this.RuleType.DateTimeCustomProperty:
                case this.RuleType.ParentLibraryDateTime:
                case this.RuleType.ParentSiteCollectionDateTime:
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
                case this.RuleType.RetentionLabelRule:
                case this.RuleType.SensitiveLabel:
                case this.RuleType.SensitiveLabelFullName:
                    item.isColumn = false;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;

                    const newRegexs = [...this.Regexs].slice(2, 6);
                    newRegexs.push(Constants.SpecialRegexs[0]);
                    dataRes = newRegexs[0];
                    for (let key of newRegexs) {
                        Matchs1.push(key);
                        if (hasData && key.id == data.Condition) {
                            dataRes = key;
                        }
                    }
                    if (dataRes.id == this.ConditionType.IsEmpty) {
                        hasData = false;
                    }
                    if (hasData) {
                        item.Value1 = data.Value1;
                        item.filterName = data.filterName;
                    } else {
                        item.Value1 = "";
                        item.filterName = "";
                    }
                    this.checkConditionId(dataRes.id, data, hasData, index);
                    break;
                default:

            }
        } else if (this.props.itemId == 'exo') {
            item = this.criterias[index];
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
                        item.sendWith = "22.7rem";
                    }
                    if (type == this.RuleType.Name || type == 7 || type == this.RuleType.Title || type == this.RuleType.URL || type == this.RuleType.Type || type == this.RuleType.Owner || type == this.RuleType.Path || type == this.RuleType.Subject) {
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
                        Matchs1.push(this.Regexs[4]);
                        if (dataRes.id == this.ConditionType.IsEmpty) {
                            hasData = false;
                        }
                    } else {//parent list
                        dataRes = this.Regexs[4];
                        Matchs1.push(this.Regexs[4], this.Regexs[5]);
                        if (hasData && data.Condition == this.Regexs[5].id) {
                            dataRes = this.Regexs[5];
                        }
                    }
                    item.Value1 = hasData ? data.Value1 : "";
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
                    }
                    let unit = this.unitSize[0];
                    dataRes = this.compare[0];
                    item.filterName = "";
                    if (type == this.RuleType.DocumentSize || this.RuleType.Size) {
                        Matchs1.push(this.compare[0], this.compare[1]);
                    } else {//SiteCollectionSizeTrigger
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
                    item.sendWith = "5rem";
            }
            if(type == this.RuleType.SendFrom || type == this.RuleType.SendTo){
                item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue_SendFromOrTo;
                item.sendWith = "22.7rem";
            }else{
                item.columnValuePlaceholder = RMResx.RM_RDM_CreateRule_PlaceHolder_EnterValue; 
                item.sendWith = "5rem";
            }
        } else {
            item = this.criterias[index];
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
                    item.Value1 = hasData ? data.Value1 : "";
                    break;
                case this.RuleType.Modified://modified time
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
                case this.RuleType.ColumnText:
                case this.RuleType.TextCustomProperty:
                case this.RuleType.NumberCustomProperty:
                    item.isColumn = true;
                    item.isText = true;
                    item.isDate1 = false;
                    item.isDate2 = false;
                    item.isMath1 = true;
                    item.isMath2 = false;
                    if (type == this.RuleType.ColumnText || type == this.RuleType.TextCustomProperty) {//cloumn text
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
                if (match.id == item.currentMatch2.id) {
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
        this.hasChanged = true;
        criteria.conditionId = item.id;  //match1Id
        criteria.currentMatch1 = item;
        criteria.isMath1popupShow = false;
        this.checkConditionId(item.id, item, false, index);
        this.clearMsg(index);
        this.conflictOwnCheck(index);
        // this.setState({
        //     criterias: this.deepCopy(this.criterias)
        // });
        this.setState({
            criterias: [...this.criterias]
        });
    }

    //验证重复规则
    conflictOwnCheck(index) {
        let isValid = true;
        let item = this.criterias;
        let item1 = item[index];
        for (let key in item) {
            if (Object.hasOwnProperty.call(item, key)) {
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
        if (this.props.itemId == "sp" || this.props.itemId == 'fs' || this.props.itemId == "spLocal" || this.props.itemId == "oneDrive" || this.props.itemId == "teams") {
            if (item1.ruleTypeId == item2.ruleTypeId && item1.conditionId == item2.conditionId) {
                switch (item1.ruleTypeId) {
                    case this.RuleType.Name:
                    case this.RuleType.CreateBy:
                    case this.RuleType.ModifiedBy:
                    case this.RuleType.ContentType:
                    case this.RuleType.ParentListId:
                    case this.RuleType.Title:
                    case this.RuleType.URL:
                    case this.RuleType.PrimaryAdministrator:
                    case this.RuleType.ParentFolderName:
                    case this.RuleType.Type:
                    case this.RuleType.Owner:
                    case this.RuleType.Path:
                    case this.RuleType.RetentionLabelRule:
                    case this.RuleType.SensitiveLabel:
                    case this.RuleType.SensitiveLabelFullName:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1);
                        break;
                    case this.RuleType.DocumentSize:
                        result = $.trim(item1.Value1) == $.trim(item2.Value1) && item1.currentMatch2.id == item2.currentMatch2.id;
                        break;
                    case this.RuleType.Modified:
                    case this.RuleType.CreateTime:
                    case this.RuleType.ColumnDateTime:
                    case this.RuleType.LastAccessTime:
                    case this.RuleType.LastActiveTime:
                    case this.RuleType.ParentLibraryDateTime:
                    case this.RuleType.ParentSiteCollectionDateTime:
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
                        if (item1.ruleTypeId == this.RuleType.ColumnDateTime || item1.ruleTypeId == this.RuleType.ParentLibraryDateTime || item1.ruleTypeId == this.RuleType.ParentSiteCollectionDateTime) {
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
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1;
                        break;
                    case this.RuleType.ColumnBoolean:
                    case this.RuleType.BooleanCustomProperty:
                    case this.RuleType.ParentLibraryYestNo:
                    case this.RuleType.ParentSiteCollectionYestNo:
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
        } else {
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
                    case this.RuleType.CreateTime:
                    case this.RuleType.ColumnDateTime:
                    case this.RuleType.LastAccessTime: // update case for azureFile
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
                    case this.RuleType.ColumnText:
                    case this.RuleType.TextCustomProperty:
                    case this.RuleType.NumberCustomProperty:
                        result = item1.filterName == item2.filterName && item1.Value1 == item2.Value1;
                        break;
                    case this.RuleType.ColumnBoolean:
                    case this.RuleType.BooleanCustomProperty:
                    case this.RuleType.ParentLibraryYestNo:
                    case this.RuleType.ParentSiteCollectionYestNo:
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
                    item.currentDate1 = data.StartTimeInfo.StartTime ? new Date(data.StartTimeInfo.StartTime) : null;
                    item.currentDate2 = data.EndTimeInfo.StartTime ? new Date(data.EndTimeInfo.StartTime) : null;
                } else {
                    item.currentDate1 = null;
                    item.currentDate2 = null;
                }
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
                    item.Value1 = data.Value1;
                    curUnit = this.findDataById(dataSize, data.Value1Unit);
                    item.currentMatch2 = curUnit;
                } else {
                    item.Value1 = "";
                    item.currentMatch2 = curUnit;
                }
                item.valueUnit = curUnit.id;
                if (item.isMath2) {
                    item.Matchs2 = RM.deepcopy(item.Matchs2);
                    item.Matchs2.forEach(match => {
                        if (match.id == item.currentMatch2.id) {
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
                if (hasData) {
                    //item.currentTimeZone = RM.TimeUtil.getTimezoneInfo(data.StartTimeInfo.TimeZoneId, data.StartTimeInfo.IsDayLightSaving);
                    item.currentDate1 = new Date(data.StartTimeInfo.StartTime);
                }
                else {
                    item.currentDate1 = null;
                }
                break;
            case this.ConditionType.IsEmpty: //Empty
                item.Value1 = "";
                item.isColumn = false;
                item.isDate2 = false;
                item.isText = false;
                item.isMath2 = false;
                item.isDate1 = false;
                item.currentDate1 = null;
                item.currentDate2 = null;
                item.Value1 = null;
                if (item.ruleTypeId == this.RuleType.ColumnText) {
                    item.isColumn = true;
                }
                break;
            case this.ConditionType.Equals:
            case this.ConditionType.IsExactlyNot:
                if (item.ruleTypeId == this.RuleType.ColumnText || item.ruleTypeId == this.RuleType.RetentionLabelRule || item.ruleTypeId == this.RuleType.SensitiveLabel || item.ruleTypeId == this.RuleType.SensitiveLabelFullName
                    || item.ruleTypeId == this.RuleType.ParentLibraryText || item.ruleTypeId == this.RuleType.ParentSiteCollectionText
                ) {
                    item.isText = true;
                }
                break;
            case this.ConditionType.Contains:
            case this.ConditionType.DoesNotContains:
            case this.ConditionType.Maths:
            case this.ConditionType.DoesNtoMath:
                if (item.ruleTypeId == this.RuleType.ColumnText || item.ruleTypeId == this.RuleType.RetentionLabelRule || item.ruleTypeId == this.RuleType.SensitiveLabel || item.ruleTypeId == this.RuleType.SensitiveLabelFullName
                    || item.ruleTypeId == this.RuleType.ParentLibraryText || item.ruleTypeId == this.RuleType.ParentSiteCollectionText
                ) {
                    item.isText = true;
                }
                break;
            default:
        }
    }

    //全部验证
    conflictAllCheck() {
        let isValid = true;
        let ruleCriterias = [];
        ruleCriterias = this.criterias;
        ruleCriterias.map((item, index) => {
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

    dateTimeRangeSelectChange(index, args) {
        let item = {};
        this.criteriaIndex = index;
        item = this.criterias[this.criteriaIndex];
        if (args.newValue.start) {
            item.currentDate1 = args.newValue.start;
        }
        if (args.newValue.end) {
            item.currentDate2 = args.newValue.end;
            item.currentDate2.setSeconds(0);
            item.currentDate2.setMilliseconds(0);
        }
        this.dateTimeSelectChange(this.criteriaIndex);
    }

    dateTimeBeforeSelectChange(index, args) {
        let item = "";
        this.criteriaIndex = index;
        item = this.criterias[this.criteriaIndex];
        item.currentDate1 = args.newValue;
        this.dateTimeSelectChange(this.criteriaIndex);

    }

    dateTimeAfterSelectChange(index, args) {
        let item = "";
        this.criteriaIndex = index;
        item = this.criterias[this.criteriaIndex];
        item.currentDate2 = args.newValue;
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
    }

    //清除验证信息
    clearMsg(index) {
        let item = this.criterias[index];
        item.isValid = false;
        item.isDateValid = false;
        item.noDateValue = false;
        item.notNumber = false;
        item.isConflict = false;
    }

    archiveContentvalidateInput(index) {
        return this.validateOwn(index) && this.conflictOwnCheck(index) && this.conflictAllCheck();
    }

    //点击add按钮
    addCondition(data, index) {
        let condition;
        condition = JSON.parse(JSON.stringify(this.Condition));
        //console.log(this.levelId)
        if (this.props.itemId == "sp") {
            condition.currentType = this.rulTypes[this.levelId][0];
            condition.ruleTypeId = this.rulTypes[this.levelId][0].id;
            condition.conditionTypes = this.rulTypes[this.levelId];
        } else if (this.props.itemId == "exo") {
            condition.currentType = this.exoRulTypes[6553601][0];
            condition.ruleTypeId = this.exoRulTypes[6553601][0].id;
            condition.conditionTypes = this.exoRulTypes[6553601];
        } else if (this.props.itemId == "phy") {
            let pLevelId = this.getRealPhysicalLevelId();
            //console.log(pLevelId)
            condition.currentType = this.phyRulTypes[pLevelId][0];
            condition.ruleTypeId = this.phyRulTypes[pLevelId][0].id;
            condition.conditionTypes = this.phyRulTypes[pLevelId];
        } else if (this.props.itemId == "fs") {
            condition.currentType = this.fsRulTypes[0];
            condition.ruleTypeId = 1;
            condition.conditionTypes = this.fsRulTypes;
        } else if (this.props.itemId == "spLocal") {
            condition.currentType = this.spLocalRulTypes[this.levelId][0];
            condition.ruleTypeId = this.spLocalRulTypes[this.levelId][0].id;
            condition.conditionTypes = this.spLocalRulTypes[this.levelId];
        } else if (this.props.itemId == "oneDrive") {
            condition.currentType = this.oneDriveRuleTypes[this.levelId][0];
            condition.ruleTypeId = this.oneDriveRuleTypes[this.levelId][0].id;
            condition.conditionTypes = this.oneDriveRuleTypes[this.levelId];
        } else if (this.props.itemId == "teams" && this.teamsRuleTypes[this.levelId]) {
            condition.currentType = this.teamsRuleTypes[this.levelId][0];
            condition.ruleTypeId = this.teamsRuleTypes[this.levelId][0].id;
            condition.conditionTypes = this.teamsRuleTypes[this.levelId];
        } else if (this.props.itemId == "azureFile") {
            condition.currentType = this.azureFileRulTypes[0];
            condition.ruleTypeId = 1;
            condition.conditionTypes = this.azureFileRulTypes;
        } else if (this.props.itemId == "box") {
            condition.currentType = this.boxRulTypes[0];
            condition.ruleTypeId = 1;
            condition.conditionTypes = this.boxRulTypes;
        } else if (this.props.itemId == "googleDrive") {
            condition.currentType = this.googleDriveRuleTypes[0];
            condition.ruleTypeId = 1;
            condition.conditionTypes = this.googleDriveRuleTypes;
        }
        if (typeof (data.RuleType) != "undefined") {
            this.criterias.splice(index + 1, 0, condition);
            this.checkOption(data, index);
        } else {
            if (!this.state.elementsEnable) {
                condition.conditionTypes[0].checked = true;
                condition.Matchs1[0].checked = true;
                this.criterias.splice(index + 1, 0, condition);
                this.setState({
                    noCondition: false,
                    criterias: this.deepCopy(this.criterias)
                });
                // }, () => { this.props.focusFirstRule(); });
            }
        }
    }

    actionMeClick(item, e) {
        this.setState({
            currentConditionSeletced: item.Name,
            CombineMode: item.id,
            isAllOrAnyShow: false
        });
        e.stopPropagation();
        if (this.refAnyOrLink) {
            this.refAnyOrLink.focus();
        }
    }

    //选中any或者all时(键盘)
    actionMeKeydown(item, event) {
        if (event.keyCode == 13) {
            this.setState({
                currentConditionSeletced: item.Name,
                CombineMode: item.id,
                isAllOrAnyShow: false
            });
            if (this.refAnyOrLink) {
                this.refAnyOrLink.focus();
            }
        }
    }

    validateOwn(index) {
        let condition = "";
        condition = this.criterias[index];
        let isValid = true;
        if (condition.isText && $.trim(condition.Value1) == "" || condition.isColumn && $.trim(condition.filterName) == "") {
            condition.isValid = true;
            isValid = false;
        } else {
            condition.isValid = false;
        }
        switch (condition.ruleTypeId) {
            case this.RuleType.DocumentSize:
            case this.RuleType.Modified:
            case this.RuleType.CreateTime:
            case this.RuleType.LastAccessTime:
            case this.RuleType.LastActiveTime:
            case this.RuleType.ColumnDateTime:
            case this.RuleType.DateTimeCustomProperty:
            case this.RuleType.SiteCollectionSizeTrigger:
            case this.RuleType.ParentLibraryDateTime:
            case this.RuleType.ParentSiteCollectionDateTime:
                if (condition.isText) {
                    if (!new RegExp("^[0-9]*$").test(condition.Value1)) {
                        isValid = false;
                        condition.notNumber = true;
                    } else {
                        condition.notNumber = false;
                    }
                } else {
                    condition.notNumber = false;
                }
                break;
            case this.RuleType.ColumnNumber:
            case this.RuleType.NumberCustomProperty:
            case this.RuleType.MetadataNumberColumn:
            case this.RuleType.ParentLibraryNumber:
            case this.RuleType.ParentSiteCollectionNumber:
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
            default:
                break;
        }
        if (condition.isDate1) {
            this.validateDateTime(index);//TODO: check logic
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

    //点击name
    currentTypeNameClick(index, event) {
        this.stopPropagation(event);
        this.criterias[index].isConditionTypesShow = true;
        this.criterias[index].isMath1popupShow = false;
        this.criterias[index].isMath2Selected = false;
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    // 删除红叉点击
    removeCondition(index) {
        this.hasChanged = true;
        this.criterias.splice(index, 1);
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
        // }, () => { this.props.focusFirstRule(); });
    }

    //condtion change
    match1CondtionInputChange(index, args) {
        this.criterias[index].filterName = args;
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    match2CondtionInputChange(index, args) {
        this.criterias[index].Value1 = args;
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    //kb,gb
    currentMatch2Click(index, event) {
        this.stopPropagation(event);
        this.criterias[index].isMath2Selected = true;
        this.criterias[index].isConditionTypesShow = false;
        this.criterias[index].isMath1popupShow = false;
        this.setState({
            criterias: this.deepCopy(this.criterias)
        });
    }

    match2Click(index, args) {
        let item = args.newValue;
        let criteria = this.criterias[index];
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

    actionMenuSelectedClick(e) {
        this.stopPropagation(e);
        this.setState({ isAllOrAnyShow: !this.state.elementsEnable });
    }

    convertRuleFilter(type, indexData) {
        let timeZoneInfo = RM.TimeUtil.getGlobalTimezoneInfo();
        let filters = [];
        let items = this.deepCopy(this.state.criterias);
        // let num = 1;
        let LevelId = "";
        if (type == "sp" || type == "phy" || type == "spLocal" || type == "oneDrive" || type == "teams") {
            LevelId = this.props.levelId;
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

        if (type == "googleDrive") {
            LevelId = '16777216';
        }

        let CombineMode = this.state.CombineMode;
        for (let key of items) {
            let Value1 = key.Value1, value2 = "", unit1 = key.valueUnit, ConditionType = this.ConditionType;
            switch (key.conditionId) {
                case ConditionType.FromTo://from to
                    Value1 = RM.TimeUtil.getCommonDateStr(key.currentDate1);
                    value2 = RM.TimeUtil.getCommonDateStr(key.currentDate2);
                    break;
                case ConditionType.OlderThan://older than
                    break;
                case ConditionType.Before://before
                    Value1 = RM.TimeUtil.getCommonDateStr(key.currentDate1);
                    break;
                default:
            }
            if (key.ruleTypeId == this.RuleType.ColumnBoolean || key.ruleTypeId == this.RuleType.BooleanCustomProperty || key.ruleTypeId == this.RuleType.ParentLibraryYestNo || key.ruleTypeId == this.RuleType.ParentSiteCollectionYestNo) {//column yes
                Value1 = key.currentMatch2.Name;
            }

            let filter = {
                Level: LevelId,
                CombineMode: CombineMode,
                filterName: key.filterName,
                Condition: key.conditionId,
                RuleType: key.ruleTypeId,
                Value1: Value1,
                Value2: value2,
                Value1Unit: unit1,// documnet size
                SequenceNo: indexData.index++
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

    getDateStringForTooltip(dateStr) {
        if (!dateStr) return "";
        return RM.TimeUtil.dateToStringSimplifyTimeZone(new Date(dateStr), RM.TimeUtil.getGlobalTimezoneInfo()).split("(")[0];
    }

    renderAllOrAny() {
        return <div
            className="ra-createRule-title-inline rm_createRule_allOrAny"
            onClick={this.actionMenuSelectedClick}>
            {<span>{this.state.currentConditionSeletced}
                {this.state.isAllOrAnyShow &&
                    <div
                        id="rm_popupTrueOrFase"
                        className='actionmenuPopupdiv ra-action-popupTrueOrFase block'>
                        <ul className="codition-ul">
                            {this.state.AllOrAny.map((item, key) => {
                                return <li className="codition-li" key={key}>
                                    <button
                                        className="action-menu-option"
                                        onClick={this.actionMeClick.bind(this, item)}
                                        onKeyDown={this.actionMeKeydown.bind(this, item)}>{item.Name}
                                    </button>
                                </li>;
                            })}
                        </ul>
                    </div>}
            </span>}
        </div>;
    }



    renderButtons(index) {
        return <React.Fragment>
            {this.state.criterias.length > 1 && <R.Button
                type="bald"
                icon="crm-criteria fia-close"
                tooltip={RMResx.RM_JS_Common_Delete}
                onClick={this.removeCondition.bind(this, index)}
            />}
            <R.Button
                type="bald"
                icon="crm-criteria fia-plus"
                tooltip={RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add}
                onClick={this.addCondition.bind(this, {}, index)}
            />
        </React.Fragment>;
    }

    renderLastAccessTimeMsg(criteria){
        let showTipSourceIds = ["sp", 'oneDrive'];
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
            );
        }
    }

    render() {
        let isEven = this.props.deepCount % 2 == 0;
        return <div data-deep-count={this.props.deepCount} id={"raCrmAutoCriteriaLevel" + this.props.deepCount}>
            <div className="ra-createRule-top flex">
                {/*Archive the content when*/}
                <$g.I18NProvider msg={RMResx.RM_SPS_AutoClassification_FollowConditions} className="ra-createRule-title-inline" tabIndex="0">
                    <a className="ra-link-a" tabIndex="0" ref={c => this.refAnyOrLink = c}>{this.renderAllOrAny()}</a>
                </$g.I18NProvider>
                <div className="ra-createRule-condition">
                    {(this.props.deepCount < 3) && <R.Button
                        text={`+ ${RMResx.RM_JS_SPS_AutoClassification_AddGroup}`}
                        classify="blank"
                        tooltip={RMResx.RM_JS_Common_Add}
                        onClick={() => { this.changed = true; this.props.addGroup(); }} />}
                    {((this.props.deepCount == 1 && this.state.groupCount > 1) || ((this.props.deepCount > 1))) && <R.Button
                        type="bald"
                        icon="fia-delete"
                        tooltip={RMResx.RM_JS_Common_Delete}
                        onClick={() => { this.changed = true; this.props.delGroup(); }} />}
                </div>
            </div>
            <div
                className="ra-autoRule-criteria-content"
                style={{ background: (this.state.elementsEnable) ? "#e6e6e6" : "#fff" }}>
                <div className="condition-group">
                    <div className={isEven ? "auto-rule-bg-color-even" : "auto-rule-bg-color-odd"}>
                        {this.state.criterias.map((criteria, index) => {
                            let needRow2 = criteria.isDate1 && criteria.isDate2;
                            let idSuffix = `-${this.props.deepCount}-${index + 1}`;
                            console.log('criteria.currentDate1: ', criteria.currentDate1);
                            return <div key={index}>
                                <div
                                    id={"raCrmAutoColumn" + index}
                                    className={"condition-group-popup " + (isEven ? "auto-condition-bg-color-even" : "auto-condition-bg-color-odd")}
                                    onClick={this.criteriaClick.bind(this, index)}>
                                    {/*type*/}
                                    <div className="condition-group-popup-row" style={needRow2 ? { maxWidth: this.state.criterias.length > 1 ? "calc(100% - 86px)" :  "calc(100% - 42px)"} : {}}>
                                        <R.Combobox
                                            id={`raCrmAutoConditionTypes${idSuffix}`}
                                            width={"100%"}
                                            height={this.autoColumnHeight}
                                            popupMaxHeight={350}
                                            items={criteria.conditionTypes}
                                            textField="Name"
                                            valueField="id"
                                            checkedField="checked"
                                            tooltipField="Name"
                                            searchable={false}
                                            onChange={this.conditionTypeClick.bind(this, index)}
                                        />
                                        {/*column name*/}
                                        {
                                            criteria.isColumn && <R.Input
                                                id={`raCrmAutoFilterName${idSuffix}`}
                                                type="text"
                                                width={"100%"}
                                                height={this.autoColumnHeight}
                                                value={criteria.filterName}
                                                onChange={this.match1CondtionInputChange.bind(this, index)}
                                                onBlur={this.archiveContentvalidateInput.bind(this, index)}
                                                placeholder={criteria.columnNamePlaceholder}
                                            />
                                        }
                                        {/*match*/}
                                        {
                                            criteria.isMath1 && <R.Combobox
                                                id={`raCrmAutoMatchFir${idSuffix}`}
                                                width={"100%"}
                                                height={this.autoColumnHeight}
                                                popupMaxHeight={350}
                                                items={criteria.Matchs1}
                                                textField="Name"
                                                valueField="id"
                                                checkedField="checked"
                                                searchable={false}
                                                onChange={this.match1Click.bind(this, index)}
                                            />
                                        }
                                        {
                                            criteria.isText && <R.Input
                                                id={`raCrmAutoValueFir${idSuffix}`}
                                                type="text"
                                                width={"100%"}
                                                height={this.autoColumnHeight}
                                                value={criteria.Value1}
                                                onChange={this.match2CondtionInputChange.bind(this, index)}
                                                onBlur={this.archiveContentvalidateInput.bind(this, index)}
                                                placeholder={criteria.columnValuePlaceholder}
                                            />
                                        }
                                        {
                                            criteria.isMath2 && <R.Combobox
                                                id={`raCrmAutoMatchsSec${idSuffix}`}
                                                width={"100%"}
                                                height={this.autoColumnHeight}
                                                popupMaxHeight={350}
                                                items={criteria.Matchs2}
                                                textField="Name"
                                                valueField="id"
                                                checkedField="checked"
                                                searchable={false}
                                                onChange={this.match2Click.bind(this, index)}
                                            />
                                        }
                                        {
                                            criteria.isDate1 && !criteria.isDate2 &&
                                            <div className='condition-date1'>
                                                <R.Datepicker
                                                    id={`raCrmAutoDateFir${idSuffix}`}
                                                    selectedDate={criteria.currentDate1}
                                                    tooltip={this.getDateStringForTooltip(criteria.currentDate1)}
                                                    data-part="vtWidget"
                                                    dateTimeFormat={this.dateTimeFormat}
                                                    disabled={false}
                                                    hasTimePicker={true}
                                                    onChange={this.dateTimeBeforeSelectChange.bind(this, index)}
                                                />
                                            </div>
                                        }
                                        {
                                            criteria.isDate2 && !criteria.isDate1 &&
                                            <div className='condition-date2'>
                                                <span className='condition-date-to'>{RMResx.RM_JS_RDM_CreateRule_DateOption_To}</span>
                                                <R.Datepicker
                                                    id={`raCrmAutoDateSec${idSuffix}`}
                                                    selectedDate={criteria.currentDate2}
                                                    tooltip={this.getDateStringForTooltip(criteria.currentDate2)}
                                                    data-part="vtWidget"
                                                    dateTimeFormat={this.dateTimeFormat}
                                                    disabled={false}
                                                    hasTimePicker={true}
                                                    onChange={this.dateTimeAfterSelectChange.bind(this, index)}
                                                />
                                            </div>
                                        }
                                        {!(needRow2) && this.renderButtons(index)}
                                    </div>
                                    {
                                        needRow2 && this.renderLastAccessTimeMsg(criteria)
                                    }
                                    {needRow2 && <div className="condition-group-popup-row">
                                        <$g.DateAndTimeRangePicker
                                            startPickerInfo={{selectedDate: criteria.currentDate1, verifyMsg: false, id: `raCrmAutoStartDate${idSuffix}`}}
                                            endPickerInfo={{selectedDate: criteria.currentDate2, verifyMsg: false, id: `raCrmAutoEndDate${idSuffix}`}}
                                            onChange={this.dateTimeRangeSelectChange.bind(this, index)}
                                        /> 
                                        {this.renderButtons(index)}
                                    </div>
                                    }

                                </div>
                                <div className="margin-bottom-s">
                                    {
                                        !needRow2 && this.renderLastAccessTimeMsg(criteria)
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
                                </div>
                            </div>;
                        })
                        }
                    </div>
                </div>
                <$g.ValidationMsg show={this.state.noCondition}>
                    {RMResx.RM_JS_RDM_CreateRule_Validation_noCriteria}
                </$g.ValidationMsg>
            </div>
        </div>;
    }
}
