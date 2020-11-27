create proc Insert_XeVao_Bai
			@MaThe nvarchar(50)
           ,@GioVao nvarchar(50)
as
INSERT INTO [DANG_NHAP].[dbo].[QL_Xe]
           ([MaThe]
           ,[GioVao])
     VALUES
           (@MaThe
           ,@GioVao)