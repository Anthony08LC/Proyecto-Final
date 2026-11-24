# QuoteFlow AI — Especificación del Sistema

| Campo | Valor |
|---|---|
| Documento | Especificación oficial del sistema (`spec.md`) |
| Producto | QuoteFlow AI |
| Cliente | TechSolutions CR |
| Versión del documento | 1.0 |
| Fecha | 16 de noviembre de 2026 |
| Estado | Aprobado — fuente de verdad del desarrollo |
| Alcance | Versión 1 (V1) |

> Este documento es la fuente de verdad del proyecto. Cualquier cambio de alcance, regla o diseño debe reflejarse primero aquí y luego en el código. Los documentos `intent.md`, `plan.md`, `README.md` y `docs/` deben mantenerse coherentes con esta especificación.

---

## Tabla de contenido

1. [Introducción](#1-introducción)
2. [Visión general del sistema](#2-visión-general-del-sistema)
3. [Arquitectura general](#3-arquitectura-general)
4. [Módulos](#4-módulos)
5. [Requisitos funcionales](#5-requisitos-funcionales)
6. [Requisitos no funcionales](#6-requisitos-no-funcionales)
7. [Reglas de negocio](#7-reglas-de-negocio)
8. [Modelo de dominio](#8-modelo-de-dominio)
9. [Modelo de datos](#9-modelo-de-datos)
10. [Seguridad](#10-seguridad)
11. [Integración con inteligencia artificial](#11-integración-con-inteligencia-artificial)
12. [Restricciones técnicas](#12-restricciones-técnicas)
13. [Fuera de alcance](#13-fuera-de-alcance)
14. [Criterios de aceptación del producto](#14-criterios-de-aceptación-del-producto)
- [Anexo A. Glosario](#anexo-a-glosario)
- [Anexo B. Casos de uso](#anexo-b-casos-de-uso)

---

## 1. Introducción

### 1.1 Propósito

Este documento especifica de forma completa y verificable el sistema QuoteFlow AI en su versión 1: su alcance, arquitectura, módulos, requisitos funcionales y no funcionales, reglas de negocio, modelo de dominio, modelo de datos, seguridad, integración con inteligencia artificial y criterios de aceptación.

Está dirigido a quienes desarrollan, revisan, prueban y evalúan el sistema.

### 1.2 Objetivos

1. Reducir significativamente el tiempo necesario para elaborar una cotización comercial.
2. Automatizar todos los cálculos financieros de una cotización (descuentos, IVA y totales).
3. Eliminar los errores de cálculo y los descuentos inconsistentes.
4. Mantener trazabilidad completa sobre clientes, productos y cotizaciones.
5. Generar documentos comerciales profesionales listos para enviar a clientes.
6. Ofrecer una experiencia de uso equivalente a la de un producto SaaS profesional y moderno.

### 1.3 Alcance

La V1 incluye:

- Autenticación con roles Administrador y Ventas, y gestión de usuarios.
- Gestión de clientes, categorías y productos.
- Gestión completa del ciclo de vida de las cotizaciones: borrador, emisión, aceptación, rechazo, vencimiento, cancelación y duplicación.
- Motor de cálculo con descuentos por tramo, descuento VIP e IVA, con parámetros configurables.
- Documento comercial con vista previa HTML y PDF.
- Asistente de redacción comercial con Google Gemini.
- Configuración del sistema.
- Auditoría mediante historial de eventos.
- Dashboard con indicadores.

Lo que queda fuera de la V1 se detalla en la [sección 13](#13-fuera-de-alcance).

---

## 2. Visión general del sistema

### 2.1 Descripción del producto

QuoteFlow AI es una plataforma web empresarial para la gestión integral de cotizaciones comerciales. Permite a un equipo de ventas crear propuestas profesionales de forma rápida, consistente, auditable y centralizada, a partir de un catálogo de productos y una cartera de clientes, con cálculos financieros automáticos y un documento comercial listo para enviar.

### 2.2 Problema que resuelve

TechSolutions CR vende equipos tecnológicos, licencias de software, servicios profesionales y soluciones empresariales. Hoy elabora sus cotizaciones manualmente con hojas de cálculo y documentos independientes, lo que genera:

| Problema actual | Respuesta de QuoteFlow AI |
|---|---|
| Pérdida de tiempo | Catálogo centralizado, cálculo automático, duplicación y redacción asistida |
| Errores de cálculo | Motor de cálculo único en el backend, con restricciones de integridad en la base de datos |
| Descuentos inconsistentes | Reglas de descuento automáticas y configurables; la condición VIP solo la asigna el Administrador |
| Historial disperso | Todas las cotizaciones persistidas, consultables y buscables |
| Falta de control | Roles, permisos, ciclo de vida con estados y bloqueo de cotizaciones emitidas |
| Baja trazabilidad | Historial de eventos de auditoría y congelamiento de datos al emitir |

### 2.3 Usuarios

| Rol | Responsabilidades |
|---|---|
| **Administrador** | Gestión de usuarios, clientes, categorías y productos; configuración del sistema; revisión de cotizaciones; consulta de auditoría. Puede realizar todas las acciones sobre cualquier cotización. |
| **Ventas** (ejecutivo de ventas) | Registrar y consultar clientes; consultar el catálogo; crear, emitir, duplicar y gestionar sus cotizaciones; consultar el historial; usar el asistente comercial. |
| **Sistema** | Actor técnico que ejecuta procesos automáticos (vencimiento de cotizaciones) y registra eventos. |

El cliente final no usa el sistema en la V1. Recibe el documento comercial por canales externos.

### 2.4 Beneficios

- **Productividad:** una cotización completa en minutos, sin cálculos manuales.
- **Exactitud:** los montos oficiales los calcula siempre el mismo motor, con las mismas reglas.
- **Consistencia:** todos los documentos comparten formato, reglas y condiciones.
- **Control:** permisos por rol y cotizaciones inmutables después de emitidas.
- **Trazabilidad:** cada acción relevante queda registrada con usuario, fecha y detalle.
- **Imagen profesional:** documentos comerciales cuidados y textos redactados con apoyo de IA.

---

## 3. Arquitectura general

QuoteFlow AI es un **monolito modular**: una única aplicación backend que expone una API REST, organizada en módulos de negocio con límites claros, y una aplicación frontend de una sola página (SPA).

### 3.1 Frontend

- **Tecnologías:** React, TypeScript (modo estricto) y Vite.
- **Responsabilidad:** presentación, interacción y validación de formularios para dar retroalimentación inmediata. **No calcula montos oficiales** ni aplica reglas de negocio de forma autoritativa.
- **Principios:**
  - Componentes reutilizables y un sistema visual consistente, con modo claro y oscuro.
  - Separación entre lógica (acceso a la API, estado) y presentación (componentes).
  - Capa de acceso a la API tipada, aislada de los componentes.
  - Estado organizado: los datos del servidor se gestionan como caché de consultas; el estado global se limita a la sesión y las preferencias de interfaz.
  - Formularios validados por esquema.
  - Rutas protegidas por autenticación y por rol.

### 3.2 Backend

- **Tecnologías:** ASP.NET Core y Entity Framework Core con el proveedor de PostgreSQL.
- **Estilo:** API REST, monolito modular.
- **Organización interna por capas ligeras:**
  - **Dominio:** entidades, value objects, reglas, máquina de estados de la cotización y motor de cálculo. No depende de la base de datos ni de HTTP.
  - **Aplicación:** servicios pequeños por caso de uso, DTOs de entrada y salida, validación centralizada y orquestación de transacciones y auditoría.
  - **Infraestructura:** persistencia con EF Core y migraciones, numeración consecutiva, cliente de Gemini, generación de documentos y proceso de vencimiento.
  - **API:** endpoints REST, autenticación y autorización, manejo de errores.
- **Aspectos transversales:** inyección de dependencias, manejo de errores consistente con respuestas *ProblemDetails*, logging estructurado básico, llenado automático de los campos de auditoría y reloj inyectable para las reglas dependientes de la fecha.

### 3.3 Base de datos

- **Motor:** PostgreSQL 18.
- **Principios:** normalización adecuada, integridad referencial, restricciones de datos como red de seguridad, índices solo donde aportan valor y cambios exclusivamente mediante migraciones controladas.
- **Identificadores:** `uuid` versión 7 generados con `uuidv7()`. La tabla de auditoría usa `bigint` autoincremental.
- **Sin extensiones adicionales.**
- **Dos usuarios de base de datos:** propietario del esquema (migraciones) y usuario de aplicación (permisos mínimos).

El detalle se encuentra en la [sección 9](#9-modelo-de-datos).

### 3.4 Integración con Gemini

- Google Gemini API se usa exclusivamente como **asistente de redacción** de los textos comerciales de una cotización.
- Toda llamada se hace **desde el backend**, detrás de una interfaz propia del módulo Asistente comercial. La clave de API nunca llega al frontend.
- La integración es **opcional en tiempo de ejecución**: si no está configurada o falla, el resto del sistema funciona con normalidad.
- El nombre del modelo de Gemini se define por configuración, no en el código.

El detalle se encuentra en la [sección 11](#11-integración-con-inteligencia-artificial).

### 3.5 Infraestructura

- Servidor: Ubuntu Server.
- Comunicación por HTTPS en producción.
- Secretos y cadenas de conexión mediante variables de entorno.
- Respaldo diario de la base de datos.

### 3.6 Git y control de versiones

- El proyecto usa Git desde su inicio.
- Convención de commits:

| Prefijo | Uso |
|---|---|
| `feat:` | Nueva funcionalidad |
| `fix:` | Corrección de errores |
| `refactor:` | Mejora interna sin cambio de comportamiento |
| `docs:` | Documentación |
| `test:` | Pruebas |
| `chore:` | Mantenimiento |

- Los commits deben ser pequeños, descriptivos y trazables a un requisito o incremento.
- Ningún secreto ni credencial se almacena en el repositorio.

---

## 4. Módulos

Cada módulo es dueño de sus datos. **Ningún módulo modifica datos de otro**: cuando un módulo necesita información de otro, la obtiene mediante una interfaz de consulta.

### 4.1 Autenticación

- **Responsabilidad:** identificar a los usuarios, emitir y validar sesiones y controlar el acceso por rol.
- **Funcionalidades:** inicio y cierre de sesión, validación de la sesión en cada solicitud, control de acceso por rol, perfil propio, cambio de contraseña y cambio obligatorio de contraseña temporal.
- **Dependencias:** Usuarios (datos de credenciales y rol), Auditoría.

### 4.2 Usuarios

- **Responsabilidad:** administrar las cuentas de usuario del sistema.
- **Funcionalidades:** listar y buscar, crear, editar, cambiar rol, desactivar y reactivar, y restablecer contraseña.
- **Dependencias:** Auditoría.

### 4.3 Clientes

- **Responsabilidad:** mantener la cartera de clientes y su condición comercial.
- **Funcionalidades:** registrar, buscar, ver detalle con historial de cotizaciones, editar, marcar o quitar la condición VIP, y desactivar y reactivar.
- **Dependencias:** Auditoría; Cotizaciones (solo lectura, para mostrar el historial del cliente).

### 4.4 Categorías

- **Responsabilidad:** clasificar el catálogo de productos.
- **Funcionalidades:** crear, listar, editar, y desactivar y reactivar.
- **Dependencias:** Auditoría.

### 4.5 Productos

- **Responsabilidad:** mantener el catálogo de productos y sus precios vigentes.
- **Funcionalidades:** crear, buscar, editar (incluido el precio), y desactivar y reactivar.
- **Dependencias:** Categorías (lectura), Auditoría.

### 4.6 Cotizaciones (núcleo)

- **Responsabilidad:** gestionar el ciclo de vida completo de las cotizaciones y calcular sus montos oficiales.
- **Funcionalidades:** crear borradores; gestionar líneas, cliente y textos; calcular el desglose; emitir con numeración consecutiva y congelamiento; registrar aceptación y rechazo; cancelar; vencer automáticamente; duplicar; consultar el detalle y el historial.
- **Componentes clave:** agregado Cotización con su máquina de estados, motor de cálculo, generador del número consecutivo y proceso de vencimiento.
- **Dependencias:** Clientes, Productos y Configuración (lectura), Auditoría.

### 4.7 Documentos

- **Responsabilidad:** producir el documento comercial de una cotización.
- **Funcionalidades:** vista previa HTML y generación de PDF a partir de la misma plantilla y los mismos datos.
- **Dependencias:** Cotizaciones y Configuración (lectura).

### 4.8 Asistente IA

- **Responsabilidad:** proponer textos comerciales redactados con Google Gemini.
- **Funcionalidades:** generar y mejorar los cuatro textos comerciales de un borrador, con degradación controlada si Gemini no está disponible y límite de uso por usuario.
- **Dependencias:** Cotizaciones (lectura del contexto), Auditoría, Google Gemini API (externa).

### 4.9 Configuración

- **Responsabilidad:** mantener los datos de la empresa y los parámetros comerciales del sistema.
- **Funcionalidades:** consultar y actualizar los datos de la empresa, las condiciones comerciales por defecto, los parámetros comerciales (IVA, tramos de descuento, descuento VIP) y la vigencia.
- **Dependencias:** Auditoría; Cotizaciones (solo para informar cuántos borradores se recalcularán).

### 4.10 Auditoría

- **Responsabilidad:** registrar de forma inmutable las acciones relevantes y permitir consultarlas.
- **Funcionalidades:** registro de eventos en la misma transacción que el cambio; consulta con filtros; historial por entidad.
- **Dependencias:** ninguna. Todos los módulos dependen de ella.

### 4.11 Dashboard

- **Responsabilidad:** presentar indicadores y actividad reciente.
- **Funcionalidades:** indicadores generales, cotizaciones recientes y actividad reciente.
- **Dependencias:** lectura de Clientes, Productos, Cotizaciones y Auditoría. No tiene datos propios.

---

## 5. Requisitos funcionales

**Convenciones**

- **Prioridad:** **E** = esencial, **I** = importante, **D** = deseable.
- **Propietario:** el ejecutivo dueño de una cotización.
- Todos los eventos de auditoría mencionados se registran según RN-19.

**Resumen:** 53 requisitos funcionales — 35 esenciales, 17 importantes y 1 deseable.

### 5.1 Autenticación

#### RF-AUT-01 · Iniciar sesión · E
- **Actor:** Administrador, Ventas
- **Descripción:** el usuario se autentica con su correo y contraseña y recibe una sesión de vida limitada.
- **Criterios de aceptación:**
  - Con credenciales válidas y el usuario activo, se emite un JWT de 8 horas que incluye el rol y el `security_stamp`.
  - Con credenciales inválidas o un usuario inactivo, se muestra el mismo mensaje genérico ("Correo o contraseña incorrectos").
  - El usuario de sistema nunca puede iniciar sesión.
  - El correo se compara sin distinguir mayúsculas ni espacios al inicio o al final.
  - Se registra un evento de inicio de sesión, exitoso o fallido.

#### RF-AUT-02 · Cerrar sesión · E
- **Actor:** Administrador, Ventas
- **Descripción:** el usuario termina su sesión desde la interfaz.
- **Criterios de aceptación:**
  - El token se elimina del cliente y el usuario vuelve a la pantalla de inicio de sesión.
  - Después de cerrar sesión, cualquier ruta protegida redirige al inicio de sesión.

#### RF-AUT-03 · Validar la sesión en cada solicitud · E
- **Actor:** Sistema
- **Descripción:** el backend verifica en cada solicitud que la sesión siga siendo válida.
- **Criterios de aceptación:**
  - Un token vencido o inválido produce *401* y la interfaz redirige al inicio de sesión.
  - Si el `security_stamp` del token no coincide con el de la base de datos, se responde *401* de inmediato.

#### RF-AUT-04 · Controlar el acceso por rol · E
- **Actor:** Sistema
- **Descripción:** cada funcionalidad está disponible solo para los roles autorizados.
- **Criterios de aceptación:**
  - Un intento de Ventas de usar una función de Administrador produce *403*.
  - La interfaz oculta las opciones no permitidas; la autoridad es siempre el backend.
  - Todo endpoint exige autenticación salvo el de inicio de sesión.

#### RF-AUT-05 · Consultar y actualizar el perfil · I
- **Actor:** Administrador, Ventas
- **Descripción:** el usuario consulta sus datos y modifica su nombre.
- **Criterios de aceptación:**
  - Se muestran nombre, correo y rol.
  - El usuario puede cambiar su nombre (obligatorio, máximo 150 caracteres).
  - El correo y el rol solo los modifica un Administrador (RF-USR-03).
  - Se registra un evento de auditoría.

#### RF-AUT-06 · Cambiar la contraseña · E
- **Actor:** Administrador, Ventas
- **Descripción:** el usuario cambia su propia contraseña.
- **Criterios de aceptación:**
  - Se exige la contraseña actual.
  - La nueva contraseña cumple RNF-SEG-03 y es distinta de la actual.
  - Se regenera el `security_stamp`; las demás sesiones quedan invalidadas y la sesión actual recibe un token nuevo.
  - Se registra un evento de auditoría.

#### RF-AUT-07 · Cambio obligatorio de contraseña · E
- **Actor:** Administrador, Ventas
- **Descripción:** un usuario con contraseña temporal debe cambiarla antes de usar el sistema.
- **Criterios de aceptación:**
  - Aplica al primer Administrador, a los usuarios nuevos y a los usuarios con contraseña restablecida (`must_change_password = true`).
  - Mientras no la cambie, solo puede acceder al cambio de contraseña; cualquier otra solicitud produce *403*.
  - Al cambiarla, `must_change_password` pasa a `false`.

### 5.2 Usuarios

#### RF-USR-01 · Listar y buscar usuarios · I
- **Actor:** Administrador
- **Descripción:** consulta de los usuarios del sistema.
- **Criterios de aceptación:**
  - Búsqueda por nombre o correo; filtros por rol y estado.
  - Resultado paginado.
  - El usuario de sistema no aparece.

#### RF-USR-02 · Crear un usuario · I
- **Actor:** Administrador
- **Descripción:** alta de un nuevo usuario.
- **Criterios de aceptación:**
  - Nombre, correo y rol son obligatorios.
  - El correo es válido y no está en uso, aunque sea por un usuario inactivo (RN-30).
  - El Administrador define una contraseña temporal que cumple RNF-SEG-03; el usuario queda con `must_change_password = true`.
  - Se registra un evento de auditoría.

#### RF-USR-03 · Editar un usuario · I
- **Actor:** Administrador
- **Descripción:** modificación de nombre, correo y rol.
- **Criterios de aceptación:**
  - Se aplican las mismas validaciones que al crear.
  - Cambiar el rol regenera el `security_stamp`.
  - No se puede quitar el rol de Administrador al último Administrador activo (RN-31).
  - Se registra un evento de auditoría, uno específico si cambió el rol.

#### RF-USR-04 · Desactivar y reactivar un usuario · I
- **Actor:** Administrador
- **Descripción:** cambio del estado de una cuenta.
- **Criterios de aceptación:**
  - Un Administrador no puede desactivarse a sí mismo ni desactivar al último Administrador activo (RN-31).
  - Al desactivar se regenera el `security_stamp` y las sesiones del usuario dejan de funcionar de inmediato.
  - Las cotizaciones y los eventos del usuario se conservan.
  - Se registra un evento de auditoría.

#### RF-USR-05 · Restablecer la contraseña de un usuario · I
- **Actor:** Administrador
- **Descripción:** asignación de una contraseña temporal.
- **Criterios de aceptación:**
  - El Administrador define una contraseña temporal; el usuario queda con `must_change_password = true`.
  - Se regenera el `security_stamp`.
  - Se registra un evento de auditoría; la contraseña nunca aparece en el detalle.

### 5.3 Clientes

#### RF-CLI-01 · Registrar un cliente · E
- **Actor:** Ventas, Administrador
- **Descripción:** alta de un cliente.
- **Criterios de aceptación:**
  - El nombre es obligatorio (máximo 150 caracteres). Empresa, correo, teléfono y observaciones son opcionales.
  - El correo y el teléfono, si se indican, tienen formato válido. El teléfono se guarda normalizado.
  - Solo el Administrador puede marcarlo como VIP; si lo registra Ventas, nace como no VIP (RN-21).
  - Si el correo ya pertenece a otro cliente, se advierte de un posible duplicado y se permite continuar.
  - Se registra un evento de auditoría.

#### RF-CLI-02 · Buscar clientes · E
- **Actor:** Ventas, Administrador
- **Descripción:** consulta de la cartera de clientes.
- **Criterios de aceptación:**
  - Búsqueda por nombre, empresa o correo, sin distinguir mayúsculas.
  - Filtros por estado y condición VIP.
  - Resultado paginado y ordenado por nombre.

#### RF-CLI-03 · Ver el detalle de un cliente · I
- **Actor:** Ventas, Administrador
- **Descripción:** consulta de los datos y las cotizaciones de un cliente.
- **Criterios de aceptación:**
  - Se muestran todos los datos y sus cotizaciones con número, estado, total y fecha.
  - Si el cliente está activo, se puede crear una cotización desde ahí.

#### RF-CLI-04 · Editar un cliente · E
- **Actor:** Administrador
- **Descripción:** modificación de los datos de un cliente.
- **Criterios de aceptación:**
  - Se aplican las mismas validaciones que al registrar.
  - Los cambios no afectan a las cotizaciones emitidas (RN-12).
  - Se registra un evento de auditoría.

#### RF-CLI-05 · Marcar o quitar la condición VIP · E
- **Actor:** Administrador
- **Descripción:** cambio de la condición VIP de un cliente.
- **Criterios de aceptación:**
  - Se registra un evento específico con el valor anterior y el nuevo.
  - Afecta de inmediato a los borradores del cliente (RN-22), no a sus cotizaciones emitidas.

#### RF-CLI-06 · Desactivar y reactivar un cliente · I
- **Actor:** Administrador
- **Descripción:** cambio del estado de un cliente.
- **Criterios de aceptación:**
  - Un cliente inactivo no aparece en el selector de cotizaciones, no se puede usar para duplicar y bloquea la emisión de sus borradores (RN-16, RN-33).
  - Su historial sigue visible.
  - Se registra un evento de auditoría.

### 5.4 Categorías

#### RF-CAT-01 · Crear una categoría · E
- **Actor:** Administrador
- **Descripción:** alta de una categoría de productos.
- **Criterios de aceptación:**
  - El nombre es obligatorio (máximo 80 caracteres) y único sin distinguir mayúsculas (RN-30).
  - La descripción es opcional (máximo 500 caracteres).
  - Si el nombre coincide con una categoría inactiva, se sugiere reactivarla.
  - Se registra un evento de auditoría.

#### RF-CAT-02 · Listar categorías · E
- **Actor:** Administrador (gestión); todos los usuarios (como filtro del catálogo)
- **Descripción:** consulta de las categorías.
- **Criterios de aceptación:**
  - Se muestran nombre, estado y cantidad de productos.
  - Filtro por estado.

#### RF-CAT-03 · Editar una categoría · I
- **Actor:** Administrador
- **Descripción:** modificación de una categoría.
- **Criterios de aceptación:**
  - Se aplican las mismas validaciones que al crear.
  - El cambio no altera las líneas de cotizaciones emitidas.
  - Se registra un evento de auditoría.

#### RF-CAT-04 · Desactivar y reactivar una categoría · I
- **Actor:** Administrador
- **Descripción:** cambio del estado de una categoría.
- **Criterios de aceptación:**
  - Una categoría inactiva no se puede asignar a productos nuevos ni editados (RN-29).
  - Los productos que ya la tienen no cambian.
  - Se registra un evento de auditoría.

### 5.5 Productos

#### RF-PRO-01 · Crear un producto · E
- **Actor:** Administrador
- **Descripción:** alta de un producto en el catálogo.
- **Criterios de aceptación:**
  - El nombre es obligatorio (máximo 150 caracteres) y único sin distinguir mayúsculas (RN-30).
  - La categoría es obligatoria y debe estar activa (RN-29).
  - El precio es ≥ 0, con 2 decimales como máximo; se acepta 0 (RN-08).
  - La descripción es opcional (máximo 1000 caracteres).
  - Se registra un evento de auditoría.

#### RF-PRO-02 · Buscar productos · E
- **Actor:** Ventas, Administrador
- **Descripción:** consulta del catálogo.
- **Criterios de aceptación:**
  - Búsqueda por nombre; filtros por categoría y estado.
  - Resultado paginado.
  - Precio con formato de colones.

#### RF-PRO-03 · Editar un producto · E
- **Actor:** Administrador
- **Descripción:** modificación de un producto, incluido su precio.
- **Criterios de aceptación:**
  - Se aplican las mismas validaciones que al crear.
  - Cambiar el precio registra un evento específico con el precio anterior y el nuevo.
  - El cambio afecta a los borradores (RN-22) y nunca a las cotizaciones emitidas (RN-12).

#### RF-PRO-04 · Desactivar y reactivar un producto · I
- **Actor:** Administrador
- **Descripción:** cambio del estado de un producto.
- **Criterios de aceptación:**
  - Un producto inactivo no aparece en el editor de cotizaciones, bloquea la emisión de los borradores que lo contienen (RN-16) y no se copia al duplicar (RN-25).
  - Se registra un evento de auditoría.

### 5.6 Cotizaciones

#### RF-COT-01 · Crear un borrador · E
- **Actor:** Ventas, Administrador
- **Descripción:** inicio de una nueva cotización.
- **Criterios de aceptación:**
  - Exige un cliente activo (RN-33).
  - El propietario es el usuario que la crea.
  - Las condiciones comerciales se llenan con el texto por defecto de la configuración.
  - No tiene número ni vigencia.
  - Se registra un evento de auditoría.

#### RF-COT-02 · Agregar una línea · E
- **Actor:** Propietario, Administrador
- **Descripción:** incorporación de un producto al borrador.
- **Criterios de aceptación:**
  - Solo se pueden elegir productos activos.
  - La cantidad es mayor que 0, con 2 decimales como máximo (RN-07).
  - Si el producto ya está en la cotización, se suma la cantidad a la línea existente (RN-28).
  - La línea toma el precio y los datos actuales del catálogo, y el desglose se recalcula.
  - Solo se permite en borradores.

#### RF-COT-03 · Modificar la cantidad o quitar una línea · E
- **Actor:** Propietario, Administrador
- **Descripción:** ajuste de las líneas de un borrador.
- **Criterios de aceptación:**
  - La cantidad se valida igual que al agregar.
  - Al quitar una línea, las restantes se renumeran.
  - El desglose se recalcula.
  - Solo se permite en borradores.

#### RF-COT-04 · Cambiar el cliente de un borrador · I
- **Actor:** Propietario, Administrador
- **Descripción:** reasignación del cliente de un borrador.
- **Criterios de aceptación:**
  - El nuevo cliente debe estar activo.
  - El descuento VIP se recalcula según el nuevo cliente.

#### RF-COT-05 · Editar los textos comerciales · E
- **Actor:** Propietario, Administrador
- **Descripción:** redacción de introducción, resumen ejecutivo, observaciones y condiciones comerciales.
- **Criterios de aceptación:**
  - Los cuatro campos son opcionales, con un máximo de 4000 caracteres cada uno.
  - Solo se permite en borradores.

#### RF-COT-06 · Calcular y mostrar el desglose · E
- **Actor:** Sistema
- **Descripción:** el backend calcula todos los montos con la política vigente y la interfaz muestra el resultado.
- **Criterios de aceptación:**
  - Se muestran subtotal, % por tramo, % VIP, monto de descuento, base imponible, % y monto de IVA, y total.
  - En borradores, los montos se rotulan como **"Estimado"**.
  - El frontend no calcula ningún monto oficial.
  - Con la configuración inicial, el motor produce exactamente:

| Caso | Resultado esperado |
|---|---|
| Subtotal ₡500.000,00, cliente no VIP | 0 % de descuento |
| Subtotal ₡500.000,01 | 5 % |
| Subtotal ₡1.000.000,00 | 5 % |
| Subtotal ₡1.000.000,01 | 10 % |
| Subtotal ₡200.000,00, cliente VIP | 3 % |
| Subtotal ₡1.200.000,00, cliente VIP | 13 %; descuento ₡156.000,00; base ₡1.044.000,00; IVA ₡135.720,00; **total ₡1.179.720,00** |
| Todas las líneas con precio 0 | Total ₡0,00, válido |

#### RF-COT-07 · Emitir una cotización · E
- **Actor:** Propietario, Administrador
- **Descripción:** formalización de un borrador como cotización oficial.
- **Criterios de aceptación:**
  - Solo se permite desde Borrador.
  - Se rechaza si no hay líneas (RN-09), si el cliente o algún producto está inactivo (RN-16) o si hubo un conflicto de concurrencia, con un mensaje que indica la causa exacta.
  - Se recalcula por última vez con el catálogo y la configuración vigentes, y se congelan líneas, datos del cliente, condición VIP, porcentajes, tasa de IVA y totales (RN-12).
  - Se asigna el número `COT-AAAA-000001` con el año de emisión en hora de Costa Rica (RN-13).
  - La fecha de vencimiento es la fecha de emisión más los días de vigencia configurados (RN-14).
  - Si la emisión falla, no se consume ningún número y la cotización sigue siendo un borrador.
  - Emisiones simultáneas reciben números distintos y consecutivos.
  - Se registra un evento de auditoría.

#### RF-COT-08 · Registrar la aceptación · E
- **Actor:** Propietario, Administrador
- **Descripción:** registro de que el cliente aceptó la cotización.
- **Criterios de aceptación:**
  - Solo desde Emitida y dentro de la vigencia (RN-35), aunque el proceso diario no haya marcado todavía la cotización como vencida.
  - Se registran la fecha de cierre y un evento de auditoría.

#### RF-COT-09 · Registrar el rechazo · E
- **Actor:** Propietario, Administrador
- **Descripción:** registro de que el cliente rechazó la cotización.
- **Criterios de aceptación:**
  - Solo desde Emitida.
  - Motivo opcional (máximo 500 caracteres).
  - Se registran la fecha de cierre y un evento de auditoría.

#### RF-COT-10 · Cancelar una cotización · E
- **Actor:** Propietario, Administrador
- **Descripción:** anulación de un borrador o de una cotización emitida.
- **Criterios de aceptación:**
  - Se permite desde Borrador o desde Emitida.
  - Si la cotización fue emitida, el motivo es obligatorio (RN-34).
  - Una cotización cancelada desde Borrador no tiene número.
  - Se registran la fecha de cierre y un evento de auditoría.

#### RF-COT-11 · Vencer cotizaciones automáticamente · E
- **Actor:** Sistema
- **Descripción:** proceso diario que vence las cotizaciones emitidas cuya vigencia expiró.
- **Criterios de aceptación:**
  - Marca como Vencida toda cotización Emitida cuya fecha de vencimiento ya pasó en hora de Costa Rica.
  - Es idempotente: ejecutarlo varias veces no produce efectos duplicados.
  - Cada vencimiento se registra con el usuario de sistema.
  - Si el servidor estuvo detenido, la siguiente ejecución vence todo lo pendiente.

#### RF-COT-12 · Duplicar una cotización · E
- **Actor:** Ventas, Administrador
- **Descripción:** creación de un borrador nuevo a partir de una cotización existente.
- **Criterios de aceptación:**
  - Se puede duplicar desde cualquier estado.
  - La copia es un Borrador nuevo con el mismo cliente, las líneas y los cuatro textos.
  - Los precios se toman del catálogo actual y todo se recalcula con la política y la condición VIP vigentes (RN-23).
  - El propietario de la copia es quien duplica y la copia guarda la referencia a la original (RN-24).
  - La original no cambia.
  - Las líneas con productos inactivos se omiten y se informa cuáles; si el cliente está inactivo, la duplicación se rechaza (RN-25).
  - Se registra un evento de auditoría.

#### RF-COT-13 · Ver el detalle de una cotización · E
- **Actor:** Ventas, Administrador
- **Descripción:** consulta completa de una cotización.
- **Criterios de aceptación:**
  - Se muestran encabezado, líneas, textos y desglose.
  - Se muestra el historial de eventos de la cotización.
  - Si es una copia, enlaza con la cotización de origen.
  - Solo se muestran las acciones válidas según el estado y los permisos del usuario.

#### RF-COT-14 · Consultar el historial y buscar cotizaciones · E
- **Actor:** Ventas, Administrador
- **Descripción:** consulta y búsqueda de cotizaciones.
- **Criterios de aceptación:**
  - Filtros por número, cliente, estado, propietario y rango de fechas.
  - Orden predeterminado: más recientes primero.
  - Resultado paginado.
  - Ventas ve todas las cotizaciones, pero solo modifica y cambia el estado de las suyas (RN-32).

### 5.7 Documentos

#### RF-DOC-01 · Vista previa HTML · E
- **Actor:** Ventas, Administrador
- **Descripción:** visualización del documento comercial en el navegador.
- **Criterios de aceptación:**
  - Disponible en cualquier estado.
  - Incluye número, fecha de emisión, vigencia, datos de la empresa y del cliente, productos, cantidades, precios unitarios, subtotal, descuentos, IVA, total y los textos que existan (introducción, resumen ejecutivo, observaciones y condiciones comerciales).
  - Los borradores muestran la marca de agua **"BORRADOR – Sin validez comercial"** (RN-36).
  - Los montos se muestran en colones con formato de Costa Rica.

#### RF-DOC-02 · Generar el PDF · E
- **Actor:** Ventas, Administrador
- **Descripción:** descarga del documento comercial en PDF.
- **Criterios de aceptación:**
  - Solo para cotizaciones emitidas (RN-36).
  - El contenido es idéntico al de la vista previa.
  - El archivo se llama `COT-AAAA-000001.pdf`.
  - Si el estado ya no es Emitida, el documento muestra el estado de forma visible (por ejemplo, "VENCIDA" o "CANCELADA").

### 5.8 Asistente IA

#### RF-IA-01 · Generar un texto comercial · I
- **Actor:** Propietario, Administrador
- **Descripción:** Gemini propone un texto para uno de los cuatro campos comerciales.
- **Criterios de aceptación:**
  - Disponible para los cuatro tipos de texto, solo en borradores (RN-18).
  - El backend arma el contexto con datos mínimos: empresa del cliente, productos, cantidades y montos calculados por el motor. Nunca envía correos ni teléfonos.
  - El resultado se muestra como sugerencia; el usuario la acepta, la edita o la descarta. Nada se guarda hasta que la acepta.
  - Se registra un evento de auditoría con el tipo de texto, sin el contenido.

#### RF-IA-02 · Mejorar un texto existente · I
- **Actor:** Propietario, Administrador
- **Descripción:** Gemini propone una versión mejorada de un texto ya escrito.
- **Criterios de aceptación:**
  - Exige que el campo tenga texto.
  - La sugerencia se muestra junto al texto original para compararlos.
  - En lo demás, se comporta igual que RF-IA-01.

#### RF-IA-03 · Funcionar sin el asistente · E
- **Actor:** Sistema
- **Descripción:** el sistema opera completamente aunque Gemini no esté disponible.
- **Criterios de aceptación:**
  - Si Gemini no está configurado, los botones del asistente aparecen deshabilitados con una explicación.
  - Si Gemini falla, excede el tiempo límite o se alcanza el límite de uso, se muestra un mensaje claro y el borrador no se modifica.
  - Ninguna otra funcionalidad depende del asistente.

### 5.9 Configuración

#### RF-CFG-01 · Consultar la configuración · E
- **Actor:** Administrador
- **Descripción:** consulta de la configuración del sistema.
- **Criterios de aceptación:**
  - Se muestran datos de la empresa, condiciones comerciales por defecto, parámetros comerciales y vigencia.

#### RF-CFG-02 · Actualizar datos de la empresa y condiciones por defecto · E
- **Actor:** Administrador
- **Descripción:** modificación de la información institucional.
- **Criterios de aceptación:**
  - El nombre de la empresa es obligatorio.
  - Los formatos de correo y teléfono se validan igual que en los clientes.
  - Se registra un evento de auditoría con los valores anteriores y los nuevos.

#### RF-CFG-03 · Actualizar los parámetros comerciales · E
- **Actor:** Administrador
- **Descripción:** modificación de IVA, tramos de descuento, descuento VIP y vigencia.
- **Criterios de aceptación:**
  - Se pueden modificar el IVA, el umbral y el porcentaje de cada tramo, el porcentaje VIP y la vigencia.
  - Se validan las reglas de RN-27, con un mensaje específico por regla incumplida.
  - Antes de guardar, se informa cuántos borradores se recalcularán; las cotizaciones emitidas no se ven afectadas (RN-26).
  - Se registra un evento de auditoría con el valor anterior y el nuevo de cada parámetro modificado.

### 5.10 Auditoría

#### RF-AUD-01 · Registrar eventos automáticamente · E
- **Actor:** Sistema
- **Descripción:** registro de cada acción relevante del sistema.
- **Criterios de aceptación:**
  - Cada acción de la lista de eventos ([sección 10.3](#103-auditoría)) genera un evento en la misma transacción que el cambio.
  - Si el cambio falla, no queda ningún evento; si el evento falla, el cambio se deshace.
  - Los eventos no se pueden modificar ni borrar.

#### RF-AUD-02 · Consultar la auditoría · I
- **Actor:** Administrador
- **Descripción:** consulta del historial de eventos.
- **Criterios de aceptación:**
  - Filtros por usuario, tipo de evento, tipo de entidad y rango de fechas.
  - Resultado paginado, más recientes primero.
  - Solo lectura.
  - Fechas mostradas en hora de Costa Rica.

### 5.11 Dashboard

#### RF-DSH-01 · Ver los indicadores · I
- **Actor:** Administrador, Ventas
- **Descripción:** resumen numérico del estado comercial.
- **Criterios de aceptación:**
  - Total de clientes activos.
  - Total de productos activos.
  - Total de cotizaciones con número (todas menos los borradores).
  - Monto total cotizado: suma de los totales de las cotizaciones Emitidas, Aceptadas, Rechazadas y Vencidas; excluye borradores y canceladas.

#### RF-DSH-02 · Ver las cotizaciones recientes · I
- **Actor:** Administrador, Ventas
- **Descripción:** acceso rápido a las últimas cotizaciones.
- **Criterios de aceptación:**
  - Se muestran las 5 más recientes con número (o "Borrador"), cliente, estado y total.
  - Al seleccionar una se abre su detalle.

#### RF-DSH-03 · Ver la actividad reciente · D
- **Actor:** Administrador, Ventas
- **Descripción:** últimos eventos del sistema.
- **Criterios de aceptación:**
  - Se muestran los últimos 10 eventos.
  - El Administrador ve los de todos los usuarios; Ventas, solo los suyos.

---

## 6. Requisitos no funcionales

### 6.1 Seguridad

| ID | Requisito |
|---|---|
| RNF-SEG-01 | Todas las comunicaciones usan HTTPS en producción. |
| RNF-SEG-02 | Las contraseñas se almacenan solo como hash con un algoritmo adaptativo estándar. Nunca se guardan en texto plano ni aparecen en logs o en la auditoría. |
| RNF-SEG-03 | Política de contraseñas: mínimo 10 caracteres, con al menos una letra y un número. |
| RNF-SEG-04 | Los JWT duran 8 horas, se firman con una clave proveniente de variables de entorno e incluyen el `security_stamp`. No hay refresh tokens. |
| RNF-SEG-05 | Se limitan los intentos de inicio de sesión a 5 por minuto por dirección IP. |
| RNF-SEG-06 | Toda entrada se valida en el backend, aunque el frontend también la valide. |
| RNF-SEG-07 | Ningún secreto se almacena en el repositorio: cadenas de conexión, clave del JWT, clave de Gemini y credenciales del primer Administrador provienen de variables de entorno. |
| RNF-SEG-08 | Se usan dos usuarios de base de datos: propietario del esquema (solo migraciones) y usuario de aplicación con permisos mínimos. La auditoría es de solo inserción y lectura para la aplicación. |
| RNF-SEG-09 | CORS solo permite el origen del frontend. |
| RNF-SEG-10 | Los errores enviados al cliente no exponen detalles internos (trazas, SQL, rutas). |
| RNF-SEG-11 | La clave de Gemini existe solo en el backend. A Gemini se envían únicamente los datos mínimos definidos en RF-IA-01, conforme a la Ley 8968 de Protección de la Persona frente al Tratamiento de sus Datos Personales. |
| RNF-SEG-12 | El uso del asistente se limita a 30 solicitudes por hora por usuario, contabilizadas mediante los eventos de auditoría. |

### 6.2 Integridad y exactitud de datos

| ID | Requisito |
|---|---|
| RNF-DAT-01 | Los montos usan `decimal` en el backend y `numeric(18,2)` en la base de datos. Nunca se usa coma flotante. |
| RNF-DAT-02 | El motor de cálculo es la única fuente de los montos oficiales. |
| RNF-DAT-03 | Las fechas con hora se almacenan en UTC. Las reglas de negocio y la interfaz usan la zona horaria America/Costa_Rica. |
| RNF-DAT-04 | Control de concurrencia optimista. Ante un conflicto, el usuario recibe un mensaje claro y ningún cambio se sobrescribe sin que lo sepa. |
| RNF-DAT-05 | La base de datos aplica las restricciones del modelo de datos como red de seguridad: integridad, unicidad, aritmética del desglose y coherencia con el estado. |

### 6.3 Rendimiento

| ID | Requisito |
|---|---|
| RNF-REN-01 | Las operaciones comunes (listados, detalle, guardado) responden en menos de 500 ms en el percentil 95, con el volumen esperado. |
| RNF-REN-02 | El recálculo del desglose responde en menos de 300 ms. |
| RNF-REN-03 | El PDF se genera en menos de 3 segundos. |
| RNF-REN-04 | La llamada a Gemini tiene un tiempo límite de 20 segundos; la interfaz muestra el progreso y no se bloquea. |
| RNF-REN-05 | Todos los listados están paginados, con 20 elementos por defecto. |

### 6.4 Disponibilidad y resiliencia

| ID | Requisito |
|---|---|
| RNF-DIS-01 | El sistema funciona por completo aunque Gemini no esté disponible. |
| RNF-DIS-02 | El proceso de vencimiento se recupera automáticamente después de un reinicio. |
| RNF-DIS-03 | Respaldo diario automático de la base de datos, con procedimiento de restauración documentado. |

### 6.5 Usabilidad y experiencia de usuario

| ID | Requisito |
|---|---|
| RNF-USA-01 | Interfaz profesional y moderna, inspirada en Stripe, Linear, HubSpot y Notion, consistente en todas las pantallas. |
| RNF-USA-02 | Modo claro y modo oscuro: respeta la preferencia del sistema operativo y el usuario puede cambiarlo; la elección se recuerda en el navegador. |
| RNF-USA-03 | Interfaz en español. Montos con formato ₡1.179.720,00 y fechas con formato dd/mm/aaaa. |
| RNF-USA-04 | Validación de formularios mientras el usuario escribe, con mensajes junto al campo. Errores del servidor en lenguaje comprensible. |
| RNF-USA-05 | El editor de cotizaciones permite buscar productos escribiendo, agregarlos y ajustar cantidades sin ventanas modales, con el desglose actualizado al momento. |
| RNF-USA-06 | Las acciones irreversibles o destructivas (emitir, cancelar, desactivar) piden confirmación y explican sus consecuencias. |
| RNF-USA-07 | Diseño para escritorio (desde 1280 px de ancho) y usable en tablet. La versión para teléfono móvil no forma parte de la V1. |
| RNF-USA-08 | Contraste legible en ambos temas y navegación con teclado en los formularios principales. |
| RNF-USA-09 | Compatibilidad con las dos últimas versiones de Chrome, Edge y Firefox. |

### 6.6 Mantenibilidad y calidad

| ID | Requisito |
|---|---|
| RNF-MAN-01 | Monolito modular con los módulos y límites definidos en la [sección 4](#4-módulos). |
| RNF-MAN-02 | Buenas prácticas obligatorias: DTOs, inyección de dependencias, validación centralizada, TypeScript estricto y separación entre lógica y presentación. |
| RNF-MAN-03 | Los cambios de base de datos se realizan solo mediante migraciones controladas de EF Core. |
| RNF-MAN-04 | El motor de cálculo tiene pruebas unitarias que cubren como mínimo todos los casos de RF-COT-06. Las transiciones de estado y la duplicación también tienen pruebas. |
| RNF-MAN-05 | Manejo de errores consistente con respuestas *ProblemDetails*. |
| RNF-MAN-06 | Logs estructurados con niveles, que incluyen errores, llamadas a Gemini (duración y resultado, nunca el contenido) y ejecuciones del proceso de vencimiento. |

### 6.7 Despliegue y operación

| ID | Requisito |
|---|---|
| RNF-OPE-01 | Despliegue en Ubuntu Server con PostgreSQL 18. |
| RNF-OPE-02 | El primer arranque crea los datos iniciales (sección 9.7) sin intervención manual. |
| RNF-OPE-03 | El README documenta instalación, variables de entorno y ejecución. |

### 6.8 Proceso y documentación

| ID | Requisito |
|---|---|
| RNF-PRO-01 | Git desde el inicio, con la convención de commits definida y commits pequeños. |
| RNF-PRO-02 | Desarrollo por incrementos pequeños, cada uno verificable antes de continuar. |
| RNF-PRO-03 | `intent.md`, `spec.md`, `plan.md`, `README.md`, `docs/uso-ia.md`, `docs/negocio.md` y `docs/demo.md` se mantienen sincronizados con el código. |

---

## 7. Reglas de negocio

35 reglas vigentes. RN-04 está derogada y su identificador no se reutiliza.

### 7.1 Cálculo

| ID | Regla |
|---|---|
| RN-01 | Si el subtotal es mayor que el umbral 1 y menor o igual que el umbral 2, se aplica el porcentaje del tramo 1. Valores iniciales: más de ₡500.000 → 5 %. |
| RN-02 | Si el subtotal es mayor que el umbral 2, se aplica el porcentaje del tramo 2. Valores iniciales: más de ₡1.000.000 → 10 %. Los tramos son excluyentes. |
| RN-03 | Si el cliente es VIP, se suma el porcentaje VIP (inicialmente 3 puntos porcentuales), haya o no tramo. |
| ~~RN-04~~ | *Derogada.* No existe tope adicional; con la configuración inicial el descuento máximo es 13 %. |
| RN-05 | El IVA es la tasa configurada (inicialmente 13 %) aplicada sobre la base imponible. |
| RN-06 | El descuento siempre se calcula antes del IVA. |
| RN-17 | Redondeo a 2 decimales, con los valores medios alejándose de cero, en tres puntos: total de cada línea, monto de descuento y monto de IVA. |

**Algoritmo oficial del motor de cálculo**

1. Total de línea = redondear(cantidad × precio unitario).
2. Subtotal = suma de los totales de línea.
3. % por tramo según RN-01 y RN-02.
4. % VIP según RN-03.
5. % total de descuento = % por tramo + % VIP.
6. Monto de descuento = redondear(subtotal × % total ÷ 100).
7. Base imponible = subtotal − monto de descuento.
8. Monto de IVA = redondear(base imponible × % IVA ÷ 100).
9. Total = base imponible + monto de IVA.

### 7.2 Validación

| ID | Regla |
|---|---|
| RN-07 | La cantidad es mayor que 0, con 2 decimales como máximo. |
| RN-08 | El precio es mayor o igual que 0. Se permite el precio 0. |
| RN-09 | Para emitir, la cotización debe tener al menos una línea. |
| RN-16 | Para emitir, el cliente y todos los productos de las líneas deben estar activos. |
| RN-28 | Un producto no puede aparecer dos veces en la misma cotización; si se agrega de nuevo, se suma la cantidad. |
| RN-29 | Todo producto pertenece a una categoría, que debe estar activa al crear o editar el producto. |
| RN-30 | No se repiten, sin distinguir mayúsculas y aunque el registro esté inactivo: correo de usuario, nombre de categoría y nombre de producto. |
| RN-33 | No se pueden crear ni duplicar cotizaciones para un cliente inactivo. |

### 7.3 Ciclo de vida

| ID | Regla |
|---|---|
| RN-10 | Una cotización emitida no puede modificarse; solo puede cambiar de estado. |
| RN-11 | Las modificaciones posteriores a la emisión se realizan mediante duplicación. |
| RN-12 | Al emitir se congelan precios, datos de productos y categorías, datos del cliente, condición VIP, porcentajes aplicados, tasa de IVA y totales. Cambios futuros en el catálogo, los clientes o la configuración no afectan a las cotizaciones emitidas. |
| RN-13 | El consecutivo se asigna al emitir, con formato `COT-AAAA-000001`. Es único, no tiene saltos y se reinicia cada año. El año es el de la fecha de emisión en hora de Costa Rica. |
| RN-14 | La fecha de vencimiento es la fecha de emisión más los días de vigencia configurados (inicialmente 30). La cotización es válida hasta el final de ese día, en hora de Costa Rica. |
| RN-15 | Transiciones permitidas: Borrador → Emitida o Cancelada; Emitida → Aceptada, Rechazada, Vencida o Cancelada. Aceptada, Rechazada, Vencida y Cancelada son estados finales. |
| RN-22 | Un borrador usa siempre los precios, la condición VIP y los parámetros comerciales vigentes; sus montos son estimados hasta la emisión. |
| RN-34 | Cancelar una cotización emitida exige un motivo. |
| RN-35 | Una cotización solo puede aceptarse dentro de su vigencia. |

### 7.4 Duplicación

| ID | Regla |
|---|---|
| RN-23 | La copia lleva el cliente, las líneas y los textos comerciales, toma los precios actuales del catálogo y se recalcula con la política vigente. |
| RN-24 | Se puede duplicar desde cualquier estado. La original no cambia. La copia es un borrador cuyo propietario es quien duplica y guarda la referencia a la original. |
| RN-25 | Las líneas con productos inactivos no se copian y se informa al usuario. Si el cliente está inactivo, no se puede duplicar. |

### 7.5 Configuración

| ID | Regla |
|---|---|
| RN-26 | Los parámetros comerciales (IVA, umbrales y porcentajes de tramo, porcentaje VIP y vigencia) provienen de la configuración del sistema. Un cambio afecta solo a los borradores, nunca a las cotizaciones emitidas. |
| RN-27 | Validez de los parámetros: umbral 2 > umbral 1 > 0; % tramo 2 ≥ % tramo 1; % tramo 2 + % VIP ≤ 100; todos los porcentajes entre 0 y 100; vigencia entre 1 y 365 días. |

### 7.6 Gobierno, permisos y control

| ID | Regla |
|---|---|
| RN-18 | La IA solo genera o mejora los cuatro textos comerciales, solo en borradores, y su resultado requiere aceptación del usuario. Nunca interviene en precios, descuentos, impuestos ni totales. |
| RN-19 | Toda acción relevante genera un evento de auditoría en la misma transacción que el cambio. |
| RN-20 | No existe borrado físico de registros de negocio. La única excepción son las líneas de un borrador. |
| RN-21 | Solo el Administrador puede marcar o quitar la condición VIP de un cliente. |
| RN-31 | Siempre debe existir al menos un Administrador activo, y ningún usuario puede desactivarse a sí mismo. |
| RN-32 | Ventas puede ver todas las cotizaciones, pero solo puede modificar y cambiar el estado de las suyas. El Administrador puede hacerlo con todas. |
| RN-36 | El PDF solo se genera para cotizaciones emitidas. La vista previa de un borrador lleva marca de agua. |

---

## 8. Modelo de dominio

### 8.1 Subdominios

| Tipo | Módulos |
|---|---|
| **Núcleo** | Cotizaciones (incluye el motor de cálculo) |
| **Soporte** | Clientes, Categorías, Productos, Documentos, Asistente IA, Configuración, Dashboard |
| **Genérico** | Autenticación, Usuarios, Auditoría |

### 8.2 Agregados

| Agregado (raíz) | Contiene | Invariantes que protege |
|---|---|---|
| **Cotización** | Líneas, textos comerciales, datos congelados del cliente, desglose financiero, vigencia | Ciclo de vida (RN-15), inmutabilidad tras emitir (RN-10), coherencia del cálculo, producto único por línea (RN-28), al menos una línea al emitir (RN-09) |
| **Cliente** | — | Datos válidos, estado, condición VIP |
| **Categoría** | — | Nombre único, estado |
| **Producto** | — | Precio ≥ 0, categoría asignada, nombre único, estado |
| **Usuario** | — | Correo único, rol válido, al menos un Administrador activo |
| **Configuración del sistema** | Datos de la empresa, política comercial | Registro único, validez de parámetros (RN-27) |
| **Secuencia de cotización** | — | Consecutivo único y sin saltos por año |
| **Evento de auditoría** | — | Solo inserción |

**Reglas entre agregados**

- Los agregados se referencian entre sí únicamente por identificador.
- Cada caso de uso modifica un solo agregado. Excepciones deliberadas, dentro de la misma transacción: el evento de auditoría que acompaña cada cambio y la secuencia del consecutivo al emitir.

### 8.3 Entidades

| Entidad | Atributos principales | Comportamiento |
|---|---|---|
| **Usuario** | Nombre completo, correo, hash de contraseña, rol, activo, es de sistema, sello de seguridad, debe cambiar contraseña | Actualizar perfil, cambiar contraseña, cambiar rol, desactivar, reactivar, restablecer contraseña |
| **Cliente** | Nombre, empresa, correo, teléfono, es VIP, activo, observaciones | Actualizar, marcar o quitar VIP, desactivar, reactivar |
| **Categoría** | Nombre, descripción, activa | Actualizar, desactivar, reactivar |
| **Producto** | Nombre, descripción, categoría, precio, activo | Actualizar, cambiar precio, desactivar, reactivar |
| **Cotización** | Número, estado, cliente, propietario, cotización de origen, textos, datos congelados del cliente, desglose, vigencia, fecha y motivo de cierre | Agregar línea, cambiar cantidad, quitar línea, cambiar cliente, editar textos, recalcular, emitir, aceptar, rechazar, cancelar, vencer |
| **Línea de cotización** (interna) | Orden, producto, copia de nombre, descripción y categoría, precio unitario, cantidad, total de línea | Cambiar cantidad, actualizar desde el catálogo (solo en borrador) |
| **Configuración del sistema** | Datos de la empresa, condiciones por defecto, política comercial, vigencia | Actualizar datos, actualizar parámetros |
| **Secuencia de cotización** | Año, último número | Obtener el siguiente número |
| **Evento de auditoría** | Fecha, usuario, copia del nombre, tipo de evento, tipo y id de entidad, detalle | Ninguno (inmutable) |

Todas las raíces de agregado, excepto Secuencia y Evento de auditoría, incluyen la **información de auditoría** (creado por, creado en, modificado por, modificado en).

### 8.4 Value Objects

| Value Object | Contenido | Reglas |
|---|---|---|
| Dinero | Monto en colones | ≥ 0; 2 decimales; encapsula el redondeo (RN-17) |
| Cantidad | Valor decimal | > 0; máximo 2 decimales (RN-07) |
| Porcentaje | Valor decimal | Entre 0 y 100 |
| Correo electrónico | Texto | Formato válido; normalizado a minúsculas y sin espacios |
| Teléfono | Texto | 8 dígitos con prefijo `+506` opcional; normalizado |
| Número de cotización | Año y secuencia | Formato `COT-AAAA-000001` |
| Vigencia | Fecha de emisión y fecha de vencimiento | Vencimiento = emisión + N días calendario, en hora de Costa Rica |
| Textos comerciales | Introducción, resumen ejecutivo, observaciones, condiciones comerciales | Opcionales; máximo 4000 caracteres cada uno |
| Desglose financiero | Subtotal, % tramo, % VIP, % total, monto de descuento, base imponible, % IVA, monto de IVA, total | Coherente por construcción; solo lo produce el motor de cálculo |
| Datos congelados del cliente | Nombre, empresa, correo, teléfono, era VIP | Se capturan al emitir |
| Datos de la empresa | Nombre, identificación, correo, teléfono, dirección | Nombre obligatorio |
| Política comercial | Umbral y % de cada tramo, % VIP, % IVA | Cumple RN-27 |
| Información de auditoría | Creado por y en, modificado por y en | Se completa automáticamente |

### 8.5 Servicios de dominio

| Servicio | Responsabilidad |
|---|---|
| **Motor de cálculo** | Recibe las líneas, la condición VIP y la política comercial; devuelve el desglose financiero. Puro: sin acceso a base de datos ni HTTP. |
| **Generador de número de cotización** | Asigna el siguiente consecutivo del año dentro de la transacción de emisión. |
| **Reloj** | Provee la fecha y hora actuales, permitiendo probar reglas dependientes del tiempo. |

### 8.6 Enumeraciones

| Enumeración | Valores (dominio y base de datos) |
|---|---|
| Rol | `Administrator` (Administrador), `Sales` (Ventas) |
| Estado de la cotización | `Draft` (Borrador), `Issued` (Emitida), `Accepted` (Aceptada), `Rejected` (Rechazada), `Expired` (Vencida), `Cancelled` (Cancelada) |
| Tipo de texto comercial | Introducción, Resumen ejecutivo, Observaciones, Condiciones comerciales |
| Acción del asistente | Generar, Mejorar |
| Tipo de entidad auditada | `User`, `Customer`, `Category`, `Product`, `Quote`, `SystemSettings`, `Session` |
| Tipo de evento | Ver la [sección 10.3](#103-auditoría) |

El estado activo o inactivo de usuarios, clientes, categorías y productos se representa como un valor booleano.

### 8.7 Máquina de estados de la cotización

| Desde | Hacia | Acción | Actor | Condiciones |
|---|---|---|---|---|
| — | Borrador | Crear o duplicar | Ventas, Administrador | Cliente activo |
| Borrador | Borrador | Editar | Propietario, Administrador | — |
| Borrador | Emitida | Emitir | Propietario, Administrador | RN-09, RN-16 |
| Borrador | Cancelada | Cancelar | Propietario, Administrador | Motivo opcional |
| Emitida | Aceptada | Registrar aceptación | Propietario, Administrador | Dentro de la vigencia (RN-35) |
| Emitida | Rechazada | Registrar rechazo | Propietario, Administrador | Motivo opcional |
| Emitida | Vencida | Vencer | Sistema | Fecha actual posterior a la fecha de vencimiento |
| Emitida | Cancelada | Cancelar | Propietario, Administrador | Motivo obligatorio (RN-34) |

---

## 9. Modelo de datos

### 9.1 Convenciones

- Tablas y columnas en inglés, en `snake_case`; tablas en plural.
- Llave primaria `id`; llaves foráneas `<entidad>_id`.
- Prefijos de restricciones: `pk_`, `fk_`, `uq_`, `ck_`, `ix_`.
- Identificadores `uuid` (versión 7, `uuidv7()`); `audit_events` usa `bigint` autoincremental.
- Montos `numeric(18,2)`; porcentajes `numeric(5,2)`; cantidades `numeric(12,2)`.
- Textos `varchar(n)`; fechas con hora `timestamptz` (UTC); fechas de negocio `date`.
- Enumeraciones como `varchar` con restricción `CHECK`.
- Sin extensiones adicionales.

### 9.2 Campos de auditoría comunes

Presentes en `users`, `customers`, `categories`, `products`, `quotes` y `system_settings`:

| Campo | Tipo | Nulo | Notas |
|---|---|---|---|
| `created_at` | timestamptz | No | |
| `created_by` | uuid | No | FK → `users` |
| `updated_at` | timestamptz | No | Igual a `created_at` al crear |
| `updated_by` | uuid | No | FK → `users` |

### 9.3 Entidades

#### `users`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `id` | uuid | No | PK |
| `full_name` | varchar(150) | No | No vacío |
| `email` | varchar(254) | No | Único; en minúsculas |
| `password_hash` | varchar(255) | Sí | Nulo solo si `is_system` |
| `role` | varchar(20) | No | `Administrator`, `Sales` |
| `is_active` | boolean | No | Por defecto `true` |
| `is_system` | boolean | No | Por defecto `false`; único usuario de sistema (índice único parcial) |
| `security_stamp` | uuid | No | Se regenera al cambiar contraseña o rol, o al desactivar |
| `must_change_password` | boolean | No | Por defecto `false` |

#### `customers`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `id` | uuid | No | PK |
| `name` | varchar(150) | No | No vacío |
| `company` | varchar(150) | Sí | |
| `email` | varchar(254) | Sí | En minúsculas; no único |
| `phone` | varchar(13) | Sí | 8 dígitos con `+506` opcional |
| `is_vip` | boolean | No | Por defecto `false` |
| `is_active` | boolean | No | Por defecto `true` |
| `notes` | varchar(1000) | Sí | |

#### `categories`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `id` | uuid | No | PK |
| `name` | varchar(80) | No | No vacío; único sobre `lower(name)` |
| `description` | varchar(500) | Sí | |
| `is_active` | boolean | No | Por defecto `true` |

#### `products`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `id` | uuid | No | PK |
| `name` | varchar(150) | No | No vacío; único sobre `lower(name)` |
| `description` | varchar(1000) | Sí | |
| `category_id` | uuid | No | FK → `categories` |
| `price` | numeric(18,2) | No | ≥ 0 |
| `is_active` | boolean | No | Por defecto `true` |

#### `system_settings` (registro único)
| Campo | Tipo | Nulo | Por defecto | Restricciones |
|---|---|---|---|---|
| `id` | smallint | No | 1 | PK; `id = 1` |
| `company_name` | varchar(150) | No | — | No vacío |
| `company_legal_id` | varchar(20) | Sí | — | |
| `company_email` | varchar(254) | Sí | — | |
| `company_phone` | varchar(13) | Sí | — | |
| `company_address` | varchar(300) | Sí | — | |
| `vat_rate` | numeric(5,2) | No | 13.00 | 0–100 |
| `tier1_threshold` | numeric(18,2) | No | 500000.00 | > 0 |
| `tier1_discount_rate` | numeric(5,2) | No | 5.00 | 0–100 |
| `tier2_threshold` | numeric(18,2) | No | 1000000.00 | > `tier1_threshold` |
| `tier2_discount_rate` | numeric(5,2) | No | 10.00 | 0–100; ≥ `tier1_discount_rate` |
| `vip_discount_rate` | numeric(5,2) | No | 3.00 | 0–100 |
| `validity_days` | smallint | No | 30 | 1–365 |
| `default_commercial_terms` | varchar(4000) | Sí | — | |

Restricción adicional: `tier2_discount_rate + vip_discount_rate <= 100`.

#### `quotes`
| Grupo | Campos |
|---|---|
| Identidad | `id` uuid PK; `number` varchar(15) único, formato `^COT-[0-9]{4}-[0-9]{6}$`, nulo en borrador |
| Relaciones | `customer_id` (FK `customers`); `owner_id` (FK `users`); `source_quote_id` (FK `quotes`, nulo) |
| Estado | `status` varchar(20): `Draft`, `Issued`, `Accepted`, `Rejected`, `Expired`, `Cancelled` |
| Textos | `introduction`, `executive_summary`, `notes`, `commercial_terms`: varchar(4000), nulos |
| Cliente congelado | `customer_name`, `customer_company`, `customer_email`, `customer_phone`, `customer_is_vip`: nulos hasta emitir |
| Desglose | `subtotal`, `discount_amount`, `taxable_base`, `vat_amount`, `total`: numeric(18,2); `tier_discount_rate`, `vip_discount_rate`, `discount_rate`, `vat_rate`: numeric(5,2). Todos obligatorios, por defecto 0 |
| Ciclo de vida | `issued_at` timestamptz; `valid_until` date; `closed_at` timestamptz; `close_reason` varchar(500) |

Restricciones de `quotes`:

- **Aritmética:** montos ≥ 0; `discount_rate = tier_discount_rate + vip_discount_rate` y ≤ 100; `discount_amount ≤ subtotal`; `taxable_base = subtotal − discount_amount`; `total = taxable_base + vat_amount`.
- **Borrador:** `number`, `issued_at`, `valid_until` y `closed_at` nulos.
- **Emitida, Aceptada, Rechazada y Vencida:** `number`, `issued_at`, `valid_until`, `customer_name` y `customer_is_vip` obligatorios.
- **Cancelada:** `number` e `issued_at` son ambos nulos o ambos no nulos.
- **Estados finales:** `closed_at` obligatorio; en Borrador y Emitida, nulo.
- **Cancelada después de emitir:** `close_reason` obligatorio.

En los borradores, el desglose contiene el último cálculo estimado; se recalcula en cada modificación y se congela al emitir.

#### `quote_lines`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `id` | uuid | No | PK |
| `quote_id` | uuid | No | FK → `quotes`, borrado en cascada |
| `product_id` | uuid | No | FK → `products` |
| `line_number` | smallint | No | > 0 |
| `product_name` | varchar(150) | No | Copia |
| `product_description` | varchar(1000) | Sí | Copia |
| `category_name` | varchar(80) | No | Copia |
| `unit_price` | numeric(18,2) | No | ≥ 0 |
| `quantity` | numeric(12,2) | No | > 0 |
| `line_total` | numeric(18,2) | No | `= round(unit_price × quantity, 2)` |

Únicos: (`quote_id`, `product_id`) y (`quote_id`, `line_number`), este último con verificación diferida al final de la transacción.

#### `quote_number_sequences`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `year` | smallint | No | PK; 2000–9999 |
| `last_number` | integer | No | 0–999999; por defecto 0 |

Al emitir, la fila del año se bloquea dentro de la transacción; si es la primera emisión del año, se crea.

#### `audit_events`
| Campo | Tipo | Nulo | Restricciones |
|---|---|---|---|
| `id` | bigint | No | PK autoincremental |
| `occurred_at` | timestamptz | No | Por defecto `now()` |
| `user_id` | uuid | Sí | FK → `users`; nulo solo en inicio de sesión fallido |
| `user_name` | varchar(150) | Sí | Copia del nombre |
| `event_type` | varchar(50) | No | Validado en la aplicación |
| `entity_type` | varchar(30) | No | `User`, `Customer`, `Category`, `Product`, `Quote`, `SystemSettings`, `Session` |
| `entity_id` | uuid | Sí | Sin FK; nulo para `SystemSettings` y `Session` |
| `detail` | varchar(2000) | No | Descripción legible |

### 9.4 Relaciones

| Relación | Cardinalidad | Al borrar el padre |
|---|---|---|
| `products.category_id` → `categories` | N:1 obligatoria | Restringido |
| `quotes.customer_id` → `customers` | N:1 obligatoria | Restringido |
| `quotes.owner_id` → `users` | N:1 obligatoria | Restringido |
| `quotes.source_quote_id` → `quotes` | N:0..1 | Restringido |
| `quote_lines.quote_id` → `quotes` | N:1 composición | Cascada |
| `quote_lines.product_id` → `products` | N:1 obligatoria | Restringido |
| `audit_events.user_id` → `users` | N:0..1 | Restringido |
| `*.created_by`, `*.updated_by` → `users` | N:1 obligatoria | Restringido |

### 9.5 Índices

| Tabla | Únicos | Índices adicionales |
|---|---|---|
| `users` | `email`; usuario de sistema único (parcial) | — |
| `categories` | `lower(name)` | — |
| `products` | `lower(name)` | `category_id` |
| `quotes` | `number` | `customer_id`; `owner_id`; `valid_until` parcial donde `status = 'Issued'` |
| `quote_lines` | (`quote_id`, `product_id`); (`quote_id`, `line_number`) diferido | — |
| `audit_events` | — | (`entity_type`, `entity_id`, `occurred_at`); `occurred_at` desc.; (`user_id`, `occurred_at` desc.) |

No se crean índices de búsqueda de texto ni índices sobre columnas de baja selectividad. Se reevaluarán con planes de ejecución reales si alguna tabla supera aproximadamente 10.000 filas.

### 9.6 Estrategias transversales

**Borrado lógico**

| Tabla | Estrategia |
|---|---|
| `users`, `customers`, `categories`, `products` | `is_active = false`. Reversible; el registro sigue visible en el historial. Los inactivos se excluyen explícitamente solo en los selectores de creación y edición; no hay filtros globales. |
| `quotes` | Nunca se borran; se usa el estado `Cancelled`. |
| `quote_lines` | Borrado físico solo al quitar una línea de un borrador. |
| `audit_events` | Inmutable. |

Las restricciones de unicidad aplican también a los registros inactivos.

**Concurrencia:** control optimista mediante la columna de sistema `xmin` en `users`, `customers`, `categories`, `products`, `quotes` y `system_settings`.

**Permisos del usuario de aplicación:** no puede modificar la estructura; en `audit_events` solo inserta y lee; solo puede borrar en `quote_lines`.

**Validación compartida:** la base de datos rechaza los estados imposibles (tipos, unicidad, integridad referencial, aritmética, coherencia con el estado). El dominio decide las acciones permitidas (transiciones, permisos, reglas entre agregados). No se usan triggers para lógica de negocio.

### 9.7 Datos iniciales

1. Usuario de sistema.
2. Primer Administrador, con correo y contraseña provenientes de variables de entorno y `must_change_password = true`.
3. Categorías: Equipo tecnológico, Licencia de software, Servicio profesional, Solución empresarial.
4. Configuración con los valores por defecto y los datos de TechSolutions CR.

Los datos de demostración (clientes, productos y cotizaciones de ejemplo) se cargan solo en el entorno de desarrollo.

---

## 10. Seguridad

### 10.1 JWT

- Autenticación con JWT de vida limitada: **8 horas**, sin refresh tokens.
- El token incluye el identificador del usuario, el rol y el `security_stamp`.
- Firmado con una clave secreta proveniente de variables de entorno.
- En cada solicitud se verifica que el `security_stamp` del token coincida con el de la base de datos. Esto invalida de inmediato las sesiones de usuarios desactivados, con rol cambiado o con contraseña cambiada o restablecida.
- El cierre de sesión elimina el token del cliente.
- Los intentos de inicio de sesión se limitan a 5 por minuto por IP.

### 10.2 Roles y permisos

| Capacidad | Administrador | Ventas |
|---|---|---|
| Perfil propio y cambio de contraseña | ✓ | ✓ |
| Gestión de usuarios | ✓ | — |
| Registrar clientes | ✓ | ✓ (sin VIP) |
| Consultar clientes | ✓ | ✓ |
| Editar, cambiar VIP, desactivar y reactivar clientes | ✓ | — |
| Gestión de categorías y productos | ✓ | — |
| Consultar categorías y productos | ✓ | ✓ |
| Ver todas las cotizaciones | ✓ | ✓ |
| Crear y duplicar cotizaciones | ✓ | ✓ |
| Editar, emitir y cambiar el estado de cotizaciones | Todas | Solo las propias |
| Vista previa y PDF | ✓ | ✓ |
| Asistente IA | Todas las cotizaciones en borrador | Solo borradores propios |
| Configuración del sistema | ✓ | — |
| Consulta de auditoría | ✓ | — |
| Dashboard | Datos globales | Indicadores globales; actividad propia |

La autorización se aplica siempre en el backend mediante políticas por rol y verificaciones de propiedad.

### 10.3 Auditoría

La auditoría opera en dos niveles:

1. **Campos de auditoría** en cada tabla principal (sección 9.2), completados automáticamente en la capa de persistencia.
2. **Historial de eventos** en `audit_events`, registrado de forma explícita por cada caso de uso en la misma transacción que el cambio, e inmutable para la aplicación.

**Tipos de evento**

| Área | Eventos |
|---|---|
| Sesión | Inicio de sesión, inicio de sesión fallido |
| Usuario | Creado, actualizado, rol cambiado, desactivado, reactivado, contraseña cambiada, contraseña restablecida |
| Cliente | Creado, actualizado, condición VIP cambiada, desactivado, reactivado |
| Categoría | Creada, actualizada, desactivada, reactivada |
| Producto | Creado, actualizado, precio cambiado, desactivado, reactivado |
| Cotización | Creada, duplicada, emitida, aceptada, rechazada, cancelada, vencida, texto generado con IA |
| Configuración | Actualizada (con valores anteriores y nuevos) |

Las ediciones de borradores no se auditan individualmente. Los vencimientos se registran con el usuario de sistema. El detalle de un evento nunca contiene contraseñas ni el texto generado por la IA.

### 10.4 Protección de datos

- Validación de toda entrada en el backend y sanitización básica de textos.
- Contraseñas almacenadas solo como hash adaptativo.
- Secretos y cadenas de conexión exclusivamente en variables de entorno; nunca en el repositorio.
- HTTPS en producción; CORS restringido al origen del frontend.
- Errores sin detalles internos.
- Usuarios de base de datos separados con privilegios mínimos.
- Identificadores no secuenciales en las URL.
- Minimización de datos personales enviados a terceros, conforme a la Ley 8968.
- Respaldo diario de la base de datos.

### 10.5 Gemini

- La clave de API solo existe en el backend, en variables de entorno.
- Al servicio solo se envían: nombre de la empresa del cliente, productos, cantidades, montos calculados y, al mejorar, el texto del usuario. Nunca correos ni teléfonos.
- Límite de 30 solicitudes por hora por usuario.
- El contenido generado no se registra en los logs ni en la auditoría.
- La salida se trata como texto sin formato y sus longitudes de entrada y salida están limitadas.

---

## 11. Integración con inteligencia artificial

### 11.1 Alcance

Google Gemini actúa como **asistente de redacción comercial**. Puede:

- Generar observaciones comerciales.
- Generar introducciones profesionales.
- Generar resúmenes ejecutivos.
- Generar condiciones comerciales.
- Mejorar la redacción de cualquiera de esos textos.

Las cinco capacidades se implementan como una sola operación parametrizada por **tipo de texto** (introducción, resumen ejecutivo, observaciones, condiciones comerciales) y **acción** (generar, mejorar).

### 11.2 Restricciones

La IA **no puede**:

- Modificar precios.
- Calcular descuentos.
- Modificar impuestos.
- Alterar totales.
- Tomar decisiones financieras.

Además:

- Todos los cálculos oficiales se realizan exclusivamente en el motor de cálculo del backend. Si un texto menciona montos, estos provienen del motor, no del modelo.
- La IA solo actúa sobre cotizaciones en Borrador.
- Ningún texto generado se guarda sin aceptación explícita del usuario.
- La aplicación funciona completamente aunque Gemini no esté disponible.

### 11.3 Flujo de uso

1. El usuario, en un borrador, elige un campo de texto y la acción (generar o mejorar).
2. El frontend envía la solicitud al backend.
3. El backend valida permisos, estado del borrador y límite de uso.
4. El backend arma el contexto con los datos mínimos permitidos y el desglose calculado por el motor.
5. El backend llama a Gemini con el tiempo límite configurado.
6. El backend devuelve la sugerencia y registra el evento de auditoría.
7. El frontend muestra la sugerencia (junto al texto original si es una mejora).
8. El usuario acepta, edita o descarta. Solo al aceptar se guarda el texto en el borrador.
9. Al emitir, el texto queda congelado junto con el resto de la cotización.

### 11.4 Manejo de errores

| Situación | Comportamiento |
|---|---|
| Gemini no configurado | Funciones del asistente deshabilitadas con una explicación; el resto del sistema funciona |
| Error del servicio o respuesta inválida | Mensaje claro al usuario; el borrador no cambia; se registra en el log |
| Tiempo límite excedido (20 s) | Mensaje claro; el usuario puede reintentar |
| Límite de uso alcanzado | Mensaje indicando cuándo podrá volver a usarlo |
| Cotización no está en Borrador o sin permisos | Solicitud rechazada con el error correspondiente |

En ningún caso un fallo del asistente impide guardar, emitir o consultar cotizaciones.

---

## 12. Restricciones técnicas

### 12.1 Tecnologías obligatorias

| Capa | Tecnología |
|---|---|
| Frontend | React, TypeScript, Vite |
| Backend | ASP.NET Core, Entity Framework Core |
| Base de datos | PostgreSQL 18 |
| Autenticación | JWT |
| IA | Google Gemini API |
| API | REST |
| Infraestructura | Ubuntu Server |
| Control de versiones | Git |

### 12.2 Restricciones arquitectónicas

- El sistema se implementa como un **monolito modular** bien organizado.
- No se utilizan:
  - Microservicios.
  - CQRS complejo.
  - Event Sourcing.
  - Arquitecturas distribuidas.
  - Dependencias innecesarias.
- Toda dependencia nueva debe justificar claramente su valor.
- Se prioriza simplicidad, mantenibilidad, legibilidad y consistencia; se evita la sobreingeniería.
- El diseño debe permitir la expansión futura (sección 13) sin reescribir el sistema.

### 12.3 Exclusiones técnicas

- Sin refresh tokens en la V1.
- Sin triggers ni procedimientos almacenados para lógica de negocio.
- Sin extensiones de PostgreSQL.
- Sin almacenamiento de archivos binarios en la base de datos; el logo de la empresa es un recurso estático de la aplicación.
- Sin llamadas a Gemini desde el frontend.

---

## 13. Fuera de alcance

Las siguientes funcionalidades no forman parte de la V1. El diseño debe permitir incorporarlas sin reescribir el sistema.

**Evoluciones futuras definidas en el contexto**

- Multiempresa.
- Multimoneda.
- Integraciones con ERP.
- Facturación electrónica.
- Portal de clientes.
- CRM.
- Integraciones bancarias.
- Automatizaciones avanzadas.

**Excluidas durante el diseño**

- Refresh tokens y sesiones revocables más allá del `security_stamp`.
- Recuperación de contraseña por correo electrónico.
- Envío de cotizaciones por correo desde el sistema.
- Flujo de aprobación interna de cotizaciones o descuentos.
- Descuentos manuales o por línea.
- Listas de precios por cliente.
- IVA diferenciado por producto, exenciones o tarifas reducidas.
- Versionado formal de cotizaciones (se usa la duplicación).
- Reordenamiento manual de líneas.
- Identificación y dirección del cliente; unidad de medida y código de producto.
- Logo de la empresa administrable desde la interfaz.
- Historial de versiones de la configuración (los cambios quedan en la auditoría).
- Depuración o archivado de la auditoría.
- Versión para teléfono móvil.
- Interfaz en otros idiomas.
- Importación o exportación masiva de datos y reportes avanzados.
- Notificaciones.

---

## 14. Criterios de aceptación del producto

La V1 se considera completada cuando se cumplen **todos** los criterios siguientes.

### 14.1 Funcionalidad

1. Todos los requisitos funcionales de prioridad **Esencial** e **Importante** (52 de 53) cumplen sus criterios de aceptación. RF-DSH-03 (Deseable) es opcional.
2. El **flujo principal de punta a punta** funciona sin intervención técnica:
   1. Un Administrador inicia sesión, configura la empresa y crea categorías y productos.
   2. Un usuario de Ventas registra un cliente.
   3. Crea un borrador, agrega productos y observa el desglose actualizado al instante.
   4. Genera y acepta textos con el asistente IA.
   5. Ve la vista previa con marca de agua.
   6. Emite la cotización y obtiene un número `COT-AAAA-000001`.
   7. Descarga el PDF.
   8. Registra la aceptación o el rechazo.
   9. Duplica la cotización y comprueba que la copia toma los precios actuales.
   10. Consulta el historial, el detalle con sus eventos y el dashboard.
3. Las 35 reglas de negocio vigentes se cumplen.
4. Cambiar un precio, la condición VIP o un parámetro comercial **no altera** ninguna cotización emitida.
5. Una cotización emitida no puede modificarse por ningún medio de la interfaz ni de la API.
6. El proceso de vencimiento marca correctamente las cotizaciones vencidas.

### 14.2 Calidad

7. El motor de cálculo tiene pruebas unitarias que cubren todos los casos de RF-COT-06 y todas pasan.
8. Las transiciones de estado y la duplicación tienen pruebas y todas pasan.
9. Las restricciones de la base de datos rechazan datos inválidos aunque se omita la validación de la aplicación.
10. Los errores se presentan de forma consistente y comprensible.

### 14.3 Seguridad

11. Ningún endpoint, salvo el inicio de sesión, es accesible sin autenticación.
12. Ventas no puede ejecutar funciones de Administrador ni modificar cotizaciones ajenas.
13. Un usuario desactivado pierde el acceso de inmediato.
14. El repositorio no contiene secretos ni credenciales.
15. La auditoría registra todas las acciones definidas y no puede modificarse desde la aplicación.

### 14.4 Resiliencia

16. Con Gemini sin configurar o fuera de servicio, todo el flujo principal (salvo la redacción asistida) funciona normalmente.

### 14.5 Experiencia de usuario

17. La interfaz es consistente, profesional y funciona en modo claro y oscuro.
18. El editor de cotizaciones permite completar una cotización sin ventanas modales.
19. Los objetivos de rendimiento de la sección 6.3 se cumplen con el volumen de demostración.

### 14.6 Despliegue y documentación

20. El sistema se despliega en Ubuntu Server con PostgreSQL 18 siguiendo el README.
21. El primer arranque crea los datos iniciales.
22. Existen datos de demostración que permiten ejecutar el guion de `docs/demo.md`.
23. `intent.md`, `spec.md`, `plan.md`, `README.md`, `docs/uso-ia.md`, `docs/negocio.md` y `docs/demo.md` están completos y sincronizados con el código.
24. El historial de Git sigue la convención de commits definida.

---

## Anexo A. Glosario

| Término | Definición |
|---|---|
| Cotización | Propuesta comercial a un cliente con líneas, cálculos, textos y ciclo de vida. |
| Borrador | Cotización en preparación: editable, sin número y con montos estimados. |
| Emitir | Formalizar una cotización: asignar número, fijar vigencia y congelar su contenido. |
| Línea | Producto cotizado con cantidad, precio unitario y total. |
| Subtotal | Suma de los totales de línea, antes de descuentos e impuestos. |
| Descuento por tramo | Porcentaje aplicado según el subtotal (RN-01, RN-02). |
| Descuento VIP | Porcentaje adicional para clientes VIP (RN-03). |
| Base imponible | Subtotal menos el descuento total. |
| Congelar | Copiar en la cotización los datos vigentes al emitir, para que cambios futuros no la afecten. |
| Duplicar | Crear un borrador nuevo a partir de una cotización existente. |
| Consecutivo | Número oficial `COT-AAAA-000001`. |
| Vigencia | Período durante el cual una cotización emitida puede ser aceptada. |
| Propietario | Usuario responsable de una cotización: quien la creó o la duplicó. |
| Evento de auditoría | Registro inmutable de una acción relevante. |
| Sello de seguridad (`security_stamp`) | Valor que cambia ante eventos de seguridad y permite invalidar sesiones. |
| Política comercial | Conjunto de parámetros de cálculo: umbrales, porcentajes de descuento e IVA. |

## Anexo B. Casos de uso

| ID | Caso de uso | Actor principal | Requisitos funcionales | Reglas de negocio |
|---|---|---|---|---|
| CU-01 | Iniciar sesión | Administrador, Ventas | RF-AUT-01, RF-AUT-03, RF-AUT-07 | — |
| CU-02 | Cerrar sesión | Administrador, Ventas | RF-AUT-02 | — |
| CU-03 | Gestionar el perfil y la contraseña | Administrador, Ventas | RF-AUT-05, RF-AUT-06 | — |
| CU-04 | Administrar usuarios | Administrador | RF-USR-01 a RF-USR-05 | RN-30, RN-31 |
| CU-05 | Registrar un cliente | Ventas, Administrador | RF-CLI-01 | RN-21 |
| CU-06 | Consultar clientes | Ventas, Administrador | RF-CLI-02, RF-CLI-03 | — |
| CU-07 | Mantener clientes | Administrador | RF-CLI-04, RF-CLI-05, RF-CLI-06 | RN-21, RN-22, RN-33 |
| CU-08 | Administrar categorías | Administrador | RF-CAT-01 a RF-CAT-04 | RN-29, RN-30 |
| CU-09 | Administrar productos | Administrador | RF-PRO-01, RF-PRO-03, RF-PRO-04 | RN-08, RN-22, RN-29, RN-30 |
| CU-10 | Consultar el catálogo | Ventas, Administrador | RF-PRO-02 | — |
| CU-11 | Preparar un borrador | Ventas, Administrador | RF-COT-01 a RF-COT-06 | RN-01 a RN-03, RN-05 a RN-07, RN-17, RN-22, RN-28, RN-33 |
| CU-12 | Redactar con el asistente | Propietario, Administrador | RF-IA-01 a RF-IA-03 | RN-18 |
| CU-13 | Emitir una cotización | Propietario, Administrador | RF-COT-07 | RN-09, RN-12, RN-13, RN-14, RN-16 |
| CU-14 | Registrar la respuesta del cliente | Propietario, Administrador | RF-COT-08, RF-COT-09 | RN-15, RN-35 |
| CU-15 | Cancelar una cotización | Propietario, Administrador | RF-COT-10 | RN-15, RN-34 |
| CU-16 | Duplicar una cotización | Ventas, Administrador | RF-COT-12 | RN-11, RN-23, RN-24, RN-25 |
| CU-17 | Vencer cotizaciones | Sistema | RF-COT-11 | RN-14, RN-15 |
| CU-18 | Consultar cotizaciones | Ventas, Administrador | RF-COT-13, RF-COT-14 | RN-32 |
| CU-19 | Generar el documento comercial | Ventas, Administrador | RF-DOC-01, RF-DOC-02 | RN-36 |
| CU-20 | Configurar el sistema | Administrador | RF-CFG-01 a RF-CFG-03 | RN-26, RN-27 |
| CU-21 | Consultar la auditoría | Administrador | RF-AUD-02 | — |
| CU-22 | Ver el dashboard | Administrador, Ventas | RF-DSH-01 a RF-DSH-03 | — |
| — | Registrar eventos (transversal) | Sistema | RF-AUD-01 | RN-19 |

Los casos CU-11, CU-12, CU-13, CU-16 y CU-19 conforman el flujo de valor principal del producto.