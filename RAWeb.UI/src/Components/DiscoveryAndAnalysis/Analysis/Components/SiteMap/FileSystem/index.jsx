function FileSystemSiteMap(props) {
    const { URL } = props;

    return (
        <section className="reco-sitemap">
            <div className="margin-top-l">
                <$g.SiteMap data={URL} />
            </div>
        </section>
    );
}

export default FileSystemSiteMap;
