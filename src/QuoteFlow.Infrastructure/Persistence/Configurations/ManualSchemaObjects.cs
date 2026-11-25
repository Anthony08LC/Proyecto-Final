namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Objetos del esquema que EF Core no puede declarar con Fluent API y que las migraciones crean
/// con SQL explícito. No forman parte del snapshot del modelo, así que las migraciones futuras
/// no los modifican ni los eliminan.
/// </summary>
internal static class ManualSchemaObjects
{
    // RN-30: nombres de categoría y de producto únicos sin distinguir mayúsculas, aunque el
    // registro esté inactivo (sección 9.5: único sobre lower(name)).
    public const string CreateCategoryNameIndex =
        "CREATE UNIQUE INDEX uq_categories_name ON categories (lower(name));";

    public const string DropCategoryNameIndex = "DROP INDEX IF EXISTS uq_categories_name;";

    public const string CreateProductNameIndex =
        "CREATE UNIQUE INDEX uq_products_name ON products (lower(name));";

    public const string DropProductNameIndex = "DROP INDEX IF EXISTS uq_products_name;";

    // Sección 9.3: (quote_id, line_number) único con verificación diferida al final de la
    // transacción, para permitir la renumeración de las líneas al quitar una (RF-COT-03).
    public const string CreateQuoteLineNumberConstraint =
        "ALTER TABLE quote_lines ADD CONSTRAINT uq_quote_lines_quote_line_number "
        + "UNIQUE (quote_id, line_number) DEFERRABLE INITIALLY DEFERRED;";

    public const string DropQuoteLineNumberConstraint =
        "ALTER TABLE quote_lines DROP CONSTRAINT IF EXISTS uq_quote_lines_quote_line_number;";
}
