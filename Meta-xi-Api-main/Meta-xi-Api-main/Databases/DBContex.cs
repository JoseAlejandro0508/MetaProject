
using Microsoft.EntityFrameworkCore;

namespace Meta_xi.Application;
public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options){
        
    }
    public required DbSet<User> Users { get ; set ; }
    public required DbSet<Plan> Plans { get ; set ; }
    public required DbSet<ReferLevel1> ReferLevel1s { get ; set ; }
    public required DbSet<ReferLevel2> ReferLevel2s { get ; set ; }
    public required DbSet<ReferLevel3> ReferLevel3s { get ; set ; }
    public required DbSet<Wallet> Wallets { get ; set ; }
    public required DbSet<UserPlans> UserPlans { get ; set ; }
    public required DbSet<UpdatePlansForUser> UpdatePlansForUser { get ; set ; }
    public required DbSet<TaskRegister> TaskRegisters { get ; set ; }
    public required DbSet<DepositHistory> DepositHistories { get ; set ; }
    public required DbSet<WithdrawalHistory> WithdrawalHistories { get ; set ; }
    public required DbSet<Mission> Missions { get ; set ; }
    public required DbSet<UserMission> UserMissions { get ; set ; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReferLevel1>().HasOne(option => option.User).WithMany(option => option.referLevel1s).HasForeignKey(option=> option.IDUserReferrer);
        modelBuilder.Entity<ReferLevel2>().HasOne(option => option.User).WithMany(option => option.referLevel2s).HasForeignKey(option=> option.IDUserReferrer);
        modelBuilder.Entity<ReferLevel3>().HasOne(option => option.User).WithMany(option => option.referLevel3s).HasForeignKey(option=> option.IDUserReferrer);

        modelBuilder.Entity<UserMission>()
            .HasIndex(um => new { um.Email, um.MissionId }).IsUnique();

        modelBuilder.Entity<UserMission>()
            .HasOne(um => um.Mission)
            .WithMany()
            .HasForeignKey(um => um.MissionId);

        modelBuilder.Entity<Plan>().HasData(
            new Plan { IDPlan = 1, Name = "Trump Turnberry", Price = 50000, MaxQuantity = 100, DaysActive = 30, DailyBenefit = 4000, TotalBenefit = 120000, Description = "Un resort de golf icónico y legendario en la costa escocesa. Turismo de lujo internacional.", DailyProfitPercentage = 8, ImageUrl = "/plans/trump-turnberry.webp" },
            new Plan { IDPlan = 2, Name = "Trump Tower", Price = 100000, MaxQuantity = 100, DaysActive = 30, DailyBenefit = 8000, TotalBenefit = 240000, Description = "El rascacielos insignia en la Quinta Avenida. Alquiler de espacios comerciales y rentas de oficinas de lujo.", DailyProfitPercentage = 8, ImageUrl = "/plans/trump-tower.webp" },
            new Plan { IDPlan = 3, Name = "Mar-a-Lago", Price = 160000, MaxQuantity = 100, DaysActive = 45, DailyBenefit = 12800, TotalBenefit = 576000, Description = "Un club privado histórico y la residencia principal de Trump.", DailyProfitPercentage = 8, ImageUrl = "/plans/mar-a-lago.webp" },
            new Plan { IDPlan = 4, Name = "40 Wall Street", Price = 250000, MaxQuantity = 100, DaysActive = 50, DailyBenefit = 15000, TotalBenefit = 750000, Description = "Un edificio de oficinas de 71 pisos en el distrito financiero. Arrendamiento comercial.", DailyProfitPercentage = 6, ImageUrl = "/plans/40-wall-street.webp" },
            new Plan { IDPlan = 5, Name = "Trump Casino", Price = 350000, MaxQuantity = 100, DaysActive = 50, DailyBenefit = 21000, TotalBenefit = 1050000, Description = "Un casino de juegos tragamonedas, poker y mucho más.", DailyProfitPercentage = 6, ImageUrl = "/plans/trump-casino.webp" },
            new Plan { IDPlan = 6, Name = "555 California Street", Price = 500000, MaxQuantity = 100, DaysActive = 50, DailyBenefit = 40000, TotalBenefit = 2000000, Description = "Uno de los edificios más altos y valiosos de la costa oeste. Alquiler de oficinas.", DailyProfitPercentage = 8, ImageUrl = "/plans/555-california.webp" },
            new Plan { IDPlan = 7, Name = "Trump International Hotel & Tower Chicago", Price = 800000, MaxQuantity = 100, DaysActive = 50, DailyBenefit = 64000, TotalBenefit = 3200000, Description = "Un rascacielos de uso mixto junto al río Chicago. Hotel y condominios.", DailyProfitPercentage = 8, ImageUrl = "/plans/trump-chicago.webp" },
            new Plan { IDPlan = 8, Name = "Trump International Hotel Las Vegas", Price = 1200000, MaxQuantity = 100, DaysActive = 50, DailyBenefit = 96000, TotalBenefit = 4800000, Description = "Una torre dorada hotel y condo-hotel. Operación hotelera y comisiones.", DailyProfitPercentage = 8, ImageUrl = "/plans/trump-vegas.webp" }
        );

        modelBuilder.Entity<Mission>().HasData(
            new Mission { Id = 1, Title = "Invita 5 nuevos amigos", Type = MissionType.Normal, Ref = 5, Gift = 1000, ImageUrl = "/missions/invite-5.webp" },
            new Mission { Id = 2, Title = "Invita 15 nuevos amigos", Type = MissionType.Normal, Ref = 15, Gift = 3000, ImageUrl = "/missions/invite-15.webp" },
            new Mission { Id = 3, Title = "Invita 20 nuevos amigos", Type = MissionType.Normal, Ref = 20, Gift = 4000, ImageUrl = "/missions/invite-20.webp" },
            new Mission { Id = 4, Title = "Invita 30 nuevos amigos", Type = MissionType.Normal, Ref = 30, Gift = 6000, ImageUrl = "/missions/invite-30.webp" },
            new Mission { Id = 5, Title = "Invita 40 nuevos amigos", Type = MissionType.Normal, Ref = 40, Gift = 8000, ImageUrl = "/missions/invite-40.webp" },
            new Mission { Id = 6, Title = "Invita 50 nuevos amigos", Type = MissionType.Normal, Ref = 50, Gift = 10000, ImageUrl = "/missions/invite-50.webp" },
            new Mission { Id = 7, Title = "Invita 10 nuevos amigos", Type = MissionType.Premium, Ref = 10, Gift = 2000, ImageUrl = "/missions/invite-10p.webp" },
            new Mission { Id = 8, Title = "Invita 100 nuevos amigos", Type = MissionType.Premium, Ref = 100, Gift = 30000, ImageUrl = "/missions/invite-100.webp" },
            new Mission { Id = 9, Title = "Invita 250 nuevos amigos", Type = MissionType.Premium, Ref = 250, Gift = 40000, ImageUrl = "/missions/invite-250.webp" },
            new Mission { Id = 10, Title = "Invita 350 nuevos amigos", Type = MissionType.Premium, Ref = 350, Gift = 60000, ImageUrl = "/missions/invite-350.webp" },
            new Mission { Id = 11, Title = "Invita 450 nuevos amigos", Type = MissionType.Premium, Ref = 450, Gift = 80000, ImageUrl = "/missions/invite-450.webp" },
            new Mission { Id = 12, Title = "Invita 500 nuevos amigos", Type = MissionType.Premium, Ref = 500, Gift = 100000, ImageUrl = "/missions/invite-500.webp" }
        );
    }

}