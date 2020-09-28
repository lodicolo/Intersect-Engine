using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Intersect.Server.Migrations.Prototype
{
    public partial class Prototype_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentStrings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentStrings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalizedContentString",
                columns: table => new
                {
                    ContentStringId = table.Column<Guid>(nullable: false),
                    LocaleId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: false),
                    Plural = table.Column<string>(nullable: true),
                    Zero = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizedContentString", x => new { x.ContentStringId, x.LocaleId });
                    table.ForeignKey(
                        name: "FK_LocalizedContentString_ContentStrings_ContentStringId",
                        column: x => x.ContentStringId,
                        principalTable: "ContentStrings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Simples",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    DescriptionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Simples_ContentStrings_DescriptionId",
                        column: x => x.DescriptionId,
                        principalTable: "ContentStrings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Junctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SetId = table.Column<Guid>(nullable: false),
                    SimpleId = table.Column<Guid>(nullable: false),
                    JunctionMetaProperty = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Junctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Junctions_Sets_SetId",
                        column: x => x.SetId,
                        principalTable: "Sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Junctions_Simples_SimpleId",
                        column: x => x.SimpleId,
                        principalTable: "Simples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Junctions_SimpleId",
                table: "Junctions",
                column: "SimpleId");

            migrationBuilder.CreateIndex(
                name: "IX_Junctions_SetId_SimpleId",
                table: "Junctions",
                columns: new[] { "SetId", "SimpleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Simples_DescriptionId",
                table: "Simples",
                column: "DescriptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Junctions");

            migrationBuilder.DropTable(
                name: "LocalizedContentString");

            migrationBuilder.DropTable(
                name: "Sets");

            migrationBuilder.DropTable(
                name: "Simples");

            migrationBuilder.DropTable(
                name: "ContentStrings");
        }
    }
}
