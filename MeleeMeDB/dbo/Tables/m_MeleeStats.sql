CREATE TABLE [dbo].[m_MeleeStats] (
    [meleeStatisticsId] INT           IDENTITY (1, 1) NOT NULL,
    [meleeId]           INT           NOT NULL,
    [meleeWinner]       VARCHAR (255) NOT NULL,
    [meleeLoser]        VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_m_MeleeStats] PRIMARY KEY CLUSTERED ([meleeStatisticsId] ASC),
    CONSTRAINT [FK_Melee_MeleeId] FOREIGN KEY ([meleeId]) REFERENCES [dbo].[m_Melee] ([meleeId])
);

