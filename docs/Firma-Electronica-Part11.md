# Firma electronica de contratos y consentimientos (Spix)

Documento de referencia del proceso de firma. Sirve como descripcion para una auditoria o para
solicitar certificacion bajo **21 CFR Part 11** (firma electronica) y como guia para el equipo.

Ultima actualizacion: 18-sep-2026.

---

## 1. Que se firma

Por cada contrato de cliente (`ContractClient`) se generan dos documentos:

| Documento | Origen |
|---|---|
| Consentimiento de datos | Plantilla PDF activa de tipo *Consent Datos* de la corporacion |
| Contrato de servicio | Plantilla PDF activa de tipo *Contrato* de la corporacion |

Las plantillas se cargan en **Plantillas PDF**, donde se marcan las coordenadas de cada dato
(nombre, documento, telefono, direccion, correo, nombre imprenta, fecha y firma).
Solo puede haber **una plantilla activa por tipo y por corporacion**, para que todos los clientes
firmen la misma version del documento.

El PDF que ve el cliente se arma en el momento, llenando la plantilla activa con los datos reales
del contrato. Mientras no firme, **no se guarda ningun documento**: lo que se muestra es una vista
previa que se regenera cada vez.

---

## 2. Quien puede firmar

El firmante es el **cliente titular del contrato**, identificado con su cuenta de la aplicacion.
El usuario de acceso es unico en toda la plataforma y **no se puede cambiar** una vez creado
(el sistema lo bloquea en el backend y en pantalla), de modo que la cuenta que firma siempre es
la misma persona a lo largo del tiempo.

---

## 3. Flujo de firma a distancia (portal del cliente)

1. **Invitacion.** El sistema envia al correo del cliente la solicitud de firma con el enlace al portal.
   El enlace es solo una comodidad: no permite firmar por si mismo.
2. **Primer componente de identificacion (algo que sabe).** El cliente entra con su usuario y su clave.
3. **Lectura.** Ve el documento completo, ya lleno con sus datos, dentro de su cuenta.
4. **Declaracion de conformidad.** Marca la casilla "He leido el documento y estoy conforme".
   Se guarda la fecha y hora de esa aceptacion.
5. **Segundo componente de identificacion (algo que tiene).** Pulsa Firmar y el sistema envia un
   codigo de un solo uso al correo registrado del cliente.
6. **Verificacion.** El cliente escribe el codigo. El sistema valida vigencia, intentos y uso previo.
7. **Firma.** Con el codigo validado, el cliente dibuja su firma y el sistema genera el PDF definitivo.
8. **Entrega.** El documento firmado queda en la cuenta del cliente y en el contrato dentro del sistema.

---

## 4. Flujo de firma presencial (oficina o visita tecnica)

Es el mismo flujo, en el dispositivo del asesor, con dos variantes:

- **Firma verificada:** el cliente recibe igualmente el codigo en su correo y lo escribe. Tiene la
  misma fuerza que la firma a distancia.
- **Firma presencial asistida:** solo cuando el cliente no puede recibir el codigo en ese momento.
  Se registra al asesor como testigo (su usuario y su id) y se conserva la foto del documento de
  identidad del cliente, que ya exige el proceso. Queda marcada como asistida para distinguirla
  en una auditoria.

---

## 5. Reglas del codigo de verificacion

| Regla | Valor |
|---|---|
| Longitud | 6 digitos |
| Vigencia | 15 minutos |
| Intentos maximos | 5 |
| Uso | Un solo uso |
| Almacenamiento | Solo el **hash** del codigo; nunca se guarda en texto plano |
| Envio | Al correo registrado del cliente, no a uno escrito en el momento |

---

## 6. Evidencia que queda guardada

Por cada documento firmado el sistema conserva:

- Metodo de firma: portal verificada, oficina verificada u oficina asistida.
- Usuario que firmo (cuenta) y su id interno.
- Fecha y hora de la firma.
- Correo al que se envio el codigo, hora de envio y hora de validacion.
- IP de origen y navegador del firmante.
- Fecha y hora de aceptacion de la casilla de conformidad.
- Huella (hash SHA-256) del PDF firmado, para demostrar que no se modifico despues.
- Asesor testigo, cuando la firma fue asistida.

La IP, el navegador y el referer los entrega el propio backend en cada llamada autenticada
(`GetSecurityContextOrThrow`), no los envia el navegador del cliente como dato editable.

---

## 7. Evidencia dentro del propio documento

El documento se defiende solo: no hay que abrir la base de datos para probar la firma.

**7.1 Sello al pie.** La ultima pagina del contenido lleva una linea con la fecha y hora de la
firma, el metodo, el correo enmascarado del firmante y la IP.

**7.2 Hoja de certificado.** Al final del PDF se anexa una pagina "Certificado de firma
electronica" con todo el rastro, en cuatro bloques:

