# FASE 1: Configuración en Unity Editor

## ✅ Scripts Creados

Ya creé los siguientes scripts (no necesitas hacer nada con ellos):
- ✅ `NetworkManager_MMO.cs` - Gestor de red del MMO
- ✅ `PlayerController.cs` - Controlador principal del jugador
- ✅ `PlayerMovement.cs` - Sistema de movimiento

## 📋 Pasos a Seguir en Unity Editor

Ahora necesitas configurar las escenas y prefabs en Unity. Sigue estos pasos **exactamente**:

---

### PASO 1: Crear Escenas

1. En Unity, ve a `File > New Scene`
2. Cuando aparezca el diálogo, selecciona **"Empty Scene"**
3. Guarda la escena:
   - `File > Save As...`
   - Navega a: `Assets/_Game/Scenes/`
   - Nombre: **GameWorld**
   - Haz clic en **Save**

4. Repite para crear otra escena:
   - `File > New Scene` > Empty Scene
   - Guarda como: `Assets/_Game/Scenes/TestScene`

**Resultado**: Deberías tener 2 escenas nuevas en `Assets/_Game/Scenes/`

---

### PASO 2: Configurar Escena GameWorld (Mundo del Juego)

1. Abre la escena **GameWorld** (doble clic en `Assets/_Game/Scenes/GameWorld.unity`)

2. **Añadir iluminación básica**:
   - Clic derecho en Hierarchy > `Light > Directional Light`
   - En el Inspector, configura:
     - Rotation: X=50, Y=-30, Z=0
     - Color: Blanco

3. **Añadir suelo**:
   - Clic derecho en Hierarchy > `3D Object > Plane`
   - Renombrar a: **"Suelo"**
   - En el Inspector, configura:
     - Position: X=0, Y=0, Z=0
     - Scale: X=5, Y=1, Z=5
   - Esto crea un suelo de 50x50 unidades

4. **Añadir cámara**:
   - Clic derecho en Hierarchy > `Camera`
   - Renombrar a: **"MainCamera"**
   - Tag: Asegúrate que tenga tag "MainCamera"
   - Position: X=0, Y=15, Z=-15
   - Rotation: X=45, Y=0, Z=0
   - Esto posiciona la cámara con vista isométrica

5. **Crear GameObject para NetworkManager**:
   - Clic derecho en Hierarchy > `Create Empty`
   - Renombrar a: **"NetworkManager"**
   - Position: X=0, Y=0, Z=0

6. **Añadir componente NetworkManager_MMO**:
   - Selecciona el GameObject "NetworkManager" en Hierarchy
   - En el Inspector, haz clic en **Add Component**
   - Busca: **"NetworkManager MMO"** (nuestro script)
   - Haz clic para añadirlo

   **IMPORTANTE**: Si no aparece el script:
   - Asegúrate que no haya errores de compilación en la consola
   - Ve a `Assets > Refresh`
   - Intenta de nuevo

7. **Configurar NetworkManager_MMO**:
   - En el componente NetworkManager_MMO que acabas de añadir:
   - **Transport**: Deberías ver "KcpTransport" (viene con Mirror)
   - **Network Address**: Deja "localhost"
   - **Max Jugadores**: 5
   - **Player Prefab**: Lo configuraremos en PASO 3
   - **Spawn Point Ciudad**: Lo configuraremos en PASO 4

8. **Crear Spawn Point (punto de aparición)**:
   - Clic derecho en Hierarchy > `Create Empty`
   - Renombrar a: **"SpawnPoint_Ciudad"**
   - Position: X=0, Y=0.5, Z=0 (ligeramente arriba del suelo)

9. **Asignar Spawn Point al NetworkManager**:
   - Selecciona "NetworkManager" en Hierarchy
   - Arrastra "SpawnPoint_Ciudad" desde Hierarchy al campo **"Spawn Point Ciudad"** en el Inspector

10. **Guardar escena**: `Ctrl+S` (Cmd+S en Mac)

---

### PASO 3: Crear Player Prefab

1. **Crear modelo temporal del jugador**:
   - Clic derecho en Hierarchy > `3D Object > Capsule`
   - Renombrar a: **"Player"**
   - Position: X=0, Y=0, Z=0
   - Scale: X=1, Y=1, Z=1

