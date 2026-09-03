const IconText = ({icon, text, children, isEllipsis = true}) =>{
    return  <div className="ra-exist-status-text">
        <div className={icon}>
            <span className="path1"></span>
            <span className="path2"></span>
            <span className="path3"></span>
            <span className="path4"></span>
            <span className="path5"></span>
            <span className="path6"></span>
        </div>
        <div className={(isEllipsis ? "text-overflow " : "") + "margin-left-s" }>
            {children ?? text}
        </div>
    </div>;
};

const LinkText = ({href, text, className}) =>{
    return <a 
        rel="noopener noreferrer"
        target="_blank"
        href={href} 
        className={className || "ra-main-cell-link margin-right-xs"} 
    >{text}</a>;
};



export { IconText, LinkText};