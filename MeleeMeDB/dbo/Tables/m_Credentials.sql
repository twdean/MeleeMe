CREATE TABLE [dbo].[m_Credentials] (
    [CredentialId] INT           IDENTITY (1, 1) NOT NULL,
    [UserId]       INT           NOT NULL,
    [AccessToken]  VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_m_Credentials] PRIMARY KEY CLUSTERED ([CredentialId] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[m_User] ([UserId])
);

