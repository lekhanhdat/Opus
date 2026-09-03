import ContentRepositoryManagementForSPO from "./CRMForSPO/ContentRepositoryManagementForSPO";
import ArchiveCRMForSPO from "./CRMForSPO/ArchiveCRMForSPO";
import { checkPermission } from "../../../Utilities/permissionManager";

export const TabIndex = {
    Records: 0,
    Archive: 1,
};

export default class CRMForSPO extends R.Component {
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
            {this.state.tabIndex == TabIndex.Records && <ContentRepositoryManagementForSPO
                tabControl={this.isRecordsPermission && this.isArchiverPermission ? this.onTabControl() : false}
            ></ContentRepositoryManagementForSPO>}
            {this.state.tabIndex == TabIndex.Archive && <ArchiveCRMForSPO
                tabControl={this.isRecordsPermission && this.isArchiverPermission ? this.onTabControl() : false}
            ></ArchiveCRMForSPO>}
        </div>;
    }
}
