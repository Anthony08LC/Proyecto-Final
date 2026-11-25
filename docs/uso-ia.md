# QuoteFlow AI — Uso de inteligencia artificial

En este proyecto la IA tiene **dos usos distintos, que no se mezclan**:

| | Parte A · Gemini en el producto | Parte B · Claude Code en el desarrollo |
|---|---|---|
| Quién la usa | Los usuarios de QuoteFlow (Ventas y Administrador) | El desarrollador del proyecto |
| Para qué | Redactar textos comerciales de una cotización | Escribir documentación, código y pruebas |
| Dónde corre | Backend de QuoteFlow → API de Google Gemini | Editor del desarrollador (VS Code) |
| Forma parte del sistema entregado | Sí | No |

---

## Parte A · Google Gemini dentro del producto

Referencia: sección 11 de [spec.md](../spec.md).

### A.1 Qué hace

Es un **asistente de redacción comercial** que trabaja sobre cuatro campos de un borrador:

- Introducción
- Resumen ejecutivo
- Observaciones
- Condiciones comerciales

Para cada campo ofrece dos acciones: **Generar** (redactar desde cero) y **Mejorar** (reescribir lo que el usuario ya escribió). Técnicamente es una sola operación, `POST /api/quotes/{id}/assistant`, parametrizada por tipo de texto y acción. `GET /api/assistant/status` informa si está disponible y cuántas solicitudes le quedan al usuario.

### A.2 Qué no puede hacer

Por diseño, la IA **no** modifica precios, no calcula descuentos, no cambia impuestos, no altera totales y no toma decisiones financieras.

- Los montos que recibe ya vienen calculados por el **motor de cálculo del backend**; si un texto los menciona, provienen del motor.
- La sugerencia solo es texto: el usuario la **acepta, edita o descarta**. Nada se guarda sin su aceptación.
- Solo funciona sobre cotizaciones en **Borrador**. Al emitir, el texto aceptado queda congelado con el resto de la cotización.

### A.3 Privacidad: qué se envía a Gemini

El contexto se arma en el backend (`AssistantContextBuilder`) con lo mínimo necesario:

| Se envía | No se envía |
|---|---|
| Nombre de la empresa que cotiza | Correos y teléfonos del cliente o de la empresa |
| Nombre y empresa del cliente, si es VIP | Cédulas, direcciones, notas internas del cliente |
| Líneas (producto, descripción recortada, cantidad, precio) | Datos de otros clientes o cotizaciones |
| Desglose calculado por el motor y vigencia | Credenciales o datos del usuario |
| Texto actual del campo (para "Mejorar") | |

Además, todos los textos libres pasan por `SensitiveDataFilter`, que reemplaza correos y teléfonos por `[correo omitido]` y `[teléfono omitido]` antes del envío.

### A.4 Controles

| Control | Valor |
|---|---|
| Clave | `Gemini__ApiKey`, solo como variable de entorno o *user-secret*; nunca en el repositorio |
| Modelo | `gemini-2.5-flash` (configurable con `Gemini__Model`) |
| Tiempo límite | 20 segundos por solicitud |
| Límite de uso | 30 solicitudes por hora por usuario (ventana móvil, contada sobre la auditoría: sobrevive a reinicios) |
| Auditoría | Cada sugerencia generada queda registrada como evento |

### A.5 Si Gemini falla

El sistema **funciona completamente sin Gemini**; solo se pierde la redacción asistida.

| Situación | Respuesta |
|---|---|
| Sin clave configurada | Botones del asistente deshabilitados con la explicación; HTTP 503 si se llama a la API |
| Error del servicio o respuesta inválida | Mensaje claro; el borrador no cambia; se registra en el log |
| Más de 20 s | HTTP 504 con mensaje; el usuario puede reintentar |
| Límite alcanzado | Mensaje con la hora a partir de la cual puede volver a usarlo |
| Cotización no está en Borrador o sin permiso | Solicitud rechazada con el error correspondiente |

En ningún caso un fallo del asistente impide guardar, emitir o consultar cotizaciones.

---

## Parte B · Claude Code durante el desarrollo

