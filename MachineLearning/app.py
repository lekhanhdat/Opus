import uvicorn
from fastapi import FastAPI, Request, status
from fastapi.responses import JSONResponse
from utils import get_summary

app = FastAPI()


@app.post('/generate_summary')
async def gen_summary(request: Request):
    body = await request.json()
    doc_content = body.get("doc_content", "")
    summary_size = body.get("summary_size", 4)

    summary = get_summary(doc_content, summary_size)

    return JSONResponse(content={"summary": summary}, status_code=status.HTTP_200_OK)


@app.get('/healthz', status_code=200)
def health():
    return "ok"


# local test
if __name__ == "__main__":
    uvicorn.run(app=app, host="0.0.0.0", port=3389)
