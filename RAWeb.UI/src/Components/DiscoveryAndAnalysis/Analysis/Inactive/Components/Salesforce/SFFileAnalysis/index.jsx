import { SFInactiveDataRequester } from '../../../../requests';
import SFDiscoveryDataView from '../../../../Components/DiscoveryDataView/Salesforce';
import SFFileTypeChart from '../../../../Components/FileTypeChart/Salesforce';
import { DiscoveryQueryDataType, SFDiscoveryNodeViewMode } from '../../../../Constants';
import { forwardRef, useImperativeHandle, useState } from 'react';
import SFFileExtensionChart from '../../Salesforce/SFFileExtensionChart';

const buildInColumns = new Map([
    [
        SFDiscoveryNodeViewMode.File,
        [
            {
                displayName: RMResx.RM_FA_SF_TableColumn_Object,
                internalName: "displayName",
                isLink: false,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_InactiveFileCount,
                internalName: "inactiveSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_TotalFileCount,
                internalName: "totalItemCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:  RMResx.RM_FA_SF_TableColumn_InactiveOfTotalCount,
                internalName: "inactiveCountOfTotal",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_InactiveFileSize,
                internalName: "inactiveTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_TotalFileSize,
                internalName: "totalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_InactiveOfTotalSize,
                internalName: "inactiveSizeOfTotal",
                isAggregateField: true,
                width: 200,
            },

            /** 
             ** Hidden in this release, It'll be implemented later 
             ** Hide the "Saving" column
            */
            // {
            //     displayName: RMResx.RM_FA_SF_TableColumn_Saving,
            //     internalName: "savings",
            //     // isAggregateField: true,
            //     width: 200,
            // },
        ],
    ],
]);
const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Inactive,
    withoutDateQueryParameter: {
        from: -1,
        to: 999,
    },
    sizeRangeQueryParameter: {},
    nodeQueryParameter: {
        viewMode: SFDiscoveryNodeViewMode.File,
        objectIds: [],
        pageSize: 5,
    },
    fileExtensionQueryParameter: {},
};

const SFFileAnalysis = (props,ref)=> {
    const { queryNodeDataInfo, queryNodeTotalAggregateInfo } = props;

    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);
    
    const getTableColumns = async () => {
        return buildInColumns;
    };

    useImperativeHandle(ref, () => ({
        onChangeWithoutDate: (withoutDate) => {
            setQueryParameter({
                ...queryParameter,
                withoutDateQueryParameter: withoutDate,
            });
        },
    }));


    return (
        <div className="reco-data">
            <section className="reco-title">
                <span tabIndex="0">
                    {RMResx.RM_FA_SF_Inactive_FileAnalysisTitle}
                </span>
            </section>
            <section className="reco-node-data">
                <div className="reco-discovery-data-table">
                    <div className="reco-discovery-scroll-table">
                        <SFDiscoveryDataView
                            getColumns={getTableColumns}
                            queryParameter={queryParameter}
                            onChange={setQueryParameter}
                            queryNodeDataInfo={queryNodeDataInfo}
                            queryNodeTotalAggregateInfo={
                                queryNodeTotalAggregateInfo
                            }
                            showPagination={false}
                        />
                    </div>
                </div>
            </section>
            <section className="reco-chart-data">
                <div>
                    <div className="reco-chart-title">
                        {RMResx.RM_FA_SF_Inactive_SummaryTab_FileSizeTitle}
                    </div>
                    <div className="reco-column-chart">
                        <SFFileTypeChart
                            id="sf_inactive_summary_file_size_range"
                            height={300}
                            queryParameter={queryParameter}
                            onChange={setQueryParameter}
                            queryData={
                                SFInactiveDataRequester.querySizeRanges
                            }
                        />
                    </div>
                </div>
                <div>
                    <div className="reco-chart-title">
                        {RMResx.RM_FA_SF_Inactive_SummaryTab_FileTypeTitle}
                    </div>
                    <div className="reco-treemap-chart">
                        <SFFileExtensionChart
                            id={"inactive_summary_file_type"}
                            height={300}
                            queryParameter={queryParameter}
                            onChange={setQueryParameter}
                            queryData={
                                SFInactiveDataRequester.queryFileExtensions
                            }
                        />
                    </div>
                </div>
            </section>
        </div>
    );
}

export default forwardRef(SFFileAnalysis)