
ORDER:
  1. FirstRun (if db doesn't exist; not logged to SchemaVersions table)
  2. BeforeMigration (only scripts not run prior)
  3. Migration (only scripts not run prior)
  4. AlwaysRun (all scripts, every time; not logged to SchemaVersions table)
  5. Test (all scripts, run every time; not logged to SchemaVersions table; Only run on DEV and QA)
  6. SeedData (only scripts not run prior)
  
*Be sure to set all .sql scripts' build actions
 to "Embedded Resource."

To control environments these are run in, you can use something like this in your sql script:
-----------------------------
IF '$AppUser$' <> 'UNDEFINED'
BEGIN
END
GO

IF '$Env$' = 'LOCAL'
BEGIN
END
GO