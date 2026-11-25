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

## 7. Estado de la versión 1.0.0

El flujo de valor principal está construido: catálogo, clientes, cotizaciones con su ciclo de vida completo, motor de cálculo, documento HTML/PDF y asistente de redacción. Quedan pendientes, entre otros, las pantallas de usuarios, configuración y perfil, la consulta de auditoría, el dashboard y el despliegue en el servidor. La lista completa, con su motivo, está en la sección 13 de [plan.md](../plan.md).
