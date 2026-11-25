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
