const Permissions = {
    None: '0x0',
    HomePageAccess: '0x1',
    PhysicalAdmin: '0xF0',
    PhysicalEndUser: '0xF00',
    FSAdmin: '0xF000',
    SPOAdmin: '0xF0000',
    EXOAdmin: '0xF00000',
    ControlPanel: '0xF000000',
    TermManagementAdmin: '0xF0000000',
    ContentRepositoyAdmin: '0xF00000000',
    RecordExplorerAdmin: '0xF000000000',
    RuleManagementAdmin: '0xF0000000000',
    ReportCenterAdmin: '0xF00000000000',
    JobMonitor: '0xF000000000000',
    ManualReviewAdmin: '0xF0000000000000',
    SuperAdmin: '0x0FFFFFFFFFFFFFFF'
}

export default Permissions;