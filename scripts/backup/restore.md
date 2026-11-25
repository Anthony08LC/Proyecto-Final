# Restaurar un respaldo de QuoteFlow AI

Procedimiento de restauración de la base `quoteflow` (T-092, RNF-DIS-03) en el servidor Ubuntu con PostgreSQL 18. Todos los comandos se ejecutan en el servidor con un usuario que tenga `sudo`.

## 1. Elegir el respaldo

Los respaldos están en `/var/backups/quoteflow/`, uno por día (`quoteflow_AAAA-MM-DD.dump`), y se conservan los últimos 7. El registro de cada ejecución está en `backup.log`.

```bash
sudo ls -lh /var/backups/quoteflow/
sudo tail -n 20 /var/backups/quoteflow/backup.log
```

Compruebe que el archivo elegido es legible:

```bash
DUMP=/var/backups/quoteflow/quoteflow_2026-11-25.dump   # ajuste la fecha
sudo -u postgres pg_restore --list "$DUMP" | head
```

## 2. Restaurar en una base nueva

La base actual no se toca hasta verificar la restaurada. Las tablas conservan sus dueños originales, por lo que los roles de la aplicación deben existir en el servidor (`pg_dump` no respalda roles).

```bash
OWNER=$(sudo -u postgres psql -XtAc "SELECT pg_get_userbyid(datdba) FROM pg_database WHERE datname = 'quoteflow'")
sudo -u postgres createdb -O "$OWNER" quoteflow_restored
sudo -u postgres pg_restore --exit-on-error --dbname=quoteflow_restored "$DUMP"
```

Verifique el contenido, por ejemplo la última migración aplicada:

```bash
sudo -u postgres psql -d quoteflow_restored -XtAc 'SELECT max("MigrationId") FROM "__EFMigrationsHistory"'
```

## 3. Sustituir la base

1. Detenga la API para cerrar sus conexiones (por ejemplo, `sudo systemctl stop quoteflow-api`; use el nombre del servicio configurado en el despliegue).
2. Intercambie las bases. La original se conserva como `quoteflow_old` hasta confirmar que todo funciona:

   ```bash
   sudo -u postgres psql -X -v ON_ERROR_STOP=1 <<'SQL'
   SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'quoteflow' AND pid <> pg_backend_pid();
   ALTER DATABASE quoteflow RENAME TO quoteflow_old;
   ALTER DATABASE quoteflow_restored RENAME TO quoteflow;
   SQL
   ```

3. Inicie la API (`sudo systemctl start quoteflow-api`) y compruebe `GET /health` y el inicio de sesión.
4. Cuando la verificación sea correcta, elimine la base anterior:

   ```bash
   sudo -u postgres dropdb quoteflow_old
   ```

## Restaurar en otro servidor

Cree antes los roles de la aplicación (los mismos nombres que en el servidor original) y luego siga los pasos 2 y 3. Si los roles no existen, puede restaurar con `--no-owner --role=<dueño>` para que todos los objetos queden a nombre de ese rol.

## Probar el procedimiento

`sudo bash install.sh` restaura el respaldo del día en una base temporal, compara tablas y migraciones con la original y la elimina. Repítalo después de cambiar el procedimiento de respaldo.
