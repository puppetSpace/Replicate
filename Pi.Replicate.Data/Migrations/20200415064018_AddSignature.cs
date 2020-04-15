using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pi.Replicate.Data.Migrations
{
    public partial class AddSignature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Signature",
                table: "Files",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Signature",
                table: "Files");
        }
    }
}
