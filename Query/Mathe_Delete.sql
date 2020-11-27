create proc MaThe_Delete
@MaThe nvarchar(50)

as
DELETE FROM [DANG_NHAP].[dbo].[SetThe]
      WHERE [MaThe] = @MaThe