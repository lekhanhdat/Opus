import { Component } from "react";
import { JobType, SourceFlags } from "../../../Constants/Constants";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import SPTree from "../../Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates  from "../../Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../Common/Tree/Instances/EXO/ReportEXOTree";
import FSTree from "../../Common/Tree/Instances/FSTree/ReportFSTree";
import ReportBoxTree from "../../Common/Tree/Instances/BoxTree/ReportBoxTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import '../../../Less/RC/commonViewDetail.less';
import StringUtil from "../../../Utilities/StringUtil";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import {
    IntervalOptions,
    WeekDayOptions,
    WeekOrderOptions,
} from "../../BCM/ContentRepositoryManagement/ScheduleSetting/ScheduleSettingPanel";
export default class Profile extends Component {
    constructor(props) {
        super(props);

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.columnAttrs = [
            "ProfileName",
            "Description",
            "Extension1",
            "Extension2"
        ];
        this.columnNames = [
            RMResx.RM_JS_RC_DueDisposal_ProfileName,
            RMResx.RM_JS_Profile_Description,
            RMResx.RM_RC_DueDisposalViewDetail_Time,
            "^Scheduled report generation",
            RMResx.RM_RC_DueDisposalViewDetail_ReportingScope,
        ];
        this.state = {
            DetailData: [],
            sourceTreeData: null,
            profileId: this.props.viewRowId
        };
        window.initTree = () => {
            this.refTermTree.setTreeData(window.treeData.items);
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
            url: "/api/DueDisposalApi/LoadProfileById",
            method:"POST",
            data:this.state.profileId,
        };
        fetchUtility(option).then((data) => {
                this.getDetailData(data);
        }).catch((e) => {
                
        });
    }

    getDetailData(profile) {
        let detailData = [];
        let columnNames = [...this.columnNames];
        if (!profile.scheduleInfo){
            columnNames = columnNames.filter(c => c !== "^Scheduled report generation");
        }
        for (let key in columnNames) {
            let columnName = columnNames[key];
            let columnValue = profile[this.columnAttrs[key]];
            let columnData = {};
            switch (columnName) {
                case RMResx.RM_RC_DueDisposalViewDetail_Time:
                    columnData.columnValue = this.getTime(columnValue);
                    break;
                case RMResx.RM_RC_DueDisposalViewDetail_ReportingScope:
                    columnData.columnValue = this.renderSourceTree(profile);
                    break;
                case "^Scheduled report generation":
                    columnData.columnValue = this.renderScheduledReport(profile);
                    break;
                default:
                    columnData.columnValue = columnValue;
            }
            if (!columnName.includes(':')) {
                columnName = `${columnName}:`;
            }
            columnData.columnName = columnName;
            detailData.push(columnData);
        }
        this.setState({
            DetailData: detailData,
        });
    }

    getTime(columnValue) {
        let timeObj = JSON.parse(columnValue);
        return RM.TimeUtil.dateToString(
            timeObj.StartTime,
            RM.TimeUtil.getTimezoneInfo(timeObj.TimeZoneId),
            true,
        );
    }

    getOrdinalSuffix(i) {
        if (i % 10 === 1 && i % 100 !== 11)
            return `${i}${RMResx.RM_JS_ScheduleSetting_Number_ST}`;
        if (i % 10 === 2 && i % 100 !== 12)
            return `${i}${RMResx.RM_JS_ScheduleSetting_Number_ND}`;
        if (i % 10 === 3 && i % 100 !== 13)
            return `${i}${RMResx.RM_JS_ScheduleSetting_Number_RD}`;
        return `${i}${RMResx.RM_JS_ScheduleSetting_Number_TH}`;
    }

