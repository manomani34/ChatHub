using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserConversationReadState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserConversationReadStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    LastReadMessageId = table.Column<int>(type: "int", nullable: true),
                    LastReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConversationReadStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConversationReadStates_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserConversationReadStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserConversationReadStates_ConversationId",
                table: "UserConversationReadStates",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConversationReadStates_UserId_ConversationId",
                table: "UserConversationReadStates",
                columns: new[] { "UserId", "ConversationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserConversationReadStates");
        }
    }
}
