import { Component } from "react";
import { JobType, SourceFlags } from "../../../Constants/Constants";
import SPTree from "../../Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates from "../../Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../Common/Tree/Instances/EXO/ReportEXOTree";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import FSTree from "../../Common/Tree/Instances/FSTree/ReportFSTree";
import ReportBoxTree from "../../Common/Tree/Instances/BoxTree/ReportBoxTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import TermTree from "../../Common/Tree/Instances/TermTree/ReportTermTree";
import '../../../Less/RC/commonViewDetail.less';
import StringUtil from "../../../Utilities/StringUtil";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        this.columnNames = [
            RMResx.RM_JS_RC_DueDisposal_ProfileName,
            RMResx.RM_JS_Profile_Description,
            RMResx.RM_JS_TermUsageReport_SelectReportType,
            RMResx.RM_JS_TermUsageReport_TermIncludeReport,
            RMResx.RM_RC_Common_ElectronicScope
        ];
        this.state = {
            DetailData: [],
            termTreeData: null,
            sourceTreeData: null,
            profileId: this.props.viewRowId,
        };
    }

    componentDidMount() {
        this.initProfileData();
    }

    componentDidUpdate(prevProps, prevState) {
        if (prevState.profileId !== this.state.profileId) {
            this.initProfileData();
        }
    }

    static getDerivedStateFromProps(nextProps, prevState) {
        return {
            profileId: nextProps.viewRowId
        };
    }

    initProfileData() {
        let option = {
            url: "/api/TermUsageReportApi/LoadProfileById",
            method:"POST",
            data:this.state.profileId,
        };
        fetchUtility(option).then((data) => {
            this.getDetailData(data);
        }).catch((e) => {

        });
    }

    isShowTermIncludeReportColumn(profile) {
        let profileType = profile.Type;
        return profileType == JobType.BCSTermUsageReport
            || profileType == JobType.EXOTermUsageReport
            || profileType == JobType.PhysicalTermUsageReport
            || profileType == JobType.FSBCSTermUsageReport
            || profileType == JobType.OneDriveTermUsageReport
            || profileType == JobType.SPOnPremiseTermUsageReport
            || profileType == JobType.BoxBCSTermUsageReport
            || profileType == JobType.GoogleBCSTermUsageReport
            || profileType == JobType.TeamsBCSTermUsageReport;
    }

    getDetailData(profile) {
        //隐藏termTree
        let detailData = [];
        let isShowTermIncludeReportColumn = this.isShowTermIncludeReportColumn(profile);
        if (!isShowTermIncludeReportColumn) {
            let termInColumnNamesIdx = this.columnNames.indexOf(RMResx.RM_JS_TermUsageReport_TermIncludeReport);
            this.columnNames.splice(termInColumnNamesIdx, 1);
        }

        for (let columnName of this.columnNames) {
            let columnObj = {};
            columnObj.columnName = columnName;
            columnObj.columnValue = this.getColumnValue(columnName, profile);
            if (!columnName.includes(':')) {
                columnName = `${columnName}:`;
            }
            detailData.push(columnObj);
        }
        this.setState({
            DetailData: detailData,
        });
    }

    getColumnValue(columnName, profile) {
        let columnValue = null;
        switch (columnName) {
            case RMResx.RM_JS_RC_DueDisposal_ProfileName:
                columnValue = profile.ProfileName;
                break;
            case RMResx.RM_JS_Profile_Description:
                columnValue = profile.Description;
                break;
            case RMResx.RM_JS_TermUsageReport_SelectReportType:
                columnValue = this.getReportTypeName(profile.Type);
                break;
            case RMResx.RM_JS_TermUsageReport_TermIncludeReport:
                columnValue = this.renderTermScope(profile);
                break;
            case RMResx.RM_RC_Common_ElectronicScope:
                columnValue = this.renderSourceTree(profile);
                break;
        }
        return columnValue;
    }

    getReportTypeName(jobType) {
        let reportTypeName = RMResx.RM_JS_TermUsageReport_ActiveTermsReport;
        if (jobType == JobType.RetiredTermReport
            || jobType == JobType.EXORetiredTermUsageReport
            || jobType == JobType.PhysicalRetiredTermUsageReport
            || jobType == JobType.FSRetiredTermReport
            || jobType == JobType.OneDriveRetiredTermReport
            || jobType == JobType.SPOnPremiseRetiredTermUsageReport
            || jobType == JobType.BoxRetiredTermUsageReport
            || jobType == JobType.GoogleRetiredTermUsageReport
            || jobType == JobType.TeamsRetiredTermUsageReport
        ) {
            reportTypeName = RMResx.RM_JS_TermUsageReport_RetiredTermsReport;
        } else if (jobType == JobType.OrphanedTermReport
            || jobType == JobType.EXOOrphanedTermUsageReport
            || jobType == JobType.PhysicalOrphanedTermUsageReport
            || jobType == JobType.FSOrphanedTermReport
            || jobType == JobType.OneDriveOrphanedTermReport
            || jobType == JobType.SPOnPremiseOrphanedTermUsageReport
            || jobType == JobType.BoxOrphanedTermUsageReport
            || jobType == JobType.GoogleOrphanedTermUsageReport
            || jobType == JobType.TeamsOrphanedTermUsageReport
        ) {
            reportTypeName = RMResx.RM_JS_TermUsageReport_OrphanTermsReport;
        }
        return reportTypeName;
    }

    renderSourceTree(profile) {
        let SourceTree = null;
        let profileType = profile.Type;
        let sourceTreeFlags = SourceFlags.SP;
        let sourceTreeData = $.parseJSON(profile.Extension2);
        if (profileType == JobType.BCSTermUsageReport || profileType == JobType.OrphanedTermReport
            || profileType == JobType.RetiredTermReport) {
            SourceTree = TreeWithTreStates;
        } else if (profileType == JobType.EXOTermUsageReport || profileType == JobType.EXOOrphanedTermUsageReport
            || profileType == JobType.EXORetiredTermUsageReport) {
            SourceTree = EXOTree;
        } else if (profileType == JobType.PhysicalTermUsageReport || profileType == JobType.PhysicalOrphanedTermUsageReport
            || profileType == JobType.PhysicalRetiredTermUsageReport) {
            SourceTree = LocationTree;
        } else if (profileType == JobType.FSBCSTermUsageReport || profileType == JobType.FSOrphanedTermReport
            || profileType == JobType.FSRetiredTermReport) {
            SourceTree = FSTree;
            sourceTreeFlags = SourceFlags.FS;
        } else if (profileType == JobType.OneDriveTermUsageReport || profileType == JobType.OneDriveOrphanedTermReport
            || profileType == JobType.OneDriveRetiredTermReport) {
            SourceTree = TreeWithTreStates;
            sourceTreeFlags = SourceFlags.OneDrive;
        } else if (profileType == JobType.SPOnPremiseTermUsageReport || profileType == JobType.SPOnPremiseOrphanedTermUsageReport
            || profileType == JobType.SPOnPremiseRetiredTermUsageReport) {
            SourceTree = SPTree;
            sourceTreeFlags = SourceFlags.SPLocal;
        } else if (profileType == JobType.BoxBCSTermUsageReport || profileType == JobType.BoxOrphanedTermUsageReport
            || profileType == JobType.BoxRetiredTermUsageReport) {
            SourceTree = ReportBoxTree;
            sourceTreeFlags = SourceFlags.Box;
        } else if (profileType == JobType.GoogleBCSTermUsageReport || profileType == JobType.GoogleOrphanedTermUsageReport
            || profileType == JobType.GoogleRetiredTermUsageReport) {
            SourceTree = ReportGoogleTree;
            sourceTreeFlags = SourceFlags.Google;
        } else if (profileType == JobType.TeamsBCSTermUsageReport || profileType == JobType.TeamsOrphanedTermUsageReport
            || profileType == JobType.TeamsRetiredTermUsageReport) {
            SourceTree = ReportTeamsTree;
            sourceTreeFlags = SourceFlags.Teams;
        }

        if (SourceTree) {
            return (
                <div className="reco-report-view-tree">
                    <SourceTree
                        ref={r => this.refSourceTree = r}
                        readonly={true}
                        data={sourceTreeData}
                        treeSource={sourceTreeFlags}
                    />
                </div>
            );
        }
    }

    renderTermScope(profile) {
        let termTreeData = $.parseJSON(profile.Extension1);
        return (
            <div className="reco-report-view-tree">
                <TermTree
                    key={new Date().getMilliseconds()}
                    ref={r => this.refTermTree = r}
                    readonly={true}
                    data={termTreeData}
                />
            </div>
        );
    }

    checkValueIsTree = (columnName) => {
        return columnName === RMResx.RM_JS_TermUsageReport_TermIncludeReport || columnName === RMResx.RM_RC_Common_ElectronicScope;
    }

    render() {
        return (
            <div>
                {
                    this.state.DetailData.map((item, index) => {
                        return (
                            <div key={index} className="reco-report-view-item">
                                <div className="reco-report-view-item-title">
                                    {StringUtil.trimEndColon(item.columnName)}
                                </div>
                                <div className="reco-report-view-item-value">
                                    {item.columnValue}
                                </div>
                            </div>
                        );
                    })
                }
            </div>
        );
    }
}
