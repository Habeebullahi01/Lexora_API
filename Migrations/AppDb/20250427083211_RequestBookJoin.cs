using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexora_api.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RequestBookJoin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LibrarianID",
                table: "Requests",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "RequestBooks",
                columns: table => new
                {
                    BooksId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestBooks", x => new { x.BooksId, x.RequestsId });
                    table.ForeignKey(
                        name: "FK_RequestBooks_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestBooks_Requests_RequestsId",
                        column: x => x.RequestsId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestBooks_RequestsId",
                table: "RequestBooks",
                column: "RequestsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestBooks");

            migrationBuilder.AlterColumn<int>(
                name: "LibrarianID",
                table: "Requests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
