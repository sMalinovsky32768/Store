USE [StoreDB]
GO
/****** Object:  UserDefinedFunction [dbo].[FindGoodType]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FindGoodType]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'


CREATE FUNCTION [dbo].[FindGoodType] 
(
	@Name nvarchar(50),
	@Code nvarchar(10)
)
RETURNS int
AS
BEGIN
	DECLARE @res int

	select top(1) @res = ID from GoodType where Name=@Name and Code=@Code

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[FindProducer]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FindProducer]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'


CREATE FUNCTION [dbo].[FindProducer] 
(
	@Name nvarchar(50),
	@Code nvarchar(10)
)
RETURNS int
AS
BEGIN
	DECLARE @res int

	select top(1) @res = ID from Producer where Name=@Name and Code=@Code

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetBasketTotalValue]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBasketTotalValue]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'

CREATE FUNCTION [dbo].[GetBasketTotalValue] 
(
	@uid int
)
RETURNS money
AS
BEGIN
	DECLARE @res money

	select @res = SUM([dbo].[GetBasketValue](ID)) from Basket where UserID=@uid and IsPlaced=0

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetBasketValue]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBasketValue]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'

CREATE FUNCTION [dbo].[GetBasketValue] 
(
	@baskedid int
)
RETURNS money
AS
BEGIN
	DECLARE @res money

	select top(1) @res = Good.Value*GoodCount from Basket inner join Good 
	on Basket.GoodID=Good.ID where Basket.ID=@baskedid

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetFromBasket]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFromBasket]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[GetFromBasket] 
(
	@uid int,
	@goodID int
)
RETURNS int
AS
BEGIN
	DECLARE @res int

	select top(1) @res = ID from Basket where UserID=@uid and GoodID=@goodID and IsPlaced=0
	if @res is null
	set @res = 0

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetGoodTypeString]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGoodTypeString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[GetGoodTypeString] 
(
	@ID int
)
RETURNS nvarchar(65)
AS
BEGIN
	DECLARE @res nvarchar(65)

	select @res = Name + N''('' + Code + N'')'' from GoodType where ID=@ID

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetProducerString]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetProducerString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[GetProducerString] 
(
	@ID int
)
RETURNS nvarchar(65)
AS
BEGIN
	DECLARE @res nvarchar(65)

	select @res = Name + N''('' + Code + N'')'' from Producer where ID=@ID

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IsAvailableName]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsAvailableName]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IsAvailableName] 
(
	@Name nvarchar(30)
)
RETURNS bit
AS
BEGIN
	DECLARE @res bit

	if not exists (select * from [User] where [Name]=@Name)
	set @res = 1
	else 
	set @res = 0

	RETURN @res

END
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IsOrderPlaced]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IsOrderPlaced]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
CREATE FUNCTION [dbo].[IsOrderPlaced] 
(
	@id int
)
RETURNS bit
AS
BEGIN
	DECLARE @res bit

	if not exists (select * from [Order] where BasketID=@id)
	set @res = 0
	else 
	set @res = 1

	RETURN @res

END
' 
END
GO
/****** Object:  Table [dbo].[Good]    Script Date: 16.07.2020 22:23:58  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Good]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Good](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Articul] [nvarchar](6) NOT NULL,
	[Value] [money] NOT NULL,
	[ProducerID] [int] NOT NULL,
	[GoodTypeID] [int] NOT NULL,
 CONSTRAINT [PK_Good] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetGoods]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGoods]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetGoods]()
RETURNS TABLE 
AS
RETURN 
(
	SELECT ID, Name, Articul, Value, dbo.GetGoodTypeString(GoodTypeID) as [GoodType],
	dbo.GetProducerString(ProducerID) as [Producer] from Good
)
' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetGood]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetGood]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetGood](@id int)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ID, Name, Articul, Value, dbo.GetGoodTypeString(ID) as [GoodType],
	dbo.GetProducerString(ID) as [Producer] from Good where ID=@id
)
' 
END
GO
/****** Object:  Table [dbo].[Basket]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Basket]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Basket](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[GoodID] [int] NOT NULL,
	[GoodCount] [int] NOT NULL,
	[IsPlaced]  AS ([dbo].[IsOrderPlaced]([ID])),
 CONSTRAINT [PK_Basket] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetUserBasketList]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserBasketList]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetUserBasketList](@UserID int)
RETURNS TABLE 
AS
RETURN 
(
	SELECT Basket.ID, Good.Name, Good.Articul, Good.Value, 
	dbo.GetGoodTypeString(Good.ID) as [GoodType],
	dbo.GetProducerString(Good.ID) as [Producer], GoodCount 
	from Good inner join Basket ON Basket.GoodID=Good.ID
	where Basket.UserID=@UserID and Basket.IsPlaced=0
)
' 
END
GO
/****** Object:  Table [dbo].[DeliveryMethod]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryMethod]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DeliveryMethod](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DeliveryMethod] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[GoodType]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GoodType]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GoodType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_GoodType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Order]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Order]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Order](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BasketID] [int] NOT NULL,
	[DeliveryMethodID] [int] NOT NULL,
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Producer]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Producer]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Producer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Code] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_Producer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[User]    Script Date: 16.07.2020 22:24:00  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[User](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
	[Password] [nvarchar](24) NOT NULL,
	[IsAdmin] [bit] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_User_IsAdmin]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_IsAdmin]  DEFAULT ((0)) FOR [IsAdmin]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Basket_Good]') AND parent_object_id = OBJECT_ID(N'[dbo].[Basket]'))
ALTER TABLE [dbo].[Basket]  WITH CHECK ADD  CONSTRAINT [FK_Basket_Good] FOREIGN KEY([GoodID])
REFERENCES [dbo].[Good] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Basket_Good]') AND parent_object_id = OBJECT_ID(N'[dbo].[Basket]'))
ALTER TABLE [dbo].[Basket] CHECK CONSTRAINT [FK_Basket_Good]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Basket_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Basket]'))
ALTER TABLE [dbo].[Basket]  WITH CHECK ADD  CONSTRAINT [FK_Basket_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Basket_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Basket]'))
ALTER TABLE [dbo].[Basket] CHECK CONSTRAINT [FK_Basket_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Good_GoodType]') AND parent_object_id = OBJECT_ID(N'[dbo].[Good]'))
ALTER TABLE [dbo].[Good]  WITH CHECK ADD  CONSTRAINT [FK_Good_GoodType] FOREIGN KEY([GoodTypeID])
REFERENCES [dbo].[GoodType] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Good_GoodType]') AND parent_object_id = OBJECT_ID(N'[dbo].[Good]'))
ALTER TABLE [dbo].[Good] CHECK CONSTRAINT [FK_Good_GoodType]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Good_Producer]') AND parent_object_id = OBJECT_ID(N'[dbo].[Good]'))
ALTER TABLE [dbo].[Good]  WITH CHECK ADD  CONSTRAINT [FK_Good_Producer] FOREIGN KEY([ProducerID])
REFERENCES [dbo].[Producer] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Good_Producer]') AND parent_object_id = OBJECT_ID(N'[dbo].[Good]'))
ALTER TABLE [dbo].[Good] CHECK CONSTRAINT [FK_Good_Producer]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Order_Basket]') AND parent_object_id = OBJECT_ID(N'[dbo].[Order]'))
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Order_Basket] FOREIGN KEY([BasketID])
REFERENCES [dbo].[Basket] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Order_Basket]') AND parent_object_id = OBJECT_ID(N'[dbo].[Order]'))
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_Basket]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Order_DeliveryMethod]') AND parent_object_id = OBJECT_ID(N'[dbo].[Order]'))
ALTER TABLE [dbo].[Order]  WITH CHECK ADD  CONSTRAINT [FK_Order_DeliveryMethod] FOREIGN KEY([DeliveryMethodID])
REFERENCES [dbo].[DeliveryMethod] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Order_DeliveryMethod]') AND parent_object_id = OBJECT_ID(N'[dbo].[Order]'))
ALTER TABLE [dbo].[Order] CHECK CONSTRAINT [FK_Order_DeliveryMethod]
GO
