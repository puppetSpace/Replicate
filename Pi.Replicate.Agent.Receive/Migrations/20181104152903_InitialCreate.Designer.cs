﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pi.Replicate.Data;

namespace Pi.Replicate.Agent.Api.Migrations
{
    [DbContext(typeof(ReplicateDbContext))]
    [Migration("20181104152903_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Pi.Replicate.Schema.FailedUploadFileChunk", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("FileChunkId");

                    b.Property<Guid?>("HostId");

                    b.HasKey("Id");

                    b.HasIndex("FileChunkId");

                    b.HasIndex("HostId");

                    b.ToTable("FailedUploadFileChunks");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AmountOfChunks");

                    b.Property<Guid?>("FolderId");

                    b.Property<string>("Hash");

                    b.Property<string>("HostSource");

                    b.Property<DateTime>("LastModifiedDate");

                    b.Property<string>("Name");

                    b.Property<string>("Path");

                    b.Property<long>("Size");

                    b.Property<int>("Status");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.FileChunk", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("FileId");

                    b.Property<int>("SequenceNo");

                    b.Property<int>("Status");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.ToTable("FileChunks");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.Folder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("DeleteFilesAfterSend");

                    b.Property<int>("FolderType");

                    b.Property<string>("Name");

                    b.Property<string>("Path");

                    b.HasKey("Id");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.Host", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Hosts");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.FailedUploadFileChunk", b =>
                {
                    b.HasOne("Pi.Replicate.Schema.FileChunk", "FileChunk")
                        .WithMany()
                        .HasForeignKey("FileChunkId");

                    b.HasOne("Pi.Replicate.Schema.Host", "Host")
                        .WithMany()
                        .HasForeignKey("HostId");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.File", b =>
                {
                    b.HasOne("Pi.Replicate.Schema.Folder", "Folder")
                        .WithMany("Files")
                        .HasForeignKey("FolderId");
                });

            modelBuilder.Entity("Pi.Replicate.Schema.FileChunk", b =>
                {
                    b.HasOne("Pi.Replicate.Schema.File", "File")
                        .WithMany()
                        .HasForeignKey("FileId");
                });
#pragma warning restore 612, 618
        }
    }
}
