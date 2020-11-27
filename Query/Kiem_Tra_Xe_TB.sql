create proc Kiem_Tra_Xe_TB
@MaThe nvarchar(50)
as
SELECT *
  FROM QL_Xe
	Where @MaThe = MaThe