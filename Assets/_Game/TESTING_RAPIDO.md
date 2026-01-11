# Testing Rápido - MMO MVP

## ✅ Verificación Inicial

Primero, verifica que **NO haya errores en la consola de Unity**.

Si hay errores, cópialos y avísame. Si la consola está limpia, continúa.

---

## 🎮 PASO 1: Configuración Mínima (5 minutos)

### 1.1 Crear Player Prefab Básico

1. En **Hierarchy**, crea: `Create Empty` → nombre: **"Player"**

2. Selecciona **Player** y añade estos componentes (Add Component):
   - **Capsule** (3D Object → Capsule como child)
   - **Network Identity**
     - ✅ Marca **"Local Player Authority"**
   - **Network Transform Reliable**
     - ✅ Sync Position
     - ✅ Sync Rotation
   - **Character Controller**
   - **Player Controller** (nuestro script)
   - **Player Movement** (nuestro script)
   - **Player Stats** (nuestro script)
   - **Player Inventory** (nuestro script)
   - **Player Class Selector** (nuestro script)
   - **Zone Detector** (nuestro script)

   **NOTA**: Los últimos 6 scripts se añadirán automáticamente por `[RequireComponent]`, solo necesitas añadir `Player Controller`.

3. Arrastra **Player** desde Hierarchy a `Assets/_Game/Prefabs/Player/` para crear el prefab

4. Elimina la instancia de Player de la Hierarchy (solo queremos el prefab)

### 1.2 Configurar Escena GameWorld

1. Abre la escena **GameWorld** que creaste

2. Añadir iluminación:
   - Hierarchy → Create → **Light → Directional Light**

3. Añadir suelo:
   - Hierarchy → Create → **3D Object → Plane**
   - Renombrar a "Suelo"
   - Scale: X=5, Y=1, Z=5

4. Añadir cámara:
   - Hierarchy → Create → **Camera**
   - Tag: MainCamera
   - Position: X=0, Y=15, Z=-15
   - Rotation: X=45, Y=0, Z=0

5. Crear NetworkManager:
   - Hierarchy → Create Empty → nombre: **"NetworkManager"**
   - Add Component → **"Network Manager MMO"** (nuestro script)
   - En el Inspector:
     - **Transport**: Debería auto-asignarse KcpTransport
     - **Player Prefab**: Arrastra tu Player.prefab aquí
     - **Max Jugadores**: 5

6. Crear SpawnPoint:
   - Hierarchy → Create Empty → nombre: **"SpawnPoint_Ciudad"**
   - Position: X=0, Y=1, Z=0
   - Arrastra a NetworkManager → campo **"Spawn Point Ciudad"**

7. Crear ItemDatabase:
   - Hierarchy → Create Empty → nombre: **"ItemDatabase"**
   - Add Component → **"Item Database"** (nuestro script)

8. Guardar escena: `Ctrl+S`

### 1.3 Build Settings

1. `File > Build Settings`
2. **Add Open Scenes** (debe aparecer GameWorld)
3. Cerrar (NO hacer build todavía)

---

## 🧪 PASO 2: Testing Básico (Solo Editor)

### 2.1 Test en Play Mode

1. Asegúrate que GameWorld está abierta
2. Haz clic en **Play** ▶

**Verificar en la consola**:
- Deberías ver logs de inicialización:
  - `[NetworkManager_MMO] Inicializado`
  - `[PlayerStats] Stats por defecto inicializadas`
  - `[ItemDatabase] Inicializada con X items`
  - `[PlayerClassSelector] Auto-seleccionando Mago (temporal)...`

3. En la pestaña **Inspector** del NetworkManager:
   - Haz clic en botón **"Start Host"**

**Qué debería pasar**:
- Un jugador (cápsula) debería aparecer en el mundo
- La consola debería mostrar:
  - `[SERVER] Nuevo jugador conectándose`
  - `[SERVER] Jugador spawneado en posición...`
  - `[PlayerClassSelector] Seleccionando Mago...`
  - `[SERVER] Clase 'Mago' asignada exitosamente`

