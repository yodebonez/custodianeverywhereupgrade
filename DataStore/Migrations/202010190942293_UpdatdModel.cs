namespace DataStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatdModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PolicyServicesDetails", "policynumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PolicyServicesDetails", "policynumber");
        }
    }
}
