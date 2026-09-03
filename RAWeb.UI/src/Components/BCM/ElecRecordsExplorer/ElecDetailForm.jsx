import { Component } from "react";
import { bindEvents, getActionDueDateI18n } from "../../../Utilities/CommonUtil";
import { SourceFlags } from "../../../Constants/Constants";
import {
    ManualReviewTableTemplate,
    RecordHistoryTableTemplate
} from "./ElecDetailTemplate";
import RuleDetail from "../../Common/RuleDetail/Index";
import StringUtil from "../../../Utilities/StringUtil";
import RuleUtil from "../../../Utilities/RuleUtil";

export default class EleDetailForm extends Component {
    constructor(props) {
        super(props);

        bindEvents(this, "handleTabIndexChanged", "openRuleDetail", "onPageChange");

        this.isSP = this.props.sourceFlag == SourceFlags.SP;
        this.isFS = this.props.sourceFlag == SourceFlags.FS;
        this.isSPLocal = this.props.sourceFlag == SourceFlags.SPLocal;
        this.isOneDrive = this.props.sourceFlag == SourceFlags.OneDrive;
        this.isAzureFileShare = this.props.sourceFlag == SourceFlags.AzureFile;
        this.isBox = this.props.sourceFlag == SourceFlags.Box;
        this.isGoogle = this.props.sourceFlag == SourceFlags.Google;
        this.isTeams = this.props.sourceFlag == SourceFlags.Teams;

        this.tabChangedIdxs = [0];

        this.state = {
            tabIndex: 0,
            tabTitles: this.getTabPanels(),
            dataSource: this.getDataSource(),
            currentRecordInfo: {},
            underReviewInfo: {},
            generalProData: (this.isSP || this.isFS || this.isSPLocal || this.isOneDrive || this.isAzureFileShare || this.isBox || this.isGoogle || this.isTeams) ? this.getSpGeneralProData() : this.getExoGeneralProData(),
            //Manual Review Information
            manualReviewColumns: this.initManualReviewColumns(),
            manualReviewPager: {
                itemsCount: 0,
                pagerIndex: 0,
                pagerSize: 10
            },
            //Record History
            recordHistoryColumns: this.initRecordHistoryColumns(),
            recordHistoryInfo: [],
            recordHistoryPager: {
                itemsCount: 0,
                pagerIndex: 0,
                pagerSize: 10
            },

            showRuleDetailPanel: { show: false },
            showRuleDetail: true,
            //前台分页，当前分页下的数据信息
            curRecordHistoryInfo: [],
        };
    }

    componentDidMount() {
        this.initDetail();
    }

    getTabPanels() {
        let tabTitles = [
            RMResx.RM_JS_BCM_Explorer_Details_Tab_UnderReview,
            RMResx.RM_JS_BCM_Explorer_Details_Tab_GeneralProperties,
            RMResx.RM_JS_BCM_Explorer_Details_Tab_RecordHistory,
        ];
        return tabTitles;
    }

    getDataSource() {
        return {
            "-1": "None",
            "0": "All",
            "1": RMResx.RM_JS_SPS_TabLabel_SP,
            "2": RMResx.RM_JS_SPS_TabLabel_FS,
            "3": RMResx.RM_JS_SPS_TabLabel_EXO,
            "4": RMResx.RM_JS_SPS_TabLabel_Physical,
            "5": RMResx.RM_Common_SharePointOnPremise,
            "6": RMResx.RM_JS_SPS_TabLabel_OneDrive,
            "7": RMResx.RM_JS_SPS_TabLabel_AF,
            "8": RMResx.RM_JS_SPS_TabLabel_Box,
            "9": RMResx.RM_JS_SPS_TabLabel_GoogleDrive,
            "11": RMResx.RM_JS_SPS_TabLabel_Teams,
        };
    }

    getSpGeneralProData() {
        return [
            {
                colName: RMResx.RM_PRM_PRE_MRR_Column_Type,
                colValueAttr: 'DateType',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_FileSize,
                colValueAttr: 'FileSize',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_CreatedTime,
                colValueAttr: 'TimeCreated',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_CreatedBy,
                colValueAttr: 'CreatedBy',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_ModifiedTime,
                colValueAttr: 'TimeModified',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_ModifiedBy,
                colValueAttr: 'ModifiedBy',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_FolderPath,
                colValueAttr: 'FolderPath',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_RecordCreatedTime,
                colValueAttr: 'CollectionTime',
            }
        ];
    }

