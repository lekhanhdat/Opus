import {Component} from "react";
import {bindEvents} from "../../../Utilities/CommonUtil";
import StringUtil from '../../../Utilities/StringUtil';
import PhyColumnUtil from "../../../Utilities/PhyColumnUtil";
import {NodeType} from "../../../Constants/DAEnums";
import RelatedRecords from "../RecordsExplorer/Components/RelatedRecords";
import RuleDetail from "../../Common/RuleDetail/Index";
import {
    PhyCategoryBaseInfoId, YesOrNo
} from "../Constants";
import {
    PhysicalObjectColumnType,
    PhysicalDefaultColumnIDs,
    PhysicalObjectStatusNames,
    EmptyGUID
} from "../../../Constants/Constants";
import RuleUtil from "../../../Utilities/RuleUtil";
import {getActionDueDateI18n} from "../../../Utilities/CommonUtil";

export default class PhyObjectDetail extends Component {
    constructor(props) {
        super(props);
        this.basicInfoColNamesNotInMateInfo = [
            RMResx.RM_PRM_PRE_Column_PersonHoldStatus,
            RMResx.RM_PRM_PRE_Column_LoanBy,
            RMResx.RM_PRM_PRE_Column_ReturnDate,
            RMResx.RM_PRM_PRE_Column_RuleName,
            RMResx.RM_PRM_PRE_Column_RuleAction,
            getActionDueDateI18n(),
            RMResx.RM_PRM_PRE_Column_DisposalStatus,
            RMResx.RM_PRM_PRE_Column_HoldType,
            RMResx.RM_PRM_PRE_Column_HoldBy,
            RMResx.RM_PRM_PRE_Column_HoldUntil,
            RMResx.RM_PRM_PRE_Column_HoldComment,
            RMResx.RM_PRM_PRE_Column_Creator,
            RMResx.RM_PRM_PRE_Column_CreatedTime,
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ];
        this.basicInfoColAttrsNotInMateInfo = [
            'PersonHold',
            'PersonHoldBy',
            'PersonHoldReleaseTime',
            'RuleName',
            'RuleAction',
            'DisposalDueDate',
            'DisposalHold',
            'HoldProfileTitle',
            'HoldBy',
            'HoldReleaseTime',
            'HoldProfileComment',
            'CreatedBy',
            'CreateTime',
            'ModifiedBy',
            'ModifiedTime'
        ];
        this.containerBasicInfoColAttrsNotInMate = ['ModifiedBy', 'ModifiedTime'];
        this.containerBasicInfoColNamesNotInMate = [
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ];
        this.physicalDetailInfo = {};
        this.state = {
            detailData: [],
            showRelatedRecordsPanel: {show: false},
            showRelatedRecords: false,
            permissionInfo: null
        };
        this.isRelatedRecords = this.props.data.isRelatedRecords;
        bindEvents(this, '');
    }

