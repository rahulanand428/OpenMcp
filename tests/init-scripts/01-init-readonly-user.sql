-- Check if user exists, if not create it
-- 1. Create the Read-Only User explicitly
CREATE ROLE openmcp_readonly WITH LOGIN PASSWORD 'readonly_pass';

-- 2. Create a dummy table so the Console Client test has something to find
CREATE TABLE stock_prices (
    id SERIAL PRIMARY KEY,
    symbol VARCHAR(10),
    price DECIMAL(10, 2),
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO stock_prices (symbol, price) VALUES ('MSFT', 410.50), ('GOOGL', 175.20);

-- 3. Grant Permissions
GRANT CONNECT ON DATABASE test_db TO openmcp_readonly;
GRANT USAGE ON SCHEMA public TO openmcp_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO openmcp_readonly;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO openmcp_readonly;
