CREATE TABLE [dbo].dmofficeBilling_BillingTransactions
(
	[ActivityDate] CHAR(10) NOT NULL, 
    [Balance] FLOAT NULL, 
    [BatchID] CHAR(5) NULL, 
    [DetailCode] CHAR(4) NULL, 
    [NetID] NVARCHAR(50) NULL, 
    [PIDM] CHAR(8) NULL, 
    [SPRIDEN_ID] CHAR(9) NULL, 
    [Amount] CHAR(9) NULL, 
    [CreditIndicator] CHAR(2) NULL, 
    [TermCode] CHAR(6) NULL, 
    [BatchUserID] CHAR(8) NULL 
)