Claude Code (Anthropic) se usó como **asistente de programación** dentro de VS Code. No forma parte del producto: no se llama desde el sistema, no recibe datos de clientes reales y nada del sistema depende de él.

### B.1 Método de trabajo

1. **Primero los documentos.** Se redactaron [intent.md](../intent.md) (por qué), [spec.md](../spec.md) (qué) y [plan.md](../plan.md) (cómo, con tareas T-001 a T-100). Esos documentos fueron el contexto con el que se trabajó con Claude Code en cada sesión.
2. **Una tarea del plan por vez.** Cada pedido a Claude Code se acotó a una o pocas tareas del backlog, con sus criterios de aceptación y los requisitos (RF, RN, RNF) que cubren.
3. **Revisión humana.** El desarrollador revisó, probó y aprobó cada cambio antes de hacer commit. Las decisiones de alcance y de diseño quedaron en manos humanas; si algo no estaba en `spec.md`, primero se actualizaba el documento.
4. **Trazabilidad en Git.** Los commits siguen *Conventional Commits* en español, citan la tarea (por ejemplo `feat(documentos): … (T-070, T-071, T-072)`) e incluyen la línea `Co-Authored-By: Claude` cuando Claude Code participó.

### B.2 En qué ayudó

- Redacción y revisión de los documentos del proyecto, incluida esta documentación final.
- Estructura del backend por capas, entidades, casos de uso, controladores y migración de EF Core.
- Frontend en React: páginas de catálogo, clientes, editor de cotizaciones sin ventanas modales, detalle, historial y documento.
- Pruebas del motor de cálculo: los 19 casos críticos C-01 a C-19 de la sección 9.4 del plan.
- Datos de demostración con un caso por cada tramo de descuento y por cada estado.
- Integración con Gemini con las restricciones de privacidad y de uso de la Parte A.

### B.3 Salvaguardas

- **El motor de cálculo se validó contra casos escritos a mano** en `plan.md` antes de implementarlo; las pruebas comparan contra esos valores, no contra lo que devolviera el código.
- **Sin secretos en el contexto:** las claves y contraseñas se manejan con variables de entorno y *user-secrets*; el repositorio solo contiene valores vacíos y plantillas.
- **Configuración personal fuera del repositorio:** `.claude/settings.local.json` está en `.gitignore`.
- **Lo pendiente está declarado:** lo que no se llegó a construir o verificar está listado en la sección 13 de `plan.md`, sin presentarlo como terminado.

---

## Parte C · Ciclos de trabajo con el agente (evidencia)

Cada ciclo sigue el mismo formato que pide la consigna: **tarea → prompt → resultado del agente → revisión humana → corrección → commit**. Los hashes corresponden al historial de `main`.

### Ciclo 1 · Análisis de reglas antes de construir (spec.md)

| Paso | Detalle |
|---|---|
| Tarea | Validar el contexto del proyecto antes del diseño de dominio |
| Prompt (extracto) | *"Analiza todo el contexto disponible… identificar posibles problemas, omisiones, riesgos, contradicciones o mejoras antes de iniciar el desarrollo."* |
| Resultado del agente | Detectó que RN-01 a RN-04 admitían tres interpretaciones (tramos excluyentes, acumulados o compuestos) con **₡31.188 de diferencia** en una misma cotización de ₡1.200.000 VIP, y que el tope de 15 % (RN-04) nunca se activaría con la lectura más natural |
| Revisión humana | El autor decidió: tramos **no acumulativos**, VIP suma 3 puntos, descuento máximo 13 % y **derogar RN-04**; además, renombrar "Aprobada" a "Aceptada" |
| Corrección | Catálogo de reglas reescrito en `spec.md` (sección 7) y casos C-01 a C-19 calculados a mano en `plan.md` (sección 9.4) |
| Evidencia | `spec.md` v1.0; pruebas `cba8ed13` (`test(motor): cubrir los casos críticos C-01 a C-19`) |

### Ciclo 2 · Error al arrancar la API: `ck_system_settings_singleton`

