USE [ExpertDB]
GO

/****** Object:  Table [dbo].[Product] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Product](
	[ProductID] [int] IDENTITY(1,1) NOT NULL,
	[Brands] [nvarchar](100) NULL,
	[Models] [nvarchar](100) NULL,
	[Colors] [nvarchar](100) NULL,
	[Memory] [int] NULL,
	[Storage] [int] NULL,
	[Camera] [bit] NULL,
	[Rating] [decimal](18, 0) NULL,
	[SellingPrice] [decimal](18, 0) NULL,
	[OriginalPrice] [decimal](18, 0) NULL,
	[Mobile] [nvarchar](100) NULL,
	[Discount] [decimal](18, 0) NULL,
	[DiscountPercentage] [decimal](18, 0) NULL,
	[OS] [nvarchar](50) NULL,
	[SellersAmount] [int] NULL,
	[ScreenSize] [decimal](18, 0) NULL,
	[BatterySize] [int] NULL,
	[Reviews] [int] NULL,

	PRIMARY KEY CLUSTERED 
	(
		[ProductID] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO