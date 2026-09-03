import { Component } from "react";
import { JobType, SourceFlags } from "../../../Constants/Constants";
import { RangeTypes, RangeNames } from "../Constants";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import SPTree from "../../Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates  from "../../Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../Common/Tree/Instances/EXO/ReportEXOTree";
import FSTree from "../../Common/Tree/Instances/FSTree/ReportFSTree";
import '../../../Less/RC/commonViewDetail.less';
import StringUtil from "../../../Utilities/StringUtil";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import { IntervalOptions, WeekDayOptions, WeekOrderOptions } from "../../BCM/ContentRepositoryManagement/ScheduleSetting/ScheduleSettingPanel";
export default class RestoreReportViewDetail extends Component {
    constructor(props) {
        super(props);

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.columnNames = [
            RMResx.RM_JS_RC_DueDisposal_ProfileName,
            RMResx.RM_JS_Profile_Description,
            RMResx.RM_JS_RC_TimeFrame_Range,
            "^Scheduled report generation",
            RMResx.RM_RC_DueDisposalViewDetail_ReportingScope,
        ];
        this.state = {
            DetailData: [],
            sourceTreeData: null,
            profileId: this.props.viewRowId,
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
            profileId: nextProps.viewRowId,
        };
    }

    initProfileData() {
        let option = {
            url: "/api/RestoreReportApi/LoadProfileById",
            method: "POST",
            data: this.state.profileId,
        };
        fetchUtility(option)
            .then((data) => {
                this.getDetailData(data);
            })
            .catch((e) => {});
    }

    getDetailData(profile) {
        let detailData = [];
        let columnNames = [...this.columnNames];
        if (!profile.scheduleInfo){
            columnNames = columnNames.filter(c => c !== "^Scheduled report generation");
        }
        for (let columnName of columnNames) {
            let columnObj = {};
            columnObj.columnName = columnName;
            columnObj.columnValue = this.getColumnValue(columnName, profile);
            if (!columnName.includes(":")) {
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
            case RMResx.RM_JS_RC_TimeFrame_Range:
                columnValue = this.getRangeTimeValue(profile);
                break;
            case "^Scheduled report generation":
                columnValue = this.renderScheduledReport(profile);
                break;
            case RMResx.RM_RC_DueDisposalViewDetail_ReportingScope:
                columnValue = this.renderSourceTree(profile);
                break;
        }
        return columnValue;
    }

    getRangeTimeValue(profile) {
        let columnValue = RangeNames[profile.RangeType];
        if (profile.RangeType == RangeTypes.Custom) {
            let rangTime = `${this.formatTime(profile.StartTime)} - ${this.formatTime(profile.EndTime)}`;
            columnValue = `${RangeNames[RangeTypes.Custom]} ${rangTime}`;
        }
        return columnValue;
    }

    formatTime(time) {
        let date = null;
        let timeStamp = RM.TimeUtil.dateToString(time);
        let timeArr = timeStamp.split(" ");
        if (timeArr[1].includes(",")) {
            date = timeArr[0] + " " + timeArr[1];
        } else {
            date = timeArr[0];
        }
        return date;
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
        if (profileType == JobType.RestoreReport) {
            SourceTree = TreeWithTreStates;
        } else if (profileType == JobType.OneDriverRestoreReport) {
            SourceTree = TreeWithTreStates;
            sourceTreeFlags = SourceFlags.OneDrive;
        } else if (profileType == JobType.TeamsRestoreReport) {
            SourceTree = ReportTeamsTree;
            sourceTreeFlags = SourceFlags.Teams;
        } else if (profileType == JobType.GoogleRestoreReport) {
            SourceTree = ReportGoogleTree;
            sourceTreeFlags = SourceFlags.Google;
        }

        if (SourceTree) {
            return (
                <div className="reco-report-view-tree">
                    <SourceTree
                        ref={(r) => (this.refSourceTree = r)}
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
    };

    render() {
        return (
            <div>
                {this.state.DetailData.map((item, index) => {
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
                })}
            </div>
        );
    }
}