    componentDidMount() {
        this.init();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.data != this.props.data) {
            this.init(nextProps.data);
        }
    }

    init(nextPropsData) {
        let data = this.props.data;
        if (data.isRequest) {
            this.initRequestFileDetail();
        } else {
            if(this.isRelatedRecords){
                this.initRelatedRecordDetail(nextPropsData);
            }else{
                this.initPhyObjecDetail();
                this.setPermissionDetail();
            }
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
        }
    }

    initRelatedRecordDetail(nextPropsData){
        $$.loading(true);
        let data = nextPropsData ? nextPropsData : this.props.data;
        let url = "/api/RelatedRecordsApi/GetRelatedRecordDetail";
        let option = {
            url: url,
            method: "POST",
            data: {
                Id: data.Id,
                SiteId: data.SiteId,
                SourceFlag: data.SourceFlag
            }
        };
        this.phyObjecDetailCallback(option);
    }

    initPhyObjecDetail() {
        $$.loading(true);
        let data = this.props.data;
        let url = `/api/PhysicalRecordApi/GetPhysicalObjectById`;
        let option = {
            url: url,
            method: "POST",
            data: {
                Id: data.id,
                NodeType: data.nodeType,
                TemplateIdPath: '',
                PhyNodeInfo: {
                    NodeType: data.nodeType,
                    BoxId: data.BoxId ?? EmptyGUID,
                    FileId: data.FileId ?? EmptyGUID,
                }
            }
        };
        this.phyObjecDetailCallback(option);
    }

    phyObjecDetailCallback(option){
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((res) => {
            $$.loading(false);
            let phyObj = JSON.parse(res);
            this.setState({
                detailData: this.getPhyObjMetadata(phyObj),
                showRelatedRecords: phyObj.RelatedRecordsCount > 0,
            });
            this.recordsStatus = phyObj.Status;
            this.physicalDetailInfo = phyObj;
        }).catch((e) => {
            $$.loading(false);
        });
    }

    initRequestFileDetail() {
        $$.loading(true);
        let url = `/api/PhysicalRequestApi/GetRequest?id=${this.props.data.requestId}`;
        let option = {
            url: url,
            method: "GET"
        };
        fetchUtility(option).then((reqDto) => {
            $$.loading(false);
            const physicalFileInfo = reqDto.PhysicalFileInfo ? reqDto.PhysicalFileInfo : reqDto.PhysicalFileInfos[0];
            let scopePerDto = physicalFileInfo.ScopePerDto;
            //scopePerDto有值代表打破继承，没有值代表继承
            if(scopePerDto){
                if(!scopePerDto.IsInheritSave){
                    scopePerDto.BreakInheritStatus = true;
                    this.setState({
                        permissionInfo: scopePerDto
                    });
                }else{
                    this.setInheritPermissionInfo(physicalFileInfo);
                }
            }
            this.setState({
                detailData: this.getPhyObjMetadata(physicalFileInfo),
            });
        });
    }

    setInheritPermissionInfo(data){
        $$.loading(true);
        //ScopePerDto如果无数据的时候，说明是继承，取父级id
        let scopePerDto = {};
        let scopeId = data.LocationId;
        if(NodeType.PhyFile){
            if(data.BoxId != EmptyGUID){
                scopeId = data.BoxId;
            }else{
                scopeId = data.LocationId;
            }
        }
        let option = {
            url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${scopeId}&includeSelf=${true}`,
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            let res = JSON.parse(result);
            scopePerDto.BreakInheritStatus = false;
            scopePerDto.Accounts = res.Accounts || [];
            this.setState({
                permissionInfo: scopePerDto
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getPhyObjMetadata(phyObj) {
        let isLocation = phyObj.NodeType <= NodeType.PhysicalBottomLocation;
        if (isLocation) {
            return this.getLocationData(phyObj);
        } else {
            let columnTitleIdx = 0;
            let columnStatusIdx = 0;
            let values = phyObj.MetaInfo;
            let templateCategories = phyObj.Template.categories;
            for (const category of templateCategories) {
                for (const index in category.columns) 
                {
                    if(category.columns[index].uniqueId == "df21d79c-bc37-fdfd-f59e-641f7d630488"){
                        category.columns.splice(index,1);
                    }
                    if(category.columns.hasOwnProperty(index)){
                        let currentColumn = category.columns[index];
                        let options = JSON.parse(currentColumn.optionsJSON);
                        currentColumn.columnValue = PhyColumnUtil.getDisplayValue(currentColumn, values);
                        if(currentColumn.uniqueId == PhysicalDefaultColumnIDs.HomeLocation){
                            currentColumn.columnValue = phyObj.HomeLocationFullPath;
                            // if(this.isRelatedRecords && phyObj.MetaInfo && phyObj.MetaInfo[PhysicalDefaultColumnIDs.HomeLocation]){
                            //     currentColumn.columnValue = JSON.parse(phyObj.MetaInfo[PhysicalDefaultColumnIDs.HomeLocation]).Name;
                            // }
                        }
                        if (currentColumn.uniqueId == PhysicalDefaultColumnIDs.Classification) {
                            currentColumn.columnValue = phyObj.TermFullPath;
                            if(this.isRelatedRecords && phyObj.MetaInfo && phyObj.MetaInfo[PhysicalDefaultColumnIDs.Classification]){
                                currentColumn.columnValue = JSON.parse(phyObj.MetaInfo[PhysicalDefaultColumnIDs.Classification]).Name;
                            }
                        }
                        if (currentColumn.typeId == PhysicalObjectColumnType.SingleChoice) {
                            let isDeleted = true;
                            for (let key in options) {
                                if(options.hasOwnProperty(key)){
                                    if (values[currentColumn.uniqueId]) {
                                        if (key == JSON.parse(values[currentColumn.uniqueId]).Value) {
                                            isDeleted = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            currentColumn.isDeleted = isDeleted;
                            if (!isDeleted) {
                                let oldColumnValue = JSON.parse(values[currentColumn.uniqueId]);
                                currentColumn.columnValue = options[oldColumnValue.Value];
                            }
                        }
                        if (currentColumn.typeId == PhysicalObjectColumnType.MultipleChoice) {
                            let isDeleted = false;
                            let mulChoiceOptValueArr = [];
                            let newMulChoiceArr = [];
                            for (let key in options) {
                                if(options.hasOwnProperty(key)){
                                    mulChoiceOptValueArr.push(key);
                                }
                            }
                            if (currentColumn.columnValue) {
                                for (let opt of JSON.parse(values[currentColumn.uniqueId])) {
                                    if (mulChoiceOptValueArr.indexOf(opt.Value) == -1) {
                                        isDeleted = true;
                                        opt.showWavyLine = true;
                                    }
                                    newMulChoiceArr.push(opt);
                                }
                            }
                            currentColumn.isDeleted = isDeleted;
                            if (!isDeleted) {
                                if (values[currentColumn.uniqueId]) {
                                    let newMulColumnValue = [];
                                    let oldColumnValue = JSON.parse(values[currentColumn.uniqueId]);
                                    oldColumnValue.filter((item) => {
                                        newMulColumnValue.push(options[item.Value]);
                                    });
                                    currentColumn.columnValue = newMulColumnValue.join("; ");
                                }
                            } else {
                                newMulChoiceArr.forEach((item) => {
                                    for (let key in options) {
                                        if (key == item.Value) {
                                            item.Name = options[key];
                                        }
                                    }
                                });
                                currentColumn.columnValue = newMulChoiceArr;
                            }
                        }
                        if (currentColumn.uniqueId == PhysicalDefaultColumnIDs.NameOrTitle) {
                            columnTitleIdx = index;
                        }
                        if (currentColumn.uniqueId == PhysicalDefaultColumnIDs.Status) {
                            columnStatusIdx = index;
                        }
                    }
                    
                }
            }
            if (phyObj.NodeType == NodeType.PhyRecord) {
                templateCategories[0].columns.push({
                    columnName: RMResx.RM_PRM_PRE_Column_Status,
                    columnValue: PhysicalObjectStatusNames[phyObj.Status]
                });
            }
            if (phyObj.DestroyedTime) {
                let destroyTimeColumn = {
                    columnName: RMResx.RM_PRM_PRE_DestroyedTime,
                    columnValue: phyObj.DestroyedTime
                };
                if (columnStatusIdx == 0) {
                    templateCategories[0].columns.push(destroyTimeColumn);
                }
                else {
                    templateCategories[0].columns.splice(columnStatusIdx * 1 + 1, 0, destroyTimeColumn);
                }
            }
            //??UniqueId column
            templateCategories[0].columns.splice(columnTitleIdx * 1 + 1, 0, {
                columnName: RMResx.RM_PRM_PRE_Column_ID,
                columnValue: phyObj.UniqueId
            });
            let basicInfoColAttrsNotInMateInfo = this.basicInfoColAttrsNotInMateInfo.slice(0);
            let basicInfoColNamesNotInMateInfo = this.basicInfoColNamesNotInMateInfo.slice(0);
            if (!phyObj.DisposalHold) {
                basicInfoColAttrsNotInMateInfo = basicInfoColAttrsNotInMateInfo.filter((value) => {
                    return value != 'HoldProfileTitle'
                        && value != 'HoldReleaseTime'
                        && value != 'HoldProfileComment';
                });
                basicInfoColNamesNotInMateInfo = basicInfoColNamesNotInMateInfo.filter((value) => {
                    return value != RMResx.RM_PRM_PRE_Column_HoldUntil
                        && value != RMResx.RM_PRM_PRE_Column_HoldType
                        && value != RMResx.RM_PRM_PRE_Column_HoldComment;
                });
            }
            if (this.props.data.isRequest) {
                basicInfoColAttrsNotInMateInfo = basicInfoColAttrsNotInMateInfo.filter((key) => {
                    return key != 'PersonHold' && key != 'PersonHoldBy' && key != 'PersonHoldReleaseTime';
                });
                basicInfoColNamesNotInMateInfo = basicInfoColNamesNotInMateInfo.filter((key) => {
                    return key != RMResx.RM_PRM_PRE_Column_PersonHoldStatus && key != RMResx.RM_PRM_PRE_Column_LoanBy && key != RMResx.RM_PRM_PRE_Column_ReturnDate;
                });
            }
            if(phyObj.NodeType == NodeType.PhyCustom) {
                basicInfoColAttrsNotInMateInfo = this.containerBasicInfoColAttrsNotInMate.slice(0);
                basicInfoColNamesNotInMateInfo = this.containerBasicInfoColNamesNotInMate.slice(0);
            }

            let categoryIndex = 0;
            for (const category of templateCategories) {
                if (categoryIndex == 0) {
                    for (let key in basicInfoColAttrsNotInMateInfo) {
                        if(basicInfoColAttrsNotInMateInfo.hasOwnProperty(key)){
                        let attr = basicInfoColAttrsNotInMateInfo[key];
                        let column = {};
                        switch (attr) {
                            case 'CreateTime' :
                            case 'ModifiedTime' :
                            case 'HoldReleaseTime' :
                            case 'PersonHoldReleaseTime' :
                                column.columnValue = phyObj[attr] > 0 ? phyObj[attr + "Str"] : '';
                                break;
                            case 'PersonHold' :
                            case 'DisposalHold' :
                                column.columnValue = phyObj[attr] ? YesOrNo[0] : YesOrNo[1];
                                break;
                            case 'PersonHoldBy' :
                            case 'HoldBy' :
                                column.columnValue = phyObj[attr] || RMResx.RM_JS_PRM_PRE_UserIsNull;
                                break;
                            case 'RuleAction' :
                                column.columnValue = this.props.isRequest? "": RuleUtil.parseDisposalAction(phyObj[attr]);
                                break;
                            default:
                                column.columnValue = phyObj[attr];
                        }
                        column.columnName = basicInfoColNamesNotInMateInfo[key];
                        category.columns.push(column);
                        }
                    }
                    categoryIndex++;
                }
            }

            templateCategories[0].columns.push({
                columnName: RMResx.RM_PRM_PRE_Column_TemplateName,
                columnValue: phyObj.Template && this.wrapperI18N(phyObj.Template.name)
            });
            return templateCategories;
        }
    }

    setPermissionDetail() {
        let data = this.props.data;
        $$.loading(true);
        let option = {
            url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${data.id}&includeSelf=${true}`,
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let res = JSON.parse(result);
            this.setState({
                permissionInfo: res
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    wrapperI18N(str) {
        return RMResx[str] ? RMResx[str] : str;
    }

    onLoadRuleDetail = () => {
        let ruleId = this.physicalDetailInfo.RuleId;
        ruleId && this.ruleDetail.load({ ruleId: ruleId });
    }

    renderDetailCellValue(column) {
        if (column.isDeleted) {
            if (column.typeId == PhysicalObjectColumnType.SingleChoice) {
                return <span className='ra-wavyLine'>{column.columnValue}</span>;
            }
            if (column.typeId == PhysicalObjectColumnType.MultipleChoice) {
                return <div>
                    {
                        column.columnValue.map((item,index) => {
                            return <span key={index} className={item.showWavyLine ? 'ra-wavyLine ' : ''}>{item.Name};</span>;
                        })
                    }
                </div>;
            }
        } else {
            if(column.columnName === RMResx.RM_PRM_PRE_Column_RuleName){
                return <a className="ra-link-a" tabIndex="0" onClick={this.onLoadRuleDetail}>{column.columnValue}</a>;
            }
            return column.columnValue;
        }
    }

    renderColumnContent(columns) {
        return <$g.DetailList className="category-content" labelWidth={180}>
            {columns.map((column, index) => {
                let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
                return <$g.DetailRow key={index}>
                    <$g.DetailCell
                        label={columnName}
                        //value={column.columnValue}
                        value={this.renderDetailCellValue(column)}
                    />
                </$g.DetailRow>;
            })}
        </$g.DetailList>;
    }

    renderRelatedRecordsPanel() {
        let item = this.props.data;
        return <R.Panel
            id="relatedRecordsPanel"
            header={RMResx.RM_PRM_PRE_MRR_RR_Title}
            size={1000}
            actionType='back'
            status={this.state.showRelatedRecordsPanel}
            destroy={true}
        >
            <div className="ra-panel-content reclassify-panel">
                <div id="reclassify-content">
                    <RelatedRecords
                        data={item}
                    > </RelatedRecords>
                </div>
            </div>
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_BCM_Explorer_Button_Back} onClick={() => {
                this.setState({ showRelatedRecordsPanel: { show: false } });
            }} />
        </R.Panel>;
    }

    showRelatedRecordsPanel() {
        this.setState({
            showRelatedRecordsPanel: {show: true},
        });
    }

    renderPermissionDetail(){
        let permissionInfo = this.state.permissionInfo;
        if(permissionInfo){
            let isInherit = !permissionInfo.BreakInheritStatus ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
            return <div>
                <div className='ra-section-head'>
                    <span tabIndex="0">{RMResx.RM_PRM_PRE_PermissionTitle}</span>
                </div>
                <$g.DetailList className="category-content" labelWidth={180}>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_InheritPermissionOrNot)}
                            value={isInherit}
                        />
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(RMResx.RM_PRM_PRE_UsersWithPermissions)}
                            value={this.renderHasPermissionUserList(permissionInfo.Accounts)}
                        />
                    </$g.DetailRow>
                </$g.DetailList>
            </div>;
        }
    }

    renderHasPermissionUserList(users){
        if(users){
            return (
                users.map((item,index) => {
                    return <div key={index}>{item.DisplayName}</div>;
                })
            );
        }
    }

    renderRuleDetailPanel(){
        return <RuleDetail ref={r => this.ruleDetail = r} panelTitle={RMResx.RM_JS_BCM_Explorer_Details_RuleTitle}></RuleDetail>;
    }

    render() {
        let isCurPhyDeleted = this.state.detailData && this.state.detailData.length == 0;
        return <div className='phyobj-detail'>
            {
                this.state.detailData.map((item, index) => {
                    let categoryName = RMResx[item.name] ? RMResx[item.name] : item.name;
                    return <div key={index} className="margin-bottom-32">
                        <div className='ra-section-head'>
                            <span tabIndex="0">{categoryName}</span>
                        </div>
                        {this.renderColumnContent(item.columns)}
                    </div>;
                })
            }
            {!isCurPhyDeleted && !this.props.data.isNotShowAccessControl && this.renderPermissionDetail()}
            {
                this.state.showRelatedRecords && !this.props.data.isNotShowRelatedRecords && <div className="margin-top-xs">
                    <a className="ra-main-cell-link" onClick={this.showRelatedRecordsPanel.bind(this)}>{RMResx.RM_PRM_PRE_Column_RelatedRecords}</a>
                </div>
            }
            {this.renderRelatedRecordsPanel()}
            {this.renderRuleDetailPanel()}
        </div>;
    }
}
