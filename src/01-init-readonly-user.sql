-- Check if user exists, if not create it
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'openmcp_readonly') THEN
      CREATE ROLE openmcp_readonly WITH LOGIN PASSWORD 'readonly_pass';
   END IF;
END
$do$;

-- Grant permissions on the specific database
GRANT CONNECT ON DATABASE test_db TO openmcp_readonly;

-- Grant usage on public schema
GRANT USAGE ON SCHEMA public TO openmcp_readonly;

-- Grant select on all existing tables
GRANT SELECT ON ALL TABLES IN SCHEMA public TO openmcp_readonly;

-- Ensure future tables are also readable
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO openmcp_readonly;