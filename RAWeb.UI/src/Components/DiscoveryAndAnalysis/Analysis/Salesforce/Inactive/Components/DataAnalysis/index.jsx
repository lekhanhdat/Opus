import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";

import { SalesforceDiscoveryDataView, SalesforceDataTypeChart } from "../index";
import { DiscoveryQueryDataType, SFDiscoveryNodeViewMode } from "../../../../Constants";
import { SalesforceBasicDataRequester, SalesforceInactiveDataRequester } from "../../../../requests/Salesforce";

const buildInColumns = new Map([
    [
        SFDiscoveryNodeViewMode.Data,
        [
            {
                displayName:  RMResx.RM_FA_SF_TableColumn_Object,
                internalName: "displayName",
                isLink: false,
                width: 350,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_InactiveRecordsCount,
                internalName: "inactiveSumCount",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_TotalRecordsCount,
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
                displayName: RMResx.RM_FA_SF_TableColumn_InactiveDataSize,
                internalName: "inactiveTotalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName: RMResx.RM_FA_SF_TableColumn_TotalDataSize,
                internalName: "totalSize",
                isAggregateField: true,
                width: 200,
            },
            {
                displayName:RMResx.RM_FA_SF_TableColumn_InactiveOfTotalSize,
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
    ]
]);

const defaultQueryParameter = {
    dataType: DiscoveryQueryDataType.Inactive,
    withoutDateQueryParameter: {
        from: -1,
        to: 999,
    },
    sizeRangeQueryParameter: {},
    nodeQueryParameter: {
        viewMode: SFDiscoveryNodeViewMode.Data,
        objectIds: [],
        pageSize: 5,
    },
    fileExtensionQueryParameter: {},
};

const DataAnalysis = (props,ref)=> {
    const { queryNodeDataInfo, queryNodeTotalAggregateInfo } = props;
    const tableRef = useRef(null)
    
    const [queryParameter, setQueryParameter] = useState(defaultQueryParameter);
    const [objects, setObjects] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const res = await SalesforceBasicDataRequester.getObjects(defaultQueryParameter);
            setObjects(res);
        };
        fetchData();
    }, []);

    const getTableColumns = async () => {
        return buildInColumns;
    };

    const onObjectChange = (args) => {
        let selectedObjectIds = args.newValue.map((item) => { return item.ObjectId; });
        setQueryParameter({
            ...queryParameter,
            selectedObjectIds,
            nodeQueryParameter:{
                ...queryParameter.nodeQueryParameter,
                pageIndex:0
            }
        });
        tableRef?.current?.refreshTableIndex();
    }


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
                    {RMResx.RM_FA_SF_Inactive_DataAnalysisTitle}
                </span>
            </section>
            <div className="reco-discovery-split-line"></div>
            <div>
                <R.Multicombobox
                    items={objects}
                    disabled={false}
                    textField="DisplayName"
                    valueField="ObjectId"
                    checkedField="Checked"
                    tooltipField="tooltip"
                    groupField="Group"
                    groupToggleable={true}
                    onChange={onObjectChange}
                    className='reco-sf-object-combobox'
                    searchPlaceholder={RMResx.RM_FA_Discovery_SF_SearchPlaceholder}
                    hasSelectAll={false}
                    clearable={true}
                    lazyStep={false}
                />

            </div>
            <section>
                <div className="reco-discovery-data-table">
                    <div className="reco-discovery-scroll-table">
                        <SalesforceDiscoveryDataView
                            id={`reco-discovery-salesforce-data-${new Date().getMinutes()}-${new Date().getSeconds()}`}
                            getColumns={getTableColumns}
                            queryParameter={queryParameter}
                            onChange={setQueryParameter}
                            queryNodeDataInfo={queryNodeDataInfo}
                            queryNodeTotalAggregateInfo={
                                queryNodeTotalAggregateInfo
                            }
                            ref={tableRef}
                        />
                    </div>
                </div>
            </section>
            <section>
                <div>
                    <div className="reco-chart-title">
                        <strong>
                            {
                                RMResx.RM_FA_SF_Inactive_SummaryTab_GrowthByCreatedTime
                            }
                        </strong>
                    </div>
                    <div
                        style={{ marginTop: -16 }}
                        className="reco-column-chart"
                    >
                        <SalesforceDataTypeChart
                            id="sf_inactive_summary_data_size_range"
                            height={320}
                            queryParameter={queryParameter}
                            queryData={
                                SalesforceInactiveDataRequester.queryFigureDataInfo
                            }
                        />
                    </div>
                </div>
            </section>
        </div>
    );
}

export default forwardRef(DataAnalysis);
