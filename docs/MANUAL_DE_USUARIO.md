# Manual de usuario — CloudKeep

## 1. ¿Qué es CloudKeep?

CloudKeep es una aplicación orientada al **respaldo de información** hacia **destinos en la nube**. Permite definir **orígenes**, **destinos** (Amazon S3 o Azure Blob Storage), **trabajos** que enlazan un origen con un destino según una **programación tipo cron**, y **scripts** opcionales que se ejecutan antes o después de cada copia.

La solución incluye:

- Un **servidor web local** (API ASP.NET Core) que sirve la **interfaz web** (Angular) y expone los datos y operaciones.
- Un **servicio en segundo plano** que revisa cada minuto si hay trabajos programados pendientes de ejecutar.
- En **Windows**, un **icono en la bandeja del sistema** para abrir la interfaz en el navegador.

Los mensajes de error y validación que verá en pantalla suelen estar en **español**.

---

## 2. Instalación y desinstalación

### 2.1 Instalación

1. Ejecute `setup.exe`.
2. Si Windows lo solicita, confirme permisos de instalación.
3. En el asistente, seleccione la carpeta de destino (si no la cambia, se instala en `C:\Program Files\CloudKeep`).
4. Continúe con **Siguiente** hasta finalizar.
5. (Opcional) Marque la creación de acceso directo en escritorio.
6. (Opcional) Marque **Ejecutar CloudKeep** al terminar.

### 2.2 Verificación posterior

1. Abra CloudKeep desde el menú Inicio o el acceso directo.
2. Confirme que la aplicación inicia y que puede abrir la interfaz web.
3. Inicie sesión y cambie la contraseña inicial si aplica.

### 2.3 Desinstalación

1. Cierre CloudKeep si está en ejecución.
2. Abra **Configuración de Windows → Aplicaciones** (o **Agregar o quitar programas**).
3. Busque **CloudKeep**.
4. Pulse **Desinstalar** y confirme.
5. Reinicie el equipo solo si el asistente lo solicita.

---

## 3. Requisitos y puesta en marcha

### 3.1 Dónde debe instalarse o ejecutarse

- El **origen** de cada respaldo es una **ruta de carpeta en el mismo equipo** donde se ejecuta la API. Si indica `C:\Usuarios\…\Documentos`, esa carpeta debe existir **en el servidor o PC donde corre CloudKeep**, no en el navegador del usuario.

### 3.2 Navegador y acceso

- Use un navegador actual (Chrome, Edge, Firefox, etc.).
- La dirección depende de cómo despliegue la aplicación (por defecto es  `http://localhost:5271` o la URL que configure su administrador).
- En Windows, si está configurada la bandeja, puede abrir la misma URL desde el **icono de CloudKeep** en el área de notificación (configurable con `Tray:OpenBrowserUrl`).

### 3.3 Base de datos y primer arranque

- La aplicación utiliza una base de datos local (SQLite por defecto en el proyecto) y puede crear **datos iniciales** (usuario, ajustes, orígenes de ejemplo, scripts de ejemplo y, si aplica, un trabajo de demostración).
- La primera vez que inicia, debe existir un usuario configurado para poder iniciar sesión (véase sección 4).

---

## 4. Inicio de sesión y seguridad

### 4.1 Inicio de sesión

1. Abra la URL de la aplicación.
2. Introduzca la **contraseña** del usuario único del sistema (en entornos de demostración suele crearse un usuario con contraseña inicial **`admin`**; cámbiela en producción).
3. Tras un login correcto, la aplicación utiliza un **token JWT** para las peticiones siguientes.

### 4.2 Comprobar si la sesión sigue válida

- Puede usarse la función prevista en la API para validar el token enviado en la cabecera `Authorization: Bearer …`.

### 4.3 Cambio de contraseña

- Desde la opción de **cambio de contraseña** deberá indicar la **contraseña actual** y la **nueva contraseña**.

**Política de la nueva contraseña** (resumen):

- Longitud entre **12** y **128** caracteres.
- Al menos **una mayúscula**, **una minúscula**, **un dígito** y **un símbolo** (por ejemplo `! @ # $ %` …).
- Debe ser **distinta** de la contraseña actual.

---

## 5. Conceptos principales

