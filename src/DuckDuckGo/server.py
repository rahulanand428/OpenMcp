import os
import urllib.request
import re
from mcp.server.fastmcp import FastMCP
from ddgs  import DDGS
import uvicorn

# Initialize FastMCP server
mcp = FastMCP("duckduckgo-search")

# Configuration: Allow overriding the default result count via environment variables
DEFAULT_MAX_RESULTS = int(os.environ.get("DDG_DEFAULT_MAX_RESULTS", "10"))

@mcp.tool()
def search(query: str, max_results: int = DEFAULT_MAX_RESULTS) -> str:
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

@mcp.tool()
def fetch_page(url: str) -> str:
    """
    Fetches the text content of a specific URL. 
    Useful for reading the full content of a search result link.
    """
    try:
        # Basic browser-like headers to avoid some blocking
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        }
        req = urllib.request.Request(url, headers=headers)
        
        with urllib.request.urlopen(req, timeout=10) as response:
            # Read and decode
            html = response.read().decode('utf-8', errors='ignore')
            
            # Simple HTML stripping (remove scripts, styles, tags)
            # Remove script and style elements
            html = re.sub(r'<(script|style).*?</\1>', '', html, flags=re.DOTALL)
            # Remove tags
            text = re.sub(r'<[^>]+>', ' ', html)
            # Collapse whitespace
            text = re.sub(r'\s+', ' ', text).strip()
            
            return text[:10000] # Return first 10k chars to avoid context overflow
    except Exception as e:
        return f"Error fetching page: {str(e)}"

@mcp.tool()
def search_news(query: str, max_results: int = DEFAULT_MAX_RESULTS) -> str:
    """
    Searches for news articles. Returns headlines, sources, and dates.
    """
    try:
        results = DDGS().news(query, max_results=max_results)
        if not results:
            return "No news found."
        
        formatted = []
        for r in results:
            # News results often have 'date', 'title', 'body', 'url', 'source'
            date = r.get('date', 'Unknown date')
            source = r.get('source', 'Unknown source')
            formatted.append(f"Title: {r['title']}\nSource: {source} ({date})\nLink: {r['url']}\nSnippet: {r['body']}\n")
        
        return "\n---\n".join(formatted)
    except Exception as e:
        return f"Error performing news search: {str(e)}"

@mcp.tool()
def search_site(domain: str, query: str = "", max_results: int = DEFAULT_MAX_RESULTS) -> str:
    """
    Searches for pages within a specific website (domain).
    Example: domain="python.org", query="tutorial"
    """
    full_query = f"site:{domain} {query}".strip()
    try:
        results = DDGS().text(full_query, max_results=max_results)
        if not results:
            return "No results found."
        
        formatted = []
        for r in results:
            formatted.append(f"Title: {r['title']}\nLink: {r['href']}\nSnippet: {r['body']}\n")
        
        return "\n---\n".join(formatted)
    except Exception as e:
        return f"Error performing site search: {str(e)}"

if __name__ == "__main__":
    # Run the server using uvicorn
    uvicorn.run(mcp.sse_app, host="0.0.0.0", port=8080)
