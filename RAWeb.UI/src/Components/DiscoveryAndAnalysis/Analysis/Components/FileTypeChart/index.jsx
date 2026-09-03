import { useEffect, useRef, useState } from "react";
import "./index.less";
import Chart from "react-apexcharts";
import ApexCharts from "apexcharts";
import _ from "lodash";
import { InactiveDataRequester } from "../../requests";
import useStableCallback from "../../../../Common/Hooks/useStableCallback";
import { NumberUtil } from "../../Utils";

// data : [
//     {
//         x,
//         y,
//         meta,
//     }
// ]

const FileTypeChart = ({ id, height, queryParameter, queryData, onChange }) => {
    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                id: id,
                events: {
                    dataPointSelection: onColumnSelect,
                },
                toolbar: {
                    show: false,
                },
            },
            plotOptions: {
                treemap: {
                    distributed: true,
                },
            },
            tooltip: {
                enabled: true,
                y: {
                    formatter: (val) => NumberUtil.internaltionalCounting(val)
                }
            },
        },
        series: [
            {
                data: [],
            },
        ],
    });

    useEffect(() => {
        const handler = async () => {
            const filteredfileExtensions =
                queryParameter.fileExtensionQueryParameter.fileExtensions;

            if (!_.isNil(filteredfileExtensions) && filteredfileExtensions.length === 1) {
                return;
            }

            let fileTypes = await queryData(queryParameter);
            fileTypes = _.sortBy(fileTypes, item => 0 - item.fileTotalSize);
            fileTypes = _.take(fileTypes, 20);
            const data = fileTypes.map(item => ({
                x: item.name,
                y: item.fileTotalSize,
                meta: item.id
            }));
            const clonedInfo = _.cloneDeep(chartInfo);
            clonedInfo.options.chart.events.dataPointSelection = onColumnSelect;
            clonedInfo.series = [{ data: data }];
            setChartInfo(clonedInfo);
        };
        handler();
    }, [queryParameter]);

    useEffect(() => {
        ApexCharts.exec(
            id,
            "updateOptions",
            chartInfo.options,
            false,
            false,
            true
        );
        ApexCharts.exec(id, "updateSeries", chartInfo.series, false);
    }, [chartInfo]);

    const onColumnSelect = useStableCallback((e, chart, options) => {
        const clonedParameter = _.cloneDeep(queryParameter);
        const selectedIndex = options.selectedDataPoints[0][0];
        if(_.isNil(selectedIndex)) {
            clonedParameter.fileExtensionQueryParameter = {};
        }
        else {
            const item = chartInfo.series[0].data[selectedIndex];
            clonedParameter.fileExtensionQueryParameter = {
                fileExtensions: [item.meta],
            };
        }
        onChange(clonedParameter);
    });

    const renderNoData = () => {
        return (
            <div className="reco-tree-map-chart-empty">
                <span className="reco-tree-map-chart-empty-icon fia-book-b">
                    <span className="path1"></span>
                    <span className="path2"></span>
                </span>
                <span className="reco-tree-map-chart-empty-text" tabIndex="0">
                    {RMResx.RM_FA_FileTypeTree_NoItem}
                </span>
            </div>
        );
    }

    return (
        <div className="reco-tree-map-chart">
            {chartInfo.series[0].data.length > 0 ? (
                <Chart
                    options={chartInfo.options}
                    series={chartInfo.series}
                    type="treemap"
                    width={"100%"}
                    height={height}
                />
            ) : renderNoData()}
        </div>
    );
};

export default FileTypeChart;
