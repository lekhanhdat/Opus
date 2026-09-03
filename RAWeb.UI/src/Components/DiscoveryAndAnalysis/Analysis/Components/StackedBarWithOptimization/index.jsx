import React from "react";
import { useEffect } from "react";
import { useState } from "react";
import Chart from "react-apexcharts";
import "./index.less";
import { NumberUtil } from "../../Utils";

const StackedBarWithOptimization = ({
    items,
    categories,
    height
}) => {
    
    const [chartInfo, setChartInfo] = useState({
        options: {
            chart: {
                type: "bar",
                // height: 350,
                stacked: true,
                toolbar: {
                    show: false,
                },
            },
            dataLabels: {
                enabled: false,
            },
            plotOptions: {
                bar: {
                    horizontal: true,
                    dataLabels: {
                        total: {
                            enabled: false,
                        },
                    },
                    // barHeight: '10%',
                },
            },
            colors: ["#FEA43E", "#4A9CDF", "#FC6969"],
            stroke: {
                width: 1,
                colors: ["#fff"],
            },
            xaxis: {
                type: "category",
                tickAmount: 3,
                labels: {
                    formatter: function (val) {
                        return NumberUtil.internaltionalCounting(val) + "GB";
                    },
                    hideOverlappingLabels: true,
                    style: {
                        fontSize: "10px",
                    },
                },
                // rotate: -45,
                tickPlacement: "between",
            },
            yaxis: {
                title: {
                    text: undefined,
                },
            },
            tooltip: {
                y: {
                    formatter: function (val) {
                        return NumberUtil.internaltionalCounting(val) + "GB";
                    },
                },
            },
            legend: {
                position: "top",
                horizontalAlign: "right",
                offsetX: 40,
            },
        },
        series: [],
    });

    useEffect(() => {
        let series = [];
        for(let category of categories) {
            const data = [];
            for(let item of items) {
                const dataItem = item.data.filter(i => i.category === category.internalName);
                if(dataItem.length === 0) {
                    data.push(0);
                }
                else {
                    data.push(dataItem[0].value);
                }
            }
            series.push({
                name: category.displayName,
                data: data
            })
        }

        series = series.filter(serie => serie.data.some(i => i !== 0));
        const itemNames = items.map(i => i.name);

        const clonedChartInfo = _.cloneDeep(chartInfo);
        clonedChartInfo.options.xaxis.categories = itemNames;
        clonedChartInfo.series = series;
        setChartInfo(clonedChartInfo);
    }, [items, categories]);

    useEffect(() => {
        ApexCharts.exec(
            "test-chart",
            "updateOptions",
            chartInfo.options,
            false,
            false,
            true
        );
    }, [chartInfo]);

    return (
        <div className="reco-stacked-bar-optimization">
            <Chart
                options={chartInfo.options}
                series={chartInfo.series}
                type="bar"
                width={"100%"}
                height={height}
            />
        </div>
    );
};

export default StackedBarWithOptimization;