| Concepto | Descripción breve |
|----------|---------------------|
| **Origen** | Carpeta local cuyo contenido se respalda. Debe existir en el equipo servidor. |
| **Destino** | Nube donde se suben los archivos: **S3** o **Azure Blob**. Las credenciales sensibles se almacenan cifradas en el servidor. |
| **Trabajo** | Une un origen y un destino, define expresión **cron**, si está **activo**, y opcionalmente scripts pre/post. |
| **Script** | Archivo **`.ps1`**, **`.bat`** o **`.js`** en disco, registrado en el sistema para asociarlo a trabajos. |
| **Historial de ejecuciones** | Registro de cada copia (manual o programada): fechas, estado, archivos copiados, errores. |
| **Log de acciones** | Auditoría de cambios relevantes (creación, edición, borrado) sobre orígenes, destinos, scripts, etc. |

---

## 6. Orígenes

### 6.1 Listar y consultar

- Puede ver todos los orígenes y abrir el detalle de uno por su identificador.

### 6.2 Crear un origen manualmente

- Indique **nombre**, **ruta** y **descripción**. El nombre debe ser **único**.
- La ruta debe ser una carpeta **existente** en el servidor en el momento de validaciones que comprueban el sistema de archivos.

### 6.3 Validar una ruta

- Use la acción de **validar ruta** antes de guardar si desea comprobar que la carpeta existe y obtener la ruta **normalizada** (ruta absoluta).


### 6.4 Editar y eliminar

- Puede actualizar nombre, ruta, descripción, tamaño máximo o filtros de exclusión según lo permita la interfaz.
- La eliminación de un origen debe hacerse con cuidado si hay trabajos que lo utilizan (el sistema puede impedir operaciones inconsistentes según las reglas de negocio vigentes).

---

## 7. Destinos en la nube

### 7.1 Tipos soportados

- **Amazon S3** (`S3`): bucket, región, prefijo de carpeta lógica (`carpetaDestino`). Puede usarse con **Access Key / Secret**.
- **Azure Blob Storage** (`AzureBlob`): nombre del contenedor, cadena de conexión y prefijo de carpeta lógica.

Los nombres de tipo deben coincidir exactamente con los valores permitidos por el sistema (respetando mayúsculas y minúsculas según la configuración).

### 7.2 Validar conexión antes de guardar

Se recomienda usar las acciones de **validación** disponibles:

- Validar **S3** (bucket, región; opcionalmente claves de acceso si las envía).
- Validar **Azure Blob** (contenedor y cadena de conexión).

Así se reducen errores por credenciales incorrectas o permisos insuficientes.

### 7.3 Prefijo de carpeta (S3 y Azure)

- El campo de **carpeta destino** es un prefijo lógico dentro del bucket o contenedor (segmentos separados por `/`).
- No debe contener segmentos `.` ni `..` por motivos de seguridad.

### 7.4 Edición y borrado

- Al editar un destino, si cambia credenciales, el sistema puede **volver a validar** contra el proveedor (por ejemplo Google o Azure) antes de guardar.
- No podrá eliminar un destino que esté **asociado a uno o más trabajos**; primero debe reasignar o eliminar esos trabajos.

---

## 8. Scripts

### 8.1 Registro

- Cada script necesita **nombre**, **ruta absoluta o relativa al sistema de archivos del servidor**, **argumentos** (opcional) y **tipo** (`.ps1`, `.bat` o `.js`).
- El archivo debe **existir físicamente** en el servidor en el momento del alta; de lo contrario, el sistema rechazará el registro.

### 8.2 Uso en trabajos

- Un trabajo puede tener un script **PRE** (antes de la copia) y/o **POST** (después).
- Puede configurarse si un fallo del script **detiene** el flujo del respaldo o solo se registra como advertencia.

### 8.3 Tiempo máximo de ejecución

- En **Configuración** (véase sección 12) puede definirse el tiempo máximo en **minutos** que puede ejecutarse un script PRE/POST.
- Si un script supera ese tiempo, la ejecución se cancela y se trata según las reglas del trabajo (mensaje de error o continuación, según configuración).

### 8.4 Eliminar un script

- No podrá borrarse un script que esté **asignado como PRE o POST** en algún trabajo. Primero debe desasociarlo en los trabajos afectados.

---

## 9. Trabajos (respaldos programados y manuales)

### 9.1 Qué define un trabajo

- **Nombre** y **descripción**.
- **Origen** y **destino** (deben existir previamente).
- **Expresión cron** en **hora UTC** (el motor revisa cada **minuto** si corresponde ejecutar el trabajo).
- Estado **activo** o inactivo: solo los trabajos **activos** y que no estén ya **en ejecución** se consideran para la programación automática.
- Opcionalmente: scripts PRE/POST y comportamiento ante fallo de script.

