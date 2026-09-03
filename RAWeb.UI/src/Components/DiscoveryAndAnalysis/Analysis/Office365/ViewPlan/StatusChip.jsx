const StatusChip = ({ backgroundColor = "transparent", color, className = "", children }) => {
    const chipClassName = ["ra-view-plan-status-chip", className]
        .filter(Boolean)
        .join(" ");

    return (
        <span className={chipClassName} style={{ backgroundColor, color }}>
            {children}
        </span>
    );
};

export default StatusChip;