2. **Añadir NetworkIdentity** (CRÍTICO):
   - Selecciona "Player" en Hierarchy
   - En Inspector, **Add Component**
   - Busca: **"Network Identity"**
   - Añádelo
   - En el componente Network Identity:
     - ✅ Marca **"Local Player Authority"** (checkbox)
     - Esto permite que el cliente controle su propio jugador

3. **Añadir NetworkTransform** (sincroniza posición):
   - Con "Player" seleccionado
   - **Add Component**
   - Busca: **"Network Transform Reliable"** (usa este, no el "Network Transform" normal)
   - Configuración:
     - Sync Position: ✅ ON
     - Sync Rotation: ✅ ON
     - Sync Scale: ❌ OFF
     - Interpolate Position: ✅ ON (suaviza movimiento)

4. **Añadir CharacterController** (para movimiento):
   - Con "Player" seleccionado
   - **Add Component**
   - Busca: **"Character Controller"**
   - Configuración:
     - Center: X=0, Y=0, Z=0
     - Radius: 0.5
     - Height: 2
     - Slope Limit: 45
     - Step Offset: 0.3

5. **Añadir PlayerController** (nuestro script):
   - Con "Player" seleccionado
   - **Add Component**
   - Busca: **"Player Controller"** (nuestro script)
   - Añádelo
   - Configuración:
     - Material Jugador Local: Dejar vacío por ahora (verde)
     - Material Jugador Remoto: Dejar vacío por ahora (azul)

6. **Añadir PlayerMovement** (nuestro script):
   - Con "Player" seleccionado
   - **Add Component**
   - Busca: **"Player Movement"**
   - Configuración:
     - Velocidad Movimiento: 5
     - Velocidad Rotacion: 300
     - Puede Moverse: ✅ ON

7. **Crear materiales de colores** (para distinguir jugadores):

   **Material Verde (jugador local)**:
   - En Project, navega a `Assets/_Game/`
   - Clic derecho > `Create > Material`
   - Nombre: **"Mat_JugadorLocal"**
   - En Inspector:
     - Color: Verde brillante (R=0, G=255, B=0)

   **Material Azul (jugadores remotos)**:
   - Clic derecho > `Create > Material`
   - Nombre: **"Mat_JugadorRemoto"**
   - En Inspector:
     - Color: Azul (R=0, G=100, B=255)

8. **Asignar materiales al PlayerController**:
   - Selecciona "Player" en Hierarchy
   - En el componente PlayerController:
     - Arrastra **"Mat_JugadorLocal"** al campo "Material Jugador Local"
     - Arrastra **"Mat_JugadorRemoto"** al campo "Material Jugador Remoto"

9. **Convertir a Prefab**:
   - Arrastra el GameObject "Player" desde **Hierarchy** a la carpeta `Assets/_Game/Prefabs/Player/`
   - Unity te preguntará si quieres crear un Prefab → **Sí, crear Original Prefab**
   - Ahora deberías tener **Player.prefab** en `Assets/_Game/Prefabs/Player/`

10. **Asignar Prefab al NetworkManager**:
    - Selecciona "NetworkManager" en Hierarchy
    - Arrastra el prefab **"Player.prefab"** desde `Assets/_Game/Prefabs/Player/` al campo **"Player Prefab"** en el componente NetworkManager_MMO

11. **IMPORTANTE**: Elimina el GameObject "Player" de la Hierarchy (ya tenemos el Prefab, no necesitamos la instancia en la escena)

12. **Guardar escena**: `Ctrl+S`

---

### PASO 4: Configurar Build Settings

1. Ve a `File > Build Settings`
2. En "Scenes In Build":
   - Haz clic en **"Add Open Scenes"**
   - Debería aparecer `GameWorld` con índice 0
3. **Platform**: Asegúrate que esté en **"PC, Mac & Linux Standalone"**
4. Haz clic en **"Close"** (NO hagas build todavía)

---

### PASO 5: Probar el Networking

¡Es hora de probar si todo funciona!

**Método 1 - Editor + Build (RECOMENDADO)**:

1. **Hacer un Build**:
   - `File > Build Settings`
   - Haz clic en **"Build"**
   - Crea una carpeta en el escritorio llamada "MMO_Build"
   - Guarda el build ahí
   - Espera a que compile (1-2 minutos)

