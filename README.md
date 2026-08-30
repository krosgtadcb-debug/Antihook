# Antihook

Especificación inicial de un dashboard estilo Steam para Rust, Battlefield 3 y Battlefield 4, con cliente Windows Forms en C#, servidor WebSocket, SQLite, panel administrativo y componentes anticheat.

## Diseño y experiencia de usuario

- **Interfaz visual:** Ventana sin bordes clásicos de Windows, barra superior personalizada con controles de minimizar y cerrar, y paleta oscura inspirada en Steam: fondos `#1b2838`, paneles `#171a21` y acentos `#66c0f4`.
- **Pantalla de juegos:** Tres portadas o tarjetas interactivas para Rust, Battlefield 3 y Battlefield 4.
- **Vista de servidores:** Transición hacia una lista detallada por juego con nombre del servidor, mapa, jugadores en línea y ping, además de unión con un clic previa validación de seguridad.

## Arquitectura del cliente

- Windows Forms en C#.
- Registro e inicio de sesión conectados asíncronamente mediante WebSockets al servidor central.
- Generación local de un hash HWID a partir de identificadores del hardware, como placa base, disco duro y procesador.
- Verificación de la ejecución y respuesta activa del driver anticheat antes de habilitar las funciones principales.

## Arquitectura del servidor

- .NET Framework y SQLite.
- Servidor WebSocket para conexiones simultáneas en tiempo real y paquetes JSON de autenticación, listados de servidores, telemetría y comandos administrativos.
- Base de datos `server.db` con tablas para usuarios, baneos y registros históricos.

## Panel administrativo

- **Usuarios conectados:** Tabla en tiempo real con nombre, HWID, IP y tiempo conectado, con acciones de Kick y Ban por HWID.
- **Logs:** Consola cronológica para conexiones, autenticaciones, intentos de bypass y bloqueos.

## Anticheat a nivel kernel

- Driver en C/C++ cargado en el núcleo de Windows.
- Uso de callbacks para impedir que depuradores o inyectores externos lean o escriban en la memoria de la aplicación.
- Mitigación de inyección de código y control de integridad frente a drivers vulnerables conocidos (BYOVD).
- Comunicación protegida entre el driver, el cliente C# y el servidor central.

> La implementación de componentes a nivel kernel debe someterse a revisión de seguridad, pruebas en entornos aislados, firma adecuada de controladores, cumplimiento de las políticas de Windows y consideración de privacidad, consentimiento y protección de datos.

## Autor

**OxyMonster**

## Estado

Especificación inicial. No se incluye código operativo en esta versión.

## Archivo fuente

El texto original recibido se conserva sin modificaciones en [`pasted_content.txt`](./pasted_content.txt).

## Licencia

Pendiente de definir por el propietario del proyecto.
