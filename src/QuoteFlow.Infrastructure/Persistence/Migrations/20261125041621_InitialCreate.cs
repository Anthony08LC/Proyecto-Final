using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuoteFlow.Infrastructure.Persistence.Configurations;

#nullable disable

namespace QuoteFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quote_number_sequences",
                columns: table => new
                {
                    year = table.Column<short>(type: "smallint", nullable: false),
                    last_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quote_number_sequences", x => x.year);
                    table.CheckConstraint("ck_quote_number_sequences_last_number", "last_number BETWEEN 0 AND 999999");
                    table.CheckConstraint("ck_quote_number_sequences_year", "year BETWEEN 2000 AND 9999");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false),
                    security_stamp = table.Column<Guid>(type: "uuid", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_email_lowercase", "email = lower(email)");
                    table.CheckConstraint("ck_users_full_name_not_empty", "length(trim(full_name)) > 0");
                    table.CheckConstraint("ck_users_password_hash", "password_hash IS NOT NULL OR is_system");
                    table.CheckConstraint("ck_users_role", "role IN ('Administrator', 'Sales')");
                    table.ForeignKey(
                        name: "fk_users_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_users_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_events", x => x.id);
                    table.CheckConstraint("ck_audit_events_entity_type", "entity_type IN ('User', 'Customer', 'Category', 'Product', 'Quote', 'SystemSettings', 'Session')");
                    table.ForeignKey(
                        name: "fk_audit_events_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.CheckConstraint("ck_categories_name_not_empty", "length(trim(name)) > 0");
                    table.ForeignKey(
                        name: "fk_categories_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_categories_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    company = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    phone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    is_vip = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.CheckConstraint("ck_customers_email_lowercase", "email IS NULL OR email = lower(email)");
                    table.CheckConstraint("ck_customers_name_not_empty", "length(trim(name)) > 0");
                    table.CheckConstraint("ck_customers_phone_format", "phone IS NULL OR phone ~ '^\\+506[0-9]{8}$'");
                    table.ForeignKey(
                        name: "fk_customers_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_customers_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    company_legal_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    company_phone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    company_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    default_commercial_terms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    vat_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    tier1_threshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tier1_discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    tier2_threshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tier2_discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    vip_discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    validity_days = table.Column<short>(type: "smallint", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    singleton = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.id);
                    table.CheckConstraint("ck_system_settings_company_name_not_empty", "length(trim(company_name)) > 0");
                    table.CheckConstraint("ck_system_settings_max_discount", "tier2_discount_rate + vip_discount_rate <= 100");
                    table.CheckConstraint("ck_system_settings_singleton", "singleton");
                    table.CheckConstraint("ck_system_settings_tier1_discount_rate", "tier1_discount_rate BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_system_settings_tier1_threshold", "tier1_threshold > 0");
                    table.CheckConstraint("ck_system_settings_tier2_discount_rate", "tier2_discount_rate BETWEEN 0 AND 100 AND tier2_discount_rate >= tier1_discount_rate");
                    table.CheckConstraint("ck_system_settings_tier2_threshold", "tier2_threshold > tier1_threshold");
                    table.CheckConstraint("ck_system_settings_validity_days", "validity_days BETWEEN 1 AND 365");
                    table.CheckConstraint("ck_system_settings_vat_rate", "vat_rate BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_system_settings_vip_discount_rate", "vip_discount_rate BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "fk_system_settings_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_system_settings_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("ck_products_name_not_empty", "length(trim(name)) > 0");
                    table.CheckConstraint("ck_products_price_not_negative", "price >= 0");
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    introduction = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    executive_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    commercial_terms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    customer_company = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    customer_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    customer_phone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    customer_is_vip = table.Column<bool>(type: "boolean", nullable: true),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    close_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxable_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tier_discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vat_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    vip_discount_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                    table.CheckConstraint("ck_quotes_amounts_not_negative", "subtotal >= 0 AND discount_amount >= 0 AND taxable_base >= 0 AND vat_amount >= 0 AND total >= 0");
                    table.CheckConstraint("ck_quotes_cancelled_number", "status <> 'Cancelled' OR ((number IS NULL) = (issued_at IS NULL))");
                    table.CheckConstraint("ck_quotes_cancelled_reason", "status <> 'Cancelled' OR issued_at IS NULL OR close_reason IS NOT NULL");
                    table.CheckConstraint("ck_quotes_closed_at", "(status IN ('Draft', 'Issued')) = (closed_at IS NULL)");
                    table.CheckConstraint("ck_quotes_discount_not_above_subtotal", "discount_amount <= subtotal");
                    table.CheckConstraint("ck_quotes_discount_rate", "discount_rate = tier_discount_rate + vip_discount_rate AND discount_rate <= 100");
                    table.CheckConstraint("ck_quotes_draft", "status <> 'Draft' OR (number IS NULL AND issued_at IS NULL AND valid_until IS NULL AND closed_at IS NULL)");
                    table.CheckConstraint("ck_quotes_issued_data", "status NOT IN ('Issued', 'Accepted', 'Rejected', 'Expired') OR (number IS NOT NULL AND issued_at IS NOT NULL AND valid_until IS NOT NULL AND customer_name IS NOT NULL AND customer_is_vip IS NOT NULL)");
                    table.CheckConstraint("ck_quotes_number_format", "number IS NULL OR number ~ '^COT-[0-9]{4}-[0-9]{6}$'");
                    table.CheckConstraint("ck_quotes_status", "status IN ('Draft', 'Issued', 'Accepted', 'Rejected', 'Expired', 'Cancelled')");
                    table.CheckConstraint("ck_quotes_taxable_base", "taxable_base = subtotal - discount_amount");
                    table.CheckConstraint("ck_quotes_total", "total = taxable_base + vat_amount");
                    table.ForeignKey(
                        name: "fk_quotes_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quotes_quotes_source_quote_id",
                        column: x => x.source_quote_id,
                        principalTable: "quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quotes_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quotes_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quotes_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quote_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<short>(type: "smallint", nullable: false),
                    product_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    product_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, computedColumnSql: "round(unit_price * quantity, 2) - round(round(unit_price * quantity, 2) * discount_percentage / 100, 2)", stored: true),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quote_lines", x => x.id);
                    table.CheckConstraint("ck_quote_lines_line_number_positive", "line_number > 0");
                    table.CheckConstraint("ck_quote_lines_quantity_positive", "quantity > 0");
                    table.CheckConstraint("ck_quote_lines_unit_price_not_negative", "unit_price >= 0");
                    table.ForeignKey(
                        name: "fk_quote_lines_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quote_lines_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_entity_type_entity_id_occurred_at",
                table: "audit_events",
                columns: new[] { "entity_type", "entity_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_occurred_at",
                table: "audit_events",
                column: "occurred_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_user_id_occurred_at",
                table: "audit_events",
                columns: new[] { "user_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_categories_created_by",
                table: "categories",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_categories_updated_by",
                table: "categories",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_customers_created_by",
                table: "customers",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_customers_updated_by",
                table: "customers",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_created_by",
                table: "products",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_products_updated_by",
                table: "products",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_quote_lines_product_id",
                table: "quote_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "uq_quote_lines_quote_product",
                table: "quote_lines",
                columns: new[] { "quote_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_created_by",
                table: "quotes",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_customer_id",
                table: "quotes",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_owner_id",
                table: "quotes",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_source_quote_id",
                table: "quotes",
                column: "source_quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_updated_by",
                table: "quotes",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_valid_until_issued",
                table: "quotes",
                column: "valid_until",
                filter: "status = 'Issued'");

            migrationBuilder.CreateIndex(
                name: "uq_quotes_number",
                table: "quotes",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_created_by",
                table: "system_settings",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_updated_by",
                table: "system_settings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_system_settings_singleton",
                table: "system_settings",
                column: "singleton",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_created_by",
                table: "users",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_users_updated_by",
                table: "users",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_single_system_user",
                table: "users",
                column: "is_system",
                unique: true,
                filter: "is_system");

            // Objetos que EF Core no puede declarar con Fluent API (ver ManualSchemaObjects).
            migrationBuilder.Sql(ManualSchemaObjects.CreateCategoryNameIndex);
            migrationBuilder.Sql(ManualSchemaObjects.CreateProductNameIndex);
            migrationBuilder.Sql(ManualSchemaObjects.CreateQuoteLineNumberConstraint);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ManualSchemaObjects.DropQuoteLineNumberConstraint);
            migrationBuilder.Sql(ManualSchemaObjects.DropProductNameIndex);
            migrationBuilder.Sql(ManualSchemaObjects.DropCategoryNameIndex);

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "quote_lines");

            migrationBuilder.DropTable(
                name: "quote_number_sequences");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
