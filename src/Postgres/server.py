import os
import psycopg2
from psycopg2.extras import RealDictCursor
from mcp.server.fastmcp import FastMCP
import uvicorn

# Initialize FastMCP server
mcp = FastMCP("postgres-query")

DB_URL = os.environ.get("DATABASE_URL")

def get_connection():
    return psycopg2.connect(DB_URL)

@mcp.tool()
def query(sql: str) -> str:
    """
    Executes a read-only SQL query against the database.
    
    Args:
        sql: The SQL query to execute (SELECT only).
    """
    # Basic safety check (The DB user should also be read-only)
    if "drop " in sql.lower() or "delete " in sql.lower() or "update " in sql.lower() or "insert " in sql.lower():
        return "Error: Only SELECT queries are allowed."

    conn = None
    try:
        conn = get_connection()
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            cur.execute(sql)
            rows = cur.fetchall()
            
            if not rows:
                return "No results found."
            
            # Format as string (or JSON)
            return str(rows)
            
    except Exception as e:
        return f"Database Error: {str(e)}"
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    # Run the server using uvicorn
    # FastMCP automatically exposes /sse and /message endpoints
    import uvicorn
    uvicorn.run(mcp.sse_app, host="0.0.0.0", port=8080)