    getIntervalUnit(data) {
        if (data.IntervalType == IntervalOptions.Weeks) {
            return RMResx.RM_JS_ScheduleSetting_Weeks;
        } else if (data.IntervalType == IntervalOptions.Days) {
            return RMResx.RM_JS_ScheduleSetting_Days;
        } else if (data.IntervalType == IntervalOptions.Hours) {
            return RMResx.RM_JS_ScheduleSetting_Hours;
        } else {
            const dayOfMonthMapping = {
                [WeekOrderOptions.First]:
                    RMResx.RM_JS_ScheduleSetting_WeekOrder_First,
                [WeekOrderOptions.Second]:
                    RMResx.RM_JS_ScheduleSetting_WeekOrder_Second,
                [WeekOrderOptions.Third]:
                    RMResx.RM_JS_ScheduleSetting_WeekOrder_Third,
                [WeekOrderOptions.Fourth]:
                    RMResx.RM_JS_ScheduleSetting_WeekOrder_Fourth,
            };

            const weekTypeMapping = {
                [WeekDayOptions.Monday]: RMResx.RM_JS_JN_WeeklyType_Monday,
                [WeekDayOptions.Tuesday]: RMResx.RM_JS_JN_WeeklyType_Tuesday,
                [WeekDayOptions.Wednesday]:
                    RMResx.RM_JS_JN_WeeklyType_Wednesday,
                [WeekDayOptions.Thursday]: RMResx.RM_JS_JN_WeeklyType_Thursday,
                [WeekDayOptions.Friday]: RMResx.RM_JS_JN_WeeklyType_Friday,
                [WeekDayOptions.Saturday]: RMResx.RM_JS_JN_WeeklyType_Saturday,
                [WeekDayOptions.Sunday]: RMResx.RM_JS_JN_WeeklyType_Sunday,
            };

            if (
                Object.values(WeekOrderOptions).includes(
                    data.DayOfMonth.toString(),
                )
            ) {
                return `${RMResx.RM_JS_ScheduleSetting_Months} ${RMResx.RM_JS_ScheduleSetting_On} ${dayOfMonthMapping[data.DayOfMonth]} ${weekTypeMapping[data.WeekType]}`;
            }

            const dayOfMonth =
                data.DayOfMonth === 31
                    ? RMResx.RM_JS_ScheduleSetting_LastDayOfMonth
                    : this.getOrdinalSuffix(data.DayOfMonth);
            return `${RMResx.RM_JS_ScheduleSetting_Months} ${RMResx.RM_JS_ScheduleSetting_On} ${dayOfMonth}`;
        }
    }

    getEndTimeDisplayStr(data) {
        let timeZone = RM.TimeUtil.getTimezoneInfo(
            data.TimeZoneId,
            data.IsDaylightSaving,
        );
        if (data.EndType == 1) {
            return RM.TimeUtil.dateToString(
                new Date(data.EndTime),
                timeZone,
                true,
            );
        } else if (data.EndType == 2) {
            return (
                RMResx.RM_JS_ScheduleSetting_EndAfter +
                " " +
                data.OccurrencesTotal +
                " " +
                RMResx.RM_JS_ScheduleSetting_Occurrences
            );
        } else {
            return RMResx.RM_JS_ScheduleSetting_NoEndDate;
        }
    }

    renderScheduledReport(profile) {
        const data = profile.scheduleInfo;
        const destination = profile.FullPath ? profile.FullPath : "^Opus download center";
        return (
            <$g.DetailList labelWidth={150}>
                    <>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={"^Start time"}
                                value={data.StartTime}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={"^Interval"}
                                value={`${data.Interval} ${this.getIntervalUnit(data)}`}
                            />
                        </$g.DetailRow>
                        <$g.DetailRow>
                            <$g.DetailCell
                                label={"^End time"}
                                value={this.getEndTimeDisplayStr(
                                    RM.TimeUtil.getTimezoneInfo(
                                        data.TimeZoneId,
                                        data.IsDaylightSaving,
                                    ),
                                )}
                            />
                        </$g.DetailRow>
                    </>
                <$g.DetailRow>
                    <$g.DetailCell
                        label={"^Export destination"}
                        value={destination}
                    />
                </$g.DetailRow>
            </$g.DetailList>
        );
    }

    renderSourceTree(profile) {
        let SourceTree = null;
        let profileType = profile.Type;
        let sourceTreeFlags = SourceFlags.SP;
        let sourceTreeData = $.parseJSON(profile.Extension2);
        if (profileType == JobType.ItemsFilesDueDisposal) {
            SourceTree = TreeWithTreStates;
        } else if (profileType == JobType.EXOItemsFilesDueDisposalReport) {
            SourceTree = EXOTree;
        } else if (profileType == JobType.PhysicalItemsFilesDueDisposalReport) {
            SourceTree = LocationTree;
        } else if (profileType == JobType.FSItemsFilesDueDisposal) {
            SourceTree = FSTree;
        } else if (profileType == JobType.OneDriveItemsFilesDueDisposal) {
            SourceTree = TreeWithTreStates;
            sourceTreeFlags = SourceFlags.OneDrive;
        } else if (profileType == JobType.SPOnPremiseItemsFilesDueDisposal) {
            SourceTree = SPTree;
            sourceTreeFlags = SourceFlags.SPLocal;
        } else if (profileType == JobType.BoxItemsFilesDueDisposal) {
            SourceTree = ReportBoxTree;
            sourceTreeFlags = SourceFlags.Box;
        } else if (profileType == JobType.GoogleDriveItemsFilesDueDisposal) {
            SourceTree = ReportGoogleTree;
            sourceTreeFlags = SourceFlags.Google;
        } else if (profileType == JobType.TeamsItemsFilesDueDisposalReport) {
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

    checkValueIsTree = (columnName) => {
        return columnName === RMResx.RM_RC_DueDisposalViewDetail_ReportingScope;
    }

    render() {
        return <div>
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
        </div>;
    }
}
