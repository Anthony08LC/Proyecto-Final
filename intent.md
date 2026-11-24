# QuoteFlow AI — Intención del Proyecto

| Campo | Valor |
|---|---|
| Documento | Intención del proyecto (`intent.md`) |
| Producto | QuoteFlow AI |
| Cliente | TechSolutions CR |
| Versión del documento | 1.0 |
| Fecha | 18 de noviembre de 2026 |
| Documentos relacionados | `spec.md` (qué se construye), `plan.md` (cómo se construye) |

> Este documento explica **por qué** existe QuoteFlow AI. El detalle funcional y técnico está en `spec.md`; el plan de construcción, en `plan.md`.

---

## 1. Propósito

QuoteFlow AI existe para que el equipo comercial de TechSolutions CR pueda elaborar cotizaciones **rápidas, correctas y controladas**, sin depender de hojas de cálculo ni de documentos sueltos.

La cotización es el punto en el que una oportunidad comercial se convierte en una propuesta concreta para el cliente. Si es lenta, se pierde la oportunidad. Si tiene errores, se pierde dinero o credibilidad. Si no queda registrada, se pierde el control del proceso comercial.

QuoteFlow AI centraliza ese proceso en una plataforma única que:

- calcula automáticamente descuentos, impuestos y totales con reglas consistentes;
- conserva un historial completo y auditable de cada cotización;
- genera documentos profesionales listos para enviar;
- ayuda a redactar los textos comerciales con inteligencia artificial, sin que esta intervenga nunca en el dinero.

---

## 2. Problema actual

Hoy, TechSolutions CR elabora sus cotizaciones de forma manual, con hojas de cálculo y documentos independientes. Este proceso tiene cinco problemas de fondo.

### 2.1 Dependencia de hojas de cálculo

Cada cotización se construye en una hoja de cálculo: se copian productos y precios, se escriben las fórmulas de subtotal, descuento e IVA y luego se traslada el resultado a un documento para el cliente. Los precios viven en archivos distintos, que pueden no estar actualizados, y cada ejecutivo puede tener su propia versión de la plantilla.

### 2.2 Procesos manuales

Buscar los datos del cliente, copiar los productos, revisar qué descuento corresponde, calcular el IVA, dar formato al documento y redactar las condiciones comerciales son pasos manuales que se repiten en cada cotización. El resultado es **pérdida de tiempo** en una tarea que debería ser rutinaria.

### 2.3 Errores de cálculo y descuentos inconsistentes

Cuando cada cotización depende de fórmulas copiadas y de la interpretación de cada persona, aparecen:

- fórmulas rotas o mal copiadas;
- descuentos aplicados en un orden distinto (antes o después del IVA);
- descuentos de volumen o de cliente VIP otorgados de forma distinta según quién cotice;
- precios desactualizados.

Cada error significa dinero que la empresa deja de cobrar o una cotización que se debe corregir frente al cliente.

### 2.4 Falta de control

No existe una regla que impida modificar una cotización después de enviada, ni un control sobre quién puede otorgar condiciones especiales. Tampoco hay una numeración oficial ni una vigencia controlada. La empresa no puede asegurar que lo que se cotiza respete su política comercial.

### 2.5 Falta de trazabilidad

El historial está **disperso** en archivos personales y carpetas. No es posible responder con certeza preguntas como:

- ¿Qué se le cotizó a este cliente y a qué precio?
- ¿Quién emitió esta cotización y cuándo?
- ¿Esta cotización fue aceptada, rechazada o venció?
- ¿Se modificó después de enviarla?

---

## 3. Objetivos de negocio

| ID | Objetivo |
|---|---|
| OB-01 | **Reducir significativamente el tiempo** necesario para elaborar una cotización comercial. |
| OB-02 | **Automatizar los cálculos financieros** (descuentos, IVA y totales) y eliminar los errores de cálculo. |
| OB-03 | **Aplicar una política comercial única y consistente**, definida por la empresa y no por cada ejecutivo. |
| OB-04 | **Mantener trazabilidad completa** sobre clientes, productos y cotizaciones. |
| OB-05 | **Generar documentos comerciales profesionales** listos para enviar a los clientes. |
| OB-06 | **Dar control sobre el proceso comercial**, con roles, estados y cotizaciones inmutables después de emitidas. |
| OB-07 | **Mejorar la calidad de las propuestas** con apoyo de inteligencia artificial en la redacción. |

---

## 4. Usuarios

### 4.1 Administrador

**Responsabilidades**

- Gestionar los usuarios del sistema y sus roles.
- Mantener el catálogo de productos, sus categorías y sus precios.
- Gestionar los clientes y decidir cuáles son VIP.
- Configurar los datos de la empresa y los parámetros comerciales (IVA, tramos de descuento, descuento VIP y vigencia).
- Revisar las cotizaciones del equipo.
- Consultar la auditoría.

**Necesidades**

