create proc Return_Gio_Vao
@MaThe nvarchar(50),
@GioVao nvarchar (50) OUTPUT 
as       
SELECT @GioVao = MAX(GioVao) FROM QL_Xe
WHERE [MaThe] = @MaThe