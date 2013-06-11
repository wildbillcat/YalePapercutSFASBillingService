CREATE TABLE [dbo].dmofficeBilling_Billings
(
	[BatchID] SMALLINT IDENTITY (0,1) NOT NULL PRIMARY KEY, 
    [BatchStatus] TINYINT NOT NULL, 
    [BatchTotal] CHAR(9) NOT NULL, 
    [BatchTotalBalance] FLOAT NULL
)