- Que la política comercial definida se aplique igual en todas las cotizaciones.
- Que un cambio de precio o de parámetros llegue de inmediato a todo el equipo, sin afectar lo ya cotizado.
- Saber quién hizo qué y cuándo.
- Ver el estado general de la actividad comercial de un vistazo.

### 4.2 Ejecutivo de Ventas

**Responsabilidades**

- Registrar y consultar clientes.
- Crear cotizaciones, emitirlas y registrar la respuesta del cliente.
- Duplicar cotizaciones cuando el cliente pide cambios.
- Consultar el historial.
- Usar el asistente comercial para redactar textos.

**Necesidades**

- Armar una cotización en pocos minutos, con el catálogo siempre actualizado.
- Confiar en que el total es correcto sin revisar fórmulas.
- Entregar un documento con aspecto profesional.
- Encontrar rápidamente cualquier cotización anterior.
- Redactar introducciones, resúmenes y condiciones sin empezar desde cero.

---

## 5. Propuesta de valor

| Situación actual | Con QuoteFlow AI |
|---|---|
| Precios en archivos dispersos y posiblemente desactualizados | Catálogo único, mantenido por el Administrador |
| Fórmulas copiadas en cada hoja de cálculo | Motor de cálculo único que aplica siempre las mismas reglas |
| Descuentos según el criterio de cada persona | Descuentos automáticos según la política configurada; la condición VIP solo la asigna el Administrador |
| Documentos con formatos distintos | Documento comercial estandarizado, con vista previa y PDF |
| Cotizaciones modificables después de enviadas | Cotizaciones congeladas al emitir; los cambios se hacen duplicando |
| Sin numeración oficial | Consecutivo único `COT-AAAA-000001` |
| Vigencias controladas a mano | Vigencia automática y vencimiento automático |
| Historial disperso en archivos personales | Historial centralizado, con búsqueda y filtros |
| Sin registro de quién hizo qué | Auditoría de cada acción relevante |
| Redacción comercial desde cero | Asistente de IA que propone textos para revisar y aceptar |

**En resumen**, QuoteFlow AI aporta:

- **Productividad:** menos tiempo por cotización y menos tareas repetitivas.
- **Exactitud:** cero errores de cálculo, porque nadie calcula a mano.
- **Consistencia:** una sola política comercial para todo el equipo.
- **Control:** permisos por rol y un ciclo de vida definido.
- **Trazabilidad:** historial completo y auditable.
- **Imagen profesional:** documentos cuidados y textos bien redactados.

---

## 6. Alcance del MVP

La primera versión incluye:

1. **Autenticación y roles:** inicio de sesión, perfil, cambio de contraseña y roles Administrador y Ventas.
2. **Gestión de usuarios.**
3. **Gestión de clientes**, incluida la condición VIP.
4. **Gestión de categorías y productos.**
5. **Cotizaciones con ciclo de vida completo:** borrador, emisión, aceptación, rechazo, vencimiento automático, cancelación y duplicación.
6. **Motor de cálculo automático:** descuentos por tramo, descuento VIP e IVA, con parámetros configurables.
7. **Documento comercial:** vista previa HTML y PDF.
8. **Asistente comercial con Google Gemini:** redacción de introducción, resumen ejecutivo, observaciones y condiciones comerciales.
9. **Configuración del sistema:** datos de la empresa, parámetros comerciales y vigencia.
10. **Auditoría:** historial de eventos consultable.
11. **Dashboard:** indicadores, cotizaciones recientes y actividad reciente.

El detalle de cada funcionalidad está en `spec.md`.

---

## 7. Fuera de alcance

Se posponen para versiones futuras:

- Multiempresa.
- Multimoneda.
- Integraciones con ERP.
- Facturación electrónica.
- Portal de clientes.
- CRM.
- Integraciones bancarias.
- Automatizaciones avanzadas.
- Envío de cotizaciones por correo desde el sistema y recuperación de contraseña por correo.
- Flujo de aprobación interna de cotizaciones o descuentos.
- Descuentos manuales o por línea y listas de precios por cliente.
- IVA diferenciado por producto, exenciones o tarifas reducidas.
- Versión para teléfono móvil.

La lista completa está en la sección 13 de `spec.md`.

---

## 8. Integración de inteligencia artificial

### 8.1 Por qué se utiliza Gemini

Una cotización no son solo números. La introducción, el resumen ejecutivo, las observaciones y las condiciones comerciales determinan cómo percibe el cliente la propuesta. Redactarlos bien en cada cotización toma tiempo, y la calidad varía de una persona a otra.

Google Gemini permite **proponer esos textos en segundos**, a partir de los datos de la cotización, para que el ejecutivo parta de un borrador profesional en lugar de una página en blanco.

### 8.2 Qué aporta

- **Menos tiempo** en la redacción de cada cotización.
- **Textos más claros y profesionales**, con un estilo más uniforme en todo el equipo.
- **Mejora de textos existentes**: el ejecutivo puede escribir una idea y pedir una versión mejor redactada.
- **Control humano**: cada propuesta de la IA se revisa, se edita o se descarta antes de guardarse.

### 8.3 Limitaciones

