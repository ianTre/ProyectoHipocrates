USE [klinicos]
GO

/****** Object:  StoredProcedure [dbo].[SP_OBTENER_ESPECIALIDADES]    Script Date: 10/9/2019 14:29:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[SP_OBTENER_ESPECIALIDADES]
AS
BEGIN

	SET NOCOUNT ON;

	SELECT * FROM General.Especialidades  where vigente= 1 order by nombre 
END
GO

