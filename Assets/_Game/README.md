# MMO MVP - Estructura del Proyecto

Este proyecto es un MMO MVP tipo Argentum Online / Albion Online desarrollado con Unity 6 y Mirror Networking.

## Estructura de Carpetas

```
_Game/
├── Scripts/              # Todo el código C#
│   ├── Core/            # Sistemas fundamentales (NetworkManager, GameManager)
│   ├── Player/          # Scripts del jugador (Controller, Stats, Combat, Inventory)
│   ├── Classes/         # 4 clases: Mago, Paladin, Clerigo, Cazador
│   ├── Combat/          # Sistema de combate (Targeting, Damage, Death, Cooldowns)
│   ├── Items/           # Sistema de items (ItemData, Database, Drop, Pickup)
│   ├── Inventory/       # Inventario y vault
│   ├── NPCs/            # NPCs (Vendor, Quest Giver, Enemigos)
│   ├── Quests/          # Sistema de misiones
│   ├── Zones/           # Zonas safe/unsafe
│   ├── Progression/     # Sistema de XP y niveles
│   └── UI/              # Interfaces de usuario
│
├── Prefabs/             # Prefabs del juego
│   ├── Player/          # Prefab del jugador
│   ├── NPCs/            # Prefabs de NPCs
│   ├── Items/           # Prefabs de items (drops)
│   ├── Combat/          # Prefabs de combate (proyectiles, efectos)
│   └── UI/              # Prefabs de UI
│
├── Data/                # ScriptableObjects (data del juego)
│   ├── Items/           # Datos de items
│   ├── Quests/          # Datos de quests
│   └── Classes/         # Configuración de clases
│
└── Scenes/              # Escenas del juego
    ├── MenuPrincipal.unity
    ├── GameWorld.unity
    └── TestScene.unity
```

## Características del MVP

- **4 Clases**: Mago, Paladin, Clerigo, Cazador (2 habilidades cada uno)
- **Combate en tiempo real**: Targeting, cooldowns, PvP
- **Full loot**: Al morir pierdes todo el inventario
- **Zonas safe/unsafe**: Ciudad segura + zona PvP
- **Inventario**: 20 slots slot-based
- **Vault**: 50 slots de almacenamiento
- **Quests**: Cadena lineal de 5-10 misiones
- **NPCs**: Vendors, quest givers, enemigos con AI
- **Progresión**: Sistema de XP y niveles
- **Networking**: 2-5 jugadores simultáneos con Mirror

## Principios Arquitectónicos

1. **Server Authority**: Todo cálculo importante en el servidor
2. **Commands**: Cliente solicita, servidor valida y ejecuta
3. **SyncVars**: Servidor sincroniza estado a clientes
4. **ScriptableObjects**: Data reutilizable del juego
5. **Separación de concerns**: Un script, una responsabilidad

## Fases de Desarrollo

0. ✅ Preparación (instalar Mirror, estructura)
1. ⏳ Networking básico
2. ⏳ Sistema de zonas
3. ⏳ Clases y stats
4. ⏳ Inventario y items
5. ⏳ Combate y habilidades
6. ⏳ Muerte y loot
7. ⏳ NPCs enemigos
8. ⏳ XP y niveles
9. ⏳ Sistema de quests
10. ⏳ Vendor y vault
11. ⏳ Persistencia en memoria
12. ⏳ UI y polish

## Recursos

- **Mirror Docs**: https://mirror-networking.gitbook.io/docs/
- **Plan completo**: Ver `/Users/lucasbaccaro/.claude/plans/federated-sauteeing-thimble.md`
