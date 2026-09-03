import React, { useState } from "react";
import SiteMapLinks from '../../../Constants/SiteMapLinks';
import PickListCommon from "../PickListCommon/Index";
import Template, { ReturnHistoryTemplate } from "./Template";
import { TableColumns, StatusList, ReturnHistoryTableColumns } from "./Config";
import Actions from "./Actions";
import ReturnHistory from "./ReturnHistory";

const PickListForLoanRequests = () => {
    const [activeTab, setActiveTab] = useState(0);

    return (
        <div>
            <$g.SiteMap data={[SiteMapLinks.MT_PickListForLoanRequests]} />
            <div id="raMtPickListForLoanRequests">
                <R.Tabcontrol
                    flex
                    onChange={(index) => setActiveTab(index)}
                    active={activeTab}
                    destroy={true}
                >
                    <R.TabPanel
                        tab={RMResx.RM_MT_PickList_LoanRequests_LoanTab}
                        aria-label={RMResx.RM_MT_PickList_LoanRequests_LoanTab}
                    >
                        <PickListCommon
                            recordListApiUrl="/api/PickListApi/QueryLoanRequest"
                            tableColumns={TableColumns}
                            tableTemplate={Template}
                            statusList={StatusList}
                            exportUrl={"/api/PickListApi/StartExportLoanJob"}
                            Actions={Actions}
                        />
                    </R.TabPanel>
                    <R.TabPanel
                        tab={RMResx.RM_MT_PickList_LoanRequests_ReturnHistoryTab}
                        aria-label={RMResx.RM_MT_PickList_LoanRequests_ReturnHistoryTab}
                    >
                        <ReturnHistory
                            recordListApiUrl="/api/PickListApi/GetReturnLoanHistoryData"
                            tableColumns={ReturnHistoryTableColumns}
                            tableTemplate={ReturnHistoryTemplate}
                            exportUrl="/api/PickListApi/StartExportReturnHistoryJob"
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            </div>
        </div>
    );
};

export default PickListForLoanRequests; 