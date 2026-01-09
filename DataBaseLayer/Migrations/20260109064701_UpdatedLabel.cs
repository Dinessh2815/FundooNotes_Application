using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Add UserId column FIRST
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Labels",
                nullable: false,
                defaultValue: 0);

            // 2️⃣ Drop old unique index on Name
            migrationBuilder.DropIndex(
                name: "IX_Labels_Name",
                table: "Labels");

            // 3️⃣ Create composite unique index (UserId + Name)
            migrationBuilder.CreateIndex(
                name: "IX_Labels_UserId_Name",
                table: "Labels",
                columns: new[] { "UserId", "Name" },
                unique: true);

            // 4️⃣ Add FK constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Labels_Users_UserId",
                table: "Labels",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Labels_Users_UserId",
                table: "Labels");

            migrationBuilder.DropIndex(
                name: "IX_Labels_UserId_Name",
                table: "Labels");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Labels",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Labels_Name",
                table: "Labels",
                column: "Name",
                unique: true);
        }

    }
}
