namespace DataStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FirstMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PolicyServicesDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        email = c.String(),
                        phonenumber = c.String(),
                        customerid = c.String(),
                        createdat = c.DateTime(nullable: false),
                        updatedat = c.DateTime(nullable: false),
                        os = c.String(),
                        devicename = c.String(),
                        deviceimei = c.String(),
                        pin = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PolicyServicesDetails");
        }
    }
}
