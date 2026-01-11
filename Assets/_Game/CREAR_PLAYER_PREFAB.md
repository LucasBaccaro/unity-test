# 🎮 Crear Player Prefab (3 minutos)

## Opción Rápida: Método 1 (Recomendado)

### Paso 1: Crear el GameObject

1. En **Hierarchy** → clic derecho → **Create Empty**
2. Renombrar a: **"Player"**
3. Position: X=0, Y=0, Z=0

### Paso 2: Añadir Visual (Cápsula)

1. Clic derecho en **Player** → **3D Object → Capsule**
2. Esto crea una cápsula como hijo de Player

### Paso 3: Añadir Componentes de Networking

1. Selecciona **Player** (el padre, no la Capsule)
2. **Add Component** → busca **"Network Identity"**
   - ✅ Marca el checkbox **"Local Player Authority"**
3. **Add Component** → busca **"Network Transform Reliable"**
   - ✅ Sync Position: ON
   - ✅ Sync Rotation: ON
   - ❌ Sync Scale: OFF

### Paso 4: Añadir Character Controller

1. Con **Player** seleccionado
2. **Add Component** → busca **"Character Controller"**
   - Center: X=0, Y=0, Z=0
   - Radius: 0.5
   - Height: 2

### Paso 5: Añadir Nuestros Scripts

1. Con **Player** seleccionado
2. **Add Component** → busca **"Player Controller"**

**IMPORTANTE**: Este script tiene `[RequireComponent]`, así que automáticamente añadirá:
- Player Movement
- Player Stats
- Player Inventory
- Player Class Selector
- Zone Detector

Si Unity pregunta "¿Añadir componentes requeridos?", haz clic en **Sí/OK**.

### Paso 6: Convertir a Prefab

1. Arrastra el GameObject **"Player"** desde **Hierarchy**
2. Suéltalo en la carpeta: `Assets/_Game/Prefabs/Player/`
3. Unity creará **Player.prefab**
4. **Elimina** la instancia de Player de la Hierarchy (solo queremos el prefab)

### Paso 7: Asignar al NetworkManager

1. Selecciona **NetworkManager** en Hierarchy
2. En el componente **Network Manager MMO**:
   - Arrastra **Player.prefab** (desde `Assets/_Game/Prefabs/Player/`) al campo **"Player Prefab"**

### ✅ Verificación

El prefab **Player** debería tener estos componentes:

```
Player
├── Capsule (Mesh Filter)
├── Capsule (Mesh Renderer)
├── Capsule Collider
├── Network Identity ✓ Local Player Authority
├── Network Transform Reliable
├── Character Controller
├── Player Controller (Script)
├── Player Movement (Script)
├── Player Stats (Script)
├── Player Inventory (Script)
├── Player Class Selector (Script)
└── Zone Detector (Script)
```

Y como hijo:
```
└── Capsule (modelo 3D visual)
```

---

## ⚠️ Problemas Comunes

### "No aparece Player Controller en Add Component"
**Solución**: Verifica que no haya errores de compilación en la consola. Si los hay, arregla primero esos errores.

### "Unity no añade los componentes requeridos automáticamente"
**Solución**: Añádelos manualmente uno por uno:
1. Player Movement
2. Player Stats
3. Player Inventory
4. Player Class Selector
5. Zone Detector

### "El prefab no se crea"
**Solución**:
1. Verifica que la carpeta `Assets/_Game/Prefabs/Player/` exista
2. Si no existe, créala: clic derecho en `_Game/Prefabs` → Create → Folder → nombre: "Player"

### "Network Identity no tiene 'Local Player Authority'"
**Solución**: Es un checkbox que aparece en el Inspector cuando seleccionas el componente Network Identity. Asegúrate de marcarlo.

---

## 🎨 Opcional: Hacer el Jugador Verde

Si quieres que tu jugador local sea verde (y los remotos azules):

### Crear Material Verde

1. En `Assets/_Game/` → clic derecho → **Create → Material**
2. Nombre: **"Mat_JugadorLocal"**
3. En Inspector:
   - Color: Verde brillante (R=0, G=255, B=0)

### Crear Material Azul

1. Clic derecho → **Create → Material**
2. Nombre: **"Mat_JugadorRemoto"**
3. Color: Azul (R=0, G=100, B=255)

### Asignar al Player Controller

1. Abre **Player.prefab** (doble clic en `Prefabs/Player/Player.prefab`)
2. Selecciona el objeto raíz **Player**
3. En el componente **Player Controller**:
   - Arrastra **Mat_JugadorLocal** al campo "Material Jugador Local"
   - Arrastra **Mat_JugadorRemoto** al campo "Material Jugador Remoto"
4. **Guardar** el prefab (Ctrl+S)

---

## 🚀 Próximo Paso

Una vez que el prefab esté creado y asignado al NetworkManager:

1. **Guarda la escena** (Ctrl+S)
2. Da **Play** ▶
3. El jugador debería aparecer automáticamente (gracias a AutoStartHost)

Si sigue sin aparecer, lee **FIX_NO_APARECE_JUGADOR.md**.

---

**Tiempo estimado**: 3-5 minutos
