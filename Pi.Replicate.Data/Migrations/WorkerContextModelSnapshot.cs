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

                    b.Property<Guid>("FileChunkId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RecipientId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("FileChunkId");

                    b.HasIndex("RecipientId");

                    b.ToTable("ChunkPackages");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FailedFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("FileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RecipientId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("RecipientId");

                    b.ToTable("FailedFiles");
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

                    b.Property<byte[]>("Hash")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime>("LastModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Signature")
                        .HasColumnType("varbinary(max)");

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

                    b.Property<int>("ChunkSource")
                        .HasColumnType("int");

                    b.Property<Guid>("FileId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("SequenceNo")
                        .HasColumnType("int");

                    b.Property<byte[]>("Value")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.ToTable("FileChunks");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Folder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FolderRecipient", b =>
                {
                    b.Property<Guid>("RecipientId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("FolderId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RecipientId", "FolderId");

                    b.HasIndex("FolderId");

                    b.ToTable("FolderRecipient");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Recipient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Recipients");
                });

            modelBuilder.Entity("Pi.Replicate.Domain.ChunkPackage", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.FileChunk", "FileChunk")
                        .WithMany()
                        .HasForeignKey("FileChunkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Pi.Replicate.Domain.Recipient", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FailedFile", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.File", "File")
                        .WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Pi.Replicate.Domain.Recipient", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Pi.Replicate.Domain.Folder", b =>
                {
                    b.OwnsOne("Pi.Replicate.Domain.FolderOption", "FolderOptions", b1 =>
                        {
                            b1.Property<Guid>("FolderId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<bool>("DeleteAfterSent")
                                .HasColumnType("bit");

                            b1.HasKey("FolderId");

                            b1.ToTable("Folders");

                            b1.WithOwner()
                                .HasForeignKey("FolderId");
                        });
                });

            modelBuilder.Entity("Pi.Replicate.Domain.FolderRecipient", b =>
                {
                    b.HasOne("Pi.Replicate.Domain.Folder", "Folder")
                        .WithMany("Recipients")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Pi.Replicate.Domain.Recipient", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
