import { Component } from "react";
import StringUtil from "../../../Utilities/StringUtil";
import { ActionTypeCol, AuditEventType, AuditObjType, ObjTypeCol, RangeNames, RangeTypes, TreeScopeType, UserScopeType } from "../Constants";
import SPTree from "../../Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import { JobType, SourceFlags } from "../../../Constants/Constants";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";
import { IntervalOptions, WeekDayOptions, WeekOrderOptions } from "../../BCM/ContentRepositoryManagement/ScheduleSetting/ScheduleSettingPanel";

export default class Profile extends Component {
    constructor(props) {
        super(props);

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.columnNames = [
            RMResx.RM_JS_RC_DueDisposal_ProfileName,
            RMResx.RM_RC_Profile_Description,
            RMResx.RM_RC_ActionAudit_TimeFrame,
            RMResx.RM_RC_ActionAudit_UserScope,
            RMResx.RM_RC_ActionAudit_ActionType,
            RMResx.RM_RC_ActionAudit_ObjType,
            "^Scheduled report generation",
            RMResx.RM_RC_ActionAudit_TreeScope,
            RMResx.RM_RC_ActionAudit_UrlScope,
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

    initProfileData() {
        let option = {
            url: "/api/ActionAuditReportApi/LoadProfileById",
            method: "POST",
            data: this.state.profileId
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
        for (let columnName of columnNames) {
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
        let extension1 = JSON.parse(profile.Extension1);
        switch (columnName) {
            case RMResx.RM_JS_RC_DueDisposal_ProfileName:
                columnValue = profile.ProfileName;
                break;
            case RMResx.RM_RC_Profile_Description:
                columnValue = profile.Description;
                break;
            case RMResx.RM_RC_ActionAudit_TimeFrame:
                columnValue = this.getRangeTimeValue(profile);
                break;
            case RMResx.RM_RC_ActionAudit_UserScope:
                columnValue = this.getUserScopeValue(extension1);
                break;
            case RMResx.RM_RC_ActionAudit_ActionType:
                columnValue = this.getActionTypeValue(extension1);
                break;
            case RMResx.RM_RC_ActionAudit_ObjType:
                columnValue = this.getObjTypeValue(extension1);
                break;
            case "^Scheduled report generation":
                columnValue = this.renderScheduledReport(profile);
                break;
            case RMResx.RM_RC_ActionAudit_TreeScope:
                columnValue = this.getTreeScopeValue(profile);
                break;
            case RMResx.RM_RC_ActionAudit_UrlScope:
                columnValue = extension1.FilterStr;
                break;
        }
        return columnValue;
    }

    getRangeTimeValue(profile) {
        let extension1 = JSON.parse(profile.Extension1);
        let columnValue = RangeNames[profile.RangeType];
        if (profile.RangeType == RangeTypes.Custom) {
            let rangTime = `${this.formatTime(extension1.StartDateTime)} - ${this.formatTime(extension1.EndDateTime)}`;
            columnValue = `${RangeNames[RangeTypes.Custom]} ${rangTime}`;
        }
        return columnValue;
    }

    formatTime(time) {
        let date = null;
        let timeStamp = RM.TimeUtil.dateToString(time);
        let timeArr = timeStamp.split(' ');
        if (timeArr[1].includes(',')) {
            date = timeArr[0] + ' ' + timeArr[1];
        } else {
            date = timeArr[0];
        }
        return date;
    }

    getUserScopeValue(profile) {
        let columnValue = "";
        if (profile.UserScope == UserScopeType.All) {
            columnValue = RMResx.RM_RC_ActionAudit_AllUsers;
        } else {
            let userData = this.getUserData(profile.userInfos);
            columnValue = userData.map((item) => {
                return <span key={item.id} className="ra-setting-profile" data-tooltip aria-label={item.tooltip} tabIndex="0">
                    <R.Profile
                        tooltip={item.tooltip}
                        name={item.name}
                        invalid="false">
                    </R.Profile>
                </span>;
            });
        }
        return columnValue;
    }

    getUserData(users) {
        let newUsers = [];
        if (users) {
            users.forEach(user => {
                newUsers.push({
                    tooltip: user.UserPrincipalName,
                    name: user.DisplayName,
                    id: user.UserId
                });
            });
        }
        return newUsers;
    }

    getActionTypeValue(profile) {
        let columnValue = [];
        if (profile.ActionType == AuditEventType.All) {
            columnValue = [RMResx.RM_RC_ActionAudit_ViewDetail_All];
        } else {
            for (const key in AuditEventType) {
                if (Object.hasOwnProperty.call(AuditEventType, key)) {
                    const element = AuditEventType[key];
                    if ((profile.ActionType & element) == element) {
                        let typeItem = ActionTypeCol.find(v => v.value == element);
                        if (typeItem) {
                            columnValue.push(typeItem.name);
                        }
                    }
                }
            }
        }
        return columnValue.join(", ");
    }

    getObjTypeValue(profile) {
        let columnValue = [];
        if (profile.ObjType == AuditObjType.All) {
            columnValue = [RMResx.RM_RC_ActionAudit_ViewDetail_All];
        } else {
            for (const key in AuditObjType) {
                if (Object.hasOwnProperty.call(AuditObjType, key)) {
                    const element = AuditObjType[key];
                    if ((profile.ObjType & element) == element) {
                        let typeItem = ObjTypeCol.find(v => v.value == element);
                        if (typeItem) {
                            columnValue.push(typeItem.name);
                        }
                    }
                }
            }
        }
        return columnValue.join(", ");
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

    getTreeScopeValue(profile) {
        let extension1 = JSON.parse(profile.Extension1);
        let SourceTree = null;
        let profileType = profile.Type;
        let sourceTreeFlags = SourceFlags.SP;
        let sourceTreeData = JSON.parse(profile.Extension2);
        if (extension1.TreeScope == TreeScopeType.All) {
            return RMResx.RM_RC_ActionAudit_AllTree;
        } else {
            if (profileType == JobType.SPOActionAuditReport) {
                SourceTree = SPTree;
            } else if (profileType == JobType.OneDriveActionAuditReport) {
                SourceTree = SPTree;
                sourceTreeFlags = SourceFlags.OneDrive;
            } else if (profileType == JobType.TeamsActionAuditReport) {
                SourceTree = ReportTeamsTree;
                sourceTreeFlags = SourceFlags.Teams;
            }
            if (SourceTree) {
                return <div className="reco-report-view-tree">
                    <SourceTree
                        ref={r => this.refSourceTree = r}
                        readonly={true}
                        data={sourceTreeData}
                        treeSource={sourceTreeFlags}
                    />
                </div>;
            }
        }
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