| Bloque | Contenido |
|---|---|
| Documento | Plantilla firmada, tipo de documento, numero de contrato, direccion del servicio |
| Firmante | Nombre, documento de identidad, usuario de la plataforma, correo registrado |
| Verificacion de identidad | Metodo, hora de envio del codigo, hora de validacion, intentos fallidos, hora de aceptacion de los terminos, IP y navegador |
| Integridad | Algoritmo SHA-256 y huella del documento firmado |

**7.3 Identificador de firma y codigo QR.** La hoja de certificado lleva arriba un identificador
de 12 caracteres en grupos (`XXXX-XXXX-XXXX`, sin letras que se confundan) y un codigo QR que
apunta a `https://<front>/verificar/<identificador>`.

La consulta **pide sesion**: quien escanea el QR entra con su usuario y clave y ahi ve el
certificado. Si no tiene sesion, la aplicacion guarda a donde iba, lo lleva al login y lo
devuelve a la pagina del documento. La respuesta nunca incluye el PDF, y el nombre y el correo
del firmante van enmascarados; la IP y el navegador quedan solo en la bitacora interna, porque
son datos personales.

Decision (2026-09-18): se evaluo dejar la verificacion abierta al publico, como DocuSign. Se
prefirio exigir sesion. Para que un tercero sin cuenta (un juez, un auditor externo) pueda
comprobar el documento por si mismo quedan dos caminos: darle un usuario temporal, o verificar
por la huella del archivo en vez del identificador.

**7.4 Acceso al PDF.** Los contenedores del blob son **privados** (`PublicAccessType.None`): la
unica puerta es un enlace firmado (SAS). Ese enlace se pide en el momento de abrir el documento,
nunca al listar, y **vence a los 3 minutos**: alcanza para abrir el PDF y leerlo, no para
compartirlo ni para que sirva si alguien lo copia del historial. Cada peticion de enlace de un
documento pendiente queda ademas en la bitacora como `DocumentViewed`.

**7.5 El archivo queda cerrado.** El PDF final se guarda con permisos de solo lectura e impresion:
no se puede editar, ensamblar ni anotar. La clave de propietario es aleatoria y no se guarda en
ninguna parte, asi que nadie puede levantar las restricciones.

**Orden del calculo y las dos huellas.**

| Huella | Que cubre | Donde esta |
|---|---|---|
| `DocumentHash` | Contenido + firma + sello al pie, ANTES de anexar el certificado | Impresa en la hoja de certificado y guardada en la base |
| `FileHash` | El archivo final completo, tal como quedo archivado | Guardada en la base; es la que comprueba quien sube el PDF a la pagina de verificacion |

Se necesitan las dos: la primera no se puede imprimir dentro de si misma, y la segunda es la que
obtiene cualquiera que calcule el SHA-256 del archivo que tiene en la mano.

---

## 7.bis Bitacora del proceso (audit trail)

Cada paso queda en `ContractSignatureEvents`, un renglon por evento, que solo se escribe: nunca
se edita ni se borra. Es lo que exige 21 CFR Part 11.10(e) y el equivalente al
"Sent / Viewed / Signed" de DocuSign.

| Evento | Cuando se registra |
|---|---|
| `RequestSent` | La oficina envia la invitacion a firmar |
| `DocumentViewed` | El cliente abre el documento en su portal |
| `CodeSent` | Pide el codigo de verificacion |
| `CodeFailed` | Escribe un codigo equivocado (queda el numero de intento) |
| `CodeValidated` | El codigo entra bien |
| `Signed` | Queda firmado (el detalle es el identificador publico) |

Cada renglon guarda ademas la fecha y hora en UTC, la IP, el navegador y el usuario que ejecuto
el paso. La bitacora se muestra completa en la pagina publica de verificacion.

---

## 7.ter Aviso de firma electronica

Antes de firmar, el cliente ve el aviso de `ElectronicSignatureConsent` (texto y version viven en
el servidor, no en el navegador) y lo acepta junto con la casilla "He leido el documento y el
aviso, y estoy conforme". Con el documento se guardan la **version** del aviso y su **huella
SHA-256**, de modo que siempre se sabe que texto exacto acepto. Si el aviso cambia, sube la
version y los documentos viejos siguen probando cual acepto su firmante.

---

## 8. Controles que impiden discutir la firma

- **Un solo documento firmado por tipo y por contrato.** Si ya esta firmado, no se puede volver a
  firmar ni regenerar.
- **La vista previa no es el documento firmado.** Solo el que pasa por el codigo se guarda como firmado.
- **El usuario de acceso no se puede cambiar** despues de creado.
- **Las plantillas con documentos firmados no se pueden eliminar**, solo desactivar, para que el
  documento original siempre se pueda rastrear.
- **Todo cambio de estado del contrato queda registrado**: al completar fotos de documento de
  identidad, consentimiento y contrato firmados, el contrato pasa a *Pending Approval*, y el paso a
  *In Progress* lo hace una persona autorizada, de forma explicita.

