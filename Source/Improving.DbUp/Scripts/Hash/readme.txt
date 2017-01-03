
*Scripts in all of these folders run if there is no record of
 them being run yet in SchemaMigrations OR if
 there is a change detected since the last run
 of the script (as tracked in SchemaMigrations by
 the ScriptHash column).

 The folder usage is as follows:

0050_PreScripts - So far we’ve identified that this could be unusual things
	like drops of foreign keys, changes with schemabinding, etc.
0500_Schemas
1000_Users
1500_Types
1750_Functions
2000_Tables
3000_Synonyms
3500_StoredProcedures
4000_Views
4500_Jobs - There might be a better place for these, but thinking of the jobs
	that update this database.
5000_Keys
9000_PostScripts - This could include things like the creation of foreign keys if
	they don’t already exist.

