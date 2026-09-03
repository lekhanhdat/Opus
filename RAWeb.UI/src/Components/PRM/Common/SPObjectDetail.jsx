import {Component} from "react";
import {bindEvents} from "../../../Utilities/CommonUtil";
import RelatedRecords from "../RecordsExplorer/Components/RelatedRecords";
import {YesOrNo} from "../Constants";
import RuleUtil from "../../../Utilities/RuleUtil";
import { SourceFlag } from "../../Common/Constants";
import {getActionDueDateI18n} from "../../../Utilities/CommonUtil";

export default class SPObjectDetail extends Component {
    constructor(props) {
        super(props);
        this.basicInfoColNames = [
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_DataSource),
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_RecordName),
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_Location),
            RMResx.RM_PRM_PRE_Column_ID,
            RMResx.RM_PRM_PRE_Column_DisposalClass,

            RMResx.RM_PRM_PRE_Column_RuleName,
            RMResx.RM_PRM_PRE_Column_RuleAction,
            getActionDueDateI18n(),

            RMResx.RM_PRM_PRE_Column_DisposalStatus,
            RMResx.RM_PRM_PRE_Column_HoldType,
            RMResx.RM_PRM_PRE_Column_HoldBy,
            RMResx.RM_PRM_PRE_Column_HoldUntil,
            RMResx.RM_PRM_PRE_Column_HoldComment,

            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_DecalreAsRecord),
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_DeclaredBy),

            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_DataType),
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_FileSize),
            RMResx.RM_PRM_PRE_Column_Creator,
            RMResx.RM_PRM_PRE_Column_CreatedTime,
            RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime,
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_FolderPath),
            this.delLastChar(RMResx.RM_JS_BCM_Explorer_Details_RecordCreatedTime),
        ];
        this.basicInfoColAttrs = [
            "SourceFlag",
            "LeafName",
            "FullPath",
            "RecordId",
            "Term",
            'RuleName',
            'DisposalAction',
            'DisposalDate',
            'HoldStatus',
            'HoldProfileTitle',
            'HoldBy',
            'HoldReleaseTime',
            'HoldProfileComment',
            'DeclareAsRecord',
            'DeclaredBy',
            'DateType',
            'FileSize',
            'CreatedBy',
            'TimeCreated',
            'ModifiedBy',
            'TimeModified',
            'FolderPath',
            'CollectionTime',
        ];
        this.state = {
            detailData: [],
            showRelatedRecordsPanel: {show: false},
            showRelatedRecords: false,
        };
        bindEvents(this, '');
    }

    componentDidMount() {
        this.init();
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        if (nextProps.data != this.props.data) {
            this.init();
        }
    }

    init() {
        this.initSPObjectDetail();
    }

    initSPObjectDetail() {
        $$.loading(true);
        let d = this.props.data;
        let url = `/api/RecordsExplorerApi/LoadDetails`;
        let option = {
            url: url,
            method: "POST",
            data: {
                isArchived: false,
                id: d.id,
                tab: 0
            }
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            let d = JSON.parse(res);
            let relatedCount = d.RelatedRecordInfo && d.RelatedRecordInfo.RelateRecordCount;
            this.setState({
                detailData: this.getGroupedData(d),
                showRelatedRecords: relatedCount > 0,
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getGroupedData(data) {
        let categories = [{
            name: RMResx.RM_PRM_PRE_MRR_Details_Section_OverView,
            startIndex: 0,
            endIndex: 5,
            columns:[],
        },
        {
            name: RMResx.RM_PRM_PRE_MRR_Details_Section_DisposalInfo,
            startIndex: 5,
            endIndex: 8,
            columns:[],
        },{
            name: RMResx.RM_PRM_PRE_MRR_Details_Section_Hold,
            startIndex: 8,
            endIndex: 13,
            columns:[],
        },
        {
            name: RMResx.RM_PRM_PRE_MRR_Details_Section_Declare,
            startIndex: 13,
            endIndex: 15,
            columns:[],
        },
        {
            name: RMResx.RM_PRM_PRE_MRR_Details_Section_GeneralProperties,
            startIndex: 15,
            endIndex: 24,
            columns:[],
        }
        ];
        let detailData = Object.assign(data.GeneralProperty, data.Summary);
        for(let c of categories){
            c.columns = this.getColumnsData(detailData, c.startIndex, c.endIndex);
        }
        return categories;
    }

    getColumnsData(data, startIndex, endIndex){
        let columns = [];
        let colArrNames = this.basicInfoColAttrs.slice(startIndex, endIndex);
        let colDisplayNames =  this.basicInfoColNames.slice(startIndex, endIndex);
        for(let idx in colArrNames){
            if(colArrNames.hasOwnProperty(idx)){
                let attr = colArrNames[idx];
                let item = {};
                item.columnName = colDisplayNames[idx];
                switch(attr){
                    case "HoldProfileTitle":
                        attr = "HoldSetting";
                        item.columnValue = (data[attr] && data[attr].Name) || "";
                        break;
                    case "HoldProfileComment":
                        attr = "HoldSetting";
                        item.columnValue = (data[attr] && data[attr].Description) || "";
                        break;
                    case "DeclareAsRecord":
                    case "HoldStatus":
                        item.columnValue = data[attr] ? YesOrNo[0] : YesOrNo[1];
                        break;
                    case "FullPath":
                    case "FolderPath":
                        item.columnValue = data[attr] && <a className="ra-link-a ra-cursor-pointer" target="_blank" rel='noreferrer noopener' href={data[attr]}>{data[attr]}</a>;
                        break;
                    case "DisposalAction":
                        item.columnValue = RuleUtil.parseDisposalActionForSP(data[attr]);
                        break;
                    case "SourceFlag":
                        switch (data.SourceFlag) {
                            case SourceFlag.Teams:
                                item.columnValue = <span className='flex ra-flex-align-center fi-ms-teams'>
                                    <span className="margin-left-xs">{RMResx.RM_JS_SPS_TabLabel_Teams}</span>
                                </span>;
                                break;
                            case SourceFlag.SharePointOnPrem:
                                item.columnValue = <span className='flex ra-flex-align-center fia-sharepoint'>
                                    <span className="margin-left-xs">{RMResx.RM_JS_SPS_TabLabel_SPLocal}</span>
                                </span>;
                                break;
                            default:
                                item.columnValue = <span className='flex ra-flex-align-center fi-ms-sharepoint'>
                                    <span className="margin-left-xs">{RMResx.RM_JS_SPS_TabLabel_SP}</span>
                                </span>;
                                break;
                        }
                        break;
                    default:
                        item.columnValue = data[attr || ""];
                        break;
                }
                if(data[attr] !== undefined){
                    columns.push(item);
                }
            }
        }
        return columns;
    }

    delLastChar(str){
        if(str){
            str = str.substr(0, str.length -1);
        }
        return str;
    }

    showRelatedRecordsPanel(){
        this.setState({
            showRelatedRecordsPanel: {show: true},
        });
    }

    renderColumnContent(columns) {
        return <$g.DetailList className="category-content" labelWidth={180}>
            {columns.map((column, index) => {
                let columnName = RMResx[column.columnName] ? RMResx[column.columnName] : column.columnName;
                return <$g.DetailRow key={index}>
                    <$g.DetailCell
                        label={columnName}
                        value={column.columnValue}/>
                </$g.DetailRow> ;
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
            <R.Button slot="buttons" text={RMResx.RM_JS_BCM_Explorer_Button_Back} onClick={() => {
                this.setState({ showRelatedRecordsPanel: { show: false } });
            }} />
        </R.Panel>;
    }

    render() {
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
            {
                this.state.showRelatedRecords && <div className="margin-top-xs">
                    <a className="ra-main-cell-link" onClick={this.showRelatedRecordsPanel.bind(this)}>{RMResx.RM_PRM_PRE_Column_RelatedRecords}</a>
                </div>
            }
            {this.renderRelatedRecordsPanel()}
        </div>;
    }
}
