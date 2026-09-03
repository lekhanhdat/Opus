import { useState } from "react";
import StackedBarWithOptimization from "../../Components/StackedBarWithOptimization";
import { useEffect } from "react";
import { SimplePager } from "../../../../Common/Pager";
import ProgressRequester from "../../requests/ProgressRequester";

const defaultCategories = [
    {
        internalName: "remaining",
        displayName: RMResx.RM_FA_Progress_Statistical_Remaining,
    },
    {
        internalName: "archived",
        displayName: RMResx.RM_FA_Progress_SummaryTab_Archived,
    },
    {
        internalName: "deleted",
        displayName: RMResx.RM_FA_Progress_SummaryTab_Deleted,
    },
];

const ContainerStorageOptimization = ({ o365TenantId }) => {
    const [totalCount, setTotalCount] = useState(0);

    const [items, setItems] = useState([]);

    const [pageInfo, setPageInfo] = useState({
        pageIndex: 0,
        pageSize: 4,
    });

    useEffect(() => {
        if (_.isNil(o365TenantId)) {
            return;
        }

        setPageInfo({
            pageIndex: 0,
            pageSize: pageInfo.pageSize,
        });
    }, [o365TenantId]);

    useEffect(() => {
        const fetchData = async () => {
            if (_.isNil(o365TenantId)) {
                return;
            }
            const res = await ProgressRequester.getContainerOptimizedInfoesAsync({
                o365TenantId: o365TenantId,
                needCalculateCount: pageInfo.pageIndex === 0,
                pageIndex: pageInfo.pageIndex,
                pageSize: pageInfo.pageSize,
            });
            const convertedItems = res.items.map((item) => {
                return {
                    name: item.name,
                    data: [
                        {
                            category: "remaining",
                            value: Math.max(0, item.remaining),
                        },
                        {
                            category: "archived",
                            value: Math.max(0, item.archived),
                        },
                        {
                            category: "deleted",
                            value: Math.max(0, item.deleted),
                        },
                    ],
                };
            });
            setItems(convertedItems);
            if (pageInfo.pageIndex === 0) {
                setTotalCount(res.count);
            }
        };

        fetchData();
    }, [pageInfo]);

    const onPageChange = (pageIndex) => {
        setPageInfo({
            pageIndex: pageIndex,
            pageSize: pageInfo.pageSize,
        });
    };

    return (
        <div className="reco-container-storage-optimization">
            <div className="reco-sub-title" tabIndex="0">
                {RMResx.RM_FA_Progress_OptimizedStatistical}
            </div>
            <StackedBarWithOptimization
                items={items}
                categories={defaultCategories}
                height={260}
            />
            <div className="reco-discovery-pager">
                <div className="reco-discovery-table-paging-total" tabIndex="0">
                    {`${RMResx.RM_FA_Table_TotalCount} ${totalCount}`}
                </div>
                <SimplePager
                    pagerIndex={pageInfo.pageIndex}
                    pagerSize={pageInfo.pageSize}
                    shownCount={items.length}
                    hasNext={
                        (pageInfo.pageIndex + 1) * pageInfo.pageSize <
                        totalCount
                    }
                    onChange={onPageChange}
                />
            </div>
        </div>
    );
};

export default ContainerStorageOptimization;