2. **Ejecutar como Servidor**:
   - Ejecuta el .exe que acabas de crear (MMO_Build)
   - En el menú que aparece:
     - Haz clic en **"Host (Server + Client)"**
     - Deberías ver el suelo y la luz

3. **Conectar desde Unity Editor**:
   - Vuelve a Unity Editor
   - Asegúrate que la escena GameWorld esté abierta
   - Haz clic en **Play** ▶
   - En el Scene View deberías ver el NetworkManager
   - En la pestaña Inspector del NetworkManager:
     - Haz clic en **"Start Client"**

4. **Verificar funcionamiento**:
   - Deberías ver 2 cápsulas en el mundo:
     - Una VERDE (tu jugador en el cliente)
     - Una AZUL (el jugador del servidor/host)
   - Prueba moverte con **WASD**
   - Prueba rotar con **Q/E**
   - Deberías ver que ambos jugadores se mueven sincronizados

**Método 2 - Solo Editor (alternativa si no quieres hacer build)**:

1. Instala **ParrelSync** desde Package Manager:
   - `Window > Package Manager`
   - Clic en `+` > `Add package from git URL`
   - URL: `https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync`

2. Crea un clon del proyecto:
   - `ParrelSync > Clones Manager > Create New Clone`
   - Abre el clon en otra instancia de Unity

3. En el proyecto original: Play > Start Host
4. En el proyecto clonado: Play > Start Client

---

### PASO 6: Verificación Final

Si todo está correcto, deberías tener:

✅ Escena GameWorld configurada con:
- Directional Light
- Suelo (Plane escalado)
- MainCamera con vista isométrica
- NetworkManager con NetworkManager_MMO
- SpawnPoint_Ciudad

✅ Player.prefab configurado con:
- NetworkIdentity (con Local Player Authority ON)
- NetworkTransformReliable
- CharacterController
- PlayerController (con materiales asignados)
- PlayerMovement (con velocidades configuradas)

✅ NetworkManager_MMO configurado con:
- Player Prefab asignado
- Spawn Point Ciudad asignado
- Max Jugadores = 5

✅ Al probar:
- 2 jugadores se conectan (servidor + cliente)
- Cada uno se ve como cápsula de diferente color
- Se pueden mover con WASD
- Rotar con Q/E
- El movimiento se sincroniza entre ambos

---

### 🐛 Troubleshooting (Si algo falla)

**Problema: No aparece el script NetworkManager_MMO**
- Solución: Ve a la consola, busca errores de compilación. Asegúrate que Mirror esté instalado correctamente.

**Problema: El jugador no se mueve**
- Solución: Verifica que CharacterController esté añadido. Verifica que "Puede Moverse" esté en ON.

**Problema: No se sincroniza entre servidor y cliente**
- Solución: Verifica que NetworkTransformReliable esté añadido. Verifica que NetworkIdentity tenga "Local Player Authority" marcado.

**Problema: El jugador cae infinitamente**
- Solución: Verifica que el Suelo (Plane) tenga un Collider. Los Planes tienen Box Collider por defecto.

**Problema: No se ve la cámara**
- Solución: Verifica que MainCamera tenga el tag "MainCamera". Verifica la posición (Y=15, Z=-15).

---

### 📸 Capturas Esperadas

**Hierarchy debería verse así**:
```
GameWorld
├── Directional Light
├── Suelo
├── MainCamera
├── NetworkManager
└── SpawnPoint_Ciudad
```

**Player.prefab debería tener estos componentes**:
```
Player (Capsule)
├── Transform
├── Capsule (Mesh Filter)
├── Capsule (Mesh Renderer)
├── Capsule Collider
├── Network Identity ✅ Local Player Authority
├── Network Transform Reliable
├── Character Controller
├── Player Controller (Script)
└── Player Movement (Script)
```

---

## ✅ Siguiente Paso

Una vez que hayas completado todos estos pasos y verificado que funciona:
- **Avísame** y continuaremos con **FASE 2: Sistema de Zonas** (safe/unsafe)

Si tienes algún problema o error, **copia el mensaje de error completo** y lo resolvemos juntos.

¡Mucha suerte! 🚀
