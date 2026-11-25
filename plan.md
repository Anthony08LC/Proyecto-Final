# QuoteFlow AI — Plan de Implementación

| Campo | Valor |
|---|---|
| Documento | Plan oficial de implementación (`plan.md`) |
| Producto | QuoteFlow AI — Versión 1 |
| Versión del documento | 1.0 |
| Fecha | 18 de noviembre de 2026 |
| Fuente de verdad funcional | `spec.md` v1.0 |
| Estado | Aprobado — guía oficial del desarrollo |

> Este plan describe **cómo y en qué orden** se construye lo que define `spec.md`. No agrega ni quita alcance. Si durante el desarrollo surge una necesidad nueva, primero se actualiza `spec.md` y después este plan.

---

## Tabla de contenido

1. [Objetivo del plan](#1-objetivo-del-plan)
2. [Estrategia general de implementación](#2-estrategia-general-de-implementación)
3. [Orden de desarrollo](#3-orden-de-desarrollo)
4. [Épicas](#4-épicas)
5. [Fases](#5-fases)
6. [Backlog de implementación](#6-backlog-de-implementación)
7. [Estrategia Git](#7-estrategia-git)
8. [Estrategia de base de datos](#8-estrategia-de-base-de-datos)
9. [Estrategia de pruebas](#9-estrategia-de-pruebas)
10. [Riesgos](#10-riesgos)
11. [Definición de completado](#11-definición-de-completado)
12. [Roadmap final](#12-roadmap-final)
13. [Estado de la entrega v1.0.0 y pendientes](#13-estado-de-la-entrega-v100-y-pendientes)
- [Anexo A. Registro de decisiones técnicas](#anexo-a-registro-de-decisiones-técnicas)
- [Anexo B. Trazabilidad de requisitos a tareas](#anexo-b-trazabilidad-de-requisitos-a-tareas)

---

## 1. Objetivo del plan

Guiar la construcción completa de QuoteFlow AI V1, desde el primer commit hasta la entrega, de forma que:

1. Cada requisito de `spec.md` quede implementado, probado y trazado a una tarea.
2. El sistema avance en **incrementos pequeños, verificables y demostrables**.
3. El flujo de valor principal (crear, calcular, emitir, documentar y duplicar cotizaciones) esté funcionando **lo antes posible** y se mantenga funcionando.
4. Los riesgos técnicos se enfrenten temprano, cuando corregirlos es barato.
5. La documentación se mantenga sincronizada con el código durante todo el proyecto.

---

## 2. Estrategia general de implementación

### 2.1 Desarrollo incremental por porciones verticales

Cada funcionalidad se construye como una **porción vertical completa**: base de datos → dominio → API → pruebas → interfaz. Una tarea no se considera terminada hasta que se puede probar de punta a punta.

Ventajas frente a construir capa por capa (toda la base de datos, luego todo el backend, luego todo el frontend):

- Cada incremento se puede demostrar y validar contra la especificación.
- Los errores de integración aparecen en días, no al final.
- Si el tiempo se agota, lo construido funciona; no quedan capas a medias.

### 2.2 Por módulos, respetando los límites del monolito modular

El trabajo se organiza en épicas que coinciden con los módulos de `spec.md`. Cada módulo es dueño de sus datos y se comunica con otros solo mediante interfaces de consulta. Al terminar un módulo, sus límites ya están establecidos y los siguientes se apoyan en ellos sin modificarlos.

### 2.3 Lo transversal primero, una sola vez

Hay piezas que todos los módulos usan: manejo de errores, logging, autenticación, autorización, campos de auditoría, registro de eventos, concurrencia optimista, reloj y formatos. Se construyen **antes** de los módulos de negocio, para que cada módulo nuevo las reciba ya resueltas y no haya implementaciones duplicadas o inconsistentes.

### 2.4 El núcleo con máxima calidad

El motor de cálculo y el agregado Cotización son el valor del producto. Se diseñan con pruebas desde el inicio: los casos críticos de la [sección 9.4](#94-casos-críticos-del-motor-de-cálculo) se escriben **antes** de considerar terminado el motor.

### 2.5 Reglas de trabajo

- Una tarea del backlog = una rama = uno o varios commits pequeños.
- Nada se integra a `main` si no compila o si alguna prueba falla.
- Al terminar cada tarea: explicar cambios, requisitos cubiertos, cómo probar y riesgos (según las instrucciones del proyecto).
- Ninguna tarea implementa funcionalidad que no esté en `spec.md`.

---

## 3. Orden de desarrollo

| # | Bloque | Justificación |
|---|---|---|
| 1 | **Infraestructura y fundaciones** | Sin proyecto, base de datos, manejo de errores y pruebas configuradas, nada más se puede verificar. Además, valida pronto el riesgo de compatibilidad de PostgreSQL 18 con EF Core. |
| 2 | **Usuarios y autenticación** | Todas las tablas tienen `created_by` y `updated_by`, que son llaves foráneas a `users`. Todo endpoint exige autenticación. Es una dependencia física y de seguridad de todo lo demás. |
| 3 | **Auditoría (registro) y concurrencia** | Cada acción de cada módulo registra un evento en la misma transacción. Si el mecanismo existe antes de los módulos de negocio, cada módulo lo usa desde su primera línea y no hay que volver atrás. La *consulta* de auditoría se deja para más adelante. |
| 4 | **Configuración** | El motor de cálculo y la creación de borradores necesitan la política comercial y las condiciones por defecto. |
| 5 | **Clientes, categorías y productos** | Son los datos maestros que una cotización necesita. Son módulos simples que consolidan los patrones (CRUD, validación, auditoría, desactivación lógica) antes del núcleo. |
| 6 | **Motor de cálculo** | Es puro y no depende de la base de datos, así que se prueba aislado. Debe estar verificado antes de construir la cotización encima. |
| 7 | **Cotizaciones** | El núcleo. Con todo lo anterior listo, se construye el ciclo de vida completo. |
| 8 | **Documentos** | Necesita cotizaciones emitidas y datos de la empresa. Al terminar este bloque existe el **MVP demostrable**: el flujo principal completo sin IA. |
| 9 | **Asistente IA (Gemini)** | Es asistencial y opcional en tiempo de ejecución; el sistema debe funcionar sin él. Construirlo después del MVP evita que un problema externo (claves, cuotas, latencia) bloquee el núcleo. |
| 10 | **Consulta de auditoría y dashboard** | Son vistas de solo lectura sobre datos que ya existen. Tienen más valor cuando hay datos reales que mostrar. |
| 11 | **QA, hardening y despliegue** | Verificación integral de seguridad, rendimiento, experiencia y despliegue en Ubuntu Server. |
| 12 | **Documentación final y demo** | Cierre con la documentación sincronizada y el guion de la demostración. |

---

## 4. Épicas

| Épica | Nombre | Alcance | Requisitos principales |
|---|---|---|---|
| **EP-01** | Infraestructura | Repositorio, solución backend, frontend base, base de datos, configuración, errores, logging, pruebas, sistema visual | RNF-MAN, RNF-SEG-07, RNF-USA-01 a 03 |
| **EP-02** | Autenticación | Login, sesión JWT, sello de seguridad, roles, cambio de contraseña, perfil, limitador de intentos | RF-AUT-01 a 07, RNF-SEG-01 a 05 |
| **EP-03** | Gestión de usuarios | Administración de cuentas y reglas del último Administrador | RF-USR-01 a 05 |
| **EP-04** | Auditoría | Campos de auditoría, registro transaccional de eventos, concurrencia optimista, consulta | RF-AUD-01, 02; RNF-DAT-04 |
| **EP-05** | Configuración | Datos de la empresa, política comercial, vigencia | RF-CFG-01 a 03 |
| **EP-06** | Clientes | Gestión de clientes y condición VIP | RF-CLI-01 a 06 |
| **EP-07** | Categorías | Gestión de categorías | RF-CAT-01 a 04 |
| **EP-08** | Productos | Gestión del catálogo | RF-PRO-01 a 04 |
| **EP-09** | Motor de cálculo | Value objects monetarios, política comercial, motor puro y sus pruebas | RF-COT-06, RN-01 a 06, RN-17 |
| **EP-10** | Cotizaciones | Agregado, máquina de estados, borradores, emisión, consecutivo, cierre, vencimiento, duplicación, consulta | RF-COT-01 a 14 |
| **EP-11** | Documentos | Plantilla, vista previa HTML y PDF | RF-DOC-01, 02 |
| **EP-12** | Asistente IA (Gemini) | Integración, contexto mínimo, límites, degradación, interfaz | RF-IA-01 a 03, RNF-SEG-11, 12 |
| **EP-13** | Dashboard | Indicadores, recientes y actividad | RF-DSH-01 a 03 |
| **EP-14** | QA y hardening | Seguridad, restricciones, rendimiento, experiencia, regresión | Sección 14 de `spec.md` |
| **EP-15** | Despliegue y entrega | Ubuntu Server, respaldos, datos de demostración, documentación final, demo | RNF-OPE, RNF-DIS-03, RNF-PRO-03 |

---

## 5. Fases

Las fases son secuenciales. Dentro de una fase, las tareas pueden ejecutarse en el orden del backlog.

### F0 · Preparación
- **Objetivo:** dejar el proyecto listo para empezar a construir, con la documentación base y las decisiones técnicas pendientes tomadas.
- **Dependencias:** `spec.md` aprobado.
- **Entregables:** repositorio Git, documentos base (`intent.md`, `spec.md`, `plan.md`, esqueletos de `README.md` y `docs/`), registro de decisiones técnicas (Anexo A) completo.
- **Criterio de finalización:** el repositorio existe con su primer commit, todos los documentos base están en él y todas las decisiones técnicas del Anexo A tienen una opción elegida y justificada.

### F1 · Fundaciones
- **Objetivo:** tener backend, frontend y base de datos conectados, con los mecanismos transversales básicos y las pruebas funcionando.
- **Dependencias:** F0.
- **Entregables:** solución backend por capas con health check; conexión a PostgreSQL 18 con migraciones; usuarios de base de datos con permisos; manejo de errores y logging; frontend con sistema visual, temas, layout, cliente API y rutas; proyectos de pruebas.
- **Criterio de finalización:** el frontend muestra el estado del backend y de la base de datos; una migración vacía se aplica sin errores; las pruebas de ejemplo de backend y frontend pasan; la aplicación no arranca si falta una variable de entorno obligatoria.

### F2 · Identidad, auditoría y configuración
- **Objetivo:** usuarios autenticados, roles aplicados, auditoría registrando eventos y configuración disponible.
- **Dependencias:** F1.
- **Entregables:** EP-02, EP-03, la parte de registro de EP-04 y EP-05.
- **Criterio de finalización:** el primer Administrador inicia sesión, cambia su contraseña obligatoria, crea un usuario de Ventas y actualiza la configuración; cada acción aparece en `audit_events`; desactivar un usuario invalida su sesión de inmediato; las pruebas de la fase pasan.

### F3 · Datos maestros
- **Objetivo:** clientes, categorías y productos completos.
- **Dependencias:** F2.
- **Entregables:** EP-06, EP-07 y EP-08.
- **Criterio de finalización:** un Administrador gestiona categorías y productos; Ventas registra y busca clientes sin poder asignar VIP; los cambios de precio generan su evento; todos los criterios de aceptación de los RF de la fase se cumplen.

### F4 · Núcleo: motor de cálculo y cotizaciones
- **Objetivo:** ciclo de vida completo de las cotizaciones con cálculos exactos.
- **Dependencias:** F3.
- **Entregables:** EP-09 y EP-10, más las integraciones pendientes con Clientes y Configuración.
- **Criterio de finalización:** todos los casos críticos del motor pasan; un usuario crea un borrador, agrega líneas, ve el desglose en vivo, emite con número consecutivo, registra aceptación o rechazo, cancela y duplica; el vencimiento automático funciona; cambiar precios o parámetros no altera cotizaciones emitidas.

### F5 · Documentos → **Hito: MVP demostrable**
- **Objetivo:** documento comercial profesional en vista previa y PDF.
- **Dependencias:** F4.
- **Entregables:** EP-11.
- **Criterio de finalización:** la vista previa y el PDF muestran el mismo contenido; los borradores llevan marca de agua; el PDF solo se genera para cotizaciones emitidas; el flujo principal completo, salvo la redacción con IA, funciona de punta a punta.

### F6 · Asistente IA
- **Objetivo:** redacción comercial asistida por Gemini, con degradación controlada.
- **Dependencias:** F5.
- **Entregables:** EP-12.
- **Criterio de finalización:** se generan y mejoran los cuatro textos en borradores; sin clave configurada o con Gemini caído, el sistema funciona y lo informa; el límite de uso se aplica; a Gemini no se envían correos ni teléfonos.

### F7 · Dashboard y consulta de auditoría
- **Objetivo:** vistas de supervisión.
- **Dependencias:** F6.
- **Entregables:** consulta de EP-04 y EP-13.
- **Criterio de finalización:** el Administrador consulta la auditoría con filtros; el dashboard muestra indicadores correctos según las definiciones de RF-DSH-01; todos los RF del proyecto están implementados.

### F8 · QA, hardening y despliegue
- **Objetivo:** dejar el sistema listo para producción.
- **Dependencias:** F7.
- **Entregables:** EP-14, despliegue y respaldos de EP-15, datos de demostración.
- **Criterio de finalización:** todos los criterios de aceptación de la sección 14 de `spec.md` están verificados; el sistema corre en Ubuntu Server con HTTPS; el respaldo se ejecuta y una restauración de prueba funciona; no hay defectos críticos ni altos abiertos.

### F9 · Documentación final y entrega
- **Objetivo:** cerrar el proyecto.
- **Dependencias:** F8.
- **Entregables:** documentación final sincronizada, guion de demo ensayado, versión `v1.0.0` etiquetada.
- **Criterio de finalización:** se cumple la definición de completado del proyecto ([sección 11.3](#113-proyecto)).

---

## 6. Backlog de implementación

**Formato:** cada tarea indica prioridad (**E** esencial, **I** importante, **D** deseable), dependencias y, cuando aplica, los requisitos que cubre. Además de sus criterios propios, toda tarea cumple la definición de completado de la [sección 11.1](#111-tarea).

**Resumen:** 100 tareas — 85 esenciales, 14 importantes y 1 deseable.

### F0 · Preparación

**T-001 · Repositorio y estructura base** · E · Depende de: —
- **Descripción:** crear el repositorio Git con las carpetas de backend, frontend y documentación; `.gitignore` (incluidos archivos de entorno), `.editorconfig` y licencia si corresponde.
- **Criterios:** el primer commit es `chore: estructura inicial`; `.env` y equivalentes están ignorados; el repositorio no contiene secretos.

**T-002 · Documentación base** · E · Depende de: T-001
- **Descripción:** agregar `spec.md` y `plan.md`; redactar `intent.md` (problema, objetivos, usuarios); crear esqueletos de `README.md`, `docs/negocio.md`, `docs/uso-ia.md` y `docs/demo.md`.
- **Criterios:** los siete documentos existen; `intent.md` es coherente con `spec.md`; el commit usa el prefijo `docs:`.

**T-003 · Decisiones técnicas pendientes** · E · Depende de: T-002
- **Descripción:** elegir y justificar cada decisión del Anexo A (organización de proyectos del backend, librería de componentes, gestión de estado de servidor, formularios, PDF, pruebas, proceso en segundo plano, servidor web).
- **Criterios:** cada decisión del Anexo A tiene opción elegida, justificación y fecha; ninguna contradice `spec.md`; toda dependencia elegida justifica su valor.

### F1 · Fundaciones

**T-004 · Solución backend por capas** · E · Depende de: T-003
- **Descripción:** crear la solución ASP.NET Core con las capas Dominio, Aplicación, Infraestructura y API según DT-01, con inyección de dependencias configurada.
- **Criterios:** compila sin advertencias relevantes; las referencias entre capas respetan la dirección (el dominio no depende de nadie); existe un endpoint de health check.

**T-005 · Configuración y secretos** · E · Depende de: T-004 · RNF-SEG-07
- **Descripción:** configuración por entorno; lectura de cadenas de conexión, clave JWT, clave y modelo de Gemini y credenciales del primer Administrador desde variables de entorno; archivo `.env.example` sin valores reales.
- **Criterios:** la aplicación no arranca y muestra un mensaje claro si falta una variable obligatoria (la de Gemini es opcional); ningún valor real está en el repositorio.

**T-006 · Manejo de errores y logging** · E · Depende de: T-004 · RNF-MAN-05, RNF-MAN-06, RNF-SEG-10
- **Descripción:** manejo global de excepciones con respuestas *ProblemDetails*; traducción de errores de validación y de dominio a códigos HTTP consistentes; logging estructurado con niveles.
- **Criterios:** un error inesperado devuelve *500* sin trazas ni SQL; un error de validación devuelve *400* con el detalle por campo; los errores quedan en el log con su contexto.

**T-007 · Persistencia con EF Core y PostgreSQL 18** · E · Depende de: T-005
- **Descripción:** configurar EF Core con el proveedor de PostgreSQL, convención `snake_case`, tipos monetarios, `uuidv7()` por defecto, migración inicial y health check de la base de datos.
- **Criterios:** la migración inicial se aplica sobre PostgreSQL 18 sin errores; se verifica que la versión del proveedor soporte PostgreSQL 18; el health check refleja el estado de la base de datos.

**T-008 · Usuarios de base de datos y permisos** · E · Depende de: T-007 · RNF-SEG-08
- **Descripción:** script documentado que crea el usuario propietario del esquema y el usuario de aplicación con permisos mínimos.
- **Criterios:** las migraciones corren con el propietario; la aplicación corre con el usuario de aplicación; el usuario de aplicación no puede crear ni alterar tablas.

**T-009 · Reloj y zona horaria** · E · Depende de: T-004 · RNF-DAT-03
- **Descripción:** servicio de reloj inyectable y utilidades para obtener la fecha de negocio en America/Costa_Rica.
- **Criterios:** el dominio no usa la hora del sistema directamente; las pruebas pueden fijar la fecha; la conversión UTC ↔ Costa Rica está probada en los bordes del día.

**T-010 · Proyecto frontend** · E · Depende de: T-003
- **Descripción:** proyecto Vite + React + TypeScript en modo estricto, con linter y formateador.
- **Criterios:** compila sin errores de tipos; el linter no reporta errores; existe un comando de desarrollo y otro de compilación para producción.

**T-011 · Sistema visual, temas y layout** · E · Depende de: T-010 · RNF-USA-01, 02, 06, 08
- **Descripción:** tokens de diseño, modo claro y oscuro (preferencia del sistema operativo más selector manual recordado), componentes base (botones, campos, tablas, paginación, diálogo de confirmación, notificaciones, estados vacíos y de carga) y layout con navegación lateral.
- **Criterios:** todos los componentes se ven correctamente en ambos temas; el contraste es legible; el diálogo de confirmación es reutilizable; el tema elegido persiste al recargar.

**T-012 · Cliente API y estado de servidor** · E · Depende de: T-010, T-006
- **Descripción:** capa tipada de acceso a la API, separada de los componentes; interpretación de *ProblemDetails*; gestión de caché de consultas según DT-03.
- **Criterios:** ningún componente llama a la API directamente; los errores del servidor se muestran en lenguaje comprensible; los errores de validación se asocian a sus campos.

**T-013 · Navegación y rutas protegidas** · E · Depende de: T-011
- **Descripción:** enrutamiento con rutas públicas y protegidas, y protección por rol.
- **Criterios:** una ruta protegida sin sesión redirige al inicio de sesión; una ruta de Administrador no es accesible para Ventas; la navegación muestra solo las opciones del rol.

**T-014 · Formatos regionales** · E · Depende de: T-010 · RNF-USA-03
- **Descripción:** utilidades de formato para colones (₡1.179.720,00), fechas (dd/mm/aaaa) y fechas con hora en hora de Costa Rica.
- **Criterios:** los formatos se aplican de forma uniforme en toda la interfaz; tienen pruebas con valores límite (cero, millones, decimales).

**T-015 · Proyectos de pruebas** · E · Depende de: T-007, T-010
- **Descripción:** proyecto de pruebas unitarias, proyecto de pruebas de integración contra una base PostgreSQL 18 de pruebas y configuración de pruebas del frontend, según DT-06.
- **Criterios:** una prueba de ejemplo de cada tipo pasa; la base de pruebas se recrea con las migraciones; un solo comando ejecuta todas las pruebas del backend.

**T-016 · Integración continua** · D · Depende de: T-015
- **Descripción:** flujo automático que compila y ejecuta las pruebas en cada envío al repositorio remoto.
- **Criterios:** una prueba fallida marca el envío como fallido.

### F2 · Identidad, auditoría y configuración

**T-017 · Entidad Usuario y tabla `users`** · E · Depende de: T-007
- **Descripción:** entidad Usuario, value object Correo electrónico, enumeración Rol y migración de `users` con sus restricciones (correo único y en minúsculas, usuario de sistema único, `security_stamp`, `must_change_password`).
- **Criterios:** la tabla coincide con la sección 9.3 de `spec.md`; la base de datos rechaza correos duplicados, correos en mayúsculas y un segundo usuario de sistema.

**T-018 · Datos iniciales de identidad** · E · Depende de: T-017, T-005
- **Descripción:** creación idempotente del usuario de sistema y del primer Administrador desde variables de entorno, con `must_change_password = true`.
- **Criterios:** ejecutar el arranque dos veces no duplica usuarios; el usuario de sistema no tiene contraseña; las credenciales del Administrador no están en el código.

**T-019 · Campos de auditoría automáticos** · E · Depende de: T-017
- **Descripción:** campos `created_at`, `created_by`, `updated_at` y `updated_by`, llenados automáticamente en la capa de persistencia con el usuario autenticado o el usuario de sistema.
- **Criterios:** ningún caso de uso los asigna manualmente; una prueba de integración verifica creación y modificación.

**T-020 · Registro de eventos de auditoría** · E · Depende de: T-019 · RF-AUD-01, RN-19
- **Descripción:** tabla `audit_events`, catálogo de tipos de evento (sección 10.3 de `spec.md`) y servicio que registra un evento dentro de la transacción del caso de uso.
- **Criterios:** si el caso de uso falla, no queda evento; si el registro falla, el cambio se deshace; el usuario de aplicación no puede modificar ni borrar eventos; los índices de la sección 9.5 existen.

**T-021 · Concurrencia optimista** · E · Depende de: T-007 · RNF-DAT-04
- **Descripción:** control de concurrencia con `xmin` y traducción del conflicto a *409* con mensaje claro.
- **Criterios:** dos actualizaciones simultáneas del mismo registro producen un *409* en la segunda; ningún cambio se sobrescribe en silencio.

**T-022 · Contraseñas** · E · Depende de: T-017 · RNF-SEG-02, RNF-SEG-03
- **Descripción:** hash adaptativo estándar y validación de la política (mínimo 10 caracteres, con letra y número).
- **Criterios:** las contraseñas nunca se guardan ni se registran en texto plano; la política se valida con mensajes específicos.

**T-023 · Inicio de sesión (API)** · E · Depende de: T-020, T-022 · RF-AUT-01, RNF-SEG-04
- **Descripción:** endpoint de login que emite un JWT de 8 horas con usuario, rol y `security_stamp`.
- **Criterios:** se cumplen todos los criterios de RF-AUT-01; los intentos exitosos y fallidos se auditan; el mensaje de error es idéntico para correo inexistente, contraseña errónea y usuario inactivo.

**T-024 · Validación de sesión por solicitud** · E · Depende de: T-023 · RF-AUT-03
- **Descripción:** verificación en cada solicitud de la firma, la vigencia y el `security_stamp` del token.
- **Criterios:** se cumplen los criterios de RF-AUT-03; una prueba de integración demuestra que desactivar un usuario invalida su token al instante.

**T-025 · Autorización por rol y propiedad** · E · Depende de: T-024 · RF-AUT-04, RN-32
- **Descripción:** políticas por rol y mecanismo reutilizable para verificar la propiedad de un recurso; autenticación obligatoria por defecto en todos los endpoints.
- **Criterios:** se cumplen los criterios de RF-AUT-04; solo el endpoint de login es anónimo.

**T-026 · Cambio obligatorio de contraseña** · E · Depende de: T-024 · RF-AUT-07
- **Descripción:** bloqueo de todas las operaciones excepto el cambio de contraseña cuando `must_change_password` es verdadero.
- **Criterios:** se cumplen los criterios de RF-AUT-07.

**T-027 · Limitador de intentos de inicio de sesión** · E · Depende de: T-023 · RNF-SEG-05
- **Descripción:** límite de 5 intentos por minuto por IP.
- **Criterios:** el sexto intento en un minuto recibe *429* con un mensaje claro.

**T-028 · HTTPS y CORS** · E · Depende de: T-004 · RNF-SEG-01, RNF-SEG-09
- **Descripción:** redirección a HTTPS fuera de desarrollo y CORS restringido al origen configurado del frontend.
- **Criterios:** un origen distinto es rechazado; en producción no se atiende HTTP sin redirección.

**T-029 · Interfaz de inicio y cierre de sesión** · E · Depende de: T-023, T-013 · RF-AUT-01, RF-AUT-02
- **Descripción:** pantalla de inicio de sesión, manejo del token, cierre de sesión y redirección ante *401*.
- **Criterios:** se cumplen los criterios de RF-AUT-01 y RF-AUT-02 desde la interfaz; al vencer la sesión, el usuario vuelve al inicio de sesión con un aviso.

**T-030 · Interfaz de cambio de contraseña** · E · Depende de: T-026, T-029 · RF-AUT-06, RF-AUT-07
- **Descripción:** API y pantalla de cambio de contraseña voluntario y obligatorio.
- **Criterios:** se cumplen los criterios de RF-AUT-06 y RF-AUT-07; las demás sesiones quedan invalidadas y la actual continúa.

**T-031 · Perfil** · I · Depende de: T-029 · RF-AUT-05
- **Descripción:** API y pantalla para consultar el perfil y cambiar el nombre.
- **Criterios:** se cumplen los criterios de RF-AUT-05.

**T-032 · Gestión de usuarios (API)** · I · Depende de: T-025, T-022 · RF-USR-01 a 05, RN-30, RN-31
- **Descripción:** listar y buscar, crear, editar, cambiar rol, desactivar, reactivar y restablecer contraseña.
- **Criterios:** se cumplen los criterios de RF-USR-01 a 05; las pruebas cubren el último Administrador, la autodesactivación y la regeneración del sello.

**T-033 · Gestión de usuarios (interfaz)** · I · Depende de: T-032, T-011
- **Descripción:** listado con filtros, formulario de alta y edición, acciones de estado y restablecimiento con confirmación.
- **Criterios:** todas las operaciones de T-032 son accesibles desde la interfaz; los errores de negocio se muestran con claridad.

**T-034 · Configuración del sistema (dominio y datos)** · E · Depende de: T-019 · RN-26, RN-27
- **Descripción:** entidad de configuración con datos de la empresa y política comercial, tabla de fila única con sus restricciones y creación idempotente de la fila inicial.
- **Criterios:** la tabla coincide con la sección 9.3 de `spec.md`; la base de datos rechaza una segunda fila y parámetros que violen RN-27; las validaciones de dominio de RN-27 tienen pruebas unitarias.

**T-035 · Configuración (API)** · E · Depende de: T-034, T-025 · RF-CFG-01 a 03
- **Descripción:** consultar y actualizar los datos de la empresa, las condiciones por defecto y los parámetros comerciales, con auditoría de valores anteriores y nuevos.
- **Criterios:** se cumplen los criterios de RF-CFG-01 a 03, excepto el conteo de borradores afectados, que se completa en T-068.

**T-036 · Configuración (interfaz)** · E · Depende de: T-035, T-011
- **Descripción:** pantalla de configuración en secciones (empresa, condiciones, parámetros comerciales).
- **Criterios:** valida mientras se escribe; cada regla de RN-27 incumplida tiene su mensaje; pide confirmación antes de guardar parámetros.

**T-037 · Pruebas de la fase F2** · E · Depende de: T-023 a T-036
- **Descripción:** pruebas de integración de autenticación, autorización, usuarios, auditoría y configuración.
- **Criterios:** cubren *401*, *403*, *409*, *429*, el sello de seguridad, el último Administrador y la atomicidad de la auditoría; todas pasan.

### F3 · Datos maestros

**T-038 · Entidad Cliente y tabla `customers`** · E · Depende de: T-019
- **Descripción:** entidad Cliente, value object Teléfono y migración con sus restricciones.
- **Criterios:** la tabla coincide con la sección 9.3 de `spec.md`; el teléfono se normaliza; la base de datos rechaza teléfonos y correos con formato inválido.

**T-039 · Clientes (API)** · E · Depende de: T-038, T-025 · RF-CLI-01, 02, 04, 05, 06; RN-21
- **Descripción:** registrar, buscar, editar, marcar o quitar VIP, desactivar y reactivar.
- **Criterios:** se cumplen los criterios de los RF indicados; Ventas no puede asignar VIP aunque envíe el campo; se advierte un posible correo duplicado; las pruebas cubren los permisos.

**T-040 · Clientes (interfaz)** · E · Depende de: T-039, T-011
- **Descripción:** listado con búsqueda y filtros, formulario, detalle y acciones de Administrador.
- **Criterios:** la interfaz oculta las acciones no permitidas para Ventas; la advertencia de duplicado permite continuar. El historial de cotizaciones del detalle se completa en T-068.

**T-041 · Entidad Categoría, tabla y datos iniciales** · E · Depende de: T-019
- **Descripción:** entidad Categoría, migración con nombre único sin distinguir mayúsculas y creación idempotente de las cuatro categorías iniciales.
- **Criterios:** la base de datos rechaza "licencia de software" si existe "Licencia de Software"; las cuatro categorías existen tras el primer arranque.

**T-042 · Categorías (API)** · E · Depende de: T-041, T-025 · RF-CAT-01 a 04
- **Descripción:** crear, listar, editar, desactivar y reactivar.
- **Criterios:** se cumplen los criterios de RF-CAT-01 a 04, incluida la sugerencia de reactivar.

**T-043 · Categorías (interfaz)** · E · Depende de: T-042, T-011
- **Descripción:** listado con cantidad de productos y formulario.
- **Criterios:** todas las operaciones de T-042 son accesibles.

**T-044 · Entidad Producto y tabla `products`** · E · Depende de: T-041
- **Descripción:** entidad Producto con value object Dinero (o el tipo preliminar que se consolida en T-048) y migración con sus restricciones e índice por categoría.
- **Criterios:** la tabla coincide con la sección 9.3 de `spec.md`; la base de datos rechaza precios negativos y nombres repetidos.

**T-045 · Productos (API)** · E · Depende de: T-044, T-025 · RF-PRO-01 a 04
- **Descripción:** crear, buscar, editar (con evento específico de cambio de precio), desactivar y reactivar.
- **Criterios:** se cumplen los criterios de RF-PRO-01 a 04; no se puede asignar una categoría inactiva.

**T-046 · Productos (interfaz)** · E · Depende de: T-045, T-011
- **Descripción:** catálogo con búsqueda y filtros, formulario y acciones de Administrador.
- **Criterios:** el precio se edita y muestra con formato de colones; Ventas ve el catálogo en modo de solo lectura.

**T-047 · Pruebas de la fase F3** · E · Depende de: T-038 a T-046
- **Descripción:** pruebas de integración de clientes, categorías y productos.
- **Criterios:** cubren validaciones, unicidad, permisos, desactivación y eventos; todas pasan.

### F4 · Núcleo: motor de cálculo y cotizaciones

**T-048 · Value objects monetarios y política comercial** · E · Depende de: T-004 · RN-07, RN-08, RN-17
- **Descripción:** Dinero, Cantidad, Porcentaje y Política comercial, con redondeo comercial a 2 decimales.
- **Criterios:** valores inválidos no pueden construirse; el redondeo de los valores medios se aleja de cero; tienen pruebas unitarias.

**T-049 · Motor de cálculo** · E · Depende de: T-048 · RF-COT-06; RN-01 a 03, 05, 06, 17
- **Descripción:** servicio de dominio puro que implementa el algoritmo oficial de la sección 7.1 de `spec.md` y devuelve el desglose financiero.
- **Criterios:** no depende de base de datos, HTTP ni reloj; recibe la política como parámetro; el desglose es coherente por construcción.

**T-050 · Pruebas del motor de cálculo** · E · Depende de: T-049 · RNF-MAN-04
- **Descripción:** pruebas unitarias con todos los casos de la [sección 9.4](#94-casos-críticos-del-motor-de-cálculo).
- **Criterios:** todos los casos pasan con los valores exactos esperados.

**T-051 · Agregado Cotización y tablas** · E · Depende de: T-049, T-038, T-044
- **Descripción:** entidades Cotización y Línea de cotización, value objects asociados y migración de `quotes` y `quote_lines` con todas sus restricciones e índices.
- **Criterios:** las tablas coinciden con la sección 9.3 de `spec.md`; la base de datos rechaza combinaciones imposibles de estado y datos, desgloses aritméticamente incoherentes, líneas con total mal calculado y productos repetidos.

**T-052 · Máquina de estados** · E · Depende de: T-051 · RN-10, RN-15, RN-34, RN-35
- **Descripción:** transiciones permitidas en el agregado, con bloqueo de modificaciones fuera de Borrador.
- **Criterios:** pruebas unitarias cubren cada transición válida y rechazan todas las inválidas; una cotización emitida no puede modificarse.

**T-053 · Crear borrador (API)** · E · Depende de: T-052, T-035, T-025 · RF-COT-01, RN-33
- **Descripción:** creación de un borrador con cliente activo, propietario y condiciones por defecto.
- **Criterios:** se cumplen los criterios de RF-COT-01.

**T-054 · Gestión de líneas (API)** · E · Depende de: T-053 · RF-COT-02, RF-COT-03; RN-07, RN-28
- **Descripción:** agregar líneas (sumando cantidades si el producto ya está), cambiar cantidades y quitar líneas con renumeración.
- **Criterios:** se cumplen los criterios de RF-COT-02 y RF-COT-03; solo el propietario o un Administrador pueden hacerlo.

**T-055 · Cliente y textos del borrador (API)** · E · Depende de: T-053 · RF-COT-04, RF-COT-05
- **Descripción:** cambio de cliente y edición de los cuatro textos comerciales.
- **Criterios:** se cumplen los criterios de RF-COT-04 y RF-COT-05.

**T-056 · Recálculo con datos vigentes** · E · Depende de: T-054 · RF-COT-06, RN-22
- **Descripción:** recálculo del borrador en cada modificación con precios, condición VIP y política vigentes; endpoint que devuelve el desglose.
- **Criterios:** un cambio de precio, de VIP o de parámetros se refleja en el siguiente recálculo del borrador; los montos se identifican como estimados; el endpoint responde en menos de 300 ms.

**T-057 · Consecutivo de cotizaciones** · E · Depende de: T-051 · RN-13
- **Descripción:** tabla `quote_number_sequences` y generador que bloquea la fila del año dentro de la transacción.
- **Criterios:** el número tiene el formato `COT-AAAA-000001`; se reinicia cada año según la hora de Costa Rica; nunca hay saltos ni duplicados.

**T-058 · Emitir cotización (API)** · E · Depende de: T-056, T-057 · RF-COT-07; RN-09, RN-12, RN-14, RN-16
- **Descripción:** validación, recálculo final, congelamiento, asignación de número y vigencia.
- **Criterios:** se cumplen los criterios de RF-COT-07; cada causa de rechazo tiene un mensaje específico.

**T-059 · Pruebas de emisión** · E · Depende de: T-058
- **Descripción:** pruebas de integración de la emisión.
- **Criterios:** emisiones simultáneas reciben números distintos y consecutivos; una emisión fallida no consume número; cambiar precio, VIP o parámetros después de emitir no altera la cotización.

**T-060 · Aceptar, rechazar y cancelar (API)** · E · Depende de: T-058 · RF-COT-08 a 10; RN-32, RN-34, RN-35
- **Descripción:** transiciones de cierre con permisos de propiedad.
- **Criterios:** se cumplen los criterios de RF-COT-08 a 10; Ventas no puede cerrar cotizaciones ajenas.

**T-061 · Vencimiento automático** · E · Depende de: T-060, T-009 · RF-COT-11, RN-14
- **Descripción:** proceso en segundo plano diario (según DT-07) que vence las cotizaciones emitidas con vigencia expirada.
- **Criterios:** se cumplen los criterios de RF-COT-11; las pruebas usan el reloj inyectable para simular el paso de los días.

**T-062 · Duplicar cotización (API)** · E · Depende de: T-056 · RF-COT-12; RN-23 a 25
- **Descripción:** creación de un borrador a partir de cualquier cotización, con precios actuales.
- **Criterios:** se cumplen los criterios de RF-COT-12; la respuesta informa las líneas omitidas.

**T-063 · Consulta de cotizaciones (API)** · E · Depende de: T-058 · RF-COT-13, RF-COT-14
- **Descripción:** detalle con historial de eventos y acciones disponibles; búsqueda con filtros y paginación.
- **Criterios:** se cumplen los criterios de RF-COT-13 y RF-COT-14; Ventas ve todas las cotizaciones.

**T-064 · Editor de cotizaciones (interfaz)** · E · Depende de: T-056, T-011, T-014 · RNF-USA-05
- **Descripción:** editor con búsqueda de productos al escribir, tabla de líneas editable, cliente, cuatro textos y desglose en vivo rotulado como estimado.
- **Criterios:** se completa una cotización sin ventanas modales; el desglose se actualiza tras cada cambio; los errores de validación aparecen junto al campo.

**T-065 · Acciones de ciclo de vida (interfaz)** · E · Depende de: T-064, T-060
- **Descripción:** emitir, aceptar, rechazar y cancelar con confirmación, explicación de consecuencias y captura de motivo.
- **Criterios:** solo se muestran las acciones válidas para el estado y el usuario; la cancelación de una emitida exige motivo.

**T-066 · Detalle e historial (interfaz)** · E · Depende de: T-063, T-011
- **Descripción:** pantalla de detalle con historial de eventos y enlace al origen; listado con filtros.
- **Criterios:** se cumplen los criterios de RF-COT-13 y RF-COT-14 desde la interfaz.

**T-067 · Duplicación (interfaz)** · E · Depende de: T-062, T-066
- **Descripción:** acción de duplicar con aviso de líneas omitidas y apertura del nuevo borrador.
- **Criterios:** el usuario ve qué líneas no se copiaron y por qué.

**T-068 · Integraciones pendientes** · E · Depende de: T-063, T-035, T-040 · RF-CLI-03, RF-CFG-03
- **Descripción:** historial de cotizaciones en el detalle del cliente y aviso de borradores afectados al cambiar parámetros.
- **Criterios:** se cumplen completos RF-CLI-03 y RF-CFG-03.

**T-069 · Pruebas de la fase F4** · E · Depende de: T-053 a T-068
- **Descripción:** pruebas de integración del ciclo de vida completo.
- **Criterios:** cubren permisos por propiedad, todas las transiciones, aceptación fuera de vigencia, vencimiento idempotente, duplicación con inactivos y auditoría de cada acción; todas pasan.

### F5 · Documentos

**T-070 · Plantilla del documento comercial** · E · Depende de: T-063, T-034
- **Descripción:** plantilla única con todos los elementos de RF-DOC-01, datos de la empresa y logo estático.
- **Criterios:** se ve profesional y consistente con la identidad visual; muestra solo los textos que existen; usa los formatos regionales.

**T-071 · Vista previa HTML** · E · Depende de: T-070 · RF-DOC-01, RN-36
- **Descripción:** generación de la vista previa para cualquier estado.
- **Criterios:** se cumplen los criterios de RF-DOC-01; los borradores muestran la marca de agua.

**T-072 · Generación de PDF** · E · Depende de: T-070 · RF-DOC-02, RN-36
- **Descripción:** generación del PDF con la misma plantilla, según DT-05.
- **Criterios:** se cumplen los criterios de RF-DOC-02; el PDF se genera en menos de 3 segundos.

**T-073 · Documentos (interfaz)** · E · Depende de: T-071, T-072, T-066
- **Descripción:** visor de vista previa y botón de descarga del PDF.
- **Criterios:** la descarga solo está disponible para cotizaciones emitidas.

**T-074 · Pruebas de documentos** · E · Depende de: T-071, T-072
- **Descripción:** pruebas de contenido y de reglas de disponibilidad.
- **Criterios:** la vista previa y el PDF contienen los mismos datos; un borrador no genera PDF; los estados cerrados muestran su sello. **Al terminar esta tarea se alcanza el MVP demostrable.**

### F6 · Asistente IA

**T-075 · Interfaz del asistente y modo no disponible** · I · Depende de: T-005 · RF-IA-03
- **Descripción:** abstracción del asistente con una implementación real y otra de "no disponible" que se usa cuando no hay clave configurada.
- **Criterios:** sin clave, la aplicación arranca y el asistente se reporta como no disponible.

**T-076 · Cliente de Gemini** · I · Depende de: T-075 · RNF-REN-04, RNF-MAN-06
- **Descripción:** llamada a la API de Gemini con el modelo configurado y tiempo límite de 20 segundos.
- **Criterios:** los errores y los tiempos excedidos se traducen a errores de dominio; el log registra duración y resultado, nunca el contenido.

**T-077 · Contexto e instrucciones** · I · Depende de: T-076, T-056 · RF-IA-01, RF-IA-02, RN-18, RNF-SEG-11
- **Descripción:** construcción del contexto con datos mínimos y montos del motor; instrucciones por tipo de texto y acción; límites de longitud de entrada y salida.
- **Criterios:** una prueba verifica que no se envían correos ni teléfonos; la salida se trata como texto sin formato.

**T-078 · Límite de uso y auditoría del asistente** · I · Depende de: T-077, T-020 · RNF-SEG-12
- **Descripción:** registro del evento de texto generado y límite de 30 solicitudes por hora por usuario, contado sobre la auditoría.
- **Criterios:** la solicitud 31 de la hora se rechaza con un mensaje que indica cuándo se podrá volver a usar; el evento no contiene el texto generado.

**T-079 · Asistente (API)** · I · Depende de: T-078 · RF-IA-01 a 03
- **Descripción:** endpoint para generar y mejorar los cuatro textos, solo en borradores y con permisos de propiedad.
- **Criterios:** se cumplen los criterios de RF-IA-01 a 03; el endpoint devuelve una sugerencia y no modifica el borrador.

**T-080 · Asistente (interfaz)** · I · Depende de: T-079, T-064
- **Descripción:** botones por campo, indicador de progreso, sugerencia para aceptar, editar o descartar, comparación con el original y estados deshabilitado y de error.
- **Criterios:** nada se guarda sin aceptación; la interfaz no se bloquea mientras espera.

**T-081 · Pruebas del asistente** · I · Depende de: T-079
- **Descripción:** pruebas con una implementación simulada de Gemini.
- **Criterios:** cubren éxito, error, tiempo excedido, límite alcanzado, estado distinto de Borrador y falta de permisos; con Gemini simulado como caído, el resto del sistema funciona.

### F7 · Dashboard y consulta de auditoría

**T-082 · Consulta de auditoría (API)** · I · Depende de: T-020, T-025 · RF-AUD-02
- **Descripción:** consulta paginada con filtros por usuario, tipo de evento, tipo de entidad y fechas.
- **Criterios:** se cumplen los criterios de RF-AUD-02; solo el Administrador tiene acceso.

**T-083 · Consulta de auditoría (interfaz)** · I · Depende de: T-082, T-011
- **Descripción:** pantalla de auditoría con filtros.
- **Criterios:** las fechas se muestran en hora de Costa Rica; es de solo lectura.

**T-084 · Dashboard (API)** · I · Depende de: T-063, T-082 · RF-DSH-01 a 03
- **Descripción:** indicadores, cotizaciones recientes y actividad reciente según las definiciones de `spec.md`.
- **Criterios:** el monto total cotizado excluye borradores y canceladas; Ventas solo ve su propia actividad; una prueba verifica cada indicador con datos conocidos.

**T-085 · Dashboard (interfaz)** · I · Depende de: T-084, T-011
- **Descripción:** página de inicio con indicadores y cotizaciones recientes (RF-DSH-01, 02) y actividad reciente (RF-DSH-03, deseable).
- **Criterios:** al seleccionar una cotización reciente se abre su detalle; si el tiempo no alcanza para RF-DSH-03, la página funciona completa sin él.

### F8 · QA, hardening y despliegue

**T-086 · Revisión de seguridad** · E · Depende de: T-085
- **Descripción:** verificación de los criterios 11 a 15 de la sección 14.3 de `spec.md` y de todos los RNF-SEG.
- **Criterios:** todos los endpoints salvo el login exigen autenticación; Ventas recibe *403* en cada función de Administrador; el historial de Git no contiene secretos; los errores no exponen detalles internos.

**T-087 · Verificación de restricciones de la base de datos** · E · Depende de: T-069
- **Descripción:** intentos de insertar directamente datos inválidos en cada tabla.
- **Criterios:** la base de datos rechaza cada caso imposible definido en la sección 9 de `spec.md`.

**T-088 · Pruebas de rendimiento básicas** · E · Depende de: T-090
- **Descripción:** medición de los objetivos de RNF-REN-01 a 05 con los datos de demostración.
- **Criterios:** se cumplen los tiempos objetivo; las mediciones quedan anotadas.

**T-089 · Revisión de experiencia de usuario** · E · Depende de: T-085
- **Descripción:** revisión de consistencia visual, ambos temas, teclado, contraste, confirmaciones, mensajes y compatibilidad con Chrome, Edge y Firefox.
- **Criterios:** se cumplen RNF-USA-01 a 09.

**T-090 · Datos de demostración** · E · Depende de: T-069
- **Descripción:** carga de clientes (VIP y no VIP), productos de las cuatro categorías y cotizaciones en todos los estados, solo en el entorno de desarrollo.
- **Criterios:** la carga es idempotente; nunca se ejecuta en producción; permite recorrer el guion de `docs/demo.md`.

**T-091 · Despliegue en Ubuntu Server** · E · Depende de: T-086, T-008
- **Descripción:** PostgreSQL 18, usuarios de base de datos, aplicación de migraciones, servicio del backend, publicación del frontend, HTTPS y variables de entorno, según DT-08.
- **Criterios:** el sistema funciona por HTTPS en el servidor; el primer arranque crea los datos iniciales; el procedimiento está documentado en el README.

**T-092 · Respaldo y restauración** · E · Depende de: T-091 · RNF-DIS-03
- **Descripción:** respaldo diario automático con retención y procedimiento de restauración.
- **Criterios:** existe al menos un respaldo automático; una restauración en una base de pruebas funciona; el procedimiento está documentado.

**T-093 · Regresión del flujo principal** · E · Depende de: T-091
- **Descripción:** ejecución manual completa de los criterios de la sección 14.1 de `spec.md` sobre el entorno desplegado.
- **Criterios:** los diez pasos del flujo principal funcionan; los resultados quedan registrados.

**T-094 · Corrección de defectos** · E · Depende de: T-086 a T-093
- **Descripción:** corrección de los defectos encontrados en la fase, priorizados por severidad.
- **Criterios:** no quedan defectos críticos ni altos abiertos; cada corrección incluye una prueba que evita su regresión cuando es posible.

### F9 · Documentación final y entrega

**T-095 · README final** · E · Depende de: T-091
- **Descripción:** descripción, requisitos, instalación, variables de entorno, ejecución, pruebas y despliegue.
- **Criterios:** una persona puede levantar el sistema siguiendo solo el README.

**T-096 · `docs/negocio.md`** · E · Depende de: T-093
- **Descripción:** problema de negocio, solución, reglas comerciales y beneficios esperados.
- **Criterios:** coherente con `spec.md` y con el sistema construido.

**T-097 · `docs/uso-ia.md`** · E · Depende de: T-093
- **Descripción:** uso de Gemini dentro del producto (alcance, restricciones, privacidad) y uso de IA durante el desarrollo.
- **Criterios:** distingue claramente ambos usos y es coherente con la sección 11 de `spec.md`.

**T-098 · `docs/demo.md`** · E · Depende de: T-090
- **Descripción:** guion de la demostración paso a paso, con los datos de demostración.
- **Criterios:** recorre el flujo de valor principal, incluye un caso de cálculo verificable a mano y un plan alternativo si Gemini no responde.

**T-099 · Sincronización final de documentos** · E · Depende de: T-095 a T-098
- **Descripción:** revisión de `intent.md`, `spec.md` y `plan.md` contra el código final.
- **Criterios:** ningún documento contradice el sistema; las desviaciones aprobadas durante el desarrollo están reflejadas.

**T-100 · Ensayo de demo y entrega** · E · Depende de: T-099
- **Descripción:** ensayo completo del guion sobre el entorno desplegado y etiqueta `v1.0.0`.
- **Criterios:** el ensayo se completa sin errores; la etiqueta `v1.0.0` apunta al commit entregado.

---

## 7. Estrategia Git

### 7.1 Ramas

| Rama | Propósito | Reglas |
|---|---|---|
| `main` | Versión estable e integrada | Siempre compila y todas las pruebas pasan. No se hacen commits directos. |
| `feat/T-xxx-descripcion` | Nueva funcionalidad de una tarea | Sale de `main` y vuelve a `main`. Ejemplo: `feat/T-058-emitir-cotizacion` |
| `fix/T-xxx-descripcion` | Corrección de un defecto | Igual que las ramas de funcionalidad |
| `docs/…`, `test/…`, `refactor/…`, `chore/…` | Trabajo del tipo correspondiente | Igual que las ramas de funcionalidad |

Es un flujo deliberadamente simple (una rama principal y ramas cortas por tarea), adecuado para un equipo pequeño y sin la complejidad de ramas de desarrollo o de versión.

### 7.2 Flujo de trabajo

1. Actualizar `main` y crear la rama de la tarea.
2. Implementar en pasos pequeños, con un commit por cada paso verificable.
3. Ejecutar todas las pruebas y el linter localmente.
4. Revisar el propio cambio con la lista de revisión técnica de las instrucciones del proyecto (duplicación, complejidad, SOLID, seguridad, rendimiento).
5. Actualizar la documentación afectada en la misma rama.
6. Integrar a `main` con historial lineal (rebase), conservando los commits pequeños.
7. Eliminar la rama.
8. Al cerrar una fase, etiquetar la versión.

### 7.3 Frecuencia de commits

- Al menos un commit por cada paso que deja el sistema compilando y probado.
- Nunca un commit que rompa la compilación en `main`.
- Preferir varios commits pequeños por tarea antes que uno grande.
- Integrar a `main` al terminar cada tarea, no al terminar una fase.

### 7.4 Convenciones

**Formato del mensaje:**

```
<tipo>(<módulo>): <descripción en imperativo> (T-xxx)
```

Ejemplos:

```
feat(cotizaciones): asignar consecutivo al emitir (T-057)
test(motor): agregar casos límite de tramos (T-050)
docs(readme): documentar variables de entorno (T-095)
```

| Tipo | Uso |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de errores |
| `refactor` | Mejora interna sin cambio de comportamiento |
| `docs` | Documentación |
| `test` | Pruebas |
| `chore` | Mantenimiento, configuración, dependencias |

**Módulos permitidos:** `infra`, `auth`, `usuarios`, `auditoria`, `config`, `clientes`, `categorias`, `productos`, `motor`, `cotizaciones`, `documentos`, `ia`, `dashboard`, `ui`, `db`, `docs`.

**Etiquetas de versión:**

| Etiqueta | Momento |
|---|---|
| `v0.1.0` | Fin de F1 · Fundaciones |
| `v0.2.0` | Fin de F2 · Identidad, auditoría y configuración |
| `v0.3.0` | Fin de F3 · Datos maestros |
| `v0.4.0` | Fin de F4 · Núcleo |
| `v0.5.0` | Fin de F5 · **MVP demostrable** |
| `v0.6.0` | Fin de F6 · Asistente IA |
| `v0.7.0` | Fin de F7 · Funcionalidad completa |
| `v1.0.0-rc.1` | Fin de F8 · Candidata a entrega |
| `v1.0.0` | Fin de F9 · Entrega |

**Seguridad en el repositorio:** archivos de entorno ignorados desde el primer commit; solo se versiona `.env.example` sin valores; si un secreto llegara a subirse, se considera comprometido y se reemplaza de inmediato.

---

## 8. Estrategia de base de datos

### 8.1 Migraciones EF Core

- **Única vía de cambio:** toda modificación del esquema se hace con una migración de EF Core. Nunca se altera la base de datos a mano.
- **Una migración por cambio coherente**, con nombre descriptivo (por ejemplo, `CreateQuotesAndQuoteLines`), creada en la misma tarea que la entidad que la necesita.
- **Restricciones en la migración:** los `CHECK`, índices únicos, índices parciales, unicidad diferida y valores por defecto de la sección 9 de `spec.md` se declaran en la migración, no se dejan solo en el código.
- **Revisión del SQL:** antes de integrar una migración, se genera y revisa el script SQL resultante.
- **Inmutabilidad:** una migración integrada a `main` nunca se edita; cualquier corrección es una migración nueva.
- **Aplicación:**
  - Desarrollo y pruebas: automática al arrancar o mediante comando.
  - Producción: con un script SQL idempotente generado desde las migraciones y ejecutado con el usuario propietario del esquema. La aplicación en producción no tiene permisos para alterar el esquema.

### 8.2 Datos iniciales

Datos necesarios para que el sistema funcione en cualquier entorno. Se crean con un proceso **idempotente al arrancar**, porque el primer Administrador depende de variables de entorno.

| Dato | Origen | Tarea |
|---|---|---|
| Usuario de sistema | Valores fijos | T-018 |
| Primer Administrador (`must_change_password = true`) | Variables de entorno | T-018 |
| Fila de configuración con valores por defecto y datos de TechSolutions CR | Valores fijos | T-034 |
| Cuatro categorías iniciales | Valores fijos | T-041 |

Ejecutar el arranque varias veces no duplica ni sobrescribe datos existentes.

### 8.3 Seeds de demostración

- Clientes VIP y no VIP, productos en las cuatro categorías y cotizaciones en todos los estados, incluidos casos que activan cada tramo de descuento (T-090).
- **Solo se ejecutan en el entorno de desarrollo**, con una verificación explícita del entorno.
- Son idempotentes y sirven de base para el guion de `docs/demo.md` y las pruebas de rendimiento.

### 8.4 Base de datos de pruebas

- Base PostgreSQL 18 separada, exclusiva para las pruebas de integración.
- Se recrea con las mismas migraciones que producción, para que las pruebas validen también el esquema real.
- No se usan bases en memoria ni SQLite, porque no soportan las características de PostgreSQL de las que depende el sistema (`uuidv7()`, `xmin`, índices parciales, unicidad diferida, bloqueo de filas).

### 8.5 Respaldo

- **Frecuencia:** diaria, automática, con `pg_dump` en formato comprimido.
- **Retención:** al menos los últimos 7 respaldos diarios.
- **Ubicación:** fuera del directorio de datos de PostgreSQL y, preferentemente, copiados a un almacenamiento distinto del servidor.
- **Verificación:** una restauración de prueba en una base separada antes de la entrega y después de cada cambio en el procedimiento.
- **Documentación:** procedimiento de respaldo y restauración en el README (T-092).

---

## 9. Estrategia de pruebas

### 9.1 Pruebas unitarias

**Alcance:** el dominio, que es puro y no depende de infraestructura.

| Área | Qué se prueba |
|---|---|
| Value objects | Construcción válida e inválida de Dinero, Cantidad, Porcentaje, Correo, Teléfono, Número de cotización y Vigencia |
| Política comercial | Todas las reglas de RN-27 |
| Motor de cálculo | Todos los casos críticos de la sección 9.4 |
| Máquina de estados | Cada transición válida y el rechazo de cada transición inválida |
| Agregado Cotización | Suma de cantidades con producto repetido, renumeración, bloqueo tras emitir, congelamiento |
| Duplicación | Copia de datos, omisión de inactivos, rechazo con cliente inactivo |
| Vigencia y vencimiento | Bordes del día en hora de Costa Rica con reloj fijo |

### 9.2 Pruebas de integración

**Alcance:** API completa contra PostgreSQL 18 real.

| Área | Qué se prueba |
|---|---|
| Autenticación | Login, mensajes genéricos, *401*, sello de seguridad, cambio obligatorio, limitador |
| Autorización | *403* para cada función de Administrador usada por Ventas; propiedad de cotizaciones |
| Auditoría | Evento por cada acción definida; atomicidad (cambio y evento juntos o ninguno) |
| Concurrencia | *409* ante actualizaciones simultáneas |
| Restricciones | Rechazo de datos inválidos por la base de datos |
| Emisión | Consecutivo sin saltos ni duplicados bajo concurrencia; congelamiento ante cambios posteriores |
| Ciclo de vida | Aceptación fuera de vigencia; vencimiento idempotente |
| Asistente | Comportamiento con Gemini simulado (éxito, error, tiempo excedido, límite) |
| Dashboard | Cada indicador con datos conocidos |

### 9.3 Pruebas manuales

| Prueba | Momento |
|---|---|
| Recorrido de los criterios de aceptación de cada RF terminado | Al cerrar cada tarea |
| Flujo principal completo | Al cerrar F5, F7 y F8 |
| Revisión de UX en ambos temas y con teclado | F8 (T-089) |
| Compatibilidad con Chrome, Edge y Firefox | F8 (T-089) |
| Funcionamiento sin Gemini (sin clave y con la red bloqueada) | F6 y F8 |
| Despliegue, respaldo y restauración | F8 (T-091, T-092) |
| Ensayo completo de la demo | F9 (T-100) |

### 9.4 Casos críticos del motor de cálculo

Configuración inicial: tramo 1 > ₡500.000 → 5 %; tramo 2 > ₡1.000.000 → 10 %; VIP 3 %; IVA 13 %. Redondeo a 2 decimales con los valores medios alejándose de cero.

| # | Caso | Entrada | Resultado esperado |
|---|---|---|---|
| C-01 | Borrador vacío | Sin líneas | Todos los montos en ₡0,00 |
| C-02 | Debajo del tramo 1 | Subtotal ₡300.000,00, no VIP | 0 %; IVA ₡39.000,00; total ₡339.000,00 |
| C-03 | Borde exacto del tramo 1 | Subtotal ₡500.000,00, no VIP | 0 %; IVA ₡65.000,00; total ₡565.000,00 |
| C-04 | Justo sobre el tramo 1 | Subtotal ₡500.000,01, no VIP | 5 %; descuento ₡25.000,00; base ₡475.000,01; IVA ₡61.750,00; total ₡536.750,01 |
| C-05 | Borde exacto del tramo 2 | Subtotal ₡1.000.000,00, no VIP | 5 %; descuento ₡50.000,00; base ₡950.000,00; IVA ₡123.500,00; total ₡1.073.500,00 |
| C-06 | Justo sobre el tramo 2 | Subtotal ₡1.000.000,01, no VIP | 10 %; descuento ₡100.000,00; base ₡900.000,01; IVA ₡117.000,00; total ₡1.017.000,01 |
| C-07 | VIP sin tramo | Subtotal ₡200.000,00, VIP | 3 %; descuento ₡6.000,00; base ₡194.000,00; IVA ₡25.220,00; total ₡219.220,00 |
| C-08 | VIP con tramo 1 | Subtotal ₡800.000,00, VIP | 8 %; descuento ₡64.000,00; base ₡736.000,00; IVA ₡95.680,00; total ₡831.680,00 |
| C-09 | VIP con tramo 2 (caso de referencia) | Subtotal ₡1.200.000,00, VIP | 13 %; descuento ₡156.000,00; base ₡1.044.000,00; IVA ₡135.720,00; total ₡1.179.720,00 |
| C-10 | Redondeo de línea | 1,5 × ₡1.000,01 | Total de línea ₡1.500,02 |
| C-11 | Redondeo del descuento | Subtotal ₡500.000,10, no VIP | Descuento ₡25.000,01 (25.000,005 redondeado) |
| C-12 | Redondeo del IVA | Base ₡100.000,05, sin descuento | IVA ₡13.000,01 (13.000,0065 redondeado) |
| C-13 | Precio cero | Todas las líneas con precio ₡0 | Total ₡0,00; válido |
| C-14 | Mezcla con precio cero | 1 × ₡600.000 + 3 × ₡0 | Subtotal ₡600.000,00; 5 % aplicado |
| C-15 | Cantidades decimales | 2,25 × ₡400.000,00 | Total de línea ₡900.000,00; 5 % aplicado |
| C-16 | Varias líneas suman al tramo | 3 × ₡200.000 + 1 × ₡450.000 | Subtotal ₡1.050.000,00; 10 % aplicado |
| C-17 | Parámetros modificados | Tramo 1 en ₡400.000 → 6 %; subtotal ₡450.000,00 | 6 %; el motor usa la política recibida, no valores fijos |
| C-18 | IVA modificado | IVA 0 %; subtotal ₡100.000,00 | IVA ₡0,00; total ₡100.000,00 |
| C-19 | Coherencia del desglose | Cualquier caso anterior | base = subtotal − descuento; total = base + IVA; % total = % tramo + % VIP |

Estos casos se implementan en T-050, se repiten como pruebas de integración de emisión en T-059 y el caso C-09 se usa en la demo como verificación manual.

---

## 10. Riesgos

| ID | Riesgo | Tipo | Prob. | Impacto | Mitigación |
|---|---|---|---|---|---|
| R-01 | El tiempo disponible no alcanza para toda la V1 | Proyecto | Media | Alto | Orden por valor; MVP demostrable al cerrar F5; IA y vistas de supervisión después; prioridades E/I/D en el backlog. |
| R-02 | Incompatibilidad del proveedor de EF Core con PostgreSQL 18 | Técnico | Baja | Alto | Verificación en T-007, al inicio del proyecto. |
| R-03 | Errores de cálculo o redondeo | Funcional | Media | Alto | Motor puro con 19 casos críticos; restricciones aritméticas en la base de datos; misma regla de redondeo en backend y base de datos. |
| R-04 | Duplicados o saltos en el consecutivo | Técnico | Baja | Alto | Bloqueo de fila en la transacción de emisión; prueba de concurrencia en T-059. |
| R-05 | Errores de zona horaria en vigencia, vencimiento o año del consecutivo | Técnico | Media | Medio | Reloj inyectable; fechas de negocio siempre en hora de Costa Rica; pruebas en los bordes del día. |
| R-06 | El PDF no coincide con la vista previa | Técnico | Media | Medio | Plantilla única; decisión DT-05 tomada en F0; pruebas en T-074. |
| R-07 | Gemini no disponible, cuota agotada, cambios en la API o costos | Externo | Media | Medio | Abstracción con modo no disponible; tiempo límite; límite por usuario; modelo configurable; construido después del MVP. |
| R-08 | Exposición de datos personales a Gemini | Seguridad | Baja | Alto | Contexto mínimo construido en el backend; prueba que verifica la exclusión de correos y teléfonos. |
| R-09 | Exposición de secretos en el repositorio | Seguridad | Baja | Alto | `.gitignore` desde T-001; variables de entorno; revisión del historial en T-086. |
| R-10 | Sesión vencida durante la edición de una cotización | UX | Media | Bajo | Los borradores se guardan en el servidor con cada cambio; aviso claro al volver a iniciar sesión. |
| R-11 | Conflictos de edición concurrente | Técnico | Baja | Medio | Concurrencia optimista con mensaje claro (T-021). |
| R-12 | La experiencia no alcanza el nivel profesional esperado | UX | Media | Medio | Sistema visual construido en F1; revisión dedicada en T-089. |
| R-13 | La documentación queda desactualizada | Proceso | Media | Medio | Actualización en la misma rama de cada tarea; sincronización final en T-099. |
| R-14 | Problemas en el despliegue (HTTPS, permisos, servicio) | Técnico | Media | Medio | Despliegue en F8 con margen antes de la entrega; procedimiento documentado; ensayo en el entorno real. |
| R-15 | Pruebas de integración lentas o frágiles | Técnico | Media | Bajo | Base de pruebas dedicada que se recrea; pruebas independientes entre sí. |
| R-16 | Ampliación de alcance durante el desarrollo | Proceso | Media | Medio | Toda funcionalidad nueva pasa primero por `spec.md`; este plan no agrega alcance. |

---

## 11. Definición de completado

### 11.1 Tarea

Una tarea está completada cuando:

1. Cumple todos sus criterios de aceptación y los de los RF que cubre.
2. Compila sin errores y el linter no reporta problemas.
3. Tiene las pruebas que le corresponden según la sección 9, y todas las pruebas del proyecto pasan.
4. Incluye validaciones, manejo de errores y casos límite.
5. Registra los eventos de auditoría que le correspondan.
6. No introduce secretos, código duplicado ni dependencias sin justificar.
7. Fue revisada con la lista de revisión técnica de las instrucciones del proyecto.
8. Su interfaz, si la tiene, funciona en modo claro y oscuro.
9. La documentación afectada está actualizada.
10. Está integrada a `main` con commits que siguen la convención.
11. Se explicaron los cambios, los requisitos cubiertos, cómo probar y los riesgos o mejoras futuras.

### 11.2 Fase

Una fase está completada cuando:

1. Todas sus tareas esenciales e importantes están completadas.
2. Se cumple su criterio de finalización (sección 5).
3. El flujo principal construido hasta ese momento funciona de punta a punta.
4. No hay defectos críticos ni altos abiertos.
5. La documentación refleja el estado del sistema.
6. La versión está etiquetada en Git.

### 11.3 Proyecto

El proyecto está completado cuando:

1. Las fases F0 a F9 están completadas.
2. Se cumplen los 24 criterios de aceptación de la sección 14 de `spec.md`.
3. Están implementados los 52 RF esenciales e importantes (RF-DSH-03 es opcional).
4. Se cumplen los 46 RNF y las 35 reglas de negocio vigentes.
5. Los 19 casos críticos del motor pasan.
6. El sistema funciona desplegado en Ubuntu Server con HTTPS y respaldo diario verificado.
7. Los siete documentos del proyecto están completos y sincronizados con el código.
8. La demo fue ensayada sin errores.
9. La versión `v1.0.0` está etiquetada.

---

## 12. Roadmap final

Tamaño relativo de cada fase: **S** pequeña, **M** mediana, **L** grande.

| Orden | Fase | Tamaño | Tareas | Hito | Etiqueta |
|---|---|---|---|---|---|
| 1 | F0 · Preparación | S | T-001 a T-003 | Primer commit y decisiones técnicas cerradas | — |
| 2 | F1 · Fundaciones | M | T-004 a T-016 | Stack conectado de punta a punta | `v0.1.0` |
| 3 | F2 · Identidad, auditoría y configuración | L | T-017 a T-037 | Usuarios autenticados y auditoría operando | `v0.2.0` |
| 4 | F3 · Datos maestros | M | T-038 a T-047 | Clientes y catálogo completos | `v0.3.0` |
| 5 | F4 · Núcleo | L | T-048 a T-069 | Ciclo de vida completo con cálculo exacto | `v0.4.0` |
| 6 | F5 · Documentos | S | T-070 a T-074 | **MVP demostrable** | `v0.5.0` |
| 7 | F6 · Asistente IA | M | T-075 a T-081 | Redacción asistida con Gemini | `v0.6.0` |
| 8 | F7 · Dashboard y auditoría | S | T-082 a T-085 | **Funcionalidad completa** | `v0.7.0` |
| 9 | F8 · QA, hardening y despliegue | M | T-086 a T-094 | Sistema desplegado y verificado | `v1.0.0-rc.1` |
| 10 | F9 · Documentación final y entrega | S | T-095 a T-100 | **Entrega final** | `v1.0.0` |

**Secuencia completa:**

```
Primer commit
  → Decisiones técnicas cerradas
  → Backend, frontend y base de datos conectados            [v0.1.0]
  → Login, roles, usuarios, auditoría y configuración       [v0.2.0]
  → Clientes, categorías y productos                        [v0.3.0]
  → Motor de cálculo verificado
  → Borradores → emisión → cierre → vencimiento → duplicación [v0.4.0]
  → Vista previa y PDF  ═══ MVP DEMOSTRABLE ═══              [v0.5.0]
  → Asistente Gemini                                        [v0.6.0]
  → Auditoría consultable y dashboard ═══ V1 COMPLETA ═══    [v0.7.0]
  → Seguridad, rendimiento, UX, despliegue, respaldo        [v1.0.0-rc.1]
  → Documentación final y ensayo de demo  ═══ ENTREGA ═══    [v1.0.0]
```

**Punto de control de alcance:** al cerrar F5, se evalúa el tiempo restante. Si no alcanza para todo lo demás, se priorizan F8 y F9 (despliegue, calidad y entrega) sobre F6 y F7, y se aplica el orden de prioridad E → I → D del backlog. El MVP demostrable cumple por sí solo el flujo de valor principal de `spec.md`.

---

## 13. Estado de la entrega v1.0.0 y pendientes

Corte al 25 de noviembre de 2026. Se aplicó el punto de control de alcance de la sección 12: se priorizó el flujo de valor principal (F0 a F5), el asistente de IA (F6) y la documentación (F9). La etiqueta `v1.0.0` marca ese corte; **no** cumple todavía la definición de completado del proyecto (sección 11.3), porque faltan las tareas de abajo.

### 13.1 Completado

- **F0–F1:** documentos base, solución por capas, configuración y secretos por variables de entorno, errores (ProblemDetails) y logging, EF Core + PostgreSQL, reloj y zona horaria, frontend con cliente API y formatos regionales.
- **F2:** usuarios, contraseñas, JWT con validación por solicitud, roles y propiedad, cambio obligatorio de contraseña, limitador de inicio de sesión, HTTPS y CORS, auditoría automática y concurrencia optimista; interfaz de inicio/cierre de sesión y cambio de contraseña. API de usuarios y de configuración.
- **F3:** clientes, categorías y productos (API e interfaz).
- **F4:** motor de cálculo con los 19 casos críticos (T-050), agregado Cotización, máquina de estados, consecutivo, emisión, aceptar/rechazar/cancelar, vencimiento automático, duplicación, consulta e interfaz completa (editor, acciones, detalle e historial, duplicación).
- **F5:** documento comercial en HTML y PDF (QuestPDF) con su interfaz.
- **F6:** asistente con Gemini (T-075 a T-080): contexto mínimo, filtro de datos sensibles, tiempo límite, límite por usuario y auditoría.
- **F8:** datos de demostración solo para Development (T-090).
- **F9:** `README.md`, `docs/negocio.md`, `docs/uso-ia.md` y `docs/demo.md` (T-095 a T-098).

### 13.2 Pendiente

| Tarea | Qué falta | Motivo |
|---|---|---|
| T-008 | Usuarios de base de datos separados (migraciones vs. aplicación) con permisos mínimos | Solo aplica al servidor; se hará junto con el despliegue (T-091). En desarrollo se usa una sola cadena de conexión |
| T-016 | Integración continua | Prioridad D; se pospuso a favor del flujo principal según el punto de control de alcance |
| T-031 | Pantalla de perfil | Prioridad I; la API de sesión existe, falta la interfaz. Se priorizaron las pantallas del flujo de cotización |
| T-033 | Pantalla de gestión de usuarios | Prioridad I; la API (`UsersController`) está hecha. Hoy solo existe el Administrador inicial, creado por el seeding |
| T-037, T-047, T-069, T-074, T-081 | Pruebas de integración por fase (API + PostgreSQL real) | Solo existe el proyecto de pruebas de dominio. Falta crear el proyecto de integración (DT-06); el riesgo mayor, el motor de cálculo, ya está cubierto por T-050 |
| T-059 | Repetir C-01 a C-19 como pruebas de integración de emisión | Depende del proyecto de pruebas de integración anterior |
| T-082, T-083 | Consulta de auditoría (API e interfaz) | Prioridad I (F7); se pospuso por tiempo. Los eventos ya se registran, el historial de cada cotización se ve en su detalle y el dashboard muestra los últimos 10 (T-084, T-085) |
| T-086 | Revisión de seguridad formal | Requiere el sistema completo; se hicieron controles parciales: sin secretos en el repositorio, 403 por rol y errores sin detalles internos |
| T-087 | Verificación de restricciones de la base de datos | Depende de las pruebas de integración (T-069) |
| T-088 | Pruebas de rendimiento | Deben medirse sobre el entorno desplegado |
| T-089 | Revisión de experiencia de usuario | Depende de que existan todas las pantallas (faltan usuarios, perfil y auditoría) |
| T-091 | Despliegue en Ubuntu Server (DT-08 sin decidir) | No hubo servidor disponible en el plazo; el README documenta los requisitos ya resueltos en el código |
| T-092 | Respaldo y restauración automáticos | Depende del despliegue |
| T-093 | Regresión del flujo principal en el entorno desplegado | Depende del despliegue; el flujo se verificó en desarrollo |
| T-094 | Corrección de defectos de F8 | Depende de T-086 a T-093 |
| T-099 | Sincronización final de documentos | Parcial: Anexo A actualizado y esta sección agregada; falta revisar `spec.md` contra el código cuando se cierren las tareas anteriores |
| T-100 | Ensayo de la demo sobre el entorno desplegado | Depende del despliegue; el guion se puede ensayar en desarrollo con los datos de demostración |

---

## Anexo A. Registro de decisiones técnicas

Decisiones de implementación que `spec.md` deja abiertas. Se cierran en T-003 y se registran aquí. Ninguna puede contradecir `spec.md` ni agregar alcance.

| ID | Decisión | Opciones a evaluar | Criterio de elección | Elegida | Fecha |
|---|---|---|---|---|---|
| DT-01 | Organización de proyectos del backend | Un proyecto por capa (Dominio, Aplicación, Infraestructura, API) + pruebas; o un proyecto de API con carpetas por capa | Que el compilador impida que el dominio dependa de la infraestructura, sin multiplicar proyectos | Un proyecto por capa (Domain, Application, Infrastructure, Api) + `QuoteFlow.Domain.Tests` | 25/11/2026 |
| DT-02 | Librería de componentes de interfaz y temas | Librería de componentes accesible con soporte de temas; o componentes propios sobre una base de estilos | Aspecto profesional, modo oscuro nativo, accesibilidad y bajo peso | Componentes propios con variables CSS (tokens y temas claro/oscuro); sin librería de UI. Control total del aspecto y cero dependencias | 24/11/2026 |
| DT-03 | Gestión del estado de servidor en el frontend | Librería de caché de consultas; o capa propia | Caché, reintentos e invalidación sin código propio extenso | TanStack Query: caché, invalidación y reintentos estándar con poco código | 24/11/2026 |
| DT-04 | Formularios y validación en el frontend | Librería de formularios con validación por esquema; o validación propia | Tipado estricto y mensajes junto al campo | React Hook Form + Zod: validación por esquema mientras se escribe, con tipos inferidos | 24/11/2026 |
| DT-05 | Generación de PDF | Convertir la plantilla HTML con un navegador sin interfaz; o librería .NET de generación de PDF con plantilla propia | Que el PDF sea idéntico a la vista previa (RF-DOC-02), licencia compatible y tiempo menor a 3 s | QuestPDF (licencia Community) con la misma estructura que la vista previa HTML | 25/11/2026 |
| DT-06 | Herramientas de pruebas | Framework de pruebas .NET con servidor de pruebas de ASP.NET Core y PostgreSQL 18 real; framework de pruebas para Vite | Ejecución con un solo comando y compatibilidad con PostgreSQL real | xUnit para .NET y Vitest para el frontend; el proyecto de integración con PostgreSQL real queda pendiente (sección 13) | 25/11/2026 |
| DT-07 | Ejecución del proceso de vencimiento | Servicio en segundo plano dentro del backend; o tarea programada del sistema operativo | Simplicidad, recuperación tras reinicio e idempotencia | Servicio en segundo plano en el backend (`QuoteExpirationService`, cada hora, configurable) | 25/11/2026 |
| DT-08 | Publicación en Ubuntu Server | Servidor web como proxy inverso con HTTPS frente al backend y al frontend estático; o el backend sirviendo también el frontend | Simplicidad operativa, certificados HTTPS y CORS coherente | Pendiente (sección 13) | — |

---

## Anexo B. Trazabilidad de requisitos a tareas

| Requisito | Tareas |
|---|---|
| RF-AUT-01 | T-023, T-029 |
| RF-AUT-02 | T-029 |
| RF-AUT-03 | T-024 |
| RF-AUT-04 | T-013, T-025 |
| RF-AUT-05 | T-031 |
| RF-AUT-06 | T-030 |
| RF-AUT-07 | T-018, T-026, T-030 |
| RF-USR-01 a 05 | T-032, T-033 |
| RF-CLI-01, 02, 04, 05, 06 | T-039, T-040 |
| RF-CLI-03 | T-040, T-068 |
| RF-CAT-01 a 04 | T-041, T-042, T-043 |
| RF-PRO-01 a 04 | T-044, T-045, T-046 |
| RF-COT-01 | T-053, T-064 |
| RF-COT-02, 03 | T-054, T-064 |
| RF-COT-04, 05 | T-055, T-064 |
| RF-COT-06 | T-049, T-050, T-056, T-064 |
| RF-COT-07 | T-057, T-058, T-059, T-065 |
| RF-COT-08 a 10 | T-060, T-065 |
| RF-COT-11 | T-061 |
| RF-COT-12 | T-062, T-067 |
| RF-COT-13, 14 | T-063, T-066 |
| RF-DOC-01 | T-070, T-071, T-073 |
| RF-DOC-02 | T-070, T-072, T-073 |
| RF-IA-01, 02 | T-077, T-079, T-080 |
| RF-IA-03 | T-075, T-079, T-080, T-081 |
| RF-CFG-01, 02 | T-034, T-035, T-036 |
| RF-CFG-03 | T-034, T-035, T-036, T-068 |
| RF-AUD-01 | T-019, T-020 |
| RF-AUD-02 | T-082, T-083 |
| RF-DSH-01 a 03 | T-084, T-085 |
| RNF-SEG | T-005, T-008, T-022, T-024, T-027, T-028, T-077, T-078, T-086 |
| RNF-DAT | T-007, T-009, T-021, T-049, T-087 |
| RNF-REN | T-056, T-072, T-076, T-088 |
| RNF-DIS | T-061, T-075, T-092 |
| RNF-USA | T-011, T-014, T-064, T-089 |
| RNF-MAN | T-004, T-006, T-007, T-050, T-015 |
| RNF-OPE | T-018, T-091, T-095 |
| RNF-PRO | T-001, T-002, T-099 |