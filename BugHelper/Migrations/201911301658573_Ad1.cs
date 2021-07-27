namespace BugHelper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ad1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.IletisimModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdSoyad = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Mesaj = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.IletisimModels");
        }
    }
}
