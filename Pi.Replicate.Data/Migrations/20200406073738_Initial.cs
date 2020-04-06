using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pi.Replicate.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FolderOptions_DeleteAfterSent = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FolderId = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Size = table.Column<long>(nullable: false),
                    AmountOfChunks = table.Column<long>(nullable: false),
                    Hash = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    LastModifiedDate = table.Column<DateTime>(nullable: false),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FolderRecipient",
                columns: table => new
                {
                    FolderId = table.Column<Guid>(nullable: false),
                    RecipientId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderRecipient", x => new { x.RecipientId, x.FolderId });
                    table.ForeignKey(
                        name: "FK_FolderRecipient_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderRecipient_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: true),
                    SequenceNo = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileChunks_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChunkPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileChunkId = table.Column<Guid>(nullable: true),
                    RecipientId = table.Column<Guid>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChunkPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChunkPackages_FileChunks_FileChunkId",
                        column: x => x.FileChunkId,
                        principalTable: "FileChunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChunkPackages_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChunkPackages_FileChunkId",
                table: "ChunkPackages",
                column: "FileChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_ChunkPackages_RecipientId",
                table: "ChunkPackages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_FileChunks_FileId",
                table: "FileChunks",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                table: "Files",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderRecipient_FolderId",
                table: "FolderRecipient",
                column: "FolderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChunkPackages");

            migrationBuilder.DropTable(
                name: "FolderRecipient");

            migrationBuilder.DropTable(
                name: "FileChunks");

            migrationBuilder.DropTable(
                name: "Recipients");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Folders");
        }
    }
}
