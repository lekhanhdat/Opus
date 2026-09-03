const Search = ({onSearch, placeholder = RMResx.RM_JS_RM_SearchContainerTxt, width}, ref) =>{

    const handleSearch = (args) =>{
        onSearch(args ? args : "");
    }; 

    return <R.Searchbox 
        placeholder={placeholder}
        onSearch={handleSearch}
        width={width || 380}
    />;
};

export default Search;