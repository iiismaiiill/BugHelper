namespace BugHelper.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ad2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UlkelerModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ulkeler = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UlkelerModels");
        }
    }
}
