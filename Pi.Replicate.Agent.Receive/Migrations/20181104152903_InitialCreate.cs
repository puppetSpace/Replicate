using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pi.Replicate.Agent.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FolderType = table.Column<int>(nullable: false),
                    DeleteFilesAfterSend = table.Column<bool>(nullable: false),
                    Path = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.Id);
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
                    HostSource = table.Column<string>(nullable: true),
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
                name: "FailedUploadFileChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileChunkId = table.Column<Guid>(nullable: true),
                    HostId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedUploadFileChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailedUploadFileChunks_FileChunks_FileChunkId",
                        column: x => x.FileChunkId,
                        principalTable: "FileChunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FailedUploadFileChunks_Hosts_HostId",
                        column: x => x.HostId,
                        principalTable: "Hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FailedUploadFileChunks_FileChunkId",
                table: "FailedUploadFileChunks",
                column: "FileChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_FailedUploadFileChunks_HostId",
                table: "FailedUploadFileChunks",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_FileChunks_FileId",
                table: "FileChunks",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FolderId",
                table: "Files",
                column: "FolderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedUploadFileChunks");

            migrationBuilder.DropTable(
                name: "FileChunks");

            migrationBuilder.DropTable(
                name: "Hosts");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Folders");
        }
    }
}
