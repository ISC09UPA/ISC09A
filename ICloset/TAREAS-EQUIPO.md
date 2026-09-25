# Tareas del equipo — ICloset

Documento de coordinación. Por ahora cubre el **Día 1**.

## Roles
- P1: Backend (recibe y guarda todo)
- P2: Backend (base de datos y archivos)
- P3: App (pantallas y carruseles)
- P4: App (estilos, colores y cámara)
- P5: Pruebas, bugs y repo de GitHub

## Día 1 — por persona
| Persona | Tarea del Día 1 | Estado |
|---|---|---|
| P1 | Instalar .NET y dejar el proyecto corriendo | — |
| P2 | Agregar SQLite y crear la tabla de prendas | — |
| P3 | Crear el proyecto de React Native (scaffold Expo) | pendiente |
| P4 | Definir colores y estilos (tema) | en curso |
| P5 | Crear repo y lista de tareas (tablero) | — |

## Detalle P4 — Día 1 (nosotros)
- **Objetivo:** tema/paleta de colores + pantalla base.
- **Ruta:** `ICloset/mobile/closet-mobile/`
- **Archivos:**
  - `src/theme/theme.ts` → paleta semántica (`background`, `textPrimary`,
    `button*`, `outfitBar*`, `rail`, `border`) + `spacing`.
  - `App.tsx` → pantalla base "Mi clóset / ¡Bienvenido!" usando `colors`/`spacing`.
- **Scaffold provisional:** como P3 aún no sube el proyecto Expo, P4 crea uno
  provisional desde `proyectoWEY` para no bloquearse.
- **Rama:** `iCloset-P4-Dia1` (merge `--no-ff` a `iCloset-Develop`).
- **Commit:** `feat(iCloset): P4 día 1 - tema y paleta de colores`.

## Lo que P3 debe hacer para complementar a P4 (Día 1)
1. Crear el proyecto Expo en `ICloset/mobile/closet-mobile/` y subirlo a
   `iCloset-Develop` (es la base sobre la que P4 aplica el tema).
2. Si P4 ya creó el scaffold provisional, **adoptarlo/rebasar en vez de duplicar**
   (coordinar antes de crear otro).
3. No escribir colores sueltos (`#hex`) en pantallas o componentes: usar
   `colors.*` de `src/theme/theme.ts` y `spacing`.

## Lo que P5 debe hacer para complementar a P4 (Día 1)
1. Registrar en el tablero la tarea de P4 y avisar cuando el scaffold de P3
   esté en `iCloset-Develop`.
2. Confirmar que el `.gitignore` excluya `node_modules/`, `.expo/`, `bin/`, `obj/`.
