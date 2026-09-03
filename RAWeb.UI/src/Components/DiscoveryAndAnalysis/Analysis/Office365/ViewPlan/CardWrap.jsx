const CardWrap = ({ title, className = "", children }) => {
    const cardClassName = ["ra-view-plan-card", className]
        .filter(Boolean)
        .join(" ");

    return (
        <article className={cardClassName}>
            <div className="ra-view-plan-card-title">{title}</div>
            {children}
        </article>
    );
};

export default CardWrap;