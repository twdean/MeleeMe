CREATE TABLE [dbo].[m_UserConnections] (
    [UserConnectionId] INT           IDENTITY (1, 1) NOT NULL,
    [UserId]           INT           NOT NULL,
    [ConnectionId]     INT           NOT NULL,
    [AccessToken]      VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_m_UserConnections] PRIMARY KEY CLUSTERED ([UserConnectionId] ASC),
    CONSTRAINT [fk_Connection] FOREIGN KEY ([ConnectionId]) REFERENCES [dbo].[m_Connections] ([ConnectionId]),
    CONSTRAINT [fk_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[m_User] ([UserId])
);