    getExoGeneralProData() {
        return [
            {
                colName: RMResx.RM_PRM_PRE_MRR_Column_Type,
                colValueAttr: 'DateType',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_FileSize,
                colValueAttr: 'FileSize',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_SendTime,
                colValueAttr: 'SendTime',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_Sender,
                colValueAttr: 'Sender',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_Recipient,
                colValueAttr: 'Recipient',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_OriginalLocation,
                colValueAttr: 'FolderPath',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_Attachment,
                colValueAttr: 'Attachment',
            },
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_RecordCreatedTime,
                colValueAttr: 'CollectionTime',
            }
        ];
    }

    initManualReviewColumns() {
        return [{
            header: RMResx.RM_JS_BCM_Explorer_Details_ReviewedTime,
            width: 280,
            resizeable: true,
        }, {
            header: RMResx.RM_JS_BCM_Explorer_Details_ReviewedBy,
            width: 280,
            resizeable: true,
        }, {
            header: RMResx.RM_JS_BCM_Explorer_Details_ReviewedAction,
            width: 280,
            resizeable: true,
        }];
    }

    initRecordHistoryColumns() {
        return [{
            header: RMResx.RM_JS_BCM_Explorer_Details_HistoryTime,
            width: 235,
            resizeable: true,
        }, {
            header: RMResx.RM_JS_BCM_Explorer_Details_HistoryUser,
            width: 200,
            resizeable: true,
        }, {
            header: RMResx.RM_JS_BCM_Explorer_Details_HistoryAction,
            width: 180,
            resizeable: true,
        }, {
            header: RMResx.RM_JM_Comment,
            width: 150,
            resizeable: true,
        }
        ];
    }

    handleTabIndexChanged(tabIndex) {
        let isTabChanged = true; //判断当前的tab是否被选中过（选中的数据就不会重新调接口）
        if (this.tabChangedIdxs.indexOf(tabIndex) == -1) {
            this.tabChangedIdxs.push(tabIndex);
            isTabChanged = false;
        }
        this.setState({
            tabIndex: tabIndex
        }, () => {
            if (!isTabChanged) {
                this.initDetail();
            }
        });
    }

    initDetail() {
        $$.loading(true);
        let param = RM.deepcopy(this.props.data);
        param.tab = this.state.tabIndex + 1;//后台的tab初始1，组件index从0开始
        if (param.tab == 3) {
            param.tab = 4;
        }
        let url = `/api/RecordsExplorerApi/LoadDetails`;
        let option = {
            url: url,
            method: "POST",
            data: param
        };
        fetchUtility(option, response => {
            this.handleError(response);
        }).then((res) => {
            $$.loading(false);
            let detailData = JSON.parse(res);
            this.setTabContent(detailData);
            this.props.detailRecordData(detailData);
            if(param.tab == 1){
                this.setState({currentRecordInfo: detailData.Record});
            }
        });
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
                        classify: "alt",
                        onClick: () => { $$.messagedialog(false); }
                    }
                ]
            });
        }
    }

    setTabContent(detailData) {
        switch (this.state.tabIndex) {
            case 0:
                this.setState({ underReviewInfo: detailData.Summary });
                break;
            case 1:
                this.setGeneralPro(detailData);
                break;
            case 2:
                this.setState({ recordHistoryInfo: detailData.RecordHistory }, () => {
                    this.onPageChange(0, 10);
                });
                break;
        }
    }

    setGeneralPro(detailData) {
        let generalProperty = detailData.GeneralProperty;
        const textDisplaySources = [SourceFlags.FS, SourceFlags.AzureFile, SourceFlags.Box, SourceFlags.Google];
        for (let item of this.state.generalProData) {
            item.colValue = generalProperty[item.colValueAttr];
            if (item.colName == RMResx.RM_JS_BCM_Explorer_Details_FolderPath && !textDisplaySources.includes(this.sourceFlag)) {
                item.isLink = true;
            }
        }
        this.setState({
            generalProData: this.state.generalProData
        });
    }

    onPageChange(index, size, callback) {
        let pagerInfo = {};
        let recordHistoryInfo = RM.deepcopy(this.state.recordHistoryInfo) || [];
        pagerInfo.pagerSize = size;
        pagerInfo.pagerIndex = index;
        recordHistoryInfo = recordHistoryInfo.slice(index * size, (index + 1) * size);
        pagerInfo.itemsCount = (this.state.recordHistoryInfo || []).length;
        this.setState({
            curRecordHistoryInfo: recordHistoryInfo,
            recordHistoryPager: pagerInfo,
        });
        if (callback) {
            callback(true);
        }
    }

    openRuleDetail() {
        this.setState({ showRuleDetailPanel: { show: true } },()=>{
            this.ruleDetail.load({ ruleId: this.state.underReviewInfo.RuleId });
        });
    }

    getSourceIcon(flag) {
        switch (flag) {
            case SourceFlags.SP:
                return "fi-ms-sharepoint";
            case SourceFlags.FS:
                return "fia-fs font-l";
            case SourceFlags.Exo:
                return "fi-ms-exchange";
            case SourceFlags.Phy:
                return "fia-physical-record font-xl";
            case SourceFlags.SPLocal:
                return "fia-sharepoint font-xl";
            case SourceFlags.OneDrive:
                return "fi-ms-onedrive";
            case SourceFlags.AzureFile:
                return "fi-ms-azure-file-share font-l";
            case SourceFlags.Box:
                return "fia-box-blue-b font-l";
            case SourceFlags.Google:
                return "fia-google-drive-f font-l";
            case SourceFlags.Teams:
                    return "fi-ms-teams";
        }
    }

    getYesOrNo(holdStatus) {
        let yesOrNo = holdStatus ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
        return yesOrNo;
    }

    colseBtn() {
        this.setState({ showRuleDetailPanel: { show: false } });
    }

    renderUnderReview() {
        if (this.state.tabIndex == 0) {
            let underReviewInfo = this.state.underReviewInfo || {};
            let sourceIcon = this.getSourceIcon(underReviewInfo.SourceFlag);
            let isOnHold = this.getYesOrNo(underReviewInfo.HoldStatus);
            let declareAsRecordString = this.getYesOrNo(underReviewInfo.DeclareAsRecord);
            let holdType = underReviewInfo.HoldSetting ? underReviewInfo.HoldSetting.Name : '';
            let holdComment = underReviewInfo.HoldSetting ? underReviewInfo.HoldSetting.Description : '';
            let isArchived = this.getYesOrNo(this.state.currentRecordInfo.RecordStatus == 8); //8 Archived Status
            this.sourceFlag = underReviewInfo.SourceFlag;
            return <div>
                {/*OVERVIEW*/}
                <div className='ra-section-head margin-top-m' tabIndex="0">{RMResx.RM_PRM_PRE_MRR_Details_Section_OverView}</div>
                <$g.DetailList className="category-content" labelWidth={180}>
                    <$g.DetailRow>
                        <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_DataSource)}>
                            <div tabIndex="0" className="flex flex-align-center">
                                <span className={sourceIcon}>
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                    <span className="path3"></span>
                                    <span className="path4"></span>
                                    <span className="path5"></span>
                                    <span className="path6"></span>
                                </span>
                                <span className='ra-source-text'>{this.state.dataSource[underReviewInfo.SourceFlag]}</span>
                            </div>
                        </$g.DetailCell>
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_RecordName)}
                            value={underReviewInfo.LeafName} />
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_Location)}>
                            {(underReviewInfo.SourceFlag == 1 || underReviewInfo.SourceFlag == 6 || underReviewInfo.SourceFlag == 11) &&
                                <a className="ra-link-a" tabIndex="0" href={underReviewInfo.FullPath}>{underReviewInfo.FullPath}</a>}
                            {(underReviewInfo.SourceFlag != 1 && underReviewInfo.SourceFlag != 6 && underReviewInfo.SourceFlag != 11) &&
                                <div tabIndex="0">{underReviewInfo.FullPath}</div>}
                        </$g.DetailCell>
                    </$g.DetailRow>
                    {
                        (this.isSP || this.isOneDrive) && <$g.DetailRow>
                            <$g.DetailCell
                                label={RMResx.RM_JS_BCM_Explorer_Details_Archived}
                                value={isArchived} />
                        </$g.DetailRow>
                    }
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_UniqueId)}
                            value={underReviewInfo.RecordId} />
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_Classification)}>
                            <div tabIndex="0">
                                <div>{underReviewInfo.Term}</div>
                                <div>{underReviewInfo.TermSettings}</div>
                            </div>
                        </$g.DetailCell>
                    </$g.DetailRow>
                </$g.DetailList>

                {/*DISPOSAL INFORMATION*/}
                <div className='ra-section-head margin-t32' tabIndex="0">{RMResx.RM_PRM_PRE_MRR_Details_Section_DisposalInfo}</div>
                <$g.DetailList className="category-content" labelWidth={180}>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_DisposalAction)}
                            value={RuleUtil.parseDisposalActionForSP(underReviewInfo.DisposalAction)} />
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell
                            label={StringUtil.trimEndColon(getActionDueDateI18n())}
                            value={underReviewInfo.DisposalDate} />
                    </$g.DetailRow>
                    <$g.DetailRow>
                        <$g.DetailCell label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_DisposalRule)}>
                            <a className="ra-link-a" tabIndex="0" onClick={this.openRuleDetail}>{underReviewInfo.RuleName}</a>
                        </$g.DetailCell>
                    </$g.DetailRow>
                </$g.DetailList>

                {/*HOLD INFORMATION*/}
                { (!this.isGoogle) &&
                    <>
                        <div className='ra-section-head margin-t32' tabIndex="0" >{RMResx.RM_PRM_PRE_MRR_Details_Section_Hold}</div>
                        <$g.DetailList className="category-content" labelWidth={180}>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_IsOnHold)}
                                    value={isOnHold} />
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_HoldType)}
                                    value={holdType} />
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_HoldComment)}
                                    value={holdComment} />
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_HoldBy)}
                                    value={underReviewInfo.HoldBy} />
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_HoldUntil)}
                                    value={underReviewInfo.HoldReleaseTime} />
                            </$g.DetailRow>
                        </$g.DetailList>
                    </>
                }

                {/*DECLARED INFORMATION*/}
                {
                    (this.isSP || this.isSPLocal || this.isOneDrive) && <div>
                        <div className='ra-section-head margin-t32' tabIndex="0">{RMResx.RM_PRM_PRE_MRR_Details_Section_Declared}</div>
                        <$g.DetailList className="category-content" labelWidth={180}>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_DecalreAsRecord)}
                                    value={declareAsRecordString} />
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell
                                    label={StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_DeclaredBy)}
                                    value={underReviewInfo.DeclareAsRecord ? underReviewInfo.DeclaredBy : ""} />
                            </$g.DetailRow>
                        </$g.DetailList>
                    </div>
                }
            </div>;
        }
    }

    renderGeneralPro() {
        if (this.state.tabIndex == 1) {
            let generalProData = this.state.generalProData;
            return <div>
                <$g.DetailList labelWidth={180}>
                    {
                        generalProData.map((item, key) => {
                            return <$g.DetailRow key={key}>
                                {
                                    item.isLink &&
                                    <$g.DetailCell label={StringUtil.trimEndColon(item.colName)}>
                                        <a className="ra-link-a" href={item.colValue}>{item.colValue}</a>
                                    </$g.DetailCell>
                                }
                                {
                                    !item.isLink &&
                                    <$g.DetailCell label={StringUtil.trimEndColon(item.colName)} value={item.colValue} />
                                }
                            </$g.DetailRow>;
                        })
                    }
                </$g.DetailList>
            </div>;
        }
    }

    renderRecordHistory() {
        if (this.state.tabIndex == 2) {
            let recordHistoryInfo = this.state.curRecordHistoryInfo;
            let pagerInfo = this.state.recordHistoryPager;
            return <div>
                <div className='ra-section-head margin-top-m' tabIndex="0">{StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_RecordOwner_History)}</div>
                <R.Table
                    id="eleDetailRecordHis"
                    columns={this.state.recordHistoryColumns}
                    rowTemplate={RecordHistoryTableTemplate}
                    items={recordHistoryInfo}
                />
                <div className="table-foot-right">
                    <$g.Pager
                        itemsCount={pagerInfo.itemsCount}
                        pagerIndex={pagerInfo.pagerIndex}
                        pagerSize={pagerInfo.pagerSize}
                        showPagerSize={true}
                        pagerSizeOptions={[5, 10, 15]}
                        onChange={this.onPageChange} />
                </div>
            </div>;
        }
    }

    renderRuleDetailPanel() {
        return <R.Panel
            id="ruleDetailPanel"
            header={RMResx.RM_JS_BCM_Explorer_Details_RuleTitle}
            size={664}
            status={this.state.showRuleDetailPanel}
            destroy={true}
        >
            <div className="recExpRuleDetail">
                <RuleDetail
                    ref={r => this.ruleDetail = r}
                    isExistPanel={false}
                ></RuleDetail>
            </div>

            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Close} onClick={this.colseBtn.bind(this)} />
        </R.Panel>;
    }

    render() {
        return <div id='electronicDetail'>
            <R.Tabcontrol
                flex
                type="underline"
                active={this.state.tabIndex}
                onChange={this.handleTabIndexChanged}
                destroy={true}
            >
                {
                    this.state.tabTitles.map((text, index) => {
                        return <R.TabPanel tab={text} key={index}>
                            <section>
                                {this.renderUnderReview()}
                                {this.renderGeneralPro()}
                                {this.renderRecordHistory()}
                            </section>
                        </R.TabPanel>;
                    })
                }
            </R.Tabcontrol>
            {this.renderRuleDetailPanel()}
        </div>;
    }
}