### 9.2 Ejecución programada

- El servicio en segundo plano evalúa la expresión cron con la hora **UTC** actual.
- Si dos disparadores coinciden, el sistema evita **ejecutar dos veces el mismo trabajo en el mismo minuto** (control interno por minuto UTC).

### 9.3 Ejecución manual

- Desde la interfaz puede solicitarse **ejecutar ahora** un trabajo concreto.
- Si el trabajo **ya se está ejecutando** (manual o programado), recibirá un mensaje indicando que debe **esperar** a que termine.

### 9.4 Qué ocurre durante una ejecución

1. Se marca el trabajo como **en proceso**.
2. Se crea una entrada en el **historial** (estado en progreso).
3. Opcionalmente se ejecuta el script **PRE**.
4. Se copian archivos del origen al destino respetando **filtros de exclusión** del origen.
5. Se actualiza el historial (completado o fallido) y el número de archivos copiados cuando aplica.
6. Opcionalmente se ejecuta el script **POST**.
7. Se libera el estado **en proceso** del trabajo.

Si la carpeta de origen **no existe** en el servidor en ese momento, la ejecución fallará con un mensaje claro.

---

## 10. Reportes e historial de ejecuciones

- Consulte el **historial** filtrando por trabajo, rango de fechas (en UTC) y paginación.
- Cada ítem muestra, entre otros datos: identificador del historial, trabajo, fechas de inicio y fin, duración (cuando el estado lo permite), estado (completado, fallido, en progreso, pendiente), número de archivos copiados, si el disparo fue **manual** o **programado**, y mensaje de error si hubo fallo.

---

## 11. Log de acciones de usuario (auditoría)

- Permite revisar las últimas acciones registradas (creación, modificación, borrado) sobre entidades clave.
- El listado tiene un **límite** configurable por consulta (con un máximo razonable para no sobrecargar el navegador).

---

## 12. Configuración global

### 12.1 Tiempo de espera de scripts (PRE/POST)

- Valor en **minutos** entre un mínimo y un máximo definidos por el sistema (típicamente de **1 minuto** hasta **24 horas**).
- Afecta a las ejecuciones de scripts asociados a trabajos.

---

## 13. Bandeja de sistema (solo Windows)

- Puede aparecer un **icono** en el área de notificación.
- Al pulsarlo suele abrirse el **navegador** en la URL configurada (por ejemplo la interfaz Angular en desarrollo o la URL de producción).
- Al **cerrar** la aplicación, el icono desaparece de forma controlada.

---

## 14. Documentación técnica de la API (desarrollo)

- En entorno de **desarrollo**, la API puede exponer documentación interactiva (OpenAPI/Scalar), por ejemplo en la ruta **`/scalar/v1`**.
- Útil para integradores o para soporte avanzado; el usuario final normalmente usará solo la **interfaz web**.

---

## 15. Problemas frecuentes

| Situación | Qué comprobar |
|-----------|----------------|
| «La carpeta no existe en el equipo donde se ejecuta la API» | La ruta del **origen** es del **servidor**, no del PC del navegador. |
| No puedo borrar un destino | Hay **trabajos** que lo usan; elimínelos o cambie el destino en esos trabajos. |
| No puedo borrar un script | Está asignado como **PRE/POST** en algún trabajo. |
| El trabajo programado no corre | Compruebe que está **activo**, que la expresión **cron** coincide con la hora **UTC**, y que no hay otra ejecución bloqueando el mismo minuto. |
| Error de credenciales en la nube | Use las acciones de **validar conexión** y revise permisos IAM, SAS o uso compartido en Drive. |
| Script que no arranca (Node, PowerShell, etc.) | En el servidor deben existir los intérpretes o rutas configuradas según el tipo de script (`.js`, `.ps1`, …). |

---

## 16. Buenas prácticas

1. Cambie la contraseña por defecto y use una **contraseña fuerte**.
2. **Valide** destinos antes de crearlos y use **prefijos de carpeta** claros por aplicación o cliente.
3. **Pruebe** primero un trabajo con **ejecución manual** antes de depender solo del cron.
4. Revise periódicamente el **historial** y los **logs** del servidor (`Logs` junto a la API) si algo falla.
5. Mantenga **copias de seguridad** de la base de datos y de la configuración de despliegue.

