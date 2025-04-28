using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lexora_api.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class ReaderAndLibrarianFKAsStringsInBorrowRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestBooks_Requests_RequestsId",
                table: "RequestBooks");

            migrationBuilder.RenameColumn(
                name: "RequestsId",
                table: "RequestBooks",
                newName: "BorrowRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestBooks_RequestsId",
                table: "RequestBooks",
                newName: "IX_RequestBooks_BorrowRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "ReaderId",
                table: "Requests",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "LibrarianID",
                table: "Requests",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestBooks_Requests_BorrowRequestId",
                table: "RequestBooks",
                column: "BorrowRequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestBooks_Requests_BorrowRequestId",
                table: "RequestBooks");

            migrationBuilder.RenameColumn(
                name: "BorrowRequestId",
                table: "RequestBooks",
                newName: "RequestsId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestBooks_BorrowRequestId",
                table: "RequestBooks",
                newName: "IX_RequestBooks_RequestsId");

            migrationBuilder.AlterColumn<int>(
                name: "ReaderId",
                table: "Requests",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "LibrarianID",
                table: "Requests",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestBooks_Requests_RequestsId",
                table: "RequestBooks",
                column: "RequestsId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