| Paso | Detalle |
|---|---|
| Tarea | T-018 / T-034: datos iniciales al conectar la API con la base en la VM |
| Prompt (extracto) | El autor pegó el log del arranque: *"new row for relation system_settings violates check constraint ck_system_settings_singleton"* |
| Resultado del agente | Leyó la entidad, su configuración de EF Core y el seeder. Diagnóstico: la columna sombra `singleton` tiene valor por defecto `true` en la base, pero EF Core insertaba `false` |
| Revisión humana | El autor volvió a ejecutar la API y verificó en `psql` que existieran los 2 usuarios, las 4 categorías y la configuración con IVA 13 %, ₡500.000, ₡1.000.000 y 30 días |
| Corrección | Una línea en `DatabaseSeeder` que asigna `Singleton = true` explícitamente, sin cambiar la migración |
| Commit | `4c02b71d` · `fix(db): asignar singleton al crear la configuración inicial` |

### Ciclo 3 · Las pruebas de integración borraron la base de pruebas

| Paso | Detalle |
|---|---|
| Tarea | T-037 / T-059: pruebas de integración de seguridad y emisión |
| Prompt (extracto) | *"Crea tests/QuoteFlow.Api.Tests con WebApplicationFactory contra la base de ConnectionStrings__QuoteFlowTests… login correcto e incorrecto, 401, 403, emisión con número consecutivo y congelamiento ante un cambio de precio."* |
| Resultado del agente | La primera versión usaba `EnsureDeletedAsync()`: eliminó `quoteflow_tests`, pero el usuario `quoteflow_owner` no tiene permiso `CREATEDB` para recrearla. El propio agente lo reportó |
| Revisión humana | El autor comprobó con `psql` que la base había desaparecido, la recreó con el superusuario y **verificó que la base principal `quoteflow` seguía intacta** |
| Corrección | La fábrica ya no borra la base: vacía el esquema `public` y aplica las migraciones; antes de limpiar verifica servidor, puerto y nombre de la base de pruebas |
| Commit | `c39146b7` · `test(api): pruebas de integración de seguridad y emisión (T-037, T-059)` |

### Ciclo 4 · Búsqueda de productos que perdía los espacios

| Paso | Detalle |
|---|---|
| Tarea | T-046: catálogo de productos en la interfaz |
| Resultado del agente | La búsqueda recortaba el texto mientras se escribía, así que no se podían buscar nombres de varias palabras |
| Revisión humana | Detectado por el autor al probar la pantalla en el navegador |
| Corrección | Conservar el texto tal como se escribe y recortarlo solo al consultar |
| Commit | `62697b71` · `fix(productos): conservar los espacios de la búsqueda mientras se escribe (T-046)` |

### Ciclo 5 · Infraestructura: la API no llegaba a PostgreSQL

| Paso | Detalle |
|---|---|
| Tarea | Conectar el backend con la VM de PostgreSQL 18 |
| Resultado del agente | Guió el diagnóstico por capas: servicio, `listen_addresses`, firewall y, con `tcpdump`, comprobó que los paquetes nunca llegaban a la VM |
| Revisión humana | El autor comparó la MAC de la VM con la de `arp -a` y confirmó un **conflicto de IP**: otro equipo usaba `192.168.0.20` |
| Corrección | IP fija `192.168.0.50` con netplan; usuario propietario `quoteflow_owner` para migrar y `quoteflow_app` con permisos mínimos (sin `DELETE` salvo en `quote_lines`; auditoría solo `INSERT` y `SELECT`) |
| Evidencia | Configuración en el servidor; permisos verificados con `\dp audit_events` |

### Lecciones de la revisión humana

- El agente propuso cadenas de conexión con `localhost` cuando la base estaba en la VM: **toda sugerencia de configuración se verificó** contra el entorno real.
- Las respuestas del agente se trataron como **hipótesis**; la evidencia de que algo funciona viene de las pruebas, de `psql` y de la ejecución en el navegador.
- Los secretos nunca se pusieron en prompts ni en archivos versionados. Cuando una clave quedó visible en una captura, se marcó para rotarla después de la entrega.