La IA tiene una función **exclusivamente asistencial**. Por diseño:

- **No puede** modificar precios, calcular descuentos, modificar impuestos, alterar totales ni tomar decisiones financieras. Todos los cálculos oficiales los realiza el motor de cálculo del sistema.
- Solo actúa sobre cotizaciones en **borrador**, y nada de lo que genera se guarda sin la aceptación del usuario.
- **No recibe datos de contacto personales** de los clientes (correos o teléfonos); solo la información mínima necesaria para redactar.
- Su uso está **limitado por usuario** para controlar el consumo.
- El sistema **funciona completamente sin ella**: si Gemini no está disponible, solo se pierde la redacción asistida.
- Como todo modelo de lenguaje, puede proponer textos imprecisos. Por eso la revisión humana es obligatoria.

---

## 9. Criterios de éxito

### 9.1 Métricas de negocio

| ID | Métrica | Meta | Cómo se mide |
|---|---|---|---|
| CE-01 | Tiempo para elaborar una cotización típica (cliente existente, 5 líneas, textos incluidos) | **5 minutos o menos**, y al menos un **50 % menos** que el proceso actual | Cronometrar el mismo caso en el proceso actual con hoja de cálculo y en QuoteFlow AI |
| CE-02 | Errores de cálculo | **0** discrepancias | Verificación manual de una muestra de cotizaciones emitidas y de los 19 casos críticos del motor de cálculo |
| CE-03 | Consistencia de descuentos | **100 %** de las cotizaciones emitidas con descuentos derivados de la política configurada | Revisión de las cotizaciones emitidas contra la configuración vigente al emitir |
| CE-04 | Integridad de lo cotizado | **0** cotizaciones emitidas alteradas por cambios posteriores de precios, clientes o configuración | Pruebas de congelamiento y revisión del historial |
| CE-05 | Trazabilidad | **100 %** de las acciones definidas registradas en la auditoría | Pruebas de auditoría y revisión de eventos durante la demo |
| CE-06 | Documento comercial | **100 %** de las cotizaciones emitidas con PDF generable, idéntico a la vista previa | Generación de PDF de todas las cotizaciones de demostración |
| CE-07 | Adopción (después de la puesta en marcha) | **100 %** de las cotizaciones de TechSolutions CR elaboradas en el sistema | Comparar las cotizaciones registradas con las enviadas a clientes |

### 9.2 Métricas de calidad del producto

| ID | Métrica | Meta |
|---|---|---|
| CE-08 | Criterios de aceptación de la V1 (sección 14 de `spec.md`) | 24 de 24 cumplidos |
| CE-09 | Requisitos funcionales esenciales e importantes implementados | 52 de 52 |
| CE-10 | Pruebas del motor de cálculo | 19 de 19 casos críticos pasan |
| CE-11 | Tiempo de respuesta de las operaciones comunes | Menos de 500 ms en el 95 % de los casos |
| CE-12 | Generación del PDF | Menos de 3 segundos |
| CE-13 | Funcionamiento sin Gemini | El flujo principal completo funciona con el asistente desactivado |
| CE-14 | Seguridad | 0 secretos en el repositorio; 0 funciones de Administrador accesibles para Ventas |

### 9.3 Criterio global

El proyecto es exitoso si un ejecutivo de ventas puede **crear, calcular, redactar, emitir y entregar** una cotización profesional en pocos minutos, con la **certeza** de que el total es correcto, que respeta la política de la empresa y que queda registrada de forma permanente y auditable.

---

## 10. Visión futura

QuoteFlow AI se diseña como un monolito modular para crecer sin reescribirse. Después de la V1, la plataforma podría evolucionar en estas direcciones:

**Cerrar el ciclo comercial**
- **Portal de clientes**, donde el cliente consulte y acepte su cotización en línea.
- **Envío de cotizaciones por correo** directamente desde el sistema.
- **Conversión de cotizaciones aceptadas** en pedidos y **facturación electrónica**.

**Integrarse con el ecosistema de la empresa**
- **Integración con ERP y CRM** para sincronizar clientes, productos y oportunidades.
- **Integraciones bancarias** para pagos y conciliación.

**Ampliar la política comercial**
- **Multimoneda**, por ejemplo para cotizar en dólares.
- **Listas de precios por cliente**, descuentos por línea y flujos de aprobación de descuentos especiales.
- **IVA diferenciado** por producto, exenciones y tarifas reducidas.

**Crecer como plataforma**
- **Multiempresa**, para ofrecer QuoteFlow AI como servicio a otras organizaciones.
- **Reportes y análisis comercial** avanzados: tasa de aceptación, montos por ejecutivo, productos más cotizados.
- **Aplicación móvil** para el equipo de ventas en campo.
- **Automatizaciones avanzadas**, como recordatorios de vencimiento y seguimiento de cotizaciones emitidas.
- **Nuevas capacidades de IA asistencial**, siempre bajo el mismo principio de la V1: la IA ayuda a redactar y analizar, pero **nunca decide precios, descuentos, impuestos ni totales**.