---

## 9. Correspondencia con 21 CFR Part 11

| Requisito | Como se cumple en Spix |
|---|---|
| 11.10(a) Sistemas validados | Documentos generados desde plantilla controlada, una activa por tipo |
| 11.10(c) Proteccion de registros | PDF en almacenamiento privado con acceso por enlace temporal firmado |
| 11.10(d) Acceso limitado | Cuentas con rol; el cliente solo ve sus propios documentos |
| 11.10(e) Rastro de auditoria | Evidencia por documento y registro de cambios de estado del contrato |
| 11.50 Manifestacion de la firma | El PDF muestra firmante, fecha, hora y metodo |
| 11.70 Vinculo firma-registro | Huella del PDF guardada junto con la evidencia |
| 11.100 Unicidad | Usuario unico en la plataforma, no reutilizable ni modificable |
| 11.200(a)(1) Dos componentes | Clave de la cuenta + codigo de un solo uso al correo |
| 11.200(a)(1)(ii) Cada firma | El codigo se pide en el momento de firmar, no al iniciar sesion |
| 11.300(b) Vigencia de credenciales | Codigo con vencimiento, intentos limitados y un solo uso |

---

## 10. Pendiente por definir

### 10.1 Firma digital certificada (para que el PDF se valide solo en Adobe Reader)

Lo que hay hoy es una **firma electronica** con evidencia completa. Para que un tercero valide el
documento sin consultarnos, hace falta ademas:

- **Firma PAdES** (PKCS#7 incrustado en el PDF). Obliga a cambiar `PdfSharpCore 1.3.67` por
  `PDFsharp 6.x`, que si soporta firmas digitales.
- **Certificado X.509 de la empresa** emitido por una entidad de certificacion acreditada ante
  ONAC (Certicamara, Andes SCD, GSE). Tiene costo anual.
- **Estampado cronologico RFC 3161** de una TSA, para que la hora no dependa de nuestro servidor.
- **Llave privada en Azure Key Vault o HSM**, nunca en el disco del servidor.

Para 21 CFR Part 11 el certificado digital no es obligatorio; para que "cualquier ente valide el
documento por su cuenta", si.

### 10.2 Otros

- Envio del codigo por WhatsApp o SMS como alternativa al correo.
- Politica de retencion y respaldo de los PDF firmados.
- Procedimiento escrito de revocacion de cuentas de clientes retirados.

---

## 11. Donde esta implementado

| Paso | Archivo |
|---|---|
| Invitacion por correo | `Spix.AppService/ImplementSignature/SignatureService.cs` -> `SendSignatureRequestAsync` |
| Plantilla del correo de invitacion | `Spix.xNotification/Templates/SignatureRequestEmailTemplate.cs` |
| Lista de documentos del cliente | `SignatureService.GetMyDocumentsAsync` |
| Envio del codigo de un solo uso | `SignatureService.RequestSignatureCodeAsync` |
| Plantilla del correo del codigo | `Spix.xNotification/Templates/SignatureCodeEmailTemplate.cs` |
| Validacion del codigo y firma | `SignatureService.SignMyDocumentAsync` |
| Guardado del codigo (solo hash) | `Spix.Domain/EntitiesContratos/ContractSignatureCode.cs` |
| Evidencia de la firma | `Spix.Domain/EntitiesContratos/ContractSignedDocument.cs` |
| Sello visible al pie del PDF | `Spix.xFiles/SignatureHelper/PdfSignatureService.cs` -> `AddEvidenceFooter` |
| Hoja de certificado anexa | `PdfSignatureService.AddCertificatePage` + `PdfCertificateData.cs`; los datos los arma `SignatureService.BuildCertificate` |
| API del portal del cliente | `Spix.AppBacken/Controllers/v1/EntitiesContracts/MySignaturesController.cs` |
| Pantalla del cliente | `Spix.AppFront/Pages/EntitiesContratos/MySignaturePage/` |
| Bitacora de eventos | `Spix.Domain/EntitiesContratos/ContractSignatureEvent.cs`; la escribe `SignatureService.AddEvent` |
| Aviso de firma electronica | `Spix.DomainLogic/ModelUtility/ElectronicSignatureConsent.cs` |
| Verificacion (API) | `Spix.AppBacken/Controllers/v1/EntitiesContracts/SignatureVerificationController.cs`, pide token |
| Verificacion (pagina) | `Spix.AppFront/Pages/EntitiesContratos/MySignaturePage/VerifySignature.razor`, ruta `/verificar/{codigo}`, con `[Authorize]` |
| Cierre del PDF | `PdfSignatureService.Protect` |
| Paso automatico de estado | `Spix.AppService/ImplementContratos/ContractRequirementRules.cs` |

La IP y el navegador NO los manda el navegador: los arma el backend en
`User.GetSecurityContextOrThrow(...)` (respeta X-Forwarded-For) y viajan al servicio dentro de `ClaimsDTOs`.
