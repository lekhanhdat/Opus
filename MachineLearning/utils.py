import nltk
nltk.download('punkt_tab')
nltk.download('stopwords')
from rake_nltk import Rake
from logger import logger

logger.info("Loading NLTK resources successful.")


stop_words = nltk.corpus.stopwords.words('english')
def get_summary(text, summary_size=4):
    logger.info("Generating summary...")
    logger.info(f"Summary size: {summary_size}KB")
    text = text.lower()
    r = Rake(stopwords=stop_words)
    r.extract_keywords_from_text(text)

    results = r.get_ranked_phrases()
    logger.info(f"Number of phrases extracted: {len(results)}")
    summary = ""
    for i in results:
        if len((summary + i).encode()) < summary_size * 1024:
            summary += i + " "
        else:
            break
    if not summary:
        summary = results[0]
    del r, results

    return summary
