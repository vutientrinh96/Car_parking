create proc Xoa_DL_Xe
@MaThe nvarchar(50)

as
DELETE FROM [DANG_NHAP].[dbo].[QL_Xe]
      WHERE [MaThe] = @MaThe