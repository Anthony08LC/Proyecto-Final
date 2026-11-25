#!/usr/bin/env bash
# QuoteFlow AI — respaldo diario de PostgreSQL (T-092, RNF-DIS-03).
#
# Genera /var/backups/quoteflow/quoteflow_AAAA-MM-DD.dump (formato personalizado de pg_dump),
# conserva los respaldos de los últimos 7 días y registra el resultado en backup.log.
#
# Se ejecuta como el usuario del sistema «postgres»: se conecta por el socket local con
# autenticación peer, por lo que no necesita ni guarda contraseñas.
#
# Variables opcionales: QUOTEFLOW_DB (por defecto quoteflow), QUOTEFLOW_BACKUP_DIR,
# QUOTEFLOW_RETENTION_DAYS (por defecto 7).

set -Eeuo pipefail

DB_NAME="${QUOTEFLOW_DB:-quoteflow}"
BACKUP_DIR="${QUOTEFLOW_BACKUP_DIR:-/var/backups/quoteflow}"
RETENTION_DAYS="${QUOTEFLOW_RETENTION_DAYS:-7}"
LOG_FILE="${BACKUP_DIR}/backup.log"

DATE="$(date +%F)"
TARGET="${BACKUP_DIR}/${DB_NAME}_${DATE}.dump"
PARTIAL="${TARGET}.partial"

log() {
    printf '%s [%s] %s\n' "$(date '+%F %T')" "$1" "$2" >>"$LOG_FILE"
}

fail() {
    log "ERROR" "$1"
    rm -f "$PARTIAL"
    echo "Respaldo fallido: $1 (ver ${LOG_FILE})" >&2
    exit 1
}

trap 'fail "error inesperado en la línea ${LINENO}"' ERR

if [[ "$(id -un)" != "postgres" ]]; then
    echo "Este script debe ejecutarse como el usuario postgres (sudo -u postgres $0)." >&2
    exit 1
fi

if [[ ! -d "$BACKUP_DIR" || ! -w "$BACKUP_DIR" ]]; then
    echo "La carpeta ${BACKUP_DIR} no existe o no tiene permiso de escritura." >&2
    exit 1
fi

umask 077

# Se escribe en un archivo temporal y se renombra al terminar: un fallo nunca deja un
# respaldo incompleto con el nombre definitivo.
if ! pg_dump --format=custom --dbname="$DB_NAME" --file="$PARTIAL" 2>>"$LOG_FILE"; then
    fail "pg_dump de la base ${DB_NAME} falló"
fi

# El archivo debe poder leerse como respaldo válido antes de darlo por bueno.
if ! pg_restore --list "$PARTIAL" >/dev/null 2>>"$LOG_FILE"; then
    fail "el respaldo generado de ${DB_NAME} no es legible por pg_restore"
fi

mv -f "$PARTIAL" "$TARGET"
SIZE="$(du -h "$TARGET" | cut -f1)"

# Retención: solo después de un respaldo exitoso, se eliminan los de 7 días o más.
DELETED="$(find "$BACKUP_DIR" -maxdepth 1 -type f -name "${DB_NAME}_*.dump" \
    -mtime +"$((RETENTION_DAYS - 1))" -print -delete | wc -l)"

log "OK" "respaldo ${TARGET} (${SIZE}); respaldos antiguos eliminados: ${DELETED}"
