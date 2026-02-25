from mcp.server.fastmcp import FastMCP
from ddgs  import DDGS
import uvicorn

# Initialize FastMCP server
mcp = FastMCP("duckduckgo-search")

@mcp.tool()
def search(query: str, max_results: int = 10) -> str:
    """
    Searches the web using DuckDuckGo.
    
    Args:
        query: The search query.
        max_results: Maximum number of results to return (default 5).
    """
    try:
        results = DDGS().text(query, max_results=max_results)
        if not results:
            return "No results found."
        
        formatted = []
        for r in results:
            formatted.append(f"Title: {r['title']}\nLink: {r['href']}\nSnippet: {r['body']}\n")
        
        return "\n---\n".join(formatted)
    except Exception as e:
        return f"Error performing search: {str(e)}"

if __name__ == "__main__":
    # Run the server using uvicorn
    # FastMCP automatically exposes /sse and /message endpoints
    import uvicorn
    uvicorn.run(mcp.sse_app, host="0.0.0.0", port=8080)
