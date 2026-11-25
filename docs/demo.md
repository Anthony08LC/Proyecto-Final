# QuoteFlow AI — Guion de la demostración

Duración estimada: **12 a 15 minutos**. Recorre el flujo de valor principal: catálogo → cliente → borrador → cálculo → redacción con IA → emisión → documento → respuesta del cliente → duplicación.

## 0. Preparación (antes de la demo)

- [ ] PostgreSQL arriba y migraciones aplicadas (ver [README.md](../README.md)).
- [ ] Backend en **Development** con `Database__SeedDemoData=true`: carga 6 clientes, 8 productos y 9 cotizaciones de demostración.
- [ ] `Gemini__ApiKey` configurada. Verificar `GET /api/assistant/status` → disponible y con cupo (30 por hora).
- [ ] Frontend en `http://localhost:5173`. Sesión ya iniciada como Administrador y contraseña temporal ya cambiada.
- [ ] Tener a mano una calculadora para el paso 4.
- [ ] Ensayar una vez la generación de un texto con Gemini, para "calentar" el servicio.

**Datos de demostración usados**

| Dato | Valor |
|---|---|
| Cliente VIP | Laura Jiménez Mora — Distribuidora Central del Valle S.A. |
| Cliente no VIP | Andrea Castro Quesada — Constructora Horizonte |
| Producto del caso de referencia | Servidor de archivos con respaldo — ₡1.200.000,00 (Solución empresarial) |

## Agenda según la consigna del curso

| Tiempo | Secciones de este guion |
|---|---|
| 0-2 min | 1 · Contexto y caso de negocio |
| 2-8 min | 2 a 7 · Flujo completo en vivo y persistencia |
| 8-10 min | 8 · Caso de error, comprobación ejecutable y corrección con el agente |
| 10-12 min | 9 y 10 · Documentos, evidencia de IA, indicador, costos, adopción y cierre |
| 12-15 min | Preguntas |

> No se usan diapositivas: la aplicación y el repositorio quedan abiertos durante toda la demo.

## 1. Contexto (1 min)

> "Hoy TechSolutions CR cotiza con hojas de cálculo: precios desactualizados, fórmulas copiadas, descuentos según quién cotice y sin historial. QuoteFlow centraliza todo y el ejecutivo ya no calcula nada a mano."

## 2. Catálogo y clientes (2 min)

1. **Productos:** mostrar los 8 productos en 4 categorías, la búsqueda y que solo el Administrador puede editar precios.
2. **Clientes:** abrir **Laura Jiménez Mora** y señalar la marca **VIP**. Explicar que solo el Administrador asigna esa condición.

## 3. Crear el borrador (2 min)

1. **Cotizaciones → Nueva cotización**, cliente **Laura Jiménez Mora**.
2. Agregar **Servidor de archivos con respaldo**, cantidad **1**.
3. Mostrar que el desglose se recalcula solo, con el catálogo y la política vigentes.

## 4. Verificación del cálculo a mano — caso C-09 (2 min)

Mostrar el panel de desglose y verificarlo en voz alta con la calculadora:

| Concepto | Cálculo | Monto |
|---|---|---|
| Subtotal | 1 × ₡1.200.000,00 | **₡1.200.000,00** |
| % descuento | Tramo 2 (> ₡1.000.000) 10 % + VIP 3 % | **13 %** |
| Descuento | ₡1.200.000,00 × 13 % | **₡156.000,00** |
| Base imponible | ₡1.200.000,00 − ₡156.000,00 | **₡1.044.000,00** |
| IVA 13 % | ₡1.044.000,00 × 13 % | **₡135.720,00** |
| **Total** | ₡1.044.000,00 + ₡135.720,00 | **₡1.179.720,00** |

> "El descuento siempre va antes del IVA, y el mismo caso está cubierto por una prueba automática del motor (C-09)."

Si se quiere mostrar sin crear nada, la cotización **emitida** de Laura Jiménez de los datos de demostración es exactamente este caso.

## 5. Redacción con Gemini (2 min)

1. En **Introducción**, pulsar **Generar**. Mostrar la sugerencia y **aceptarla**.
2. En **Condiciones comerciales**, escribir una idea corta (por ejemplo "pago a 30 días, entrega en 2 semanas") y pulsar **Mejorar**. Mostrar original y propuesta lado a lado; editar una palabra y aceptar.
3. Remarcar: la IA **no toca los montos**, solo trabaja en borradores, nada se guarda sin aceptar, y **no recibe correos ni teléfonos** del cliente.

