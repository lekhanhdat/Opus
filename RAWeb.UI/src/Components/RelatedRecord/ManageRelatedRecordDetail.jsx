import {SourceFlags } from "../../Constants/Constants";
import PhyObjectDetail  from "../PRM/Common/PhyObjectDetail";
import StringUtil from "../../Utilities/StringUtil";
import RuleUtil from "../../Utilities/RuleUtil";

export default class ManageRelatedRecordDetail extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            index: 0,
            generalProData: this.getSpGeneralProData(),
            detailInfo: null,
            selectedNavItem: [],
        };
    }

    componentReceive(item){
        switch (item.SourceFlag) {
            case SourceFlags.SP:
                this.setState({selectedNavItem: item},()=>{
                    this.loadSpRelatedRecordDetail();
                });
                break;
            case SourceFlags.Phy:
                this.setState({selectedNavItem: item});
                break;
        }
    }

    loadSpRelatedRecordDetail(){
        $$.loading(true);
        let selectedNavItem = this.state.selectedNavItem;
        let option = {
            url: "/api/RelatedRecordsApi/GetRelatedRecordDetail",
            data: {
                Id: selectedNavItem.id,
                SiteId: selectedNavItem.SiteId,
                SourceFlag : selectedNavItem.SourceFlag
            }
        };
        fetchUtility(option).then((res) => {
            let detailInfo = JSON.parse(res);
            this.setGeneralPro(detailInfo);
            this.setState({
                detailInfo: detailInfo
            });
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    renderRow(name, value, isLink){
        return <div className="rd-section-summary">
            <span className="rd-section-title">{StringUtil.trimEndColon(name)}</span>
            {isLink && <a className="rd-section-text" href={value}>{value}</a>}
            {!isLink && <span className="rd-section-text">{value}</span>}
        </div>;
    }
 
    getYesOrNo(holdStatus) {
        let yesOrNo = holdStatus ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No;
        return yesOrNo;
    }

    getSpGeneralProData() {
        return [   
            {
                colName: RMResx.RM_JS_BCM_Explorer_Details_DataType,
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
                colName: RMResx.RM_JS_BCM_Explorer_Details_RecordCreatedTime,
                colValueAttr: 'CollectionTime',
            }
        ];
    }

    setGeneralPro(detailData) {
        let generalProperty = detailData.GeneralProperty;
        for (let item of this.state.generalProData) {
            item.colValue = generalProperty[item.colValueAttr];
        }
        this.setState({
            generalProData: this.state.generalProData
        });
    }

    renderSPDetail(){
        let detailInfo = this.state.detailInfo;
        let selectedNavItem = this.state.selectedNavItem;
        let isSpSource = selectedNavItem.SourceFlag == SourceFlags.SP;
        if(isSpSource && detailInfo && detailInfo.Summary){
            let underReviewInfo = this.state.detailInfo.Summary;
            let isOnHold = this.getYesOrNo(underReviewInfo.HoldStatus);
            let declareAsRecordString = this.getYesOrNo(underReviewInfo.DeclareAsRecord);
            let holdType = underReviewInfo.HoldSetting ? underReviewInfo.HoldSetting.Name : "";
            let holdComment = underReviewInfo.HoldSetting ? underReviewInfo.HoldSetting.Description : "";
            let declaredBy = underReviewInfo.DeclareAsRecord ? underReviewInfo.DeclaredBy : "";
    
            return <div>
                {/*OVERVIEW*/}
                <div className="rd-detail-title">{RMResx.RM_PRM_PRE_ViewDetail}</div>
                <div className="rd-section-head">{RMResx.RM_PRM_PRE_MRR_Details_Section_OverView}</div> 
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_RecordName, underReviewInfo.LeafName)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_Location, underReviewInfo.FullPath, true)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_ID, underReviewInfo.RecordId)}
                <div className="rd-section-summary flex">
                    <div className="rd-section-title">{StringUtil.trimEndColon(RMResx.RM_JS_BCM_Explorer_Details_TermName)}</div>
                    <div className="rd-section-text">
                        <div>{underReviewInfo.Term}</div>
                        <div className="margin-top-s">{underReviewInfo.TermSettings}</div>
                    </div>
                </div>
    
                {/*DISPOSAL INFORMATION*/}
                <div className="rd-section-head">{RMResx.RM_PRM_PRE_MRR_Details_Section_DisposalInfo}</div>
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_DisposalRule, underReviewInfo.RuleName)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_DisposalAction, RuleUtil.parseDisposalActionForSP(underReviewInfo.DisposalAction))}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_DisposalDueDate, underReviewInfo.DisposalDate)}
    
                {/*HOLD INFORMATION*/}
                <div className="rd-section-head">{RMResx.RM_PRM_PRE_MRR_Details_Section_Hold}</div>
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_IsOnHold, isOnHold)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_HoldType, holdType)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_HoldComment, holdComment)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_HoldBy, underReviewInfo.HoldBy)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_HoldUntil, underReviewInfo.HoldReleaseTime)}
    
                {/*HOLD INFORMATION*/}
                <div className="rd-section-head">{RMResx.RM_PRM_PRE_MRR_Details_Section_Declared}</div>
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_DecalreAsRecord, declareAsRecordString)}
                {this.renderRow(RMResx.RM_JS_BCM_Explorer_Details_DeclaredBy, declaredBy)}
    
                {/* General Properties */}
                <div className="rd-section-head">{RMResx.RM_JS_BCM_Explorer_Details_Tab_GeneralProperties}</div>
                {
                    this.state.generalProData.map((item,key) => {
                        return <div key={key}>
                            {this.renderRow(item.colName, item.colValue)}
                        </div> ;
                    })
                }
            </div>;
        }
    }

    renderPhyDetail(){
        let selectedNavItem = this.state.selectedNavItem;
        let isPhySource = selectedNavItem.SourceFlag == SourceFlags.Phy;
        if(isPhySource && selectedNavItem){
            let data={
                isRelatedRecords: true,
                isNotShowAccessControl: true,
                isNotShowRelatedRecords: true,
                Id: selectedNavItem.id,
                SiteId: selectedNavItem.SiteId,
                SourceFlag: selectedNavItem.SourceFlag
            };
            return <div>
                <div className="rd-detail-title">{RMResx.RM_PRM_PRE_ViewDetail}</div>
                <div className="rd-section-head">
                    <PhyObjectDetail
                        data={data}
                    />
                </div> 
            </div>;
        }
    }

    render() {
        let hasNoNavItem = this.state.selectedNavItem.length == 0;
        return <div id="raManageRelatedRecordDetail" style={{display: hasNoNavItem ? "none": "block" }}>
            {this.renderSPDetail()}
            {this.renderPhyDetail()}
        </div>;
    }
}