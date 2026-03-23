using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spix.AppBacken.Migrations
{
    /// <inheritdoc />
    public partial class InitialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChainTypes",
                columns: table => new
                {
                    ChainTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChainName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChainTypes", x => x.ChainTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Latin1_General_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "FrecuencyTypes",
                columns: table => new
                {
                    FrecuencyTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrecuencyTypes", x => x.FrecuencyTypeId);
                });

            migrationBuilder.CreateTable(
                name: "HotSpotTypes",
                columns: table => new
                {
                    HotSpotTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotSpotTypes", x => x.HotSpotTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    OperationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.OperationId);
                });

            migrationBuilder.CreateTable(
                name: "Securities",
                columns: table => new
                {
                    SecurityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SecurityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Securities", x => x.SecurityId);
                });

            migrationBuilder.CreateTable(
                name: "SoftPlans",
                columns: table => new
                {
                    SoftPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AS"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Meses = table.Column<int>(type: "int", nullable: false),
                    ClientsCount = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftPlans", x => x.SoftPlanId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    StateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Latin1_General_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.StateId);
                    table.ForeignKey(
                        name: "FK_States_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Frecuencies",
                columns: table => new
                {
                    FrecuencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrecuencyTypeId = table.Column<int>(type: "int", nullable: false),
                    FrecuencyName = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frecuencies", x => x.FrecuencyId);
                    table.ForeignKey(
                        name: "FK_Frecuencies_FrecuencyTypes_FrecuencyTypeId",
                        column: x => x.FrecuencyTypeId,
                        principalTable: "FrecuencyTypes",
                        principalColumn: "FrecuencyTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Corporations",
                columns: table => new
                {
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Latin1_General_CI_AS"),
                    NroDocument = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FaxNumber2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    SoftPlanId = table.Column<int>(type: "int", nullable: false),
                    DateStart = table.Column<DateTime>(type: "date", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "date", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corporations", x => x.CorporationId);
                    table.ForeignKey(
                        name: "FK_Corporations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Corporations_SoftPlans_SoftPlanId",
                        column: x => x.SoftPlanId,
                        principalTable: "SoftPlans",
                        principalColumn: "SoftPlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Latin1_General_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityId);
                    table.ForeignKey(
                        name: "FK_Cities_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobPosition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DocumentName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeId);
                    table.ForeignKey(
                        name: "FK_DocumentTypes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IpNets",
                columns: table => new
                {
                    IpNetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Ip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false),
                    Excluded = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpNets", x => x.IpNetId);
                    table.ForeignKey(
                        name: "FK_IpNets_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IpNetworks",
                columns: table => new
                {
                    IpNetworkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Ip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Assigned = table.Column<bool>(type: "bit", nullable: false),
                    Excluded = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpNetworks", x => x.IpNetworkId);
                    table.ForeignKey(
                        name: "FK_IpNetworks_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    ManagerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AS"),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AS"),
                    NroDocument = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false),
                    Job = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.ManagerId);
                    table.ForeignKey(
                        name: "FK_Managers_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Marks",
                columns: table => new
                {
                    MarkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    MarkName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marks", x => x.MarkId);
                    table.ForeignKey(
                        name: "FK_Marks_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanCategories",
                columns: table => new
                {
                    PlanCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PlanCategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanCategories", x => x.PlanCategoryId);
                    table.ForeignKey(
                        name: "FK_PlanCategories_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.ProductCategoryId);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registers",
                columns: table => new
                {
                    RegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Contratos = table.Column<int>(type: "int", nullable: false),
                    Solicitudes = table.Column<int>(type: "int", nullable: false),
                    RegPurchase = table.Column<int>(type: "int", nullable: false),
                    RegSells = table.Column<int>(type: "int", nullable: false),
                    RegTransfer = table.Column<int>(type: "int", nullable: false),
                    Cargue = table.Column<int>(type: "int", nullable: false),
                    Egresos = table.Column<int>(type: "int", nullable: false),
                    Adelantado = table.Column<int>(type: "int", nullable: false),
                    Exonerado = table.Column<int>(type: "int", nullable: false),
                    NotaCobro = table.Column<int>(type: "int", nullable: false),
                    Factura = table.Column<int>(type: "int", nullable: false),
                    PagoContratista = table.Column<int>(type: "int", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registers", x => x.RegisterId);
                    table.ForeignKey(
                        name: "FK_Registers_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.ServiceCategoryId);
                    table.ForeignKey(
                        name: "FK_ServiceCategories_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TaxName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.TaxId);
                    table.ForeignKey(
                        name: "FK_Taxes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AS"),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AS"),
                    Nro_Document = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Job = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductStorages",
                columns: table => new
                {
                    ProductStorageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    StorageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStorages", x => x.ProductStorageId);
                    table.ForeignKey(
                        name: "FK_ProductStorages_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductStorages_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductStorages_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    ZoneName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.ZoneId);
                    table.ForeignKey(
                        name: "FK_Zones_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Zones_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zones_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateTransfer = table.Column<DateTime>(type: "date", nullable: false),
                    NroTransfer = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FromStorageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FromProductStorageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStorageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToProductStorageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_Transfers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleDetails",
                columns: table => new
                {
                    UserRoleDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserType = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleDetails", x => x.UserRoleDetailsId);
                    table.ForeignKey(
                        name: "FK_UserRoleDetails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: true),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeCountry = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CodeNumber = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                    table.ForeignKey(
                        name: "FK_Clients_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clients_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contractors",
                columns: table => new
                {
                    ContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    GuardarPago = table.Column<bool>(type: "bit", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contractors", x => x.ContractorId);
                    table.ForeignKey(
                        name: "FK_Contractors_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contractors_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                    table.ForeignKey(
                        name: "FK_Suppliers_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Suppliers_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateCreated = table.Column<DateTime>(type: "date", nullable: true),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Document = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    Imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technicians", x => x.TechnicianId);
                    table.ForeignKey(
                        name: "FK_Technicians_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Technicians_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarkModels",
                columns: table => new
                {
                    MarkModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    MarkModelName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MarkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkModels", x => x.MarkModelId);
                    table.ForeignKey(
                        name: "FK_MarkModels_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarkModels_Marks_MarkId",
                        column: x => x.MarkId,
                        principalTable: "Marks",
                        principalColumn: "MarkId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PlanCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SpeedUp = table.Column<int>(type: "int", nullable: false),
                    SpeedUpType = table.Column<int>(type: "int", nullable: false),
                    SpeedDown = table.Column<int>(type: "int", nullable: false),
                    SpeedDownType = table.Column<int>(type: "int", nullable: false),
                    TasaReuso = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.PlanId);
                    table.ForeignKey(
                        name: "FK_Plans_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_PlanCategories_PlanCategoryId",
                        column: x => x.PlanCategoryId,
                        principalTable: "PlanCategories",
                        principalColumn: "PlanCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plans_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Costo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WithSerials = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceClients",
                columns: table => new
                {
                    ServiceClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Costo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceClients", x => x.ServiceClientId);
                    table.ForeignKey(
                        name: "FK_ServiceClients_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceClients_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceClients_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => x.UsuarioRoleId);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: false),
                    NroPurchase = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductStorageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacuraDate = table.Column<DateTime>(type: "date", nullable: false),
                    NroFactura = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_Purchases_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchases_ProductStorages_ProductStorageId",
                        column: x => x.ProductStorageId,
                        principalTable: "ProductStorages",
                        principalColumn: "ProductStorageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchases_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    NodesName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpNetworkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    Mac = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    MarkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarkModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(12,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(12,7)", nullable: true),
                    FrecuencyTypeId = table.Column<int>(type: "int", nullable: true),
                    FrecuencyId = table.Column<int>(type: "int", nullable: true),
                    ChannelId = table.Column<int>(type: "int", nullable: true),
                    SecurityId = table.Column<int>(type: "int", nullable: true),
                    FraseSeguridad = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.NodeId);
                    table.ForeignKey(
                        name: "FK_Nodes_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Nodes_Frecuencies_FrecuencyId",
                        column: x => x.FrecuencyId,
                        principalTable: "Frecuencies",
                        principalColumn: "FrecuencyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_FrecuencyTypes_FrecuencyTypeId",
                        column: x => x.FrecuencyTypeId,
                        principalTable: "FrecuencyTypes",
                        principalColumn: "FrecuencyTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_IpNetworks_IpNetworkId",
                        column: x => x.IpNetworkId,
                        principalTable: "IpNetworks",
                        principalColumn: "IpNetworkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_MarkModels_MarkModelId",
                        column: x => x.MarkModelId,
                        principalTable: "MarkModels",
                        principalColumn: "MarkModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Marks_MarkId",
                        column: x => x.MarkId,
                        principalTable: "Marks",
                        principalColumn: "MarkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "OperationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "SecurityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nodes_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "ZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ServerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpNetworkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    WanName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ApiPort = table.Column<int>(type: "int", nullable: false),
                    MarkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarkModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.ServerId);
                    table.ForeignKey(
                        name: "FK_Servers_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Servers_IpNetworks_IpNetworkId",
                        column: x => x.IpNetworkId,
                        principalTable: "IpNetworks",
                        principalColumn: "IpNetworkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Servers_MarkModels_MarkModelId",
                        column: x => x.MarkModelId,
                        principalTable: "MarkModels",
                        principalColumn: "MarkModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Servers_Marks_MarkId",
                        column: x => x.MarkId,
                        principalTable: "Marks",
                        principalColumn: "MarkId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Servers_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "ZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductStocks",
                columns: table => new
                {
                    ProductStockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductStorageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStocks", x => x.ProductStockId);
                    table.ForeignKey(
                        name: "FK_ProductStocks_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductStocks_ProductStorages_ProductStorageId",
                        column: x => x.ProductStorageId,
                        principalTable: "ProductStorages",
                        principalColumn: "ProductStorageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductStocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransferDetails",
                columns: table => new
                {
                    TransferDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameProduct = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferDetails", x => x.TransferDetailsId);
                    table.ForeignKey(
                        name: "FK_TransferDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferDetails_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferDetails_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "TransferId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractClients",
                columns: table => new
                {
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreado = table.Column<DateTime>(type: "date", nullable: false),
                    ControlContrato = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ContractorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeCountry = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CodeNumber = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StateType = table.Column<int>(type: "int", nullable: false),
                    EquipoEmpres = table.Column<bool>(type: "bit", nullable: false),
                    EnvoiceClient = table.Column<bool>(type: "bit", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Impuesto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractClients", x => x.ContractClientId);
                    table.ForeignKey(
                        name: "FK_ContractClients_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractClients_Contractors_ContractorId",
                        column: x => x.ContractorId,
                        principalTable: "Contractors",
                        principalColumn: "ContractorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractClients_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractClients_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractClients_ServiceClients_ServiceClientId",
                        column: x => x.ServiceClientId,
                        principalTable: "ServiceClients",
                        principalColumn: "ServiceClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractClients_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "ZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseDetails",
                columns: table => new
                {
                    PurchaseDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameProduct = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RateTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseDetails", x => x.PurchaseDetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "ProductCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseDetails_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractIps",
                columns: table => new
                {
                    ContractIpId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IpNetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractIps", x => x.ContractIpId);
                    table.ForeignKey(
                        name: "FK_ContractIps_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractIps_IpNets_IpNetId",
                        column: x => x.IpNetId,
                        principalTable: "IpNets",
                        principalColumn: "IpNetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractNodes",
                columns: table => new
                {
                    ContractNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractNodes", x => x.ContractNodeId);
                    table.ForeignKey(
                        name: "FK_ContractNodes_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractPlans",
                columns: table => new
                {
                    ContractPlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPlans", x => x.ContractPlanId);
                    table.ForeignKey(
                        name: "FK_ContractPlans_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractPlans_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractQues",
                columns: table => new
                {
                    ContractQueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IpNetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalVelocidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MikrotikId = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractQues", x => x.ContractQueId);
                    table.ForeignKey(
                        name: "FK_ContractQues_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractQues_IpNets_IpNetId",
                        column: x => x.IpNetId,
                        principalTable: "IpNets",
                        principalColumn: "IpNetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractQues_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractQues_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractServers",
                columns: table => new
                {
                    ContractServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractServers", x => x.ContractServerId);
                    table.ForeignKey(
                        name: "FK_ContractServers_ContractClients_ContractClientId",
                        column: x => x.ContractClientId,
                        principalTable: "ContractClients",
                        principalColumn: "ContractClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractServers_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cargues",
                columns: table => new
                {
                    CargueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DateCargue = table.Column<DateTime>(type: "date", nullable: false),
                    ControlCargue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 20, nullable: false),
                    PurchaseDetailId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 20, nullable: false),
                    CantToUp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargues", x => x.CargueId);
                    table.ForeignKey(
                        name: "FK_Cargues_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cargues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargues_PurchaseDetails_PurchaseDetailId",
                        column: x => x.PurchaseDetailId,
                        principalTable: "PurchaseDetails",
                        principalColumn: "PurchaseDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargues_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CargueDetails",
                columns: table => new
                {
                    CargueDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CargueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MacWlan = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    DateCargue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CorporationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargueDetails", x => x.CargueDetailId);
                    table.ForeignKey(
                        name: "FK_CargueDetails_Cargues_CargueId",
                        column: x => x.CargueId,
                        principalTable: "Cargues",
                        principalColumn: "CargueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargueDetails_Corporations_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporations",
                        principalColumn: "CorporationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CorporationId",
                table: "AspNetUsers",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_CargueId",
                table: "CargueDetails",
                column: "CargueId");

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_CorporationId",
                table: "CargueDetails",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_CargueDetails_MacWlan_CorporationId",
                table: "CargueDetails",
                columns: new[] { "MacWlan", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargues_CorporationId",
                table: "Cargues",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargues_ProductId",
                table: "Cargues",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargues_PurchaseDetailId",
                table: "Cargues",
                column: "PurchaseDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargues_PurchaseId",
                table: "Cargues",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ChainTypes_ChainName",
                table: "ChainTypes",
                column: "ChainName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ChannelName",
                table: "Channels",
                column: "ChannelName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityId",
                table: "Cities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_StateId",
                table: "Cities",
                columns: new[] { "Name", "StateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_StateId",
                table: "Cities",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CorporationId_DocumentTypeId_Document",
                table: "Clients",
                columns: new[] { "CorporationId", "DocumentTypeId", "Document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CorporationId_FirstName_LastName",
                table: "Clients",
                columns: new[] { "CorporationId", "FirstName", "LastName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DocumentTypeId",
                table: "Clients",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UserName",
                table: "Clients",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_ClientId",
                table: "ContractClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_ContractorId",
                table: "ContractClients",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_CorporationId_ControlContrato",
                table: "ContractClients",
                columns: new[] { "CorporationId", "ControlContrato" },
                unique: true,
                filter: "[ControlContrato] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_ServiceCategoryId",
                table: "ContractClients",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_ServiceClientId",
                table: "ContractClients",
                column: "ServiceClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractClients_ZoneId",
                table: "ContractClients",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractIps_ContractClientId_IpNetId",
                table: "ContractIps",
                columns: new[] { "ContractClientId", "IpNetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractIps_IpNetId",
                table: "ContractIps",
                column: "IpNetId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractNodes_ContractClientId_NodeId",
                table: "ContractNodes",
                columns: new[] { "ContractClientId", "NodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractNodes_NodeId",
                table: "ContractNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_CorporationId_DocumentTypeId_Document",
                table: "Contractors",
                columns: new[] { "CorporationId", "DocumentTypeId", "Document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_CorporationId_FirstName_LastName",
                table: "Contractors",
                columns: new[] { "CorporationId", "FirstName", "LastName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_DocumentTypeId",
                table: "Contractors",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_UserName",
                table: "Contractors",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractPlans_ContractClientId_PlanId",
                table: "ContractPlans",
                columns: new[] { "ContractClientId", "PlanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractPlans_PlanId",
                table: "ContractPlans",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractQues_ContractClientId_IpNetId",
                table: "ContractQues",
                columns: new[] { "ContractClientId", "IpNetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractQues_IpNetId",
                table: "ContractQues",
                column: "IpNetId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractQues_PlanId",
                table: "ContractQues",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractQues_ServerId",
                table: "ContractQues",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractServers_ContractClientId_ServerId",
                table: "ContractServers",
                columns: new[] { "ContractClientId", "ServerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractServers_ServerId",
                table: "ContractServers",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Corporations_CountryId",
                table: "Corporations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Corporations_Name_NroDocument",
                table: "Corporations",
                columns: new[] { "Name", "NroDocument" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Corporations_SoftPlanId",
                table: "Corporations",
                column: "SoftPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_CorporationId",
                table: "DocumentTypes",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_DocumentName_CorporationId",
                table: "DocumentTypes",
                columns: new[] { "DocumentName", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Frecuencies_FrecuencyTypeId_FrecuencyName",
                table: "Frecuencies",
                columns: new[] { "FrecuencyTypeId", "FrecuencyName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrecuencyTypes_TypeName",
                table: "FrecuencyTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotSpotTypes_TypeName",
                table: "HotSpotTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IpNets_CorporationId",
                table: "IpNets",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_IpNets_Ip_CorporationId",
                table: "IpNets",
                columns: new[] { "Ip", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IpNetworks_CorporationId",
                table: "IpNetworks",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_IpNetworks_Ip_CorporationId",
                table: "IpNetworks",
                columns: new[] { "Ip", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_CorporationId",
                table: "Managers",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_Email",
                table: "Managers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_FirstName_LastName_NroDocument",
                table: "Managers",
                columns: new[] { "FirstName", "LastName", "NroDocument" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_UserName",
                table: "Managers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarkModels_CorporationId",
                table: "MarkModels",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkModels_MarkId",
                table: "MarkModels",
                column: "MarkId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkModels_MarkModelName_CorporationId_MarkId",
                table: "MarkModels",
                columns: new[] { "MarkModelName", "CorporationId", "MarkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marks_CorporationId",
                table: "Marks",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Marks_MarkName_CorporationId",
                table: "Marks",
                columns: new[] { "MarkName", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ChannelId",
                table: "Nodes",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CorporationId",
                table: "Nodes",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_FrecuencyId",
                table: "Nodes",
                column: "FrecuencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_FrecuencyTypeId",
                table: "Nodes",
                column: "FrecuencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_IpNetworkId",
                table: "Nodes",
                column: "IpNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_MarkId",
                table: "Nodes",
                column: "MarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_MarkModelId",
                table: "Nodes",
                column: "MarkModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_NodesName_CorporationId_OperationId",
                table: "Nodes",
                columns: new[] { "NodesName", "CorporationId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_OperationId",
                table: "Nodes",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_SecurityId",
                table: "Nodes",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ZoneId",
                table: "Nodes",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_OperationName",
                table: "Operations",
                column: "OperationName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanCategories_CorporationId",
                table: "PlanCategories",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanCategories_PlanCategoryName_CorporationId",
                table: "PlanCategories",
                columns: new[] { "PlanCategoryName", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_CorporationId_PlanName",
                table: "Plans",
                columns: new[] { "CorporationId", "PlanName" });

            migrationBuilder.CreateIndex(
                name: "IX_Plans_PlanCategoryId",
                table: "Plans",
                column: "PlanCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_PlanId",
                table: "Plans",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_TaxId",
                table: "Plans",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CorporationId_Name",
                table: "ProductCategories",
                columns: new[] { "CorporationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CorporationId_ProductName",
                table: "Products",
                columns: new[] { "CorporationId", "ProductName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxId",
                table: "Products",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_CorporationId_ProductId_ProductStorageId",
                table: "ProductStocks",
                columns: new[] { "CorporationId", "ProductId", "ProductStorageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_ProductId",
                table: "ProductStocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_ProductStorageId",
                table: "ProductStocks",
                column: "ProductStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStorages_CityId",
                table: "ProductStorages",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStorages_CorporationId_StorageName",
                table: "ProductStorages",
                columns: new[] { "CorporationId", "StorageName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStorages_StateId",
                table: "ProductStorages",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDetails_CorporationId_ProductId_PurchaseId",
                table: "PurchaseDetails",
                columns: new[] { "CorporationId", "ProductId", "PurchaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDetails_ProductCategoryId",
                table: "PurchaseDetails",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDetails_ProductId",
                table: "PurchaseDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDetails_PurchaseId",
                table: "PurchaseDetails",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CorporationId_NroPurchase",
                table: "Purchases",
                columns: new[] { "CorporationId", "NroPurchase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CorporationId_SupplierId_NroFactura",
                table: "Purchases",
                columns: new[] { "CorporationId", "SupplierId", "NroFactura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ProductStorageId",
                table: "Purchases",
                column: "ProductStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SupplierId",
                table: "Purchases",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Adelantado",
                table: "Registers",
                columns: new[] { "CorporationId", "Adelantado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Cargue",
                table: "Registers",
                columns: new[] { "CorporationId", "Cargue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Contratos",
                table: "Registers",
                columns: new[] { "CorporationId", "Contratos" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Egresos",
                table: "Registers",
                columns: new[] { "CorporationId", "Egresos" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Exonerado",
                table: "Registers",
                columns: new[] { "CorporationId", "Exonerado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Factura",
                table: "Registers",
                columns: new[] { "CorporationId", "Factura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_NotaCobro",
                table: "Registers",
                columns: new[] { "CorporationId", "NotaCobro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_PagoContratista",
                table: "Registers",
                columns: new[] { "CorporationId", "PagoContratista" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CorporationId_Solicitudes",
                table: "Registers",
                columns: new[] { "CorporationId", "Solicitudes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Securities_SecurityName",
                table: "Securities",
                column: "SecurityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_CorporationId",
                table: "Servers",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_IpNetworkId_CorporationId",
                table: "Servers",
                columns: new[] { "IpNetworkId", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_MarkId",
                table: "Servers",
                column: "MarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_MarkModelId",
                table: "Servers",
                column: "MarkModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ServerName_CorporationId",
                table: "Servers",
                columns: new[] { "ServerName", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ZoneId",
                table: "Servers",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_CorporationId_Name",
                table: "ServiceCategories",
                columns: new[] { "CorporationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceClients_CorporationId_ServiceName",
                table: "ServiceClients",
                columns: new[] { "CorporationId", "ServiceName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceClients_ServiceCategoryId",
                table: "ServiceClients",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceClients_TaxId",
                table: "ServiceClients",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftPlans_Name",
                table: "SoftPlans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_CountryId",
                table: "States",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_States_Name_CountryId",
                table: "States",
                columns: new[] { "Name", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_StateId",
                table: "States",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CityId",
                table: "Suppliers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CorporationId_Document_DocumentTypeId",
                table: "Suppliers",
                columns: new[] { "CorporationId", "Document", "DocumentTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CorporationId_Name",
                table: "Suppliers",
                columns: new[] { "CorporationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_DocumentTypeId",
                table: "Suppliers",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_StateId",
                table: "Suppliers",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_CorporationId_Rate",
                table: "Taxes",
                columns: new[] { "CorporationId", "Rate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_CorporationId_TaxName",
                table: "Taxes",
                columns: new[] { "CorporationId", "TaxName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_CorporationId_DocumentTypeId_Document",
                table: "Technicians",
                columns: new[] { "CorporationId", "DocumentTypeId", "Document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_CorporationId_FirstName_LastName",
                table: "Technicians",
                columns: new[] { "CorporationId", "FirstName", "LastName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_DocumentTypeId",
                table: "Technicians",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_UserName",
                table: "Technicians",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_CorporationId_ProductId_TransferId",
                table: "TransferDetails",
                columns: new[] { "CorporationId", "ProductId", "TransferId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_ProductCategoryId",
                table: "TransferDetails",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_ProductId",
                table: "TransferDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_TransferId",
                table: "TransferDetails",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_CorporationId_NroTransfer",
                table: "Transfers",
                columns: new[] { "CorporationId", "NroTransfer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_UserId",
                table: "Transfers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleDetails_UserId",
                table: "UserRoleDetails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleDetails_UserRoleDetailsId",
                table: "UserRoleDetails",
                column: "UserRoleDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleDetails_UserType_UserId",
                table: "UserRoleDetails",
                columns: new[] { "UserType", "UserId" },
                unique: true,
                filter: "[UserType] IS NOT NULL AND [UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_CorporationId",
                table: "UsuarioRoles",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_UsuarioId_UserType",
                table: "UsuarioRoles",
                columns: new[] { "UsuarioId", "UserType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CorporationId",
                table: "Usuarios",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_FirstName_LastName_Nro_Document_CorporationId",
                table: "Usuarios",
                columns: new[] { "FirstName", "LastName", "Nro_Document", "CorporationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UserName",
                table: "Usuarios",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zones_CityId",
                table: "Zones",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Zones_CorporationId_StateId_CityId_ZoneName",
                table: "Zones",
                columns: new[] { "CorporationId", "StateId", "CityId", "ZoneName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zones_StateId",
                table: "Zones",
                column: "StateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CargueDetails");

            migrationBuilder.DropTable(
                name: "ChainTypes");

            migrationBuilder.DropTable(
                name: "ContractIps");

            migrationBuilder.DropTable(
                name: "ContractNodes");

            migrationBuilder.DropTable(
                name: "ContractPlans");

            migrationBuilder.DropTable(
                name: "ContractQues");

            migrationBuilder.DropTable(
                name: "ContractServers");

            migrationBuilder.DropTable(
                name: "HotSpotTypes");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "ProductStocks");

            migrationBuilder.DropTable(
                name: "Registers");

            migrationBuilder.DropTable(
                name: "Technicians");

            migrationBuilder.DropTable(
                name: "TransferDetails");

            migrationBuilder.DropTable(
                name: "UserRoleDetails");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Cargues");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "IpNets");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "ContractClients");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "PurchaseDetails");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Frecuencies");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Securities");

            migrationBuilder.DropTable(
                name: "PlanCategories");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Contractors");

            migrationBuilder.DropTable(
                name: "ServiceClients");

            migrationBuilder.DropTable(
                name: "IpNetworks");

            migrationBuilder.DropTable(
                name: "MarkModels");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "FrecuencyTypes");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "Marks");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.DropTable(
                name: "ProductStorages");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Corporations");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "SoftPlans");
        }
    }
}
