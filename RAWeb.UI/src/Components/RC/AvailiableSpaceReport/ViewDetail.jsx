import { Component } from "react";
import LocationTree from "../../../Components/Common/Tree/Instances/Physical/SingleModeLocationTree";
import '../../../Less/RC/commonViewDetail.less';
import StringUtil from "../../../Utilities/StringUtil";
export default class Profile extends Component {
    constructor(props) {
        super(props);

        this.profileId = RM.Url.getParam(window.location.href, "id");
        this.columnAttrs = [
            "ProfileName",
            "Description",
            "Extension2"
        ];
        this.columnNames = [
            RMResx.RM_JS_RC_DueDisposal_ProfileName,
            RMResx.RM_JS_Profile_Description,
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
            url: "/api/AvailableSpaceReportApi/LoadProfileById",
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
        for (let key in this.columnNames) {
            let columnName = this.columnNames[key];
            let columnValue = profile[this.columnAttrs[key]];
            let columnData = {};
            switch (this.columnNames[key]) {
                case RMResx.RM_RC_DueDisposalViewDetail_ReportingScope:
                    columnData.columnValue = this.renderSourceTree(profile);
                    break;
                default:
                    columnData.columnValue = columnValue;
            }
            if (!columnName.includes(':')) { columnName = `${columnName}:`; }
            columnData.columnName = columnName;
            detailData.push(columnData);
        }
        this.setState({
            DetailData: detailData,
        });
    }

    renderSourceTree(profile) {
        let sourceTreeData = $.parseJSON(profile.Extension1);
        if (sourceTreeData) {
            return <div className="reco-report-view-tree">
                <LocationTree
                    ref={r => this.refSourceTree = r}
                    readonly={true}
                    selectedItemId={profile.Extension2}
                    data={sourceTreeData}
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
