USE [klinicos]
GO

/****** Object:  StoredProcedure [dbo].[SP_Insertar_Profesional_Especialidad]    Script Date: 10/9/2019 14:29:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[SP_Insertar_Profesional_Especialidad]
(
@idProfesional int,
@idEspecialidad int

)
AS
BEGIN

	insert into Persona.ProfesionalesEspecialidades values(@idProfesional,@idEspecialidad)

END
GO

