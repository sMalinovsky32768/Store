INSERT [dbo].[DeliveryMethod] ([Name]) VALUES (N'Самовывоз')
GO
INSERT [dbo].[DeliveryMethod] ([Name]) VALUES (N'Почта России')
GO
INSERT [dbo].[DeliveryMethod] ([Name]) VALUES (N'Курьерская доставка')
GO
INSERT [dbo].[User] ([Name], [Password], [IsAdmin]) VALUES (N'admin', N'RCNfOS9HrZIfk1f+GkSBww==', 1)
GO
INSERT [dbo].[User] ([Name], [Password], [IsAdmin]) VALUES (N'user', N'IRKh6R8b6wIgrH5XSOhnfA==', 0)