4. Prueba moverte:
   - **WASD**: Movimiento
   - **Q/E**: Rotación

**Si funciona**: ¡Excelente! El networking básico está funcionando.

**Si NO aparece el jugador o hay errores**: Avísame y copia los errores de la consola.

---

## 🧪 PASO 3: Testing Multiplayer (Build)

**Solo si el PASO 2 funcionó correctamente**:

### 3.1 Hacer Build

1. `File > Build Settings`
2. **Build** (crea carpeta en Escritorio: "MMO_Test")
3. Espera a que compile (1-2 minutos)

### 3.2 Probar con 2 Jugadores

1. **Ejecuta el .exe**
   - En el menú de Mirror, haz clic: **"Start Host"**
   - Deberías ver el mundo y tu jugador

2. **En Unity Editor**:
   - Asegura que GameWorld está abierta
   - **Play** ▶
   - En NetworkManager Inspector: **"Start Client"**

**Qué debería pasar**:
- En el .exe deberías ver 2 cápsulas (tu jugador local + el del editor)
- En Unity deberías ver 2 cápsulas (tu jugador local + el del exe)
- Ambos se pueden mover con WASD
- El movimiento se sincroniza entre ambos

---

## ✅ Checklist de Funcionalidad

Marca lo que funcione:

### Networking Básico
- [ ] Jugador se spawnea al hacer Start Host
- [ ] Se puede mover con WASD
- [ ] Se puede rotar con Q/E
- [ ] Aparece cápsula verde (jugador local)

### Stats y Clase
- [ ] Consola muestra "Clase Mago asignada"
- [ ] Stats inicializadas (HP, Mana, etc.)
- [ ] No hay errores en consola

### Inventario
- [ ] ItemDatabase se inicializa sin errores
- [ ] Inventario tiene 20 slots (verificar en consola)

### Multiplayer (si hiciste build)
- [ ] 2 jugadores conectados (exe + editor)
- [ ] Ambos se ven entre sí
- [ ] Movimiento sincronizado
- [ ] No hay lag excesivo

---

## 🐛 Problemas Comunes

### "Player no aparece"
- Verifica que Player Prefab esté asignado en NetworkManager
- Verifica que Player tenga Network Identity
- Revisa consola por errores

### "No puedo moverme"
- Verifica que Player tenga Character Controller
- Verifica que Player Movement tenga "Puede Moverse" en ON
- Asegura que estás haciendo clic en Start Host, no solo Play

### "Errores de compilación"
- Asegura que Mirror esté instalado correctamente
- Verifica que todos los scripts estén en las carpetas correctas
- Copia el error completo y avísame

### "ItemDatabase no se encuentra"
- Crea un GameObject vacío llamado "ItemDatabase"
- Añádele el componente Item Database
- Debe estar en la escena GameWorld

---

## 📸 Capturas de Referencia

**Scene View debería verse así:**
```
Hierarchy:
├── Directional Light
├── Suelo (Plane)
├── Main Camera
├── NetworkManager (con NetworkManager_MMO)
├── SpawnPoint_Ciudad
└── ItemDatabase
```

**Player Prefab debería tener:**
```
Player (con Capsule visual)
├── Network Identity (✓ Local Player Authority)
├── Network Transform Reliable
├── Character Controller
├── Player Controller
├── Player Movement
├── Player Stats
├── Player Inventory
├── Player Class Selector
└── Zone Detector
```

---

## 🎯 Siguiente Paso

Una vez que esto funcione:

1. **Dime qué funcionó** (usa el checklist arriba)
2. **Si hay errores**, copia la consola completa
3. **Si todo funciona**, podemos:
   - Crear las 4 clases con el configurator
   - Crear algunas zonas safe/unsafe
   - Añadir items al inventario

---

**Tiempo estimado**: 10-15 minutos si todo va bien

¡Avísame cómo te va!
