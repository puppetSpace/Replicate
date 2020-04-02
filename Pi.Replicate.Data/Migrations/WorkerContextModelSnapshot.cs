﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pi.Replicate.Data.Db;

namespace Pi.Replicate.Data.Migrations
{
    [DbContext(typeof(WorkerContext))]
    partial class WorkerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Pi.Replicate.Domain.ChunkPackage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("FileChunkId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RecipientId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FileChunkId");

                    b.HasIndex("RecipientId");

                    b.ToTable("ChunkPackages");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AmountOfChunks")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("FolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Hash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LastModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FileChunk", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("FileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("SequenceNo")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.ToTable("FileChunks");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Folder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("FolderOptionsId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FolderOptionsId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FolderOption", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("DeleteAfterSent")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("FolderOption");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Recipient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("FolderOptionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("FolderOptionId");

                    b.ToTable("Recipient");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.ChunkPackage", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.FileChunk", "FileChunk")
                        .WithMany()
                        .HasForeignKey("FileChunkId");

                    b.HasOne("Pi.Replicate.Domain.Recipient", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.File", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.Folder", "Folder")
                        .WithMany()
                        .HasForeignKey("FolderId");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FileChunk", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.File", "File")
                        .WithMany()
                        .HasForeignKey("FileId");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Folder", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.FolderOption", "FolderOptions")
                        .WithMany()
                        .HasForeignKey("FolderOptionsId");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Recipient", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.FolderOption", null)
                        .WithMany("Recipient")
                        .HasForeignKey("FolderOptionId");
                });
#pragma warning restore 612, 618
        }
    }
}
