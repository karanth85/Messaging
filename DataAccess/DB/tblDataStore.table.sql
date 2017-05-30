USE [MessageQueue]
GO

/****** Object:  Table [dbo].[Sprint]    Script Date: 03/03/2017 17:35:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tblMessageStore](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Topic] [varchar](20) NULL,
	[Source] [varchar](20) NOT NULL,
	[Content] NVARCHAR (MAX) NULL,
	[Created] [datetime] NOT NULL, 
	[Received] [datetime] NOT NULL	
 CONSTRAINT [PK_tblDataStore] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
