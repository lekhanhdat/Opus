import SiteMapLinks from "../../../Constants/SiteMapLinks";
import GroupManagement from "./GroupManagement";
import UserManagement from "./UserManagement";
import "../../../Less/CP/accountManagement.less";

export default class AccountManagement extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            groupItems: [],
            spContainerItems: [],
            exoContainerItems: [],
            oneDriveContainerItems: [],
            teamsContainerItems: [],
            physicalLocationItems: [],
            tabIndex: 0,
            tabTitles: [RMResx.RM_CP_AM_GroupManagement_Title, RMResx.RM_CP_AM_UserManagement_Title],
            groupManagementCopId: "raGroupManagementComponent"
        };
        this.bind(["onTabChanged"]);
    }

    componentInit() {
        this.initData();
    }

    initData() {
        $$.loading(true);
        let option = {
            url: "/api/CPApi/LoadGroupsAndContainers",
            method: "GET"
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                let data = JSON.parse(result);
                this.setState({
                    groupItems: data.GroupItems,
                    spContainerItems: data.SPContainerItems,
                    exoContainerItems: data.EXOContainerItems,
                    oneDriveContainerItems: data.OneDriveContainerItems,
                    teamsContainerItems: data.TeamsContainerItems,
                    physicalLocationItems: data.PhysicalLocationItems,
                }, () => {
                    this.dispatch(this.state.groupManagementCopId, "init", data.GroupItems);
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    initTabContent(tabIndex) {
        if (tabIndex == 0) {
            this.dispatch(this.state.groupManagementCopId, "init", []);
        }
    }

    onTabChanged(index) {
        let tabIndex = index;
        this.setState({
            tabIndex: tabIndex
        }, () => {
            this.initTabContent(tabIndex);
        });
    }

    renderSiteMap() {
        return <$g.SiteMap data={[SiteMapLinks.CP, SiteMapLinks.CP_AccountManagement]} />;
    }

    renderTabControl() {
        return <div className="ra-account-management-tab">
            <R.Tabcontrol
                type='underline'
                active={this.state.tabIndex}
                onChange={this.onTabChanged}
                destroy={false}
            >
                {
                    this.state.tabTitles.map((text, index) => {
                        return <R.TabPanel tab={text} key={index}></R.TabPanel>;
                    })
                }
            </R.Tabcontrol>
        </div>;
    }

    render() {
        return (
            <div id="raAccountManagement">
                {this.renderSiteMap()}
                <div className="ra-page-container">
                    {this.renderTabControl()}
                    {this.state.tabIndex == 0 && (
                        <GroupManagement
                            id={this.state.groupManagementCopId}
                            spContainerItems={this.state.spContainerItems}
                            exoContainerItems={this.state.exoContainerItems}
                            oneDriveContainerItems={this.state.oneDriveContainerItems}
                            teamsContainerItems={this.state.teamsContainerItems}
                            physicalLocationItems={this.state.physicalLocationItems}
                        />
                    )}
                    {this.state.tabIndex == 1 && (
                        <UserManagement
                            spContainerItems={this.state.spContainerItems}
                            exoContainerItems={this.state.exoContainerItems}
                            oneDriveContainerItems={
                                this.state.oneDriveContainerItems
                            }
                            teamsContainerItems={this.state.teamsContainerItems}
                            phyContainerItems={this.state.physicalLocationItems}
                        />
                    )}
                </div>
            </div>
        );
    }
}