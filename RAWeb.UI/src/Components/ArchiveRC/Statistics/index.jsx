import React from 'react';

function ViewStatistics(props) {
    const { errorMsg, statisticInfo } = props;

    const statisticData = React.useMemo(() => {
        const value = [
            {
                id: 1,
                levelType: "2",
                label: RMResx.RM_AR_RC_StatisticPanel_SiteCollection,
                value: "",
            },
            {
                id: 2,
                levelType: "4",
                label: RMResx.RM_AR_RC_StatisticPanel_Site,
                value: "",
            },
            {
                id: 3,
                levelType: "8",
                label: RMResx.RM_AR_RC_StatisticPanel_List,
                value: "",
            },
            {
                id: 4,
                levelType: "16",
                label: RMResx.RM_AR_RC_StatisticPanel_Folder,
                value: "",
            },
            {
                id: 5,
                levelType: "32",
                label: RMResx.RM_AR_RC_StatisticPanel_Item,
                value: "",
            },
            {
                id: 6,
                levelType: "512",
                label: RMResx.RM_AR_RC_StatisticPanel_ItemVersion,
                value: "",
            },
            {
                id: 7,
                levelType: "64",
                label: RMResx.RM_AR_RC_StatisticPanel_Document,
                value: "",
            },
            {
                id: 8,
                levelType: "256",
                label: RMResx.RM_AR_RC_StatisticPanel_DocumentVersion,
                value: "",
            },
            {
                id: 9,
                levelType: "128",
                label: RMResx.RM_AR_RC_StatisticPanel_Attachment,
                value: "",
            },
            {
                id: 10,
                levelType: "totalSize",
                label: RMResx.RM_AR_RC_StatisticPanel_TotalSize,
                value: "",
            },
        ];

        if (statisticInfo) {
            value.map((item) => {
                item.value = statisticInfo[item.levelType];
                return item;
            });
        }

        return value;
    }, [statisticInfo])


    if (errorMsg) {
        return (
            <div className='text-center'>
                <div className="margin-bottom-l">
                    <img src={`${RM.gData.resCdnURL}/cloud%20records/failed.svg`} alt={RMResx.RM_JS_Common_RecourdAutomation} />
                </div>
                <span>{RMResx[errorMsg]}</span>
            </div>
        );
    }

    if (statisticInfo) {
        return (
            <div>
                <span>{RMResx.RM_AR_RC_StatisticPanel_Desc}</span>
                <$g.DetailList>
                    {statisticData.map((item) => (
                        <div key={item.id}>
                            <$g.DetailRow>
                                <$g.DetailCell label={item.label}>
                                    <span tabIndex={0}>{item.levelType === "totalSize" && item.value !== "" ? `${item.value} ${RMResx.RM_AR_RC_StatisticPanel_Unit_GB}` : item.value}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                        </div>
                    ))}
                </$g.DetailList>
            </div>
        );
    }

    return (
        <div className='text-center'>
            <div className="margin-bottom-l">
                <img src={`${RM.gData.resCdnURL}/cloud%20records/calculating.svg`} width={98} height={98} alt={RMResx.RM_JS_Common_RecourdAutomation} />
            </div>
            <span>{RMResx.RM_AR_RC_StatisticPanel_WaitingJob}</span>
        </div>
    );
}

export default ViewStatistics