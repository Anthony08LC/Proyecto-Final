# QuoteFlow AI — Documento de negocio

> Resumen para el negocio. El porqué completo está en [intent.md](../intent.md) y el detalle funcional en [spec.md](../spec.md).

## 1. El problema

TechSolutions CR cotiza hoy con hojas de cálculo y documentos sueltos. Eso provoca:

- **Pérdida de tiempo:** cada cotización repite a mano la búsqueda de precios, las fórmulas, el formato y la redacción.
- **Errores de cálculo:** fórmulas rotas o copiadas, precios desactualizados, descuentos aplicados en distinto orden respecto al IVA.
- **Descuentos inconsistentes:** cada ejecutivo interpreta la política comercial a su manera.
- **Falta de control:** una cotización se puede cambiar después de enviada; no hay numeración oficial ni vigencia controlada.
- **Falta de trazabilidad:** no se sabe con certeza qué se cotizó, a quién, a qué precio, quién lo emitió ni en qué terminó.

## 2. La solución

Una plataforma única donde el ejecutivo arma la cotización a partir del catálogo oficial y el sistema hace el resto:

| Antes | Con QuoteFlow AI |
|---|---|
| Precios en archivos dispersos | Catálogo único que mantiene el Administrador |
| Fórmulas en cada hoja | Un motor de cálculo con reglas fijas y parámetros configurables |
| Descuentos a criterio | Descuentos automáticos; la condición VIP solo la asigna el Administrador |
| Formatos distintos | Documento comercial estándar con vista previa y PDF |
| Cotizaciones editables después de enviadas | Al emitir, la cotización queda congelada; los cambios se hacen duplicando |
| Sin numeración | Consecutivo único `COT-AAAA-000001` |
| Vigencias a mano | Vigencia automática y vencimiento automático |
| Redacción desde cero | Asistente de IA que propone textos para revisar y aceptar |

## 3. Roles

- **Administrador:** usuarios, catálogo (categorías y productos), clientes y condición VIP, configuración de la empresa y de la política comercial, revisión de todas las cotizaciones.
- **Ventas:** clientes, creación y emisión de cotizaciones, registro de la respuesta del cliente, duplicación y uso del asistente.

## 4. Reglas comerciales

Valores iniciales; el Administrador puede cambiarlos y el cambio aplica a los borradores, nunca a lo ya emitido.

| Regla | Valor inicial |
|---|---|
| Tramo 1 | Subtotal **mayor** que ₡500.000 → 5 % |
| Tramo 2 | Subtotal **mayor** que ₡1.000.000 → 10 % (los tramos son excluyentes) |
| Cliente VIP | +3 puntos porcentuales, haya o no tramo |
| Descuento máximo | 13 % (tramo 2 + VIP) |
| IVA | 13 % sobre la base imponible |
| Orden | El descuento siempre se calcula **antes** del IVA |
| Redondeo | 2 decimales; los valores medios se alejan de cero |
| Vigencia | 30 días desde la emisión |

**Fórmula:**

```
subtotal         = Σ (cantidad × precio unitario)
% descuento      = % tramo + % VIP
descuento        = subtotal × % descuento
base imponible   = subtotal − descuento
IVA              = base imponible × 13 %
total            = base imponible + IVA
```

**Ejemplo de referencia (caso C-09):** cliente VIP, subtotal ₡1.200.000,00 → 10 % + 3 % = 13 % → descuento ₡156.000,00 → base ₡1.044.000,00 → IVA ₡135.720,00 → **total ₡1.179.720,00**.

## 5. Ciclo de vida de una cotización

```
Borrador ──emitir──► Emitida ──► Aceptada
   │                    ├──────► Rechazada (con motivo)
   │                    ├──────► Vencida (automático, al pasar la vigencia)
   │                    └──────► Cancelada (motivo obligatorio)
   └──cancelar──► Cancelada (motivo opcional)
```

- Solo el **borrador** se edita. Mientras es borrador, siempre usa precios, cliente y política **vigentes**.
- Al **emitir** se asigna el consecutivo y se congelan precios, datos del cliente, descuentos, IVA y textos. Cambios posteriores en el catálogo o la configuración no la alteran.
- Cualquier cotización se puede **duplicar** como un borrador nuevo con los datos vigentes.
- Cada acción relevante queda registrada en la auditoría (quién, qué y cuándo).

## 6. Beneficios esperados

| Beneficio | Meta (ver intent.md, sección 9) |
|---|---|
| Productividad | Cotización típica en 5 minutos o menos (al menos 50 % menos que hoy) |
| Exactitud | 0 errores de cálculo; 19 de 19 casos críticos del motor verificados |
| Consistencia | 100 % de los descuentos derivados de la política configurada |
| Integridad | 0 cotizaciones emitidas alteradas por cambios posteriores |
| Trazabilidad | 100 % de las acciones definidas en la auditoría |
| Imagen profesional | Documento estándar en PDF, idéntico a la vista previa |
| Continuidad | El flujo completo funciona aunque Gemini no esté disponible |

