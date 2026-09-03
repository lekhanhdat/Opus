import { useEffect, useState } from "react";
import _ from "lodash";
import Chart from "react-apexcharts";
import ApexCharts from "apexcharts";

import useStableCallback from "../../../../../../Common/Hooks/useStableCallback";
import { DataSizeType } from "../../../../Constants";
import { UnitConvertsionUtil } from "../../../../Utils";

import "./index.less";

const FileExtensionChart = ({ id, height, queryParameter, queryData, onChange }) => {
    
    const [unit,setUnit] = useState(DataSizeType.MB);
    
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
                y: {
                  formatter: function(value, { series, seriesIndex, dataPointIndex, w }) {
                    return `${value} ${UnitConvertsionUtil.GetUnitI18N(unit)}`
                  }
                }
              }
        },
        series: [
            {
                data: [],
            },
        ],
    });


    useEffect(() => {
        const fetchData = async () => {
            const filteredfileExtensions =
                queryParameter.fileExtensionQueryParameter.fileExtensions;

            if (!_.isNil(filteredfileExtensions) && filteredfileExtensions.length === 1) {
                return;
            }

            let { fileTypes, unit } = await queryData(queryParameter);
            fileTypes = _.sortBy(fileTypes, item => 0 - item.fileTotalSize);
            fileTypes = _.take(fileTypes, 20);
            const data = fileTypes.map(item => ({
                x: item.name,
                y: item.fileTotalSize,
                meta: item.name
            }));
            const clonedInfo = _.cloneDeep(chartInfo);
            clonedInfo.options.chart.events.dataPointSelection = onColumnSelect;
            clonedInfo.series = [{ data: data }];
            setChartInfo(clonedInfo);
            setUnit(unit);
        };

        fetchData();
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

    return (
        <div className="reco-tree-map-chart">
            <Chart
                options={chartInfo.options}
                series={chartInfo.series}
                type="treemap"
                width={"100%"}
                height={height}
            />
        </div>
    );
};

export default FileExtensionChart;
