using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Delivery.Models
{
    public partial class deliveryContext : DbContext
    {
        public deliveryContext()
        {
        }

        public deliveryContext(DbContextOptions<deliveryContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CategoryStore> CategoryStores { get; set; }
        public virtual DbSet<Courier> Couriers { get; set; }
        public virtual DbSet<Locality> Localities { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<StoreCategory> StoreCategories { get; set; }
        public virtual DbSet<StoreStatus> StoreStatuses { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAddress> UserAddresses { get; set; }
        public virtual DbSet<WorkCourierStatus> WorkCourierStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=delivery;Username=peter;Password=peter");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.UTF-8");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasIndex(e => e.LocalityId, "IX_Addresses_LocalityId");

                entity.Property(e => e.Apartment).HasMaxLength(10);

                entity.Property(e => e.Building)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Entrance).HasMaxLength(10);

                entity.Property(e => e.Level).HasMaxLength(4);

                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Locality)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.LocalityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("addresses_locality_id_fk");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => new { e.UserLogin, e.ProductId })
                    .HasName("cart_pk");

                entity.ToTable("Cart");

                entity.HasIndex(e => e.ProductId, "IX_Cart_ProductId");

                entity.Property(e => e.UserLogin).HasMaxLength(25);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Carts)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("cart_product_id_fk");

                entity.HasOne(d => d.UserLoginNavigation)
                    .WithMany(p => p.Carts)
                    .HasForeignKey(d => d.UserLogin)
                    .HasConstraintName("cart_users_login_fk");
            });

            modelBuilder.Entity<CategoryStore>(entity =>
            {
                entity.ToTable("CategoryStore");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Courier>(entity =>
            {
                entity.HasKey(e => e.UserLogin)
                    .HasName("couriers_pk");

                entity.HasIndex(e => e.WorkStatusId, "IX_Couriers_WorkStatusId");

                entity.HasIndex(e => new { e.Citizenship, e.PassportNumber }, "couriers_citizenshipid_passportnumber_uindex")
                    .IsUnique();

                entity.Property(e => e.UserLogin).HasMaxLength(25);

                entity.Property(e => e.Birth).HasColumnType("date");

                entity.Property(e => e.Citizenship)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("''::character varying");

                entity.Property(e => e.DateWorkBegin).HasColumnType("date");

                entity.Property(e => e.PassportNumber)
                    .IsRequired()
                    .HasMaxLength(35);

                entity.Property(e => e.Payroll).HasColumnType("money");

                entity.HasOne(d => d.UserLoginNavigation)
                    .WithOne(p => p.Courier)
                    .HasForeignKey<Courier>(d => d.UserLogin)
                    .HasConstraintName("couriers_users_login_fk");

                entity.HasOne(d => d.WorkStatus)
                    .WithMany(p => p.Couriers)
                    .HasForeignKey(d => d.WorkStatusId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("couriers_workcourierstatuses_id_fk");
            });

            modelBuilder.Entity<Locality>(entity =>
            {
                entity.ToTable("Locality");

                entity.HasComment("Населенные пункты");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.HasIndex(e => e.CourierLogin, "IX_Order_CourierLogin");

                entity.HasIndex(e => e.StatusId, "IX_Order_StatusId");

                entity.HasIndex(e => e.StoreId, "IX_Order_StoreId");

                entity.HasIndex(e => e.UserLogin, "IX_Order_UserLogin");

                entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 2147483647L, null, null);

                entity.Property(e => e.CourierLogin).HasMaxLength(25);

                entity.Property(e => e.UserLogin)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.HasOne(d => d.CourierLoginNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CourierLogin)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("order_couriers_userlogin_fk");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("order_orderstatuses_id_fk");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("order_stores_id_fk");

                entity.HasOne(d => d.UserLoginNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserLogin)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("order_users_login_fk");
            });

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId })
                    .HasName("orderproducts_pk");

                entity.HasIndex(e => e.ProductId, "IX_OrderProducts_ProductId");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderProducts)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("orderproducts_order_id_fk");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderProducts)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("orderproducts_product_id_fk");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.HasIndex(e => e.Name, "orderstatuses_name_uindex")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.HasIndex(e => e.StoreId, "IX_Product_StoreId");

                entity.Property(e => e.Price).HasColumnType("money");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("product_stores_id_fk");
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.StoreId, "IX_Ratings_StoreId");

                entity.HasIndex(e => e.UserLogin, "IX_Ratings_UserLogin");

                entity.Property(e => e.Rating1).HasColumnName("Rating");

                entity.Property(e => e.UserLogin)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.HasOne(d => d.Store)
                    .WithMany()
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("ratings_stores_id_fk");

                entity.HasOne(d => d.UserLoginNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.UserLogin)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("ratings_users_login_fk");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.Id, "roles_id_uindex")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasIndex(e => e.AddressId, "IX_Stores_AddressId");

                entity.HasIndex(e => e.OwnerLogin, "IX_Stores_OwnerLogin");

                entity.HasIndex(e => e.StoreStatusId, "IX_Stores_StoreStatusId");

                entity.Property(e => e.BeginWorking).HasColumnType("time without time zone");

                entity.Property(e => e.EndWorking).HasColumnType("time without time zone");

                entity.Property(e => e.OwnerLogin)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("stores_addresses_id_fk");

                entity.HasOne(d => d.OwnerLoginNavigation)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.OwnerLogin)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("stores_users_login_fk");

                entity.HasOne(d => d.StoreStatus)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.StoreStatusId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("stores_storestatuses_id_fk");
            });

            modelBuilder.Entity<StoreCategory>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.CategoryId, "IX_StoreCategories_CategoryId");

                entity.HasIndex(e => e.StoreId, "IX_StoreCategories_StoreId");

                entity.HasOne(d => d.Category)
                    .WithMany()
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("storecategories_categorystore_id_fk");

                entity.HasOne(d => d.Store)
                    .WithMany()
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("storecategories_stores_id_fk");
            });

            modelBuilder.Entity<StoreStatus>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Login)
                    .HasName("users_pk");

                entity.HasComment("Пользователи системы");

                entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");

                entity.Property(e => e.Login).HasMaxLength(25);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.MiddleName).HasMaxLength(50);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Phone).HasMaxLength(25);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("users_roles_id_fk");
            });

            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.HasKey(e => new { e.UserLogin, e.AddressId })
                    .HasName("useraddress_pk");

                entity.ToTable("UserAddress");

                entity.HasIndex(e => e.AddressId, "IX_UserAddress_AddressId");

                entity.Property(e => e.UserLogin).HasMaxLength(25);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.UserAddresses)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("useraddress_addresses_id_fk");

                entity.HasOne(d => d.UserLoginNavigation)
                    .WithMany(p => p.UserAddresses)
                    .HasForeignKey(d => d.UserLogin)
                    .HasConstraintName("useraddress_users_login_fk");
            });

            modelBuilder.Entity<WorkCourierStatus>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
