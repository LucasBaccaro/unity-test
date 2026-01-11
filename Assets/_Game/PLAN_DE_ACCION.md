Plan de Implementación - MMO MVP (Tipo Argentum/Albion Online)

     Resumen Ejecutivo

     Vamos a crear un MVP de MMO con Mirror Networking que incluye:
     - 4 clases: Mago, Paladin, Clerigo, Cazador (cada uno con 2 habilidades)
     - Combate en tiempo real con targeting y cooldowns
     - Full loot: al morir pierdes todo el inventario
     - Zonas safe/unsafe: ciudad segura + zona PvP
     - Sistema de inventario: slot-based (20 slots) + bóveda (50 slots)
     - Sistema de quests: cadena lineal de 5-10 quests que dan XP
     - NPCs: vendors, quest givers, enemigos
     - Persistencia en memoria: sin base de datos, datos se mantienen mientras servidor activo
     - 2-5 jugadores simultáneos para testing

     Tiempo estimado: 27-37 horas (~1-1.5 semanas)

     ---
     Estado Actual del Proyecto

     Unity Version: 6000.3.3f1 (Unity 6 con URP)

     Paquetes instalados:
     - ✅ Multiplayer Center (1.0.1)
     - ✅ AI Navigation (2.0.9)
     - ✅ Input System (1.17.0)
     - ✅ URP (17.3.0)
     - ❌ Mirror Networking (necesita instalarse)

     Código existente: Mínimo (solo HolaMundo.cs de prueba)

     ---
     Arquitectura del Proyecto

     Estructura de Carpetas

     Assets/_Game/
     ├── Scripts/
     │   ├── Core/               # NetworkManager_MMO, GameManager
     │   ├── Player/             # PlayerController, PlayerStats, PlayerCombat, PlayerInventory
     │   ├── Classes/            # Mago, Paladin, Clerigo, Cazador + HabilidadBase
     │   ├── Combat/             # TargetingSystem, DamageSystem, DeathHandler, CooldownManager
     │   ├── Items/              # ItemData, ItemDatabase, ItemDrop, ItemPickup
     │   ├── Inventory/          # InventoryManager, VaultManager
     │   ├── NPCs/               # VendorNPC, QuestNPC, EnemyNPC, NPCSpawner
     │   ├── Quests/             # QuestData, QuestManager, QuestTracker
     │   ├── Zones/              # ZoneController, SafeZone, UnsafeZone
     │   ├── Progression/        # ExperienceManager, LevelSystem
     │   └── UI/                 # InventoryUI, CharacterSheetUI, QuestLogUI, VendorUI, etc.
     ├── Prefabs/
     │   ├── Player/             # PlayerPrefab.prefab
     │   ├── NPCs/               # VendorNPC.prefab, EnemyNPC.prefab
     │   └── UI/                 # MainCanvas.prefab, InventoryPanel.prefab
     ├── Data/                   # ScriptableObjects (items, quests, clases)
     │   ├── Items/
     │   ├── Quests/
     │   └── Classes/
     └── Scenes/
         ├── MenuPrincipal.unity
         ├── GameWorld.unity
         └── TestScene.unity

     Principios Arquitectónicos

     1. Server Authority: TODO el juego lógica importante (combate, items, XP) se ejecuta en el servidor
     2. Commands: Cliente solicita acciones al servidor usando [Command]
     3. SyncVars: Servidor sincroniza estado a clientes usando [SyncVar]
     4. ScriptableObjects: Data del juego (items, quests, clases) en assets reutilizables
     5. Separación de concerns: Cada sistema en su propio script

     ---
     Fases de Implementación

     FASE 0: PREPARACIÓN (30 min)

     Objetivo: Instalar Mirror y configurar proyecto

     Tareas:
     1. Instalar Mirror Networking:
       - Package Manager → Add package from git URL
       - URL: https://github.com/MirrorNetworking/Mirror.git
       - O desde Asset Store (Mirror Networking)
     2. Crear estructura de carpetas arriba mencionada
     3. Eliminar o archivar HolaMundo.cs
     4. Crear escenas: MenuPrincipal.unity, GameWorld.unity, TestScene.unity

     Milestone: Mirror instalado, proyecto organizado

     ---
     FASE 1: NETWORKING BÁSICO (2-3h)

     Objetivo: Múltiples jugadores conectándose y viéndose

     Scripts a crear:
     Assets/_Game/Scripts/Core/NetworkManager_MMO.cs
     Assets/_Game/Scripts/Player/PlayerController.cs
     Assets/_Game/Scripts/Player/PlayerMovement.cs

     Conceptos clave:
     - NetworkManager de Mirror gestiona conexiones
     - NetworkBehaviour para scripts con lógica de red
     - [SyncVar] sincroniza variables del servidor a clientes
     - hasAuthority determina quién controla el objeto
     - [Command] envía acciones del cliente al servidor

     Prefabs:
     - PlayerPrefab.prefab: Cubo primitivo + NetworkIdentity + NetworkTransform + PlayerController

     Testing:
     - Build → Run exe como servidor
     - Editor → Conectar como cliente
     - Verificar que ambos jugadores se vean y muevan

     Milestone: 2-5 jugadores se conectan, ven y mueven

     ---
     FASE 2: SISTEMA DE ZONAS (1-2h)

     Objetivo: Zonas seguras e inseguras detectables

     Scripts a crear:
     Assets/_Game/Scripts/Zones/ZoneController.cs
     Assets/_Game/Scripts/Zones/SafeZone.cs
     Assets/_Game/Scripts/Zones/UnsafeZone.cs
     Assets/_Game/Scripts/Zones/ZoneDetector.cs

     Scene setup:
     - Ciudad (Safe Zone): Plano verde con collider tag "SafeZone"
     - Exterior (Unsafe Zone): Plano rojo con collider tag "UnsafeZone"
     - Usar primitivos Unity (Plane, Cube) con colores para diferenciar

     Networking:
     - [SyncVar] ZoneType currentZone en PlayerController
     - Server determina zona actual basado en colliders
     - Hook para actualizar UI cuando cambia zona

     Milestone: Jugadores se mueven entre zonas, UI muestra zona actual

     ---
     FASE 3: CLASES Y STATS (2-3h)

     Objetivo: 4 clases con stats base diferentes

     Scripts a crear:
     Assets/_Game/Scripts/Player/PlayerStats.cs
     Assets/_Game/Scripts/Classes/ClaseBase.cs
     Assets/_Game/Scripts/Classes/Mago.cs
     Assets/_Game/Scripts/Classes/Paladin.cs
     Assets/_Game/Scripts/Classes/Clerigo.cs
     Assets/_Game/Scripts/Classes/Cazador.cs
     Assets/_Game/Scripts/Player/PlayerClassSelector.cs

     Stats por clase:
     Mago:      HP: 80,  Mana: 150, Damage: 25, Defense: 5,  Speed: 4
     Paladin:   HP: 150, Mana: 50,  Damage: 20, Defense: 15, Speed: 3
     Clerigo:   HP: 100, Mana: 120, Damage: 12, Defense: 8,  Speed: 4
     Cazador:   HP: 110, Mana: 70,  Damage: 22, Defense: 6,  Speed: 6

     ScriptableObject:
     - Crear ClaseBase.cs como ScriptableObject
     - Subclases heredan y configuran valores específicos
     - [CreateAssetMenu] para crear assets desde editor

     Networking:
     - [SyncVar] para HP, Mana, Level, XP
     - [Command] CmdSelectClass() para elegir clase
     - Servidor valida y asigna clase

     Milestone: Jugadores eligen clase, stats se muestran en HUD

     ---
     FASE 4: INVENTARIO Y ITEMS (3-4h)

     Objetivo: Inventario slot-based sincronizado

     Scripts a crear:
     Assets/_Game/Scripts/Items/ItemData.cs
     Assets/_Game/Scripts/Items/ItemDatabase.cs
     Assets/_Game/Scripts/Items/InventorySlot.cs
     Assets/_Game/Scripts/Inventory/InventoryManager.cs
     Assets/_Game/Scripts/Player/PlayerInventory.cs
     Assets/_Game/Scripts/UI/InventoryUI.cs

     Conceptos clave:
     - ItemData.cs como ScriptableObject (itemID, name, type, value)
     - InventorySlot struct serializable (itemID + cantidad)
     - SyncList<InventorySlot> para sincronizar inventario completo
     - Solo sincronizar IDs (no ScriptableObjects completos)
     - ItemDatabase singleton para lookup de items

     Inventario:
     - 20 slots (grid 4x5 o 5x4)
     - Drag & drop entre slots (envía Command al servidor)
     - Servidor valida y ejecuta movimientos

     Networking:
     - CRÍTICO: Solo servidor modifica inventario
     - Cliente envía Commands, servidor valida
     - SyncList.Callback para actualizar UI

     Milestone: Inventario funcional, items se pueden mover

     ---
     FASE 5: COMBATE Y HABILIDADES (4-5h)

     Objetivo: Combate en tiempo real con targeting

     Scripts a crear:
     Assets/_Game/Scripts/Combat/TargetingSystem.cs
     Assets/_Game/Scripts/Combat/DamageSystem.cs
     Assets/_Game/Scripts/Player/PlayerCombat.cs
     Assets/_Game/Scripts/Classes/HabilidadBase.cs
     Assets/_Game/Scripts/Combat/CooldownManager.cs
     Assets/_Game/Scripts/Combat/ProjectileController.cs
     Assets/_Game/Scripts/UI/TargetFrameUI.cs

     Habilidades por clase (2 cada uno):
     Mago:
       1. Bola de Fuego (Projectile, 5s CD, 20 damage, 15 range)
       2. Escudo Arcano (Instant, 10s CD, +50 shield temporal)

     Paladin:
       1. Golpe Sagrado (Instant, 3s CD, 15 damage, 3 range melee)
       2. Bendición (Instant, 12s CD, +20 HP heal)

     Clerigo:
       1. Curación (Instant, 5s CD, 30 HP heal, 10 range)
       2. Destello Sagrado (Instant, 8s CD, 10 damage AoE)

     Cazador:
       1. Disparo Rápido (Projectile, 2s CD, 12 damage, 20 range)
       2. Trampa (AoE, 15s CD, 25 damage, 5 range)

     Flujo de combate:
     1. Cliente: Jugador selecciona target (Tab o click)
     2. Cliente: Presiona tecla habilidad (1 o 2)
     3. Cliente: Envía [Command] CmdUseAbility(abilityIndex)
     4. Servidor: Valida (cooldown, mana, rango, line-of-sight)
     5. Servidor: Ejecuta (consume mana, aplica daño, inicia cooldown)
     6. Servidor: Sincroniza cambios a clientes (SyncVars + ClientRpc)

     Networking:
     - Server Authority: TODO el combate en servidor
     - Validación server-side: rango, cooldown, mana, target válido
     - ClientRpc para efectos visuales (particles, sounds)

     PvP Rules:
     - Solo en zona unsafe
     - En safe zone, combate deshabilitado

     Milestone: Jugadores se atacan, habilidades funcionan, cooldowns trabajan

     ---
     FASE 6: MUERTE Y FULL LOOT (2-3h)

     Objetivo: Al morir, dropear todo el inventario

     Scripts a crear:
     Assets/_Game/Scripts/Combat/DeathHandler.cs
     Assets/_Game/Scripts/Items/ItemDrop.cs
     Assets/_Game/Scripts/Items/ItemPickup.cs

     Flujo de muerte:
     1. Servidor detecta HP <= 0
     2. Servidor llama DeathHandler.OnPlayerDeath()
     3. Para cada item en inventario:
       - Spawn ItemDrop prefab en posición jugador
       - NetworkServer.Spawn() para sincronizar
     4. Limpiar inventario del jugador muerto
     5. Respawn en ciudad con HP/Mana full, inventario vacío

     ItemDrop:
     - NetworkBehaviour con SyncVar itemID/cantidad
     - Collider trigger para detección
     - Despawn automático después de 5 minutos

     Pickup:
     - OnTriggerEnter detecta ItemDrop
     - [Command] CmdPickupItem() solicita al servidor
     - Servidor verifica espacio, agrega item, destruye drop

     Milestone: Jugadores mueren, dropean items, otros recogen

     ---
     FASE 7: NPCs ENEMIGOS (2-3h)

     Objetivo: Enemigos que atacan jugadores

     Scripts a crear:
     Assets/_Game/Scripts/NPCs/NPCBase.cs
     Assets/_Game/Scripts/NPCs/EnemyNPC.cs
     Assets/_Game/Scripts/NPCs/NPCSpawner.cs

     AI básico:
     - NavMeshAgent para movimiento (usar AI Navigation package)
     - Aggro radius: 10 unidades
     - Chase player si en rango
     - Attack a 2 unidades (hacer daño cada 2 segundos)
     - Volver a spawn si pierde aggro

     Loot:
     - Al morir, dropear 1-3 items random
     - Dar XP al jugador killer (50-100 XP)

     Spawning:
     - NPCSpawner en zona unsafe
     - Respawn después de 30 segundos si mueren
     - Server Authority: AI solo en servidor

     Milestone: NPCs atacan jugadores, pueden ser matados, dan XP/loot

     ---
     FASE 8: XP Y NIVELES (1-2h)

     Objetivo: Sistema de progresión

     Scripts a crear:
     Assets/_Game/Scripts/Progression/ExperienceManager.cs
     Assets/_Game/Scripts/Progression/LevelSystem.cs
     Assets/_Game/Scripts/Progression/StatsScaling.cs

     Fórmula XP:
     - XP para level N: 100 * N^2
     - Level 1→2: 100 XP
     - Level 2→3: 400 XP
     - Level 3→4: 900 XP

     Level up benefits:
     - +10 HP max
     - +5 Mana max
     - +2 Damage
     - +1 Defense

     Sources de XP:
     - Matar enemy NPC: 50-100 XP
     - Completar quest: 100-500 XP

     Milestone: Jugadores suben de nivel, stats aumentan

     ---
     FASE 9: SISTEMA DE QUESTS (3-4h)

     Objetivo: Cadena lineal de quests

     Scripts a crear:
     Assets/_Game/Scripts/Quests/QuestData.cs
     Assets/_Game/Scripts/Quests/QuestManager.cs
     Assets/_Game/Scripts/Quests/QuestTracker.cs
     Assets/_Game/Scripts/NPCs/QuestNPC.cs
     Assets/_Game/Scripts/UI/QuestLogUI.cs

     Quest chain ejemplo (5-7 quests):
     1. "Bienvenido a la Ciudad" - Hablar con Herrero (0 XP, tutorial)
     2. "Primer Equipo" - Matar 5 enemigos (100 XP, espada básica)
     3. "Cazando" - Matar 10 enemigos (200 XP, armadura)
     4. "Explorador" - Visitar zona unsafe (150 XP)
     5. "Recolector" - Recolectar 10 items de enemigos (250 XP)
     6. "Desafío" - Matar 20 enemigos (400 XP, equipo mejorado)
     7. "El Héroe" - Completar todas las anteriores (500 XP)

     Quest tracking:
     - SyncList de quests activas/completadas
     - Progress tracking (3/10 enemigos)
     - UI con lista de quests
     - Markers sobre NPCs (! para nueva quest, ? para completar)

     Milestone: Quests funcionan, jugadores completan cadena

     ---
     FASE 10: VENDOR Y VAULT (2-3h)

     Objetivo: Comprar items y almacenar en banco

     Scripts a crear:
     Assets/_Game/Scripts/NPCs/VendorNPC.cs
     Assets/_Game/Scripts/Inventory/VaultManager.cs
     Assets/_Game/Scripts/UI/VendorUI.cs
     Assets/_Game/Scripts/UI/VaultUI.cs

     Vendor:
     - Lista de items disponibles
     - Precio en oro (agregar [SyncVar] int gold a PlayerStats)
     - Interacción: presionar E cerca del NPC
     - [Command] CmdBuyItem() para comprar

     Vault:
     - 50 slots storage
     - SyncList como inventario
     - Transfer entre inventario y vault
     - [Command] CmdDepositItem() / CmdWithdrawItem()
     - Vault personal (cada jugador tiene su propio)

     Sistema de oro:
     - Enemigos dropean 10-50 gold
     - Usar gold para comprar items

     Milestone: Vendor funcional, vault almacena items

     ---
     FASE 11: PERSISTENCIA EN MEMORIA (1-2h)

     Objetivo: Mantener datos mientras servidor activo

     Scripts a crear:
     Assets/_Game/Scripts/Core/ServerManager.cs

     Implementación:
     - Singleton ServerManager
     - Dictionary<string, PlayerData> para guardar datos
     - OnServerConnect: cargar datos si existen
     - OnServerDisconnect: guardar datos en dictionary
     - OnApplicationQuit: datos se pierden (no hay DB)

     PlayerData struct:
     [Serializable]
     class PlayerData {
         public string playerName;
         public int level, xp, gold;
         public List<InventorySlot> inventory;
         public List<InventorySlot> vault;
         public List<int> completedQuests;
         public Vector3 lastPosition;
     }

     Milestone: Jugadores reconectan sin perder progreso (durante sesión)

     ---
     FASE 12: UI Y POLISH (2-3h)

     Objetivo: UI completa y funcional

     UIs a crear:
     1. MainHUD: HP/Mana/XP bars, Level, Gold, Zona actual
     2. CharacterSheetUI: Stats, clase, nivel
     3. TargetFrameUI: HP del target seleccionado
     4. CooldownUI: Iconos de habilidades con CD visual
     5. DeathScreenUI: "Has muerto" + botón respawn

     Keybinds (usando Input System):
     - WASD: Movimiento
     - Tab: Seleccionar target
     - 1-2: Usar habilidades
     - I: Inventario
     - C: Character sheet
     - L: Quest log
     - E: Interactuar con NPC
     - Esc: Cerrar paneles

     Milestone: UI completa, todos los sistemas accesibles

     ---
     Archivos Críticos (Prioridad)

     Estos son los scripts más importantes que forman la columna vertebral del sistema:

     1. NetworkManager_MMO.cs (Assets/_Game/Scripts/Core/)
       - Punto de entrada del networking
       - Gestiona spawning de jugadores
       - Configuración de conexiones
     2. PlayerController.cs (Assets/_Game/Scripts/Player/)
       - Orquestador principal del jugador
       - Referencias a todos los subsistemas
       - Demuestra arquitectura de componentes
     3. PlayerInventory.cs (Assets/_Game/Scripts/Player/)
       - Sistema de inventario con SyncList
       - Ejemplo perfecto de sincronización de datos complejos
       - Server Authority pattern
     4. PlayerCombat.cs (Assets/_Game/Scripts/Player/)
       - Sistema de combate completo
       - Commands, validación server-side
       - Targeting y habilidades
     5. DamageSystem.cs (Assets/_Game/Scripts/Combat/)
       - Cálculo centralizado de daño
       - Server Authority para anti-cheat
       - Usado por todas las fuentes de daño
     6. ItemData.cs (Assets/_Game/Scripts/Items/)
       - ScriptableObject base para items
       - Patrón data-driven design
       - Reutilizable y fácil de expandir
     7. DeathHandler.cs (Assets/_Game/Scripts/Combat/)
       - Mecánica core: full loot drop
       - Spawning de items en red
       - Respawn de jugador

     ---
     Conceptos de Networking Clave

     1. Server Authority

     CRÍTICO: Todo cálculo importante se hace en el servidor.

     // ❌ MAL - Cliente puede hacer trampa
     void UseAbility() {
         currentMana -= 20;  // Cliente modifica directamente
         target.HP -= 50;    // Cliente hace daño directamente
     }

     // ✅ BIEN - Server Authority
     [Command]
     void CmdUseAbility() {
         // Se ejecuta EN EL SERVIDOR
         if (currentMana < 20) return;  // Servidor valida
         currentMana -= 20;              // Servidor modifica
         target.TakeDamage(50);          // Servidor aplica daño
     }

     2. Atributos de Mirror

     - [Command]: Cliente → Servidor (prefijo Cmd)
     [Command]
     void CmdAttack() { /* ejecuta en servidor */ }
     - [ClientRpc]: Servidor → Todos los clientes (prefijo Rpc)
     [ClientRpc]
     void RpcPlayEffect() { /* ejecuta en todos los clientes */ }
     - [SyncVar]: Sincroniza variable automáticamente
     [SyncVar] int health;  // Servidor → Clientes
     [SyncVar(hook = nameof(OnHealthChanged))] int health;  // Con callback
     - SyncList: Sincroniza listas
     SyncList<InventorySlot> inventory;
     inventory.Callback += OnInventoryChanged;

     3. Flujo de Acción Típico

     Ejemplo: Usar habilidad

     1. Cliente: Input.GetKeyDown(KeyCode.Alpha1)
        └─> Llama: CmdUseAbility(0)

     2. Mensaje se envía por red → Servidor

     3. Servidor: Recibe CmdUseAbility(0)
        └─> Valida: cooldown, mana, rango, target
        └─> Ejecuta: consume mana, aplica daño
        └─> Sincroniza: SyncVars se actualizan automáticamente
        └─> Opcional: RpcPlayEffect() para visuales

     4. Clientes: Reciben cambios
        └─> SyncVar hooks se llaman
        └─> UI se actualiza
        └─> Efectos visuales se muestran

     4. Authority y Control

     void Update() {
         // Solo procesar input si este es NUESTRO jugador
         if (!hasAuthority) return;

         if (Input.GetKey(KeyCode.W)) {
             CmdMove(Vector3.forward);
         }
     }

     [Command]
     void CmdMove(Vector3 direction) {
         // Solo el servidor puede mover al jugador
         transform.position += direction * speed * Time.deltaTime;
     }

     5. Qué Sincronizar

     SÍ sincronizar:
     - ✅ HP, Mana, Stats
     - ✅ Posición del jugador (NetworkTransform)
     - ✅ Inventario completo
     - ✅ Quests activas
     - ✅ Target actual
     - ✅ Zona actual
     - ✅ Cooldowns

     NO sincronizar:
     - ❌ Input del jugador (solo enviar Commands)
     - ❌ UI state (local a cada cliente)
     - ❌ Efectos puramente cosméticos
     - ❌ Cálculos intermedios

     ---
     Estilo de Comentarios

     TODO el código debe estar MUY bien comentado en español.

     Ejemplo de estilo:

     using Mirror;
     using UnityEngine;

     /// <summary>
     /// Controlador principal del jugador en el MMO.
     /// Coordina todos los subsistemas: movimiento, combate, inventario, quests.
     ///
     /// NETWORKING: Hereda de NetworkBehaviour para existir en la red.
     /// Hay una copia de este objeto en el servidor Y en todos los clientes.
     /// </summary>
     public class PlayerController : NetworkBehaviour
     {
         #region Variables de Red (Sincronizadas)

         /// <summary>
         /// Vida actual del jugador.
         /// [SyncVar] sincroniza automáticamente del servidor a todos los clientes.
         /// Solo el servidor puede modificar este valor.
         ///
         /// El 'hook' hace que OnHealthChanged() se llame cada vez que cambia.
         /// </summary>
         [SyncVar(hook = nameof(OnHealthChanged))]
         private int currentHealth;

         #endregion

         #region Unity Callbacks

         /// <summary>
         /// Se llama cuando el jugador local se spawna.
         /// SOLO se ejecuta para TU jugador, no para los otros jugadores remotos.
         /// </summary>
         public override void OnStartLocalPlayer()
         {
             base.OnStartLocalPlayer();

             // Configurar cámara solo para nuestro jugador
             SetupCamera();
         }

         void Update()
         {
             // Solo procesar input si este es nuestro jugador
             if (!hasAuthority) return;

             HandleInput();
         }

         #endregion

         #region Commands (Cliente → Servidor)

         /// <summary>
         /// Command que el cliente llama para usar una habilidad.
         ///
         /// NETWORKING: [Command] hace que esta función se ejecute EN EL SERVIDOR,
         /// aunque la llamemos desde el cliente.
         ///
         /// FLUJO:
         /// 1. Cliente presiona tecla "1"
         /// 2. Cliente llama CmdUseAbility(0)
         /// 3. Mensaje se ENVÍA POR RED al servidor
         /// 4. Servidor VALIDA y EJECUTA la habilidad
         /// 5. Servidor sincroniza resultados a todos los clientes
         ///
         /// IMPORTANTE: Siempre validar en el servidor. NUNCA confiar en el cliente.
         /// </summary>
         /// <param name="abilityIndex">Índice de la habilidad (0 o 1)</param>
         [Command]
         private void CmdUseAbility(int abilityIndex)
         {
             // Este código se ejecuta EN EL SERVIDOR
             Debug.Log($"[SERVER] Jugador intenta usar habilidad {abilityIndex}");

             // Validar y ejecutar
             if (ValidateAbility(abilityIndex))
             {
                 ExecuteAbility(abilityIndex);
             }
         }

         #endregion

         #region SyncVar Hooks

         /// <summary>
         /// Se llama automáticamente cuando 'currentHealth' cambia.
         /// Se ejecuta EN TODOS LOS CLIENTES cuando el servidor modifica el valor.
         ///
         /// Perfecto para actualizar UI o triggear efectos visuales.
         /// </summary>
         private void OnHealthChanged(int oldHealth, int newHealth)
         {
             Debug.Log($"Vida cambió: {oldHealth} → {newHealth}");
             UpdateHealthUI();

             // Si murió, mostrar pantalla de muerte
             if (newHealth <= 0)
             {
                 ShowDeathScreen();
             }
         }

         #endregion
     }

     Tags a usar en comentarios:
     - NETWORKING: - Explicar conceptos de red
     - IMPORTANTE: - Resaltar puntos críticos
     - FLUJO: - Explicar secuencias paso a paso
     - NOTA: - Información adicional
     - TODO: - Cosas pendientes de implementar

     ---
     Verificación y Testing

     Testing por Fase

     Fase 1:
     - ✅ 2 jugadores conectados
     - ✅ Ambos se ven
     - ✅ Movimiento sincronizado

     Fase 2:
     - ✅ Entrar a zona safe → UI muestra "Zona Segura"
     - ✅ Salir a zona unsafe → UI muestra "Zona Peligrosa"

     Fase 3:
     - ✅ Seleccionar cada una de las 4 clases
     - ✅ Stats diferentes por clase
     - ✅ HP/Mana se muestran correctamente

     Fase 4:
     - ✅ Agregar items al inventario
     - ✅ Mover items entre slots
     - ✅ Inventario sincronizado entre clientes

     Fase 5:
     - ✅ Seleccionar target con Tab
     - ✅ Usar habilidad 1 y 2
     - ✅ Daño se aplica correctamente
     - ✅ Cooldowns funcionan
     - ✅ Mana se consume

     Fase 6:
     - ✅ Jugador A mata a jugador B
     - ✅ Jugador B dropea todos sus items
     - ✅ Jugador A puede recoger items
     - ✅ Jugador B respawnea en ciudad vacío

     Fase 7:
     - ✅ NPCs atacan jugadores en zona unsafe
     - ✅ Jugadores matan NPCs
     - ✅ NPCs dropean loot
     - ✅ NPCs respawnean después de morir

     Fase 8:
     - ✅ Matar NPC da XP
     - ✅ Subir de nivel aumenta stats
     - ✅ XP bar se actualiza correctamente

     Fase 9:
     - ✅ NPC muestra quest disponible (!)
     - ✅ Aceptar quest
     - ✅ Completar objetivo (matar X enemigos)
     - ✅ Entregar quest (?)
     - ✅ Recibir recompensa (XP + items)

     Fase 10:
     - ✅ Comprar item en vendor
     - ✅ Gold se descuenta
     - ✅ Depositar item en vault
     - ✅ Retirar item de vault

     Fase 11:
     - ✅ Desconectar jugador
     - ✅ Reconectar mismo jugador
     - ✅ Inventario y progreso intactos

     Fase 12:
     - ✅ Todas las UIs funcionan
     - ✅ Keybinds responden correctamente

     Checklist Final MVP

     Antes de considerar el MVP completo:

     - 5 jugadores simultáneos sin lag significativo
     - 4 clases jugables (Mago, Paladin, Clerigo, Cazador)
     - 8 habilidades totales (2 por clase) funcionando
     - Combate PvP funcional en zona unsafe
     - Full loot al morir (pierdes todo)
     - Zonas safe/unsafe correctamente implementadas
     - Inventario de 20 slots funcionando
     - Vault de 50 slots funcionando
     - 5-10 quests en cadena lineal
     - Vendor vendiendo items
     - NPCs enemigos spawneando y atacando
     - Sistema de XP y level up
     - Persistencia durante sesión de servidor
     - Sin errores en consola
     - Framerate estable con 5 jugadores
     - TODO comentado en español

     ---
     Próximos Pasos (Post-MVP)

     Una vez completado el MVP, puedes expandir con:

     1. Más clases (Guerrero, Arquero, Nigromante, etc.)
     2. Más habilidades (4-6 por clase)
     3. Sistema de guilds (clanes de jugadores)
     4. Dungeons (instancias para grupos)
     5. Crafting (fabricación de items)
     6. Trading (comercio entre jugadores)
     7. Mounts (monturas)
     8. World bosses (jefes de mundo)
     9. Base de datos real (PostgreSQL/MongoDB)
     10. Gráficos 3D (modelos, animaciones, efectos)

     ---
     Recursos

     Mirror Networking:
     - Documentación: https://mirror-networking.gitbook.io/docs/
     - GitHub: https://github.com/MirrorNetworking/Mirror
     - Ejemplos incluidos: Tanks, Pong, Benchmark

     Unity Packages Necesarios:
     - Mirror Networking (instalar en Fase 0)
     - AI Navigation (ya instalado)
     - Input System (ya instalado)
     - TextMeshPro (para UI mejorada, opcional)

     Referencias de Juegos:
     - Argentum Online: Sistema de combate, zonas, NPCs
     - Albion Online: Full loot, clases, zones, inventario

     ---
     Notas Importantes

     1. No optimizar prematuramente: Enfocarse en funcionalidad primero, optimización después
     2. Testing constante: Probar después de cada fase antes de continuar
     3. Server Authority siempre: NUNCA confiar en el cliente para lógica crítica
     4. Comentar TODO: Este es un proyecto para aprender, comentarios son cruciales
     5. Usar primitivos: Cubos/esferas para jugadores, planos para terreno. Gráficos después.
     6. Mirror tiene ejemplos: Revisar ejemplos de Mirror para patrones comunes
     7. Build frecuentemente: Hacer builds para testing con múltiples instancias
     8. Git: Considerar usar Git para version control (proyecto usa serialización de texto)

     ---
     Tiempo total estimado: 27-37 horas (~1-1.5 semanas full-time, o 2-3 semanas part-time)

     Complejidad: Media-Alta (requiere entender networking, pero Mirror simplifica mucho)

     Resultado: MVP funcional de MMO que demuestra todos los conceptos core de networking en Unity