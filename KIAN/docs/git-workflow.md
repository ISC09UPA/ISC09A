# Flujo de Git

## Ramas

```
main
  │
  └── develop
        ├── feature/*
        ├── fix/*
        ├── refactor/*
        ├── test/*
        ├── docs/*
        └── chore/*
```

| Rama         | Propósito                                        | ¿Se trabaja directamente? |
| ------------ | ------------------------------------------------ | ------------------------- |
| `main`       | Código estable, listo para release               | **Nunca**                 |
| `develop`    | Integración de las ramas de trabajo              | **No** (solo integración) |
| `feature/*`  | Nueva funcionalidad                              | Sí                        |
| `fix/*`      | Corrección de errores                            | Sí                        |
| `refactor/*` | Cambios internos sin alterar el comportamiento   | Sí                        |
| `test/*`     | Agregar o mejorar tests                          | Sí                        |
| `docs/*`     | Solo documentación                               | Sí                        |
| `chore/*`    | Tooling, dependencias, configuración             | Sí                        |

Nombres en `kebab-case` y descriptivos:

```
feature/backend-project-setup
feature/blob-storage
feature/cards-api
feature/spaced-repetition
feature/review-screen
fix/blob-sas-url
refactor/card-service
test/cards-api
docs/project-architecture
chore/update-ef-core
```

## Reglas

1. **Nunca** hacer commit, merge local ni `git push origin main` sobre `main`.
2. **Nunca** integrar `feature/* → main` directamente. El camino siempre pasa por `develop`.
3. No implementar funcionalidades directamente en `develop`.
4. Cada integrante trabaja en su propia rama.
5. Una rama = un objetivo. No mezclar frontend y backend en la misma rama salvo que la feature lo requiera.
6. Mantener commits pequeños y coherentes.

## Flujo diario

### 1. Empezar una rama

```bash
git clone <repo-url> imagecards   # solo la primera vez
cd imagecards

git checkout develop
git pull origin develop
git checkout -b feature/mi-feature
```

### 2. Trabajar y hacer commits

```bash
git status
git branch --show-current          # confirmar que NO estás en main ni develop
git add <archivos concretos>        # evita `git add .` sin revisar antes
git commit -m "feat: add card creation endpoint"
```

### 3. Mantener la rama actualizada con develop

```bash
git fetch origin
git merge origin/develop            # opción segura, no reescribe historial
```

Un `rebase` reescribe el historial. Úsalo solo en ramas que nadie más haya descargado y
**nunca** sobre ramas compartidas.

### 4. Publicar la rama

```bash
git push -u origin feature/mi-feature
```

La integración en `develop` (y luego de `develop` a `main`) la decide el equipo. Por ahora no hay
Pull Requests ni reglas remotas configuradas.

## Antes de cualquier operación importante

1. `git status`: revisar cambios sin commit.
2. `git branch --show-current`: confirmar la rama.
3. Si estás en `main`, **detente** y cambia de rama.
4. Si hay cambios sin commit que no son tuyos, revisa su contexto antes de tocarlos.

## Comandos destructivos

Requieren confirmación explícita del equipo antes de ejecutarlos:

- `git reset --hard`
- `git clean -fd`
- `git push --force` / `git push --force-with-lease`
- `git branch -D`
- `git rebase` sobre trabajo compartido
- Cualquier comando que sobrescriba cambios sin commit

Alternativas seguras: `git stash` para apartar cambios, `git revert` para deshacer un commit ya publicado,
`git branch -d` (minúscula) que se niega a borrar ramas sin integrar.

## Conventional Commits

Formato: `<tipo>: <descripción en imperativo, minúsculas, sin punto final>`

| Tipo       | Uso                                                                 |
| ---------- | ------------------------------------------------------------------- |
| `feat`     | Nueva funcionalidad                                                 |
| `fix`      | Corrección de un bug                                                |
| `refactor` | Cambio interno sin cambio de comportamiento                         |
| `test`     | Tests nuevos o modificados                                          |
| `docs`     | Documentación                                                       |
| `chore`    | Tooling, dependencias, configuración                                |
| `ci`       | Reservado para pipelines futuros (todavía no existen)               |

Ejemplos:

```
feat: add card creation endpoint
feat: integrate Azure Blob Storage
fix: prevent invalid image upload
test: add spaced repetition tests
refactor: separate review scheduling service
docs: add development workflow
```

Se puede agregar un scope opcional: `feat(api): ...`, `feat(mobile): ...`.

## Evitar conflictos

- Evita modificar archivos globales (`Program.cs`, `App.tsx`, `ImageCards.slnx`, `package.json`) si
  tu feature no lo necesita. Si tienes que hacerlo, avisa al equipo.
- No hagas refactors masivos dentro de una feature pequeña; crea una rama `refactor/*`.
- No reformatees archivos que no forman parte de tu cambio.
- Integra `develop` en tu rama con frecuencia.

## Estado inicial del repositorio

El repositorio se inicializó así, sin hacer ningún commit sobre `main`:

```bash
git init -b develop
git commit --allow-empty -m "chore: initialize repository"   # commit raíz vacío en develop
git branch main                                              # main apunta al commit raíz vacío
git checkout -b feature/project-bootstrap
```
