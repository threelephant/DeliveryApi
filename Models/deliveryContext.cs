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
        public virtual DbSet<Building> Buildings { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CategoryStore> CategoryStores { get; set; }
        public virtual DbSet<Courier> Couriers { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Locality> Localities { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<StoreCategory> StoreCategories { get; set; }
        public virtual DbSet<StoreStatus> StoreStatuses { get; set; }
        public virtual DbSet<Street> Streets { get; set; }
        public virtual DbSet<StreetType> StreetTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAddress> UserAddresses { get; set; }
        public virtual DbSet<WorkCourierStatus> WorkCourierStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=delivery;Username=peter;Password=Aqswde10");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.UTF-8");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(e => e.ApartmentNumber).HasMaxLength(10);

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("addresses_buildings_id_fk");
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasComment("Здания");

                entity.Property(e => e.BuildingNumber)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasOne(d => d.Street)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.StreetId)
                    .HasConstraintName("address_street_id_fk");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => new { e.UserLogin, e.ProductId })
                    .HasName("cart_pk");

                entity.ToTable("Cart");

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

                entity.HasIndex(e => new { e.CitizenshipId, e.PassportNumber }, "couriers_citizenshipid_passportnumber_uindex")
                    .IsUnique();

                entity.Property(e => e.UserLogin).HasMaxLength(25);

                entity.Property(e => e.DateWorkBegin).HasColumnType("date");

                entity.Property(e => e.PassportNumber)
                    .IsRequired()
                    .HasMaxLength(35);

                entity.Property(e => e.Payroll).HasColumnType("money");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Couriers)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("couriers_currency_id_fk");

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

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("Currency");

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
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

                entity.Property(e => e.Price).HasColumnType("money");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("product_currency_id_fk");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("product_stores_id_fk");
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasNoKey();

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

            modelBuilder.Entity<Store>(entity =>
            {
                entity.Property(e => e.BeginWorking).HasColumnType("time without time zone");

                entity.Property(e => e.EndWorking).HasColumnType("time without time zone");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("stores_addresses_id_fk");

                entity.HasOne(d => d.StoreStatus)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.StoreStatusId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("stores_storestatuses_id_fk");
            });

            modelBuilder.Entity<StoreCategory>(entity =>
            {
                entity.HasNoKey();

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

            modelBuilder.Entity<Street>(entity =>
            {
                entity.ToTable("Street");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Locality)
                    .WithMany(p => p.Streets)
                    .HasForeignKey(d => d.LocalityId)
                    .HasConstraintName("street_locality_id_fk");

                entity.HasOne(d => d.StreetType)
                    .WithMany(p => p.Streets)
                    .HasForeignKey(d => d.StreetTypeId)
                    .HasConstraintName("street_streettype_id_fk");
            });

            modelBuilder.Entity<StreetType>(entity =>
            {
                entity.ToTable("StreetType");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Login)
                    .HasName("users_pk");

                entity.HasComment("Пользователи системы");

                entity.Property(e => e.Login).HasMaxLength(25);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.MiddleName).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(25);
            });

            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.HasKey(e => new { e.UserLogin, e.AddressId })
                    .HasName("useraddress_pk");

                entity.ToTable("UserAddress");

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
