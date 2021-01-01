﻿// <auto-generated />
using System;
using Delivery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Delivery.Migrations
{
    [DbContext(typeof(deliveryContext))]
    [Migration("20210101094534_ChangeCitizenshipType")]
    partial class ChangeCitizenshipType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:Collation", "en_US.UTF-8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Delivery.Models.Address", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Apartment")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("Building")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Entrance")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("Level")
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)");

                    b.Property<long?>("LocalityId")
                        .HasColumnType("bigint");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("LocalityId");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("Delivery.Models.Cart", b =>
                {
                    b.Property<string>("UserLogin")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.HasKey("UserLogin", "ProductId")
                        .HasName("cart_pk");

                    b.HasIndex("ProductId");

                    b.ToTable("Cart");
                });

            modelBuilder.Entity("Delivery.Models.CategoryStore", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("CategoryStore");
                });

            modelBuilder.Entity("Delivery.Models.Courier", b =>
                {
                    b.Property<string>("UserLogin")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<string>("Citizenship")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateWorkBegin")
                        .HasColumnType("date");

                    b.Property<string>("PassportNumber")
                        .IsRequired()
                        .HasMaxLength(35)
                        .HasColumnType("character varying(35)");

                    b.Property<decimal>("Payroll")
                        .HasColumnType("money");

                    b.Property<int>("WorkStatusId")
                        .HasColumnType("integer");

                    b.HasKey("UserLogin")
                        .HasName("couriers_pk");

                    b.HasIndex("WorkStatusId");

                    b.HasIndex(new[] { "Citizenship", "PassportNumber" }, "couriers_citizenshipid_passportnumber_uindex")
                        .IsUnique();

                    b.ToTable("Couriers");
                });

            modelBuilder.Entity("Delivery.Models.Locality", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("Locality");

                    b
                        .HasComment("Населенные пункты");
                });

            modelBuilder.Entity("Delivery.Models.Order", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("CourierLogin")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<DateTime?>("DeliveryDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("StatusId")
                        .HasColumnType("integer");

                    b.Property<long>("StoreId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("TakingDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("UserLogin")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.HasKey("Id");

                    b.HasIndex("CourierLogin");

                    b.HasIndex("StatusId");

                    b.HasIndex("StoreId");

                    b.HasIndex("UserLogin");

                    b.ToTable("Order");
                });

            modelBuilder.Entity("Delivery.Models.OrderProduct", b =>
                {
                    b.Property<long>("OrderId")
                        .HasColumnType("bigint");

                    b.Property<long>("ProductId")
                        .HasColumnType("bigint");

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.HasKey("OrderId", "ProductId")
                        .HasName("orderproducts_pk");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderProducts");
                });

            modelBuilder.Entity("Delivery.Models.OrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "orderstatuses_name_uindex")
                        .IsUnique();

                    b.ToTable("OrderStatuses");
                });

            modelBuilder.Entity("Delivery.Models.Product", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<decimal>("Price")
                        .HasColumnType("money");

                    b.Property<long>("StoreId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int?>("Weight")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("Delivery.Models.Rating", b =>
                {
                    b.Property<short>("Rating1")
                        .HasColumnType("smallint")
                        .HasColumnName("Rating");

                    b.Property<long>("StoreId")
                        .HasColumnType("bigint");

                    b.Property<string>("UserLogin")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.HasIndex("StoreId");

                    b.HasIndex("UserLogin");

                    b.ToTable("Ratings");
                });

            modelBuilder.Entity("Delivery.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Id" }, "roles_id_uindex")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Delivery.Models.Store", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityByDefaultColumn();

                    b.Property<long?>("AddressId")
                        .HasColumnType("bigint");

                    b.Property<TimeSpan?>("BeginWorking")
                        .HasColumnType("time without time zone");

                    b.Property<TimeSpan?>("EndWorking")
                        .HasColumnType("time without time zone");

                    b.Property<string>("OwnerLogin")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<int>("StoreStatusId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("OwnerLogin");

                    b.HasIndex("StoreStatusId");

                    b.ToTable("Stores");
                });

            modelBuilder.Entity("Delivery.Models.StoreCategory", b =>
                {
                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<long>("StoreId")
                        .HasColumnType("bigint");

                    b.HasIndex("CategoryId");

                    b.HasIndex("StoreId");

                    b.ToTable("StoreCategories");
                });

            modelBuilder.Entity("Delivery.Models.StoreStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("StoreStatuses");
                });

            modelBuilder.Entity("Delivery.Models.User", b =>
                {
                    b.Property<string>("Login")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Phone")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<Guid>("Salt")
                        .HasColumnType("uuid");

                    b.HasKey("Login")
                        .HasName("users_pk");

                    b.HasIndex("RoleId");

                    b.ToTable("Users");

                    b
                        .HasComment("Пользователи системы");
                });

            modelBuilder.Entity("Delivery.Models.UserAddress", b =>
                {
                    b.Property<string>("UserLogin")
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<long>("AddressId")
                        .HasColumnType("bigint");

                    b.HasKey("UserLogin", "AddressId")
                        .HasName("useraddress_pk");

                    b.HasIndex("AddressId");

                    b.ToTable("UserAddress");
                });

            modelBuilder.Entity("Delivery.Models.WorkCourierStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("WorkCourierStatuses");
                });

            modelBuilder.Entity("Delivery.Models.Address", b =>
                {
                    b.HasOne("Delivery.Models.Locality", "Locality")
                        .WithMany("Addresses")
                        .HasForeignKey("LocalityId")
                        .HasConstraintName("addresses_locality_id_fk")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Locality");
                });

            modelBuilder.Entity("Delivery.Models.Cart", b =>
                {
                    b.HasOne("Delivery.Models.Product", "Product")
                        .WithMany("Carts")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("cart_product_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Delivery.Models.User", "UserLoginNavigation")
                        .WithMany("Carts")
                        .HasForeignKey("UserLogin")
                        .HasConstraintName("cart_users_login_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("UserLoginNavigation");
                });

            modelBuilder.Entity("Delivery.Models.Courier", b =>
                {
                    b.HasOne("Delivery.Models.User", "UserLoginNavigation")
                        .WithOne("Courier")
                        .HasForeignKey("Delivery.Models.Courier", "UserLogin")
                        .HasConstraintName("couriers_users_login_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Delivery.Models.WorkCourierStatus", "WorkStatus")
                        .WithMany("Couriers")
                        .HasForeignKey("WorkStatusId")
                        .HasConstraintName("couriers_workcourierstatuses_id_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("UserLoginNavigation");

                    b.Navigation("WorkStatus");
                });

            modelBuilder.Entity("Delivery.Models.Order", b =>
                {
                    b.HasOne("Delivery.Models.Courier", "CourierLoginNavigation")
                        .WithMany("Orders")
                        .HasForeignKey("CourierLogin")
                        .HasConstraintName("order_couriers_userlogin_fk")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Delivery.Models.OrderStatus", "Status")
                        .WithMany("Orders")
                        .HasForeignKey("StatusId")
                        .HasConstraintName("order_orderstatuses_id_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.HasOne("Delivery.Models.Store", "Store")
                        .WithMany("Orders")
                        .HasForeignKey("StoreId")
                        .HasConstraintName("order_stores_id_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.HasOne("Delivery.Models.User", "UserLoginNavigation")
                        .WithMany("Orders")
                        .HasForeignKey("UserLogin")
                        .HasConstraintName("order_users_login_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("CourierLoginNavigation");

                    b.Navigation("Status");

                    b.Navigation("Store");

                    b.Navigation("UserLoginNavigation");
                });

            modelBuilder.Entity("Delivery.Models.OrderProduct", b =>
                {
                    b.HasOne("Delivery.Models.Order", "Order")
                        .WithMany("OrderProducts")
                        .HasForeignKey("OrderId")
                        .HasConstraintName("orderproducts_order_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Delivery.Models.Product", "Product")
                        .WithMany("OrderProducts")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("orderproducts_product_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Delivery.Models.Product", b =>
                {
                    b.HasOne("Delivery.Models.Store", "Store")
                        .WithMany("Products")
                        .HasForeignKey("StoreId")
                        .HasConstraintName("product_stores_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Store");
                });

            modelBuilder.Entity("Delivery.Models.Rating", b =>
                {
                    b.HasOne("Delivery.Models.Store", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId")
                        .HasConstraintName("ratings_stores_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Delivery.Models.User", "UserLoginNavigation")
                        .WithMany()
                        .HasForeignKey("UserLogin")
                        .HasConstraintName("ratings_users_login_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Store");

                    b.Navigation("UserLoginNavigation");
                });

            modelBuilder.Entity("Delivery.Models.Store", b =>
                {
                    b.HasOne("Delivery.Models.Address", "Address")
                        .WithMany("Stores")
                        .HasForeignKey("AddressId")
                        .HasConstraintName("stores_addresses_id_fk");

                    b.HasOne("Delivery.Models.User", "OwnerLoginNavigation")
                        .WithMany("Stores")
                        .HasForeignKey("OwnerLogin")
                        .HasConstraintName("stores_users_login_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.HasOne("Delivery.Models.StoreStatus", "StoreStatus")
                        .WithMany("Stores")
                        .HasForeignKey("StoreStatusId")
                        .HasConstraintName("stores_storestatuses_id_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Address");

                    b.Navigation("OwnerLoginNavigation");

                    b.Navigation("StoreStatus");
                });

            modelBuilder.Entity("Delivery.Models.StoreCategory", b =>
                {
                    b.HasOne("Delivery.Models.CategoryStore", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("storecategories_categorystore_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Delivery.Models.Store", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId")
                        .HasConstraintName("storecategories_stores_id_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("Delivery.Models.User", b =>
                {
                    b.HasOne("Delivery.Models.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .HasConstraintName("users_roles_id_fk")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Delivery.Models.UserAddress", b =>
                {
                    b.HasOne("Delivery.Models.Address", "Address")
                        .WithMany("UserAddresses")
                        .HasForeignKey("AddressId")
                        .HasConstraintName("useraddress_addresses_id_fk")
                        .IsRequired();

                    b.HasOne("Delivery.Models.User", "UserLoginNavigation")
                        .WithMany("UserAddresses")
                        .HasForeignKey("UserLogin")
                        .HasConstraintName("useraddress_users_login_fk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");

                    b.Navigation("UserLoginNavigation");
                });

            modelBuilder.Entity("Delivery.Models.Address", b =>
                {
                    b.Navigation("Stores");

                    b.Navigation("UserAddresses");
                });

            modelBuilder.Entity("Delivery.Models.Courier", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("Delivery.Models.Locality", b =>
                {
                    b.Navigation("Addresses");
                });

            modelBuilder.Entity("Delivery.Models.Order", b =>
                {
                    b.Navigation("OrderProducts");
                });

            modelBuilder.Entity("Delivery.Models.OrderStatus", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("Delivery.Models.Product", b =>
                {
                    b.Navigation("Carts");

                    b.Navigation("OrderProducts");
                });

            modelBuilder.Entity("Delivery.Models.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Delivery.Models.Store", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("Delivery.Models.StoreStatus", b =>
                {
                    b.Navigation("Stores");
                });

            modelBuilder.Entity("Delivery.Models.User", b =>
                {
                    b.Navigation("Carts");

                    b.Navigation("Courier");

                    b.Navigation("Orders");

                    b.Navigation("Stores");

                    b.Navigation("UserAddresses");
                });

            modelBuilder.Entity("Delivery.Models.WorkCourierStatus", b =>
                {
                    b.Navigation("Couriers");
                });
#pragma warning restore 612, 618
        }
    }
}
