CREATE TABLE [dbo].[m_Melee] (
    [meleeId]    INT           IDENTITY (1, 1) NOT NULL,
    [challenger] VARCHAR (255) NOT NULL,
    [opponent]   VARCHAR (255) NOT NULL,
    [timestamp]  DATETIME      NOT NULL,
    CONSTRAINT [PK_m_Melee] PRIMARY KEY CLUSTERED ([meleeId] ASC)
);

