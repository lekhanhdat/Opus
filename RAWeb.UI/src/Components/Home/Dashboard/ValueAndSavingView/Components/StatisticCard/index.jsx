import './index.less';

export const StatisticCard = (props) => {
    const { title, description, value, unit, isLoading } = props;

    return (
        <div className="reco-dashboard-statistic-card">
            <div tabIndex={0}>
                {title}
                <$g.Popover>{description}</$g.Popover>
            </div>
            <div className="flex-row align-end gap-xs">
                <div className="card-value" tabIndex={0}>{value}</div>
                {unit && <div tabIndex={0}>{unit}</div>}
            </div>
        </div>
    )
}