## 6. Emitir y documento (2 min)

1. **Emitir.** Mostrar el consecutivo asignado (formato `COT-AAAA-NNNNNN`), la vigencia de 30 días y que la cotización ya no se puede editar.
2. Abrir la **vista previa** del documento y **descargar el PDF**: mismo contenido, mismo formato.
3. Mostrar el **historial** de la cotización: creación, textos generados y emisión, con usuario y hora.

## 7. Respuesta del cliente y duplicación (2 min)

1. Volver al listado y filtrar por estado: hay cotizaciones **Aceptada**, **Rechazada** (con motivo), **Vencida** y **Cancelada** de los datos de demostración.
2. En la cotización recién emitida, registrar **Aceptada**.
3. **Duplicar** una cotización emitida: se crea un borrador nuevo con datos vigentes, sin tocar el original.

## 8. Caso de error y comprobación ejecutable (8-10 min)

| Entrada | Resultado esperado |
|---|---|
| Cantidad **0** o **negativa** en una línea | El campo muestra el error y la API responde 400; la línea no se guarda (RN-07) |
| **Emitir** un borrador **sin líneas** | Se rechaza con el mensaje de la causa (RN-09) |
| Usuario de Ventas que intenta editar un producto | 403; la opción no aparece en su menú (RF-AUT-04) |
| Cambiar el precio de un producto después de emitir | La cotización emitida **no cambia**; un borrador nuevo sí usa el precio nuevo (RN-12) |

Comprobación automatizada en la terminal:

```powershell
dotnet test
```

Muestra los 19 casos del motor (C-01 a C-19) y las pruebas de integración. Luego explica una corrección hecha con el agente: el **ciclo 2** de [uso-ia.md](uso-ia.md), el error `ck_system_settings_singleton` y el commit `4c02b71d`.

## 9. Documentos y evidencia (10-12 min)

1. Abrir `intent.md` (problema, usuario e indicador), `spec.md` (RF, RN y modelo) y `plan.md` (estado real y trazabilidad, sección 13).
2. Abrir `docs/uso-ia.md`, Parte C: un ciclo con su commit.
3. Cerrar con `docs/negocio.md`: indicador de 15 a 5 minutos (supuesto), 20 h al mes liberadas (estimación), costos, limitaciones y los tres pasos de adopción.

## 10. Cierre (1 min)

> "Cinco minutos por cotización, cero errores de cálculo, una sola política comercial, documentos profesionales y todo queda registrado."

Mencionar con honestidad lo que queda pendiente para después de la V1.0.0 (sección 13 de [plan.md](../plan.md)).

---

## Plan alternativo si Gemini falla

El asistente es la **única** parte que depende de un servicio externo; el resto de la demo no cambia.

| Síntoma | Qué hacer en la demo |
|---|---|
| Botones del asistente deshabilitados ("no configurado") | Falta `Gemini__ApiKey`. **No** reiniciar en vivo: seguir con el plan B |
| Error del servicio o espera de más de 20 s | Reintentar **una** vez. Si vuelve a fallar, pasar al plan B |
| "Alcanzó el límite de 30 solicitudes por hora" | Se agotó el cupo del usuario en el ensayo. Pasar al plan B (o usar otro usuario si existe) |
| Sin internet | Plan B directamente |

**Plan B (sin perder el hilo):**

1. Convertir el fallo en argumento: *"Esto es justamente lo que exige la especificación: si Gemini no responde, el sistema sigue funcionando y solo se pierde la redacción asistida."*
2. Mostrar el mensaje claro que da el sistema y que **el borrador no cambió**.
3. Escribir o pegar a mano el texto de la introducción (tenerlo preparado abajo) y continuar con **Emitir** (paso 6). El resto del guion es idéntico.
4. Si se quiere mostrar cómo luce un texto generado, abrir una cotización ya emitida cuyos textos se hayan generado en el ensayo.

**Texto preparado para pegar** (Introducción):

> Estimada Laura: en atención a su solicitud, le presentamos nuestra propuesta para la implementación de un servidor de archivos con respaldo diario en la nube para Distribuidora Central del Valle S.A., pensada para proteger la información de su empresa y simplificar su operación. Como cliente preferente, la propuesta incluye las condiciones especiales que le corresponden.
