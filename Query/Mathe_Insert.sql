create proc MaThe_Insert
			@MaThe nvarchar(50)
           ,@NgaySet nvarchar(50)
as
INSERT INTO [DANG_NHAP].[dbo].[SetThe]
           ([MaThe]
           ,[NgaySet])
     VALUES
           (@MaThe
           ,@NgaySet)