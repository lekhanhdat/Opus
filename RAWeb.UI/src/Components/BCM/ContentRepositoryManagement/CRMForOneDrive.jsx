import ArchiveCRMForOneDrive from "./CRMForOneDrive/ArchiveCRMForOneDrive";
import ContentRepositoryManagementForOD from "./CRMForOneDrive/ContentRepositoryManagementForOD";
import { TabIndex } from "./CRMForSPO";
import { checkPermission } from "../../../Utilities/permissionManager";

export default class CRMForOneDrive extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.isRecordsPermission = checkPermission("RECO_ContentSource_Tab", RM.UserResources);
        this.isArchiverPermission = checkPermission("Archiver_ContentSource_Tab", RM.UserResources);
        this.state = {
            tabIndex: this.isRecordsPermission ? TabIndex.Records : TabIndex.Archive,
            tabs: [
                { head: RMResx.RM_AR_SPS_TabControl_Information },
                { head: RMResx.RM_AR_SPS_TabControl_Storage }
            ],
        };
    }

    onTabControl() {
        return <R.Tabcontrol flex active={this.state.tabIndex} onChange={this.handleSelectedTabChanged.bind(this)}>
            {this.state.tabs.map((tab, index) => {
                return (
                    <R.TabPanel key={index} tab={tab.head}></R.TabPanel>
                );
            })}
        </R.Tabcontrol>;
    }

    handleSelectedTabChanged(newIndex) {
        this.setState({ tabIndex: newIndex });
    }

    render() {
        return <div style={{ height: "100%" }} id={this.props.id}>
            {this.state.tabIndex == TabIndex.Records && <ContentRepositoryManagementForOD
                tabControl={this.isRecordsPermission && this.isArchiverPermission ? this.onTabControl() : false}
            ></ContentRepositoryManagementForOD>}
            {this.state.tabIndex == TabIndex.Archive && <ArchiveCRMForOneDrive
                tabControl={this.isRecordsPermission && this.isArchiverPermission ? this.onTabControl() : false}
            ></ArchiveCRMForOneDrive>}
        </div>;
    }
}