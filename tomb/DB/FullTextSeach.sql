SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled');

CREATE FULLTEXT CATALOG TombCatalog AS DEFAULT;

EXEC sp_helpindex 'tomb';

CREATE FULLTEXT INDEX ON tomb(Name, Description)
KEY INDEX PK_Tomb
ON TombCatalog;

SELECT * FROM tomb
WHERE CONTAINS((Name, Description), 'example OR example1')
