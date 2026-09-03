import SiteMapLinks from '../../Constants/SiteMapLinks';
import JobMonitor from './JobMonitor';
import JobQueue from './JobQueue';
import '../../Less/JM/jm.less';
import { checkPermission } from '../../Utilities/permissionManager';
import { JobType, StatusCode } from './JMConstants';
import RouterUrls from '../../Constants/RouterUrls';
import { LicenseHelper } from '../../Utilities/CommonUtil';

export default class JM extends R.Component {
    constructor(props) {
        super(props);
        this.state = {
            index: 0,
        };
    }

    handleSelectedIndexChanged = (index)=> {
        this.setState({ index: index});
    }

    render() {
        const tabs = [];

        tabs.push({
            id: 'jobMonitor',
            tabTitle: RMResx.RM_JS_JM_Tab_JobMonitor,
            content: (
                <JobMonitor
                    buttonNames={["ExportSettings", "Priority", "Refresh", "ShowReport", "DownloadReport", "Delete", "Stop"]}
                    deleteButtonName={RMResx.RM_JS_Common_Delete}
                    filterCacheNamePrefix=""    //Can't change this prefix, because custom browser exist old data
                    manageColumnCacheName="JMManageColumnCheckedIds"
                    deleteJobUrl={"/api/JMApi/BatchDelete"}
                    queryPagerUrl={"/api/JMApi/QueryPager"}
                    enableJobIdColLink={true}
                    deleteJobConfirmContent={RMResx.RM_JS_JM_ConfirmDeleteJob}
                    deleteJobSuccessMsg={RMResx.RM_JS_JM_DeleteJobSuccess}
                    showSearchbox={true}
                    supportSort={true}
                />
            )
        });

        if (checkPermission("JM_JobQueue", RM.UserResources)) {
            tabs.push({
                id: 'jobQueue',
                tabTitle: RMResx.RM_JS_JM_Tab_JobQueue,
                content: <JobQueue/>
            });
        }

        if (LicenseHelper.HasOpusSOLicense() && RM.gData.enableRecordsArchiver && RM.gData.enableDeleteOrphanData && checkPermission(RouterUrls.CP_Index, RM.UserResources)) {
            tabs.push({
                id: 'failedJobCleanup',
                tabTitle: RMResx.RM_JS_JM_Tab_FailedJobCleanup,
                content: (
                    <JobMonitor
                        buttonNames={["StaticDownloadReport", "Delete"]}
                        deleteButtonName={RMResx.RM_JS_JM_Btn_DeleteOrphanData}
                        filterCacheNamePrefix="FailedJob"
                        manageColumnCacheName="JMFailedJobManageColumnCheckedIds"
                        deleteJobUrl={"/api/RetentionApi/RunDeleteOrphanDatasJob"}
                        queryPagerUrl={"/api/CleanupApi/QueryPager"}
                        enableJobIdColLink={false}
                        deleteJobConfirmContent={RMResx.RM_JS_JM_DeleteOrphanDataMsg}
                        deleteJobSuccessMsg={<$g.I18NProvider msg={RMResx.RM_JS_SPS_RunJobSucceed}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>}
                        filterSupportStatus={[StatusCode.Failed, StatusCode.FinishWithException]}
                        filterSupportJobType={[JobType.RecordsDisposal, JobType.OneDriveRecordsDisposal, JobType.SpecifyTeamsArchiverBackup, JobType.RMArchiverBackup, JobType.TeamsArchiverBackup, JobType.MailBoxBackup, JobType.DiscoverOptimization, JobType.TeamsRecordsDisposal, JobType.ArchiverByHSMXml]}
                        showSearchbox={false}
                        supportSort={false}
                    />
                )
            });
        }

        return <div id="rmJobMonitor">
            <$g.SiteMap data={[SiteMapLinks.JM]} />
            <div className="ra-page-container">
                <div className="jm-tab-header-wrapper">
                    <R.Tabcontrol
                        maxWidth={"none"}
                        active={this.state.index}
                        onChange={this.handleSelectedIndexChanged}
                        destroy={true}
                    >
                        {tabs.map(item => (
                            <R.TabPanel key={item.id} tab={item.tabTitle} />
                        ))}
                    </R.Tabcontrol>
                </div>

                <div className="jm-tab-content-wrapper">
                    {tabs[this.state.index]?.content}
                </div>
            </div>
        </div>;
    }
}