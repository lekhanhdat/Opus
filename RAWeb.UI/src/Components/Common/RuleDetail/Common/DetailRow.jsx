const DetailRow = ({ label, children }) => {
    return <$g.DetailRow labelWidth={220}>
        <$g.DetailCell label={label} >
            <span tabIndex="0">{children}</span>
        </$g.DetailCell>
    </$g.DetailRow>;
};
export default DetailRow;