# ReportU — Matriz de Casos de Prueba (MVP)

## 1. Autenticación
- [ ] **TC-01:** Registro exitoso de nuevo estudiante con datos válidos.
- [ ] **TC-02:** Intento de registro con correo duplicado o campos vacíos (error controlado).
- [ ] **TC-03:** Inicio de sesión exitoso y almacenamiento de token de sesión.
- [ ] **TC-04:** Intento de inicio de sesión con contraseña incorrecta.

## 2. Creación y Visualización de Publicaciones
- [ ] **TC-05:** Creación de publicación tomando foto directa desde la cámara.
- [ ] **TC-06:** Creación de publicación seleccionando imagen de la galería.
- [ ] **TC-07:** Subida exitosa a Azure Blob Storage y guardado de URL en base de datos.
- [ ] **TC-08:** Visualización correcta del post e imagen en el feed principal.
- [ ] **TC-09:** Visualización detallada del post al seleccionarlo.

## 3. Interacciones Sociales
- [ ] **TC-10:** Usuario B agrega un comentario en la publicación del Usuario A.
- [ ] **TC-11:** Usuario B da "apoyo" (like/upvote) y el contador se actualiza.

## 4. Edición y Eliminación
- [ ] **TC-12:** Edición de título y contenido por parte del propietario del post.
- [ ] **TC-13:** Intento de edición/eliminación por un usuario ajeno (acción bloqueada/oculta).
- [ ] **TC-14:** Eliminación de publicación por su creador (desaparece del feed).
- [ ] **TC-15:** Verificación de borrado físico del archivo en Azure Blob Storage tras eliminar el post.
