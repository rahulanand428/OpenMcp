import os
from mcp.server.fastmcp import FastMCP
import uvicorn

# Initialize FastMCP server
mcp = FastMCP("filesystem")

# Security: Only allow access within this directory
DATA_DIR = "/data"

def validate_path(path: str) -> str:
    """Ensures the path is within the allowed DATA_DIR."""
    # Normalize path to remove .. components
    full_path = os.path.abspath(os.path.join(DATA_DIR, path.lstrip("/")))
    if not full_path.startswith(os.path.abspath(DATA_DIR)):
        raise ValueError(f"Access denied: Path must be within {DATA_DIR}")
    return full_path

@mcp.tool()
def list_directory(path: str = ".") -> str:
    """Lists files and directories in the specified path."""
    try:
        target = validate_path(path)
        if not os.path.exists(target):
            return "Path does not exist."
        
        items = os.listdir(target)
        return "\n".join(items) if items else "(empty directory)"
    except Exception as e:
        return f"Error: {str(e)}"

@mcp.tool()
def read_file(path: str) -> str:
    """Reads the content of a file."""
    try:
        target = validate_path(path)
        if not os.path.exists(target):
            return "File does not exist."
        
        with open(target, "r", encoding="utf-8") as f:
            return f.read()
    except Exception as e:
        return f"Error reading file: {str(e)}"

@mcp.tool()
def write_file(path: str, content: str) -> str:
    """Writes content to a file (overwrites if exists)."""
    try:
        target = validate_path(path)
        
        # Ensure directory exists
        os.makedirs(os.path.dirname(target), exist_ok=True)
        
        with open(target, "w", encoding="utf-8") as f:
            f.write(content)
        return f"Successfully wrote to {path}"
    except Exception as e:
        return f"Error writing file: {str(e)}"

if __name__ == "__main__":
    # Run the server using uvicorn
    import uvicorn
    uvicorn.run(mcp.sse_app, host="0.0.0.0", port=8080)
