#!/usr/bin/env bash
# QuoteFlow AI — instalación de los respaldos automáticos (T-092, RNF-DIS-03).
#
# Uso (en el servidor Ubuntu, desde la carpeta copiada):  sudo bash install.sh
#
# 1. Crea /var/backups/quoteflow con dueño postgres.
# 2. Copia backup.sh a /usr/local/bin/quoteflow-backup.sh.
# 3. Programa el respaldo diario a las 2:00 como postgres (/etc/cron.d/quoteflow-backup).
# 4. Ejecuta un respaldo de prueba.
# 5. Restaura ese respaldo en una base temporal, la verifica y la elimina.
#
# Es idempotente: puede ejecutarse de nuevo para actualizar el script o el cron.

set -Eeuo pipefail

DB_NAME="${QUOTEFLOW_DB:-quoteflow}"
BACKUP_DIR="/var/backups/quoteflow"
INSTALLED_SCRIPT="/usr/local/bin/quoteflow-backup.sh"
CRON_FILE="/etc/cron.d/quoteflow-backup"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_DB="${DB_NAME}_restore_test_$(date +%Y%m%d%H%M%S)"

step() { printf '\n==> %s\n' "$1"; }
as_postgres() { sudo -u postgres -H "$@"; }
psql_value() { as_postgres psql -X -v ON_ERROR_STOP=1 -tA -d "$1" -c "$2"; }

if [[ "$(id -u)" -ne 0 ]]; then
    echo "Ejecute este script con sudo: sudo bash $0" >&2
    exit 1
fi

for command in pg_dump pg_restore psql createdb dropdb; do
    command -v "$command" >/dev/null || { echo "No se encontró ${command}; instale el cliente de PostgreSQL 18." >&2; exit 1; }
done

id postgres >/dev/null 2>&1 || { echo "No existe el usuario del sistema postgres." >&2; exit 1; }

if [[ "$(psql_value postgres "SELECT count(*) FROM pg_database WHERE datname = '${DB_NAME}'")" != "1" ]]; then
    echo "No existe la base ${DB_NAME} en este servidor." >&2
    exit 1
fi

step "Carpeta de respaldos ${BACKUP_DIR}"
install -d -o postgres -g postgres -m 750 "$BACKUP_DIR"
touch "${BACKUP_DIR}/backup.log"
chown postgres:postgres "${BACKUP_DIR}/backup.log"
chmod 640 "${BACKUP_DIR}/backup.log"

step "Script ${INSTALLED_SCRIPT}"
# Se eliminan posibles finales de línea CRLF si los archivos se copiaron desde Windows.
sed 's/\r$//' "${SCRIPT_DIR}/backup.sh" >"${INSTALLED_SCRIPT}.tmp"
install -o root -g root -m 755 "${INSTALLED_SCRIPT}.tmp" "$INSTALLED_SCRIPT"
rm -f "${INSTALLED_SCRIPT}.tmp"

step "Cron diario a las 2:00 (${CRON_FILE})"
cat >"$CRON_FILE" <<EOF
# QuoteFlow AI — respaldo diario de la base ${DB_NAME} (T-092). Instalado por scripts/backup/install.sh.
SHELL=/bin/bash
PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
0 2 * * * postgres QUOTEFLOW_DB=${DB_NAME} ${INSTALLED_SCRIPT}
EOF
chmod 644 "$CRON_FILE"

step "Respaldo de prueba"
as_postgres env QUOTEFLOW_DB="$DB_NAME" "$INSTALLED_SCRIPT"
DUMP_FILE="${BACKUP_DIR}/${DB_NAME}_$(date +%F).dump"
[[ -s "$DUMP_FILE" ]] || { echo "No se generó ${DUMP_FILE}." >&2; exit 1; }
echo "Generado: ${DUMP_FILE} ($(du -h "$DUMP_FILE" | cut -f1))"
tail -n 1 "${BACKUP_DIR}/backup.log"

step "Restauración de prueba en la base temporal ${TEST_DB}"
cleanup() { as_postgres dropdb --if-exists "$TEST_DB" >/dev/null 2>&1 || true; }
trap cleanup EXIT

as_postgres createdb "$TEST_DB"
as_postgres pg_restore --exit-on-error --dbname="$TEST_DB" "$DUMP_FILE"

TABLE_COUNT_SQL="SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'"
MIGRATION_COUNT_SQL='SELECT count(*) FROM "__EFMigrationsHistory"'

SOURCE_TABLES="$(psql_value "$DB_NAME" "$TABLE_COUNT_SQL")"
RESTORED_TABLES="$(psql_value "$TEST_DB" "$TABLE_COUNT_SQL")"
SOURCE_MIGRATIONS="$(psql_value "$DB_NAME" "$MIGRATION_COUNT_SQL")"
RESTORED_MIGRATIONS="$(psql_value "$TEST_DB" "$MIGRATION_COUNT_SQL")"

echo "Tablas:      origen ${SOURCE_TABLES}, restaurada ${RESTORED_TABLES}"
echo "Migraciones: origen ${SOURCE_MIGRATIONS}, restaurada ${RESTORED_MIGRATIONS}"

if [[ "$RESTORED_TABLES" -eq 0 || "$SOURCE_TABLES" != "$RESTORED_TABLES" || "$SOURCE_MIGRATIONS" != "$RESTORED_MIGRATIONS" ]]; then
    echo "La restauración de prueba no coincide con la base original." >&2
    exit 1
fi

cleanup
trap - EXIT
echo "Base temporal ${TEST_DB} eliminada."

step "Listo"
echo "Respaldo diario a las 2:00 en ${BACKUP_DIR}, retención de 7 días; restauración verificada."
