using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexora_api.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class BookIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestBooks_Books_BooksId",
                table: "RequestBooks");

            migrationBuilder.RenameColumn(
                name: "BooksId",
                table: "RequestBooks",
                newName: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestBooks_Books_BookId",
                table: "RequestBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestBooks_Books_BookId",
                table: "RequestBooks");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "RequestBooks",
                newName: "BooksId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestBooks_Books_BooksId",
                table: "RequestBooks",
                column: "BooksId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
