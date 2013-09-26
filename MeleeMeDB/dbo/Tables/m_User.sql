CREATE TABLE [dbo].[m_User] (
    [UserId]        INT           IDENTITY (1, 1) NOT NULL,
    [TwitterUserId] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_m_User] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

