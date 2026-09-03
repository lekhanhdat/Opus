import { useEffect, useState } from "react";
import _ from "lodash";
import "./index.less";
import Chart from "react-apexcharts";

const defaultChartInfo = {
    series: [0],
    options: {
        chart: {
            type: 'radialBar',
            offsetY: -20,
            sparkline: {
                enabled: true
            },
            selection: {
                enabled: false,
            },
        },
        plotOptions: {
            radialBar: {
                startAngle: -90,
                endAngle: 90,
                dataLabels: {
                    name: {
                        show: true,
                        fontSize: '14px',
                        fontWeight: '0',
                        color: "#171D24",
                    },
                    value: {
                        offsetY: -38,
                        fontSize: '25px',
                        fontWeight: '600',
                        color: "#323E4D"
                    }
                }
            }
        },
        // tooltip: {
        //     enabled: true,
        //     enabledOnSeries: [70],
        // },
        grid: {
            padding: {
                top: -10
            }
        },
        labels: [],
        colors: ["#0866ca"]
    }
};

const PieChartWithProgress = ({ total, active, unit, name }) => {

    const [chartInfo, setChartInfo] = useState(defaultChartInfo);

    useEffect(() => {
        const clonedChartInfo = _.cloneDeep(defaultChartInfo);
        clonedChartInfo.series = [active];
        clonedChartInfo.options.labels = [name];
        setChartInfo(clonedChartInfo);
    }, [active, name]);

    return (
        <div className="reco-pie-char-with-progress">
            <Chart
                options={chartInfo.options}
                series={chartInfo.series}
                type="radialBar"
                width={"100%"}
                height={"250"}
            />
            <div className="reco-pie-char-start">{`${0} ${unit}`}</div>
            <div className="reco-pie-char-end">{`${total} ${unit}`}</div>
        </div>
    );
};

export default PieChartWithProgress;