## 7. Medición del indicador

**Indicador principal:** minutos para elaborar una cotización típica (cliente existente, 5 líneas, con textos).

| | Proceso actual (hoja de cálculo) | QuoteFlow AI |
|---|---|---|
| Valor | **15 min (supuesto)** | **Meta: 5 min o menos** |
| Cómo se mide | Cronometrar el mismo caso de demostración, 3 repeticiones, tomar la mediana | Mismo caso, mismas 3 repeticiones |
| Errores de cálculo | Comparar el total con el cálculo a mano | Igual; además, los 19 casos C-01 a C-19 automatizados |

> La línea base de 15 minutos es un **supuesto** que se verifica con la prueba de tareas anterior. No es un dato real de una empresa.

## 8. Estimación de valor (no es un resultado garantizado)

| Supuesto | Valor |
|---|---|
| Cotizaciones por mes | 120 |
| Minutos ahorrados por cotización | 10 (de 15 a 5) |
| Costo por hora de un ejecutivo | ₡4.000 |

- Tiempo liberado: 120 × 10 min = **20 horas al mes**.
- Valor del tiempo liberado: 20 h × ₡4.000 = **₡80.000 al mes**. Es tiempo disponible para vender, **no** ahorro efectivo de dinero.
- Errores evitados: no se cuantifican en dinero por falta de datos; con el motor y sus pruebas, el objetivo es 0.

## 9. Costos estimados

| Concepto | Inicial | Recurrente | Supuesto |
|---|---|---|---|
| Licencias (.NET, React, PostgreSQL, EF Core) | ₡0 | ₡0 | Software libre |
| PDF (QuestPDF Community) | ₡0 | ₡0 | Válida para organizaciones con ingresos menores a USD 1 millón; revisar al adoptar |
| Gemini | ₡0 | ₡0 en nivel gratuito; bajo en nivel de pago | El nivel gratuito puede usar los datos para mejorar el servicio: para producción se recomienda el nivel de pago |
| Servidor (VM Ubuntu con PostgreSQL) | Infraestructura existente | Energía y mantenimiento | Se reutiliza un servidor interno |
| Implementación y capacitación | ≈ 8 h de TI | ≈ 2 h al mes de mantenimiento | Horas del equipo interno |

**Beneficio neto estimado** = valor del tiempo liberado (₡80.000/mes) − costos recurrentes (mantenimiento y, si se contrata, Gemini de pago). Con los supuestos anteriores es positivo desde el primer mes; debe recalcularse con datos del piloto.

## 10. Controles y revisión humana

| Qué | Control |
|---|---|
| Montos | Solo el motor del backend los calcula; restricciones aritméticas en la base de datos; 19 pruebas automatizadas |
| Textos de la IA | Siempre revisados y aceptados por una persona antes de guardarse |
| Descuentos especiales | La condición VIP solo la asigna el Administrador |
| Cotizaciones emitidas | Inmutables; los cambios se hacen duplicando |
| Acciones | Auditoría inmutable para la aplicación (solo inserción y lectura) |
| Código generado con IA | Revisado por el autor, probado y versionado con commits trazables (ver [uso-ia.md](uso-ia.md)) |

## 11. Limitaciones (por qué no va directo a producción)

- No está desplegado con HTTPS en un servidor de producción.
- No hay recuperación de contraseña por correo ni envío de cotizaciones por correo.
- Sin pruebas de carga; el rendimiento se validó solo con el volumen de demostración.
- IVA único del 13 %: no maneja productos exentos ni tarifas reducidas.
- Solo colones y una empresa.
- La línea base y el valor estimado son supuestos hasta medir el piloto.

## 12. Ruta de adopción

| Paso | Qué | Responsable | Criterio para avanzar | Costo |
|---|---|---|---|---|
| 1. Piloto | 2 ejecutivos durante 2 semanas, con el catálogo real | Gerente comercial | ≥ 20 cotizaciones, 0 errores de cálculo, mediana ≤ 5 min | Capacitación de 2 h |
| 2. Ajustes | Corregir lo reportado en el piloto y validar la política de descuentos | Administrador y TI | Sin defectos críticos abiertos; aprobación del gerente | Horas de TI |
| 3. Ampliación controlada | Todo el equipo; despliegue con HTTPS y respaldos diarios | TI | 1 mes estable; 100 % de cotizaciones en el sistema | Mantenimiento mensual |

## 13. Estado de la versión 1.0.0

El flujo de valor principal está construido: catálogo, clientes, cotizaciones con su ciclo de vida completo, motor de cálculo, documento HTML/PDF, asistente de redacción, dashboard de inicio, pantalla de configuración, gestión de usuarios y consulta de auditoría. Quedan pendientes, entre otros, la pantalla de perfil y el despliegue en el servidor. La lista completa, con su motivo, está en la sección 13 de [plan.md](../plan.md).
