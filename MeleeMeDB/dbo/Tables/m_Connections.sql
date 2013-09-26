CREATE TABLE [dbo].[m_Connections] (
    [ConnectionId]   INT           IDENTITY (1, 1) NOT NULL,
    [ConnectionName] VARCHAR (255) NOT NULL,
    [ConnectionIcon] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_m_Connections] PRIMARY KEY CLUSTERED ([ConnectionId] ASC)
);

