USE [klinicos]
GO

/****** Object:  StoredProcedure [dbo].[SP_Insertar_Profesional]    Script Date: 10/9/2019 14:28:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[SP_Insertar_Profesional]
(
@contactoObsevaciones varchar(100),
@email varchar(100),
@fechaCrea datetime ,
@fechaModi datetime,
@idSexo int,
@idTipoDocumento int,
@matricula varchar(50),
@numeroDocumento varchar(20),
@primerApellido varchar(100),
@primerNombre  varchar(100),
@otrosNombres  varchar(100),
@telefono varchar(50),
@tipoTelefono varchar(3),
@usuarioCrea varchar(50),
@usuarioModi varchar(50),
@vigente bit
)
AS
BEGIN

	SET NOCOUNT ON;

	INSERT INTO General.Profesionales(
		contactoObservaciones,
		email ,
		fechaCrea ,
		fechaModi,
		idSexo ,
		idTipoDocumento ,
		matricula ,
		numeroDocumento ,
		primerApellido ,
		primerNombre  ,
		otrosNombres,
		telefono ,
		tipoTelefono ,
		usuarioCrea ,
		usuarioModi ,
		vigente )
	 VALUES( 
		@contactoObsevaciones ,
		@email ,
		@fechaCrea ,
		@fechaModi ,
		@idSexo ,
		@idTipoDocumento ,
		@matricula ,
		@numeroDocumento ,
		@primerApellido ,
		@primerNombre  ,
		@otrosNombres,
		@telefono ,
		@tipoTelefono ,
		@usuarioCrea ,
		@usuarioModi ,
		@vigente )
END
GO

