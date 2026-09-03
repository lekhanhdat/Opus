import { Component } from "react";
import { JobType, SourceFlags } from "../../../Constants/Constants";
import { ActionTypes, ActionTypeNames, RangeTypes, RangeNames } from "../Constants";
import SPTree from "../../Common/Tree/Instances/SPTree/ReportSPTree";
import TreeWithTreStates from "../../Common/Tree/Instances/SPTree/ReportTreeWithTreStates";
import EXOTree from "../../Common/Tree/Instances/EXO/ReportEXOTree";
import FSTree from "../../Common/Tree/Instances/FSTree/ReportFSTree";
import ReportBoxTree from "../../Common/Tree/Instances/BoxTree/ReportBoxTree";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/ReportLocationTree";
import '../../../Less/RC/commonViewDetail.less';
import StringUtil from "../../../Utilities/StringUtil";
import ReportGoogleTree from "../../Common/Tree/Instances/GoogleTree/ReportGoogleTree";
import ReportTeamsTree from "../../Common/Tree/Instances/TeamsTree/ReportTeamsTree";

export default class Profile extends Component {
    constructor(props) {
        super(props);
        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.columnNames = [
            RMResx.RM_JS_RC_DueDisposal_ProfileName,
            RMResx.RM_JS_Profile_Description,
            RMResx.RM_JS_RC_TimeFrame_OprationType,
            RMResx.RM_JS_RC_TimeFrame_Range,
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
            profileId: nextProps.viewRowId
        };
    }

    initProfileData() {
        let option = {
            url: "/api/TimeFrameProfileApi/LoadProfileById",
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
            case RMResx.RM_JS_RC_TimeFrame_OprationType:
                columnValue = this.getOprationTypeValue(profile);
                break;
            case RMResx.RM_JS_RC_TimeFrame_Range:
                columnValue = this.getRangeTimeValue(profile);
                break;
            case RMResx.RM_RC_DueDisposalViewDetail_ReportingScope:
                columnValue = this.renderSourceTree(profile);
                break;
        }
        return columnValue;
    }

    getOprationTypeValue(profile) {
        let columnValue;
        let isCreated = profile.IsCreated ? ActionTypeNames[ActionTypes.Create] : "";
        let isDestoryed = profile.IsDestoryed ? ActionTypeNames[ActionTypes.Destroyed] : "";
        if (profile.IsCreated && !profile.IsDestoryed) {
            columnValue = isCreated;
        } else if (!profile.IsCreated && profile.IsDestoryed) {
            columnValue = isDestoryed;
        } else {
            columnValue = `${isCreated}, ${isDestoryed}`;
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

    getRangeTimeValue(profile) {
        let columnValue = RangeNames[profile.RangeType];
        if (profile.RangeType == RangeTypes.Custom) {
            let rangTime = `${this.formatTime(profile.StartTime)} - ${this.formatTime(profile.EndTime)}`;
            columnValue = `${RangeNames[RangeTypes.Custom]} ${rangTime}`;
        }
        return columnValue;
    }

    renderSourceTree(profile) {
        let SourceTree = null;
        let profileType = profile.Type;
        let sourceTreeFlags = SourceFlags.SP;
        let sourceTreeData = $.parseJSON(profile.Extension2);
        if (profileType == JobType.CreateAndDestroyedFileReport) {
            SourceTree = TreeWithTreStates;
        } else if (profileType == JobType.EXOCreateAndDestroyedFileReport) {
            SourceTree = EXOTree;
        } else if (profileType == JobType.PhysicalCreateAndDestroyedFileReport) {
            SourceTree = LocationTree;
        } else if (profileType == JobType.FSCreateAndDestroyedFileReport) {
            SourceTree = FSTree;
        } else if (profileType == JobType.OneDriveCreateAndDestroyedFileReport) {
            SourceTree = TreeWithTreStates;
            sourceTreeFlags = SourceFlags.OneDrive;
        } else if (profileType == JobType.SPOnPremiseCreateAndDestroyedFileReport) {
            SourceTree = SPTree;
            sourceTreeFlags = SourceFlags.SPLocal;
        } else if (profileType == JobType.BoxCreateAndDestroyedFileReport) {
            SourceTree = ReportBoxTree;
            sourceTreeFlags = SourceFlags.Box;
        } else if (profileType == JobType.GoogleDriveCreateAndDestroyedFileReport) {
            SourceTree = ReportGoogleTree;
            sourceTreeFlags = SourceFlags.Google;
        } else if (profileType == JobType.TeamsCreateAndDestroyedFileReport) {
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
