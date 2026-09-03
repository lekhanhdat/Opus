import { useEffect, useState, forwardRef, useImperativeHandle } from 'react';

const ContinuationTokenPager = ({
    onChange, 
    totalCount, 
    continuationToken, 
    showPagerSize, 
    pagerSizeOptions, 
    shownCount, 
    showPagerCounter
}, ref) =>{

    const [ cachedContinuationTokens, setCachedContinuationTokens ] = useState([]);

    const [ pagerIndex, setPagerIndex ] = useState(0);

    const [ pagerSize, setPagerSize ] = useState(10);

    useEffect(()=>{
        addToCachedContinuationTokens();  
    },[continuationToken]);

    useImperativeHandle(ref, () => ({
        reset: () => { resetPager(); },
    }));

    const resetPager = () =>{
        setPagerIndex(0);
        setCachedContinuationTokens([]);
    };

    const addToCachedContinuationTokens = () =>{
        if (continuationToken && !cachedContinuationTokens.includes(continuationToken)) {
            cachedContinuationTokens.push(continuationToken);
        } 
    };
 
    const onPagerChange = (pagerIndex, pagerSize) => {
        let continuationToken = getContinuationToken(pagerIndex);  
        setPagerIndex(pagerIndex);
        setPagerSize(pagerSize);
        onChange(continuationToken, pagerSize);
    };

    const getContinuationToken = (currentPagerIndex) =>{
        let currentContinuationToken = "";
        if (currentPagerIndex != 0) {
            currentContinuationToken = continuationToken;
            if (currentPagerIndex < pagerIndex) {
                currentContinuationToken = cachedContinuationTokens[currentPagerIndex - 1];
            }
        } else {
            setCachedContinuationTokens([]);
        }
        return currentContinuationToken;
    };

    return <$g.SimplePager
        pagerIndex={pagerIndex}
        pagerSize={pagerSize}
        hasNext={totalCount - pagerSize * (pagerIndex * 1 + 1) > 0}
        shownCount={shownCount}
        showPagerSize={showPagerSize}
        pagerSizeOptions={pagerSizeOptions}
        showPagerCounter={showPagerCounter}
        totalCount={totalCount}
        onChange={onPagerChange}
    />;
};

export default forwardRef(ContinuationTokenPager);
